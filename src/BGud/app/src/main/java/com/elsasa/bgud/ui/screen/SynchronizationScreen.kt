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
 * ```text
 * Master Data Card    (Last Barang Sync, Last Barcode Sync)
 * Return Order Card   (Last Reference Sync, Pending, Synced)
 * Queue Card          (Pending, Success, Rejected)
 * Actions             (Sync Now)
 * Connectivity State  (Online / Offline)
 * ```
 *
 * State display (§14.5, §14.3): `Idle` → `Sync Now` → `Synchronizing` →
 * `Synchronized` (timestamps + queue counts refreshed via the DataStore/Room
 * flows) or `Failed` ("Gagal sinkronisasi.", UX §14) with Retry. Sync Now
 * triggers the Return Order sync worker in addition to the barcode worker
 * (SCR-MOB-RO-005, S4.10) and is disabled while offline (IR-M5/IR-M9) and
 * while a run is in progress (IR-M6/OQ-1). The screen issues no network call
 * itself; the view models enqueue the one-shot sync workers (S5.3/S4.5, §20).
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

    // SCR-MOB-RO-005 (S4.10): Return Order sync state shown alongside the
    // barcode state (§19.1 `ReturnOrderSyncViewModel`).
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
        if (syncState == SyncState.FAILED) add(error ?: "Gagal sinkronisasi.")
        if (returnOrderSyncState == SyncState.FAILED) {
            add(returnOrderError ?: "Gagal sinkronisasi.")
        }
    }.distinct()
    // IR-M5/IR-M9: offline disables Sync Now (progress impossible). IR-M6/OQ-1:
    // one sync run at a time — Sync Now is disabled while Synchronizing.
    val isConnected = isOnline && returnOrderOnline
    val syncNowEnabled = isConnected && !isSyncing

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

            // Return Order Card (SCR-MOB-RO-005, §14.3, §19.1): reference
            // timestamps + pending/synced counts. Device vocabulary is
            // `Draft`/`Synced` only (ADR-RO-006); `lastRefSync` is the most
            // recent Customer/SalesPerson/Driver reference download.
            Card(modifier = Modifier.fillMaxWidth()) {
                Column(modifier = Modifier.padding(16.dp)) {
                    SyncSectionTitle("Return Order")
                    Spacer(modifier = Modifier.height(8.dp))
                    SyncRow("Last Reference Sync", formatSyncTime(lastRefSync))
                    SyncRow("Pending", returnOrderPendingCount.toString())
                    SyncRow("Synced", returnOrderSyncedCount.toString())
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

            // Sync state feedback (§14.5, §14.3). Barcode and Return Order
            // runs are reported together: failure wins, then progress, then
            // success.
            when {
                returnOrderSyncing || barcodeSyncing -> {
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
                isFailed -> {
                    Text(
                        text = if (failures.isEmpty()) {
                            "Gagal sinkronisasi."
                        } else {
                            failures.joinToString(" ")
                        },
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.error
                    )
                }
                isSynchronized -> {
                    Text(
                        text = "Sinkronisasi berhasil.",
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.primary
                    )
                }
                else -> Unit
            }

            // Actions (§12.9): Sync Now triggers the Return Order worker in
            // addition to the barcode worker (SCR-MOB-RO-005); on failure the
            // same button is the Retry (UX §14 "Gagal sinkronisasi." → Retry).
            Button(
                onClick = {
                    viewModel.syncNow(context)
                    returnOrderViewModel.syncNow(context)
                },
                enabled = syncNowEnabled,
                modifier = Modifier.fillMaxWidth()
            ) {
                Text(
                    when {
                        isSyncing -> "Synchronizing…"
                        isFailed -> "Retry"
                        else -> "Sync Now"
                    }
                )
            }
            if (!isConnected) {
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
