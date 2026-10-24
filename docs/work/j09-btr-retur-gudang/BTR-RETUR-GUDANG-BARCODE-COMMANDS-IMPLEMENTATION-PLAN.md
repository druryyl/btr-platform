---
Title: BTR Retur Gudang - Hands-Free Barcode Scanner Commands Implementation Plan
Code: BTR-RETUR-BARCODE-COMMANDS
Artifact: IMPLEMENTATION-PLAN
Version: 1.0
LastUpdated: 2026-10-24
Status: COMPLETED
Execution Approval: APPROVED
---

# 1. Objective

Implement high-throughput hands-free barcode scanner ergonomics in **BTR Retur Gudang WPF Client (`BtrReturGudang.Wpf`)**, enabling warehouse officers to perform the complete intake process via USB barcode scanner:
1. Multi-scan quantity accumulation (1st scan = Qty 1; re-scans = +1 per scan).
2. Desk command barcodes to switch active quantity entry mode between **Qty Bagus** and **Qty Rusak** without touching keyboard/mouse.
3. Desk command barcodes for document actions: **[F9] Simpan Draft** and **[Esc] Reset Form** (with bonus accelerators for **[Enter] Tambah Item** and **[F10] Simpan & Upload**).
4. Continuous chained auto-commit when scanning subsequent items.

Planning Mode: FEATURE-PLANNING

Referenced artifacts:
- FEATURE: `docs/features/return-order-desktop/feature.md` (AC §16)
- ARCHITECTURE: `docs/work/j09-btr-retur-gudang/BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md`
- FEASIBILITY-ASSESSMENT: `docs/work/j09-btr-retur-gudang/BTR-RETUR-GUDANG-DESKTOP-FEASIBILITY-ASSESSMENT.md`
- DESIGN-SPEC: `docs/work/j09-btr-retur-gudang/barcode-scanner-quantity-and-commands-implementation-plan.md`

Architecture Applicability: ARCHITECTURE-NOT-REQUIRED
No architectural target-state artifact was required. Implementation relies on existing technical structure. Approved feasibility decisions are authoritative for the change. The current codebase is the source of current technical truth.

---

# 2. Planning Scope

The plan encompasses:
1. **Domain Value Object (`BtrReturGudang.Domain`)**: `BarcodeCommand` value object parsing Code 128 / Code 39 strings into strongly-typed `BarcodeCommandType` with zero third-party dependencies.
2. **ViewModel State Machine (`BtrReturGudang.Wpf`)**:
   - `QuantityEntryMode` (`Bagus`, `Rusak`) tracking in `ReturnOrderEntryViewModel`.
   - Command barcode dispatching in `OnBarcodeScanned`.
   - Multi-scan auto-increment (+1) for both Bagus and Rusak modes.
   - Chained auto-commit: scanning a new product automatically commits previous staged item if `Qty > 0`.
   - Action command integration: auto-commit and save draft on `CMD:F9`, reset on `CMD:ESC`.
3. **Ergonomic Presentation UI (`BtrReturGudang.Wpf`)**:
   - Mode indicator pill badge (`[● MODE: QTY BAGUS]` / `[● MODE: QTY RUSAK]`) in the quantity strip.
   - Dynamic border/accent highlighting on `TxtQtyBagus` (Emerald) vs `TxtQtyRusak` (Rose).
   - Code-behind focus routing (`FocusQtyRusak()` alongside `FocusQtyBagus()`).
4. **Automated Verification**:
   - Domain unit test suite in `BtrReturGudang.Domain.Tests`.
   - Integration and workflow test suite in `BtrReturGudang.Wpf.Tests`.

---

# 3. Dependencies

- **Runtime Target**: .NET Framework 4.7.1.
- **Participating Projects**:
  - `BtrReturGudang.Domain` (Value Objects, Entities)
  - `BtrReturGudang.Domain.Tests` (Domain Unit Tests)
  - `BtrReturGudang.Wpf` (MVVM, Presentation Views, Styles)
  - `BtrReturGudang.Wpf.Tests` (WPF Integration Tests)

---

# 4. Progress Summary

| Phase | Description | Implementation Status | Review Status | Progress |
| :--- | :--- | :--- | :--- | :--- |
| **P1** | Domain Value Object & Command Parsing | IMPLEMENTED | GO | 2/2 |
| **P2** | ViewModel State Machine & Multi-Scan Logic | IMPLEMENTED | GO | 2/2 |
| **P3** | Presentation UI & Visual Mode Indicators | IMPLEMENTED | GO | 1/1 |
| **P4** | Verification & End-to-End Workflow Tests | IMPLEMENTED | GO | 1/1 |

---

# 5. Phases

## P1 - Domain Value Object & Command Parsing

Implementation Status: IMPLEMENTED  
Review Status: GO  

### P1-S01: BarcodeCommand Value Object
- **Title:** Implement `BarcodeCommand` Value Object in Domain
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Create `BtrReturGudang.Domain.ValueObjects.BarcodeCommand` and `BarcodeCommandType` enum to parse and classify command barcodes without external dependencies.
- **Depends On:** None
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.Domain/ValueObjects/BarcodeCommand.cs` created.
  - Defines `BarcodeCommandType`:
    `None`, `SwitchToQtyBagus`, `SwitchToQtyRusak`, `SaveDraft`, `ResetForm`, `CommitItem`, `SaveAndUpload`.
  - Implements `bool TryParse(string rawInput, out BarcodeCommandType command)`.
  - Normalizes input via `BarcodeNormalization.Normalize` and performs case-insensitive prefix/alias matching:
    - `CMD:BAGUS`, `CMD:QTY_BAGUS`, `$$BAGUS`, `BTR:QTY-BAGUS` ➔ `SwitchToQtyBagus`
    - `CMD:RUSAK`, `CMD:QTY_RUSAK`, `$$RUSAK`, `BTR:QTY-RUSAK` ➔ `SwitchToQtyRusak`
    - `CMD:F9`, `CMD:SIMPAN`, `CMD:SIMPAN_DRAFT`, `$$F9` ➔ `SaveDraft`
    - `CMD:ESC`, `CMD:RESET`, `CMD:RESET_FORM`, `$$ESC` ➔ `ResetForm`
    - `CMD:ENTER`, `CMD:TAMBAH`, `$$ENTER` ➔ `CommitItem`
    - `CMD:F10`, `CMD:UPLOAD`, `$$F10` ➔ `SaveAndUpload`
  - Returns `false` / `None` for standard EAN-13, UPC, SKU barcodes.
  - Project builds cleanly.
- **Implementation Notes:**
  - Created `BarcodeCommand` value object and `BarcodeCommandType` enum in `BtrReturGudang.Domain.ValueObjects`.
  - Implemented `TryParse(string rawInput, out BarcodeCommandType command)` and `TryParse(string rawInput, out BarcodeCommand command)` using `BarcodeNormalization.Normalize` with case-insensitive matching across all command aliases.
  - Added support for stripped start/stop asterisks (Code 39 scanner fallbacks).
  - Built cleanly with 0 warnings and 0 errors.
- **Changed Files:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.Domain/ValueObjects/BarcodeCommand.cs`

---

### P1-S02: Domain Unit Tests for BarcodeCommand
- **Title:** Implement Unit Tests for `BarcodeCommand` in `BtrReturGudang.Domain.Tests`
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Author comprehensive unit tests validating parsing, aliases, whitespace/case-insensitivity, and negative cases.
- **Depends On:** P1-S01
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.Domain.Tests/BarcodeCommandTests.cs` created.
  - Verifies all command types and aliases parse correctly.
  - Verifies case-insensitivity (`cmd:bagus`, `Cmd:Rusak`, `CMD:f9`).
  - Verifies leading/trailing whitespace and CR/LF/Tab resilience.
  - Verifies product barcodes (`8998008152130`, `1018013`, `BR0182`) return `None`.
  - All tests in `BtrReturGudang.Domain.Tests` pass (`dotnet test`).
- **Implementation Notes:**
  - Created `src/j09-btr-retur-gudang/BtrReturGudang.Domain.Tests/BarcodeCommandTests.cs` using xUnit.
  - Added unit test cases for all command aliases across `SwitchToQtyBagus`, `SwitchToQtyRusak`, `SaveDraft`, `ResetForm`, `CommitItem`, and `SaveAndUpload`.
  - Added test cases validating case-insensitivity, whitespace/CR/LF/Tab resilience, Code 39 start/stop asterisk stripping, null/empty/whitespace handling, product barcodes and unrecognized command rejection returning `false` and `BarcodeCommandType.None`.
  - Added test cases validating `BarcodeCommand` Value Object equality, hash code calculation, `==`/`!=` operator consistency, `Parse`, `TryParse`, and `None` singleton property.
  - Verified all 86 unit tests in `BtrReturGudang.Domain.Tests` pass (`dotnet test`).
  - Verified solution builds cleanly with 0 warnings and 0 errors.
- **Changed Files:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.Domain.Tests/BarcodeCommandTests.cs`

---

## P2 - ViewModel State Machine & Multi-Scan Logic

Implementation Status: IMPLEMENTED  
Review Status: GO  

### P2-S03: Mode Tracking & Multi-Scan Quantity Accumulation
- **Title:** Implement `QuantityEntryMode` & Multi-Scan Accumulation in `ReturnOrderEntryViewModel`
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Update `ReturnOrderEntryViewModel` to manage active scan mode (`Bagus` vs `Rusak`), handle `CMD:BAGUS` / `CMD:RUSAK`, and initialize/increment quantities on single and repeated scans.
- **Depends On:** P1-S01
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - Defines `QuantityEntryMode { Bagus, Rusak }` in `ReturnOrderEntryViewModel.cs`.
  - Properties `ActiveQuantityMode` (default `Bagus`), `IsModeBagus`, `IsModeRusak`, `ModeBadgeText`.
  - When `CMD:BAGUS` is scanned: `ActiveQuantityMode = QuantityEntryMode.Bagus`, audio feedback plays, banner status updates to `"MODE SCAN: QTY BAGUS"`, invokes `RequestFocusQtyBagus`.
  - When `CMD:RUSAK` is scanned: `ActiveQuantityMode = QuantityEntryMode.Rusak`, audio feedback plays, banner status updates to `"MODE SCAN: QTY RUSAK"`, invokes `RequestFocusQtyRusak`.
  - When scanning an item barcode:
    - 1st scan: sets item in banner; sets initial quantity = 1 for the active mode (`QtyBagus = 1` if Bagus; `QtyRusak = 1` if Rusak).
    - Repeat scan of same barcode: increments active mode quantity (+1) per scan (`2`, `3`, etc.) with success audio cue.
    - Switching mode on same item: retains active product and switches target quantity increment.
- **Implementation Notes:**
  - Defined `QuantityEntryMode { Bagus, Rusak }` enum in `ReturnOrderEntryViewModel.cs`.
  - Added properties `ActiveQuantityMode` (default `Bagus`), `IsModeBagus`, `IsModeRusak`, and `ModeBadgeText` (`"[● MODE: QTY BAGUS]"` vs `"[● MODE: QTY RUSAK]"`), along with `ToggleQuantityModeCommand` and `ToggleQuantityMode()`.
  - Added `RequestFocusQtyRusak` event to complement `RequestFocusQtyBagus`.
  - Updated `OnBarcodeScanned` to evaluate `BarcodeCommand.TryParse`:
    - On `SwitchToQtyBagus` (`CMD:BAGUS`, `$$BAGUS`, etc.): updates `ActiveQuantityMode = QuantityEntryMode.Bagus`, plays success tone, updates `BannerStatus = "MODE SCAN: QTY BAGUS"`, and invokes `RequestFocusQtyBagus`.
    - On `SwitchToQtyRusak` (`CMD:RUSAK`, `$$RUSAK`, etc.): updates `ActiveQuantityMode = QuantityEntryMode.Rusak`, plays success tone, updates `BannerStatus = "MODE SCAN: QTY RUSAK"`, and invokes `RequestFocusQtyRusak`.
  - Multi-scan quantity accumulation:
    - On initial scan (1st scan): `ApplyActiveProduct` sets initial quantity to 1 based on active mode (`QtyBagus = 1` if Bagus; `QtyRusak = 1` if Rusak).
    - On consecutive re-scans of the same barcode: increments active mode quantity (`+1`) per scan with success audio cue.
    - On desk mode switch: active staged product is preserved, allowing target quantity to switch and increment seamlessly.
  - Form reset resets `ActiveQuantityMode` to `Bagus`.
  - Updated existing WPF test assertions to align with initial Qty = 1 ergonomics and added new integration tests in `WorkflowIntegrationTests.cs`.
  - Built cleanly with 0 warnings and 0 errors (`dotnet build src/j09-btr-retur-gudang/BtrReturGudang.Wpf/BtrReturGudang.Wpf.csproj`).
- **Changed Files:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.Wpf/ViewModels/ReturnOrderEntryViewModel.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Wpf.Tests/WorkflowIntegrationTests.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Wpf.Tests/ReturnOrderEntryViewTests.cs`

---

### P2-S04: Chained Auto-Commit & Command Barcode Execution
- **Title:** Implement Chained Auto-Commit and `CMD:F9` / `CMD:ESC` Actions in ViewModel
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Add seamless chained auto-commit when scanning a new item and route action commands (`SaveDraft`, `ResetForm`, `CommitItem`, `SaveAndUpload`).
- **Depends On:** P2-S03
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - In `OnBarcodeScanned`: if a different barcode is scanned while an existing item is staged with `QtyBagus > 0 || QtyRusak > 0`, system automatically calls `CommitQuantityStripToGrid()` before loading the new item.
  - When `CMD:F9` (or `SaveDraft` command) is scanned: auto-commits active item if staged, then executes `SaveDraftCommand`.
  - When `CMD:ESC` (or `ResetForm` command) is scanned: executes `ResetFormCommand`.
  - When `CMD:ENTER` is scanned: executes `CommitQuantityStripToGrid()`.
  - When `CMD:F10` is scanned: auto-commits active item if staged, then executes `SaveAndUploadCommand`.
  - Solves hands-free intake without keyboard intervention.
- **Implementation Notes:**
  - Updated `OnBarcodeScanned` in `ReturnOrderEntryViewModel.cs`:
    - Added chained auto-commit: scanning a different barcode while an item is actively staged with `QtyBagus > 0 || QtyRusak > 0` calls `CommitQuantityStripToGrid()` before staging the newly scanned barcode.
    - Added `CommitItem` (`CMD:ENTER`, `CMD:TAMBAH`): executes `CommitQuantityStripToGrid()` and returns early.
    - Added `SaveDraft` (`CMD:F9`, `CMD:SIMPAN`): auto-commits active staged item if `QtyBagus > 0 || QtyRusak > 0`, executes `SaveDraftCommand` (or `SaveDraftAsync()`), and returns early.
    - Added `SaveAndUpload` (`CMD:F10`, `CMD:UPLOAD`): auto-commits active staged item if `QtyBagus > 0 || QtyRusak > 0`, executes `SaveAndUploadCommand` (or `SaveAndUploadAsync()`), and returns early.
    - Added `ResetForm` (`CMD:ESC`, `CMD:RESET`): executes `ResetFormCommand` (or `ResetForm(promptUser: true)`), and returns early.
  - Added workflow integration tests in `WorkflowIntegrationTests.cs`:
    - `BarcodeScan_ChainedScanDifferentProduct_AutoCommitsPreviousItem`
    - `BarcodeCommand_F9_AutoCommitsAndSavesDraft`
    - `BarcodeCommand_Esc_ResetsForm`
  - Verified compilation with 0 warnings, 0 errors, and all 118 WPF tests passing cleanly.
- **Changed Files:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.Wpf/ViewModels/ReturnOrderEntryViewModel.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Wpf.Tests/WorkflowIntegrationTests.cs`

---

## P3 - Presentation UI & Visual Mode Indicators

Implementation Status: IMPLEMENTED  
Review Status: GO  

### P3-S05: Ergonomic Mode Pill & Dynamic Highlighting
- **Title:** Integrate Mode Pill Badge and Border Highlighting in `ReturnOrderEntryView`
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Add visual mode indicators in XAML and code-behind focus routing for `QtyRusak`.
- **Depends On:** P2-S03
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - `Views/ReturnOrderEntryView.xaml`:
    - Quantity Strip header includes Mode Pill Button (`[● MODE: QTY BAGUS]` with green accent vs `[● MODE: QTY RUSAK]` with rose accent).
    - Mode Pill is interactive (clicking it toggles mode).
    - `TxtQtyBagus` and `TxtQtyRusak` highlight with distinct accent border and glow when their respective mode is active.
  - `Views/ReturnOrderEntryView.xaml.cs`:
    - Implements `FocusQtyRusak()` with `Dispatcher.BeginInvoke`.
    - Wires `RequestFocusQtyRusak` event subscription.
- **Implementation Notes:**
  - Updated `Views/ReturnOrderEntryView.xaml`:
    - Refactored Quantity Strip container into a 2-row Grid with a dedicated header row (`DockPanel`).
    - Added interactive `BtnModePill` button displaying `ModeBadgeText` bound to `ToggleQuantityModeCommand`, with `Focusable="False"`, cursor pointer, rounded pill border (`CornerRadius="12"`), and dynamic color switching via `DataTrigger` (Green/Emerald accent when `IsModeBagus` is true, Rose/Danger accent when `IsModeRusak` is true).
    - Enhanced `TxtQtyBagus` and `TxtQtyRusak` with dynamic style triggers applying high-contrast accent border (Emerald vs Rose, `BorderThickness="2"`), subtle background tint (`Brush.Success.Light` / `Brush.Danger.Light`), and drop shadow glow effect (`DropShadowEffect`, BlurRadius 6, Opacity 0.35) when their respective mode is active.
  - Updated `Views/ReturnOrderEntryView.xaml.cs`:
    - Implemented `FocusQtyRusak()` with `Dispatcher.BeginInvoke` focusing and selecting text in `TxtQtyRusak` (mirroring `FocusQtyBagus()`).
    - Wired subscription to `vm.RequestFocusQtyRusak += FocusQtyRusak;` in `OnDataContextChanged` (both unsubscription on old VM and subscription on new VM).
    - Added `Unloaded += OnUnloaded;` event handler cleanly unsubscribing `RequestFocusQtyBagus`, `RequestFocusQtyRusak`, and `RequestFocusGrid` on view teardown.
    - Updated `OnLoaded` initial focus logic to focus `TxtQtyRusak` if `vm.IsModeRusak` is active.
  - Verified project build succeeded with 0 warnings and 0 errors. All 86 domain tests and 118 WPF tests pass cleanly.
- **Changed Files:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.Wpf/Views/ReturnOrderEntryView.xaml`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Wpf/Views/ReturnOrderEntryView.xaml.cs`

---

## P4 - Verification & End-to-End Workflow Tests

Implementation Status: IMPLEMENTED  
Review Status: GO  

### P4-S06: Automated Workflow Integration Tests
- **Title:** Implement Automated Workflow Tests in `BtrReturGudang.Wpf.Tests`
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Verify end-to-end multi-scan accumulation, desk mode switching, chained auto-commit, and barcode-triggered save/reset actions.
- **Depends On:** P1-S02, P2-S04, P3-S05
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - New integration tests added to `BtrReturGudang.Wpf.Tests`:
    - `BarcodeCommand_SwitchToBagusAndRusak_UpdatesModeState`
    - `BarcodeScan_SingleScan_InitializesQuantityToOne`
    - `BarcodeScan_RepeatedScan_IncrementsQuantity`
    - `BarcodeScan_MixedBagusAndRusak_StagesBothQuantities`
    - `BarcodeScan_ChainedScanDifferentProduct_AutoCommitsPreviousItem`
    - `BarcodeCommand_F9_AutoCommitsAndSavesDraft`
    - `BarcodeCommand_Esc_ResetsForm`
  - All tests in `BtrReturGudang.Wpf.Tests` pass.
  - Regression testing across solution confirms 0 warnings, 0 errors.
- **Implementation Notes:**
  - Implemented comprehensive automated workflow integration tests in `WorkflowIntegrationTests.cs`:
    - `BarcodeCommand_SwitchToBagusAndRusak_UpdatesModeState`: verifies mode switching via barcode commands (`CMD:RUSAK`, `$$BAGUS`) and corresponding UI state updates and focus requests.
    - `BarcodeScan_SingleScan_InitializesQuantityToOne`: verifies initial scan in Bagus mode initializes QtyBagus=1, and in Rusak mode initializes QtyRusak=1.
    - `BarcodeScan_RepeatedScan_IncrementsQuantity`: verifies repeated scans of the same barcode increment the active mode's quantity counter.
    - `BarcodeScan_MixedBagusAndRusak_StagesBothQuantities`: verifies scanning an item in Bagus mode, switching to Rusak via barcode, and scanning again accumulates both Bagus and Rusak quantities for the same item.
    - `BarcodeScan_ChainedScanDifferentProduct_AutoCommitsPreviousItem`: verifies scanning a different product automatically commits the staged line item to the grid before staging the new product.
    - `BarcodeCommand_F9_AutoCommitsAndSavesDraft`: verifies scanning `CMD:F9` auto-commits the staged item and saves the order to the local draft repository.
    - `BarcodeCommand_Esc_ResetsForm`: verifies scanning `CMD:ESC` resets the form inputs and returns to standby state.
    - `BarcodeCommand_Enter_CommitsItem_AndClearsStrip`: verifies scanning `CMD:ENTER` and `CMD:TAMBAH` commits active staged items to the grid and clears the strip.
    - `BarcodeCommand_F10_AutoCommitsAndSavesAndUploads`: verifies scanning `CMD:F10` auto-commits active staged items and executes full save and cloud upload pass, leaving the document in Synced state.
    - `BarcodeCommand_ChainedScanMixedBagusAndRusak_AutoCommitsBothLines`: verifies chained scanning of a new product when the previous item has both Bagus and Rusak staged results in both lines being committed to the grid.
    - `BarcodeCommand_ToggleQuantityModeCommand_TogglesCorrectly`: verifies WPF binding command toggling between Bagus and Rusak.
    - `BarcodeCommand_UnrecognizedCommand_TreatedAsUnknownBarcode`: verifies unrecognized barcode commands do not throw, maintain state, and are treated as catalog misses with appropriate audio warning.
  - All 123 tests in `BtrReturGudang.Wpf.Tests` passed cleanly.
  - All 271 tests across the entire solution (`BtrReturGudang.sln`) passed cleanly (0 failed, 0 skipped).
  - Clean build succeeded with 0 warnings and 0 errors across all projects.
- **Changed Files:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.Wpf.Tests/WorkflowIntegrationTests.cs`

---

# 6. Change Log

- 2026-10-24: Initial version 1.0 created by `ica-architect` from approved design artifact. Structurally sliced into 4 phases and 6 continuous execution slices. Execution Approval granted.
