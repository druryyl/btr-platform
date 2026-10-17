---
Title: Warehouse Desktop Return Order & Barcode Management (.NET 4.7.1) — Architecture
Code: BTR-RETUR-DESKTOP-001
Artifact: ARCHITECTURE
Version: 1.2
LastUpdated: 2026-10-17
---

# 1. Overview

This architecture defines the technical realization of the Warehouse Desktop Return Order Capture and Barcode Management capability (`BTR-RETUR-DESKTOP-001`), replacing the rejected Android mobile client (`src/BGud`).

Originating ISSUE: `BTR-RETUR-GUDANG-DESKTOP-001` (`docs/issues/BTR-RETUR-GUDANG-DESKTOP-ISSUE.md`)  
Realized FEATURE: `docs/features/return-order-desktop/feature.md` (`BTR-RETUR-DESKTOP-001`)

The architecture establishes a dedicated, lightweight Windows Forms application running on the onsite warehouse PC within the .NET Framework 4.7.1 runtime environment, integrating with a dedicated USB barcode scanner, persisting data locally in SQL Server (`btrgd`), and synchronizing upstream via the existing Cloud staging relay (`j06-pkl-btrade-api`).

Version 1.2 aligns the desktop client's Cloud session-context contract with the actor
contract introduced by the BGud Google sign-in change in `j06-pkl-btrade-api`: the
client now carries the operator's registered **Google email** as `X-Session-Actor`
(resolved server-side to the authoritative BTR user) instead of a free-text operator
name, and validates that email through `POST api/session/resolve`. The operator name
is retained for on-screen display and local record attribution only. See TD-007 and
TD-012.

---

# 2. Architectural Basis

## Business Context

- **DOMAIN:** `docs/work/return-order/RETURN-ORDER-DOMAIN.md` and `docs/work/barcode-registry/BARCODE-REGISTRY-DOMAIN.md` define the business concepts:
  - Return Order as a warehouse physical intake document (not financial accounting).
  - Return classification into `BAGUS` and `RUSAK`.
  - Barcode normalization and catalog lookup.
  - On-the-spot barcode registration for unmapped items.
- **FEATURE:** `docs/features/return-order-desktop/feature.md` defines:
  - Hands-free USB barcode scanning workflow.
  - Active item quantity strip and manual lookup fallback (`F2`).
  - Workstation session scoping and local offline persistence.

## Analysis Input

- **FEASIBILITY-ASSESSMENT:** `docs/work/j09-btr-retur-gudang/BTR-RETUR-GUDANG-DESKTOP-FEASIBILITY-ASSESSMENT.md` provides approved decisions:
  - Standalone layered solution in `src/j09-btr-retur-gudang` (GAP-001).
  - Local persistence in `btrgd` database on `(local)` via Dapper (GAP-002).
  - USB scanner keyboard-wedge listener with 50ms burst threshold (GAP-003, OQ-002).
  - Dual-trigger synchronization engine with Cloud API session headers (GAP-004, OQ-001).
  - Native Windows multimedia beeps for audio feedback (GAP-005).
  - Registry workstation configuration and shift operator switcher (GAP-006).

- **BUG-INVESTIGATION:** `docs/investigations/return-order-upload-409-investigation.md` provides the approved correction for the Return Order upload `HTTP 409` refusal ("Google account is not registered or is not active"): the workstation operator identity carried as `X-Session-Actor` must be the registered Google email, not the operator display name, and is validated via `POST api/session/resolve` (realized by TD-007, TD-012).

```text
DOMAIN (Return Order + Barcode Registry)
                +
FEATURE (BTR-RETUR-DESKTOP-001)
                +
FEASIBILITY-ASSESSMENT (BTR-RETUR-GUDANG-DESKTOP-001)
                +
BUG-INVESTIGATION (return-order-upload-409)
                ↓
           ARCHITECTURE
```

---

# 3. Scope

## Included
- Scaffolding a standalone solution at `src/j09-btr-retur-gudang` targeting **.NET Framework 4.7.1**.
- Clean layered architecture:
  - `BtrReturGudang.Domain`: Core business aggregates, value objects, and status enums.
  - `BtrReturGudang.AppTier`: Repository interfaces, DTOs, and application orchestration services.
  - `BtrReturGudang.Infrastructure`: Dapper DALs, Cloud API client, Audio helper, and Registry configuration.
  - `BtrReturGudang.Winform`: Modern single-window interface with docked sidebar navigation, smart scanner buffer hook, and approved Screen Design Inventory:
    - `MainForm` (SCR-01): Application shell hosting docked sidebar, status bar, and global scanner hook.
    - `ReturnOrderEntryView` (SCR-02): Primary operational intake view with customer autocomplete, scanner focus banner, quantity strip (`Qty Bagus`/`Qty Rusak`), items DataGridView, and action bar.
    - `ManualItemSearchDialog` (SCR-03): Modal product lookup dialog on `F2` fallback.
    - `QuickBarcodeRegistrationDialog` (SCR-04): Modal on-the-spot unknown barcode linking dialog.
    - `ReturnOrderListView` (SCR-05): History and status inspection view with post-sync read-only locking.
    - `BarcodeRegistryView` (SCR-06): Local barcode catalog and registration queue management view.
    - `SettingsView` (SCR-07): Workstation configuration, local DB tester, shift operator switcher, and audio beeper toggle.
  - `BtrReturGudang.SqlDb`: SQL DDL scripts for warehouse tables and indexes in `btrgd`.
- Hardware integration: Global keyboard-wedge listener for USB HID barcode scanners.
- Ergonomic intake flow: Scanned item populates quantity strip (`Qty Bagus` / `Qty Rusak`) with auto-advance and `F2` product search fallback.
- Barcode registration: On-the-spot unknown barcode linking with immediate local use in the active order and queued upload.
- Persistence: SQL Server `(local)\btrgd` with connection pooling, transactional scopes, and indexed queries.
- Cloud relay integration: RestSharp HTTP client targeting `j06-pkl-btrade-api` with `X-Session-Location` (`WarehouseCode`) and `X-Session-Actor` (registered Google email) headers.
- Operator session identity: workstation-configured operator Google email, validated at sign-in via `POST api/session/resolve` (anonymous account resolution), with the operator name retained for display and local attribution.
- Synchronization engine: Dual triggers (save-time upload + 5-minute background periodic timer + manual sync button).
- Packaging: Portable folder distribution (xcopy / ZIP) without installation wizards.

## Excluded
- Physical paper receipt / slip printing (confirmed out of scope; digital capture only).
- Inventory valuation, financial pricing, and accounting posting (remains in Main Office BTR Desktop `ReturJual`).
- Modifications to existing Cloud API endpoints in `src/j06-pkl-btrade-api`.
- Modifications to existing Packing Order codebase in `src/j07-btr-gudang`.

---

# 4. Technical Decisions

### TD-001: Runtime & UI Technology Baseline
- **Decision:** Target **.NET Framework 4.7.1** using **Windows Forms (WinForms)**.
- **Realization:** Solves the workstation hardware and older Windows OS limitations. Uses GDI+ rendering with double-buffering for flicker-free grids, native control responsiveness, and sub-second startup times without GPU overhead.
- *Traceable to:* GAP-001, ASM-001, `src/j07-btr-gudang` precedent.

### TD-002: Solution Architecture & Project Layering
- **Decision:** Scaffold `src/j09-btr-retur-gudang` into 5 decoupled projects:
  ```text
  src/j09-btr-retur-gudang/
  ├── BtrReturGudang.Domain/          # Core Domain entities, enums, value objects
  ├── BtrReturGudang.AppTier/         # Application services, repository contracts, DTOs
  ├── BtrReturGudang.Infrastructure/  # Dapper DALs, RestSharp API client, Registry, Audio
  ├── BtrReturGudang.Winform/         # WinForms UI, Sidebar navigation, Scanner hook
  └── BtrReturGudang.SqlDb/           # SQL migration/DDL scripts for btrgd
  ```
- **Realization:** Isolates business logic from WinForms UI and Dapper persistence, matching the architectural conventions established across BTR (`j05`, `j07`).
- *Traceable to:* GAP-001.

### TD-003: Local Persistence Engine & Dapper DAL
- **Decision:** Persist all operational records in the local SQL Server instance `(local)\btrgd` using Dapper and short-lived ADO.NET connections.
- **Realization:** Database connection string is managed via `ConnStringHelper` (`Server=(local);Database=btrgd;User Id=btrGudangLogin;Password=btrGudang123!;Max Pool Size=20;`). No foreign keys are declared on warehouse tables (IR-RO-11), ensuring zero lock contention and application-enforced integrity.
- *Traceable to:* GAP-002, OQ-003, IR-RO-11.

### TD-004: Primary Keys & Entity Identification
- **Decision:** Generate ULID strings (`VARCHAR(26)`) for `ReturnOrderId` and `RegistrationRequestId`.
- **Realization:** Consumes the established `Ulid` library (`Ulid.NewUlid().ToString()`). Line items use composite keys `(ReturnOrderId, NoUrut)` where `NoUrut` is a 1-based sequential integer.
- *Traceable to:* IR-RO-01, IR-RO-02, IR-RO-10.

### TD-005: USB Barcode Scanner Wedge Interception
- **Decision:** Capture USB scanner input via a form-level keyboard filter (`KeyPreview = true` and `IMessageFilter`).
- **Realization:** A scanner listener buffers incoming characters. When consecutive keystrokes arrive with an inter-character interval <= 50ms and terminate with `\r` or `\n`, the buffered string is treated as a barcode scan. The keystrokes are suppressed from active child controls, and the parsed barcode is dispatched to the scan handler. Keystroke streams with > 50ms intervals pass through as normal human typing.
- *Traceable to:* GAP-003, OQ-002.

### TD-006: Barcode Normalization
- **Decision:** Implement VO-01 normalization for all scanned and typed barcodes:
  1. Strip CR (`\r`), LF (`\n`), and TAB (`\t`).
  2. Trim leading and trailing whitespace.
  3. Strictly preserve leading zeros (never convert to numeric).
  4. Lookup comparison key = `UPPER(normalized_value)`.
- *Traceable to:* VO-01, `BGud/util/BarcodeNormalization.kt`.

### TD-007: Cloud API Relay Client & Session Headers
- **Decision:** Communicate with `j06-pkl-btrade-api` using RestSharp and System.Text.Json, attaching session headers:
  - `X-Session-Location`: Configured `WarehouseCode` (resolved server-side through `BTR_WarehouseMapping` to a tenant `ServerId`).
  - `X-Session-Actor`: Active operator's **registered Google email** (`RegistryHelper.LastOperatorEmail`), which the Cloud resolves through `BTRADE_User.Email` to the authoritative BTR `UserId`.
- **Realization:** The actor input aligns with the Cloud anonymous session-context contract introduced by the BGud Google sign-in change (`SessionContextResolver`; `POST api/return-order` and `POST api/barcode-registration` read the header through `SessionContextResolver.ResolveAccount`). A free-text operator name is never sent as the actor. The operator display name (`RegistryHelper.LastOperatorName`) is retained for UI and local record attribution only and does not participate in Cloud identity resolution. An unresolvable actor is rejected by the Cloud as `HTTP 409 Conflict`, which the client surfaces as an explicit session refusal rather than a silent upload failure.
- *Traceable to:* BUG-INVESTIGATION (return-order-upload-409), OQ-001, `BGud/network/SessionContextInterceptor.kt`.

### TD-012: Operator Google-Email Configuration & Session Resolution
- **Decision:** The workstation operator identity used for Cloud attribution is a registered Google email, configured on the workstation and validated at sign-in before warehouse work begins:
  1. **Configuration:** The operator Google email is persisted in the workstation registry (`HKCU\Software\BTRApp\ReturGudang\LastOperatorEmail`) alongside the operator display name, entered/updated through the shift operator switcher (`SCR-07` / operator dialog).
  2. **Resolution:** Validation calls the Cloud anonymous endpoint `POST api/session/resolve` with `{ "Email": <operator email> }` and no session headers. A `200` response yields the authoritative BTR `UserId`/`UserName`/`RoleId` and the warehouse mapping; a `400` response (unmapped or inactive Google account) is surfaced to the operator verbatim as a sign-in refusal.
  3. **Gate:** When the configured email is unresolvable, the operator is warned and upload operations are pre-empted with the refusal message; uploads are not attempted against an unresolved identity.
- **Realization:** The resolution result is consumed by the presentation layer (`SettingsView` "verify operator", startup/operator-change validation). The client performs no Google OAuth flow and stores no BTR password; the workstation carries a pre-registered operator email only.
- *Traceable to:* BUG-INVESTIGATION (return-order-upload-409), `docs/features/bgud-operator-signin/feature.md`, `DEPLOYMENT-GUIDELINE.md` §5.1.1.

### TD-008: Dual-Trigger Synchronization Engine
- **Decision:** Implement a non-blocking background synchronization engine with dual triggers:
  1. **Immediate Save Trigger:** When saving via "Simpan & Upload", the document is committed to local SQL as `Draft`, and an asynchronous upload task is dispatched. Upon HTTP 200/201, status transitions to `Synced`.
  2. **Periodic Timer:** A `System.Windows.Forms.Timer` running every 5 minutes in `MainForm` scans for un-synced Return Orders and pending Barcode Registrations, while pulling updated reference deltas.
  3. **Manual Trigger:** "Sync Now" button on the sidebar with live online/offline indicator dot.
- *Traceable to:* GAP-004.

### TD-009: Audio Cues Without External Media
- **Decision:** Generate distinct audio feedback using native Windows multimedia beeps:
  - Success Scan: `Console.Beep(2000, 80)` (high-pitch short click).
  - Error / Unrecognized Scan: `Console.Beep(400, 200)` (lower buzz tone).
  - Settings provide a simple toggle (`EnableSoundFeedback = true/false`).
- *Traceable to:* GAP-005.

### TD-010: Post-Sync Immutability & Document Locking
- **Decision:** Enforce strict write-protection: Once a Return Order is marked `StatusSync = 'Synced'`, the desktop application blocks Edit and Delete actions, presenting the document in read-only inspection mode.
- *Traceable to:* P-10, ADR-RO-006, GAP-014.

### TD-011: Screen Inventory Architecture & Ergonomic WinForms Navigation Model
- **Decision:** Realize the desktop user interface as a single-window shell (`MainForm`) with docked sidebar navigation hosting dynamic views (`UserControl` with `Dock = DockStyle.Fill`), complemented by modal dialogs (`Form.ShowDialog`) strictly for short-lived workflows (`F2` search, unknown barcode registration).
- **Realization:**
  1. **Single-Window Hosting:** Navigation between views (`ReturnOrderEntryView`, `ReturnOrderListView`, `BarcodeRegistryView`, `SettingsView`) switches the active control in `MainForm.pnlContentHost`, maintaining background scanner listener continuity and avoiding MDI window sprawl.
  2. **Ergonomic Hands-Free Intake:** `ReturnOrderEntryView` captures USB barcode scans without requiring a focused textbox. Upon scan, it updates the Scanned Item Banner and transfers focus into the Quantity Strip (`numQtyBagus` / `numQtyRusak`). Hitting `Enter` commits the line item to the double-buffered `DataGridView` and immediately resets the scanner buffer for the next scan.
  3. **Visual Classification:** Line items and summary footers strictly differentiate `BAGUS` (sellable, emerald/green accent) and `RUSAK` (damaged, rose/crimson accent).
  4. **Post-Sync Immutability Enforcement:** In `ReturnOrderListView`, inspecting an order with `StatusSync = 'Synced'` locks all editing controls and displays a read-only badge banner.
  5. **Standardized Keyboard Map:**
     - `F1`: Switch to Intake Retur (`ReturnOrderEntryView`)
     - `F2`: Manual Item Search Dialog (`ManualItemSearchDialog`)
     - `F3`: Switch to Daftar Retur (`ReturnOrderListView`)
     - `F4`: Switch to Barcode Registry (`BarcodeRegistryView`)
     - `F5`: Switch to Settings (`SettingsView`)
     - `F9`: Simpan Draft Lokal
     - `F10`: Simpan & Upload (Immediate Sync)
     - `Enter`: Commit quantity strip to grid / select in search modal
     - `Esc`: Clear form / close active modal dialog
- *Traceable to:* GAP-003, GAP-005, GAP-014, Approved Screen Design Inventory.

---

# 5. Component Responsibilities

## 5.1 Project-Level Responsibilities

| Project / Component | Responsibilities |
| :--- | :--- |
| **`BtrReturGudang.Domain`** | • Core entities: `ReturnOrder`, `ReturnOrderItem`, `Barcode`, `Barang`, `Customer`, `Driver`, `SalesPerson`.<br>• Enums: `JenisRetur` (`BAGUS`, `RUSAK`), `StatusSync` (`Draft`, `Synced`).<br>• Value objects and normalization rules (`BarcodeNormalization`, ULID generator). |
| **`BtrReturGudang.AppTier`** | • Repository contracts: `IReturnOrderRepo`, `IBarcodeRepo`, `IReferenceRepo`.<br>• DTOs: `ReturnOrderDto`, `BarcodeRegistrationDto`, `MasterSyncDto`, `ApiUploadResultDto`, `SessionResolveResultDto` / `SessionWarehouseDto`.<br>• Orchestration services: `ReturnOrderService`, `BarcodeService`, `SyncService`.<br>• Registry contract `IRegistryHelper` (exposes `LastOperatorName` and `LastOperatorEmail`). |
| **`BtrReturGudang.Infrastructure`** | • Dapper DALs: `ReturnOrderDal`, `BarcodeDal`, `MasterDataDal`.<br>• SQL connection management (`ConnStringHelper`).<br>• Cloud API Client (`BtradeApiClient`) handling RestSharp calls, JSON serialization, `X-Session-*` headers, and session resolution (`ResolveSessionAsync`).<br>• Hardware & OS helpers: `AudioHelper` (beeps), `RegistryHelper` (workstation settings, including operator name and registered Google email). |
| **`BtrReturGudang.Winform`** | • Application entry point (`Program.cs`) with DI service container.<br>• Shell window (`MainForm`) with docked Sidebar Navigation and live Sync status indicator.<br>• Approved Screen Design Inventory (SCR-01 through SCR-07).<br>• Input filters: `ScannerWedgeListener` (50ms burst detection). |
| **`BtrReturGudang.SqlDb`** | • Database DDL and indexing scripts for `btrgd` database. |

## 5.2 Screen Design Inventory (`BtrReturGudang.Winform`)

| Screen ID | Control / Class Name | WinForms Base Type | Primary Purpose & Responsibilities | Trigger / Hotkey | Data Target / Store |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **SCR-01** | `MainForm` | `Form` (Shell) | Container hosting docked sidebar navigation, workstation location badge, shift operator display, global USB scanner buffer hook (`IMessageFilter`), 5-min sync timer, and view switcher. | `App Startup` | Registry, AppSettings |
| **SCR-02** | `ReturnOrderEntryView` | `UserControl` (`Dock.Fill`) | **Core Operational Intake:** Customer header autocomplete, active scanner focus banner, ergonomic Quantity Strip (`Qty Bagus`/`Qty Rusak`), double-buffered intake DataGridView, and action toolbar. | `F1` / `Alt+1` | `BTRG_ReturnOrder`<br>`BTRG_ReturnOrderItem` |
| **SCR-03** | `ManualItemSearchDialog` | `Form` (`ShowDialog`) | **Manual Product Search (Fallback):** Fast keyboard-driven product search when barcode is damaged, torn, or missing. Search by item code/name, packaging unit selector, and return to intake strip. | `F2` | `BTRG_Barang` (Local Cache) |
| **SCR-04** | `QuickBarcodeRegistrationDialog` | `Form` (`ShowDialog`) | **On-the-spot Barcode Registration:** Modal dialog triggered on unknown barcode scan. Emits warning buzzer (400Hz), maps barcode to Item master & unit, queues upload request, and immediately adds to return. | *Auto on Unknown Scan* | `BTRG_BarcodeRegistration`<br>`BTRG_Barcode` (Local Cache) |
| **SCR-05** | `ReturnOrderListView` | `UserControl` (`Dock.Fill`) | **Return Order History & Inspector:** Filterable log of return orders by date/status (`Draft`/`Synced`). Displays detail inspector and strictly enforces Post-Sync Immutability (read-only lock). | `F3` / `Alt+2` | `BTRG_ReturnOrder`<br>`BTRG_ReturnOrderItem` |
| **SCR-06** | `BarcodeRegistryView` | `UserControl` (`Dock.Fill`) | **Barcode Catalog Management:** Local barcode lookup, review of pending on-the-spot registration queues, and manual registration of packaging barcodes with conversion multipliers. | `F4` / `Alt+3` | `BTRG_Barcode`<br>`BTRG_BarcodeRegistration` |
| **SCR-07** | `SettingsView` | `UserControl` (`Dock.Fill`) | **Station Configuration:** Depo/Warehouse code assignment, local SQL Server (`btrgd`) connection tester, active shift operator switcher (operator name + registered Google email, with session verification via `POST api/session/resolve`), and audio feedback test/toggle. | `F5` / `Alt+4` | Windows Registry<br>App.config |

---

# 6. Integration Design

```text
┌─────────────────────────────────────────────────────────────────────────────┐
│                       BtrReturGudang.Winform                                │
│                                                                             │
│  [USB Scanner] ──► [ScannerWedgeListener]                                   │
│                            │                                                │
│                            ▼                                                │
│                 [ReturnOrderEntryView] ◄──► [AudioHelper] (Beeps)           │
│                            │                                                │
│                            ▼                                                │
│                 [ReturnOrderService] ──► [RegistryHelper] (Depo / Operator) │
└────────────────────────────┬────────────────────────────────────────────────┘
                             │
              ┌──────────────┴──────────────┐
              ▼                             ▼
┌───────────────────────────┐ ┌───────────────────────────────────────────────┐
│ BtrReturGudang.AppTier    │ │ BtrReturGudang.Infrastructure                 │
│ (IReturnOrderRepo / DAL)  │ │ (BtradeApiClient)                             │
└─────────────┬─────────────┘ └───────────────────────┬───────────────────────┘
              │                                       │
              ▼                                       ▼
┌───────────────────────────┐         ┌───────────────────────────────────────┐
│ Local SQL Server (btrgd)  │         │ Cloud Staging API (j06-pkl-btrade-api)│
│  - BTRG_ReturnOrder       │         │  - POST api/session/resolve           │
│  - BTRG_ReturnOrderItem   │         │  - POST api/return-order              │
│  - BTRG_Barcode           │         │  - GET  api/barcodes/sync             │
│  - BTRG_Barang            │         │  - POST api/barcode-registration      │
└───────────────────────────┘         │  - GET  api/Customer/{serverId}       │
                                      └───────────────────────────────────────┘
```

| Source Component | Target Component | Interaction Description |
| :--- | :--- | :--- |
| **USB Scanner** | `ScannerWedgeListener` | Hardware emits HID keystroke bursts terminating with `\r\n`. |
| **`ScannerWedgeListener`** | `ReturnOrderEntryView` | Dispatches clean, normalized barcode string directly to intake logic. |
| **`ReturnOrderEntryView`** | `AudioHelper` | Invokes `PlaySuccess()` or `PlayWarning()` depending on lookup outcome. |
| **`ReturnOrderEntryView`** | `ManualItemSearchDialog` | `F2` opens keyboard modal; selected item populates intake quantity strip. |
| **`ReturnOrderEntryView`** | `QuickBarcodeRegistrationDialog` | Unknown scan triggers modal; maps barcode locally and queues registration request. |
| **`ReturnOrderEntryView`** | `ReturnOrderService` | Creates, updates, and commits Return Order aggregate. |
| **`ReturnOrderService`** | `ReturnOrderDal` (Dapper) | Persists documents locally in `(local)\btrgd` database tables. |
| **`SyncService`** | `BtradeApiClient` | Uploads pending return orders and pulls master deltas via HTTP. |
| **`SettingsView`** / operator switch | `BtradeApiClient` | Verifies the configured operator Google email via `ResolveSessionAsync` (`POST api/session/resolve`) before warehouse work begins. |
| **`BtradeApiClient`** | `j06-pkl-btrade-api` | Sends REST payloads with `X-Session-Location` (`WarehouseCode`) and `X-Session-Actor` (registered Google email) headers; resolves the operator email through the anonymous session-resolution endpoint. |

---

# 7. Data Ownership

| Data Aggregate / Store | Authoritative Owner | Staging / Local Cache Role |
| :--- | :--- | :--- |
| **Return Order (Captured)** | **Local Desktop (`btrgd`)** while `Draft`; **Main Office (`btr.sql`)** once `Imported`. | Desktop client owns the document during capture. Ownership transfers upstream upon sync (P-10). |
| **Cloud Relay (`btrade.sqldb`)** | **Cloud API Relay (`j06-pkl-btrade-api`)** | Pure staging transport; never authoritative (P-02). |
| **Master Data (Item / Satuan)** | **Main Office (`btr.sql`)** | Desktop stores local read-only cache in `BTRG_Barang`. |
| **Active Barcodes** | **Main Office (`btr.sql`)** | Desktop stores local lookup cache in `BTRG_Barcode`. |
| **Barcode Registrations** | **Desktop Client** (local queue) -> **Main Office** (approval) | Desktop queues new requests in `BTRG_BarcodeRegistration`. |
| **Customer / Driver / Salesman** | **Main Office (`btr.sql`)** | Desktop stores local read-only caches in `BTRG_Customer`, etc. |
| **Workstation Config & Session** | **Local Workstation Registry / AppSettings** | Stores workstation settings (`DepoId`, `ServerTargetId`, `OperatorName`, `OperatorEmail`). The operator Google email is local session input only; it is not an identity authority. |
| **Operator Google-Email → BTR User Mapping** | **Main Office (`btr.sql`, `BTR_User.Email`)** | Authoritative registration linking a Google email to a BTR user; projected to Cloud `BTRADE_User.Email` by `j07-btrade-sync`. The desktop client never owns or invents this mapping. |

---

# 8. Database Design (`btrgd`)

All tables reside in the local SQL Server database `btrgd` on `(local)`. Per BTR architectural convention (IR-RO-11), tables declare **no foreign keys**; integrity is maintained at the application layer.

## New Tables

| Table | Purpose |
| :--- | :--- |
| **`BTRG_ReturnOrder`** | Header record for locally captured Return Orders. |
| **`BTRG_ReturnOrderItem`** | Individual line items recording physical product, unit, and good/damaged quantities. |
| **`BTRG_Barcode`** | Local cache of active product barcodes for instantaneous scanner lookup. |
| **`BTRG_BarcodeRegistration`** | Queue of on-the-spot barcode registration requests awaiting upload. |
| **`BTRG_Barang`** | Local cache of product master data (`BrgId`, `BrgName`, `SatuanList`). |
| **`BTRG_Customer`** | Local cache of customer master data (`CustId`, `CustName`, `Alamat`). |
| **`BTRG_Driver`** | Local cache of delivery drivers (`DriverId`, `DriverName`). |
| **`BTRG_SalesPerson`** | Local cache of sales personnel (`SalesPersonId`, `SalesPersonName`). |
| **`BTRG_SyncLog`** | Execution timestamps and result tracking for synchronization runs. |

## Schema Definitions

### 1. `BTRG_ReturnOrder`
```sql
CREATE TABLE BTRG_ReturnOrder (
    ReturnOrderId    VARCHAR(26)   NOT NULL PRIMARY KEY, -- ULID
    ReturnOrderDate  DATE          NOT NULL,
    CustId           VARCHAR(10)   NOT NULL,
    CustName         VARCHAR(100)  NOT NULL,
    DriverId         VARCHAR(10)   NULL,
    DriverName       VARCHAR(100)  NULL,
    SalesPersonId    VARCHAR(10)   NULL,
    SalesPersonName  VARCHAR(100)  NULL,
    WarehouseCode    VARCHAR(20)   NOT NULL,
    OperatorName     VARCHAR(50)   NOT NULL,
    Keterangan       VARCHAR(250)  NULL,
    StatusSync       VARCHAR(10)   NOT NULL DEFAULT 'Draft', -- 'Draft' or 'Synced'
    CreatedDate      DATETIME      NOT NULL DEFAULT GETDATE(),
    ModifiedDate     DATETIME      NOT NULL DEFAULT GETDATE(),
    SyncedDate       DATETIME      NULL
);
CREATE INDEX IX_BTRG_ReturnOrder_StatusSync ON BTRG_ReturnOrder (StatusSync, ReturnOrderDate);
```

### 2. `BTRG_ReturnOrderItem`
```sql
CREATE TABLE BTRG_ReturnOrderItem (
    ReturnOrderId    VARCHAR(26)   NOT NULL,
    NoUrut           INT           NOT NULL,
    BrgId            VARCHAR(20)   NOT NULL,
    BrgName          VARCHAR(100)  NOT NULL,
    Barcode          VARCHAR(50)   NULL,
    JenisRetur       VARCHAR(5)    NOT NULL, -- 'BAGUS' or 'RUSAK'
    Qty              DECIMAL(18,2) NOT NULL,
    SatId            VARCHAR(7)    NOT NULL, -- Packaging unit (Dus, Pak, Pcs)
    Keterangan       VARCHAR(250)  NULL,
    PRIMARY KEY (ReturnOrderId, NoUrut)
);
```

### 3. `BTRG_Barcode`
```sql
CREATE TABLE BTRG_Barcode (
    Barcode          VARCHAR(50)   NOT NULL PRIMARY KEY,
    BrgId            VARCHAR(20)   NOT NULL,
    BrgName          VARCHAR(100)  NOT NULL,
    Satuan           VARCHAR(7)    NOT NULL,
    Pengali          DECIMAL(18,2) NOT NULL DEFAULT 1.0,
    IsActive         BIT           NOT NULL DEFAULT 1
);
CREATE INDEX IX_BTRG_Barcode_BrgId ON BTRG_Barcode (BrgId);
```

### 4. `BTRG_BarcodeRegistration`
```sql
CREATE TABLE BTRG_BarcodeRegistration (
    RequestId        VARCHAR(26)   NOT NULL PRIMARY KEY, -- ULID
    Barcode          VARCHAR(50)   NOT NULL,
    BrgId            VARCHAR(20)   NOT NULL,
    Satuan           VARCHAR(7)    NOT NULL,
    WarehouseCode    VARCHAR(20)   NOT NULL,
    OperatorName     VARCHAR(50)   NOT NULL,
    StatusSync       VARCHAR(10)   NOT NULL DEFAULT 'Pending', -- 'Pending' or 'Uploaded'
    CreatedDate      DATETIME      NOT NULL DEFAULT GETDATE()
);
```

### 5. `BTRG_Barang` (Master Cache)
```sql
CREATE TABLE BTRG_Barang (
    BrgId            VARCHAR(20)   NOT NULL PRIMARY KEY,
    BrgName          VARCHAR(100)  NOT NULL,
    Satuan1          VARCHAR(7)    NULL,
    Satuan2          VARCHAR(7)    NULL,
    Satuan3          VARCHAR(7)    NULL,
    IsActive         BIT           NOT NULL DEFAULT 1
);
CREATE INDEX IX_BTRG_Barang_BrgName ON BTRG_Barang (BrgName);
```

### 6. Reference Caches (`Customer`, `Driver`, `SalesPerson`)
```sql
CREATE TABLE BTRG_Customer (
    CustId           VARCHAR(10)   NOT NULL PRIMARY KEY,
    CustName         VARCHAR(100)  NOT NULL,
    Alamat           VARCHAR(150)  NULL
);
CREATE INDEX IX_BTRG_Customer_Name ON BTRG_Customer (CustName);

CREATE TABLE BTRG_Driver (
    DriverId         VARCHAR(10)   NOT NULL PRIMARY KEY,
    DriverName       VARCHAR(100)  NOT NULL
);

CREATE TABLE BTRG_SalesPerson (
    SalesPersonId    VARCHAR(10)   NOT NULL PRIMARY KEY,
    SalesPersonName  VARCHAR(100)  NOT NULL
);
```

---

# 9. Cross-Cutting Concerns

### Concurrency & Thread Safety
- **UI Thread Isolation:** Database queries and HTTP sync calls run via `Task.Run` / `async-await` off the main UI thread to prevent UI freezing during scanner bursts.
- **Connection Scope:** Dapper calls use localized, short-lived `using (var conn = ConnStringHelper.GetConnection())` to prevent pool starvation alongside `j07-btr-gudang`.
- **Sync Guard:** Synchronization worker employs an `Interlocked.CompareExchange` re-entrancy lock to guarantee only one sync pass executes at any time.

### Scanner Burst Filtration
- An in-memory queue buffers raw key inputs. When the time between consecutive characters exceeds 50ms, the buffer is flushed and cleared. When an `Enter` key arrives following a rapid stream (< 50ms per key), the string is validated, normalized via `BarcodeNormalization`, and sent to the intake handler while setting `KeyEventArgs.Handled = true`.

### Error Resilience & Offline Tolerance
- Network loss does not prevent saving: Return Orders save to `BTRG_ReturnOrder` with `StatusSync = 'Draft'`. The UI displays a clear offline indicator and the count of pending documents. When connectivity resumes, background timer pushes queued records automatically.

---

# 10. Implementation Constraints

1. **Strict Runtime Environment:** Projects must target **.NET Framework 4.7.1** (`<TargetFrameworkVersion>v4.7.1</TargetFrameworkVersion>`).
2. **Dapper Only:** Use Dapper for all database queries and commands; Entity Framework / EF Core are strictly prohibited.
3. **No Database Foreign Keys:** Tables in `btrgd` must not define SQL foreign keys, adhering to BTR warehouse database precedent.
4. **API Contract Compatibility:** The REST client must strictly adhere to the existing `j06-pkl-btrade-api` contracts. All outbound session-context requests must send `X-Session-Location` (`WarehouseCode`) and `X-Session-Actor` (**registered Google email**, never the display name). The actor input must never be a free-text operator name, username, or BTR password.
5. **No Third-Party Commercial Controls:** Use native Windows Forms controls with clean custom drawing/styling; do not introduce heavy UI suites (DevExpress, Infragistics, etc.).
6. **Keyboard Operability & Shortcuts:** The entire Return Order intake flow must be operable without requiring a mouse: `Enter` to commit quantity strip, `F2` to trigger manual item search, `Esc` to cancel/reset, `F9` for local draft, `F10` for save & upload, and `F1`-`F5` for view switching (TD-011).
7. **Screen Inventory Fidelity & Flicker Prevention:** Implement all 7 approved UI artifacts (`SCR-01` to `SCR-07`) conforming to TD-011. All `DataGridView` instances must enable double-buffering to eliminate rendering flicker on older workstation displays.
8. **Operator Email Required for Uploads:** Cloud uploads must not be attempted unless the workstation has a configured operator Google email that resolves successfully via `POST api/session/resolve`. An unresolved operator email is surfaced as an explicit sign-in refusal (matching the Cloud `409`/`400` semantics), not a silent retry.

---

# 11. Acceptance Conditions

1. Solution `src/j09-btr-retur-gudang/BtrReturGudang.sln` compiles successfully with 0 errors targeting .NET 4.7.1.
2. DDL script creates all `BTRG_` warehouse tables in `btrgd` successfully.
3. Application shell `MainForm` (SCR-01) provides docked sidebar navigation switching smoothly between views (SCR-02, SCR-05, SCR-06, SCR-07) while hosting the global `IMessageFilter` scanner hook and 5-min sync timer.
4. Connected USB barcode scanner input is recognized hands-free in `ReturnOrderEntryView` (SCR-02) without requiring focus on a specific textbox.
5. Scanning recognized barcodes populates product info, focuses the quantity strip (`numQtyBagus` / `numQtyRusak`), and pressing `Enter` commits the line to the double-buffered grid with a 2000Hz success beep.
6. Scanning an unrecognized barcode triggers `QuickBarcodeRegistrationDialog` (SCR-04) with a 400Hz warning beep, permitting instant item mapping and active return inclusion while queuing registration.
7. Pressing `F2` opens `ManualItemSearchDialog` (SCR-03) with fast keyboard navigation, item filtering, and unit selection.
8. Documents saved with "Simpan & Upload" (`F10`) persist to `btrgd` and upload to Cloud API (`POST api/return-order`) under the resolved operator identity, transitioning from `Draft` to `Synced`; the request carries `X-Session-Location` and `X-Session-Actor` (registered Google email) and no `HTTP 409` "Google account is not registered or is not active" refusal occurs for a correctly configured operator.
9. In `ReturnOrderListView` (SCR-05), synchronized Return Orders are locked in read-only inspection mode, blocking Edit and Delete.
10. `BarcodeRegistryView` (SCR-06) allows searching and filtering local barcode cache and pending registration queues.
11. `SettingsView` (SCR-07) supports Depo assignment, local database connection testing, shift operator switching (operator name and registered Google email), session verification via `POST api/session/resolve`, and audio feedback toggling.
12. Background timer executes periodic sync every 5 minutes without freezing the application UI.
13. The operator Google email resolves to an active BTR user via `POST api/session/resolve`; an unregistered or inactive email is refused with the Cloud message surfaced to the operator, and no Cloud upload is attempted with an unresolved or name-based actor.
