package com.elsasa.bgud.network

import okhttp3.Call
import okhttp3.Connection
import okhttp3.Interceptor
import okhttp3.Protocol
import okhttp3.Request
import okhttp3.Response
import okhttp3.ResponseBody.Companion.toResponseBody
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertNull
import org.junit.Assert.assertTrue
import org.junit.Test
import java.util.concurrent.TimeUnit

/**
 * TD-12 session-context interceptor and TD-13 409 handling.
 *
 * Verifies the fixed header names (`X-Session-Location` / `X-Session-Actor`)
 * are attached only when a session exists, that no `Authorization` header is
 * ever added, and that a Cloud 409 reports the session as invalidated.
 */
class SessionContextInterceptorTest {

    private val request = Request.Builder().url("http://example.test/api/barcodes/sync").build()

    private fun responseFor(request: Request, code: Int): Response = Response.Builder()
        .request(request)
        .protocol(Protocol.HTTP_1_1)
        .code(code)
        .message("message")
        .body("".toResponseBody(null))
        .build()

    @Test
    fun attachesBothSessionHeaders_whenASessionExists() {
        val chain = FakeChain(request, responseFor(request, 200))
        val interceptor = SessionContextInterceptor(
            sessionProvider = {
                SessionContextInterceptor.SessionContext(
                    locationId = "GAMPING",
                    actorEmail = "operator@gmail.com"
                )
            }
        )

        interceptor.intercept(chain)

        val proceeded = requireNotNull(chain.proceeded)
        assertEquals("GAMPING", proceeded.header(SessionContextInterceptor.HEADER_LOCATION))
        assertEquals(
            "operator@gmail.com",
            proceeded.header(SessionContextInterceptor.HEADER_ACTOR)
        )
        assertNull(proceeded.header("Authorization"))
    }

    @Test
    fun attachesOnlyTheLocation_whenTheActorIsBlank() {
        val chain = FakeChain(request, responseFor(request, 200))
        val interceptor = SessionContextInterceptor(
            sessionProvider = {
                SessionContextInterceptor.SessionContext(locationId = "CONCAT", actorEmail = "")
            }
        )

        interceptor.intercept(chain)

        val proceeded = requireNotNull(chain.proceeded)
        assertEquals("CONCAT", proceeded.header(SessionContextInterceptor.HEADER_LOCATION))
        assertNull(proceeded.header(SessionContextInterceptor.HEADER_ACTOR))
    }

    @Test
    fun attachesNoHeaders_whenThereIsNoSession() {
        val chain = FakeChain(request, responseFor(request, 200))
        val interceptor = SessionContextInterceptor(sessionProvider = { null })

        interceptor.intercept(chain)

        val proceeded = requireNotNull(chain.proceeded)
        assertNull(proceeded.header(SessionContextInterceptor.HEADER_LOCATION))
        assertNull(proceeded.header(SessionContextInterceptor.HEADER_ACTOR))
        assertNull(proceeded.header("Authorization"))
    }

    @Test
    fun reportsSessionInvalidated_onHttp409() {
        val chain = FakeChain(request, responseFor(request, 409))
        var invalidated = false
        val interceptor = SessionContextInterceptor(
            sessionProvider = {
                SessionContextInterceptor.SessionContext("GAMPING", "operator@gmail.com")
            },
            onSessionInvalidated = { invalidated = true }
        )

        val response = interceptor.intercept(chain)

        assertEquals(409, response.code)
        assertTrue(invalidated)
    }

    @Test
    fun doesNotReportSessionInvalidated_onSuccess() {
        val chain = FakeChain(request, responseFor(request, 200))
        var invalidated = false
        val interceptor = SessionContextInterceptor(
            sessionProvider = {
                SessionContextInterceptor.SessionContext("GAMPING", "operator@gmail.com")
            },
            onSessionInvalidated = { invalidated = true }
        )

        interceptor.intercept(chain)

        assertFalse(invalidated)
    }

    /** Minimal [Interceptor.Chain] capturing the request actually proceeded. */
    private class FakeChain(
        private val request: Request,
        private val response: Response
    ) : Interceptor.Chain {
        var proceeded: Request? = null

        override fun request(): Request = request

        override fun proceed(request: Request): Response {
            proceeded = request
            return response
        }

        override fun connection(): Connection? = null

        override fun call(): Call = throw UnsupportedOperationException("call not used")

        override fun connectTimeoutMillis(): Int = 0

        override fun withConnectTimeout(timeout: Int, unit: TimeUnit): Interceptor.Chain = this

        override fun readTimeoutMillis(): Int = 0

        override fun withReadTimeout(timeout: Int, unit: TimeUnit): Interceptor.Chain = this

        override fun writeTimeoutMillis(): Int = 0

        override fun withWriteTimeout(timeout: Int, unit: TimeUnit): Interceptor.Chain = this
    }
}
