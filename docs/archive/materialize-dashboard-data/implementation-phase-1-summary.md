# Implementation Summary: Materialized Dashboard Data — Phase 1 (Piutang Snapshot)

## Status

Phase 1 is complete. The Piutang dashboard (`GET /api/dashboard/piutang`) now reads from materialized snapshot tables when a `CURRENT` snapshot exists, with configurable live fallback when no snapshot is present. Aggregation logic is extracted into a shared `DashboardPiutangAggregator` used by the refresh worker. All 19 Phase 1 unit and shadow tests pass. The worker is implemented but not yet schedulable — console host and Task Scheduler setup are deferred to Phase 4.

**Authoritative plan:** [implementation-plan.md](./implementation-plan.md)  
**Requirements:** [analysis-report.md](./analysis-report.md)

---

## Goal Delivered

Move Piutang dashboard aggregation off the HTTP request path by pre-computing KPIs, aging buckets, and Top 10 customers into dedicated SQL Server tables. The API contract (`DashboardPiutangResponse`, route, MediatR handler) is unchanged.

**Exit criterion met:** API serves from snapshot when populated; falls back to live aggregation when snapshot is empty and `DashboardSnapshot:AllowLiveFallback = true` (default).

---

## Architecture

```text
Refresh path (background):
  PiutangOpenBalanceDal (Sisa > 1)
    → DashboardPiutangAggregator
    → RefreshDashboardPiutangSnapshotWorker
    → DashboardPiutangSnapshotDal.ReplaceCurrent()
    → BTRPD_* tables

Read path (HTTP):
  GET /api/dashboard/piutang
    → DashboardPiutangDal (facade)
      → snapshot exists: DashboardPiutangSnapshotDal.GetCurrent()
      → snapshot missing + fallback: DashboardPiutangLiveDal (legacy live query)
      → snapshot missing + no fallback: HTTP 503
```

Snapshot pattern: single active row per domain via `SnapshotKey = 'CURRENT'`. Child rows (aging, Top 10) are delete-and-replace on each refresh within a transaction.

---

## Database Changes

### New tables (`btr.sql/Tables/ReportingContext/`)

| Table | Purpose |
| --- | --- |
| `BTRPD_RefreshLog` | Per-refresh audit (domain, status, duration, error) |
| `BTRPD_PiutangKpi` | Layer A headline KPIs (`TotalPiutang`, `TotalCustomer`, `OverdueCustomer`, `GeneratedAt`) |
| `BTRPD_PiutangAging` | Layer B aging bucket amounts (5 buckets) |
| `BTRPD_PiutangTopCustomer` | Layer B Top 10 customers by outstanding balance |

### Index

| Index | Purpose |
| --- | --- |
| `IX_BTR_Piutang_OpenBalance` | Filtered index on `BTR_Piutang (Sisa, PiutangId) WHERE Sisa > 1` — accelerates refresh source query |

### ParamNo seed

| Prefix | Usage |
| --- | --- |
| `PDR` | `RefreshLogId` |
| `PDA` | `PiutangAgingId` |
| `PDT` | `PiutangTopCustomerId` |

Seed script: `btr.sql/DataSeeds/BTR_ParamNo_PortalDashboard.sql` (run after deploy if prefixes missing).

All objects registered in `btr.sql.sqlproj`.

---

## Files Added

### Application (`btr.application/ReportingContext/DashboardSnapshotAgg/`)

| File | Purpose |
| --- | --- |
| `Contracts/IPiutangOpenBalanceDal.cs` | Refresh source read contract |
| `Contracts/IDashboardPiutangSnapshotDal.cs` | Snapshot read/write contract |
| `Contracts/IDashboardSnapshotRefreshLogDal.cs` | Refresh log contract |
| `Models/PiutangOpenBalanceDto.cs` | Slim DTO for open-balance rows |
| `Models/DashboardPiutangAggregateResult.cs` | Aggregator output / snapshot payload |
| `Models/DashboardSnapshotRefreshLogModel.cs` | Refresh log entity |
| `Services/DashboardPiutangAggregator.cs` | Shared aggregation logic (extracted from live DAL) |
| `UseCases/RefreshDashboardPiutangSnapshotRequest.cs` | Worker request/result DTOs |
| `UseCases/RefreshDashboardPiutangSnapshotWorker.cs` | Snapshot refresh orchestration |
| `DashboardSnapshotOptions.cs` | Config binding (`AllowLiveFallback`) |
| `DashboardSnapshotUnavailableException.cs` | Thrown when snapshot missing and fallback disabled |

### Infrastructure (`btr.infrastructure/ReportingContext/`)

| File | Purpose |
| --- | --- |
| `DashboardSnapshotAgg/PiutangOpenBalanceDal.cs` | Dapper query: `BTR_Piutang` + `BTR_Faktur` + `BTR_Customer`, `Sisa > 1` |
| `DashboardSnapshotAgg/DashboardPiutangSnapshotDal.cs` | Snapshot GET + transactional replace (MERGE KPI, delete/insert children) |
| `DashboardSnapshotAgg/DashboardSnapshotRefreshLogDal.cs` | Insert running / mark success / mark failed |
| `DashboardPiutangAgg/DashboardPiutangLiveDal.cs` | Preserved live aggregation (2000→today via `IPiutangSalesWilayahDal`) |

### Tests (`btr.test/ReportingContext/`)

| File | Purpose |
| --- | --- |
| `DashboardPiutangAggregatorTest.cs` | 15 unit tests — filter, aging buckets, customer key, Top 10, rollups |
| `DashboardPiutangDalTest.cs` | Read-path tests — snapshot priority, live fallback, 503 exception |
| `DashboardPiutangSnapshotVerificationTest.cs` | Shadow comparison: aggregator vs live DAL on equivalent fixture rows |

### SQL

| File | Purpose |
| --- | --- |
| `Tables/ReportingContext/BTRPD_RefreshLog.sql` | Refresh log table |
| `Tables/ReportingContext/BTRPD_PiutangKpi.sql` | Piutang KPI table |
| `Tables/ReportingContext/BTRPD_PiutangAging.sql` | Aging bucket table |
| `Tables/ReportingContext/BTRPD_PiutangTopCustomer.sql` | Top customer table |
| `Tables/ReportingContext/IX_BTR_Piutang_OpenBalance.sql` | Filtered index |
| `DataSeeds/BTR_ParamNo_PortalDashboard.sql` | ID prefix seed |

---

## Files Modified

| File | Change |
| --- | --- |
| `DashboardPiutangAgg/DashboardPiutangDal.cs` | Rewritten as facade: snapshot → fallback → 503 |
| `btr.sql.sqlproj` | Registered ReportingContext folder, tables, index; ParamNo seed |
| `btr.application.csproj` | Registered DashboardSnapshotAgg sources |
| `btr.infrastructure.csproj` | Registered snapshot DALs + DashboardPiutangLiveDal |
| `btr.test.csproj` | Registered three new test files |
| `Configurations/InfrastructurePortalExtensions.cs` | DI for snapshot DALs, open-balance DAL, live DAL |
| `Configurations/ApplicationPortalExtensions.cs` | `DashboardSnapshotOptions` config + `DashboardPiutangAggregator` registration |
| `appsettings.json` | Added `DashboardSnapshot.AllowLiveFallback: true` |
| `Filters/GlobalExceptionFilter.cs` | Maps `DashboardSnapshotUnavailableException` → HTTP 503 |

---

## Unchanged (by design)

| Area | Notes |
| --- | --- |
| `IDashboardPiutangDal` interface | Same `GetSummary()` signature |
| `GetDashboardPiutangQuery` / handler | Still delegates to DAL |
| `PiutangDashboardController` | Same route `GET /api/dashboard/piutang` |
| `DashboardPiutangResponse` DTO | Same shape; `GeneratedAt` now reflects snapshot refresh time when populated |
| `PiutangReportDal` | Unaffected; report footer reconciliation path preserved |
| Frontend | No changes in Phase 1 |

---

## Configuration

```json
"DashboardSnapshot": {
  "AllowLiveFallback": true
}
```

| Setting | Default | Behavior |
| --- | --- | --- |
| `AllowLiveFallback` | `true` | When no `CURRENT` snapshot exists, API uses live `DashboardPiutangLiveDal` (same as pre-Phase-1 behavior) |

Set to `false` after shadow validation and first successful refresh to enforce snapshot-only reads (returns 503 if snapshot missing).

---

## Aggregation Rules Preserved

Logic in `DashboardPiutangAggregator` matches the former live `DashboardPiutangDal`:

| Rule | Implementation |
| --- | --- |
| Outstanding filter | `KurangBayar > 1` (open balance from `Sisa` on refresh path) |
| Customer key | `CustomerCode` trimmed; fallback to `CustomerName` |
| Aging buckets | 5 buckets from `JatuhTempo` vs refresh `today` |
| Overdue customers | Customers with any non-`Current` bucket exposure |
| Top 10 | By outstanding balance desc, name asc tie-break |
| `GeneratedAt` | Set at refresh time (stored in KPI table; not request time when reading snapshot) |

Refresh source uses `PiutangOpenBalanceDal` (`Sisa > 1`) instead of the 2000→today `PiutangSalesWilayahDal` scan. Shadow test confirms aggregator output matches live DAL when input rows are equivalent.

---

## Test Results

| Suite | Tests | Result |
| --- | --- | --- |
| `DashboardPiutangAggregatorTest` | 15 | Pass |
| `DashboardPiutangDalTest` | 3 | Pass |
| `DashboardPiutangSnapshotVerificationTest` | 1 | Pass |
| **Total Phase 1** | **19** | **Pass** |

Existing `PiutangReportDalTest` and `DashboardInventoryDalTest` unaffected.

---

## Deployment Steps (Operations)

1. Deploy `btr.sql` schema (tables + `IX_BTR_Piutang_OpenBalance`).
2. Run `DataSeeds/BTR_ParamNo_PortalDashboard.sql` if `PDR`/`PDA`/`PDT` prefixes are missing.
3. Deploy portal API with Phase 1 code (`AllowLiveFallback: true` initially).
4. Run first snapshot refresh (see **Known gap** below).
5. Validate Piutang dashboard totals against Piutang Report footer over 3–5 business days.
6. Set `AllowLiveFallback: false` when satisfied.

---

## Known Gap — Worker Scheduling (Phase 4)

`RefreshDashboardPiutangSnapshotWorker` is implemented and auto-registered via Scrutor (`INunaServiceVoid<>`), but **`btr.portal.worker` console host does not exist yet**. Until Phase 4:

- Worker can be invoked programmatically (tests, one-off ops script, or temporary harness).
- Scheduled 15-minute Piutang refresh requires Phase 4 Task Scheduler setup.
- With empty snapshot tables and `AllowLiveFallback: true`, production behavior remains identical to pre-Phase-1 live aggregation.

---

## Deferred to Later Phases

| Item | Phase |
| --- | --- |
| Inventory snapshot tables + worker | Phase 2 |
| Sales snapshot (Faktur source) | Phase 3 |
| `btr.portal.worker` console host | Phase 4 |
| `GET /api/dashboard/overview` | Phase 4 |
| Frontend `GeneratedAt` / label updates | Phase 3–4 |
| Disable live fallback in production | Phase 4 |
| Manual refresh admin endpoint | Phase 5 |
| `HealthController` refresh status | Phase 5 |

---

## Verification Checklist (Phase 1)

| # | Criterion | Status |
| --- | --- | --- |
| 1 | SQL tables + index deploy from `btr.sql` | Ready — deploy pending ops |
| 2 | Aggregator unit tests pass | Done (15/15) |
| 3 | Shadow test: aggregator matches live DAL | Done |
| 4 | Read path: snapshot when populated | Done (code + unit test) |
| 5 | Read path: live fallback when snapshot empty | Done (default config) |
| 6 | API contract unchanged | Done |
| 7 | Piutang Report unaffected | Done (existing tests pass) |
| 8 | First production snapshot populated | Pending — requires worker invocation |
| 9 | Production fallback disabled | Pending — Phase 4 cutover |

---

## Next Steps

1. **Phase 2:** Inventory snapshot (tables, aggregator, worker, read-path swap).
2. **Phase 4 (before production cutover):** Deploy worker host; schedule Piutang refresh every 15 minutes; run shadow period; disable `AllowLiveFallback`.
3. **Ops:** After schema deploy, execute one refresh to populate `BTRPD_Piutang*` before disabling fallback.
