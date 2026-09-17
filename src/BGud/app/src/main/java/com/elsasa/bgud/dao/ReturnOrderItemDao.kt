package com.elsasa.bgud.dao

import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import com.elsasa.bgud.model.ReturnOrderItemEntity
import kotlinx.coroutines.flow.Flow

/**
 * Return Order line-item DAO (Return Order Architecture §6.4, S4.1).
 */
@Dao
interface ReturnOrderItemDao {

    @Query("SELECT * FROM return_order_item_entity WHERE returnOrderId = :returnOrderId ORDER BY noUrut")
    suspend fun listByParent(returnOrderId: String): List<ReturnOrderItemEntity>

    @Query("SELECT * FROM return_order_item_entity WHERE returnOrderId = :returnOrderId ORDER BY noUrut")
    fun observeByParent(returnOrderId: String): Flow<List<ReturnOrderItemEntity>>

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsert(item: ReturnOrderItemEntity)

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsertAll(items: List<ReturnOrderItemEntity>)

    @Query("DELETE FROM return_order_item_entity WHERE returnOrderId = :returnOrderId")
    suspend fun deleteByParent(returnOrderId: String)

    @Query("DELETE FROM return_order_item_entity")
    suspend fun deleteAll()
}
