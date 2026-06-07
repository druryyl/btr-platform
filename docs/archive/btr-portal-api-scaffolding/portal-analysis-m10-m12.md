# BTR Portal Analysis — Milestones M10–M12

> **Superseded by product decisions.** Authoritative version: [`portal-analysis-m10-m12-final.md`](./portal-analysis-m10-m12-final.md)

**Scope:** Discovery and analysis only. No implementation plans.  
**Date:** 2026-06-06  
**Context:** BTR Portal is a read-only reporting and dashboard application. M1–M9 are complete. This document analyzes how M10 (Piutang Report), M11 (Inventory Report), and M12 (Purchasing Report) can reuse existing BTR Desktop reporting assets.

**Product decisions:** Approved 2026-06-06. See Section 8 and `portal-analysis-m10-m12-final.md` for finalized scope.

**Reference documents:** `btr-portal-milestone.md`, `PRODUCT.md`, `DOMAIN.md`, `LANDSCAPE.md`, `implementation-summary-milestone-9.md`

---

## 1. Executive Summary

M10–M12 extend the pattern established in M9 (Sales Report V1): a thin `ReportingContext` wrapper over an existing desktop DAL, exposed via MediatR and rendered in a PrimeVue DataTable with **footer summary totals**. All three milestones have **strong reuse candidates** already wired into the portal for dashboard KPIs (M5 Piutang, M6 Inventory) or structurally identical to M9 (M12 Purchasing). Product Owner has approved scope for all three.

| Milestone | Primary desktop screen | Primary DAL | Dashboard link | Approved V1 scope |
| --------- | ---------------------- | ----------- | -------------- | ----------------- |
| **M10** Piutang Report | FF1 Piutang Sales (`PiutangSalesWilayahForm`) | `IPiutangSalesWilayahDal` | M5 — KPI traceability | 7 columns; period 2000→today; hide paid; footer totals |
| **M11** Inventory Report | IF1 Stok Balance (`StokBalanceInfo2Form`) | `IStokBalanceViewDal` | M6 — KPI traceability | 5 columns; Qty > 0; Stock Balance only; footer totals |
| **M12** Purchasing Report | PF1 Invoice Info (`InvoiceInfoForm`) | `IInvoiceViewDal` | None | Current month; PF1; PostingStok; footer totals |

**Key architectural finding:** Piutang, Inventory, and Purchasing reporting have **no dedicated Policy classes**. Business rules live in DAL SQL, aggregate builders (write path), and WinForms UI filters. Portal wrappers should delegate to existing DALs and apply only presentation-level mapping — not reimplement calculations.

**Cross-cutting V1 conventions (approved):** Fixed period defaults; no date-range or search filters; footer summary totals on all report pages; client-side pagination.

**Recommended delivery order:** M10 → M11 → M12. Product decisions reduce prior uncertainty; M10 and M11 can still be developed in parallel.

---

## 2. M10 Analysis — Piutang Report V1

### 2.1 Goal

Expose receivable detail behind the M5 Piutang Dashboard KPI (`Total Piutang`, `Total Customer`). Report footer totals must reconcile with dashboard KPIs.

### 2.2 Approved Product Scope

| Aspect | Approved decision |
| ------ | ----------------- |
| Period | `2000-01-01` → today — match `DashboardPiutangDal` |
| Open balance | `KurangBayar > 1` only — no Show Paid toggle |
| Columns | 7 primary: Customer, Sales, Faktur, Tanggal, Jatuh Tempo, Total Jual, Kurang Bayar |
| Footer totals | Total Piutang, Total Customer |
| Filters | No date range or search in V1 |

Jatuh Tempo displayed prominently to prepare for M14 Piutang Aging Dashboard.

### 2.3 Existing Desktop Assets

#### Report / Info screens

| Menu | Screen | Path | Purpose |
| ---- | ------ | ---- | ------- |
| FF1 | `PiutangSalesWilayahForm` | `FinanceContext/PiutangSalesWilayahRpt/` | **Primary M10 source** — per-faktur receivable breakdown by sales + wilayah |
| FF2 | `PenerimaanPelunasanSalesForm` | `FinanceContext/PenerimaanPelunasanSalesRpt/` | Collections summary by sales + date (payment activity, not open balance) |
| FF4 | `PelunasanInfoForm` | `FinanceContext/LunasPiutangAgg/` | Payment detail — one row per pelunasan line |
| FT5 | `PiutangTrackerForm` | `FinanceContext/TagihanAgg/` | Single-invoice lifecycle timeline (not tabular period report) |

**Operational screens (not report sources):** `LunasPiutang2Form`, `TagihanForm`, `TandaTerimaTagihanForm`, `TagihanPrintOutForm`.

#### DALs

| Interface | Implementation | Filter parameter | Returns |
| --------- | -------------- | ---------------- | ------- |
| **`IPiutangSalesWilayahDal`** | `PiutangSalesWilayahDal` | `Periode` on `BTR_Piutang.PiutangDate` | `PiutangSalesWilayahDto` |
| `IPelunasanInfoDal` | `PelunasanInfoDal` | `Periode` on `BTR_PiutangLunas.LunasDate` | `PelunasanInfoDto` |
| `IPenerimaanPelunasanSalesDal` | `PenerimaanPelunasanSalesDal` | `Periode` on `LunasDate` | `PenerimaanPelunasanSalesDto` (aggregated) |
| `IPiutangTrackerDal` | `PiutangTrackerDal` | `IPiutangKey` (FakturId) | `PiutangTrackerDto` |
| `IPIutangLunasViewDal` | `PIutangLunasViewDal` | `Periode` on `PiutangDate` | `PiutangLunasView` (simpler header list) |

**Portal (existing):** `DashboardPiutangDal` wraps `IPiutangSalesWilayahDal` for M5 KPIs.

#### DTOs

**Primary report DTO — `PiutangSalesWilayahDto`** (`IPiutangSalesWilayahDal.cs`):

| Field | Meaning |
| ----- | ------- |
| `SalesName` | Sales person |
| `WilayahName` | Customer territory |
| `FakturCode`, `FakturDate` | Invoice reference |
| `CustomerCode`, `CustomerName`, `Alamat` | Customer identity |
| `JatuhTempo` | Due date |
| `TotalJual` | Original receivable amount (`BTR_Piutang.Total`) |
| `BayarTunai`, `BayarGiro` | Cash vs cheque/BG payments |
| `Retur`, `Potongan`, `MateraiAdmin` | Adjustments from `BTR_PiutangElement` |
| `KurangBayar` | Outstanding balance (`BTR_Piutang.Sisa`, persisted) |

**Domain models (write path, not report-first):** `PiutangModel`, `PiutangElementModel`, `PiutangLunasModel`, `PiutangLunasView`.

#### Queries

| Query | Status |
| ----- | ------ |
| `GetDashboardPiutangQuery` | Exists — delegates to `IDashboardPiutangDal` |
| `GetPiutangReportQuery` | **Does not exist** |

#### Builders

| Builder | Role |
| ------- | ---- |
| `PiutangBuilder` | Write-path aggregate: `Potongan = Σ(NilaiPlus − NilaiMinus)`, `Terbayar = Σ(lunas.Nilai)`, `Sisa = Total + Potongan − Terbayar` |

Reports read persisted `Sisa`; they do not recalculate via builder.

#### Policies

**None.** Piutang reporting rules are in DAL SQL and UI filters.

### 2.4 Report Parameters and Default Filters (approved)

| Parameter | Portal V1 (approved) | Desktop reference |
| --------- | -------------------- | ----------------- |
| Date range | **`2000-01-01` → today** | Dashboard / `DashboardPiutangDal` |
| Open balance | **`KurangBayar > 1` (fixed)** | Desktop default when Show Paid off |
| Show Paid toggle | **No** | Desktop has checkbox — not in portal V1 |
| Grouping | Flat list | Desktop groups Sales → Wilayah |
| Search / date picker | **Not in V1** | Deferred post-M15 |

### 2.5 Business Rules and Calculations

| Rule | Where enforced |
| ---- | -------------- |
| Outstanding balance | `BTR_Piutang.Sisa` persisted; DAL exposes as `KurangBayar` |
| Payment type split | `BTR_PiutangLunas.JenisLunas`: 0=Cash, 1=Cek/BG, 2=UangMuka |
| Adjustment breakdown | `BTR_PiutangElement.ElementName` must match `'Retur'`, `'Potongan'`, `'Materai'`, `'Admin'` |
| "Open" threshold | **`KurangBayar > 1`** (not `> 0`) — desktop UI + dashboard |
| PiutangId = FakturId | 1:1 key throughout |
| No aging buckets | Only `JatuhTempo` exposed; no 30/60/90 logic |

SQL core (from `PiutangSalesWilayahDal`):

```sql
-- Filter: aa.PiutangDate BETWEEN @Tgl1 AND @Tgl2
-- KurangBayar = aa.Sisa (persisted, not computed in SELECT)
-- Payment splits via subqueries on BTR_PiutangLunas (JenisLunas 0/1)
-- Adjustments via BTR_PiutangElement grouped by ElementName
```

### 2.6 Report Columns

**Portal V1 (approved):** Customer, Sales, Faktur, Tanggal, Jatuh Tempo, Total Jual, Kurang Bayar.

**Deferred:** Wilayah, Alamat, Bayar Tunai, Bayar Giro, Retur, Potongan, Materai/Admin.

Desktop FF1 shows all 15 columns; portal uses reduced management-oriented set.

### 2.7 What Should Be Reused

| Asset | Reuse rationale |
| ----- | --------------- |
| `IPiutangSalesWilayahDal` + `PiutangSalesWilayahDto` | Same source as M5 dashboard; proven SQL |
| `DashboardPiutangDal` period + filter logic | Ensures report rows reconcile to dashboard KPI |
| `ITglJamDal` | Server date for `GeneratedAt` and period end |
| M9 `SalesReportAgg` structure | MediatR query, wrapper DAL, thin controller, Pinia store, DataTable view |
| Scrutor auto-registration | `IPiutangSalesWilayahDal` already registered via `IListData<,>` scan |

### 2.8 What Should NOT Be Duplicated

| Do not duplicate | Reason |
| ---------------- | ------ |
| Piutang SQL in `PiutangSalesWilayahDal` | Single source of truth for receivable breakdown |
| `Sisa` / `KurangBayar` calculation | Persisted on `BTR_Piutang`; builder owns write-path recalc |
| Payment/element aggregation subqueries | Already in DAL |
| Syncfusion grouping/hierarchical Excel export | Desktop-only presentation |
| `PelunasanInfoDal`, `PenerimaanPelunasanSalesDal` | Different report purposes (payments, not open balance) |
| `PiutangTrackerDal` | Single-invoice drill-down, not period report |

### 2.9 Secondary Assets (out of M10 V1 scope)

| Asset | Use case |
| ----- | -------- |
| `PelunasanInfoForm` / `IPelunasanInfoDal` | Payment activity report (future finance report) |
| `PenerimaanPelunasanSalesForm` | Collections by sales (future M14 analytics input) |
| `PiutangTrackerForm` | Invoice-level drill-down from report row |
| `IPIutangLunasViewDal` | Simpler header list if column reduction needed |

---

## 3. M11 Analysis — Inventory Report V1

### 3.1 Goal

Expose inventory detail behind the M6 Inventory Dashboard KPI (`Total Inventory Value`, `Total Item`). Report footer totals must reconcile with dashboard KPIs.

### 3.2 Approved Product Scope

| Aspect | Approved decision |
| ------ | ----------------- |
| Report type | Stock Balance only — Kartu Stok deferred to future Inventory Drilldown milestone |
| Row filter | `Qty > 0` only |
| Warehouse filter | Exclude `"In-Transit"` — match dashboard |
| Columns | Item, Warehouse, Qty, HPP, Nilai Sediaan (no QtyBesar/QtyKecil) |
| Footer totals | Total Inventory Value, Total Item |
| Filters | No date range or search in V1 |

### 3.3 Existing Desktop Assets

#### Report / Info screens (menu group IF*, parent 31 INFO)

| Menu | Screen | Path | Focus area |
| ---- | ------ | ---- | ---------- |
| IF1 | **`StokBalanceInfo2Form`** *(active)* | `InventoryContext/StokBalanceRpt/` | **Stock Balance — primary M11 source** |
| IF1 | `StokBalanceInfoForm` *(legacy)* | Same folder | Aggregates across warehouses by product |
| IF2 | `KartuStokInfoForm` | `InventoryContext/KartuStokRpt/` | **Kartu Stok** — per-product movement ledger |
| IF8 | `KartuStokSummaryForm` | Same folder | **Inventory Summary** — period movement by mutation type |
| IF4 | `StokBrgSupplierForm` | `InventoryContext/StokBrgSupplierRpt/` | Stock by supplier |
| IF5 | `StokPeriodikForm` | `InventoryContext/StokPeriodikRpt/` | Point-in-time stock from mutasi history |
| IF6 | `MutasiInfoForm` | `InventoryContext/MutasiRpt/` | Warehouse transfer movements |
| IF7 | `StokOpInfoForm` | `InventoryContext/OpnameAgg/` | Stock opname report |

#### DALs

| Interface | Implementation | Params | Data source |
| --------- | -------------- | ------ | ----------- |
| **`IStokBalanceViewDal`** | `StokBalanceViewDal` | None | `BTR_StokBalanceWarehouse` + dims |
| `IKartuStokDal` | `KartuStokDal` | `Periode`, `BrgId` | `BTR_StokMutasi` ledger |
| `IKartuStokSummaryDal` | `KartuStokSummaryDal` | `Periode`, `WarehouseId` | `BTR_StokMutasi` (mutation buckets) |
| `IStokPeriodikDal` | `StokPeriodikDal` | `DateTime` or `DateTime + WarehouseId` | `BTR_StokMutasi` cumulative |
| `IStokBrgSupplierDal` | `StokBrgSupplierDal` | None | Balance + MT/GT pricing |
| `IStokOpInfoDal` | `StokOpInfoDal` | `Periode` | `BTR_StokOp` |
| `IMutasiBrgViewDal` | `MutasiBrgViewDal` | `Periode` | `BTR_Mutasi` (non-void) |

**Portal (existing):** `DashboardInventoryDal` wraps `IStokBalanceViewDal`.

**Important data-source distinction:** Stock Balance reads **`BTR_StokBalanceWarehouse`** (denormalized, maintained at posting). Kartu Stok / Summary / Periodik read **`BTR_StokMutasi`** (transaction ledger). These must not be mixed for the same KPI without explicit product decision.

#### DTOs

**Primary report DTO — `StokBalanceView`**:

| Field | Meaning |
| ----- | ------- |
| `SupplierName`, `KategoriName` | Dimensions |
| `BrgId`, `BrgCode`, `BrgName` | Product identity |
| `WarehouseId`, `WarehouseName` | Warehouse |
| `QtyBesar`, `SatBesar`, `Conversion`, `QtyKecil`, `SatKecil` | Unit breakdown |
| `Qty` | Total pieces (`InPcs` in grid) |
| `Hpp` | Cost per piece |
| `NilaiSediaan` | `Hpp × Qty` (computed in form/dashboard, not in DAL) |

**Presentation DTO (desktop grid):** `StokBalanceInfoDto` — projects `StokBalanceView` with computed `QtyBesar`/`QtyKecil`.

**Kartu Stok DTOs:** `KartuStokView`, `KartuStokStokAwalView` — movement rows with running balance.

**Kartu Stok Summary DTO:** `KartuStokSummaryDto` — Invoice, Faktur, Retur, Mutasi, Opname buckets + StokAwal/Akhir, NilaiAwal/Moving/Akhir, HppAvg.

#### Queries

| Query | Status |
| ----- | ------ |
| `GetDashboardInventoryQuery` | Exists — delegates to `IDashboardInventoryDal` |
| `GetInventoryReportQuery` | **Does not exist** |

#### Builders

| Builder | Role |
| ------- | ---- |
| `StokBalanceBuilder` | Write-path: load/set warehouse qty for `StokBalanceModel` |
| `GenStokBalanceWorker` | Materializes balance from `SUM(BTR_Stok.Qty)` per Brg+Warehouse |
| `StokOpBuilder`, `OpnameBuilder` | Opname transactions (not reports) |

#### Policies

**None.** Rules embedded in form `Proses()` and DAL SQL.

### 3.4 Focus Areas

#### Stock Balance (M11 V1 — approved scope)

| Aspect | Detail |
| ------ | ------ |
| Screen | `StokBalanceInfo2Form` (menu IF1) |
| DAL | `IStokBalanceViewDal.ListData()` |
| Row filter | **`Qty > 0` only** (approved) |
| Warehouse filter | Exclude `"In-Transit"` |
| Columns | **Item, Warehouse, Qty, HPP, Nilai Sediaan** |
| Footer totals | Total Inventory Value, Total Item — use dashboard grouping logic (by `BrgId`) |
| Calculation | `NilaiSediaan = Hpp × Qty` |

**Dashboard alignment (`DashboardInventoryDal`):**

1. Load all `StokBalanceView` rows
2. Exclude `"In-Transit"`
3. Group by `BrgId`: `Qty = Sum(Qty)`, `NilaiSediaan = Sum(Hpp × Qty)`
4. `TotalInventoryValue = Sum(NilaiSediaan)`; `TotalItem = count where Qty > 0`

#### Kartu Stok (deferred — not in M11 V1)

| Aspect | Detail |
| ------ | ------ |
| Screen | `KartuStokInfoForm` (menu IF2) |
| Future milestone | Inventory Drilldown |

#### Inventory Summary (deferred — not in M11 V1)

| Aspect | Detail |
| ------ | ------ |
| Screen | `KartuStokSummaryForm` (menu IF8) |
| Future milestone | Inventory Drilldown |

### 3.5 What Should Be Reused

| Asset | Reuse rationale |
| ----- | --------------- |
| `IStokBalanceViewDal` + `StokBalanceView` | Same source as M6 dashboard |
| In-Transit exclusion logic from `DashboardInventoryDal` | KPI traceability |
| `NilaiSediaan = Hpp × Qty` computation | Matches desktop + dashboard |
| `StokBalanceInfoDto` column set | **Not used for V1** — approved columns are reduced subset |
| M9 frontend pattern | DataTable, Pinia store, formatters |

### 3.6 What Should NOT Be Duplicated

| Do not duplicate | Reason |
| ---------------- | ------ |
| `StokBalanceViewDal` SQL | Reads denormalized balance table |
| `GenStokBalanceWorker` / balance materialization | Write-path maintenance |
| Kartu Stok running-balance logic | Complex; belongs in `KartuStokDal` when drill-down is added |
| `KartuStokSummaryDal` mutation bucket SQL | Separate report type; different data source |
| Syncfusion grouping/filter bar | Desktop-only |
| Legacy `StokBalanceInfoForm` warehouse aggregation | Superseded by `StokBalanceInfo2Form` |

---

## 4. M12 Analysis — Purchasing Report V1

### 4.1 Goal

Provide visibility into purchasing activities. Standalone report — no dashboard anchor.

### 4.2 Approved Product Scope

| Aspect | Approved decision |
| ------ | ----------------- |
| Period | Current calendar month — same as M9 |
| Report source | PF1 header only (`IInvoiceViewDal`) |
| PostingStok column | Include — values `SUDAH` / `BELUM` |
| Footer totals | Grand Total Purchase, Total Invoice |
| Filters | No date range or search in V1 |

**Deferred:** PF2 line detail, PF3 daily detail, PF4 retur beli.

### 4.3 Terminology Note

BTR has **no separate Purchase Order entity**. Purchasing is modeled as **`Invoice`**:

| Business concept | BTR equivalent | Type |
| ---------------- | -------------- | ---- |
| Purchase order entry | `InvoiceForm` (PT1) | Transaction |
| Receiving / stock posting | `PostingStokForm` (PT2) | Transaction |
| Purchasing summary (header) | `InvoiceInfoForm` (PF1) | **Report — primary M12 source** |
| Purchasing summary (line) | `InvoiceBrgInfoForm` (PF2) | Report |
| Daily purchase detail | `InvoiceHarianDetilForm` (PF3) | Report |
| Purchase return detail | `ReturBeliBrgInfoForm` (PF4) | Report |

### 4.4 Existing Desktop Assets

#### Report / Info screens

| Menu | Screen | Path | Purpose |
| ---- | ------ | ---- | ------- |
| PF1 | **`InvoiceInfoForm`** | `PurchaseContext/InvoiceInfo/` | Header-level purchasing: invoice totals + posting status |
| PF2 | `InvoiceBrgInfoForm` | Same folder | Line-item purchasing detail |
| PF3 | `InvoiceHarianDetilForm` | `PurchaseContext/InvoiceHarianDetilRpt/` | Full discount/PPN breakdown per line |
| PF4 | `ReturBeliBrgInfoForm` | `PurchaseContext/ReturBeliFeature/` | Purchase return detail |
| PT2 | `PostingStokForm` | `PurchaseContext/PostingStokAgg/` | Receiving operations (not a report) |

#### DALs

| Interface | Implementation | Filter | Returns |
| --------- | -------------- | ------ | ------- |
| **`IInvoiceViewDal`** | `InvoiceViewDal` | `Periode` on `InvoiceDate` | `InvoiceView` |
| `IInvoiceBrgViewDal` | `InvoiceBrgViewDal` | `Periode` | `InvoiceBrgViewDto` |
| `IInvoiceHarianDetilDal` | `InvoiceHarianDetilDal` | `Periode` | `InvoiceHarianDetilView` |
| `IReturBeliBrgViewDal` | `ReturBeliBrgViewDal` | `Periode` | `ReturBeliBrgViewDto` |

All reporting DALs exclude voided records: `VoidDate = '3000-01-01'`.

#### DTOs

**Primary report DTO — `InvoiceView`**:

| Field | Meaning |
| ----- | ------- |
| `InvoiceId`, `InvoiceCode`, `Tgl` | Invoice identity |
| `SupplierName`, `WarehouseName` | Dimensions |
| `Total`, `Disc`, `Tax`, `GrandTotal` | Persisted header totals |
| `PostingStok` | `"SUDAH"` / `"BELUM"` from `IsStokPosted` — **receiving status** |

**Line-level DTOs:** `InvoiceBrgViewDto` (PF2), `InvoiceHarianDetilView` (PF3 with 4-tier discount), `ReturBeliBrgViewDto` (PF4).

#### Queries

**No purchasing report MediatR queries exist.** PurchaseContext queries are master-data only (`GetSupplierQuery`, `ListSupplierQuery`).

#### Builders

| Builder | Role |
| ------- | ---- |
| `InvoiceBuilder` | `CalcTotal()`: `Total = Sum(SubTotal)`, `Disc = Sum(DiscRp)`, `Tax = Sum(PpnRp)`, `GrandTotal = Sum(Total)`; `IsPosted(bool)` for receiving |
| `CreateInvoiceItemWorker` | Line calculation: qty conversion, cascading 4-tier discount, DPP, PPN |
| `GenStokInvoiceWorker` | Receiving: adds stock at `HppSat`; bonus qty at HPP=0 |

#### Policies

**None for reporting.** UI enforces max 122-day period on PF1/PF2/PF4. Void filter in DAL SQL.

### 4.5 Report Parameters and Default Filters (approved)

| Parameter | Portal V1 (approved) | Desktop reference |
| --------- | -------------------- | ----------------- |
| Date range | **Current calendar month** | Desktop user-selected, max 122 days |
| Void filter | Excluded at DAL | Same |
| PostingStok | **Included** | Same as PF1 |
| Keyword search | **Not in V1** | Desktop has supplier/invoice search |

### 4.6 Report Columns (approved)

Invoice, Date, Supplier, Warehouse, Total, Disc, Tax, Grand Total, **Posting Stok**.

### 4.7 Focus Areas

| Focus | M12 V1 mapping | Asset |
| ----- | -------------- | ----- |
| **Purchase Order** | Invoice header list | `IInvoiceViewDal` / PF1 |
| **Receiving** | `PostingStok` column (`SUDAH`/`BELUM`) | Same DAL — no need for `PostingStokForm` |
| **Purchasing Summary** | Header totals per invoice | PF1; line detail deferred to PF2/PF3 |

### 4.8 What Should Be Reused

| Asset | Reuse rationale |
| ----- | --------------- |
| `IInvoiceViewDal` + `InvoiceView` | Closest analog to M9's `IFakturViewDal` + `FakturView` |
| Void filter in DAL | Consistent with all desktop reports |
| `PostingStok` display logic | Receiving visibility without transaction exposure |
| M9 `SalesReportAgg` pattern | Same MediatR + wrapper DAL + DataTable structure |
| Current-month period default | Consistent portal UX across report pages |

### 4.9 What Should NOT Be Duplicated

| Do not duplicate | Reason |
| ---------------- | ------ |
| `InvoiceViewDal` SQL | Single source for header report |
| `InvoiceBuilder.CalcTotal()` / line discount logic | Persisted on invoice; builder owns write path |
| `GenStokInvoiceWorker` posting logic | Transactional receiving |
| PF3 4-tier discount pivot SQL | Overkill for V1 summary report |
| RDLC print templates | Different presentation channel |
| 122-day period validation | Portal V1 uses fixed current month (no user period picker) |

---

## 5. Shared Opportunities

### 5.1 Cross-Milestone Comparison

| Milestone | Existing Screen | Existing DAL | Complexity | Risk |
| --------- | --------------- | ------------ | ---------- | ---- |
| **M10** Piutang | `PiutangSalesWilayahForm` (FF1) | `IPiutangSalesWilayahDal` | **Low** — 7 columns; decisions finalize scope | **Low** — period, columns, filter approved |
| **M11** Inventory | `StokBalanceInfo2Form` (IF1) | `IStokBalanceViewDal` | **Low** — 5 columns; Qty > 0 reduces rows | **Low** — Kartu Stok explicitly deferred |
| **M12** Purchasing | `InvoiceInfoForm` (PF1) | `IInvoiceViewDal` | **Low** — mirrors M9; PostingStok included | **Low** — structurally proven |

### 5.2 Reusable DataTable Patterns (from M9)

| Pattern | M9 implementation | Reusable for M10–M12 |
| ------- | ------------------- | -------------------- |
| Page layout | Header + period label + Refresh button + Card wrapper | Yes — copy structure |
| DataTable config | PrimeVue: paginator, 25 rows default, options 10/25/50/100, striped, removable-sort | Yes — identical |
| Loading state | `:loading` on DataTable + Refresh button spinner | Yes |
| Empty state | Custom `#empty` template with icon + message | Yes — domain-specific message |
| Column formatters | `formatDate()`, `formatCurrency()` from `@/services/formatters` | Yes — all three reports need both |
| Error display | PrimeVue `Message` severity error | Yes |
| Generated timestamp | Footer showing `GeneratedAt` | Yes |
| **Footer totals** | Not in M9 | **Yes — M10/M11/M12 approved** (summary bar below table) |

### 5.3 Reusable Frontend Architecture (from M9)

| Layer | M9 files | M10–M12 equivalent |
| ----- | -------- | ------------------ |
| Types | `src/models/reports.ts` | Extend with Piutang/Inventory/Purchase response types |
| API | `src/api/reportsApi.ts` | Add fetch functions per endpoint |
| Store | `src/stores/salesReportStore.ts` | One Pinia store per report (same shape: report, loading, error, loadReport) |
| View | `src/views/reports/SalesReportView.vue` | One view per report |
| Router | `/reports/sales` | `/reports/piutang`, `/reports/inventory`, `/reports/purchasing` |
| Sidebar | Reports menu group | Add entries under existing Reports group |

### 5.4 Reusable Backend Patterns (from M9 + M3)

| Pattern | Description |
| ------- | ----------- |
| `ReportingContext/{Domain}ReportAgg/` | Application: contract + MediatR query/handler/response DTOs |
| `{Domain}ReportDal` | Infrastructure: wraps existing desktop DAL, maps to portal row DTO |
| `{Domain}ReportController` | Portal API: thin MediatR delegate, JWT required |
| Scrutor scan | Desktop DALs auto-registered via `IListData<,>` — wrapper DAL registered explicitly |
| `ITglJamDal` | Server time for period boundaries and `GeneratedAt` |

### 5.5 Shared Filtering Patterns

| Filter type | M9 | M10 | M11 | M12 |
| ----------- | -- | --- | --- | --- |
| Period | Current month (fixed) | **2000→today (fixed)** | Snapshot (no period) | Current month (fixed) |
| Void/deleted exclusion | DAL-level | N/A | N/A | DAL-level |
| Open balance | N/A | **`KurangBayar > 1` (fixed)** | N/A | N/A |
| Zero qty rows | N/A | N/A | **Exclude `Qty = 0`** | N/A |
| Warehouse exclusion | N/A | N/A | Exclude In-Transit | N/A |
| Show paid toggle | N/A | **No** | N/A | N/A |
| Date range / search | **No** | **No** | **No** | **No** |

**V1 convention (approved):** Fixed defaults only. Date range, search, and advanced filtering deferred to post-M15 enhancement phase.

### 5.6 Shared Pagination Patterns

M9 uses **client-side pagination**. M11 row count reduced by `Qty > 0` filter. Server-side pagination deferred to post-M15 filtering phase.

### 5.7 Dashboard Traceability Matrix

| Dashboard KPI | Report that validates it | Shared DAL | Reconciliation rule |
| ------------- | ------------------------ | ---------- | ------------------- |
| M4/M8 Total Omzet | M9 Sales Report | `IFakturViewDal` | Sum of `FakturTotal` ≈ dashboard omzet (different period scopes) |
| M5 Total Piutang | **M10 footer** | `IPiutangSalesWilayahDal` | Sum of `KurangBayar` where `> 1` = `TotalPiutang` |
| M5 Total Customer | **M10 footer** | Same | Distinct customer count = `TotalCustomer` |
| M6 Total Inventory Value | **M11 footer** | `IStokBalanceViewDal` | Grouped sum = `TotalInventoryValue` |
| M6 Total Item | **M11 footer** | Same | Count BrgId where Qty > 0 = `TotalItem` |
| — | M12 footer | `IInvoiceViewDal` | Grand Total Purchase, Total Invoice — no dashboard anchor |

---

## 6. Risks

| Risk | Milestone | Severity | Status / Mitigation |
| ---- | --------- | -------- | ------------------- |
| Period semantics mismatch (M10 vs M9) | M10 | ~~Medium~~ | **Resolved** — Decision 1: dashboard period |
| Column overload on Piutang grid | M10 | ~~Low~~ | **Resolved** — Decision 2: 7 columns |
| Show Paid toggle expectation | M10 | ~~Low~~ | **Resolved** — Decision 3: fixed filter |
| Large row counts | M10, M11 | Medium | M11 mitigated by Qty > 0; monitor M10 open receivables |
| M11 footer vs row-level aggregation | M11 | Medium | Footer uses dashboard grouping by BrgId — do not sum visible rows naively |
| Two inventory data sources | M11 | Low | **Contained** — Decision 6: Stock Balance only |
| No purchasing dashboard | M12 | Low | Accepted — standalone visibility |
| In-Transit warehouse hardcoded string | M11 | Low | Reuse constant from `DashboardInventoryDal` |
| `KurangBayar > 1` threshold | M10 | Low | Match dashboard; do not change |
| M9 lacks footer totals | All | Low | M10–M12 introduce pattern; M9 unchanged |
| DAL wrapper DI registration | All | Low | Follow M9 pattern |
| Purchasing "Invoice" terminology | M12 | Low | UI labels: "Purchasing Report" |

---

## 7. Recommended Delivery Order

| Order | Milestone | Rationale |
| ----- | --------- | --------- |
| **1** | M10 Piutang Report | Dashboard traceability; decisions reduce design ambiguity |
| **2** | M11 Inventory Report | Dashboard traceability; Kartu Stok explicitly out of scope |
| **3** | M12 Purchasing Report | Mirrors M9; no dashboard dependency |

**Parallelization:** M10 and M11 remain independent. Approved decisions provide sufficient specification for parallel Architect work.

**Note:** M9 Sales Report does not include footer totals. M10–M12 introduce this pattern.

---

## 8. Final Product Decisions

All decisions approved by Product Owner. **Authoritative detail:** [`portal-analysis-m10-m12-final.md`](./portal-analysis-m10-m12-final.md)

| # | Decision | Outcome |
| - | -------- | ------- |
| 1 | M10 period scope | Dashboard period: `2000-01-01` → today |
| 2 | M10 column scope | 7 columns — reduced management set |
| 3 | M10 Show Paid toggle | No — fixed `KurangBayar > 1` filter |
| 4 | M11 zero-qty rows | Exclude — `Qty > 0` only |
| 5 | M11 unit breakdown | No QtyBesar/QtyKecil — 5 columns |
| 6 | M11 Kartu Stok | Not in V1 — Stock Balance only |
| 7 | M12 default period | Current month (same as M9) |
| 8 | M12 PostingStok | Include — `SUDAH` / `BELUM` |
| 9 | Date range filters | Not in M10–M12 — deferred post-M15 |
| 10 | Footer totals | Yes — all three reports |
| 11 | Purchasing detail | PF1 only — PF2/PF3 deferred |
| 12 | M14 influence on M10 | Jatuh Tempo prominent; column order: Customer, Sales, Faktur, Tanggal, Jatuh Tempo, Total Jual, Kurang Bayar |

---

## Appendix A — File Index

### M10 Piutang

| Category | Path |
| -------- | ---- |
| Screen | `btr.distrib/FinanceContext/PiutangSalesWilayahRpt/PiutangSalesWilayahForm.cs` |
| DAL contract | `btr.application/FinanceContext/PiutangAgg/Contracts/IPiutangSalesWilayahDal.cs` |
| DAL impl | `btr.infrastructure/FinanceContext/PiutangSalesWilayahRpt/PiutangSalesWilayahDal.cs` |
| Dashboard | `btr.infrastructure/ReportingContext/DashboardPiutangAgg/DashboardPiutangDal.cs` |
| Builder | `btr.application/FinanceContext/PiutangAgg/Workers/PiutangBuilder.cs` |

### M11 Inventory

| Category | Path |
| -------- | ---- |
| Screen | `btr.distrib/InventoryContext/StokBalanceRpt/StokBalanceInfo2Form.cs` |
| DAL contract | `btr.application/InventoryContext/StokBalanceInfo/IStokBalanceViewDal.cs` |
| DTO | `btr.application/InventoryContext/StokBalanceInfo/StokBalanceView.cs` |
| Dashboard | `btr.infrastructure/ReportingContext/DashboardInventoryAgg/DashboardInventoryDal.cs` |
| Kartu Stok | `btr.distrib/InventoryContext/KartuStokRpt/KartuStokInfoForm.cs` |
| Kartu Stok Summary | `btr.distrib/InventoryContext/KartuStokRpt/KartuStokSummaryForm.cs` |

### M12 Purchasing

| Category | Path |
| -------- | ---- |
| Screen | `btr.distrib/PurchaseContext/InvoiceInfo/InvoiceInfoForm.cs` |
| DAL contract | `btr.application/PurchaseContext/InvoiceInfo/IInvoiceViewDal.cs` |
| DTO | `btr.application/PurchaseContext/InvoiceInfo/InvoiceView.cs` |
| DAL impl | `btr.infrastructure/PurchaseContext/InvoiceInfoRpt/InvoiceViewDal.cs` |

### M9 Reference Pattern

| Category | Path |
| -------- | ---- |
| Query | `btr.application/ReportingContext/SalesReportAgg/Queries/GetSalesReportQuery.cs` |
| Wrapper DAL | `btr.infrastructure/ReportingContext/SalesReportAgg/SalesReportDal.cs` |
| Controller | `btr.portal.api/Controllers/Reports/SalesReportController.cs` |
| Frontend | `btr.portal.web/src/views/reports/SalesReportView.vue` |
