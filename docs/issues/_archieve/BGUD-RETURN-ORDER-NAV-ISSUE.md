# ISSUE

## Metadata

ID: `BGUD-RETURN-ORDER-NAV-001`  
Type: `CHANGE-REQUEST`  
Status: `New`  
Title: **BGud — Make Return Order the operational center of navigation and unify return capture**

## Source

Reported By: Repository owner (direct request)  
Reported Date: 2026-09-25

## Description

BGud currently presents Barcode Registry and Return Order as nearly equal primary
features. In actual warehouse operation, Return Order is the daily operational
workflow, while Barcode Registry is supporting infrastructure. The current
navigation is functionally complete but exposes too many conceptual destinations
and does not make Return Order the dominant path.

The request is to restructure **navigation and screen/layout structure only**,
so that:

- Return Order becomes the center of the application.
- New Return becomes the primary action.
- Barcode scanning becomes a contextual action inside Return Order capture,
  not a primary navigation destination.
- Return capture uses a continuous transaction surface rather than navigating
  through a sequence of steps.
- Draft Return Orders can be resumed directly from the list.
- Barcode Registry, Synchronization, and Settings remain accessible but
  secondary.
- Navigation minimizes screen transitions and typing for warehouse operators.

The reporter states this is about navigation hierarchy, interaction flow, screen
structure, and layout priorities. Visual styling (colors, typography,
iconography, branding, spacing, component styling) is out of scope and is to be
handled by a separate UI design change request.

## Desired Outcome

### Requested information architecture

```text
LOGIN
  |
  v
HOME
  |
  +--> NEW RETURN
  |
  +--> RETURN ORDERS
  |      |
  |      +--> Draft   -> Edit/Resume
  |      |
  |      +--> Synced  -> Read-only Detail
  |
  +--> MORE
         |
         +--> Barcode Registry
         +--> Synchronization
         +--> Settings
```

### Home

Home changes from a feature launcher into a work launcher:

```text
BGud
Warehouse Context

----------------------------------

RETURN ORDER

[ + NEW RETURN ]

[ RETURN ORDERS ]

----------------------------------

SYNC STATUS

Online / Offline
Pending Count
Last Sync

----------------------------------

More

Barcode Registry
Synchronization
Settings
```

- New Return must be the most prominent action.
- Return Orders must be the primary navigation destination.
- Sync status may be shown as a compact card that leads to the Synchronization
  screen.
- Barcode Registry must not compete visually with Return Order.
- `Scan Barcode`, `Search Barcode`, and `Register Barcode` must not be exposed
  as equal top-level Home actions.

### Return Order capture

Return capture becomes a single continuous transaction surface with the general
shape:

```text
New Return

Customer
[ Select Customer ]

----------------------------------

Returned Items

[ Item Rows ]

----------------------------------

[ Scan Barcode ] [ + Add Item ]

----------------------------------

[ SAVE RETURN ]
```

Each item row should display Item, Quantity, Unit, and Return Type
(`BAGUS` / `RUSAK`).

The reporter explicitly states that no new business rules are introduced. The
existing domain rules remain unchanged:

- Customer mandatory
- Warehouse mandatory
- At least one item
- Item must exist
- Quantity > 0
- Unit mandatory
- Return Type mandatory
- Salesman optional
- Driver optional

### Barcode scanning inside Return capture

Scanning becomes part of the Return Order workflow rather than a separate
destination:

```text
Scan
  |
  v
Local Barcode Lookup
  |
  v
Identify Item
  |
  v
Append Item To Current Return
  |
  v
Ready To Scan Again
```

- Barcode lookup remains offline/local.
- No dedicated Scan Result screen.
- Multiple items can be scanned consecutively.
- Manual item search remains available.
- No network call during capture.

### Item entry

```text
Scan/Search
     |
     v
Item Identified
     |
     v
Quantity
     |
     v
Unit
     |
     v
BAGUS / RUSAK
     |
     v
Add Item
     |
     v
Ready For Next Item
```

- Reuse cached item/unit information where possible.
- Prefill values instead of asking users to repeatedly enter data.

### Return Orders list

The list becomes a work queue rather than generic CRUD navigation:

```text
Return Orders

[ Draft ] [ Synced ]

Search

----------------------------------

TODAY

08:42 Customer A
      4 Items
      Draft

08:31 Customer B
      7 Items
      Synced
```

- Distinguish Draft and Synced clearly.
- Show recent transactions first.
- Preserve search.
- Draft opens directly to editing.
- Synced opens read-only detail.
- New Return remains visible and accessible.
- The existing lifecycle is not changed.

### Draft vs Synced navigation

```text
Draft
  |
  v
Edit / Resume

Synced
  |
  v
Read Only Detail
```

The reporter's stated reason is that Draft is active operational work while
Synced is completed operational work, and removing the Detail step for Draft
reduces unnecessary navigation. Existing rules remain:

- Draft: edit allowed, delete allowed.
- Synced: edit prohibited, delete prohibited.

### Customer, Salesman, Driver

- Customer is selected once near the top of the transaction, remains visible,
  and is not re-selected during item entry.
- Salesman and Driver remain optional, administrative, and secondary. They must
  not be prominent during warehouse capture and must not gain additional
  workflow. The reporter states no new business rules are introduced.

### Barcode Registry

Barcode Registry remains available but becomes a secondary tool reached through
More, with its existing Search / Register / Edit functionality preserved. No
Barcode Registry business rules change.

### Synchronization

Synchronization remains a secondary destination. Home displays Online/Offline,
Pending Count, and Last Sync Time, and the status card leads to the
Synchronization screen. Manual synchronization is preserved; no background sync
is introduced; synchronization architecture is not changed.

### Settings

Settings remains reachable through More with no behavior change.

### Explicit non-goals

The reporter excludes: new Return Order business rules; domain model changes;
API changes; database changes; synchronization redesign; background sync;
Barcode Registry redesign; approval workflows; inventory processing changes;
finance processing changes; pricing changes; and visual branding redesign.

## Current Situation

### Home and navigation

- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/HomeScreen.kt`
  presents "Quick Actions" as `Scan Barcode` (primary button), `Search Barcode`,
  `Register Barcode`, and `Return Order` (all outlined buttons of similar
  weight), then a Synchronization Status card, then a "Navigation" list with
  Barcode Registry, Synchronization, and Settings.
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/Navigation.kt` wires Home to
  `scan`, `barcode_registry`, `register`, `return_order_list`,
  `barcode_registry`, `synchronization`, and `settings`.
- `return_order_list` is currently reached only through the Return Order quick
  action; `Scan Barcode` and `Search Barcode` are separate top-level
  destinations.

### Return Order list and Draft/Synced

- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/ReturnOrderListScreen.kt`
  provides a search field, `Semua` / `Draft` / `Synced` filter chips, a list
  whose rows show customer, date, item count, and status, and a `+` FAB to
  create.
- Row tap navigates to `return_order_detail?returnOrderId={id}` for both Draft
  and Synced.
- Per `Navigation.kt`, Edit is reachable only from Detail, and Detail gates the
  Edit action on `Draft`.

### Return Order capture

- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/CreateReturnOrderScreen.kt`
  is currently a single scrolling form containing: read-only Warehouse; Customer
  search/select; Salesman search/select; Driver search/select; Notes; an item
  region with an embedded `BarcodeScannerView` plus manual item search; Qty;
  Unit dropdown; BAGUS/RUSAK chips; Add Item; the list of added lines; and
  Save/Cancel.
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/EditReturnOrderScreen.kt`
  provides the Draft modification surface.

### Barcode scanning

- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/ScanScreen.kt` is a
  standalone top-level destination with a camera viewfinder, manual entry, a
  "Barcode Found" result region (with Scan Again / Close), and a "Barcode Not
  Found" region (with Register Barcode / Cancel).
- Item identification during capture already uses the shared
  `ui/component/BarcodeScannerView.kt` and local lookup.

### Architecture references

- `docs/work/return-order/RETURN-ORDER-ARCHITECTURE.md` defines the current
  BGud Navigation Compose graph (§13.1), the mobile screens SCR-MOB-RO-001..005
  (§11–§12), and the Draft/Synced status model.
- `docs/work/barcode-registry/BARCODE-REGISTRY-ARCHITECTURE.md` and
  `docs/work/barcode-registry/BARCODE-REGISTRY-UX-BLUEPRINT.md` define the
  current Barcode Registry screens and Home navigation model.
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/Navigation.kt` documents the
  current navigation graph in its header comment, including
  `home -> Scan Barcode -> scan`, `home -> Search Barcode -> barcode_registry`,
  and `home -> Return Order -> return_order_list`.

## Evidence

- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/Navigation.kt` — current
  navigation graph and route wiring.
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/HomeScreen.kt` — current
  Home quick actions, sync status card, and secondary navigation list.
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/ReturnOrderListScreen.kt`
  — current list, filters, and row-to-detail navigation.
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/CreateReturnOrderScreen.kt`
  — current capture surface and embedded scanner.
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/EditReturnOrderScreen.kt`
  — current Draft edit surface.
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/ReturnOrderDetailScreen.kt`
  — current read-only detail and status-gated actions.
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/ScanScreen.kt` — current
  standalone scan destination.
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/component/BarcodeScannerView.kt`
  — shared scanner component.
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/BarcodeRegistryScreen.kt`,
  `RegisterScreen.kt`, `EditBarcodeScreen.kt` — existing Barcode Registry
  functionality.
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/SynchronizationScreen.kt`
  and `SettingsScreen.kt` — existing secondary destinations.
- `docs/work/return-order/RETURN-ORDER-ARCHITECTURE.md` — Return Order contract,
  screens, and navigation.
- `docs/work/barcode-registry/BARCODE-REGISTRY-ARCHITECTURE.md`,
  `docs/work/barcode-registry/BARCODE-REGISTRY-UX-BLUEPRINT.md` — Barcode
  Registry and Home navigation model.

## Notes

The following points clarify the intake information without defining analysis,
architecture, implementation, planning, or testing strategy.

1. **Scope of this change.** The reporter frames this as navigation and
   screen/layout structure only. Visual styling is explicitly deferred to a
   separate UI design change request. It should be confirmed that this ISSUE
   covers structure and flow, not visual design.

2. **Existing capture surface.** The current `CreateReturnOrderScreen` is
   already a single scrolling form rather than a literal multi-step wizard, and
   it already embeds the barcode scanner. The reporter's "Do Not Implement"
   wizard sequence appears to be a caution rather than a description of the
   current implementation. It should be confirmed that the requested change is
   to restructure the existing continuous form so item entry dominates and
   Salesman/Driver/Notes are de-emphasized.

3. **Top-level scan destination.** Removing `Scan Barcode`, `Search Barcode`,
   and `Register Barcode` from Home raises the question of whether the existing
   standalone `scan` route and `ScanScreen` are removed, retained only as an
   internal capture step, or retained as a secondary tool. `Register Barcode`
   is currently also reachable from the standalone scan Not-Found flow, so the
   fate of that path needs confirmation.

4. **Draft row behavior.** The requested "Draft opens directly to editing"
   removes the current List → Detail → Edit path for Draft. It should be
   confirmed whether Detail remains reachable for Draft orders at all (for
   example, for viewing before editing), or whether Draft rows navigate only to
   Edit/Resume.

5. **Sync status card interaction.** It should be confirmed that tapping the
   Home sync status card is the intended entry to Synchronization, and whether
   the existing "Synchronization" secondary menu entry is retained alongside it.

6. **Reporter-provided impact hints.** The reporter suggests reusing the
   existing Compose architecture and names `ui/Navigation.kt`, `HomeScreen.kt`,
   `ReturnOrderListScreen.kt`, `CreateReturnOrderScreen.kt`,
   `EditReturnOrderScreen.kt`, `ReturnOrderDetailScreen.kt`, and `ScanScreen.kt`
   as the primary surfaces, while asking that ViewModels, repositories,
   persistence, synchronization, networking, and domain logic be left
   unchanged. These are reporter preferences for downstream analysis, not
   decisions recorded by this ISSUE.

7. **No new business rules.** The reporter states that existing Return Order
   business rules and the Draft/Synced lifecycle and permissions are unchanged.
   Any downstream change must preserve them.
