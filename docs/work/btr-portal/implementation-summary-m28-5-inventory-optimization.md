# M28.5 Inventory Optimization Dashboard — Implementation Summary

| Field | Value |
| --- | --- |
| Milestone | M28.5 — Inventory Optimization Dashboard |
| Plan | [implementation-plan-m28-5-inventory-optimization.md](./implementation-plan-m28-5-inventory-optimization.md) |
| Analysis | [portal-analysis-m28-5-inventory-optimization.md](./portal-analysis-m28-5-inventory-optimization.md) |
| Status | **Complete** |
| Date | 2026-06-21 |

## Delivered

### Backend — optimization aggregation

- `InventoryOptimizationPolicy` — action type constants, priority score, category resolver, budget cap, transfer qty, reason text, precedence (Do Not Reorder > Post Purchase First > Delay > Transfer > Purchase > Promote/Bundle/Clearance).
- `WarehouseBalanceRecommendationBuilder` — warehouse-grain transfer pairs (IO-01..IO-06); excludes In-Transit, inactive, and special warehouses.
- `InventoryOptimizationRecommendationBuilder` — deterministic action keys, `RuleId`, `ReasonText`, report/drill-down routes; caps 25 unified / 15 reorder / 10 per specialized table.
- `DashboardInventoryOptimizationAggregator` — orchestrates M28 `ForecastItemContext`, M19 risk, M21 purchasing snapshot cross-read, warehouse consumption, KPI rollups, priority distribution, action heat.
- `InventoryOptimizationExecutiveSummaryBuilder` — server-side narrative for API response.
- `BrgWarehouseConsumptionDal` — aggregated FakturItem consumption by `BrgId` + `WarehouseId` (30-day window).
- Extended `RefreshDashboardInventoryRiskSnapshotWorker` — warehouse consumption load, M21 `GetCurrent()`, optimization aggregate after M28 forecast.
- Extended `DashboardInventoryRiskSnapshotDal.ReplaceCurrent` — persists Inventory Risk + Inventory Forecast + Inventory Optimization atomically (four-parameter overload).
- M28 refactor — `ForecastItemContext` extracted to shared model; `DashboardInventoryForecastAggregateResult.ItemContexts` populated for optimization reuse (IO-31).

### Database

- `BTRPD_InventoryOptimizationKpi` — optimization KPI snapshot (Layer A).
- `BTRPD_InventoryOptimizationAction` — top 25 unified actions.
- `BTRPD_InventoryOptimizationReorder` — top 15 purchase rows.
- `BTRPD_InventoryOptimizationTransfer` — top 10 transfer pairs.
- `BTRPD_InventoryOptimizationDelay` — top 10 delay/reduce rows.
- `BTRPD_InventoryOptimizationClearance` — top 10 dead stock clearance rows.
- `BTRPD_InventoryOptimizationPriorityDist` — category distribution chart.
- `BTRPD_InventoryOptimizationActionHeat` — action type × category grid.
- Appended to `Create_BTRPD_PortalDashboard_Tables.sql` and `btr.sql.sqlproj`.

### API

- `GET /api/dashboard/inventory-optimization` — MediatR query + read DAL.
- Server-side `InventoryOptimizationExecutiveSummaryBuilder` — action counts, budget narrative, cross-dashboard traceability.
- Graceful degradation when snapshot missing (`IsAvailable = false`, HTTP 200).

### Portal Web

- Route `/dashboard/inventory-optimization` — sidebar entry **Inventory Optimization** (after Inventory Forecast).
- Components: `InventoryOptimizationSummary`, `InventoryOptimizationPriorityChart`, `InventoryOptimizationImpactChart`, `InventoryOptimizationActionHeat`, `InventoryOptimizationActionsTable`, `InventoryOptimizationReorderTable`, `InventoryOptimizationTransferTable`, `InventoryOptimizationDelayTable`, `InventoryOptimizationClearanceTable`.
- Store: `inventoryOptimization` state + `loadInventoryOptimization()`.

### Configuration

- `DashboardSnapshot:InventoryOptimizationDefaultBudgetCapIdr` (optional — defer purchases beyond cap)
- `DashboardSnapshot:InventoryOptimizationWarehouseShortageDosDays` (default 14)
- `DashboardSnapshot:InventoryOptimizationWarehouseExcessDosDays` (default 60)
- `DashboardSnapshot:InventoryOptimizationMaxTopActions` (default 25)
- `DashboardSnapshot:InventoryOptimizationMaxReorderRows` (default 15)
- `DashboardSnapshot:InventoryOptimizationMaxTransferRows` (default 10)
- `DashboardSnapshot:InventoryOptimizationReduceQtyFactor` (default 0.5)
- Reuses existing `InventoryForecast*` keys for horizon, lead time, overstock — no duplication.

## Tests

| File | Coverage |
| ---- | -------- |
| `InventoryOptimizationPolicyTest.cs` | Priority score, category, budget cap, transfer qty, Do Not Reorder |
| `DashboardInventoryOptimizationAggregatorTest.cs` | IO-50 health score match, IO-52 dead stock excluded from reorder, top action cap |
| `WarehouseBalanceRecommendationBuilderTest.cs` | In-Transit exclusion, max 10 transfer cap |
| `InventoryOptimizationExecutiveSummaryBuilderTest.cs` | Summary templates |
| `router/index.spec.ts` | Route resolution for `/dashboard/inventory-optimization` |

**Test run:** 11 passed (`InventoryOptimization*` + `WarehouseBalance*` filter via vstest.console).

## Knowledge sync

- Created [docs/features/inventory-optimization/feature.md](../features/inventory-optimization/feature.md)
- Updated [btr-portal-domain.md](../features/btr-portal/btr-portal-domain.md) — M28.5 dashboard, optimization maturity level, milestone map
- Updated [materialized-dashboard-domain.md](../features/materialized-dashboard/materialized-dashboard-domain.md) — optimization extension on Inventory Risk worker

## Out of scope (unchanged)

- M28 `/api/dashboard/inventory-forecast` response shape and tables
- M19 Inventory Risk API response
- Automatic purchase order or mutasi creation
- Alert Center / Executive Dashboard integration
- User-entered budget in portal UI

## Verification

1. Run InventoryRisk snapshot refresh (existing `"InventoryRisk"` domain).
2. `GET /api/dashboard/inventory-optimization` returns KPIs, charts data, five action tables, executive summary, traceability.
3. `InventoryHealthScore` matches M28 forecast KPI on same refresh (IO-50).
4. Dead / Never Sold items excluded from reorder list (IO-52).
5. Navigate Sidebar → Dashboard → Inventory Optimization.
6. `npm run build` passes for portal web; `btr.application`, `btr.infrastructure`, `btr.test` compile successfully.
