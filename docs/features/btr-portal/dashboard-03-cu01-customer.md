# CU01 — Customer Dashboard KPI Explanation

## Scope

**CU01 — Customer Analytics** answers:

> Which customers require management attention across collection, concentration,
> activity, inactivity, and credit?

The dashboard is a customer-management view combining:

- **Current-month sales activity:** distinct customers with non-void `Faktur`
  in the current calendar month.
- **All-time receivable exposure:** open customer balances from the receivable
  snapshot.
- **Customer master context:** `Plafond`, suspension status, `Klasifikasi`, and
  `Wilayah`.

The dashboard is read-only. Management investigates the evidence and performs
operational resolution in the relevant BTR Desktop workflow.

## Important Reading Rules

- **Omzet** means current-month invoiced sales.
- **Piutang** means all-time open customer receivable balance.
- Customer count cards count **customers**. The Attention List counts
  **customer × signal rows**, so one customer can appear more than once.
- Concentration percentages are informational indicators. CU01 does not assign
  an automatic warning threshold to them.
- Customer value shown through Omzet is a sales-value proxy, not profitability.

---

## 1. Attention Cards

The Attention Cards provide a fast scan of the main customer risks and
portfolio conditions. A high count or amount is not automatically a business
decision; it identifies where management should investigate.

### 1.1 Overdue Customers

1. **KPI Canonical:** `CU-KPI-001` — Overdue Customer Count

2. **Question Answered:**  
   How many customers currently require collection attention because they have
   past-due receivables?

3. **Definition:**  
   The number of distinct customers with an overdue open balance. Overdue means
   the receivable is past its due date; current, not-yet-due balances are not
   counted as overdue.

4. **Business Meaning:**  
   This measures the breadth of the collection workload. A customer may have a
   small overdue amount, but each overdue customer represents a follow-up
   requirement and a risk of slower cash conversion.

5. **How To Interpret:**

   - A high value means collection attention is spread across many customers.
   - An increasing value may indicate weakening payment discipline or an
     expanding collection workload.
   - Compare the count with **>90 Day Exposure**. Many overdue customers
     indicates broad workload; a large >90-day amount indicates more severe
     aging.
   - Use the Piutang Dashboard and Piutang Report to inspect the underlying
     receivables.

### 1.2 >90 Day Exposure

1. **KPI Canonical:** `CU-KPI-002` — >90 Day Exposure

2. **Question Answered:**  
   How much customer receivable is in the chronic overdue category?

3. **Definition:**  
   The amount of open Piutang more than 90 days past due. This is an amount,
   not a customer count, and uses the all-time open receivable snapshot.

4. **Business Meaning:**  
   Exposure older than 90 days is a strong signal of collection failure,
   potential bad-debt risk, and working capital that may not return soon.

5. **How To Interpret:**

   - A high value means a material amount of company capital is tied up in
     seriously overdue accounts.
   - A rising value means overdue debt is becoming more chronic.
   - Prioritize the customers and invoices contributing to the amount, rather
     than relying on the total alone.
   - Investigate through the Piutang Dashboard or Piutang Report.

### 1.3 Top Omzet Customer %

1. **KPI Canonical:** `CU-KPI-003` — Top Omzet Customer %

2. **Question Answered:**  
   How dependent is current-month sales on the single largest customer?

3. **Definition:**  
   The largest customer's current-month invoiced Omzet divided by total
   company current-month invoiced Omzet.

   ```text
   Top Omzet Customer % =
   Largest Customer MTD Omzet ÷ Company MTD Omzet × 100%
   ```

4. **Business Meaning:**  
   This is a revenue concentration indicator. It shows whether a large part of
   current billing depends on one account, which may create commercial
   dependency if that customer reduces purchases or changes supplier.

5. **How To Interpret:**

   - A high percentage means revenue is concentrated in one customer.
   - A low percentage generally indicates broader revenue distribution.
   - Treat a high value as a management discussion point, not an automatic
     failure.
   - Review the **Top 10 by Omzet** table to see whether concentration is
     limited to one customer or spread across several major accounts.

### 1.4 Top Piutang Customer %

1. **KPI Canonical:** `CU-KPI-004` — Top Piutang Customer %

2. **Question Answered:**  
   How dependent is the receivable portfolio on the single largest debtor?

3. **Definition:**  
   The largest customer's open Piutang divided by total company open Piutang.

   ```text
   Top Piutang Customer % =
   Largest Customer Open Piutang ÷ Total Open Piutang × 100%
   ```

4. **Business Meaning:**  
   This identifies receivable concentration risk. If one customer represents a
   large share of the receivable book, delayed payment or default by that
   customer can materially affect cash flow.

5. **How To Interpret:**

   - A high percentage means collection risk is concentrated in one debtor.
   - A low percentage indicates that receivable exposure is distributed more
     broadly.
   - Compare this card with **Top Omzet Customer %**. A customer can dominate
     sales, receivables, or both.
   - Use the **Top 10 by Piutang** table and Piutang Report to investigate.

### 1.5 Active Customers (month)

1. **KPI Canonical:** `CU-KPI-005` — Active Customer Count

2. **Question Answered:**  
   How many distinct customers have bought from the business this month?

3. **Definition:**  
   The number of distinct customers with at least one non-void Faktur during
   the current calendar month.

4. **Business Meaning:**  
   Active Customers measures the breadth of current market activity. Omzet can
   look strong because of a few large orders; this count shows whether the
   customer base is broadly participating in current sales.

5. **How To Interpret:**

   - A high or rising value generally indicates broad customer engagement.
   - A low value may indicate narrow coverage, reduced demand, or dependence on
     a few large customers.
   - Read this together with **Top Omzet Customer %** and **Dormant Customers**.
   - Use the Sales Dashboard for company-level invoicing context.

### 1.6 Dormant Customers (90-day)

1. **KPI Canonical:** `CU-KPI-006` — Dormant Customer Count

2. **Question Answered:**  
   How many customers with prior purchase history have stopped buying for at
   least 90 days?

3. **Definition:**  
   The number of customers with prior Faktur history whose latest Faktur is at
   least 90 days old and who are not active in the current month.

4. **Business Meaning:**  
   Dormancy is an early customer-retention and revenue-attrition signal. These
   customers may require a recovery visit, sales follow-up, or investigation
   into service, pricing, product availability, or competitor activity.

5. **How To Interpret:**

   - A high or increasing value means more previously active relationships are
     no longer producing current sales.
   - A low value suggests fewer established accounts have become inactive under
     the 90-day rule.
   - Do not treat dormant customers as never-purchased customers; Dormant
     requires prior purchase history.
   - Review the Dormant tab in the Attention List and the customer's sales
     history.

### 1.7 Plafond Breach

1. **KPI Canonical:** `CU-KPI-007` — Plafond Breach Count

2. **Question Answered:**  
   How many customers currently owe more than their approved credit limit?

3. **Definition:**  
   The number of customers whose open Piutang exceeds their configured
   `Plafond`. Only customers with a positive configured Plafond are evaluated.

   ```text
   Plafond Breach =
   Customer Open Piutang > Customer Approved Plafond
   ```

4. **Business Meaning:**  
   This is a credit-policy enforcement signal. A breach means the company has
   extended exposure beyond the customer's approved limit and may be carrying
   additional collection risk.

5. **How To Interpret:**

   - A high value means credit-limit exceptions are widespread.
   - Review both the size of the breach and the customer's payment history;
     the count alone does not show financial impact.
   - Investigate whether the limit is outdated, the account needs collection,
     or further sales should be reviewed.
   - Use the Plafond Breach tab and Piutang Report as evidence.

### 1.8 Suspended + Sales

1. **KPI Canonical:** `CU-KPI-008` — Suspended + Sales Count

2. **Question Answered:**  
   Are suspended customer accounts still receiving current-month billing?

3. **Definition:**  
   The number of customers marked as suspended in master data that still have
   at least one current-month Faktur.

   ```text
   Suspended + Sales =
   Customer IsSuspend = true AND at least one current-month Faktur
   ```

4. **Business Meaning:**  
   This is an operational and compliance exception. It indicates that the
   customer master status and actual billing activity are inconsistent.

5. **How To Interpret:**

   - Any non-zero value requires review.
   - Verify whether the suspension is still valid, whether the billing was
     authorized, and whether master data or sales handling needs correction.
   - This signal is different from a Plafond Breach: a customer can be
     suspended without exceeding Plafond, and can exceed Plafond without being
     suspended.
   - Validate the account in customer master data and confirm the Faktur in the
     Sales Report.

---

## 2. Customer Attention List

1. **KPI Canonical:** Not Found — this is a management attention table, not a
   separately catalogued KPI.

2. **Question Answered:**  
   Which individual customers require follow-up, and why?

3. **Definition:**  
   A filterable list of attention items with one row per **customer × signal**.
   The available tabs are:

   - **All:** all attention signals.
   - **Overdue:** customers with past-due receivables.
   - **Dormant:** customers meeting the 90-day inactivity rule.
   - **Plafond Breach:** customers exceeding their approved credit limit.
   - **Suspended + Sales:** suspended customers with current-month billing.

4. **Business Meaning:**  
   The cards show the scale of an issue; this list identifies the accounts that
   management, Sales, or Collection can actually investigate. It converts
   summary exceptions into a customer-level work queue.

5. **How To Interpret:**

   - Start with the relevant tab when a card shows a material issue.
   - A customer may appear in multiple tabs because multiple signals can apply.
   - Use the row's signal and value to determine the next investigation path.
   - Collection and credit issues generally require Piutang evidence; Dormant
     and Suspended + Sales issues generally require sales and master-data
     review.
   - The list identifies exceptions; it does not automatically change credit
     limits, suspend accounts, or record collection activity.

---

## 3. Top Customer Rankings

The two rankings show where the largest customer value and receivable exposure
are concentrated. They are complementary and use different periods.

### 3.1 Top 10 by Omzet (Current Month)

1. **KPI Canonical:** `CU-KPI-009` — Top 10 Omzet (Ranking)

2. **Question Answered:**  
   Which customers contribute the most invoiced sales this month?

3. **Definition:**  
   Up to ten customers ordered by descending current-month invoiced Omzet.
   Each row includes the customer's Omzet and its percentage of company
   current-month Omzet.

4. **Business Meaning:**  
   This ranking identifies the accounts currently driving billing. It supports
   account prioritization, sales coverage decisions, and assessment of revenue
   dependency.

5. **How To Interpret:**

   - A high rank means the customer is a major contributor to this month's
     invoiced sales.
   - Compare the ranking with **Active Customers** to distinguish broad
     customer activity from sales driven by a small group.
   - Review whether top accounts are also overdue, dormant in a prior period,
     or near credit limits.
   - A ranking position is not a profitability measure and does not by itself
     indicate customer quality.

### 3.2 Top 10 by Piutang (All Open)

1. **KPI Canonical:** `CU-KPI-010` — Top 10 Piutang (Ranking)

2. **Question Answered:**  
   Which customers carry the largest outstanding receivable balances?

3. **Definition:**  
   Up to ten customers ordered by descending all-time open Piutang. Each row
   includes the customer's open balance and its percentage of total open
   Piutang.

4. **Business Meaning:**  
   This ranking identifies the accounts with the greatest amount of company
   working capital tied up in receivables. It helps Collection and management
   prioritize exposure review.

5. **How To Interpret:**

   - A high rank means the customer represents a large receivable exposure.
   - A large balance is not automatically overdue; check aging and due dates
     before treating it as a collection failure.
   - Compare the ranking with **Overdue Customers**, **>90 Day Exposure**, and
     **Plafond Breach**.
   - Use the Piutang Report to inspect the open Faktur behind the ranking.

---

## 4. Active vs Dormant Summary

1. **KPI Canonical:** Not Found — this is a distribution/comparison section
   using `CU-KPI-005` and `CU-KPI-006`.

2. **Question Answered:**  
   Is the customer base currently active, or is a meaningful portion inactive?

3. **Definition:**  
   A comparison of:

   - **Active:** distinct customers invoiced in the current calendar month.
   - **Dormant:** customers with prior purchase history, no Faktur for at least
     90 days, and no current-month activity.

4. **Business Meaning:**  
   This gives management a quick view of customer-base vitality and potential
   retention risk. It connects current sales reach with the backlog of
   established accounts that may need recovery.

5. **How To Interpret:**

   - More Active customers generally indicates broader current sales coverage.
   - More Dormant customers indicates a larger recovery or reactivation
     opportunity.
   - An Active customer can still have receivable or credit issues; activity
     does not mean the account is healthy.
   - Active and Dormant are not necessarily an exhaustive partition of all
     master customers. For example, a recently inactive customer may be
     neither active this month nor dormant for 90 days.

---

## 5. Customer Segmentation Tables

The segmentation tables explain where customers are distributed. They are
context for the attention cards and rankings, not separate financial KPIs.

### 5.1 By Klasifikasi

1. **KPI Canonical:** Not Found — segmentation table.

2. **Question Answered:**  
   How is the customer base distributed across the classifications maintained
   in customer master data?

3. **Definition:**  
   Customer counts grouped by `Klasifikasi`, with Active/Dormant context where
   displayed. The values reflect the classification assigned in master data;
   the dashboard does not redefine the classification.

4. **Business Meaning:**  
   This shows the composition of the customer portfolio and whether activity or
   inactivity is concentrated in a particular customer class.

5. **How To Interpret:**

   - A large classification may represent a major customer segment or simply a
     broad master-data category.
   - Compare its active and dormant counts to identify segments requiring
     retention or sales coverage attention.
   - Do not treat `Klasifikasi` as a credit limit, profitability tier, or
     automatically assigned portfolio value.

### 5.2 By Wilayah

1. **KPI Canonical:** Not Found — segmentation table.

2. **Question Answered:**  
   In which commercial territories are customers concentrated, active, or
   dormant?

3. **Definition:**  
   Customer counts grouped by `Wilayah`, with Active/Dormant context where
   displayed. `Wilayah` is the customer's commercial territory.

4. **Business Meaning:**  
   This reveals geographic coverage and territory-level customer health. It
   helps management identify regions with strong activity, weak coverage, or a
   concentration of dormant accounts.

5. **How To Interpret:**

   - A high customer count does not necessarily mean high sales; validate with
     Omzet rankings and sales results.
   - A territory with many Dormant customers may need a focused recovery or
     field-sales review.
   - Compare territory distribution with overdue and Piutang exposure before
     assigning collection capacity.
   - `Wilayah` is a segmentation dimension; it is not itself an overdue or
     sales-performance score.

---

## Recommended Management Reading Sequence

1. **Scan Attention Cards** to identify the largest customer risks or
   concentrations.
2. **Open the matching Attention List tab** to identify the affected
   customers.
3. **Compare Top 10 Omzet and Top 10 Piutang** to see whether sales leaders and
   largest debtors overlap.
4. **Review Active vs Dormant** to assess current customer-base vitality.
5. **Use Klasifikasi and Wilayah** to locate segment or territory patterns.
6. **Investigate evidence** in the Sales Dashboard/Report or Piutang
   Dashboard/Report, then resolve operational issues in BTR Desktop.

CU01 is a current-state customer attention view. Forward-looking customer risk
is handled by the Customer Risk Forecast Dashboard, while daily collection
prioritization is handled by Collection Optimization.
