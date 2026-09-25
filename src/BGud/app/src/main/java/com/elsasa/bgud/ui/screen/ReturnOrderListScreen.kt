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
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.KeyboardActions
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.FloatingActionButton
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.OutlinedTextFieldDefaults
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
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
import com.elsasa.bgud.model.ReturnOrderEntity
import com.elsasa.bgud.ui.component.BrandHeaderBar
import com.elsasa.bgud.ui.component.ChevronRightIcon
import com.elsasa.bgud.ui.component.IndustrialCard
import com.elsasa.bgud.ui.component.ReturnBoxIcon
import com.elsasa.bgud.ui.component.StatusBadge
import com.elsasa.bgud.ui.theme.BrandCrimson
import com.elsasa.bgud.ui.theme.BrandGreen
import com.elsasa.bgud.ui.theme.BrandSage
import com.elsasa.bgud.viewmodel.ReturnOrderListItem
import com.elsasa.bgud.viewmodel.ReturnOrderListViewModel
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

/**
 * Return Order List screen (SCR-MOB-RO-001, Architecture §11.1, §12.1, §14.4,
 * BGUD-RETURN-ORDER-NAV-001 TD-004, TD-006).
 *
 * Modern Industrial Work-Queue layout:
 * - Search bar with brand border styling
 * - Filter chips (Semua, Draft, Synced) with brand color accents
 * - Date section groupings (TODAY / YESTERDAY / EARLIER)
 * - Industrial Cards for return order items with status pills and clear typography
 * - High-contrast FAB for "+ New Return"
 */
@Composable
fun ReturnOrderListScreen(
    viewModel: ReturnOrderListViewModel,
    onOpenDetail: ((returnOrderId: String) -> Unit)? = null,
    onEditDraft: ((returnOrderId: String) -> Unit)? = null,
    onEditReturnOrder: ((returnOrderId: String) -> Unit)? = null,
    onViewDetail: ((returnOrderId: String) -> Unit)? = null,
    onCreate: () -> Unit,
    onBack: () -> Unit
) {
    val handleEditDraft = onEditDraft ?: onEditReturnOrder ?: onOpenDetail ?: {}
    val handleViewDetail = onViewDetail ?: onOpenDetail ?: {}

    val query by viewModel.query.collectAsState()
    val statusFilter by viewModel.statusFilter.collectAsState()
    val results by viewModel.results.collectAsState()
    val groupedSections by viewModel.groupedSections.collectAsState()
    val isEmpty by viewModel.isEmpty.collectAsState()
    val isLoading by viewModel.isLoading.collectAsState()
    val isLoadingMore by viewModel.isLoadingMore.collectAsState()
    val hasMore by viewModel.hasMore.collectAsState()

    Scaffold(
        containerColor = MaterialTheme.colorScheme.background,
        floatingActionButton = {
            FloatingActionButton(
                onClick = onCreate,
                containerColor = BrandGreen,
                contentColor = Color.White,
                shape = RoundedCornerShape(18.dp)
            ) {
                Row(
                    modifier = Modifier.padding(horizontal = 16.dp, vertical = 8.dp),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text(
                        text = "+",
                        style = MaterialTheme.typography.titleLarge.copy(
                            fontWeight = FontWeight.ExtraBold,
                            fontSize = 22.sp
                        )
                    )
                    Spacer(modifier = Modifier.width(6.dp))
                    Text(
                        text = "Retur Baru",
                        style = MaterialTheme.typography.labelLarge.copy(
                            fontWeight = FontWeight.Bold
                        )
                    )
                }
            }
        }
    ) { paddingValues ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
                .padding(horizontal = 16.dp, vertical = 12.dp),
            verticalArrangement = Arrangement.spacedBy(12.dp)
        ) {
            // Header Bar
            BrandHeaderBar(
                title = "Daftar Return Order",
                subtitle = "Antrean kerja penerimaan fisik gudang",
                onBack = onBack
            )

            // Search Bar
            OutlinedTextField(
                value = query,
                onValueChange = viewModel::onQueryChange,
                modifier = Modifier.fillMaxWidth(),
                placeholder = {
                    Text(
                        text = "Cari pelanggan atau tanggal...",
                        style = MaterialTheme.typography.bodyMedium
                    )
                },
                singleLine = true,
                shape = RoundedCornerShape(14.dp),
                keyboardOptions = KeyboardOptions(imeAction = ImeAction.Search),
                keyboardActions = KeyboardActions(
                    onSearch = { viewModel.onSearchSubmitted() }
                ),
                colors = OutlinedTextFieldDefaults.colors(
                    focusedBorderColor = BrandGreen,
                    unfocusedBorderColor = MaterialTheme.colorScheme.outlineVariant,
                    focusedContainerColor = MaterialTheme.colorScheme.surface,
                    unfocusedContainerColor = MaterialTheme.colorScheme.surface
                )
            )

            // Status filter chips: Semua / Draft / Synced
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.spacedBy(8.dp)
            ) {
                ModernFilterPill(
                    label = "Semua",
                    selected = statusFilter == ReturnOrderListViewModel.STATUS_ALL,
                    onClick = { viewModel.onStatusFilterChange(ReturnOrderListViewModel.STATUS_ALL) }
                )
                ModernFilterPill(
                    label = "Draft",
                    selected = statusFilter == ReturnOrderEntity.STATUS_DRAFT,
                    onClick = { viewModel.onStatusFilterChange(ReturnOrderEntity.STATUS_DRAFT) },
                    accentColor = Color(0xFF856404)
                )
                ModernFilterPill(
                    label = "Synced",
                    selected = statusFilter == ReturnOrderEntity.STATUS_SYNCED,
                    onClick = { viewModel.onStatusFilterChange(ReturnOrderEntity.STATUS_SYNCED) },
                    accentColor = BrandGreen
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
                        CircularProgressIndicator(color = BrandGreen)
                    }
                }
                isEmpty -> {
                    Box(
                        modifier = Modifier
                            .fillMaxWidth()
                            .weight(1f),
                        contentAlignment = Alignment.Center
                    ) {
                        Column(
                            horizontalAlignment = Alignment.CenterHorizontally,
                            verticalArrangement = Arrangement.Center
                        ) {
                            Surface(
                                shape = CircleShape,
                                color = BrandSage.copy(alpha = 0.2f),
                                modifier = Modifier.size(64.dp)
                            ) {
                                Box(contentAlignment = Alignment.Center) {
                                    ReturnBoxIcon(
                                        tint = MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.6f),
                                        modifier = Modifier.size(32.dp)
                                    )
                                }
                            }
                            Spacer(modifier = Modifier.height(12.dp))
                            Text(
                                text = "Tidak ada dokumen Return Order",
                                style = MaterialTheme.typography.bodyMedium.copy(
                                    fontWeight = FontWeight.SemiBold
                                ),
                                color = MaterialTheme.colorScheme.onSurface
                            )
                            Text(
                                text = if (query.isNotBlank()) "Tidak ada hasil cocok dengan pencarian" else "Tekan tombol + di bawah untuk membuat retur baru",
                                style = MaterialTheme.typography.bodySmall,
                                color = MaterialTheme.colorScheme.onSurfaceVariant,
                                textAlign = TextAlign.Center,
                                modifier = Modifier.padding(horizontal = 32.dp, vertical = 4.dp)
                            )
                        }
                    }
                }
                else -> {
                    LazyColumn(
                        modifier = Modifier.weight(1f),
                        verticalArrangement = Arrangement.spacedBy(10.dp)
                    ) {
                        groupedSections.forEach { sectionGroup ->
                            item(key = "section_${sectionGroup.section.name}") {
                                DateSectionDivider(
                                    title = sectionGroup.section.displayName,
                                    count = sectionGroup.items.size
                                )
                            }
                            items(
                                items = sectionGroup.items,
                                key = { item -> item.order.returnOrderId }
                            ) { item ->
                                ReturnOrderCardItem(
                                    item = item,
                                    onClick = {
                                        if (item.order.status == ReturnOrderEntity.STATUS_DRAFT) {
                                            handleEditDraft(item.order.returnOrderId)
                                        } else {
                                            handleViewDetail(item.order.returnOrderId)
                                        }
                                    }
                                )
                                // Incremental paging: load next page at list bottom
                                if (item.order.returnOrderId == results.lastOrNull()?.order?.returnOrderId && hasMore && !isLoadingMore) {
                                    LaunchedEffect(results.size) {
                                        viewModel.loadMore()
                                    }
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
                                    CircularProgressIndicator(
                                        modifier = Modifier.size(24.dp),
                                        strokeWidth = 2.dp,
                                        color = BrandGreen
                                    )
                                }
                            }
                        }

                        // Bottom space so FAB doesn't cover last item
                        item(key = "bottom-spacer") {
                            Spacer(modifier = Modifier.height(72.dp))
                        }
                    }
                }
            }
        }
    }
}

/**
 * Filter Pill component styled for industrial warehouse aesthetic.
 */
@Composable
private fun ModernFilterPill(
    label: String,
    selected: Boolean,
    onClick: () -> Unit,
    accentColor: Color = BrandGreen
) {
    val bgColor = if (selected) accentColor else MaterialTheme.colorScheme.surface
    val textColor = if (selected) Color.White else MaterialTheme.colorScheme.onSurface
    val borderColor = if (selected) accentColor else MaterialTheme.colorScheme.outlineVariant

    Surface(
        modifier = Modifier.clickable(onClick = onClick),
        shape = RoundedCornerShape(10.dp),
        color = bgColor,
        border = BorderStroke(1.dp, borderColor)
    ) {
        Text(
            text = label,
            modifier = Modifier.padding(horizontal = 14.dp, vertical = 6.dp),
            style = MaterialTheme.typography.labelMedium.copy(
                fontWeight = if (selected) FontWeight.Bold else FontWeight.Medium
            ),
            color = textColor
        )
    }
}

/**
 * Section grouping header (TODAY, YESTERDAY, EARLIER).
 */
@Composable
private fun DateSectionDivider(title: String, count: Int) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(top = 10.dp, bottom = 4.dp),
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.SpaceBetween
    ) {
        Row(
            verticalAlignment = Alignment.CenterVertically,
            modifier = Modifier.weight(1f, fill = false)
        ) {
            Text(
                text = title.uppercase(),
                style = MaterialTheme.typography.labelSmall.copy(
                    fontWeight = FontWeight.Bold,
                    fontFamily = FontFamily.Monospace,
                    letterSpacing = 1.sp
                ),
                color = BrandCrimson
            )
            Spacer(modifier = Modifier.width(8.dp))
            HorizontalDivider(
                modifier = Modifier.weight(1f),
                color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f)
            )
        }
        Spacer(modifier = Modifier.width(8.dp))
        Text(
            text = "$count dokumen",
            style = MaterialTheme.typography.labelSmall.copy(
                fontSize = 11.sp,
                fontFamily = FontFamily.Monospace
            ),
            color = MaterialTheme.colorScheme.onSurfaceVariant
        )
    }
}

/**
 * Individual Return Order Card in the Work Queue.
 */
@Composable
private fun ReturnOrderCardItem(
    item: ReturnOrderListItem,
    onClick: () -> Unit
) {
    val order = item.order
    val isDraft = order.status == ReturnOrderEntity.STATUS_DRAFT

    IndustrialCard(
        modifier = Modifier,
        onClick = onClick,
        borderColor = if (isDraft) Color(0xFFFFE082) else MaterialTheme.colorScheme.outlineVariant,
        containerColor = MaterialTheme.colorScheme.surface
    ) {
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.SpaceBetween,
            verticalAlignment = Alignment.Top
        ) {
            Column(modifier = Modifier.weight(1f)) {
                Text(
                    text = order.customerName.ifBlank { order.customerCode },
                    style = MaterialTheme.typography.titleSmall.copy(
                        fontWeight = FontWeight.Bold
                    ),
                    color = MaterialTheme.colorScheme.onSurface,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis
                )
                if (order.customerName.isNotBlank() && order.customerCode.isNotBlank()) {
                    Text(
                        text = order.customerCode,
                        style = MaterialTheme.typography.labelSmall.copy(
                            fontFamily = FontFamily.Monospace,
                            fontSize = 10.sp
                        ),
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
            }

            StatusBadge(status = order.status)
        }

        Spacer(modifier = Modifier.height(10.dp))
        HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.4f))
        Spacer(modifier = Modifier.height(8.dp))

        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.SpaceBetween,
            verticalAlignment = Alignment.CenterVertically
        ) {
            Row(verticalAlignment = Alignment.CenterVertically) {
                Text(
                    text = formatOrderDate(order.createdAt),
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
                Text(
                    text = " • ",
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
                Surface(
                    shape = RoundedCornerShape(4.dp),
                    color = BrandSage.copy(alpha = 0.35f)
                ) {
                    Text(
                        text = "${item.itemCount} Item",
                        modifier = Modifier.padding(horizontal = 5.dp, vertical = 1.dp),
                        style = MaterialTheme.typography.labelSmall.copy(
                            fontWeight = FontWeight.Bold,
                            fontSize = 10.sp
                        ),
                        color = MaterialTheme.colorScheme.onSurface
                    )
                }
            }

            Row(verticalAlignment = Alignment.CenterVertically) {
                Text(
                    text = if (isDraft) "Lanjut Edit" else "Lihat Detail",
                    style = MaterialTheme.typography.labelSmall.copy(
                        fontWeight = FontWeight.Bold
                    ),
                    color = if (isDraft) BrandCrimson else BrandGreen
                )
                Spacer(modifier = Modifier.width(4.dp))
                ChevronRightIcon(
                    tint = if (isDraft) BrandCrimson else BrandGreen,
                    modifier = Modifier.size(12.dp)
                )
            }
        }
    }
}

private fun formatOrderDate(timestampMillis: Long): String {
    if (timestampMillis <= 0L) return "-"
    return SimpleDateFormat("dd MMM yyyy HH:mm", Locale.getDefault())
        .format(Date(timestampMillis))
}
