---
Title: Enable Interactive Column-Header Sorting on Faktur Control Grid
Code: BTRDISTR-FC-SORT-001
Artifact: IMPLEMENTATION-PLAN
Version: 1.0
LastUpdated: 2026-10-09
Status: COMPLETED
Execution Approval: APPROVED
---

# 1. Objective

Enable users to sort the grid in `FakturControlForm` by clicking column headers, refactoring the previous manual LINQ reordering logic to use `SortableBindingList<T>` (the standard pattern in `btr.distrib`), maintaining sort state across refreshes/searches, preserving row selection, and ensuring clean row numbering via `RowPostPaint`.

Referenced artifacts:
- FEATURE: `docs/features/faktur/feature.md`
- DOMAIN: `docs/foundation/DOMAIN.md` (BTR Distrib Sales Context)
- HELPER: `src/j05-btr-distrib/btr.distrib/Helpers/SortableBindingListHelper.cs`

---

# 2. Planning Scope

This implementation plan focuses on `FakturControlForm.cs` in `src/j05-btr-distrib/btr.distrib/SalesContext/FakturControlAgg/`:
- Refactoring `_listItem` from `BindingList<FakturControlView>` to `SortableBindingList<FakturControlView>`.
- Setting `SortMode = DataGridViewColumnSortMode.Automatic` on data columns and `SortMode = DataGridViewColumnSortMode.NotSortable` on checkbox/metadata columns.
- Removing manual reordering boilerplate (`FakturGrid_ColumnHeaderMouseClick`, `GetSortValue`) in favor of `SortableBindingList<T>` native sorting.
- Setting up initial sort order (`FakturDate` Ascending) and displaying the sort glyph on the `Tgl` column.
- Persisting active sort column and sort direction across keyword searches and period refreshes.
- Preserving the active row selection (`FakturId`) and scrolling it into view after sort/refresh.
- Replacing manual row header number loops with `FakturGrid.RowPostPaint += DataGridViewExtensions.DataGridView_RowPostPaint`.
- Ensuring Excel export routines (`PrintAll_OnClick`, `PrintKembali_OnClick`, `PrintBelumKembali_OnClick`) preserve the current sorted list order.

---

# 3. Dependencies

- External dependencies: None.
- Internal dependencies:
  - Phase 1 (Core Sort Refactoring & Persistence) must be implemented and verified before Phase 2 (Validation & Build Verification).

---

# 4. Progress Summary

| Phase | Description | Implementation Status | Review Status | Progress |
| ----- | ----------- | --------------------- | ------------- | -------- |
| P1 | Refactor Grid Sorting & Selection Persistence | IMPLEMENTED | GO | 2/2 |
| P2 | Build Verification & Integration Testing | IMPLEMENTED | GO | 1/1 |

---

# 5. Phases and Slices

## Phase 1 — Refactor Grid Sorting & Selection Persistence

### Slice P1-S01: Refactor to `SortableBindingList<T>` & Clean Up Manual Handlers

- **Status:** IMPLEMENTED
- **Review Status:** GO
- **Depends On:** None
- **Affected Files:**
  - `src/j05-btr-distrib/btr.distrib/SalesContext/FakturControlAgg/FakturControlForm.cs`

#### Implementation Steps:
1. Change `_listItem` declaration from `BindingList<FakturControlView>` to `SortableBindingList<FakturControlView>`.
2. Remove manual event handler `FakturGrid_ColumnHeaderMouseClick` and helper `GetSortValue(FakturControlView item, string propertyName)`.
3. In `InitGrid()`:
   - Configure data columns (`FakturDate`, `FakturCode`, `CustomerName`, `Npwp`, `SalesPersonName`, `GrandTotal`, `Bayar`, `PotBiayaLain`, `Sisa`, `UserId`) with `SortMode = DataGridViewColumnSortMode.Automatic`.
   - Configure checkbox columns (`Posted`, `Kirim`, `Kembali`, `Lunas`, `Pajak`, `IsHasKlaim`) and hidden columns (`FakturId`, `NoFakturPajak`) with `SortMode = DataGridViewColumnSortMode.NotSortable`.
   - Wire `FakturGrid.RowPostPaint += DataGridViewExtensions.DataGridView_RowPostPaint;` and remove manual `row.HeaderCell.Value` assignment loops.
4. Update `FakturGrid_CellContentClick` to safely access `(FakturControlView)grid.Rows[e.RowIndex].DataBoundItem` rather than index-accessing `_listItem[e.RowIndex]`.

---

### Slice P1-S02: Sort Persistence, Initial Sort & Selection Retention

- **Status:** IMPLEMENTED
- **Review Status:** GO
- **Depends On:** P1-S01
- **Affected Files:**
  - `src/j05-btr-distrib/btr.distrib/SalesContext/FakturControlAgg/FakturControlForm.cs`

#### Implementation Steps:
1. Track active sort state (`_sortColumn = "FakturDate"`, `_sortDirection = ListSortDirection.Ascending`).
2. Hook `FakturGrid.Sorted` event:
   - When a user clicks a column header and sorting completes, update `_sortColumn` and `_sortDirection` from `FakturGrid.SortedColumn` and `FakturGrid.SortOrder`.
3. In `RefreshGrid()`:
   - Capture the current row's `FakturId` before updating data (`var selectedFakturId = FakturGrid.CurrentRow?.Cells["FakturId"]?.Value?.ToString();`).
   - Populate `_listItem = new SortableBindingList<FakturControlView>(listTemp);`.
   - Apply active sort:
     - If `_sortColumn` is valid, call `FakturGrid.Sort(FakturGrid.Columns[_sortColumn], _sortDirection)`.
     - Otherwise default to sorting by `FakturDate` Ascending.
   - Restore row selection:
     - Search rows for matching `selectedFakturId`.
     - If found, set `FakturGrid.CurrentCell` and ensure the row is displayed in view.
4. Verify that Excel exports (`PrintFakturControl`) continue receiving `_listItem` in its sorted order.

---

## Phase 2 — Build Verification & Integration Testing

### Slice P2-S01: Compile & Verification

- **Status:** IMPLEMENTED
- **Review Status:** GO
- **Depends On:** P1-S02
- **Affected Files:**
  - `src/j05-btr-distrib/btr.distrib/SalesContext/FakturControlAgg/FakturControlForm.cs`

#### Implementation Steps:
1. Build `j05-btr-distrib` project via `dotnet build` or `msbuild` to ensure zero compilation warnings or errors.
2. Verify all sorting scenarios:
   - Initial load shows `FakturDate` Ascending with sort glyph.
   - Clicking headers toggles Ascending / Descending.
   - Checkbox clicking (Posted, Kembali, etc.) triggers operations on the correct row even when sorted.
   - Filter / search preserves the active sort column and direction.
   - Excel export maintains active sort order.
