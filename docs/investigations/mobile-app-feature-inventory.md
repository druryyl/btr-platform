# Mobile App Feature Inventory and UX Discovery

**Artifact type:** Investigation / discovery only  
**Scope:** `src/BTrade3` — the sole Android application in the BTR repository  
**Package:** `com.elsasa.btrade3`  
**App label:** BTrade3 (branded in UI as **Sales Order App**)  
**Status:** Discovery complete — no implementation recommendations

---

## Section 1 — Executive Summary

### Current Home Screen

After login, the app lands on **Sales Orders** (`faktur_list` / `OrderListScreen`). This screen shows a searchable list of local sales orders with status chips (In Progress, Ready, Sent), per-order actions, bulk selection/delete, and a multi-action floating action button (FAB).

The home screen also displays the **active server target** (Jogja or Magelang) in the top app bar subtitle.

### Current Navigation Pattern

The app uses a **single-activity, Jetpack Compose Navigation** model with **no bottom navigation**, **no drawer**, and **no tab bar**. Navigation is:

- **Stack-based** (back arrow on inner screens)
- **Hub-and-spoke** from the Sales Order List home
- **Overflow (triple-dot) menu** on the home screen for secondary features
- **Expandable FAB** on the home screen for New Order and Check-In
- **Per-card overflow menus** on order list items
- **SavedStateHandle** for picker screens (customer, sales person, barang) returning results to parent screens

There is **one Activity** (`MainActivity`), **zero Fragments**, and **no XML menu resources**.

### Number of Major Features Discovered

| Category | Count |
| -------- | ----- |
| Top-level screens (composable routes) | **15** |
| Major business feature areas | **6** (Sales, Visit, Customer, Sync, Reporting, Administration) |
| Distinct user-facing capabilities | **~18** |
| API integration endpoints consumed | **6** |

### Initial Observations About Navigation Complexity

1. **Sales-order-centric home** — The order list is appropriate for the primary workflow but functions as both dashboard and workspace; visit, customer, and sync features are secondary entry points.
2. **Feature concentration in overflow menu** — Five of six non-order features (Manage Customers, Check-In History, Sync Master Data, Sync Transaction, Order Summary) live behind the home screen triple-dot menu, plus Logout.
3. **Deep order-creation chain** — New Order → Order Entry → Customer Selection → back → Sales Selection → back → Item List → Add Item → Barang Selection requires **4–6 screen transitions** before the first line item is saved.
4. **Duplicate sync entry points** — Orders can be synced from the order card menu (single order) and from Sync Transaction (batch); check-ins sync only from Sync Transaction.
5. **No role-based UI** — All authenticated users see the same navigation; identity is a Google email stored locally, bridged to `SalesPerson` master data on the server.
6. **No push notifications or deep links** — The manifest exposes only the launcher intent; there is no FCM, approval inbox, or external URL routing.
7. **Features absent from mobile** (present elsewhere in BTR ecosystem) — Collection entry, approval/sign-request workflows, route planning, and portal dashboards are **not implemented** in BTrade3.

---

## Section 2 — Current Navigation Structure

### Complete Navigation Tree

```text
App Launch
└── Login (if no saved email)
    ├── Server Target selection (JOG / MGL)
    └── Google Sign-In
        └── Sales Orders [HOME — startDestination when logged in]
            │
            ├── [Top bar] Search (inline search mode)
            ├── [Top bar] Overflow Menu (⋮)
            │   ├── Manage Customers
            │   ├── Check-In History
            │   ├── Sync Master Data
            │   ├── Sync Transaction
            │   ├── Order Summary
            │   └── Logout → Login
            │
            ├── [FAB — expandable]
            │   ├── New Sales Order
            │   └── Check In
            │
            ├── [Order card tap] → Edit Sales Order
            ├── [Order card long-press] → Bulk selection mode
            │   └── [Selection bar] Bulk Delete
            └── [Order card ⋮ menu]
                ├── Edit
                ├── Delete
                └── Sync (only if status = Ready)

Sales Orders
├── New Sales Order / Edit Sales Order (faktur_entry)
│   ├── Select Customer → Customer Selection (fromMain=false)
│   │   └── [returns selection to Order Entry]
│   ├── Select Sales Person → Sales Selection
│   │   └── [returns selection to Order Entry]
│   ├── View / Edit Items → Order Items
│   │   ├── [FAB] Add Item → Add Item
│   │   │   └── Select Barang → Barang Selection
│   │   │       └── [single or bulk return to Add Item]
│   │   ├── [Item tap] Edit Item → Add Item (with itemId)
│   │   ├── [Item delete] Confirm dialog
│   │   └── [Top bar] Selesai → back to Sales Orders
│   ├── Finish Order (IN_PROGRESS → READY_TO_SYNC)
│   └── Reopen for Editing (READY_TO_SYNC → IN_PROGRESS)
│
├── Manage Customers (from overflow)
│   └── Customer Selection (fromMain=true)
│       ├── Search customers
│       ├── [Map icon] Open in Google Maps (external)
│       └── [Edit location icon] → Set Customer Location
│           └── [Nearby customers] → another Set Customer Location
│
├── Check-In History (from overflow)
│   ├── Date filter (prev/next day, date picker)
│   ├── [Card] Open in Google Maps (external)
│   └── [Card] Delete check-in
│
├── Sync Master Data (from overflow)
│   ├── Sync Barang Data
│   ├── Sync Customer Data
│   └── Sync Sales Person
│
├── Sync Transaction (from overflow)
│   ├── Select & Sync finished orders (batch)
│   └── Sync Check-In Data
│
└── Check In (from FAB)
    ├── GPS status / nearby customers (100m radius)
    ├── Select customer → Check In → back to Sales Orders
    └── Refresh Location

Order Summary (from overflow)
├── Date range filter
└── Refresh / reset to all data
```

### Bottom Navigation

**Not present.** No `BottomNavigation`, `NavigationBar`, or equivalent composable exists.

### Drawer Menu

**Not present.** No `ModalNavigationDrawer`, `NavigationDrawer`, or `Scaffold(drawerContent=…)` usage.

### Triple-Dot Menu (Home — `OverflowMenu`)

| Menu Item | Route | Icon |
| --------- | ----- | ---- |
| Manage Customers | `customer_selection?fromMain=true` | People |
| Check-In History | `check_in_history` | LocationOn |
| Sync Master Data | `sync` | Sync |
| Sync Transaction | `order_sync` | CloudUpload |
| Order Summary | `order_summary` | Analytics |
| Logout | `login` (clears session) | Logout |

**Source:** `OrderListScreen.kt` — `OverflowMenu` composable

### Triple-Dot Menu (Order Card — per order)

| Menu Item | Condition |
| --------- | --------- |
| Edit | Always (navigates to order entry) |
| Delete | Always |
| Sync | Only when `statusSync == READY_TO_SYNC` |

**Source:** `OrderCard.kt` — `SelectableModernOrderCard`

### Notification Deeplink

**Not implemented.**

- `AndroidManifest.xml` contains only `MAIN` / `LAUNCHER` intent filter
- No `FirebaseMessagingService`, notification channels, or custom URL schemes
- `google-services.json` is present; Firebase is used for **Google Sign-In** (`play-services-auth`), not push messaging

### Other Entry Points

| Entry Point | Target | Notes |
| ----------- | ------ | ----- |
| App launcher icon | Login or Sales Orders | Session persisted via SharedPreferences (`sales_order_prefs`) |
| Login success | Sales Orders | `popUpTo("login") { inclusive = true }` |
| Home FAB (collapsed → expanded) | New Order or Check-In | `MovableFloatingActionButton` with `isMultiAction = true` |
| Item List "Selesai" button | Sales Orders | Shortcut after adding items |
| Customer list map icon | Google Maps app | External intent via `MapUtils.openInGoogleMaps` |
| Order card map icon | Google Maps app | External, when customer coordinates exist |
| Check-In History map action | Google Maps app | External |

---

## Section 3 — Screen Inventory

Screens are Compose `@Composable` functions registered in `Navigation.kt`. There are no Fragment-based or legacy XML-layout screens.

### Authentication

| Screen | Composable | Purpose | Entry Point |
| ------ | ---------- | ------- | ----------- |
| Login | `LoginScreen` | Google Sign-In and server target selection (JOG/MGL) | App launch (cold start, no session) |

### Sales

| Screen | Composable | Purpose | Entry Point |
| ------ | ---------- | ------- | ----------- |
| Sales Orders (Home) | `OrderListScreen` | List, search, bulk delete, single-order sync, server indicator | Post-login default; Item List "Selesai" |
| New / Edit Sales Order | `OrderEntryScreen` | Customer, sales person, note, finish/reopen order, navigate to items | Home FAB; order card tap/edit |
| Order Items | `ItemListScreen` | Line items list, total, add/edit/delete items | Order Entry "View / Edit Items" |
| Add / Edit Item | `AddBarangScreen` | Quantity, bonus, discounts (disc1–4), barang selection | Item List FAB; item card tap |
| Barang Selection | `BarangSelectionScreen` | Search/filter products; single or bulk select | Add Item "Select Barang" |
| Sales Person Selection | `SalesSelectionScreen` | Pick salesman for order | Order Entry "Select Sales Person" |

### Customer

| Screen | Composable | Purpose | Entry Point |
| ------ | ---------- | ------- | ----------- |
| Customer Selection / Manage Customers | `CustomerSelectionScreen` | Search customers; select for order or browse from menu; location actions | Order Entry; Home overflow menu |
| Set Customer Location | `LocationCaptureScreen` | GPS capture, reverse geocode, save coordinates, nearby customers | Customer list "Manage Location" icon |

### Visit (Field Activity)

| Screen | Composable | Purpose | Entry Point |
| ------ | ---------- | ------- | ----------- |
| Check In | `CheckInScreen` | GPS-based visit check-in with nearby customer matching | Home FAB |
| Check-In History | `CheckInHistoryScreen` | Daily filtered history, map view, delete local check-ins | Home overflow menu |

### Sync

| Screen | Composable | Purpose | Entry Point |
| ------ | ---------- | ------- | ----------- |
| Data Synchronization (Master) | `SyncScreen` | Download Barang, Customer, Sales Person from server | Home overflow menu |
| Sync Transaction | `OrderSyncScreen` | Batch sync finished orders; sync draft check-ins | Home overflow menu |

### Reporting

| Screen | Composable | Purpose | Entry Point |
| ------ | ---------- | ------- | ----------- |
| Order Summary | `OrderSummaryScreen` | Local aggregated sales by date (orders, items, gross) | Home overflow menu |

---

## Section 4 — Feature Inventory

### Sales

| Feature | Description | Screens / Components |
| ------- | ----------- | -------------------- |
| Sales Order List | View all local orders with status, search, bulk operations | `OrderListScreen`, `SelectableModernOrderCard` |
| Create Sales Order | New order with auto-generated local ID (`FriendlyIdGenerator`) | `OrderEntryScreen`, Home FAB |
| Edit Sales Order | Modify customer, sales, note while `IN_PROGRESS` | `OrderEntryScreen` |
| Finish Order | Mark complete → `READY_TO_SYNC` (validates customer, sales, ≥1 item) | `OrderEntryScreen`, `OrderEntryViewModel` |
| Reopen Order | Return `READY_TO_SYNC` → `IN_PROGRESS` for edits | `OrderEntryScreen` |
| Delete Order | Single or bulk delete with confirmation | `OrderListScreen` |
| Order Line Items | CRUD for items on an order | `ItemListScreen`, `AddBarangScreen` |
| Product (Barang) Search | Multi-word search, recent searches, category display | `BarangSelectionScreen` |
| Bulk Product Selection | Select multiple barangs with matching bulk profile | `BarangSelectionScreen`, `BulkInputProfile` |
| Item Pricing | Unit price, qty besar/kecil, bonus qty, 4 discount tiers | `AddBarangScreen` |
| Sales Person Assignment | Link order to `SalesPerson` master record | `SalesSelectionScreen` |
| Order Note | Free-text note for admin | `OrderEntryScreen` |
| Single-Order Sync | Send one ready order from list card menu | `OrderListScreen`, `OrderListViewModel` |
| Order Status Lifecycle | `IN_PROGRESS` → `READY_TO_SYNC` → `SENT` | `OrderSyncStatus` |
| Open Customer on Map | Launch Google Maps from order card | `MapUtils` (external) |

### Visit

| Feature | Description | Screens / Components |
| ------- | ----------- | -------------------- |
| GPS Check-In | Record visit at customer location with accuracy | `CheckInScreen`, `CheckInViewModel` |
| Nearby Customer Detection | Suggest customers within ~100m of current GPS | `CheckInScreen` |
| Check-In History | Browse by date, view on map, delete unsynced records | `CheckInHistoryScreen` |
| Check-In Sync | Upload draft check-ins to server | `OrderSyncScreen` |

### Customer

| Feature | Description | Screens / Components |
| ------- | ----------- | -------------------- |
| Customer Search | Multi-word search with recent search history | `CustomerSelectionScreen` |
| Customer Selection for Order | Return customer + coordinates to order entry | `CustomerSelectionScreen` (`fromMain=false`) |
| Customer Browse (Menu) | View/search customers without selecting for order | `CustomerSelectionScreen` (`fromMain=true`) |
| Customer Location Capture | Set/update GPS coordinates on device, sync to server | `LocationCaptureScreen` |
| Customer Location Sync | PATCH updated coordinates to API | `CustomerSyncRepository` |
| Open Customer in Maps | External navigation to stored coordinates | `MapUtils` |

### Sync

| Feature | Description | Screens / Components |
| ------- | ----------- | -------------------- |
| Sync Barang (Products) | Download product master to local Room DB | `SyncScreen` |
| Sync Customers | Download customer master | `SyncScreen` |
| Sync Sales Persons | Download salesman master | `SyncScreen` |
| Batch Order Sync | Select and upload multiple `READY_TO_SYNC` orders | `OrderSyncScreen` |
| Check-In Data Sync | Upload local draft check-ins | `OrderSyncScreen` |
| Server Target Selection | Choose JOG (Jogja) or MGL (Magelang) backend partition | `LoginScreen`, `ServerHelper` |

### Reporting

| Feature | Description | Screens / Components |
| ------- | ----------- | -------------------- |
| Order Summary (Local) | Daily aggregates: order count, item count, gross sales by user email | `OrderSummaryScreen` |
| Date Range Filter | Manual YYYY-MM-DD range on summary | `OrderSummaryScreen` |

### Administration

| Feature | Description | Screens / Components |
| ------- | ----------- | -------------------- |
| Google Sign-In | Authenticate via Google account email | `LoginScreen`, `GoogleSignInHelper` |
| Session Persistence | Remember login email in SharedPreferences | `Navigation.kt` (`sales_order_prefs`) |
| Server Selection | Persist server target in DataStore | `ServerPreferencesDataSource`, `LoginScreen` |
| Logout | Clear session, return to login | Home overflow menu |

### Not Present in Mobile App

The following domains exist in the broader BTR platform but have **no mobile implementation** in BTrade3:

- Collection / Piutang entry
- Approval inbox / sign-request workflows
- Route planning / visit schedule
- Portal dashboards or alerts
- Faktur (invoice) viewing beyond sync status
- User profile or settings screen (beyond login-time server pick)
- Role-based or permission-gated menus
- Feature flags or remote configuration
- Offline conflict resolution UI

---

## Section 5 — User Workflow Analysis

BTrade3 does not implement distinct UI personas. All users share one navigation tree. Workflows below are inferred from feature design and BTR domain knowledge (`SalesPerson.Email` as the mobile identity bridge).

### Field Salesman — Daily Order Capture

```text
Login (Google + server)
  → [Optional] Sync Master Data (Barang, Customer, Sales Person)
  → Sales Orders (Home)
  → FAB: New Sales Order
  → Select Customer
  → Select Sales Person
  → View / Edit Items
  → Add Item → Select Barang → Enter qty/discounts → Save
  → (repeat items)
  → Selesai → back to Order Entry
  → Finish Order
  → [End of day] Sync Transaction → select orders → Sync Selected
```

### Field Salesman — Visit Check-In

```text
Login
  → Sales Orders (Home)
  → FAB: Check In
  → Wait for GPS lock
  → Select nearby customer (or from list)
  → Check In
  → [Later] Sync Transaction → Sync Check-In Data
  → [Optional] Check-In History → review by date
```

### Field Salesman — Customer Location Maintenance

```text
Login
  → Overflow: Manage Customers
  → Search customer
  → Edit Location icon
  → Capture GPS → Save Location
  → (coordinates sync to server on save)
```

### Field Salesman — Quick Single-Order Upload

```text
Login
  → Sales Orders
  → Finish order(s) beforehand
  → Order card ⋮ → Sync
  → Confirm dialog → sent to office
```

### Field Salesman — Performance Review (Local)

```text
Login
  → Overflow: Order Summary
  → [Optional] filter date range
  → Review daily totals (local DB only)
```

### Supervisor / Collector / Approver

**No dedicated workflows.** The app has no approval inbox, collection screens, team views, or role-gated features. Supervisors using BTrade3 today would follow the same salesman workflows with their own Google account (must match a `SalesPerson.Email` in master data per BTR business rules).

### First-Time / Setup Workflow

```text
Install app
  → Login: select server (JOG/MGL)
  → Google Sign-In
  → Sync Master Data (Barang → Customer → Sales Person)
  → Begin order or check-in work
```

---

## Section 6 — Navigation Usage Importance

Classification is based on: post-login default destination, FAB prominence, workflow dependency chains, sync/API coupling, and code surface area.

### Primary Features

| Feature | Rationale |
| ------- | --------- |
| Sales Order List (Home) | Default `startDestination` after login; central hub for all navigation |
| Create / Edit Sales Order | Home FAB primary action; core business purpose of the app |
| Order Line Items (Add/Edit) | Required step in every order; deepest workflow chain |
| Barang Selection | Required for every line item; search is heavily implemented |
| Customer Selection (for orders) | Mandatory before finishing an order |
| Finish Order | Gates sync eligibility; explicit status transition |
| Sync Transaction (orders) | Only path to send orders to office (besides single-card sync) |

### Secondary Features

| Feature | Rationale |
| ------- | --------- |
| Check-In | Home FAB secondary action; important for visit tracking but separate from order flow |
| Sync Master Data | Prerequisite for offline product/customer lookup; not daily for established users |
| Sales Person Selection | Required for order completion but one-time per order |
| Single-Order Sync from card | Convenience shortcut; duplicates batch sync |
| Customer Location Capture | Supports map/check-in quality; accessed from customer list, not home |
| Order Summary | Useful self-review; local-only; buried in overflow menu |

### Supporting Features

| Feature | Rationale |
| ------- | --------- |
| Check-In History | Review/audit; post-hoc; overflow menu only |
| Check-In Sync | Bundled on Sync Transaction screen, not standalone |
| Bulk order delete / selection | Power-user maintenance |
| Order search | Filter on home list |
| Open in Google Maps | External shortcut from cards |
| Bulk barang selection | Efficiency for multi-SKU entry; optional mode |
| Reopen for Editing | Edge case after premature finish |

### Administrative Features

| Feature | Rationale |
| ------- | --------- |
| Login / Google Sign-In | Gate; not part of daily loop after first session |
| Server Target selection | Infrastructure; chosen at login |
| Logout | Session management; overflow menu |
| Sync Barang / Customer / Sales Person (individual buttons) | Maintenance; periodic |

---

## Section 7 — Notification and Deeplink Analysis

### Notification Types

**None implemented.**

- No `FirebaseMessaging` dependency in `build.gradle.kts`
- No notification permission in manifest
- No `NotificationChannel`, `NotificationCompat`, or messaging service classes

### Deeplink Targets

**None implemented.**

- No `android:scheme`, `android:host`, or App Links intent filters
- No `navController.handleDeepLink()` or `NavDeepLink` definitions in `Navigation.kt`
- No handling of `intent.extras` in `MainActivity.onCreate`

### Screens Opened Directly from Notifications

**N/A** — push notifications are not implemented.

### Approval-Related Navigation

**Not present.** No sign-request, approval inbox, or supervisor action screens exist in the codebase.

### Sign-Request Navigation

**Not present.**

### Special Shortcut Behavior

| Behavior | Mechanism |
| -------- | --------- |
| Session restore | If `user_email` exists in SharedPreferences, skip login → `faktur_list` |
| Picker result passing | `NavController.previousBackStackEntry?.savedStateHandle` for customer, sales, barang |
| Item List "Selesai" | Navigates directly to `faktur_list` without finishing order |
| External maps | `MapUtils.openInGoogleMaps` — leaves app |
| Movable FAB | Draggable on screen; does not change routes |

---

## Section 8 — UX Redesign Inputs

Discovery only — no redesign proposed.

### Navigation Pain Points

| Pain Point | Evidence |
| ---------- | -------- |
| **Most non-order features hidden in overflow menu** | 5 features + logout behind single ⋮ on home; no visual affordance for Check-In History, Sync, or Summary |
| **No persistent navigation chrome** | Users must return to home to reach any secondary feature; no bottom nav or drawer |
| **Deep order creation path** | Home → Order Entry → Customer → back → Sales → back → Items → Add → Barang = many steps before value |
| **Check-In competes with New Order on same FAB** | Expandable FAB requires two taps; Check-In is co-equal but structurally secondary in business priority |
| **"Manage Customers" from menu is incomplete** | `fromMain=true` click on customer does nothing (comment: "stay on customer list"); no customer detail screen |
| **Duplicate sync paths** | Per-order sync on card vs. Sync Transaction batch — may confuse users about when to use which |
| **Sync Master Data UI inconsistency** | Customer/Sales Person sync buttons exist but a "Future Sync Options" card still lists them as upcoming |
| **No end-of-day hub** | Sync Transaction and Order Summary are separate overflow items; no unified "wrap up" flow |
| **Item List "Selesai" skips Finish Order** | Returns to home without prompting to finish order; user may forget to mark ready for sync |
| **Server shown on home but not changeable in-app** | Must logout and re-login to change server target |
| **No visit-to-order bridge** | Check-in and order creation are disconnected (no "create order for checked-in customer") |
| **No route guidance** | Per BTR domain analysis, BTrade3 does not consume sales route data |

### Home Screen Candidates

Features that appear important enough for **direct access from a future home screen** (candidates only):

| Candidate | Why |
| --------- | --- |
| Sales Orders / Order List | Already home; remains the anchor |
| New Sales Order | Primary FAB action; highest-frequency create flow |
| Check-In | Second FAB action; daily field activity |
| Sync Transaction | End-of-day critical path; currently buried |
| Manage Customers | Supports location maintenance and lookup; overflow only today |
| Order Summary | Self-service performance view; useful dashboard widget |
| Sync Master Data | Needed after install or master changes; could be onboarding prompt rather than permanent home tile |

**Lower priority for home direct access:** Check-In History, Logout, Server settings, single-order card actions.

---

## Section 9 — Technical Notes

### Navigation Framework

| Item | Detail |
| ---- | ------ |
| Framework | Jetpack Navigation Compose (`androidx.navigation.compose`) |
| Entry | `MainActivity` → `SalesOrderApp()` → `AppNavigation()` |
| Nav controller | `rememberNavController()` in `MainActivity.kt` |
| Route definitions | Inline `composable(...)` blocks in `Navigation.kt` |
| Arguments | `navArgument` with `NavType.StringType`, `NavType.BoolType` |
| State passing | `SavedStateHandle` on back stack entries |
| Start destination | `faktur_list` if logged in, else `login` |

### Activities

| Activity | File | Role |
| -------- | ---- | ---- |
| `MainActivity` | `MainActivity.kt` | Single activity; hosts entire Compose tree |

### Fragments

**None.** The app is 100% Jetpack Compose.

### Route Definitions (Complete)

| Route | Screen | Parameters |
| ----- | ------ | ---------- |
| `login` | LoginScreen | — |
| `faktur_list` | OrderListScreen | — |
| `faktur_entry/{orderId}/{statusSync}` | OrderEntryScreen | orderId, statusSync |
| `customer_selection?fromMain={fromMain}` | CustomerSelectionScreen | fromMain (bool, default false) |
| `sales_selection` | SalesSelectionScreen | — |
| `item_list/{orderId}/{statusSync}` | ItemListScreen | orderId, statusSync |
| `add_barang/{orderId}?itemId={itemId}` | AddBarangScreen | orderId, itemId (optional) |
| `barang_selection` | BarangSelectionScreen | — |
| `sync` | SyncScreen | — |
| `order_sync` | OrderSyncScreen | — |
| `order_summary` | OrderSummaryScreen | — |
| `location_capture/{customerId}/{customerName}/{customerAddress}/{customerCity}` | LocationCaptureScreen | 4 string args |
| `check_in` | CheckInScreen | — |
| `check_in_history` | CheckInHistoryScreen | — |

### Menu Resource Files

**None.** All menus are Compose `DropdownMenu` / `DropdownMenuItem` defined in Kotlin:

- `OrderListScreen.kt` — `OverflowMenu`
- `OrderCard.kt` — per-order card menu
- `LoginScreen.kt` — server dropdown (`ExposedDropdownMenuBox`)

### Deeplink Implementation Locations

**Not applicable** — no deeplink implementation exists.

### Local Persistence

| Store | Purpose | File |
| ----- | ------- | ---- |
| Room (`sales_order_database`) | Orders, items, barang, customers, sales persons, check-ins | `AppDatabase.kt` (v24) |
| SharedPreferences (`sales_order_prefs`) | User email session | `Navigation.kt` |
| DataStore | Server target (JOG/MGL) | `ServerPreferencesDataSource` |
| SharedPreferences (`faktur_sequence`) | Local order ID sequence | `FriendlyIdGenerator.kt` |
| SharedPreferences (per entity) | Recent searches | `RecentSearchManager.kt` |

### Network / API

| Item | Detail |
| ---- | ------ |
| Client | Retrofit 2 + OkHttp | `NetworkModule.kt` |
| Base URL | `http://dev.smart-ics.com:8089/belajar-api/api/` |
| Server routing | `serverId` path param from DataStore (`JOG`, `MGL`) |
| Endpoints | `GET Brg/{serverId}`, `GET Customer/{serverId}`, `GET SalesPerson/{serverId}`, `POST Order`, `PATCH Customer`, `POST CheckIn` |
| API interface | `ApiService.kt` |

### Authentication

| Item | Detail |
| ---- | ------ |
| Provider | Google Sign-In (`play-services-auth`) |
| Identity stored | Google account email |
| Authorization model | No in-app RBAC; server presumably validates email against `SalesPerson` |

### Key UI Component Files

| Component | File |
| --------- | ---- |
| Order card | `ui/component/OrderCard.kt` |
| Search bar | `ui/component/SearchBar.kt` |
| Multi-action FAB | `util/MovableFloatingActionButton.kt` |
| Maps helper | `util/MapUtils.kt` |
| Location utilities | `util/LocationHelper.kt`, `LocationCaptureViewModel.kt` |

### Feature Flags

**None found.** No Remote Config, BuildConfig feature toggles, or conditional navigation based on flags.

### Role-Based Menus

**None.** Navigation graph is static for all users.

---

## Appendix — Designer Readiness Checklist

After reading this document, a UX designer can answer:

| Question | Answer |
| -------- | ------ |
| What features exist? | Sales orders (full lifecycle), visit check-in, customer/location management, master + transaction sync, local order summary, Google login |
| What are the main user workflows? | Order capture + sync; field check-in + sync; customer location maintenance; periodic master data sync |
| Which features are most important? | Sales order list/create/items/sync are primary; check-in and master sync are secondary; history/summary/admin are supporting |
| How do users navigate today? | Single home (order list) + overflow menu + expandable FAB + deep stack navigation for order building |
| What is needed before redesigning Home? | Decide whether home stays order-centric or becomes a dashboard; prioritize buried features (sync, check-in, customers); resolve incomplete "Manage Customers" behavior; consider missing domains (collection, approval, routes) as out of scope or future phases |

---

*Generated from static analysis of `src/BTrade3` on 2026-06-12. No runtime testing performed.*
