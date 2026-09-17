package com.elsasa.bgud.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.elsasa.bgud.dao.ReturnOrderDao
import com.elsasa.bgud.dao.ReturnOrderItemDao

/**
 * Factory for [ReturnOrderListViewModel], following the `BTrade3`
 * `…ViewModelFactory` per-screen convention (Architecture §19.3).
 */
class ReturnOrderListViewModelFactory(
    private val returnOrderDao: ReturnOrderDao,
    private val returnOrderItemDao: ReturnOrderItemDao
) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        if (modelClass.isAssignableFrom(ReturnOrderListViewModel::class.java)) {
            @Suppress("UNCHECKED_CAST")
            return ReturnOrderListViewModel(returnOrderDao, returnOrderItemDao) as T
        }
        throw IllegalArgumentException("Unknown ViewModel class")
    }
}
