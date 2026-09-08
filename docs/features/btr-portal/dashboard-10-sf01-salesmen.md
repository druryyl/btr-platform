# SF01 — Salesmen Dashboard KPI Explanation

## Scope

SF01 — **Salesmen** is the Salesman Performance dashboard at
`/dashboard/salesmen`. It answers:

> Which salesmen require management attention, and why?

The dashboard combines:

- current-month invoiced sales (`Omzet`);
- monthly target achievement;
- all-open customer receivable (`Piutang`) exposure; and
- the quality of each salesman's customer portfolio.

It is an outcome and portfolio dashboard. Visit execution, route compliance,
GPS, and effective-call metrics belong to the Sales Force and Salesman Field
Activity dashboards.

All monetary values are in IDR. The KPI Canonical identifiers below use the
hierarchical identifiers from the BTR KPI Encyclopedia. Where the encyclopedia
does not define a separate KPI for a display item, this document states
**KPI Canonical: Not Found** rather than inventing a code.

## Important Reading Rules

- Sales KPIs use official, non-voided Faktur from the current calendar month.
- Piutang rankings and exposure use all open receivable balances, not only this
  month's invoices.
- A salesman can appear more than once in the Attention List because one
  salesman can have several attention signals.
- **Missing Target Setup** is a planning problem. It must not be interpreted as
  zero achievement or as proof that the salesman is underperforming.
- **Below Target** applies only when a target exists and achievement is below
  the configured performance bands.
- High exposure is relative: the default rule identifies the top 20% among
  salesmen with a positive balance. It is not a fixed rupiah threshold.
- Portfolio concentration percentages are informational; the dashboard does
  not define an automatic warning threshold for them.

---

## 1. Attention Cards

The three cards provide the first management scan. A card's attention styling
means that at least one of its related exceptions exists.

### 1.1 Performance Card

#### Below Target Count

1. **KPI Canonical:** `2.1.1.1` — Jumlah di Bawah Target

2. **Question Answered:**  
How many salesmen with configured targets are currently behind plan?

3. **Definition:**  
The number of salesmen whose achievement is in the Warning band
(80–99%) or Critical band (below 80%). Salesmen without a configured target
are excluded.

4. **Business Meaning:**  
This is the coaching and intervention queue. It separates salesmen who are
behind an agreed plan from salesmen whose plan has not been configured.

5. **How To Interpret:**

- A high or rising count means more of the sales team is falling behind its
  assigned plan.
- A low count does not prove that the whole team is healthy; check Missing
  Target Setup first.
- Repeated appearance of the same names suggests a sustained performance,
  coverage, capability, or territory issue.
- Open the Top 10 Achievement % table and Sales Report to investigate.

#### Missing Target Setup Count

1. **KPI Canonical:** `2.1.1.2` — Jumlah Salesman tanpa Target

2. **Question Answered:**  
How many commercially active salesmen cannot yet be evaluated against a target?

3. **Definition:**  
The number of salesmen with current-month activity—Omzet or invoiced
customers—but no configured monthly target.

4. **Business Meaning:**  
This measures planning completeness, not sales ability. A missing target makes
achievement comparisons incomplete and can make the Below Target count look
artificially low.

5. **How To Interpret:**

- Any non-zero result after the beginning of the month deserves review.
- A high count means management cannot consistently assess sales performance
  against plan.
- Set the target before judging the salesman's achievement percentage.
- Until a target exists, use Omzet and customer activity as supporting evidence,
  not as a substitute for a formal target.

### 1.2 Collection Exposure Card

#### High Piutang Exposure Count

1. **KPI Canonical:** `2.3.2.1` — Jumlah Eksposur Piutang Tinggi

2. **Question Answered:**  
How many salesmen carry a disproportionately large share of open customer
receivables?

3. **Definition:**  
The number of salesmen in the top 20% by open Piutang among salesmen with a
positive open balance.

4. **Business Meaning:**  
Receivable risk is also a sales-management issue because the invoicing
salesman owns the customer relationships that created the outstanding balance.
The KPI identifies where Sales and Finance should coordinate.

5. **How To Interpret:**

- A high result means open receivables are concentrated in several salesman
  portfolios.
- A high count in a small team may still represent only a few names because the
  threshold is relative.
- Read the count together with Top 10 Piutang and High Overdue Exposure.
- Investigate a salesman with high Piutang but modest current-month Omzet for
  possible legacy debt or weak account maintenance.

#### High Overdue Exposure Count

1. **KPI Canonical:** `2.3.2.2` — Jumlah Eksposur Piutang Jatuh Tempo Tinggi

2. **Question Answered:**  
How many salesmen carry a disproportionately large share of overdue
receivables?

3. **Definition:**  
The number of salesmen in the top 20% by overdue balance among salesmen with
overdue receivables.

4. **Business Meaning:**  
This is the salesman-level collection accountability signal. It distinguishes
money that is merely outstanding from money that is already late.

5. **How To Interpret:**

- A high result means overdue balances are concentrated in a limited number of
  portfolios.
- The same salesman appearing repeatedly needs a joint Sales and Finance
  collection plan.
- Strong current sales do not cancel this risk; a salesman may be generating
  new billing while old invoices remain unpaid.
- Use the Piutang Report to inspect the customers behind the salesman.

### 1.3 Portfolio Card

#### Dormant Portfolio Count

1. **KPI Canonical:** `2.3.1.1` — Jumlah Portofolio Tidak Aktif

2. **Question Answered:**  
How many salesmen have at least one dormant customer in their portfolio?

3. **Definition:**  
The number of salesmen linked to at least one customer with prior purchase
history but no Faktur for 90 or more days. Attribution normally follows the
salesman on the customer's last invoice.

4. **Business Meaning:**  
This shows where customer relationships may be deteriorating under a
salesman's ownership. Dormant customers can reduce future Omzet and may also
leave receivables harder to collect.

5. **How To Interpret:**

- A high result means customer-recovery work is spread across many portfolios.
- A dormant portfolio under a top Omzet salesman is still a warning; current
  sales can hide deterioration in other accounts.
- Check whether the customer has open Piutang and whether visits or recovery
  activity are planned.
- Use Customer Analytics and the Sales Report to decide whether to recover,
  reassign, or formally close the account.

#### Top Omzet Salesman %

1. **KPI Canonical:** `2.1.2.2` — Salesman Omzet Teratas %

2. **Question Answered:**  
How dependent is company invoiced sales on its single highest-Omzet salesman?

3. **Definition:**  
The highest salesman's current-month invoiced Omzet divided by total
current-month salesman Omzet.

4. **Business Meaning:**  
This is a concentration measure. It indicates how exposed company billing is
to the absence, performance change, or customer relationships of one person.

5. **How To Interpret:**

- A high percentage means the sales result depends heavily on one salesman.
- A rising percentage while the number of active salesmen is stable may mean
  the rest of the team is contributing less.
- There is no automatic warning threshold; compare the percentage over time and
  with the Top 10 Omzet table.
- Also review the salesman's Piutang and dormant portfolio exposure.

#### Top Piutang Salesman %

1. **KPI Canonical:** `2.3.2.3` — Salesman Piutang Teratas %

2. **Question Answered:**  
How concentrated is company open Piutang in the largest salesman portfolio?

3. **Definition:**  
The largest salesman's open Piutang divided by total open Piutang attributed to
salesmen.

4. **Business Meaning:**  
This identifies working capital dependence on one customer portfolio. A
salesman with a large share of Piutang also carries a large share of collection
and customer-quality risk.

5. **How To Interpret:**

- A high percentage means open receivables are concentrated in one portfolio.
- A high percentage combined with High Overdue Exposure is more urgent than a
  high percentage made up mainly of current, not-yet-due invoices.
- There is no automatic warning threshold.
- Read it with Top 10 Piutang and the Piutang Report.

---

## 2. Salesman Filters

### Show Inactive Salesmen

1. **KPI Canonical:** Not Found — this is a presentation filter.

2. **Question Answered:**  
Should the dashboard focus on currently active salesmen or show the full
salesman population?

3. **Definition:**  
By default, rankings and attention rows show salesmen with at least one
current-month Faktur. Enabling the toggle includes salesmen without current
month invoicing activity.

4. **Business Meaning:**  
The default keeps management attention on the active selling team. The
expanded view helps investigate inactive capacity, dormant portfolios, and
salesmen who still carry receivable exposure.

5. **How To Interpret:**

- Use the default view for current-month performance review.
- Enable the toggle when reviewing inactive manpower, historical portfolios,
  or salesmen with open Piutang but no current billing.
- Inactive does not automatically mean poor performance; it means there is no
  current-month invoicing activity.

---

## 3. Salesman Attention List

The Attention List contains one row per **salesman × signal**, so its row count
can be greater than the number of salesmen shown on the Attention Cards.

### Attention Signals

| Signal | Business explanation |
| --- | --- |
| Below Target | Configured target exists and achievement is in Warning or Critical band. |
| Missing Target Setup | Current-month sales activity exists, but no target is configured. |
| High Overdue Exposure | Salesman is in the relative top 20% for overdue balance. |
| High Piutang Exposure | Salesman is in the relative top 20% for open balance. |
| Customer Concentration | A large share of the salesman's Omzet comes from one customer; informational. |
| Dormant Customer Portfolio | At least one formerly purchasing customer has been inactive for 90+ days. |

### Attention List Table

Columns:

- Salesman Code
- Salesman
- Signal
- Detail, such as an amount, count, or percentage
- Wilayah
- Profile action
- Investigate action

**KPI Canonical:** Not Found — this is an exception worklist composed of the
signals above.

**Question Answered:**  
Which named salesmen require attention, and what is the reason?

**Business Meaning:**  
The list converts summary counts into an actionable management queue. It tells
management whether the next discussion should be about target coaching,
collection, customer recovery, or account concentration.

**How To Interpret:**

- Start with the signal, then read the Detail value and salesman name.
- Multiple rows for one salesman indicate combined risk and should be reviewed
  together.
- Use Sales Report for performance, target, concentration, and dormant signals.
- Use Piutang Report for open and overdue exposure signals.

---

## 4. Performance Rankings

### Top 10 Omzet — Current Month

1. **KPI Canonical:** `2.1.2.3` — Peringkat 10 Omzet Teratas

2. **Question Answered:**  
Which salesmen generated the most invoiced sales this month?

3. **Definition:**  
Up to ten salesmen ranked by current-month invoiced Omzet, with their share of
total salesman Omzet.

4. **Business Meaning:**  
This identifies the main contributors to current billing and reveals whether
the company depends on a narrow group of producers.

5. **How To Interpret:**

- A high rank means high current-month billing, not necessarily high target
  achievement.
- Compare this table with Top 10 Achievement %. Different names indicate a
  difference between absolute production and performance against plan.
- A concentrated ranking may indicate key-person and customer-relationship
  dependency.

Columns: Rank, Salesman Code, Salesman, Omzet, and % of Total.

### Top 10 Achievement %

1. **KPI Canonical:** `2.1.1.3` — Peringkat 10 Pencapaian Teratas

2. **Question Answered:**  
Which salesmen are furthest ahead against their configured plans?

3. **Definition:**  
Up to ten salesmen ranked by current-month Omzet divided by their configured
monthly target. Salesmen without targets cannot be ranked by achievement.

4. **Business Meaning:**  
This measures performance relative to expectation. It prevents a large
territory or large absolute book from being treated as the only definition of
success.

5. **How To Interpret:**

- A high rank means the salesman is ahead of, or closest to, their plan.
- A salesman can rank high here with lower absolute Omzet if the target is
  smaller; compare with Top 10 Omzet.
- Achievement bands are:
  - **Healthy:** 100% or higher
  - **Warning:** 80–99%
  - **Critical:** below 80%
  - **Unknown:** no target
- Review target quality separately; a high percentage does not prove the target
  was sufficiently challenging.

Columns: Rank, Salesman Code, Salesman, Achievement %, and Omzet.

---

## 5. Exposure Rankings

### Top 10 Piutang — All Open

1. **KPI Canonical:** Not Found — the encyclopedia defines the related
   concentration KPI `2.3.2.3`, but not a separate salesman Top 10 Piutang
   ranking code.

2. **Question Answered:**  
Which salesmen carry the largest open receivable portfolios?

3. **Definition:**  
Up to ten salesmen ranked by open customer Piutang attributed to their
invoicing portfolios. The table also shows each salesman's share of total
salesman-attributed open Piutang.

4. **Business Meaning:**  
This creates a named ownership view of working capital exposure. It helps
management connect customer debt to the sales relationship responsible for the
account.

5. **How To Interpret:**

- A high rank means a large open balance, not necessarily a large overdue
  balance.
- Compare with High Overdue Exposure and the Piutang Report.
- A high Piutang rank with low current Omzet may indicate aging or legacy debt.
- A high Piutang rank with high Omzet requires a joint review of growth and
  collection quality.

Columns: Rank, Salesman Code, Salesman, Outstanding, and % of Total.

---

## 6. Salesman Detail Drawer

The detail drawer opens from a salesman name or ranking row. It contains
Principal-level performance and historical achievement context.

### 6.1 Principal Achievement Table

1. **KPI Canonical:** `2.1.1.4` — Tabel Pencapaian Utama

2. **Question Answered:**  
Which Principals are contributing to, or falling behind, this salesman's
target?

3. **Definition:**  
For the selected salesman, the table compares each Principal's target,
current-month Omzet, achievement percentage, and achievement band.

4. **Business Meaning:**  
A salesman can appear acceptable at total level while missing important
Principal targets. This table shows where coaching, product focus, inventory
checking, or Principal coordination is needed.

5. **How To Interpret:**

- Review the Principal rows with low achievement or missing targets.
- If many salesmen are weak on the same Principal, investigate availability,
  pricing, promotion, or inventory rather than blaming one person.
- If one salesman is weak across Principals, investigate coverage, capability,
  or customer portfolio quality.

Columns: Principal, Target, Omzet, Achievement %, and Achievement Band.

### 6.2 Achievement Trend Chart

1. **KPI Canonical:** Not Found — this is a historical visualization of
   salesman achievement.

2. **Question Answered:**  
Is this salesman's performance against target improving, stable, or weakening
over time?

3. **Definition:**  
A line chart of monthly Achievement % history, normally showing up to the last
12 available periods. At least two history periods are required to display a
meaningful trend.

4. **Business Meaning:**  
A single month can be affected by timing, seasonality, or one large invoice.
The trend helps management distinguish a temporary miss from a persistent
coaching or territory problem.

5. **How To Interpret:**

- An upward line suggests improving target attainment.
- A downward line suggests weakening performance and requires investigation.
- A flat line below 100% indicates a persistent shortfall.
- A trend with fewer than two periods should not be treated as evidence; wait
  for more month-end snapshots.
- Read the trend together with Omzet, target setup, customer portfolio, and
  field-activity evidence.

---

## 7. Segmentation Summary

### Active vs Inactive

1. **KPI Canonical:** `2.5.1.1` — Salesman Aktif

2. **Question Answered:**  
How large is the currently active sales team compared with the inactive
population?

3. **Definition:**  
**Active** means the salesman has at least one current-month Faktur.
**Inactive** means there is no current-month Faktur activity.

4. **Business Meaning:**  
This provides the capacity context for the dashboard. A small active team,
inactive portfolios, or open Piutang without current billing changes how
rankings and totals should be interpreted.

5. **How To Interpret:**

- A falling Active count may indicate reduced selling capacity.
- A high Inactive count requires review of staffing, territory assignment,
  leave, or data completeness.
- Do not compare raw Omzet totals across months without considering the active
  team size.
- This definition is different from an active customer count.

The summary displays two values: Active salesmen and Inactive salesmen.

### By Wilayah Table

1. **KPI Canonical:** Not Found — this is a segmentation table.

2. **Question Answered:**  
How are salesman capacity and activity distributed across Wilayah?

3. **Definition:**  
Salesmen are grouped by Wilayah with Total, Active, and Inactive counts.

4. **Business Meaning:**  
The table helps identify territories with limited active coverage, inactive
capacity, or a different staffing profile from the rest of the business.

5. **How To Interpret:**

- A high Inactive count in one Wilayah may indicate a local staffing or
  territory-management issue.
- A high Active count does not prove good performance; compare it with Omzet,
  achievement, and attention signals.
- Use the table to focus management review, not to assume that Wilayah is the
  cause of a performance problem.

Columns: Wilayah, Total, Active, and Inactive.

### By Segment Table

1. **KPI Canonical:** Not Found — this is a segmentation table.

2. **Question Answered:**  
How does the salesman population divide across configured business segments?

3. **Definition:**  
Each configured segment is shown with Total, Active, and Inactive salesman
counts. The table is shown only when segment data is available.

4. **Business Meaning:**  
It provides another management lens for comparing the size and activity of
different salesman groups.

5. **How To Interpret:**

- Compare Active and Inactive mix between segments.
- A segment with many inactive salesmen may need staffing, target, or
  territory review.
- Always connect the segment mix to actual Omzet and achievement before making
  resource decisions.

Columns: Segment, Total, Active, and Inactive.

---

## 8. Navigation and Investigation

### Sales Links

- Sales Dashboard
- Sales Report

### Piutang Links

- Piutang Dashboard
- Piutang Report

**KPI Canonical:** Not Found — these are navigation and evidence links.

The recommended investigation path is:

1. Identify the attention card or ranking signal.
2. Open the salesman detail drawer for Principal and trend context.
3. Open the Sales Report for target, Omzet, concentration, and dormant signals.
4. Open the Piutang Report for open or overdue exposure signals.
5. Complete operational action in BTR Desktop.

The Piutang Dashboard uses all-open balance semantics, while the Piutang Report
may use its own default period. Management should confirm the period before
reconciling totals.

---

## Recommended Management Reading Sequence

1. Check **Active vs Inactive** to understand the available sales capacity.
2. Check **Missing Target Setup** before judging Below Target.
3. Review **Below Target** and Top 10 Achievement % for plan performance.
4. Review Top 10 Omzet and both concentration percentages for dependency risk.
5. Review High Piutang and High Overdue Exposure for collection ownership.
6. Review Dormant Portfolio for customer-retention risk.
7. Open the salesman detail drawer for Principal Achievement and Trend.
8. Use the appropriate Sales or Piutang Report to verify evidence.
9. Take operational action in BTR Desktop.

## Scope Boundary

SF01 does not measure:

- planned visits;
- actual visits;
- missed or unplanned visits;
- Visit Execution %;
- Effective Call Rate;
- GPS validation;
- cash collection performance; or
- collection achievement.

Those measures belong to the relevant Field Activity and Collection
dashboards.
