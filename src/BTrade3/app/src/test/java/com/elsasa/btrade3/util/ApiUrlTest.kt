package com.elsasa.btrade3.util

import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test

class ApiUrlTest {

    @Test
    fun normalize_addsHttpsSchemeAndApiPath() {
        assertEquals(
            "https://dev.smart-ics.com:8089/belajar-api/api/",
            ApiUrl.normalize("dev.smart-ics.com:8089/belajar-api")
        )
    }

    @Test
    fun normalize_keepsConfiguredHttpScheme() {
        assertEquals(
            "http://dev.smart-ics.com:8089/belajar-api/api/",
            ApiUrl.normalize("http://dev.smart-ics.com:8089/belajar-api")
        )
    }

    @Test
    fun normalize_trimsTrailingSlashes() {
        assertEquals(
            "https://dev.smart-ics.com:8089/belajar-api/api/",
            ApiUrl.normalize("  https://dev.smart-ics.com:8089/belajar-api///  ")
        )
    }

    @Test
    fun normalize_isIdempotentForFullBaseUrl() {
        val normalized = ApiUrl.normalize(ApiUrl.DEFAULT_BASE_URL)
        assertEquals(normalized, ApiUrl.normalize(normalized))
        assertEquals("${ApiUrl.DEFAULT_BASE_URL}/api/", normalized)
    }

    @Test
    fun normalize_defaultsBlankValueToDefaultBaseUrl() {
        assertEquals("${ApiUrl.DEFAULT_BASE_URL}/api/", ApiUrl.normalize("   "))
        assertEquals("${ApiUrl.DEFAULT_BASE_URL}/api/", ApiUrl.normalize(null))
    }

    @Test
    fun isValid_acceptsHttpAndHttpsHosts() {
        assertTrue(ApiUrl.isValid("https://dev.smart-ics.com:8089/belajar-api"))
        assertTrue(ApiUrl.isValid("http://10.0.2.2:8089/belajar-api/api/"))
        assertTrue(ApiUrl.isValid("dev.smart-ics.com:8089/belajar-api"))
    }

    @Test
    fun isValid_rejectsBlankAndMalformedValues() {
        assertFalse(ApiUrl.isValid(null))
        assertFalse(ApiUrl.isValid("   "))
        assertFalse(ApiUrl.isValid("ftp://dev.smart-ics.com/belajar-api"))
        assertFalse(ApiUrl.isValid("https://"))
    }
}
