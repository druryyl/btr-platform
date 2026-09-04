# BUSINESS QUESTION CATALOG v2

**Role:** Owner Navigation Foundation  
**Audience:** Business Owner, Director, Department Head  
**Purpose:** Give an owner a question → investigation → decision path using the existing BTR Portal. This is the input for Phase-4 Question Navigation Mapping.  
**Sources:** [kpi-inventory.md](./kpi-inventory.md), [kpi-encyclopedia.html](./kpi-encyclopedia.html), [business-question-catalog.md](./business-question-catalog.md)  
**Date:** 2 September 2026

This version does not discover new questions. It redesigns the v1 catalog so an owner can open the portal, find the concern, follow a path, and decide.

---

# How an owner uses this catalog

```text
Open the portal
        ↓
Ask one Executive Question
        ↓
Open the matching Management Question
        ↓
Follow the Investigation Path
        ↓
Decide
```

Do not start from a dashboard list. Start from the concern.

| Level | What it is | What it is not |
|-------|------------|----------------|
| Executive Question | The concern that made the owner open the portal | A KPI name |
| Management Question | Where to look inside that concern | A formula or validation check |
| Investigation Path | The route through dashboard, KPI, cause, and action | A standalone question |

BTR terms are kept as used in the business: Piutang, Omzet, Faktur, Plafond, Principal, Wilayah.

---

# Owner journey

```text
Owner opens the portal
        ↓
EQ-001  Is the business healthy?
        │
        ├─ Risk stands out              → EQ-002
        ├─ Something must be done today → EQ-003
        ├─ Cash feels tight             → EQ-004
        ├─ Sales feel weak              → EQ-005
        ├─ Warehouse or piutang feel heavy → EQ-006
        ├─ A few names seem to run the business → EQ-007
        └─ Nothing is broken yet, but signals are forming → EQ-008
```

---

# Hierarchy map

```text
EQ-001  Is the business healthy?
        └── (starting path only — branch to EQ-002 … EQ-008)

EQ-002  Where is my biggest risk?
        MQ-001  Which customers are becoming risky?
        MQ-002  Which inventory is unhealthy?
        MQ-003  Which salespeople are carrying overdue or dormant books?
        MQ-004  Which principals are becoming a buying or stock risk?

EQ-003  What needs immediate attention?
        MQ-005  Who should be contacted today?
        MQ-006  What should we buy, delay, transfer, or clear today?

EQ-004  Is cash flow healthy?
        MQ-007  Which receivables require attention?
        MQ-008  Are we collecting as fast as we are billing?

EQ-005  Is growth sustainable?
        MQ-009  Which salespeople are underperforming?
        MQ-010  Which customers are declining?
        MQ-011  Did the field team cover the planned route and produce orders?

EQ-006  Is working capital healthy?
        MQ-012  Which inventory is becoming dead stock?
        MQ-013  Which products are losing momentum?
        MQ-014  Will selling products run out while other stock sits idle?

EQ-007  Are we overly dependent on specific customers, suppliers, or salespeople?
        MQ-015  Which customers dominate omzet or piutang?
        MQ-016  Which suppliers are becoming dominant?
        MQ-017  Are we too dependent on a few salespeople?

EQ-008  What is likely to become a problem next?
        MQ-018  Is credit policy being respected?
        MQ-019  Are purchased goods posted, or stuck in backlog?
```

Each management question has one investigation path with the same ID number (MQ-001 → IP-001).

---

# LEVEL 1
# EXECUTIVE QUESTIONS

These are the only questions an owner should need at the door of the portal.

---

## EQ-001

Question:

Is the business healthy?

Why Owner Would Ask This:

The owner needs a single morning judgment before sending anyone into a dashboard. Customer quality, sales delivery, cash conversion, and warehouse capital can move in different directions on the same day.

Typical Trigger:

- Start of day / start of week review
- Month feels “busy” but results feel unclear
- Conflicting stories from Sales, Finance, and Warehouse

Expected Decision:

- Continue as usual, or pick the one area that is not healthy and go there next

Leads To:

EQ-002 through EQ-008

Starting Path:

```text
Is the business healthy?
        ↓
Executive
        ↓
Domain Attention Summary Cards
        ↓
Portfolio Health Score
        ↓
Achievement %
        ↓
Recovery vs Billing %
        ↓
Inventory Health Score
        ↓
Open the weakest of EQ-002 … EQ-008
```

---

## EQ-002

Question:

Where is my biggest risk?

Why Owner Would Ask This:

Attention is scarce. Overdue customers, stuck stock, weak sales books, and Principal dependence can all look urgent. The owner must see which problem is largest before assigning work.

Typical Trigger:

- Several areas look bad at once
- Cash tightening
- Inventory increasing while sales are flat
- Alert Center is busy

Expected Decision:

- Put Finance, Sales, Warehouse, or Purchasing on the largest risk first — not on the loudest report

Leads To:

MQ-001, MQ-002, MQ-003, MQ-004

Starting Path:

```text
Where is my biggest risk?
        ↓
Alert Center / Executive
        ↓
Alert counts by area
        ↓
Customers At Risk Count
        ↓
Overdue Exposure
        ↓
At-Risk Inventory %
        ↓
Principal At-Risk Count
        ↓
Open the matching management question
```

---

## EQ-003

Question:

What needs immediate attention?

Why Owner Would Ask This:

Monthly totals do not tell the owner what must happen before the day ends. Today’s work is contacts, posting, buying, delaying, or clearing — not another summary.

Typical Trigger:

- Start of day
- Collection slowing
- Warehouse asking what to buy
- Pinned platform alerts

Expected Decision:

- Direct today’s collection contacts, or today’s inventory actions, or both

Leads To:

MQ-005, MQ-006

---

## EQ-004

Question:

Is cash flow healthy?

Why Owner Would Ask This:

Invoiced omzet is not cash. If billing outruns collection, the next purchase cycle is funded with delayed money.

Typical Trigger:

- Cash tightening
- Collection slowing
- Piutang growing while omzet looks fine

Expected Decision:

- Accelerate collection, restrict supply to late accounts, or accept the month’s cash finish

Leads To:

MQ-007, MQ-008

---

## EQ-005

Question:

Is growth sustainable?

Why Owner Would Ask This:

A month can hit target on a few names while the rest of the book goes quiet, routes are skipped, or next month has no pipeline. The owner needs to know whether growth will still be there after this month.

Typical Trigger:

- Sales declining
- Target looks reachable but the book feels thinner
- A few salespeople carrying the month

Expected Decision:

- Push this month, rebuild coverage, recover declining customers, or stop celebrating concentrated omzet

Leads To:

MQ-009, MQ-010, MQ-011

---

## EQ-006

Question:

Is working capital healthy?

Why Owner Would Ask This:

Cash sitting in overdue piutang and cash sitting in dead stock are the same owner problem: money that cannot fund the next cycle. A high warehouse value can fund sales or hide trapped capital.

Typical Trigger:

- Inventory increasing
- Warehouse “full” while selling items run out
- Piutang not turning back into cash

Expected Decision:

- Delay buying, clear stuck stock, or free capital from aging piutang

Leads To:

MQ-012, MQ-013, MQ-014

---

## EQ-007

Question:

Are we overly dependent on specific customers, suppliers, or salespeople?

Why Owner Would Ask This:

A book, a sales force, or a warehouse can look healthy in total and still be one relationship away from a shock. Concentration is a strategic risk even when aging still looks acceptable.

Typical Trigger:

- One customer, Principal, or salesperson dominates the conversation
- Fear of losing a key account or a key Principal
- Omzet looks fine until one name is removed in the owner’s head

Expected Decision:

- Protect the key relationship, reduce exposure, or broaden the book / buying / sales force

Leads To:

MQ-015, MQ-016, MQ-017

---

## EQ-008

Question:

What is likely to become a problem next?

Why Owner Would Ask This:

Overdue, dormant, dead stock, and Plafond breaches appear after the damage. The owner wants the signals that are forming now: Watch accounts, slowing products, credit-limit pressure, unposted purchases.

Typical Trigger:

- The totals still look acceptable
- Early-month planning
- A sense that next month will be harder than this one

Expected Decision:

- Act early on credit, posting, or slowing offtake — or confirm that the quiet picture is real

Leads To:

MQ-018, MQ-019  
(also use MQ-001 Watch customers, MQ-010 declining customers, MQ-013 slowing products)

---

# LEVEL 2
# MANAGEMENT QUESTIONS

These are the concerns a manager or owner takes into a dashboard. Each one has one investigation path.

---

## MQ-001

Question:

Which customers are becoming risky?

Parent Executive Question:

EQ-002

Why Owner Would Ask This:

Collection time and credit attention should go to accounts that are weakening, not only to accounts that are already overdue.

Typical Trigger:

- Collection slowing
- Customer complaints about supply holds
- Portfolio health weakening

Expected Decision:

- Protect, restrict, or recover the named accounts — and decide whether Finance or Sales owns the contact

Investigation Path:

IP-001

---

## MQ-002

Question:

Which inventory is unhealthy?

Parent Executive Question:

EQ-002

Why Owner Would Ask This:

Stuck stock is trapped cash. Management must see the unhealthy share before releasing more purchase cash.

Typical Trigger:

- Inventory increasing
- Warehouse feels full
- Purchasing asking to buy more

Expected Decision:

- Stop or delay buying into unhealthy stock; start a clearance or transfer review

Investigation Path:

IP-002

---

## MQ-003

Question:

Which salespeople are carrying overdue or dormant books?

Parent Executive Question:

EQ-002

Why Owner Would Ask This:

A salesperson’s omzet can look strong while the owned book is late or going quiet. Those books become next month’s collection problem.

Typical Trigger:

- Collection slowing on certain routes
- Dormant customers rising
- High omzet names with high piutang

Expected Decision:

- Put Sales and Finance on the same books; coach collection, not only selling

Investigation Path:

IP-003

---

## MQ-004

Question:

Which principals are becoming a buying or stock risk?

Parent Executive Question:

EQ-002

Why Owner Would Ask This:

Continuing to buy a Principal whose goods are already aging compounds trapped capital.

Typical Trigger:

- Inventory increasing on one Principal
- Purchasing still placing orders on slow lines
- One Principal dominating spend and sick stock

Expected Decision:

- Keep buying, delay buying, or review the Principal relationship

Investigation Path:

IP-004

---

## MQ-005

Question:

Who should be contacted today?

Parent Executive Question:

EQ-003

Why Owner Would Ask This:

Not every weak account needs the same work. Some need immediate collection, some a reminder, some a credit review, some a sales visit, and some owner escalation.

Typical Trigger:

- Start of day
- Collection slowing
- Cash tightening

Expected Decision:

- Run today’s contact list by type: collect, remind, review credit, recover sales, or escalate

Investigation Path:

IP-005

---

## MQ-006

Question:

What should we buy, delay, transfer, or clear today?

Parent Executive Question:

EQ-003

Why Owner Would Ask This:

Holes and excess often exist at the same time. Buying everything that feels empty while slow movers age traps cash.

Typical Trigger:

- Warehouse asking what to buy
- Inventory increasing
- Selling items running out

Expected Decision:

- Buy, delay, transfer, or clear — as separate actions, not one purchase wave

Investigation Path:

IP-006

---

## MQ-007

Question:

Which receivables require attention?

Parent Executive Question:

EQ-004

Why Owner Would Ask This:

Total piutang is scale. Overdue is urgency. Names are what collectors can work.

Typical Trigger:

- Cash tightening
- Collection slowing
- Piutang growing

Expected Decision:

- Accelerate collection on the named overdue accounts; restrict supply if aging is chronic

Investigation Path:

IP-007

---

## MQ-008

Question:

Are we collecting as fast as we are billing?

Parent Executive Question:

EQ-004

Why Owner Would Ask This:

If new Faktur keep outrunning cash in, the book grows even when collectors are busy.

Typical Trigger:

- Omzet looks strong but cash is tight
- Collection slowing
- Month-end cash worry

Expected Decision:

- Push collection pace, slow credit, or accept that this month’s billing will not return as cash in time

Investigation Path:

IP-008

---

## MQ-009

Question:

Which salespeople are underperforming?

Parent Executive Question:

EQ-005

Why Owner Would Ask This:

Company omzet can be carried by a few names. “Below target” and “no target” are different failures. Coaching the wrong cause wastes the week.

Typical Trigger:

- Sales declining
- Target gap widening
- A few people carrying the month

Expected Decision:

- Coach named people this week, set missing targets, or change coverage

Investigation Path:

IP-009

---

## MQ-010

Question:

Which customers are declining?

Parent Executive Question:

EQ-005

Why Owner Would Ask This:

A customer who is still current but buying less is next month’s dormant account. Recovery is cheaper than winning a new name.

Typical Trigger:

- Sales declining
- Book feels quieter
- Strategic accounts buying less

Expected Decision:

- Recover the declining names, or stop spending coverage time on accounts that will not return

Investigation Path:

IP-010

---

## MQ-011

Question:

Did the field team cover the planned route and produce orders?

Parent Executive Question:

EQ-005

Why Owner Would Ask This:

Visits without orders are activity without result. Orders without planned visits are unmanaged coverage. Growth that depends on skipped routes will not last.

Typical Trigger:

- Sales declining
- Salespeople claim they are working
- Orders not matching visit stories

Expected Decision:

- Fix coverage, fix conversion, or challenge check-in credibility

Investigation Path:

IP-011

---

## MQ-012

Question:

Which inventory is becoming dead stock?

Parent Executive Question:

EQ-006

Why Owner Would Ask This:

Dead stock is cash that has already stopped working. It needs clearance or a stop-buy, not another purchase.

Typical Trigger:

- Inventory increasing
- Warehouse full of old goods
- Purchasing still replenishing slow lines

Expected Decision:

- Clear, return, or stop buying the named SKUs and their Principal

Investigation Path:

IP-012

---

## MQ-013

Question:

Which products are losing momentum?

Parent Executive Question:

EQ-006

Why Owner Would Ask This:

Slow movers become dead stock. Acting while goods still move is cheaper than clearing them later.

Typical Trigger:

- Inventory increasing
- Offtake slowing on familiar lines
- New receipts not selling

Expected Decision:

- Delay replenishment, push sell-through, or treat the line as a buying-quality problem

Investigation Path:

IP-013

---

## MQ-014

Question:

Will selling products run out while other stock sits idle?

Parent Executive Question:

EQ-006

Why Owner Would Ask This:

The company can be overstocked on what does not sell and understocked on what does. Those two conditions need opposite actions in the same week.

Typical Trigger:

- Sales lost to empty shelves
- Inventory increasing overall
- Warehouse “empty” on a few holes

Expected Decision:

- Buy the holes, delay the piles, transfer between locations if one warehouse holds the idle stock

Investigation Path:

IP-014

---

## MQ-015

Question:

Which customers dominate omzet or piutang?

Parent Executive Question:

EQ-007

Why Owner Would Ask This:

Growth from a shrinking set of accounts, or piutang sitting with a few names, is concentration risk. Losing one name would change the month.

Typical Trigger:

- One customer dominates the conversation
- Fear of losing a key account
- Piutang looks large because of a few names

Expected Decision:

- Protect the key accounts, reduce exposure, or broaden the book

Investigation Path:

IP-015

---

## MQ-016

Question:

Which suppliers are becoming dominant?

Parent Executive Question:

EQ-007

Why Owner Would Ask This:

A Principal can take this month’s purchase cash, already hold the warehouse, and own the sick stock. Those are three different kinds of dependence.

Typical Trigger:

- One Principal dominates purchasing talk
- Stock and risk sitting with the same name
- Fear of supply interruption

Expected Decision:

- Treat the dependence as chosen strategy, or reduce buying into that Principal

Investigation Path:

IP-016

---

## MQ-017

Question:

Are we too dependent on a few salespeople?

Parent Executive Question:

EQ-007

Why Owner Would Ask This:

If one or two people produce most omzet, next month’s plan is a people risk, not a market risk.

Typical Trigger:

- A star salesperson dominates results
- Active force feels thin
- Fear of losing a key person

Expected Decision:

- Protect key people, rebuild coverage across the team, or stop treating star omzet as team health

Investigation Path:

IP-017

---

## MQ-018

Question:

Is credit policy being respected?

Parent Executive Question:

EQ-008

Why Owner Would Ask This:

Plafond and hold rules only protect cash if they are observed. A credit-limit signal is still preventable. A breach, or a suspended account still being billed, is already a failure.

Typical Trigger:

- Cash tightening
- Customers buying above comfort
- Collection slowing on large accounts

Expected Decision:

- Review credit, restrict supply, or stop billing suspended accounts

Investigation Path:

IP-018

---

## MQ-019

Question:

Are purchased goods posted, or stuck in backlog?

Parent Executive Question:

EQ-008

Why Owner Would Ask This:

Unposted purchases mean the company has already spent or committed without gaining sellable stock. Posting can be more urgent than placing new orders. Quiet purchasing late in the month can be a freeze or a stall.

Typical Trigger:

- Warehouse waiting for stock that was already bought
- Purchasing gone quiet
- Selling SKUs at risk of running out

Expected Decision:

- Post existing invoices first, then resume or keep the freeze

Investigation Path:

IP-019

---

# LEVEL 3
# INVESTIGATION PATHS

These are not questions. They are the routes an owner follows after a management question.

Format:

```text
Problem
        ↓
Dashboard
        ↓
Data Source
        ↓
KPI
        ↓
Next KPI
        ↓
Possible Cause
        ↓
Possible Action
```

---

## IP-001

Parent:

MQ-001 Which customers are becoming risky?

```text
Customer risk increasing
        ↓
Customer Risk Forecast
        ↓
Customer Risk Assessment
        ↓
Customers At Risk Count / Risk Category
        ↓
Payment Delay Signal Count
        ↓
Overdue Exposure
        ↓
Top Overdue Customers
        ↓
Possible cause: late payment, inactivity, purchase decline, or credit-limit pressure
        ↓
Possible action: review collection strategy, restrict supply, or recover the account
```

If the at-risk names are also strategic, open Strategic Customers At Risk Count before changing policy.

---

## IP-002

Parent:

MQ-002 Which inventory is unhealthy?

```text
Inventory becoming unhealthy
        ↓
Inventory Risk
        ↓
Inventory Exposure
        ↓
At-Risk Inventory %
        ↓
Aging Distribution
        ↓
Dead Stock / Slow Moving / Never Sold
        ↓
Possible cause: overbuying, offtake slowing, or new receipts that never sold
        ↓
Possible action: delay purchasing, start clearance, or stop replenishing the unhealthy share
```

---

## IP-003

Parent:

MQ-003 Which salespeople are carrying overdue or dormant books?

```text
Sales books going late or quiet
        ↓
Salesmen / Collection Dashboard
        ↓
Customer Exposure
        ↓
High Overdue Exposure Count
        ↓
Dormant Portfolio Count
        ↓
Top Overdue Salesmen
        ↓
Possible cause: selling without collecting, skipped recovery visits, or a few large late accounts
        ↓
Possible action: joint Sales–Finance review of the named books
```

If outstanding is large but not late, treat it as credit size (High Piutang Exposure), not as a collection failure.

---

## IP-004

Parent:

MQ-004 Which principals are becoming a buying or stock risk?

```text
Principal becoming a stock or buying risk
        ↓
Purchasing Management / Inventory Risk
        ↓
Supplier Risk Monitoring
        ↓
Principal At-Risk Count
        ↓
Supplier Risk Exposure
        ↓
Principal Exposure Comparison
        ↓
Possible cause: still buying a Principal whose goods already sit as sick stock
        ↓
Possible action: delay buying, review the Principal, or keep buying only if the dependence is chosen strategy
```

---

## IP-005

Parent:

MQ-005 Who should be contacted today?

```text
Today’s collection work is unclear
        ↓
Collection Optimization
        ↓
Collection Action Queue
        ↓
Actions Today
        ↓
Immediate Collection Count
        ↓
Proactive Reminder / Credit Review / Sales Recovery / Escalation counts
        ↓
Possible cause: mixed jobs on one list, or no contact plan for invoices due within 7 days
        ↓
Possible action: work the list by type — collect, remind, review credit, recover sales, or escalate
```

---

## IP-006

Parent:

MQ-006 What should we buy, delay, transfer, or clear today?

```text
Today’s inventory action is unclear
        ↓
Inventory Optimization
        ↓
Inventory Action Queue
        ↓
Critical Actions Count
        ↓
Action Counts by Type (Purchase / Delay / Transfer / Clearance)
        ↓
Stock-Out Risk vs Recoverable Capital
        ↓
Possible cause: holes and excess at the same time
        ↓
Possible action: buy the holes, delay the piles, transfer, or clear — not one purchase wave
```

---

## IP-007

Parent:

MQ-007 Which receivables require attention?

```text
Receivables need attention
        ↓
Piutang Dashboard / Collection Dashboard
        ↓
Piutang Exposure
        ↓
Total Piutang
        ↓
Overdue Exposure
        ↓
Piutang > 90 Hari
        ↓
Top Overdue Customers
        ↓
Possible cause: a few large late names, chronic >90-day debt, or overdue spread across wilayah / salespeople
        ↓
Possible action: accelerate collection on named accounts; restrict supply if aging is chronic
```

If overdue is concentrated, continue to Top Overdue Salesmen and Top Overdue Wilayah before treating it as a capacity problem.

---

## IP-008

Parent:

MQ-008 Are we collecting as fast as we are billing?

```text
Cash not returning as fast as billing
        ↓
Collection Dashboard / Cash Flow Forecast
        ↓
Collection Recovery
        ↓
Recovery vs Billing %
        ↓
Cash Collected MTD
        ↓
Collection Gap / Required Daily Collection
        ↓
Possible cause: billing outrunning receipts, or remaining days too few to close the gap
        ↓
Possible action: push daily collection, slow new credit, or accept this month’s cash finish
```

---

## IP-009

Parent:

MQ-009 Which salespeople are underperforming?

```text
Salespeople missing the plan
        ↓
Salesmen / Sales Forecast
        ↓
Sales Target Achievement
        ↓
Below Target Count
        ↓
Missing Target Setup Count
        ↓
Principal Achievement Table
        ↓
Possible cause: missed plan, no plan at all, or a Principal-mix problem
        ↓
Possible action: coach named people, set missing targets, or fix range / buying behind the miss
```

Then compare Top 10 Achievement % with Top 10 Omzet — money producers and plan attainers are not always the same people.

---

## IP-010

Parent:

MQ-010 Which customers are declining?

```text
Customers buying less
        ↓
Customer Portfolio
        ↓
Customer Lifecycle
        ↓
Declining Count
        ↓
Purchase Decline Signal Count
        ↓
Dormant Count / Inactivity Signal Count
        ↓
Possible cause: offtake slowing before 90-day dormant, or accounts that never converted
        ↓
Possible action: recover declining names, or stop covering never-purchased / dormant accounts
```

If declining names are strategic, treat them as owner-level relationships, not only as a salesperson task.

---

## IP-011

Parent:

MQ-011 Did the field team cover the planned route and produce orders?

```text
Field work not producing results
        ↓
Field Activity
        ↓
Visit Execution
        ↓
Visit Execution %
        ↓
Missed Visits / Bottom Visit Execution
        ↓
Effective Call Rate / GPS Valid Rate
        ↓
Orders Generated vs Omzet Generated
        ↓
Possible cause: skipped routes, visits that do not convert, or check-ins that are not credible
        ↓
Possible action: fix coverage, fix conversion, or challenge GPS-perfect execution
```

---

## IP-012

Parent:

MQ-012 Which inventory is becoming dead stock?

```text
Dead stock increasing
        ↓
Inventory Risk
        ↓
Inventory Aging
        ↓
Dead Stock Count & Value
        ↓
Top 10 Dead / Slow Moving
        ↓
Supplier Risk Exposure
        ↓
Possible cause: overbuying a Principal or category that has stopped moving
        ↓
Possible action: clearance, stop-buy, and purchasing review on the named SKUs
```

---

## IP-013

Parent:

MQ-013 Which products are losing momentum?

```text
Products slowing down
        ↓
Inventory Risk
        ↓
Inventory Aging
        ↓
Slow Moving Count & Value
        ↓
Never Sold Count & Value
        ↓
Category Risk Exposure
        ↓
Possible cause: offtake slowing, or new receipts that have not started selling
        ↓
Possible action: delay replenishment, push sell-through, or treat intake as a buying-quality problem
```

Act here before the same goods become dead stock (IP-012).

---

## IP-014

Parent:

MQ-014 Will selling products run out while other stock sits idle?

```text
Holes and piles at the same time
        ↓
Inventory Forecast / Inventory Optimization
        ↓
Inventory Coverage
        ↓
Stock-Out Risk Items / Value
        ↓
Overstock / Understock Value
        ↓
Recommended Purchase Qty vs Recoverable Capital
        ↓
Possible cause: average days of supply hiding empty selling SKUs next to idle stock
        ↓
Possible action: buy the holes, delay or clear the piles; check Locations if one warehouse holds the idle stock
```

---

## IP-015

Parent:

MQ-015 Which customers dominate omzet or piutang?

```text
Too much business in too few customers
        ↓
Customers / Customer Portfolio
        ↓
Customer Exposure Concentration
        ↓
Top Omzet Customer %
        ↓
Top Piutang Customer %
        ↓
Top 5 Customers (Critical Exposure)
        ↓
Possible cause: growth or cash sitting with a shrinking set of names
        ↓
Possible action: protect key accounts, reduce exposure, or broaden the book
```

If those same names are at risk, continue through IP-001 before increasing supply.

---

## IP-016

Parent:

MQ-016 Which suppliers are becoming dominant?

```text
Too much buying or stock in too few principals
        ↓
Purchasing Management
        ↓
Principal Dependency
        ↓
Top 1 Principal %
        ↓
Top Supplier %
        ↓
Compound Dependency Count
        ↓
Principal Exposure Comparison
        ↓
Possible cause: spend, warehouse capital, and sick stock sitting with the same Principal
        ↓
Possible action: treat as chosen strategy, or reduce buying into that Principal
```

---

## IP-017

Parent:

MQ-017 Are we too dependent on a few salespeople?

```text
Too much omzet in too few salespeople
        ↓
Salesmen
        ↓
Revenue Production
        ↓
Top Omzet Salesman %
        ↓
Top 10 Omzet
        ↓
Active Salesmen
        ↓
Possible cause: a thin active force, or stars covering a weak team
        ↓
Possible action: protect key people, rebuild team coverage, or stop reading star omzet as company health
```

If those stars also carry overdue books, continue through IP-003.

---

## IP-018

Parent:

MQ-018 Is credit policy being respected?

```text
Credit rules may be leaking
        ↓
Customers / Customer Risk Forecast
        ↓
Credit Control
        ↓
Credit Limit Signal Count
        ↓
Plafond Breach Count
        ↓
Suspended + Sales Count
        ↓
Possible cause: accounts approaching Plafond, already breaching, or suspended accounts still being billed
        ↓
Possible action: review credit, restrict supply, or stop billing suspended accounts
```

---

## IP-019

Parent:

MQ-019 Are purchased goods posted, or stuck in backlog?

```text
Purchased goods not becoming sellable stock
        ↓
Purchasing Management
        ↓
Purchase Processing
        ↓
Posted %
        ↓
Pending Posting Value
        ↓
Qualified Backlog Count & Value
        ↓
Purchasing Inactivity Flag
        ↓
Possible cause: invoices waiting to post, aged backlog, or a freeze that was never declared
        ↓
Possible action: post existing invoices first; resume buying only if selling SKUs are at stock-out risk
```

If the inactivity flag is on and Stock-Out Risk is also rising, treat the quiet month as a stall, not a freeze.

---

# TRACEABILITY

| ID | Owner concern | Path | Primary KPI |
|----|---------------|------|-------------|
| EQ-001 | Is the business healthy? | Starting path | Portfolio Health Score, Achievement %, Recovery vs Billing %, Inventory Health Score |
| EQ-002 | Where is my biggest risk? | Starting path | Alert counts, Customers At Risk, Overdue Exposure, At-Risk Inventory %, Principal At-Risk Count |
| EQ-003 | What needs immediate attention? | IP-005, IP-006 | Actions Today, Critical Actions Count |
| EQ-004 | Is cash flow healthy? | IP-007, IP-008 | Recovery vs Billing %, Overdue Exposure, Collection Gap |
| EQ-005 | Is growth sustainable? | IP-009, IP-010, IP-011 | Below Target Count, Declining Count, Visit Execution % |
| EQ-006 | Is working capital healthy? | IP-012, IP-013, IP-014 | Dead Stock, Slow Moving, Stock-Out Risk, Recoverable Capital |
| EQ-007 | Are we overly dependent? | IP-015, IP-016, IP-017 | Top Customer %, Top 1 Principal %, Top Omzet Salesman % |
| EQ-008 | What is likely to become a problem next? | IP-018, IP-019 | Credit Limit Signal, Watch / Due Within 7 Days, Pending Posting, Purchasing Inactivity Flag |
| MQ-001 | Which customers are becoming risky? | IP-001 | Customers At Risk Count, Risk Category |
| MQ-002 | Which inventory is unhealthy? | IP-002 | At-Risk Inventory % |
| MQ-003 | Which salespeople are carrying overdue or dormant books? | IP-003 | High Overdue Exposure Count, Dormant Portfolio Count |
| MQ-004 | Which principals are becoming a buying or stock risk? | IP-004 | Principal At-Risk Count, Compound Dependency Count |
| MQ-005 | Who should be contacted today? | IP-005 | Actions Today |
| MQ-006 | What should we buy, delay, transfer, or clear today? | IP-006 | Action Counts by Type |
| MQ-007 | Which receivables require attention? | IP-007 | Overdue Exposure, Top Overdue Customers |
| MQ-008 | Are we collecting as fast as we are billing? | IP-008 | Recovery vs Billing % |
| MQ-009 | Which salespeople are underperforming? | IP-009 | Below Target Count, Missing Target Setup Count |
| MQ-010 | Which customers are declining? | IP-010 | Declining Count, Purchase Decline Signal Count |
| MQ-011 | Did the field team cover the planned route and produce orders? | IP-011 | Visit Execution %, Effective Call Rate, Orders Generated |
| MQ-012 | Which inventory is becoming dead stock? | IP-012 | Dead Stock Count & Value, Top 10 Dead / Slow Moving |
| MQ-013 | Which products are losing momentum? | IP-013 | Slow Moving Count & Value, Never Sold Count & Value |
| MQ-014 | Will selling products run out while other stock sits idle? | IP-014 | Stock-Out Risk, Overstock / Understock Value |
| MQ-015 | Which customers dominate omzet or piutang? | IP-015 | Top Omzet Customer %, Top Piutang Customer % |
| MQ-016 | Which suppliers are becoming dominant? | IP-016 | Top 1 Principal %, Compound Dependency Count |
| MQ-017 | Are we too dependent on a few salespeople? | IP-017 | Top Omzet Salesman %, Active Salesmen |
| MQ-018 | Is credit policy being respected? | IP-018 | Plafond Breach Count, Credit Limit Signal Count |
| MQ-019 | Are purchased goods posted, or stuck in backlog? | IP-019 | Posted %, Qualified Backlog Count & Value |

---

# CONSOLIDATION FROM v1

v1 discovered 8 executive, 29 management, and 32 investigative questions. This version keeps owner concerns, converts drill-downs into paths, and removes analyst nodes.

## Kept as Executive Question

| v2 | v1 source |
|----|-----------|
| EQ-001 Is the business healthy? | EQ-001 |
| EQ-002 Where is my biggest risk? | EQ-002 |
| EQ-003 What needs immediate attention? | EQ-003 |
| EQ-004 Is cash flow healthy? | EQ-004, reworded |
| EQ-005 Is growth sustainable? | EQ-005 plus customer lifecycle |
| EQ-006 Is working capital healthy? | EQ-006 plus trapped piutang |
| EQ-007 Are we overly dependent…? | EQ-007 |
| EQ-008 What is likely to become a problem next? | Early-warning KPIs that were scattered across v1 IQs |

## Kept as Management Question

| v2 | v1 source |
|----|-----------|
| MQ-001 Which customers are becoming risky? | MQ-005 |
| MQ-002 Which inventory is unhealthy? | MQ-006 |
| MQ-003 Which salespeople are carrying overdue or dormant books? | MQ-007 |
| MQ-004 Which principals are becoming a buying or stock risk? | MQ-008 |
| MQ-005 Who should be contacted today? | MQ-009 |
| MQ-006 What should we buy, delay, transfer, or clear today? | MQ-010 |
| MQ-007 Which receivables require attention? | MQ-012 + MQ-013 |
| MQ-008 Are we collecting as fast as we are billing? | MQ-015 |
| MQ-009 Which salespeople are underperforming? | MQ-017 |
| MQ-010 Which customers are declining? | IQ-006, promoted |
| MQ-011 Did the field team cover the planned route and produce orders? | MQ-018 |
| MQ-012 Which inventory is becoming dead stock? | MQ-021, focused |
| MQ-013 Which products are losing momentum? | Slow-moving family from MQ-021 |
| MQ-014 Will selling products run out while other stock sits idle? | MQ-022 |
| MQ-015 Which customers dominate omzet or piutang? | MQ-024 |
| MQ-016 Which suppliers are becoming dominant? | MQ-025 |
| MQ-017 Are we too dependent on a few salespeople? | MQ-019, focused |
| MQ-018 Is credit policy being respected? | MQ-027 |
| MQ-019 Are purchased goods posted, or stuck in backlog? | MQ-028 + MQ-029 |

## Converted to Investigation Path

| v1 | Now lives in |
|----|----------------|
| MQ-001, IQ-001, IQ-002, IQ-004, IQ-005 | IP-001 |
| MQ-003, IQ-019, IQ-020 | IP-002, IP-012, IP-013 |
| IQ-032 | IP-003 |
| MQ-004, IQ-024, IQ-025 | IP-004, IP-016 |
| MQ-011, IQ-031 | EQ-002 / EQ-003 starting paths |
| IQ-007, IQ-008, IQ-009 | IP-007 |
| MQ-014, IQ-010, IQ-011 | IP-008 |
| IQ-012, IQ-013 | IP-005 |
| MQ-016, IQ-014, IQ-015 | IP-009 |
| IQ-016, IQ-017, IQ-018 | IP-011 |
| MQ-010, IQ-023 | IP-006 |
| MQ-023, IQ-021, IQ-022 | IP-014 |
| IQ-026, IQ-027 | IP-019 |
| IQ-028, IQ-029 | Continue step inside IP-014 |
| IQ-030 | IP-018 |
| MQ-001 health headlines, MQ-002, MQ-020 | EQ starting paths, not standalone questions |

## Removed as standalone questions

Removed because they were KPI-centric, analyst-centric, or only useful as a step inside a path:

- How healthy is the customer book over the next 30 days? (headline, not a navigation node)
- Is the sales team delivering against plan? (covered by MQ-009)
- Is warehouse capital healthy? (covered by EQ-006)
- Will the current sales pace close the target gap? (step in IP-008 / IP-009)
- How much capital sits in the warehouse, and in which categories? (KPI read, not an owner question)
- How much idle capital could still be recovered? (step in IP-006 / IP-014)
- Does one warehouse or wilayah hold too much stock? (continue step, not a door question)
- Forecast confidence questions
- Payment mix questions
- GPS validity as a standalone question
- Aging-bucket questions
- Alert-domain questions as a management concern (alerts are the starting path, not the question)

No new business questions were added. EQ-005, EQ-006, and EQ-008 are owner-language consolidations of concerns already present in v1.

---

# Use in Phase-4

Phase-4 should map one playbook page per management question:

```text
Executive Question
        ↓
Management Question
        ↓
Investigation Path
        ↓
Dashboard
        ↓
Data Source
        ↓
KPI
        ↓
Next KPI
        ↓
Possible Cause
        ↓
Possible Action
```

Start from EQ-001. Do not start from a dashboard list.

Counts for navigation:

| Layer | Count |
|-------|-------|
| Executive questions | 8 |
| Management questions | 19 |
| Investigation paths | 19 |

Every management question has exactly one path. Every path ends in a possible action.
