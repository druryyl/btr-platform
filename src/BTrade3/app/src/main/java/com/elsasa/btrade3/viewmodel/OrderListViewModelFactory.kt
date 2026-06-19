package com.elsasa.btrade3.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.elsasa.btrade3.repository.CheckInRepository
import com.elsasa.btrade3.repository.OrderRepository
import com.elsasa.btrade3.repository.OrderSyncRepository

class OrderListViewModelFactory(
    private val repository: OrderRepository,
    private val orderSyncRepository: OrderSyncRepository,
    private val checkInRepository: CheckInRepository,
    private val userEmail: String
) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        if (modelClass.isAssignableFrom(OrderListViewModel::class.java)) {
            @Suppress("UNCHECKED_CAST")
            return OrderListViewModel(
                repository,
                orderSyncRepository,
                checkInRepository,
                userEmail
            ) as T
        }
        throw IllegalArgumentException("Unknown ViewModel class")
    }
}
