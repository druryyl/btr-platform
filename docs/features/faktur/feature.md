Business Rules

- Faktur creation immediately reduces inventory.
- Customer arrears generate warning only.
- Admin may continue Faktur creation.
- One Faktur belongs to exactly one Warehouse.

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
