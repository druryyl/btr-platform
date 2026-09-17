package com.elsasa.bgud.network

import com.google.gson.GsonBuilder
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory

/**
 * Retrofit/OkHttp/Gson client factory (mandated stack TQ-7).
 *
 * - JWT is attached by [AuthInterceptor] on every request (ADR-002/ADR-003);
 *   no `ServerId` is ever attached (ADR-007, IR-09).
 * - HTTP logging never includes headers (the `Authorization` header must never
 *   be logged) and is off by default; enable only for local diagnostics.
 * - `baseUrl` is supplied by the caller (transport/security is unresolved,
 *   C-3/R-03) — no environment URL is hardcoded here.
 */
object ApiClient {

    fun create(
        baseUrl: String,
        tokenProvider: () -> String?,
        enableLogging: Boolean = false
    ): BtradeApiService {
        val okHttpBuilder = OkHttpClient.Builder()
            .addInterceptor(AuthInterceptor(tokenProvider))

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
