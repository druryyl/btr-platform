---
Title: Sliced Query Loading and Grid Scalability for Faktur Per Customer Report
Code: BTRDISTR-FPC-MASSLOAD-001
Artifact: IMPLEMENTATION-PLAN
Version: 1.0
LastUpdated: 2026-10-09
Status: COMPLETED
Execution Approval: APPROVED
---

# 1. Objective

Enable users to reliably query large date ranges (3 to 6 months) in `FakturPerCustomerForm` without database query timeouts or UI thread freezing, while addressing the memory and performance limitations of `Syncfusion.Windows.Forms.Grid.Grouping.GridGroupingControl` when handling massive datasets (100,000+ line items).

Referenced artifacts:
- FEATURE: `docs/features/faktur/feature.md`
- DOMAIN: `docs/foundation/DOMAIN.md`
- FORM: `src/j05-btr-distrib/btr.distrib/SalesContext/FakturPerCustomerRpt/FakturPerCustomerForm.cs`
- DAL: `src/j05-btr-distrib/btr.infrastructure/SalesContext/FakturPerCustomerRpt/FakturPerCustomerDal.cs`

---

# 2. Problem Analysis & Feasibility Evaluation

### 2.1 Why Did the Previous Query Time Out?
1. **Inefficient Join Structure:** `FakturPerCustomerDal` started with `BTR_FakturItem aa` (a table with millions of historical rows) and left-joined `BTR_Faktur bb` where the date filter `bb.FakturDate BETWEEN @Tgl1 AND @Tgl2` was placed. This prevented optimal index seeking on `FakturDate`.
2. **Quadruple Subquery Scans:** It performed four separate subqueries on `BTR_FakturDiscount` (`WHERE NoUrut = 1`, `NoUrut = 2`, etc.), resulting in 4 table/index scans over discount records.
3. **Default Timeout:** Dapper defaulted to a 30-second timeout.
4. **Duplicate Execution:** In `FakturPerCustomerForm.InitGrid()`, `Proses()` was called twice on form initialization (line 43 and line 128).

### 2.2 Can Syncfusion GridGroupingControl Hold 6 Months of Data?
**Technical Answer: Highly problematic / High Risk of `OutOfMemoryException` (OOM).**
1. **In-Memory Tree & Grouping Model:** `GridGroupingControl` creates hierarchical wrapper objects (`Record`, `RecordRow`, grouping nodes, summary aggregates) for every row in memory. Memory overhead is roughly 1.5 KB to 3 KB per row.
2. **Data Volume:** In distribution businesses, 6 months typically amounts to 100,000 – 500,000+ invoice line items. Loading 300,000 rows into `GridGroupingControl` requires **1.2 GB to 2+ GB** of memory.
3. **32-Bit Memory Ceiling:** In .NET 4.8 WinForms (x86 / AnyCPU with 32-bit preference), total virtual address space is 2 GB. Applications crash with OOM around 1.2 – 1.4 GB due to heap fragmentation.
4. **Summary & Filter Freezes:** The grid defines 5 summary columns (`DoubleAggregate`) and a filter bar. Recalculating summaries and managing grouping over hundreds of thousands of records locks the WinForms UI thread.
5. **ClosedXML Crash Risk:** `ExcelButton_Click` loads all filtered grid rows into ClosedXML DOM in memory. Exporting >100,000 rows via ClosedXML DOM triggers OOM.

---

# 3. Architecture & Technical Strategy

### Strategy Component 1: SQL Optimization (Root Cause Fix)
- Restructured SQL in `FakturPerCustomerDal`: Drives directly from `BTR_Faktur` filtered by `FakturDate BETWEEN @Tgl1 AND @Tgl2 AND VoidDate = '3000-01-01'`, then `INNER JOIN BTR_FakturItem`.
- Optimized discount joins into a single grouped aggregation filtered by date range.
- Configured explicit command timeout (`commandTimeout: 120`).
- Eliminated duplicate `Proses()` call on form initialization.

### Strategy Component 2: Sliced Query Loading (Async / Batching)
- Implemented `CreateSlices`: splits large date ranges into 14-day slices.
- Executes slices asynchronously (`async/await`, `ListDataAsync`, `CancellationTokenSource`) so UI remains fully responsive.
- Displays progress via `ProgressBar` and `StatusLabel` in a newly added `StatusStrip`.
- Provides instant cancellation support by toggling `ProsesButton` into a "Batal" button during loading.

### Strategy Component 3: Safe Data Rendering & Safeguards for the Grid
- **Batch UI Updating:** Layout updates are suspended with `InfoGrid.BeginUpdate()` and resumed with `InfoGrid.EndUpdate(true)`.
- **Threshold Safeguard:**
  - If loaded records exceed 50,000 rows, prompts user with a memory warning dialog. Users can choose to keep data in memory for direct Excel export rather than forcing hundreds of thousands of records into the WinForms grid.
- **Export Protection:**
  - `ExcelButton_Click` checks if row count exceeds 100,000 and warns the user before allocating the ClosedXML workbook. Supports exporting from memory even if the user chose not to bind the large dataset to the grid.

---

# 4. Progress Summary

| Phase | Description | Implementation Status | Review Status | Progress |
| ----- | ----------- | --------------------- | ------------- | -------- |
| P1 | SQL Query Optimization & Timeout Adjustment | IMPLEMENTED | GO | 2/2 |
| P2 | Async Sliced Query Engine & UI Progress/Cancel | IMPLEMENTED | GO | 2/2 |
| P3 | Grid Memory Safeguards & Large Export Handling | IMPLEMENTED | GO | 2/2 |

---

# 5. Phases and Slices

## Phase 1 — SQL Query Optimization & Timeout Adjustment

### Slice P1-S01: Refactor SQL Join Order & Discount Aggregation
- **Status:** IMPLEMENTED
- **Review Status:** GO
- **Target:** `src/j05-btr-distrib/btr.infrastructure/SalesContext/FakturPerCustomerRpt/FakturPerCustomerDal.cs`
- **Changes Applied:**
  - Refactored `SqlQuery` to drive from `BTR_Faktur bb` with indexed seek on `FakturDate` and `VoidDate`.
  - Inner join `BTR_FakturItem aa`.
  - Grouped discount calculations into single derived table `ff` with date filtering on `BTR_FakturDiscount`.
  - Set `commandTimeout: 120`.

### Slice P1-S02: Eliminate Duplicate Execution on Form Initialization
- **Status:** IMPLEMENTED
- **Review Status:** GO
- **Target:** `src/j05-btr-distrib/btr.distrib/SalesContext/FakturPerCustomerRpt/FakturPerCustomerForm.cs`
- **Changes Applied:**
  - Removed duplicate calls to `Proses()` in `InitGrid()`.
  - Configured typed empty list on startup and triggered async load in `OnShown()`.

---

## Phase 2 — Async Sliced Query Engine & UI Progress/Cancel

### Slice P2-S01: Add Sliced Data Retrieval with Progress & Cancellation
- **Status:** IMPLEMENTED
- **Review Status:** GO
- **Target:** `IFakturPerCustomerDal.cs` and `FakturPerCustomerDal.cs`
- **Changes Applied:**
  - Added `Task<IEnumerable<FakturPerCustomerView>> ListDataAsync(Periode filter, CancellationToken cancellationToken = default)` to interface and implementation.
  - Implemented with Dapper `CommandDefinition` for native server query cancellation.

### Slice P2-S02: Async UI Implementation with Progress Bar and Cancel Button
- **Status:** IMPLEMENTED
- **Review Status:** GO
- **Target:** `FakturPerCustomerForm.cs` and `FakturPerCustomerForm.Designer.cs`
- **Changes Applied:**
  - Added `StatusStrip`, `StatusLabel`, and `ProgressBar`.
  - Added 14-day slicing logic (`CreateSlices`).
  - Added `ProsesAsync` with progress updates, control disabling, and "Batal" cancellation handling.
  - Handled `OnFormClosing` cancellation.

---

## Phase 3 — Grid Memory Safeguards & Large Export Handling

### Slice P3-S01: Grid Row Count Guard & Memory Safety
- **Status:** IMPLEMENTED
- **Review Status:** GO
- **Target:** `FakturPerCustomerForm.cs`
- **Changes Applied:**
  - Added 50,000 row safety prompt before binding into `InfoGrid`.
  - Optimized in-memory `Filter` method to single-pass evaluation, eliminating 5 intermediate list allocations and 4 `.Union()` calls.
  - Wrapped grid updates with `InfoGrid.BeginUpdate()` / `InfoGrid.EndUpdate(true)`.

### Slice P3-S02: Large Dataset Excel Export Optimization
- **Status:** IMPLEMENTED
- **Review Status:** GO
- **Target:** `ExcelButton_Click` in `FakturPerCustomerForm.cs`
- **Changes Applied:**
  - Added 100,000 row warning confirmation before invoking ClosedXML.
  - Allowed direct Excel export from memory (`_rawLoadedList`) even when bypassed from the grid.
  - Wrapped export process with `Cursor.WaitCursor` and UI status feedback.
