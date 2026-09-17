package com.elsasa.bgud.viewmodel

import android.content.Context
import android.net.ConnectivityManager
import android.net.Network
import android.net.NetworkCapabilities
import android.net.NetworkRequest
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import androidx.work.WorkInfo
import androidx.work.WorkManager
import com.elsasa.bgud.dao.ReturnOrderDao
import com.elsasa.bgud.datastore.SessionPreferencesDataSource
import com.elsasa.bgud.model.ReturnOrderEntity
import com.elsasa.bgud.sync.ReturnOrderSyncWorker
import kotlinx.coroutines.Job
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.combine
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.flow.map
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.launch
import java.util.UUID

/**
 * Return Order synchronization view model (SCR-MOB-RO-005, Architecture
 * §14.3, §19.1).
 *
 * Extension of the existing Synchronization surface (S5.10 SCR-MOB-007): the
 * Return Order sync state is shown alongside the barcode state and `Sync Now`
 * triggers the Return Order worker in addition to the barcode worker.
 *
 * Key fields (§19.1): `pendingCount`, `syncedCount`, `lastRefSync`,
 * `isSyncing`. Sources: Room (`return_order_entity`) + DataStore reference
 * timestamps only; the screen issues no network call itself. `Sync Now`
 * enqueues the one-shot [ReturnOrderSyncWorker] (S4.5: submit `DRAFT` orders
 * → reference downloads, §20) and observes its `WorkInfo`; the refreshed
 * timestamps and counts arrive automatically through the DataStore/Room flows.
 *
 * Rules (no new decisions):
 * - Device vocabulary is `DRAFT` | `SYNCED` (ADR-RO-006): pending = `DRAFT`,
 *   synced = `SYNCED`; no import outcome is ever counted.
 * - Offline disables Sync Now (IR-M9, §14.6 `Online | Offline`); the guard is
 *   duplicated here for safety, matching the barcode view model.
 * - One sync run at a time (OQ-1): a Sync Now issued while `Synchronizing` is
 *   ignored, matching the worker's `KEEP` policy.
 * - No valid JWT → the worker fails fast ("no session token; login
 *   required"); the failure surfaces here as `Failed` + Retry.
 * - Blank `baseUrl` (transport unresolved, C-3/R-03; no URL hardcoded per
 *   S5.2) reports via the error region instead of issuing a call.
 * - A completed run carrying step errors (S4.5 `SyncRunResult.errors`) is
 *   `Failed` ("Gagal sinkronisasi.") with Retry; only an error-free run is
 *   `Synchronized`.
 * - No background/scheduled/realtime trigger is exposed (OQ-1): one-shot
 *   enqueue only.
 */
class ReturnOrderSyncViewModel(
    private val session: SessionPreferencesDataSource,
    returnOrderDao: ReturnOrderDao,
    private val connectivityManager: ConnectivityManager?,
    private val baseUrl: String
) : ViewModel() {

    /** Local `DRAFT` orders awaiting submission (device vocabulary). */
    val pendingCount: StateFlow<Int> = returnOrderDao
        .observeByStatus(ReturnOrderEntity.STATUS_DRAFT)
        .map { it.size }
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5_000), 0)

    /** Local `SYNCED` orders (submitted, ADR-RO-006). */
    val syncedCount: StateFlow<Int> = returnOrderDao
        .observeByStatus(ReturnOrderEntity.STATUS_SYNCED)
        .map { it.size }
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5_000), 0)

    /**
     * Latest reference download time (§19.1 `lastRefSync`): the most recent of
     * the Customer / SalesPerson / Driver cache downloads (S4.3 timestamps).
     */
    val lastRefSync: StateFlow<Long> = combine(
        session.lastCustomerSync,
        session.lastSalesPersonSync,
        session.lastDriverSync
    ) { customer, salesPerson, driver ->
        maxOf(customer, salesPerson, driver)
    }.stateIn(viewModelScope, SharingStarted.WhileSubscribed(5_000), 0L)

    private val _syncState = MutableStateFlow(SyncState.IDLE)
    val syncState: StateFlow<SyncState> = _syncState.asStateFlow()

    val isSyncing: StateFlow<Boolean> = syncState
        .map { it == SyncState.SYNCHRONIZING }
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5_000), false)

    private val _error = MutableStateFlow<String?>(null)
    val error: StateFlow<String?> = _error.asStateFlow()

    private val _isOnline = MutableStateFlow(currentlyOnline())
    val isOnline: StateFlow<Boolean> = _isOnline

    private var observeJob: Job? = null

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

    /**
     * Sync Now (§14.3): enqueue one ordered Return Order sync run and observe
     * it to `Synchronized` / `Failed`. Ignored while `Synchronizing` (OQ-1)
     * or offline (IR-M9 — the button is disabled; this is a safety guard).
     */
    fun syncNow(context: Context) {
        if (_syncState.value == SyncState.SYNCHRONIZING) return
        if (!_isOnline.value) return
        if (baseUrl.isBlank()) {
            // C-3: transport unresolved — no environment URL is hardcoded
            // (S5.2); without a configured server no call can be issued.
            _error.value = "Server belum dikonfigurasi. Hubungi administrator."
            _syncState.value = SyncState.FAILED
            return
        }
        viewModelScope.launch {
            _error.value = null
            _syncState.value = SyncState.SYNCHRONIZING
            val serverId = session.officeCode.first().orEmpty()
            val requestId = ReturnOrderSyncWorker.enqueueUnique(
                context.applicationContext,
                baseUrl,
                serverId
            )
            observeRun(context.applicationContext, requestId)
        }
    }

    private fun observeRun(context: Context, requestId: UUID) {
        observeJob?.cancel()
        observeJob = viewModelScope.launch {
            WorkManager.getInstance(context).getWorkInfoByIdFlow(requestId)
                .collect { workInfo ->
                    when (workInfo?.state) {
                        WorkInfo.State.ENQUEUED,
                        WorkInfo.State.RUNNING,
                        WorkInfo.State.BLOCKED -> {
                            _syncState.value = SyncState.SYNCHRONIZING
                        }
                        WorkInfo.State.SUCCEEDED -> {
                            val errors = workInfo.outputData
                                .getString(ReturnOrderSyncWorker.OUTPUT_ERRORS)
                                .orEmpty()
                            if (errors.isBlank()) {
                                _syncState.value = SyncState.SYNCHRONIZED
                            } else {
                                _error.value = "Gagal sinkronisasi. $errors"
                                _syncState.value = SyncState.FAILED
                            }
                            observeJob?.cancel()
                        }
                        WorkInfo.State.FAILED -> {
                            val errors = workInfo.outputData
                                .getString(ReturnOrderSyncWorker.OUTPUT_ERRORS)
                                .orEmpty()
                            _error.value = if (errors.isBlank()) {
                                "Gagal sinkronisasi."
                            } else {
                                "Gagal sinkronisasi. $errors"
                            }
                            _syncState.value = SyncState.FAILED
                            observeJob?.cancel()
                        }
                        WorkInfo.State.CANCELLED,
                        null -> {
                            _syncState.value = SyncState.IDLE
                            observeJob?.cancel()
                        }
                    }
                }
        }
    }

    private fun currentlyOnline(): Boolean {
        val manager = connectivityManager ?: return false
        val network = manager.activeNetwork ?: return false
        val capabilities = manager.getNetworkCapabilities(network) ?: return false
        return capabilities.hasCapability(NetworkCapabilities.NET_CAPABILITY_INTERNET) &&
            capabilities.hasCapability(NetworkCapabilities.NET_CAPABILITY_VALIDATED)
    }
}
