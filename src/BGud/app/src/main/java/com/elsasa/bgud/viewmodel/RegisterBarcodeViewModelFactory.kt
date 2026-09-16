package com.elsasa.bgud.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.elsasa.bgud.dao.BarcodeDao
import com.elsasa.bgud.dao.BarangDao
import com.elsasa.bgud.dao.BarcodeRegistrationRequestDao
import com.elsasa.bgud.datastore.SessionPreferencesDataSource

/**
 * Factory for [RegisterBarcodeViewModel], following the `BTrade3`
 * `…ViewModelFactory` per-screen convention (Architecture §19.3).
 */
class RegisterBarcodeViewModelFactory(
    private val barcodeDao: BarcodeDao,
    private val barangDao: BarangDao,
    private val requestDao: BarcodeRegistrationRequestDao,
    private val session: SessionPreferencesDataSource,
    private val initialBarcode: String
) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        if (modelClass.isAssignableFrom(RegisterBarcodeViewModel::class.java)) {
            @Suppress("UNCHECKED_CAST")
            return RegisterBarcodeViewModel(
                barcodeDao,
                barangDao,
                requestDao,
                session,
                initialBarcode
            ) as T
        }
        throw IllegalArgumentException("Unknown ViewModel class")
    }
}
