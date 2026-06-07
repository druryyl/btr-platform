# Review Report

**Phase:** Phase 1 — Piutang Snapshot  
**Review Date:** 2026-06-07  
**Reviewer:** Reviewer Agent

**Authoritative plan:** [implementation-plan.md](./implementation-plan.md)  
**Implementation summary:** [implementation-phase-1-summary.md](./implementation-phase-1-summary.md)

## Reviewed Artifacts

- `docs/work/materialize-dashboard-data/implementation-plan.md` (Phase 1 scope)
- `docs/work/materialize-dashboard-data/implementation-phase-1-summary.md`
- SQL: `BTR_PortalDashboardRefreshLog`, `BTR_PortalDashboardPiutangKpi`, `BTR_PortalDashboardPiutangAging`, `BTR_PortalDashboardPiutangTopCustomer`, `IX_BTR_Piutang_OpenBalance`, `BTR_ParamNo_PortalDashboard.sql`
- Application: `DashboardSnapshotAgg/` (aggregator, worker, contracts, models, options)
- Infrastructure: `PiutangOpenBalanceDal`, `DashboardPiutangSnapshotDal`, `DashboardSnapshotRefreshLogDal`, `DashboardPiutangDal` (facade), `DashboardPiutangLiveDal`
- Portal: `InfrastructurePortalExtensions.cs`, `ApplicationPortalExtensions.cs`, `appsettings.json`, `GlobalExceptionFilter.cs`
- Tests: `DashboardPiutangAggregatorTest`, `DashboardPiutangDalTest`, `DashboardPiutangSnapshotVerificationTest`

---

## Checklist

| Item | Result |
| --- | --- |
| Requirement implementation | **PASS** |
| Phase 1 scope complete | **PASS** |
| Architecture compliance | **PASS** |
| Acceptance criteria (Phase 1 scope) | **PASS** |
| Required artifacts present | **PASS** |
| Test coverage | **PASS** (with observation) |
| Unauthorized scope additions | **PASS** — none found |
| Production cutover readiness | **PENDING** — documented, not Phase 1 code scope |

---

## Phase 1 Plan Traceability

| Step | Requirement | Evidence | Status |
| --- | --- | --- | --- |
| 1.1 | SQL tables: RefreshLog, Piutang KPI/Aging/TopCustomer | 4 table scripts + index on refresh log; registered in `btr.sql.sqlproj` | **PASS** |
| 1.2 | `IX_BTR_Piutang_OpenBalance` filtered index | `IX_BTR_Piutang_OpenBalance.sql` matches plan (`Sisa > 1`, INCLUDE columns) | **PASS** |
| 1.3 | `PiutangOpenBalanceDal`, `DashboardPiutangAggregator` | SQL matches plan §5.5; aggregator preserves bucket/customer/Top 10 rules | **PASS** |
| 1.4 | Worker + snapshot writer/reader DALs | `RefreshDashboardPiutangSnapshotWorker` follows plan template (log → aggregate → replace → mark success/fail) | **PASS** |
| 1.5 | Aggregator unit tests | 15 test cases (7 `[Fact]` + 8 `[Theory]` inline cases) | **PASS** |
| 1.6 | Shadow-run vs live DAL | `DashboardPiutangSnapshotVerificationTest` compares aggregator vs `DashboardPiutangLiveDal` | **PASS** (caveat below) |
| 1.7 | Rewrite read path + fallback flag | `DashboardPiutangDal` facade: snapshot → fallback → 503; `AllowLiveFallback` in config | **PASS** |

**Explicitly out of Phase 1 (correctly deferred):** `btr.portal.worker` console host, overview endpoint, Inventory/Sales snapshots, frontend changes, disabling production fallback — all aligned with plan §12.

---

## Findings

### Critical

None.

### Major

None blocking Phase 1 approval.

### Minor

1. **Duplicated aggregation logic** — `DashboardPiutangLiveDal` (~130 lines) mirrors `DashboardPiutangAggregator` instead of mapping live rows into the aggregator. Acceptable for Phase 1 fallback, but increases drift risk while `AllowLiveFallback = true`.

2. **`RefreshLog.CompletedAt` uses `DateTime.Now`** — `DashboardSnapshotRefreshLogDal.MarkSuccess` / `MarkFailed` use wall-clock time while the worker uses `ITglJamDal` for `GeneratedAt`. Minor inconsistency vs plan §11 criterion 6 (`GeneratedAt` vs `RefreshLog` alignment).

3. **No FK on `LastRefreshLogId`** — Plan §4.3 references FK to refresh log; schema stores `VARCHAR(13)` without constraint. Functional; weaker referential integrity.

4. **ParamNo seed is manual** — `BTR_ParamNo_PortalDashboard.sql` is `<None Include>` (not post-deploy). Documented in summary; ops must run after deploy.

### Observation

1. **Shadow test uses equivalent fixture rows only** — It validates logic parity when `Sisa`-mapped inputs match live `KurangBayar` rows. It does **not** exercise real-world `Sisa` vs recomputed `KurangBayar` variance (plan §10 medium risk). Production shadow over 3–5 business days remains required before disabling fallback.

2. **No integration test** for worker → DB write → read path — Plan §13 mentions integration tests; Phase 1 steps do not require them. Unit + shadow tests cover logic; DB round-trip is untested in CI.

3. **Test execution not independently verified** — `btr.test` is .NET Framework 4.8; tests could not be run in this review environment (no MSBuild). Test artifacts are present and structurally sound; summary claim of 19/19 pass is **not independently confirmed**.

4. **Worker scheduling gap** — Correctly documented. `RefreshDashboardPiutangSnapshotWorker` is implemented and Scrutor-registered, but without `btr.portal.worker` (Phase 4) there is no schedulable host. Safe with `AllowLiveFallback: true`.

---

## Architecture Compliance

| Design point | Compliance |
| --- | --- |
| `SnapshotKey = 'CURRENT'` upsert | **Yes** — MERGE on KPI, delete/insert children |
| Aggregation extracted, not duplicated on refresh path | **Yes** — worker uses aggregator |
| Read path preserves `IDashboardPiutangDal` contract | **Yes** |
| `PiutangOpenBalanceDal` uses `Sisa > 1`, not `PiutangSalesWilayahDal` on refresh | **Yes** |
| HTTP 503 when snapshot missing and fallback disabled | **Yes** — `DashboardSnapshotUnavailableException` → `GlobalExceptionFilter` |
| `GeneratedAt` from snapshot when populated | **Yes** — mapped from KPI table |
| Piutang Report path untouched | **Yes** — no changes to report DALs |
| No `btr.distrib` / frontend / overview changes | **Yes** |

---

## Acceptance Criteria (Phase 1 relevance)

| # | Criterion | Phase 1 status |
| --- | --- | --- |
| 3 | Piutang KPIs/buckets/Top 10 match live DAL | **Code + shadow test** — production validation pending |
| 6 | `GeneratedAt` reflects refresh, not request time | **PASS** when snapshot populated |
| — | API contract unchanged | **PASS** |
| — | Piutang Report unaffected | **PASS** (by design) |

Criteria 1, 2, 8, and 10 are cross-phase or production-cutover items — correctly not claimed as Phase 1 deliverables.

---

## Required Actions

### Before Phase 2 may start

None — Phase 1 implementation scope is complete.

### Before production snapshot cutover (Phase 4 ops)

1. Deploy `btr.sql` schema, index, and ParamNo seeds.
2. Invoke `RefreshDashboardPiutangSnapshotWorker` at least once to populate `CURRENT` snapshot.
3. Run production shadow comparison (aggregator totals vs Piutang Report footer; investigate `Sisa` vs `KurangBayar` tolerance).
4. Deploy `btr.portal.worker` and schedule 15-minute Piutang refresh.
5. Set `AllowLiveFallback: false` only after shadow validation.

### Recommended (non-blocking)

1. Refactor `DashboardPiutangLiveDal` to delegate to `DashboardPiutangAggregator` with a row mapper to remove duplication.
2. Use `ITglJamDal` in `DashboardSnapshotRefreshLogDal` for `CompletedAt`.
3. Add an integration test for `ReplaceCurrent` → `GetCurrent` round-trip when a test database is available.

---

## Status

**APPROVED**

Phase 1 implementation matches the approved plan. All required artifacts exist, scope boundaries are respected, and the read/refresh architecture is correctly implemented. Unresolved items are operational cutover prerequisites and maintainability improvements — not Phase 1 code gaps.

**Stage-gate:** Phase 2 (Inventory snapshot) may proceed.

---

*Approved by: Reviewer Agent*
