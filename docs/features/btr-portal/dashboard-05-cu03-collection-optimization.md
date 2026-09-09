# CU03 — Collection Optimization Dashboard KPI Explanation

## Scope

**CU03 — Collection Optimization** answers:

> Which customers should Finance and Sales contact first today, what action
> should they take, and how much near-term collection impact is involved?

The dashboard turns customer risk and receivable conditions into a daily,
prioritized worklist. It is read-only and provides deterministic business
recommendations; it does not initiate collection activity, change credit
limits, or schedule visits.

**Route:** `/dashboard/collection-optimization`

## Important Reading Rules

- **Actions Today** excludes customers classified as Defer Collection or No
  Action Today.
- **Collection Impact** is a planning amount, not cash already collected. In
  the current design it represents overdue balance plus balance due within
  seven days for actionable customers.
- **Overdue Exposure** is only past-due receivables. It is not the same as
  Total Piutang, which also includes amounts that are not yet due.
- **Recovery vs Billing** is context copied from the Collection dashboard. It
  may be slightly older than the live Collection dashboard.
- **Planning Confidence** reflects how far the month has progressed. It is not
  a rating of employee or team quality.
- One customer may appear in the priority queue and in a specialized queue.
- Collection queues remain Customer-level. Routing a collection action to a
  Salesman or function is operational routing, not Customer account ownership.
- No Principal collection impact is shown.

## 1. Executive Summary

The Executive Summary is a short daily narrative describing the collection
plan, the most important action mix, and the expected impact.

### Business Meaning

This gives management a fast answer before reviewing individual customers:
whether today is mainly an urgent collection day, a preventive reminder day, a
credit-control day, or a sales-recovery day.

### How To Interpret

- Read it together with **Actions Today** and **Collection Impact Total**.
- Use the priority and specialized queues to identify the customers behind the
  narrative.
- Treat the summary as operational guidance, not as a guarantee of collected
  cash.

## 2. Workload KPIs

These cards describe the size and composition of today’s actionable workload.

### 2.1 Actions Today

1. **KPI Canonical:** `1.5.1.1` — Actions Today  
   **Portal catalog:** `CU-KPI-040`

2. **Question Answered:**  
   How many customers require an actionable collection or customer-retention
   response today?

3. **Definition:**  
   The number of customers assigned to an action category other than Defer
   Collection or No Action Today.

4. **Business Meaning:**  
   This is the main morning workload number for Finance, Collection, and Sales.
   It shows whether the team has a manageable contact list and whether the
   optimization process is producing actionable work.

5. **How To Interpret:**

   - A high value means a heavy contact day; prioritize Immediate Collection
     and Management Escalation first.
   - A low value means a lighter actionable set, but confirm that overdue or
     at-risk customers have not been overlooked.
   - A high value with low Planning Confidence should be treated as today’s
     worklist, not as a reliable month-end cash forecast.

### 2.2 Immediate Collection

1. **KPI Canonical:** `1.5.1.2` — Immediate Collection Count  
   **Portal catalog:** `CU-KPI-041`

2. **Question Answered:**  
   How many customers need urgent collection contact because their receivables
   are overdue and severe?

3. **Definition:**  
   The number of customers classified as Immediate Collection.

4. **Business Meaning:**  
   This is the cash-urgent part of today’s work. If it is ignored, overdue
   exposure is unlikely to improve.

5. **How To Interpret:**

   - A high or rising value means many severely overdue customers need contact
     now.
   - A low value does not mean the whole collection workload is low; reminders,
     credit reviews, or sales recovery may still be substantial.
   - A small count with a large Immediate Impact Total means each customer is
     financially significant and should receive senior attention.

### 2.3 Proactive Reminders

1. **KPI Canonical:** `1.5.1.3` — Proactive Reminder Count  
   **Portal catalog:** `CU-KPI-042`

2. **Question Answered:**  
   How many customers should be reminded before their receivables become
   overdue?

3. **Definition:**  
   The number of customers with a current balance, a due date approaching, and
   forecast risk that justifies preventive contact.

4. **Business Meaning:**  
   Reminders protect future cash flow. Working this list can prevent customers
   from moving into the Immediate Collection queue.

5. **How To Interpret:**

   - A high value means there is a large preventive opportunity this week.
   - Compare it with **Due Within 7 Days** to see whether the queue covers the
     near-term receivable exposure.
   - Customers repeatedly moving from Proactive Reminder to Immediate
     Collection may need a stronger credit or relationship decision.

### 2.4 Credit Review

1. **KPI Canonical:** `1.5.1.4` — Credit Review Count  
   **Portal catalog:** `CU-KPI-043`

2. **Question Answered:**  
   How many customers require a credit-limit decision because of current or
   projected credit pressure?

3. **Definition:**  
   The number of customers classified for Credit Review because their
   receivable position has breached or is projected to breach the approved
   credit limit.

4. **Business Meaning:**  
   Credit review prevents new exposure from growing while collection is still
   unresolved. It is a Finance decision, not simply another collector call.

5. **How To Interpret:**

   - A high value means Finance has a significant credit-control workload.
   - A low value while credit-limit warnings are high may indicate that
     accounts need investigation.
   - Decide whether supply should continue, be restricted, or be stopped before
     the next shipment.

### 2.5 Sales Recovery

1. **KPI Canonical:** `1.5.1.5` — Sales Recovery Count  
   **Portal catalog:** `CU-KPI-044`

2. **Question Answered:**  
   How many customers need a Sales recovery visit because purchase decline is
   the dominant concern?

3. **Definition:**  
   The number of customers classified as Sales Recovery Visit.

4. **Business Meaning:**  
   This separates “the customer is not buying” from “the customer is not
   paying.” The distinction ensures that the right owner visits the account.

5. **How To Interpret:**

   - A high value means many customer relationships may be cooling and need
     field recovery.
   - A low value while decline and inactivity signals are high may mean Sales
     follow-up is insufficient.
   - Send Sales to retain or recover these accounts; do not send collectors
     first unless overdue exposure is also material.

### 2.6 Collection Impact

1. **KPI Canonical:** `1.2.3.1` — Collection Impact Total  
   **Portal catalog:** `CU-KPI-046`

2. **Question Answered:**  
   How much near-term receivable value is attached to today’s actionable
   customers?

3. **Definition:**  
   The total collection impact for actionable customers. In V1, the impact for
   a customer is overdue balance plus balance due within seven days.

4. **Business Meaning:**  
   This shows whether the team is spending time on meaningful money rather than
   merely completing a long list of low-value contacts.

5. **How To Interpret:**

   - A high value means today’s prioritized customers carry substantial
     near-term collectible exposure.
   - A low value may mean the receivable book is healthy or that the queue is
     not reaching the largest exposures.
   - Compare it with **Actions Today**. A long queue with small impact may
     require different workload allocation.

## 3. Context KPIs

These cards provide the financial and planning context needed to interpret the
action workload.

### 3.1 Overdue Exposure

1. **KPI Canonical:** `1.2.1.4` — Overdue Exposure  
   **Portal catalog:** `CU-KPI-047`

2. **Question Answered:**  
   How much customer receivable is already past due?

3. **Definition:**  
   The total amount in all overdue aging categories, excluding receivables that
   are not yet due.

4. **Business Meaning:**  
   This is the monetary size of the current collection problem. It tells
   management how much cash is late, rather than how much customers owe in
   total.

5. **How To Interpret:**

   - A high or rising value means more company cash is trapped in late
     receivables.
   - Compare it with **Collection Impact Total** to confirm that the daily
     queue is aimed at the overdue book.
   - Review the named customers and salesmen when exposure is concentrated.

### 3.2 Due Within 7 Days

1. **KPI Canonical:** `1.2.2.1` — Due Within 7 Days  
   **Portal catalog:** `CU-KPI-048`

2. **Question Answered:**  
   How much currently current receivable will become due during the next seven
   days?

3. **Definition:**  
   The total balance of current receivables whose due date falls within the
   next seven days.

4. **Business Meaning:**  
   This is the next collection wave and the amount that can still be influenced
   before it becomes overdue.

5. **How To Interpret:**

   - A high value means reminder work has significant near-term cash leverage.
   - Compare it with **Proactive Reminder Count** to see whether customers
     receiving reminders cover the upcoming exposure.
   - This value is not overdue. Treating it as already late overstates today’s
     problem.

### 3.3 Recovery vs Billing

1. **KPI Canonical:** `1.2.3.3` — Recovery vs Billing % (Context)  
   **Portal catalog:** `CU-KPI-049`

2. **Question Answered:**  
   Is cash recovery keeping pace with new invoiced sales?

3. **Definition:**  
   The Collection dashboard’s Recovery vs Billing percentage, shown as
   context on CU03. It compares current-month collections with current-month
   invoiced sales and is not recalculated on this page.

4. **Business Meaning:**  
   If billing grows faster than recovery, receivable exposure can increase even
   when sales performance looks strong.

5. **How To Interpret:**

   - A high value means collections are keeping pace with new billing.
   - A value below 100% indicates that new invoicing is outrunning recovery
     when current-month billing is positive.
   - A falling value together with rising Overdue Exposure requires stronger
     collection and credit attention.
   - Small differences from the Collection dashboard can result from snapshot
     refresh timing.

### 3.4 Planning Confidence

1. **KPI Canonical:** `1.2.2.3` — Planning Confidence  
   **Portal catalog:** `CU-KPI-050`

2. **Question Answered:**  
   How much confidence should management place in the month’s collection
   planning picture?

3. **Definition:**  
   A time-based indicator:

   - **Low:** five or fewer days have elapsed.
   - **Medium:** six through twenty days have elapsed.
   - **High:** twenty-one or more days have elapsed.

4. **Business Meaning:**  
   The same queue is more informative late in the month than early in the
   month. This indicator tells management how firmly to steer from the current
   numbers.

5. **How To Interpret:**

   - **Low:** use the list for today’s work, but treat impact as directional.
   - **Medium:** use the list for active steering and monitor changes closely.
   - **High:** weak recovery or a large urgent queue deserves firm escalation.
   - Confidence is not a judgment of team quality.

## 4. Supporting Action KPI

### 4.1 Management Escalation

1. **KPI Canonical:** `1.5.1.6` — Management Escalation Count  
   **Portal catalog:** `CU-KPI-045`

2. **Question Answered:**  
   How many customers require same-day leadership review?

3. **Definition:**  
   The number of customers with critical forecast risk that should not remain
   only in the ordinary collection or sales queue.

4. **Business Meaning:**  
   Escalation prevents serious accounts from being hidden inside a long
   operational list. It is the owner or General Manager’s review workload.

5. **How To Interpret:**

   - Any material or strategic customer in this group deserves same-day
     leadership attention.
   - A rising value means ordinary collection activity is not sufficient for
     several accounts.
   - Decide whether to collect aggressively, restrict supply, or exit the
     relationship, then assign Finance and Sales owners.

**Display note:** This KPI is represented by the **Management Escalation**
specialized queue rather than a separate visible KPI card in the current CU03
layout.

### 4.2 Immediate Impact Total

1. **KPI Canonical:** `1.2.3.4` — Immediate Impact Total  
   **Portal catalog:** `CU-KPI-051`

2. **Question Answered:**  
   How much impact is attached to the most urgent collection actions?

3. **Definition:**  
   The collection impact attached to Immediate Collection and Priority
   Follow-up actions.

4. **Business Meaning:**  
   It separates the urgent cash slice from softer actions such as reminders,
   credit review, and sales recovery.

5. **How To Interpret:**

   - A high value means the day’s collection effort should start with the
     urgent queue.
   - A small customer count with high immediate impact means each contact is
     financially significant.
   - A high total Collection Impact with low Immediate Impact means more of the
     opportunity is preventive or relationship-oriented.

**Display note:** This is a supporting catalog KPI and is not a separate visible
KPI card in the current CU03 layout.

## 5. Charts

### 5.1 Actions by Category

**Chart type:** Doughnut chart

**Content:** The customer-count mix across collection action categories, such
as Immediate Collection, Proactive Reminder, Credit Review, Sales Recovery,
and Management Escalation.

**Business question:** What kind of work makes up today’s actionable workload?

**How to use it:**

- A large Immediate Collection segment indicates cash urgency.
- A large Proactive Reminder segment indicates an opportunity to prevent new
  overdue debt.
- A large Sales Recovery segment indicates relationship or demand weakness,
  rather than only payment delay.
- Compare the mix with Collection Impact; the largest customer-count segment
  is not necessarily the largest financial exposure.

### 5.2 Workload

**Chart type:** Horizontal bar chart

**Available views:** Wilayah and Routed Salesman

**Content:** The number of collection actions grouped by selected Wilayah or
by the Salesman used as an operational route. The Salesman view is not a
Customer-ownership book.

**Business question:** Where is today’s operational workload concentrated?

**How to use it:**

- A high Wilayah value may require regional coordination or capacity balancing.
- A high Routed Salesman value indicates operational concentration of today’s
  collection follow-ups. It does not mean that Salesman owns those Customer
  accounts.
- Compare workload counts with impact and overdue exposure; equal action counts
  do not imply equal financial responsibility.

### 5.3 Impact by Action Category

**Chart type:** Bar chart

**Content:** Collection impact amount in IDR grouped by action category.

**Business question:** Which type of action carries the greatest financial
opportunity or risk?

**How to use it:**

- A high Immediate Collection impact indicates urgent cash recovery work.
- A high Proactive Reminder impact indicates that preventive contact can
  protect substantial near-term cash.
- A high Sales Recovery impact indicates that relationship recovery may affect
  future revenue as well as collections.
- Use this chart to allocate management attention, not to treat impact as
  money already received.

## 6. Tables

### 6.1 Today’s Collection Priorities

**Purpose:** The main prioritized customer worklist for today.

**Capacity:** Up to 30 customers, ordered by collection priority.

**Visible content:**

- Rank
- Customer
- Action
- Priority score
- Impact
- Risk category
- Action Route

**Expandable explanation:**

- Why the customer was selected
- Why the customer received its priority
- Why the recommended action applies
- Triggered business rules

**How to use it:**

1. Start with the highest-priority customers.
2. Review Impact alongside Priority so high-value work is not overlooked.
3. Confirm the Action Route and send the work to Collection, Finance, Sales,
   or Management. Action Route is operational routing, not Customer ownership.
4. Expand the row when the team needs to understand or challenge the
   recommendation.

The priority score is a sorting aid, not a probability of payment.

### 6.2 Specialized Queues

**Purpose:** Separate the priority work into operational queues.

**Available tabs:**

- Proactive Reminders
- Credit Review
- Sales Recovery
- Management Escalation

**Capacity:** Up to 15 customers per queue.

**Visible content:**

- Customer
- Action
- Impact
- Risk
- Action Route
- Reason

**How to use it:**

- Use **Proactive Reminders** for upcoming due balances and prevention.
- Use **Credit Review** for credit-limit decisions.
- Use **Sales Recovery** for declining customer relationships.
- Use **Management Escalation** for critical cases requiring leadership review.

These queues remain Customer-level. They help each function work the same
customer list according to the operational route, not as owned accounts.

### 6.3 Top Impact Opportunities

**Purpose:** Identify the customers with the largest collection impact amount,
regardless of whether the recommended action is urgent, preventive, or
relationship-oriented.

**Capacity:** Up to 15 customers.

**Visible content:**

- Customer
- Impact
- Action
- Overdue
- Due within 7 days
- Routed Salesman
- Wilayah

**How to use it:**

- Use it to focus management attention on the largest financial opportunities.
- Compare Overdue with Due within 7 days to distinguish late cash from
  upcoming cash.
- Use Routed Salesman and Wilayah to coordinate operational follow-up. The
  Routed Salesman is not the Customer owner.

## 7. Recommended Management Reading Order

1. Read the Executive Summary.
2. Check **Actions Today**, **Collection Impact**, and **Planning Confidence**.
3. Review **Overdue Exposure**, **Due Within 7 Days**, and **Recovery vs
   Billing** for financial context.
4. Use **Actions by Category** and **Impact by Action Category** to understand
   the workload mix.
5. Open **Today’s Collection Priorities** and expand explanations for unusual
   or high-impact customers.
6. Work the appropriate Specialized Queue.
7. Use **Top Impact Opportunities** to confirm that the largest opportunities
   have an operational route. That route is not Customer ownership.

## 8. Footer and Traceability

CU03 provides links to:

- Customer Risk Forecast
- Collection
- Customer Analytics
- Piutang
- Piutang Report
- Sales Report

Recommendations are indicative operational guidance based on deterministic
business rules. BTR Portal does not initiate collection contact, modify credit
limits, or schedule visits. Operational actions remain with the responsible
teams and BTR Desktop or field operations.

## Related Knowledge

- [Collection Optimization feature](../collection-optimization/feature.md)
- [BTR Portal operational guide](./btr-portal-operational.md)
- [BTR Portal KPI catalog](./btr-portal-kpi-catalog.md)
- [KPI Encyclopedia](./kpi-encyclopedia.html)
