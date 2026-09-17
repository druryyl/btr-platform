package com.elsasa.bgud.model

import androidx.room.Entity
import androidx.room.Index
import androidx.room.PrimaryKey

/**
 * Offline registration intent queue (Architecture §6.4).
 *
 * Local writes are QUEUED INTENT, not state (P-08): a row becomes
 * authoritative only when the Main Office accepts it during
 * synchronization. `warehouseCode` (never a `ServerId`) binds the request
 * to the operational location under which it was captured (IR-09); queued
 * requests are held on warehouse switch, never re-homed.
 */
@Entity(
    tableName = "barcode_registration_request_entity",
    indices = [
        Index(value = ["status"])
    ]
)
data class BarcodeRegistrationRequestEntity(
    /** Device-generated idempotency key; maps to Cloud `ClientRequestId`. */
    @PrimaryKey
    val clientRequestId: String,
    val barcodeValue: String,
    val brgId: String,
    /** '' when absent (BR-005). */
    val satuan: String = "",
    /** PENDING | SYNCED | REJECTED */
    val status: String = STATUS_PENDING,
    /** Operational location code, e.g. GAMPING / CONCAT / MAGELANG. */
    val warehouseCode: String = "",
    val createdAt: Long = 0L,
    /** Rejection reason returned by the Cloud. */
    val serverNote: String = ""
) {
    companion object {
        const val STATUS_PENDING = "PENDING"
        const val STATUS_SYNCED = "SYNCED"
        const val STATUS_REJECTED = "REJECTED"
    }
}
