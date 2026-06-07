# Implementation Plan: M14 — Piutang Dashboard V2

## Document Status

| Field | Value |
| --- | --- |
| Milestone | M14 — Piutang Dashboard V2 |
| Authoritative requirements | `portal-analysis-m13-m15-final.md` |
| Reference pattern | M5 Piutang Dashboard V1 + M10 Piutang Report (`implementation-summary-m10.md`) |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |

---

## 1. Goal

Enhance the piutang dashboard with **Aging Analysis** (5 buckets) and **Top 10 Outstanding Customers** on dedicated route `/dashboard/piutang`. Extend `GET /api/dashboard/piutang` with aging, overdue customer KPI, and ranking data.

**Out of scope:** collection effectiveness KPI, drilldown, custom date filtering, export, Show Paid toggle, deferred columns, new API endpoints.

---

## 2. Architecture Overview

```text
PiutangDashboardView.vue (/dashboard/piutang)
    ↓ loadPiutang()
dashboardStore (Pinia)
    ↓ GET /api/dashboard/piutang
PiutangDashboardController
    ↓ MediatR
GetDashboardPiutangHandler
    ↓ IDashboardPiutangDal
DashboardPiutangDal (extended)
    ↓ IPiutangSalesWilayahDal + ITglJamDal
PiutangSalesWilayahDal (existing desktop DAL)
```

Aging and customer ranking are computed **server-side** in `DashboardPiutangDal` over the same filtered row set as M5/M10 — not client-side from table rows.

---

## 3. Backend Implementation

### 3.1 ReportingContext Structure

**No new aggregate folders.** Extend existing `DashboardPiutangAgg`:

```text
btr.application/ReportingContext/DashboardPiutangAgg/
├── Contracts/
│   └── IDashboardPiutangDal.cs          (unchanged signature)
└── Queries/
    └── GetDashboardPiutangQuery.cs        (extend DTOs)

btr.infrastructure/ReportingContext/DashboardPiutangAgg/
└── DashboardPiutangDal.cs                 (extend GetSummary())
```

### 3.2 Query Objects

| Type | Change |
| --- | --- |
| `GetDashboardPiutangQuery` | Unchanged |
| `GetDashboardPiutangHandler` | Unchanged |

### 3.3 Response DTO Changes

**File:** `GetDashboardPiutangQuery.cs`

#### Extended `DashboardPiutangResponse`

| Property | Type | New? | Meaning |
| --- | --- | --- | --- |
| `TotalPiutang` | `decimal` | Existing | M5 — unchanged |
| `TotalCustomer` | `int` | Existing | M5 — unchanged |
| `GeneratedAt` | `DateTime` | Existing | Unchanged |
| `OverdueCustomer` | `int` | **New** | Customers with sum of overdue-bucket balances > 0 |
| `AgingBuckets` | `List<DashboardPiutangAgingBucket>` | **New** | Five buckets — amounts sum to TotalPiutang |
| `TopCustomers` | `List<DashboardPiutangTopCustomer>` | **New** | Top 10 by outstanding balance |

#### `DashboardPiutangAgingBucket` (new)

| Property | Type | Values |
| --- | --- | --- |
| `BucketKey` | `string` | Stable key for chart colors — see table below |
| `BucketLabel` | `string` | UI display label |
| `Amount` | `decimal` | `Sum(KurangBayar)` in bucket |
| `SortOrder` | `int` | Fixed display order 1–5 |

**Approved bucket definitions:**

| SortOrder | BucketKey | BucketLabel | Rule (`DaysOverdue = Today − JatuhTempo.Date`) |
| --- | --- | --- | --- |
| 1 | `Current` | Current (Not Yet Due) | `DaysOverdue ≤ 0` |
| 2 | `Days1To30` | 1–30 Days | `1 ≤ DaysOverdue ≤ 30` |
| 3 | `Days31To60` | 31–60 Days | `31 ≤ DaysOverdue ≤ 60` |
| 4 | `Days61To90` | 61–90 Days | `61 ≤ DaysOverdue ≤ 90` |
| 5 | `DaysOver90` | > 90 Days | `DaysOverdue > 90` |

Boundaries are **inclusive** as specified. Each open faktur row assigns to exactly one bucket.

#### `DashboardPiutangTopCustomer` (new)

| Property | Type | Source |
| --- | --- | --- |
| `Rank` | `int` | 1-based after sort |
| `CustomerName` | `string` | Display name — see aggregation rules |
| `OutstandingBalance` | `decimal` | `Sum(KurangBayar)` per customer |

Do **not** expose `CustomerCode` in API response — aggregation key only (internal).

### 3.4 Aging Aggregation Design

All aging logic lives in `DashboardPiutangDal` as private static helpers — no new application-layer service class for M14.

#### Data scope (unchanged from M5)

1. Period: `OpenReceivablesPeriode()` → `2000-01-01` to `ITglJamDal.Now.Date`
2. Load: `_piutangSalesWilayahDal.ListData(periode)`
3. Filter: `KurangBayar > 1` (open balance)

#### Reference date

```csharp
var today = _tglJamDal.Now.Date;
```

#### Per-row bucket assignment

```csharp
private static string ResolveAgingBucketKey(DateTime jatuhTempo, DateTime today)
{
    var daysOverdue = (today - jatuhTempo.Date).Days;

    if (daysOverdue <= 0) return "Current";
    if (daysOverdue <= 30) return "Days1To30";
    if (daysOverdue <= 60) return "Days31To60";
    if (daysOverdue <= 90) return "Days61To90";
    return "DaysOver90";
}
```

#### Bucket aggregation

After assigning each outstanding row to a bucket:

```csharp
var bucketTotals = outstanding
    .GroupBy(r => ResolveAgingBucketKey(r.JatuhTempo, today))
    .ToDictionary(g => g.Key, g => g.Sum(r => r.KurangBayar));
```

Build `AgingBuckets` list with **all five buckets** always present (amount `0` when empty) in fixed `SortOrder`. Map keys to labels via a static dictionary.

**Reconciliation invariant:** `AgingBuckets.Sum(b => b.Amount)` === `TotalPiutang`.

### 3.5 Bucket Calculation Strategy

| Step | Action |
| --- | --- |
| 1 | Filter to open rows (`KurangBayar > 1`) |
| 2 | For each row, compute `DaysOverdue` from `JatuhTempo.Date` |
| 3 | Assign single bucket via inclusive boundary rules |
| 4 | Sum `KurangBayar` per bucket |
| 5 | Emit all 5 buckets (zero-filled) for stable pie chart |

**Do not** age on `FakturDate` — approved basis is `JatuhTempo` only.

### 3.6 Overdue Customer Calculation

**Definition:** Distinct customers where sum of balances in buckets **excluding Current** is **> 0**.

#### Customer aggregation key

Copy **verbatim** from `DashboardPiutangDal` / M10 `ResolveCustomerKey`:

```csharp
private static string ResolveCustomerKey(PiutangSalesWilayahDto row)
{
    if (row is null) return string.Empty;
    if (!string.IsNullOrWhiteSpace(row.CustomerCode))
        return row.CustomerCode.Trim();
    return row.CustomerName?.Trim() ?? string.Empty;
}
```

**Primary key:** `CustomerCode` when present; fallback to `CustomerName` (aligned with M5/M10).

#### Algorithm

```csharp
var overdueCustomerCount = outstanding
    .Where(r => ResolveAgingBucketKey(r.JatuhTempo, today) != "Current")
    .GroupBy(r => ResolveCustomerKey(r))
    .Where(g => g.Key.Length > 0)
    .Count(g => g.Sum(r => r.KurangBayar) > 0);
```

**Invariant:** `OverdueCustomer` ≤ `TotalCustomer`.

### 3.7 Top Customer Aggregation

```csharp
private static string ResolveCustomerDisplayName(IGrouping<string, PiutangSalesWilayahDto> group)
{
    // Prefer first non-empty CustomerName in group
    return group
        .Select(r => r.CustomerName?.Trim())
        .FirstOrDefault(n => !string.IsNullOrEmpty(n))
        ?? group.Key;
}

var topCustomers = outstanding
    .GroupBy(r => ResolveCustomerKey(r))
    .Where(g => g.Key.Length > 0)
    .Select(g => new
    {
        CustomerName = ResolveCustomerDisplayName(g),
        OutstandingBalance = g.Sum(r => r.KurangBayar)
    })
    .OrderByDescending(x => x.OutstandingBalance)
    .ThenBy(x => x.CustomerName, StringComparer.OrdinalIgnoreCase)
    .Take(10)
    .Select((x, index) => new DashboardPiutangTopCustomer
    {
        Rank = index + 1,
        CustomerName = x.CustomerName,
        OutstandingBalance = x.OutstandingBalance
    })
    .ToList();
```

Metric: `Sum(KurangBayar)` per customer, descending. Top N = **10**.

### 3.8 Dashboard DAL Changes

**File:** `DashboardPiutangDal.cs`

Refactor `GetSummary()` to:

1. Load and filter rows (existing M5 logic — unchanged).
2. Compute `TotalPiutang` and `TotalCustomer` (existing — unchanged).
3. Compute `AgingBuckets` (new).
4. Compute `OverdueCustomer` (new).
5. Compute `TopCustomers` (new).
6. Return extended response.

**Dependencies:** unchanged — `IPiutangSalesWilayahDal`, `ITglJamDal` only.

**Do not** call `PiutangReportDal` from dashboard DAL — duplicate logic inline to avoid cross-aggregate coupling (same pattern as M10).

### 3.9 API Contract Changes

#### Endpoint (unchanged)

```
GET /api/dashboard/piutang
Authorization: Bearer <JWT>
```

#### Extended success response

```json
{
  "Status": "success",
  "Code": 200,
  "Message": null,
  "Data": {
    "TotalPiutang": 125000000.0,
    "TotalCustomer": 42,
    "GeneratedAt": "2026-06-06T14:30:00",
    "OverdueCustomer": 18,
    "AgingBuckets": [
      { "BucketKey": "Current", "BucketLabel": "Current (Not Yet Due)", "Amount": 30000000.0, "SortOrder": 1 },
      { "BucketKey": "Days1To30", "BucketLabel": "1–30 Days", "Amount": 25000000.0, "SortOrder": 2 },
      { "BucketKey": "Days31To60", "BucketLabel": "31–60 Days", "Amount": 20000000.0, "SortOrder": 3 },
      { "BucketKey": "Days61To90", "BucketLabel": "61–90 Days", "Amount": 15000000.0, "SortOrder": 4 },
      { "BucketKey": "DaysOver90", "BucketLabel": "> 90 Days", "Amount": 35000000.0, "SortOrder": 5 }
    ],
    "TopCustomers": [
      {
        "Rank": 1,
        "CustomerName": "PT Example",
        "OutstandingBalance": 15000000.0
      }
    ]
  }
}
```

### 3.10 Controller Changes

**None.** `PiutangDashboardController` unchanged.

### 3.11 DI Registration Changes

**None.** `IDashboardPiutangDal` → `DashboardPiutangDal` already registered.

### 3.12 Unit Tests (recommended)

**File:** `btr.test/ReportingContext/DashboardPiutangDalTest.cs` (new or extend)

| Test | Assertion |
| --- | --- |
| Bucket boundaries | Row with `DaysOverdue = 0` → Current; `1` → Days1To30; `30` → Days1To30; `31` → Days31To60; `91` → DaysOver90 |
| Bucket sum | Sum of bucket amounts === TotalPiutang |
| Overdue customer | Customer with only Current balance not counted |
| Overdue customer | Customer with any 1–30+ balance counted |
| Top 10 limit | At most 10 customers |
| Customer key fallback | Empty `CustomerCode` uses `CustomerName` |

Use in-memory stub rows — no SQL.

---

## 4. Frontend Implementation

### 4.1 Route Structure

**File:** `src/router/index.ts`

| Path | Name | Component |
| --- | --- | --- |
| `/dashboard/piutang` | `piutang-dashboard` | `PiutangDashboardView.vue` (new) |

### 4.2 Navigation

**File:** `src/layouts/MainLayout.vue`

Add to Dashboard submenu (after Sales):

```typescript
{
  label: 'Piutang',
  icon: 'pi pi-wallet',
  command: () => router.push('/dashboard/piutang'),
  class: route.path === '/dashboard/piutang' ? 'layout-menu-item--active' : '',
},
```

**File:** `DashboardHomeView.vue` — add Piutang KPI card link:

```vue
<RouterLink to="/dashboard/piutang" class="kpi-card__link">
  View piutang analytics →
</RouterLink>
```

### 4.3 `/dashboard/piutang` Page

**File:** `src/views/dashboard/PiutangDashboardView.vue` (new)

#### Approved layout order

```text
┌─────────────────────────────────────────────────────┐
│ Header: "Piutang Dashboard" + Refresh               │
├─────────────────────────────────────────────────────┤
│ KPI Row: Total Piutang | Total Customer | Overdue   │
├─────────────────────────────────────────────────────┤
│ Aging Distribution (pie chart — 5 buckets)          │
├─────────────────────────────────────────────────────┤
│ Top 10 Outstanding Customers (table)                │
└─────────────────────────────────────────────────────┘
```

Wrap in `DashboardDetailLayout.vue` (from M13).

**Subtitle:** `Outstanding balance snapshot — open receivables only.`

**On mount:** `dashboard.loadPiutang()`.

### 4.4 KPI Cards

| KPI | Field | Formatter |
| --- | --- | --- |
| Total Piutang | `TotalPiutang` | `formatCurrency` |
| Total Customer | `TotalCustomer` | `formatNumber` |
| Overdue Customer | `OverdueCustomer` | `formatNumber` |

Use same metric styling as M13 detail KPI row.

### 4.5 Aging Pie Chart

**File:** `src/components/dashboard/AgingPieChart.vue` (new)

- Props: `buckets: DashboardPiutangAgingBucket[]`, `loading: boolean`
- PrimeVue `Chart` type `pie`
- Labels: `buckets.map(b => b.BucketLabel)` sorted by `SortOrder`
- Data: `buckets.map(b => b.Amount)`
- Tooltip: currency via `formatCurrency`
- Color palette: 5 distinct colors (define static array — Current=green-ish, overdue=warm tones)
- Empty state: all amounts zero → "No outstanding piutang data."
- Legend: display on right or bottom

**Do not** filter out zero buckets — API always sends 5; chart may hide zero slices via Chart.js `filter` or show all for consistency (implementer choice: show all 5 labels, zero slices omitted from pie is acceptable).

### 4.6 Top 10 Customer Table

Reuse `Top10RankingTable.vue` (from M13):

| Column | Field |
| --- | --- |
| Rank | `Rank` |
| Customer Name | `CustomerName` |
| Outstanding Balance | `OutstandingBalance` (currency) |

Title: `Top 10 Outstanding Customers`.

### 4.7 Store Changes

**File:** `src/stores/dashboardStore.ts`

Add:

```typescript
async function loadPiutang(): Promise<void> {
  loading.value = true
  error.value = null
  try {
    piutang.value = await fetchDashboardPiutang()
  } catch (err) {
    error.value = getApiErrorMessage(err, 'Failed to load piutang dashboard.')
  } finally {
    loading.value = false
  }
}
```

Export `loadPiutang`.

### 4.8 API Integration / Types

**File:** `src/models/dashboard.ts`

```typescript
export interface DashboardPiutangAgingBucket {
  BucketKey: string
  BucketLabel: string
  Amount: number
  SortOrder: number
}

export interface DashboardPiutangTopCustomer {
  Rank: number
  CustomerName: string
  OutstandingBalance: number
}

export interface DashboardPiutangResponse {
  TotalPiutang: number
  TotalCustomer: number
  GeneratedAt: string
  OverdueCustomer: number
  AgingBuckets: DashboardPiutangAgingBucket[]
  TopCustomers: DashboardPiutangTopCustomer[]
}
```

**File:** `src/api/dashboardApi.ts` — no change.

---

## 5. Verification

### 5.1 Bucket Reconciliation

| Check | Expected |
| --- | --- |
| Sum of `AgingBuckets[].Amount` | === `TotalPiutang` (exact) |
| Bucket count | Always 5 entries |
| Boundary spot-check | Manually verify 2–3 rows from M10 report against bucket assignment |

```powershell
curl.exe http://localhost:5050/api/dashboard/piutang -H "Authorization: Bearer <token>"
# Sum AgingBuckets Amounts in script; compare to TotalPiutang
```

### 5.2 Total Piutang Reconciliation

| Check | Expected |
| --- | --- |
| `TotalPiutang` | === M5 home card (exact) |
| `TotalPiutang` | === M10 report `Summary.TotalPiutang` (exact) |

### 5.3 Total Customer Reconciliation

| Check | Expected |
| --- | --- |
| `TotalCustomer` | === M5 home card (exact) |
| `TotalCustomer` | === M10 report `Summary.TotalCustomer` (exact) |

### 5.4 Overdue Customer Verification

| Check | Expected |
| --- | --- |
| `OverdueCustomer` | ≤ `TotalCustomer` |
| Customer with only Current faktur | Not counted in OverdueCustomer |
| Customer with any 1–30+ balance | Counted |

Manual validation: pick 2 customers from M10 report — compute overdue balance by hand.

### 5.5 Top Customer Verification

| Check | Expected |
| --- | --- |
| Ranking count | ≤ 10 |
| Sort order | Descending by OutstandingBalance |
| Sum of Top 10 | ≤ TotalPiutang |

### 5.6 Build Verification

| # | Check | Expected |
| --- | --- | --- |
| 1 | Backend build | Zero errors |
| 2 | `npm run build` | Zero errors |
| 3 | `/dashboard/piutang` | Page renders with all sections |
| 4 | `/dashboard` home | Piutang summary card + link works |
| 5 | JWT auth | 401 without token |

---

## 6. Risks

| Risk | Severity | Mitigation |
| --- | --- | --- |
| **Large dataset aggregation** (~11K rows) | Medium | Single-pass in-memory aggregation in DAL (same as M10 load); no N+1 queries |
| **Customer grouping consistency** | Low | Reuse exact `ResolveCustomerKey` from M5/M10 |
| **Empty CustomerCode rows** | Low | Fallback to `CustomerName` — same as M10 |
| **M5 regression** | Low | Keep existing TotalPiutang/TotalCustomer logic untouched |
| **Timezone on JatuhTempo** | Low | Normalize to `.Date` before diff |

---

## 7. File Checklist

### Modified (backend)

| File | Change |
| --- | --- |
| `GetDashboardPiutangQuery.cs` | New DTO types |
| `DashboardPiutangDal.cs` | Aging, overdue, top customers |

### Modified (frontend)

| File | Change |
| --- | --- |
| `dashboard.ts` | Extended piutang types |
| `dashboardStore.ts` | `loadPiutang()` |
| `DashboardHomeView.vue` | Piutang detail link |
| `MainLayout.vue` | Piutang submenu item |
| `router/index.ts` | `/dashboard/piutang` route |

### New (frontend)

| File | Purpose |
| --- | --- |
| `PiutangDashboardView.vue` | M14 detail page |
| `AgingPieChart.vue` | 5-bucket pie chart |

### Reused (from M13)

| File | Usage |
| --- | --- |
| `DashboardDetailLayout.vue` | Page shell |
| `Top10RankingTable.vue` | Customer ranking |

---

## 8. Out of Scope Confirmation

- [ ] No collection effectiveness KPI
- [ ] No new API endpoints
- [ ] No drilldown to faktur
- [ ] No date range params
- [ ] No export
- [ ] No Show Paid toggle
