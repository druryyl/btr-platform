# Implementation Plan: Materialized Dashboard Data

## Document Status

| Field | Value |
| --- | --- |
| Initiative | Materialized Dashboard Data |
| Authoritative requirements | `analysis-report.md` (same folder) |
| Related features | `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-architecture.md` |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |
| Status | Ready for implementation |
| Product decisions | Confirmed — see Section 14 |

---

## 1. Goal

Move CPU-heavy dashboard aggregation off the HTTP request path for the three **analytical** dashboard routes (`/dashboard/sales`, `/dashboard/piutang`, `/dashboard/inventory`) by introducing a **background snapshot refresh** that pre-computes KPIs and dimensional aggregates into dedicated SQL Server tables. Portal API endpoints keep the same routes and response DTOs; only the backing data source changes.

**Primary outcomes:**

- Analytical dashboard APIs respond from materialized storage (target p95 &lt; 500 ms under normal load).
- `GeneratedAt` on every dashboard response reflects the last successful background refresh, not request execution time.
- Piutang and Inventory materialized output matches today's live `Dashboard*Dal` semantics.
- Sales dashboard switches from `BTR_SalesOmzet` to **`BTR_Faktur`** semantics — `SUM(GrandTotal)` for non-void Fakturs in the current month (confirmed).
- Overview Dashboard (`/dashboard`) reads **Layer A KPI snapshots only** for fast home-page load (confirmed — see Section 8).
- `BTR_SalesOmzet` and RO2 workflows remain untouched.

**Explicitly out of scope:**

- Report pages (`/reports/*`) — continue live queries.
- Custom date filters, drilldown, export.
- Changes to `BTR_SalesOmzet` materialization.

---

## 2. Architecture Overview

### 2.1 Target topology

```text
Windows Task Scheduler (or manual CLI)
    ↓
btr.portal.worker (new console host, net48)
    ↓
RefreshAllDashboardSnapshotsWorker
    ├── RefreshDashboardPiutangSnapshotWorker
    ├── RefreshDashboardInventorySnapshotWorker
    └── RefreshDashboardSalesSnapshotWorker
    ↓
Dashboard*Aggregator (shared aggregation logic)
    ↓
Dashboard*SnapshotWriter → BTRPD_* tables

Browser → GET /api/dashboard/{sales|piutang|inventory}   (detail pages — Layer A + B)
Browser → GET /api/dashboard/overview                     (home — Layer A only)
    ↓ MediatR
GetDashboard*Handler / GetDashboardOverviewHandler
    ↓ IDashboard*Dal / IDashboardOverviewDal
Dashboard*Dal (read path — rewritten)
    ↓ IDashboard*SnapshotDal
Snapshot tables (fast SELECT)
```

### 2.2 Design principles

| Principle | Application |
| --- | --- |
| Preserve API contract | Same endpoints, same `Dashboard{Domain}Response` DTOs; additive changes only |
| Extract, don't duplicate | Aggregation rules move from `Dashboard*Dal` into `Dashboard*Aggregator` services used by both refresh worker and shadow verification |
| Follow existing materialization patterns | Transactional delete-and-replace per domain (like `ReconcileSalesOmzetWorker`); operational metadata table (like `BTR_SalesOmzetHealthWeekly`) |
| No `btr.distrib` reference | Worker is a separate console host; portal API does not host long-running timers |
| Same database | Snapshot tables live in the operational SQL Server database |

### 2.3 Worker placement decision

**Recommended: `btr.portal.worker` console application**

| Option | Verdict | Rationale |
| --- | --- | --- |
| Portal API internal timer | Rejected | IIS app-pool recycle kills timers; Web API 2 has no `IHostedService` |
| BTR Desktop (`btr.distrib`) | Rejected | Portal architecture forbids API → distrib reference; desktop should not own portal refresh |
| Hangfire / new service framework | Rejected | No existing dependency; unnecessary complexity |
| **`btr.portal.worker` + Task Scheduler** | **Selected** | Mirrors `btr.sync` timer-host pattern; shares `appsettings.json` connection config with portal; isolated failure domain |

**Manual refresh (deferred):** On-demand snapshot rebuild is required eventually but **not in Phase 1**. Defer portal admin endpoint and Desktop trigger to a later phase; worker CLI (`--triggered-by Manual`) may be added in Phase 4 for operations use only.

---

## 3. Impact Analysis

### 3.1 Affected modules

| Layer | Module | Change type |
| --- | --- | --- |
| SQL | `btr.sql` | New tables + index on `BTR_Piutang` |
| Application | `ReportingContext/DashboardSnapshotAgg/` (new) | Workers, aggregators, snapshot contracts |
| Application | `ReportingContext/DashboardPiutangAgg/` | Refactor read DAL; optional live DAL rename |
| Application | `ReportingContext/DashboardInventoryAgg/` | Refactor read DAL |
| Application | `ReportingContext/DashboardSalesAgg/` | Rewrite aggregation source to Faktur |
| Infrastructure | `ReportingContext/DashboardSnapshotAgg/` (new) | Snapshot DALs (Dapper writers/readers) |
| Infrastructure | `FinanceContext/` (new DAL) | Efficient open-piutang query (`Sisa > 1`) |
| Host | `btr.portal.worker/` (new project) | CLI entry point |
| API | `btr.portal.api` | DI registration; new `GET /api/dashboard/overview` endpoint |
| Frontend | `btr.portal.web` | Overview home uses overview endpoint; Sales label/copy updates; emphasize `GeneratedAt` |
| Docs | `btr-portal-domain.md` | Update Sales KPI definitions post-cutover |

### 3.2 Unaffected modules

- All `*ReportDal` implementations and report controllers.
- `ReconcileSalesOmzetWorker`, `BTR_SalesOmzet`, RO2 Desktop forms.
- `GenStokBalanceWorker`, `BTR_StokBalanceWarehouse` (source data only).
- Authentication, routing, MediatR handlers (signatures unchanged).

### 3.3 Business rule preservation

| Domain | Rules preserved | Optimization allowed |
| --- | --- | --- |
| Piutang | `KurangBayar > 1`; aging buckets; customer key; Top 10 | Replace `2000-01-01` date scan with `Sisa > 1` filter; use persisted `Sisa` instead of payment subqueries |
| Inventory | BrgId-first; In-Transit exclusion; Qty &gt; 0; Unknown dimensions | None — same inputs from `IStokBalanceViewDal` |
| Sales | Current month; void exclusion; Top 10; weekly trend; target from `BTR_SalesOmzetTarget` | **Source change:** `BTR_Faktur.GrandTotal` replaces omzet recognition |

---

## 4. Database Design

### 4.1 Table naming convention

Prefix: `BTRPD_` — clearly portal-owned, read-only analytics artifacts.

### 4.2 Operational metadata — `BTRPD_RefreshLog`

One row per refresh attempt (all domains or single domain).

| Column | Type | Purpose |
| --- | --- | --- |
| `RefreshLogId` | `VARCHAR(13)` PK | Generated ID |
| `Domain` | `VARCHAR(20)` | `All`, `Sales`, `Piutang`, `Inventory` |
| `StartedAt` | `DATETIME` | Refresh start |
| `CompletedAt` | `DATETIME` | Refresh end (null if failed) |
| `Status` | `VARCHAR(10)` | `Running`, `Success`, `Failed` |
| `DurationMs` | `INT` | Elapsed milliseconds |
| `ErrorMessage` | `VARCHAR(500)` | Truncated error on failure |
| `TriggeredBy` | `VARCHAR(20)` | `Scheduler`, `Manual` |

Index: `IX_BTRPD_RefreshLog_Domain_CompletedAt` on `(Domain, CompletedAt DESC)`.

### 4.3 Layer A — headline KPI tables (one active row per domain)

Use **`SnapshotKey = 'CURRENT'`** upsert pattern — only the latest snapshot is retained per domain. **No historical snapshot retention** (confirmed — do not store prior refresh versions).

#### `BTRPD_PiutangKpi`

| Column | Type |
| --- | --- |
| `SnapshotKey` | `VARCHAR(10)` PK — always `'CURRENT'` |
| `GeneratedAt` | `DATETIME` |
| `TotalPiutang` | `DECIMAL(18,2)` |
| `TotalCustomer` | `INT` |
| `OverdueCustomer` | `INT` |
| `LastRefreshLogId` | `VARCHAR(13)` FK |

#### `BTRPD_InventoryKpi`

| Column | Type |
| --- | --- |
| `SnapshotKey` | `VARCHAR(10)` PK |
| `GeneratedAt` | `DATETIME` |
| `TotalInventoryValue` | `DECIMAL(18,2)` |
| `TotalItem` | `INT` |
| `LastRefreshLogId` | `VARCHAR(13)` |

#### `BTRPD_SalesKpi`

| Column | Type |
| --- | --- |
| `SnapshotKey` | `VARCHAR(10)` PK |
| `GeneratedAt` | `DATETIME` |
| `PeriodYear` | `INT` |
| `PeriodMonth` | `INT` |
| `TotalOmzet` | `DECIMAL(18,2)` |
| `TotalFaktur` | `INT` |
| `TotalCustomer` | `INT` |
| `TotalTarget` | `DECIMAL(18,2)` |
| `TotalAchievement` | `DECIMAL(18,2)` |
| `AchievementPercent` | `DECIMAL(9,4)` nullable |
| `CompletedOmzet` | `DECIMAL(18,2)` |
| `PipelineOmzet` | `DECIMAL(18,2)` — always `0` after Faktur cutover |
| `LastRefreshLogId` | `VARCHAR(13)` |

`PeriodYear` / `PeriodMonth` allow detecting stale snapshots when the calendar month rolls over.

### 4.4 Layer B — dimensional aggregate tables

Child rows reference `SnapshotKey = 'CURRENT'` via domain FK. On refresh: **delete all child rows for domain, then insert** within the same transaction as KPI upsert.

#### `BTRPD_PiutangAging`

| Column | Type |
| --- | --- |
| `PiutangAgingId` | `VARCHAR(13)` PK |
| `SnapshotKey` | `VARCHAR(10)` — `'CURRENT'` |
| `BucketKey` | `VARCHAR(20)` |
| `BucketLabel` | `VARCHAR(50)` |
| `SortOrder` | `INT` |
| `Amount` | `DECIMAL(18,2)` |

Unique: `(SnapshotKey, BucketKey)`.

#### `BTRPD_PiutangTopCustomer`

| Column | Type |
| --- | --- |
| `PiutangTopCustomerId` | `VARCHAR(13)` PK |
| `SnapshotKey` | `VARCHAR(10)` |
| `Rank` | `INT` |
| `CustomerName` | `VARCHAR(50)` |
| `OutstandingBalance` | `DECIMAL(18,2)` |

Unique: `(SnapshotKey, Rank)`.

#### `BTRPD_InventoryBreakdown`

| Column | Type |
| --- | --- |
| `InventoryBreakdownId` | `VARCHAR(13)` PK |
| `SnapshotKey` | `VARCHAR(10)` |
| `DimensionType` | `VARCHAR(10)` — `Category` or `Supplier` |
| `Name` | `VARCHAR(50)` |
| `InventoryValue` | `DECIMAL(18,2)` |
| `IsTop10` | `BIT` — `1` if row is in Top 10 table |
| `Top10Rank` | `INT` nullable |

Store **full** breakdown for chart bars; flag Top 10 rows for table component. Alternative: store only Top 10 for tables and full breakdown separately — implementer may choose one table with `IsTop10` flag (recommended, fewer tables).

#### `BTRPD_SalesWeekTrend`

| Column | Type |
| --- | --- |
| `SalesWeekTrendId` | `VARCHAR(13)` PK |
| `SnapshotKey` | `VARCHAR(10)` |
| `WeekStart` | `DATETIME` |
| `WeekEnd` | `DATETIME` |
| `WeekLabel` | `VARCHAR(30)` |
| `RecognizedAmount` | `DECIMAL(18,2)` — Faktur `GrandTotal` sum |

#### `BTRPD_SalesTopSalesman`

| Column | Type |
| --- | --- |
| `SalesTopSalesmanId` | `VARCHAR(13)` PK |
| `SnapshotKey` | `VARCHAR(10)` |
| `Rank` | `INT` |
| `SalesPersonName` | `VARCHAR(30)` |
| `CompletedOmzet` | `DECIMAL(18,2)` — Faktur `GrandTotal` sum |

### 4.5 Layer C — Piutang open fact (deferred)

**Not required for initial delivery.** The refresh worker can aggregate in memory from an efficient open-balance query (expected row count: open fakturs only). Add `BTRPD_PiutangOpenFact` only if shadow reconciliation requires persisted row-level compare or refresh memory becomes prohibitive.

### 4.6 Source table index

Add filtered index to accelerate refresh query:

```sql
CREATE INDEX IX_BTR_Piutang_OpenBalance
    ON BTR_Piutang (Sisa, PiutangId)
    INCLUDE (DueDate, Total, CustomerId)
    WHERE Sisa > 1;
```

Deploy index in Phase 1 before worker cutover.

### 4.7 SQL project files

Add under `btr.sql/Tables/ReportingContext/`:

- `BTRPD_RefreshLog.sql`
- `BTRPD_PiutangKpi.sql`
- `BTRPD_PiutangAging.sql`
- `BTRPD_PiutangTopCustomer.sql`
- `BTRPD_InventoryKpi.sql`
- `BTRPD_InventoryBreakdown.sql`
- `BTRPD_SalesKpi.sql`
- `BTRPD_SalesWeekTrend.sql`
- `BTRPD_SalesTopSalesman.sql`

Register all in `btr.sql.sqlproj`.

---

## 5. Application Design

### 5.1 New aggregate: `DashboardSnapshotAgg`

```text
btr.application/ReportingContext/DashboardSnapshotAgg/
├── Contracts/
│   ├── IDashboardSnapshotRefreshLogDal.cs
│   ├── IDashboardPiutangSnapshotDal.cs
│   ├── IDashboardInventorySnapshotDal.cs
│   ├── IDashboardSalesSnapshotDal.cs
│   └── IPiutangOpenBalanceDal.cs          (efficient source read)
├── Services/
│   ├── DashboardPiutangAggregator.cs
│   ├── DashboardInventoryAggregator.cs
│   └── DashboardSalesFakturAggregator.cs
├── Models/
│   ├── DashboardPiutangAggregateResult.cs
│   ├── DashboardInventoryAggregateResult.cs
│   └── DashboardSalesAggregateResult.cs
└── UseCases/
    ├── RefreshDashboardPiutangSnapshotWorker.cs
    ├── RefreshDashboardInventorySnapshotWorker.cs
    ├── RefreshDashboardSalesSnapshotWorker.cs
    └── RefreshAllDashboardSnapshotsWorker.cs

btr.infrastructure/ReportingContext/DashboardSnapshotAgg/
├── DashboardSnapshotRefreshLogDal.cs
├── DashboardPiutangSnapshotDal.cs
├── DashboardInventorySnapshotDal.cs
├── DashboardSalesSnapshotDal.cs
├── DashboardOverviewDal.cs               (Layer A reads — Phase 4)
└── PiutangOpenBalanceDal.cs

btr.application/ReportingContext/DashboardOverviewAgg/   (Phase 4)
├── Contracts/IDashboardOverviewDal.cs
└── Queries/GetDashboardOverviewQuery.cs
```

### 5.2 Aggregation extraction

Move computation from existing DALs into aggregator services. Each aggregator returns a result model matching the portal response shape.

#### `DashboardPiutangAggregator`

| Input | Source |
| --- | --- |
| Open receivable rows | `IPiutangOpenBalanceDal.ListOpenBalances()` |
| Today (aging reference) | `ITglJamDal.Now.Date` at refresh time |

Logic: **copy verbatim** from current `DashboardPiutangDal` — `ResolveAgingBucketKey`, `ResolveCustomerKey`, bucket definitions, Top 10 sort.

**Do not call** `IPiutangSalesWilayahDal` in the refresh path.

#### `DashboardInventoryAggregator`

| Input | Source |
| --- | --- |
| Stock balance rows | `IStokBalanceViewDal.ListData()` |

Logic: **copy verbatim** from current `DashboardInventoryDal`.

#### `DashboardSalesFakturAggregator` (new semantics)

| Input | Source |
| --- | --- |
| Faktur rows | `IFakturViewDal.ListData(currentMonthPeriode)` |
| Target | `ISalesOmzetTargetDal.SumTargetAmountForMonth(year, month)` |
| Week buckets | Reuse `SalesOmzetChartWeekGrouper` from `btr.application` |

| Output field | Faktur rule |
| --- | --- |
| `TotalOmzet` | `Sum(GrandTotal)` |
| `CompletedOmzet` | Same as `TotalOmzet` |
| `PipelineOmzet` | `0` |
| `TotalFaktur` | Row count |
| `TotalCustomer` | Distinct `CustomerCode` (fallback `Customer`) |
| `WeeklyTrend` | `GrandTotal` grouped by calendar week within month |
| `TopSalesmanRanking` | Top 10 by `Sum(GrandTotal)` per `SalesPersonName` |
| `TotalTarget` | From target DAL (unchanged) |
| `TotalAchievement` | Same as `TotalOmzet` |
| `AchievementPercent` | `SalesOmzetChartAchievementPolicy.ComputePercent` (reuse) |

Void fakturs are already excluded by `FakturViewDal` (`VoidDate = '3000-01-01'`).

### 5.3 Refresh workers

Each domain worker follows this template:

1. Insert `RefreshLog` row — `Status = Running`.
2. Call aggregator inside `TransHelper.NewScope()`.
3. Delete child rows + upsert KPI row + insert child rows.
4. Update `RefreshLog` — `Status = Success`, `DurationMs`.
5. On exception: update `RefreshLog` — `Status = Failed`, `ErrorMessage`; rethrow.

`RefreshAllDashboardSnapshotsWorker` runs Piutang → Inventory → Sales sequentially (Piutang first per business priority). Each domain is independent — a failure in Sales does not roll back Piutang if Piutang already committed (log per domain).

**Idempotency:** Each run replaces `CURRENT` snapshot entirely. Safe to re-run.

**Month rollover (Sales):** Worker must detect when `PeriodYear/PeriodMonth` on stored KPI ≠ current month and force full rebuild.

### 5.4 Read path — modify existing `Dashboard*Dal`

Rewrite `GetSummary()` to read from snapshot DAL and map to existing response DTOs. **Do not change** `IDashboard*Dal` interface or MediatR handlers.

| DAL | Read behavior |
| --- | --- |
| `DashboardPiutangDal` | `IDashboardPiutangSnapshotDal.GetCurrent()` → map to `DashboardPiutangResponse` |
| `DashboardInventoryDal` | `IDashboardInventorySnapshotDal.GetCurrent()` → map |
| `DashboardSalesDal` | `IDashboardSalesSnapshotDal.GetCurrent()` → map |

**Empty snapshot handling:** If no `CURRENT` row exists (first deploy), return HTTP 503 from API with message "Dashboard data not yet available" OR fall back to live aggregation behind config flag `Dashboard:AllowLiveFallback` (default `true` until first successful refresh). Implementer: use config flag for safer rollout.

`GeneratedAt` on API response = `GeneratedAt` from KPI table, **not** `ITglJamDal.Now`.

### 5.5 New source DAL: `PiutangOpenBalanceDal`

Efficient refresh query — join dimensions, filter open balances only:

```sql
SELECT
    ISNULL(ee.CustomerCode, '') AS CustomerCode,
    ISNULL(ee.CustomerName, '') AS CustomerName,
    ISNULL(aa.DueDate, '3000-01-01') AS JatuhTempo,
    aa.Sisa AS KurangBayar
FROM BTR_Piutang aa
    LEFT JOIN BTR_Faktur bb ON aa.PiutangId = bb.FakturId
    LEFT JOIN BTR_Customer ee ON bb.CustomerId = ee.CustomerId
WHERE aa.Sisa > 1
```

Map to a slim DTO (`PiutangOpenBalanceDto`) with only fields needed for dashboard aggregation. Register in portal infrastructure extensions.

**Note:** This is a new SQL query for the **refresh worker only**. It does not violate the portal "no duplicate SQL" rule for request-path DALs because request path no longer hits operational tables for dashboards.

### 5.6 `btr.portal.worker` console host

| Concern | Approach |
| --- | --- |
| Framework | .NET Framework 4.8 console EXE |
| DI | Same pattern as `btr.portal.api` — `Microsoft.Extensions.DependencyInjection`, `DependencyConfig` shared or mirrored |
| Config | `appsettings.json` + `appsettings.{MachineName}.json` (copy from portal API) |
| Entry | `Program.cs` — parse args: `--domain All|Sales|Piutang|Inventory`, `--triggered-by Scheduler|Manual` |
| Exit code | `0` on success, non-zero on failure (for Task Scheduler monitoring) |
| Logging | NLog to file |

Add project to `j05-btr-distrib.sln`.

### 5.7 DI registration

**`InfrastructurePortalExtensions`:** Register snapshot DALs. `Dashboard*Dal` read implementations unchanged registration names.

**`btr.portal.worker`:** Register aggregators, workers, snapshot writers, source DALs. Do **not** register live dashboard read fallbacks unless config enabled.

---

## 6. API and Frontend Changes

### 6.1 API

| Endpoint | Change |
| --- | --- |
| `GET /api/dashboard/sales` | Read path only — same controller, handler, response shape |
| `GET /api/dashboard/piutang` | Read path only |
| `GET /api/dashboard/inventory` | Read path only |
| `GET /api/dashboard/overview` | **New** — Layer A KPIs from snapshot tables only |

#### `GET /api/dashboard/overview` (new)

Serves Dashboard Home (`/dashboard`) without loading Layer B dimensional data.

**Response shape — `DashboardOverviewResponse`:**

| Field | Source |
| --- | --- |
| `Sales.TotalOmzet` | `BTRPD_SalesKpi` |
| `Sales.TotalFaktur` | `BTRPD_SalesKpi` |
| `Sales.TotalCustomer` | `BTRPD_SalesKpi` |
| `Sales.GeneratedAt` | `BTRPD_SalesKpi.GeneratedAt` |
| `Piutang.TotalPiutang` | `BTRPD_PiutangKpi` |
| `Piutang.TotalCustomer` | `BTRPD_PiutangKpi` |
| `Piutang.GeneratedAt` | `BTRPD_PiutangKpi.GeneratedAt` |
| `Inventory.TotalInventoryValue` | `BTRPD_InventoryKpi` |
| `Inventory.TotalItem` | `BTRPD_InventoryKpi` |
| `Inventory.GeneratedAt` | `BTRPD_InventoryKpi.GeneratedAt` |

Implementation: `DashboardOverviewDal` performs three indexed reads against KPI tables. No joins to Layer B tables. JWT required (same as other dashboard routes).

Optional: extend `HealthController` to expose last refresh status per domain from `BTRPD_RefreshLog`.

### 6.2 Frontend (`btr.portal.web`)

| Change | Priority |
| --- | --- |
| Display `GeneratedAt` prominently on analytical pages (detail layout footer) | Required |
| Sales dashboard subtitle: change "omzet period" → "invoiced sales (Faktur)" | Required |
| Home card Sales labels: clarify Faktur-based omzet (`SUM(GrandTotal)`) | Required |
| Ranking column header: "Completed Omzet" → "Invoiced Omzet" (or keep API field name, change label only) | Recommended |
| Hide/remove pipeline references if any remain on home card | Check `DashboardHomeView.vue` |

Add `loadOverview()` in `dashboardApi.ts` / `dashboardStore` — home page calls `GET /api/dashboard/overview` instead of three full dashboard endpoints. Detail pages continue using existing `loadSales()`, `loadPiutang()`, `loadInventory()`.

### 6.3 Domain documentation update

After Sales cutover, update `docs/features/btr-portal/btr-portal-domain.md` Sales KPI section to reflect Faktur-based definitions per analysis Section 5.3. Include stakeholder communication note about pipeline exclusion.

---

## 7. Migration and Verification Strategy

### 7.1 Shadow-run phase

Before switching read path:

1. Deploy tables and worker.
2. Run worker on schedule against production data.
3. Add **`DashboardSnapshotVerificationTest`** (integration or manual script) comparing:
   - Piutang: aggregator output vs current `DashboardPiutangDal` (live) — totals, buckets, Top 10.
   - Inventory: aggregator vs live — totals, breakdowns.
   - Sales: aggregator vs `Sum(Faktur.GrandTotal)` from `SalesReportDal` rows — not vs old `DashboardSalesDal`.
4. Log diffs; investigate any Piutang variance (likely `Sisa` vs recomputed `KurangBayar` — document tolerance).

### 7.2 Cutover sequence

1. Enable read from snapshot with `AllowLiveFallback = true`.
2. Run shadow comparison for 3–5 business days.
3. Set `AllowLiveFallback = false`.
4. Remove live aggregation code path from `Dashboard*Dal` (keep aggregators for worker).

### 7.3 Rollback

Re-enable `AllowLiveFallback = true` and revert DI to live DAL implementations. Snapshot tables can remain — they are inert when not read.

---

## 8. Overview Dashboard — Confirmed (Option B)

**Product decision:** Dashboard Home (`/dashboard`) reads **Layer A KPI snapshots only**.

| Concern | Approach |
| --- | --- |
| Home page performance | `GET /api/dashboard/overview` — three fast KPI table reads, no Layer B |
| Detail pages | Unchanged routes; full `GET /api/dashboard/{domain}` with charts and Top 10 |
| Staleness display | Per-domain `GeneratedAt` on overview response (domains may differ if refresh cadences differ) |
| Fallback | If a domain KPI snapshot is missing, return partial overview with `null` section + global error flag, or fall back to live KPI read behind `AllowLiveFallback` |

**Delivery:** Ship in **Phase 4** after all three domain KPI tables are populated. Do not block Phases 1–3 analytical cutover on overview endpoint — home may continue calling full endpoints until Phase 4.

---

## 9. Refresh Schedule — Confirmed

Default configuration in `appsettings.json`:

```json
{
  "DashboardSnapshot": {
    "AllowLiveFallback": true,
    "PiutangIntervalMinutes": 15,
    "InventoryIntervalMinutes": 60,
    "SalesIntervalMinutes": 30
  }
}
```

**Product decision:** Piutang 15 min, Sales 30 min, Inventory 60 min maximum staleness.

Task Scheduler setup (operations runbook) — **three separate jobs** so each domain respects its cadence:

| Job | Schedule | Worker command |
| --- | --- | --- |
| `BTR-Portal-Dashboard-Piutang` | Every **15 minutes** | `btr.portal.worker.exe --domain Piutang --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Sales` | Every **30 minutes** | `btr.portal.worker.exe --domain Sales --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Inventory` | Every **60 minutes** | `btr.portal.worker.exe --domain Inventory --triggered-by Scheduler` |

`RefreshAllDashboardSnapshotsWorker` remains available for initial backfill and manual ops (`--domain All`). Scheduled production use prefers per-domain jobs.

Piutang refreshes most frequently because balances change intraday with pelunasan.

**Future event-driven triggers (out of scope):** hook after `AddLunasPiutangWorker`, `GenStokBalanceWorker` — document as extension point only.

---

## 10. Risk Assessment

| Risk | Severity | Mitigation |
| --- | --- | --- |
| Snapshot stale vs Desktop | Medium | Prominent `GeneratedAt`; confirmed cadences (15/30/60 min); per-domain timestamps on overview |
| Sales KPI semantic shift | Medium | Confirmed Faktur-only source; update domain doc; compare against Sales Report before go-live |
| Piutang `Sisa` ≠ subquery `KurangBayar` | Medium | Shadow-run reconciliation; sample against Piutang Report footer |
| No snapshot on first deploy | Low | `AllowLiveFallback = true` until first successful refresh |
| Worker failure silent | Medium | `RefreshLog` status; non-zero exit code; NLog alerts |
| Month-boundary Sales snapshot | Low | Worker validates `PeriodYear/Month` on each run |
| Overview partial snapshot on first deploy | Low | `AllowLiveFallback` until all three KPI tables populated; Phase 4 ships overview endpoint |
| IIS / worker credential access | Low | Worker uses same SQL credentials as portal `appsettings.json` |

---

## 11. Acceptance Criteria

| # | Criterion | Verification |
| --- | --- | --- |
| 1 | Analytical dashboard APIs read from snapshot tables when fallback disabled | Code review + integration test |
| 2 | p95 response time &lt; 500 ms for each dashboard GET (snapshot populated, typical data volume) | Manual/load test with timestamp |
| 3 | Piutang KPIs, aging buckets, Top 10 match live `DashboardPiutangDal` output | Shadow-run diff test |
| 4 | Inventory KPIs and breakdowns match live `DashboardInventoryDal` output | Shadow-run diff test |
| 5 | Sales KPIs match Faktur-based definitions; totals reconcile with Sales Report for current month | Compare `TotalOmzet` to sum of report rows |
| 6 | `GeneratedAt` equals last successful refresh `CompletedAt`, not request time | API response vs `RefreshLog` |
| 7 | `BTR_SalesOmzet` reconcile workflow unaffected | Regression test `SalesOmzetReconcileTest` |
| 8 | Overview home loads from `GET /api/dashboard/overview` (Layer A only); p95 &lt; 500 ms | Home page load test |
| 9 | `PipelineOmzet` returned as `0`; ranking uses Faktur totals | API response inspection |
| 10 | Refresh cadence matches 15 / 30 / 60 min per domain | `RefreshLog` timestamps over 2-hour window |

---

## 12. Implementation Phases

### Phase 1 — Piutang snapshot (highest value)

| Step | Task |
| --- | --- |
| 1.1 | Create SQL tables: `RefreshLog`, Piutang KPI/Aging/TopCustomer |
| 1.2 | Add `IX_BTR_Piutang_OpenBalance` index |
| 1.3 | Implement `PiutangOpenBalanceDal`, `DashboardPiutangAggregator` |
| 1.4 | Implement `RefreshDashboardPiutangSnapshotWorker` + snapshot writer/reader DALs |
| 1.5 | Unit tests for aggregator (mirror `DashboardInventoryDalTest` style) |
| 1.6 | Shadow-run: compare aggregator vs live `DashboardPiutangDal` |
| 1.7 | Rewrite `DashboardPiutangDal` read path; enable with fallback flag |

**Exit:** Piutang API serves from snapshot; Piutang Report footer still reconciles.

### Phase 2 — Inventory snapshot

| Step | Task |
| --- | --- |
| 2.1 | Create SQL tables: Inventory KPI, Breakdown |
| 2.2 | Implement `DashboardInventoryAggregator` (extract from existing DAL) |
| 2.3 | Implement `RefreshDashboardInventorySnapshotWorker` |
| 2.4 | Shadow-run vs live `DashboardInventoryDal` |
| 2.5 | Rewrite `DashboardInventoryDal` read path |

**Exit:** Inventory API serves from snapshot; Inventory Report footer still reconciles.

### Phase 3 — Sales snapshot (Faktur source)

| Step | Task |
| --- | --- |
| 3.1 | Create SQL tables: Sales KPI, WeekTrend, TopSalesman |
| 3.2 | Implement `DashboardSalesFakturAggregator` |
| 3.3 | Implement `RefreshDashboardSalesSnapshotWorker` |
| 3.4 | Verify against Sales Report totals |
| 3.5 | Rewrite `DashboardSalesDal` read path |
| 3.6 | Update frontend labels; update `btr-portal-domain.md` Sales KPIs |

**Exit:** Sales API serves Faktur-based snapshot; aligns with Sales Report.

### Phase 4 — Worker host, overview endpoint, and operations

| Step | Task |
| --- | --- |
| 4.1 | Create `btr.portal.worker` project |
| 4.2 | Implement `RefreshAllDashboardSnapshotsWorker` orchestrator |
| 4.3 | Document per-domain Task Scheduler jobs (15 / 30 / 60 min) in deploy runbook |
| 4.4 | Implement `GET /api/dashboard/overview` + `DashboardOverviewDal` (Layer A reads) |
| 4.5 | Update `DashboardHomeView` / `dashboardStore` to use overview endpoint |
| 4.6 | Disable `AllowLiveFallback` after shadow period |
| 4.7 | Remove dead live-aggregation code from read DALs |

**Exit:** Scheduled per-domain refresh operational; overview home fast path live; live fallback disabled in production.

### Phase 5 — Deferred enhancements

| Step | Task |
| --- | --- |
| 5.1 | Manual refresh — `POST /api/admin/dashboard/refresh` and/or BTR Desktop trigger (confirmed needed eventually) |
| 5.2 | `HealthController` refresh status |
| 5.3 | Layer C piutang open fact if reconciliation requires it |
| 5.4 | Historical monthly snapshot retention (only if M16+ date-range analytics require it) |

---

## 13. Testing Plan

| Test type | Scope |
| --- | --- |
| Unit | `DashboardPiutangAggregator`, `DashboardInventoryAggregator`, `DashboardSalesFakturAggregator` — pure logic with fixture rows |
| Unit | Snapshot DAL mapping to response DTOs |
| Integration | Worker writes snapshot; read DAL returns expected response |
| Regression | Existing `DashboardInventoryDalTest` patterns extended for aggregators |
| Reconciliation | Automated or scripted shadow diff (Phase 7.1) |
| Manual | Board stakeholder review of Sales semantic change before go-live |
| Performance | Measure dashboard GET p95 before/after with snapshot populated |

---

## 14. Product Decisions — Confirmed

| Question | Decision | Implementation impact |
| --- | --- | --- |
| Overview Dashboard | **Option B** — Layer A KPI snapshot | `GET /api/dashboard/overview` in Phase 4; home store switch |
| Sales KPI source | **Faktur only** — `SUM(GrandTotal)` for non-void current-month Fakturs | `DashboardSalesFakturAggregator`; `PipelineOmzet = 0`; domain doc update |
| Refresh cadence | **Piutang 15 min, Sales 30 min, Inventory 60 min** | Three Task Scheduler jobs; per-domain `GeneratedAt` on overview |
| Manual refresh | **Needed eventually; defer from Phase 1** | No admin endpoint or Desktop trigger until Phase 5; worker CLI acceptable for ops in Phase 4 |
| Historical snapshots | **Not now — `CURRENT` only** | Delete-and-replace per refresh; no archive tables |

---

## 15. Reference — Current Code Locations

| Component | Location |
| --- | --- |
| Piutang dashboard DAL (live) | `btr.infrastructure/ReportingContext/DashboardPiutangAgg/DashboardPiutangDal.cs` |
| Inventory dashboard DAL (live) | `btr.infrastructure/ReportingContext/DashboardInventoryAgg/DashboardInventoryDal.cs` |
| Sales dashboard DAL (live) | `btr.infrastructure/ReportingContext/DashboardSalesAgg/DashboardSalesDal.cs` |
| Piutang heavy source SQL | `btr.infrastructure/FinanceContext/PiutangSalesWilayahRpt/PiutangSalesWilayahDal.cs` |
| Faktur view SQL | `btr.infrastructure/SalesContext/FakturInfoAgg/FakturViewDal.cs` |
| Sales report (Faktur) | `btr.infrastructure/ReportingContext/SalesReportAgg/SalesReportDal.cs` |
| Stock balance view | `btr.infrastructure/InventoryContext/StokBalanceRpt/StokBalanceViewDal.cs` |
| Week grouper (reuse) | `btr.application/SalesContext/SalesOmzetAgg/Services/SalesOmzetChartWeekGrouper.cs` |
| Materialization precedent | `ReconcileSalesOmzetWorker`, `GenStokBalanceWorker`, `BTR_SalesOmzetHealthWeekly` |
| Portal DI | `btr.portal.api/Configurations/InfrastructurePortalExtensions.cs` |

---

## 16. Architecture Decision Summary

| Decision | Choice | Rationale |
| --- | --- | --- |
| Snapshot storage | Same SQL Server DB, `BTRPD_*` tables | Consistent with `BTR_StokBalanceWarehouse` / `BTR_SalesOmzet` pattern |
| Active snapshot pattern | Single `CURRENT` row per domain | Simplest read path; no history management |
| Worker host | `btr.portal.worker` + Task Scheduler | No IIS timer fragility; no distrib coupling |
| Piutang source | New `PiutangOpenBalanceDal` with `Sisa > 1` | Eliminates 25-year date scan; business-equivalent |
| Sales source | `IFakturViewDal` via new aggregator | Board alignment with Sales Report |
| API contract | Unchanged endpoints and DTOs | Minimizes frontend churn |
| Delivery order | Piutang → Inventory → Sales → Worker + Overview | Matches analysis priority and risk profile |
| Overview home | Layer A snapshot via `GET /api/dashboard/overview` | Confirmed Option B — Phase 4 |
| Refresh cadence | Piutang 15 min / Sales 30 min / Inventory 60 min | Confirmed; separate scheduler jobs |
| Manual refresh | Deferred to Phase 5 | Worker CLI in Phase 4 for ops only |
| Snapshot history | `CURRENT` row only | No historical retention |
