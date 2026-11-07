---
Title: Feasibility Assessment - BTRG_Barang Satuan Synchronization Standardization
Code: IS001-BARCODE-SATUAN-SYNC
Artifact: FEASIBILITY-ASSESSMENT
Version: 1.0
LastUpdated: 2026-11-07
Status: READY-FOR-PLANNING
---

# 1. Request Summary

Assessment of issue `ISSUE-BARCODE-SATUAN-001` (recorded in `docs/issues/IS001 - issue-barcode-satuan-sync.md`).

Referenced artifacts:
- `docs/foundation/PRODUCT.md`
- `docs/foundation/DOMAIN.md`
- `docs/foundation/LANDSCAPE.md`
- `docs/foundation/WORKFLOW.md`
- `docs/work/j09-btr-retur-gudang/BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md`

## Objective

Assess the technical feasibility, schema impact, synchronization mapping, and UI adjustments required to standardize `BTRG_Barang` product units to exactly 2 fields (`SatBesar`, `SatKecil`), matching `BTRADE_Brg` from `j05-btr-distrib` / `j06-pkl-btrade-api`, and ensuring the Satuan combo box in `BtrReturGudang.Wpf` displays strictly the product's valid registered packaging units.

---

# 2. Current State

## Existing Behavior

1. **Source & Cloud Layer (`j05-btr-distrib`, `j07-btrade-sync`, `j06-pkl-btrade-api`):**
   - Source table `BTR_BrgSatuan` holds packaging units and conversion multipliers.
   - Synchronization job `j07-btrade-sync` (`BrgDal.cs`) maps items to cloud table `BTRADE_Brg`:
     - `SatBesar`: derived from `BTR_BrgSatuan` where `Conversion > 1`.
     - `SatKecil`: derived from `BTR_BrgSatuan` where `Conversion = 1`.
   - Cloud API `GET /api/Brg/{serverId}` returns `BrgType` containing `satBesar` and `satKecil`.

2. **Warehouse Client Database (`BtrReturGudang.SqlDb`):**
   - Table `BTRG_Barang` is defined in `001_CreateWarehouseTables.sql` with 3 unit fields: `Satuan1 VARCHAR(7)`, `Satuan2 VARCHAR(7)`, and `Satuan3 VARCHAR(7)`.
   - Columns are non-standardized compared to cloud source table `BTRADE_Brg`.

3. **Data Access & DTO Layer (`BtrReturGudang.Domain`, `AppTier`, `Infrastructure`):**
   - Domain entity `Barang` defines properties `Satuan1`, `Satuan2`, `Satuan3`.
   - DTO `BarangDto` defines properties `Satuan1`, `Satuan2`, `Satuan3`.
   - `BtradeApiClient.ParseBarang` parses `satBesar`, `satKecil`, and `satuan3` into `Satuan1..3`.
   - `MasterDataDal` performs bulk upsert and queries on columns `Satuan1`, `Satuan2`, `Satuan3`.

4. **User Interface (`BtrReturGudang.Wpf`):**
   - `BarcodeRegistrationDialog.xaml` hardcodes static items: `<ComboBoxItem Content="Dus" />`, `Pak`, `Pcs`, `Ball`, `Lusin`. In code-behind (`BarcodeRegistrationDialog.xaml.cs`), it attempts to match `selected.Satuan1` against those static options; if a product has a different unit (e.g., `KG`, `BTL`, `SACHET`, `CAN`), it cannot be selected.
   - `QuickBarcodeRegistrationViewModel` initializes `PackagingUnits` with default strings `{"Dus", "Pak", "Pcs", "Ball", "Lusin"}`. When an item is selected, `UpdatePackagingUnits` populates from `Satuan1..3`, but retains dummy fallback values if units are unaligned.
   - `ManualItemSearchDialog.xaml` renders three columns for `Satuan 1`, `Satuan 2`, and `Satuan 3`.
   - `ReturnOrderEntryViewModel` inspects `Satuan1..3` with a fallback to `"Pcs"`.

## Existing Constraints

1. **Local Offline-First SQLite/SQL Server Cache:** `BTRG_Barang` is a local read-only cache table in `btrgd`. It has no foreign keys referencing it and is refreshed via reference delta sync (`ISyncService.PullReferenceDeltasAsync`).
2. **Compact Viewport (1280x720 / 580x480):** Barcode registration dialogs operate in low-resolution warehouse touch/screen environments where combobox layout and keyboard navigation (Enter/Tab) must remain responsive and ergonomic.

---

# 3. Gap Analysis

| ID | Severity | Gap |
| :--- | :--- | :--- |
| **GAP-001** | CRITICAL | **Schema & Column Inconsistency:** `BTRG_Barang` defines 3 unit fields (`Satuan1`, `Satuan2`, `Satuan3`) instead of standardizing on the 2 fields provided by cloud source `BTRADE_Brg` (`SatBesar`, `SatKecil`). |
| **GAP-002** | CRITICAL | **Hardcoded & Inaccurate UI Units:** `BarcodeRegistrationDialog.xaml` hardcodes generic units (`Dus`, `Pak`, `Pcs`, `Ball`, `Lusin`), preventing operators from selecting actual registered units from `BTR_BrgSatuan`. |
| **GAP-003** | MAJOR | **Domain & DTO Misalignment:** `Barang` domain entity, `BarangDto`, and `MasterDataDal` carry 3 unit properties, proliferating non-standardized field names across the solution. |
| **GAP-004** | MINOR | **Redundant Display in Search Dialogs:** `ManualItemSearchDialog.xaml` displays `Satuan 1`, `Satuan 2`, and `Satuan 3` columns instead of standard `Satuan Besar` and `Satuan Kecil`. |

---

# 4. Open Questions

| ID | Question | Impact | Status |
| :--- | :--- | :--- | :--- |
| **OQ-001** | What is the scope of standardizing units across the WPF application? | Determines whether changes are confined to barcode registration dialogs or applied across all WPF views. | **CLOSED** |
| **OQ-002** | How should the Satuan ComboBox populate when an item is selected? | Governs whether fallback/dummy units are preserved or purged. | **CLOSED** |
| **OQ-003** | What should the ComboBox state and default selection be before and after picking an item? | Governs UX flow, enabling/disabling, and initial selected index. | **CLOSED** |
| **OQ-004** | How should local warehouse database (`btrgd`) migration be performed? | Determines migration script structure for `BtrReturGudang.SqlDb`. | **CLOSED** |
| **OQ-005** | How should legacy WinForms code (`BtrReturGudang.Winform`) be handled? | Determines solution build scope and maintenance boundary. | **CLOSED** |

---

# 5. Assumptions

| ID | Assumption |
| :--- | :--- |
| **ASM-001** | Cloud API `GET /api/Brg/{serverId}` already correctly returns `satBesar` and `satKecil` from `BTRADE_Brg`. No backend Cloud API or distribution sync code modifications are needed. |
| **ASM-002** | Master Data in `BTR_BrgSatuan` defines at most two operational unit levels for distribution (`SatBesar` with `Conversion > 1` and `SatKecil` with `Conversion = 1`). Products with a single packaging unit have `SatBesar` blank or equal to `SatKecil`. |
| **ASM-003** | `BTRG_Barang` is strictly a local reference cache and can be safely migrated using idempotent `ALTER TABLE` / `sp_rename` scripts without risking transactional data loss. |

---

# 6. Risks

| ID | Risk | Impact | Mitigation |
| :--- | :--- | :--- | :--- |
| **RISK-001** | Existing warehouse workstations running outdated database schemas encounter runtime SQL errors on `SELECT SatBesar, SatKecil`. | Medium | Provide idempotent migration script `003_Standardize_Barang_Satuan_Columns.sql` that runs during DB initialization. |
| **RISK-002** | Product in cloud catalog has both `SatBesar` and `SatKecil` empty (data quality issue at head office). | Low | Domain method `GetAvailableUnits()` returns distinct non-empty units. If none are present, UI disables registration and prompts operator to verify master data. |
| **RISK-003** | Breaking unit and integration tests asserting legacy `Satuan1..3` properties. | Low | Systematically update mock catalogs and assertions in `AppTier.Tests`, `Infrastructure.Tests`, and `Wpf.Tests`. |

---

# 7. Recommendations

## Option A: Full End-to-End Standardization (Recommended)

1. Standardize `BTRG_Barang` to `SatBesar VARCHAR(7) NULL` and `SatKecil VARCHAR(7) NULL`.
2. Add migration script `003_Standardize_Barang_Satuan_Columns.sql` and update `001_CreateWarehouseTables.sql`.
3. Update `Barang` domain entity with `SatBesar`, `SatKecil`, and `GetAvailableUnits()`.
4. Update `BarangDto`, `BtradeApiClient.ParseBarang`, and `MasterDataDal`.
5. Remove all hardcoded dummy units from `QuickBarcodeRegistrationDialog` and `BarcodeRegistrationDialog`.
6. Bind combo boxes strictly to `barang.GetAvailableUnits()`, defaulting to `SatBesar` (or `SatKecil` if `SatBesar` is blank).
7. Update `ManualItemSearchDialog` grid columns to `Satuan Besar` and `Satuan Kecil`.
8. Deprecate / unload WinForms project to prevent unnecessary legacy maintenance churn.

### Advantages
- Complete alignment with Cloud and Head Office data architecture.
- Eliminates impossible-to-select units for non-standard packaging (KG, BTL, CAN, etc.).
- Clean domain model with unified unit harvesting logic (`GetAvailableUnits()`).

### Disadvantages
- Requires updating database migration script and unit test fixtures across multiple projects.

---

# 8. Gap Closure

## GAP-001 / OQ-004: Schema Standardization & Migration
- **Decision:** Standardize `BTRG_Barang` table schema to exactly 2 unit fields: `SatBesar VARCHAR(7) NULL` and `SatKecil VARCHAR(7) NULL`. Implement idempotent migration script `003_Standardize_Barang_Satuan_Columns.sql` to rename/add columns and drop `Satuan1`, `Satuan2`, `Satuan3`. Update baseline script `001_CreateWarehouseTables.sql` for new installations.
- **Rationale:** Aligns local warehouse database directly with cloud table `BTRADE_Brg`.
- **Impact:** `BtrReturGudang.SqlDb/Scripts/`.
- **Resolved Date:** 2026-11-07

## GAP-002 / OQ-002 / OQ-003: UI Unit Selection & Lifecycle
- **Decision:** Remove all hardcoded dummy units (`Dus`, `Pak`, `Pcs`, `Ball`, `Lusin`) from both `QuickBarcodeRegistrationDialog` and `BarcodeRegistrationDialog`. Keep Satuan ComboBox disabled/empty until a product is selected. Upon selection, populate strictly with distinct non-empty units returned by `barang.GetAvailableUnits()`, defaulting selection to `SatBesar` (or `SatKecil` if `SatBesar` is empty).
- **Rationale:** Guarantees barcode registrations always map to genuine units configured in head office `BTR_BrgSatuan`.
- **Impact:** `BtrReturGudang.Wpf` (`Views/` and `ViewModels/`).
- **Resolved Date:** 2026-11-07

## GAP-003 / OQ-001: Comprehensive Domain & UI Alignment
- **Decision:** Apply standardization across all layers and WPF unit displays:
  - `Barang` domain entity: replace `Satuan1..3` with `SatBesar`, `SatKecil`. Provide `GetAvailableUnits()` returning `IEnumerable<string>`.
  - `BarangDto`: replace properties with `SatBesar`, `SatKecil`.
  - `BtradeApiClient.ParseBarang`: parse `satBesar`/`SatBesar` and `satKecil`/`SatKecil`.
  - `MasterDataDal`: execute CRUD using `SatBesar` and `SatKecil`.
  - `ManualItemSearchDialog.xaml`: replace three columns with `Satuan Besar` and `Satuan Kecil`.
  - `ReturnOrderEntryViewModel`: consume `GetAvailableUnits()`.
- **Rationale:** Ensures architectural integrity and eliminates technical debt across the entire desktop warehouse module.
- **Impact:** `BtrReturGudang.Domain`, `AppTier`, `Infrastructure`, `Wpf`.
- **Resolved Date:** 2026-11-07

## OQ-005: WinForms Maintenance Boundary
- **Decision:** Exclude or unload `BtrReturGudang.Winform` from active builds; focus test suites and active delivery exclusively on `BtrReturGudang.Wpf`.
- **Rationale:** WPF is the active production runtime; WinForms is superseded and legacy.
- **Impact:** `BtrReturGudang.sln`.
- **Resolved Date:** 2026-11-07

---

# 9. Architecture Applicability

## Decision

**ARCHITECTURE-REQUIRED**

## Rationale

Cross-layer schema standardization, database migration scripts, domain entity contract updates, DTO mapping changes, and WPF view/viewmodel dynamic unit binding across multiple projects require formal technical realization and clear component implementation boundaries.

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

The task is ready for an executable implementation plan (`IMPLEMENTATION-PLAN`).

---

# 11. References

### Referenced Artifacts
- `docs/issues/IS001 - issue-barcode-satuan-sync.md`
- `docs/work/j09-btr-retur-gudang/BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md`
- `docs/foundation/DOMAIN.md`

### Referenced Codebase Locations
- `src/j09-btr-retur-gudang/BtrReturGudang.SqlDb/Scripts/001_CreateWarehouseTables.sql`
- `src/j09-btr-retur-gudang/BtrReturGudang.Domain/Entities/Barang.cs`
- `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/DTOs/ReferenceDtos.cs`
- `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure/Persistence/MasterDataDal.cs`
- `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure/Http/BtradeApiClient.cs`
- `src/j09-btr-retur-gudang/BtrReturGudang.Wpf/Views/BarcodeRegistrationDialog.xaml`
- `src/j09-btr-retur-gudang/BtrReturGudang.Wpf/Views/QuickBarcodeRegistrationDialog.xaml`
- `src/j09-btr-retur-gudang/BtrReturGudang.Wpf/ViewModels/QuickBarcodeRegistrationViewModel.cs`
- `src/j09-btr-retur-gudang/BtrReturGudang.Wpf/Views/ManualItemSearchDialog.xaml`
