package com.elsasa.bgud.ui.screen

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.elsasa.bgud.viewmodel.SettingsViewModel

/**
 * Settings screen (SCR-MOB-008, Architecture §12 layout inventory,
 * UX Blueprint §4 navigation leaf).
 *
 * ```text
 * Session Card  (User, Warehouse, Office — display-only)
 * Actions       (Logout, Kembali)
 * ```
 *
 * Settings is a navigation leaf (traceability: navigation — no workflow,
 * no domain capability). It issues no network call and stores/sends no
 * `ServerId` (ADR-007, IR-09): the session readout mirrors the Home
 * context header (S5.5 precedent, `officeCode` display-only); `Logout`
 * clears the DataStore session via [SettingsViewModel.logout] so the
 * Navigation start-destination gate returns to `login` (IR-M8, §13.2
 * `any → login`). Warehouse change is logout + re-authentication (IR-09);
 * no activation/deactivation surface exists on this screen (IR-06, C-1).
 */
@Composable
fun SettingsScreen(
    viewModel: SettingsViewModel,
    onLoggedOut: () -> Unit,
    onBack: () -> Unit
) {
    val user by viewModel.user.collectAsState()
    val warehouse by viewModel.warehouse.collectAsState()
    val office by viewModel.office.collectAsState()

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
                text = "Settings",
                style = MaterialTheme.typography.headlineSmall.copy(
                    fontWeight = FontWeight.Bold
                ),
                color = MaterialTheme.colorScheme.onSurface
            )

            // Session Card: login-bound context, display-only (IR-09).
            Card(modifier = Modifier.fillMaxWidth()) {
                Column(modifier = Modifier.padding(16.dp)) {
                    SettingsSectionTitle("Current Session")
                    Spacer(modifier = Modifier.height(8.dp))
                    SettingsRow("Logged In User", user.ifBlank { "-" })
                    SettingsRow("Warehouse", warehouse.ifBlank { "-" })
                    SettingsRow("Office", office.ifBlank { "-" })
                }
            }

            // Actions: Logout (session close → login) + Kembali (back).
            Button(
                onClick = { viewModel.logout(onLoggedOut) },
                modifier = Modifier.fillMaxWidth()
            ) {
                Text("Logout")
            }

            OutlinedButton(
                onClick = onBack,
                modifier = Modifier.fillMaxWidth()
            ) {
                Text("Kembali")
            }
        }
    }
}

@Composable
private fun SettingsSectionTitle(text: String) {
    Text(
        text = text,
        style = MaterialTheme.typography.titleSmall.copy(
            fontWeight = FontWeight.Bold
        ),
        color = MaterialTheme.colorScheme.onSurface
    )
}

@Composable
private fun SettingsRow(label: String, value: String) {
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
