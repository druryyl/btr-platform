package com.elsasa.btrade3.dao

import androidx.room.*
import com.elsasa.btrade3.model.CheckIn
import kotlinx.coroutines.flow.Flow

@Dao
interface CheckInDao {
    @Query("SELECT * FROM checkin_table ORDER BY checkInDate DESC, checkInTime DESC")
    fun getAllCheckIns(): Flow<List<CheckIn>>

    @Query("SELECT * FROM checkin_table WHERE checkInId = :checkInId")
    suspend fun getCheckInById(checkInId: String): CheckIn?

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertCheckIn(checkIn: CheckIn)

    @Update
    suspend fun updateCheckIn(checkIn: CheckIn)

    @Delete
    suspend fun deleteCheckIn(checkIn: CheckIn)

    @Query("DELETE FROM checkin_table WHERE checkInId = :checkInId")
    suspend fun deleteCheckInById(checkInId: String)

    @Query("""
        SELECT * FROM checkin_table
        WHERE userEmail = :userEmail AND checkOutTime = '' AND isExplicitlyOpen = 1
        ORDER BY checkInDate DESC, checkInTime DESC LIMIT 1
    """)
    suspend fun getOpenCheckInForUser(userEmail: String): CheckIn?

    @Query("""
        SELECT * FROM checkin_table
        WHERE userEmail = :userEmail AND checkOutTime = '' AND isExplicitlyOpen = 1
        ORDER BY checkInDate DESC, checkInTime DESC LIMIT 1
    """)
    fun observeOpenCheckInForUser(userEmail: String): Flow<CheckIn?>

    @Query("SELECT * FROM checkin_table WHERE statusSync = 'DRAFT' ORDER BY checkInDate DESC, checkInTime DESC")
    fun getDraftCheckIns(): Flow<List<CheckIn>>
}