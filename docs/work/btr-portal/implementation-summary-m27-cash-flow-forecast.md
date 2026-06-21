# M27 Cash Flow Forecast Dashboard — Implementation Summary

| Field | Value |
| --- | --- |
| Milestone | M27 — Cash Flow Forecast Dashboard |
| Plan | [implementation-plan-m27-cash-flow-forecast.md](./implementation-plan-m27-cash-flow-forecast.md) |
| Analysis | [portal-analysis-m27-cash-flow-forecast.md](./portal-analysis-m27-cash-flow-forecast.md) |
| Status | **Complete** |
| Date | 2026-06-21 |

## Delivered

### Backend — forecast aggregation

- `CollectionDayGrouper` — calendar-day buckets within month (`id-ID` labels).
- `CashFlowForecastPolicy` — dual pace (cash + total collections), confidence, required-daily severity, recovery forecast band.
- `CashFlowCollectionRiskBuilder` — eight deterministic risk rules, top 10 cap.
- `DashboardCashFlowForecastAggregator` — daily pace, recovery trend, scenario bands, CFR-50/51/52 traceability from Collection KPIs.
- Extended `RefreshDashboardCollectionSnapshotWorker` — forecast + 30-day pelunasan lookback in same Collection refresh.
- Extended `DashboardCollectionSnapshotDal.ReplaceCurrent` — persists Collection + Cash Flow Forecast snapshots atomically.

### Database

- `BTRPD_CashFlowForecastKpi` — forecast KPI snapshot (Layer A).
- `BTRPD_CashFlowDailyPace` — daily cash pace buckets (Layer B).
- `BTRPD_CashFlowRecoveryTrend` — cumulative collections vs billing (Layer B).
- `BTRPD_CashFlowCollectionRisk` — top collection risk rows (Layer B).
- Appended to `Create_BTRPD_PortalDashboard_Tables.sql` and `btr.sql.sqlproj`.

### API

- `GET /api/dashboard/cash-flow-forecast` — MediatR query + read DAL.
- Server-side `CashFlowForecastExecutiveSummaryBuilder` — billing-unavailable, low-confidence, healthy/warning/critical templates.
- Graceful degradation when snapshot missing (`IsAvailable = false`, HTTP 200).

### Portal Web

- Route `/dashboard/cash-flow-forecast` — sidebar entry **Cash Flow Forecast** (after Collection).
- Components: `CashFlowForecastSummary`, `CashFlowKpiGrid`, `CashFlowDailyPaceChart`, `CashFlowForecastVsBillingChart`, `CashFlowRecoveryTrendChart`, `CashFlowCollectionRisksTable`.
- Store: `cashFlowForecast` state + `loadCashFlowForecast()`.

### Configuration

- `DashboardSnapshot:CashFlowForecastLargeDueSoonFloorAmount` (default 50_000_000) in API and Worker appsettings.

## Tests

| File | Coverage |
| ---- | -------- |
| `CollectionDayGrouperTest.cs` | Bucket count, labels, find-bucket |
| `CashFlowForecastPolicyTest.cs` | Mid-month, day-1, BO=0, MC≥BO, last day, best/worst, confidence, severity |
| `DashboardCashFlowForecastAggregatorTest.cs` | Daily cash sum, traceability, outstanding due, risk cap |
| `CashFlowForecastExecutiveSummaryBuilderTest.cs` | All summary templates |
| `router/index.spec.ts` | Route resolution for `/dashboard/cash-flow-forecast` |

## Knowledge sync

- Created [docs/features/cash-flow-forecast/feature.md](../features/cash-flow-forecast/feature.md)
- Updated [btr-portal-domain.md](../features/btr-portal/btr-portal-domain.md) — M27 dashboard + forecasting layer
- Updated [btr-portal-operational.md](../features/btr-portal/btr-portal-operational.md) — route, API, usage
- Updated [materialized-dashboard-architecture.md](../features/materialized-dashboard/materialized-dashboard-architecture.md) — new tables
- Updated [materialized-dashboard-domain.md](../features/materialized-dashboard/materialized-dashboard-domain.md) — Collection worker extension

## Out of scope (unchanged)

- Collection Dashboard behavior and API response
- Per-customer/salesman/wilayah forecast
- Working-day / holiday calendar
- Alert Center / Executive Dashboard integration
- Pelunasan Report, purchasing cash-outflow forecast

## Verification

1. Run Collection snapshot refresh (existing `"Collection"` domain).
2. `GET /api/dashboard/cash-flow-forecast` returns KPIs, daily pace, recovery trend, risks, executive summary.
3. Forecast `CashCollectedMtd` / `MonthCollections` / `MonthFakturOmzet` reconcile with Collection Dashboard for same refresh.
4. Navigate Sidebar → Dashboard → Cash Flow Forecast.
