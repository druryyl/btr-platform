package com.elsasa.btrade3.repository

import android.content.Context
import com.elsasa.btrade3.model.Order
import com.elsasa.btrade3.model.OrderSyncStatus
import com.elsasa.btrade3.model.api.OrderItemSyncDto
import com.elsasa.btrade3.model.api.OrderSyncRequest
import com.elsasa.btrade3.network.ApiService
import com.elsasa.btrade3.util.ServerHelper
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.async
import kotlinx.coroutines.awaitAll
import kotlinx.coroutines.flow.firstOrNull
import kotlinx.coroutines.withContext

class OrderSyncRepository(
    private val apiService: ApiService,
    private val orderRepository: OrderRepository,
    private val serverHelper: ServerHelper
) {
    sealed class SyncResult {
        data class Success(val message: String, val count: Int) : SyncResult()
        data class Error(val message: String) : SyncResult()
        data class Progress(val current: Int, val total: Int, val orderCode: String) : SyncResult()
        object Loading : SyncResult()
    }

    suspend fun syncSelectedOrdersWithProgress(
        orderIds: List<String>,
        onProgress: (SyncResult.Progress) -> Unit,
        context: Context
    ): SyncResult = withContext(Dispatchers.IO) {
        try {
            if (orderIds.isEmpty()) {
                return@withContext SyncResult.Error("No orders selected for sync")
            }

            val readyOrders = orderRepository.getReadyToSyncOrders().firstOrNull() ?: emptyList()
            val ordersToSync = readyOrders.filter { it.orderId in orderIds }

            if (ordersToSync.isEmpty()) {
                return@withContext SyncResult.Error("No eligible orders to sync")
            }

            val totalOrders = ordersToSync.size
            val syncResults = ordersToSync.mapIndexed { index, order ->
                async {
                    onProgress(
                        SyncResult.Progress(
                            current = index + 1,
                            total = totalOrders,
                            orderCode = order.orderLocalId
                        )
                    )
                    syncSingleOrder(order, context)
                }
            }.awaitAll()

            val syncedCount = syncResults.count { it }

            SyncResult.Success(
                message = "Successfully synced $syncedCount of $totalOrders orders",
                count = syncedCount
            )
        } catch (e: Exception) {
            SyncResult.Error("Sync failed: ${e.message}")
        }
    }

    private suspend fun syncSingleOrder(order: Order, context: Context): Boolean {
        return try {
            val orderItems = orderRepository.getOrderItemsByOrderId(order.orderId).firstOrNull() ?: emptyList()

            if (!OrderSyncStatus.isEligibleForSync(order, orderItems.size)) {
                return false
            }

            val serverId = serverHelper.getSelectedServer(context)

            val orderItemDtos = orderItems.map { item ->
                OrderItemSyncDto(
                    orderId = item.orderId,
                    noUrut = item.noUrut,
                    brgId = item.brgId,
                    brgCode = item.brgCode,
                    brgName = item.brgName,
                    kategoriName = item.kategoriName,
                    qtyBesar = item.qtyBesar,
                    satBesar = item.satBesar,
                    qtyKecil = item.qtyKecil,
                    satKecil = item.satKecil,
                    qtyBonus = item.qtyBonus,
                    konversi = item.konversi,
                    unitPrice = item.unitPrice,
                    disc1 = item.disc1,
                    disc2 = item.disc2,
                    disc3 = item.disc3,
                    disc4 = item.disc4,
                    lineTotal = item.lineTotal
                )
            }

            val syncRequest = OrderSyncRequest(
                orderId = order.orderId,
                orderLocalId = order.orderLocalId,
                customerId = order.customerId,
                customerCode = order.customerCode,
                customerName = order.customerName,
                address = order.customerAddress,
                orderDate = order.orderDate,
                salesId = order.salesId,
                salesName = order.salesName,
                totalAmount = order.totalAmount,
                userEmail = order.userEmail,
                orderNote = order.orderNote,
                serverId = serverId,
                listItem = orderItemDtos
            )

            val response = apiService.syncOrder(syncRequest)

            if (response.isSuccessful) {
                val apiResponse = response.body()
                if (apiResponse?.status == "success") {
                    orderRepository.updateOrderSyncStatus(
                        orderId = order.orderId,
                        status = OrderSyncStatus.SENT
                    )
                    true
                } else {
                    false
                }
            } else {
                false
            }
        } catch (e: Exception) {
            false
        }
    }
}
