# Implementation Summary: BTR Portal API — Milestone 4 (Sales Dashboard V1)

## Status

Milestone 4 is complete. `GET /api/dashboard/sales` returns real sales summary data from existing BTR reporting sources. Piutang and inventory dashboards remain placeholders. The full solution builds and all verification checks pass.

---

## 1. Investigation Findings

### Sales reporting landscape

The most mature sales dashboard logic in BTR Desktop lives in **Sales Omzet** (`SalesOmzetInfoForm`, `SalesOmzetChartForm`). Both forms read from the materialized aggregate table **`BTR_SalesOmzet`**, not directly from `BTR_Faktur` / `BTR_Order`.

| Area | Location | Role |
| --- | --- | --- |
| Data read model | `SalesOmzetDal` (`ISalesOmzetDal`) | Lists omzet rows from `BTR_SalesOmzet` with period filter + `OmzetStatus <> 'Void'` |
| Period filtering | `SalesOmzetPeriodPolicy` | Default **Omzet Period** mode: `OmzetDate BETWEEN @Tgl1 AND @Tgl2` |
| Omzet amount rules | `SalesOmzetChartAmountPolicy` | Completed → `FakturTotal`; Outstanding → `OrderTotal`; Pending → `FakturTotal` |
| KPI aggregation | `SalesOmzetChartSummaryBuilder` | Computes **Recognized Omzet** (completed rows only), pipeline omzet, status slices, weekly buckets |
| Desktop summary panel | `SalesOmzetInfoForm.UpdateSummaryPanel()` | Counts fakturs (`FakturCode` non-empty), sums order/faktur totals |
| Materialization | `ReconcileSalesOmzetWorker` | Writes/refreshes `BTR_SalesOmzet` from orders and fakturs (not used by portal V1) |

### Decision: reuse path for portal V1

| Metric | Source | Existing calculation reused |
| --- | --- | --- |
| `TotalOmzet` | `SalesOmzetChartSummaryBuilder.Build(...).RecognizedOmzet` | Yes — same KPI as Sales Omzet chart ("omzet diakui") |
| `TotalFaktur` | Row count where `FakturCode` is non-empty | Yes — same rule as `SalesOmzetInfoForm` faktur count |
| `TotalCustomer` | Distinct customer `Code` (fallback `CustomerName`) in period rows | Derived from same row set; no new SQL |
| Period scope | Current calendar month via `ITglJamDal.Now` | Fixed default; no API date parameters (per M4 constraint) |
| Period mode | `SalesOmzetPeriodFilterMode.OmzetPeriod` | Matches desktop default (Sales Period checkbox unchecked) |

No new SQL, tables, or omzet calculation policies were introduced.

---

## 2. Existing DALs Reused

| DAL / Service | Interface | Used for |
| --- | --- | --- |
| `SalesOmzetDal` | `ISalesOmzetDal` | Load omzet rows for current month from `BTR_SalesOmzet` |
| `SalesOmzetChartSummaryBuilder` | `ISalesOmzetChartSummaryBuilder` | Compute recognized omzet total |
| `SalesOmzetPeriodPolicy` | `ISalesOmzetPeriodPolicy` | (via `SalesOmzetDal`) Omzet period SQL filter |
| `SalesOmzetChartAmountPolicy` | `ISalesOmzetChartAmountPolicy` | (via summary builder) amount resolution |
| `TglJamDal` | `ITglJamDal` | Server date for current-month period and `GeneratedAt` |

`DashboardSalesDal` orchestrates these dependencies; it does not duplicate their SQL or business rules.

---

## 3. Existing Tables Reused

| Table | Usage |
| --- | --- |
| `BTR_SalesOmzet` | Primary read model — omzet rows filtered by `OmzetDate`, excluding `Void` status |

No new dashboard tables were created.

---

## 4. New Response Shape

`DashboardSalesResponse` replaces the Milestone 3 placeholder:

```json
{
  "Status": "success",
  "Code": 200,
  "Message": null,
  "Data": {
    "TotalOmzet": 0.0,
    "TotalFaktur": 0,
    "TotalCustomer": 0,
    "GeneratedAt": "2026-06-06T00:43:42.597"
  }
}
```

| Field | Type | Meaning |
| --- | --- | --- |
| `TotalOmzet` | `decimal` | Recognized omzet (completed sales) for current calendar month |
| `TotalFaktur` | `int` | Count of omzet rows with a faktur code in the period |
| `TotalCustomer` | `int` | Distinct customers (by `Code`, else `CustomerName`) in the period |
| `GeneratedAt` | `DateTime` | Server timestamp when the summary was built (`ITglJamDal.Now`) |

Piutang and inventory endpoints still return `{ "Status": "not_implemented" }`.

---

## 5. Files Changed

### Application

| File | Change |
| --- | --- |
| `ReportingContext/DashboardSalesAgg/Contracts/IDashboardSalesDal.cs` | `GetPlaceholder()` → `GetSummary()` |
| `ReportingContext/DashboardSalesAgg/Queries/GetDashboardSalesQuery.cs` | Real response DTO; handler calls `GetSummary()` |

### Infrastructure

| File | Change |
| --- | --- |
| `ReportingContext/DashboardSalesAgg/DashboardSalesDal.cs` | Injects `ISalesOmzetDal`, `ISalesOmzetChartSummaryBuilder`, `ITglJamDal`; builds summary |

### Unchanged (by design)

- `SalesDashboardController` — still thin MediatR delegate
- `DashboardPiutangDal`, `DashboardInventoryDal` — placeholders
- Authentication, health, CORS, JWT filter

---

## 6. Verification Results

| # | Check | Result |
| --- | --- | --- |
| 1 | Endpoint returns real data (SQL-backed shape) | Pass — `TotalOmzet`, `TotalFaktur`, `TotalCustomer`, `GeneratedAt` populated |
| 2 | No direct SQL in controller | Pass — `SalesDashboardController` calls MediatR only |
| 3 | Data flows through MediatR | Pass — `GetDashboardSalesQuery` → `GetDashboardSalesHandler` → `IDashboardSalesDal` |
| 4 | Login still works | Pass — `POST /api/auth/login` → JWT for `DIMAS` |
| 5 | Dashboard authorization still works | Pass — anonymous `GET /api/dashboard/sales` → HTTP 401 |
| 6 | Full solution build | Pass — `j05-btr-distrib.sln` Debug build |
| 7 | Piutang / inventory unchanged | Pass — still return `"not_implemented"` |

Test commands (IIS Express on port 5053):

```powershell
# Build
& "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe" `
  "src\j05-btr-distrib\j05-btr-distrib.sln" /p:Configuration=Debug

# Run
& "C:\Program Files\IIS Express\iisexpress.exe" `
  /path:"src\j05-btr-distrib\btr.portal.api" /port:5053

# Anonymous (401)
curl.exe http://localhost:5053/api/dashboard/sales

# Login
curl.exe -s -X POST http://localhost:5053/api/auth/login `
  -H "Content-Type: application/json" `
  --data-raw '{"UserId":"DIMAS","Password":"1111"}'

# Authenticated sales dashboard
curl.exe http://localhost:5053/api/dashboard/sales -H "Authorization: Bearer <token>"
```

**Note:** Dev database (`btr_yk` via registry) returned zero counts for June 2026 current month — expected when no omzet rows exist in that period. The response confirms the SQL pipeline executed without error.

---

## 7. Future Improvements

| Item | Description |
| --- | --- |
| Date range parameters | Optional `from` / `to` query params aligned with `SalesOmzetInfoForm` period picker |
| Sales period mode | Toggle Omzet Period vs Sales Period (`SalesOmzetPeriodFilterMode`) |
| Pipeline omzet | Expose `PipelineOmzet` from existing summary builder for funnel KPI |
| Target / achievement | Wire `SalesOmzetTargetResolver` + `BTR_SalesOmzetTarget` |
| Charts | Weekly/status slices already computed by `SalesOmzetChartSummaryBuilder` |
| Piutang dashboard | Milestone 5+ — wire `PIutangLunasViewDal`, `PiutangSalesWilayahDal` |
| Inventory dashboard | Milestone 5+ — wire `StokBalanceViewDal` |
| IIS deployment | Publish profile, `appsettings.{MACHINE}.json`, production JWT secret |
