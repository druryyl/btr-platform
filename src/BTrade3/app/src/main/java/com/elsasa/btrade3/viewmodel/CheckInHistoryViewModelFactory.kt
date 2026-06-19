package com.elsasa.btrade3.viewmodel


import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.elsasa.btrade3.repository.CheckInRepository
import com.elsasa.btrade3.repository.CheckInSyncRepository

class CheckInHistoryViewModelFactory(
    private val checkInRepository: CheckInRepository,
    private val checkInSyncRepository: CheckInSyncRepository
) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        if (modelClass.isAssignableFrom(CheckInHistoryViewModel::class.java)) {
            @Suppress("UNCHECKED_CAST")
            return CheckInHistoryViewModel(checkInRepository, checkInSyncRepository) as T
        }
        throw IllegalArgumentException("Unknown ViewModel class")
    }
}
