# How to Use Bulk Order Item Input

This guide is for **salespersons** using the BTrade3 mobile app. It explains how to add several items at once when they share the same price and unit.

---

## Before You Start

- Master item data must be synced to your device (**Sync Master Data** from the menu).
- Bulk input works only when adding **new** items. Editing an existing line still uses the single-item form.
- Each item you bulk-add appears as its **own line** on the order — you can edit or remove lines individually afterward.

---

## When to Use Bulk vs Single Entry

| Situation | Use |
|-----------|-----|
| One product, one qty | **Single** mode (default) — tap item, enter qty, save |
| Several variants, **same price and unit**, **same qty each** | **Bulk** mode |
| Same price but **different** carton/pcs setup | **Single** mode — bulk is blocked for mismatched items |
| Different qty per variant | **Single** mode — enter each line separately |

**Example — good for bulk:**  
Customer wants 10 pcs of GUAVA, 10 pcs of ORANGE, and 10 pcs of STRAW (all same price).

**Example — not for bulk:**  
Customer wants 10 GUAVA and 5 ORANGE — enter each variant separately.

---

## Step 1 — Open Add Item

1. Open a Sales Order (**In Progress**).
2. Tap **View / Edit Items**.
3. Tap **+** to add an item.

You land on the **Add Item** screen.

---

## Step 2 — Turn On Bulk Mode and Select Items

1. Tap **Search for an item...** (or **Change selection** if you already picked items).
2. On the **Select Items** screen, tap **Bulk** in the top-right corner.

Bulk mode is now active. The list shows checkboxes.

3. **Search** for the products (e.g. `DEEDEE MOSQUITO`).
4. Select items by tapping each row (checkbox toggles).

### Rules while selecting

- The **first** item you pick sets the allowed price and unit profile.
- After that, only items with the **same price and same units** can be selected.
- Items that do not match appear faded and cannot be checked.
- Tap **Clear** (top-right) to start selection over.

### Shortcut — Select all from search

When your search returns **two or more** items with the same price and unit, chips appear above the list, for example:

**`3 items @ Rp 12.000`**

Tap the chip to select all items in that group at once.

5. When finished, tap **Continue with N items** on the bottom bar.

---

## Step 3 — Enter Quantity and Discounts Once

You return to the add screen titled **Add N Items**.

- The top card lists every selected product name and code.
- **Shared price** is shown once (e.g. `Rp 12.000/pcs`).
- Enter **Qty Besar**, **Qty Kecil**, **Qty Bonus**, and **Discounts** as you would for a single item.

These values apply **equally to every selected item**.

The **Order Summary** shows:

- Per-item breakdown  
- **Grand Total** across all selected lines (before you save)

---

## Step 4 — Save

1. Enter at least one quantity (besar, kecil, or bonus must be greater than zero).
2. Tap **Add N items to order**.

The app creates **one order line per item**, each with the same qty and discounts.

3. You return to the item list and see all new lines listed separately.

---

## Switch Back to Single Item Mode

On the item picker screen, tap **Single** in the top-right corner.

- Checkboxes disappear.  
- Tapping an item immediately returns you to Add Item with that one product (original behavior).

---

## Edit or Remove Bulk-Added Lines

Bulk input only affects **how lines are created**. After saving:

- Tap a line to **edit** qty or discounts for that item only.  
- Swipe or use delete to **remove** individual lines.

There is no “bulk edit” for existing lines.

---

## Troubleshooting

| Issue | What to do |
|-------|------------|
| Cannot check an item | It has a different price or unit than your first selection. Use Single mode, or clear selection and pick a matching set. |
| No “select all” chip | Fewer than two matching items in search results, or items differ in price/unit. Select manually or adjust search. |
| **Continue** not visible | Bulk mode is off, or no items are checked. Enable **Bulk** and select at least one item. |
| Save button disabled | Enter at least one qty (besar, kecil, or bonus). |
| Only one line added | You used Single mode or continued with one item — bulk save needs **two or more** selected items. |

---

## Quick Reference

```text
Add Item → Search → Bulk → select matching items → Continue
→ enter qty once → Add N items to order
```

For a single product, skip **Bulk** and tap the item directly.
