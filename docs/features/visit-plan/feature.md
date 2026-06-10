# Territory Execution Plan (Visit Plan Materialization)

## Overview

The Territory Execution Plan converts the recurring route template (`BTR_SalesRute` / `BTR_SalesRuteItem`) into a **dated operational visit schedule** with a separate **exception overlay** for one-off changes.

Business framing: same template serves sales visits and collection routes.

## Layers

| Layer | Storage | Mutability | UI |
| ----- | ------- | ---------- | -- |
| Template | `BTR_SalesRute*` | Current state; SM4 overwrites slot items | SM4-Rute |
| Materialized base | `BTR_VisitPlan` | Future rows regenerated on template change; past rows never updated/deleted | SM7 (read-only base grid) |
| Exceptions | `BTR_VisitPlanException` | CRUD for future dates only | SM7 |
| Effective plan | Computed | Derived at read time | SM7 (effective grid) |

## Cycle Rule

- Fixed anchor date (`ROUTE_CYCLE_ANCHOR_DATE`) + continuous 14-day cycle (Minggu-1 / Minggu-2).
- Monday–Saturday only; Sunday produces no materialized rows.
- No reset on month/quarter/year boundaries.

Calendar date maps to `H11`–`H26` via `IRuteCycleCalendar`.

## Materialization

**Hybrid strategy:**

1. **SM4 save** — regenerates future rows for the affected salesman (`FromDate = today`).
2. **Scheduled worker** (`btr.visitplan.worker`) — maintains rolling horizon daily (`VISIT_PLAN_HORIZON_DAYS`, default 90).

Past materialized rows are immutable when templates or exceptions change later.

## Exception Types (v1)

| Type | Effect |
| ---- | ------ |
| Add | Include customer even if not on template that day |
| Remove | Exclude customer even if on template that day |
| Replace | Remove source customer; add replacement (single row) |

Holidays and leave are handled via exceptions, not template edits.

## Authorization

- SM7 access: supervisor level and above (`LEADR`, `SFKTR`, `SYSAD`).
- Salesmen (`SALES`) cannot modify planned schedules.
- No approval workflow in v1.

## Configuration

| Parameter | Default | Purpose |
| --------- | ------- | ------- |
| `ROUTE_CYCLE_ANCHOR_DATE` | `2026-01-05` | Cycle epoch |
| `VISIT_PLAN_HORIZON_DAYS` | `90` | Rolling window |

Administered via XX2 – Parameter Sistem.

## Future Consumers

- **M25 route compliance** — join effective plan ↔ `BTR_CheckIn` / `BTR_Order` via `SalesPerson.Email`
- **FT1 / FT2** — default HariRute from calendar date
- **RO1 / RO3** — optional planned column

## Related Artifacts

- Implementation plan: `docs/work/sales-route-materialization/implementation-plan.md`
- Ops runbook: `src/j05-btr-distrib/docs/ops/visit-plan-deploy.md`
