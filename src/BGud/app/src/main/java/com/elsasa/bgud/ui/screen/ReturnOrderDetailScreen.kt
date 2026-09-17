package com.elsasa.bgud.ui.screen

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.AlertDialog
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
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import com.elsasa.bgud.model.ReturnOrderEntity
import com.elsasa.bgud.model.ReturnOrderItemEntity
import com.elsasa.bgud.viewmodel.ReturnOrderDetailViewModel

/**
 * Return Order Detail screen (SCR-MOB-RO-003, Architecture §11.1, §12.3,
 * §14.2).
 *
 * ```text
 * Header Region     (Customer, Warehouse, Salesman/Driver, Notes, status)
 * Item Region       (Item, Qty, Unit, Return Type)
 * Action Region     (Edit, Delete — enabled only while Draft)
 * ```
 *
 * Read-only view of a local Return Order with status-gated actions
 * (IR-M7/M8). The status vocabulary is exactly `Draft`/`Synced`
 * (ADR-RO-006, §14.4): a `Draft` order offers Edit and Delete, a `Synced`
 * order is view-only (BR-017–020). Delete is a `Draft`-only action on this
 * screen — not a separate screen — and is confirmed before running
 * (§11.1, §13.1). It removes the order and its items in one local
 * transaction and never propagates (BC-003, GAP-014).
 *
 * Source is the local Room capture store only — no network call (§17.1).
 * Edit routes to `return_order_edit?returnOrderId={id}` (§13.1); the route
 * wiring is S4.11 and the Edit screen is S4.9, so this screen only raises
 * [onEdit].
 */
@Composable
fun ReturnOrderDetailScreen(
    viewModel: ReturnOrderDetailViewModel,
    onEdit: (returnOrderId: String) -> Unit,
    onDeleted: () -> Unit,
    onBack: () -> Unit
) {
    val isLoading by viewModel.isLoading.collectAsState()
    val order by viewModel.order.collectAsState()
    val items by viewModel.items.collectAsState()
    val notFound by viewModel.notFound.collectAsState()
    val isDeleting by viewModel.isDeleting.collectAsState()
    val deleteError by viewModel.deleteError.collectAsState()
    val deleted by viewModel.deleted.collectAsState()

    var showDeleteConfirm by remember { mutableStateOf(false) }

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
                text = "Detail Return Order",
                style = MaterialTheme.typography.headlineSmall.copy(
                    fontWeight = FontWeight.Bold
                ),
                color = MaterialTheme.colorScheme.onSurface
            )

            when {
                isLoading -> {
                    Box(
                        modifier = Modifier
                            .fillMaxWidth()
                            .weight(1f),
                        contentAlignment = Alignment.Center
                    ) {
                        CircularProgressIndicator()
                    }
                    return@Column
                }
                notFound || order == null -> {
                    // Unknown id: nothing to view.
                    Text(
                        text = "Data tidak ditemukan.",
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                        textAlign = TextAlign.Center,
                        modifier = Modifier.fillMaxWidth()
                    )
                    OutlinedButton(
                        onClick = onBack,
                        modifier = Modifier.fillMaxWidth()
                    ) {
                        Text("Kembali")
                    }
                    return@Column
                }
            }

            // Deleted (BC-003): the local order and its items are gone;
            // the caller returns to the list (§13.1).
            if (deleted) {
                Card(modifier = Modifier.fillMaxWidth()) {
                    Column(
                        modifier = Modifier.padding(16.dp),
                        verticalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        Text(
                            text = "Return Order telah dihapus.",
                            style = MaterialTheme.typography.titleMedium.copy(
                                fontWeight = FontWeight.Bold
                            ),
                            color = MaterialTheme.colorScheme.onSurface
                        )
                        Text(
                            text = "Penghapusan hanya berlaku di perangkat ini.",
                            style = MaterialTheme.typography.bodyMedium,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                        Button(
                            onClick = onDeleted,
                            modifier = Modifier.fillMaxWidth()
                        ) {
                            Text("Kembali")
                        }
                    }
                }
                return@Column
            }

            val loaded = order!!
            // Header Region (§12.3).
            Card(modifier = Modifier.fillMaxWidth()) {
                Column(
                    modifier = Modifier.padding(16.dp),
                    verticalArrangement = Arrangement.spacedBy(6.dp)
                ) {
                    DetailRow(
                        label = "Customer",
                        value = loaded.customerName.ifBlank { loaded.customerCode }
                    )
                    DetailRow(label = "Warehouse", value = loaded.warehouseCode)
                    // Optional references (ADR-RO-005): shown when captured.
                    DetailRow(
                        label = "Salesman",
                        value = loaded.salesPersonName.ifBlank { "-" }
                    )
                    DetailRow(
                        label = "Driver",
                        value = loaded.driverName.ifBlank { "-" }
                    )
                    DetailRow(
                        label = "Catatan",
                        value = loaded.note.ifBlank { "-" }
                    )
                    DetailRow(
                        label = "Status",
                        // Status vocabulary is exactly Draft / Synced (§14.4).
                        value = statusLabel(loaded.status),
                        emphasize = true
                    )
                }
            }

            // Item Region (Item, Qty, Unit, Return Type; §12.3).
            Text(
                text = "Item",
                style = MaterialTheme.typography.titleMedium.copy(
                    fontWeight = FontWeight.Bold
                ),
                color = MaterialTheme.colorScheme.onSurface
            )
            if (items.isEmpty()) {
                Text(
                    text = "Tidak ada item.",
                    style = MaterialTheme.typography.bodyMedium,
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    modifier = Modifier.fillMaxWidth()
                )
            } else {
                items.forEach { item -> DetailItemCard(item) }
            }

            if (deleteError != null) {
                Text(
                    text = deleteError!!,
                    color = MaterialTheme.colorScheme.error,
                    style = MaterialTheme.typography.bodySmall,
                    textAlign = TextAlign.Center,
                    modifier = Modifier.fillMaxWidth()
                )
            }

            // Action Region (Edit, Delete — enabled only while Draft; §12.3,
            // IR-M7/M8). Delete is confirmed before running (§13.1).
            Button(
                onClick = { onEdit(loaded.returnOrderId) },
                enabled = viewModel.canEdit(),
                modifier = Modifier.fillMaxWidth()
            ) {
                Text("Edit")
            }
            OutlinedButton(
                onClick = { showDeleteConfirm = true },
                enabled = viewModel.canDelete(),
                modifier = Modifier.fillMaxWidth()
            ) {
                if (isDeleting) {
                    CircularProgressIndicator()
                } else {
                    Text("Hapus")
                }
            }
            OutlinedButton(
                onClick = onBack,
                enabled = !isDeleting,
                modifier = Modifier.fillMaxWidth()
            ) {
                Text("Kembali")
            }

            Spacer(modifier = Modifier.height(8.dp))
        }
    }

    if (showDeleteConfirm) {
        AlertDialog(
            onDismissRequest = { if (!isDeleting) showDeleteConfirm = false },
            title = { Text("Hapus Return Order") },
            text = {
                Text(
                    "Return Order Draft ini akan dihapus dari perangkat. " +
                        "Tindakan ini tidak dapat dibatalkan."
                )
            },
            confirmButton = {
                Button(
                    onClick = {
                        showDeleteConfirm = false
                        viewModel.delete()
                    },
                    enabled = !isDeleting
                ) {
                    Text("Hapus")
                }
            },
            dismissButton = {
                OutlinedButton(
                    onClick = { showDeleteConfirm = false },
                    enabled = !isDeleting
                ) {
                    Text("Batal")
                }
            }
        )
    }
}

/** One header field of the Detail view (§12.3). */
@Composable
private fun DetailRow(
    label: String,
    value: String,
    emphasize: Boolean = false
) {
    Column(verticalArrangement = Arrangement.spacedBy(0.dp)) {
        Text(
            text = label,
            style = MaterialTheme.typography.labelSmall,
            color = MaterialTheme.colorScheme.onSurfaceVariant
        )
        Text(
            text = value.ifBlank { "-" },
            style = if (emphasize) {
                MaterialTheme.typography.bodyMedium.copy(fontWeight = FontWeight.Bold)
            } else {
                MaterialTheme.typography.bodyMedium
            },
            color = MaterialTheme.colorScheme.onSurface
        )
    }
}

/** One item line of the Detail view: Item, Qty, Unit, Return Type (§12.3). */
@Composable
private fun DetailItemCard(item: ReturnOrderItemEntity) {
    Card(modifier = Modifier.fillMaxWidth()) {
        Column(
            modifier = Modifier.padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(2.dp)
        ) {
            Text(
                text = item.brgName.ifBlank { item.brgCode },
                style = MaterialTheme.typography.bodyMedium.copy(
                    fontWeight = FontWeight.Medium
                ),
                color = MaterialTheme.colorScheme.onSurface
            )
            Text(
                text = item.brgCode,
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
            // Qty is the recorded physical-unit quantity — no small-unit
            // normalization (P-09, ADR-RO-003).
            Text(
                text = formatQty(item.qty) + " " + item.satId + " \u2022 " + item.jenisRetur,
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
        }
    }
}

/** Device status vocabulary is exactly Draft / Synced (§14.4, ADR-RO-006). */
private fun statusLabel(status: String): String = when (status) {
    ReturnOrderEntity.STATUS_DRAFT -> "Draft"
    ReturnOrderEntity.STATUS_SYNCED -> "Synced"
    else -> status
}

private fun formatQty(qty: Double): String {
    return if (qty % 1.0 == 0.0) qty.toLong().toString() else qty.toString()
}
