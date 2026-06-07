# Implementation Summary: Materialized Dashboard Data — Phase 3 (Sales Snapshot)

## Status

Phase 3 is complete. The Sales dashboard (`GET /api/dashboard/sales`) now reads from materialized snapshot tables when a `CURRENT` snapshot exists, with configurable live fallback when no snapshot is present. Aggregation uses **Faktur `GrandTotal`** semantics (replacing `BTR_SalesOmzet` omzet recognition). All 39 Phase 1 + 2 + 3 unit and shadow tests pass. The worker is implemented but not yet schedulable — console host and Task Scheduler setup remain deferred to Phase 4.

**Authoritative plan:** [implementation-plan.md](./implementation-plan.md)  
**Requirements:** [analysis-report.md](./analysis-report.md)  
**Prior phase:** [implementation-summary-phase-2.md](./implementation-summary-phase-2.md)

---

## Goal Delivered

Move Sales dashboard aggregation off the HTTP request path by pre-computing Faktur-based KPIs, weekly trend, and Top 10 salesman ranking into dedicated SQL Server tables. The API contract (`DashboardSalesResponse`, route, MediatR handler) is unchanged.

**Exit criterion met:** API serves Faktur-based snapshot when populated; falls back to live Faktur aggregation when snapshot is empty and `DashboardSnapshot:AllowLiveFallback = true` (default).

---

## Architecture

```text
Refresh path (background):
  IFakturViewDal.ListData(currentMonth)
    + ISalesOmzetTargetDal.SumTargetAmountForMonth
    → DashboardSalesFakturAggregator
    → RefreshDashboardSalesSnapshotWorker
    → DashboardSalesSnapshotDal.ReplaceCurrent()
    → BTR_PortalDashboardSales* tables

Read path (HTTP):
  GET /api/dashboard/sales
    → DashboardSalesDal (facade)
      → snapshot exists: DashboardSalesSnapshotDal.GetCurrent()
      → snapshot missing + fallback: DashboardSalesLiveDal (via aggregator)
      → snapshot missing + no fallback: HTTP 503
```

Snapshot pattern: single active row per domain via `SnapshotKey = 'CURRENT'`. `PeriodYear` / `PeriodMonth` stored on KPI row for month-boundary detection. Child rows (week trend, top salesman) are delete-and-replace on each refresh within a transaction.

---

## Database Changes

### New tables (`btr.sql/Tables/ReportingContext/`)

| Table | Purpose |
| --- | --- |
| `BTR_PortalDashboardSalesKpi` | Layer A headline KPIs including period, target, achievement, pipeline (= 0) |
| `BTR_PortalDashboardSalesWeekTrend` | Layer B weekly `GrandTotal` buckets |
| `BTR_PortalDashboardSalesTopSalesman` | Layer B Top 10 salesman ranking |

### ParamNo seed

| Prefix | Usage |
| --- | --- |
| `PDW` | `SalesWeekTrendId` |
| `PDS` | `SalesTopSalesmanId` |

Added to `btr.sql/DataSeeds/BTR_ParamNo_PortalDashboard.sql`.

All objects registered in `btr.sql.sqlproj`.

---

## Files Added

### Application (`btr.application/ReportingContext/DashboardSnapshotAgg/`)

| File | Purpose |
| --- | --- |
| `Contracts/IDashboardSalesSnapshotDal.cs` | Snapshot read/write contract |
| `Models/DashboardSalesAggregateResult.cs` | Aggregator output / snapshot payload |
| `Services/DashboardSalesFakturAggregator.cs` | Faktur-based aggregation (GrandTotal, weekly trend, Top 10) |
| `UseCases/RefreshDashboardSalesSnapshotRequest.cs` | Worker request/result DTOs |
| `UseCases/RefreshDashboardSalesSnapshotWorker.cs` | Snapshot refresh orchestration |

### Infrastructure (`btr.infrastructure/ReportingContext/`)

| File | Purpose |
| --- | --- |
| `DashboardSnapshotAgg/DashboardSalesSnapshotDal.cs` | Snapshot GET + transactional replace |
| `DashboardSalesAgg/DashboardSalesLiveDal.cs` | Live Faktur aggregation via shared aggregator |

### Tests (`btr.test/ReportingContext/`)

| File | Purpose |
| --- | --- |
| `DashboardSalesFakturAggregatorTest.cs` | 6 unit tests — GrandTotal sums, pipeline=0, customers, weekly trend, Top 10 |
| `DashboardSalesDalTest.cs` | Read-path tests — snapshot priority, live fallback, 503 exception |
| `DashboardSalesSnapshotVerificationTest.cs` | Shadow comparison: aggregator vs live DAL; total vs sum of Faktur GrandTotal |

### SQL

| File | Purpose |
| --- | --- |
| `Tables/ReportingContext/BTR_PortalDashboardSalesKpi.sql` | Sales KPI table |
| `Tables/ReportingContext/BTR_PortalDashboardSalesWeekTrend.sql` | Week trend table + index |
| `Tables/ReportingContext/BTR_PortalDashboardSalesTopSalesman.sql` | Top salesman table + unique rank |

---

## Files Modified

| File | Change |
| --- | --- |
| `DashboardSalesAgg/DashboardSalesDal.cs` | Rewritten as facade: snapshot → fallback → 503 |
| `btr.sql.sqlproj` | Registered Sales snapshot tables |
| `btr.sql/DataSeeds/BTR_ParamNo_PortalDashboard.sql` | Added `PDW`, `PDS` prefixes |
| `btr.application.csproj` | Registered Sales snapshot sources |
| `btr.infrastructure.csproj` | Registered snapshot DAL + DashboardSalesLiveDal |
| `btr.test.csproj` | Registered three new test files |
| `Configurations/InfrastructurePortalExtensions.cs` | DI for sales snapshot DAL + live DAL |
| `Configurations/ApplicationPortalExtensions.cs` | `DashboardSalesFakturAggregator` registration |
| `btr.portal.web/.../SalesDashboardView.vue` | Faktur labels, GeneratedAt display, ranking header |
| `btr.portal.web/.../DashboardHomeView.vue` | Home card label: Invoiced Omzet (Faktur) |
| `btr.portal.web/.../DashboardDetailLayout.vue` | Optional `generatedAt` prop for staleness display |
| `docs/features/btr-portal/btr-portal-domain.md` | Sales KPI definitions updated to Faktur semantics |

---

## Unchanged (by design)

| Area | Notes |
| --- | --- |
| `IDashboardSalesDal` interface | Same `GetSummary()` signature |
| `GetDashboardSalesQuery` / handler | Still delegates to DAL |
| `SalesDashboardController` | Same route `GET /api/dashboard/sales` |
| `DashboardSalesResponse` DTO | Same shape; `PipelineOmzet` always `0`; `GeneratedAt` reflects snapshot refresh time when populated |
| `BTR_SalesOmzet` / `ReconcileSalesOmzetWorker` | Unaffected |
| `SalesReportDal` | Unaffected; same Faktur source for reconciliation |
| Overview home endpoint | Deferred to Phase 4 |

---

## Aggregation Rules (Faktur Cutover)

| Field | Rule |
| --- | --- |
| `TotalOmzet` / `CompletedOmzet` / `TotalAchievement` | `SUM(GrandTotal)` of non-void Fakturs in current month |
| `PipelineOmzet` | Always `0` |
| `TotalFaktur` | Row count |
| `TotalCustomer` | Distinct `CustomerCode` (fallback `Customer` name) |
| `WeeklyTrend` | `GrandTotal` grouped by calendar week (`SalesOmzetChartWeekGrouper`) using `FakturDate` |
| `TopSalesmanRanking` | Top 10 by `SUM(GrandTotal)` per `SalesPersonName` |
| `TotalTarget` | `ISalesOmzetTargetDal.SumTargetAmountForMonth` (unchanged) |
| `AchievementPercent` | `SalesOmzetChartAchievementPolicy.ComputePercent` (unchanged) |
| Void exclusion | Handled by `FakturViewDal` (`VoidDate = '3000-01-01'`) |

---

## Test Results

| Suite | Tests | Result |
| --- | --- | --- |
| `DashboardSalesFakturAggregatorTest` | 6 | Pass |
| `DashboardSalesDalTest` | 3 | Pass |
| `DashboardSalesSnapshotVerificationTest` | 2 | Pass |
| Phase 1 + 2 regression | 28 | Pass |
| **Total Phase 1 + 2 + 3** | **39** | **Pass** |

---

## Deployment Steps (Operations)

1. Deploy `btr.sql` schema (Sales KPI + WeekTrend + TopSalesman tables).
2. Run `DataSeeds/BTR_ParamNo_PortalDashboard.sql` if `PDW` / `PDS` prefixes are missing.
3. Deploy portal API with Phase 3 code (`AllowLiveFallback: true` initially).
4. Run first Sales snapshot refresh (see **Known gap** below).
5. Validate Sales dashboard totals against sum of Sales Report `GrandTotal` for current month over 3–5 business days.
6. Set `AllowLiveFallback: false` when satisfied (Phase 4 cutover).

---

## Known Gap — Worker Scheduling (Phase 4)

`RefreshDashboardSalesSnapshotWorker` is implemented and auto-registered via Scrutor (`INunaServiceVoid<>`), but **`btr.portal.worker` console host does not exist yet**. Until Phase 4:

- Worker can be invoked programmatically (tests, one-off ops script).
- Scheduled 30-minute Sales refresh requires Phase 4 Task Scheduler setup.
- With empty snapshot tables and `AllowLiveFallback: true`, production uses live Faktur aggregation (new semantics, not old SalesOmzet path).

---

## Deferred to Later Phases

| Item | Phase |
| --- | --- |
| `btr.portal.worker` console host | Phase 4 |
| `GET /api/dashboard/overview` + home store switch | Phase 4 |
| Disable live fallback in production | Phase 4 |
| Manual refresh admin endpoint | Phase 5 |

---

## Verification Checklist (Phase 3)

| # | Criterion | Status |
| --- | --- | --- |
| 1 | SQL tables deploy from `btr.sql` | Ready — deploy pending ops |
| 2 | Aggregator unit tests pass | Done (6/6) |
| 3 | Shadow test: aggregator matches live DAL + Faktur totals | Done |
| 4 | Read path: snapshot when populated | Done (code + unit test) |
| 5 | Read path: live fallback when snapshot empty | Done (Faktur-based) |
| 6 | API contract unchanged | Done |
| 7 | `PipelineOmzet` = 0 | Done |
| 8 | Frontend labels + domain doc updated | Done |
| 9 | First production snapshot populated | Pending — requires worker invocation |
| 10 | Production fallback disabled | Pending — Phase 4 cutover |

---

## Next Steps

1. **Phase 4:** Deploy worker host; schedule per-domain refresh (15/30/60 min); implement overview endpoint; run shadow period; disable `AllowLiveFallback`.
2. **Ops:** After schema deploy, execute Sales refresh to populate `BTR_PortalDashboardSales*` before disabling fallback.
3. **Stakeholders:** Communicate Sales semantic shift — dashboard now matches Sales Report Faktur totals; pipeline omzet no longer shown.
