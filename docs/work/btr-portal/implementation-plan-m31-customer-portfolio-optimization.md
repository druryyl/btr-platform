# Implementation Plan — M31 Customer Portfolio Optimization Dashboard

## Document Status

| Field | Value |
| ----- | ----- |
| Milestone | M31 — Customer Portfolio Optimization Dashboard |
| Authoritative requirements | [feature.md](../../features/customer-portfolio-optimization/feature.md) |
| Discovery source | [portal-analysis-m31-customer-portfolio-optimization.md](./portal-analysis-m31-customer-portfolio-optimization.md) |
| Reference pattern | M30 Collection Optimization, M29 Customer Risk Forecast, M17 Customer Analytics, materialized-dashboard architecture |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |
| Status | **Ready for implementation** |

**Prerequisites:**

| Prerequisite | Status | Notes |
| ------------ | ------ | ----- |
| M17 Customer Analytics | Required | Current-state inputs |
| M29 Customer Risk Forecast | Required | Forward-risk inputs — [implementation-summary-m29-customer-risk-forecast.md](./implementation-summary-m29-customer-risk-forecast.md) or equivalent |
| M30 Collection Optimization | Required | Collect action link resolution — [implementation-summary-m30-collection-optimization.md](./implementation-summary-m30-collection-optimization.md) or equivalent |
| IsSuspend CustomerForm Desktop fix | Parallel work item | Do not block M31 delivery; suspend-related portfolio rules degrade gracefully when master flag unreliable |

---

## 1. Goal

Deliver a read-only **Customer Portfolio Optimization Dashboard** at `/dashboard/customer-portfolio` that composes M17, M29, and M30 outputs into **portfolio management actions** (Grow · Retain · Protect · Collect · Review Credit · Recover · Monitor · Exit Review) across **all customers**, with default view **Attention Customers only**.

Deliver a supporting **Customer Report** at `/reports/customers` as the approved drill-down layer between portfolio dashboard and Sales/Piutang reports.

**In scope:**

- New portfolio aggregation step during existing **Customer** snapshot refresh (after M30 step)
- In-memory consumption of M17 aggregate result, M29 `CustomerRiskForecastContext` list, and M30 `CollectionOptimizationContext` map — **no recalculation** of upstream rules
- Cross-read M18 salesman snapshot (summary fields) and optionally M22 location snapshot (wilayah breakdown)
- New lightweight DAL loads for **first Faktur date** and **purchase frequency** (tier/lifecycle inputs not available today)
- New snapshot tables for portfolio KPIs, distributions, customer rows, and concentration
- New API endpoints `GET /api/dashboard/customer-portfolio` and `GET /api/reports/customers`
- New Portal Web dashboard page, Customer Report page, and M16 executive summary cards
- Unit tests for portfolio policy, action builder, lifecycle/tier resolvers, and aggregator

**Out of scope:**

- Changes to M17/M29/M30 API response shapes or upstream rule engines
- M30 collection queue duplication on M31 surface
- Alert Center integration
- Field activity (M18.5) integration
- Profitability, retur ratio, net sales, HargaType in portfolio logic
- AI/ML, Desktop write-back, credit enforcement
- Historical portfolio snapshot retention

---

## 2. Impact Analysis

### 2.1 Business areas affected

| Area | Impact |
| ---- | ------ |
| Management (Owner/GM) | Primary — portfolio health, concentration, working capital allocation |
| Sales management | Primary — assigned portfolio, growth/retention actions |
| Finance | Secondary — Collect and Review Credit handoff to M30 |
| Customer master | Read-only — Wilayah, Klasifikasi, Plafond, IsSuspend |

### 2.2 Systems affected

| System | Impact |
| ------ | ------ |
| BTR Portal API | New dashboard + report controllers, MediatR queries |
| BTR Portal Web | New dashboard route, report route, executive cards, navigation |
| BTR Portal Worker | Extended Customer snapshot refresh |
| BTR SQL | New snapshot tables + optional DAL-supporting views |
| BTR Desktop | None (IsSuspend fix is separate prerequisite) |

### 2.3 Existing features — preserve behavior

| Feature | Change |
| ------- | ------ |
| Customer Analytics (M17) | **None** |
| Customer Risk Forecast (M29) | **None** |
| Collection Optimization (M30) | **None** — M31 links to M30 for Collect |
| Piutang / Collection / Salesman dashboards | **None** |
| Sales Report / Piutang Report | **None** — Customer Report is additive |
| Customer refresh cadence | **Unchanged** (~30 min) |

### 2.4 Source code modules affected

| Layer | Module | Change type |
| ----- | ------ | ----------- |
| Application | `ReportingContext/DashboardSnapshotAgg/Services/` | **Add** `DashboardCustomerPortfolioAggregator`, `CustomerPortfolioOptimizationPolicy`, `CustomerPortfolioLifecycleResolver`, `CustomerPortfolioTierResolver`, `CustomerPortfolioActionBuilder`, `CustomerPortfolioExecutiveSummaryBuilder` |
| Application | `ReportingContext/DashboardSnapshotAgg/Models/` | **Add** portfolio aggregate DTOs and in-memory context |
| Application | `ReportingContext/DashboardSnapshotAgg/UseCases/` | **Extend** `RefreshDashboardCustomerSnapshotWorker` |
| Application | `ReportingContext/DashboardSnapshotAgg/Contracts/` | **Extend** `IDashboardCustomerSnapshotDal` |
| Application | `ReportingContext/DashboardCustomerPortfolioAgg/` | **New** query + read contract |
| Application | `ReportingContext/CustomerReportAgg/` | **New** query + read contract |
| Application | `ReportingContext/DashboardExecutiveAgg/` | **Extend** composer + response for portfolio summary cards |
| Application | `SalesContext/FakturInfo/` | **Extend** `ICustomerLastFakturDal` or **Add** frequency/first-purchase DAL |
| Application | `ReportingContext/Shared/` | **Extend** `InvestigationRegistry` with M31 routes |
| Infrastructure | `ReportingContext/DashboardSnapshotAgg/` | **Extend** snapshot DAL write/read |
| Infrastructure | `ReportingContext/DashboardCustomerPortfolioAgg/` | **Add** read facade |
| Infrastructure | `ReportingContext/CustomerReportAgg/` | **Add** read facade |
| Infrastructure | `SalesContext/FakturInfoAgg/` | **Extend** DAL implementations |
| SQL | `btr.sql/Tables/`, `btr.sql/Scripts/` | **Add** portfolio tables + upgrade script |
| Portal API | `Controllers/Dashboard/`, `Controllers/Reports/` | **Add** controllers |
| Portal Web | `views/dashboard/`, `views/reports/`, `components/`, `stores/`, `api/`, `router/` | **Add** pages + components |
| Tests | `btr.test/ReportingContext/` | **Add** policy, builder, aggregator tests |

### 2.5 Risk summary

| Risk | Severity | Mitigation |
| ---- | -------- | ---------- |
| Scope creep — reimplementing M17/M29/M30 rules | High | Composition-only constraint; code review against §15 analysis do-not-duplicate list |
| Large customer row materialization | Medium | Same pattern as M14 `BTRPD_PiutangCustomerAging`; index on `SnapshotKey` + `IsAttention`; client default filter |
| First-purchase / frequency data not cached today | Medium | New deterministic DAL queries; load once per refresh |
| M18 snapshot temporal skew vs Customer refresh | Low | Cross-read at refresh start; null-safe salesman summary fields |
| IsSuspend master data unreliable | Medium | Prerequisite Desktop fix tracked separately; Protect/Review Credit rules do not solely depend on suspend flag |
| False precision on tier/lifecycle labels | Medium | Document thresholds in policy constants; reason text cites rule ids |
| Collect action diverges from M30 | High | Collect resolves only when M30 `IsActionable`; link carries customer key query param |
| Executive page scope creep | Medium | Summary cards only — no portfolio tables on M16 |

---

## 3. Architecture Overview

### 3.1 Topology

```text
Task Scheduler (Customer domain, ~30 min)
        ↓
RefreshDashboardCustomerSnapshotWorker
        ↓
Load: (existing M17/M29/M30 loads)
      ICustomerLastFakturDal.ListLastFakturWithSalesmanByCustomer()     ← portfolio owner
      ICustomerFirstFakturDal.ListFirstFakturByCustomer()               ← NEW — lifecycle New
      ICustomerPurchaseFrequencyDal.ListFrequency(6mo lookback)         ← NEW — tier input
      IDashboardSalesmanSnapshotDal.GetCurrent()                        ← M18 cross-read
      IDashboardLocationSnapshotDal.GetCurrent()                        ← optional M22 cross-read
        ↓
DashboardCustomerAggregator.Aggregate()                               → BTRPD_Customer* (unchanged)
        ↓
DashboardCustomerRiskForecastAggregator.Aggregate()                   → BTRPD_CustomerRiskForecast* (unchanged)
        ↓  (returns forecastAggregate + Contexts)
DashboardCollectionOptimizationAggregator.Aggregate()                 → BTRPD_CollectionOptimization* (unchanged)
        ↓  (returns optimizationAggregate + per-customer opt contexts)
DashboardCustomerPortfolioAggregator.Aggregate()                      → BTRPD_CustomerPortfolio* (new)
        ↓
DashboardCustomerSnapshotDal.ReplaceCurrent()                         → single transaction (M17+M29+M30+M31)

Browser → GET /api/dashboard/customer-portfolio
        ↓ MediatR → IDashboardCustomerPortfolioDal → snapshot SELECT

Browser → GET /api/reports/customers?customerCode=...
        ↓ MediatR → ICustomerReportDal → snapshot SELECT (+ optional master join)

Executive → GET /api/dashboard/executive
        ↓ includes portfolio summary from BTRPD_CustomerPortfolioKpi
```

**Design decision:** Extend the **existing Customer snapshot refresh** after M30. Portfolio aggregator receives in-memory contexts from M17 result, M29 forecast contexts, and M30 optimization contexts — guarantees composition integrity (CPO-104, CPO-105).

**Alternatives rejected:**

| Alternative | Reason rejected |
| ----------- | --------------- |
| Separate Portfolio worker domain | Duplicates Customer-domain loads; temporal skew vs M29/M30 |
| API-time composition of M17+M29+M30 snapshots | Violates materialized dashboard pattern; slower reads |
| Recalculate risk/collection inside M31 | Violates PO-approved composition constraint |
| Tier from Klasifikasi master | Explicitly forbidden (CPO-31) |

### 3.2 Layering

```text
btr.portal.api          → CustomerPortfolioDashboardController, CustomerReportController (thin)
btr.application         → GetDashboardCustomerPortfolioQuery + Handler
                          → GetCustomerReportQuery + Handler
                          → DashboardCustomerPortfolioAggregator
                          → CustomerPortfolioOptimizationPolicy
                          → CustomerPortfolioLifecycleResolver
                          → CustomerPortfolioTierResolver
                          → CustomerPortfolioActionBuilder
                          → CustomerPortfolioExecutiveSummaryBuilder
                          → IDashboardCustomerPortfolioDal, ICustomerReportDal (contracts)
btr.infrastructure      → DashboardCustomerPortfolioDal, CustomerReportDal
                          → DashboardCustomerSnapshotDal (extended write)
                          → CustomerFirstFakturDal, CustomerPurchaseFrequencyDal
btr.portal.web          → CustomerPortfolioDashboardView.vue + components
                          → CustomerReportView.vue
                          → ExecutivePortfolioSummarySection.vue (M16)
```

MediatR pattern preserved. No business logic in controllers.

### 3.3 Refactor note — M30 aggregator return shape

**Minimal change to M30:** Extend `DashboardCollectionOptimizationAggregator.Aggregate()` to expose `IReadOnlyDictionary<string, CollectionOptimizationContext>` (keyed by `CustomerKey`) alongside existing aggregate result — same pattern as M29 returning `Contexts`.

If M30 already builds `processed` list internally, return it as `ContextsByKey` without changing materialized M30 tables or API.

---

## 4. Policy and Builder Design

### 4.1 `CustomerPortfolioOptimizationPolicy`

**File:** `btr.application/ReportingContext/DashboardSnapshotAgg/Services/CustomerPortfolioOptimizationPolicy.cs`

Central constants and scoring. All thresholds configurable via `CustomerPortfolioOptions` nested in `DashboardSnapshotOptions`.

```csharp
public static class CustomerPortfolioOptimizationPolicy
{
    // Lifecycle
    public const int NewCustomerDaysThreshold = 90;           // aligns with M17 dormant horizon
    public const int PurchaseFrequencyLookbackMonths = 6;

    // Tier — omzet/piutang rank bands (reuse M17 TopCustomerCount = 10, M14/M29 top 20)
    public const int StrategicOmzetRankMax = 10;
    public const int StrategicPiutangRankMax = 10;
    public const int HighValueOmzetRankMax = 20;
    public const int HighValuePiutangRankMax = 20;
    public const decimal StrategicOpenBalanceFloorIdr = 10_000_000m;
    public const decimal HighValueMtdOmzetFloorIdr = 5_000_000m;
    public const int HighFrequencyFakturCountMin = 4;         // in 6-month lookback

    // Actions
    public const string ActionGrow = "Grow";
    public const string ActionRetain = "Retain";
    public const string ActionProtect = "Protect";
    public const string ActionCollect = "Collect";
    public const string ActionReviewCredit = "ReviewCredit";
    public const string ActionRecover = "Recover";
    public const string ActionMonitor = "Monitor";
    public const string ActionExitReview = "ExitReview";

    // Attention
    public static bool QualifiesForAttention(PortfolioCustomerContext ctx);
    public static int ComputePortfolioPriorityScore(PortfolioCustomerContext ctx);
    public static string ResolveActionOwner(string actionKey);
}
```

### 4.2 `CustomerPortfolioLifecycleResolver`

**File:** `CustomerPortfolioLifecycleResolver.cs`

Computed lifecycle — **not stored on master**. Precedence order (first match wins):

| Order | Stage | Rule |
| ----- | ----- | ---- |
| 1 | `NeverPurchased` | Customer in master with resolvable key; **no** row in first/last Faktur historical set |
| 2 | `Dormant` | M17 dormant rule — `DaysSinceLastFaktur >= 90`, prior history, not active MTD (**authoritative** — do not redefine) |
| 3 | `New` | `FirstFakturDate >= today - 90 days` |
| 4 | `Declining` | M29 signal `ModerateDecline`, `SevereDecline`, or `StoppedAfterHistory` present |
| 5 | `Growing` | Prior month omzet > 0 AND projected/MTD omzet ratio ≥ 1.0 AND M29 category ≤ `Watch` AND not `New` |
| 6 | `Mature` | Default for customers with purchase history who are not Dormant/Declining/Growing/New |

**Inputs (composed, not recalculated):**

- `FirstFakturDate` — new DAL
- `LastFakturDate`, `DaysSinceLastFaktur` — existing last Faktur DAL
- `IsActiveMtd` — from M17 active set (passed in-memory)
- M29 decline signals — from `CustomerRiskForecastContext.Signals`

**Note:** M17 **Active** (invoiced MTD) remains a separate flag on portfolio rows — not conflated with lifecycle stage.

### 4.3 `CustomerPortfolioTierResolver`

**File:** `CustomerPortfolioTierResolver.cs`

Computed tier — **never uses Klasifikasi**. Precedence (highest tier wins):

| Tier | Qualification (any) |
| ---- | ------------------- |
| **Strategic** | MTD omzet rank ≤ 10 OR open piutang rank ≤ 10 OR (M29 category ≥ `Attention` AND open balance ≥ Rp 10M) |
| **High Value** | MTD omzet rank ≤ 20 OR open piutang rank ≤ 20 OR MTD omzet ≥ Rp 5M OR purchase frequency ≥ 4 Fakturs in 6 months |
| **Medium Value** | MTD omzet > 0 OR open balance > 1 OR purchase frequency ≥ 2 in 6 months |
| **Low Value** | Default |

**Purchase frequency formula:**

```text
FakturCount6Mo = COUNT(non-void Faktur) per customer in [today - 6 months, today]
```

Implement via `ICustomerPurchaseFrequencyDal.ListFakturCountByCustomer(from, to)`.

Rankings computed once in aggregator from MTD Faktur load and open balance load — same sources as M17.

### 4.4 Portfolio action resolution

**File:** `CustomerPortfolioActionBuilder.cs`

Exactly **one primary action** per customer (CPO-40). Precedence (first qualifying action wins):

| Order | Action | Qualification |
| ----- | ------ | ------------- |
| 1 | **Collect** | M30 `CollectionOptimizationContext` exists AND M30 action is actionable (not `NoActionToday` / `DeferCollection`) |
| 2 | **Review Credit** | M30 action category `CreditReview` OR M29 recommendation key in credit-review set OR M17 `PlafondBreach` signal |
| 3 | **Exit Review** | Tier = `Low Value` AND M29 category ≥ `HighRisk` |
| 4 | **Protect** | Tier = `Strategic` AND M29 category ≥ `Watch` |
| 5 | **Retain** | Tier ∈ {Strategic, High Value} AND lifecycle = `Declining` |
| 6 | **Recover** | Lifecycle = `Dormant` OR M17 `Dormant` attention signal |
| 7 | **Grow** | Lifecycle ∈ {New, Growing} AND M29 category ≤ `Watch` |
| 8 | **Monitor** | Default for attention-qualified customers without higher action |
| — | *(non-attention)* | Customers not qualifying for attention filter may still receive `Monitor` when in All Customers view |

**Collect behavior (CPO-41):** M31 stores `Collect` as action key with `M30LinkRoute = "/dashboard/collection-optimization?customerKey={key}"` — **does not** copy M30 priority score or queue row.

**Reason text:** Builder produces human-readable `ActionReasonText` citing lifecycle, tier, M29 category, and triggered rule ids (CPO-43).

### 4.5 Attention default filter

**File:** `CustomerPortfolioOptimizationPolicy.QualifiesForAttention`

Customer qualifies when **any**:

| # | Condition |
| - | --------- |
| A | M17 attention signal (Overdue, Dormant, PlafondBreach, SuspendedWithSales) |
| B | M29 risk category > `Healthy` |
| C | Primary portfolio action ≠ `Monitor` |
| D | Lifecycle ∈ {Declining, Dormant, NeverPurchased} |
| E | Tier = Strategic AND M29 category ≥ Watch |

Store `IsAttention = true/false` on each materialized customer row. KPI `AttentionCustomerCount` must equal count of rows where `IsAttention = true` (CPO-52).

### 4.6 Portfolio priority score

Distinct from M30 `CollectionPriorityScore` (CPO-63).

```text
PortfolioPriorityScore =
    ActionWeight(action)           // Collect=900, ReviewCredit=850, ExitReview=800, Protect=700,
                                   // Retain=600, Recover=500, Grow=400, Monitor=100
  + TierWeight(tier)               // Strategic=300, High=200, Medium=100, Low=0
  + M29 CategoryWeight             // reuse CustomerRiskForecastPolicy category weights
  + ImpactComponent              // min(openBalance, mtdOmzet) / 1_000_000 (integer truncation, cap 200)
```

**Tie-break:** open balance DESC → MTD omzet DESC → customer name ASC.

Materialize top **50** attention customers in priority queue table; full customer set in `BTRPD_CustomerPortfolioCustomer` for All Customers view and Customer Report.

### 4.7 `CustomerPortfolioExecutiveSummaryBuilder`

Plain-language brief from KPI snapshot:

- Portfolio health % (from M29 `PortfolioHealthScore`)
- Attention customer count and dominant action mix
- Strategic-at-risk count
- Working capital tied (sum open balance for attention customers)
- Value disclaimer: *Customer Value = Omzet Proxy, NOT profitability*

### 4.8 In-memory `PortfolioCustomerContext`

```csharp
public sealed class PortfolioCustomerContext
{
    public string CustomerKey { get; set; }
    public string CustomerCode { get; set; }
    public string CustomerName { get; set; }
    public string WilayahName { get; set; }
    public string Klasifikasi { get; set; }          // display/filter only
    public string LifecycleStage { get; set; }
    public string PortfolioTier { get; set; }
    public string PrimaryActionKey { get; set; }
    public string ActionOwner { get; set; }
    public string ActionReasonText { get; set; }
    public string TriggeredRuleIds { get; set; }
    public bool IsAttention { get; set; }
    public int PortfolioPriorityScore { get; set; }
    public decimal MtdOmzet { get; set; }
    public decimal OpenBalance { get; set; }
    public decimal? OverdueBalance { get; set; }
    public DateTime? LastPurchaseDate { get; set; }
    public DateTime? FirstPurchaseDate { get; set; }
    public int FakturCount6Mo { get; set; }
    public bool IsActiveMtd { get; set; }
    public string M29Category { get; set; }
    public string SalesPersonName { get; set; }      // last invoicing salesman
    public decimal? SalesmanAchievementPercent { get; set; }
    public bool SalesmanHighPiutangExposure { get; set; }
    public string M30LinkRoute { get; set; }         // when Collect
    public string CustomerReportRoute { get; set; }
    public string ValueDisclaimer { get; set; }      // constant proxy text
}
```

---

## 5. Aggregator Design

### 5.1 `DashboardCustomerPortfolioAggregator`

**Method signature (indicative):**

```csharp
public DashboardCustomerPortfolioAggregateResult Aggregate(
    DashboardCustomerAggregateResult customerAggregate,
    IReadOnlyList<CustomerRiskForecastContext> forecastContexts,
    DashboardCustomerRiskForecastAggregateResult forecastAggregate,
    IReadOnlyDictionary<string, CollectionOptimizationContext> optimizationByKey,
    DashboardSalesmanAggregateResult salesmanSnapshot,
    IEnumerable<CustomerModel> customers,
    IEnumerable<CustomerLastFakturWithSalesmanDto> lastFakturWithSalesman,
    IEnumerable<CustomerFirstFakturDto> firstFakturRows,
    IEnumerable<CustomerPurchaseFrequencyDto> frequencyRows,
    IEnumerable<FakturView> currentMonthFakturRows,
    IEnumerable<PiutangOpenBalanceDto> piutangRows,
    DateTime businessDate,
    DateTime generatedAt,
    CustomerPortfolioOptions options);
```

### 5.2 Processing steps

```text
1. Build master customer universe — all CustomerModel with resolvable DashboardCustomerKeyResolver key
2. Build lookup maps: first faktur, last faktur+salesman, frequency, M17 attention keys, forecast by key, M30 opt by key
3. Build MTD omzet rank and open balance rank maps (for tier)
4. Build M18 salesman achievement/exposure lookup by SalesPersonName (normalized)
5. For each customer in universe:
   a. Compose PortfolioCustomerContext from upstream contexts
   b. CustomerPortfolioLifecycleResolver.Resolve
   c. CustomerPortfolioTierResolver.Resolve
   d. CustomerPortfolioActionBuilder.ResolvePrimaryAction
   e. CustomerPortfolioOptimizationPolicy.QualifiesForAttention
   f. CustomerPortfolioOptimizationPolicy.ComputePortfolioPriorityScore
6. Aggregate KPIs (health score from M29, attention count, strategic at risk, working capital, tier/lifecycle/action distributions)
7. Materialize:
   - All customer rows → BTRPD_CustomerPortfolioCustomer
   - Top 50 attention priority → BTRPD_CustomerPortfolioPriority
   - Distribution tables (lifecycle, tier, action)
   - Concentration (reuse M17 top omzet/piutang from customerAggregate — copy references, do not recompute rankings)
   - Optional wilayah breakdown from M22 or M17 segmentation
8. CustomerPortfolioExecutiveSummaryBuilder.Build
9. Return aggregate result
```

### 5.3 M18 cross-read usage

| M18 source | M31 use |
| ---------- | ------- |
| `RepRankings` (or equivalent rep list with `SalesPersonName`, `AchievementPercent`) | Row field `SalesmanAchievementPercent` matched via last invoicing salesman |
| M18 attention / signal keys for `HighPiutangExposure` per rep | `SalesmanHighPiutangExposure` flag on portfolio row when owner rep flagged |

When `salesmanSnapshot` is null: leave achievement null; exposure flag false.

### 5.4 M17/M29/M30 consumption map

| Upstream | M31 field / logic |
| -------- | ----------------- |
| M17 `AttentionList.SignalKey` | Attention qualification; reason text |
| M17 `DormantDaysThreshold` | Lifecycle Dormant — pass `dormantSet` or re-use keys from aggregate internals via shared helper **without second 90d computation** — prefer passing `HashSet<string> dormantKeys` built by M17 aggregator refactor OR derive from same lastFaktur+active inputs in portfolio step using M17's public threshold constant only |
| M29 `CustomerRiskForecastContext.Category` | Tier, action, attention |
| M29 `PortfolioHealthScore` | Headline KPI |
| M30 `CollectionOptimizationContext.ActionCategoryKey` | Collect / Review Credit resolution |
| M30 actionable filter | Collect action gate |

**Important:** For dormant detection, call shared logic with M17 inputs already loaded in worker — use `DashboardCustomerAggregator.DormantDaysThreshold` constant and the same `lastFakturRows` + `activeSet` inputs. Do **not** duplicate plafond, aging, or forecast signal evaluation.

---

## 6. Materialized Data Impact

### 6.1 New tables

| Table | Layer | Content |
| ----- | ----- | ------- |
| `BTRPD_CustomerPortfolioKpi` | A | Headline KPIs, executive summary, health score, counts |
| `BTRPD_CustomerPortfolioLifecycleDist` | B | Lifecycle stage distribution |
| `BTRPD_CustomerPortfolioTierDist` | B | Tier distribution |
| `BTRPD_CustomerPortfolioActionDist` | B | Action distribution |
| `BTRPD_CustomerPortfolioPriority` | B | Top 50 attention customers by portfolio priority |
| `BTRPD_CustomerPortfolioCustomer` | B | **All** portfolio customers — full row for report + All Customers view |
| `BTRPD_CustomerPortfolioConcentration` | B | Top 10 omzet + top 10 piutang (copied from M17 aggregate at refresh) |
| `BTRPD_CustomerPortfolioWilayah` | B | Optional — customer count / attention count by Wilayah (top 15) |

**Snapshot key:** `CURRENT` — delete-and-replace each refresh.

### 6.2 KPI table columns (indicative)

`BTRPD_CustomerPortfolioKpi`:

- `SnapshotKey`, `GeneratedAt`, `BusinessDate`
- `PortfolioHealthScore`, `PortfolioHealthyPercent`
- `TotalCustomerCount`, `AttentionCustomerCount`
- `StrategicCustomerCount`, `StrategicAtRiskCount`, `CustomersAtRiskCount`
- `WorkingCapitalTiedAmount`, `TotalMtdOmzet`, `TotalOpenBalance`
- `NeverPurchasedCount`, `DormantCount`, `DecliningCount`
- `ExecutiveSummaryText`, `ValueDisclaimerText`
- `LastRefreshLogId`

### 6.3 Customer row columns (indicative)

`BTRPD_CustomerPortfolioCustomer`:

- Identity: `CustomerKey`, `CustomerCode`, `CustomerName`, `WilayahName`, `Klasifikasi`
- Classification: `LifecycleStage`, `LifecycleLabel`, `PortfolioTier`, `TierLabel`
- Action: `PrimaryActionKey`, `PrimaryActionLabel`, `ActionOwner`, `ActionReasonText`, `TriggeredRuleIds`
- Metrics: `MtdOmzet`, `OpenBalance`, `OverdueBalance`, `FakturCount6Mo`, `IsActiveMtd`
- Dates: `LastPurchaseDate`, `FirstPurchaseDate`
- Risk: `M29Category`, `M29PrimarySignalKey`
- Salesman summary: `SalesPersonName`, `SalesmanAchievementPercent`, `SalesmanHighPiutangExposure`
- Portfolio: `IsAttention`, `PortfolioPriorityScore`
- Navigation: `M30LinkRoute`, `CustomerReportRoute`, `DrillDownRouteM17`, `DrillDownRouteM29`
- `SortOrder` (for default attention sort)

### 6.4 SQL scripts

- Add `CREATE TABLE` definitions under `btr.sql/Tables/ReportingContext/BTRPD_CustomerPortfolio*.sql`
- Append to `btr.sql/Scripts/Create_BTRPD_PortalDashboard_Tables.sql`
- Upgrade script `Upgrade_M31_CustomerPortfolioOptimization.sql`

### 6.5 Existing tables

**No changes** to M17, M29, M30, M18, M22 snapshot tables.

---

## 7. Snapshot Refresh Strategy

### 7.1 Worker extension

Extend `RefreshDashboardCustomerSnapshotWorker`:

```csharp
// After existing loads, add:
var lastFakturWithSalesman = _lastFakturDal.ListLastFakturWithSalesmanByCustomer()?.ToList();
var firstFakturRows = _firstFakturDal.ListFirstFakturByCustomer()?.ToList();
var frequencyRows = _purchaseFrequencyDal.ListFakturCountByCustomer(frequencyFrom, today)?.ToList();
var salesmanSnapshot = _salesmanSnapshotDal.GetCurrent();
var locationSnapshot = _locationSnapshotDal.GetCurrent(); // optional

// Existing steps 1-3 unchanged...

var portfolioAggregate = _portfolioAggregator.Aggregate(
    aggregate,
    forecastAggregate.Contexts,
    forecastAggregate,
    optimizationAggregate.ContextsByKey,
    salesmanSnapshot,
    customers,
    lastFakturWithSalesman,
    firstFakturRows,
    frequencyRows,
    fakturRows,
    piutangRows,
    today,
    generatedAt,
    portfolioOptions);

_snapshotDal.ReplaceCurrent(aggregate, forecastAggregate, optimizationAggregate, portfolioAggregate, refreshLogId);
```

| Aspect | Choice |
| ------ | ------ |
| Worker | `RefreshDashboardCustomerSnapshotWorker` extended |
| Cadence | 30 minutes (`CustomerIntervalMinutes`) |
| Transaction | Single `ReplaceCurrent` — M17 + M29 + M30 + M31 |
| Manual refresh | `POST /api/admin/dashboard/refresh?domain=Customer` |
| M18 cross-read | `IDashboardSalesmanSnapshotDal.GetCurrent()` |
| M22 cross-read | Optional — `IDashboardLocationSnapshotDal.GetCurrent()` |

### 7.2 `IDashboardCustomerSnapshotDal` extension

Add overload:

```csharp
void ReplaceCurrent(
    DashboardCustomerAggregateResult result,
    DashboardCustomerRiskForecastAggregateResult forecast,
    DashboardCollectionOptimizationAggregateResult optimization,
    DashboardCustomerPortfolioAggregateResult portfolio,
    string refreshLogId);
```

Chain existing overloads for backward compatibility.

---

## 8. New DAL Loads

### 8.1 `ICustomerFirstFakturDal`

**File:** `btr.application/SalesContext/FakturInfo/ICustomerFirstFakturDal.cs`

```csharp
public class CustomerFirstFakturDto
{
    public string CustomerCode { get; set; }
    public string CustomerName { get; set; }
    public DateTime FirstFakturDate { get; set; }
}

public interface ICustomerFirstFakturDal
{
    IEnumerable<CustomerFirstFakturDto> ListFirstFakturByCustomer();
}
```

**SQL pattern:** `MIN(FakturDate)` grouped by customer from non-void Faktur — mirror `CustomerLastFakturDal` structure.

### 8.2 `ICustomerPurchaseFrequencyDal`

```csharp
public class CustomerPurchaseFrequencyDto
{
    public string CustomerCode { get; set; }
    public string CustomerName { get; set; }
    public int FakturCount { get; set; }
}

public interface ICustomerPurchaseFrequencyDal
{
    IEnumerable<CustomerPurchaseFrequencyDto> ListFakturCountByCustomer(DateTime from, DateTime to);
}
```

Register in DI alongside existing FakturInfo DALs.

---

## 9. API Design

### 9.1 Dashboard endpoint

```
GET /api/dashboard/customer-portfolio
```

**Query parameters (optional — client may also filter locally):**

| Param | Values | Default |
| ----- | ------ | ------- |
| `view` | `attention`, `all` | `attention` |

**Auth:** Same as other dashboard endpoints.

**Response:** `DashboardCustomerPortfolioResponse`

```csharp
public class DashboardCustomerPortfolioResponse
{
    public bool IsAvailable { get; set; }
    public DashboardCustomerPortfolioKpiDto Kpi { get; set; }
    public IReadOnlyList<DistributionDto> LifecycleDistribution { get; set; }
    public IReadOnlyList<DistributionDto> TierDistribution { get; set; }
    public IReadOnlyList<DistributionDto> ActionDistribution { get; set; }
    public IReadOnlyList<DashboardCustomerPortfolioPriorityDto> PriorityQueue { get; set; }
    public IReadOnlyList<DashboardCustomerPortfolioCustomerDto> Customers { get; set; }
    public IReadOnlyList<ConcentrationRowDto> TopOmzet { get; set; }
    public IReadOnlyList<ConcentrationRowDto> TopPiutang { get; set; }
    public IReadOnlyList<WilayahBreakdownDto> WilayahBreakdown { get; set; }
    public DateTime GeneratedAt { get; set; }
}
```

When `view=attention`, handler may filter `Customers` to `IsAttention=true` or return all with flag for client filter — **prefer returning all with `IsAttention` flag** so KPI reconciliation stays server-side consistent.

### 9.2 Application query

**Folder:** `btr.application/ReportingContext/DashboardCustomerPortfolioAgg/`

- `Queries/GetDashboardCustomerPortfolioQuery.cs`
- Handler reads `IDashboardCustomerPortfolioDal.GetCurrent()` — no business logic in handler

### 9.3 Controller

**File:** `btr.portal.api/Controllers/Dashboard/CustomerPortfolioDashboardController.cs`

Thin — delegates to MediatR only.

---

## 10. Customer Report

### 10.1 Endpoint

```
GET /api/reports/customers
GET /api/reports/customers?customerCode={code}
```

### 10.2 Design

**Primary source:** `BTRPD_CustomerPortfolioCustomer` snapshot (materialized during M31 refresh) — ensures lifecycle, tier, and action match dashboard exactly (acceptance criterion #3).

**Grain:** One row per customer.

**Columns:**

| Column | Source |
| ------ | ------ |
| CustomerCode, CustomerName | Snapshot |
| WilayahName, Klasifikasi | Snapshot |
| MtdOmzet, OpenBalance, OverdueBalance | Snapshot |
| LastPurchaseDate, FirstPurchaseDate | Snapshot |
| LifecycleStage, PortfolioTier | Snapshot |
| PrimaryActionKey, ActionOwner, ActionReasonText | Snapshot |
| SalesPersonName, SalesmanAchievementPercent | Snapshot |
| M29Category | Snapshot |
| ValueDisclaimer | KPI constant text |

**Filtering:** Optional `customerCode` query param pre-filters for investigation drill-down from M31 row (CPO-91).

### 10.3 Application layer

**Folder:** `btr.application/ReportingContext/CustomerReportAgg/`

- `Queries/GetCustomerReportQuery.cs`
- `ICustomerReportDal` → reads portfolio customer table (+ generated-at from KPI)

### 10.4 Investigation registry

Extend `InvestigationRegistry`:

```csharp
public const string CustomerReportRoute = "/reports/customers";
public const string CustomerPortfolioDashboardRoute = "/dashboard/customer-portfolio";
```

Add portfolio action keys with investigation steps:

```text
Portfolio → Customer Report → Sales Report / Piutang Report → Desktop
```

---

## 11. Executive Dashboard Integration

### 11.1 M16 portfolio summary cards

Extend `DashboardExecutiveComposer` and `DashboardExecutiveResponse`:

```csharp
public class DashboardExecutivePortfolioAttention
{
    public bool IsAvailable { get; set; }
    public decimal? PortfolioHealthyPercent { get; set; }
    public int CustomersAtRiskCount { get; set; }
    public int StrategicCustomersAtRiskCount { get; set; }
    public string DashboardRoute { get; set; }  // "/dashboard/customer-portfolio"
}
```

Add `Portfolio` property to `DashboardExecutiveResponse`.

**Composer input:** Load `BTRPD_CustomerPortfolioKpi` via new `IDashboardCustomerPortfolioDal.GetCurrentKpi()` or include in existing portfolio DAL.

**UI:** `ExecutivePortfolioSummarySection.vue` — three summary cards, link to M31. **No detailed tables** on executive page (CPO-80).

### 11.2 `GetDashboardExecutiveQuery` handler

Extend handler to read portfolio KPI snapshot alongside existing domain snapshots.

---

## 12. Portal Web Implementation

### 12.1 Routes and navigation

| Item | Value |
| ---- | ----- |
| Dashboard route | `/dashboard/customer-portfolio` |
| Report route | `/reports/customers` |
| Sidebar label | Customer Portfolio |
| Position | After Collection Optimization, before Salesman Performance |
| Router | Lazy import in `src/router/index.ts` |

### 12.2 Dashboard view structure

**File:** `CustomerPortfolioDashboardView.vue`

Reuse patterns from `CollectionOptimizationDashboardView.vue` and `CustomerRiskForecastDashboardView.vue`:

| Component | Purpose |
| --------- | ------- |
| `DashboardDetailLayout` | Shell, refresh, generated-at |
| `CustomerPortfolioSummary` | Executive summary + value disclaimer |
| `CustomerPortfolioKpiGrid` | Health, strategic at risk, working capital, attention count |
| `CustomerPortfolioLifecycleChart` | Lifecycle distribution donut/bar |
| `CustomerPortfolioTierChart` | Tier distribution |
| `CustomerPortfolioConcentrationChart` | Omzet vs piutang concentration |
| `CustomerPortfolioFilterBar` | View toggle, Wilayah, Klasifikasi, Tier, Lifecycle, Action, Salesman |
| `CustomerPortfolioPriorityTable` | Default attention queue with expand detail |
| `CustomerPortfolioActionSegments` | Expandable lists by action |
| `CustomerPortfolioConcentrationTables` | Top 10 omzet/piutang |
| `CustomerPortfolioDetailDrawer` | Optional row detail with evidence links |

### 12.3 Customer Report view

**File:** `CustomerReportView.vue`

- Reuse report layout pattern from `SalesReportView.vue` / `PiutangReportView.vue`
- Client-side search by customer name/code
- Pre-filter from route query `customerCode`
- Footer: total customers, total MTD omzet, total open balance
- Value disclaimer banner

### 12.4 Store and API

| File | Change |
| ---- | ------ |
| `src/api/dashboardApi.ts` | `getCustomerPortfolio()` |
| `src/api/reportApi.ts` (or equivalent) | `getCustomerReport()` |
| `src/models/dashboard.ts` | Portfolio response interfaces |
| `src/models/reports.ts` | Customer report interfaces |
| `src/stores/dashboardStore.ts` | `loadCustomerPortfolio()` |
| `src/stores/customerReportStore.ts` | **New** |

### 12.5 Action badge colors

| Action | Token |
| ------ | ----- |
| Collect | `severity-critical` |
| ReviewCredit | `severity-warning` |
| ExitReview | `severity-critical` |
| Protect | `severity-warning` |
| Retain | `severity-info` |
| Recover | `severity-info` |
| Grow | `severity-success` |
| Monitor | neutral |

### 12.6 Client helper

**File:** `src/services/customerPortfolioSignals.ts`

- Lifecycle label map
- Tier label map
- Action label + owner map
- Collect → M30 link builder

### 12.7 Traceability footer

Links:

- `/dashboard/customers` — M17 current state
- `/dashboard/customer-risk-forecast` — M29 forward risk
- `/dashboard/collection-optimization` — M30 collection operations
- `/reports/customers` — Customer Report
- `/reports/sales`, `/reports/piutang` — transaction evidence

Disclaimer:

> *Portfolio recommendations are read-only management guidance based on deterministic business rules. Customer Value = Omzet Proxy, NOT profitability. BTR Portal does not modify credit limits, suspend accounts, or initiate customer contact.*

---

## 13. Configuration

Extend `DashboardSnapshotOptions`:

```json
"DashboardSnapshot": {
  "CustomerPortfolioNewCustomerDaysThreshold": 90,
  "CustomerPortfolioPurchaseFrequencyLookbackMonths": 6,
  "CustomerPortfolioHighFrequencyFakturCountMin": 4,
  "CustomerPortfolioStrategicOpenBalanceFloorIdr": 10000000,
  "CustomerPortfolioHighValueMtdOmzetFloorIdr": 5000000,
  "CustomerPortfolioMaxPriorityRows": 50,
  "CustomerPortfolioMaxWilayahRows": 15
}
```

Add nested class `CustomerPortfolioOptions` with `FromDashboardOptions` factory — mirror `CollectionOptimizationOptions` pattern.

---

## 14. Testing Strategy

### 14.1 Unit tests

**File:** `CustomerPortfolioLifecycleResolverTest.cs`

| Test case | Validates |
| --------- | --------- |
| Never purchased — no faktur history | Lifecycle `NeverPurchased` |
| Dormant — 90d rule | Uses M17 threshold |
| New — first purchase 30 days ago | Lifecycle `New` |
| Declining — M29 severe decline signal | Lifecycle `Declining` |
| Growing — ratio ≥ 1.0, low risk | Lifecycle `Growing` |
| Precedence — dormant beats declining | Order |

**File:** `CustomerPortfolioTierResolverTest.cs`

| Test case | Validates |
| --------- | --------- |
| Strategic — top 10 omzet rank | Tier `Strategic` |
| High value — frequency ≥ 4 | Tier `High Value` |
| Klasifikasi ignored | Same tier regardless of klasifikasi |
| Low value default | Minimal omzet/balance |

**File:** `CustomerPortfolioOptimizationPolicyTest.cs`

| Test case | Validates |
| --------- | --------- |
| Collect precedence — M30 actionable | Action `Collect` |
| Review credit — plafond breach without M30 collect | Action `ReviewCredit` |
| Protect — strategic + watch | Action `Protect` |
| Exit review — low tier + critical | Action `ExitReview` |
| Attention qualification matrix | CPO-50 rules |
| Portfolio priority ordering | Score + tie-break |

**File:** `CustomerPortfolioActionBuilderTest.cs`

| Test case | Validates |
| --------- | --------- |
| Reason text includes tier + lifecycle | CPO-43 |
| M30 link only for Collect | CPO-41 |
| TriggeredRuleIds traceability | |

**File:** `DashboardCustomerPortfolioAggregatorTest.cs`

| Test case | Validates |
| --------- | --------- |
| End-to-end synthetic portfolio | KPI counts match distributions |
| All customers materialized | Includes never purchased |
| M18 null graceful degrade | |
| No M29 signal re-evaluation | Contexts passed through |
| Attention count reconciliation | CPO-52 |

### 14.2 Integration tests

**File:** `DashboardCustomerSnapshotVerificationTest.cs` (extend)

- Round-trip portfolio tables after refresh
- M17/M29/M30 tables unchanged when portfolio added

### 14.3 Frontend tests

**File:** `customerPortfolioSignals.spec.ts`

- Action/lifecycle/tier label mapping
- M30 link builder for Collect
- Default attention view filter

---

## 15. Implementation Phases

### Phase 1 — Foundation (2–3 days)

1. SQL tables + `Upgrade_M31_CustomerPortfolioOptimization.sql`
2. `ICustomerFirstFakturDal` + `ICustomerPurchaseFrequencyDal` + infrastructure
3. Portfolio models + policy/resolver/builder classes
4. Unit tests for lifecycle, tier, policy

### Phase 2 — Aggregation (2–3 days)

1. `DashboardCustomerPortfolioAggregator`
2. M30 context-by-key return shape (minimal refactor)
3. Extend `RefreshDashboardCustomerSnapshotWorker`
4. Extend `DashboardCustomerSnapshotDal.ReplaceCurrent`
5. Aggregator unit tests

### Phase 3 — API (1–2 days)

1. `IDashboardCustomerPortfolioDal` + infrastructure read
2. `GetDashboardCustomerPortfolioQuery` + controller
3. `ICustomerReportDal` + `GetCustomerReportQuery` + controller
4. Extend executive query + composer

### Phase 4 — Portal Web (3–4 days)

1. Dashboard view + components
2. Customer Report view
3. Executive portfolio summary section
4. Router, sidebar, store, API client
5. Investigation registry entries
6. Frontend unit tests

### Phase 5 — Verification (1–2 days)

1. Integration snapshot verification test
2. Manual acceptance against criteria (§16)
3. Update feature artifact status + portal domain docs

**Estimated total:** 9–14 days

---

## 16. Acceptance Criteria Mapping

| # | Criterion | Verification |
| - | --------- | -------------- |
| 1 | One primary action per attention row | Priority/Customer tables + UI |
| 2 | Default Attention view; All Customers toggle | Filter bar + `IsAttention` flag |
| 3 | M29/M17 signals match upstream | Same refresh timestamp; spot-check customer keys |
| 4 | Collect links to M30; no queue duplication | `M30LinkRoute` only; no M30 rows on M31 |
| 5 | Omzet value labeled as proxy | KPI disclaimer + row `ValueDisclaimer` |
| 6 | Lifecycle + tier visible; Klasifikasi filter only | Row columns; tier resolver tests |
| 7 | Salesman summary on rows | M18 cross-read fields |
| 8 | M16 summary cards only | Executive section — no tables |
| 9 | Investigation chain to Customer Report | Registry + query param pre-filter |
| 10 | Read-only | No write endpoints |

---

## 17. Documentation Updates (Implementer)

After implementation:

1. Set [feature.md](../../features/customer-portfolio-optimization/feature.md) status to **Current**
2. Add M31 entry to [btr-portal-domain.md](../../features/btr-portal/btr-portal-domain.md) — route, lifecycle, tier, actions
3. Add operational notes to [btr-portal-operational.md](../../features/btr-portal/btr-portal-operational.md)
4. Record `implementation-summary-m31-customer-portfolio-optimization.md` under `docs/work/btr-portal/`

---

## 18. Architecture Decisions Summary

| Decision | Rationale |
| -------- | --------- |
| Composition after M30 in Customer worker | Same pattern as M30 after M29; single transaction; no temporal skew |
| Materialize all customers | Supports All Customers view and Customer Report without API-time aggregation |
| New first-faktur + frequency DALs | Required inputs not in M17/M29/M30; minimal scoped queries |
| Tier from rank + frequency + risk | PO-approved computed tiers; explainable thresholds |
| Collect action links to M30 | Avoids duplicating collection queue (CPO-41) |
| Customer Report reads M31 snapshot | Guarantees consistency with dashboard |
| M16 summary cards only | PO decision Q15; prevents executive scope creep |
| IsSuspend fix as parallel prerequisite | Do not block M31; graceful degrade on suspend signal |

---

**Implementer should execute phases sequentially. Stop and escalate if upstream M29/M30 context return shapes require larger refactor than §3.3 describes.**
