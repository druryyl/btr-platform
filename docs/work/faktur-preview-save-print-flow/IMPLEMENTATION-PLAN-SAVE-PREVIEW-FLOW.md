# IMPLEMENTATION PLAN

## Faktur Save/Print Flow Re-Ordering (Preview → Save → Print)

| Field | Value |
| --- | --- |
| Planning mode | Mode B — Feasibility-Driven Planning |
| Planning authority | FEASIBILITY ASSESSMENT |
| Authority artifact | `docs/work/faktur-preview-save-print-flow/FEASIBILITY-ASSESSMENT-SAVE-PREVIEW-FLOW.md` |
| Authority date | 2026-09-12 |
| Authority status | READY — all blocking decisions closed (D-001 through D-012) |
| Scope | Desktop `FakturForm` (btr.distrib), Faktur save/print path, both save modes (NEW and EDIT) |
| Recommendation implemented | Option A — In-memory preview, mode-aware number handling (no reservation). Option B rejected. Option C rejected. |

---

## Planning Authority

```text
FEASIBILITY ASSESSMENT
```

The feasibility report is authoritative. This plan does not reinterpret decisions D-001 through D-012, does not change approved scope, and does not introduce new business or architecture decisions.

Authoritative decisions consumed by this plan:

- D-001 — Preview built from in-memory aggregate, no persistence or side effects.
- D-002 — NEW preview shows `[DRAFT]` (or pre-selected open code); EDIT shows existing number; no reservation.
- D-003 — Save button is the preview entry point; save occurs only after Confirm Save from preview; no separate Preview button.
- D-004 — Missing optional preview data renders as `-`; DTO null-safe; save-time validation unchanged.
- D-005 — Preview dialog provides SAVE (persist, no print) and SAVE & PRINT (persist, then print finalized document); print only after successful save.
- D-006 — Single shared Faktur construction path for preview and save; no preview-specific logic.
- D-007 — Preview is read-only, non-transactional; cancelling leaves system unchanged.
- D-008 — Flow applies to NEW, EDIT, Faktur Klaim (per corresponding mode), voided Faktur (preview/print allowed, save unavailable).
- D-009 — No preview logging/audit; no new tables; save audit unchanged.
- D-010 — Preview executes save-level validation via shared (non-duplicated) logic; invalid Faktur is not previewed.
- D-011 — EDIT preview reflects current form state including unsaved modifications; SAVE & PRINT reloads and prints persisted version.
- D-012 — Preview is a draft review document; only the saved Faktur is final.

Non-blocking open item carried as constraint, not decision:

- BQ-004 (same viewer instance vs two instances) is left as implementation detail. The plan constrains only what the feasibility report decides: preview is a draft document; SAVE & PRINT prints the reloaded persisted version (D-005, D-011, D-012). Whether the implementer reuses one viewer window or opens a second instance is permitted either way provided the draft/final distinction holds.

---

## Scope Summary

Approved implementation scope (Feasibility §7, §10, §11):

1. Re-order the Faktur entry flow from `Save → Print Preview → Print` to `Print Preview → Save → Print`, where Save is the preview entry point and persistence occurs only after SAVE / SAVE & PRINT confirmation from the preview.
2. Build the preview from the same in-memory aggregate construction path that Save persists (single path; side-effect-free view), covering item mapping, quantities, discounts, taxes, claims, price-refresh (`isRefreshHrg`), and grand total.
3. Enforce save-level business validation before preview generation through shared logic (no duplication); invalid documents show the validation message and are not previewed.
4. Display `No.Faktur` by mode: NEW shows `[DRAFT]` when no code assigned (or the pre-selected open code); EDIT shows the existing loaded number and save preserves it; Klaim follows its corresponding mode; voided allows preview/print with save unavailable.
5. Provide SAVE (persist, no print) and SAVE & PRINT (persist, reload persisted Faktur, print final) from the preview dialog; cancel/back-to-edit persists nothing; failed save prevents printing; only the saved Faktur is the official printable document.
6. Make the print DTO null-safe with `-` fallback for optional/unavailable display data; preview never throws solely for missing optional fields.
7. Preserve side-effect freedom: preview never triggers GenStok, piutang, packing order, order status, persistence, commits, or counter consumption. No number-reservation work. No database, integration, or security changes. No preview logging/audit artifacts.

Out of scope (explicitly excluded by authority):

- Number reservation / void / reuse semantics (Option B rejected, D-002).
- New database objects, migrations, counter changes, `BTR_FakturCodeOpen` behavior changes.
- New integrations, permission/security changes.
- Preview activity logging, history, analytics, audit reporting, abandoned-preview tracking (D-009).
- Separate Preview button (D-003 rejects).
- Relaxing save-time mandatory-field validation (D-004 non-goal).
- Automatic printing on preview open (D-005 rejects).

---

## Impact Inventory

### Backend

- `FakturBuilder` (`src/j05-btr-distrib/btr.application/SalesContext/FakturAgg/Workers/FakturBuilder.cs`) — single construction path shared by preview and save per D-001/D-006; no preview-specific mapping/calculation logic permitted. Refactor-to-share only; no numbering change.
- `SaveFakturWorker` (`.../SalesContext/FakturAgg/UseCases/SaveFakturWorker.cs`) — no behavioral change (numbering stays at save, D-002); validation guards exposed/shared for preview per D-010 without duplication; construction path shared per D-006.
- `FakturWriter` (`.../FakturWriter.cs`) — no change; generates `FakturId`/`FakturCode` only when empty (NEW); EDIT preserves loaded values. Verified, not modified.
- `SaveFakturRequest` mapping (`FakturForm.cs:956-1009`) — preview and save share the same construction path, not a duplicated mapping (D-006).
- Validation logic (`SaveFakturWorker` guards + form-level mandatory checks) — extracted/shared entry point callable from preview and save paths (D-010); remains side-effect free (reads only, D-007).

### Database

- None. No new tables, indexes, constraints, or migrations (D-002, D-009).
- `BTR_FakturCodeOpen` / `INunaCounterBL.Generate` — untouched; no reservation semantics.

### Frontend

- `FakturForm.cs` (`src/j05-btr-distrib/btr.distrib/SalesContext/FakturAgg/FakturForm.cs`) — `SaveButton_Click` (`:918-954`) becomes preview entry point: run shared save-level validation first, build in-memory preview aggregate, open preview dialog, persist only after SAVE / SAVE & PRINT; construct unsaved-aggregate-to-print-DTO; branch NEW vs EDIT number display; EDIT preview from current edited state (never reloaded persisted version); voided/Klaim handling.
- `FakturForm.Designer.cs` — no dedicated Preview button; Save remains single primary action (D-003).
- `FakturPrintOutDto.cs` (`.../FakturAgg/FakturPrintOutDto.cs`) — null-safe with `-` fallback (D-004); empty `FakturCode` renders `[DRAFT]` for NEW (D-002); no throw on missing optional fields.
- `RdlcViewerForm` / preview host (`.../FakturAgg/RdlcViewerForm.cs`) — hosts SAVE, SAVE & PRINT, Cancel / Back to Edit (D-003, D-005); SAVE persists without printing; SAVE & PRINT persists then prints finalized reloaded document; toolbar print is no longer the primary workflow trigger; draft visually distinguishable from final.

### Integration

- None. Printed output remains local RDLC; no external service changes.
- Security: none. Existing user/permission handling unchanged.

---

## Phases

| Phase | Name | Objective | Slices |
| --- | --- | --- | --- |
| Phase 1 | Safe preview foundation | Establish null-safe display, shared validation, and single construction path without changing the user-visible save flow. | SL-01, SL-02, SL-03 |
| Phase 2 | Preview-before-save flow | Rewire Save as preview entry point with SAVE / SAVE & PRINT / Cancel and mode-aware draft numbering. | SL-04, SL-05 |
| Phase 3 | Finalization and mode coverage | Complete post-save finalize/print semantics and prove side-effect freedom across NEW, EDIT, Klaim, and voided modes with no audit artifacts. | SL-06, SL-07 |

Phase ordering rationale: Phase 1 is independently testable (DTO, validation, builder) and has no UI flow dependency. Phase 2 depends on Phase 1. Phase 3 depends on Phase 2. Each phase produces independently reviewable value and respects dependency order.

---

## Slices

### Slice SL-01 — Null-safe print DTO with draft-number handling

### Objective

Make `FakturPrintOutDto` null-safe with `-` fallback for optional/unavailable display data and `[DRAFT]` handling for unassigned NEW numbers, without changing save-time validation.

### Dependencies

- Prerequisite slices: none.
- Required artifacts: FEASIBILITY-ASSESSMENT D-002, D-004.
- Blocking conditions: none.

### Acceptance Criteria

1. `FakturPrintOutDto` renders null `customer.Address2` as `-` instead of throwing `NullReferenceException`.
2. `FakturPrintOutDto` renders null `customer.Kota` as `-` instead of throwing `NullReferenceException`.
3. `FakturPrintOutDto` renders an empty item-list lookup result without throwing (fallback `-` or equivalent non-exceptional rendering; no `NullReferenceException`).
4. `FakturPrintOutDto` renders empty `FakturCode` as `[DRAFT]` for the NEW pre-save preview path.
5. `FakturPrintOutDto` renders a pre-selected open `FakturCode` verbatim when present in NEW preview.
6. `FakturPrintOutDto` renders the loaded persisted `FakturCode` verbatim for EDIT preview.
7. Save-time mandatory-field validation behavior is unchanged (no relaxation introduced by this slice).

### Review Focus

- Architecture Compliance (D-004 tolerance vs save validation boundary; D-002 draft rule).
- Persistence Compliance (no persistence touched).
- UI State Compliance (display fallback correctness).

---

### Slice SL-02 — Shared save-level validation entry for preview and save

### Objective

Expose a single shared validation entry covering all `SaveFakturWorker` business guards plus mandatory form requirements (customer, warehouse, salesperson, due date, at least one item line) that both preview and save call, with no duplicated validation logic, keeping preview side-effect free.

### Dependencies

- Prerequisite slices: none (independently implementable alongside SL-01).
- Required artifacts: FEASIBILITY-ASSESSMENT D-007, D-010; existing `SaveFakturWorker` guards (`SaveFakturWorker.cs:82-88`).
- Blocking conditions: none.

### Acceptance Criteria

1. A single validation entry exists and is called by both the preview path and the save path (no duplicated rule sets between Preview and Save).
2. Validation rejects a Faktur with no customer selection and returns the corresponding validation message.
3. Validation rejects a Faktur with no warehouse selection and returns the corresponding validation message.
4. Validation rejects a Faktur with no salesperson selection and returns the corresponding validation message.
5. Validation rejects a Faktur with missing due date (where required by existing save rules) and returns the corresponding validation message.
6. Validation rejects a Faktur with zero item lines and returns the corresponding validation message.
7. Validation rejects every case currently rejected by `SaveFakturWorker` guards with the same message Save produces.
8. Executing validation performs reads only: it triggers no GenStok, piutang, packing order, order status, persistence, commit, or counter consumption.

### Review Focus

- Architecture Compliance (no duplicated logic; D-006/D-010 sharing principle).
- Workflow Compliance (preview represents a ready-to-save document).
- Persistence Compliance (validation performs no writes).

---

### Slice SL-03 — Single shared in-memory aggregate construction path

### Objective

Establish one aggregate-construction path (`Form Input → SaveFakturRequest → FakturBuilder → FakturModel`) used identically by preview and save, where preview is a side-effect-free view of the same aggregate that save will persist, with no preview-specific mapping or calculation logic.

### Dependencies

- Prerequisite slices: none (independently implementable; integrates with SL-02 at SL-04).
- Required artifacts: FEASIBILITY-ASSESSMENT D-001, D-006, D-007, D-011; `FakturBuilder.cs:113-337`; `SaveFakturWorker.cs:168-177`.
- Blocking conditions: none.

### Acceptance Criteria

1. Preview builds its `FakturModel` through the same construction path (same item mapping, quantities, discounts, taxes, claim processing, `isRefreshHrg` handling, `CalcTotal`) that Save persists — no separate preview-specific mapping, calculation, item transformation, claim handling, pricing-refresh, or total logic exists.
2. Preview construction calls none of: `FakturWriter.Save`, `IGenStokFakturWorker`, piutang creation/update, packing order creation/update, order mapping updates, database commits, or counter consumption.
3. NEW preview construction produces an aggregate with empty `FakturId` (number deferred to save) unless an open code was pre-selected, in which case the pre-selected `FakturCode` is carried on the aggregate.
4. EDIT preview construction produces an aggregate from the current form state including unsaved modifications, carrying the existing loaded `FakturId`/`FakturCode` (never the reloaded persisted version).
5. For identical form input, the preview aggregate (items, totals, taxes, discounts, claims) equals the aggregate that the save path would persist (divergence check passes for at least one NEW and one EDIT case).

### Review Focus

- Architecture Compliance (single path; D-001/D-006; no divergent logic).
- Persistence Compliance (preview performs no writes/commits/counter consumption).
- Workflow Compliance (preview shows exactly what will be saved).

---

### Slice SL-04 — Save as preview entry point with SAVE / SAVE & PRINT / Cancel

### Objective

Rewire `SaveButton_Click` so Save runs shared validation, then opens the preview dialog; persistence occurs only after explicit SAVE or SAVE & PRINT confirmation; closing/Cancel/Back-to-Edit persists nothing.

### Dependencies

- Prerequisite slices: SL-01 (null-safe DTO), SL-02 (shared validation), SL-03 (shared construction).
- Required artifacts: FEASIBILITY-ASSESSMENT D-001, D-003, D-005, D-007, D-010.
- Blocking conditions: SL-01, SL-02, SL-03 must be IMPLEMENTED (this slice wires them into the form flow).

### Acceptance Criteria

1. Clicking Save on a valid Faktur opens the preview dialog instead of persisting immediately.
2. Clicking Save on an invalid Faktur shows the shared validation message and does not open the preview.
3. The preview dialog provides three outcomes: SAVE, SAVE & PRINT, and Cancel / Back to Edit.
4. Choosing Cancel / Back to Edit (or closing the dialog without choosing SAVE or SAVE & PRINT) persists nothing and leaves inventory, piutang, packing order, order status, numbering, and persisted Faktur data unchanged.
5. No dedicated Preview button is added; Save remains the single primary action on the form.
6. Preview generation on this path performs no GenStok, piutang, packing order, order status, persistence, commit, or counter consumption.

### Review Focus

- Workflow Compliance (D-003 Save-entry flow; D-005 dialog as decision point).
- UI State Compliance (dialog actions; cancel-leaves-unchanged).
- Architecture Compliance (side-effect freedom preserved on preview path).

---

### Slice SL-05 — Mode-aware preview number and mode coverage

### Objective

Apply mode-specific preview behavior: NEW draft number (`[DRAFT]` or pre-selected code), EDIT existing number from current edited state, Faktur Klaim per corresponding mode, voided Faktur preview/print allowed with save unavailable.

### Dependencies

- Prerequisite slices: SL-03 (construction carries correct number state), SL-04 (preview dialog flow).
- Required artifacts: FEASIBILITY-ASSESSMENT D-002, D-008, D-011.
- Blocking conditions: SL-03, SL-04 must be IMPLEMENTED.

### Acceptance Criteria

1. NEW preview with empty `FakturCode` displays `[DRAFT]` as `No.Faktur` and is visually distinguishable as a draft.
2. NEW preview with a pre-selected open code displays that code as `No.Faktur`.
3. EDIT preview displays the existing loaded `FakturCode` (not `[DRAFT]`) and reflects current unsaved form modifications (an edited item quantity/total appears in preview without saving).
4. EDIT save preserves the existing `FakturId`/`FakturCode` (save does not regenerate a new number; writer skip-empty behavior verified on this path).
5. Faktur Klaim in NEW mode behaves per NEW preview/save rules; Faktur Klaim in EDIT mode behaves per EDIT preview/save rules; existing Klaim business rules are unchanged.
6. Voided Faktur allows preview and print but Save (SAVE / SAVE & PRINT) remains unavailable (`FakturForm.cs:450-457` behavior preserved).

### Review Focus

- Workflow Compliance (D-008 mode matrix; D-011 current-state preview).
- UI State Compliance (draft distinguishable from final; number display per mode).
- Persistence Compliance (EDIT number preservation; NEW number deferred to save).

---

### Slice SL-06 — Post-save finalize: SAVE vs SAVE & PRINT on persisted document

### Objective

Implement post-confirmation finalize semantics: SAVE persists without printing; SAVE & PRINT persists, reloads the persisted Faktur, and prints the final persisted version; failed save prevents printing; only the saved Faktur counts as the final official document.

### Dependencies

- Prerequisite slices: SL-04 (dialog flow), SL-05 (mode-aware numbers).
- Required artifacts: FEASIBILITY-ASSESSMENT D-005, D-007, D-011, D-012.
- Blocking conditions: SL-04, SL-05 must be IMPLEMENTED.

### Acceptance Criteria

1. SAVE persists the Faktur and closes without invoking any print action.
2. SAVE & PRINT persists the Faktur, reloads the persisted Faktur, and prints the reloaded persisted version (not the in-memory draft), including the final generated number for NEW.
3. If save fails, neither SAVE nor SAVE & PRINT prints; the operator sees the save failure and no print output is produced.
4. Printing is never automatic on preview open; print initiates only after a successful save via explicit SAVE & PRINT.
5. The draft preview is not treated as an official printed document; only the successfully saved Faktur is printed as final (no special paper control or draft-print audit introduced).

### Review Focus

- Workflow Compliance (D-005 SAVE vs SAVE & PRINT; D-012 draft vs final).
- Persistence Compliance (reload-persisted-then-print; failed-save-prevents-print).
- Architecture Compliance (existing print template/`CLIENT_ID` selection unchanged).

---

### Slice SL-07 — Cross-mode regression and side-effect-free verification

### Objective

Prove the re-ordered flow across NEW, EDIT, Faktur Klaim, and voided modes: preview output equals saved output, preview/cancel is side-effect free, and no preview logging/audit artifacts are introduced.

### Dependencies

- Prerequisite slices: SL-01, SL-02, SL-03, SL-04, SL-05, SL-06.
- Required artifacts: FEASIBILITY-ASSESSMENT D-006, D-007, D-009; all prior slices.
- Blocking conditions: SL-01 through SL-06 must be IMPLEMENTED.

### Acceptance Criteria

1. For at least one NEW case, preview totals/items equal post-save persisted totals/items for the same input.
2. For at least one EDIT case, preview totals/items (including an unsaved modification) equal post-save persisted totals/items for the same input.
3. Opening and cancelling a preview (NEW and EDIT) changes no inventory, piutang, packing order, order status, persisted Faktur rows, or consumed counters.
4. No new database tables, preview-session records, preview audit-trail entries, or abandoned-preview tracking artifacts are introduced; save-related audit behavior is unchanged.
5. Voided Faktur verification passes: preview/print available, save unavailable.
6. No counter gaps are introduced by preview (NEW preview consumes no numbers; verification passes with counters unchanged after preview-cancel).

### Review Focus

- Architecture Compliance (D-006 consistency; D-007 side-effect freedom).
- Persistence Compliance (no writes on preview/cancel; no counter consumption).
- Workflow Compliance (all modes per D-008; D-009 no-logging scope respected).

---

## Progress Tracker

| Slice ID | Status | Implementation History | Review History | Remediation History |
| --- | --- | --- | --- | --- |
| SL-01 | GO | 2026-09-12: SL-01 implemented (null-safe FakturPrintOutDto + [DRAFT] handling) | 2026-09-12: GO — all 7 ACs verified, no findings above INFO | — |
| SL-02 | GO | 2026-09-12: SL-02 implemented (shared SaveFakturValidator entry; SaveFakturWorker delegates; zero-item-line guard) | 2026-09-12: GO — all 8 ACs verified, no findings above INFO | — |
| SL-03 | PLANNED | — | — | — |
| SL-04 | PLANNED | — | — | — |
| SL-05 | PLANNED | — | — | — |
| SL-06 | PLANNED | — | — | — |
| SL-07 | PLANNED | — | — | — |

Lifecycle per slice:

```text
PLANNED
    ↓
IN IMPLEMENTATION
    ↓
IMPLEMENTED
    ↓
IN REVIEW
    ↓
GO
```

or on rejection:

```text
PLANNED
    ↓
IN IMPLEMENTATION
    ↓
IMPLEMENTED
    ↓
IN REVIEW
    ↓
NO-GO
    ↓
REMEDIATION
    ↓
IN REVIEW
    ↓
GO
```

---

## Plan Validation

- Complete scope coverage: §7 steps 1–8 mapped — step 1 → SL-03/SL-04; step 2 → SL-03; step 3 → SL-03/SL-04/SL-07; step 4 → SL-01/SL-05; step 5 → SL-06; step 6 → SL-01; step 7 → SL-05/SL-07; step 8 → SL-02/SL-04. Gaps GAP-001–GAP-009 closed by D-001–D-009 respectively and each decision is enforced by at least one slice; D-010/D-011/D-012 enforced by SL-02/SL-03/SL-05/SL-06.
- Valid dependencies: SL-01–SL-03 independent; SL-04 depends only on earlier slices; SL-05/SL-06 depend only on earlier slices; SL-07 depends on all earlier slices. No forward dependency.
- Reviewable slices: each slice has a single objective, explicit acceptance criteria (objective/testable), and review focus areas for the Review Agent.
- Tracker completeness: all seven slices tracked with lifecycle states.
- Planning authority compliance: no new business or architecture decisions; BQ-004 handled as unconstrained implementation detail within decided draft/final constraints; Option B/C changes excluded; database/integration/security scope respected as none.
