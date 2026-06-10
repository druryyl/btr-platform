# Implementation Summary: Materialized Dashboard Data — Phase 4 (Worker, Overview, Cutover)

## Status

Phase 4 is complete. The `btr.portal.worker` console host schedules per-domain snapshot refresh. Dashboard home loads from `GET /api/dashboard/overview` (Layer A KPI reads only). Live aggregation fallback has been removed from analytical dashboard read paths. All 50 ReportingContext unit and shadow tests pass; frontend builds cleanly.

**Authoritative plan:** [implementation-plan.md](./implementation-plan.md)  
**Prior phase:** [implementation-summary-phase-3.md](./implementation-summary-phase-3.md)  
**Operations runbook:** [dashboard-snapshot-worker-runbook.md](./dashboard-snapshot-worker-runbook.md)

---

## Goal Delivered

Complete the materialized dashboard rollout: schedulable background refresh, fast overview home path, and production cutover to snapshot-only reads.

**Exit criterion met:** Worker CLI operational with per-domain Task Scheduler jobs documented; overview endpoint live; read DALs serve snapshot or HTTP 503 only.

---

## Architecture

```text
Task Scheduler (15 / 30 / 60 min)
    ↓
btr.portal.worker.exe --domain {Piutang|Sales|Inventory}
    ↓
RefreshDashboard*SnapshotWorker
    ↓
BTRPD_* tables

Browser → GET /api/dashboard/overview     (home — Layer A KPI only)
Browser → GET /api/dashboard/{domain}     (detail — Layer A + B)
    ↓ snapshot read or 503 if empty
```

Initial backfill / manual ops: `btr.portal.worker.exe --domain All --triggered-by Manual`

---

## Files Added

### Worker host (`btr.portal.worker/`)

| File | Purpose |
| --- | --- |
| `Program.cs` | CLI entry — parses `--domain` and `--triggered-by`; exit code 0/1 |
| `WorkerDependencyConfig.cs` | DI via shared portal Application/Infrastructure extensions |
| `appsettings.json` | Database + refresh cadence config |
| `NLog.config` | File + console logging |
| `btr.portal.worker.csproj` | net48 console EXE; added to `j05-btr-distrib.sln` |

### Application

| File | Purpose |
| --- | --- |
| `RefreshAllDashboardSnapshotsWorker.cs` | Orchestrator — Piutang → Inventory → Sales; per-domain failure isolation |
| `RefreshAllDashboardSnapshotsRequest.cs` | Request/result DTOs |
| `DashboardOverviewAgg/Contracts/IDashboardOverviewDal.cs` | Overview read contract |
| `DashboardOverviewAgg/Queries/GetDashboardOverviewQuery.cs` | MediatR query + response DTOs |

### Infrastructure

| File | Purpose |
| --- | --- |
| `DashboardOverviewDal.cs` | Three KPI-table SELECTs; partial response when domain missing |

### API

| File | Purpose |
| --- | --- |
| `OverviewDashboardController.cs` | `GET /api/dashboard/overview` |

### Tests

| File | Purpose |
| --- | --- |
| `RefreshAllDashboardSnapshotsWorkerTest.cs` | Orchestrator order + aggregate failure |
| `DashboardOverviewDalTest.cs` | Layer A mapping + unavailable flag |

### Documentation

| File | Purpose |
| --- | --- |
| `dashboard-snapshot-worker-runbook.md` | Task Scheduler setup, monitoring, rollback |

---

## Files Modified

| File | Change |
| --- | --- |
| `Dashboard*Dal.cs` (×3) | Snapshot-only read path; removed live fallback |
| `InfrastructurePortalExtensions.cs` | Registered `IDashboardOverviewDal`; removed Live DAL DI |
| `PortalPresentationExtensions.cs` | Registered `OverviewDashboardController` |
| `DashboardSnapshotOptions.cs` | Replaced `AllowLiveFallback` with interval metadata |
| `appsettings.json` | Removed fallback flag; added interval minutes |
| `dashboardStore.ts` | Home uses `fetchDashboardOverview()` |
| `DashboardHomeView.vue` | Binds to `overview` state |
| `dashboardApi.ts` / `dashboard.ts` | Overview API + types |
| `Dashboard*DalTest.cs` (×3) | Removed fallback tests |
| `btr-portal-domain.md` | Overview data source documented |
| `btr.application.csproj`, `btr.infrastructure.csproj`, `btr.portal.api.csproj`, `btr.test.csproj` | Registered new sources |

---

## Cutover Changes

| Before (Phases 1–3) | After (Phase 4) |
| --- | --- |
| `AllowLiveFallback: true` default | Fallback removed — snapshot required |
| Home calls 3 full dashboard endpoints | Home calls `/api/dashboard/overview` |
| Workers implemented but not schedulable | `btr.portal.worker` CLI + runbook |
| No orchestrator | `RefreshAllDashboardSnapshotsWorker` for `--domain All` |

Live DAL classes (`Dashboard*LiveDal`) remain for shadow verification tests only.

---

## Test Results

| Suite | Tests | Result |
| --- | --- | --- |
| Phase 4 new tests | 4 | Pass |
| Full ReportingContext suite | 50 | Pass |
| `npm run build` (portal web) | — | Pass |
| `btr.portal.worker` build | — | Pass |

---

## Deployment Steps (Operations)

1. Deploy Phase 4 portal API + worker binaries.
2. Run initial backfill: `btr.portal.worker.exe --domain All --triggered-by Manual`
3. Register three Task Scheduler jobs per [runbook](./dashboard-snapshot-worker-runbook.md).
4. Verify overview home shows KPIs with per-domain `GeneratedAt`.
5. Monitor `BTRPD_RefreshLog` for `Success` status.

---

## Deferred to Phase 5

| Item | Notes |
| --- | --- |
| `POST /api/admin/dashboard/refresh` | Manual refresh admin endpoint |
| `HealthController` refresh status | Optional observability |
| Layer C piutang open fact | Only if reconciliation requires |

---

## Verification Checklist (Phase 4)

| # | Criterion | Status |
| --- | --- | --- |
| 1 | `btr.portal.worker` builds and runs CLI | Done |
| 2 | Per-domain + All orchestrator | Done |
| 3 | Task Scheduler runbook documented | Done |
| 4 | `GET /api/dashboard/overview` Layer A only | Done |
| 5 | Home page uses overview endpoint | Done |
| 6 | Live fallback removed from read DALs | Done |
| 7 | Unit tests pass | Done (50/50) |
| 8 | Production scheduler jobs registered | Pending — ops |
