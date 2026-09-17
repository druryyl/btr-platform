package com.elsasa.bgud.viewmodel

import android.net.ConnectivityManager
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.elsasa.bgud.dao.ReturnOrderDao
import com.elsasa.bgud.datastore.SessionPreferencesDataSource

/**
 * Factory for [ReturnOrderSyncViewModel], following the `BTrade3`
 * `…ViewModelFactory` per-screen convention (Architecture §19.1).
 */
class ReturnOrderSyncViewModelFactory(
    private val session: SessionPreferencesDataSource,
    private val returnOrderDao: ReturnOrderDao,
    private val connectivityManager: ConnectivityManager?,
    private val baseUrl: String
) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        if (modelClass.isAssignableFrom(ReturnOrderSyncViewModel::class.java)) {
            @Suppress("UNCHECKED_CAST")
            return ReturnOrderSyncViewModel(
                session,
                returnOrderDao,
                connectivityManager,
                baseUrl
            ) as T
        }
        throw IllegalArgumentException("Unknown ViewModel class")
    }
}
