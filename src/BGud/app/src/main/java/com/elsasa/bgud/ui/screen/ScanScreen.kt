package com.elsasa.bgud.ui.screen

import android.Manifest
import android.content.pm.PackageManager
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.BorderStroke
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
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.MaterialTheme
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
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.core.content.ContextCompat
import com.elsasa.bgud.model.BarcodeEntity
import com.elsasa.bgud.ui.component.BarcodeScannerView
import com.elsasa.bgud.ui.component.BrandHeaderBar
import com.elsasa.bgud.ui.component.IndustrialCard
import com.elsasa.bgud.ui.theme.BrandCrimson
import com.elsasa.bgud.ui.theme.BrandGreen
import com.elsasa.bgud.ui.theme.BrandSage
import com.elsasa.bgud.viewmodel.ScanState
import com.elsasa.bgud.viewmodel.ScanViewModel

/**
 * Scan Barcode screen (SCR-MOB-003, Architecture §12.5, UX Blueprint §7).
 *
 * Modern Industrial layout:
 * - Camera viewfinder with brand-tinted frame
 * - Manual barcode entry fallback
 * - Scenario A: Barcode Found result card
 * - Scenario B: Barcode Not Found card with register action
 */
@Composable
fun ScanScreen(
    viewModel: ScanViewModel,
    onClose: () -> Unit,
    onRegisterBarcode: (String) -> Unit
) {
    val scanState by viewModel.scanState.collectAsState()
    val lastScannedValue by viewModel.lastScannedValue.collectAsState()
    val resolvedItem by viewModel.resolvedItem.collectAsState()

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

    var manualEntry by remember { mutableStateOf("") }

    fun submitManual() {
        if (manualEntry.isNotBlank()) {
            viewModel.onManualSubmit(manualEntry)
        }
    }

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
                title = "Pindai Barcode",
                subtitle = "Identifikasi produk melalui kamera atau input kode",
                onBack = onClose
            )

            // Camera Viewfinder
            if (hasCameraPermission) {
                Surface(
                    modifier = Modifier
                        .fillMaxWidth()
                        .height(260.dp),
                    shape = RoundedCornerShape(16.dp),
                    border = BorderStroke(2.dp, BrandSage)
                ) {
                    BarcodeScannerView(
                        enabled = scanState == ScanState.Scanning,
                        onBarcode = viewModel::onBarcodeScanned,
                        modifier = Modifier.fillMaxSize()
                    )
                }
            } else {
                IndustrialCard(borderColor = MaterialTheme.colorScheme.outlineVariant) {
                    Text(
                        text = "Izin kamera belum diberikan.",
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.onSurface
                    )
                    Spacer(modifier = Modifier.height(8.dp))
                    Button(
                        onClick = { permissionLauncher.launch(Manifest.permission.CAMERA) },
                        modifier = Modifier.fillMaxWidth(),
                        colors = ButtonDefaults.buttonColors(containerColor = BrandGreen)
                    ) {
                        Text("Berikan Izin Kamera")
                    }
                }
            }

            // Manual Entry Card
            IndustrialCard(borderColor = MaterialTheme.colorScheme.outlineVariant) {
                Text(
                    text = "INPUT MANUAL (OPSIONAL)",
                    style = MaterialTheme.typography.labelSmall.copy(
                        fontWeight = FontWeight.Bold,
                        fontFamily = FontFamily.Monospace,
                        letterSpacing = 1.sp
                    ),
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
                Spacer(modifier = Modifier.height(8.dp))
                OutlinedTextField(
                    value = manualEntry,
                    onValueChange = { manualEntry = it },
                    label = { Text("Nomor Barcode Fisik") },
                    singleLine = true,
                    shape = RoundedCornerShape(12.dp),
                    keyboardOptions = KeyboardOptions(imeAction = ImeAction.Search),
                    keyboardActions = KeyboardActions(onSearch = { submitManual() }),
                    modifier = Modifier.fillMaxWidth()
                )
                Spacer(modifier = Modifier.height(8.dp))
                Button(
                    onClick = { submitManual() },
                    enabled = scanState != ScanState.Resolving && manualEntry.isNotBlank(),
                    modifier = Modifier.fillMaxWidth(),
                    shape = RoundedCornerShape(12.dp),
                    colors = ButtonDefaults.buttonColors(containerColor = BrandGreen)
                ) {
                    Text("Cari Barcode", fontWeight = FontWeight.Bold)
                }
            }

            when (scanState) {
                ScanState.Scanning -> Unit
                ScanState.Resolving -> {
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.Center
                    ) {
                        CircularProgressIndicator(color = BrandGreen)
                    }
                }
                ScanState.Found -> {
                    val item = resolvedItem
                    if (item != null) {
                        FoundResultCardModern(
                            scannedValue = lastScannedValue,
                            item = item,
                            onScanAgain = viewModel::resetToScanning,
                            onClose = onClose
                        )
                    }
                }
                ScanState.NotFound -> {
                    IndustrialCard(
                        borderColor = BrandCrimson.copy(alpha = 0.6f),
                        containerColor = BrandCrimson.copy(alpha = 0.05f)
                    ) {
                        Text(
                            text = "Barcode Tidak Ditemukan",
                            style = MaterialTheme.typography.titleMedium.copy(fontWeight = FontWeight.Bold),
                            color = BrandCrimson
                        )
                        Spacer(modifier = Modifier.height(4.dp))
                        Text(
                            text = "Kode: $lastScannedValue",
                            style = MaterialTheme.typography.bodySmall.copy(fontFamily = FontFamily.Monospace),
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                        Spacer(modifier = Modifier.height(12.dp))
                        Button(
                            onClick = { onRegisterBarcode(lastScannedValue) },
                            modifier = Modifier.fillMaxWidth(),
                            shape = RoundedCornerShape(12.dp),
                            colors = ButtonDefaults.buttonColors(containerColor = BrandCrimson)
                        ) {
                            Text("Daftarkan Barcode Baru", fontWeight = FontWeight.Bold)
                        }
                        Spacer(modifier = Modifier.height(6.dp))
                        OutlinedButton(
                            onClick = viewModel::resetToScanning,
                            modifier = Modifier.fillMaxWidth(),
                            shape = RoundedCornerShape(12.dp)
                        ) {
                            Text("Pindai Ulang")
                        }
                    }
                }
            }

            Spacer(modifier = Modifier.height(4.dp))
            OutlinedButton(
                onClick = onClose,
                modifier = Modifier
                    .fillMaxWidth()
                    .height(50.dp),
                shape = RoundedCornerShape(14.dp),
                colors = ButtonDefaults.outlinedButtonColors(
                    containerColor = MaterialTheme.colorScheme.surface
                ),
                border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant)
            ) {
                Text("Tutup", fontWeight = FontWeight.SemiBold)
            }

            Spacer(modifier = Modifier.height(12.dp))
        }
    }
}

@Composable
private fun FoundResultCardModern(
    scannedValue: String,
    item: BarcodeEntity,
    onScanAgain: () -> Unit,
    onClose: () -> Unit
) {
    IndustrialCard(
        borderColor = BrandGreen,
        containerColor = BrandSage.copy(alpha = 0.15f)
    ) {
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.SpaceBetween,
            verticalAlignment = Alignment.CenterVertically
        ) {
            Text(
                text = "Barcode Terdaftar",
                style = MaterialTheme.typography.titleSmall.copy(fontWeight = FontWeight.Bold),
                color = BrandGreen
            )
            Surface(
                shape = RoundedCornerShape(4.dp),
                color = BrandGreen.copy(alpha = 0.2f)
            ) {
                Text(
                    text = "DITEMUKAN",
                    modifier = Modifier.padding(horizontal = 6.dp, vertical = 2.dp),
                    style = MaterialTheme.typography.labelSmall.copy(
                        fontWeight = FontWeight.Bold,
                        fontFamily = FontFamily.Monospace,
                        fontSize = 10.sp
                    ),
                    color = BrandGreen
                )
            }
        }

        Spacer(modifier = Modifier.height(10.dp))
        Text(
            text = item.brgName,
            style = MaterialTheme.typography.bodyLarge.copy(fontWeight = FontWeight.Bold),
            color = MaterialTheme.colorScheme.onSurface
        )

        Row(
            verticalAlignment = Alignment.CenterVertically,
            modifier = Modifier.padding(top = 2.dp)
        ) {
            Text(
                text = item.brgCode,
                style = MaterialTheme.typography.labelSmall.copy(fontFamily = FontFamily.Monospace),
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
            if (item.satuan.isNotBlank()) {
                Text(text = " • ", style = MaterialTheme.typography.labelSmall)
                Surface(
                    shape = RoundedCornerShape(4.dp),
                    color = BrandSage.copy(alpha = 0.4f)
                ) {
                    Text(
                        text = item.satuan,
                        modifier = Modifier.padding(horizontal = 5.dp, vertical = 1.dp),
                        style = MaterialTheme.typography.labelSmall.copy(fontWeight = FontWeight.Bold, fontSize = 10.sp),
                        color = MaterialTheme.colorScheme.onSurface
                    )
                }
            }
        }

        Spacer(modifier = Modifier.height(10.dp))
        HorizontalDivider(color = BrandSage.copy(alpha = 0.5f))
        Spacer(modifier = Modifier.height(10.dp))

        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            Button(
                onClick = onScanAgain,
                modifier = Modifier.weight(1f),
                shape = RoundedCornerShape(10.dp),
                colors = ButtonDefaults.buttonColors(containerColor = BrandGreen)
            ) {
                Text("Scan Lagi", fontWeight = FontWeight.Bold)
            }
            OutlinedButton(
                onClick = onClose,
                modifier = Modifier.weight(1f),
                shape = RoundedCornerShape(10.dp)
            ) {
                Text("Selesai")
            }
        }
    }
}
