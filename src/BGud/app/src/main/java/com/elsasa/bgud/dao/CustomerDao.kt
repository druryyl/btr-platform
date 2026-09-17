package com.elsasa.bgud.dao

import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import com.elsasa.bgud.model.CustomerEntity
import kotlinx.coroutines.flow.Flow

/**
 * Mandatory Customer reference-cache DAO (Return Order Architecture §6.4,
 * GAP-004, S4.1).
 *
 * Full-replace download (deleteAll + upsertAll); local search only.
 */
@Dao
interface CustomerDao {

    @Query("SELECT * FROM customer_entity WHERE customerId = :customerId LIMIT 1")
    suspend fun getById(customerId: String): CustomerEntity?

    @Query(
        "SELECT * FROM customer_entity " +
            "WHERE customerCode LIKE '%' || :query || '%' " +
            "OR customerName LIKE '%' || :query || '%' " +
            "ORDER BY customerName LIMIT 50"
    )
    suspend fun search(query: String): List<CustomerEntity>

    @Query("SELECT * FROM customer_entity ORDER BY customerName")
    fun getAll(): Flow<List<CustomerEntity>>

    @Query("SELECT * FROM customer_entity ORDER BY customerName")
    suspend fun listAll(): List<CustomerEntity>

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsert(customer: CustomerEntity)

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsertAll(customers: List<CustomerEntity>)

    @Query("DELETE FROM customer_entity")
    suspend fun deleteAll()
}
