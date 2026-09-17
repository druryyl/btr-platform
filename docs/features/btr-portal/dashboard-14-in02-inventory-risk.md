# IN02 — Inventory Risk Dashboard KPI Explanation

## Scope

**IN02 — Inventory Risk** answers:

> Which stock is not moving, how much capital is at risk, and where should
> management start corrective action?

**Route:** `/dashboard/inventory-risk`

This dashboard is intended for the Owner, Director, General Manager,
Inventory, and Purchasing managers. It is a backward-looking inventory-health
view based on item movement and inventory value.

## Important Reading Rules

- Inventory value is measured at **HPP × quantity**, not selling price.
- In-Transit stock and items with zero net quantity are excluded.
- **Slow Moving** means no sale for **90–179 days**.
- **Dead Stock** means no sale for **180 days or more**.
- **Never Sold** means the item has stock but no Faktur sales history.
- **Active** means the item was sold within the last 89 days.
- Slow Moving, Dead Stock, and Never Sold are mutually exclusive at the item
  level.
- At-Risk Inventory is the combined value of Slow Moving, Dead Stock, and
  Never Sold inventory.
- A high inventory value is not automatically healthy or unhealthy. Always
  read it together with movement and risk classification.

---

## 1. Inventory Attention Cards

The attention cards provide the headline size of inventory risk before
management investigates categories, suppliers, or individual items.

### 1.1 Dead Stock Item Count and Dead Stock Value

1. **KPI Canonical:** `3.2.1.2` — Dead Stock Count & Value  
   **Portal catalog:** `IN-KPI-005` — Dead Stock Count & Value

2. **Question Answered:**  
   How many items have stopped moving, and how much capital is trapped in them?

3. **Definition:**  
   The card shows both the number of stocked items with no sale for at least
   180 days and their total inventory value.

4. **Business Meaning:**  
   Dead stock is capital that has stopped contributing to sales. It may require
   clearance, return, transfer, write-off review, or a permanent stop-buy
   decision.

5. **How To Interpret:**

   - A high count means the problem is broad across the assortment.
   - A high value means the financial impact is large, even if only a few
     items are involved.
   - A rising value indicates that slow-moving stock is becoming more severe or
     that purchasing continues after demand has weakened.
   - Open **Top 10 Dead Stock by Value** to identify the first items to act on.

### 1.2 Slow Moving Item Count and Slow Moving Value

1. **KPI Canonical:** `3.2.1.3` — Slow Moving Count & Value  
   **Portal catalog:** `IN-KPI-006` — Slow Moving Count & Value

2. **Question Answered:**  
   Which items are losing momentum before they become dead stock?

3. **Definition:**  
   The card shows the number and inventory value of items with no sale for
   90–179 days.

4. **Business Meaning:**  
   Slow Moving stock is an early warning. It may still be recoverable through
   sell-through activity, promotion, transfer, or delayed replenishment.

5. **How To Interpret:**

   - A high count suggests weakening movement across many items.
   - A high value identifies a meaningful working-capital exposure.
   - Increasing Slow Moving value is a warning that future Dead Stock is
     forming.
   - Review the largest items before approving additional purchases in the
     same category or from the same Principal.

### 1.3 At-Risk Inventory %

1. **KPI Canonical:** `3.2.2.1` — At-Risk Inventory %  
   **Portal catalog:** `IN-KPI-008` — At-Risk Inventory %

2. **Question Answered:**  
   What share of inventory capital already requires management attention?

3. **Definition:**

   ```text
   At-Risk Inventory % =
     (Slow Moving Value + Dead Stock Value + Never Sold Value)
     ÷ Total Inventory Value
   ```

4. **Business Meaning:**  
   This is the headline inventory-quality ratio. It separates the size of the
   warehouse from the quality of the capital held in it.

5. **How To Interpret:**

   - A high percentage means a large share of warehouse capital is not moving
     normally.
   - A low percentage generally indicates healthier movement, but it does not
     prove that active items have sufficient stock.
   - A rising percentage means inventory quality is deteriorating even if
     Total Inventory Value is stable.
   - Split the percentage into Slow Moving, Dead Stock, and Never Sold to
     determine the appropriate action.

### 1.4 Inventory Risk Indicator

1. **KPI Canonical:** `3.2.2.2` — Inventory Risk Summary  
   **Portal catalog:** `EX-KPI-028` — Inventory Risk Summary (Alert Center)

2. **Question Answered:**  
   Does the current inventory position require management attention?

3. **Definition:**  
   A conditional attention card that highlights the At-Risk Inventory
   percentage when the inventory risk position warrants review.

4. **Business Meaning:**  
   The indicator prevents a material inventory problem from being hidden inside
   a large total inventory figure. It is a signal to move from summary review
   to item-level investigation.

5. **How To Interpret:**

   - When displayed, open the Inventory Attention List and identify the
     underlying items.
   - Treat the indicator as a prioritization signal, not as a replacement for
     the risk percentage or item values.
   - Compare it with Category Risk Exposure and Supplier Risk Exposure to find
     where the problem is concentrated.

### Reading the Attention Cards Together

1. Use **At-Risk Inventory %** to understand the overall quality of
   inventory capital.
2. Use **Dead Stock Value** to measure the most severe existing exposure.
3. Use **Slow Moving Value** to identify risk that may still be recoverable.
4. Use the item counts to determine whether the problem is concentrated or
   widespread.
5. Use the Attention List and rankings to assign action to named items.

---

## 2. Risk Exposure

This section explains how at-risk capital is distributed across movement
classes, categories, and Suppliers or Principals.

### 2.1 Inventory Aging Distribution

1. **KPI Canonical:** `3.2.1.1` — Aging Distribution  
   **Portal catalog:** `IN-KPI-009` — Aging Distribution (Movement Classes)

2. **Question Answered:**  
   How is inventory value split between healthy movement and aging risk?

3. **Definition:**  
   A pie chart showing inventory value in four movement classes:

   - Active — sold within the last 89 days
   - Slow Moving — no sale for 90–179 days
   - Dead Stock — no sale for 180 days or more
   - Never Sold — no Faktur sales history

4. **Business Meaning:**  
   The chart shows whether warehouse capital is working, slowing, or already
   trapped. It gives management a portfolio view before reviewing individual
   SKUs.

5. **How To Interpret:**

   - A large **Active** slice generally indicates healthy movement.
   - Large **Slow Moving**, **Dead Stock**, or **Never Sold** slices indicate
     increasing capital risk.
   - Active shrinking while the risk slices grow is an early deterioration
     signal.
   - Never Sold is not the same as Dead Stock: Dead Stock had a previous sale;
     Never Sold never had one.

### 2.2 Category Risk Exposure

1. **KPI Canonical:** `3.1.3.1` — Category Risk Exposure  
   **Portal catalog:** `IN-KPI-010` — Category Risk Exposure

2. **Question Answered:**  
   Which product categories contain the most at-risk inventory value?

3. **Definition:**  
   A horizontal bar chart of At-Risk Inventory value by category. It includes
   Slow Moving, Dead Stock, and Never Sold value, not the total value of all
   inventory in the category.

4. **Business Meaning:**  
   The chart identifies the product ranges where obsolescence capital is
   concentrated. It helps management focus purchasing, promotion, clearance,
   and assortment decisions.

5. **How To Interpret:**

   - A long bar means a large amount of unhealthy capital is concentrated in
     that category.
   - A category with high total inventory but low risk exposure may be healthy.
   - A smaller category with high risk exposure may be a more urgent problem
     than a larger, healthy category.
   - Compare the category with the Attention List and Top 10 rankings before
     changing its purchasing policy.

### 2.3 Supplier Risk Exposure

1. **KPI Canonical:** `3.2.2.3` — Supplier Risk Exposure  
   **Portal catalog:** `IN-KPI-011` — Supplier Risk Exposure

2. **Question Answered:**  
   Which Suppliers or Principals are associated with the most at-risk stock?

3. **Definition:**  
   A horizontal bar chart of At-Risk Inventory value attributed to each
   Supplier or Principal.

4. **Business Meaning:**  
   The chart reveals when a Supplier relationship is also a source of
   obsolescence risk. A Supplier may be commercially important while still
   having goods that are not moving.

5. **How To Interpret:**

   - A long bar means significant slow-moving, dead, or never-sold capital is
     tied to that Supplier.
   - High Supplier risk together with continued purchasing is a stop-buy or
     purchasing-review signal.
   - High exposure concentrated in one Supplier may indicate both dependency
     and trapped capital.
   - Review the underlying items before concluding that the Supplier itself is
     the cause; demand, assortment, and intake decisions may also be factors.

---

## 3. Inventory Attention List

### Inventory Attention List

1. **KPI Canonical:** No separate canonical KPI. This is the item-level action
   table supporting `IN-KPI-005` through `IN-KPI-011`.

2. **Question Answered:**  
   Which specific inventory items require investigation or action?

3. **Definition:**  
   A filtered table of items classified with an inventory attention signal.
   Users can filter by signal, including Dead Stock, Slow Moving, and Never
   Sold.

4. **Visible Content:**

   - Code
   - Item
   - Category
   - Supplier
   - Quantity
   - Value
   - Days Since Last Faktur
   - Signal

5. **Business Meaning:**  
   The table converts portfolio-level risk into a workable queue. It gives
   Inventory, Purchasing, and Sales the names needed to investigate demand,
   clearance, transfer, return, or replenishment decisions.

6. **How To Use The Table:**

   - Filter by signal to work one risk class at a time.
   - Start with the highest-value items, not only the largest item count.
   - Check the Supplier and Category columns for repeated patterns.
   - Use **Days Since Last Faktur** to distinguish recent slowdown from
     long-standing inactivity.
   - Open the item profile or investigation view before taking action.

7. **Warning Signs:**

   - Many items from one Supplier share the same risk signal.
   - High-value items remain unattended for a long period.
   - Never Sold items continue to be replenished.
   - Slow Moving items are not acted on and begin entering the Dead Stock
     class.

---

## 4. Rankings

The rankings are prioritization tables. They do not show every risky item;
they identify the highest-value items where management should begin.

### 4.1 Top 10 Dead Stock by Value

1. **KPI Canonical:** `3.2.3.1` — Top 10 Dead / Slow Moving  
   **Portal catalog:** `IN-KPI-012` — Top 10 Dead / Slow Moving (Ranking)

2. **Question Answered:**  
   Which dead-stock items represent the largest immediate capital exposure?

3. **Definition:**  
   Up to ten Dead Stock items ordered by inventory value.

4. **Visible Content:**

   - Rank
   - Code
   - Item
   - Value
   - Days Idle
   - % of Dead Stock

5. **Business Meaning:**  
   This is the first practical queue for clearance, return, transfer,
   write-off review, or stop-buy decisions. It focuses management effort on
   the items with the greatest financial impact.

6. **How To Interpret:**

   - A high Value means a large amount of capital is trapped in one item.
   - A high Days Idle means recovery through normal sales is becoming less
     likely.
   - A high percentage means the item represents a material share of total
     Dead Stock exposure.
   - Review Supplier, Category, and purchase history before deciding whether
     the remedy is clearance, return, or a purchasing-policy change.

### 4.2 Top 10 Slow Moving by Value

1. **KPI Canonical:** `3.2.3.1` — Top 10 Dead / Slow Moving  
   **Portal catalog:** `IN-KPI-012` — Top 10 Dead / Slow Moving (Ranking)

2. **Question Answered:**  
   Which slow-moving items should management address before they become dead
   stock?

3. **Definition:**  
   Up to ten Slow Moving items ordered by inventory value.

4. **Visible Content:**

   - Rank
   - Code
   - Item
   - Value
   - Days Idle
   - % of Slow Moving

5. **Business Meaning:**  
   This is an early-action queue. Slow Moving items may still be recovered
   through sell-through activity, promotion, transfer, or delayed purchasing.

6. **How To Interpret:**

   - A high Value identifies a large amount of capital at risk.
   - Increasing Days Idle indicates that the item is moving toward Dead Stock.
   - A high percentage identifies items that dominate the Slow Moving
     exposure.
   - Compare the ranking with current demand and planned purchases before
     replenishing the same items.

---

## Recommended Management Reading Order

1. Confirm the snapshot freshness and point-in-time scope.
2. Read **At-Risk Inventory %** to understand overall inventory quality.
3. Compare **Dead Stock Value** and **Slow Moving Value**.
4. Review **Inventory Aging Distribution** to see whether risk is isolated or
   broad.
5. Use **Category Risk Exposure** and **Supplier Risk Exposure** to find
   concentration.
6. Open the **Inventory Attention List** and filter by signal.
7. Start action with the **Top 10 Dead Stock by Value** and **Top 10 Slow
   Moving by Value** rankings.
8. Cross-check decisions in Inventory Forecast, Inventory Optimization, and
   Inventory Report before approving purchases, transfers, or clearance.

## Related Dashboards

- **IN01 — Inventory:** current inventory value, item breadth, and
  composition.
- **IN03 — Inventory Forecast:** future stock-out, overstock, and coverage
  risk for active items.
- **IN04 — Inventory Optimization:** recommended purchase, delay, transfer,
  and clearance actions.
- **IN05 — Inventory Report:** item-by-warehouse stock evidence.
- **PU01 — Purchasing:** purchasing activity, Supplier dependency, and posting
  context.

## Principal-Centric Addendum (PCM-058, implemented surfaces only)

IN02 remains an inventory measure. Supplier Risk Exposure rows expose the
Principal identity and a navigation action to SA04 Principal Performance
(`/dashboard/principal-performance`) for the same Principal (PCM-051). No
inventory metric is copied into `PRN-SALES-001`. IN03, IN04, and IN05
measures and views are otherwise unchanged by this addendum.
