package com.elsasa.bgud.ui.component

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.elsasa.bgud.ui.theme.BrandCrimson
import com.elsasa.bgud.ui.theme.BrandGreen
import com.elsasa.bgud.ui.theme.BrandSage
import com.elsasa.bgud.viewmodel.BarcodeSyncSummary
import com.elsasa.bgud.viewmodel.ReturnOrderSyncSummary

/**
 * Status visual hasil sinkronisasi data dua arah.
 */
enum class SyncResultStatus {
    SUCCESS,
    PARTIAL,
    FAILED
}

/**
 * Industrial card displaying comprehensive two-way synchronization metrics
 * (Upload & Download), dynamic status badge (Sukses Penuh / Selesai Sebagian / Gagal),
 * and detailed breakdown per category.
 */
@Composable
fun SyncResultCard(
    barcodeSummary: BarcodeSyncSummary?,
    returnOrderSummary: ReturnOrderSyncSummary?,
    modifier: Modifier = Modifier,
    isFailed: Boolean = false,
    errorMessage: String? = null
) {
    val allErrors = buildList {
        if (!errorMessage.isNullOrBlank()) add(errorMessage.trim())
        if (!barcodeSummary?.errors.isNullOrBlank()) add(barcodeSummary.errors.trim())
        if (!returnOrderSummary?.errors.isNullOrBlank()) add(returnOrderSummary.errors.trim())
    }.distinct()

    val roSubmitted = returnOrderSummary?.submitted ?: 0
    val roFailed = returnOrderSummary?.submitFailed ?: 0
    val bcSubmitted = barcodeSummary?.submitted ?: 0
    val bcSkipped = barcodeSummary?.submitSkipped ?: 0
    val bcFailed = barcodeSummary?.submitFailed ?: 0

    val totalUploaded = roSubmitted + bcSubmitted
    val totalFailedUpload = roFailed + bcFailed

    val barangCount = barcodeSummary?.barangCount ?: 0
    val barcodeCount = barcodeSummary?.barcodeCount ?: 0
    val customerCount = returnOrderSummary?.customerCount ?: 0
    val salesPersonCount = returnOrderSummary?.salesPersonCount ?: 0
    val driverCount = returnOrderSummary?.driverCount ?: 0

    val totalDownloaded = barangCount + barcodeCount + customerCount + salesPersonCount + driverCount

    val hasAnyProcessedData = totalUploaded > 0 || totalDownloaded > 0
    val hasErrors = allErrors.isNotEmpty() || isFailed
    val hasItemFailures = totalFailedUpload > 0

    val status = when {
        // Gagal: Kedua worker gagal atau tidak ada data yang berhasil disinkronkan saat status failed
        (!hasAnyProcessedData && hasErrors) -> SyncResultStatus.FAILED
        // Selesai Sebagian: Terdapat data yang berhasil disinkronkan, namun ada item yang gagal terkirim atau ada worker mengembalikan error
        hasItemFailures || hasErrors -> SyncResultStatus.PARTIAL
        // Sukses Penuh: Tidak ada error dan tidak ada data yang gagal terkirim
        else -> SyncResultStatus.SUCCESS
    }

    val (badgeText, badgeTextColor, badgeBgColor, cardBorderColor) = when (status) {
        SyncResultStatus.SUCCESS -> Quad(
            "SUKSES PENUH",
            BrandGreen,
            BrandGreen.copy(alpha = 0.15f),
            BrandGreen.copy(alpha = 0.5f)
        )
        SyncResultStatus.PARTIAL -> Quad(
            "SELESAI SEBAGIAN",
            Color(0xFFD97706),
            Color(0xFFFEF3C7),
            Color(0xFFF59E0B).copy(alpha = 0.7f)
        )
        SyncResultStatus.FAILED -> Quad(
            "GAGAL",
            BrandCrimson,
            BrandCrimson.copy(alpha = 0.12f),
            BrandCrimson.copy(alpha = 0.5f)
        )
    }

    IndustrialCard(
        modifier = modifier,
        borderColor = cardBorderColor,
        containerColor = MaterialTheme.colorScheme.surface
    ) {
        // 1. Header: Judul & Badge Status
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.SpaceBetween,
            verticalAlignment = Alignment.CenterVertically
        ) {
            Text(
                text = "HASIL SINKRONISASI",
                style = MaterialTheme.typography.labelSmall.copy(
                    fontWeight = FontWeight.Bold,
                    fontFamily = FontFamily.Monospace,
                    letterSpacing = 1.sp
                ),
                color = when (status) {
                    SyncResultStatus.SUCCESS -> BrandGreen
                    SyncResultStatus.PARTIAL -> Color(0xFFD97706)
                    SyncResultStatus.FAILED -> BrandCrimson
                }
            )

            Surface(
                shape = RoundedCornerShape(6.dp),
                color = badgeBgColor,
                border = BorderStroke(1.dp, badgeTextColor.copy(alpha = 0.35f))
            ) {
                Text(
                    text = badgeText,
                    modifier = Modifier.padding(horizontal = 8.dp, vertical = 3.dp),
                    style = MaterialTheme.typography.labelSmall.copy(
                        fontWeight = FontWeight.Bold,
                        fontFamily = FontFamily.Monospace,
                        fontSize = 10.sp,
                        letterSpacing = 0.5.sp
                    ),
                    color = badgeTextColor
                )
            }
        }

        Spacer(modifier = Modifier.height(12.dp))

        // 2. Ringkasan Metrik Total: Diunggah & Diunduh
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.spacedBy(10.dp)
        ) {
            TotalMetricSurface(
                modifier = Modifier.weight(1f),
                title = "TOTAL DIUNGGAH",
                countText = "$totalUploaded Data",
                subtitle = "Order & Barcode",
                highlight = totalUploaded > 0
            )

            TotalMetricSurface(
                modifier = Modifier.weight(1f),
                title = "TOTAL DIUNDUH",
                countText = "$totalDownloaded Data",
                subtitle = "Katalog & Referensi",
                highlight = totalDownloaded > 0
            )
        }

        Spacer(modifier = Modifier.height(14.dp))
        HorizontalDivider(
            color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.35f),
            modifier = Modifier.padding(vertical = 2.dp)
        )

        // 3. Rincian Pengiriman (Upload)
        Spacer(modifier = Modifier.height(8.dp))
        Text(
            text = "PENGIRIMAN DATA (UPLOAD)",
            style = MaterialTheme.typography.labelSmall.copy(
                fontWeight = FontWeight.Bold,
                fontFamily = FontFamily.Monospace,
                letterSpacing = 0.8.sp
            ),
            color = MaterialTheme.colorScheme.primary
        )
        Spacer(modifier = Modifier.height(6.dp))

        SyncMetricItemRow(label = "Return Order Terkirim", value = "$roSubmitted dokumen")
        if (roFailed > 0) {
            SyncMetricItemRow(
                label = "Return Order Gagal",
                value = "$roFailed dokumen",
                valueColor = BrandCrimson,
                isWarning = true
            )
        }

        HorizontalDivider(
            color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.25f),
            modifier = Modifier.padding(vertical = 4.dp)
        )

        SyncMetricItemRow(label = "Registrasi Barcode Terkirim", value = "$bcSubmitted barcode")
        if (bcSkipped > 0) {
            SyncMetricItemRow(
                label = "Registrasi Barcode Dilewati",
                value = "$bcSkipped barcode",
                valueColor = Color.Gray
            )
        }
        if (bcFailed > 0) {
            SyncMetricItemRow(
                label = "Registrasi Barcode Gagal",
                value = "$bcFailed barcode",
                valueColor = BrandCrimson,
                isWarning = true
            )
        }

        Spacer(modifier = Modifier.height(10.dp))
        HorizontalDivider(
            color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.35f),
            modifier = Modifier.padding(vertical = 2.dp)
        )

        // 4. Rincian Pembaruan (Download)
        Spacer(modifier = Modifier.height(8.dp))
        Text(
            text = "PEMBARUAN DATA MASTER (DOWNLOAD)",
            style = MaterialTheme.typography.labelSmall.copy(
                fontWeight = FontWeight.Bold,
                fontFamily = FontFamily.Monospace,
                letterSpacing = 0.8.sp
            ),
            color = MaterialTheme.colorScheme.primary
        )
        Spacer(modifier = Modifier.height(6.dp))

        SyncMetricItemRow(label = "Katalog Barang", value = "$barangCount item")
        HorizontalDivider(
            color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.25f),
            modifier = Modifier.padding(vertical = 4.dp)
        )

        SyncMetricItemRow(label = "Barcode Registry", value = "$barcodeCount item")
        HorizontalDivider(
            color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.25f),
            modifier = Modifier.padding(vertical = 4.dp)
        )

        SyncMetricItemRow(label = "Referensi Pelanggan", value = "$customerCount item")
        HorizontalDivider(
            color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.25f),
            modifier = Modifier.padding(vertical = 4.dp)
        )

        SyncMetricItemRow(label = "Referensi Salesman", value = "$salesPersonCount item")
        HorizontalDivider(
            color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.25f),
            modifier = Modifier.padding(vertical = 4.dp)
        )

        SyncMetricItemRow(label = "Referensi Driver", value = "$driverCount item")

        // 5. Catatan Kendala / Error jika ada
        if (allErrors.isNotEmpty()) {
            Spacer(modifier = Modifier.height(12.dp))
            Surface(
                modifier = Modifier.fillMaxWidth(),
                shape = RoundedCornerShape(10.dp),
                color = MaterialTheme.colorScheme.errorContainer.copy(alpha = 0.45f),
                border = BorderStroke(1.dp, MaterialTheme.colorScheme.error.copy(alpha = 0.4f))
            ) {
                Column(modifier = Modifier.padding(10.dp)) {
                    Text(
                        text = "Catatan Kendala Sinkronisasi:",
                        style = MaterialTheme.typography.labelSmall.copy(
                            fontWeight = FontWeight.Bold
                        ),
                        color = MaterialTheme.colorScheme.error
                    )
                    Spacer(modifier = Modifier.height(4.dp))
                    allErrors.forEach { err ->
                        Text(
                            text = "• $err",
                            style = MaterialTheme.typography.bodySmall,
                            color = MaterialTheme.colorScheme.onErrorContainer
                        )
                    }
                }
            }
        }
    }
}

@Composable
private fun TotalMetricSurface(
    modifier: Modifier = Modifier,
    title: String,
    countText: String,
    subtitle: String,
    highlight: Boolean
) {
    Surface(
        modifier = modifier,
        shape = RoundedCornerShape(10.dp),
        color = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.45f),
        border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.4f))
    ) {
        Column(modifier = Modifier.padding(10.dp)) {
            Text(
                text = title,
                style = MaterialTheme.typography.labelSmall.copy(
                    fontWeight = FontWeight.Bold,
                    fontFamily = FontFamily.Monospace,
                    fontSize = 9.sp,
                    letterSpacing = 0.5.sp
                ),
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
            Spacer(modifier = Modifier.height(4.dp))
            Text(
                text = countText,
                style = MaterialTheme.typography.titleMedium.copy(
                    fontWeight = FontWeight.Bold,
                    fontFamily = FontFamily.Monospace
                ),
                color = if (highlight) BrandGreen else MaterialTheme.colorScheme.onSurface
            )
            Spacer(modifier = Modifier.height(2.dp))
            Text(
                text = subtitle,
                style = MaterialTheme.typography.labelSmall.copy(fontSize = 10.sp),
                color = MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.8f)
            )
        }
    }
}

@Composable
private fun SyncMetricItemRow(
    label: String,
    value: String,
    valueColor: Color = MaterialTheme.colorScheme.onSurface,
    isWarning: Boolean = false
) {
    Row(
        modifier = Modifier.fillMaxWidth(),
        horizontalArrangement = Arrangement.SpaceBetween,
        verticalAlignment = Alignment.CenterVertically
    ) {
        Text(
            text = label,
            style = MaterialTheme.typography.bodySmall,
            color = if (isWarning) BrandCrimson else MaterialTheme.colorScheme.onSurfaceVariant
        )
        Text(
            text = value,
            style = MaterialTheme.typography.bodySmall.copy(
                fontWeight = if (isWarning) FontWeight.Bold else FontWeight.SemiBold,
                fontFamily = FontFamily.Monospace
            ),
            color = valueColor
        )
    }
}

private data class Quad<A, B, C, D>(
    val first: A,
    val second: B,
    val third: C,
    val fourth: D
)
