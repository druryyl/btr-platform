# IN03 — Inventory Forecast Dashboard KPI Explanation

## Scope

**IN03 — Inventory Forecast** answers:

> Which active items may run out of stock within the next 30 days, what
> inventory value is expected at the end of the horizon, and when should
> Purchasing review replenishment?

**Route:** `/dashboard/inventory-forecast`

This is a forward-looking planning dashboard for the Owner, Director, General
Manager, Inventory, and Purchasing managers. It uses the current stock position
and gross Faktur sales quantity from the last 30 days to estimate depletion.
The forecast is deterministic and explainable; it is not an AI or machine
learning prediction.

## Important Reading Rules

- The default planning horizon is **30 calendar days**.
- Inventory value is measured at **HPP × quantity**, not selling price.
- In-Transit stock is excluded.
- Forecast-eligible items are active items that are not classified as Dead
  Stock or Never Sold.
- Consumption uses gross Faktur sales quantity; Retur is not netted from the
  consumption rate.
- **Average Daily Consumption (ADC)** is the quantity sold during the latest
  30-day period divided by 30.
- **Days of Supply (DOS)** is current quantity divided by ADC when ADC is
  greater than zero.
- Recommended quantities are decision support only. They are not approved
  purchase orders.
- A forecast is an estimate based on recent consumption. Confirm supplier lead
  time, pending postings, and In-Transit stock before purchasing.

## Forecast Basis

The dashboard uses the following business logic:

```text
ADC = Faktur quantity sold in the last 30 days ÷ 30

DOS = Current quantity ÷ ADC

Forecast quantity at horizon =
  MAX(0, Current quantity − ADC × Horizon days)
```

The default assumptions for the purchasing hint are:

- Lead time: 7 days
- Additional coverage: 14 days
- Overstock threshold: more than 90 DOS

---

## 1. Executive Summary

The Executive Summary is a narrative management interpretation of the current
forecast. It normally brings together the projected inventory position,
stock-out exposure, and the most important risks.

### Business Purpose

It gives management a quick answer before reviewing individual KPI cards and
tables:

- Is inventory generally expected to remain stable?
- Is stock-out risk material?
- Is inventory value declining because of healthy sales or because stock is
  becoming insufficient?
- Which issue deserves immediate attention?

### How To Interpret

Use the summary as an orientation signal, not as a replacement for the
underlying KPIs. When the summary highlights stock-out risk, review the Top
Inventory Risks table. When it highlights declining value, compare the
projection with the Consumption Trend and the Risk Exposure KPIs.

---

## 2. Position vs Projection

This KPI row compares the inventory position today with the projected position
at the end of the planning horizon.

### 2.1 Current Inventory Value

1. **KPI Canonical:** `IN-KPI-026` — Report Footer — Total Inventory Value
   (cross-dashboard reference)

2. **Question Answered:**  
   How much capital is currently tied up in inventory?

3. **Definition:**  
   The current value of eligible inventory, measured using HPP multiplied by
   current quantity. In-Transit stock is excluded.

4. **Business Meaning:**  
   This is the starting point for the forecast. It represents the capital
   position that may be converted into sales, remain in stock, or be exposed
   to stock and demand risk.

5. **How To Interpret:**

   - A high value means more working capital is committed to inventory.
   - A falling value may be healthy when caused by strong sales, but concerning
     when Stock-Out Risk Items are increasing.
   - Compare this value with Projected Inventory Value @ H and Overstock Value
     before reducing or increasing purchases.

### 2.2 Projected Inventory Value @ H

1. **KPI Canonical:** `IN-KPI-013` — Projected Inventory Value @ Horizon

2. **Question Answered:**  
   How much inventory value is expected to remain at the end of the
   30-day planning horizon?

3. **Definition:**  
   The estimated HPP value of remaining forecast-eligible inventory after
   applying the current 30-day ADC through the horizon.

   ```text
   Projected Inventory Value @ H =
   SUM(MAX(0, Current Quantity − ADC × H) × HPP)
   ```

4. **Business Meaning:**  
   This supports working-capital planning and shows the likely value of stock
   still held after expected sales consumption.

5. **How To Interpret:**

   - A large reduction from Current Inventory Value may indicate strong
     sell-through or possible future availability pressure.
   - A small reduction may indicate low movement, excess stock, or insufficient
     demand.
   - Read it together with Stock-Out Risk Items and Overstock Value; the same
     portfolio can contain both shortages and excesses.

### 2.3 Avg Days of Supply

1. **KPI Canonical:** `IN-KPI-014` — Average Days of Supply (Company)

2. **Question Answered:**  
   At the current consumption pace, how many days can the eligible inventory
   support sales?

3. **Definition:**  
   A company-level coverage measure based on eligible quantity and consumption
   pace.

   ```text
   Average DOS = Total eligible quantity ÷ Total ADC
   ```

   Items with no positive ADC do not provide a meaningful depletion estimate.

4. **Business Meaning:**  
   This is a broad replenishment signal. It helps management understand
   whether inventory coverage is generally short, balanced, or long.

5. **How To Interpret:**

   - Low DOS means the company may need to review replenishment or service
     availability.
   - High DOS may indicate comfortable coverage, but can also hide slow-moving
     or excessive stock.
   - Never rely on the average alone. A healthy average can hide a small group
     of high-value items that will stock out soon.
   - Always compare it with Stock-Out Risk Items and the Risk Heat Summary.

### 2.4 Inventory Health Score

1. **KPI Canonical:** `IN-KPI-015` — Inventory Health Score

2. **Question Answered:**  
   Is the forward inventory position broadly healthy, or does it require
   management attention?

3. **Definition:**  
   A composite score from 0 to 100. It starts at 100 and applies weighted
   penalties for stock-out exposure, overstock exposure, and inventory risk.

4. **Business Meaning:**  
   The score gives management a compact view of competing inventory problems:
   insufficient stock, excessive coverage, and unhealthy inventory.

5. **How To Interpret:**

   - A high score indicates a more balanced forecast position.
   - A medium score indicates that one or more conditions need review.
   - A low score indicates material forward inventory pressure.
   - The score is a prioritization aid, not a diagnosis. Open the Risk
     Exposure KPIs and Top Inventory Risks table to identify the cause.

---

## 3. Risk Exposure

This KPI row identifies the financial and operational consequences of the
forecast.

### 3.1 Stock-Out Risk Items

1. **KPI Canonical:** `IN-KPI-016` — Stock-Out Risk Items / Value

2. **Question Answered:**  
   How many active items are projected to run out within the planning horizon?

3. **Definition:**  
   The count of forecast-eligible items whose DOS is within the horizon and
   whose ADC is greater than zero.

   ```text
   Stock-Out Risk = DOS ≤ planning horizon and ADC > 0
   ```

4. **Business Meaning:**  
   This is a direct service-availability warning. These items may interrupt
   customer fulfillment, reduce sales opportunities, or require urgent
   purchasing decisions.

5. **How To Interpret:**

   - A value greater than zero requires review, especially when the affected
     items have high value or high sales importance.
   - A rising count means more active items are approaching depletion.
   - A zero count does not prove that inventory is healthy; it does not measure
     overstock or items with no recent sales.
   - Open Top Inventory Risks to review the item-level DOS, stock-out date,
     value, and urgency.

### 3.2 Overstock Value

1. **KPI Canonical:** `IN-KPI-017` — Overstock / Understock Value

2. **Question Answered:**  
   How much inventory value is projected to have more than the target
   coverage?

3. **Definition:**  
   The HPP value of forecast-eligible inventory with DOS above the default
   overstock threshold of 90 days.

4. **Business Meaning:**  
   Overstock ties up capital and increases the risk of slow-moving, dead, or
   obsolete inventory.

5. **How To Interpret:**

   - A high value suggests that purchasing should be cautious and demand
     should be reviewed before adding more stock.
   - A rising value may indicate purchases are exceeding demand.
   - Overstock is not automatically a reason to clear stock; check item
     importance, seasonality, sales plans, and supplier agreements.

### 3.3 Understock Value

1. **KPI Canonical:** `IN-KPI-017` — Overstock / Understock Value

2. **Question Answered:**  
   How much inventory value is exposed to insufficient coverage?

3. **Definition:**  
   The HPP value of items classified as understock according to the forecast
   coverage rules. It complements the Stock-Out Risk Items count with a
   monetary view.

4. **Business Meaning:**  
   The value estimates the financial scale of inventory that may require
   replenishment attention. It helps management prioritize beyond item count.

5. **How To Interpret:**

   - A high value means the potential shortage exposure is financially
     material.
   - A low value can still be operationally important when critical,
     fast-moving, or strategically important items are affected.
   - Compare it with Recommended Purchase Qty and supplier availability before
     approving a purchase.

### 3.4 At-Risk Inventory %

1. **KPI Canonical:** `IN-KPI-008` — At-Risk Inventory %
   (copied from Inventory Risk for traceability)

2. **Question Answered:**  
   What share of inventory capital is already classified as unhealthy?

3. **Definition:**  
   The proportion of inventory value classified as Slow Moving, Dead Stock, or
   Never Sold.

   ```text
   At-Risk Inventory % =
   (Slow Moving Value + Dead Stock Value + Never Sold Value)
   ÷ Total Inventory Value
   ```

4. **Business Meaning:**  
   This provides backward-looking inventory quality context for the
   forward-looking forecast. It prevents a forecast from being interpreted
   without considering existing unhealthy stock.

5. **How To Interpret:**

   - A high percentage means a large share of capital already needs attention.
   - A low percentage is positive for inventory quality, but does not prove
     that active items have sufficient coverage.
   - High At-Risk Inventory % together with high Stock-Out Risk Items indicates
     an imbalance: some stock is not moving while other items are running out.

---

## 4. Scenario Bands

The scenario row shows how the projected inventory value changes when recent
consumption history is viewed under different rates.

### 4.1 Best Case Projected

1. **KPI Canonical:** `IN-KPI-018` — Scenario Projected Value
   (Best/Expected/Worst)

2. **Question Answered:**  
   What inventory value may remain if consumption is at the more favorable
   recent rate?

3. **Definition:**  
   The projected inventory value using the lower consumption rate between the
   30-day and 90-day ADC scenarios.

4. **Business Meaning:**  
   It provides an optimistic planning boundary for working-capital and
   replenishment discussions.

5. **How To Interpret:**

   - A higher Best Case value means more stock may remain if demand is slower.
   - A wide gap between Best Case and Worst Case indicates meaningful demand
     uncertainty.
   - Do not use Best Case alone to delay purchases of critical items.

### 4.2 Expected Projected

1. **KPI Canonical:** `IN-KPI-018` — Scenario Projected Value
   (Best/Expected/Worst)

2. **Question Answered:**  
   What inventory value is expected if the primary recent consumption pace
   continues?

3. **Definition:**  
   The main projected inventory value using the 30-day ADC.

4. **Business Meaning:**  
   This is the central planning estimate used by the dashboard for projected
   value and depletion.

5. **How To Interpret:**

   - Treat it as the baseline scenario for routine planning.
   - Compare it with Current Inventory Value to understand expected depletion.
   - Validate important decisions against the scenario range and item-level
     risks.

### 4.3 Worst Case Projected

1. **KPI Canonical:** `IN-KPI-018` — Scenario Projected Value
   (Best/Expected/Worst)

2. **Question Answered:**  
   What inventory value may remain if consumption is at the more demanding
   recent rate?

3. **Definition:**  
   The projected inventory value using the higher consumption rate between the
   30-day and 90-day ADC scenarios.

4. **Business Meaning:**  
   It provides a conservative boundary for stock-out and service-level
   planning.

5. **How To Interpret:**

   - A lower Worst Case value means faster depletion under stronger demand.
   - Use it when planning for high-demand periods or when stock-out exposure
     would be costly.
   - A large difference between Expected and Worst Case calls for closer
     monitoring and earlier purchasing review.

### 4.4 Forecast Confidence

1. **KPI Canonical:** `IN-KPI-019` — Forecast Confidence (Inventory)

2. **Question Answered:**  
   How much historical consumption evidence supports the forecast?

3. **Definition:**  
   A confidence indicator based on the depth of company consumption history.
   Confidence is Low when fewer than 30 days of consumption data are
   available.

4. **Business Meaning:**  
   Confidence tells management how cautiously to use the projected values.
   It distinguishes a forecast supported by sufficient history from one based
   on limited evidence.

5. **How To Interpret:**

   - Low confidence means the forecast should be used directionally and
     confirmed with operational knowledge.
   - Medium confidence supports active planning with frequent monitoring.
   - High confidence provides a more stable baseline, although it remains an
     estimate.
   - Low confidence combined with high Stock-Out Risk requires manual review
     before delaying or accelerating purchases.

---

## 5. Forecast Inventory Level

**Widget type:** Line chart

**KPI Canonical:** `IN-KPI-013` — Projected Inventory Value @ Horizon

### Business Question

How is inventory value expected to decline across the planning horizon?

### Visible Content

- Day 0 through Day H on the horizontal axis
- Projected inventory value on the vertical axis
- A line representing the projected inventory value for each day

### Business Meaning

The chart turns the end-of-horizon KPI into a time path. It helps management
see whether depletion is gradual or whether the inventory position is likely
to become materially smaller during the horizon.

### How To Interpret

- A steep decline indicates strong expected consumption and possible
  replenishment pressure.
- A shallow decline may indicate low movement, but should be read with
  Overstock Value and At-Risk Inventory %.
- The chart is a company-level view. Use Top Inventory Risks for item-level
  stock-out timing.

---

## 6. Consumption Trend and Risk Heat Summary

### 6.1 Consumption Trend

**Widget type:** Combined bar and line chart

**KPI Canonical:** Supporting visualization for ADC; no separate canonical KPI
was found.

### Business Question

What sales consumption pattern supports the forecast?

### Visible Content

- Bars: daily units sold during the 30-day lookback
- Line: 30-day ADC reference
- Horizontal axis: lookback day
- Vertical axis: units

### Business Meaning

The chart lets management judge whether the average consumption rate is
representative. It exposes spikes, quiet periods, and irregular demand that
may make a straight-line forecast less reliable.

### How To Interpret

- Bars consistently above the ADC line suggest faster depletion than the
  baseline.
- Bars consistently below the line suggest slower depletion.
- Large spikes or gaps indicate demand variability; use the scenario bands and
  Forecast Confidence before making a major purchasing decision.

### 6.2 Risk Heat Summary

**Widget type:** 3×3 management table/grid

**KPI Canonical:** Supporting risk distribution; no separate canonical KPI was
found.

### Visible Content

The grid counts items across two dimensions:

- DOS bands: Low DOS (≤14 days), Medium DOS (15–60 days), High DOS (>60 days)
- Value bands: Low Value, Medium Value, High Value

### Business Meaning

The grid shows whether risk is concentrated in a few financially important
items or spread across many items.

### How To Interpret

- High-value items in the Low DOS row are the strongest replenishment
  priorities.
- Many low-value items in the Low DOS row may indicate a broad operational
  availability issue.
- High-value items in the High DOS row may indicate working capital tied up in
  excessive coverage.
- A balanced grid is not automatically healthy; confirm the underlying item
  and demand context.

---

## 7. Consumption Pace

This KPI row provides supporting measures for understanding the rate and
coverage behind the forecast.

### 7.1 Avg Daily Consumption (units)

1. **KPI Canonical:** Not Found — supporting forecast measure.

2. **Question Answered:**  
   How many units are sold on an average day across the forecast population?

3. **Definition:**  
   The average daily quantity sold during the latest 30-day period.

   ```text
   ADC = Gross Faktur quantity sold in the last 30 days ÷ 30
   ```

4. **Business Meaning:**  
   ADC is the demand pace driving DOS, projected depletion, stock-out timing,
   and recommended purchase quantity.

5. **How To Interpret:**

   - A higher ADC means inventory will be consumed faster.
   - A rising ADC increases replenishment urgency unless stock levels rise
     proportionally.
   - A low ADC may indicate low demand, but can also reflect incomplete or
     unusual sales history.

### 7.2 Forecast Consumption @ H

1. **KPI Canonical:** Not Found — supporting forecast measure.

2. **Question Answered:**  
   How many units are expected to be consumed during the planning horizon?

3. **Definition:**  
   The estimated consumption over H days based on the forecast ADC.

   ```text
   Forecast Consumption @ H = ADC × H
   ```

4. **Business Meaning:**  
   This converts the daily demand rate into a quantity useful for inventory and
   purchasing planning.

5. **How To Interpret:**

   - A high value means the business expects substantial unit movement during
     the horizon.
   - Compare it with current quantity to understand whether current stock can
     support expected demand.
   - It is a quantity estimate, not a revenue or margin forecast.

### 7.3 Inventory Coverage %

1. **KPI Canonical:** Not Found — supporting forecast measure.

2. **Question Answered:**  
   What portion of the expected horizon consumption is covered by current
   inventory?

3. **Definition:**  
   A percentage comparing available inventory coverage with expected
   consumption across the planning horizon.

4. **Business Meaning:**  
   It provides a normalized view of coverage that is easier to compare than
   raw units across different inventory positions.

5. **How To Interpret:**

   - A high percentage generally indicates stronger coverage.
   - A low percentage indicates that current stock may not cover expected
     demand through the horizon.
   - Read it together with DOS and Stock-Out Risk Items because company-level
     percentages can hide item-level shortages.

### 7.4 Turnover Forecast

1. **KPI Canonical:** Not Found — supporting forecast measure.

2. **Question Answered:**  
   How quickly is inventory expected to cycle based on projected consumption?

3. **Definition:**  
   A forward-looking turnover estimate based on forecast consumption relative to
   inventory held.

4. **Business Meaning:**  
   It helps management connect inventory coverage with movement and working
   capital efficiency.

5. **How To Interpret:**

   - A higher forecast turnover generally indicates faster movement.
   - A low turnover may indicate excess coverage or weak demand.
   - A high turnover is not automatically positive if it is accompanied by
     Stock-Out Risk Items.

---

## 8. Top Inventory Risks

**Widget type:** Priority table, maximum 10 items

**KPI Canonical:** Uses `IN-KPI-016` — Stock-Out Risk Items / Value and
`IN-KPI-020` — Days of Supply (Item)

### Business Question

Which individual items require the earliest investigation or replenishment
decision?

### Visible Content

- Item
- Signal
- DOS
- Stock-Out Date
- Value
- Urgency
- Investigate link

### Business Meaning

This table converts the company-level forecast into an action queue. It lets
Inventory and Purchasing begin with named items rather than investigating the
entire assortment.

### How To Use The Table

1. Start with Critical and High urgency items.
2. Review the earliest Stock-Out Date.
3. Prioritize high-value items, but do not ignore low-value items that are
   operationally critical.
4. Check Supplier, demand pattern, pending purchases, and current postings.
5. Investigate the item before approving a purchase or changing its stock
   policy.

### Warning Signs

- Many high-value items have low DOS.
- Several items from the same Supplier are approaching stock-out.
- The Stock-Out Date occurs before normal supplier lead time.
- The table grows while the Consumption Trend remains above the ADC reference.

---

## 9. Purchasing Recommendations

**Widget type:** Decision-support table, maximum 10 items

### 9.1 Recommended Purchase Qty

1. **KPI Canonical:** `IN-KPI-021` — Recommended Purchase Qty (Indicative)

2. **Question Answered:**  
   How many units may need to be purchased to cover lead time and additional
   coverage?

3. **Definition:**  
   An indicative quantity based on ADC, default lead time, coverage days, and
   current quantity.

   ```text
   Recommended Purchase Qty =
   MAX(0, CEILING(ADC × (Lead Time + Coverage Days) − Current Quantity))
   ```

4. **Business Meaning:**  
   It gives Purchasing a starting point for replenishment review before
   stock-out occurs.

5. **How To Interpret:**

   - A high quantity indicates that current stock may not cover expected demand
     through lead time plus the target coverage period.
   - A zero quantity means the formula does not identify an immediate purchase
     need under the current assumptions.
   - The result must be checked against supplier minimum order quantities,
     pending purchase orders, in-transit goods, warehouse policies, and
     expected demand changes.
   - It is not an approved PO and does not replace purchasing judgment.

### 9.2 Recommendation Table

The table shows the operational context needed to assess the recommendation.

**Visible Content:**

- Item
- Supplier
- Reorder Date
- Recommended Quantity
- ADC
- Current Quantity
- Urgency
- View link

### Business Meaning

The table turns the forecast into a practical replenishment review queue. It
connects quantity recommendations with timing, supplier ownership, and current
stock.

### How To Use The Table

1. Review items with the earliest Reorder Date.
2. Compare Recommended Quantity with Current Quantity and ADC.
3. Validate supplier availability and lead time.
4. Check pending postings and In-Transit stock in BTR Desktop.
5. Confirm the purchase decision against Inventory Risk and Purchasing
   Management.

### Warning Signs

- A recommendation is urgent but supplier lead time exceeds the remaining
  coverage.
- Recommended quantities remain high while Overstock Value is also high.
- The same Supplier appears repeatedly among urgent recommendations.
- The recommendation conflicts with known promotions, seasonality, or planned
  assortment changes.

---

## 10. Traceability Footer

The footer links the forecast to the supporting management views:

- **Inventory Report** — validate item and warehouse stock evidence.
- **Inventory Dashboard** — review current inventory value and composition.
- **Inventory Risk** — review Dead Stock, Slow Moving, and Never Sold exposure.
- **Purchasing Dashboard** — review purchasing activity and supplier context.

### Business Purpose

Forecast decisions should be traceable to current stock evidence, historical
inventory risk, and purchasing context. The footer helps management move from
forecast signal to supporting evidence before taking action.

## Recommended Management Reading Order

1. Confirm the business date, planning horizon, and snapshot freshness.
2. Read the Executive Summary.
3. Compare Current Inventory Value with Projected Inventory Value @ H.
4. Review Stock-Out Risk Items, Understock Value, and Overstock Value.
5. Check Forecast Confidence and the Consumption Trend.
6. Use the Risk Heat Summary to locate high-value shortage or excess
   concentrations.
7. Open Top Inventory Risks and begin with the earliest, highest-urgency
   items.
8. Review Purchasing Recommendations as indicative decision support.
9. Cross-check Inventory Risk, Inventory Report, and Purchasing Dashboard
   before approving purchases or changing inventory policy.

## Related Dashboards

- **IN01 — Inventory:** current inventory value, item breadth, and composition.
- **IN02 — Inventory Risk:** Dead Stock, Slow Moving, Never Sold, and
  At-Risk Inventory %.
- **IN04 — Inventory Optimization:** recommended purchase, delay, transfer,
  and clearance actions.
- **IN05 — Inventory Report:** item-by-warehouse stock evidence.
- **PU01 — Purchasing:** supplier purchasing activity and posting context.
