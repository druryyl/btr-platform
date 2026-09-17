# Implementation Plan: Separate Faktur Save Preview Form

## Objective

Separate the Faktur preview-before-save workflow from the shared RDLC preview form.

The application must have two physically separate Windows Forms:

1. `RdlcViewerForm`: the original read-only report preview used by existing modules.
2. `FakturSavePreviewForm`: the Faktur-specific preview that provides `SAVE`, `SAVE & PRINT`, and `Cancel` outcomes.

The change must not alter the persistence workflow already implemented in `FakturForm` and must not require unrelated forms to adopt Faktur-specific behavior.

## Current State

`RdlcViewerForm` is shared by multiple modules, including:

- `PurchaseContext/InvoiceAgg/InvoiceForm.cs`
- `InventoryContext/ReturJualAgg/ReturJualForm.cs`
- `InventoryContext/MutasiAgg/MutasiForm.cs`
- `PurchaseContext/ReturBeliFeature/ReturBeliForm.cs`
- `FinanceContext/TagihanAgg/TagihanForm.cs`
- `SalesContext/FakturControlAgg/FakturControlForm.cs`
- `SalesContext/FakturAgg/FakturForm.cs`

The current shared form contains Faktur-specific save-confirm functionality, including `FakturPreviewChoice`, `PreviewChoice`, save buttons, `EnableSaveConfirm()`, and save-confirm event handlers. This couples unrelated report-preview callers to the Faktur workflow.

`FakturForm` currently uses the shared form in two different ways:

- `ShowPreviewDialog()` opens a preview and expects a save choice.
- `ShowReadOnlyPreview()` opens a read-only preview for voided Faktur.

`FakturForm.PrintFakturRdlc()` also calls the shared form's static `PrintDirect()` method for silent printing after persistence.

## Target Design

### `RdlcViewerForm`

Restore this form to the original shared report-preview responsibility:

- Display the configured RDLC report and data sources.
- Preserve normal ReportViewer toolbar behavior, including the toolbar Print action.
- Preserve paper-size switching and the existing last-paper-size behavior.
- Preserve `SetReportData()` and `PrintDirect()` behavior used by existing callers and Faktur silent printing.
- Do not expose Faktur save choices or persistence-related state.
- Do not contain `FakturPreviewChoice`, `PreviewChoice`, `EnableSaveConfirm()`, save buttons, or Faktur-specific toolbar hiding.

### `FakturSavePreviewForm`

Create a separate physical form in `SalesContext/FakturAgg` based on the current save-capable preview implementation.

It must:

- Own `FakturPreviewChoice` or reference a clearly Faktur-specific equivalent.
- Provide `SAVE`, `SAVE & PRINT`, and `Cancel` buttons.
- Default to `Cancel` when the form is closed through the window close action.
- Hide the ReportViewer toolbar Print action while save confirmation is active.
- Preserve the current RDLC report loading, data-source binding, paper-size switching, and draft-title behavior needed by Faktur preview.
- Return the selected choice to `FakturForm`; it must not save Faktur data itself.
- Keep direct printing out of the preview form. `FakturForm` remains responsible for saving, reloading, and invoking `PrintDirect()` after `SAVE & PRINT`.

The new form should have its own code-behind and designer files, for example:

- `src/j05-btr-distrib/btr.distrib/SalesContext/FakturAgg/FakturSavePreviewForm.cs`
- `src/j05-btr-distrib/btr.distrib/SalesContext/FakturAgg/FakturSavePreviewForm.Designer.cs`

Use a distinct form class and distinct designer partial class. Do not implement this separation as a mode flag on `RdlcViewerForm`.

## Implementation Steps

### 1. Restore the shared viewer

Update:

- `src/j05-btr-distrib/btr.distrib/SalesContext/FakturAgg/RdlcViewerForm.cs`
- `src/j05-btr-distrib/btr.distrib/SalesContext/FakturAgg/RdlcViewerForm.Designer.cs`

Remove the save-confirm additions from the shared form and restore its original layout and behavior. The designer must no longer contain the Faktur bottom action panel or save buttons. Keep the report viewer and any existing paper-size controls.

Ensure existing callers can continue compiling without changes. In particular, retain the public APIs used by the application:

- `RdlcViewerForm()`
- `SetReportData(...)`
- `ShowDialog()` through normal form behavior
- `RdlcViewerForm.PrintDirect(...)`

### 2. Add the Faktur-specific form

Create `FakturSavePreviewForm.cs` and `FakturSavePreviewForm.Designer.cs`.

Move the save-capable behavior from the current shared form into this new form, updating class names and private field references consistently. Keep the report viewer implementation functionally equivalent for Faktur preview.

The choice behavior must remain:

- `SAVE` returns `FakturPreviewChoice.Save` and closes the dialog.
- `SAVE & PRINT` returns `FakturPreviewChoice.SaveAndPrint` and closes the dialog.
- `Cancel` returns `FakturPreviewChoice.Cancel` and closes the dialog.
- Closing the dialog without choosing an action returns `Cancel`.

If the implementation retains `EnableSaveConfirm()`, it must be local to `FakturSavePreviewForm`. Prefer making the new Faktur form inherently save-confirm capable if that simplifies the class, but do not expose the capability through `RdlcViewerForm`.

### 3. Update Faktur preview call sites

Update only `FakturForm.cs`:

- In `ShowPreviewDialog()`, instantiate `FakturSavePreviewForm` instead of `RdlcViewerForm`.
- Continue calling the new form's report-data setup and save-confirm configuration as appropriate.
- In `ShowReadOnlyPreview()`, continue instantiating the restored `RdlcViewerForm`, because voided Faktur preview must remain read-only while retaining normal report printing.
- Leave `PrintFakturRdlc()` using `RdlcViewerForm.PrintDirect()` unless the implementation must move this utility for compilation. If moved, preserve the same public static API or update only this Faktur call site without changing print semantics.

Do not change `_saveFakturWorker.Execute(req)`, `SavePiutang()`, `SavePackingOrder()`, the reload-after-save logic, or the `SAVE` versus `SAVE & PRINT` branching.

### 4. Verify other callers remain on the original form

Do not modify the existing `new RdlcViewerForm()` calls in:

- `InvoiceForm.cs`
- `ReturJualForm.cs`
- `MutasiForm.cs`
- `ReturBeliForm.cs`
- `TagihanForm.cs`
- `FakturControlForm.cs`

Search the entire `btr.distrib` project for `new RdlcViewerForm()` after implementation. Every non-Faktur caller must continue to use the original viewer. The only save-capable viewer construction should be in `FakturForm.ShowPreviewDialog()`.

## Behavioral Requirements

### Shared preview behavior

- Existing forms open a report preview without Faktur save buttons.
- Existing forms retain their normal ReportViewer toolbar Print action.
- Existing report templates and data-source names remain unchanged.
- Existing paper-size switching remains unchanged.
- Existing forms do not receive `FakturPreviewChoice` or save-confirm state.

### Faktur preview behavior

- Clicking FakturForm SAVE opens the save-capable preview form.
- Opening the preview alone does not persist the Faktur.
- Cancel or closing the preview persists nothing.
- Preview SAVE returns control to `FakturForm`, which then persists the Faktur without printing.
- Preview SAVE & PRINT returns control to `FakturForm`, which persists and prints the reloaded persisted Faktur.
- Voided Faktur preview remains read-only and uses `RdlcViewerForm`.

## Files Affected

### New files

- `src/j05-btr-distrib/btr.distrib/SalesContext/FakturAgg/FakturSavePreviewForm.cs`
- `src/j05-btr-distrib/btr.distrib/SalesContext/FakturAgg/FakturSavePreviewForm.Designer.cs`

### Modified files

- `src/j05-btr-distrib/btr.distrib/SalesContext/FakturAgg/RdlcViewerForm.cs`
- `src/j05-btr-distrib/btr.distrib/SalesContext/FakturAgg/RdlcViewerForm.Designer.cs`
- `src/j05-btr-distrib/btr.distrib/SalesContext/FakturAgg/FakturForm.cs`

### Files that should not require changes

- `PurchaseContext/InvoiceAgg/InvoiceForm.cs`
- `InventoryContext/ReturJualAgg/ReturJualForm.cs`
- `InventoryContext/MutasiAgg/MutasiForm.cs`
- Other existing `RdlcViewerForm` callers

There are no database, domain, application-worker, or report-template changes in this scope.

## Risks and Mitigations

### Risk: Designer references remain attached to the wrong partial class

Mitigation: Rename the class declaration, form `Name`, and all generated field references consistently in both new designer files. Confirm the project recognizes both forms as Windows Forms files.

### Risk: Shared viewer loses normal printing behavior

Mitigation: Compare the restored `RdlcViewerForm` against the pre-save-capability behavior and test a representative report from Invoice, Retur Jual, and Mutasi.

### Risk: Faktur preview accidentally persists data

Mitigation: Keep persistence exclusively in `FakturForm.SaveButton_Click()`. The new preview form may only set a choice and close itself.

### Risk: Faktur silent printing is changed

Mitigation: Preserve `RdlcViewerForm.PrintDirect()` and verify that `SAVE & PRINT` still prints the reloaded persisted Faktur, not the draft preview data.

### Risk: New save form exposes normal toolbar printing

Mitigation: Verify the save-capable form hides the built-in ReportViewer Print button while leaving the shared viewer unchanged.

## Verification Plan

### Static verification

- Search for all `RdlcViewerForm` usages and confirm unrelated forms still instantiate only `RdlcViewerForm`.
- Confirm `FakturForm.ShowPreviewDialog()` instantiates only `FakturSavePreviewForm`.
- Confirm `FakturForm.ShowReadOnlyPreview()` instantiates `RdlcViewerForm`.
- Confirm no save-specific symbols remain in `RdlcViewerForm`.
- Confirm no production save worker or persistence dependency exists in `FakturSavePreviewForm`.

### Build verification

- Build `src/j05-btr-distrib/btr.distrib/btr.distrib.csproj` for the repository's supported configuration.
- Resolve any Windows Forms designer nesting or generated-code compile errors.

### Manual verification

1. Open Invoice, Retur Jual, and Mutasi print previews. Confirm they show the original viewer without `SAVE` or `SAVE & PRINT` buttons.
2. Confirm the normal ReportViewer toolbar Print action remains available for those forms.
3. In FakturForm, click SAVE and confirm the new save-capable preview opens.
4. Close or cancel the Faktur preview and confirm no Faktur, Piutang, or Packing Order is persisted.
5. Choose SAVE and confirm the Faktur is persisted without printing.
6. Choose SAVE & PRINT and confirm the persisted, reloaded Faktur is printed.
7. Open a voided Faktur and confirm its preview is read-only and uses the original viewer.

## Acceptance Criteria

- Two distinct physical form classes exist: `RdlcViewerForm` and `FakturSavePreviewForm`.
- All non-Faktur report-preview callers continue to use `RdlcViewerForm` without source changes or save controls.
- `FakturForm` uses `FakturSavePreviewForm` only for preview-before-save confirmation.
- Faktur persistence and printing behavior remains unchanged from the currently working flow.
- The shared viewer retains normal report preview, toolbar printing, paper-size, and direct-print behavior.
- The project builds successfully and the manual verification scenarios pass.
