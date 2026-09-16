package com.elsasa.bgud.ui

import android.net.ConnectivityManager
import android.net.Uri
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavHostController
import androidx.navigation.NavType
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.navArgument
import com.elsasa.bgud.database.AppDatabase
import com.elsasa.bgud.datastore.SessionPreferencesDataSource
import com.elsasa.bgud.ui.screen.BarcodeRegistryScreen
import com.elsasa.bgud.ui.screen.EditBarcodeScreen
import com.elsasa.bgud.ui.screen.HomeScreen
import com.elsasa.bgud.ui.screen.LoginScreen
import com.elsasa.bgud.ui.screen.RegisterScreen
import com.elsasa.bgud.ui.screen.ScanScreen
import com.elsasa.bgud.ui.screen.SynchronizationScreen
import com.elsasa.bgud.viewmodel.BarcodeRegistryViewModel
import com.elsasa.bgud.viewmodel.BarcodeRegistryViewModelFactory
import com.elsasa.bgud.viewmodel.EditBarcodeViewModel
import com.elsasa.bgud.viewmodel.EditBarcodeViewModelFactory
import com.elsasa.bgud.viewmodel.HomeViewModel
import com.elsasa.bgud.viewmodel.HomeViewModelFactory
import com.elsasa.bgud.viewmodel.LoginViewModel
import com.elsasa.bgud.viewmodel.LoginViewModelFactory
import com.elsasa.bgud.viewmodel.RegisterBarcodeViewModel
import com.elsasa.bgud.viewmodel.RegisterBarcodeViewModelFactory
import com.elsasa.bgud.viewmodel.ScanViewModel
import com.elsasa.bgud.viewmodel.ScanViewModelFactory
import com.elsasa.bgud.viewmodel.SynchronizationViewModel
import com.elsasa.bgud.viewmodel.SynchronizationViewModelFactory

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
 *
 * home
 *   ├─ Scan Barcode ────────▶ scan
 *   ├─ Search Barcode ──────▶ barcode_registry (S5.8)
 *   ├─ Register Barcode ────▶ register (barcode argument optional)
 *   ├─ Barcode Registry ────▶ barcode_registry (S5.8)
 *   ├─ Synchronization ─────▶ synchronization (S5.10)
 *   └─ Settings ────────────▶ settings (S5.11)
 *
 * scan
 *   ├─ found ───────────────▶ scan (result region) ── Close ──▶ back
 *   ├─ not found + Register ─▶ register?barcode={value} (S5.7)
 *   └─ not found + Cancel ───▶ scan (re-armed Scanning)
 *
 * register
 *   ├─ Save ────▶ local queue ── success message ──▶ back
 *   └─ Cancel ──▶ back
 *
 * barcode_registry (S5.8, SCR-MOB-005)
 *   └─ Edit Barcode ────────▶ edit?barcodeId={id} (S5.9, SCR-MOB-006)
 *
 * edit (S5.9, SCR-MOB-006)
 *   ├─ Save ────▶ local queue ── success message ──▶ back
 *   └─ Cancel ──▶ back
 *
 * synchronization (S5.10, SCR-MOB-007)
 *   └─ Sync Now ──▶ sync worker ──▶ synchronization (refreshed)
 * ```
 *
 * Start destination: `login` when no session (no valid JWT) exists,
 * otherwise `home` (IR-M8: no valid JWT → all operational commands
 * blocked). The session gate re-reads the DataStore token, so a warehouse
 * change (which requires re-authentication, IR-09) returns here via logout
 * (S5.11). The `settings` destination is owned by S5.11; the home
 * navigation row targets its §13.2 route.
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
            val homeFactory = remember {
                val connectivityManager = context.getSystemService(
                    ConnectivityManager::class.java
                )
                HomeViewModelFactory(
                    session,
                    database.barcodeRegistrationRequestDao(),
                    connectivityManager
                )
            }
            val homeViewModel: HomeViewModel = viewModel(factory = homeFactory)
            HomeScreen(
                viewModel = homeViewModel,
                onScanBarcode = { navController.navigate("scan") },
                onSearchBarcode = { navController.navigate("barcode_registry") },
                onRegisterBarcode = { navController.navigate("register") },
                onOpenBarcodeRegistry = { navController.navigate("barcode_registry") },
                onOpenSynchronization = { navController.navigate("synchronization") },
                onOpenSettings = { navController.navigate("settings") }
            )
        }
        composable("scan") {
            val scanFactory = remember {
                ScanViewModelFactory(database.barcodeDao())
            }
            val scanViewModel: ScanViewModel = viewModel(factory = scanFactory)
            ScanScreen(
                viewModel = scanViewModel,
                onClose = { navController.popBackStack() },
                onRegisterBarcode = { barcodeValue ->
                    // §13.2: not found + Register ─▶ register?barcode={value}.
                    navController.navigate("register?barcode=${Uri.encode(barcodeValue)}")
                }
            )
        }
        composable(
            route = "register?barcode={barcode}",
            arguments = listOf(
                navArgument("barcode") {
                    type = NavType.StringType
                    nullable = true
                    defaultValue = null
                }
            )
        ) { backStackEntry ->
            // SCR-MOB-004 (S5.7): the barcode argument is optional — the
            // scan Not Found flow passes it, the home quick action does not.
            val barcodeArg = backStackEntry.arguments?.getString("barcode").orEmpty()
            val registerFactory = remember(barcodeArg) {
                RegisterBarcodeViewModelFactory(
                    database.barcodeDao(),
                    database.barangDao(),
                    database.barcodeRegistrationRequestDao(),
                    session,
                    barcodeArg
                )
            }
            val registerViewModel: RegisterBarcodeViewModel =
                viewModel(factory = registerFactory)
            RegisterScreen(
                viewModel = registerViewModel,
                onBack = { navController.popBackStack() }
            )
        }
        composable("barcode_registry") {
            // SCR-MOB-005 (S5.8): searchable local cache list. Row action
            // targets `edit?barcodeId={id}` (§13.2, S5.9).
            val registryFactory = remember {
                BarcodeRegistryViewModelFactory(database.barcodeDao())
            }
            val registryViewModel: BarcodeRegistryViewModel =
                viewModel(factory = registryFactory)
            BarcodeRegistryScreen(
                viewModel = registryViewModel,
                onEditBarcode = { barcodeId ->
                    navController.navigate("edit?barcodeId=$barcodeId")
                },
                onBack = { navController.popBackStack() }
            )
        }
        composable(
            route = "edit?barcodeId={barcodeId}",
            arguments = listOf(
                navArgument("barcodeId") {
                    type = NavType.StringType
                    nullable = true
                    defaultValue = null
                }
            )
        ) { backStackEntry ->
            // SCR-MOB-006 (S5.9): correction target loaded by the cached
            // mapping's primary key (§13.2). A missing argument yields the
            // ViewModel's not-found state — no entry path is invented.
            val barcodeIdArg = backStackEntry.arguments?.getString("barcodeId").orEmpty()
            val editFactory = remember(barcodeIdArg) {
                EditBarcodeViewModelFactory(
                    database.barcodeDao(),
                    database.barangDao(),
                    database.barcodeRegistrationRequestDao(),
                    session,
                    barcodeIdArg
                )
            }
            val editViewModel: EditBarcodeViewModel =
                viewModel(factory = editFactory)
            EditBarcodeScreen(
                viewModel = editViewModel,
                onBack = { navController.popBackStack() }
            )
        }
        composable("synchronization") {
            // SCR-MOB-007 (S5.10): sync state display + Sync Now trigger
            // (§12.9, §14.5). Closes the S5.5 INFO-002 forward reference for
            // this route; `settings` remains owned by S5.11.
            val syncFactory = remember {
                val connectivityManager = context.getSystemService(
                    ConnectivityManager::class.java
                )
                SynchronizationViewModelFactory(
                    session,
                    database.barcodeRegistrationRequestDao(),
                    connectivityManager,
                    CLOUD_BASE_URL
                )
            }
            val syncViewModel: SynchronizationViewModel =
                viewModel(factory = syncFactory)
            SynchronizationScreen(
                viewModel = syncViewModel,
                onBack = { navController.popBackStack() }
            )
        }
    }
}
