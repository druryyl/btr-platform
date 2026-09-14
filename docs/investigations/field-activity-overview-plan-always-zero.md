# Investigation — Sales Force Overview `PLAN` Always 0

**Type:** Bug investigation (Analyst + Architect) — root cause and recommended solution.
**Surface:** `GET /api/dashboard/field-activity/overview?visitDate=2026-06-20` in BTR Portal (`btr.portal.web` → `btr.portal.api`).
**Status:** Root cause confirmed against the live database (`JUDE7.btr2`).
**Related artifacts:** `docs/features/field-activity-overview/feature.md`, `docs/features/visit-plan/feature.md`, `docs/ops/visit-plan-deploy.md`, `docs/investigations/sales-route-materialization-analysis.md`.

---

## 1. Executive Summary

The `PLAN` metric on the Sales Force Overview dashboard is the **Planned Visits** KPI (`PlannedVisits`). It is derived **only** from the materialized visit-plan tables **`BTR_VisitPlan`** (base plan) plus **`BTR_VisitPlanException`** (add/remove/replace overrides), joined to salesmen by `SalesPersonId`.

In the database the portal reads (`JUDE7.btr2`), **`BTR_VisitPlan` contains 0 rows** (and `BTR_VisitPlanException` 0 rows). Therefore `PlannedVisits` is 0 for every salesman and for the team total, while Actual Visits and Orders are populated normally (`BTR_CheckIn`, `BTR_Order`).

The visit-plan materialization pipeline (`btr.visitplan.worker`) has never successfully populated `btr2`:

1. The worker requires system parameter `ROUTE_CYCLE_ANCHOR_DATE`; it is **not configured** in `btr2`. Worker logs show `System parameter 'ROUTE_CYCLE_ANCHOR_DATE' is not configured.` for every salesman — so no plan rows are ever written.
2. Materialization is **forward-only from the real system "today"** and there is **no historical backfill** (by design). The portal runs in **presentation mode pinned to business date `2026-06-20`**, while the real system date is `2026-09-13`; even a healthy worker would not produce plan rows for `2026-06-20`.
3. `btr2` was created `2026-06-26`, **after** the only worker runs logged (`2026-06-11` … `2026-06-22`, which targeted the earlier `btr` database). `btr2` was never seeded/materialized with visit plans.

---

## 2. Data Path (which table the data comes from)

### 2.1 Request handling

`FieldActivityDashboardController.GetOverview` (`btr.portal.api/Controllers/Dashboard/FieldActivityDashboardController.cs:57`) sends `GetFieldActivityOverviewQuery` (`btr.application/.../DashboardFieldActivityOverviewAgg/Queries/GetFieldActivityOverviewQuery.cs`).

- `visitDay = request.VisitDate ?? businessDateProvider.Today`.
- Because `Presentation.BusinessDate = 2026-06-20` and the request date equals it, the handler returns the **cached snapshot** (`_snapshotDal.GetCurrent()`), i.e. `BTRPD_FieldActivityKpi` / `BTRPD_FieldActivitySalesman` / `BTRPD_FieldActivityTrend` with `SnapshotKey = 'CURRENT'`.
- Otherwise it runs the live composer (`FieldActivityOverviewComposer`). Both paths share the same plan source.

### 2.2 Source tables

| Layer | Table(s) | Role |
| ----- | -------- | ---- |
| Plan template | `BTR_SalesRute` + `BTR_SalesRuteItem` | Recurring Territory Execution Plan (2-week cycle) |
| **Materialized plan (PLAN source)** | **`BTR_VisitPlan`** | Dated effective plan rows `(SalesPersonId, VisitDate, CustomerId, NoUrut, HariRuteId, PlanSource, MaterializedAt)` |
| Plan exceptions | `BTR_VisitPlanException` | Add / Remove / Replace overrides for a date |
| Portal snapshot cache | `BTRPD_FieldActivityKpi.PlannedVisits`, `BTRPD_FieldActivitySalesman.PlannedVisits` | Snapshot of the computed KPI |

Read path for PLAN:

```text
BTR_VisitPlan  +  BTR_VisitPlanException
      │  (FieldActivityBatchVisitPlanDal.ListByDate)
      ▼
EffectiveVisitPlanResolver.Resolve()  → EffectiveVisitPlanEntry[]
      ▼
FieldActivityKpiCalculator.Compute()  → PlannedVisits = plan.Count
      ▼
FieldActivityOverviewComposer.BuildSalesmanRowsForDate()
      ▼
TeamKpis.PlannedVisits / SalesmanRow.PlannedVisits
      ▼
BTRPD_FieldActivityKpi / BTRPD_FieldActivitySalesman  →  "Planned" KPI & table column
```

Key code:

- `btr.infrastructure/ReportingContext/DashboardFieldActivityOverviewAgg/FieldActivityBatchVisitPlanDal.cs:32` — `FROM BTR_VisitPlan` (and `:43` `FROM BTR_VisitPlanException`).
- `btr.application/.../DashboardFieldActivityOverviewAgg/Services/FieldActivityOverviewComposer.cs:52,164` — `planDataAvailable = visitDay >= VisitPlanGoLiveDate`; plan attached per `salesman.SalesPersonId`.
- `btr.application/.../DashboardFieldActivityOverviewAgg/Services/FieldActivityKpiCalculator.cs:46,82` — `PlannedVisits = plan.Count`.
- UI: `FieldActivityTeamKpiStrip.vue:63` ("Planned") and `FieldActivitySalesmanTable.vue:72` ("Planned").

Plan availability gate: `FieldActivity.VisitPlanGoLiveDate = 2026-03-01` (all appsettings). `2026-06-20` is after go-live, so the code **expects** plan data — the zero is not the go-live gate.

---

## 3. Evidence

### 3.1 Database (`JUDE7.btr2`, the DB the API reads)

```text
BTR_VisitPlan rows total .......................... 0
BTR_VisitPlan 2026-06-20 .......................... 0
BTR_VisitPlanException 2026-06-20 ................. 0
BTR_SalesRute rows ................................ 68   (routes exist)
BTR_SalesRuteItem rows ............................ 287
BTR_CheckIn 2026-06-20 ............................ 108
BTR_Order 2026-06-20 .............................. 74
BTRPD_FieldActivityKpi (CURRENT) .................. ActivityDate=2026-06-20,
                                                     PlannedVisits=0, ActualVisits=106, TotalOrders=74
BTR_ParamSistem total rows ........................ 7  (none route/visit-plan related)
BTR_ParamSistem 'ROUTE_CYCLE_ANCHOR_DATE' ......... 0 rows (NOT CONFIGURED)
```

`BTRPD_FieldActivitySalesman` has 41 rows; every `PlannedVisits` value is 0, while `ActualVisits` is populated for salesmen with email.

### 3.2 Worker logs (`btr.visitplan.worker/bin/Debug/logs`)

`btr-visitplan-worker-2026-06-11.log`:

```text
ERROR btr.application...RegenerateVisitPlanWorker|
  Visit plan regeneration failed for SalesPersonId="SP000", TriggeredBy="Scheduler"|
  System.InvalidOperationException: System parameter 'ROUTE_CYCLE_ANCHOR_DATE' is not configured.
    at RuteCycleCalendar.GetAnchorDate()  (RuteCycleCalendar.cs:61)
    at RuteCycleCalendar.ResolveHariRuteId(DateTime visitDate)  (RuteCycleCalendar.cs:24)
    at RegenerateVisitPlanWorker.RegenerateForSalesPerson(...)  (RegenerateVisitPlanWorker.cs:101)
```

This repeats for every salesman (`SP000`, `SP001`, `SP002`, `SP003`, `SP009`, …), then the coordinator reports "completed successfully" because the exception is swallowed per-salesman (`RegenerateVisitPlanWorker.cs:62-76`).

### 3.3 Environment timeline

| Event | Date |
| ----- | ---- |
| `btr` database created | 2026-06-06 |
| Visit-plan worker introduced / run | 2026-06-11 |
| Worker last logged runs | 2026-06-21 / 2026-06-22 |
| **`btr2` database created** | **2026-06-26** |
| Presentation `BusinessDate` | 2026-06-20 (real system date 2026-09-13) |

The worker runs predate `btr2`; the current portal database was created afterwards and never received visit-plan materialization.

---

## 4. Root Cause

**Primary:** `BTR_VisitPlan` is empty in the database the portal reads (`btr2`), so `PlannedVisits` is always 0. The KPI has no fallback to the route template (`BTR_SalesRuteItem`) or to on-the-fly cycle expansion.

**Contributing causes:**

1. **Missing system parameter `ROUTE_CYCLE_ANCHOR_DATE`** in `btr2`. `RuteCycleCalendar.ResolveHariRuteId` throws without it, and `RegenerateVisitPlanWorker` silently skips every salesman, so materialization produces nothing. (`VISIT_PLAN_HORIZON_DAYS` is also absent; it is optional and defaults to 90.)
2. **Forward-only materialization with no backfill.** `RegenerateVisitPlanWorker.Execute` clamps `fromDate` to the real `today` (`RegenerateVisitPlanWorker.cs:52`) and `VisitPlanDal.DeleteFuture` only touches `VisitDate >= today`. Presentation mode pins the business date to `2026-06-20`, which the worker window never covers.
3. **`btr2` gap.** The materialization that ran in June targeted the earlier database; `btr2` (created 2026-06-26) never got a backfill.

Note: `FieldActivityOverviewComposer` only attaches a plan when the salesman `HasEmail`; salesmen with no email also show `PlannedVisits = 0` (`FieldActivityOverviewComposer.cs:171-173`). This is a secondary, per-row effect and not the cause of the all-zero plan here (plan table is globally empty).

---

## 5. Recommended Solution

### Option A — Restore plan data for the presentation database (fixes the symptom immediately)

1. Seed the two required system parameters in `btr2` (`BTR_ParamSistem`), matching the approved rules in `docs/ops/visit-plan-deploy.md`:
   - `ROUTE_CYCLE_ANCHOR_DATE` = approved Monday anchor (e.g. `2026-01-05`).
   - `VISIT_PLAN_HORIZON_DAYS` = `90` (optional; defaults to 90).
2. Materialize `BTR_VisitPlan` for the presentation date. Because the worker is forward-only from the **real** clock, one of:
   - Temporarily set the server/system date (or run a dedicated backfill utility) to `2026-06-20` and run `btr.visitplan.worker.exe --triggered-by Manual`; or
   - Provide a one-time SQL backfill that expands `BTR_SalesRute` + `BTR_SalesRuteItem` for `2026-06-20` using the same cycle rule (anchor → Minggu-1/2 → weekday → `HariRuteId`). A demo seed pattern already exists in `btr.sql/Scripts/Seed_FieldActivity_Demo.sql` (it currently seeds "yesterday", not `2026-06-20`).
3. Re-run the portal snapshot worker (`btr.portal.worker.exe --domain FieldActivity` or `--domain All`) so `BTRPD_FieldActivityKpi` / `BTRPD_FieldActivitySalesman` recompute `PlannedVisits` from the new `BTR_VisitPlan` rows (presentation mode requires re-run after changing the business date / data — see `docs/ops/btr-portal-deploy.md:194`).
4. Verify: `SELECT PlannedVisits FROM BTRPD_FieldActivityKpi` is non-zero and `Planned >= Actual` sanity holds for the date.

### Option B — Make plan generation robust (recommended for production quality)

1. **Fail fast / surface the misconfiguration:** `RuteCycleCalendar.GetAnchorDate` currently throws, and `RegenerateVisitPlanWorker` swallows it per salesman, so the worker reports success while writing nothing. Log at worker level and/or fail the run when the anchor parameter is missing.
2. **Seed the required parameters** with the schema/DataSeeds so a fresh database is materialization-ready (`DataSeeds/BTR_ParamSistem_VisitPlan.sql` is referenced by the runbook but verify it is applied to every environment, including `btr2`).
3. **Provide an explicit, guarded backfill path** (ops-only) rather than silently enforcing `fromDate >= today`, so a historical/presentation date can be materialized deliberately without changing the system clock. Keep the default "no backfill" behavior for normal operation.
4. **Deployment checklist:** after any database restore/clone (e.g. `btr` → `btr2`), re-apply visit-plan parameters, run the worker, and re-run the portal snapshot worker.

### Option C — Guarantee the demo regardless of plan data (fallback only)

If plan history genuinely cannot be produced, the dashboard already has a `Meta.PlanDataAvailable` banner mechanism (`FieldActivityOverviewView.vue:100-106`). Extend the semantics so an empty `BTR_VisitPlan` for an otherwise-available date renders "planned data unavailable" rather than a misleading `0`, and label the KPI as N/A. This avoids presenting a false zero but does not restore the PLAN KPI.

---

## 6. Verification Queries

```sql
-- Plan source must be non-empty for the viewed date
SELECT COUNT(*) FROM BTR_VisitPlan WHERE VisitDate = '2026-06-20';
SELECT COUNT(*) FROM BTR_VisitPlanException WHERE VisitDate = '2026-06-20';

-- Required parameters
SELECT ParamCode, ParamValue FROM BTR_ParamSistem
WHERE ParamCode IN ('ROUTE_CYCLE_ANCHOR_DATE','VISIT_PLAN_HORIZON_DAYS');

-- Snapshot consumed by the endpoint
SELECT ActivityDate, PlannedVisits, ActualVisits FROM BTRPD_FieldActivityKpi WHERE SnapshotKey='CURRENT';
SELECT COUNT(*) AS RowsWithPlan FROM BTRPD_FieldActivitySalesman WHERE SnapshotKey='CURRENT' AND PlannedVisits > 0;
```

---

## 7. Acceptance / Done Criteria

- `BTR_VisitPlan` has rows for the viewed date.
- `ROUTE_CYCLE_ANCHOR_DATE` is configured in the portal database.
- Portal snapshot re-run makes `BTRPD_FieldActivityKpi.PlannedVisits > 0`.
- `GET /api/dashboard/field-activity/overview?visitDate=2026-06-20` returns non-zero `TeamKpis.PlannedVisits` and per-salesman `PlannedVisits`, with `VisitExecutionPercent` computed from them.

---

*Investigation produced from source review, live `JUDE7.btr2` queries, and worker logs. No code changed.*
