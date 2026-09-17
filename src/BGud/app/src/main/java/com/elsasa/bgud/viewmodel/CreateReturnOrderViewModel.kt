package com.elsasa.bgud.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.elsasa.bgud.dao.BarangDao
import com.elsasa.bgud.dao.BarcodeDao
import com.elsasa.bgud.dao.CustomerDao
import com.elsasa.bgud.dao.DriverDao
import com.elsasa.bgud.dao.SalesPersonDao
import com.elsasa.bgud.datastore.SessionPreferencesDataSource
import com.elsasa.bgud.model.BarangEntity
import com.elsasa.bgud.model.CustomerEntity
import com.elsasa.bgud.model.DriverEntity
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
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.launch

/**
 * One captured Return Order line held in memory until Save (SCR-MOB-RO-002).
 *
 * `brgCode`/`brgName` come from the local Item cache; `qty` is the physical-unit
 * quantity exactly as entered — no small-unit normalization (P-09,
 * ADR-RO-003); `satId` is the chosen unit taken from the item's cached unit
 * data (BR-011); `jenisRetur` is exactly `BAGUS`/`RUSAK` (ADR-RO-004).
 */
data class ReturnOrderCaptureLine(
    val brgId: String,
    val brgCode: String,
    val brgName: String,
    val qty: Double,
    val satId: String,
    val jenisRetur: String
)

/**
 * Create Return Order view model (SCR-MOB-RO-002, Architecture §11.1, §12.2,
 * §14.1, §19.1, §20).
 *
 * Key fields: `customer`, `salesman`, `driver`, `note`, `items`, `saveState`
 * (`isSaving` / `saveError` / `saved`). Every reference (Customer, Salesman,
 * Driver, Item, Barcode) resolves from the local Room caches — no capture step
 * triggers a network call (P-07, §17.1, BR-009).
 *
 * Rules (no new decisions):
 * - Customer is mandatory and selected from the cache (BR-001/002, GAP-004);
 *   Salesman/Driver are optional (BR-013–016, ADR-RO-005).
 * - Warehouse is read-only and session-bound (BR-005/006); it is never
 *   user-selectable and never hardcoded (ADR-RO-008).
 * - At least one valid item line (BR-012, IR-M2); `Qty > 0` (BR-010, IR-M4);
 *   unit from the item's cached unit data recorded as `SatId` (BR-011,
 *   IR-M5, ADR-RO-003); `JenisRetur` exactly `BAGUS`/`RUSAK` (ADR-RO-004,
 *   IR-M6); mixed return types are allowed per line.
 * - `Save` writes locally only (`DRAFT`, offline-first BG-003) through
 *   [ReturnOrderCaptureRepository]; it is enabled only when a Customer is set
 *   and at least one valid line exists (§14.1). No `ReturnOrderNo` is authored
 *   and no `ServerId` is stored or sent (ADR-RO-002, P-06).
 */
class CreateReturnOrderViewModel(
    private val captureRepository: ReturnOrderCaptureRepository,
    private val customerDao: CustomerDao,
    private val salesPersonDao: SalesPersonDao,
    private val driverDao: DriverDao,
    private val barangDao: BarangDao,
    private val barcodeDao: BarcodeDao,
    private val session: SessionPreferencesDataSource
) : ViewModel() {

    companion object {
        /** Local capture search debounce, mirroring the existing pattern. */
        const val SEARCH_DEBOUNCE_MILLIS = 300L
    }

    /** Session-bound warehouse, displayed read-only (BR-005/006, IR-RO-04). */
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

    /** Added lines; order is re-sequenced by the repository on save. */
    private val _items = MutableStateFlow<List<ReturnOrderCaptureLine>>(emptyList())
    val items: StateFlow<List<ReturnOrderCaptureLine>> = _items.asStateFlow()

    // --- Action Region: save state -------------------------------------------

    private val _isSaving = MutableStateFlow(false)
    val isSaving: StateFlow<Boolean> = _isSaving.asStateFlow()

    private val _saveError = MutableStateFlow<String?>(null)
    val saveError: StateFlow<String?> = _saveError.asStateFlow()

    private val _saved = MutableStateFlow(false)
    val saved: StateFlow<Boolean> = _saved.asStateFlow()

    private var customerSearchJob: Job? = null
    private var salesmanSearchJob: Job? = null
    private var driverSearchJob: Job? = null
    private var itemSearchJob: Job? = null

    init {
        // Read-only session-bound warehouse (BR-005/006, IR-RO-04).
        viewModelScope.launch {
            _warehouseCode.value = try {
                session.warehouseCode.first().orEmpty()
            } catch (e: Exception) {
                ""
            }
        }
    }

    // --- Customer picker -----------------------------------------------------

    /** Customer search text changed — debounced local cache lookup. */
    fun onCustomerQueryChange(value: String) {
        _customerQuery.value = value
        // Typing means the user is choosing anew.
        _customer.value = null
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
    }

    /** Customer selection cleared. */
    fun onClearCustomer() {
        _customer.value = null
    }

    // --- Salesman picker (optional) ------------------------------------------

    fun onSalesmanQueryChange(value: String) {
        _salesmanQuery.value = value
        _salesman.value = null
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
    }

    fun onClearSalesman() {
        _salesman.value = null
    }

    // --- Driver picker (optional) --------------------------------------------

    fun onDriverQueryChange(value: String) {
        _driverQuery.value = value
        _driver.value = null
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
    }

    fun onClearDriver() {
        _driver.value = null
    }

    fun onNoteChange(value: String) {
        _note.value = value
    }

    // --- Item identification (Barcode Scan or Manual Item Search, BR-009) ----

    /**
     * Scanner callback (BR-009). Resolves the barcode against the local
     * `barcode_entity` cache and then loads its Item from the local
     * `barang_entity` cache — no network call (P-07). The scanner is only
     * armed while no item is pending (single-scan mode).
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

    /** Pending item cleared: back to scan/search (add/remove lines, §12.2). */
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
        // Reset the editor and re-arm the scanner for the next line (§12.2).
        _pendingItem.value = null
        _pendingQty.value = ""
        _pendingUnit.value = ""
        _pendingJenisRetur.value = ""
    }

    /** Removes an added line (add/remove lines, §12.2). */
    fun removeItem(index: Int) {
        val current = _items.value
        if (index !in current.indices) return
        _items.value = current.toMutableList().apply { removeAt(index) }
    }

    // --- Save (local write only, offline-first BG-003, §14.1) ----------------

    /** True when the capture may be saved (§14.1, IR-M1/M2). */
    fun canSave(): Boolean =
        !_isSaving.value && !_saved.value &&
            _customer.value != null && _items.value.isNotEmpty()

    /**
     * Save the captured Return Order locally as `DRAFT` (BC-001, §14.1).
     *
     * Local Room write only — no network call (P-07); the order is submitted
     * later by the sync run (S4.5, I-RO-01). No `ReturnOrderNo` is authored
     * (ADR-RO-002). The offline guardrails run inside
     * [ReturnOrderCaptureRepository.createDraft].
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
                when (val result = captureRepository.createDraft(draft)) {
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
}
