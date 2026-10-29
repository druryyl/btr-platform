---
Title: SM7-Jadwal Kunjungan — Enable visit data materialization within menu
Code: BTR-DISTR-SM7-VISIT-MATERIALIZE
Artifact: IMPLEMENTATION-PLAN
Version: 1.0
LastUpdated: 2026-10-28
Status: COMPLETED
Execution Approval: APPROVED
---

# 1. Objective

Enable sales visit data materialization directly from within the SM7 – Jadwal Kunjungan menu (`JadwalKunjunganForm`) for a selected salesperson and date, allowing supervisors to populate and inspect operational visit schedules without leaving the application.

Planning Mode: FEATURE-PLANNING

Referenced artifacts:

- FEATURE: `docs/features/visit-plan/feature.md`
- FEASIBILITY-ASSESSMENT: `docs/work/btr-distr-sm7-visit-materialize/BTR-DISTR-SM7-VISIT-MATERIALIZE-FEASIBILITY-ASSESSMENT.md`
- ISSUE: `docs/issues/BTR-DISTR-SM7-VISIT-MATERIALIZE-ISSUE.md` (`BTR-DISTR-SM7-VISIT-MATERIALIZE-001`)

Architecture Applicability: ARCHITECTURE-NOT-REQUIRED

No architectural target-state artifact was required. Implementation relies on existing technical structure. Approved feasibility decisions are authoritative for the change. The current codebase is the source of current technical truth.

---

# 2. Planning Scope

The implementation scope is strictly confined to the desktop presentation and testing layers:

1. **SM7 Form (`btr.distrib`):**
   - Inject the existing `IRegenerateVisitPlanWorker` into `JadwalKunjunganForm`.
   - Add a `MaterializeButton` ("Materialisasi") control to `TopPanel` next to `RefreshButton`.
   - Implement `MaterializeButton_Click` to trigger single-date materialization for `SelectedSalesPersonId` and `SelectedVisitDate`.
   - Add validation guards (past dates disallowed, Sunday skipped with notification).
   - Provide user feedback (wait cursor during execution, result count dialog on completion).
   - Refresh the grids (`BasePlanGrid` and `EffectivePlanGrid`) immediately upon completion.

2. **Automated Verification (`btr.test`):**
   - Add test coverage verifying `JadwalKunjunganForm` constructor dependency resolution with `IRegenerateVisitPlanWorker`.
   - Verify single-date materialization execution and event handling.

**Explicitly out of scope:**
- Modifying `RegenerateVisitPlanWorker`, DALs, or domain models (existing implementation already supports single-date execution).
- Database table or schema changes (`BTR_VisitPlan` and `BTR_VisitPlanException` remain unchanged).
- Modifying the scheduled worker (`btr.visitplan.worker`).

---

# 3. Dependencies

- `IRegenerateVisitPlanWorker` is already registered in `btr.distrib` DI (`Program.cs`) via assembly scanning of `INunaServiceVoid<>`.
- `BTR_VisitPlanException` overlay resolution via `EffectiveVisitPlanResolver` operates independently and preserves existing exceptions.

Slice dependencies:

- `P1-S01` has no prerequisites (`Depends On: None`).
- `P1-S02` depends on `P1-S01` (`Depends On: P1-S01`).

---

# 4. Progress Summary

| Phase | Implementation Status | Review Status | Progress |
|-------|-----------------------|---------------|----------|
| P1    | IMPLEMENTED           | GO            | 2/2      |

Plan Status: COMPLETED  
Execution Approval: APPROVED

---

# 5. Phases

## P1 - Presentation & Verification

Implementation Status: IMPLEMENTED  
Review Status: GO  

### P1-S01

Title: Implement In-Menu Materialization in `JadwalKunjunganForm`

Implementation Status: IMPLEMENTED  
Review Status: GO  

Objective:
Add `MaterializeButton` to `JadwalKunjunganForm` TopPanel, inject `IRegenerateVisitPlanWorker`, and implement the click handler to regenerate the base plan for the selected salesperson and date.

Depends On: None

Repository: `btr-platform` (`src/j05-btr-distrib/btr.distrib`)

Completion Criteria:
1. `JadwalKunjunganForm.Designer.cs` defines `MaterializeButton` in `TopPanel` with proper layout, text ("Materialisasi"), and styling consistent with existing buttons (`RefreshButton`).
2. `JadwalKunjunganForm.cs` constructor accepts `IRegenerateVisitPlanWorker` and assigns it to a private field `_regenerateWorker`.
3. Event handler `MaterializeButton_Click` is wired and performs:
   - Guard: If `SelectedSalesPersonId` is empty, ignore or return.
   - Guard: If `SelectedVisitDate < DateTime.Today`, display warning message ("Tanggal kunjungan harus hari ini atau ke depan.").
   - Guard: If `SelectedVisitDate.DayOfWeek == DayOfWeek.Sunday`, display info message ("Hari Minggu tidak memiliki jadwal rute.").
   - Execution: Set wait cursor (`Cursors.WaitCursor`), call `_regenerateWorker.Execute(new RegenerateVisitPlanRequest { SalesPersonId = SelectedSalesPersonId, FromDate = SelectedVisitDate, ToDate = SelectedVisitDate, AllowPast = false, TriggeredBy = "SM7Manual" })`.
   - Grid update: Call `RefreshPlans()`.
   - Feedback: If `_basePlanRows.Count > 0`, display info ("Materialisasi berhasil: {count} kunjungan dimuat."); if `== 0`, display info ("Materialisasi selesai: template tidak memiliki kunjungan untuk rute ini.").
   - Error handling: Display error dialog on exception.
   - Cleanup: Reset cursor to `Cursors.Default`.
4. `btr.distrib` builds cleanly with zero compilation errors.

Implementation Notes:
- Added `MaterializeButton` ("Materialisasi") control to `TopPanel` of `JadwalKunjunganForm.Designer.cs` styled consistently with `RefreshButton`.
- Injected `IRegenerateVisitPlanWorker` into `JadwalKunjunganForm` constructor and stored in private field `_regenerateWorker`.
- Implemented `MaterializeButton_Click` handler with all required validation guards (empty salesperson guard, past-date warning dialog, Sunday info notification), wait cursor toggle with finally block cleanup, `RegenerateVisitPlanRequest` execution (`TriggeredBy = "SM7Manual"`), grid refreshing via `RefreshPlans()`, and user count/status feedback dialogs.
- `btr.distrib` builds cleanly with 0 warnings and 0 compilation errors.

Changed Files:
- `src/j05-btr-distrib/btr.distrib/SalesContext/VisitPlanAgg/JadwalKunjunganForm.Designer.cs`
- `src/j05-btr-distrib/btr.distrib/SalesContext/VisitPlanAgg/JadwalKunjunganForm.cs`
- `docs/work/btr-distr-sm7-visit-materialize/BTR-DISTR-SM7-VISIT-MATERIALIZE-IMPLEMENTATION-PLAN.md`

Notes:
- Reuses `IRegenerateVisitPlanWorker` from `btr.application.SalesContext.VisitPlanAgg.UseCases`.
- Preserves existing exceptions in `BTR_VisitPlanException`.

---

### P1-S02

Title: Add Verification Tests for SM7 Materialization

Implementation Status: IMPLEMENTED  
Review Status: GO  

Objective:
Provide unit and integration test coverage in `btr.test` ensuring `JadwalKunjunganForm` constructor dependency resolution and single-date materialization behavior.

Depends On: P1-S01

Repository: `btr-platform` (`src/j05-btr-distrib/btr.test`)

Completion Criteria:
1. Create or extend test class in `btr.test/SalesContext` verifying:
   - `JadwalKunjunganForm` can be instantiated with its full dependency graph including `IRegenerateVisitPlanWorker`.
   - `RegenerateVisitPlanWorker` executes successfully for single-date requests (`FromDate == ToDate`) with `TriggeredBy = "SM7Manual"`.
2. All tests pass with `dotnet test`.

Implementation Notes:
- Created `btr.test/SalesContext/JadwalKunjunganFormTest.cs` verifying:
  - `JadwalKunjunganForm` can be directly instantiated with its full dependency graph including `IRegenerateVisitPlanWorker`, verifying control instantiation and private field injection.
  - `JadwalKunjunganForm` can be resolved via `Microsoft.Extensions.DependencyInjection` service provider with all required dependencies.
- Extended `btr.test/SalesContext/RegenerateVisitPlanWorkerTest.cs` with test `Execute_SingleDateRequest_WithSM7ManualTrigger_ExecutesSuccessfullyAndPopulatesPlan` verifying single-date execution (`FromDate == ToDate`) with `TriggeredBy = "SM7Manual"`, checking correct range/future deletion and template plan population.
- Configured project references (`btr.distrib`, `System.Windows.Forms`, `System.Drawing`, `Microsoft.Extensions.DependencyInjection`) in `btr.test.csproj` and `packages.config`.
- All tests pass with `dotnet test` (9/9 passed across `JadwalKunjunganFormTest` and `RegenerateVisitPlanWorkerTest`).

Changed Files:
- `src/j05-btr-distrib/btr.test/SalesContext/JadwalKunjunganFormTest.cs`
- `src/j05-btr-distrib/btr.test/SalesContext/RegenerateVisitPlanWorkerTest.cs`
- `src/j05-btr-distrib/btr.test/btr.test.csproj`
- `src/j05-btr-distrib/btr.test/packages.config`
- `docs/work/btr-distr-sm7-visit-materialize/BTR-DISTR-SM7-VISIT-MATERIALIZE-IMPLEMENTATION-PLAN.md`

Notes:
- Uses existing mock/stub DAL patterns established in `btr.test/SalesContext/RegenerateVisitPlanWorkerTest.cs`.

---

# 6. Change Log

- 2026-10-28: Initial implementation plan created. Execution Approval set to APPROVED.
