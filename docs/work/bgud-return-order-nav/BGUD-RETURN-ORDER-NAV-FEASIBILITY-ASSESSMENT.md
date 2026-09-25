---
Title: BGud — Return Order Navigation Restructure — Feasibility Assessment
Code: BGUD-RETURN-ORDER-NAV-001
Artifact: FEASIBILITY-ASSESSMENT
Version: 1.5
LastUpdated: 2026-09-25
Status: READY-FOR-PLANNING
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
  (see GAP-001). **CLOSED (2026-09-25):** the FEATURE requirement is waived for
  this change; the ISSUE itself is the accepted Discovery input for the
  requested navigation/operational-flow change.
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
| GAP-001 | MAJOR | **No FEATURE artifact defines the target operational flow.** — **CLOSED (2026-09-25).** The request changes BGud's operational flow and navigation hierarchy ("Return Order is the daily operational workflow"), which is FEATURE-owned knowledge, yet no FEATURE artifact exists for the BGud Return Order capability. Only DOMAIN (`RETURN-ORDER-DOMAIN.md`) and ARCHITECTURE (`RETURN-ORDER-ARCHITECTURE.md`) exist. Architecture therefore had no authoritative business-outcome/operational-flow input for the target navigation. **Resolution:** waived — the ISSUE plus the DOMAIN is accepted as the Discovery input; see §8 GAP-001. |
| GAP-002 | CRITICAL | **Removing the Home barcode actions / standalone `scan` breaks the Register Barcode entry.** `register` is reachable only from the Home `Register Barcode` quick action (`Navigation.kt:246`) and the `scan` Not-Found flow (`Navigation.kt:375`). `BarcodeRegistryScreen` exposes only search + Edit and has **no Register action** (`BarcodeRegistryScreen.kt:167-183`). The requested IA removes the Home actions and makes scanning contextual; unless a replacement entry is defined, "Barcode Registry … Register … functionality preserved" cannot hold. **CLOSED (2026-09-25):** Register Barcode entry point moved into Barcode Registry; Home-level action removed; standalone `scan` no longer required as a navigation entry point for barcode registration (see §8 GAP-002). |
| GAP-003 | CRITICAL | **Routing Draft rows directly to Edit removes the only Delete entry point.** — **CLOSED (2026-09-25).** Delete is a Draft-only action that lives on Detail (`ReturnOrderDetailScreen.kt:229-239`), and Edit is reachable only from Detail (`Navigation.kt:321-330`). If a Draft row opens Edit/Resume directly, the Draft Detail surface (and therefore Delete) becomes unreachable unless the delete action is relocated. DOMAIN BR-019/BR-020 (Draft delete allowed, Synced delete prohibited) must be preserved. **Resolution:** Draft Detail screen removed; Delete action relocated from Detail to Edit; see §8 GAP-003. |
| GAP-004 | MAJOR | **Home is a feature launcher; the target is a work launcher.** — **CLOSED (2026-09-25).** Current Home exposes Scan/Search/Register/Return Order with near-equal weight, a non-clickable sync card, and a flat Navigation list. The target requires New Return as the most prominent action, Return Orders as the primary destination, a clickable sync status card, and a "More" group (Barcode Registry / Synchronization / Settings). Evidence: `HomeScreen.kt:98-150`. **Resolution:** Home transitions from feature launcher to work launcher; see §8 GAP-004. |
| GAP-005 | MAJOR | **Return Orders list becomes a work queue.** — **CLOSED (2026-09-25).** The target adds date grouping (e.g. "TODAY"), keeps recent-first, preserves search, keeps New Return accessible, and routes Draft → Edit/Resume while Synced → read-only Detail. The current list is a flat, ungrouped list whose rows always open Detail. Ordering is already recent-first. Evidence: `ReturnOrderListScreen.kt:160-251`, `ReturnOrderDao.kt`. **Resolution:** see §8 GAP-005. |
| GAP-006 | MAJOR | **Capture is already a continuous form; the real change is layout re-prioritization.** `CreateReturnOrderScreen` already embeds the scanner and identifies items locally with consecutive scans, so the requested "continuous transaction surface" largely exists. The substantive change is structural emphasis: item entry dominates while Salesman/Driver/Notes are de-emphasized, and each row shows Item, Quantity, Unit, Return Type. The ISSUE itself (Note 2) leaves this confirmation open. Evidence: `CreateReturnOrderScreen.kt:136-477`. — **CLOSED (2026-09-25):** the capture workflow is retained; only layout/structural re-prioritization applies; see §8 GAP-006. |
| GAP-007 | MAJOR | **Existing architecture artifacts become inconsistent.** The approved architecture fixes the current navigation and layouts that this request contradicts: `RETURN-ORDER-ARCHITECTURE.md` §11.1/§12.2/§13.1 (List → Detail → Edit; Home → return_order_list only) and `BARCODE-REGISTRY-UX-BLUEPRINT.md` §4/§6 and `BARCODE-REGISTRY-ARCHITECTURE.md` (Home quick actions; Barcode Registry branch). ARCHITECTURE is owned by the Architect; it must be reviewed and updated so knowledge and code stay synchronized. — **CLOSED (2026-09-25):** the affected architecture artifacts shall be updated by the Architect to reflect the approved navigation and workflow changes; see §8 GAP-007. |
| GAP-008 | MAJOR | **Scope boundary between "structure" and deferred visual styling is not fixed.** — **CLOSED (2026-09-25):** the boundary is fixed; this change request is limited to Navigation and Layout (navigation hierarchy, screen routing, screen ownership, information architecture, content grouping, component placement, layout structure, workflow prioritization, visibility of operational and administrative actions). Visual styling (colors, typography, iconography, branding, component styling, elevation, shadows, animations, visual emphasis through color or styling treatment, design-system refinements) is explicitly excluded and deferred to a separate UI/UX Styling change request after Navigation and Layout implementation is complete. See §8 GAP-008. |
| GAP-009 | MINOR | **Duplicate route to Synchronization.** — **CLOSED (2026-09-25).** The Home Synchronization Status card becomes the authoritative navigation entry point to Synchronization; the separate Synchronization menu item is removed from More. See §8 GAP-009. |
| GAP-010 | MINOR | **Filter set drift.** — **CLOSED (2026-09-25).** The target mockup shows only `[Draft] [Synced]`, while the current list has `Semua` / `Draft` / `Synced`; "preserve search" is stated but the fate of the `Semua` chip is not. Evidence: `ReturnOrderListScreen.kt:111-134`. **Resolution:** existing status filters retained (Semua, Draft, Synced); default remains Semua; see §8 GAP-010. |

Severity key: CRITICAL — blocks target architecture from being finalized;
MAJOR — significant structure/flow decision required; MINOR — small,
localized decision.

---

# 4. Open Questions

| ID | Question | Impact |
|------|------|------|
| OQ-001 | What is the fate of the standalone `scan` route and `ScanScreen` — removed entirely, retained as a secondary tool under "More", or retained only as an internal capture step? What happens to the scan Not-Found → `register?barcode=` path (`Navigation.kt:375`)? | **CLOSED (2026-09-25)** — see §8 GAP-002. |
| OQ-002 | Does Return Order Detail remain reachable for a **Draft** order at all (e.g. to view before editing), or do Draft rows navigate only to Edit/Resume? | **CLOSED (2026-09-25)** — see §8 GAP-005. Draft rows navigate directly to Edit/Resume; Detail is not the entry point for Draft from the list. |
| OQ-003 | If Draft rows skip Detail, where does the Draft **Delete** action live (Edit screen action, long-press, swipe)? | **CLOSED (2026-09-25)** — see §8 GAP-003. Delete is relocated from Detail to the Edit/Resume screen; the Draft Detail screen is removed entirely. |
| OQ-004 | Is the Home sync status card the entry to Synchronization, and is the existing "Synchronization" item in "More" retained alongside it or removed? | **CLOSED (2026-09-25)** — the Home Synchronization Status card is the authoritative navigation entry point to Synchronization; the separate Synchronization menu item is removed from More. See §8 GAP-009. |
| OQ-005 | How is **Register Barcode** reached after the top-level Home actions and (possibly) standalone `scan` are removed — a new Register action inside Barcode Registry, or only from a retained scan Not-Found flow? | **CLOSED (2026-09-25)** — see §8 GAP-002. |
| OQ-006 | Which prominence/emphasis requirements belong to this structure change versus the separate UI design change request? | **CLOSED (2026-09-25)** — see §8 GAP-008. This change is limited to Navigation and Layout; visual styling is explicitly excluded and deferred to a separate UI/UX Styling change request after Navigation and Layout implementation is complete. |
| OQ-007 | How strictly must the reporter's "leave ViewModels, repositories, persistence, networking, and domain logic unchanged" hold? May ViewModels be adjusted where the requested structure requires it (date grouping, Draft resume, Delete relocation, capture state)? | **CLOSED (2026-09-25)** — see §8 GAP-006. |
| OQ-008 | Is the `Semua` status filter chip retained, or does the list show only `Draft` / `Synced`? | **CLOSED (2026-09-25)** — see §8 GAP-010. |
| OQ-009 | Is a FEATURE artifact for the BGud Return Order operational flow required before architecture, or does the detailed ISSUE serve as the Discovery input? | **CLOSED (2026-09-25) — see §8 GAP-001.** Answer: waived — the ISSUE (`BGUD-RETURN-ORDER-NAV-ISSUE.md`) together with `RETURN-ORDER-DOMAIN.md` is the authoritative Discovery input; no FEATURE artifact is created for this change. |
| OQ-010 | Does Home's "New Return" reuse the existing full-screen `return_order_create` route, or become a distinct/inline capture entry? | **CLOSED (2026-09-25)** — see §8 OQ-010. Home's New Return reuses the existing full-screen `return_order_create` route; no new capture route, inline capture, modal workflow, wizard flow, or embedded Home-screen transaction surface is introduced. |

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
| RISK-001 | Register Barcode becomes unreachable after the Home actions and standalone `scan` are removed | High — a preserved capability regresses | **CLOSED (2026-09-25):** resolved — the Register Barcode entry point is moved into Barcode Registry (see §8 GAP-002). |
| RISK-002 | Draft Delete capability regresses when Draft rows bypass Detail | High — violates BR-019/BR-020 as experienced by operators | **CLOSED (2026-09-25):** resolved — Delete relocated from Detail to the Edit/Resume screen; Draft Detail screen removed from the navigation graph; see §8 GAP-003. |
| RISK-003 | Structure and visual styling overlap/conflict with the separate UI design change request | Medium — duplicated or contradictory changes to the same screens | **CLOSED (2026-09-25):** boundary fixed in §8 GAP-008. This change is limited to Navigation and Layout; visual styling is explicitly excluded and deferred to a separate UI/UX Styling change request after Navigation and Layout implementation is complete. |
| RISK-004 | "No ViewModel changes" constraint conflicts with the requested structure | Medium — either the constraint is violated or the structure is left incomplete | **CLOSED (2026-09-25):** resolved — OQ-007 closed via §8 GAP-006; the capture workflow and its behavior are retained and the implementation surface is bounded to the approved navigation/layout structure. |
| RISK-005 | Knowledge drift: architecture/UX artifacts still describe the pre-change navigation | Medium — implementers may follow stale navigation contracts | **CLOSED (2026-09-25):** resolved — GAP-007 closed; the affected architecture artifacts are updated by the Architect as an assigned Architecture-phase activity before planning starts (see §8 GAP-007). |
| RISK-006 | Removing the top-level Scan quick action increases steps for ad-hoc item lookup | Medium — operator efficiency and muscle memory | Keep scanning fast inside capture and preserve manual search; confirm Barcode Registry search covers lookup. |
| RISK-007 | Two navigation entries to Synchronization (card + More item) confuse operators | Low — minor UX inconsistency | **CLOSED (2026-09-25):** resolved — Home Synchronization Status card becomes the authoritative navigation entry point; Synchronization menu item removed from More (see §8 GAP-009). |
| RISK-008 | Home grouping ("work launcher") is interpreted as a redesign beyond navigation and pulls in visual styling | Medium — scope creep across change requests | **CLOSED (2026-09-25):** resolved — scope boundary fixed in §8 GAP-008 (OQ-006); visual styling excluded and deferred to a separate UI/UX Styling change request. |

---

# 7. Recommendations

Alternative solution directions only. **No final decision is recorded here** —
decisions belong in §8 Gap Closure after stakeholder/architecture input.

*Status note (v1.5):* all gaps and open questions are resolved and recorded in
§8 — **GAP-001 / OQ-009**, **GAP-002 / OQ-001 / OQ-005**, **GAP-003 / OQ-002 /
OQ-003**, **GAP-004**, **GAP-005**, **GAP-006 / OQ-007**, **GAP-007**,
**GAP-008 / OQ-006**, **GAP-009 / OQ-004**, **GAP-010 / OQ-008**, and
**OQ-010**. Selected directions: the FEATURE artifact requirement is waived for
this change (ISSUE + DOMAIN accepted as the Discovery input); the Register
Barcode entry point moves into Barcode Registry; the Draft Delete action is
relocated from Detail to Edit with the Draft Detail screen removed; and Home's
New Return reuses the existing full-screen `return_order_create` route. The
options below are the pre-decision analysis record; no option remains open.

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
Discovery for this change — **SELECTED DIRECTION (§8 GAP-001, 2026-09-25)**

- Advantages: faster; the ISSUE is unusually detailed and adds no business rules.
- Disadvantages: leaves FEATURE-owned operational-flow knowledge without an
  owner; risks the same gap recurring on the next change.

**Selected.** The ISSUE (`BGUD-RETURN-ORDER-NAV-ISSUE.md`) together with
`RETURN-ORDER-DOMAIN.md` is the authoritative Discovery input for this change
and no FEATURE artifact is created. The waiver is specific to this change and
does not remove the general expectation that a FEATURE artifact should exist
when a change introduces or materially defines a new business capability or
operational feature.

---

# 8. Gap Closure

Ledger: **GAP-001 — CLOSED (2026-09-25)**, **GAP-002 — CLOSED (2026-09-25)**,
**GAP-003 — CLOSED (2026-09-25)**,
**GAP-004 — CLOSED (2026-09-25)**, **GAP-005 — CLOSED (2026-09-25)**, **GAP-006 — CLOSED (2026-09-25)**,
**GAP-007 — CLOSED (2026-09-25)**, **GAP-008 — CLOSED (2026-09-25)**, **GAP-009 — CLOSED (2026-09-25)**, **GAP-010 — CLOSED (2026-09-25)**,
**OQ-009 — CLOSED (2026-09-25)**, **OQ-001, OQ-005 — CLOSED (2026-09-25)**,
**OQ-002, OQ-003, OQ-004 — CLOSED (2026-09-25)**, **OQ-008 — CLOSED (2026-09-25)**,
**OQ-006, OQ-007 — CLOSED (2026-09-25)**, **OQ-010 — CLOSED (2026-09-25)**. Eleven decisions have been
approved (§8 GAP-001, §8 GAP-002, §8 GAP-003, §8 GAP-004, §8 GAP-005, §8 GAP-006, §8 GAP-007, §8 GAP-008, §8 GAP-009, §8 GAP-010, §8 OQ-010).
When a further resolution is approved it will be recorded in place (Decision,
Rationale, Impact, Architecture Impact, Resolved By, Resolved Date) without
renumbering, and Planning Readiness will be updated accordingly.

## GAP-001 — Missing FEATURE artifact

**Status: CLOSED**

### Decision

**Waive the FEATURE artifact requirement for this change.**

The existing ISSUE `BGUD-RETURN-ORDER-NAV-ISSUE.md`, together with the
authoritative `RETURN-ORDER-DOMAIN.md`, is accepted as the Discovery input for
this change. No separate FEATURE artifact will be created for
`BGUD-RETURN-ORDER-NAV-001`.

The ISSUE is sufficiently detailed to define the requested operational-flow and
navigation change, while the DOMAIN remains authoritative for business
capabilities, business rules, and the Return Order lifecycle.

### Rationale

This change does not introduce a new business capability or new business rules.
It restructures the existing BGud Return Order experience: navigation hierarchy,
screen flow, capture layout, Draft/Synced entry behavior, and placement of
supporting functions. The ISSUE already describes the intended operational
outcome and interaction flow in sufficient detail for architecture work.

Creating a FEATURE artifact would largely duplicate information already present
in the ISSUE and DOMAIN without adding meaningful decision-making value.

This waiver is specific to this change and does not remove the general
expectation that a FEATURE artifact should exist when a change introduces or
materially defines a new business capability or operational feature.

### Impact

* The Architecture phase may use the ISSUE as the authoritative target-flow
  input for this change.
* `RETURN-ORDER-DOMAIN.md` remains the authority for business terminology,
  rules, capabilities, and lifecycle.
* `BGUD-RETURN-ORDER-NAV-ISSUE.md` becomes the authoritative Discovery input
  for the requested navigation/operational-flow change.
* No new FEATURE artifact is required before architecture can proceed.
* GAP-001 no longer blocks planning readiness.

### Architecture Impact

Architecture must explicitly reference the ISSUE as the target operational-flow
input and must remain consistent with the existing DOMAIN rules.

The Architect must not introduce new business behavior while translating the
requested flow into the target navigation and screen architecture.

### Resolved By

Stakeholder / Product Owner

### Resolved Date

2026-09-25

---

## GAP-002 — Register Barcode entry point

**Status: CLOSED**

### Decision

Move the **Register Barcode** entry point into **Barcode Registry**.

The Home screen will no longer expose a top-level Register Barcode action.

Barcode Registration capability remains preserved through Barcode Registry, which becomes the authoritative destination for barcode administration activities:

* Search Barcode
* Register Barcode
* Edit Barcode

The standalone `scan` route is no longer required as a navigation entry point for barcode registration.

### Rationale

The requested information architecture establishes Return Order as the primary operational workflow and demotes barcode-management activities to secondary administrative functions.

Register Barcode is not part of the normal Return Order workflow. It is a maintenance/administrative activity belonging to Barcode Registry.

Placing Register Barcode inside Barcode Registry aligns navigation ownership with capability ownership:

* Return Order owns return processing.
* Barcode Registry owns barcode management.
* Home focuses on operational work rather than administrative functions.

This preserves all existing barcode-registration capability while removing unnecessary competition with the Return Order workflow.

### Impact

Required changes:

* Add a **Register Barcode** action within Barcode Registry.
* Remove the Home-level Register Barcode action.
* Barcode Registry becomes the single navigation destination for barcode administration activities.
* Barcode registration capability remains available to users.

No domain, API, database, synchronization, or business-rule changes are required.

### Architecture Impact

Navigation ownership changes:

Current:

```
Home
 ├─ Register Barcode
 └─ Barcode Registry
```

Target:

```
Home
 └─ More
      └─ Barcode Registry
            ├─ Search
            ├─ Register
            └─ Edit
```

Barcode Registry becomes the authoritative entry point for barcode-management operations.

### Resolved By

Stakeholder / Product Owner

### Resolved Date

2026-09-25

---

## GAP-003 — Draft Delete entry point

**Status: CLOSED**

### Decision

The **Draft Detail screen is removed** from the Draft workflow.

When a Return Order has status **Draft**:

* Selecting the row opens **Edit / Resume Return Order** directly.
* The Draft Detail screen is no longer reachable.
* The Edit screen becomes the authoritative maintenance surface for Draft orders.
* The **Delete Draft** action is relocated from Detail to Edit.

When a Return Order has status **Synced**:

* Selecting the row opens the existing read-only Detail screen.
* Delete remains prohibited.

### Rationale

Draft orders represent unfinished work.

The primary operator intent for a Draft order is to continue working on it, not to
inspect it through an intermediate Detail screen.

Routing Draft orders directly to Edit reduces navigation steps and aligns with the
objective of making Return Order the primary operational workflow.

Since DOMAIN BR-019 requires Draft orders to remain deletable, the Delete action is
moved to the Edit screen.

The Draft Detail screen becomes unnecessary and is removed from the Draft
navigation graph.

### Impact

Required changes:

* Draft row → Edit / Resume.
* Synced row → Detail (unchanged).
* Delete action removed from Detail.
* Delete action added to Edit.
* Delete available only when status = Draft.
* Synced orders remain non-editable and non-deletable.

### Architecture Impact

Current:

```text
Return Order List
    ↓
Draft Detail
    ├─ Edit
    └─ Delete
```

Target:

```text
Return Order List
    ↓
Draft Edit / Resume
    ├─ Save
    ├─ Continue Editing
    └─ Delete
```

Synced flow:

```text
Return Order List
    ↓
Synced Detail (Read Only)
```

Draft Detail is removed from the navigation graph.

### Compliance

* BR-019 (Draft delete allowed): PRESERVED
* BR-020 (Synced delete prohibited): PRESERVED

### Resolved By

Stakeholder / Product Owner

### Resolved Date

2026-09-25

---

## GAP-004 — Home structure

**Status: CLOSED**

### Decision

BGud Home shall transition from a **feature launcher** to a **work launcher**.

The Home screen will prioritize the operator's primary daily activity: Return Order
processing.

Target Home structure:

```text
Home
│
├─ New Return
│
├─ Return Orders
│
├─ Synchronization Status
│    └─ Open Synchronization
│
└─ More
     ├─ Barcode Registry
     └─ Settings
```

### Rationale

The change request explicitly establishes Return Order as the primary operational
workflow of BGud.

The current Home treats all features with approximately equal importance:

* Scan Barcode
* Search Barcode
* Register Barcode
* Return Order

This reflects a feature-centric navigation model.

The requested navigation adopts a workflow-centric model where Home serves as an
operational starting point rather than a catalog of system capabilities.

Operators should be able to:

1. Start a new Return Order immediately.
2. Resume or review existing Return Orders.
3. See synchronization status without leaving Home.
4. Access secondary administrative functions when required.

Administrative and supporting functions remain available but are intentionally
de-emphasized.

### Impact

Required changes:

* Remove Home-level Scan Barcode action.
* Remove Home-level Search Barcode action.
* Remove Home-level Register Barcode action.
* Promote New Return as the primary action.
* Promote Return Orders as the primary navigation destination.
* Make Synchronization Status card clickable.
* Introduce a More section containing:

  * Barcode Registry
  * Synchronization
  * Settings

No business-rule changes are introduced.

No domain, API, database, synchronization, or lifecycle changes are required.

### Architecture Impact

Current:

```text
Home
├─ Scan Barcode
├─ Search Barcode
├─ Register Barcode
├─ Return Order
├─ Synchronization
└─ Settings
```

Target:

```text
Home
├─ New Return
├─ Return Orders
├─ Synchronization Status
└─ More
     ├─ Barcode Registry
     └─ Settings
```

The navigation hierarchy becomes aligned with the primary operational workflow
while preserving access to all existing capabilities.

### Resolved By

Stakeholder / Product Owner

### Resolved Date

2026-09-25

---

## GAP-005 — Return Orders list as a work queue

**Status: CLOSED**

### Decision

The Return Orders screen shall evolve from a record list into an operational work queue.

The target behavior is:

* Orders remain sorted by most recent first.
* Orders are grouped by date sections (e.g. TODAY, YESTERDAY, EARLIER).
* Search capability remains unchanged.
* New Return remains directly accessible from the screen.
* Draft orders open Edit / Resume.
* Synced orders open read-only Detail.

### Rationale

The requested navigation establishes Return Order as the primary operational workflow.

In that workflow, operators primarily need to:

* continue unfinished work,
* verify recently completed work,
* locate recent transactions quickly,
* and create new transactions.

A work queue presentation better supports these objectives than a flat historical list.

Recent-first ordering already exists and remains appropriate.

Date grouping improves operator scanning and aligns the list with daily operational usage patterns without changing business behavior.

Status-based routing aligns navigation with operator intent:

* Draft = continue work.
* Synced = review completed work.

### Impact

Required changes:

* Add date-based grouping to the list.
* Preserve existing recent-first ordering.
* Preserve existing search functionality.
* Preserve New Return entry point.
* Change row navigation:

  * Draft → Edit / Resume.
  * Synced → Read-Only Detail.
* Retain existing status indicators.

No business rules change.

No domain, API, database, synchronization, or lifecycle changes are required.

### Architecture Impact

Current:

```text
Return Orders
│
├─ Search
│
├─ Flat List
│    ├─ Draft  → Detail
│    └─ Synced → Detail
│
└─ New Return
```

Target:

```text
Return Orders
│
├─ Search
│
├─ TODAY
│    ├─ Draft  → Edit / Resume
│    └─ Synced → Detail
│
├─ YESTERDAY
│    ├─ Draft  → Edit / Resume
│    └─ Synced → Detail
│
├─ EARLIER
│    ├─ Draft  → Edit / Resume
│    └─ Synced → Detail
│
└─ New Return
```

The screen becomes the operational queue for Return Order work while preserving all existing Return Order capabilities.

### Resolved By

Stakeholder / Product Owner

### Resolved Date

2026-09-25

---

## GAP-006 — Return Order Capture Layout Re-prioritization

**Status: CLOSED**

### Decision

The existing Return Order capture workflow is retained.

The current `CreateReturnOrderScreen` already provides the required continuous transaction surface, including:

* single-screen capture,
* embedded barcode scanning,
* local item lookup,
* consecutive item scanning,
* existing transaction lifecycle.

Therefore, this change does **not** introduce a new Return Order capture workflow.

The scope of this gap is limited to **structural layout and information hierarchy**:

* Item entry becomes the primary working area.
* The item list becomes a primary part of the transaction surface.
* Salesman becomes secondary information.
* Driver becomes secondary information.
* Notes become secondary information.
* Item rows clearly expose Item, Quantity, Unit, and Return Type.

Visual styling is explicitly governed by **GAP-008** and is not part of this decision.

### Rationale

The current implementation already satisfies the functional intent of a continuous transaction surface.

The requested change is therefore a reorganization of the existing screen rather than a workflow redesign.

Creating a new capture flow would add unnecessary complexity and duplicate behavior that already exists.

### Impact

The implementation shall:

* retain the existing capture workflow;
* retain the existing barcode-scanning behavior;
* retain the existing local/offline lookup behavior;
* reorganize the screen structure and information hierarchy.

The implementation shall not introduce:

* a new capture workflow;
* a wizard;
* additional transaction steps;
* new business rules;
* API changes;
* database changes;
* synchronization changes.

### Scope Boundary

This decision covers **structure and layout only**.

The following remain outside this gap:

* colors,
* typography,
* iconography,
* component styling,
* spacing treatment,
* visual emphasis,
* branding,
* design-system refinement.

Those items are deferred to the separate UI/UX Styling change defined by GAP-008.

### Architecture Impact

The architecture shall describe the target capture structure as:

```text
Transaction Context
├─ Warehouse
└─ Customer

Item Entry / Item List
├─ Barcode Scan
├─ Item Search
├─ Quantity
├─ Unit
└─ Return Type

Additional Information
├─ Salesman
├─ Driver
└─ Notes
```

The existing transaction behavior remains unchanged.

### Resolved By

Stakeholder / Product Owner

### Resolved Date

2026-09-25

---

## GAP-007 — Architecture Artifact Consistency

**Status: CLOSED**

### Decision

The existing architecture artifacts are acknowledged as describing the **pre-change architecture** and therefore require update.

This does not block feasibility.

The approved decisions from this feasibility assessment are sufficient to establish the target direction. The Architect shall update the affected architecture artifacts during the Architecture phase.

Affected artifacts:

* `docs/work/return-order/RETURN-ORDER-ARCHITECTURE.md`
* `docs/work/barcode-registry/BARCODE-REGISTRY-ARCHITECTURE.md`
* `docs/work/barcode-registry/BARCODE-REGISTRY-UX-BLUEPRINT.md`

### Rationale

The architecture documents are not expected to remain unchanged when an approved change intentionally modifies navigation, routing, and screen structure.

The inconsistency is therefore a **knowledge-update task**, not an unresolved product or feasibility decision.

No additional stakeholder decision is required to determine whether the architecture should reflect the approved target state.

### Impact

The Architect must update the affected artifacts to reflect the decisions already approved in this assessment, including:

* Home navigation hierarchy;
* New Return entry;
* Return Orders entry;
* Draft → Edit / Resume;
* Synced → Read-only Detail;
* Draft Delete relocation;
* Barcode Registry navigation;
* Register Barcode entry point;
* Synchronization navigation;
* Return Order list structure;
* Return Order capture layout structure.

The updated architecture becomes the authoritative technical input for implementation planning.

### Important Boundary

Closing GAP-007 does **not** mean the architecture documents have already been updated.

It means:

> **No unresolved feasibility decision remains regarding those artifacts.**

The remaining work is an explicitly assigned Architecture-phase activity.

### Planning Impact

GAP-007 is **not a feasibility blocker**.

However, implementation planning shall use the **updated architecture artifacts** as its authoritative technical input.

Therefore:

```text
Feasibility
    → GAP-007 CLOSED

Architecture
    → Update affected artifacts

Planning
    → Use updated architecture
```

### Resolved By

Stakeholder / Product Owner

### Resolved Date

2026-09-25

---

## GAP-008 — Structure vs Visual Styling Boundary

**Status: CLOSED**

### Decision

This change request is limited to **Navigation and Layout**.

Visual styling is explicitly excluded from scope and will be addressed by a separate UI/UX Styling change request after Navigation and Layout implementation has been completed.

### Scope of This Change

Included:

* Navigation hierarchy
* Screen routing
* Screen ownership
* Information architecture
* Content grouping
* Component placement
* Layout structure
* Workflow prioritization
* Visibility of operational actions
* Visibility of administrative actions

Examples:

* New Return becomes the primary Home action.
* Return Orders becomes the primary operational destination.
* Barcode Registry moves under More.
* Synchronization Status becomes a navigation entry.
* Item Entry becomes the dominant region of Return Order capture.
* Salesman, Driver, and Notes become secondary regions.

### Out of Scope

Deferred to a future UI/UX Styling issue:

* Colors
* Typography
* Iconography
* Branding
* Component styling
* Elevation
* Shadows
* Animations
* Visual emphasis through color treatment
* Visual emphasis through styling treatment
* Design-system refinements

Examples:

* Primary button colors
* Card styling
* Chip styling
* Font sizing
* Visual weight of labels
* Theme adjustments

### Rationale

The objective of this change request is to establish the correct operational workflow and navigation structure.

Navigation and layout define how operators move through the application and where information is located.

Visual styling determines how those structures are visually presented.

Separating these concerns reduces implementation risk and allows operational workflow validation before investing in visual refinement.

### Impact

The implementation team shall focus on:

* Navigation changes
* Routing changes
* Layout changes
* Information architecture changes

The implementation team shall not introduce styling redesign as part of this change unless required for technical compatibility.

### Architecture Impact

Architecture artifacts shall describe:

* navigation structure,
* routing behavior,
* screen hierarchy,
* layout organization,

but shall not prescribe visual styling decisions beyond what is necessary to explain structure.

A future UI/UX Styling issue may update presentation details without altering the approved navigation and workflow architecture.

### Resolved By

Stakeholder / Product Owner

### Resolved Date

2026-09-25

---

## GAP-009 — Duplicate Synchronization route

**Status: CLOSED**

### Decision

The Home Synchronization Status card becomes the authoritative navigation entry point to the Synchronization screen.

The separate **Synchronization** menu item shall be removed from the **More** section.

Target Home structure:

```text
Home
│
├─ New Return
├─ Return Orders
├─ Synchronization Status
│    └─ Open Synchronization
│
└─ More
     ├─ Barcode Registry
     └─ Settings
```

### Rationale

The Synchronization Status card already exists to communicate synchronization state.

Once the card becomes interactive, it naturally serves as both:

* synchronization status indicator, and
* synchronization navigation entry point.

Maintaining a second navigation entry to the same destination provides no additional operational value and introduces unnecessary duplication.

The Home screen should expose a single clear path to each operational function whenever possible.

### Impact

Required changes:

* Synchronization Status card becomes clickable.
* Synchronization screen remains unchanged.
* Synchronization menu item removed from More.

No business rules change.

No domain changes are required.

No API changes are required.

No database changes are required.

No synchronization behavior changes are required.

### Architecture Impact

Current:

```text
Home
├─ Synchronization Status
├─ Synchronization
└─ Settings
```

Target:

```text
Home
├─ Synchronization Status
│    └─ Open Synchronization
└─ Settings
```

Synchronization remains fully accessible while eliminating redundant navigation paths.

### Resolved By

Stakeholder / Product Owner

### Resolved Date

2026-09-25

---

## GAP-010 — Status filter set

**Status: CLOSED**

### Decision

The existing status filters shall be retained:

* Semua
* Draft
* Synced

The default selection remains **Semua**.

### Rationale

The Return Orders screen functions as an operational work queue containing both active and completed work.

While Draft and Synced filters support focused views, operators still require a complete view of all Return Orders.

The existing filter set already satisfies this requirement and introduces no operational confusion.

Removing the Semua filter would reduce visibility of the complete queue while providing no meaningful simplification.

### Impact

Required changes:

* Preserve the existing filter chips:

  * Semua
  * Draft
  * Synced
* Preserve existing search behavior.
* Preserve existing filtering behavior.

The status-based navigation behavior introduced by this change remains:

* Draft → Edit / Resume
* Synced → Read-Only Detail

### Architecture Impact

Current:

```text
[ Semua ] [ Draft ] [ Synced ]
```

Target:

```text
[ Semua ] [ Draft ] [ Synced ]
```

No change to filter structure is required.

The only behavioral change is row routing:

```text
Draft  → Edit / Resume
Synced → Detail
```

### Rationale for Default View

When the screen opens:

```text
Filter = Semua
Sort   = Most Recent First
```

This provides the broadest operational visibility and remains consistent with existing user behavior.

### Resolved By

Stakeholder / Product Owner

### Resolved Date

2026-09-25

---

## OQ-010 — New Return entry behavior

**Status: CLOSED**

### Decision

Home's **New Return** action shall reuse the existing full-screen `return_order_create` route.

No new capture route, inline capture experience, modal workflow, wizard flow, or embedded Home-screen transaction surface will be introduced.

### Rationale

The existing Create Return Order screen already satisfies the approved Return Order capture workflow:

* single-screen transaction capture,
* embedded barcode scanning,
* local item lookup,
* consecutive item entry,
* offline operation,
* existing Draft lifecycle.

The purpose of this change request is to improve navigation hierarchy and operational workflow prioritization, not to redesign the Return Order capture process.

Reusing the existing `return_order_create` route minimizes implementation effort, avoids duplicate transaction-entry experiences, and remains consistent with the approved decision recorded in GAP-006.

### Impact

Required navigation behavior:

```text
Home
 └─ New Return
       ↓
return_order_create
```

No new routes are required.

No ViewModel changes are required solely for Home navigation.

No domain, API, database, synchronization, or lifecycle changes are required.

### Architecture Impact

Current:

```text
Home
 └─ Return Order List
         └─ Create
```

Target:

```text
Home
 ├─ New Return
 │     ↓
 │  return_order_create
 │
 └─ Return Orders
       ↓
   return_order_list
```

The existing Create Return Order screen remains the authoritative transaction-entry surface.

### Resolved By

Stakeholder / Product Owner

### Resolved Date

2026-09-25

---

## OQ-001 … OQ-010

**OQ-010 — CLOSED (2026-09-25)** — Home's New Return reuses the existing full-screen `return_order_create` route; no new capture route, inline capture, modal workflow, wizard flow, or embedded Home-screen transaction surface is introduced; see §8 OQ-010.
**OQ-009 — CLOSED (2026-09-25)** — waived; see §8 GAP-001.
**OQ-001 — CLOSED (2026-09-25)** — Register Barcode moved into Barcode Registry;
standalone `scan` no longer required as a navigation entry point for barcode
registration; see §8 GAP-002.
**OQ-002 — CLOSED (2026-09-25)** — Draft rows navigate directly to Edit/Resume;
Detail is not the entry point for Draft from the list; see §8 GAP-005.
**OQ-003 — CLOSED (2026-09-25)** — Draft Delete relocated from Detail to Edit;
the Draft Detail screen is removed from the navigation graph; see §8 GAP-003.
**OQ-004 — CLOSED (2026-09-25)** — the Home Synchronization Status card is the
authoritative navigation entry point to Synchronization; the separate
Synchronization menu item is removed from More; see §8 GAP-009.
**OQ-005 — CLOSED (2026-09-25)** — see §8 GAP-002.
**OQ-006 — CLOSED (2026-09-25)** — see §8 GAP-008.
**OQ-007 — CLOSED (2026-09-25)** — see §8 GAP-006.
**OQ-008 — CLOSED (2026-09-25)** — the existing status filters (Semua, Draft, Synced) are retained; default remains Semua; see §8 GAP-010.

All open questions are now **CLOSED**; their dispositions are recorded in §3
and §7 above. No open question remains unresolved: **OQ-006** is closed via
§8 GAP-008 and **OQ-007** via §8 GAP-006.

---

# 9. Planning Readiness

## Readiness Checklist

- [x] All critical gaps resolved
- [x] All required decisions recorded
- [x] All blocking open questions resolved
- [x] Architecture can be finalized or updated

## Status

NOT-READY

## Notes

Blocking items: none.

Readiness re-review (2026-09-25): every gap (GAP-001 … GAP-010) and every open
question (OQ-001 … OQ-010) is **CLOSED**. With GAP-007 now closed, no blocking
gap or open question remains and feasibility is sufficiently resolved for the
Architecture phase to finalize/update the target architecture.

Action items assigned to other roles:

- **GAP-007 (Architect)** — update the affected architecture artifacts
  (`RETURN-ORDER-ARCHITECTURE.md`, `BARCODE-REGISTRY-ARCHITECTURE.md`,
  `BARCODE-REGISTRY-UX-BLUEPRINT.md`) to reflect the approved navigation and
  workflow changes (see §8 GAP-007). This is an architecture-maintenance
  activity and does not require additional business decisions.

Resolved items:

- **GAP-003 / OQ-002 / OQ-003** — **CLOSED (2026-09-25):** Draft rows navigate
  directly to Edit/Resume; the Draft Detail screen is removed from the navigation
  graph; the Delete action is relocated from Detail to the Edit/Resume screen.
  BR-019 (Draft delete allowed) and BR-020 (Synced delete prohibited) are preserved
  (see §8 GAP-003).
- **GAP-001 / OQ-009** — **CLOSED (2026-09-25):** the workflow-Discovery input
  for the BGud Return Order operational flow is accepted as the ISSUE plus the
  DOMAIN (FEATURE artifact waived; see §8 GAP-001). No FEATURE artifact is
  required.
- **GAP-002 / OQ-001 / OQ-005** — **CLOSED (2026-09-25):** the Register
  Barcode entry point is moved into Barcode Registry; the Home-level Register
  Barcode action is removed; the standalone `scan` route is no longer required
  as a navigation entry point for barcode registration (see §8 GAP-002).
- **GAP-004** — **CLOSED (2026-09-25):** Home transitions from a feature
  launcher to a work launcher; New Return and Return Orders become the primary
  actions; Synchronization Status card becomes clickable; a More section
  contains Barcode Registry and Settings (see §8 GAP-004 and §8 GAP-009).
- **GAP-005** — **CLOSED (2026-09-25):** Return Orders list becomes a work
  queue with date grouping (TODAY/YESTERDAY/EARLIER), recent-first ordering
  preserved, search preserved, New Return preserved, and status-based routing:
  Draft → Edit/Resume, Synced → read-only Detail (see §8 GAP-005).
- **GAP-006 / OQ-007** — **CLOSED (2026-09-25):** the Return Order capture
  workflow remains unchanged; the existing Create Return Order screen already
  satisfies the continuous transaction capture requirement. The required change
  is limited to layout and structural prioritization: item entry becomes the
  dominant working area, Salesman/Driver/Notes become secondary information, and
  the item list emphasizes Item, Quantity, Unit, and Return Type. Barcode
  scanning remains embedded within the capture process (see §8 GAP-006).
- **GAP-007** — **CLOSED (2026-09-25):** the affected architecture artifacts
  (`RETURN-ORDER-ARCHITECTURE.md`, `BARCODE-REGISTRY-ARCHITECTURE.md`,
  `BARCODE-REGISTRY-UX-BLUEPRINT.md`) shall be updated by the Architect to
  reflect the approved navigation and workflow changes (see §8 GAP-007). This
  is an architecture-maintenance activity and does not require additional
  business decisions.
- **GAP-008 / OQ-006** — **CLOSED (2026-09-25):** this change request is
  limited to Navigation and Layout (navigation hierarchy, screen routing,
  screen ownership, information architecture, content grouping, component
  placement, layout structure, workflow prioritization, visibility of
  operational and administrative actions). Visual styling (colors, typography,
  iconography, branding, component styling, elevation, shadows, animations,
  visual emphasis through color or styling treatment, design-system
  refinements) is explicitly excluded and deferred to a separate UI/UX Styling
  change request after Navigation and Layout implementation is complete (see
  §8 GAP-008).
- **GAP-009 / OQ-004** — **CLOSED (2026-09-25):** the Home Synchronization
  Status card becomes the authoritative navigation entry point to the
  Synchronization screen; the separate Synchronization menu item is removed
  from the More section (see §8 GAP-009).
- **GAP-010 / OQ-008** — **CLOSED (2026-09-25):** the existing status filters
  (Semua, Draft, Synced) are retained; default remains Semua; see §8 GAP-010.
- **OQ-010** — **CLOSED (2026-09-25):** Home's New Return reuses the existing
  full-screen `return_order_create` route; no new capture route, inline capture,
  modal workflow, wizard flow, or embedded Home-screen transaction surface is
  introduced (see §8 OQ-010).

Non-blocking items: none.

The Analyst maintains this checklist and keeps Status `NOT-READY` while blocking
gaps remain. No blocking gap or open question now remains, so the Analyst
readiness review is complete; the Status is left `NOT-READY` only because the
Architect is the sole role that grants the gate by setting it to
`READY-FOR-PLANNING` after the affected architecture artifacts have been
updated. `READY-FOR-PLANNING` means feasibility is sufficiently resolved for the
Architecture skill to finalize or update the target architecture; it does not
mean architecture is complete or that the architecture step may be skipped.
Planning begins only after the required architecture work (the GAP-007
architecture-artifact update) is complete.

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
