package com.elsasa.bgud.dao

import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import com.elsasa.bgud.model.SalesPersonEntity
import kotlinx.coroutines.flow.Flow

/**
 * Optional Salesman reference-cache DAO (Return Order Architecture §6.4,
 * ADR-RO-005, S4.1).
 *
 * Full-replace download (deleteAll + upsertAll); local search only.
 */
@Dao
interface SalesPersonDao {

    @Query("SELECT * FROM salesperson_entity WHERE salesPersonId = :salesPersonId LIMIT 1")
    suspend fun getById(salesPersonId: String): SalesPersonEntity?

    @Query(
        "SELECT * FROM salesperson_entity " +
            "WHERE salesPersonName LIKE '%' || :query || '%' " +
            "ORDER BY salesPersonName LIMIT 50"
    )
    suspend fun search(query: String): List<SalesPersonEntity>

    @Query("SELECT * FROM salesperson_entity ORDER BY salesPersonName")
    fun getAll(): Flow<List<SalesPersonEntity>>

    @Query("SELECT * FROM salesperson_entity ORDER BY salesPersonName")
    suspend fun listAll(): List<SalesPersonEntity>

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsert(salesPerson: SalesPersonEntity)

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsertAll(salesPersons: List<SalesPersonEntity>)

    @Query("DELETE FROM salesperson_entity")
    suspend fun deleteAll()
}
