# IN03 — Inventory Optimization Dashboard KPI Explanation

## Scope

**IN03 — Inventory Optimization** answers:

> Which inventory decision should management make now: purchase, delay,
> transfer, post pending purchases, or recover capital from dead stock?

**Route:** `/dashboard/inventory-optimization`

This dashboard is intended for the Owner, Director, General Manager,
Inventory, and Purchasing managers. It converts Inventory Forecast, Inventory
Risk, and Purchasing context into prioritized decision-support actions.

## Important Reading Rules

- The dashboard provides recommendations; it does not create purchase orders
  or warehouse transfers.
- Confirm current stock, pending postings, In-Transit goods, supplier lead
  time, minimum order quantities, and supplier terms before acting.
- Inventory value and purchase impact are measured using inventory cost
  (HPP), not selling price.
- The default planning horizon is 30 days unless the business configuration
  changes it.
- Days of Supply (DOS) estimates how long current quantity can support demand
  at the recent consumption pace.
- Critical, High, Medium, and Low are action-priority categories. They are
  prioritization signals, not accounting classifications.
- A high Inventory Health Score does not mean every item is healthy. Always
  review the named actions and their reasons.

---

## 1. Executive Summary

The Executive Summary is a short management briefing for the current business
date. It normally identifies:

- Critical products that may require purchase review.
- Products for which purchasing should be delayed.
- Warehouse pairs where inventory should be transferred.
- Pending purchases that should be posted before creating new orders.
- Lower-priority purchases that may be deferred.
- Dead stock items whose capital should be reviewed for recovery.
- The highest-priority action currently identified.

### Business Meaning

The summary gives management a starting point. It answers “what should we
look at first?” before management reviews the KPI cards and action tables.

### How To Interpret

Treat the summary as an orientation signal, not as approval for action. Open
the related table and confirm the item, reason, value, and supporting
dashboard evidence before deciding.

### Planning Horizon and Budget Cap

The dashboard also displays the planning horizon and, when configured, the
purchase budget cap.

- **Planning Horizon:** the number of future days used to identify upcoming
  stock pressure and purchase decisions.
- **Budget Cap:** the maximum purchase-spend limit used to separate higher
  priority purchases from lower-priority purchases that may be deferred.

---

## 2. Health and Budget KPIs

This KPI row summarizes the overall inventory condition, the number of urgent
actions, and the financial size of purchase and delay decisions.

### 2.1 Inventory Health Score

1. **KPI Canonical:** `IN-KPI-015` — Inventory Health Score
   (cross-dashboard reference from Inventory Forecast)

2. **Question Answered:**  
   Is the forward inventory position broadly healthy, or does it require
   management attention?

3. **Definition:**  
   A composite score from 0 to 100 that summarizes competing inventory
   pressures, including stock-out exposure, excessive coverage, and existing
   inventory risk.

4. **Business Meaning:**  
   The score gives management a quick view of whether the inventory position
   is balanced enough to support sales without tying up unnecessary capital.

5. **How To Interpret:**

   - A high score indicates a more balanced overall position.
   - A medium score indicates that one or more conditions need review.
   - A low score indicates material inventory pressure.
   - Use the score to set review priority, then use the action tables to find
     the specific cause and decision.

### 2.2 Critical Actions

1. **KPI Canonical:** Not Found — supporting optimization measure.

2. **Question Answered:**  
   How many of the current recommended actions require the most immediate
   management attention?

3. **Definition:**  
   The number of prioritized optimization actions classified as **Critical**.
   A Critical category can result from urgent stock coverage, high-value
   exposure, or an action that prevents a significant purchase or inventory
   risk.

4. **Business Meaning:**  
   This is the size of the urgent decision queue. It helps management estimate
   whether the issue can be handled through routine review or requires
   immediate coordination between Purchasing, Inventory, and Sales.

5. **How To Interpret:**

   - A value greater than zero requires review of the Top Optimization Actions.
   - A high value means urgent work is broad or several material exposures
     exist.
   - A low value does not mean there are no Medium or High actions.
   - Start with the highest priority score and the largest impact.

### 2.3 Recommended Purchase Budget

1. **KPI Canonical:** Not Found — supporting optimization measure.

2. **Question Answered:**  
   How much purchase spending is recommended across the current action
   priorities?

3. **Definition:**  
   The estimated HPP cost of purchase recommendations included in the current
   optimization result, including the purchase priorities that are not
   classified as Critical or High.

4. **Business Meaning:**  
   This KPI shows the potential cash commitment required to protect product
   availability. It helps management compare replenishment needs with the
   available purchasing budget.

5. **How To Interpret:**

   - A high value means replenishment may require significant working capital.
   - Compare it with Deferrable Spend and the configured Budget Cap.
   - Do not approve the amount automatically; verify lead time, pending
     purchases, In-Transit stock, and actual demand.
   - A high recommendation together with high Overstock or At-Risk Inventory
     requires careful item-level review.

### 2.4 Deferrable Spend

1. **KPI Canonical:** Not Found — supporting optimization measure.

2. **Question Answered:**  
   How much planned or potential purchasing spend may be postponed?

3. **Definition:**  
   The estimated purchase-related value associated with lower-priority
   purchases or delay recommendations.

4. **Business Meaning:**  
   Deferrable Spend identifies a possible working-capital release. It helps
   management avoid purchasing stock that has adequate coverage, weak demand,
   or lower priority than more urgent needs.

5. **How To Interpret:**

   - A high value indicates an opportunity to preserve cash or reduce excess
     stock.
   - Deferral is not the same as cancellation. Recheck demand, seasonality,
     promotions, and supplier lead time.
   - If DOS is high or movement is Slow Moving, delaying replenishment is
     generally more defensible.

---

## 3. Action Mix KPIs

This KPI row shows how the optimization result is distributed across the main
decision types.

### 3.1 Purchase Now

1. **KPI Canonical:** Not Found — supporting optimization measure.

2. **Question Answered:**  
   How many items may need replenishment review now to protect availability?

3. **Definition:**  
   The count of purchase recommendations for eligible active items whose
   coverage or reorder timing indicates a near-term need.

4. **Business Meaning:**  
   This is the potential service-level risk queue. It helps Purchasing focus
   on items that may run out before normal replenishment can arrive.

5. **How To Interpret:**

   - Review the earliest reorder date and lowest DOS first.
   - Give additional attention to high-value or strategically important items.
   - Validate pending purchases and In-Transit stock before placing a new
     order.

### 3.2 Delay

1. **KPI Canonical:** Not Found — supporting optimization measure.

2. **Question Answered:**  
   How many items should not be replenished immediately?

3. **Definition:**  
   The count of recommendations to hold or reduce purchasing because current
   coverage is high, movement is weak, or demand does not justify immediate
   replenishment.

4. **Business Meaning:**  
   Delay recommendations protect cash and reduce the chance that active stock
   becomes Slow Moving or Dead Stock.

5. **How To Interpret:**

   - Review the item DOS, movement class, supplier, and reason text.
   - A delay recommendation is stronger when inventory already has long
     coverage or the same supplier has multiple overstock items.
   - Do not delay critical items solely because the average company DOS is
     high.

### 3.3 Transfer

1. **KPI Canonical:** Not Found — supporting optimization measure.

2. **Question Answered:**  
   How many warehouse-to-warehouse movements may solve a shortage without a
   new purchase?

3. **Definition:**  
   The count of warehouse pairs where inventory can be moved from a location
   with excess coverage to a location with insufficient coverage.

4. **Business Meaning:**  
   Transfer recommendations use stock that the company already owns. They can
   reduce emergency purchases, improve availability, and rebalance warehouse
   inventory.

5. **How To Interpret:**

   - Review the source warehouse, destination warehouse, transfer quantity,
     and destination DOS.
   - Confirm physical availability, warehouse capacity, logistics cost, and
     operational timing.
   - A transfer is most valuable when it avoids a Critical purchase.

### 3.4 Clearance Review

1. **KPI Canonical:** Not Found — supporting optimization measure.

2. **Question Answered:**  
   How many dead-stock items require a capital-recovery decision?

3. **Definition:**  
   The count of Dead Stock items selected for clearance, return, transfer,
   write-off review, or another recovery action.

4. **Business Meaning:**  
   Clearance Review identifies capital that has stopped contributing to sales.
   It helps management reduce storage burden and recover working capital.

5. **How To Interpret:**

   - Start with the highest inventory value and longest idle period.
   - Consider customer demand, return terms, sell-through opportunities, and
     item condition.
   - Do not treat every Dead Stock item as an automatic clearance decision;
     confirm the appropriate recovery route.

---

## 4. Priority Score Distribution

**Widget type:** Horizontal bar chart

**KPI Canonical:** Not Found — supporting priority distribution.

### Business Question

How many optimization actions fall into each management-priority category?

### Visible Content

- Critical
- High
- Medium
- Low
- Action count for each category

### Business Meaning

The chart shows whether the current decision workload is concentrated in
urgent actions or spread across routine improvement opportunities.

### How To Interpret

- A large Critical or High bar indicates immediate management workload.
- A large Medium or Low bar indicates broader planning work that may be
  scheduled after urgent risks are handled.
- Compare the chart with the Top Optimization Actions table. A count alone
  does not show which item has the greatest financial or service impact.

---

## 5. Business Impact Summary

**Widget type:** Bar chart

**KPI Canonical:** No separate canonical KPI was found. The chart visualizes
supporting optimization measures.

### Visible Content

- **Purchase:** estimated purchase impact.
- **Deferrable:** estimated spend that may be delayed.
- **Recoverable:** inventory capital associated with recovery review.

### Business Question

What is the financial scale of the main optimization opportunities?

### Business Meaning

The chart connects operational recommendations with working-capital
consequences. It helps management compare the cash required for availability
with the cash that may be protected or recovered.

### How To Interpret

- A large Purchase bar indicates a material funding requirement for
  replenishment.
- A large Deferrable bar indicates an opportunity to avoid or postpone cash
  outflow.
- A large Recoverable bar indicates substantial capital tied up in stock that
  may need clearance or another recovery decision.
- These amounts are not automatically additive savings or profit. Confirm the
  underlying items and business action before measuring the result.

### Supporting Measures

#### Purchase Impact

**KPI Canonical:** Not Found — supporting optimization measure.

Estimated cost of the items in the recommended reorder list. It indicates the
potential purchase commitment, not an approved purchase order.

#### Deferrable Spend

**KPI Canonical:** Not Found — supporting optimization measure.

Estimated purchasing value that may be postponed because the item has enough
coverage or lower urgency.

#### Recoverable Capital

**KPI Canonical:** Not Found — supporting optimization measure.

Estimated inventory value represented by dead-stock and selected
slow-moving-stock recovery opportunities. It is a potential capital-recovery
opportunity, not guaranteed cash recovery.

---

## 6. Action Heat Summary

**Widget type:** Management table/grid

**KPI Canonical:** Not Found — supporting action-priority distribution.

### Business Question

Which action types are creating the greatest concentration of Critical, High,
Medium, or Low work?

### Visible Content

Rows may include:

- Purchase Product
- Delay Purchase
- Transfer Inventory
- Post Purchase First
- Clearance Review

Columns:

- Critical
- High
- Medium
- Low

Each cell contains the number of actions for that action type and priority
category.

### Business Meaning

The grid helps management understand the character of the workload. For
example, Critical Purchase actions require availability and funding review,
while Critical Clearance actions require a capital-recovery decision.

### How To Interpret

- Start with cells containing Critical actions.
- A high Purchase concentration indicates replenishment pressure.
- A high Delay concentration indicates excess coverage or weak demand.
- A high Transfer concentration indicates warehouse imbalance may be solved
  internally.
- A high Post Purchase First concentration indicates that purchasing records
  or pending goods should be resolved before new orders.
- A high Clearance concentration indicates trapped capital requiring a
  recovery plan.

---

## 7. Top Optimization Actions

**Widget type:** Prioritized action table

**KPI Canonical:** No separate canonical KPI. The table supports the
Inventory Health Score, action counts, budget measures, and action-specific
recommendations.

### Business Question

Which named actions should management review first?

### Visible Content

- Priority score
- Priority category
- Action
- Item or warehouse pair
- Reason
- Impact
- Drill-down link

### Business Meaning

This is the main decision queue. It converts portfolio-level signals into
named items and practical next steps.

### How To Use The Table

1. Start with Critical and High actions.
2. Review the reason before focusing on the amount.
3. Check whether the action is a purchase, delay, transfer, posting, campaign,
   bundle, or clearance decision.
4. Follow the drill-down link to review the supporting forecast, risk, or
   purchasing context.
5. Confirm the final decision in the relevant operational process.

### Warning Signs

- The same supplier appears repeatedly in high-priority actions.
- A purchase action appears while the same item or supplier has overstock.
- A transfer action could avoid an urgent purchase but has not been executed.
- Dead-stock actions remain open while new purchases continue.

---

## 8. Recommended Reorder List

**Widget type:** Purchase decision-support table

### Business Question

Which items may need to be reordered, in what quantity, and by when?

### Visible Content

- Item
- Supplier
- Recommended quantity
- Estimated cost
- Days of Supply
- Reorder Date
- Priority category

### KPI Explanations

#### Recommended Purchase Quantity

**KPI Canonical:** `IN-KPI-021` — Recommended Purchase Qty (Indicative)

An indicative quantity based on recent consumption, expected coverage needs,
and current quantity. It is a starting point for purchasing review, not an
approved purchase order.

#### Estimated Cost

**KPI Canonical:** Not Found — supporting purchase measure.

The estimated HPP cost of the recommended quantity. Use it to compare the
purchase decision with the available budget and the value of alternative
actions such as transfer.

#### Days of Supply

**KPI Canonical:** `IN-KPI-020` — Days of Supply (Item)

The estimated number of days current quantity can support demand at the recent
consumption pace. Lower DOS generally increases replenishment urgency.

#### Reorder Date

**KPI Canonical:** Not Found — supporting forecast measure.

The estimated date by which purchasing review should occur to avoid
depletion under the current demand and lead-time assumptions.

### How To Interpret

- Review the earliest Reorder Date and lowest DOS first.
- Compare the recommended quantity with pending purchase and In-Transit
  information.
- Check supplier lead time, minimum order quantity, promotions, and
  seasonality.
- Treat the estimate as decision support, not an automatic order.

---

## 9. Warehouse Rebalancing

**Widget type:** Warehouse transfer decision-support table

### Business Question

Can an internal warehouse transfer solve a destination shortage without a new
purchase?

### Visible Content

- Item
- Source warehouse
- Destination warehouse
- Transfer quantity
- Destination Days of Supply
- Priority category

### KPI Explanations

#### Transfer Quantity

**KPI Canonical:** Not Found — supporting optimization measure.

The quantity suggested for movement from a warehouse with excess coverage to a
warehouse with insufficient coverage.

#### Destination Days of Supply

**KPI Canonical:** `IN-KPI-020` — Days of Supply (Item)
  (destination context)

The estimated coverage at the destination after considering its current
consumption and stock position. A low value indicates stronger service
pressure.

### How To Interpret

- Confirm that the source warehouse can release the quantity without creating
  a shortage there.
- Check physical stock, warehouse capacity, delivery timing, and transfer
  cost.
- Prioritize transfers that prevent a Critical purchase or protect important
  customer demand.

---

## 10. Overstock and Delay Purchasing

**Widget type:** Purchase-delay decision-support table

### Business Question

Which purchases should be held, reduced, or reviewed because current coverage
is already sufficient or demand is weak?

### Visible Content

- Item
- Supplier
- Days of Supply
- Movement class
- Action
- Reason

### KPI Explanations

#### Movement Class

**KPI Canonical:** `IN-KPI-009` — Aging Distribution (Movement Classes)
  (cross-dashboard reference)

The item’s recent movement condition, such as Active, Slow Moving, Dead Stock,
or Never Sold. It provides the demand context behind the delay recommendation.

#### Suggested Quantity

**KPI Canonical:** Not Found — supporting optimization measure.

When present, the quantity represents a reduced replenishment amount to
consider when a full purchase would increase overstock exposure.

### Business Meaning

This table protects the business from buying into excess coverage. It also
highlights cases where a supplier already has multiple overstock items or
where a pending purchase should be resolved first.

### How To Interpret

- High DOS combined with Slow Moving movement is a strong delay signal.
- A “Reduce Purchase Quantity” action should be compared with current demand
  and supplier terms.
- A “Post Purchase First” action means management should complete or verify
  the existing purchase posting before creating another order.
- Do not delay an item solely because its supplier has other overstock
  products; review the item’s own demand and coverage.

---

## 11. Dead Stock Recovery

**Widget type:** Capital-recovery decision-support table

### Business Question

Which Dead Stock items contain the greatest capital exposure and what recovery
action should be reviewed?

### Visible Content

- Item
- Inventory value
- Idle days
- Recommended action
- Priority category

### KPI Explanations

#### Inventory Value

**KPI Canonical:** `IN-KPI-001` — Total Inventory Value
  (item-level context)

The HPP value currently tied up in the item. It measures the capital exposure
that may be reduced through clearance, return, transfer, or another approved
action.

#### Idle Days

**KPI Canonical:** Not Found — supporting movement measure.

The number of days since the item last appeared in a Faktur sale. Longer idle
time generally indicates a lower likelihood of recovery through normal sales.

#### Recommended Action

**KPI Canonical:** Not Found — supporting optimization recommendation.

The proposed decision path, such as clearance or return review. It identifies
what management should investigate; it is not an automatic disposition.

### How To Interpret

- Begin with high-value items and long idle periods.
- Check whether a return agreement, transfer opportunity, promotion, bundle,
  or clearance channel exists.
- Stop new purchasing for items classified as Dead Stock or Never Sold unless
  management has a documented exception.
- Record the final operational and financial decision outside the dashboard
  process.

---

## 12. Traceability and Related Dashboards

The footer links the optimization recommendations to their supporting views:

- **Inventory Forecast** — projected demand, DOS, stock-out timing, and
  purchase quantity context.
- **Inventory Risk** — Slow Moving, Dead Stock, Never Sold, and at-risk
  inventory context.
- **Purchasing Management** — supplier purchasing activity, pending posting,
  and supplier context.
- **Inventory Report** — item and warehouse stock evidence.
- **Purchasing Report** — purchasing transaction evidence.

### Business Purpose

Optimization recommendations should be traceable to inventory evidence,
demand context, and purchasing information. Management should use these links
to validate the recommendation before changing purchasing, warehouse, or
clearance decisions.

---

## Recommended Management Reading Order

1. Confirm the business date, planning horizon, budget cap, and data freshness.
2. Read the Executive Summary.
3. Review Inventory Health Score and Critical Actions.
4. Compare Recommended Purchase Budget with Deferrable Spend.
5. Review the Action Mix KPIs and Priority Score Distribution.
6. Use Business Impact Summary to understand the working-capital scale.
7. Start with Critical cells in the Action Heat Summary.
8. Open Top Optimization Actions and review the highest-priority named items.
9. Validate purchase decisions in Recommended Reorder List.
10. Check Warehouse Rebalancing before approving new purchases.
11. Review Overstock and Delay Purchasing before adding stock.
12. Review Dead Stock Recovery for the largest idle capital exposure.
13. Cross-check Inventory Forecast, Inventory Risk, Purchasing Management, and
    the related reports before taking action.

## Related Dashboards

- **IN01 — Inventory:** current inventory value, item breadth, and
  concentration.
- **IN02 — Inventory Risk:** Dead Stock, Slow Moving, Never Sold, and
  At-Risk Inventory.
- **IN03 — Inventory Forecast:** future coverage, stock-out, overstock, and
  purchase quantity context.
- **IN05 — Inventory Report:** item-by-warehouse stock evidence.
- **PU01 — Purchasing:** supplier purchasing activity and posting context.
