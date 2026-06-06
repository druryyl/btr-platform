# BTR Portal Analysis — Milestones M13–M15 (Final)

**Status:** Product decisions approved (Round 1 + Round 2) — authoritative input for Architect.  
**Scope:** Discovery and analysis with finalized product decisions. No implementation plans.  
**Date:** 2026-06-06  
**Supersedes:** `portal-analysis-m13-m15.md`, Open Questions in prior draft of this document  
**Context:** BTR Portal is a read-only reporting and dashboard application. M1–M12 are complete.

**Reference documents:** `btr-portal-milestone.md`, `portal-analysis-m13-m15.md`, `portal-analysis-m10-m12-final.md`, `implementation-summary-milestone-8.md`, `implementation-summary-m10.md`, `implementation-summary-m11.md`, `btr-reporting-investigation.md`

---

## 1. Executive Summary

M13–M15 add detailed analytics on **separate dashboard routes**, while `/dashboard` home keeps summary KPI cards with links to detail pages. Product Owner has approved all metrics, layouts, aggregation rules, routes, and API extension pattern.

| Milestone | Route | API | Approved scope (summary) |
| --------- | ----- | --- | ------------------------ |
| **M13** Sales Dashboard V3 | `/dashboard/sales` | Extend `GET /api/dashboard/sales` | Current month; Total Target = sum all targets for month; company Target vs Achievement bar; keep M8 weekly trend; Top 10 ranking |
| **M14** Piutang Dashboard V2 | `/dashboard/piutang` | Extend `GET /api/dashboard/piutang` | Open balance; 5 aging buckets (inclusive boundaries); overdue customer KPI; pie + Top 10 table |
| **M15** Inventory Dashboard V2 | `/dashboard/inventory` | Extend `GET /api/dashboard/inventory` | BrgId-first aggregation; HPP × Qty; exclude In-Transit + Qty ≤ 0; horizontal bar charts; Top 10 tables |

**Key architectural finding:**

- **M13** reuses `SalesOmzetChartSummaryBuilder`, `SalesOmzetChartAchievementPolicy`, `BuildManagerComparison(topCount: 10)`. New work: **sum all monthly targets** from `BTR_SalesOmzetTarget` (not single-rep `SalesOmzetTargetResolver` scope).
- **M14** requires new aging/ranking aggregation over `IPiutangSalesWilayahDal` — bucket rules now fully specified.
- **M15** requires BrgId-first grouping (match M6), then category/supplier rollup — reconciles with inventory KPI.

**Cross-cutting conventions (approved):**

- **Routes:** `/dashboard` = summary cards + links; detail on `/dashboard/sales`, `/dashboard/piutang`, `/dashboard/inventory`
- **API:** Extend existing dashboard GET endpoints with new response sections (M8 pattern) — no new endpoints
- Top N = **10** for salesman, customer, category, supplier
- No drilldown, custom periods, or transaction screens in M13–M15
- Deferred M16+: margin analysis, collection effectiveness, ABC inventory, warehouse analysis, drilldown, custom periods, pie/donut composition views (inventory)

**All open questions resolved.** Ready for Architect implementation planning.

**Recommended delivery order:** M13 → M14 → M15.

---

## 2. M13 Analysis — Sales Dashboard V3

### 2.1 Goal

Enhance sales dashboard with **Target vs Achievement** and **Sales Ranking** on dedicated route `/dashboard/sales`.

### 2.2 Approved Product Scope

| Aspect | Approved decision |
| ------ | ----------------- |
| **Achievement metric** | `Achievement % = Completed Omzet / Monthly Target × 100%` (aggregate KPI row) |
| **Total Target** | Sum of `TargetAmount` for **all salespeople with a target row** for the selected month — **not** conditioned on achievement > 0 |
| **Ranking metric** | Completed Omzet; exclude Pipeline; descending |
| **Ranking period** | Current calendar month |
| **Top N salesman** | Top 10 |
| **Target vs Achievement chart** | **Company-level** two-bar comparison (Target \| Achievement) — not per-salesman |
| **Weekly trend** | **Keep M8** weekly trend card; place **below** Target vs Achievement chart |
| **Drilldown** | Not in M13 |
| **Route** | `/dashboard/sales` |
| **API** | Extend `GET /api/dashboard/sales` response DTO |

#### Approved KPI row

| KPI | Calculation |
| --- | ----------- |
| Total Target | `Sum(TargetAmount)` for all rows in `BTR_SalesOmzetTarget` where `TargetYear` + `TargetMonth` = current month |
| Total Achievement | Sum of Completed Omzet for current month (recognized omzet) |
| Achievement % | `SalesOmzetChartAchievementPolicy.ComputePercent(Total Achievement, Total Target)` |

#### Approved page layout (`/dashboard/sales`)

```
1. KPI Row        → Total Target | Total Achievement | Achievement %
2. Chart          → Target vs Achievement (company-level bar chart)
3. Chart          → Weekly Trend (M8 — line chart, unchanged data)
4. Table          → Top 10 Salesman (Completed Omzet, descending)
```

#### `/dashboard` home (summary only)

- Retain compact Sales KPI card (Total Omzet, Faktur, Customer — M4/M7)
- Add link/navigation to `/dashboard/sales` for full analytics

### 2.3 Mapping to existing BTR logic

| Approved rule | Existing implementation | Portal adjustment |
| ------------- | ----------------------- | ----------------- |
| Completed Omzet | `SalesOmzetChartAmountPolicy` — Completed → `FakturTotal` | Same |
| Total Achievement | `SalesOmzetChartSummaryBuilder.Build()` → `RecognizedOmzet` | Same as M8 `TotalOmzet` |
| Achievement % | `SalesOmzetChartAchievementPolicy` | Apply to aggregate totals |
| Ranking | `BuildManagerComparison(rows, topCount: 10)` | Override desktop default 15 |
| Period | `DashboardSalesDal.CurrentMonthPeriode()` | Same |
| Period mode | `SalesOmzetPeriodFilterMode.OmzetPeriod` | Pipeline excluded from achievement/ranking |
| Total Target | `ISalesOmzetTargetDal.GetTargetAmount` per rep | **New aggregation:** sum all targets for month — do **not** use `SalesOmzetTargetResolver` (single-rep scope) |
| Weekly trend | M8 `summary.ByWeek` | Unchanged; reposition on page only |

### 2.4 Reusable assets

| Asset | Reuse rationale |
| ----- | --------------- |
| `ISalesOmzetDal` | Same source as M4/M8 |
| `SalesOmzetChartSummaryBuilder` | Achievement, weekly, ranking |
| `SalesOmzetChartAchievementPolicy` | Percent — not capped at 100% |
| `ISalesOmzetTargetDal` + `BTR_SalesOmzetTarget` | Target amounts — may need list/sum query for all reps in month |
| `SalesOmzetChartForm` | Reference for company KPI + bar intent |
| `SalesTrendCard.vue` | Weekly trend — reuse on `/dashboard/sales` |

### 2.5 What should NOT be duplicated

| Do not duplicate | Reason |
| ---------------- | ------ |
| Omzet SQL / status rules | Use existing policies + DAL |
| Per-salesman target resolver for Total Target | Resolver is ambiguous-scope; M13 uses full target table sum |
| Desktop top-15 constant | Portal uses Top 10 |

### 2.6 Dashboard traceability

| M13 metric | Reconciliation |
| ---------- | -------------- |
| Total Achievement | = M8 `TotalOmzet` / `CompletedOmzet` for same month |
| Sum of all salesman Completed Omzet | = Total Achievement |
| Sum of Top 10 ranking | ≤ Total Achievement |
| Total Target | = Direct sum from `BTR_SalesOmzetTarget` for month |

---

## 3. M14 Analysis — Piutang Dashboard V2

### 3.1 Goal

Enhance piutang dashboard with **Aging Analysis** and **Top Customer** on `/dashboard/piutang`.

### 3.2 Approved Product Scope

| Aspect | Approved decision |
| ------ | ----------------- |
| **Data scope** | Current outstanding balance (M5/M10: `2000-01-01 → today`, `KurangBayar > 1`) |
| **Aging basis** | `DaysOverdue = Today − JatuhTempo` (calendar days) |
| **Top customer metric** | `Sum(KurangBayar)` per customer; descending |
| **Top N** | Top 10 |
| **Customer key** | **`CustomerCode`** for aggregation; display **`CustomerName`** in UI |
| **Overdue Customer KPI** | Customers where `Sum(overdue bucket amounts) > 0` — **Current bucket excluded** from overdue test |
| **Collection KPI** | Out of scope |
| **Route** | `/dashboard/piutang` |
| **API** | Extend `GET /api/dashboard/piutang` |

#### Approved aging buckets (inclusive boundaries)

| Bucket | Rule |
| ------ | ---- |
| **Current** (Not Yet Due) | `DaysOverdue ≤ 0` |
| **1–30 Days** | `1 ≤ DaysOverdue ≤ 30` |
| **31–60 Days** | `31 ≤ DaysOverdue ≤ 60` |
| **61–90 Days** | `61 ≤ DaysOverdue ≤ 90` |
| **> 90 Days** | `DaysOverdue > 90` |

Bucket amount = `Sum(KurangBayar)` of open faktur assigned to bucket.

**Reconciliation:** Sum of all five bucket amounts = Total Piutang.

#### Approved KPI row

| KPI | Calculation |
| --- | ----------- |
| Total Piutang | M5 — `Sum(KurangBayar)` where `> 1` |
| Total Customer | M5 — distinct count by `CustomerCode` (fallback per M5/M10 if code empty — see note below) |
| Overdue Customer | Distinct customers where sum of balances in buckets 1–30, 31–60, 61–90, > 90 is **> 0** |

**Note on customer key:** Product decision specifies `CustomerCode` as primary aggregation key. M5/M10 `ResolveCustomerKey()` uses `CustomerCode` when non-empty, else `CustomerName`. Implementation should use **`CustomerCode` when present**; define fallback for empty code consistently with M10 (Architect to align — likely `CustomerName` fallback to avoid collapsing unrelated rows).

#### Approved page layout (`/dashboard/piutang`)

```
1. KPI Row   → Total Piutang | Total Customer | Overdue Customer
2. Chart     → Aging Distribution (Pie Chart) — 5 buckets
3. Table     → Top 10 Outstanding Customers (CustomerName, Outstanding Balance)
```

#### `/dashboard` home

- Compact Piutang KPI card (M5) + link to `/dashboard/piutang`

### 3.3 Mapping to existing BTR logic

| Approved rule | Source | New work |
| ------------- | ------ | -------- |
| Open rows | `IPiutangSalesWilayahDal` | Reuse M5/M10 period + filter |
| Aging assignment | None in desktop | New bucket function from approved boundaries |
| Top 10 customers | None in desktop | `GroupBy CustomerCode`, `Sum(KurangBayar)`, `Take(10)` |
| Overdue customer count | None in desktop | Group by customer; test overdue bucket sum > 0 |
| Reference date | `ITglJamDal.Now.Date` | For `DaysOverdue` |

### 3.4 Reusable assets

| Asset | Reuse |
| ----- | ----- |
| `IPiutangSalesWilayahDal` | FF1 / M10 source |
| `DashboardPiutangDal` | Existing KPIs |
| `PiutangReportDal` | Traceability to M10 report |
| M10 customer key pattern | Aggregation consistency |

### 3.5 Dashboard traceability

| M14 metric | Reconciliation |
| ---------- | -------------- |
| Total Piutang | = M5 home card = M10 report footer |
| Total Customer | = M5 = M10 footer (same key logic) |
| Sum of aging buckets | = Total Piutang |
| Overdue Customer | ≤ Total Customer |
| Top 10 sum | ≤ Total Piutang |

---

## 4. M15 Analysis — Inventory Dashboard V2

### 4.1 Goal

Enhance inventory dashboard with **Category** and **Supplier** analysis on `/dashboard/inventory`.

### 4.2 Approved Product Scope

| Aspect | Approved decision |
| ------ | ----------------- |
| **Valuation** | Inventory Value = `HPP × Qty` |
| **Grouping** | **BrgId first** (match M6 `DashboardInventoryDal`), then roll up to Category / Supplier |
| **Warehouse scope** | Exclude In-Transit |
| **Quantity filter** | Exclude `Qty ≤ 0` |
| **Blank category/supplier** | Display as **"Unknown"** — do not exclude |
| **Category chart** | **Horizontal bar chart** |
| **Supplier chart** | **Horizontal bar chart** |
| **Rankings** | Top 10 categories + Top 10 suppliers by inventory value |
| **Route** | `/dashboard/inventory` |
| **API** | Extend `GET /api/dashboard/inventory` |

#### Approved aggregation pipeline

```
1. Load IStokBalanceViewDal.ListData()
2. Exclude WarehouseName = "In-Transit"
3. Group by BrgId → Sum(Qty), Sum(Hpp × Qty) per item   [same as M6]
4. Exclude groups where aggregated Qty ≤ 0
5. Map each BrgId to KategoriName / SupplierName (blank → "Unknown")
6. Group by Category or Supplier → Sum(inventory value)
7. Top 10 for tables; full or top-N for charts (Architect: charts show ranked bars — likely Top 10 or all with scroll)
```

#### Approved page layout (`/dashboard/inventory`)

```
1. KPI Row   → Total Inventory Value | Total Item
2. Chart     → Inventory by Category (horizontal bar)
3. Chart     → Inventory by Supplier (horizontal bar)
4. Table     → Top 10 Categories
5. Table     → Top 10 Suppliers
```

#### `/dashboard` home

- Compact Inventory KPI card (M6) + link to `/dashboard/inventory`

### 4.3 Mapping to existing BTR logic

| Approved rule | Source |
| ------------- | ------ |
| BrgId grouping | `DashboardInventoryDal` verbatim |
| In-Transit exclusion | `InTransitWarehouseName = "In-Transit"` |
| Total Inventory Value / Total Item | Existing M6 KPI logic unchanged |
| Category/supplier fields | `StokBalanceView.KategoriName`, `SupplierName` |
| Pie/donut composition | **Not in M15** — future milestone |

### 4.4 Reusable assets

| Asset | Reuse |
| ----- | ----- |
| `IStokBalanceViewDal` | M6/M11 source |
| `DashboardInventoryDal` | KPI + BrgId grouping reference |
| `InventoryReportDal` | M11 traceability |
| M11 `InventoryReportDalTest` | Documents grouping expectations |

### 4.5 Dashboard traceability

| M15 metric | Reconciliation |
| ---------- | -------------- |
| Total Inventory Value | = M6 home card = M11 report footer |
| Total Item | = M6 = M11 footer |
| Sum of all category values (incl. Unknown) | = Total Inventory Value |
| Sum of all supplier values (incl. Unknown) | = Total Inventory Value |
| Top 10 sums | ≤ totals |

---

## 5. Dashboard UX — Approved Layouts

### 5.1 Navigation architecture (Round 2)

```
/dashboard                 → Summary KPI cards (Sales, Piutang, Inventory) + links
/dashboard/sales           → M13 full analytics
/dashboard/piutang         → M14 full analytics
/dashboard/inventory       → M15 full analytics
/reports/*                 → M9–M12 (unchanged)
```

Sidebar: add nested Dashboard menu items or links from home cards.

### 5.2 M13 — `/dashboard/sales`

```
+--------------------------------------------------+
| KPI: Total Target | Total Achievement | Ach %  |
+--------------------------------------------------+
| TARGET VS ACHIEVEMENT (Bar — company level)      |
| [ Target ] [ Achievement ]                       |
+--------------------------------------------------+
| WEEKLY TREND (M8 line chart)                     |
+--------------------------------------------------+
| TOP 10 SALESMAN (Table)                          |
| Rank | Salesman | Completed Omzet                |
+--------------------------------------------------+
```

### 5.3 M14 — `/dashboard/piutang`

```
+--------------------------------------------------+
| KPI: Total Piutang | Total Customer | Overdue    |
+--------------------------------------------------+
| AGING DISTRIBUTION (Pie — 5 buckets)             |
+--------------------------------------------------+
| TOP 10 OUTSTANDING CUSTOMERS (Table)             |
| Rank | Customer Name | Outstanding Balance         |
+--------------------------------------------------+
```

### 5.4 M15 — `/dashboard/inventory`

```
+--------------------------------------------------+
| KPI: Total Inventory Value | Total Item          |
+--------------------------------------------------+
| INVENTORY BY CATEGORY (Horizontal bar)           |
+--------------------------------------------------+
| INVENTORY BY SUPPLIER (Horizontal bar)           |
+--------------------------------------------------+
| TOP 10 CATEGORIES (Table)                        |
+--------------------------------------------------+
| TOP 10 SUPPLIERS (Table)                         |
+--------------------------------------------------+
```

### 5.5 UI component conventions

| Component | Usage |
| --------- | ----- |
| KPI cards | All three detail pages + home summary |
| Vertical/grouped bar | M13 Target vs Achievement |
| Line chart | M13 Weekly Trend (M8) |
| Pie chart | M14 aging only |
| Horizontal bar | M15 category + supplier |
| DataTable Top 10 | All three detail pages |

---

## 6. Cross-Milestone Comparison (Final)

| Milestone | Screen | DAL | Builder | Complexity | Risk |
| --------- | ------ | --- | ------- | ---------- | ---- |
| **M13** | `SalesOmzetChartForm` | `ISalesOmzetDal`, `ISalesOmzetTargetDal` | `SalesOmzetChartSummaryBuilder` | **Low** | **Low** |
| **M14** | `PiutangSalesWilayahForm` | `IPiutangSalesWilayahDal` | New aging aggregation | **Medium** | **Low–Medium** (large dataset) |
| **M15** | `StokBalanceInfoForm` | `IStokBalanceViewDal` | New category/supplier rollup | **Medium** | **Low** |

### 6.1 API extension pattern (approved)

Extend existing endpoints — add sections to response DTOs:

| Endpoint | Existing fields (M4–M8) | New sections (M13–M15) |
| -------- | ------------------------ | ------------------------ |
| `GET /api/dashboard/sales` | TotalOmzet, WeeklyTrend, … | TotalTarget, TotalAchievement, AchievementPercent, TargetVsAchievement chart data, TopSalesmanRanking[] |
| `GET /api/dashboard/piutang` | TotalPiutang, TotalCustomer | OverdueCustomer, AgingBuckets[], TopCustomers[] |
| `GET /api/dashboard/inventory` | TotalInventoryValue, TotalItem | CategoryBreakdown[], SupplierBreakdown[], TopCategories[], TopSuppliers[] |

No new routes under `/api`. Frontend detail pages call same endpoints as home (or dedicated store fetch per route).

### 6.2 Date scope

| Domain | Scope |
| ------ | ----- |
| Sales | Current calendar month |
| Piutang | Current outstanding snapshot |
| Inventory | Point-in-time snapshot |

---

## 7. Risks

| Risk | Milestone | Severity | Mitigation |
| ---- | --------- | -------- | ---------- |
| Sum-all-targets query | M13 | Low | May need new DAL list method — table is small |
| Piutang row volume (~11K) | M14 | Medium | Server-side aggregation in DAL (same as M10 load) |
| Empty CustomerCode rows | M14 | Low | Fallback to CustomerName aligned with M10 |
| BrgId → category mapping | M15 | Low | One category/supplier per BrgId from balance view |
| Home vs detail data duplication | All | Low | Same API endpoints; detail pages consume full response |
| Chart.js horizontal bar | M15 | Low | Chart.js indexAxis: 'y' — same library as M8 |

**All product-rule risks resolved in Round 2.**

---

## 8. Final Product Decisions (Authoritative)

### Round 1 — Metrics, layout, and scope

| # | Decision |
| - | -------- |
| 1 | M13 Achievement = Completed Omzet / Monthly Target × 100% |
| 2 | M13 Ranking = Completed Omzet, exclude Pipeline, descending |
| 3 | M13 Period = current month |
| 4 | M13 Top 10 salesman |
| 5 | M13 Layout = KPI + Target vs Achievement bar + Top 10 table; no drilldown |
| 6 | M14 Aging buckets: Current, 1–30, 31–60, 61–90, > 90 |
| 7 | M14 Aging basis = JatuhTempo; DaysOverdue = Today − JatuhTempo |
| 8 | M14 Top 10 customers by KurangBayar |
| 9 | M14 Layout = KPI + aging pie + Top 10 table; collections out of scope |
| 10 | M15 Valuation = HPP × Qty for category and supplier |
| 11 | M15 Top 10 categories and suppliers |
| 12 | M15 Exclude In-Transit |
| 13 | M15 Layout = KPI + 2 charts + 2 tables |
| 14 | UI: KPI cards, bar, pie, tables; no drilldown/transactions |
| 15 | Top 10 standardized |
| 16 | Sales = current month; Piutang = outstanding snapshot |
| 17 | M16+ deferrals: margin, collection effectiveness, ABC, warehouse, drilldown, custom periods |

### Round 2 — Aggregation, navigation, and API

| # | Decision | Implementation direction |
| - | -------- | ------------------------ |
| 18 | **Total Target** = sum targets for all salespeople with a target row for the month; **not** gated on achievement | Query/sum `BTR_SalesOmzetTarget` for `(TargetYear, TargetMonth)` |
| 19 | **Target vs Achievement chart** = company-level two-bar (Target \| Achievement) | Not per-salesman |
| 20 | **Weekly Trend** = keep M8; order: KPI → Target vs Achievement → Weekly Trend → Top 10 | Reuse `SalesTrendCard` / `WeeklyTrend` on `/dashboard/sales` |
| 21 | **Overdue Customer** = customers with `Sum(overdue buckets) > 0`; Current bucket excluded | Group by CustomerCode after bucket assignment |
| 22 | **Bucket boundaries** = Current ≤0; 1–30: 1–30; 31–60: 31–60; 61–90: 61–90; >90: >90 | Inclusive ranges as specified |
| 23 | **Customer key** = CustomerCode for aggregation; CustomerName for display | Align with M5/M10 |
| 24 | **Zero qty** = exclude Qty ≤ 0 from M15 analytics | After BrgId grouping |
| 25 | **M15 charts** = horizontal bar for category and supplier | Pie/deferred to future composition view |
| 26 | **Blank supplier/category** = show as "Unknown" | Include in totals |
| 27 | **M15 grouping** = BrgId first (M6 logic), then category/supplier | KPI reconciliation |
| 28 | **Placement** = separate routes `/dashboard/sales`, `/dashboard/piutang`, `/dashboard/inventory`; home = summary + links | Refactor `DashboardHomeView` |
| 29 | **API** = extend existing `GET /api/dashboard/{sales|piutang|inventory}` | No new endpoints |

---

## 9. Recommended Analysis Findings

### 9.1 M13 — ready for implementation

All rules map to existing builders/policies. New code: sum-all-targets, company bar chart DTO, Top 10 table DTO, `/dashboard/sales` view, extend `DashboardSalesResponse`, reposition weekly trend.

### 9.2 M14 — ready for implementation

Aging buckets and overdue customer fully specified. New code: bucket assignment function, pie chart + table DTOs, `OverdueCustomer` count, extend `DashboardPiutangResponse`, `/dashboard/piutang` view.

### 9.3 M15 — ready for implementation

BrgId-first pipeline matches M6 — KPI reconciliation path clear. New code: category/supplier rollup after M6 grouping, horizontal bar chart data, Top 10 tables, extend `DashboardInventoryResponse`, `/dashboard/inventory` view.

### 9.4 Frontend architecture

- Refactor dashboard into **home summary** + **three detail views**
- Router: add `/dashboard/sales`, `/dashboard/piutang`, `/dashboard/inventory`
- Stores: extend `dashboardStore` or split per-domain stores — same API endpoints
- Sidebar: Dashboard group with sub-items

### 9.5 Delivery order

**M13 → M14 → M15.** M13 establishes detail-page pattern and chart extensions. M14/M15 can parallelize after M13 page scaffold exists.

---

## Appendix A — File Index

### M13

| Category | Path |
| -------- | ---- |
| Builder | `btr.application/.../SalesOmzetChartSummaryBuilder.cs` |
| Policies | `SalesOmzetChartAchievementPolicy.cs`, `SalesOmzetChartAmountPolicy.cs` |
| Target DAL | `btr.infrastructure/.../SalesOmzetTargetDal.cs` |
| Portal DAL | `btr.infrastructure/ReportingContext/DashboardSalesAgg/DashboardSalesDal.cs` |
| Query/DTO | `btr.application/ReportingContext/DashboardSalesAgg/Queries/GetDashboardSalesQuery.cs` |
| Frontend | `btr.portal.web/src/views/dashboard/`, `SalesTrendCard.vue` |

### M14

| Category | Path |
| -------- | ---- |
| DAL | `btr.infrastructure/FinanceContext/PiutangSalesWilayahRpt/PiutangSalesWilayahDal.cs` |
| Portal DAL | `btr.infrastructure/ReportingContext/DashboardPiutangAgg/DashboardPiutangDal.cs` |
| M10 wrapper | `btr.infrastructure/ReportingContext/PiutangReportAgg/PiutangReportDal.cs` |

### M15

| Category | Path |
| -------- | ---- |
| DAL | `btr.infrastructure/InventoryContext/StokBalanceRpt/StokBalanceViewDal.cs` |
| Portal DAL | `btr.infrastructure/ReportingContext/DashboardInventoryAgg/DashboardInventoryDal.cs` |
| M11 wrapper | `btr.infrastructure/ReportingContext/InventoryReportAgg/InventoryReportDal.cs` |

### Portal routing (new)

| Route | Purpose |
| ----- | ------- |
| `/dashboard` | Summary KPI cards + links |
| `/dashboard/sales` | M13 |
| `/dashboard/piutang` | M14 |
| `/dashboard/inventory` | M15 |
