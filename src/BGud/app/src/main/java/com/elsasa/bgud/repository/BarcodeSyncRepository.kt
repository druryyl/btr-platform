package com.elsasa.bgud.repository

import com.elsasa.bgud.dao.BarcodeDao
import com.elsasa.bgud.dao.BarangDao
import com.elsasa.bgud.dao.BarcodeRegistrationRequestDao
import com.elsasa.bgud.datastore.SessionBinding
import com.elsasa.bgud.datastore.SessionPreferencesDataSource
import com.elsasa.bgud.model.BarcodeEntity
import com.elsasa.bgud.model.BarangEntity
import com.elsasa.bgud.model.BarcodeRegistrationRequestEntity
import com.elsasa.bgud.model.api.BarcodeDto
import com.elsasa.bgud.model.api.BarcodeRegistrationSubmitRequest
import com.elsasa.bgud.model.api.BrgDto
import com.elsasa.bgud.network.BtradeApiService
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.withContext

/**
 * Barcode Registry synchronization repository (S5.3).
 *
 * Runs the mandated sync order (§15.2, §17.2 "Sync Now"):
 *
 * ```text
 * submit pending requests (I-04, one HTTP call each)
 *     ↓
 * download barcodes (I-05, replace local Active cache)
 *     ↓
 * download Barang (I-06, replace local reference cache)
 *     ↓
 * refresh own request statuses (I-09, PENDING → SYNCED | REJECTED)
 * ```
 *
 * Rules (no new decisions):
 * - Every request carries the session context via [BtradeApiService] (TD-12
 *   `SessionContextInterceptor`); this class performs no authentication itself
 *   (a valid local session is enforced by the caller).
 * - No `ServerId` is stored or sent as a command input on the session-context
 *   routes (IR-09, P-06). The only tenant value the device ever supplies is
 *   the legacy read-route path `GET /api/Brg/{serverId}` (TD-08), using the
 *   session-resolved `serverId` passed per run for that route only. It is
 *   never persisted here.
 * - Submission is filtered by [SessionBinding.canSubmit] (IR-M7, IR-09):
 *   queued requests captured under a different warehouse are held, never
 *   re-homed, never submitted.
 * - Submission is idempotent by `ClientRequestId` (INV-12): one HTTP call
 *   per queued request; per-item failures never abort the run. A submitted
 *   row stays `PENDING` locally; only I-09 transitions it (`SYNCED` on
 *   Cloud `ACCEPTED`, `REJECTED` with `serverNote = processedNote`).
 * - Downloads replace the local caches (full Active set, §20); timestamps
 *   advance only for the download that committed.
 */
class BarcodeSyncRepository(
    private val api: BtradeApiService,
    private val barcodeDao: BarcodeDao,
    private val barangDao: BarangDao,
    private val requestDao: BarcodeRegistrationRequestDao,
    private val session: SessionPreferencesDataSource
) {

    /**
     * Outcome of one ordered sync run. Steps never abort each other;
     * failures are collected in [errors] for the Sync screen (S5.10)
     * retry surface (UX §14 "Gagal sinkronisasi." → Retry).
     */
    data class SyncRunResult(
        val submittedCount: Int = 0,
        val submitSkippedCount: Int = 0,
        val submitFailedCount: Int = 0,
        val barcodeCount: Int = 0,
        val barangCount: Int = 0,
        val syncedCount: Int = 0,
        val rejectedCount: Int = 0,
        val errors: List<String> = emptyList()
    )

    /**
     * Full ordered run: submit → barcodes → barang → statuses.
     *
     * @param serverId session-resolved Office id for the legacy I-06 read
     * route only; blank skips the Barang download (recorded in [errors]).
     */
    suspend fun sync(serverId: String): SyncRunResult = withContext(Dispatchers.IO) {
        val errors = mutableListOf<String>()

        val submitted = try {
            submitPendingRequests()
        } catch (e: Exception) {
            errors.add("submit: ${e.message}")
            SubmitOutcome()
        }

        val barcodes = try {
            downloadBarcodes()
        } catch (e: Exception) {
            errors.add("barcodes: ${e.message}")
            0
        }

        val barangs = try {
            downloadBarang(serverId)
        } catch (e: Exception) {
            errors.add("barang: ${e.message}")
            0
        }

        val outcomes = try {
            refreshRequestStatus()
        } catch (e: Exception) {
            errors.add("status: ${e.message}")
            0 to 0
        }

        SyncRunResult(
            submittedCount = submitted.submittedCount,
            submitSkippedCount = submitted.submitSkippedCount,
            submitFailedCount = submitted.submitFailedCount,
            barcodeCount = barcodes,
            barangCount = barangs,
            syncedCount = outcomes.first,
            rejectedCount = outcomes.second,
            errors = errors + submitted.errors
        )
    }

    data class SubmitOutcome(
        val submittedCount: Int = 0,
        val submitSkippedCount: Int = 0,
        val submitFailedCount: Int = 0,
        val errors: List<String> = emptyList()
    )

    /**
     * Step 1 — submit queued `PENDING` requests, one HTTP call each (I-04).
     *
     * Only requests bound to the current warehouse are submitted
     * ([SessionBinding.canSubmit]); others are counted as skipped and held
     * (IR-M7). Local rows stay `PENDING`; outcomes arrive via I-09.
     */
    suspend fun submitPendingRequests(): SubmitOutcome = withContext(Dispatchers.IO) {
        val pending = requestDao
            .listByStatus(BarcodeRegistrationRequestEntity.STATUS_PENDING)
            .first()
        if (pending.isEmpty()) return@withContext SubmitOutcome()

        val locationId = session.locationId.first().orEmpty()
        val userId = session.userId.first().orEmpty()
        val binding = SessionBinding(
            userId = userId,
            locationId = locationId
        )

        var submitted = 0
        var skipped = 0
        var failed = 0
        val errors = mutableListOf<String>()

        for (request in pending) {
            if (!binding.canSubmit(request.warehouseCode)) {
                skipped++
                continue
            }
            try {
                api.submitRegistration(
                    BarcodeRegistrationSubmitRequest(
                        clientRequestId = request.clientRequestId,
                        barcodeValue = request.barcodeValue,
                        brgId = request.brgId,
                        satuan = request.satuan
                    )
                )
                submitted++
            } catch (e: Exception) {
                failed++
                errors.add("submit ${request.clientRequestId}: ${e.message}")
            }
        }

        SubmitOutcome(
            submittedCount = submitted,
            submitSkippedCount = skipped,
            submitFailedCount = failed,
            errors = errors
        )
    }

    /**
     * Step 2 — bulk Active barcode download (I-05); replaces the local
     * Active cache (TQ-3, TQ-6). The timestamp advances only on commit.
     */
    suspend fun downloadBarcodes(): Int = withContext(Dispatchers.IO) {
        val envelope = api.barcodeSync()
        val dtos = envelope.data ?: emptyList()
        val entities = dtos.map { it.toEntity() }
        barcodeDao.deleteAll()
        if (entities.isNotEmpty()) {
            barcodeDao.upsertAll(entities)
        }
        session.setLastBarcodeSync(System.currentTimeMillis())
        entities.size
    }

    /**
     * Step 3 — Barang reference download (I-06); replaces the local
     * reference cache (ADR-005). [serverId] is the session-resolved Office id,
     * used for this legacy read route only (§8.4, ADR-007 §8).
     */
    suspend fun downloadBarang(serverId: String): Int = withContext(Dispatchers.IO) {
        if (serverId.isBlank()) {
            throw IllegalArgumentException("serverId is required for Barang download (I-06)")
        }
        val envelope = api.brgList(serverId)
        val dtos = envelope.data ?: emptyList()
        val entities = dtos.map { it.toEntity() }
        barangDao.deleteAll()
        if (entities.isNotEmpty()) {
            barangDao.upsertAll(entities)
        }
        session.setLastBarangSync(System.currentTimeMillis())
        entities.size
    }

    /**
     * Step 4 — refresh own request outcomes (I-09).
     *
     * Cloud `ACCEPTED` → local `SYNCED`; Cloud `REJECTED` → local
     * `REJECTED` with `serverNote = processedNote` (§8.3). Only local
     * `PENDING` rows transition; terminal rows are never rewritten.
     *
     * @return `syncedCount to rejectedCount`.
     */
    suspend fun refreshRequestStatus(): Pair<Int, Int> = withContext(Dispatchers.IO) {
        val envelope = api.registrationStatus()
        val statuses = envelope.data ?: emptyList()

        var synced = 0
        var rejected = 0

        for (dto in statuses) {
            if (dto.clientRequestId.isBlank()) continue
            val local = requestDao.getById(dto.clientRequestId) ?: continue
            if (local.status != BarcodeRegistrationRequestEntity.STATUS_PENDING) continue
            when (dto.status.uppercase()) {
                "ACCEPTED" -> {
                    requestDao.updateOutcome(
                        dto.clientRequestId,
                        BarcodeRegistrationRequestEntity.STATUS_SYNCED,
                        dto.processedNote
                    )
                    synced++
                }
                "REJECTED" -> {
                    requestDao.updateOutcome(
                        dto.clientRequestId,
                        BarcodeRegistrationRequestEntity.STATUS_REJECTED,
                        dto.processedNote
                    )
                    rejected++
                }
                else -> Unit
            }
        }

        synced to rejected
    }

    companion object {
        /** I-05 item → Active cache row; key derived by VO-01 (§5.1). */
        fun BarcodeDto.toEntity(): BarcodeEntity = BarcodeEntity(
            brgBarcodeId = brgBarcodeId,
            barcodeValue = barcodeValue,
            barcodeValueKey = BarcodeEntity.keyOf(barcodeValue),
            brgId = brgId,
            brgCode = brgCode,
            brgName = brgName,
            satuan = satuan
        )

        /**
         * I-06 item → reference row. I-06 carries no `IsAktif` field; the
         * downloaded set is the Active reference set, so every replaced row
         * is `isAktif = true` (BQ-7 cached-Active validation: an Item that
         * disappears from the download is removed from the cache and can no
         * longer be selected).
         */
        fun BrgDto.toEntity(): BarangEntity = BarangEntity(
            brgId = brgId,
            brgCode = brgCode,
            brgName = brgName,
            isAktif = true,
            satKecil = satKecil,
            satBesar = satBesar
        )
    }
}
