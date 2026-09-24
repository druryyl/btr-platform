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
import com.elsasa.bgud.dao.BarcodeRegistrationRequestDao
import com.elsasa.bgud.datastore.SessionPreferencesDataSource
import com.elsasa.bgud.sync.BarcodeSyncWorker
import kotlinx.coroutines.Job
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.flow.map
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.launch
import java.util.UUID

/**
 * Sync UI state (SCR-MOB-007, Architecture §14.5).
 *
 * ```text
 * Idle
 *   ↓ Sync Now
 * Synchronizing
 *   ├─ success ──▶ Synchronized (timestamps + queue counts refreshed)
 *   └─ failure ──▶ Failed ("Gagal sinkronisasi." + Retry)
 * ```
 */
enum class SyncState {
    IDLE,
    SYNCHRONIZING,
    SYNCHRONIZED,
    FAILED
}

/**
 * Synchronization view model (SCR-MOB-007, Architecture §12.9, §14.5, §19.3).
 *
 * Key fields: `lastBarangSyncAt`, `lastBarcodeSyncAt`, `pendingCount`,
 * `successCount`, `rejectedCount`, `isSyncing`, `error`. Sources: DataStore +
 * Room only — the screen issues no network call itself. `Sync Now` enqueues
 * the one-shot [BarcodeSyncWorker] (S5.3: submit I-04 → download I-05 →
 * download I-06 → refresh I-09 statuses; §20 WorkManager usage, no periodic
 * work) and observes its `WorkInfo`; the refreshed timestamps and queue
 * counts arrive automatically through the DataStore/Room flows.
 *
 * Rules (no new decisions):
 * - Offline disables Sync Now (IR-M5, §14.6 `Online | Offline`); scan,
 *   register, and edit remain available offline (UX-001).
 * - One sync run at a time (IR-M6): a Sync Now issued while
 *   `Synchronizing` is ignored, matching the worker's `KEEP` policy.
 * - No valid local session (`google_email` absent) → the worker fails fast
 *   ("no session; login required"); the failure surfaces here as `Failed` +
 *   Retry. A Cloud 409 clears the session (TD-13) and the Navigation gate
 *   returns the operator to sign-in.
 * - Blank `baseUrl` (transport unresolved, C-3/R-03; no URL hardcoded per
 *   S5.2) reports via the error region instead of issuing a call (S5.4
 *   precedent).
 * - A completed run carrying step errors (S5.3 `SyncRunResult.errors`) is
 *   `Failed` ("Gagal sinkronisasi.", UX §14) with Retry; only an
 *   error-free run is `Synchronized`.
 */
class SynchronizationViewModel(
    private val session: SessionPreferencesDataSource,
    requestDao: BarcodeRegistrationRequestDao,
    private val connectivityManager: ConnectivityManager?,
    private val baseUrl: String
) : ViewModel() {

    val lastBarangSyncAt: StateFlow<Long> = session.lastBarangSync
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5_000), 0L)

    val lastBarcodeSyncAt: StateFlow<Long> = session.lastBarcodeSync
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5_000), 0L)

    val pendingCount: StateFlow<Int> = requestDao.pendingCount()
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5_000), 0)

    val successCount: StateFlow<Int> = requestDao.syncedCount()
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5_000), 0)

    val rejectedCount: StateFlow<Int> = requestDao.rejectedCount()
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5_000), 0)

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
     * Sync Now (§13.2, §17.2): enqueue one ordered sync run and observe it
     * to `Synchronized` / `Failed`. Ignored while `Synchronizing` (IR-M6)
     * or offline (IR-M5 — the button is disabled; this is a safety guard).
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
            val serverId = session.serverId.first().orEmpty()
            val requestId = BarcodeSyncWorker.enqueueUnique(
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
                                .getString(BarcodeSyncWorker.OUTPUT_ERRORS)
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
                                .getString(BarcodeSyncWorker.OUTPUT_ERRORS)
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
