package com.elsasa.bgud.network

import okhttp3.Interceptor
import okhttp3.Response

/**
 * JWT bearer interceptor (Architecture §9.1, ADR-002/ADR-003).
 *
 * Every request carries `Authorization: Bearer <token>` when a token is
 * available; no `ServerId` is ever added to a request (ADR-007, P-06, IR-09).
 * The login call (I-07) is `AllowAnonymous` — sending no header when no token
 * is cached is the correct behavior for that call.
 *
 * The token is supplied by the caller (wired to
 * `SessionPreferencesDataSource.getToken()` with caching in S5.4). This class
 * performs no I/O itself so it stays unit-testable and never blocks on
 * DataStore inside the OkHttp dispatcher.
 */
class AuthInterceptor(
    private val tokenProvider: () -> String?
) : Interceptor {

    override fun intercept(chain: Interceptor.Chain): Response {
        val request = chain.request()
        val token = tokenProvider()?.takeIf { it.isNotBlank() }
            ?: return chain.proceed(request)
        return chain.proceed(
            request.newBuilder()
                .header("Authorization", "Bearer $token")
                .build()
        )
    }
}
