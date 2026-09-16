package com.elsasa.bgud.viewmodel

import android.net.ConnectivityManager
import android.net.Network
import android.net.NetworkCapabilities
import android.net.NetworkRequest
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.elsasa.bgud.dao.BarcodeRegistrationRequestDao
import com.elsasa.bgud.datastore.SessionPreferencesDataSource
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.combine
import kotlinx.coroutines.flow.map
import kotlinx.coroutines.flow.stateIn

/**
 * Home view model (SCR-MOB-002, Architecture §19.3).
 *
 * Key fields: `user`, `warehouse`, `office`, `isOnline`, `lastSyncAt`,
 * `pendingCount`. Sources: DataStore + Room only — no network call is
 * issued from this screen (the sync status card summarizes local state).
 *
 * - `user` / `warehouse` / `office` re-expose the login-bound session
 *   (`userId`, `warehouseCode`, `officeCode`; IR-09 — `warehouseCode`, never
 *   a `ServerId`, is the stored tenant binding; `officeCode` is the
 *   login-returned `serverId` kept for display).
 * - `lastSyncAt` is the most recent committed download timestamp
 *   (`max(lastBarangSync, lastBarcodeSync)`; `0` = never synced).
 * - `pendingCount` counts locally queued `PENDING` requests (the offline
 *   intent queue, P-08; never a lookup source, INV-10).
 * - `isOnline` reflects the device connectivity (`Online | Offline`,
 *   §14.6) via `ConnectivityManager`; it gates nothing on this screen —
 *   the S5.10 Synchronization screen disables Sync Now while offline
 *   (IR-M5).
 */
class HomeViewModel(
    session: SessionPreferencesDataSource,
    requestDao: BarcodeRegistrationRequestDao,
    private val connectivityManager: ConnectivityManager?
) : ViewModel() {

    val user: StateFlow<String> = session.userId
        .map { it.orEmpty() }
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5_000), "")

    val warehouse: StateFlow<String> = session.warehouseCode
        .map { it.orEmpty() }
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5_000), "")

    val office: StateFlow<String> = session.officeCode
        .map { it.orEmpty() }
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5_000), "")

    val lastSyncAt: StateFlow<Long> = combine(
        session.lastBarangSync,
        session.lastBarcodeSync
    ) { barangSync, barcodeSync -> maxOf(barangSync, barcodeSync) }
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5_000), 0L)

    val pendingCount: StateFlow<Int> = requestDao.pendingCount()
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5_000), 0)

    private val _isOnline = MutableStateFlow(currentlyOnline())
    val isOnline: StateFlow<Boolean> = _isOnline

    private val networkCallback = object : ConnectivityManager.NetworkCallback() {
        override fun onAvailable(network: Network) {
            _isOnline.value = true
        }

        override fun onLost(network: Network) {
            _isOnline.value = currentlyOnline()
        }

        override fun onCapabilitiesChanged(
            network: Network,
            networkCapabilities: NetworkCapabilities
        ) {
            _isOnline.value = currentlyOnline()
        }
    }

    init {
        connectivityManager?.registerNetworkCallback(
            NetworkRequest.Builder()
                .addCapability(NetworkCapabilities.NET_CAPABILITY_INTERNET)
                .build(),
            networkCallback
        )
    }

    override fun onCleared() {
        connectivityManager?.unregisterNetworkCallback(networkCallback)
        super.onCleared()
    }

    private fun currentlyOnline(): Boolean {
        val manager = connectivityManager ?: return false
        val network = manager.activeNetwork ?: return false
        val capabilities = manager.getNetworkCapabilities(network) ?: return false
        return capabilities.hasCapability(NetworkCapabilities.NET_CAPABILITY_INTERNET) &&
            capabilities.hasCapability(NetworkCapabilities.NET_CAPABILITY_VALIDATED)
    }
}
