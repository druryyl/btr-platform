# M30 Collection Optimization Dashboard — Implementation Summary

| Field | Value |
| --- | --- |
| Milestone | M30 — Collection Optimization Dashboard |
| Plan | [implementation-plan-m30-collection-optimization.md](./implementation-plan-m30-collection-optimization.md) |
| Analysis | [portal-analysis-m30-collection-optimization.md](./portal-analysis-m30-collection-optimization.md) |
| Status | **Complete** |
| Date | 2026-06-22 |

## Delivered

### Backend — collection optimization aggregation

- `CollectionOptimizationPolicy` — COL-OPT-CAT action categories, priority score, impact amount, action owner, planning confidence.
- `CollectionOptimizationActionBuilder` — selection/priority/action reason texts and `TriggeredRuleIds`.
- `CollectionOptimizationExecutiveSummaryBuilder` — server-composed daily brief (analysis §10.1).
- `DashboardCollectionOptimizationAggregator` — consumes M29 `CustomerRiskForecastContext` in-memory; M20 cross-read; no forecast recalculation (COL-OPT-06).
- Extended `RefreshDashboardCustomerSnapshotWorker` — M20 cross-read + optimization step after M29.
- Extended `DashboardCustomerSnapshotDal.ReplaceCurrent` — persists M17 + M29 + M30 atomically.
- Extended `DashboardCustomerRiskForecastAggregateResult.Contexts` — exposes in-memory forecast contexts for M30 (mirrors M28.5 `ItemContexts` pattern).

### Database

- `BTRPD_CollectionOptimizationKpi` — workload/context KPIs + executive summary (Layer A).
- `BTRPD_CollectionOptimizationActionDist` — action category distribution.
- `BTRPD_CollectionOptimizationWorkload` — Salesman / Wilayah / Klasifikasi workload (top 10 each).
- `BTRPD_CollectionOptimizationPriority` — top 30 priority queue.
- `BTRPD_CollectionOptimizationQueue` — specialized queues (15 each).
- `BTRPD_CollectionOptimizationImpact` — top 15 impact opportunities.
- Individual table files under `btr.sql/Tables/ReportingContext/` and `Upgrade_M30_CollectionOptimization.sql`.

### API

- `GET /api/dashboard/collection-optimization` — MediatR query + read DAL.
- Graceful degradation when snapshot missing (`IsAvailable = false`, HTTP 200).

### Portal Web

- Route `/dashboard/collection-optimization` — sidebar **Collection Optimization** (after Customer Risk Forecast).
- Components: summary, KPI grids, action/workload/impact charts, priority table (expand detail), specialized queue tabs, impact table.
- Store: `collectionOptimization` state + `loadCollectionOptimization()`.
- Service: `collectionOptimizationSignals.ts` — category labels, owner badges, queue filters.

### Configuration

- `DashboardSnapshot:CollectionOptimization*` caps/thresholds in API and Worker appsettings.

## Tests

| File | Coverage |
| ---- | -------- |
| `CollectionOptimizationPolicyTest.cs` | COL-OPT-CAT precedence, sales recovery floor, priority score, impact formula, action owner |
| `CollectionOptimizationActionBuilderTest.cs` | Reason text, TriggeredRuleIds (COL-OPT-REC-02/05) |
| `DashboardCollectionOptimizationAggregatorTest.cs` | Synthetic portfolio, M20 null degrade, priority cap, rank-1 match |
| `collectionOptimizationSignals.spec.ts` | Category labels, queue filter, severity |
| `router/index.spec.ts` | Route `/dashboard/collection-optimization` |

## Knowledge sync

- Created [docs/features/collection-optimization/feature.md](../features/collection-optimization/feature.md)
- Updated [btr-portal-domain.md](../features/btr-portal/btr-portal-domain.md) — M30 dashboard
- Updated [btr-portal-operational.md](../features/btr-portal/btr-portal-operational.md) — route, API, usage

## Out of scope (unchanged)

- M29 `/api/dashboard/customer-risk-forecast` response shape
- M20/M17/M14 dashboard APIs
- Alert Center / Executive integration
- Automatic collection actions or Desktop write-back

## Acceptance criteria

| Criterion | Status |
| --------- | ------ |
| Dashboard at `/dashboard/collection-optimization` | Done |
| Executive summary + COL-OPT-KPI workload/context KPIs | Done |
| Priority queue top 30 with explainability | Done |
| Specialized queues by `QueueKey` | Done |
| Customer worker refresh includes M30 | Done |
| COL-OPT-KPI-51 recovery from M20 cross-read | Done |
| COL-OPT-06 no M29 forecast re-evaluation | Done |
| Traceability footer + disclaimer | Done |
