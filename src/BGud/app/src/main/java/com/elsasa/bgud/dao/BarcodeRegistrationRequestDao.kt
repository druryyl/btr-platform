package com.elsasa.bgud.dao

import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import com.elsasa.bgud.model.BarcodeRegistrationRequestEntity
import kotlinx.coroutines.flow.Flow

/**
 * Offline intent-queue DAO (P-08).
 *
 * The queue is never a lookup source (INV-10). Submission and status
 * refresh run only inside the sync worker (S5.3).
 */
@Dao
interface BarcodeRegistrationRequestDao {

    @Query("SELECT * FROM barcode_registration_request_entity WHERE clientRequestId = :clientRequestId LIMIT 1")
    suspend fun getById(clientRequestId: String): BarcodeRegistrationRequestEntity?

    @Query("SELECT * FROM barcode_registration_request_entity WHERE status = :status ORDER BY createdAt")
    fun listByStatus(status: String): Flow<List<BarcodeRegistrationRequestEntity>>

    @Query("SELECT COUNT(*) FROM barcode_registration_request_entity WHERE status = 'PENDING'")
    fun pendingCount(): Flow<Int>

    @Insert(onConflict = OnConflictStrategy.ABORT)
    suspend fun enqueue(request: BarcodeRegistrationRequestEntity)

    @Query("UPDATE barcode_registration_request_entity SET status = :status, serverNote = :serverNote WHERE clientRequestId = :clientRequestId")
    suspend fun updateOutcome(clientRequestId: String, status: String, serverNote: String)

    @Query("DELETE FROM barcode_registration_request_entity")
    suspend fun deleteAll()
}
