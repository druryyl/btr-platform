package com.elsasa.bgud.dao

import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import com.elsasa.bgud.model.BarangEntity
import kotlinx.coroutines.flow.Flow

/**
 * Barang reference-cache DAO (ADR-005).
 *
 * Local Item search backing registration capture (S5.7): only a cached
 * Active Item may be selected (BQ-7, IR-M3).
 */
@Dao
interface BarangDao {

    @Query("SELECT * FROM barang_entity WHERE brgId = :brgId LIMIT 1")
    suspend fun getById(brgId: String): BarangEntity?

    @Query("SELECT * FROM barang_entity ORDER BY brgName")
    fun getAll(): Flow<List<BarangEntity>>

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsert(barang: BarangEntity)

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsertAll(barangs: List<BarangEntity>)

    @Query("DELETE FROM barang_entity")
    suspend fun deleteAll()
}
