# BTR Portal Analysis — M26 Sales Forecast Dashboard

**Status:** Analysis complete — ready for Architect review and Product Owner approval before implementation.  
**Scope:** Business analysis, forecasting model design, KPI definitions, wireframe, and business rules only. No production code.  
**Date:** 2026-06-21  
**Author role:** Analyst  
**Companion document:** [implementation-plan-m26-sales-forecast.md](./implementation-plan-m26-sales-forecast.md)

**Reference documents:** `docs/foundation/PRODUCT.md`, `docs/foundation/DOMAIN.md`, `docs/foundation/LANDSCAPE.md`, `docs/foundation/WORKFLOW.md`, `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-operational.md`, `docs/features/btr-portal/btr-portal-architecture.md`, `docs/features/materialized-dashboard/materialized-dashboard-architecture.md`, `docs/features/sales-person-principal-target/feature.md`, `docs/work/btr-portal/M16 Executive Dashboard - Analysis.md`, `docs/work/btr-portal/M18 Salesman Performance - Analysis.md`

---

## 1. Executive Summary

BTR Portal today answers three management questions well:

| Layer | Question | Example dashboards |
| ----- | -------- | ------------------ |
| Reporting | What happened? | Sales Report, Piutang Report |
| Analytics | How are we performing? | Sales Dashboard, Salesman Performance |
| Decision Support | What requires attention? | Executive Dashboard, Alert Center |

**M26 introduces the fourth layer — Forecasting:**

> **"What is likely to happen by the end of this month?"**

Management needs to anticipate month-end sales performance early enough to take corrective action — before the month closes. This must remain **read-only**, **deterministic**, **explainable**, and **traceable** to existing Faktur data. No AI, machine learning, or external prediction models.

### Key findings

| Finding | Implication for M26 |
| ------- | ------------------- |
| Sales Dashboard (M13) already computes **current-month actuals**, **target**, **achievement %**, and **weekly trend** from non-void Fakturs | Forecast KPIs must **reuse** the same data source and achievement policy — not duplicate or diverge |
| M16 analysis explicitly flags **"Achievement % MTD pace / projected month-end achievement"** as **not computed** | M26 directly closes this documented gap |
| No forecasting, projection, or run-rate logic exists anywhere in Portal sales code | All forecast math is **net-new business logic** — but built on existing aggregators |
| No working-day or holiday calendar exists in BTR Portal | V1 uses **calendar days** only; working-day projection is deferred |
| No daily sales buckets exist in Portal (weekly only via `SalesOmzetChartWeekGrouper`) | Daily pace chart requires **new daily aggregation** during snapshot refresh |
| Pipeline/outstanding omzet is **excluded** from Portal sales (`PipelineOmzet = 0`) | M26 V1 forecasts **invoiced Faktur omzet only** — same scope as Sales Dashboard |
| `BTRPD_SalesmanRepHistory` retains monthly achievement history per rep | Useful for **future** forecast accuracy tracking; not required for company-level V1 |
| Collection Dashboard (M20) already computes **pace ratios** (Recovery vs Billing) | Pattern reference for pace KPI presentation — not formula reuse |

### Product intent

At any point during the current month, management should be able to answer:

> **"If our current sales pace continues, where will we finish at month-end, and how much daily sales are required to still achieve the target?"**

---

## 2. Business Objective

### Problem

Management reviews the Sales Dashboard and sees current achievement (e.g., 62% on day 18). They must mentally extrapolate whether the team will hit target. This mental calculation is inconsistent, error-prone, and unavailable to non-finance users.

### Outcome

A dedicated **Sales Forecast Dashboard** that:

1. Projects expected month-end invoiced sales based on current pace.
2. Shows forecast achievement % against the same monthly target used by Sales Dashboard.
3. Calculates required daily sales to close any remaining target gap.
4. Presents pace trend so management can see whether billing is accelerating or slowing.
5. Provides a plain-language executive summary of forecast risk.

### Users

| User | Need |
| ---- | ---- |
| Operations leadership | Anticipate month-end miss before it happens |
| Sales management | Know required daily run-rate for remaining days |
| Finance administration | Cross-check forecast against billing evidence |

### Explicitly out of scope (V1)

- AI / ML / statistical models
- Per-salesman forecast dashboard (company-level only; rep-level deferred)
- Custom period selection (current month only)
- Pipeline / outstanding / Sales Order inclusion
- Working-day or holiday-adjusted pace
- Forecast write-back or target adjustment
- Historical forecast accuracy scoring
- Automated alert registration (Executive/Alert Center integration deferred)

---

## 3. Phase 1 — Existing Capability Analysis

### 3.1 Sales Dashboard calculations (reuse as-is)

**Source:** `DashboardSalesFakturAggregator` → `BTRPD_SalesKpi`, `BTRPD_SalesWeekTrend`, `BTRPD_SalesTopSalesman`

| Metric | Formula / rule | Source |
| ------ | -------------- | ------ |
| **Current Sales (Total Omzet)** | `SUM(FakturView.GrandTotal)` for current calendar month | `DashboardSalesFakturAggregator.Aggregate()` |
| **Current Achievement** | Same as Total Omzet (Faktur-only; pipeline = 0) | Same |
| **Total Target** | `SalesOmzetTargetDal.SumTargetAmountForMonth(year, month)` — sum of per-rep resolved targets (principal targets preferred over legacy) | `RefreshDashboardSalesSnapshotWorker` |
| **Achievement %** | `SalesOmzetChartAchievementPolicy.ComputePercent(omzet, target)` = `(recognized / target) × 100`, rounded 1 decimal, not capped | `SalesOmzetChartAchievementPolicy` |
| **Weekly Trend** | 7-day buckets from month start via `SalesOmzetChartWeekGrouper`; sum GrandTotal per bucket | `BuildWeekTrend()` |
| **Top 10 Salesman** | Group by SalesPerson, order by CompletedOmzet DESC | `BuildTopSalesman()` |

**Data filter (must match for traceability):**

- Source: `IFakturViewDal.ListData(periode)` — current calendar month
- Non-void Fakturs only (`VoidDate = '3000-01-01'`)
- Grouping date: `FakturView.Tgl` (Faktur date)
- Period: `IBusinessDateProvider.Today` → current month (`Tgl1` = 1st, `Tgl2` = last day)

### 3.2 Achievement calculation policy

**Class:** `SalesOmzetChartAchievementPolicy`

```
Achievement % = (recognizedOmzet / targetAmount) × 100
Returns null when target ≤ 0 or missing
Rounded to 1 decimal place
Not capped — over-achievement shown as > 100%
```

**Achievement bands** (reuse for forecast risk coloring):

| Band | Threshold | Resolver |
| ---- | --------- | -------- |
| Healthy | ≥ 100% | `ExecutiveSalesAchievementBandResolver` |
| Warning | 80–99% | Same |
| Critical | < 80% | Same |
| Unknown | null achievement | Same |

### 3.3 Target calculation

**Class:** `SalesOmzetTargetDal`

Per rep/month resolution:

1. If sum of principal targets (`BTR_SalesPersonPrincipalTarget`) > 0 → use that sum
2. Else fallback to legacy `BTR_SalesOmzetTarget` row

Company target = sum of all rep resolved targets for the month.

See `docs/features/sales-person-principal-target/feature.md`.

### 3.4 Weekly trend calculation

**Class:** `SalesOmzetChartWeekGrouper`

- Splits month into **7-day segments from Tgl1** (not ISO calendar weeks)
- Last bucket may be shorter at month end
- Labels: `"dd MMM–dd MMM"` in `id-ID` culture
- Portal uses Faktur date directly (not Desktop omzet period mode)

**Gap:** No prior-week comparison, acceleration flag, or projected line on weekly chart.

### 3.5 Daily / monthly aggregation

| Granularity | Exists? | Location |
| ----------- | ------- | -------- |
| Monthly (company) | Yes | `BTRPD_SalesKpi.TotalOmzet` |
| Monthly (per rep) | Yes | `BTRPD_SalesmanKpi`, `BTRPD_SalesmanRepHistory` |
| Weekly buckets | Yes | `BTRPD_SalesWeekTrend` |
| **Daily buckets** | **No** | Must be added for pace chart |

### 3.6 Calendar utilities

| Utility | Purpose | Forecast relevance |
| ------- | ------- | ------------------ |
| `IBusinessDateProvider.Today` | Business "as-of" date (supports Presentation Mode) | Defines days elapsed / remaining |
| `ITglJamDal.Now` | Server timestamp for `GeneratedAt` | Snapshot metadata |
| `ReportPeriodValidator` | Report date range validation (max 31 days) | Sales Report drill-down |
| `SalesOmzetChartWeekGrouper` | Week buckets within month | Weekly pace context |
| `IIsoWeekCalendar` | RO2 materialization health only | Not applicable |
| `IRuteCycleCalendar` | Visit route cycles | Not applicable |

**Not found:** Working-day counter, holiday table, business-day calendar.

### 3.7 Reporting and dashboard builders

| Component | Role | M26 reuse |
| --------- | ---- | --------- |
| `DashboardSalesFakturAggregator` | Company sales snapshot | **Extend** — same Faktur load, add forecast + daily buckets |
| `DashboardSalesDal` | Read facade for `/api/dashboard/sales` | Pattern reference for new read facade |
| `RefreshDashboardSalesSnapshotWorker` | Snapshot refresh | **Extend** — compute forecast during same refresh |
| `DashboardSalesSnapshotDal` | Snapshot persistence | **Extend** — new columns / child table |
| `SalesOmzetChartSummaryBuilder` | Desktop omzet (pipeline, status slices) | **Not used** in Portal — do not introduce pipeline in M26 |
| `DashboardCollectionAggregator` | Recovery vs Billing pace ratio | Presentation pattern only |

### 3.8 Existing forecasting logic

**None found.** Grep across Portal codebase confirms no `SalesForecast`, `projected`, `run rate`, `month-end`, or `required daily` calculations in sales reporting.

M16 §6.5 documents the gap explicitly:

> Achievement % MTD pace — **Not computed** — Projected month-end achievement — **requires projection logic**

### 3.9 Duplication guard

M26 must **not** reimplement:

- Achievement % formula → call `SalesOmzetChartAchievementPolicy`
- Target resolution → call `ISalesOmzetTargetDal.SumTargetAmountForMonth`
- Faktur filtering → same `IFakturViewDal.ListData(periode)` path
- Week bucketing → call `SalesOmzetChartWeekGrouper` for weekly context chart

M26 **must** add:

- Daily bucketing (new grouper or inline in aggregator)
- Linear pace projection formulas (new policy class)
- Scenario band derivation (new policy class)
- Forecast-specific KPI snapshot fields

---

## 4. Phase 2 — Forecasting Model Analysis

### 4.1 Candidate algorithms

All candidates use **invoiced Faktur omzet only** and the same monthly target as Sales Dashboard.

| # | Algorithm | Formula (conceptual) | Simplicity | Explainability | Stability | Business usefulness |
| - | --------- | -------------------- | ---------- | -------------- | --------- | ------------------- |
| A | **Current Pace (Calendar-Day Linear)** | `Forecast = (MTD Sales ÷ Days Elapsed) × Days in Month` | ★★★★★ | ★★★★★ | ★★★☆☆ | ★★★★★ |
| B | Daily Average (equivalent to A) | Same math, different label | ★★★★★ | ★★★★★ | ★★★☆☆ | ★★★★★ |
| C | Remaining Working Day projection | `Forecast = MTD + (Daily Avg × Remaining Working Days)` | ★★★☆☆ | ★★★★☆ | ★★☆☆☆ | ★★★★☆ |
| D | Previous Month comparison | Benchmark: prior month total at same day-of-month | ★★★★☆ | ★★★★★ | ★★★★☆ | ★★★☆☆ |
| E | Previous Year same month | Benchmark: last year month total at same day | ★★★☆☆ | ★★★★☆ | ★★★☆☆ | ★★★☆☆ |
| F | Weighted recent days (7-day weighted avg) | `Forecast = (Weighted recent daily avg) × Days in Month` | ★★★☆☆ | ★★★☆☆ | ★★☆☆☆ | ★★★★☆ |
| G | Weekday-aware projection | Adjust pace by day-of-week historical pattern | ★★☆☆☆ | ★★☆☆☆ | ★★☆☆☆ | ★★★☆☆ |

### 4.2 Algorithm evaluation notes

**A — Current Pace (recommended primary):**

- Directly answers: *"If we continue at today's average daily rate, where do we land?"*
- Managers already think in this pattern ("we're averaging X per day, month has Y days")
- Requires only calendar-day count — no new master data
- Weakness: early-month forecasts swing wildly on first few days; mitigated by confidence indicator

**C — Working Day projection:**

- More realistic for field-sales businesses where weekends/holidays have near-zero billing
- **Blocked for V1:** BTR has no holiday calendar or working-day definition in Portal
- Deferred to M26.1 or later after business defines working-day rules

**D/E — Historical benchmarks:**

- Useful as **context** ("last month at this point we were at X") but not a forward projection
- Company-level historical snapshots are not retained (`SnapshotKey = 'CURRENT'` only)
- Would require new historical data load — out of V1 scope

**F — Weighted recent days:**

- Captures acceleration/deceleration better than month-average
- Less stable early in month; harder to explain to non-technical managers
- Recommended as **secondary input for scenario bands** (Best/Worst), not primary forecast

**G — Weekday-aware:**

- Requires multi-month history and day-of-week pattern analysis
- Over-engineered for V1; contradicts simplicity principle

### 4.3 Recommended primary algorithm (V1)

**Current Pace Projection — Calendar-Day Linear Extrapolation**

Management explanation (plain language):

> *"We divide this month's invoiced sales so far by the number of days that have passed, then multiply by the total days in the month. This assumes billing continues at the same daily average."*

#### Core formulas

Let:

- `B` = business date (`IBusinessDateProvider.Today`)
- `MS` = month start (1st of month)
- `ME` = month end (last day of month)
- `DIM` = days in month = `(ME - MS).Days + 1`
- `DE` = days elapsed = `(B - MS).Days + 1` (inclusive of today)
- `DR` = days remaining (calendar) = `(ME - B).Days`
- `CS` = current sales = `SUM(Faktur.GrandTotal)` MTD through `B`
- `T` = monthly target

| KPI | Formula |
| --- | ------- |
| Daily Average Sales | `CS / DE` |
| **Forecast Sales (Expected)** | `(CS / DE) × DIM` |
| Forecast Achievement % | `SalesOmzetChartAchievementPolicy.ComputePercent(ForecastSales, T)` |
| Required Daily Sales | `(T - CS) / DR` when `T > CS` and `DR > 0`; else `0` |
| Target Gap | `T - ForecastSales` |
| Forecast Variance (remaining projected) | `ForecastSales - CS` |
| Sales Pace | Same as Daily Average Sales |

**Rounding:** Currency values to 2 decimal places (match existing dashboard). Percentages via achievement policy (1 decimal).

#### Scenario bands (secondary — explainable range)

| Scenario | Formula | Plain language |
| -------- | ------- | -------------- |
| **Expected** | Current Pace (primary algorithm) | Month-average daily rate continues |
| **Best Case** | `MAX(MTD daily avg, Recent-7-day daily avg) × DIM` | Recent week is stronger than month average |
| **Worst Case** | `MIN(MTD daily avg, Recent-7-day daily avg) × DIM` | Recent week is weaker than month average |

**Recent-7-day daily avg:** Sum Faktur omzet for calendar days `(B-6)..B` inclusive, divided by 7.

When `DE < 7`: Recent-7-day avg falls back to MTD daily avg (insufficient recent window).

These bands are **not separate forecasts** — they bracket the expected projection using observable recent momentum.

---

## 5. Phase 3 — Dashboard Wireframe

Text layout only. Route: `/dashboard/sales-forecast`.

```text
┌─────────────────────────────────────────────────────────────────────────────┐
│  Sales Forecast Dashboard                                    [↻ Refresh]    │
│  Current month forecast — invoiced sales (Faktur)                           │
│  As of: {GeneratedAt}  ·  Period: {Month Year}  ·  Day {DE} of {DIM}       │
├─────────────────────────────────────────────────────────────────────────────┤
│  EXECUTIVE SUMMARY (plain-language sentence)                                │
│  "At current pace, invoiced sales are projected to reach {ForecastSales}   │
│   ({ForecastAchievement}%) against a target of {Target}. {Risk sentence}."│
├─────────────────────────────────────────────────────────────────────────────┤
│  KPI ROW 1 — Actual vs Forecast                                             │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐       │
│  │ Current Sales│ │ Current      │ │ Forecast     │ │ Forecast     │       │
│  │ {CS}         │ │ Achievement  │ │ Sales        │ │ Achievement  │       │
│  │              │ │ {Ach%}       │ │ {FS}         │ │ {Forecast%}  │       │
│  └──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘       │
├─────────────────────────────────────────────────────────────────────────────┤
│  KPI ROW 2 — Pace & Gap                                                     │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐       │
│  │ Daily Average│ │ Required     │ │ Target Gap   │ │ Days         │       │
│  │ Sales        │ │ Daily Sales  │ │ {T-FS}       │ │ Remaining    │       │
│  │ {Pace}       │ │ {Required}   │ │              │ │ {DR} calendar│       │
│  └──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘       │
├─────────────────────────────────────────────────────────────────────────────┤
│  KPI ROW 3 — Scenario & Confidence                                          │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐       │
│  │ Best Case    │ │ Expected     │ │ Worst Case   │ │ Forecast     │       │
│  │ {BestFS}     │ │ {ExpectedFS} │ │ {WorstFS}    │ │ Confidence   │       │
│  │              │ │              │ │              │ │ {Low/Med/Hi} │       │
│  └──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘       │
├─────────────────────────────────────────────────────────────────────────────┤
│  CHART 1 — Daily Pace Trend (full width)                                    │
│  Bar/line: Actual daily omzet (solid) + Projected daily pace line (dashed) │
│  X-axis: calendar days 1..DIM  ·  Y-axis: omzet amount                     │
│  Highlight: elapsed days vs remaining days                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│  CHART 2 — Forecast vs Target (half width)  │  CHART 3 — Weekly Pace     │
│  Grouped bar: Target | Current | Forecast   │  (reuse WeeklyTrendChart)   │
│  Three-bar comparison                         │  Actual weekly omzet only  │
├─────────────────────────────────────────────────────────────────────────────┤
│  RISK INDICATOR CARD                                                        │
│  Band: {Healthy/Warning/Critical/Unknown} based on Forecast Achievement %  │
│  Required action hint when Critical/Warning                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│  FOOTER — Traceability                                                      │
│  "Forecast based on {N} Fakturs through {B}. Same rules as Sales Dashboard.│
│   View evidence → Sales Report"                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

**Navigation placement:** Sidebar under **Dashboard → Sales Forecast** (after Sales, before Piutang). Executive Dashboard may link to forecast in a future milestone — not V1.

---

## 6. Phase 4 — KPI Definitions

### 6.1 Primary KPIs

#### Current Sales

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Total invoiced omzet recognized so far this month |
| **Formula** | `SUM(FakturView.GrandTotal)` for current month through business date |
| **Management interpretation** | "How much have we billed so far?" |
| **Color/status** | Neutral (informational) |
| **Drill-down** | Sales Report (`/reports/sales`) — current month, default period |

#### Current Achievement

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Actual invoiced omzet as a percentage of monthly target |
| **Formula** | `SalesOmzetChartAchievementPolicy.ComputePercent(CS, T)` |
| **Management interpretation** | "Where are we today against plan?" |
| **Color/status** | `ExecutiveSalesAchievementBandResolver` bands |
| **Drill-down** | Sales Dashboard (`/dashboard/sales`) |

#### Current Achievement % (alias)

Same as Current Achievement — displayed as percentage in KPI row.

#### Forecast Sales (Expected)

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Projected month-end invoiced sales if current daily pace continues |
| **Formula** | `(CS / DE) × DIM` |
| **Management interpretation** | "Where will we likely finish if nothing changes?" |
| **Color/status** | Neutral; compare visually against Target bar |
| **Drill-down** | Sales Report (evidence behind MTD actuals) |

#### Forecast Achievement %

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Projected target attainment at month-end |
| **Formula** | `ComputePercent(ForecastSales, T)` |
| **Management interpretation** | "Will we hit target at this pace?" |
| **Color/status** | `ExecutiveSalesAchievementBandResolver` on **forecast** percent |
| **Drill-down** | Sales Dashboard (target context) |

#### Required Daily Sales

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Average daily invoiced sales needed on remaining days to exactly hit target |
| **Formula** | `(T - CS) / DR` when `T > CS` and `DR > 0`; else `0` |
| **Management interpretation** | "How much must we bill each remaining day to still make target?" |
| **Color/status** | Warning when Required Daily > 1.5× Daily Average; Critical when > 2× |
| **Drill-down** | Sales Report |

#### Daily Average Sales

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Average invoiced omzet per elapsed calendar day this month |
| **Formula** | `CS / DE` |
| **Management interpretation** | "What is our billing pace so far?" |
| **Color/status** | Neutral; compare against Required Daily |
| **Drill-down** | Daily Pace Trend chart (in-page) |

#### Remaining Calendar Days

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Calendar days left in the month after business date |
| **Formula** | `(ME - B).Days` |
| **Management interpretation** | "How much time remains?" |
| **Color/status** | Neutral; informational |
| **Drill-down** | None |

#### Remaining Working Days

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Business days excluding weekends/holidays |
| **Formula** | **Not computed in V1** — display "—" with tooltip "Working-day calendar not configured" |
| **Management interpretation** | Deferred |
| **Color/status** | N/A |
| **Drill-down** | N/A |

#### Sales Pace

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Same as Daily Average Sales — the run-rate driving the forecast |
| **Formula** | `CS / DE` |
| **Management interpretation** | "Our current billing speed" |
| **Color/status** | Neutral |
| **Drill-down** | Daily Pace Trend chart |

#### Forecast Variance

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Additional omzet projected between today and month-end |
| **Formula** | `ForecastSales - CS` |
| **Management interpretation** | "How much more billing is projected?" |
| **Color/status** | Neutral |
| **Drill-down** | None |

#### Target Gap

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Shortfall (or surplus) between target and forecast |
| **Formula** | `T - ForecastSales` (negative = projected over-achievement) |
| **Management interpretation** | "How far above or below target are we projected to finish?" |
| **Color/status** | Critical if gap > 20% of target; Warning if gap > 0; Healthy if gap ≤ 0 |
| **Drill-down** | Sales Dashboard |

#### Best Case Projection

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Optimistic month-end if recent week momentum continues |
| **Formula** | `MAX(MTD daily avg, Recent-7-day daily avg) × DIM` |
| **Management interpretation** | "Best realistic outcome if recent billing strength holds" |
| **Color/status** | Informational (green tint if above target) |
| **Drill-down** | Weekly Trend chart |

#### Expected Projection

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Primary forecast — same as Forecast Sales |
| **Formula** | Current Pace formula |
| **Management interpretation** | "Most likely outcome at current pace" |
| **Color/status** | Primary highlight |
| **Drill-down** | Sales Report |

#### Worst Case Projection

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Conservative month-end if recent week weakness continues |
| **Formula** | `MIN(MTD daily avg, Recent-7-day daily avg) × DIM` |
| **Management interpretation** | "Downside if recent slowdown continues" |
| **Color/status** | Informational (red tint if below target) |
| **Drill-down** | Weekly Trend chart |

### 6.2 Derived indicators

#### Forecast Confidence

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Qualitative reliability of the forecast based on elapsed time |
| **Formula** | `DE ≤ 5 → Low`; `6 ≤ DE ≤ 20 → Medium`; `DE ≥ 21 → High` |
| **Management interpretation** | Early-month projections are inherently uncertain |
| **Color/status** | Low = gray; Medium = amber; High = green |
| **Drill-down** | None |

#### Risk Indicator

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Overall forecast risk band |
| **Formula** | `ExecutiveSalesAchievementBandResolver.Resolve(ForecastAchievementPercent)` |
| **Management interpretation** | "Is projected performance acceptable?" |
| **Color/status** | Healthy / Warning / Critical / Unknown |
| **Drill-down** | Sales Dashboard + Sales Report |

---

## 7. Phase 5 — Business Rules

All rules are **deterministic**. Same inputs always produce same outputs.

### 7.1 Scope rules

| Rule ID | Rule |
| ------- | ---- |
| FR-01 | Forecast **current calendar month only** — same period as Sales Dashboard |
| FR-02 | Business date from `IBusinessDateProvider.Today` defines "as-of" point |
| FR-03 | **Faktur-only** — same as Sales Dashboard; pipeline/outstanding excluded |
| FR-04 | **Non-void Fakturs only** — `VoidDate = '3000-01-01'` |
| FR-05 | Grouping date = `FakturView.Tgl` (Faktur date) |
| FR-06 | **Ignore future-dated Fakturs** beyond business date within the month |
| FR-07 | Retur does **not** reduce achievement (consistent with target feature v1) |

### 7.2 Target rules

| Rule ID | Rule |
| ------- | ---- |
| FR-10 | Target = `SumTargetAmountForMonth` — same source as Sales Dashboard |
| FR-11 | When target is missing or zero: Achievement % and Forecast Achievement % = null; Required Daily Sales = null; Risk = Unknown |
| FR-12 | Mid-month target revision: last saved target wins on next snapshot refresh |

### 7.3 Calculation rules

| Rule ID | Rule |
| ------- | ---- |
| FR-20 | `DE = max(1, (B - MS).Days + 1)` — prevents division by zero on month start |
| FR-21 | `DR = max(0, (ME - B).Days)` |
| FR-22 | When `CS = 0` on day 1: Forecast Sales = 0; Daily Average = 0 |
| FR-23 | When target already achieved (`CS ≥ T`): Required Daily Sales = 0 |
| FR-24 | When month is complete (`B = ME`): Forecast Sales = CS (actual = forecast); DR = 0 |
| FR-25 | Recalculate on every Sales snapshot refresh (~30 min) |
| FR-26 | Forecast totals must equal traceable sum: daily buckets for elapsed days sum to CS |

### 7.4 Calendar rules

| Rule ID | Rule |
| ------- | ---- |
| FR-30 | **Calendar days** used for pace — includes weekends and holidays |
| FR-31 | Weekends: **no special treatment** in V1 (billing may occur any day) |
| FR-32 | Holidays: **no holiday table** — treated as normal calendar days |
| FR-33 | Working-day metrics: display "—" in V1; do not estimate |

### 7.5 Beginning-of-month behavior

| Day | Behavior |
| --- | -------- |
| Day 1, CS = 0 | All forecasts = 0; Confidence = Low; Executive summary: "Insufficient billing data — forecast will stabilize as the month progresses." |
| Day 1, CS > 0 | Forecast extrapolates from single-day pace; Confidence = Low |
| Days 2–5 | Forecast computed normally; Confidence = Low |

### 7.6 End-of-month behavior

| Condition | Behavior |
| --------- | -------- |
| Last day of month | Forecast Sales = Current Sales; Target Gap = T - CS (actual gap) |
| After month end | Dashboard shows **previous month** until business date rolls to new month (same as Sales Dashboard) |

### 7.7 Presentation Mode

When Presentation Mode pins a business date, all forecast calculations use the pinned date as `B`. Same rule as other Portal dashboards.

### 7.8 Traceability rules

| Rule ID | Rule |
| ------- | ---- |
| FR-40 | Forecast Current Sales **must equal** `BTRPD_SalesKpi.TotalOmzet` for same refresh |
| FR-41 | Forecast Target **must equal** `BTRPD_SalesKpi.TotalTarget` for same refresh |
| FR-42 | API response includes `GeneratedAt` from snapshot — not request time |
| FR-43 | Footer links to Sales Report for Faktur-level evidence |

---

## 8. Phase 6 — User Experience Recommendations

### 8.1 Dashboard layout

- Use existing `DashboardDetailLayout` shell (title, subtitle, GeneratedAt, refresh button)
- **Future-oriented subtitle:** "Current month forecast — where invoiced sales are likely to finish."
- Three KPI rows before charts — most important numbers above the fold
- Executive summary sentence at top — plain language, no jargon

### 8.2 Card ordering rationale

| Order | Rationale |
| ----- | --------- |
| 1. Executive Summary | Answers the question immediately |
| 2. Current vs Forecast KPIs | Actual baseline then projection |
| 3. Pace & Gap KPIs | Actionable numbers (required daily) |
| 4. Scenario & Confidence | Range and reliability context |
| 5. Daily Pace Trend | Visual proof of pace |
| 6. Forecast vs Target + Weekly Trend | Comparison and momentum |
| 7. Risk Indicator | Confirms urgency |
| 8. Traceability footer | Trust and evidence link |

### 8.3 Charts

| Chart | Type | Data |
| ----- | ---- | ---- |
| Daily Pace Trend | Combined bar (actual daily) + dashed horizontal line (MTD daily average) + optional projected fill for remaining days | New daily bucket snapshot |
| Forecast vs Target | Grouped bar (3 bars: Target, Current, Forecast) | Extend `TargetVsAchievementChart` pattern |
| Weekly Pace | Reuse `WeeklyTrendChart` | Existing `BTRPD_SalesWeekTrend` |

### 8.4 Trend visualization

- Daily chart: elapsed days in solid color; remaining days in muted tone with projected daily amount annotation
- Weekly chart: unchanged from Sales Dashboard — provides acceleration/deceleration visual context without automated flags

### 8.5 Executive summary wording templates

**On track (Forecast ≥ 100%):**

> "At current pace, invoiced sales are projected to reach **{ForecastSales}** (**{ForecastAchievement}%**) against a target of **{Target}**. The team is on track to meet or exceed the monthly target."

**At risk (Forecast 80–99%):**

> "At current pace, invoiced sales are projected to reach **{ForecastSales}** (**{ForecastAchievement}%**) against a target of **{Target}**. **{RequiredDaily}** daily billing is required on remaining days to close the gap."

**Critical (Forecast < 80%):**

> "At current pace, invoiced sales are projected to reach **{ForecastSales}** (**{ForecastAchievement}%**) against a target of **{Target}**. Immediate corrective action is needed — **{RequiredDaily}** daily billing required to recover."

**Unknown target:**

> "At current pace, invoiced sales are projected to reach **{ForecastSales}**. No monthly target is configured — achievement comparison unavailable."

**Early month (Confidence = Low):**

> "Forecast is preliminary (day **{DE}** of **{DIM}**). Projection will become more reliable as billing accumulates."

### 8.6 Drill-down behavior

| Interaction | Destination |
| ----------- | ----------- |
| Current Sales KPI click | `/reports/sales` (current month) |
| Forecast KPI click | `/reports/sales` (current month) |
| Target Gap click | `/dashboard/sales` |
| Risk Indicator click | `/dashboard/sales` |
| Footer "View evidence" | `/reports/sales` |
| Daily chart day click | `/reports/sales?from={day}&to={day}` (if M24 investigation supports date filter) |

Follow M24 investigation patterns where available.

### 8.7 Differentiation from Sales Dashboard

| Sales Dashboard | Sales Forecast Dashboard |
| --------------- | ------------------------ |
| "What happened?" | "What will probably happen?" |
| Current achievement focus | Projected achievement focus |
| Weekly trend (historical) | Daily pace + projection line |
| No required daily sales | Required daily sales prominent |
| No scenario bands | Best / Expected / Worst range |

**Existing Sales Dashboard behavior is unchanged.**

---

## 9. Acceptance Criteria

1. Forecast dashboard accessible at `/dashboard/sales-forecast` for authenticated users.
2. All KPI values are deterministic and reproducible from documented formulas.
3. Current Sales and Target match Sales Dashboard snapshot for the same refresh.
4. Executive summary renders correctly for all risk bands and missing-target case.
5. Required Daily Sales shows actionable value when target not yet achieved.
6. Daily Pace Trend chart displays elapsed-day actuals and projected pace.
7. Forecast Confidence reflects day-of-month rules.
8. No AI/ML/external models used.
9. Dashboard is read-only — no write operations.
10. GeneratedAt timestamp displayed; footer links to Sales Report.

---

## 10. Open Questions for Product Owner

| # | Question | Recommended default |
| - | -------- | ------------------- |
| Q1 | Should Executive Dashboard link to Sales Forecast in V1? | No — defer to M26.1 |
| Q2 | Should Alert Center register "Forecast Below Target" signal? | No — defer; avoid alert noise early in month |
| Q3 | Display Remaining Working Days as "—" or hide the card? | Show card with "—" and tooltip |
| Q4 | Sidebar label: "Sales Forecast" or "Sales Forecast Dashboard"? | "Sales Forecast" |
| Q5 | Include Faktur count and Customer Reach from Sales Dashboard? | No — keep forecast-focused; link to Sales Dashboard |

---

## 11. Related Assets Index

| Asset | Path |
| ----- | ---- |
| Sales aggregator | `btr.application/ReportingContext/DashboardSnapshotAgg/Services/DashboardSalesFakturAggregator.cs` |
| Achievement policy | `btr.application/SalesContext/SalesOmzetAgg/Policies/SalesOmzetChartAchievementPolicy.cs` |
| Week grouper | `btr.application/SalesContext/SalesOmzetAgg/Services/SalesOmzetChartWeekGrouper.cs` |
| Target DAL | `btr.infrastructure/SalesContext/SalesOmzetAgg/SalesOmzetTargetDal.cs` |
| Sales snapshot worker | `btr.application/ReportingContext/DashboardSnapshotAgg/UseCases/RefreshDashboardSalesSnapshotWorker.cs` |
| Achievement bands | `btr.application/ReportingContext/DashboardExecutiveAgg/Services/ExecutiveSalesAchievementBandResolver.cs` |
| Sales dashboard UI | `btr.portal.web/src/views/dashboard/SalesDashboardView.vue` |
| M16 forecast gap | `docs/work/btr-portal/M16 Executive Dashboard - Analysis.md` §6.5 |
