package com.elsasa.bgud.repository

import androidx.room.withTransaction
import com.elsasa.bgud.database.AppDatabase
import com.elsasa.bgud.datastore.SessionPreferencesDataSource
import com.elsasa.bgud.model.BarangEntity
import com.elsasa.bgud.model.CustomerEntity
import com.elsasa.bgud.model.DriverEntity
import com.elsasa.bgud.model.ReturnOrderEntity
import com.elsasa.bgud.model.ReturnOrderItemEntity
import com.elsasa.bgud.model.SalesPersonEntity
import com.elsasa.bgud.util.Ulid
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.withContext

/**
 * Return Order capture payload — header + item lines (S4.4).
 *
 * Carries only what the Warehouse Officer selects. Reference names
 * (`customerCode`/`customerName`, `salesPersonName`, `driverName`, `brgCode`,
 * `brgName`) are resolved from the local caches by
 * [ReturnOrderCaptureRepository]; the device never invents them.
 */
data class ReturnOrderDraft(
    val customerId: String,
    /** Optional (BR-013/015, ADR-RO-005); `""` when not captured. */
    val salesPersonId: String = "",
    /** Optional (BR-015/016, ADR-RO-005); `""` when not captured. */
    val driverId: String = "",
    val note: String = "",
    val items: List<ReturnOrderDraftItem> = emptyList()
)

/**
 * One Return Order line as captured (S4.4).
 *
 * `qty` is the physical-unit quantity recorded exactly as entered — no
 * small-unit normalization anywhere (P-09, ADR-RO-003). `satId` is the chosen
 * unit from the item's cached unit data (BR-011); `jenisRetur` is exactly
 * `BAGUS` | `RUSAK` (ADR-RO-004).
 */
data class ReturnOrderDraftItem(
    val brgId: String,
    val qty: Double,
    val satId: String,
    val jenisRetur: String
)

/** Outcome of a local capture write (create/update/delete). */
sealed class ReturnOrderCaptureResult {
    /** The draft was persisted locally; [returnOrderId] is the ULID identity. */
    data class Saved(val returnOrderId: String) : ReturnOrderCaptureResult()

    /** The draft was rejected by the offline guardrails; nothing was written. */
    data class Rejected(val errors: List<String>) : ReturnOrderCaptureResult()
}

/**
 * Offline Return Order capture repository (S4.4, Architecture §4.4, §15.1,
 * §18.3, §23.2).
 *
 * Owns create / update / delete of local `DRAFT` orders. Every write is local
 * Room only — a capture step never triggers a network call (P-07, §17.1); the
 * order is submitted later by the sync run (S4.5, I-RO-01).
 *
 * Rules enforced here (no new decisions):
 * - `ReturnOrderId` is generated on the device as a canonical ULID matching the
 *   platform's `Ulid.NewUlid().ToString()` format (IR-RO-01, [Ulid]); new
 *   orders are `DRAFT` (ADR-RO-006) and never carry `ReturnOrderNo`.
 * - `WarehouseCode` is stamped from the session binding for new orders
 *   (BR-005/006, IR-RO-04) and preserved on update — a draft is never re-homed.
 * - Customer is mandatory and must exist in the local cache (BR-001/002,
 *   GAP-004); Salesman/Driver are optional, but a supplied reference must exist
 *   in the local cache (BR-013–016, ADR-RO-005).
 * - At least one item (BR-012); the item must exist in the cached Item Master
 *   (BR-007/008); `Qty > 0` (BR-010); the unit is mandatory and must come from
 *   the item's cached unit data (BR-011); `JenisRetur` is exactly
 *   `BAGUS`/`RUSAK` per line, mixed types allowed (ADR-RO-004, DOMAIN §9).
 * - Header + items are written in one local transaction; delete removes both
 *   (GAP-014).
 * - Update and delete are gated by `DRAFT` status (BR-017–020); a `SYNCED` order
 *   is rejected and never modified. Pre-sync delete is local-only and never
 *   propagates (GAP-014).
 * - The device never stores or sends `ServerId` as a command input (P-06).
 */
class ReturnOrderCaptureRepository(
    private val database: AppDatabase,
    private val session: SessionPreferencesDataSource
) {

    private val returnOrderDao = database.returnOrderDao()
    private val returnOrderItemDao = database.returnOrderItemDao()
    private val customerDao = database.customerDao()
    private val salesPersonDao = database.salesPersonDao()
    private val driverDao = database.driverDao()
    private val barangDao = database.barangDao()

    /**
     * Creates a new local `DRAFT` Return Order (BC-001, §15.1 Capture Mode).
     *
     * The ULID `ReturnOrderId` is generated here; the session-bound
     * `warehouseCode` is stamped here; the local caches resolve every
     * denormalized name. Header + items are persisted in one transaction.
     */
    suspend fun createDraft(draft: ReturnOrderDraft): ReturnOrderCaptureResult =
        withContext(Dispatchers.IO) {
            val warehouseCode = session.warehouseCode.first().orEmpty()
            when (val resolution = resolve(draft, warehouseCode)) {
                is DraftResolution.Invalid ->
                    return@withContext ReturnOrderCaptureResult.Rejected(resolution.errors)

                is DraftResolution.Valid -> {
                    val returnOrderId = Ulid.newUlid()
                    val header = ReturnOrderEntity(
                        returnOrderId = returnOrderId,
                        customerId = resolution.customer.customerId,
                        customerCode = resolution.customer.customerCode,
                        customerName = resolution.customer.customerName,
                        warehouseCode = warehouseCode,
                        salesPersonId = resolution.salesPerson?.salesPersonId.orEmpty(),
                        salesPersonName = resolution.salesPerson?.salesPersonName.orEmpty(),
                        driverId = resolution.driver?.driverId.orEmpty(),
                        driverName = resolution.driver?.driverName.orEmpty(),
                        note = draft.note,
                        status = ReturnOrderEntity.STATUS_DRAFT,
                        createdAt = System.currentTimeMillis(),
                        createdBy = session.userId.first().orEmpty()
                    )
                    val items = resolution.items.mapIndexed { index, item ->
                        item.toEntity(returnOrderId, index + 1)
                    }

                    database.withTransaction {
                        returnOrderDao.upsert(header)
                        returnOrderItemDao.deleteByParent(returnOrderId)
                        if (items.isNotEmpty()) {
                            returnOrderItemDao.upsertAll(items)
                        }
                    }
                    ReturnOrderCaptureResult.Saved(returnOrderId)
                }
            }
        }

    /**
     * Modifies a local `DRAFT` Return Order (BC-002).
     *
     * Rejected unless the stored order is `DRAFT` (BR-018). The captured
     * `WarehouseCode`, `createdAt`, `createdBy` and status are preserved; only
     * the editable fields and the item lines are replaced. Local write only.
     */
    suspend fun updateDraft(
        returnOrderId: String,
        draft: ReturnOrderDraft
    ): ReturnOrderCaptureResult = withContext(Dispatchers.IO) {
        val existing = returnOrderDao.getById(returnOrderId)
            ?: return@withContext ReturnOrderCaptureResult.Rejected(listOf(NOT_FOUND))
        if (existing.status != ReturnOrderEntity.STATUS_DRAFT) {
            return@withContext ReturnOrderCaptureResult.Rejected(listOf(NOT_MODIFIABLE))
        }

        when (val resolution = resolve(draft, existing.warehouseCode)) {
            is DraftResolution.Invalid ->
                return@withContext ReturnOrderCaptureResult.Rejected(resolution.errors)

            is DraftResolution.Valid -> {
                val header = existing.copy(
                    customerId = resolution.customer.customerId,
                    customerCode = resolution.customer.customerCode,
                    customerName = resolution.customer.customerName,
                    salesPersonId = resolution.salesPerson?.salesPersonId.orEmpty(),
                    salesPersonName = resolution.salesPerson?.salesPersonName.orEmpty(),
                    driverId = resolution.driver?.driverId.orEmpty(),
                    driverName = resolution.driver?.driverName.orEmpty(),
                    note = draft.note,
                    status = ReturnOrderEntity.STATUS_DRAFT
                )
                val items = resolution.items.mapIndexed { index, item ->
                    item.toEntity(returnOrderId, index + 1)
                }

                database.withTransaction {
                    returnOrderDao.upsert(header)
                    returnOrderItemDao.deleteByParent(returnOrderId)
                    if (items.isNotEmpty()) {
                        returnOrderItemDao.upsertAll(items)
                    }
                }
                ReturnOrderCaptureResult.Saved(returnOrderId)
            }
        }
    }

    /**
     * Removes a local `DRAFT` Return Order and its items (BC-003, GAP-014).
     *
     * Rejected unless the stored order is `DRAFT` (BR-020). The delete is
     * local-only and never propagates to the Cloud or the Main Office.
     */
    suspend fun deleteDraft(returnOrderId: String): ReturnOrderCaptureResult =
        withContext(Dispatchers.IO) {
            val existing = returnOrderDao.getById(returnOrderId)
                ?: return@withContext ReturnOrderCaptureResult.Rejected(listOf(NOT_FOUND))
            if (existing.status != ReturnOrderEntity.STATUS_DRAFT) {
                return@withContext ReturnOrderCaptureResult.Rejected(listOf(NOT_DELETABLE))
            }

            database.withTransaction {
                returnOrderItemDao.deleteByParent(returnOrderId)
                returnOrderDao.deleteById(returnOrderId)
            }
            ReturnOrderCaptureResult.Saved(returnOrderId)
        }

    /** Local header read backing Detail/Edit (SCR-MOB-RO-003/004). */
    suspend fun getOrder(returnOrderId: String): ReturnOrderEntity? =
        withContext(Dispatchers.IO) { returnOrderDao.getById(returnOrderId) }

    /** Local item read for a Return Order, ordered by `noUrut`. */
    suspend fun listItems(returnOrderId: String): List<ReturnOrderItemEntity> =
        withContext(Dispatchers.IO) { returnOrderItemDao.listByParent(returnOrderId) }

    /** Resolved capture payload; every name is sourced from the local caches. */
    private sealed class DraftResolution {
        data class Valid(
            val customer: CustomerEntity,
            val salesPerson: SalesPersonEntity?,
            val driver: DriverEntity?,
            val items: List<ResolvedItem>
        ) : DraftResolution()

        data class Invalid(val errors: List<String>) : DraftResolution()
    }

    private data class ResolvedItem(
        val brgId: String,
        val brgCode: String,
        val brgName: String,
        val qty: Double,
        val satId: String,
        val jenisRetur: String
    ) {
        fun toEntity(returnOrderId: String, noUrut: Int) = ReturnOrderItemEntity(
            returnOrderId = returnOrderId,
            noUrut = noUrut,
            brgId = brgId,
            brgCode = brgCode,
            brgName = brgName,
            qty = qty,
            satId = satId,
            jenisRetur = jenisRetur
        )
    }

    /**
     * Offline guardrails (§18.3): validates the capture payload against the
     * local reference caches and resolves the names to persist. Returns every
     * violation so the screen can surface them; writes nothing.
     */
    private suspend fun resolve(
        draft: ReturnOrderDraft,
        warehouseCode: String
    ): DraftResolution {
        val errors = mutableListOf<String>()

        // BR-005/006 — Warehouse is mandatory and belongs to one Warehouse.
        if (warehouseCode.isBlank()) {
            errors.add(WAREHOUSE_REQUIRED)
        }

        // BR-001/002, GAP-004 — Customer mandatory, selected from the cache.
        val customer: CustomerEntity?
        if (draft.customerId.isBlank()) {
            customer = null
            errors.add(CUSTOMER_REQUIRED)
        } else {
            customer = customerDao.getById(draft.customerId)
            if (customer == null) errors.add(CUSTOMER_NOT_CACHED)
        }

        // BR-013–016, ADR-RO-005 — Salesman/Driver optional; when supplied they
        // must be a cached reference so the denormalized name is trustworthy.
        val salesPerson: SalesPersonEntity?
        if (draft.salesPersonId.isBlank()) {
            salesPerson = null
        } else {
            salesPerson = salesPersonDao.getById(draft.salesPersonId)
            if (salesPerson == null) errors.add(SALESPERSON_NOT_CACHED)
        }

        val driver: DriverEntity?
        if (draft.driverId.isBlank()) {
            driver = null
        } else {
            driver = driverDao.getById(draft.driverId)
            if (driver == null) errors.add(DRIVER_NOT_CACHED)
        }

        // BR-012 — at least one Return Order Item.
        if (draft.items.isEmpty()) {
            errors.add(ITEM_REQUIRED)
        }

        val resolvedItems = mutableListOf<ResolvedItem>()
        draft.items.forEachIndexed { index, line ->
            val row = "Baris ${index + 1}"

            // BR-007/008 — item mandatory and must exist in the cached Item Master.
            val item: BarangEntity?
            if (line.brgId.isBlank()) {
                item = null
                errors.add("$row: $ITEM_ID_REQUIRED")
            } else {
                item = barangDao.getById(line.brgId)
                if (item == null) errors.add("$row: $ITEM_NOT_CACHED")
            }

            // BR-010 — Quantity greater than zero.
            if (line.qty <= 0.0) {
                errors.add("$row: $QTY_INVALID")
            }

            // BR-011, ADR-RO-003 — unit mandatory, taken from the item's cached
            // unit data. Recorded verbatim as `satId`; no small-unit conversion.
            val cachedUnits = item?.cachedUnits().orEmpty()
            when {
                line.satId.isBlank() -> errors.add("$row: $UNIT_REQUIRED")
                item != null && line.satId !in cachedUnits ->
                    errors.add("$row: $UNIT_INVALID")
            }

            // ADR-RO-004 — Return Type is exactly BAGUS | RUSAK, per line.
            if (line.jenisRetur != ReturnOrderItemEntity.JENIS_RETUR_BAGUS &&
                line.jenisRetur != ReturnOrderItemEntity.JENIS_RETUR_RUSAK
            ) {
                errors.add("$row: $RETURN_TYPE_INVALID")
            }

            if (item != null) {
                resolvedItems.add(
                    ResolvedItem(
                        brgId = item.brgId,
                        brgCode = item.brgCode,
                        brgName = item.brgName,
                        // P-09/ADR-RO-003 — physical quantity recorded as captured.
                        qty = line.qty,
                        satId = line.satId,
                        jenisRetur = line.jenisRetur
                    )
                )
            }
        }

        return if (errors.isNotEmpty() || customer == null) {
            DraftResolution.Invalid(errors)
        } else {
            DraftResolution.Valid(
                customer = customer,
                salesPerson = salesPerson,
                driver = driver,
                items = resolvedItems
            )
        }
    }

    companion object {
        /** The item's own cached units (IR-01); blanks are ignored. */
        private fun BarangEntity.cachedUnits(): List<String> =
            listOf(satKecil, satBesar).filter { it.isNotBlank() }

        const val NOT_FOUND = "Return Order tidak ditemukan."
        const val NOT_MODIFIABLE =
            "Return Order yang sudah disinkronkan tidak dapat diubah."
        const val NOT_DELETABLE =
            "Return Order yang sudah disinkronkan tidak dapat dihapus."
        const val WAREHOUSE_REQUIRED = "Warehouse wajib diisi."
        const val CUSTOMER_REQUIRED = "Customer wajib dipilih."
        const val CUSTOMER_NOT_CACHED = "Customer tidak ditemukan pada cache lokal."
        const val SALESPERSON_NOT_CACHED =
            "Salesman tidak ditemukan pada cache lokal."
        const val DRIVER_NOT_CACHED = "Driver tidak ditemukan pada cache lokal."
        const val ITEM_REQUIRED =
            "Return Order harus memiliki minimal satu item."
        const val ITEM_ID_REQUIRED = "Item wajib dipilih."
        const val ITEM_NOT_CACHED = "Item tidak ditemukan pada cache lokal."
        const val QTY_INVALID = "Qty harus lebih dari 0."
        const val UNIT_REQUIRED = "Satuan wajib dipilih."
        const val UNIT_INVALID = "Satuan tidak sesuai dengan data item."
        const val RETURN_TYPE_INVALID = "Jenis retur harus BAGUS atau RUSAK."
    }
}
