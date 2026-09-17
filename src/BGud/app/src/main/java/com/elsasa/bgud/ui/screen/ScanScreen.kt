package com.elsasa.bgud.ui.screen

import android.Manifest
import android.content.pm.PackageManager
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
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
import androidx.compose.material3.MaterialTheme
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
import androidx.compose.ui.unit.dp
import androidx.core.content.ContextCompat
import com.elsasa.bgud.model.BarcodeEntity
import com.elsasa.bgud.ui.component.BarcodeScannerView
import com.elsasa.bgud.viewmodel.ScanState
import com.elsasa.bgud.viewmodel.ScanViewModel

/**
 * Scan Barcode screen (SCR-MOB-003, Architecture §12.5, UX Blueprint §7).
 *
 * ```text
 * Camera Viewfinder   (CameraX + ML Kit region)
 * Manual Entry        (fallback text input + Search)
 * Result Region       (Barcode, Item Code, Item Name, Unit) — Scenario A
 * Not Found Region    ("Barcode Not Found" + Register Barcode / Cancel) — Scenario B
 * ```
 *
 * Scan-first (UX-002): the camera is the primary input; manual typing is a
 * fallback only, resolved through the same [ScanViewModel] path (§20).
 * Offline behaviour is identical to online behaviour — resolution is a
 * local Room lookup with no network call (P-07, ADR-006; §14.2).
 *
 * Navigation (§13.2): Found shows the result region with Close (back) and
 * Scan Again (re-arm); Not Found offers Register Barcode (navigates to
 * `register?barcode={value}`, owned by S5.7) or Cancel (re-arm to Scanning).
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
                text = "Scan Barcode",
                style = MaterialTheme.typography.headlineSmall.copy(
                    fontWeight = FontWeight.Bold
                ),
                color = MaterialTheme.colorScheme.onSurface
            )

            // Camera Viewfinder (CameraX + ML Kit region, §12.5).
            if (hasCameraPermission) {
                BarcodeScannerView(
                    // Single-scan mode: analyze only while Scanning (§20);
                    // resolution re-arms via Scan Again / Cancel (§14.2).
                    enabled = scanState == ScanState.Scanning,
                    onBarcode = viewModel::onBarcodeScanned,
                    modifier = Modifier
                        .fillMaxWidth()
                        .height(320.dp)
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

            // Manual Entry (fallback text input + Search, §12.5).
            Card(modifier = Modifier.fillMaxWidth()) {
                Column(
                    modifier = Modifier.padding(16.dp),
                    verticalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    Text(
                        text = "Manual Entry",
                        style = MaterialTheme.typography.titleSmall.copy(
                            fontWeight = FontWeight.Bold
                        ),
                        color = MaterialTheme.colorScheme.onSurface
                    )
                    OutlinedTextField(
                        value = manualEntry,
                        onValueChange = { manualEntry = it },
                        label = { Text("Barcode") },
                        singleLine = true,
                        keyboardOptions = KeyboardOptions(imeAction = ImeAction.Search),
                        keyboardActions = KeyboardActions(onSearch = { submitManual() }),
                        modifier = Modifier.fillMaxWidth()
                    )
                    Button(
                        onClick = { submitManual() },
                        enabled = scanState != ScanState.Resolving &&
                            manualEntry.isNotBlank(),
                        modifier = Modifier.fillMaxWidth()
                    ) {
                        Text("Search")
                    }
                }
            }

            when (scanState) {
                ScanState.Scanning -> Unit
                ScanState.Resolving -> {
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.Center
                    ) {
                        CircularProgressIndicator()
                    }
                }
                ScanState.Found -> {
                    val item = resolvedItem
                    if (item != null) {
                        // Result Region — Scenario A (UX Blueprint §7):
                        // Barcode, Item Code, Item Name, Unit.
                        FoundResultCard(
                            scannedValue = lastScannedValue,
                            item = item,
                            onScanAgain = viewModel::resetToScanning,
                            onClose = onClose
                        )
                    }
                }
                ScanState.NotFound -> {
                    // Not Found Region — Scenario B (UX Blueprint §7,
                    // IR-M1): Register Barcode / Cancel.
                    Card(modifier = Modifier.fillMaxWidth()) {
                        Column(
                            modifier = Modifier.padding(16.dp),
                            verticalArrangement = Arrangement.spacedBy(8.dp)
                        ) {
                            Text(
                                text = "Barcode Not Found",
                                style = MaterialTheme.typography.titleMedium.copy(
                                    fontWeight = FontWeight.Bold
                                ),
                                color = MaterialTheme.colorScheme.onSurface
                            )
                            Text(
                                text = lastScannedValue,
                                style = MaterialTheme.typography.bodyMedium,
                                color = MaterialTheme.colorScheme.onSurfaceVariant
                            )
                            Button(
                                onClick = { onRegisterBarcode(lastScannedValue) },
                                modifier = Modifier.fillMaxWidth()
                            ) {
                                Text("Register Barcode")
                            }
                            OutlinedButton(
                                onClick = viewModel::resetToScanning,
                                modifier = Modifier.fillMaxWidth()
                            ) {
                                Text("Cancel")
                            }
                        }
                    }
                }
            }

            Spacer(modifier = Modifier.height(8.dp))
            OutlinedButton(
                onClick = onClose,
                modifier = Modifier.fillMaxWidth()
            ) {
                Text("Back")
            }
        }
    }
}

@Composable
private fun FoundResultCard(
    scannedValue: String,
    item: BarcodeEntity,
    onScanAgain: () -> Unit,
    onClose: () -> Unit
) {
    Card(modifier = Modifier.fillMaxWidth()) {
        Column(
            modifier = Modifier.padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(2.dp)
        ) {
            Text(
                text = "Barcode Found",
                style = MaterialTheme.typography.titleMedium.copy(
                    fontWeight = FontWeight.Bold
                ),
                color = MaterialTheme.colorScheme.onSurface
            )
            Spacer(modifier = Modifier.height(6.dp))
            ResultRow("Barcode", scannedValue)
            ResultRow("Item Code", item.brgCode)
            ResultRow("Item Name", item.brgName)
            ResultRow(
                "Unit",
                item.satuan.ifBlank { "-" }
            )
            Spacer(modifier = Modifier.height(8.dp))
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.spacedBy(8.dp),
                verticalAlignment = Alignment.CenterVertically
            ) {
                Button(
                    onClick = onScanAgain,
                    modifier = Modifier.weight(1f)
                ) {
                    Text("Scan Again")
                }
                OutlinedButton(
                    onClick = onClose,
                    modifier = Modifier.weight(1f)
                ) {
                    Text("Close")
                }
            }
        }
    }
}

@Composable
private fun ResultRow(label: String, value: String) {
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
