---
Title: BGud — Return Order Navigation Restructure — Architecture
Code: BGUD-RETURN-ORDER-NAV-001
Artifact: ARCHITECTURE
Version: 1.0
LastUpdated: 2026-09-25
---

# 1. Overview

This architecture realizes the BGud Return Order Navigation Restructure change
request (`BGUD-RETURN-ORDER-NAV-001`): restructuring BGud's navigation
hierarchy, screen routing, information architecture, layout organization, and
workflow prioritization so that Return Order becomes the operational center of
the application.

The change is confined to **Navigation and Layout** only. All backend systems,
domain logic, persistence, synchronization, networking, and ViewModels are
unchanged except where the approved navigation structure strictly requires it.

---

# 2. Architectural Basis

## Business Context

- **DOMAIN:** `docs/work/return-order/RETURN-ORDER-DOMAIN.md` — authoritative
  for business capabilities BC-001…BC-005, business rules BR-001…BR-020, and
  the Draft → Synced → Imported lifecycle.
- **ISSUE (Discovery input):** `docs/issues/BGUD-RETURN-ORDER-NAV-ISSUE.md` —
  accepted as the authoritative operational-flow input for this change in lieu
  of a FEATURE artifact (GAP-001, waived).
- **No FEATURE artifact** was created for this change; the waiver is specific
  to this request and does not apply to future changes that introduce new
  business capabilities.

## Analysis Input

- **FEASIBILITY-ASSESSMENT:**
  `docs/work/bgud-return-order-nav/BGUD-RETURN-ORDER-NAV-FEASIBILITY-ASSESSMENT.md`

All eleven approved gap-closure decisions (GAP-001…GAP-010, OQ-010) are
consumed here. This architecture does not re-evaluate them.

## Consumed Decisions

| Gap | Decision |
|-----|----------|
| GAP-001 | FEATURE artifact waived; ISSUE + DOMAIN are the Discovery input. |
| GAP-002 | Register Barcode entry point moved into Barcode Registry; Home-level action removed; standalone `scan` route no longer required as a navigation entry point for barcode registration. |
| GAP-003 | Draft Detail screen removed from the navigation graph; Draft rows navigate directly to Edit/Resume; Delete action relocated from Detail to Edit. |
| GAP-004 | Home transitions from a feature launcher to a work launcher: New Return (primary action), Return Orders (primary destination), Synchronization Status card (clickable), More section (Barcode Registry, Settings). |
| GAP-005 | Return Orders list becomes a work queue: date grouping (TODAY / YESTERDAY / EARLIER), recent-first ordering preserved, search preserved, New Return preserved, Draft → Edit/Resume, Synced → read-only Detail. |
| GAP-006 | Return Order capture workflow retained; layout re-prioritized: Item Entry becomes the dominant working area; Salesman, Driver, Notes become secondary. |
| GAP-007 | Affected architecture artifacts updated by this architecture work: `RETURN-ORDER-ARCHITECTURE.md`, `BARCODE-REGISTRY-ARCHITECTURE.md`, `BARCODE-REGISTRY-UX-BLUEPRINT.md`. |
| GAP-008 | Scope is Navigation and Layout only; visual styling (colors, typography, iconography, branding, component styling) is excluded and deferred to a separate UI/UX Styling change request. |
| GAP-009 | Synchronization Status card is the authoritative navigation entry to Synchronization; Synchronization menu item removed from More. |
| GAP-010 | Existing status filters (Semua, Draft, Synced) retained; default remains Semua. |
| OQ-010 | Home's New Return reuses the existing full-screen `return_order_create` route; no new capture route or inline experience is introduced. |

---

# 3. Scope

## Included

- BGud navigation hierarchy (`ui/Navigation.kt`)
- BGud Home screen structure (`HomeScreen.kt`)
- BGud Return Order list structure and routing (`ReturnOrderListScreen.kt`)
- BGud Return Order capture layout re-prioritization (`CreateReturnOrderScreen.kt`)
- BGud Return Order edit screen: Delete action relocation (`EditReturnOrderScreen.kt`)
- BGud Barcode Registry screen: Register Barcode action addition (`BarcodeRegistryScreen.kt`)
- BGud Synchronization Status card: make clickable (`HomeScreen.kt`)
- Draft Detail screen removal from navigation graph
- Update of `RETURN-ORDER-ARCHITECTURE.md` §11.1, §12, §13.1, §14, §16
- Update of `BARCODE-REGISTRY-ARCHITECTURE.md` BGud navigation section
- Update of `BARCODE-REGISTRY-UX-BLUEPRINT.md` §4 and §6

## Excluded

- BTR Desktop, Cloud API, `j07-btrade-sync`, domain logic, persistence,
  synchronization, networking — all unchanged.
- Visual styling: colors, typography, iconography, branding, component styling,
  elevation, shadows, animations, visual emphasis — deferred to a separate
  UI/UX Styling change request (GAP-008).
- New business rules, Return Order lifecycle, API changes, database changes.
- `ScanScreen.kt` and `ScanViewModel` as navigation entry points for barcode
  registration (scan route is no longer a top-level Home destination; the
  embedded `BarcodeScannerView` inside capture is unchanged).
- `ReturnOrderDetailScreen.kt` for Draft orders (Draft Detail removed from
  navigation graph); the screen file may remain but it is no longer reachable
  for Draft.

---

# 4. Technical Decisions

## TD-001 — Home becomes a work launcher

Home is restructured from a feature launcher to a work launcher. The primary
operational hierarchy is:

1. **New Return** — primary action; navigates to `return_order_create`.
2. **Return Orders** — primary destination; navigates to `return_order_list`.
3. **Synchronization Status card** — existing card becomes clickable;
   navigates to `synchronization`.
4. **More** — secondary section containing Barcode Registry and Settings only.

The `Scan Barcode`, `Search Barcode`, and `Register Barcode` quick actions are
removed from Home. The standalone `scan` route is no longer a top-level Home
destination.

*Traceable to: GAP-004, GAP-009.*

## TD-002 — Standalone `scan` route removed from Home navigation

The standalone `scan` route (`ScanScreen`) is no longer exposed as a top-level
navigation destination from Home. `BarcodeScannerView` embedded within
`CreateReturnOrderScreen` and `EditReturnOrderScreen` is unchanged — scanning
remains available contextually within Return capture.

The `scan` route may be retained in the navigation graph as a technical route
if needed for backward compatibility, but it is not advertised or reachable
from the redesigned Home.

*Traceable to: GAP-002, OQ-001.*

## TD-003 — Register Barcode entry point relocated to Barcode Registry

`BarcodeRegistryScreen` gains an explicit **Register Barcode** action that
navigates to `register` (the existing `RegisterScreen`). The `register` route
accepts an optional `barcode` parameter (existing), so navigation from Barcode
Registry passes no barcode value — the field starts empty.

The Home-level Register Barcode quick action is removed.

*Traceable to: GAP-002, OQ-005.*

## TD-004 — Draft Detail screen removed; Draft rows route to Edit

From `ReturnOrderListScreen`, row tap behavior is now status-conditional:

- **Draft row** → navigates to `return_order_edit?returnOrderId={id}`
  (Edit / Resume).
- **Synced row** → navigates to `return_order_detail?returnOrderId={id}`
  (read-only Detail; unchanged behavior).

The Draft Detail path (`return_order_detail` for a Draft order) is removed from
the navigation graph. `ReturnOrderDetailScreen` remains reachable for Synced
orders only.

*Traceable to: GAP-003, OQ-002.*

## TD-005 — Delete action relocated from Detail to Edit

The **Delete Draft** action is relocated from `ReturnOrderDetailScreen` to
`EditReturnOrderScreen`. On the Edit screen:

- Delete is visible and enabled only when `status == DRAFT` (BR-019).
- Delete triggers confirmation and then local Room delete (unchanged behavior).
- Synced orders remain non-editable and non-deletable on the Edit screen
  (BR-020).

*Traceable to: GAP-003, OQ-003.*

## TD-006 — Return Orders list gains date-section grouping

The Return Orders list is restructured as a work queue:

- Items remain sorted by most-recent-first (`ORDER BY createdAt DESC` — unchanged).
- Items are visually grouped into date sections: **TODAY**, **YESTERDAY**,
  **EARLIER**.
- Date grouping is a UI-layer concern only; no DAO, query, or Room schema
  change is required. The ViewModel transforms the existing flat result into
  grouped sections by comparing `createdAt` to the current local date.
- Status filters (Semua / Draft / Synced) are retained; default remains Semua.
- New Return FAB and search are retained.

*Traceable to: GAP-005, GAP-010.*

## TD-007 — Capture layout re-prioritized

`CreateReturnOrderScreen` and `EditReturnOrderScreen` retain the existing
single-screen capture workflow and embedded `BarcodeScannerView`. The layout
structure is re-prioritized:

```text
Transaction Context  (Customer [mandatory], Warehouse [read-only])
Item Entry / List    (Barcode Scanner, Item Search, Qty, Unit, Return Type;
                      added-item list: Item, Qty, Unit, Return Type per row)
Additional Info      (Salesman [optional], Driver [optional], Notes)
Action Region        (Save, Cancel; Delete on Edit screen when Draft)
```

Item Entry / List becomes the dominant working area. Salesman, Driver, and
Notes are moved to a secondary / collapsed region below item entry.

The existing capture behavior (consecutive barcode scanning, offline/local
lookup, no network call) is unchanged.

*Traceable to: GAP-006, OQ-007.*

## TD-008 — Synchronization Status card becomes a navigation entry

The Home Synchronization Status card (currently non-clickable) becomes
clickable and navigates to the Synchronization screen. The separate
Synchronization menu item is removed from More.

The Synchronization screen itself is unchanged.

*Traceable to: GAP-009, OQ-004.*

## TD-009 — No new routes, ViewModels, or screens introduced

All navigation reuses existing routes:

- `return_order_create` — Home New Return action.
- `return_order_list` — Home Return Orders action.
- `return_order_edit?returnOrderId={id}` — Draft row from list.
- `return_order_detail?returnOrderId={id}` — Synced row from list.
- `synchronization` — Synchronization Status card tap.
- `barcode_registry` — More → Barcode Registry.
- `settings` — More → Settings.
- `register` (existing, optional `barcode` param) — Barcode Registry → Register.

No new routes are created. No new ViewModels are created. `ReturnOrderListViewModel`
requires a date-grouping transformation (TD-006); this is an additive change
to the existing ViewModel.

*Traceable to: OQ-010.*

---

# 5. Component Responsibilities

| Component | Responsibility |
|-----------|----------------|
| `HomeScreen.kt` | Work-launcher Home: New Return (primary), Return Orders (primary), Synchronization Status (clickable), More (Barcode Registry, Settings). Removes Scan/Search/Register quick actions. |
| `ReturnOrderListScreen.kt` | Work-queue list with date-section grouping (TODAY/YESTERDAY/EARLIER); status-conditional row routing (Draft → Edit, Synced → Detail); retains search, filters, New Return FAB. |
| `ReturnOrderListViewModel` | Additive: transforms flat Room result into date-grouped sections. Existing query, paging, and search are unchanged. |
| `CreateReturnOrderScreen.kt` | Re-prioritized layout: Transaction Context → Item Entry/List → Additional Info → Action. Existing capture workflow unchanged. |
| `EditReturnOrderScreen.kt` | Re-prioritized layout (mirrors Create). Hosts relocated Delete action (Draft only). Existing edit workflow unchanged. |
| `ReturnOrderDetailScreen.kt` | Now exclusively for Synced orders. Delete action removed. Draft Detail is unreachable from the navigation graph. |
| `BarcodeRegistryScreen.kt` | Gains a Register Barcode action that navigates to `register` (no pre-filled barcode). Existing Search and Edit actions unchanged. |
| `ui/Navigation.kt` | Updated navigation graph: Home routes, status-conditional list routing, More section, Synchronization card route, Register from Barcode Registry, removal of Home-level scan/search/register routes. |

---

# 6. Integration Design

| Source | Target | Purpose |
|--------|--------|---------|
| `HomeScreen` New Return | `return_order_create` | Navigate to existing Create screen. |
| `HomeScreen` Return Orders | `return_order_list` | Navigate to existing List screen. |
| `HomeScreen` Sync Status card | `synchronization` | Navigate to existing Sync screen (card now clickable). |
| `HomeScreen` More → Barcode Registry | `barcode_registry` | Navigate to Barcode Registry. |
| `HomeScreen` More → Settings | `settings` | Navigate to Settings. |
| `ReturnOrderListScreen` Draft row | `return_order_edit` | Navigate to Edit / Resume (status-conditional). |
| `ReturnOrderListScreen` Synced row | `return_order_detail` | Navigate to read-only Detail (unchanged). |
| `ReturnOrderListScreen` New Return FAB | `return_order_create` | Navigate to Create (unchanged). |
| `EditReturnOrderScreen` Delete | local Room delete | Delete Draft; confirmation then local-only delete (unchanged behavior, relocated entry). |
| `BarcodeRegistryScreen` Register | `register` | Navigate to existing Register screen (no pre-filled barcode). |

No new API calls, no new backend integrations.

---

# 7. Data Ownership

| Data | Owner |
|------|-------|
| Return Order list (grouped) | `ReturnOrderListViewModel` — date grouping is a ViewModel transformation; Room ownership unchanged. |
| Navigation state | `ui/Navigation.kt` — single source of truth for all routes. |
| Draft/Synced status-based routing logic | `ReturnOrderListScreen.kt` — row onClick delegates to ViewModel or passes status to NavController. |
| Delete action (Draft) | `EditReturnOrderScreen.kt` (relocated from Detail). |
| Register Barcode entry | `BarcodeRegistryScreen.kt` (relocated from Home). |

---

# 8. Database Design

## New Tables

None. No database changes are introduced.

## Modified Tables

None.

## Relationships

No new relationships.

## Migration Considerations

None. No schema changes; no Room database version bump required for the
navigation/layout restructure. If the ViewModel date-grouping transformation
requires no new DAO query shape, no DAO changes are needed. If a new date-range
query is more efficient than client-side grouping, it may be introduced as an
additive DAO method — this is an implementation-time decision; no schema change
is required.

---

# 9. Cross-Cutting Concerns

## Offline Invariant

The offline-first constraint (P-07 from `RETURN-ORDER-ARCHITECTURE.md`) is
preserved. No navigation change introduces a network dependency on any capture
or list screen.

## Business Rule Preservation

All existing business rules (BR-001…BR-020) are preserved unchanged:

- BR-019 (Draft delete allowed): preserved — Delete relocated to Edit, not
  removed.
- BR-020 (Synced delete prohibited): preserved — Delete is gated on Draft
  status.
- BR-017/018 (modify pre-sync only): preserved — Edit is still gated on Draft.

## Scope Boundary — Navigation and Layout Only

Implementers must not introduce visual styling changes (colors, typography,
iconography, component styling) as part of this change. Visual styling is
explicitly deferred to a separate UI/UX Styling change request (GAP-008).

---

# 10. Implementation Constraints

1. **No new routes.** All navigation uses existing route definitions. The
   navigation graph is modified (rewired), not extended with new destinations.
2. **No new screens.** No new Composable screen files are created for this
   change.
3. **No new ViewModels.** `ReturnOrderListViewModel` receives an additive
   date-grouping transformation; no new ViewModel is created.
4. **No backend changes.** ViewModels, repositories, DAOs, Room schema,
   synchronization, networking, and domain logic must not change except for
   the additive date-grouping transformation in `ReturnOrderListViewModel`.
5. **Status-conditional routing must be correct.** Draft → Edit; Synced →
   Detail. Mixing these routes is a regression of BR-017/BR-019/BR-020.
6. **Delete on Edit, Draft only.** Delete must be visible and enabled only
   when `status == DRAFT`. The guard must be implemented at the ViewModel level,
   not only at the UI level.
7. **Navigation and Layout only.** Do not change colors, typography,
   iconography, component styling, or any visual styling treatment.
8. **Register from Barcode Registry passes no barcode.** The `register` route
   navigated from `BarcodeRegistryScreen` carries no `barcode` query parameter
   — the registration form starts with an empty Barcode field.

---

# 11. Acceptance Conditions

1. Home screen exposes exactly: New Return (primary), Return Orders (primary),
   Synchronization Status card (clickable → Synchronization screen), More
   section (Barcode Registry, Settings). No Scan Barcode, Search Barcode, or
   Register Barcode quick actions appear on Home.
2. More section contains Barcode Registry and Settings only. Synchronization
   does not appear in More.
3. Return Orders list displays date-grouped sections (TODAY / YESTERDAY /
   EARLIER) with items ordered most-recent-first within each section.
4. Tapping a Draft row navigates to `EditReturnOrderScreen` (Edit / Resume).
5. Tapping a Synced row navigates to `ReturnOrderDetailScreen` (read-only).
6. Status filters (Semua / Draft / Synced) are present; default is Semua.
7. `EditReturnOrderScreen` shows a Delete action that is enabled only when
   `status == DRAFT`.
8. `ReturnOrderDetailScreen` no longer shows a Delete action.
9. `ReturnOrderDetailScreen` is not reachable from the list for a Draft order.
10. `BarcodeRegistryScreen` shows a Register Barcode action that navigates to
    `RegisterScreen` with no pre-filled barcode value.
11. `CreateReturnOrderScreen` layout presents Transaction Context at the top,
    Item Entry / Item List as the dominant region, and Additional Info
    (Salesman, Driver, Notes) in a secondary position below item entry.
12. `EditReturnOrderScreen` layout mirrors Create in structure.
13. No capture step triggers a network call (preserved from P-07).
14. BR-019 and BR-020 are preserved: Draft orders are deletable; Synced orders
    are not deletable and not editable.
15. No visual styling changes (colors, typography, iconography, component
    styling) are introduced.
