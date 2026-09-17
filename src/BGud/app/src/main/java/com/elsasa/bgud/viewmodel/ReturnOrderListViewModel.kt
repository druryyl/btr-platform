package com.elsasa.bgud.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.elsasa.bgud.dao.ReturnOrderDao
import com.elsasa.bgud.dao.ReturnOrderItemDao
import com.elsasa.bgud.model.ReturnOrderEntity
import kotlinx.coroutines.Job
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

/**
 * Return Order List row (SCR-MOB-RO-001, §12.1): the local order plus its
 * line count. Display vocabulary is `Draft`/`Synced` only (ADR-RO-006, §14.4).
 */
data class ReturnOrderListItem(
    val order: ReturnOrderEntity,
    val itemCount: Int
)

/**
 * Return Order list view model (SCR-MOB-RO-001, Architecture §12.1, §14.4,
 * §19.1, §20).
 *
 * Key fields: `query`, `results`, `isEmpty`, `statusFilter`. Source: Room
 * `return_order_entity` / `return_order_item_entity` only — local cache, no
 * network (§17.1).
 *
 * Rules (no new decisions):
 * - Search covers Customer name, date, and status (§12.1); the status filter
 *   is `Draft`/`Synced` only (§11.1, ADR-RO-006).
 * - Minimum 3 characters; auto search debounced 300 ms; Enter submits
 *   immediately (§20).
 * - Default load 50 rows; incremental paging — load more on scroll end
 *   (§20).
 * - A row opens Detail and the Create action is owned by the screen (§12.1,
 *   §13.1; route wiring is S4.11 and the destinations themselves are
 *   S4.7/S4.8).
 */
class ReturnOrderListViewModel(
    private val returnOrderDao: ReturnOrderDao,
    private val returnOrderItemDao: ReturnOrderItemDao
) : ViewModel() {

    companion object {
        /** Default page size (§20: default load 50 rows). */
        const val PAGE_SIZE = 50

        /** Minimum search characters (§20). */
        const val MIN_QUERY_LENGTH = 3

        /** Auto-search debounce (§20: 300 ms). */
        const val SEARCH_DEBOUNCE_MILLIS = 300L

        /** Status filter value selecting every local order (§11.1). */
        const val STATUS_ALL = ""
    }

    private val _query = MutableStateFlow("")
    val query: StateFlow<String> = _query.asStateFlow()

    /** `""` = all, otherwise `DRAFT` / `SYNCED` (§11.1, ADR-RO-006). */
    private val _statusFilter = MutableStateFlow(STATUS_ALL)
    val statusFilter: StateFlow<String> = _statusFilter.asStateFlow()

    private val _results = MutableStateFlow<List<ReturnOrderListItem>>(emptyList())
    val results: StateFlow<List<ReturnOrderListItem>> = _results.asStateFlow()

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
        // Default load: first 50 rows, no query, all statuses (§20).
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

    /** Status filter changed (§11.1) — reload from the first page. */
    fun onStatusFilterChange(status: String) {
        if (_statusFilter.value == status) return
        _statusFilter.value = status
        searchJob?.cancel()
        loadFirstPage()
    }

    /** Load more on scroll end (§20: incremental paging). */
    fun loadMore() {
        if (_isLoading.value || _isLoadingMore.value || !_hasMore.value) return
        viewModelScope.launch {
            _isLoadingMore.value = true
            try {
                val offset = _results.value.size
                val page = queryDao(offset)
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
                val page = queryDao(0)
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

    private suspend fun queryDao(offset: Int): List<ReturnOrderListItem> {
        val query = _query.value.trim()
        val effectiveQuery = if (query.length >= MIN_QUERY_LENGTH) query else ""
        return returnOrderDao.search(
            effectiveQuery,
            _statusFilter.value,
            PAGE_SIZE,
            offset
        ).map { order ->
            ReturnOrderListItem(
                order = order,
                itemCount = returnOrderItemDao.countByParent(order.returnOrderId)
            )
        }
    }
}
