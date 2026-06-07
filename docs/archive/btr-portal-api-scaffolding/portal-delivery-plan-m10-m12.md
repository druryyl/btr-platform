# Portal Delivery Plan: M10–M12

## Document Status

| Field | Value |
| --- | --- |
| Scope | M10 Piutang Report V1, M11 Inventory Report V1, M12 Purchasing Report V1 |
| Authoritative requirements | `portal-analysis-m10-m12-final.md` |
| Individual plans | `implementation-plan-m10-piutang-report-v1.md`, `implementation-plan-m11-inventory-report-v1.md`, `implementation-plan-m12-purchasing-report-v1.md` |
| Reference pattern | M9 Sales Report V1 |
| Date | 2026-06-06 |

---

## 1. Shared Architecture

M10–M12 extend the M9 report pattern with **footer summary totals**. All three milestones share the same vertical slice architecture.

```text
Vue 3 (PrimeVue DataTable)
    ↓ Axios
btr.portal.api (Web API 2 Controller)
    ↓ MediatR
ReportingContext/{Domain}ReportAgg (Application)
    ↓ I{Domain}ReportDal
{Domain}ReportDal (Infrastructure wrapper)
    ↓ Existing Desktop DAL (IListData)
SQL Server
```

### 1.1 Shared DTO Patterns

| Pattern | Convention | M9 | M10 | M11 | M12 |
| --- | --- | --- | --- | --- | --- |
| Response wrapper | `{Domain}ReportResponse` | ✓ | ✓ | ✓ | ✓ |
| Row DTO | `{Domain}ReportRow` | ✓ | ✓ | ✓ | ✓ |
| Summary DTO | `{Domain}ReportSummary` | — | ✓ | ✓ | ✓ |
| Period fields | `PeriodFrom`, `PeriodTo` | ✓ | ✓ | — (snapshot) | ✓ |
| Timestamp | `GeneratedAt` | ✓ | ✓ | ✓ | ✓ |
| Rows collection | `List<{Domain}ReportRow> Rows` | ✓ | ✓ | ✓ | ✓ |

**Summary DTO fields by milestone:**

| Milestone | Summary properties |
| --- | --- |
| M10 | `TotalPiutang`, `TotalCustomer` |
| M11 | `TotalInventoryValue`, `TotalItem` |
| M12 | `GrandTotalPurchase`, `TotalInvoice` |

All row DTOs are **presentation subsets** of existing desktop DTOs — no new business fields.

### 1.2 Shared Response Patterns

#### API envelope

All endpoints return existing `ApiResponse<T>`:

```json
{
  "Status": "success",
  "Code": 200,
  "Message": null,
  "Data": { /* report response */ }
}
```

#### Endpoint naming

| Milestone | Route |
| --- | --- |
| M9 (existing) | `GET /api/reports/sales` |
| M10 | `GET /api/reports/piutang` |
| M11 | `GET /api/reports/inventory` |
| M12 | `GET /api/reports/purchasing` |

No query parameters on any V1 report endpoint.

#### Controller pattern

- `[Authorize]` + `[RoutePrefix("api/reports/{domain}")]`
- Single `GET` action → `IMediator.Send(new Get{Domain}ReportQuery())`
- Return `Ok(ApiResponse<{Domain}ReportResponse>.Success(result))`

### 1.3 Shared DataTable Components

| Component | Location | Introduced | Reused by |
| --- | --- | --- | --- |
| PrimeVue `DataTable` | Each report view | M9 | M10–M12 |
| PrimeVue `Column` | Each report view | M9 | M10–M12 |
| PrimeVue `Card` | Each report view | M9 | M10–M12 |
| PrimeVue `Message` (error) | Each report view | M9 | M10–M12 |
| PrimeVue `Button` (Refresh) | Each report view | M9 | M10–M12 |

**Shared DataTable settings (all reports):**

| Setting | Value |
| --- | --- |
| Pagination | Client-side |
| Default page size | 25 |
| Page size options | 10, 25, 50, 100 |
| Sorting | Column sort enabled (`removable-sort`) |
| Striped rows | Yes |
| Loading overlay | Bound to store `loading` |
| Empty template | Custom `#empty` with inbox icon |

### 1.4 Shared Footer Summary Components

| Component | Location | Purpose |
| --- | --- | --- |
| `ReportSummaryBar.vue` | `src/components/reports/` | Horizontal summary bar below DataTable |

**Introduced in M10.** M11 and M12 reuse without modification.

```typescript
interface SummaryItem {
  label: string
  value: string  // pre-formatted
}
```

**Rule:** Footer values always come from API `Summary` — never computed client-side from `Rows` (especially critical for M10/M11).

**Layout placement (all reports):**

```text
DataTable
ReportSummaryBar
Generated-at timestamp (formatDateTime)
```

M9 Sales Report does **not** have footer totals — retrofit out of scope.

### 1.5 Shared Pinia Patterns

One store per report, identical structure:

| State | Type | Purpose |
| --- | --- | --- |
| `report` | `{Domain}ReportResponse \| null` | Full API payload |
| `loading` | `boolean` | Request in flight |
| `error` | `string \| null` | User-facing error |

| Action | Behavior |
| --- | --- |
| `loadReport()` | Fetch, set report, handle errors via `getApiErrorMessage` |
| `reset()` | Clear all state |

**Stores:**

| Milestone | Store file | Store ID |
| --- | --- | --- |
| M9 | `salesReportStore.ts` | `salesReport` |
| M10 | `piutangReportStore.ts` | `piutangReport` |
| M11 | `inventoryReportStore.ts` | `inventoryReport` |
| M12 | `purchasingReportStore.ts` | `purchasingReport` |

### 1.6 Shared API Patterns

**File:** `src/api/reportsApi.ts` — one fetch function per report.

**File:** `src/models/reports.ts` — all report types in single module.

**HTTP client:** Existing `httpClient` with JWT interceptor (M2).

**Error handling:** `isApiSuccess()` check + throw with message.

### 1.7 Shared Backend Patterns

| Pattern | Location | Convention |
| --- | --- | --- |
| Aggregate folder | `ReportingContext/{Domain}ReportAgg/` | One per report |
| DAL contract | `Contracts/I{Domain}ReportDal.cs` | Single `GetReport()` method |
| MediatR query | `Queries/Get{Domain}ReportQuery.cs` | Query + Handler + all DTOs in one file (M9 convention) |
| Wrapper DAL | `Infrastructure/ReportingContext/{Domain}ReportAgg/{Domain}ReportDal.cs` | Orchestrates desktop DAL + mapping + summary |
| DI registration | `InfrastructurePortalExtensions.cs` | Explicit `AddScoped<I{Domain}ReportDal, {Domain}ReportDal>()` |
| Controller registration | `PortalPresentationExtensions.cs` | Explicit `AddTransient<{Domain}ReportController>()` |

Desktop DALs (`IPiutangSalesWilayahDal`, `IStokBalanceViewDal`, `IInvoiceViewDal`) remain auto-registered via Scrutor — do not register manually.

### 1.8 Shared Formatters

**File:** `src/services/formatters.ts` (existing from M9)

| Function | Used for |
| --- | --- |
| `formatDate()` | Date columns |
| `formatCurrency()` | Monetary columns |
| `formatDateTime()` | Generated-at timestamp |

M11 Qty column uses plain integer display (no formatter).

---

## 2. Recommended Delivery Order

### Order: M10 → M11 → M12

| Order | Milestone | Rationale |
| --- | --- | --- |
| **1** | M10 Piutang Report | Dashboard traceability (M5); introduces `ReportSummaryBar` shared component; Finance reporting pair with M9 Sales Report; product decisions fully resolved |
| **2** | M11 Inventory Report | Dashboard traceability (M6); reuses M10 footer component; simpler 5-column grid; Kartu Stok explicitly deferred |
| **3** | M12 Purchasing Report | No dashboard dependency; structurally mirrors M9; lowest cross-milestone risk; benefits from established report page patterns |

### Why not M12 first?

M12 is the simplest structurally (mirrors M9), but delivering M10 first:

1. Establishes the footer summary pattern that M11/M12 reuse
2. Validates KPI reconciliation workflow (highest business value)
3. Keeps Finance reports (Sales + Piutang) adjacent in delivery timeline

### Milestone dependencies

```text
M9 (complete)
    ↓
M10 (creates ReportSummaryBar)
    ↓
M11 (reuses ReportSummaryBar, independent backend)
    ↓
M12 (reuses ReportSummaryBar, independent backend)
```

M11 and M12 backend work has **no dependency** on M10 backend — only the shared frontend component from M10.

---

## 3. Parallelization Analysis

### 3.1 Backend Parallelization

| Task | M10 | M11 | M12 | Can parallelize? |
| --- | --- | --- | --- | --- |
| Application DTOs + Query | ✓ | ✓ | ✓ | **Yes** — independent aggregates |
| Wrapper DAL | ✓ | ✓ | ✓ | **Yes** — different desktop DALs |
| Controller | ✓ | ✓ | ✓ | **Yes** |
| DI registration | ✓ | ✓ | ✓ | **Sequential merge** — same two config files |
| `.csproj` updates | ✓ | ✓ | ✓ | **Sequential merge** |

**Recommendation:**

- **Single implementer:** Deliver M10 → M11 → M12 sequentially (lowest merge conflict risk).
- **Two implementers:** Developer A: M10 + M12 backend; Developer B: M11 backend. Merge DI registrations carefully.
- **Three implementers:** One per milestone backend — feasible but requires coordinated merge of `InfrastructurePortalExtensions.cs`, `PortalPresentationExtensions.cs`, and `.csproj` files.

### 3.2 Frontend Parallelization

| Task | M10 | M11 | M12 | Can parallelize? |
| --- | --- | --- | --- | --- |
| `ReportSummaryBar.vue` | Create | Reuse | Reuse | **M10 must land first** |
| Report view | ✓ | ✓ | ✓ | **Yes** — after M10 component exists |
| Pinia store | ✓ | ✓ | ✓ | **Yes** |
| Types + API function | ✓ | ✓ | ✓ | **Yes** — same files, sequential merge |
| Router + sidebar | ✓ | ✓ | ✓ | **Sequential merge** |

**Recommendation:**

- M10 frontend must complete (or at minimum `ReportSummaryBar.vue`) before M11/M12 frontend starts.
- M11 and M12 views can be built in parallel once shared component exists.

### 3.3 Shared Components — Build First

| Priority | Component / Pattern | Milestone | Blocks |
| --- | --- | --- | --- |
| **P0** | `ReportSummaryBar.vue` | M10 | M11, M12 footer UI |
| **P1** | Report page layout template (header + card + error + meta) | M10 (de facto) | M11, M12 view scaffolding |
| **P2** | Summary DTO in API response shape | M10 | M11, M12 API design (already specified) |

Optional future extraction (post-M12, not required for V1):

- `ReportPageHeader.vue` — title + subtitle + refresh button
- `ReportEmptyState.vue` — inbox icon + message

Do **not** extract these prematurely — three report views with copy-paste from M9/M10 is acceptable for V1.

---

## 4. Effort Estimate

Estimates assume one developer familiar with M9 codebase. Includes backend, frontend, manual verification, and implementation summary doc.

| Milestone | Backend | Frontend | Testing | Total |
| --- | --- | --- | --- | --- |
| **M10** Piutang | 0.5 day | 0.5 day | 0.25 day | **1.25 days** |
| **M11** Inventory | 0.5 day | 0.375 day | 0.25 day | **1.125 days** |
| **M12** Purchasing | 0.375 day | 0.375 day | 0.25 day | **1 day** |
| **Total** | **1.375 days** | **1.25 days** | **0.75 days** | **~3.4 days** |

### Effort breakdown notes

**M10 backend (0.5 day):** Straightforward DAL wrapper; summary logic copied from `DashboardPiutangDal`. Main care point: customer key reconciliation.

**M10 frontend (0.5 day):** Includes creating `ReportSummaryBar.vue` — amortized across M11/M12.

**M11 backend (0.5 day):** Grouping logic copied from `DashboardInventoryDal`. Slightly more complex than M10 due to BrgId aggregation.

**M11 frontend (0.375 day):** Reuses M10 component; 5 columns; no period label.

**M12 backend (0.375 day):** Near-identical to M9 `SalesReportDal` + summary. Lowest complexity.

**M12 frontend (0.375 day):** 9 columns including PostingStok styling; otherwise mirrors Sales Report.

**Testing (0.25 day each):** Manual API checks, KPI reconciliation (M10/M11), desktop comparison (M12), build verification, screenshot.

### Parallel delivery scenario (2 developers, ~2 days)

| Day | Developer A | Developer B |
| --- | --- | --- |
| 1 AM | M10 backend + frontend (incl. ReportSummaryBar) | M11 backend |
| 1 PM | M10 verification + merge | M12 backend |
| 2 AM | M12 frontend | M11 frontend |
| 2 PM | M12 verification | M11 verification + merge |

---

## 5. Acceptance Criteria

### 5.1 M10 — Piutang Report V1

#### Backend

- [ ] `GET /api/reports/piutang` returns HTTP 200 with JWT; HTTP 401 without
- [ ] Response includes `PeriodFrom` = `2000-01-01`, `PeriodTo` = today
- [ ] Response includes `Summary.TotalPiutang` and `Summary.TotalCustomer`
- [ ] All rows have `KurangBayar > 1` (open balance filter applied)
- [ ] Rows contain exactly 7 approved fields in approved order
- [ ] No deferred columns exposed (Wilayah, Bayar Tunai/Giro, etc.)
- [ ] Controller delegates exclusively through MediatR
- [ ] No new SQL introduced; `IPiutangSalesWilayahDal` is sole data source
- [ ] Solution builds with zero errors

#### Frontend

- [ ] Route `/reports/piutang` accessible when authenticated
- [ ] Sidebar: Reports → Piutang Report navigates correctly
- [ ] DataTable displays 7 columns: Customer, Sales, Faktur, Tanggal, Jatuh Tempo, Total Jual, Kurang Bayar
- [ ] Jatuh Tempo column visually prominent (CSS emphasis)
- [ ] `ReportSummaryBar` shows Total Piutang and Total Customer from API
- [ ] Loading, error, and empty states functional
- [ ] Client-side pagination and sorting work
- [ ] Refresh reloads data
- [ ] `npm run build` passes

#### Reconciliation

- [ ] `Summary.TotalPiutang` === M5 dashboard `TotalPiutang` (exact)
- [ ] `Summary.TotalCustomer` === M5 dashboard `TotalCustomer` (exact)

#### Out of scope confirmed

- [ ] No date range params, search, export, drilldown, or Show Paid toggle

---

### 5.2 M11 — Inventory Report V1

#### Backend

- [ ] `GET /api/reports/inventory` returns HTTP 200 with JWT; HTTP 401 without
- [ ] Response includes `Summary.TotalInventoryValue` and `Summary.TotalItem`
- [ ] No `PeriodFrom`/`PeriodTo` (snapshot report)
- [ ] All displayed rows have `Qty > 0`
- [ ] In-Transit warehouse rows excluded
- [ ] Row `NilaiSediaan` = `Hpp * Qty`
- [ ] Summary uses BrgId grouping (matches `DashboardInventoryDal`)
- [ ] No Kartu Stok / `BTR_StokMutasi` usage
- [ ] Solution builds with zero errors

#### Frontend

- [ ] Route `/reports/inventory` accessible when authenticated
- [ ] Sidebar: Reports → Inventory Report navigates correctly
- [ ] DataTable displays 5 columns: Item, Warehouse, Qty, HPP, Nilai Sediaan
- [ ] Qty displayed as integer; HPP/Nilai Sediaan as currency
- [ ] `ReportSummaryBar` shows totals from API (not client-computed)
- [ ] Helper text explains item-level aggregation (optional muted note)
- [ ] Loading, error, and empty states functional
- [ ] `npm run build` passes

#### Reconciliation

- [ ] `Summary.TotalInventoryValue` === M6 dashboard `TotalInventoryValue` (exact)
- [ ] `Summary.TotalItem` === M6 dashboard `TotalItem` (exact)

#### Out of scope confirmed

- [ ] No Kartu Stok, date filters, search, export, unit breakdown columns

---

### 5.3 M12 — Purchasing Report V1

#### Backend

- [ ] `GET /api/reports/purchasing` returns HTTP 200 with JWT; HTTP 401 without
- [ ] Response includes current calendar month `PeriodFrom`/`PeriodTo`
- [ ] Response includes `Summary.GrandTotalPurchase` and `Summary.TotalInvoice`
- [ ] All rows from PF1 header (`IInvoiceViewDal`); void invoices excluded via DAL
- [ ] `PostingStok` values are `SUDAH` or `BELUM`
- [ ] 9 approved columns mapped correctly
- [ ] No PF2/PF3/PF4 data exposed
- [ ] Solution builds with zero errors

#### Frontend

- [ ] Route `/reports/purchasing` accessible when authenticated
- [ ] Sidebar: Reports → Purchasing Report navigates correctly
- [ ] Page title "Purchasing Report"; invoice column labeled "Invoice"
- [ ] DataTable displays 9 columns including Posting Stok
- [ ] PostingStok visually distinguished (SUDAH vs BELUM)
- [ ] `ReportSummaryBar` shows Grand Total Purchase and Total Invoice
- [ ] Period label shows current month
- [ ] Loading, error, and empty states functional
- [ ] `npm run build` passes

#### Validation

- [ ] `Summary.GrandTotalPurchase` equals sum of row `GrandTotal` values
- [ ] `Summary.TotalInvoice` equals row count
- [ ] Spot-check 2–3 rows against desktop PF1 (`InvoiceInfoForm`) for current month

#### Out of scope confirmed

- [ ] No PF2/PF3 detail, date filters, search, export, PT2 workflow link

---

## 6. Cross-Milestone Regression Checklist

Run after each milestone merge:

| # | Check | Expected |
| --- | --- | --- |
| 1 | Dashboard home loads | All 3 KPI cards (Sales, Piutang, Inventory) |
| 2 | Sales Report (M9) | Unchanged — no footer added |
| 3 | Login / JWT | Still functional |
| 4 | All prior report routes | Still accessible |
| 5 | Backend build | `j05-btr-distrib.sln` Debug — zero errors |
| 6 | Frontend build | `npm run build` — zero errors |

---

## 7. Implementation Summary Deliverables

After each milestone, create:

| Milestone | Summary doc |
| --- | --- |
| M10 | `implementation-summary-milestone-10.md` |
| M11 | `implementation-summary-milestone-11.md` |
| M12 | `implementation-summary-milestone-12.md` |

Follow format of `implementation-summary-milestone-9.md`: investigation findings, API contract, files changed, verification results, screenshots, out-of-scope list.

Screenshot targets:

| Milestone | Screenshot |
| --- | --- |
| M10 | Piutang Report page + dashboard reconciliation note |
| M11 | Inventory Report page with summary bar |
| M12 | Purchasing Report page with PostingStok column |

Store in `docs/work/btr-portal-api-scaffolding/screenshots/`.

---

## 8. File Index (All Milestones)

### New backend files (12 total)

| File | Milestone |
| --- | --- |
| `ReportingContext/PiutangReportAgg/Contracts/IPiutangReportDal.cs` | M10 |
| `ReportingContext/PiutangReportAgg/Queries/GetPiutangReportQuery.cs` | M10 |
| `ReportingContext/PiutangReportAgg/PiutangReportDal.cs` | M10 |
| `Controllers/Reports/PiutangReportController.cs` | M10 |
| `ReportingContext/InventoryReportAgg/Contracts/IInventoryReportDal.cs` | M11 |
| `ReportingContext/InventoryReportAgg/Queries/GetInventoryReportQuery.cs` | M11 |
| `ReportingContext/InventoryReportAgg/InventoryReportDal.cs` | M11 |
| `Controllers/Reports/InventoryReportController.cs` | M11 |
| `ReportingContext/PurchasingReportAgg/Contracts/IPurchasingReportDal.cs` | M12 |
| `ReportingContext/PurchasingReportAgg/Queries/GetPurchasingReportQuery.cs` | M12 |
| `ReportingContext/PurchasingReportAgg/PurchasingReportDal.cs` | M12 |
| `Controllers/Reports/PurchasingReportController.cs` | M12 |

### New frontend files (7 total)

| File | Milestone |
| --- | --- |
| `src/components/reports/ReportSummaryBar.vue` | M10 |
| `src/stores/piutangReportStore.ts` | M10 |
| `src/views/reports/PiutangReportView.vue` | M10 |
| `src/stores/inventoryReportStore.ts` | M11 |
| `src/views/reports/InventoryReportView.vue` | M11 |
| `src/stores/purchasingReportStore.ts` | M12 |
| `src/views/reports/PurchasingReportView.vue` | M12 |

### Modified files (shared across milestones)

| File | Changes |
| --- | --- |
| `btr.application.csproj` | Add Compile includes per milestone |
| `btr.infrastructure.csproj` | Add Compile includes per milestone |
| `InfrastructurePortalExtensions.cs` | Add 3 DAL registrations |
| `PortalPresentationExtensions.cs` | Add 3 controller registrations |
| `src/models/reports.ts` | Add types per milestone |
| `src/api/reportsApi.ts` | Add fetch function per milestone |
| `src/router/index.ts` | Add 3 routes |
| `src/layouts/MainLayout.vue` | Add 3 sidebar menu items |
