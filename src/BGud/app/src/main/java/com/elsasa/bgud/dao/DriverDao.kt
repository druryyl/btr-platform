package com.elsasa.bgud.dao

import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import com.elsasa.bgud.model.DriverEntity
import kotlinx.coroutines.flow.Flow

/**
 * Optional Driver reference-cache DAO (Return Order Architecture §6.4,
 * ADR-RO-005, S4.1).
 *
 * Full-replace download (deleteAll + upsertAll); local search only.
 */
@Dao
interface DriverDao {

    @Query("SELECT * FROM driver_entity WHERE driverId = :driverId LIMIT 1")
    suspend fun getById(driverId: String): DriverEntity?

    @Query(
        "SELECT * FROM driver_entity " +
            "WHERE driverName LIKE '%' || :query || '%' " +
            "ORDER BY driverName LIMIT 50"
    )
    suspend fun search(query: String): List<DriverEntity>

    @Query("SELECT * FROM driver_entity ORDER BY driverName")
    fun getAll(): Flow<List<DriverEntity>>

    @Query("SELECT * FROM driver_entity ORDER BY driverName")
    suspend fun listAll(): List<DriverEntity>

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsert(driver: DriverEntity)

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsertAll(drivers: List<DriverEntity>)

    @Query("DELETE FROM driver_entity")
    suspend fun deleteAll()
}
