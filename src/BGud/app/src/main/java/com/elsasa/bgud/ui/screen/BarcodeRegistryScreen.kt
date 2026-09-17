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
import com.elsasa.bgud.model.BarcodeEntity
import com.elsasa.bgud.viewmodel.BarcodeRegistryViewModel

/**
 * Barcode Registry list screen (SCR-MOB-005, Architecture §12.7, UX Blueprint §9).
 *
 * ```text
 * Search Bar       (Barcode, Item Code, Item Name)
 * Result List      (Barcode, Item Code, Item Name, Unit)
 * Row Action       (Edit Barcode)
 * Empty State
 * ```
 *
 * Local-only source (§17.2): every query hits Room `barcode_entity` — no
 * network call is issued from this screen. Search requires a minimum of 3
 * characters with 300 ms debounce; a shorter query shows the default 50-row
 * page (§20). Paging is incremental — more rows load on scroll end (§20).
 * Row tap navigates to Edit Barcode (`edit?barcodeId={id}`, §13.2; the edit
 * destination itself is owned by S5.9).
 */
@Composable
fun BarcodeRegistryScreen(
    viewModel: BarcodeRegistryViewModel,
    onEditBarcode: (barcodeId: String) -> Unit,
    onBack: () -> Unit
) {
    val query by viewModel.query.collectAsState()
    val results by viewModel.results.collectAsState()
    val isEmpty by viewModel.isEmpty.collectAsState()
    val isLoading by viewModel.isLoading.collectAsState()
    val isLoadingMore by viewModel.isLoadingMore.collectAsState()
    val hasMore by viewModel.hasMore.collectAsState()

    Scaffold(
        containerColor = MaterialTheme.colorScheme.surface
    ) { paddingValues ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
                .padding(horizontal = 16.dp, vertical = 16.dp),
            verticalArrangement = Arrangement.spacedBy(12.dp)
        ) {
            Text(
                text = "Barcode Registry",
                style = MaterialTheme.typography.headlineSmall.copy(
                    fontWeight = FontWeight.Bold
                ),
                color = MaterialTheme.colorScheme.onSurface
            )

            // Search Bar (Barcode, Item Code, Item Name, §12.7).
            OutlinedTextField(
                value = query,
                onValueChange = viewModel::onQueryChange,
                modifier = Modifier.fillMaxWidth(),
                label = { Text("Cari barcode / kode / nama item") },
                singleLine = true,
                keyboardOptions = KeyboardOptions(imeAction = ImeAction.Search),
                keyboardActions = KeyboardActions(
                    onSearch = { viewModel.onSearchSubmitted() }
                )
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
                }
                isEmpty -> {
                    // Empty State (§12.7).
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
                            key = { _, item -> item.brgBarcodeId }
                        ) { index, item ->
                            RegistryRow(
                                item = item,
                                onEdit = { onEditBarcode(item.brgBarcodeId) }
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
private fun RegistryRow(
    item: BarcodeEntity,
    onEdit: () -> Unit
) {
    // Row Action: tap navigates to Edit Barcode (§12.7, §13.2).
    ListItem(
        headlineContent = { Text(item.barcodeValue) },
        supportingContent = {
            Text(
                item.brgCode + " — " + item.brgName +
                    if (item.satuan.isNotBlank()) " (" + item.satuan + ")" else ""
            )
        },
        modifier = Modifier.clickable(onClick = onEdit)
    )
}
