# Implementation Plan: M12 — Purchasing Report V1

## Document Status

| Field | Value |
| --- | --- |
| Milestone | M12 — Purchasing Report V1 |
| Authoritative requirements | `portal-analysis-m10-m12-final.md` |
| Reference pattern | M9 Sales Report V1 (structural mirror) + M10 footer summary |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |

---

## 1. Goal

Provide visibility into purchasing activities via PF1 invoice header report. Standalone report — **no dashboard KPI anchor**. Users inspect purchase invoice rows in a PrimeVue DataTable with footer summary totals.

**Out of scope:** PF2 line detail, PF3 daily detail, PF4 retur beli, date/search filters, export, drilldown, PostingStok transactional workflow (PT2).

**Terminology:** BTR has no separate Purchase Order entity. UI page title is **Purchasing Report**; invoice column header is **Invoice**.

---

## 2. Architecture Overview

```text
PurchasingReportView.vue
    ↓ fetchPurchasingReport()
purchasingReportStore (Pinia)
    ↓ GET /api/reports/purchasing
PurchasingReportController
    ↓ MediatR
GetPurchasingReportHandler
    ↓ IPurchasingReportDal
PurchasingReportDal (wrapper)
    ↓ IInvoiceViewDal.ListData(Periode)
InvoiceViewDal (existing desktop DAL)
    ↓ SQL
BTR_Invoice / BTR_Supplier / BTR_Warehouse
```

Structurally identical to M9 `SalesReportDal` with added `Summary` footer and `PostingStok` column.

---

## 3. Backend Implementation

### 3.1 ReportingContext Structure

```text
btr.application/ReportingContext/PurchasingReportAgg/
├── Contracts/
│   └── IPurchasingReportDal.cs
└── Queries/
    └── GetPurchasingReportQuery.cs

btr.infrastructure/ReportingContext/PurchasingReportAgg/
└── PurchasingReportDal.cs

btr.portal.api/Controllers/Reports/
└── PurchasingReportController.cs
```

Register `.cs` files in `btr.application.csproj` and `btr.infrastructure.csproj`.

**Naming note:** Use `PurchasingReportAgg` (not `InvoiceReportAgg`) to align with portal product language ("Purchasing Report") while wrapping `IInvoiceViewDal`.

### 3.2 Query Objects

**File:** `GetPurchasingReportQuery.cs`

| Type | Purpose |
| --- | --- |
| `GetPurchasingReportQuery` | Empty `IRequest<PurchasingReportResponse>` |
| `GetPurchasingReportHandler` | Delegates to `_dal.GetReport()` |

### 3.3 Response DTOs

#### `PurchasingReportResponse`

| Property | Type | Description |
| --- | --- | --- |
| `PeriodFrom` | `DateTime` | Current month start |
| `PeriodTo` | `DateTime` | Current month end |
| `GeneratedAt` | `DateTime` | Server timestamp |
| `Summary` | `PurchasingReportSummary` | Footer totals |
| `Rows` | `List<PurchasingReportRow>` | Invoice rows |

#### `PurchasingReportSummary`

| Property | Type | Calculation |
| --- | --- | --- |
| `GrandTotalPurchase` | `decimal` | `Sum(GrandTotal)` over all rows |
| `TotalInvoice` | `int` | Count of invoice rows |

No dashboard reconciliation required — standalone totals.

### 3.4 Row DTOs

#### `PurchasingReportRow`

Map from `InvoiceView`. Nine approved columns (PF1 header):

| Property | Source (`InvoiceView`) | Notes |
| --- | --- | --- |
| `InvoiceCode` | `InvoiceCode` | |
| `InvoiceDate` | `Tgl` | `.Date` normalization |
| `SupplierName` | `SupplierName` | |
| `WarehouseName` | `WarehouseName` | |
| `Total` | `Total` | Before discount/tax |
| `Disc` | `Disc` | |
| `Tax` | `Tax` | |
| `GrandTotal` | `GrandTotal` | |
| `PostingStok` | `PostingStok` | Values: `SUDAH` or `BELUM` — pass through from DAL |

Do **not** expose `InvoiceId` in API response (internal only if needed for `data-key` — use `InvoiceCode` as key like M9 uses `FakturCode`).

### 3.5 DAL Wrapper Design

**File:** `PurchasingReportDal.cs`  
**Interface:** `IPurchasingReportDal` with `PurchasingReportResponse GetReport()`.

#### Dependencies

| Dependency | Purpose |
| --- | --- |
| `IInvoiceViewDal` | Load invoice rows (auto-registered via Scrutor `IListData<InvoiceView, Periode>`) |
| `ITglJamDal` | Current month period + timestamp |

#### `GetReport()` algorithm

1. **Period:** `CurrentMonthPeriode()` — copy verbatim from `SalesReportDal`:
   ```csharp
   var today = _tglJamDal.Now.Date;
   var monthStart = new DateTime(today.Year, today.Month, 1);
   var monthEnd = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
   return new Periode(monthStart, monthEnd);
   ```
2. **Load:** `_invoiceViewDal.ListData(periode)?.ToList() ?? empty`
   - Void filter already in DAL SQL (`VoidDate = '3000-01-01'`)
   - `PostingStok` already computed in DAL (`IIF(IsStokPosted = 1, 'SUDAH', 'BELUM')`)
3. **Summary:**
   ```csharp
   var summary = new PurchasingReportSummary
   {
       GrandTotalPurchase = rows.Sum(r => r.GrandTotal),
       TotalInvoice = rows.Count,
   };
   ```
4. **Map rows:** Order by `Tgl`, then `InvoiceCode`
5. **Return:** Full response

#### `MapRow` helper

```csharp
private static PurchasingReportRow MapRow(InvoiceView row) => new PurchasingReportRow
{
    InvoiceCode = row.InvoiceCode ?? string.Empty,
    InvoiceDate = row.Tgl.Date,
    SupplierName = row.SupplierName ?? string.Empty,
    WarehouseName = row.WarehouseName ?? string.Empty,
    Total = row.Total,
    Disc = row.Disc,
    Tax = row.Tax,
    GrandTotal = row.GrandTotal,
    PostingStok = row.PostingStok ?? string.Empty,
};
```

### 3.6 Existing DAL Integration Strategy

| Asset | Registration | Usage |
| --- | --- | --- |
| `IInvoiceViewDal` → `InvoiceViewDal` | Auto via Scrutor | `ListData(periode)` |
| `ITglJamDal` | Explicit | Period + timestamp |

**Rules:**

- Do not modify `InvoiceViewDal` SQL.
- Do not expose PF2/PF3 DALs.
- Do not invoke `InvoiceBuilder` or `GenStokInvoiceWorker` (write path).
- Do not apply 122-day period validation from desktop — V1 uses fixed current month only.

### 3.7 API Contract

#### Endpoint

```
GET /api/reports/purchasing
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
    "PeriodFrom": "2026-06-01T00:00:00",
    "PeriodTo": "2026-06-30T23:59:59",
    "GeneratedAt": "2026-06-06T14:30:00",
    "Summary": {
      "GrandTotalPurchase": 45000000.0,
      "TotalInvoice": 12
    },
    "Rows": [
      {
        "InvoiceCode": "INV-001",
        "InvoiceDate": "2026-06-03T00:00:00",
        "SupplierName": "PT Supplier",
        "WarehouseName": "Gudang Utama",
        "Total": 10000000.0,
        "Disc": 0.0,
        "Tax": 1100000.0,
        "GrandTotal": 11100000.0,
        "PostingStok": "SUDAH"
      }
    ]
  }
}
```

### 3.8 Controller Design

**File:** `PurchasingReportController.cs`

```csharp
[Authorize]
[RoutePrefix("api/reports/purchasing")]
public class PurchasingReportController : ApiController
{
    [HttpGet, Route("")]
    public async Task<IHttpActionResult> Get()
    {
        var result = await _mediator.Send(new GetPurchasingReportQuery());
        return Ok(ApiResponse<PurchasingReportResponse>.Success(result));
    }
}
```

### 3.9 DI Registrations

#### `InfrastructurePortalExtensions.cs`

```csharp
services.AddScoped<IPurchasingReportDal, PurchasingReportDal>();
```

#### `PortalPresentationExtensions.cs`

```csharp
services.AddTransient<Controllers.Reports.PurchasingReportController>();
```

---

## 4. Frontend Implementation

### 4.1 Route Structure

| Path | Name | Component |
| --- | --- | --- |
| `/reports/purchasing` | `purchasing-report` | `PurchasingReportView.vue` |

### 4.2 Navigation

Add to Reports submenu in `MainLayout.vue`:

```typescript
{
  label: 'Purchasing Report',
  icon: 'pi pi-shopping-cart',
  command: () => router.push('/reports/purchasing'),
  class: route.path === '/reports/purchasing' ? 'layout-menu-item--active' : '',
},
```

### 4.3 View Structure

**File:** `src/views/reports/PurchasingReportView.vue`

Mirror `SalesReportView.vue` with footer summary added:

```text
┌─────────────────────────────────────────────────────┐
│ Header: "Purchasing Report"                         │
│ Subtitle: "Purchase invoices for {periodLabel}."    │
│                                    [Refresh button] │
├─────────────────────────────────────────────────────┤
│ Error Message                                       │
├─────────────────────────────────────────────────────┤
│ Card                                                │
│   DataTable (9 columns)                             │
│   ReportSummaryBar (reuse from M10)                 │
│   Generated-at timestamp                            │
└─────────────────────────────────────────────────────┘
```

### 4.4 DataTable Configuration

| Setting | Value |
| --- | --- |
| `data-key` | `InvoiceCode` |
| Paginator | Client-side, 25 default |
| Other settings | Same as M9 |

#### Columns

| Field | Header | Sortable | Formatter |
| --- | --- | --- | --- |
| `InvoiceCode` | Invoice | Yes | — |
| `InvoiceDate` | Date | Yes | `formatDate()` |
| `SupplierName` | Supplier | Yes | — |
| `WarehouseName` | Warehouse | Yes | — |
| `Total` | Total | Yes | `formatCurrency()` |
| `Disc` | Disc | Yes | `formatCurrency()` |
| `Tax` | Tax | Yes | `formatCurrency()` |
| `GrandTotal` | Grand Total | Yes | `formatCurrency()` |
| `PostingStok` | Posting Stok | Yes | Badge/tag styling (see below) |

#### PostingStok display

Use simple text with optional CSS class — no new component library:

| Value | Display | CSS class |
| --- | --- | --- |
| `SUDAH` | SUDAH | `purchasing-report__posting--done` (green/muted-success) |
| `BELUM` | BELUM | `purchasing-report__posting--pending` (amber/muted-warn) |
| empty/other | — | Muted dash |

Do not link to PT2 PostingStok form — read-only visibility only.

### 4.5 Footer Summary Design

Reuse `ReportSummaryBar.vue`:

```typescript
const summaryItems = computed(() => {
  if (!purchasingReport.report?.Summary) return []
  return [
    { label: 'Grand Total Purchase', value: formatCurrency(purchasingReport.report.Summary.GrandTotalPurchase) },
    { label: 'Total Invoice', value: String(purchasingReport.report.Summary.TotalInvoice) },
  ]
})
```

For M12, row-level sum of `GrandTotal` **will** equal footer `GrandTotalPurchase` (simple sum, no grouping). Still use API `Summary` for consistency.

### 4.6 Store Design

**File:** `src/stores/purchasingReportStore.ts`

Same pattern as `salesReportStore.ts` / `piutangReportStore.ts`.

### 4.7 API Integration

#### Types — `src/models/reports.ts`

```typescript
export interface PurchasingReportRow {
  InvoiceCode: string
  InvoiceDate: string
  SupplierName: string
  WarehouseName: string
  Total: number
  Disc: number
  Tax: number
  GrandTotal: number
  PostingStok: string
}

export interface PurchasingReportSummary {
  GrandTotalPurchase: number
  TotalInvoice: number
}

export interface PurchasingReportResponse {
  PeriodFrom: string
  PeriodTo: string
  GeneratedAt: string
  Summary: PurchasingReportSummary
  Rows: PurchasingReportRow[]
}
```

#### API function — `src/api/reportsApi.ts`

```typescript
export async function fetchPurchasingReport(): Promise<PurchasingReportResponse> {
  const { data } = await httpClient.get<ApiResponse<PurchasingReportResponse>>('/api/reports/purchasing')
  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load purchasing report.')
  }
  return data.Data
}
```

### 4.8 Loading State

Same as M9/M10: DataTable loading overlay + Refresh button spinner.

### 4.9 Error State

PrimeVue `Message` above Card on API failure.

### 4.10 Empty State

```html
<div class="purchasing-report__empty">
  <i class="pi pi-inbox purchasing-report__empty-icon" />
  <p>No purchase invoices found for this period.</p>
</div>
```

---

## 5. Verification

### 5.1 Backend Tests

#### Manual API checks

```powershell
curl.exe -o NUL -w "%{http_code}" http://localhost:5056/api/reports/purchasing
curl.exe http://localhost:5056/api/reports/purchasing -H "Authorization: Bearer <token>"
```

#### Structural checks

| # | Check | Expected |
| --- | --- | --- |
| 1 | Period | Current calendar month |
| 2 | Void invoices excluded | DAL handles via SQL |
| 3 | PostingStok values | Only `SUDAH` or `BELUM` |
| 4 | Summary.GrandTotalPurchase | Equals sum of row GrandTotal |
| 5 | Summary.TotalInvoice | Equals row count |
| 6 | MediatR pipeline | No DAL in controller |

#### Optional unit test

**File:** `btr.test/ReportingContext/PurchasingReportDalTest.cs`

- Mock `IInvoiceViewDal` with known rows
- Assert summary totals
- Assert PostingStok passthrough
- Assert current month period boundaries

### 5.2 Frontend Tests

```powershell
npm run build
```

| # | Check | Expected |
| --- | --- | --- |
| 1 | Route `/reports/purchasing` | Loads |
| 2 | 9 columns | All PF1 columns present |
| 3 | PostingStok styling | SUDAH/BELUM visually distinct |
| 4 | Period label | Current month displayed |
| 5 | Summary bar | Grand Total Purchase + Total Invoice |
| 6 | Currency columns | Formatted correctly |

### 5.3 Purchasing Summary Validation

Compare against desktop PF1 (`InvoiceInfoForm`) for same period:

| Check | Method |
| --- | --- |
| Row count matches desktop | Open PF1 with current month in BTR Desktop; compare invoice count |
| Grand total matches desktop | Sum GrandTotal column in desktop grid vs API `Summary.GrandTotalPurchase` |
| PostingStok values | Spot-check 2–3 invoices against desktop PF1 PostingStok column |
| Void exclusion | Confirm voided invoices absent in both |

No dashboard reconciliation — standalone validation against desktop reference.

---

## 6. Risks

| Risk | Severity | Mitigation |
| --- | --- | --- |
| **Period handling** | Low | Reuse `SalesReportDal.CurrentMonthPeriode()` pattern exactly. Same `ITglJamDal` source. |
| **Purchasing terminology** | Low | Page title "Purchasing Report"; column "Invoice"; subtitle mentions "purchase invoices". No "PO" language. |
| **PostingStok visibility** | Low | Read-only column from DAL. No link to PT2. Badge styling only. Document in UI that values reflect stock posting status. |
| **122-day desktop validation** | Low | Not applied in portal V1 — fixed current month only. |

---

## 7. Implementation Checklist

### Backend

- [ ] Create `IPurchasingReportDal.cs`
- [ ] Create `GetPurchasingReportQuery.cs`
- [ ] Create `PurchasingReportDal.cs`
- [ ] Update `.csproj` files
- [ ] Create `PurchasingReportController.cs`
- [ ] Register DI
- [ ] Build solution

### Frontend

- [ ] Add types to `reports.ts`
- [ ] Add `fetchPurchasingReport()` to `reportsApi.ts`
- [ ] Create `purchasingReportStore.ts`
- [ ] Create `PurchasingReportView.vue`
- [ ] Add route and sidebar entry
- [ ] Run `npm run build`

### Verification

- [ ] Desktop PF1 reconciliation (§5.3)
- [ ] PostingStok values correct
- [ ] All existing reports/dashboard still work

---

## 8. Files Summary

| Layer | File | Action |
| --- | --- | --- |
| Application | `ReportingContext/PurchasingReportAgg/Contracts/IPurchasingReportDal.cs` | Create |
| Application | `ReportingContext/PurchasingReportAgg/Queries/GetPurchasingReportQuery.cs` | Create |
| Infrastructure | `ReportingContext/PurchasingReportAgg/PurchasingReportDal.cs` | Create |
| Portal API | `Controllers/Reports/PurchasingReportController.cs` | Create |
| Portal API | `Configurations/InfrastructurePortalExtensions.cs` | Modify |
| Portal API | `Configurations/PortalPresentationExtensions.cs` | Modify |
| Frontend | `src/models/reports.ts` | Modify |
| Frontend | `src/api/reportsApi.ts` | Modify |
| Frontend | `src/stores/purchasingReportStore.ts` | Create |
| Frontend | `src/views/reports/PurchasingReportView.vue` | Create |
| Frontend | `src/router/index.ts` | Modify |
| Frontend | `src/layouts/MainLayout.vue` | Modify |

**Prerequisite:** M10 `ReportSummaryBar.vue` (reuse unchanged).
