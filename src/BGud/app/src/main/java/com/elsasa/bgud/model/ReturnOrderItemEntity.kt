package com.elsasa.bgud.model

import androidx.room.Entity

/**
 * Return Order line items (Return Order Architecture §6.4, S4.1).
 *
 * Composite PK (returnOrderId, noUrut). Quantity is the physical-unit
 * quantity (IR-RO-03); no small-unit normalization anywhere (P-09).
 * `jenisRetur` is exactly BAGUS | RUSAK (ADR-RO-004).
 */
@Entity(
    tableName = "return_order_item_entity",
    primaryKeys = ["returnOrderId", "noUrut"]
)
data class ReturnOrderItemEntity(
    val returnOrderId: String,
    val noUrut: Int,
    val brgId: String = "",
    val brgCode: String = "",
    val brgName: String = "",
    val qty: Double = 0.0,
    val satId: String = "",
    val jenisRetur: String = ""
) {
    companion object {
        const val JENIS_RETUR_BAGUS = "BAGUS"
        const val JENIS_RETUR_RUSAK = "RUSAK"
    }
}
