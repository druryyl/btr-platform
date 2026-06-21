# Materialized Dashboard Data — Architecture

> **Table naming:** Portal snapshot tables use the `BTRPD_*` prefix (formerly `BTR_PortalDashboard*`).

**Audience:** Developers, Architects, Future Agents  
**Purpose:** Describe how BTR Portal dashboard analytics are pre-computed, stored, and served.

**Related docs:** [Domain (WHY)](./materialized-dashboard-domain.md) · [Operational (HOW)](./materialized-dashboard-operational.md) · [BTR Portal architecture](../btr-portal/btr-portal-architecture.md)

---

## Problem Solved

Analytical dashboard routes (`/dashboard/sales`, `/dashboard/piutang`, `/dashboard/inventory`, `/dashboard/purchasing`) previously aggregated operational data on every HTTP request. Piutang in particular scanned receivables from 2000-01-01 through today with heavy joins. Materialization moves aggregation to a background worker and serves pre-computed results from SQL snapshot tables.

Reports (`/reports/*`) are **not** materialized — they continue live Desktop DAL queries.

---

## Topology

```text
Windows Task Scheduler (per-domain jobs; see cadence table below)
        ↓
btr.portal.worker.exe  (--domain Piutang|Inventory|InventoryRisk|Sales|Purchasing|PurchasingManagement|Customer|Salesman|All)
        ↓
RefreshDashboard{Domain}SnapshotWorker
        ↓
Dashboard{Domain}Aggregator  (shared aggregation rules)
        ↓
Dashboard{Domain}SnapshotDal.ReplaceCurrent()
        ↓
BTRPD_* tables  (SnapshotKey = 'CURRENT')

Browser → GET /api/dashboard/overview              (home — Layer A KPI only)
Browser → GET /api/dashboard/{sales|piutang|inventory|purchasing}  (detail — Layer A + B)
        ↓ MediatR → Dashboard*Dal → Dashboard*SnapshotDal → snapshot SELECT
```

**Worker host:** `btr.portal.worker` (.NET Framework 4.8 console). Not hosted in IIS — app pool recycle would kill in-process timers.

**Database:** Same SQL Server as BTR Desktop. Tables prefixed `BTRPD_*`.

---

## Snapshot Layers

| Layer | Tables | Consumed by |
| ----- | ------ | ----------- |
| **A — KPI** | `BTRPD_{Sales,Piutang,Inventory,Purchasing}Kpi` | Overview home + detail KPI rows |
| **B — Dimensional** | Aging, TopCustomer, Breakdown, WeekTrend, TopSalesman, PostingStatus, TopPrincipal | Detail dashboards only |
| **Metadata** | `BTRPD_RefreshLog` | Health endpoint, ops monitoring |

**Active snapshot pattern:** One row per domain with `SnapshotKey = 'CURRENT'`. Each refresh deletes child rows and upserts KPI within a transaction. No historical snapshot retention.

**GeneratedAt:** Stored on KPI row at refresh completion. API responses expose this timestamp — not request execution time.

---

## Refresh Workers

| Worker | Source DAL | Aggregator |
| ------ | ---------- | ---------- |
| `RefreshDashboardPiutangSnapshotWorker` | `IPiutangOpenBalanceDal` (`BTR_Piutang.Sisa > 1`) | `DashboardPiutangAggregator` |
| `RefreshDashboardInventorySnapshotWorker` | `IStokBalanceViewDal.ListData()` | `DashboardInventoryAggregator` |
| `RefreshDashboardSalesSnapshotWorker` | `IFakturViewDal` (current month) + `ISalesOmzetTargetDal` | `DashboardSalesFakturAggregator` |
| `RefreshDashboardPurchasingSnapshotWorker` | `IInvoiceViewDal` (current month) | `DashboardPurchasingInvoiceAggregator` |
| `RefreshDashboardPurchasingManagementSnapshotWorker` | `IInvoiceViewDal` (+ `CreateTime`/`LastUpdate`), V1/M15/M19 snapshot DALs | `DashboardPurchasingManagementAggregator` |
| `RefreshDashboardCustomerSnapshotWorker` | `IFakturViewDal`, `ICustomerLastFakturDal`, `IPiutangOpenBalanceDal`, `ICustomerDal`, `ICustomerOmzetHistoryDal`, `ICustomerPelunasanSummaryDal`, `ICustomerPaymentBehaviorDal` | `DashboardCustomerAggregator`, `DashboardCustomerRiskForecastAggregator` |
| `RefreshDashboardSalesmanSnapshotWorker` | `IFakturViewDal`, `IPiutangOpenBalanceWithSalesmanDal`, `ICustomerLastFakturDal.ListLastFakturWithSalesmanByCustomer()`, `ISalesPersonDal`, `ISalesOmzetTargetDal.ListTargetsForMonth()` | `DashboardSalesmanAggregator` |
| `RefreshDashboardInventoryRiskSnapshotWorker` | `IStokBalanceViewDal`, `IBrgLastFakturDal` | `DashboardInventoryRiskAggregator` (+ `DashboardInventoryItemGroupBuilder`) |
| `RefreshAllDashboardSnapshotsWorker` | Orchestrator | Piutang → Inventory → InventoryRisk → Sales → Purchasing → PurchasingManagement → Customer → Salesman; per-domain failure isolation |

**CLI arguments:**

| Argument | Values | Default |
| -------- | ------ | ------- |
| `--domain` | `All`, `Piutang`, `Inventory`, `InventoryRisk`, `Sales`, `Purchasing`, `PurchasingManagement`, `Customer`, `Salesman` | `All` |
| `--triggered-by` | `Scheduler`, `Manual` | `Scheduler` |

Exit code `0` = success. Logs: `{worker-folder}/logs/btr-portal-worker-{date}.log`.

**Scheduled cadence (Task Scheduler — separate job per domain):**

| Domain | Interval |
| ------ | -------- |
| Piutang | 15 minutes |
| Sales | 30 minutes |
| Purchasing | 30 minutes |
| PurchasingManagement | 30 minutes |
| Customer | 30 minutes |
| Salesman | 30 minutes |
| Inventory | 60 minutes |
| InventoryRisk | 60 minutes |

---

## Application Structure

```text
btr.application/ReportingContext/
├── DashboardSnapshotAgg/
│   ├── Contracts/          IDashboard*SnapshotDal, IPiutangOpenBalanceDal, IDashboardSnapshotRefreshLogDal
│   ├── Services/           DashboardPiutangAggregator, DashboardInventoryAggregator, DashboardInventoryRiskAggregator, DashboardInventoryItemGroupBuilder, DashboardSalesFakturAggregator, DashboardPurchasingInvoiceAggregator, DashboardPurchasingManagementAggregator, DashboardCustomerAggregator, DashboardSalesmanAggregator
│   ├── Models/             Aggregate result DTOs
│   └── UseCases/           RefreshDashboard*SnapshotWorker, RefreshAllDashboardSnapshotsWorker
├── DashboardOverviewAgg/   GetDashboardOverviewQuery, IDashboardOverviewDal
├── DashboardSalesAgg/      DashboardSalesDal (read facade)
├── DashboardPiutangAgg/    DashboardPiutangDal (read facade)
├── DashboardInventoryAgg/  DashboardInventoryDal (read facade)
├── DashboardPurchasingAgg/ DashboardPurchasingDal (read facade)
├── DashboardCustomerAgg/   DashboardCustomerDal (read facade)
├── DashboardSalesmanAgg/   DashboardSalesmanDal (read facade)
└── DashboardInventoryRiskAgg/ DashboardInventoryRiskDal (read facade)

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
| `BTRPD_PiutangKpi` | `TotalPiutang`, `TotalCustomer`, `OverdueCustomer`, `GeneratedAt` |
| `BTRPD_InventoryKpi` | `TotalInventoryValue`, `TotalItem`, `GeneratedAt` |
| `BTRPD_SalesKpi` | `PeriodYear`, `PeriodMonth`, omzet/faktur/customer/target/achievement fields, `PipelineOmzet` (= 0) |
| `BTRPD_SalesForecastKpi` | Forecast KPIs: projected sales, required daily, confidence, risk band, business date metadata (M26) |
| `BTRPD_CashFlowForecastKpi` | Cash flow forecast KPIs: expected cash, collection forecast %, required daily, confidence, risk band (M27) |
| `BTRPD_InventoryForecastKpi` | Inventory forecast KPIs: projected value, DOS, health score, scenario bands, confidence (M28) |
| `BTRPD_PurchasingKpi` | `GrandTotalPurchase`, `TotalInvoice`, `PendingPostingInvoiceCount`, `PeriodYear`, `PeriodMonth` |

### Dimensional tables (Layer B)

| Table | Content |
| ----- | ------- |
| `BTRPD_PiutangAging` | 5 aging buckets |
| `BTRPD_PiutangTopCustomer` | Top 10 customers |
| `BTRPD_InventoryBreakdown` | Category/supplier rows with `IsTop10` flag |
| `BTRPD_SalesWeekTrend` | Weekly Faktur totals |
| `BTRPD_SalesDailyPace` | Daily Faktur pace buckets for forecast chart (M26) |
| `BTRPD_CashFlowDailyPace` | Daily cash collection pace for forecast chart (M27) |
| `BTRPD_CashFlowRecoveryTrend` | Cumulative collections vs billing by day (M27) |
| `BTRPD_CashFlowCollectionRisk` | Top collection risk rows for forecast dashboard (M27) |
| `BTRPD_InventoryForecastDailyConsumption` | Daily company consumption + ADC reference (M28) |
| `BTRPD_InventoryForecastLevel` | Projected inventory value by horizon day (M28) |
| `BTRPD_InventoryForecastRisk` | Top inventory forecast risk rows (M28) |
| `BTRPD_InventoryForecastRecommendation` | Top purchase recommendation rows (M28) |
| `BTRPD_SalesTopSalesman` | Top 10 salespeople |
| `BTRPD_PurchasingWeekTrend` | Weekly purchase totals |
| `BTRPD_PurchasingPostingStatus` | `SUDAH` / `BELUM` purchase value buckets |
| `BTRPD_PurchasingTopPrincipal` | Top 10 principals by purchase amount |

### Customer tables (M17 — dedicated cross-domain domain)

| Table | Content |
| ----- | ------- |
| `BTRPD_CustomerKpi` | Attention card counts, concentration %, period metadata |
| `BTRPD_CustomerTopOmzet` | Top 10 customers by current-month omzet |
| `BTRPD_CustomerTopPiutang` | Top 10 customers by all-time open balance |
| `BTRPD_CustomerAttention` | Attention list rows (customer × signal) |
| `BTRPD_CustomerSegmentation` | Klasifikasi, Wilayah, Active/Dormant counts |
| `BTRPD_CustomerRiskForecastKpi` | Customer risk forecast KPIs: elevated risk receivable, portfolio health, confidence (M29) |

Customer worker reads **source DALs** at refresh — not Sales/Piutang snapshot tables.

**M29 child tables:** `BTRPD_CustomerRiskForecastDist`, `BTRPD_CustomerRiskForecastWilayah`, `BTRPD_CustomerRiskForecastSignalMix`, `BTRPD_CustomerRiskForecastCustomer`, `BTRPD_CustomerRiskForecastAttention`, `BTRPD_CustomerRiskForecastRecommendation`.

### Salesman tables (M18 — dedicated cross-domain domain)

| Table | Content |
| ----- | ------- |
| `BTRPD_SalesmanKpi` | Headline KPIs, attention card counts, concentration %, period metadata |
| `BTRPD_SalesmanTopOmzet` | Top 10 salesmen by current-month omzet |
| `BTRPD_SalesmanTopAchievement` | Top 10 salesmen by achievement % |
| `BTRPD_SalesmanTopPiutang` | Top 10 salesmen by all-time open balance |
| `BTRPD_SalesmanAttention` | Attention list rows (salesman × signal) |
| `BTRPD_SalesmanSegmentation` | Wilayah, Segment, Active/Inactive counts |

Salesman worker reads **source DALs** at refresh — not Sales/Piutang/Customer snapshot tables. Child row ID prefix: **PDS**.

### Inventory Risk tables (M19 — dedicated domain)

| Table | Content |
| ----- | ------- |
| `BTRPD_InventoryRiskKpi` | Headline KPIs, at-risk %, `RequiresAttention` |
| `BTRPD_InventoryRiskAging` | Four aging buckets (Active, Slow, Dead, Never Sold) |
| `BTRPD_InventoryRiskAttention` | Attention list rows (item × signal) |
| `BTRPD_InventoryRiskTopDead` | Top 10 dead stock by value |
| `BTRPD_InventoryRiskTopSlow` | Top 10 slow moving by value |
| `BTRPD_InventoryRiskBreakdown` | Category/Supplier at-risk exposure (Top 10 each) |

Inventory Risk worker reads **source DALs** at refresh — not M15 Inventory snapshot tables. Child row ID prefix: **PDIR**.

### Purchasing Management tables (M21 — dedicated cross-domain domain)

| Table | Content |
| ----- | ------- |
| `BTRPD_PurchasingManagementKpi` | Qualified backlog, concentration %, compound counts, inactivity flag |
| `BTRPD_PurchasingManagementAttention` | Attention list rows (principal × signal) |
| `BTRPD_PurchasingManagementTopPrincipal` | Top 10 MTD purchase with % and cross-domain inventory/at-risk columns |

Purchasing Management worker reads extended `IInvoiceViewDal` plus V1 purchasing, M15 inventory, and M19 inventory-risk snapshots at refresh — does not duplicate inventory aggregation SQL. V1 `BTRPD_Purchasing*` tables remain unchanged.

### Supporting objects

| Object | Purpose |
| ------ | ------- |
| `BTRPD_RefreshLog` | Per-attempt audit (domain, status, duration, error, trigger) |
| `IX_BTR_Piutang_OpenBalance` | Filtered index `WHERE Sisa > 1` — accelerates Piutang refresh |
| `BTR_ParamNo_PortalDashboard.sql` | ID prefixes: `PDR`, `PDA`, `PDT`, `PDB`, `PDW`, `PDS`, `PDP`, `PDG` (Purchasing week trend, posting status) |

SQL definitions: `btr.sql/Tables/ReportingContext/BTRPD_*.sql`

---

## API Endpoints

| Endpoint | Auth | Data source | Notes |
| -------- | ---- | ----------- | ----- |
| `GET /api/dashboard/overview` | JWT | Layer A KPI tables only | Home page — fast path |
| `GET /api/dashboard/sales` | JWT | Layer A + B | Full sales analytics |
| `GET /api/dashboard/piutang` | JWT | Layer A + B | Full piutang analytics |
| `GET /api/dashboard/inventory` | JWT | Layer A + B | Full inventory analytics |
| `GET /api/dashboard/purchasing` | JWT | Layer A + B | Full purchasing analytics |
| `GET /api/dashboard/customers` | JWT | Customer snapshot (5 tables) | Customer Analytics (M17) |
| `GET /api/dashboard/salesmen` | JWT | Salesman snapshot (6 tables) | Salesman Performance (M18) |
| `GET /api/dashboard/inventory-risk` | JWT | Inventory Risk snapshot (6 tables) | Slow Moving & Dead Stock (M19) |
| `POST /api/admin/dashboard/refresh` | JWT | Triggers workers synchronously | IIS timeout risk for `--domain All` |
| `GET /api/health/dashboard-snapshots` | None | `BTRPD_RefreshLog` | `unknown` / `ok` / `refreshing` / `degraded` |

**Empty snapshot behavior:**

- Domain detail endpoints (Sales, Piutang, Inventory, Purchasing) → HTTP 503 (`DashboardSnapshotUnavailableException`)
- Customer Analytics → `IsAvailable = false` (graceful; navigation links retained)
- Salesman Performance → `IsAvailable = false` (graceful; navigation links retained)
- Inventory Risk → `IsAvailable = false` (graceful; navigation links retained)
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
    "InventoryRiskIntervalMinutes": 60,
    "SalesIntervalMinutes": 30,
    "PurchasingIntervalMinutes": 30,
    "PurchasingManagementIntervalMinutes": 30,
    "PurchasingQualifiedBacklogDays": 3,
    "CustomerIntervalMinutes": 30,
    "SalesmanIntervalMinutes": 30
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
| Shared inventory item groups | `.../DashboardInventoryItemGroupBuilder.cs` |
| Inventory Risk aggregator | `.../DashboardInventoryRiskAggregator.cs` |
| Item last Faktur DAL | `btr.infrastructure/SalesContext/FakturInfoAgg/BrgLastFakturDal.cs` |
| Purchasing aggregator | `.../DashboardPurchasingInvoiceAggregator.cs` |
| Purchasing Management aggregator | `.../DashboardPurchasingManagementAggregator.cs` |
| Purchasing Management worker | `.../RefreshDashboardPurchasingManagementSnapshotWorker.cs` |
| Customer aggregator | `.../DashboardCustomerAggregator.cs` |
| Salesman aggregator | `.../DashboardSalesmanAggregator.cs` |
| Salesman key resolver | `.../DashboardSalesmanKeyResolver.cs` |
| Piutang with salesman DAL | `btr.infrastructure/ReportingContext/DashboardSnapshotAgg/PiutangOpenBalanceWithSalesmanDal.cs` |
| Customer last Faktur DAL | `btr.infrastructure/SalesContext/FakturInfoAgg/CustomerLastFakturDal.cs` |
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
| 8 | Customer and Salesman workers read source DALs — do not compose from domain snapshot tables |
| 9 | Do not modify `DashboardSalesFakturAggregator` or `BTRPD_SalesTopSalesman` for salesman performance |
| 10 | Inventory Risk worker reads source DALs — do not derive risk metrics from M15 Inventory snapshot |
| 11 | M15 and M19 share `DashboardInventoryItemGroupBuilder` — prevent denominator drift |
| 12 | Do not modify `DashboardInventoryAggregator` or `BTRPD_Inventory*` for inventory risk |

---

## Deferred (Not Implemented)

| Item | Rationale |
| ---- | --------- |
| Layer C piutang open fact table | Shadow reconciliation passed without persisted row-level fact |
| Historical monthly snapshot retention | Product decision — `CURRENT` row only |
| BTR Desktop manual refresh trigger | Worker CLI + portal API cover operations |
| Event-driven refresh hooks | Extension point after pelunasan / stock balance — future |
