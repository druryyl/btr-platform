package com.elsasa.bgud.ui.screen

import android.Manifest
import android.content.pm.PackageManager
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
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
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.ExposedDropdownMenuBox
import androidx.compose.material3.ExposedDropdownMenuDefaults
import androidx.compose.material3.FilterChip
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
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.core.content.ContextCompat
import com.elsasa.bgud.model.ReturnOrderItemEntity
import com.elsasa.bgud.ui.component.BarcodeScannerView
import com.elsasa.bgud.viewmodel.EditReturnOrderViewModel
import com.elsasa.bgud.viewmodel.ReturnOrderCaptureLine

/**
 * Edit Return Order screen (SCR-MOB-RO-004, Architecture §11.1, §12.4,
 * §14.2, §20).
 *
 * ```text
 * Header Region     (editable Customer/Salesman/Driver/Notes)
 * Item Region       (editable item lines)
 * Action Region     (Save, Cancel)
 * ```
 *
 * Maintenance mode for a `Draft` order (§15.1): the loaded fields are prefilled
 * from the local Room capture store and edited in place. A non-`DRAFT` order is
 * rejected on entry (BR-018); the Detail screen already never offers Edit for
 * `SYNCED` (IR-M7). Warehouse is the stored, session-bound code and is never
 * editable — a draft is never re-homed (BR-005/006).
 *
 * Capture is offline-first (BG-003): item identification reuses
 * [BarcodeScannerView] (scan) and the existing local Item Search (BR-009); no
 * step issues a network call (P-07, §17.1). `Save` is enabled only after a
 * change (§14.2 `Dirty`) with a Customer and at least one valid line, and writes
 * locally only — the order remains `Draft` (BR-017/018). Destination wiring
 * (routes) is owned by S4.11.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun EditReturnOrderScreen(
    viewModel: EditReturnOrderViewModel,
    onSaved: () -> Unit,
    onCancel: () -> Unit
) {
    val isLoading by viewModel.isLoading.collectAsState()
    val notFound by viewModel.notFound.collectAsState()
    val notEditable by viewModel.notEditable.collectAsState()
    val warehouseCode by viewModel.warehouseCode.collectAsState()
    val customer by viewModel.customer.collectAsState()
    val customerQuery by viewModel.customerQuery.collectAsState()
    val customerResults by viewModel.customerResults.collectAsState()
    val isSearchingCustomer by viewModel.isSearchingCustomer.collectAsState()
    val salesman by viewModel.salesman.collectAsState()
    val salesmanQuery by viewModel.salesmanQuery.collectAsState()
    val salesmanResults by viewModel.salesmanResults.collectAsState()
    val isSearchingSalesman by viewModel.isSearchingSalesman.collectAsState()
    val driver by viewModel.driver.collectAsState()
    val driverQuery by viewModel.driverQuery.collectAsState()
    val driverResults by viewModel.driverResults.collectAsState()
    val isSearchingDriver by viewModel.isSearchingDriver.collectAsState()
    val note by viewModel.note.collectAsState()
    val pendingItem by viewModel.pendingItem.collectAsState()
    val itemQuery by viewModel.itemQuery.collectAsState()
    val itemResults by viewModel.itemResults.collectAsState()
    val isSearchingItem by viewModel.isSearchingItem.collectAsState()
    val pendingQty by viewModel.pendingQty.collectAsState()
    val pendingUnit by viewModel.pendingUnit.collectAsState()
    val pendingJenisRetur by viewModel.pendingJenisRetur.collectAsState()
    val pendingError by viewModel.pendingError.collectAsState()
    val items by viewModel.items.collectAsState()
    val isSaving by viewModel.isSaving.collectAsState()
    val saveError by viewModel.saveError.collectAsState()
    val saved by viewModel.saved.collectAsState()

    val context = LocalContext.current
    var hasCameraPermission by remember {
        mutableStateOf(
            ContextCompat.checkSelfPermission(
                context,
                Manifest.permission.CAMERA
            ) == PackageManager.PERMISSION_GRANTED
        )
    }
    val permissionLauncher = rememberLauncherForActivityResult(
        ActivityResultContracts.RequestPermission()
    ) { granted ->
        hasCameraPermission = granted
    }

    var unitExpanded by remember { mutableStateOf(false) }
    val unitOptions = viewModel.unitOptions()
    val unitLabel = pendingUnit.ifBlank { "Pilih satuan" }

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
                text = "Edit Return Order",
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
                            .height(120.dp),
                        contentAlignment = Alignment.Center
                    ) {
                        CircularProgressIndicator()
                    }
                    return@Column
                }
                notFound -> {
                    // Unknown id: nothing to edit.
                    Text(
                        text = "Data tidak ditemukan.",
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                        textAlign = TextAlign.Center,
                        modifier = Modifier.fillMaxWidth()
                    )
                    OutlinedButton(
                        onClick = onCancel,
                        modifier = Modifier.fillMaxWidth()
                    ) {
                        Text("Kembali")
                    }
                    return@Column
                }
                notEditable -> {
                    // BR-018 — a synced order can never enter edit.
                    Card(modifier = Modifier.fillMaxWidth()) {
                        Column(
                            modifier = Modifier.padding(16.dp),
                            verticalArrangement = Arrangement.spacedBy(8.dp)
                        ) {
                            Text(
                                text = "Tidak dapat diubah.",
                                style = MaterialTheme.typography.titleMedium.copy(
                                    fontWeight = FontWeight.Bold
                                ),
                                color = MaterialTheme.colorScheme.onSurface
                            )
                            Text(
                                text = "Return Order yang sudah disinkronkan " +
                                    "hanya dapat dilihat.",
                                style = MaterialTheme.typography.bodyMedium,
                                color = MaterialTheme.colorScheme.onSurfaceVariant
                            )
                            Button(
                                onClick = onCancel,
                                modifier = Modifier.fillMaxWidth()
                            ) {
                                Text("Kembali")
                            }
                        }
                    }
                    return@Column
                }
            }

            if (saved) {
                // Saved (§14.2) ──▶ back (local write only, no network).
                Card(modifier = Modifier.fillMaxWidth()) {
                    Column(
                        modifier = Modifier.padding(16.dp),
                        verticalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        Text(
                            text = "Perubahan tersimpan sebagai Draft.",
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
                            onClick = onSaved,
                            modifier = Modifier.fillMaxWidth()
                        ) {
                            Text("Kembali")
                        }
                    }
                }
                return@Column
            }

            // Header Region: Warehouse is the stored session binding and never
            // editable — a draft is never re-homed (BR-005/006).
            OutlinedTextField(
                value = warehouseCode,
                onValueChange = {},
                enabled = false,
                modifier = Modifier.fillMaxWidth(),
                label = { Text("Warehouse") }
            )

            // Header Region: Customer picker — mandatory, local cache
            // (BR-001/002, GAP-004).
            if (customer == null) {
                ReferenceSearchSection(
                    label = "Cari Customer (kode / nama)",
                    query = customerQuery,
                    results = customerResults,
                    isSearching = isSearchingCustomer,
                    enabled = !isSaving,
                    onQueryChange = viewModel::onCustomerQueryChange,
                    onSelect = viewModel::onSelectCustomer,
                    rowHeadline = { it.customerName },
                    rowSupporting = { it.customerCode }
                )
            } else {
                SelectedReferenceCard(
                    label = "Customer",
                    title = customer!!.customerName.ifBlank { customer!!.customerCode },
                    subtitle = customer!!.customerCode,
                    onClear = viewModel::onClearCustomer,
                    enabled = !isSaving
                )
            }

            // Header Region: Salesman picker — optional (BR-013/014,
            // ADR-RO-005).
            if (salesman == null) {
                ReferenceSearchSection(
                    label = "Cari Salesman (opsional)",
                    query = salesmanQuery,
                    results = salesmanResults,
                    isSearching = isSearchingSalesman,
                    enabled = !isSaving,
                    onQueryChange = viewModel::onSalesmanQueryChange,
                    onSelect = viewModel::onSelectSalesman,
                    rowHeadline = { it.salesPersonName },
                    rowSupporting = { it.salesPersonId }
                )
            } else {
                SelectedReferenceCard(
                    label = "Salesman",
                    title = salesman!!.salesPersonName,
                    subtitle = salesman!!.salesPersonId,
                    onClear = viewModel::onClearSalesman,
                    enabled = !isSaving
                )
            }

            // Header Region: Driver picker — optional (BR-015/016,
            // ADR-RO-005).
            if (driver == null) {
                ReferenceSearchSection(
                    label = "Cari Driver (opsional)",
                    query = driverQuery,
                    results = driverResults,
                    isSearching = isSearchingDriver,
                    enabled = !isSaving,
                    onQueryChange = viewModel::onDriverQueryChange,
                    onSelect = viewModel::onSelectDriver,
                    rowHeadline = { it.driverName },
                    rowSupporting = { it.driverId }
                )
            } else {
                SelectedReferenceCard(
                    label = "Driver",
                    title = driver!!.driverName,
                    subtitle = driver!!.driverId,
                    onClear = viewModel::onClearDriver,
                    enabled = !isSaving
                )
            }

            // Header Region: Notes.
            OutlinedTextField(
                value = note,
                onValueChange = viewModel::onNoteChange,
                modifier = Modifier.fillMaxWidth(),
                label = { Text("Catatan") },
                enabled = !isSaving
            )

            // Item Region (item lines: Item [scan/search], Qty, Unit, Return
            // Type; add/remove lines, §12.4).
            Text(
                text = "Item",
                style = MaterialTheme.typography.titleMedium.copy(
                    fontWeight = FontWeight.Bold
                ),
                color = MaterialTheme.colorScheme.onSurface
            )

            if (pendingItem == null) {
                // Item identification — Barcode Scan (BR-009), local cache
                // only (P-07).
                if (hasCameraPermission) {
                    BarcodeScannerView(
                        enabled = !isSaving,
                        onBarcode = viewModel::onBarcodeScanned,
                        modifier = Modifier
                            .fillMaxWidth()
                            .height(240.dp)
                    )
                } else {
                    Card(modifier = Modifier.fillMaxWidth()) {
                        Column(
                            modifier = Modifier.padding(16.dp),
                            verticalArrangement = Arrangement.spacedBy(8.dp)
                        ) {
                            Text(
                                text = "Izin kamera belum diberikan.",
                                style = MaterialTheme.typography.bodyMedium,
                                color = MaterialTheme.colorScheme.onSurface
                            )
                            Button(
                                onClick = {
                                    permissionLauncher.launch(Manifest.permission.CAMERA)
                                },
                                modifier = Modifier.fillMaxWidth()
                            ) {
                                Text("Berikan Izin Kamera")
                            }
                        }
                    }
                }

                // Manual Item Search — local Barang cache only (BR-009).
                ReferenceSearchSection(
                    label = "Cari Item (kode / nama)",
                    query = itemQuery,
                    results = itemResults,
                    isSearching = isSearchingItem,
                    enabled = !isSaving,
                    onQueryChange = viewModel::onItemQueryChange,
                    onSelect = viewModel::onSelectItem,
                    rowHeadline = { it.brgName },
                    rowSupporting = { it.brgCode }
                )
            } else {
                SelectedReferenceCard(
                    label = "Item",
                    title = pendingItem!!.brgName,
                    subtitle = pendingItem!!.brgCode,
                    onClear = viewModel::onClearPendingItem,
                    enabled = !isSaving
                )

                // Qty > 0 (BR-010, IR-M4).
                OutlinedTextField(
                    value = pendingQty,
                    onValueChange = viewModel::onQtyChange,
                    modifier = Modifier.fillMaxWidth(),
                    label = { Text("Qty") },
                    singleLine = true,
                    enabled = !isSaving,
                    keyboardOptions = KeyboardOptions(
                        keyboardType = KeyboardType.Decimal,
                        imeAction = ImeAction.Next
                    )
                )

                // Unit — mandatory, from the item's cached unit data
                // (BR-011, IR-M5, ADR-RO-003).
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
                        label = { Text("Satuan") },
                        trailingIcon = {
                            ExposedDropdownMenuDefaults.TrailingIcon(unitExpanded)
                        },
                        colors = ExposedDropdownMenuDefaults.outlinedTextFieldColors()
                    )
                    ExposedDropdownMenu(
                        expanded = unitExpanded,
                        onDismissRequest = { unitExpanded = false }
                    ) {
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

                // Return Type — exactly BAGUS/RUSAK (ADR-RO-004, IR-M6).
                Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    ReturnTypeChip(
                        label = ReturnOrderItemEntity.JENIS_RETUR_BAGUS,
                        selected = pendingJenisRetur ==
                            ReturnOrderItemEntity.JENIS_RETUR_BAGUS,
                        onClick = {
                            viewModel.onJenisReturChange(
                                ReturnOrderItemEntity.JENIS_RETUR_BAGUS
                            )
                        }
                    )
                    ReturnTypeChip(
                        label = ReturnOrderItemEntity.JENIS_RETUR_RUSAK,
                        selected = pendingJenisRetur ==
                            ReturnOrderItemEntity.JENIS_RETUR_RUSAK,
                        onClick = {
                            viewModel.onJenisReturChange(
                                ReturnOrderItemEntity.JENIS_RETUR_RUSAK
                            )
                        }
                    )
                }

                Button(
                    onClick = viewModel::addItem,
                    enabled = !isSaving,
                    modifier = Modifier.fillMaxWidth()
                ) {
                    Text("Tambah Item")
                }
            }

            // Per-line guardrail message (IR-M3…IR-M6).
            if (pendingError != null) {
                Text(
                    text = pendingError!!,
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.error,
                    modifier = Modifier.fillMaxWidth()
                )
            }

            // Editable lines, prefilled from the stored order and modifiable
            // (add/remove lines, §12.4).
            items.forEachIndexed { index, line ->
                CaptureLineCard(
                    line = line,
                    onRemove = { viewModel.removeItem(index) },
                    enabled = !isSaving
                )
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

            // Action Region (Save, Cancel, §12.4). Save is enabled only after a
            // change with a Customer and at least one valid line (§14.2).
            Button(
                onClick = viewModel::save,
                enabled = viewModel.canSave(),
                modifier = Modifier.fillMaxWidth()
            ) {
                if (isSaving) {
                    CircularProgressIndicator()
                } else {
                    Text("Save")
                }
            }
            OutlinedButton(
                onClick = onCancel,
                enabled = !isSaving,
                modifier = Modifier.fillMaxWidth()
            ) {
                Text("Cancel")
            }

            Spacer(modifier = Modifier.height(8.dp))
        }
    }
}

/**
 * Local-cache reference search section (Customer / Salesman / Driver / Item):
 * a debounced query field plus its selectable local result list — no network
 * call (P-07, §17.1).
 */
@Composable
private fun <T> ReferenceSearchSection(
    label: String,
    query: String,
    results: List<T>,
    isSearching: Boolean,
    enabled: Boolean,
    onQueryChange: (String) -> Unit,
    onSelect: (T) -> Unit,
    rowHeadline: (T) -> String,
    rowSupporting: (T) -> String
) {
    OutlinedTextField(
        value = query,
        onValueChange = onQueryChange,
        modifier = Modifier.fillMaxWidth(),
        label = { Text(label) },
        singleLine = true,
        enabled = enabled,
        keyboardOptions = KeyboardOptions(imeAction = ImeAction.Search)
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
        query.trim().isNotEmpty() && results.isEmpty() -> {
            // Item must exist in the local cache (BR-008, IR-M3).
            Text(
                text = "Tidak ditemukan pada cache lokal.",
                style = MaterialTheme.typography.bodyMedium,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                textAlign = TextAlign.Center,
                modifier = Modifier.fillMaxWidth()
            )
        }
        results.isNotEmpty() -> {
            Column(verticalArrangement = Arrangement.spacedBy(0.dp)) {
                results.forEach { result ->
                    ListItem(
                        headlineContent = { Text(rowHeadline(result)) },
                        supportingContent = { Text(rowSupporting(result)) },
                        modifier = Modifier.clickable(enabled = enabled) {
                            onSelect(result)
                        }
                    )
                }
            }
        }
    }
}

/** Selected Customer/Salesman/Driver/Item card with a clear action. */
@Composable
private fun SelectedReferenceCard(
    label: String,
    title: String,
    subtitle: String,
    onClear: () -> Unit,
    enabled: Boolean
) {
    Card(modifier = Modifier.fillMaxWidth()) {
        Column(
            modifier = Modifier.padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(2.dp)
        ) {
            Text(
                text = label,
                style = MaterialTheme.typography.titleSmall.copy(
                    fontWeight = FontWeight.Bold
                ),
                color = MaterialTheme.colorScheme.onSurface
            )
            Text(
                text = title.ifBlank { subtitle },
                style = MaterialTheme.typography.bodyMedium.copy(
                    fontWeight = FontWeight.Medium
                ),
                color = MaterialTheme.colorScheme.onSurface
            )
            if (subtitle.isNotBlank() && title.isNotBlank()) {
                Text(
                    text = subtitle,
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }
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

/** One editable item line with its remove action (§12.4). */
@Composable
private fun CaptureLineCard(
    line: ReturnOrderCaptureLine,
    onRemove: () -> Unit,
    enabled: Boolean
) {
    Card(modifier = Modifier.fillMaxWidth()) {
        Column(
            modifier = Modifier.padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(2.dp)
        ) {
            Text(
                text = line.brgName.ifBlank { line.brgCode },
                style = MaterialTheme.typography.bodyMedium.copy(
                    fontWeight = FontWeight.Medium
                ),
                color = MaterialTheme.colorScheme.onSurface
            )
            Text(
                text = line.brgCode,
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
            // Qty is the recorded physical-unit quantity — no small-unit
            // normalization (P-09, ADR-RO-003).
            Text(
                text = formatQty(line.qty) + " " + line.satId + " \u2022 " + line.jenisRetur,
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
            Spacer(modifier = Modifier.height(8.dp))
            OutlinedButton(
                onClick = onRemove,
                enabled = enabled,
                modifier = Modifier.fillMaxWidth()
            ) {
                Text("Hapus Item")
            }
        }
    }
}

@Composable
private fun ReturnTypeChip(
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

private fun formatQty(qty: Double): String {
    return if (qty % 1.0 == 0.0) qty.toLong().toString() else qty.toString()
}
