# CU04 — Customer Portfolio Dashboard KPI Explanation

## Scope

**CU04 — Customer Portfolio** answers:

> What should management do with each customer — grow, retain, protect,
> collect, recover, monitor, or review for exit?

The dashboard is a read-only, deterministic management view. It combines
current customer attention, forward customer risk, receivable exposure, sales
value, lifecycle, portfolio tier, and recommended action.

Customer value in this dashboard means **MTD invoiced omzet as a proxy**. It
does **not** mean profitability, gross margin, or contribution.

## Important Reading Rules

- The default view is **Attention Only**. Use **All Customers** to see the
  complete customer portfolio, including customers who have never purchased.
- **Attention Customers** and **Customers At Risk** are not identical:
  Attention is the portfolio worklist; At Risk is the broader forward-risk
  population.
- **Strategic** is a computed portfolio tier. It is not the same as the
  customer's master-data Klasifikasi.
- Each customer receives one primary portfolio action.
- **Collect** is a link to the Collection Optimization dashboard. CU04 does
  not duplicate the collection queue.
- All monetary values are in IDR.

---

## 1. Executive Summary

The executive summary gives management a short portfolio brief containing:

- Portfolio healthy percentage and health score
- Attention customer count
- Strategic customers at risk
- Customers at elevated forward risk
- Working capital tied in attention customers
- Lifecycle counts for never-purchased, dormant, and declining customers
- The value disclaimer that omzet is a proxy, not profitability

This section is intended for the first management scan. It shows the scale and
urgency of the portfolio before management opens the named customer queues.

---

## 2. Health KPIs

### 2.1 Portfolio Health Score

1. **KPI Canonical:** `CU-KPI-060 — Portfolio Health Score`

2. **Question Answered:**  
   How healthy is the customer portfolio when both customer risk and
   receivable exposure are considered?

3. **Definition:**  
   A score from 0 to 100 inherited from the Customer Risk Forecast view. It
   reflects the financial and customer-count impact of High Risk and Critical
   customers.

4. **Business Meaning:**  
   This is the portfolio's overall risk headline. It helps management decide
   whether attention can remain focused on growth or must shift toward
   collection, credit protection, and customer retention.

5. **How To Interpret:**

   - A high score generally means serious risk affects a smaller share of
     customers and receivables.
   - A low or falling score means risk is spreading or becoming financially
     significant.
   - Always read it with **Portfolio Healthy %**, **Customers At Risk**, and
     **Strategic At Risk**.
   - It is an indicative management score, not an automatic credit stop or
     default probability.

### 2.2 Portfolio Healthy %

1. **KPI Canonical:** `CU-KPI-061 — Portfolio Healthy %`

2. **Question Answered:**  
   What percentage of the customer portfolio remains in a healthy condition?

3. **Definition:**  
   The percentage of all portfolio customers classified as Healthy:

   ```text
   Portfolio Healthy % =
     Healthy Customers ÷ Total Customers × 100%
   ```

4. **Business Meaning:**  
   It shows how much of the customer base can be managed through normal
   commercial activity rather than exception handling.

5. **How To Interpret:**

   - A high percentage indicates a broad, stable customer base.
   - A low or declining percentage means management capacity is increasingly
     consumed by protection, recovery, or collection work.
   - A healthy percentage can still hide important problems if Strategic At
     Risk is rising.
   - Check the Lifecycle Distribution and risk categories for the direction of
     deterioration.

### 2.3 Attention Customers

1. **KPI Canonical:** `CU-KPI-062 — Attention Customer Count`

2. **Question Answered:**  
   How many customers require a portfolio-level review or action?

3. **Definition:**  
   The number of customers qualifying for the default Attention view because
   they have a current attention signal, forward risk above Healthy, a
   non-Monitor portfolio action, or a concerning lifecycle such as Declining,
   Dormant, or Never Purchased.

4. **Business Meaning:**  
   This is the practical size of the management worklist. It converts
   portfolio analysis into the number of customer relationships that need an
   owner and a next step.

5. **How To Interpret:**

   - A high count means management attention is spread across many accounts.
   - A rising count indicates that normal portfolio management is being
     replaced by exception work.
   - Compare it with **Customers At Risk** to separate current portfolio
     actions from the broader forward-risk population.
   - Use the Priority Queue to identify the most urgent names.

### 2.4 Customers At Risk

1. **KPI Canonical:** `EX-KPI-020 — Customers At Risk Count`  
   This canonical KPI is also used on CU04.

2. **Question Answered:**  
   How many customers show forward-risk conditions that may require preventive
   management?

3. **Definition:**  
   The count of customers above the Healthy forward-risk category, including
   Watch, Attention, High Risk, and Critical customers.

4. **Business Meaning:**  
   It measures the breadth of emerging or existing customer risk. Management
   can use it to estimate the Sales, Finance, and Collection capacity needed
   before risk becomes more expensive.

5. **How To Interpret:**

   - A high count means risk is affecting a broad part of the customer base.
   - A rising count while overdue balances are stable can indicate risk is
     forming before it appears in receivable aging.
   - Compare the count with Strategic At Risk and the Lifecycle Distribution.
   - Do not treat it as proof that every customer will default.

---

## 3. Strategic KPIs

### 3.1 Strategic Customers

1. **KPI Canonical:** `CU-KPI-063 — Strategic Customer Count`

2. **Question Answered:**  
   How many customers are important enough to receive the highest level of
   portfolio protection and management attention?

3. **Definition:**  
   The number of customers assigned to the Strategic portfolio tier based on a
   combination of sales contribution, open receivable exposure, purchase
   frequency, and forward-risk context.

4. **Business Meaning:**  
   Strategic customers represent relationships whose loss, deterioration, or
   payment failure could materially affect revenue or working capital.

5. **How To Interpret:**

   - A high count means management has a large group of commercially important
     relationships to protect.
   - Read this KPI with **Strategic At Risk** rather than in isolation.
   - A Strategic customer is not automatically a high-risk customer.
   - Klasifikasi is a display and filter dimension; it does not determine this
     tier.

### 3.2 Strategic At Risk

1. **KPI Canonical:** `CU-KPI-064 — Strategic At Risk Count`

2. **Question Answered:**  
   How many strategically important customers show at least an early forward
   risk signal?

3. **Definition:**  
   The number of Strategic customers classified at Watch level or above in
   the customer risk view.

4. **Business Meaning:**  
   It identifies risk in the part of the portfolio where deterioration can
   affect both customer relationships and company results.

5. **How To Interpret:**

   - Any increase deserves review, even if the overall Customers At Risk count
     is unchanged.
   - A high value means risk is concentrated in important accounts.
   - Review overdue exposure, credit pressure, purchase decline, and the
     assigned portfolio action for each named customer.
   - Coordinate Sales and Finance responses rather than treating the issue as
     only a collection or sales problem.

### 3.3 Working Capital Tied

1. **KPI Canonical:** `CU-KPI-065 — Working Capital Tied Amount`

2. **Question Answered:**  
   How much open receivable is tied up in customers currently requiring
   portfolio attention?

3. **Definition:**  
   The sum of open customer balances for Attention Customers.

4. **Business Meaning:**  
   It translates the customer worklist into financial exposure. The count of
   attention customers may be manageable, but the associated balance may still
   require immediate protection or collection.

5. **How To Interpret:**

   - A high amount means significant company working capital is attached to
     customers needing review.
   - A low amount with many attention customers indicates broad operational
     workload but limited financial concentration.
   - A high amount with few customers indicates concentrated exposure and
     should lead to named-account review.
   - Compare it with Strategic At Risk and the Top Open Piutang table.

### 3.4 Total Customers

1. **KPI Canonical:** Not Found — not separately catalogued as a CU04 KPI.

2. **Question Answered:**  
   How large is the customer population being managed?

3. **Definition:**  
   The count of customers included in the portfolio universe.

4. **Business Meaning:**  
   It provides the denominator for Portfolio Healthy %, Attention Customers,
   and lifecycle percentages. It also shows the scale of the customer book
   beyond the current attention queue.

5. **How To Interpret:**

   - Use it as context for all count KPIs.
   - A stable total with a rising Attention count indicates deterioration
     within the existing customer base.
   - A change in total should be checked against customer master-data changes.

---

## 4. Lifecycle KPIs

### 4.1 Never Purchased

1. **KPI Canonical:** `CU-KPI-068 — Never Purchased Count`

2. **Question Answered:**  
   How many customers exist in the master portfolio but have never generated a
   purchase?

3. **Definition:**  
   The number of customers with no purchase history.

4. **Business Meaning:**  
   These accounts represent either unconverted acquisition opportunities,
   inactive customer records, or customer-master data that needs commercial
   review.

5. **How To Interpret:**

   - A high count may indicate a large unactivated customer base.
   - Review whether the accounts are genuine prospects, newly created
     customers, or records that should no longer receive sales coverage.
   - Never Purchased customers remain visible in the All Customers view; they
     must not be hidden by the portfolio analysis.

### 4.2 Dormant

1. **KPI Canonical:** `CU-KPI-069 — Dormant Count (Lifecycle)`

2. **Question Answered:**  
   How many customers with a purchase history have stopped purchasing for a
   prolonged period?

3. **Definition:**  
   The number of prior customers with no Faktur for 90 days or more.

4. **Business Meaning:**  
   Dormant customers represent lost commercial momentum and possible
   reactivation opportunities. If they also have open receivables, recovery
   becomes both a sales and collection concern.

5. **How To Interpret:**

   - A high or rising count indicates customer relationships are becoming
     inactive.
   - Check whether Dormant customers are assigned the Recover action.
   - Review open balance and last purchase date before deciding whether to
     reactivate, collect, or exit the relationship.

### 4.3 Declining

1. **KPI Canonical:** `CU-KPI-070 — Declining Count`

2. **Question Answered:**  
   How many customers show a weakening purchase trajectory before or while
   becoming inactive?

3. **Definition:**  
   The number of customers whose forward customer-risk assessment contains a
   purchase-decline condition.

4. **Business Meaning:**  
   Declining customers may still be active today, but their relationship is
   weakening. Early intervention can protect future omzet before the customer
   becomes Dormant.

5. **How To Interpret:**

   - A high count indicates potential future revenue attrition.
   - Review Strategic At Risk and the customer's assigned Retain or Protect
     action.
   - Investigate product availability, service quality, pricing, coverage,
     and competing suppliers before assuming the cause is collection.

### 4.4 Total MTD Omzet

1. **KPI Canonical:** `CU-KPI-066 — Total MTD Omzet (Portfolio)`

2. **Question Answered:**  
   How much invoiced sales value has the customer portfolio generated this
   month?

3. **Definition:**  
   The total current-month invoiced omzet for all customers in the portfolio.

4. **Business Meaning:**  
   It provides the commercial scale of the portfolio and establishes context
   for customer tiers, concentration, and Grow or Retain decisions.

5. **How To Interpret:**

   - A high value shows strong current commercial activity, but does not prove
     that the portfolio is healthy or profitable.
   - A falling value should be read with Declining, Dormant, and Never
     Purchased counts.
   - Use the Top 10 MTD Omzet table to determine whether the result depends on
     only a few customers.
   - This is an omzet proxy, not a profitability measure.

---

## 5. Charts

### 5.1 Lifecycle Distribution

**KPI Canonical:** Not Found — distribution chart composed of lifecycle counts.

This doughnut chart shows the number of customers in:

- Never Purchased
- Dormant
- New
- Declining
- Growing
- Mature

**Business use:**

- A large Growing or Mature share indicates a broad active commercial base.
- A large Declining or Dormant share indicates retention and recovery work.
- A large Never Purchased share indicates an unconverted or potentially
  outdated customer population.
- Use the chart together with Total MTD Omzet and Customers At Risk; customer
  count alone does not show financial impact.

### 5.2 Tier Distribution

**KPI Canonical:** Not Found — distribution chart composed of portfolio-tier
counts.

This doughnut chart shows the number of customers in:

- Strategic
- High Value
- Medium Value
- Low Value

**Business use:**

- A large Strategic or High Value share means management has a concentrated
  group of important accounts to protect.
- A large Low Value share may indicate a broad long tail that should be
  managed with proportionate effort.
- Compare the tier distribution with Strategic At Risk and Working Capital
  Tied to see where exposure is concentrated.

---

## 6. Portfolio Priority Queue

**KPI Canonical:** Not Found — management ranking table, not a separately
catalogued KPI.

The Portfolio Priority Queue is the default named worklist for Attention
Customers. It ranks customers using a deterministic Portfolio Priority Score
that combines:

- Primary portfolio action
- Portfolio tier
- Forward-risk category
- Sales and open-balance impact

The score is a prioritization aid. It is not a probability of default and is
distinct from the Collection Optimization priority score.

### Information Shown

- Rank
- Customer code and name
- Portfolio tier
- Lifecycle
- Primary action
- Portfolio priority score
- MTD Omzet
- Open Balance
- Risk category
- Action owner
- Links to Profile, Customer Report, M30, and M29 when applicable

Expanded rows also show:

- Action reason
- Triggered rules
- Last Invoicing Salesman
- Wilayah

The Last Invoicing Salesman is the commercial attribution on the latest invoice. It is not the Customer owner.

### Business Use

Start with the highest-priority rows and check the action, risk, omzet, and
open balance together. Then open the Customer Report and the relevant Sales or
Piutang evidence before taking action in BTR Desktop.

---

## 7. Customers by Portfolio Action

**KPI Canonical:** Not Found — action-segment table, not a separately
catalogued KPI.

Customers are grouped into expandable action segments:

- **Grow** — invest sales effort in an opportunity customer.
- **Retain** — protect a valuable customer showing decline.
- **Protect** — defend a Strategic customer while managing elevated risk.
- **Collect** — hand off to Collection Optimization for collection detail.
- **Review Credit** — review credit or plafond exposure.
- **Recover** — attempt to reactivate a Dormant relationship.
- **Monitor** — continue observation without immediate intervention.
- **Exit Review** — review whether a very low-value, high-risk relationship
  should continue.

Each segment shows:

- Customer code and name
- Priority score
- Open Balance
- Action reason
- Customer Report link

The action is a portfolio-management recommendation. It does not automatically
change a credit limit, suspend a customer, contact a customer, or write back
to BTR Desktop.

---

## 8. Concentration Tables

### 8.1 Top 10 MTD Omzet Concentration

**KPI Canonical:** Not Found for the CU04 table; related customer analytics
ranking is `CU-KPI-009 — Top 10 Omzet (Ranking)`.

This table ranks the ten customers with the largest current-month invoiced
omzet and shows:

- Rank
- Customer code
- Customer
- Amount
- Percentage of total omzet

**Business use:**

- Identifies the customers driving current commercial performance.
- A high percentage held by a few customers indicates revenue concentration.
- Review these names against Strategic At Risk and Declining before assuming
  the concentration is safe.

### 8.2 Top 10 Open Piutang Concentration

**KPI Canonical:** Not Found for the CU04 table; related customer analytics
ranking is `CU-KPI-010 — Top 10 Piutang (Ranking)`.

This table ranks the ten customers with the largest open receivable and shows:

- Rank
- Customer code
- Customer
- Amount
- Percentage of total open receivable

**Business use:**

- Identifies where the largest receivable exposure is concentrated.
- A small number of customers may represent most of the working capital risk.
- Compare these names with Strategic At Risk, Dormant, and the Collection
  Optimization dashboard.

---

## 9. Filters

The dashboard supports:

- **Attention Only / All Customers**
- **Wilayah**
- **Klasifikasi**
- **Tier**
- **Lifecycle**
- **Action**
- **Last Invoicing Salesman**

The Salesman filter selects the last invoicing Salesman, a commercial attribution on the latest invoice. It does not mean that Salesman owns the Customer.

Klasifikasi is available for filtering and display only. It must not be
interpreted as the source of the computed portfolio tier.

---

## Recommended Management Reading Sequence

1. Read the Executive Summary.
2. Check Portfolio Health Score, Portfolio Healthy %, and Customers At Risk.
3. Check Strategic At Risk and Working Capital Tied.
4. Review Lifecycle and Tier Distribution.
5. Open the Portfolio Priority Queue and start with the highest-priority
   customers.
6. Use Customers by Portfolio Action to assign the appropriate Sales,
   Finance, or Management response.
7. Check Top 10 MTD Omzet and Top 10 Open Piutang for concentration risk.
8. Open Customer Report, Sales Report, or Piutang Report to validate evidence.
9. Complete operational action in BTR Desktop; CU04 remains read-only.

## Related Dashboards

- **CU01 — Customer Analytics:** current customer attention and segmentation
- **CU02 — Customer Risk Forecast:** forward customer risk
- **CU03 — Collection Optimization:** daily collection action queue
- **CU05 — Customer Report:** customer-level evidence
- **Sales Report:** invoice-level sales evidence
- **Piutang Report:** open-receivable evidence

## Principal-Centric Addendum (PCM-058, implemented surfaces only)

CU04 portfolio mix reads stored `BTRPD_CustomerPrincipalRelationship` rows
only. Pair status and pair-attributed `PRN-SALES-001` are read from the
projection (PCM-044). Only Principals present on each Customer's projection
are listed. No pre-purchase assigned Principal is shown. Relationships are
not recomputed from raw transactions. Customer portfolio measures remain
Customer-level.
