package com.elsasa.bgud.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.elsasa.bgud.database.AppDatabase
import com.elsasa.bgud.datastore.SessionPreferencesDataSource
import com.elsasa.bgud.model.api.LoginRequest
import com.elsasa.bgud.network.ApiClient
import com.elsasa.bgud.repository.BarcodeSyncRepository
import com.elsasa.bgud.repository.ReturnOrderReferenceSyncRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import retrofit2.HttpException
import java.io.IOException

/**
 * Warehouse selector option (SCR-MOB-001).
 *
 * `displayName` is shown in the selector (UX Blueprint §5); `locationId` is
 * sent as `LocationId` on I-07. Values mirror the `BTRADE_Location` seed
 * (Architecture §6.3): GAMPING/CONCAT → JOGJA, MAGELANG → MGL. The
 * `ServerId` (office) resolution stays server-side (ADR-007).
 */
data class WarehouseOption(
    val displayName: String,
    val locationId: String
)

/**
 * Login view model (SCR-MOB-001, Architecture §19.3).
 *
 * Key fields: `username`, `password`, `warehouse`, `isLoading`, `error`.
 * `isSyncing` reports the login-time master data sync phase (§13.2).
 *
 * Flow (§13.2, UX §5): I-07 login → office resolution (login-returned
 * `serverId`) → master data synchronization ([BarcodeSyncRepository.sync],
 * S5.3) → [onSuccess] (caller navigates to home). Any failure — login or
 * sync — lands in the error region and does not navigate.
 *
 * Session binding (IR-09): `warehouseCode` (the selected `locationId`) is
 * stored, never a `ServerId`. The login-returned `serverId` is kept as
 * `officeCode` for display and for the legacy I-06 read route only (§8.4,
 * ADR-007 §8). IR-M8 is enforced by the navigation start-destination gate
 * (`ui/Navigation.kt`) together with the token checks in S5.3.
 *
 * The password is sent raw on I-07; SHA-256 verification runs server-side
 * (`IssueTokenCommand`, IR-05). No `ServerId` is ever sent (ADR-007, P-06).
 */
class LoginViewModel(
    private val session: SessionPreferencesDataSource,
    private val database: AppDatabase,
    private val baseUrl: String
) : ViewModel() {

    val warehouseOptions: List<WarehouseOption> = listOf(
        WarehouseOption("Gudang Gamping", "GAMPING"),
        WarehouseOption("Gudang Concat", "CONCAT"),
        WarehouseOption("Gudang Magelang", "MAGELANG")
    )

    private val _username = MutableStateFlow("")
    val username: StateFlow<String> = _username.asStateFlow()

    private val _password = MutableStateFlow("")
    val password: StateFlow<String> = _password.asStateFlow()

    private val _selectedWarehouse = MutableStateFlow(warehouseOptions.first())
    val selectedWarehouse: StateFlow<WarehouseOption> = _selectedWarehouse.asStateFlow()

    private val _isLoading = MutableStateFlow(false)
    val isLoading: StateFlow<Boolean> = _isLoading.asStateFlow()

    private val _isSyncing = MutableStateFlow(false)
    val isSyncing: StateFlow<Boolean> = _isSyncing.asStateFlow()

    private val _error = MutableStateFlow<String?>(null)
    val error: StateFlow<String?> = _error.asStateFlow()

    fun onUsernameChange(value: String) {
        _username.value = value
    }

    fun onPasswordChange(value: String) {
        _password.value = value
    }

    fun onWarehouseChange(option: WarehouseOption) {
        _selectedWarehouse.value = option
    }

    fun login(onSuccess: () -> Unit) {
        if (_isLoading.value) return
        viewModelScope.launch {
            _error.value = null
            val userId = _username.value.trim()
            val locationId = _selectedWarehouse.value.locationId
            if (userId.isBlank() || _password.value.isBlank()) {
                _error.value = "Username dan password wajib diisi."
                return@launch
            }
            if (baseUrl.isBlank()) {
                // C-3: transport unresolved — no environment URL is hardcoded
                // (S5.2); without a configured server no call can be issued.
                _error.value = "Server belum dikonfigurasi. Hubungi administrator."
                return@launch
            }
            _isLoading.value = true
            try {
                // I-07 is AllowAnonymous: no JWT exists yet, so no bearer is
                // attached (AuthInterceptor passes the request through).
                val anonymousApi = ApiClient.create(
                    baseUrl = normalizedBaseUrl(baseUrl),
                    tokenProvider = { null }
                )
                val envelope = anonymousApi.login(
                    LoginRequest(
                        userId = userId,
                        password = _password.value,
                        locationId = locationId
                    )
                )
                val result = envelope.data
                if (result == null || result.token.isBlank()) {
                    _error.value = "Username, password, atau gudang tidak valid."
                    return@launch
                }
                session.saveSession(
                    token = result.token,
                    userId = result.userId.ifBlank { userId },
                    warehouseCode = result.locationId.ifBlank { locationId },
                    officeCode = result.serverId
                )
                // Login-time master data synchronization (§13.2, OQ-2,
                // UX §13): submit → barcodes → barang → statuses (S5.3),
                // then Return Order references: customer → salesperson →
                // driver (S4.3, I-RO-03/04/05). Authenticated with the
                // just-issued token; no DataStore I/O on the OkHttp
                // dispatcher (fixed provider, S5.3 pattern). Reference
                // failures never block navigation (S4.3 acceptance).
                _isSyncing.value = true
                try {
                    val authedApi = ApiClient.create(
                        baseUrl = normalizedBaseUrl(baseUrl),
                        tokenProvider = { result.token }
                    )
                    val repository = BarcodeSyncRepository(
                        api = authedApi,
                        barcodeDao = database.barcodeDao(),
                        barangDao = database.barangDao(),
                        requestDao = database.barcodeRegistrationRequestDao(),
                        session = session
                    )
                    repository.sync(result.serverId)
                    try {
                        val referenceRepository = ReturnOrderReferenceSyncRepository(
                            api = authedApi,
                            customerDao = database.customerDao(),
                            salesPersonDao = database.salesPersonDao(),
                            driverDao = database.driverDao(),
                            session = session
                        )
                        referenceRepository.sync(result.serverId)
                    } catch (_: Exception) {
                        // Reference download failures never block navigation.
                    }
                } finally {
                    _isSyncing.value = false
                }
                onSuccess()
            } catch (e: HttpException) {
                _error.value = if (e.code() == 401 || e.code() == 403) {
                    "Username, password, atau gudang tidak valid."
                } else {
                    "Login gagal (kode ${e.code()})."
                }
            } catch (e: IOException) {
                _error.value = "Tidak dapat terhubung ke server. Periksa koneksi."
            } catch (e: Exception) {
                _error.value = "Login gagal: ${e.message?.take(200) ?: "kesalahan tidak diketahui."}"
            } finally {
                _isLoading.value = false
                _isSyncing.value = false
            }
        }
    }

    private fun normalizedBaseUrl(value: String): String =
        if (value.endsWith("/")) value else "$value/"
}
