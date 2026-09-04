# BUSINESS QUESTION CATALOG v3

**Role:** Owner Navigation Foundation  
**Audience:** Business Owner, Director, Department Head  
**Purpose:** Question → investigation steps → decision. Direct input for Phase-4 Navigation Mapping.  
**Primary source:** [business-question-catalog-v2.md](./business-question-catalog-v2.md)  
**Supporting sources:** [kpi-inventory.md](./kpi-inventory.md), [kpi-encyclopedia.html](./kpi-encyclopedia.html)  
**Date:** 2 September 2026

This refinement does not add KPIs, dashboards, or new business concerns. It tightens hierarchy, turns remaining investigative questions into steps, and connects every management question to a trigger, a starting KPI, and a decision.

---

# How an owner uses this catalog

```text
Open the portal
        ↓
Ask one Executive Question
        ↓
Open the matching Management Question
        ↓
Follow the five investigation steps
        ↓
Decide
```

There is no Investigative Question layer. Drill-downs are steps, not questions.

---

# Owner journey

```text
Owner opens the portal
        ↓
EQ-001  Is the business healthy?
        │
        ├─ Risk stands out                 → EQ-002
        ├─ Something must be done today    → EQ-003
        ├─ Cash feels tight                → EQ-004
        ├─ Sales feel weak                 → EQ-005
        ├─ Warehouse capital feels stuck   → EQ-006
        ├─ A few names run the business    → EQ-007
        └─ Signals are forming, totals still look fine → EQ-008
```

---

# Hierarchy map

```text
EQ-001  Is the business healthy?

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

EQ-006  Is working capital healthy?
        MQ-011  Is warehouse capital trapped or about to run out?

EQ-007  Are we overly dependent on specific customers, suppliers, or salespeople?
        MQ-012  Which customers dominate omzet or piutang?
        MQ-013  Which suppliers are becoming dominant?
        MQ-014  Are we too dependent on a few salespeople?

EQ-008  What is likely to become a problem next?
        MQ-015  Is credit policy being respected?
        MQ-016  Are purchased goods posted, or stuck in backlog?
```

---

# EQ-001

Question:

Is the business healthy?

Why This Matters:

The owner needs one morning judgment before opening any other screen. Customer quality, sales delivery, cash conversion, and warehouse capital can move in different directions on the same day.

Investigation Path:

- Step 1: Read Domain Attention Summary Cards on Executive
- Step 2: Read Portfolio Health Score
- Step 3: Read Achievement %
- Step 4: Read Recovery vs Billing % and Inventory Health Score
- Step 5: Open the weakest of EQ-002 through EQ-008

Supported By:

- Business Intent: Cross-area business health
- Data Source: Executive attention; Customer Risk Assessment; Sales Target Achievement; Collection Recovery; Inventory Health
- KPI: Domain Attention Summary Cards; Portfolio Health Score; Achievement %; Recovery vs Billing %; Inventory Health Score

---

# EQ-002

Question:

Where is my biggest risk?

Why This Matters:

Attention is scarce. Overdue customers, stuck stock, weak sales books, and Principal dependence can all look urgent. The owner must see which problem is largest before assigning work.

---

## MQ-001

Question:

Which customers are becoming risky?

Why This Matters:

Collection time and credit attention should go to accounts that are weakening, not only to accounts that are already overdue.

Typical Trigger:

- Collection slowing
- Cash flow pressure
- Portfolio health weakening
- Customer complaints about supply holds

Expected Decision:

- Increase collection effort, review customer credit, or recover the named accounts

Investigation Starts With:

Customers At Risk Count

Investigation Path:

- Step 1: Identify how many customers sit in Watch, Attention, High Risk, and Critical
- Step 2: Identify whether the important customers are the ones at risk
- Step 3: Identify why the risk fired — payment delay, inactivity, purchase decline, or credit-limit pressure
- Step 4: Identify the named customers with the largest overdue or outstanding
- Step 5: Determine corrective action — collect, restrict supply, or recover sales

Supported By:

- Business Intent: Kesehatan Portofolio Customer
- Data Source: Customer Risk Assessment
- KPI: Customers At Risk Count; Risk Category Counts; Strategic Customers At Risk Count; Payment Delay Signal Count; Inactivity Signal Count; Purchase Decline Signal Count; Credit Limit Signal Count; Top Overdue Customers; Overdue Exposure

---

## MQ-002

Question:

Which inventory is unhealthy?

Why This Matters:

Stuck stock is trapped cash. The owner must see the unhealthy share before releasing more purchase cash.

Typical Trigger:

- Inventory increasing
- Warehouse feels full
- Purchasing asking to buy more
- Sales flat while stock rises

Expected Decision:

- Reduce purchasing, accelerate inventory liquidation, or stop replenishing unhealthy lines

Investigation Starts With:

At-Risk Inventory %

Investigation Path:

- Step 1: Identify the unhealthy share of warehouse capital
- Step 2: Identify worst dead and slow moving SKUs
- Step 3: Identify categories owning the stock
- Step 4: Identify suppliers owning the stock
- Step 5: Determine corrective action — delay buy, clear, or review purchasing

Supported By:

- Business Intent: Risiko Persediaan
- Data Source: Inventory Exposure; Inventory Aging; Critical Inventory
- KPI: At-Risk Inventory %; Aging Distribution; Dead Stock Count & Value; Slow Moving Count & Value; Never Sold Count & Value; Top 10 Dead / Slow Moving; Category Risk Exposure; Supplier Risk Exposure

---

## MQ-003

Question:

Which salespeople are carrying overdue or dormant books?

Why This Matters:

A salesperson’s omzet can look strong while the owned book is late or going quiet. Those books become next month’s collection problem.

Typical Trigger:

- Collection slowing on certain routes
- Dormant customers rising
- High omzet names with high piutang

Expected Decision:

- Reallocate sales resources toward collection and recovery on the named books

Investigation Starts With:

High Overdue Exposure Count

Investigation Path:

- Step 1: Identify salespeople with high overdue exposure
- Step 2: Separate large outstanding from actually late
- Step 3: Identify salespeople with dormant portfolios
- Step 4: Identify the named overdue books
- Step 5: Determine corrective action — joint Sales–Finance review of those books

Supported By:

- Business Intent: Kualitas Portofolio Customer (Salesman)
- Data Source: Customer Exposure; Customer Retention
- KPI: High Overdue Exposure Count; High Piutang Exposure Count; Dormant Portfolio Count; Top Overdue Salesmen; Top Piutang Salesman %

---

## MQ-004

Question:

Which principals are becoming a buying or stock risk?

Why This Matters:

Continuing to buy a Principal whose goods are already aging compounds trapped capital.

Typical Trigger:

- Inventory increasing on one Principal
- Supplier dependency growing
- Purchasing still placing orders on slow lines

Expected Decision:

- Reduce purchasing from that Principal, or keep buying only if the dependence is chosen strategy

Investigation Starts With:

Principal At-Risk Count

Investigation Path:

- Step 1: Identify principals already at risk in the warehouse
- Step 2: Compare spend, stock holding, and sick stock on the same names
- Step 3: Identify whether dependence is spend, stock, or both
- Step 4: Identify whether purchasing is still feeding the same risk
- Step 5: Determine corrective action — delay buy, review Principal, or accept as strategy

Supported By:

- Business Intent: Risiko Supplier; Ketergantungan Supplier
- Data Source: Supplier Risk Monitoring; Principal Dependency
- KPI: Principal At-Risk Count; Supplier Risk Exposure; Principal Exposure Comparison; Compound Dependency Count; Top 10 Principal (Ranking)

---

# EQ-003

Question:

What needs immediate attention?

Why This Matters:

Monthly totals do not tell the owner what must happen before the day ends. Today’s work is contacts, buying, delaying, or clearing.

---

## MQ-005

Question:

Who should be contacted today?

Why This Matters:

Not every weak account needs the same work. Mixing collection, reminders, credit review, and sales recovery on one undifferentiated list wastes the day.

Typical Trigger:

- Start of day
- Collection slowing
- Cash flow pressure

Expected Decision:

- Increase collection effort on named accounts, or split the day by contact type

Investigation Starts With:

Actions Today

Investigation Path:

- Step 1: Identify today’s action volume
- Step 2: Split work into collect, remind, review credit, recover sales, or escalate
- Step 3: Identify cash due within 7 days versus legacy debt
- Step 4: Identify impact of immediate contacts
- Step 5: Determine corrective action — run today’s list by type

Supported By:

- Business Intent: Recovery Action Management
- Data Source: Collection Action Queue; Collection Planning
- KPI: Actions Today; Immediate Collection Count; Proactive Reminder Count; Credit Review Count; Sales Recovery Count; Management Escalation Count; Due Within 7 Days; Legacy Debt Count

---

## MQ-006

Question:

What should we buy, delay, transfer, or clear today?

Why This Matters:

Holes and excess often exist at the same time. Buying everything that feels empty while slow movers age traps cash.

Typical Trigger:

- Warehouse asking what to buy
- Inventory increasing
- Selling items running out

Expected Decision:

- Reduce purchasing, accelerate inventory liquidation, or transfer stock — as separate actions

Investigation Starts With:

Critical Actions Count

Investigation Path:

- Step 1: Identify critical inventory actions
- Step 2: Split actions into purchase, delay, transfer, or clearance
- Step 3: Identify stock-out holes versus recoverable idle capital
- Step 4: Compare recommended purchase budget with capital that could still be recovered
- Step 5: Determine corrective action — buy holes, delay piles, transfer, or clear

Supported By:

- Business Intent: Optimasi Persediaan
- Data Source: Inventory Action Queue; Replenishment Planning; Capital Recovery
- KPI: Critical Actions Count; Action Counts by Type; Stock-Out Risk Items / Value; Recoverable Capital; Recommended Purchase Budget

---

# EQ-004

Question:

Is cash flow healthy?

Why This Matters:

Invoiced omzet is not cash. If billing outruns collection, the next purchase cycle is funded with delayed money.

---

## MQ-007

Question:

Which receivables require attention?

Why This Matters:

Total piutang is scale. Overdue is urgency. Names are what collectors can work.

Typical Trigger:

- Cash flow pressure
- Collection slowing
- Piutang growing

Expected Decision:

- Increase collection effort on named overdue accounts; restrict supply if aging is chronic

Investigation Starts With:

Overdue Exposure

Investigation Path:

- Step 1: Separate total piutang from overdue
- Step 2: Identify how much is already more than 90 days old
- Step 3: Identify the named overdue customers
- Step 4: Identify whether overdue sits with a few salespeople or wilayah
- Step 5: Determine corrective action — collect named accounts or restrict supply

Supported By:

- Business Intent: Risiko Piutang & Collection
- Data Source: Piutang Exposure
- KPI: Total Piutang; Overdue Exposure; Piutang > 90 Hari; Top Overdue Customers; Overdue Concentration %; Top Overdue Salesmen; Top Overdue Wilayah; Legacy Debt Count

---

## MQ-008

Question:

Are we collecting as fast as we are billing?

Why This Matters:

If new Faktur keep outrunning cash in, the book grows even when collectors are busy.

Typical Trigger:

- Cash flow pressure
- Omzet looks strong but cash is tight
- Month-end cash worry

Expected Decision:

- Increase collection effort, slow new credit, or accept this month’s cash finish

Investigation Starts With:

Recovery vs Billing %

Investigation Path:

- Step 1: Compare cash collected with billed sales
- Step 2: Identify the remaining collection gap
- Step 3: Identify required daily collection versus current pace
- Step 4: Identify whether remaining days can still close the month
- Step 5: Determine corrective action — push collection, slow credit, or accept the finish

Supported By:

- Business Intent: Pemulihan Collection
- Data Source: Collection Recovery; Cash Flow Forecast
- KPI: Recovery vs Billing %; Cash Collected MTD; Collection Gap; Required Daily Collection; Daily Cash Collection Average; Expected Cash Collection; Scenario Cash (Best / Expected / Worst)

---

# EQ-005

Question:

Is growth sustainable?

Why This Matters:

A month can hit target on a few names while the rest of the book goes quiet. The owner needs to know whether growth will still be there after this month.

---

## MQ-009

Question:

Which salespeople are underperforming?

Why This Matters:

Company omzet can be carried by a few names. “Below target” and “no target” are different failures. Coaching the wrong cause wastes the week.

Typical Trigger:

- Sales decline
- Target gap widening
- A few people carrying the month

Expected Decision:

- Reallocate sales resources, set missing targets, or coach named people on coverage versus conversion

Investigation Starts With:

Below Target Count

Investigation Path:

- Step 1: Identify who is below a real target versus who has no target
- Step 2: Identify whether the miss is a Principal-mix problem
- Step 3: Identify whether the field team covered the planned route
- Step 4: Identify whether visits produced orders, or whether check-ins are not credible
- Step 5: Determine corrective action — set targets, fix coverage, fix conversion, or coach named people

Supported By:

- Business Intent: Pencapaian Target Penjualan; Produktivitas Lapangan
- Data Source: Sales Target Achievement; Visit Execution; Sales Call Effectiveness; Order Generation
- KPI: Below Target Count; Missing Target Setup Count; Principal Achievement Table; Visit Execution %; Missed Visits; Effective Call Rate; GPS Valid Rate; Orders Generated; Omzet Generated

---

## MQ-010

Question:

Which customers are declining?

Why This Matters:

A customer who is still current but buying less is next month’s dormant account. Recovery is cheaper than winning a new name.

Typical Trigger:

- Sales decline
- Book feels quieter
- Strategic accounts buying less

Expected Decision:

- Increase sales focus on declining names, or stop covering accounts that will not return

Investigation Starts With:

Declining Count

Investigation Path:

- Step 1: Identify declining customers
- Step 2: Identify purchase-decline and inactivity signals before 90-day dormant
- Step 3: Identify dormant and never-purchased accounts
- Step 4: Identify whether declining names are strategic
- Step 5: Determine corrective action — recover, protect, or stop covering

Supported By:

- Business Intent: Customer Lifecycle
- Data Source: Customer Lifecycle; Strategic Customer Portfolio
- KPI: Declining Count; Purchase Decline Signal Count; Inactivity Signal Count; Dormant Count; Never Purchased Count; Strategic Customer Count; Strategic At Risk Count

---

# EQ-006

Question:

Is working capital healthy?

Why This Matters:

Cash sitting in dead stock and cash missing from empty selling SKUs are the same owner problem: money that cannot fund the next cycle in the right place.

---

## MQ-011

Question:

Is warehouse capital trapped or about to run out?

Why This Matters:

The company can be overstocked on what does not sell and understocked on what does. Those two conditions need opposite actions in the same week.

Typical Trigger:

- Inventory increasing
- Warehouse “full” while selling items run out
- Sales lost to empty shelves

Expected Decision:

- Accelerate inventory liquidation on idle stock, and buy only the holes

Investigation Starts With:

Dead Stock Value / Stock-Out Risk Items

Investigation Path:

- Step 1: Identify dead, slow, and never-sold capital
- Step 2: Identify worst SKUs and whether one warehouse holds the idle stock
- Step 3: Identify selling SKUs at stock-out risk
- Step 4: Compare recommended purchase budget with recoverable capital
- Step 5: Determine corrective action — clear or delay piles, buy holes, transfer if location is the problem

Supported By:

- Business Intent: Risiko Persediaan; Kesehatan & Forecast Persediaan; Optimasi Persediaan
- Data Source: Inventory Aging; Inventory Coverage; Capital Recovery; Locations
- KPI: Dead Stock Count & Value; Slow Moving Count & Value; Stock-Out Risk Items / Value; Overstock / Understock Value; Recoverable Capital; Recommended Purchase Budget; Top 1 Warehouse At-Risk %; Inactive Warehouse With Stock Count

---

# EQ-007

Question:

Are we overly dependent on specific customers, suppliers, or salespeople?

Why This Matters:

A book, a sales force, or a warehouse can look healthy in total and still be one relationship away from a shock.

---

## MQ-012

Question:

Which customers dominate omzet or piutang?

Why This Matters:

Growth or cash sitting with a shrinking set of names is concentration risk. Losing one name would change the month.

Typical Trigger:

- One customer dominates the conversation
- Fear of losing a key account
- Piutang looks large because of a few names

Expected Decision:

- Reduce exposure, protect key accounts, or broaden the book

Investigation Starts With:

Top Piutang Customer % / Top Omzet Customer %

Investigation Path:

- Step 1: Identify omzet concentration
- Step 2: Identify piutang concentration
- Step 3: Identify the named critical-exposure customers
- Step 4: Identify whether those same names are already at risk
- Step 5: Determine corrective action — protect, reduce exposure, or broaden the book

Supported By:

- Business Intent: Kontribusi Customer Terhadap Bisnis
- Data Source: Customer Revenue Contribution; Customer Exposure Concentration
- KPI: Top Omzet Customer %; Top Piutang Customer %; Top Customer % (Piutang Concentration); Top 5 Customers (Critical Exposure); Strategic Customer Count

---

## MQ-013

Question:

Which suppliers are becoming dominant?

Why This Matters:

A Principal can take this month’s purchase cash, already hold the warehouse, and own the sick stock. Those are three different kinds of dependence.

Typical Trigger:

- Supplier dependency growing
- One Principal dominates purchasing talk
- Stock and risk sitting with the same name

Expected Decision:

- Diversify supplier exposure, or treat the dependence as chosen strategy

Investigation Starts With:

Top 1 Principal %

Investigation Path:

- Step 1: Identify who takes most purchase cash
- Step 2: Identify who already holds most warehouse capital
- Step 3: Identify compound dependence — spend and stock on the same Principal
- Step 4: Identify whether that Principal also owns sick stock
- Step 5: Determine corrective action — diversify, delay buy, or accept as strategy

Supported By:

- Business Intent: Ketergantungan Supplier
- Data Source: Principal Dependency; Supplier Dependency
- KPI: Top 1 Principal %; Top 3 Principal %; Top Supplier % (Inventory); Compound Dependency Count; Principal Exposure Comparison; Grand Total Purchase

---

## MQ-014

Question:

Are we too dependent on a few salespeople?

Why This Matters:

If one or two people produce most omzet, next month’s plan is a people risk, not a market risk.

Typical Trigger:

- Sales decline if one person is absent
- A star salesperson dominates results
- Active force feels thin

Expected Decision:

- Reallocate sales resources and rebuild coverage across the team

Investigation Starts With:

Top Omzet Salesman %

Investigation Path:

- Step 1: Identify omzet concentration among salespeople
- Step 2: Identify how large the active force is
- Step 3: Identify whether stars also carry overdue books
- Step 4: Identify whether the rest of the team is below target
- Step 5: Determine corrective action — protect key people, rebuild team coverage

Supported By:

- Business Intent: Revenue Production; Kapasitas Tim Sales
- Data Source: Revenue Production; Sales Force Capacity
- KPI: Top Omzet Salesman %; Top 10 Omzet; Active Salesmen; Below Target Count; High Overdue Exposure Count

---

# EQ-008

Question:

What is likely to become a problem next?

Why This Matters:

Overdue, dormant, dead stock, and Plafond breaches appear after the damage. The owner wants the signals that are forming now.

---

## MQ-015

Question:

Is credit policy being respected?

Why This Matters:

Plafond and hold rules only protect cash if they are observed. A credit-limit signal is still preventable. A breach, or a suspended account still being billed, is already a failure.

Typical Trigger:

- Cash flow pressure
- Customers buying above comfort
- Collection slowing on large accounts

Expected Decision:

- Review customer credit, restrict supply, or stop billing suspended accounts

Investigation Starts With:

Credit Limit Signal Count

Investigation Path:

- Step 1: Identify accounts approaching Plafond
- Step 2: Identify accounts that already breached
- Step 3: Identify suspended accounts still being billed
- Step 4: Identify payment-delay and collection-risk signals on the same names
- Step 5: Determine corrective action — review credit, restrict supply, or stop billing

Supported By:

- Business Intent: Credit & Compliance
- Data Source: Credit Control; Payment Discipline
- KPI: Credit Limit Signal Count; Plafond Breach Count; Suspended + Sales Count; Payment Delay Signal Count; Collection Risk Signal Count

---

## MQ-016

Question:

Are purchased goods posted, or stuck in backlog?

Why This Matters:

Unposted purchases mean the company has already spent or committed without gaining sellable stock. Quiet purchasing late in the month can be a freeze or a stall.

Typical Trigger:

- Warehouse waiting for stock that was already bought
- Purchasing gone quiet
- Selling SKUs at risk of running out

Expected Decision:

- Post existing invoices first; resume buying only if selling SKUs are at stock-out risk

Investigation Starts With:

Pending Posting Value / Posted %

Investigation Path:

- Step 1: Identify how much purchasing is still unposted
- Step 2: Identify aged qualified backlog
- Step 3: Identify whether purchasing has gone quiet after mid-month
- Step 4: Identify whether selling SKUs are at stock-out risk during that quiet
- Step 5: Determine corrective action — post first, declare a freeze, or resume buying the holes

Supported By:

- Business Intent: Kualitas Operasional Purchasing
- Data Source: Purchase Processing; Purchasing Backlog; Purchasing Activity
- KPI: Posted %; Pending Posting Value; Qualified Backlog Count & Value; Purchasing Inactivity Flag; Stock-Out Risk Items / Value

---

# TRACEABILITY

| ID | Question | Investigation Starts With |
|----|----------|---------------------------|
| EQ-001 | Is the business healthy? | Domain Attention Summary Cards |
| EQ-002 | Where is my biggest risk? | Alert counts / Customers At Risk Count |
| EQ-003 | What needs immediate attention? | Actions Today / Critical Actions Count |
| EQ-004 | Is cash flow healthy? | Recovery vs Billing % |
| EQ-005 | Is growth sustainable? | Below Target Count / Declining Count |
| EQ-006 | Is working capital healthy? | Dead Stock Value / Stock-Out Risk Items |
| EQ-007 | Are we overly dependent? | Top Customer % / Top 1 Principal % / Top Omzet Salesman % |
| EQ-008 | What is likely to become a problem next? | Credit Limit Signal Count / Pending Posting Value |
| MQ-001 | Which customers are becoming risky? | Customers At Risk Count |
| MQ-002 | Which inventory is unhealthy? | At-Risk Inventory % |
| MQ-003 | Which salespeople are carrying overdue or dormant books? | High Overdue Exposure Count |
| MQ-004 | Which principals are becoming a buying or stock risk? | Principal At-Risk Count |
| MQ-005 | Who should be contacted today? | Actions Today |
| MQ-006 | What should we buy, delay, transfer, or clear today? | Critical Actions Count |
| MQ-007 | Which receivables require attention? | Overdue Exposure |
| MQ-008 | Are we collecting as fast as we are billing? | Recovery vs Billing % |
| MQ-009 | Which salespeople are underperforming? | Below Target Count |
| MQ-010 | Which customers are declining? | Declining Count |
| MQ-011 | Is warehouse capital trapped or about to run out? | Dead Stock Value / Stock-Out Risk Items |
| MQ-012 | Which customers dominate omzet or piutang? | Top Piutang Customer % / Top Omzet Customer % |
| MQ-013 | Which suppliers are becoming dominant? | Top 1 Principal % |
| MQ-014 | Are we too dependent on a few salespeople? | Top Omzet Salesman % |
| MQ-015 | Is credit policy being respected? | Credit Limit Signal Count |
| MQ-016 | Are purchased goods posted, or stuck in backlog? | Pending Posting Value / Posted % |

---

# WHAT CHANGED FROM v2

## Hierarchy

| Change | Why |
|--------|-----|
| Management questions sit under their parent executive question | Owner reads one concern and its paths without jumping sections |
| No Investigative Question layer | Remaining IQs were drill-downs, not owner questions |
| EQ-001 is a router, not a parent of weak health headlines | Avoids “business health → credit-limit usage” style mismatches |

## Merged (fewer, stronger)

| v2 | v3 |
|----|----|
| MQ-012 dead stock + MQ-013 losing momentum + MQ-014 holes vs piles | MQ-011 Is warehouse capital trapped or about to run out? |
| MQ-011 Did the field team cover the route? | Steps 3–4 under MQ-009 (why salespeople underperform) |
| MQ-002 unhealthy inventory vs separate dead-stock question | Dead / slow / never-sold are steps under MQ-002 and MQ-011 |

## Converted from IQ / question to investigation step

| Former question-style node | Now |
|----------------------------|-----|
| Why is this customer risky? | MQ-001 Step 3 |
| Which named customers carry the most overdue? | MQ-001 Step 4 |
| Which SKUs are the worst dead or slow movers? | MQ-002 Step 2 |
| Which categories / suppliers own the sick stock? | MQ-002 Steps 3–4 |
| Where is overdue sitting by age? | MQ-007 Step 2 |
| Which salespeople or wilayah drive overdue? | MQ-007 Step 4 |
| What daily collection pace is required? | MQ-008 Steps 3–4 |
| Planned vs missed visits / GPS credibility | MQ-009 Steps 3–4 |
| Are field orders turning into omzet? | MQ-009 Step 4 |
| What purchase budget vs recoverable capital? | MQ-006 Step 4 and MQ-011 Step 4 |
| Is Principal dependence spend, stock, or both? | MQ-013 Steps 1–3 |
| One warehouse holding idle stock | MQ-011 Step 2 |
| Plafond approach vs breach | MQ-015 Steps 1–2 |
| Unposted vs aged backlog vs inactivity | MQ-016 Steps 1–3 |

No investigative question was kept as a standalone IQ. Each failed the owner-first test: an owner would not ask it as a door question; they would do it as a step.

## Counts

| Layer | v2 | v3 |
|-------|----|----|
| Executive questions | 8 | 8 |
| Management questions | 19 | 16 |
| Investigation paths | 19 flows | 16 paths × 5 steps |
| Investigative questions | 0 (already converted) | 0 |

---

# Use in Phase-4

Map one playbook page per management question:

```text
Executive Question
        ↓
Management Question
        ↓
Investigation Starts With
        ↓
Step 1
        ↓
Step 2
        ↓
Step 3
        ↓
Step 4
        ↓
Step 5  Determine corrective action
        ↓
Expected Decision
```

Start from EQ-001. Do not start from a dashboard list.
