---

Code: BTR-RETUR-SESSION-ACTOR-001
Artifact: REVIEW
Slice: P2-S04
ReviewIteration: 1
Decision: GO
---

# Slice P2-S04 - Operator Google-Email Entry and Display in the Desktop UI

Repository: `src/j09-btr-retur-gudang/BtrReturGudang.Winform`
Reviewed against: IMPLEMENTATION-PLAN `BTR-RETUR-SESSION-ACTOR-001` (v1.1);
ARCHITECTURE `BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md` (v1.2 - TD-007, TD-012,
5.1, 5.2 SCR-01/SCR-07, 7, 10.4).

This REVIEW artifact is written at the explicit instruction of the review run.
Under the standard review policy a GO decision does not require a REVIEW artifact;
it is produced here to preserve verifiable criteria-by-criteria evidence.

# Testing Gate

A GO decision applies only to this slice. It does not authorize testing. Testing
may begin only when the IMPLEMENTATION-PLAN is COMPLETED: every slice has
implementation status IMPLEMENTED and review status GO. That condition is not
met (P2-S05 and P3-S06 are NOT-STARTED), so COMPLETED was not set.

# Scope Reviewed

- Objective: allow the workstation operator identity to be configured and
  reviewed as a Google email, alongside the existing display name.
- Planned scope: `OperatorSwitcherDialog`, `SettingsView`, `MainForm` within
  `BtrReturGudang.Winform`.
- Dependency: P1-S02 (IMPLEMENTED, Review Status GO) - satisfied.

# Criteria Verification

## C1 - OperatorSwitcherDialog: email input beside name input, validation, persistence, SelectedOperatorEmail

Status: SATISFIED

Evidence (`Views/OperatorSwitcherDialog.cs`):
- Email input added beside the name input: name label at `Location(20,18)` /
  textbox `(20,42)`; "Google Email Operator:" label at `Location(350,18)` /
  textbox `(350,42)` (lines 86-121). Both are added to the body panel
  (lines 159-165). Dialog widened to `ClientSize = new Size(680, 240)` (line 47).
- Non-empty + basic email-shape validation in `OnSaveClick` (lines 191-201):
  `if (string.IsNullOrWhiteSpace(newEmail) || !EmailShapeRegex.IsMatch(newEmail))`
  with `EmailShapeRegex = ^[^@\s]+@[^@\s]+\.[^@\s]+$` (lines 17-18), and a
  warning `MessageBox` + `DialogResult = DialogResult.None` on failure.
- Persists both values (lines 203-204):
  `_registryHelper.LastOperatorName = newOperator;`
  `_registryHelper.LastOperatorEmail = newEmail;`
- Exposes `SelectedOperatorEmail` (line 27) plus existing `SelectedOperatorName`
  (line 26); sets both on successful save (lines 205-206).
- The existing non-empty operator-name validation is preserved (lines 179-189).

Note: existence of an `IRegistryHelper` property `LastOperatorEmail` confirmed in
`BtrReturGudang.AppTier/Contracts/IRegistryHelper.cs` line 13 (P1-S01, GO).

## C2 - SettingsView: load/display operator email in identity card and persist on save

Status: SATISFIED

Evidence (`Views/SettingsView.cs`):
- Identity card height raised 185 -> 215 (line 188) and a "Google Email Operator:"
  label (`Location(24,162)`) plus `_lblOperatorEmailDisplay` (`Location(210,162)`,
  default `"Belum diatur"`) added to the card (lines 236-257).
- `LoadCurrentConfiguration` reads and displays the configured email (lines 471-474):
  `_currentOperatorEmail = _registryHelper.LastOperatorEmail ?? string.Empty;` and
  display of `"Belum diatur"` when blank, otherwise the email value. The
  `_registryHelper == null` fallback branch also resets it (lines 483-484).
- Save re-persists the email in `OnSaveAllClick` (lines 643-644):
  `_registryHelper.LastOperatorEmail = _currentOperatorEmail ?? string.Empty;`.
- The operator switcher click handler refreshes the displayed email after a
  dialog save (lines 498-502).
- Test accessor `OperatorEmailLabel` added (line 61).

## C3 - MainForm: badge shows operator name; operator email exposed as badge tooltip

Status: SATISFIED

Evidence (`Views/MainForm.cs`):
- Badge text remains the operator display name: `string operatorName =
  !string.IsNullOrWhiteSpace(_registryHelper.LastOperatorName) ?
  _registryHelper.LastOperatorName : "Operator-1";` and
  `_lblOperatorBadge = new Label { Text = $"👤 {operatorName}", ... }`
  (lines 210-223).
- Email exposed as the badge tooltip (lines 225-228):
  `string operatorEmail = _registryHelper.LastOperatorEmail ?? string.Empty;`
  `_operatorToolTip = new ToolTip();`
  `_operatorToolTip.SetToolTip(_lblOperatorBadge, FormatOperatorEmailTooltip(operatorEmail));`
  `FormatOperatorEmailTooltip` renders the email, or a "belum dikonfigurasi"
  message when blank (lines 548-553).
- Tooltip updated on operator switch (line 538) while the badge label keeps the
  selected name (line 537).
- Test accessor `OperatorBadge` added (line 720); tooltip disposed in
  `OnFormClosed` (line 775).

## C4 - BtrReturGudang.Winform compiles with zero errors

Status: SATISFIED

Evidence:
- Command: `dotnet build "src/j09-btr-retur-gudang/BtrReturGudang.Winform/BtrReturGudang.Winform.csproj" -v minimal`
- Observed: `Build succeeded. 0 Warning(s) 0 Error(s)` (Domain, AppTier,
  Infrastructure, and Winform all built; `BtrReturGudang.Winform.exe` produced).

## Constraint - Record-attribution call sites remain name-based and unchanged

Status: SATISFIED

Evidence:
- `Views/ReturnOrderEntryView.cs` line 1220:
  `var operatorName = _registryHelper?.LastOperatorName ?? "OPERATOR";` - still
  name-based, and `operatorName` is what is passed to `_returnOrderService.CreateAsync(...)`.
  `git diff` shows the file carries unrelated working-tree changes from other work
  (e.g. `Create` -> `CreateAsync`), but the attribution call site remains
  name-sourced; the email was not substituted into record attribution.
- `Views/QuickBarcodeRegistrationDialog.cs` line 408:
  `string operatorName = _registryHelper?.LastOperatorName ?? "OPERATOR";`
  `git diff --numstat` reports zero changes to this file.
- `Views/BarcodeRegistrationDialog.cs` line 309 likewise still reads
  `_registryHelper?.LastOperatorName`; zero changes (`git diff --numstat`).

## Scope creep

Status: NONE ATTRIBUTABLE TO P2-S04

`git status` shows a broad working tree with changes from other slices/features
(e.g. `ReturnOrderEntryView.cs`, `j06` controller, tests). The P2-S04 deliverable
itself is confined to `OperatorSwitcherDialog.cs`, `SettingsView.cs`, and
`MainForm.cs`; no other file was changed by this slice. P2-S05 (session-resolve
validation) is correctly NOT-STARTED.

# Findings

| ID | Severity | Description | Evidence | Required Correction |
| :--- | :--- | :--- | :--- | :--- |
| F1 | NOTE | The RS-07 identity card already has a `_btnSwitchOperator`; the email display is read-only in the card and edited only via the switcher dialog, consistent with the plan (settings save only re-persists the current value). | `SettingsView.cs` lines 221-257, 643-644 | None - observation. |
| F2 | NOTE | Email-shape validation is intentionally a permissive structural check (`^[^@\s]+@[^@\s]+\.[^@\s]+$`); authoritative registration/active-account enforcement is deferred to `POST api/session/resolve` (P2-S05 / TD-012). | `OperatorSwitcherDialog.cs` lines 17-18, 191-201 | None - matches approved slice. |

No BLOCKER or MAJOR findings.

# Decision

GO.

Slice scope, architecture compliance (TD-007, TD-012), and dependency (P1-S02)
are satisfied; all four completion criteria are met and the zero-error build was
reproduced independently.

# Re-Review History

- ReviewIteration 1 (2026-10-17): Initial review. Decision GO. No prior findings.
