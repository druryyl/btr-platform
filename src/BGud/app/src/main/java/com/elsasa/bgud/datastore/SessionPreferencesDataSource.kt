package com.elsasa.bgud.datastore

import android.content.Context
import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.longPreferencesKey
import androidx.datastore.preferences.core.stringPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.flow.map

private val Context.sessionPreferencesDataStore: DataStore<Preferences> by preferencesDataStore(name = "session_preferences")

/**
 * Session + sync-state store (Architecture §6.4 `session_preferences`).
 *
 * Holds: token, user, warehouseCode, officeCode, last sync timestamps.
 * `warehouseCode` (not `ServerId`) is stored on queued requests (IR-09);
 * switching warehouse requires re-authentication and the local cache is
 * replaced for the new tenant (S5.3).
 */
class SessionPreferencesDataSource(private val context: Context) {

    companion object {
        private val TOKEN_KEY = stringPreferencesKey("token")
        private val USER_ID_KEY = stringPreferencesKey("user_id")
        private val WAREHOUSE_CODE_KEY = stringPreferencesKey("warehouse_code")
        private val OFFICE_CODE_KEY = stringPreferencesKey("office_code")
        private val LAST_BARANG_SYNC_KEY = longPreferencesKey("last_barang_sync")
        private val LAST_BARCODE_SYNC_KEY = longPreferencesKey("last_barcode_sync")
    }

    val token: Flow<String?> = context.sessionPreferencesDataStore.data
        .map { it[TOKEN_KEY] }

    val userId: Flow<String?> = context.sessionPreferencesDataStore.data
        .map { it[USER_ID_KEY] }

    val warehouseCode: Flow<String?> = context.sessionPreferencesDataStore.data
        .map { it[WAREHOUSE_CODE_KEY] }

    val officeCode: Flow<String?> = context.sessionPreferencesDataStore.data
        .map { it[OFFICE_CODE_KEY] }

    val lastBarangSync: Flow<Long> = context.sessionPreferencesDataStore.data
        .map { it[LAST_BARANG_SYNC_KEY] ?: 0L }

    val lastBarcodeSync: Flow<Long> = context.sessionPreferencesDataStore.data
        .map { it[LAST_BARCODE_SYNC_KEY] ?: 0L }

    suspend fun saveSession(
        token: String,
        userId: String,
        warehouseCode: String,
        officeCode: String
    ) {
        context.sessionPreferencesDataStore.edit { preferences ->
            preferences[TOKEN_KEY] = token
            preferences[USER_ID_KEY] = userId
            preferences[WAREHOUSE_CODE_KEY] = warehouseCode
            preferences[OFFICE_CODE_KEY] = officeCode
        }
    }

    suspend fun clearSession() {
        context.sessionPreferencesDataStore.edit { preferences ->
            preferences.remove(TOKEN_KEY)
            preferences.remove(USER_ID_KEY)
            preferences.remove(WAREHOUSE_CODE_KEY)
            preferences.remove(OFFICE_CODE_KEY)
        }
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

    suspend fun getToken(): String? {
        return context.sessionPreferencesDataStore.data.first()[TOKEN_KEY]
    }
}
