package com.elsasa.bgud.model

import androidx.room.ColumnInfo
import androidx.room.Entity
import androidx.room.Index
import androidx.room.PrimaryKey

/**
 * Active barcode cache for the bound Office (Architecture §6.4).
 *
 * Holds ONLY Active barcodes (TQ-3, TQ-6); deactivation is a removal,
 * never a local state change. Scoped to the single Office bound to the
 * session (OQ-2, IR-09).
 */
@Entity(
    tableName = "barcode_entity",
    indices = [
        Index(value = ["barcodeValueKey"], unique = true),
        Index(value = ["brgId"])
    ]
)
data class BarcodeEntity(
    @PrimaryKey
    val brgBarcodeId: String,
    val barcodeValue: String,
    /** UPPER(normalized) lookup key; never compare the raw value. */
    val barcodeValueKey: String,
    val brgId: String,
    val brgCode: String,
    val brgName: String,
    /** '' when no packaging level recorded (BR-005). */
    val satuan: String = ""
) {
    companion object {
        fun keyOf(barcodeValue: String): String {
            return com.elsasa.bgud.util.BarcodeNormalization.toKey(barcodeValue)
        }
    }
}
