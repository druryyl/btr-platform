package com.elsasa.bgud.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.elsasa.bgud.dao.BarcodeDao

/**
 * Factory for [BarcodeRegistryViewModel], following the `BTrade3`
 * `…ViewModelFactory` per-screen convention (Architecture §19.3).
 */
class BarcodeRegistryViewModelFactory(
    private val barcodeDao: BarcodeDao
) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        if (modelClass.isAssignableFrom(BarcodeRegistryViewModel::class.java)) {
            @Suppress("UNCHECKED_CAST")
            return BarcodeRegistryViewModel(barcodeDao) as T
        }
        throw IllegalArgumentException("Unknown ViewModel class")
    }
}
