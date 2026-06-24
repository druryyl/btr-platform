# M26 Sales Forecast Dashboard — Implementation Summary

| Field | Value |
| --- | --- |
| Milestone | M26 — Sales Forecast Dashboard |
| Plan | [implementation-plan-m26-sales-forecast.md](./implementation-plan-m26-sales-forecast.md) |
| Analysis | [portal-analysis-m26-sales-forecast.md](./portal-analysis-m26-sales-forecast.md) |
| Status | **Complete** |
| Date | 2026-06-21 |

## Delivered

### Backend — forecast aggregation

- `SalesOmzetChartDayGrouper` — calendar-day buckets within month (`id-ID` labels).
- `SalesForecastPolicy` — current-pace formulas, confidence, required-daily severity.
- `DashboardSalesForecastAggregator` — daily pace rows, best/worst case, FR-40/41 alignment with Sales KPI.
- Extended `RefreshDashboardSalesSnapshotWorker` — forecast computed in same Sales refresh transaction.
- Extended `DashboardSalesSnapshotDal.ReplaceCurrent` — persists Sales + Forecast snapshots atomically.

### Database

- `BTRPD_SalesForecastKpi` — forecast KPI snapshot (Layer A).
- `BTRPD_SalesDailyPace` — daily pace buckets (Layer B).
- Appended to `Create_BTRPD_PortalDashboard_Tables.sql` and `btr.sql.sqlproj`.

### API

- `GET /api/dashboard/sales-forecast` — MediatR query + read DAL.
- Server-side `SalesForecastExecutiveSummaryBuilder` — risk-band wording templates.
- Weekly trend reused from `BTRPD_SalesWeekTrend` (AD-7).

### Portal Web

- Route `/dashboard/sales-forecast` — sidebar entry **Sales Forecast** (after Sales).
- Components: `SalesForecastSummary`, `SalesForecastKpiRow`, `ForecastVsTargetChart`, `DailyPaceTrendChart`, `ForecastRiskCard`.
- Store: `salesForecast` state + `loadSalesForecast()`.

## Tests

| File | Coverage |
| ---- | -------- |
| `SalesOmzetChartDayGrouperTest.cs` | Bucket count, labels, find-bucket |
| `SalesForecastPolicyTest.cs` | Mid-month, day-1, target achieved/null, last day, DE guard, confidence, severity |
| `DashboardSalesForecastAggregatorTest.cs` | Daily sum, sales parity, recent-7 fallback, scenario ordering |
| `DashboardSalesForecastSnapshotVerificationTest.cs` | FR-40/41, daily row count, elapsed sum |
| `SalesForecastExecutiveSummaryBuilderTest.cs` | All risk-band summary templates |
| `router/index.spec.ts` | Route resolution for `/dashboard/sales-forecast` |

## Knowledge sync

- Created [docs/features/sales-forecast/feature.md](../features/sales-forecast/feature.md)
- Updated [btr-portal-domain.md](../features/btr-portal/btr-portal-domain.md) — M26 roadmap + forecasting layer
- Updated [btr-portal-operational.md](../features/btr-portal/btr-portal-operational.md) — route, API, usage
- Updated [materialized-dashboard-architecture.md](../features/materialized-dashboard/materialized-dashboard-architecture.md) — new tables

## Out of scope (unchanged)

- Sales Dashboard behavior and API response
- Per-salesman forecast
- Working-day / holiday calendar
- Alert Center / Executive Dashboard integration
- Custom period selection
- Pipeline / outstanding omzet inclusion

## Verification

1. Run Sales snapshot refresh (existing `"Sales"` domain).
2. `GET /api/dashboard/sales-forecast` returns KPIs, daily pace, weekly trend, executive summary.
3. Forecast `CurrentSales` = Sales Dashboard `TotalAchievement` for same refresh.
4. Navigate Sidebar → Dashboard → Sales Forecast.
