package com.elsasa.bgud.ui.screen

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
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.elsasa.bgud.model.ReturnOrderItemEntity
import com.elsasa.bgud.ui.component.BrandHeaderBar
import com.elsasa.bgud.ui.component.IndustrialCard
import com.elsasa.bgud.ui.component.StatusBadge
import com.elsasa.bgud.ui.theme.BrandGreen
import com.elsasa.bgud.ui.theme.BrandSage
import com.elsasa.bgud.ui.theme.BrandWarmCream
import com.elsasa.bgud.viewmodel.ReturnOrderDetailViewModel

/**
 * Return Order Detail screen (SCR-MOB-RO-003, Architecture §11.1, §12.3,
 * §14.2). Exclusively for Synced orders; Delete action relocated to Edit
 * screen (TD-004, TD-005, P2-S03).
 *
 * Modern Industrial layout:
 * - Brand Header with SYNCED status badge
 * - Synchronized status banner
 * - Header context card: Customer, Warehouse, Salesman, Driver, Notes
 * - Item list breakdown cards
 * - Back action
 */
@Composable
fun ReturnOrderDetailScreen(
    viewModel: ReturnOrderDetailViewModel,
    onEdit: (returnOrderId: String) -> Unit,
    onDeleted: () -> Unit = {},
    onBack: () -> Unit
) {
    val isLoading by viewModel.isLoading.collectAsState()
    val order by viewModel.order.collectAsState()
    val items by viewModel.items.collectAsState()
    val notFound by viewModel.notFound.collectAsState()

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
                title = "Detail Return Order",
                subtitle = "Arsip dokumen penerimaan fisik tersinkron",
                onBack = onBack,
                actions = {
                    order?.let { StatusBadge(status = it.status) }
                }
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
                notFound || order == null -> {
                    IndustrialCard(borderColor = MaterialTheme.colorScheme.outlineVariant) {
                        Text(
                            text = "Data Return Order tidak ditemukan.",
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
                            Text("Kembali ke Antrean")
                        }
                    }
                    return@Column
                }
            }

            val loaded = order!!

            // Synced Read-Only Banner
            Surface(
                modifier = Modifier.fillMaxWidth(),
                shape = RoundedCornerShape(12.dp),
                color = BrandGreen.copy(alpha = 0.12f),
                border = BorderStroke(1.dp, BrandGreen.copy(alpha = 0.4f))
            ) {
                Row(
                    modifier = Modifier.padding(horizontal = 14.dp, vertical = 10.dp),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text(
                        text = "✓",
                        style = MaterialTheme.typography.titleMedium.copy(fontWeight = FontWeight.Bold),
                        color = BrandGreen
                    )
                    Spacer(modifier = Modifier.width(10.dp))
                    Text(
                        text = "Dokumen telah tersinkronkan ke server pusat. Data bersifat arsip resmi (read-only).",
                        style = MaterialTheme.typography.bodySmall.copy(fontWeight = FontWeight.Medium),
                        color = Color(0xFF1E460E)
                    )
                }
            }

            // Context Card
            IndustrialCard(borderColor = BrandSage, containerColor = MaterialTheme.colorScheme.surface) {
                Text(
                    text = "KONTEKS DOKUMEN",
                    style = MaterialTheme.typography.labelSmall.copy(
                        fontWeight = FontWeight.Bold,
                        fontFamily = FontFamily.Monospace,
                        letterSpacing = 1.sp
                    ),
                    color = MaterialTheme.colorScheme.primary
                )

                Spacer(modifier = Modifier.height(10.dp))

                DetailInfoRow(
                    label = "Customer Pelanggan",
                    value = loaded.customerName.ifBlank { loaded.customerCode },
                    subValue = loaded.customerCode.takeIf { loaded.customerName.isNotBlank() }
                )

                HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.4f), modifier = Modifier.padding(vertical = 8.dp))

                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween
                ) {
                    DetailInfoRow(label = "Gudang Penugasan", value = loaded.warehouseCode, isMonospace = true)
                    DetailInfoRow(label = "Status", value = loaded.status, isStatus = true)
                }

                HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.4f), modifier = Modifier.padding(vertical = 8.dp))

                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween
                ) {
                    DetailInfoRow(label = "Salesman", value = loaded.salesPersonName.ifBlank { "-" })
                    DetailInfoRow(label = "Driver", value = loaded.driverName.ifBlank { "-" })
                }

                if (loaded.note.isNotBlank()) {
                    HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.4f), modifier = Modifier.padding(vertical = 8.dp))
                    DetailInfoRow(label = "Catatan", value = loaded.note)
                }
            }

            // Items Region
            Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text(
                        text = "DAFTAR BARANG DITERIMA",
                        style = MaterialTheme.typography.labelSmall.copy(
                            fontWeight = FontWeight.Bold,
                            fontFamily = FontFamily.Monospace,
                            letterSpacing = 1.sp
                        ),
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                    Surface(
                        shape = RoundedCornerShape(4.dp),
                        color = BrandSage.copy(alpha = 0.4f)
                    ) {
                        Text(
                            text = "${items.size} Item",
                            modifier = Modifier.padding(horizontal = 6.dp, vertical = 2.dp),
                            style = MaterialTheme.typography.labelSmall.copy(fontWeight = FontWeight.Bold),
                            color = MaterialTheme.colorScheme.onSurface
                        )
                    }
                }

                if (items.isEmpty()) {
                    IndustrialCard(borderColor = MaterialTheme.colorScheme.outlineVariant) {
                        Text(
                            text = "Tidak ada rincian item.",
                            style = MaterialTheme.typography.bodyMedium,
                            color = MaterialTheme.colorScheme.onSurfaceVariant,
                            textAlign = TextAlign.Center,
                            modifier = Modifier.fillMaxWidth()
                        )
                    }
                } else {
                    items.forEach { item ->
                        ModernDetailItemCard(item)
                    }
                }
            }

            // Actions
            Spacer(modifier = Modifier.height(4.dp))
            OutlinedButton(
                onClick = onBack,
                modifier = Modifier
                    .fillMaxWidth()
                    .height(50.dp),
                shape = RoundedCornerShape(14.dp),
                colors = ButtonDefaults.outlinedButtonColors(
                    containerColor = MaterialTheme.colorScheme.surface
                ),
                border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant)
            ) {
                Text("Kembali ke Antrean", fontWeight = FontWeight.SemiBold)
            }

            Spacer(modifier = Modifier.height(12.dp))
        }
    }
}

@Composable
private fun DetailInfoRow(
    label: String,
    value: String,
    subValue: String? = null,
    isMonospace: Boolean = false,
    isStatus: Boolean = false
) {
    Column {
        Text(
            text = label,
            style = MaterialTheme.typography.labelSmall,
            color = MaterialTheme.colorScheme.onSurfaceVariant
        )
        Spacer(modifier = Modifier.height(2.dp))
        if (isStatus) {
            StatusBadge(status = value)
        } else {
            Text(
                text = value,
                style = MaterialTheme.typography.bodyMedium.copy(
                    fontWeight = FontWeight.Bold,
                    fontFamily = if (isMonospace) FontFamily.Monospace else FontFamily.Default
                ),
                color = MaterialTheme.colorScheme.onSurface
            )
            if (subValue != null) {
                Text(
                    text = subValue,
                    style = MaterialTheme.typography.labelSmall.copy(fontFamily = FontFamily.Monospace),
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }
        }
    }
}

@Composable
private fun ModernDetailItemCard(item: ReturnOrderItemEntity) {
    Surface(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(12.dp),
        color = MaterialTheme.colorScheme.surface,
        border = BorderStroke(1.dp, MaterialTheme.colorScheme.outlineVariant)
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(14.dp),
            horizontalArrangement = Arrangement.SpaceBetween,
            verticalAlignment = Alignment.CenterVertically
        ) {
            Column(modifier = Modifier.weight(1f)) {
                Text(
                    text = item.brgName.ifBlank { item.brgCode },
                    style = MaterialTheme.typography.bodyMedium.copy(fontWeight = FontWeight.SemiBold),
                    color = MaterialTheme.colorScheme.onSurface
                )
                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    modifier = Modifier.padding(top = 2.dp)
                ) {
                    Text(
                        text = item.brgCode,
                        style = MaterialTheme.typography.labelSmall.copy(
                            fontFamily = FontFamily.Monospace,
                            fontSize = 10.sp
                        ),
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                    Text(text = " • ", style = MaterialTheme.typography.labelSmall)
                    Text(
                        text = "${formatQty(item.qty)} ${item.satId}",
                        style = MaterialTheme.typography.labelSmall.copy(fontWeight = FontWeight.Bold),
                        color = MaterialTheme.colorScheme.onSurface
                    )
                }
            }

            StatusBadge(status = item.jenisRetur)
        }
    }
}

private fun formatQty(qty: Double): String {
    return if (qty % 1.0 == 0.0) qty.toLong().toString() else qty.toString()
}
