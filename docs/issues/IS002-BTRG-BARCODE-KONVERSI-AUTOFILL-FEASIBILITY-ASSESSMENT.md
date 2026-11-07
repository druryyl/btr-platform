---
Title: Feasibility Assessment - Auto-populate Conversion Multiplier in Barcode Registration
Code: IS002-BTRG-BARCODE-KONVERSI-AUTOFILL
Artifact: FEASIBILITY-ASSESSMENT
Version: 1.0
LastUpdated: 2026-11-07
Status: READY-FOR-PLANNING
---

# 1. Request Summary

Assessment of change request `BTRG-BARCODE-KONVERSI-001` (recorded in `docs/issues/IS002 - BTRG-BARCODE-KONVERSI-AUTOFILL-ISSUE.md`).

Referenced artifacts:
- `docs/foundation/PRODUCT.md`
- `docs/foundation/DOMAIN.md`
- `docs/foundation/LANDSCAPE.md`
- `docs/foundation/WORKFLOW.md`
- `docs/issues/IS001 - issue-barcode-satuan-sync.md`
- `docs/issues/IS001-BARCODE-SATUAN-SYNC-FEASIBILITY-ASSESSMENT.md`
- `docs/issues/IS001-BARCODE-SATUAN-SYNC-ARCHITECTURE.md`

## Objective

Assess the technical feasibility, database schema updates, cloud synchronization mapping, and UI behavior required to store the item conversion multiplier in local warehouse cache (`BTRG_Barang.Konversi`) and automatically populate/recalculate it as a read-only field in the barcode registration dialogs (`BarcodeRegistrationDialog` and `QuickBarcodeRegistrationDialog`).

---

# 2. Current State

## Existing Behavior

1. **Source & Cloud Layer (`j06-pkl-btrade-api`, `j07-btrade-sync`):**
   - Cloud database `BTRADE_Brg` already possesses `Konversi INT NOT NULL CONSTRAINT DF_BTRADE_Brg_Konversi DEFAULT(0)`.
   - `j07-btrade-sync` maps `ISNULL(cc.Conversion, 1) AS Konversi` into `BTRADE_Brg`.
   - `j06-pkl-btrade-api` domain model `BrgType` and controller `GET /api/Brg/{serverId}` already return `konversi` in the serialized JSON response.

2. **Warehouse Local Database (`BtrReturGudang.SqlDb`):**
   - Local table `BTRG_Barang` contains columns `BrgId`, `BrgName`, `SatBesar`, `SatKecil`, `IsActive`.
   - Column `Konversi` is absent from `BTRG_Barang`.

3. **Data Access & DTO Layer (`BtrReturGudang.Domain`, `AppTier`, `Infrastructure`):**
   - Domain entity `Barang` lacks `Konversi`.
   - DTO `BarangDto` lacks `Konversi`.
   - `BtradeApiClient.ParseBarang` parses `brgId`, `brgName`, `satBesar`, `satKecil`, but ignores `konversi`.
   - `MasterDataDal` does not persist or read `Konversi`.

4. **Barcode Registration Presentation Layer (`BtrReturGudang.Wpf`):**
   - `BarcodeRegistrationDialog.xaml`: `TxtPengali` is a free-text input initialized to `"1"`. Selecting an item or changing `CboSatuan` does not update `TxtPengali`.
   - `QuickBarcodeRegistrationDialog.xaml` & `QuickBarcodeRegistrationViewModel.cs`: `Pengali` defaults to `1m`. Selecting an item or switching `SelectedSatuan` does not update `Pengali`. Operators must manually enter conversion multipliers.

## Existing Constraints

1. **Master Data Integrity:** The conversion ratio between `SatBesar` and `SatKecil` is definitively established in head office master data (`BTR_BrgSatuan`). Manual operator overrides risk entering invalid conversion multipliers, causing inventory discrepancies during return intake verification.
2. **Compact Viewport (1280x720 / 580x480):** Barcode registration dialogs operate in low-resolution warehouse touch/screen environments where layout and keyboard navigation (Tab/Enter) must remain responsive.

---

# 3. Gap Analysis

| ID | Severity | Gap |
| :--- | :--- | :--- |
| **GAP-001** | CRITICAL | **Local Persistence Gap:** `BTRG_Barang` table and `Barang` entity/DTO do not persist or expose `Konversi` synchronized from cloud master data `BTRADE_Brg`. |
| **GAP-002** | CRITICAL | **Deserialization Gap:** `BtradeApiClient.ParseBarang` ignores the `konversi` field returned by cloud API `GET /api/Brg/{serverId}`. |
| **GAP-003** | MAJOR | **Manual & Error-Prone UI:** `TxtPengali` in `BarcodeRegistrationDialog` and `QuickBarcodeRegistrationDialog` requires manual operator entry instead of dynamically autofilling based on the selected product and packaging unit. |
| **GAP-004** | MINOR | **Operator Editability Risk:** `TxtPengali` is currently editable, allowing human error to override authoritative master data conversion factors. |

---

# 4. Open Questions

| ID | Question | Impact | Status |
| :--- | :--- | :--- | :--- |
| **OQ-001** | How should 'Pengali' behave when switching 'Satuan' in the dialogs? | Governs UI event handling and dynamic calculation logic. | **CLOSED** |
| **OQ-002** | Should 'Pengali' remain editable or become read-only? | Enforces master data integrity vs. allowing warehouse overrides. | **CLOSED** |
| **OQ-003** | What is the implementation scope regarding legacy WinForms? | Determines solution build scope and project references. | **CLOSED** |
| **OQ-004** | Should `ManualItemSearchDialog` also display the conversion column? | Determines scope of grid presentation changes. | **CLOSED** |

---

# 5. Assumptions

| ID | Assumption |
| :--- | :--- |
| **ASM-001** | Cloud table `BTRADE_Brg` and Cloud API `GET /api/Brg/{serverId}` already correctly provide `konversi`. No modifications are required in `j06-pkl-btrade-api` or `j07-btrade-sync`. |
| **ASM-002** | In the business domain, `SatBesar` conversion to `SatKecil` is equal to `Konversi` (where `Konversi > 1`), and `SatKecil` conversion to itself is always `1`. If an item has only one unit, `Konversi` is `1`. |
| **ASM-003** | If `Konversi` is missing, NULL, or `<= 0` in master data, it safely defaults to `1`. |

---

# 6. Risks

| ID | Risk | Impact | Mitigation |
| :--- | :--- | :--- | :--- |
| **RISK-001** | Existing warehouse databases lack `Konversi` column, causing SQL errors on startup or query. | Medium | Add idempotent migration script `004_Add_Barang_Konversi_Column.sql` with default constraint `DEFAULT 1`. |
| **RISK-002** | Legacy data in `BTRG_Barang` has `Konversi = 0`. | Low | Migration script executes `UPDATE BTRG_Barang SET Konversi = 1 WHERE Konversi <= 0 OR Konversi IS NULL;`. |
| **RISK-003** | WinForms projects fail to build after `Barang` domain model changes. | Low | Exclude `BtrReturGudang.Winform` and `BtrReturGudang.Winform.Tests` from `BtrReturGudang.sln`. |

---

# 7. Recommendations

## Option A: Full Dynamic Autofill with Read-Only Lock (Recommended)

1. **Database & Persistence:**
   - Add `004_Add_Barang_Konversi_Column.sql` to add `Konversi INT NOT NULL DEFAULT 1` to `BTRG_Barang`.
   - Update `001_CreateWarehouseTables.sql` baseline script.
   - Update `Barang` entity, `BarangDto`, `BtradeApiClient.ParseBarang`, and `MasterDataDal`.
2. **Domain Conversion Helper:**
   - Add `Barang.GetConversionForUnit(string satuan)`: returns `Konversi > 0 ? Konversi : 1` when `satuan == SatBesar`, and `1` when `satuan == SatKecil` (or fallback `1`).
3. **UI Dynamic Recalculation & Read-Only Lock:**
   - In `BarcodeRegistrationDialog.xaml` and `QuickBarcodeRegistrationDialog.xaml`:
     - Set `TxtPengali` to `IsReadOnly="True"` (locked).
     - On item selection and on `CboSatuan` selection change, automatically recalculate `Pengali` using `selectedBarang.GetConversionForUnit(selectedSatuan)`.
4. **Maintenance Cleanup:**
   - Exclude `BtrReturGudang.Winform` and `BtrReturGudang.Winform.Tests` from `BtrReturGudang.sln`.

### Advantages
- Completely eliminates manual data entry and operator conversion errors.
- Dynamic recalculation ensures selecting "Dus" sets `Konversi` (e.g. 24) and selecting "Pcs" sets `1`.
- Clean separation: read-only UI protects data integrity.

### Disadvantages
- Operators cannot manually override conversion for damaged/split units (intended business constraint).

---

# 8. Gap Closure

## GAP-001 / GAP-002: Local Persistence & Deserialization Standardization
- **Decision:** Add `Konversi INT NOT NULL DEFAULT 1` to `BTRG_Barang` via migration script `004_Add_Barang_Konversi_Column.sql` and update `001_CreateWarehouseTables.sql`. Update `Barang` entity, `BarangDto`, `BtradeApiClient.ParseBarang` (with `GetIntProp`), and `MasterDataDal` CRUD operations to persist and retrieve `Konversi`.
- **Rationale:** Aligns local warehouse master cache with cloud `BTRADE_Brg`.
- **Impact:** `BtrReturGudang.SqlDb`, `BtrReturGudang.Domain`, `BtrReturGudang.AppTier`, `BtrReturGudang.Infrastructure`.
- **Resolved Date:** 2026-11-07

## GAP-003 / OQ-001: Dynamic Recalculation on Unit Change
- **Decision:** Implement dynamic recalculation in `BarcodeRegistrationDialog` and `QuickBarcodeRegistrationViewModel`. When an item is selected or when the user changes `CboSatuan`, `Pengali` automatically updates:
  - If `SatBesar` is selected: `Pengali = (barang.Konversi > 0 ? barang.Konversi : 1)`.
  - If `SatKecil` is selected: `Pengali = 1`.
  - Fallback: `1`.
- **Rationale:** Ensures barcodes registered for big packaging (e.g., Dus) receive the master conversion multiplier, while atomic packaging (e.g., Pcs) receives 1.
- **Impact:** `BtrReturGudang.Wpf/Views/BarcodeRegistrationDialog.xaml.cs` and `BtrReturGudang.Wpf/ViewModels/QuickBarcodeRegistrationViewModel.cs`.
- **Resolved Date:** 2026-11-07

## GAP-004 / OQ-002: Enforce Read-Only Multiplier
- **Decision:** Set `TxtPengali` to `IsReadOnly="True"` (focusable for tab navigation, but locked against manual edits) in both `BarcodeRegistrationDialog.xaml` and `QuickBarcodeRegistrationDialog.xaml`.
- **Rationale:** Strictly enforces authoritative master data conversion and prevents operator typos.
- **Impact:** `BtrReturGudang.Wpf/Views/`.
- **Resolved Date:** 2026-11-07

## OQ-003: Exclude Legacy WinForms Project
- **Decision:** Exclude `BtrReturGudang.Winform` and `BtrReturGudang.Winform.Tests` from `BtrReturGudang.sln`.
- **Rationale:** WPF is the sole active desktop runtime; eliminating legacy WinForms from the solution prevents continuous maintenance churn.
- **Impact:** `BtrReturGudang.sln`.
- **Resolved Date:** 2026-11-07

## OQ-004: Manual Item Search Scope
- **Decision:** Leave `ManualItemSearchDialog` as is without adding a `Konversi` column.
- **Rationale:** Keeps scope minimalist, explicit, and focused strictly on barcode registration workflows.
- **Impact:** None (no changes to `ManualItemSearchDialog`).
- **Resolved Date:** 2026-11-07

---

# 9. Architecture Applicability

## Decision

**ARCHITECTURE-REQUIRED**

## Rationale

Database schema migration, DTO contract modification, HTTP client JSON parsing updates, domain entity extension with conversion helper logic, solution file project exclusion, and dual-dialog WPF dynamic binding adjustments require formal technical target-state specification.

---

# 10. Planning Readiness

## Readiness Checklist

- [x] All critical gaps resolved
- [x] All required decisions recorded
- [x] All blocking open questions resolved
- [x] Architecture can be finalized or updated

## Status

**READY-FOR-PLANNING**

## Notes

All feasibility requirements and operator decisions have been resolved. The task is ready for Architecture and Implementation Planning.

---

# 11. References

### Referenced Artifacts
- `docs/issues/IS002 - BTRG-BARCODE-KONVERSI-AUTOFILL-ISSUE.md`
- `docs/issues/IS001-BARCODE-SATUAN-SYNC-FEASIBILITY-ASSESSMENT.md`
- `docs/issues/IS001-BARCODE-SATUAN-SYNC-ARCHITECTURE.md`
- `docs/work/j09-btr-retur-gudang/BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md`
- `docs/foundation/DOMAIN.md`

### Referenced Codebase Locations
- `src/j06-pkl-btrade-api/btrade.sqldb/SalesContext/BTRADE_Brg.sql`
- `src/j09-btr-retur-gudang/BtrReturGudang.SqlDb/Scripts/001_CreateWarehouseTables.sql`
- `src/j09-btr-retur-gudang/BtrReturGudang.Domain/Entities/Barang.cs`
- `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/DTOs/ReferenceDtos.cs`
- `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure/Http/BtradeApiClient.cs`
- `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure/Persistence/MasterDataDal.cs`
- `src/j09-btr-retur-gudang/BtrReturGudang.Wpf/Views/BarcodeRegistrationDialog.xaml`
- `src/j09-btr-retur-gudang/BtrReturGudang.Wpf/Views/BarcodeRegistrationDialog.xaml.cs`
- `src/j09-btr-retur-gudang/BtrReturGudang.Wpf/Views/QuickBarcodeRegistrationDialog.xaml`
- `src/j09-btr-retur-gudang/BtrReturGudang.Wpf/ViewModels/QuickBarcodeRegistrationViewModel.cs`
