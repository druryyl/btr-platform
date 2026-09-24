package com.elsasa.bgud.datastore

/**
 * Session/warehouse binding model (TD-10, IR-09).
 *
 * A session binds the device to exactly one operational location. The binding
 * carries `locationId` — never a `ServerId` (IR-09; the device must not store
 * or send `ServerId` as a command input on the four session-context routes).
 * Switching Gudang requires ending the current session; previously queued
 * records keep their original `locationId` binding and are never re-homed
 * (FEATURE §6.8, §8).
 *
 * Source of truth for the stored values is [SessionPreferencesDataSource];
 * this type is the pure binding rule used before submitting a queued request
 * during a sync run.
 */
data class SessionBinding(
    val userId: String,
    val locationId: String
) {
    /**
     * IR-09: a queued request may be submitted only by a session bound to the
     * same operational location under which it was captured. A request with a
     * blank `locationId` is never submittable.
     */
    fun canSubmit(requestLocationId: String): Boolean {
        if (requestLocationId.isBlank() || locationId.isBlank()) return false
        return requestLocationId == locationId
    }
}
