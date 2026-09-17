package com.elsasa.bgud.model

import androidx.room.Entity
import androidx.room.Index
import androidx.room.PrimaryKey

/**
 * Offline Return Order capture (Return Order Architecture §6.4, S4.1).
 *
 * Device lifecycle is Draft -> Synced only (ADR-RO-006); no import outcome
 * or ReturnOrderNo is ever stored here. `returnOrderId` is the device-generated
 * ULID idempotency key (IR-RO-01).
 */
@Entity(
    tableName = "return_order_entity",
    indices = [
        Index(value = ["status"]),
        Index(value = ["customerName"])
    ]
)
data class ReturnOrderEntity(
    @PrimaryKey
    val returnOrderId: String,
    val customerId: String = "",
    val customerCode: String = "",
    val customerName: String = "",
    /** Session-bound operational location code (BR-005/006, IR-RO-04). */
    val warehouseCode: String = "",
    /** Optional ('' when absent, ADR-RO-005). */
    val salesPersonId: String = "",
    val salesPersonName: String = "",
    /** Optional ('' when absent, ADR-RO-005). */
    val driverId: String = "",
    val driverName: String = "",
    val note: String = "",
    /** DRAFT | SYNCED */
    val status: String = STATUS_DRAFT,
    val createdAt: Long = 0L,
    val createdBy: String = ""
) {
    companion object {
        const val STATUS_DRAFT = "DRAFT"
        const val STATUS_SYNCED = "SYNCED"
    }
}
