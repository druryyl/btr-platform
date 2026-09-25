---
Title: BGud — Return Order Navigation Restructure — Implementation Plan
Code: BGUD-RETURN-ORDER-NAV-001
Artifact: IMPLEMENTATION-PLAN
Version: 1.0
LastUpdated: 2026-09-25
Status: COMPLETED
Execution Approval: APPROVED
---

# 1. Objective

Restructure BGud's navigation hierarchy, screen routing, layout organization, and workflow prioritization so that Return Order becomes the operational center of the application.

Referenced artifacts:

- ISSUE: `docs/issues/BGUD-RETURN-ORDER-NAV-ISSUE.md` (FEATURE waived)
- ARCHITECTURE: `docs/work/bgud-return-order-nav/BGUD-RETURN-ORDER-NAV-ARCHITECTURE.md`
- FEASIBILITY-ASSESSMENT: `docs/work/bgud-return-order-nav/BGUD-RETURN-ORDER-NAV-FEASIBILITY-ASSESSMENT.md`

---

# 2. Planning Scope

This implementation plan focuses on Navigation and Layout only within the BGud application. It covers:
- Restructuring `HomeScreen` into a work launcher.
- Updating `ReturnOrderListScreen` with date-grouped sections and status-conditional routing.
- Re-prioritizing layout in `CreateReturnOrderScreen` and `EditReturnOrderScreen`.
- Relocating the Delete action from Detail to Edit screens.
- Adding a Register Barcode entry point in `BarcodeRegistryScreen`.
- Wiring up all routing changes in `ui/Navigation.kt` while removing obsolete routes.
Visual styling changes and backend modifications are strictly excluded.

---

# 3. Dependencies

External dependencies: None.

For slice dependencies:

- `Depends On` declares implementation prerequisites.
- Dependencies reference Slice IDs only.
- Dependency satisfaction requires the referenced slice to have implementation
  status IMPLEMENTED.
- Dependency satisfaction does not require review status GO.
- Dependencies must represent real implementation prerequisites.

---

# 4. Progress Summary

Plan status values are:

- NOT-STARTED
- IN-PROGRESS
- BLOCKED
- COMPLETED

Execution Approval values are:

- PENDING
- APPROVED

Execution Approval is owned by the Architect. It is PENDING during Planning
and set to APPROVED when the plan is released for execution. Execution must
not begin while Execution Approval is PENDING.

COMPLETED is a plan-level status only. Set it only when every slice has
implementation status IMPLEMENTED and review status GO.

Testing and test-package creation must not begin until the plan is COMPLETED.
An individual slice with review status GO is not a testing entry condition.

Slice implementation status values are:

- NOT-STARTED
- IN-PROGRESS
- IMPLEMENTED
- BLOCKED

Slice review status values are:

- NOT-REVIEWED
- GO
- NO-GO

| Phase | Implementation Status | Review Status | Progress |
|---------|---------|---------|---------|
| P1 | IMPLEMENTED | GO | 1/1 |
| P2 | IMPLEMENTED | GO | 5/5 |
| P3 | IMPLEMENTED | GO | 1/1 |

---

# 5. Phases

## P1 - Data Preparation

Implementation Status: IMPLEMENTED
Review Status: GO

### P1-S01

Title: Date-Grouped Sections for Return Orders

Implementation Status: IMPLEMENTED
Review Status: GO

Objective: Add date-grouping transformation logic (TODAY, YESTERDAY, EARLIER) to `ReturnOrderListViewModel` without altering DAO queries.

Depends On: None

Repository: btr-platform

Completion Criteria:
- `ReturnOrderListViewModel` exposes UI state that groups return orders by date section.
- Existing features (most-recent-first sorting, status filters, search) are retained.

Implementation Notes:
- Added `ReturnOrderDateSection` enum (`TODAY`, `YESTERDAY`, `EARLIER`) and `ReturnOrderGroupedSection` data class.
- Added `groupedSections` StateFlow (`StateFlow<List<ReturnOrderGroupedSection>>`) to `ReturnOrderListViewModel` exposing non-empty date sections.
- Implemented `categorizeDate` and `groupByDateSection` comparing `createdAt` against local start-of-day and start-of-yesterday boundaries.
- Preserved existing sorting (`ORDER BY createdAt DESC`), search query handling, status filtering, incremental paging, and flat `results` StateFlow.
- Added unit tests in `ReturnOrderListViewModelTest.kt` verifying date categorization, midnight/year boundaries, and section grouping.

Changed Files:
- `src/BGud/app/src/main/java/com/elsasa/bgud/viewmodel/ReturnOrderListViewModel.kt`
- `src/BGud/app/src/test/java/com/elsasa/bgud/viewmodel/ReturnOrderListViewModelTest.kt`

Notes:
- Transformation should compare `createdAt` to the current local date.

---

## P2 - Screen Component Restructure

Implementation Status: IMPLEMENTED
Review Status: GO

### P2-S02

Title: Return Order List Layout and Row Actions

Implementation Status: IMPLEMENTED
Review Status: GO

Objective: Update `ReturnOrderListScreen` to display grouped sections and support status-conditional row clicks.

Depends On: P1-S01

Repository: btr-platform

Completion Criteria:
- Screen visualizes date-grouped sections (TODAY / YESTERDAY / EARLIER).
- Row click handlers distinguish Draft (trigger edit callback) and Synced (trigger detail callback) statuses.

Implementation Notes:
- Updated `ReturnOrderListScreen` to collect `viewModel.groupedSections` and render items grouped under date section headers ("TODAY", "YESTERDAY", "EARLIER").
- Updated row click handlers to conditionally route based on `item.order.status`: Draft triggers edit callback (`handleEditDraft`), and Synced triggers detail callback (`handleViewDetail`).
- Added optional `onEditDraft`, `onEditReturnOrder`, and `onViewDetail` parameters to `ReturnOrderListScreen` with default fallback to `onOpenDetail` to maintain clean backwards compatibility until P3-S07 wires them.
- Preserved status filters (Semua / Draft / Synced), debounced search query, FAB for Create Return, and incremental paging.

Changed Files:
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/ReturnOrderListScreen.kt`

Notes:
- None.

---

### P2-S03

Title: Relocate Delete Action for Draft Orders

Implementation Status: IMPLEMENTED
Review Status: GO

Objective: Remove the Delete action from `ReturnOrderDetailScreen` and add it to `EditReturnOrderScreen` for Draft orders.

Depends On: None

Repository: btr-platform

Completion Criteria:
- `ReturnOrderDetailScreen` contains no Delete action.
- `EditReturnOrderScreen` contains a Delete action that is only visible and enabled when `status == DRAFT`.

Implementation Notes:
- Removed Delete action, confirmation dialog, and delete state observations (`isDeleting`, `deleteError`, `deleted`) from `ReturnOrderDetailScreen.kt`. Retained optional `onDeleted` parameter with default no-op to maintain caller compatibility.
- Removed delete state and operations (`_isDeleting`, `_deleteError`, `_deleted`, `canDelete()`, `delete()`) from `ReturnOrderDetailViewModel.kt`. Updated `canEdit()` to check `isDraft()`.
- Added status tracking (`status: StateFlow<String?>`) and delete state flows (`isDeleting`, `deleteError`, `deleted`) to `EditReturnOrderViewModel.kt`. Implemented `canDelete()` and `delete()` guarded to only Draft orders (`status == ReturnOrderEntity.STATUS_DRAFT && !notEditable`) delegating to `captureRepository.deleteDraft(returnOrderId)`. Updated `canSave()` and `save()` to prevent concurrent save while deleting.
- Added Delete action button (`OutlinedButton`) and confirmation `AlertDialog` on `EditReturnOrderScreen.kt` guarded so it is only visible and enabled when `status == DRAFT`. Added deleted state confirmation card invoking `onDeleted` (defaulting to `onCancel`). Screen controls are disabled when either saving or deleting (`isBusy`).

Changed Files:
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/ReturnOrderDetailScreen.kt`
- `src/BGud/app/src/main/java/com/elsasa/bgud/viewmodel/ReturnOrderDetailViewModel.kt`
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/EditReturnOrderScreen.kt`
- `src/BGud/app/src/main/java/com/elsasa/bgud/viewmodel/EditReturnOrderViewModel.kt`

Notes:
- Delete must trigger confirmation and then local Room delete logic (unchanged behavior).

---

### P2-S04

Title: Re-prioritize Capture Layouts

Implementation Status: IMPLEMENTED
Review Status: GO

Objective: Restructure the layout of `CreateReturnOrderScreen` and `EditReturnOrderScreen` to make Item Entry the dominant region.

Depends On: P2-S03

Repository: btr-platform

Completion Criteria:
- Capture layouts strictly follow the top-down priority: Transaction Context -> Item Entry / List -> Additional Info (Salesman, Driver, Notes) -> Action Region.

Implementation Notes:
- Reordered composables in `CreateReturnOrderScreen.kt` and `EditReturnOrderScreen.kt` to follow the top-down priority:
  1. Transaction Context (Customer selector [mandatory], Warehouse info [read-only])
  2. Item Entry / List (dominant region: Barcode Scanner, Item Search, Qty, Unit, Return Type, and added-items list)
  3. Additional Info (Salesman [optional], Driver [optional], Notes)
  4. Action Region (Save, Cancel; Delete Draft on Edit screen)
- Preserved all existing functionality, busy/enabled state management, consecutive barcode scanning, local cache lookups, and validations.
- Updated KDoc layout diagrams in both screen files to reflect the re-prioritized sections.

Changed Files:
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/CreateReturnOrderScreen.kt`
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/EditReturnOrderScreen.kt`

Notes:
- Dependent on P2-S03 to avoid file conflict on `EditReturnOrderScreen.kt`. No styling/theme changes.

---

### P2-S05

Title: Add Register Barcode Action to Registry

Implementation Status: IMPLEMENTED
Review Status: GO

Objective: Add a "Register Barcode" entry point to `BarcodeRegistryScreen`.

Depends On: None

Repository: btr-platform

Completion Criteria:
- `BarcodeRegistryScreen` contains a UI element to trigger a register barcode callback (which will eventually pass no pre-filled barcode parameter).

Implementation Notes:
- Added `onRegisterBarcode: () -> Unit = {}` callback parameter to `BarcodeRegistryScreen` with default no-op value for backward compatibility with existing callers until P3-S07 wires it to `register`.
- Added a full-width primary `Button` labeled "Register Barcode" in the action area above "Kembali" (`OutlinedButton`) in `BarcodeRegistryScreen.kt`.
- Updated KDoc layout ASCII diagram in `BarcodeRegistryScreen.kt` to document the Action Region containing Register Barcode and Kembali.
- Preserved existing local-only search, paged list, edit row-action, and empty state behaviors without visual styling changes.

Changed Files:
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/BarcodeRegistryScreen.kt`

Notes:
- None.

---

### P2-S06

Title: Home Screen Layout Restructure

Implementation Status: IMPLEMENTED
Review Status: GO

Objective: Transform `HomeScreen` from a feature launcher into a work launcher and reorganize secondary menus.

Depends On: None

Repository: btr-platform

Completion Criteria:
- Scan/Search/Register Barcode quick actions are removed.
- Primary actions are New Return and Return Orders.
- Synchronization Status card is interactive and triggers a sync callback.
- More section retains only Barcode Registry and Settings.

Implementation Notes:
- Restructured `HomeScreen.kt` from a feature launcher to a work launcher (TD-001, TD-008):
  - Removed Quick Actions for "Scan Barcode", "Search Barcode", and "Register Barcode".
  - Promoted "New Return" (primary `Button`) and "Return Orders" (`OutlinedButton`) under the "Actions" section.
  - Made the Synchronization Status `Card` interactive/clickable via `Modifier.clickable(onClick = handleSyncClick)`.
  - Reorganized the secondary section to "More" containing only "Barcode Registry" and "Settings" `ListItem` entries; removed the standalone "Synchronization" item from More.
  - Added new callback parameters (`onNewReturn`, `onReturnOrders`, `onSyncStatusClick`, `onOpenSync`, `onBarcodeRegistry`, `onSettings`) with default values and fallback handling for existing callbacks (`onOpenReturnOrder`, `onOpenSynchronization`, `onOpenBarcodeRegistry`, `onOpenSettings`, `onScanBarcode`, `onSearchBarcode`, `onRegisterBarcode`) so existing callers (`Navigation.kt`) remain backward compatible until P3-S07.
- Updated KDoc layout ASCII diagram in `HomeScreen.kt` documenting the work-launcher hierarchy.

Changed Files:
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/HomeScreen.kt`

Notes:
- None.

---

## P3 - Navigation Assembly

Implementation Status: IMPLEMENTED
Review Status: GO

### P3-S07

Title: Update Navigation Graph Wiring

Implementation Status: IMPLEMENTED
Review Status: GO

Objective: Update `ui/Navigation.kt` to route the updated screens and remove obsolete destinations.

Depends On: P2-S02, P2-S04, P2-S05, P2-S06

Repository: btr-platform

Completion Criteria:
- `return_order_list` Draft rows route to `return_order_edit`. Synced rows route to `return_order_detail`.
- Home navigation routes correctly mapped to Create, List, Sync, Registry, and Settings.
- Barcode Registry's Register action routes to `register` with no barcode parameter.
- Standalone `scan` route removed from top-level Home navigation.
- Draft Detail route (`return_order_detail` for Draft status) is removed/unreachable.

Implementation Notes:
- Updated `HomeScreen` wiring in `ui/Navigation.kt` (TD-001, TD-008): mapped work-launcher callbacks `onNewReturn` to `return_order_create`, `onReturnOrders` to `return_order_list`, `onSyncStatusClick` to `synchronization`, `onBarcodeRegistry` to `barcode_registry`, and `onSettings` to `settings`; removed top-level quick action wirings (`onScanBarcode`, `onSearchBarcode`, `onRegisterBarcode`).
- Updated `ReturnOrderListScreen` wiring in `ui/Navigation.kt` (TD-004): mapped `onEditDraft` to `return_order_edit?returnOrderId={id}` and `onViewDetail` to `return_order_detail?returnOrderId={id}`, ensuring Draft rows route to Edit/Resume while Synced rows route to Detail, making Draft Detail unreachable from the list.
- Updated `BarcodeRegistryScreen` wiring in `ui/Navigation.kt` (TD-003): mapped `onRegisterBarcode` to `register` without any barcode argument.
- Explicitly wired `onDeleted` in `EditReturnOrderScreen` to `navController.popBackStack()` (TD-005).
- Updated KDoc layout ASCII diagram and docstring in `Navigation.kt` to reflect the work-launcher navigation graph.
- Verified debug compilation and unit test suite (`compileDebugKotlin`, `testDebugUnitTest`).

Changed Files:
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/Navigation.kt`

Notes:
- None.

---

# 6. Change Log

- 2026-09-25: Initial creation.
