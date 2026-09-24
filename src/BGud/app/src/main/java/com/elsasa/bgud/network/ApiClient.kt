package com.elsasa.bgud.network

import com.google.gson.GsonBuilder
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.util.concurrent.TimeUnit

/**
 * Retrofit/OkHttp/Gson client factory (mandated stack TQ-7).
 *
 * - The session context is attached by [SessionContextInterceptor] on every
 *   request when a session exists (TD-12); no `Authorization` header is ever
 *   sent and no `ServerId` is ever attached (ARCHITECTURE §10).
 * - The account resolution call (`POST api/session/resolve`) passes no
 *   session-context provider: it carries only the body email (TD-12).
 * - HTTP logging never includes headers and is off by default; enable only for
 *   local diagnostics.
 * - `baseUrl` is supplied by the caller; no environment URL is hardcoded here
 *   (§10 single composition-root base URL).
 */
object ApiClient {

    fun create(
        baseUrl: String,
        sessionContextProvider: () -> SessionContextInterceptor.SessionContext? = { null },
        onSessionInvalidated: () -> Unit = {},
        enableLogging: Boolean = false
    ): BtradeApiService {
        val okHttpBuilder = OkHttpClient.Builder()
            .addInterceptor(
                SessionContextInterceptor(sessionContextProvider, onSessionInvalidated)
            )
            .connectTimeout(30, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .writeTimeout(30, TimeUnit.SECONDS)

        if (enableLogging) {
            okHttpBuilder.addInterceptor(
                HttpLoggingInterceptor().apply { level = HttpLoggingInterceptor.Level.BASIC }
            )
        }

        return Retrofit.Builder()
            .baseUrl(baseUrl)
            .client(okHttpBuilder.build())
            .addConverterFactory(GsonConverterFactory.create(GsonBuilder().create()))
            .build()
            .create(BtradeApiService::class.java)
    }
}
