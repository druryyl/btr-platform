# Implementation Summary: BTR Portal API — Milestone 6 (Inventory Dashboard V1)

## Status

Milestone 6 is complete. `GET /api/dashboard/inventory` returns real inventory summary data from existing BTR stock balance reporting sources. Sales (M4) and piutang (M5) dashboards are unchanged. The full solution builds and all verification checks pass.

---

## 1. Investigation Findings

### Inventory reporting landscape

BTR Desktop inventory summaries read from the **denormalized balance table `BTR_StokBalanceWarehouse`**, not from transaction history. Stock quantities are maintained at write time by posting workers; reports consume persisted `Qty` and product `Hpp` from `BTR_Brg`.

| Area | Location | Role |
| --- | --- | --- |
| Stock balance read model | `StokBalanceViewDal` (`IStokBalanceViewDal`) | Lists all products with per-warehouse qty, HPP, units, supplier/kategori dimensions |
| Primary balance table | `BTR_StokBalanceWarehouse` | Persisted qty per `(BrgId, WarehouseId)` — source of truth for balance reports |
| Stock card (movement) | `KartuStokDal` | Transaction-level `BTR_StokMutasi` ledger — drilldown, not summary KPI |
| Stock card summary | `KartuStokSummaryDal` | Period movement aggregates from `BTR_StokMutasi` per warehouse — requires date range + warehouse |
| Periodic stock snapshot | `StokPeriodikDal` | Point-in-time qty from mutasi history — used by Kartu Stok Summary form |
| Stock per supplier | `StokBrgSupplierDal` | Alternate grouping of `BTR_StokBalanceWarehouse` by supplier — not a company-wide total |

### Existing Inventory Info screens

| Screen | DAL | Summary behavior |
| --- | --- | --- |
| **Stok Balance** (`StokBalanceInfoForm`) | `StokBalanceViewDal` | Groups rows by product across warehouses; `NilaiSediaan = Sum(Hpp × Qty)` per product |
| **Stok Balance Info** (`StokBalanceInfo2Form`) | `StokBalanceViewDal` | Per product × warehouse rows; grid footer sums `NilaiSediaan`; **excludes In-Transit warehouse by default** |
| **Kartu Stok** (`KartuStokInfoForm`) | `KartuStokDal` | Movement ledger for one product — not a dashboard total |
| **Kartu Stok Summary** (`KartuStokSummaryForm`) | `KartuStokSummaryDal` + `StokPeriodikDal` | Period/warehouse-scoped movement report with opening/closing values |
| **Stok per Supplier** (`StokBrgSupplierForm`) | `StokBrgSupplierDal` | Supplier-filtered balance view — subset, not company total |

### Existing inventory KPI calculations

| KPI | Existing behavior | Source |
| --- | --- | --- |
| Qty on hand | `BTR_StokBalanceWarehouse.Qty` (persisted) | Written by stock posting, not recalculated in reports |
| Inventory value per row | `Hpp × Qty` → `NilaiSediaan` | `StokBalanceInfo2Form.Proses()` line 90; `StokBalanceInfoForm` group sum |
| Company total value | Sum of `NilaiSediaan` | `StokBalanceInfo2Form` grid summary `{Sum}` on `NilaiSediaan` |
| Product aggregation | Group by `BrgId`, sum qty across warehouses | `StokBalanceInfoForm.Proses()` LINQ group |
| In-Transit exclusion | `WarehouseName != "In-Transit"` when checkbox unchecked (default) | `StokBalanceInfo2Form` default filter |

### Decision: reuse path for portal V1

| Metric | Source | Existing calculation reused |
| --- | --- | --- |
| `TotalInventoryValue` | `StokBalanceViewDal` → sum `Hpp × Qty` after In-Transit exclusion | Yes — same formula as `StokBalanceInfo2Form` footer and `StokBalanceInfoForm.NilaiSediaan` |
| `TotalItem` | Distinct `BrgId` with aggregated `Qty > 0` | Derived from same row set; products with stock only |
| Warehouse scope | All warehouses except In-Transit | Matches `StokBalanceInfo2Form` default (no In-Transit checkbox) |
| `GeneratedAt` | `ITglJamDal.Now` | Same pattern as Sales (M4) and Piutang (M5) |

**`StokBalanceViewDal`** was selected because it is the established **stock balance summary** source backed by `BTR_StokBalanceWarehouse`. **`KartuStokSummaryDal`** recalculates movement from `BTR_StokMutasi` for a selected warehouse and period — explicitly out of scope per constraints (do not recalculate from transaction history).

No new SQL, tables, or inventory calculation policies were introduced.

---

## 2. Existing DALs Reused

| DAL / Service | Interface | Used for |
| --- | --- | --- |
| `StokBalanceViewDal` | `IStokBalanceViewDal` | Load stock balance rows from `BTR_StokBalanceWarehouse` + dimension joins |
| `TglJamDal` | `ITglJamDal` | Server timestamp for `GeneratedAt` |

`DashboardInventoryDal` orchestrates these dependencies; it does not duplicate SQL or business rules.

**Investigated but not wired for V1:**

| DAL | Reason not primary |
| --- | --- |
| `KartuStokDal` | Transaction ledger — per-product drilldown, not company summary |
| `KartuStokSummaryDal` | Recalculates from `BTR_StokMutasi`; requires warehouse + date range |
| `StokPeriodikDal` | Point-in-time qty from mutasi history — supports Kartu Stok Summary only |
| `StokBrgSupplierDal` | Supplier-scoped subset of same balance table |
| `BrgStokViewDal` | FIFO lot view from `BTR_Stok` — operational, not balance report total |

---

## 3. Existing Tables Reused

| Table | Usage |
| --- | --- |
| `BTR_StokBalanceWarehouse` | Primary read model — persisted qty per product × warehouse |
| `BTR_Brg` | Product master — `Hpp`, `BrgCode`, `BrgName`, classification keys |
| `BTR_Warehouse` | Warehouse name (In-Transit filter) |
| `BTR_BrgSatuan` | Unit conversion (`SatKecil`, `SatBesar`, `Conversion`) — carried in DAL row; not used in V1 KPI |
| `BTR_Kategori` | Category name (carried in report row; not used in V1 KPI) |
| `BTR_Supplier` | Supplier name (carried in report row; not used in V1 KPI) |

No new dashboard tables were created.

---

## 4. Existing Calculations Reused

| Calculation | Where defined | Portal usage |
| --- | --- | --- |
| Qty on hand | `BTR_StokBalanceWarehouse.Qty` | Read via `StokBalanceView.Qty` |
| Value per balance row | `Hpp × Qty` | In-memory product of DAL fields |
| In-Transit exclusion | `WarehouseName != "In-Transit"` | In-memory filter matching `StokBalanceInfo2Form` default |
| Product-level qty rollup | `Sum(Qty)` grouped by `BrgId` | In-memory group matching `StokBalanceInfoForm` |
| Total inventory value | `Sum(NilaiSediaan)` / `Sum(Hpp × Qty)` | In-memory sum matching grid footer |
| Items in stock | Count of products with `Qty > 0` after rollup | In-memory count on grouped rows |

---

## 5. New Response Shape

`DashboardInventoryResponse` replaces the Milestone 3 placeholder:

```json
{
  "Status": "success",
  "Code": 200,
  "Message": null,
  "Data": {
    "TotalInventoryValue": 14673597942.16,
    "TotalItem": 1716,
    "GeneratedAt": "2026-06-06T01:10:30.96"
  }
}
```

| Field | Type | Meaning |
| --- | --- | --- |
| `TotalInventoryValue` | `decimal` | Sum of `Hpp × Qty` across all warehouses except In-Transit |
| `TotalItem` | `int` | Count of distinct products (`BrgId`) with on-hand qty greater than zero |
| `GeneratedAt` | `DateTime` | Server timestamp when the summary was built (`ITglJamDal.Now`) |

---

## 6. Files Changed

### Application

| File | Change |
| --- | --- |
| `ReportingContext/DashboardInventoryAgg/Contracts/IDashboardInventoryDal.cs` | `GetPlaceholder()` → `GetSummary()` |
| `ReportingContext/DashboardInventoryAgg/Queries/GetDashboardInventoryQuery.cs` | Real response DTO; handler calls `GetSummary()` |

### Infrastructure

| File | Change |
| --- | --- |
| `ReportingContext/DashboardInventoryAgg/DashboardInventoryDal.cs` | Injects `IStokBalanceViewDal`, `ITglJamDal`; builds summary from balance rows |

### Unchanged (by design)

- `InventoryDashboardController` — still thin MediatR delegate
- `DashboardSalesDal`, `DashboardPiutangDal` — M4/M5 real data unchanged
- Authentication, health, CORS, JWT filter

---

## 7. Verification Results

| # | Check | Result |
| --- | --- | --- |
| 1 | Endpoint returns real data (SQL-backed shape) | Pass — `TotalInventoryValue`, `TotalItem`, `GeneratedAt` populated from dev DB |
| 2 | No direct SQL in controller | Pass — `InventoryDashboardController` calls MediatR only |
| 3 | Data flows through MediatR | Pass — `GetDashboardInventoryQuery` → `GetDashboardInventoryHandler` → `IDashboardInventoryDal` |
| 4 | Login still works | Pass — `POST /api/auth/login` → JWT for `DIMAS` |
| 5 | Dashboard authorization still works | Pass — anonymous `GET /api/dashboard/inventory` → HTTP 401 |
| 6 | Full solution build | Pass — `j05-btr-distrib.sln` Debug build |
| 7 | Sales / piutang unchanged | Pass — M4/M5 response shapes still returned |

Test commands (IIS Express on port 5055):

```powershell
# Build
& "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe" `
  "src\j05-btr-distrib\j05-btr-distrib.sln" /p:Configuration=Debug

# Run
& "C:\Program Files\IIS Express\iisexpress.exe" `
  /path:"src\j05-btr-distrib\btr.portal.api" /port:5055

# Anonymous (401)
curl.exe http://localhost:5055/api/dashboard/inventory

# Login
curl.exe -s -X POST http://localhost:5055/api/auth/login `
  -H "Content-Type: application/json" `
  --data-raw '{"UserId":"DIMAS","Password":"1111"}'

# Authenticated inventory dashboard
curl.exe http://localhost:5055/api/dashboard/inventory -H "Authorization: Bearer <token>"
```

**Sample dev DB response (`btr_yk`):** `TotalInventoryValue` ≈ 14.7B IDR, `TotalItem` = 1716 products with stock. Values depend on database content.

---

## 8. Inventory Risks Discovered

| Risk | Impact | Notes |
| --- | --- | --- |
| `BTR_StokBalanceWarehouse` staleness | Dashboard total may lag if balance table is not refreshed after postings | Same risk as desktop Stok Balance reports — portal inherits desktop behavior |
| In-Transit hard-coded by name | Filter breaks if warehouse is renamed | Matches `StokBalanceInfo2Form` string comparison `"In-Transit"` |
| `StokBalanceInfoForm` vs `StokBalanceInfo2Form` scope | Info form includes In-Transit; Info2 excludes by default | Portal follows Info2 default; totals will differ from Info form when In-Transit has qty |
| Full catalog load | `StokBalanceViewDal.ListData()` returns all products × warehouses | Acceptable for V1; may be slow on very large catalogs |
| Zero-stock products | Excluded from `TotalItem` but included in DAL result set | Intentional KPI — only counts SKUs with on-hand qty |
| HPP from `BTR_Brg` | Value uses current product HPP, not FIFO lot cost | Same as desktop `NilaiSediaan` calculation |

---

## 9. Future Improvements

| Item | Description |
| --- | --- |
| Warehouse filter | Mirror `StokBalanceInfoForm` warehouse picker or `KartuStokSummaryForm` combo |
| Include In-Transit toggle | Mirror `ShowInTransitCheckBox` on `StokBalanceInfo2Form` |
| Supplier / kategori slices | Group open stock by dimensions already present in `StokBalanceView` rows |
| Unit display | Expose `QtyBesar` / `QtyKecil` breakdown from existing conversion logic in `StokBalanceInfoForm` |
| Charts | Category or supplier distribution from grouped balance rows |
| Kartu Stok drilldown | Per-product movement from `KartuStokDal` — separate endpoint, not dashboard V1 |
| Performance | Dedicated summary query if full balance row load becomes slow |
| IIS deployment | Publish profile, `appsettings.{MACHINE}.json`, production JWT secret |
