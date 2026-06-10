# Visit Plan Deploy Runbook

Operations guide for Territory Execution Plan materialization (`BTR_VisitPlan`) and exception management.

## Prerequisites

1. Deploy `btr.sql` schema including:
   - `BTR_VisitPlan`
   - `BTR_VisitPlanException`
2. Run data seeds:
   - `DataSeeds/BTR_ParamSistem_VisitPlan.sql`
   - `DataSeeds/BTR_Menu.dataseed.sql` (SM7 menu)
   - `DataSeeds/BTR_RoleMenu_VisitPlan.sql`
3. Publish desktop (`btr.distrib`) and worker (`btr.visitplan.worker`) to the application server.

## System Parameters

Configure via **XX2 – Parameter Sistem**:

| ParamCode | Default | Purpose |
| --------- | ------- | ------- |
| `ROUTE_CYCLE_ANCHOR_DATE` | `2026-01-05` | Fixed Monday anchor for Minggu-1 / Minggu-2 cycle |
| `VISIT_PLAN_HORIZON_DAYS` | `90` | Rolling materialization window (days ahead of today) |

Confirm anchor date with business before first production run.

## Initial Backfill

After schema deploy:

```text
btr.visitplan.worker.exe --triggered-by Manual
```

This materializes `[today, today + horizon]` for all salesmen with route templates.

Do **not** backfill dates before go-live unless business explicitly requests it — historical template state is not recoverable.

## Scheduled Worker

| Setting | Value |
| ------- | ----- |
| Task name | `BTR-VisitPlan-Horizon` |
| Schedule | Daily at 02:00 local |
| Command | `btr.visitplan.worker.exe --triggered-by Scheduler` |
| Stop if running | > 30 minutes |

Worker config: `appsettings.json` (plus optional `appsettings.{MachineName}.json` override).

Logs: `logs/btr-visitplan-worker-{date}.log` under the worker install directory.

## Desktop Integration

- **SM4-Rute** — template save triggers future plan regeneration for the edited salesman (past rows unchanged).
- **SM7-Jadwal Kunjungan** — supervisor exception management (Add / Remove / Replace).

### Role Access

SM7 is granted by default to `SYSAD`, `LEADR`, and `SFKTR` via seed. Existing deployments must assign SM7 via **XX4-Role** if seeds were not re-run.

`SALES` role must **not** have SM7 access.

## Manual Recovery

If the scheduled worker was offline:

1. Run manual worker: `--triggered-by Manual`
2. Regeneration is idempotent for the horizon window — no duplicate rows expected.

If SM4 save regen failed (check desktop/worker logs), the scheduled worker fills gaps on next run.

## Three-Layer Model (Operations Reference)

| Layer | Table / Form | Purpose |
| ----- | ------------ | ------- |
| Template | `BTR_SalesRute*` / SM4 | Recurring Territory Execution Plan |
| Materialized base | `BTR_VisitPlan` | Dated template expansion (past rows immutable) |
| Exceptions | `BTR_VisitPlanException` / SM7 | One-off schedule changes |

Effective plan = materialized base ⊕ exceptions (computed at read time).
