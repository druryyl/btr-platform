# IN01 — Inventory Dashboard KPI Explanation

## Scope

**IN01 — Inventory** answers:

> How much capital is currently held in stock, how broad is the stocked
> product range, and where is inventory value concentrated by category and
> supplier?

**Route:** `/dashboard/inventory`

This is a point-in-time inventory composition dashboard for the Owner,
Director, General Manager, Inventory, and Purchasing managers. Monetary values
are in IDR.

## Important Reading Rules

- Inventory value is measured at **HPP × quantity**, not selling price.
- **In-Transit** warehouse stock is excluded.
- Items with zero net quantity are excluded.
- **Total Item** counts distinct stocked products, not every item in the product
  master.
- Blank category or supplier names are displayed as **Unknown**.
- The category and supplier charts show the same Top 10 population as their
  corresponding ranking tables.
- A high inventory value is not automatically good or bad. It must be read
  together with movement, aging, stock-out, and inventory-risk information.

---

## 1. KPI Summary

The KPI row gives management the size and breadth of the current inventory
position before reviewing concentration.

### 1.1 Total Inventory Value

1. **KPI Canonical:** `3.1.1.1` — Total Inventory Value  
   **Portal catalog:** `IN-KPI-001`  
   **Related executive KPI:** `EX-KPI-010`

2. **Question Answered:**  
   How much company capital is currently tied up in stock?

3. **Definition:**  
   The point-in-time value of stocked items measured at cost:

   ```text
   Total Inventory Value = HPP × Quantity
   ```

   The inventory value is aggregated by item, excludes In-Transit stock, and
   includes only items with positive net quantity.

4. **Business Meaning:**  
   This is the primary inventory working-capital measure. It shows how much
   money is sitting in stock instead of being available as cash. The value
   supports decisions about purchasing, stock reduction, service availability,
   and capital allocation.

5. **How To Interpret:**

   - A **high value** means more capital is committed to stock. This may support
     strong service levels, or it may hide overstock and dead stock.
   - A **low value** may indicate efficient working capital, but it can also
     indicate insufficient stock and future stock-out risk.
   - A **rising value** should be checked against sales movement and inventory
     health. Rising value without corresponding demand can indicate capital
     buildup.
   - A **falling value** is positive when it comes from healthy sales or stock
     reduction, but concerning when Stock-Out Risk Items are increasing.
   - Check the Inventory Risk and Inventory Forecast dashboards before approving
     additional purchases.

### 1.2 Total Item

1. **KPI Canonical:** `3.1.1.2` — Total Item  
   **Portal catalog:** `IN-KPI-002`

2. **Question Answered:**  
   How many distinct products currently have stock on hand?

3. **Definition:**  
   The number of distinct products with positive net quantity after the same
   exclusions used for Total Inventory Value.

4. **Business Meaning:**  
   This measures the breadth of the live stocked assortment. It helps
   management distinguish a large inventory value spread across many products
   from a large value concentrated in only a few products.

5. **How To Interpret:**

   - A **high count** means many products are stocked, but it does not prove
     that the assortment is healthy or productive.
   - A high count together with high Dead Stock or Never Sold exposure may mean
     the breadth consists of leftover products rather than useful assortment.
   - A **low count** may reflect deliberate assortment focus, but it may also
     indicate empty shelves or missing active products.
   - A **falling count** should be checked against sales demand and Stock-Out
     Risk Items.
   - Read this KPI together with Total Inventory Value: high value with few
     items indicates capital concentration, while low value with many items may
     indicate many small-value leftovers.

### Reading the KPI Row Together

1. Start with **Total Inventory Value** to understand the financial size of the
   stock position.
2. Use **Total Item** to understand how broadly that value is distributed.
3. Review the category and supplier sections to identify where the capital is
   concentrated.
4. Check Inventory Risk before treating a high value or high item count as a
   positive result.

---

## 2. Inventory by Category

**Widget type:** Horizontal bar chart  
**KPI Canonical:** No separate chart KPI. The chart is the visual composition
of `IN-KPI-003 — Top 10 Category (Ranking)`.

### Business Question

Which product categories hold the largest share of inventory value?

### Definition

Categories are ranked by inventory value in descending order. The chart
displays the Top 10 categories. A blank category is grouped under **Unknown**.

```text
Category Inventory Value = Sum of inventory value for items in the category
```

### Business Meaning

The chart shows where warehouse capital is concentrated by product group. It
helps management focus purchasing reviews, category-level stock policies, and
aging investigations on the categories where an inventory problem would have
the greatest financial impact.

### How To Interpret

- A **large bar** means substantial capital is committed to that category.
- A category with a large bar is not automatically overstocked; compare it with
  sales movement and Inventory Risk.
- A category growing in value while demand is weak may indicate overstock.
- A category with low value but important sales demand may still create service
  risk if its stock is insufficient.
- Compare the chart with **Top 10 Categories** to identify the category names
  behind the concentration.

---

## 3. Inventory by Supplier

**Widget type:** Horizontal bar chart  
**KPI Canonical:** No separate chart KPI. The chart is the visual composition
of `IN-KPI-004 — Top 10 Supplier (Ranking)`.

### Business Question

Which suppliers or Principals account for the largest value of inventory
currently held?

### Definition

Suppliers are ranked by the inventory value attributed to their stocked items
in descending order. The chart displays the Top 10 suppliers. A blank supplier
is grouped under **Unknown**.

```text
Supplier Inventory Value = Sum of inventory value for items supplied
```

### Business Meaning

The chart shows dependence on suppliers for current inventory capital. It
supports supplier discussions, purchasing prioritization, range review, and
assessment of whether a supplier's stock is productive or becoming a working
capital burden.

### How To Interpret

- A **large bar** means a significant amount of company capital is tied to
  that supplier's products.
- High supplier concentration can be useful when the relationship is
  strategic, but it increases exposure to demand changes, supply disruption,
  commercial terms, and slow-moving stock.
- A supplier with high inventory value and high inventory risk requires review
  before additional purchasing.
- A low supplier value does not mean the supplier is unimportant; it may
  provide critical products with high movement and low stock value.
- Compare the chart with the Purchasing dashboard and Inventory Risk dashboard.

---

## 4. Top 10 Categories

1. **KPI Canonical:** `3.1.2.2` — Top 10 Category Ranking  
   **Portal catalog:** `IN-KPI-003` — Top 10 Category (Ranking)

2. **Table Purpose:**  
   Identify the ten categories holding the greatest inventory value.

3. **Visible Content:**  
   Each row shows the category and its inventory value. The table is ordered
   from the largest value to the smallest and contains up to ten categories.
   Blank category names appear as **Unknown**.

4. **Business Meaning:**  
   The table converts the category chart into a named management priority list.
   It shows where purchasing and inventory owners should start when reviewing
   capital concentration, aging, and replenishment decisions.

5. **How To Use The Table:**

   - Start with the largest category values.
   - Check whether the largest categories also have slow-moving, dead-stock,
     or never-sold exposure.
   - Compare category value with sales demand before reducing or increasing
     stock.
   - Use the Inventory Report to investigate the underlying item and warehouse
     rows.

6. **Warning Signs:**

   - A category remains highly valued while its sales movement weakens.
   - A top category also leads Category Risk Exposure on Inventory Risk.
   - Several top categories grow together without a corresponding sales or
     service requirement.

The table is a concentration ranking. It is not an automatic overstock alert.

---

## 5. Top 10 Suppliers

1. **KPI Canonical:** `4.2.2.1` — Top 10 Supplier Ranking  
   **Portal catalog:** `IN-KPI-004` — Top 10 Supplier (Ranking)

2. **Table Purpose:**  
   Identify the ten suppliers or Principals with the greatest inventory value
   currently held.

3. **Visible Content:**  
   Each row shows the supplier or Principal and its inventory value. The table
   is ordered from the largest value to the smallest and contains up to ten
   suppliers. Blank supplier names appear as **Unknown**.

4. **Business Meaning:**  
   The table identifies supplier concentration in inventory capital. It helps
   Purchasing and Inventory management decide which supplier relationships,
   product ranges, and purchasing commitments deserve closer review.

5. **How To Use The Table:**

   - Start with suppliers holding the largest inventory value.
   - Compare the supplier ranking with purchasing volume and current
     replenishment needs.
   - Check whether high supplier inventory is supported by healthy movement or
     is tied to slow-moving and dead stock.
   - Review the Inventory Report for item and warehouse evidence.
   - Use Purchasing Management to assess supplier dependency and purchasing
     activity before placing new orders.

6. **Warning Signs:**

   - A supplier has high inventory value and high inventory-risk exposure.
   - Inventory value is concentrated in a supplier without a deliberate
     strategic reason.
   - Stock from a supplier remains high while demand or purchasing need is
     declining.

The table shows inventory exposure by supplier. It does not by itself measure
supplier quality, delivery performance, or purchasing dependency.

---

## Recommended Management Reading Order

1. Confirm the snapshot freshness and point-in-time scope.
2. Read **Total Inventory Value** to understand capital tied in stock.
3. Read **Total Item** to understand the breadth of stocked products.
4. Review **Inventory by Category** and **Top 10 Categories** for category
   concentration.
5. Review **Inventory by Supplier** and **Top 10 Suppliers** for supplier
   concentration.
6. Cross-check concentrated categories and suppliers in Inventory Risk for
   aging and at-risk exposure.
7. Use Inventory Forecast to check stock-out and future coverage risk.
8. Use Inventory Report to validate the item-by-warehouse evidence before
   changing purchasing or stock policy.

## Related Dashboards

- **IN02 — Inventory Risk:** dead stock, slow-moving, never-sold, and at-risk
  inventory.
- **IN03 — Inventory Forecast:** projected inventory value, days of supply, and
  stock-out or overstock risk.
- **IN04 — Inventory Optimization:** recommended purchase, delay, transfer,
  and clearance actions.
- **IN05 — Inventory Report:** item-by-warehouse stock evidence and summary
  totals.
- **PU01 — Purchasing:** supplier purchasing activity, dependency, and posting
  context.

## Principal-Centric Addendum (PCM-058, implemented surfaces only)

IN01 remains an inventory measure. Supplier rows expose the Principal
identity and a navigation action to SA04 Principal Performance
(`/dashboard/principal-performance`) for the same Principal (PCM-051). No
inventory metric is copied into `PRN-SALES-001`. `PRN-INV-001` and
`PRN-INV-002` publication is owned by its own slice and is not described
here.
