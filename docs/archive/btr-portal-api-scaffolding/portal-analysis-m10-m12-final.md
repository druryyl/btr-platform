# BTR Portal Analysis — Milestones M10–M12 (Final)

**Status:** Product decisions approved — authoritative input for Architect.  
**Scope:** Discovery and analysis with finalized product decisions. No implementation plans.  
**Date:** 2026-06-06  
**Supersedes:** Open Questions in `portal-analysis-m10-m12.md`  
**Context:** BTR Portal is a read-only reporting and dashboard application. M1–M9 are complete.

**Reference documents:** `btr-portal-milestone.md`, `PRODUCT.md`, `DOMAIN.md`, `LANDSCAPE.md`, `implementation-summary-milestone-9.md`

---

## 1. Executive Summary

M10–M12 extend the pattern established in M9 (Sales Report V1): a thin `ReportingContext` wrapper over an existing desktop DAL, exposed via MediatR and rendered in a PrimeVue DataTable with **footer summary totals**. Product Owner has approved scope, columns, filters, and period conventions for all three milestones.

| Milestone | Primary desktop screen | Primary DAL | Dashboard link | V1 scope (approved) |
| --------- | ---------------------- | ----------- | -------------- | ------------------- |
| **M10** Piutang Report | FF1 `PiutangSalesWilayahForm` | `IPiutangSalesWilayahDal` | M5 — KPI traceability required | 7 columns; period 2000→today; hide paid; footer totals |
| **M11** Inventory Report | IF1 `StokBalanceInfo2Form` | `IStokBalanceViewDal` | M6 — KPI traceability required | 5 columns; Qty > 0 only; Stock Balance only; footer totals |
| **M12** Purchasing Report | PF1 `InvoiceInfoForm` | `IInvoiceViewDal` | None | Current month; PF1 header; PostingStok column; footer totals |

**Key architectural finding:** Piutang, Inventory, and Purchasing reporting have **no dedicated Policy classes**. Business rules live in DAL SQL, aggregate builders (write path), and WinForms UI filters. Portal wrappers should delegate to existing DALs and apply only presentation-level mapping — not reimplement calculations.

**Cross-cutting V1 conventions (approved):**

- Fixed period defaults — no date-range query parameters (Decision 9)
- No search or advanced filtering (Decision 9)
- Footer summary totals on all three report pages (Decision 10)
- Client-side DataTable pagination (same as M9)

**Recommended delivery order:** M10 → M11 → M12. Product decisions reduce prior uncertainty; parallel development of M10 and M11 remains feasible.

---

## 2. M10 Analysis — Piutang Report V1

### 2.1 Goal

Expose receivable detail behind the M5 Piutang Dashboard KPI (`Total Piutang`, `Total Customer`). Report totals must reconcile with dashboard KPIs.

### 2.2 Approved Product Scope

| Aspect | Approved decision |
| ------ | ----------------- |
| **Period** | `2000-01-01` → today — match `DashboardPiutangDal` (Decision 1) |
| **Open balance filter** | `KurangBayar > 1` only — no "Show Paid" toggle (Decision 3) |
| **Columns** | Reduced set — 7 primary columns (Decision 2, 12) |
| **Footer totals** | Total Piutang, Total Customer (Decision 10) |
| **Date/search filters** | Not in V1 (Decision 9) |

#### Approved column set and order

| # | Column | Source field | Notes |
| - | ------ | ------------ | ----- |
| 1 | Customer | `CustomerName` | First column — prepares for M14 aging |
| 2 | Sales | `SalesName` | |
| 3 | Faktur | `FakturCode` | |
| 4 | Tanggal | `FakturDate` | |
| 5 | Jatuh Tempo | `JatuhTempo` | Prominent — prepares for M14 aging |
| 6 | Total Jual | `TotalJual` | |
| 7 | Kurang Bayar | `KurangBayar` | |

**Deferred columns (not in V1):** Wilayah, Customer Code, Alamat, Bayar Tunai, Bayar Giro, Retur, Potongan, Materai/Admin.

#### Footer totals (must reconcile with M5 dashboard)

| Footer metric | Calculation | Dashboard field |
| ------------- | ----------- | --------------- |
| Total Piutang | `Sum(KurangBayar)` where `KurangBayar > 1` | `TotalPiutang` |
| Total Customer | Distinct customer count (same key logic as `DashboardPiutangDal.ResolveCustomerKey`) | `TotalCustomer` |

### 2.3 Existing Desktop Assets

#### Primary source

| Menu | Screen | DAL | Purpose |
| ---- | ------ | --- | ------- |
| FF1 | `PiutangSalesWilayahForm` | `IPiutangSalesWilayahDal` | Per-faktur receivable breakdown |

**Secondary screens (out of M10 V1):** `PelunasanInfoForm` (FF4), `PenerimaanPelunasanSalesForm` (FF2), `PiutangTrackerForm` (FT5).

#### DAL and DTO

- **DAL:** `IPiutangSalesWilayahDal` → `PiutangSalesWilayahDal`
- **Filter parameter:** `Periode` on `BTR_Piutang.PiutangDate`
- **DTO:** `PiutangSalesWilayahDto` — portal maps subset of fields to `PiutangReportRow`
- **Portal dashboard:** `DashboardPiutangDal` — reuse period and filter logic verbatim

#### Business rules (do not reimplement)

| Rule | Where enforced |
| ---- | -------------- |
| Outstanding balance | `BTR_Piutang.Sisa` → `KurangBayar` |
| Open threshold | **`KurangBayar > 1`** |
| Period | `PiutangDate BETWEEN @Tgl1 AND @Tgl2` |
| PiutangId = FakturId | 1:1 throughout |

### 2.4 What Should Be Reused

| Asset | Reuse rationale |
| ----- | --------------- |
| `IPiutangSalesWilayahDal` | Same source as M5 dashboard |
| `DashboardPiutangDal.OpenReceivablesPeriode()` logic | Period 2000→today |
| `DashboardPiutangDal` filter + customer key logic | KPI reconciliation |
| M9 `SalesReportAgg` structure | MediatR + wrapper DAL + controller + Pinia + DataTable |

### 2.5 What Should NOT Be Duplicated

| Do not duplicate | Reason |
| ---------------- | ------ |
| Piutang SQL in `PiutangSalesWilayahDal` | Single source of truth |
| Payment/element aggregation subqueries | Already in DAL; deferred columns not exposed |
| Syncfusion grouping / Excel export | Desktop-only |
| Show Paid toggle behavior | Not in V1; filter is fixed |

---

## 3. M11 Analysis — Inventory Report V1

### 3.1 Goal

Expose inventory detail behind the M6 Inventory Dashboard KPI (`Total Inventory Value`, `Total Item`). Report footer totals must reconcile with dashboard KPIs.

### 3.2 Approved Product Scope

| Aspect | Approved decision |
| ------ | ----------------- |
| **Report type** | Stock Balance only — no Kartu Stok (Decision 6) |
| **Row filter** | `Qty > 0` only — exclude zero-qty rows (Decision 4) |
| **Warehouse filter** | Exclude `"In-Transit"` — match dashboard (existing M6 behavior) |
| **Columns** | 5 primary columns — no unit breakdown (Decision 5) |
| **Footer totals** | Total Inventory Value, Total Item (Decision 10) |
| **Date/search filters** | Not in V1 (Decision 9) |

#### Approved column set

| Column | Source field | Notes |
| ------ | ------------ | ----- |
| Item | `BrgCode` + `BrgName` | Single display column |
| Warehouse | `WarehouseName` | |
| Qty | `Qty` | Pieces (desktop `InPcs`) |
| HPP | `Hpp` | |
| Nilai Sediaan | `Hpp × Qty` | Computed — same as dashboard |

**Deferred columns:** Supplier, Kategori, QtyBesar, QtyKecil, SatBesar, SatKecil, Conversion.

#### Footer totals (must reconcile with M6 dashboard)

| Footer metric | Calculation | Dashboard field |
| ------------- | ----------- | --------------- |
| Total Inventory Value | Sum of `NilaiSediaan` after filters, grouped by `BrgId` then summed | `TotalInventoryValue` |
| Total Item | Count of distinct `BrgId` where aggregated `Qty > 0` | `TotalItem` |

**Note:** Row-level report shows product × warehouse rows; footer aggregation follows `DashboardInventoryDal` grouping logic (by `BrgId`), not a simple sum of visible rows.

### 3.3 Existing Desktop Assets

#### Primary source

| Menu | Screen | DAL | Purpose |
| ---- | ------ | --- | ------- |
| IF1 | `StokBalanceInfo2Form` | `IStokBalanceViewDal` | Stock balance grid |

**Deferred to future "Inventory Drilldown" milestone:** `KartuStokInfoForm` (IF2), `KartuStokSummaryForm` (IF8), and all `BTR_StokMutasi`-based DALs.

#### Data source

Stock Balance reads **`BTR_StokBalanceWarehouse`** (denormalized). Kartu Stok reads **`BTR_StokMutasi`** (ledger). M11 V1 uses balance table only — do not mix sources.

### 3.4 What Should Be Reused

| Asset | Reuse rationale |
| ----- | --------------- |
| `IStokBalanceViewDal` + `StokBalanceView` | Same source as M6 dashboard |
| In-Transit exclusion from `DashboardInventoryDal` | KPI traceability |
| `NilaiSediaan = Hpp × Qty` | Matches desktop + dashboard |
| Dashboard grouping logic for footer totals | Ensures reconciliation |

### 3.5 What Should NOT Be Duplicated

| Do not duplicate | Reason |
| ---------------- | ------ |
| `StokBalanceViewDal` SQL | Single source of truth |
| `StokBalanceInfoDto` unit conversion | QtyBesar/QtyKecil deferred |
| Kartu Stok running-balance logic | Out of M11 V1 scope |
| Zero-qty rows | Product decision — excluded |

---

## 4. M12 Analysis — Purchasing Report V1

### 4.1 Goal

Provide visibility into purchasing activities. Standalone report — no dashboard anchor.

### 4.2 Approved Product Scope

| Aspect | Approved decision |
| ------ | ----------------- |
| **Period** | Current calendar month — same as M9 Sales Report (Decision 7) |
| **Report source** | PF1 header only — `IInvoiceViewDal` (Decision 11) |
| **PostingStok column** | Include — values `SUDAH` / `BELUM` (Decision 8) |
| **Footer totals** | Grand Total Purchase, Total Invoice (Decision 10) |
| **Date/search filters** | Not in V1 (Decision 9) |

#### Approved columns (PF1 header)

| Column | Source field |
| ------ | ------------ |
| Invoice | `InvoiceCode` |
| Date | `Tgl` |
| Supplier | `SupplierName` |
| Warehouse | `WarehouseName` |
| Total | `Total` |
| Disc | `Disc` |
| Tax | `Tax` |
| Grand Total | `GrandTotal` |
| Posting Stok | `PostingStok` |

**Deferred:** PF2 line detail, PF3 daily detail, PF4 retur beli.

#### Footer totals

| Footer metric | Calculation |
| ------------- | ----------- |
| Grand Total Purchase | `Sum(GrandTotal)` |
| Total Invoice | Count of invoice rows |

### 4.3 Existing Desktop Assets

| Menu | Screen | DAL |
| ---- | ------ | --- |
| PF1 | `InvoiceInfoForm` | `IInvoiceViewDal` |
| PT2 | `PostingStokForm` | Operational — not used; receiving status via `PostingStok` column |

**Terminology:** BTR has no separate Purchase Order entity. Purchasing = **`Invoice`**.

### 4.4 What Should Be Reused

| Asset | Reuse rationale |
| ----- | --------------- |
| `IInvoiceViewDal` + `InvoiceView` | Closest analog to M9 `IFakturViewDal` |
| `SalesReportDal.CurrentMonthPeriode()` pattern | Same period convention |
| Void filter in DAL (`VoidDate = '3000-01-01'`) | Consistent with desktop |
| `PostingStok` from DAL | Receiving visibility without PT2 exposure |

### 4.5 What Should NOT Be Duplicated

| Do not duplicate | Reason |
| ---------------- | ------ |
| `InvoiceViewDal` SQL | Single source of truth |
| PF2/PF3 line-level SQL | Deferred |
| `InvoiceBuilder` / `GenStokInvoiceWorker` | Write-path only |
| 122-day period validation | V1 uses fixed current month |

---

## 5. Shared Opportunities

### 5.1 Cross-Milestone Comparison

| Milestone | Existing Screen | Existing DAL | Complexity | Risk |
| --------- | --------------- | ------------ | ---------- | ---- |
| **M10** Piutang | `PiutangSalesWilayahForm` (FF1) | `IPiutangSalesWilayahDal` | **Low** — 7 columns; dashboard logic reusable | **Low** — decisions finalize period, columns, filter |
| **M11** Inventory | `StokBalanceInfo2Form` (IF1) | `IStokBalanceViewDal` | **Low** — 5 columns; Qty > 0 reduces row count | **Low** — Kartu Stok explicitly deferred |
| **M12** Purchasing | `InvoiceInfoForm` (PF1) | `IInvoiceViewDal` | **Low** — mirrors M9; PostingStok adds one column | **Low** — structurally proven |

### 5.2 Reusable DataTable Patterns

| Pattern | M9 (delivered) | M10–M12 (approved extension) |
| ------- | -------------- | ---------------------------- |
| Page layout | Header + period label + Refresh + Card | Same |
| DataTable | Paginator 25 default; sortable columns | Same |
| Loading / empty / error states | PrimeVue patterns | Same |
| Formatters | `formatDate()`, `formatCurrency()` | Same |
| **Footer totals** | Not in M9 | **New — summary bar below table** |
| Generated timestamp | Footer area | Same |

### 5.3 Reusable Frontend Architecture

| Layer | Pattern |
| ----- | ------- |
| Types | Extend `src/models/reports.ts` per report + footer summary fields in response |
| API | Extend `src/api/reportsApi.ts` |
| Store | One Pinia store per report; expose summary totals from API response |
| View | One view per report; summary totals rendered outside DataTable |
| Routes | `/reports/piutang`, `/reports/inventory`, `/reports/purchasing` |

### 5.4 Reusable Backend Patterns

| Pattern | Description |
| ------- | ----------- |
| `ReportingContext/{Domain}ReportAgg/` | MediatR query + response DTO including summary totals |
| `{Domain}ReportDal` | Wraps desktop DAL; applies approved filters; computes footer totals using dashboard logic where applicable |
| Response shape | `{ PeriodFrom, PeriodTo, GeneratedAt, Summary, Rows }` |
| Dashboard alignment | M10/M11 footer totals computed with same logic as respective `Dashboard*Dal` |

### 5.5 Approved Filtering Patterns

| Filter | M9 | M10 | M11 | M12 |
| ------ | -- | --- | --- | --- |
| Period | Current month | **2000-01-01 → today** | Snapshot (no period) | Current month |
| Open balance | — | **`KurangBayar > 1` (fixed)** | — | — |
| Zero qty | — | — | **Exclude `Qty = 0`** | — |
| In-Transit warehouse | — | — | **Exclude** | — |
| Void invoices | DAL | — | — | DAL |
| Show paid toggle | — | **No** | — | — |
| Date range params | No | **No** | **No** | **No** |
| Search | No | **No** | **No** | **No** |

**Future enhancement (post-M15):** date range filters, search, advanced filtering (Decision 9).

### 5.6 Pagination

Client-side pagination (same as M9). M11 row count reduced by `Qty > 0` filter. Server-side pagination deferred to post-M15 filtering phase.

### 5.7 Dashboard Traceability Matrix

| Dashboard KPI | Report | Reconciliation rule |
| ------------- | ------ | ------------------- |
| M5 Total Piutang | M10 footer | `Sum(KurangBayar)` where `> 1` |
| M5 Total Customer | M10 footer | Distinct customer count |
| M6 Total Inventory Value | M11 footer | Grouped `Sum(Hpp × Qty)` excl. In-Transit |
| M6 Total Item | M11 footer | Count `BrgId` where aggregated Qty > 0 |
| — | M12 footer | Standalone — no dashboard anchor |

---

## 6. Risks

| Risk | Milestone | Severity | Status / Mitigation |
| ---- | --------- | -------- | ------------------- |
| Period semantics mismatch (M10 vs M9) | M10 | ~~Medium~~ | **Resolved** — Decision 1: dashboard period; document in report header |
| Column overload on Piutang grid | M10 | ~~Low~~ | **Resolved** — Decision 2: 7 columns |
| Show Paid toggle expectation | M10 | ~~Low~~ | **Resolved** — Decision 3: fixed filter, no toggle |
| Large row counts | M10, M11 | Medium | M11 mitigated by Qty > 0 filter; monitor M10 open receivables volume |
| Two inventory data sources | M11 | Low | **Contained** — Decision 6: Stock Balance only; Kartu Stok deferred |
| M11 footer vs row-level aggregation mismatch | M11 | Medium | Footer uses dashboard grouping (by BrgId); document in API; do not sum visible rows naively |
| No purchasing dashboard | M12 | Low | Accepted — standalone visibility |
| In-Transit warehouse hardcoded string | M11 | Low | Reuse constant from `DashboardInventoryDal` |
| `KurangBayar > 1` threshold undocumented in DOMAIN.md | M10 | Low | Match dashboard; do not change |
| M9 lacks footer totals | All | Low | M10–M12 introduce pattern; M9 unchanged unless separately requested |
| DAL wrapper DI registration | All | Low | Follow M9 explicit registration pattern |
| Purchasing "Invoice" terminology | M12 | Low | UI labels: "Purchasing Report" / "Invoice" |

---

## 7. Recommended Delivery Order

| Order | Milestone | Rationale |
| ----- | --------- | --------- |
| **1** | M10 Piutang Report | Dashboard traceability; Finance reporting pair with M9; decisions reduce design ambiguity |
| **2** | M11 Inventory Report | Dashboard traceability; simpler column set; Kartu Stok explicitly out of scope |
| **3** | M12 Purchasing Report | No dashboard dependency; mirrors M9 structurally |

**Parallelization:** M10 and M11 remain independent and can be developed in parallel. Approved decisions on columns, filters, and footer totals provide sufficient specification for parallel Architect work.

**Enhancement note:** M9 Sales Report does not include footer totals. M10–M12 introduce this pattern. Retrofitting M9 is out of scope unless separately requested.

---

## 8. Final Product Decisions

All decisions approved by Product Owner. Treat as final unless a critical technical limitation is discovered during implementation.

### Decision 1 — M10 Period Scope

**Decision:** Use dashboard period (`2000-01-01` → today).

**Implementation direction:** Match `DashboardPiutangDal` behavior. Maintain KPI traceability — report footer must reconcile with M5 `Total Piutang` and `Total Customer`.

---

### Decision 2 — M10 Column Scope

**Decision:** Reduce columns for V1.

**Primary columns:** Sales, Customer, Faktur, Tanggal, Jatuh Tempo, Total Jual, Kurang Bayar.

**Deferred:** Bayar Tunai, Bayar Giro, Retur, Potongan, Materai, Admin, Wilayah, Alamat.

**Rationale:** Management-oriented readability.

---

### Decision 3 — M10 Show Paid Toggle

**Decision:** No toggle.

**Implementation direction:** Hide paid invoices. Fixed filter: `KurangBayar > 1`. Follow dashboard behavior.

---

### Decision 4 — M11 Zero Quantity Rows

**Decision:** Exclude zero-qty rows.

**Implementation direction:** Only show records where `Qty > 0`.

---

### Decision 5 — M11 Unit Breakdown Columns

**Decision:** No QtyBesar / QtyKecil for V1.

**Primary columns:** Item, Warehouse, Qty, HPP, Nilai Sediaan.

**Deferred:** QtyBesar, QtyKecil, Conversion details, Supplier, Kategori.

---

### Decision 6 — M11 Kartu Stok

**Decision:** Not included in M11 V1.

**Implementation direction:** Stock Balance only. Kartu Stok, movement analysis, and running balance history deferred to a potential future **Inventory Drilldown** milestone.

---

### Decision 7 — M12 Default Period

**Decision:** Current month.

**Implementation direction:** Same period convention as M9 Sales Report (`SalesReportDal.CurrentMonthPeriode()`).

---

### Decision 8 — M12 PostingStok Column

**Decision:** Include PostingStok column.

**Values:** `SUDAH`, `BELUM`.

**Rationale:** Receiving visibility without exposing transactional workflows.

---

### Decision 9 — Date Range Filters

**Decision:** Not in M10–M12.

**Implementation direction:** Fixed defaults only. Future enhancement phase (post-M15) may introduce date range filters, search, and advanced filtering.

**Rationale:** Current priority is breadth of reporting coverage.

---

### Decision 10 — Footer Totals

**Decision:** Yes — all three reports show footer totals.

| Report | Footer metrics |
| ------ | -------------- |
| M10 | Total Piutang, Total Customer |
| M11 | Total Inventory Value, Total Item |
| M12 | Grand Total Purchase, Total Invoice |

**Rationale:** Desktop reports provide summaries; management users expect quick totals. M10/M11 footers must reconcile with dashboard KPIs.

---

### Decision 11 — Purchasing Detail Reports

**Decision:** PF1 only for M12 V1.

**Deferred:** PF2 Invoice Detail, PF3 Daily Purchase Detail, PF4 Retur Beli.

**Focus:** Invoice summary, purchasing summary, receiving status.

---

### Decision 12 — M14 Influence on M10

**Decision:** Yes — design M10 to prepare for M14 Piutang Aging Dashboard.

**Implementation direction:** Display Jatuh Tempo prominently.

**Approved column order:** Customer, Sales, Faktur, Tanggal, Jatuh Tempo, Total Jual, Kurang Bayar.

---

## Appendix A — File Index

### M10 Piutang

| Category | Path |
| -------- | ---- |
| Screen | `btr.distrib/FinanceContext/PiutangSalesWilayahRpt/PiutangSalesWilayahForm.cs` |
| DAL contract | `btr.application/FinanceContext/PiutangAgg/Contracts/IPiutangSalesWilayahDal.cs` |
| DAL impl | `btr.infrastructure/FinanceContext/PiutangSalesWilayahRpt/PiutangSalesWilayahDal.cs` |
| Dashboard | `btr.infrastructure/ReportingContext/DashboardPiutangAgg/DashboardPiutangDal.cs` |

### M11 Inventory

| Category | Path |
| -------- | ---- |
| Screen | `btr.distrib/InventoryContext/StokBalanceRpt/StokBalanceInfo2Form.cs` |
| DAL contract | `btr.application/InventoryContext/StokBalanceInfo/IStokBalanceViewDal.cs` |
| DTO | `btr.application/InventoryContext/StokBalanceInfo/StokBalanceView.cs` |
| Dashboard | `btr.infrastructure/ReportingContext/DashboardInventoryAgg/DashboardInventoryDal.cs` |

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
