package com.elsasa.bgud.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.elsasa.bgud.dao.BarangDao
import com.elsasa.bgud.dao.BarcodeDao
import com.elsasa.bgud.dao.CustomerDao
import com.elsasa.bgud.dao.DriverDao
import com.elsasa.bgud.dao.SalesPersonDao
import com.elsasa.bgud.datastore.SessionPreferencesDataSource
import com.elsasa.bgud.repository.ReturnOrderCaptureRepository

/**
 * Factory for [CreateReturnOrderViewModel], following the `BTrade3`
 * `…ViewModelFactory` per-screen convention (Architecture §19.1, §19.3).
 */
class CreateReturnOrderViewModelFactory(
    private val captureRepository: ReturnOrderCaptureRepository,
    private val customerDao: CustomerDao,
    private val salesPersonDao: SalesPersonDao,
    private val driverDao: DriverDao,
    private val barangDao: BarangDao,
    private val barcodeDao: BarcodeDao,
    private val session: SessionPreferencesDataSource
) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        if (modelClass.isAssignableFrom(CreateReturnOrderViewModel::class.java)) {
            @Suppress("UNCHECKED_CAST")
            return CreateReturnOrderViewModel(
                captureRepository,
                customerDao,
                salesPersonDao,
                driverDao,
                barangDao,
                barcodeDao,
                session
            ) as T
        }
        throw IllegalArgumentException("Unknown ViewModel class")
    }
}
