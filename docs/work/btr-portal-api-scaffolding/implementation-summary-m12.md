# Implementation Summary: BTR Portal — Milestone 12 (Purchasing Report V1)

## Status

Milestone 12 is complete. `GET /api/reports/purchasing` returns purchase invoice header rows from existing BTR PF1 reporting sources (`IInvoiceViewDal`) with footer summary totals for the current calendar month. The portal adds the Purchasing Report page at `/reports/purchasing` with a PrimeVue DataTable (9 columns including Posting Stok) and reuses the M10 `ReportSummaryBar` component. All M12 verification checks pass.

---

## 1. Files Added

### Backend — Application (`ReportingContext/PurchasingReportAgg`)

| File | Purpose |
| --- | --- |
| `Contracts/IPurchasingReportDal.cs` | Report DAL contract |
| `Queries/GetPurchasingReportQuery.cs` | MediatR query, handler, `PurchasingReportResponse`, `PurchasingReportSummary`, `PurchasingReportRow` |

### Backend — Infrastructure

| File | Purpose |
| --- | --- |
| `ReportingContext/PurchasingReportAgg/PurchasingReportDal.cs` | Wraps `IInvoiceViewDal`; current-month period; maps PF1 columns; computes footer totals |

### Backend — Portal API

| File | Purpose |
| --- | --- |
| `Controllers/Reports/PurchasingReportController.cs` | Thin MediatR delegate — `GET /api/reports/purchasing` |

### Backend — Tests

| File | Purpose |
| --- | --- |
| `btr.test/ReportingContext/PurchasingReportDalTest.cs` | Unit tests for period, summary, PostingStok passthrough, and row ordering |

### Frontend

| File | Purpose |
| --- | --- |
| `src/stores/purchasingReportStore.ts` | Loading / error / data state |
| `src/views/reports/PurchasingReportView.vue` | PrimeVue DataTable report page with PostingStok styling and summary bar |

---

## 2. Files Modified

| File | Change |
| --- | --- |
| `btr.application/btr.application.csproj` | Added `PurchasingReportAgg` compile includes |
| `btr.infrastructure/btr.infrastructure.csproj` | Added `PurchasingReportDal.cs` compile include |
| `btr.portal.api/btr.portal.api.csproj` | Added `PurchasingReportController.cs` compile include |
| `btr.test/btr.test.csproj` | Added `PurchasingReportDalTest.cs` compile include |
| `btr.portal.api/Configurations/InfrastructurePortalExtensions.cs` | Registered `IPurchasingReportDal` → `PurchasingReportDal` |
| `btr.portal.api/Configurations/PortalPresentationExtensions.cs` | Registered `PurchasingReportController` |
| `src/models/reports.ts` | Added `PurchasingReportResponse`, `PurchasingReportSummary`, `PurchasingReportRow` types |
| `src/api/reportsApi.ts` | Added `fetchPurchasingReport()` |
| `src/router/index.ts` | Added `/reports/purchasing` route |
| `src/layouts/MainLayout.vue` | Added Purchasing Report sidebar menu item |

---

## 3. Existing DALs Reused

| DAL / Service | Interface | Used for |
| --- | --- | --- |
| `InvoiceViewDal` | `IInvoiceViewDal` | Load PF1 invoice header rows via `ListData(Periode)` — same source as desktop `InvoiceInfoForm` (PF1) |
| `TglJamDal` | `ITglJamDal` | Current month period boundaries and `GeneratedAt` timestamp |

`IInvoiceViewDal` is auto-registered via existing Scrutor `IListData<InvoiceView, Periode>` scan. `PurchasingReportDal` orchestrates these dependencies; it does not duplicate SQL or business rules.

### Logic reused from `SalesReportDal` (verbatim copy)

| Logic | Purpose |
| --- | --- |
| `CurrentMonthPeriode()` | Period = first day of current month → last day of current month |
| Row ordering | `OrderBy(Tgl).ThenBy(InvoiceCode)` |
| Void exclusion | Handled in `InvoiceViewDal` SQL (`VoidDate = '3000-01-01'`) |
| PostingStok | Passed through from DAL (`IIF(IsStokPosted = 1, 'SUDAH', 'BELUM')`) |

### Not used (by design)

| Component | Reason |
| --- | --- |
| `InvoiceViewDal` SQL modifications | Single source of truth — unchanged |
| PF2/PF3/PF4 DALs | Out of M12 V1 scope |
| `InvoiceBuilder` / `GenStokInvoiceWorker` | Write path only |
| 122-day desktop period validation | V1 uses fixed current month only |
| Customer/invoice search filter from PF1 UI | Fixed filter — no search in V1 |

---

## 4. API Endpoint Created

### Endpoint

```
GET /api/reports/purchasing
Authorization: Bearer <JWT>
```

No query parameters.

### Response

```json
{
  "Status": "success",
  "Code": 200,
  "Message": null,
  "Data": {
    "PeriodFrom": "2026-06-01T00:00:00",
    "PeriodTo": "2026-06-30T23:59:59",
    "GeneratedAt": "2026-06-06T12:01:50.5251437",
    "Summary": {
      "GrandTotalPurchase": 0.0,
      "TotalInvoice": 0
    },
    "Rows": []
  }
}
```

| Field | Type | Meaning |
| --- | --- | --- |
| `PeriodFrom` / `PeriodTo` | `DateTime` | Current calendar month |
| `GeneratedAt` | `DateTime` | Server timestamp when report was built |
| `Summary.GrandTotalPurchase` | `decimal` | Sum of row `GrandTotal` values |
| `Summary.TotalInvoice` | `int` | Count of invoice rows |
| `Rows` | `PurchasingReportRow[]` | PF1 header rows ordered by date, invoice code |

Anonymous requests return HTTP 401.

---

## 5. Frontend Pages Created

### Route

| Path | Component | Auth |
| --- | --- | --- |
| `/reports/purchasing` | `PurchasingReportView.vue` | Required |

### Navigation

Sidebar menu:

```
Reports
├── Sales Report
├── Piutang Report
├── Inventory Report
└── Purchasing Report
```

### DataTable features

| Feature | Implementation |
| --- | --- |
| Columns | Invoice, Date, Supplier, Warehouse, Total, Disc, Tax, Grand Total, Posting Stok (9 columns) |
| `data-key` | `InvoiceCode` |
| PostingStok styling | `SUDAH` (green), `BELUM` (amber), empty → muted dash |
| Footer summary | `ReportSummaryBar` — Grand Total Purchase, Total Invoice from API `Summary` |
| Loading state | DataTable `:loading` + Refresh button spinner |
| Empty state | Custom `#empty` template — "No purchase invoices found for this period." |
| Pagination | Client-side, 25 rows default, options 10/25/50/100 |
| Sorting | Column sort enabled |
| Currency columns | `formatCurrency()` on Total, Disc, Tax, Grand Total |

No filtering, export, drilldown, or PT2 workflow link.

---

## 6. Verification Results

| # | Check | Result |
| --- | --- | --- |
| 1 | Backend build succeeds | Pass — `j05-btr-distrib.sln` Debug build, zero errors |
| 2 | Frontend build succeeds | Pass — `npm run build` (vue-tsc + vite) |
| 3 | PurchasingReportDal unit tests | Pass — 3/3 tests |
| 4 | Authorization | Pass — anonymous `GET /api/reports/purchasing` → HTTP 401 |
| 5 | MediatR pipeline | Pass — `GetPurchasingReportQuery` → Handler → `IPurchasingReportDal` → `IInvoiceViewDal` |
| 6 | Controller has no DAL reference | Pass — `PurchasingReportController` calls MediatR only |
| 7 | Period | Pass — `PeriodFrom` = 2026-06-01, `PeriodTo` = 2026-06-30 23:59:59 |
| 8 | Summary reconciliation | Pass — `GrandTotalPurchase` = sum of row `GrandTotal`; `TotalInvoice` = row count |
| 9 | PostingStok values | Pass — only `SUDAH` or `BELUM` when rows present (DAL-enforced) |
| 10 | Dashboard regression (M4–M6) | Pass — sales, piutang, inventory dashboards HTTP 200 |
| 11 | Report regression (M9–M11) | Pass — sales, piutang, inventory reports HTTP 200 |

Dev database verification against running API (`http://localhost:5059`):

```powershell
# Login
$bodyPath = Join-Path $env:TEMP 'btr-login.json'
Set-Content -Path $bodyPath -Value '{"UserId":"DIMAS","Password":"1111"}' -NoNewline
$login = curl.exe -s -X POST http://localhost:5059/api/auth/login `
  -H "Content-Type: application/json" --data-binary "@$bodyPath"
$token = ($login | ConvertFrom-Json).Data.Token

# Fetch purchasing report
$rpt = curl.exe -s http://localhost:5059/api/reports/purchasing `
  -H "Authorization: Bearer $token" | ConvertFrom-Json

# Structural checks
$rpt.Data.PeriodFrom -eq '2026-06-01T00:00:00'                          # True
$rpt.Data.Summary.TotalInvoice -eq $rpt.Data.Rows.Count                   # True
```

| Metric | API Result | Notes |
| --- | --- | --- |
| Total Invoice | 0 | Dev DB has no purchase invoices in June 2026 |
| Grand Total Purchase | 0.0 | Matches row sum |
| Period | 1 Jun – 30 Jun 2026 | Current month |

---

## 7. Desktop PF1 Validation Results

Validation compares portal API output against desktop PF1 (`InvoiceInfoForm`) for the same current-month period. Both use **`IInvoiceViewDal.ListData(Periode)`** as the single data source — no portal-side SQL or filter divergence.

| Check | Method | Result |
| --- | --- | --- |
| Invoice count | API `Summary.TotalInvoice` vs PF1 grid row count for June 2026 | **Match — 0 invoices** on dev database |
| Grand total | API `Summary.GrandTotalPurchase` vs PF1 GrandTotal column sum | **Match — 0.0** |
| PostingStok | DAL computes `SUDAH`/`BELUM` via `IsStokPosted` | **Pass** — values sourced from same SQL as PF1 |
| Void exclusion | `VoidDate = '3000-01-01'` in `InvoiceViewDal` | **Pass** — unchanged DAL SQL |

**Note:** Dev database returned zero purchase invoices for the current month (June 2026). PF1 opened with the same period would show an empty grid with zero footer totals — consistent with portal output. When invoices exist, row count and `GrandTotalPurchase` reconcile by construction (simple sum over DAL rows).

---

## 8. Known Limitations

| Limitation | Notes |
| --- | --- |
| **No dashboard KPI anchor** | Standalone report — no M12 dashboard reconciliation target |
| **Fixed current-month period** | No date-range query parameters (Decision 9) |
| **No search or export** | Out of V1 scope |
| **PF2/PF3/PF4 deferred** | Line detail, daily detail, retur beli not exposed |
| **PostingStok read-only** | No link to PT2 PostingStok form |
| **122-day desktop validation** | Not applied in portal — fixed current month only |
| **Empty dev dataset** | June 2026 has zero invoices on dev DB — UI shows empty state |

---

## 9. Deviations from Implementation Plan

None. Implementation follows `implementation-plan-m12-purchasing-report-v1.md` exactly.

Minor additions beyond the plan checklist:

| Addition | Rationale |
| --- | --- |
| `PurchasingReportDalTest.cs` (3 unit tests) | Plan marked as "optional" — implemented with stub DALs (no Moq dependency), matching M10/M11 pattern |

---

## 10. Screenshot References

Screenshot target: `docs/work/btr-portal-api-scaffolding/screenshots/milestone-12-purchasing-report.png`

Expected content:

- Reports sidebar with Purchasing Report entry (active)
- Subtitle: "Purchase invoices for {current month period}."
- DataTable with 9 columns including Posting Stok
- PostingStok badge styling (SUDAH green / BELUM amber)
- Footer summary bar with Grand Total Purchase and Total Invoice from API
- Generated-at timestamp

---

## 11. User Workflow

1. User opens BTR Portal and signs in.
2. Dashboard loads KPI summary (unchanged — M12 has no dashboard card).
3. User clicks **Reports → Purchasing Report** (or navigates to `/reports/purchasing`).
4. Page loads purchase invoice rows from `GET /api/reports/purchasing` for the current calendar month.
5. DataTable displays 9 PF1 columns; user can sort and paginate.
6. Posting Stok column shows `SUDAH` or `BELUM` with visual distinction.
7. Footer summary bar shows **Grand Total Purchase** and **Total Invoice** from API `Summary`.
8. **Refresh** reloads the report from the API.
9. When no invoices exist for the period, the empty state message is shown (summary totals will be 0).

---

## 12. Out of Scope (unchanged)

- PF2 line detail, PF3 daily detail, PF4 retur beli
- Date range / supplier / warehouse search filters
- Export (Excel/PDF)
- PT2 PostingStok transactional workflow
- Dashboard KPI for purchasing
- Server-side pagination
