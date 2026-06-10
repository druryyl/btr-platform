# Implementation Summary: Materialized Dashboard Data — Phase 2 (Inventory Snapshot)

## Status

Phase 2 is complete. The Inventory dashboard (`GET /api/dashboard/inventory`) now reads from materialized snapshot tables when a `CURRENT` snapshot exists, with configurable live fallback when no snapshot is present. Aggregation logic is extracted into a shared `DashboardInventoryAggregator` used by the refresh worker. All 28 Phase 1 + Phase 2 unit and shadow tests pass. The worker is implemented but not yet schedulable — console host and Task Scheduler setup remain deferred to Phase 4.

**Authoritative plan:** [implementation-plan.md](./implementation-plan.md)  
**Requirements:** [analysis-report.md](./analysis-report.md)  
**Prior phase:** [implementation-summary-phase-1.md](./implementation-summary-phase-1.md)

---

## Goal Delivered

Move Inventory dashboard aggregation off the HTTP request path by pre-computing KPIs and dimensional breakdowns into dedicated SQL Server tables. The API contract (`DashboardInventoryResponse`, route, MediatR handler) is unchanged.

**Exit criterion met:** API serves from snapshot when populated; falls back to live aggregation when snapshot is empty and `DashboardSnapshot:AllowLiveFallback = true` (default).

---

## Architecture

```text
Refresh path (background):
  IStokBalanceViewDal.ListData()
    → DashboardInventoryAggregator
    → RefreshDashboardInventorySnapshotWorker
    → DashboardInventorySnapshotDal.ReplaceCurrent()
    → BTRPD_Inventory* tables

Read path (HTTP):
  GET /api/dashboard/inventory
    → DashboardInventoryDal (facade)
      → snapshot exists: DashboardInventorySnapshotDal.GetCurrent()
      → snapshot missing + fallback: DashboardInventoryLiveDal (via aggregator)
      → snapshot missing + no fallback: HTTP 503
```

Snapshot pattern: single active row per domain via `SnapshotKey = 'CURRENT'`. Full category and supplier breakdown rows are stored with `IsTop10` / `Top10Rank` flags; child rows are delete-and-replace on each refresh within a transaction.

---

## Database Changes

### New tables (`btr.sql/Tables/ReportingContext/`)

| Table | Purpose |
| --- | --- |
| `BTRPD_InventoryKpi` | Layer A headline KPIs (`TotalInventoryValue`, `TotalItem`, `GeneratedAt`) |
| `BTRPD_InventoryBreakdown` | Layer B full category/supplier breakdown with Top 10 flags |

### ParamNo seed

| Prefix | Usage |
| --- | --- |
| `PDB` | `InventoryBreakdownId` |

Added to `btr.sql/DataSeeds/BTR_ParamNo_PortalDashboard.sql`. Reuses existing `PDR` prefix for refresh log IDs.

All objects registered in `btr.sql.sqlproj`.

---

## Files Added

### Application (`btr.application/ReportingContext/DashboardSnapshotAgg/`)

| File | Purpose |
| --- | --- |
| `Contracts/IDashboardInventorySnapshotDal.cs` | Snapshot read/write contract |
| `Models/DashboardInventoryAggregateResult.cs` | Aggregator output / snapshot payload |
| `Services/DashboardInventoryAggregator.cs` | Shared aggregation logic (extracted from live DAL) |
| `UseCases/RefreshDashboardInventorySnapshotRequest.cs` | Worker request/result DTOs |
| `UseCases/RefreshDashboardInventorySnapshotWorker.cs` | Snapshot refresh orchestration |

### Infrastructure (`btr.infrastructure/ReportingContext/`)

| File | Purpose |
| --- | --- |
| `DashboardSnapshotAgg/DashboardInventorySnapshotDal.cs` | Snapshot GET + transactional replace (MERGE KPI, delete/insert breakdown) |
| `DashboardInventoryAgg/DashboardInventoryLiveDal.cs` | Preserved live aggregation via shared aggregator |

### Tests (`btr.test/ReportingContext/`)

| File | Purpose |
| --- | --- |
| `DashboardInventoryAggregatorTest.cs` | 5 unit tests — In-Transit exclusion, Unknown dimensions, rollups, Top 10 flags |
| `DashboardInventoryDalTest.cs` | Read-path tests — snapshot priority, live fallback, 503 exception |
| `DashboardInventorySnapshotVerificationTest.cs` | Shadow comparison: aggregator vs live DAL on equivalent fixture rows |

### SQL

| File | Purpose |
| --- | --- |
| `Tables/ReportingContext/BTRPD_InventoryKpi.sql` | Inventory KPI table |
| `Tables/ReportingContext/BTRPD_InventoryBreakdown.sql` | Breakdown table + index |

---

## Files Modified

| File | Change |
| --- | --- |
| `DashboardInventoryAgg/DashboardInventoryDal.cs` | Rewritten as facade: snapshot → fallback → 503 |
| `btr.sql.sqlproj` | Registered Inventory snapshot tables |
| `btr.sql/DataSeeds/BTR_ParamNo_PortalDashboard.sql` | Added `PDB` prefix |
| `btr.application.csproj` | Registered DashboardSnapshotAgg Inventory sources |
| `btr.infrastructure.csproj` | Registered snapshot DAL + DashboardInventoryLiveDal |
| `btr.test.csproj` | Registered three new test files; replaced old live-only DalTest |
| `Configurations/InfrastructurePortalExtensions.cs` | DI for inventory snapshot DAL + live DAL |
| `Configurations/ApplicationPortalExtensions.cs` | `DashboardInventoryAggregator` registration |

---

## Unchanged (by design)

| Area | Notes |
| --- | --- |
| `IDashboardInventoryDal` interface | Same `GetSummary()` signature |
| `GetDashboardInventoryQuery` / handler | Still delegates to DAL |
| `InventoryDashboardController` | Same route `GET /api/dashboard/inventory` |
| `DashboardInventoryResponse` DTO | Same shape; `GeneratedAt` reflects snapshot refresh time when populated |
| `InventoryReportDal` | Unaffected; report footer reconciliation path preserved |
| Frontend | No changes in Phase 2 |

---

## Aggregation Rules Preserved

Logic in `DashboardInventoryAggregator` matches the former live `DashboardInventoryDal`:

| Rule | Implementation |
| --- | --- |
| BrgId-first grouping | Group `StokBalanceView` rows by `BrgId`, sum qty across warehouses |
| In-Transit exclusion | Filter `WarehouseName != "In-Transit"` before grouping |
| Zero-qty exclusion | Only items with summed `Qty > 0` |
| Unknown dimensions | Blank category/supplier → `"Unknown"` |
| Inventory value | `Sum(Hpp * Qty)` per item group |
| Top 10 | By dimension value desc, name asc tie-break |
| CategoryBreakdown / SupplierBreakdown | Top 10 only (same API contract as before) |
| Full breakdown storage | All dimension rows persisted with `IsTop10` flag for future chart use |
| `GeneratedAt` | Set at refresh time (stored in KPI table; not request time when reading snapshot) |

---

## Test Results

| Suite | Tests | Result |
| --- | --- | --- |
| `DashboardInventoryAggregatorTest` | 5 | Pass |
| `DashboardInventoryDalTest` | 3 | Pass |
| `DashboardInventorySnapshotVerificationTest` | 1 | Pass |
| `DashboardPiutangAggregatorTest` | 15 | Pass (regression) |
| `DashboardPiutangDalTest` | 3 | Pass (regression) |
| `DashboardPiutangSnapshotVerificationTest` | 1 | Pass (regression) |
| **Total Phase 1 + 2** | **28** | **Pass** |

---

## Deployment Steps (Operations)

1. Deploy `btr.sql` schema (Inventory KPI + Breakdown tables).
2. Run `DataSeeds/BTR_ParamNo_PortalDashboard.sql` if `PDB` prefix is missing.
3. Deploy portal API with Phase 2 code (`AllowLiveFallback: true` initially).
4. Run first Inventory snapshot refresh (see **Known gap** below).
5. Validate Inventory dashboard totals against Inventory Report footer over 3–5 business days.
6. Set `AllowLiveFallback: false` when satisfied (Phase 4 cutover).

---

## Known Gap — Worker Scheduling (Phase 4)

`RefreshDashboardInventorySnapshotWorker` is implemented and auto-registered via Scrutor (`INunaServiceVoid<>`), but **`btr.portal.worker` console host does not exist yet**. Until Phase 4:

- Worker can be invoked programmatically (tests, one-off ops script, or temporary harness).
- Scheduled 60-minute Inventory refresh requires Phase 4 Task Scheduler setup.
- With empty snapshot tables and `AllowLiveFallback: true`, production behavior remains identical to pre-Phase-2 live aggregation.

---

## Deferred to Later Phases

| Item | Phase |
| --- | --- |
| Sales snapshot (Faktur source) | Phase 3 |
| `btr.portal.worker` console host | Phase 4 |
| `GET /api/dashboard/overview` | Phase 4 |
| Frontend `GeneratedAt` / label updates | Phase 3–4 |
| Disable live fallback in production | Phase 4 |
| Manual refresh admin endpoint | Phase 5 |

---

## Verification Checklist (Phase 2)

| # | Criterion | Status |
| --- | --- | --- |
| 1 | SQL tables deploy from `btr.sql` | Ready — deploy pending ops |
| 2 | Aggregator unit tests pass | Done (5/5) |
| 3 | Shadow test: aggregator matches live DAL | Done |
| 4 | Read path: snapshot when populated | Done (code + unit test) |
| 5 | Read path: live fallback when snapshot empty | Done (default config) |
| 6 | API contract unchanged | Done |
| 7 | Inventory Report unaffected | Done (existing tests pass) |
| 8 | First production snapshot populated | Pending — requires worker invocation |
| 9 | Production fallback disabled | Pending — Phase 4 cutover |

---

## Next Steps

1. **Phase 3:** Sales snapshot (Faktur source, tables, aggregator, worker, read-path swap, frontend labels).
2. **Phase 4 (before production cutover):** Deploy worker host; schedule per-domain refresh (15/30/60 min); run shadow period; disable `AllowLiveFallback`.
3. **Ops:** After schema deploy, execute Inventory refresh to populate `BTRPD_Inventory*` before disabling fallback.
