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
 * §14.2).
 *
 * Key fields: `order` (header), `items` (lines), `isLoading`, `notFound`, and
 * the Delete state (`isDeleting` / `deleteError` / `deleted`). Source is the
 * local Room capture store only, read through [ReturnOrderCaptureRepository] —
 * the screen issues no network call (§17.1, P-07).
 *
 * Rules (no new decisions):
 * - The header is shown read-only (§12.3); the device vocabulary is exactly
 *   `Draft`/`Synced` (ADR-RO-006, §14.4).
 * - Edit and Delete are enabled only while the order is `DRAFT`
 *   (IR-M7/M8, BR-017–020). A `SYNCED` order is view-only; the repository
 *   re-derives the same gate on write (BR-018/020).
 * - Delete removes the order and its items in one local transaction and never
 *   propagates (GAP-014, BC-003); it is a `Draft`-only action on Detail, not a
 *   separate screen (§11.1).
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

    private val _isDeleting = MutableStateFlow(false)
    val isDeleting: StateFlow<Boolean> = _isDeleting.asStateFlow()

    private val _deleteError = MutableStateFlow<String?>(null)
    val deleteError: StateFlow<String?> = _deleteError.asStateFlow()

    private val _deleted = MutableStateFlow(false)
    val deleted: StateFlow<Boolean> = _deleted.asStateFlow()

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
    fun canEdit(): Boolean = isDraft() && !_isDeleting.value && !_deleted.value

    /** Delete is offered only for a `DRAFT` order (IR-M7/M8, BR-019/020). */
    fun canDelete(): Boolean = isDraft() && !_isDeleting.value && !_deleted.value

    /**
     * Delete the `DRAFT` order and its items (BC-003).
     *
     * Delegates to [ReturnOrderCaptureRepository.deleteDraft]: one local
     * transaction, local-only, never propagated (GAP-014). A `SYNCED` order is
     * rejected by the repository and surfaced as an error.
     */
    fun delete() {
        if (!canDelete()) return
        viewModelScope.launch {
            _isDeleting.value = true
            _deleteError.value = null
            try {
                when (val result = captureRepository.deleteDraft(returnOrderId)) {
                    is ReturnOrderCaptureResult.Saved -> _deleted.value = true
                    is ReturnOrderCaptureResult.Rejected ->
                        _deleteError.value = result.errors.joinToString("\n")
                }
            } catch (e: Exception) {
                _deleteError.value =
                    "Gagal menghapus: ${e.message?.take(200) ?: "kesalahan tidak diketahui."}"
            } finally {
                _isDeleting.value = false
            }
        }
    }
}
