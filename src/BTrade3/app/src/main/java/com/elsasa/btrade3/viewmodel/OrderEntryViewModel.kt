package com.elsasa.btrade3.viewmodel

import android.content.Context
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.elsasa.btrade3.model.Order
import com.elsasa.btrade3.model.OrderSyncStatus
import com.elsasa.btrade3.repository.OrderRepository
import com.elsasa.btrade3.ui.getUserEmail
import com.elsasa.btrade3.util.FriendlyIdGenerator
import com.elsasa.btrade3.util.LastSelectionManager
import com.elsasa.btrade3.util.UlidHelper
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import java.text.SimpleDateFormat
import java.util.*

class OrderEntryViewModel(
    private val repository: OrderRepository,
    private val context: Context

) : ViewModel() {
    private val _order = MutableStateFlow<Order?>(null)
    val order: StateFlow<Order?> = _order.asStateFlow()

    private val _actionMessage = MutableStateFlow<String?>(null)
    val actionMessage: StateFlow<String?> = _actionMessage.asStateFlow()

    private val lastSelectionManager = LastSelectionManager(context)
    private var hasCreatedNewOrder = false

    fun loadOrder(orderId: String) {
        viewModelScope.launch {
            _order.value = repository.getOrderById(orderId)
            hasCreatedNewOrder = false
        }
    }

    fun createNewOrder(context: Context) {
        if (hasCreatedNewOrder) return

        val currentDate = SimpleDateFormat("yyyy-MM-dd", Locale.getDefault()).format(Date())
        val userEmail = getUserEmail(context) ?: ""
        val idGenerator = FriendlyIdGenerator()

        val lastSalesPersonId = lastSelectionManager.getLastSalesPersonId() ?: ""
        val lastSalesPersonName = lastSelectionManager.getLastSalesPersonName() ?: ""

        val orderId = UlidHelper.generate()
        val orderLocalCode = idGenerator.generateCompactDateSequenceId(context)

        val newOrder =  Order(
            orderId = orderId,
            orderLocalId = orderLocalCode,
            customerId = "",
            customerCode = "",
            customerName = "",
            customerAddress = "",
            orderDate = currentDate,
            salesId = lastSalesPersonId,
            salesName = lastSalesPersonName,
            totalAmount = 0.0,
            userEmail = userEmail,
            statusSync = OrderSyncStatus.IN_PROGRESS,
            fakturCode = "",
            orderNote = ""
        )
        _order.value = newOrder
        hasCreatedNewOrder = true

        saveOrder(newOrder)
    }

    fun finishOrder() {
        viewModelScope.launch {
            val current = _order.value ?: return@launch
            val itemCount = repository.getItemCount(current.orderId)
            val validationError = OrderSyncStatus.validateForReadyToSync(current, itemCount)
            if (validationError != null) {
                _actionMessage.value = validationError
                return@launch
            }

            val updatedOrder = current.copy(statusSync = OrderSyncStatus.READY_TO_SYNC)
            repository.insertOrder(updatedOrder)
            _order.value = repository.getOrderById(current.orderId)
            _actionMessage.value = "Order marked as ready to sync"
        }
    }

    fun reopenForEditing() {
        viewModelScope.launch {
            val current = _order.value ?: return@launch
            if (current.statusSync != OrderSyncStatus.READY_TO_SYNC) return@launch

            val updatedOrder = current.copy(statusSync = OrderSyncStatus.IN_PROGRESS)
            repository.insertOrder(updatedOrder)
            _order.value = repository.getOrderById(current.orderId)
            _actionMessage.value = "Order reopened for editing"
        }
    }

    fun clearActionMessage() {
        _actionMessage.value = null
    }

    fun updateCustomerInfo(customerId: String, customerCode: String, customerName: String, customerAddress: String,
                           customerLatitude: Double, customerLongitude: Double) {
        val current = _order.value ?: return
        val updatedOrder = current.copy(
            customerId = customerId,
            customerCode = customerCode,
            customerName = customerName,
            customerAddress = customerAddress,
            customerLatitude = customerLatitude,
            customerLongitude = customerLongitude,
        )
        _order.value = updatedOrder
        saveOrderAndReload(updatedOrder)

        lastSelectionManager.saveLastCustomer(customerId,customerCode, customerName, customerAddress)
    }

    fun updateSalesInfo(salesId: String, salesName: String) {
        val current = _order.value ?: return
        val updatedFaktur = current.copy(
            salesId = salesId,
            salesName = salesName
        )
        _order.value = updatedFaktur
        saveOrderAndReload(updatedFaktur)

        lastSelectionManager.saveLastSalesPerson(salesId,salesName)
    }

    fun updateOrderNote(orderNote: String) {
        val current = _order.value ?: return
        val updatedFaktur = current.copy(
            orderNote = orderNote
        )
        _order.value = updatedFaktur
        saveOrderAndReload(updatedFaktur)
    }

    fun updateTotalAmount(totalAmount: Double) {
        val current = _order.value ?: return
        val updatedFaktur = current.copy(totalAmount = totalAmount)
        _order.value = updatedFaktur
        saveOrderAndReload(updatedFaktur)
    }

    fun updateUserEmail(email: String){
        val current = _order.value?:return
        val updatedFaktur = current.copy(userEmail = email)
        _order.value = updatedFaktur
        saveOrderAndReload(updatedFaktur)
    }

    private fun saveOrder(orderToSave: Order) {
        viewModelScope.launch {
            repository.insertOrder(orderToSave)
        }
    }
    private fun saveOrderAndReload(orderToSave: Order) {
        viewModelScope.launch {
            repository.insertOrder(orderToSave)
            _order.value = repository.getOrderById(orderToSave.orderId)
        }
    }
}
