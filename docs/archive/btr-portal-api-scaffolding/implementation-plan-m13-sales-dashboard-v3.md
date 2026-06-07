# Implementation Plan: M13 — Sales Dashboard V3

## Document Status

| Field | Value |
| --- | --- |
| Milestone | M13 — Sales Dashboard V3 |
| Authoritative requirements | `portal-analysis-m13-m15-final.md` |
| Reference pattern | M8 Sales Dashboard V2 (`implementation-summary-milestone-8.md`) |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |

---

## 1. Goal

Enhance the sales dashboard with **Target vs Achievement** and **Top 10 Salesman ranking** on dedicated route `/dashboard/sales`. Refactor `/dashboard` home to summary KPI cards with navigation links only (remove full Sales Trend card from home).

**Out of scope:** drilldown, custom date filtering, export, per-salesman target chart, `SalesOmzetTargetResolver`, pipeline in achievement/ranking, new API endpoints, transaction screens.

---

## 2. Architecture Overview

```text
DashboardHomeView.vue          SalesDashboardView.vue (/dashboard/sales)
    ↓ loadDashboard()              ↓ loadSales()
dashboardStore (Pinia)           dashboardStore (same store)
    ↓ GET /api/dashboard/sales     ↓ GET /api/dashboard/sales (same endpoint)
SalesDashboardController
    ↓ MediatR
GetDashboardSalesHandler
    ↓ IDashboardSalesDal
DashboardSalesDal (extended)
    ↓ ISalesOmzetDal + ISalesOmzetChartSummaryBuilder + ISalesOmzetTargetDal + ITglJamDal
Existing desktop DALs / builders
```

No new controllers, aggregates, or endpoints. Extend existing `DashboardSalesAgg` response DTO and `DashboardSalesDal` mapping only.

---

## 3. Backend Implementation

### 3.1 ReportingContext Structure

**No new aggregate folders.** All changes stay within existing `DashboardSalesAgg`:

```text
btr.application/ReportingContext/DashboardSalesAgg/
├── Contracts/
│   └── IDashboardSalesDal.cs          (unchanged signature)
└── Queries/
    └── GetDashboardSalesQuery.cs        (extend DTOs)

btr.infrastructure/ReportingContext/DashboardSalesAgg/
└── DashboardSalesDal.cs                 (extend GetSummary())

btr.application/SalesContext/SalesOmzetAgg/Contracts/
└── ISalesOmzetTargetDal.cs              (add SumTargetAmountForMonth)

btr.infrastructure/SalesContext/SalesOmzetAgg/
└── SalesOmzetTargetDal.cs               (implement new method)
```

### 3.2 Aggregate Structure

| Layer | Component | Change |
| --- | --- | --- |
| Application | `DashboardSalesAgg` | Extend response DTOs in query file |
| Application | `ISalesOmzetTargetDal` | Add month-sum method (not per-rep resolver) |
| Infrastructure | `DashboardSalesDal` | Orchestrate builder + target sum + ranking |
| Infrastructure | `SalesOmzetTargetDal` | Implement month-sum SQL |
| Portal API | `SalesDashboardController` | **No change** — same `GetDashboardSalesQuery` |

MediatR handler (`GetDashboardSalesHandler`) unchanged — still delegates to `_dal.GetSummary()`.

### 3.3 Query Objects

**File:** `GetDashboardSalesQuery.cs`

| Type | Change |
| --- | --- |
| `GetDashboardSalesQuery` | Unchanged — empty `IRequest<DashboardSalesResponse>`, no query params |
| `GetDashboardSalesHandler` | Unchanged |

### 3.4 Response DTOs

**File:** `GetDashboardSalesQuery.cs` — extend existing types in same file (M8 convention).

#### Extended `DashboardSalesResponse`

| Property | Type | New? | Meaning |
| --- | --- | --- | --- |
| `TotalOmzet` | `decimal` | Existing | Recognized omzet — backward compatible (M4/M7/M8) |
| `CompletedOmzet` | `decimal` | Existing | Same as `RecognizedOmzet` |
| `PipelineOmzet` | `decimal` | Existing | Unchanged |
| `TotalFaktur` | `int` | Existing | Unchanged |
| `TotalCustomer` | `int` | Existing | Unchanged |
| `GeneratedAt` | `DateTime` | Existing | Unchanged |
| `WeeklyTrend` | `List<DashboardSalesWeekTrendItem>` | Existing | Unchanged — M8 weekly data |
| `TotalTarget` | `decimal` | **New** | Sum of all `TargetAmount` rows for current month |
| `TotalAchievement` | `decimal` | **New** | Same value as `CompletedOmzet` / `RecognizedOmzet` |
| `AchievementPercent` | `decimal?` | **New** | `SalesOmzetChartAchievementPolicy.ComputePercent(TotalAchievement, TotalTarget)` |
| `TargetVsAchievement` | `DashboardSalesTargetVsAchievement` | **New** | Company-level two-bar chart data |
| `TopSalesmanRanking` | `List<DashboardSalesRankingItem>` | **New** | Top 10 by Completed Omzet |

**Backward compatibility:** All M4/M7/M8 fields remain populated with identical semantics. New fields are additive.

#### `DashboardSalesTargetVsAchievement` (new)

| Property | Type | Value |
| --- | --- | --- |
| `TargetAmount` | `decimal` | Same as `TotalTarget` |
| `AchievementAmount` | `decimal` | Same as `TotalAchievement` |

Two scalar properties — frontend renders as grouped bar chart with labels `"Target"` and `"Achievement"`.

#### `DashboardSalesRankingItem` (new)

| Property | Type | Source |
| --- | --- | --- |
| `Rank` | `int` | 1-based index after sort |
| `SalesPersonName` | `string` | `SalesOmzetSalesPersonSlice.SalesPersonName` |
| `CompletedOmzet` | `decimal` | `SalesOmzetSalesPersonSlice.RecognizedOmzet` |

#### Existing `DashboardSalesWeekTrendItem`

Unchanged — no modifications.

### 3.5 Dashboard DAL Changes

**File:** `DashboardSalesDal.cs`

#### New dependency

Inject `ISalesOmzetTargetDal _targetDal` alongside existing dependencies.

#### Updated constructor

```csharp
public DashboardSalesDal(
    ISalesOmzetDal salesOmzetDal,
    ISalesOmzetChartSummaryBuilder chartSummaryBuilder,
    ISalesOmzetTargetDal targetDal,
    ITglJamDal tglJamDal)
```

#### `GetSummary()` algorithm (extended)

1. **Period:** `CurrentMonthPeriode()` — unchanged.
2. **Mode:** `SalesOmzetPeriodFilterMode.OmzetPeriod` — unchanged (Pipeline excluded from achievement/ranking).
3. **Load rows:** `_salesOmzetDal.ListData(periode, mode)` — unchanged.
4. **Build summary:** `_chartSummaryBuilder.Build(rows, periode, mode)` — unchanged (do **not** pass `targetAmount`; do **not** use `SalesOmzetTargetResolver`).
5. **Total Target:** `_targetDal.SumTargetAmountForMonth(today.Year, today.Month)`.
6. **Total Achievement:** `summary.RecognizedOmzet`.
7. **Achievement %:** `SalesOmzetChartAchievementPolicy.ComputePercent(totalAchievement, totalTarget)`.
8. **Ranking:** `_chartSummaryBuilder.BuildManagerComparison(rows, topCount: 10)` — portal Top 10, not desktop default 15.
9. **Map response:** populate all existing fields plus new fields.

#### Ranking mapping

```csharp
var rankingSlices = _chartSummaryBuilder.BuildManagerComparison(rows, topCount: 10);
var ranking = rankingSlices
    .Select((slice, index) => new DashboardSalesRankingItem
    {
        Rank = index + 1,
        SalesPersonName = slice.SalesPersonName,
        CompletedOmzet = slice.RecognizedOmzet
    })
    .ToList();
```

#### Target vs Achievement mapping

```csharp
TargetVsAchievement = new DashboardSalesTargetVsAchievement
{
    TargetAmount = totalTarget,
    AchievementAmount = totalAchievement
}
```

### 3.6 Existing Builder Reuse Strategy

| Builder / Policy | Method | Usage in M13 |
| --- | --- | --- |
| `SalesOmzetChartSummaryBuilder` | `Build(rows, periode, mode)` | `RecognizedOmzet`, `PipelineOmzet`, `ByWeek` — identical to M8 |
| `SalesOmzetChartSummaryBuilder` | `BuildManagerComparison(rows, topCount: 10)` | Top 10 salesman ranking |
| `SalesOmzetChartAmountPolicy` | (via builder) | Completed omzet amount rules — no direct portal call |
| `SalesOmzetChartAchievementPolicy` | `ComputePercent(achievement, target)` | Aggregate KPI percent — not capped at 100% |

**Do not use:**

| Component | Reason |
| --- | --- |
| `SalesOmzetTargetResolver` | Single-rep scope; M13 requires sum-all-targets |
| `Build()` with `targetAmount` parameter | Would bind to one rep's target |
| `ByStatus` slices | Out of scope |
| Desktop `ManagerComparisonTopCount` (15) | Portal uses 10 |

### 3.7 Existing Target DAL Reuse Strategy

**Extend** `ISalesOmzetTargetDal` — do not create a new interface.

#### New interface method

**File:** `ISalesOmzetTargetDal.cs`

```csharp
/// <summary>Sum of TargetAmount for all salespeople with a target row for the given month.</summary>
decimal SumTargetAmountForMonth(int year, int month);
```

#### Implementation

**File:** `SalesOmzetTargetDal.cs`

```csharp
public decimal SumTargetAmountForMonth(int year, int month)
{
    const string sql = @"
        SELECT ISNULL(SUM(TargetAmount), 0)
        FROM BTR_SalesOmzetTarget
        WHERE TargetYear = @TargetYear
          AND TargetMonth = @TargetMonth";

    // Same connection/parameter pattern as GetTargetAmount
}
```

**Rules:**

- Sum **all rows** for `(TargetYear, TargetMonth)` — not gated on achievement > 0.
- Return `0m` when no rows exist (not null).
- Reuse existing `BTR_SalesOmzetTarget` table — no schema changes.
- `GetTargetAmount(salesPersonId, year, month)` remains unchanged for desktop/resolver.

`ISalesOmzetTargetDal` is already registered in `InfrastructurePortalExtensions.cs` — no new DI registration for the interface.

### 3.8 API Contract Changes

#### Endpoint (unchanged)

```
GET /api/dashboard/sales
Authorization: Bearer <JWT>
```

No query parameters.

#### Extended success response (HTTP 200)

```json
{
  "Status": "success",
  "Code": 200,
  "Message": null,
  "Data": {
    "TotalOmzet": 150000000.0,
    "CompletedOmzet": 150000000.0,
    "PipelineOmzet": 25000000.0,
    "TotalFaktur": 42,
    "TotalCustomer": 28,
    "GeneratedAt": "2026-06-06T14:30:00",
    "WeeklyTrend": [
      {
        "WeekStart": "2026-06-01T00:00:00",
        "WeekEnd": "2026-06-07T00:00:00",
        "WeekLabel": "01 Jun–07 Jun",
        "RecognizedAmount": 50000000.0
      }
    ],
    "TotalTarget": 200000000.0,
    "TotalAchievement": 150000000.0,
    "AchievementPercent": 75.0,
    "TargetVsAchievement": {
      "TargetAmount": 200000000.0,
      "AchievementAmount": 150000000.0
    },
    "TopSalesmanRanking": [
      {
        "Rank": 1,
        "SalesPersonName": "BUDI",
        "CompletedOmzet": 45000000.0
      }
    ]
  }
}
```

When `TotalTarget <= 0`, `AchievementPercent` is `null` (policy behavior).

#### Error responses

Unchanged — 401 via JWT filter, 500 via `GlobalExceptionFilter`.

### 3.9 Controller Changes

**None.** `SalesDashboardController.Get()` continues to send `GetDashboardSalesQuery` and return `ApiResponse<DashboardSalesResponse>.Success(result)`.

### 3.10 DI Registration Changes

| File | Change |
| --- | --- |
| `InfrastructurePortalExtensions.cs` | **No change** — `IDashboardSalesDal` and `ISalesOmzetTargetDal` already registered |
| `ApplicationPortalExtensions.cs` | **No change** — `ISalesOmzetChartSummaryBuilder` already registered |
| `PortalPresentationExtensions.cs` | **No change** |
| `DashboardSalesDal.cs` constructor | Add `ISalesOmzetTargetDal` parameter — DI resolves automatically |

### 3.11 Unit Tests (recommended)

**File:** `btr.test/ReportingContext/DashboardSalesDalTest.cs` (new)

| Test | Assertion |
| --- | --- |
| TotalAchievement equals RecognizedOmzet | Stub builder returns known omzet |
| TotalTarget from target DAL | Stub returns known sum |
| AchievementPercent | Matches policy for stub values |
| Ranking count | At most 10 items |
| Ranking order | Descending by CompletedOmzet |

Reuse stub patterns from `SalesOmzetChartSummaryTest.cs` and `SalesOmzetTargetTest.cs`.

---

## 4. Frontend Implementation

### 4.1 Route Structure

**File:** `src/router/index.ts`

Add child routes under authenticated layout:

| Path | Name | Component |
| --- | --- | --- |
| `/dashboard` | `dashboard` | `DashboardHomeView.vue` (existing — refactored) |
| `/dashboard/sales` | `sales-dashboard` | `SalesDashboardView.vue` (new) |

Routes for M14/M15 added in later milestones — do not add placeholder routes in M13.

### 4.2 Navigation / Sidebar

**File:** `src/layouts/MainLayout.vue`

Replace flat Dashboard menu item with nested group:

```typescript
{
  label: 'Dashboard',
  icon: 'pi pi-home',
  items: [
    {
      label: 'Overview',
      icon: 'pi pi-th-large',
      command: () => router.push('/dashboard'),
      class: route.path === '/dashboard' ? 'layout-menu-item--active' : '',
    },
    {
      label: 'Sales',
      icon: 'pi pi-chart-line',
      command: () => router.push('/dashboard/sales'),
      class: route.path === '/dashboard/sales' ? 'layout-menu-item--active' : '',
    },
  ],
},
```

M14/M15 add Piutang and Inventory sub-items in their milestones.

**Active class rule:** Use exact path match for sub-items; Overview active only on `/dashboard` exactly.

### 4.3 Dashboard Home Changes

**File:** `src/views/dashboard/DashboardHomeView.vue`

| Change | Detail |
| --- | --- |
| Remove | `<SalesTrendCard>` — weekly trend moves to `/dashboard/sales` only |
| Add | "View details" link on each KPI card → respective detail route |
| Keep | Three summary KPI cards (Sales, Piutang, Inventory) with M4/M5/M6 metrics |
| Keep | Header + Refresh button + `loadDashboard()` |

**Sales KPI card link:**

```vue
<RouterLink to="/dashboard/sales" class="kpi-card__link">
  View sales analytics →
</RouterLink>
```

Piutang/Inventory links point to `/dashboard/piutang` and `/dashboard/inventory` — add in M14/M15 when views exist, or use `v-if="false"` placeholder comment until then. **M13 implementer:** add Sales link only; stub comments for others.

### 4.4 `/dashboard/sales` Page

**File:** `src/views/dashboard/SalesDashboardView.vue` (new)

#### Page layout (approved order)

```text
┌─────────────────────────────────────────────────────┐
│ Header: "Sales Dashboard" + Refresh                 │
├─────────────────────────────────────────────────────┤
│ Error Message (if error)                            │
├─────────────────────────────────────────────────────┤
│ KPI Row: Total Target | Total Achievement | Ach %  │
├─────────────────────────────────────────────────────┤
│ Target vs Achievement (grouped bar chart)           │
├─────────────────────────────────────────────────────┤
│ Weekly Trend (line chart — M8 data)                 │
├─────────────────────────────────────────────────────┤
│ Top 10 Salesman (DataTable)                         │
└─────────────────────────────────────────────────────┘
```

Use shared `DashboardDetailLayout.vue` (new — see §4.5) for header/refresh/error shell.

**Subtitle:** `Current month performance — omzet period.`

**On mount:** call `dashboard.loadSales()` (new store action).

### 4.5 Shared Components (introduced in M13)

#### `DashboardDetailLayout.vue`

**File:** `src/components/dashboard/DashboardDetailLayout.vue`

Props: `title`, `subtitle`, `loading`, `error`.

Slots: default (page content).

Provides: page header, refresh button (emits `@refresh`), error `Message`.

Reused by M14/M15 detail pages.

#### `WeeklyTrendChart.vue`

**File:** `src/components/dashboard/WeeklyTrendChart.vue`

Extract chart section from `SalesTrendCard.vue`:

- Props: `weeklyTrend: DashboardSalesWeekTrendItem[]`, `loading: boolean`
- PrimeVue `Chart` type `line` — same colors/options as M8
- Empty state when all `RecognizedAmount === 0`

**Refactor `SalesTrendCard.vue`:** Either delete (no longer used on home) or refactor to use `WeeklyTrendChart` internally. **Preferred:** delete `SalesTrendCard.vue` after extraction — home no longer renders it.

#### `TargetVsAchievementChart.vue`

**File:** `src/components/dashboard/TargetVsAchievementChart.vue`

- Props: `data: DashboardSalesTargetVsAchievement | null`, `loading: boolean`
- PrimeVue `Chart` type `bar`
- Labels: `['Target', 'Achievement']`
- Dataset values: `[TargetAmount, AchievementAmount]`
- Y-axis: currency formatter via `formatCurrency`
- Empty state when both values are zero

#### `Top10RankingTable.vue`

**File:** `src/components/dashboard/Top10RankingTable.vue`

Generic ranking table — reused by M14/M15.

Props:

| Prop | Type | Purpose |
| --- | --- | --- |
| `title` | `string` | Card/table heading |
| `columns` | `{ field, header }[]` | Column definitions |
| `rows` | `Record<string, unknown>[]` | Data rows |
| `loading` | `boolean` | Loading state |
| `valueField` | `string` | Currency column field name |
| `emptyMessage` | `string` | Empty state text |

M13 usage:

| Column | Field |
| --- | --- |
| Rank | `Rank` |
| Salesman | `SalesPersonName` |
| Completed Omzet | `CompletedOmzet` (currency) |

PrimeVue `DataTable` — no pagination (max 10 rows), striped, sort disabled (pre-sorted by API).

### 4.6 KPI Cards on Detail Page

**File:** `SalesDashboardView.vue` — three KPI metrics in a horizontal grid (reuse `KpiCard` or inline metric divs matching home style).

| KPI | Source field | Formatter |
| --- | --- | --- |
| Total Target | `TotalTarget` | `formatCurrency` |
| Total Achievement | `TotalAchievement` | `formatCurrency` |
| Achievement % | `AchievementPercent` | `formatPercent` (new helper) or inline `{value}%` / `—` when null |

**File:** `src/services/formatters.ts` — add:

```typescript
export function formatPercent(value: number | null | undefined): string {
  if (value == null) return '—'
  return `${value.toFixed(1)}%`
}
```

### 4.7 Target vs Achievement Chart

Render `TargetVsAchievementChart` bound to `dashboard.sales?.TargetVsAchievement`.

Company-level two-bar — **not** per-salesman.

### 4.8 Weekly Trend Reuse

Render `WeeklyTrendChart` bound to `dashboard.sales?.WeeklyTrend`.

Same M8 data and chart styling. Position **below** Target vs Achievement chart.

### 4.9 Top 10 Salesman Table

Render `Top10RankingTable` with `dashboard.sales?.TopSalesmanRanking ?? []`.

Title: `Top 10 Salesman`.

### 4.10 Store Changes

**File:** `src/stores/dashboardStore.ts`

Add domain-specific load actions (detail pages fetch one endpoint; home fetches all three):

```typescript
async function loadSales(): Promise<void> {
  loading.value = true
  error.value = null
  try {
    sales.value = await fetchDashboardSales()
  } catch (err) {
    error.value = getApiErrorMessage(err, 'Failed to load sales dashboard.')
  } finally {
    loading.value = false
  }
}
```

Export `loadSales` from store. M14/M15 add `loadPiutang()` / `loadInventory()` in their milestones.

**Note:** `loadDashboard()` unchanged for home — still loads all three endpoints in parallel. Detail pages call single-domain loaders to avoid unnecessary API calls.

### 4.11 API Integration / Types

**File:** `src/models/dashboard.ts` — extend:

```typescript
export interface DashboardSalesTargetVsAchievement {
  TargetAmount: number
  AchievementAmount: number
}

export interface DashboardSalesRankingItem {
  Rank: number
  SalesPersonName: string
  CompletedOmzet: number
}

export interface DashboardSalesResponse {
  // ... existing fields ...
  TotalTarget: number
  TotalAchievement: number
  AchievementPercent: number | null
  TargetVsAchievement: DashboardSalesTargetVsAchievement
  TopSalesmanRanking: DashboardSalesRankingItem[]
}
```

**File:** `src/api/dashboardApi.ts` — **no change** (same endpoint; extended response deserializes automatically).

---

## 5. Verification

### 5.1 KPI Reconciliation

| Check | Expected |
| --- | --- |
| `TotalAchievement` === `CompletedOmzet` | Exact match (same source) |
| `TotalAchievement` === M8 `TotalOmzet` | Exact match for same month |
| `TotalOmzet` unchanged | Backward compatible with M4/M7 home card |

```powershell
curl.exe http://localhost:5050/api/dashboard/sales -H "Authorization: Bearer <token>"
# Compare TotalAchievement, CompletedOmzet, TotalOmzet — all equal
```

### 5.2 Target Reconciliation

| Check | Expected |
| --- | --- |
| `TotalTarget` === `TargetVsAchievement.TargetAmount` | Exact match |
| `TotalTarget` === direct SQL sum on `BTR_SalesOmzetTarget` for current month | Spot-check in SSMS |

```sql
SELECT SUM(TargetAmount)
FROM BTR_SalesOmzetTarget
WHERE TargetYear = YEAR(GETDATE()) AND TargetMonth = MONTH(GETDATE());
```

### 5.3 Ranking Reconciliation

| Check | Expected |
| --- | --- |
| Ranking count | ≤ 10 |
| Ranking sort | Descending by `CompletedOmzet` |
| Sum of all ranking `CompletedOmzet` | ≤ `TotalAchievement` |
| Sum of all salesman omzet (manual group) | === `TotalAchievement` |

Cross-check against desktop `SalesOmzetChartForm` manager comparison (top 10 override) if available.

### 5.4 Achievement Percent

| Check | Expected |
| --- | --- |
| When `TotalTarget > 0` | `AchievementPercent` = round(`TotalAchievement / TotalTarget * 100`, 1) |
| When `TotalTarget <= 0` | `AchievementPercent` is null |

### 5.5 Build Verification

| # | Command | Expected |
| --- | --- | --- |
| 1 | `j05-btr-distrib.sln` Debug build | Zero errors |
| 2 | `npm run build` in `btr.portal.web` | Zero errors |
| 3 | Login + JWT | Still works |
| 4 | `GET /api/dashboard/sales` without JWT | HTTP 401 |
| 5 | `/dashboard` home | Summary cards only — no Sales Trend card |
| 6 | `/dashboard/sales` | Full M13 layout renders |

### 5.6 M8 Regression

| Check | Expected |
| --- | --- |
| `WeeklyTrend` still populated | Same buckets as pre-M13 |
| `PipelineOmzet` on API | Still present (not shown on M13 page — OK) |
| Home Sales KPI card | Total Omzet, Faktur, Customer unchanged |

---

## 6. Risks

| Risk | Severity | Mitigation |
| --- | --- | --- |
| **Target aggregation** — sum-all-targets differs from resolver | Low | Use dedicated `SumTargetAmountForMonth`; do not call `SalesOmzetTargetResolver` |
| **Ranking consistency** — portal Top 10 vs desktop Top 15 | Low | Pass `topCount: 10` explicitly to `BuildManagerComparison` |
| **M8 regression** — weekly trend or KPI breakage | Low | Keep existing M8 mapping unchanged; add fields additively; run regression checklist §5.6 |
| **AchievementPercent null** when no targets | Low | Frontend displays `—`; matches desktop policy |
| **Home page UX change** — removing Sales Trend card | Low | Approved in Round 2; link to `/dashboard/sales` |

---

## 7. File Checklist

### Modified (backend)

| File | Change |
| --- | --- |
| `GetDashboardSalesQuery.cs` | New DTO types + extended response |
| `DashboardSalesDal.cs` | Target sum, ranking, new fields |
| `ISalesOmzetTargetDal.cs` | `SumTargetAmountForMonth` |
| `SalesOmzetTargetDal.cs` | SQL implementation |

### Modified (frontend)

| File | Change |
| --- | --- |
| `dashboard.ts` | Extended sales types |
| `formatters.ts` | `formatPercent` |
| `dashboardStore.ts` | `loadSales()` |
| `DashboardHomeView.vue` | Remove trend card; add links |
| `MainLayout.vue` | Nested Dashboard menu |
| `router/index.ts` | `/dashboard/sales` route |

### New (frontend)

| File | Purpose |
| --- | --- |
| `SalesDashboardView.vue` | M13 detail page |
| `DashboardDetailLayout.vue` | Shared detail page shell |
| `WeeklyTrendChart.vue` | Extracted M8 line chart |
| `TargetVsAchievementChart.vue` | Company bar chart |
| `Top10RankingTable.vue` | Shared ranking table |

### Deleted (frontend)

| File | Reason |
| --- | --- |
| `SalesTrendCard.vue` | Replaced by detail page components (if fully extracted) |

### Optional (tests)

| File | Purpose |
| --- | --- |
| `DashboardSalesDalTest.cs` | Backend unit tests |
| `SalesOmzetTargetDalTest.cs` | Sum method test |

---

## 8. Out of Scope Confirmation

- [ ] No new API endpoints
- [ ] No drilldown from ranking/chart
- [ ] No custom date filtering
- [ ] No export
- [ ] No per-salesman target chart
- [ ] No `SalesOmzetTargetResolver` usage for Total Target
