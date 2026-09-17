package com.elsasa.bgud.dao

import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import com.elsasa.bgud.model.ReturnOrderEntity
import kotlinx.coroutines.flow.Flow

/**
 * Offline Return Order capture DAO (Return Order Architecture §6.4, S4.1).
 *
 * Local-only source; device vocabulary is DRAFT | SYNCED (ADR-RO-006).
 */
@Dao
interface ReturnOrderDao {

    @Query("SELECT * FROM return_order_entity WHERE returnOrderId = :returnOrderId LIMIT 1")
    suspend fun getById(returnOrderId: String): ReturnOrderEntity?

    @Query("SELECT * FROM return_order_entity WHERE status = :status ORDER BY createdAt DESC")
    suspend fun listByStatus(status: String): List<ReturnOrderEntity>

    @Query("SELECT * FROM return_order_entity WHERE status = :status ORDER BY createdAt DESC")
    fun observeByStatus(status: String): Flow<List<ReturnOrderEntity>>

    @Query("SELECT * FROM return_order_entity ORDER BY createdAt DESC")
    fun observeAll(): Flow<List<ReturnOrderEntity>>

    /**
     * Local searchable list backing SCR-MOB-RO-001 (S4.6, §12.1, §20): by
     * Customer name, date, or note against the local Room rows only — no
     * network — plus the status filter (empty `status` = all, §11.1
     * `Draft`/`Synced` only). `createdAt` is the local capture day rendered
     * for the search bar (same `localtime` basis as the list display).
     */
    @Query(
        "SELECT * FROM return_order_entity " +
            "WHERE (:status = '' OR status = :status) " +
            "AND (customerName LIKE '%' || :query || '%' " +
            "OR note LIKE '%' || :query || '%' " +
            "OR date(createdAt / 1000, 'unixepoch', 'localtime') " +
            "LIKE '%' || :query || '%') " +
            "ORDER BY createdAt DESC LIMIT :limit OFFSET :offset"
    )
    suspend fun search(query: String, status: String, limit: Int, offset: Int): List<ReturnOrderEntity>

    @Query("SELECT COUNT(*) FROM return_order_entity WHERE status = :status")
    suspend fun countByStatus(status: String): Int

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsert(order: ReturnOrderEntity)

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsertAll(orders: List<ReturnOrderEntity>)

    @Query("DELETE FROM return_order_entity WHERE returnOrderId = :returnOrderId")
    suspend fun deleteById(returnOrderId: String)

    @Query("DELETE FROM return_order_entity")
    suspend fun deleteAll()
}
