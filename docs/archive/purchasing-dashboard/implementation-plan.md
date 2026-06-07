# Implementation Plan: Purchasing Dashboard V1

## Document Status

| Field | Value |
| --- | --- |
| Initiative | Purchasing Dashboard V1 |
| Authoritative requirements | `docs/work/purchasing-dashboard/analysis-report.md` (Sections 6 and 10) |
| Related features | `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md` |
| Reference pattern | Sales Dashboard (M13) + Purchasing Report (M12) + Materialized Dashboard Data |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |
| Status | Ready for re-review |
| Product decisions | Confirmed — see analysis-report Section 10 |

---

## 1. Goal

Add a **Purchasing Dashboard** as the fourth portal analytics domain, giving management at-a-glance visibility into current-month purchasing activity with mandatory traceability to the existing Purchasing Report footer KPIs.

**Primary outcomes:**

- Dashboard home (`/dashboard`) shows a **4th Purchasing summary card** with Grand Total Purchase and Total Invoice.
- Detail dashboard (`/dashboard/purchasing`) shows KPI row, weekly trend, posting-status breakdown, and Top 10 Principal ranking.
- `GET /api/dashboard/purchasing` reads from **materialized snapshot tables** (same architecture as Sales, Piutang, Inventory).
- `Grand Total Purchase` and `Total Invoice` on dashboard **match exactly** Purchasing Report footer totals for the same invoice source and period.
- Snapshot refresh runs on a **30-minute cadence** via `btr.portal.worker`.

**Explicitly out of scope (V1):**

- Warehouse breakdown, pending posting value KPI, Retur Beli analytics, budget vs actual.
- Drill-down, custom date range, export.
- Changes to BTR Desktop, Purchasing Report route/behavior, or transactional write capability.
- Report pages remain live queries (unchanged).

---

## 2. Architecture Overview

### 2.1 Target topology

```text
Windows Task Scheduler (30 min)
    ↓
btr.portal.worker --domain Purchasing
    ↓
RefreshDashboardPurchasingSnapshotWorker
    ↓
DashboardPurchasingInvoiceAggregator
    ↓ (IInvoiceViewDal.ListData(current month))
DashboardPurchasingSnapshotDal.ReplaceCurrent()
    ↓
BTR_PortalDashboardPurchasing* tables

Browser → GET /api/dashboard/purchasing        (detail — Layer A + B)
Browser → GET /api/dashboard/overview            (home — Layer A only, 4 domains)
    ↓ MediatR
GetDashboardPurchasingHandler / GetDashboardOverviewHandler
    ↓ IDashboardPurchasingDal / IDashboardOverviewDal
Snapshot read DALs
    ↓
BTR_PortalDashboardPurchasing* tables

Purchasing Report (unchanged — live query)
    ↓
PurchasingReportDal → IInvoiceViewDal.ListData(same period)
```

### 2.2 Design principles

| Principle | Application |
| --- | --- |
| Preserve report behavior | Dashboard and report share `IInvoiceViewDal` + current-month period; void exclusion remains in SQL |
| No cross-aggregate coupling | `DashboardPurchasingInvoiceAggregator` is independent of `PurchasingReportDal`; duplicate period/filter logic inline (portal precedent) |
| Same snapshot model | `SnapshotKey = 'CURRENT'` KPI upsert + delete-and-replace child rows per refresh |
| Reuse proven UX | Mirror `SalesDashboardView.vue` layout; reuse `WeeklyTrendChart`, `Top10RankingTable` |
| Principal terminology | UI labels say **Principal**; API/DAL may use `SupplierName` internally, mapped at response boundary |

### 2.3 Architecture decisions

| Decision | Choice | Rationale |
| --- | --- | --- |
| Data source | `IInvoiceViewDal.ListData(periode)` | Same as Purchasing Report; void/posting rules already enforced in `InvoiceViewDal` SQL |
| Aggregation location | `DashboardPurchasingInvoiceAggregator` in `DashboardSnapshotAgg/Services` | Consistent with Sales/Piutang/Inventory; shared by refresh worker and verification tests |
| Week bucketing | Reuse `SalesOmzetChartWeekGrouper` | Calendar-week buckets within current month already proven for Sales weekly trend |
| Posting breakdown storage | Dedicated `BTR_PortalDashboardPurchasingPostingStatus` table | Mirrors `BTR_PortalDashboardPiutangAging` bucket pattern |
| Top 10 dimension key | Group by trimmed `SupplierName`; blank → `"Unknown"` | Product decision A6; Inventory precedent |
| Pending posting KPI | Count of invoices where `PostingStok = 'BELUM'` | Dashboard-only KPI; not in report footer |
| Overview availability flag | Extend `HasUnavailableDomain` to include Purchasing KPI null | Keeps existing home-page warning behavior |
| Posting pie chart | New `PostingStatusPieChart.vue` (thin copy of `AgingPieChart.vue`) | Aging chart has hardcoded aging colors/title; avoid over-generalizing shared component in V1 |
| Weekly trend chart | Extend `WeeklyTrendChart.vue` with optional `title` and `emptyMessage` props | Defaults preserve Sales behavior; Purchasing passes domain-specific copy |

---

## 3. Impact Analysis

### 3.1 Affected modules

| Layer | Module | Change type |
| --- | --- | --- |
| SQL | `btr.sql` | 4 new snapshot tables; update `btr.sql.sqlproj` |
| Application | `ReportingContext/DashboardSnapshotAgg/` | New aggregator, models, refresh worker, extend `RefreshAllDashboardSnapshotsWorker` and `RefreshDashboardSnapshotsCommand` |
| Application | `ReportingContext/DashboardPurchasingAgg/` (new) | Query, handler, contracts |
| Application | `ReportingContext/DashboardOverviewAgg/` | Extend overview response DTO |
| Application | `DashboardSnapshotOptions` | Add `PurchasingIntervalMinutes = 30` |
| Infrastructure | `ReportingContext/DashboardSnapshotAgg/` | `DashboardPurchasingSnapshotDal`; extend `DashboardOverviewDal` |
| Infrastructure | `ReportingContext/DashboardPurchasingAgg/` (new) | Read DAL |
| API | `btr.portal.api` | New controller; extend health + admin refresh; DI registration |
| Worker | `btr.portal.worker` | Register Purchasing worker; extend CLI domain list |
| Frontend | `btr.portal.web` | Route, view, store, API, models, sidebar, home card |
| Tests | `btr.test` | Aggregator tests, snapshot verification, handler tests, traceability test |
| Docs | Post-delivery | `btr-portal-domain.md`, `materialized-dashboard-domain.md`, runbook |

### 3.2 Unaffected modules

- `PurchasingReportDal`, `PurchasingReportController`, `PurchasingReportView.vue` (behavior unchanged).
- `InvoiceViewDal` SQL (no schema change to `BTR_Invoice`).
- Piutang, Sales, Inventory dashboard read paths (except overview extension).
- BTR Desktop purchase/posting workflows.

### 3.3 Business rule preservation

| Rule | Dashboard | Report | Notes |
| --- | --- | --- | --- |
| Period = current calendar month | Yes | Yes | Use `ITglJamDal.Now` + `Periode` month bounds |
| Void exclusion | Yes (via DAL) | Yes (via DAL) | `VoidDate = '3000-01-01'` in SQL |
| Posting Stok SUDAH/BELUM | Yes | Yes | Derived in SQL from `IsStokPosted` |
| Grand Total Purchase = Sum(GrandTotal) | Yes | Yes | **Must reconcile** |
| Total Invoice = row count | Yes | Yes | **Must reconcile** |
| Retur Beli excluded | Yes | Yes | Same invoice source as report today |

---

## 4. Database Design

Prefix: `BTR_PortalDashboard` — portal-owned read-only analytics artifacts.

Register all scripts in `btr.sql/btr.sql.sqlproj` under `Tables/ReportingContext/`.

### 4.1 Layer A — `BTR_PortalDashboardPurchasingKpi`

One active row per domain (`SnapshotKey = 'CURRENT'`).

| Column | Type | Purpose |
| --- | --- | --- |
| `SnapshotKey` | `VARCHAR(10)` PK | Always `'CURRENT'` |
| `GeneratedAt` | `DATETIME` | Last successful refresh timestamp |
| `PeriodYear` | `INT` | Detect stale snapshot on month rollover |
| `PeriodMonth` | `INT` | Detect stale snapshot on month rollover |
| `GrandTotalPurchase` | `DECIMAL(18,2)` | Sum(GrandTotal) — traces to report footer |
| `TotalInvoice` | `INT` | Invoice count — traces to report footer |
| `PendingPostingInvoiceCount` | `INT` | Count where PostingStok = `'BELUM'` |
| `LastRefreshLogId` | `VARCHAR(13)` | FK to refresh log |

Follow default-constraint pattern from `BTR_PortalDashboardSalesKpi.sql`.

### 4.2 Layer B — `BTR_PortalDashboardPurchasingWeekTrend`

| Column | Type | Purpose |
| --- | --- | --- |
| `PurchasingWeekTrendId` | `VARCHAR(13)` PK | Generated ID (`PDP` counter prefix — see Section 7.4) |
| `SnapshotKey` | `VARCHAR(10)` | `'CURRENT'` |
| `WeekStart` | `DATETIME` | Bucket start |
| `WeekEnd` | `DATETIME` | Bucket end |
| `WeekLabel` | `VARCHAR(30)` | Display label |
| `PurchaseAmount` | `DECIMAL(18,2)` | Sum(GrandTotal) for week |

Index: `IX_BTR_PortalDashboardPurchasingWeekTrend_SnapshotKey_WeekStart`.

### 4.3 Layer B — `BTR_PortalDashboardPurchasingPostingStatus`

Mirror `BTR_PortalDashboardPiutangAging` structure.

| Column | Type | Purpose |
| --- | --- | --- |
| `PurchasingPostingStatusId` | `VARCHAR(13)` PK | Generated ID |
| `SnapshotKey` | `VARCHAR(10)` | `'CURRENT'` |
| `StatusKey` | `VARCHAR(10)` | `SUDAH` or `BELUM` |
| `StatusLabel` | `VARCHAR(50)` | Display label (same as key in V1) |
| `SortOrder` | `INT` | `1` = BELUM, `2` = SUDAH (backlog first) |
| `PurchaseAmount` | `DECIMAL(18,2)` | Sum(GrandTotal) for status |

Unique: `(SnapshotKey, StatusKey)`.

Always persist **both** buckets even when amount is zero (consistent pie chart rendering).

### 4.4 Layer B — `BTR_PortalDashboardPurchasingTopPrincipal`

| Column | Type | Purpose |
| --- | --- | --- |
| `PurchasingTopPrincipalId` | `VARCHAR(13)` PK | Generated ID |
| `SnapshotKey` | `VARCHAR(10)` | `'CURRENT'` |
| `Rank` | `INT` | 1–10 |
| `PrincipalName` | `VARCHAR(100)` | Trimmed supplier name or `"Unknown"` |
| `PurchaseAmount` | `DECIMAL(18,2)` | Sum(GrandTotal) for principal |

Index: `IX_BTR_PortalDashboardPurchasingTopPrincipal_SnapshotKey_Rank`.

### 4.5 Refresh log domain value

`BTR_PortalDashboardRefreshLog.Domain` accepts new value **`Purchasing`**. No schema change required (column is `VARCHAR(20)`).

---

## 5. Backend Implementation

### 5.1 New folder structure

```text
btr.application/ReportingContext/
├── DashboardPurchasingAgg/
│   ├── Contracts/
│   │   └── IDashboardPurchasingDal.cs
│   └── Queries/
│       └── GetDashboardPurchasingQuery.cs
└── DashboardSnapshotAgg/
    ├── Contracts/
    │   └── IDashboardPurchasingSnapshotDal.cs          (new)
    ├── Models/
    │   └── DashboardPurchasingAggregateResult.cs       (new)
    ├── Services/
    │   └── DashboardPurchasingInvoiceAggregator.cs     (new)
    └── UseCases/
        ├── RefreshDashboardPurchasingSnapshotWorker.cs (new)
        ├── RefreshDashboardPurchasingSnapshotRequest.cs
        └── RefreshDashboardPurchasingSnapshotResult.cs

btr.infrastructure/ReportingContext/
├── DashboardPurchasingAgg/
│   └── DashboardPurchasingDal.cs                       (new)
└── DashboardSnapshotAgg/
    └── DashboardPurchasingSnapshotDal.cs               (new)
```

Add all new `.cs` files to respective `.csproj` Compile includes (net48 projects do not auto-glob).

### 5.2 Aggregator — `DashboardPurchasingInvoiceAggregator`

**Input:** `IEnumerable<InvoiceView> rows`, `Periode periode`, `DateTime generatedAt`

**Output:** `DashboardPurchasingAggregateResult`

**Algorithm:**

```text
1. list = rows.ToList()   (void already excluded by DAL)
2. GrandTotalPurchase = list.Sum(r => r.GrandTotal)
3. TotalInvoice = list.Count
4. PendingPostingInvoiceCount = list.Count(r => r.PostingStok == "BELUM")
5. WeekTrend = SalesOmzetChartWeekGrouper buckets → Sum(GrandTotal) by invoice Tgl
6. PostingStatus = group by PostingStok (default missing → "BELUM" only if DAL returns null;
   DAL always returns SUDAH/BELUM — treat unexpected values as separate bucket excluded from pie)
   → Sum(GrandTotal) for SUDAH and BELUM
7. TopPrincipal = group by Trim(SupplierName) or "Unknown" when blank
   → OrderByDescending PurchaseAmount, ThenBy PrincipalName, Take(10), assign Rank
8. PeriodYear/PeriodMonth from periode.Tgl1
9. GeneratedAt = generatedAt
```

**Constants:**

```csharp
public const int TopPrincipalCount = 10;
private const string UnknownPrincipal = "Unknown";
```

### 5.3 Refresh worker — `RefreshDashboardPurchasingSnapshotWorker`

Mirror `RefreshDashboardSalesSnapshotWorker`:

1. Insert refresh log (`Domain = "Purchasing"`, `TriggeredBy` from request).
2. Compute `periode = CurrentMonthPeriode(_tglJamDal.Now.Date)`.
3. Load `_invoiceViewDal.ListData(periode)`.
4. `_aggregator.Aggregate(rows, periode, generatedAt)`.
5. Transaction: `_snapshotDal.ReplaceCurrent(aggregate, refreshLogId)`.
6. Mark refresh log success/failure.

### 5.4 Snapshot DAL — `DashboardPurchasingSnapshotDal`

Implement:

| Method | Behavior |
| --- | --- |
| `GetCurrent()` | Read KPI + 3 child tables; return `DashboardPurchasingAggregateResult` or `null` |
| `ReplaceCurrent(result, refreshLogId)` | DELETE child rows for `SnapshotKey`; MERGE KPI; INSERT children |

Use counter prefixes (add to `BTR_ParamNo` seed if needed):

| Entity | Prefix | Example existing |
| --- | --- | --- |
| Week trend row | `PDP` | Sales uses `PDW` |
| Posting status row | `PDG` | — |
| Top principal row | `PDR` | Refresh log uses `PDR` — **use `PDT` for top principal** to avoid collision |

**Correction:** Refresh log uses `PDR`. Use **`PDT`** for top principal rows, **`PDG`** for posting status, **`PDP`** for week trend.

Provide static `MapToResponse(DashboardPurchasingAggregateResult)` → `DashboardPurchasingResponse` (same pattern as `DashboardSalesSnapshotDal.MapToResponse`).

### 5.5 Read DAL — `DashboardPurchasingDal`

```csharp
public DashboardPurchasingResponse GetSummary()
{
    var snapshot = _snapshotDal.GetCurrent();
    if (snapshot != null)
        return DashboardPurchasingSnapshotDal.MapToResponse(snapshot);
    throw new DashboardSnapshotUnavailableException("Dashboard data not yet available");
}
```

Optional **`DashboardPurchasingLiveDal`** (infrastructure, test-only helper) for verification tests — mirrors `DashboardSalesLiveDal` pattern: loads invoices live, runs aggregator, maps to response. Not registered in production DI.

### 5.6 Extend orchestration

| File | Change |
| --- | --- |
| `RefreshAllDashboardSnapshotsWorker` | Inject and run Purchasing after Sales |
| `RefreshDashboardSnapshotsCommand` handler | Add `case "Purchasing"`; update validation message |
| `Program.cs` (worker) | Add `Purchasing` to `ValidDomains` and switch |
| `DashboardOverviewDal.GetOverview()` | Query `BTR_PortalDashboardPurchasingKpi`; map `DashboardOverviewPurchasingSection` |
| `GetDashboardOverviewQuery.cs` | Add `Purchasing` section; include in `HasUnavailableDomain` |
| `DashboardSnapshotOptions` | `PurchasingIntervalMinutes = 30` |
| `HealthController.BuildDomainStatuses` | Add `"Purchasing"` domain; `GetIntervalMinutes` case |
| `ApplicationPortalExtensions` | `services.AddScoped<DashboardPurchasingInvoiceAggregator>()` |
| `InfrastructurePortalExtensions` | Register `IDashboardPurchasingSnapshotDal`, `IDashboardPurchasingDal` |

Worker DI (`WorkerDependencyConfig`) must register new worker interface (follow existing snapshot worker registrations).

---

## 6. API Contract

### 6.1 New endpoint

```
GET /api/dashboard/purchasing
Authorization: Bearer required
```

**Controller:** `PurchasingDashboardController` at route prefix `api/dashboard/purchasing` — mirror `SalesDashboardController`.

### 6.2 `DashboardPurchasingResponse`

| Property | Type | Meaning |
| --- | --- | --- |
| `GrandTotalPurchase` | `decimal` | Report footer equivalent |
| `TotalInvoice` | `int` | Report footer equivalent |
| `PendingPostingInvoiceCount` | `int` | Dashboard-only |
| `GeneratedAt` | `DateTime` | Snapshot freshness |
| `WeeklyTrend` | `List<DashboardPurchasingWeekTrendItem>` | Calendar weeks in current month |
| `PostingStatusBreakdown` | `List<DashboardPurchasingPostingStatusItem>` | SUDAH / BELUM by value |
| `TopPrincipalRanking` | `List<DashboardPurchasingRankingItem>` | Top 10 principals |

#### `DashboardPurchasingWeekTrendItem`

| Property | Type |
| --- | --- |
| `WeekStart` | `DateTime` |
| `WeekEnd` | `DateTime` |
| `WeekLabel` | `string` |
| `PurchaseAmount` | `decimal` |

#### `DashboardPurchasingPostingStatusItem`

| Property | Type |
| --- | --- |
| `StatusKey` | `string` — `SUDAH` / `BELUM` |
| `StatusLabel` | `string` |
| `SortOrder` | `int` |
| `PurchaseAmount` | `decimal` |

#### `DashboardPurchasingRankingItem`

| Property | Type |
| --- | --- |
| `Rank` | `int` |
| `PrincipalName` | `string` |
| `PurchaseAmount` | `decimal` |

### 6.3 Extended overview — `GET /api/dashboard/overview`

Add section:

```json
"Purchasing": {
  "GrandTotalPurchase": 150000000.00,
  "TotalInvoice": 42,
  "GeneratedAt": "2026-06-07T14:30:00"
}
```

`HasUnavailableDomain = true` when any of Sales, Piutang, Inventory, **or Purchasing** KPI row is missing.

### 6.4 Admin refresh

`POST /api/admin/dashboard/refresh` body `{ "Domain": "Purchasing" }` — supported after handler extension.

### 6.5 Health

`GET /api/health/dashboard-snapshots` includes Purchasing domain with `IntervalMinutes: 30`.

---

## 7. Worker and Operations

### 7.1 Task Scheduler job

Add a **fourth scheduled task** (or extend existing Sales 30-min task to run `--domain All` if operations prefer single orchestrator):

**Recommended:** dedicated task aligned with Sales cadence:

```text
Program: btr.portal.worker.exe
Arguments: --domain Purchasing --triggered-by Scheduler
Interval: every 30 minutes
```

Alternatively, change the existing `--domain All` schedule to 30 minutes and include Purchasing in the all-domain worker — **only if** operations accepts Piutang (15 min) and Inventory (60 min) sharing the same trigger. **Preferred approach:** separate Purchasing task at 30 min; keep existing per-domain schedules unchanged.

### 7.2 Configuration

Add to `appsettings.json` (API and worker):

```json
"DashboardSnapshot": {
  "PiutangIntervalMinutes": 15,
  "InventoryIntervalMinutes": 60,
  "SalesIntervalMinutes": 30,
  "PurchasingIntervalMinutes": 30
}
```

### 7.3 First deploy sequence

1. Deploy SQL tables to target database.
2. Deploy API + worker binaries.
3. Run manual refresh: `btr.portal.worker.exe --domain Purchasing --triggered-by Manual`.
4. Verify health endpoint shows Purchasing `Success`.
5. Deploy frontend.

Until step 3 completes, dashboard routes return snapshot-unavailable error (existing pattern).

### 7.4 ParamNo seed

If counter prefixes `PDP`, `PDG`, `PDT` are new, add rows to `BTR_ParamNo` seed script (`DataSeeds/BTR_ParamNo_PortalDashboard.sql`) following existing portal dashboard prefixes.

---

## 8. Frontend Implementation

### 8.1 Route

Add to `src/router/index.ts`:

```text
path: 'dashboard/purchasing'
name: 'purchasing-dashboard'
component: PurchasingDashboardView.vue
```

### 8.2 Sidebar

Add under Dashboard submenu in `MainLayout.vue` (after Inventory):

```text
label: 'Purchasing'
icon: 'pi pi-shopping-cart'
route: /dashboard/purchasing
```

### 8.3 Store and API

| File | Change |
| --- | --- |
| `src/models/dashboard.ts` | Add purchasing interfaces |
| `src/api/dashboardApi.ts` | Add `fetchDashboardPurchasing()` |
| `src/stores/dashboardStore.ts` | Add `purchasing` ref, `loadPurchasing()`, include in `reset()` |

### 8.4 Dashboard home — `DashboardHomeView.vue`

- Change grid to **4 columns** (responsive: 2×2 on medium, 1 column on small).
- Add 4th `KpiCard`:
  - Title: **Purchasing**
  - Icon: `pi pi-shopping-cart`
  - Metrics: Grand Total Purchase, Total Invoice
  - Link: `View purchasing analytics →` → `/dashboard/purchasing`
  - Meta: `GeneratedAt` from `overview.Purchasing`

### 8.5 Detail view — `PurchasingDashboardView.vue`

Mirror `SalesDashboardView.vue` structure with `DashboardDetailLayout`:

| Element | Value |
| --- | --- |
| Title | Purchasing Dashboard |
| Subtitle | Current Month Purchasing Activity (Void invoices excluded) |
| KPI 1 | Grand Total Purchase (`formatCurrency`) |
| KPI 2 | Total Invoice (`formatNumber`) |
| KPI 3 | Pending Posting Invoice Count (`formatNumber`) |

**Sections:**

1. `WeeklyTrendChart` — pass mapped trend (`PurchaseAmount` → `RecognizedAmount` for chart compatibility); props: `title="Weekly Purchase Trend"`, `emptyMessage="No weekly purchase data for the current period."`
2. `PostingStatusPieChart` — new component; props: `items` from `PostingStatusBreakdown`
3. `Top10RankingTable` — title **Top 10 Principal**; columns Rank, PrincipalName, PurchaseAmount

### 8.6 Component changes

#### `WeeklyTrendChart.vue` (minimal extension)

Add optional props with defaults preserving current Sales behavior:

| Prop | Default |
| --- | --- |
| `title` | `"Weekly Trend"` |
| `emptyMessage` | `"No weekly omzet data for the current period."` |

#### `PostingStatusPieChart.vue` (new)

Copy structure from `AgingPieChart.vue`:

| StatusKey | Color | Label |
| --- | --- | --- |
| `BELUM` | `#f97316` (orange) | BELUM |
| `SUDAH` | `#22c55e` (green) | SUDAH |

Title: **Posting Status Breakdown**. Tooltip formats currency.

---

## 9. Testing Strategy

### 9.1 Unit tests — aggregator

File: `DashboardPurchasingInvoiceAggregatorTest.cs`

| Test | Assertion |
| --- | --- |
| `GrandTotalPurchase_MatchesSumOfGrandTotal` | Sum of invoice GrandTotals |
| `TotalInvoice_MatchesRowCount` | Count of invoices |
| `PendingPostingInvoiceCount_CountsBelumOnly` | Only BELUM rows |
| `WeekTrend_SumsMatchGrandTotal` | Sum of week buckets ≤ GrandTotalPurchase; invoices outside month excluded |
| `PostingStatus_SudahBelum_SumToGrandTotal` | SUDAH + BELUM amounts = GrandTotalPurchase |
| `TopPrincipal_BlankSupplierMapsToUnknown` | Empty/null SupplierName → `"Unknown"` |
| `TopPrincipal_Take10_OrderedByPurchaseAmount` | Ranking correctness |

### 9.2 Traceability test (mandatory)

File: `DashboardPurchasingReportTraceabilityTest.cs`

Given the same stub `IInvoiceViewDal` rows and fixed `ITglJamDal`:

```text
aggregator.Aggregate(...).GrandTotalPurchase == PurchasingReportDal.GetReport().Summary.GrandTotalPurchase
aggregator.Aggregate(...).TotalInvoice == PurchasingReportDal.GetReport().Summary.TotalInvoice
```

This is the primary guard for acceptance criterion #6.

### 9.3 Snapshot verification

Optional `DashboardPurchasingLiveDal` + test mirroring `DashboardSalesSnapshotVerificationTest` (aggregator output matches live DAL mapping).

### 9.4 Handler tests

Extend `RefreshDashboardSnapshotsHandlerTest`:

- `Domain = "Purchasing"` invokes purchasing worker.
- Invalid domain message includes Purchasing in allowed list.

### 9.5 Manual test checklist

1. Sign in → sidebar **Dashboard → Purchasing** opens `/dashboard/purchasing`.
2. Home card shows purchasing KPIs with timestamp.
3. Compare dashboard Grand Total Purchase / Total Invoice with `/reports/purchasing` footer.
4. Posting pie shows SUDAH/BELUM; pending count matches BELUM invoice count in report rows.
5. Top 10 Principal table uses Principal labels.
6. After worker refresh, `GeneratedAt` updates.
7. Health endpoint lists Purchasing domain.

---

## 10. Risk Analysis

| Risk | Likelihood | Impact | Mitigation |
| --- | --- | --- | --- |
| Dashboard/report KPI drift | Medium | High | Shared `IInvoiceViewDal`; dedicated traceability unit test |
| Missing snapshot on first deploy | High | Medium | Deploy runbook: manual worker run before UI release; home shows existing unavailable warning |
| Month rollover stale KPI | Low | Medium | Store `PeriodYear`/`PeriodMonth`; worker overwrite on next refresh (same as Sales) |
| Week grouper mismatch | Low | Low | Reuse `SalesOmzetChartWeekGrouper` unchanged |
| Counter prefix collision | Low | Low | Use distinct prefixes PDP/PDG/PDT |
| Overview layout with 4 cards | Low | Low | Responsive CSS grid adjustment |

---

## 11. Documentation Updates (post-delivery)

| Document | Update |
| --- | --- |
| `docs/features/btr-portal/btr-portal-domain.md` | Add Purchasing Dashboard section; reverse "no dashboard" statement; KPI definitions |
| `docs/features/materialized-dashboard/materialized-dashboard-domain.md` | Add Purchasing domain tables and refresh cadence |
| `docs/work/materialize-dashboard-data/dashboard-snapshot-worker-runbook.md` | Add Purchasing task instructions |
| `docs/features/btr-portal/btr-portal-operational.md` | Navigation and monitoring |

---

## 12. Implementation Steps

Execute in order. Each step should compile before proceeding.

### Phase 1 — Database

1. Create SQL scripts for 4 purchasing snapshot tables.
2. Update `btr.sql.sqlproj`.
3. Update ParamNo seed if needed.
4. Deploy tables to dev database.

### Phase 2 — Application core

5. Add `DashboardPurchasingAggregateResult` model.
6. Implement `DashboardPurchasingInvoiceAggregator`.
7. Add aggregator unit tests (including traceability test).
8. Add `IDashboardPurchasingSnapshotDal` contract.
9. Implement `RefreshDashboardPurchasingSnapshotWorker` + request/result types.

### Phase 3 — Infrastructure

10. Implement `DashboardPurchasingSnapshotDal` (read + replace).
11. Implement `DashboardPurchasingDal` read path.
12. Add `DashboardPurchasingAgg` query/handler/DTOs.

### Phase 4 — Orchestration and API

13. Extend `RefreshAllDashboardSnapshotsWorker`, refresh command handler, worker `Program.cs`.
14. Extend `DashboardOverviewDal` and overview DTOs.
15. Add `PurchasingDashboardController`.
16. Extend `HealthController`, `DashboardSnapshotOptions`, appsettings.
17. Register DI in portal API and worker configs.
18. Extend handler tests.

### Phase 5 — Worker verification

19. Run `--domain Purchasing --triggered-by Manual`; confirm tables populated.
20. Verify `GET /api/dashboard/purchasing` and overview return data.

### Phase 6 — Frontend

21. Add TypeScript models and API client.
22. Extend dashboard store.
23. Extend `WeeklyTrendChart` props.
24. Create `PostingStatusPieChart.vue`.
25. Create `PurchasingDashboardView.vue`.
26. Update router, sidebar, dashboard home grid.
27. Manual UI verification against report.

### Phase 7 — Closeout

28. Update feature documentation.
29. Configure Task Scheduler job (30 min).
30. Capture screenshot for implementation summary (optional).

---

## 13. Acceptance Criteria Traceability

| # | Criterion | Implementation |
| --- | --- | --- |
| 1 | Sidebar + home link to `/dashboard/purchasing` | §8.1, §8.2, §8.4 |
| 2 | 4th home card with Grand Total Purchase + Total Invoice | §8.4, §6.3 |
| 3 | Title and subtitle | §8.5 |
| 4 | KPI row with pending posting count (count only) | §5.2, §8.5 |
| 5 | Weekly trend, posting breakdown, Top 10 Principal | §5.2, §8.5 |
| 6 | Footer KPI exact match | §5.2, §9.2 |
| 7 | GeneratedAt + 30 min refresh | §7, §5.3 |
| 8 | Void + Retur Beli exclusion | §3.3 (same DAL as report) |
| 9 | SUDAH/BELUM in breakdown | §4.3, §8.6 |
| 10 | Deferred features absent | §1 |
| 11 | Read-only | No write endpoints introduced |

---

## 14. Reference Files

| Area | Path |
| --- | --- |
| Analysis (scope) | `docs/work/purchasing-dashboard/analysis-report.md` |
| Report DAL | `btr.infrastructure/ReportingContext/PurchasingReportAgg/PurchasingReportDal.cs` |
| Sales aggregator | `btr.application/.../DashboardSalesFakturAggregator.cs` |
| Sales refresh worker | `btr.application/.../RefreshDashboardSalesSnapshotWorker.cs` |
| Sales snapshot DAL | `btr.infrastructure/.../DashboardSalesSnapshotDal.cs` |
| Overview DAL | `btr.infrastructure/.../DashboardOverviewDal.cs` |
| Sales dashboard UI | `btr.portal.web/src/views/dashboard/SalesDashboardView.vue` |
| Home dashboard UI | `btr.portal.web/src/views/dashboard/DashboardHomeView.vue` |
| Materialized dashboard plan | `docs/work/materialize-dashboard-data/implementation-plan.md` |
| M12 purchasing report plan | `docs/work/btr-portal-api-scaffolding/implementation-plan-m12-purchasing-report-v1.md` |
