package com.elsasa.bgud.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.elsasa.bgud.dao.BarcodeDao
import com.elsasa.bgud.dao.BarangDao
import com.elsasa.bgud.dao.BarcodeRegistrationRequestDao
import com.elsasa.bgud.datastore.SessionPreferencesDataSource

/**
 * Factory for [EditBarcodeViewModel], following the `BTrade3`
 * `…ViewModelFactory` per-screen convention (Architecture §19.3).
 */
class EditBarcodeViewModelFactory(
    private val barcodeDao: BarcodeDao,
    private val barangDao: BarangDao,
    private val requestDao: BarcodeRegistrationRequestDao,
    private val session: SessionPreferencesDataSource,
    private val barcodeId: String
) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        if (modelClass.isAssignableFrom(EditBarcodeViewModel::class.java)) {
            @Suppress("UNCHECKED_CAST")
            return EditBarcodeViewModel(
                barcodeDao,
                barangDao,
                requestDao,
                session,
                barcodeId
            ) as T
        }
        throw IllegalArgumentException("Unknown ViewModel class")
    }
}
