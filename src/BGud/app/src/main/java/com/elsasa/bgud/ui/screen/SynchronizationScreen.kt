package com.elsasa.bgud.ui.screen

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.elsasa.bgud.ui.component.BrandHeaderBar
import com.elsasa.bgud.ui.component.IndustrialCard
import com.elsasa.bgud.ui.theme.BrandCrimson
import com.elsasa.bgud.ui.theme.BrandGreen
import com.elsasa.bgud.ui.theme.BrandSage
import com.elsasa.bgud.viewmodel.ReturnOrderSyncViewModel
import com.elsasa.bgud.viewmodel.SyncState
import com.elsasa.bgud.viewmodel.SynchronizationViewModel
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

/**
 * Synchronization screen (SCR-MOB-007 / SCR-MOB-RO-005, Architecture §12.9,
 * §14.3, §14.5, UX Blueprint §11).
 *
 * Modern Industrial layout:
 * - Master Data Cache Card
 * - Return Order Operational Queue Card
 * - Registration Queue Card
 * - Connectivity indicator
 * - Sync Now CTA button with loading states
 */
@Composable
fun SynchronizationScreen(
    viewModel: SynchronizationViewModel,
    returnOrderViewModel: ReturnOrderSyncViewModel,
    onBack: () -> Unit
) {
    val context = LocalContext.current
    val lastBarangSyncAt by viewModel.lastBarangSyncAt.collectAsState()
    val lastBarcodeSyncAt by viewModel.lastBarcodeSyncAt.collectAsState()
    val pendingCount by viewModel.pendingCount.collectAsState()
    val successCount by viewModel.successCount.collectAsState()
    val rejectedCount by viewModel.rejectedCount.collectAsState()
    val syncState by viewModel.syncState.collectAsState()
    val error by viewModel.error.collectAsState()
    val isOnline by viewModel.isOnline.collectAsState()

    val lastRefSync by returnOrderViewModel.lastRefSync.collectAsState()
    val returnOrderPendingCount by returnOrderViewModel.pendingCount.collectAsState()
    val returnOrderSyncedCount by returnOrderViewModel.syncedCount.collectAsState()
    val returnOrderSyncState by returnOrderViewModel.syncState.collectAsState()
    val returnOrderError by returnOrderViewModel.error.collectAsState()
    val returnOrderOnline by returnOrderViewModel.isOnline.collectAsState()

    val barcodeSyncing = syncState == SyncState.SYNCHRONIZING
    val returnOrderSyncing = returnOrderSyncState == SyncState.SYNCHRONIZING
    val isSyncing = barcodeSyncing || returnOrderSyncing
    val isFailed = syncState == SyncState.FAILED || returnOrderSyncState == SyncState.FAILED
    val isSynchronized = syncState == SyncState.SYNCHRONIZED ||
        returnOrderSyncState == SyncState.SYNCHRONIZED
    val failures = buildList {
        if (syncState == SyncState.FAILED) add(error ?: "Gagal sinkronisasi barcode.")
        if (returnOrderSyncState == SyncState.FAILED) {
            add(returnOrderError ?: "Gagal sinkronisasi return order.")
        }
    }.distinct()

    val isConnected = isOnline && returnOrderOnline
    val syncNowEnabled = isConnected && !isSyncing

    Scaffold(
        containerColor = MaterialTheme.colorScheme.background
    ) { paddingValues ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
                .verticalScroll(rememberScrollState())
                .padding(horizontal = 16.dp, vertical = 12.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            // Header Bar
            BrandHeaderBar(
                title = "Sinkronisasi Data",
                subtitle = "Sinkronisasi data master & pengiriman antrean",
                onBack = onBack,
                actions = {
                    Surface(
                        shape = RoundedCornerShape(10.dp),
                        color = if (isConnected) BrandGreen.copy(alpha = 0.15f) else BrandCrimson.copy(alpha = 0.15f)
                    ) {
                        Row(
                            modifier = Modifier.padding(horizontal = 8.dp, vertical = 4.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Box(
                                modifier = Modifier
                                    .size(6.dp)
                                    .clip(CircleShape)
                                    .background(if (isConnected) BrandGreen else BrandCrimson)
                            )
                            Spacer(modifier = Modifier.width(5.dp))
                            Text(
                                text = if (isConnected) "Online" else "Offline",
                                style = MaterialTheme.typography.labelSmall.copy(
                                    fontWeight = FontWeight.Bold,
                                    fontSize = 10.sp
                                ),
                                color = if (isConnected) BrandGreen else BrandCrimson
                            )
                        }
                    }
                }
            )

            // 1. Master Data Cache Card
            IndustrialCard(borderColor = BrandSage, containerColor = MaterialTheme.colorScheme.surface) {
                Text(
                    text = "1. DATA MASTER (UNDUH DARI SERVER)",
                    style = MaterialTheme.typography.labelSmall.copy(
                        fontWeight = FontWeight.Bold,
                        fontFamily = FontFamily.Monospace,
                        letterSpacing = 1.sp
                    ),
                    color = MaterialTheme.colorScheme.primary
                )
                Spacer(modifier = Modifier.height(10.dp))
                SyncDataRow("Katalog Barang (Barang Entity)", formatSyncTime(lastBarangSyncAt))
                HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.4f), modifier = Modifier.padding(vertical = 6.dp))
                SyncDataRow("Barcode Registry", formatSyncTime(lastBarcodeSyncAt))
                HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.4f), modifier = Modifier.padding(vertical = 6.dp))
                SyncDataRow("Referensi Retur (Pelanggan/Sales/Driver)", formatSyncTime(lastRefSync))
            }

            // 2. Return Order Queue Card
            IndustrialCard(borderColor = BrandGreen.copy(alpha = 0.5f), containerColor = MaterialTheme.colorScheme.surface) {
                Text(
                    text = "2. ANTREAN RETURN ORDER",
                    style = MaterialTheme.typography.labelSmall.copy(
                        fontWeight = FontWeight.Bold,
                        fontFamily = FontFamily.Monospace,
                        letterSpacing = 1.sp
                    ),
                    color = BrandGreen
                )
                Spacer(modifier = Modifier.height(10.dp))
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Column {
                        Text(
                            text = "Menunggu Pengiriman",
                            style = MaterialTheme.typography.bodySmall,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                        Text(
                            text = "$returnOrderPendingCount Dokumen Draft",
                            style = MaterialTheme.typography.bodyMedium.copy(
                                fontWeight = FontWeight.Bold,
                                color = if (returnOrderPendingCount > 0) BrandCrimson else MaterialTheme.colorScheme.onSurface
                            )
                        )
                    }

                    Column(horizontalAlignment = Alignment.End) {
                        Text(
                            text = "Telah Disinkronkan",
                            style = MaterialTheme.typography.bodySmall,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                        Text(
                            text = "$returnOrderSyncedCount Dokumen",
                            style = MaterialTheme.typography.bodyMedium.copy(
                                fontWeight = FontWeight.Bold,
                                color = BrandGreen
                            )
                        )
                    }
                }
            }

            // 3. Barcode Registration Queue Card
            IndustrialCard(borderColor = MaterialTheme.colorScheme.outlineVariant) {
                Text(
                    text = "3. ANTREAN REGISTRASI BARCODE",
                    style = MaterialTheme.typography.labelSmall.copy(
                        fontWeight = FontWeight.Bold,
                        fontFamily = FontFamily.Monospace,
                        letterSpacing = 1.sp
                    ),
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
                Spacer(modifier = Modifier.height(10.dp))
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween
                ) {
                    QueueStatusCount("Menunggu", pendingCount, BrandCrimson)
                    QueueStatusCount("Berhasil", successCount, BrandGreen)
                    QueueStatusCount("Ditolak", rejectedCount, Color.Gray)
                }
            }

            // Sync State Feedback Banner
            when {
                isSyncing -> {
                    Surface(
                        modifier = Modifier.fillMaxWidth(),
                        shape = RoundedCornerShape(12.dp),
                        color = BrandSage.copy(alpha = 0.25f),
                        border = BorderStroke(1.dp, BrandSage)
                    ) {
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(12.dp),
                            verticalAlignment = Alignment.CenterVertically,
                            horizontalArrangement = Arrangement.Center
                        ) {
                            CircularProgressIndicator(
                                modifier = Modifier.size(18.dp),
                                strokeWidth = 2.dp,
                                color = BrandGreen
                            )
                            Spacer(modifier = Modifier.width(10.dp))
                            Text(
                                text = "Proses sinkronisasi data sedang berjalan...",
                                style = MaterialTheme.typography.bodySmall.copy(fontWeight = FontWeight.SemiBold),
                                color = MaterialTheme.colorScheme.onSurface
                            )
                        }
                    }
                }
                isFailed -> {
                    Surface(
                        modifier = Modifier.fillMaxWidth(),
                        shape = RoundedCornerShape(12.dp),
                        color = MaterialTheme.colorScheme.errorContainer.copy(alpha = 0.5f),
                        border = BorderStroke(1.dp, MaterialTheme.colorScheme.error.copy(alpha = 0.5f))
                    ) {
                        Column(modifier = Modifier.padding(12.dp)) {
                            Text(
                                text = "Gagal Sinkronisasi",
                                style = MaterialTheme.typography.bodyMedium.copy(fontWeight = FontWeight.Bold),
                                color = MaterialTheme.colorScheme.error
                            )
                            Text(
                                text = failures.joinToString(" • "),
                                style = MaterialTheme.typography.bodySmall,
                                color = MaterialTheme.colorScheme.error
                            )
                        }
                    }
                }
                isSynchronized -> {
                    Surface(
                        modifier = Modifier.fillMaxWidth(),
                        shape = RoundedCornerShape(12.dp),
                        color = BrandGreen.copy(alpha = 0.15f),
                        border = BorderStroke(1.dp, BrandGreen.copy(alpha = 0.4f))
                    ) {
                        Row(
                            modifier = Modifier.padding(12.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Text(text = "✓", color = BrandGreen, fontWeight = FontWeight.Bold)
                            Spacer(modifier = Modifier.width(8.dp))
                            Text(
                                text = "Semua data berhasil disinkronkan dengan server.",
                                style = MaterialTheme.typography.bodySmall.copy(fontWeight = FontWeight.Medium),
                                color = Color(0xFF1E460E)
                            )
                        }
                    }
                }
                else -> Unit
            }

            // Sync CTA Button
            Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                Button(
                    onClick = {
                        viewModel.syncNow(context)
                        returnOrderViewModel.syncNow(context)
                    },
                    enabled = syncNowEnabled,
                    modifier = Modifier
                        .fillMaxWidth()
                        .height(52.dp),
                    shape = RoundedCornerShape(14.dp),
                    colors = ButtonDefaults.buttonColors(
                        containerColor = BrandGreen,
                        contentColor = Color.White
                    ),
                    elevation = ButtonDefaults.buttonElevation(defaultElevation = 2.dp)
                ) {
                    if (isSyncing) {
                        CircularProgressIndicator(
                            modifier = Modifier.size(20.dp),
                            strokeWidth = 2.dp,
                            color = Color.White
                        )
                        Spacer(modifier = Modifier.width(8.dp))
                        Text("Sedang Menyinkronkan...", fontWeight = FontWeight.Bold)
                    } else if (isFailed) {
                        Text("Coba Sinkronkan Ulang", fontWeight = FontWeight.Bold)
                    } else {
                        Text("Sinkronkan Sekarang", fontWeight = FontWeight.Bold)
                    }
                }

                if (!isConnected) {
                    Text(
                        text = "Perangkat sedang offline. Sambungkan koneksi internet untuk melakukan sinkronisasi.",
                        style = MaterialTheme.typography.labelSmall,
                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                        textAlign = TextAlign.Center,
                        modifier = Modifier.fillMaxWidth()
                    )
                }

                OutlinedButton(
                    onClick = onBack,
                    modifier = Modifier
                        .fillMaxWidth()
                        .height(50.dp),
                    shape = RoundedCornerShape(14.dp),
                    colors = ButtonDefaults.outlinedButtonColors(
                        containerColor = MaterialTheme.colorScheme.surface
                    ),
                    border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant)
                ) {
                    Text("Kembali", fontWeight = FontWeight.SemiBold)
                }
            }

            Spacer(modifier = Modifier.height(12.dp))
        }
    }
}

@Composable
private fun SyncDataRow(label: String, value: String) {
    Row(
        modifier = Modifier.fillMaxWidth(),
        horizontalArrangement = Arrangement.SpaceBetween,
        verticalAlignment = Alignment.CenterVertically
    ) {
        Text(
            text = label,
            style = MaterialTheme.typography.bodySmall,
            color = MaterialTheme.colorScheme.onSurfaceVariant
        )
        Text(
            text = value,
            style = MaterialTheme.typography.bodySmall.copy(
                fontWeight = FontWeight.SemiBold,
                fontFamily = FontFamily.Monospace
            ),
            color = MaterialTheme.colorScheme.onSurface
        )
    }
}

@Composable
private fun QueueStatusCount(label: String, count: Int, color: Color) {
    Column(horizontalAlignment = Alignment.CenterHorizontally) {
        Text(
            text = label,
            style = MaterialTheme.typography.labelSmall,
            color = MaterialTheme.colorScheme.onSurfaceVariant
        )
        Spacer(modifier = Modifier.height(2.dp))
        Surface(
            shape = RoundedCornerShape(6.dp),
            color = color.copy(alpha = 0.15f)
        ) {
            Text(
                text = count.toString(),
                modifier = Modifier.padding(horizontal = 10.dp, vertical = 2.dp),
                style = MaterialTheme.typography.labelMedium.copy(
                    fontWeight = FontWeight.Bold,
                    fontFamily = FontFamily.Monospace
                ),
                color = color
            )
        }
    }
}

private fun formatSyncTime(timestampMillis: Long): String {
    if (timestampMillis <= 0L) return "Belum pernah"
    return SimpleDateFormat("dd MMM yyyy HH:mm", Locale.getDefault())
        .format(Date(timestampMillis))
}
