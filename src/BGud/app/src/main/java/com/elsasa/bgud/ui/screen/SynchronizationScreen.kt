package com.elsasa.bgud.ui.screen

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.elsasa.bgud.viewmodel.SyncState
import com.elsasa.bgud.viewmodel.SynchronizationViewModel
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

/**
 * Synchronization screen (SCR-MOB-007, Architecture §12.9, UX Blueprint §11).
 *
 * ```text
 * Master Data Card    (Last Barang Sync, Last Barcode Sync)
 * Queue Card          (Pending, Success, Rejected)
 * Actions             (Sync Now)
 * Connectivity State  (Online / Offline)
 * ```
 *
 * State display (§14.5): `Idle` → `Sync Now` → `Synchronizing` →
 * `Synchronized` (timestamps + queue counts refreshed via the DataStore/Room
 * flows) or `Failed` ("Gagal sinkronisasi.", UX §14) with Retry. Sync Now is
 * disabled while offline (IR-M5) and while a run is in progress (IR-M6).
 * The screen issues no network call itself; [SynchronizationViewModel]
 * enqueues the one-shot sync worker (S5.3, §20).
 */
@Composable
fun SynchronizationScreen(
    viewModel: SynchronizationViewModel,
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

    val isSyncing = syncState == SyncState.SYNCHRONIZING
    // IR-M5: offline disables Sync Now (progress impossible). IR-M6: one
    // sync run at a time — Sync Now is disabled while Synchronizing.
    val syncNowEnabled = isOnline && !isSyncing

    Scaffold(
        containerColor = MaterialTheme.colorScheme.surface
    ) { paddingValues ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
                .verticalScroll(rememberScrollState())
                .padding(horizontal = 16.dp, vertical = 16.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            Text(
                text = "Synchronization",
                style = MaterialTheme.typography.headlineSmall.copy(
                    fontWeight = FontWeight.Bold
                ),
                color = MaterialTheme.colorScheme.onSurface
            )

            // Master Data Card (§12.9, UX Blueprint §11).
            Card(modifier = Modifier.fillMaxWidth()) {
                Column(modifier = Modifier.padding(16.dp)) {
                    SyncSectionTitle("Master Data")
                    Spacer(modifier = Modifier.height(8.dp))
                    SyncRow("Last Barang Sync", formatSyncTime(lastBarangSyncAt))
                    SyncRow("Last Barcode Sync", formatSyncTime(lastBarcodeSyncAt))
                }
            }

            // Queue Card (§12.9, UX Blueprint §11; §14.6
            // Pending | Synced | Rejected).
            Card(modifier = Modifier.fillMaxWidth()) {
                Column(modifier = Modifier.padding(16.dp)) {
                    SyncSectionTitle("Registration Queue")
                    Spacer(modifier = Modifier.height(8.dp))
                    SyncRow("Pending", pendingCount.toString())
                    SyncRow("Success", successCount.toString())
                    SyncRow("Rejected", rejectedCount.toString())
                }
            }

            // Connectivity State (§12.9, §14.6 Online | Offline).
            Card(modifier = Modifier.fillMaxWidth()) {
                Column(modifier = Modifier.padding(16.dp)) {
                    SyncSectionTitle("Connectivity")
                    Spacer(modifier = Modifier.height(8.dp))
                    SyncRow("Status", if (isOnline) "Online" else "Offline")
                }
            }

            // Sync state feedback (§14.5).
            when (syncState) {
                SyncState.SYNCHRONIZING -> {
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.spacedBy(
                            12.dp,
                            Alignment.CenterHorizontally
                        ),
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        CircularProgressIndicator()
                        Text(
                            text = "Sinkronisasi berjalan…",
                            style = MaterialTheme.typography.bodyMedium,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    }
                }
                SyncState.SYNCHRONIZED -> {
                    Text(
                        text = "Sinkronisasi berhasil.",
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.primary
                    )
                }
                SyncState.FAILED -> {
                    Text(
                        text = error ?: "Gagal sinkronisasi.",
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.error
                    )
                }
                SyncState.IDLE -> Unit
            }

            // Actions (§12.9): Sync Now; on failure the same button is the
            // Retry (UX §14 "Gagal sinkronisasi." → Retry).
            Button(
                onClick = { viewModel.syncNow(context) },
                enabled = syncNowEnabled,
                modifier = Modifier.fillMaxWidth()
            ) {
                Text(
                    when {
                        isSyncing -> "Synchronizing…"
                        syncState == SyncState.FAILED -> "Retry"
                        else -> "Sync Now"
                    }
                )
            }
            if (!isOnline) {
                Text(
                    text = "Offline — Sync Now tidak tersedia.",
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }

            OutlinedButton(
                onClick = onBack,
                modifier = Modifier.fillMaxWidth()
            ) {
                Text("Kembali")
            }
        }
    }
}

@Composable
private fun SyncSectionTitle(text: String) {
    Text(
        text = text,
        style = MaterialTheme.typography.titleSmall.copy(
            fontWeight = FontWeight.Bold
        ),
        color = MaterialTheme.colorScheme.onSurface
    )
}

@Composable
private fun SyncRow(label: String, value: String) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(vertical = 2.dp),
        horizontalArrangement = Arrangement.SpaceBetween
    ) {
        Text(
            text = label,
            style = MaterialTheme.typography.bodyMedium,
            color = MaterialTheme.colorScheme.onSurfaceVariant
        )
        Text(
            text = value,
            style = MaterialTheme.typography.bodyMedium.copy(
                fontWeight = FontWeight.Medium
            ),
            color = MaterialTheme.colorScheme.onSurface
        )
    }
}

private fun formatSyncTime(timestampMillis: Long): String {
    if (timestampMillis <= 0L) return "Never"
    return SimpleDateFormat("dd MMM yyyy HH:mm", Locale.getDefault())
        .format(Date(timestampMillis))
}
