# Implementation Plan — M29 Customer Risk Forecast Dashboard

## Document Status

| Field | Value |
| ----- | ----- |
| Milestone | M29 — Customer Risk Forecast Dashboard |
| Authoritative requirements | [portal-analysis-m29-customer-risk-forecast.md](./portal-analysis-m29-customer-risk-forecast.md) |
| Reference pattern | M27 Cash Flow Forecast, M17 Customer Analytics, M28 Inventory Forecast, materialized-dashboard architecture |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |
| Status | **Ready for implementation** (pending Product Owner approval of analysis) |

**Prerequisite:** Product Owner approval of analysis document (risk categories, forecast rules, wireframe, KPI definitions, recommendation types).

**Prerequisite implementation:** M17 Customer Analytics, M20 Collection Dashboard, M27 Cash Flow Forecast complete.

---

## 1. Goal

Deliver a read-only **Customer Risk Forecast Dashboard** at `/dashboard/customer-risk-forecast` that predicts **customer-level collection and relationship risk** within a **30-day horizon** using **deterministic, explainable business rules** — payment delay, credit limit projection, inactivity lead, purchase decline, and collection risk composition.

**In scope:**

- New customer risk forecast aggregation during existing **Customer** snapshot refresh
- New DALs for customer pelunasan summary and prior-month Faktur omzet (aggregated SQL)
- New snapshot tables for forecast KPIs, customer risk rows, attention signals, and recommendations
- New API endpoint `GET /api/dashboard/customer-risk-forecast`
- New Portal Web dashboard page with executive summary, KPIs, charts, and risk tables
- Unit tests for forecast policy, risk builder, and aggregator

**Out of scope:**

- Changes to M17 `/api/dashboard/customers` response shape
- Changes to M20/M27 dashboard APIs
- AI/ML, probability scoring, DSO headline KPI
- Alert Center / Executive integration
- Pelunasan Report / Customer Report
- Historical snapshot retention
- Per-salesman or per-wilayah forecast pages (wilayah chart only as aggregation)

---

## 2. Impact Analysis

### 2.1 Business areas affected

| Area | Impact |
| ---- | ------ |
| Finance (Piutang / Collection) | Primary — forward receivable risk |
| Sales | Read-only — purchase decline and inactivity from Faktur |
| Customer master | Read-only — Plafond, IsSuspend, Wilayah, Klasifikasi |
| Field activity | None V1 |

### 2.2 Systems affected

| System | Impact |
| ------ | ------ |
| BTR Portal API | New controller + MediatR query |
| BTR Portal Web | New route, view, components, store method |
| BTR Portal Worker | Extended Customer snapshot refresh |
| BTR SQL | New snapshot tables + optional index |
| BTR Desktop | None |

### 2.3 Existing features — preserve behavior

| Feature | Change |
| ------- | ------ |
| Customer Analytics Dashboard | **None** |
| Piutang / Collection / Cash Flow Forecast | **None** |
| Customer snapshot refresh cadence | **Unchanged** (~30 min) |
| `GET /api/dashboard/customers` | **Unchanged** |
| M17 customer tables | **None** — forecast is additive |

### 2.4 Source code modules affected

| Layer | Module | Change type |
| ----- | ------ | ----------- |
| Application | `ReportingContext/DashboardSnapshotAgg/Services/` | **Add** `DashboardCustomerRiskForecastAggregator`, `CustomerRiskForecastPolicy`, `CustomerRiskSignalBuilder`, `CustomerRiskRecommendationBuilder`, `CustomerRiskExecutiveSummaryBuilder` |
| Application | `ReportingContext/DashboardSnapshotAgg/Models/` | **Add** customer risk forecast aggregate DTOs |
| Application | `ReportingContext/DashboardSnapshotAgg/UseCases/` | **Extend** `RefreshDashboardCustomerSnapshotWorker` |
| Application | `ReportingContext/DashboardSnapshotAgg/Contracts/` | **Extend** `IDashboardCustomerSnapshotDal` |
| Application | `ReportingContext/DashboardCustomerRiskForecastAgg/` | **New** query + read contract |
| Application | `FinanceContext/PiutangAgg/Contracts/` | **Add** `ICustomerPelunasanSummaryDal`, `ICustomerPaymentBehaviorDal` |
| Application | `SalesContext/FakturInfo/` | **Add** `ICustomerOmzetHistoryDal` |
| Infrastructure | `FinanceContext/PiutangAgg/` | **Add** pelunasan/payment behavior SQL |
| Infrastructure | `SalesContext/FakturInfoAgg/` | **Add** customer omzet history SQL |
| Infrastructure | `ReportingContext/DashboardSnapshotAgg/` | **Extend** snapshot DAL write/read |
| SQL | `btr.sql/Scripts/` | **Add** customer risk forecast tables |
| Portal API | `Controllers/Dashboard/` | **Add** `CustomerRiskForecastDashboardController` |
| Portal Web | `views/dashboard/`, `components/dashboard/`, `stores/`, `api/`, `router/` | **Add** forecast page + components |
| Tests | `btr.test/ReportingContext/` | **Add** policy + builder + aggregator tests |

### 2.5 Risk summary

| Risk | Severity | Mitigation |
| ---- | -------- | ---------- |
| Duplicate aging logic vs M14 | High | Use `PiutangAgingBucketResolver` exclusively |
| Duplicate dormant/plafond vs M17 | High | Call shared constants; forecast extends — does not replace M17 signals |
| Customer pelunasan SQL performance | Medium | Aggregated `GROUP BY CustomerId`; 600s timeout pattern; index recommendation |
| Projected plafond misinterpreted as credit approval | Medium | UI disclaimer — indicative upper bound |
| Customer key inconsistency (Id vs Code) | Medium | Standardize on `DashboardCustomerKeyResolver`; document piutang rows without code |
| Prior month omzet load duplication | Low | Single `ICustomerOmzetHistoryDal` returns current + prior month in one query |
| Early-month decline false positives | Medium | Forecast confidence indicator; prior month omzet floor |
| Recommendation volume | Low | Cap materialized rows (20/25/15) |

---

## 3. Architecture Overview

### 3.1 Topology

```text
Task Scheduler (Customer domain, ~30 min)
        ↓
RefreshDashboardCustomerSnapshotWorker
        ↓
Load: IFakturViewDal.ListData(current month)
      IFakturViewDal / ICustomerOmzetHistoryDal (current + prior month by customer)   ← new
      ICustomerLastFakturDal.ListLastFakturByCustomer()
      IPiutangOpenBalanceDal.ListOpenBalances()
      ICustomerDal.ListData()
      ICustomerPelunasanSummaryDal.ListSummary(30d lookback)                          ← new
      ICustomerPaymentBehaviorDal.ListPaymentBehavior(90d lookback)                    ← new
        ↓
DashboardCustomerAggregator.Aggregate()                    → BTRPD_Customer* (unchanged)
        ↓
DashboardCustomerRiskForecastAggregator.Aggregate()        → BTRPD_CustomerRiskForecast* (new)
        ↓
DashboardCustomerSnapshotDal.ReplaceCurrent()              → single transaction

Browser → GET /api/dashboard/customer-risk-forecast
        ↓ MediatR
GetDashboardCustomerRiskForecastHandler
        ↓ IDashboardCustomerRiskForecastDal
DashboardCustomerRiskForecastDal → snapshot SELECT
```

**Design decision:** Extend the **existing Customer snapshot refresh** after M17 aggregate step. Customer worker already loads Faktur, piutang, last Faktur, and customer master — minimal incremental loads for pelunasan and prior-month omzet.

**Alternatives rejected:**

| Alternative | Reason rejected |
| ----------- | --------------- |
| Separate `CustomerRiskForecast` worker domain | Duplicates piutang/Faktur load; temporal skew vs M17 |
| Extend Collection worker | M29 is customer-portfolio-centric; Collection worker already heavy with M20+M27 |
| Cross-read M17/M14 snapshots at API time | Violates materialized dashboard pattern; stale composition |

### 3.2 Layering

```text
btr.portal.api          → CustomerRiskForecastDashboardController (thin)
btr.application         → GetDashboardCustomerRiskForecastQuery + Handler
                          → DashboardCustomerRiskForecastAggregator
                          → CustomerRiskForecastPolicy
                          → CustomerRiskSignalBuilder
                          → CustomerRiskRecommendationBuilder
                          → CustomerRiskExecutiveSummaryBuilder
                          → IDashboardCustomerRiskForecastDal (contract)
btr.infrastructure      → DashboardCustomerRiskForecastDal (read facade)
                          → CustomerPelunasanSummaryDal
                          → CustomerPaymentBehaviorDal
                          → CustomerOmzetHistoryDal
                          → DashboardCustomerSnapshotDal (extended write)
btr.portal.web          → CustomerRiskForecastDashboardView.vue + components
```

MediatR pattern preserved. No business logic in controller.

### 3.3 New policy class

**File:** `btr.application/ReportingContext/DashboardSnapshotAgg/Services/CustomerRiskForecastPolicy.cs`

Mirror `CashFlowForecastPolicy` / `InventoryForecastPolicy` structure:

```csharp
public static class CustomerRiskForecastPolicy
{
    public const int DefaultHorizonDays = 30;
    public const int ApproachingDormantDaysMin = 60;
    public const int DormantDaysThreshold = 90; // align M17
    public const int PaymentLagLookbackDays = 90;
    public const int MinSettledFaktursForLag = 2;
    public const decimal ModerateDeclineRatio = 0.70m;
    public const decimal SevereDeclineRatio = 0.50m;
    public const decimal ApproachingPlafondRatio = 0.90m;
    public const int AvgPaymentLagLikelyLateDays = 7;
    public const int AvgPaymentLagEscalatingDays = 14;
    public const int NoPaymentRecencyDays = 30;
    public const int DueSoonSlowPayerDays = 14;
    public const int DueUrgentDays = 7;
    public const decimal DueConcentrationThresholdPercent = 15m;

    public static string ResolveCategory(IReadOnlyList<CustomerRiskSignalContext> signals);
    public static int ComputeRiskPriorityScore(string category, decimal openBalance, int strongCount, int moderateCount, int weakCount, int minDaysUntilDue);
    public static decimal ComputeProjectedOpenBalance(decimal currentOpen, decimal mtdOmzet, int daysElapsed, int horizonDays);
    public static decimal ComputeProjectedMonthOmzet(decimal mtdOmzet, int daysElapsed, int daysInMonth);
    public static decimal ComputePortfolioHealthScore(decimal elevatedRiskReceivable, decimal totalPiutang, int highRiskCount, int activeCustomerCount);
    public static string ResolveForecastConfidence(int daysElapsed);
}
```

### 3.4 Signal builder

**File:** `CustomerRiskSignalBuilder.cs`

Static builder (mirror `CashFlowCollectionRiskBuilder`):

- Input: per-customer context DTO assembled in aggregator
- Output: `List<CustomerRiskSignalRow>` with `SignalKey`, `Severity`, `RuleId`, `Explanation`
- Dedupe: one row per `(CustomerKey, SignalKey)`

### 3.5 Recommendation builder

**File:** `CustomerRiskRecommendationBuilder.cs`

- Input: category + signals + customer context
- Output: primary recommendation per customer + top 15 company recommendations table
- Priority order per analysis §8.1

### 3.6 Executive summary builder

**File:** `CustomerRiskExecutiveSummaryBuilder.cs`

- Input: KPI snapshot + top customer row
- Output: single `ExecutiveSummaryText` string (server-composed)

---

## 4. New DAL Contracts

### 4.1 `ICustomerPelunasanSummaryDal`

**Purpose:** Customer-level payment recency for CRF-P03, CRF-L03.

```csharp
public interface ICustomerPelunasanSummaryDal
{
    IEnumerable<CustomerPelunasanSummaryDto> ListSummary(DateTime windowStart, DateTime windowEnd);
}

public class CustomerPelunasanSummaryDto
{
    public string CustomerCode { get; set; }
    public string CustomerName { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public decimal TotalCash { get; set; }
    public decimal TotalSettlement { get; set; }
    public int PaymentCount { get; set; }
}
```

**SQL pattern:** Join `BTR_PiutangLunas` → `BTR_Piutang` → `BTR_Customer`; `GROUP BY CustomerId`; exclude `JenisLunas = 2` (UangMuka); `MAX(LunasDate)` for recency.

### 4.2 `ICustomerPaymentBehaviorDal`

**Purpose:** Average payment lag for CRF-P01, CRF-P02, CRF-P04.

```csharp
public interface ICustomerPaymentBehaviorDal
{
    IEnumerable<CustomerPaymentBehaviorDto> ListPaymentBehavior(DateTime windowStart, DateTime windowEnd);
}

public class CustomerPaymentBehaviorDto
{
    public string CustomerCode { get; set; }
    public string CustomerName { get; set; }
    public int SettledFakturCount { get; set; }
    public decimal? AvgPaymentLagDays { get; set; }
}
```

**SQL pattern:**

```sql
-- Conceptual: fully settled piutang in lookback window
AVG(DATEDIFF(day, DueDate, LastLunasDate))
GROUP BY CustomerId
HAVING COUNT(*) >= @MinSettledFakturs
```

`LastLunasDate` = `MAX(LunasDate)` per PiutangId where `Sisa <= 1`.

### 4.3 `ICustomerOmzetHistoryDal`

**Purpose:** MoM decline rules CRF-D01, CRF-D02, CRF-D03.

```csharp
public interface ICustomerOmzetHistoryDal
{
    IEnumerable<CustomerOmzetHistoryDto> ListOmzetByCustomer(Periode currentMonth, Periode priorMonth);
}

public class CustomerOmzetHistoryDto
{
    public string CustomerCode { get; set; }
    public string CustomerName { get; set; }
    public decimal CurrentMonthOmzet { get; set; }
    public decimal PriorMonthOmzet { get; set; }
}
```

**SQL pattern:** Two conditional aggregates on `BTR_Faktur` grouped by customer — void filter `VoidDate = '3000-01-01'`.

**Optimization:** Single query with `SUM(CASE WHEN FakturDate IN current THEN GrandTotal ELSE 0 END)` preferred over two DAL calls.

---

## 5. Aggregator Design

### 5.1 `DashboardCustomerRiskForecastAggregator`

**Method signature (indicative):**

```csharp
public DashboardCustomerRiskForecastAggregateResult Aggregate(
    IEnumerable<PiutangOpenBalanceDto> piutangRows,
    IEnumerable<CustomerOmzetHistoryDto> omzetHistoryRows,
    IEnumerable<CustomerLastFakturDto> lastFakturRows,
    IEnumerable<CustomerModel> customers,
    IEnumerable<CustomerPelunasanSummaryDto> pelunasanSummaryRows,
    IEnumerable<CustomerPaymentBehaviorDto> paymentBehaviorRows,
    IEnumerable<FakturView> currentMonthFakturRows,
    DateTime businessDate,
    DateTime generatedAt,
    CustomerRiskForecastOptions options);
```

### 5.2 Processing steps

```text
1. Build customer master lookup (Plafond, Wilayah, Klasifikasi, IsSuspend)
2. Build open balance / aging / due-within-H maps (PiutangAgingBucketResolver)
3. Build omzet pace + decline ratios per customer
4. Build days-since-last-Faktur + approaching dormant flags
5. Build payment lag + recency maps
6. Compute company due-within-H total (concentration denominator)
7. CustomerRiskSignalBuilder.Build → signals per customer
8. CustomerRiskForecastPolicy.ResolveCategory → category per customer
9. CustomerRiskForecastPolicy.ComputeRiskPriorityScore → ranking
10. CustomerRiskRecommendationBuilder.Build → recommendations
11. Aggregate KPIs + distribution buckets + wilayah breakdown
12. CustomerRiskExecutiveSummaryBuilder.Build → summary text
13. Materialize top 20 / 25 / 15 rows with sort orders
```

### 5.3 Context DTO (in-memory)

**File:** `Models/CustomerRiskForecastContext.cs`

Per-customer working object passed to builders — avoids repeated dictionary lookups:

```csharp
public sealed class CustomerRiskForecastContext
{
    public string CustomerKey { get; set; }
    public string CustomerCode { get; set; }
    public string CustomerName { get; set; }
    public string WilayahName { get; set; }
    public decimal OpenBalance { get; set; }
    public decimal OverdueBalance { get; set; }
    public decimal DueWithinHorizon { get; set; }
    public int MinDaysUntilDue { get; set; }
    public decimal MtdOmzet { get; set; }
    public decimal PriorMonthOmzet { get; set; }
    public decimal? DeclineRatio { get; set; }
    public int? DaysSinceLastFaktur { get; set; }
    public decimal? AvgPaymentLagDays { get; set; }
    public int? DaysSinceLastPayment { get; set; }
    public decimal Plafond { get; set; }
    public decimal ProjectedOpenBalance { get; set; }
    public bool IsActiveThisMonth { get; set; }
    public bool IsCurrentlyPlafondBreach { get; set; }
    public List<CustomerRiskSignalContext> Signals { get; set; }
}
```

---

## 6. Materialized Data Impact

### 6.1 New tables

| Table | Layer | Content |
| ----- | ----- | ------- |
| `BTRPD_CustomerRiskForecastKpi` | A | Portfolio KPIs, health score, confidence, executive summary text |
| `BTRPD_CustomerRiskForecastDist` | B | Category distribution (5 rows) |
| `BTRPD_CustomerRiskForecastWilayah` | B | Top 10 wilayah by elevated-risk customer count |
| `BTRPD_CustomerRiskForecastSignalMix` | B | Signal type counts for chart |
| `BTRPD_CustomerRiskForecastCustomer` | B | Top 20 customer risk rows |
| `BTRPD_CustomerRiskForecastAttention` | B | Top 25 forecast attention signals |
| `BTRPD_CustomerRiskForecastRecommendation` | B | Top 15 recommended actions |

**Snapshot key:** `CURRENT` — delete-and-replace each refresh.

### 6.2 KPI table columns (indicative)

`BTRPD_CustomerRiskForecastKpi`:

- `SnapshotKey`, `GeneratedAt`, `BusinessDate`, `HorizonDays`
- `CustomersForecastedAtRisk`, `HighRiskCustomerCount`, `CriticalCustomerCount`
- `ElevatedRiskReceivable`, `ElevatedRiskReceivablePercent`, `PortfolioHealthScore`
- `TotalPiutang`, `ForecastConfidence`
- `PaymentDelaySignalCount`, `CreditLimitSignalCount`, `InactivitySignalCount`, `PurchaseDeclineSignalCount`, `CollectionRiskSignalCount`
- `HealthyCount`, `WatchCount`, `AttentionCount`, `HighRiskCount`, `CriticalCount`
- `ExecutiveSummaryText`
- `LastRefreshLogId`

### 6.3 Customer risk row columns (indicative)

`BTRPD_CustomerRiskForecastCustomer`:

- `SortOrder`, `RiskPriorityScore`, `Category`, `CategoryLabel`
- `CustomerCode`, `CustomerName`, `WilayahName`, `SalesPersonName` (nullable, from last Faktur with salesman cross-ref if loaded)
- `OpenBalance`, `OverdueBalance`, `DueWithinHorizon`, `Plafond`, `ProjectedOpenBalance`
- `MtdOmzet`, `PriorMonthOmzet`, `DeclineRatio`, `DaysSinceLastFaktur`, `AvgPaymentLagDays`
- `PrimarySignalKey`, `PrimarySignalLabel`, `ReasonText`
- `RecommendationKey`, `RecommendationLabel`
- `ReportRoute`, `DrillDownRoute`

### 6.4 Attention row columns (indicative)

`BTRPD_CustomerRiskForecastAttention`:

- `SortOrder`, `CustomerCode`, `CustomerName`
- `SignalKey`, `SignalLabel`, `Severity` (Strong/Moderate/Weak)
- `Amount`, `HorizonText`, `RuleId`, `Explanation`
- `ReportRoute`

### 6.5 SQL scripts

Add to `btr.sql/Scripts/Create_BTRPD_PortalDashboard_Tables.sql`:

- `CREATE TABLE` statements matching existing `BTRPD_*` FK/snapshot pattern
- Upgrade script `Upgrade_M29_CustomerRiskForecast.sql` for existing deployments

### 6.6 Existing tables

**No changes** to M17 `BTRPD_Customer*` tables.

---

## 7. Snapshot Refresh Strategy

| Aspect | Choice |
| ------ | ------ |
| Worker | `RefreshDashboardCustomerSnapshotWorker` extended |
| Cadence | 30 minutes (`CustomerIntervalMinutes`) |
| Transaction | Single `ReplaceCurrent` — M17 + M29 in one transaction |
| Manual refresh | `POST /api/admin/dashboard/refresh?domain=Customer` rebuilds forecast |
| CLI | `btr.portal.worker --domain Customer` |
| Presentation Mode | `IBusinessDateProvider.Today` drives all date rules |
| Load steps | Increase from 4 to 7 (add omzet history, pelunasan summary, payment behavior) |

### 7.1 Worker extension pseudocode

```csharp
// After existing M17 aggregate:
var priorMonth = PriorMonthPeriode(today);
var omzetHistory = _customerOmzetHistoryDal.ListOmzetByCustomer(periode, priorMonth);
var pelunasanSummary = _customerPelunasanSummaryDal.ListSummary(today.AddDays(-30), today);
var paymentBehavior = _customerPaymentBehaviorDal.ListPaymentBehavior(today.AddDays(-90), today);

var forecastAggregate = _forecastAggregator.Aggregate(
    piutangRows, omzetHistory, lastFakturRows, customers,
    pelunasanSummary, paymentBehavior, fakturRows,
    today, generatedAt, _options);

_snapshotDal.ReplaceCurrent(aggregate, forecastAggregate, refreshLogId);
```

### 7.2 `IDashboardCustomerSnapshotDal` extension

Extend `ReplaceCurrent` and read methods to include forecast tables in same delete-insert transaction as M17 tables.

---

## 8. API Design

### 8.1 Endpoint

```
GET /api/dashboard/customer-risk-forecast
```

**Auth:** Same as other dashboard endpoints.

**Response:** `DashboardCustomerRiskForecastResponse`

```csharp
public class DashboardCustomerRiskForecastResponse
{
    public DashboardCustomerRiskForecastKpiDto Kpi { get; set; }
    public IReadOnlyList<DashboardCustomerRiskForecastDistDto> CategoryDistribution { get; set; }
    public IReadOnlyList<DashboardCustomerRiskForecastWilayahDto> TopWilayah { get; set; }
    public IReadOnlyList<DashboardCustomerRiskForecastSignalMixDto> SignalMix { get; set; }
    public IReadOnlyList<DashboardCustomerRiskForecastCustomerDto> TopCustomers { get; set; }
    public IReadOnlyList<DashboardCustomerRiskForecastAttentionDto> AttentionList { get; set; }
    public IReadOnlyList<DashboardCustomerRiskForecastRecommendationDto> Recommendations { get; set; }
    public DateTime GeneratedAt { get; set; }
}
```

### 8.2 Application query

**Folder:** `btr.application/ReportingContext/DashboardCustomerRiskForecastAgg/`

- `Queries/GetDashboardCustomerRiskForecastQuery.cs`
- Handler reads `IDashboardCustomerRiskForecastDal.GetCurrent()` — no business logic in handler

### 8.3 Controller

**File:** `btr.portal.api/Controllers/Dashboard/CustomerRiskForecastDashboardController.cs`

Thin — delegates to MediatR only.

---

## 9. Portal Web Implementation

### 9.1 Route and navigation

| Item | Value |
| ---- | ----- |
| Route | `/dashboard/customer-risk-forecast` |
| Sidebar label | Customer Risk Forecast |
| Position | After Customer Analytics, before Salesman Performance |
| Router | Lazy import in `src/router/index.ts` |

### 9.2 View structure

**File:** `CustomerRiskForecastDashboardView.vue`

Reuse patterns from `CashFlowForecastDashboardView.vue` and `CustomerDashboardView.vue`:

| Component | Purpose |
| --------- | ------- |
| `DashboardDetailLayout` | Shell, refresh, generated-at |
| `CustomerRiskForecastSummary` | Executive summary block |
| `CustomerRiskForecastKpiGrid` | 2 KPI rows (portfolio + signal mix) |
| `CustomerRiskForecastCategoryChart` | Donut — category distribution |
| `CustomerRiskForecastWilayahChart` | Bar — top wilayah |
| `CustomerRiskForecastSignalMixChart` | Stacked bar — signal types |
| `CustomerRiskForecastExposureChart` | Elevated risk vs total piutang |
| `CustomerRiskForecastCustomersTable` | Top 20 customers |
| `CustomerRiskForecastAttentionList` | Forecast attention signals |
| `CustomerRiskForecastRecommendations` | Action cards / table |

### 9.3 Store and API

| File | Change |
| ---- | ------ |
| `src/api/dashboardApi.ts` | `getCustomerRiskForecast()` |
| `src/models/dashboard.ts` | Response interfaces |
| `src/stores/dashboardStore.ts` | `loadCustomerRiskForecast()` |

### 9.4 Category badge colors

| Category | Token |
| -------- | ----- |
| Healthy | `severity-success` |
| Watch | `severity-info` |
| Attention | `severity-warning` |
| High Risk | `severity-critical` |
| Critical | `severity-critical` + bold |

### 9.5 Client signal helper (optional)

**File:** `src/services/customerRiskForecastSignals.ts`

Filter/label map for attention list — mirror `customerAttentionSignals.ts`.

### 9.6 Traceability footer

Links:

- `/dashboard/customers` — current-state customer analytics
- `/dashboard/piutang` — receivable exposure
- `/dashboard/collection` — recovery performance
- `/dashboard/cash-flow-forecast` — company liquidity forecast
- `/reports/piutang` — evidence
- `/reports/sales` — purchase evidence

Disclaimer:

> *Forecasts are indicative decision support based on deterministic business rules. BTR Portal does not modify credit limits, suspend customers, or schedule collections. Confirm account status in BTR Desktop before acting.*

---

## 10. Configuration

Extend `DashboardSnapshotOptions` (or nested section):

```json
"DashboardSnapshot": {
  "CustomerRiskForecastHorizonDays": 30,
  "CustomerRiskForecastPriorMonthOmzetFloorIdr": 1000000,
  "CustomerRiskForecastNoPaymentRecencyDays": 30,
  "CustomerRiskForecastPaymentLagLookbackDays": 90,
  "CustomerRiskForecastMinSettledFaktursForLag": 2,
  "CustomerRiskForecastMaxTopCustomers": 20,
  "CustomerRiskForecastMaxAttentionRows": 25,
  "CustomerRiskForecastMaxRecommendations": 15
}
```

Policy constants in `CustomerRiskForecastPolicy` read from options when injected; static defaults match analysis document.

---

## 11. Testing Strategy

### 11.1 Unit tests

**File:** `btr.test/ReportingContext/CustomerRiskForecastPolicyTest.cs`

| Test case | Validates |
| --------- | --------- |
| Projected open balance — zero omzet | Balance unchanged |
| Projected open balance — pace adds billing | Conservative projection |
| Decline ratio 65% | Moderate decline signal |
| Decline ratio 40% | Severe decline signal |
| Category Healthy — no signals | CRF-G02 |
| Category Critical — triple strong | §6.1 |
| Priority score ordering | High exposure ranks above low |
| Confidence bands | M27 parity |
| Portfolio health score edge — zero piutang | No divide-by-zero |

**File:** `CustomerRiskSignalBuilderTest.cs`

| Test case | Validates |
| --------- | --------- |
| Likely late payer — lag 10 days | CRF-P01 |
| Approaching dormant — 65 days | CRF-I01 |
| Projected plafond breach | CRF-C01 |
| Due concentration 20% | CRF-L02 |
| Dedupe same signal | One row per key |

**File:** `CustomerRiskRecommendationBuilderTest.cs`

| Test case | Validates |
| --------- | --------- |
| Critical → ManagementReview | Priority order |
| Due 5d + slow payer → EarlyCollection | |
| Severe decline → SalesRecovery | |

**File:** `DashboardCustomerRiskForecastAggregatorTest.cs`

| Test case | Validates |
| --------- | --------- |
| End-to-end synthetic portfolio | KPI counts match rows |
| CRF-KPI-50 total piutang traceability | |
| Customer without history excluded | CRF-G03 |
| Top 20 cap enforced | CRF-10 |

### 11.2 Integration tests

**File:** `DashboardCustomerSnapshotVerificationTest.cs` (extend)

- Round-trip forecast tables after refresh
- M17 tables unchanged when forecast added

### 11.3 Frontend tests

**File:** `customerRiskForecastSignals.spec.ts`

- Signal label mapping
- Category color mapping

---

## 12. Implementation Phases

### Phase 1 — Foundation (3–4 days)

1. SQL tables + upgrade script
2. DAL contracts + Infrastructure SQL (pelunasan summary, payment behavior, omzet history)
3. `CustomerRiskForecastPolicy` + unit tests
4. DI registration in `InfrastructurePortalExtensions.cs`

### Phase 2 — Aggregation (4–5 days)

1. Context DTOs + aggregate result models
2. `CustomerRiskSignalBuilder` + tests
3. `CustomerRiskRecommendationBuilder` + tests
4. `DashboardCustomerRiskForecastAggregator` + tests
5. `CustomerRiskExecutiveSummaryBuilder`
6. Extend `DashboardCustomerSnapshotDal` write path
7. Extend `RefreshDashboardCustomerSnapshotWorker`

### Phase 3 — API (1–2 days)

1. `IDashboardCustomerRiskForecastDal` read facade
2. MediatR query + handler
3. Controller
4. Snapshot verification test

### Phase 4 — Portal UI (3–4 days)

1. API model + store + route
2. View + components
3. Charts (reuse chart primitives from existing dashboards)
4. Navigation sidebar entry
5. Frontend signal spec

### Phase 5 — Documentation & UAT (1–2 days)

1. `docs/features/customer-risk-forecast/feature.md`
2. Update `btr-portal-operational.md` usage section
3. UAT against acceptance criteria
4. Knowledge extraction note

**Estimated total effort:** 12–17 developer days (1 implementer, excluding PO review cycles)

---

## 13. Acceptance Criteria

### 13.1 Functional

1. Dashboard accessible at `/dashboard/customer-risk-forecast` for authorized users.
2. Executive summary displays server-composed plain-language text.
3. All CRF-KPI-01 through CRF-KPI-07 render and match materialized snapshot.
4. Category distribution sums to forecasted customer count (excluding no-history customers).
5. Top customer table shows Category, Reason, Recommendation with drill-down routes.
6. Attention list shows only forecast signals (not M17 current-state duplicates without forward component).
7. Every customer row includes at least one `RuleId` traceable to analysis §9.
8. Forecast refreshes with Customer domain scheduler (~30 min) and manual admin refresh.

### 13.2 Traceability

1. CRF-KPI-07 equals sum of open balances from same piutang load (CRF-KPI-50).
2. Aging bucket assignment matches `PiutangAgingBucketResolver` for sample customers (CRF-KPI-51 spot check).
3. Dormant customers (90+ days) classified consistently with M17 when inactive.

### 13.3 Non-regression

1. `GET /api/dashboard/customers` response unchanged.
2. M17 snapshot tables byte-identical structure and M17 KPI values unchanged after refresh.
3. Customer refresh duration increase ≤ 20s on staging dataset (measure and document).

### 13.4 Quality

1. All unit tests pass.
2. No business logic in API controller or read DAL.
3. UI disclaimer visible on dashboard footer.

---

## 14. Database Impact

| Object | Impact |
| ------ | ------ |
| `BTR_Faktur` | Read-only — omzet history aggregate |
| `BTR_Piutang` / `BTR_PiutangLunas` | Read-only — payment behavior |
| `BTR_Customer` | Read-only — plafond, wilayah |
| `BTRPD_Customer*` | Unchanged |
| `BTRPD_CustomerRiskForecast*` | 7 new tables |

**Optional index:**

```sql
-- If payment behavior query exceeds SLA
CREATE NONCLUSTERED INDEX IX_PiutangLunas_LunasDate
ON BTR_PiutangLunas (LunasDate)
INCLUDE (PiutangId, JenisLunas);
```

---

## 15. Performance Considerations

| Concern | Mitigation |
| ------- | ---------- |
| Three new aggregate queries | Run once per refresh; bounded GROUP BY customer |
| In-memory customer loop | O(customers with history) — typically < 10k iterations |
| Single transaction write | 7 child tables + M17 — same pattern as Collection+M27 |
| API read | 1 KPI + bounded lists — p95 target < 500ms |
| Worker increment | Target < 10s additional on typical dataset |

---

## 16. DI Registration Checklist

**File:** `btr.infrastructure/Portal/InfrastructurePortalExtensions.cs`

Register:

- `ICustomerPelunasanSummaryDal` → `CustomerPelunasanSummaryDal`
- `ICustomerPaymentBehaviorDal` → `CustomerPaymentBehaviorDal`
- `ICustomerOmzetHistoryDal` → `CustomerOmzetHistoryDal`
- `IDashboardCustomerRiskForecastDal` → `DashboardCustomerRiskForecastDal`
- `DashboardCustomerRiskForecastAggregator` (scoped/transient per existing pattern)

**File:** `btr.application` DI — register builders and policy if instance-based options injection required.

**File:** `btr.portal.api` — controller auto-registration.

---

## 17. File Manifest (expected new files)

```text
btr.application/
  ReportingContext/DashboardSnapshotAgg/Services/
    CustomerRiskForecastPolicy.cs
    CustomerRiskSignalBuilder.cs
    CustomerRiskRecommendationBuilder.cs
    CustomerRiskExecutiveSummaryBuilder.cs
    DashboardCustomerRiskForecastAggregator.cs
  ReportingContext/DashboardSnapshotAgg/Models/
    DashboardCustomerRiskForecastAggregateResult.cs
    CustomerRiskForecastContext.cs
    (+ row DTOs)
  ReportingContext/DashboardCustomerRiskForecastAgg/
    Contracts/IDashboardCustomerRiskForecastDal.cs
    Queries/GetDashboardCustomerRiskForecastQuery.cs
  FinanceContext/PiutangAgg/Contracts/
    ICustomerPelunasanSummaryDal.cs
    ICustomerPaymentBehaviorDal.cs
  SalesContext/FakturInfo/
    ICustomerOmzetHistoryDal.cs

btr.infrastructure/
  FinanceContext/PiutangAgg/
    CustomerPelunasanSummaryDal.cs
    CustomerPaymentBehaviorDal.cs
  SalesContext/FakturInfoAgg/
    CustomerOmzetHistoryDal.cs
  ReportingContext/DashboardCustomerRiskForecastAgg/
    DashboardCustomerRiskForecastDal.cs

btr.portal.api/Controllers/Dashboard/
  CustomerRiskForecastDashboardController.cs

btr.portal.web/src/
  views/dashboard/CustomerRiskForecastDashboardView.vue
  components/dashboard/customer-risk-forecast/*.vue
  services/customerRiskForecastSignals.ts

btr.test/ReportingContext/
  CustomerRiskForecastPolicyTest.cs
  CustomerRiskSignalBuilderTest.cs
  CustomerRiskRecommendationBuilderTest.cs
  DashboardCustomerRiskForecastAggregatorTest.cs

btr.sql/Scripts/
  Upgrade_M29_CustomerRiskForecast.sql
```

---

## Document Maintenance

When implemented:

1. Create `docs/features/customer-risk-forecast/feature.md`
2. Update `docs/features/btr-portal/btr-portal-domain.md` — move M29 from future to current
3. Add operational usage to `btr-portal-operational.md`
4. Archive this plan after `implementation-summary-m29-customer-risk-forecast.md` is written

**Success criterion:** An Implementer can begin coding with minimal ambiguity after Product Owner approves the analysis document.
