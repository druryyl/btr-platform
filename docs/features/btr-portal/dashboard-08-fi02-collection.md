# FI02 — Collection Dashboard KPI Explanation

## Scope

**FI02 — Collection** answers:

> Is customer debt converting to cash, how large is the overdue problem, how
> old is the overdue balance, and where is collection effort most needed?

**Route:** `/dashboard/collection`

This dashboard is a read-only management view for the Owner, Director, General
Manager, Finance, Collection, and Sales managers. Monetary values are in IDR.

## Important Reading Rules

- **Overdue Exposure** includes only receivables that are past due. It excludes
  Current, not-yet-due balances.
- **Recovery vs Billing %** compares collections with newly invoiced sales. It
  does not mean that all open receivables have been collected.
- **>90d Exposure** is the chronic portion of overdue debt and requires more
  escalation than recently overdue balances.
- The dashboard uses current-month collection measures together with an
  all-open overdue exposure snapshot.
- Rankings identify where overdue money is concentrated. They do not measure
  employee performance or probability of payment.
- The dashboard supports management decisions; payment collection, credit
  decisions, and customer actions remain the responsibility of the relevant
  teams.

## 1. Collection Attention Cards

The attention cards provide the first scan of collection urgency. They combine
the size of overdue debt, the effectiveness of current-month recovery, and the
number of inactive customers that still owe money.

### 1.1 Exposure Card

The Exposure card answers:

> How much customer money is late, how much of it is chronic, and is the
> overdue problem concentrated in a few customers?

#### 1.1.1 Overdue Exposure

1. **KPI Canonical:** `1.2.1.4` — Overdue Exposure  
   **Portal catalog:** `FI-KPI-013`

2. **Question Answered:**  
   How much customer receivable is already past due?

3. **Definition:**  
   The total balance in all overdue aging buckets:

   ```text
   Overdue Exposure =
     1–30 Days + 31–60 Days + 61–90 Days + >90 Days
   ```

   Current, not-yet-due balances are excluded.

4. **Business Meaning:**  
   This is the monetary size of the current collection problem. It tells
   management how much company cash is late and should be prioritized for
   recovery.

5. **How To Interpret:**

   - A high value means more working capital is trapped in late receivables.
   - A rising value means overdue debt is growing or is not being recovered
     quickly enough.
   - A low value is positive, but it does not remove the need to monitor
     concentration or balances that are about to become due.
   - Compare it with **Cash Collected MTD** and **Recovery vs Billing %** to
     distinguish the size of the problem from the recovery pace.
   - Use **Top 10 Overdue Customers** to identify the customers behind the
     amount.

#### 1.1.2 >90d Exposure

1. **KPI Canonical:** `1.2.1.5` — >90d Exposure  
   **Portal catalog:** `FI-KPI-014`

2. **Question Answered:**  
   How much overdue debt has become chronically late?

3. **Definition:**  
   The outstanding amount in the >90 Days overdue bucket. It is a subset of
   Overdue Exposure.

4. **Business Meaning:**  
   This is the most severe overdue exposure in monetary terms. It may require
   senior collection, credit restriction, restructuring, legal review, or
   write-off assessment.

5. **How To Interpret:**

   - A high value means a material amount of cash is tied up in very old debt.
   - A rising value means overdue balances are moving deeper into chronic risk.
   - A low value suggests that collection is resolving debt before it becomes
     severely aged.
   - Compare it with the **Aging Risk Summary** and **Legacy Debt Count**.
   - Identify the responsible customers and salesmen before deciding whether to
     continue supplying those accounts.

#### 1.1.3 Overdue Concentration %

1. **KPI Canonical:** `1.2.1.6` — Overdue Concentration %  
   **Portal catalog:** `FI-KPI-015`

2. **Question Answered:**  
   What share of total overdue debt is held by the largest overdue customer?

3. **Definition:**  

   ```text
   Overdue Concentration % =
     Largest customer overdue balance ÷ Total Overdue Exposure × 100%
   ```

4. **Business Meaning:**  
   This shows whether the collection problem is broad or dominated by one
   customer. A concentrated problem can be reduced by resolving a small number
   of important accounts, but it also creates dependency on those accounts’
   payment decisions.

5. **How To Interpret:**

   - A high percentage means one customer can materially affect company cash.
   - A rising percentage means overdue risk is becoming more concentrated.
   - A low percentage means the overdue problem is spread more broadly and may
     require collection capacity across many accounts.
   - A high percentage is more urgent when the same customer also has >90-day
     debt or appears in the Collection Attention List.
   - Review **Top 10 Overdue Customers** to name the account and assign an
     owner.

### 1.2 Recovery Card

The Recovery card answers:

> Is cash being recovered quickly enough compared with new billing?

#### 1.2.1 Cash Collected MTD

1. **KPI Canonical:** Not Found in the KPI Encyclopedia  
   **Portal catalog:** `FI-KPI-016`

2. **Question Answered:**  
   How much cash has been received from customer settlements this month?

3. **Definition:**  
   The total customer cash settlement received from the beginning of the
   current calendar month through the dashboard snapshot date.

4. **Business Meaning:**  
   This is the actual cash recovery result for the month. It is the liquidity
   available from collection activity, not merely a promise to pay or a
   reduction in an account balance.

5. **How To Interpret:**

   - A high value indicates stronger realized cash recovery, but it must be
     compared with new billing and the remaining target.
   - A low value may indicate weak collection, a young month, low billing, or
     settlements recorded through other methods.
   - A rising daily pace is encouraging; a flat pace while billing continues
     means net receivable pressure may be increasing.
   - Read it together with **Recovery vs Billing %** and **Overdue Exposure**.

#### 1.2.2 Recovery vs Billing %

1. **KPI Canonical:** `1.2.3.2` — Recovery vs Billing %  
   **Portal catalog:** `FI-KPI-017`

2. **Question Answered:**  
   Is customer cash recovery keeping up with newly invoiced sales?

3. **Definition:**  

   ```text
   Recovery vs Billing % =
     Customer settlements MTD ÷ New invoiced sales MTD × 100%
   ```

   The value is unavailable when current-month invoiced sales are zero.

4. **Business Meaning:**  
   This indicates whether the working-capital cycle is keeping pace. When
   billing grows faster than recovery, the receivable book can expand even
   while collection activity continues.

5. **How To Interpret:**

   - Around or above 100% means recovery is keeping pace with current-month
     billing, subject to the age and quality of the receivable book.
   - Below 100% means new billing is exceeding current-month recovery and
     requires attention when invoiced sales are positive.
   - A falling percentage together with rising Overdue Exposure is a stronger
     warning than either measure alone.
   - A high percentage does not prove that old debt is being recovered; check
     **>90d Exposure**, **Legacy Debt Count**, and the aging chart.
   - Use the result to decide whether to intensify collection, review credit,
     or slow additional exposure.

### 1.3 Portfolio Card

#### 1.3.1 Legacy Debt Count

1. **KPI Canonical:** `1.2.2.2` — Legacy Debt Count  
   **Portal catalog:** `FI-KPI-021`

2. **Question Answered:**  
   How many inactive customers still carry an open receivable balance?

3. **Definition:**  
   The number of customers that have had no Faktur for at least 90 days and
   still have an open balance greater than 1.

4. **Business Meaning:**  
   Legacy debt combines customer inactivity with unpaid receivables. These
   accounts may have a lower likelihood of naturally generating new business
   that could support repayment and therefore need explicit recovery or
   closure decisions.

5. **How To Interpret:**

   - A high value means collection includes inactive accounts, not only active
     customers with temporary payment delays.
   - A rising value means more receivables are becoming detached from current
     sales activity.
   - A low value does not mean overdue risk is low; active customers can still
     carry substantial late balances.
   - Compare it with **>90d Exposure**, **Top 10 Overdue Customers**, and
     customer dormancy information.
   - Decide whether each important account needs recovery, escalation,
     restructuring, or write-off review.

## 2. Recovery Summary

The Recovery Summary repeats the two primary recovery KPIs and adds the
composition of settlements by payment method. It answers:

> How much has been recovered this month, and what form did the recovery take?

### 2.1 Recovery Summary KPIs

The **Cash Collected MTD** and **Recovery vs Billing %** KPIs have the same
definitions and interpretations described in Sections 1.2.1 and 1.2.2.

### 2.2 Payment Mix

Payment Mix is displayed as a segmented bar and legend. It explains the
composition of current-month settlement activity; it is not an additional
measure of total recovery.

#### 2.2.1 Payment Mix — Cash

1. **KPI Canonical:** Not Found in the KPI Encyclopedia  
   **Portal catalog:** `FI-KPI-018` — Payment Mix — Cash

2. **Question Answered:**  
   What share of this month’s settlement total came from cash?

3. **Definition:**  

   ```text
   Cash Mix % =
     Cash settlement amount ÷ Total settlement amount × 100%
   ```

   Total settlement consists of Cash, Giro, and Adjustment.

4. **Business Meaning:**  
   Cash settlements represent the most direct liquidity contribution from
   collection activity. The mix helps Finance understand how recovered value
   enters the business.

5. **How To Interpret:**

   - A high share indicates that more settlement value is being received as
     cash.
   - A low share is not automatically negative; Giro and valid adjustments may
     be appropriate for the customer or transaction.
   - Read this percentage with total recovery value. A high mix share on a
     small recovery total may still produce little cash.

#### 2.2.2 Payment Mix — Giro

1. **KPI Canonical:** Not Found in the KPI Encyclopedia  
   **Portal catalog:** `FI-KPI-019` — Payment Mix — Giro

2. **Question Answered:**  
   What share of this month’s settlement total came from Giro?

3. **Definition:**  

   ```text
   Giro Mix % =
     Giro settlement amount ÷ Total settlement amount × 100%
   ```

4. **Business Meaning:**  
   Giro shows the portion of settlement value received through giro
   instruments. It informs Finance about the form and timing characteristics
   of recovery, rather than simply treating every settlement as immediate cash.

5. **How To Interpret:**

   - A high share means recovery relies substantially on Giro settlements.
   - Consider the timing and certainty of Giro realization when judging
     liquidity.
   - Compare the mix with Cash Collected MTD and the total settlement amount.
   - Do not treat a high Giro share as collection failure without checking the
     business’s accepted payment practice.

#### 2.2.3 Payment Mix — Adjustment

1. **KPI Canonical:** Not Found in the KPI Encyclopedia  
   **Portal catalog:** `FI-KPI-020` — Payment Mix — Adjustment

2. **Question Answered:**  
   What share of this month’s settlement total came from adjustments?

3. **Definition:**  

   ```text
   Adjustment Mix % =
     Adjustment amount ÷ Total settlement amount × 100%
   ```

4. **Business Meaning:**  
   Adjustments explain the part of settlement activity that does not represent
   direct Cash or Giro receipt. A material share requires Finance to understand
   the reason for the adjustment and its effect on real liquidity.

5. **How To Interpret:**

   - A high share means the recovery result contains a significant
     non-Cash/non-Giro component.
   - A low share means the settlement mix is primarily Cash or Giro.
   - Review material adjustments to ensure that reported recovery is not being
     mistaken for cash available to fund operations.
   - The three payment-mix percentages should sum to approximately 100% when
     settlement activity exists.

## 3. Chart — Aging Risk Summary (Overdue Only)

**Chart type:** Pie chart

**KPI Canonical:** Not Found as a single KPI. The chart is composed of the
four portal aging KPIs below.

**Question Answered:**  
Where is the overdue balance concentrated in the aging ladder?

**Business Meaning:**  
The chart separates new overdue debt from persistent and chronic overdue debt.
It helps management decide whether ordinary follow-up is sufficient or whether
named-account escalation is required.

The four buckets reconcile to **Overdue Exposure**:

```text
1–30 + 31–60 + 61–90 + >90 Days = Overdue Exposure
```

### 3.1 Aging Risk Summary — 1–30 Days

1. **KPI Canonical:** Not Found in the KPI Encyclopedia  
   **Portal catalog:** `FI-KPI-022` — Aging Risk Summary — 1–30 Days

2. **Question Answered:**  
   How much debt has recently become overdue?

3. **Definition:**  
   The overdue balance that is one to thirty days past its due date. Current
   balances are excluded.

4. **Business Meaning:**  
   This is the earliest overdue stage and often the best opportunity to correct
   payment delay before it becomes persistent.

5. **How To Interpret:**

   - A high or rising segment means new debt is entering overdue status.
   - A low segment is positive only when older buckets are also controlled.
   - Contact customers promptly and confirm payment commitments.
   - Watch whether this balance repeatedly moves into older buckets.

### 3.2 Aging Risk Summary — 31–60 Days

1. **KPI Canonical:** Not Found in the KPI Encyclopedia  
   **Portal catalog:** `FI-KPI-023` — Aging Risk Summary — 31–60 Days

2. **Question Answered:**  
   How much overdue debt has remained unresolved beyond the initial delay?

3. **Definition:**  
   The overdue balance that is thirty-one to sixty days past its due date.

4. **Business Meaning:**  
   This represents more persistent payment delay. Routine reminders may no
   longer be enough; ownership and credit decisions may be needed.

5. **How To Interpret:**

   - A high value means early overdue debt is not being resolved quickly.
   - A rising value indicates that balances are aging without successful
     recovery.
   - Identify the affected customers and salesmen.
   - Check whether new sales or supply are continuing while older debt remains
     unpaid.

### 3.3 Aging Risk Summary — 61–90 Days

1. **KPI Canonical:** Not Found in the KPI Encyclopedia  
   **Portal catalog:** `FI-KPI-024` — Aging Risk Summary — 61–90 Days

2. **Question Answered:**  
   How much overdue debt is approaching chronic status?

3. **Definition:**  
   The overdue balance that is sixty-one to ninety days past its due date.

4. **Business Meaning:**  
   This is a severe warning stage. Without intervention, the balance will move
   into >90-day exposure with greater recovery and bad-debt risk.

5. **How To Interpret:**

   - A high or rising segment requires named-account review.
   - Confirm the payment plan, collection owner, credit position, and supply
     decision.
   - Read it with **>90d Exposure** to see whether chronic debt is already
     material.

### 3.4 Aging Risk Summary — >90 Days

1. **KPI Canonical:** Not Found in the KPI Encyclopedia  
   **Portal catalog:** `FI-KPI-025` — Aging Risk Summary — >90 Days

2. **Question Answered:**  
   How much overdue debt is already chronically late?

3. **Definition:**  
   The overdue balance more than ninety days past its due date. It is the same
   chronic overdue slice represented by **>90d Exposure**.

4. **Business Meaning:**  
   This is the most severe aging category. It identifies capital that may
   require escalation, restructuring, legal review, write-off assessment, or a
   decision to stop increasing exposure.

5. **How To Interpret:**

   - A large segment means significant cash is stuck in very late debt.
   - A rising segment means collection is not resolving older balances quickly
     enough.
   - Identify the customers behind the segment in the Attention List and
     rankings.
   - Do not assume every account will default; use the segment as a trigger for
     investigation and explicit management action.

## 4. Collection Attention List

The Collection Attention List answers:

> Which customers, salesmen, or wilayah require collection attention, and why?

The list contains one row per entity and attention signal. It can be filtered
by:

- All
- Chronic Overdue
- Plafond Breach + Overdue
- Legacy Debt
- Overdue
- High Overdue Workload
- Low Recovery vs Billing
- Wilayah Hotspot

### Visible Content

Each row displays:

- Type
- Name
- Signal
- Detail
- Wilayah
- Investigate action, when a report investigation is available

Customer and Salesman rows can be investigated in the relevant receivable
evidence. Wilayah rows are display-only in the current dashboard.

### How To Use The List

1. Start with **Chronic Overdue** when the priority is old debt.
2. Use **Low Recovery vs Billing** when the concern is recovery pace.
3. Use **Legacy Debt** to separate inactive customers from active late payers.
4. Review **Plafond Breach + Overdue** before allowing additional exposure.
5. Use **High Overdue Workload** to identify salesmen or areas carrying a
   disproportionate collection burden.
6. Open the related evidence before changing customer credit, supply, or
   collection ownership.

The list is a prioritization aid. It is not a probability-of-default score and
does not replace transaction-level review.

## 5. Rankings

The rankings answer:

> Where is overdue exposure concentrated by customer, salesman, and wilayah?

All three rankings use overdue balance only, not total outstanding balance.
Each ranking displays:

- Rank
- Code
- Name
- Overdue
- % of Total Overdue

### 5.1 Top 10 Overdue Customers

1. **KPI Canonical:** `1.2.1.11` — Top Overdue Customers  
   **Portal catalog:** `FI-KPI-026`

2. **Question Answered:**  
   Which ten customers hold the largest overdue balances?

3. **Definition:**  
   Customers are ranked by overdue balance in descending order. The list
   contains up to ten customers.

4. **Business Meaning:**  
   This is the primary name list for collection prioritization. It translates
   the company-level Overdue Exposure into specific accounts that can be
   contacted and investigated.

5. **How To Interpret:**

   - A small number of high-value names indicates concentrated collection risk.
   - Compare each customer’s overdue amount with the Aging Risk Summary and
     Overdue Concentration %.
   - A customer with a large >90-day balance needs stronger action than one
     whose overdue balance is mostly 1–30 days.
   - Use the investigation path to review open invoices and assign Sales and
     Finance ownership.

### 5.2 Top 10 Overdue Salesmen

1. **KPI Canonical:** `2.4.4.1` — Top Overdue Salesmen  
   **Portal catalog:** `FI-KPI-027`

2. **Question Answered:**  
   Which salesmen’s customer books contain the largest overdue balances?

3. **Definition:**  
   Salesmen are ranked by the overdue balance of the customer books associated
   with them, in descending order. The list contains up to ten salesmen.

4. **Business Meaning:**  
   This provides accountability and workload context for joint Sales–Finance
   collection. It does not mean the salesman personally caused the overdue
   balance.

5. **How To Interpret:**

   - A high amount means the salesman’s customer portfolio requires collection
     coordination.
   - Compare the ranking with **Top 10 Overdue Customers** to identify the
     accounts behind the salesman’s exposure.
   - A salesman with high overdue exposure and strong current sales may be
     growing debt faster than it is being recovered.
   - Assign joint follow-up rather than treating the ranking as an individual
     performance score.

### 5.3 Top 10 Overdue Wilayah

1. **KPI Canonical:** Not Found in the KPI Encyclopedia  
   **Portal catalog:** `FI-KPI-028` — Top Overdue Wilayah

2. **Question Answered:**  
   Which territories contain the largest overdue balances?

3. **Definition:**  
   Wilayah are ranked by their total overdue balance in descending order. The
   list contains up to ten wilayah.

4. **Business Meaning:**  
   This identifies geographic concentration of collection workload. It helps
   management organize regional follow-up and understand where overdue
   exposure is clustered.

5. **How To Interpret:**

   - A high amount indicates that a territory deserves collection capacity and
     management attention.
   - Compare the wilayah ranking with customer and salesman rankings to find
     the names responsible for the regional exposure.
   - A high regional amount does not by itself identify the cause; review
     customer aging, sales ownership, and local payment patterns.
   - The ranking has no direct row drill-down in the current dashboard, so use
     the other rankings and Piutang Report for named evidence.

## 6. Navigation

The Navigation section provides links to related pages:

- **Piutang** — review total open receivables and full aging.
- **Customers** — review customer activity, credit, and portfolio context.
- **Salesmen** — review salesman performance and collection exposure.
- **Piutang Report** — review open receivable and invoice-level evidence.

Navigation is supporting dashboard content. It has no additional KPI, table, or
chart. Use it when a summary value or ranking requires more detailed evidence.

## 7. Recommended Management Reading Order

1. Confirm the snapshot generation time and data freshness.
2. Read **Overdue Exposure** to understand the monetary size of late debt.
3. Compare **Cash Collected MTD** with **Recovery vs Billing %**.
4. Check **>90d Exposure** and the aging chart for chronic debt.
5. Check **Overdue Concentration %** to determine whether the problem is broad
   or concentrated.
6. Review **Legacy Debt Count** for inactive customers with remaining balances.
7. Filter the **Collection Attention List** by the management concern.
8. Use customer, salesman, and wilayah rankings to assign ownership.
9. Validate important cases in the Piutang Report before changing credit,
   supply, or collection policy.

## Related Dashboards

- **FI01 — Piutang:** total open receivables, full aging, and receivable
  concentration.
- **FI03 — Cash Flow Forecast:** projected month-end cash, collection pace,
  and cash scenarios.
- **FI04 — Piutang Report:** open-receivable and invoice-level evidence.
- **CU01 — Customer Analytics:** customer activity, credit, and portfolio
  context.
- **CU03 — Collection Optimization:** prioritized daily collection actions.
