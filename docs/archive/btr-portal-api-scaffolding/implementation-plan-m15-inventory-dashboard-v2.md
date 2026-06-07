# Implementation Plan: M15 — Inventory Dashboard V2

## Document Status

| Field | Value |
| --- | --- |
| Milestone | M15 — Inventory Dashboard V2 |
| Authoritative requirements | `portal-analysis-m13-m15-final.md` |
| Reference pattern | M6 Inventory Dashboard V1 + M11 Inventory Report (`implementation-summary-m11.md`) |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |

---

## 1. Goal

Enhance the inventory dashboard with **Category** and **Supplier** analysis on dedicated route `/dashboard/inventory`. Extend `GET /api/dashboard/inventory` with breakdown charts and Top 10 ranking tables.

**Out of scope:** pie/donut composition views (deferred M16+), warehouse analysis, ABC classification, drilldown, Kartu Stok, custom filters, export, new API endpoints.

---

## 2. Architecture Overview

```text
InventoryDashboardView.vue (/dashboard/inventory)
    ↓ loadInventory()
dashboardStore (Pinia)
    ↓ GET /api/dashboard/inventory
InventoryDashboardController
    ↓ MediatR
GetDashboardInventoryHandler
    ↓ IDashboardInventoryDal
DashboardInventoryDal (extended)
    ↓ IStokBalanceViewDal + ITglJamDal
StokBalanceViewDal (existing desktop DAL)
```

Category and supplier analytics derive from the **same BrgId-first pipeline** as M6 KPIs — ensuring footer/chart reconciliation.

---

## 3. Backend Implementation

### 3.1 ReportingContext Structure

**No new aggregate folders.** Extend existing `DashboardInventoryAgg`:

```text
btr.application/ReportingContext/DashboardInventoryAgg/
├── Contracts/
│   └── IDashboardInventoryDal.cs          (unchanged signature)
└── Queries/
    └── GetDashboardInventoryQuery.cs        (extend DTOs)

btr.infrastructure/ReportingContext/DashboardInventoryAgg/
└── DashboardInventoryDal.cs                 (extend GetSummary())
```

### 3.2 Query Objects

| Type | Change |
| --- | --- |
| `GetDashboardInventoryQuery` | Unchanged |
| `GetDashboardInventoryHandler` | Unchanged |

### 3.3 Response DTO Changes

**File:** `GetDashboardInventoryQuery.cs`

#### Extended `DashboardInventoryResponse`

| Property | Type | New? | Meaning |
| --- | --- | --- | --- |
| `TotalInventoryValue` | `decimal` | Existing | M6 — unchanged |
| `TotalItem` | `int` | Existing | M6 — unchanged |
| `GeneratedAt` | `DateTime` | Existing | Unchanged |
| `CategoryBreakdown` | `List<DashboardInventoryBreakdownItem>` | **New** | Top 10 categories by value (chart) |
| `SupplierBreakdown` | `List<DashboardInventoryBreakdownItem>` | **New** | Top 10 suppliers by value (chart) |
| `TopCategories` | `List<DashboardInventoryRankingItem>` | **New** | Top 10 categories (table) |
| `TopSuppliers` | `List<DashboardInventoryRankingItem>` | **New** | Top 10 suppliers (table) |

#### `DashboardInventoryBreakdownItem` (new — shared chart DTO)

| Property | Type | Meaning |
| --- | --- | --- |
| `Name` | `string` | Category or supplier name (`"Unknown"` when blank) |
| `InventoryValue` | `decimal` | `Sum(Hpp × Qty)` for group |

#### `DashboardInventoryRankingItem` (new — shared ranking DTO)

| Property | Type | Meaning |
| --- | --- | --- |
| `Rank` | `int` | 1-based |
| `Name` | `string` | Category or supplier name |
| `InventoryValue` | `decimal` | Group total value |

**Chart vs table:** Both use Top 10 by `InventoryValue` descending. Chart and table data contain the **same 10 rows** (duplicate DTO lists simplify frontend binding — charts bind to `CategoryBreakdown`, tables bind to `TopCategories`).

Alternative (equivalent): single list per dimension — implementer may populate both properties from one `Take(10)` result if values identical.

### 3.4 BrgId-First Aggregation Pipeline

Implement as private method `BuildItemGroups()` in `DashboardInventoryDal` — shared by KPI and breakdown logic.

#### Step-by-step (approved)

```text
1. Load IStokBalanceViewDal.ListData()
2. Exclude WarehouseName = "In-Transit"
3. Group by BrgId → Sum(Qty), Sum(Hpp × Qty) per item   [same as M6]
4. Exclude groups where aggregated Qty ≤ 0
5. Map each BrgId to KategoriName / SupplierName (blank → "Unknown")
6. Group by Category or Supplier → Sum(inventory value)
7. Order descending, Take(10) for charts and tables
```

#### Reference implementation (M6 — extract, do not duplicate divergently)

**File:** `DashboardInventoryDal.cs` — refactor existing `GetSummary()`:

```csharp
private const string InTransitWarehouseName = "In-Transit";
private const string UnknownLabel = "Unknown";

private sealed class ItemGroup
{
    public string BrgId { get; set; }
    public decimal Qty { get; set; }
    public decimal InventoryValue { get; set; }
    public string CategoryName { get; set; }
    public string SupplierName { get; set; }
}

private List<ItemGroup> BuildItemGroups()
{
    var rows = _stokBalanceViewDal.ListData()?.ToList()
               ?? new List<StokBalanceView>();

    var filtered = rows
        .Where(x => x.WarehouseName != InTransitWarehouseName)
        .ToList();

    return (
        from row in filtered
        group row by row.BrgId into g
        let qty = g.Sum(x => x.Qty)
        where qty > 0
        select new ItemGroup
        {
            BrgId = g.Key ?? string.Empty,
            Qty = qty,
            InventoryValue = g.Sum(x => x.Hpp * x.Qty),
            CategoryName = NormalizeDimensionName(g.Select(x => x.KategoriName).FirstOrDefault()),
            SupplierName = NormalizeDimensionName(g.Select(x => x.SupplierName).FirstOrDefault())
        }).ToList();
}

private static string NormalizeDimensionName(string name)
{
    return string.IsNullOrWhiteSpace(name) ? UnknownLabel : name.Trim();
}
```

**Category/supplier per BrgId:** Use first non-null value from grouped warehouse rows (all rows for same `BrgId` should share master-data names — if multiple, `FirstOrDefault` is acceptable).

#### KPI from item groups

```csharp
var itemGroups = BuildItemGroups();

TotalInventoryValue = itemGroups.Sum(x => x.InventoryValue),
TotalItem = itemGroups.Count,
```

This must produce **identical** KPI values to pre-M15 logic.

### 3.5 Category Rollup

```csharp
private List<DashboardInventoryRankingItem> BuildTopCategories(List<ItemGroup> itemGroups)
{
    return itemGroups
        .GroupBy(x => x.CategoryName, StringComparer.OrdinalIgnoreCase)
        .Select(g => new
        {
            Name = g.Key,
            InventoryValue = g.Sum(x => x.InventoryValue)
        })
        .OrderByDescending(x => x.InventoryValue)
        .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
        .Take(10)
        .Select((x, index) => new DashboardInventoryRankingItem
        {
            Rank = index + 1,
            Name = x.Name,
            InventoryValue = x.InventoryValue
        })
        .ToList();
}
```

Map to `TopCategories` and `CategoryBreakdown` (same data, map to `DashboardInventoryBreakdownItem`).

**Reconciliation:** Sum of **all** category groups (not just Top 10) === `TotalInventoryValue`. Implementer verifies with debug assertion or unit test.

### 3.6 Supplier Rollup

Same algorithm as category — group by `SupplierName` instead of `CategoryName`.

Populate `TopSuppliers` and `SupplierBreakdown`.

**Reconciliation:** Sum of **all** supplier groups (incl. Unknown) === `TotalInventoryValue`.

### 3.7 Unknown Category / Supplier Handling

| Condition | Display | Included in totals |
| --- | --- | --- |
| `KategoriName` blank/null/whitespace | `"Unknown"` | Yes |
| `SupplierName` blank/null/whitespace | `"Unknown"` | Yes |

Do **not** exclude items with blank dimensions.

Multiple BrgIds with blank category roll up into single `"Unknown"` category group.

### 3.8 Dashboard DAL Changes

**File:** `DashboardInventoryDal.cs`

Refactored structure:

```csharp
public DashboardInventoryResponse GetSummary()
{
    var itemGroups = BuildItemGroups();
    var topCategories = BuildTopCategories(itemGroups);
    var topSuppliers = BuildTopSuppliers(itemGroups);

    return new DashboardInventoryResponse
    {
        TotalInventoryValue = itemGroups.Sum(x => x.InventoryValue),
        TotalItem = itemGroups.Count,
        GeneratedAt = _tglJamDal.Now,
        CategoryBreakdown = MapBreakdown(topCategories),
        SupplierBreakdown = MapBreakdown(topSuppliers),
        TopCategories = topCategories,
        TopSuppliers = topSuppliers
    };
}

private static List<DashboardInventoryBreakdownItem> MapBreakdown(
    List<DashboardInventoryRankingItem> ranking)
    => ranking.Select(r => new DashboardInventoryBreakdownItem
    {
        Name = r.Name,
        InventoryValue = r.InventoryValue
    }).ToList();
```

**Dependencies:** unchanged — `IStokBalanceViewDal`, `ITglJamDal`.

### 3.9 API Contract Changes

#### Endpoint (unchanged)

```
GET /api/dashboard/inventory
Authorization: Bearer <JWT>
```

#### Extended success response

```json
{
  "Status": "success",
  "Code": 200,
  "Message": null,
  "Data": {
    "TotalInventoryValue": 850000000.0,
    "TotalItem": 1250,
    "GeneratedAt": "2026-06-06T14:30:00",
    "CategoryBreakdown": [
      { "Name": "Electronics", "InventoryValue": 200000000.0 },
      { "Name": "Unknown", "InventoryValue": 50000000.0 }
    ],
    "SupplierBreakdown": [
      { "Name": "PT Supplier A", "InventoryValue": 180000000.0 }
    ],
    "TopCategories": [
      { "Rank": 1, "Name": "Electronics", "InventoryValue": 200000000.0 }
    ],
    "TopSuppliers": [
      { "Rank": 1, "Name": "PT Supplier A", "InventoryValue": 180000000.0 }
    ]
  }
}
```

Charts display Top 10 horizontal bars from `CategoryBreakdown` / `SupplierBreakdown`.

### 3.10 Controller Changes

**None.**

### 3.11 DI Registration Changes

**None.**

### 3.12 Unit Tests (recommended)

**File:** `btr.test/ReportingContext/DashboardInventoryDalTest.cs` (new)

Reference expectations from M11 `InventoryReportDalTest` if present.

| Test | Assertion |
| --- | --- |
| In-Transit excluded | Rows in In-Transit not counted |
| Qty ≤ 0 excluded | Zero-qty BrgId groups omitted |
| Unknown mapping | Blank category → `"Unknown"` |
| KPI reconciliation | TotalInventoryValue === sum of all category groups |
| KPI reconciliation | TotalInventoryValue === sum of all supplier groups |
| Top 10 limit | At most 10 items per ranking |
| M6 parity | TotalItem/TotalInventoryValue match pre-refactor stub |

---

## 4. Frontend Implementation

### 4.1 Route Structure

**File:** `src/router/index.ts`

| Path | Name | Component |
| --- | --- | --- |
| `/dashboard/inventory` | `inventory-dashboard` | `InventoryDashboardView.vue` (new) |

### 4.2 Navigation

**File:** `src/layouts/MainLayout.vue`

Add to Dashboard submenu:

```typescript
{
  label: 'Inventory',
  icon: 'pi pi-box',
  command: () => router.push('/dashboard/inventory'),
  class: route.path === '/dashboard/inventory' ? 'layout-menu-item--active' : '',
},
```

**File:** `DashboardHomeView.vue` — add Inventory KPI card link to `/dashboard/inventory`.

### 4.3 `/dashboard/inventory` Page

**File:** `src/views/dashboard/InventoryDashboardView.vue` (new)

#### Approved layout order

```text
┌─────────────────────────────────────────────────────┐
│ Header: "Inventory Dashboard" + Refresh               │
├─────────────────────────────────────────────────────┤
│ KPI Row: Total Inventory Value | Total Item         │
├─────────────────────────────────────────────────────┤
│ Inventory by Category (horizontal bar)              │
├─────────────────────────────────────────────────────┤
│ Inventory by Supplier (horizontal bar)              │
├─────────────────────────────────────────────────────┤
│ Top 10 Categories (table)                           │
├─────────────────────────────────────────────────────┤
│ Top 10 Suppliers (table)                            │
└─────────────────────────────────────────────────────┘
```

Use `DashboardDetailLayout.vue` (M13).

**Subtitle:** `Point-in-time stock snapshot — excludes In-Transit, zero qty items.`

**On mount:** `dashboard.loadInventory()`.

### 4.4 KPI Cards

| KPI | Field | Formatter |
| --- | --- | --- |
| Total Inventory Value | `TotalInventoryValue` | `formatCurrency` |
| Total Item | `TotalItem` | `formatNumber` |

Same styling as other detail pages.

### 4.5 Category Chart

**File:** `src/components/dashboard/InventoryHorizontalBarChart.vue` (new)

Generic horizontal bar chart — reused for category and supplier.

Props:

| Prop | Type |
| --- | --- |
| `title` | `string` |
| `items` | `DashboardInventoryBreakdownItem[]` |
| `loading` | `boolean` |

Implementation:

- PrimeVue `Chart` with `indexAxis: 'y'` (Chart.js horizontal bar)
- Labels: `items.map(i => i.Name)`
- Data: `items.map(i => i.InventoryValue)`
- Tooltip: `formatCurrency`
- Chart height: scale with row count (min 280px, ~28px per bar)
- Empty state: no items or all zero

M15 renders twice:

```vue
<InventoryHorizontalBarChart
  title="Inventory by Category"
  :items="dashboard.inventory?.CategoryBreakdown ?? []"
  :loading="dashboard.loading"
/>
```

### 4.6 Supplier Chart

Same component:

```vue
<InventoryHorizontalBarChart
  title="Inventory by Supplier"
  :items="dashboard.inventory?.SupplierBreakdown ?? []"
  :loading="dashboard.loading"
/>
```

### 4.7 Top 10 Category Table

Reuse `Top10RankingTable.vue`:

| Column | Field |
| --- | --- |
| Rank | `Rank` |
| Category | `Name` |
| Inventory Value | `InventoryValue` (currency) |

Title: `Top 10 Categories`.

Data: `dashboard.inventory?.TopCategories ?? []`.

### 4.8 Top 10 Supplier Table

Same table component:

Title: `Top 10 Suppliers`.

Data: `dashboard.inventory?.TopSuppliers ?? []`.

Columns: Rank, Supplier, Inventory Value.

### 4.9 Store Changes

**File:** `src/stores/dashboardStore.ts`

Add:

```typescript
async function loadInventory(): Promise<void> {
  loading.value = true
  error.value = null
  try {
    inventory.value = await fetchDashboardInventory()
  } catch (err) {
    error.value = getApiErrorMessage(err, 'Failed to load inventory dashboard.')
  } finally {
    loading.value = false
  }
}
```

Export `loadInventory`.

### 4.10 API Integration / Types

**File:** `src/models/dashboard.ts`

```typescript
export interface DashboardInventoryBreakdownItem {
  Name: string
  InventoryValue: number
}

export interface DashboardInventoryRankingItem {
  Rank: number
  Name: string
  InventoryValue: number
}

export interface DashboardInventoryResponse {
  TotalInventoryValue: number
  TotalItem: number
  GeneratedAt: string
  CategoryBreakdown: DashboardInventoryBreakdownItem[]
  SupplierBreakdown: DashboardInventoryBreakdownItem[]
  TopCategories: DashboardInventoryRankingItem[]
  TopSuppliers: DashboardInventoryRankingItem[]
}
```

---

## 5. Verification

### 5.1 Inventory Value Reconciliation

| Check | Expected |
| --- | --- |
| `TotalInventoryValue` | === M6 home card (exact) |
| `TotalInventoryValue` | === M11 report `Summary.TotalInventoryValue` (exact) |

### 5.2 Item Count Reconciliation

| Check | Expected |
| --- | --- |
| `TotalItem` | === M6 home card (exact) |
| `TotalItem` | === M11 report `Summary.TotalItem` (exact) |

### 5.3 Category Total Reconciliation

| Check | Expected |
| --- | --- |
| Sum of all category groups (full rollup, not just Top 10) | === `TotalInventoryValue` |
| `"Unknown"` included when blank categories exist | Present in breakdown |

Verify via unit test or temporary debug endpoint — not client-side sum of Top 10.

### 5.4 Supplier Total Reconciliation

| Check | Expected |
| --- | --- |
| Sum of all supplier groups (full rollup) | === `TotalInventoryValue` |
| `"Unknown"` included when blank suppliers exist | Present in breakdown |

### 5.5 Top 10 Verification

| Check | Expected |
| --- | --- |
| Each ranking count | ≤ 10 |
| Sort order | Descending by InventoryValue |
| Sum of Top 10 categories | ≤ TotalInventoryValue |
| Chart data matches table data | Same names/values for category (and supplier) |

### 5.6 Build Verification

| # | Check | Expected |
| --- | --- | --- |
| 1 | Backend build | Zero errors |
| 2 | `npm run build` | Zero errors |
| 3 | `/dashboard/inventory` | All 5 sections render |
| 4 | Horizontal bars | Chart.js `indexAxis: 'y'` displays correctly |
| 5 | M6 home regression | KPI card values unchanged |

---

## 6. Risks

| Risk | Severity | Mitigation |
| --- | --- | --- |
| **Aggregation correctness** | Low | Single `BuildItemGroups()` shared by KPI + rollups; unit test full category sum |
| **Unknown category handling** | Low | `NormalizeDimensionName` → `"Unknown"`; include in totals |
| **Unknown supplier handling** | Low | Same helper |
| **BrgId → category mapping** | Low | One name per BrgId from balance view; matches M6/M11 |
| **M6 KPI regression** | Low | KPI computed from same `itemGroups` list |
| **Chart.js horizontal bar** | Low | `indexAxis: 'y'` — same library as M8 |

---

## 7. File Checklist

### Modified (backend)

| File | Change |
| --- | --- |
| `GetDashboardInventoryQuery.cs` | New DTO types |
| `DashboardInventoryDal.cs` | Refactor + category/supplier rollups |

### Modified (frontend)

| File | Change |
| --- | --- |
| `dashboard.ts` | Extended inventory types |
| `dashboardStore.ts` | `loadInventory()` |
| `DashboardHomeView.vue` | Inventory detail link |
| `MainLayout.vue` | Inventory submenu item |
| `router/index.ts` | `/dashboard/inventory` route |

### New (frontend)

| File | Purpose |
| --- | --- |
| `InventoryDashboardView.vue` | M15 detail page |
| `InventoryHorizontalBarChart.vue` | Category + supplier charts |

### Reused (from M13)

| File | Usage |
| --- | --- |
| `DashboardDetailLayout.vue` | Page shell |
| `Top10RankingTable.vue` | Category + supplier tables |

---

## 8. Out of Scope Confirmation

- [ ] No pie/donut composition chart
- [ ] No warehouse breakdown
- [ ] No Kartu Stok / `BTR_StokMutasi`
- [ ] No new API endpoints
- [ ] No drilldown to item detail
- [ ] No date filters or export
