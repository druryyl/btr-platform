package com.elsasa.bgud.network

import okhttp3.Interceptor
import okhttp3.Response

/**
 * Session-context interceptor (TD-12), replacing the former JWT bearer
 * `AuthInterceptor`.
 *
 * Every request carries the local session context explicitly when a session
 * exists:
 *
 * | Header | Value |
 * | ------ | ----- |
 * | `X-Session-Location` | selected `locationId` (`GAMPING`/`CONCAT`/`MAGELANG`) |
 * | `X-Session-Actor` | signed-in Google email |
 *
 * No `Authorization` header is ever attached, and no `ServerId` is ever added
 * (ARCHITECTURE §10). The account resolution call (`POST api/session/resolve`)
 * carries the email in its body, so it is issued with no session context (the
 * provider returns null before a session exists).
 *
 * A Cloud HTTP 409 means the carried actor no longer resolves to a valid BTR
 * user — the session has ended (TD-13). The interceptor reports that through
 * [onSessionInvalidated] so the caller can clear the local session and return
 * the operator to sign-in. The values are supplied by the caller (a fixed
 * provider), so no DataStore I/O blocks the OkHttp dispatcher.
 */
class SessionContextInterceptor(
    private val sessionProvider: () -> SessionContext?,
    private val onSessionInvalidated: () -> Unit = {}
) : Interceptor {

    /** The local session context attached to Cloud requests. */
    data class SessionContext(
        val locationId: String,
        val actorEmail: String
    )

    override fun intercept(chain: Interceptor.Chain): Response {
        val request = chain.request()
        val session = sessionProvider()
        val proceeded = if (session == null) {
            chain.proceed(request)
        } else {
            val builder = request.newBuilder()
            if (session.locationId.isNotBlank()) {
                builder.header(HEADER_LOCATION, session.locationId)
            }
            if (session.actorEmail.isNotBlank()) {
                builder.header(HEADER_ACTOR, session.actorEmail)
            }
            chain.proceed(builder.build())
        }

        if (proceeded.code == HTTP_CONFLICT) {
            onSessionInvalidated()
        }
        return proceeded
    }

    companion object {
        /** §10 — fixed session-context header names. */
        const val HEADER_LOCATION = "X-Session-Location"
        const val HEADER_ACTOR = "X-Session-Actor"

        /** TD-13 — the actor mapping was removed; the session has ended. */
        const val HTTP_CONFLICT = 409
    }
}
