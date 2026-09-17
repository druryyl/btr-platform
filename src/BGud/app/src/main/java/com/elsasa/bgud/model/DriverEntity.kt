package com.elsasa.bgud.model

import androidx.room.Entity
import androidx.room.PrimaryKey

/**
 * Optional Driver reference cache (Return Order Architecture §6.4,
 * ADR-RO-005, S4.1).
 *
 * Full-replace download (deleteAll + upsertAll) scoped to the bound Office.
 * `isAktif` is persisted as INTEGER (0/1) by Room.
 */
@Entity(tableName = "driver_entity")
data class DriverEntity(
    @PrimaryKey
    val driverId: String,
    val driverName: String = "",
    val isAktif: Boolean = true
)
