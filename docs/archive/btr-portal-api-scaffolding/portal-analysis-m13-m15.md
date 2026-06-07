# BTR Portal Analysis — Milestones M13–M15

**Status:** Discovery and analysis only — no implementation plans, API designs, UI specifications, or product decisions.  
**Scope:** M13 Sales Dashboard V3, M14 Piutang Dashboard V2, M15 Inventory Dashboard V2  
**Date:** 2026-06-06  
**Context:** BTR Portal is a read-only reporting and dashboard application. M1–M12 are complete or in progress per `btr-portal-milestone.md`.

**Reference documents:** `btr-portal-milestone.md`, `portal-analysis-m10-m12-final.md`, `implementation-summary-milestone-8.md`, `implementation-summary-m10.md`, `implementation-summary-m11.md`, `implementation-summary-m12.md`, `btr-reporting-investigation.md`

---

## 1. Executive Summary

M13–M15 extend the dashboard layer (M4–M8) with analytics widgets that already exist in BTR Desktop in partial or complete form. Unlike M10–M12 (report pages wrapping a single DAL), these milestones combine **existing builders/policies** with **in-memory aggregation** over data already loaded by portal or desktop DALs.

| Milestone | Planned features | Primary desktop screen | Primary reusable builder/DAL | Portal anchor | Discovery headline |
| --------- | ---------------- | ---------------------- | ---------------------------- | ------------- | ------------------ |
| **M13** Sales Dashboard V3 | Target vs Achievement, Sales Ranking | `SalesOmzetChartForm` (RO2 chart) hosted by `SalesOmzetInfoForm` | `SalesOmzetChartSummaryBuilder`, `SalesOmzetTargetResolver`, `ISalesOmzetDal` | M4/M8 `DashboardSalesDal` | **High reuse** — target, achievement %, and top-15 ranking logic already implemented and registered in portal DI |
| **M14** Piutang Dashboard V2 | Aging Analysis, Top Customer | `PiutangSalesWilayahForm` (FF1) | `IPiutangSalesWilayahDal` / M10 `PiutangReportDal` | M5/M10 `DashboardPiutangDal` | **Partial reuse** — open-balance data and `JatuhTempo` exist; **no aging bucket policy or top-customer builder found in codebase** |
| **M15** Inventory Dashboard V2 | Category Analysis, Supplier Analysis | `StokBalanceInfoForm`, `StokBalanceInfo2Form`, `StokBrgSupplierForm` | `IStokBalanceViewDal`, `IStokBrgSupplierDal` | M6/M11 `DashboardInventoryDal` | **Moderate reuse** — category/supplier fields on `StokBalanceView`; aggregation patterns exist in desktop forms but **no dedicated category/supplier chart builders** |

**Key architectural finding (consistent with M10–M12):** Sales omzet analytics have **dedicated Policy and Builder classes** in `btr.application`. Piutang and inventory analytics have **rules embedded in DAL SQL and WinForms grouping/aggregation** — no `*AgingPolicy`, `*CustomerRankingBuilder`, or `*CategoryAnalysisBuilder` classes exist.

**Portal readiness:** M8 already wires `SalesOmzetChartSummaryBuilder` into `DashboardSalesDal` but deliberately omits `targetAmount` and `BuildManagerComparison()`. M13 is primarily an **extension of existing portal + desktop sales chart stack**, not greenfield logic.

---

## 2. M13 Analysis — Sales Dashboard V3

### 2.1 Goal (from milestone history)

Enhance sales analytics with **Target vs Achievement** and **Sales Ranking** (salesman ranking, achievement percentage, monthly target comparison).

### 2.2 Existing Desktop Screens

| Menu / context | Screen | Path | Relevance |
| -------------- | ------ | ---- | --------- |
| RO2 — Sales Omzet Info | `SalesOmzetInfoForm` | `btr.distrib/SalesContext/SalesPersonAgg/SalesOmzetInfoForm.cs` | Primary data host; implements `ISalesOmzetChartHost`; opens chart dialog |
| RO2 — Sales Omzet Grafik | `SalesOmzetChartForm` | `btr.distrib/SalesContext/SalesPersonAgg/SalesOmzetChartForm.cs` | **Primary reference for M13 widgets** — KPI panel + three chart modes |
| RO2 — Pusat (manager) | Same forms with `ConfigureAsManagerView()` | `SalesOmzetInfoForm.ConfigureAsManagerView()`, `SalesOmzetChartForm.ConfigureAsManagerView()` | Manager view defaults to **ManagerComparison** chart mode (top sales bar chart) |
| RO2 — Materialize | `SalesOmzetMaterializeForm` | `SalesOmzetMaterializeForm.cs` | Operational — reconciles `BTR_SalesOmzet`; not a dashboard screen |

**`SalesOmzetChartForm` layout (desktop):**

- **KPI panel:** Recognized omzet (`Omzet diakui`), transaction count, **Target (Rp)**, **Tercapai (achievement %)**, optional Pipeline omzet (Sales Period mode only)
- **Chart mode selector:** Status (stacked column) | Weekly (column + cumulative line) | Manager comparison (horizontal bar, top 15)
- **Target overlay:** Orange-red horizontal strip line on Y-axis when target is set (`ApplyTargetStripLine`)

No separate "Sales Ranking Report" or "Achievement Report" screen exists beyond this chart form and the omzet info grid.

### 2.3 Existing DALs

| DAL | Interface | Data source | Used by portal |
| --- | --------- | ----------- | -------------- |
| `SalesOmzetDal` | `ISalesOmzetDal` | `BTR_SalesOmzet` | Yes — `DashboardSalesDal` |
| `SalesOmzetTargetDal` | `ISalesOmzetTargetDal` | `BTR_SalesOmzetTarget` | Registered in portal DI; **not yet called by `DashboardSalesDal`** |
| `SalesPersonDal` | `ISalesPersonDal` | `BTR_SalesPerson` | Used by `SalesOmzetTargetResolver` (name → id resolution) |

**Target table schema** (`BTR_SalesOmzetTarget.sql`):

- PK: `(SalesPersonId, TargetYear, TargetMonth)`
- Column: `TargetAmount DECIMAL(18,2)`
- One monthly target **per salesperson**, not a company-wide aggregate target

### 2.4 Existing Builders

| Builder | Location | M13-relevant outputs |
| ------- | -------- | -------------------- |
| `SalesOmzetChartSummaryBuilder` | `btr.application/SalesContext/SalesOmzetAgg/Services/` | `Target`, `AchievementPercent`, `RecognizedOmzet`, `ByWeek`, `ByStatus` |
| `BuildManagerComparison()` | Same builder | `SalesOmzetSalesPersonSlice[]` — top N by recognized omzet |
| `SalesOmzetChartWeekGrouper` | Same namespace | Weekly buckets (already used in M8) |

**`BuildManagerComparison()` rules (from source + tests):**

- Input: `IEnumerable<SalesOmzetView>`
- Filter: `IncludeInRecognizedTotal` — **Completed status only** (`SalesOmzetChartAmountPolicy`)
- Group by: `SalesPersonName` (trimmed, case-insensitive)
- Metric: `RecognizedOmzet = Sum(ResolveAmount(row))` → for Completed rows, amount = `FakturTotal`
- Sort: descending by `RecognizedOmzet`, then name ascending
- Take: **`ManagerComparisonTopCount = 15`** (constant on builder)
- Excludes rows with empty salesperson name

**`Build()` target/achievement path:**

- Accepts optional `targetAmount` parameter
- Sets `Target = targetAmount`
- Sets `AchievementPercent = SalesOmzetChartAchievementPolicy.ComputePercent(recognizedOmzet, targetAmount)`

### 2.5 Existing Policies

| Policy | Location | Rules |
| ------ | -------- | ----- |
| `SalesOmzetChartAchievementPolicy` | `Policies/SalesOmzetChartAchievementPolicy.cs` | `Achievement % = recognized / target × 100`; **not capped** (over-achievement shown); null when no target or target ≤ 0; display format `"N1%"` or `"—"` |
| `SalesOmzetChartAmountPolicy` | `Policies/SalesOmzetChartAmountPolicy.cs` | Completed/Pending → `FakturTotal`; Outstanding → `OrderTotal`; recognized total = Completed only |
| `SalesOmzetTargetResolver` | `Services/SalesOmzetTargetResolver.cs` | Resolves **single** salesperson target for filtered scope |
| `SalesOmzetPeriodPolicy` | Via DAL | Portal uses `SalesOmzetPeriodFilterMode.OmzetPeriod` (current month) |

**`SalesOmzetTargetResolver` resolution logic:**

1. Derive `(TargetYear, TargetMonth)` from **`periode.Tgl2`** (end of period month)
2. Resolve `SalesPersonId` when scope is unambiguous:
   - Exactly one distinct `SalesPersonName` in filtered rows → lookup id by name
   - Else match `searchKeyword` against salesperson master (single match)
   - Else match `currentUserDisplayName` (single match)
3. If ambiguous → returns **`null`** (no target shown on desktop)
4. Load target via `_targetDal.GetTargetAmount(salesPersonId, year, month)`

**Implication:** Desktop target/achievement KPI is **per-salesperson**, not company-wide. Manager comparison view uses **full unfiltered rows** for ranking when search is empty.

### 2.6 Existing Reports

No standalone RDLC or grid report named "Salesman Ranking" or "Achievement Report" was found. Ranking and achievement are **embedded in `SalesOmzetChartForm`** only.

Related sales reports (out of M13 scope but same data family):

- `FakturInfoForm` / `IFakturViewDal` — M9 Sales Report source
- `FakturPerCustomerDal` — per-customer faktur detail (not ranking)

### 2.7 Existing DTOs (chart-ready)

| DTO | Properties relevant to M13 |
| --- | -------------------------- |
| `SalesOmzetChartSummary` | `Target`, `AchievementPercent`, `RecognizedOmzet`, `BySalesPerson` (property exists; builder sets via separate method) |
| `SalesOmzetSalesPersonSlice` | `SalesPersonName`, `RecognizedOmzet` |
| `SalesOmzetWeekSlice` | Already exposed as `DashboardSalesWeekTrendItem` in portal (M8) |
| `SalesOmzetView` | Raw rows from `ISalesOmzetDal` — includes `SalesPersonName`, `OmzetStatus`, `FakturTotal`, `OrderTotal` |

### 2.8 Portal current state (M4 + M8)

| Component | M13-related behavior |
| --------- | ------------------- |
| `DashboardSalesDal` | Calls `Build(rows, periode, mode)` **without** `targetAmount`; maps `ByWeek` only |
| `ApplicationPortalExtensions` | Registers `SalesOmzetChartSummaryBuilder`, `SalesOmzetTargetResolver`, `SalesOmzetChartAmountPolicy`, `ISalesOmzetTargetDal` |
| `InfrastructurePortalExtensions` | Registers `ISalesOmzetTargetDal` → `SalesOmzetTargetDal` |
| `SalesTrendCard.vue` | Line chart for weekly trend; KPI row for Completed/Pipeline omzet — **no target or ranking widgets** |

### 2.9 Reusable assets

| Asset | Reuse rationale |
| ----- | --------------- |
| `SalesOmzetChartSummaryBuilder.Build(..., targetAmount)` | Target + achievement already computed |
| `SalesOmzetChartSummaryBuilder.BuildManagerComparison()` | Sales ranking with fixed top-15 rule |
| `SalesOmzetTargetResolver` | Same target resolution as desktop (with scope ambiguity rules) |
| `SalesOmzetChartAchievementPolicy` | Percent calculation and display formatting |
| `ISalesOmzetDal` + current-month period in `DashboardSalesDal` | Same data load as M4/M8 |
| `SalesOmzetChartForm` | Reference for KPI labels (`Target (Rp)`, `Tercapai`), chart types, colors |
| Unit tests | `SalesOmzetTargetTest`, `SalesOmzetChartSummaryTest` document expected behavior |

### 2.10 Existing business rules (do not reimplement)

| Rule | Where enforced |
| ---- | -------------- |
| Recognized omzet = Completed rows only | `SalesOmzetChartAmountPolicy.IncludeInRecognizedTotal` |
| Ranking metric = sum of recognized (`FakturTotal`) per salesperson | `BuildManagerComparison()` |
| Top N = 15 | `ManagerComparisonTopCount` constant |
| Achievement not capped at 100% | `SalesOmzetChartAchievementPolicy` |
| Target month = calendar month of period end | `ResolveTargetYearMonth(periode.Tgl2)` |
| Target null when multiple salespeople in scope | `SalesOmzetTargetResolver` |
| Portal period = current month, Omzet Period mode | `DashboardSalesDal` (fixed today) |

### 2.11 Existing visualizations (desktop)

| Widget | Chart type | Data binding |
| ------ | ---------- | ------------ |
| Target vs achievement KPI | Text labels in KPI panel | `summary.Target`, `summary.AchievementPercent` |
| Target reference line | Chart Y-axis strip line | `summary.Target` on Status and Weekly charts |
| Weekly trend | Column + cumulative line | `summary.ByWeek` |
| Sales ranking | Horizontal bar chart | `BuildManagerComparison()` slices |
| Status breakdown | Stacked column | `summary.ByStatus` (M13 milestone does not list this; exists in desktop) |

### 2.12 Gaps discovered

| Gap | Notes |
| --- | ----- |
| Company-wide target | `BTR_SalesOmzetTarget` is per `SalesPersonId` only — no table or resolver for org-level target |
| Ranking without salesperson filter | Desktop manager view uses `FullRows` when search empty; portal has no search/filter concept today |
| `BySalesPerson` on summary DTO | Property exists on `SalesOmzetChartSummary` but `Build()` does not populate it — ranking is separate method |
| Materialization freshness | Dashboard numbers depend on `BTR_SalesOmzet` reconcile job (documented in `btr-reporting-investigation.md`) |

---

## 3. M14 Analysis — Piutang Dashboard V2

### 3.1 Goal (from milestone history)

Enhance receivable analytics with **Aging Analysis** and **Top Customer** (aging buckets, largest outstanding customers, collection monitoring).

### 3.2 Existing Desktop Screens

| Menu | Screen | Path | Relevance |
| ---- | ------ | ---- | --------- |
| FF1 | `PiutangSalesWilayahForm` | `FinanceContext/PiutangSalesWilayahRpt/` | **Primary open-receivable grid** — grouped by Sales + Wilayah; shows `JatuhTempo`, `KurangBayar` |
| FT5 | `PiutangTrackerForm` | `FinanceContext/TagihanAgg/` | Per-faktur **timeline** (piutang/tagihan/pelunasan events) — not aging summary |
| FF2 | `PenerimaanPelunasanSalesForm` | `FinanceContext/PenerimaanPelunasanSalesRpt/` | **Collection monitoring** — payments grouped by date + salesperson |
| FF4 | `PelunasanInfoForm` | `FinanceContext/` | Payment detail listing |
| FT* | `TagihanForm` / related | Tagihan workflow | Transactional — not dashboard analytics |

**`PiutangSalesWilayahForm` behaviors relevant to M14:**

- Period filter on `PiutangDate` (user-selected date range; portal/M10 uses fixed `2000-01-01 → today`)
- Open balance filter: `KurangBayar > 1` when "Show Paid" unchecked (same as M5/M10)
- Auto-group: `SalesName`, then `WilayahName`
- Grid summary row: sums including `KurangBayar`
- Excel export with hierarchical Sales → Wilayah → Faktur structure
- **No aging bucket columns, no customer ranking chart, no overdue-days calculation in form code**

### 3.3 Existing DALs

| DAL | Interface | DTO | M14 relevance |
| --- | --------- | --- | ------------- |
| `PiutangSalesWilayahDal` | `IPiutangSalesWilayahDal` | `PiutangSalesWilayahDto` | **Primary source** — same as M5 dashboard and M10 report |
| `PenerimaanPelunasanSalesDal` | `IPenerimaanPelunasanSalesDal` | `PenerimaanPelunasanSalesDto` | Collection totals by `LunasDate` + `SalesName` — not customer ranking |
| `PiutangTrackerDal` | `IPiutangTrackerDal` | `PiutangTrackerDto` | Single-faktur drilldown |
| `PelunasanInfoDal` | — | — | Payment listing |

**`PiutangSalesWilayahDto` fields available for aging / top customer:**

| Field | Source (SQL) | Notes |
| ----- | ------------ | ----- |
| `JatuhTempo` | `BTR_Piutang.DueDate` | Due date per faktur — **raw input for aging; no bucket assignment in DAL** |
| `KurangBayar` | `BTR_Piutang.Sisa` | Outstanding balance |
| `CustomerCode`, `CustomerName` | `BTR_Customer` | Customer identity |
| `FakturDate` | `BTR_Faktur.FakturDate` | Invoice date — alternative aging anchor (not used in desktop aging — none exists) |
| `SalesName`, `WilayahName` | Joins | Secondary grouping dimensions |

**Portal wrappers already using this DAL:**

- `DashboardPiutangDal` — KPI only (`TotalPiutang`, `TotalCustomer`)
- `PiutangReportDal` (M10) — row list + footer totals; **`JatuhTempo` exposed for M14 prep** per M10 product decision

### 3.4 Existing Builders

**None found** for aging buckets or customer ranking.

Searched patterns: `Aging`, `UmurPiutang`, `PiutangAging`, `TopCustomer`, `CustomerRanking`, `Leaderboard` — **no matches** in `src/j05-btr-distrib`.

Customer aggregation in desktop is **inline LINQ** in Excel export (`GroupBy SalesName → WilayahName`) — not a reusable builder.

### 3.5 Existing Policies

**None found** for aging bucket definitions or customer ranking.

Business rules for open receivables (from M5/M10, embedded in portal DALs):

| Rule | Where enforced |
| ---- | -------------- |
| Outstanding balance | `BTR_Piutang.Sisa` → `KurangBayar` |
| Open threshold | `KurangBayar > 1` |
| Period | `PiutangDate BETWEEN @Tgl1 AND @Tgl2` |
| Customer key for distinct count | `CustomerCode` if present, else `CustomerName` (`ResolveCustomerKey`) |

**Confirmed by `btr-reporting-investigation.md`:** "Customer aging — **Not implemented** as dedicated aging buckets. Reports expose `DueDate` / `JatuhTempo` but no 30/60/90-day aging logic found."

### 3.6 Existing Reports

| Report | Customer aggregation | Aging |
| ------ | -------------------- | ----- |
| FF1 Piutang Sales Wilayah | Groups by Sales + Wilayah in grid; Excel nests by Sales/Wilayah | Shows `JatuhTempo` column only |
| FF2 Penerimaan Pelunasan Sales | Groups by date + salesperson | N/A — collections, not outstanding |
| M10 Piutang Report (portal) | Flat list; footer `TotalCustomer` | `JatuhTempo` column emphasized in UI |

No "Outstanding Customer Report" or "Aging Report" screen exists as a named feature.

### 3.7 Chart-ready data (would require aggregation)

From M10 report rows (`PiutangReportRow`), the following **inputs exist** but **no pre-built chart DTOs**:

| Potential aggregation | Source fields | Existing desktop equivalent |
| --------------------- | ------------- | --------------------------- |
| Aging buckets | `JatuhTempo`, `KurangBayar`, reference date (`ITglJamDal.Now`) | **None** |
| Top customer by outstanding | `CustomerName`/`CustomerCode`, `Sum(KurangBayar)` | **None** (grid can group manually via Syncfusion) |
| Top customer by faktur count | Same + count | **None** |

### 3.8 Reusable assets

| Asset | Reuse rationale |
| ----- | --------------- |
| `IPiutangSalesWilayahDal` | Single source for open receivable rows |
| `DashboardPiutangDal.OpenReceivablesPeriode()` | Period `2000-01-01 → today` |
| `DashboardPiutangDal` filter + `ResolveCustomerKey()` | Consistent with M5/M10 |
| `PiutangReportDal` | M10 wrapper — same rows M14 would aggregate; KPI traceability pattern proven |
| M10 `PiutangReportRow` | Already includes `CustomerName`, `JatuhTempo`, `KurangBayar` |
| `PenerimaanPelunasanSalesDal` | Secondary source if collection monitoring is in scope (different metric family) |

### 3.9 Gaps discovered

| Gap | Severity | Notes |
| --- | -------- | ----- |
| No aging bucket policy | High | Product must define buckets; nothing to reuse from Desktop |
| No top-customer ranking logic | High | Must aggregate from `PiutangSalesWilayahDto` — pattern exists in sales `BuildManagerComparison` but not piutang |
| Large dataset | Medium | M10 loads ~11K rows in one API call on dev DB — aging/top-N aggregation adds compute on same dataset |
| No piutang chart in portal | Low | M8 explicitly deferred piutang charts; no `chart.js` widget for piutang yet |
| Collection vs outstanding | Medium | FF2 measures **payments received**; M14 "Top Customer" likely means **outstanding** — different DALs |

---

## 4. M15 Analysis — Inventory Dashboard V2

### 4.1 Goal (from milestone history)

Enhance inventory analytics with **Category Analysis** and **Supplier Analysis** (inventory by category, by supplier, composition, concentration).

### 4.2 Existing Desktop Screens

| Menu | Screen | Path | Relevance |
| ---- | ------ | ---- | --------- |
| IF1 | `StokBalanceInfo2Form` | `InventoryContext/StokBalanceRpt/` | **Primary M11 report screen** — Supplier + Kategori columns; Syncfusion grouping |
| IF1-alt | `StokBalanceInfoForm` | Same folder | Aggregates by item across warehouses; **Supplier/Kategori filters**; shows Supplier + Kategori columns |
| IF* | `StokBrgSupplierForm` | `InventoryContext/StokBrgSupplierRpt/` | **Stock per supplier** dedicated report — item-level with supplier/kategori |
| IF8 | `KartuStokSummaryForm` | `InventoryContext/KartuStokRpt/` | Period + warehouse movement summary; includes Supplier + Kategori columns — **uses `BTR_StokMutasi`**, not balance table |
| IF* | `StokPeriodikForm` | `InventoryContext/StokPeriodikRpt/` | Point-in-time periodic stock; includes `KategoriName` |

**`StokBalanceInfoForm` aggregation pattern (relevant to category/supplier analysis):**

1. Load `IStokBalanceViewDal.ListData()`
2. Optional filter by Supplier name, Kategori name, item search
3. **Group by** `(BrgId, BrgCode, BrgName, SupplierId, SupplierName, KategoriId, KategoriName, ...)` summing `Qty` and `NilaiSediaan = Sum(Hpp × Qty)` across warehouses
4. Sort: Supplier → Kategori → BrgCode

**`StokBalanceInfo2Form`:** Maps rows to `StokBalanceInfoDto` with `Supplier`, `Kategori` columns; supports In-Transit toggle (same `"In-Transit"` string as M6/M11).

**`StokBrgSupplierForm`:** Uses separate `IStokBrgSupplierDal`; item × warehouse rows with `NilaiInPcs = Qty × Hpp`; footer sums `NilaiStokBesar`, `NilaiStokKecil`, `NilaiInPcs`.

### 4.3 Existing DALs

| DAL | Interface | DTO | Data source | Portal usage |
| --- | --------- | --- | ----------- | -------------- |
| `StokBalanceViewDal` | `IStokBalanceViewDal` | `StokBalanceView` | `BTR_StokBalanceWarehouse` + `BTR_Brg` + Kategori + Supplier | M6 dashboard, M11 report |
| `StokBrgSupplierDal` | `IStokBrgSupplierDal` | `StokBrgSupplierView` | Same balance table; includes price tiers (GT/MT) | Desktop only |
| `KartuStokSummaryDal` | `IKartuStokSummaryDal` | `KartuStokSummaryDto` | `BTR_StokMutasi` (period-scoped) | Desktop only — **different data source** |
| `StokPeriodikDal` | `IStokPeriodikDal` | `StokPeriodikDto` | Periodic snapshot | Desktop only |

**`StokBalanceView` fields for M15:**

```
SupplierId, SupplierName, KategoriId, KategoriName, BrgId, BrgCode, BrgName,
WarehouseId, WarehouseName, Qty, Hpp, NilaiSediaan (= Hpp × Qty in forms)
```

### 4.4 Existing Builders

**None found** for category composition or supplier concentration charts.

Aggregation is performed **inline in WinForms `Proses()` methods** (LINQ `group by`).

### 4.5 Existing Policies

| Rule | Where enforced |
| ---- | -------------- |
| Valuation | `NilaiSediaan = Hpp × Qty` | Desktop forms + `DashboardInventoryDal` |
| In-Transit exclusion | `WarehouseName != "In-Transit"` | M6/M11 portal; optional toggle on desktop IF1 |
| Dashboard item grouping | Group by `BrgId`, sum Qty and NilaiSediaan across warehouses | `DashboardInventoryDal` |
| Zero qty in M11 report | `Qty > 0` filter on report rows | `InventoryReportDal` — dashboard counts items where aggregated Qty > 0 |
| M11 deferred columns | Supplier, Kategori not in report V1 | Product decision — **fields exist on DAL** |

No `InventoryValuationPolicy` or `CategoryAggregationPolicy` class exists — rules are duplicated between dashboard wrapper and desktop forms.

### 4.6 Existing Reports

| Report | Category/supplier presentation |
| ------ | ------------------------------ |
| IF1 `StokBalanceInfo2Form` | Columns + manual Syncfusion group-by |
| IF1 `StokBalanceInfoForm` | Filter + aggregate; Supplier/Kategori visible |
| `StokBrgSupplierForm` | Supplier-centric item listing with unit breakdown |
| M11 Inventory Report (portal) | **No** Supplier/Kategori columns in V1 |
| M6 Dashboard (portal) | KPI totals only — no breakdown |

### 4.7 Chart-ready data (would require aggregation)

| Potential slice | Aggregation pattern (from desktop forms) | Pre-built DTO |
| --------------- | ------------------------------------------ | ------------- |
| By Kategori | `GroupBy KategoriId/KategoriName`, `Sum(Hpp × Qty)`, optionally `Sum(Qty)` | None |
| By Supplier | `GroupBy SupplierId/SupplierName`, same sums | None |
| Composition % | Derived from category/supplier totals / `TotalInventoryValue` | None |

**Note:** M11 documents that footer `TotalInventoryValue` uses **BrgId grouping across warehouses**, not a naive sum of visible rows. Category/supplier dashboards must align with this KPI if traceability is required.

### 4.8 Reusable assets

| Asset | Reuse rationale |
| ----- | --------------- |
| `IStokBalanceViewDal` + `StokBalanceView` | Same source as M6/M11; includes Supplier + Kategori |
| `DashboardInventoryDal` grouping + In-Transit exclusion | KPI baseline for reconciliation |
| `InventoryReportDal` | M11 wrapper — row-level detail behind KPI |
| `StokBalanceInfoForm.Proses()` LINQ | Reference aggregation by supplier/kategori/item |
| `StokBrgSupplierDal` | Alternative if supplier-specific price/unit columns needed |
| M11 unit tests | Document In-Transit, BrgId grouping, zero-qty behavior |

### 4.9 Gaps discovered

| Gap | Notes |
| --- | ----- |
| No category/supplier builder | Aggregation logic only in WinForms — would need new ReportingContext aggregation or inline in dashboard DAL |
| Two data sources | Balance table (`StokBalanceViewDal`) vs movement ledger (`KartuStokSummaryDal`) — M11 decision: balance only for portal V1 |
| Warehouse dimension | Desktop IF1-alt collapses warehouses; M11 shows per-warehouse rows — category/supplier totals must decide grouping level |
| `IStokBrgSupplierDal` not in portal | Separate SQL from `StokBalanceViewDal`; richer columns but not yet wired |
| No inventory charts in portal | M8 deferred inventory charts; no chart component for inventory domain |

---

## 5. Dashboard UX Discovery

Conceptual layouts derived from **existing desktop widget arrangements**. Not visual designs or UI specifications.

### 5.1 Current portal dashboard (M7–M8 baseline)

```
+-------------+  +-------------+  +-------------+
| Sales KPI   |  | Piutang KPI |  | Inventory KPI|
| Omzet       |  | Total       |  | Total Value  |
| Faktur      |  | Customer    |  | Total Item   |
| Customer    |  |             |  |              |
+-------------+  +-------------+  +-------------+

+-----------------------------------------------+
| Sales Trend Card (M8)                         |
| Completed | Pipeline | Faktur | Customer      |
| [ Weekly Omzet Trend — line chart ]           |
+-----------------------------------------------+
```

Reference: `DashboardHomeView.vue`, `SalesTrendCard.vue`, `SalesOmzetChartForm` KPI panel.

### 5.2 M13 — Sales Dashboard V3 (from `SalesOmzetChartForm`)

**Option A — Extend Sales Trend Card (desktop KPI + weekly chart pattern):**

```
+-----------------------------------------------+
| Sales Analytics                               |
| Omzet diakui | Target (Rp) | Tercapai %      |  <- SalesOmzetChartForm KPI panel
| Completed | Pipeline | Faktur | Customer      |  <- existing M8 summary row
+-----------------------------------------------+
| [ Weekly Omzet Trend — line/column ]          |  <- existing M8 (BindWeeklyChart)
+-----------------------------------------------+
| [ Top 15 Sales — horizontal bar ranking ]     |  <- BindManagerComparisonChart
+-----------------------------------------------+
```

**Option B — Manager view default (desktop `ConfigureAsManagerView`):**

```
+-----------------------------------------------+
| Target (Rp) | Tercapai % | Omzet diakui       |
+-----------------------------------------------+
| [ Perbandingan omzet diakui per sales — bar ] |
| (Top 15, click bar filters — desktop only)    |
+-----------------------------------------------+
```

**Desktop chart types to mirror logically:**

| Mode | Desktop chart | Data |
| ---- | ------------- | ---- |
| Weekly | Column + cumulative line | `ByWeek` |
| Manager comparison | Horizontal bar | `BuildManagerComparison()` |
| Target reference | Y-axis strip line | `summary.Target` |

### 5.3 M14 — Piutang Dashboard V2 (no desktop chart — inferred from FF1 grid + M10 report)

**Option A — Aging + Top Customer below Piutang KPI card:**

```
+-------------+
| Piutang KPI |  (existing M5)
+-------------+

+-----------------------------------------------+
| Piutang Aging                                 |
| [ Bucket chart — no desktop equivalent ]      |
| Current | 1-30 | 31-60 | 61-90 | >90 (TBD)   |
+-----------------------------------------------+
| Top Customers by Outstanding                  |
| [ Ranked table or bar — no desktop equiv. ]   |
+-----------------------------------------------+
```

**Option B — FF1 grouping metaphor (Sales → Wilayah — not customer-focused):**

```
+-----------------------------------------------+
| Piutang by Sales / Wilayah (grouped table)    |
| <- PiutangSalesWilayahForm grid grouping      |
+-----------------------------------------------+
```

M14 milestone names **Aging** and **Top Customer** — FF1 grouping is Sales/Wilayah oriented; customer ranking is not a first-class desktop widget.

**Secondary desktop reference (collections — if scope expands):**

```
+-----------------------------------------------+
| Penerimaan Pelunasan by Date / Sales          |
| <- PenerimaanPelunasanSalesForm               |
+-----------------------------------------------+
```

### 5.4 M15 — Inventory Dashboard V2 (from IF1 forms)

**Option A — Composition charts below Inventory KPI:**

```
+-------------+
| Inventory KPI|  (existing M6)
+-------------+

+-----------------------------------------------+
| Inventory by Category                         |
| [ Pie or bar — group StokBalanceView ]        |
| <- StokBalanceInfoForm Kategori aggregation   |
+-----------------------------------------------+
| Inventory by Supplier                         |
| [ Pie or bar — group StokBalanceView ]        |
| <- StokBalanceInfoForm Supplier aggregation   |
+-----------------------------------------------+
```

**Option B — StokBrgSupplierForm style (item-level supplier report as table):**

```
+-----------------------------------------------+
| Stock by Supplier (item detail table)         |
| Supplier | Kategori | Item | Qty | Nilai      |
| <- StokBrgSupplierForm grid                   |
+-----------------------------------------------+
```

**Desktop KPI/summary patterns:**

| Screen | Summary widget |
| ------ | -------------- |
| `StokBalanceInfo2Form` | Grid footer sum `NilaiSediaan` |
| `StokBrgSupplierForm` | Footer sums `NilaiStokBesar`, `NilaiStokKecil`, `NilaiInPcs` |
| M11 portal | `ReportSummaryBar` — Total Inventory Value, Total Item |

---

## 6. Cross-Milestone Reuse Opportunities

### 6.1 Comparison matrix

| Milestone | Existing Screen | Existing DAL | Existing Builder | Complexity | Risk |
| --------- | --------------- | ------------ | ---------------- | ---------- | ---- |
| **M13** Sales Dashboard V3 | `SalesOmzetChartForm` + `SalesOmzetInfoForm` | `ISalesOmzetDal`, `ISalesOmzetTargetDal` | `SalesOmzetChartSummaryBuilder`, `SalesOmzetTargetResolver`, `SalesOmzetChartAchievementPolicy` | **Low–Medium** — extend M8 `DashboardSalesDal`; builders already in portal DI | **Low–Medium** — target scope ambiguity; materialization freshness; manager vs rep view |
| **M14** Piutang Dashboard V2 | `PiutangSalesWilayahForm` (FF1) | `IPiutangSalesWilayahDal` | **None** — aging/ranking aggregation not implemented | **Medium–High** — new aggregation over M10 dataset; no desktop bucket policy | **High** — no aging definition; large row volume; KPI traceability for bucket totals |
| **M15** Inventory Dashboard V2 | `StokBalanceInfoForm`, `StokBalanceInfo2Form`, `StokBrgSupplierForm` | `IStokBalanceViewDal` (primary), `IStokBrgSupplierDal` (alternate) | **None** — LINQ in WinForms only | **Medium** — new aggregation; align with M6 BrgId grouping | **Medium** — warehouse vs item-level totals; M11 deferred Supplier/Kategori; two data sources if Kartu Stok considered |

### 6.2 Shared chart infrastructure

| Infrastructure | M13 | M14 | M15 | Notes |
| -------------- | --- | --- | --- | ----- |
| `chart.js` + PrimeVue `Chart` | Yes (M8) | Not yet used | Not yet used | Proven in `SalesTrendCard.vue` |
| Line / column charts | M8 weekly trend | Candidate for aging | Candidate for composition | Desktop uses WinForms `DataVisualization.Charting` |
| Horizontal bar (ranking) | Desktop manager comparison | Candidate for top customer | Candidate for supplier ranking | M13 has builder |
| KPI card pattern | `KpiCard.vue` | Same | Same | M7 foundation |
| `ReportSummaryBar` | N/A (dashboard) | Traceability pattern from M10 | Traceability from M11 | Footer reconciliation pattern |

### 6.3 Shared aggregation logic

| Pattern | Source | Applicable milestones |
| ------- | ------ | --------------------- |
| Top-N ranking by metric, descending | `BuildManagerComparison()` | M13 directly; **pattern** for M14 top customer |
| Group-by + sum | PiutangSalesWilayah Excel export; StokBalanceInfoForm | M14, M15 |
| Distinct count with key resolver | `ResolveCustomerKey()` in piutang; sales customer key in `DashboardSalesDal` | M14 |
| BrgId cross-warehouse grouping | `DashboardInventoryDal` | M15 — must align category/supplier sums with M6 KPI |
| Period conventions | Current month (sales), 2000→today (piutang), snapshot (inventory) | All three |

### 6.4 Shared ranking patterns

| Aspect | M13 (exists) | M14 (gap) | M15 (gap) |
| ------ | ------------ | --------- | --------- |
| Ranking entity | SalesPersonName | CustomerName/CustomerCode | SupplierName / KategoriName |
| Ranking metric | RecognizedOmzet (Completed FakturTotal) | Likely Sum(KurangBayar) — **not defined** | Likely Sum(Hpp×Qty) — **not defined** |
| Top N limit | 15 (constant) | **Not defined** | **Not defined** |
| Sort tie-break | Name ascending | **Not defined** | **Not defined** |
| Filter before rank | Completed omzet rows only | KurangBayar > 1 (M5/M10) | Qty > 0, excl. In-Transit (M6/M11) |

### 6.5 Shared KPI patterns

| Pattern | Portal component | Desktop equivalent |
| ------- | ---------------- | ------------------ |
| Scalar KPI card | `KpiCard.vue` | `SalesOmzetChartForm` KPI labels |
| Target + achievement pair | Not in portal yet | `TargetValueLabel` + `AchievementValueLabel` |
| Generated timestamp | Card footer meta | — |
| Dashboard ↔ report reconciliation | M10/M11 footer totals | FF1/IF1 grid summary rows |

### 6.6 Shared frontend opportunities

| Layer | Reuse across M13–M15 |
| ----- | -------------------- |
| `dashboardStore` | Extend per-domain dashboard responses |
| `formatCurrency`, `formatNumber` | All three |
| Chart card layout | Model after `SalesTrendCard.vue` |
| Loading / error / empty states | Same as M8 |
| Ranking table | **New pattern** — no portal ranking table yet; desktop uses chart not DataTable for M13 |

---

## 7. Risks

| Risk | Milestone | Severity | Evidence |
| ---- | --------- | -------- | -------- |
| No aging bucket policy in codebase | M14 | **High** | `btr-reporting-investigation.md`; grep found no Aging classes |
| No top-customer builder | M14 | **High** | Only flat FF1 grid + M10 report rows |
| Large piutang dataset single-load | M14 | **Medium** | M10: ~11K rows, ~12s API on dev hardware |
| Target scope ambiguity (multi-rep) | M13 | **Medium** | `SalesOmzetTargetResolver` returns null when ambiguous — portal has no search/filter |
| Company-wide vs per-rep target | M13 | **Medium** | `BTR_SalesOmzetTarget` is per salesperson only |
| `BTR_SalesOmzet` materialization stale | M13 | **Medium** | Dashboard depends on reconcile job |
| Category/supplier totals vs M6 KPI mismatch | M15 | **Medium** | M11 documents BrgId grouping vs row-level sum divergence |
| Two inventory data sources | M15 | **Medium** | Balance vs Kartu Stok — M11 explicitly deferred ledger |
| No inventory/piutang chart components | M14, M15 | **Low** | M8 deferred; chart.js available but unused |
| In-Transit hardcoded string | M15 | **Low** | `"In-Transit"` in `DashboardInventoryDal` |
| DAL wrapper + DI registration | All | **Low** | Pattern proven M8–M12 |
| Portal read-only constraint | All | **Low** | All sources are read/query paths |

---

## 8. Open Questions

Product Owner to decide. **Do not assume answers.**

### 8.1 M13 — Sales Dashboard V3

| # | Question |
| - | -------- |
| 1 | Should target vs achievement show **company-wide** totals, **single salesperson**, or **both** depending on context? Desktop supports single-rep only via `SalesOmzetTargetResolver`. |
| 2 | When multiple salespeople exist in the period (portal default = all), should target/achievement be **hidden** (desktop behavior) or replaced with an **aggregate target** (no table exists for aggregate)? |
| 3 | Should sales ranking always show **top 15** (`ManagerComparisonTopCount`) or a different N? |
| 4 | Should ranking use **recognized omzet only** (desktop `BuildManagerComparison`) or include pipeline/outstanding? |
| 5 | Should portal use **Omzet Period** (current M8 default) or offer **Sales Period** (includes pipeline KPI on desktop)? |
| 6 | Should ranking be **interactive** (desktop: click bar → filter salesperson) in read-only portal? |
| 7 | Should **status breakdown chart** (`ByStatus`) be included though not listed in milestone brief? |
| 8 | Is **target strip line on weekly chart** required, or KPI text sufficient? |
| 9 | How should dashboard behave when **no target row** exists in `BTR_SalesOmzetTarget` for the resolved salesperson/month? |
| 10 | Must ranking/t achievement numbers **reconcile** with M9 Sales Report totals for the same period? |

### 8.2 M14 — Piutang Dashboard V2

| # | Question |
| - | -------- |
| 1 | What **aging bucket definitions** should be used (e.g., Current, 1–30, 31–60, 61–90, 90+ days)? **No desktop precedent.** |
| 2 | Should aging days be calculated from **`JatuhTempo`** (due date) or **`FakturDate`** (invoice date)? |
| 3 | What is the **reference date** for aging — today (`ITglJamDal.Now`), end of period, or user-selectable (post-M15 enhancement)? |
| 4 | Should aging amounts use **`KurangBayar`** (outstanding) per faktur — consistent with M5/M10? |
| 5 | Should **paid faktur** (`KurangBayar ≤ 1`) be excluded from aging — consistent with M10 fixed filter? |
| 6 | For **Top Customer**, rank by **sum of KurangBayar**, **faktur count**, or **TotalJual**? |
| 7 | How many customers should appear in **Top N**? |
| 8 | Should customer identity use **`CustomerCode`**, **`CustomerName`**, or same logic as `ResolveCustomerKey()`? |
| 9 | Should aging bucket totals **reconcile** to M5 `TotalPiutang` and M10 report footer? |
| 10 | Is **collection monitoring** (FF2 `PenerimaanPelunasanSalesDal`) in scope for M14 or a separate milestone? |
| 11 | Should piutang dashboard use same period as M10 (**2000-01-01 → today**) or a different window? |
| 12 | Is **Sales/Wilayah grouping** (FF1 grid) desired in portal or strictly aging + top customer? |

### 8.3 M15 — Inventory Dashboard V2

| # | Question |
| - | -------- |
| 1 | Should category/supplier analysis use **`IStokBalanceViewDal`** (M6/M11 source) or **`IStokBrgSupplierDal`** (richer supplier report)? |
| 2 | Should analysis include **In-Transit** warehouse or exclude — match M6/M11? |
| 3 | Should rows with **Qty = 0** be excluded — match M11 report filter? |
| 4 | Should valuation use **`Hpp × Qty`** per row — match desktop and M6? |
| 5 | Should totals aggregate **across all warehouses** (like `StokBalanceInfoForm`) or show **per warehouse**? |
| 6 | For **category/supplier composition**, show **Nilai Sediaan**, **Qty**, **item count**, or **percentage share**? |
| 7 | How many categories/suppliers should appear in **Top N** or should all be shown? |
| 8 | Should category/supplier chart totals **reconcile** to M6 `TotalInventoryValue` / `TotalItem`? |
| 9 | Should **unit breakdown** (QtyBesar/QtyKecil) appear — deferred in M11? |
| 10 | Is **`KartuStokSummaryDal`** (movement-based, period + warehouse required) in scope or out — M11 deferred Kartu Stok? |
| 11 | Should empty/null Supplier or Kategori names be grouped as **"Unknown"** or excluded? |
| 12 | Should M11 report gain Supplier/Kategori columns as part of M15 traceability, or dashboard only? |

### 8.4 Cross-milestone

| # | Question |
| - | -------- |
| 1 | Delivery order: M13 → M14 → M15, or parallel? M13 has lowest discovery risk. |
| 2 | Should all three dashboards remain on **`/dashboard`** home or get dedicated sub-routes? |
| 3 | Are **date-range query parameters** deferred to post-M15 (per M10–M12 Decision 9) for dashboards too? |
| 4 | Should new dashboard widgets include **footer reconciliation links** to M9–M12 reports? |
| 5 | Server-side aggregation vs client-side on full dataset (M10/M11 pattern) for chart data? |

---

## 9. Recommended Analysis Findings

These are **discovery conclusions for planning** — not implementation recommendations or product decisions.

### 9.1 M13 has the strongest reuse path

Target vs achievement and sales ranking are **fully implemented** in `SalesOmzetChartSummaryBuilder`, `SalesOmzetTargetResolver`, and `SalesOmzetChartAchievementPolicy`. Portal already registers these services and loads omzet data via `DashboardSalesDal`. M8 implementation summary explicitly deferred target and manager comparison as "future milestone" work.

**Facts for planners:**

- Wire `targetAmount` from `SalesOmzetTargetResolver` into existing `Build()` call
- Expose `BuildManagerComparison()` output separately from weekly trend
- Desktop `SalesOmzetChartForm` is the authoritative UX reference for labels, chart modes, and top-15 behavior

### 9.2 M14 requires new aggregation layer

Open receivable **raw data is ready** via `IPiutangSalesWilayahDal` and M10 `PiutangReportDal`. **`JatuhTempo` and `KurangBayar` are present** on every row. However, **aging buckets and customer ranking do not exist** in application layer — Product Owner must define buckets and ranking rules before implementation.

**Facts for planners:**

- Do not search for aging DALs — none exist
- M10 column order (Customer first, Jatuh Tempo emphasized) was explicitly chosen to prepare for M14
- FF1 grid grouping (Sales/Wilayah) is a different analytic slice than Top Customer
- `PenerimaanPelunasanSalesDal` is collections, not outstanding — separate metric family

### 9.3 M15 can reuse StokBalance data with new grouping

`StokBalanceView` already includes **`SupplierName` and `KategoriName`**. Desktop forms demonstrate **group-by aggregation** patterns in `StokBalanceInfoForm.Proses()`. M6/M11 established valuation and In-Transit rules. **No chart builder exists** — category/supplier composition would be new ReportingContext aggregation over existing DAL, analogous to how M10 added footer totals over existing piutang rows.

**Facts for planners:**

- Prefer `IStokBalanceViewDal` for KPI alignment with M6 unless product chooses `IStokBrgSupplierDal`
- Reconcile composition charts against `DashboardInventoryDal` BrgId grouping logic, not naive row sums
- M11 intentionally deferred Supplier/Kategori columns — M15 dashboard may expose breakdown before report columns

### 9.4 Architectural consistency

All three milestones should follow the established pattern:

```
Controller → MediatR → ReportingContext Dashboard*Dal → existing Desktop DAL + optional Builder/Policy
```

M13 extends an existing dashboard DAL. M14 and M15 likely add **aggregation methods** in dashboard DAL wrappers similar to M10/M11 footer logic — but **without inventing business rules** where desktop rules already exist (M13) and **with explicit PO decisions** where they do not (M14 aging, M15 top-N).

### 9.5 Traceability expectation

M10–M12 established that dashboard KPIs should reconcile to report data where paired reports exist:

| Dashboard | Report | M13–M15 implication |
| --------- | ------ | ------------------- |
| M5 Piutang | M10 Piutang Report | M14 aging/top customer sums should align with open `KurangBayar` totals |
| M6 Inventory | M11 Inventory Report | M15 category/supplier sums should align with M6 valuation rules |
| M4/M8 Sales | M9 Sales Report | M13 ranking/achievement should align with recognized omzet definitions |

---

## Appendix A — File Index

### M13 Sales Dashboard V3

| Category | Path |
| -------- | ---- |
| Desktop chart | `btr.distrib/SalesContext/SalesPersonAgg/SalesOmzetChartForm.cs` |
| Desktop host | `btr.distrib/SalesContext/SalesPersonAgg/SalesOmzetInfoForm.cs` |
| Builder | `btr.application/SalesContext/SalesOmzetAgg/Services/SalesOmzetChartSummaryBuilder.cs` |
| Target resolver | `btr.application/SalesContext/SalesOmzetAgg/Services/SalesOmzetTargetResolver.cs` |
| Achievement policy | `btr.application/SalesContext/SalesOmzetAgg/Policies/SalesOmzetChartAchievementPolicy.cs` |
| Amount policy | `btr.application/SalesContext/SalesOmzetAgg/Policies/SalesOmzetChartAmountPolicy.cs` |
| Target DAL | `btr.infrastructure/SalesContext/SalesOmzetAgg/SalesOmzetTargetDal.cs` |
| Target table | `btr.sql/Tables/SalesContext/BTR_SalesOmzetTarget.sql` |
| Portal dashboard DAL | `btr.infrastructure/ReportingContext/DashboardSalesAgg/DashboardSalesDal.cs` |
| Portal DI | `btr.portal.api/Configurations/ApplicationPortalExtensions.cs` |
| Portal frontend | `btr.portal.web/src/components/SalesTrendCard.vue` |
| Tests | `btr.test/SalesContext/SalesOmzetChartSummaryTest.cs`, `SalesOmzetTargetTest.cs` |

### M14 Piutang Dashboard V2

| Category | Path |
| -------- | ---- |
| Desktop screen | `btr.distrib/FinanceContext/PiutangSalesWilayahRpt/PiutangSalesWilayahForm.cs` |
| DAL | `btr.infrastructure/FinanceContext/PiutangSalesWilayahRpt/PiutangSalesWilayahDal.cs` |
| DTO | `btr.application/FinanceContext/PiutangAgg/Contracts/IPiutangSalesWilayahDal.cs` |
| Portal dashboard | `btr.infrastructure/ReportingContext/DashboardPiutangAgg/DashboardPiutangDal.cs` |
| Portal report (M10) | `btr.infrastructure/ReportingContext/PiutangReportAgg/PiutangReportDal.cs` |
| Collection report | `btr.infrastructure/FinanceContext/PiutangAgg/PenerimaanPelunasanSalesDal.cs` |
| Tracker (drilldown) | `btr.distrib/FinanceContext/TagihanAgg/PiutangTrackerForm.cs` |

### M15 Inventory Dashboard V2

| Category | Path |
| -------- | ---- |
| Desktop — balance + filters | `btr.distrib/InventoryContext/StokBalanceRpt/StokBalanceInfoForm.cs` |
| Desktop — IF1 grid | `btr.distrib/InventoryContext/StokBalanceRpt/StokBalanceInfo2Form.cs` |
| Desktop — supplier report | `btr.distrib/InventoryContext/StokBrgSupplierRpt/StokBrgSupplierForm.cs` |
| DAL — primary | `btr.infrastructure/InventoryContext/StokBalanceRpt/StokBalanceViewDal.cs` |
| DTO | `btr.application/InventoryContext/StokBalanceInfo/StokBalanceView.cs` |
| DAL — supplier variant | `btr.infrastructure/InventoryContext/StokBrgSupplierRpt/StokBrgSupplierDal.cs` |
| Portal dashboard | `btr.infrastructure/ReportingContext/DashboardInventoryAgg/DashboardInventoryDal.cs` |
| Portal report (M11) | `btr.infrastructure/ReportingContext/InventoryReportAgg/InventoryReportDal.cs` |
| Movement summary (alternate) | `btr.infrastructure/InventoryContext/KartuStokRpt/KartuStokSummaryDal.cs` |

### Portal shared

| Category | Path |
| -------- | ---- |
| Dashboard home | `btr.portal.web/src/views/dashboard/DashboardHomeView.vue` |
| KPI card | `btr.portal.web/src/components/KpiCard.vue` |
| Report summary bar | `btr.portal.web/src/components/reports/ReportSummaryBar.vue` |
| Milestone history | `docs/work/btr-portal-api-scaffolding/btr-portal-milestone.md` |
