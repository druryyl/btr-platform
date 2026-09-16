package com.elsasa.bgud.datastore

/**
 * Session/warehouse binding model (Architecture §9.2, ADR-007, IR-09).
 *
 * A session binds the device to exactly one operational location. The binding
 * carries `warehouseCode` — never a `ServerId` (IR-09; the device must not
 * store or send `ServerId` as a command input). Switching warehouse requires
 * re-authentication; the local cache is replaced for the new tenant and
 * previously queued requests are held, never re-homed (S5.3 owns the switch).
 *
 * Source of truth for the stored values is [SessionPreferencesDataSource]
 * (S5.1); this type is the pure binding rule used before submitting a queued
 * request during a sync run (S5.3).
 */
data class SessionBinding(
    val userId: String,
    val warehouseCode: String,
    val officeCode: String
) {
    /**
     * IR-09: a queued request may be submitted only by a session bound to the
     * same operational location under which it was captured. A request with a
     * blank `warehouseCode` is never submittable.
     */
    fun canSubmit(requestWarehouseCode: String): Boolean {
        if (requestWarehouseCode.isBlank() || warehouseCode.isBlank()) return false
        return requestWarehouseCode == warehouseCode
    }
}
