package com.elsasa.bgud.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.elsasa.bgud.dao.BarangDao
import com.elsasa.bgud.dao.BarcodeDao
import com.elsasa.bgud.dao.CustomerDao
import com.elsasa.bgud.dao.DriverDao
import com.elsasa.bgud.dao.SalesPersonDao
import com.elsasa.bgud.model.BarangEntity
import com.elsasa.bgud.model.CustomerEntity
import com.elsasa.bgud.model.DriverEntity
import com.elsasa.bgud.model.ReturnOrderEntity
import com.elsasa.bgud.model.ReturnOrderItemEntity
import com.elsasa.bgud.model.SalesPersonEntity
import com.elsasa.bgud.repository.ReturnOrderCaptureRepository
import com.elsasa.bgud.repository.ReturnOrderCaptureResult
import com.elsasa.bgud.repository.ReturnOrderDraft
import com.elsasa.bgud.repository.ReturnOrderDraftItem
import com.elsasa.bgud.util.BarcodeNormalization
import kotlinx.coroutines.Job
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

/**
 * Edit Return Order view model (SCR-MOB-RO-004, Architecture §11.1, §12.4,
 * §14.2, §19.1, §20).
 *
 * Maintenance mode for a `Draft` order only (§15.1): the editable fields
 * (Customer, optional Salesman/Driver, Notes, item lines) are loaded from the
 * local Room capture store and written back locally through
 * [ReturnOrderCaptureRepository.updateDraft] — no network call (§17.1, P-07).
 *
 * Rules (no new decisions):
 * - Entry is rejected unless the stored order is `DRAFT` (BR-018): a `SYNCED`
 *   order can never enter edit (`notEditable`), and the UI never offers edit
 *   for `SYNCED` (the Detail action is gated by `ReturnOrderDetailViewModel`).
 * - Warehouse is the stored, session-bound code and is never editable — a
 *   draft is never re-homed (BR-005/006, IR-RO-04).
 * - Customer is mandatory and selected from the cache (BR-001/002, GAP-004);
 *   Salesman/Driver are optional (BR-013–016, ADR-RO-005).
 * - At least one valid item line (BR-012, IR-M2); `Qty > 0` (BR-010, IR-M4);
 *   unit from the item's cached unit data recorded as `SatId` (BR-011,
 *   IR-M5, ADR-RO-003); `JenisRetur` exactly `BAGUS`/`RUSAK` (ADR-RO-004,
 *   IR-M6); mixed return types are allowed per line.
 * - `Save` is enabled only after a change (§14.2 `Dirty`) with a Customer and
 *   at least one line; it writes locally only and the order remains `DRAFT`
 *   (BR-017/018, BG-003, P-06/P-07). No `ReturnOrderNo` is authored
 *   (ADR-RO-002) and no `ServerId` is stored or sent (P-06).
 */
class EditReturnOrderViewModel(
    private val captureRepository: ReturnOrderCaptureRepository,
    private val customerDao: CustomerDao,
    private val salesPersonDao: SalesPersonDao,
    private val driverDao: DriverDao,
    private val barangDao: BarangDao,
    private val barcodeDao: BarcodeDao,
    private val returnOrderId: String
) : ViewModel() {

    companion object {
        /** Local reference search debounce, mirroring the existing pattern. */
        const val SEARCH_DEBOUNCE_MILLIS = 300L
    }

    private val _isLoading = MutableStateFlow(true)
    val isLoading: StateFlow<Boolean> = _isLoading.asStateFlow()

    /** True when [returnOrderId] matches no local row. */
    private val _notFound = MutableStateFlow(false)
    val notFound: StateFlow<Boolean> = _notFound.asStateFlow()

    /**
     * True when the stored order is not `DRAFT` (BR-018): edit entry is
     * rejected and no field is offered.
     */
    private val _notEditable = MutableStateFlow(false)
    val notEditable: StateFlow<Boolean> = _notEditable.asStateFlow()

    /** Stored, session-bound warehouse — read-only (BR-005/006, IR-RO-04). */
    private val _warehouseCode = MutableStateFlow("")
    val warehouseCode: StateFlow<String> = _warehouseCode.asStateFlow()

    // --- Header: Customer (mandatory, local cache) ---------------------------

    private val _customer = MutableStateFlow<CustomerEntity?>(null)
    val customer: StateFlow<CustomerEntity?> = _customer.asStateFlow()

    private val _customerQuery = MutableStateFlow("")
    val customerQuery: StateFlow<String> = _customerQuery.asStateFlow()

    private val _customerResults = MutableStateFlow<List<CustomerEntity>>(emptyList())
    val customerResults: StateFlow<List<CustomerEntity>> = _customerResults.asStateFlow()

    private val _isSearchingCustomer = MutableStateFlow(false)
    val isSearchingCustomer: StateFlow<Boolean> = _isSearchingCustomer.asStateFlow()

    // --- Header: Salesman (optional, local cache) ----------------------------

    private val _salesman = MutableStateFlow<SalesPersonEntity?>(null)
    val salesman: StateFlow<SalesPersonEntity?> = _salesman.asStateFlow()

    private val _salesmanQuery = MutableStateFlow("")
    val salesmanQuery: StateFlow<String> = _salesmanQuery.asStateFlow()

    private val _salesmanResults = MutableStateFlow<List<SalesPersonEntity>>(emptyList())
    val salesmanResults: StateFlow<List<SalesPersonEntity>> = _salesmanResults.asStateFlow()

    private val _isSearchingSalesman = MutableStateFlow(false)
    val isSearchingSalesman: StateFlow<Boolean> = _isSearchingSalesman.asStateFlow()

    // --- Header: Driver (optional, local cache) ------------------------------

    private val _driver = MutableStateFlow<DriverEntity?>(null)
    val driver: StateFlow<DriverEntity?> = _driver.asStateFlow()

    private val _driverQuery = MutableStateFlow("")
    val driverQuery: StateFlow<String> = _driverQuery.asStateFlow()

    private val _driverResults = MutableStateFlow<List<DriverEntity>>(emptyList())
    val driverResults: StateFlow<List<DriverEntity>> = _driverResults.asStateFlow()

    private val _isSearchingDriver = MutableStateFlow(false)
    val isSearchingDriver: StateFlow<Boolean> = _isSearchingDriver.asStateFlow()

    // --- Header: Notes -------------------------------------------------------

    private val _note = MutableStateFlow("")
    val note: StateFlow<String> = _note.asStateFlow()

    // --- Item Region: pending line editor ------------------------------------

    /** Selected item for the line being entered; null while scanning/searching. */
    private val _pendingItem = MutableStateFlow<BarangEntity?>(null)
    val pendingItem: StateFlow<BarangEntity?> = _pendingItem.asStateFlow()

    private val _itemQuery = MutableStateFlow("")
    val itemQuery: StateFlow<String> = _itemQuery.asStateFlow()

    private val _itemResults = MutableStateFlow<List<BarangEntity>>(emptyList())
    val itemResults: StateFlow<List<BarangEntity>> = _itemResults.asStateFlow()

    private val _isSearchingItem = MutableStateFlow(false)
    val isSearchingItem: StateFlow<Boolean> = _isSearchingItem.asStateFlow()

    private val _pendingQty = MutableStateFlow("")
    val pendingQty: StateFlow<String> = _pendingQty.asStateFlow()

    private val _pendingUnit = MutableStateFlow("")
    val pendingUnit: StateFlow<String> = _pendingUnit.asStateFlow()

    /** `""` until the user picks a Return Type (IR-M6). */
    private val _pendingJenisRetur = MutableStateFlow("")
    val pendingJenisRetur: StateFlow<String> = _pendingJenisRetur.asStateFlow()

    private val _pendingError = MutableStateFlow<String?>(null)
    val pendingError: StateFlow<String?> = _pendingError.asStateFlow()

    /** Loaded + edited lines; order is re-sequenced by the repository on save. */
    private val _items = MutableStateFlow<List<ReturnOrderCaptureLine>>(emptyList())
    val items: StateFlow<List<ReturnOrderCaptureLine>> = _items.asStateFlow()

    // --- Action Region: edit state (§14.2 Loaded → Dirty → Saving → Saved) ---

    /** True when the editable fields differ from the loaded order. */
    private val _isDirty = MutableStateFlow(false)
    val isDirty: StateFlow<Boolean> = _isDirty.asStateFlow()

    private val _isSaving = MutableStateFlow(false)
    val isSaving: StateFlow<Boolean> = _isSaving.asStateFlow()

    private val _saveError = MutableStateFlow<String?>(null)
    val saveError: StateFlow<String?> = _saveError.asStateFlow()

    private val _saved = MutableStateFlow(false)
    val saved: StateFlow<Boolean> = _saved.asStateFlow()

    /** Snapshot of the loaded order, the baseline for `Dirty` (§14.2). */
    private var initial: EditSnapshot? = null

    private var customerSearchJob: Job? = null
    private var salesmanSearchJob: Job? = null
    private var driverSearchJob: Job? = null
    private var itemSearchJob: Job? = null

    init {
        load()
    }

    /**
     * Loads the stored order and prefills the editable fields (§14.2
     * `Loaded`). A non-`DRAFT` order is rejected before any field is offered
     * (BR-018).
     */
    private fun load() {
        viewModelScope.launch {
            try {
                val loaded = if (returnOrderId.isBlank()) {
                    null
                } else {
                    captureRepository.getOrder(returnOrderId)
                }
                if (loaded == null) {
                    _notFound.value = true
                    return@launch
                }
                if (loaded.status != ReturnOrderEntity.STATUS_DRAFT) {
                    // BR-018 — a synced order can never enter edit.
                    _notEditable.value = true
                    return@launch
                }

                _warehouseCode.value = loaded.warehouseCode
                // Prefill each reference from the local cache; when the cache
                // no longer holds it the user searches anew (EditBarcode
                // precedent).
                _customer.value = try {
                    customerDao.getById(loaded.customerId)
                } catch (e: Exception) {
                    null
                }
                _salesman.value = if (loaded.salesPersonId.isBlank()) {
                    null
                } else {
                    try {
                        salesPersonDao.getById(loaded.salesPersonId)
                    } catch (e: Exception) {
                        null
                    }
                }
                _driver.value = if (loaded.driverId.isBlank()) {
                    null
                } else {
                    try {
                        driverDao.getById(loaded.driverId)
                    } catch (e: Exception) {
                        null
                    }
                }
                _note.value = loaded.note
                _items.value = try {
                    captureRepository.listItems(returnOrderId).map { it.toCaptureLine() }
                } catch (e: Exception) {
                    emptyList()
                }

                initial = snapshot()
                refreshDirty()
            } catch (e: Exception) {
                _notFound.value = true
            } finally {
                _isLoading.value = false
            }
        }
    }

    // --- Customer picker -----------------------------------------------------

    /** Customer search text changed — debounced local cache lookup. */
    fun onCustomerQueryChange(value: String) {
        _customerQuery.value = value
        // Typing means the user is choosing anew.
        _customer.value = null
        refreshDirty()
        customerSearchJob?.cancel()
        val query = value.trim()
        if (query.isEmpty()) {
            _customerResults.value = emptyList()
            _isSearchingCustomer.value = false
            return
        }
        _isSearchingCustomer.value = true
        customerSearchJob = viewModelScope.launch {
            delay(SEARCH_DEBOUNCE_MILLIS)
            try {
                _customerResults.value = customerDao.search(query)
            } catch (e: Exception) {
                _customerResults.value = emptyList()
            } finally {
                _isSearchingCustomer.value = false
            }
        }
    }

    /** Customer selected from the local result list (BR-001/002, GAP-004). */
    fun onSelectCustomer(customer: CustomerEntity) {
        _customer.value = customer
        _customerQuery.value = ""
        _customerResults.value = emptyList()
        refreshDirty()
    }

    /** Customer selection cleared. */
    fun onClearCustomer() {
        _customer.value = null
        refreshDirty()
    }

    // --- Salesman picker (optional) ------------------------------------------

    fun onSalesmanQueryChange(value: String) {
        _salesmanQuery.value = value
        _salesman.value = null
        refreshDirty()
        salesmanSearchJob?.cancel()
        val query = value.trim()
        if (query.isEmpty()) {
            _salesmanResults.value = emptyList()
            _isSearchingSalesman.value = false
            return
        }
        _isSearchingSalesman.value = true
        salesmanSearchJob = viewModelScope.launch {
            delay(SEARCH_DEBOUNCE_MILLIS)
            try {
                _salesmanResults.value = salesPersonDao.search(query)
            } catch (e: Exception) {
                _salesmanResults.value = emptyList()
            } finally {
                _isSearchingSalesman.value = false
            }
        }
    }

    fun onSelectSalesman(salesman: SalesPersonEntity) {
        _salesman.value = salesman
        _salesmanQuery.value = ""
        _salesmanResults.value = emptyList()
        refreshDirty()
    }

    fun onClearSalesman() {
        _salesman.value = null
        refreshDirty()
    }

    // --- Driver picker (optional) --------------------------------------------

    fun onDriverQueryChange(value: String) {
        _driverQuery.value = value
        _driver.value = null
        refreshDirty()
        driverSearchJob?.cancel()
        val query = value.trim()
        if (query.isEmpty()) {
            _driverResults.value = emptyList()
            _isSearchingDriver.value = false
            return
        }
        _isSearchingDriver.value = true
        driverSearchJob = viewModelScope.launch {
            delay(SEARCH_DEBOUNCE_MILLIS)
            try {
                _driverResults.value = driverDao.search(query)
            } catch (e: Exception) {
                _driverResults.value = emptyList()
            } finally {
                _isSearchingDriver.value = false
            }
        }
    }

    fun onSelectDriver(driver: DriverEntity) {
        _driver.value = driver
        _driverQuery.value = ""
        _driverResults.value = emptyList()
        refreshDirty()
    }

    fun onClearDriver() {
        _driver.value = null
        refreshDirty()
    }

    fun onNoteChange(value: String) {
        _note.value = value
        refreshDirty()
    }

    // --- Item identification (Barcode Scan or Manual Item Search, BR-009) ----

    /**
     * Scanner callback (BR-009). Resolves the barcode against the local
     * `barcode_entity` cache and then loads its Item from the local
     * `barang_entity` cache — no network call (P-07).
     */
    fun onBarcodeScanned(rawValue: String?) {
        if (_pendingItem.value != null) return
        if (rawValue.isNullOrEmpty()) return
        val displayValue = BarcodeNormalization.normalize(rawValue)
        if (displayValue.isEmpty()) return
        viewModelScope.launch {
            _pendingError.value = null
            val barcode = try {
                barcodeDao.getByKey(BarcodeNormalization.toKey(displayValue))
            } catch (e: Exception) {
                null
            }
            if (barcode == null) {
                _pendingError.value = ReturnOrderCaptureRepository.ITEM_NOT_CACHED
                return@launch
            }
            val item = try {
                barangDao.getById(barcode.brgId)
            } catch (e: Exception) {
                null
            }
            if (item == null) {
                _pendingError.value = ReturnOrderCaptureRepository.ITEM_NOT_CACHED
                return@launch
            }
            selectPendingItem(item)
        }
    }

    /** Manual Item Search text changed — debounced local cache lookup. */
    fun onItemQueryChange(value: String) {
        _itemQuery.value = value
        _pendingItem.value = null
        _pendingQty.value = ""
        _pendingUnit.value = ""
        _pendingJenisRetur.value = ""
        itemSearchJob?.cancel()
        val query = value.trim()
        if (query.isEmpty()) {
            _itemResults.value = emptyList()
            _isSearchingItem.value = false
            return
        }
        _isSearchingItem.value = true
        itemSearchJob = viewModelScope.launch {
            delay(SEARCH_DEBOUNCE_MILLIS)
            try {
                _itemResults.value = barangDao.search(query)
            } catch (e: Exception) {
                _itemResults.value = emptyList()
            } finally {
                _isSearchingItem.value = false
            }
        }
    }

    /** Item selected from the local search results (BR-009). */
    fun onSelectItem(item: BarangEntity) {
        selectPendingItem(item)
    }

    private fun selectPendingItem(item: BarangEntity) {
        _pendingItem.value = item
        _pendingQty.value = ""
        _pendingUnit.value = ""
        _pendingJenisRetur.value = ""
        _pendingError.value = null
        _itemQuery.value = ""
        _itemResults.value = emptyList()
    }

    /** Pending item cleared: back to scan/search (add/remove lines, §12.4). */
    fun onClearPendingItem() {
        _pendingItem.value = null
        _pendingQty.value = ""
        _pendingUnit.value = ""
        _pendingJenisRetur.value = ""
        _pendingError.value = null
    }

    /**
     * Unit options for the pending item: its own cached small/big units
     * (BR-011, ADR-RO-003); empty when nothing is selected.
     */
    fun unitOptions(): List<String> {
        val item = _pendingItem.value ?: return emptyList()
        return listOf(item.satKecil, item.satBesar)
            .filter { it.isNotBlank() }
            .distinct()
    }

    fun onQtyChange(value: String) {
        _pendingQty.value = value
    }

    fun onUnitChange(unit: String) {
        _pendingUnit.value = unit
    }

    fun onJenisReturChange(value: String) {
        _pendingJenisRetur.value = value
    }

    /**
     * Adds the pending line after the per-line guardrails (IR-M2…IR-M6):
     * the item exists in the local cache (BR-007/008), `Qty > 0` (BR-010),
     * the unit comes from the item's cached unit data (BR-011), and the
     * Return Type is exactly `BAGUS`/`RUSAK` (ADR-RO-004).
     */
    fun addItem() {
        val item = _pendingItem.value
        if (item == null) {
            _pendingError.value = ReturnOrderCaptureRepository.ITEM_ID_REQUIRED
            return
        }
        val qty = _pendingQty.value.trim().toDoubleOrNull()
        if (qty == null || qty <= 0.0) {
            _pendingError.value = ReturnOrderCaptureRepository.QTY_INVALID
            return
        }
        val unit = _pendingUnit.value
        if (unit.isBlank()) {
            _pendingError.value = ReturnOrderCaptureRepository.UNIT_REQUIRED
            return
        }
        if (unit !in unitOptions()) {
            _pendingError.value = ReturnOrderCaptureRepository.UNIT_INVALID
            return
        }
        val jenisRetur = _pendingJenisRetur.value
        if (jenisRetur != ReturnOrderItemEntity.JENIS_RETUR_BAGUS &&
            jenisRetur != ReturnOrderItemEntity.JENIS_RETUR_RUSAK
        ) {
            _pendingError.value = ReturnOrderCaptureRepository.RETURN_TYPE_INVALID
            return
        }

        _items.value = _items.value + ReturnOrderCaptureLine(
            brgId = item.brgId,
            brgCode = item.brgCode,
            brgName = item.brgName,
            qty = qty,
            satId = unit,
            jenisRetur = jenisRetur
        )
        _pendingError.value = null
        // Reset the editor and re-arm the scanner for the next line (§12.4).
        _pendingItem.value = null
        _pendingQty.value = ""
        _pendingUnit.value = ""
        _pendingJenisRetur.value = ""
        refreshDirty()
    }

    /** Removes an added line (add/remove lines, §12.4). */
    fun removeItem(index: Int) {
        val current = _items.value
        if (index !in current.indices) return
        _items.value = current.toMutableList().apply { removeAt(index) }
        refreshDirty()
    }

    // --- Save (local write only, BR-017/018, §14.2) --------------------------

    /**
     * True when a change may be saved (§14.2): the order entered edit as
     * `DRAFT`, at least one field changed (`Dirty`), a Customer is set and at
     * least one line exists.
     */
    fun canSave(): Boolean =
        !_notEditable.value && !_isSaving.value && !_saved.value &&
            _isDirty.value && _customer.value != null && _items.value.isNotEmpty()

    /**
     * Save the modified Return Order locally (BC-002, §14.2).
     *
     * Local Room write only through [ReturnOrderCaptureRepository.updateDraft]
     * — no network call (P-07). The repository re-gates on `DRAFT` and keeps
     * the order `DRAFT`; the stored warehouse/identity/audit are preserved and
     * no `ReturnOrderNo` is authored (ADR-RO-002).
     */
    fun save() {
        if (_isSaving.value || _saved.value) return
        val customer = _customer.value
        if (customer == null) {
            _saveError.value = ReturnOrderCaptureRepository.CUSTOMER_REQUIRED
            return
        }
        val lines = _items.value
        if (lines.isEmpty()) {
            _saveError.value = ReturnOrderCaptureRepository.ITEM_REQUIRED
            return
        }
        viewModelScope.launch {
            _isSaving.value = true
            _saveError.value = null
            try {
                val draft = ReturnOrderDraft(
                    customerId = customer.customerId,
                    salesPersonId = _salesman.value?.salesPersonId.orEmpty(),
                    driverId = _driver.value?.driverId.orEmpty(),
                    note = _note.value,
                    items = lines.map { line ->
                        ReturnOrderDraftItem(
                            brgId = line.brgId,
                            // P-09/ADR-RO-003 — recorded exactly as captured.
                            qty = line.qty,
                            satId = line.satId,
                            jenisRetur = line.jenisRetur
                        )
                    }
                )
                when (val result = captureRepository.updateDraft(returnOrderId, draft)) {
                    is ReturnOrderCaptureResult.Saved -> _saved.value = true
                    is ReturnOrderCaptureResult.Rejected ->
                        _saveError.value = result.errors.joinToString("\n")
                }
            } catch (e: Exception) {
                _saveError.value =
                    "Gagal menyimpan: ${e.message?.take(200) ?: "kesalahan tidak diketahui."}"
            } finally {
                _isSaving.value = false
            }
        }
    }

    // --- Dirty tracking (§14.2 Loaded → Dirty) -------------------------------

    private data class EditSnapshot(
        val customerId: String,
        val salesPersonId: String,
        val driverId: String,
        val note: String,
        val items: List<ReturnOrderCaptureLine>
    )

    private fun snapshot(): EditSnapshot = EditSnapshot(
        customerId = _customer.value?.customerId.orEmpty(),
        salesPersonId = _salesman.value?.salesPersonId.orEmpty(),
        driverId = _driver.value?.driverId.orEmpty(),
        note = _note.value,
        items = _items.value
    )

    private fun refreshDirty() {
        _isDirty.value = initial?.let { snapshot() != it } ?: false
    }

    private fun ReturnOrderItemEntity.toCaptureLine() = ReturnOrderCaptureLine(
        brgId = brgId,
        brgCode = brgCode,
        brgName = brgName,
        // Qty is the recorded physical-unit quantity — never re-derived
        // (P-09, ADR-RO-003).
        qty = qty,
        satId = satId,
        jenisRetur = jenisRetur
    )
}
