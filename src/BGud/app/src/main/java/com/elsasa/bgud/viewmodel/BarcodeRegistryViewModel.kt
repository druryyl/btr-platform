package com.elsasa.bgud.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.elsasa.bgud.dao.BarcodeDao
import com.elsasa.bgud.model.BarcodeEntity
import kotlinx.coroutines.Job
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

/**
 * Barcode Registry list view model (SCR-MOB-005, Architecture §12.7, §19.3, §20).
 *
 * Key fields: `query`, `results`, `isEmpty`. Sources: Room
 * `barcode_entity` only — local cache, no network (§17.2).
 *
 * Rules (no new decisions):
 * - Search covers Barcode, Item Code, and Item Name (UX Blueprint §9).
 * - Minimum 3 characters; auto search debounced 300 ms; Enter submits
 *   immediately (§20).
 * - Default load 50 rows; incremental paging — load more on scroll end
 *   (§20).
 * - Row selection navigates to Edit Barcode (`edit?barcodeId={id}`, §13.2;
 *   the edit destination itself is owned by S5.9).
 */
class BarcodeRegistryViewModel(
    private val barcodeDao: BarcodeDao
) : ViewModel() {

    companion object {
        /** Default page size (§20: default load 50 rows). */
        const val PAGE_SIZE = 50

        /** Minimum search characters (§20). */
        const val MIN_QUERY_LENGTH = 3

        /** Auto-search debounce (§20: 300 ms). */
        const val SEARCH_DEBOUNCE_MILLIS = 300L
    }

    private val _query = MutableStateFlow("")
    val query: StateFlow<String> = _query.asStateFlow()

    private val _results = MutableStateFlow<List<BarcodeEntity>>(emptyList())
    val results: StateFlow<List<BarcodeEntity>> = _results.asStateFlow()

    /** True when the current result set is empty and no load is in flight. */
    private val _isEmpty = MutableStateFlow(false)
    val isEmpty: StateFlow<Boolean> = _isEmpty.asStateFlow()

    private val _isLoading = MutableStateFlow(false)
    val isLoading: StateFlow<Boolean> = _isLoading.asStateFlow()

    private val _isLoadingMore = MutableStateFlow(false)
    val isLoadingMore: StateFlow<Boolean> = _isLoadingMore.asStateFlow()

    private val _hasMore = MutableStateFlow(true)
    val hasMore: StateFlow<Boolean> = _hasMore.asStateFlow()

    private var searchJob: Job? = null

    init {
        // Default load: first 50 rows, no query (§20).
        loadFirstPage()
    }

    /**
     * Search text changed — debounced local cache lookup (§20: auto search,
     * debounced 300 ms, minimum 3 characters).
     */
    fun onQueryChange(value: String) {
        _query.value = value
        searchJob?.cancel()
        searchJob = viewModelScope.launch {
            delay(SEARCH_DEBOUNCE_MILLIS)
            loadFirstPage()
        }
    }

    /**
     * Enter to search (§20): submit immediately without waiting for the
     * debounce window.
     */
    fun onSearchSubmitted() {
        searchJob?.cancel()
        loadFirstPage()
    }

    /** Load more on scroll end (§20: incremental paging). */
    fun loadMore() {
        if (_isLoading.value || _isLoadingMore.value || !_hasMore.value) return
        viewModelScope.launch {
            _isLoadingMore.value = true
            try {
                val query = _query.value.trim()
                val offset = _results.value.size
                val page = queryDao(query, offset)
                _results.value = _results.value + page
                _hasMore.value = page.size == PAGE_SIZE
                _isEmpty.value = _results.value.isEmpty()
            } catch (e: Exception) {
                // Keep the loaded rows; a failed page simply stops
                // further auto paging until the next explicit search.
                _hasMore.value = false
            } finally {
                _isLoadingMore.value = false
            }
        }
    }

    private fun loadFirstPage() {
        viewModelScope.launch {
            _isLoading.value = true
            try {
                val query = _query.value.trim()
                val page = queryDao(query, 0)
                _results.value = page
                _hasMore.value = page.size == PAGE_SIZE
                _isEmpty.value = page.isEmpty()
            } catch (e: Exception) {
                _results.value = emptyList()
                _hasMore.value = false
                _isEmpty.value = true
            } finally {
                _isLoading.value = false
            }
        }
    }

    private suspend fun queryDao(query: String, offset: Int): List<BarcodeEntity> {
        return if (query.length >= MIN_QUERY_LENGTH) {
            barcodeDao.search(query, PAGE_SIZE, offset)
        } else {
            barcodeDao.paged(PAGE_SIZE, offset)
        }
    }
}
