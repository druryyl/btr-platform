# Materialized Dashboard Data — Architecture

**Audience:** Developers, Architects, Future Agents  
**Purpose:** Describe how BTR Portal dashboard analytics are pre-computed, stored, and served.

**Related docs:** [Domain (WHY)](./materialized-dashboard-domain.md) · [Operational (HOW)](./materialized-dashboard-operational.md) · [BTR Portal architecture](../btr-portal/btr-portal-architecture.md)

---

## Problem Solved

Analytical dashboard routes (`/dashboard/sales`, `/dashboard/piutang`, `/dashboard/inventory`) previously aggregated operational data on every HTTP request. Piutang in particular scanned receivables from 2000-01-01 through today with heavy joins. Materialization moves aggregation to a background worker and serves pre-computed results from SQL snapshot tables.

Reports (`/reports/*`) are **not** materialized — they continue live Desktop DAL queries.

---

## Topology

```text
Windows Task Scheduler (per-domain jobs, 15 / 30 / 60 min)
        ↓
btr.portal.worker.exe  (--domain Piutang|Sales|Inventory|All)
        ↓
RefreshDashboard{Domain}SnapshotWorker
        ↓
Dashboard{Domain}Aggregator  (shared aggregation rules)
        ↓
Dashboard{Domain}SnapshotDal.ReplaceCurrent()
        ↓
BTR_PortalDashboard* tables  (SnapshotKey = 'CURRENT')

Browser → GET /api/dashboard/overview              (home — Layer A KPI only)
Browser → GET /api/dashboard/{sales|piutang|inventory}  (detail — Layer A + B)
        ↓ MediatR → Dashboard*Dal → Dashboard*SnapshotDal → snapshot SELECT
```

**Worker host:** `btr.portal.worker` (.NET Framework 4.8 console). Not hosted in IIS — app pool recycle would kill in-process timers.

**Database:** Same SQL Server as BTR Desktop. Tables prefixed `BTR_PortalDashboard*`.

---

## Snapshot Layers

| Layer | Tables | Consumed by |
| ----- | ------ | ----------- |
| **A — KPI** | `BTR_PortalDashboard{Sales,Piutang,Inventory}Kpi` | Overview home + detail KPI rows |
| **B — Dimensional** | Aging, TopCustomer, Breakdown, WeekTrend, TopSalesman | Detail dashboards only |
| **Metadata** | `BTR_PortalDashboardRefreshLog` | Health endpoint, ops monitoring |

**Active snapshot pattern:** One row per domain with `SnapshotKey = 'CURRENT'`. Each refresh deletes child rows and upserts KPI within a transaction. No historical snapshot retention.

**GeneratedAt:** Stored on KPI row at refresh completion. API responses expose this timestamp — not request execution time.

---

## Refresh Workers

| Worker | Source DAL | Aggregator |
| ------ | ---------- | ---------- |
| `RefreshDashboardPiutangSnapshotWorker` | `IPiutangOpenBalanceDal` (`BTR_Piutang.Sisa > 1`) | `DashboardPiutangAggregator` |
| `RefreshDashboardInventorySnapshotWorker` | `IStokBalanceViewDal.ListData()` | `DashboardInventoryAggregator` |
| `RefreshDashboardSalesSnapshotWorker` | `IFakturViewDal` (current month) + `ISalesOmzetTargetDal` | `DashboardSalesFakturAggregator` |
| `RefreshAllDashboardSnapshotsWorker` | Orchestrator | Piutang → Inventory → Sales; per-domain failure isolation |

**CLI arguments:**

| Argument | Values | Default |
| -------- | ------ | ------- |
| `--domain` | `All`, `Piutang`, `Inventory`, `Sales` | `All` |
| `--triggered-by` | `Scheduler`, `Manual` | `Scheduler` |

Exit code `0` = success. Logs: `{worker-folder}/logs/btr-portal-worker-{date}.log`.

**Scheduled cadence (Task Scheduler — three separate jobs):**

| Domain | Interval |
| ------ | -------- |
| Piutang | 15 minutes |
| Sales | 30 minutes |
| Inventory | 60 minutes |

---

## Application Structure

```text
btr.application/ReportingContext/
├── DashboardSnapshotAgg/
│   ├── Contracts/          IDashboard*SnapshotDal, IPiutangOpenBalanceDal, IDashboardSnapshotRefreshLogDal
│   ├── Services/           DashboardPiutangAggregator, DashboardInventoryAggregator, DashboardSalesFakturAggregator
│   ├── Models/             Aggregate result DTOs
│   └── UseCases/           RefreshDashboard*SnapshotWorker, RefreshAllDashboardSnapshotsWorker
├── DashboardOverviewAgg/   GetDashboardOverviewQuery, IDashboardOverviewDal
├── DashboardSalesAgg/      DashboardSalesDal (read facade)
├── DashboardPiutangAgg/    DashboardPiutangDal (read facade)
└── DashboardInventoryAgg/  DashboardInventoryDal (read facade)

btr.infrastructure/ReportingContext/
├── DashboardSnapshotAgg/   Snapshot readers/writers, PiutangOpenBalanceDal, RefreshLogDal
├── DashboardOverviewDal.cs
└── Dashboard*Agg/          Read DALs map snapshot → response DTOs

btr.portal.worker/          Program.cs, WorkerDependencyConfig, appsettings.json, NLog.config
```

**Design principles:**

| Principle | Application |
| --------- | ----------- |
| Preserve API contract | Same endpoints and `Dashboard{Domain}Response` DTOs |
| Extract, don't duplicate | Aggregators hold shared rules; workers and shadow tests reuse them |
| Snapshot-only read path | No live aggregation fallback in production read DALs |
| No `btr.distrib` reference | Worker is separate console host |
| Transactional replace | Delete child rows + MERGE KPI per refresh (like `ReconcileSalesOmzetWorker`) |

**Live DAL classes** (`Dashboard*LiveDal`) remain for unit/shadow verification tests only — not registered in production read path.

---

## Database Objects

### KPI tables (Layer A)

| Table | Key columns |
| ----- | ----------- |
| `BTR_PortalDashboardPiutangKpi` | `TotalPiutang`, `TotalCustomer`, `OverdueCustomer`, `GeneratedAt` |
| `BTR_PortalDashboardInventoryKpi` | `TotalInventoryValue`, `TotalItem`, `GeneratedAt` |
| `BTR_PortalDashboardSalesKpi` | `PeriodYear`, `PeriodMonth`, omzet/faktur/customer/target/achievement fields, `PipelineOmzet` (= 0) |

### Dimensional tables (Layer B)

| Table | Content |
| ----- | ------- |
| `BTR_PortalDashboardPiutangAging` | 5 aging buckets |
| `BTR_PortalDashboardPiutangTopCustomer` | Top 10 customers |
| `BTR_PortalDashboardInventoryBreakdown` | Category/supplier rows with `IsTop10` flag |
| `BTR_PortalDashboardSalesWeekTrend` | Weekly Faktur totals |
| `BTR_PortalDashboardSalesTopSalesman` | Top 10 salespeople |

### Supporting objects

| Object | Purpose |
| ------ | ------- |
| `BTR_PortalDashboardRefreshLog` | Per-attempt audit (domain, status, duration, error, trigger) |
| `IX_BTR_Piutang_OpenBalance` | Filtered index `WHERE Sisa > 1` — accelerates Piutang refresh |
| `BTR_ParamNo_PortalDashboard.sql` | ID prefixes: `PDR`, `PDA`, `PDT`, `PDB`, `PDW`, `PDS` |

SQL definitions: `btr.sql/Tables/ReportingContext/BTR_PortalDashboard*.sql`

---

## API Endpoints

| Endpoint | Auth | Data source | Notes |
| -------- | ---- | ----------- | ----- |
| `GET /api/dashboard/overview` | JWT | Layer A KPI tables only | Home page — fast path |
| `GET /api/dashboard/sales` | JWT | Layer A + B | Full sales analytics |
| `GET /api/dashboard/piutang` | JWT | Layer A + B | Full piutang analytics |
| `GET /api/dashboard/inventory` | JWT | Layer A + B | Full inventory analytics |
| `POST /api/admin/dashboard/refresh` | JWT | Triggers workers synchronously | IIS timeout risk for `--domain All` |
| `GET /api/health/dashboard-snapshots` | None | `BTR_PortalDashboardRefreshLog` | `unknown` / `ok` / `refreshing` / `degraded` |

**Empty snapshot behavior:**

- Detail endpoints → HTTP 503 (`DashboardSnapshotUnavailableException`)
- Overview → partial response with `HasUnavailableDomain = true` when any Layer A row missing

---

## Piutang Refresh Optimization

Pre-materialization: `PiutangSalesWilayahDal` scanned `PiutangDate BETWEEN 2000-01-01 AND today` with correlated payment subqueries.

Post-materialization refresh path:

```sql
SELECT ... FROM BTR_Piutang aa
  LEFT JOIN BTR_Faktur bb ON aa.PiutangId = bb.FakturId
  LEFT JOIN BTR_Customer ee ON bb.CustomerId = ee.CustomerId
WHERE aa.Sisa > 1
```

Aggregation rules unchanged (`KurangBayar > 1`, aging buckets, customer key, Top 10). Business result equivalent; performance improved.

---

## Sales Source Change

| Aspect | Before | After |
| ------ | ------ | ----- |
| Source table | `BTR_SalesOmzet` | `BTR_Faktur` via `IFakturViewDal` |
| Omzet basis | Omzet recognition (Completed/Pending/Outstanding) | `SUM(GrandTotal)` invoiced |
| Pipeline | Included in model | `PipelineOmzet` always `0` |
| Report alignment | Indirect | Direct — same source as Sales Report |

`BTR_SalesOmzet` and `ReconcileSalesOmzetWorker` are **unchanged** — Desktop RO2 / salesperson fee workflows unaffected.

---

## Configuration

`appsettings.json` → `DashboardSnapshot` (informational cadence; Task Scheduler enforces schedule):

```json
{
  "DashboardSnapshot": {
    "PiutangIntervalMinutes": 15,
    "InventoryIntervalMinutes": 60,
    "SalesIntervalMinutes": 30
  }
}
```

Portal API and worker both require `Database` section (JSON only — no registry). Use `appsettings.{MachineName}.json` for per-server SQL settings.

---

## Key Code Locations

| Component | Path |
| --------- | ---- |
| Piutang aggregator | `btr.application/ReportingContext/DashboardSnapshotAgg/Services/DashboardPiutangAggregator.cs` |
| Sales Faktur aggregator | `.../DashboardSalesFakturAggregator.cs` |
| Inventory aggregator | `.../DashboardInventoryAggregator.cs` |
| Snapshot workers | `.../DashboardSnapshotAgg/UseCases/RefreshDashboard*SnapshotWorker.cs` |
| Worker CLI | `btr.portal.worker/Program.cs` |
| Read DALs | `btr.infrastructure/ReportingContext/Dashboard*Agg/Dashboard*Dal.cs` |
| Overview DAL | `.../DashboardSnapshotAgg/DashboardOverviewDal.cs` |
| Admin refresh | `btr.portal.api/Controllers/Admin/AdminDashboardRefreshController.cs` |
| Portal DI | `btr.portal.api/Configurations/InfrastructurePortalExtensions.cs` |

---

## Architectural Rules (Materialization)

| # | Rule |
| - | ---- |
| 1 | Dashboard read path uses snapshot tables only — no live aggregation in production |
| 2 | Snapshot refresh runs in `btr.portal.worker`, not inside IIS app pool |
| 3 | Aggregators are the single source of truth for dashboard calculation rules |
| 4 | `GeneratedAt` on API = KPI table refresh time, not `ITglJamDal.Now` on read |
| 5 | Reports remain live queries — do not snapshot report endpoints |
| 6 | Preserve dashboard–report traceability (see domain doc) |
| 7 | `SnapshotKey = 'CURRENT'` only — no historical snapshot tables unless product adds date-range analytics |

---

## Deferred (Not Implemented)

| Item | Rationale |
| ---- | --------- |
| Layer C piutang open fact table | Shadow reconciliation passed without persisted row-level fact |
| Historical monthly snapshot retention | Product decision — `CURRENT` row only |
| BTR Desktop manual refresh trigger | Worker CLI + portal API cover operations |
| Event-driven refresh hooks | Extension point after pelunasan / stock balance — future |
