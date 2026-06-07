# Implementation Summary: Purchasing Dashboard V1

## Status

**Ready for re-review.** Purchasing Dashboard V1 is implemented per `implementation-plan.md`. Reviewer closeout actions (documentation, operational deploy, health verification) are complete.

---

## Reviewer Closeout (this session)

| Action | Result |
| --- | --- |
| Update `btr-portal-operational.md` | Done — Purchasing dashboard section, menu/routes, workflow, admin Task Scheduler |
| Update `btr-portal-architecture.md` | Done — endpoint, routing, aggregation, traceability |
| Deploy SQL to target database | Done — 4 `BTR_PortalDashboardPurchasing*` tables + ParamNo seed on `JUDE7/btr` |
| Manual worker refresh | Done — `btr.portal.worker.exe --domain Purchasing --triggered-by Manual` → exit 0 |
| Health endpoint Purchasing Success | Done — after fix to `GetLatestPerDomain` SQL (see below) |
| Task Scheduler (30 min) | Done — `BTR-Portal-Dashboard-Purchasing` registered on dev machine |

---

## Bug Fix During Closeout

`DashboardSnapshotRefreshLogDal.GetLatestPerDomain()` omitted `'Purchasing'` from its SQL `WHERE Domain IN (...)` filter, so `GET /api/health/dashboard-snapshots` always returned `LastRefresh: null` for Purchasing even after a successful refresh. Fixed by adding `'Purchasing'` to the domain list.

---

## Operational Evidence

**SQL tables (JUDE7):**

```text
BTR_PortalDashboardPurchasingKpi
BTR_PortalDashboardPurchasingPostingStatus
BTR_PortalDashboardPurchasingTopPrincipal
BTR_PortalDashboardPurchasingWeekTrend
```

**Refresh log:**

```text
Domain=Purchasing, Status=Success, TriggeredBy=Manual, CompletedAt=2026-06-07 23:08:25
```

**KPI snapshot:** `GrandTotalPurchase=1225387768.00`, `TotalInvoice=7`

**Health (`GET /api/health/dashboard-snapshots`):** Overall `Status=ok`; Purchasing `LastRefresh.Status=Success`, `IntervalMinutes=30`

**Unit tests:** 14 Purchasing-related tests — all passed (aggregator, traceability, snapshot verification, refresh handler)

---

## Documentation Updated

| Document | Change |
| --- | --- |
| `docs/features/btr-portal/btr-portal-operational.md` | Full Purchasing dashboard UX; removed "no dashboard" note; 4-card home; menu/routes; workflow; admin ops |
| `docs/features/btr-portal/btr-portal-architecture.md` | Purchasing API route, frontend route, aggregation, charts, traceability |
| `docs/features/btr-portal/btr-portal-domain.md` | *(prior session)* Purchasing dashboard definitions |
| `docs/features/materialized-dashboard/materialized-dashboard-domain.md` | *(prior session)* Purchasing tables and cadence |
| `docs/archive/materialize-dashboard-data/dashboard-snapshot-worker-runbook.md` | *(prior session)* Purchasing task instructions |

---

## Configuration

Added `PurchasingIntervalMinutes: 30` to machine-specific appsettings:

- `btr.portal.worker/appsettings.JUDE7.json`
- `btr.portal.api/appsettings.JUDE7.json`

(Base `appsettings.json` files already included Purchasing interval from implementation.)

---

## Acceptance Criteria Traceability

| # | Criterion | Status |
| --- | --- | --- |
| 1 | Sidebar + home link to `/dashboard/purchasing` | Implemented |
| 2 | 4th home card with Grand Total Purchase + Total Invoice | Implemented |
| 3 | Title and subtitle on detail page | Implemented |
| 4 | KPI row with pending posting count | Implemented |
| 5 | Weekly trend, posting breakdown, Top 10 Principal | Implemented |
| 6 | Footer KPI exact match with report | Verified via `DashboardPurchasingReportTraceabilityTest` |
| 7 | GeneratedAt + 30 min refresh | Worker + Task Scheduler configured |
| 8 | Void + Retur Beli exclusion | Same `IInvoiceViewDal` as report |
| 9 | SUDAH/BELUM in breakdown | Implemented |
| 10 | Deferred features absent | Confirmed |
| 11 | Read-only | No write endpoints |

---

## Production Deployment Notes

Dev machine Task Scheduler points at local `bin\Release` worker path. Production should follow `docs/archive/materialize-dashboard-data/dashboard-snapshot-worker-runbook.md` with the published worker folder, service account, and IIS-deployed API/worker binaries.

---

## Re-Review Request

All required reviewer actions are complete. Please re-run reviewer-agent against this summary and the updated operational/architecture artifacts.
