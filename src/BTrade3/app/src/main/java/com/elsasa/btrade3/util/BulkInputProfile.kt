package com.elsasa.btrade3.util

import com.elsasa.btrade3.model.Barang

data class BulkInputProfile(
    val hrgSat: Double,
    val konversi: Int,
    val satBesar: String,
    val satKecil: String
)

fun Barang.bulkProfile(): BulkInputProfile =
    BulkInputProfile(hrgSat, konversi, satBesar, satKecil)

fun List<Barang>.canBulkTogether(): Boolean =
    map { it.bulkProfile() }.distinct().size == 1

fun List<Barang>.groupByBulkProfile(): Map<BulkInputProfile, List<Barang>> =
    groupBy { it.bulkProfile() }
