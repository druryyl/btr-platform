package com.elsasa.bgud.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.elsasa.bgud.model.ReturnOrderEntity
import com.elsasa.bgud.model.ReturnOrderItemEntity
import com.elsasa.bgud.repository.ReturnOrderCaptureRepository
import com.elsasa.bgud.repository.ReturnOrderCaptureResult
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

/**
 * Return Order Detail view model (SCR-MOB-RO-003, Architecture §11.1, §12.3,
 * §14.2). Exclusively for Synced orders; Delete action relocated to Edit
 * screen (TD-004, TD-005).
 *
 * Key fields: `order` (header), `items` (lines), `isLoading`, `notFound`.
 * Source is the local Room capture store only, read through
 * [ReturnOrderCaptureRepository] — the screen issues no network call
 * (§17.1, P-07).
 *
 * Rules (no new decisions):
 * - The header is shown read-only (§12.3); the device vocabulary is exactly
 *   `Draft`/`Synced` (ADR-RO-006, §14.4).
 * - Edit is enabled only while the order is `DRAFT` (IR-M7/M8, BR-017–018).
 *   A `SYNCED` order is view-only; the repository re-derives the same gate on
 *   write (BR-018).
 * - Delete action has been relocated to Edit screen (TD-005, P2-S03).
 * - Edit routes to `return_order_edit?returnOrderId={id}` (§13.1); the route
 *   wiring is owned by S4.11 and the Edit screen by S4.9, so this view model
 *   only reports whether Edit is allowed.
 */
class ReturnOrderDetailViewModel(
    private val captureRepository: ReturnOrderCaptureRepository,
    private val returnOrderId: String
) : ViewModel() {

    private val _isLoading = MutableStateFlow(true)
    val isLoading: StateFlow<Boolean> = _isLoading.asStateFlow()

    /** Loaded header, or null while loading / not found. */
    private val _order = MutableStateFlow<ReturnOrderEntity?>(null)
    val order: StateFlow<ReturnOrderEntity?> = _order.asStateFlow()

    /** Item lines loaded from the local store (Item, Qty, Unit, Return Type). */
    private val _items = MutableStateFlow<List<ReturnOrderItemEntity>>(emptyList())
    val items: StateFlow<List<ReturnOrderItemEntity>> = _items.asStateFlow()

    /** True when [returnOrderId] matches no local row. */
    private val _notFound = MutableStateFlow(false)
    val notFound: StateFlow<Boolean> = _notFound.asStateFlow()

    init {
        load()
    }

    /** Local load of the header + lines (§14.2 `Loaded`). */
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
                } else {
                    _order.value = loaded
                    _items.value = captureRepository.listItems(returnOrderId)
                }
            } catch (e: Exception) {
                _notFound.value = true
            } finally {
                _isLoading.value = false
            }
        }
    }

    /** True while the loaded order is `DRAFT` (IR-M8, BR-017/019). */
    private fun isDraft(): Boolean =
        _order.value?.status == ReturnOrderEntity.STATUS_DRAFT

    /** Edit is offered only for a `DRAFT` order (IR-M7/M8, BR-017/018). */
    fun canEdit(): Boolean = isDraft()
}
