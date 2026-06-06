# Bulk Order Item Input

## What Is This Feature?

Bulk Order Item Input lets a salesperson add **multiple items to a Sales Order in one step** when those items share the same selling terms.

Instead of opening the add-item form once per product, the salesperson:

1. Selects several items at once in the item picker.
2. Enters quantity and discounts **once**.
3. Saves — the app creates **one order line per item**, each with the same qty and discount values.

A typical use case is product variants that differ only by flavor or color but share price and packaging — for example:

- DEEDEE MOSQUITO GUAVA 20ML  
- DEEDEE MOSQUITO ORNGE 20ML  
- DEEDEE MOSQUITO STRAW 20ML  

If a customer orders 10 of each, the salesperson selects all three, enters qty 10 once, and gets three separate order lines without repeating the same entry three times.

The feature does **not** merge items into a single order line. Each item still becomes its own `OrderItem` row (with its own `brgId`, name, and line total). That keeps stock tracking, sync, and office Faktur processing unchanged.

---

## Why Does This Feature Exist?

### The Problem

BTrade3 previously required **strictly one-by-one** item entry:

1. Search and pick one item.  
2. Enter qty and discounts.  
3. Save.  
4. Repeat for every variant.

Field sales often encounter orders where several SKUs share identical commercial terms — same unit price, same carton/pcs conversion, same discount. Re-entering the same numbers for each variant is slow, error-prone, and frustrating during customer visits.

Master item data (`barang`) has **no variant grouping** today. There is no parent product or “family” field to drive bulk selection. Building that in master data would help long-term discovery but is not required to solve the immediate operational pain.

### The Design Choice: Price + Unit Profile, Not Variant Group

Rather than waiting for a master-data grouping model, bulk eligibility is based on a **bulk input profile**:

| Field | Role |
|-------|------|
| Unit price (`hrgSat`) | Same selling price per small unit |
| Conversion (`konversi`) | Same qty besar → qty kecil math |
| Large unit (`satBesar`) | Same meaning for qty besar |
| Small unit (`satKecil`) | Same meaning for qty kecil |

Items that match on all four can be bulk-added. Price is the primary signal sales staff recognize; units and conversion prevent incorrect bulk entry when two unrelated products happen to share a price.

### What the Feature Delivers

**For salespersons**

- Faster entry when customers order multiple variants with the same qty.  
- Multi-select in the item picker with clear feedback on what can be combined.  
- Search shortcuts: chips to “select all” items in a matching price group from search results.  
- Single-item flow unchanged — no learning curve for simple orders.

**For the business**

- No database or API schema changes.  
- Each line remains a normal order item for sync and Faktur creation.  
- Reduced data-entry errors from repetitive typing.

**For engineering**

- Logic lives in BTrade3 mobile (`BulkInputProfile`, selection ViewModel, bulk `saveItems()`).  
- Backward compatible with existing single-item navigation and edit flows.

---

## Scope and Limits

| In scope | Out of scope |
|----------|--------------|
| Multi-select on item picker | Master `barang` variant grouping |
| Same-profile validation | Bulk **edit** of existing lines |
| One qty form → N order lines | Combined single line for multiple SKUs |
| Search “select all” chips for groups of 2+ | Office/desktop BTrade bulk entry |

---

## Related Documentation

- Operational guide: [bulk-input-operational.md](bulk-input-operational.md)  
- Sales Order lifecycle: [../../features/sales-order/feature.md](../../features/sales-order/feature.md)
