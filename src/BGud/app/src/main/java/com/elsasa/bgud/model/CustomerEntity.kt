package com.elsasa.bgud.model

import androidx.room.Entity
import androidx.room.PrimaryKey

/**
 * Mandatory Customer reference cache (Return Order Architecture §6.4,
 * GAP-004, S4.1).
 *
 * Full-replace download (deleteAll + upsertAll) scoped to the bound Office.
 */
@Entity(tableName = "customer_entity")
data class CustomerEntity(
    @PrimaryKey
    val customerId: String,
    val customerCode: String = "",
    val customerName: String = "",
    val address: String = ""
)
