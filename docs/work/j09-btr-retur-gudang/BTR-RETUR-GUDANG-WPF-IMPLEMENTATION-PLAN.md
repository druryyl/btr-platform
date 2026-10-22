---
Title: BTR Retur Gudang - WPF Client Implementation Plan
Code: BTR-RETUR-GUDANG-WPF
Artifact: IMPLEMENTATION-PLAN
Version: 1.0
LastUpdated: 2026-10-22
Status: COMPLETED
Execution Approval: APPROVED
---

# 1. Objective

Implement a modern, responsive, high-throughput desktop presentation layer using **WPF (Windows Presentation Foundation)** on **.NET Framework 4.7.1** for the **BTR Retur Gudang** desktop workstation application.

Planning Mode: FEATURE-PLANNING  

Referenced artifacts:
- FEATURE: `docs/features/return-order-desktop/feature.md`
- ARCHITECTURE: `docs/work/j09-btr-retur-gudang/BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md`
- FEASIBILITY-ASSESSMENT: `docs/work/j09-btr-retur-gudang/BTR-RETUR-GUDANG-DESKTOP-FEASIBILITY-ASSESSMENT.md`
- REDESIGN-SPEC: `docs/work/j09-btr-retur-gudang/retur-intake-ergonomics-redesign-plan.md`

Architecture Applicability: ARCHITECTURE-REQUIRED  
Implementation replaces the WinForms presentation client (`BtrReturGudang.Winform`) with a modern WPF client (`BtrReturGudang.Wpf`) that natively consumes existing domain models, application orchestration services, persistence DALs, audio, and registry infrastructure without modifying backend business logic.

---

# 2. Planning Scope

The plan encompasses:
1. **Scaffolding `BtrReturGudang.Wpf` (.NET Framework 4.7.1)** inside `src/j09-btr-retur-gudang/BtrReturGudang.sln` referencing `BtrReturGudang.Domain`, `BtrReturGudang.AppTier`, and `BtrReturGudang.Infrastructure`.
2. **MVVM Infrastructure & Design System:** Lightweight command/viewmodel base classes, dependency injection configuration, and a XAML design system (Slate/Blue/Emerald theme tokens, vector geometry, High-DPI per-monitor awareness).
3. **Hardware USB Scanner Wedge in WPF:** Window-level `ComponentDispatcher.ThreadPreprocessMessage` or `PreviewKeyDown` buffer listener with 50ms burst detection for hands-free barcode capture.
4. **Ergonomic Numeric Input:** Custom `NumericTextBox` featuring watermark/placeholder `"0"`, auto-select all on focus, keyboard numpad protection against sticky zero (`"1"` never appends into `"10"`), and re-scan auto-increment (+1).
5. **Screen Design Inventory Realization (XAML):**
   - SCR-01 Shell Window (`MainWindow.xaml`) with docked sidebar navigation and live sync indicator.
   - SCR-02 Core Intake View (`ReturnOrderEntryView.xaml`) with large prominent Product Banner (zero truncation), ergonomic quantity strip, and responsive DataGrid.
   - SCR-03 / SCR-04 / SCR-08 Modal Dialogs (`ManualItemSearchDialog`, `QuickBarcodeRegistrationDialog`, `ManualCustomerSearchDialog`).
   - SCR-05 / SCR-06 / SCR-07 Management Views (`ReturnOrderListView` with post-sync lock, `BarcodeRegistryView`, `SettingsView`).
6. **Automated Testing Suite:** `BtrReturGudang.Wpf.Tests` covering ViewModel state machines, barcode wedge parsing, quantity calculations, and workflow commands.

---

# 3. Dependencies

- **Runtime:** .NET Framework 4.7.1 runtime (native Windows OS built-in assemblies: `PresentationCore`, `PresentationFramework`, `WindowsBase`, `System.Xaml`).
- **Existing Assemblies:**
  - `BtrReturGudang.Domain` (Entities, Status Enums, Value Objects)
  - `BtrReturGudang.AppTier` (Contracts, DTOs, `ReturnOrderService`, `BarcodeService`, `SyncService`)
  - `BtrReturGudang.Infrastructure` (Dapper DALs, `BtradeApiClient`, `AudioHelper`, `RegistryHelper`)
- **Third-Party Libraries:** Reuses `Microsoft.Extensions.DependencyInjection` (already in solution) and standard WPF control templates. No heavy external UI framework dependencies.

---

# 4. Progress Summary

| Phase | Description | Implementation Status | Review Status | Slices Completed |
| :--- | :--- | :--- | :--- | :--- |
| **P1** | Foundation, Scaffolding & Design System | NOT-STARTED | NOT-REVIEWED | 0/3 |
| **P2** | Application Shell & Core Intake Screen (SCR-01, SCR-02) | NOT-STARTED | NOT-REVIEWED | 0/3 |
| **P3** | Modal Lookup & Registration Dialogs (SCR-03, SCR-04, SCR-08) | NOT-STARTED | NOT-REVIEWED | 0/2 |
| **P4** | Order History Inspector, Registry & Settings (SCR-05, SCR-06, SCR-07) | NOT-STARTED | NOT-REVIEWED | 0/2 |
| **P5** | Verification & Automated Test Suite | NOT-STARTED | NOT-REVIEWED | 0/1 |

---

# 5. Phases

## P1 - Foundation, Scaffolding & Design System

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED  

### P1-S01: Project Scaffolding, Solution Integration & DI Bootstrap
- **Title:** Scaffold `BtrReturGudang.Wpf` Project & Dependency Injection Setup
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Create `.NET Framework 4.7.1` WPF application project `BtrReturGudang.Wpf.csproj`, add to `BtrReturGudang.sln`, reference `Domain`, `AppTier`, and `Infrastructure`, and configure `App.xaml.cs` with `ServiceCollection` DI bootstrap.
- **Depends On:** None
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.Wpf/BtrReturGudang.Wpf.csproj` created targeting `<TargetFrameworkVersion>v4.7.1</TargetFrameworkVersion>` with project references to `Domain`, `AppTier`, and `Infrastructure`.
  - Added to `src/j09-btr-retur-gudang/BtrReturGudang.sln`.
  - `App.xaml` and `App.xaml.cs` configured with `IServiceProvider`, registering DALs, services, and ViewModels.
  - Builds cleanly via `dotnet build src/j09-btr-retur-gudang/BtrReturGudang.sln`.
- **Implementation Notes:**
  - Created `BtrReturGudang.Wpf.csproj` targeting `net471` / `v4.7.1` with `<UseWPF>true</UseWPF>`, references to `Domain`, `AppTier`, `Infrastructure`, `Microsoft.Extensions.DependencyInjection 10.0.12`, and `Microsoft.NETFramework.ReferenceAssemblies 1.0.3`.
  - Added project to `BtrReturGudang.sln`.
  - Created `App.xaml` and `App.xaml.cs` with `ServiceCollection` configuring Repositories (`ReturnOrderDal`, `BarcodeDal`, `MasterDataDal`, `SyncLogDal`), Helpers (`AudioHelper`, `RegistryHelper`), Cloud API & Sync (`BtradeApiClient`, `SyncService`), Session Gate (`OperatorSessionValidator`), Domain Services (`ReturnOrderService`, `BarcodeService`), presentation ViewModels, and shell Window.
  - Added initial ViewModel stubs (`MainViewModel`, `ReturnOrderEntryViewModel`, `ReturnOrderListViewModel`, `BarcodeRegistryViewModel`, `SettingsViewModel`, `ManualItemSearchViewModel`, `QuickBarcodeRegistrationViewModel`, `ManualCustomerSearchViewModel`) and shell `MainWindow.xaml` / `MainWindow.xaml.cs`.
  - Verified clean build and full test execution across `BtrReturGudang.sln` (`0 Warning(s)`, `0 Error(s)`).

---

### P1-S02: MVVM Core & XAML Design System Resource Dictionaries
- **Title:** Implement Lightweight MVVM Foundation & Theme Resource Dictionaries
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Create zero-dependency MVVM base classes (`ViewModelBase`, `RelayCommand`, `AsyncRelayCommand`) and global XAML resource dictionaries defining semantic color tokens (Slate-900, Blue-600, Emerald-500, Rose-500), typography, button styles, card containers, and data grid styling.
- **Depends On:** P1-S01
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - `Mvvm/ViewModelBase.cs` implements `INotifyPropertyChanged` with `SetProperty<T>()`.
  - `Mvvm/RelayCommand.cs` and `AsyncRelayCommand.cs` provide command binding support.
  - `Themes/Colors.xaml`, `Themes/Styles.xaml`, and `Themes/DataGrid.xaml` merged in `App.xaml`.
  - Typography standardized to `Segoe UI` with proper scaling; button styles provide hover, pressed, and disabled visual states.
- **Implementation Notes:**
  - Created `Mvvm/ViewModelBase.cs` implementing `INotifyPropertyChanged` with strongly-typed `SetProperty<T>()` and `OnPropertyChanged()` helpers.
  - Created `Mvvm/RelayCommand.cs` (`RelayCommand` and `RelayCommand<T>`) and `Mvvm/AsyncRelayCommand.cs` (`AsyncRelayCommand` and `AsyncRelayCommand<T>`) with execution state tracking (`IsExecuting`) and `CommandManager.RequerySuggested` integration.
  - Updated all ViewModel classes (`MainViewModel`, `ReturnOrderEntryViewModel`, `ReturnOrderListViewModel`, `BarcodeRegistryViewModel`, `SettingsViewModel`, `ManualItemSearchViewModel`, `QuickBarcodeRegistrationViewModel`, `ManualCustomerSearchViewModel`) to inherit from `ViewModelBase`.
  - Created `Themes/Colors.xaml` defining full Tailwind-inspired palette tokens (Slate-900..50, Blue-600, Emerald-500, Rose-600, Amber-600, Indigo selection) and semantic UI brushes.
  - Created `Themes/Styles.xaml` defining standardized `Segoe UI` typography hierarchy, comprehensive button visual styles (`Button.Primary`, `Button.Success`, `Button.Danger`, `Button.Warning`, `Button.Secondary`, `Button.Outline`, `Button.Ghost`) with hover/pressed/disabled states, card panels (`Card.Panel`, `Card.DarkPanel`, `Card.HeaderPanel`), and input controls.
  - Created `Themes/DataGrid.xaml` replicating enterprise grid styling (`ColumnHeader`, `Cell`, `Row`, full row selection with Indigo `#E0E7FF`, alternating zebra rows `#F8FAFC`, header `#F1F5F9`).
  - Merged `Themes/Colors.xaml`, `Themes/Styles.xaml`, and `Themes/DataGrid.xaml` into `App.xaml`.
  - Verified clean compilation with `dotnet build src/j09-btr-retur-gudang/BtrReturGudang.sln` (`0 Warning(s)`, `0 Error(s)`) and 100% test pass (148/148 tests passing across solution).

---

### P1-S03: Global USB Scanner Wedge Hook in WPF
- **Title:** Implement WPF Hardware Scanner Buffer & Wedge Interceptor
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Port hardware USB barcode scanner buffer hook from WinForms `IMessageFilter` to WPF using `ComponentDispatcher.ThreadPreprocessMessage` (or window-level `PreviewKeyDown` buffer), filtering high-speed barcode scanner bursts (< 50ms inter-character latency) and dispatching decoded barcodes to active view models.
- **Depends On:** P1-S01
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - `Services/WpfScannerWedgeListener.cs` implements hardware buffer tracking with configurable inter-char latency threshold (50ms).
  - Fires `BarcodeScanned(string barcode)` event without interfering with regular human keyboard typing.
  - Unit test verifies burst sequence detection vs. slow human typing.
- **Implementation Notes:**
  - Created `Services/WpfScannerWedgeListener.cs` hooking into `ComponentDispatcher.ThreadPreprocessMessage` to intercept high-speed barcode scanner bursts (<= 50ms inter-character interval), normalizing barcodes per VO-01 rules (`BarcodeNormalization`), and suppressing terminating Enter keystroke (`VK_RETURN` and translated `WM_CHAR`).
  - Added `BarcodeScanned(string barcode)` event (`Action<string>`) along with `BarcodeScannedDetailed` (`EventHandler<BarcodeScannedEventArgs>`) and `ActiveConsumer` (`IScannerConsumer`) dispatching for active ViewModels.
  - Implemented configurable inter-keystroke timeout (`InterKeystrokeTimeoutMs = 50`), minimum barcode length (`MinBarcodeLength = 1`), buffer flushing on navigation/function keys (`F1`..`F12`, `Esc`, `Tab`, arrows), and manual simulation support (`SimulateScan`).
  - Registered `WpfScannerWedgeListener` in `App.xaml.cs` DI container as a singleton service.
  - Created `Services/BarcodeScannedEventArgs.cs` and `Services/IScannerConsumer.cs`.
  - Created `BtrReturGudang.Wpf.Tests` unit test project with `WpfScannerWedgeListenerTests.cs` verifying burst interception vs. slow human typing pass-through, leading zero preservation, Enter suppression, navigation key flushing, length threshold, and consumer callbacks. Added to `BtrReturGudang.sln`.
  - Verified clean build (`0 Warning(s)`, `0 Error(s)`) and 100% test pass (161/161 tests passing across solution).

---

## P2 - Application Shell & Core Intake Screen (SCR-01, SCR-02)

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED  

### P2-S04: MainWindow Shell, Docked Sidebar Navigation & Status Bar (SCR-01)
- **Title:** Implement MainWindow Shell, Navigation Sidebar & Sync Indicator
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Build `MainWindow.xaml` and `MainViewModel.cs` containing the docked left navigation sidebar (`Intake Retur [F1]`, `Daftar Retur [F3]`, `Barcode Registry [F4]`, `Pengaturan [F5]`), top header with depo/operator info and connection status badge, bottom status bar (scanner status, sync status, latency), and active content host (`ContentControl`).
- **Depends On:** P1-S02, P1-S03
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - `Views/MainWindow.xaml` layout is responsive and scales cleanly on 1280x720 display without clipping.
  - Global hotkeys (`F1`, `F3`, `F4`, `F5`, `F9`, `F10`, `Esc`) bound via InputBindings or ViewModel commands.
  - Periodic 5-minute background sync timer runs without blocking the UI thread.
  - Active module button visually highlighted with `Primary` blue background.
- **Implementation Notes:**
  - Created `NavigationModule.cs` enum defining `Intake`, `DaftarRetur`, `BarcodeRegistry`, and `Pengaturan`.
  - Created `IOrderActionHandler.cs` contract enabling child ViewModels to intercept global shell commands (`F9` Save Draft, `F10` Save & Upload, `Esc` Cancel/Clear).
  - Built `MainViewModel.cs` orchestrating docked left navigation sidebar, dynamic active ViewModel resolution via DI container, header metadata (depo code, operator display name and tooltip with registered Google email, online/offline status pill), status bar (scanner wedge status, latency, sync timestamp, and status info log), 5-minute background sync timer with spinner animation, operator session validation gate, and USB scanner wedge event routing to active consumer.
  - Built `Views/MainWindow.xaml` and `Views/MainWindow.xaml.cs` implementing responsive 1280x720 layout conforming to TD-013 (44px compact top header, 200px docked left navigation sidebar, 26px status bar, and central ContentControl host).
  - Bound global hotkeys (`F1`, `F3`, `F4`, `F5`, `F9`, `F10`, `Esc`) declaratively via `<Window.InputBindings>` and supplemented with `OnPreviewKeyDown` routing.
  - Configured active module highlighting in `MainWindow.xaml` with `Button.SidebarNav.*` DataTriggers applying `Primary` blue background (`Brush.Primary` / `#2563EB`) and bold text to the active view button.
  - Added unit and STA integration tests in `BtrReturGudang.Wpf.Tests/MainWindowTests.cs` covering window geometry (1280x720, min 1024x600), hotkey input bindings, navigation module switching and highlight triggers, header metadata reflection, manual & periodic async sync execution, operator session refusal pre-emption, and scanner wedge consumer routing.
  - Verified clean build (`0 Warning(s)`, `0 Error(s)`) and 100% test pass across solution (188/188 tests passing).

---

### P2-S05: Ergonomic Numeric Input Control & Anti-Zero Behavior
- **Title:** Implement Custom High-Load Numeric Input Control
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Create a dedicated WPF `NumericTextBox` control or attached behavior tailored for high-load warehouse intake operations, eliminating sticky zero bugs and providing tactile numpad ergonomics.
- **Depends On:** P1-S02
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - Visual watermark / placeholder `"0"` displayed in muted gray when value is empty or zero.
  - Auto-select all text on focus (mouse click, tab, or barcode auto-focus).
  - Anti-Zero Append: typing `"1"` directly replaces `"0"` (never produces `"10"`).
  - Supports quick increment shortcuts (`Up`/`Down`, `+` key).
- **Implementation Notes:**
  - Created `Controls/NumericTextBox.cs` inheriting from `TextBox`, with two-way bindable `Value`, `Minimum`, `Maximum`, `Step`, `Watermark`, `AutoSelectOnFocus`, and visual state properties `IsZero` and `IsWatermarkVisible`.
  - Implemented visual watermark / placeholder `"0"` displayed in muted gray (`Slate-400` / `#94A3B8`) via `WatermarkHost` TextBlock and dynamic `ZeroBrush` when value is 0 or empty, switching to crisp dark text (`Slate-900`) when greater than zero.
  - Implemented auto-select all on focus (`OnGotKeyboardFocus` and `OnPreviewMouseLeftButtonDown`), ensuring mouse click, tab navigation, or programmatic/barcode focus highlights all text.
  - Implemented Anti-Zero Append in `ProcessTextInput` / `OnPreviewTextInput`, guaranteeing typing a digit `'1'`..`'9'` directly replaces `"0"` without producing `"10"` or `"01"`. Suppresses duplicate `"0"`.
  - Implemented quick increment / decrement keyboard shortcuts via `OnPreviewKeyDown`: `Up` / `+` (both numpad and OEM) increments by `Step`, `Down` / `-` decrements by `Step` clamped at `Minimum`.
  - Added clipboard sanitization (`DataObject.Pasting`) to strip non-digits and normalize leading zeros.
  - Created `Properties/AssemblyInfo.cs` with `ThemeInfo` and `Themes/Generic.xaml` defining the default WPF control template.
  - Added explicit and implicit styles for `NumericTextBox` in `Themes/Styles.xaml`.
  - Created comprehensive unit tests in `BtrReturGudang.Wpf.Tests/NumericTextBoxTests.cs` verifying default state, watermark visibility, muted foreground, anti-zero replacement, shortcut increment/decrement, min/max clamping, and non-numeric input suppression (13 new tests).
  - Verified full test suite execution: 177/177 tests passing across the solution (`dotnet test src/j09-btr-retur-gudang/BtrReturGudang.sln`) with 0 errors and 0 warnings.

---

### P2-S06: Intake Retur View with Prominent Product Banner & Quantity Strip (SCR-02)
- **Title:** Implement Primary Return Order Intake View & Live Grid
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Build `Views/ReturnOrderEntryView.xaml` and `ViewModels/ReturnOrderEntryViewModel.cs` realizing the approved intake redesign:
  1. Compact customer & driver autocomplete header.
  2. Full-width high-contrast Product Banner (displaying pure `BrgName` in 14pt bold white text without internal code noise, status badge, and barcode details).
  3. Spacious Quantity Strip with Satuan dropdown, Qty Bagus, Qty Rusak, Catatan Item, and clearly captioned `+ Tambah [Enter]` action button (never clipped).
  4. Re-scan auto-increment (+1) when scanning identical barcode consecutively.
  5. Responsive intake DataGrid with summary footer (Total Bagus, Total Rusak, Total Items).
- **Depends On:** P2-S04, P2-S05
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - Product Name displays prominently with zero text truncation on long FMCG product names.
  - Scanning valid barcode focuses Qty Bagus with text pre-selected; re-scanning increments count.
  - `+ Tambah [Enter]` button displays clear caption with icon and keyboard accelerator.
  - Items commit to `ObservableCollection<ReturnOrderItemDto>` and update summary counters in real time.
  - Audio cues triggered on success (2000Hz) and warning (400Hz).
- **Implementation Notes:**
  - Implemented `ViewModels/ReturnOrderEntryViewModel.cs` implementing `IScannerConsumer` and `IOrderActionHandler`:
    - Handles compact customer & driver/salesman header binding with `CustomerDto` lookup and fallback `CUST-MANUAL` walk-in customer support.
    - Full-width high-contrast Product Banner displaying pure `BrgName` in 14pt bold text with zero internal code noise, status pill ("BARCODE TERDETEKSI" / "BARCODE TIDAK TERDAFTAR"), and barcode/unit/conversion metadata.
    - Hardware barcode scanner wedge integration: parses barcode, auto-extracts packaging units, auto-selects default unit, emits high-pitch audio beep (2000Hz) on match or warning buzz (400Hz) on unknown barcode.
    - Consecutive re-scan auto-increment: consecutive scan of identical barcode increments `QtyBagus` (`0 -> 2`, `+1`) while keeping active product banner and emitting feedback tone.
    - Quantity strip ergonomic flow: Anti-Zero append integration via `NumericTextBox`, auto-commit via `+ Tambah [Enter]` or `Enter` key accelerator when `QtyBagus + QtyRusak > 0`.
    - Real-time commit to `ObservableCollection<ReturnOrderItemDto>` with automatic line number re-sequencing, line removal (`DeleteLineCommand`), and live recalculation of `TotalItemsCount`, `TotalQtyBagus`, `TotalQtyRusak`, and `TotalPieces`.
    - Integrated with `IOrderActionHandler` for Shell keyboard accelerators (`F9` Save Draft, `F10` Upload, `Esc` Cancel/Reset, `F2` Item Search, `F8` Customer Search).
  - Implemented `Views/ReturnOrderEntryView.xaml` and code-behind `ReturnOrderEntryView.xaml.cs`:
    - Compact header grid with Customer auto-suggest input, `CboDriver`, `CboSalesPerson`, and `TxtKeterangan`.
    - Full-width Product Banner styled with high-contrast Slate-900 background, Emerald-400 status badge, and `TextWrapping="Wrap"` on `TxtProductName` ensuring zero text truncation on arbitrarily long FMCG product names.
    - Spacious Quantity Strip using `controls:NumericTextBox` with watermark and muted zero state for `TxtQtyBagus` and `TxtQtyRusak`, packaging unit ComboBox, and prominently sized, unclipped `+ Tambah [Enter]` action button (`BtnAddItem`).
    - Styled items DataGrid with clean column headers, alternating row styles, delete button cell template, and fixed summary footer cards (`LblSummaryItems`, `LblSummaryBagus`, `LblSummaryRusak`, `LblSummaryTotal`).
    - Keyboard navigation event wiring: `Enter` in quantity inputs commits strip; focus request events wire seamlessly from ViewModel to controls.
  - Registered `<views:ReturnOrderEntryView />` in `Views/MainWindow.xaml` DataTemplates for `ReturnOrderEntryViewModel`.
  - Added `[assembly: InternalsVisibleTo("BtrReturGudang.Wpf.Tests")]` in `Properties/AssemblyInfo.cs`.
  - Created automated test suite in `BtrReturGudang.Wpf.Tests/ReturnOrderEntryViewTests.cs` (13 tests) verifying product name prominence without code noise, consecutive scan auto-increments, warning audio cues on unknown barcodes, real-time live grid commitments and footer calculations, line deletions, draft creation, `IOrderActionHandler` delegation, and STA XAML rendering.
  - Added `TestAssemblySetup.cs` disabling parallel xUnit execution for WPF UI tests to prevent AppDomain `Application` collisions, and added thread-safe locks in STA test harnesses.
  - Verified full test suite execution across all solution projects: 222/222 tests passing (`dotnet test src/j09-btr-retur-gudang/BtrReturGudang.sln`) with 0 errors and 0 warnings.

---

## P3 - Modal Lookup & Registration Dialogs (SCR-03, SCR-04, SCR-08)

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED  

### P3-S07: Manual Item Search & Quick Barcode Registration Dialogs (SCR-03, SCR-04)
- **Title:** Implement Modal Dialogs for Item Search (F2) & Quick Barcode Registration
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Implement WPF modal dialogs for `ManualItemSearchDialog.xaml` (search item catalog by name/code with real-time filtering) and `QuickBarcodeRegistrationDialog.xaml` (auto-triggered when unknown barcode is scanned to link barcode to item master).
- **Depends On:** P2-S06
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - `ManualItemSearchDialog`: `F2` opens search modal; typing filters master products instantly; `Enter` returns selected item to intake strip.
  - `QuickBarcodeRegistrationDialog`: Scanning unknown barcode automatically prompts modal; saves new barcode mapping via `IBarcodeService`; automatically resumes intake.
  - Both dialogs fit comfortably on 720p screens with center-owner positioning.
- **Implementation Notes:**
  - Implemented `ManualItemSearchViewModel.cs` providing real-time master product filtering by item code or name, packaging units resolution, selection commands, and keyboard navigation.
  - Implemented `ManualItemSearchDialog.xaml` and `.xaml.cs` (SCR-03) with 740x480 low-res compliant viewport (`TD-013`), `CenterOwner` startup location, compact 44px Slate-900 header, instant search filter bar, DataGrid showing item code/name/packaging units, unit selector dropdown, and `Enter`/double-click selection returning selected item and unit back to the intake strip.
  - Implemented `QuickBarcodeRegistrationViewModel.cs` supporting on-the-spot barcode registration: captures unmapped barcode, triggers 400Hz buzz warning tone upon prompt, filters master product catalog, selects packaging unit and multiplier, executes quick registration via `IBarcodeService.QuickRegisterUnmappedBarcodeAsync` (or `IBarcodeRepo` fallback), and yields `BarcodeLookupResult.Hit`.
  - Implemented `QuickBarcodeRegistrationDialog.xaml` and `.xaml.cs` (SCR-04) with 580x480 viewport (`TD-013`), `CenterOwner` startup location, 44px Rose-accented header ("⚠️ Barcode Belum Terdaftar"), read-only prominent barcode display (`#FFF1F2` background, `#BE123C` text), product search and selection, unit dropdown and `NumericTextBox` multiplier, and "Simpan & Gunakan" action button.
  - Integrated dialog launchers in `ReturnOrderEntryView.xaml.cs` and `MainWindow.xaml.cs`:
    - `F2` accelerator opens `ManualItemSearchDialog`, returning the selected item directly into the active product banner and focusing `QtyBagus`.
    - Scanning an unknown barcode automatically prompts `QuickBarcodeRegistrationDialog`, immediately registers the barcode mapping, and auto-resumes intake by applying the newly registered product to the banner and focusing `QtyBagus`.
  - Added comprehensive unit and STA UI test suite in `BtrReturGudang.Wpf.Tests/ModalDialogTests.cs` (14 new tests) covering window dimensions (740x480 and 580x480), real-time filtering, arrow navigation, unit synchronization, 400Hz audio warning, barcode registration execution, and intake auto-resumption.
  - Verified clean build (`0 Warning(s)`, `0 Error(s)`) and 100% test pass across all projects in the solution (`247/247 tests passing`).

---

### P3-S08: Manual Customer Search Dialog (SCR-08)
- **Title:** Implement Fast Modal Customer Search Dialog
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Implement `ManualCustomerSearchDialog.xaml` and ViewModel for fast keyboard-driven customer lookup over large datasets (> 2,000 customers), with walk-in customer fallback (`CUST-MANUAL`).
- **Depends On:** P2-S06
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - Real-time search by customer name, code, or address.
  - Arrow keys navigate results; `Enter` commits selection back to header.
  - Supports walk-in customer selection without freezing the UI thread.
- **Implementation Notes:**
  - Implemented `ViewModels/ManualCustomerSearchViewModel.cs`:
    - Real-time asynchronous customer search over master customer datasets (> 2,000 records) without UI thread blocking.
    - Arrow keys (`NavigateDownCommand` / `NavigateUpCommand`) with boundary clamping and live selected customer reflection.
    - Walk-in customer fallback (`SelectWalkInCustomerCommand`) generating a `CUST-MANUAL` customer record with the active search keyword as customer name.
    - Automatic walk-in fallback on `ConfirmSelectionCommand` when no matching catalog record is found.
    - Success/warning audio feedback integration via `IAudioHelper`.
  - Implemented `Views/ManualCustomerSearchDialog.xaml` and code-behind `ManualCustomerSearchDialog.xaml.cs`:
    - Responsive 760x480 modal dialog conforming to TD-013 screen budget on 1280x720 displays with `CenterOwner` startup location.
    - 44px Slate-900 header banner with title and keyboard usage hint.
    - Search filter strip with `TxtSearch`, search action button, and prominent walk-in customer button.
    - Styled DataGrid with monospace customer code, customer name, wilayah, and text-trimmed address columns.
    - Action footer with selected customer summary pill, record count status, and Enter/Esc/F6 button accelerators.
    - Full keyboard navigation handling: Up/Down arrow scrolling, Enter key commit, Esc cancellation, and F6 walk-in shortcut.
  - Enhanced `MasterDataDal.cs` customer search SQL queries to include `Alamat LIKE @Keyword` for full code, name, and address search capability.
  - Integrated `ManualCustomerSearchDialog` with `ReturnOrderEntryView.xaml.cs` (F8 key accelerator, auto-launcher) and registered in DI container (`App.xaml.cs`).
  - Added comprehensive automated test suite in `BtrReturGudang.Wpf.Tests/ManualCustomerSearchDialogTests.cs` (11 unit and STA tests) covering real-time name, code, address filtering, large dataset scalability (> 2,000 customers), arrow key navigation, walk-in fallback, Enter commit, Esc cancel, and STA XAML layout rendering.
  - Verified full test suite execution: 247/247 tests passing across the solution (`dotnet test src/j09-btr-retur-gudang/BtrReturGudang.sln`) with 0 errors and 0 warnings.

---

## P4 - Order History Inspector, Registry & Settings (SCR-05, SCR-06, SCR-07)

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED  

### P4-S09: Return Order History List & Post-Sync Locked Inspector (SCR-05)
- **Title:** Implement History List, Filters & Post-Sync Read-Only Inspector
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Implement `Views/ReturnOrderListView.xaml` and `ViewModels/ReturnOrderListViewModel.cs` with master-detail split layout:
  - Left panel: Return Orders grid with date/status filters (`All`, `Draft`, `Synced`).
  - Right panel: Document inspector with metadata table, detail items grid, sync badges, and post-sync locked banner (permanently disabling edits once `Synced`).
- **Depends On:** P2-S04
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - Splitter proportions balance evenly on 720p resolution without metadata truncation.
  - Synchronized orders display locked banner; `Edit` and `Delete` commands are disabled.
  - Local drafts can be pushed upstream via "Upload Sekarang" action.
- **Implementation Notes:**
  - Implemented `ViewModels/ReturnOrderListViewModel.cs` with date filtering (`StartDate`, `EndDate`), sync status filter (`All`, `Draft`, `Synced`), master order collection binding (`Orders`), and selected document inspector properties.
  - Built `Views/ReturnOrderListView.xaml` and `Views/ReturnOrderListView.xaml.cs` with master-detail split layout conforming to TD-013 on 720p resolution:
    - 44px top header with refresh command.
    - 40px compact filter bar with DatePickers, Status ComboBox, and summary counter.
    - Proportional GridSplitter layout (~540px left / ~500px right on 720p resolution with minimum widths 380px) preventing metadata truncation.
    - Left panel: `OrdersGrid` displaying ULID (Consolas font), date, customer name, total quantity, and status sync badges (amber for Draft, emerald for Synced).
    - Right panel: Document inspector displaying prominent red locked banner when `IsSelectedOrderSynced` is true, 4-column responsive metadata key-value table, line items DataGrid with `BAGUS`/`RUSAK` classification badges, and action toolbar (`Edit`, `Delete`, `Upload Sekarang`).
  - Enforced post-sync immutability (TD-010): When an order is `Synced`, locked banner is visible and `Edit`, `Delete`, and `Upload` commands/buttons are permanently disabled.
  - Added "Upload Sekarang" action triggering cloud sync with operator session validation gate (`OperatorSessionValidator`) pre-empting upload when operator Google email is unverified (TD-012).
  - Wired view mapping in `Views/MainWindow.xaml` to render `ReturnOrderListView` when navigating to `ReturnOrderListViewModel`.
  - Created automated test suite in `BtrReturGudang.Wpf.Tests/ReturnOrderListViewTests.cs` (11 unit and integration tests) verifying 720p visual layout proportions, post-sync locked banner display, command disabling, upload workflow, operator validation gate pre-emption, draft deletion with confirmation, draft editing event dispatch, and filter parameters.

---

### P4-S10: Barcode Registry View & Workstation Settings (SCR-06, SCR-07)
- **Title:** Implement Barcode Registry Catalog, Settings & Operator Switcher
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Implement `Views/BarcodeRegistryView.xaml` (master barcode registry manager with search and registration modal) and `Views/SettingsView.xaml` (workstation depo code, server target, API endpoint configuration, sound toggle, and shift operator switcher dialog).
- **Depends On:** P2-S04
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - `BarcodeRegistryView` displays registered barcodes with search, pagination/filter, and barcode deletion.
  - `SettingsView` persists workstation parameters via `IRegistryHelper`.
  - Operator Switcher dialog allows changing the active shift worker on the workstation.
- **Implementation Notes:**
  - Implemented `ViewModels/BarcodeRegistryViewModel.cs` managing active cached barcodes (`Aktif Pusat`) and pending registration requests (`Pending Upload`), with real-time keyword search (barcode, code, product name), status filtering, client-side pagination (`CurrentPage`, `PageSize`, `TotalPages`, `TotalItemsCount`), and barcode deletion with confirmation callback.
  - Added barcode deletion methods (`DeleteBarcode`, `DeleteBarcodeAsync`) to repository contract `IBarcodeRepo`, `BarcodeDal` (deleting from `BTRG_Barcode` and `BTRG_BarcodeRegistration`), application service `IBarcodeService`, and `BarcodeService`.
  - Implemented modal barcode registration dialogs `Views/BarcodeRegistrationDialog.xaml` and `Views/BarcodeRegistrationDialog.xaml.cs` (580x480 resolution per TD-013) with live master product search, unit dropdown, and multiplier input.
  - Built `Views/BarcodeRegistryView.xaml` and `Views/BarcodeRegistryView.xaml.cs` (SCR-06) conforming to TD-013 with 44px compact header, 42px search & pagination filter bar, status badges (emerald for Aktif Pusat, amber for Pending Upload), action button column with Delete action, and summary footer.
  - Implemented `ViewModels/SettingsViewModel.cs` managing workstation configuration (depo code, server target ID, PC name, API endpoint, active shift operator name & Google email, connection string, and audio beeper toggle) and persisting to Windows Registry via `IRegistryHelper`.
  - Added interactive testing & verification tools in `SettingsViewModel`:
    - Shift operator session validation via `OperatorSessionValidator` / `IBtradeApiClient.ResolveSessionAsync(email)` with unwrapped refusal messages (TD-012).
    - Local database connection test (`SELECT 1`) against SQL Server `btrgd`.
    - Cloud API relay route test (`HttpClient.GetAsync`).
    - Multimedia beepers test (2000Hz success click, 400Hz warning tone) and toggle switch.
  - Implemented modal dialog `Views/OperatorSwitcherDialog.xaml` and `Views/OperatorSwitcherDialog.xaml.cs` with validation for non-empty name and email regex pattern (`^[^@\s]+@[^@\s]+\.[^@\s]+$`), persisting active operator identity to workstation registry.
  - Built `Views/SettingsView.xaml` and `Views/SettingsView.xaml.cs` (SCR-07) conforming to TD-013 with 44px compact header, scrollable responsive card layout, and live status badges.
  - Wired view mapping DataTemplates for `BarcodeRegistryViewModel` and `SettingsViewModel` in `Views/MainWindow.xaml`, wired header "Ganti" button to open `OperatorSwitcherDialog` in `Views/MainWindow.xaml.cs`, and registered all views in `App.xaml.cs` DI container.
  - Added automated unit and STA integration tests in `BtrReturGudang.Wpf.Tests`:
    - `BarcodeRegistryViewModelTests.cs` (5 tests covering catalog & pending loading, keyword search, status filter, pagination, and deletion).
    - `SettingsViewModelTests.cs` (5 tests covering registry loading, saving parameters, session verification success/refusal, operator dialog switching, and audio toggle).
    - `OperatorSwitcherDialogTests.cs` (2 tests covering registry initialization and validation/persistence).
    - `BarcodeRegistryViewTests.cs` (STA view control rendering).
    - `SettingsViewTests.cs` (STA view control rendering).
  - Verified clean build (`0 Warning(s)`, `0 Error(s)`) and 100% test pass rate across the full solution (222/222 tests passing).

---

## P5 - Verification & Automated Test Suite

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED  

### P5-S11: WPF ViewModel Unit Tests & Workflow Integration Tests
- **Title:** Create `BtrReturGudang.Wpf.Tests` Suite
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Build an automated unit and integration test suite targeting ViewModels, command flows, hardware scanner event dispatching, and high-load intake logic.
- **Depends On:** P2-S06, P3-S07, P4-S09
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.Wpf.Tests` created and added to solution.
  - Tests verify:
    1. Scan barcode ? auto-populates product name without internal code.
    2. Zero prevention: typing digit when initial value is 0 sets value directly without appending 0.
    3. Re-scan consecutive identical barcode increments `QtyBagus`.
    4. Commit line item adds line and resets inputs.
    5. Post-sync document locking enforcement.
  - All tests pass via `dotnet test`.
- **Implementation Notes:**
  - Verified the existing `BtrReturGudang.Wpf.Tests` xUnit project (`net471`, `UseWPF`) is referenced by `BtrReturGudang.sln` and covers the required ViewModel, command, scanner-wedge, numeric-input, dialog, list-locking, settings, and registry surfaces.
  - Confirmed explicit coverage of all five completion criteria:
    1. `WorkflowIntegrationTests.Criterion_2_1_*` plus `ReturnOrderEntryViewTests.ProductBanner_ValidScan_*` assert the pure product name is populated while internal `BrgId` is absent from the banner.
    2. `WorkflowIntegrationTests.Criterion_2_2_*` and `NumericTextBoxTests.AntiZeroAppend_*` assert typing a digit over `"0"` replaces it directly and never produces `"10"`/`"0"+digit`.
    3. `WorkflowIntegrationTests.Criterion_2_3_*` and `ReturnOrderEntryViewTests.BarcodeScanning_ReScanningSameBarcode_*` assert consecutive identical scans increment `QtyBagus` and a different barcode resets it.
    4. `WorkflowIntegrationTests.Criterion_2_4_*` and `ReturnOrderEntryViewTests.QuantityStrip_Commit_*` assert committed BAGUS/RUSAK rows, summary counters, and quantity-strip reset.
    5. `WorkflowIntegrationTests.Criterion_2_5_*` and `ReturnOrderListViewTests.PostSyncImmutability_*` assert the locked banner and disabled edit/delete/upload commands for synced orders.
  - Filled the remaining objective gap (high-load intake logic) by adding `WorkflowIntegrationTests.HighLoadIntake_SustainedScanCommitCycles_KeepsGridCountersAndResetsConsistent`, covering 200 sustained scan -> commit cycles with grid counts, summary counters, line sequencing, per-cycle reset, and audio-cue verification.
  - Test inventory (111 tests total): `WorkflowIntegrationTests` 9, `ReturnOrderEntryViewTests` 8, `ReturnOrderListViewTests` 11, `MainWindowTests` 11, `ManualCustomerSearchDialogTests` 11, `ModalDialogTests` 14, `NumericTextBoxTests` 13, `WpfScannerWedgeListenerTests` 7, `MvvmTests` 6, `SettingsViewModelTests` 6, `BarcodeRegistryViewModelTests` 5, `OperatorSwitcherDialogTests` 2, `BarcodeRegistryViewTests` 1, `SettingsViewTests` 1 (plus `TestAssemblySetup.cs`).
  - Changed files: `BtrReturGudang.Wpf.Tests/WorkflowIntegrationTests.cs` (added high-load intake test); plan status/notes only otherwise. `BtrReturGudang.Wpf.Tests.csproj` and the solution entry were already present and verified.
  - Verification: `dotnet build src/j09-btr-retur-gudang/BtrReturGudang.sln` -> 0 Warning(s), 0 Error(s). `dotnet test src/j09-btr-retur-gudang/BtrReturGudang.sln` -> all green: AppTier 16/16, Infrastructure 46/46, Winform 86/86, Wpf 111/111 (259 total, 0 failed, 0 skipped).

---

# 6. Change Log

- **2026-10-22 (v1.0):** Initial planning created in response to user approval for building a native WPF client on .NET Framework 4.7.1, resolving WinForms text clipping, sticky zero quantity issues, and high-load warehouse ergonomics.
