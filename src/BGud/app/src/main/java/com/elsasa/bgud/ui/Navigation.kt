package com.elsasa.bgud.ui

import android.net.ConnectivityManager
import android.net.Uri
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
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
import com.elsasa.bgud.repository.ReturnOrderCaptureRepository
import com.elsasa.bgud.ui.screen.BarcodeRegistryScreen
import com.elsasa.bgud.ui.screen.CreateReturnOrderScreen
import com.elsasa.bgud.ui.screen.EditBarcodeScreen
import com.elsasa.bgud.ui.screen.EditReturnOrderScreen
import com.elsasa.bgud.ui.screen.HomeScreen
import com.elsasa.bgud.ui.screen.LoginScreen
import com.elsasa.bgud.ui.screen.RegisterScreen
import com.elsasa.bgud.ui.screen.ReturnOrderDetailScreen
import com.elsasa.bgud.ui.screen.ReturnOrderListScreen
import com.elsasa.bgud.ui.screen.ScanScreen
import com.elsasa.bgud.ui.screen.SettingsScreen
import com.elsasa.bgud.ui.screen.SynchronizationScreen
import com.elsasa.bgud.viewmodel.BarcodeRegistryViewModel
import com.elsasa.bgud.viewmodel.BarcodeRegistryViewModelFactory
import com.elsasa.bgud.viewmodel.CreateReturnOrderViewModel
import com.elsasa.bgud.viewmodel.CreateReturnOrderViewModelFactory
import com.elsasa.bgud.viewmodel.EditBarcodeViewModel
import com.elsasa.bgud.viewmodel.EditBarcodeViewModelFactory
import com.elsasa.bgud.viewmodel.EditReturnOrderViewModel
import com.elsasa.bgud.viewmodel.EditReturnOrderViewModelFactory
import com.elsasa.bgud.viewmodel.HomeViewModel
import com.elsasa.bgud.viewmodel.HomeViewModelFactory
import com.elsasa.bgud.viewmodel.LoginViewModel
import com.elsasa.bgud.viewmodel.LoginViewModelFactory
import com.elsasa.bgud.viewmodel.RegisterBarcodeViewModel
import com.elsasa.bgud.viewmodel.RegisterBarcodeViewModelFactory
import com.elsasa.bgud.viewmodel.ReturnOrderDetailViewModel
import com.elsasa.bgud.viewmodel.ReturnOrderDetailViewModelFactory
import com.elsasa.bgud.viewmodel.ReturnOrderListViewModel
import com.elsasa.bgud.viewmodel.ReturnOrderListViewModelFactory
import com.elsasa.bgud.viewmodel.ReturnOrderSyncViewModel
import com.elsasa.bgud.viewmodel.ReturnOrderSyncViewModelFactory
import com.elsasa.bgud.viewmodel.ScanViewModel
import com.elsasa.bgud.viewmodel.ScanViewModelFactory
import com.elsasa.bgud.viewmodel.SettingsViewModel
import com.elsasa.bgud.viewmodel.SettingsViewModelFactory
import com.elsasa.bgud.viewmodel.SynchronizationViewModel
import com.elsasa.bgud.viewmodel.SynchronizationViewModelFactory

/**
 * Cloud API base URL.
 *
 * Aligned with the BTrade3 network module (`src/BTrade3`
 * `NetworkModule.BASE_URL`), which targets the same backend host. BTrade3
 * declares its endpoints without the `api/` segment (e.g. `Brg/{serverId}`)
 * because its base already ends in `api/`; BGud's [BtradeApiService] declares
 * the full path (e.g. `api/Brg/{serverId}`), so the shared prefix is
 * `belajar-api/` here. This resolves to the same effective endpoints, e.g.
 * `http://dev.smart-ics.com:8089/belajar-api/api/Brg/{serverId}`.
 *
 * The single composition-root value is held here and passed down; no
 * environment URL is hardcoded in the network layer (§10). Never append a
 * tenant segment: the tenant is resolved server-side from
 * `X-Session-Location` (TD-03). Cleartext HTTP is permitted for this host via
 * `res/xml/network_security_config.xml`.
 */
private const val CLOUD_BASE_URL = "http://dev.smart-ics.com:8089/belajar-api/"

/**
 * Sentinel meaning "the session DataStore has not emitted yet".
 *
 * A plain `null` cannot be used for this: `null` is also the legitimate token
 * value when no session is stored (first install, logout). Conflating
 * "not loaded" with "no token" held the startup gate forever — the spinner bug.
 */
private const val SESSION_NOT_LOADED = "\u0000session-not-loaded"

/**
 * Navigation graph (Architecture §13.2, BGUD-RETURN-ORDER-NAV-001).
 *
 * ```text
 * login
 *   ├─ success ──▶ office resolution ──▶ master data synchronization ──▶ home
 *   └─ failure ──▶ login (error)
 *
 * home (work launcher, TD-001)
 *   ├─ New Return ──────────▶ return_order_create (TD-001)
 *   ├─ Return Orders ───────▶ return_order_list (TD-001)
 *   ├─ Sync Status Card ────▶ synchronization (TD-001, TD-008)
 *   ├─ Barcode Registry ────▶ barcode_registry (TD-001)
 *   └─ Settings ────────────▶ settings (TD-001)
 *
 * barcode_registry (SCR-MOB-005, TD-003)
 *   ├─ Register Barcode ────▶ register (no barcode argument, TD-003)
 *   ├─ Edit Barcode ────────▶ edit?barcodeId={id} (S5.9, SCR-MOB-006)
 *   └─ Kembali ─────────────▶ back
 *
 * register
 *   ├─ Save ────▶ local queue ── success message ──▶ back
 *   └─ Cancel ──▶ back
 *
 * edit (S5.9, SCR-MOB-006)
 *   ├─ Save ────▶ local queue ── success message ──▶ back
 *   └─ Cancel ──▶ back
 *
 * return_order_list (SCR-MOB-RO-001, TD-004, TD-006)
 *   ├─ Create ─────────────▶ return_order_create (FAB)
 *   ├─ Draft row ──────────▶ return_order_edit?returnOrderId={id} (TD-004)
 *   ├─ Synced row ─────────▶ return_order_detail?returnOrderId={id} (TD-004)
 *   └─ Back ───────────────▶ back
 *
 * return_order_create (SCR-MOB-RO-002, TD-007)
 *   ├─ Save ────▶ local write ──▶ back
 *   └─ Cancel ──▶ back
 *
 * return_order_edit (SCR-MOB-RO-004, TD-004, TD-005)
 *   ├─ Save ────▶ local write ──▶ back
 *   ├─ Delete (Draft) ─────▶ confirm ──▶ back (TD-005)
 *   └─ Cancel ──▶ back
 *
 * return_order_detail (SCR-MOB-RO-003, TD-004, TD-005)
 *   └─ Back ──▶ back
 *
 * synchronization (SCR-MOB-007)
 *   ├─ Sync Now ──▶ sync worker ──▶ synchronization (refreshed)
 *   └─ Back ──────▶ back
 *
 * settings (SCR-MOB-008)
 *   ├─ Logout ──▶ session cleared ──▶ login
 *   └─ Kembali ──▶ back
 *
 * scan (technical route retained for compatibility, TD-002)
 *   ├─ found ───────────────▶ scan (result region) ── Close ──▶ back
 *   ├─ not found + Register ─▶ register?barcode={value} (S5.7)
 *   └─ not found + Cancel ───▶ scan (re-armed Scanning)
 * ```
 *
 * Start destination: `login` when no valid local session exists, otherwise
 * `home` (TD-11: session validity = `google_email` present; no JWT or Cloud
 * token is consulted). The session gate re-reads the DataStore session, so a
 * Gudang change (which requires a new session, IR-09) returns here via
 * logout/change (SCR-MOB-008): the session is cleared and the operator
 * navigates to `login` with the back stack cleared. A Cloud 409 (session
 * ended, TD-13) also clears the session and lands here. All §13.2 routes
 * (`login`, `home`, `scan`, `register?barcode={value}`,
 * `barcode_registry`, `edit?barcodeId={id}`, `synchronization`,
 * `settings`) are wired; no forward references remain. The §13.1 Return
 * Order routes (`return_order_list`, `return_order_create`,
 * `return_order_detail?returnOrderId={id}`,
 * `return_order_edit?returnOrderId={id}`) are rewired by BGUD-RETURN-ORDER-NAV-001
 * (P3-S07): Home operates as a work launcher; `return_order_list` performs
 * status-conditional routing (Draft → Edit, Synced → Detail); Draft Detail is
 * unreachable; Barcode Registry initiates registration with no barcode parameter.
 */
@Composable
fun AppNavigation(
    navController: NavHostController,
    database: AppDatabase
) {
    val context = LocalContext.current
    val session = remember { SessionPreferencesDataSource(context) }
    val captureRepository = remember { ReturnOrderCaptureRepository(database, session) }
    val googleEmailOrLoading by session.googleEmail.collectAsState(initial = SESSION_NOT_LOADED)

    if (googleEmailOrLoading == SESSION_NOT_LOADED) {
        // Session not yet read — hold the gate instead of guessing.
        Box(
            modifier = Modifier.fillMaxSize(),
            contentAlignment = Alignment.Center
        ) {
            CircularProgressIndicator()
        }
        return
    }

    // Loaded: a blank/absent `google_email` means "no session" → login. Session
    // validity is the presence of `google_email` (TD-10/TD-11); a legacy
    // install holding only the removed `token` key therefore routes here too.
    // On first install and after logout this is the resolved state, not the
    // loading spinner.
    val hasSession = !googleEmailOrLoading.isNullOrBlank()

    // TD-11/TD-13: if a valid session ends while the app is running — logout,
    // Gudang change, or a Cloud 409 clearing the local session — return the
    // operator to sign-in. The logout path already navigates; this guard keeps
    // the two from double-navigating.
    var hadSession by rememberSaveable { mutableStateOf(hasSession) }
    LaunchedEffect(hasSession) {
        if (hasSession) {
            hadSession = true
        } else if (hadSession && navController.currentDestination?.route != "login") {
            hadSession = false
            navController.navigate("login") {
                popUpTo("home") { inclusive = true }
            }
        }
    }

    NavHost(
        navController = navController,
        startDestination = if (hasSession) "home" else "login"
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
                onNewReturn = { navController.navigate("return_order_create") },
                onReturnOrders = { navController.navigate("return_order_list") },
                onSyncStatusClick = { navController.navigate("synchronization") },
                onBarcodeRegistry = { navController.navigate("barcode_registry") },
                onSettings = { navController.navigate("settings") }
            )
        }
        composable("return_order_list") {
            // SCR-MOB-RO-001 (S4.6, TD-004, TD-006): searchable local list with
            // date grouping. Create → `return_order_create`; Draft row →
            // `return_order_edit?returnOrderId={id}`; Synced row →
            // `return_order_detail?returnOrderId={id}`.
            val listFactory = remember {
                ReturnOrderListViewModelFactory(
                    database.returnOrderDao(),
                    database.returnOrderItemDao()
                )
            }
            val listViewModel: ReturnOrderListViewModel =
                viewModel(factory = listFactory)
            ReturnOrderListScreen(
                viewModel = listViewModel,
                onEditDraft = { returnOrderId ->
                    navController.navigate(
                        "return_order_edit?returnOrderId=$returnOrderId"
                    )
                },
                onViewDetail = { returnOrderId ->
                    navController.navigate(
                        "return_order_detail?returnOrderId=$returnOrderId"
                    )
                },
                onCreate = { navController.navigate("return_order_create") },
                onBack = { navController.popBackStack() }
            )
        }
        composable("return_order_create") {
            // SCR-MOB-RO-002 (S4.7): offline capture; Save writes locally and
            // returns to the list (§13.1).
            val createFactory = remember {
                CreateReturnOrderViewModelFactory(
                    captureRepository,
                    database.customerDao(),
                    database.salesPersonDao(),
                    database.driverDao(),
                    database.barangDao(),
                    database.barcodeDao(),
                    session
                )
            }
            val createViewModel: CreateReturnOrderViewModel =
                viewModel(factory = createFactory)
            CreateReturnOrderScreen(
                viewModel = createViewModel,
                onSaved = { navController.popBackStack() },
                onCancel = { navController.popBackStack() }
            )
        }
        composable(
            route = "return_order_detail?returnOrderId={returnOrderId}",
            arguments = listOf(
                navArgument("returnOrderId") {
                    type = NavType.StringType
                    nullable = true
                    defaultValue = null
                }
            )
        ) { backStackEntry ->
            // SCR-MOB-RO-003 (S4.8, TD-004, TD-005): read-only detail for Synced orders.
            // A missing argument yields the ViewModel's not-found state — no entry path is
            // invented. Draft Detail is removed from the navigation graph (TD-004).
            // Delete action relocated to Edit screen (TD-005).
            val returnOrderIdArg =
                backStackEntry.arguments?.getString("returnOrderId").orEmpty()
            val detailFactory = remember(returnOrderIdArg) {
                ReturnOrderDetailViewModelFactory(
                    captureRepository,
                    returnOrderIdArg
                )
            }
            val detailViewModel: ReturnOrderDetailViewModel =
                viewModel(factory = detailFactory)
            ReturnOrderDetailScreen(
                viewModel = detailViewModel,
                onEdit = { returnOrderId ->
                    navController.navigate(
                        "return_order_edit?returnOrderId=$returnOrderId"
                    )
                },
                onDeleted = { navController.popBackStack() },
                onBack = { navController.popBackStack() }
            )
        }
        composable(
            route = "return_order_edit?returnOrderId={returnOrderId}",
            arguments = listOf(
                navArgument("returnOrderId") {
                    type = NavType.StringType
                    nullable = true
                    defaultValue = null
                }
            )
        ) { backStackEntry ->
            // SCR-MOB-RO-004 (S4.9, TD-004, TD-005): Draft-only modification. A non-`Draft`
            // order is refused by the ViewModel (`notEditable`, BR-018). Hosts relocated
            // Delete action for Draft orders (TD-005).
            val returnOrderIdArg =
                backStackEntry.arguments?.getString("returnOrderId").orEmpty()
            val editFactory = remember(returnOrderIdArg) {
                EditReturnOrderViewModelFactory(
                    captureRepository,
                    database.customerDao(),
                    database.salesPersonDao(),
                    database.driverDao(),
                    database.barangDao(),
                    database.barcodeDao(),
                    returnOrderIdArg
                )
            }
            val editViewModel: EditReturnOrderViewModel =
                viewModel(factory = editFactory)
            EditReturnOrderScreen(
                viewModel = editViewModel,
                onSaved = { navController.popBackStack() },
                onCancel = { navController.popBackStack() },
                onDeleted = { navController.popBackStack() }
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
            // SCR-MOB-005 (S5.8, TD-003): searchable local cache list. Row action
            // targets `edit?barcodeId={id}` (§13.2, S5.9). Register Barcode action
            // targets `register` with no barcode argument (TD-003).
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
                onRegisterBarcode = { navController.navigate("register") },
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
            // this route; `settings` is owned by S5.11 (SCR-MOB-008).
            // SCR-MOB-RO-005 (S4.10): the same surface shows Return Order
            // sync state and Sync Now additionally triggers the Return Order
            // worker (S4.5); the Return Order view model is constructed here
            // as a direct prerequisite of the extension (§19.1).
            val connectivityManager = context.getSystemService(
                ConnectivityManager::class.java
            )
            val syncFactory = remember {
                SynchronizationViewModelFactory(
                    session,
                    database.barcodeRegistrationRequestDao(),
                    connectivityManager,
                    CLOUD_BASE_URL
                )
            }
            val syncViewModel: SynchronizationViewModel =
                viewModel(factory = syncFactory)
            val returnOrderSyncFactory = remember {
                ReturnOrderSyncViewModelFactory(
                    session,
                    database.returnOrderDao(),
                    connectivityManager,
                    CLOUD_BASE_URL
                )
            }
            val returnOrderSyncViewModel: ReturnOrderSyncViewModel =
                viewModel(factory = returnOrderSyncFactory)
            SynchronizationScreen(
                viewModel = syncViewModel,
                returnOrderViewModel = returnOrderSyncViewModel,
                onBack = { navController.popBackStack() }
            )
        }
        composable("settings") {
            // SCR-MOB-008 (S5.11): settings surface + finalized §13.2
            // wiring. Closes the S5.5 INFO-002 forward reference for this
            // route — all §13.2 destinations now exist. Logout clears the
            // DataStore session so the start-destination gate returns to
            // `login` (IR-M8, IR-09); warehouse change is logout +
            // re-authentication.
            val settingsFactory = remember {
                SettingsViewModelFactory(session)
            }
            val settingsViewModel: SettingsViewModel =
                viewModel(factory = settingsFactory)
            SettingsScreen(
                viewModel = settingsViewModel,
                onSessionEnded = {
                    // Guard against a double navigation with the reactive gate
                    // above (both react to the same cleared session).
                    if (navController.currentDestination?.route != "login") {
                        navController.navigate("login") {
                            popUpTo("home") { inclusive = true }
                        }
                    }
                },
                onBack = { navController.popBackStack() }
            )
        }
    }
}
