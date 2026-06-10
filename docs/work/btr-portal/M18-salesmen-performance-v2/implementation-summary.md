# M18 Salesmen Performance V2 — Implementation Summary

| Field | Value |
| --- | --- |
| Initiative | M18 Salesmen Performance V2 |
| Plan | `docs/work/btr-portal/M18-salesmen-performance-v2/M18-salesmen-performance-v2-plan.md` |
| Status | **Complete** (V2.1–V2.3; V2.4 deferred) |
| Date | 2026-06-11 |

## Delivered

### V2.1 — Release alignment

- Renamed signal `NoTarget` → `MissingTargetSetup` across aggregator, snapshot, API, Alert Center, Investigation registry, and SPA.
- High Piutang / High Overdue exposure signals use **Top 20%** configurable threshold (`DashboardSnapshot:SalesmanExposureTopPercent`).
- `IsActive` persisted on attention and ranking snapshot rows; API exposes `IsActive` and `Filters.DefaultActiveOnly`.
- SPA: **Show Inactive Salesmen** toggle; default active-only filter on list and rankings.

### V2.2 — Principal Achievement drill-down

- New source DAL `IFakturPrincipalOmzetDal` (`FakturItem.Total` by salesman + principal).
- `ISalesPersonPrincipalTargetDal.ListByPeriod` for batch principal targets.
- Snapshot table `BTRPD_SalesmanPrincipalAchievement`.
- API `GET /api/dashboard/salesmen/{salesPersonId}/principals`.
- SPA detail drawer **Principal** tab.

### V2.3 — Achievement trend

- Snapshot table `BTRPD_SalesmanRepHistory` with period-keyed upsert (retains prior months).
- API `GET /api/dashboard/salesmen/{salesPersonId}/trend?months=12`.
- SPA detail drawer **Trend** tab (line chart).

## Database

Deploy: `src/j05-btr-distrib/btr.sql/Scripts/Upgrade_M18_SalesmenPerformance_V2.sql`

## Tests

- Extended `DashboardSalesmanAggregatorTest` (28 cases): signal rename, top-percent exposure, `IsActive`, principal rows, rep history.
- Added `DashboardSalesmanSnapshotRepHistoryTest`: two refreshes same month update one history row; prior months retained on refresh.
- Updated Alert/Investigation registry tests.
- Frontend `salesmanAttentionSignals.spec.ts` (6 cases) — passing.

## Review remediation (2026-06-11)

- Registered `Scripts/Upgrade_M18_SalesmenPerformance_V2.sql` in `btr.sql.sqlproj` (deploy bundle, M14 pattern).
- Added rep history upsert verification test per plan §9.1.

## Knowledge sync

- Updated `docs/features/btr-portal/btr-portal-domain.md` (M18 section).
- Updated `docs/features/materialized-dashboard/materialized-dashboard-domain.md` (Salesman history retention).

## Out of scope (unchanged)

- V2.4 extended rankings / New Customer KPI.
- Visit execution (M18.5), collection performance (M20), executive dashboard changes.
