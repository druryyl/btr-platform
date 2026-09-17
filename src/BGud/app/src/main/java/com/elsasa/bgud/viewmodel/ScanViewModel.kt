package com.elsasa.bgud.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.elsasa.bgud.dao.BarcodeDao
import com.elsasa.bgud.model.BarcodeEntity
import com.elsasa.bgud.util.BarcodeNormalization
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

/**
 * Scan resolution state (SCR-MOB-003, Architecture §14.2).
 *
 * ```text
 * Scanning
 *   ↓ barcode recognized / manual submit
 * Resolving            (local cache lookup, no network)
 *   ├─ found ──────▶ Found
 *   └─ not found ───▶ NotFound
 * Found
 *   ↓ Close / scan again
 * Scanning
 *
 * NotFound
 *   ├─ Register Barcode ──▶ (navigate to register with barcode)
 *   └─ Cancel ────────────▶ Scanning
 * ```
 */
enum class ScanState {
    Scanning,
    Resolving,
    Found,
    NotFound
}

/**
 * Scan view model (SCR-MOB-003, Architecture §19.3).
 *
 * Key fields: `lastScannedValue`, `scanState`
 * (`Scanning`/`Resolving`/`Found`/`NotFound`), `resolvedItem`.
 * Source: Room `barcode_entity` only — every lookup goes through the unique
 * `barcodeValueKey` index (cache-first, P-07, ADR-006); no scan triggers a
 * network call. Manual entry uses the same resolution path as a scan (§20).
 *
 * Single-scan resolution mode (§20): a recognized barcode is accepted only
 * while in `Scanning`; once resolved to `Found`/`NotFound` the scanner input
 * is ignored until [resetToScanning] re-arms it.
 */
class ScanViewModel(
    private val barcodeDao: BarcodeDao
) : ViewModel() {

    private val _scanState = MutableStateFlow(ScanState.Scanning)
    val scanState: StateFlow<ScanState> = _scanState.asStateFlow()

    private val _lastScannedValue = MutableStateFlow("")
    val lastScannedValue: StateFlow<String> = _lastScannedValue.asStateFlow()

    private val _resolvedItem = MutableStateFlow<BarcodeEntity?>(null)
    val resolvedItem: StateFlow<BarcodeEntity?> = _resolvedItem.asStateFlow()

    /**
     * Camera/ML Kit callback. Accepted only while `Scanning` (single-scan
     * mode); results arriving while resolving or after resolution are
     * dropped until the screen re-arms via [resetToScanning].
     */
    fun onBarcodeScanned(rawValue: String?) {
        if (_scanState.value != ScanState.Scanning) return
        if (rawValue.isNullOrEmpty()) return
        resolve(rawValue)
    }

    /**
     * Manual entry fallback (§12.5, §20). Uses the same resolution path as
     * a scan. Accepted whenever no lookup is already in flight.
     */
    fun onManualSubmit(rawValue: String) {
        if (_scanState.value == ScanState.Resolving) return
        resolve(rawValue)
    }

    /** Re-arm single-scan mode: back to `Scanning` (§20, §14.2). */
    fun resetToScanning() {
        _resolvedItem.value = null
        _scanState.value = ScanState.Scanning
    }

    private fun resolve(rawValue: String) {
        // VO-01 local form: strip CR/LF/TAB, trim, preserve leading zeros;
        // lookup key is UPPER(normalized) — never the raw value (BQ-5).
        val displayValue = BarcodeNormalization.normalize(rawValue)
        if (displayValue.isEmpty()) return
        _lastScannedValue.value = displayValue
        _resolvedItem.value = null
        _scanState.value = ScanState.Resolving
        viewModelScope.launch {
            val key = BarcodeNormalization.toKey(displayValue)
            val entity = barcodeDao.getByKey(key)
            _resolvedItem.value = entity
            // `Resolving` never waits on the network (P-07); the cache holds
            // only Active barcodes (TQ-6), so a hit is an Active mapping
            // (BR-007) and a miss is the Not Found region (IR-M1).
            _scanState.value = if (entity != null) ScanState.Found else ScanState.NotFound
        }
    }
}
