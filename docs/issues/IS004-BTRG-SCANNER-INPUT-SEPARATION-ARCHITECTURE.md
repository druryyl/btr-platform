---
Title: Target Architecture - Barcode Scanner & Keyboard Input Separation in BtrReturGudang.Wpf
Code: IS004-BTRG-SCANNER-INPUT-SEPARATION
Artifact: ARCHITECTURE
Version: 1.0
LastUpdated: 2026-11-12
---

# 1. Overview

This architecture defines the technical realization for separating USB barcode scanner wedge input from physical keyboard input in the warehouse desktop application (`BtrReturGudang.Wpf`).

Specifically, it resolves defect `BTRG-SCANNER-INPUT-SEPARATION-001` (recorded in [IS004 - BTRG-SCANNER-KEYBOARD-INPUT-SEPARATION-ISSUE.md](file:///d:/Project.Private/btr-platform/docs/issues/IS004%20-%20BTRG-SCANNER-KEYBOARD-INPUT-SEPARATION-ISSUE.md)) by establishing a two-pillar defense architecture:
1. **Presentation Layer — Scanner-First Neutral Focus Pattern:** Eliminates premature and aggressive focus hijacking onto quantity textboxes (`TxtQtyBagus`, `TxtQtyRusak`) during single item scans, consecutive item scans, and mode toggles. Restores and anchors UI focus on neutral container `DgvItems` across view loading, line-item commits, and modal dialog dismissals. Strips focusability (`Focusable="False"`) from action and toolbar buttons (`BtnSearchCustomer`, `BtnAddItem`, `BtnSaveDraft`, `BtnSaveAndUpload`, `BtnReset`, `BtnManualSearch`), preventing buttons from capturing focus and being accidentally triggered by incoming scan bursts or Enter keystrokes.
2. **Service Layer — Active Burst Suppression & Contamination Rollback:** Enhances `WpfScannerWedgeListener` to selectively suppress `WM_CHAR` messages for characters 2..N whenever inter-keystroke intervals confirm a high-speed hardware scan burst ($\le 50\text{ms}$). Provides defense-in-depth for the leading character (character 1) by detecting whether an editable `TextBox` held focus upon scan completion (`VK_RETURN`), automatically reverting the single leaked character via `TextBox.Undo()`, clearing focus to `DgvItems`, and dispatching the clean normalized barcode to `ActiveConsumer.OnBarcodeScanned`.

**Referenced BUG ISSUE:** [IS004 - BTRG-SCANNER-KEYBOARD-INPUT-SEPARATION-ISSUE.md](file:///d:/Project.Private/btr-platform/docs/issues/IS004%20-%20BTRG-SCANNER-KEYBOARD-INPUT-SEPARATION-ISSUE.md) (`BTRG-SCANNER-INPUT-SEPARATION-001`)  
**Originating Analysis Artifact:** [IS004-BTRG-SCANNER-INPUT-SEPARATION-FEASIBILITY-ASSESSMENT.md](file:///d:/Project.Private/btr-platform/docs/issues/IS004-BTRG-SCANNER-INPUT-SEPARATION-FEASIBILITY-ASSESSMENT.md) (`IS004-BTRG-SCANNER-INPUT-SEPARATION`)  
**Parent / Baseline ARCHITECTURE:** [BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md](file:///d:/Project.Private/btr-platform/docs/work/j09-btr-retur-gudang/BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md) (`BTR-RETUR-DESKTOP-001`, updating `TD-005` and refining `TD-011` / `TD-013` for WPF)

---

# 2. Architectural Basis

## Business Context

- **DOMAIN:** [PRODUCT.md](file:///d:/Project.Private/btr-platform/docs/foundation/PRODUCT.md), [DOMAIN.md](file:///d:/Project.Private/btr-platform/docs/foundation/DOMAIN.md), [WORKFLOW.md](file:///d:/Project.Private/btr-platform/docs/foundation/WORKFLOW.md), [RETURN-ORDER-DOMAIN.md](file:///d:/Project.Private/btr-platform/docs/work/return-order/RETURN-ORDER-DOMAIN.md), and [BARCODE-REGISTRY-DOMAIN.md](file:///d:/Project.Private/btr-platform/docs/work/barcode-registry/BARCODE-REGISTRY-DOMAIN.md):
  - Warehouse return intake is a high-throughput physical intake operation conducted at warehouse loading bays.
  - Operators handle physical products, wear industrial work gloves, and operate handheld USB barcode scanners.
  - The primary operational posture is **hands-free, continuous barcode scanning**: operators must be able to scan dozens of return items consecutively without touching the mouse or keyboard to manage UI focus.
  - Manual keyboard interaction is secondary and reserved strictly for intentional exceptions: searching customers (`F8`), searching uncatalogued products (`F2`), typing large bulk quantities (e.g., 50 pcs), or entering return notes.

## Analysis Input

- **FEASIBILITY-ASSESSMENT:** [IS004-BTRG-SCANNER-INPUT-SEPARATION-FEASIBILITY-ASSESSMENT.md](file:///d:/Project.Private/btr-platform/docs/issues/IS004-BTRG-SCANNER-INPUT-SEPARATION-FEASIBILITY-ASSESSMENT.md) provides the approved gap-closure decisions:
  - **GAP-001 / GAP-004 / OQ-001 / OQ-003 / OQ-005 (Burst Suppression & Rollback):** Retain zero hardware reconfiguration (standard plug-and-play USB HID keyboard wedge per ADR-001). In `WpfScannerWedgeListener`, suppress incoming `WM_CHAR` messages for characters 2..N whenever inter-keystroke interval is $\le 50\text{ms}$. Upon valid scan termination (`VK_RETURN`), if an editable `TextBox` currently holds keyboard focus, execute `TextBox.Undo()` to revert the single leaked character and immediately redirect focus to `DgvItems`.
  - **GAP-002 / GAP-003 / OQ-002 / OQ-004 (Scanner-First Neutral Focus Orchestration):** In `ReturnOrderEntryViewModel`, eliminate automatic `RequestFocusQtyBagus` / `RequestFocusQtyRusak` invocations on initial scan, consecutive scan increment, and mode toggle. In `ReturnOrderEntryView.xaml`, set `Focusable="False"` on `BtnSearchCustomer` and toolbar action buttons. In `ReturnOrderEntryView.xaml.cs`, ensure view loaded and all modal dialog completion callbacks explicitly restore focus to `DgvItems`.

```text
BUG ISSUE (BTRG-SCANNER-INPUT-SEPARATION-001)
                    +
FEASIBILITY-ASSESSMENT (IS004-BTRG-SCANNER-INPUT-SEPARATION)
                    +
PARENT ARCHITECTURE (BTR-RETUR-DESKTOP-001: TD-005 / TD-011 / TD-013)
                    ↓
               ARCHITECTURE
```

---

# 3. Scope

## Included

1. **Wedge Listener Service Layer (`BtrReturGudang.Wpf/Services/WpfScannerWedgeListener.cs`):**
   - Implement active burst suppression state machine: suppress `WM_CHAR` messages for characters 2..N when `elapsedSinceLast <= InterKeystrokeTimeoutMs` (50ms).
   - Preserve `WM_KEYDOWN` dispatch for alphanumeric keys to ensure Windows `TranslateMessage` continues generating `WM_CHAR` for the listener buffer.
   - Suppress `VK_RETURN` on `WM_KEYDOWN` and subsequent translated `\r` / `\n` on `WM_CHAR`.
   - Implement defensive post-scan focus detection and rollback: if `Keyboard.FocusedElement is TextBox tb && !tb.IsReadOnly`, invoke `tb.Undo()` and clear focus / restore neutral focus.
   - Dispatch normalized barcode to `ActiveConsumer.OnBarcodeScanned(normalized)`.

2. **Presentation Layer — ViewModel Orchestration (`BtrReturGudang.Wpf/ViewModels/ReturnOrderEntryViewModel.cs`):**
   - Decouple scan handling from textbox focus: remove `RequestFocusQtyBagus?.Invoke()` from `ApplyActiveProduct()`.
   - Remove `RequestFocusQtyBagus?.Invoke()` and `RequestFocusQtyRusak?.Invoke()` from consecutive scan increment logic.
   - Update `SetQuantityMode()` to maintain neutral focus instead of stealing focus into quantity textboxes.
   - Ensure consecutive scans fluidly update `QtyBagus` / `QtyRusak`, trigger success audio beeps, and update `BannerStatus` while keeping focus neutral.
   - Retain `RequestFocusGrid?.Invoke()` upon `CommitQuantityStripToGrid()` to keep focus anchored on the grid after commits.

3. **Presentation Layer — View Focusability Hardening (`BtrReturGudang.Wpf/Views/ReturnOrderEntryView.xaml`):**
   - Set `Focusable="False"` on `BtnSearchCustomer` (Row 0 Customer Header).
   - Set `Focusable="False"` on `BtnAddItem` (Row 2 Quantity Strip).
   - Set `Focusable="False"` on toolbar buttons: `BtnSaveDraft`, `BtnSaveAndUpload`, `BtnReset`, `BtnManualSearch` (Row 5 Action Toolbar).

4. **Presentation Layer — View Code-Behind & Dialog Completion Routing (`BtrReturGudang.Wpf/Views/ReturnOrderEntryView.xaml.cs`):**
   - In `OnLoaded`, default initial keyboard focus to `DgvItems` via `FocusGrid()` rather than quantity inputs.
   - In `CustomerSearchDialogLauncher` completion handler, explicitly invoke `FocusGrid()` after customer selection.
   - In `ManualSearchDialogLauncher` completion handler, explicitly invoke `FocusGrid()` after product selection.
   - In `QuickRegistrationDialogLauncher` completion handler, explicitly invoke `FocusGrid()` after barcode registration.

5. **Automated Test Suite (`src/j09-btr-retur-gudang/BtrReturGudang.Wpf.Tests`):**
   - Update `WpfScannerWedgeListenerTests.cs` to verify active suppression (`Assert.True(handledChar)`) for characters 2..N during rapid bursts ($\le 50\text{ms}$).
   - Add unit tests verifying `TextBox.Undo()` rollback and focus clearing upon rapid scan completion while a `TextBox` is focused.
   - Update `WorkflowIntegrationTests.cs` to align focus assertions with the neutral focus architecture (asserting focus remains on grid / `RequestFocusGrid` rather than `RequestFocusQtyBagus`).

## Excluded

- Hardware barcode scanner reconfiguration (no vendor barcode programming sheets or scanner reconfiguration needed).
- Hardware communication protocol changes (no OPOS, POS for .NET, or Virtual COM port drivers; standard USB HID wedge remains unchanged per ADR-001).
- Database schema changes (no modifications to local SQL Server `btrgd` or SQLite).
- Cloud API synchronization contracts (`j06-pkl-btrade-api`).
- Core business entities, value objects, and domain logic (`BtrReturGudang.Domain`).

---

# 4. Technical Decisions

### TD-005-A: High-Speed Burst Discrimination & Selective Character Suppression in Wedge Listener
- **Decision:** Implement selective message suppression in `WpfScannerWedgeListener.ProcessMessage` using an inter-keystroke timing heuristic of $\le 50\text{ms}$ (`InterKeystrokeTimeoutMs`).
- **Realization:**
  1. **Character 1 ($\text{Length} = 1$):** At the arrival of the first character, intent cannot be proven without delaying human typing. Character 1's `WM_CHAR` passes through to the active control (`return false`). Character 1 is appended to `_buffer` and `_lastKeystrokeTimestamp` is recorded.
  2. **Characters 2..N ($\text{Length} \ge 2$):** When `WM_CHAR` arrives with `elapsedSinceLast <= InterKeystrokeTimeoutMs`, the keystroke stream is confirmed as a hardware scanner burst. Character $N$ is appended to `_buffer`, `_lastKeystrokeTimestamp` is updated, and `ProcessMessage` returns `true` (`handled = true`).
  3. **WPF `WM_KEYDOWN` vs `WM_CHAR` Separation:** `WM_KEYDOWN` for alphanumeric characters (keys $0..9, A..Z$) is deliberately **not** marked handled (`return false`). In Win32 and WPF, marking `WM_KEYDOWN` as handled in `ThreadPreprocessMessage` prevents Windows `TranslateMessage` from running, which would prevent subsequent `WM_CHAR` messages from being generated for the listener. Because WPF `TextBox` inserts text exclusively during `WM_CHAR` processing (`TextInput` event), suppressing `WM_CHAR` completely blocks character entry into any focused control while allowing `_buffer` to capture the entire barcode.
  4. **Terminating Enter:** When `VK_RETURN` arrives on `WM_KEYDOWN` within the burst threshold, `WM_KEYDOWN` returns `true` (handled), preventing Enter from triggering default buttons or inserting line breaks. `_suppressNextEnterChar` is set to `true` to suppress the subsequent translated `WM_CHAR '\r'`.
  5. **Navigation / Function Keys:** Keystrokes matching `IsNavigationOrFunctionKey` (`Backspace`, `Tab`, `Escape`, `PageUp/Down`, `F1`..`F12`) flush the buffer immediately and pass through (`return false`).

### TD-005-B: Post-Burst Contamination Rollback & Neutral Focus Restoration
- **Decision:** Provide defense-in-depth against single-character leakage into focused textboxes upon valid scan completion.
- **Realization:**
  1. When a complete scan terminates (`VK_RETURN` with `_buffer.Length >= MinBarcodeLength` and interval $\le 50\text{ms}$):
  2. The listener inspects the active focus target: `var focusedTextBox = Keyboard.FocusedElement as TextBox;`.
  3. If an editable, non-read-only `TextBox` holds keyboard focus:
     - If `focusedTextBox.CanUndo` is true, invoke `focusedTextBox.Undo()` to revert the single leaked initial character and restore the previous text and selection state.
     - Clear keyboard focus via `Keyboard.ClearFocus()` (or trigger focus redirection to `DgvItems`).
  4. The normalized barcode is then dispatched to `ActiveConsumer.OnBarcodeScanned(normalized)` and event subscribers.

### TD-005-C: Presentation Layer Scanner-First Neutral Focus Pattern
- **Decision:** Shift the UI focus model in `ReturnOrderEntryView` from "aggressive textbox auto-focus" to "scanner-first neutral focus".
- **Realization:**
  1. **Decouple Initial Scan:** In `ReturnOrderEntryViewModel.ApplyActiveProduct`, remove `RequestFocusQtyBagus?.Invoke()` and `RequestFocusQtyRusak?.Invoke()`. After an item is scanned, focus remains on `DgvItems`.
  2. **Decouple Consecutive Scans:** In `ReturnOrderEntryViewModel.OnBarcodeScanned`, when the same barcode is scanned consecutively, increment `QtyBagus` or `QtyRusak`, update `BannerStatus` (`QTY BAGUS: 2`), and play success audio without requesting focus on the quantity textboxes.
  3. **Decouple Mode Toggles:** In `SetQuantityMode()`, remove `RequestFocusQtyBagus` / `RequestFocusQtyRusak`. Mode switches update the mode pill badge without shifting focus into textboxes.
  4. **Manual Quantity Input Workflow:** If an operator needs to manually enter a large quantity (e.g., 50 pcs), they explicitly click `TxtQtyBagus` or press `Tab`. `AutoSelectOnFocus` selects the current number, the operator types "50", and presses `Enter`. Pressing `Enter` commits the strip to `DgvItems` and fires `RequestFocusGrid`, immediately returning focus to the neutral grid for subsequent scans.

### TD-005-D: Action Button Non-Focusability & Dialog Dismissal Focus Redirection
- **Decision:** Prevent buttons from capturing keyboard focus, eliminating accidental button clicks caused by scanner burst terminators or Enter keystrokes.
- **Realization:**
  1. In `ReturnOrderEntryView.xaml`, set `Focusable="False"` on:
     - `BtnSearchCustomer` (`x:Name="BtnSearchCustomer"`, `🔍 Cari [F8]`)
     - `BtnAddItem` (`x:Name="BtnAddItem"`, `+ Tambah [Enter]`)
     - `BtnSaveDraft` (`x:Name="BtnSaveDraft"`, `[F9] Simpan Draft`)
     - `BtnSaveAndUpload` (`x:Name="BtnSaveAndUpload"`, `[F10] Simpan & Upload`)
     - `BtnReset` (`x:Name="BtnReset"`, `[Esc] Reset Form`)
     - `BtnManualSearch` (`x:Name="BtnManualSearch"`, `[F2] Cari Barang`)
  2. Keyboard shortcuts (`F2`, `F8`, `F9`, `F10`, `Esc`) continue to function globally via `Window.PreviewKeyDown` and `UserControl.PreviewKeyDown`, independent of button focus.
  3. In `ReturnOrderEntryView.xaml.cs`, all modal dialog completion callbacks (`CustomerSearchDialogLauncher`, `ManualSearchDialogLauncher`, `QuickRegistrationDialogLauncher`) explicitly call `FocusGrid()` before returning, ensuring focus is always anchored on `DgvItems`.
  4. In `OnLoaded`, invoke `FocusGrid()` so initial view activation anchors focus on `DgvItems`.

---

# 5. Component Responsibilities

| Component | Responsibility |
| :--- | :--- |
| `WpfScannerWedgeListener` | Intercepts Win32 messages via `ComponentDispatcher.ThreadPreprocessMessage`; measures inter-keystroke intervals; selectively suppresses `WM_CHAR` for characters 2..N during bursts; suppresses terminating `VK_RETURN`; executes defensive `TextBox.Undo()` rollback on character 1; normalizes barcodes (VO-01); dispatches clean scans to `ActiveConsumer`. |
| `ReturnOrderEntryViewModel` | Manages operational intake state, customer/driver selection, staged item quantities (`QtyBagus`, `QtyRusak`), banner statuses, consecutive scan increments, and line-item commits to `Items`; fires `RequestFocusGrid` upon commits and mode changes; completely devoid of aggressive textbox focus requests. |
| `ReturnOrderEntryView.xaml` | Declares UI presentation layout; configures `Focusable="False"` on all action and toolbar buttons; hosts `DgvItems` as the primary neutral focus target; binds commands and controls. |
| `ReturnOrderEntryView.xaml.cs` | Orchestrates UI keyboard focus via `FocusGrid()`, `FocusQtyBagus()`, `FocusQtyRusak()`; handles `Loaded` event to set initial focus to `DgvItems`; routes `PreviewKeyDown` for `F2` and `F8`; ensures all dialog launcher callbacks explicitly refocus `DgvItems` upon dismissal. |
| `NumericTextBox` | Custom numeric text control for `TxtQtyBagus` and `TxtQtyRusak`; supports `AutoSelectOnFocus`, numpad ergonomics, watermark, and standard `TextBox.Undo()` capability. |
| `ManualCustomerSearchDialog` / `ManualItemSearchDialog` / `QuickBarcodeRegistrationDialog` | Modal dialogs providing search and barcode registration workflows; dismissed cleanly without retaining focus on parent trigger controls. |
| `WpfScannerWedgeListenerTests` / `WorkflowIntegrationTests` | Automated unit and integration test suites validating burst suppression, character rollback, neutral focus anchoring, and hands-free intake workflows. |

---

# 6. Integration Design

| Source | Target | Purpose |
| :--- | :--- | :--- |
| Win32 Message Pump | `WpfScannerWedgeListener.OnThreadPreprocessMessage` | Pre-processes all UI thread messages (`WM_KEYDOWN`, `WM_CHAR`) before WPF control routing. |
| `WpfScannerWedgeListener` | `BarcodeNormalization` | Cleans raw scanner strings per VO-01 rules (trims whitespace/control chars, preserves leading zeros). |
| `WpfScannerWedgeListener` | `Keyboard.FocusedElement` (`TextBox`) | Performs `Undo()` rollback on the single leaked character if a `TextBox` had focus during a valid scan burst. |
| `WpfScannerWedgeListener` | `IScannerConsumer` (`ReturnOrderEntryViewModel.OnBarcodeScanned`) | Delivers clean, normalized barcode string to active view model. |
| `ReturnOrderEntryViewModel` | `ReturnOrderEntryView.xaml.cs` (`RequestFocusGrid`) | Notifies view to set WPF keyboard focus to `DgvItems` on commits and mode updates. |
| `ReturnOrderEntryView.xaml.cs` | Modal Dialogs (`ManualCustomerSearchDialog`, etc.) | Launches modal dialogs and unconditionally restores focus to `DgvItems` upon dialog closure. |
| `MainWindow.xaml.cs` | `ReturnOrderEntryViewModel` | Dispatches global keyboard hotkeys (`F9`, `F10`, `Esc`) to the active view model without requiring focused buttons. |

---

# 7. Data Ownership

| Data | Owner |
| :--- | :--- |
| Keystroke Timing & Buffer State | `WpfScannerWedgeListener` (private encapsulated state: `_buffer`, `_lastKeystrokeTimestamp`, `_suppressNextEnterChar`) |
| Return Order Intake State (Header & Items) | `ReturnOrderEntryViewModel` (`Items`, `CurrentReturnOrderId`, `CustomerInputText`, `SelectedDriver`, etc.) |
| Staged Scanned Item & Quantity | `ReturnOrderEntryViewModel` (`_currentScanResult`, `QtyBagus`, `QtyRusak`, `ActiveQuantityMode`) |
| WPF Keyboard Focus Target | `ReturnOrderEntryView` (managed on WPF UI Thread via `DgvItems.Focus()`) |

---

# 8. Database Design

## New Tables

None.

## Modified Tables

None.

## Relationships

No database relationships introduced or modified.

## Migration Considerations

None. This architecture is an in-process presentation and service layer improvement within `BtrReturGudang.Wpf`. No database schema migrations or data backfills are required.

---

# 9. Cross-Cutting Concerns

### Performance
- `WpfScannerWedgeListener.ProcessMessage` executes on the UI thread for every keyboard message.
- The timing evaluation, character appending, and condition checks execute in under $0.05\text{ms}$ ($\approx 50\text{ns}$), introducing zero human-perceptible latency to keystrokes or UI responsiveness.

### Concurrency & Threading
- Message preprocessing and focus manipulation execute strictly on the single-threaded WPF UI Dispatcher.
- No cross-thread marshaling or lock contention is introduced.

### Ergonomics & Usability
- Warehouse operators experience true hands-free continuous scanning.
- Consecutive scans of the same item automatically increment quantity without dumping numbers into textboxes.
- Action buttons never capture focus, completely eliminating accidental dialog launches.
- Physical keyboard typing remains 100% operational for intentional search, text notes, and manual quantity overrides.

### Backward Compatibility & Hardware Independence
- Fully compliant with ADR-001: requires zero physical barcode scanner reconfiguration or vendor-specific drivers.
- Works across all existing warehouse USB HID keyboard-wedge scanners.

---

# 10. Implementation Constraints

1. **WPF Framework Compatibility:**
   - Must run reliably under .NET Framework 4.7.1 within `BtrReturGudang.Wpf`.
2. **Win32 Message Pipeline Integrity:**
   - Must **not** suppress `WM_KEYDOWN` messages for alphanumeric characters ($0..9, A..Z$), as doing so suppresses Windows `TranslateMessage` and prevents `WM_CHAR` generation for the listener buffer.
   - Suppression for characters 2..N must occur on `WM_CHAR`.
   - Suppression for terminating Enter must occur on `WM_KEYDOWN` (`VK_RETURN`) with follow-up suppression on `WM_CHAR` (`\r`).
3. **No Focus Hijacking:**
   - ViewModel scan handlers must never call `RequestFocusQtyBagus` or `RequestFocusQtyRusak`. Focus must remain anchored on `DgvItems`.
4. **Button Focusability:**
   - All action and toolbar buttons in `ReturnOrderEntryView.xaml` must declare `Focusable="False"`.
5. **Regression Safety:**
   - All existing keyboard accelerators (`F2`, `F8`, `F9`, `F10`, `Esc`, `Enter`) must continue to function.
   - All unit and integration tests in `BtrReturGudang.Wpf.Tests` must pass.

---

# 11. Acceptance Conditions

Implementation of this architecture is complete when all of the following conditions are satisfied:

1. **Consecutive Item Scanning:**
   - Scanning the same item barcode multiple times consecutively increments the active quantity (`QtyBagus` or `QtyRusak`), updates the Product Banner, and produces success beeps without moving focus into `TxtQtyBagus` or polluting any field.
2. **Dialog Return Protection:**
   - Dismissing `ManualCustomerSearchDialog` (via selection or Cancel/Esc) restores focus to `DgvItems`. A subsequent barcode scan immediately registers the scanned item without re-opening the customer search dialog.
3. **Neutral Focus Integrity:**
   - During continuous scanning with focus on `DgvItems`, raw barcode characters are not entered into any control, and the items grid remains cleanly selected.
4. **Defensive Textbox Rollback:**
   - If an operator deliberately focuses an editable textbox (e.g., `TxtCustomer` or `TxtQtyBagus`) and then triggers a hardware barcode scan burst ($\le 50\text{ms}$), the single leading character is reverted via `TextBox.Undo()`, focus is cleared/redirected to `DgvItems`, and the barcode is processed normally.
5. **Manual Keyboard Typing Preserved:**
   - Typing manually into `TxtCustomer`, `TxtKeterangan`, `TxtItemNotes`, and `TxtQtyBagus` operates with full responsiveness; slow typing ($> 50\text{ms}$ interval) is never suppressed or treated as a barcode scan.
6. **Action Button Non-Focusability:**
   - Clicking `BtnSearchCustomer`, `BtnAddItem`, `BtnSaveDraft`, `BtnSaveAndUpload`, `BtnReset`, or `BtnManualSearch` does not leave keyboard focus resting on the button.
7. **Test Verification:**
   - All unit tests in `WpfScannerWedgeListenerTests.cs` and workflow integration tests in `WorkflowIntegrationTests.cs` pass without failure.
