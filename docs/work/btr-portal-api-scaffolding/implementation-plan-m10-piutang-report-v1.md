# Implementation Plan: M10 — Piutang Report V1

## Document Status

| Field | Value |
| --- | --- |
| Milestone | M10 — Piutang Report V1 |
| Authoritative requirements | `portal-analysis-m10-m12-final.md` |
| Reference pattern | M9 Sales Report V1 (`SalesReportAgg`) |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |

---

## 1. Goal

Expose receivable detail behind the M5 Piutang Dashboard KPIs (`Total Piutang`, `Total Customer`). Users inspect open receivable rows in a PrimeVue DataTable with footer summary totals that **must reconcile** with dashboard KPIs.

**Out of scope:** date-range params, search, export, drilldown, Show Paid toggle, deferred columns (Wilayah, Bayar Tunai/Giro, Retur, Potongan, Materai/Admin).

---

## 2. Architecture Overview

```text
PiutangReportView.vue
    ↓ fetchPiutangReport()
piutangReportStore (Pinia)
    ↓ GET /api/reports/piutang
PiutangReportController
    ↓ MediatR
GetPiutangReportHandler
    ↓ IPiutangReportDal
PiutangReportDal (wrapper)
    ↓ IPiutangSalesWilayahDal.ListData(Periode)
PiutangSalesWilayahDal (existing desktop DAL)
    ↓ SQL
BTR_Piutang / BTR_Faktur / …
```

Footer totals are computed in `PiutangReportDal` using the **same logic** as `DashboardPiutangDal` — not by summing visible DataTable rows on the client.

---

## 3. Backend Implementation

### 3.1 ReportingContext Structure

Create aggregate folder under Application and Infrastructure:

```text
btr.application/ReportingContext/PiutangReportAgg/
├── Contracts/
│   └── IPiutangReportDal.cs
└── Queries/
    └── GetPiutangReportQuery.cs

btr.infrastructure/ReportingContext/PiutangReportAgg/
└── PiutangReportDal.cs

btr.portal.api/Controllers/Reports/
└── PiutangReportController.cs
```

Register new `.cs` files in `btr.application.csproj` and `btr.infrastructure.csproj` (same explicit `<Compile Include=…>` pattern as `SalesReportAgg`).

### 3.2 Query Objects

**File:** `GetPiutangReportQuery.cs`

| Type | Purpose |
| --- | --- |
| `GetPiutangReportQuery` | Empty `IRequest<PiutangReportResponse>` — no query parameters (fixed defaults) |
| `GetPiutangReportHandler` | Injects `IPiutangReportDal`; delegates to `_dal.GetReport()` |

Handler pattern — copy from `GetSalesReportHandler`:

```csharp
public Task<PiutangReportResponse> Handle(
    GetPiutangReportQuery request,
    CancellationToken cancellationToken)
{
    return Task.FromResult(_dal.GetReport());
}
```

MediatR auto-registers the handler via existing `AddApplicationPortal` assembly scan. No manual handler registration.

### 3.3 Response DTOs

**File:** `GetPiutangReportQuery.cs` (same file as query/handler — M9 convention)

#### `PiutangReportResponse`

| Property | Type | Description |
| --- | --- | --- |
| `PeriodFrom` | `DateTime` | `2000-01-01` (open receivables window start) |
| `PeriodTo` | `DateTime` | Today (`ITglJamDal.Now.Date`) |
| `GeneratedAt` | `DateTime` | Server timestamp |
| `Summary` | `PiutangReportSummary` | Footer totals — must reconcile with M5 dashboard |
| `Rows` | `List<PiutangReportRow>` | Filtered, mapped rows |

#### `PiutangReportSummary`

| Property | Type | Calculation | Dashboard field |
| --- | --- | --- | --- |
| `TotalPiutang` | `decimal` | `Sum(KurangBayar)` where `KurangBayar > 1` | `TotalPiutang` |
| `TotalCustomer` | `int` | Distinct customer count via `ResolveCustomerKey` | `TotalCustomer` |

### 3.4 Row DTOs

#### `PiutangReportRow`

Map from `PiutangSalesWilayahDto`. Expose only approved columns in approved order:

| # | Property | Source (`PiutangSalesWilayahDto`) | Notes |
| - | --- | --- | --- |
| 1 | `CustomerName` | `CustomerName` | |
| 2 | `SalesName` | `SalesName` | |
| 3 | `FakturCode` | `FakturCode` | |
| 4 | `FakturDate` | `FakturDate` | `.Date` normalization |
| 5 | `JatuhTempo` | `JatuhTempo` | `.Date` normalization; prominent for M14 prep |
| 6 | `TotalJual` | `TotalJual` | |
| 7 | `KurangBayar` | `KurangBayar` | |

Do **not** expose: `WilayahName`, `CustomerCode`, `Alamat`, `BayarTunai`, `BayarGiro`, `Retur`, `Potongan`, `MateraiAdmin`.

### 3.5 DAL Wrapper Design

**File:** `PiutangReportDal.cs`  
**Interface:** `IPiutangReportDal` with single method `PiutangReportResponse GetReport()`.

#### Dependencies

| Dependency | Purpose |
| --- | --- |
| `IPiutangSalesWilayahDal` | Load raw receivable rows (auto-registered via Scrutor `IListData<,>` scan) |
| `ITglJamDal` | Period end date and `GeneratedAt` |

#### `GetReport()` algorithm

1. **Period:** `OpenReceivablesPeriode()` — copy verbatim from `DashboardPiutangDal`:
   ```csharp
   var today = _tglJamDal.Now.Date;
   return new Periode(new DateTime(2000, 1, 1), today);
   ```
2. **Load:** `_piutangSalesWilayahDal.ListData(periode)?.ToList() ?? empty`
3. **Filter:** `rows.Where(r => r.KurangBayar > 1).ToList()` — fixed open-balance filter
4. **Summary:** Compute using same logic as `DashboardPiutangDal.GetSummary()`:
   - `TotalPiutang = outstanding.Sum(r => r.KurangBayar)`
   - `TotalCustomer = outstanding.Select(ResolveCustomerKey).Where(key => key.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).Count()`
5. **Rows:** Map filtered rows, order by `CustomerName`, then `FakturDate`, then `FakturCode`
6. **Return:** `PiutangReportResponse` with period, summary, rows, `GeneratedAt`

#### `ResolveCustomerKey` helper

Copy **verbatim** from `DashboardPiutangDal`:

```csharp
private static string ResolveCustomerKey(PiutangSalesWilayahDto row)
{
    if (row is null) return string.Empty;
    if (!string.IsNullOrWhiteSpace(row.CustomerCode))
        return row.CustomerCode.Trim();
    return row.CustomerName?.Trim() ?? string.Empty;
}
```

**Do not** extract to a shared class in M10 unless duplication becomes painful — inline copy matches dashboard behavior exactly.

#### `MapRow` helper

```csharp
private static PiutangReportRow MapRow(PiutangSalesWilayahDto row) => new PiutangReportRow
{
    CustomerName = row.CustomerName ?? string.Empty,
    SalesName = row.SalesName ?? string.Empty,
    FakturCode = row.FakturCode ?? string.Empty,
    FakturDate = row.FakturDate.Date,
    JatuhTempo = row.JatuhTempo.Date,
    TotalJual = row.TotalJual,
    KurangBayar = row.KurangBayar,
};
```

### 3.6 Existing DAL Integration Strategy

| Asset | Registration | Usage |
| --- | --- | --- |
| `IPiutangSalesWilayahDal` → `PiutangSalesWilayahDal` | Auto via Scrutor `IListData<PiutangSalesWilayahDto, Periode>` scan | `ListData(periode)` — no SQL changes |
| `ITglJamDal` | Explicit in `ApplicationPortalExtensions` | Period + timestamp |

**Rules:**

- Do not modify `PiutangSalesWilayahDal` SQL.
- Do not add new SQL to the portal layer.
- Do not reimplement payment/element subqueries — they remain inside the desktop DAL; deferred columns are simply not mapped.

### 3.7 API Contract

#### Endpoint

```
GET /api/reports/piutang
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
    "PeriodFrom": "2000-01-01T00:00:00",
    "PeriodTo": "2026-06-06T00:00:00",
    "GeneratedAt": "2026-06-06T14:30:00",
    "Summary": {
      "TotalPiutang": 125000000.0,
      "TotalCustomer": 42
    },
    "Rows": [
      {
        "CustomerName": "PT Example",
        "SalesName": "BUDI",
        "FakturCode": "FK-001",
        "FakturDate": "2026-05-15T00:00:00",
        "JatuhTempo": "2026-06-15T00:00:00",
        "TotalJual": 5000000.0,
        "KurangBayar": 3000000.0
      }
    ]
  }
}
```

#### Error responses

| Condition | HTTP | Body |
| --- | --- | --- |
| Missing/invalid JWT | 401 | Existing `JwtAuthenticationFilter` behavior |
| Unhandled exception | 500 | `GlobalExceptionFilter` → `ApiResponse` error shape |

### 3.8 Controller Design

**File:** `PiutangReportController.cs`

```csharp
[Authorize]
[RoutePrefix("api/reports/piutang")]
public class PiutangReportController : ApiController
{
    private readonly IMediator _mediator;

    [HttpGet, Route("")]
    public async Task<IHttpActionResult> Get()
    {
        var result = await _mediator.Send(new GetPiutangReportQuery());
        return Ok(ApiResponse<PiutangReportResponse>.Success(result));
    }
}
```

Controller must contain **no** DAL calls, filtering, or summary logic.

### 3.9 DI Registrations

#### `InfrastructurePortalExtensions.cs`

Add after `ISalesReportDal` registration:

```csharp
services.AddScoped<IPiutangReportDal, PiutangReportDal>();
```

#### `PortalPresentationExtensions.cs`

Add:

```csharp
services.AddTransient<Controllers.Reports.PiutangReportController>();
```

#### `WebApiConfig.cs`

No route changes needed — attribute routing on controller.

---

## 4. Frontend Implementation

### 4.1 Route Structure

**File:** `src/router/index.ts`

Add child route under authenticated layout:

| Path | Name | Component |
| --- | --- | --- |
| `/reports/piutang` | `piutang-report` | `PiutangReportView.vue` (lazy import) |

### 4.2 Navigation

**File:** `src/layouts/MainLayout.vue`

Add to Reports submenu (after Sales Report):

```typescript
{
  label: 'Piutang Report',
  icon: 'pi pi-wallet',
  command: () => router.push('/reports/piutang'),
  class: route.path === '/reports/piutang' ? 'layout-menu-item--active' : '',
},
```

### 4.3 View Structure

**File:** `src/views/reports/PiutangReportView.vue`

Mirror `SalesReportView.vue` layout with these differences:

```text
┌─────────────────────────────────────────────────────┐
│ Header: "Piutang Report"                            │
│ Subtitle: "Open receivables from {PeriodFrom} – …"  │
│                                    [Refresh button] │
├─────────────────────────────────────────────────────┤
│ Error Message (PrimeVue Message, if error)          │
├─────────────────────────────────────────────────────┤
│ Card                                                │
│   DataTable (7 columns)                             │
│   ReportSummaryBar (NEW shared component)            │
│   Generated-at timestamp                            │
└─────────────────────────────────────────────────────┘
```

**Header copy:**

- Title: `Piutang Report`
- Subtitle when loaded: `Open receivables from {periodLabel}.`
- Subtitle placeholder: `Open receivables (outstanding balance only).`

**Period label:** Format `PeriodFrom` – `PeriodTo` via `formatDate()`. Display full range including year 2000 start — this is intentional and matches dashboard semantics.

### 4.4 DataTable Configuration

| Setting | Value |
| --- | --- |
| Component | PrimeVue `DataTable` |
| `data-key` | `FakturCode` |
| Paginator | Client-side, default 25 rows |
| `rows-per-page-options` | `[10, 25, 50, 100]` |
| `striped-rows` | `true` |
| `removable-sort` | `true` |
| `:loading` | Bound to store `loading` |

#### Columns (in order)

| Field | Header | Sortable | Formatter |
| --- | --- | --- | --- |
| `CustomerName` | Customer | Yes | — |
| `SalesName` | Sales | Yes | — |
| `FakturCode` | Faktur | Yes | — |
| `FakturDate` | Tanggal | Yes | `formatDate()` |
| `JatuhTempo` | Jatuh Tempo | Yes | `formatDate()` — apply subtle emphasis class for M14 prep |
| `TotalJual` | Total Jual | Yes | `formatCurrency()` |
| `KurangBayar` | Kurang Bayar | Yes | `formatCurrency()` |

**Do not** add column filters, global search, or export buttons.

### 4.5 Footer Summary Design

**New shared component:** `src/components/reports/ReportSummaryBar.vue`

Introduce in M10; M11/M12 reuse unchanged.

#### Props

```typescript
interface SummaryItem {
  label: string
  value: string  // pre-formatted display string
}

defineProps<{
  items: SummaryItem[]
}>()
```

#### Layout

Horizontal bar below DataTable, above generated-at timestamp:

```text
┌──────────────────────────────────────────────────────────┐
│  Total Piutang: Rp 125.000.000    Total Customer: 42    │
└──────────────────────────────────────────────────────────┘
```

Style: flex row, gap between items, muted background (`var(--p-surface-100)` or equivalent), padding `0.75rem 1rem`, font-weight 600 on values.

#### M10 usage in `PiutangReportView.vue`

```typescript
const summaryItems = computed(() => {
  if (!piutangReport.report?.Summary) return []
  return [
    { label: 'Total Piutang', value: formatCurrency(piutangReport.report.Summary.TotalPiutang) },
    { label: 'Total Customer', value: String(piutangReport.report.Summary.TotalCustomer) },
  ]
})
```

**Critical:** Display `Summary` from API response — do **not** compute totals client-side from `Rows`.

### 4.6 Store Design

**File:** `src/stores/piutangReportStore.ts`

Copy `salesReportStore.ts` pattern:

| State | Type | Purpose |
| --- | --- | --- |
| `report` | `PiutangReportResponse \| null` | Full API payload including `Summary` |
| `loading` | `boolean` | Request in flight |
| `error` | `string \| null` | User-facing error message |

| Action | Behavior |
| --- | --- |
| `loadReport()` | Sets loading, clears error, calls `fetchPiutangReport()`, stores result |
| `reset()` | Clears all state (for logout/testing) |

Use `getApiErrorMessage(err, 'Failed to load piutang report.')` for errors.

### 4.7 API Integration

#### Types — `src/models/reports.ts`

Add:

```typescript
export interface PiutangReportRow {
  CustomerName: string
  SalesName: string
  FakturCode: string
  FakturDate: string
  JatuhTempo: string
  TotalJual: number
  KurangBayar: number
}

export interface PiutangReportSummary {
  TotalPiutang: number
  TotalCustomer: number
}

export interface PiutangReportResponse {
  PeriodFrom: string
  PeriodTo: string
  GeneratedAt: string
  Summary: PiutangReportSummary
  Rows: PiutangReportRow[]
}
```

#### API function — `src/api/reportsApi.ts`

```typescript
export async function fetchPiutangReport(): Promise<PiutangReportResponse> {
  const { data } = await httpClient.get<ApiResponse<PiutangReportResponse>>('/api/reports/piutang')
  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load piutang report.')
  }
  return data.Data
}
```

### 4.8 Loading State

| Element | Behavior |
| --- | --- |
| DataTable `:loading` | `true` while `loadReport()` in progress |
| Refresh button | `:loading="piutangReport.loading"` — disables spinner on button |
| Initial load | `onMounted(() => void piutangReport.loadReport())` |

Table skeleton/spinner handled by PrimeVue DataTable loading overlay.

### 4.9 Error State

| Element | Behavior |
| --- | --- |
| PrimeVue `Message` | `severity="error"`, `:closable="false"`, shown when `piutangReport.error` is set |
| DataTable | Still rendered (empty) — error message appears above Card |
| Refresh | User can retry via Refresh button |

### 4.10 Empty State

Custom `#empty` template inside DataTable:

```html
<div class="piutang-report__empty">
  <i class="pi pi-inbox piutang-report__empty-icon" />
  <p>No open receivables found.</p>
</div>
```

Show when `Rows.length === 0` and no error. Summary bar still displays (totals will be 0).

---

## 5. Verification

### 5.1 Backend Tests

**Location:** Manual verification + optional unit test in `btr.test` if time permits.

#### Manual API checks

```powershell
# Build
MSBuild.exe src\j05-btr-distrib\j05-btr-distrib.sln /p:Configuration=Debug

# Anonymous → 401
curl.exe -o NUL -w "%{http_code}" http://localhost:5056/api/reports/piutang

# Authenticated
curl.exe http://localhost:5056/api/reports/piutang -H "Authorization: Bearer <token>"
```

#### Structural checks

| # | Check | Expected |
| --- | --- | --- |
| 1 | Controller has no DAL reference | Pass |
| 2 | MediatR pipeline | `GetPiutangReportQuery` → Handler → `IPiutangReportDal` → `IPiutangSalesWilayahDal` |
| 3 | Response includes `Summary` | Both fields populated |
| 4 | Rows filtered | All rows have `KurangBayar > 1` |
| 5 | Column count | 7 fields per row, no deferred fields |

#### Optional unit test (recommended)

**File:** `btr.test/ReportingContext/PiutangReportDalTest.cs`

Mock `IPiutangSalesWilayahDal` and `ITglJamDal`:

- Given mixed rows (some `KurangBayar <= 1`), assert only outstanding rows returned
- Assert `Summary.TotalPiutang` equals sum of outstanding `KurangBayar`
- Assert `Summary.TotalCustomer` uses `CustomerCode` when present, falls back to `CustomerName`
- Assert period is `2000-01-01` → mocked today

### 5.2 Frontend Tests

#### Build verification

```powershell
cd src\j05-btr-distrib\btr.portal.web
npm run build
```

Must pass `vue-tsc` + Vite build with zero errors.

#### Manual UI checks

| # | Check | Expected |
| --- | --- | --- |
| 1 | Route `/reports/piutang` loads | Page renders with title |
| 2 | Sidebar navigation | Reports → Piutang Report navigates correctly |
| 3 | Auth guard | Unauthenticated redirect to login |
| 4 | DataTable columns | 7 columns in approved order |
| 5 | Pagination | Client-side paginator works |
| 6 | Sorting | Column sort works |
| 7 | Loading | Spinner during fetch |
| 8 | Error | Simulated API failure shows error message |
| 9 | Empty | Zero rows shows empty state |
| 10 | Summary bar | Displays API `Summary` values |
| 11 | Generated-at | Timestamp shown below summary |

### 5.3 KPI Reconciliation Validation

**Mandatory acceptance test** — run against dev/staging database:

```powershell
# Fetch dashboard
$dash = curl.exe -s http://localhost:5056/api/dashboard/piutang -H "Authorization: Bearer <token>" | ConvertFrom-Json

# Fetch report
$rpt = curl.exe -s http://localhost:5056/api/reports/piutang -H "Authorization: Bearer <token>" | ConvertFrom-Json

# Assert reconciliation
$dash.Data.TotalPiutang -eq $rpt.Data.Summary.TotalPiutang   # must match
$dash.Data.TotalCustomer -eq $rpt.Data.Summary.TotalCustomer # must match
```

| Metric | Dashboard endpoint | Report endpoint | Rule |
| --- | --- | --- | --- |
| Total Piutang | `Data.TotalPiutang` | `Data.Summary.TotalPiutang` | Exact match |
| Total Customer | `Data.TotalCustomer` | `Data.Summary.TotalCustomer` | Exact match |

If mismatch: inspect `PiutangReportDal` filter/summary logic against `DashboardPiutangDal` — do not change business rules; fix wrapper alignment.

---

## 6. Risks

| Risk | Severity | Mitigation |
| --- | --- | --- |
| **Performance — large open receivables** | Medium | Client-side pagination limits DOM rows; full dataset loaded in one API call (same as M9). Monitor row count in dev. Server-side pagination deferred to post-M15. |
| **Data reconciliation drift** | High | Copy dashboard period, filter, and customer-key logic verbatim. Mandatory reconciliation test in §5.3. |
| **Large result sets — memory/time** | Medium | Accept for V1 (matches M9). Document in implementation summary if row count exceeds ~5000. No premature optimization. |

---

## 7. Implementation Checklist

### Backend (in order)

- [ ] Create `IPiutangReportDal.cs`
- [ ] Create `GetPiutangReportQuery.cs` (query, handler, response, summary, row DTOs)
- [ ] Create `PiutangReportDal.cs`
- [ ] Update `btr.application.csproj` and `btr.infrastructure.csproj`
- [ ] Create `PiutangReportController.cs`
- [ ] Register `IPiutangReportDal` in `InfrastructurePortalExtensions.cs`
- [ ] Register controller in `PortalPresentationExtensions.cs`
- [ ] Build solution — zero errors

### Frontend (in order)

- [ ] Add types to `reports.ts`
- [ ] Add `fetchPiutangReport()` to `reportsApi.ts`
- [ ] Create `piutangReportStore.ts`
- [ ] Create `ReportSummaryBar.vue` (shared component)
- [ ] Create `PiutangReportView.vue`
- [ ] Add route in `router/index.ts`
- [ ] Add sidebar menu item in `MainLayout.vue`
- [ ] Run `npm run build`

### Verification

- [ ] API returns 401 for anonymous
- [ ] KPI reconciliation passes (§5.3)
- [ ] Dashboard still loads unchanged
- [ ] Screenshot for implementation summary

---

## 8. Files Summary

| Layer | File | Action |
| --- | --- | --- |
| Application | `ReportingContext/PiutangReportAgg/Contracts/IPiutangReportDal.cs` | Create |
| Application | `ReportingContext/PiutangReportAgg/Queries/GetPiutangReportQuery.cs` | Create |
| Infrastructure | `ReportingContext/PiutangReportAgg/PiutangReportDal.cs` | Create |
| Portal API | `Controllers/Reports/PiutangReportController.cs` | Create |
| Portal API | `Configurations/InfrastructurePortalExtensions.cs` | Modify |
| Portal API | `Configurations/PortalPresentationExtensions.cs` | Modify |
| Frontend | `src/models/reports.ts` | Modify |
| Frontend | `src/api/reportsApi.ts` | Modify |
| Frontend | `src/stores/piutangReportStore.ts` | Create |
| Frontend | `src/components/reports/ReportSummaryBar.vue` | Create |
| Frontend | `src/views/reports/PiutangReportView.vue` | Create |
| Frontend | `src/router/index.ts` | Modify |
| Frontend | `src/layouts/MainLayout.vue` | Modify |
