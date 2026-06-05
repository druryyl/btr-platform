# Selective Sales Order Sync

## What Is This Feature?

Selective Sales Order Sync is the controlled process by which a salesperson sends finished Sales Orders from the BTrade3 mobile app to the office.

It introduces two ideas that work together:

1. **Order completion** — a salesperson must explicitly mark an order as finished before it can be sent.
2. **Selective synchronization** — the salesperson chooses which finished orders to send, rather than sending every open order automatically.

On the mobile device, a Sales Order now passes through three sync-related states before it reaches the office:

```text
In Progress  →  Ready  →  Sent
```

| Mobile status | Meaning |
|---|---|
| **In Progress** | The order is being entered or edited on the device. It is not eligible for sync. |
| **Ready** | The order is complete and waiting to be sent to the office. |
| **Sent** | The order has been successfully transmitted and is available for office processing. |

After a order is **Sent**, the office system receives it and continues the broader Sales Order lifecycle (Imported → Invoiced) described in [feature.md](feature.md).

---

## Why Does This Feature Exist?

### The Problem

Previously, BTrade3 treated every unsynchronized order as a single **Draft** status. As soon as a salesperson tapped **New Sales Order**, the order was saved locally and included the next time **Sync Transaction** ran.

That caused real operational issues:

- An order still being entered at a customer visit could be sent to the office before it was finished.
- Incomplete orders — missing customer, sales person, or items — could be synchronized unintentionally.
- The salesperson had no moment to review which orders should go to the office and which should stay on the device.
- Office staff could receive partial or incorrect demand records, creating rework before Faktur creation.

Field sales work is interrupt-driven. A salesperson may start an order, pause for another customer, continue later, or sync at end of day. The system must distinguish **“still working on it”** from **“ready for the office.”**

### The Business Need

Sales Orders are the source document for office Faktur creation. What gets synchronized must be:

- **Complete** — customer, sales person, and at least one item are present.
- **Intentional** — the salesperson decided the order is ready.
- **Controlled** — the salesperson decides when and which orders are sent.

Selective sync protects data quality upstream so office processing starts from reliable customer demand.

### What the Feature Delivers

**For salespersons**

- Freedom to create and save orders while offline without fear that unfinished work will be sent.
- A clear **Finish Order** step that signals completion.
- Choice over what to send: one order after a visit, or several at end of day.

**For office operations**

- Fewer incomplete or accidental uploads.
- Imported orders that better reflect actual customer requests.
- Less administrative correction before Faktur creation.

**For the platform**

- A explicit mobile lifecycle that aligns with offline field sales.
- Validation at completion and again at sync time.
- Multiple sync entry points that share the same rules.

---

## Scope

### In Scope

- Marking an order as finished (**Finish Order**).
- Reopening a ready order for editing (**Reopen for Editing**).
- Batch selective sync via **Sync Transaction**.
- Single-order sync from the Sales Order list.
- Status visibility: In Progress, Ready, Sent.
- Database migration of legacy Draft orders to the new statuses.

### Out of Scope

- Faktur creation at the office.
- Automatic background sync without user action.
- Sync of master data (customers, items, sales persons).
- Customer Check-In sync (separate flow on the same Sync Transaction screen).
- Office-side rejection or recall of a sent order.

---

## Business Rules

### Completion Rules (Finish Order)

An order may move from **In Progress** to **Ready** only when:

- A customer is selected.
- A sales person is selected.
- The order contains at least one item.

### Sync Eligibility

An order may be synchronized only when:

- Its status is **Ready**.
- It still satisfies the completion rules above.

Orders in **In Progress** or **Sent** are never included in sync.

### Editing Rules

- **In Progress** orders can be edited (customer, sales person, items, notes).
- **Ready** orders cannot be edited until the salesperson taps **Reopen for Editing**, which returns the order to **In Progress**.
- **Sent** orders cannot be edited on the mobile device.

### Synchronization Behavior

- Successfully synced orders become **Sent** and are not synced again.
- Failed sync attempts leave the order as **Ready** so the salesperson can retry.
- The salesperson always confirms what will be sent before sync proceeds.

---

## Sync Entry Points

Both paths use the same eligibility rules:

| Entry point | Typical use |
|---|---|
| **Sync Transaction** (menu) | End-of-day or batch sync; review and select multiple ready orders |
| **Order list → ⋮ → Sync** | Sync one order immediately after finishing a visit |

---

## Relationship to Sales Order Domain

This feature extends the Sales Order domain for the mobile channel. It does not replace the office lifecycle.

```text
Mobile (BTrade3)                    Office (BTR)
─────────────────                   ─────────────
In Progress
    ↓ Finish Order
Ready
    ↓ Selective Sync
Sent        ──────────────────→     Imported
                                        ↓
                                    Invoiced
```

See [feature.md](feature.md) for the full Sales Order domain model and office-side lifecycle.

---

## Success Criteria

The feature is working as intended when:

- Unfinished orders never appear on the sync selection screen.
- Salespersons can complete a visit, finish the order, and sync it without opening the full sync review screen.
- Salespersons can defer sync and send multiple ready orders together when connectivity is available.
- Office staff receive complete orders that match what the salesperson intended to submit.
