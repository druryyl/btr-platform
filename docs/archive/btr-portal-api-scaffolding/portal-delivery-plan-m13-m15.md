# Portal Delivery Plan: M13–M15

## Document Status

| Field | Value |
| --- | --- |
| Scope | M13 Sales Dashboard V3, M14 Piutang Dashboard V2, M15 Inventory Dashboard V2 |
| Authoritative requirements | `portal-analysis-m13-m15-final.md` |
| Individual plans | `implementation-plan-m13-sales-dashboard-v3.md`, `implementation-plan-m14-piutang-dashboard-v2.md`, `implementation-plan-m15-inventory-dashboard-v2.md` |
| Reference pattern | M8 dashboard extension + M10–M12 delivery plan |
| Date | 2026-06-06 |

---

## 1. Shared Architecture

M13–M15 extend the M4–M8 dashboard pattern: **same GET endpoints, extended response DTOs, no new API routes**. Frontend adds detail pages under `/dashboard/{domain}` while `/dashboard` home remains summary KPI cards with navigation links.

```text
Vue 3 (PrimeVue Chart + DataTable)
    ↓ Axios
btr.portal.api (existing Dashboard Controllers)
    ↓ MediatR
ReportingContext/Dashboard{Domain}Agg (Application — extended DTOs)
    ↓ IDashboard{Domain}Dal
Dashboard{Domain}Dal (Infrastructure — extended mapping)
    ↓ Existing Desktop DAL / Builder
SQL Server
```

### 1.1 Milestone Summary

| Milestone | Route | API | Primary new backend work | Primary new frontend work |
| --- | --- | --- | --- | --- |
| **M13** | `/dashboard/sales` | `GET /api/dashboard/sales` | Target sum, ranking via builder | Detail page scaffold, bar + line charts |
| **M14** | `/dashboard/piutang` | `GET /api/dashboard/piutang` | Aging buckets, overdue count | Pie chart |
| **M15** | `/dashboard/inventory` | `GET /api/dashboard/inventory` | Category/supplier rollup | Horizontal bar charts |

---

## 2. Shared Backend Components

### 2.1 Shared Dashboard DTO Patterns

All three milestones extend existing `Dashboard{Domain}Response` in `GetDashboard{Domain}Query.cs` (query + handler + DTOs in one file — M3/M8 convention).

| Pattern | Convention | M13 | M14 | M15 |
| --- | --- | --- | --- | --- |
| Response wrapper | `Dashboard{Domain}Response` | Extended | Extended | Extended |
| Timestamp | `GeneratedAt` | ✓ | ✓ | ✓ |
| Existing KPI fields | Unchanged semantics | ✓ | ✓ | ✓ |
| Additive new fields | Backward compatible | ✓ | ✓ | ✓ |
| API envelope | `ApiResponse<T>` | ✓ | ✓ | ✓ |

**No new endpoints.** Controllers, MediatR handlers, and DI registrations for dashboard DALs remain unchanged (M13 adds method to `ISalesOmzetTargetDal` only).

### 2.2 Shared Ranking DTO Patterns

| DTO | Milestone | Properties |
| --- | --- | --- |
| `DashboardSalesRankingItem` | M13 | `Rank`, `SalesPersonName`, `CompletedOmzet` |
| `DashboardPiutangTopCustomer` | M14 | `Rank`, `CustomerName`, `OutstandingBalance` |
| `DashboardInventoryRankingItem` | M15 | `Rank`, `Name`, `InventoryValue` |

**Shared conventions:**

| Rule | Value |
| --- | --- |
| Top N | **10** (all milestones) |
| Rank | 1-based integer assigned after sort |
| Sort | Descending by primary metric |
| Tie-break | Ascending by display name (`StringComparer.OrdinalIgnoreCase`) |
| API pre-sorts | Frontend does not re-sort |

M15 reuses `DashboardInventoryRankingItem` for both category and supplier tables.

### 2.3 Shared Chart DTO Patterns

| DTO | Milestone | Chart type | Properties |
| --- | --- | --- | --- |
| `DashboardSalesTargetVsAchievement` | M13 | Grouped bar | `TargetAmount`, `AchievementAmount` |
| `DashboardSalesWeekTrendItem` | M13 (existing M8) | Line | `WeekLabel`, `RecognizedAmount`, … |
| `DashboardPiutangAgingBucket` | M14 | Pie | `BucketKey`, `BucketLabel`, `Amount`, `SortOrder` |
| `DashboardInventoryBreakdownItem` | M15 | Horizontal bar | `Name`, `InventoryValue` |

**Chart data rules:**

| Rule | Detail |
| --- | --- |
| Server-side aggregation | All chart values computed in DAL — never client-summed |
| Empty states | Frontend handles all-zero datasets |
| Currency | Monetary values as `decimal` in API; `formatCurrency` in tooltips/axes |
| Stable ordering | Aging buckets use `SortOrder`; rankings pre-sorted |

### 2.4 Shared Aggregation Patterns

| Pattern | Source | Used by |
| --- | --- | --- |
| Current month period | `DashboardSalesDal.CurrentMonthPeriode()` | M13 |
| Omzet period mode | `SalesOmzetPeriodFilterMode.OmzetPeriod` | M13 |
| Open receivables period | `2000-01-01 → today` | M14 (same as M5/M10) |
| Open balance filter | `KurangBayar > 1` | M14 |
| Customer key resolution | `CustomerCode` → fallback `CustomerName` | M14 |
| In-Transit exclusion | `WarehouseName != "In-Transit"` | M15 |
| BrgId-first grouping | Group by `BrgId`, sum Qty and Hpp×Qty | M15 (same as M6/M11) |
| Qty > 0 filter | After BrgId aggregation | M15 |
| Unknown dimension label | Blank category/supplier → `"Unknown"` | M15 |

**Reconciliation rule (all milestones):** New analytics must reconcile with existing dashboard KPIs and corresponding report footers (M9/M10/M11).

---

## 3. Shared Frontend Components

### 3.1 Component Inventory

| Component | Location | Introduced | Reused by |
| --- | --- | --- | --- |
| `KpiCard.vue` | `src/components/` | M7 | Home + all detail pages |
| `DashboardDetailLayout.vue` | `src/components/dashboard/` | **M13** | M13, M14, M15 |
| `Top10RankingTable.vue` | `src/components/dashboard/` | **M13** | M13, M14, M15 |
| `WeeklyTrendChart.vue` | `src/components/dashboard/` | **M13** | M13 only |
| `TargetVsAchievementChart.vue` | `src/components/dashboard/` | **M13** | M13 only |
| `AgingPieChart.vue` | `src/components/dashboard/` | **M14** | M14 only |
| `InventoryHorizontalBarChart.vue` | `src/components/dashboard/` | **M15** | M15 only (category + supplier) |

### 3.2 KPI Card Reuse

| Location | KPIs shown |
| --- | --- |
| `/dashboard` home | M4/M5/M6 summary (Total Omzet, Faktur, Customer / Total Piutang, Customer / Total Inventory Value, Item) |
| `/dashboard/sales` | Total Target, Total Achievement, Achievement % |
| `/dashboard/piutang` | Total Piutang, Total Customer, Overdue Customer |
| `/dashboard/inventory` | Total Inventory Value, Total Item |

Home cards add **"View details →"** `RouterLink` to respective detail routes.

Detail pages use inline metric divs or `KpiCard` — match home metric styling (`metric`, `metric__label`, `metric__value` classes).

### 3.3 Chart Wrapper Reuse

All charts use **PrimeVue `Chart`** + **Chart.js** (installed in M8).

| Chart | Type | Config highlight |
| --- | --- | --- |
| Target vs Achievement | `bar` | Two categories: Target, Achievement |
| Weekly Trend | `line` | M8 colors (`#22c55e` stroke) |
| Aging Distribution | `pie` | 5 buckets, legend visible |
| Category / Supplier | `bar` | `indexAxis: 'y'` for horizontal bars |

Shared chart conventions:

- `responsive: true`, `maintainAspectRatio: false`
- Fixed canvas height container (~280px minimum)
- Currency tooltips via `formatCurrency`
- Empty-state message when no non-zero data

### 3.4 Top 10 Table Reuse

`Top10RankingTable.vue` — generic PrimeVue `DataTable`:

| Setting | Value |
| --- | --- |
| Pagination | Off (≤ 10 rows) |
| Sorting | Off (API pre-sorted) |
| Striped | Yes |
| Loading | Bound to parent loading |
| Currency column | Via `formatCurrency` |

Each milestone passes column definitions and row data — no per-domain table components.

### 3.5 Dashboard Layout Reuse

`DashboardDetailLayout.vue` provides:

```text
Page title + subtitle
Refresh button (emit @refresh)
Error Message
<slot /> for content
```

All three detail views follow identical page shell. Home (`DashboardHomeView.vue`) keeps its own header pattern (unchanged except removing Sales Trend card).

### 3.6 Shared Pinia Patterns

**Single store:** `dashboardStore.ts` (existing from M7).

| State | Type | Purpose |
| --- | --- | --- |
| `sales` | `DashboardSalesResponse \| null` | Sales API payload |
| `piutang` | `DashboardPiutangResponse \| null` | Piutang API payload |
| `inventory` | `DashboardInventoryResponse \| null` | Inventory API payload |
| `loading` | `boolean` | Request in flight |
| `error` | `string \| null` | User-facing error |

| Action | Behavior | Used by |
| --- | --- | --- |
| `loadDashboard()` | Parallel fetch all 3 endpoints | `/dashboard` home |
| `loadSales()` | Fetch sales only | M13 — **introduced in M13** |
| `loadPiutang()` | Fetch piutang only | M14 — **introduced in M14** |
| `loadInventory()` | Fetch inventory only | M15 — **introduced in M15** |
| `reset()` | Clear state | Logout |

**Rule:** Detail pages call single-domain loaders — not `loadDashboard()` — to avoid unnecessary API traffic.

### 3.7 Shared API / Type Patterns

**File:** `src/models/dashboard.ts` — all dashboard types in single module.

**File:** `src/api/dashboardApi.ts` — existing fetch functions unchanged (extended responses deserialize automatically).

**File:** `src/services/formatters.ts` — `formatCurrency`, `formatNumber`, `formatDateTime`; M13 adds `formatPercent`.

### 3.8 Sidebar Navigation Pattern

**File:** `src/layouts/MainLayout.vue`

Dashboard menu becomes nested group (M13 introduces; M14/M15 extend):

```text
Dashboard
├── Overview      → /dashboard
├── Sales         → /dashboard/sales      (M13)
├── Piutang       → /dashboard/piutang    (M14)
└── Inventory     → /dashboard/inventory  (M15)
```

Reports submenu unchanged.

---

## 4. Recommended Delivery Order

### Order: M13 → M14 → M15

| Order | Milestone | Rationale |
| --- | --- | --- |
| **1** | M13 Sales Dashboard V3 | Lowest complexity; establishes detail-page pattern (`DashboardDetailLayout`, `Top10RankingTable`, router/sidebar refactor); reuses existing builder with minimal new SQL; removes Sales Trend from home — foundational UX change |
| **2** | M14 Piutang Dashboard V2 | Reuses M13 layout + ranking table; introduces pie chart pattern; new aging aggregation is self-contained in piutang DAL |
| **3** | M15 Inventory Dashboard V2 | Reuses M13 layout + ranking table; introduces horizontal bar pattern; BrgId pipeline refactor must preserve M6 KPI parity |

### Why M13 first?

1. Creates shared frontend infrastructure (`DashboardDetailLayout`, `Top10RankingTable`) that M14/M15 depend on
2. Establishes nested Dashboard sidebar and home → detail navigation pattern
3. Lowest backend risk — mostly builder/DAL extension vs new aggregation logic
4. Product-approved page scaffold for M14/M15 to follow

### Milestone dependencies

```text
M7/M8 (complete)
    ↓
M13 (detail page scaffold + shared components + home refactor)
    ↓
M14 (reuses scaffold + pie chart)     M15 (reuses scaffold + horizontal bar)
    ↓                                      ↓
    └──────────── both can parallelize ──────┘
                  after M13 merges
```

M14 and M15 have **no backend dependency** on each other — only shared frontend components from M13.

---

## 5. Parallelization Opportunities

### 5.1 Backend Parallelization

| Task | M13 | M14 | M15 | Can parallelize? |
| --- | --- | --- | --- | --- |
| Extend response DTOs | ✓ | ✓ | ✓ | **Yes** — different query files |
| Extend Dashboard DAL | ✓ | ✓ | ✓ | **Yes** — different DAL classes |
| Target DAL method (M13 only) | ✓ | — | — | M13-only |
| Controller changes | — | — | — | **N/A** — none |
| DI registration | — | — | — | **N/A** — none |
| Unit tests | ✓ | ✓ | ✓ | **Yes** |

**Recommendation:**

- **Single implementer:** M13 → M14 → M15 sequentially (lowest risk).
- **Two implementers after M13:** Developer A: M14 backend + frontend; Developer B: M15 backend + frontend — no merge conflicts on DAL files.
- **Three implementers:** Not recommended until M13 shared frontend lands.

### 5.2 Frontend Parallelization

| Task | M13 | M14 | M15 | Can parallelize? |
| --- | --- | --- | --- | --- |
| `DashboardDetailLayout.vue` | Create | Reuse | Reuse | **M13 must land first** |
| `Top10RankingTable.vue` | Create | Reuse | Reuse | **M13 must land first** |
| Detail view page | ✓ | ✓ | ✓ | **Yes** — after M13 scaffold |
| Domain chart component | Target + Weekly | Aging pie | Horizontal bar | **Yes** — independent files |
| Store load action | `loadSales` | `loadPiutang` | `loadInventory` | **Sequential merge** — same store file |
| Router + sidebar | ✓ | ✓ | ✓ | **Sequential merge** |
| Home view links | Sales link | Piutang link | Inventory link | **Sequential merge** |

**Recommendation:**

- M13 frontend must complete (minimum: `DashboardDetailLayout` + `Top10RankingTable` + `/dashboard/sales` route) before M14/M15 frontend starts.
- M14 and M15 views can be built in parallel once M13 components exist.

### 5.3 Shared Infrastructure — Build First

| Priority | Component / Pattern | Milestone | Blocks |
| --- | --- | --- | --- |
| **P0** | `DashboardDetailLayout.vue` | M13 | M14, M15 page shells |
| **P0** | `Top10RankingTable.vue` | M13 | M14, M15 ranking tables |
| **P0** | Nested Dashboard sidebar + home refactor | M13 | All detail navigation |
| **P1** | `WeeklyTrendChart.vue` extraction | M13 | M13 weekly section only |
| **P1** | `loadSales()` store action | M13 | M13 page data loading |
| **P2** | `formatPercent` helper | M13 | M13 achievement KPI |

Do **not** prematurely extract a generic chart wrapper — domain charts differ enough (bar/line/pie/horizontal) that separate components per M13/M14/M15 plan is correct.

---

## 6. Effort Estimate

Estimates assume one developer familiar with M7/M8 codebase. Includes backend, frontend, manual verification, and implementation summary doc.

| Milestone | Backend | Frontend | Testing | Total |
| --- | --- | --- | --- | --- |
| **M13** Sales Dashboard V3 | 0.5 day | 0.75 day | 0.25 day | **1.5 days** |
| **M14** Piutang Dashboard V2 | 0.5 day | 0.5 day | 0.25 day | **1.25 days** |
| **M15** Inventory Dashboard V2 | 0.5 day | 0.5 day | 0.25 day | **1.25 days** |
| **Total** | **1.5 days** | **1.75 days** | **0.75 days** | **~4 days** |

### Effort breakdown notes

**M13 backend (0.5 day):** Extend DTOs; add `SumTargetAmountForMonth`; wire builder ranking + achievement policy. Straightforward.

**M13 frontend (0.75 day):** Highest frontend effort — home refactor, new route, 3 new shared components, 2 chart components, detail page layout. Amortized across M14/M15.

**M14 backend (0.5 day):** Aging bucket logic + overdue count + top customers in single DAL pass. Careful boundary testing.

**M14 frontend (0.5 day):** Reuses M13 scaffold; one new pie chart component.

**M15 backend (0.5 day):** Refactor M6 grouping into shared method; category/supplier rollups. KPI parity testing important.

**M15 frontend (0.5 day):** Reuses M13 scaffold; one horizontal bar component used twice.

**Testing (0.25 day each):** KPI reconciliation against M5/M6/M8/M10/M11, build verification, screenshots.

### Parallel delivery scenario (2 developers, ~2.5 days)

| Day | Developer A | Developer B |
| --- | --- | --- |
| 1 AM | M13 backend + frontend (shared components) | — |
| 1 PM | M13 verification + merge | M14 backend |
| 2 AM | M14 frontend | M15 backend |
| 2 PM | M14 verification | M15 frontend + verification |

---

## 7. Acceptance Criteria

### 7.1 M13 — Sales Dashboard V3

#### Backend

- [ ] `GET /api/dashboard/sales` returns HTTP 200 with JWT; HTTP 401 without
- [ ] Response includes `TotalTarget`, `TotalAchievement`, `AchievementPercent`, `TargetVsAchievement`, `TopSalesmanRanking`
- [ ] All M4/M7/M8 fields unchanged in semantics (`TotalOmzet`, `WeeklyTrend`, etc.)
- [ ] `TotalAchievement` === `CompletedOmzet` === `RecognizedOmzet` from builder
- [ ] `TotalTarget` === sum of `BTR_SalesOmzetTarget` for current month (all reps)
- [ ] `AchievementPercent` from `SalesOmzetChartAchievementPolicy` (null when target ≤ 0)
- [ ] Ranking uses `BuildManagerComparison(rows, topCount: 10)` — Completed Omzet, Pipeline excluded
- [ ] No `SalesOmzetTargetResolver` usage
- [ ] Solution builds with zero errors

#### Frontend

- [ ] Route `/dashboard/sales` accessible when authenticated
- [ ] Sidebar: Dashboard → Sales navigates correctly
- [ ] `/dashboard` home shows summary KPI cards only — **no** Sales Trend card
- [ ] Home Sales card links to `/dashboard/sales`
- [ ] Detail page layout: KPI row → Target vs Achievement bar → Weekly Trend line → Top 10 table
- [ ] `loadSales()` fetches sales endpoint only
- [ ] `npm run build` passes

#### Reconciliation

- [ ] `TotalAchievement` === M8 `TotalOmzet` for same month
- [ ] Sum of Top 10 ranking ≤ `TotalAchievement`
- [ ] `TotalTarget` spot-check against SQL sum

#### Out of scope confirmed

- [ ] No new endpoints, drilldown, date filters, export, per-salesman target chart

---

### 7.2 M14 — Piutang Dashboard V2

#### Backend

- [ ] `GET /api/dashboard/piutang` returns HTTP 200 with JWT; HTTP 401 without
- [ ] Response includes `OverdueCustomer`, `AgingBuckets` (5 entries), `TopCustomers`
- [ ] M5 fields `TotalPiutang`, `TotalCustomer` unchanged
- [ ] Aging buckets use inclusive boundaries on `DaysOverdue = Today − JatuhTempo.Date`
- [ ] Sum of bucket amounts === `TotalPiutang`
- [ ] `OverdueCustomer` = customers with overdue-bucket sum > 0 (Current excluded)
- [ ] Customer aggregation uses `CustomerCode` with `CustomerName` fallback
- [ ] Top 10 customers by `Sum(KurangBayar)` descending
- [ ] Solution builds with zero errors

#### Frontend

- [ ] Route `/dashboard/piutang` accessible when authenticated
- [ ] Sidebar: Dashboard → Piutang navigates correctly
- [ ] Home Piutang card links to `/dashboard/piutang`
- [ ] Detail page layout: KPI row → aging pie → Top 10 customer table
- [ ] Pie chart shows 5 aging buckets
- [ ] `loadPiutang()` fetches piutang endpoint only
- [ ] `npm run build` passes

#### Reconciliation

- [ ] `TotalPiutang` === M5 home card === M10 report footer (exact)
- [ ] `TotalCustomer` === M5 === M10 footer (exact)
- [ ] `OverdueCustomer` ≤ `TotalCustomer`

#### Out of scope confirmed

- [ ] No collection KPI, new endpoints, drilldown, date filters, export

---

### 7.3 M15 — Inventory Dashboard V2

#### Backend

- [ ] `GET /api/dashboard/inventory` returns HTTP 200 with JWT; HTTP 401 without
- [ ] Response includes `CategoryBreakdown`, `SupplierBreakdown`, `TopCategories`, `TopSuppliers`
- [ ] M6 fields `TotalInventoryValue`, `TotalItem` unchanged in value
- [ ] BrgId-first grouping matches M6 (exclude In-Transit, Qty ≤ 0 after group)
- [ ] Blank category/supplier displayed as `"Unknown"` — included in totals
- [ ] Top 10 categories and suppliers by inventory value
- [ ] Full category rollup sum === `TotalInventoryValue`
- [ ] Full supplier rollup sum === `TotalInventoryValue`
- [ ] Solution builds with zero errors

#### Frontend

- [ ] Route `/dashboard/inventory` accessible when authenticated
- [ ] Sidebar: Dashboard → Inventory navigates correctly
- [ ] Home Inventory card links to `/dashboard/inventory`
- [ ] Detail page layout: KPI row → category horizontal bar → supplier horizontal bar → Top 10 category table → Top 10 supplier table
- [ ] Charts use `indexAxis: 'y'`
- [ ] `loadInventory()` fetches inventory endpoint only
- [ ] `npm run build` passes

#### Reconciliation

- [ ] `TotalInventoryValue` === M6 home card === M11 report footer (exact)
- [ ] `TotalItem` === M6 === M11 footer (exact)
- [ ] Top 10 sums ≤ respective totals

#### Out of scope confirmed

- [ ] No pie/donut composition, warehouse analysis, Kartu Stok, new endpoints, drilldown, export

---

## 8. Cross-Milestone Regression Checklist

Run after each milestone merge:

| # | Check | Expected |
| --- | --- | --- |
| 1 | `/dashboard` home loads | 3 summary KPI cards with detail links |
| 2 | Prior detail routes | Still accessible (cumulative) |
| 3 | Reports (M9–M12) | Unchanged |
| 4 | Login / JWT | Still functional |
| 5 | Backend build | `j05-btr-distrib.sln` Debug — zero errors |
| 6 | Frontend build | `npm run build` — zero errors |
| 7 | Dashboard KPI values | Match pre-milestone values for unchanged metrics |

---

## 9. Implementation Summary Deliverables

After each milestone, create:

| Milestone | Summary doc |
| --- | --- |
| M13 | `implementation-summary-milestone-13.md` |
| M14 | `implementation-summary-milestone-14.md` |
| M15 | `implementation-summary-milestone-15.md` |

Follow format of `implementation-summary-milestone-8.md`: investigation findings, API contract, files changed, verification results, screenshots, out-of-scope list.

Screenshot targets:

| Milestone | Screenshot |
| --- | --- |
| M13 | `/dashboard/sales` full page + home with Sales link |
| M14 | `/dashboard/piutang` with aging pie + customer table |
| M15 | `/dashboard/inventory` with horizontal bars + tables |

Store in `docs/work/btr-portal-api-scaffolding/screenshots/`.

---

## 10. File Index (All Milestones)

### Modified backend files

| File | M13 | M14 | M15 |
| --- | --- | --- | --- |
| `GetDashboardSalesQuery.cs` | ✓ | | |
| `DashboardSalesDal.cs` | ✓ | | |
| `ISalesOmzetTargetDal.cs` | ✓ | | |
| `SalesOmzetTargetDal.cs` | ✓ | | |
| `GetDashboardPiutangQuery.cs` | | ✓ | |
| `DashboardPiutangDal.cs` | | ✓ | |
| `GetDashboardInventoryQuery.cs` | | | ✓ |
| `DashboardInventoryDal.cs` | | | ✓ |

### New frontend files

| File | M13 | M14 | M15 |
| --- | --- | --- | --- |
| `SalesDashboardView.vue` | ✓ | | |
| `PiutangDashboardView.vue` | | ✓ | |
| `InventoryDashboardView.vue` | | | ✓ |
| `DashboardDetailLayout.vue` | ✓ | | |
| `Top10RankingTable.vue` | ✓ | | |
| `WeeklyTrendChart.vue` | ✓ | | |
| `TargetVsAchievementChart.vue` | ✓ | | |
| `AgingPieChart.vue` | | ✓ | |
| `InventoryHorizontalBarChart.vue` | | | ✓ |

### Modified frontend files (shared)

| File | Changes across M13–M15 |
| --- | --- |
| `dashboard.ts` | Extended types per milestone |
| `dashboardStore.ts` | Add `loadSales`, `loadPiutang`, `loadInventory` |
| `formatters.ts` | `formatPercent` (M13) |
| `DashboardHomeView.vue` | Remove trend card; add detail links |
| `MainLayout.vue` | Nested Dashboard menu (3 sub-items) |
| `router/index.ts` | 3 detail routes |

### Deleted frontend files

| File | Milestone | Reason |
| --- | --- | --- |
| `SalesTrendCard.vue` | M13 | Replaced by detail page + `WeeklyTrendChart` |

---

## 11. Navigation Architecture (Final)

```text
/dashboard                 → Summary KPI cards + links (M7 refactored in M13)
/dashboard/sales           → M13 full analytics
/dashboard/piutang         → M14 full analytics
/dashboard/inventory       → M15 full analytics
/reports/*                 → M9–M12 (unchanged)
```

Sidebar Dashboard group:

```text
Dashboard
├── Overview
├── Sales
├── Piutang
└── Inventory
```
