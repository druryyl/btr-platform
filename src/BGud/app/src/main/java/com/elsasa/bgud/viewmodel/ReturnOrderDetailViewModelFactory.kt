package com.elsasa.bgud.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.elsasa.bgud.repository.ReturnOrderCaptureRepository

/**
 * Factory for [ReturnOrderDetailViewModel], following the `BTrade3`
 * `…ViewModelFactory` per-screen convention (Architecture §19.1, §19.3) and
 * the `EditBarcodeViewModelFactory` id-carrying precedent (§13.1
 * `return_order_detail?returnOrderId={id}`).
 */
class ReturnOrderDetailViewModelFactory(
    private val captureRepository: ReturnOrderCaptureRepository,
    private val returnOrderId: String
) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        if (modelClass.isAssignableFrom(ReturnOrderDetailViewModel::class.java)) {
            @Suppress("UNCHECKED_CAST")
            return ReturnOrderDetailViewModel(
                captureRepository,
                returnOrderId
            ) as T
        }
        throw IllegalArgumentException("Unknown ViewModel class")
    }
}
