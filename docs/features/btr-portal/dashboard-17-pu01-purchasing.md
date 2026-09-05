# PU01 — Purchasing Dashboard KPI Explanation

## Scope

**PU01 — Purchasing** is the **Purchasing Management Dashboard** at
`/dashboard/purchasing`.

It answers:

> Which suppliers and purchasing activities require management attention?

The dashboard covers the current calendar month and combines purchasing spend,
invoice activity, posting progress, supplier concentration, and purchasing
exposure against inventory and inventory risk.

## Important Reading Rules

- **Principal** means Supplier. The dashboard uses Principal in business labels.
- Purchase figures represent purchase invoices for the current calendar month.
- Void invoices are excluded.
- `BELUM` means the purchase has not yet been posted into inventory. A new
  `BELUM` invoice is normal staging and is not automatically a management
  problem.
- **Qualified Backlog** is the actionable portion of `BELUM`: invoices pending
  for at least three calendar days.
- Concentration percentages are management indicators, not automatic failure
  thresholds.
- Grand Total Purchase and Total Invoice should reconcile with the unfiltered
  Purchasing Report for the same month.

---

## 1. Purchasing Attention Cards

The cards provide a fast management scan. They identify the type of issue;
the Attention List identifies the affected Principal or company-level signal.

### 1.1 Posting Exposure

**KPI Canonical:** Composite card — individual canonical KPIs are listed below.

**Question Answered:**  
How much purchased value is still waiting to become available in inventory?

**Displayed KPIs:**

- **Qualified Backlog Count & Value** — `4.4.2.1`; Portal catalog:
  `PU-KPI-005`
- **Pending Posting Value** — `4.4.1.2`; Portal catalog: `PU-KPI-004`

**Business Meaning:**  
This card separates normal recent staging from aged posting delay. A high
qualified backlog means the business has already committed money to goods that
may not yet be available for sales or replenishment decisions.

**How To Interpret:**

- High Qualified Backlog requires investigation and posting follow-up.
- High Pending Posting Value with low Qualified Backlog may represent recent,
  normal invoice staging.
- High backlog together with stock-out risk is especially urgent: the needed
  goods may already have been purchased but are not yet posted.
- Review the Purchasing Report and complete the operational posting workflow.

### 1.2 Principal Dependency

**KPI Canonical:** Composite card — individual canonical KPIs are listed below.

**Question Answered:**  
How dependent is current purchasing on a small number of Principals?

**Displayed KPIs:**

- **Top 1 Principal %** — `4.1.2.1`; Portal catalog: `PU-KPI-006`
- **Top 3 Principal %** — `4.1.2.2`; Portal catalog: `PU-KPI-007`
- **Compound Dependency Count** — `4.1.1.3`; Portal catalog: `PU-KPI-008`

**Business Meaning:**  
This card shows whether purchasing cash and supply continuity depend on a small
number of Principals. Dependency may be intentional, but it increases exposure
to supply disruption, price changes, and weak negotiation alternatives.

**How To Interpret:**

- A high Top 1 percentage means one Principal dominates current-month buying.
- A high Top 3 percentage means concentration remains material even when no
  single Principal appears dominant.
- A high Compound Dependency Count means Principals are important in purchasing
  and also important in inventory or at-risk inventory.
- Review the Top 10 Principals and Principal Exposure Comparison before
  increasing the same dependency.

### 1.3 Purchasing Pace

**KPI Canonical:** Composite card — individual canonical KPIs are listed below.

**Question Answered:**  
Is purchasing activity progressing during the month?

**Displayed KPIs:**

- **Total Invoice** — `4.3.1.2`; Portal catalog: `PU-KPI-002`
- **Purchasing Inactivity Flag** — `4.4.3.1`; Portal catalog: `PU-KPI-009`

**Business Meaning:**  
The card distinguishes a quiet purchasing month from a purchasing process that
has stopped unexpectedly. Inactivity can be healthy when stock is sufficient,
but it is a replenishment risk when stock-out exposure is increasing.

**How To Interpret:**

- A non-zero Total Invoice shows that purchasing documents have been recorded.
- An active Purchasing Inactivity flag means no purchase invoice has been
  recorded after the mid-month threshold.
- Inactivity together with stock-out risk requires immediate replenishment
  review.
- Confirm whether the quiet period is an intentional purchasing freeze or a
  process failure.

### 1.4 Inventory Cross-Risk

**KPI Canonical:** Composite card — individual canonical KPIs are listed below.

**Question Answered:**  
Are important Principals also creating inventory or inventory-risk exposure?

**Displayed KPIs:**

- **Top 1 Supplier Inventory %** — related supplier concentration measure;
  `4.1.1.1` — Top Supplier %, Portal catalog `EX-KPI-012` — Top Supplier %
  (Inventory)
- **Principal At-Risk Count** — `4.2.1.1`; Portal catalog: `PU-KPI-010`
- **Inventory, No Purchase Count** — management signal; **KPI Canonical:
  Not Found**

**Business Meaning:**  
This card connects purchasing decisions with the stock already held. It helps
management avoid treating purchase volume in isolation when a Principal's goods
are already concentrated, unhealthy, or no longer being replenished.

**How To Interpret:**

- A high Principal At-Risk Count means several Principal relationships require
  review because of at-risk inventory exposure.
- Inventory, No Purchase identifies high inventory from a Principal with no
  current-month purchase; this can indicate legacy stock or a deliberate pause.
- High purchase and high at-risk inventory on the same Principal is a warning
  against automatically buying more.
- Cross-check Inventory Risk and Principal Exposure Comparison.

---

## 2. Purchasing Summary

The summary establishes the scale of purchasing and the condition of the
current-month intake.

### 2.1 Grand Total Purchase

1. **KPI Canonical:** `4.3.1.1` — Grand Total Purchase  
   **Portal catalog:** `PU-KPI-001`

2. **Question Answered:**  
   How much purchase value has been recorded this month?

3. **Definition:**  
   The total value of current-month purchase invoices.

4. **Business Meaning:**  
   This is the headline purchasing spend. It shows how much cash has been
   committed to supplier purchases and provides the scale for interpreting
   posting, concentration, and inventory exposure.

5. **How To Interpret:**

   - A high value means heavy purchasing activity; check whether it fills real
     stock needs or adds to existing inventory.
   - A low value may mean controlled purchasing, but it may also signal
     replenishment failure.
   - Read it with Posted %, Qualified Backlog, stock-out risk, and Principal
     concentration.
   - It is not a measure of inventory value or supplier payment settlement.

### 2.2 Total Invoice

1. **KPI Canonical:** `4.3.1.2` — Total Invoice  
   **Portal catalog:** `PU-KPI-002`

2. **Question Answered:**  
   How many purchase invoices were recorded this month?

3. **Definition:**  
   The count of current-month purchase invoices.

4. **Business Meaning:**  
   Total Invoice measures purchasing document activity, not purchase spend.
   It helps management understand administrative and warehouse processing load.

5. **How To Interpret:**

   - High count with low spend usually means many smaller purchases.
   - Low count with high spend means purchasing is concentrated in a few large
     invoices.
   - High count with low Posted % indicates a possible processing bottleneck.
   - Zero activity after mid-month should be read with Purchasing Inactivity
     and stock-out risk.

### 2.3 Posted %

1. **KPI Canonical:** `4.4.1.1` — Posted %  
   **Portal catalog:** `PU-KPI-003`

2. **Question Answered:**  
   What share of this month's purchase value has been posted into inventory?

3. **Definition:**  
   The percentage of current-month purchase value whose posting status is
   `SUDAH`.

4. **Business Meaning:**  
   Posted % measures the conversion of purchasing intake into stock available
   in the inventory records. Purchased goods that remain unposted cannot be
   reliably used for sales or replenishment decisions.

5. **How To Interpret:**

   - High Posted % means most purchased value has entered inventory records.
   - Low Posted % means a large share of purchased value is still waiting for
     posting.
   - Low Posted % late in the month is more concerning than low Posted % just
     after invoice entry.
   - Compare it with Pending Posting Value and Qualified Backlog; Posted %
     measures completion, while those measures show the remaining exposure.

### 2.4 Pending Posting Value

1. **KPI Canonical:** `4.4.1.2` — Pending Posting Value  
   **Portal catalog:** `PU-KPI-004`

2. **Question Answered:**  
   How much current-month purchase value has not yet been posted?

3. **Definition:**  
   The total value of current-month invoices with posting status `BELUM`,
   regardless of age.

4. **Business Meaning:**  
   This is the raw posting exposure. It includes both recent normal staging and
   older invoices that may require intervention.

5. **How To Interpret:**

   - A high or growing value means purchased goods are not yet represented as
     posted inventory.
   - Do not treat all Pending Posting Value as overdue work.
   - Use Qualified Backlog to identify the aged, actionable portion.
   - Prioritize Principals whose pending goods relate to stock-out risk.

### 2.5 Qualified Backlog

1. **KPI Canonical:** `4.4.2.1` — Qualified Backlog Count & Value  
   **Portal catalog:** `PU-KPI-005`

2. **Question Answered:**  
   How many purchase invoices have remained unposted long enough to require
   management attention?

3. **Definition:**  
   The count and value of `BELUM` purchase invoices that have been pending for
   at least three calendar days.

4. **Business Meaning:**  
   Qualified Backlog identifies an actionable intake delay. It prevents recent,
   normal staging from being treated as a failure while highlighting invoices
   that may already be delaying stock availability.

5. **How To Interpret:**

   - A high count indicates broad processing delay.
   - A high value indicates material capital is tied up in unposted purchases.
   - Any backlog connected to stock-out SKUs or compound-dependent Principals
     deserves priority.
   - The management response is generally to post first and avoid duplicate
     buying of goods already purchased.

---

## 3. Purchasing Attention List

1. **KPI Canonical:** Not Found — this is a management attention table, not a
   separately catalogued KPI.

2. **Question Answered:**  
   Which Principals or purchasing conditions require follow-up, and why?

3. **Definition:**  
   A filterable list with one row per **entity × signal**. The table displays
   Entity, Signal, Amount, Context, and available Profile or Investigate
   actions.

   The approved signals are:

   - Qualified Backlog
   - Principal Spend Concentration
   - Principal Inventory Concentration
   - Principal At-Risk Exposure
   - Compound Dependency
   - Purchasing Inactivity
   - Principal Inventory No Purchase
   - Unknown Principal

4. **Business Meaning:**  
   The cards show the scale of an issue; this list names the Principals and
   conditions that management can investigate. It turns dashboard signals into
   a practical purchasing follow-up queue.

5. **How To Interpret:**

   - Start with the signal selected from the relevant attention card.
   - A Principal may appear more than once because different signals can apply.
   - A high Amount shows financial exposure, while Context explains the reason
     or supporting condition.
   - Use Purchasing Report for invoice evidence and Inventory or Inventory Risk
     for cross-domain evidence.
   - A Company-level Purchasing Inactivity row represents a business condition,
     not a specific Principal.

---

## 4. Charts

### 4.1 Weekly Purchase Trend

1. **KPI Canonical:** Not Found — visual trend analysis.

2. **Question Answered:**  
   Is purchasing activity accelerating, stable, or slowing during the month?

3. **Definition:**  
   A line chart showing purchase value by calendar week in the current month.

4. **Business Meaning:**  
   The trend adds timing context to Grand Total Purchase. The same monthly
   spend can represent very different management situations depending on
   whether purchasing is steady, delayed, or concentrated near month-end.

5. **How To Interpret:**

   - Rising weekly values may indicate increasing replenishment activity.
   - A flat or declining trend may indicate controlled buying, delayed intake,
     or a developing replenishment gap.
   - A sharp late-month spike should be checked against stock needs and
     supplier concentration.
   - Read the trend with Purchasing Inactivity, stock-out risk, and Qualified
     Backlog. The chart itself does not create an automatic alert.

### 4.2 Posting Status Breakdown

1. **KPI Canonical:** Not Found — visual distribution of Posted % and Pending
   Posting Value.

2. **Question Answered:**  
   How is current-month purchase value divided between posted and unposted
   status?

3. **Definition:**  
   A pie chart comparing purchase value with status:

   - `SUDAH` — posted into inventory
   - `BELUM` — not yet posted

4. **Business Meaning:**  
   The chart makes the conversion of purchases into inventory visible at a
   glance. It helps management see whether purchasing activity is translating
   into usable stock.

5. **How To Interpret:**

   - A large `SUDAH` segment indicates stronger posting completion.
   - A large `BELUM` segment indicates material unposted purchase exposure.
   - A large `BELUM` segment is not automatically overdue; check Qualified
     Backlog for age.
   - If `BELUM` is large while stock-out risk is also high, investigate whether
     the needed goods are already purchased but blocked in posting.

---

## 5. Rankings and Exposure Tables

### 5.1 Top 10 Principals

1. **KPI Canonical:** `4.3.2.1` — Top 10 Principal Ranking  
   **Portal catalog:** `PU-KPI-011`

2. **Question Answered:**  
   Which Principals are receiving the most purchasing spend this month?

3. **Definition:**  
   Up to ten Principals ranked by current-month purchase value. The table shows
   Rank, Principal, MTD Purchase, and % of Purchase.

4. **Business Meaning:**  
   This is the working list for purchasing concentration. It identifies where
   the company's current purchasing cash is going and which supplier
   relationships deserve review.

5. **How To Interpret:**

   - A high rank means the Principal is a major current-month purchasing
     destination.
   - A concentrated ranking may indicate supplier dependency.
   - Compare the names with Principal Exposure Comparison and Inventory Risk.
   - A high purchase rank does not prove good supplier performance or healthy
     inventory.

### 5.2 Principal Exposure Comparison

1. **KPI Canonical:** `4.1.2.5` — Principal Exposure Comparison  
   **Portal catalog:** `PU-KPI-012`

2. **Question Answered:**  
   Which Principals are important in purchasing, inventory, and inventory risk
   at the same time?

3. **Definition:**  
   A side-by-side table comparing each Principal's:

   - MTD Purchase and % of Purchase
   - Inventory Value and % of Inventory
   - At-Risk Value and % of At-Risk Inventory
   - Compound and No Purchase flags

4. **Business Meaning:**  
   This table shows whether the company is buying from a Principal, holding a
   large amount of that Principal's goods, and carrying unhealthy stock from
   the same relationship. It makes compound dependency visible by name.

5. **How To Interpret:**

   - High purchase, high inventory, and high at-risk value on one row is a
     strong dependency warning.
   - High inventory with zero current-month purchase may indicate legacy stock
     or an intentional purchasing pause.
   - High purchase with low at-risk exposure is generally a healthier
     dependency, but should still be monitored.
   - Use the table before increasing purchases from a Principal with existing
     inventory or at-risk exposure.

---

## 6. Navigation

The Navigation section links management to supporting evidence and related
decisions:

- **PU02 — Purchasing Report:** verify purchase invoices and posting evidence.
- **IN01 — Inventory:** review current stock composition.
- **IN02 — Inventory Risk:** review dead, slow-moving, never-sold, and at-risk
  inventory.

---

## Recommended Management Reading Order

1. Scan the **Purchasing Attention Cards**.
2. Read Grand Total Purchase, Total Invoice, Posted %, and Qualified Backlog.
3. Open the relevant **Attention List** signal to identify Principals requiring
   follow-up.
4. Review the Weekly Purchase Trend and Posting Status Breakdown.
5. Compare the Top 10 Principals with Principal Exposure Comparison.
6. Cross-check the affected Principals in Inventory and Inventory Risk.
7. Validate transaction evidence in PU02 Purchasing Report before taking
   operational action.

The dashboard supports management decisions; posting, purchasing changes,
warehouse action, and supplier follow-up are completed through the appropriate
operational BTR workflows.
