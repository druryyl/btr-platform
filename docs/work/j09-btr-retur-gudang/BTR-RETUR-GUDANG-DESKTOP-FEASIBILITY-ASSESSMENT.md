---
Title: Warehouse Desktop Return Order & Barcode Management (.NET 4.7.1) — Feasibility Assessment
Code: BTR-RETUR-GUDANG-DESKTOP-001
Artifact: FEASIBILITY-ASSESSMENT
Version: 1.0
LastUpdated: 2026-10-11
Status: READY-FOR-PLANNING
---

# 1. Request Summary

Assessment of ISSUE `BTR-RETUR-GUDANG-DESKTOP-001` (`docs/issues/BTR-RETUR-GUDANG-DESKTOP-ISSUE.md`): replace the rejected Android mobile client (`src/BGud`) with a dedicated Windows desktop application targeting .NET Framework 4.7.1, operating with a connected USB barcode scanner on the onsite warehouse PC.

Referenced artifacts:

- DOMAIN: `docs/work/return-order/RETURN-ORDER-DOMAIN.md`, `docs/work/barcode-registry/BARCODE-REGISTRY-DOMAIN.md`
- FEATURE: `docs/features/return-order-desktop/feature.md` (`BTR-RETUR-DESKTOP-001`)
- Precedent Architecture: `docs/work/return-order/RETURN-ORDER-ARCHITECTURE.md`
- Reference Warehouse Implementation: `src/j07-btr-gudang`

## Objective

Determine whether and how the requested desktop application can be implemented within the current system, establishing technical feasibility, assessing gaps against existing backend and database services, evaluating workstation constraints, and recording gap-closure decisions before target architecture can be finalized.

---

# 2. Current State

Facts verified on 2026-10-11 across the codebase and infrastructure.

## Existing Behavior

### Warehouse Mobile Client (`src/BGud`)
- The Android application was built with Kotlin, Jetpack Compose, Room (SQLite), and Retrofit.
- Implemented features:
  - Create, edit, list, detail, and sync Return Orders (`CreateReturnOrderScreen`, `ReturnOrderListScreen`).
  - Barcode lookup, in-screen registration, and edit (`BarcodeRegistryScreen`, `RegisterScreen`).
  - Workstation session binding (`SessionPreferencesDataSource`, `SessionBinding`).
  - Sync with Cloud API (`BtradeApiService`).
- **Operational Reality:** Client stakeholders explicitly rejected the Android mobile form-factor for warehouse operations due to rugged device costs, handling ergonomics in warehouse receiving, and battery/device maintenance friction.

### Existing Warehouse Desktop Client (`src/j07-btr-gudang`)
- Windows Forms application (`BtrGudang.Winform`) targeting .NET Framework 4.7.1.
- Layered structure: `BtrGudang.Domain`, `BtrGudang.AppTier`, `BtrGudang.Helper`, `BtrGudang.Infrastructure`, `BtrGudang.Winform`, `BtrGudang.SqlDb`.
- Data persistence: Local SQL Server database `btrgd` on `(local)` accessed via Dapper and ADO.NET (`ConnStringHelper`). User credentials: `btrGudangLogin` / `btrGudang123!`.
- Configuration: Workstation settings stored in `appsettings.json` and Windows Registry (`HKCU\DrurySoftware\BTRApp`).
- Network client: RestSharp downloading packing orders from `BtradeCloud` (`BtradeCloudOptions`).
- UI style: Traditional Windows Forms MDI layout.

### Cloud API Relay (`src/j06-pkl-btrade-api`)
- Existing endpoints already realized and operational:
  - `POST api/return-order`: Idempotent Return Order submission keyed by ULID `ReturnOrderId`.
  - `GET api/barcodes/sync`: Active barcode catalog bulk download.
  - `POST api/barcode-registration`: Submit barcode registration request.
  - `GET api/BarcodeRegistration/status`: Query registration request outcomes.
  - `GET api/Brg/{serverId}`: Product catalog reference download.
  - `GET api/Customer/{serverId}`: Customer reference download.
  - `GET api/Driver/{serverId}`: Driver reference download.
  - `GET api/SalesPerson/{serverId}`: Sales person reference download.
- Ingress convention: Staging relay (`StatusSync` = `TERKIRIM` -> `DOWNLOADED`).

### Main Office Server (`src/j05-btr-distrib`) and Sync (`src/j07-btrade-sync`)
- Authoritative aggregate `ReturnOrder` in `btr.domain` / `btr.application` / `btr.sql`.
- In-process sync download and import generates `ReturnOrderNo` and stages documents for Sales Return (`ReturJual`) generation.

## Existing Constraints

1. **Workstation Hardware & OS:** The onsite warehouse PC has limited CPU, memory, and graphics hardware. It cannot support modern .NET 8 / WPF / MAUI desktop stacks. It is constrained to **.NET Framework 4.7.1** and GDI+ rendering.
2. **Scanner Device:** Warehouse receiving uses a dedicated USB barcode scanner operating in USB HID Keyboard Emulation (Wedge) mode, transmitting keystrokes rapidly and appending an Enter suffix (`\r\n`).
3. **Network Stability:** Warehouse network links can be unstable. Receiving must continue uninterrupted during internet drops, with local persistence in `btrgd` and background synchronization.
4. **Local Database Instance:** The onsite PC runs a local SQL Server instance (`(local)`) hosting database `btrgd`. Table naming convention uses the prefix `BTRG_`.

---

# 3. Gap Analysis

| ID | Severity | Gap |
| :--- | :--- | :--- |
| **GAP-001** | CRITICAL | **Missing Desktop Application Project:** There is currently no Windows desktop application providing Return Order capture and Barcode Registry. |
| **GAP-002** | CRITICAL | **Missing Schema in `btrgd`:** The local SQL Server database `btrgd` contains only packing order tables (`BTRG_PackingOrder*`); it lacks tables for Return Orders, Barcodes, and reference caches. |
| **GAP-003** | MAJOR | **Barcode Scanner Input Interception:** WinForms standard controls require explicit focus to receive keystrokes; without global key preview and scanner buffer detection, scans made while focus is on grid cells or buttons are lost or misdirected. |
| **GAP-004** | MAJOR | **Dual-Trigger Synchronization Service in .NET 4.7.1:** Absence of an asynchronous synchronization worker in the desktop solution to handle immediate save-time uploads, periodic 5-minute background syncs, and manual trigger with live online/offline detection. |
| **GAP-005** | MINOR | **Audio Feedback Infrastructure:** No sound generation utility in the existing desktop libraries to emit distinct tones for scan success vs scan error without external media dependencies. |
| **GAP-006** | MINOR | **Workstation Configuration & Operator Switcher:** Absence of a lightweight shift operator selection mechanism linked to Registry workstation settings. |

---

# 4. Open Questions

| ID | Question | Impact |
| :--- | :--- | :--- |
| **OQ-001** | How should the desktop client supply tenant/location context to Cloud API endpoints that previously read session headers from mobile login? | Impacts HTTP request builder and interceptor design in `BtradeApiClient`. |
| **OQ-002** | What timing threshold reliably differentiates rapid USB barcode scanner bursts from normal manual keyboard typing in WinForms? | Impacts scanner wedge listener responsiveness and prevents false keystroke capture. |
| **OQ-003** | How should local database transactions and connections be managed alongside `j07-btr-gudang` without connection pool exhaustion? | Impacts Dapper repository lifecycle and ADO.NET connection strings. |

---

# 5. Assumptions

| ID | Assumption |
| :--- | :--- |
| **ASM-001** | The onsite PC has .NET Framework 4.7.1 runtime and SQL Server Express / Developer running with database `btrgd` accessible via `btrGudangLogin` / `btrGudang123!`. |
| **ASM-002** | The USB barcode scanner emits standard ASCII characters terminated by a Carriage Return / Line Feed (`\r` or `\r\n`). |
| **ASM-003** | Upstream Cloud API endpoints in `j06-pkl-btrade-api` are deployed and accessible via the configured base URL. |
| **ASM-004** | Master data contracts for Item (`Brg`), Customer, Driver, and SalesPerson returned by Cloud API match the models used by BGud. |
| **ASM-005** | Standalone deployment via portable ZIP / folder distribution satisfies client IT maintenance requirements. |

---

# 6. Risks

| ID | Risk | Impact | Mitigation |
| :--- | :--- | :--- | :--- |
| **RISK-001** | **Scan Keystroke Leakage:** Barcode characters leak into an active text box or grid cell if global key preview does not suppress keyboard events during scan bursts. | Data corruption in description/search fields. | Implement a scanner listener that buffers burst characters, cancels handled key events, and dispatches the completed barcode directly to the scan processor. |
| **RISK-002** | **Network Timeout on Document Save:** If "Simpan & Upload" blocks UI thread waiting for HTTP response during slow internet, operator experiences screen freeze. | Frustrating operator delays during unloading. | Perform local SQL commit synchronously first, then dispatch upload asynchronously in a background task; update UI status asynchronously. |
| **RISK-003** | **Large Master Data Query Latency:** Querying thousands of items or customers on low-spec hardware could cause UI stutter. | Slow manual search grid. | Implement indexed columns in `btrgd`, parameterized Dapper queries with `TOP 50` / `LIKE`, and debounce search inputs. |
| **RISK-004** | **Split-Brain Edits Post-Sync:** If an operator edits a Return Order after it has synchronized upstream, local state diverges from Main Office. | Conflicting return documents. | Enforce strict state validation: lock UI and database rows to read-only once `StatusSync` = `Synced` (ADR-RO-006). |

---

# 7. Recommendations

## Option A — Standalone Layered Solution `src/j09-btr-retur-gudang` (Recommended)

Create a dedicated standalone solution matching `j07-btr-gudang`'s proven architecture:
- `BtrReturGudang.Domain`: Core entities (`ReturnOrder`, `ReturnOrderItem`, `Barcode`, `Barang`, `Customer`).
- `BtrReturGudang.AppTier`: Repository interfaces, DTOs, synchronization services.
- `BtrReturGudang.Infrastructure`: Dapper DALs, Cloud API client using RestSharp/System.Text.Json, Registry configuration.
- `BtrReturGudang.Winform`: Modern single-window interface with docked Sidebar Navigation, smart scanner listener, quantity strip, and audio beeps.
- `BtrReturGudang.SqlDb`: SQL scripts for creating tables and indexes in `btrgd`.

### Advantages
- Clean separation of concerns with zero risk of breaking existing `j07-btr-gudang` packing order features.
- Independent compile, test, versioning, and portable deployment.
- Reuses established patterns from `j07-btr-gudang` (Dapper, DI, RestSharp, `RegistryHelper`).

### Disadvantages
- Minor code duplication of shared helper utilities (`ConnStringHelper`, `RegistryHelper`) if not linked as a shared project.

## Option B — Monolithic Extension Inside `src/j07-btr-gudang`

Add Return Order forms directly into the existing `BtrGudang.Winform` project.

### Advantages
- Reuses common infrastructure projects directly without separate solution files.

### Disadvantages
- Blurs domain boundaries (Packing Order printing vs Return Order receiving).
- Regression risk to packing order operations during Return Order updates.
- Violates client mandate for a dedicated, independent application.

---

# 8. Gap Closure

Decisions recorded from stakeholder alignment and technical assessment.

## GAP-001 — Standalone Desktop Solution Location & Structure
### Decision
Create a dedicated standalone solution in `src/j09-btr-retur-gudang` targeting .NET Framework 4.7.1, structured into layered projects (`Domain`, `AppTier`, `Infrastructure`, `Winform`, `SqlDb`).
### Rationale
Provides clean separation of concerns, independent deployment, and isolation from `j07-btr-gudang` packing order code, adhering to repository naming conventions.
### Impact
Requires solution scaffolding and project configurations matching .NET 4.7.1.
### Architecture Impact
Defines the component boundary and deployment unit for the warehouse desktop client.
### Resolved By
Stakeholder alignment session (`/grill-me`).
### Resolved Date
2026-10-11

---

## GAP-002 — Local Database Persistence Schema in `btrgd`
### Decision
Create dedicated warehouse tables in the existing local SQL Server database `btrgd` on `(local)`:
- `BTRG_ReturnOrder` (Header: `ReturnOrderId` VARCHAR(26) PK, `ReturnOrderDate`, `CustId`, `DriverId`, `SalesPersonId`, `WarehouseCode`, `OperatorName`, `Keterangan`, `StatusSync`, `CreatedDate`, `ModifiedDate`)
- `BTRG_ReturnOrderItem` (Line Items: `ReturnOrderId`, `NoUrut`, `BrgId`, `Barcode`, `JenisRetur`, `Qty`, `SatId`, `Keterangan`)
- `BTRG_Barcode` (Local active barcode cache)
- `BTRG_BarcodeRegistration` (Queued registration requests: `RequestId`, `Barcode`, `BrgId`, `Satuan`, `StatusSync`, `CreatedDate`)
- `BTRG_Barang`, `BTRG_Customer`, `BTRG_Driver`, `BTRG_SalesPerson` (Reference caches)
- Tables will declare no foreign keys (IR-RO-11).
### Rationale
Reuses the local SQL Server instance already running on the onsite PC, providing offline durability and fast indexed queries without requiring new database installations.
### Impact
Migration/DDL scripts must be executed against `btrgd`.
### Architecture Impact
Establishes the local persistence model and schema contracts for Dapper repositories.
### Resolved By
Stakeholder alignment session (`/grill-me`).
### Resolved Date
2026-10-11

---

## GAP-003 — USB Barcode Scanner Interception & Ergonomics
### Decision
Implement a smart scanner wedge interceptor at the form level (`KeyPreview = true` / Win32 input filter). Characters arriving in rapid succession (< 50ms inter-character delay) terminating with `Enter` are identified as barcode scans and captured globally. When valid:
- Product details populate on the active item strip.
- Focus immediately lands on `Qty Bagus` (with Tab to `Qty Rusak`).
- Pressing `Enter` adds the item to the grid and returns focus to the scanner listener.
- Unknown barcodes trigger an on-the-spot registration dialog with immediate local usability.
- Pressing `F2` triggers manual product lookup.
### Rationale
Delivers fast, hands-free receiving ergonomics where operators scan boxes without clicking into a specific textbox for every scan.
### Impact
Requires WinForms keyboard event handling logic in the form presentation layer.
### Architecture Impact
Governs the presentation-tier input pipeline.
### Resolved By
Stakeholder alignment session (`/grill-me`).
### Resolved Date
2026-10-11

---

## GAP-004 — Dual-Trigger Synchronization Strategy
### Decision
Implement an asynchronous synchronization service (`ReturnOrderSyncService` / `MasterDataSyncService`) with dual triggers:
1. Save-Time Trigger: Immediate background HTTP post when saving with "Simpan & Upload".
2. Periodic Background Timer: Runs every 5 minutes using `System.Windows.Forms.Timer` to upload pending records and pull updated master reference data.
3. Manual Trigger: "Sync Now" button on the navigation sidebar with live online/offline indicator.
### Rationale
Ensures data is relayed to Cloud staging as soon as possible while remaining fully resilient to warehouse network outages.
### Impact
Requires background thread execution and thread-safe UI status updates.
### Architecture Impact
Defines the integration service contracts and scheduler components.
### Resolved By
Stakeholder alignment session (`/grill-me`).
### Resolved Date
2026-10-11

---

## GAP-005 — Audio Feedback Infrastructure
### Decision
Implement an audio feedback helper using native Windows multimedia beeps (`System.Media.SystemSounds` / `Console.Beep(frequency, duration)`):
- Scan Success: High pitch short tone (e.g., 2000 Hz, 80 ms).
- Scan Error / Unrecognized: Lower dual-buzz tone (e.g., 400 Hz, 200 ms).
- Configurable toggle (Mute / Enable) stored in Settings.
### Rationale
Zero external audio asset or codec dependencies; runs reliably on any Windows PC speaker or buzzer; allows operators to verify scans by ear.
### Impact
Low complexity utility class.
### Architecture Impact
Presentation-tier UX component.
### Resolved By
Stakeholder alignment session (`/grill-me`).
### Resolved Date
2026-10-11

---

## GAP-006 — Workstation Configuration & Operator Switcher
### Decision
Workstation settings (Warehouse code, `ServerTargetId`, Cloud API BaseUrl) are stored in Windows Registry (`HKCU\DrurySoftware\BTRApp`) and `appsettings.json`. On shift start, operators select or enter their Operator Name via a clean header switcher without a heavy password login screen.
### Rationale
Balances accountability with operational speed on shared warehouse terminals.
### Impact
Simple session state model in the desktop application.
### Architecture Impact
Configuration and session context provider design.
### Resolved By
Stakeholder alignment session (`/grill-me`).
### Resolved Date
2026-10-11

---

## OQ-001 — Cloud API Session Context Headers
### Decision
The desktop HTTP client will supply `X-Session-Location: {WarehouseCode}` and `X-Session-Actor: {OperatorName}` on outgoing requests to `j06-pkl-btrade-api`, identical to the contract consumed by BGud (`SessionContextInterceptor`).
### Rationale
Allows the existing Cloud API controllers (`ReturnOrderController`, `BarcodeRegistrationController`) to resolve tenant mapping without requiring modifications to backend server controllers.
### Impact
Full drop-in API contract compatibility with existing Cloud staging services.
### Architecture Impact
Client-side HTTP interceptor/request builder design.
### Resolved By
Analysis & code verification of `BtradeApiService.kt` and `ReturnOrderController.cs`.
### Resolved Date
2026-10-11

---

## OQ-002 — Scanner Keystroke Timing Threshold
### Decision
Set the maximum inter-character timing threshold for scanner bursts to 50 milliseconds. Keystroke bursts with inter-character intervals <= 50ms terminated by `Enter` are processed as scanner inputs; keystrokes with larger intervals are treated as human keyboard input.
### Rationale
Standard barcode scanners send characters within 5-20ms of each other. A 50ms window reliably separates scanner transmission from even rapid human typing.
### Impact
Configurable in application settings (default: 50ms).
### Architecture Impact
Scanner wedge listener implementation detail.
### Resolved By
Industry standard scanner integration practice.
### Resolved Date
2026-10-11

---

## OQ-003 — Local SQL Connection Lifecycle
### Decision
Dapper repositories will use short-lived `SqlConnection` instances wrapped in `using` blocks, utilizing ADO.NET built-in connection pooling with standard pool size (max 20).
### Rationale
Prevents connection leaks and ensures seamless co-existence with `j07-btr-gudang`.
### Impact
Standard database helper implementation in `BtrReturGudang.Infrastructure`.
### Architecture Impact
Repository and DAL architecture.
### Resolved By
Technical assessment.
### Resolved Date
2026-10-11

---

# 9. Architecture Applicability

## Decision

**ARCHITECTURE-REQUIRED**

## Rationale

This change introduces:
1. A new standalone desktop system boundary (`src/j09-btr-retur-gudang`).
2. New database persistence tables and Dapper repositories in `btrgd`.
3. Hardware integration for dedicated USB barcode scanner keyboard wedge interception.
4. Asynchronous synchronization and offline queueing services in .NET Framework 4.7.1.
5. Migration of business rules and presentation flows from Android/Kotlin to Windows Forms/C#.

Formal technical realization and target architecture design are required before implementation planning can proceed.

---

# 10. Planning Readiness

## Readiness Checklist

- [x] All critical gaps resolved (GAP-001 through GAP-006 resolved and recorded)
- [x] All required decisions recorded (Decisions, rationales, impacts, and dates documented)
- [x] All blocking open questions resolved (OQ-001 through OQ-003 resolved)
- [x] Architecture Applicability established (ARCHITECTURE-REQUIRED)
- [x] Architecture finalized or updated (Target architecture delivered in BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md)

## Status

**READY-FOR-PLANNING**

## Notes

All feasibility gaps, operational questions, hardware integration modes, and backend compatibility items have been resolved and documented, and the target architecture has been finalized. The Architect has evaluated the readiness checklist and granted the **READY-FOR-PLANNING** gate.

---

# 11. References

### Referenced Artifacts
- DOMAIN: `docs/work/return-order/RETURN-ORDER-DOMAIN.md`
- FEATURE: `docs/features/return-order-desktop/feature.md` (`BTR-RETUR-DESKTOP-001`)
- ISSUE: `docs/issues/BTR-RETUR-GUDANG-DESKTOP-ISSUE.md` (`BTR-RETUR-GUDANG-DESKTOP-001`)
- Precedent Architecture: `docs/work/return-order/RETURN-ORDER-ARCHITECTURE.md`

### Referenced Codebase Locations
- Existing Warehouse App Baseline: `src/j07-btr-gudang`
- Rejected Android Client Reference: `src/BGud`
- Cloud API Relay Endpoints: `src/j06-pkl-btrade-api/btrade.webapi/Controllers/ReturnOrderController.cs`
- Main Office Synchronization: `src/j07-btrade-sync`
- Main Office Aggregate: `src/j05-btr-distrib/btr.application/InventoryContext/ReturnOrderAgg/`

### Referenced Documents
- AGENTS Manifesto: `AGENTS.md`
- Stakeholder Alignment Transcript: 2026-10-11
