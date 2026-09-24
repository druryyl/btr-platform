package com.elsasa.bgud.datastore

import androidx.datastore.preferences.core.longPreferencesKey
import androidx.datastore.preferences.core.mutablePreferencesOf
import androidx.datastore.preferences.core.stringPreferencesKey
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test

/**
 * TD-10 local session store and validity rule.
 *
 * Exercises the pure preference mapping directly (no Android `Context`), so
 * the stored key set, the round-trip, and the "validity = google_email
 * present" rule are covered without a device.
 */
class SessionPreferencesTest {

    @Test
    fun writeThenSnapshot_roundTripsEveryTd10Key() {
        val preferences = mutablePreferencesOf()
        val state = SessionState(
            googleEmail = "operator@gmail.com",
            userId = "U-1",
            userName = "Operator",
            roleId = "WHS",
            locationId = "GAMPING",
            serverId = "JOGJA"
        )

        SessionPreferences.write(preferences, state)

        assertEquals(state, SessionPreferences.snapshot(preferences))
    }

    @Test
    fun snapshot_readsTheTd10KeyNames() {
        val preferences = mutablePreferencesOf(
            stringPreferencesKey("google_email") to "a@b.com",
            stringPreferencesKey("user_id") to "U-2",
            stringPreferencesKey("user_name") to "Name",
            stringPreferencesKey("role_id") to "WHS",
            stringPreferencesKey("location_id") to "CONCAT",
            stringPreferencesKey("server_id") to "JOGJA"
        )

        val state = SessionPreferences.snapshot(preferences)

        assertEquals("a@b.com", state.googleEmail)
        assertEquals("U-2", state.userId)
        assertEquals("Name", state.userName)
        assertEquals("WHS", state.roleId)
        assertEquals("CONCAT", state.locationId)
        assertEquals("JOGJA", state.serverId)
        assertTrue(state.isValid)
    }

    @Test
    fun sessionValidity_requiresANonBlankGoogleEmail() {
        assertTrue(
            SessionState("a@b.com", "", "", "", "", "").isValid
        )
        assertFalse(
            SessionState("", "U-1", "", "", "", "").isValid
        )
        assertFalse(
            SessionState("   ", "U-1", "", "", "", "").isValid
        )
    }

    @Test
    fun legacyInstallHoldingOnlyTheRemovedTokenKey_isNotAValidSession() {
        val preferences = mutablePreferencesOf(
            stringPreferencesKey("token") to "legacy.jwt.value"
        )

        val state = SessionPreferences.snapshot(preferences)

        // TD-10: the removed `token` key is never consulted; no google_email
        // means "signed out", so the legacy install routes to sign-in.
        assertFalse(state.isValid)
        assertEquals("", state.googleEmail)
    }

    @Test
    fun clear_removesSessionKeysButRetainsSyncTimestamps() {
        val preferences = mutablePreferencesOf()
        SessionPreferences.write(
            preferences,
            SessionState("a@b.com", "U-1", "Name", "WHS", "GAMPING", "JOGJA")
        )
        val barcodeSyncKey = longPreferencesKey("last_barcode_sync")
        preferences[barcodeSyncKey] = 1234L

        SessionPreferences.clear(preferences)

        assertFalse(SessionPreferences.snapshot(preferences).isValid)
        assertEquals(1234L, preferences[barcodeSyncKey])
    }
}
