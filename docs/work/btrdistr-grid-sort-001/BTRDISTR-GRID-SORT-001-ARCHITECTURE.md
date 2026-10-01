---
Title: BTR Distrib — Enable click-to-sort on Faktur Control grid headers
Code: BTRDISTR-GRID-SORT-001
Artifact: ARCHITECTURE
Version: 1.0
LastUpdated: 2026-09-25
---

# 1. Overview

This architecture realizes the technical implementation for enabling interactive column-header sorting on the Faktur Control grid (`FakturControlForm`).

**Referenced FEATURE:** `docs/features/faktur/feature.md` (Faktur Control feature)
**Referenced FEASIBILITY-ASSESSMENT:** `docs/issues/BTRDISTR-GRID-SORT-001-FEASIBILITY-ASSESSMENT.md`

The approved approach follows the existing `AlokasiFpForm` pattern (Option B: manual `BindingList` reordering via `ColumnHeaderMouseClick`).

---

# 2. Architectural Basis

## Business Context

- **DOMAIN:** `docs/foundation/DOMAIN.md` — BTR Distrib Sales Context
- **FEATURE:** `docs/features/faktur/feature.md` — Faktur Control operational flow

The Faktur Control form provides a period-bounded grid view of faktur records with status toggling (Posted, Kirim, Kembali, Lunas, Pajak), search/filter, context menu actions (Edit, Create Klaim, Print), and reporting. Adding click-to-sort on column headers is a presentation-layer enhancement.

## Analysis Input

**FEASIBILITY-ASSESSMENT:** `BTRDISTR-GRID-SORT-001-FEASIBILITY-ASSESSMENT.md` — All gaps and open questions resolved with decisions:

| Decision ID | Summary |
|-------------|---------|
| GAP-001 | Register `ColumnHeaderMouseClick` event handler |
| GAP-002 | Configure column `SortMode` in `InitGrid()` |
| GAP-003 | Add visual sort direction glyphs on column headers |
| GAP-004 | Persist sort state across `RefreshGrid()` calls |
| GAP-005 | Row numbering regenerates automatically (no action needed) |
| GAP-006 | Checkbox columns (`Posted`, `Kirim`, `Kembali`, `Lunas`, `Pajak`, `IsHasKlaim`) set to `NotSortable`; all data columns sortable |
| OQ-001 | Sort state persists across refreshes (search, clear, period change) |
| OQ-002 | Single-column sort only (no Shift+Click multi-column) |
| OQ-003 | Sortable columns: `FakturDate`, `FakturCode`, `CustomerName`, `Npwp`, `SalesPersonName`, `GrandTotal`, `Bayar`, `PotBiayaLain`, `Sisa`, `UserId` |
| OQ-004 | No initial sort glyph on form load; glyph appears only after user click |
| OQ-005 | No existing `SortableBindingList`; use `AlokasiFpForm` manual reordering pattern |

---

# 3. Scope

## Included

- Add `ColumnHeaderMouseClick` event handler to `FakturGrid`
- Configure `SortMode` on all grid columns in `InitGrid()`:
  - Data columns: `SortMode = Automatic`
  - Checkbox/status columns: `SortMode = NotSortable`
- Implement sort logic: reorder `_listItem` using LINQ `OrderBy`/`OrderByDescending`, recreate `BindingList`, rebind grid
- Add form-level fields to persist sort state: `_sortColumn` (string), `_sortAsc` (bool)
- Re-apply sort state after `RefreshGrid()` rebind
- Set sort glyph (`ColumnHeaderCell.SortGlyphDirection`) on active column
- Initial default sort by `FakturDate` (from data query) — no glyph displayed

## Excluded

- Multi-column sort (Shift+Click)
- Server-side / database-level sorting
- SortableBindingList wrapper implementation
- Changes to `FakturControlView` model
- Changes to data access layer (`ListFakturControlWorker`)
- Changes to business logic or domain rules

---

# 4. Technical Decisions

| Decision | Realization |
|----------|-------------|
| Sort implementation pattern | Manual `BindingList` reordering (Option B), following `AlokasiFpForm` pattern |
| Sort event | `DataGridView.ColumnHeaderMouseClick` (single-click, not double-click) |
| Sort direction toggle | Single column tracked; click same column toggles direction; click different column sorts ascending |
| Glyph management | `ColumnHeaderCell.SortGlyphDirection` set to `Ascending`/`Descending`/`None` |
| Sort state persistence | Two form fields: `_sortColumn` (column Name), `_sortAsc` (direction); re-applied in `RefreshGrid()` after rebind |
| Checkbox columns | Explicitly set `SortMode = DataGridViewColumnSortMode.NotSortable` |
| Initial state | Data ordered by `FakturDate` from query; `_sortColumn = null`, `_sortAsc = true`; no glyph shown |

---

# 5. Component Responsibilities

| Component | Responsibility |
|-----------|----------------|
| `FakturControlForm` | Owns grid configuration, event registration, sort state persistence, sort execution, glyph management, and `RefreshGrid()` sort re-application |
| `FakturGrid` (`DataGridView`) | Displays data; raises `ColumnHeaderMouseClick`; renders sort glyphs |
| `_listItem` (`BindingList<FakturControlView>`) | In-memory data source; recreated on each sort operation with new ordering |

---

# 6. Integration Design

| Source | Target | Purpose |
|--------|--------|---------|
| `FakturControlForm.ColumnHeaderMouseClick` | `_listItem` | Reorder list via LINQ, recreate `BindingList`, rebind grid, update glyphs |
| `FakturControlForm.RefreshGrid` | `FakturGrid` | After data reload, re-apply persisted sort state and glyph |

---

# 7. Data Ownership

| Data | Owner |
|------|-------|
| `_listItem` (grid data) | `FakturControlForm` |
| `_sortColumn`, `_sortAsc` (sort state) | `FakturControlForm` |
| `FakturGrid` column `SortMode` | `FakturControlForm.InitGrid()` |
| Sort glyphs | `FakturControlForm` (via `ColumnHeaderCell.SortGlyphDirection`) |

No shared ownership. All sort-related state is private to `FakturControlForm`.

---

# 8. Database Design

No database changes required. Sorting is entirely in-memory on the `BindingList<FakturControlView>`.

---

# 9. Cross-Cutting Concerns

| Concern | Treatment |
|---------|-----------|
| **Performance** | In-memory LINQ sort on ≤ few thousand rows (period max 31 days); negligible impact |
| **UX Consistency** | Sort persists across search, filter, period change; glyph shows active column and direction |
| **Existing Behavior Preservation** | Conditional row formatting (`CellFormatting`), row header numbering, checkbox status toggling, context menu, printing — all unchanged |
| **Accessibility** | Standard `DataGridView` keyboard navigation unaffected; header click is mouse-driven enhancement |

---

# 10. Implementation Constraints

- **Framework:** Windows Forms (.NET Framework / .NET 6+ compatible)
- **Pattern:** Must follow existing `AlokasiFpForm` sort implementation pattern (manual `BindingList` reordering)
- **No new dependencies:** No external libraries, no `SortableBindingList` implementation
- **Column names:** Use `DataGridViewColumn.Name` property (matches `FakturControlView` property names)
- **Event registration:** Add handler in `RegisterEventHandler()`
- **Sort re-application:** In `RefreshGrid()`, after rebinding `_listItem` to grid, re-apply sort if `_sortColumn` is not null

---

# 11. Acceptance Conditions

1. Clicking a sortable column header sorts the grid by that column (ascending first click, toggles on subsequent clicks).
2. Sort glyph (▲/▼) appears on the active column header indicating direction.
3. Clicking a different sortable column sorts by the new column (ascending first).
4. Checkbox columns (`Posted`, `Kirim`, `Kembali`, `Lunas`, `Pajak`, `IsHasKlaim`) do not respond to header clicks (no sort, no glyph).
5. Sort state persists across: search (Enter/Click), clear search, period change, and any operation triggering `RefreshGrid()`.
6. Initial form load: data ordered by `FakturDate` (from query), no glyph visible.
7. All existing functionality preserved: conditional row formatting (Kembali=red, Lunas=yellow, IsHasKlaim=bold), row header numbering, checkbox status toggling, right-click context menu, print buttons.
8. No database queries added or modified for sorting.