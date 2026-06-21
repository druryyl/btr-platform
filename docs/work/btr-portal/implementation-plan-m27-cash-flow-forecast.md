# Implementation Plan — M27 Cash Flow Forecast Dashboard

## Document Status

| Field | Value |
| ----- | ----- |
| Milestone | M27 — Cash Flow Forecast Dashboard |
| Authoritative requirements | [portal-analysis-m27-cash-flow-forecast.md](./portal-analysis-m27-cash-flow-forecast.md) |
| Reference pattern | M26 Sales Forecast Dashboard, M20 Collection Dashboard, materialized-dashboard architecture |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |
| Status | **Ready for implementation** (pending Product Owner approval of analysis) |

**Prerequisite:** Product Owner approval of analysis document (forecast algorithm, KPI set, wireframe, risk rules).

---

## 1. Goal

Deliver a read-only **Cash Flow Forecast Dashboard** at `/dashboard/cash-flow-forecast` that projects month-end **cash collection** and **total collections** using **Current Pace (Calendar-Day Linear Extrapolation)** — deterministic, explainable, and traceable to the same pelunasan and Faktur data as Collection and Sales dashboards.

**In scope:**

- New forecast aggregation during existing **Collection** snapshot refresh
- New snapshot tables for forecast KPIs, daily cash pace, recovery trend, and collection risk rows
- New API endpoint `GET /api/dashboard/cash-flow-forecast`
- New Portal Web dashboard page with KPI rows, charts, executive summary, risk table
- Unit tests for forecast policy and aggregator

**Out of scope:**

- Changes to existing `/dashboard/collection` behavior or API response
- Per-customer / per-salesman / per-wilayah forecast pages
- Working-day / holiday calendar
- Collection target master data
- Alert Center / Executive integration
- Pelunasan Report
- AI / ML models
- Purchasing cash-outflow forecast

---

## 2. Impact Analysis

### 2.1 Business areas affected

| Area | Impact |
| ---- | ------ |
| Finance (Piutang / Collection) | Primary — forecast derives from pelunasan and open balances |
| Sales | Read-only — Month Faktur Omzet benchmark |
| Master Data | Read-only — customer plafond for risk rules |
| Inventory / Purchasing | None |

### 2.2 Systems affected

| System | Impact |
| ------ | ------ |
| BTR Portal API | New controller + MediatR query |
| BTR Portal Web | New route, view, components, store method |
| BTR Portal Worker | Extended Collection snapshot refresh |
| BTR SQL | New snapshot tables |
| BTR Desktop | None |

### 2.3 Existing features — preserve behavior

| Feature | Change |
| ------- | ------ |
| Collection Dashboard (`/dashboard/collection`) | **None** |
| Collection snapshot refresh cadence | **Unchanged** (~30 min) |
| `GET /api/dashboard/collection` | **Unchanged** |
| Piutang / Sales dashboards | **Unchanged** |
| `DashboardCollectionAggregator` exposure/attention logic | **Reused**, not modified for M20 outputs |

### 2.4 Source code modules affected

| Layer | Module | Change type |
| ----- | ------ | ----------- |
| Application | `ReportingContext/DashboardSnapshotAgg/Services/` | **Add** `DashboardCashFlowForecastAggregator`, `CashFlowForecastPolicy`, `CollectionDayGrouper` (or reuse pattern from `SalesOmzetChartDayGrouper`) |
| Application | `ReportingContext/DashboardSnapshotAgg/Models/` | **Add** cash flow forecast aggregate DTOs |
| Application | `ReportingContext/DashboardSnapshotAgg/UseCases/` | **Extend** `RefreshDashboardCollectionSnapshotWorker` |
| Application | `ReportingContext/DashboardSnapshotAgg/Contracts/` | **Extend** `IDashboardCollectionSnapshotDal` |
| Application | `ReportingContext/DashboardCashFlowForecastAgg/` | **New** query + read contract |
| Infrastructure | `ReportingContext/DashboardSnapshotAgg/` | **Extend** `DashboardCollectionSnapshotDal` |
| Infrastructure | `ReportingContext/DashboardCashFlowForecastAgg/` | **New** read DAL |
| SQL | `btr.sql/Tables/ReportingContext/` | **Add** forecast tables |
| Portal API | `Controllers/Dashboard/` | **Add** `CashFlowForecastDashboardController` |
| Portal Web | `views/dashboard/`, `components/dashboard/`, `stores/`, `api/` | **Add** forecast page + components |
| Tests | `btr.test/ReportingContext/` | **Add** policy + aggregator tests |

### 2.5 Risk summary

| Risk | Severity | Mitigation |
| ---- | -------- | ---------- |
| Early-month cash forecast volatility | Medium | Forecast Confidence indicator; executive summary wording |
| Forecast diverges from Collection KPIs | High | Single pelunasan load in worker; CFR-50/51/52 traceability tests |
| Risk rule thresholds misaligned with business scale | Medium | Configurable `LargeDueSoonFloorAmount` in appsettings |
| Performance — daily bucket + risk computation | Low | Computed during existing refresh; data already in memory |
| No Pelunasan Report for cash drill-down | Low | Footer links Collection Dashboard + Piutang Report |
| Giro projected as part of recovery but not cash | Low | UI labels distinguish cash vs total collections |

---

## 3. Architecture Overview

### 3.1 Topology

```text
Task Scheduler (Collection domain, ~30 min)
        ↓
RefreshDashboardCollectionSnapshotWorker
        ↓
Load: open balances + pelunasan + faktur + customers + salespeople (unchanged)
        ↓
DashboardCollectionAggregator.Aggregate()     → BTRPD_Collection* (unchanged)
        ↓
DashboardCashFlowForecastAggregator.Aggregate() → BTRPD_CashFlowForecast* (new)
        ↓
DashboardCollectionSnapshotDal.ReplaceCurrent() → single transaction

Browser → GET /api/dashboard/cash-flow-forecast
        ↓ MediatR
GetDashboardCashFlowForecastHandler
        ↓ IDashboardCashFlowForecastDal
DashboardCashFlowForecastDal → snapshot SELECT
```

**Design decision:** Extend the **existing Collection snapshot refresh** rather than create a separate worker domain. Forecast computation reuses the same `pelunasanRows`, `fakturRows`, and `openBalanceRows` already loaded in memory — guaranteed consistency with Collection KPIs and minimal incremental cost.

**Alternative rejected:** Separate `CashFlowForecast` worker domain — would duplicate pelunasan/piutang load and risk temporal inconsistency between Collection and Cash Flow Forecast dashboards.

### 3.2 Layering

```text
btr.portal.api          → CashFlowForecastDashboardController (thin)
btr.application         → GetDashboardCashFlowForecastQuery + Handler
                          → DashboardCashFlowForecastAggregator + CashFlowForecastPolicy
                          → IDashboardCashFlowForecastDal (contract)
btr.infrastructure      → DashboardCashFlowForecastDal (read facade)
                          → DashboardCollectionSnapshotDal (extended write)
btr.portal.web          → CashFlowForecastDashboardView.vue + components
```

MediatR pattern preserved. No business logic in controller.

### 3.3 New policy class

**File:** `btr.application/ReportingContext/DashboardSnapshotAgg/Services/CashFlowForecastPolicy.cs`

Mirror `SalesForecastPolicy` structure:

```csharp
public static class CashFlowForecastPolicy
{
    public static CashFlowForecastCalculation Compute(
        decimal cashCollectedMtd,
        decimal monthCollections,
        decimal monthFakturOmzet,
        DateTime businessDate,
        DateTime monthStart,
        DateTime monthEnd);

    public static decimal ComputeBestCaseCash(decimal mtdDailyCash, decimal recent7DailyCash, int daysInMonth);
    public static decimal ComputeWorstCaseCash(decimal mtdDailyCash, decimal recent7DailyCash, int daysInMonth);
    public static string ResolveConfidence(int daysElapsed, int daysInMonth);
    public static string ResolveRecoveryForecastBand(decimal? collectionForecastPercent);
    public static string ResolveRequiredDailySeverity(decimal requiredDaily, decimal dailyCollectionAverage);
}
```

**Recovery forecast band** (new resolver — parallel to `ExecutiveSalesAchievementBandResolver`):

| Band | Collection Forecast % |
| ---- | --------------------- |
| Healthy | ≥ 100% |
| Warning | 80–99% |
| Critical | < 80% |
| Unknown | null |

### 3.4 Daily grouper

**File:** `btr.application/FinanceContext/PiutangAgg/Services/CollectionDayGrouper.cs` (or shared `ChartDayGrouper` if extracted)

Mirror `SalesOmzetChartDayGrouper`:

- `BuildBuckets(Periode periode)` → one bucket per calendar day
- Labels: `"dd MMM"` in `id-ID` culture

Used to build daily cash pace rows and recent-7-day cash average.

### 3.5 New aggregator

**File:** `btr.application/ReportingContext/DashboardSnapshotAgg/Services/DashboardCashFlowForecastAggregator.cs`

**Inputs:** Same lists as `DashboardCollectionAggregator` plus `DashboardCollectionAggregateResult` KPI values for traceability cross-check.

**Responsibilities:**

1. Build daily cash totals from `pelunasanRows` (`BayarTunai` by `LunasDate`)
2. Build daily total collection totals (`TotalBayar` by `LunasDate`)
3. Build daily billing totals from `fakturRows` (for recovery trend cumulative line)
4. Call `CashFlowForecastPolicy.Compute`
5. Compute scenario bands (best/worst cash)
6. Compute `OutstandingDueRemaining` from open balance rows
7. Build `TopCollectionRisks` via `CashFlowCollectionRiskBuilder` (new helper class in same folder)
8. Build recovery trend rows (cumulative MC vs cumulative BO by day)

---

## 4. Database / Materialized Data Impact

### 4.1 New tables

#### BTRPD_CashFlowForecastKpi

Layer A — forecast KPI snapshot. One row per `SnapshotKey = 'CURRENT'`.

| Column | Type | Meaning |
| ------ | ---- | ------- |
| SnapshotKey | VARCHAR(10) PK | `'CURRENT'` |
| GeneratedAt | DATETIME | Same as Collection KPI refresh |
| PeriodYear | INT | Current month year |
| PeriodMonth | INT | Current month number |
| BusinessDate | DATETIME | As-of date |
| DaysInMonth | INT | Calendar days in month |
| DaysElapsed | INT | Inclusive elapsed days |
| DaysRemaining | INT | Calendar days remaining |
| CashCollectedMtd | DECIMAL(18,2) | = Collection KPI |
| MonthCollections | DECIMAL(18,2) | = Collection KPI |
| MonthFakturOmzet | DECIMAL(18,2) | = Collection KPI |
| DailyCashCollectionAverage | DECIMAL(18,2) | CC / DE |
| DailyCollectionAverage | DECIMAL(18,2) | MC / DE |
| ExpectedCashCollection | DECIMAL(18,2) | Primary cash forecast |
| ProjectedMonthEndTotalCollections | DECIMAL(18,2) | MC pace projection |
| CollectionForecastPercent | DECIMAL(9,4) NULL | Projected recovery % |
| RecoveryVsBillingPercent | DECIMAL(9,4) NULL | Actual — copy from Collection |
| RecoveryVsBillingForecastPercent | DECIMAL(9,4) NULL | Same as CollectionForecastPercent |
| RemainingCollectionTarget | DECIMAL(18,2) | BO − MC (floor 0) |
| RequiredDailyCollection | DECIMAL(18,2) NULL | Required total collections/day |
| OutstandingDueRemaining | DECIMAL(18,2) | Due-soon open balance |
| OverdueOutstanding | DECIMAL(18,2) | = Collection OverdueExposure |
| CollectionGap | DECIMAL(18,2) | BO − projected total |
| ForecastVarianceCash | DECIMAL(18,2) | Expected cash − CC |
| ExpectedCollectionRatePercent | DECIMAL(9,4) NULL | Context KPI |
| BestCaseCash | DECIMAL(18,2) | Scenario band |
| WorstCaseCash | DECIMAL(18,2) | Scenario band |
| ForecastConfidence | VARCHAR(10) | Low / Medium / High |
| ForecastRiskBand | VARCHAR(10) | Healthy / Warning / Critical / Unknown |
| LastRefreshLogId | VARCHAR(26) | FK to refresh log |

#### BTRPD_CashFlowDailyPace

Layer B — daily cash pace for chart.

| Column | Type | Meaning |
| ------ | ---- | ------- |
| CashFlowDailyPaceId | VARCHAR(26) PK | ULID |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| PaceDate | DATETIME | Calendar day |
| DayOfMonth | INT | 1..DIM |
| IsElapsed | BIT | PaceDate ≤ BusinessDate |
| ActualCashAmount | DECIMAL(18,2) | Sum BayarTunai for day |
| ActualCollectionAmount | DECIMAL(18,2) | Sum TotalBayar for day |
| ProjectedDailyCashAmount | DECIMAL(18,2) | MTD daily cash average |

**Index:** `IX_BTRPD_CashFlowDailyPace_SnapshotKey_PaceDate`

#### BTRPD_CashFlowRecoveryTrend

Layer B — cumulative recovery vs billing chart.

| Column | Type | Meaning |
| ------ | ---- | ------- |
| CashFlowRecoveryTrendId | VARCHAR(26) PK | ULID |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| TrendDate | DATETIME | Calendar day |
| DayOfMonth | INT | |
| IsElapsed | BIT | |
| CumulativeCollections | DECIMAL(18,2) | Running sum TotalBayar |
| CumulativeBilling | DECIMAL(18,2) | Running sum Faktur GrandTotal |

#### BTRPD_CashFlowCollectionRisk

Layer B — Top collection risks (max 10 persisted).

| Column | Type | Meaning |
| ------ | ---- | ------- |
| CashFlowCollectionRiskId | VARCHAR(26) PK | ULID |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| SortOrder | INT | Priority rank |
| RiskKey | VARCHAR(30) | Rule key |
| RiskLabel | VARCHAR(60) | Display label |
| EntityType | VARCHAR(20) | Customer / Wilayah |
| EntityId | VARCHAR(13) | |
| EntityName | VARCHAR(100) | |
| Amount | DECIMAL(18,2) | |
| DueOrAgingText | VARCHAR(50) | |
| RuleExplanation | VARCHAR(200) | |
| ReportRoute | VARCHAR(100) | `/reports/piutang` |

Child row ID prefix: **PDCF** (add to `BTR_ParamNo_PortalDashboard.sql`).

### 4.2 Existing tables — no schema change

`BTRPD_CollectionKpi` and all `BTRPD_Collection*` child tables remain unchanged. Forecast KPIs are stored in dedicated tables to avoid breaking Collection API contract.

### 4.3 SQL scripts

| File | Action |
| ---- | ------ |
| `btr.sql/Tables/ReportingContext/BTRPD_CashFlowForecastKpi.sql` | **Create** |
| `btr.sql/Tables/ReportingContext/BTRPD_CashFlowDailyPace.sql` | **Create** |
| `btr.sql/Tables/ReportingContext/BTRPD_CashFlowRecoveryTrend.sql` | **Create** |
| `btr.sql/Tables/ReportingContext/BTRPD_CashFlowCollectionRisk.sql` | **Create** |
| `btr.sql/Scripts/Create_BTRPD_PortalDashboard_Tables.sql` | **Append** IF NOT EXISTS blocks |
| `btr.sql/btr.sql.sqlproj` | **Include** new table files |

### 4.4 Snapshot refresh strategy

**Worker:** `RefreshDashboardCollectionSnapshotWorker` (extended)

```text
1. Load source data (unchanged — 8 load steps)
2. Aggregate collection KPI (unchanged — DashboardCollectionAggregator)
3. Aggregate cash flow forecast (NEW — DashboardCashFlowForecastAggregator)
4. ReplaceCurrent in single transaction:
   a. Replace BTRPD_Collection* (unchanged)
   b. Replace BTRPD_CashFlowForecast* (new)
5. Mark refresh log success (unchanged)
```

**Domain label in refresh log:** remains `"Collection"`.

**CLI / scheduler:** No new `--domain` value. Existing Collection job covers forecast.

### 4.5 Performance considerations

| Concern | Assessment |
| ------- | ---------- |
| Additional pelunasan scan | **None** — reuses loaded list |
| Daily bucket iteration | O(DIM) ≈ 28–31 — negligible |
| Risk builder | O(open rows) — same order as Collection aggregator |
| Snapshot write | +1 KPI row + ~31 daily + ~31 trend + ≤10 risk rows |
| API read | 4 table reads — sub-millisecond |

No new indexes on operational `BTR_PiutangLunas` required.

---

## 5. API Plan

### 5.1 New endpoint

| Method | Route | Auth | Description |
| ------ | ----- | ---- | ----------- |
| GET | `/api/dashboard/cash-flow-forecast` | Bearer token | Cash flow forecast dashboard payload |

No query parameters in V1.

### 5.2 Controller

**File:** `btr.portal.api/Controllers/Dashboard/CashFlowForecastDashboardController.cs`

Thin controller — delegates to MediatR.

### 5.3 MediatR query

**Folder:** `btr.application/ReportingContext/DashboardCashFlowForecastAgg/`

**Files:**

- `Queries/GetDashboardCashFlowForecastQuery.cs` — query, handler, response DTOs
- `Contracts/IDashboardCashFlowForecastDal.cs` — read contract

#### DashboardCashFlowForecastResponse (summary)

| Section | Properties |
| ------- | ---------- |
| Metadata | GeneratedAt, PeriodYear, PeriodMonth, BusinessDate, DaysInMonth, DaysElapsed, DaysRemaining, IsAvailable |
| Kpis | All columns from `BTRPD_CashFlowForecastKpi` |
| DailyPace | List of daily pace rows |
| RecoveryTrend | List of cumulative trend rows |
| CollectionRisks | List of risk rows |
| ExecutiveSummary | Server-computed plain-language string |

**Empty snapshot:** `IsAvailable = false` with graceful degradation (same pattern as Customer Analytics) — HTTP 200 with empty KPIs, not 503.

### 5.4 Error handling

| Condition | Response |
| --------- | -------- |
| Snapshot row missing | `IsAvailable = false` |
| Partial child data | Log warning; return available sections |

---

## 6. UI Implementation Plan

### 6.1 Route and navigation

| Item | Value |
| ---- | ----- |
| Route | `/dashboard/cash-flow-forecast` |
| Page title | Cash Flow Forecast Dashboard |
| Sidebar label | Cash Flow Forecast |
| Placement | After **Collection**, before Customer Analytics |
| Auth | All authenticated users (no role routing) |

### 6.2 Files (Portal Web)

| File | Purpose |
| ---- | ------- |
| `src/views/dashboard/CashFlowForecastDashboardView.vue` | Page shell |
| `src/components/dashboard/CashFlowForecastSummary.vue` | Executive summary |
| `src/components/dashboard/CashFlowKpiGrid.vue` | Four KPI rows |
| `src/components/dashboard/CashFlowDailyPaceChart.vue` | Daily pace chart |
| `src/components/dashboard/CashFlowForecastVsBillingChart.vue` | Grouped bar |
| `src/components/dashboard/CashFlowRecoveryTrendChart.vue` | Cumulative line |
| `src/components/dashboard/CashFlowCollectionRisksTable.vue` | Risk table |
| `src/api/dashboardApi.ts` | `getCashFlowForecast()` |
| `src/stores/dashboardStore.ts` | `fetchCashFlowForecast` action |
| `src/models/dashboard.ts` | TypeScript interfaces |
| `src/router/index.ts` | Route registration |
| `src/layouts/MainLayout.vue` | Sidebar entry |

### 6.3 Component reuse

| Existing component | Reuse |
| ---------------- | ----- |
| `DashboardDetailLayout` | Page shell |
| `KpiCard` / achievement band classes | Risk band coloring |
| `WeeklyTrendChart` pattern | Chart option structure for daily pace |
| `CollectionRecoverySummary` | Band styling reference for recovery % |

### 6.4 Presentation Mode

Reuse `IBusinessDateProvider` via API snapshot `BusinessDate` — forecast reflects pinned date when Presentation Mode active.

---

## 7. Collection Risk Builder

**File:** `btr.application/ReportingContext/DashboardSnapshotAgg/Services/CashFlowCollectionRiskBuilder.cs`

Static builder invoked from `DashboardCashFlowForecastAggregator`.

**Inputs:**

- Open balance rows (with customer, JatuhTempo, KurangBayar)
- `DashboardCollectionAggregateResult` (for chronic, legacy, plafond, hotspot, low recovery sets)
- `pelunasanRows` (for deteriorating payment rule — 30-day lookback via additional DAL load or in-memory filter)
- Configuration: `LargeDueSoonFloorAmount` (default 50_000_000)

**Output:** Ordered list, `Take(10)`.

**30-day collection lookback:** Extend worker to load pelunasan for `(B-30, B]` in addition to current month — **additional DAL call** acceptable for risk rule only; does not affect pace KPIs.

---

## 8. Testing Plan

### 8.1 Unit tests

**File:** `btr.test/ReportingContext/CashFlowForecastPolicyTest.cs`

| Test case | Assertion |
| --------- | --------- |
| Mid-month pace | Expected cash = (CC/DE)×DIM |
| Day 1 DE=1 | No division by zero |
| BO=0 | CollectionForecastPercent null |
| MC ≥ BO | Remaining target = 0 |
| Last day of month | Forecast = actual |
| Best/worst case | MAX/MIN recent 7 vs MTD |
| Confidence bands | DE thresholds |

**File:** `btr.test/ReportingContext/DashboardCashFlowForecastAggregatorTest.cs`

| Test case | Assertion |
| --------- | --------- |
| Daily buckets sum to CC | CFR-25 |
| Outstanding due remaining | JatuhTempo filter |
| Traceability | CashCollectedMtd matches input |
| Risk builder | Top 10 cap, priority order |

### 8.2 Shadow / integration verification

| Check | Rule |
| ----- | ---- |
| CashCollectedMtd | = Collection aggregator output same inputs |
| MonthFakturOmzet | = Sales Faktur sum same month |
| RecoveryVsBillingPercent | = Collection aggregator |

---

## 9. Configuration

**appsettings.json** (Portal Worker + API):

```json
{
  "DashboardSnapshot": {
    "CollectionIntervalMinutes": 30,
    "CashFlowForecastLargeDueSoonFloorAmount": 50000000
  }
}
```

---

## 10. Step-by-Step Implementation Plan

| Step | Task | Verification |
| ---- | ---- | ------------ |
| 1 | Create SQL tables + param prefix PDCF | Deploy script |
| 2 | Add `CashFlowForecastPolicy` + unit tests | Tests pass |
| 3 | Add `CollectionDayGrouper` | Mirror sales day grouper tests |
| 4 | Add `CashFlowCollectionRiskBuilder` + tests | Risk priority order |
| 5 | Add `DashboardCashFlowForecastAggregator` + tests | Traceability tests |
| 6 | Extend `DashboardCollectionAggregateResult` / snapshot DAL models | Compile |
| 7 | Extend `RefreshDashboardCollectionSnapshotWorker` | Worker runs Collection domain |
| 8 | Extend `DashboardCollectionSnapshotDal.ReplaceCurrent` | Transaction replaces all tables |
| 9 | Add `IDashboardCashFlowForecastDal` + infrastructure read DAL | Integration read |
| 10 | Add `GetDashboardCashFlowForecastQuery` + handler | MediatR |
| 11 | Add `CashFlowForecastDashboardController` | API returns JSON |
| 12 | Register DI (API + Worker) | App starts |
| 13 | Portal Web: models, API, store, route, sidebar | Navigation works |
| 14 | Portal Web: view + components | Page renders |
| 15 | Manual verification: reconcile with Collection + Sales dashboards | CFR-50/51/52 |
| 16 | Knowledge Curator: `docs/features/cash-flow-forecast/feature.md` + update `btr-portal-domain.md` | Docs synced |

**Recommended order:** Steps 1–8 (backend data path) → 9–11 (API) → 12–14 (UI) → 15–16 (verification + docs).

---

## 11. Future Extensibility (Technical)

| Extension | Technical hook |
| --------- | -------------- |
| Salesman-level forecast | Add `SalesPersonId` filter to aggregator; new dimensional table |
| Wilayah-level forecast | Join `PiutangOpenBalanceWithWilayahDal` in pace aggregation |
| Weekly forecast | New policy method with `DR=7` cap |
| Customer payment profile | New `BTRPD_CustomerPaymentProfile` populated from historical `BTR_PiutangLunas` batch job |
| Alert Center | Read `CollectionForecastPercent` from KPI; register `LowCollectionForecast` in `ALERT-REGISTRY.md` |
| Executive promotion | Overview DAL reads forecast KPI — optional column on overview response |
| Working-day calendar | Inject `IWorkingDayCalendar` into policy; replace DIM/DR |
| Net cash flow | Combine with `BTRPD_PurchasingKpi.GrandTotalPurchase` in new aggregator |

---

## 12. Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
| ---- | ---------- | ------ | ---------- |
| Management confuses cash vs total collections | Medium | Medium | Clear labels; dual KPI rows |
| Forecast perceived as guarantee | Medium | High | Confidence indicator + scenario bands + footer disclaimer |
| Risk thresholds too noisy | Medium | Low | Top 10 cap; tunable floor in config |
| Worker transaction grows | Low | Low | Same pattern as M26; ~70 extra rows per refresh |
| 30-day pelunasan load for risk rule slows worker | Low | Low | Single additional DAL call; indexed on LunasDate |

---

## Document Maintenance

On implementation completion:

1. Create `docs/features/cash-flow-forecast/feature.md` (permanent knowledge)
2. Update `docs/features/btr-portal/btr-portal-domain.md` — add Cash Flow Forecast to Section 4 and dashboard table Section 8
3. Update `docs/features/materialized-dashboard/materialized-dashboard-architecture.md` — new tables and worker note
4. Update `docs/features/btr-portal/btr-portal-operational.md` — navigation entry
5. Archive or remove temporary work emphasis after Knowledge Curator pass

**Success criterion:** `GET /api/dashboard/cash-flow-forecast` serves deterministic, explainable month-end cash projection that reconciles with Collection and Sales snapshots on every refresh.
