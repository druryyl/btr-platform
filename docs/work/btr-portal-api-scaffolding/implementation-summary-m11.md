# Implementation Summary: BTR Portal — Milestone 11 (Inventory Report V1)

## Status

Milestone 11 is complete. `GET /api/reports/inventory` returns stock balance rows from existing BTR reporting sources with footer summary totals that reconcile exactly with the M6 Inventory Dashboard. The portal adds the Inventory Report page at `/reports/inventory` with a PrimeVue DataTable and reuses the M10 `ReportSummaryBar` component. All M11 verification checks pass.

---

## 1. Files Added

### Backend — Application (`ReportingContext/InventoryReportAgg`)

| File | Purpose |
| --- | --- |
| `Contracts/IInventoryReportDal.cs` | Report DAL contract |
| `Queries/GetInventoryReportQuery.cs` | MediatR query, handler, `InventoryReportResponse`, `InventoryReportSummary`, `InventoryReportRow` |

### Backend — Infrastructure

| File | Purpose |
| --- | --- |
| `ReportingContext/InventoryReportAgg/InventoryReportDal.cs` | Wraps `IStokBalanceViewDal`; excludes In-Transit; computes footer totals using `DashboardInventoryDal` grouping logic |

### Backend — Portal API

| File | Purpose |
| --- | --- |
| `Controllers/Reports/InventoryReportController.cs` | Thin MediatR delegate — `GET /api/reports/inventory` |

### Backend — Tests

| File | Purpose |
| --- | --- |
| `btr.test/ReportingContext/InventoryReportDalTest.cs` | Unit tests for In-Transit exclusion, zero-qty filter, BrgId grouping, and row ordering |

### Frontend

| File | Purpose |
| --- | --- |
| `src/stores/inventoryReportStore.ts` | Loading / error / data state |
| `src/views/reports/InventoryReportView.vue` | PrimeVue DataTable report page with summary bar and aggregation helper text |

---

## 2. Files Modified

| File | Change |
| --- | --- |
| `btr.application/btr.application.csproj` | Added `InventoryReportAgg` compile includes |
| `btr.infrastructure/btr.infrastructure.csproj` | Added `InventoryReportDal.cs` compile include |
| `btr.portal.api/btr.portal.api.csproj` | Added `InventoryReportController.cs` compile include |
| `btr.test/btr.test.csproj` | Added `InventoryReportDalTest.cs` compile include |
| `btr.portal.api/Configurations/InfrastructurePortalExtensions.cs` | Registered `IInventoryReportDal` → `InventoryReportDal` |
| `btr.portal.api/Configurations/PortalPresentationExtensions.cs` | Registered `InventoryReportController` |
| `src/models/reports.ts` | Added `InventoryReportResponse`, `InventoryReportSummary`, `InventoryReportRow` types |
| `src/api/reportsApi.ts` | Added `fetchInventoryReport()` |
| `src/router/index.ts` | Added `/reports/inventory` route |
| `src/layouts/MainLayout.vue` | Added Inventory Report sidebar menu item |

---

## 3. Existing DALs Reused

| DAL / Service | Interface | Used for |
| --- | --- | --- |
| `StokBalanceViewDal` | `IStokBalanceViewDal` | Load stock balance rows via `ListData()` — same source as desktop IF1 and M6 dashboard |
| `TglJamDal` | `ITglJamDal` | `GeneratedAt` timestamp |

`IStokBalanceViewDal` is auto-registered via existing Scrutor `IListData<StokBalanceView>` scan. `InventoryReportDal` orchestrates these dependencies; it does not duplicate SQL or business rules.

### Logic reused from `DashboardInventoryDal` (verbatim copy)

| Logic | Purpose |
| --- | --- |
| `InTransitWarehouseName = "In-Transit"` | Exclude in-transit warehouse from all calculations |
| BrgId grouping | `Qty = Sum(Qty)`, `NilaiSediaan = Sum(Hpp * Qty)` per item |
| `TotalInventoryValue` | Sum of grouped `NilaiSediaan` |
| `TotalItem` | Count of BrgId groups where aggregated `Qty > 0` |

### Not used (by design)

| Component | Reason |
| --- | --- |
| `StokBalanceViewDal` SQL modifications | Single source of truth — unchanged |
| `BTR_StokMutasi` / Kartu Stok DALs | Out of M11 V1 scope (Decision 6) |
| Unit conversion (`QtyBesar`/`QtyKecil`) | Deferred columns not mapped |
| Supplier/Kategori columns | Deferred |

---

## 4. API Endpoint Created

### Endpoint

```
GET /api/reports/inventory
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
    "GeneratedAt": "2026-06-06T11:49:55.17",
    "Summary": {
      "TotalInventoryValue": 14673597942.16,
      "TotalItem": 1716
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

| Field | Type | Meaning |
| --- | --- | --- |
| `GeneratedAt` | `DateTime` | Server timestamp when report was built |
| `Summary.TotalInventoryValue` | `decimal` | Grouped sum of `Hpp × Qty` — reconciles with M6 dashboard |
| `Summary.TotalItem` | `int` | Distinct items with aggregated `Qty > 0` — reconciles with M6 dashboard |
| `Rows` | `InventoryReportRow[]` | Product × warehouse rows with `Qty > 0`, ordered by `BrgCode`, `WarehouseName` |

No `PeriodFrom` / `PeriodTo` — inventory is a point-in-time snapshot.

Anonymous requests return HTTP 401.

---

## 5. Frontend Pages Created

### Route

| Path | Component | Auth |
| --- | --- | --- |
| `/reports/inventory` | `InventoryReportView.vue` | Required |

### Navigation

Sidebar menu:

```
Reports
├── Sales Report
├── Piutang Report
└── Inventory Report
```

### DataTable features

| Feature | Implementation |
| --- | --- |
| Columns | Item, Warehouse, Qty, HPP, Nilai Sediaan (5 columns) |
| `data-key` | Composite `${BrgId}\|${WarehouseName}` via computed `dataKey` field |
| Footer summary | `ReportSummaryBar` — Total Inventory Value, Total Item from API `Summary` |
| Helper text | Muted note explaining item-level aggregation across warehouses |
| Loading state | DataTable `:loading` + Refresh button spinner |
| Empty state | Custom `#empty` template — "No inventory items with quantity on hand." |
| Pagination | Client-side, 25 rows default, options 10/25/50/100 |
| Sorting | Column sort enabled |
| Qty formatting | Plain integer display (no currency formatter) |
| HPP / Nilai Sediaan | `formatCurrency()` |

No period label, filtering, export, or drilldown.

---

## 6. Verification Results

| # | Check | Result |
| --- | --- | --- |
| 1 | Backend build succeeds | Pass — `j05-btr-distrib.sln` Debug build, zero errors |
| 2 | Frontend build succeeds | Pass — `npm run build` (vue-tsc + vite) |
| 3 | InventoryReportDal unit tests | Pass — 4/4 tests |
| 4 | Authorization | Pass — anonymous `GET /api/reports/inventory` → HTTP 401 |
| 5 | MediatR pipeline | Pass — `GetInventoryReportQuery` → Handler → `IInventoryReportDal` → `IStokBalanceViewDal` |
| 6 | Controller has no DAL reference | Pass — `InventoryReportController` calls MediatR only |
| 7 | No period fields in response | Pass — snapshot only |
| 8 | All displayed rows have `Qty > 0` | Pass — 1,716 rows, all positive |
| 9 | In-Transit warehouse excluded | Pass — zero In-Transit rows in response |
| 10 | Row `NilaiSediaan = Hpp × Qty` | Pass — all rows verified |
| 11 | **KPI reconciliation** | **Pass** — dashboard and report totals match exactly |
| 12 | Dashboard regression (M4–M6) | Pass — sales, piutang, inventory dashboards HTTP 200 |
| 13 | Report regression (M9–M10) | Pass — sales and piutang reports HTTP 200 |

### KPI reconciliation (mandatory acceptance test)

Dev database verification against running API (`http://localhost:5058`):

```powershell
# Login
$login = curl.exe -s -X POST http://localhost:5058/api/auth/login `
  -H "Content-Type: application/json" --data-raw '{"UserId":"DIMAS","Password":"1111"}'
$token = ($login | ConvertFrom-Json).Data.Token

# Fetch dashboard and report
$dash = curl.exe -s http://localhost:5058/api/dashboard/inventory -H "Authorization: Bearer $token" | ConvertFrom-Json
$rpt = curl.exe -s http://localhost:5058/api/reports/inventory -H "Authorization: Bearer $token" | ConvertFrom-Json

# Assert reconciliation
$dash.Data.TotalInventoryValue -eq $rpt.Data.Summary.TotalInventoryValue   # True
$dash.Data.TotalItem -eq $rpt.Data.Summary.TotalItem                         # True
```

| Metric | Dashboard | Report Summary | Match |
| --- | --- | --- | --- |
| Total Inventory Value | 14,673,597,942.16 | 14,673,597,942.16 | Yes |
| Total Item | 1,716 | 1,716 | Yes |
| Row count | — | 1,716 | — |

**Note:** On this dev database, visible row `NilaiSediaan` sum also equals footer total because each item appears in a single warehouse. When the same `BrgId` spans multiple warehouses, visible row sum will differ from footer — this is expected. Reconciliation is against dashboard KPIs only.

---

## 7. Known Limitations

| Limitation | Notes |
| --- | --- |
| **Large dataset — single API call** | Dev database returned 1,716 rows in one response. Client-side pagination limits DOM rendering; full dataset loaded upfront (same as M9/M10). |
| **No date-range parameters** | Snapshot report — point-in-time stock balance per product decision |
| **No search or export** | Out of V1 scope |
| **Deferred columns** | Supplier, Kategori, QtyBesar, QtyKecil, SatBesar, SatKecil, Conversion not exposed |
| **Kartu Stok** | Movement ledger (`BTR_StokMutasi`) deferred to future Inventory Drilldown milestone |
| **Footer vs visible row sum** | Footer uses BrgId grouping; visible rows are per-warehouse — sums may differ when items span warehouses |
| **Zero-qty items** | Excluded from table but included in grouping logic (affects `TotalItem` count only when aggregated Qty = 0) |

---

## 8. Deviations from Implementation Plan

None. Implementation follows `implementation-plan-m11-inventory-report-v1.md` exactly.

Minor additions beyond the plan checklist:

| Addition | Rationale |
| --- | --- |
| `InventoryReportDalTest.cs` (4 unit tests) | Plan marked as "recommended" — implemented with stub DALs (no Moq dependency), matching M10 pattern |

---

## 9. Screenshot References

### Inventory Report page

Screenshot target: `docs/work/btr-portal-api-scaffolding/screenshots/milestone-11-inventory-report.png`

Expected content:

- Reports sidebar with Inventory Report entry (active)
- Subtitle: "Stock balance snapshot (qty > 0)."
- DataTable with 5 columns: Item, Warehouse, Qty, HPP, Nilai Sediaan
- Client-side pagination
- Footer summary bar with Total Inventory Value and Total Item from API
- Helper text explaining item-level aggregation
- Generated-at timestamp

---

## 10. User Workflow

1. User opens BTR Portal and signs in.
2. Dashboard loads KPI summary including M6 Inventory card (unchanged).
3. User clicks **Reports → Inventory Report** (or navigates to `/reports/inventory`).
4. Page loads stock balance rows from `GET /api/reports/inventory`.
5. DataTable displays 5 columns; user can sort and paginate.
6. Footer summary bar shows **Total Inventory Value** and **Total Item** matching dashboard KPIs.
7. Helper text explains that totals are aggregated by item across warehouses.
8. **Refresh** reloads the report from the API.
9. When no items have quantity on hand, the empty state message is shown (summary totals will be 0).

---

## 11. Out of Scope (unchanged)

- Kartu Stok / stock movement ledger
- Date range / warehouse / item search filters
- Export (Excel/PDF)
- Unit breakdown columns (QtyBesar/QtyKecil)
- Supplier/Kategori columns
- Zero-qty rows in table
- Server-side pagination
- Dashboard behavior changes
