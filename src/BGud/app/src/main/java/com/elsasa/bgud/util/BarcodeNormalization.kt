package com.elsasa.bgud.util

import java.util.Locale

/**
 * Local form of the VO-01 normalization (Architecture §5.1) used to derive
 * the Room lookup key `barcodeValueKey` (Architecture §6.4).
 *
 * 1. Remove CR, LF and TAB characters.
 * 2. Trim leading/trailing whitespace.
 * 3. Preserve leading zeros (no numeric conversion; string only).
 * 4. Comparison key = UPPER(normalized value).
 */
object BarcodeNormalization {

    fun normalize(value: String): String {
        return value
            .replace("\r", "")
            .replace("\n", "")
            .replace("\t", "")
            .trim()
    }

    fun toKey(value: String): String {
        return normalize(value).uppercase(Locale.US)
    }
}
