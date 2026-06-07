# Implementation Summary: BTR Portal API — Milestone 5 (Piutang Dashboard V1)

## Status

Milestone 5 is complete. `GET /api/dashboard/piutang` returns real outstanding receivable summary data from existing BTR reporting sources. Sales dashboard (M4) and inventory placeholder (M3) are unchanged. The full solution builds and all verification checks pass.

---

## 1. Investigation Findings

### Piutang reporting landscape

Finance receivable reporting in BTR Desktop is built around **`BTR_Piutang`** with persisted balance columns (`Total`, `Potongan`, `Terbayar`, **`Sisa`**). Outstanding balances are stored at write time by `PiutangBuilder.ReCalc()` — reports read `Sisa` directly rather than recalculating from payments.

| Area | Location | Role |
| --- | --- | --- |
| Piutang per Sales/Wilayah report | `PiutangSalesWilayahDal` (`IPiutangSalesWilayahDal`) | Detailed piutang rows with payment breakdown; `KurangBayar` maps to `BTR_Piutang.Sisa` |
| Piutang list (pelunasan context) | `PIutangLunasViewDal` (`IPiutangLunasViewDal`) | Lighter row set with `Sisa`, customer, faktur code; filtered by `PiutangDate` |
| Pelunasan activity report | `PelunasanInfoDal` | Payment entries in a date range — not a piutang balance snapshot |
| Piutang tracker | `PiutangTrackerDal` | Per-faktur lifecycle timeline — drilldown, not summary |
| Tagihan / pelunasan UI | `TagihanForm`, `LunasPiutang2Form` | Operational screens; use `Sisa <= 1` / `Sisa < 1` as paid threshold |

### Existing Piutang Info screens

| Screen | DAL | Summary behavior |
| --- | --- | --- |
| **Piutang Sales Wilayah** (`PiutangSalesWilayahForm`) | `PiutangSalesWilayahDal` | Grid footer sums `KurangBayar`; default filter hides paid rows (`KurangBayar > 1`) |
| **Lunas Piutang** (`LunasPiutang2Form`) | `PIutangLunasViewDal` (injected) | Per-faktur payment entry; uses `piutang.Sisa <= 1` as paid check |
| **Pelunasan Info** (`PelunasanInfoForm`) | `PelunasanInfoDal` | Payment totals in period — not outstanding balance KPI |
| **Piutang Tracker** (`PiutangTrackerForm`) | `PiutangTrackerDal` | Single-faktur audit trail |

### Existing receivable KPI calculations

| KPI | Existing behavior | Source |
| --- | --- | --- |
| Outstanding per faktur | `BTR_Piutang.Sisa` (persisted) | `PiutangBuilder.ReCalc()` |
| Paid vs open threshold | `Sisa <= 1` (paid) / `Sisa > 1` (open) | `PiutangSalesWilayahForm.FilterSisaTagihan`, `TagihanForm`, `LunasPiutang2Form` |
| Total outstanding (report UI) | Sum of `KurangBayar` on filtered rows | `PiutangSalesWilayahForm` grid summary row |
| Customer grouping | `CustomerCode` / `CustomerName` | `PiutangSalesWilayahDto`, `PiutangLunasView` |

### Decision: reuse path for portal V1

| Metric | Source | Existing calculation reused |
| --- | --- | --- |
| `TotalPiutang` | `PiutangSalesWilayahDal` → sum `KurangBayar` where `KurangBayar > 1` | Yes — same field and paid/open filter as `PiutangSalesWilayahForm` default view |
| `TotalCustomer` | Distinct `CustomerCode` (fallback `CustomerName`) on open rows | Derived from same row set; no new SQL |
| Period scope | Internal wide range `2000-01-01` → server today via `ITglJamDal` | Fixed default; no API date parameters (per M5 constraint). Required because existing DALs filter on `PiutangDate BETWEEN @Tgl1 AND @Tgl2` |
| `GeneratedAt` | `ITglJamDal.Now` | Same pattern as Sales Dashboard M4 |

`PIutangLunasViewDal` exposes the same `Sisa` values with a lighter query but is oriented to the pelunasan entry flow rather than the summary report footer. **`PiutangSalesWilayahDal`** was selected because it is the established piutang **summary report** with explicit outstanding-total behavior in the desktop UI.

No new SQL, tables, or receivable calculation policies were introduced.

---

## 2. Existing DALs Reused

| DAL / Service | Interface | Used for |
| --- | --- | --- |
| `PiutangSalesWilayahDal` | `IPiutangSalesWilayahDal` | Load piutang rows with `KurangBayar` (`BTR_Piutang.Sisa`) and customer identifiers |
| `TglJamDal` | `ITglJamDal` | Server date for internal period end and `GeneratedAt` |

`DashboardPiutangDal` orchestrates these dependencies; it does not duplicate their SQL or business rules.

**Investigated but not wired for V1:**

| DAL | Reason not primary |
| --- | --- |
| `PIutangLunasViewDal` | Same `Sisa` source; lighter query but not the summary-report aggregation pattern |
| `PelunasanInfoDal` | Payment activity in period, not outstanding balance |
| `PiutangTrackerDal` | Per-faktur drilldown only |
| `PiutangDal` | Same date-filtered `Sisa` read; no summary aggregation in desktop UI |

---

## 3. Existing Tables Reused

| Table | Usage |
| --- | --- |
| `BTR_Piutang` | Primary read model — `Sisa` (outstanding), `PiutangDate` filter |
| `BTR_Faktur` | Faktur code, date, sales person join |
| `BTR_Customer` | Customer code and name |
| `BTR_SalesPerson` | Sales name (carried in report row; not used in V1 KPI) |
| `BTR_Wilayah` | Wilayah name (carried in report row; not used in V1 KPI) |
| `BTR_PiutangLunas` | Payment subqueries in `PiutangSalesWilayahDal` (existing SQL; not re-summed in portal) |
| `BTR_PiutangElement` | Adjustment subqueries in `PiutangSalesWilayahDal` (existing SQL; not re-summed in portal) |

No new dashboard tables were created.

---

## 4. Existing Calculations Reused

| Calculation | Where defined | Portal usage |
| --- | --- | --- |
| Outstanding balance per faktur | `BTR_Piutang.Sisa` (written by `PiutangBuilder.ReCalc()`) | Read via `PiutangSalesWilayahDto.KurangBayar` |
| Paid/open threshold | `KurangBayar > 1` | In-memory filter matching `PiutangSalesWilayahForm.FilterSisaTagihan` default |
| Total outstanding | Sum of `KurangBayar` on open rows | In-memory sum matching grid footer `{Sum}` on `KurangBayar` |
| Distinct customers | `CustomerCode` else `CustomerName` | In-memory distinct count on open rows |

---

## 5. New Response Shape

`DashboardPiutangResponse` replaces the Milestone 3 placeholder:

```json
{
  "Status": "success",
  "Code": 200,
  "Message": null,
  "Data": {
    "TotalPiutang": 16042764169.35,
    "TotalCustomer": 1551,
    "GeneratedAt": "2026-06-06T01:05:50.97"
  }
}
```

| Field | Type | Meaning |
| --- | --- | --- |
| `TotalPiutang` | `decimal` | Sum of outstanding `Sisa` (`KurangBayar`) for open receivables |
| `TotalCustomer` | `int` | Distinct customers (by `CustomerCode`, else `CustomerName`) with open receivables |
| `GeneratedAt` | `DateTime` | Server timestamp when the summary was built (`ITglJamDal.Now`) |

Inventory endpoint still returns `{ "Status": "not_implemented" }`.

---

## 6. Files Changed

### Application

| File | Change |
| --- | --- |
| `ReportingContext/DashboardPiutangAgg/Contracts/IDashboardPiutangDal.cs` | `GetPlaceholder()` → `GetSummary()` |
| `ReportingContext/DashboardPiutangAgg/Queries/GetDashboardPiutangQuery.cs` | Real response DTO; handler calls `GetSummary()` |

### Infrastructure

| File | Change |
| --- | --- |
| `ReportingContext/DashboardPiutangAgg/DashboardPiutangDal.cs` | Injects `IPiutangSalesWilayahDal`, `ITglJamDal`; builds summary from open rows |

### Unchanged (by design)

- `PiutangDashboardController` — still thin MediatR delegate
- `DashboardInventoryDal` — placeholder
- `DashboardSalesDal` — M4 real data unchanged
- Authentication, health, CORS, JWT filter

---

## 7. Verification Results

| # | Check | Result |
| --- | --- | --- |
| 1 | Endpoint returns real data (SQL-backed shape) | Pass — `TotalPiutang`, `TotalCustomer`, `GeneratedAt` populated from dev DB |
| 2 | No direct SQL in controller | Pass — `PiutangDashboardController` calls MediatR only |
| 3 | Data flows through MediatR | Pass — `GetDashboardPiutangQuery` → `GetDashboardPiutangHandler` → `IDashboardPiutangDal` |
| 4 | Login still works | Pass — `POST /api/auth/login` → JWT for `DIMAS` |
| 5 | Dashboard authorization still works | Pass — anonymous `GET /api/dashboard/piutang` → HTTP 401 |
| 6 | Full solution build | Pass — `j05-btr-distrib.sln` Debug build |
| 7 | Sales dashboard unchanged | Pass — M4 response shape still returned |

Test commands (IIS Express on port 5054):

```powershell
# Build
& "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe" `
  "src\j05-btr-distrib\j05-btr-distrib.sln" /p:Configuration=Debug

# Run
& "C:\Program Files\IIS Express\iisexpress.exe" `
  /path:"src\j05-btr-distrib\btr.portal.api" /port:5054

# Anonymous (401)
curl.exe http://localhost:5054/api/dashboard/piutang

# Login
$body = '{"UserId":"DIMAS","Password":"1111"}'
curl.exe -s -X POST http://localhost:5054/api/auth/login -H "Content-Type: application/json" -d $body

# Authenticated piutang dashboard
curl.exe http://localhost:5054/api/dashboard/piutang -H "Authorization: Bearer <token>"
```

**Sample dev DB response (`btr_yk`):** `TotalPiutang` ≈ 16.0B IDR, `TotalCustomer` = 1551 open-customer count. Values depend on database content.

---

## 8. Future Improvements

| Item | Description |
| --- | --- |
| Date range parameters | Optional `from` / `to` aligned with `PiutangSalesWilayahForm` period picker |
| Show paid toggle | Mirror `ShowLunasCheckBox` — include fully paid rows when requested |
| Aging buckets | Group open `Sisa` by `DueDate` / `JatuhTempo` (not implemented in desktop today) |
| Payment breakdown | Expose `BayarTunai`, `BayarGiro`, `Retur`, `Potongan`, `MateraiAdmin` from existing DAL columns |
| Sales / wilayah slices | Group open piutang by `SalesName` / `WilayahName` already present in DAL rows |
| Charts | Regional or salesman distribution from `PiutangSalesWilayahDto` rows |
| Inventory dashboard | Milestone 6+ — wire `StokBalanceViewDal` |
| Performance | Consider dedicated summary query if full-history row load becomes slow on large databases |
| IIS deployment | Publish profile, `appsettings.{MACHINE}.json`, production JWT secret |
