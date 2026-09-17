Business Rules

- Faktur creation immediately reduces inventory.
- Customer arrears generate warning only.
- Admin may continue Faktur creation.
- One Faktur belongs to exactly one Warehouse.

## Preview Before Save (D-002)

- Pre-save preview is a draft document; it triggers no persistence or business side effects.
- For NEW Faktur with no `FakturCode` assigned yet, the preview `No.Faktur` displays `[DRAFT]`.
- If an open invoice code was pre-selected, the preview shows that pre-selected code.
- The final invoice number is assigned only at save and appears on the post-save print.
- For EDIT Faktur, the preview shows the existing persisted invoice number; save must not regenerate it.

## Save Opens Preview (D-003)

- The existing Save button is the single primary action; no separate Preview button.
- Clicking Save renders the pre-save preview first; nothing is persisted yet.
- Persistence occurs only after Confirm Save from the preview.
- The preview offers Confirm Save and Cancel / Back to Edit.
- Flow: Input Data → Save Button → Preview → Confirm Save → Persist Faktur → Print Final Document.

## Preview Save Actions (D-005)

- The preview dialog provides SAVE (persist, no print) and SAVE & PRINT (persist, then print the finalized document).
- Printing initiates only after a successful save; failed save prevents printing.
- Both actions operate on the finalized persisted Faktur.

## Preview Robustness (D-004)

- Missing optional preview data renders as `-`; preview never fails solely for missing optional fields.
- Save-time mandatory-field validation is unchanged.

## Preview Validation (D-010)

- Preview executes the same business validation rules as Save; a Faktur that cannot be saved shall not be previewed.
- Failed validation shows the corresponding message and no preview is generated.
- Display-only fields remain tolerant per D-004 and do not affect transaction validity.

## EDIT Preview State (D-011)

- EDIT preview is generated from the current form state including unsaved modifications, never the last persisted version.
- SAVE & PRINT reloads and prints the successfully persisted Faktur; if save fails, no print occurs.

## Draft vs Final Document (D-012)

- The preview is a draft review document, not an official printed document.
- Only the successfully saved Faktur is the final official printable document.
- No special paper control, draft-print audit, or paper-stock distinction is required.

## Preview Mode Coverage (D-008)

- The Preview → Save → Print workflow applies to all supported Faktur entry modes.
- NEW Faktur: preview shows the draft number/placeholder; save generates the final invoice number.
- EDIT Faktur: preview shows the existing invoice number; save preserves it.
- Faktur Klaim: follows the same workflow as the corresponding NEW or EDIT mode; existing business rules unchanged.
- Voided Faktur: preview and print are allowed; save remains unavailable.

## Void (Cancellation)

Void is triggered only from **Faktur Control** by unchecking the **Posted** checkbox.

### Void Reason (mandatory for new voids)

Every new void operation must record a business reason before completion.

| Code | Display | Meaning |
| ---- | ------- | ------- |
| 1 | Salah Input | Invoice created incorrectly or for testing only |
| 2 | Revisi | Invoice cancelled because a completely new invoice will be created (not invoice editing) |
| 3 | Customer Reject | Invoice cannot be delivered because customer rejects delivery |

- `VoidReasonNote` is optional (max 200 characters).
- Historical voided records may have `VoidReasonCode = 0` (valid; no migration required).
- Void side effects (stock rollback, piutang removal, soft delete) are unchanged; only metadata is added.
