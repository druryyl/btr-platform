---
Title: Feasibility Assessment - Barcode Scanner & Keyboard Input Separation in BtrReturGudang.Wpf
Code: IS004-BTRG-SCANNER-INPUT-SEPARATION
Artifact: FEASIBILITY-ASSESSMENT
Version: 1.0
LastUpdated: 2026-11-12
Status: READY-FOR-PLANNING
---

# 1. Request Summary

Assessment of defect `BTRG-SCANNER-INPUT-SEPARATION-001` (recorded in [IS004 - BTRG-SCANNER-KEYBOARD-INPUT-SEPARATION-ISSUE.md](file:///d:/Project.Private/btr-platform/docs/issues/IS004%20-%20BTRG-SCANNER-KEYBOARD-INPUT-SEPARATION-ISSUE.md)).

Referenced artifacts:
- [PRODUCT.md](file:///d:/Project.Private/btr-platform/docs/foundation/PRODUCT.md)
- [DOMAIN.md](file:///d:/Project.Private/btr-platform/docs/foundation/DOMAIN.md)
- [LANDSCAPE.md](file:///d:/Project.Private/btr-platform/docs/foundation/LANDSCAPE.md)
- [WORKFLOW.md](file:///d:/Project.Private/btr-platform/docs/foundation/WORKFLOW.md)
- [ADR-001-desktop-keyboard-wedge-scanning.md](file:///d:/Project.Private/btr-platform/docs/work/barcode-registry/adrs/ADR-001-desktop-keyboard-wedge-scanning.md)
- [BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md](file:///d:/Project.Private/btr-platform/docs/work/j09-btr-retur-gudang/BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md)

## Objective

Assess the technical feasibility, architectural impact, and concrete presentation/service adjustments required in `BtrReturGudang.Wpf` to cleanly separate barcode scanner input from physical keyboard input. The solution must eliminate raw scan leakage into focused textboxes (e.g., `TxtQtyBagus`, `TxtCustomer`), prevent accidental dialog launches from focused buttons (e.g., `BtnSearchCustomer`), and support seamless, hands-free consecutive scanning without manual UI focus management, while preserving full physical keyboard usability.

---

# 2. Current State

## Existing Behavior

1. **Scanner Wedge Interception (`WpfScannerWedgeListener.cs`):**
   - Hooks into `ComponentDispatcher.ThreadPreprocessMessage` to inspect incoming Win32 messages (`WM_KEYDOWN`, `WM_CHAR`).
   - Accumulates printable characters (`c >= 32`) in a `StringBuilder` buffer and measures inter-keystroke intervals against `InterKeystrokeTimeoutMs` (50ms).
   - **Critical Defect:** For all printable characters (`c >= 32`), `ProcessMessage` unconditionally returns `false` (`// Allow normal keystrokes to pass through to the active control`). As a result, every character of the barcode stream is passed directly to whatever WPF control currently has keyboard focus.
   - Only the terminating `VK_RETURN` (0x0D) and its corresponding `WM_CHAR` (`\r`) are suppressed (`handled = true`), at which point `BarcodeScanned` / `OnBarcodeScanned` is dispatched.

2. **Aggressive Textbox Auto-Focusing (`ReturnOrderEntryViewModel.cs` & `ReturnOrderEntryView.xaml.cs`):**
   - When an item is scanned (`ApplyActiveProduct`), the ViewModel explicitly fires `RequestFocusQtyBagus?.Invoke()`.
   - When the same item is scanned consecutively to increment quantity (e.g., Qty 1 -> 2), line 578 of `ReturnOrderEntryViewModel.cs` again fires `RequestFocusQtyBagus?.Invoke()`.
   - `ReturnOrderEntryView.xaml.cs` handles this event by calling `TxtQtyBagus.Focus()` and `TxtQtyBagus.SelectAll()`.
   - Consequently, immediately after the first scan, `TxtQtyBagus` holds keyboard focus. Any subsequent scan leaks its raw barcode characters directly into `TxtQtyBagus`, replacing or corrupting the quantity.

3. **Dialog Return Button Focus Retention (`ReturnOrderEntryView.xaml`):**
   - `BtnSearchCustomer` (`x:Name="BtnSearchCustomer"`, Content: `🔍 Cari [F8]`) is a focusable WPF `Button`.
   - When the user searches/selects a customer via `ManualCustomerSearchDialog`, WPF restores focus to `BtnSearchCustomer` upon dialog dismissal.
   - If the operator then scans an item, incoming characters or Enter keystrokes interact with the focused button, causing the search dialog to re-open unexpectedly.

## Existing Constraints

1. **Hardware & Deployment Constraint (ADR-001):**
   - Per `ADR-001-desktop-keyboard-wedge-scanning.md`, Desktop barcode acquisition must use standard plug-and-play USB HID keyboard wedge scanners without requiring vendor-specific SDKs, specialized COM port drivers, or physical barcode reprogramming across warehouse workstations.
2. **Minimalist, Pragmatic Scope:**
   - The solution must be localized to `BtrReturGudang.Wpf` (presentation and listener service) without disrupting domain logic, local SQLite/SQL Server schemas, or cloud synchronization contracts.
3. **Warehouse Operational Ergonomics:**
   - Warehouse operators frequently scan multiple items consecutively. Scanning must not require touching the mouse or manually shifting focus between scans.

---

# 3. Gap Analysis

| ID | Severity | Gap |
| :--- | :--- | :--- |
| **GAP-001** | CRITICAL | **Uncontrolled Keystroke Passthrough in Wedge Listener:** `WpfScannerWedgeListener.cs` passes all printable characters (`c >= 32`) through to active UI controls during a scan burst, suppressing only the final Enter key. |
| **GAP-002** | CRITICAL | **Premature Focus Hijacking on Quantity Textbox:** `ReturnOrderEntryViewModel` automatically grabs focus onto `TxtQtyBagus` upon initial and consecutive item scans, guaranteeing that subsequent scans dump raw digits into the quantity field. |
| **GAP-003** | MAJOR | **Action Button Focus Capture & Accidental Triggering:** Action/search buttons (such as `BtnSearchCustomer`) retain keyboard focus after dialog closure, causing subsequent scans or Enter keys to re-trigger dialogs. |
| **GAP-004** | MINOR | **Absence of Burst Suppression & Rollback when Typing:** If an operator intentionally focuses an editable textbox and then pulls the scanner trigger, no burst suppression or text rollback occurs, leaving contaminated digits in the field. |

---

# 4. Open Questions

| ID | Question | Impact | Status |
| :--- | :--- | :--- | :--- |
| **OQ-001** | What is the operational constraint regarding barcode scanner hardware configuration? | Governs whether solution requires scanner reconfiguration (prefix/suffix, Virtual COM) or pure software wedge. | **CLOSED** |
| **OQ-002** | How should UI focus behave in `ReturnOrderEntryView` during continuous barcode scanning? | Dictates whether textboxes remain focused by default or focus is kept on a neutral element. | **CLOSED** |
| **OQ-003** | How should the application handle scans when an operator has intentionally focused a TextBox? | Determines wedge burst suppression and text rollback behavior. | **CLOSED** |
| **OQ-004** | How should buttons (e.g., 'Cari Customer [F8]') be prevented from capturing focus and re-triggering? | Determines WPF button focusability and post-dialog focus redirection. | **CLOSED** |
| **OQ-005** | What inter-keystroke interval threshold should be used to detect scanner bursts? | Governs listener timing sensitivity between human typing and hardware scans. | **CLOSED** |

---

# 5. Assumptions

| ID | Assumption |
| :--- | :--- |
| **ASM-001** | All warehouse handheld scanners operate in USB HID keyboard emulation mode and emit ASCII characters terminated by `\r` (Enter / `VK_RETURN`). |
| **ASM-002** | Physical human typing speed does not sustain consecutive inter-character intervals under 50ms across 8 or more characters, whereas USB hardware scanners transmit characters at 2ms to 20ms intervals. |
| **ASM-003** | Action buttons with keyboard shortcuts (`F1`..`F10`, `Esc`) execute globally via Window-level `OnPreviewKeyDown` or command bindings, and therefore do not require WPF keyboard focus to operate. |
| **ASM-004** | The items DataGrid (`DgvItems`) serves as an ideal neutral focus target: it provides visual context, does not handle alphanumeric text entry, and allows keyboard navigation (Up/Down arrows) without input contamination. |

---

# 6. Risks

| ID | Risk | Impact | Mitigation |
| :--- | :--- | :--- | :--- |
| **RISK-001** | Operator needs to manually enter a large quantity (e.g., 100 pcs) but `TxtQtyBagus` is not auto-focused. | Low | Operator can explicitly click `TxtQtyBagus` or press `Tab` / `Alt+Q`. Once quantity is typed, pressing `Enter` commits to grid and automatically restores focus to `DgvItems`. |
| **RISK-002** | Slow or degraded USB ports/hubs causing scanner inter-keystroke intervals to fluctuate near 50ms. | Low | Retain 50ms as the baseline default and ensure `InterKeystrokeTimeoutMs` remains configurable. |
| **RISK-003** | Existing unit tests in `BtrReturGudang.Wpf.Tests` asserting `RequestFocusQtyBagus` fail after removing auto-focus invocations. | Medium | Update test assertions to verify that focus defaults to `RequestFocusGrid` / neutral target rather than quantity textboxes. |

---

# 7. Recommendations & Solution Architecture

## Two-Pillar Solution Architecture

### Pillar 1: Scanner-First Neutral Focus Orchestration (Presentation Layer)
1. **Defocus Quantity Inputs on Scan:**
   - Remove `RequestFocusQtyBagus?.Invoke()` from `ReturnOrderEntryViewModel.ApplyActiveProduct` and from consecutive scan increment logic.
   - Upon initial scan and consecutive scans, focus remains comfortably on `DgvItems` (or neutral container).
   - Quantity increments fluidly in the ViewModel and updates the high-contrast Product Banner without moving keyboard focus into `TxtQtyBagus`.
2. **Neutral Focus on Load & Commit:**
   - On view loaded, and after `CommitQuantityStripToGrid()`, ensure focus is parked on `DgvItems`.
3. **Non-Focusable Action Buttons:**
   - Set `Focusable="False"` on `BtnSearchCustomer` and toolbar action buttons in `ReturnOrderEntryView.xaml`.
   - In `CustomerSearchDialogLauncher`, `ManualSearchDialogLauncher`, and `QuickRegistrationDialogLauncher` completion blocks, explicitly invoke `FocusGrid()` so focus always returns to `DgvItems` rather than lingering on trigger controls.

### Pillar 2: Defensive Burst Suppression & Rollback (Service Layer)
1. **Active Burst Suppression in `WpfScannerWedgeListener`:**
   - In `WpfScannerWedgeListener.ProcessMessage`: when `_buffer.Length >= 1` and `elapsedSinceLast <= InterKeystrokeTimeoutMs`, the keystroke stream is confirmed as a hardware burst.
   - Suppress `WM_KEYDOWN` and `WM_CHAR` for characters 2..N (`handled = true`), preventing them from entering any focused control.
2. **Pragmatic Defocus & Leaked Character Rollback:**
   - When a valid scan terminates with `VK_RETURN`:
     - If an editable WPF `TextBox` currently holds keyboard focus (`Keyboard.FocusedElement as TextBox`):
       - Automatically invoke `TextBox.Undo()` or strip the single leaked leading character to restore its pre-burst text state.
       - Immediately move focus to `DgvItems` (`Keyboard.ClearFocus()` / `FocusGrid()`).
     - Dispatch the normalized barcode to `ActiveConsumer.OnBarcodeScanned`.

---

# 8. Gap Closure

## GAP-001 / GAP-004 / OQ-001 / OQ-003 / OQ-005: Wedge Listener Burst Suppression & Rollback
- **Decision:** Maintain zero hardware reconfiguration (standard USB HID keyboard wedge). In `WpfScannerWedgeListener`, suppress incoming `WM_KEYDOWN` and `WM_CHAR` messages for characters 2..N whenever inter-keystroke interval is `<= 50ms`. Upon valid scan termination (`VK_RETURN`), if a `TextBox` had focus, revert the single leaked initial character and immediately redirect focus to `DgvItems`.
- **Rationale:** Solves input leakage purely in software without requiring warehouse scanner reprogramming sheets or specialized drivers, while providing defense-in-depth even if an operator was actively editing a textbox.
- **Impact:** `src/j09-btr-retur-gudang/BtrReturGudang.Wpf/Services/WpfScannerWedgeListener.cs`.
- **Architecture Impact:** Updates TD-005 realization to enforce active keystroke suppression and post-scan focus reset.
- **Resolved By:** User Alignment via `/grill-me`.
- **Resolved Date:** 2026-11-12

## GAP-002 / GAP-003 / OQ-002 / OQ-004: Scanner-First Neutral Focus & Non-Focusable Buttons
- **Decision:** In `ReturnOrderEntryViewModel`, eliminate automatic `RequestFocusQtyBagus` calls on item scan and quantity increment. In `ReturnOrderEntryView.xaml`, set `Focusable="False"` on `BtnSearchCustomer` and toolbar action buttons. In `ReturnOrderEntryView.xaml.cs`, ensure dialog completion launchers explicitly restore focus to `DgvItems`.
- **Rationale:** Align UI presentation with the primary warehouse workflow: hands-free continuous scanning. Operators only interact with textboxes when intentionally typing bulk quantities. Action buttons cannot capture focus, completely eliminating accidental dialog re-launches.
- **Impact:** `ReturnOrderEntryView.xaml`, `ReturnOrderEntryView.xaml.cs`, `ReturnOrderEntryViewModel.cs`.
- **Architecture Impact:** Localized UI focus orchestration refinement.
- **Resolved By:** User Alignment via `/grill-me`.
- **Resolved Date:** 2026-11-12

---

# 9. Architecture Applicability

## Decision

**ARCHITECTURE-REQUIRED**

## Rationale

Technical realization requires formal updates to technical decision **TD-005 (USB Barcode Scanner Wedge Interception)** in `BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md`, defining the exact state machine for high-speed burst suppression, textbox contamination rollback, and view-level focus orchestration contracts across `WpfScannerWedgeListener` and `ReturnOrderEntryView`.

---

# 10. Planning Readiness

## Readiness Checklist

- [x] All critical gaps resolved
- [x] All required decisions recorded
- [x] All blocking open questions resolved
- [x] Architecture can be finalized or updated

## Status

**READY-FOR-PLANNING**

## Notes

The technical assessment is complete. The next step is to update TD-005 in the target architecture / produce `IS004-BTRG-SCANNER-INPUT-SEPARATION-ARCHITECTURE.md` and generate an executable `IMPLEMENTATION-PLAN`.

---

# 11. References

### Referenced Artifacts
- [IS004 - BTRG-SCANNER-KEYBOARD-INPUT-SEPARATION-ISSUE.md](file:///d:/Project.Private/btr-platform/docs/issues/IS004%20-%20BTRG-SCANNER-KEYBOARD-INPUT-SEPARATION-ISSUE.md)
- [BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md](file:///d:/Project.Private/btr-platform/docs/work/j09-btr-retur-gudang/BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md)
- [ADR-001-desktop-keyboard-wedge-scanning.md](file:///d:/Project.Private/btr-platform/docs/work/barcode-registry/adrs/ADR-001-desktop-keyboard-wedge-scanning.md)

### Referenced Codebase Locations
- [WpfScannerWedgeListener.cs](file:///d:/Project.Private/btr-platform/src/j09-btr-retur-gudang/BtrReturGudang.Wpf/Services/WpfScannerWedgeListener.cs)
- [ReturnOrderEntryView.xaml](file:///d:/Project.Private/btr-platform/src/j09-btr-retur-gudang/BtrReturGudang.Wpf/Views/ReturnOrderEntryView.xaml)
- [ReturnOrderEntryView.xaml.cs](file:///d:/Project.Private/btr-platform/src/j09-btr-retur-gudang/BtrReturGudang.Wpf/Views/ReturnOrderEntryView.xaml.cs)
- [ReturnOrderEntryViewModel.cs](file:///d:/Project.Private/btr-platform/src/j09-btr-retur-gudang/BtrReturGudang.Wpf/ViewModels/ReturnOrderEntryViewModel.cs)
- [WpfScannerWedgeListenerTests.cs](file:///d:/Project.Private/btr-platform/src/j09-btr-retur-gudang/BtrReturGudang.Wpf.Tests/WpfScannerWedgeListenerTests.cs)
