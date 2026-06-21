# Implementation Plan — M28.5 Inventory Optimization Dashboard

## Document Status

| Field | Value |
| ----- | ----- |
| Milestone | M28.5 — Inventory Optimization Dashboard |
| Authoritative requirements | [portal-analysis-m28-5-inventory-optimization.md](./portal-analysis-m28-5-inventory-optimization.md) |
| Reference pattern | M28 Inventory Forecast, M27 Cash Flow Forecast, M21 Purchasing Management, M22 Location, materialized-dashboard architecture |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |
| Status | **Ready for implementation** (pending Product Owner approval of analysis) |

**Prerequisite:** Product Owner approval of analysis document (recommendation types, priority rules, wireframe, business rules).

**Prerequisite implementation:** M28 Inventory Forecast Dashboard complete ([implementation-summary-m28-inventory-forecast.md](./implementation-summary-m28-inventory-forecast.md)).

---

## 1. Goal

Deliver a read-only **Inventory Optimization Dashboard** at `/dashboard/inventory-optimization` that converts M28 forecast outputs, M19 risk classification, and M21 purchasing signals into **deterministic, explainable action recommendations** — purchase, defer, delay, transfer, post-first, promote, clearance — ranked by integer priority score.

**In scope:**

- New optimization aggregation during existing **Inventory Risk** snapshot refresh (after M28 forecast step)
- New snapshot tables for optimization KPIs, unified action rows, and specialized recommendation lists
- New warehouse-scoped consumption load for transfer recommendations only
- Cross-read M21 purchasing management snapshot at refresh for qualified backlog
- New API endpoint `GET /api/dashboard/inventory-optimization`
- New Portal Web dashboard page with executive summary, KPIs, charts, and action tables
- Unit tests for optimization policy and aggregator

**Out of scope:**

- Changes to M28 `/api/dashboard/inventory-forecast` response shape
- Automatic purchase order or mutasi creation
- Optimization solvers, ML, or probabilistic scoring
- User-entered budget in portal UI (optional appsettings cap only)
- Alert Center / Executive integration
- Per-warehouse forecast dashboard (warehouse grain limited to transfer logic)

---

## 2. Impact Analysis

### 2.1 Business areas affected

| Area | Impact |
| ---- | ------ |
| Inventory | Primary — recommendations for replenishment, transfer, clearance |
| Sales | Read-only — warehouse-scoped Faktur consumption for transfer logic |
| Purchasing | Read-only cross-read — backlog, delay, post-first signals |
| Master Data | Read-only — `IsAktif`, warehouse active flags |

### 2.2 Systems affected

| System | Impact |
| ------ | ------ |
| BTR Portal API | New controller + MediatR query |
| BTR Portal Web | New route, view, components, store method |
| BTR Portal Worker | Extended Inventory Risk snapshot refresh (post-M28 step) |
| BTR SQL | New snapshot tables |
| BTR Desktop | None |

### 2.3 Existing features — preserve behavior

| Feature | Change |
| ------- | ------ |
| Inventory Forecast Dashboard | **None** — optimization reads same refresh context |
| Inventory Risk Dashboard | **None** |
| M28 forecast tables | **None** — optimization is additive |
| Inventory Risk refresh cadence | **Unchanged** (~60 min) |
| `GET /api/dashboard/inventory-forecast` | **Unchanged** |

### 2.4 Source code modules affected

| Layer | Module | Change type |
| ----- | ------ | ----------- |
| Application | `ReportingContext/DashboardSnapshotAgg/Services/` | **Add** `DashboardInventoryOptimizationAggregator`, `InventoryOptimizationPolicy`, `InventoryOptimizationRecommendationBuilder`, `WarehouseBalanceRecommendationBuilder` |
| Application | `ReportingContext/DashboardSnapshotAgg/Models/` | **Add** optimization aggregate DTOs |
| Application | `ReportingContext/DashboardSnapshotAgg/UseCases/` | **Extend** `RefreshDashboardInventoryRiskSnapshotWorker` |
| Application | `ReportingContext/DashboardSnapshotAgg/Contracts/` | **Extend** `IDashboardInventoryRiskSnapshotDal` |
| Application | `ReportingContext/DashboardInventoryOptimizationAgg/` | **New** query + read contract |
| Application | `SalesContext/FakturInfo/` | **Extend** `IBrgConsumptionDal` or **Add** `IBrgWarehouseConsumptionDal` |
| Infrastructure | `SalesContext/FakturInfoAgg/` | **Add** warehouse consumption SQL |
| Infrastructure | `ReportingContext/DashboardSnapshotAgg/` | **Extend** snapshot DAL write/read |
| SQL | `btr.sql/Scripts/` | **Add** optimization tables |
| Portal API | `Controllers/Dashboard/` | **Add** `InventoryOptimizationDashboardController` |
| Portal Web | `views/dashboard/`, `components/dashboard/`, `stores/`, `api/`, `router/` | **Add** optimization page + components |
| Tests | `btr.test/ReportingContext/` | **Add** policy + aggregator + traceability tests |

### 2.5 Risk summary

| Risk | Severity | Mitigation |
| ---- | -------- | ---------- |
| Duplicate ADC calculation vs M28 | High | Pass M28 `ForecastItemContext` list into optimization aggregator — single compute pass |
| Warehouse consumption scan performance | Medium | Aggregated SQL `GROUP BY BrgId, WarehouseId`; 600s timeout pattern |
| Transfer false positives (sparse warehouse sales) | Medium | Require dest warehouse ADC > 0; min transfer qty threshold |
| Budget cap misinterpreted as approval limit | Low | UI disclaimer — indicative planning only |
| Recommendation volume overwhelms UI | Low | Cap materialized rows (25/15/10); client pagination |
| M21 snapshot temporal skew | Low | Read M21 tables in same `All` refresh order before InventoryRisk; document staleness up to 60 min |
| Conflict: Purchase + Delay same item | Medium | Policy precedence: Do Not Reorder > Delay > Purchase; document in tests |

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
      IBrgConsumptionDal.ListConsumptionByBrg(30d + 90d)
      IBrgWarehouseConsumptionDal.ListConsumptionByBrgWarehouse(30d)   ← new
      IBrgActiveDal / IsAktif
      IDashboardPurchasingManagementSnapshotDal.GetCurrent()           ← cross-read M21
        ↓
DashboardInventoryRiskAggregator.Aggregate()           → BTRPD_InventoryRisk* (unchanged)
        ↓
DashboardInventoryForecastAggregator.Aggregate()       → BTRPD_InventoryForecast* (unchanged)
        ↓
DashboardInventoryOptimizationAggregator.Aggregate()     → BTRPD_InventoryOptimization* (new)
        ↓
DashboardInventoryRiskSnapshotDal.ReplaceCurrent()     → single transaction

Browser → GET /api/dashboard/inventory-optimization
        ↓ MediatR
GetDashboardInventoryOptimizationHandler
        ↓ IDashboardInventoryOptimizationDal
DashboardInventoryOptimizationDal → snapshot SELECT
```

**Design decision:** Extend the **existing Inventory Risk refresh** after M28 forecast step. Optimization receives in-memory `ForecastItemContext` from forecast aggregator — guarantees IO-31 (no duplicate ADC) and IO-50 traceability.

**Alternative rejected:** Separate `InventoryOptimization` worker domain — would duplicate expensive loads and allow temporal skew between forecast and recommendations.

### 3.2 Layering

```text
btr.portal.api          → InventoryOptimizationDashboardController (thin)
btr.application         → GetDashboardInventoryOptimizationQuery + Handler
                          → DashboardInventoryOptimizationAggregator + InventoryOptimizationPolicy
                          → IDashboardInventoryOptimizationDal (contract)
btr.infrastructure      → DashboardInventoryOptimizationDal (read facade)
                          → BrgWarehouseConsumptionDal
                          → DashboardInventoryRiskSnapshotDal (extended write)
btr.portal.web          → InventoryOptimizationDashboardView.vue + components
```

MediatR pattern preserved. No business logic in controller.

### 3.3 New policy class

**File:** `btr.application/ReportingContext/DashboardSnapshotAgg/Services/InventoryOptimizationPolicy.cs`

Mirror `InventoryForecastPolicy` / `CashFlowForecastPolicy` structure:

```csharp
public static class InventoryOptimizationPolicy
{
    public const string ActionPurchase = "PurchaseProduct";
    public const string ActionDefer = "DeferPurchase";
    public const string ActionDelay = "DelayPurchase";
    public const string ActionReduceQty = "ReducePurchaseQuantity";
    public const string ActionTransfer = "TransferInventory";
    public const string ActionPostFirst = "PostPurchaseFirst";
    public const string ActionPromote = "PromoteCampaignReview";
    public const string ActionBundle = "BundleProductsReview";
    public const string ActionClearance = "ClearanceReview";
    public const string ActionDoNotReorder = "DoNotReorder";

    public static int ComputePriorityScore(
        string category,
        decimal impactValueIdr,
        decimal? daysOfSupply,
        int defaultLeadTimeDays,
        bool isStrategicItem,
        string actionType);

    public static string ResolveCategory(
        string actionType,
        decimal? daysOfSupply,
        DateTime? reorderDate,
        DateTime businessDate,
        string movementClass,
        decimal impactValueIdr,
        decimal deadStockP75);

    public static decimal ComputeRecommendedBudget(
        IEnumerable<PurchaseRecommendationContext> purchases,
        params string[] categories);

    public static IEnumerable<PurchaseRecommendationContext> ApplyBudgetCap(
        IEnumerable<PurchaseRecommendationContext> sortedPurchases,
        decimal? budgetCapIdr);

    public static decimal ComputeTransferQty(
        decimal sourceQty,
        decimal sourceAdc,
        decimal destQty,
        decimal destAdc,
        int leadTimeDays);

    public static string BuildReasonText(
        string actionType,
        ForecastItemContext item,
        WarehouseTransferContext transfer = null,
        bool supplierBacklog = false);
}
```

**Category weights (configurable defaults):** Critical=1000, High=750, Medium=500, Low=250.

**Action precedence when multiple rules match same item:**

```text
1. DoNotReorder (Dead / Never Sold / inactive)
2. PostPurchaseFirst (if supplier backlog + stock-out)
3. DelayPurchase (overstock / slow)
4. TransferInventory (if pair exists — may suppress Purchase for same item when transfer covers gap)
5. PurchaseProduct
6. Promote / Bundle / Clearance (parallel — additional rows allowed)
```

### 3.4 Warehouse consumption DAL

**File:** `btr.infrastructure/SalesContext/FakturInfoAgg/BrgWarehouseConsumptionDal.cs`

**Contract:** `IBrgWarehouseConsumptionDal.ListConsumptionByBrgWarehouse(DateTime windowStart, DateTime windowEnd)`

```sql
SELECT
    fi.BrgId,
    aa.WarehouseId,
    SUM(fi.QtyJual) AS SoldQty30
FROM BTR_Faktur aa
INNER JOIN BTR_FakturItem fi ON aa.FakturId = fi.FakturId
WHERE aa.VoidDate = '3000-01-01'
  AND aa.FakturDate BETWEEN @Start30 AND @End
GROUP BY fi.BrgId, aa.WarehouseId
```

Join with `StokBalanceView` rows (Item × Warehouse) for warehouse DOS:

```text
WarehouseADC = SoldQty30 ÷ 30
WarehouseDOS = Qty ÷ WarehouseADC  (when ADC > 0)
```

### 3.5 New aggregator

**File:** `btr.application/ReportingContext/DashboardSnapshotAgg/Services/DashboardInventoryOptimizationAggregator.cs`

**Inputs:**

- `IEnumerable<ForecastItemContext> forecastItems` — from M28 aggregator (in-memory)
- `IEnumerable<StokBalanceView> balanceRows` — warehouse grain
- `IEnumerable<BrgWarehouseConsumptionDto> warehouseConsumption`
- `DashboardInventoryRiskAggregateResult riskResult` — movement class per item
- `DashboardPurchasingManagementAggregateResult purchasingMgmt` — qualified backlog suppliers, principal inventory no purchase
- `DashboardInventoryForecastKpiSnapshot forecastKpi` — health score copy
- `DateTime businessDate`, `InventoryOptimizationOptions`

**Responsibilities:**

1. Build purchase recommendations from forecast items (expand beyond M28 Top 10 — all eligible Critical/High/Medium, cap materialized at 15 for reorder table)
2. Apply action precedence — generate unified action list
3. `WarehouseBalanceRecommendationBuilder.BuildTransferPairs` — max 10
4. Apply budget cap deferral via policy
5. Build supplier bundle rows (≥ 2 overstock SKUs)
6. Roll up KPIs: action counts, budgets, recoverable capital
7. Build priority distribution + action heat summary DTOs
8. `InventoryOptimizationExecutiveSummaryBuilder` — server-side narrative

**Output:** `DashboardInventoryOptimizationAggregateResult`

### 3.6 Recommendation builder

**File:** `btr.application/ReportingContext/DashboardSnapshotAgg/Services/InventoryOptimizationRecommendationBuilder.cs`

Parallel to `InventoryForecastRiskBuilder`:

- Deterministic action keys from analysis §7
- Priority score via policy
- Cap: 25 unified actions, 15 reorder, 10 transfer, 10 delay, 10 clearance
- Include `RuleId`, `ReasonText`, `ReportRoute`, `DrillDownRoute`

---

## 4. API Design

### 4.1 Endpoint

```
GET /api/dashboard/inventory-optimization
```

**Auth:** Same as other dashboard endpoints.

**Response:** `DashboardInventoryOptimizationResponse`

| Section | Content |
| ------- | ------- |
| `Metadata` | `GeneratedAt`, `BusinessDate`, `PlanningHorizonDays`, `BudgetCapIdr`, `IsAvailable` |
| `Kpis` | Health score, critical count, budgets, deferrable spend, recoverable capital, action mix counts |
| `PriorityDistribution` | Category → count |
| `ActionHeatSummary` | Action type × category grid |
| `BusinessImpactSummary` | Purchase / defer / recoverable value buckets |
| `TopActions` | Unified recommendation rows (max 25) |
| `ReorderList` | Purchase actions only (max 15) |
| `TransferList` | Warehouse pairs (max 10) |
| `DelayList` | Delay / reduce qty (max 10) |
| `ClearanceList` | Dead stock recovery (max 10) |
| `ExecutiveSummary` | Server-composed plain-language block |
| `Traceability` | Links + reconciliation footnotes |

**Unavailable snapshot:** `IsAvailable = false`, HTTP 200 (match M28).

### 4.2 MediatR

```text
GetDashboardInventoryOptimizationQuery
GetDashboardInventoryOptimizationHandler
  → IDashboardInventoryOptimizationDal.GetCurrent()
```

### 4.3 Controller

```text
InventoryOptimizationDashboardController
  [Route("api/dashboard/inventory-optimization")]
  [HttpGet] → MediatR send
```

---

## 5. ReportingContext Additions

### 5.1 Application contracts

| Type | Purpose |
| ---- | ------- |
| `IDashboardInventoryOptimizationDal` | Read facade for API |
| `IBrgWarehouseConsumptionDal` | Warehouse-scoped consumption |
| `BrgWarehouseConsumptionDto` | `BrgId`, `WarehouseId`, `WarehouseName`, `SoldQty30` |
| `DashboardInventoryOptimizationAggregateResult` | Worker output |
| `DashboardInventoryOptimizationKpiSnapshot` | Company KPIs |
| `DashboardInventoryOptimizationActionRow` | Unified action row |
| `DashboardInventoryOptimizationReorderRow` | Purchase subset |
| `DashboardInventoryOptimizationTransferRow` | Transfer pair |
| `DashboardInventoryOptimizationDelayRow` | Delay / reduce |
| `DashboardInventoryOptimizationClearanceRow` | Dead stock |

### 5.2 Options

Extend `DashboardSnapshotOptions`:

```json
"DashboardSnapshot": {
  "InventoryOptimizationDefaultBudgetCapIdr": null,
  "InventoryOptimizationWarehouseShortageDosDays": 14,
  "InventoryOptimizationWarehouseExcessDosDays": 60,
  "InventoryOptimizationMaxTopActions": 25,
  "InventoryOptimizationMaxReorderRows": 15,
  "InventoryOptimizationMaxTransferRows": 10,
  "InventoryOptimizationReduceQtyFactor": 0.5
}
```

Reuse existing M28 settings for lead time, overstock threshold, horizon — do not duplicate keys.

### 5.3 Worker extension

**File:** `RefreshDashboardInventoryRiskSnapshotWorker.cs`

Add steps after M28 forecast aggregate:

```text
Load warehouse consumption (30d)
Load purchasing management snapshot (M21 read DAL)
AggregateOptimization(forecastItems, ...)
Save → ReplaceCurrent(..., optimization, refreshLogId)
```

**Domain label:** Remains `"InventoryRisk"`.

**Refresh order in `All`:** Purchasing Management completes before InventoryRisk in existing orchestration — verify cross-read freshness; if not guaranteed, load M21 inline from same DB transaction at refresh start.

---

## 6. Materialized Data Impact

### 6.1 New tables

| Table | Layer | Content |
| ----- | ----- | ------- |
| `BTRPD_InventoryOptimizationKpi` | A | Action counts, budgets, health score copy, recoverable capital |
| `BTRPD_InventoryOptimizationAction` | B | Top 25 unified actions |
| `BTRPD_InventoryOptimizationReorder` | B | Top 15 purchase rows |
| `BTRPD_InventoryOptimizationTransfer` | B | Top 10 transfer pairs |
| `BTRPD_InventoryOptimizationDelay` | B | Top 10 delay/reduce rows |
| `BTRPD_InventoryOptimizationClearance` | B | Top 10 dead stock recovery rows |
| `BTRPD_InventoryOptimizationPriorityDist` | B | Category distribution chart |
| `BTRPD_InventoryOptimizationActionHeat` | B | Type × category heat grid |

**Snapshot key:** `CURRENT` — delete-and-replace each refresh.

### 6.2 KPI table columns (indicative)

`BTRPD_InventoryOptimizationKpi`:

- `SnapshotKey`, `GeneratedAt`, `BusinessDate`, `PlanningHorizonDays`, `BudgetCapIdr`
- `InventoryHealthScore` (from M28)
- `CriticalActionCount`, `HighActionCount`, `MediumActionCount`, `LowActionCount`
- `PurchaseNowCount`, `DelayCount`, `TransferCount`, `ClearanceCount`, `PostFirstCount`, `DeferCount`
- `RequiredPurchaseBudgetIdr`, `RecommendedPurchaseBudgetIdr`, `DeferrableSpendIdr`, `RecoverableCapitalIdr`
- `LastRefreshLogId`

### 6.3 Action row columns (indicative)

`BTRPD_InventoryOptimizationAction`:

- `SortOrder`, `PriorityScore`, `Category`, `ActionType`, `ActionLabel`
- `BrgId`, `BrgName`, `SupplierName`
- `WarehouseFromId`, `WarehouseFromName`, `WarehouseToId`, `WarehouseToName` (nullable)
- `Quantity`, `ImpactValueIdr`, `DaysOfSupply`, `ReasonText`, `RuleId`
- `ReportRoute`, `DrillDownRoute`

### 6.4 SQL scripts

Add to `btr.sql/Scripts/Create_BTRPD_PortalDashboard_Tables.sql`:

- `CREATE TABLE` statements matching `BTRPD_InventoryForecast*` FK pattern
- Indexes on `SnapshotKey` only

### 6.5 Existing tables

**No changes** to M28 forecast tables or M19 risk tables.

---

## 7. Snapshot Refresh Strategy

| Aspect | Choice |
| ------ | ------ |
| Worker | `RefreshDashboardInventoryRiskSnapshotWorker` extended |
| Cadence | 60 minutes (`InventoryRiskIntervalMinutes`) |
| Transaction | Single `ReplaceCurrent` — M19 + M28 + M28.5 in one transaction |
| Manual refresh | `POST /api/admin/dashboard/refresh?domain=InventoryRisk` rebuilds optimization |
| CLI | `btr.portal.worker --domain InventoryRisk` |
| Presentation Mode | `IBusinessDateProvider.Today` drives all date rules |

---

## 8. Database Impact

| Object | Impact |
| ------ | ------ |
| `BTR_Faktur` / `BTR_FakturItem` | Read-only — warehouse consumption aggregate |
| `BTR_StokBalanceWarehouse` / view | Read-only — warehouse qty |
| `BTRPD_PurchasingManagement*` | Read-only cross-read at refresh |
| `BTRPD_InventoryForecast*` | Read-only in-memory via aggregator output |
| `BTRPD_*` | 8 new optimization tables |

**Index recommendation (optional):** Covering index on `BTR_Faktur (VoidDate, FakturDate)` include `WarehouseId`, `FakturId` if warehouse consumption query exceeds SLA.

---

## 9. Performance Considerations

| Concern | Mitigation |
| ------- | ---------- |
| Warehouse consumption GROUP BY | Returns BrgId × Warehouse rows only — not line-level |
| Reuse M28 forecast item list | O(n) recommendation pass — no second FakturItem scan for company ADC |
| M21 snapshot read | Single KPI + attention list read — supplier name set in memory |
| Transfer pair algorithm | O(items × warehouses²) worst case — prune to items with stock-out risk only |
| API read | 1 KPI row + bounded child tables — p95 target < 500ms |
| Worker increment | Target < 15s additional on typical dataset (measure UAT) |

---

## 10. UI Implementation Plan

### 10.1 Route and navigation

| Item | Value |
| ---- | ----- |
| Route | `/dashboard/inventory-optimization` |
| Sidebar label | Inventory Optimization |
| Position | After Inventory Forecast, before Purchasing |
| Router module | `src/router/index.ts` lazy import |

### 10.2 View structure

**File:** `InventoryOptimizationDashboardView.vue`

Reuse patterns from `InventoryForecastDashboardView.vue` and `CashFlowForecastDashboardView.vue`:

| Component | Purpose |
| --------- | ------- |
| `DashboardDetailLayout` | Shell, refresh, generated-at |
| `InventoryOptimizationSummary` | Executive summary block |
| `InventoryOptimizationKpiGrid` | 2 KPI rows (health/budget + action mix) |
| `InventoryOptimizationPriorityChart` | Category distribution horizontal bar |
| `InventoryOptimizationImpactChart` | Business impact stacked bar |
| `InventoryOptimizationActionHeat` | Type × category grid |
| `InventoryOptimizationActionsTable` | Top 25 unified queue |
| `InventoryOptimizationReorderTable` | Purchase list |
| `InventoryOptimizationTransferTable` | Warehouse pairs |
| `InventoryOptimizationDelayTable` | Delay / reduce |
| `InventoryOptimizationClearanceTable` | Dead stock recovery |

### 10.3 Store and API

| File | Change |
| ---- | ------ |
| `src/api/dashboardApi.ts` | `getInventoryOptimization()` |
| `src/models/dashboard.ts` | `DashboardInventoryOptimizationResponse` interfaces |
| `src/stores/dashboardStore.ts` | `loadInventoryOptimization()`, state slice |

### 10.4 Category badge colors

| Category | Semantic color token |
| -------- | -------------------- |
| Critical | `severity-critical` (match Collection Risk table) |
| High | `severity-warning` |
| Medium | `severity-info` |
| Low | `severity-secondary` |

### 10.5 Traceability footer

Links:

- `/dashboard/inventory-forecast` — upstream forecast
- `/dashboard/inventory-risk` — obsolescence
- `/dashboard/purchasing-management` — posting backlog
- `/reports/inventory` — evidence
- `/reports/purchasing` — purchase evidence

Disclaimer:

> *Recommendations are indicative decision support. BTR Portal does not create purchases or warehouse transfers. Confirm stock, pending postings, and supplier terms in BTR Desktop before acting.*

---

## 11. Testing Plan

### 11.1 Unit tests

| Test class | Coverage |
| ---------- | -------- |
| `InventoryOptimizationPolicyTest` | Priority score, category resolution, budget cap deferral, transfer qty, precedence |
| `DashboardInventoryOptimizationAggregatorTest` | Action generation, caps, DoNotReorder suppresses purchase, IO-50 health score copy |
| `WarehouseBalanceRecommendationBuilderTest` | Pair detection, inactive warehouse exclusion, max 10 cap |
| `InventoryOptimizationExecutiveSummaryBuilderTest` | Summary template with counts |

### 11.2 Traceability tests

| Test | Rule |
| ---- | ---- |
| `InventoryOptimizationHealthTraceabilityTest` | IO-50 — health score matches M28 KPI on same fixture |
| `InventoryOptimizationBudgetTraceabilityTest` | IO-51 — reorder cost sum ≤ M28 rec qty × Hpp for same items |
| `InventoryOptimizationPrecedenceTest` | IO-52 — dead stock never in reorder list |

### 11.3 Integration smoke

- Worker `InventoryRisk` domain completes with optimization tables populated
- API returns `IsAvailable = true` after refresh
- UI renders without console errors
- Drill-down routes resolve

---

## 12. Step-by-Step Implementation Plan

### Phase A — SQL and contracts (1–2 days)

1. Add `BTRPD_InventoryOptimization*` table definitions to SQL scripts.
2. Add DTOs and `DashboardInventoryOptimizationAggregateResult` models.
3. Add `IBrgWarehouseConsumptionDal` + DTO.
4. Extend `DashboardSnapshotOptions` with optimization settings.
5. Add `IDashboardInventoryOptimizationDal` read contract; extend write contract.

### Phase B — Domain logic (3–4 days)

6. Implement `BrgWarehouseConsumptionDal` with aggregated SQL.
7. Implement `InventoryOptimizationPolicy` with full unit tests.
8. Implement `WarehouseBalanceRecommendationBuilder`.
9. Implement `InventoryOptimizationRecommendationBuilder`.
10. Implement `DashboardInventoryOptimizationAggregator` with unit tests.
11. Implement `InventoryOptimizationExecutiveSummaryBuilder`.

### Phase C — Snapshot pipeline (1–2 days)

12. Extend `RefreshDashboardInventoryRiskSnapshotWorker` — warehouse consumption + M21 read + optimization aggregate.
13. Extend `DashboardInventoryRiskSnapshotDal.ReplaceCurrent` — write optimization tables in same transaction.
14. Register services in DI extensions.
15. Verify worker via CLI on dev database.

### Phase D — API (1 day)

16. Add `GetDashboardInventoryOptimizationQuery` + handler.
17. Implement `DashboardInventoryOptimizationDal` read facade.
18. Add `InventoryOptimizationDashboardController`.

### Phase E — UI (2–3 days)

19. Add TypeScript models and API client method.
20. Extend dashboard store.
21. Build chart, heat, and table components.
22. Build `InventoryOptimizationDashboardView.vue`.
23. Add route and sidebar entry.
24. Run `npm run build` verification.

### Phase F — Verification and documentation (1 day)

25. Run full unit test suite.
26. Manual UAT: verify Critical purchase rows match M28 forecast urgency for sample items.
27. Write `docs/features/inventory-optimization/feature.md` (Knowledge Curator).
28. Update `btr-portal-domain.md` — optimization layer + dashboard entry.

**Estimated effort:** 9–13 developer days.

---

## 13. Future Extensibility (Architecture Hooks)

| Extension | Hook |
| --------- | ---- |
| Purchase scenario simulation | `BTRPD_InventoryOptimizationReorder` includes all formula inputs (ADC, DOS, LT, Qty) for client what-if |
| Budget what-if UI | API accepts optional `?budgetCap=` query param filtering read model only — no re-aggregate |
| Supplier lead time | Add `LeadTimeDays` column on reorder rows |
| Alert Center | Read `BTRPD_InventoryOptimizationAction` where `Category = Critical` |
| Executive promotion | Subset of optimization KPIs in `DashboardExecutiveComposer` |
| EOQ | Optional `EoqSuggestedQty` column — parallel to `RecommendedPurchaseQty` |
| AI explanation | Add `ExplanationText` column — populated by future service; `ReasonText` remains authoritative |
| Net consumption | Pass through M28 item context when M28 adds net ADC |

V1 row shapes include `RuleId`, `PriorityScore`, and formula inputs on reorder rows so future simulation milestones avoid schema migration.

---

## 14. Acceptance Criteria

1. `/dashboard/inventory-optimization` loads from materialized snapshot; p95 API < 500ms.
2. Recommendations are deterministic — no ML/AI/solvers.
3. `InventoryHealthScore` matches M28 forecast KPI on same refresh (IO-50).
4. Dead stock / never sold items never appear in Recommended Reorder List (IO-52).
5. Every action row includes `ReasonText` and `RuleId`.
6. Unified Top Actions sorted by `PriorityScore` descending.
7. Transfer recommendations exclude inactive and In-Transit warehouses.
8. Post Purchase First fires when M21 qualified backlog supplier matches stock-out item supplier.
9. Budget cap deferral works when `InventoryOptimizationDefaultBudgetCapIdr` configured.
10. Existing M28 Inventory Forecast dashboard unchanged.
11. Unit tests pass for policy, aggregator, precedence, and traceability.
12. `GeneratedAt` reflects Inventory Risk refresh timestamp.

---

## Document Maintenance

After implementation, Knowledge Curator promotes durable rules to:

- `docs/features/inventory-optimization/feature.md`
- `docs/features/materialized-dashboard/materialized-dashboard-domain.md` (Inventory Optimization extension)
- `docs/features/btr-portal/btr-portal-domain.md` (Section 8 dashboards, maturity level 5)

Temporary work artifacts remain in `docs/work/btr-portal/` until curator pass completes.
