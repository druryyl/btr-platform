# Review Report

**Phase:** Phase 2 — Inventory Snapshot  
**Review Date:** 2026-06-07  
**Reviewer:** Reviewer Agent

**Authoritative plan:** [implementation-plan.md](./implementation-plan.md)  
**Implementation summary:** [implementation-summary-phase-2.md](./implementation-summary-phase-2.md)

## Reviewed Artifacts

- `docs/work/materialize-dashboard-data/implementation-plan.md` (Phase 2 scope)
- `docs/work/materialize-dashboard-data/implementation-summary-phase-2.md`
- SQL: `BTRPD_InventoryKpi`, `BTRPD_InventoryBreakdown`, `BTR_ParamNo_PortalDashboard.sql` (`PDB` prefix)
- Application: `DashboardInventoryAggregator`, `RefreshDashboardInventorySnapshotWorker`, contracts/models
- Infrastructure: `DashboardInventorySnapshotDal`, `DashboardInventoryDal` (facade), `DashboardInventoryLiveDal`
- Portal: `InfrastructurePortalExtensions.cs`, `ApplicationPortalExtensions.cs`
- Tests: `DashboardInventoryAggregatorTest`, `DashboardInventoryDalTest`, `DashboardInventorySnapshotVerificationTest`
- Baseline comparison: pre-Phase-2 `DashboardInventoryDal` at commit `8bc5494`

---

## Checklist

| Item | Result |
| --- | --- |
| Requirement implementation | **PASS** |
| Phase 2 scope complete | **PASS** |
| Architecture compliance | **PASS** |
| Acceptance criteria (Phase 2 scope) | **PASS** |
| Required artifacts present | **PASS** |
| Test coverage | **PASS** (with observation) |
| Unauthorized scope additions | **PASS** — none found |
| Production cutover readiness | **PENDING** — documented, not Phase 2 code scope |

---

## Phase 2 Plan Traceability

| Step | Requirement | Evidence | Status |
| --- | --- | --- | --- |
| 2.1 | SQL tables: Inventory KPI, Breakdown | `BTRPD_InventoryKpi.sql`, `BTRPD_InventoryBreakdown.sql`; registered in `btr.sql.sqlproj`; `PDB` ParamNo seed | **PASS** |
| 2.2 | `DashboardInventoryAggregator` (extract from existing DAL) | Logic matches pre-refactor `GetSummary` at `8bc5494`: BrgId-first, In-Transit exclusion, Qty > 0, Unknown dimensions, HPP×Qty, Top 10 | **PASS** |
| 2.3 | `RefreshDashboardInventorySnapshotWorker` | Follows plan template: refresh log → aggregate → transactional replace → success/fail; uses `IStokBalanceViewDal` | **PASS** |
| 2.4 | Shadow-run vs live `DashboardInventoryDal` | `DashboardInventorySnapshotVerificationTest` compares aggregator vs `DashboardInventoryLiveDal` on fixture rows; parity with committed baseline confirmed | **PASS** |
| 2.5 | Rewrite `DashboardInventoryDal` read path | Facade: snapshot → `AllowLiveFallback` → HTTP 503 | **PASS** |

**Explicitly out of Phase 2 (correctly deferred):** Sales snapshot, `btr.portal.worker` host, overview endpoint, frontend changes, disabling production fallback — aligned with plan §12.

---

## Findings

### Critical

None.

### Major

None blocking Phase 2 approval.

### Minor

1. **Duplicated `MapToResponse` logic** — `DashboardInventoryLiveDal` and `DashboardInventorySnapshotDal.MapToResponse` contain identical mapping (~45 lines each). Drift risk between snapshot and live read paths.

2. **`RefreshLog.CompletedAt` uses `DateTime.Now`** — inherited from Phase 1 (`DashboardSnapshotRefreshLogDal`); worker uses `ITglJamDal` for `GeneratedAt`. Minor inconsistency vs plan §11 criterion 6.

3. **No FK on `LastRefreshLogId`** — inherited from Phase 1; schema stores `VARCHAR(13)` without constraint.

4. **ParamNo seed is manual** — `PDB` prefix in `BTR_ParamNo_PortalDashboard.sql` is `<None Include>` (not post-deploy). Documented in summary; ops must run after deploy.

5. **`TransHelper.NewScope()` vs independent SQL connection** — `ReplaceCurrent` opens its own `SqlConnection` and may not enlist in the ambient transaction scope (same pattern as Piutang Phase 1). KPI and breakdown writes are still atomic within the DAL method.

### Observation

1. **Shadow test validates internal consistency** — `DashboardInventoryLiveDal` delegates to the same `DashboardInventoryAggregator`, so the shadow test confirms mapping parity rather than an independent legacy code path. This is an improvement over Phase 1 Piutang (where `LiveDal` duplicated logic). Baseline comparison against commit `8bc5494` confirms extraction fidelity.

2. **Full breakdown persisted, Top 10 returned on API** — Snapshot stores all category/supplier rows with `IsTop10` flags; API `CategoryBreakdown` / `SupplierBreakdown` still expose Top 10 only. Matches pre-refactor behavior (original DAL used `MapBreakdown(topCategories)` with `.Take(10)`).

3. **Test execution not independently verified** — `btr.test` is .NET Framework 4.8; tests could not be run in this review environment (MSBuild unavailable). Test artifacts are present and structurally sound; summary claim of 28/28 pass is **not independently confirmed**.

4. **No integration test** for worker → DB write → read path — Plan §13 mentions integration tests; Phase 2 steps do not require them. Unit + shadow tests cover logic; DB round-trip is untested in CI.

5. **Worker scheduling gap** — Correctly documented. `RefreshDashboardInventorySnapshotWorker` is Scrutor-registered (`INunaServiceVoid<>`), but without `btr.portal.worker` (Phase 4) there is no schedulable host. Safe with `AllowLiveFallback: true`.

6. **Test refactor preserves coverage** — Old `DashboardInventoryDalTest` (4 aggregation facts) split into aggregator tests (5 facts), facade tests (3 facts), and verification test (1 fact). No aggregation rules dropped.

---

## Architecture Compliance

| Design point | Compliance |
| --- | --- |
| `SnapshotKey = 'CURRENT'` upsert | **Yes** — MERGE on KPI, delete/insert breakdown |
| Aggregation extracted, shared on refresh and live fallback | **Yes** — worker and `DashboardInventoryLiveDal` use aggregator |
| Read path preserves `IDashboardInventoryDal` contract | **Yes** |
| `IStokBalanceViewDal` source; BrgId-first, In-Transit exclusion | **Yes** |
| HTTP 503 when snapshot missing and fallback disabled | **Yes** — `DashboardSnapshotUnavailableException` (Phase 1) |
| `GeneratedAt` from snapshot when populated | **Yes** — mapped from KPI table |
| Inventory Report path untouched | **Yes** — no changes to `InventoryReportDal` |
| No Sales / frontend / overview / worker host changes | **Yes** |
| Layer B breakdown with `IsTop10` flag (plan §4.4) | **Yes** |

---

## Acceptance Criteria (Phase 2 relevance)

| # | Criterion | Phase 2 status |
| --- | --- | --- |
| 4 | Inventory KPIs and breakdowns match live `DashboardInventoryDal` | **Code + shadow test + baseline diff** — production validation pending |
| 6 | `GeneratedAt` reflects refresh, not request time | **PASS** when snapshot populated |
| — | API contract unchanged | **PASS** |
| — | Inventory Report unaffected | **PASS** (by design) |

Criteria 1, 2, 8, and 10 are cross-phase or production-cutover items — correctly not claimed as Phase 2 deliverables.

---

## Required Actions

### Before Phase 3 may start

None — Phase 2 implementation scope is complete.

### Before production snapshot cutover (Phase 4 ops)

1. Deploy `btr.sql` schema (Inventory KPI + Breakdown tables).
2. Run `DataSeeds/BTR_ParamNo_PortalDashboard.sql` if `PDB` prefix is missing.
3. Invoke `RefreshDashboardInventorySnapshotWorker` at least once to populate `CURRENT` snapshot.
4. Validate Inventory dashboard totals against Inventory Report footer over 3–5 business days.
5. Deploy `btr.portal.worker` and schedule 60-minute Inventory refresh (with Piutang/Sales jobs).
6. Set `AllowLiveFallback: false` only after shadow validation.

### Recommended (non-blocking)

1. Extract shared `MapToResponse` for inventory snapshot and live paths to a single mapper (eliminate duplication between `DashboardInventoryLiveDal` and `DashboardInventorySnapshotDal`).
2. Use `ITglJamDal` in `DashboardSnapshotRefreshLogDal` for `CompletedAt` (Phase 1 carry-over).
3. Add an integration test for `ReplaceCurrent` → `GetCurrent` round-trip when a test database is available.
4. Consider applying the Phase 2 `LiveDal → aggregator` pattern to Piutang live fallback (Phase 1 review recommendation).

---

## Status

**APPROVED**

Phase 2 implementation matches the approved plan. All required artifacts exist, scope boundaries are respected, and the read/refresh architecture is correctly implemented. Inventory aggregation rules are faithfully extracted from the pre-refactor DAL. Unresolved items are operational cutover prerequisites and maintainability improvements — not Phase 2 code gaps.

**Stage-gate:** Phase 3 (Sales snapshot) may proceed.

---

*Approved by: Reviewer Agent*
