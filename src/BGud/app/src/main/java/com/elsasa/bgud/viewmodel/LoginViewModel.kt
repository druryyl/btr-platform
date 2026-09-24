package com.elsasa.bgud.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.elsasa.bgud.database.AppDatabase
import com.elsasa.bgud.datastore.SessionPreferencesDataSource
import com.elsasa.bgud.network.ApiClient
import com.elsasa.bgud.network.SessionContextInterceptor
import com.elsasa.bgud.repository.BarcodeSyncRepository
import com.elsasa.bgud.repository.ResolvedSessionAccount
import com.elsasa.bgud.repository.ReturnOrderReferenceSyncRepository
import com.elsasa.bgud.repository.ReturnOrderSyncRepository
import com.elsasa.bgud.repository.SessionResolveOutcome
import com.elsasa.bgud.repository.SessionResolverRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import java.util.concurrent.atomic.AtomicBoolean

/**
 * Warehouse (Gudang) selector option.
 *
 * `displayName` is shown in the selector; `locationId` is sent as
 * `X-Session-Location` and `serverId` is the Cloud-resolved tenant value used
 * only for the legacy `{serverId}` read routes (TD-08). Options come from the
 * TD-02 resolution response, so the device never invents the mapping.
 */
data class WarehouseOption(
    val displayName: String,
    val locationId: String,
    val serverId: String
)

/**
 * Login view model (SCR-MOB-001).
 *
 * Flow (FEATURE §6): Google Sign-In yields the account email → the email is
 * resolved through [SessionResolverRepository] (`POST api/session/resolve`,
 * TD-02) → the operator selects the Gudang → the local session is persisted
 * (TD-10) → login-time synchronization runs under the session context (TD-09)
 * → [establishSession]'s caller navigates to Home.
 *
 * - An unmapped/invalid account (HTTP 400) is refused with an administrator
 *   message and creates no session; a cancelled/failed sign-in leaves the
 *   operator signed out.
 * - No password, JWT, or `Authorization` header is used anywhere (ARCHITECTURE
 *   §10). The resolution call carries only the body email (TD-12).
 * - Login-time synchronization runs with the session-context client (no
 *   bearer). Barcode sync remains structurally blocking and Return Order sync
 *   failure never blocks navigation (TD-09, current semantics).
 * - A Cloud 409 during login-time sync means the session ended (TD-13): the
 *   local session is cleared and the operator stays on sign-in.
 */
class LoginViewModel(
    private val session: SessionPreferencesDataSource,
    private val database: AppDatabase,
    private val baseUrl: String
) : ViewModel() {

    private val _resolvedAccount = MutableStateFlow<ResolvedSessionAccount?>(null)
    val resolvedAccount: StateFlow<ResolvedSessionAccount?> = _resolvedAccount.asStateFlow()

    private val _warehouseOptions = MutableStateFlow<List<WarehouseOption>>(emptyList())
    val warehouseOptions: StateFlow<List<WarehouseOption>> = _warehouseOptions.asStateFlow()

    private val _selectedWarehouse = MutableStateFlow<WarehouseOption?>(null)
    val selectedWarehouse: StateFlow<WarehouseOption?> = _selectedWarehouse.asStateFlow()

    private val _isLoading = MutableStateFlow(false)
    val isLoading: StateFlow<Boolean> = _isLoading.asStateFlow()

    private val _isSyncing = MutableStateFlow(false)
    val isSyncing: StateFlow<Boolean> = _isSyncing.asStateFlow()

    private val _error = MutableStateFlow<String?>(null)
    val error: StateFlow<String?> = _error.asStateFlow()

    /**
     * Google Sign-In succeeded on-device: resolve the email through
     * `POST api/session/resolve` (TD-02). On success the Gudang selector is
     * populated from the returned mapping; no session is created yet.
     */
    fun onGoogleAccountSelected(email: String) {
        if (_isLoading.value) return
        viewModelScope.launch {
            _error.value = null
            _resolvedAccount.value = null
            _warehouseOptions.value = emptyList()
            _selectedWarehouse.value = null

            if (baseUrl.isBlank()) {
                // C-3: transport unresolved — no environment URL is hardcoded.
                _error.value = "Server belum dikonfigurasi. Hubungi administrator."
                return@launch
            }

            _isLoading.value = true
            try {
                // The resolution call carries only the body email (TD-12): no
                // session context is attached.
                val anonymousApi = ApiClient.create(baseUrl = normalizedBaseUrl(baseUrl))
                when (val outcome = SessionResolverRepository(anonymousApi).resolve(email)) {
                    is SessionResolveOutcome.Resolved -> {
                        val account = outcome.account
                        val options = account.warehouses.map { warehouse ->
                            WarehouseOption(
                                displayName = warehouseDisplayName(warehouse.locationId),
                                locationId = warehouse.locationId,
                                serverId = warehouse.serverId
                            )
                        }
                        _resolvedAccount.value = account
                        _warehouseOptions.value = options
                        _selectedWarehouse.value = options.firstOrNull()
                        if (options.isEmpty()) {
                            _error.value =
                                "Gudang tidak tersedia untuk akun ini. Hubungi administrator."
                        }
                    }

                    is SessionResolveOutcome.Refused -> _error.value = outcome.message
                    is SessionResolveOutcome.Failed -> _error.value = outcome.message
                }
            } catch (e: Exception) {
                _error.value =
                    "Login gagal: ${e.message?.take(200) ?: "kesalahan tidak diketahui."}"
            } finally {
                _isLoading.value = false
            }
        }
    }

    /** Google Sign-In cancelled or failed — remain signed out. */
    fun onSignInCancelled() {
        _error.value = null
        _resolvedAccount.value = null
        _warehouseOptions.value = emptyList()
        _selectedWarehouse.value = null
    }

    fun onWarehouseChange(option: WarehouseOption) {
        _selectedWarehouse.value = option
    }

    /**
     * Establish the local session for the resolved account and selected Gudang
     * (TD-10), run login-time synchronization under the session context
     * (TD-09), and report success so the caller navigates to Home.
     */
    fun establishSession(onSuccess: () -> Unit) {
        if (_isLoading.value) return
        viewModelScope.launch {
            _error.value = null
            val account = _resolvedAccount.value
            val warehouse = _selectedWarehouse.value
            if (account == null || warehouse == null) {
                _error.value = "Masuk dengan Google dan pilih gudang terlebih dahulu."
                return@launch
            }
            if (baseUrl.isBlank()) {
                _error.value = "Server belum dikonfigurasi. Hubungi administrator."
                return@launch
            }

            _isLoading.value = true
            val sessionEnded = AtomicBoolean(false)
            try {
                session.saveSession(
                    googleEmail = account.email,
                    userId = account.userId,
                    userName = account.userName,
                    roleId = account.roleId,
                    locationId = warehouse.locationId,
                    serverId = warehouse.serverId
                )

                val api = ApiClient.create(
                    baseUrl = normalizedBaseUrl(baseUrl),
                    sessionContextProvider = {
                        SessionContextInterceptor.SessionContext(
                            locationId = warehouse.locationId,
                            actorEmail = account.email
                        )
                    },
                    onSessionInvalidated = { sessionEnded.set(true) }
                )

                // Login-time master data synchronization (TD-09): barcode
                // registry run, then the Return Order run. No DataStore I/O on
                // the OkHttp dispatcher (fixed provider).
                _isSyncing.value = true
                try {
                    val repository = BarcodeSyncRepository(
                        api = api,
                        barcodeDao = database.barcodeDao(),
                        barangDao = database.barangDao(),
                        requestDao = database.barcodeRegistrationRequestDao(),
                        session = session
                    )
                    repository.sync(warehouse.serverId)
                    try {
                        val returnOrderSync = ReturnOrderSyncRepository(
                            api = api,
                            returnOrderDao = database.returnOrderDao(),
                            returnOrderItemDao = database.returnOrderItemDao(),
                            referenceSync = ReturnOrderReferenceSyncRepository(
                                api = api,
                                customerDao = database.customerDao(),
                                salesPersonDao = database.salesPersonDao(),
                                driverDao = database.driverDao(),
                                session = session
                            )
                        )
                        returnOrderSync.sync(warehouse.serverId)
                    } catch (_: Exception) {
                        // Return Order sync failures never block navigation.
                    }
                } finally {
                    _isSyncing.value = false
                }

                if (sessionEnded.get()) {
                    // TD-13 — the actor mapping was removed; the session ended.
                    session.clearSession()
                    _error.value = "Sesi berakhir. Silakan masuk kembali."
                    return@launch
                }

                onSuccess()
            } catch (e: Exception) {
                _error.value =
                    "Login gagal: ${e.message?.take(200) ?: "kesalahan tidak diketahui."}"
            } finally {
                _isLoading.value = false
                _isSyncing.value = false
            }
        }
    }

    private fun normalizedBaseUrl(value: String): String =
        if (value.endsWith("/")) value else "$value/"

    private fun warehouseDisplayName(locationId: String): String = when (locationId.uppercase()) {
        "GAMPING" -> "Gudang Gamping"
        "CONCAT" -> "Gudang Concat"
        "MAGELANG" -> "Gudang Magelang"
        else -> locationId
    }
}
