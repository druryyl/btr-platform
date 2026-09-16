package com.elsasa.bgud.ui.screen

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.text.KeyboardActions
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.ExposedDropdownMenuBox
import androidx.compose.material3.ExposedDropdownMenuDefaults
import androidx.compose.material3.ListItem
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.MenuAnchorType
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.OutlinedTextField
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
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import com.elsasa.bgud.model.BarangEntity
import com.elsasa.bgud.viewmodel.EditBarcodeViewModel

/**
 * Edit Barcode screen (SCR-MOB-006, Architecture §12.8, UX Blueprint §10).
 *
 * ```text
 * Barcode Field    (read-only)
 * Item Search      (local Barang cache)
 * Unit Selector    (optional)
 * Actions          (Save, Cancel)
 * ```
 *
 * Correction capture (§14.4, P-08): `Save` writes a `PENDING` correction
 * carrying the barcode identity to the local request queue only — no
 * network is required. The correction becomes authoritative only when the
 * Main Office accepts it during synchronization (submitted later via I-04
 * by the S5.3 sync worker).
 *
 * Guardrails (§18.4): the Item must exist in the local Barang cache and be
 * Active (BQ-7) — otherwise "Item sudah tidak aktif." (IR-M3); Unit is
 * optional and never gates `Save` (BR-005). Only `BrgId` and/or `Satuan`
 * change (INV-07); the barcode value is never editable (UX Blueprint §10)
 * and no activation/deactivation surface exists on mobile (IR-06, C-1).
 *
 * Navigation (§13.2): `barcode_registry` row ─▶ `edit?barcodeId={id}`;
 * `Save` shows the success message and the caller goes back; `Cancel`
 * goes back.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun EditBarcodeScreen(
    viewModel: EditBarcodeViewModel,
    onBack: () -> Unit
) {
    val isLoading by viewModel.isLoading.collectAsState()
    val notFound by viewModel.notFound.collectAsState()
    val original by viewModel.original.collectAsState()
    val barcodeValue by viewModel.barcodeValue.collectAsState()
    val itemQuery by viewModel.itemQuery.collectAsState()
    val itemResults by viewModel.itemResults.collectAsState()
    val isSearching by viewModel.isSearching.collectAsState()
    val selectedItem by viewModel.selectedItem.collectAsState()
    val selectedUnit by viewModel.selectedUnit.collectAsState()
    val isDirty by viewModel.isDirty.collectAsState()
    val isSaving by viewModel.isSaving.collectAsState()
    val saveError by viewModel.saveError.collectAsState()
    val saved by viewModel.saved.collectAsState()

    var unitExpanded by remember { mutableStateOf(false) }
    val unitOptions = viewModel.unitOptions()
    val unitLabel = selectedUnit.ifBlank { "Pilih satuan (opsional)" }

    // `Save` is enabled only when the selection differs from the loaded
    // mapping (`Dirty`, §14.4) and the selected Item is Active (BQ-7);
    // Unit never gates `Save` (BR-005).
    val canSave = !isLoading && !notFound && !saved && !isSaving &&
        isDirty &&
        selectedItem != null && selectedItem!!.isAktif

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
                text = "Edit Barcode",
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
                notFound || original == null -> {
                    // Unknown id: nothing to correct.
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

            if (saved) {
                // Saved (success message) ──▶ back (§13.2, §14.4).
                Card(modifier = Modifier.fillMaxWidth()) {
                    Column(
                        modifier = Modifier.padding(16.dp),
                        verticalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        Text(
                            text = "Koreksi tersimpan di antrean lokal.",
                            style = MaterialTheme.typography.titleMedium.copy(
                                fontWeight = FontWeight.Bold
                            ),
                            color = MaterialTheme.colorScheme.onSurface
                        )
                        Text(
                            text = "Akan dikirim saat sinkronisasi.",
                            style = MaterialTheme.typography.bodyMedium,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                        Button(
                            onClick = onBack,
                            modifier = Modifier.fillMaxWidth()
                        ) {
                            Text("Kembali")
                        }
                    }
                }
                return@Column
            }

            // Barcode Field (read-only, never editable, §12.8).
            OutlinedTextField(
                value = barcodeValue,
                onValueChange = {},
                readOnly = true,
                modifier = Modifier.fillMaxWidth(),
                label = { Text("Barcode") }
            )

            // Loaded mapping summary: the correction target (§14.4
            // `Loaded`; INV-07 changes BrgId and/or Satuan only).
            val loaded = original!!
            Card(modifier = Modifier.fillMaxWidth()) {
                Column(
                    modifier = Modifier.padding(16.dp),
                    verticalArrangement = Arrangement.spacedBy(2.dp)
                ) {
                    Text(
                        text = "Mapping Saat Ini",
                        style = MaterialTheme.typography.titleSmall.copy(
                            fontWeight = FontWeight.Bold
                        ),
                        color = MaterialTheme.colorScheme.onSurface
                    )
                    Text(
                        text = loaded.brgCode,
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                    Text(
                        text = loaded.brgName,
                        style = MaterialTheme.typography.bodyMedium.copy(
                            fontWeight = FontWeight.Medium
                        ),
                        color = MaterialTheme.colorScheme.onSurface
                    )
                    if (loaded.satuan.isNotBlank()) {
                        Text(
                            text = loaded.satuan,
                            style = MaterialTheme.typography.bodySmall,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    }
                }
            }

            if (selectedItem == null) {
                // Item Search (local Barang cache, §12.8).
                OutlinedTextField(
                    value = itemQuery,
                    onValueChange = viewModel::onQueryChange,
                    modifier = Modifier.fillMaxWidth(),
                    label = { Text("Cari Item (kode / nama)") },
                    singleLine = true,
                    enabled = !isSaving,
                    keyboardOptions = KeyboardOptions(imeAction = ImeAction.Search),
                    keyboardActions = KeyboardActions(
                        onSearch = { viewModel.onQueryChange(itemQuery) }
                    )
                )

                when {
                    isSearching -> {
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.Center
                        ) {
                            CircularProgressIndicator()
                        }
                    }
                    itemQuery.trim().isNotEmpty() && itemResults.isEmpty() -> {
                        // Empty: Item not found in the local cache.
                        Text(
                            text = "Item tidak ditemukan.",
                            style = MaterialTheme.typography.bodyMedium,
                            color = MaterialTheme.colorScheme.onSurfaceVariant,
                            textAlign = TextAlign.Center,
                            modifier = Modifier.fillMaxWidth()
                        )
                    }
                    itemResults.isNotEmpty() -> {
                        // Item Result List (selectable, §12.8).
                        Column(verticalArrangement = Arrangement.spacedBy(0.dp)) {
                            itemResults.forEach { item ->
                                EditItemResultRow(
                                    item = item,
                                    onSelect = { viewModel.onSelectItem(item) }
                                )
                            }
                        }
                    }
                }
            } else {
                // Item Selected — shows the chosen cached Item; search is
                // replaced until the selection is cleared.
                EditSelectedItemCard(
                    item = selectedItem!!,
                    onClear = viewModel::onClearSelection,
                    enabled = !isSaving
                )
            }

            // Inactive Item guard (IR-M3, BQ-7).
            if (selectedItem != null && !selectedItem!!.isAktif) {
                Text(
                    text = "Item sudah tidak aktif.",
                    style = MaterialTheme.typography.bodyMedium,
                    color = MaterialTheme.colorScheme.error
                )
            }

            // Unit Selector (optional; Small Unit / Big Unit from the
            // selected Item, §12.8; never gates Save, BR-005).
            if (selectedItem != null && selectedItem!!.isAktif) {
                ExposedDropdownMenuBox(
                    expanded = unitExpanded,
                    onExpandedChange = { if (!isSaving) unitExpanded = !unitExpanded }
                ) {
                    OutlinedTextField(
                        value = unitLabel,
                        onValueChange = {},
                        readOnly = true,
                        enabled = !isSaving,
                        modifier = Modifier
                            .menuAnchor(MenuAnchorType.PrimaryNotEditable)
                            .fillMaxWidth(),
                        label = { Text("Unit") },
                        trailingIcon = {
                            ExposedDropdownMenuDefaults.TrailingIcon(unitExpanded)
                        },
                        colors = ExposedDropdownMenuDefaults.outlinedTextFieldColors()
                    )
                    ExposedDropdownMenu(
                        expanded = unitExpanded,
                        onDismissRequest = { unitExpanded = false }
                    ) {
                        DropdownMenuItem(
                            text = { Text("Tanpa satuan") },
                            onClick = {
                                viewModel.onUnitChange("")
                                unitExpanded = false
                            }
                        )
                        unitOptions.forEach { unit ->
                            DropdownMenuItem(
                                text = { Text(unit) },
                                onClick = {
                                    viewModel.onUnitChange(unit)
                                    unitExpanded = false
                                }
                            )
                        }
                    }
                }
            }

            if (saveError != null) {
                Text(
                    text = saveError!!,
                    color = MaterialTheme.colorScheme.error,
                    style = MaterialTheme.typography.bodySmall,
                    textAlign = TextAlign.Center,
                    modifier = Modifier.fillMaxWidth()
                )
            }

            // Actions (Save, Cancel, §12.8). No activation/deactivation
            // surface exists on mobile (IR-06, C-1).
            Button(
                onClick = viewModel::save,
                enabled = canSave,
                modifier = Modifier.fillMaxWidth()
            ) {
                if (isSaving) {
                    CircularProgressIndicator()
                } else {
                    Text("Save")
                }
            }
            OutlinedButton(
                onClick = onBack,
                enabled = !isSaving,
                modifier = Modifier.fillMaxWidth()
            ) {
                Text("Cancel")
            }

            Spacer(modifier = Modifier.height(8.dp))
        }
    }
}

@Composable
private fun EditItemResultRow(
    item: BarangEntity,
    onSelect: () -> Unit
) {
    ListItem(
        headlineContent = { Text(item.brgName) },
        supportingContent = {
            Text(
                item.brgCode +
                    if (!item.isAktif) " (nonaktif)" else ""
            )
        },
        modifier = Modifier.clickable(onClick = onSelect)
    )
}

@Composable
private fun EditSelectedItemCard(
    item: BarangEntity,
    onClear: () -> Unit,
    enabled: Boolean
) {
    Card(modifier = Modifier.fillMaxWidth()) {
        Column(
            modifier = Modifier.padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(2.dp)
        ) {
            Text(
                text = "Item Terpilih",
                style = MaterialTheme.typography.titleSmall.copy(
                    fontWeight = FontWeight.Bold
                ),
                color = MaterialTheme.colorScheme.onSurface
            )
            Text(
                text = item.brgCode,
                style = MaterialTheme.typography.bodyMedium,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
            Text(
                text = item.brgName,
                style = MaterialTheme.typography.bodyMedium.copy(
                    fontWeight = FontWeight.Medium
                ),
                color = MaterialTheme.colorScheme.onSurface
            )
            Spacer(modifier = Modifier.height(8.dp))
            OutlinedButton(
                onClick = onClear,
                enabled = enabled,
                modifier = Modifier.fillMaxWidth()
            ) {
                Text("Hapus Pilihan")
            }
        }
    }
}
