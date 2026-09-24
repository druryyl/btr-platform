package com.elsasa.bgud.ui.screen

import android.app.Activity
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Button
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.ExposedDropdownMenuBox
import androidx.compose.material3.ExposedDropdownMenuDefaults
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
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import com.elsasa.bgud.util.GoogleSignInHelper
import com.elsasa.bgud.viewmodel.LoginViewModel

/**
 * Login screen (SCR-MOB-001, FEATURE §6).
 *
 * ```text
 * Header          (application identity)
 * Google gate     (Sign in with Google → resolve the account)
 * Identity        (resolved Google email)
 * Gudang selector (Gudang Gamping / Gudang Concat / Gudang Magelang)
 * Action          (Masuk)
 * Feedback        (error message region)
 * ```
 *
 * The operator signs in with a Google account; the email is resolved through
 * `POST api/session/resolve` (TD-02). Only then is the Gudang selector usable.
 * On success the caller navigates to Home; a refusal or cancellation leaves
 * the operator on this screen, signed out. No password, JWT, or
 * `Authorization` header exists anywhere (ARCHITECTURE §10).
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun LoginScreen(
    viewModel: LoginViewModel,
    onLoginSuccess: () -> Unit
) {
    val context = LocalContext.current
    val googleSignInHelper = remember { GoogleSignInHelper(context) }

    val resolvedAccount by viewModel.resolvedAccount.collectAsState()
    val selectedWarehouse by viewModel.selectedWarehouse.collectAsState()
    val warehouseOptions by viewModel.warehouseOptions.collectAsState()
    val isLoading by viewModel.isLoading.collectAsState()
    val isSyncing by viewModel.isSyncing.collectAsState()
    val error by viewModel.error.collectAsState()

    var warehouseExpanded by remember { mutableStateOf(false) }

    val isResolved = resolvedAccount != null
    val canEstablish = isResolved && selectedWarehouse != null && !isLoading

    val signInLauncher = rememberLauncherForActivityResult(
        ActivityResultContracts.StartActivityForResult()
    ) { result ->
        if (result.resultCode == Activity.RESULT_OK) {
            val email = googleSignInHelper.getSignedInEmail(result.data)
            if (email.isNullOrBlank()) {
                viewModel.onSignInCancelled()
            } else {
                viewModel.onGoogleAccountSelected(email)
            }
        } else {
            // Google sign-in cancelled or failed — remain signed out.
            viewModel.onSignInCancelled()
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
                .padding(horizontal = 32.dp),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Spacer(modifier = Modifier.height(72.dp))

            Text(
                text = "BGud",
                style = MaterialTheme.typography.headlineLarge.copy(
                    fontWeight = FontWeight.Bold
                ),
                color = MaterialTheme.colorScheme.onSurface
            )
            Text(
                text = "Masuk untuk operasional gudang",
                style = MaterialTheme.typography.bodyMedium,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                textAlign = TextAlign.Center,
                modifier = Modifier.padding(top = 4.dp, bottom = 32.dp)
            )

            Button(
                onClick = { signInLauncher.launch(googleSignInHelper.getSignInIntent()) },
                enabled = !isLoading,
                modifier = Modifier
                    .fillMaxWidth()
                    .height(52.dp)
            ) {
                if (isLoading && !isResolved) {
                    CircularProgressIndicator(
                        modifier = Modifier.size(22.dp),
                        strokeWidth = 2.dp
                    )
                } else {
                    Text("Masuk dengan Google")
                }
            }

            if (isResolved) {
                Text(
                    text = resolvedAccount?.email.orEmpty(),
                    style = MaterialTheme.typography.bodyMedium,
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    textAlign = TextAlign.Center,
                    modifier = Modifier.padding(top = 12.dp)
                )
            }

            Spacer(modifier = Modifier.height(16.dp))

            ExposedDropdownMenuBox(
                expanded = warehouseExpanded,
                onExpandedChange = {
                    if (isResolved && !isLoading) warehouseExpanded = !warehouseExpanded
                }
            ) {
                OutlinedTextField(
                    value = selectedWarehouse?.displayName.orEmpty(),
                    onValueChange = {},
                    readOnly = true,
                    enabled = isResolved && !isLoading,
                    modifier = Modifier
                        .menuAnchor(MenuAnchorType.PrimaryNotEditable)
                        .fillMaxWidth(),
                    label = { Text("Gudang") },
                    trailingIcon = {
                        ExposedDropdownMenuDefaults.TrailingIcon(warehouseExpanded)
                    },
                    colors = ExposedDropdownMenuDefaults.outlinedTextFieldColors()
                )
                ExposedDropdownMenu(
                    expanded = warehouseExpanded,
                    onDismissRequest = { warehouseExpanded = false }
                ) {
                    warehouseOptions.forEach { option ->
                        DropdownMenuItem(
                            text = { Text(option.displayName) },
                            onClick = {
                                viewModel.onWarehouseChange(option)
                                warehouseExpanded = false
                            }
                        )
                    }
                }
            }

            Spacer(modifier = Modifier.height(24.dp))

            OutlinedButton(
                onClick = { viewModel.establishSession(onLoginSuccess) },
                enabled = canEstablish,
                modifier = Modifier
                    .fillMaxWidth()
                    .height(52.dp)
            ) {
                if (isLoading && isResolved) {
                    CircularProgressIndicator(
                        modifier = Modifier.size(22.dp),
                        strokeWidth = 2.dp
                    )
                } else {
                    Text("Masuk")
                }
            }

            if (isSyncing) {
                Text(
                    text = "Sinkronisasi data master…",
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    modifier = Modifier.padding(top = 12.dp)
                )
            }

            if (error != null) {
                Text(
                    text = error!!,
                    color = MaterialTheme.colorScheme.error,
                    style = MaterialTheme.typography.bodySmall,
                    textAlign = TextAlign.Center,
                    modifier = Modifier.padding(top = 12.dp)
                )
            }

            Spacer(modifier = Modifier.height(32.dp))
        }
    }
}
