# Implementation Plan — M30 Collection Optimization Dashboard

## Document Status

| Field | Value |
| ----- | ----- |
| Milestone | M30 — Collection Optimization Dashboard |
| Authoritative requirements | [portal-analysis-m30-collection-optimization.md](./portal-analysis-m30-collection-optimization.md) |
| Reference pattern | M28.5 Inventory Optimization, M29 Customer Risk Forecast, M20 Collection Dashboard, materialized-dashboard architecture |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |
| Status | **Ready for implementation** (pending Product Owner approval of analysis) |

**Prerequisite:** Product Owner approval of analysis document (action categories, priority rules, wireframe, KPI definitions, queue definitions).

**Prerequisite implementation:** M29 Customer Risk Forecast Dashboard complete ([implementation-summary-m29-customer-risk-forecast.md](./implementation-summary-m29-customer-risk-forecast.md) or equivalent).

---

## 1. Goal

Deliver a read-only **Collection Optimization Dashboard** at `/dashboard/collection-optimization` that converts M29 customer risk forecast outputs, current piutang position, and M20 collection context into **deterministic, explainable operational recommendations** — prioritized collection actions, proactive reminders, credit review, sales recovery, and workload planning — ranked by integer `CollectionPriorityScore`.

**In scope:**

- New collection optimization aggregation during existing **Customer** snapshot refresh (after M29 forecast step)
- In-memory consumption of `CustomerRiskForecastContext` list from M29 aggregator — no forecast recalculation
- Cross-read M20 collection snapshot at refresh for recovery KPIs and attention signal keys
- New snapshot tables for optimization KPIs, priority queue, specialized queues, workload, and impact opportunities
- New API endpoint `GET /api/dashboard/collection-optimization`
- New Portal Web dashboard page with executive summary, KPIs, charts, priority table, and specialized queues
- Unit tests for optimization policy, action builder, and aggregator

**Out of scope:**

- Changes to M29 `/api/dashboard/customer-risk-forecast` response shape or forecast rules
- Changes to M20/M17/M14 dashboard APIs
- Automatic collection actions, Desktop write-back, CRM
- AI/ML, optimization solvers, route planning
- Alert Center / Executive integration
- Promise-to-pay tracking
- Historical snapshot retention

---

## 2. Impact Analysis

### 2.1 Business areas affected

| Area | Impact |
| ---- | ------ |
| Finance (Piutang / Collection) | Primary — daily operational collection workspace |
| Sales | Read-only — sales recovery queue and visit recommendations |
| Customer master | Read-only — Plafond, Wilayah, Klasifikasi |
| Credit control | Read-only — credit review queue |

### 2.2 Systems affected

| System | Impact |
| ------ | ------ |
| BTR Portal API | New controller + MediatR query |
| BTR Portal Web | New route, view, components, store method |
| BTR Portal Worker | Extended Customer snapshot refresh |
| BTR SQL | New snapshot tables |
| BTR Desktop | None |

### 2.3 Existing features — preserve behavior

| Feature | Change |
| ------- | ------ |
| Customer Risk Forecast Dashboard | **None** — optimization reads same refresh context |
| Customer Analytics Dashboard | **None** |
| Collection / Piutang / Cash Flow Forecast | **None** |
| M29 forecast tables | **None** — optimization is additive |
| Customer refresh cadence | **Unchanged** (~30 min) |
| `GET /api/dashboard/customer-risk-forecast` | **Unchanged** |

### 2.4 Source code modules affected

| Layer | Module | Change type |
| ----- | ------ | ----------- |
| Application | `ReportingContext/DashboardSnapshotAgg/Services/` | **Add** `DashboardCollectionOptimizationAggregator`, `CollectionOptimizationPolicy`, `CollectionOptimizationActionBuilder`, `CollectionOptimizationExecutiveSummaryBuilder` |
| Application | `ReportingContext/DashboardSnapshotAgg/Models/` | **Add** collection optimization aggregate DTOs |
| Application | `ReportingContext/DashboardSnapshotAgg/UseCases/` | **Extend** `RefreshDashboardCustomerSnapshotWorker` |
| Application | `ReportingContext/DashboardSnapshotAgg/Contracts/` | **Extend** `IDashboardCustomerSnapshotDal`, **Add** `IDashboardCollectionSnapshotDal` read for cross-read (if not already injected) |
| Application | `ReportingContext/DashboardCollectionOptimizationAgg/` | **New** query + read contract |
| Infrastructure | `ReportingContext/DashboardSnapshotAgg/` | **Extend** snapshot DAL write/read |
| Infrastructure | `ReportingContext/DashboardCollectionOptimizationAgg/` | **Add** read facade |
| SQL | `btr.sql/Scripts/` | **Add** collection optimization tables |
| Portal API | `Controllers/Dashboard/` | **Add** `CollectionOptimizationDashboardController` |
| Portal Web | `views/dashboard/`, `components/dashboard/`, `stores/`, `api/`, `router/` | **Add** optimization page + components |
| Tests | `btr.test/ReportingContext/` | **Add** policy + builder + aggregator tests |

### 2.5 Risk summary

| Risk | Severity | Mitigation |
| ---- | -------- | ---------- |
| Duplicate M29 forecast logic | High | Pass `List<CustomerRiskForecastContext>` from M29 aggregator — single compute pass (COL-OPT-06) |
| Duplicate aging logic vs M14 | High | Reuse piutang metrics from M29 context build — no second aging pass |
| M20 snapshot temporal skew | Medium | Cross-read at refresh start; document staleness ≤ 30 min (COL-OPT-KPI-50) |
| Action category conflict (collection vs sales) | Medium | Document precedence in `CollectionOptimizationPolicy`; unit tests per COL-OPT-CAT |
| Recommendation volume overwhelms UI | Low | Cap materialized rows (30/15/10); client-side row expand for detail |
| M20 unavailable during Customer refresh | Low | Graceful degrade — recovery KPI null with confidence note; workload boost skipped |
| Sales recovery mis-routing high overdue | Medium | COL-OPT-CAT-01/03 take precedence over COL-OPT-CAT-06; test overdue floor |

---

## 3. Architecture Overview

### 3.1 Topology

```text
Task Scheduler (Customer domain, ~30 min)
        ↓
RefreshDashboardCustomerSnapshotWorker
        ↓
Load: IFakturViewDal.ListData(current month)
      ICustomerOmzetHistoryDal (current + prior month)
      ICustomerLastFakturDal.ListLastFakturByCustomer()
      IPiutangOpenBalanceDal.ListOpenBalances()
      ICustomerDal.ListData()
      ICustomerPelunasanSummaryDal.ListSummary(30d)
      ICustomerPaymentBehaviorDal.ListPaymentBehavior(90d)
      IDashboardCollectionSnapshotDal.GetCurrent()                    ← cross-read M20
        ↓
DashboardCustomerAggregator.Aggregate()                    → BTRPD_Customer* (unchanged)
        ↓
DashboardCustomerRiskForecastAggregator.Aggregate()        → BTRPD_CustomerRiskForecast* (unchanged)
        ↓  (returns in-memory contexts + aggregate result)
DashboardCollectionOptimizationAggregator.Aggregate()      → BTRPD_CollectionOptimization* (new)
        ↓
DashboardCustomerSnapshotDal.ReplaceCurrent()              → single transaction (M17 + M29 + M30)

Browser → GET /api/dashboard/collection-optimization
        ↓ MediatR
GetDashboardCollectionOptimizationHandler
        ↓ IDashboardCollectionOptimizationDal
DashboardCollectionOptimizationDal → snapshot SELECT
```

**Design decision:** Extend the **existing Customer snapshot refresh** after M29 forecast step. Optimization receives in-memory `CustomerRiskForecastContext` list from forecast aggregator — guarantees COL-OPT-06 (no duplicate forecast) and traceability to M29 rule outputs.

**Alternatives rejected:**

| Alternative | Reason rejected |
| ----------- | --------------- |
| Separate `CollectionOptimization` worker domain | Duplicates piutang/Faktur load; temporal skew vs M29 |
| Extend Collection worker only | M29 lives in Customer worker; would require cross-read all M29 tables |
| API-time composition of M29 + M20 | Violates materialized dashboard pattern; slower reads |
| Recalculate forecast inside M30 | Violates milestone scope; duplicates M29 |

### 3.2 Layering

```text
btr.portal.api          → CollectionOptimizationDashboardController (thin)
btr.application         → GetDashboardCollectionOptimizationQuery + Handler
                          → DashboardCollectionOptimizationAggregator
                          → CollectionOptimizationPolicy
                          → CollectionOptimizationActionBuilder
                          → CollectionOptimizationExecutiveSummaryBuilder
                          → IDashboardCollectionOptimizationDal (contract)
btr.infrastructure      → DashboardCollectionOptimizationDal (read facade)
                          → DashboardCustomerSnapshotDal (extended write)
btr.portal.web          → CollectionOptimizationDashboardView.vue + components
```

MediatR pattern preserved. No business logic in controller.

### 3.3 Refactor note — M29 aggregator return shape

**Minimal change to M29:** Extend `DashboardCustomerRiskForecastAggregator.Aggregate()` to return contexts alongside aggregate result, OR extract shared `BuildContexts(...)` method used by both M29 materialization and M30 optimization.

**Preferred approach (minimal impact):**

```csharp
public sealed class CustomerRiskForecastBuildResult
{
    public DashboardCustomerRiskForecastAggregateResult Aggregate { get; set; }
    public IReadOnlyList<CustomerRiskForecastContext> Contexts { get; set; }
}
```

M29 aggregator populates materialized rows from `Contexts` as today. M30 aggregator receives same list — no second customer loop for forecast signals.

---

## 4. Policy and Builder Design

### 4.1 `CollectionOptimizationPolicy`

**File:** `btr.application/ReportingContext/DashboardSnapshotAgg/Services/CollectionOptimizationPolicy.cs`

```csharp
public static class CollectionOptimizationPolicy
{
    public const decimal SalesRecoveryOverdueFloorIdr = 500_000m;
    public const decimal StrategicPriorMonthOmzetFloorIdr = 5_000_000m;
    public const int TopStrategicOmzetRank = 10;
    public const int SecondaryStrategicOmzetRank = 20;
    public const decimal ImpactConcentrationThresholdPercent = 10m;

    public const string ActionImmediateCollection = "ImmediateCollection";
    public const string ActionEscalateManagement = "EscalateManagement";
    public const string ActionPriorityFollowUp = "PriorityFollowUp";
    public const string ActionProactiveReminder = "ProactiveReminder";
    public const string ActionCreditReview = "CreditReview";
    public const string ActionSalesRecoveryVisit = "SalesRecoveryVisit";
    public const string ActionLegacyDebtReview = "LegacyDebtReview";
    public const string ActionRelationshipMonitor = "RelationshipMonitor";
    public const string ActionDeferCollection = "DeferCollection";
    public const string ActionNoActionToday = "NoActionToday";

    public static string ResolveActionCategory(CustomerRiskForecastContext forecast, CollectionOptimizationContext opt);
    public static int ComputeCollectionPriorityScore(string actionCategory, CustomerRiskForecastContext forecast, CollectionOptimizationContext opt);
    public static decimal ComputeCollectionImpactAmount(decimal overdueBalance, decimal dueWithin7Days);
    public static string ResolveActionOwner(string actionCategory);
    public static string ResolvePlanningConfidence(int daysElapsed);
}
```

Constants align with analysis §6, §8. Options injection via `CollectionOptimizationOptions` nested in `DashboardSnapshotOptions` for floors and caps.

### 4.2 `CollectionOptimizationActionBuilder`

**File:** `CollectionOptimizationActionBuilder.cs`

- Input: `CustomerRiskForecastContext` + `CollectionOptimizationContext` + M20 cross-read keys
- Output: `CollectionOptimizationActionRow` with explainability fields
- Methods:
  - `BuildSelectionReasonText`
  - `BuildPriorityReasonText`
  - `BuildActionReasonText`
  - `BuildTriggeredRuleIds`

### 4.3 `CollectionOptimizationExecutiveSummaryBuilder`

**File:** `CollectionOptimizationExecutiveSummaryBuilder.cs`

- Input: KPI snapshot + top priority row + M20 recovery percent
- Output: `ExecutiveSummaryText` per analysis §10.1

### 4.4 `CollectionOptimizationContext` (in-memory)

Per-customer enrichment beyond M29 forecast context:

```csharp
public sealed class CollectionOptimizationContext
{
    public string CustomerKey { get; set; }
    public decimal DueWithin7Days { get; set; }
    public decimal DueWithin14Days { get; set; }
    public int MinDaysUntilDue { get; set; }          // -1 if no open balance
    public bool HasChronicOverdue { get; set; }
    public bool HasLegacyDebtSignal { get; set; }     // from M20 cross-read
    public bool HasPlafondBreachOverdueSignal { get; set; }
    public bool SalesmanLowRecovery { get; set; }
    public bool IsTop10MtdOmzet { get; set; }
    public bool IsTop20MtdOmzet { get; set; }
    public decimal? CreditUtilizationPercent { get; set; }
    public string ActionCategoryKey { get; set; }
    public string RecommendedActionKey { get; set; }
    public string ActionOwner { get; set; }
    public int CollectionPriorityScore { get; set; }
    public decimal CollectionImpactAmount { get; set; }
    public string SelectionReasonText { get; set; }
    public string PriorityReasonText { get; set; }
    public string ActionReasonText { get; set; }
    public string TriggeredRuleIds { get; set; }
}
```

Built in aggregator from shared piutang metrics (already computed in M29 path) + M20 attention key sets + top omzet rank from Faktur load.

---

## 5. Aggregator Design

### 5.1 `DashboardCollectionOptimizationAggregator`

**Method signature (indicative):**

```csharp
public DashboardCollectionOptimizationAggregateResult Aggregate(
    IReadOnlyList<CustomerRiskForecastContext> forecastContexts,
    DashboardCustomerRiskForecastAggregateResult forecastAggregate,
    DashboardCollectionAggregateResult collectionSnapshot,
    IEnumerable<FakturView> currentMonthFakturRows,
    DateTime businessDate,
    DateTime generatedAt,
    CollectionOptimizationOptions options);
```

### 5.2 Processing steps

```text
1. Build top 10/20 MTD omzet customer key sets from Faktur load
2. Build M20 attention key sets (chronic, legacy, plafond breach, low recovery salesmen)
3. For each CustomerRiskForecastContext:
   a. Build CollectionOptimizationContext enrichment
   b. CollectionOptimizationPolicy.ResolveActionCategory
   c. Compute CollectionImpactAmount
   d. CollectionOptimizationPolicy.ComputeCollectionPriorityScore
   e. CollectionOptimizationActionBuilder.Build explainability texts
4. Filter eligible rows per queue rules (§9.2 analysis)
5. Materialize priority queue (top 30), specialized queues (top 15 each), impact (top 15)
6. Aggregate workload by Salesman, Wilayah, Klasifikasi (top 10 each)
7. Aggregate KPIs + action category distribution
8. CollectionOptimizationExecutiveSummaryBuilder.Build
9. Return aggregate result
```

### 5.3 M20 cross-read usage

| M20 source | M30 use |
| ---------- | ------- |
| `RecoveryVsBillingPercent` | COL-OPT-KPI-12 — copy to optimization KPI |
| `OverdueExposure` | COL-OPT-KPI-10 validation |
| `AttentionList` customer keys by `SignalKey` | `HasLegacyDebtSignal`, `HasPlafondBreachOverdueSignal`, chronic |
| `AttentionList` salesman `LowRecoveryVsBilling` | `SalesmanLowRecovery` per customer via salesman name match |
| `TopOverdueWilayah` hotspot | Wilayah workload elevation flag |

When `collectionSnapshot` is null (first deploy before Collection refresh): skip recovery boost; set recovery KPI to 0 with summary note *"Collection context unavailable."*

---

## 6. Materialized Data Impact

### 6.1 New tables

| Table | Layer | Content |
| ----- | ----- | ------- |
| `BTRPD_CollectionOptimizationKpi` | A | Workload KPIs, impact totals, recovery context, executive summary |
| `BTRPD_CollectionOptimizationActionDist` | B | Action category distribution (≤10 rows) |
| `BTRPD_CollectionOptimizationWorkload` | B | Salesman / Wilayah / Klasifikasi workload (top 10 per type) |
| `BTRPD_CollectionOptimizationPriority` | B | Top 30 collection priority queue |
| `BTRPD_CollectionOptimizationQueue` | B | Specialized queues (ProactiveReminder, CreditReview, SalesRecovery, EscalateManagement) — top 15 each |
| `BTRPD_CollectionOptimizationImpact` | B | Top 15 high impact opportunities |

**Snapshot key:** `CURRENT` — delete-and-replace each refresh.

### 6.2 KPI table columns (indicative)

`BTRPD_CollectionOptimizationKpi`:

- `SnapshotKey`, `GeneratedAt`, `BusinessDate`
- `ActionsTodayCount`, `ImmediateCollectionCount`, `ProactiveReminderCount`, `CreditReviewCount`, `SalesRecoveryCount`, `EscalateManagementCount`
- `CollectionImpactTotal`, `ImmediateImpactTotal`, `OverdueExposure`, `DueWithin7Days`
- `RecoveryVsBillingPercent`, `DeferNoActionCount`, `PlanningConfidence`
- `ExecutiveSummaryText`
- `LastRefreshLogId`

### 6.3 Priority row columns (indicative)

`BTRPD_CollectionOptimizationPriority`:

- `SortOrder`, `CollectionPriorityScore`
- `CustomerCode`, `CustomerName`, `WilayahName`, `SalesPersonName`, `Klasifikasi`
- `ActionCategoryKey`, `ActionCategoryLabel`, `RecommendedActionKey`, `RecommendedActionLabel`, `ActionOwner`
- `OpenBalance`, `OverdueBalance`, `DueWithin7Days`, `CollectionImpactAmount`
- `M29Category`, `M29RecommendationKey`, `M29PrimarySignalKey`
- `MinDaysUntilDue`, `CreditUtilizationPercent`
- `SelectionReasonText`, `PriorityReasonText`, `ActionReasonText`, `TriggeredRuleIds`
- `ReportRoute`, `DrillDownRoute`

### 6.4 Queue row columns (indicative)

`BTRPD_CollectionOptimizationQueue`:

- `QueueKey` (`ProactiveReminder`, `CreditReview`, `SalesRecovery`, `EscalateManagement`)
- `SortOrder`, same customer/action fields as priority (subset)
- `QueueReasonText`

### 6.5 Workload row columns (indicative)

`BTRPD_CollectionOptimizationWorkload`:

- `WorkloadType` (`Salesman`, `Wilayah`, `Klasifikasi`)
- `EntityKey`, `EntityLabel`
- `ActionCount`, `ImmediateCount`, `ImpactTotal`, `OverdueExposure`
- `IsHotspot` (wilayah only)
- `SortOrder`

### 6.6 SQL scripts

- Add `CREATE TABLE` to `btr.sql/Scripts/Create_BTRPD_PortalDashboard_Tables.sql`
- Upgrade script `Upgrade_M30_CollectionOptimization.sql`

### 6.7 Existing tables

**No changes** to M29 `BTRPD_CustomerRiskForecast*` or M20 `BTRPD_Collection*` tables.

---

## 7. Snapshot Refresh Strategy

| Aspect | Choice |
| ------ | ------ |
| Worker | `RefreshDashboardCustomerSnapshotWorker` extended |
| Cadence | 30 minutes (`CustomerIntervalMinutes`) |
| Transaction | Single `ReplaceCurrent` — M17 + M29 + M30 in one transaction |
| Manual refresh | `POST /api/admin/dashboard/refresh?domain=Customer` rebuilds optimization |
| CLI | `btr.portal.worker --domain Customer` |
| M20 cross-read | `IDashboardCollectionSnapshotDal.GetCurrent()` at start of worker execute |
| Presentation Mode | `IBusinessDateProvider.Today` drives all date rules |

### 7.1 Worker extension pseudocode

```csharp
var collectionSnapshot = _collectionSnapshotDal.GetCurrent();

var customerAggregate = _aggregator.Aggregate(...);

var forecastBuild = _forecastAggregator.AggregateWithContexts(...);
// or: var forecastAggregate = _forecastAggregator.Aggregate(...); contexts returned via out param

var optimizationAggregate = _optimizationAggregator.Aggregate(
    forecastBuild.Contexts,
    forecastBuild.Aggregate,
    collectionSnapshot,
    fakturRows,
    today,
    generatedAt,
    _options.CollectionOptimization);

_snapshotDal.ReplaceCurrent(customerAggregate, forecastBuild.Aggregate, optimizationAggregate, refreshLogId);
```

### 7.2 `IDashboardCustomerSnapshotDal` extension

Extend `ReplaceCurrent` and read methods to include optimization tables in same delete-insert transaction as M17/M29 tables.

---

## 8. API Design

### 8.1 Endpoint

```
GET /api/dashboard/collection-optimization
```

**Auth:** Same as other dashboard endpoints.

**Response:** `DashboardCollectionOptimizationResponse`

```csharp
public class DashboardCollectionOptimizationResponse
{
    public DashboardCollectionOptimizationKpiDto Kpi { get; set; }
    public IReadOnlyList<DashboardCollectionOptimizationActionDistDto> ActionDistribution { get; set; }
    public IReadOnlyList<DashboardCollectionOptimizationWorkloadDto> Workload { get; set; }
    public IReadOnlyList<DashboardCollectionOptimizationPriorityDto> PriorityQueue { get; set; }
    public IReadOnlyList<DashboardCollectionOptimizationQueueDto> SpecializedQueues { get; set; }
    public IReadOnlyList<DashboardCollectionOptimizationImpactDto> TopImpactOpportunities { get; set; }
    public DateTime GeneratedAt { get; set; }
}
```

`SpecializedQueues` includes `QueueKey` on each row for client-side tab filtering.

### 8.2 Application query

**Folder:** `btr.application/ReportingContext/DashboardCollectionOptimizationAgg/`

- `Queries/GetDashboardCollectionOptimizationQuery.cs`
- Handler reads `IDashboardCollectionOptimizationDal.GetCurrent()` — no business logic in handler

### 8.3 Controller

**File:** `btr.portal.api/Controllers/Dashboard/CollectionOptimizationDashboardController.cs`

Thin — delegates to MediatR only.

---

## 9. Portal Web Implementation

### 9.1 Route and navigation

| Item | Value |
| ---- | ----- |
| Route | `/dashboard/collection-optimization` |
| Sidebar label | Collection Optimization |
| Position | After Customer Risk Forecast, before Salesman Performance |
| Router | Lazy import in `src/router/index.ts` |

### 9.2 View structure

**File:** `CollectionOptimizationDashboardView.vue`

Reuse patterns from `InventoryOptimizationDashboardView.vue` and `CustomerRiskForecastDashboardView.vue`:

| Component | Purpose |
| --------- | ------- |
| `DashboardDetailLayout` | Shell, refresh, generated-at |
| `CollectionOptimizationSummary` | Executive summary block |
| `CollectionOptimizationKpiGrid` | 2 KPI rows (workload + context) |
| `CollectionOptimizationActionChart` | Donut — action category distribution |
| `CollectionOptimizationWorkloadChart` | Bar — wilayah / salesman toggle |
| `CollectionOptimizationImpactChart` | Bar — impact by category |
| `CollectionOptimizationPriorityTable` | Top 30 priority queue with expand detail |
| `CollectionOptimizationQueueTabs` | Specialized queues |
| `CollectionOptimizationImpactTable` | Top 15 impact opportunities |

### 9.3 Store and API

| File | Change |
| ---- | ------ |
| `src/api/dashboardApi.ts` | `getCollectionOptimization()` |
| `src/models/dashboard.ts` | Response interfaces |
| `src/stores/dashboardStore.ts` | `loadCollectionOptimization()` |

### 9.4 Action category badge colors

| Action Category | Token |
| --------------- | ----- |
| ImmediateCollection | `severity-critical` |
| EscalateManagement | `severity-critical` + emphasis |
| PriorityFollowUp | `severity-warning` |
| ProactiveReminder | `severity-info` |
| CreditReview | `severity-warning` |
| SalesRecoveryVisit | `severity-info` |
| LegacyDebtReview | `severity-warning` |
| RelationshipMonitor | `severity-info` |
| DeferCollection | `severity-success` |
| NoActionToday | neutral |

### 9.5 Client helper

**File:** `src/services/collectionOptimizationSignals.ts`

- Action category label map
- Action owner badge map
- Queue key labels

### 9.6 Traceability footer

Links:

- `/dashboard/customer-risk-forecast` — forecast rationale
- `/dashboard/collection` — recovery performance
- `/dashboard/customers` — current-state attention
- `/dashboard/piutang` — receivable exposure
- `/reports/piutang` — evidence
- `/reports/sales` — purchase evidence

Disclaimer:

> *Recommendations are indicative operational guidance based on deterministic business rules. BTR Portal does not initiate collection contact, modify credit limits, or schedule visits. Execute actions in BTR Desktop or field operations.*

---

## 10. Configuration

Extend `DashboardSnapshotOptions`:

```json
"DashboardSnapshot": {
  "CollectionOptimizationSalesRecoveryOverdueFloorIdr": 500000,
  "CollectionOptimizationStrategicPriorMonthOmzetFloorIdr": 5000000,
  "CollectionOptimizationLargeDueSoonFloorIdr": 10000000,
  "CollectionOptimizationMaxPriorityRows": 30,
  "CollectionOptimizationMaxQueueRows": 15,
  "CollectionOptimizationMaxImpactRows": 15,
  "CollectionOptimizationMaxWorkloadRows": 10
}
```

Add nested class `CollectionOptimizationOptions` with `FromDashboardOptions` factory — mirror `CustomerRiskForecastOptions` pattern.

---

## 11. Testing Strategy

### 11.1 Unit tests

**File:** `btr.test/ReportingContext/CollectionOptimizationPolicyTest.cs`

| Test case | Validates |
| --------- | --------- |
| Immediate collection — overdue + critical | COL-OPT-CAT-01 |
| Sales recovery — decline, low overdue | COL-OPT-CAT-06 |
| Sales recovery blocked — high overdue | COL-OPT-CAT-01 precedence |
| Proactive reminder — current, due 10d, watch | COL-OPT-CAT-04 |
| Defer — healthy, due 20d | COL-OPT-DEF-01 |
| Strategic watch — relationship monitor | COL-OPT-DEF-03 |
| Priority score ordering | High impact ranks above low |
| Impact amount formula | overdue + due7d |
| Action owner resolution | Sales vs Collection vs Finance |

**File:** `CollectionOptimizationActionBuilderTest.cs`

| Test case | Validates |
| --------- | --------- |
| Reason text includes overdue amount | COL-OPT-REC-02 |
| TriggeredRuleIds includes M29 + CAT rules | COL-OPT-REC-05 |

**File:** `DashboardCollectionOptimizationAggregatorTest.cs`

| Test case | Validates |
| --------- | --------- |
| End-to-end synthetic portfolio | KPI counts match queues |
| COL-OPT-KPI-53 top customer = rank 1 | |
| M20 null graceful degrade | |
| Priority cap 30 enforced | COL-OPT-REC-07 |
| No duplicate forecast signal evaluation | COL-OPT-06 — contexts passed through |

### 11.2 Integration tests

**File:** `DashboardCustomerSnapshotVerificationTest.cs` (extend)

- Round-trip optimization tables after refresh
- M29 tables unchanged when optimization added

### 11.3 Frontend tests

**File:** `collectionOptimizationSignals.spec.ts`

- Action category label mapping
- Queue key filtering

---

## 12. Implementation Phases

### Phase 1 — Foundation (2–3 days)

1. SQL tables + upgrade script
2. `CollectionOptimizationPolicy` + unit tests
3. `CollectionOptimizationOptions` + config binding
4. Refactor M29 aggregator to expose `Contexts` (minimal)

### Phase 2 — Aggregation (3–4 days)

1. Context DTOs + aggregate result models
2. `CollectionOptimizationActionBuilder` + tests
3. `DashboardCollectionOptimizationAggregator` + tests
4. `CollectionOptimizationExecutiveSummaryBuilder`
5. Extend `DashboardCustomerSnapshotDal` write path
6. Extend `RefreshDashboardCustomerSnapshotWorker` (M20 cross-read inject)

### Phase 3 — API (1–2 days)

1. `IDashboardCollectionOptimizationDal` read facade
2. MediatR query + handler
3. Controller
4. Snapshot verification test

### Phase 4 — Portal UI (3–4 days)

1. API model + store + route
2. View + components
3. Charts (reuse chart primitives)
4. Navigation sidebar entry
5. Frontend signal spec

### Phase 5 — Documentation & UAT (1–2 days)

1. `docs/features/collection-optimization/feature.md`
2. Update `btr-portal-operational.md` usage section
3. UAT against acceptance criteria
4. Knowledge extraction note

**Estimated total effort:** 10–15 developer days (1 implementer, excluding PO review cycles)

---

## 13. Acceptance Criteria

### 13.1 Functional

1. Dashboard accessible at `/dashboard/collection-optimization` for authorized users.
2. Executive summary displays server-composed plain-language text answering *"What should Finance and Sales focus on today?"*
3. All COL-OPT-KPI-01 through COL-OPT-KPI-07 render and match materialized snapshot.
4. Priority queue shows Action, Priority Score, Impact, Risk Category, and Reason for top 30 customers.
5. Specialized queues filter correctly by `QueueKey`.
6. Every priority row includes `TriggeredRuleIds` traceable to analysis §8 and M29 rules.
7. Optimization refreshes with Customer domain scheduler (~30 min) and manual admin refresh.
8. Sales recovery queue rows show `ActionOwner = Sales`; collection rows show `Collection`.

### 13.2 Traceability

1. COL-OPT-KPI-12 equals M20 `RecoveryVsBillingPercent` from cross-read (COL-OPT-KPI-51).
2. M29 forecast signals on optimization rows match M29 customer row for same `CustomerCode` (spot check).
3. No M29 forecast rule re-evaluation in optimization code path (code review + unit test).

### 13.3 Non-regression

1. `GET /api/dashboard/customer-risk-forecast` response unchanged.
2. M29 snapshot KPI values unchanged after optimization added.
3. Customer refresh duration increase ≤ 15s on staging dataset.

### 13.4 Quality

1. All unit tests pass.
2. No business logic in API controller or read DAL.
3. UI disclaimer visible on dashboard footer.

---

## 14. Database Impact

| Object | Impact |
| ------ | ------ |
| `BTRPD_CustomerRiskForecast*` | Unchanged |
| `BTRPD_Collection*` | Read-only cross-read |
| `BTRPD_CollectionOptimization*` | 6 new tables |
| `BTR_Piutang` / `BTR_Faktur` / `BTR_Customer` | Read-only — via existing Customer worker loads |

**No new indexes required V1** — optimization writes snapshot rows only; reads bounded lists.

---

## 15. Performance Considerations

| Concern | Mitigation |
| ------- | ---------- |
| Second customer loop for optimization | O(customers with M29 context) — same list as M29; no extra SQL |
| M20 cross-read | Single snapshot SELECT per refresh |
| In-memory enrichment | Lightweight dictionary lookups |
| Single transaction write | 6 child tables + M17 + M29 — same pattern as existing Customer refresh |
| API read | 1 KPI + bounded lists — p95 target < 500ms |
| Worker increment | Target < 8s additional on typical dataset |

---

## 16. DI Registration Checklist

**File:** `btr.infrastructure/Portal/InfrastructurePortalExtensions.cs`

Register:

- `IDashboardCollectionOptimizationDal` → `DashboardCollectionOptimizationDal`
- `DashboardCollectionOptimizationAggregator` (scoped/transient per existing pattern)

**Worker:** Inject `IDashboardCollectionSnapshotDal` into `RefreshDashboardCustomerSnapshotWorker` if not present.

---

## 17. File Manifest (expected new files)

```text
btr.application/
  ReportingContext/DashboardSnapshotAgg/Services/
    CollectionOptimizationPolicy.cs
    CollectionOptimizationActionBuilder.cs
    CollectionOptimizationExecutiveSummaryBuilder.cs
    DashboardCollectionOptimizationAggregator.cs
  ReportingContext/DashboardSnapshotAgg/Models/
    DashboardCollectionOptimizationAggregateResult.cs
    CollectionOptimizationContext.cs
    CustomerRiskForecastBuildResult.cs          ← if refactor chosen
    (+ row DTOs)
  ReportingContext/DashboardCollectionOptimizationAgg/
    Contracts/IDashboardCollectionOptimizationDal.cs
    Queries/GetDashboardCollectionOptimizationQuery.cs

btr.infrastructure/
  ReportingContext/DashboardCollectionOptimizationAgg/
    DashboardCollectionOptimizationDal.cs

btr.portal.api/Controllers/Dashboard/
  CollectionOptimizationDashboardController.cs

btr.portal.web/src/
  views/dashboard/CollectionOptimizationDashboardView.vue
  components/dashboard/collection-optimization/*.vue
  services/collectionOptimizationSignals.ts

btr.test/ReportingContext/
  CollectionOptimizationPolicyTest.cs
  CollectionOptimizationActionBuilderTest.cs
  DashboardCollectionOptimizationAggregatorTest.cs

btr.sql/Scripts/
  Upgrade_M30_CollectionOptimization.sql
```

---

## Document Maintenance

When implemented:

1. Create `docs/features/collection-optimization/feature.md`
2. Update `docs/features/btr-portal/btr-portal-domain.md` — move M30 from future to current
3. Add operational usage to `btr-portal-operational.md`
4. Archive this plan after `implementation-summary-m30-collection-optimization.md` is written

**Success criterion:** An Implementer can begin coding with minimal ambiguity after Product Owner approves the analysis document.
