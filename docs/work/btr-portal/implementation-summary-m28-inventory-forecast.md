# M28 Inventory Forecast Dashboard — Implementation Summary

| Field | Value |
| --- | --- |
| Milestone | M28 — Inventory Forecast Dashboard |
| Plan | [implementation-plan-m28-inventory-forecast.md](./implementation-plan-m28-inventory-forecast.md) |
| Analysis | [portal-analysis-m28-inventory-forecast.md](./portal-analysis-m28-inventory-forecast.md) |
| Status | **Complete** |
| Date | 2026-06-21 |

## Delivered

### Backend — forecast aggregation

- `InventoryForecastPolicy` — Rolling 30-day ADC, DOS, stock-out/reorder dates, recommended qty, scenario bands, health score, severity/urgency resolvers.
- `InventoryConsumptionGrouper` — 30-day calendar buckets for company consumption pace chart.
- `InventoryForecastRiskBuilder` — deterministic risk signals, top 10 cap, purchase recommendation ranking.
- `DashboardInventoryForecastAggregator` — item forecast roll-ups, projected level series, heat summary, IFR-50/51 traceability from shared item groups and M19 KPIs.
- `BrgConsumptionDal` — aggregated FakturItem consumption by BrgId (30d + 90d) and daily company totals.
- Extended `RefreshDashboardInventoryRiskSnapshotWorker` — consumption load + forecast aggregate in same InventoryRisk refresh.
- Extended `DashboardInventoryRiskSnapshotDal.ReplaceCurrent` — persists Inventory Risk + Inventory Forecast snapshots atomically.

### Database

- `BTRPD_InventoryForecastKpi` — forecast KPI snapshot (Layer A).
- `BTRPD_InventoryForecastDailyConsumption` — 30-day daily units + ADC reference (Layer B).
- `BTRPD_InventoryForecastLevel` — projected inventory value days 0..H (Layer B).
- `BTRPD_InventoryForecastRisk` — top inventory risk rows (Layer B).
- `BTRPD_InventoryForecastRecommendation` — top purchase recommendation rows (Layer B).
- Appended to `Create_BTRPD_PortalDashboard_Tables.sql` and `btr.sql.sqlproj`.

### API

- `GET /api/dashboard/inventory-forecast` — MediatR query + read DAL.
- Server-side `InventoryForecastExecutiveSummaryBuilder` — stock-out count, value projection, top-risk narrative.
- Graceful degradation when snapshot missing (`IsAvailable = false`, HTTP 200).

### Portal Web

- Route `/dashboard/inventory-forecast` — sidebar entry **Inventory Forecast** (after Inventory Risk).
- Components: `InventoryForecastSummary`, `InventoryForecastKpiGrid`, `InventoryForecastLevelChart`, `InventoryConsumptionTrendChart`, `InventoryRiskHeatSummary`, `InventoryForecastRisksTable`, `InventoryPurchaseRecommendationsTable`.
- Store: `inventoryForecast` state + `loadInventoryForecast()`.

### Configuration

- `DashboardSnapshot:InventoryForecastPlanningHorizonDays` (default 30)
- `DashboardSnapshot:InventoryForecastDefaultLeadTimeDays` (default 7)
- `DashboardSnapshot:InventoryForecastCoverageDays` (default 14)
- `DashboardSnapshot:InventoryForecastOverstockDosDays` (default 90)
- `DashboardSnapshot:InventoryForecastMinDosHealthy` (default 30)
- `DashboardSnapshot:InventoryForecastStockOutCriticalDays` (default 7)

## Tests

| File | Coverage |
| ---- | -------- |
| `InventoryForecastPolicyTest.cs` | ADC, DOS, stock-out/reorder, scenarios, health score, severity |
| `DashboardInventoryForecastAggregatorTest.cs` | IFR-50/51 traceability, eligibility, top 10 cap |
| `InventoryForecastRiskBuilderTest.cs` | Priority ordering, urgency |
| `InventoryForecastExecutiveSummaryBuilderTest.cs` | Summary templates |
| `router/index.spec.ts` | Route resolution for `/dashboard/inventory-forecast` |

## Knowledge sync

- Created [docs/features/inventory-forecast/feature.md](../features/inventory-forecast/feature.md)
- Updated [btr-portal-domain.md](../features/btr-portal/btr-portal-domain.md) — M28 dashboard + forecasting layer
- Updated [btr-portal-operational.md](../features/btr-portal/btr-portal-operational.md) — route, API, usage
- Updated [materialized-dashboard-architecture.md](../features/materialized-dashboard/materialized-dashboard-architecture.md) — new tables
- Updated [materialized-dashboard-domain.md](../features/materialized-dashboard/materialized-dashboard-domain.md) — Inventory Risk worker extension

## Out of scope (unchanged)

- Inventory Dashboard and Inventory Risk API responses
- Per-warehouse forecast
- Automatic purchase order creation
- Alert Center / Executive Dashboard integration
- Net-of-retur consumption

## Verification

1. Run InventoryRisk snapshot refresh (existing `"InventoryRisk"` domain).
2. `GET /api/dashboard/inventory-forecast` returns KPIs, daily consumption, projected level, risks, recommendations, executive summary.
3. Forecast `CurrentInventoryValue` reconciles with Inventory Dashboard for same refresh (IFR-50).
4. `AtRiskInventoryPercent` reconciles with Inventory Risk snapshot (IFR-51).
5. Navigate Sidebar → Dashboard → Inventory Forecast.
6. `npm run build` passes for portal web; `btr.application`, `btr.infrastructure`, `btr.test` compile successfully.
