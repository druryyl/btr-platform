package com.elsasa.bgud.sync

import android.content.Context
import androidx.work.Constraints
import androidx.work.CoroutineWorker
import androidx.work.Data
import androidx.work.ExistingWorkPolicy
import androidx.work.NetworkType
import androidx.work.OneTimeWorkRequestBuilder
import androidx.work.WorkManager
import androidx.work.WorkerParameters
import androidx.work.workDataOf
import com.elsasa.bgud.database.AppDatabase
import com.elsasa.bgud.datastore.SessionPreferencesDataSource
import com.elsasa.bgud.network.ApiClient
import com.elsasa.bgud.network.SessionContextInterceptor
import com.elsasa.bgud.repository.BarcodeSyncRepository
import kotlinx.coroutines.flow.first
import java.io.IOException
import java.util.UUID
import java.util.concurrent.atomic.AtomicBoolean

/**
 * Barcode Registry sync worker (S5.3).
 *
 * Executes one ordered [BarcodeSyncRepository.sync] run
 * (submit → barcodes → barang → statuses; §15.2, §17.2) under a
 * connectivity constraint. The local session is read once at run start and
 * supplied as a fixed session-context provider, so no DataStore I/O blocks the
 * OkHttp dispatcher. No bearer token is used (TD-09/TD-12).
 *
 * Concurrency (IR-M6) and scheduling (UX §13):
 * - One sync run at a time via unique work ([ExistingWorkPolicy.KEEP]);
 *   a duplicate Sync Now while a run is in progress is ignored.
 * - One-shot work only; no periodic API is exposed (MVP must not schedule
 *   automatic background synchronization, §20, §23.4).
 */
class BarcodeSyncWorker(
    appContext: Context,
    params: WorkerParameters
) : CoroutineWorker(appContext, params) {

    override suspend fun doWork(): Result {
        val baseUrl = inputData.getString(KEY_BASE_URL).orEmpty()
        if (baseUrl.isBlank()) {
            return Result.failure(
                workDataOf(OUTPUT_ERRORS to "baseUrl is required (transport unresolved, C-3)")
            )
        }

        val session = SessionPreferencesDataSource(applicationContext)
        val actorEmail = session.googleEmail.first().orEmpty()
        if (actorEmail.isBlank()) {
            // TD-10/TD-11: no valid local session → operational sync is
            // blocked; the user must sign in again. Retrying without a session
            // cannot succeed.
            return Result.failure(
                workDataOf(OUTPUT_ERRORS to "no session; login required")
            )
        }
        val locationId = session.locationId.first().orEmpty()
        val sessionEnded = AtomicBoolean(false)

        return try {
            val database = AppDatabase.getDatabase(applicationContext)
            val api = ApiClient.create(
                baseUrl = normalizedBaseUrl(baseUrl),
                sessionContextProvider = {
                    SessionContextInterceptor.SessionContext(
                        locationId = locationId,
                        actorEmail = actorEmail
                    )
                },
                onSessionInvalidated = { sessionEnded.set(true) }
            )
            val repository = BarcodeSyncRepository(
                api = api,
                barcodeDao = database.barcodeDao(),
                barangDao = database.barangDao(),
                requestDao = database.barcodeRegistrationRequestDao(),
                session = session
            )

            val serverId = inputData.getString(KEY_SERVER_ID).orEmpty()
            val run = repository.sync(serverId)

            if (sessionEnded.get()) {
                // TD-13 — the actor mapping was removed; clear the session and
                // let the Navigation gate return the operator to sign-in.
                session.clearSession()
                return Result.failure(
                    workDataOf(OUTPUT_ERRORS to "session ended; login required")
                )
            }

            Result.success(run.toOutputData())
        } catch (e: IOException) {
            Result.retry()
        } catch (e: Exception) {
            Result.failure(
                workDataOf(OUTPUT_ERRORS to (e.message ?: "sync failed"))
            )
        }
    }

    companion object {
        /** Unique work name enforcing one sync run at a time (IR-M6). */
        const val UNIQUE_WORK_NAME = "barcode-sync"

        /**
         * Base URL for the Cloud API (no URL is hardcoded; transport is
         * unresolved, C-3/R-03). Supplied per enqueue by the caller.
         */
        const val KEY_BASE_URL = "baseUrl"

        /**
         * Session-resolved Office id for the legacy I-06 read route only
         * (TD-08). Supplied per enqueue by the caller; never persisted by the
         * sync layer.
         */
        const val KEY_SERVER_ID = "serverId"

        const val OUTPUT_SUBMITTED = "submitted"
        const val OUTPUT_SUBMIT_SKIPPED = "submitSkipped"
        const val OUTPUT_SUBMIT_FAILED = "submitFailed"
        const val OUTPUT_BARCODE_COUNT = "barcodeCount"
        const val OUTPUT_BARANG_COUNT = "barangCount"
        const val OUTPUT_SYNCED = "synced"
        const val OUTPUT_REJECTED = "rejected"
        const val OUTPUT_ERRORS = "errors"

        /**
         * Enqueue a one-shot sync run. While a run is in progress, a further
         * Sync Now is kept out by [ExistingWorkPolicy.KEEP] (IR-M6).
         */
        fun enqueueUnique(
            context: Context,
            baseUrl: String,
            serverId: String
        ): UUID {
            val request = OneTimeWorkRequestBuilder<BarcodeSyncWorker>()
                .setInputData(
                    workDataOf(
                        KEY_BASE_URL to baseUrl,
                        KEY_SERVER_ID to serverId
                    )
                )
                .setConstraints(
                    Constraints.Builder()
                        .setRequiredNetworkType(NetworkType.CONNECTED)
                        .build()
                )
                .build()
            WorkManager.getInstance(context).enqueueUniqueWork(
                UNIQUE_WORK_NAME,
                ExistingWorkPolicy.KEEP,
                request
            )
            return request.id
        }

        private fun normalizedBaseUrl(baseUrl: String): String =
            if (baseUrl.endsWith("/")) baseUrl else "$baseUrl/"

        private fun BarcodeSyncRepository.SyncRunResult.toOutputData(): Data =
            workDataOf(
                OUTPUT_SUBMITTED to submittedCount,
                OUTPUT_SUBMIT_SKIPPED to submitSkippedCount,
                OUTPUT_SUBMIT_FAILED to submitFailedCount,
                OUTPUT_BARCODE_COUNT to barcodeCount,
                OUTPUT_BARANG_COUNT to barangCount,
                OUTPUT_SYNCED to syncedCount,
                OUTPUT_REJECTED to rejectedCount,
                OUTPUT_ERRORS to errors.joinToString("; ")
            )
    }
}
