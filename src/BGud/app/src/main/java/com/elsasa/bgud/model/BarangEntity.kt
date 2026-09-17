package com.elsasa.bgud.model

import androidx.room.Entity
import androidx.room.PrimaryKey

/**
 * Barang reference cache (Architecture §6.4, ADR-005).
 *
 * The Barang Master is a shared reference dataset synced for offline lookup.
 * Fields mirror the shared `Brg` reference shape (code/name/active plus the
 * Item's own small/big units per IR-01) so registration can validate against
 * a cached Active Item (BQ-7) and display Item Code/Name/Unit. Full sync
 * mapping is owned by S5.3.
 */
@Entity(tableName = "barang_entity")
data class BarangEntity(
    @PrimaryKey
    val brgId: String,
    val brgCode: String,
    val brgName: String,
    val isAktif: Boolean = true,
    val satKecil: String = "",
    val satBesar: String = ""
)
