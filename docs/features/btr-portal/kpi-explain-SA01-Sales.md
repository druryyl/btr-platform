# Sales Dashboard KPI Explanation

## Scope

These KPIs appear together in **SA01 — Sales Dashboard** and provide a
company-level view of current-month sales performance against the sales plan.
The values are based on invoiced sales (`Faktur`) for the current calendar
month.

### Total Target

**KPI Canonical Code:** EX-KPI-003  

**KPI Canonical Name:** Total Target

**Question Answered:**  
What sales value is the company expected to achieve this month?

**Definition:**  
The combined monthly sales target for all Salesmen who have a target configured
for the current month. Salesmen without a target do not increase this total.

**Business Meaning:**  
This is the reference point for evaluating whether the sales organization is
on plan. It allows management to compare the value the business intended to
sell with the value actually invoiced.

**How To Interpret:**

- A higher target represents a larger sales commitment for the month.
- Review target setup at the beginning of each month; a low total may reflect
  missing Salesman targets rather than a small business plan.
- This KPI is the denominator used by **Achievement %**.

### Total Achievement

**KPI Canonical Code:** EX-KPI-002  

**KPI Canonical Name:** Total Achievement (Total Omzet MTD)

**Question Answered:**  
How much sales value has the company invoiced so far this month?

**Definition:**  
The total value of official, non-voided Faktur issued during the current
calendar month. It represents current month-to-date invoiced sales
(`omzet`).

**Business Meaning:**  
This is the actual sales result generated so far. Management uses it to monitor
revenue progress, compare performance with the monthly plan, and identify
whether the sales team is generating enough business before month-end.

**How To Interpret:**

- A rising value shows that invoiced sales are being generated during the
  month.
- Compare it with **Total Target**; a large difference indicates that more
  sales must be generated before month-end.
- Review the weekly trend and Salesman ranking when achievement is behind
  plan. Use the Sales Report when transaction-level evidence is needed.

### Achievement %

**KPI Canonical Code:** EX-KPI-001  

**KPI Canonical Name:** Achievement %

**Question Answered:**  
How much of the company’s monthly sales target has been achieved?

**Definition:**  
The percentage of the monthly target represented by current month-to-date
invoiced sales.

**Formula:**

```text
Achievement % = Total Achievement ÷ Total Target × 100%
```

When **Total Target** is zero, the KPI is not displayed because performance
cannot be evaluated against a configured plan.

**Business Meaning:**  
This is the primary plan-attainment signal for Sales management. It combines
actual sales and the target into one measure that shows whether the company is
on track, needs attention, or is exceeding its plan.

**How To Interpret:**

- **Healthy — 100% or higher:** The company has met or exceeded the monthly
  target.
- **Warning — 80% to 99%:** Sales are below target and require management
  attention.
- **Critical — below 80%:** The shortfall is significant and may require
  immediate intervention.
- **Not displayed / Unknown:** No monthly target is configured; this is a
  planning setup issue, not proof of poor sales performance.

### Reading the Three KPIs Together

Read the values from left to right:

1. **Total Target** establishes the planned sales value.
2. **Total Achievement** shows the invoiced sales delivered so far.
3. **Achievement %** summarizes progress against that plan.

For example, a Total Target of 10,000,000 and Total Achievement of 8,500,000
produces an Achievement % of 85%, which is in the **Warning** range. Management
should then review the weekly sales trend and individual Salesman performance
to determine the appropriate follow-up.
