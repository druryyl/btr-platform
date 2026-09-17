# FEASIBILITY ASSESSMENT

## Faktur Save/Print Flow Re-Ordering (Preview → Save → Print)

| Field | Value |
| --- | --- |
| Status | Analysis only; no implementation |
| Assessment date | 2026-09-12 |
| Requested decision | GO / CONDITIONAL GO / NO GO |
| Scope | Desktop `FakturForm` (btr.distrib), Faktur save/print path, both save modes (NEW and EDIT) |
| Evidence boundary | Source code at assessment time: `FakturForm.cs`, `FakturPrintOutDto.cs`, `RdlcViewerForm.cs`, `FakturBuilder.cs`, `FakturWriter.cs`, `SaveFakturWorker.cs`; `docs/features/faktur/feature.md`; `docs/foundation/WORKFLOW.md` |
| Planning handoff | READY — all blocking decisions closed (see §9, §11). GAP-001 CLOSED by D-001 (§12); GAP-002 CLOSED by D-002 (§12); GAP-003 CLOSED by D-003 (§12); GAP-004 CLOSED by D-004 (§12); GAP-005 CLOSED by D-005 (§12); GAP-006 CLOSED by D-006 (§12); GAP-007 CLOSED by D-007 (§12); GAP-008 CLOSED by D-008 (§12); GAP-009 CLOSED by D-009 (§12); BQ-001 RESOLVED by D-002; BQ-003 RESOLVED by D-003; BQ-002 RESOLVED by D-005; BQ-005 RESOLVED by D-008; OQ-001 RESOLVED by D-009; TQ-003 RESOLVED by D-010; TQ-004 RESOLVED by D-011; OQ-002 RESOLVED by D-012 |

---

## 1. Executive Summary

### Request

Change the Faktur data-entry flow from the current sequence:

```text
Save → Print Preview → Print
```

to:

```text
Print Preview → Save → Print
```

After the user finishes input, the system should first show a print preview so the user can verify the document, then save, and finally print.

### Recommendation

**CONDITIONAL GO.** The re-ordering is technically achievable because the Faktur aggregate can already be built in memory without persisting it (`IFakturBuilder.CreateNew/Attach/AddItem/CalcTotal`; `FakturBuilder.cs:113-337`) and the print DTO/RLDC viewer already accepts an arbitrary `FakturModel`. The work is concentrated in one desktop form and its print helper.

The Save action has **two distinct modes**, and they have different feasibility:

- **Edit existing Faktur — feasible.** The aggregate is loaded by `FakturIdText` before editing (`FakturForm.cs:364-448`), so the invoice number (`FakturId`/`FakturCode`) already exists. A pre-save preview can therefore display the final `No.Faktur`, and the writer already skips ID/code generation because those values are non-empty on edit (`FakturWriter.cs:56-61`).
- **New Faktur — conditionally feasible.** No ID exists until save. `FakturId`/`FakturCode` are assigned only **inside** the writer during save (`FakturWriter.cs:56-61`), so a pre-save preview has no final invoice number. Per D-002 (§12), NEW preview shows a draft placeholder (`[DRAFT]`) unless the operator pre-selected an open code (BQ-001 resolved).

Per D-005 (§12), the post-save print trigger is decided: the preview dialog offers SAVE (persist, no print) and SAVE & PRINT (persist, then print the finalized document). Printing occurs only after a successful save (BQ-002 resolved).

### Feasibility Result

```text
PARTIALLY FEASIBLE
```

- Edit mode is **fully feasible** (final invoice number is available before save).
- New mode is **feasible** (number display decided by D-002: `[DRAFT]` placeholder; BQ-001 resolved; print trigger decided by D-005: SAVE vs SAVE & PRINT; BQ-002 resolved).
- No database, integration, or security blocker was found. Per D-002 (§12), no number-reservation mechanism is required.

---

## 2. Request Understanding

### Requested capability

- Insert a **print-preview step before persistence** in the Faktur entry form.
- Keep **Save** as the persistence step.
- Perform **Print after Save**.

### Business objective

- Let the operator visually verify the printed invoice before committing the transaction.
- Reduce wrong invoices / rework caused by discovering errors only after save + print.

### Expected user-visible outcome

```text
1. Operator finishes input.
2. Operator clicks Save; system renders preview (unsaved draft document).
3. Operator confirms from preview; Faktur is saved.
4. Faktur is printed (final document).
```

Per D-003 (§12), Save is the entry point to preview; no separate Preview button. Persistence occurs only after explicit confirmation from the preview.

### Save modes in scope

The re-ordered flow must work for both save modes, which behave differently with respect to the invoice number:

| Mode | Trigger | Invoice number before save | Number generated at save |
| --- | --- | --- | --- |
| **NEW** | `FakturIdText` is empty; save builds via `CreateNew` (`SaveFakturWorker.cs:168-170`) | Not available | Yes, unless the operator pre-selected a code from the open-code browser |
| **EDIT** | `FakturIdText` holds an existing id; save builds via `Load` (`SaveFakturWorker.cs:172-177`) | Already available on the loaded aggregate | No — `FakturWriter` only generates when empty (`FakturWriter.cs:56-61`) |

### Interpretation note (not a decision)

The request may also mean a single preview window whose internal sequence is preview → save → print. This assessment assumes two logical documents (pre-save draft, post-save final) unless the product owner states otherwise. See BQ-004.

---

## 3. Current State Analysis

### Existing Business Flow

From `docs/features/faktur/feature.md` and `docs/foundation/WORKFLOW.md`:

- Faktur creation **immediately reduces inventory**.
- Faktur is the official sales transaction in the workflow: Sales Order → Faktur → Warehouse Fulfillment → Faktur Kembali.
- One Faktur belongs to exactly one Warehouse.

### Existing Components (save/print path)

| Component | Location | Role |
| --- | --- | --- |
| `FakturForm` | `FakturForm.cs` | Data entry; single `SaveButton` |
| `SaveButton_Click` | `FakturForm.cs:918-954` | Save → fetch customer → save piutang → save packing order → clear form → reload → print |
| `SaveFaktur` | `FakturForm.cs:956-1009` | Maps form to `SaveFakturRequest` |
| `PrintFakturRdlc` | `FakturForm.cs:1059-1086` | Builds datasets, selects template by `CLIENT_ID`, opens `RdlcViewerForm` modal |
| `FakturPrintOutDto` | `FakturPrintOutDto.cs:15-128` | Formats report fields from a `FakturModel` + `CustomerModel` + `UserModel` |
| `RdlcViewerForm` | `RdlcViewerForm.cs` | Modal RDLC preview; print is a built-in toolbar action (`TheViewer.Print`), not automatic |
| `SaveFakturWorker` | `SaveFakturWorker.cs:80-141` | Guard → build → persist → order map → gen stok (transactional) |
| `FakturBuilder` | `FakturBuilder.cs` | Builds in memory; `AddItem` computes item lines; `CalcTotal` computes totals |
| `FakturWriter` | `FakturWriter.cs:50-112` | **Assigns `FakturId` and `FakturCode` at save time**, then persists |

### Key current-state facts

1. **Print depends on a persisted aggregate.** After save, the form re-loads from DB (`_fakturBuilder.Load(faktur).Build()`, `FakturForm.cs:929-931`) and prints. The `FakturId`/`FakturCode` in that model were assigned by the writer.
2. **In-memory build already exists.** `IFakturBuilder.CreateNew(...)...AddItem(...)...CalcTotal().Build()` produces a complete `FakturModel` without any DAL write (`FakturBuilder.cs:113-337`), so a preview can be rendered from unsaved data.
3. **Number generation is a save side effect, and only for NEW.** `FakturWriter.Save` generates `FakturId` (`FakturWriter.cs:56-57`) and `FakturCode` (`FakturWriter.cs:59-61`) only if empty. In **EDIT**, both are already present because the aggregate was loaded via `FakturIdText_Validating`/`ValidateFaktur` (`FakturForm.cs:364-448`) and passed through `SaveFaktur` (`FakturForm.cs:956-1009`); no new number is generated. In **NEW**, `FakturIdText` is empty, so the number is generated during save. `FakturCode` may alternatively be pre-selected by the operator from the open-code browser (`FakturCodeButton_Click`, `FakturForm.cs:255-258`), in either mode.
4. **Print output uses `FakturCode`,** not `FakturId` (`FakturPrintOutDto.cs:17`), plus customer `Address2`/`Kota` (`FakturPrintOutDto.cs:22-23`) and item `DppProsen`/`PpnProsen` (`:39-41`).
5. **Single action.** The form exposes only `SaveButton`; there is no preview or print button (`FakturForm.Designer.cs:376-386`).
6. **Side effects are isolated to save.** Stock reduction (`IGenStokFakturWorker`), piutang, packing order, and order-map updates all occur inside/after `SaveFakturWorker` and `SaveButton_Click`, not during build. Preview would not trigger them.
7. **Validation is minimal.** `FakturValidator` is empty (`FakturValidator.cs:6-9`); real guards live in `SaveFakturWorker.Execute` (`SaveFakturWorker.cs:82-88`).

### Existing Database

- No new database object is implied by the re-ordering itself. Numbering uses `INunaCounterBL.Generate` and the `BTR_FakturCodeOpen` table.

### Existing Integrations / Security

- None relevant. Printed output is local RDLC; no external system.

---

## 4. Impact Analysis

### Backend Impact

- `SaveFakturWorker` / `FakturWriter`: **no change required under D-002 (§12).** Number generation remains at save for NEW; EDIT already carries the number and needs no change here. (Option B reservation rejected.)
- `FakturBuilder`: per D-001 (§12) + D-006 (§12), a single aggregate-construction path serves both preview and save — preview is a side-effect-free view of the same aggregate that will later be persisted; no preview-specific mapping, calculation, or total logic is permitted.
- `SaveFakturRequest` / item mapping: the preview builds the same request from the grid; the request already distinguishes NEW (`FakturId` empty) from EDIT (`FakturId` populated) (`SaveFakturWorker.cs:168-177`).

### Database Impact

- None for the basic re-ordering, in either NEW or EDIT mode.
- If a number-reservation approach is chosen **for NEW** (Option B), a reservation/void mechanism for consumed-but-unsaved numbers would have database and counter semantics implications (GAP-002). **Per D-002 (§12), Option B is rejected; EDIT and NEW both require no number-reservation work.**

### Frontend Impact

- `FakturForm.cs`: `SaveButton_Click` becomes preview entry point per D-003 (§12) — run save-level validation first per D-010 (§12, invalid Faktur shows the message and is not previewed), then build in-memory preview, persist only after SAVE / SAVE & PRINT; add unsaved-aggregate-to-print-DTO construction; display-only nulls fall back to `-` per D-004; branch the preview on NEW vs EDIT (EDIT shows the existing number and is built from the current edited state per D-011, never the reloaded persisted version; NEW shows `[DRAFT]` or the pre-selected open code per D-002).
- `FakturForm.Designer.cs`: no dedicated Preview button required per D-003 (§12); Save remains the single primary action.
- `FakturPrintOutDto.cs`: null-safe per D-004 (§12) — optional/unavailable display data renders as `-`; preview never throws solely for missing optional fields (save-time validation unchanged).
- `RdlcViewerForm` / preview host: per D-003 (§12) + D-005 (§12), provides SAVE, SAVE & PRINT, and Cancel / Back to Edit; SAVE persists without printing, SAVE & PRINT persists then prints the finalized document; print initiates only after a successful save.

### Integration Impact

- None.

### Security Impact

- None. Existing user/permission handling is unchanged.

---

## 5. Gap Analysis

| Gap ID | Type | Description | Severity |
| --- | --- | --- | --- |
| GAP-001 | Functional | There is no preview-before-save path. `SaveButton_Click` saves, reloads, then prints; the print DTO is built from a persisted aggregate. **CLOSED by D-001 (2026-09-12, APPROVED) — see §12.** | High — closed |
| GAP-002 | Data | **NEW only.** `FakturCode`/`FakturId` are assigned inside `FakturWriter.Save`; a NEW pre-save preview has no final invoice number, yet the report shows `No.Faktur`. In EDIT the number is already loaded, so this gap does not apply. **CLOSED by D-002 (2026-09-12, APPROVED) — NEW preview shows `[DRAFT]` when no code assigned; Option B rejected — see §12.** | High for NEW (closed); None for EDIT |
| GAP-003 | UX | The form has a single Save action. There is no dedicated preview action, and no defined confirmation step between preview and save. **CLOSED by D-003 (2026-09-12, APPROVED) — Save is the preview entry point; save occurs only after Confirm Save from preview; no separate Preview button — see §12.** | Medium (closed) |
| GAP-004 | Functional | `FakturPrintOutDto` assumes non-null `customer.Address2`/`Kota` and at least one list item (`.FirstOrDefault()`); a preview with incomplete input can throw `NullReferenceException`. **CLOSED by D-004 (2026-09-12, APPROVED) — missing optional preview data renders as `-`; DTO must be null-safe; save-time validation unchanged — see §12.** | Medium (closed) |
| GAP-005 | Functional | "Print after save" is undefined relative to the current modal viewer, where printing is a manual toolbar click. **CLOSED by D-005 (2026-09-12, APPROVED) — preview dialog provides SAVE (persist, no print) and SAVE & PRINT (persist, then print finalized document); print only after successful save — see §12.** | Medium (closed) |
| GAP-006 | Technical | Rebuilding the aggregate for preview risks divergence from `SaveFakturWorker`'s construction (totals, item mapping, claim list, `isRefreshHrg`). Applies to both NEW and EDIT. **CLOSED by D-006 (2026-09-12, APPROVED) — single construction path for preview and save; preview is a side-effect-free view of the same aggregate that will be persisted — see §12.** | Medium (closed) |
| GAP-007 | Functional | Preview must remain side-effect free (no stock, piutang, packing order, order status). Current build path already satisfies this; must be preserved. **CLOSED by D-007 (2026-09-12, APPROVED) — preview is read-only and non-transactional; protected operations (GenStok, piutang, packing order, order status, persistence, commits, counter consumption) never trigger; cancelling preview leaves the system unchanged — see §12.** | Low (closed) |
| GAP-008 | UX | The flow was not specified per save mode. EDIT already has a persisted number (preview can show it, and save must not regenerate it); NEW has no number (see GAP-002). Voided faktur hides Save. Product must confirm the flow applies to both modes and to Faktur Klaim. **CLOSED by D-008 (2026-09-12, APPROVED) — flow applies to NEW, EDIT, Faktur Klaim (same as corresponding mode), and voided Faktur (preview/print allowed, save unavailable) — see §12.** | Medium (closed) |
| GAP-009 | Operational | Abandoned preview sessions, and whether a preview is logged/auditable, are undefined. **CLOSED by D-009 (2026-09-12, APPROVED) — preview sessions are not logged or audited; no new tables, no abandoned-preview tracking; save-related audit unchanged — see §12.** | Low (closed) |

---

## 6. Solution Options

### Option A — In-memory preview, mode-aware number handling (no reservation)

Build the aggregate in memory (`FakturBuilder`) and render the report from it.

- **EDIT:** use the loaded aggregate, which already carries the existing `FakturId`/`FakturCode`; preview shows the real number and save must not regenerate it (it already does not).
- **NEW:** per D-002 (§12), show `[DRAFT]` as `No.Faktur` when no code is pre-selected, or the operator-pre-selected open code. On confirm, run the existing save (which generates the number), then open the viewer again with the persisted data for printing.

**Advantages**

- Fully reuses existing builder and viewer.
- No counter/ID changes, no number gaps, no new persistence.
- Correct and complete for EDIT; acceptable for NEW if a draft number is allowed.
- Lowest risk and smallest change.

**Disadvantages**

- NEW pre-save preview may not show the final invoice number (unless the operator pre-selects a code via the existing open-code browser).

**Risk:** Low.

### Option B — Reserve the NEW invoice number before preview

For **NEW only**, generate `FakturId`/`FakturCode` before preview (without persisting, or by persisting a draft) so preview shows the final number; save commits that number. **EDIT is unaffected** — it already has the number.

**Advantages**

- NEW preview looks identical to the final print.

**Disadvantages**

- Consumed numbers on abandoned previews → gaps; needs reservation/void/reuse semantics.
- Counter generation is currently coupled to `FakturWriter`; moving it earlier touches backend behavior.
- Conflicts with the open-code model in `BTR_FakturCodeOpen`.
- No benefit for EDIT, which Option A already handles.

**Risk:** High.

### Option C — Keep current Save, relabel the existing viewer as preview

No re-order; only clarify wording. Not a solution to the request.

**Recommendation:** reject.

### Recommended option

- **EDIT:** **Option A, unconditionally.** The final invoice number is already available, so preview, save, and print all involve the same number with no counter work and no gaps.
- **NEW:** **Option A, decided per D-002 (§12).** Preview shows `[DRAFT]` when no `FakturCode` is assigned (or the pre-selected open code when present). Option B (reservation) is rejected; no counter/void/reuse semantics required.

---

## 7. Recommended Approach (implementation-neutral)

1. **Separate the three concerns** currently fused in `SaveButton_Click`: (a) produce an in-memory preview document, (b) persist, (c) print. Per D-001 (§12), (a) must not execute persistence or business side effects. Per D-003 (§12), (a) is entered via the existing Save button and (b) executes only after SAVE / SAVE & PRINT from the preview. Per D-006 (§12), (a) and (b) share a single construction path — preview is a side-effect-free view of the same aggregate that (b) persists.
2. **Build the preview document from the same construction path used by save** so preview and saved output cannot diverge. Per D-001 (§12) + D-006 (§12): `Form Input → Build Faktur Aggregate → Preview → User Confirmation → Persist Same Aggregate`. No preview-specific mapping, calculation, item transformation, claim handling, pricing refresh, or total logic is permitted. Per D-011 (§12), EDIT preview is built from the current form state including unsaved modifications — the last persisted version is never used as the preview source while editing.
3. **Render preview without side effects** — per D-001 (§12) + D-007 (§12), preview is read-only and non-transactional. It must never trigger GenStok/inventory reduction, piutang creation/update, packing order creation/update, order status updates, Faktur persistence, database commits, or counter consumption. Permitted: read reference data, build the in-memory aggregate, calculate totals/discounts/taxes, construct the print DTO, render the preview. Cancelling preview leaves the system unchanged; save remains the single entry point for all business side effects.
4. **Handle the preview `No.Faktur` by save mode:** EDIT uses the existing number; NEW uses `[DRAFT]` placeholder when no `FakturCode` is assigned, or the operator-pre-selected open code when present, per D-002 (§12). No number reservation.
5. **After save, print the finalized document** using the persisted/reloaded aggregate, per D-005 (§12) + D-011 (§12) + D-012 (§12): preview is a draft review document and only the successfully saved Faktur is the final official printable document; SAVE persists without printing; SAVE & PRINT persists, reloads the persisted Faktur, then prints the final persisted version; print initiates only after a successful save (failed save prevents printing). For EDIT, ensure save never regenerates the existing number (current writer already skips non-empty values).
6. **Make the print DTO null-safe** per D-004 (§12): optional/unavailable display fields fall back to `-`; missing optional data never blocks preview generation and never throws. Save-time mandatory-field validation is unchanged.
7. **Apply the flow to all supported modes** per D-008 (§12): NEW previews the draft (`[DRAFT]`/pre-selected code) and generates the number at save; EDIT previews and preserves the existing number; Faktur Klaim follows the corresponding NEW/EDIT mode with existing business rules unchanged; voided Faktur allows preview/print but save remains unavailable (`FakturForm.cs:450-457`).
8. **Validate at preview with save-level rules** per D-010 (§12): preview executes the same business validation as Save (customer, warehouse, salesperson, due date, at least one item line, all `SaveFakturWorker` guards) before generating the preview; an invalid Faktur is not previewed and the operator sees the validation message. Display-only fields remain tolerant per D-004. Validation logic must not be duplicated between Preview and Save, and validation keeps preview side-effect free per D-007 (reads only, no writes).

No architecture change and no schema change are proposed by this assessment.

---

## 8. Risks

| Risk | Impact | Probability | Mitigation |
| --- | --- | --- | --- |
| NEW preview number differs from final printed number, confusing operators | Medium | High | **Decided by D-002 (§12):** NEW preview is labeled as draft with `[DRAFT]` placeholder when no code is assigned. (EDIT has no such risk — the number is fixed before preview.) |
| EDIT save regenerates a new invoice number, orphaning the original | High | Low | Writer already generates only when `FakturId`/`FakturCode` are empty (`FakturWriter.cs:56-61`); verify EDIT path preserves the loaded values in review. |
| Preview and save produce different totals/items (divergent build logic) | High | Medium | **Decided by D-006 (§12):** single construction path — preview is a side-effect-free view of the same aggregate that will be persisted; no preview-specific logic permitted. Verify preview output equals post-save output for the same input. |
| Preview triggers inventory/receivable side effects | High | Low | **Decided by D-007 (§12):** preview is read-only and non-transactional — no GenStok, piutang, packing order, order status, persistence, commit, or counter consumption; cancelling preview leaves the system unchanged. Assert no such calls on the preview path. |
| `NullReferenceException` on incomplete input (customer/address/items) | Medium | Medium | **Decided by D-004 (§12) + D-010 (§12):** mandatory fields are enforced by save-level validation before preview (invalid Faktur is not previewed); null-safe DTO with `-` fallback covers the remaining optional display data. |
| Preview succeeds but save fails (invalid document reviewed) | Medium | Medium | **Decided by D-010 (§12):** preview executes the same validation as Save, so a previewed document is expected to pass Save validation absent external changes. |
| Counter gaps if Option B is chosen | Medium | Medium | **Not applicable — Option B rejected by D-002 (§12); Option A introduces no counter gaps.** |
| Operator prints the draft instead of the final document | Medium | Medium | **Decided by D-005 (§12) + D-012 (§12):** preview is a draft review document, not an official printed document — only the saved Faktur is final; both SAVE and SAVE & PRINT operate on the finalized persisted Faktur; preview dialog is the decision point, toolbar print is no longer the primary trigger. |
| Print step becomes automatic and floods the printer on cancel/edit | Low | Medium | **Decided by D-005 (§12):** printing is never automatic on preview — SAVE persists without printing; only explicit SAVE & PRINT prints after a successful save. |

---

## 9. Open Questions

Items marked **BLOCKING** prevent planning.

### Business Questions

| ID | Status | Question |
| --- | --- | --- |
| BQ-001 | **RESOLVED by D-002 (§12)** | For a **NEW** Faktur, must the pre-save preview display the **final invoice number** (`No.Faktur`)? **Answer: No. NEW pre-save preview is a draft; it shows `[DRAFT]` when no `FakturCode` is assigned (or the pre-selected open code when present). No number reservation; Option B rejected.** No decision was needed for EDIT — the number already exists on the loaded aggregate. |
| BQ-002 | **RESOLVED by D-005 (§12)** | What exactly is the final "Print" step: automatic print to the default printer, or the existing manual print button in `RdlcViewerForm`? **Answer: neither — the preview dialog provides SAVE (persist, no print) and SAVE & PRINT (persist, then print the finalized document). Print initiates only after a successful save.** |
| BQ-003 | **RESOLVED by D-003 (§12)** | What is the interaction model: a separate **Preview** button, or the existing Save button first shows the preview and asks for confirmation before saving? **Answer: Save opens the preview; save occurs only after Confirm Save from the preview. No separate Preview button.** |
| BQ-004 | | Is the preview the same window reused after save, or two separate preview instances? |
| BQ-005 | **RESOLVED by D-008 (§12)** | Confirm the flow applies identically to **NEW and EDIT**, and whether **Faktur Klaim** and **voided** faktur (Save hidden, `FakturForm.cs:450-457`) are included. **Answer: Yes — NEW (draft preview, number at save), EDIT (existing number preserved), Faktur Klaim (same as corresponding mode), voided Faktur (preview/print allowed, save unavailable).** |

### Technical Questions

| ID | Status | Question |
| --- | --- | --- |
| TQ-001 | **RESOLVED by D-001 (§12)** | Should the preview aggregate be produced by a shared builder path used by `SaveFakturWorker`, or by a form-local mapping? (Affects divergence risk.) **Decision: shared `FakturBuilder` construction rules; preview builds `FakturModel` in memory from `SaveFakturRequest` with no form-local divergent mapping.** |
| TQ-002 | **MOOT — Option B rejected by D-002 (§12)** | If Option B is chosen, where does ID/code reservation live, and how are unused numbers returned to `BTR_FakturCodeOpen`? |
| TQ-003 | **RESOLVED by D-010 (§12)** | Should preview validate required fields up front (customer, salesperson, warehouse, due date) to avoid incomplete report data? **Answer: Yes — preview executes the same business validation rules as Save (customer, warehouse, salesperson, due date, at least one item line, all `SaveFakturWorker` guards); an invalid Faktur is not previewed and the operator sees the validation message. Display-only fields remain tolerant per D-004.** |
| TQ-004 | **RESOLVED by D-011 (§12)** | Should previewing an **existing** Faktur be allowed without re-saving, and should it print the persisted version? **Answer: EDIT preview is generated from the current form state including unsaved modifications — never the last persisted version. Final printing (SAVE & PRINT) reloads and prints the successfully persisted Faktur; if save fails, no print occurs.** |

### Operational Questions

| ID | Status | Question |
| --- | --- | --- |
| OQ-001 | **RESOLVED by D-009 (§12)** | Are abandoned/stale previews auditable, and should any logging be added? **Answer: No — preview sessions are not logged or audited; abandoned-preview tracking is out of scope.** |
| OQ-002 | **RESOLVED by D-012 (§12)** | Is there a paper/printing policy (draft vs final copies) that the new flow must respect? **Answer: preview is a draft review document, not an official printed document; only the successfully saved Faktur is final. SAVE does not print; SAVE & PRINT prints only the saved Faktur. No special paper control, draft-print audit, or paper-stock distinction required.** |

---

## 10. Implementation Impact Inventory

### Backend

- `FakturForm` save/print sequence (`FakturForm.cs:918-1086`) — Save becomes preview entry point per D-003; persist only after confirm.
- `SaveFakturWorker` / `FakturWriter` — no change (D-002 rejects Option B; numbering stays at save).
- `FakturBuilder` — single construction path for preview and save per D-006 (§12); preview is a side-effect-free view of the same aggregate that will be persisted (implementation detail for planning).
- `SaveFakturRequest` mapping — preview and save share the same construction path, not a duplicated mapping.

### Database

- None expected for Option A (decided by D-002).
- ~~Option B would touch number reservation semantics and `BTR_FakturCodeOpen` behavior~~ — not applicable; Option B rejected by D-002 (§12).
- No preview-session tables or audit-trail entries per D-009 (§12); save-related audit behavior unchanged.

### Frontend

- `FakturForm.cs` — Save opens preview, confirm persists; construct preview DTO; null guards.
- `FakturForm.Designer.cs` — no Preview button; Save remains single primary action (D-003).
- `FakturPrintOutDto.cs` — null-safe with `-` fallback for missing optional display data (D-004); draft-number handling (D-002).
- `RdlcViewerForm` / preview host — SAVE + SAVE & PRINT + Cancel / Back to Edit (D-003, D-005); SAVE persists without printing, SAVE & PRINT persists then prints the finalized document.

### Integration

- None.

### Security

- None.

---

## 11. Planning Readiness

### Status

```text
READY
```

The request is understood and the technical path is viable. **All blocking decisions are closed**: the EDIT scope, NEW numbering (D-001/D-002), interaction model (D-003), print trigger (D-005), and mode scope (D-008: NEW, EDIT, Faktur Klaim, voided) — see §12.

### Blocking Issues

None. All blocking business questions resolved: BQ-001 (D-002), BQ-002 (D-005), BQ-003 (D-003), BQ-005 (D-008).

### Planner Guidance

- **Implementation scope:** concentrated in `FakturForm` and its print DTO; likely no database or backend-domain change under Option A. Two mode paths: EDIT (number already known) and NEW (number generated at save).
- **Major dependencies:** NEW/EDIT number display decided by D-002 (§12); single `FakturBuilder` construction path for preview and save decided by D-006 (§12, refines D-001); null-safe print DTO with `-` fallback decided by D-004 (§12); confirmation that the EDIT path preserves the existing number.
- **Sequencing concerns:** all blocking decisions closed (D-001 through D-008); plan the shared Save → Preview → SAVE / SAVE & PRINT flow once and cover all modes (NEW, EDIT, Faktur Klaim, voided) per D-008; prove preview output equals saved output for each applicable mode.
- **Review concerns:** confirm preview is read-only and non-transactional with no inventory/receivable/packing-order/commit/counter side effects and that cancelling leaves the system unchanged (enforced by D-001 + D-007, §12); confirm preview and save share a single construction path with no preview-specific logic (enforced by D-006, §12); confirm EDIT save never regenerates the invoice number; confirm NEW draft preview shows `[DRAFT]` per D-002 (§12) and is visually distinguishable from the final document; confirm SAVE prints nothing and SAVE & PRINT reloads and prints the successfully persisted document only after a successful save (failed save prevents printing) per D-005 + D-011 (§12); confirm voided Faktur allows preview/print with save unavailable and Faktur Klaim follows its corresponding mode per D-008 (§12); confirm preview enforces save-level validation with shared (non-duplicated) logic and shows the message instead of previewing invalid documents per D-010 (§12); confirm no preview-session logging/audit artifacts are introduced per D-009 (§12); no counter-gap review needed (Option B rejected).

## 12. Closing Decisions

### D-001 — Preview Must Be Built From In-Memory Aggregate

| Field | Value |
| --- | --- |
| Decision | APPROVED |
| Type | Architectural Decision |
| Priority | Mandatory |
| Date | 2026-09-12 |
| Closes | GAP-001 |
| Constrains | GAP-006 (shared construction rules), GAP-007 (side-effect-free preview), TQ-001 |

**Statement**

The preview document shall be generated from an in-memory `FakturModel` built from the current form input, without executing any persistence or business side-effect operations.

**Rationale**

The requested business flow is:

```text
Preview → Save → Print
```

A preview that depends on a persisted aggregate cannot satisfy this requirement because persistence would already have occurred before the preview is shown.

The system already has the capability to construct a complete `FakturModel` in memory through `IFakturBuilder` without performing any database writes. Therefore, preview generation should use the builder path rather than the persisted/reloaded aggregate path.

**Implications**

- Preview must not call:
  - `FakturWriter.Save()`
  - `IGenStokFakturWorker`
  - Piutang creation/update
  - Packing Order creation/update
  - Order mapping updates
  - Any database commit operation

- Preview must use the same aggregate-construction rules as Save to avoid output divergence.

- The preview output should be generated from:

```text
Form Input
    ↓
SaveFakturRequest
    ↓
FakturBuilder
    ↓
FakturModel
    ↓
FakturPrintOutDto
    ↓
RDLC Preview
```

**Expected Outcome**

The system gains a true preview-before-save capability while preserving all existing save-side business rules and side effects exclusively within the save operation.

### D-002 — NEW Faktur Preview Uses Draft Invoice Number

| Field | Value |
| --- | --- |
| Decision | APPROVED |
| Type | Business / UX Decision |
| Priority | Mandatory |
| Date | 2026-09-12 |
| Closes | GAP-002 |
| Resolves | BQ-001 |
| Constrains | GAP-005 (draft vs final print distinction), GAP-008 (NEW vs EDIT mode handling) |

**Statement**

For NEW Faktur, the pre-save preview is treated as a draft document. If no `FakturCode` has been assigned yet, the report shall display a placeholder value instead of a final invoice number.

```text
[DRAFT]
```

If the operator has pre-selected an open invoice code via the existing open-code browser, the preview shows that pre-selected code. The final invoice number is assigned only at save by `FakturWriter.Save` and appears on the post-save print.

For EDIT Faktur, no change: the preview shows the existing persisted `FakturCode`, and save must not regenerate it.

**Rationale**

`FakturId`/`FakturCode` for NEW are generated inside `FakturWriter.Save` (`FakturWriter.cs:56-61`). Generating or reserving the final number before preview (Option B) would introduce counter gaps on abandoned previews and conflict with the `BTR_FakturCodeOpen` model. Treating the NEW preview as a draft preserves existing numbering semantics with zero backend/counter change.

**Implications**

- NEW pre-save `No.Faktur` rule:
  - `FakturCode` empty → display `[DRAFT]`.
  - `FakturCode` pre-selected → display the pre-selected code.
- No number reservation, no void/reuse mechanism, no `FakturWriter`/`SaveFakturWorker` change.
- `FakturPrintOutDto` must tolerate an empty `FakturCode` and render the `[DRAFT]` placeholder.
- The draft preview must be visually distinguishable from the post-save final print (see GAP-005).
- EDIT path unchanged: preview and print use the loaded persisted number.

**Expected Outcome**

GAP-002 is closed. NEW preview can be planned and built under Option A with no numbering side effects; operators verify content on a clearly-marked draft before save, then receive the final numbered document after save.

### D-003 — Save Button Becomes Preview Entry Point

| Field | Value |
| --- | --- |
| Decision | APPROVED |
| Type | UX Decision |
| Priority | Mandatory |
| Date | 2026-09-12 |
| Closes | GAP-003 |
| Resolves | BQ-003 |

**Statement**

The existing **Save** button shall remain the primary action on the form. When clicked, it shall first generate and display a preview of the Faktur.

The save operation shall occur only after the operator explicitly confirms from the preview.

The resulting user flow becomes:

```text
Input Data
    ↓
Save Button
    ↓
Preview
    ↓
Confirm Save
    ↓
Persist Faktur
    ↓
Print Final Document
```

**Rationale**

The requested business workflow is:

```text
Preview → Save → Print
```

Adding a separate Preview button introduces an additional action that operators must learn and remember to use. In practice, many users will continue pressing Save directly, bypassing the intended review step.

By making Save the entry point to Preview:

- The review step becomes mandatory.
- Existing user habits remain valid.
- No additional primary action is introduced.
- The workflow matches the business objective of preventing incorrect invoices before persistence.

**Alternatives Considered**

**Option A — Separate Preview Button**

```text
Preview
Save
```

Rejected because:

- Users may ignore Preview and continue using Save directly.
- The review step becomes optional.
- Additional UI complexity with limited benefit.

**Option B — Save Opens Preview (Selected)**

```text
Save
    ↓
Preview
    ↓
Confirm Save
```

Accepted because:

- Enforces document verification.
- Preserves a single primary action.
- Requires minimal UI change.

**Implications**

- No dedicated Preview button is required.
- Save no longer immediately persists data.
- Preview must provide:
  - Confirm Save
  - Cancel / Back to Edit
- Existing save-side business logic remains unchanged and executes only after confirmation.

**Expected Outcome**

Every Faktur is reviewed before persistence while preserving a simple, single-action user experience.

### D-004 — Missing Preview Data Shall Be Rendered As Dash ("-")

| Field | Value |
| --- | --- |
| Decision | APPROVED |
| Type | UX / Robustness Decision |
| Priority | Mandatory |
| Date | 2026-09-12 |
| Closes | GAP-004 |

**Statement**

When generating a Faktur preview, any optional or unavailable data shall be rendered as a dash (`-`) rather than causing an exception or displaying empty values.

Examples:

| Field | Value in Preview |
| --------- | --------- |
| Customer Address2 = null | `-` |
| Customer Kota = null | `-` |
| Missing optional text field | `-` |
| Empty collection lookup result | `-` |

**Rationale**

The purpose of the preview is to allow the operator to verify the document before saving. The preview should remain available even when some non-critical information is incomplete.

Throwing a `NullReferenceException` provides no business value and prevents the operator from reviewing the document.

Displaying a dash:

- Clearly indicates that data is unavailable.
- Preserves preview usability.
- Avoids application errors.
- Requires minimal implementation effort.
- Matches common document-printing conventions.

**Implications**

- `FakturPrintOutDto` must be null-safe.
- Optional fields shall use `"-"` as the display fallback.
- Preview generation must never fail solely because an optional field is missing.
- Missing optional data does not block preview generation.

**Non-Goals**

This decision does not relax save-time business validation rules. Fields that are mandatory for persistence may still be validated during Save.

**Expected Outcome**

The preview remains stable and usable even when optional customer or document information is incomplete, eliminating `NullReferenceException` risks caused by missing display data.

### D-005 — Preview Dialog Provides SAVE and SAVE & PRINT Actions

| Field | Value |
| --- | --- |
| Decision | APPROVED |
| Type | UX Decision |
| Priority | Mandatory |
| Date | 2026-09-12 |
| Closes | GAP-005 |
| Resolves | BQ-002 |
| Refines | D-003 (Confirm Save becomes two explicit actions) |

**Statement**

The preview dialog shall provide two explicit actions:

| Action | Behavior |
| ---------- | ---------- |
| **SAVE** | Persist the Faktur and close the preview without printing. |
| **SAVE & PRINT** | Persist the Faktur and immediately invoke the print action for the finalized document. |

The resulting workflow becomes:

```text
Input Data
    ↓
Save Button
    ↓
Preview Dialog
    ├─ SAVE
    │      ↓
    │   Save Faktur
    │      ↓
    │    Finish
    │
    └─ SAVE & PRINT
           ↓
        Save Faktur
           ↓
        Print Final Faktur
           ↓
         Finish
```

**Rationale**

The original requirement introduces two distinct operator intentions:

1. Verify and save the transaction.
2. Verify, save, and immediately print the document.

Providing separate actions makes the operator's intent explicit and removes ambiguity regarding post-save printing behavior.

This approach also avoids forcing automatic printing when the operator only wants to save the transaction.

**Implications**

- The existing preview dialog becomes the decision point before persistence.
- The current toolbar print action is no longer the primary workflow trigger.
- Printing is initiated only after a successful save.
- Failed save operations must prevent printing.
- Both actions operate on the finalized persisted Faktur.
- D-003 Cancel / Back to Edit is preserved: closing the preview without choosing SAVE or SAVE & PRINT persists nothing.

**Expected Outcome**

The system supports both operational scenarios:

- Save without printing.
- Save and immediately print.

without requiring additional dialogs or ambiguous print behavior.

### D-006 — Preview and Save Must Share a Single Faktur Construction Path

| Field | Value |
| --- | --- |
| Decision | APPROVED |
| Type | Architectural Decision |
| Priority | Mandatory |
| Date | 2026-09-12 |
| Closes | GAP-006 |
| Refines | D-001 (same construction rules become a single shared path; preview persists the same aggregate) |

**Statement**

The preview document and the save operation shall be generated from the same Faktur construction path.

The system shall not introduce a separate preview-specific aggregate mapping, calculation logic, item transformation, claim handling, pricing refresh behavior, or total calculation logic.

A single Faktur construction path shall be the source of truth for:

- Item mapping
- Quantity calculations
- Discount calculations
- Tax calculations
- Claim processing
- Price refresh behavior (`isRefreshHrg`)
- Grand total calculation

**Rationale**

Maintaining separate construction logic for Preview and Save creates a high risk that the document shown to the operator differs from the document ultimately persisted.

Examples of divergence include:

- Different item totals
- Different discounts
- Different tax values
- Different claim calculations
- Different pricing refresh behavior

The operator must review exactly the same document that will later be saved.

**Implementation Principle**

The workflow should follow:

```text
Form Input
    ↓
Build Faktur Aggregate
    ↓
Preview
    ↓
User Confirmation
    ↓
Persist Same Aggregate
```

and not:

```text
Form Input
    ↓
Build Preview Aggregate
    ↓
Preview

Form Input
    ↓
Build Save Aggregate
    ↓
Persist
```

**Implications**

- Only one aggregate-construction path is permitted.
- Preview becomes a side-effect-free view of the same aggregate that will later be persisted.
- Any future calculation changes automatically affect both Preview and Save.
- Divergence risk between Preview and Save is eliminated.

**Expected Outcome**

The operator always reviews the exact Faktur that will be saved, ensuring totals, items, pricing, claims, and tax calculations remain consistent between Preview and persistence.

### D-007 — Preview Shall Remain Side-Effect Free

| Field | Value |
| --- | --- |
| Decision | APPROVED |
| Type | Architectural Constraint |
| Priority | Mandatory |
| Date | 2026-09-12 |
| Closes | GAP-007 |
| Reinforces | D-001 (in-memory preview with no persistence or side effects) |

**Statement**

The preview operation shall remain a read-only, side-effect-free process.

Generating a preview must not create, modify, or commit any business transaction data, nor trigger any downstream business process.

**Rationale**

The purpose of preview is to allow the operator to review the document before deciding whether the transaction should be persisted.

Executing business side effects during preview would violate the requested workflow:

```text
Preview → Save → Print
```

and could result in inventory, receivable, or operational data being changed even when the operator cancels the transaction.

The current architecture already satisfies this requirement because aggregate construction can be performed entirely in memory without persistence.

**Protected Business Operations**

The preview operation must never trigger:

- Inventory reduction (`GenStok`)
- Piutang creation or update
- Packing Order creation or update
- Order status updates
- Faktur persistence
- Database commits
- Counter consumption
- Any other transactional side effect

**Permitted Operations**

The preview operation may:

- Read reference data
- Build an in-memory Faktur aggregate
- Calculate totals, discounts, and taxes
- Construct the print DTO
- Render the preview document

**Implications**

- Preview remains a non-transactional operation.
- Save remains the single entry point for business side effects.
- Cancelling preview leaves the system unchanged.
- Existing inventory and accounting behavior remains unchanged.

**Expected Outcome**

Operators can freely preview, close, or cancel a document without affecting inventory, receivables, packing orders, order status, numbering, or any persisted business data.

### D-008 — Preview → Save → Print Flow Applies To All Supported Faktur Modes

| Field | Value |
| --- | --- |
| Decision | APPROVED |
| Type | Business / UX Decision |
| Priority | Mandatory |
| Date | 2026-09-12 |
| Closes | GAP-008 |
| Resolves | BQ-005 |

**Statement**

The Preview → Save → Print workflow shall be applied consistently across all supported Faktur entry modes.

| Mode | Preview Behavior | Save Behavior |
| -------- | -------- | -------- |
| NEW Faktur | Preview displays draft number or placeholder per D-002 | Save generates the final invoice number |
| EDIT Faktur | Preview displays the existing invoice number | Save preserves the existing invoice number |
| Faktur Klaim | Same workflow as corresponding NEW or EDIT mode | Existing business rules remain unchanged |
| Voided Faktur | Preview and print are allowed; save remains unavailable | No save operation permitted |

**Rationale**

The workflow should be predictable and consistent regardless of how the Faktur was created.

Operators should not need to learn different preview behaviors for different Faktur types.

The existing domain behavior already distinguishes NEW and EDIT through aggregate state and save logic. The preview workflow should follow the same distinction rather than introducing mode-specific user experiences.

Voided Faktur are already prevented from being saved. The new workflow must preserve this existing business rule.

**Implications**

- NEW Faktur continues to generate the invoice number only during save.
- EDIT Faktur continues to retain its existing invoice number.
- Faktur Klaim follows the same workflow as regular Faktur.
- Voided Faktur cannot be saved regardless of preview availability.
- No additional numbering rules are introduced.

**Expected Outcome**

All Faktur variants follow a consistent Preview → Save → Print experience while preserving their existing business rules and lifecycle constraints.

### D-009 — Preview Sessions Shall Not Be Logged or Audited

| Field | Value |
| --- | --- |
| Decision | APPROVED |
| Type | Scope Decision |
| Priority | Low |
| Date | 2026-09-12 |
| Closes | GAP-009 |
| Resolves | OQ-001 |

**Statement**

Preview sessions shall not be recorded, audited, or persisted.

Opening, viewing, closing, or abandoning a preview does not create a business transaction and therefore does not require audit-trail or operational logging.

Only successful save operations shall remain subject to existing business records and audit requirements.

**Rationale**

The purpose of preview is to allow the operator to inspect a document before deciding whether it should be persisted.

A preview:

- Does not modify business data.
- Does not consume inventory.
- Does not create receivables.
- Does not create packing orders.
- Does not change order status.
- Does not generate a committed Faktur.

Recording abandoned previews would introduce additional implementation complexity without providing meaningful business value for the requested feature.

The requested enhancement concerns document verification before save, not user activity tracking.

**Implications**

- No new database tables are required.
- No audit-trail entries are created for preview actions.
- No abandoned-preview tracking is implemented.
- Existing save-related audit behavior remains unchanged.

**Out of Scope**

The following are explicitly excluded from this change:

- Preview activity logging
- Preview history
- Preview analytics
- Preview audit reporting
- Abandoned preview tracking

**Expected Outcome**

Operators can freely open, review, close, and abandon previews without generating audit records or operational data, while all existing save-related business tracking remains unchanged.

### D-010 — Preview Shall Execute Save-Level Business Validation

| Field | Value |
| --- | --- |
| Decision | APPROVED |
| Type | Business / UX Decision |
| Priority | Mandatory |
| Date | 2026-09-12 |
| Resolves | TQ-003 |
| Constrains | D-004 (display-only tolerance), D-007 (validation keeps preview side-effect free) |

**Statement**

The Preview operation shall execute the same business validation rules required by the Save operation.

A Faktur that cannot be saved shall not be previewed.

If validation fails, the preview shall not be generated and the operator shall be shown the corresponding validation message.

**Rationale**

The purpose of Preview is to allow the operator to verify the exact document that can later be persisted.

Allowing preview generation for an invalid Faktur creates an inconsistent workflow:

```text
Preview Success
    ↓
Save Failed
```

This leads to confusion because the operator reviews a document that cannot actually become a valid business transaction.

Preview should therefore represent a "ready-to-save" document.

**Validation Scope**

Preview shall validate all mandatory business requirements currently enforced by Save, including but not limited to:

- Customer selection
- Warehouse selection
- Salesperson selection
- Due date requirements
- At least one item line
- Any existing SaveFakturWorker business guards

**Permitted Differences**

Display-only fields remain tolerant of missing values and shall follow D-004.

Examples:

- Address2 = "-"
- Kota = "-"
- Optional remarks = "-"

These fields do not affect transaction validity.

**Implications**

- Preview and Save use the same validation rules.
- A document that passes Preview validation is expected to pass Save validation, assuming no external changes occur.
- Validation logic should not be duplicated between Preview and Save.
- Preview remains side-effect free despite executing validation.

**Expected Outcome**

Operators only review Faktur documents that satisfy all mandatory business requirements and are eligible for persistence.

### D-011 — Existing Faktur Preview Shall Reflect Current Edited State

| Field | Value |
| --- | --- |
| Decision | APPROVED |
| Type | Business / UX Decision |
| Priority | Mandatory |
| Date | 2026-09-12 |
| Resolves | TQ-004 |
| Constrains | D-005 (SAVE & PRINT prints the reloaded persisted version), D-006 (preview and save stay consistent) |

**Statement**

When previewing an existing Faktur in EDIT mode, the preview shall be generated from the current form state, including any unsaved modifications made by the operator.

The preview shall not automatically revert to or display the last persisted version from the database.

**Rationale**

The purpose of preview is to allow the operator to verify the document that will be saved.

Displaying the persisted version while unsaved changes exist creates a misleading workflow:

```text
Edit Faktur
    ↓
Preview
    ↓
Shows Old Persisted Version
    ↓
Save
    ↓
Persists Different Data
```

In this scenario, the operator reviews one document but saves another.

Instead, the workflow should remain:

```text
Edit Faktur
    ↓
Preview
    ↓
Shows Current Edited Version
    ↓
Save
    ↓
Persists Same Version
```

This preserves consistency between what is reviewed and what is ultimately stored.

**Printing Behavior**

If the operator selects **SAVE & PRINT**, the system shall:

```text
Preview Current Changes
    ↓
Save Changes
    ↓
Reload Persisted Faktur
    ↓
Print Final Persisted Version
```

Printing must always use the successfully persisted Faktur.

If save fails, printing shall not occur.

**Implications**

- EDIT preview is generated from the current in-memory state.
- Unsaved changes are visible in preview.
- Persisted data is not used as the preview source while editing.
- Final printing continues to use the saved Faktur.
- Preview and save remain consistent.

**Expected Outcome**

Operators always review the exact version of the Faktur that will be saved, while printed output remains based on the successfully persisted document.

### D-012 — Preview Is Treated As Draft Document; Only Saved Faktur Is Final

| Field | Value |
| --- | --- |
| Decision | APPROVED |
| Type | Operational / UX Decision |
| Priority | Low |
| Date | 2026-09-12 |
| Resolves | OQ-002 |
| Reinforces | D-002 (NEW draft placeholder), D-005 (SAVE vs SAVE & PRINT), D-007 (uncommitted state at preview time) |

**Statement**

The preview document shall be treated as a draft review document.

Only a successfully saved Faktur shall be considered the final printable business document.

The preview exists solely to allow the operator to verify document content before persistence.

**Rationale**

The requested workflow is:

```text
Preview
    ↓
Save
    ↓
Print
```

At preview time:

- The transaction has not yet been committed.
- Inventory has not been reduced.
- Piutang has not been created.
- Packing Order has not been created.
- The transaction may still be cancelled.

Therefore the preview cannot be considered the official business document.

The official document is the persisted Faktur produced after a successful save.

**Printing Policy**

- Preview may be viewed freely by the operator.
- Preview is not considered an official printed document.
- SAVE does not print.
- SAVE & PRINT prints only the successfully saved Faktur.
- If save fails, printing shall not occur.

**Implications**

- No special paper-control process is required.
- No draft-print audit trail is required.
- No distinction between draft paper stock and final paper stock is required.
- Existing printing procedures remain unchanged.

**Expected Outcome**

Operators review a draft representation before saving, while all operational and business processes continue to recognize only the successfully saved Faktur as the official printable document.
