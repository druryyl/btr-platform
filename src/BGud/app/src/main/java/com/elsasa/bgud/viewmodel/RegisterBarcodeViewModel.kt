package com.elsasa.bgud.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.elsasa.bgud.dao.BarcodeDao
import com.elsasa.bgud.dao.BarangDao
import com.elsasa.bgud.dao.BarcodeRegistrationRequestDao
import com.elsasa.bgud.datastore.SessionPreferencesDataSource
import com.elsasa.bgud.model.BarcodeEntity
import com.elsasa.bgud.model.BarangEntity
import com.elsasa.bgud.model.BarcodeRegistrationRequestEntity
import com.elsasa.bgud.util.BarcodeNormalization
import kotlinx.coroutines.Job
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.launch
import java.util.UUID

/**
 * Register Barcode view model (SCR-MOB-004, Architecture §12.6, §14.3, §19.3).
 *
 * Key fields: `barcodeValue`, `itemQuery`, `itemResults`, `selectedItem`,
 * `selectedUnit`, `saveState` (`isSaving` / `saveError` / `saved`).
 * Sources: Room `barang_entity` (Item search) + `barcode_entity` (local
 * duplicate detection) + `barcode_registration_request_entity` (local queue).
 *
 * State machine (§14.3):
 *
 * ```text
 * Initial             (barcode captured, read-only)
 *   ↓ Item search text entered
 * Searching           (local Barang cache)
 *   ├─ results ──▶ Results
 *   └─ none ─────▶ Empty
 * Results
 *   ↓ Item selected
 * Item Selected
 *   ↓ (optional) Unit selected
 * Ready To Save
 *   ↓ Save
 * Saving              (local write only)
 *   └─ success ──▶ Saved (success message) ──▶ back
 * ```
 *
 * Rules (no new decisions):
 * - Barcode is captured and read-only; it is normalized once (VO-01 local
 *   form) and never edited here (UX Blueprint §8).
 * - Item search is local Room cache only (by Item Code or Item Name).
 * - `Save` is enabled only when Barcode is present and an **Active** cached
 *   Item is selected (BQ-7); Unit is optional and never gates `Save`
 *   (BR-005); `Saving` performs a local write only — no network (P-08).
 * - A selected Item that is not Active surfaces "Item sudah tidak aktif."
 *   (IR-M3, UX Blueprint §14); a barcode already in the local cache
 *   surfaces "Barcode sudah terdaftar." (IR-M4) and disables `Save`.
 */
class RegisterBarcodeViewModel(
    private val barcodeDao: BarcodeDao,
    private val barangDao: BarangDao,
    private val requestDao: BarcodeRegistrationRequestDao,
    private val session: SessionPreferencesDataSource,
    initialBarcode: String
) : ViewModel() {

    /** Captured barcode, read-only (normalized display form, VO-01). */
    private val _barcodeValue = MutableStateFlow(BarcodeNormalization.normalize(initialBarcode))
    val barcodeValue: StateFlow<String> = _barcodeValue.asStateFlow()

    private val _itemQuery = MutableStateFlow("")
    val itemQuery: StateFlow<String> = _itemQuery.asStateFlow()

    private val _itemResults = MutableStateFlow<List<BarangEntity>>(emptyList())
    val itemResults: StateFlow<List<BarangEntity>> = _itemResults.asStateFlow()

    private val _isSearching = MutableStateFlow(false)
    val isSearching: StateFlow<Boolean> = _isSearching.asStateFlow()

    private val _selectedItem = MutableStateFlow<BarangEntity?>(null)
    val selectedItem: StateFlow<BarangEntity?> = _selectedItem.asStateFlow()

    /** `""` means no packaging level recorded (BR-005). */
    private val _selectedUnit = MutableStateFlow("")
    val selectedUnit: StateFlow<String> = _selectedUnit.asStateFlow()

    /**
     * Local duplicate detection (IR-M4, best-effort; the Main Office remains
     * authoritative, §18.4). Non-null when the captured barcode already
     * exists in the local Active cache.
     */
    private val _duplicateFound = MutableStateFlow<BarcodeEntity?>(null)
    val duplicateFound: StateFlow<BarcodeEntity?> = _duplicateFound.asStateFlow()

    private val _isSaving = MutableStateFlow(false)
    val isSaving: StateFlow<Boolean> = _isSaving.asStateFlow()

    private val _saveError = MutableStateFlow<String?>(null)
    val saveError: StateFlow<String?> = _saveError.asStateFlow()

    private val _saved = MutableStateFlow(false)
    val saved: StateFlow<Boolean> = _saved.asStateFlow()

    private var searchJob: Job? = null

    init {
        // Local duplicate probe against the Active cache (IR-M4).
        val captured = _barcodeValue.value
        if (captured.isNotBlank()) {
            viewModelScope.launch {
                _duplicateFound.value =
                    barcodeDao.getByKey(BarcodeNormalization.toKey(captured))
            }
        }
    }

    /**
     * Unit options for the selected Item: its own cached small/big units
     * (IR-01); empty when nothing is selected. Unit is optional (BR-005).
     */
    fun unitOptions(): List<String> {
        val item = _selectedItem.value ?: return emptyList()
        return listOf(item.satKecil, item.satBesar)
            .filter { it.isNotBlank() }
            .distinct()
    }

    /** Item search text changed — debounced local cache lookup. */
    fun onQueryChange(value: String) {
        _itemQuery.value = value
        // A new search invalidates the previous selection (the user is
        // choosing anew); Unit resets with it (BR-005: optional).
        _selectedItem.value = null
        _selectedUnit.value = ""
        searchJob?.cancel()
        val query = value.trim()
        if (query.isEmpty()) {
            _itemResults.value = emptyList()
            _isSearching.value = false
            return
        }
        _isSearching.value = true
        searchJob = viewModelScope.launch {
            delay(300)
            try {
                _itemResults.value = barangDao.search(query)
            } catch (e: Exception) {
                _itemResults.value = emptyList()
            } finally {
                _isSearching.value = false
            }
        }
    }

    /** Item selected from the local result list. */
    fun onSelectItem(item: BarangEntity) {
        _selectedItem.value = item
        _selectedUnit.value = ""
        _saveError.value = if (!item.isAktif) "Item sudah tidak aktif." else null
    }

    /** Selection cleared by the user. */
    fun onClearSelection() {
        _selectedItem.value = null
        _selectedUnit.value = ""
        _saveError.value = null
    }

    /** Optional Unit selected; `""` clears back to "no packaging level". */
    fun onUnitChange(unit: String) {
        _selectedUnit.value = unit
    }

    /**
     * Save the registration request to the local queue (`PENDING`).
     *
     * Local write only; no network is required (P-08, UX Blueprint §8 "No
     * immediate server communication is required"). The row becomes
     * authoritative only when the Main Office accepts it during
     * synchronization (§17.2, later `POST /api/barcode-registration`, I-04
     * via the S5.3 sync worker).
     */
    fun save() {
        if (_isSaving.value || _saved.value) return
        viewModelScope.launch {
            _saveError.value = null
            val barcode = _barcodeValue.value
            if (barcode.isBlank()) {
                _saveError.value = "Barcode wajib diisi."
                return@launch
            }
            val selected = _selectedItem.value
            if (selected == null) {
                _saveError.value = "Item wajib dipilih."
                return@launch
            }
            // Re-validate cached Active state at save time (BQ-7): the
            // reference cache may have been replaced since selection.
            val fresh = try {
                barangDao.getById(selected.brgId)
            } catch (e: Exception) {
                null
            }
            val effective = fresh ?: selected
            if (!effective.isAktif) {
                _selectedItem.value = effective
                _saveError.value = "Item sudah tidak aktif."
                return@launch
            }
            // Re-probe local duplicates at save time (IR-M4).
            val duplicate = try {
                barcodeDao.getByKey(BarcodeNormalization.toKey(barcode))
            } catch (e: Exception) {
                null
            }
            if (duplicate != null) {
                _duplicateFound.value = duplicate
                _saveError.value = "Barcode sudah terdaftar."
                return@launch
            }
            _isSaving.value = true
            try {
                val warehouseCode = try {
                    session.locationId.first().orEmpty()
                } catch (e: Exception) {
                    ""
                }
                requestDao.enqueue(
                    BarcodeRegistrationRequestEntity(
                        clientRequestId = UUID.randomUUID().toString(),
                        barcodeValue = barcode,
                        brgId = effective.brgId,
                        satuan = _selectedUnit.value,
                        status = BarcodeRegistrationRequestEntity.STATUS_PENDING,
                        warehouseCode = warehouseCode,
                        createdAt = System.currentTimeMillis()
                    )
                )
                _saved.value = true
            } catch (e: Exception) {
                _saveError.value = "Gagal menyimpan: ${e.message?.take(200) ?: "kesalahan tidak diketahui."}"
            } finally {
                _isSaving.value = false
            }
        }
    }
}
