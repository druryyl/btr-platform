package com.elsasa.bgud.viewmodel

import android.net.ConnectivityManager
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.elsasa.bgud.dao.BarcodeRegistrationRequestDao
import com.elsasa.bgud.datastore.SessionPreferencesDataSource

/**
 * Factory for [SynchronizationViewModel], following the `BTrade3`
 * `…ViewModelFactory` per-screen convention (Architecture §19.3).
 */
class SynchronizationViewModelFactory(
    private val session: SessionPreferencesDataSource,
    private val requestDao: BarcodeRegistrationRequestDao,
    private val connectivityManager: ConnectivityManager?,
    private val baseUrl: String
) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        if (modelClass.isAssignableFrom(SynchronizationViewModel::class.java)) {
            @Suppress("UNCHECKED_CAST")
            return SynchronizationViewModel(
                session,
                requestDao,
                connectivityManager,
                baseUrl
            ) as T
        }
        throw IllegalArgumentException("Unknown ViewModel class")
    }
}
