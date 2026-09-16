package com.elsasa.bgud.ui.screen

import androidx.compose.foundation.clickable
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
import androidx.compose.material3.ListItem
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.elsasa.bgud.viewmodel.HomeViewModel
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

/**
 * Home screen (SCR-MOB-002, Architecture §12.4, UX Blueprint §6).
 *
 * ```text
 * Context Header   (User, Warehouse, Office)
 * Quick Actions    (Scan Barcode, Search Barcode, Register Barcode)
 * Sync Status Card (Online/Offline, Last Sync Time, Pending Upload Count)
 * Navigation       (Barcode Registry, Synchronization, Settings)
 * ```
 *
 * The screen is a navigation hub only (traceability: navigation — no
 * workflow, no domain capability). It issues no network call: context and
 * sync summary come from DataStore + Room via [HomeViewModel] (§19.3).
 * Destinations `scan`, `barcode_registry`, `register`, `synchronization`,
 * and `settings` follow §13.2 (`Search Barcode` resolves to
 * `barcode_registry`; `register` carries an optional barcode argument
 * owned by S5.6/S5.7).
 */
@Composable
fun HomeScreen(
    viewModel: HomeViewModel,
    onScanBarcode: () -> Unit,
    onSearchBarcode: () -> Unit,
    onRegisterBarcode: () -> Unit,
    onOpenBarcodeRegistry: () -> Unit,
    onOpenSynchronization: () -> Unit,
    onOpenSettings: () -> Unit
) {
    val user by viewModel.user.collectAsState()
    val warehouse by viewModel.warehouse.collectAsState()
    val office by viewModel.office.collectAsState()
    val isOnline by viewModel.isOnline.collectAsState()
    val lastSyncAt by viewModel.lastSyncAt.collectAsState()
    val pendingCount by viewModel.pendingCount.collectAsState()

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
                text = "BGud",
                style = MaterialTheme.typography.headlineSmall.copy(
                    fontWeight = FontWeight.Bold
                ),
                color = MaterialTheme.colorScheme.onSurface
            )

            Card(modifier = Modifier.fillMaxWidth()) {
                Column(modifier = Modifier.padding(16.dp)) {
                    SectionTitle("Current Context")
                    Spacer(modifier = Modifier.height(8.dp))
                    ContextRow("Logged In User", user.ifBlank { "-" })
                    ContextRow("Warehouse", warehouse.ifBlank { "-" })
                    ContextRow("Office", office.ifBlank { "-" })
                }
            }

            Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                SectionTitle("Quick Actions")
                Button(
                    onClick = onScanBarcode,
                    modifier = Modifier.fillMaxWidth()
                ) {
                    Text("Scan Barcode")
                }
                OutlinedButton(
                    onClick = onSearchBarcode,
                    modifier = Modifier.fillMaxWidth()
                ) {
                    Text("Search Barcode")
                }
                OutlinedButton(
                    onClick = onRegisterBarcode,
                    modifier = Modifier.fillMaxWidth()
                ) {
                    Text("Register Barcode")
                }
            }

            Card(modifier = Modifier.fillMaxWidth()) {
                Column(modifier = Modifier.padding(16.dp)) {
                    SectionTitle("Synchronization Status")
                    Spacer(modifier = Modifier.height(8.dp))
                    ContextRow("Status", if (isOnline) "Online" else "Offline")
                    ContextRow("Last Sync Time", formatLastSync(lastSyncAt))
                    ContextRow("Pending Upload Count", pendingCount.toString())
                }
            }

            Column(verticalArrangement = Arrangement.spacedBy(0.dp)) {
                SectionTitle("Navigation")
                ListItem(
                    headlineContent = { Text("Barcode Registry") },
                    modifier = Modifier.clickable(onClick = onOpenBarcodeRegistry)
                )
                ListItem(
                    headlineContent = { Text("Synchronization") },
                    modifier = Modifier.clickable(onClick = onOpenSynchronization)
                )
                ListItem(
                    headlineContent = { Text("Settings") },
                    modifier = Modifier.clickable(onClick = onOpenSettings)
                )
            }
        }
    }
}

@Composable
private fun SectionTitle(text: String) {
    Text(
        text = text,
        style = MaterialTheme.typography.titleSmall.copy(
            fontWeight = FontWeight.Bold
        ),
        color = MaterialTheme.colorScheme.onSurface
    )
}

@Composable
private fun ContextRow(label: String, value: String) {
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

private fun formatLastSync(timestampMillis: Long): String {
    if (timestampMillis <= 0L) return "Never"
    return SimpleDateFormat("dd MMM yyyy HH:mm", Locale.getDefault())
        .format(Date(timestampMillis))
}
