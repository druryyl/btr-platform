# M29 Customer Risk Forecast Dashboard — Implementation Summary

| Field | Value |
| --- | --- |
| Milestone | M29 — Customer Risk Forecast Dashboard |
| Plan | [implementation-plan-m29-customer-risk-forecast.md](./implementation-plan-m29-customer-risk-forecast.md) |
| Analysis | [portal-analysis-m29-customer-risk-forecast.md](./portal-analysis-m29-customer-risk-forecast.md) |
| Status | **Complete** |
| Date | 2026-06-21 |

## Delivered

### Backend — forecast aggregation

- `CustomerRiskForecastPolicy` — category resolution, priority score, projected balance/omzet, portfolio health, confidence bands.
- `CustomerRiskSignalBuilder` — payment delay, credit limit, inactivity, purchase decline, and collection risk rules (CRF-P/C/I/D/L).
- `CustomerRiskRecommendationBuilder` — ten recommendation types with priority ordering.
- `CustomerRiskExecutiveSummaryBuilder` — server-composed plain-language summary.
- `DashboardCustomerRiskForecastAggregator` — full portfolio processing with CRF-KPI-50 traceability.
- Extended `RefreshDashboardCustomerSnapshotWorker` — 7 load steps; M17 + M29 in one Customer refresh.
- Extended `DashboardCustomerSnapshotDal.ReplaceCurrent` — persists Customer Analytics + Customer Risk Forecast atomically.

### Source DALs

- `ICustomerPelunasanSummaryDal` / `CustomerPelunasanSummaryDal` — customer payment recency (30d lookback).
- `ICustomerPaymentBehaviorDal` / `CustomerPaymentBehaviorDal` — average payment lag (90d lookback, min 2 settled Fakturs).
- `ICustomerOmzetHistoryDal` / `CustomerOmzetHistoryDal` — current + prior month omzet in one query.

### Database

- `BTRPD_CustomerRiskForecastKpi` — portfolio KPIs + executive summary (Layer A).
- `BTRPD_CustomerRiskForecastDist` — category distribution (5 rows).
- `BTRPD_CustomerRiskForecastWilayah` — top 10 wilayah by elevated-risk count.
- `BTRPD_CustomerRiskForecastSignalMix` — signal family counts.
- `BTRPD_CustomerRiskForecastCustomer` — top 20 risk rows.
- `BTRPD_CustomerRiskForecastAttention` — top 25 forecast attention signals.
- `BTRPD_CustomerRiskForecastRecommendation` — top 15 recommended actions.
- Appended to `Create_BTRPD_PortalDashboard_Tables.sql`, individual table files, and `Upgrade_M29_CustomerRiskForecast.sql`.

### API

- `GET /api/dashboard/customer-risk-forecast` — MediatR query + read DAL.
- Graceful degradation when snapshot missing (`IsAvailable = false`, HTTP 200).

### Portal Web

- Route `/dashboard/customer-risk-forecast` — sidebar entry **Customer Risk Forecast** (after Customer Analytics).
- Components: `CustomerRiskForecastSummary`, `CustomerRiskForecastKpiGrid`, category/wilayah/signal/exposure charts, customers table, attention list, recommendations.
- Store: `customerRiskForecast` state + `loadCustomerRiskForecast()`.
- Service: `customerRiskForecastSignals.ts` — category colors, signal labels, filters.

### Configuration

- `DashboardSnapshot:CustomerRiskForecastHorizonDays` (30) and related caps/thresholds in API and Worker appsettings.

## Tests

| File | Coverage |
| ---- | -------- |
| `CustomerRiskForecastPolicyTest.cs` | Projected balance, decline ratios, categories, priority score, confidence, zero-piutang |
| `CustomerRiskSignalBuilderTest.cs` | CRF-P01, CRF-I01, CRF-C01, CRF-L02, dedupe |
| `CustomerRiskRecommendationBuilderTest.cs` | Critical→ManagementReview, EarlyCollection, SalesRecovery |
| `DashboardCustomerRiskForecastAggregatorTest.cs` | End-to-end portfolio, CRF-KPI-50, top-20 cap, CRF-G03 exclusion |
| `customerRiskForecastSignals.spec.ts` | Signal labels, category colors |
| `router/index.spec.ts` | Route resolution for `/dashboard/customer-risk-forecast` |

## Knowledge sync

- Created [docs/features/customer-risk-forecast/feature.md](../features/customer-risk-forecast/feature.md)
- Updated [btr-portal-domain.md](../features/btr-portal/btr-portal-domain.md) — M29 dashboard
- Updated [btr-portal-operational.md](../features/btr-portal/btr-portal-operational.md) — route, API, usage

## Out of scope (unchanged)

- Customer Analytics Dashboard API response shape
- Collection / Cash Flow Forecast behavior
- Alert Center / Executive Dashboard integration
- Per-salesman or per-wilayah forecast pages
- AI/ML probability scoring

## Verification

1. Deploy `Upgrade_M29_CustomerRiskForecast.sql` (or full portal dashboard DDL).
2. Run Customer snapshot refresh (`domain=Customer`).
3. `GET /api/dashboard/customer-risk-forecast` returns KPIs, distribution, charts data, top customers, attention, recommendations, executive summary.
4. CRF-KPI-07 matches piutang sum from same refresh load.
5. Navigate Sidebar → Dashboard → Customer Risk Forecast; disclaimer and traceability footer visible.
6. Confirm `GET /api/dashboard/customers` unchanged (non-regression).
