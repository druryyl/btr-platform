# Implementation Plan — M26 Sales Forecast Dashboard

## Document Status

| Field | Value |
| ----- | ----- |
| Milestone | M26 — Sales Forecast Dashboard |
| Authoritative requirements | [portal-analysis-m26-sales-forecast.md](./portal-analysis-m26-sales-forecast.md) |
| Reference pattern | M13 Sales Dashboard V3, M20 Collection Dashboard, materialized-dashboard architecture |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |

**Prerequisite:** Product Owner approval of analysis document (forecast algorithm, KPI set, wireframe).

---

## 1. Goal

Deliver a read-only **Sales Forecast Dashboard** at `/dashboard/sales-forecast` that projects month-end invoiced sales using **Current Pace (Calendar-Day Linear Extrapolation)** — deterministic, explainable, and traceable to the same Faktur data as Sales Dashboard.

**In scope:**

- New forecast aggregation during existing Sales snapshot refresh
- New snapshot tables for forecast KPIs and daily pace buckets
- New API endpoint `GET /api/dashboard/sales-forecast`
- New Portal Web dashboard page with KPI rows, charts, executive summary, risk indicator
- Unit tests for forecast policy and aggregator

**Out of scope:**

- Changes to existing `/dashboard/sales` behavior or API response
- Per-salesman forecast
- Working-day / holiday calendar
- Alert Center / Executive integration
- Custom period selection
- Pipeline / outstanding omzet inclusion
- AI / ML models

---

## 2. Impact Analysis

### 2.1 Business areas affected

| Area | Impact |
| ---- | ------ |
| Sales | Primary — forecast derives from Faktur omzet and sales targets |
| Master Data | None — read-only target data |
| Finance | None — no piutang/collection changes |
| Inventory | None |

### 2.2 Systems affected

| System | Impact |
| ------ | ------ |
| BTR Portal API | New controller + MediatR query |
| BTR Portal Web | New route, view, components, store method |
| BTR Portal Worker | Extended Sales snapshot refresh (same worker, additional aggregation step) |
| BTR Desktop | None |
| BTR SQL | New snapshot tables + optional KPI column extensions |

### 2.3 Existing features — preserve behavior

| Feature | Change |
| ------- | ------ |
| Sales Dashboard (`/dashboard/sales`) | **None** |
| Sales snapshot refresh cadence | **Unchanged** (~30 min) |
| `GET /api/dashboard/sales` | **Unchanged** |
| Achievement policy | **Reused**, not modified |
| Target DAL | **Reused**, not modified |
| FakturView DAL | **Reused**, not modified |

### 2.4 Source code modules affected

| Layer | Module | Change type |
| ----- | ------ | ----------- |
| Application | `ReportingContext/DashboardSnapshotAgg/Services/` | **Add** `DashboardSalesForecastAggregator`, `SalesForecastPolicy` |
| Application | `ReportingContext/DashboardSnapshotAgg/Models/` | **Add** forecast aggregate DTOs |
| Application | `ReportingContext/DashboardSnapshotAgg/UseCases/` | **Extend** `RefreshDashboardSalesSnapshotWorker` |
| Application | `ReportingContext/DashboardSnapshotAgg/Contracts/` | **Extend** snapshot DAL interface |
| Application | `ReportingContext/DashboardSalesForecastAgg/` | **New** query + read contract |
| Application | `SalesContext/SalesOmzetAgg/Services/` | **Add** `SalesOmzetChartDayGrouper` (daily buckets) |
| Infrastructure | `ReportingContext/DashboardSnapshotAgg/` | **Extend** `DashboardSalesSnapshotDal`; **Add** forecast snapshot DAL methods |
| Infrastructure | `ReportingContext/DashboardSalesForecastAgg/` | **New** read DAL |
| SQL | `btr.sql/Tables/ReportingContext/` | **Add** `BTRPD_SalesForecastKpi.sql`, `BTRPD_SalesDailyPace.sql` |
| Portal API | `Controllers/Dashboard/` | **Add** `SalesForecastDashboardController` |
| Portal Web | `views/dashboard/`, `components/dashboard/`, `stores/`, `api/` | **Add** forecast page + components |
| Tests | `btr.test/ReportingContext/` | **Add** forecast policy + aggregator tests |

### 2.5 Risk summary

| Risk | Severity | Mitigation |
| ---- | -------- | ---------- |
| Early-month forecast volatility | Medium | Forecast Confidence indicator; executive summary wording |
| Forecast diverges from Sales Dashboard actuals | High | Single Faktur load in worker; FR-40/41 traceability rules; verification test |
| Performance — daily bucket computation | Low | Computed during existing refresh; same Faktur list reused |
| Division by zero on month start | Low | `DE = max(1, ...)` rule in policy |
| Target null edge cases | Low | Reuse achievement policy null handling; match Sales Dashboard |
| Presentation Mode date pinning | Low | Reuse `IBusinessDateProvider` — already tested in Portal |

---

## 3. Architecture Overview

### 3.1 Topology

```text
Task Scheduler (Sales domain, ~30 min)
        ↓
RefreshDashboardSalesSnapshotWorker
        ↓
IFakturViewDal.ListData(currentMonth)  +  ISalesOmzetTargetDal.SumTargetAmountForMonth
        ↓
DashboardSalesFakturAggregator.Aggregate()     → BTRPD_SalesKpi, WeekTrend, TopSalesman (unchanged)
        ↓
DashboardSalesForecastAggregator.Aggregate()     → BTRPD_SalesForecastKpi, BTRPD_SalesDailyPace (new)
        ↓
DashboardSalesSnapshotDal.ReplaceCurrent()       → single transaction

Browser → GET /api/dashboard/sales-forecast
        ↓ MediatR
GetDashboardSalesForecastHandler
        ↓ IDashboardSalesForecastDal
DashboardSalesForecastDal → DashboardSalesForecastSnapshotDal → BTRPD_SalesForecast*
```

**Design decision:** Extend the **existing Sales snapshot refresh** rather than create a separate worker domain. Forecast computation reuses the same Faktur row list already loaded in memory — minimal incremental cost, guaranteed consistency with Sales KPIs.

**Alternative rejected:** Separate `SalesForecast` worker domain — would duplicate Faktur load and risk temporal inconsistency between Sales and Forecast dashboards.

### 3.2 Layering (consistent with BTR Portal)

```text
btr.portal.api          → SalesForecastDashboardController (thin)
btr.application         → GetDashboardSalesForecastQuery + Handler
                          → DashboardSalesForecastAggregator + SalesForecastPolicy
                          → IDashboardSalesForecastDal (contract)
btr.infrastructure      → DashboardSalesForecastDal (read facade)
                          → DashboardSalesForecastSnapshotDal (SQL)
btr.portal.web          → SalesForecastDashboardView.vue + components
```

MediatR pattern preserved. Controller delegates to handler. Handler delegates to DAL. No business logic in controller.

### 3.3 New policy class

**File:** `btr.application/ReportingContext/DashboardSnapshotAgg/Services/SalesForecastPolicy.cs`

Static, deterministic formulas extracted from analysis §4.3:

```csharp
public static class SalesForecastPolicy
{
    public static SalesForecastCalculation Compute(
        decimal currentSales,
        decimal? target,
        DateTime businessDate,
        DateTime monthStart,
        DateTime monthEnd);

    public static string ResolveConfidence(int daysElapsed, int daysInMonth);
    public static string ResolveRequiredDailySeverity(decimal requiredDaily, decimal dailyAverage);
}
```

Achievement % delegated to `SalesOmzetChartAchievementPolicy`. Risk band delegated to `ExecutiveSalesAchievementBandResolver`.

### 3.4 New daily grouper

**File:** `btr.application/SalesContext/SalesOmzetAgg/Services/SalesOmzetChartDayGrouper.cs`

Mirror `SalesOmzetChartWeekGrouper` pattern:

- `BuildBuckets(Periode periode)` → one bucket per calendar day
- `FindBucket(buckets, date)` → placement helper
- Labels: `"dd MMM"` in `id-ID` culture

Used by `DashboardSalesForecastAggregator` to build daily pace rows and recent-7-day average.

---

## 4. Database / Materialized Data Impact

### 4.1 New tables

#### BTRPD_SalesForecastKpi

Layer A — forecast KPI snapshot. One row per `SnapshotKey = 'CURRENT'`.

| Column | Type | Meaning |
| ------ | ---- | ------- |
| SnapshotKey | VARCHAR(10) PK | `'CURRENT'` |
| GeneratedAt | DATETIME | Same as Sales KPI refresh |
| PeriodYear | INT | Current month year |
| PeriodMonth | INT | Current month number |
| BusinessDate | DATETIME | As-of date used for calculation |
| DaysInMonth | INT | Calendar days in month |
| DaysElapsed | INT | Inclusive elapsed days |
| DaysRemaining | INT | Calendar days remaining |
| CurrentSales | DECIMAL(18,2) | MTD omzet (= Sales KPI TotalOmzet) |
| TotalTarget | DECIMAL(18,2) | Monthly target (= Sales KPI TotalTarget) |
| CurrentAchievementPercent | DECIMAL(9,4) NULL | Actual achievement % |
| DailyAverageSales | DECIMAL(18,2) | CS / DE |
| ForecastSales | DECIMAL(18,2) | Expected projection |
| ForecastAchievementPercent | DECIMAL(9,4) NULL | Forecast achievement % |
| RequiredDailySales | DECIMAL(18,2) NULL | Required to hit target |
| TargetGap | DECIMAL(18,2) | T - ForecastSales |
| ForecastVariance | DECIMAL(18,2) | ForecastSales - CS |
| BestCaseSales | DECIMAL(18,2) | Best scenario |
| WorstCaseSales | DECIMAL(18,2) | Worst scenario |
| ForecastConfidence | VARCHAR(10) | Low / Medium / High |
| ForecastRiskBand | VARCHAR(10) | Healthy / Warning / Critical / Unknown |
| LastRefreshLogId | VARCHAR(26) | FK to refresh log |

#### BTRPD_SalesDailyPace

Layer B — daily pace buckets for chart.

| Column | Type | Meaning |
| ------ | ---- | ------- |
| SalesDailyPaceId | VARCHAR(26) PK | ULID |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| PaceDate | DATETIME | Calendar day |
| DayOfMonth | INT | 1..DIM |
| IsElapsed | BIT | true when PaceDate ≤ BusinessDate |
| ActualAmount | DECIMAL(18,2) | Sum Faktur omzet for day (0 if future) |
| ProjectedDailyAmount | DECIMAL(18,2) | MTD daily average (for chart reference line) |

**Index:** `IX_BTRPD_SalesDailyPace_SnapshotKey_PaceDate`

### 4.2 Existing tables — no schema change

`BTRPD_SalesKpi`, `BTRPD_SalesWeekTrend`, `BTRPD_SalesTopSalesman` remain unchanged.

Forecast reads weekly trend from existing `BTRPD_SalesWeekTrend` via the forecast DAL (join in read layer) to avoid duplication.

### 4.3 SQL scripts

| File | Action |
| ---- | ------ |
| `btr.sql/Tables/ReportingContext/BTRPD_SalesForecastKpi.sql` | **Create** |
| `btr.sql/Tables/ReportingContext/BTRPD_SalesDailyPace.sql` | **Create** |
| `btr.sql/Scripts/Create_BTRPD_PortalDashboard_Tables.sql` | **Append** IF NOT EXISTS blocks |
| `btr.sql/btr.sql.sqlproj` | **Include** new table files |

### 4.4 Snapshot refresh strategy

**Worker:** `RefreshDashboardSalesSnapshotWorker` (extended)

```text
1. Load Faktur rows + target (unchanged)
2. Aggregate sales KPI (unchanged — DashboardSalesFakturAggregator)
3. Aggregate forecast (NEW — DashboardSalesForecastAggregator)
4. ReplaceCurrent in single transaction:
   a. Replace BTRPD_SalesKpi + child tables (unchanged)
   b. Replace BTRPD_SalesForecastKpi + BTRPD_SalesDailyPace (new)
5. Mark refresh log success (unchanged)
```

**Domain label in refresh log:** remains `"Sales"` — forecast is part of Sales domain refresh.

**CLI / scheduler:** No new `--domain` value. Existing Sales job covers forecast.

### 4.5 Performance considerations

| Concern | Assessment |
| ------- | ---------- |
| Additional Faktur scan | **None** — reuses loaded list |
| Daily bucket iteration | O(days in month) ≈ 28–31 iterations — negligible |
| Snapshot write | +1 KPI row + ~31 daily rows per refresh — negligible |
| API read | 2 table reads (forecast KPI + daily pace) + week trend reuse — sub-millisecond |
| Memory | Daily buckets add ~31 small objects to existing refresh — negligible |

No new indexes on operational Faktur tables required.

---

## 5. API Plan

### 5.1 New endpoint

| Method | Route | Auth | Description |
| ------ | ----- | ---- | ----------- |
| GET | `/api/dashboard/sales-forecast` | Bearer token | Sales forecast dashboard payload |

No query parameters in V1 (current month only).

### 5.2 Controller

**File:** `btr.portal.api/Controllers/Dashboard/SalesForecastDashboardController.cs`

```csharp
[ApiController]
[Route("api/dashboard/sales-forecast")]
public class SalesForecastDashboardController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardSalesForecastResponse>> Get(CancellationToken ct)
        => Ok(await _mediator.Send(new GetDashboardSalesForecastQuery(), ct));
}
```

Thin controller — no business logic.

### 5.3 MediatR query

**Folder:** `btr.application/ReportingContext/DashboardSalesForecastAgg/`

**Files:**

- `Queries/GetDashboardSalesForecastQuery.cs` — query, handler, response DTOs
- `Contracts/IDashboardSalesForecastDal.cs` — read contract

#### DashboardSalesForecastResponse

| Property | Type | Source |
| -------- | ---- | ------ |
| GeneratedAt | DateTime | Snapshot |
| PeriodYear | int | Snapshot |
| PeriodMonth | int | Snapshot |
| BusinessDate | DateTime | Snapshot |
| DaysInMonth | int | Snapshot |
| DaysElapsed | int | Snapshot |
| DaysRemaining | int | Snapshot |
| CurrentSales | decimal | Snapshot |
| TotalTarget | decimal | Snapshot |
| CurrentAchievementPercent | decimal? | Snapshot |
| DailyAverageSales | decimal | Snapshot |
| ForecastSales | decimal | Snapshot |
| ForecastAchievementPercent | decimal? | Snapshot |
| RequiredDailySales | decimal? | Snapshot |
| TargetGap | decimal | Snapshot |
| ForecastVariance | decimal | Snapshot |
| BestCaseSales | decimal | Snapshot |
| WorstCaseSales | decimal | Snapshot |
| ForecastConfidence | string | Snapshot |
| ForecastRiskBand | string | Snapshot |
| ExecutiveSummary | string | Computed in DAL or dedicated summary builder |
| ForecastVsTarget | DashboardSalesForecastVsTarget | Chart DTO |
| DailyPace | List\<DashboardSalesDailyPaceItem\> | Snapshot |
| WeeklyTrend | List\<DashboardSalesWeekTrendItem\> | Reuse existing week trend items from Sales snapshot |

#### DashboardSalesForecastVsTarget

| Property | Type |
| -------- | ---- |
| TargetAmount | decimal |
| CurrentAmount | decimal |
| ForecastAmount | decimal |

#### DashboardSalesDailyPaceItem

| Property | Type |
| -------- | ---- |
| PaceDate | DateTime |
| DayOfMonth | int |
| IsElapsed | bool |
| ActualAmount | decimal |
| ProjectedDailyAmount | decimal |

### 5.4 Read DAL

**File:** `btr.infrastructure/ReportingContext/DashboardSalesForecastAgg/DashboardSalesForecastDal.cs`

- Reads `BTRPD_SalesForecastKpi` + `BTRPD_SalesDailyPace`
- Reads `BTRPD_SalesWeekTrend` for weekly chart (same snapshot)
- Builds `ExecutiveSummary` from template strings in analysis §8.5
- Returns empty-safe defaults when snapshot not yet refreshed

### 5.5 Admin refresh

No new admin endpoint. Existing `POST /api/admin/dashboard/refresh` with Sales domain triggers forecast computation.

---

## 6. Application Structure (new files)

```text
btr.application/
├── ReportingContext/
│   ├── DashboardSnapshotAgg/
│   │   ├── Services/
│   │   │   ├── DashboardSalesForecastAggregator.cs      (NEW)
│   │   │   └── SalesForecastPolicy.cs                   (NEW)
│   │   ├── Models/
│   │   │   └── DashboardSalesForecastAggregateResult.cs (NEW)
│   │   └── Contracts/
│   │       └── IDashboardSalesSnapshotDal.cs            (EXTEND ReplaceCurrent signature)
│   └── DashboardSalesForecastAgg/
│       ├── Queries/
│       │   └── GetDashboardSalesForecastQuery.cs        (NEW)
│       └── Contracts/
│           └── IDashboardSalesForecastDal.cs            (NEW)
└── SalesContext/SalesOmzetAgg/Services/
    └── SalesOmzetChartDayGrouper.cs                     (NEW)

btr.infrastructure/
├── ReportingContext/
│   ├── DashboardSnapshotAgg/
│   │   └── DashboardSalesSnapshotDal.cs                 (EXTEND — write forecast tables)
│   └── DashboardSalesForecastAgg/
│       └── DashboardSalesForecastDal.cs                 (NEW)

btr.portal.api/
└── Controllers/Dashboard/
    └── SalesForecastDashboardController.cs              (NEW)

btr.sql/Tables/ReportingContext/
├── BTRPD_SalesForecastKpi.sql                           (NEW)
└── BTRPD_SalesDailyPace.sql                             (NEW)

btr.test/ReportingContext/
├── SalesForecastPolicyTest.cs                           (NEW)
├── DashboardSalesForecastAggregatorTest.cs              (NEW)
└── DashboardSalesForecastSnapshotVerificationTest.cs    (NEW)
```

### 6.1 Dependency injection

Register in Portal API and Worker DI containers:

- `DashboardSalesForecastAggregator` (transient or singleton — match aggregator pattern)
- `IDashboardSalesForecastDal` → `DashboardSalesForecastDal`
- `GetDashboardSalesForecastHandler`

### 6.2 Aggregator algorithm

**DashboardSalesForecastAggregator.Aggregate():**

```text
Input:  fakturRows, periode, totalTarget, businessDate, generatedAt
        (same inputs as DashboardSalesFakturAggregator + explicit businessDate)

1. currentSales = SUM(fakturRows where Tgl <= businessDate)
2. dayBuckets = SalesOmzetChartDayGrouper.BuildBuckets(periode)
3. Populate ActualAmount per elapsed day from fakturRows
4. forecast = SalesForecastPolicy.Compute(currentSales, totalTarget, businessDate, monthStart, monthEnd)
5. recent7Avg = SUM(last 7 elapsed days) / 7  (fallback to MTD avg if DE < 7)
6. bestCase = MAX(MTD avg, recent7Avg) × DIM
7. worstCase = MIN(MTD avg, recent7Avg) × DIM
8. Build daily pace rows with ProjectedDailyAmount = MTD avg for all days
9. Return DashboardSalesForecastAggregateResult
```

**Verification invariant (test):** `currentSales` must equal `DashboardSalesFakturAggregator` totalOmzet when businessDate = last day of data range.

---

## 7. UI Implementation Plan

### 7.1 Route and navigation

**Router:** Add route `/dashboard/sales-forecast` → `SalesForecastDashboardView.vue`

**Sidebar** (`AppSidebar` or equivalent navigation config):

```text
Dashboard
├── Executive
├── Alert Center
├── Sales
├── Sales Forecast          ← NEW
├── Piutang
...
```

Update `docs/features/btr-portal/btr-portal-operational.md` after implementation (Implementer responsibility — not in this plan's code scope).

### 7.2 New files

| File | Purpose |
| ---- | ------- |
| `src/views/dashboard/SalesForecastDashboardView.vue` | Main page |
| `src/components/dashboard/SalesForecastSummary.vue` | Executive summary sentence |
| `src/components/dashboard/SalesForecastKpiRow.vue` | Reusable KPI metric row |
| `src/components/dashboard/ForecastVsTargetChart.vue` | 3-bar chart |
| `src/components/dashboard/DailyPaceTrendChart.vue` | Daily actual + projected line |
| `src/components/dashboard/ForecastRiskCard.vue` | Risk band indicator |
| `src/api/dashboardApi.ts` | Add `fetchDashboardSalesForecast()` |
| `src/models/dashboard.ts` | Add forecast TypeScript interfaces |
| `src/stores/dashboardStore.ts` | Add `salesForecast` state + `loadSalesForecast()` |

### 7.3 Page structure

Follow `SalesDashboardView.vue` patterns:

- `DashboardDetailLayout` wrapper
- `onMounted → dashboard.loadSalesForecast()`
- Three KPI rows using `SalesForecastKpiRow`
- Charts section
- `ForecastRiskCard`
- Traceability footer with link to `/reports/sales`

### 7.4 Chart implementation

| Component | Base | Notes |
| --------- | ---- | ----- |
| `ForecastVsTargetChart` | Copy pattern from `TargetVsAchievementChart.vue` | 3 bars instead of 2 |
| `DailyPaceTrendChart` | Copy pattern from `WeeklyTrendChart.vue` | Bar chart with elapsed/remaining color split |
| Weekly trend | Reuse `WeeklyTrendChart.vue` directly | Pass `WeeklyTrend` from forecast API response |

Use existing chart library (same as other dashboard charts — likely Chart.js via vue-chartjs or project equivalent).

### 7.5 Formatting

Reuse existing formatters:

- `formatCurrency()` for amounts
- `formatPercent()` for achievement percentages
- Risk band colors: match Executive Dashboard band styling

### 7.6 Executive summary

Render `ExecutiveSummary` string from API — computed server-side for consistency and i18n-readiness. Do not reconstruct client-side.

---

## 8. Testing Plan

### 8.1 Unit tests — SalesForecastPolicy

| Test case | Expected |
| --------- | -------- |
| Mid-month with sales and target | Correct forecast, required daily, gap |
| Day 1 with zero sales | Forecast = 0, confidence Low |
| Day 1 with sales > 0 | Forecast extrapolated, confidence Low |
| Target already achieved | Required daily = 0 |
| Target null / zero | Achievement null, required daily null, band Unknown |
| Last day of month | Forecast = current sales, days remaining = 0 |
| DE max(1) guard | No divide by zero on month start |

### 8.2 Unit tests — DashboardSalesForecastAggregator

| Test case | Expected |
| --------- | -------- |
| Daily buckets sum to currentSales | FR-26 traceability |
| Recent-7-day avg with DE < 7 | Falls back to MTD avg |
| Best/Worst case ordering | Best ≥ Expected ≥ Worst |
| Faktur after businessDate excluded | FR-06 |

### 8.3 Snapshot verification test

| Test case | Expected |
| --------- | -------- |
| After refresh: ForecastKpi.CurrentSales = SalesKpi.TotalOmzet | FR-40 |
| After refresh: ForecastKpi.TotalTarget = SalesKpi.TotalTarget | FR-41 |
| Daily pace row count = days in month | Schema integrity |

Follow pattern from `DashboardSalesSnapshotVerificationTest.cs`.

### 8.4 API integration test (optional)

`GET /api/dashboard/sales-forecast` returns 200 with token after Sales worker run.

---

## 9. Step-by-Step Implementation Plan

Execute in order. Each step should compile and pass existing tests before proceeding.

### Phase 1 — Foundation (backend core)

| Step | Task | Verification |
| ---- | ---- | ------------ |
| 1.1 | Create `SalesOmzetChartDayGrouper` + unit tests | Tests pass |
| 1.2 | Create `SalesForecastPolicy` + unit tests | Tests pass |
| 1.3 | Create `DashboardSalesForecastAggregateResult` model | Compile |
| 1.4 | Create `DashboardSalesForecastAggregator` + unit tests | Tests pass |

### Phase 2 — Persistence

| Step | Task | Verification |
| ---- | ---- | ------------ |
| 2.1 | Create SQL table scripts `BTRPD_SalesForecastKpi`, `BTRPD_SalesDailyPace` | Scripts valid |
| 2.2 | Update `btr.sql.sqlproj` and `Create_BTRPD_PortalDashboard_Tables.sql` | Build succeeds |
| 2.3 | Extend `IDashboardSalesSnapshotDal` + `DashboardSalesSnapshotDal.ReplaceCurrent()` | Compile |
| 2.4 | Extend `RefreshDashboardSalesSnapshotWorker` to call forecast aggregator and persist | Worker runs locally |
| 2.5 | Add snapshot verification test | Test pass |

### Phase 3 — API

| Step | Task | Verification |
| ---- | ---- | ------------ |
| 3.1 | Create `IDashboardSalesForecastDal` + `DashboardSalesForecastDal` | Compile |
| 3.2 | Create `GetDashboardSalesForecastQuery` + handler + response DTOs | Compile |
| 3.3 | Create `SalesForecastDashboardController` | Compile |
| 3.4 | Register DI bindings in Portal API + Worker | API starts |
| 3.5 | Manual test: run Sales worker → `GET /api/dashboard/sales-forecast` | JSON payload correct |

### Phase 4 — Frontend

| Step | Task | Verification |
| ---- | ---- | ------------ |
| 4.1 | Add TypeScript models + `fetchDashboardSalesForecast()` | Compile |
| 4.2 | Add `loadSalesForecast()` to dashboard store | Compile |
| 4.3 | Create chart components | Visual review |
| 4.4 | Create `SalesForecastDashboardView.vue` | Page renders |
| 4.5 | Add route + sidebar navigation | Navigation works |
| 4.6 | End-to-end smoke test | All KPIs populated after worker run |

### Phase 5 — Documentation (Implementer)

| Step | Task |
| ---- | ---- |
| 5.1 | Update `docs/features/btr-portal/btr-portal-domain.md` — add Forecast concepts |
| 5.2 | Update `docs/features/btr-portal/btr-portal-operational.md` — route, API, sidebar |
| 5.3 | Update `docs/features/materialized-dashboard/materialized-dashboard-architecture.md` — new tables |
| 5.4 | Create `docs/features/sales-forecast/feature.md` — permanent knowledge (curated from analysis) |

---

## 10. Future Extensibility

Designed for incremental enhancement without breaking V1:

| Enhancement | Extension point |
| ----------- | --------------- |
| Working-day calendar | Add `IWorkingDayCalendar` service; swap `DIM`/`DE`/`DR` in `SalesForecastPolicy` |
| Per-salesman forecast | New aggregator scoped by SalesPersonId; reuse same policy |
| Executive Dashboard link | Read `BTRPD_SalesForecastKpi.ForecastRiskBand` in `DashboardExecutiveComposer` |
| Alert Center signal | Register `ForecastBelowTarget` in `AlertCenterRegistry` |
| Historical forecast accuracy | Store forecast snapshots monthly (extend MERGE pattern from `BTRPD_SalesmanRepHistory`) |
| Custom period | Add query params to API + worker period resolver |
| Pipeline inclusion | Would require Portal to adopt `SalesOmzetChartSummaryBuilder` — major scope change |

---

## 11. Architecture Decisions Record

| # | Decision | Rationale | Alternatives rejected |
| - | -------- | --------- | --------------------- |
| AD-1 | Primary algorithm = Current Pace (calendar-day linear) | Simplest, most explainable, no new master data | Working-day (no calendar), weighted ML-like models |
| AD-2 | Extend Sales snapshot worker, not new domain | Same Faktur load, guaranteed consistency, minimal ops change | Separate SalesForecast worker |
| AD-3 | New tables, not columns on BTRPD_SalesKpi | Separation of concerns; Sales Dashboard schema untouched | Alter SalesKpi |
| AD-4 | Separate API endpoint and page | Preserves Sales Dashboard; clear UX for forecasting layer | Extend `/api/dashboard/sales` |
| AD-5 | Server-side executive summary | Consistent wording; single source for summary logic | Client-side template |
| AD-6 | Scenario bands from MTD vs recent-7-day | Explainable momentum bracket without ML | Standard deviation bands |
| AD-7 | Weekly trend read from existing Sales snapshot | Avoid duplicate storage | Copy week trend to forecast tables |

---

## 12. Rollback Strategy

| Level | Action |
| ----- | ------ |
| UI only | Remove route + sidebar entry; no backend impact |
| API only | Remove controller; snapshot tables remain harmless |
| Full rollback | Drop `BTRPD_SalesForecastKpi`, `BTRPD_SalesDailyPace`; revert worker extension |

Existing Sales Dashboard unaffected at every rollback level.

---

## 13. Definition of Done

1. All implementation plan steps complete.
2. Unit tests pass for policy, aggregator, and snapshot verification.
3. Existing Sales Dashboard tests still pass (no regression).
4. `/dashboard/sales-forecast` renders all wireframe sections.
5. Forecast Current Sales = Sales Dashboard Total Achievement for same refresh.
6. Executive summary renders for all risk bands.
7. Knowledge artifacts updated (Phase 5).
8. Product Owner acceptance against analysis acceptance criteria (§9).
