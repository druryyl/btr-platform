---
Title: BGud — Return Order Navigation Restructure — Implementation Plan
Code: BGUD-RETURN-ORDER-NAV-001
Artifact: IMPLEMENTATION-PLAN
Version: 1.0
LastUpdated: 2026-09-25
Status: NOT-STARTED
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
| P1 | NOT-STARTED | NOT-REVIEWED | 0/1 |
| P2 | NOT-STARTED | NOT-REVIEWED | 0/5 |
| P3 | NOT-STARTED | NOT-REVIEWED | 0/1 |

---

# 5. Phases

## P1 - Data Preparation

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

### P1-S01

Title: Date-Grouped Sections for Return Orders

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

Objective: Add date-grouping transformation logic (TODAY, YESTERDAY, EARLIER) to `ReturnOrderListViewModel` without altering DAO queries.

Depends On: None

Repository: btr-platform

Completion Criteria:
- `ReturnOrderListViewModel` exposes UI state that groups return orders by date section.
- Existing features (most-recent-first sorting, status filters, search) are retained.

Notes:
- Transformation should compare `createdAt` to the current local date.

---

## P2 - Screen Component Restructure

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

### P2-S02

Title: Return Order List Layout and Row Actions

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

Objective: Update `ReturnOrderListScreen` to display grouped sections and support status-conditional row clicks.

Depends On: P1-S01

Repository: btr-platform

Completion Criteria:
- Screen visualizes date-grouped sections (TODAY / YESTERDAY / EARLIER).
- Row click handlers distinguish Draft (trigger edit callback) and Synced (trigger detail callback) statuses.

Notes:
- None.

---

### P2-S03

Title: Relocate Delete Action for Draft Orders

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

Objective: Remove the Delete action from `ReturnOrderDetailScreen` and add it to `EditReturnOrderScreen` for Draft orders.

Depends On: None

Repository: btr-platform

Completion Criteria:
- `ReturnOrderDetailScreen` contains no Delete action.
- `EditReturnOrderScreen` contains a Delete action that is only visible and enabled when `status == DRAFT`.

Notes:
- Delete must trigger confirmation and then local Room delete logic (unchanged behavior).

---

### P2-S04

Title: Re-prioritize Capture Layouts

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

Objective: Restructure the layout of `CreateReturnOrderScreen` and `EditReturnOrderScreen` to make Item Entry the dominant region.

Depends On: P2-S03

Repository: btr-platform

Completion Criteria:
- Capture layouts strictly follow the top-down priority: Transaction Context -> Item Entry / List -> Additional Info (Salesman, Driver, Notes) -> Action Region.

Notes:
- Dependent on P2-S03 to avoid file conflict on `EditReturnOrderScreen.kt`. No styling/theme changes.

---

### P2-S05

Title: Add Register Barcode Action to Registry

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

Objective: Add a "Register Barcode" entry point to `BarcodeRegistryScreen`.

Depends On: None

Repository: btr-platform

Completion Criteria:
- `BarcodeRegistryScreen` contains a UI element to trigger a register barcode callback (which will eventually pass no pre-filled barcode parameter).

Notes:
- None.

---

### P2-S06

Title: Home Screen Layout Restructure

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

Objective: Transform `HomeScreen` from a feature launcher into a work launcher and reorganize secondary menus.

Depends On: None

Repository: btr-platform

Completion Criteria:
- Scan/Search/Register Barcode quick actions are removed.
- Primary actions are New Return and Return Orders.
- Synchronization Status card is interactive and triggers a sync callback.
- More section retains only Barcode Registry and Settings.

Notes:
- None.

---

## P3 - Navigation Assembly

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

### P3-S07

Title: Update Navigation Graph Wiring

Implementation Status: NOT-STARTED
Review Status: NOT-REVIEWED

Objective: Update `ui/Navigation.kt` to route the updated screens and remove obsolete destinations.

Depends On: P2-S02, P2-S04, P2-S05, P2-S06

Repository: btr-platform

Completion Criteria:
- `return_order_list` Draft rows route to `return_order_edit`. Synced rows route to `return_order_detail`.
- Home navigation routes correctly mapped to Create, List, Sync, Registry, and Settings.
- Barcode Registry's Register action routes to `register` with no barcode parameter.
- Standalone `scan` route removed from top-level Home navigation.
- Draft Detail route (`return_order_detail` for Draft status) is removed/unreachable.

Notes:
- None.

---

# 6. Change Log

- 2026-09-25: Initial creation.
