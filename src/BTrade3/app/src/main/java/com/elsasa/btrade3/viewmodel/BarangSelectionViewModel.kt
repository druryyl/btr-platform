package com.elsasa.btrade3.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.elsasa.btrade3.model.Barang
import com.elsasa.btrade3.repository.BarangRepository
import com.elsasa.btrade3.util.BulkInputProfile
import com.elsasa.btrade3.util.bulkProfile
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class BarangSelectionViewModel(
    private val barangRepository: BarangRepository
) : ViewModel() {
    private val _barangs = MutableStateFlow<List<Barang>>(emptyList())
    val barangs: StateFlow<List<Barang>> = _barangs.asStateFlow()

    private val _searchQuery = MutableStateFlow("")
    val searchQuery: StateFlow<String> = _searchQuery.asStateFlow()

    private val _bulkModeEnabled = MutableStateFlow(false)
    val bulkModeEnabled: StateFlow<Boolean> = _bulkModeEnabled.asStateFlow()

    private val _selectedBrgIds = MutableStateFlow<Set<String>>(emptySet())
    val selectedBrgIds: StateFlow<Set<String>> = _selectedBrgIds.asStateFlow()

    init {
        loadBarangs()
    }

    private fun loadBarangs() {
        viewModelScope.launch {
            barangRepository.getAllBarangs().collect { barangList ->
                _barangs.value = barangList
            }
        }
    }

    fun setSearchQuery(query: String) {
        _searchQuery.value = query
    }

    fun setBulkMode(enabled: Boolean) {
        _bulkModeEnabled.value = enabled
        if (!enabled) {
            _selectedBrgIds.value = emptySet()
        }
    }

    fun toggleBulkMode() {
        setBulkMode(!_bulkModeEnabled.value)
    }

    fun canSelect(barang: Barang): Boolean {
        val selected = getSelectedBarangs()
        if (selected.isEmpty()) return true
        return barang.bulkProfile() == selected.first().bulkProfile()
    }

    fun toggleSelection(barang: Barang): Boolean {
        if (!_bulkModeEnabled.value) return false
        if (!canSelect(barang)) return false

        val current = _selectedBrgIds.value
        _selectedBrgIds.value = if (barang.brgId in current) {
            current - barang.brgId
        } else {
            current + barang.brgId
        }
        return true
    }

    fun selectGroup(barangs: List<Barang>) {
        if (barangs.isEmpty()) return
        _bulkModeEnabled.value = true
        _selectedBrgIds.value = barangs.map { it.brgId }.toSet()
    }

    fun clearSelection() {
        _selectedBrgIds.value = emptySet()
    }

    fun getSelectedBarangs(): List<Barang> {
        val ids = _selectedBrgIds.value
        return _barangs.value.filter { it.brgId in ids }
    }

    fun activeBulkProfile(): BulkInputProfile? =
        getSelectedBarangs().firstOrNull()?.bulkProfile()
}
