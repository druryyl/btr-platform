---
Title: BTR Distrib — Enable click-to-sort on Faktur Control grid headers
Code: BTRDISTR-GRID-SORT-001
Artifact: FEASIBILITY-ASSESSMENT
Version: 1.0
LastUpdated: 2026-09-25
Status: READY-FOR-PLANNING
---

# 1. Request Summary

Feature being assessed: **Enable header-click sorting on the Faktur Control grid**

Referenced artifacts:

- DOMAIN
- FEATURE (faktur)

## Objective

Assess the feasibility of adding interactive column-header sorting to the existing Faktur Control grid (`FakturControlForm`) so users can reorder faktur data by clicking column headers, with visual sort direction indication, while preserving all existing functionality (search, filtering, status toggling, context menu, printing).

---

# 2. Current State

Summarize relevant findings from:
- Current Artifacts
- Current Codebase

This section contains facts only. Do not propose solutions.

## Existing Behavior

**FakturControlForm** (`src/j05-btr-distrib/btr.distrib/SalesContext/FakturControlAgg/FakturControlForm.cs`):

1. **Grid Configuration (`InitGrid()` - lines 503-574)**:
   - `FakturGrid` is a `DataGridView` bound to a `BindingSource` wrapping `_listItem` (`BindingList<FakturControlView>`)
   - Column configuration includes widths, header text, visibility, and cell formatting
   - Conditional row formatting based on `Kembali` (red text) and `Lunas` (yellow background) status
   - No sort handling registered on column header clicks

2. **Data Loading (`RefreshGrid()` - lines 576-637)**:
   - Data fetched via `_listFaktorControlWorker.Execute(periode)` returning `IEnumerable<FakturControlModel>`
   - Optional client-side filtering via `FilterFaktur()` using `SearchText.Text`
   - Data adapted to `FakturControlView` and assigned to new `BindingList<FakturControlView>`
   - Status flags (`Posted`, `Kirim`, `Kembali`, `Lunas`, `Pajak`) populated from separate DAL queries
   - Piutang data joined to calculate `GrandTotal`, `Bayar`, `PotBiayaLain`, `Sisa`
   - Grid rebound via new `BindingSource`; row header numbers reset

3. **Current Column Set (Visible)**:
   - `FakturDate` (formatted "ddd dd MMM yyyy", header "Tgl")
   - `FakturCode` (header "Code")
   - `CustomerName` (header "Customer")
   - `Npwp` (header "NPWP")
   - `SalesPersonName` (header "Sales")
   - `GrandTotal` (PaleTurquoise background)
   - `Bayar` (Pink background)
   - `PotBiayaLain` (Pink background, header "Pot/Biaya")
   - `Sisa` (PaleTurquoise background)
   - `Posted` (checkbox)
   - `Kembali` (checkbox)
   - `Lunas` (checkbox)
   - `Pajak` (checkbox)
   - `UserId` (header "Admin")
   - `IsHasKlaim` (header "Klaim")

4. **Hidden Columns**: `FakturId`, `IsHasKlaim`, `NoFakturPajak`, `UserId`

5. **Existing Event Handlers**:
   - `SearchText.KeyDown` (Enter → RefreshGrid)
   - `SearchButton.Click` → RefreshGrid
   - `ClearButton.Click` → Clear search + RefreshGrid
   - `FakturGrid.CellContentClick` → Checkbox status toggling (Posted, Kirim, Kembali, Lunas, Pajak)
   - `FakturGrid.MouseClick` (Right-click → Context menu with Edit, Create Klaim, Print options)
   - Print buttons (PrintFakturJual, PrintFakturKlaim)

6. **Default Ordering**: `FilterFaktur()` returns results ordered by `FakturDate` (line 660)

## Existing Constraints

**Technical**:
- Windows Forms `DataGridView` with `BindingSource` + `BindingList<T>` data source
- `BindingList<T>` supports sorting via `IBindingList` when `AllowNew`/`AllowEdit`/`AllowRemove` are properly configured, but default `BindingList<T>` does not implement sorting natively
- The form is instantiated via DI with multiple dependencies (workers, DALs, builders)
- Conditional row formatting (`CellFormatting` event) depends on row data binding
- Row header numbering is reset on every `RefreshGrid()`

**Operational**:
- Period limited to max 31 days (validated in `RefreshGrid()`)
- Search/filter runs client-side on in-memory data
- Status checkboxes trigger business operations (void, reactivate, kirim, kembali)
- Context menu actions (Edit, Create Klaim, Print) depend on current row selection

**Business**:
- No new business rules; sorting is presentation-layer only
- Default ordering by `FakturDate` should remain initial state
- Existing functionality must remain unaffected

---

# 3. Gap Analysis

Identify gaps between the requested FEATURE and the current system.

| ID | Severity | Gap |
|------|------|------|
| GAP-001 | CRITICAL | No column header click event handler registered for sorting. Current handlers only handle `CellContentClick` (checkboxes) and `MouseClick` (right-click context menu). |
| GAP-002 | CRITICAL | `DataGridView` sort mode not configured. Default `DataGridViewColumn.SortMode` is `Automatic` for bound columns, but `BindingList<T>` does not support sorting natively — the grid's built-in sorting will not work without a sortable wrapper or custom implementation. |
| GAP-003 | MAJOR | No visual sort direction indication (arrow glyphs) on column headers. Standard `DataGridView` shows sort glyphs only when `Sort()` is called programmatically or when using a sortable data source. |
| GAP-004 | MAJOR | `RefreshGrid()` recreates `BindingList` and rebinds grid, which would reset any user-applied sort. Sort state must be persisted across refreshes or re-applied after refresh. |
| GAP-005 | MINOR | Conditional row formatting (`CellFormatting` event) and row header numbering may need verification after sort operations to ensure visual consistency. |
| GAP-006 | MINOR | Need to determine which columns should be sortable. Status checkbox columns (`Posted`, `Kembali`, `Lunas`, `Pajak`) and `IsHasKlaim` may not be meaningful for sorting. Hidden columns should not be sortable. |

---

# 4. Open Questions

Identify unresolved questions that prevent confident architecture decisions.

| ID | Question | Impact |
|------|------|------|
| OQ-001 | Should sort state (column + direction) persist across `RefreshGrid()` calls (search, clear, period change)? | High — affects UX consistency; if not persisted, users lose sort on every search/refresh. |
| OQ-002 | Should sorting be single-column only or multi-column (Shift+Click)? | Medium — single-column is simpler; multi-column adds complexity. |
| OQ-003 | Which columns should be excluded from sorting? (e.g., checkbox columns, `IsHasKlaim`, `UserId`) | Medium — determines implementation scope. |
| OQ-004 | Should the initial default sort (by `FakturDate`) be indicated with a sort glyph on form load? | Low — visual consistency; standard DataGridView behavior doesn't show glyph until user clicks. |
| OQ-005 | Is there an existing `BindingList` sorting wrapper or helper in the codebase (e.g., `SortableBindingList`) that should be reused? | Medium — affects implementation approach; reusing existing patterns reduces risk. |

---

# 5. Assumptions

Document assumptions made during the assessment.

| ID | Assumption |
|------|------|
| ASM-001 | The `FakturControlView` class properties are all readable and suitable for sorting (no computed properties that would cause exceptions). |
| ASM-002 | Sorting will be performed in-memory on the `BindingList<FakturControlView>` — no database re-query required. |
| ASM-003 | The `DataGridView` columns are auto-generated from the `BindingList` properties; column `Name` matches property names (e.g., "FakturDate", "CustomerName"). |
| ASM-004 | The existing `CellFormatting` event and row header numbering will continue to work correctly after sorting (they operate on displayed rows via `RowIndex` and `DataBoundItem`). |
| ASM-005 | The `AlokasiFpForm` implementation (double-click header sort on `CustomerName`, `NoFakturPajak`, `Npwp`) is the reference pattern for this codebase. |

---

# 6. Risks

Document identified risks.

| ID | Risk | Impact | Mitigation |
|------|------|------|------|
| RISK-001 | `BindingList<T>` does not support sorting natively; using `DataGridView.Sort()` will throw or silently fail. | High — core functionality broken. | Use `BindingList` with `IBindingListView` support, wrap in `SortableBindingList`, or implement custom sort via `ColumnHeaderMouseClick` reordering the list. |
| RISK-002 | `RefreshGrid()` recreates the `BindingList`, losing sort state and glyphs. | High — UX regression. | Persist sort column/direction in form fields; re-apply after rebind in `RefreshGrid()`. |
| RISK-003 | Checkbox column sorting may confuse users (sorting boolean values has limited utility). | Low — UX confusion. | Disable sorting on checkbox columns (`Posted`, `Kirim`, `Kembali`, `Lunas`, `Pajak`, `IsHasKlaim`) via `SortMode = NotSortable`. |
| RISK-004 | Conditional row formatting (`CellFormatting`) relies on `DataBoundItem` cast to `FakturControlView`; sorting changes row order but not data binding, so should be safe. | Low — verify during testing. | Test row coloring (Kembali=red, Lunas=yellow) and bold font (IsHasKlaim) after sort. |
| RISK-005 | Row header numbering (1, 2, 3...) resets on `RefreshGrid()`; after sort, numbers reflect display order not data order. | Low — cosmetic. | Acceptable; row headers are display indices. |

---

# 7. Recommendations

Recommend possible approaches for addressing major gaps.

## Option A: Use DataGridView built-in sorting with SortableBindingList wrapper

**Description**: Wrap `BindingList<FakturControlView>` in a `SortableBindingList<T>` implementation (common pattern in WinForms) that supports `IBindingListView`/`IBindingList` sorting. Set `DataGridViewColumn.SortMode = Automatic` on sortable columns. Handle `ColumnHeaderMouseClick` to toggle direction and update glyphs.

### Advantages
- Leverages standard DataGridView sorting behavior (glyphs, multi-column with Shift+Click)
- Minimal custom code; reusable `SortableBindingList` pattern
- Sort state maintained by grid automatically

### Disadvantages
- Need to implement or locate existing `SortableBindingList<T>` in codebase
- Must handle `RefreshGrid()` rebind carefully to preserve sort state
- Checkbox columns need explicit `SortMode = NotSortable`

## Option B: Custom ColumnHeaderMouseClick handler with manual list reordering

**Description**: Register `ColumnHeaderMouseClick` event. On click, determine column and direction, reorder `_listItem` using LINQ `OrderBy`/`OrderByDescending`, recreate `BindingList`, rebind grid. Manually set sort glyph on column header. Persist sort column/direction in form fields.

### Advantages
- Full control over sort behavior
- No dependency on `SortableBindingList` implementation
- Can easily exclude specific columns
- Sort state persistence explicit and simple

### Disadvantages
- Manual glyph management (`ColumnHeaderCell.SortGlyphDirection`)
- No built-in multi-column sort support
- More code to maintain; diverges from standard DataGridView patterns
- Must handle `RefreshGrid()` re-application logic

## Option C: Hybrid — Use DataGridView.Sort() with custom IComparer on BindingList

**Description**: Keep `BindingList<FakturControlView>` but implement a custom `IComparer<FakturControlView>` and use `DataGridView.Sort(IComparer)` programmatically from `ColumnHeaderMouseClick`. Set `SortMode = Programmatic` on columns.

### Advantages
- Uses DataGridView's native sort glyph management
- Clean separation: grid handles UI, comparer handles logic
- Programmatic sort gives full control over comparison logic

### Disadvantages
- `BindingList` still doesn't maintain sort; grid sorts view only
- On `RefreshGrid()` rebind, sort must be re-applied
- More complex than Option B for marginal benefit

---

# 8. Gap Closure

Record resolutions for gaps and open questions.

## GAP-001

### Decision
Accepted. A `ColumnHeaderMouseClick` event handler will be registered on `FakturGrid` to handle user-initiated sorting.

### Rationale
The current `FakturControlForm` registers handlers for `CellContentClick` (checkbox status toggling) and `MouseClick` (right-click context menu) but has no handler for column header clicks. Adding `ColumnHeaderMouseClick` is the standard WinForms approach for intercepting header click events and implementing custom sort logic.

### Impact
- New event handler added to `RegisterEventHandler()` method
- Sort logic will execute on every column header click
- Must coordinate with GAP-002 (sort implementation) and GAP-004 (sort state persistence across RefreshGrid)

### Architecture Impact
- Presentation-layer change only; no new dependencies or services required
- Sort logic will be contained within `FakturControlForm`
- Architect will define the specific sort implementation pattern (Option A, B, or C from recommendations)

### Resolved By
Analyst

### Resolved Date
2026-09-25

---

## GAP-002

### Decision
Accepted. Sort Mode will be configured in implementation.

### Rationale
The DataGridView column SortMode property will be set appropriately during implementation to enable header-click sorting behavior.

### Impact
- Column SortMode configuration in InitGrid()
- Checkbox columns set to NotSortable
- Data columns set to Automatic or Programmatic as needed

### Architecture Impact
- Architect will specify exact SortMode values in ARCHITECTURE artifact
- Implementation detail for the presentation layer

### Resolved By
Analyst

### Resolved Date
2026-09-25

---

## GAP-003

### Decision
The sort direction indication will be added in implementation.

### Rationale
Visual sort direction (arrow glyphs) will be implemented on column headers to show current sort state.

### Impact
- Glyph display on clicked column headers
- Ascending/descending indication

### Architecture Impact
- Implementation detail for presentation layer

### Resolved By
Analyst

### Resolved Date
2026-09-25

---

## GAP-004

### Decision
Sort state will be persisted across refreshes.

### Rationale
The current sort column and direction will be stored in form-level fields and re-applied after each RefreshGrid() call to maintain user's sort preference.

### Impact
- Sort survives search, filter, period change operations
- Two form fields to track sort state

### Architecture Impact
- Architect will specify persistence and re-application logic

### Resolved By
Analyst

### Resolved Date
2026-09-25

---

## GAP-005

### Decision
Row numbering is just a serial number to represent row, not an ID. The number may re-populated after sorting.

### Rationale
The row header numbers are display indices (1, 2, 3...) that reflect the current display order. After sorting, they will be regenerated to match the new display order, which is the expected behavior.

### Impact
- No code changes needed
- Row numbers refresh automatically in RefreshGrid()

### Architecture Impact
- None; existing behavior is correct

### Resolved By
Analyst

### Resolved Date
2026-09-25

---

## GAP-006

### Decision
All columns can be sorted, unless the column's data type is checkbox.

### Rationale
Checkbox columns (Posted, Kirim, Kembali, Lunas, Pajak, IsHasKlaim) are status indicators with limited analytical value for sorting. All other data columns (dates, codes, names, numbers, amounts) are sortable.

### Impact
- Checkbox columns: SortMode = NotSortable
- Data columns: SortMode = Automatic (or Programmatic)
- 10 sortable columns, 6 non-sortable visible columns

### Architecture Impact
- Architect will specify exact column configuration

### Resolved By
Analyst

### Resolved Date
2026-09-25

---

## OQ-001

### Decision
Yes. Sort state (column + direction) will persist across `RefreshGrid()` calls including search, clear, and period change.

### Rationale
Users expect their sort preference to remain when they refine results via search or adjust the date period. Losing sort on every interaction would be a significant UX regression. The implementation (GAP-004 decision) persists sort state in form fields and re-applies after each `RefreshGrid()`.

### Impact
- Sort survives search/filter operations
- Sort survives period changes
- Sort survives status toggle operations (if they trigger RefreshGrid)
- Only explicit user action (clicking a different column header) changes the sort

### Resolved By
Analyst

### Resolved Date
2026-09-25

---

## OQ-002

### Decision
Single-column sort only. Multi-column sort (Shift+Click) is not required.

### Rationale
- The issue requests "click-to-sort on column headers" — single column is the standard interpretation
- The `AlokasiFpForm` reference implementation only supports single-column sort
- Multi-column sort adds significant complexity (glyph management, sort order tracking) for marginal benefit
- Users can achieve multi-column effect by sorting least-significant column first, then most-significant (stable sort)

### Impact
- Simpler implementation: only track one sort column + direction
- No Shift+Click detection needed
- Glyph management simplified (only one column shows glyph at a time)

### Resolved By
Analyst

### Resolved Date
2026-09-25

---

## OQ-003

### Decision
Resolved via GAP-006 decision. Sortable columns: `FakturDate`, `FakturCode`, `CustomerName`, `Npwp`, `SalesPersonName`, `GrandTotal`, `Bayar`, `PotBiayaLain`, `Sisa`, `UserId`. Non-sortable: `Posted`, `Kirim`, `Kembali`, `Lunas`, `Pajak`, `IsHasKlaim`. Hidden columns remain hidden.

### Rationale
See GAP-006 decision for full rationale. Checkbox/status columns excluded; all data columns included.

### Impact
- 10 sortable columns, 6 non-sortable visible columns
- Column list codified in `InitGrid()` via `SortMode` property

### Resolved By
Analyst

### Resolved Date
2026-09-25

---

## OQ-004

### Decision
No. The initial default sort (by `FakturDate` from data query) will NOT show a sort glyph on form load. Glyph appears only after user clicks a column header.

### Rationale
- Standard `DataGridView` behavior: no glyph until user initiates sort
- The default `FakturDate` ordering comes from the data query (`FilterFaktur` returns `OrderBy(x => x.FakturDate)`), not from a grid sort operation
- Showing a glyph without user action would imply a sort was applied when it wasn't
- Consistent with `AlokasiFpForm` reference (no initial glyph)

### Impact
- Form load: data ordered by FakturDate, no glyph visible
- First user click on any column: glyph appears, sort applied
- If user clicks `FakturDate` header: glyph appears, sort toggles (descending first click since data is already ascending)

### Resolved By
Analyst

### Resolved Date
2026-09-25

---

## OQ-005

### Decision
No existing `SortableBindingList<T>` or similar helper found in the codebase. The `AlokasiFpForm` uses manual `BindingList` reordering (Option B pattern), which will be the approach for this feature.

### Rationale
- Codebase search revealed no `SortableBindingList`, `IBindingListView`, or sorting wrapper implementations
- The `AlokasiFpForm` (lines 669-697) is the only sorting implementation found, and it uses manual `OrderBy`/`OrderByDescending` on the list followed by grid refresh
- This confirms Option B (custom manual reordering) is the established pattern in this codebase

### Impact
- No reusable component to leverage; implementation will follow `AlokasiFpForm` pattern
- Architect will formalize this pattern in ARCHITECTURE artifact
- Consistent with existing codebase conventions

### Resolved By
Analyst

### Resolved Date
2026-09-25

---

# 9. Planning Readiness

## Readiness Checklist

- [x] All critical gaps resolved
- [x] All required decisions recorded
- [x] All blocking open questions resolved
- [x] Architecture can be finalized or updated

## Status

READY-FOR-PLANNING

## Notes

All critical gaps (GAP-001, GAP-002, GAP-003, GAP-004) and blocking open questions (OQ-001, OQ-003, OQ-005) have been resolved with recorded decisions. Minor gaps (GAP-005, GAP-006) and remaining open questions (OQ-002, OQ-004) have also been resolved. The Architecture skill can now finalize the target architecture based on these approved decisions. The approved approach follows the existing `AlokasiFpForm` pattern (Option B: manual BindingList reordering via ColumnHeaderMouseClick).

---

# 10. References

Referenced artifacts:

- DOMAIN: `docs/foundation/DOMAIN.md`
- FEATURE: `docs/features/faktur/feature.md`

Referenced codebase locations:

- `src/j05-btr-distrib/btr.distrib/SalesContext/FakturControlAgg/FakturControlForm.cs`
- `src/j05-btr-distrib/btr.distrib/SalesContext/FakturControlAgg/FakturControlView.cs`
- `src/j05-btr-distrib/btr.distrib/SalesContext/AlokasiFpAgg/AlokasiFpForm.cs` (reference implementation for header double-click sort)
- `src/j05-btr-distrib/btr.application/SalesContext/FakturControlAgg/ListFakturControlWorker.cs`

Referenced documents:

- `docs/issues/BTRDISTR-GRID-SORT-001.md`
- `docs/foundation/PRODUCT.md`
- `docs/foundation/LANDSCAPE.md`
- `docs/foundation/WORKFLOW.md`