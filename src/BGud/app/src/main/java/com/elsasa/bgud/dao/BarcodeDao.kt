package com.elsasa.bgud.dao

import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import com.elsasa.bgud.model.BarcodeEntity
import kotlinx.coroutines.flow.Flow

/**
 * Cache-first lookup DAO (P-07, ADR-006).
 *
 * Every value lookup goes through the unique `barcodeValueKey` index;
 * no scan triggers a network call. The cache holds only Active barcodes
 * (TQ-6); deactivation is a physical removal.
 */
@Dao
interface BarcodeDao {

    /** Cache-first single-scan resolution (S5.6). */
    @Query("SELECT * FROM barcode_entity WHERE barcodeValueKey = :barcodeValueKey LIMIT 1")
    suspend fun getByKey(barcodeValueKey: String): BarcodeEntity?

    @Query("SELECT * FROM barcode_entity WHERE brgId = :brgId ORDER BY barcodeValue")
    fun listByBrg(brgId: String): Flow<List<BarcodeEntity>>

    @Query("SELECT * FROM barcode_entity ORDER BY brgName, barcodeValue LIMIT :limit OFFSET :offset")
    suspend fun paged(limit: Int, offset: Int): List<BarcodeEntity>

    /**
     * Registry list search backing SCR-MOB-005 (S5.8, §12.7, §20): by
     * Barcode, Item Code, or Item Name against the local Active cache
     * only — no network. Supporting prerequisite not named in the §3
     * impact inventory; required for the searchable list.
     */
    @Query(
        "SELECT * FROM barcode_entity " +
            "WHERE barcodeValue LIKE '%' || :query || '%' " +
            "OR brgCode LIKE '%' || :query || '%' " +
            "OR brgName LIKE '%' || :query || '%' " +
            "ORDER BY brgName, barcodeValue LIMIT :limit OFFSET :offset"
    )
    suspend fun search(query: String, limit: Int, offset: Int): List<BarcodeEntity>

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsert(barcode: BarcodeEntity)

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsertAll(barcodes: List<BarcodeEntity>)

    /** Deactivation is a removal from the read-model cache (TQ-3). */
    @Query("DELETE FROM barcode_entity WHERE brgBarcodeId = :brgBarcodeId")
    suspend fun deleteById(brgBarcodeId: String)

    @Query("DELETE FROM barcode_entity")
    suspend fun deleteAll()
}
