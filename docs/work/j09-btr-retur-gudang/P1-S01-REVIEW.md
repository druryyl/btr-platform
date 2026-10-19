---
Code: BTR-RETUR-1280X720-LAYOUT
Artifact: REVIEW
Slice: P1-S01
ReviewIteration: 0
Decision: GO
---

# Slice P1-S01 — Shell Dimensions, Auto-Maximize, Compact Header & Narrower Sidebar (`MainForm.cs`, `UiTheme.cs`)

- Repository: `d:\Project.Private\btr-platform` (`src/j09-btr-retur-gudang`)
- Reviewed against: IMPLEMENTATION-PLAN `BTR-RETUR-1280X720-LAYOUT` (v1.0)
- Architecture: `BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md` (v1.3 — TD-013 §1)
- Feature: `docs/features/return-order-desktop/feature.md` (v1.1)

---

# 1. Testing Gate

A **GO** decision applies only to this slice (P1-S01). It does not authorize testing. Testing may begin only when the IMPLEMENTATION-PLAN is `COMPLETED`: every slice has implementation status `IMPLEMENTED` and review status `GO`. That condition is not met (P2-S02, P3-S03, P4-S05 are NOT-STARTED; P3-S04 is NOT-REVIEWED).

---

# 2. Scope & Criteria-by-Criteria Verification

| Criteria | Expected | Actual (Observed) | Verdict |
| :--- | :--- | :--- | :---: |
| **C1: Window Size** | `new Size(1280, 720)` | `MainForm.cs` line 102: `Size = new Size(1280, 720);` | **PASS** |
| **C2: MinimumSize** | `new Size(1024, 600)` | `MainForm.cs` line 103: `MinimumSize = new Size(1024, 600);` | **PASS** |
| **C3: Auto-Maximize Startup** | Auto-maximize if working area height <= 768 or width <= 1366 | `MainForm.cs` lines 104-108: `Screen.PrimaryScreen != null && (Screen.PrimaryScreen.WorkingArea.Height <= 768 \|\| Screen.PrimaryScreen.WorkingArea.Width <= 1366)` -> `WindowState = FormWindowState.Maximized;` | **PASS** |
| **C4: Header Panel Height & Padding** | Height 44px, Padding `(16, 0, 16, 0)` | `MainForm.cs` lines 140-142: `Height = 44`, `Padding = new Padding(16, 0, 16, 0)` | **PASS** |
| **C5: Brand Container & Typography** | Width 300px, Height 44px; Logo badge (36x22px, font 8.5pt bold); Title 11pt bold at `(44, 11)`; Subtitle hidden | `MainForm.cs` lines 156-190: `pnlBrand` (300x44px), `lblLogoBadge` (36x22px, font 8.5pt bold at `(0, 11)`), `_lblAppTitle` (11pt bold at `(44, 11)`), `lblAppSubtitle.Visible = false` | **PASS** |
| **C6: Right Header Controls** | Height 44px, Padding `(0, 7, 0, 0)`; Depo/Operator badges 8.5pt font, padding `(8, 4, 8, 4)`; Switch button 48x26px; Online pill 26px height, padding `(6, 4, 8, 4)` | `MainForm.cs` lines 200-285: `pnlRightHeader` Height 44px, Padding `(0, 7, 0, 0)`; Badges 8.5pt font, padding `(8, 4, 8, 4)`; `_btnSwitchOperator` 48x26px; `pnlOnlinePill` 26px height, padding `(6, 4, 8, 4)` | **PASS** |
| **C7: Sidebar Width, Padding & Nav Buttons** | Width 200px, Padding `(8, 12, 8, 12)`; Nav buttons 184x40px, top offsets (36, 80, 124, 168); Sync container 80px, sync button 38px | `MainForm.cs` lines 370-455: `_pnlSidebar` Width 200px, Padding `(8, 12, 8, 12)`; Nav buttons 184x40px at Y=(36, 80, 124, 168); Sync container 80px, sync button 38px | **PASS** |
| **C8: Status Strip Height** | Maintained at 24-26px | `MainForm.cs` line 308: `Height = 26` | **PASS** |
| **C9: Net Client Content Host Dimensions** | Width >= 1064px, Height >= 590px on 1280x720 display | Width = 1280 - 200 = 1080px (>= 1064px). Height with 48px taskbar (672px working area) = 672 - 44 - 26 = 602px (>= 590px); Height without taskbar = 720 - 44 - 26 = 650px | **PASS** |
| **C10: UiTheme App Title Font** | `UiTheme.FontAppTitle` adjusted to 11pt Bold | `UiTheme.cs` line 62: `FontAppTitle = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point);` | **PASS** |

---

# 3. Architecture Compliance

- **TD-013 §1 (Shell Geometry & Initialization):**
  - Default startup window size reduced from 1280x800 to 1280x720, and `MinimumSize` reduced from 1024x650 to 1024x600. Fully compliant.
  - Startup detection of working area height <= 768px auto-maximizes window. Fully compliant.
  - Header panel height streamlined to 44px with horizontal alignment of brand and operator badges. Fully compliant.
  - Sidebar panel width streamlined to 200px with 40px nav buttons, yielding 1080px net content width. Fully compliant.
- **Project Layering & Boundaries:**
  - Zero modification to AppTier, Domain, or Infrastructure contracts. UI changes localized strictly to WinForms presentation shell (`MainForm.cs`, `UiTheme.cs`).

---

# 4. Findings

### F-P1-S01-01: Legacy Layout Assertions in `MainFormTests.MainForm_LayoutBounds_AreCorrectWithoutOverlap`
- **Severity:** NOTE
- **Description:** Existing test `MainFormTests.MainForm_LayoutBounds_AreCorrectWithoutOverlap` failed (`Expected: 64, Actual: 44` for header height, and tests for 230px sidebar width) because it asserts the pre-re-engineering layout geometry.
- **Evidence:** `dotnet test` output on `BtrReturGudang.Winform.Tests.dll`:
  ```text
  [FAIL] BtrReturGudang.Winform.Tests.MainFormTests.MainForm_LayoutBounds_AreCorrectWithoutOverlap
  Assert.Equal() Failure: Values differ. Expected: 64, Actual: 44
  ```
- **Resolution Plan:** Per the approved IMPLEMENTATION-PLAN, updating and expanding unit tests to reflect the new 1280x720 layout geometry across all views is explicitly scheduled and assigned to phase **P4-S05** (`BtrReturGudang.Winform.Tests`). This intermediate failure is expected and does not invalidate the P1-S01 slice implementation.
- **Required Correction:** None for P1-S01. Will be updated in P4-S05.

---

# 5. Build and Verification Records

1. **Build:**
   ```powershell
   dotnet build src/j09-btr-retur-gudang/BtrReturGudang.sln
   ```
   - Result: Succeeded (0 Warning(s), 0 Error(s)).

2. **Automated Test Suites:**
   - `BtrReturGudang.AppTier.Tests`: 16 Passed, 0 Failed.
   - `BtrReturGudang.Infrastructure.Tests`: 46 Passed, 0 Failed.
   - `BtrReturGudang.Winform.Tests`: 69 Passed, 1 Failed (due to legacy bounds assertion tracked in F-P1-S01-01, assigned to P4-S05).

---

# 6. Conclusion & Decision

Slice P1-S01 satisfies all planned requirements, architectural decisions, and acceptance criteria. No BLOCKER or MAJOR findings exist.

**Decision: GO**
