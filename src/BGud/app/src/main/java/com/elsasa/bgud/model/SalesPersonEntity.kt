package com.elsasa.bgud.model

import androidx.room.Entity
import androidx.room.PrimaryKey

/**
 * Optional Salesman reference cache (Return Order Architecture §6.4,
 * ADR-RO-005, S4.1).
 *
 * Full-replace download (deleteAll + upsertAll) scoped to the bound Office.
 */
@Entity(tableName = "salesperson_entity")
data class SalesPersonEntity(
    @PrimaryKey
    val salesPersonId: String,
    val salesPersonName: String = ""
)
