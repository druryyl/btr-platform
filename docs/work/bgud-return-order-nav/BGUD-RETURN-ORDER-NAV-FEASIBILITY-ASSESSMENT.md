---
Title: BGud — Return Order Navigation Restructure — Feasibility Assessment
Code: BGUD-RETURN-ORDER-NAV-001
Artifact: FEASIBILITY-ASSESSMENT
Version: 1.0
LastUpdated: 2026-09-25
Status: NOT-READY
---

# 1. Request Summary

Assessment of ISSUE `BGUD-RETURN-ORDER-NAV-001`
(`docs/issues/BGUD-RETURN-ORDER-NAV-ISSUE.md`): restructure **BGud navigation
and screen/layout structure only** so that Return Order becomes the operational
center of the application — New Return as the primary action, barcode scanning
contextual inside Return capture, Return Orders as the primary destination,
Draft orders resumable directly from the list, and Barcode Registry /
Synchronization / Settings demoted to secondary ("More") destinations. Visual
styling is explicitly deferred to a separate UI design change request.

Referenced artifacts:

- DOMAIN: `docs/work/return-order/RETURN-ORDER-DOMAIN.md` (Return Order
  business knowledge, actors, business capabilities BC-001…BC-005, business
  rules BR-001…BR-020, Draft → Synced → Imported lifecycle)
- FEATURE: **none exists** for the BGud Return Order operational flow
  (see GAP-001). The ISSUE itself is the only definition of the requested
  navigation/operational-flow change.
- Current architecture (owned by the Architect): 
  `docs/work/return-order/RETURN-ORDER-ARCHITECTURE.md` §11–§13, §20;
  `docs/work/barcode-registry/BARCODE-REGISTRY-ARCHITECTURE.md` and
  `docs/work/barcode-registry/BARCODE-REGISTRY-UX-BLUEPRINT.md` §4, §6.

## Objective

Determine whether the requested navigation/structure change can be implemented
within the current BGud system, and identify the gaps, open questions, risks,
and decisions that must be resolved before the target architecture can be
finalized. This assessment defines no target state and creates no business
decision; every ambiguity is surfaced, never silently resolved.

---

# 2. Current State

This section contains facts only. Verified 2026-09-25 against the repository
working tree.

## Existing Behavior

### Home (`HomeScreen.kt`, SCR-MOB-002)

- Home is a **feature launcher**. It renders, in order: title `BGud`; a
  "Current Context" card (Logged In User, Warehouse, Office); a **Quick
  Actions** group; a "Synchronization Status" card; and a "Navigation" list.
- Quick Actions: `Scan Barcode` (primary `Button`), then `Search Barcode`,
  `Register Barcode`, and `Return Order` (all `OutlinedButton` of similar
  visual weight). Evidence: `HomeScreen.kt:98-124`.
- "Synchronization Status" shows Status (Online/Offline), Last Sync Time, and
  Pending Upload Count. The card is **not clickable** — no navigation is
  attached. Evidence: `HomeScreen.kt:126-134`.
- Navigation list: `Barcode Registry`, `Synchronization`, `Settings`.
  Evidence: `HomeScreen.kt:136-150`.

### Navigation graph (`ui/Navigation.kt`)

- `home` currently wires: `scan`, `barcode_registry` (via both `Search Barcode`
  and `Barcode Registry`), `register`, `return_order_list`,
  `synchronization`, `settings`. Evidence: `Navigation.kt:230-252` and the
  header comment `Navigation.kt:95-167`.
- `return_order_list` → row → `return_order_detail?returnOrderId={id}`;
  Create → `return_order_create`. Evidence: `Navigation.kt:253-275`.
- `return_order_detail` → Edit (gated on `Draft`) → 
  `return_order_edit?returnOrderId={id}`; Delete is a Draft-only action **on
  Detail only**. Evidence: `Navigation.kt:298-331`,
  `ReturnOrderDetailScreen.kt:220-246`.
- `scan` (SCR-MOB-003) is a standalone destination offering camera viewfinder,
  manual entry, a "Barcode Found" result region (Scan Again / Close), and a
  "Barcode Not Found" region (Register Barcode / Cancel). The Not-Found action
  navigates to `register?barcode={value}`. Evidence: `Navigation.kt:365-378`,
  `ScanScreen.kt`.
- `register?barcode={barcode}` is reached from **two** places: the Home
  `Register Barcode` quick action (no barcode) and the `scan` Not-Found flow
  (with barcode). Evidence: `Navigation.kt:244-246,375`.
- `barcode_registry` (SCR-MOB-005) provides search + a result list whose only
  row action is **Edit** (`edit?barcodeId={id}`). It has **no Register
  action**. Evidence: `BarcodeRegistryScreen.kt:52-56,129-132,167-183`,
  `Navigation.kt:408-453`.

### Return Order list (`ReturnOrderListScreen.kt`, SCR-MOB-RO-001)

- Title `Return Order`; search field (`Cari customer / tanggal`, min 3 chars,
  300 ms debounce); filter chips `Semua` / `Draft` / `Synced`; a flat
  `LazyColumn` of rows (customer, date, item count, status); a `+` FAB for
  Create; `Kembali` button. Evidence: `ReturnOrderListScreen.kt:74-203`.
- Rows show customer, a **day-formatted** creation date
  (`dd MMM yyyy`), item count, and a `Draft`/`Synced` label. There is **no
  date group header** (e.g. "TODAY"). Evidence: `ReturnOrderListScreen.kt:219-251`.
- Row tap navigates to Detail for **both** Draft and Synced.
  Evidence: `ReturnOrderListScreen.kt:167-171`, `Navigation.kt:267-271`.
- Ordering is already most-recent-first (`ORDER BY createdAt DESC`), with
  incremental paging (default 50 rows). Evidence: `ReturnOrderDao.kt`,
  `ReturnOrderListScreen.kt:160-179`.

### Return Order capture (`CreateReturnOrderScreen.kt`, SCR-MOB-RO-002)

- The screen is **already a single scrolling form**, not a multi-step wizard.
  It renders, in order: read-only Warehouse; Customer picker (mandatory);
  Salesman picker (optional); Driver picker (optional); Notes; an "Item" region
  containing an embedded `BarcodeScannerView` plus manual Item search; Qty;
  Unit dropdown; `BAGUS`/`RUSAK` chips; Add Item; the added-line list; Save /
  Cancel. Evidence: `CreateReturnOrderScreen.kt:136-477`.
- Item identification already reuses the shared `BarcodeScannerView` and local
  Room lookup; the scanner is enabled whenever there is no pending item, so
  multiple items can be scanned consecutively. No capture step issues a network
  call. Evidence: `CreateReturnOrderScreen.kt:286-331`,
  `BarcodeScannerView.kt`.
- `EditReturnOrderScreen.kt` (SCR-MOB-RO-004) mirrors the capture surface with
  prefilled fields and a "not editable" state for non-Draft orders. Evidence:
  `EditReturnOrderScreen.kt:80-212`.

### Secondary destinations

- `SynchronizationScreen` (SCR-MOB-007) shows sync state and a Sync Now
  trigger; it also renders Return Order sync state. Evidence:
  `Navigation.kt:454-490`.
- `SettingsScreen` (SCR-MOB-008) provides logout / warehouse change. Evidence:
  `Navigation.kt:491-516`.

### Artifacts describing the current navigation

- `RETURN-ORDER-ARCHITECTURE.md` §11.1 defines SCR-MOB-RO-001…005 and §13.1
  defines the current BGud graph (`home → Return Order → return_order_list`;
  `return_order_list` row → Detail; Detail → Edit). §12.2 fixes the capture
  layout as Header (Customer/Warehouse/Salesman/Driver/Notes) then Item then
  Action. §20 fixes list paging/search.
- `BARCODE-REGISTRY-UX-BLUEPRINT.md` §4 defines the current Home navigation
  (`Scan Barcode`, `Barcode Registry { Search, Register, Edit }`,
  `Synchronization`, `Settings`), and §6 fixes Home Quick Actions as
  Scan / Search / Register.

## Existing Constraints

- **Stated scope.** The reporter frames the change as navigation hierarchy,
  interaction flow, screen structure, and layout priorities only. Visual
  styling (colors, typography, iconography, branding, spacing, component
  styling) is explicitly out of scope and deferred to a separate UI design
  change request.
- **No new business rules.** The reporter states existing Return Order domain
  rules are unchanged, including Customer/Warehouse mandatory, at least one
  item, item must exist, Qty > 0, Unit and Return Type mandatory, optional
  Salesman/Driver, and the Draft/`Synced` lifecycle and permissions
  (DOMAIN BR-017…BR-020).
- **Explicit non-goals.** No domain model changes, API changes, database
  changes, synchronization redesign, background sync, Barcode Registry
  redesign, approval workflows, inventory/finance/pricing changes, or visual
  branding redesign.
- **Offline and architecture invariants.** Barcode lookup remains
  offline/local; no network call during capture; manual sync only;
  synchronization architecture unchanged (DOMAIN BG-003;
  `RETURN-ORDER-ARCHITECTURE.md` P-07, §8.6).
- **Reporter preference, not a decision.** The reporter asks that ViewModels,
  repositories, persistence, synchronization, networking, and domain logic be
  left unchanged and names `ui/Navigation.kt`, `HomeScreen.kt`,
  `ReturnOrderListScreen.kt`, `CreateReturnOrderScreen.kt`,
  `EditReturnOrderScreen.kt`, `ReturnOrderDetailScreen.kt`, and
  `ScanScreen.kt` as the primary surfaces.
- **Knowledge state.** No FEATURE artifact exists for the BGud Return Order
  operational flow; the Return Order knowledge lives as DOMAIN and ARCHITECTURE
  under `docs/work/return-order/`.

---

# 3. Gap Analysis

| ID | Severity | Gap |
|------|------|------|
| GAP-001 | MAJOR | **No FEATURE artifact defines the target operational flow.** The request changes BGud's operational flow and navigation hierarchy ("Return Order is the daily operational workflow"), which is FEATURE-owned knowledge, yet no FEATURE artifact exists for the BGud Return Order capability. Only DOMAIN (`RETURN-ORDER-DOMAIN.md`) and ARCHITECTURE (`RETURN-ORDER-ARCHITECTURE.md`) exist. Architecture therefore has no authoritative business-outcome/operational-flow input for the target navigation. |
| GAP-002 | CRITICAL | **Removing the Home barcode actions / standalone `scan` breaks the Register Barcode entry.** `register` is reachable only from the Home `Register Barcode` quick action (`Navigation.kt:246`) and the `scan` Not-Found flow (`Navigation.kt:375`). `BarcodeRegistryScreen` exposes only search + Edit and has **no Register action** (`BarcodeRegistryScreen.kt:167-183`). The requested IA removes the Home actions and makes scanning contextual; unless a replacement entry is defined, "Barcode Registry … Register … functionality preserved" cannot hold. |
| GAP-003 | CRITICAL | **Routing Draft rows directly to Edit removes the only Delete entry point.** Delete is a Draft-only action that lives on Detail (`ReturnOrderDetailScreen.kt:229-239`), and Edit is reachable only from Detail (`Navigation.kt:321-330`). If a Draft row opens Edit/Resume directly, the Draft Detail surface (and therefore Delete) becomes unreachable unless the delete action is relocated. DOMAIN BR-019/BR-020 (Draft delete allowed, Synced delete prohibited) must be preserved. |
| GAP-004 | MAJOR | **Home is a feature launcher; the target is a work launcher.** Current Home exposes Scan/Search/Register/Return Order with near-equal weight, a non-clickable sync card, and a flat Navigation list. The target requires New Return as the most prominent action, Return Orders as the primary destination, a clickable sync status card, and a "More" group (Barcode Registry / Synchronization / Settings). Evidence: `HomeScreen.kt:98-150`. |
| GAP-005 | MAJOR | **Return Orders list becomes a work queue.** The target adds date grouping (e.g. "TODAY"), keeps recent-first, preserves search, keeps New Return accessible, and routes Draft → Edit/Resume while Synced → read-only Detail. The current list is a flat, ungrouped list whose rows always open Detail. Ordering is already recent-first. Evidence: `ReturnOrderListScreen.kt:160-251`, `ReturnOrderDao.kt`. |
| GAP-006 | MAJOR | **Capture is already a continuous form; the real change is layout re-prioritization.** `CreateReturnOrderScreen` already embeds the scanner and identifies items locally with consecutive scans, so the requested "continuous transaction surface" largely exists. The substantive change is structural emphasis: item entry dominates while Salesman/Driver/Notes are de-emphasized, and each row shows Item, Quantity, Unit, Return Type. The ISSUE itself (Note 2) leaves this confirmation open. Evidence: `CreateReturnOrderScreen.kt:136-477`. |
| GAP-007 | MAJOR | **Existing architecture artifacts become inconsistent.** The approved architecture fixes the current navigation and layouts that this request contradicts: `RETURN-ORDER-ARCHITECTURE.md` §11.1/§12.2/§13.1 (List → Detail → Edit; Home → return_order_list only) and `BARCODE-REGISTRY-UX-BLUEPRINT.md` §4/§6 and `BARCODE-REGISTRY-ARCHITECTURE.md` (Home quick actions; Barcode Registry branch). ARCHITECTURE is owned by the Architect; it must be reviewed and updated so knowledge and code stay synchronized. |
| GAP-008 | MAJOR | **Scope boundary between "structure" and deferred visual styling is not fixed.** Requirements such as "New Return must be the most prominent action", "Barcode Registry must not compete visually with Return Order", and card/chip emphasis are realized partly through visual properties (size, weight, color, spacing). The boundary between this change and the separate UI design change request is undefined, creating overlap/conflict risk. |
| GAP-009 | MINOR | **Duplicate route to Synchronization.** The target makes the Home sync status card lead to Synchronization while the "More" group may also retain a "Synchronization" item, yielding two entries to the same destination. ISSUE Note 5 asks for confirmation. Evidence: `HomeScreen.kt:142-145`. |
| GAP-010 | MINOR | **Filter set drift.** The target mockup shows only `[Draft] [Synced]`, while the current list has `Semua` / `Draft` / `Synced`; "preserve search" is stated but the fate of the `Semua` chip is not. Evidence: `ReturnOrderListScreen.kt:111-134`. |

Severity key: CRITICAL — blocks target architecture from being finalized;
MAJOR — significant structure/flow decision required; MINOR — small,
localized decision.

---

# 4. Open Questions

| ID | Question | Impact |
|------|------|------|
| OQ-001 | What is the fate of the standalone `scan` route and `ScanScreen` — removed entirely, retained as a secondary tool under "More", or retained only as an internal capture step? What happens to the scan Not-Found → `register?barcode=` path (`Navigation.kt:375`)? | Determines the whole navigation graph and the Barcode Registry entry points. ISSUE Note 3. |
| OQ-002 | Does Return Order Detail remain reachable for a **Draft** order at all (e.g. to view before editing), or do Draft rows navigate only to Edit/Resume? | Determines the list → detail/edit routing and whether Detail remains in the graph. ISSUE Note 4. |
| OQ-003 | If Draft rows skip Detail, where does the Draft **Delete** action live (Edit screen action, long-press, swipe)? | Required to preserve BR-019/BR-020 delete capability (GAP-003). |
| OQ-004 | Is the Home sync status card the entry to Synchronization, and is the existing "Synchronization" item in "More" retained alongside it or removed? | Determines whether two entries to one destination remain. ISSUE Note 5. |
| OQ-005 | How is **Register Barcode** reached after the top-level Home actions and (possibly) standalone `scan` are removed — a new Register action inside Barcode Registry, or only from a retained scan Not-Found flow? | Required to satisfy "Barcode Registry Search/Register/Edit preserved" (GAP-002). |
| OQ-006 | Which prominence/emphasis requirements belong to this structure change versus the separate UI design change request? | Fixes scope and prevents overlap/conflict between the two change requests (GAP-008). |
| OQ-007 | How strictly must the reporter's "leave ViewModels, repositories, persistence, networking, and domain logic unchanged" hold? May ViewModels be adjusted where the requested structure requires it (date grouping, Draft resume, Delete relocation, capture state)? | If absolutely no ViewModel change is allowed, part of the requested structure may be infeasible. ISSUE Note 6. |
| OQ-008 | Is the `Semua` status filter chip retained, or does the list show only `Draft` / `Synced`? | Determines the filter control set; search is confirmed preserved. |
| OQ-009 | Is a FEATURE artifact for the BGud Return Order operational flow required before architecture, or does the detailed ISSUE serve as the Discovery input? | Determines whether workflow Discovery is complete (GAP-001). |
| OQ-010 | Does Home's "New Return" reuse the existing full-screen `return_order_create` route, or become a distinct/inline capture entry? | Determines whether a new navigation target/route is introduced. |

---

# 5. Assumptions

| ID | Assumption |
|------|------|
| ASM-001 | The change is confined to BGud UI/navigation. BTR Desktop, the Cloud API, `j07-btrade-sync`, domain logic, persistence, and synchronization remain unchanged (ISSUE non-goals). |
| ASM-002 | No new Return Order business rules are introduced; the Draft/Synced lifecycle and edit/delete permissions (DOMAIN BR-017…BR-020) remain exactly as approved. |
| ASM-003 | Visual styling is owned by a separate UI design change request; this assessment treats prominence/emphasis as placement, ordering, grouping, and hierarchy only. |
| ASM-004 | The IA diagrams in the ISSUE express hierarchy and intent, not pixel-accurate layouts. |
| ASM-005 | The existing Return Order decisions remain authoritative: Return Type `BAGUS`/`RUSAK`, session-bound Warehouse, offline/local capture with no network call, manual synchronization only. |
| ASM-006 | The reported "Current Situation" matches the code at HEAD; this assessment spot-verified Home, Navigation, List, Create/Edit/Detail, Scan, Register, and Barcode Registry. |

---

# 6. Risks

| ID | Risk | Impact | Mitigation |
|------|------|------|------|
| RISK-001 | Register Barcode becomes unreachable after the Home actions and standalone `scan` are removed | High — a preserved capability regresses | Resolve OQ-005 / GAP-002 before architecture; add or retain an explicit Register entry point. |
| RISK-002 | Draft Delete capability regresses when Draft rows bypass Detail | High — violates BR-019/BR-020 as experienced by operators | Resolve OQ-003 / GAP-003; relocate the Draft delete action with a clear confirmation. |
| RISK-003 | Structure and visual styling overlap/conflict with the separate UI design change request | Medium — duplicated or contradictory changes to the same screens | Fix the boundary (OQ-006 / GAP-008); record which change owns each requirement. |
| RISK-004 | "No ViewModel changes" constraint conflicts with the requested structure | Medium — either the constraint is violated or the structure is left incomplete | Resolve OQ-007; define the permitted implementation surface before planning. |
| RISK-005 | Knowledge drift: architecture/UX artifacts still describe the pre-change navigation | Medium — implementers may follow stale navigation contracts | Architecture review/update by the Architect (GAP-007) before planning starts. |
| RISK-006 | Removing the top-level Scan quick action increases steps for ad-hoc item lookup | Medium — operator efficiency and muscle memory | Keep scanning fast inside capture and preserve manual search; confirm Barcode Registry search covers lookup. |
| RISK-007 | Two navigation entries to Synchronization (card + More item) confuse operators | Low — minor UX inconsistency | Resolve OQ-004 (GAP-009). |
| RISK-008 | Home grouping ("work launcher") is interpreted as a redesign beyond navigation and pulls in visual styling | Medium — scope creep across change requests | Enforce ASM-003 and the OQ-006 boundary. |

---

# 7. Recommendations

Alternative solution directions only. **No final decision is recorded here** —
decisions belong in §8 Gap Closure after stakeholder/architecture input.

## OQ-001 / GAP-002 — Standalone scan and the Register entry

### Option A — Remove the standalone `scan` route; make Register reachable from Barcode Registry

Scanning exists only inside Return capture; Barcode Registry gains an explicit
Register action; the `register?barcode=` Not-Found path is repurposed or removed.

- Advantages: matches the requested IA ("scanning becomes part of Return
  capture"); minimizes top-level destinations; keeps a single Register entry.
- Disadvantages: requires a Register action in Barcode Registry (new surface
  behavior); retires `ScanScreen`/`ScanViewModel` usage; changes the
  scan-not-found experience.

### Option B — Retain `scan` as a secondary tool under "More"

Home promotes Return Order; `scan`, Barcode Registry, Synchronization, and
Settings move under "More"; Register remains reachable via scan Not-Found.

- Advantages: preserves the existing scan and register-not-found flows with the
  least behavioral change; lower regression risk.
- Disadvantages: keeps a barcode-centric destination that the request intends to
  de-emphasize; less faithful to the requested IA.

### Option C — Retain `scan` only as an internal capture step

`BarcodeScannerView` inside capture remains; the standalone route is removed and
the registry has no scan/register; Register is entered from capture as needed.

- Advantages: smallest navigation surface.
- Disadvantages: ad-hoc (non-return) lookup and registration may become
  unreachable; high regression risk without a replacement.

## OQ-003 / GAP-003 — Draft Delete placement

### Option A — Delete moves onto the Edit/Resume surface (Draft only)

- Advantages: preserves single-surface Draft maintenance; no separate Detail
  step for Draft.
- Disadvantages: delete shares a screen with editing; needs clear confirmation.

### Option B — Keep a Detail surface for Draft and route Draft rows to it

- Advantages: preserves the current Delete location and view-before-edit.
- Disadvantages: contradicts the requested "Draft opens directly to editing".

### Option C — Row-level Draft actions (long-press / swipe)

- Advantages: quick delete without leaving the list.
- Disadvantages: discoverability; diverges from the requested row behavior.

## OQ-009 / GAP-001 — Workflow Discovery completeness

### Option A — Create a FEATURE artifact for the BGud Return Order operational flow

- Advantages: restores artifact ownership; gives architecture an authoritative
  business-outcome/operational-flow input; conforms to the workflow gate.
- Disadvantages: an additional Discovery step before architecture.

### Option B — Treat the detailed ISSUE plus the existing DOMAIN as sufficient
Discovery for this change

- Advantages: faster; the ISSUE is unusually detailed and adds no business rules.
- Disadvantages: leaves FEATURE-owned operational-flow knowledge without an
  owner; risks the same gap recurring on the next change.

---

# 8. Gap Closure

Ledger: **GAP-001 … GAP-010 — OPEN**, **OQ-001 … OQ-010 — OPEN**. No decisions
have been approved; every entry below requires stakeholder/architect input. When
a resolution is approved it will be recorded in place (Decision, Rationale,
Impact, Architecture Impact, Resolved By, Resolved Date) without renumbering,
and Planning Readiness will be updated accordingly.

## GAP-001 — Missing FEATURE artifact

**Status: OPEN**

### Decision

Pending. No target operational-flow definition has been approved. Decision
options recorded in §7 (OQ-009).

### Rationale

The requested change is a change to operational flow and navigation hierarchy,
which is FEATURE-owned knowledge; no FEATURE artifact exists.

### Impact

Architecture cannot be finalized against an authoritative FEATURE definition
until this is resolved.

---

## GAP-002 — Register Barcode entry point

**Status: OPEN**

### Decision

Pending. See OQ-001 and OQ-005.

### Rationale

`register` is currently reachable only from the Home quick action and the
standalone scan Not-Found flow; Barcode Registry has no Register action.

### Impact

Removing the current entries without a replacement breaks a stated-preserved
Barcode Registry capability.

---

## GAP-003 — Draft Delete entry point

**Status: OPEN**

### Decision

Pending. See OQ-002 and OQ-003.

### Rationale

Delete is a Draft-only action on Detail; routing Draft rows to Edit would remove
its only entry point.

### Impact

Draft delete capability (BR-019) would regress unless relocated.

---

## GAP-004 — Home structure

**Status: OPEN**

### Decision

Pending. See OQ-004, OQ-010.

### Rationale

Current Home is a feature launcher; the target is a work launcher.

### Impact

HomeScreen and its navigation wiring change; prominence decisions depend on the
OQ-006 scope boundary.

---

## GAP-005 — Return Orders list as a work queue

**Status: OPEN**

### Decision

Pending. See OQ-002, OQ-008.

### Rationale

Current list is flat and always opens Detail; the target adds date grouping and
Draft/Synced-specific routing.

### Impact

List layout and row routing change; requires a date-grouping decision and a
Draft-routing decision.

---

## GAP-006 — Capture layout re-prioritization

**Status: OPEN**

### Decision

Pending. See OQ-007.

### Rationale

The continuous capture surface already exists; the substantive change is
structural emphasis of the item region and de-emphasis of Salesman/Driver/Notes.

### Impact

Capture screen structure changes; may require ViewModel/state adjustments
depending on OQ-007.

---

## GAP-007 — Architecture artifact inconsistency

**Status: OPEN**

### Decision

Pending. Architecture review is owned by the Architect and follows approval of
the target flow.

### Rationale

`RETURN-ORDER-ARCHITECTURE.md` and the Barcode Registry UX/architecture
artifacts fix the navigation this request changes.

### Impact

Those artifacts must be updated before planning so knowledge and code stay
synchronized.

---

## GAP-008 — Structure vs visual styling boundary

**Status: OPEN**

### Decision

Pending. See OQ-006.

### Rationale

Prominence/emphasis requirements are partly visual; the boundary with the
separate UI design change request is undefined.

### Impact

Scope and ownership of prominence requirements must be fixed to avoid conflict.

---

## GAP-009 — Duplicate Synchronization route

**Status: OPEN**

### Decision

Pending. See OQ-004.

### Rationale

The target adds a sync status card link while a Synchronization item may remain
under More.

### Impact

Minor navigation redundancy.

---

## GAP-010 — Status filter set

**Status: OPEN**

### Decision

Pending. See OQ-008.

### Rationale

The target mockup shows only Draft/Synced; the current list also has `Semua`.

### Impact

Minor filter-control change.

---

## OQ-001 … OQ-010

All open questions remain **OPEN**; their dispositions are recorded in §3 and
§7 above. None has an approved decision.

---

# 9. Planning Readiness

## Readiness Checklist

- [ ] All critical gaps resolved
- [ ] All required decisions recorded
- [ ] All blocking open questions resolved
- [ ] Architecture can be finalized or updated

## Status

NOT-READY

## Notes

Blocking items:

- **GAP-002 / OQ-001 / OQ-005** — the fate of the standalone `scan` route and
  the Register Barcode entry point must be decided before the navigation graph
  can be finalized.
- **GAP-003 / OQ-002 / OQ-003** — Draft detail/delete reachability must be
  decided before the list → detail/edit routing can be finalized.
- **GAP-001 / OQ-009** — the workflow-Discovery input for the BGud Return Order
  operational flow must be complete (FEATURE artifact or an explicit waiver).
- **GAP-007** — the Architect must review and update the affected architecture
  artifacts.

Non-blocking but required for a coherent target:

- OQ-004 (sync card / More duplication), OQ-006 (structure vs styling
  boundary), OQ-007 (ViewModels/repositories change permission), OQ-008
  (`Semua` filter), OQ-010 (New Return entry).

The Analyst maintains this checklist and will keep Status `NOT-READY` while
blocking gaps remain. Only the Architect sets Status to `READY-FOR-PLANNING`
once the gaps and questions above are resolved and the architecture has been
updated. `READY-FOR-PLANNING` will mean feasibility is sufficiently resolved for
the Architecture skill to finalize or update the target architecture; it does
not mean architecture is complete or that the architecture step may be skipped.

---

# 10. References

Referenced artifacts:

- DOMAIN: `docs/work/return-order/RETURN-ORDER-DOMAIN.md`
- FEATURE: none (see GAP-001)
- ISSUE: `docs/issues/BGUD-RETURN-ORDER-NAV-ISSUE.md`
- ARCHITECTURE (current, owned by the Architect):
  `docs/work/return-order/RETURN-ORDER-ARCHITECTURE.md` §11–§13, §20;
  `docs/work/barcode-registry/BARCODE-REGISTRY-ARCHITECTURE.md`;
  `docs/work/barcode-registry/BARCODE-REGISTRY-UX-BLUEPRINT.md` §4, §6
- Precedent assessments: `docs/work/return-order/RETURN-ORDER-FEASIBILITY-ASSESSMENT.md`;
  `docs/work/bgud-google-signin/BGUD-GOOGLE-SIGNIN-FEASIBILITY-ASSESSMENT.md`
- Foundation: `docs/foundation/PRODUCT.md`, `docs/foundation/DOMAIN.md`,
  `docs/foundation/LANDSCAPE.md`, `docs/foundation/WORKFLOW.md`

Referenced codebase locations:

- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/Navigation.kt`
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/HomeScreen.kt`
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/ReturnOrderListScreen.kt`
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/CreateReturnOrderScreen.kt`
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/EditReturnOrderScreen.kt`
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/ReturnOrderDetailScreen.kt`
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/ScanScreen.kt`
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/BarcodeRegistryScreen.kt`
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/RegisterScreen.kt`
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/SynchronizationScreen.kt`
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/SettingsScreen.kt`
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/component/BarcodeScannerView.kt`
- `src/BGud/app/src/main/java/com/elsasa/bgud/viewmodel/ReturnOrderListViewModel.kt`
- `src/BGud/app/src/main/java/com/elsasa/bgud/dao/ReturnOrderDao.kt`
