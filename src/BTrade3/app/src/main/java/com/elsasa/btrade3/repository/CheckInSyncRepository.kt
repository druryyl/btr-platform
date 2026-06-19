package com.elsasa.btrade3.repository

import android.content.Context
import android.util.Log
import com.elsasa.btrade3.model.CheckIn
import com.elsasa.btrade3.model.api.CheckInRequest
import com.elsasa.btrade3.network.ApiService
import com.elsasa.btrade3.util.ServerHelper
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.flow.firstOrNull
import kotlinx.coroutines.withContext

class CheckInSyncRepository(
    private val apiService: ApiService,
    private val checkInRepository: CheckInRepository,
    private val serverHelper: ServerHelper
) {
    companion object {
        private const val TAG = "CheckInSyncRepository"
    }

    sealed class SyncResult {
        data class Success(val message: String, val count: Int) : SyncResult()
        data class Error(val message: String) : SyncResult()
        data class Progress(val current: Int, val total: Int, val customerName: String) : SyncResult()
        object Loading : SyncResult()
    }

    suspend fun uploadCheckIn(checkInId: String, context: Context): Boolean = withContext(Dispatchers.IO) {
        val checkIn = checkInRepository.getCheckInById(checkInId) ?: return@withContext false
        uploadSingleCheckIn(checkIn, context)
    }

    suspend fun syncDraftCheckIns(userEmail: String, context: Context): SyncResult = withContext(Dispatchers.IO) {
        try {
            Log.d(TAG, "Starting draft check-ins sync...")
            val draftCheckIns = checkInRepository.getDraftCheckIns().firstOrNull() ?: emptyList()
            if (draftCheckIns.isEmpty()) {
                return@withContext SyncResult.Success("No draft check-ins to sync", 0)
            }

            val totalCheckIns = draftCheckIns.size
            var syncedCount = 0

            draftCheckIns.forEach { checkIn ->
                if (uploadSingleCheckIn(checkIn, context)) {
                    syncedCount++
                }
            }

            SyncResult.Success(
                message = "Successfully synced $syncedCount of $totalCheckIns check-ins",
                count = syncedCount
            )
        } catch (e: Exception) {
            Log.e(TAG, "Check-in sync error", e)
            SyncResult.Error("Check-in sync failed: ${e.message}")
        }
    }

    suspend fun syncDraftCheckInsWithProgress(
        userEmail: String,
        onProgress: (SyncResult.Progress) -> Unit,
        context: Context
    ): SyncResult = withContext(Dispatchers.IO) {
        try {
            Log.d(TAG, "Starting draft check-ins sync with progress...")
            val draftCheckIns = checkInRepository.getDraftCheckIns().firstOrNull() ?: emptyList()

            if (draftCheckIns.isEmpty()) {
                return@withContext SyncResult.Success("No draft check-ins to sync", 0)
            }

            val totalCheckIns = draftCheckIns.size
            var syncedCount = 0

            draftCheckIns.forEachIndexed { index, checkIn ->
                onProgress(
                    SyncResult.Progress(
                        current = index + 1,
                        total = totalCheckIns,
                        customerName = checkIn.customerName
                    )
                )

                if (uploadSingleCheckIn(checkIn, context)) {
                    syncedCount++
                }
            }

            SyncResult.Success(
                message = "Successfully synced $syncedCount of $totalCheckIns check-ins",
                count = syncedCount
            )
        } catch (e: Exception) {
            Log.e(TAG, "Check-in sync error", e)
            SyncResult.Error("Check-in sync failed: ${e.message}")
        }
    }

    private suspend fun uploadSingleCheckIn(checkIn: CheckIn, context: Context): Boolean {
        return try {
            val serverId = serverHelper.getSelectedServer(context)
            val checkInReq = toRequest(checkIn, serverId)
            val response = apiService.syncCheckIn(checkInReq)

            if (response.isSuccessful) {
                val apiResponse = response.body()
                if (apiResponse?.status == "success") {
                    val updatedCheckIn = checkIn.copy(statusSync = "SENT")
                    checkInRepository.updateCheckIn(updatedCheckIn)
                    Log.d(TAG, "Successfully synced check-in: ${checkIn.customerName}")
                    true
                } else {
                    val errorMessage = apiResponse?.data ?: "Unknown error"
                    Log.e(TAG, "API error for check-in ${checkIn.customerName}: $errorMessage")
                    false
                }
            } else {
                Log.e(TAG, "HTTP error for check-in ${checkIn.customerName}: ${response.code()} ${response.message()}")
                false
            }
        } catch (e: Exception) {
            Log.e(TAG, "Error syncing check-in ${checkIn.customerName}", e)
            false
        }
    }

    internal fun toRequest(checkIn: CheckIn, serverId: String): CheckInRequest {
        return CheckInRequest(
            checkInId = checkIn.checkInId,
            checkInDate = checkIn.checkInDate,
            checkInTime = checkIn.checkInTime,
            userEmail = checkIn.userEmail,
            checkInLatitude = checkIn.checkInLatitude,
            checkInLongitude = checkIn.checkInLongitude,
            accuracy = checkIn.accuracy,
            customerId = checkIn.customerId,
            customerCode = checkIn.customerCode,
            customerName = checkIn.customerName,
            customerAddress = checkIn.customerAddress,
            customerLatitude = checkIn.customerLatitude,
            customerLongitude = checkIn.customerLongitude,
            statusSync = checkIn.statusSync,
            serverId = serverId,
            checkOutTime = checkIn.checkOutTime,
            checkOutLatitude = checkIn.checkOutLatitude,
            checkOutLongitude = checkIn.checkOutLongitude,
            checkOutAccuracy = checkIn.checkOutAccuracy,
            checkOutMode = checkIn.checkOutMode
        )
    }
}
