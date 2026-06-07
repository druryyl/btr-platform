# BTR Portal — Architecture

**Audience:** Developers, Architects, Future Agents  
**Purpose:** Describe how BTR Portal is built and the conventions to follow when extending it.

**Related permanent docs:** [Domain (WHY)](./btr-portal-domain.md) · [Operational (HOW)](./btr-portal-operational.md) · [Materialized dashboards](../materialized-dashboard/materialized-dashboard-architecture.md) · [Extraction report M1–M15](./knowledge-extraction-report-m1-m15.md)

For business definitions, see [btr-portal-domain.md](./btr-portal-domain.md).  
For user-facing behavior, see [btr-portal-operational.md](./btr-portal-operational.md).

---

## Architectural Overview

BTR Portal is a three-tier read-only analytics application within the existing BTR Desktop solution.

```text
Vue 3 Portal (btr.portal.web)
        ↓ HTTPS / HTTP (Axios + JWT)
ASP.NET Web API 2 (btr.portal.api, .NET Framework 4.8, IIS)
        ↓ MediatR
Application Layer (btr.application — ReportingContext)
        ↓ IDashboard*Dal / I*ReportDal contracts
Infrastructure Layer (btr.infrastructure — ReportingContext wrappers)
        ↓ Existing Desktop DALs (Dapper)
SQL Server (same database as BTR Desktop)
```

### Technology Stack

| Layer | Technology |
| ----- | ---------- |
| Frontend | Vue 3, TypeScript, Vite, PrimeVue, Pinia, Axios, Chart.js |
| API | ASP.NET Web API 2, .NET Framework 4.8, MediatR, Scrutor DI |
| Application | Existing BTR policies, builders, aggregates |
| Data access | Dapper via existing `*ViewDal` / `*Rpt` DALs |
| Auth | JWT (Bearer token); credentials validated against `BTR_User` |

### Key Constraints

- **ASP.NET Web API 2** (not ASP.NET Core) — direct references to `net48` class libraries.
- **No reference to `btr.distrib`** — WinForms host is not pulled into the API.
- **Same database** — no microservices, no separate reporting database.
- **Read-only** — no business POST/PUT/DELETE for transactional data; auth and admin refresh are the only POST endpoints.

### Solution Projects

| Project | Type | Role |
| ------- | ---- | ---- |
| `btr.portal.api` | ASP.NET Web API 2 (.NET 4.8, IIS) | HTTP API — auth, dashboards, reports, health |
| `btr.portal.web` | Vue 3 SPA (Vite) | Browser UI — static files hosted separately from API |
| `btr.portal.worker` | Console host (.NET 4.8) | Background dashboard snapshot refresh (Task Scheduler) |
| `btr.application` | Class library | MediatR handlers, aggregates, snapshot workers |
| `btr.infrastructure` | Class library | DAL wrappers, snapshot readers/writers |
| `btr.distrib` | WinForms | BTR Desktop — unchanged; not referenced by portal |

All portal projects live under `src/j05-btr-distrib/` in `j05-btr-distrib.sln` (nested under the `backend` solution folder).

---

## ReportingContext

All portal reporting features live under `ReportingContext` in Application and Infrastructure layers.

```text
btr.application/ReportingContext/
├── DashboardOverviewAgg/
├── DashboardSnapshotAgg/          ← workers, aggregators, refresh commands
├── DashboardSalesAgg/
├── DashboardPiutangAgg/
├── DashboardInventoryAgg/
├── SalesReportAgg/
├── PiutangReportAgg/
├── InventoryReportAgg/
└── PurchasingReportAgg/

btr.infrastructure/ReportingContext/
├── DashboardSnapshotAgg/            ← snapshot readers/writers, overview DAL
├── DashboardSalesAgg/DashboardSalesDal.cs
├── DashboardPiutangAgg/DashboardPiutangDal.cs
├── DashboardInventoryAgg/DashboardInventoryDal.cs
├── SalesReportAgg/SalesReportDal.cs
├── PiutangReportAgg/PiutangReportDal.cs
├── InventoryReportAgg/InventoryReportDal.cs
└── PurchasingReportAgg/PurchasingReportDal.cs
```

### Aggregate Structure (per feature)

Each aggregate follows the same vertical slice:

| Layer | Artifact | Responsibility |
| ----- | -------- | -------------- |
| Application | `Contracts/I{Feature}Dal.cs` | Portal-specific DAL contract |
| Application | `Queries/Get{Feature}Query.cs` | MediatR query, handler, response DTOs |
| Infrastructure | `{Feature}Dal.cs` | Wraps existing Desktop DAL; maps to portal DTOs |
| API | `Controllers/.../{Feature}Controller.cs` | Thin controller → `IMediator.Send()` |

Query files contain query, handler, and response types in one file (M3/M8 convention).

### DAL Reuse Strategy

Portal DALs are **wrappers**, not reimplementations:

1. Call existing Desktop DAL (`IFakturViewDal`, `IPiutangSalesWilayahDal`, `IStokBalanceViewDal`, `IInvoiceViewDal`, `ISalesOmzetDal`, etc.).
2. Apply portal-approved presentation filters (subset of desktop UI filters).
3. Map desktop DTOs to portal response rows.
4. Compute aggregations using existing builders/policies where available.
5. Register portal DALs explicitly in `InfrastructurePortalExtensions`; existing DALs are auto-registered via Scrutor `IListData<,>` scan.

**Never duplicate SQL** already in Desktop DALs. **Never introduce new business calculation policies** when an existing builder or policy applies.

---

## Dashboard Architecture

### Materialized Snapshot Strategy

Dashboard KPIs and charts are **pre-computed** by `btr.portal.worker` and stored in `BTR_PortalDashboard*` tables. Portal API read endpoints SELECT from snapshot tables — live aggregation on the HTTP request path has been removed.

```text
Windows Task Scheduler (per-domain jobs)
        ↓
btr.portal.worker
        ↓
RefreshDashboard{Domain}SnapshotWorker
        ↓
Dashboard{Domain}Aggregator  (shared aggregation rules)
        ↓
SnapshotWriter → BTR_PortalDashboard* tables

Browser → GET /api/dashboard/overview          (home — Layer A KPI only)
Browser → GET /api/dashboard/{sales|piutang|inventory}  (detail — Layer A + B)
        ↓ MediatR → Dashboard*Dal → Dashboard*SnapshotDal → snapshot tables
```

**Snapshot layers:**

| Layer | Tables | Used by |
| ----- | ------ | ------- |
| **A — KPI** | `BTR_PortalDashboard{Sales,Piutang,Inventory}Kpi` | Overview home + detail KPI rows |
| **B — Dimensional** | Week trend, aging, breakdown, Top-N tables | Detail dashboards only |

All snapshots use `SnapshotKey = 'CURRENT'`. `GeneratedAt` on every dashboard response reflects the last successful background refresh, not request execution time.

**Refresh cadence (Task Scheduler):**

| Domain | Interval |
| ------ | -------- |
| Piutang | 15 minutes |
| Sales | 30 minutes |
| Inventory | 60 minutes |

**Operational metadata:** `BTR_PortalDashboardRefreshLog` records each refresh attempt (domain, status, duration, error, trigger source).

Reports (`/api/reports/*`) remain **live queries** — unaffected by snapshot materialization.

### Dashboard API Strategy

| Endpoint | Controller | Data source | Purpose |
| -------- | ---------- | ----------- | ------- |
| `GET /api/dashboard/overview` | `OverviewDashboardController` | Layer A KPI snapshots only | Dashboard home — fast summary cards |
| `GET /api/dashboard/sales` | `SalesDashboardController` | Layer A + B snapshots | Sales detail analytics |
| `GET /api/dashboard/piutang` | `PiutangDashboardController` | Layer A + B snapshots | Piutang detail analytics |
| `GET /api/dashboard/inventory` | `InventoryDashboardController` | Layer A + B snapshots | Inventory detail analytics |
| `POST /api/admin/dashboard/refresh` | `AdminDashboardRefreshController` | Triggers snapshot rebuild | On-demand refresh (sync; IIS timeout risk) |
| `GET /api/health/dashboard-snapshots` | `HealthController` | `BTR_PortalDashboardRefreshLog` | Monitoring — no auth |

Domain detail endpoints were extended additively across M8 and M13–M15; response DTOs unchanged in shape. All dashboard data endpoints require JWT except health.

**Empty snapshot behavior:** Detail endpoints return HTTP 503 when snapshot tables are empty. Overview sets `HasUnavailableDomain = true` when any Layer A row is missing.

### Dashboard Routing Strategy (Frontend)

| Route | View | API Call |
| ----- | ---- | -------- |
| `/dashboard` | `DashboardHomeView` | `GET /api/dashboard/overview` (single call) |
| `/dashboard/sales` | `SalesDashboardView` | `GET /api/dashboard/sales` |
| `/dashboard/piutang` | `PiutangDashboardView` | `GET /api/dashboard/piutang` |
| `/dashboard/inventory` | `InventoryDashboardView` | `GET /api/dashboard/inventory` |

Home shows summary KPI cards with per-domain `GeneratedAt` timestamps and links to detail pages. Detail pages consume the full API response (including chart/ranking sections not shown on home).

### Dashboard DTO Patterns

`Dashboard{Domain}Response` in `GetDashboard{Domain}Query.cs`:

| Field | Convention |
| ----- | ---------- |
| `GeneratedAt` | Server timestamp on every response |
| Existing KPI fields | Semantics unchanged when extended (backward compatible) |
| New sections | Additive only — M13–M15 pattern |

**Sales response sections:**

| Section | DTO | Introduced |
| ------- | --- | ---------- |
| Core KPIs | `TotalOmzet`, `TotalFaktur`, `TotalCustomer` | M4 |
| Trend | `CompletedOmzet`, `PipelineOmzet`, `WeeklyTrend[]` | M8 |
| Target | `TotalTarget`, `TotalAchievement`, `AchievementPercent` | M13 |
| Chart | `TargetVsAchievement` | M13 |
| Ranking | `TopSalesmanRanking[]` | M13 |

**Piutang response sections:**

| Section | DTO | Introduced |
| ------- | --- | ---------- |
| Core KPIs | `TotalPiutang`, `TotalCustomer` | M5 |
| Overdue | `OverdueCustomer` | M14 |
| Aging | `AgingBuckets[]` | M14 |
| Ranking | `TopCustomers[]` | M14 |

**Inventory response sections:**

| Section | DTO | Introduced |
| ------- | --- | ---------- |
| Core KPIs | `TotalInventoryValue`, `TotalItem` | M6 |
| Breakdown | `CategoryBreakdown[]`, `SupplierBreakdown[]` | M15 |
| Ranking | `TopCategories[]`, `TopSuppliers[]` | M15 |

### KPI Aggregation Patterns

Aggregation runs in `Dashboard{Domain}Aggregator` during snapshot refresh (worker path). Read DALs map snapshot rows to response DTOs.

| Domain | Aggregation Source | Key Logic |
| ------ | ------------------- | --------- |
| Sales | `IFakturViewDal` (Faktur list) | Current month non-void Fakturs; `SUM(GrandTotal)` for omzet KPIs; `PipelineOmzet` always `0` |
| Sales target | `ISalesOmzetTargetDal.SumTargetAmountForMonth()` | Sum all target rows for month — not single-rep resolver |
| Sales achievement % | `SalesOmzetChartAchievementPolicy.ComputePercent()` | Applied to aggregate totals |
| Sales weekly trend | Faktur `GrandTotal` grouped by calendar week | 7-day segments from month start |
| Sales ranking | `SUM(GrandTotal)` per `SalesPersonName`, top 10 descending | Completed Omzet = invoiced omzet |
| Piutang | `IPiutangOpenBalanceDal` (`Sisa > 1`) + inline aggregation | Open balances only; `KurangBayar > 1`; customer key resolution |
| Piutang aging | Inline bucket assignment | `DaysOverdue = Today − JatuhTempo`; five inclusive buckets |
| Inventory | `IStokBalanceViewDal` + BrgId-first pipeline | Exclude In-Transit; group by `BrgId`; sum Qty and Hpp×Qty |
| Inventory rollup | Category/supplier grouping after BrgId step | Blank → `"Unknown"` |

All chart and ranking values are computed **server-side** in the DAL. Frontend never sums rows to derive KPIs.

### Ranking Patterns

Shared across M13–M15:

| Rule | Value |
| ---- | ----- |
| Top N | 10 |
| Rank | 1-based, assigned after sort |
| Sort | Descending by primary metric |
| Tie-break | Ascending by display name (`OrdinalIgnoreCase`) |
| Pre-sorted | API returns final order; frontend does not re-sort |

| DTO | Properties |
| --- | ---------- |
| `DashboardSalesRankingItem` | `Rank`, `SalesPersonName`, `CompletedOmzet` |
| `DashboardPiutangTopCustomer` | `Rank`, `CustomerName`, `OutstandingBalance` |
| `DashboardInventoryRankingItem` | `Rank`, `Name`, `InventoryValue` |

### Chart Patterns

PrimeVue `Chart` + Chart.js on detail pages.

| Chart | Type | Data DTO | Config |
| ----- | ---- | -------- | ------ |
| Target vs Achievement | `bar` | `DashboardSalesTargetVsAchievement` | Two categories: Target, Achievement |
| Chart | Weekly Trend | `line` | `DashboardSalesWeekTrendItem[]` | X: `WeekLabel`; Y: Faktur `GrandTotal` per week |
| Aging Distribution | `pie` | `DashboardPiutangAgingBucket[]` | 5 buckets; `SortOrder` for stable sequence |
| Category / Supplier | `bar` (`indexAxis: 'y'`) | `DashboardInventoryBreakdownItem[]` | Horizontal bars |

Empty-state handling: frontend shows message when all values are zero.

---

## Report Architecture

### Report API Strategy

| Endpoint | Controller | Desktop Source |
| -------- | ---------- | -------------- |
| `GET /api/reports/sales` | `SalesReportController` | `IFakturViewDal` (FakturInfoForm) |
| `GET /api/reports/piutang` | `PiutangReportController` | `IPiutangSalesWilayahDal` (FF1) |
| `GET /api/reports/inventory` | `InventoryReportController` | `IStokBalanceViewDal` (IF1) |
| `GET /api/reports/purchasing` | `PurchasingReportController` | `IInvoiceViewDal` (PF1) |

No query parameters in V1. JWT required.

### Report Routing Strategy (Frontend)

| Route | View | Store |
| ----- | ---- | ----- |
| `/reports/sales` | `SalesReportView` | `salesReportStore` |
| `/reports/piutang` | `PiutangReportView` | `piutangReportStore` |
| `/reports/inventory` | `InventoryReportView` | `inventoryReportStore` |
| `/reports/purchasing` | `PurchasingReportView` | `purchasingReportStore` |

### Report DTO Patterns

```text
{Domain}ReportResponse
├── PeriodFrom / PeriodTo     (omit for inventory snapshot)
├── GeneratedAt
├── Summary                   (M10–M12; absent in M9 Sales)
│   └── {Domain}ReportSummary
└── Rows[]
    └── {Domain}ReportRow     (presentation subset of desktop DTO)
```

Row DTOs expose only approved columns — never the full desktop DTO surface.

### DataTable Conventions

| Setting | Value |
| ------- | ----- |
| Component | PrimeVue `DataTable` + `Column` |
| Pagination | Client-side |
| Default page size | 25 |
| Page size options | 10, 25, 50, 100 |
| Sorting | Column sort enabled (`removable-sort`) |
| Loading | Bound to store `loading` |
| Empty state | Custom `#empty` template |
| Refresh | `Button` triggers `loadReport()` |

### Footer Summary Conventions

`ReportSummaryBar.vue` renders summary below the DataTable:

```text
DataTable
ReportSummaryBar    ← values from API Summary only
Generated-at timestamp
```

**Critical rule:** Footer values come from API `Summary` — never computed client-side from `Rows`. Essential for Piutang and Inventory where footer aggregation differs from visible row sums.

| Report | Summary Fields |
| ------ | -------------- |
| Sales | None |
| Piutang | `TotalPiutang`, `TotalCustomer` |
| Inventory | `TotalInventoryValue`, `TotalItem` |
| Purchasing | `GrandTotalPurchase`, `TotalInvoice` |

---

## Backend Conventions

### MediatR Usage

```text
Controller → IMediator.Send(Get{Feature}Query) → Get{Feature}Handler → I{Feature}Dal
```

- Controllers depend on `IMediator` only — never on DAL interfaces.
- Handlers depend on Application contracts (`IDashboard*Dal`, `I*ReportDal`).
- One query + handler per feature, co-located with response DTOs.

### API Conventions

| Convention | Detail |
| ---------- | ------ |
| Route prefix | `api/dashboard/{domain}`, `api/dashboard/overview`, `api/reports/{domain}`, `api/auth`, `api/health`, `api/admin/dashboard` |
| HTTP verbs | `GET` for data; `POST` for login and admin dashboard refresh |
| Auth | `[Authorize]` on all data endpoints; `JwtAuthenticationFilter` validates Bearer token |
| Controller shape | Single action per controller for dashboard/report endpoints |
| DI registration | Portal DALs in `InfrastructurePortalExtensions`; controllers in `PortalPresentationExtensions` |

### Response Conventions

All endpoints return the standard envelope:

```json
{
  "Status": "success",
  "Code": 200,
  "Message": null,
  "Data": { }
}
```

Errors are handled by `GlobalExceptionFilter`. Anonymous data requests return HTTP 401.

### Portal API Project Layout

```text
btr.portal.api/
├── App_Start/WebApiConfig.cs
├── Configurations/
│   ├── DependencyConfig.cs
│   ├── ApplicationPortalExtensions.cs
│   ├── InfrastructurePortalExtensions.cs
│   └── PortalPresentationExtensions.cs
├── Controllers/
│   ├── AuthController.cs
│   ├── HealthController.cs
│   ├── Admin/
│   │   └── AdminDashboardRefreshController.cs
│   ├── Dashboard/
│   │   ├── OverviewDashboardController.cs
│   │   ├── SalesDashboardController.cs
│   │   ├── PiutangDashboardController.cs
│   │   └── InventoryDashboardController.cs
│   └── Reports/
├── Filters/
│   ├── GlobalExceptionFilter.cs
│   └── JwtAuthenticationFilter.cs
├── Infrastructure/
│   ├── JwtOptions.cs
│   ├── JwtTokenService.cs
│   └── ServiceProviderDependencyResolver.cs
└── Models/
    ├── ApiResponse.cs
    ├── LoginRequest.cs
    └── LoginResponse.cs
```

---

## Frontend Conventions

### Vue Structure

```text
btr.portal.web/src/
├── api/           # Axios clients (httpClient, authApi, dashboardApi, reportsApi)
├── stores/        # Pinia stores (authStore, dashboardStore, *ReportStore)
├── views/
│   ├── auth/      # LoginView
│   ├── dashboard/ # Home + detail views
│   └── reports/   # Report views
├── components/
│   ├── KpiCard.vue
│   ├── dashboard/ # Detail layout, charts, ranking table
│   └── reports/   # ReportSummaryBar
├── layouts/       # MainLayout (header, sidebar, logout)
├── models/        # TypeScript DTOs (api, auth, dashboard, reports)
├── router/        # Routes + navigation guard
└── services/      # authStorage, formatters (currency, date, percent)
```

### Pinia Usage

| Store | Responsibility |
| ----- | -------------- |
| `authStore` | Login, logout, JWT hydration from localStorage |
| `dashboardStore` | `loadDashboard()` → overview endpoint (home); `loadSales()`, `loadPiutang()`, `loadInventory()` (detail pages) |
| `{domain}ReportStore` | `report`, `loading`, `error`; `loadReport()`, `reset()` |

One store per report. No premature module stores beyond auth, dashboard, and reports.

### Dashboard Layout Conventions

| Component | Purpose |
| --------- | ------- |
| `KpiCard.vue` | Reusable card shell for home summary metrics |
| `DashboardDetailLayout.vue` | Shared detail page header, refresh, error slot |
| `Top10RankingTable.vue` | Generic ranked DataTable (salesman, customer, category, supplier) |
| `TargetVsAchievementChart.vue` | M13 company bar chart |
| `WeeklyTrendChart.vue` | M8/M13 line chart |
| `AgingPieChart.vue` | M14 pie chart |
| `InventoryHorizontalBarChart.vue` | M15 horizontal bar (category + supplier) |

Detail page section order is fixed per domain (see operational doc).

### Report Layout Conventions

Each report view follows:

```text
Page header (title + period label)
Refresh button
PrimeVue Card
  └── DataTable
ReportSummaryBar (if applicable)
Generated-at timestamp
Error Message (on failure)
```

### Shared Component Conventions

| Concern | Convention |
| ------- | ---------- |
| API types | PascalCase JSON matching backend (`TotalOmzet`, not `totalOmzet`) |
| Currency | `formatCurrency()` — IDR |
| Dates | `formatDate()`, `formatDateTime()` |
| Percent | `formatPercent()` |
| HTTP | Axios interceptor attaches Bearer token; 401 clears session |
| Auth persistence | `localStorage` keys: `btr_portal_token`, `btr_portal_expires_at`, `btr_portal_user` |
| CORS | API allows frontend origin via `appsettings.json` `Cors:AllowedOrigins` |
| Dev API URL | Frontend `.env`: `VITE_API_BASE_URL=http://localhost:5050` (IIS Express default) |

---

## Connection Configuration

Portal and Desktop share DAL code but resolve SQL Server settings differently.

```text
Desktop (btr.distrib)                    Portal (btr.portal.api / worker)
        ↓                                        ↓
RegistryConnectionSettingProvider      JsonConnectionSettingProvider
  (HKCU\DrurySoftware\BTRApp)            (appsettings.json only — no registry)
        ↓                                        ↓
        └──────── ConnectionStringFactory ───────┘
                          ↓
              ConnStringHelper.Get()  →  all DALs unchanged
```

| Provider | Host | Source |
| -------- | ---- | ------ |
| `RegistryConnectionSettingProvider` | `btr.distrib` | Registry with appsettings fallback |
| `JsonConnectionSettingProvider` | `btr.portal.api`, `btr.portal.worker` | JSON config only |

**Portal config load order** (`Global.asax.cs` / worker startup):

```text
appsettings.json  →  appsettings.{Environment.MachineName}.json (optional override)
```

Per-office IIS deployment requires `appsettings.{MACHINE_NAME}.json` beside the published site with real SQL values:

```json
{
  "Database": {
    "ServerName": "OFFICE-SQL01\\SQLEXPRESS",
    "DbName": "btr",
    "IsTest": false
  }
}
```

Property names: `ServerName`, `DbName`, `IsTest` (via `DatabaseOptions`). SQL credentials (`btrLogin`) are embedded in `ConnectionStringFactory` — unchanged from Desktop. App pool identity must have network access to SQL Server.

**Also configure per server:** `Jwt` section (strong secret, issuer, audience, expiry) and `Cors:AllowedOrigins` (Vue app URL).

---

## Deployment Topology

```text
IIS site: /btr-portal-api     →  btr.portal.api (ASP.NET Web API 2)
IIS site: /btr-portal         →  btr.portal.web static build (dist/)
Scheduled tasks               →  btr.portal.worker.exe (separate folder)
SQL Server                    →  same database as BTR Desktop
```

| Component | Publish method | Output / path |
| --------- | -------------- | ------------- |
| API | Visual Studio Publish → **FolderProfile** | `src/j05-btr-distrib/publish/btr-portal-api/` |
| Frontend | `npm run build` in `btr.portal.web` | `dist/` → copy to IIS static site |
| Worker | Build Release, copy bin output | Dedicated folder with `appsettings.json`, `NLog.config`, DLLs |

**IIS app pool (API):** .NET CLR v4.0, Integrated pipeline. Ensure `logs/` folder exists with write permission for the app pool identity.

**Post-deploy smoke tests:**

```text
GET  /api/health                              → 200
GET  /api/health/dashboard-snapshots          → 200 (status may be unknown until first refresh)
POST /api/auth/login                          → 200 with JWT (valid BTR user)
GET  /api/dashboard/overview                  → 401 without token; 200 with token
GET  /api/dashboard/sales                     → 503 if snapshots empty; 200 after worker run
```

Run initial snapshot backfill before go-live: `btr.portal.worker.exe --domain All --triggered-by Manual`

---

## Architectural Rules

Rules that must be preserved when extending the portal:

| # | Rule |
| - | ---- |
| 1 | Reuse existing BTR reporting logic — DALs, builders, policies, aggregates |
| 2 | Do not duplicate business calculations already in Desktop |
| 3 | All new reporting features go in `ReportingContext` only |
| 4 | Portal remains read-only — no transactional endpoints |
| 5 | Keep controllers thin — MediatR only |
| 6 | Avoid new SQL when equivalent reporting logic exists |
| 7 | Dashboard metrics must be traceable to report data (Piutang, Inventory) |
| 8 | Extend existing dashboard GET endpoints — do not add parallel routes |
| 9 | Footer/report summaries computed server-side — never client-summed |
| 10 | Presentation mapping only in portal DALs — business rules stay in Desktop DALs |
| 11 | Register portal-specific DALs explicitly; rely on Scrutor for existing DALs |
| 12 | Do not reference `btr.distrib` from the API project |
| 13 | Top N rankings = 10 unless product explicitly changes |
| 14 | BrgId-first grouping for all inventory value calculations |
| 15 | Piutang open-balance filter `KurangBayar > 1` is fixed — no toggle |
| 16 | Dashboard read path uses snapshot tables only — no live aggregation fallback |
| 17 | Snapshot refresh runs in `btr.portal.worker`, not inside IIS app pool |
| 18 | Portal SQL config via JSON only — never registry under IIS app pool identity |

### Dashboard–Report Traceability Matrix

| Dashboard KPI | Report | Reconciliation |
| ------------- | ------ | -------------- |
| Total Piutang | Piutang Report footer | `Sum(KurangBayar)` where `> 1` |
| Total Customer (Piutang) | Piutang Report footer | Distinct customer key count |
| Total Inventory Value | Inventory Report footer | BrgId-grouped `Sum(Hpp × Qty)` excl. In-Transit |
| Total Item | Inventory Report footer | Count BrgId where aggregated Qty > 0 |
| Sales Total Omzet / Achievement | Sales Report | Sum of `GrandTotal` from report rows (same month, same Faktur source) |

### Testing Convention

Portal wrapper DALs have unit tests in `btr.test/ReportingContext/` verifying filters, summary totals, customer-key logic, and reconciliation expectations.
