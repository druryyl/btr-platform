package com.elsasa.btrade3.repository

import com.elsasa.btrade3.dao.CheckInDao
import com.elsasa.btrade3.model.CheckIn
import com.elsasa.btrade3.model.CheckOutMode
import kotlinx.coroutines.flow.Flow
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

class CheckInRepository(
    private val checkInDao: CheckInDao
) {
    fun getAllCheckIns(): Flow<List<CheckIn>> = checkInDao.getAllCheckIns()

    fun getDraftCheckIns(): Flow<List<CheckIn>> = checkInDao.getDraftCheckIns()

    suspend fun getCheckInById(checkInId: String): CheckIn? = checkInDao.getCheckInById(checkInId)

    suspend fun insertCheckIn(checkIn: CheckIn) = checkInDao.insertCheckIn(checkIn)

    suspend fun updateCheckIn(checkIn: CheckIn) = checkInDao.updateCheckIn(checkIn)

    suspend fun deleteCheckIn(checkIn: CheckIn) = checkInDao.deleteCheckIn(checkIn)

    suspend fun deleteCheckInById(checkInId: String) = checkInDao.deleteCheckInById(checkInId)

    suspend fun getOpenCheckInForUser(userEmail: String): CheckIn? =
        checkInDao.getOpenCheckInForUser(userEmail)

    fun observeOpenCheckInForUser(userEmail: String): Flow<CheckIn?> =
        checkInDao.observeOpenCheckInForUser(userEmail)

    /**
     * Closes an open visit manually. Records end time only; checkout GPS is not captured.
     */
    suspend fun manualCheckOut(checkIn: CheckIn): CheckIn {
        if (!checkIn.isOpenVisit()) return checkIn
        val updated = checkIn.copy(
            checkOutTime = SimpleDateFormat("HH:mm:ss", Locale.getDefault()).format(Date()),
            checkOutMode = CheckOutMode.MANUAL,
            statusSync = "DRAFT",
            isExplicitlyOpen = false
        )
        updateCheckIn(updated)
        return updated
    }

    /**
     * Silently closes the salesperson's open visit using the next check-in moment.
     * Returns the closed visit, or null if none was open.
     */
    suspend fun autoCloseOpenVisit(
        userEmail: String,
        checkOutTime: String,
        checkOutLatitude: Double,
        checkOutLongitude: Double,
        checkOutAccuracy: Float
    ): CheckIn? {
        val openVisit = getOpenCheckInForUser(userEmail) ?: return null
        val closed = openVisit.copy(
            checkOutTime = checkOutTime,
            checkOutLatitude = checkOutLatitude,
            checkOutLongitude = checkOutLongitude,
            checkOutAccuracy = checkOutAccuracy,
            checkOutMode = CheckOutMode.AUTO,
            statusSync = "DRAFT",
            isExplicitlyOpen = false
        )
        updateCheckIn(closed)
        return closed
    }
}