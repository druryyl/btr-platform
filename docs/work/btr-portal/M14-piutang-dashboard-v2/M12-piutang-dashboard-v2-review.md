# Rejection Report

**Phase:** M14 Piutang Dashboard V2 (full implementation)  
**Review Date:** 2026-06-10  
**Plan reviewed:** `docs/work/btr-portal/M14-piutang-dashboard-v2/M14-piutang-dashboard-v2-plan.md`  
**Reviewer:** Reviewer Agent

---

## Reviewed Artifacts

- `src/j05-btr-distrib/btr.sql/` — KPI alter, new tables, upgrade script, sqlproj
- `src/j05-btr-distrib/btr.application/ReportingContext/DashboardSnapshotAgg/` — aggregator, resolver, models, worker
- `src/j05-btr-distrib/btr.infrastructure/ReportingContext/DashboardSnapshotAgg/` — snapshot DAL, open-balance DAL
- `src/j05-btr-distrib/btr.application/ReportingContext/DashboardPiutangAgg/` — API query/DTOs
- `src/j05-btr-distrib/btr.infrastructure/ReportingContext/DashboardPiutangAgg/` — read DAL
- `src/j05-btr-distrib/btr.application/ReportingContext/DashboardExecutiveAgg/` — executive composer
- `src/j05-btr-distrib/btr.application/ReportingContext/DashboardAlertCenterAgg/` — alert center composer
- `src/j05-btr-distrib/btr.portal.web/` — Piutang dashboard view, risk table, types
- `src/j05-btr-distrib/btr.test/ReportingContext/` — Piutang, Executive, Alert Center tests
- `docs/features/btr-portal/btr-portal-domain.md`
- `docs/features/materialized-dashboard/materialized-dashboard-domain.md`
- `docs/work/btr-portal/M14-piutang-dashboard-v2/implementation-summary.md`

---

## Checklist

| Item | Result |
| ---- | ------ |
| Requirement implementation | **FAIL** — backend does not compile |
| Acceptance criteria | **FAIL** — automated verification blocked by build failure |
| Architecture compliance | **PASS** — topology matches plan (snapshot worker extension, new tables, API additive fields) |
| Scope completeness | **PASS** — planned deliverables present; no unauthorized cross-domain worker changes |
| Test coverage | **FAIL** — tests exist but solution build fails; integration test scope not met |
| Documentation | **PASS** — feature artifacts and implementation summary updated |
| Frontend build | **PASS** — `npm run build` succeeds |
| Backend build | **FAIL** — `dotnet build` fails |

---

## Findings

### Critical

1. **Solution does not compile — duplicate `DashboardPiutangTopCustomerRiskRow` types**

   `DashboardPiutangAggregator.cs` imports both `DashboardPiutangAgg.Queries` and `DashboardSnapshotAgg.Models`. Both namespaces define `DashboardPiutangTopCustomerRiskRow`, causing:

   ```
   error CS0104: 'DashboardPiutangTopCustomerRiskRow' is an ambiguous reference between
   '...DashboardSnapshotAgg.Models.DashboardPiutangTopCustomerRiskRow' and
   '...DashboardPiutangAgg.Queries.DashboardPiutangTopCustomerRiskRow'
   ```

   Evidence: `dotnet build btr.test/btr.test.csproj` fails at `DashboardPiutangAggregator.cs:94`.

   The plan (§6.3) places aggregate-layer types in `DashboardSnapshotAgg/Models`. `DashboardPiutangAggregateResult` incorrectly imports `DashboardPiutangAgg.Queries`, coupling the snapshot domain to API DTOs and introducing the duplicate type. **No backend artifact can ship until this is resolved.**

2. **Implementation summary overstates verification**

   `implementation-summary.md` states `dotnet test` passes for Piutang, Executive, and Alert Center reporting tests. Reviewer could not reproduce: `dotnet build` fails before any test execution. Claims of passing backend tests are not evidenced.

### Major

3. **Plan §8.2 integration verification not implemented**

   The plan requires `DashboardPiutangSnapshotVerificationTest` to validate reconciliation **after refresh against test DB**, including row counts on `BTRPD_PiutangCustomerAging`. The delivered test is an in-memory aggregator scenario only — no snapshot DAL round-trip, no database refresh, no persisted row-count assertion. The test name implies integration scope that is not delivered.

4. **Manual verification checklist (plan §8.5 V1–V7) not evidenced**

   No test logs, screenshots, or checklist sign-off for worker refresh, live page validation, M10 footer reconciliation, or Collection Dashboard non-duplication check. Required for production readiness per plan.

5. **Tie-break rule not unit-tested**

   Plan §2.4 requires Top 20 ranking tie-break: customer name ascending (case-insensitive). Aggregator implements `ThenBy(c => c.CustomerName, StringComparer.OrdinalIgnoreCase)` but no test asserts equal-balance tie-break behavior.

### Minor

6. **Operational deploy doc stale**

   `src/j05-btr-distrib/docs/ops/btr-portal-deploy.md` still lists `BTRPD_PiutangTopCustomer` as the Piutang snapshot table set. V2 deprecates writes to that table and adds `BTRPD_PiutangCustomerAging` and `BTRPD_PiutangTopCustomerRisk`. Not in plan §4.1 doc list, but creates deployment confusion.

7. **`DashboardPiutangLiveDal` updated for V2 fields**

   Plan §3.1 states snapshot-only reads with no live aggregation fallback. `DashboardPiutangLiveDal` was extended (comment: legacy helper for comparison tests). Acceptable if unused in production DI, but increases maintenance surface without plan mention.

### Observations (no action required for approval)

- Frontend layout, copy, KPI grid, concentration row, aging chart, and Top 20 risk table match plan §7.
- SQL schema, idempotent upgrade script, and greenfield create script align with plan §5.
- `BTRPD_PiutangTopCustomer` writes correctly stopped in snapshot DAL.
- Executive and Alert Center composers correctly source rank-1 % from `TopCustomerRisk`.
- Worker logs skipped `CustomerId` count in progress detail per plan §2.5.
- Permanent knowledge artifacts (`btr-portal-domain.md`, `materialized-dashboard-domain.md`) reflect V2 semantics.

---

## Acceptance Criteria Evaluation

| Criterion (from plan) | Status | Notes |
| --------------------- | ------ | ----- |
| Five KPI tiles (all-time open balance) | **Not verified** | UI implemented; API/backend blocked by compile error |
| Concentration KPIs (Top 10/20 %) | **Not verified** | Aggregator logic present; backend not buildable |
| Aging chart unchanged (5 buckets, JatuhTempo) | **Pass** | `PiutangAgingBucketResolver` + existing pie component |
| Top 20 customer risk table with aging columns | **Pass** (FE) | `PiutangCustomerRiskTable.vue` matches column spec |
| Customer aging snapshot on Piutang worker (15 min) | **Pass** (code) | Worker + DAL write path present; DB integration unverified |
| Drill-down to M10 Piutang Report | **Pass** | Investigation metadata on risk rows |
| Reconciliation invariants (§2.2) | **Partial** | Unit tests cover most; full invariant #4 (Σ customer aging = Total Piutang) only when all rows have CustomerId |
| API: `TopCustomers` removed, `TopCustomerRisk` added | **Pass** (code) | Types and DAL mapping updated |
| Executive regression | **Not verified** | Tests exist but build fails |
| `npm run build` | **Pass** | Verified 2026-06-10 |
| Feature docs updated | **Pass** | Both foundation feature artifacts updated |

---

## Required Actions

1. **Resolve compile error** — Remove cross-layer type duplication. Aggregate layer (`DashboardPiutangAggregator`, `DashboardPiutangAggregateResult`) must use `DashboardSnapshotAgg.Models.DashboardPiutangTopCustomerRiskRow` only; remove `using DashboardPiutangAgg.Queries` from aggregate models/services. Map to API DTOs in `DashboardPiutangDal` (already partially done via type alias).

2. **Confirm backend builds and tests pass** — Run `dotnet build` on solution and `dotnet test` for `DashboardPiutangAggregatorTest`, `DashboardPiutangSnapshotVerificationTest`, `DashboardPiutangDalTest`, `DashboardExecutiveComposerTest`, `DashboardAlertCenterComposerTest`. Update `implementation-summary.md` with actual command output.

3. **Deliver plan §8.2 integration coverage** — Either extend `DashboardPiutangSnapshotVerificationTest` with snapshot DAL round-trip against test DB after worker refresh, or add an equivalent integration test that asserts persisted row counts and KPI reconciliation on `BTRPD_*` tables.

4. **Add tie-break unit test** — Two customers with equal `TotalPiutang`; assert lower name ranks first.

5. **Complete manual verification V1–V7** — Document results (or attach evidence) in implementation summary before re-review.

6. **(Recommended)** Update `docs/ops/btr-portal-deploy.md` Piutang table list for V2.

---

## Status

**REJECTED**

Implementation is substantially complete in scope and aligns with the approved architecture, but the backend **does not compile**. Automated test verification cannot be completed. Integration and manual acceptance criteria are not fully demonstrated. Correct compile error and re-submit for review.

---

*Reviewer Agent — independent quality gate per `docs/agents/reviewer-agent.md`*
