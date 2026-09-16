package com.elsasa.bgud.ui

import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavHostController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import com.elsasa.bgud.database.AppDatabase
import com.elsasa.bgud.datastore.SessionPreferencesDataSource
import com.elsasa.bgud.ui.screen.LoginScreen
import com.elsasa.bgud.viewmodel.LoginViewModel
import com.elsasa.bgud.viewmodel.LoginViewModelFactory

/**
 * Cloud API base URL (transport unresolved, C-3/R-03).
 *
 * No environment URL is hardcoded in the network layer (S5.2); the single
 * composition-root value is held here and passed down. It is intentionally
 * blank until deployment/transport is resolved — [LoginViewModel] reports
 * the missing configuration in the login error region instead of issuing a
 * call. Never append a tenant segment: the tenant is JWT-resolved (ADR-007).
 */
private const val CLOUD_BASE_URL = ""

/**
 * Navigation graph (Architecture §13.2).
 *
 * ```text
 * login
 *   ├─ success ──▶ office resolution ──▶ master data synchronization ──▶ home
 *   └─ failure ──▶ login (error)
 * ```
 *
 * Start destination: `login` when no session (no valid JWT) exists,
 * otherwise `home` (IR-M8: no valid JWT → all operational commands
 * blocked). The session gate re-reads the DataStore token, so a warehouse
 * change (which requires re-authentication, IR-09) returns here via logout
 * (S5.11). Remaining destinations (home content, scan, register, registry,
 * edit, synchronization, settings) are owned by S5.5..S5.11.
 */
@Composable
fun AppNavigation(
    navController: NavHostController,
    database: AppDatabase
) {
    val context = LocalContext.current
    val session = remember { SessionPreferencesDataSource(context) }
    val token by session.token.collectAsState(initial = null)

    if (token == null) {
        // Session not yet read — hold the gate instead of guessing.
        Box(
            modifier = Modifier.fillMaxSize(),
            contentAlignment = Alignment.Center
        ) {
            CircularProgressIndicator()
        }
        return
    }

    NavHost(
        navController = navController,
        startDestination = if (token.isNullOrBlank()) "login" else "home"
    ) {
        composable("login") {
            val factory = remember {
                LoginViewModelFactory(session, database, CLOUD_BASE_URL)
            }
            val loginViewModel: LoginViewModel = viewModel(factory = factory)
            LoginScreen(
                viewModel = loginViewModel,
                onLoginSuccess = {
                    navController.navigate("home") {
                        popUpTo("login") { inclusive = true }
                    }
                }
            )
        }
        composable("home") {
            PlaceholderScreen("Home (S5.5)")
        }
    }
}

@Composable
private fun PlaceholderScreen(label: String) {
    Box(
        modifier = Modifier.fillMaxSize(),
        contentAlignment = Alignment.Center
    ) {
        Text(text = label)
    }
}
