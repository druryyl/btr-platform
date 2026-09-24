package com.elsasa.bgud.datastore

import android.content.Context
import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.emptyPreferences
import androidx.datastore.preferences.core.longPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.catch
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.flow.map

private val Context.sessionPreferencesDataStore: DataStore<Preferences> by preferencesDataStore(name = "session_preferences")

/**
 * Local session + sync-state store (TD-10).
 *
 * The local session replaces the JWT-based session and stores `google_email`,
 * `user_id`, `user_name`, `role_id`, `location_id`, and `server_id` (TD-10).
 * The former `token` and `office_code` keys are removed: no code path stores,
 * reads, or sends a JWT or password (ARCHITECTURE §10). Session validity is
 * "`google_email` present" (see [SessionState.isValid]), so a legacy install
 * holding only a `token` key is treated as signed out and routed to sign-in.
 *
 * `locationId` (not `ServerId`) binds queued requests (IR-09); `serverId` is
 * stored only to supply the legacy `{serverId}` read routes (TD-08). Switching
 * Gudang clears the session and starts a new one; queued records are never
 * re-homed.
 *
 * Return Order reference timestamps (S4.3): each advances only on the
 * committed download of its own reference type, mirroring the
 * Barang/Barcode timestamp pattern. Timestamps survive logout/warehouse
 * change ([clearSession] removes only the session keys).
 */
class SessionPreferencesDataSource(private val context: Context) {

    companion object {
        private val LAST_BARANG_SYNC_KEY = longPreferencesKey("last_barang_sync")
        private val LAST_BARCODE_SYNC_KEY = longPreferencesKey("last_barcode_sync")
        private val LAST_CUSTOMER_SYNC_KEY = longPreferencesKey("last_customer_sync")
        private val LAST_SALESPERSON_SYNC_KEY = longPreferencesKey("last_salesperson_sync")
        private val LAST_DRIVER_SYNC_KEY = longPreferencesKey("last_driver_sync")
    }

    /**
     * Single source for all preference reads. A corrupted preferences file
     * would otherwise throw `IOException` and leave the session flow
     * unresolved forever (the startup gate would spin); recover with empty
     * preferences so the app degrades to "no session" instead.
     */
    private val preferences: Flow<Preferences> = context.sessionPreferencesDataStore.data
        .catch { emit(emptyPreferences()) }

    val googleEmail: Flow<String?> = preferences
        .map { it[SessionPreferences.GoogleEmailKey] }

    val userId: Flow<String?> = preferences
        .map { it[SessionPreferences.UserIdKey] }

    val userName: Flow<String?> = preferences
        .map { it[SessionPreferences.UserNameKey] }

    val roleId: Flow<String?> = preferences
        .map { it[SessionPreferences.RoleIdKey] }

    val locationId: Flow<String?> = preferences
        .map { it[SessionPreferences.LocationIdKey] }

    val serverId: Flow<String?> = preferences
        .map { it[SessionPreferences.ServerIdKey] }

    val lastBarangSync: Flow<Long> = preferences
        .map { it[LAST_BARANG_SYNC_KEY] ?: 0L }

    val lastBarcodeSync: Flow<Long> = preferences
        .map { it[LAST_BARCODE_SYNC_KEY] ?: 0L }

    val lastCustomerSync: Flow<Long> = preferences
        .map { it[LAST_CUSTOMER_SYNC_KEY] ?: 0L }

    val lastSalesPersonSync: Flow<Long> = preferences
        .map { it[LAST_SALESPERSON_SYNC_KEY] ?: 0L }

    val lastDriverSync: Flow<Long> = preferences
        .map { it[LAST_DRIVER_SYNC_KEY] ?: 0L }

    /**
     * Persist the established local session (TD-10). Called only after
     * `POST api/session/resolve` succeeded and a Gudang was selected.
     */
    suspend fun saveSession(
        googleEmail: String,
        userId: String,
        userName: String,
        roleId: String,
        locationId: String,
        serverId: String
    ) {
        context.sessionPreferencesDataStore.edit { preferences ->
            SessionPreferences.write(
                preferences,
                SessionState(
                    googleEmail = googleEmail,
                    userId = userId,
                    userName = userName,
                    roleId = roleId,
                    locationId = locationId,
                    serverId = serverId
                )
            )
        }
    }

    /**
     * Clear the local session (logout, Gudang change, or Cloud 409). Only the
     * session keys are removed; queued records and sync timestamps are
     * untouched (IR-09).
     */
    suspend fun clearSession() {
        context.sessionPreferencesDataStore.edit { preferences ->
            SessionPreferences.clear(preferences)
        }
    }

    /** The signed-in Google email, or null when there is no session. */
    suspend fun getGoogleEmail(): String? {
        return preferences.first()[SessionPreferences.GoogleEmailKey]
    }

    suspend fun setLastBarangSync(timestampMillis: Long) {
        context.sessionPreferencesDataStore.edit { preferences ->
            preferences[LAST_BARANG_SYNC_KEY] = timestampMillis
        }
    }

    suspend fun setLastBarcodeSync(timestampMillis: Long) {
        context.sessionPreferencesDataStore.edit { preferences ->
            preferences[LAST_BARCODE_SYNC_KEY] = timestampMillis
        }
    }

    suspend fun setLastCustomerSync(timestampMillis: Long) {
        context.sessionPreferencesDataStore.edit { preferences ->
            preferences[LAST_CUSTOMER_SYNC_KEY] = timestampMillis
        }
    }

    suspend fun setLastSalesPersonSync(timestampMillis: Long) {
        context.sessionPreferencesDataStore.edit { preferences ->
            preferences[LAST_SALESPERSON_SYNC_KEY] = timestampMillis
        }
    }

    suspend fun setLastDriverSync(timestampMillis: Long) {
        context.sessionPreferencesDataStore.edit { preferences ->
            preferences[LAST_DRIVER_SYNC_KEY] = timestampMillis
        }
    }
}
