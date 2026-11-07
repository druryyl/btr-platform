---
Title: Target Architecture - Auto-populate Conversion Multiplier in Barcode Registration
Code: IS002-BTRG-BARCODE-KONVERSI-AUTOFILL
Artifact: ARCHITECTURE
Version: 1.0
LastUpdated: 2026-11-07
---

# 1. Overview

This architecture defines the technical realization for storing the product conversion multiplier in the local warehouse database cache (`BTRG_Barang.Konversi`) and automatically populating/recalculating it as a read-only field within the barcode registration dialogs (`BarcodeRegistrationDialog` and `QuickBarcodeRegistrationDialog`) of the warehouse desktop application (`BtrReturGudang.Wpf`).

Specifically, it resolves change request `BTRG-BARCODE-KONVERSI-001` by:
1. Enhancing the local product master cache table `BTRG_Barang` with `Konversi INT NOT NULL CONSTRAINT DF_BTRG_Barang_Konversi DEFAULT 1`, harmonizing schema and synchronization with cloud catalog `BTRADE_Brg` from `j06-pkl-btrade-api` and distribution master `BTR_BrgSatuan` from `j05-btr-distrib`.
2. Extending domain entity `Barang`, DTO `BarangDto`, cloud JSON deserializer `BtradeApiClient`, and persistence mapper `MasterDataDal` to ingest, persist, and expose `Konversi`.
3. Encapsulating unit conversion logic within domain method `Barang.GetConversionForUnit(string satuan)`.
4. Enforcing strict master data integrity by locking `TxtPengali` (`IsReadOnly="True"`) across both `BarcodeRegistrationDialog.xaml` and `QuickBarcodeRegistrationDialog.xaml`, and dynamically updating the multiplier whenever an item is picked or the packaging unit selection changes.
5. Streamlining the solution build configuration by excluding legacy WinForms projects (`BtrReturGudang.Winform`, `BtrReturGudang.Winform.Tests`) from `BtrReturGudang.sln`.

**Referenced CHANGE-REQUEST ISSUE:** `docs/issues/IS002 - BTRG-BARCODE-KONVERSI-AUTOFILL-ISSUE.md` (`BTRG-BARCODE-KONVERSI-001`)  
**Originating Analysis Artifact:** `docs/issues/IS002-BTRG-BARCODE-KONVERSI-AUTOFILL-FEASIBILITY-ASSESSMENT.md` (`IS002-BTRG-BARCODE-KONVERSI-AUTOFILL`)  
**Parent / Baseline ARCHITECTURE:** `docs/work/j09-btr-retur-gudang/BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md` (`BTR-RETUR-DESKTOP-001`)  
**Related ARCHITECTURE:** `docs/issues/IS001-BARCODE-SATUAN-SYNC-ARCHITECTURE.md` (`IS001-BARCODE-SATUAN-SYNC`)

---

# 2. Architectural Basis

## Business Context

- **DOMAIN:** `docs/foundation/PRODUCT.md`, `docs/foundation/DOMAIN.md`, and `docs/work/barcode-registry/BARCODE-REGISTRY-DOMAIN.md`:
  - Distribution master data (`BTR_BrgSatuan`) establishes authoritative commercial packaging hierarchies (`Conversion > 1` for large packaging / *Satuan Besar*, `Conversion = 1` for atomic unit / *Satuan Kecil*).
  - Cloud catalog table `BTRADE_Brg` contains `Konversi INT NOT NULL DEFAULT(0)` populated from master data conversion factor `ISNULL(cc.Conversion, 1)`.
  - In warehouse return intake, barcodes scanned for large packaging (e.g., `Dus`) must inherit the exact master conversion multiplier into `BTRG_Barcode.Pengali`, while atomic packaging (e.g., `Pcs`) defaults to `1`.
  - Allowing operators to enter or override the conversion multiplier introduces severe inventory discrepancy risks during return intake verification and credit note generation.

## Analysis Input

- **FEASIBILITY-ASSESSMENT:** `docs/issues/IS002-BTRG-BARCODE-KONVERSI-AUTOFILL-FEASIBILITY-ASSESSMENT.md` provides approved decisions:
  - **GAP-001 / GAP-002 / TD-001:** Standardize `BTRG_Barang` schema with `Konversi INT NOT NULL CONSTRAINT DF_BTRG_Barang_Konversi DEFAULT 1`. Create idempotent migration script `004_Add_Barang_Konversi_Column.sql` and update baseline `001_CreateWarehouseTables.sql`.
  - **GAP-002 / TD-003:** Update `BtradeApiClient.ParseBarang` to deserialize `konversi` from `GET /api/Brg/{serverId}` JSON payload into `BarangDto.Konversi`, safely defaulting to `1`.
  - **GAP-001 / TD-004:** Extend `Barang` domain model and `BarangDto` with `Konversi`. Implement `Barang.GetConversionForUnit(string satuan)` to calculate conversion dynamically (`Konversi` for `SatBesar`, `1` for `SatKecil`, fallback `1`).
  - **GAP-001 / TD-001:** Update `MasterDataDal` Dapper CRUD queries and internal row mappers to persist and retrieve `Konversi`.
  - **GAP-003 / GAP-004 / OQ-001 / OQ-002 / TD-005 / TD-006:** In `BarcodeRegistrationDialog` and `QuickBarcodeRegistrationDialog`, make `TxtPengali` read-only (`IsReadOnly="True"`). Automatically recalculate `Pengali` whenever a product is selected or `CboSatuan` selection changes.
  - **OQ-003 / TD-007:** Exclude `BtrReturGudang.Winform` and `BtrReturGudang.Winform.Tests` from `BtrReturGudang.sln`.
  - **OQ-004:** Leave `ManualItemSearchDialog` grid unchanged.

```text
CHANGE-REQUEST (BTRG-BARCODE-KONVERSI-001)
                    +
FEASIBILITY-ASSESSMENT (IS002-BTRG-BARCODE-KONVERSI-AUTOFILL)
                    +
PARENT ARCHITECTURE (BTR-RETUR-DESKTOP-001)
                    ↓
               ARCHITECTURE
```

---

# 3. Scope

## Included

1. **Database Schema & Migration (`BtrReturGudang.SqlDb`):**
   - Create migration script `004_Add_Barang_Konversi_Column.sql` in `Scripts/` to migrate existing `BTRG_Barang` tables idempotently:
     - Add column `Konversi INT NOT NULL CONSTRAINT DF_BTRG_Barang_Konversi DEFAULT 1` if absent.
     - Backfill legacy or zero rows: `UPDATE BTRG_Barang SET Konversi = 1 WHERE Konversi <= 0 OR Konversi IS NULL;`.
   - Update baseline script `001_CreateWarehouseTables.sql` so newly provisioned workstation databases declare `Konversi INT NOT NULL CONSTRAINT DF_BTRG_Barang_Konversi DEFAULT 1` directly on `BTRG_Barang`.

2. **Domain Model (`BtrReturGudang.Domain`):**
   - Update `Barang` entity:
     - Add property `public int Konversi { get; private set; }`.
     - Update constructors to accept `int konversi = 1`. If incoming `konversi <= 0`, safely default to `1`.
     - Implement business helper method `public int GetConversionForUnit(string satuan)`:
       - If `satuan` equals `SatBesar` (case-insensitive) and `Konversi > 0`, return `Konversi`.
       - If `satuan` equals `SatKecil` (case-insensitive) or `Konversi <= 1`, return `1`.
       - Fallback: return `1`.

3. **Application Tier (`BtrReturGudang.AppTier`):**
   - Update `BarangDto`:
     - Add property `public int Konversi { get; set; } = 1;`.
     - Update `FromDomain(Barang barang)` to assign `Konversi = barang.Konversi`.
     - Update `ToDomain()` to pass `Konversi` to the `Barang` constructor.

4. **Infrastructure & Persistence Layer (`BtrReturGudang.Infrastructure`):**
   - Update `BtradeApiClient`:
     - Add private helper `GetIntProp(JsonElement elem, int defaultValue = 1, params string[] propNames)` supporting numeric tokens or numeric string parsing.
     - Update `ParseBarang(string json)` to extract `konversi`/`Konversi` into `BarangDto.Konversi`, defaulting to `1` when missing, zero, or negative.
   - Update `MasterDataDal`:
     - Update Dapper queries for `BTRG_Barang`:
       - `BulkUpsertBarang` & `BulkUpsertBarangAsync`: include `Konversi = @Konversi` in `UPDATE` and `Konversi` / `@Konversi` in `INSERT`.
       - `SearchBarangByNameOrCode` & `SearchBarangByNameOrCodeAsync`: add `Konversi` to `SELECT` statement.
       - `GetBarangById` & `GetBarangByIdAsync`: add `Konversi` to `SELECT` statement.
     - Update private class `BarangRow`: add `public int Konversi { get; set; }`.
     - Update private mapper `MapToBarang(BarangRow row)`: pass `row.Konversi` to `Barang` instantiation.

5. **WPF Presentation Layer (`BtrReturGudang.Wpf`):**
   - `BarcodeRegistrationDialog.xaml` & `BarcodeRegistrationDialog.xaml.cs`:
     - In XAML: set `TxtPengali` to `IsReadOnly="True"`, `Focusable="True"`, with read-only background styling (`#F8FAFC`). Wire `SelectionChanged="OnSatuanSelectionChanged"` on `CboSatuan`.
     - In code-behind: implement helper `UpdatePengali()` that computes `_selectedBarang?.GetConversionForUnit(RegisteredSatuan) ?? 1` and updates `TxtPengali.Text`.
     - Invoke `UpdatePengali()` on item selection in `OnItemSelected` and unit selection in `OnSatuanSelectionChanged`.
   - `QuickBarcodeRegistrationDialog.xaml` & `QuickBarcodeRegistrationViewModel.cs`:
     - In XAML: set `TxtPengali` (`controls:NumericTextBox`) to `IsReadOnly="True"`.
     - In `QuickBarcodeRegistrationViewModel.cs`:
       - Whenever `SelectedBarang` changes, automatically recompute `Pengali = SelectedBarang?.GetConversionForUnit(SelectedSatuan) ?? 1m`.
       - Whenever `SelectedSatuan` changes, automatically recompute `Pengali = SelectedBarang?.GetConversionForUnit(SelectedSatuan) ?? 1m`.
       - Default `Pengali` to `1m` if `SelectedBarang` is null or unit is atomic.

6. **Solution Configuration (`BtrReturGudang.sln`):**
   - Remove `BtrReturGudang.Winform` and `BtrReturGudang.Winform.Tests` project configurations and build entries from `BtrReturGudang.sln`.

7. **Unit & Integration Tests:**
   - Update `BarangTests.cs` in `BtrReturGudang.Domain.Tests` to verify `Konversi` assignment and `GetConversionForUnit` evaluation under various unit configurations.
   - Update `MasterSyncDtoTests.cs` in `BtrReturGudang.AppTier.Tests` to verify `BarangDto` round-trip domain mapping with `Konversi`.
   - Update `BtradeApiClientTests.cs` in `BtrReturGudang.Infrastructure.Tests` to verify `ParseBarang` correctly parses `konversi` from JSON responses.
   - Add/update ViewModel and control tests in `BtrReturGudang.Wpf.Tests` covering `QuickBarcodeRegistrationViewModel` dynamic `Pengali` recalculation.

## Excluded

- Modifications to Cloud API `j06-pkl-btrade-api` endpoints — `GET /api/Brg/{serverId}` already delivers `konversi`.
- Modifications to Main Office distribution sync `j07-btrade-sync` or master data `j05-btr-distrib`.
- Modifications to table schemas for `BTRG_Barcode` or `BTRG_ReturnOrderItem` (`Pengali DECIMAL(18,2)` remains unchanged).
- Modifications to `ManualItemSearchDialog.xaml` (retains existing columns; conversion display is excluded per OQ-004).
- Modifications or bug fixes to legacy WinForms code (`BtrReturGudang.Winform`).

---

# 4. Technical Decisions

| Decision ID | Topic | Technical Realization |
| :--- | :--- | :--- |
| **TD-001** | **Table Schema & Default Constraint** | Add `Konversi INT NOT NULL CONSTRAINT DF_BTRG_Barang_Konversi DEFAULT 1` to `BTRG_Barang`. If legacy or invalid records exist, backfill with `1`. Update baseline `001_CreateWarehouseTables.sql` with the same constraint. |
| **TD-002** | **Migration Script Idempotency** | Implement `004_Add_Barang_Konversi_Column.sql` with standard idempotency guards (`IF NOT EXISTS sys.columns ... ALTER TABLE ADD`). Ensure execution is safe against existing warehouse workstations. |
| **TD-003** | **Cloud JSON Deserialization** | Implement private static method `GetIntProp(JsonElement elem, int defaultValue = 1, params string[] propNames)` in `BtradeApiClient`. Safely handles JSON integer tokens (`GetInt32()`) and numeric string tokens (`int.TryParse()`). If missing, zero, or negative, returns `defaultValue` (`1`). |
| **TD-004** | **Domain Entity Encapsulation** | `Barang` domain entity exposes `Konversi` (integer) and method `int GetConversionForUnit(string satuan)`. The method compares `satuan` against `SatBesar` and `SatKecil` (using `StringComparison.OrdinalIgnoreCase`). If matching `SatBesar` and `Konversi > 0`, it returns `Konversi`; if matching `SatKecil` or `Konversi <= 1`, it returns `1`; otherwise fallback `1`. |
| **TD-005** | **Reactive & Dynamic UI Autofill** | In both `BarcodeRegistrationDialog` and `QuickBarcodeRegistrationViewModel`, selecting an item or changing the packaging unit ComboBox instantly updates the conversion multiplier via `selectedBarang.GetConversionForUnit(selectedSatuan)`. No manual button click or refresh is required. |
| **TD-006** | **Operator Override Elimination & Read-Only Lock** | Set `TxtPengali` to `IsReadOnly="True"` across both registration dialogs (`BarcodeRegistrationDialog.xaml` and `QuickBarcodeRegistrationDialog.xaml`). Multipliers are authoritative master data; operators cannot type arbitrary numbers. Control remains focusable for keyboard Tab navigation. |
| **TD-007** | **Legacy Solution Build Decoupling** | Exclude `BtrReturGudang.Winform` and `BtrReturGudang.Winform.Tests` from `BtrReturGudang.sln`. Eliminates legacy build failures and ensures continuous integration focuses solely on WPF and core shared libraries. |

---

# 5. Component Responsibilities

| Component | Layer / Project | Technical Responsibility |
| :--- | :--- | :--- |
| `004_Add_Barang_Konversi_Column.sql` | `BtrReturGudang.SqlDb` | Executes idempotent DDL schema migration adding `Konversi INT NOT NULL DEFAULT 1` and backfilling non-positive values to `1`. |
| `001_CreateWarehouseTables.sql` | `BtrReturGudang.SqlDb` | Defines baseline DDL table definition for `BTRG_Barang` with `Konversi INT NOT NULL CONSTRAINT DF_BTRG_Barang_Konversi DEFAULT 1`. |
| `Barang` | `BtrReturGudang.Domain` | Core domain entity; holds `Konversi` and encapsulates conversion resolution via `GetConversionForUnit(string satuan)`. |
| `BarangDto` | `BtrReturGudang.AppTier` | Data transfer contract; exposes `Konversi`, `FromDomain()`, and `ToDomain()`. |
| `BtradeApiClient` | `BtrReturGudang.Infrastructure` | Deserializes `GET /api/Brg/{serverId}` JSON payload into `BarangDto` with `Konversi` using `GetIntProp`. |
| `MasterDataDal` | `BtrReturGudang.Infrastructure` | Implements `IReferenceRepo` with Dapper queries reading and writing `Konversi` in `BTRG_Barang`. |
| `BarcodeRegistrationDialog` | `BtrReturGudang.Wpf` | Code-behind modal dialog; enforces `TxtPengali.IsReadOnly = true` and updates multiplier on item/unit selection change. |
| `QuickBarcodeRegistrationViewModel` | `BtrReturGudang.Wpf` | MVVM viewmodel for unmapped barcode scan; auto-calculates `Pengali` reactively when `SelectedBarang` or `SelectedSatuan` changes. |
| `QuickBarcodeRegistrationDialog.xaml` | `BtrReturGudang.Wpf` | Presentation view; binds `TxtPengali` (`controls:NumericTextBox`) with `IsReadOnly="True"`. |
| `BtrReturGudang.sln` | Root Solution | Defines solution project membership; excludes legacy WinForms projects. |

---

# 6. Integration Design

| Source | Target | Interaction / Protocol | Purpose |
| :--- | :--- | :--- | :--- |
| Cloud API `GET /api/Brg/{serverId}` | `BtradeApiClient.ParseBarang` | JSON payload over HTTPS | Ingests cloud catalog items containing `konversi` alongside `satBesar` and `satKecil`. |
| `SyncService.PullReferenceDeltasAsync` | `MasterDataDal.BulkUpsertBarangAsync` | In-process C# call | Transfers downloaded `BarangDto` items to local persistence with `Konversi`. |
| `MasterDataDal` | Local SQL Server `BTRG_Barang` | T-SQL via Dapper / `SqlConnection` | Upserts product records using column `Konversi`. |
| `QuickBarcodeRegistrationViewModel` | `Barang.GetConversionForUnit(string)` | In-process domain method call | Resolves dynamic conversion multiplier to bind `Pengali`. |
| `BarcodeRegistrationDialog.xaml.cs` | `Barang.GetConversionForUnit(string)` | In-process domain method call | Sets `TxtPengali.Text` upon item selection or ComboBox change. |
| `BarcodeRegistrationDialog` / `QuickBarcodeRegistrationViewModel` | `IBarcodeService.QuickRegisterUnmappedBarcodeAsync` | In-process async service call | Registers barcode with resolved authoritative `pengali`. |

---

# 7. Data Ownership

| Data Element | Authoritative Owner | Cache / Local Consumer |
| :--- | :--- | :--- |
| Item Conversion Ratio (`Konversi`) | Head Office `BTR_BrgSatuan` (`j05-btr-distrib`) | Published to Cloud `BTRADE_Brg`; cached locally in `BTRG_Barang.Konversi` (`btrgd`). |
| Local Product Master Cache (`BTRG_Barang`) | Local `btrgd` database (`BtrReturGudang.SqlDb`) | Managed by `MasterDataDal`; read-only for operational WPF views. |
| Displayed Registration Multiplier (`TxtPengali` / `Pengali`) | Barcode Registration View / ViewModel | Dynamically derived from `Barang.GetConversionForUnit(satuan)`. Read-only to operators. |
| Registered Barcode Multiplier (`BTRG_Barcode.Pengali`) | `BTRG_Barcode` table in local `btrgd` | Minted upon saving barcode registration using authoritative multiplier. |

---

# 8. Database Design

## Modified Tables

### `BTRG_Barang`

| Column | Change Type | Data Type | Nullable | Default | Description |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `Konversi` | **ADD** | `INT` | NO | `1` | Conversion multiplier from `SatBesar` to `SatKecil`. Defaults to `1`. |

## Relationships

`BTRG_Barang` remains a local read-only cache table refreshed from Cloud. It carries no foreign key constraints. Local tables `BTRG_Barcode` and `BTRG_ReturnOrderItem` reference `BrgId` logically.

## Migration Considerations

1. **Idempotent Migration Script (`004_Add_Barang_Konversi_Column.sql`):**
   ```sql
   -- ============================================================================
   -- Script: 004_Add_Barang_Konversi_Column.sql
   -- Description: Upgrade BTRG_Barang table to add Konversi column for auto-populating
   --              conversion multiplier in barcode registration dialogs.
   -- ============================================================================

   IF EXISTS (SELECT * FROM sys.tables WHERE name = 'BTRG_Barang' AND type = 'U')
   BEGIN
       -- 1. Add Konversi column with default constraint if missing
       IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_Barang') AND name = 'Konversi')
       BEGIN
           ALTER TABLE BTRG_Barang 
           ADD Konversi INT NOT NULL CONSTRAINT DF_BTRG_Barang_Konversi DEFAULT 1;
       END;

       -- 2. Backfill existing rows with invalid or zero conversion
       UPDATE BTRG_Barang 
       SET Konversi = 1 
       WHERE Konversi <= 0 OR Konversi IS NULL;
   END;
   ```

2. **Baseline Script Update (`001_CreateWarehouseTables.sql`):**
   Update table definition of `BTRG_Barang`:
   ```sql
   -- 5. BTRG_Barang (Master Cache)
   IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BTRG_Barang' AND type = 'U')
   BEGIN
       CREATE TABLE BTRG_Barang (
           BrgId            VARCHAR(20)   NOT NULL PRIMARY KEY,
           BrgName          VARCHAR(100)  NOT NULL,
           SatBesar         VARCHAR(7)    NULL,
           SatKecil         VARCHAR(7)    NULL,
           Konversi         INT           NOT NULL CONSTRAINT DF_BTRG_Barang_Konversi DEFAULT 1,
           IsActive         BIT           NOT NULL CONSTRAINT DF_BTRG_Barang_IsActive DEFAULT 1
       );
   END;
   ```

3. **Rollout & Backfill:**
   The migration script executes automatically on client startup via `DbInitializer` / `UpgradeScripts`. The subsequent periodic master data sync (`ISyncService.PullReferenceDeltasAsync`) repopulates `BTRG_Barang` with fresh `Konversi` values downloaded from Cloud.

---

# 9. Cross-Cutting Concerns

| Concern | Treatment |
| :--- | :--- |
| **Data Integrity & Fraud Prevention** | Locking `TxtPengali` as read-only eliminates human error and intentional alteration of conversion multipliers during barcode intake. Barcodes are registered strictly with authoritative master data ratios. |
| **UI Ergonomics & 720p Compatibility** | Dialog dimensions (580x480) and control arrangements are preserved. Making `TxtPengali` read-only changes only visual state and user editability without causing reflow or breaking Tab navigation. |
| **Defensive Anomaly Handling** | If a product record has `Konversi <= 0`, NULL, or missing in cloud sync data, domain method `GetConversionForUnit` and deserializer `GetIntProp` safely fall back to `1`, preventing runtime divide-by-zero or non-positive multiplier exceptions. |
| **Backward Compatibility** | Existing `BTRG_Barcode` records remain valid; barcode registration upload payload to Cloud (`POST api/barcode-sync`) remains unchanged. |

---

# 10. Implementation Constraints

1. **Target Framework:** All changes must compile and run within `.NET Framework 4.7.1` (C# 7.3 / 8.0 language compatibility).
2. **Dapper Persistence:** Direct parameterized SQL queries only; no Entity Framework or query generators.
3. **Domain Encapsulation:** ViewModels and code-behind views must not perform ad-hoc conversion arithmetic; all multiplier resolution must route through `Barang.GetConversionForUnit(satuan)`.
4. **Read-Only Enforcement in XAML:** `TxtPengali` must have `IsReadOnly="True"` defined in both XAML files to guarantee read-only behavior regardless of code execution paths.
5. **Zero WinForms Maintenance:** `BtrReturGudang.Winform` and its test project must be decoupled from `BtrReturGudang.sln`. No code changes should be applied to WinForms projects.

---

# 11. Acceptance Conditions

1. Baseline table script `001_CreateWarehouseTables.sql` declares `Konversi INT NOT NULL CONSTRAINT DF_BTRG_Barang_Konversi DEFAULT 1` on `BTRG_Barang`.
2. Migration script `004_Add_Barang_Konversi_Column.sql` successfully updates existing databases idempotently, adding the column and backfilling non-positive values to `1`.
3. Domain entity `Barang` exposes property `Konversi` (integer) and domain method `GetConversionForUnit(string satuan)`.
4. `Barang.GetConversionForUnit(satuan)` returns `Konversi` when `satuan` matches `SatBesar` (and `Konversi > 0`), and returns `1` when `satuan` matches `SatKecil` or when `Konversi <= 1`.
5. `BarangDto` carries property `Konversi`, mapping accurately to and from `Barang` domain entity.
6. `BtradeApiClient.ParseBarang` correctly extracts `konversi` from cloud JSON payload into `BarangDto.Konversi`, safely defaulting to `1` when omitted or invalid.
7. `MasterDataDal` performs all upsert, select, and search queries against `BTRG_Barang` including `Konversi`.
8. In `BarcodeRegistrationDialog.xaml` (and `.xaml.cs`), `TxtPengali` is read-only (`IsReadOnly="True"`), and its value automatically updates to match the selected unit conversion when an item is selected or when `CboSatuan` selection changes.
9. In `QuickBarcodeRegistrationDialog.xaml` (and `QuickBarcodeRegistrationViewModel.cs`), `TxtPengali` is read-only (`IsReadOnly="True"`), and `Pengali` automatically updates to match the selected unit conversion when `SelectedBarang` or `SelectedSatuan` changes.
10. `BtrReturGudang.sln` excludes `BtrReturGudang.Winform` and `BtrReturGudang.Winform.Tests`, and `dotnet test src/j09-btr-retur-gudang/BtrReturGudang.sln` compiles and passes all test suites.
