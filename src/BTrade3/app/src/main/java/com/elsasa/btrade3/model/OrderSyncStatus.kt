package com.elsasa.btrade3.model

object OrderSyncStatus {
    const val IN_PROGRESS = "IN_PROGRESS"
    const val READY_TO_SYNC = "READY_TO_SYNC"
    const val SENT = "SENT"

    /** @deprecated Legacy value migrated on database upgrade */
    const val LEGACY_DRAFT = "DRAFT"

    fun isEditable(status: String): Boolean = status == IN_PROGRESS

    fun displayLabel(status: String): String = when (status) {
        IN_PROGRESS -> "In Progress"
        READY_TO_SYNC -> "Ready"
        SENT -> "Sent"
        LEGACY_DRAFT -> "Draft"
        else -> status
    }

    fun validateForReadyToSync(order: Order, itemCount: Int): String? {
        if (order.customerId.isBlank()) return "Please select a customer"
        if (order.salesId.isBlank()) return "Please select a sales person"
        if (itemCount == 0) return "Please add at least one item"
        return null
    }

    fun isEligibleForSync(order: Order, itemCount: Int): Boolean =
        order.statusSync == READY_TO_SYNC && validateForReadyToSync(order, itemCount) == null

    fun canSyncFromList(status: String): Boolean = status == READY_TO_SYNC
}
