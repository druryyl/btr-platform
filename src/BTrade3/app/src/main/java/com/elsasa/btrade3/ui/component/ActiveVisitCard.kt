package com.elsasa.btrade3.ui.component

import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.NavigateNext
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableLongStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import com.elsasa.btrade3.model.CheckIn
import com.elsasa.btrade3.model.CheckInSyncStatus
import com.elsasa.btrade3.model.formatCheckInElapsedTime
import kotlinx.coroutines.delay

private val SyncIndicatorGreen = Color(0xFF4CAF50)
private val SyncIndicatorRed = Color(0xFFF44336)

@Composable
fun ActiveCheckInStatusRow(
    checkIn: CheckIn,
    onClick: () -> Unit,
    modifier: Modifier = Modifier
) {
    var nowMillis by remember { mutableLongStateOf(System.currentTimeMillis()) }

    LaunchedEffect(checkIn.checkInId) {
        nowMillis = System.currentTimeMillis()
        while (true) {
            delay(60_000)
            nowMillis = System.currentTimeMillis()
        }
    }

    val isUploaded = CheckInSyncStatus.isUploaded(checkIn.statusSync)
    val indicatorColor = if (isUploaded) SyncIndicatorGreen else SyncIndicatorRed
    val elapsedTime = formatCheckInElapsedTime(checkIn, nowMillis)

    Row(
        modifier = modifier
            .fillMaxWidth()
            .clickable(onClick = onClick)
            .padding(top = 2.dp),
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.SpaceBetween
    ) {
        Row(
            verticalAlignment = Alignment.CenterVertically,
            modifier = Modifier.weight(1f)
        ) {
            Box(
                modifier = Modifier
                    .size(8.dp)
                    .background(indicatorColor, CircleShape)
            )
            Spacer(modifier = Modifier.width(6.dp))
            Text(
                text = "Check-In: ${checkIn.customerName} ($elapsedTime)",
                style = MaterialTheme.typography.labelSmall,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                fontWeight = FontWeight.Medium,
                maxLines = 1,
                overflow = TextOverflow.Ellipsis
            )
        }
        Icon(
            imageVector = Icons.AutoMirrored.Filled.NavigateNext,
            contentDescription = "View Check-In History",
            modifier = Modifier.size(16.dp),
            tint = MaterialTheme.colorScheme.onSurfaceVariant
        )
    }
}

@Composable
fun CheckOutConfirmDialog(
    customerName: String,
    onConfirm: () -> Unit,
    onDismiss: () -> Unit,
    isCheckingOut: Boolean = false
) {
    AlertDialog(
        onDismissRequest = { if (!isCheckingOut) onDismiss() },
        title = { Text("Finish Visit?") },
        text = {
            Text("Are you sure you want to finish your visit to \"$customerName\"?")
        },
        confirmButton = {
            TextButton(
                onClick = onConfirm,
                enabled = !isCheckingOut
            ) {
                Text("Check Out")
            }
        },
        dismissButton = {
            TextButton(
                onClick = onDismiss,
                enabled = !isCheckingOut
            ) {
                Text("Cancel")
            }
        }
    )
}
