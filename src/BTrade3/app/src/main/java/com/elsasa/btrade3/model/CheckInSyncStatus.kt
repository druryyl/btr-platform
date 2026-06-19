package com.elsasa.btrade3.model

object CheckInSyncStatus {
    const val DRAFT = "DRAFT"
    const val SENT = "SENT"

    fun displayLabel(status: String): String = when (status) {
        SENT -> "Uploaded"
        DRAFT -> "Waiting for Upload"
        else -> status
    }

    fun isUploaded(status: String): Boolean = status == SENT
}
