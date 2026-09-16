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
import kotlinx.coroutines.Job
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.launch
import java.util.UUID

/**
 * Edit Barcode view model (SCR-MOB-006, Architecture §12.8, §14.4, §19.3).
 *
 * Key fields: `barcodeValue` (read-only), `selectedItem`, `selectedUnit`,
 * `saveState` (`isSaving` / `saveError` / `saved`). Sources: Room
 * `barcode_entity` (correction target, loaded by `brgBarcodeId` from the
 * §13.2 `edit?barcodeId={id}` route) + `barang_entity` (local Item search)
 * + `barcode_registration_request_entity` (local queue).
 *
 * State machine (§14.4):
 *
 * ```text
 * Loaded              (cached mapping; barcode read-only)
 *   ↓ change Item or Unit
 * Dirty
 *   ↓ Save
 * Saving              (local write only)
 *   └─ success ──▶ Saved ──▶ back
 * ```
 *
 * Rules (no new decisions):
 * - Barcode is never editable (UX Blueprint §10); only `BrgId` and/or
 *   `Satuan` change (INV-07); activation state is never touched — no
 *   activation/deactivation surface exists on mobile (IR-06, C-1).
 * - Item search is local Room cache only (by Item Code or Item Name).
 * - `Save` is enabled only when the selection differs from the loaded
 *   mapping (`Dirty`) and the selected Item is **Active** (BQ-7); Unit is
 *   optional and never gates `Save` (BR-005); `Saving` performs a local
 *   write only — no network (P-08, §17.2 "Save correction | local update;
 *   later `POST /api/barcode-registration` (correction carries the barcode
 *   identity)").
 * - A selected Item that is not Active surfaces "Item sudah tidak aktif."
 *   (IR-M3, UX Blueprint §14).
 * - No local duplicate probe: the barcode identity itself already exists
 *   in the cache (it is the correction target); uniqueness remains
 *   Main Office–authoritative (§18.1, §18.4).
 */
class EditBarcodeViewModel(
    private val barcodeDao: BarcodeDao,
    private val barangDao: BarangDao,
    private val requestDao: BarcodeRegistrationRequestDao,
    private val session: SessionPreferencesDataSource,
    private val barcodeId: String
) : ViewModel() {

    private val _isLoading = MutableStateFlow(true)
    val isLoading: StateFlow<Boolean> = _isLoading.asStateFlow()

    /**
     * The loaded cached mapping, or null when still loading / not found.
     * The barcode identity for the correction is carried from this row
     * (§17.2).
     */
    private val _original = MutableStateFlow<BarcodeEntity?>(null)
    val original: StateFlow<BarcodeEntity?> = _original.asStateFlow()

    /** True when [barcodeId] matches no cached row. */
    private val _notFound = MutableStateFlow(false)
    val notFound: StateFlow<Boolean> = _notFound.asStateFlow()

    /** Read-only barcode display; never editable (UX Blueprint §10). */
    private val _barcodeValue = MutableStateFlow("")
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

    /** True when the selection differs from the loaded mapping (§14.4). */
    private val _isDirty = MutableStateFlow(false)
    val isDirty: StateFlow<Boolean> = _isDirty.asStateFlow()

    private val _isSaving = MutableStateFlow(false)
    val isSaving: StateFlow<Boolean> = _isSaving.asStateFlow()

    private val _saveError = MutableStateFlow<String?>(null)
    val saveError: StateFlow<String?> = _saveError.asStateFlow()

    private val _saved = MutableStateFlow(false)
    val saved: StateFlow<Boolean> = _saved.asStateFlow()

    private var searchJob: Job? = null

    init {
        viewModelScope.launch {
            try {
                val loaded = if (barcodeId.isBlank()) {
                    null
                } else {
                    barcodeDao.getById(barcodeId)
                }
                if (loaded == null) {
                    _notFound.value = true
                } else {
                    _original.value = loaded
                    _barcodeValue.value = loaded.barcodeValue
                    _selectedUnit.value = loaded.satuan
                    // Prefill the current Item from the reference cache so
                    // the loaded mapping is visible as the selection; when
                    // the cache no longer holds it, the user searches anew.
                    val currentItem = try {
                        barangDao.getById(loaded.brgId)
                    } catch (e: Exception) {
                        null
                    }
                    _selectedItem.value = currentItem
                    refreshDirty()
                }
            } catch (e: Exception) {
                _notFound.value = true
            } finally {
                _isLoading.value = false
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
        refreshDirty()
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
        refreshDirty()
    }

    /** Selection cleared by the user. */
    fun onClearSelection() {
        _selectedItem.value = null
        _selectedUnit.value = ""
        _saveError.value = null
        refreshDirty()
    }

    /** Optional Unit selected; `""` clears back to "no packaging level". */
    fun onUnitChange(unit: String) {
        _selectedUnit.value = unit
        refreshDirty()
    }

    private fun refreshDirty() {
        val current = _original.value
        val selected = _selectedItem.value
        _isDirty.value = current != null &&
            selected != null &&
            (selected.brgId != current.brgId || _selectedUnit.value != current.satuan)
    }

    /**
     * Save the correction to the local queue (`PENDING`).
     *
     * Local write only; no network is required (P-08, §14.4, §17.2). The
     * row carries the barcode identity and becomes authoritative only when
     * the Main Office accepts it during synchronization (submitted later
     * via I-04 by the S5.3 sync worker).
     */
    fun save() {
        if (_isSaving.value || _saved.value) return
        viewModelScope.launch {
            _saveError.value = null
            val current = _original.value
            if (current == null) {
                _saveError.value = "Data tidak ditemukan."
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
            if (effective.brgId == current.brgId && _selectedUnit.value == current.satuan) {
                _saveError.value = "Tidak ada perubahan."
                return@launch
            }
            _isSaving.value = true
            try {
                val warehouseCode = try {
                    session.warehouseCode.first().orEmpty()
                } catch (e: Exception) {
                    ""
                }
                requestDao.enqueue(
                    BarcodeRegistrationRequestEntity(
                        clientRequestId = UUID.randomUUID().toString(),
                        barcodeValue = current.barcodeValue,
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
