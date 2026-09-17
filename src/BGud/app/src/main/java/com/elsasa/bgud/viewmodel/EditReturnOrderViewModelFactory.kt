package com.elsasa.bgud.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.elsasa.bgud.dao.BarangDao
import com.elsasa.bgud.dao.BarcodeDao
import com.elsasa.bgud.dao.CustomerDao
import com.elsasa.bgud.dao.DriverDao
import com.elsasa.bgud.dao.SalesPersonDao
import com.elsasa.bgud.repository.ReturnOrderCaptureRepository

/**
 * Factory for [EditReturnOrderViewModel], following the `BTrade3`
 * `…ViewModelFactory` per-screen convention (Architecture §19.1, §19.3) and
 * the id-carrying precedent (§13.1 `return_order_edit?returnOrderId={id}`).
 */
class EditReturnOrderViewModelFactory(
    private val captureRepository: ReturnOrderCaptureRepository,
    private val customerDao: CustomerDao,
    private val salesPersonDao: SalesPersonDao,
    private val driverDao: DriverDao,
    private val barangDao: BarangDao,
    private val barcodeDao: BarcodeDao,
    private val returnOrderId: String
) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        if (modelClass.isAssignableFrom(EditReturnOrderViewModel::class.java)) {
            @Suppress("UNCHECKED_CAST")
            return EditReturnOrderViewModel(
                captureRepository,
                customerDao,
                salesPersonDao,
                driverDao,
                barangDao,
                barcodeDao,
                returnOrderId
            ) as T
        }
        throw IllegalArgumentException("Unknown ViewModel class")
    }
}
