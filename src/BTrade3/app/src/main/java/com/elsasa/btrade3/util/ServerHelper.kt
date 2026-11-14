package com.elsasa.btrade3.util


import android.content.Context
import com.elsasa.btrade3.datastore.ServerPreferencesDataSource

object ServerHelper {

    suspend fun getSelectedServer(context: Context): String {
        val dataSource = ServerPreferencesDataSource(context.applicationContext)
        return dataSource.getServerTarget()
    }

    suspend fun setSelectedServer(context: Context, server: String) {
        val dataSource = ServerPreferencesDataSource(context.applicationContext)
        dataSource.setServerTarget(server)
    }

    suspend fun getServerTarget(context: Context): String {
        return getSelectedServer(context)
    }

    suspend fun getApiBaseUrl(context: Context): String {
        val dataSource = ServerPreferencesDataSource(context.applicationContext)
        return dataSource.getApiBaseUrl()
    }

    suspend fun setApiBaseUrl(context: Context, baseUrl: String) {
        val dataSource = ServerPreferencesDataSource(context.applicationContext)
        dataSource.setApiBaseUrl(ApiUrl.normalize(baseUrl))
    }

    fun normalizeApiBaseUrl(baseUrl: String): String {
        return ApiUrl.normalize(baseUrl)
    }
}
