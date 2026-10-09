# Faktur and Faktur Control Feature

## Purpose

The Faktur feature handles the sales invoicing process in BTR Distrib, including inventory reduction, accounts receivable creation, and post-invoicing administrative control (Faktur Control).

The Faktur Control sub-feature provides an administrative control center where staff can monitor faktur lifecycle milestones (Printed/Posted, Dispatched/Kirim, Returned/Kembali, Settled/Lunas, Tax Invoiced/Pajak, Claims/Klaim), filter by date period and search keywords, reorder grid records interactively, and export data to Excel.

---

# Business Rules

## Core Faktur Rules

- Faktur creation immediately reduces inventory.
- Customer arrears generate warning only.
- Admin may continue Faktur creation.
- One Faktur belongs to exactly one Warehouse.

---

## Faktur Control Grid & Sorting Rules

### BR-FC-001: Sortable Columns
Users can sort the Faktur Control grid by clicking the column header of any data column:
- `FakturDate` (Tgl)
- `FakturCode` (Code)
- `CustomerName` (Customer)
- `Npwp` (NPWP)
- `SalesPersonName` (Sales)
- `UserId` (Admin)
- `GrandTotal` (Total Nilai Penjualan)
- `Bayar` (Terbayar)
- `PotBiayaLain` (Potongan / Biaya Lain)
- `Sisa` (Sisa Piutang)

### BR-FC-002: Non-Sortable Columns
Checkbox/status and metadata columns are non-sortable:
- `Posted`
- `Kirim`
- `Kembali`
- `Lunas`
- `Pajak`
- `IsHasKlaim` (Klaim)
- `FakturId` (Hidden)
- `NoFakturPajak` (Hidden)

### BR-FC-003: Initial Sort Order
When the form is first opened or refreshed for a period, the grid defaults to sorting by `FakturDate` in **Ascending** order, with an ascending visual sort glyph displayed on the header of the `Tgl` column.

### BR-FC-004: Two-State Toggle
Clicking the header of an active sort column toggles the sort direction between **Ascending** and **Descending**. Clicking a different sortable column sorts that column in **Ascending** order.

### BR-FC-005: Sort State Persistence
The active sort column and sort direction persist across user searches (keyword query) and data refreshes (date period adjustments).

### BR-FC-006: Row Selection Preservation
When grid data is sorted or refreshed, the active selection is preserved based on `FakturId` so that the user's selected row remains selected and scrolled into view.

### BR-FC-007: Row Numbering
Row header numbers (1, 2, 3...) reflect visual row positions and stay correctly indexed regardless of sort order or scrolling.

### BR-FC-008: Export Fidelity
Excel exports (Print All, Print Kembali, Print Belum Kembali) export data in the current visual sort order of the grid.