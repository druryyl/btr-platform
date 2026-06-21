# Implementation Plan — M28 Inventory Forecast Dashboard

## Document Status

| Field | Value |
| ----- | ----- |
| Milestone | M28 — Inventory Forecast Dashboard |
| Authoritative requirements | [portal-analysis-m28-inventory-forecast.md](./portal-analysis-m28-inventory-forecast.md) |
| Reference pattern | M19 Inventory Risk, M26 Sales Forecast, M27 Cash Flow Forecast, materialized-dashboard architecture |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |
| Status | **Ready for implementation** (pending Product Owner approval of analysis) |

**Prerequisite:** Product Owner approval of analysis document (forecast algorithm, KPI set, wireframe, business rules).

---

## 1. Goal

Deliver a read-only **Inventory Forecast Dashboard** at `/dashboard/inventory-forecast` that projects **days of supply**, **stock-out timing**, and **purchasing review recommendations** using **Rolling 30-Day Average Daily Consumption (ADC) with Linear Stock Depletion** — deterministic, explainable, and traceable to the same stock balance and Faktur-item data as Inventory and Inventory Risk dashboards.

**In scope:**

- New forecast aggregation during existing **Inventory Risk** snapshot refresh
- New snapshot tables for forecast KPIs, daily consumption pace, projected level series, risk rows, and purchase recommendation rows
- New item consumption load (`BrgId` sales qty aggregation) during refresh
- New API endpoint `GET /api/dashboard/inventory-forecast`
- New Portal Web dashboard page with KPI rows, charts, executive summary, risk and recommendation tables
- Unit tests for forecast policy and aggregator

**Out of scope:**

- Changes to existing `/dashboard/inventory` or `/dashboard/inventory-risk` API responses
- Per-warehouse / per-depo forecast
- Automatic purchase order creation
- Kartu Stok / `BTR_StokMutasi` consumption source
- Lead time per SKU/supplier from master data
- Alert Center / Executive integration
- Item-level sales report in portal
- AI / ML models

---

## 2. Impact Analysis

### 2.1 Business areas affected

| Area | Impact |
| ---- | ------ |
| Inventory | Primary — position, consumption, projected depletion |
| Sales | Read-only — Faktur item qty as demand signal |
| Purchasing | Read-only cross-link — recommendations are decision support only |
| Master Data | Read-only — `BTR_Brg.IsAktif` filter |

### 2.2 Systems affected

| System | Impact |
| ------ | ------ |
| BTR Portal API | New controller + MediatR query |
| BTR Portal Web | New route, view, components, store method |
| BTR Portal Worker | Extended Inventory Risk snapshot refresh |
| BTR SQL | New snapshot tables |
| BTR Desktop | None |

### 2.3 Existing features — preserve behavior

| Feature | Change |
| ------- | ------ |
| Inventory Dashboard (`/dashboard/inventory`) | **None** |
| Inventory Risk Dashboard (`/dashboard/inventory-risk`) | **None** |
| Inventory Risk refresh cadence | **Unchanged** (~60 min) |
| `GET /api/dashboard/inventory` | **Unchanged** |
| `GET /api/dashboard/inventory-risk` | **Unchanged** |
| `DashboardInventoryRiskAggregator` M19 outputs | **Unchanged** — forecast is additive step |
| `DashboardInventoryItemGroupBuilder` | **Reused**, not modified |

### 2.4 Source code modules affected

| Layer | Module | Change type |
| ----- | ------ | ----------- |
| Application | `ReportingContext/DashboardSnapshotAgg/Services/` | **Add** `DashboardInventoryForecastAggregator`, `InventoryForecastPolicy`, `InventoryConsumptionGrouper`, `InventoryForecastRiskBuilder` |
| Application | `ReportingContext/DashboardSnapshotAgg/Models/` | **Add** inventory forecast aggregate DTOs |
| Application | `ReportingContext/DashboardSnapshotAgg/UseCases/` | **Extend** `RefreshDashboardInventoryRiskSnapshotWorker` |
| Application | `ReportingContext/DashboardSnapshotAgg/Contracts/` | **Extend** `IDashboardInventoryRiskSnapshotDal` |
| Application | `ReportingContext/DashboardInventoryForecastAgg/` | **New** query + read contract |
| Application | `SalesContext/FakturBrgInfo/` or `FakturInfo/` | **Add** `IBrgConsumptionDal` contract + `BrgConsumptionDto` |
| Infrastructure | `SalesContext/FakturInfoAgg/` | **Add** `BrgConsumptionDal` (aggregated SQL) |
| Infrastructure | `ReportingContext/DashboardSnapshotAgg/` | **Extend** `DashboardInventoryRiskSnapshotDal` |
| Infrastructure | `ReportingContext/DashboardInventoryForecastAgg/` | **New** read DAL |
| SQL | `btr.sql/Scripts/` or `Tables/ReportingContext/` | **Add** forecast tables |
| Portal API | `Controllers/Dashboard/` | **Add** `InventoryForecastDashboardController` |
| Portal API | `Configurations/` | Register new services |
| Portal Web | `views/dashboard/`, `components/dashboard/`, `stores/`, `api/`, `router/` | **Add** forecast page + components |
| Tests | `btr.test/ReportingContext/` | **Add** policy + aggregator + traceability tests |

### 2.5 Risk summary

| Risk | Severity | Mitigation |
| ---- | -------- | ---------- |
| Consumption scan performance (90-day FakturItem) | Medium | Single aggregated SQL in `BrgConsumptionDal`; 600s timeout pattern from `BrgLastFakturDal` |
| Forecast inventory value diverges from M15 | High | Same `BuildItemGroups` in one worker load; IFR-50 traceability test |
| Early-month / sparse-history volatility | Medium | IFR-20 fallback + Forecast Confidence indicator |
| No portal item sales report for drill-down | Low | Footer links Inventory Report + Desktop path documented |
| Gross consumption overstates demand when retur high | Low | Document IFR-13; net demand deferred |
| Large item universe — snapshot row volume | Medium | Top 10 risks + Top 10 recommendations only at item grain |
| `FakturBrgViewDal` missing `BrgId` | Medium | New `BrgConsumptionDal` — do not overload report-oriented view DAL |

---

## 3. Architecture Overview

### 3.1 Topology

```text
Task Scheduler (InventoryRisk domain, ~60 min)
        ↓
RefreshDashboardInventoryRiskSnapshotWorker
        ↓
Load: IStokBalanceViewDal.ListData()
      IBrgLastFakturDal.ListLastFakturByBrg()
      IBrgConsumptionDal.ListConsumptionByBrg(rolling 30d + 90d)   ← new
      IBrgMasterDal or Brg IsAktif filter                           ← new minimal load
        ↓
DashboardInventoryRiskAggregator.Aggregate()        → BTRPD_InventoryRisk* (unchanged)
        ↓
DashboardInventoryForecastAggregator.Aggregate()      → BTRPD_InventoryForecast* (new)
        ↓
DashboardInventoryRiskSnapshotDal.ReplaceCurrent()    → single transaction

Browser → GET /api/dashboard/inventory-forecast
        ↓ MediatR
GetDashboardInventoryForecastHandler
        ↓ IDashboardInventoryForecastDal
DashboardInventoryForecastDal → snapshot SELECT
```

**Design decision:** Extend the **existing Inventory Risk snapshot refresh** (same pattern as M27 extending Collection). Forecast shares stock balance, last-Faktur, and item-group builder output in memory — guarantees IFR-50/51 traceability and avoids duplicate expensive scans.

**Alternative rejected:** Separate `InventoryForecast` worker domain — would duplicate balance + last-Faktur load and allow temporal skew between Inventory Risk and Inventory Forecast dashboards.

### 3.2 Layering

```text
btr.portal.api          → InventoryForecastDashboardController (thin)
btr.application         → GetDashboardInventoryForecastQuery + Handler
                          → DashboardInventoryForecastAggregator + InventoryForecastPolicy
                          → IDashboardInventoryForecastDal (contract)
btr.infrastructure      → DashboardInventoryForecastDal (read facade)
                          → BrgConsumptionDal
                          → DashboardInventoryRiskSnapshotDal (extended write)
btr.portal.web          → InventoryForecastDashboardView.vue + components
```

MediatR pattern preserved. No business logic in controller.

### 3.3 New policy class

**File:** `btr.application/ReportingContext/DashboardSnapshotAgg/Services/InventoryForecastPolicy.cs`

Mirror `SalesForecastPolicy` / `CashFlowForecastPolicy` structure:

```csharp
public static class InventoryForecastPolicy
{
    public static InventoryForecastCalculation ComputeItem(
        decimal currentQty,
        decimal unitHpp,
        decimal soldQty30,
        decimal soldQty90,
        int planningHorizonDays,
        int defaultLeadTimeDays,
        int coverageDays,
        DateTime businessDate);

    public static decimal ComputeAdc30(decimal soldQty30);
    public static decimal ComputeAdc90(decimal soldQty90);
    public static decimal? ComputeDaysOfSupply(decimal qty, decimal adc);
    public static DateTime? ComputeProjectedStockOutDate(DateTime businessDate, decimal? daysOfSupply);
    public static DateTime? ComputeReorderDate(DateTime? stockOutDate, int leadTimeDays);
    public static decimal ComputeRecommendedPurchaseQty(decimal qty, decimal adc, int leadTimeDays, int coverageDays);
    public static decimal ComputeForecastQtyAtHorizon(decimal qty, decimal adc, int horizonDays);
    public static decimal ComputeProjectedValue(decimal forecastQty, decimal unitHpp);

    public static decimal ComputeBestCaseAdc(decimal adc30, decimal adc90);
    public static decimal ComputeWorstCaseAdc(decimal adc30, decimal adc90);

    public static string ResolveConfidence(int horizonDays, decimal companySoldQty30);
    public static string ResolveDosSeverity(decimal? daysOfSupply);
    public static string ResolvePurchaseUrgency(decimal? daysOfSupply, DateTime? reorderDate, DateTime businessDate);
    public static int ComputeHealthScore(decimal stockOutRiskPct, decimal overstockValuePct, decimal atRiskInventoryPct);
}
```

**DOS severity bands (configurable defaults):**

| Band | Days of Supply |
| ---- | -------------- |
| Critical | ≤ 7 |
| Warning | 8–14 |
| Normal | > 14 |

**Forecast confidence:**

| Condition | Confidence |
| --------- | ---------- |
| Company `S₃₀` = 0 or horizon day 1–5 with sparse data | Low |
| Normal operations | Medium |
| ≥ 21 days into rolling window with stable item coverage | High |

### 3.4 Consumption DAL

**File:** `btr.infrastructure/SalesContext/FakturInfoAgg/BrgConsumptionDal.cs`

**Contract:** `IBrgConsumptionDal.ListConsumptionByBrg(DateTime window30Start, DateTime window90Start, DateTime windowEnd)`

```sql
SELECT
    fi.BrgId,
    SUM(CASE WHEN aa.FakturDate BETWEEN @Start30 AND @End THEN fi.QtyJual ELSE 0 END) AS SoldQty30,
    SUM(CASE WHEN aa.FakturDate BETWEEN @Start90 AND @End THEN fi.QtyJual ELSE 0 END) AS SoldQty90,
    MIN(aa.FakturDate) AS FirstFakturDate
FROM BTR_Faktur aa
INNER JOIN BTR_FakturItem fi ON aa.FakturId = fi.FakturId
WHERE aa.VoidDate = '3000-01-01'
  AND aa.FakturDate BETWEEN @Start90 AND @End
GROUP BY fi.BrgId
```

**Rationale:** Aggregated SQL avoids loading full `FakturBrgView` row set into memory. `BrgId` not present on current `FakturBrgView` model — do not extend report DAL for dashboard-only aggregation.

**Optional daily pace:** `InventoryConsumptionGrouper.BuildDailyTotals` groups `QtyJual` by `FakturDate` for last 30 days (company total or top-N items only for chart performance — **company total** for V1 chart).

### 3.5 New aggregator

**File:** `btr.application/ReportingContext/DashboardSnapshotAgg/Services/DashboardInventoryForecastAggregator.cs`

**Inputs:**

- `IEnumerable<StokBalanceView> rows`
- `IEnumerable<BrgLastFakturDto> lastFakturRows`
- `IEnumerable<BrgConsumptionDto> consumptionRows`
- `IEnumerable<BrgActiveDto> activeFlags` (or join `IsAktif` in consumption query)
- `DashboardInventoryRiskAggregateResult riskResult` (for at-risk % traceability)
- `DateTime businessDate`, `DateTime generatedAt`
- `InventoryForecastOptions` (horizon, lead time, thresholds)

**Responsibilities:**

1. `BuildItemGroups(rows)` — shared builder
2. Classify each item (reuse M19 classifier for eligibility)
3. Per eligible item: compute ADC, DOS, stock-out, reorder, recommended qty via policy
4. Roll up company KPIs (current value, projected value, understock/overstock values, health score)
5. Build scenario bands (best/expected/worst projected value)
6. Build daily consumption pace series (30 days)
7. Build projected inventory level series (days 0..H)
8. `InventoryForecastRiskBuilder.BuildTopRisks` — priority ordered Top 10
9. `InventoryForecastRiskBuilder.BuildPurchaseRecommendations` — Top 10 by urgency

**Eligibility filter (IFR-08):**

```text
IsAktif = true
AND Qty > 0
AND NOT NeverSold
AND NOT DeadStock (≥ 180d idle)
```

### 3.6 Risk builder

**File:** `btr.application/ReportingContext/DashboardSnapshotAgg/Services/InventoryForecastRiskBuilder.cs`

Parallel to `CashFlowCollectionRiskBuilder`:

- Deterministic signal keys from analysis §7
- Sort priority from analysis §7.3
- Cap at 10 rows per table
- Include `ReportRoute` = `/reports/inventory` with `EntityCode` for client `?q=` filter

---

## 4. API Design

### 4.1 Endpoint

```
GET /api/dashboard/inventory-forecast
```

**Auth:** Same as other dashboard endpoints.

**Response:** `DashboardInventoryForecastResponse`

| Section | Content |
| ------- | ------- |
| `Metadata` | `GeneratedAt`, `PeriodYear`, `PeriodMonth`, `BusinessDate`, `PlanningHorizonDays`, `IsAvailable` |
| `Kpis` | Company-level forecast KPIs |
| `ScenarioBands` | Best / Expected / Worst projected values |
| `DailyConsumption` | Last 30 days company units sold + ADC line |
| `ProjectedLevel` | Days 0..H value series |
| `TopRisks` | Top 10 inventory risk rows |
| `PurchaseRecommendations` | Top 10 recommendation rows |
| `ExecutiveSummary` | Server-composed plain-language sentence |
| `Traceability` | Links + reconciliation footnotes |

**Unavailable snapshot:** `IsAvailable = false`, HTTP 200 (match M26/M27).

### 4.2 MediatR

```text
GetDashboardInventoryForecastQuery
GetDashboardInventoryForecastHandler
  → IDashboardInventoryForecastDal.GetCurrent()
```

### 4.3 Controller

```text
InventoryForecastDashboardController
  [Route("api/dashboard/inventory-forecast")]
  [HttpGet] → MediatR send
```

---

## 5. ReportingContext Additions

### 5.1 Application contracts

| Type | Purpose |
| ---- | ------- |
| `IDashboardInventoryForecastDal` | Read facade for API |
| `IBrgConsumptionDal` | Item consumption aggregation |
| `BrgConsumptionDto` | `BrgId`, `SoldQty30`, `SoldQty90`, `FirstFakturDate` |
| `DashboardInventoryForecastAggregateResult` | Worker output |
| `DashboardInventoryForecastKpiSnapshot` | Company KPIs |
| `DashboardInventoryForecastDailyConsumptionRow` | Pace chart |
| `DashboardInventoryForecastLevelRow` | Depletion series |
| `DashboardInventoryForecastRiskRow` | Top risks |
| `DashboardInventoryForecastRecommendationRow` | Purchase hints |

### 5.2 Options

Extend `DashboardSnapshotOptions`:

```json
"DashboardSnapshot": {
  "InventoryForecastPlanningHorizonDays": 30,
  "InventoryForecastDefaultLeadTimeDays": 7,
  "InventoryForecastCoverageDays": 14,
  "InventoryForecastOverstockDosDays": 90,
  "InventoryForecastMinDosHealthy": 30,
  "InventoryForecastStockOutCriticalDays": 7
}
```

### 5.3 Worker extension

**File:** `RefreshDashboardInventoryRiskSnapshotWorker.cs`

Add steps after existing aggregate:

```text
Load consumption (30d + 90d windows)
Load Brg IsAktif flags (if not in consumption query)
AggregateForecast
Save → ReplaceCurrent(aggregate, forecast, refreshLogId)
```

**Domain label:** Remains `"InventoryRisk"` — forecast tables are child of same refresh log (match M27 `Collection` domain label pattern).

**Refresh order in `All`:** Unchanged — `Piutang → Inventory → InventoryRisk → Sales → …`

---

## 6. Materialized Data Impact

### 6.1 New tables

| Table | Layer | Content |
| ----- | ----- | ------- |
| `BTRPD_InventoryForecastKpi` | A | Company forecast KPIs, scenario bands, health score, confidence |
| `BTRPD_InventoryForecastDailyConsumption` | B | Daily units sold (30d lookback) + ADC reference |
| `BTRPD_InventoryForecastLevel` | B | Projected inventory value days 0..H |
| `BTRPD_InventoryForecastRisk` | B | Top 10 risk rows |
| `BTRPD_InventoryForecastRecommendation` | B | Top 10 purchase recommendation rows |

**Snapshot key:** `CURRENT` — delete-and-replace each refresh (consistent with all portal domains).

### 6.2 KPI table columns (indicative)

`BTRPD_InventoryForecastKpi`:

- `SnapshotKey`, `GeneratedAt`, `BusinessDate`, `PlanningHorizonDays`
- `CurrentInventoryValue`, `ProjectedInventoryValue`, `BestCaseProjectedValue`, `WorstCaseProjectedValue`
- `AverageDailyConsumptionUnits`, `WeightedAverageDaysOfSupply`
- `UnderstockValue`, `OverstockValue`, `StockOutRiskItemCount`
- `InventoryCoveragePercent`, `InventoryTurnoverForecast`, `InventoryHealthScore`
- `ForecastConfidence`, `AtRiskInventoryPercent` (traceability copy from M19)
- `ForecastConsumptionUnits`
- `LastRefreshLogId`

### 6.3 SQL scripts

Add to `btr.sql/Scripts/Create_BTRPD_PortalDashboard_Tables.sql` (or incremental migration script):

- `CREATE TABLE` statements with `SnapshotKey` FK pattern matching `BTRPD_CashFlowForecastKpi`
- Indexes on `SnapshotKey` only (CURRENT singleton pattern)

### 6.4 Existing tables

**No changes** to `BTRPD_Inventory*` or `BTRPD_InventoryRisk*`.

---

## 7. Snapshot Refresh Strategy

| Aspect | Choice |
| ------ | ------ |
| Worker | `RefreshDashboardInventoryRiskSnapshotWorker` extended |
| Cadence | 60 minutes (`InventoryRiskIntervalMinutes`) |
| Transaction | Single `ReplaceCurrent` — M19 + M28 tables in one transaction |
| Manual refresh | Existing `POST /api/admin/dashboard/refresh?domain=InventoryRisk` rebuilds forecast too |
| CLI | `btr.portal.worker --domain InventoryRisk` |
| Presentation Mode | `IBusinessDateProvider.Today` drives horizon and consumption windows |

---

## 8. Database Impact

| Object | Impact |
| ------ | ------ |
| `BTR_Faktur` / `BTR_FakturItem` | Read-only aggregate scan — no schema change |
| `BTR_StokBalanceWarehouse` / view | Read-only — no schema change |
| `BTR_Brg` | Read-only `IsAktif` — no schema change |
| `BTRPD_*` | 5 new tables |

**Index recommendation (optional, DBA review):**

- Existing Faktur indexes on `FakturDate`, `VoidDate` usually sufficient
- If refresh exceeds SLA, consider covering index on `(VoidDate, FakturDate)` include `FakturId` for consumption query

---

## 9. Performance Considerations

| Concern | Mitigation |
| ------- | ---------- |
| Full FakturItem 90-day scan each hour | Pre-aggregated SQL; only grouped rows returned (~SKU count not line count) |
| Item groups already in memory from M19 step | Reuse list — no second balance load |
| Large SKU count (10k+) | Top 10 materialized only; company roll-up is O(n) in memory |
| API read | Single KPI row + small child tables — p95 target < 500ms |
| Worker duration increase | Acceptable within 60-min cadence; log `RecordCount` on consumption load |

**Target:** Worker increment < 30s on typical distributor dataset (assumption — measure in UAT).

---

## 10. UI Implementation Plan

### 10.1 Route and navigation

| Item | Value |
| ---- | ----- |
| Route | `/dashboard/inventory-forecast` |
| Sidebar label | Inventory Forecast |
| Position | After Slow Moving & Dead Stock, before Purchasing |
| Router module | `src/router/index.ts` lazy import |

### 10.2 View structure

**File:** `InventoryForecastDashboardView.vue`

Reuse patterns from `CashFlowForecastDashboardView.vue` and `InventoryRiskDashboardView.vue`:

| Component | Purpose |
| --------- | ------- |
| `DashboardDetailLayout` | Shell, refresh, generated-at |
| `InventoryForecastSummary` | Executive summary sentence |
| `InventoryForecastKpiGrid` | 3 KPI rows (reuse `SalesForecastKpiRow` severity pattern) |
| `InventoryForecastLevelChart` | Projected value depletion (new — line/area) |
| `InventoryConsumptionTrendChart` | 30d bars + ADC line (new) |
| `InventoryRiskHeatSummary` | 3×3 DOS × value heat (new, simple CSS grid) |
| `InventoryForecastRisksTable` | Top risks (pattern from `CashFlowCollectionRisksTable`) |
| `InventoryPurchaseRecommendationsTable` | Top 10 recommendations (new) |

### 10.3 Store and API

| File | Change |
| ---- | ------ |
| `src/api/dashboardApi.ts` | `getInventoryForecast()` |
| `src/models/dashboard.ts` | `DashboardInventoryForecastResponse` interfaces |
| `src/stores/dashboardStore.ts` | `loadInventoryForecast()`, state slice |

### 10.4 Traceability footer

Links:

- `/dashboard/inventory` — current position
- `/dashboard/inventory-risk` — obsolescence context
- `/reports/inventory` — evidence
- `/dashboard/purchasing-management` — posting backlog context

Disclaimer text for recommended purchase qty (analysis §8).

---

## 11. Testing Plan

### 11.1 Unit tests

| Test class | Coverage |
| ---------- | -------- |
| `InventoryForecastPolicyTest` | ADC, DOS, stock-out date, reorder date, recommended qty, scenario ADC, health score, severity bands |
| `DashboardInventoryForecastAggregatorTest` | Eligibility filter, roll-up values, Top 10 ordering, never-sold exclusion, inactive exclusion |
| `InventoryForecastRiskBuilderTest` | Priority ordering, signal assignment |

### 11.2 Traceability tests

| Test | Rule |
| ---- | ---- |
| `InventoryForecastInventoryTraceabilityTest` | IFR-50 — `CurrentInventoryValue` = aggregator `TotalInventoryValue` on same fixture |
| `InventoryForecastRiskTraceabilityTest` | IFR-51 — at-risk % matches M19 aggregator on same fixture |

### 11.3 Integration smoke

- Worker `InventoryRisk` domain completes with forecast tables populated
- API returns `IsAvailable = true` after refresh
- UI renders without console errors

---

## 12. Step-by-Step Implementation Plan

### Phase A — SQL and contracts (1–2 days)

1. Add `BTRPD_InventoryForecast*` table definitions to SQL scripts.
2. Add DTOs and `DashboardInventoryForecastAggregateResult` models.
3. Add `IBrgConsumptionDal` + `BrgConsumptionDto`.
4. Extend `DashboardSnapshotOptions` with inventory forecast settings.
5. Extend `IDashboardInventoryRiskSnapshotDal` / read contract `IDashboardInventoryForecastDal`.

### Phase B — Domain logic (2–3 days)

6. Implement `BrgConsumptionDal` with aggregated SQL.
7. Implement `InventoryForecastPolicy` with full unit tests.
8. Implement `InventoryConsumptionGrouper` for daily pace buckets.
9. Implement `InventoryForecastRiskBuilder`.
10. Implement `DashboardInventoryForecastAggregator` with unit tests.

### Phase C — Snapshot pipeline (1–2 days)

11. Extend `RefreshDashboardInventoryRiskSnapshotWorker` — load consumption + active flags.
12. Extend `DashboardInventoryRiskSnapshotDal.ReplaceCurrent` — write forecast tables in same transaction.
13. Register services in `ApplicationPortalExtensions` / `InfrastructurePortalExtensions`.
14. Verify worker via CLI on dev database.

### Phase D — API (1 day)

15. Add `GetDashboardInventoryForecastQuery` + handler.
16. Implement `DashboardInventoryForecastDal` read facade.
17. Add `InventoryForecastDashboardController`.
18. Add executive summary composer (server-side string).

### Phase E — UI (2–3 days)

19. Add TypeScript models and API client method.
20. Extend dashboard store.
21. Build chart and table components.
22. Build `InventoryForecastDashboardView.vue`.
23. Add route and sidebar entry.
24. Build `npm run build` verification.

### Phase F — Verification and documentation (1 day)

25. Run full unit test suite.
26. Manual UAT: reconcile Current Inventory Value with Inventory Report footer.
27. Write `docs/features/inventory-forecast/feature.md` (Knowledge Curator).
28. Update `btr-portal-domain.md` roadmap section (M28 → Current).

**Estimated effort:** 8–12 developer days.

---

## 13. Future Extensibility (Architecture Hooks)

| Extension | Hook |
| --------- | ---- |
| Warehouse grain | Add `WarehouseId` to `BrgConsumptionDal` signature + item groups from warehouse-scoped balance |
| Supplier lead time | `BTRPD_InventoryForecastRecommendation.LeadTimeDays` column sourced from future master |
| Net consumption | Optional parameter on consumption DAL `NetOfRetur` |
| Alert Center | Read `BTRPD_InventoryForecastRisk` in `DashboardAlertCenterComposer` |
| EOQ simulation | Separate `InventoryReplenishmentSimulationPolicy` — no change to V1 tables |
| Executive promotion | Read subset of `BTRPD_InventoryForecastKpi` in `DashboardExecutiveComposer` |

V1 table shapes deliberately include `BrgId`, `SupplierName`, `SignalKey`, `ValueAmount` on row tables to support future Alert Center consumption without schema migration.

---

## 14. Acceptance Criteria

1. `/dashboard/inventory-forecast` loads from materialized snapshot; p95 API < 500ms.
2. Forecast uses Rolling 30-Day ADC with linear depletion — no ML/AI.
3. `CurrentInventoryValue` reconciles with Inventory Dashboard / Report footer (IFR-50).
4. `AtRiskInventoryPercent` reconciles with Inventory Risk snapshot when refreshed together (IFR-51).
5. Inactive and dead-stock items excluded from stock-out projection per IFR-08/09.
6. Top 10 risks and Top 10 purchase recommendations are deterministic and priority-ordered.
7. Executive summary and confidence indicator render on UI.
8. Existing Inventory and Inventory Risk dashboards unchanged.
9. Unit tests pass for policy, aggregator, and traceability.
10. `GeneratedAt` reflects Inventory Risk refresh timestamp.

---

## Document Maintenance

After implementation, Knowledge Curator promotes durable rules to:

- `docs/features/inventory-forecast/feature.md`
- `docs/features/materialized-dashboard/materialized-dashboard-domain.md` (Inventory Forecast extension section)
- `docs/features/btr-portal/btr-portal-domain.md` (Section 8 dashboards, Section 7 KPIs)

Temporary work artifacts remain in `docs/work/btr-portal/` until curator pass completes.
