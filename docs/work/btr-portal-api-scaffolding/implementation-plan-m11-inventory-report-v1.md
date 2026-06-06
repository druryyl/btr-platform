# Implementation Plan: M11 — Inventory Report V1

## Document Status

| Field | Value |
| --- | --- |
| Milestone | M11 — Inventory Report V1 |
| Authoritative requirements | `portal-analysis-m10-m12-final.md` |
| Reference pattern | M9 Sales Report V1 + M10 footer summary pattern |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |

---

## 1. Goal

Expose inventory detail behind the M6 Inventory Dashboard KPIs (`Total Inventory Value`, `Total Item`). Users inspect stock balance rows in a PrimeVue DataTable with footer summary totals that **must reconcile** with dashboard KPIs.

**Out of scope:** Kartu Stok, date/search filters, export, drilldown, unit breakdown columns (QtyBesar/QtyKecil), Supplier/Kategori columns, zero-qty rows.

---

## 2. Architecture Overview

```text
InventoryReportView.vue
    ↓ fetchInventoryReport()
inventoryReportStore (Pinia)
    ↓ GET /api/reports/inventory
InventoryReportController
    ↓ MediatR
GetInventoryReportHandler
    ↓ IInventoryReportDal
InventoryReportDal (wrapper)
    ↓ IStokBalanceViewDal.ListData()
StokBalanceViewDal (existing desktop DAL)
    ↓ SQL
BTR_StokBalanceWarehouse / BTR_Brg / …
```

**Critical distinction:** Row-level data is product × warehouse. Footer totals use **BrgId grouping** (same as `DashboardInventoryDal`) — not a naive sum of visible rows.

---

## 3. Backend Implementation

### 3.1 ReportingContext Structure

```text
btr.application/ReportingContext/InventoryReportAgg/
├── Contracts/
│   └── IInventoryReportDal.cs
└── Queries/
    └── GetInventoryReportQuery.cs

btr.infrastructure/ReportingContext/InventoryReportAgg/
└── InventoryReportDal.cs

btr.portal.api/Controllers/Reports/
└── InventoryReportController.cs
```

Register `.cs` files in `btr.application.csproj` and `btr.infrastructure.csproj`.

### 3.2 Query Objects

**File:** `GetInventoryReportQuery.cs`

| Type | Purpose |
| --- | --- |
| `GetInventoryReportQuery` | Empty `IRequest<InventoryReportResponse>` — snapshot report, no period params |
| `GetInventoryReportHandler` | Delegates to `_dal.GetReport()` |

### 3.3 Response DTOs

#### `InventoryReportResponse`

| Property | Type | Description |
| --- | --- | --- |
| `GeneratedAt` | `DateTime` | Server timestamp |
| `Summary` | `InventoryReportSummary` | Footer totals — must reconcile with M6 dashboard |
| `Rows` | `List<InventoryReportRow>` | Filtered, mapped rows |

**Note:** No `PeriodFrom` / `PeriodTo` — inventory is a point-in-time snapshot (Stock Balance), not a period report. Frontend subtitle reflects snapshot semantics.

#### `InventoryReportSummary`

| Property | Type | Calculation | Dashboard field |
| --- | --- | --- | --- |
| `TotalInventoryValue` | `decimal` | Sum of grouped `NilaiSediaan` (see §3.5) | `TotalInventoryValue` |
| `TotalItem` | `int` | Count of `BrgId` groups where aggregated `Qty > 0` | `TotalItem` |

### 3.4 Row DTOs

#### `InventoryReportRow`

Map from `StokBalanceView`. Five approved columns:

| Property | Source | Notes |
| --- | --- | --- |
| `ItemDisplay` | `BrgCode` + `BrgName` | Format: `"{BrgCode} — {BrgName}"` (trim empty parts) |
| `BrgId` | `BrgId` | Include for `data-key` uniqueness (product × warehouse); not displayed as column |
| `WarehouseName` | `WarehouseName` | |
| `Qty` | `Qty` | Pieces (`InPcs` equivalent) |
| `Hpp` | `Hpp` | |
| `NilaiSediaan` | `Hpp * Qty` | Computed at map time — same formula as dashboard |

**Do not** expose: `SupplierName`, `KategoriName`, `QtyBesar`, `QtyKecil`, `SatBesar`, `SatKecil`, `Conversion`.

### 3.5 DAL Wrapper Design

**File:** `InventoryReportDal.cs`  
**Interface:** `IInventoryReportDal` with `InventoryReportResponse GetReport()`.

#### Dependencies

| Dependency | Purpose |
| --- | --- |
| `IStokBalanceViewDal` | Load stock balance rows (auto-registered via Scrutor) |
| `ITglJamDal` | `GeneratedAt` timestamp |

#### Constants

Copy from `DashboardInventoryDal`:

```csharp
private const string InTransitWarehouseName = "In-Transit";
```

#### `GetReport()` algorithm

1. **Load:** `_stokBalanceViewDal.ListData()?.ToList() ?? empty`
2. **Exclude In-Transit:** `.Where(x => x.WarehouseName != InTransitWarehouseName)`
3. **Summary computation** — copy grouping logic verbatim from `DashboardInventoryDal.GetSummary()`:
   ```csharp
   var grouped = (
       from row in filteredRows
       group row by row.BrgId into g
       select new
       {
           Qty = g.Sum(x => x.Qty),
           NilaiSediaan = g.Sum(x => x.Hpp * x.Qty)
       }).ToList();

   var summary = new InventoryReportSummary
   {
       TotalInventoryValue = grouped.Sum(x => x.NilaiSediaan),
       TotalItem = grouped.Count(x => x.Qty > 0),
   };
   ```
4. **Row filter for display:** From filtered rows (post In-Transit exclusion), keep only `Qty > 0`
5. **Map rows:** Compute `NilaiSediaan = row.Hpp * row.Qty`, build `ItemDisplay`, order by `BrgCode`, then `WarehouseName`
6. **Return:** Response with summary, rows, `GeneratedAt`

#### `MapRow` helper

```csharp
private static InventoryReportRow MapRow(StokBalanceView row)
{
    var code = row.BrgCode?.Trim() ?? string.Empty;
    var name = row.BrgName?.Trim() ?? string.Empty;
    var itemDisplay = string.IsNullOrEmpty(code)
        ? name
        : string.IsNullOrEmpty(name)
            ? code
            : $"{code} — {name}";

    return new InventoryReportRow
    {
        BrgId = row.BrgId ?? string.Empty,
        ItemDisplay = itemDisplay,
        WarehouseName = row.WarehouseName ?? string.Empty,
        Qty = row.Qty,
        Hpp = row.Hpp,
        NilaiSediaan = row.Hpp * row.Qty,
    };
}
```

#### `BuildDataKey` note for frontend

Composite key recommended: `${BrgId}|${WarehouseName}` — document in frontend as `data-key` field or use a computed property.

### 3.6 Existing DAL Integration Strategy

| Asset | Registration | Usage |
| --- | --- | --- |
| `IStokBalanceViewDal` → `StokBalanceViewDal` | Auto via Scrutor `IListData<StokBalanceView>` scan | `ListData()` — no period filter |
| `ITglJamDal` | Explicit in `ApplicationPortalExtensions` | Timestamp only |

**Rules:**

- Do not modify `StokBalanceViewDal` SQL.
- Do not read from `BTR_StokMutasi` (Kartu Stok source) — balance table only.
- Do not reimplement unit conversion logic.

### 3.7 API Contract

#### Endpoint

```
GET /api/reports/inventory
Authorization: Bearer <JWT>
```

No query parameters.

#### Success response (HTTP 200)

```json
{
  "Status": "success",
  "Code": 200,
  "Message": null,
  "Data": {
    "GeneratedAt": "2026-06-06T14:30:00",
    "Summary": {
      "TotalInventoryValue": 850000000.0,
      "TotalItem": 312
    },
    "Rows": [
      {
        "BrgId": "BRG001",
        "ItemDisplay": "ABC-001 — Product Name",
        "WarehouseName": "Gudang Utama",
        "Qty": 150,
        "Hpp": 12500.0,
        "NilaiSediaan": 1875000.0
      }
    ]
  }
}
```

### 3.8 Controller Design

**File:** `InventoryReportController.cs`

```csharp
[Authorize]
[RoutePrefix("api/reports/inventory")]
public class InventoryReportController : ApiController
{
    [HttpGet, Route("")]
    public async Task<IHttpActionResult> Get()
    {
        var result = await _mediator.Send(new GetInventoryReportQuery());
        return Ok(ApiResponse<InventoryReportResponse>.Success(result));
    }
}
```

### 3.9 DI Registrations

#### `InfrastructurePortalExtensions.cs`

```csharp
services.AddScoped<IInventoryReportDal, InventoryReportDal>();
```

#### `PortalPresentationExtensions.cs`

```csharp
services.AddTransient<Controllers.Reports.InventoryReportController>();
```

---

## 4. Frontend Implementation

### 4.1 Route Structure

| Path | Name | Component |
| --- | --- | --- |
| `/reports/inventory` | `inventory-report` | `InventoryReportView.vue` |

### 4.2 Navigation

Add to Reports submenu in `MainLayout.vue`:

```typescript
{
  label: 'Inventory Report',
  icon: 'pi pi-box',
  command: () => router.push('/reports/inventory'),
  class: route.path === '/reports/inventory' ? 'layout-menu-item--active' : '',
},
```

### 4.3 View Structure

**File:** `src/views/reports/InventoryReportView.vue`

```text
┌─────────────────────────────────────────────────────┐
│ Header: "Inventory Report"                          │
│ Subtitle: "Stock balance snapshot (qty > 0)."       │
│                                    [Refresh button] │
├─────────────────────────────────────────────────────┤
│ Error Message                                       │
├─────────────────────────────────────────────────────┤
│ Card                                                │
│   DataTable (5 visible columns)                     │
│   ReportSummaryBar (reuse from M10)                 │
│   Generated-at timestamp                            │
└─────────────────────────────────────────────────────┘
```

No period label — inventory is a snapshot, not a date-range report.

### 4.4 DataTable Configuration

| Setting | Value |
| --- | --- |
| `data-key` | Composite: `` `${data.BrgId}|${data.WarehouseName}` `` via template binding or precomputed field |
| Paginator | Client-side, 25 default |
| Other settings | Same as M9/M10 |

#### Columns

| Field | Header | Sortable | Formatter |
| --- | --- | --- | --- |
| `ItemDisplay` | Item | Yes | — |
| `WarehouseName` | Warehouse | Yes | — |
| `Qty` | Qty | Yes | Integer display (no decimals) |
| `Hpp` | HPP | Yes | `formatCurrency()` |
| `NilaiSediaan` | Nilai Sediaan | Yes | `formatCurrency()` |

Do **not** display `BrgId` as a column.

### 4.5 Footer Summary Design

Reuse `ReportSummaryBar.vue` from M10.

```typescript
const summaryItems = computed(() => {
  if (!inventoryReport.report?.Summary) return []
  return [
    { label: 'Total Inventory Value', value: formatCurrency(inventoryReport.report.Summary.TotalInventoryValue) },
    { label: 'Total Item', value: String(inventoryReport.report.Summary.TotalItem) },
  ]
})
```

**Critical:** Do not sum `NilaiSediaan` from visible rows client-side — footer values come from API `Summary` which uses BrgId grouping.

Optional helper text below summary bar (muted, small font):

> Totals are aggregated by item across warehouses. Row values show per-warehouse balance.

### 4.6 Store Design

**File:** `src/stores/inventoryReportStore.ts`

Same Pinia composition pattern as `piutangReportStore.ts`:

| State | Type |
| --- | --- |
| `report` | `InventoryReportResponse \| null` |
| `loading` | `boolean` |
| `error` | `string \| null` |

| Action | Behavior |
| --- | --- |
| `loadReport()` | Calls `fetchInventoryReport()` |
| `reset()` | Clears state |

### 4.7 API Integration

#### Types — `src/models/reports.ts`

```typescript
export interface InventoryReportRow {
  BrgId: string
  ItemDisplay: string
  WarehouseName: string
  Qty: number
  Hpp: number
  NilaiSediaan: number
}

export interface InventoryReportSummary {
  TotalInventoryValue: number
  TotalItem: number
}

export interface InventoryReportResponse {
  GeneratedAt: string
  Summary: InventoryReportSummary
  Rows: InventoryReportRow[]
}
```

#### API function — `src/api/reportsApi.ts`

```typescript
export async function fetchInventoryReport(): Promise<InventoryReportResponse> {
  const { data } = await httpClient.get<ApiResponse<InventoryReportResponse>>('/api/reports/inventory')
  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load inventory report.')
  }
  return data.Data
}
```

### 4.8 Loading State

Same pattern as M10: DataTable `:loading` + Refresh button spinner.

### 4.9 Error State

PrimeVue `Message` with `severity="error"` above Card.

### 4.10 Empty State

```html
<div class="inventory-report__empty">
  <i class="pi pi-inbox inventory-report__empty-icon" />
  <p>No inventory items with quantity on hand.</p>
</div>
```

---

## 5. Verification

### 5.1 Backend Tests

#### Manual API checks

```powershell
curl.exe -o NUL -w "%{http_code}" http://localhost:5056/api/reports/inventory
curl.exe http://localhost:5056/api/reports/inventory -H "Authorization: Bearer <token>"
```

#### Structural checks

| # | Check | Expected |
| --- | --- | --- |
| 1 | No period fields in response | Snapshot only |
| 2 | All rows have `Qty > 0` | Filter applied |
| 3 | No In-Transit warehouse rows | Excluded |
| 4 | `NilaiSediaan` per row | `Hpp * Qty` |
| 5 | MediatR pipeline intact | No DAL in controller |

#### Optional unit test (recommended)

**File:** `btr.test/ReportingContext/InventoryReportDalTest.cs`

Mock `IStokBalanceViewDal`:

- Rows with `Qty = 0` excluded from `Rows` but included in grouping for summary
- In-Transit warehouse excluded from both rows and summary
- Multi-warehouse same `BrgId`: summary groups correctly; row count > distinct item count
- `TotalItem` counts items with aggregated Qty > 0 only

### 5.2 Frontend Tests

```powershell
cd src\j05-btr-distrib\btr.portal.web
npm run build
```

| # | Check | Expected |
| --- | --- | --- |
| 1 | Route loads | `/reports/inventory` |
| 2 | 5 columns displayed | Item, Warehouse, Qty, HPP, Nilai Sediaan |
| 3 | Summary bar | API values, not client sum |
| 4 | Qty formatting | Integer, no currency formatter |
| 5 | HPP/Nilai Sediaan | Currency formatted |

### 5.3 Dashboard KPI Reconciliation Validation

```powershell
$dash = curl.exe -s http://localhost:5056/api/dashboard/inventory -H "Authorization: Bearer <token>" | ConvertFrom-Json
$rpt = curl.exe -s http://localhost:5056/api/reports/inventory -H "Authorization: Bearer <token>" | ConvertFrom-Json

$dash.Data.TotalInventoryValue -eq $rpt.Data.Summary.TotalInventoryValue  # must match
$dash.Data.TotalItem -eq $rpt.Data.Summary.TotalItem                      # must match
```

| Metric | Dashboard | Report Summary | Rule |
| --- | --- | --- | --- |
| Total Inventory Value | `TotalInventoryValue` | `Summary.TotalInventoryValue` | Exact match |
| Total Item | `TotalItem` | `Summary.TotalItem` | Exact match |

**Anti-pattern check:** Sum of visible row `NilaiSediaan` will **not** equal footer if same item appears in multiple warehouses — this is expected. Reconciliation is against dashboard only.

---

## 6. Risks

| Risk | Severity | Mitigation |
| --- | --- | --- |
| **Inventory aggregation mismatch** | High | Footer uses `DashboardInventoryDal` grouping verbatim. Document helper text on UI. Reconciliation test mandatory. |
| **Warehouse filtering — In-Transit** | Medium | Reuse exact string constant `"In-Transit"` from dashboard. Do not parameterize. |
| **Footer reconciliation vs row sum** | Medium | Never compute footer client-side. API owns summary. UI note explains aggregation. |
| **Row count** | Low | `Qty > 0` filter reduces volume. Client-side pagination same as M9. |

---

## 7. Implementation Checklist

### Backend

- [ ] Create `IInventoryReportDal.cs`
- [ ] Create `GetInventoryReportQuery.cs` (query, handler, DTOs)
- [ ] Create `InventoryReportDal.cs` with dashboard-aligned summary logic
- [ ] Update `.csproj` files
- [ ] Create `InventoryReportController.cs`
- [ ] Register DI
- [ ] Build solution

### Frontend

- [ ] Add types to `reports.ts`
- [ ] Add `fetchInventoryReport()` to `reportsApi.ts`
- [ ] Create `inventoryReportStore.ts`
- [ ] Create `InventoryReportView.vue` (reuse `ReportSummaryBar`)
- [ ] Add route and sidebar entry
- [ ] Run `npm run build`

### Verification

- [ ] KPI reconciliation (§5.3)
- [ ] Confirm In-Transit excluded
- [ ] Confirm zero-qty rows excluded from table
- [ ] Dashboard unchanged

---

## 8. Files Summary

| Layer | File | Action |
| --- | --- | --- |
| Application | `ReportingContext/InventoryReportAgg/Contracts/IInventoryReportDal.cs` | Create |
| Application | `ReportingContext/InventoryReportAgg/Queries/GetInventoryReportQuery.cs` | Create |
| Infrastructure | `ReportingContext/InventoryReportAgg/InventoryReportDal.cs` | Create |
| Portal API | `Controllers/Reports/InventoryReportController.cs` | Create |
| Portal API | `Configurations/InfrastructurePortalExtensions.cs` | Modify |
| Portal API | `Configurations/PortalPresentationExtensions.cs` | Modify |
| Frontend | `src/models/reports.ts` | Modify |
| Frontend | `src/api/reportsApi.ts` | Modify |
| Frontend | `src/stores/inventoryReportStore.ts` | Create |
| Frontend | `src/views/reports/InventoryReportView.vue` | Create |
| Frontend | `src/router/index.ts` | Modify |
| Frontend | `src/layouts/MainLayout.vue` | Modify |

**Prerequisite:** M10 `ReportSummaryBar.vue` must exist (create in M10, reuse here unchanged).
