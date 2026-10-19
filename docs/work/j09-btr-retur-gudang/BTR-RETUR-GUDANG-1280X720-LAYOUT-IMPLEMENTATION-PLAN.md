---
Title: Warehouse Desktop Return Order 1280x720 Low-Resolution Layout Adaptation — Implementation Plan
Code: BTR-RETUR-1280X720-LAYOUT
Artifact: IMPLEMENTATION-PLAN
Version: 1.0
LastUpdated: 2026-10-19
Status: COMPLETED
Execution Approval: APPROVED
---

# 1. Objective

Implementation plan to re-engineer the presentation layer of the `BtrReturGudang.Winform` desktop application (`src/j09-btr-retur-gudang`), optimizing the program layout to operate smoothly and ergonomically on warehouse workstations equipped with low-resolution 1280x720 displays.

Planning Mode: FEATURE-PLANNING

Referenced artifacts:

- FEATURE: `docs/features/return-order-desktop/feature.md` (`BTR-RETUR-DESKTOP-001`, v1.1)
- ARCHITECTURE: `docs/work/j09-btr-retur-gudang/BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md` (v1.3, TD-013)

Architecture Applicability: ARCHITECTURE-REQUIRED

---

# 2. Planning Scope

The scope encompasses re-architecting and streamlining the user interface layout across the WinForms presentation layer (`BtrReturGudang.Winform`) and validating with automated unit tests (`BtrReturGudang.Winform.Tests`):

1. **Shell Geometry & Initialization (`MainForm` SCR-01):**
   - Startup window size changed from `1280x800` to `1280x720`; `MinimumSize` reduced from `1024x650` to `1024x600`.
   - Automatic maximization on launch when desktop working area height is `<= 768px`.
   - Compact application header (`_pnlHeader`): height streamlined from 64px to 44px, arranging brand logo, title, Depo badge, Operator badge, and online indicator pill along a single horizontal plane.
   - Sidebar navigation (`_pnlSidebar`): width streamlined from 230px to 200px, button heights adjusted from 46px to 40px, giving ~1064px net horizontal width to the content host.

2. **Core Operational Intake Re-design (`ReturnOrderEntryView` SCR-02):**
   - Eliminate vertical card stacking that consumed 302px and starved the items grid down to ~82px (~1 row).
   - Document Header area: Compact 2-row table (Customer autocomplete, Driver, Salesman, Notes) with height ~58-60px (reduced from 110px).
   - Unified Scan & Quantity Strip: Merge separate Scanner Banner (64px) and Quantity Strip (80px) into a single unified card of height ~54-58px with scanner status and product name on the left and `cboSatuan`, `numQtyBagus`, `numQtyRusak`, `btnAddItem` on the right.
   - Bottom Action Toolbar: Height reduced from 54px to 40-42px. Summary footer height reduced from 46px to 32-34px.
   - Net DataGridView visible vertical space expands from ~82px (1 row) to **~370-400px (10-12 concurrent rows visible)**.
   - Defensive Scrolling: Wrap main intake container with `AutoScroll = true` to prevent control clipping under non-standard DPI scaling (125%).

3. **History & Inspector Streamlining (`ReturnOrderListView` SCR-05):**
   - Top Header height reduced from 68px to 44px; Filter bar height reduced from 52px to 38-40px.
   - Proportional Splitter: Set `SplitterDistance = 520` (out of ~1040px available), allocating ~520px to the Inspector panel (up from ~410px).
   - Inspector Metadata Panel: Replaced fixed-coordinate layout (`X=230`) with responsive 2-column table of height 80-84px (down from 160px), eliminating text clipping.
   - Post-sync locked banner reduced from 54px to 36-38px.
   - Detail items grid height expands from ~104px (1-2 rows) to **~210-230px (6-7 concurrent rows visible)**.

4. **Dialog Viewport Compliance (SCR-03, SCR-04, Dialogs):**
   - `ManualItemSearchDialog` (SCR-03) resized from `720x540` to `740x480`.
   - `QuickBarcodeRegistrationDialog` (SCR-04) resized from `540x560` to `580x480`.
   - `BarcodeRegistrationDialog` resized from `540x560` to `580x480`.
   - Header banners reduced from 68px to 44px. All dialogs guarantee >= 190px vertical clearance relative to 672px working area.

5. **Automated Verification:**
   - Update `BtrReturGudang.Winform.Tests` to verify new dimensions, minimum sizes, layout proportions, and confirm all 132+ tests pass without regression.

Excluded from scope:
- Backend database schema or Dapper queries (remains untouched in `btrgd`).
- Cloud API synchronization contracts or endpoints (remains untouched in `j06-pkl-btrade-api`).
- AppTier business orchestration or domain entities (remains untouched).

---

# 3. Dependencies

### External Dependencies
- .NET Framework 4.7.1 runtime and Windows Forms libraries.
- Standard 1280x720 display with Windows taskbar (~1280x672 working area).

### Slice Dependency Rules
- `Depends On` declares strict implementation prerequisites.
- Dependency satisfaction requires the referenced slice implementation status to be `IMPLEMENTED`.
- Dependency satisfaction does not require review status `GO`.

---

# 4. Progress Summary

| Phase | Description | Slices | Implementation Status | Review Status | Progress |
| :--- | :--- | :---: | :---: | :---: | :---: |
| **P1** | Shell Geometry & Chrome Adaptation | 1 | IMPLEMENTED | GO | 1/1 |
| **P2** | Operational Intake View Re-design | 1 | IMPLEMENTED | GO | 1/1 |
| **P3** | History Inspector & Dialogs Adaptation | 2 | IMPLEMENTED | GO | 2/2 |
| **P4** | Automated UI Tests & Layout Verification | 1 | IMPLEMENTED | GO | 1/1 |
| **Total** | | **5** | **IMPLEMENTED** | **GO** | **5/5** |

---

# 5. Phases

## P1 - Shell Geometry & Chrome Adaptation

Implementation Status: IMPLEMENTED  
Review Status: GO  

### P1-S01: Shell Dimensions, Auto-Maximize, Compact Header & Narrower Sidebar (`MainForm.cs`, `UiTheme.cs`)
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Re-engineer `MainForm` window bounds, startup state, compact 44px header, and 200px sidebar navigation to maximize content area on 1280x720 displays.
- **Depends On:** None
- **Repository:** `d:\Project.Private\btr-platform`
- **Implementation Notes:**
  - `MainForm.cs`:
    - Updated window `Size` to `1280, 720` and `MinimumSize` to `1024, 600`.
    - Added auto-maximize logic in `InitializeComponent()` targeting displays with `WorkingArea.Height <= 768` or `Width <= 1366`.
    - Compacted `_pnlHeader` to 44px height and padding `(16, 0, 16, 0)`.
    - Compacted `pnlBrand` to 300x44px with 36x22px logo badge, 11pt bold title at Point(44, 11), and hidden subtitle.
    - Compacted `pnlRightHeader` to 44px height with padding `(0, 7, 0, 0)`, 8.5pt badges with padding `(8, 4, 8, 4)`, 48x26px "Ganti" button, and 26px online pill with padding `(6, 4, 8, 4)`.
    - Maintained status strip height at 26px.
    - Narrowed `_pnlSidebar` to 200px width with padding `(8, 12, 8, 12)`, nav buttons to 184x40px with top offsets (36, 80, 124, 168), bottom sync container to 80px, and sync button to 38px height.
    - Client content host net dimensions: 1080px width (>= 1064px) and 602px-650px height (>= 590px).
  - `UiTheme.cs`:
    - Updated `FontAppTitle` to `new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point)`.
- **Changed Files:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/MainForm.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Controls/UiTheme.cs`
- **Completion Criteria:**
  - `MainForm.cs`:
    - `Size` changed from `new Size(1280, 800)` to `new Size(1280, 720)`.
    - `MinimumSize` changed from `new Size(1024, 650)` to `new Size(1024, 600)`.
    - Startup logic in `InitializeComponent()` detects if `Screen.PrimaryScreen.WorkingArea.Height <= 768` or `Width <= 1366`, and initializes `WindowState = FormWindowState.Maximized`.
    - `_pnlHeader`: Height reduced from 64px to 44px; padding adjusted to `(16, 0, 16, 0)`.
    - `pnlBrand`: Height 44px, width 300px; logo badge (36x22px, font 8.5pt bold), title label (11pt bold, location `Point(44, 11)`), subtitle hidden or compacted.
    - `pnlRightHeader`: Height 44px, padding `(0, 7, 0, 0)`; Depo and Operator badges with padding `(8, 4, 8, 4)` and font 8.5pt; "Ganti" button height 26px, width 48px; online pill height 26px, padding `(6, 4, 8, 4)`.
    - `_pnlSidebar`: Width reduced from 230px to 200px; padding `(8, 12, 8, 12)`. Nav buttons width 184px, height 40px, top offsets adjusted `(36, 80, 124, 168)`. Bottom sync container height 80px, sync button height 38px.
    - Status strip height maintained at 24-26px.
  - Net client content host dimensions: Width >= 1064px, Height >= 590px when maximized under 1280x720.

---

## P2 - Operational Intake View Re-design

Implementation Status: IMPLEMENTED  
Review Status: GO  

### P2-S02: Compact Customer Header, Unified Horizontal Scan-Quantity Strip & Grid Expansion (`ReturnOrderEntryView.cs`)
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Overhaul `ReturnOrderEntryView` (SCR-02) layout, replacing stacked cards with a compact 2-row document header and a unified horizontal Scan & Quantity strip, expanding `_dgvItems` visible vertical height from ~82px (1 row) to ~380px (10-12 rows).
- **Depends On:** P1-S01
- **Repository:** `d:\Project.Private\btr-platform`
- **Implementation Notes:**
  - `ReturnOrderEntryView.cs`:
    - `InitializeComponent()`:
      - Adjusted `tblMain.Padding` to `(12, 6, 12, 6)` and enabled `tblMain.AutoScroll = true` to guard against clipping under non-standard DPI scaling.
    - `BuildHeaderArea()`:
      - Removed large card title header panel ("📄 INFORMASI DOKUMEN & CUSTOMER").
      - Streamlined card padding to `(12, 6, 12, 8)` and bottom margin to `(0, 0, 0, 6)`.
      - Rebuilt into a 2-row TableLayoutPanel of height 58px:
        - Row 0: Customer label (85px) + autocomplete ComboBox (fill, 50%), Driver label (100px) + ComboBox (fill, 50%).
        - Row 1: Salesman label (85px) + ComboBox (fill, 50%), Catatan label (100px) + TextBox (fill, 50%).
    - `BuildIntakeCard()`:
      - Merged stacked scanner banner (64px) and quantity strip (80px) into a single unified horizontal card of height 56px, padding `(10, 6, 10, 6)`, and margin `(0, 0, 0, 6)`.
      - Left section (Percent 42%): `_pnlScannerBanner` with compact pill status badge (`_lblBannerStatus`), scanned item name (`_lblBannerTitle`, font 9.5pt bold), and barcode details (`_lblBannerDetails`, font Consolas 8.5pt).
      - Right section (Percent 58%): Satuan ComboBox (85px), Qty Bagus NumericUpDown (95px, font 10.5pt bold, Emerald), Qty Rusak NumericUpDown (95px, font 10.5pt bold, Rose), Item Notes TextBox (fill / ~110px), and `+ Tambah [Enter]` button (115px, height 30px).
      - Hands-free USB scanner capture and keyboard focus behavior (`Enter` commit) strictly preserved.
    - `BuildGridArea()`:
      - Streamlined card padding to `(8, 6, 8, 6)` and section title height to 20px.
      - Compacted summary footer `BuildSummaryFooter()` to height 32px, padding `(12, 6, 12, 6)`, and font 9pt bold.
    - `BuildActionToolbar()`:
      - Streamlined toolbar height from 54px to 40px, padding `(12, 4, 12, 4)`, and action button heights to 32px.
    - Net vertical space allocated to `_dgvItems` in standard 720p maximized mode is 360px+ (accommodating 12+ rows concurrently without vertical scrolling, well exceeding the 10-row criterion).
  - `BtrReturGudang.Winform.Tests`:
    - Added `ReturnOrderEntryView_LayoutDimensions_ConformTo720pCompactSpecifications` asserting compact control dimensions, padding, margins, autoscroll, and >= 10 rows capacity.
- **Changed Files:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/ReturnOrderEntryView.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform.Tests/ReturnOrderEntryViewTests.cs`
- **Completion Criteria:**
  - `ReturnOrderEntryView.cs`:
    - `BuildHeaderArea()`:
      - Remove large card title panel ("📄 INFORMASI DOKUMEN & CUSTOMER").
      - Card padding reduced to `(12, 6, 12, 8)` and margin to `(0, 0, 0, 6)`.
      - 2-row TableLayoutPanel:
        - Row 0: Customer Label (85px) + ComboBox (fill), Driver Label (100px) + ComboBox (fill).
        - Row 1: Salesman Label (85px) + ComboBox (fill), Catatan Label (100px) + TextBox (fill).
      - Total header area height: ~58-60px (reduced from 110px).
    - `BuildIntakeCard()`:
      - Replace separate Scanner Banner (64px) and Quantity Strip (80px) with a single unified card of height ~54-58px, padding `(10, 6, 10, 6)`, margin `(0, 0, 0, 6)`.
      - Single-card TableLayoutPanel:
        - Left section (Percent 42%): Scanned status badge (`_lblBannerStatus`, compact pill) + Scanned item name (`_lblBannerTitle`, font 9.5pt bold) + barcode details (`_lblBannerDetails`, font Consolas 8.5pt).
        - Right section (Percent 58%): Satuan ComboBox (width 85px), Qty Bagus NumericUpDown (width 95px, Emerald, font 10.5pt bold), Qty Rusak NumericUpDown (width 95px, Rose, font 10.5pt bold), Item Notes TextBox (fill / width 110px), `+ Tambah [Enter]` button (width 115px, height 30px).
      - Hands-free scanner hook and keyboard focus behavior (`Enter` commit) strictly preserved.
    - `BuildGridArea()`:
      - Card padding reduced to `(8, 6, 8, 6)`. Title height 20px.
      - Summary footer `BuildSummaryFooter()`: Height reduced from 46px to 32px, padding `(12, 6, 12, 6)`, font 9pt bold.
    - `BuildActionToolbar()`:
      - Height reduced from 54px to 40px, padding `(12, 4, 12, 4)`; action buttons height 32px.
    - Root layout container: `tblMain.AutoScroll = true` ensuring zero clipping under any scaling or resizing.
    - Observable result: In standard 720p maximized mode, `_dgvItems` displays at least 10 data rows concurrently without vertical scrolling.

---

## P3 - History Inspector & Dialogs Adaptation

Implementation Status: IMPLEMENTED  
Review Status: GO  

### P3-S03: Inspector Metadata Panel Compact Layout & Balanced Splitter (`ReturnOrderListView.cs`)
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Streamline `ReturnOrderListView` (SCR-05) top chrome, balance splitter proportion, and redesign inspector metadata into a compact 2-row table, eliminating text truncation and expanding detail grid height.
- **Depends On:** P1-S01
- **Repository:** `d:\Project.Private\btr-platform`
- **Changed Files:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/ReturnOrderListView.cs`
- **Implementation Notes:**
  - Streamlined `BuildHeader()`: Height reduced from 68px to 44px, padding `(16, 6, 16, 6)`. Title font set to 11.5pt bold (`Point(16, 4)`), subtitle font to 8pt (`Point(16, 24)`). Refresh button height reduced to 28px and width 105px with `AnchorStyles.Top | AnchorStyles.Right`.
  - Streamlined `BuildFilterBar()`: Height reduced from 52px to 38px, padding `(16, 4, 16, 4)`. Filter controls height standardized to 26px, filter button height to 26px and width 110px.
  - Balanced `_splitBody`: Explicitly initialized with `Width = 1080` and `SplitterDistance = 520` (allocating ~520px to orders grid card and ~520px to inspector panel). Tightened panel padding to `(16, 10, 4, 12)` and `(4, 10, 16, 12)`.
  - Re-engineered `BuildInspectorPanel()`:
    - Reduced panel padding from 16px to `(12, 8, 12, 8)`.
    - Inspector title height reduced from 32px to 24px (`FontCardTitle`).
    - Post-Sync locked banner (`_pnlLockedBanner`) height reduced from 54px to 36px, font 8.5pt bold, vertically centered.
    - Metadata panel: Replaced fixed-coordinate layout (`X=230`) with a responsive `TableLayoutPanel` (4 columns: Key1 65px absolute, Val1 50% percent, Key2 65px absolute, Val2 50% percent; 5 rows at 20% percent). Total height reduced from 160px to 80px. All value labels configured with `AutoEllipsis = true` and `DockStyle.Fill`, completely eliminating text truncation. Note label spans columns 1 to 3.
    - Detail items grid header: Height reduced from 28px to 22px.
    - Action panel `BuildInspectorActions()`: Height reduced from 52px to 38px, padding `(8, 5, 8, 5)`, action button heights reduced from 34px to 28px.
    - Detail items grid `_dgvItems`: Row template height set to 28px, column headers height to 28px. Vertical height available expands to ~328px (up to 11-12 concurrent visible rows, far exceeding the 220px / 6-7 rows requirement).
  - All existing unit tests in `ReturnOrderListViewTests` passed cleanly (5/5).
- **Completion Criteria:**
  - `ReturnOrderListView.cs`:
    - `BuildHeader()`: Height reduced from 68px to 44px, padding `(16, 6, 16, 6)`. Title font 11.5pt bold, subtitle font 8pt. Refresh button height 28px, width 105px.
    - `BuildFilterBar()`: Height reduced from 52px to 38px, padding `(16, 4, 16, 4)`. Controls height 26px, filter button height 26px.
    - `_splitBody`: Set `SplitterDistance = 520` (allocating ~520px to orders grid and ~520px to inspector panel).
    - `BuildInspectorPanel()`:
      - Title height: 24px.
      - Locked banner (`_pnlLockedBanner`): Height reduced from 54px to 36px, font 8.5pt.
      - Metadata panel: Replaced fixed point coordinates (`X=230`) with a responsive TableLayoutPanel (4 columns: Key1 65px, Val1 fill, Key2 65px, Val2 fill). Total height reduced from 160px to 80px.
      - Detail items grid header: Height 22px.
      - Action panel `BuildInspectorActions()`: Height reduced from 52px to 38px, button heights 28px.
      - Detail items grid `_dgvItems`: Vertical height expands from ~104px to **~220px (6-7 visible rows)**.

---

### P3-S04: Modal Dialog Dimension & Viewport Bounds Adaptation (`ManualItemSearchDialog`, `QuickBarcodeRegistrationDialog`, `BarcodeRegistrationDialog`)
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Re-scale modal dialogs to guarantee they fit within 720p screens (< 500px height) with comfortable vertical safety margins.
- **Depends On:** None
- **Repository:** `d:\Project.Private\btr-platform`
- **Changed Files:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/ManualItemSearchDialog.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/QuickBarcodeRegistrationDialog.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/BarcodeRegistrationDialog.cs`
- **Implementation Notes:**
  - Resized `ManualItemSearchDialog` (SCR-03) `ClientSize` from `(720, 540)` to `(740, 480)` and streamlined header banner height from 68px to 44px. Adjusted search textbox width to 490px and bottom action buttons ("Pilih", "Batal") to guarantee complete visibility.
  - Resized `QuickBarcodeRegistrationDialog` (SCR-04) `ClientSize` from `(540, 560)` to `(580, 480)` and streamlined warning header height from 68px to 44px. Compacted internal controls and action buttons ("Simpan & Gunakan", "Batal") within 346px vertical space, ensuring all controls are fully accessible and never clipped.
  - Resized `BarcodeRegistrationDialog` `ClientSize` from `(540, 560)` to `(580, 480)` and streamlined header height from 68px to 44px. Aligned controls and action buttons within 480px total height.
  - Verified all dialogs maintain >= 190px clearance from the bottom of a 1280x720 display (~672px working area).
- **Completion Criteria:**
  - `ManualItemSearchDialog.cs` (SCR-03):
    - `ClientSize` changed from `new Size(720, 540)` to `new Size(740, 480)`.
    - Header height reduced from 68px to 44px.
    - Grid and search box properly fill available space without pushing bottom action buttons off-screen.
  - `QuickBarcodeRegistrationDialog.cs` (SCR-04):
    - `ClientSize` changed from `new Size(540, 560)` to `new Size(580, 480)`.
    - Header height reduced from 68px to 44px.
    - Item search box, listbox, and unit selector fit within 480px with action buttons fully visible.
  - `BarcodeRegistrationDialog.cs`:
    - `ClientSize` changed from `new Size(540, 560)` to `new Size(580, 480)`.
    - Header height reduced from 68px to 44px.
  - All dialogs maintain >= 190px clearance from bottom of 1280x720 display, ensuring title bar and action buttons are always accessible.

---

## P4 - Automated UI Tests & Layout Verification

Implementation Status: IMPLEMENTED  
Review Status: GO  

### P4-S05: Automated UI Tests & Layout Verification (`BtrReturGudang.Winform.Tests`)
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Update and expand unit tests in `BtrReturGudang.Winform.Tests` to verify all 1280x720 layout constraints and ensure 0 regressions.
- **Depends On:** P1-S01, P2-S02, P3-S03, P3-S04
- **Repository:** `d:\Project.Private\btr-platform`
- **Implementation Notes:**
  - `MainFormTests.cs`:
    - Updated legacy test `MainForm_LayoutBounds_AreCorrectWithoutOverlap` asserting compact 44px header, 200px sidebar, 1280x720 dimensions, and content host net bounds.
    - Added `MainForm_WindowBounds_ConformTo720pSpecifications` verifying initial `Size` is `(1280, 720)` and `MinimumSize` is `(1024, 600)`.
  - `ReturnOrderListView.cs` & `ReturnOrderListViewTests.cs`:
    - Exposed typed test accessors `BodySplitter` and `MetadataPanel`.
    - Added `ReturnOrderListView_SplitterAndLayoutDefaults_ConformTo720pSpecifications` asserting splitter distance (520px), splitter width (6px), panel paddings, 44px header, and 38px filter bar.
    - Added `ReturnOrderListView_MetadataPanel_ResponsiveLayoutConformsToCompactSpecifications` asserting 80px height, 4-column 5-row responsive grid (65px absolute, 50% percent), and AutoEllipsis on value labels.
  - `ModalDialogTests.cs`:
    - Added `ModalDialogs_ClientSizes_ConformToLowResolutionConstraint` asserting `ManualItemSearchDialog` (`740x480`), `QuickBarcodeRegistrationDialog` (`580x480`), and `BarcodeRegistrationDialog` (`580x480`) all have ClientSize height `<= 480px` and compact 44px headers.
  - `AssemblyInfo.cs`:
    - Added `[assembly: CollectionBehavior(DisableTestParallelization = true)]` to `BtrReturGudang.Winform.Tests` to eliminate race conditions on shared Windows Registry (`HKCU`) keys and static audio state during automated test execution.
  - Verification:
    - All 163 unit and UI tests across the entire solution passed cleanly with 0 errors (`Domain.Tests`: 26, `AppTier.Tests`: 16, `Infrastructure.Tests`: 46, `Winform.Tests`: 75).
- **Changed Files:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/ReturnOrderListView.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform.Tests/MainFormTests.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform.Tests/ReturnOrderListViewTests.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform.Tests/ModalDialogTests.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform.Tests/AssemblyInfo.cs`
- **Completion Criteria:**
  - Tests in `BtrReturGudang.Winform.Tests` updated and added:
    - Verify `MainForm` initial `Size` is `(1280, 720)` and `MinimumSize` is `(1024, 600)`.
    - Verify `ReturnOrderEntryView` header, intake strip, and action toolbar heights conform to low-resolution specifications.
    - Verify `ReturnOrderListView` splitter defaults and metadata panel responsiveness.
    - Verify dialog `ClientSize` heights are `<= 480px`.
  - All unit tests in the solution (`AppTier.Tests`, `Infrastructure.Tests`, `Winform.Tests`) compile and pass with 0 errors.

---

# 6. Change Log

- 2026-10-19: Initial plan creation for 1280x720 low-resolution layout re-engineering.
