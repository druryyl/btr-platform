---
Title: Target Architecture - BTRG_Barang Satuan Synchronization Standardization
Code: IS001-BARCODE-SATUAN-SYNC
Artifact: ARCHITECTURE
Version: 1.0
LastUpdated: 2026-11-07
---

# 1. Overview

This architecture defines the technical realization for standardizing packaging unit synchronization and selection between Head Office / Cloud master data and the local warehouse desktop application (`BtrReturGudang`).

Specifically, it resolves `ISSUE-BARCODE-SATUAN-001` by:
1. Standardizing the local product master cache table `BTRG_Barang` from 3 non-standardized unit columns (`Satuan1`, `Satuan2`, `Satuan3`) to exactly 2 fields (`SatBesar`, `SatKecil`), aligning directly with cloud source `BTRADE_Brg` from `j06-pkl-btrade-api` and distribution master `BTR_BrgSatuan` from `j05-btr-distrib`.
2. Eliminating hardcoded dummy packaging units (`Dus`, `Pak`, `Pcs`, `Ball`, `Lusin`) across the WPF presentation layer (`BtrReturGudang.Wpf`), binding all unit selection ComboBoxes strictly to genuine product packaging units returned by `Barang.GetAvailableUnits()`.

**Referenced BUG ISSUE:** `docs/issues/IS001 - issue-barcode-satuan-sync.md` (`ISSUE-BARCODE-SATUAN-001`)  
**Parent / Baseline ARCHITECTURE:** `docs/work/j09-btr-retur-gudang/BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md` (`BTR-RETUR-DESKTOP-001`)

---

# 2. Architectural Basis

## Business Context

- **DOMAIN:** `docs/foundation/DOMAIN.md` and `docs/work/barcode-registry/BARCODE-REGISTRY-DOMAIN.md`:
  - Product units in distribution master data (`BTR_BrgSatuan`) represent valid commercial packaging hierarchies (`Conversion > 1` for large packaging / *Satuan Besar*, `Conversion = 1` for smallest atomic unit / *Satuan Kecil*).
  - Cloud distribution table `BTRADE_Brg` projects master catalog items into cloud endpoints using `SatBesar` and `SatKecil`.
  - Warehouse intake and barcode registration require operators to map scanned physical barcodes exclusively against valid packaging units configured for the product. Selecting non-existent units causes data discrepancies in upstream return verification and stock adjustment.

## Analysis Input

- **FEASIBILITY-ASSESSMENT:** `docs/issues/IS001-BARCODE-SATUAN-SYNC-FEASIBILITY-ASSESSMENT.md` provides approved decisions:
  - **GAP-001 / OQ-004:** Standardize `BTRG_Barang` schema to `SatBesar VARCHAR(7) NULL` and `SatKecil VARCHAR(7) NULL`. Add idempotent migration script `003_Standardize_Barang_Satuan_Columns.sql` and update baseline script `001_CreateWarehouseTables.sql`.
  - **GAP-002 / OQ-002 / OQ-003:** Purge static hardcoded units (`Dus`, `Pak`, `Pcs`, `Ball`, `Lusin`) from `BarcodeRegistrationDialog.xaml` and `QuickBarcodeRegistrationViewModel`. Keep ComboBox empty/disabled until a master item is picked; upon selection, populate strictly with distinct non-empty units from `barang.GetAvailableUnits()`, defaulting to `SatBesar` (or `SatKecil` if `SatBesar` is blank).
  - **GAP-003 / OQ-001:** Update domain entity `Barang`, DTO `BarangDto`, persistence mapper `MasterDataDal`, and cloud JSON deserializer `BtradeApiClient.ParseBarang` to eliminate `Satuan1..3` and standardize on `SatBesar` and `SatKecil`.
  - **GAP-004:** Align `ManualItemSearchDialog.xaml` grid columns to `Satuan Besar` and `Satuan Kecil`.
  - **OQ-005:** Exclude/unload legacy `BtrReturGudang.Winform` from active builds; focus delivery on `BtrReturGudang.Wpf`.

```text
ISSUE (ISSUE-BARCODE-SATUAN-001)
              +
FEASIBILITY-ASSESSMENT (IS001-BARCODE-SATUAN-SYNC)
              +
PARENT ARCHITECTURE (BTR-RETUR-DESKTOP-001)
              ↓
         ARCHITECTURE
```

---

# 3. Scope

## Included

1. **Database Schema & Migration (`BtrReturGudang.SqlDb`):**
   - Create migration script `003_Standardize_Barang_Satuan_Columns.sql` in `Scripts/` to migrate existing `BTRG_Barang` tables idempotently:
     - Add `SatBesar VARCHAR(7) NULL` and `SatKecil VARCHAR(7) NULL` if missing.
     - Copy existing data (`Satuan1` → `SatBesar`, `Satuan2` → `SatKecil`) where applicable.
     - Drop legacy columns `Satuan1`, `Satuan2`, `Satuan3`.
   - Update baseline script `001_CreateWarehouseTables.sql` for new workstation database initializations.

2. **Domain Model (`BtrReturGudang.Domain`):**
   - Update `Barang` entity:
     - Replace properties `Satuan1`, `Satuan2`, `Satuan3` with `SatBesar` and `SatKecil`.
     - Update constructor parameters to `(string brgId, string brgName, string satBesar = null, string satKecil = null, bool isActive = true)`.
     - Update `GetAvailableUnits()` to return `IEnumerable<string>` containing distinct, non-empty values of `SatBesar` followed by `SatKecil`.

3. **Application Tier (`BtrReturGudang.AppTier`):**
   - Update `BarangDto`:
     - Replace properties `Satuan1`, `Satuan2`, `Satuan3` with `SatBesar` and `SatKecil`.
     - Update `FromDomain(Barang)` and `ToDomain()` mappers.

4. **Infrastructure & Persistence Layer (`BtrReturGudang.Infrastructure`):**
   - Update `BtradeApiClient.ParseBarang(string json)`:
     - Extract `satBesar`/`SatBesar` and `satKecil`/`SatKecil` from cloud JSON response into `BarangDto.SatBesar` and `BarangDto.SatKecil`.
     - Cease reading legacy `satuan3` / `Satuan3`.
   - Update `MasterDataDal`:
     - Update Dapper SQL statements in `BulkUpsertBarang`, `BulkUpsertBarangAsync`, `SearchBarangByNameOrCode`, `SearchBarangByNameOrCodeAsync`, `GetBarangById`, and `GetBarangByIdAsync`.
     - Update internal POCO `BarangRow` and mapper method `MapToBarang` to bind `SatBesar` and `SatKecil`.

5. **WPF Presentation Layer (`BtrReturGudang.Wpf`):**
   - `BarcodeRegistrationDialog.xaml` & `BarcodeRegistrationDialog.xaml.cs`:
     - Remove hardcoded `<ComboBoxItem>` elements from `CboSatuan`.
     - Disable `CboSatuan` until an item is selected from `LstItems`.
     - On item selection, populate `CboSatuan.ItemsSource` with `selected.GetAvailableUnits().ToList()`.
     - Set initial selection to `selected.SatBesar` (or `selected.SatKecil` if `selected.SatBesar` is blank).
     - Update `RegisteredSatuan` property accessor to read selected string directly.
   - `QuickBarcodeRegistrationDialog.xaml` & `QuickBarcodeRegistrationViewModel.cs`:
     - Remove static default items (`Dus`, `Pak`, `Pcs`, `Ball`, `Lusin`) from `PackagingUnits` constructor initialization.
     - Update `UpdatePackagingUnits(Barang barang)` to clear `PackagingUnits` and populate from `barang.GetAvailableUnits()`.
     - Auto-select `barang.SatBesar` (or `barang.SatKecil` if `barang.SatBesar` is empty).
     - In XAML, bind `CboSatuan.IsEnabled` to check that `PackagingUnits.Count > 0` and an item is selected.
   - `ManualItemSearchDialog.xaml` & `ManualItemSearchViewModel.cs`:
     - Replace grid columns `Satuan 1`, `Satuan 2`, `Satuan 3` with two columns: `Satuan Besar` (binding `SatBesar`, width 85) and `Satuan Kecil` (binding `SatKecil`, width 85).
     - Update `UpdatePackagingUnits(Barang barang)` to populate `PackagingUnits` strictly from `barang.GetAvailableUnits()`.
   - `ReturnOrderEntryViewModel.cs`:
     - Update `PopulatePackagingUnits(BarcodeLookupResult lookup)` to harvest available units from `barang.GetAvailableUnits()`.
     - Update `SetSelectedItem(Barang item, string selectedSatuan = null)` default unit selection fallback: prioritize `item.SatBesar`, then `item.SatKecil`, then `item.GetAvailableUnits().FirstOrDefault()`.

6. **Unit & Integration Tests:**
   - Update test fixtures and assertions in `BtrReturGudang.AppTier.Tests` (`MasterSyncDtoTests.cs`), `BtrReturGudang.Infrastructure.Tests` (`BtradeApiClientTests.cs`, `SyncServiceTests.cs`).

## Excluded

- Modifications to Cloud API (`j06-pkl-btrade-api`) endpoints — `GET api/Brg/{serverId}` already delivers `satBesar` and `satKecil`.
- Modifications to Main Office distribution sync (`j07-btrade-sync`) or master data (`j05-btr-distrib`).
- Schema modifications to `BTRG_Barcode` or `BTRG_ReturnOrderItem` (`Satuan` and `SatId` remain `VARCHAR(7)`).
- Legacy WinForms application (`BtrReturGudang.Winform`).

---

# 4. Technical Decisions

| Decision ID | Topic | Technical Realization |
| :--- | :--- | :--- |
| **TD-001** | **Table Schema Alignment** | Standardize `BTRG_Barang` table schema to exactly 2 packaging unit columns: `SatBesar VARCHAR(7) NULL` and `SatKecil VARCHAR(7) NULL`. Drop `Satuan1`, `Satuan2`, `Satuan3`. |
| **TD-002** | **Migration Script Idempotency** | Implement `003_Standardize_Barang_Satuan_Columns.sql` following project migration conventions (`IF EXISTS`, `sp_rename` / `ALTER TABLE ADD`, data backfill, column drop). Update `001_CreateWarehouseTables.sql` to define new columns directly for clean setups. |
| **TD-003** | **Domain Entity Contract** | `Barang` domain entity encapsulates unit logic via `GetAvailableUnits()`: returns an `IEnumerable<string>` yielding `SatBesar` (if non-empty) and `SatKecil` (if non-empty and distinct). Eliminates direct external references to legacy `Satuan1..3`. |
| **TD-004** | **Cloud JSON Deserialization** | `BtradeApiClient.ParseBarang` parses `satBesar`/`SatBesar` and `satKecil`/`SatKecil` using case-insensitive property inspection. Deserialization into `BarangDto` assigns directly to `SatBesar` and `SatKecil`. |
| **TD-005** | **Dynamic UI ComboBox Population** | Purge all hardcoded strings (`Dus`, `Pak`, `Pcs`, `Ball`, `Lusin`) from XAML and ViewModel constructors. ComboBox items are dynamically bound to `barang.GetAvailableUnits()`. When no item is selected or no units exist, ComboBox is disabled. Initial selection defaults to `SatBesar` (or `SatKecil` if `SatBesar` is empty). |
| **TD-006** | **Search Dialog Grid Column Alignment** | `ManualItemSearchDialog.xaml` replaces 3 legacy unit columns with 2 columns: `Satuan Besar` (`SatBesar`) and `Satuan Kecil` (`SatKecil`), with right-aligned text style and 85px column width. |

---

# 5. Component Responsibilities

| Component | Layer / Project | Technical Responsibility |
| :--- | :--- | :--- |
| `003_Standardize_Barang_Satuan_Columns.sql` | `BtrReturGudang.SqlDb` | Executes idempotent schema migration on existing `btrgd` database: adds `SatBesar`/`SatKecil`, migrates existing data, drops `Satuan1..3`. |
| `001_CreateWarehouseTables.sql` | `BtrReturGudang.SqlDb` | Defines baseline DDL table definition for `BTRG_Barang` with `SatBesar VARCHAR(7) NULL` and `SatKecil VARCHAR(7) NULL`. |
| `Barang` | `BtrReturGudang.Domain` | Core aggregate entity; exposes `BrgId`, `BrgName`, `SatBesar`, `SatKecil`, `IsActive`, and encapsulation method `GetAvailableUnits()`. |
| `BarangDto` | `BtrReturGudang.AppTier` | Data transfer contract; exposes `SatBesar`, `SatKecil`, `FromDomain()`, and `ToDomain()`. |
| `BtradeApiClient` | `BtrReturGudang.Infrastructure` | Deserializes `GET api/Brg/{serverId}` JSON payload into `BarangDto` with `SatBesar` and `SatKecil`. |
| `MasterDataDal` | `BtrReturGudang.Infrastructure` | Implements `IReferenceRepo` with Dapper queries targeting `SatBesar` and `SatKecil` in `BTRG_Barang`. |
| `BarcodeRegistrationDialog` | `BtrReturGudang.Wpf` | Modal code-behind registration view; dynamically populates `CboSatuan` items from `selectedBarang.GetAvailableUnits()`. |
| `QuickBarcodeRegistrationViewModel` | `BtrReturGudang.Wpf` | MVVM viewmodel for unmapped barcode dialog; manages `PackagingUnits` collection derived from `SelectedBarang.GetAvailableUnits()`. |
| `ManualItemSearchDialog` / `ViewModel` | `BtrReturGudang.Wpf` | Renders `Satuan Besar` and `Satuan Kecil` in master search DataGrid; synchronizes unit options in `PackagingUnits`. |
| `ReturnOrderEntryViewModel` | `BtrReturGudang.Wpf` | Primary operational entry screen; populates intake unit selector using `barang.GetAvailableUnits()`. |

---

# 6. Integration Design

| Source | Target | Interaction / Protocol | Purpose |
| :--- | :--- | :--- | :--- |
| Cloud API `GET api/Brg/{serverId}` | `BtradeApiClient.ParseBarang` | JSON payload over HTTPS | Ingests cloud catalog items containing `satBesar` and `satKecil`. |
| `SyncService.PullReferenceDeltasAsync` | `MasterDataDal.BulkUpsertBarangAsync` | In-process C# call | Transfers downloaded `BarangDto` items to persistence. |
| `MasterDataDal` | Local SQL Server `BTRG_Barang` | T-SQL via Dapper / `SqlConnection` | Upserts product records using columns `SatBesar` and `SatKecil`. |
| `QuickBarcodeRegistrationViewModel` | `Barang.GetAvailableUnits()` | In-process domain method call | Harvests distinct non-empty units to bind `PackagingUnits`. |
| `BarcodeRegistrationDialog.xaml.cs` | `Barang.GetAvailableUnits()` | In-process domain method call | Sets `CboSatuan.ItemsSource` upon picking item in list. |
| `ReturnOrderEntryViewModel` | `IReferenceRepo.GetBarangById` | In-process async repository query | Retrieves master `Barang` to load units via `GetAvailableUnits()`. |

---

# 7. Data Ownership

| Data Element | Authoritative Owner | Cache / Local Consumer |
| :--- | :--- | :--- |
| Product Unit Definitions (`SatBesar`, `SatKecil`, Multipliers) | Head Office `BTR_BrgSatuan` (`j05-btr-distrib`) | Published to Cloud `BTRADE_Brg`; cached locally in `BTRG_Barang` (`btrgd`). |
| Local Product Master Cache (`BTRG_Barang`) | Local `btrgd` database (`BtrReturGudang.SqlDb`) | Managed by `MasterDataDal`; read-only for operational WPF views. |
| Available Units Collection (`PackagingUnits`) | Active WPF ViewModel / View | Temporary view state derived from `Barang.GetAvailableUnits()`. |
| Registered Barcode Satuan (`BTRG_Barcode.Satuan`) | `BTRG_Barcode` table in local `btrgd` | Minted by operator selecting from valid product units during barcode registration. |

---

# 8. Database Design

## Modified Tables

### `BTRG_Barang`

| Column | Change Type | Data Type | Nullable | Description |
| :--- | :--- | :--- | :--- | :--- |
| `SatBesar` | **ADD** | `VARCHAR(7)` | YES | Large packaging unit (`Conversion > 1`), e.g., `Dus`, `Ball`, `Pak`. |
| `SatKecil` | **ADD** | `VARCHAR(7)` | YES | Smallest atomic packaging unit (`Conversion = 1`), e.g., `Pcs`, `Btl`, `Sachet`. |
| `Satuan1` | **DROP** | `VARCHAR(7)` | YES | Legacy column; values migrated to `SatBesar` prior to drop. |
| `Satuan2` | **DROP** | `VARCHAR(7)` | YES | Legacy column; values migrated to `SatKecil` prior to drop. |
| `Satuan3` | **DROP** | `VARCHAR(7)` | YES | Legacy column; dropped. |

## Relationships

`BTRG_Barang` is a local read-only cache table refreshed from Cloud. It carries no foreign key constraints. Local tables `BTRG_Barcode` and `BTRG_ReturnOrderItem` reference `BrgId` logically.

## Migration Considerations

1. **Idempotent Migration Script (`003_Standardize_Barang_Satuan_Columns.sql`):**
   ```sql
   IF EXISTS (SELECT * FROM sys.tables WHERE name = 'BTRG_Barang' AND type = 'U')
   BEGIN
       -- 1. Add SatBesar if missing
       IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_Barang') AND name = 'SatBesar')
       BEGIN
           ALTER TABLE BTRG_Barang ADD SatBesar VARCHAR(7) NULL;
       END;

       -- 2. Add SatKecil if missing
       IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_Barang') AND name = 'SatKecil')
       BEGIN
           ALTER TABLE BTRG_Barang ADD SatKecil VARCHAR(7) NULL;
       END;

       -- 3. Copy legacy data if legacy columns exist
       IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_Barang') AND name = 'Satuan1')
       BEGIN
           EXEC('UPDATE BTRG_Barang SET SatBesar = Satuan1 WHERE SatBesar IS NULL AND Satuan1 IS NOT NULL;');
       END;

       IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_Barang') AND name = 'Satuan2')
       BEGIN
           EXEC('UPDATE BTRG_Barang SET SatKecil = Satuan2 WHERE SatKecil IS NULL AND Satuan2 IS NOT NULL;');
       END;

       -- 4. Drop legacy columns
       IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_Barang') AND name = 'Satuan1')
       BEGIN
           ALTER TABLE BTRG_Barang DROP COLUMN Satuan1;
       END;

       IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_Barang') AND name = 'Satuan2')
       BEGIN
           ALTER TABLE BTRG_Barang DROP COLUMN Satuan2;
       END;

       IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_Barang') AND name = 'Satuan3')
       BEGIN
           ALTER TABLE BTRG_Barang DROP COLUMN Satuan3;
       END;
   END;
   ```

2. **Rollout & Backfill:**
   The script executes automatically during local database initialization on client startup via `DbInitializer` / `UpgradeScripts`. Next periodic master data sync (`ISyncService.PullReferenceDeltasAsync`) will repopulate the table with fresh `SatBesar` and `SatKecil` records from the cloud.

---

# 9. Cross-Cutting Concerns

| Concern | Treatment |
| :--- | :--- |
| **Data Integrity** | Barcodes registered through the UI will only carry units that genuinely exist in product master data (`BTR_BrgSatuan`), preventing invalid unit errors when return orders sync upstream. |
| **UI Ergonomics & 720p Compatibility** | In accordance with TD-013 (`BTR-RETUR-DESKTOP-001`), ComboBox sizes and dialog dimensions (580x480) remain strictly within low-res warehouse viewport constraints. Dynamic ComboBox population does not cause UI reflow or layout distortion. |
| **Defensive Fallback & Data Quality** | If a product in the cloud master data has neither `SatBesar` nor `SatKecil` populated (data quality gap at HO), `GetAvailableUnits()` returns an empty sequence. The registration UI disables unit selection and prompts the operator to contact the system administrator, rather than allowing invalid entries. |
| **Backward Compatibility** | Existing `BTRG_Barcode` records remain untouched; barcode registration request payload formats sent to Cloud remain compliant. |

---

# 10. Implementation Constraints

1. **Target Framework:** All changes must compile and run within `.NET Framework 4.7.1` (C# 7.3 / 8.0 language compatibility).
2. **Dapper Queries:** Direct parameterized SQL queries only; no Entity Framework or query generators.
3. **Encapsulation:** External callers must never attempt to access packaging units via indexed properties (`Satuan1`, `Satuan2`, etc.); callers must use `barang.GetAvailableUnits()`.
4. **No Static Fallback Units in XAML:** XAML views must not declare static `<ComboBoxItem>` values for units; ComboBoxes must rely entirely on dynamic binding.
5. **No WinForms Churn:** `BtrReturGudang.Winform` must not be updated; changes are restricted to `BtrReturGudang.Wpf`, `Domain`, `AppTier`, `Infrastructure`, and `SqlDb`.

---

# 11. Acceptance Conditions

1. Baseline table script `001_CreateWarehouseTables.sql` specifies `SatBesar VARCHAR(7) NULL` and `SatKecil VARCHAR(7) NULL` on table `BTRG_Barang`, without `Satuan1`, `Satuan2`, or `Satuan3`.
2. Migration script `003_Standardize_Barang_Satuan_Columns.sql` successfully updates existing databases idempotently, transferring legacy data and dropping old columns without errors.
3. Domain entity `Barang` has properties `SatBesar` and `SatKecil`, and `GetAvailableUnits()` returns an enumerable of distinct non-empty units.
4. `BtradeApiClient.ParseBarang` correctly deserializes `satBesar` and `satKecil` from cloud JSON payload into `BarangDto`.
5. `MasterDataDal` performs all insert, update, select, and search operations against `BTRG_Barang` using `SatBesar` and `SatKecil`.
6. In `BarcodeRegistrationDialog.xaml` (and `.xaml.cs`), `CboSatuan` contains zero hardcoded ComboBox items, remains disabled until an item is selected, and populates strictly with the selected item's `GetAvailableUnits()`, defaulting to `SatBesar` (or `SatKecil`).
7. In `QuickBarcodeRegistrationDialog.xaml` (and `QuickBarcodeRegistrationViewModel.cs`), `PackagingUnits` is initialized empty, populates strictly from `SelectedBarang.GetAvailableUnits()`, and auto-selects `SatBesar` (or `SatKecil`).
8. In `ManualItemSearchDialog.xaml`, the DataGrid columns display `Satuan Besar` and `Satuan Kecil` instead of `Satuan 1`, `Satuan 2`, and `Satuan 3`.
9. `ReturnOrderEntryViewModel` populates units via `barang.GetAvailableUnits()`, and default unit selection resolves to `item.SatBesar` or `item.SatKecil`.
10. All existing automated test suites in `BtrReturGudang.AppTier.Tests` and `BtrReturGudang.Infrastructure.Tests` pass with updated fixtures.
