package com.elsasa.bgud.ui.component

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.Canvas
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.ColumnScope
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.geometry.CornerRadius
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.geometry.Size
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.Path
import androidx.compose.ui.graphics.drawscope.Stroke
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.elsasa.bgud.model.ReturnOrderEntity
import com.elsasa.bgud.ui.theme.BrandCrimson
import com.elsasa.bgud.ui.theme.BrandGreen
import com.elsasa.bgud.ui.theme.BrandSage
import com.elsasa.bgud.ui.theme.BrandWarmCream

/**
 * Shared Industrial Card container used across modern BGud screens.
 */
@Composable
fun IndustrialCard(
    modifier: Modifier = Modifier,
    borderColor: Color = MaterialTheme.colorScheme.outlineVariant,
    containerColor: Color = MaterialTheme.colorScheme.surface,
    elevation: Dp = 1.5.dp,
    onClick: (() -> Unit)? = null,
    content: @Composable ColumnScope.() -> Unit
) {
    val cardModifier = if (onClick != null) {
        modifier.clickable(onClick = onClick)
    } else {
        modifier
    }

    Card(
        modifier = cardModifier.fillMaxWidth(),
        shape = RoundedCornerShape(18.dp),
        colors = CardDefaults.cardColors(containerColor = containerColor),
        elevation = CardDefaults.cardElevation(defaultElevation = elevation),
        border = BorderStroke(1.dp, borderColor)
    ) {
        Column(
            modifier = Modifier
                .fillMaxWidth()
                .padding(16.dp),
            content = content
        )
    }
}

/**
 * Top App Header Bar with optional back button, title, and actions.
 */
@Composable
fun BrandHeaderBar(
    title: String,
    subtitle: String? = null,
    onBack: (() -> Unit)? = null,
    actions: (@Composable () -> Unit)? = null
) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(vertical = 4.dp),
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.SpaceBetween
    ) {
        Row(
            verticalAlignment = Alignment.CenterVertically,
            modifier = Modifier.weight(1f, fill = false)
        ) {
            if (onBack != null) {
                Surface(
                    modifier = Modifier
                        .size(36.dp)
                        .clip(CircleShape)
                        .clickable(onClick = onBack),
                    color = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f)
                ) {
                    Box(contentAlignment = Alignment.Center) {
                        ChevronLeftIcon(
                            tint = MaterialTheme.colorScheme.onSurface,
                            modifier = Modifier.size(18.dp)
                        )
                    }
                }
                Spacer(modifier = Modifier.width(12.dp))
            }
            Column {
                Text(
                    text = title,
                    style = MaterialTheme.typography.titleLarge.copy(
                        fontWeight = FontWeight.Bold,
                        letterSpacing = (-0.3).sp
                    ),
                    color = MaterialTheme.colorScheme.onSurface
                )
                if (subtitle != null) {
                    Text(
                        text = subtitle,
                        style = MaterialTheme.typography.labelSmall,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
            }
        }
        if (actions != null) {
            actions()
        }
    }
}

/**
 * Logistics Brand Badge with Crimson container.
 */
@Composable
fun LogisticsBrandBadge(modifier: Modifier = Modifier, sizeDp: Dp = 40.dp) {
    Surface(
        modifier = modifier.size(sizeDp),
        shape = RoundedCornerShape(12.dp),
        color = BrandCrimson,
        shadowElevation = 3.dp
    ) {
        Box(contentAlignment = Alignment.Center) {
            Canvas(modifier = Modifier.size(sizeDp * 0.55f)) {
                val sw = 1.8f * density
                val w = size.width
                val h = size.height

                val path = Path().apply {
                    moveTo(w * 0.15f, h * 0.35f)
                    lineTo(w * 0.5f, h * 0.15f)
                    lineTo(w * 0.85f, h * 0.35f)
                    lineTo(w * 0.85f, h * 0.85f)
                    lineTo(w * 0.15f, h * 0.85f)
                    close()
                }
                drawPath(path, color = BrandWarmCream, style = Stroke(width = sw))

                drawRoundRect(
                    color = BrandWarmCream,
                    topLeft = Offset(w * 0.38f, h * 0.55f),
                    size = Size(w * 0.24f, h * 0.30f),
                    cornerRadius = CornerRadius(1.5f * density, 1.5f * density),
                    style = Stroke(width = sw * 0.9f)
                )
            }
        }
    }
}

/**
 * Standard Status Badge for Return Orders and Sync statuses.
 */
@Composable
fun StatusBadge(
    status: String,
    modifier: Modifier = Modifier
) {
    val (bgColor, textColor, label) = when (status.uppercase()) {
        ReturnOrderEntity.STATUS_DRAFT, "DRAFT" -> Triple(
            Color(0xFFFFF3CD),
            Color(0xFF856404),
            "DRAFT"
        )
        ReturnOrderEntity.STATUS_SYNCED, "SYNCED" -> Triple(
            BrandGreen.copy(alpha = 0.15f),
            BrandGreen,
            "SYNCED"
        )
        "ONLINE" -> Triple(
            BrandGreen.copy(alpha = 0.15f),
            BrandGreen,
            "ONLINE"
        )
        "OFFLINE" -> Triple(
            BrandCrimson.copy(alpha = 0.12f),
            BrandCrimson,
            "OFFLINE"
        )
        "BAGUS" -> Triple(
            BrandGreen.copy(alpha = 0.15f),
            BrandGreen,
            "BAGUS"
        )
        "RUSAK" -> Triple(
            BrandCrimson.copy(alpha = 0.12f),
            BrandCrimson,
            "RUSAK"
        )
        else -> Triple(
            MaterialTheme.colorScheme.surfaceVariant,
            MaterialTheme.colorScheme.onSurfaceVariant,
            status
        )
    }

    Surface(
        modifier = modifier,
        shape = RoundedCornerShape(6.dp),
        color = bgColor,
        border = BorderStroke(1.dp, textColor.copy(alpha = 0.3f))
    ) {
        Text(
            text = label,
            modifier = Modifier.padding(horizontal = 7.dp, vertical = 2.dp),
            style = MaterialTheme.typography.labelSmall.copy(
                fontWeight = FontWeight.Bold,
                fontFamily = FontFamily.Monospace,
                fontSize = 10.sp,
                letterSpacing = 0.5.sp
            ),
            color = textColor
        )
    }
}

// ==========================================
// CANVASES & VECTOR ICONS
// ==========================================

@Composable
fun ChevronLeftIcon(tint: Color, modifier: Modifier = Modifier) {
    Canvas(modifier = modifier) {
        val w = size.width
        val h = size.height
        val path = Path().apply {
            moveTo(w * 0.62f, h * 0.25f)
            lineTo(w * 0.35f, h * 0.5f)
            lineTo(w * 0.62f, h * 0.75f)
        }
        drawPath(path, color = tint, style = Stroke(width = 2.2f * density))
    }
}

@Composable
fun ChevronRightIcon(tint: Color, modifier: Modifier = Modifier) {
    Canvas(modifier = modifier) {
        val w = size.width
        val h = size.height
        val path = Path().apply {
            moveTo(w * 0.38f, h * 0.25f)
            lineTo(w * 0.65f, h * 0.5f)
            lineTo(w * 0.38f, h * 0.75f)
        }
        drawPath(path, color = tint, style = Stroke(width = 2.2f * density))
    }
}

@Composable
fun ReturnBoxIcon(tint: Color, modifier: Modifier = Modifier) {
    Canvas(modifier = modifier) {
        val w = size.width
        val h = size.height
        val sw = 1.8f * density

        // Box outline
        drawRoundRect(
            color = tint,
            topLeft = Offset(w * 0.15f, h * 0.25f),
            size = Size(w * 0.70f, h * 0.60f),
            cornerRadius = CornerRadius(2f * density, 2f * density),
            style = Stroke(width = sw)
        )
        // Top flap line
        drawLine(
            color = tint,
            start = Offset(w * 0.15f, h * 0.45f),
            end = Offset(w * 0.85f, h * 0.45f),
            strokeWidth = sw * 0.8f
        )
        // Arrow pointing back
        val arrow = Path().apply {
            moveTo(w * 0.65f, h * 0.32f)
            lineTo(w * 0.45f, h * 0.32f)
            lineTo(w * 0.52f, h * 0.22f)
        }
        drawPath(arrow, color = tint, style = Stroke(width = sw))
    }
}

@Composable
fun SyncCircleIcon(tint: Color, modifier: Modifier = Modifier) {
    Canvas(modifier = modifier) {
        val w = size.width
        val h = size.height
        val sw = 1.8f * density
        val radius = w * 0.35f
        val center = Offset(w / 2f, h / 2f)

        drawArc(
            color = tint,
            startAngle = 30f,
            sweepAngle = 140f,
            useCenter = false,
            topLeft = Offset(center.x - radius, center.y - radius),
            size = Size(radius * 2, radius * 2),
            style = Stroke(width = sw)
        )
        drawArc(
            color = tint,
            startAngle = 210f,
            sweepAngle = 140f,
            useCenter = false,
            topLeft = Offset(center.x - radius, center.y - radius),
            size = Size(radius * 2, radius * 2),
            style = Stroke(width = sw)
        )
    }
}

@Composable
fun BarcodeCardIcon(tint: Color, modifier: Modifier = Modifier) {
    Canvas(modifier = modifier) {
        val w = size.width
        val h = size.height
        val sw = 1.5f * density

        drawLine(tint, Offset(w * 0.20f, h * 0.25f), Offset(w * 0.20f, h * 0.75f), sw * 1.6f)
        drawLine(tint, Offset(w * 0.32f, h * 0.25f), Offset(w * 0.32f, h * 0.75f), sw * 0.9f)
        drawLine(tint, Offset(w * 0.45f, h * 0.25f), Offset(w * 0.45f, h * 0.75f), sw * 2.0f)
        drawLine(tint, Offset(w * 0.60f, h * 0.25f), Offset(w * 0.60f, h * 0.75f), sw * 1.0f)
        drawLine(tint, Offset(w * 0.72f, h * 0.25f), Offset(w * 0.72f, h * 0.75f), sw * 1.8f)
        drawLine(tint, Offset(w * 0.82f, h * 0.25f), Offset(w * 0.82f, h * 0.75f), sw * 0.9f)
    }
}

@Composable
fun GearIcon(tint: Color, modifier: Modifier = Modifier) {
    Canvas(modifier = modifier) {
        val w = size.width
        val h = size.height
        val sw = 1.8f * density
        val center = Offset(w / 2f, h / 2f)

        drawCircle(
            color = tint,
            radius = w * 0.32f,
            center = center,
            style = Stroke(width = sw)
        )
        drawCircle(
            color = tint,
            radius = w * 0.12f,
            center = center,
            style = Stroke(width = sw)
        )
    }
}
