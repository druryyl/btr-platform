package com.elsasa.bgud.repository

import com.elsasa.bgud.dao.ReturnOrderDao
import com.elsasa.bgud.dao.ReturnOrderItemDao
import com.elsasa.bgud.model.ReturnOrderEntity
import com.elsasa.bgud.model.ReturnOrderItemEntity
import com.elsasa.bgud.model.api.ReturnOrderItemDto
import com.elsasa.bgud.model.api.ReturnOrderSubmitRequest
import com.elsasa.bgud.network.BtradeApiService
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

/**
 * Return Order synchronization repository (S4.5, Architecture §4.4, §8.1,
 * §20).
 *
 * Runs the mandated sync order (Arch §17.1, §20 "Synchronization"):
 *
 * ```text
 * submit DRAFT orders (I-RO-01, one HTTP call each)
 *     ↓
 * reference downloads (I-RO-03/04/05, S4.3 replace-cache)
 * ```
 *
 * Rules (no new decisions):
 * - Every submit carries the JWT via [BtradeApiService] (S4.2
 *   `AuthInterceptor`); this class performs no authentication itself.
 * - No `ServerId` is stored or sent as a command input (P-06). The only
 *   tenant value the device ever supplies is the reference read-route path
 *   `GET /api/Customer|SalesPerson|Driver/{serverId}` (S4.3), using the
 *   login-returned `serverId` passed per run.
 * - A `DRAFT` order is submitted exactly once per run; a `2xx` response moves
 *   the local order to `SYNCED` (ADR-RO-006). A failed submission keeps the
 *   order `DRAFT` and counts as failed without aborting the run; resubmission
 *   after failure is safe (the Cloud is idempotent on the ULID
 *   `ReturnOrderId`, Arch §10.6).
 * - The device vocabulary is `DRAFT` | `SYNCED` only. No import outcome and no
 *   `ReturnOrderNo` is ever downloaded — there is no status refresh step
 *   (ADR-RO-006).
 * - Reference downloads run after submission (Arch §20 ordering); steps never
 *   abort each other and failures are collected in [SyncRunResult.errors].
 */
class ReturnOrderSyncRepository(
    private val api: BtradeApiService,
    private val returnOrderDao: ReturnOrderDao,
    private val returnOrderItemDao: ReturnOrderItemDao,
    private val referenceSync: ReturnOrderReferenceSyncRepository
) {

    /**
     * Outcome of one ordered sync run. Steps never abort each other;
     * failures are collected in [errors] for the caller retry surface
     * (S4.10).
     */
    data class SyncRunResult(
        val submittedCount: Int = 0,
        val submitFailedCount: Int = 0,
        val customerCount: Int = 0,
        val salesPersonCount: Int = 0,
        val driverCount: Int = 0,
        val errors: List<String> = emptyList()
    )

    /** Outcome of the DRAFT submission step (I-RO-01). */
    data class SubmitOutcome(
        val submittedCount: Int = 0,
        val failedCount: Int = 0,
        val errors: List<String> = emptyList()
    )

    /**
     * Full ordered run: submit DRAFT orders → reference downloads.
     *
     * @param serverId login-returned Office id for the S4.3 reference read
     * routes only; blank fails each reference download (recorded in
     * [SyncRunResult.errors]).
     */
    suspend fun sync(serverId: String): SyncRunResult = withContext(Dispatchers.IO) {
        val errors = mutableListOf<String>()

        val submission = try {
            submitDraftOrders()
        } catch (e: Exception) {
            errors.add("submit: ${e.message}")
            SubmitOutcome()
        }

        val references = try {
            referenceSync.sync(serverId)
        } catch (e: Exception) {
            errors.add("reference: ${e.message}")
            ReturnOrderReferenceSyncRepository.SyncRunResult()
        }

        SyncRunResult(
            submittedCount = submission.submittedCount,
            submitFailedCount = submission.failedCount,
            customerCount = references.customerCount,
            salesPersonCount = references.salesPersonCount,
            driverCount = references.driverCount,
            errors = errors + submission.errors + references.errors
        )
    }

    /**
     * Step 1 — submit every local `DRAFT` order, one HTTP call each (I-RO-01).
     *
     * The ULID `ReturnOrderId` is the idempotency key (IR-RO-01); a `2xx`
     * response moves the order to `SYNCED` (ADR-RO-006) without touching its
     * items. A per-order failure leaves the order `DRAFT` and never aborts
     * the run.
     */
    suspend fun submitDraftOrders(): SubmitOutcome = withContext(Dispatchers.IO) {
        val drafts = returnOrderDao.listByStatus(ReturnOrderEntity.STATUS_DRAFT)
        if (drafts.isEmpty()) return@withContext SubmitOutcome()

        var submitted = 0
        var failed = 0
        val errors = mutableListOf<String>()

        for (order in drafts) {
            try {
                val items = returnOrderItemDao.listByParent(order.returnOrderId)
                api.submitReturnOrder(order.toSubmitRequest(items))
                returnOrderDao.upsert(order.copy(status = ReturnOrderEntity.STATUS_SYNCED))
                submitted++
            } catch (e: Exception) {
                failed++
                errors.add("submit ${order.returnOrderId}: ${e.message}")
            }
        }

        SubmitOutcome(submittedCount = submitted, failedCount = failed, errors = errors)
    }

    companion object {

        /**
         * Local header + items → I-RO-01 payload (`ReturnOrderUploadCommand`).
         *
         * `ReturnOrderDate` is derived from the capture timestamp `createdAt`
         * as a `yyyy-MM-dd` string (the device captures no separate business
         * date, Arch §6.4). No `ServerId` is included (P-06).
         */
        fun ReturnOrderEntity.toSubmitRequest(
            items: List<ReturnOrderItemEntity>
        ): ReturnOrderSubmitRequest = ReturnOrderSubmitRequest(
            returnOrderId = returnOrderId,
            returnOrderDate = formatReturnOrderDate(createdAt),
            warehouseCode = warehouseCode,
            customerId = customerId,
            customerName = customerName,
            salesPersonId = salesPersonId,
            salesPersonName = salesPersonName,
            driverId = driverId,
            driverName = driverName,
            note = note,
            listItem = items.map { it.toDto() }
        )

        /**
         * Local item line → I-RO-01 payload item. `qty` is the physical-unit
         * quantity recorded exactly as captured (P-09, ADR-RO-003).
         */
        fun ReturnOrderItemEntity.toDto(): ReturnOrderItemDto = ReturnOrderItemDto(
            returnOrderId = returnOrderId,
            noUrut = noUrut,
            brgId = brgId,
            brgCode = brgCode,
            brgName = brgName,
            qty = qty,
            satId = satId,
            jenisRetur = jenisRetur
        )

        /** Capture timestamp → `yyyy-MM-dd` (cloud `ReturnOrderType` convention). */
        fun formatReturnOrderDate(timestampMillis: Long): String =
            SimpleDateFormat("yyyy-MM-dd", Locale.US).format(Date(timestampMillis))
    }
}
