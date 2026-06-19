package com.elsasa.btrade3.model

/**
 * Indicates how a visit was closed. Stored as string in Room for forward compatibility.
 */
object CheckOutMode {
    const val MANUAL = "MANUAL"
    const val AUTO = "AUTO"

    fun isValid(value: String): Boolean = value == MANUAL || value == AUTO
}
