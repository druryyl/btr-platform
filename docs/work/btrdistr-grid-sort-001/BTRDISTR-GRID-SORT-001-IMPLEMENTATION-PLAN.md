---
Title: BTR Distrib — Enable click-to-sort on Faktur Control grid headers
Code: BTRDISTR-GRID-SORT-001
Artifact: IMPLEMENTATION-PLAN
Version: 1.0
LastUpdated: 2026-09-25
Status: NOT-STARTED
Execution Approval: APPROVED
---

# 1. Objective

Implement interactive column-header sorting on the Faktur Control grid (`FakturControlForm`) so users can reorder faktur data by clicking column headers, with visual sort direction indication, while preserving all existing functionality (search, filtering, status toggling, context menu, printing).

**Referenced artifacts:**

- FEATURE: `docs/features/faktur/feature.md`
- ARCHITECTURE: `docs/work/btrdistr-grid-sort-001/BTRDISTR-GRID-SORT-001-ARCHITECTURE.md`
- FEASIBILITY-ASSESSMENT: `docs/issues/BTRDISTR-GRID-SORT-001-FEASIBILITY-ASSESSMENT.md`

---

# 2. Planning Scope

This plan covers the presentation-layer implementation of click-to-sort on `FakturControlForm` in the `btr.distrib` project. The implementation follows the established `AlokasiFpForm` pattern: manual `BindingList` reordering via `ColumnHeaderMouseClick` event, with sort state persisted across `RefreshGrid()` calls.

**Target repository:** `src/j05-btr-distrib/btr.distrib` (single repository — all changes in `FakturControlForm.cs`)

**Scope boundaries:**

- In: `FakturControlForm.cs` — grid init, event registration, sort logic, glyph management, sort persistence
- Out: No changes to DAL, workers, builders, view models, database, or other forms

---

# 3. Dependencies

No external dependencies. All work is within `FakturControlForm.cs` in the `btr.distrib` project.

For slice dependencies:

- `Depends On` declares implementation prerequisites
- Dependencies reference Slice IDs only
- Dependency satisfaction requires the referenced slice to have implementation status IMPLEMENTED
- Dependency satisfaction does not require review status GO
- Dependencies must represent real implementation prerequisites

---

# 4. Progress Summary

Plan status values are:

- NOT-STARTED
- IN-PROGRESS
- BLOCKED
- COMPLETED

Execution Approval values are:

- PENDING
- APPROVED

Execution Approval is owned by the Architect. It is PENDING during Planning and set to APPROVED when the plan is released for execution. Execution must not begin while Execution Approval is PENDING.

COMPLETED is a plan-level status only. Set it only when every slice has implementation status IMPLEMENTED and review status GO.

Testing and test-package creation must not begin until the plan is COMPLETED. An individual slice with review status GO is not a testing entry condition.

Slice implementation status values are:

- NOT-STARTED
- IN-PROGRESS
- IMPLEMENTED
- BLOCKED

Slice review status values are:

- NOT-REVIEWED
- GO
- NO-GO

| Phase | Implementation Status | Review Status | Progress |
|-------|----------------------|---------------|----------|
| P1 | IN-PROGRESS | NOT-REVIEWED | 5/5 IMPLEMENTED |

---

# 5. Phases

## P1 - Faktur Control Grid Sort Implementation

        Implementation Status: IN-PROGRESS
        Review Status: NOT-REVIEWED

        ### P1-S01

**Title:** Add sort state fields to `FakturControlForm`

**Implementation Status:** IMPLEMENTED
**Review Status:** NOT-REVIEWED

**Objective:**
Add private form-level fields to persist the current sort column and direction across `RefreshGrid()` calls.

**Depends On:** None

**Repository:** `src/j05-btr-distrib/btr.distrib`

**Completion Criteria:**
- Field `_sortColumn` (string, nullable) added — stores the `DataGridViewColumn.Name` of the currently sorted column
- Field `_sortAsc` (bool) added — stores sort direction (true = ascending, false = descending)
- Fields initialized to `null` and `true` respectively in constructor or field initializer
- No other code changes in this slice

**Notes:**
These fields enable OQ-001 decision (sort persists across refreshes). The initial state (`_sortColumn = null`) ensures no glyph shows on form load (OQ-004 decision).

---

### P1-S02

        **Title:** Configure column `SortMode` in `InitGrid()`

        **Implementation Status:** IMPLEMENTED
        **Review Status:** NOT-REVIEWED

**Objective:**
Set `DataGridViewColumn.SortMode` on all columns per the architecture: data columns = `Automatic`, checkbox/status columns = `NotSortable`.

**Depends On:** None

**Repository:** `src/j05-btr-distrib/btr.distrib`

**Completion Criteria:**
- In `InitGrid()`, after column configuration, set `SortMode` for each column:
  - Sortable columns (`FakturDate`, `FakturCode`, `CustomerName`, `Npwp`, `SalesPersonName`, `GrandTotal`, `Bayar`, `PotBiayaLain`, `Sisa`, `UserId`): `SortMode = DataGridViewColumnSortMode.Automatic`
  - Non-sortable columns (`Posted`, `Kirim`, `Kembali`, `Lunas`, `Pajak`, `IsHasKlaim`): `SortMode = DataGridViewColumnSortMode.NotSortable`
  - Hidden columns (`FakturId`, `NoFakturPajak`) remain hidden; `SortMode` not critical but set to `NotSortable` for consistency
- No sort logic or event handlers added in this slice

**Notes:**
Implements GAP-002 and GAP-006 decisions. The `Automatic` mode allows the grid to show glyphs when we programmatically set them; `NotSortable` prevents header click interaction on checkbox columns.

---

### P1-S03

**Title:** Register `ColumnHeaderMouseClick` event handler

**Implementation Status:** NOT-STARTED
**Review Status:** NOT-REVIEWED

**Objective:**
Register the `ColumnHeaderMouseClick` event on `FakturGrid` in `RegisterEventHandler()` and create the handler method stub.

**Depends On:** P1-S01, P1-S02

**Repository:** `src/j05-btr-distrib/btr.distrib`

**Completion Criteria:**
- In `RegisterEventHandler()`, add: `FakturGrid.ColumnHeaderMouseClick += FakturGrid_ColumnHeaderMouseClick;`
- Create private method `FakturGrid_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)` with empty body (logic in next slice)
- Handler signature matches `DataGridViewColumnHeaderMouseClickEventHandler`
- No sort logic implemented yet — just registration and stub

**Notes:**
Implements GAP-001 decision. Depends on P1-S01 (fields exist) and P1-S02 (SortMode configured so non-sortable columns won't trigger meaningful sorts).

---

### P1-S04

**Title:** Implement sort logic in `ColumnHeaderMouseClick` handler

**Implementation Status:** NOT-STARTED
**Review Status:** NOT-REVIEWED

**Objective:**
Implement the core sort logic: reorder `_listItem` via LINQ, recreate `BindingList`, rebind grid, update sort glyphs, and persist sort state.

**Depends On:** P1-S03

**Repository:** `src/j05-btr-distrib/btr.distrib`

**Completion Criteria:**
In `FakturGrid_ColumnHeaderMouseClick`:
1. Get clicked column name: `var colName = FakturGrid.Columns[e.ColumnIndex].Name;`
2. If column is non-sortable (`SortMode == NotSortable`), return early
3. Determine sort direction:
   - If `colName == _sortColumn`: toggle `_sortAsc = !_sortAsc`
   - Else: `_sortColumn = colName; _sortAsc = true` (new column sorts ascending first)
4. Reorder `_listItem`:
   - Use LINQ `OrderBy` / `OrderByDescending` on the property matching `colName`
   - Create new `BindingList<FakturControlView>(orderedList.ToList())`
   - Assign to `_listItem`
5. Rebind grid:
   - Create new `BindingSource { DataSource = _listItem }`
   - `FakturGrid.DataSource = binding`
   - `FakturGrid.Refresh()`
6. Update row header numbering (existing pattern from `RefreshGrid()`):
   - `foreach (DataGridViewRow row in FakturGrid.Rows) row.HeaderCell.Value = $"{(row.Index + 1):N0}";`
7. Update sort glyphs:
   - Clear all glyphs: `foreach (DataGridViewColumn c in FakturGrid.Columns) c.HeaderCell.SortGlyphDirection = SortOrder.None;`
   - Set glyph on active column: `FakturGrid.Columns[_sortColumn].HeaderCell.SortGlyphDirection = _sortAsc ? SortOrder.Ascending : SortOrder.Descending;`
8. Handle edge cases: `e.ColumnIndex < 0` guard, null checks

**Notes:**
Follows `AlokasiFpForm` pattern (lines 669-697 in `AlokasiFpForm.cs`) but uses `ColumnHeaderMouseClick` (single-click) instead of `ColumnHeaderMouseDoubleClick`, and handles all sortable columns dynamically via reflection/property name matching rather than hardcoded switch.

Implements GAP-001, GAP-002, GAP-003, GAP-004, OQ-001, OQ-002, OQ-003 decisions.

---

### P1-S05

**Title:** Re-apply sort state in `RefreshGrid()` after rebind

**Implementation Status:** NOT-STARTED
**Review Status:** NOT-REVIEWED

**Objective:**
Modify `RefreshGrid()` to re-apply the persisted sort state (`_sortColumn`, `_sortAsc`) after the grid is rebound with fresh data.

**Depends On:** P1-S04

**Repository:** `src/j05-btr-distrib/btr.distrib`

**Completion Criteria:**
- In `RefreshGrid()`, after the existing rebind code (after `FakturGrid.Refresh()` and row header numbering), add:
  ```csharp
  if (_sortColumn != null && FakturGrid.Columns.Contains(_sortColumn))
  {
      // Re-sort the list using current sort state
      var ordered = _sortAsc ?
          _listItem.OrderBy(x => GetPropertyValue(x, _sortColumn)).ToList() :
          _listItem.OrderByDescending(x => GetPropertyValue(x, _sortColumn)).ToList();
      _listItem = new BindingList<FakturControlView>(ordered);
      
      // Rebind
      var binding = new BindingSource { DataSource = _listItem };
      FakturGrid.DataSource = binding;
      FakturGrid.Refresh();
      
      // Renumber row headers
      foreach (DataGridViewRow row in FakturGrid.Rows)
          row.HeaderCell.Value = $"{(row.Index + 1):N0}";
      
      // Restore glyph
      foreach (DataGridViewColumn c in FakturGrid.Columns)
          c.HeaderCell.SortGlyphDirection = SortOrder.None;
      FakturGrid.Columns[_sortColumn].HeaderCell.SortGlyphDirection = _sortAsc ? SortOrder.Ascending : SortOrder.Descending;
  }
  ```
- Add helper method `GetPropertyValue(FakturControlView item, string propertyName)` using reflection or a switch expression to get the property value for sorting
- Ensure the re-sort logic uses the same property access pattern as the click handler for consistency

**Notes:**
Implements GAP-004 and OQ-001 decisions (sort persists across refreshes). The helper method avoids code duplication between click handler and refresh re-application.

---

# 6. Change Log

- v1.0 (2026-09-25): Initial plan created from approved FEASIBILITY-ASSESSMENT and ARCHITECTURE.
- All gaps and open questions resolved; plan follows established `AlokasiFpForm` pattern.
- Execution Approval: APPROVED — released for execution.
- All 5 slices (P1-S01 through P1-S05) implemented; build verified (0 errors, 0 warnings). Awaiting Reviewer GO.
