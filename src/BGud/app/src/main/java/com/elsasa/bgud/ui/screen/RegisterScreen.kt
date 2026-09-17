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
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import com.elsasa.bgud.model.BarangEntity
import com.elsasa.bgud.viewmodel.RegisterBarcodeViewModel

/**
 * Register Barcode screen (SCR-MOB-004, Architecture §12.6, UX Blueprint §8).
 *
 * ```text
 * Barcode Field    (read-only, captured from scanner)
 * Item Search      (by Item Code or Item Name; local Barang cache)
 * Item Result List (selectable)
 * Unit Selector    (optional; Small Unit / Big Unit from the selected Item)
 * Actions          (Save, Cancel)
 * ```
 *
 * Offline capture (§14.3, P-08): `Save` writes a `PENDING` row to the local
 * request queue only — no network is required ("No immediate server
 * communication is required", UX Blueprint §8). The request becomes
 * authoritative only when the Main Office accepts it during synchronization
 * (submitted later via I-04 by the S5.3 sync worker).
 *
 * Guardrails (§18.4): the Item must exist in the local Barang cache and be
 * Active (BQ-7) — otherwise "Item sudah tidak aktif." (IR-M3); a barcode
 * already in the local cache surfaces "Barcode sudah terdaftar." (IR-M4)
 * and disables `Save`. Unit is optional and never gates `Save` (BR-005).
 *
 * Navigation (§13.2): `register` carries an optional barcode argument
 * (`register?barcode={value}`, S5.6 Not Found → Register); `Save` shows the
 * success message and the caller goes back; `Cancel` goes back.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun RegisterScreen(
    viewModel: RegisterBarcodeViewModel,
    onBack: () -> Unit
) {
    val barcodeValue by viewModel.barcodeValue.collectAsState()
    val itemQuery by viewModel.itemQuery.collectAsState()
    val itemResults by viewModel.itemResults.collectAsState()
    val isSearching by viewModel.isSearching.collectAsState()
    val selectedItem by viewModel.selectedItem.collectAsState()
    val selectedUnit by viewModel.selectedUnit.collectAsState()
    val duplicateFound by viewModel.duplicateFound.collectAsState()
    val isSaving by viewModel.isSaving.collectAsState()
    val saveError by viewModel.saveError.collectAsState()
    val saved by viewModel.saved.collectAsState()

    var unitExpanded by remember { mutableStateOf(false) }
    val unitOptions = viewModel.unitOptions()
    val unitLabel = selectedUnit.ifBlank { "Pilih satuan (opsional)" }

    // `Save` is enabled only when Barcode is present and an Active cached
    // Item is selected (BQ-7, §14.3); Unit never gates `Save` (BR-005).
    val canSave = !saved && !isSaving &&
        barcodeValue.isNotBlank() &&
        selectedItem != null && selectedItem!!.isAktif &&
        duplicateFound == null

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
                text = "Register Barcode",
                style = MaterialTheme.typography.headlineSmall.copy(
                    fontWeight = FontWeight.Bold
                ),
                color = MaterialTheme.colorScheme.onSurface
            )

            if (saved) {
                // Saved (success message) ──▶ back (§13.2, §14.3).
                Card(modifier = Modifier.fillMaxWidth()) {
                    Column(
                        modifier = Modifier.padding(16.dp),
                        verticalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        Text(
                            text = "Registrasi tersimpan di antrean lokal.",
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

            // Barcode Field (read-only, captured from scanner, §12.6).
            OutlinedTextField(
                value = barcodeValue,
                onValueChange = {},
                readOnly = true,
                modifier = Modifier.fillMaxWidth(),
                label = { Text("Barcode") }
            )

            // Local duplicate detection (IR-M4): best-effort; the Main
            // Office remains authoritative (§18.4).
            if (duplicateFound != null) {
                Card(modifier = Modifier.fillMaxWidth()) {
                    Column(
                        modifier = Modifier.padding(16.dp),
                        verticalArrangement = Arrangement.spacedBy(4.dp)
                    ) {
                        Text(
                            text = "Barcode sudah terdaftar.",
                            style = MaterialTheme.typography.bodyMedium.copy(
                                fontWeight = FontWeight.Bold
                            ),
                            color = MaterialTheme.colorScheme.error
                        )
                        Text(
                            text = "${duplicateFound!!.brgCode} — ${duplicateFound!!.brgName}",
                            style = MaterialTheme.typography.bodySmall,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    }
                }
            }

            if (selectedItem == null) {
                // Item Search (by Item Code or Item Name; local Barang
                // cache, §12.6).
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
                        // Empty (IR-M2): Item not found in the local cache.
                        Text(
                            text = "Item tidak ditemukan.",
                            style = MaterialTheme.typography.bodyMedium,
                            color = MaterialTheme.colorScheme.onSurfaceVariant,
                            textAlign = TextAlign.Center,
                            modifier = Modifier.fillMaxWidth()
                        )
                    }
                    itemResults.isNotEmpty() -> {
                        // Item Result List (selectable, §12.6).
                        Column(verticalArrangement = Arrangement.spacedBy(0.dp)) {
                            itemResults.forEach { item ->
                                ItemResultRow(
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
                SelectedItemCard(
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
            // selected Item, §12.6; never gates Save, BR-005).
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

            // Actions (Save, Cancel, §12.6).
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
private fun ItemResultRow(
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
private fun SelectedItemCard(
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
