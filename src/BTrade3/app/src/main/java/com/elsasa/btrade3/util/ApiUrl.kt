package com.elsasa.btrade3.util

import java.net.URI

/**
 * Single source of truth for the shape of the cloud API base URL.
 *
 * The configured value is the server root, for example:
 *   https://dev.smart-ics.com:8089/belajar-api
 *
 * Retrofit needs a trailing slash and every endpoint in [com.elsasa.btrade3.network.ApiService]
 * is relative to the "/api/" path segment, so [normalize] appends both.
 */
object ApiUrl {

    private const val API_PATH = "api"

    const val DEFAULT_BASE_URL = "https://dev.smart-ics.com:8089/belajar-api"

    fun normalize(raw: String?): String {
        val candidate = raw?.trim().orEmpty().ifEmpty { DEFAULT_BASE_URL }

        val withScheme = when {
            candidate.startsWith("http://", ignoreCase = true) -> candidate
            candidate.startsWith("https://", ignoreCase = true) -> candidate
            SCHEME_PATTERN.containsMatchIn(candidate) -> DEFAULT_BASE_URL
            else -> "https://$candidate"
        }

        val withoutTrailingSlash = withScheme.trimEnd('/')
        val basePath = if (withoutTrailingSlash.endsWith("/$API_PATH", ignoreCase = true)) {
            withoutTrailingSlash
        } else {
            "$withoutTrailingSlash/$API_PATH"
        }

        return "$basePath/"
    }

    fun isValid(raw: String?): Boolean {
        val candidate = raw?.trim().orEmpty()
        if (candidate.isEmpty()) return false

        val hasScheme = SCHEME_PATTERN.containsMatchIn(candidate)
        if (hasScheme &&
            !candidate.startsWith("http://", ignoreCase = true) &&
            !candidate.startsWith("https://", ignoreCase = true)
        ) {
            return false
        }

        val withScheme = if (hasScheme) candidate else "https://$candidate"

        return try {
            val uri = URI(withScheme)
            val scheme = uri.scheme?.lowercase()
            (scheme == "http" || scheme == "https") && !uri.host.isNullOrBlank()
        } catch (e: Exception) {
            false
        }
    }

    private val SCHEME_PATTERN = Regex("^[a-zA-Z][a-zA-Z0-9+.-]*://")
}
