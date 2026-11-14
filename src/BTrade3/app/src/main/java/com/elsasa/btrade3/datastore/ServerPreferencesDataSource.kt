package com.elsasa.btrade3.datastore


import android.content.Context
import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.stringPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import com.elsasa.btrade3.util.ApiUrl
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.flow.map

private val Context.serverPreferencesDataStore: DataStore<Preferences> by preferencesDataStore(name = "server_preferences")

class ServerPreferencesDataSource(private val context: Context) {
    companion object {
        private val SERVER_TARGET_KEY = stringPreferencesKey("server_target")
        private val API_BASE_URL_KEY = stringPreferencesKey("api_base_url")
        const val DEFAULT_SERVER = "JOG" // Default to Jogja
        const val DEFAULT_API_BASE_URL = ApiUrl.DEFAULT_BASE_URL
    }

    val serverTarget: Flow<String> = context.serverPreferencesDataStore.data
        .map { preferences ->
            preferences[SERVER_TARGET_KEY] ?: DEFAULT_SERVER
        }

    val apiBaseUrl: Flow<String> = context.serverPreferencesDataStore.data
        .map { preferences ->
            preferences[API_BASE_URL_KEY]?.takeIf { it.isNotBlank() } ?: DEFAULT_API_BASE_URL
        }

    suspend fun setServerTarget(server: String) {
        context.serverPreferencesDataStore.edit { preferences ->
            preferences[SERVER_TARGET_KEY] = server
        }
    }

    suspend fun getServerTarget(): String {
        return context.serverPreferencesDataStore.data.first()[SERVER_TARGET_KEY] ?: DEFAULT_SERVER
    }

    suspend fun setApiBaseUrl(baseUrl: String) {
        context.serverPreferencesDataStore.edit { preferences ->
            preferences[API_BASE_URL_KEY] = baseUrl
        }
    }

    suspend fun getApiBaseUrl(): String {
        return context.serverPreferencesDataStore.data.first()[API_BASE_URL_KEY]
            ?.takeIf { it.isNotBlank() } ?: DEFAULT_API_BASE_URL
    }
}