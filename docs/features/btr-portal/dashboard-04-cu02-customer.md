# CU02 — Customer Risk Forecast Dashboard KPI Explanation

## Scope

**CU02 — Customer Risk Forecast** answers:

> Which customers are likely to become collection, credit, inactivity, or
> relationship risks within the next 30 days, and where should management act
> first?

This dashboard is a **read-only**, **deterministic**, and **explainable**
forecast. It does not use AI/ML, automatically change credit limits, suspend
customers, or schedule collection activity. Forecast results are indicative
decision support and should be confirmed in the relevant BTR Desktop workflow.

All monetary values are in IDR. “Piutang” means open customer receivable.

## Important Reading Rules

- The forecast horizon is normally **30 calendar days** from the business date.
- “At risk” includes **Watch, Attention, High Risk, and Critical** customers.
- **High Risk** and **Critical** are forecast categories, not proof that a
  customer will default.
- A customer may trigger more than one signal family.
- Customer value in this dashboard refers to sales and receivable exposure; it
  is not a profitability measure.

---

## 1. First Section — Portfolio Forecast

### 1.1 Horizon (days)

1. **KPI Canonical:** Not Found — this is a forecast parameter, not a separately
   catalogued KPI.

2. **Question Answered:**  
   How far into the future does this risk view look?

3. **Definition:**  
   The number of calendar days after the dashboard’s business date covered by
   the customer risk forecast. The current CU02 design uses a 30-day horizon.

4. **Business Meaning:**  
   The horizon defines the planning window for preventive collection, credit
   review, sales recovery, and customer follow-up.

5. **How To Interpret:**

   - A longer horizon gives management more time to prepare, but projections
     are more dependent on assumptions about future billing and behaviour.
   - A shorter horizon focuses attention on nearer-term risk.
   - Always read the horizon together with **Forecast Confidence**.

### 1.2 Customer At Risk

1. **KPI Canonical:** `CU-KPI-020` — Customers Forecasted at Risk

2. **Question Answered:**  
   How many customers may require preventive attention during the forecast
   horizon?

3. **Definition:**  
   The number of customers in the **Watch, Attention, High Risk, or Critical**
   forecast categories. The dashboard’s shortened display label, “Customer At
   Risk,” refers to this forecasted population.

4. **Business Meaning:**  
   This is the breadth of forward risk. It helps management estimate how much
   Finance, Collection, and Sales capacity will be needed before problems become
   overdue, over-limit, inactive, or commercially weaker.

5. **How To Interpret:**

   - A high or rising count means preventive work is spreading across more
     accounts.
   - A low count means attention can remain concentrated, but the category mix
     still matters.
   - A rising count while current overdue balances remain calm means risk may be
     forming before it appears in aging.
   - Compare it with **High/Critical Customers**, the category counts, and the
     risk-priority table.

### 1.3 Portfolio Health Score

1. **KPI Canonical:** `CU-KPI-024` — Portfolio Health Score

2. **Question Answered:**  
   How healthy does the customer portfolio look over the forecast horizon?

3. **Definition:**  
   A composite score from 0 to 100 that combines:

   - the share of Total Piutang held by High Risk and Critical customers; and
   - the share of active customers in High Risk and Critical categories.

   The documented formula is:

   ```text
   100 − MIN(100,
     (Elevated Risk Receivable ÷ Total Piutang × 50) +
     (High Risk Customer Count ÷ Active Customer Base × 50))
   ```

4. **Business Meaning:**  
   The score provides a single management headline for preventive collection and
   credit planning. It shows whether risk is becoming material in either the
   money tied up or the number of active customer relationships affected.

5. **How To Interpret:**

   - A high score generally indicates that fewer serious-risk customers hold
     receivables and that serious risk affects a smaller share of active
     customers.
   - A low or falling score means risk is spreading through the portfolio or
     becoming financially larger.
   - Read the score with **Elevated Risk Receivable %** and the category counts;
     an average score can hide a small number of very important customers.
   - This is an indicative management index, not an automatic credit stop and
     not a profitability score.

### 1.4 Forecast Confidence

1. **KPI Canonical:** `CU-KPI-025` — Forecast Confidence

2. **Question Answered:**  
   How much operating history is available to support this forecast?

3. **Definition:**  
   A confidence band based on how many days have elapsed in the current month,
   because pace-based billing, decline, and credit projections become more
   informative as the month progresses:

   - **Low:** day 1–5
   - **Medium:** day 6–20
   - **High:** day 21 onward

4. **Business Meaning:**  
   This prevents management from treating an early-month projection as equally
   reliable as a late-month projection. It indicates how cautiously the forecast
   should be used for planning.

5. **How To Interpret:**

   - **Low** confidence means current-month activity is still limited; use the
     forecast as an early warning and expect movement as more transactions occur.
   - **Medium** confidence supports active planning, while still allowing for
     changes.
   - **High** confidence means the month contains more observed activity, but
     the result remains a rule-based indication.
   - Low confidence does not mean the risk is false; it means the projection
     should be validated more frequently.

---

## 2. Second Section — Receivable Exposure

### 2.1 Elevated Risk Receivable

1. **KPI Canonical:** `CU-KPI-022` — Elevated Risk Receivable

2. **Question Answered:**  
   How much open receivable is held by customers with serious forecast risk?

3. **Definition:**  
   The sum of open Piutang belonging to customers in the **High Risk** or
   **Critical** categories.

4. **Business Meaning:**  
   Customer counts show how many accounts need attention; this amount shows how
   much company cash is attached to those accounts. A small number of
   high-balance customers can therefore be more important than a large number
   of low-balance customers.

5. **How To Interpret:**

   - A high or rising amount means more working capital is exposed to serious
     forward risk.
   - An increase while Total Piutang is stable means risk is moving into the
     existing receivable book.
   - Compare it with **Elevated Risk %**, **High/Critical Customers**, and the
     named customer table.
   - This is current open receivable held by forecast-risk customers, not a
     prediction of future sales or a statement that the balance is
     uncollectible.

### 2.2 Elevated Risk %

1. **KPI Canonical:** `CU-KPI-023` — Elevated Risk Receivable %

2. **Question Answered:**  
   What proportion of all open receivable is held by High Risk and Critical
   customers?

3. **Definition:**  
   The share of Total Piutang represented by Elevated Risk Receivable:

   ```text
   Elevated Risk % =
   Elevated Risk Receivable ÷ Total Piutang × 100%
   ```

4. **Business Meaning:**  
   This converts the exposure amount into a portfolio concentration measure. It
   shows whether forward risk is limited to a small corner of the receivable
   book or has become a significant part of working capital.

5. **How To Interpret:**

   - A high or rising percentage means a larger share of company cash is
     connected to serious-risk customers.
   - The percentage can rise even when Total Piutang is unchanged.
   - A low percentage does not remove the need to investigate a single
     strategic or very large customer.
   - Use the percentage with the amount; a small percentage can still represent
     a material IDR exposure.

### 2.3 Total Piutang

1. **KPI Canonical:** `CU-KPI-026` — Total Piutang (Context)

2. **Question Answered:**  
   How large is the total open receivable book against which forecast risk is
   being assessed?

3. **Definition:**  
   The total open Piutang for all customers at the current snapshot. In CU02 it
   is a context and denominator KPI and should reconcile with the corresponding
   receivable total from the same refresh.

4. **Business Meaning:**  
   It gives scale to the risk indicators. Elevated Risk Receivable cannot be
   judged properly without knowing the size of the total receivable book.

5. **How To Interpret:**

   - A high amount means more company working capital is tied up in customer
     receivables.
   - Compare it with **Elevated Risk Receivable** and **Elevated Risk %**.
   - A growing Total Piutang is not automatically bad, but growth accompanied by
     rising risk exposure or delayed payment requires attention.
   - This is an open-balance measure, not current-month sales and not cash
     collected.

### 2.4 High/Critical Customers

1. **KPI Canonical:** `CU-KPI-021` — High Risk Customer Count, together with
   `CU-KPI-036` — Risk Category Count — Critical

2. **Question Answered:**  
   How many customers are in the severe forecast population, and how many of
   those are at the most serious category?

3. **Definition:**  
   The display shows two related counts:

   - **High Risk:** customers in High Risk or Critical; Critical customers are
     included in this number.
   - **Critical:** the subset in the Critical category.

4. **Business Meaning:**  
   This separates broad severe-risk workload from the most urgent management
   cases. It helps determine whether the response should be routine
   intervention, firm collection and credit review, or direct management
   escalation.

5. **How To Interpret:**

   - A high High Risk count means many customers have stacked serious signals.
   - Any Critical customers deserve individual review, especially when they are
     strategic or hold significant receivables.
   - If Critical grows faster than High Risk, the severity of the at-risk
     population is worsening.
   - Read the counts together with Elevated Risk Receivable and the
     risk-priority table. Category alone does not show financial impact.

---

## 3. Third Section — Risk Category Counts

These four displayed counts are part of the five-band category distribution.
The dashboard also has a **Critical** category, shown in the fifth section.
Categories are assigned from the number and severity of forecast signals; they
are not probabilities.

### 3.1 Healthy

1. **KPI Canonical:** `CU-KPI-032` — Risk Category Count — Healthy

2. **Question Answered:**  
   How many customers currently show no material forward-risk signal?

3. **Definition:**  
   The number of customers in the Healthy forecast category. Healthy generally
   means no material forecast rule is triggered, or the customer has no balance
   while continuing to purchase.

4. **Business Meaning:**  
   Healthy customers are the part of the portfolio that can generally be
   managed for normal growth and service rather than protection or recovery.

5. **How To Interpret:**

   - A high count means the clean portion of the book is broad.
   - A falling count means exception work is replacing normal commercial
     management.
   - Review Healthy together with Watch; a shrinking Healthy count may first
     appear as growth in Watch.

### 3.2 Watch

1. **KPI Canonical:** `CU-KPI-033` — Risk Category Count — Watch

2. **Question Answered:**  
   How many customers show early warning signs but are not yet in a more severe
   category?

3. **Definition:**  
   The number of customers with early, moderate risk, such as one moderate
   signal or two weak signals, with no strong signal.

4. **Business Meaning:**  
   Watch is the least costly point for intervention. A reminder, visit, or
   credit check may prevent the customer from progressing to Attention or High
   Risk.

5. **How To Interpret:**

   - A high count is a proactive worklist, not an immediate crisis.
   - A rapidly growing Watch count may be an early portfolio deterioration
     signal.
   - Watch falling while Attention or High Risk rises may indicate that
     customers are getting worse rather than recovering.

### 3.3 Attention

1. **KPI Canonical:** `CU-KPI-034` — Risk Category Count — Attention

2. **Question Answered:**  
   How many customers need a named owner and a specific next step within the
   forecast horizon?

3. **Definition:**  
   The number of customers with one strong signal or several moderate signals,
   but not enough severity to enter High Risk.

4. **Business Meaning:**  
   Attention is the operating layer between normal monitoring and serious
   intervention. Leaving these accounts without an owner allows risk to
   accumulate.

5. **How To Interpret:**

   - A high count means Sales and Finance need to divide and assign the work.
   - If Attention rises with Due Within 7 Days or payment signals, prioritize
     collection.
   - If it rises with inactivity or purchase decline, prioritize Sales recovery.
   - Repeated appearance across refreshes without improvement indicates that
     existing actions are not resolving the issue.

### 3.4 High Risk

1. **KPI Canonical:** `CU-KPI-035` — Risk Category Count — High Risk

2. **Question Answered:**  
   How many customers combine multiple serious signals and may soon create
   material credit, collection, or relationship problems?

3. **Definition:**  
   The number of customers in the High Risk category. Typical entry conditions
   include at least two strong signals, one strong plus multiple moderate
   signals, or chronic overdue combined with a forward signal.

4. **Business Meaning:**  
   This category should drive firm collection and credit-control attention. It
   identifies customers that should not be handled as routine follow-up.

5. **How To Interpret:**

   - A high or rising count means serious cases are becoming more widespread.
   - Pay special attention when High Risk customers are also top sales,
     strategic, or high-receivable accounts.
   - Compare the count with **Critical**, Elevated Risk Receivable, and the
     action recommendations.

### 3.5 Critical Category

1. **KPI Canonical:** `CU-KPI-036` — Risk Category Count — Critical

2. **Question Answered:**  
   How many customers are in the most severe forecast category?

3. **Definition:**  
   The number of customers with the highest combination of signal severity,
   such as at least three strong signals, or chronic overdue combined with
   plafond pressure and decline or inactivity.

4. **Business Meaning:**  
   Critical is the management-escalation population. These relationships can
   damage cash and revenue at the same time if no decision is made.

5. **How To Interpret:**

   - Any Critical customer deserves individual review; the count alone does not
     show the size of the exposure.
   - A rising count requires management to review collection, credit, and sales
     continuation together.
   - Confirm the underlying receivables, current billing, and customer history
     before deciding whether to continue, restrict, recover, or exit the
     relationship.
   - Critical is not a legal write-off status and does not by itself mean the
     debt is uncollectible.

---

## 4. Fourth Section — Signal Families

Signal-family counts show **why** customers are entering risk. They count
customers with at least one signal in the family. A customer can appear in
more than one family.

### 4.1 Payment Delay

1. **KPI Canonical:** `CU-KPI-027` — Payment Delay Signal Count

2. **Question Answered:**  
   How many customers show signs that they may pay late or are already
   developing payment-discipline problems?

3. **Definition:**  
   The number of customers with at least one payment-delay forecast signal,
   including likely late payment, escalating overdue, no recent payment, or
   slow payment on amounts due soon.

4. **Business Meaning:**  
   This identifies risk that primarily requires collection discipline and
   timely customer contact, rather than only a sales visit or credit-limit
   change.

5. **How To Interpret:**

   - A high or rising count means slower payment behaviour may create future
     overdue exposure.
   - Pay particular attention when Payment Delay is high and receivables are
     due within the horizon.
   - Compare it with collection activity; many signals with few reminders or
     collection actions indicate an execution gap.

### 4.2 Credit Limit

1. **KPI Canonical:** `CU-KPI-028` — Credit Limit Signal Count

2. **Question Answered:**  
   How many customers may approach, exceed, or worsen against their approved
   credit limit?

3. **Definition:**  
   The number of customers with at least one forward credit-limit signal:
   approaching 90% of plafond, projected to breach plafond, or already
   breached and worsening.

4. **Business Meaning:**  
   It gives Finance an opportunity to review credit exposure before a breach
   becomes larger. It also highlights where continued sales may increase
   unapproved capital exposure.

5. **How To Interpret:**

   - A high count means credit-review workload is likely to increase.
   - Credit Limit signals rising while current Plafond Breach is still low means
     the problem is forming ahead of the current-state breach.
   - Review the customer’s open balance, projected billing, and payment
     behaviour before changing supply or credit treatment.
   - A projected breach is indicative, not an automatic hold.

### 4.3 Inactivity

1. **KPI Canonical:** `CU-KPI-029` — Inactivity Signal Count

2. **Question Answered:**  
   How many customers are approaching inactivity or dormancy?

3. **Definition:**  
   The number of customers with at least one inactivity signal, such as
   approaching dormant status at 60–79 days, imminent dormant status at
   80–89 days, or forward legacy debt associated with inactivity.

4. **Business Meaning:**  
   This is an early customer-retention warning. It gives Sales time to recover
   a relationship before the customer becomes dormant and the revenue
   opportunity is harder to restore.

5. **How To Interpret:**

   - A high or rising count means more established customers may stop buying
     soon.
   - Inactivity combined with open Piutang is more serious because the customer
     may become both commercially inactive and harder to collect.
   - Compare it with Purchase Decline and actual sales visits to determine
     whether a recovery action is already underway.
   - Inactivity signals are not the same as the current Dormant Customer Count;
     they identify customers approaching the dormant condition.

### 4.4 Purchase Decline

1. **KPI Canonical:** `CU-KPI-030` — Purchase Decline Signal Count

2. **Question Answered:**  
   How many customers show a projected or observed decline in purchasing?

3. **Definition:**  
   The number of customers with at least one purchase-decline signal, including
   moderate decline, severe decline, or stopped purchasing after a prior
   purchase history.

4. **Business Meaning:**  
   This measures potential revenue attrition and relationship weakening. It
   directs management toward sales recovery, service investigation, pricing
   review, or product-availability investigation.

5. **How To Interpret:**

   - A high count means future sales from the customer base may weaken.
   - Severe or repeated decline requires faster Sales investigation than a
     single early warning.
   - When Purchase Decline overlaps with Payment Delay or Collection Risk,
     Sales and Finance should coordinate rather than treat it as only a sales
     issue.
   - Check whether the affected customers are strategically important or carry
     significant receivables.

---

## 5. Fifth Section — Collection and Severity Summary

### 5.1 Collection Risk

1. **KPI Canonical:** `CU-KPI-031` — Collection Risk Signal Count

2. **Question Answered:**  
   How many customers may require more than routine collection follow-up?

3. **Definition:**  
   The number of customers with at least one collection-risk signal, such as
   concentrated amounts due within the horizon, a chronic overdue trajectory,
   or legacy debt combined with forward overdue risk.

4. **Business Meaning:**  
   This identifies structural collection difficulty. These customers may need
   senior collection ownership, escalation, or a specific recovery plan rather
   than ordinary reminders.

5. **How To Interpret:**

   - A high or rising count means more accounts require specialised collection
     attention.
   - Collection Risk overlapping with Piutang older than 90 days indicates
     higher urgency.
   - Payment Delay and Collection Risk are different: Payment Delay describes
     payment behaviour, while Collection Risk describes difficulty or
     concentration in recovering the receivable. A customer can have both.

### 5.2 Critical Category

1. **KPI Canonical:** `CU-KPI-036` — Risk Category Count — Critical

2. **Question Answered:**  
   How many customers have reached the most severe forecast category?

3. **Definition:**  
   The same Critical count described in Section 3.5. It is repeated here so
   the management reader can compare the most severe category directly with
   Collection Risk and High Risk.

4. **Business Meaning:**  
   It highlights the customers most likely to require management involvement
   across collection, credit, and commercial decisions.

5. **How To Interpret:**

   - Any non-zero result should be investigated by customer name and exposure.
   - A Critical count without corresponding collection or management actions
     indicates that forecast information is not being converted into work.

### 5.3 High Risk Category

1. **KPI Canonical:** `CU-KPI-035` — Risk Category Count — High Risk

2. **Question Answered:**  
   How many customers are in the High Risk category, below Critical but above
   normal monitoring?

3. **Definition:**  
   The same High Risk category count described in Section 3.4. It excludes the
   separate Critical category when displayed as a category distribution.

4. **Business Meaning:**  
   It shows the size of the serious intervention queue that may require firm
   collection, credit review, or sales-continuation decisions.

5. **How To Interpret:**

   - A high result means serious risk is not limited to isolated cases.
   - Compare it with the combined **High/Critical Customers** count to
     understand how much of the severe population is Critical.
   - Investigate whether the risk is concentrated in top customers, a single
     wilayah, or one dominant signal family.

### 5.4 Customer Forecasted At Risk

1. **KPI Canonical:** `CU-KPI-020` — Customers Forecasted at Risk

2. **Question Answered:**  
   What is the total forward-risk population that needs preventive planning?

3. **Definition:**  
   The same count described in Section 1.2: customers in Watch, Attention,
   High Risk, or Critical. The fifth-section display repeats the metric to
   provide a final summary alongside the risk drivers and severity categories.

4. **Business Meaning:**  
   This is the portfolio-wide workload headline. It connects the signal
   families and category counts to the number of customer relationships that
   may require action.

5. **How To Interpret:**

   - Compare it with High Risk and Critical to distinguish broad early warning
     from severe cases.
   - A high total with mostly Watch customers suggests preventive coverage is
     needed.
   - A high total with many High Risk or Critical customers indicates immediate
     collection, credit, and management capacity is needed.

---

## 6. Charts

### 6.1 Risk Category Distribution

1. **KPI Canonical:** Not Found — this is a distribution chart composed of
   `CU-KPI-032` through `CU-KPI-036`.

2. **Question Answered:**  
   How is the forecasted customer population distributed across the five risk
   categories?

3. **Definition:**  
   A pie/doughnut chart showing customer counts in Healthy, Watch, Attention,
   High Risk, and Critical. Categories are derived from the severity and
   combination of forecast signals.

4. **Business Meaning:**  
   It shows whether the portfolio is mostly healthy, building an early-warning
   belt, or accumulating severe cases. The shape of the distribution is often
   more informative than one total count.

5. **How To Interpret:**

   - A large Healthy share indicates broad normal monitoring capacity.
   - A large Watch share indicates an opportunity for inexpensive preventive
     action.
   - A growing Attention, High Risk, or Critical share indicates deterioration
     and increasing intervention intensity.
   - Compare the chart with Elevated Risk Receivable; a small severe slice can
     still hold a large amount of money.

### 6.2 Elevated Risk vs Total Piutang

1. **KPI Canonical:** Not Found — this is a comparison chart using
   `CU-KPI-022` and `CU-KPI-026`.

2. **Question Answered:**  
   How much of the total open receivable is held by serious forecast-risk
   customers versus other customers?

3. **Definition:**  
   The chart compares:

   - **Elevated Risk Receivable:** open balance held by High Risk and Critical
     customers; and
   - **Other Open Piutang:** Total Piutang less Elevated Risk Receivable.

   Together, the two amounts represent Total Piutang.

4. **Business Meaning:**  
   This translates customer-risk categories into working-capital exposure. It
   helps management see whether the risk is primarily a count problem, a money
   problem, or both.

5. **How To Interpret:**

   - A large Elevated Risk bar means serious-risk customers hold substantial
     company cash.
   - A small Elevated Risk bar does not remove the need to review a single
     strategic or very large customer.
   - Read this chart with Elevated Risk % and the Top Customers table to find
     the accounts behind the amount.

### 6.3 Top Wilayah By Elevated Risk

1. **KPI Canonical:** Not Found — this is a geographic concentration chart.

2. **Question Answered:**  
   In which commercial territories are elevated-risk customers concentrated?

3. **Definition:**  
   A horizontal bar chart ranking the top wilayah by the **number of elevated-
   risk customers**. Elevated risk means High Risk or Critical category.

4. **Business Meaning:**  
   It helps management identify where Sales and Collection capacity may need
   coordinated attention. A territory concentration can indicate a local
   market, coverage, payment, or account-management issue.

5. **How To Interpret:**

   - A high bar means many elevated-risk customer accounts are located in that
     wilayah.
   - This chart measures customer count, not the rupiah value of exposure.
   - A wilayah with fewer customers may still carry more financial risk if its
     customers have larger balances; confirm with the table and receivable
     evidence.
   - Use the pattern to focus investigation, not to assume that the wilayah or
     its salesman is the cause.

### 6.4 Signal Family Mix

1. **KPI Canonical:** Not Found — this is a visual composition of
   `CU-KPI-027` through `CU-KPI-031`.

2. **Question Answered:**  
   What types of warning signs dominate the current customer risk forecast?

3. **Definition:**  
   A bar chart showing the number of customers with signals in each family:

   - Payment Delay
   - Credit Limit
   - Inactivity
   - Purchase Decline
   - Collection Risk

4. **Business Meaning:**  
   It helps management assign the right response. Payment Delay and Collection
   Risk generally require Finance or Collection leadership; Inactivity and
   Purchase Decline generally require Sales recovery; Credit Limit requires
   credit review.

5. **How To Interpret:**

   - The tallest bar identifies the dominant type of forward concern.
   - A high Payment Delay or Collection Risk bar suggests cash-conversion
     pressure.
   - A high Inactivity or Purchase Decline bar suggests relationship or revenue
     attrition pressure.
   - A high Credit Limit bar suggests upcoming exposure-control work.
   - The bars do not necessarily add up to the total number of at-risk
     customers because one customer may trigger multiple families.

---

## 7. Top Customers By Risk Priority

1. **KPI Canonical:** Not Found — this is a management ranking table, not a
   separately catalogued KPI.

2. **Question Answered:**  
   Which named customers should management investigate first, considering
   severity, exposure, signal burden, and due-date urgency?

3. **Definition:**  
   A ranked list of up to the top 20 forecast-risk customers. The ranking uses
   an explainable **Risk Priority Score**, not a probability of default:

   ```text
   Risk Priority Score =
     Category Weight
     + Exposure Component
     + Signal Count Component
     + Due Urgency Component
   ```

   Category weights place Critical above High Risk, followed by Attention,
   Watch, and Healthy. The ranking also considers open balance, signal
   severity, and amounts due soon. Ties are resolved by open balance and then
   customer name.

4. **Business Meaning:**  
   The table converts the dashboard from portfolio-level warning into a named
   management worklist. It shows who owns the largest combination of risk and
   financial or commercial impact.

5. **How To Interpret:**

   - Start with the highest-priority rows, but read the Category, Open Balance,
     Overdue, Due in Horizon, Primary Signal, and Decline information together.
   - A customer may rank highly because of severe category, large exposure,
     urgent due amounts, or a combination of these factors.
   - A high priority score is not a default probability and does not by itself
     authorize a credit hold or suspension.
   - Use the named customer’s receivable and sales evidence to decide the next
     action: collection, credit review, sales recovery, monitoring, or
     management escalation.

## Recommended Management Reading Sequence

1. Check **Horizon** and **Forecast Confidence** to understand the planning
   window and how mature the forecast is.
2. Read **Portfolio Health Score**, **Customer At Risk**, and **Elevated Risk
   Receivable %** for the portfolio headline.
3. Compare **Elevated Risk Receivable** with **Total Piutang** to understand
   financial exposure.
4. Use the category distribution to separate Watch/Attention workload from
   High Risk/Critical urgency.
5. Use **Signal Family Mix** to determine whether Finance, Collection, Sales,
   or Credit should lead.
6. Open **Top Customers By Risk Priority** and verify the evidence in the
    Piutang Report, Sales Report, and BTR Desktop before acting.

## Principal-Centric Addendum (PCM-058, implemented surfaces only)

CU02 Principal decline or inactivity reads stored
`BTRPD_CustomerPrincipalRelationship` rows only. Pair status and
pair-attributed `PRN-SALES-001` are read for the at-risk customers on the
page (PCM-043). Customer totals, latest-Faktur Salesman, and raw
transaction scans are not used for the Principal decline view. One Customer
can show Active and Dormant pairs at the same time. Customer risk, credit,
piutang, and decline measures otherwise remain Customer-level. Other
Customer pages are unchanged by this addendum.
