package com.elsasa.bgud.ui.screen

import android.Manifest
import android.content.pm.PackageManager
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.background
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
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
import androidx.compose.ui.draw.clip
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.ExposedDropdownMenuBox
import androidx.compose.material3.ExposedDropdownMenuDefaults
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.MenuAnchorType
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.OutlinedTextFieldDefaults
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.core.content.ContextCompat
import com.elsasa.bgud.model.ReturnOrderItemEntity
import com.elsasa.bgud.ui.component.BarcodeScannerView
import com.elsasa.bgud.ui.component.BrandHeaderBar
import com.elsasa.bgud.ui.component.IndustrialCard
import com.elsasa.bgud.ui.component.StatusBadge
import com.elsasa.bgud.ui.theme.BrandCrimson
import com.elsasa.bgud.ui.theme.BrandGreen
import com.elsasa.bgud.ui.theme.BrandSage
import com.elsasa.bgud.ui.theme.BrandWarmCream
import com.elsasa.bgud.viewmodel.CreateReturnOrderViewModel
import com.elsasa.bgud.viewmodel.ReturnOrderCaptureLine

/**
 * Create Return Order screen (SCR-MOB-RO-002, Architecture §11.1, §12.2,
 * §14.1, §20, BGUD-RETURN-ORDER-NAV-001 TD-007).
 *
 * Modern Industrial layout prioritizing dominant item capture:
 * 1. Transaction Context (Customer [mandatory], Warehouse [read-only session bound])
 * 2. Item Entry / Working Area (Dominant: Camera Scanner, Local Search, Qty, Unit, Return Type, Item List)
 * 3. Additional Info (Salesman, Driver, Notes - secondary)
 * 4. Action Region (Save draft, Cancel)
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun CreateReturnOrderScreen(
    viewModel: CreateReturnOrderViewModel,
    onSaved: () -> Unit,
    onCancel: () -> Unit
) {
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
                title = "Buat Return Order",
                subtitle = "Penerimaan fisik barang retur pelanggan",
                onBack = onCancel
            )

            if (saved) {
                // Success State Card
                IndustrialCard(
                    borderColor = BrandGreen,
                    containerColor = BrandSage.copy(alpha = 0.2f)
                ) {
                    Column(
                        modifier = Modifier.padding(8.dp),
                        horizontalAlignment = Alignment.CenterHorizontally,
                        verticalArrangement = Arrangement.spacedBy(12.dp)
                    ) {
                        Surface(
                            shape = RoundedCornerShape(12.dp),
                            color = BrandGreen,
                            modifier = Modifier.size(48.dp)
                        ) {
                            Box(contentAlignment = Alignment.Center) {
                                Text(
                                    text = "✓",
                                    style = MaterialTheme.typography.titleLarge.copy(
                                        fontWeight = FontWeight.Bold
                                    ),
                                    color = Color.White
                                )
                            }
                        }
                        Text(
                            text = "Return Order Tersimpan sebagai Draft",
                            style = MaterialTheme.typography.titleMedium.copy(
                                fontWeight = FontWeight.Bold
                            ),
                            color = MaterialTheme.colorScheme.onSurface,
                            textAlign = TextAlign.Center
                        )
                        Text(
                            text = "Dokumen disimpan di penyimpanan lokal perangkat dan siap dikirim ke server pusat saat sinkronisasi.",
                            style = MaterialTheme.typography.bodySmall,
                            color = MaterialTheme.colorScheme.onSurfaceVariant,
                            textAlign = TextAlign.Center
                        )
                        Button(
                            onClick = onSaved,
                            modifier = Modifier
                                .fillMaxWidth()
                                .height(48.dp),
                            shape = RoundedCornerShape(12.dp),
                            colors = ButtonDefaults.buttonColors(containerColor = BrandGreen)
                        ) {
                            Text("Kembali ke Antrean", fontWeight = FontWeight.Bold)
                        }
                    }
                }
                return@Column
            }

            // ==========================================
            // 1. TRANSACTION CONTEXT (BR-001/002, TD-007)
            // ==========================================
            IndustrialCard(
                borderColor = if (customer == null) BrandCrimson.copy(alpha = 0.5f) else BrandSage,
                containerColor = MaterialTheme.colorScheme.surface
            ) {
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text(
                        text = "1. KONTEKS TRANSAKSI",
                        style = MaterialTheme.typography.labelSmall.copy(
                            fontWeight = FontWeight.Bold,
                            fontFamily = FontFamily.Monospace,
                            letterSpacing = 1.sp
                        ),
                        color = MaterialTheme.colorScheme.primary
                    )
                    if (customer == null) {
                        Surface(
                            shape = RoundedCornerShape(4.dp),
                            color = BrandCrimson.copy(alpha = 0.12f)
                        ) {
                            Text(
                                text = "Wajib Diisi",
                                modifier = Modifier.padding(horizontal = 6.dp, vertical = 2.dp),
                                style = MaterialTheme.typography.labelSmall.copy(
                                    fontWeight = FontWeight.Bold,
                                    fontSize = 10.sp
                                ),
                                color = BrandCrimson
                            )
                        }
                    }
                }

                Spacer(modifier = Modifier.height(10.dp))

                // Customer Selection
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
                        label = "Customer Terpilih",
                        title = customer!!.customerName.ifBlank { customer!!.customerCode },
                        subtitle = customer!!.customerCode,
                        onClear = viewModel::onClearCustomer,
                        enabled = !isSaving
                    )
                }

                Spacer(modifier = Modifier.height(10.dp))

                // Fixed Warehouse
                OutlinedTextField(
                    value = warehouseCode,
                    onValueChange = {},
                    enabled = false,
                    modifier = Modifier.fillMaxWidth(),
                    label = { Text("Warehouse (Terkunci Sesi)") },
                    shape = RoundedCornerShape(12.dp),
                    colors = OutlinedTextFieldDefaults.colors(
                        disabledBorderColor = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.6f),
                        disabledContainerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.25f),
                        disabledTextColor = MaterialTheme.colorScheme.onSurface
                    )
                )
            }

            // ==========================================
            // 2. DOMINANT WORKING AREA: ITEM ENTRY / LIST (TD-007)
            // ==========================================
            IndustrialCard(
                borderColor = BrandGreen.copy(alpha = 0.6f),
                containerColor = MaterialTheme.colorScheme.surface,
                elevation = 2.dp
            ) {
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Row(verticalAlignment = Alignment.CenterVertically) {
                        Box(
                            modifier = Modifier
                                .size(8.dp)
                                .clip(RoundedCornerShape(2.dp))
                                .background(BrandGreen)
                        )
                        Spacer(modifier = Modifier.width(8.dp))
                        Text(
                            text = "2. SCAN & INPUT ITEM RETUR",
                            style = MaterialTheme.typography.labelSmall.copy(
                                fontWeight = FontWeight.Bold,
                                fontFamily = FontFamily.Monospace,
                                letterSpacing = 1.sp
                            ),
                            color = BrandGreen
                        )
                    }
                    Surface(
                        shape = RoundedCornerShape(6.dp),
                        color = BrandSage.copy(alpha = 0.35f)
                    ) {
                        Text(
                            text = "Area Utama",
                            modifier = Modifier.padding(horizontal = 6.dp, vertical = 2.dp),
                            style = MaterialTheme.typography.labelSmall.copy(
                                fontWeight = FontWeight.Bold,
                                fontSize = 10.sp
                            ),
                            color = MaterialTheme.colorScheme.onSurface
                        )
                    }
                }

                Spacer(modifier = Modifier.height(12.dp))

                if (pendingItem == null) {
                    // Barcode Scanner View
                    if (hasCameraPermission) {
                        Surface(
                            modifier = Modifier
                                .fillMaxWidth()
                                .height(220.dp),
                            shape = RoundedCornerShape(14.dp),
                            border = BorderStroke(2.dp, BrandSage.copy(alpha = 0.5f))
                        ) {
                            BarcodeScannerView(
                                enabled = !isSaving,
                                onBarcode = viewModel::onBarcodeScanned,
                                modifier = Modifier.fillMaxSize()
                            )
                        }
                    } else {
                        IndustrialCard(
                            borderColor = MaterialTheme.colorScheme.outlineVariant,
                            containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.3f)
                        ) {
                            Text(
                                text = "Izin kamera diperlukan untuk memindai barcode.",
                                style = MaterialTheme.typography.bodySmall,
                                color = MaterialTheme.colorScheme.onSurfaceVariant
                            )
                            Spacer(modifier = Modifier.height(8.dp))
                            Button(
                                onClick = { permissionLauncher.launch(Manifest.permission.CAMERA) },
                                modifier = Modifier.fillMaxWidth(),
                                shape = RoundedCornerShape(10.dp),
                                colors = ButtonDefaults.buttonColors(containerColor = BrandGreen)
                            ) {
                                Text("Berikan Izin Kamera")
                            }
                        }
                    }

                    Spacer(modifier = Modifier.height(12.dp))

                    // Manual Item Search
                    ReferenceSearchSection(
                        label = "Cari Item Manual (kode / nama barang)",
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
                    // Item Selected Card
                    SelectedReferenceCard(
                        label = "Item Teridentifikasi",
                        title = pendingItem!!.brgName,
                        subtitle = pendingItem!!.brgCode,
                        onClear = viewModel::onClearPendingItem,
                        enabled = !isSaving
                    )

                    Spacer(modifier = Modifier.height(10.dp))

                    // Qty & Unit Input Row
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.spacedBy(10.dp)
                    ) {
                        OutlinedTextField(
                            value = pendingQty,
                            onValueChange = viewModel::onQtyChange,
                            modifier = Modifier.weight(1f),
                            label = { Text("Qty") },
                            singleLine = true,
                            enabled = !isSaving,
                            shape = RoundedCornerShape(12.dp),
                            keyboardOptions = KeyboardOptions(
                                keyboardType = KeyboardType.Decimal,
                                imeAction = ImeAction.Next
                            )
                        )

                        ExposedDropdownMenuBox(
                            expanded = unitExpanded,
                            onExpandedChange = { if (!isSaving) unitExpanded = !unitExpanded },
                            modifier = Modifier.weight(1f)
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
                                trailingIcon = { ExposedDropdownMenuDefaults.TrailingIcon(unitExpanded) },
                                shape = RoundedCornerShape(12.dp)
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
                    }

                    Spacer(modifier = Modifier.height(10.dp))

                    // Return Type Selection (BAGUS vs RUSAK)
                    Text(
                        text = "Kondisi Fisik Retur",
                        style = MaterialTheme.typography.labelSmall,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                    Spacer(modifier = Modifier.height(4.dp))
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        ModernReturnTypePill(
                            label = ReturnOrderItemEntity.JENIS_RETUR_BAGUS,
                            description = "Bagus (Sellable)",
                            selected = pendingJenisRetur == ReturnOrderItemEntity.JENIS_RETUR_BAGUS,
                            onClick = { viewModel.onJenisReturChange(ReturnOrderItemEntity.JENIS_RETUR_BAGUS) },
                            modifier = Modifier.weight(1f),
                            activeColor = BrandGreen
                        )
                        ModernReturnTypePill(
                            label = ReturnOrderItemEntity.JENIS_RETUR_RUSAK,
                            description = "Rusak (Damaged)",
                            selected = pendingJenisRetur == ReturnOrderItemEntity.JENIS_RETUR_RUSAK,
                            onClick = { viewModel.onJenisReturChange(ReturnOrderItemEntity.JENIS_RETUR_RUSAK) },
                            modifier = Modifier.weight(1f),
                            activeColor = BrandCrimson
                        )
                    }

                    Spacer(modifier = Modifier.height(12.dp))

                    Button(
                        onClick = viewModel::addItem,
                        enabled = !isSaving,
                        modifier = Modifier
                            .fillMaxWidth()
                            .height(48.dp),
                        shape = RoundedCornerShape(12.dp),
                        colors = ButtonDefaults.buttonColors(containerColor = BrandGreen)
                    ) {
                        Text("+ Tambah Item ke Daftar Retur", fontWeight = FontWeight.Bold)
                    }
                }

                if (pendingError != null) {
                    Spacer(modifier = Modifier.height(8.dp))
                    Text(
                        text = pendingError!!,
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.error,
                        modifier = Modifier.fillMaxWidth()
                    )
                }

                // Added Items List
                if (items.isNotEmpty()) {
                    Spacer(modifier = Modifier.height(14.dp))
                    HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
                    Spacer(modifier = Modifier.height(10.dp))

                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.SpaceBetween,
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Text(
                            text = "Item Telah Ditambahkan",
                            style = MaterialTheme.typography.labelSmall.copy(
                                fontWeight = FontWeight.Bold,
                                fontFamily = FontFamily.Monospace
                            ),
                            color = MaterialTheme.colorScheme.onSurface
                        )
                        Surface(
                            shape = RoundedCornerShape(4.dp),
                            color = BrandSage.copy(alpha = 0.4f)
                        ) {
                            Text(
                                text = "${items.size} Item",
                                modifier = Modifier.padding(horizontal = 6.dp, vertical = 2.dp),
                                style = MaterialTheme.typography.labelSmall.copy(
                                    fontWeight = FontWeight.Bold
                                ),
                                color = MaterialTheme.colorScheme.onSurface
                            )
                        }
                    }

                    Spacer(modifier = Modifier.height(8.dp))

                    items.forEachIndexed { index, line ->
                        ModernCaptureLineCard(
                            line = line,
                            onRemove = { viewModel.removeItem(index) },
                            enabled = !isSaving
                        )
                        Spacer(modifier = Modifier.height(6.dp))
                    }
                }
            }

            // ==========================================
            // 3. ADDITIONAL INFO (Secondary Region)
            // ==========================================
            IndustrialCard(
                borderColor = MaterialTheme.colorScheme.outlineVariant,
                containerColor = MaterialTheme.colorScheme.surface
            ) {
                Text(
                    text = "3. DATA PENDUKUNG (OPSIONAL)",
                    style = MaterialTheme.typography.labelSmall.copy(
                        fontWeight = FontWeight.Bold,
                        fontFamily = FontFamily.Monospace,
                        letterSpacing = 1.sp
                    ),
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )

                Spacer(modifier = Modifier.height(10.dp))

                // Salesman picker
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

                Spacer(modifier = Modifier.height(10.dp))

                // Driver picker
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

                Spacer(modifier = Modifier.height(10.dp))

                // Notes
                OutlinedTextField(
                    value = note,
                    onValueChange = viewModel::onNoteChange,
                    modifier = Modifier.fillMaxWidth(),
                    label = { Text("Catatan Penerimaan") },
                    shape = RoundedCornerShape(12.dp),
                    enabled = !isSaving
                )
            }

            if (saveError != null) {
                Surface(
                    modifier = Modifier.fillMaxWidth(),
                    shape = RoundedCornerShape(10.dp),
                    color = MaterialTheme.colorScheme.errorContainer.copy(alpha = 0.5f)
                ) {
                    Text(
                        text = saveError!!,
                        color = MaterialTheme.colorScheme.error,
                        style = MaterialTheme.typography.bodySmall.copy(
                            fontWeight = FontWeight.Medium
                        ),
                        textAlign = TextAlign.Center,
                        modifier = Modifier.padding(12.dp)
                    )
                }
            }

            // ==========================================
            // 4. ACTION REGION (TD-007)
            // ==========================================
            Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                Button(
                    onClick = viewModel::save,
                    enabled = viewModel.canSave(),
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
                    if (isSaving) {
                        CircularProgressIndicator(
                            modifier = Modifier.size(20.dp),
                            color = Color.White,
                            strokeWidth = 2.dp
                        )
                        Spacer(modifier = Modifier.width(8.dp))
                        Text("Menyimpan draft...", fontWeight = FontWeight.Bold)
                    } else {
                        Text("Simpan Sebagai Draft (Lokal)", fontWeight = FontWeight.Bold)
                    }
                }

                OutlinedButton(
                    onClick = onCancel,
                    enabled = !isSaving,
                    modifier = Modifier
                        .fillMaxWidth()
                        .height(50.dp),
                    shape = RoundedCornerShape(14.dp),
                    colors = ButtonDefaults.outlinedButtonColors(
                        containerColor = MaterialTheme.colorScheme.surface
                    ),
                    border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant)
                ) {
                    Text("Batalkan & Kembali", fontWeight = FontWeight.SemiBold)
                }
            }

            Spacer(modifier = Modifier.height(16.dp))
        }
    }
}

/**
 * Filter Pill for Jenis Retur (BAGUS vs RUSAK).
 */
@Composable
private fun ModernReturnTypePill(
    label: String,
    description: String,
    selected: Boolean,
    onClick: () -> Unit,
    modifier: Modifier = Modifier,
    activeColor: Color
) {
    val bgColor = if (selected) activeColor.copy(alpha = 0.15f) else MaterialTheme.colorScheme.surface
    val borderColor = if (selected) activeColor else MaterialTheme.colorScheme.outlineVariant
    val textColor = if (selected) activeColor else MaterialTheme.colorScheme.onSurface

    Surface(
        modifier = modifier.clickable(onClick = onClick),
        shape = RoundedCornerShape(12.dp),
        color = bgColor,
        border = BorderStroke(if (selected) 2.dp else 1.dp, borderColor)
    ) {
        Column(
            modifier = Modifier.padding(vertical = 10.dp, horizontal = 12.dp),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Text(
                text = label,
                style = MaterialTheme.typography.labelLarge.copy(
                    fontWeight = FontWeight.Bold
                ),
                color = textColor
            )
            Text(
                text = description,
                style = MaterialTheme.typography.labelSmall.copy(
                    fontSize = 10.sp
                ),
                color = textColor.copy(alpha = 0.8f)
            )
        }
    }
}

/**
 * Local-cache reference search section with dropdown results.
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
        shape = RoundedCornerShape(12.dp),
        keyboardOptions = KeyboardOptions(imeAction = ImeAction.Search)
    )

    when {
        isSearching -> {
            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(vertical = 8.dp),
                horizontalArrangement = Arrangement.Center
            ) {
                CircularProgressIndicator(
                    modifier = Modifier.size(20.dp),
                    strokeWidth = 2.dp,
                    color = BrandGreen
                )
            }
        }
        query.trim().isNotEmpty() && results.isEmpty() -> {
            Text(
                text = "Tidak ditemukan pada cache lokal perangkat.",
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                textAlign = TextAlign.Center,
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(vertical = 8.dp)
            )
        }
        results.isNotEmpty() -> {
            Surface(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(top = 4.dp),
                shape = RoundedCornerShape(12.dp),
                color = MaterialTheme.colorScheme.surface,
                border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant)
            ) {
                Column {
                    results.take(6).forEachIndexed { idx, result ->
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .clickable(enabled = enabled) { onSelect(result) }
                                .padding(12.dp),
                            horizontalArrangement = Arrangement.SpaceBetween,
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Column(modifier = Modifier.weight(1f)) {
                                Text(
                                    text = rowHeadline(result),
                                    style = MaterialTheme.typography.bodyMedium.copy(
                                        fontWeight = FontWeight.SemiBold
                                    ),
                                    maxLines = 1,
                                    overflow = TextOverflow.Ellipsis
                                )
                                Text(
                                    text = rowSupporting(result),
                                    style = MaterialTheme.typography.labelSmall.copy(
                                        fontFamily = FontFamily.Monospace
                                    ),
                                    color = MaterialTheme.colorScheme.onSurfaceVariant
                                )
                            }
                            Text(
                                text = "Pilih",
                                style = MaterialTheme.typography.labelSmall.copy(
                                    fontWeight = FontWeight.Bold
                                ),
                                color = BrandGreen
                            )
                        }
                        if (idx < results.take(6).lastIndex) {
                            HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
                        }
                    }
                }
            }
        }
    }
}

/** Selected Reference card with clean remove button. */
@Composable
private fun SelectedReferenceCard(
    label: String,
    title: String,
    subtitle: String,
    onClear: () -> Unit,
    enabled: Boolean
) {
    Surface(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(12.dp),
        color = BrandSage.copy(alpha = 0.2f),
        border = BorderStroke(1.dp, BrandSage)
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(12.dp),
            horizontalArrangement = Arrangement.SpaceBetween,
            verticalAlignment = Alignment.CenterVertically
        ) {
            Column(modifier = Modifier.weight(1f)) {
                Text(
                    text = label,
                    style = MaterialTheme.typography.labelSmall.copy(
                        fontWeight = FontWeight.Bold,
                        fontSize = 10.sp
                    ),
                    color = MaterialTheme.colorScheme.primary
                )
                Text(
                    text = title.ifBlank { subtitle },
                    style = MaterialTheme.typography.bodyMedium.copy(
                        fontWeight = FontWeight.Bold
                    ),
                    color = MaterialTheme.colorScheme.onSurface,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis
                )
                if (subtitle.isNotBlank() && title.isNotBlank()) {
                    Text(
                        text = subtitle,
                        style = MaterialTheme.typography.labelSmall.copy(
                            fontFamily = FontFamily.Monospace
                        ),
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
            }

            OutlinedButton(
                onClick = onClear,
                enabled = enabled,
                shape = RoundedCornerShape(8.dp),
                border = BorderStroke(1.dp, BrandCrimson.copy(alpha = 0.5f)),
                colors = ButtonDefaults.outlinedButtonColors(
                    contentColor = BrandCrimson
                )
            ) {
                Text(
                    text = "Ganti",
                    style = MaterialTheme.typography.labelSmall.copy(
                        fontWeight = FontWeight.Bold
                    )
                )
            }
        }
    }
}

/** Added item line card with remove button. */
@Composable
private fun ModernCaptureLineCard(
    line: ReturnOrderCaptureLine,
    onRemove: () -> Unit,
    enabled: Boolean
) {
    Surface(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(12.dp),
        color = MaterialTheme.colorScheme.surface,
        border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant)
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(12.dp),
            horizontalArrangement = Arrangement.SpaceBetween,
            verticalAlignment = Alignment.CenterVertically
        ) {
            Column(modifier = Modifier.weight(1f)) {
                Text(
                    text = line.brgName.ifBlank { line.brgCode },
                    style = MaterialTheme.typography.bodyMedium.copy(
                        fontWeight = FontWeight.SemiBold
                    ),
                    color = MaterialTheme.colorScheme.onSurface,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis
                )
                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    modifier = Modifier.padding(top = 2.dp)
                ) {
                    Text(
                        text = line.brgCode,
                        style = MaterialTheme.typography.labelSmall.copy(
                            fontFamily = FontFamily.Monospace,
                            fontSize = 10.sp
                        ),
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                    Text(text = " • ", style = MaterialTheme.typography.labelSmall)
                    Text(
                        text = "${formatQty(line.qty)} ${line.satId}",
                        style = MaterialTheme.typography.labelSmall.copy(
                            fontWeight = FontWeight.Bold
                        ),
                        color = MaterialTheme.colorScheme.onSurface
                    )
                    Spacer(modifier = Modifier.width(6.dp))
                    StatusBadge(status = line.jenisRetur)
                }
            }

            Surface(
                modifier = Modifier
                    .clickable(enabled = enabled, onClick = onRemove)
                    .padding(4.dp),
                shape = RoundedCornerShape(6.dp),
                color = BrandCrimson.copy(alpha = 0.1f)
            ) {
                Text(
                    text = "Hapus",
                    modifier = Modifier.padding(horizontal = 8.dp, vertical = 4.dp),
                    style = MaterialTheme.typography.labelSmall.copy(
                        fontWeight = FontWeight.Bold
                    ),
                    color = BrandCrimson
                )
            }
        }
    }
}

private fun formatQty(qty: Double): String {
    return if (qty % 1.0 == 0.0) qty.toLong().toString() else qty.toString()
}
