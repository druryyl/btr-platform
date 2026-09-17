package com.elsasa.bgud.ui.screen

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.itemsIndexed
import androidx.compose.foundation.text.KeyboardActions
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.FilterChip
import androidx.compose.material3.FloatingActionButton
import androidx.compose.material3.ListItem
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import com.elsasa.bgud.model.ReturnOrderEntity
import com.elsasa.bgud.viewmodel.ReturnOrderListItem
import com.elsasa.bgud.viewmodel.ReturnOrderListViewModel
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

/**
 * Return Order List screen (SCR-MOB-RO-001, Architecture §11.1, §12.1, §14.4).
 *
 * ```text
 * Search Bar        (Customer name / date / status)
 * Result List       (Customer, date, item count, status badge)
 * Row Action        (open Detail)
 * Create Action     (FAB / toolbar -> Create)
 * ```
 *
 * Local-only source (§17.1): every query hits Room `return_order_entity` — no
 * network call is issued from this screen. Search requires a minimum of 3
 * characters with 300 ms debounce; a shorter query shows the default 50-row
 * page (§20). Paging is incremental — more rows load on scroll end (§20). The
 * status filter is `Draft`/`Synced` only (ADR-RO-006, §14.4). Row tap opens
 * Detail and the FAB opens Create (§12.1, §13.1); both destinations are owned
 * by S4.7/S4.8 and the routes are wired by S4.11.
 */
@Composable
fun ReturnOrderListScreen(
    viewModel: ReturnOrderListViewModel,
    onOpenDetail: (returnOrderId: String) -> Unit,
    onCreate: () -> Unit,
    onBack: () -> Unit
) {
    val query by viewModel.query.collectAsState()
    val statusFilter by viewModel.statusFilter.collectAsState()
    val results by viewModel.results.collectAsState()
    val isEmpty by viewModel.isEmpty.collectAsState()
    val isLoading by viewModel.isLoading.collectAsState()
    val isLoadingMore by viewModel.isLoadingMore.collectAsState()
    val hasMore by viewModel.hasMore.collectAsState()

    Scaffold(
        containerColor = MaterialTheme.colorScheme.surface,
        // Create Action: FAB -> Create (§12.1, §13.1).
        floatingActionButton = {
            FloatingActionButton(onClick = onCreate) {
                Text("+")
            }
        }
    ) { paddingValues ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
                .padding(horizontal = 16.dp, vertical = 16.dp),
            verticalArrangement = Arrangement.spacedBy(12.dp)
        ) {
            Text(
                text = "Return Order",
                style = MaterialTheme.typography.headlineSmall.copy(
                    fontWeight = FontWeight.Bold
                ),
                color = MaterialTheme.colorScheme.onSurface
            )

            // Search Bar (Customer name / date / status, §12.1).
            OutlinedTextField(
                value = query,
                onValueChange = viewModel::onQueryChange,
                modifier = Modifier.fillMaxWidth(),
                label = { Text("Cari customer / tanggal") },
                singleLine = true,
                keyboardOptions = KeyboardOptions(imeAction = ImeAction.Search),
                keyboardActions = KeyboardActions(
                    onSearch = { viewModel.onSearchSubmitted() }
                )
            )

            // Status filter: Draft / Synced only (§11.1, ADR-RO-006, §14.4).
            Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                ReturnOrderFilterChip(
                    label = "Semua",
                    selected = statusFilter == ReturnOrderListViewModel.STATUS_ALL,
                    onClick = {
                        viewModel.onStatusFilterChange(ReturnOrderListViewModel.STATUS_ALL)
                    }
                )
                ReturnOrderFilterChip(
                    label = "Draft",
                    selected = statusFilter == ReturnOrderEntity.STATUS_DRAFT,
                    onClick = {
                        viewModel.onStatusFilterChange(ReturnOrderEntity.STATUS_DRAFT)
                    }
                )
                ReturnOrderFilterChip(
                    label = "Synced",
                    selected = statusFilter == ReturnOrderEntity.STATUS_SYNCED,
                    onClick = {
                        viewModel.onStatusFilterChange(ReturnOrderEntity.STATUS_SYNCED)
                    }
                )
            }

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
                }
                isEmpty -> {
                    Text(
                        text = "Tidak ada data.",
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                        textAlign = TextAlign.Center,
                        modifier = Modifier
                            .fillMaxWidth()
                            .weight(1f)
                            .padding(top = 32.dp)
                    )
                }
                else -> {
                    LazyColumn(
                        modifier = Modifier.weight(1f),
                        verticalArrangement = Arrangement.spacedBy(0.dp)
                    ) {
                        itemsIndexed(
                            items = results,
                            key = { _, item -> item.order.returnOrderId }
                        ) { index, item ->
                            ReturnOrderRow(
                                item = item,
                                onClick = { onOpenDetail(item.order.returnOrderId) }
                            )
                            // Incremental paging: request the next page when
                            // the last row becomes visible (§20).
                            if (index == results.lastIndex && hasMore && !isLoadingMore) {
                                LaunchedEffect(results.size) {
                                    viewModel.loadMore()
                                }
                            }
                        }
                        if (isLoadingMore) {
                            item(key = "loading-more") {
                                Row(
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .padding(vertical = 12.dp),
                                    horizontalArrangement = Arrangement.Center
                                ) {
                                    CircularProgressIndicator()
                                }
                            }
                        }
                    }
                }
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
private fun ReturnOrderFilterChip(
    label: String,
    selected: Boolean,
    onClick: () -> Unit
) {
    FilterChip(
        selected = selected,
        onClick = onClick,
        label = { Text(label) }
    )
}

@Composable
private fun ReturnOrderRow(
    item: ReturnOrderListItem,
    onClick: () -> Unit
) {
    val order = item.order
    // Row Action: tap opens Detail (§12.1, §13.1).
    ListItem(
        headlineContent = {
            Text(order.customerName.ifBlank { order.customerCode })
        },
        supportingContent = {
            Text(formatOrderDate(order.createdAt) + " \u2022 " + item.itemCount + " item")
        },
        trailingContent = {
            // Status badge vocabulary is exactly Draft / Synced (§14.4).
            Text(
                text = statusLabel(order.status),
                style = MaterialTheme.typography.labelMedium.copy(
                    fontWeight = FontWeight.Medium
                ),
                color = MaterialTheme.colorScheme.primary
            )
        },
        modifier = Modifier.clickable(onClick = onClick)
    )
}

private fun statusLabel(status: String): String = when (status) {
    ReturnOrderEntity.STATUS_DRAFT -> "Draft"
    ReturnOrderEntity.STATUS_SYNCED -> "Synced"
    else -> status
}

private fun formatOrderDate(timestampMillis: Long): String {
    if (timestampMillis <= 0L) return "-"
    return SimpleDateFormat("dd MMM yyyy", Locale.getDefault())
        .format(Date(timestampMillis))
}
