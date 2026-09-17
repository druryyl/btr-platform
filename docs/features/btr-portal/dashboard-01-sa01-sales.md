# SA01 — Sales Dashboard KPI Explanation

## Scope

These KPIs provide management with a company-level view of current-month
sales performance against the monthly sales plan. The sales result is based on
official, non-voided Faktur issued during the current calendar month.

### 1. Total Target

1. **KPI Canonical:** `EX-KPI-003` — Total Target

2. **Question Answered:**  
   What sales value is the company expected to achieve this month?

3. **Definition:**  
   The combined monthly sales target for all Salesmen with a target configured
   for the current month. Salesmen without a target do not increase this total.

4. **Business Meaning:**  
   Total Target is the plan against which current sales performance is judged.
   It gives management a clear reference for the sales commitment that should
   be delivered during the month.

5. **How To Interpret:**

   - A higher value represents a larger sales commitment for the month.
   - Review the target setup at the beginning of each month.
   - A lower-than-expected value may indicate missing Salesman targets rather
     than a smaller business plan.
   - This KPI is the denominator used by **Achievement %**.

### 2. Total Achievement

1. **KPI Canonical:** `EX-KPI-002` — Total Achievement (Total Omzet MTD)

2. **Question Answered:**  
   How much sales value has the company invoiced so far this month?

3. **Definition:**  
   The total invoiced omzet from official, non-voided Faktur issued during the
   current calendar month. It represents the actual month-to-date sales result.

4. **Business Meaning:**  
   Total Achievement shows how much sales value the business has actually
   generated so far. Management uses it to monitor billing progress and
   identify early whether additional sales effort is needed before month-end.

5. **How To Interpret:**

   - A rising value indicates that current-month invoicing is progressing.
   - Compare it with **Total Target** to understand the remaining sales gap.
   - A result that remains well below target as the month advances requires
     investigation of the weekly trend and Salesman performance.
   - Use the Sales Report when transaction-level evidence is needed.

### 3. Achievement %

1. **KPI Canonical:** `EX-KPI-001` — Achievement %

2. **Question Answered:**  
   How much of the company’s monthly sales target has been achieved?

3. **Definition:**  
   The percentage of the monthly target represented by current month-to-date
   invoiced omzet.

   ```text
   Achievement % = Total Achievement ÷ Total Target × 100%
   ```

   When **Total Target** is zero, the KPI is blank because performance cannot
   be evaluated against a configured plan.

4. **Business Meaning:**  
   Achievement % is the primary company-level signal of sales performance
   against plan. It helps management decide whether sales execution is on
   track, needs attention, or is exceeding expectations.

5. **How To Interpret:**

   - **Healthy — 100% or higher:** The monthly target has been met or exceeded.
   - **Warning — 80% to 99%:** Sales are below target and need management
     attention.
   - **Critical — below 80%:** The shortfall is significant and may require
     immediate intervention.
   - **Blank / Unknown:** No monthly target is configured. This is a planning
     setup issue, not proof of poor sales performance.

## Reading the Three KPIs Together

Read the KPIs in this order:

1. **Total Target** establishes the planned sales value.
2. **Total Achievement** shows the invoiced sales delivered so far.
3. **Achievement %** summarizes progress against the plan.

For example, a Total Target of 10,000,000 and Total Achievement of 8,500,000
produces an Achievement % of 85%, which is in the **Warning** range.
Management should then review the weekly sales trend and individual Salesman
performance to determine the appropriate follow-up.

## Supporting Sales Sections

### 4. Weekly Trend

1. **KPI Canonical:** `SA-KPI-006` — Weekly Invoiced Sales Trend

2. **Question Answered:**  
   Is invoiced sales momentum accelerating, stable, or slowing during the
   month?

3. **Definition:**  
   The total invoiced omzet for each calendar week within the current month.
   It uses official, non-voided Faktur and shows how billing is distributed
   across the month.

4. **Business Meaning:**  
   Weekly Trend adds timing and momentum context to the month-to-date sales
   total. Two months can have the same achievement value but require different
   management responses if one is accelerating and the other is slowing.

5. **How To Interpret:**

   - Increasing weekly values generally indicate improving billing momentum.
   - Declining weekly values may signal weakening demand, reduced field
     execution, or a need to intensify sales follow-up.
   - A strong early week followed by weaker weeks is a warning that the month
     may finish below plan even when current Achievement % looks acceptable.
   - Use the trend together with **Achievement %** and the remaining days in
     the month; the trend alone does not measure target attainment.

### 5. Top 10 Salesman

1. **KPI Canonical:** `SA-KPI-007` — Top 10 Salesman (Omzet)

2. **Question Answered:**  
   Which Salesmen are contributing the most invoiced sales this month?

3. **Definition:**  
   A ranking of up to ten Salesmen with the highest current-month invoiced
   omzet, ordered from the highest invoiced amount to the lowest.

4. **Business Meaning:**  
   This ranking identifies the main contributors to company billing. It helps
   management recognize strong performance, understand where current sales are
   coming from, and identify whether the business depends too heavily on a
   small number of Salesmen.

5. **How To Interpret:**

   - A high position means the Salesman has generated a large invoiced sales
     amount during the current month.
   - A short or concentrated ranking may indicate that only a few Salesmen are
     carrying most of the company’s billing.
   - Compare the ranking with **Achievement %** or the Salesman Performance
     Dashboard. A high omzet rank does not necessarily mean the Salesman has
     achieved the highest percentage of their target.
    - Review target attainment, customer portfolio, and field execution before
      treating the ranking as a complete performance judgment.

## Principal-Centric Addendum (PCM-058, implemented surfaces only)

SA01 keeps company header totals from the existing Faktur header
`GrandTotal` measure and keeps Top Salesman contribution. Principal
contribution reads stored `PRN-SALES-001` Principal Sales-Out and stored
`PRN-TGT-001` Principal Target, ranks Principals by `PRN-SALES-001` only,
and opens SA04 Principal Performance (`/dashboard/principal-performance`)
for the selected Principal (PCM-008).

Purchase-In, Inventory, Returns, collection, credit, and Net Sales are not
shown as Principal sales on SA01. Company header totals are not required to
equal the sum of `PRN-SALES-001`.
