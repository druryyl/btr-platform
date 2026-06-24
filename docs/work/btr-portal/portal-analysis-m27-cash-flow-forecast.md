# BTR Portal Analysis — M27 Cash Flow Forecast Dashboard

**Status:** Analysis complete — ready for Architect review and Product Owner approval before implementation.  
**Scope:** Business analysis, forecasting model design, KPI definitions, wireframe, business rules, and collection risk concepts only. No production code.  
**Date:** 2026-06-21  
**Author role:** Analyst  
**Companion document:** [implementation-plan-m27-cash-flow-forecast.md](./implementation-plan-m27-cash-flow-forecast.md)

**Reference documents:** `docs/foundation/PRODUCT.md`, `docs/foundation/DOMAIN.md`, `docs/foundation/LANDSCAPE.md`, `docs/foundation/WORKFLOW.md`, `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-operational.md`, `docs/features/btr-portal/btr-portal-architecture.md`, `docs/features/btr-portal/collection-attention-list-ux/feature.md`, `docs/features/materialized-dashboard/materialized-dashboard-architecture.md`, `docs/features/sales-forecast/feature.md`, `docs/work/btr-portal/M20-collection-dashboard-analysist.md`, `docs/work/btr-portal/M20 Collection Dashboard - Plan.md`, `docs/work/btr-portal/portal-analysis-m26-sales-forecast.md`

---

## 1. Executive Summary

BTR Portal has evolved through four product layers:

| Layer | Question | Example dashboards |
| ----- | -------- | ------------------ |
| Reporting | What happened? | Sales Report, Piutang Report |
| Analytics | How are we performing? | Sales, Piutang, Collection dashboards |
| Decision Support | What requires attention? | Executive Dashboard, Alert Center |
| **Forecasting** | What is likely to happen by month-end? | Sales Forecast (M26), **Cash Flow Forecast (M27)** |

**M27 extends forecasting from invoiced sales into cash collection prediction.**

Management today can answer:

- How much has been invoiced (Sales Dashboard)
- How much is still owed (Piutang Dashboard)
- How much has been collected this month (Collection Dashboard)
- Whether collections keep pace with billing (Recovery vs Billing %)

Management cannot yet answer:

> **"How much cash are we likely to receive before the end of this month?"**

### Key findings

| Finding | Implication for M27 |
| ------- | ------------------- |
| Collection Dashboard (M20) already computes **Cash Collected MTD**, **Month Collections**, **Recovery vs Billing %**, aging, and overdue exposure from materialized snapshots | Forecast KPIs must **reuse** the same pelunasan semantics and reconcile with `BTRPD_CollectionKpi` |
| Sales Forecast (M26) established **Current Pace (Calendar-Day Linear)** as the approved deterministic projection pattern | M27 applies the **same explainable pace model** to collection/cash metrics |
| **No collection target master data** exists in BTR (confirmed M18 analysis) | V1 uses **Month Faktur Omzet** as the implicit collection benchmark (same denominator as Recovery vs Billing) |
| Pelunasan data is available per day via `IPenerimaanPelunasanSalesDal` (FF2 pattern) | Daily cash pace chart is feasible during Collection snapshot refresh |
| Open receivable due dates (`JatuhTempo`) and aging buckets are already computed in `DashboardCollectionAggregator` | Due-date exposure KPIs are **derivable without new SQL** |
| Giro and adjustments are part of **Month Collections** but not literal cash liquidity | Dashboard distinguishes **cash inflow** (BayarTunai) from **total settlement** (TotalBayar) |
| No working-day or holiday calendar in Portal | V1 uses **calendar days** only — same deferral as M26 |
| No historical collection snapshots retained (`SnapshotKey = 'CURRENT'` only) | Historical payment-behavior models deferred; pace projection only |

### Product intent

At any point during the current month, management should answer:

> **"If current collection performance continues, how much cash will we likely receive before month-end, where are the biggest collection risks, and what daily collection pace is required to keep up with billing?"**

---

## 2. Business Objective

### Problem

Finance and operations leadership review the Collection Dashboard and see Cash Collected MTD and Recovery vs Billing %. They must mentally extrapolate whether liquidity will be sufficient through month-end. This mental calculation is inconsistent and does not surface **required daily collection pace** or **projected recovery vs billing**.

Cash flow is more important than revenue recognition for short-term liquidity decisions. M27 shifts the lens from *"how much was collected?"* to *"how much cash should we expect?"*

### Outcome

A dedicated **Cash Flow Forecast Dashboard** that:

1. Projects expected month-end **cash** collection based on current pace.
2. Projects expected month-end **total collections** (cash + giro) for recovery effectiveness.
3. Shows required daily collection to match current-month billing (implicit target).
4. Surfaces receivables due in the remaining month and overdue collection risk.
5. Presents scenario bands (Best / Expected / Worst) using recent momentum.
6. Provides plain-language executive summary for liquidity review.

### Users

| User | Need |
| ---- | ---- |
| Owner / GM | Anticipate month-end liquidity before cash shortfall |
| Finance administration | Validate projected cash against open receivables |
| Collection management | Know required daily pace and priority risks |
| Sales management | Understand whether billing is outpacing recovery |

### Explicitly out of scope (V1)

- AI / ML / statistical models or probability scoring
- Per-customer, per-salesman, or per-wilayah forecast dashboards (company-level only)
- Custom period selection (current month only)
- Working-day or holiday-adjusted pace
- Collection target master data (no BTR source)
- DSO (Days Sales Outstanding) — explicitly deferred in M20
- Pelunasan Report in portal — deferred platform capability
- Alert Center / Executive Dashboard integration
- Cash **outflow** forecasting (purchasing payables) — separate future milestone
- Giro clearing date / bank realization modeling

---

## 3. Phase 1 — Existing Business Capability Analysis

### 3.1 Piutang Dashboard (M14/M5)

**Source:** `DashboardPiutangAggregator` → `BTRPD_PiutangKpi`, `BTRPD_PiutangAging`, `BTRPD_PiutangTopCustomer`

| Capability | Rule | M27 reuse |
| ---------- | ---- | --------- |
| Open balance scope | `KurangBayar > 1` (equivalent to `BTR_Piutang.Sisa > 1`) | Same filter for due-date exposure |
| Aging basis | `DaysOverdue = Today − JatuhTempo` (calendar days) | Same bucket boundaries |
| Aging buckets | Current, 1–30, 31–60, 61–90, >90 (inclusive boundaries) | Reuse `ResolveAgingBucketKey` logic |
| Total Piutang | Sum all open balances | Context KPI — not forecast input |
| Overdue customer | Customer with any overdue bucket balance > 0 | Risk context |
| Top customers | By total outstanding balance | Contrast with M27 overdue/due-soon risks |

**M27 does not duplicate Piutang Dashboard.** Piutang answers exposure; M27 answers projected cash inflow.

### 3.2 Collection Dashboard (M20)

**Source:** `DashboardCollectionAggregator` → `BTRPD_CollectionKpi` and child tables  
**Worker:** `RefreshDashboardCollectionSnapshotWorker` (~30 min cadence)

| Capability | Formula / rule | Source |
| ---------- | -------------- | ------ |
| **Cash Collected MTD** | `SUM(BayarTunai)` for current month | `IPenerimaanPelunasanSalesDal` |
| **Month Collections** | `SUM(TotalBayar)` = Cash + Giro per FF2 | Same DAL |
| **Month Faktur Omzet** | `SUM(Faktur.GrandTotal)` current month | `IFakturViewDal` — same as Sales Dashboard |
| **Recovery vs Billing %** | `Month Collections ÷ Month Faktur Omzet × 100` | Aggregator |
| **Payment Mix** | Cash / Giro / Adjustment shares of settlement total | Aggregator |
| **Overdue Exposure** | Sum `KurangBayar` where bucket ≠ Current | Aggregator |
| **>90d Exposure** | Sum in `DaysOver90` bucket | Aggregator |
| **Overdue Concentration %** | Top debtor overdue ÷ total overdue | Aggregator |
| **Attention signals** | ChronicOverdue, LegacyDebt, PlafondBreachOverdue, LowRecoveryVsBilling, WilayahHotspot, etc. | `BTRPD_CollectionAttention` |
| **Top Overdue rankings** | Customer, Salesman, Wilayah by overdue balance | Child tables |

**Pelunasan semantics (FF2 parity — authoritative):**

| Component | Definition |
| --------- | ---------- |
| BayarTunai | `JenisLunas = 0` |
| BayarGiro | `JenisLunas = 1` |
| TotalBayar | BayarTunai + BayarGiro |
| Adjustment | Retur + Potongan + MateraiAdmin from `BTR_PiutangElement` |
| SettlementTotal | TotalBayar + Adjustment components |
| UangMuka (`JenisLunas = 2`) | **Excluded** — preserve Desktop parity |

**Attribution:** Collections attributed to **invoicing salesman** on underlying Faktur (FF2).

### 3.3 Payment transactions and Pelunasan workflow

**Business workflow** (`WORKFLOW.md`): Collection Visit → Customer Payment → Payment Recording.

**Technical path:**

```text
BTR Desktop: Lunas Piutang (Pelunasan)
    ↓
BTR_PiutangLunas (payment rows by LunasDate)
    ↓
BTR_PiutangElement (adjustments)
    ↓
BTR_Piutang.Sisa recalculated
    ↓
FF2: PenerimaanPelunasanSalesDal (portal aggregation)
```

Portal is read-only — observes posted pelunasan only. No void handling in forecast beyond existing DAL filters.

### 3.4 Sales Forecast Dashboard (M26) — pattern reference

| Pattern | M26 implementation | M27 adaptation |
| ------- | ------------------ | -------------- |
| Primary algorithm | Current Pace linear extrapolation | Same on cash and total collections |
| Policy class | `SalesForecastPolicy` | New `CashFlowForecastPolicy` (parallel structure) |
| Daily buckets | `SalesOmzetChartDayGrouper` + `BTRPD_SalesDailyPace` | New collection day grouper + pace table |
| Scenario bands | Best/Worst from MAX/MIN of MTD vs recent-7-day avg | Same on cash collections |
| Confidence | Low ≤5d, Medium 6–20d, High ≥21d elapsed | Reuse same thresholds |
| Snapshot extension | Extends Sales worker | **Extends Collection worker** |
| Traceability | Forecast actuals = Sales KPI | Forecast cash MTD = Collection KPI |

### 3.5 Materialized reporting tables (relevant)

| Table | Role for M27 |
| ----- | ------------ |
| `BTRPD_CollectionKpi` | Traceability source for Cash Collected MTD, Month Collections, Month Faktur Omzet |
| `BTRPD_CollectionAging` | Overdue bucket context |
| `BTRPD_CollectionAttention` | Risk list seed — forecast extends with due-soon rules |
| `BTRPD_CollectionTopOverdue*` | Rankings context — not duplicated |
| `BTRPD_SalesKpi` | Cross-check Month Faktur Omzet = Total Omzet |
| `BTRPD_PiutangKpi` | Total exposure context (footer reference) |

### 3.6 Reusable calculations — do not duplicate

| Calculation | Reuse from |
| ----------- | ---------- |
| Cash Collected MTD | `DashboardCollectionAggregator` (same pelunasan load) |
| Month Collections | Same |
| Month Faktur Omzet | Same |
| Recovery vs Billing % (actual) | Same |
| Aging bucket assignment | `DashboardCollectionAggregator.ResolveAgingBucketKey` |
| Open balance filter | `KurangBayar > 1` |
| Business date | `IBusinessDateProvider.Today` |
| Current month period | `CurrentMonthPeriode(today)` in Collection worker |

### 3.7 Net-new calculations (M27)

| Calculation | Reason |
| ----------- | ------ |
| Daily cash collection buckets | Not materialized today |
| Cash pace projection (month-end) | No forecast logic exists |
| Total collection pace projection | No forecast logic exists |
| Required daily collection | New policy (mirror M26 Required Daily Sales) |
| Outstanding due in remaining month | New aggregation over open balances |
| Collection forecast risk rows | Due-soon and concentration rules |
| Recovery vs Billing forecast % | Projected collections ÷ billing |
| Scenario bands on cash | New policy |

### 3.8 Purchasing context (read-only cross-reference)

Purchasing Dashboard tracks **cash outflow** via purchase invoices. M27 V1 focuses on **customer payment inflow** only. Purchasing spend may appear in future net-cash-flow milestones but is **not** part of M27 forecast numerator or denominator.

---

## 4. Phase 2 — Cash Flow Forecast Analysis

### 4.1 Candidate algorithms

All candidates use **current calendar month** pelunasan data and the same Month Faktur Omzet benchmark as Collection Dashboard.

| # | Algorithm | Formula (conceptual) | Simplicity | Explainability | Stability | Business usefulness |
| - | --------- | -------------------- | ---------- | -------------- | --------- | ------------------- |
| A | **Current Cash Pace (Calendar-Day Linear)** | `Forecast Cash = (Cash MTD ÷ DE) × DIM` | ★★★★★ | ★★★★★ | ★★★☆☆ | ★★★★★ |
| B | Historical Daily Collection Average | Equivalent to A | ★★★★★ | ★★★★★ | ★★★☆☆ | ★★★★★ |
| C | Total Collection Pace (Cash + Giro) | `(Month Collections ÷ DE) × DIM` | ★★★★★ | ★★★★☆ | ★★★☆☆ | ★★★★☆ |
| D | Remaining Working Day projection | Pace × remaining working days | ★★★☆☆ | ★★★★☆ | ★★☆☆☆ | ★★★★☆ |
| E | Aging-Based Collection Probability | `Σ (bucket balance × fixed collection rate)` | ★★☆☆☆ | ★★★☆☆ | ★★☆☆☆ | ★★★☆☆ |
| F | Due-Date-Based Expected Collection | `Cash MTD + Σ balances due in remaining month` | ★★★★☆ | ★★★★★ | ★★★★☆ | ★★★★☆ |
| G | Customer Payment Pattern Projection | Per-customer historical avg days-to-pay | ★☆☆☆☆ | ★★☆☆☆ | ★☆☆☆☆ | ★★★☆☆ |
| H | Month-End Collection = MIN(Pace, Due Pipeline) | Cap pace by collectible due amounts | ★★★☆☆ | ★★★☆☆ | ★★★☆☆ | ★★★★☆ |

### 4.2 Algorithm evaluation notes

**A — Current Cash Pace (recommended primary for liquidity):**

- Directly answers: *"If cash collection continues at today's daily average, how much cash will we receive by month-end?"*
- Finance managers already reason this way for liquidity
- Uses only observed cash (BayarTunai) — aligns with "cash flow" language
- Weakness: early-month volatility — mitigated by Forecast Confidence indicator

**C — Total Collection Pace (recommended secondary for recovery):**

- Projects **Month Collections** (Cash + Giro) for Recovery vs Billing Forecast
- Aligns with M20 Recovery vs Billing semantics
- Displayed alongside cash projection — not a separate "model"

**F — Due-Date-Based floor/context:**

- `Outstanding Due Remaining` = open balances not yet due but with `JatuhTempo` in `(today, month-end]`
- Useful **context KPI** — shows collectible pipeline independent of pace
- **Not used as primary forecast** because many overdue balances may still convert to cash this month; using only due-soon amounts **under-forecasts** typical distributor collection patterns

**E — Aging-Based Probability:**

- Requires arbitrary collection-rate assumptions per bucket (e.g., 80% for 1–30 days)
- Rates are not defined in BTR business rules — would appear as black-box to management
- **Rejected for V1**

**G — Customer Payment Pattern:**

- Requires multi-month payment history per customer at daily grain
- No persisted historical collection snapshots — high implementation cost
- **Deferred** to future customer payment profiling milestone

**H — MIN(Pace, Pipeline) cap:**

- Artificially limits forecast when pace exceeds due pipeline
- Distributor businesses routinely collect against overdue balances, not only current-due
- **Rejected** — pace projection is more useful for management action

### 4.3 Recommended algorithm (V1)

**Dual-metric Current Pace Projection — Calendar-Day Linear Extrapolation**

Management explanation (plain language):

> *"We divide this month's cash collected so far by the number of days that have passed, then multiply by the total days in the month. This assumes cash collection continues at the same daily average. We show the same projection for total collections (cash + giro) to forecast recovery vs billing."*

#### Symbols

| Symbol | Meaning |
| ------ | ------- |
| `B` | Business date (`IBusinessDateProvider.Today`) |
| `MS` | Month start (1st) |
| `ME` | Month end (last day) |
| `DIM` | Days in month = `(ME - MS).Days + 1` |
| `DE` | Days elapsed = `(B - MS).Days + 1` (inclusive, min 1) |
| `DR` | Days remaining = `(ME - B).Days` |
| `CC` | Cash Collected MTD = `SUM(BayarTunai)` |
| `MC` | Month Collections = `SUM(TotalBayar)` |
| `BO` | Month Faktur Omzet (billing benchmark) |

#### Core formulas

| KPI | Formula |
| --- | ------- |
| Daily Cash Collection Average | `CC / DE` |
| **Expected Cash Collection** | `(CC / DE) × DIM` |
| **Projected Month-End Collection (cash)** | Same as Expected Cash Collection |
| Daily Collection Average (total) | `MC / DE` |
| **Projected Month-End Total Collections** | `(MC / DE) × DIM` |
| **Collection Forecast %** | `Projected Month-End Total Collections ÷ BO × 100` |
| **Recovery vs Billing Forecast %** | Same as Collection Forecast % |
| **Remaining Collection Target** | `BO − MC` when `BO > MC`; else `0` |
| **Required Daily Collection** | `(BO − MC) / DR` when `BO > MC` and `DR > 0`; else `0` |
| **Collection Gap** | `BO − Projected Month-End Total Collections` |
| **Forecast Variance (cash)** | `Expected Cash Collection − CC` |
| **Expected Collection Rate (context)** | `CC ÷ (CC + Outstanding Due Remaining) × 100` when denominator > 0 |

#### Scenario bands (cash — secondary, explainable range)

| Scenario | Formula | Plain language |
| -------- | ------- | -------------- |
| **Expected** | Current Cash Pace | Month-average daily cash rate continues |
| **Best Case** | `MAX(MTD daily cash avg, Recent-7-day daily cash avg) × DIM` | Recent week cash collection stronger than month average |
| **Worst Case** | `MIN(MTD daily cash avg, Recent-7-day daily cash avg) × DIM` | Recent week weaker than month average |

Recent-7-day cash average: sum `BayarTunai` for calendar days `(B−6)..B` inclusive, divided by 7. When `DE < 7`: fallback to MTD daily cash average.

#### Outstanding Due Remaining (context KPI — not forecast cap)

| KPI | Formula |
| --- | ------- |
| **Outstanding Due This Month (remaining)** | `SUM(KurangBayar)` where `JatuhTempo > B` AND `JatuhTempo ≤ ME` AND `KurangBayar > 1` |
| **Overdue Outstanding** | Reuse Collection `OverdueExposure` from same refresh |

---

## 5. Phase 3 — Dashboard Wireframe

Text layout only. Route: `/dashboard/cash-flow-forecast`.

```text
┌─────────────────────────────────────────────────────────────────────────────┐
│  Cash Flow Forecast Dashboard                                [↻ Refresh]    │
│  Current month — projected cash collection (liquidity)                      │
│  As of: {GeneratedAt}  ·  Period: {Month Year}  ·  Day {DE} of {DIM}       │
├─────────────────────────────────────────────────────────────────────────────┤
│  EXECUTIVE SUMMARY (plain-language sentence)                                │
│  "At current pace, cash collection is projected to reach {ExpectedCash}     │
│   by month-end. Total collections are projected at {ProjectedTotal}        │
│   ({CollectionForecast}% of billing). {Risk sentence}."                     │
├─────────────────────────────────────────────────────────────────────────────┤
│  KPI ROW 1 — Cash Position                                                  │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐       │
│  │ Cash         │ │ Expected     │ │ Projected    │ │ Collection   │       │
│  │ Collected MTD│ │ Cash         │ │ Month-End    │ │ Forecast %   │       │
│  │ {CC}         │ │ Collection   │ │ Collection   │ │ {CF%}        │       │
│  │              │ │ {Expected}   │ │ {Expected}   │ │              │       │
│  └──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘       │
├─────────────────────────────────────────────────────────────────────────────┤
│  KPI ROW 2 — Pace & Target                                                  │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐       │
│  │ Daily Cash   │ │ Required     │ │ Remaining    │ │ Remaining    │       │
│  │ Collection   │ │ Daily        │ │ Collection   │ │ Calendar     │       │
│  │ Average      │ │ Collection   │ │ Target       │ │ Days         │       │
│  │ {Pace}       │ │ {Required}   │ │ {BO-MC}      │ │ {DR}         │       │
│  └──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘       │
├─────────────────────────────────────────────────────────────────────────────┤
│  KPI ROW 3 — Recovery & Scenarios                                             │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐       │
│  │ Recovery vs  │ │ Recovery vs  │ │ Best / Exp / │ │ Forecast     │       │
│  │ Billing      │ │ Billing      │ │ Worst Cash   │ │ Confidence   │       │
│  │ (Actual)     │ │ Forecast     │ │ Projections  │ │ {Low/Med/Hi} │       │
│  │ {Actual%}    │ │ {Forecast%}  │ │ {3 values}   │ │              │       │
│  └──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘       │
├─────────────────────────────────────────────────────────────────────────────┤
│  KPI ROW 4 — Receivable Context                                             │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐       │
│  │ Outstanding  │ │ Overdue      │ │ Collection   │ │ Collection   │       │
│  │ Due Remaining│ │ Outstanding  │ │ Gap          │ │ Forecast     │       │
│  │ This Month   │ │              │ │ {BO-Proj}    │ │ Variance     │       │
│  │ {DueRem}     │ │ {Overdue}    │ │              │ │ {FC-CC}      │       │
│  └──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘       │
├─────────────────────────────────────────────────────────────────────────────┤
│  CHART 1 — Daily Collection Pace (full width)                               │
│  Bar: Actual daily cash (solid) + dashed MTD daily average reference line │
│  X-axis: calendar days 1..DIM  ·  Y-axis: cash amount                      │
│  Highlight: elapsed vs remaining days                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│  CHART 2 — Cash Forecast vs Billing (half)  │  CHART 3 — Recovery Trend   │
│  Grouped bar: Billing | Cash MTD | Forecast │  Line: Cumulative cash MTD   │
│  Three-bar liquidity comparison              │  vs cumulative billing pace  │
├─────────────────────────────────────────────────────────────────────────────┤
│  TOP COLLECTION RISKS (table — forecast-specific, Top 10)                   │
│  Columns: Risk Type | Entity | Amount | Due / Aging | Rule Label           │
│  Rows: Due within 7 days · Large overdue · Chronic · Concentration          │
├─────────────────────────────────────────────────────────────────────────────┤
│  FOOTER — Traceability                                                      │
│  "Forecast based on {N} pelunasan days through {B}. Cash MTD matches       │
│   Collection Dashboard. Billing matches Sales Dashboard.                      │
│   View receivable evidence → Piutang Report"                                │
└─────────────────────────────────────────────────────────────────────────────┘
```

**Navigation placement:** Sidebar under **Dashboard → Cash Flow Forecast** (after Collection, before Customer Analytics or per operational doc update).

**Relationship to Collection Dashboard:** Collection = recovery performance and attention signals today; Cash Flow Forecast = projected month-end liquidity and required pace.

---

## 6. Phase 4 — KPI Definitions

All monetary values in IDR. Percentages rounded to 1 decimal unless noted.

### 6.1 Primary KPIs

#### Cash Collected (Month-To-Date)

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Literal cash received from customers this month |
| **Formula** | `SUM(BayarTunai)` for `LunasDate` in current month |
| **Management interpretation** | "How much cash has actually come in?" |
| **Color/status** | Neutral (informational) |
| **Drill-down** | Collection Dashboard Recovery Summary; Piutang Report (evidence gap — no Pelunasan Report V1) |

#### Expected Cash Collection

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Projected month-end cash if current daily cash pace continues |
| **Formula** | `(CC / DE) × DIM` |
| **Management interpretation** | "How much cash will we likely receive by month-end?" |
| **Color/status** | Neutral; compare to billing bar |
| **Drill-down** | Daily Collection Pace chart |

#### Projected Month-End Collection

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Same as Expected Cash Collection (primary liquidity headline) |
| **Formula** | `(CC / DE) × DIM` |
| **Management interpretation** | Month-end cash expectation |
| **Color/status** | Neutral |
| **Drill-down** | Cash Forecast vs Billing chart |

#### Collection Forecast %

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Projected total collections as a percentage of current-month billing |
| **Formula** | `Projected Month-End Total Collections ÷ BO × 100` |
| **Management interpretation** | "Will we collect enough to match what we billed this month?" |
| **Color/status** | Healthy ≥100%; Warning 80–99%; Critical <80%; Unknown when BO = 0 |
| **Drill-down** | Collection Dashboard (actual Recovery vs Billing) |

#### Recovery vs Billing (Actual)

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Current collections pace against new billing — M20 KPI |
| **Formula** | `MC ÷ BO × 100` |
| **Management interpretation** | "Are we keeping up right now?" |
| **Color/status** | Same bands as Collection Forecast % |
| **Drill-down** | Collection Dashboard |

#### Recovery vs Billing Forecast

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Projected month-end recovery effectiveness |
| **Formula** | Same as Collection Forecast % |
| **Management interpretation** | "Where will recovery vs billing finish?" |
| **Color/status** | Healthy / Warning / Critical bands |
| **Drill-down** | Recovery Trend chart |

#### Daily Collection Average (Cash)

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Average daily cash collected per elapsed calendar day |
| **Formula** | `CC / DE` |
| **Management interpretation** | "What is our cash collection pace?" |
| **Color/status** | Neutral; compare to Required Daily |
| **Drill-down** | Daily Pace chart |

#### Remaining Collection Target

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Additional total collections needed to match current-month billing |
| **Formula** | `BO − MC` when `BO > MC`; else `0` |
| **Management interpretation** | "How much more must we collect to keep up with billing?" |
| **Color/status** | Warning when > 0; Healthy when 0 |
| **Drill-down** | Collection Dashboard |

#### Required Daily Collection

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Average total collections needed per remaining day to match billing |
| **Formula** | `(BO − MC) / DR` when `BO > MC` and `DR > 0`; else `0` |
| **Management interpretation** | "What daily collection run-rate closes the billing gap?" |
| **Color/status** | Warning when > 1.5× daily collection average; Critical when > 2× |
| **Drill-down** | Daily Pace chart |

#### Remaining Working Days

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Business days excluding weekends/holidays |
| **Formula** | **Not computed in V1** — display "—" with tooltip |
| **Management interpretation** | Deferred |
| **Color/status** | N/A |
| **Drill-down** | N/A |

#### Outstanding Due This Month (Remaining)

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Open balances not yet due but with due date in the remaining month |
| **Formula** | `SUM(KurangBayar)` where `JatuhTempo > B` AND `JatuhTempo ≤ ME` |
| **Management interpretation** | "What is still collectible on schedule this month?" |
| **Color/status** | Neutral (context) |
| **Drill-down** | Piutang Report (filter client-side by due date) |

#### Expected Collection Rate

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Share of near-term collectible pipeline already converted to cash |
| **Formula** | `CC ÷ (CC + Outstanding Due Remaining) × 100` |
| **Management interpretation** | "How much of the due-soon pipeline have we already collected?" |
| **Color/status** | Healthy ≥80%; Warning 50–79%; Critical <50% |
| **Drill-down** | Piutang Report |

#### Forecast Variance

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Additional cash projected between today and month-end |
| **Formula** | `Expected Cash Collection − CC` |
| **Management interpretation** | "How much more cash is projected?" |
| **Color/status** | Neutral |
| **Drill-down** | None |

#### Collection Gap

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Shortfall between projected total collections and billing |
| **Formula** | `BO − Projected Month-End Total Collections` (negative = surplus) |
| **Management interpretation** | "Will billing outpace collections by month-end?" |
| **Color/status** | Critical if gap > 20% of BO; Warning if gap > 0 |
| **Drill-down** | Collection Dashboard |

#### Best Case Projection (Cash)

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Optimistic cash month-end if recent week momentum holds |
| **Formula** | `MAX(MTD daily cash avg, Recent-7-day daily cash avg) × DIM` |
| **Management interpretation** | "Best realistic cash outcome" |
| **Color/status** | Informational |
| **Drill-down** | Daily Pace chart |

#### Expected Projection (Cash)

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Primary cash forecast — same as Expected Cash Collection |
| **Formula** | Current Cash Pace |
| **Management interpretation** | Most likely cash outcome |
| **Color/status** | Primary highlight |
| **Drill-down** | Daily Pace chart |

#### Worst Case Projection (Cash)

| Attribute | Definition |
| --------- | ---------- |
| **Business meaning** | Conservative cash month-end if recent week weakness continues |
| **Formula** | `MIN(MTD daily cash avg, Recent-7-day daily cash avg) × DIM` |
| **Management interpretation** | "Downside cash scenario" |
| **Color/status** | Informational |
| **Drill-down** | Daily Pace chart |

### 6.2 Derived indicators

#### Forecast Confidence

| Days elapsed | Confidence |
| ------------ | ---------- |
| ≤ 5 | Low |
| 6–20 | Medium |
| ≥ 21 | High |

#### Forecast Risk Band (liquidity)

Based on **Collection Forecast %**:

| Band | Threshold |
| ---- | --------- |
| Healthy | ≥ 100% |
| Warning | 80–99% |
| Critical | < 80% |
| Unknown | BO = 0 |

---

## 7. Phase 5 — Collection Risk Forecast

Forecast-specific risks use **deterministic rules** on open receivable and collection data. No AI scoring. Rules produce a **Top Collection Risks** table (max 10 rows, priority ordered).

### 7.1 Risk rules

| Risk key | Label | Deterministic rule | Priority |
| -------- | ----- | ------------------ | -------- |
| `LargeDueSoon` | Large Invoice Due Soon | Open `KurangBayar > 1` AND `JatuhTempo` in `(B, B+7]` AND balance ≥ max(company overdue P75, fixed floor IDR 50M) | 1 |
| `ChronicOverdueLarge` | Chronic Overdue — Large | Any balance in `DaysOver90` bucket AND customer total overdue ≥ Top-10 customer overdue threshold | 2 |
| `OverdueConcentration` | Collection Concentration Risk | Customer overdue balance ≥ 15% of company overdue exposure (reuse M20 concentration concept) | 3 |
| `LegacyDebtOverdue` | Legacy Debt — Overdue | Customer in M20 `LegacyDebt` signal set AND overdue balance > 0 | 4 |
| `PlafondBreachDueSoon` | Plafond Breach — Due Soon | Plafond breach (M20 rule) AND `JatuhTempo` in `(B, B+14]` | 5 |
| `LowRecoveryCustomer` | Deteriorating — Low Recovery | Customer's invoicing salesman in M20 `LowRecoveryVsBilling` set AND customer overdue > 0 | 6 |
| `WilayahHotspotDue` | Wilayah Hotspot — Due Exposure | Wilayah in M20 hotspot set AND sum due-soon balances in wilayah ≥ 10% of company due-soon total | 7 |
| `ExpectedOverdueGrowth` | Expected Overdue Growth | Customer overdue > 0 AND no cash collection (`BayarTunai`) attributed to customer's open faktur salesman in last 30 days AND overdue increased (balance in overdue buckets only) | 8 |

**Fixed floor IDR 50M:** Architect may adjust to configurable threshold aligned with company scale; default constant in policy class for V1.

**P75 floor:** Computed at refresh from customer overdue distribution — if insufficient data, use fixed floor only.

### 7.2 Risk row shape

| Column | Content |
| ------ | ------- |
| RiskType | Rule label |
| EntityType | Customer / Wilayah |
| EntityName | Display name |
| Amount | Relevant balance (overdue or due-soon) |
| DueOrAging | JatuhTempo date or aging bucket label |
| RuleExplanation | Short deterministic text, e.g. "Due in 4 days, balance Rp 120M" |
| ReportRoute | `/reports/piutang` |

### 7.3 Explicit exclusions

| Concept | Status |
| ------- | ------ |
| AI risk score | Not accepted |
| Payment probability by ML | Not accepted |
| Promise-to-pay | No BTR data source |
| Visit-to-payment conversion | Deferred (M25) |

---

## 8. Phase 6 — Business Rules

### 8.1 Scope rules

| Rule ID | Rule |
| ------- | ---- |
| CFR-01 | Forecast **current calendar month only** |
| CFR-02 | Business date from `IBusinessDateProvider.Today` |
| CFR-03 | Pelunasan grouped by `LunasDate` (payment posting date) |
| CFR-04 | Billing benchmark = non-void Faktur `GrandTotal` current month — same as Sales/Collection |
| CFR-05 | Open receivable rows: `KurangBayar > 1` |
| CFR-06 | Void invoices excluded from billing benchmark |
| CFR-07 | UangMuka excluded from collections — FF2 parity |

### 8.2 Collection target rules

| Rule ID | Rule |
| ------- | ---- |
| CFR-10 | **No collection target master** — implicit target = Month Faktur Omzet |
| CFR-11 | When `BO = 0`: Collection Forecast % = null; Required Daily = 0; Risk = Unknown |
| CFR-12 | When collections already match or exceed billing (`MC ≥ BO`): Remaining Collection Target = 0; Required Daily = 0 |

### 8.3 Calculation rules

| Rule ID | Rule |
| ------- | ---- |
| CFR-20 | `DE = max(1, (B - MS).Days + 1)` |
| CFR-21 | `DR = max(0, (ME - B).Days)` |
| CFR-22 | When `CC = 0` on day 1: cash forecast = 0 |
| CFR-23 | When month complete (`B = ME`): forecast = actual MTD |
| CFR-24 | Recalculate on every **Collection** snapshot refresh (~30 min) |
| CFR-25 | Daily cash buckets for elapsed days sum to `CC` |

### 8.4 Receivable treatment

| Rule ID | Rule |
| ------- | ---- |
| CFR-30 | Overdue invoices included in risk rules — not excluded from forecast context |
| CFR-31 | Future due dates: `Outstanding Due Remaining` uses `JatuhTempo > B` |
| CFR-32 | Closed receivables (`Sisa ≤ 1`) ignored |
| CFR-33 | Aging for risks reuses M14/M20 inclusive boundaries |

### 8.5 Calendar rules

| Rule ID | Rule |
| ------- | ---- |
| CFR-40 | **Calendar days** for pace — weekends and holidays included |
| CFR-41 | Working-day metrics: display "—" in V1 |

### 8.6 Beginning-of-month behavior

| Day | Behavior |
| --- | -------- |
| Day 1, CC = 0 | Forecasts = 0; Confidence = Low; Summary: "Insufficient collection data — forecast stabilizes as payments are recorded." |
| Days 2–5 | Computed normally; Confidence = Low |

### 8.7 Traceability rules

| Rule ID | Rule |
| ------- | ---- |
| CFR-50 | Forecast `CashCollectedMtd` **must equal** `BTRPD_CollectionKpi.CashCollectedMtd` same refresh |
| CFR-51 | Forecast `MonthCollections` **must equal** `BTRPD_CollectionKpi.MonthCollections` |
| CFR-52 | Forecast `MonthFakturOmzet` **must equal** `BTRPD_CollectionKpi.MonthFakturOmzet` and `BTRPD_SalesKpi.TotalOmzet` |
| CFR-53 | API `GeneratedAt` = snapshot time, not request time |

---

## 9. Phase 7 — User Experience Recommendations

### 9.1 Layout principles

- Use `DashboardDetailLayout` shell (title, subtitle, GeneratedAt, refresh)
- **Subtitle:** "Current month forecast — projected cash collection and recovery pace."
- Executive summary first — answers liquidity question in one sentence
- Four KPI rows: Cash → Pace → Recovery → Receivable context
- Charts prove pace; risk table drives action
- Traceability footer links Piutang Report

### 9.2 Card ordering rationale

| Order | Rationale |
| ----- | --------- |
| 1. Executive Summary | Immediate liquidity answer |
| 2. Cash Position KPIs | Headline numbers |
| 3. Pace & Target | Actionable required daily |
| 4. Recovery & Scenarios | Billing alignment + range |
| 5. Receivable Context | Pipeline and gap |
| 6. Daily Pace Chart | Visual evidence |
| 7. Forecast vs Billing + Recovery Trend | Comparison |
| 8. Top Collection Risks | Prioritized intervention list |
| 9. Footer | Trust and evidence |

### 9.3 Charts

| Chart | Type | Data |
| ----- | ---- | ---- |
| Daily Collection Pace | Bar (actual daily cash) + dashed line (MTD avg) | `BTRPD_CashFlowDailyPace` |
| Cash Forecast vs Billing | Grouped bar: Billing, Cash MTD, Projected Cash | KPI snapshot |
| Recovery Trend | Dual line: cumulative MC vs cumulative BO by day | Derived at refresh from daily buckets + faktur daily |

### 9.4 Executive summary templates

| Condition | Template |
| --------- | -------- |
| BO = 0 | "No billing recorded this month — collection forecast unavailable." |
| Confidence Low | "Early-month forecast — cash pace may change significantly." |
| Forecast ≥ billing | "Cash collection is projected to keep pace with billing." |
| Forecast < billing, Warning | "Projected collections trail billing — increased collection effort needed." |
| Forecast < billing, Critical | "Liquidity risk — projected collections significantly below billing." |

### 9.5 Drill-down destinations

| Investigation | Destination |
| ------------- | ----------- |
| Validate cash collected | Collection Dashboard |
| Validate billing | Sales Dashboard / Sales Report |
| Validate open balances | Piutang Report |
| Operational collection action | BTR Desktop — Lunas Piutang |

---

## 10. Future Extensibility

M27 V1 architecture should allow extension without redesign:

| Future capability | Extension path |
| ----------------- | -------------- |
| Customer payment behavior profiling | Add customer-level history tables; new aggregator methods |
| Collection trend forecasting | Retain daily pace tables; add weekly/monthly rollups |
| DSO | New KPI from billing and collection dates — separate policy |
| Cash flow by territory / salesman | Filter pelunasan aggregation — new dimensional tables |
| Weekly cash flow forecast | Shorter horizon policy variant on same pace engine |
| Scenario simulation | Best/Expected/Worst already scaffolded — add user-adjustable pace % |
| Net cash flow (in − out) | Join purchasing outflow snapshot — new dashboard |
| Working-day calendar | Replace `DIM`/`DR` in policy when master data exists |
| Collection target master | Optional override of `BO` implicit target |
| Alert Center signals | Register `LowCollectionForecast` from snapshot KPIs |

---

## 11. Open Questions for Product Owner

| # | Question | Analyst recommendation |
| - | -------- | ---------------------- |
| 1 | Should primary headline be **cash only** or **total collections**? | **Cash** for headline (liquidity); total collections for recovery % |
| 2 | Large invoice due-soon floor (IDR 50M)? | Confirm with business scale |
| 3 | Executive Dashboard promotion of Collection Forecast %? | Defer post-M27 — same pattern as M20 promotion |
| 4 | Sidebar label: "Cash Flow Forecast" vs "Collection Forecast"? | **Cash Flow Forecast** — distinguishes from Collection Dashboard |

---

## Document Maintenance

On Product Owner approval, Architect produces `implementation-plan-m27-cash-flow-forecast.md`. On implementation, Knowledge Curator updates `docs/features/btr-portal/btr-portal-domain.md` Section 4 (add Forecasting concepts) and creates `docs/features/cash-flow-forecast/feature.md`.

**Success criterion:** Management can answer, at any point during the month: *"If current collection performance continues, how much cash will we likely receive before month-end, where are the biggest collection risks, and what daily collection pace is required to achieve the company's cash flow target?"* — using only explainable, deterministic rules traceable to Collection and Piutang data.
