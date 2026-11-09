# ISSUE

## Metadata

ID: `BTRDISTR-GRID-SORT-001`  
Type: `CHANGE-REQUEST`  
Status: `New`  
Title: **BTR Distrib — Enable click-to-sort on Faktur Control grid headers**

## Source

Reported By: Repository owner (direct request)  
Reported Date: 2026-09-25

## Description

The Faktur Control form (`src/j05-btr-distrib/btr.distrib/SalesContext/FakturControlAgg/FakturControlForm.cs`) displays a list of faktur (sales invoices) in a grid. Currently the grid does not allow users to reorder the data by clicking column headers. The request is to enable header-click sorting so that users can quickly scan and analyze faktur data by different columns without leaving the form.

The grid is populated from a `BindingList<FakturControlView>` and supports search, filtering, and status toggling. The desired enhancement is to add interactive column-header sorting to this existing grid.

## Desired Outcome

- Clicking a column header in the Faktur Control grid reorders the rows based on that column's data.
- The current sort direction (ascending/descending) is visually indicated on the header.
- Existing functionality (search, filtering, status toggling, context menu, printing) remains unaffected.
- The grid continues to use the existing `BindingList<FakturControlView>` data source.

## Current Situation

The `FakturControlForm` contains a `FakturGrid` (DataGridView) bound to a `BindingSource` whose `DataSource` is a `BindingList<FakturControlView>` named `_listItem`.

In `InitGrid()`, the grid is configured with column widths, headers, visibility, and conditional formatting (row colors based on `Kembali` and `Lunas` status). No sort handling is registered on column header clicks.

In `RefreshGrid()`, data is fetched via `_listFaktorControlWorker.Execute(periode)`, optionally filtered by `SearchText.Text`, adapted to `FakturControlView`, and assigned to a new `BindingList<FakturControlView>`. The grid is rebound and row header numbers are reset.

Visible columns include: `FakturDate`, `FakturCode`, `CustomerName`, `Npwp`, `SalesPersonName`, `GrandTotal`, `Bayar`, `PotBiayaLain`, `Sisa`, and status checkboxes (`Posted`, `Kirim`, `Kembali`, `Lunas`, `Pajak`). Hidden columns include `FakturId`, `IsHasKlaim`, `NoFakturPajak`, `UserId`.

## Evidence

- `src/j05-btr-distrib/btr.distrib/SalesContext/FakturControlAgg/FakturControlForm.cs` — source file containing `InitGrid()`, `RefreshGrid()`, and the grid binding logic.
- `FakturGrid` is a `DataGridView` with `DataSource` set to a `BindingSource` wrapping `_listItem`.

## Notes

1. **Scope.** This change request is limited to enabling header-click sorting on the existing grid. No changes to data retrieval, filtering logic, status management, or printing are implied.
2. **Data source.** The grid uses `BindingList<FakturControlView>`, which supports sorting via `Sort` descriptors when the grid's `Sort` method is invoked. The implementation should leverage this rather than re-fetching or re-sorting raw data.
3. **Visual indication.** The request implies that the sort direction should be shown on the clicked header (e.g., arrow glyph), consistent with standardDataGridView behavior.
4. **Existing behavior preservation.** The current default ordering (by `FakturDate` as seen in `FilterFaktur`) should be the initial state before any user-initiated sort.
5. **No new business rules.** Sorting is a presentation-layer enhancement; it does not alter any business logic, domain rules, or data integrity constraints.