package com.elsasa.bgud.ui.screen

import androidx.compose.foundation.BorderStroke
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
import androidx.compose.foundation.text.KeyboardActions
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
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
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.elsasa.bgud.model.BarangEntity
import com.elsasa.bgud.ui.component.BrandHeaderBar
import com.elsasa.bgud.ui.component.IndustrialCard
import com.elsasa.bgud.ui.theme.BrandCrimson
import com.elsasa.bgud.ui.theme.BrandGreen
import com.elsasa.bgud.ui.theme.BrandSage
import com.elsasa.bgud.ui.theme.BrandWarmCream
import com.elsasa.bgud.viewmodel.EditBarcodeViewModel

/**
 * Edit Barcode screen (SCR-MOB-006, Architecture §12.8, UX Blueprint §10).
 *
 * Modern Industrial layout:
 * - Barcode value card (read-only monospace badge)
 * - Current mapping summary card
 * - Corrected Item search field + results list
 * - Selected item summary card with clear action
 * - Unit selector dropdown
 * - Save correction & Cancel buttons
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

    val canSave = !isLoading && !notFound && !saved && !isSaving &&
        isDirty &&
        selectedItem != null && selectedItem!!.isAktif

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
                title = "Edit Barcode",
                subtitle = "Koreksi pemetaan barcode fisik ke item master",
                onBack = onBack
            )

            when {
                isLoading -> {
                    Box(
                        modifier = Modifier
                            .fillMaxWidth()
                            .height(140.dp),
                        contentAlignment = Alignment.Center
                    ) {
                        CircularProgressIndicator(color = BrandGreen)
                    }
                    return@Column
                }
                notFound || original == null -> {
                    IndustrialCard(borderColor = MaterialTheme.colorScheme.outlineVariant) {
                        Text(
                            text = "Data pemetaan barcode tidak ditemukan.",
                            style = MaterialTheme.typography.bodyMedium,
                            color = MaterialTheme.colorScheme.onSurfaceVariant,
                            textAlign = TextAlign.Center,
                            modifier = Modifier.fillMaxWidth()
                        )
                        Spacer(modifier = Modifier.height(12.dp))
                        OutlinedButton(
                            onClick = onBack,
                            modifier = Modifier.fillMaxWidth()
                        ) {
                            Text("Kembali")
                        }
                    }
                    return@Column
                }
            }

            if (saved) {
                IndustrialCard(borderColor = BrandGreen, containerColor = BrandSage.copy(alpha = 0.2f)) {
                    Column(
                        modifier = Modifier.padding(8.dp),
                        horizontalAlignment = Alignment.CenterHorizontally,
                        verticalArrangement = Arrangement.spacedBy(10.dp)
                    ) {
                        Text(
                            text = "Koreksi Tersimpan di Antrean Lokal",
                            style = MaterialTheme.typography.titleMedium.copy(fontWeight = FontWeight.Bold),
                            color = MaterialTheme.colorScheme.onSurface
                        )
                        Text(
                            text = "Koreksi pemetaan barcode akan dikirim ke server pusat saat sinkronisasi.",
                            style = MaterialTheme.typography.bodySmall,
                            color = MaterialTheme.colorScheme.onSurfaceVariant,
                            textAlign = TextAlign.Center
                        )
                        Button(
                            onClick = onBack,
                            modifier = Modifier.fillMaxWidth(),
                            colors = ButtonDefaults.buttonColors(containerColor = BrandGreen)
                        ) {
                            Text("Kembali")
                        }
                    }
                }
                return@Column
            }

            // Barcode Read-Only Card
            IndustrialCard(borderColor = BrandSage, containerColor = MaterialTheme.colorScheme.surface) {
                Text(
                    text = "BARCODE FISIK (TETAP)",
                    style = MaterialTheme.typography.labelSmall.copy(
                        fontWeight = FontWeight.Bold,
                        fontFamily = FontFamily.Monospace,
                        letterSpacing = 1.sp
                    ),
                    color = MaterialTheme.colorScheme.primary
                )
                Spacer(modifier = Modifier.height(6.dp))
                Surface(
                    modifier = Modifier.fillMaxWidth(),
                    shape = RoundedCornerShape(10.dp),
                    color = BrandWarmCream.copy(alpha = 0.4f),
                    border = BorderStroke(1.dp, BrandSage)
                ) {
                    Text(
                        text = barcodeValue,
                        modifier = Modifier.padding(horizontal = 14.dp, vertical = 12.dp),
                        style = MaterialTheme.typography.bodyLarge.copy(
                            fontWeight = FontWeight.Bold,
                            fontFamily = FontFamily.Monospace,
                            letterSpacing = 1.sp
                        ),
                        color = MaterialTheme.colorScheme.onSurface
                    )
                }
            }

            // Current Loaded Mapping Card
            val loaded = original!!
            IndustrialCard(borderColor = MaterialTheme.colorScheme.outlineVariant) {
                Text(
                    text = "PEMETAAN SAAT INI",
                    style = MaterialTheme.typography.labelSmall.copy(
                        fontWeight = FontWeight.Bold,
                        fontFamily = FontFamily.Monospace,
                        letterSpacing = 1.sp
                    ),
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
                Spacer(modifier = Modifier.height(6.dp))
                Text(
                    text = loaded.brgName,
                    style = MaterialTheme.typography.bodyMedium.copy(fontWeight = FontWeight.Bold),
                    color = MaterialTheme.colorScheme.onSurface
                )
                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    modifier = Modifier.padding(top = 2.dp)
                ) {
                    Text(
                        text = loaded.brgCode,
                        style = MaterialTheme.typography.labelSmall.copy(fontFamily = FontFamily.Monospace),
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                    if (loaded.satuan.isNotBlank()) {
                        Text(text = " • ", style = MaterialTheme.typography.labelSmall)
                        Surface(
                            shape = RoundedCornerShape(4.dp),
                            color = BrandSage.copy(alpha = 0.4f)
                        ) {
                            Text(
                                text = loaded.satuan,
                                modifier = Modifier.padding(horizontal = 5.dp, vertical = 1.dp),
                                style = MaterialTheme.typography.labelSmall.copy(fontWeight = FontWeight.Bold, fontSize = 10.sp),
                                color = MaterialTheme.colorScheme.onSurface
                            )
                        }
                    }
                }
            }

            // Corrected Item Selection
            IndustrialCard(borderColor = BrandGreen.copy(alpha = 0.5f)) {
                Text(
                    text = "KOREKSI PEMETAAN BARU",
                    style = MaterialTheme.typography.labelSmall.copy(
                        fontWeight = FontWeight.Bold,
                        fontFamily = FontFamily.Monospace,
                        letterSpacing = 1.sp
                    ),
                    color = BrandGreen
                )

                Spacer(modifier = Modifier.height(10.dp))

                if (selectedItem == null) {
                    OutlinedTextField(
                        value = itemQuery,
                        onValueChange = viewModel::onQueryChange,
                        modifier = Modifier.fillMaxWidth(),
                        label = { Text("Cari Item Baru (kode / nama barang)") },
                        singleLine = true,
                        enabled = !isSaving,
                        shape = RoundedCornerShape(12.dp),
                        keyboardOptions = KeyboardOptions(imeAction = ImeAction.Search),
                        keyboardActions = KeyboardActions(
                            onSearch = { viewModel.onQueryChange(itemQuery) }
                        )
                    )

                    when {
                        isSearching -> {
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(vertical = 12.dp),
                                horizontalArrangement = Arrangement.Center
                            ) {
                                CircularProgressIndicator(modifier = Modifier.size(20.dp), strokeWidth = 2.dp, color = BrandGreen)
                            }
                        }
                        itemQuery.trim().isNotEmpty() && itemResults.isEmpty() -> {
                            Text(
                                text = "Item tidak ditemukan pada penyimpanan lokal.",
                                style = MaterialTheme.typography.bodySmall,
                                color = MaterialTheme.colorScheme.onSurfaceVariant,
                                textAlign = TextAlign.Center,
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(vertical = 8.dp)
                            )
                        }
                        itemResults.isNotEmpty() -> {
                            Surface(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(top = 4.dp),
                                shape = RoundedCornerShape(12.dp),
                                color = MaterialTheme.colorScheme.surface,
                                border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant)
                            ) {
                                Column {
                                    itemResults.take(6).forEachIndexed { idx, item ->
                                        Row(
                                            modifier = Modifier
                                                .fillMaxWidth()
                                                .clickable(onClick = { viewModel.onSelectItem(item) })
                                                .padding(12.dp),
                                            horizontalArrangement = Arrangement.SpaceBetween,
                                            verticalAlignment = Alignment.CenterVertically
                                        ) {
                                            Column(modifier = Modifier.weight(1f)) {
                                                Text(
                                                    text = item.brgName,
                                                    style = MaterialTheme.typography.bodyMedium.copy(fontWeight = FontWeight.SemiBold),
                                                    maxLines = 1,
                                                    overflow = TextOverflow.Ellipsis
                                                )
                                                Text(
                                                    text = item.brgCode + if (!item.isAktif) " (nonaktif)" else "",
                                                    style = MaterialTheme.typography.labelSmall.copy(fontFamily = FontFamily.Monospace),
                                                    color = if (!item.isAktif) MaterialTheme.colorScheme.error else MaterialTheme.colorScheme.onSurfaceVariant
                                                )
                                            }
                                            Text(
                                                text = "Pilih",
                                                style = MaterialTheme.typography.labelSmall.copy(fontWeight = FontWeight.Bold),
                                                color = BrandGreen
                                            )
                                        }
                                        if (idx < itemResults.take(6).lastIndex) {
                                            HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f))
                                        }
                                    }
                                }
                            }
                        }
                    }
                } else {
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
                                    text = "Item Baru Terpilih",
                                    style = MaterialTheme.typography.labelSmall.copy(fontWeight = FontWeight.Bold, fontSize = 10.sp),
                                    color = MaterialTheme.colorScheme.primary
                                )
                                Text(
                                    text = selectedItem!!.brgName,
                                    style = MaterialTheme.typography.bodyMedium.copy(fontWeight = FontWeight.Bold),
                                    maxLines = 1,
                                    overflow = TextOverflow.Ellipsis
                                )
                                Text(
                                    text = selectedItem!!.brgCode,
                                    style = MaterialTheme.typography.labelSmall.copy(fontFamily = FontFamily.Monospace),
                                    color = MaterialTheme.colorScheme.onSurfaceVariant
                                )
                            }
                            OutlinedButton(
                                onClick = viewModel::onClearSelection,
                                enabled = !isSaving,
                                shape = RoundedCornerShape(8.dp),
                                border = BorderStroke(1.dp, BrandCrimson.copy(alpha = 0.5f)),
                                colors = ButtonDefaults.outlinedButtonColors(contentColor = BrandCrimson)
                            ) {
                                Text("Ganti", style = MaterialTheme.typography.labelSmall.copy(fontWeight = FontWeight.Bold))
                            }
                        }
                    }
                }

                if (selectedItem != null && !selectedItem!!.isAktif) {
                    Spacer(modifier = Modifier.height(8.dp))
                    Text(
                        text = "Item sudah tidak aktif di master data.",
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.error
                    )
                }

                if (selectedItem != null && selectedItem!!.isAktif) {
                    Spacer(modifier = Modifier.height(12.dp))
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
                            label = { Text("Satuan Kemasan (Opsional)") },
                            trailingIcon = { ExposedDropdownMenuDefaults.TrailingIcon(unitExpanded) },
                            shape = RoundedCornerShape(12.dp)
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
                        style = MaterialTheme.typography.bodySmall.copy(fontWeight = FontWeight.Medium),
                        textAlign = TextAlign.Center,
                        modifier = Modifier.padding(12.dp)
                    )
                }
            }

            // Action Region
            Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                Button(
                    onClick = viewModel::save,
                    enabled = canSave,
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
                        CircularProgressIndicator(modifier = Modifier.size(20.dp), color = Color.White, strokeWidth = 2.dp)
                        Spacer(modifier = Modifier.width(8.dp))
                        Text("Menyimpan...", fontWeight = FontWeight.Bold)
                    } else {
                        Text("Simpan Koreksi Barcode", fontWeight = FontWeight.Bold)
                    }
                }

                OutlinedButton(
                    onClick = onBack,
                    enabled = !isSaving,
                    modifier = Modifier
                        .fillMaxWidth()
                        .height(50.dp),
                    shape = RoundedCornerShape(14.dp),
                    colors = ButtonDefaults.outlinedButtonColors(containerColor = MaterialTheme.colorScheme.surface),
                    border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant)
                ) {
                    Text("Batalkan & Kembali", fontWeight = FontWeight.SemiBold)
                }
            }

            Spacer(modifier = Modifier.height(12.dp))
        }
    }
}
