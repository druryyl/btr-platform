package com.elsasa.btrade3.viewmodel

import android.content.Context
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.elsasa.btrade3.model.CheckIn
import com.elsasa.btrade3.model.Order
import com.elsasa.btrade3.repository.CheckInRepository
import com.elsasa.btrade3.repository.OrderRepository
import com.elsasa.btrade3.repository.OrderSyncRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.map
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.launch

sealed class SingleOrderSyncState {
    object Idle : SingleOrderSyncState()
    data class Syncing(val orderLocalId: String) : SingleOrderSyncState()
    data class Completed(val message: String, val isError: Boolean) : SingleOrderSyncState()
}

class OrderListViewModel(
    private val repository: OrderRepository,
    private val orderSyncRepository: OrderSyncRepository,
    private val checkInRepository: CheckInRepository,
    private val userEmail: String
) : ViewModel() {
    val orders: StateFlow<List<Order>> = repository.getAllOrders()
        .stateIn(
            scope = viewModelScope,
            started = SharingStarted.WhileSubscribed(5000),
            initialValue = emptyList()
        )

    val activeVisit: StateFlow<CheckIn?> = checkInRepository
        .observeOpenCheckInForUser(userEmail)
        .stateIn(
            scope = viewModelScope,
            started = SharingStarted.WhileSubscribed(5000),
            initialValue = null
        )

    private val _syncState = MutableStateFlow<SingleOrderSyncState>(SingleOrderSyncState.Idle)
    val syncState: StateFlow<SingleOrderSyncState> = _syncState.asStateFlow()

    private val itemCountCache = mutableMapOf<String, StateFlow<Int>>()

    fun getItemCountAsState(fakturId: String): StateFlow<Int> {
        return itemCountCache.getOrPut(fakturId) {
            repository.getOrderItemsByOrderId(fakturId)
                .map { items -> items.count() }
                .stateIn(
                    scope = viewModelScope,
                    started = SharingStarted.WhileSubscribed(5000),
                    initialValue = 0
                )
        }
    }

    fun deleteOrder(order: Order) {
        viewModelScope.launch {
            repository.deleteOrder(order)
        }
    }

    fun syncOrder(order: Order, context: Context) {
        viewModelScope.launch {
            _syncState.value = SingleOrderSyncState.Syncing(order.orderLocalId)
            try {
                val result = orderSyncRepository.syncSelectedOrdersWithProgress(
                    orderIds = listOf(order.orderId),
                    onProgress = {},
                    context = context
                )
                _syncState.value = when (result) {
                    is OrderSyncRepository.SyncResult.Success ->
                        SingleOrderSyncState.Completed(result.message, isError = result.count == 0)
                    is OrderSyncRepository.SyncResult.Error ->
                        SingleOrderSyncState.Completed(result.message, isError = true)
                    else ->
                        SingleOrderSyncState.Completed("Sync failed", isError = true)
                }
            } catch (e: Exception) {
                _syncState.value = SingleOrderSyncState.Completed(
                    "Sync failed: ${e.message}",
                    isError = true
                )
            }
        }
    }

    fun clearSyncState() {
        _syncState.value = SingleOrderSyncState.Idle
    }
}
