# How to Use Selective Sales Order Sync

This guide is for **salespersons** using the BTrade3 mobile app. It explains how to finish an order and send it to the office — without accidentally syncing work that is not done yet.

---

## Before You Start

- Master data (customers, items, sales persons) should already be synced to your device. Use **Manage Customers** and **Sync Master Data** from the menu if needed.
- You can create and edit orders **without internet**. Sync requires a connection.
- Every new order starts as **In Progress**. It will **not** be sent until you finish it and explicitly sync it.

---

## Order Statuses on Your Screen

You will see one of these labels on each sales order:

| Status | What it means | Can you edit? | Can you sync? |
|---|---|---|---|
| **In Progress** | You are still entering the order | Yes | No |
| **Ready** | Order is complete and waiting to be sent | No (reopen first) | Yes |
| **Sent** | Order has been sent to the office | No | No (already sent) |

---

## Step 1 — Create and Enter the Order

1. Open **Sales Orders**.
2. Tap the **+** button to create a new order.
3. Select **Customer** and **Sales** person.
4. Tap **View / Edit Items** and add at least one item.
5. Add an **Order Note** for admin if needed.

The order is saved automatically as you work. You can leave and come back later — it stays **In Progress** until you finish it.

> **Tip:** If you are interrupted mid-visit, simply go back to the order list. The unfinished order will not be sent when you sync later.

---

## Step 2 — Finish the Order

When the order is complete:

1. Open the order from the list (or stay on the entry screen).
2. Scroll down and tap **Finish Order**.

The app checks that you have:

- A customer
- A sales person
- At least one item

If something is missing, you will see a message telling you what to fix.

After a successful finish, the status changes to **Ready**.

### Need to change something after finishing?

1. Open the **Ready** order.
2. Tap **Reopen for Editing**.
3. Make your changes.
4. Tap **Finish Order** again when done.

---

## Step 3 — Send the Order to the Office

You have **two ways** to sync. Use whichever fits your situation.

### Option A — Sync one order from the list (after a visit)

Best when you just finished one customer order and have internet.

1. Go to **Sales Orders**.
2. Find the order with status **Ready**.
3. Tap the **⋮** menu on the order card.
4. Tap **Sync**.
5. Confirm **Sync** in the dialog.
6. Wait for the progress message, then check the result toast.

The order status changes to **Sent** when successful.

> **Note:** The **Sync** menu option only appears on **Ready** orders. You will not see it on **In Progress** or **Sent** orders.

### Option B — Sync multiple orders from Sync Transaction (end of day)

Best when you want to review and send several orders at once.

1. Open the **⋮** menu on the Sales Orders screen.
2. Tap **Sync Transaction**.
3. Under **Ready to Sync**, review the list of finished orders.
   - All ready orders are selected by default.
   - Uncheck any order you do not want to send yet.
   - Use **Select All** or **Clear** to adjust quickly.
4. Tap **Sync Selected (N)** where N is the number of checked orders.
5. Wait for the sync progress to complete.

Orders that sync successfully are marked **Sent**.

### Still Editing section

At the bottom of **Sync Transaction**, you may see **Still Editing (N)**. These are **In Progress** orders. They are listed for your awareness only — they cannot be selected or synced from this screen. Finish them first (Step 2).

---

## Typical Daily Workflows

### Workflow 1 — Sync after each visit

```text
Visit customer → enter order → Finish Order → ⋮ → Sync → Sent
```

### Workflow 2 — Sync at end of day

```text
Visit 1 → order (In Progress) → Finish → Ready
Visit 2 → order (In Progress) → Finish → Ready
...
End of day → Sync Transaction → select orders → Sync Selected → Sent
```

### Workflow 3 — Finish today, sync tomorrow

```text
Today:   create orders → Finish Order → Ready (stay on device)
Tomorrow: Sync Transaction or per-order Sync when online → Sent
```

---

## What Happens After Sync?

- On your phone, the order shows **Sent** and cannot be edited or synced again.
- At the office, the order becomes available for processing and eventual Faktur creation.
- If sync fails (no connection, server error), the order stays **Ready**. Try again when connectivity is restored.

---

## Frequently Asked Questions

### Why is my order not on the sync screen?

Only **Ready** orders appear. If your order is **In Progress**, tap **Finish Order** first.

### I tapped New Order but did not finish it. Will it sync?

No. **In Progress** orders are never sent automatically.

### Can I delete an order I no longer need?

Yes. From the order list, use the **⋮** menu → **Delete**, or long-press to select multiple orders and delete in bulk.

### Can I sync Check-In data from the same screen?

Yes. **Sync Transaction** also has **Sync Check-In Data**. That is separate from sales order sync and does not affect your order statuses.

### I finished an order by mistake. What do I do?

Open the order → **Reopen for Editing** → fix it → **Finish Order** again. Do not sync until you are satisfied.

---

## Quick Reference

| I want to… | Do this |
|---|---|
| Start a new order | Sales Orders → **+** |
| Add items | Open order → **View / Edit Items** |
| Mark order complete | Open order → **Finish Order** |
| Fix a ready order | **Reopen for Editing** → edit → **Finish Order** |
| Send one order | Order list → **⋮** → **Sync** (Ready only) |
| Send several orders | Menu → **Sync Transaction** → **Sync Selected** |
| See what was already sent | Look for status **Sent** on the order list |

---

## Related Documentation

- [feature.md](feature.md) — Sales Order domain and office lifecycle
- [selective-sync-feature.md](selective-sync-feature.md) — What this feature is and why it exists
