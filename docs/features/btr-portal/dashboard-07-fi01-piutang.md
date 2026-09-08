# FI01 — Piutang Dashboard KPI Explanation

## Scope

**FI01 — Piutang** answers:

> How much do customers currently owe, how much is already overdue, how old
> is the receivable book, and which customers hold the greatest exposure?

**Route:** `/dashboard/piutang`

This dashboard is a read-only management view of all open customer receivables.
It is intended for the Owner, Director, General Manager, Finance, and Collection
managers. Monetary values are in IDR.

## Important Reading Rules

- **Total Piutang** includes balances that are not yet due and balances that are
  already overdue.
- **Overdue Piutang** excludes the Current bucket and represents the monetary
  size of the late-payment problem.
- **Piutang > 90 Hari** is the chronic-overdue portion of the receivable book.
- The dashboard is an all-time open-balance snapshot, not a current-month sales
  report.
- A high Total Piutang is not automatically a collection failure. Some of the
  balance may still be within agreed payment terms.
- The dashboard supports management decisions; operational credit, collection,
  and customer actions remain the responsibility of the relevant teams.

## 1. Header and Dashboard Purpose

The header identifies the page as **Piutang Dashboard** and states that it is a
portfolio-quality snapshot of all open receivables. It also provides the
generation time so management can judge how current the snapshot is.

The page intentionally separates receivable exposure from collection recovery.
Questions about cash collected, recovery pace, and collection execution belong
to the Collection Dashboard.

## 2. Receivable KPI Summary

These five KPIs provide the first management scan of the receivable portfolio.

### 2.1 Total Piutang

1. **KPI Canonical:** `1.2.1.1` — Total Piutang  
   **Portal catalog:** `FI-KPI-001`

2. **Question Answered:**  
   How much money is currently outstanding from all customers?

3. **Definition:**  
   The total amount customers owe on open receivables, including both Current
   balances and overdue balances.

4. **Business Meaning:**  
   This is the primary Finance exposure number. It shows how much company
   working capital is temporarily held by customers and provides the
   denominator for the concentration and aging measures.

5. **How To Interpret:**

   - A high value means more working capital is tied up in customer debt.
   - A rising value may indicate healthy sales growth, slower collection, or
     both. It must be read together with overdue KPIs.
   - A falling value may indicate effective collection, lower sales activity,
     or both.
   - Do not restrict sales based on this KPI alone when the balance is current
     and within approved terms.
   - Check **Overdue Piutang**, **Piutang > 90 Hari**, and the Top 20 table to
     understand the quality and concentration of the total.

### 2.2 Total Customer

1. **KPI Canonical:** Not Found in the KPI Encyclopedia — the portal catalog
   defines this as `FI-KPI-002` — Total Customer (with balance).

2. **Question Answered:**  
   How many customers currently owe money?

3. **Definition:**  
   The number of distinct customers carrying an open receivable balance.

4. **Business Meaning:**  
   This measures the breadth of the collection and credit workload. The same
   Total Piutang requires a different management response when it is spread
   across many customers instead of concentrated in a few large accounts.

5. **How To Interpret:**

   - A high value means Finance and Collection are managing many debtor
     relationships.
   - A low value with high Total Piutang indicates concentrated exposure in a
     smaller number of customers.
   - A high value with modest Total Piutang may indicate broad administrative
     workload but limited exposure per customer.
   - Compare this KPI with Total Piutang and the Top 20 customer list before
     deciding how to allocate collection capacity.

### 2.3 Overdue Customer

1. **KPI Canonical:** `1.2.1.2` — Overdue Customer Count  
   **Portal catalog:** `FI-KPI-003` — Overdue Customer

2. **Question Answered:**  
   How many customers have at least some receivable that is past due?

3. **Definition:**  
   The number of distinct customers with a balance in any overdue aging bucket.
   Customers whose balances are entirely Current are excluded.

4. **Business Meaning:**  
   This measures the breadth of the collection problem rather than its
   financial size. It tells management how many customer conversations may be
   required.

5. **How To Interpret:**

   - A high value means collection effort must cover many accounts.
   - A low value does not necessarily mean low risk; a few customers may still
     hold a very large overdue amount.
   - A rising count with stable Overdue Piutang suggests more customers are
     becoming late, even if the total late amount has not yet increased.
   - Compare it with Overdue Piutang and the Top 20 table to distinguish broad
     workload from concentrated financial exposure.

### 2.4 Overdue Piutang

1. **KPI Canonical:** `1.2.1.4` — Overdue Exposure  
   **Portal catalog:** `FI-KPI-013` — Overdue Exposure (shared Finance/Collection
   concept; displayed as Overdue Piutang on FI01)

2. **Question Answered:**  
   How much customer money is already late?

3. **Definition:**  
   The sum of all receivables in the 1–30, 31–60, 61–90, and >90-day buckets.
   Current, not-yet-due balances are excluded.

   ```text
   Overdue Piutang =
     1–30 Days + 31–60 Days + 61–90 Days + >90 Days
   ```

4. **Business Meaning:**  
   This is the monetary size of the current collection problem. It is more
   useful than Total Piutang when management needs to understand how much cash
   is late rather than how much customers owe in total.

5. **How To Interpret:**

   - A high or rising value means more company cash is trapped in late
     receivables.
   - A low value means most outstanding balances remain within terms, although
     customer concentration and future due dates may still require attention.
   - If Overdue Piutang rises while Total Piutang is stable, receivable quality
     is deteriorating inside the same portfolio size.
   - Review the aging chart and customer names to determine whether the problem
     is early delay, persistent delay, or chronic debt.

### 2.5 Piutang > 90 Hari

1. **KPI Canonical:** `1.2.1.3` — Piutang > 90 Hari  
   **Portal catalog:** `FI-KPI-011` — Piutang > 90 Hari (Amount & %)

2. **Question Answered:**  
   How much receivable has become chronically overdue?

3. **Definition:**  
   The amount in the >90 Days aging bucket. FI01 also displays its percentage
   of Total Piutang:

   ```text
   Piutang > 90 Hari % =
     Piutang > 90 Hari ÷ Total Piutang × 100%
   ```

4. **Business Meaning:**  
   Debt older than 90 days is a serious capital-recovery and relationship
   decision issue. Routine reminders may no longer be sufficient, and the
   account may require senior collection, credit, legal, restructuring, or
   exit review.

5. **How To Interpret:**

   - A high or rising amount indicates that receivables are aging into chronic
     risk.
   - A rising percentage is a warning even when Total Piutang is unchanged.
   - A low value suggests that collection is resolving problems before they
     become chronic.
   - Identify the customers behind the amount in the Top 20 table and confirm
     their collection and credit plan.
   - This measures days past due, not simply the age of an invoice that is
     still within its payment terms.

### Reading the Summary KPIs Together

Read the summary in this order:

1. **Total Piutang** establishes the size of the outstanding book.
2. **Total Customer** shows how widely the exposure is distributed.
3. **Overdue Customer** shows how many relationships require collection
   attention.
4. **Overdue Piutang** shows the monetary size of the late-payment problem.
5. **Piutang > 90 Hari** shows how much of the problem has become chronic.

For example, a high Total Piutang with low Overdue Piutang may represent a
large but generally current customer book. A stable Total Piutang with rising
Overdue Piutang and Piutang > 90 Hari indicates worsening receivable quality.

## 3. Receivable Concentration

This section shows how much of Total Piutang is held by the largest customers.
It helps management judge whether a payment problem at a few accounts could
become a company-wide cash problem.

### 3.1 Top 10 Customer %

1. **KPI Canonical:** `1.3.2.2` — Top Customer % (Piutang Concentration)  
   **Portal catalog:** `FI-KPI-004` — Top 10 Customer %

2. **Question Answered:**  
   What percentage of Total Piutang is held by the ten largest customers?

3. **Definition:**  
   The combined outstanding balance of the ten largest customers divided by
   Total Piutang:

   ```text
   Top 10 Customer % =
     Top 10 customer balances ÷ Total Piutang × 100%
   ```

4. **Business Meaning:**  
   This shows dependency on a small group of debtor relationships. A high share
   means the payment behaviour of a few customers can materially affect
   company liquidity.

5. **How To Interpret:**

   - A high percentage means receivable exposure is concentrated.
   - A low percentage means the exposure is spread across more customers.
   - A high percentage is more concerning when the same customers are also
     overdue or appear in the >90-day bucket.
   - Use the Top 20 table to identify the names and review their credit,
     collection ownership, and supply status.
   - This is an informational risk indicator; the KPI Encyclopedia does not
     define an automatic numeric threshold.

### 3.2 Top 20 Customer %

1. **KPI Canonical:** Not Found as a dedicated Top 20 entry in the KPI
   Encyclopedia.  
   **Portal catalog:** `FI-KPI-005` — Top 20 Customer %

2. **Question Answered:**  
   What percentage of Total Piutang is held by the twenty largest customers?

3. **Definition:**  
   The combined outstanding balance of the twenty largest customers divided by
   Total Piutang:

   ```text
   Top 20 Customer % =
     Top 20 customer balances ÷ Total Piutang × 100%
   ```

4. **Business Meaning:**  
   This is a wider view of concentration than Top 10 Customer %. It shows
   whether receivable risk is concentrated in a broader group of important
   accounts rather than only in the very largest ten.

5. **How To Interpret:**

   - A high percentage means a substantial part of company working capital is
     dependent on a relatively small customer group.
   - Compare Top 10 Customer % with Top 20 Customer %. A small difference
     indicates that the largest ten dominate the exposure; a larger difference
     indicates that the next ten customers also materially affect risk.
   - Rising concentration together with rising overdue amounts is a stronger
     warning than concentration alone.
   - Review the customer aging breakdown before changing credit or supply
     policy.

## 4. Chart — Aging Distribution

**Chart type:** Pie chart

**KPI Canonical:** Not Found as a single chart KPI. The chart is composed of
the five aging-bucket measures listed below.

**Question Answered:**  
How is Total Piutang distributed between current and overdue age brackets?

**Business Meaning:**  
The chart shows the quality and maturity of the receivable book at a glance.
It helps management distinguish a portfolio that is mostly within terms from
one that is moving toward persistent or chronic overdue status.

The five buckets should reconcile to Total Piutang:

```text
Current + 1–30 + 31–60 + 61–90 + >90 = Total Piutang
```

### 4.1 Current — Not Yet Due

1. **KPI Canonical:** Not Found as a dedicated Encyclopedia entry.  
   **Portal catalog:** `FI-KPI-006` — Aging Bucket: Current

2. **Question Answered:**  
   How much receivable has not reached its due date?

3. **Definition:**  
   The outstanding amount whose payment due date has not yet passed.

4. **Business Meaning:**  
   Current receivable is normally part of the expected operating cycle. It is
   not automatically a collection failure, although customer payment behaviour
   and credit limits still matter.

5. **How To Interpret:**

   - A large Current segment generally indicates that much of the book remains
     within agreed terms.
   - A small Current segment means more of the book has moved into overdue
     categories.
   - Current balances should not be treated as cash already collected.
   - Compare this segment with Overdue Piutang to understand the balance between
     normal terms and collection urgency.

### 4.2 1–30 Days

1. **KPI Canonical:** Not Found as a dedicated Encyclopedia entry.  
   **Portal catalog:** `FI-KPI-007` — Aging Bucket: 1–30 Days

2. **Question Answered:**  
   How much receivable has recently become overdue?

3. **Definition:**  
   The outstanding amount that is one to thirty days past its due date.

4. **Business Meaning:**  
   This is the early-overdue portion of the collection workload. It is often
   the best opportunity to correct payment delay before the balance becomes
   difficult to recover.

5. **How To Interpret:**

   - A high or rising segment indicates that new balances are entering overdue
     status.
   - Review payment commitments and collection follow-up promptly.
   - If 1–30 day balances repeatedly move into older buckets, the collection
     process or customer credit terms may need review.

### 4.3 31–60 Days

1. **KPI Canonical:** Not Found as a dedicated Encyclopedia entry.  
   **Portal catalog:** `FI-KPI-008` — Aging Bucket: 31–60 Days

2. **Question Answered:**  
   How much receivable has remained overdue beyond the initial delay period?

3. **Definition:**  
   The outstanding amount that is thirty-one to sixty days past its due date.

4. **Business Meaning:**  
   This indicates more persistent payment delay. The account may require
   stronger follow-up, clearer ownership, or a credit decision rather than
   routine reminders alone.

5. **How To Interpret:**

   - A high value means overdue balances are not being resolved quickly.
   - A rising value suggests early-overdue debt is aging without successful
     recovery.
   - Check whether the affected customers are strategically important,
     concentrated, or still receiving new supply.

### 4.4 61–90 Days

1. **KPI Canonical:** Not Found as a dedicated Encyclopedia entry.  
   **Portal catalog:** `FI-KPI-009` — Aging Bucket: 61–90 Days

2. **Question Answered:**  
   How much receivable is approaching chronic-overdue status?

3. **Definition:**  
   The outstanding amount that is sixty-one to ninety days past its due date.

4. **Business Meaning:**  
   This is a severe overdue warning. Without effective intervention, these
   balances will move into the >90-day category and carry greater recovery and
   bad-debt risk.

5. **How To Interpret:**

   - A high or rising value requires named-account review.
   - Confirm the collection plan, customer commitment, credit exposure, and
     decision owner.
   - Read this segment together with Piutang > 90 Hari to determine whether
     the aging problem is worsening or being resolved.

### 4.5 > 90 Days

1. **KPI Canonical:** Not Found as a dedicated Encyclopedia entry; the related
   canonical KPI is `1.2.1.3` — Piutang > 90 Hari.  
   **Portal catalog:** `FI-KPI-010` — Aging Bucket: >90 Days

2. **Question Answered:**  
   How much receivable has become chronically overdue?

3. **Definition:**  
   The outstanding amount more than ninety days past its due date.

4. **Business Meaning:**  
   This is the most severe aging category and the financial basis of the
   Piutang > 90 Hari KPI. It identifies capital that may require escalation,
   restructuring, legal action, write-off review, or a decision to stop
   expanding exposure.

5. **How To Interpret:**

   - A large segment means serious cash is stuck in very late debt.
   - A rising segment indicates that collection is not resolving older debt
     quickly enough.
   - Identify the customers behind the amount and assign an explicit
     management decision.
   - Do not interpret this as proof that every customer in the segment will
     default; it is a severe risk signal requiring investigation.

## 5. Table — Top 20 Outstanding Customers: Aging Breakdown

**Table purpose:** Identify the largest customer receivable exposures and show
how each customer’s balance is distributed across aging categories.

**KPI Canonical:** Not Found for the current Top 20 version in the KPI
Encyclopedia. The closest canonical entries are:

- `1.2.1.10` — Top 10 Outstanding Customers
- `1.3.2.4` — Top 10 Piutang Ranking

The portal catalog contains `FI-KPI-012` — Top 10 Outstanding Customers, while
the current FI01 table is the broader **Top 20** risk table.

### Visible Content

Each row shows:

- Customer
- Total outstanding balance
- Current balance
- 1–30-day balance
- 31–60-day balance
- 61–90-day balance
- >90-day balance

The table is ordered by total outstanding balance and contains up to twenty
customers. The current display does not show Rank or Customer Code as separate
columns.

### Business Question

Which customers hold the largest receivable exposure, and how much of each
customer’s balance is already overdue?

### Business Meaning

Totals tell management the size of the problem; this table provides the names
behind the exposure. The aging breakdown prevents management from treating a
large current account and a smaller chronic debtor as the same type of risk.

### How To Use The Table

1. Start with the largest total balances.
2. Separate customers whose balances are mostly Current from customers whose
   balances are mostly overdue.
3. Give immediate attention to customers with material 61–90-day or >90-day
   balances.
4. Compare the table with Top 10 and Top 20 Customer % to understand
   concentration.
5. Confirm the collection owner, salesman responsibility, credit position, and
   supply decision for important or chronically overdue customers.
6. Use the available row investigation action when more detail is required.

### Warning Signs

- The same customers remain at the top while their balances continue growing.
- A customer appears in the Top 20 table and has a large >90-day balance.
- A customer has a large Current balance but also shows overdue exposure,
  indicating that new credit may be growing while old debt remains unpaid.
- A small number of customers account for most of Total Piutang.

The table is a prioritization aid, not a probability-of-default score. A large
outstanding balance is not necessarily overdue, and a smaller customer may
still require urgent action if its aging is severe.

## 6. Recommended Management Reading Order

1. Confirm the snapshot generation time and all-open scope.
2. Read Total Piutang and Total Customer to understand scale and breadth.
3. Compare Overdue Customer with Overdue Piutang to distinguish the number of
   affected accounts from the money at risk.
4. Check Piutang > 90 Hari for chronic exposure.
5. Review Top 10 Customer % and Top 20 Customer % for concentration.
6. Use Aging Distribution to see where balances sit in the aging ladder.
7. Use the Top 20 table to identify names, owners, and required decisions.
8. Validate transaction-level evidence in the Piutang Report before changing
   credit, supply, or customer status.

## Related Dashboards

- **FI02 — Collection:** collection recovery, overdue exposure, payment mix,
  and collection priorities.
- **FI04 — Piutang Report:** open-receivable evidence and customer/invoice
  detail.
- **CU01 — Customer Analytics:** customer activity, credit, and portfolio
  context.
- **CU02 — Customer Risk Forecast:** forward-looking customer risk signals.
- **CU03 — Collection Optimization:** prioritized collection and customer
  actions.

