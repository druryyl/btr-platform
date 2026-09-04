# QUESTION NAVIGATION MAP

**Role:** Business Navigation Architect  
**Audience:** Business Owner, Director, Department Head  
**Purpose:** From a business question, tell the owner where to go, what to look at, how to interpret it, where evidence lives, and what decision is usually made.  
**Sources of truth:** [business-question-catalog-v3.md](./business-question-catalog-v3.md), [navigation-assets.md](./navigation-assets.md)  
**Date:** 2 September 2026

This is an **investigation map**, not a page-to-page drill-down map, and not the final Navigation Playbook.

```text
Question
    ↓
Dashboard
    ↓
KPI
    ↓
Interpretation
    ↓
Evidence Source
    ↓
Decision
```

**Naming rule:** Questions and investigation intent come from the catalog. Screen titles, widgets, KPIs, and evidence come from the asset registry (on-screen names). Catalog KPI phrases that are not a fixed widget title are mapped to the closest discovered widget; they are not treated as extra dashboards.

**Not used:** Desktop Report, Desktop Inquiry, and Export Data were not discovered in Phase-4A. Evidence is portal reports, portal tables, performance profiles, investigation workspace, and the salesman detail drawer.

---

# How an owner uses this map

```text
Open the portal
        ↓
EQ-001  Is the business healthy?
        ↓
Open the weakest Executive Question
        ↓
Open the matching Management Question
        ↓
Follow Open → Review → Interpret → Evidence → Decide
```

---

# SECTION 1
# EXECUTIVE NAVIGATION MAP

EQ-001 is the morning router. EQ-002 through EQ-008 are the investigation doors. Management questions sit under those doors.

---

## EQ-001

Question:

Is the business healthy?

Purpose:

One morning judgment before opening any other screen. Customer quality, sales delivery, cash conversion, and warehouse capital can move in different directions on the same day.

Open first:

Menu **Executive** → page **Management Attention Center**

Read first:

- Domain Summaries
- Sales attention: Achievement %
- Piutang attention: Total Piutang, Overdue Customer, > 90 Day Amount
- Purchasing attention: Pending Posting, Top Principal %
- Inventory attention: Total Inventory Value, Top Category %, Top Supplier %
- Portfolio Healthy %, Customers At Risk, Strategic At Risk

Then, if needed:

- Portfolio Health Score on **Customer Portfolio Dashboard** or **Customer Risk Forecast Dashboard**
- Recovery vs Billing % on **Collection Dashboard**
- Inventory Health Score on **Inventory Forecast Dashboard**

Navigation Flow:

```text
EQ-001  Is the business healthy?
    ↓
Weakest domain on Management Attention Center
    ↓
EQ-002  risk stands out
EQ-003  something must be done today
EQ-004  cash feels tight
EQ-005  sales feel weak
EQ-006  warehouse capital feels stuck
EQ-007  a few names run the business
EQ-008  signals are forming, totals still look fine
```

Supported By:

- MQ-001, MQ-002, MQ-003, MQ-004 (via EQ-002)
- MQ-005, MQ-006 (via EQ-003)
- MQ-007, MQ-008 (via EQ-004)
- MQ-009, MQ-010 (via EQ-005)
- MQ-011 (via EQ-006)
- MQ-012, MQ-013, MQ-014 (via EQ-007)
- MQ-015, MQ-016 (via EQ-008)

---

## EQ-002

Question:

Where is my biggest risk?

Purpose:

Attention is scarce. Overdue customers, stuck stock, weak sales books, and Principal dependence can all look urgent. The owner must see which problem is largest before assigning work.

Navigation Flow:

```text
EQ-002  Where is my biggest risk?
    ↓
MQ-001  Which customers are becoming risky?
MQ-002  Which inventory is unhealthy?
MQ-003  Which salespeople are carrying overdue or dormant books?
MQ-004  Which principals are becoming a buying or stock risk?
```

Supported By:

- MQ-001
- MQ-002
- MQ-003
- MQ-004

Compare the four starting KPIs (Customers At Risk, At-Risk Inventory %, High Overdue Exposure, Inventory Cross-Risk / Principal Dependency) and investigate the largest problem first.

---

## EQ-003

Question:

What needs immediate attention?

Purpose:

Monthly totals do not tell the owner what must happen before the day ends. Today’s work is contacts, buying, delaying, or clearing.

Navigation Flow:

```text
EQ-003  What needs immediate attention?
    ↓
MQ-005  Who should be contacted today?
MQ-006  What should we buy, delay, transfer, or clear today?
```

Supported By:

- MQ-005
- MQ-006

---

## EQ-004

Question:

Is cash flow healthy?

Purpose:

Invoiced omzet is not cash. If billing outruns collection, the next purchase cycle is funded with delayed money.

Navigation Flow:

```text
EQ-004  Is cash flow healthy?
    ↓
MQ-007  Which receivables require attention?
MQ-008  Are we collecting as fast as we are billing?
```

Supported By:

- MQ-007
- MQ-008

---

## EQ-005

Question:

Is growth sustainable?

Purpose:

A month can hit target on a few names while the rest of the book goes quiet. The owner needs to know whether growth will still be there after this month.

Navigation Flow:

```text
EQ-005  Is growth sustainable?
    ↓
MQ-009  Which salespeople are underperforming?
MQ-010  Which customers are declining?
```

Supported By:

- MQ-009
- MQ-010

---

## EQ-006

Question:

Is working capital healthy?

Purpose:

Cash sitting in dead stock and cash missing from empty selling SKUs are the same owner problem: money that cannot fund the next cycle in the right place.

Navigation Flow:

```text
EQ-006  Is working capital healthy?
    ↓
MQ-011  Is warehouse capital trapped or about to run out?
```

Supported By:

- MQ-011

---

## EQ-007

Question:

Are we overly dependent on specific customers, suppliers, or salespeople?

Purpose:

A book, a sales force, or a warehouse can look healthy in total and still be one relationship away from a shock.

Navigation Flow:

```text
EQ-007  Are we overly dependent?
    ↓
MQ-012  Which customers dominate omzet or piutang?
MQ-013  Which suppliers are becoming dominant?
MQ-014  Are we too dependent on a few salespeople?
```

Supported By:

- MQ-012
- MQ-013
- MQ-014

---

## EQ-008

Question:

What is likely to become a problem next?

Purpose:

Overdue, dormant, dead stock, and Plafond breaches appear after the damage. The owner wants the signals that are forming now.

Navigation Flow:

```text
EQ-008  What is likely to become a problem next?
    ↓
MQ-015  Is credit policy being respected?
MQ-016  Are purchased goods posted, or stuck in backlog?
```

Supported By:

- MQ-015
- MQ-016

---

# SECTION 2
# MANAGEMENT NAVIGATION MAP

---

## MQ-001

Question:

Which customers are becoming risky?

Why Owner Asks This:

Collection time and credit attention should go to accounts that are weakening, not only to accounts that are already overdue.

Typical Trigger:

- Collection slowing
- Cash flow pressure
- Portfolio health weakening
- Customer complaints about supply holds

---

### Step 1
### Open

Dashboard: Customer Risk Forecast Dashboard  
Page: Menu **Customers** → **Customer Risk Forecast**

(Also visible as Customers At Risk on Management Attention Center and Customer Portfolio Dashboard. Start on Customer Risk Forecast because categories and signal families live there.)

---

### Step 2
### Review

Primary KPI: Customers At Risk  

Supporting KPI: Portfolio Health Score; Healthy / Watch / Attention / High Risk (and High / Critical Customers); Strategic At Risk; Payment Delay; Credit Limit; Inactivity; Purchase Decline; Elevated Risk Receivable

---

### Step 3
### Interpret

Healthy Signal:

Most of the book sits in Healthy. Customers At Risk and Strategic At Risk are not the story of the day. Signal families are quiet.

Warning Signal:

Watch and Attention are filling. Payment Delay, Inactivity, or Purchase Decline is rising while totals still look manageable. Strategic customers appear in the at-risk count.

Critical Signal:

High Risk / Critical Customers and Elevated Risk Receivable are material versus Total Piutang. Strategic At Risk is no longer a small exception.

(Do not invent numeric cut-offs. Use the risk categories and signal families the page already shows.)

---

### Step 4
### Gather Evidence

Evidence Sources:

- Customer Risk Attention List (portal table)
- Top Customers by Risk Priority (portal table)
- Customer Performance Profile
- Customer Report
- Piutang Report (footer)
- Sales Report (footer)

Purpose:

Name the customers. See whether the important names are the ones at risk. See why the signal fired — payment delay, inactivity, purchase decline, or credit-limit pressure. Open invoices and recent faktur as supporting records.

---

### Step 5
### Decide

Typical Decisions:

- Increase collection effort on the named accounts
- Review customer credit
- Recover sales on declining or inactive names
- Restrict supply where credit or overdue is already chronic

---

### Investigation Path

```text
Which customers are becoming risky?
    ↓
Customer Risk Forecast Dashboard
    ↓
Customers At Risk
    ↓
Risk categories + signal families + Strategic At Risk
    ↓
Customer Risk Attention List → Customer Performance Profile → Piutang Report / Sales Report
    ↓
Collect, restrict supply, or recover sales
```

---

## MQ-002

Question:

Which inventory is unhealthy?

Why Owner Asks This:

Stuck stock is trapped cash. The owner must see the unhealthy share before releasing more purchase cash.

Typical Trigger:

- Inventory increasing
- Warehouse feels full
- Purchasing asking to buy more
- Sales flat while stock rises

---

### Step 1
### Open

Dashboard: Slow Moving & Dead Stock Dashboard  
Page: Menu **Inventory** → **Inventory Risk**

(Alert Center Inventory Risk Summary is a shortcut to the same problem: Dead Stock, Slow Moving, Never Sold, At-Risk Inventory.)

---

### Step 2
### Review

Primary KPI: At-Risk Inventory %  

Supporting KPI: Dead Stock Item Count; Dead Stock Value; Slow Moving Item Count; Slow Moving Value; Inventory Aging Distribution (Active / Slow Moving / Dead Stock / Never Sold style buckets); Category Risk Exposure; Supplier Risk Exposure

---

### Step 3
### Interpret

Healthy Signal:

At-Risk Inventory % is not the dominant share. Aging sits in Active. Dead Stock and Slow Moving are small in both count and value.

Warning Signal:

Slow Moving Value is rising. Category Risk Exposure or Supplier Risk Exposure is concentrating in a few names. Never Sold appears in aging or on Alert Center.

Critical Signal:

Dead Stock Value and At-Risk Inventory % dominate warehouse capital. The same suppliers or categories own the sick stock.

---

### Step 4
### Gather Evidence

Evidence Sources:

- Inventory Attention List (portal table; filter Dead Stock / Slow Moving)
- Top 10 Dead Stock by Value; Top 10 Slow Moving by Value (ranking)
- Item Performance Profile
- Inventory Report
- Inventory Dashboard (category / supplier concentration context)

Purpose:

Name the worst SKUs. See which categories and suppliers own the stock. Confirm warehouse rows on Inventory Report.

---

### Step 5
### Decide

Typical Decisions:

- Reduce purchasing on unhealthy lines
- Accelerate inventory liquidation / clearance
- Stop replenishing dead or slow movers

---

### Investigation Path

```text
Which inventory is unhealthy?
    ↓
Slow Moving & Dead Stock Dashboard
    ↓
At-Risk Inventory %
    ↓
Dead Stock Value / Slow Moving Value / Category Risk Exposure / Supplier Risk Exposure
    ↓
Inventory Attention List → Item Performance Profile → Inventory Report
    ↓
Delay buy, clear, or review purchasing
```

---

## MQ-003

Question:

Which salespeople are carrying overdue or dormant books?

Why Owner Asks This:

A salesperson’s omzet can look strong while the owned book is late or going quiet. Those books become next month’s collection problem.

Typical Trigger:

- Collection slowing on certain routes
- Dormant customers rising
- High omzet names with high piutang

---

### Step 1
### Open

Dashboard: Salesman Performance  
Page: Menu **Sales Force** → **Salesmen**

---

### Step 2
### Review

Primary KPI: High Overdue Exposure  

Supporting KPI: High Piutang Exposure; Dormant Portfolio; Top Omzet Salesman %; Top Piutang Salesman %; Top 10 Overdue Salesmen (on Collection Dashboard)

---

### Step 3
### Interpret

Healthy Signal:

Collection Exposure is quiet. Dormant Portfolio is not filling the attention list. Overdue is not concentrated in a few books.

Warning Signal:

High Piutang Exposure without High Overdue Exposure — large outstanding that may not yet be late. Dormant Portfolio is rising on otherwise busy salesmen.

Critical Signal:

High Overdue Exposure on named salesmen. Stars on Top 10 Omzet also appear on Top 10 Piutang or Top 10 Overdue Salesmen.

---

### Step 4
### Gather Evidence

Evidence Sources:

- Salesman Attention List (portal table; including Dormant Customer Portfolio)
- Top 10 Piutang ranking on Salesman Performance
- Salesman Performance Profile
- Salesman Detail drawer (Principal Achievement, Trend — when no profile route)
- Collection Dashboard → Top 10 Overdue Salesmen → related report (investigation)
- Piutang Report
- Piutang Dashboard (company receivable context from the Collection Exposure card)

Purpose:

Separate large outstanding from actually late. Name the overdue books. See whether dormancy is a portfolio problem, not only a collection problem.

---

### Step 5
### Decide

Typical Decisions:

- Reallocate sales time toward collection and recovery on the named books
- Joint Sales–Finance review of those books
- Do not treat high omzet as healthy if the same book is overdue or dormant

---

### Investigation Path

```text
Which salespeople are carrying overdue or dormant books?
    ↓
Salesman Performance
    ↓
High Overdue Exposure
    ↓
High Piutang Exposure / Dormant Portfolio / Top Overdue Salesmen
    ↓
Salesman Attention List → Salesman Performance Profile → Piutang Report
    ↓
Joint review of the named books
```

---

## MQ-004

Question:

Which principals are becoming a buying or stock risk?

Why Owner Asks This:

Continuing to buy a Principal whose goods are already aging compounds trapped capital.

Typical Trigger:

- Inventory increasing on one Principal
- Supplier dependency growing
- Purchasing still placing orders on slow lines

---

### Step 1
### Open

Dashboard: Purchasing Management Dashboard  
Page: Menu **Purchasing** → **Purchasing**

Then open **Slow Moving & Dead Stock Dashboard** for Supplier Risk Exposure.

The catalog start phrase “Principal At-Risk Count” is not a fixed widget title. On-screen, start with the **Inventory Cross-Risk** and **Principal Dependency** attention cards (metric labels are live data).

---

### Step 2
### Review

Primary KPI: Inventory Cross-Risk (card)  

Supporting KPI: Principal Dependency (card); Top Principal % (Management Attention Center); Top 10 Principals; Principal Exposure Comparison; Supplier Risk Exposure; Grand Total Purchase

---

### Step 3
### Interpret

Healthy Signal:

Inventory Cross-Risk is quiet. Purchasing spend and warehouse risk are not the same names. Principal Dependency is not concentrating spend and stock together.

Warning Signal:

Top 10 Principals and Principal Exposure Comparison show spend concentration. Supplier Risk Exposure on inventory risk is rising on those same names.

Critical Signal:

Compound Dependency appears on Purchasing Attention List. The Principal that takes purchase cash also holds sick stock, and purchasing is still feeding it.

---

### Step 4
### Gather Evidence

Evidence Sources:

- Purchasing Attention List (portal table; Compound Dependency / Inventory Cross-Risk filter)
- Top 10 Principals; Principal Exposure Comparison
- Supplier Performance Profile
- Inventory Attention List / Supplier Risk Exposure
- Purchasing Report
- Inventory Report

Purpose:

Compare spend, stock holding, and sick stock on the same Principal. See whether purchasing invoices are still being placed on that name.

---

### Step 5
### Decide

Typical Decisions:

- Reduce purchasing from that Principal
- Delay buy on aging lines
- Keep buying only if the dependence is chosen strategy

---

### Investigation Path

```text
Which principals are becoming a buying or stock risk?
    ↓
Purchasing Management Dashboard
    ↓
Inventory Cross-Risk
    ↓
Principal Dependency / Principal Exposure Comparison / Supplier Risk Exposure
    ↓
Purchasing Attention List → Supplier Performance Profile → Purchasing Report / Inventory Report
    ↓
Delay buy, review Principal, or accept as strategy
```

---

## MQ-005

Question:

Who should be contacted today?

Why Owner Asks This:

Not every weak account needs the same work. Mixing collection, reminders, credit review, and sales recovery on one undifferentiated list wastes the day.

Typical Trigger:

- Start of day
- Collection slowing
- Cash flow pressure

---

### Step 1
### Open

Dashboard: Collection Optimization Dashboard  
Page: Menu **Customers** → **Collection Optimization**

---

### Step 2
### Review

Primary KPI: Actions Today  

Supporting KPI: Immediate Collection; Proactive Reminders; Credit Review; Sales Recovery; Collection Impact; Overdue Exposure; Due Within 7 Days; Recovery vs Billing

(Management Escalation is a Specialized Queues tab, not a named KPI card.)

---

### Step 3
### Interpret

Healthy Signal:

Actions Today is a short, typed list. Immediate Collection is not crowding out everything else. Due Within 7 Days is workable.

Warning Signal:

Actions Today is large and mixed. Due Within 7 Days is rising while Recovery vs Billing is already weak.

Critical Signal:

Immediate Collection and Management Escalation dominate. Overdue Exposure and Due Within 7 Days sit together with a large Collection Impact that will be missed if the day is not run by type.

---

### Step 4
### Gather Evidence

Evidence Sources:

- Today's Collection Priorities (portal table)
- Specialized Queues: Proactive Reminders, Credit Review, Sales Recovery, Management Escalation (portal tables)
- Top Impact Opportunities (portal table)
- Customer Performance Profile (when row links exist)
- Piutang Report; Sales Report (footer)

Purpose:

Split work into collect, remind, review credit, recover sales, or escalate. Separate cash due within 7 days from legacy debt (legacy sits on Collection Dashboard — Legacy Debt Count).

---

### Step 5
### Decide

Typical Decisions:

- Run today’s list by contact type
- Increase collection effort on named Immediate Collection accounts
- Send credit-review names to credit, not to collectors
- Send sales-recovery names to sales, not to collectors

---

### Investigation Path

```text
Who should be contacted today?
    ↓
Collection Optimization Dashboard
    ↓
Actions Today
    ↓
Immediate Collection / Due Within 7 Days / Specialized Queues
    ↓
Today's Collection Priorities → Piutang Report / Customer Performance Profile
    ↓
Run today’s list by type
```

---

## MQ-006

Question:

What should we buy, delay, transfer, or clear today?

Why Owner Asks This:

Holes and excess often exist at the same time. Buying everything that feels empty while slow movers age traps cash.

Typical Trigger:

- Warehouse asking what to buy
- Inventory increasing
- Selling items running out

---

### Step 1
### Open

Dashboard: Inventory Optimization Dashboard  
Page: Menu **Inventory** → **Inventory Optimization**

---

### Step 2
### Review

Primary KPI: Critical Actions  

Supporting KPI: Recommended Purchase Budget; Deferrable Spend; Purchase Now; Delay; Transfer; Clearance Review; Inventory Health Score

(Catalog “Recoverable Capital” is the Business Impact Summary: purchase impact, deferrable spend, recoverable capital. Catalog “Stock-Out Risk Items / Value” is reviewed on Inventory Forecast Dashboard as Stock-Out Risk Items.)

---

### Step 3
### Interpret

Healthy Signal:

Critical Actions is small. Purchase Now is about holes, not piles. Deferrable Spend is available to fund those holes.

Warning Signal:

Purchase Now and Delay both large — holes and piles in the same week. Transfer recommendations appear (location, not true shortage).

Critical Signal:

Clearance Review / Dead Stock Recovery is large while Recommended Purchase Budget is also large. Buying would add capital on top of idle stock.

---

### Step 4
### Gather Evidence

Evidence Sources:

- Top Optimization Actions (portal table)
- Recommended Reorder List (portal table)
- Warehouse Rebalancing (portal table)
- Overstock & Delay Purchasing (portal table)
- Dead Stock Recovery (portal table)
- Item Performance Profile
- Inventory Forecast Dashboard (Stock-Out Risk Items)
- Inventory Report; Purchasing Report (footer)
- Purchasing Management Dashboard (footer)

Purpose:

Split actions into purchase, delay, transfer, or clearance. Compare recommended purchase budget with capital that could still be recovered. Confirm SKU and warehouse on reports.

---

### Step 5
### Decide

Typical Decisions:

- Buy holes only
- Delay piles
- Transfer between warehouses when location is the problem
- Clear dead stock as a separate action from buying

---

### Investigation Path

```text
What should we buy, delay, transfer, or clear today?
    ↓
Inventory Optimization Dashboard
    ↓
Critical Actions
    ↓
Purchase Now / Delay / Transfer / Clearance Review / Recommended Purchase Budget vs Deferrable Spend
    ↓
Action tables → Item Performance Profile → Inventory Report / Purchasing Report
    ↓
Buy holes, delay piles, transfer, or clear
```

---

## MQ-007

Question:

Which receivables require attention?

Why Owner Asks This:

Total piutang is scale. Overdue is urgency. Names are what collectors can work.

Typical Trigger:

- Cash flow pressure
- Collection slowing
- Piutang growing

---

### Step 1
### Open

Dashboard: Piutang Dashboard  
Page: Menu **Finance** → **Piutang**

Then open **Collection Dashboard** for overdue concentration, overdue salesmen, overdue wilayah, and legacy debt.

---

### Step 2
### Review

Primary KPI: Overdue Piutang (Overdue Exposure)  

Supporting KPI: Total Piutang; Overdue Customer; Piutang > 90 Hari; Aging Distribution; Top 10 Customer %; Legacy Debt Count; Overdue Concentration %; Top 10 Overdue Customers; Top 10 Overdue Salesmen; Top 10 Overdue Wilayah

---

### Step 3
### Interpret

Healthy Signal:

Overdue Piutang is small versus Total Piutang. Aging sits in Current / early buckets. Piutang > 90 Hari is not the book.

Warning Signal:

Overdue Customer count is rising. Aging is moving into 61–90. Overdue Concentration % shows a few names carrying the overdue.

Critical Signal:

Piutang > 90 Hari and Legacy Debt Count are material. Top 10 Overdue Customers / Salesmen / Wilayah show the overdue is not “everywhere” — it is named.

---

### Step 4
### Gather Evidence

Evidence Sources:

- Top 20 Outstanding Customers — Aging Breakdown (portal table) → Piutang Report (investigation)
- Collection Attention List (Chronic Overdue / Legacy Debt)
- Top 10 Overdue Customers / Top 10 Overdue Salesmen → related report (investigation)
- Customer Performance Profile (from customer pages, not from Piutang KPI tiles)
- Piutang Report (open receivable rows: Faktur, Jatuh Tempo, Kurang Bayar)

Purpose:

Separate total piutang from overdue. See how much is already more than 90 days. Name customers, then salespeople and wilayah. Top 10 Overdue Wilayah has **no row drill-down** — use it as a pointer, then Collection Dashboard / Piutang Report for names.

---

### Step 5
### Decide

Typical Decisions:

- Increase collection effort on named overdue accounts
- Restrict supply if aging is chronic
- Assign collectors by salesman or wilayah when concentration is obvious

---

### Investigation Path

```text
Which receivables require attention?
    ↓
Piutang Dashboard
    ↓
Overdue Piutang
    ↓
Piutang > 90 Hari / Aging / Top Overdue Customers / Salesmen / Wilayah
    ↓
Piutang Report / Collection Attention List / Customer Performance Profile
    ↓
Collect named accounts or restrict supply
```

---

## MQ-008

Question:

Are we collecting as fast as we are billing?

Why Owner Asks This:

If new Faktur keep outrunning cash in, the book grows even when collectors are busy.

Typical Trigger:

- Cash flow pressure
- Omzet looks strong but cash is tight
- Month-end cash worry

---

### Step 1
### Open

Dashboard: Collection Dashboard  
Page: Menu **Finance** → **Collection**

Then open **Cash Flow Forecast Dashboard** for pace, gap, and month-end scenarios.

---

### Step 2
### Review

Primary KPI: Recovery vs Billing %  

Supporting KPI: Cash Collected MTD; Collection Gap; Required Daily Collection; Daily Cash Collection Average; Expected Cash Collection; Best / Expected / Worst Cash; Remaining Collection Target; Remaining Calendar Days; Recovery vs Billing Forecast

---

### Step 3
### Interpret

Healthy Signal:

Recovery vs Billing % shows cash keeping up with billing. Required Daily Collection is close to Daily Cash Collection Average. Remaining Calendar Days can still close Remaining Collection Target.

Warning Signal:

Cash Collected MTD is busy but Recovery vs Billing % is slipping. Collection Gap is opening. Forecast Confidence on Cash Flow Forecast is not high.

Critical Signal:

Recovery vs Billing Forecast and Worst Cash show the month will not close. Required Daily Collection is far above current pace with few Remaining Calendar Days.

(Sales Forecast Dashboard uses Healthy / Warning / Critical **forecast bands** for sales, not for cash. Do not copy those bands onto collection unless the cash page states them.)

---

### Step 4
### Gather Evidence

Evidence Sources:

- Collection Attention List (Low Recovery vs Billing)
- Recovery Summary; Daily Collection Pace; Cash Forecast vs Billing; Recovery Trend
- Top Collection Risks (portal table)
- Piutang Report (footer from Cash Flow Forecast)
- Collection Optimization Dashboard (today’s actions that can still move cash)

Purpose:

Compare cash collected with billed sales. See the remaining gap and whether remaining days can still close the month. Name the customers threatening the cash forecast.

---

### Step 5
### Decide

Typical Decisions:

- Push collection this week
- Slow new credit
- Accept this month’s cash finish and plan next month from Expected / Worst Cash

---

### Investigation Path

```text
Are we collecting as fast as we are billing?
    ↓
Collection Dashboard → Cash Flow Forecast Dashboard
    ↓
Recovery vs Billing %
    ↓
Collection Gap / Required Daily Collection / Expected vs Worst Cash
    ↓
Collection Attention List → Piutang Report / Top Collection Risks
    ↓
Push collection, slow credit, or accept the finish
```

---

## MQ-009

Question:

Which salespeople are underperforming?

Why Owner Asks This:

Company omzet can be carried by a few names. “Below target” and “no target” are different failures. Coaching the wrong cause wastes the week.

Typical Trigger:

- Sales decline
- Target gap widening
- A few people carrying the month

---

### Step 1
### Open

Dashboard: Salesman Performance  
Page: Menu **Sales Force** → **Salesmen**

Then, for why the miss happened: **Sales Dashboard** (company attainment), **Salesman Detail** drawer (Principal Achievement), **Sales Force Overview** and **Salesman Field Activity** (coverage and conversion).

---

### Step 2
### Review

Primary KPI: Below Target  

Supporting KPI: Missing Target Setup; Achievement % (Sales Dashboard and Top 10 Achievement %); Total Achievement vs Total Target; Visit Execution % (Execution %); Missed Visit; Effective Call Rate; GPS Valid Rate; Orders; Order Value

---

### Step 3
### Interpret

Healthy Signal:

Below Target is not the team story. Achievement % is on plan. Execution % and Effective Call Rate show visits that produce orders.

Warning Signal:

Missing Target Setup is not zero — some people cannot be judged as “below target.” Execution % is weak (Missed Visit). Orders exist but Order Value / company Achievement % is still short (conversion or mix).

Critical Signal:

Below Target is widespread while Top 10 Omzet is a few names. GPS Valid Rate is below the on-screen target (>95%). Effective Call Rate is low despite Actual visits — check-ins are not credible selling.

---

### Step 4
### Gather Evidence

Evidence Sources:

- Salesman Attention List (portal table)
- Salesman Performance Profile
- Salesman Detail drawer (Principal Achievement; Trend)
- Sales Dashboard → Top 10 Salesman → Sales Report (faktur evidence)
- Sales Force Overview table and rankings → Salesman Field Activity (map, missed visits, visit timeline)
- Sales Forecast Dashboard (Required Daily Sales / Target Gap) when the question is month-end, not people

Purpose:

Separate no-target from below-target. See whether the miss is Principal mix (drawer), coverage (missed visits), conversion (orders vs visits), or GPS credibility.

---

### Step 5
### Decide

Typical Decisions:

- Set missing targets
- Fix coverage (missed / unplanned visits)
- Fix conversion (effective calls → orders)
- Coach named people
- Reallocate sales resources toward people who still have a book

---

### Investigation Path

```text
Which salespeople are underperforming?
    ↓
Salesman Performance
    ↓
Below Target
    ↓
Missing Target Setup / Principal Achievement / Execution % / Effective Call Rate / GPS Valid Rate / Orders
    ↓
Salesman Performance Profile / Salesman Field Activity / Sales Report
    ↓
Set targets, fix coverage, fix conversion, or coach named people
```

---

## MQ-010

Question:

Which customers are declining?

Why Owner Asks This:

A customer who is still current but buying less is next month’s dormant account. Recovery is cheaper than winning a new name.

Typical Trigger:

- Sales decline
- Book feels quieter
- Strategic accounts buying less

---

### Step 1
### Open

Dashboard: Customer Portfolio Dashboard  
Page: Menu **Customers** → **Customer Portfolio**

Then **Customer Analytics** for dormancy and attention list, **Customer Risk Forecast Dashboard** for Purchase Decline and Inactivity signal families.

---

### Step 2
### Review

Primary KPI: Declining  

Supporting KPI: Dormant; Never Purchased; Purchase Decline; Inactivity; Strategic Customers; Strategic At Risk; Total MTD Omzet; Dormant Customers (90-day) on Customer Analytics

---

### Step 3
### Interpret

Healthy Signal:

Lifecycle mix is not filling Declining and Dormant. Strategic At Risk is quiet. Active this month is the bulk of the book.

Warning Signal:

Declining is rising before Dormant (90-day). Purchase Decline and Inactivity signals appear on Customer Risk Forecast. Strategic customers are in the declining set.

Critical Signal:

Dormant and Never Purchased dominate lifecycle. Strategic At Risk is no longer an exception. The book is quieter even if Total MTD Omzet is still carried by Top 10 MTD Omzet Concentration.

---

### Step 4
### Gather Evidence

Evidence Sources:

- Lifecycle Distribution; Portfolio Priority Queue; Customers by Portfolio Action (portal tables)
- Customer Analytics — Inactivity card → Customer Attention List (Dormant)
- Customer Risk Attention List (Purchase Decline / Inactivity)
- Customer Performance Profile
- Customer Report
- Sales Report (last purchase / faktur evidence)

Purpose:

Catch purchase-decline and inactivity before 90-day dormant. See whether declining names are strategic. Decide recover versus stop covering.

---

### Step 5
### Decide

Typical Decisions:

- Increase sales focus on declining names
- Protect strategic accounts that are slipping
- Stop covering accounts that will not return

---

### Investigation Path

```text
Which customers are declining?
    ↓
Customer Portfolio Dashboard
    ↓
Declining
    ↓
Purchase Decline / Inactivity / Dormant / Never Purchased / Strategic At Risk
    ↓
Portfolio Priority Queue → Customer Performance Profile → Sales Report
    ↓
Recover, protect, or stop covering
```

---

## MQ-011

Question:

Is warehouse capital trapped or about to run out?

Why Owner Asks This:

The company can be overstocked on what does not sell and understocked on what does. Those two conditions need opposite actions in the same week.

Typical Trigger:

- Inventory increasing
- Warehouse “full” while selling items run out
- Sales lost to empty shelves

---

### Step 1
### Open

Dashboard: Slow Moving & Dead Stock Dashboard **and** Inventory Forecast Dashboard  
Page: Menu **Inventory** → **Inventory Risk**, then **Inventory Forecast**

Then **Inventory Optimization Dashboard** for buy / delay / transfer / clear, and **Branch / Warehouse Performance Dashboard** if one location holds the idle stock.

---

### Step 2
### Review

Primary KPI: Dead Stock Value **and** Stock-Out Risk Items  

Supporting KPI: Slow Moving Value; At-Risk Inventory %; Overstock Value; Understock Value; Inventory Health Score; Recommended Purchase Budget; Deferrable Spend; Top Warehouse At-Risk %; Inactive Warehouse With Stock

---

### Step 3
### Interpret

Healthy Signal:

Dead Stock Value is not trapping capital. Stock-Out Risk Items is small. Inventory Health Score is not the problem. Overstock Value and Understock Value are not both large.

Warning Signal:

Overstock Value and Understock Value exist together. Top Warehouse At-Risk % shows one warehouse holding the idle stock. Inactive Warehouse With Stock is not zero.

Critical Signal:

Dead Stock Value is large **and** Stock-Out Risk Items is large. Recommended Purchase Budget would buy holes while Deferrable Spend / clearance is still sitting in piles.

---

### Step 4
### Gather Evidence

Evidence Sources:

- Inventory Attention List; Top 10 Dead / Slow Moving
- Top Inventory Risks; Purchasing Recommendations (Inventory Forecast)
- Warehouse Rebalancing; Dead Stock Recovery; Recommended Reorder List (Inventory Optimization)
- Location Attention List; Top 10 Warehouse by At-Risk Value → Slow Moving & Dead Stock
- Item Performance Profile
- Inventory Report (stock rows by warehouse)

Purpose:

Name idle SKUs and whether one warehouse holds them. Name selling SKUs at stock-out risk. Compare recommended purchase budget with capital that could still be recovered.

---

### Step 5
### Decide

Typical Decisions:

- Clear or delay piles
- Buy holes only
- Transfer if location is the problem
- Do not fund a full restock while dead stock is still the larger pile

---

### Investigation Path

```text
Is warehouse capital trapped or about to run out?
    ↓
Slow Moving & Dead Stock Dashboard + Inventory Forecast Dashboard
    ↓
Dead Stock Value / Stock-Out Risk Items
    ↓
Overstock vs Understock / Top Warehouse At-Risk % / Recommended Purchase Budget vs Deferrable Spend
    ↓
Item Performance Profile / Inventory Report / Warehouse Rebalancing
    ↓
Clear piles, buy holes, or transfer
```

---

## MQ-012

Question:

Which customers dominate omzet or piutang?

Why Owner Asks This:

Growth or cash sitting with a shrinking set of names is concentration risk. Losing one name would change the month.

Typical Trigger:

- One customer dominates the conversation
- Fear of losing a key account
- Piutang looks large because of a few names

---

### Step 1
### Open

Dashboard: Customer Analytics  
Page: Menu **Customers** → **Customers**

Also: Piutang Dashboard (Top 10 Customer % / Top 20 Customer %), Customer Portfolio Dashboard (Top 10 MTD Omzet Concentration / Top 10 Open Piutang Concentration), Management Attention Center (Top 5 Customers).

---

### Step 2
### Review

Primary KPI: Top Omzet Customer % **and** Top Piutang Customer %  

Supporting KPI: Top 10 Customer %; Top 20 Customer %; Top 10 by Omzet; Top 10 by Piutang; Top 5 Customers (executive exposure); Strategic Customers; Customers At Risk on the same names

---

### Step 3
### Interpret

Healthy Signal:

Top Omzet Customer % and Top Piutang Customer % are not the whole month. Strategic Customers are a chosen set, not an accidental monopoly.

Warning Signal:

Top 10 Customer % of piutang is high. Top 10 by Omzet is a short list. The same names appear on both omzet and piutang rankings.

Critical Signal:

Top 5 Customers (executive) or Top 10 Open Piutang Concentration is the book. Those same names are already on Customer Risk Forecast (Customers At Risk / High Risk).

---

### Step 4
### Gather Evidence

Evidence Sources:

- Top 10 by Omzet / Top 10 by Piutang → Customer Performance Profile
- Portfolio concentration rankings → Customer Performance Profile / Customer Report
- Top 5 Customers on Management Attention Center → related report (investigation)
- Compare Customers (from Entity Analytics / profile header)
- Piutang Report; Sales Report

Purpose:

Name omzet concentration and piutang concentration separately. See whether those names are already at risk.

---

### Step 5
### Decide

Typical Decisions:

- Protect key accounts
- Reduce exposure on names that already combine concentration and risk
- Broaden the book

---

### Investigation Path

```text
Which customers dominate omzet or piutang?
    ↓
Customer Analytics
    ↓
Top Omzet Customer % / Top Piutang Customer %
    ↓
Top 10 rankings / Top 10 Customer % / Strategic Customers / Customers At Risk
    ↓
Customer Performance Profile → Sales Report / Piutang Report
    ↓
Protect, reduce exposure, or broaden the book
```

---

## MQ-013

Question:

Which suppliers are becoming dominant?

Why Owner Asks This:

A Principal can take this month’s purchase cash, already hold the warehouse, and own the sick stock. Those are three different kinds of dependence.

Typical Trigger:

- Supplier dependency growing
- One Principal dominates purchasing talk
- Stock and risk sitting with the same name

---

### Step 1
### Open

Dashboard: Purchasing Management Dashboard  
Page: Menu **Purchasing** → **Purchasing**

Also: Management Attention Center (Top Principal %), Inventory Dashboard (Top 10 Suppliers / Inventory by Supplier).

---

### Step 2
### Review

Primary KPI: Top Principal % (executive Purchasing card) **and** Top 10 Principals (MTD Purchase Amount / % of Purchase)  

Supporting KPI: Principal Dependency; Principal Exposure Comparison; Grand Total Purchase; Top Supplier % (executive Inventory card); Inventory by Supplier; Compound Dependency (Purchasing Attention List)

The catalog start phrase “Top 1 Principal %” is the executive / ranking concentration idea. On-screen titles are **Top Principal %** and **Top 10 Principals**. Inventory concentration uses **Top Supplier %**, not a second invented “Top 1” tile.

---

### Step 3
### Interpret

Healthy Signal:

Purchase cash is spread across Top 10 Principals. Warehouse Top 10 Suppliers is a different mix. Compound Dependency is quiet.

Warning Signal:

One Principal takes most Grand Total Purchase **or** most warehouse value — but not both yet.

Critical Signal:

Compound Dependency: spend and stock on the same Principal, and Supplier Risk Exposure shows that Principal also owns sick stock.

---

### Step 4
### Gather Evidence

Evidence Sources:

- Purchasing Attention List (Compound Dependency)
- Principal Exposure Comparison → Supplier Performance Profile
- Top 10 Principals → Supplier Performance Profile or Investigate
- Inventory Dashboard Top 10 Suppliers → Inventory Report
- Purchasing Report
- Compare Suppliers

Purpose:

See who takes purchase cash, who already holds warehouse capital, and whether those are the same name as sick stock.

---

### Step 5
### Decide

Typical Decisions:

- Diversify supplier exposure
- Delay buy on the dominant Principal’s slow lines
- Treat the dependence as chosen strategy (and stop pretending it is accidental)

---

### Investigation Path

```text
Which suppliers are becoming dominant?
    ↓
Purchasing Management Dashboard
    ↓
Top Principal % / Top 10 Principals
    ↓
Principal Exposure Comparison / Compound Dependency / Inventory Top 10 Suppliers
    ↓
Supplier Performance Profile → Purchasing Report / Inventory Report
    ↓
Diversify, delay buy, or accept as strategy
```

---

## MQ-014

Question:

Are we too dependent on a few salespeople?

Why Owner Asks This:

If one or two people produce most omzet, next month’s plan is a people risk, not a market risk.

Typical Trigger:

- Sales decline if one person is absent
- A star salesperson dominates results
- Active force feels thin

---

### Step 1
### Open

Dashboard: Salesman Performance  
Page: Menu **Sales Force** → **Salesmen**

Also: Sales Dashboard (Top 10 Salesman), Sales Force Overview (Active Salesmen).

---

### Step 2
### Review

Primary KPI: Top Omzet Salesman %  

Supporting KPI: Top 10 Omzet (Salesman Performance and Sales Dashboard); Active Salesmen; Below Target; High Overdue Exposure; Top Piutang Salesman %

---

### Step 3
### Interpret

Healthy Signal:

Top Omzet Salesman % is not the whole company. Active Salesmen is a real team. Below Target is not “everyone except the stars.”

Warning Signal:

Top 10 Omzet is steep. Active Salesmen is thin. The rest of the team is Below Target.

Critical Signal:

Stars carry omzet **and** High Overdue Exposure or Top Piutang Salesman %. Losing one person would hit both sales and collection.

---

### Step 4
### Gather Evidence

Evidence Sources:

- Top 10 Omzet → Salesman Performance Profile
- Salesman Attention List
- Sales Dashboard Top 10 Salesman → Sales Report
- Sales Force Overview (Active Salesmen; field table)
- Compare Salesmen

Purpose:

See omzet concentration, size of the active force, whether stars also carry overdue books, and whether the rest of the team is below target.

---

### Step 5
### Decide

Typical Decisions:

- Protect key people
- Rebuild coverage across the team
- Reallocate sales resources so next month is not a single-person plan

---

### Investigation Path

```text
Are we too dependent on a few salespeople?
    ↓
Salesman Performance
    ↓
Top Omzet Salesman %
    ↓
Active Salesmen / Below Target / High Overdue Exposure
    ↓
Salesman Performance Profile / Sales Report / Sales Force Overview
    ↓
Protect key people and rebuild team coverage
```

---

## MQ-015

Question:

Is credit policy being respected?

Why Owner Asks This:

Plafond and hold rules only protect cash if they are observed. A credit-limit signal is still preventable. A breach, or a suspended account still being billed, is already a failure.

Typical Trigger:

- Cash flow pressure
- Customers buying above comfort
- Collection slowing on large accounts

---

### Step 1
### Open

Dashboard: Customer Analytics  
Page: Menu **Customers** → **Customers**

Then **Customer Risk Forecast Dashboard** for Credit Limit and Collection Risk signal families.

---

### Step 2
### Review

Primary KPI: Credit Limit (signal family) **and** Plafond Breach  

Supporting KPI: Suspended + Sales; Payment Delay; Collection Risk; Customers Forecasted At Risk; Customer Attention List filtered to Plafond Breach

---

### Step 3
### Interpret

Healthy Signal:

Credit card on Customer Analytics is quiet. Credit Limit signals are Watch-level, not breaches. Suspended + Sales is zero.

Warning Signal:

Credit Limit signal count is rising (approaching Plafond). Payment Delay appears on the same names.

Critical Signal:

Plafond Breach is not empty. Suspended + Sales shows suspended accounts still being billed. Collection Risk / Critical Category is already on those names.

---

### Step 4
### Gather Evidence

Evidence Sources:

- Customer Attention List (Plafond Breach / credit signals)
- Customer Risk Attention List (Credit Limit / Payment Delay)
- Customer Performance Profile
- Sales Report (faktur still issued)
- Piutang Report (open balance versus terms)

Purpose:

Separate approaching Plafond from already breached. Find suspended accounts still billed. See payment-delay on the same names.

---

### Step 5
### Decide

Typical Decisions:

- Review customer credit
- Restrict supply
- Stop billing suspended accounts

---

### Investigation Path

```text
Is credit policy being respected?
    ↓
Customer Analytics
    ↓
Plafond Breach / Credit Limit
    ↓
Suspended + Sales / Payment Delay / Collection Risk
    ↓
Customer Attention List → Customer Performance Profile → Sales Report
    ↓
Review credit, restrict supply, or stop billing
```

---

## MQ-016

Question:

Are purchased goods posted, or stuck in backlog?

Why Owner Asks This:

Unposted purchases mean the company has already spent or committed without gaining sellable stock. Quiet purchasing late in the month can be a freeze or a stall.

Typical Trigger:

- Warehouse waiting for stock that was already bought
- Purchasing gone quiet
- Selling SKUs at risk of running out

---

### Step 1
### Open

Dashboard: Purchasing Management Dashboard  
Page: Menu **Purchasing** → **Purchasing**

Also: Management Attention Center Purchasing card (Pending Posting). Inventory Forecast Dashboard for Stock-Out Risk Items during a quiet purchasing period.

---

### Step 2
### Review

Primary KPI: Pending Posting Value (all BELUM) **and** Posted %  

Supporting KPI: Qualified Backlog; Grand Total Purchase; Purchasing Pace (attention card — live labels; catalog “Purchasing Inactivity Flag” is not a fixed title); Stock-Out Risk Items (Inventory Forecast)

---

### Step 3
### Interpret

Healthy Signal:

Posted % is the bulk of invoices. Pending Posting Value and Qualified Backlog are small. Purchasing Pace is not an inactivity story while sales still have cover.

Warning Signal:

Pending Posting Value is material. Qualified Backlog is aging. Purchasing Pace shows the month going quiet after mid-month.

Critical Signal:

Unposted value is large **and** Stock-Out Risk Items is rising — money is committed but not sellable, while holes appear. Or purchasing is quiet while holes are already on Inventory Forecast.

---

### Step 4
### Gather Evidence

Evidence Sources:

- Posting Exposure card → Purchasing Attention List (Qualified Backlog)
- Posting Status Breakdown
- Purchasing Report (invoice rows including Posting Stok; optional posting filter from investigation)
- Inventory Forecast — Stock-Out Risk Items / Purchasing Recommendations
- Supplier Performance Profile (when the backlog is a supplier, not only a posting queue)

Purpose:

See how much purchasing is still unposted. See aged qualified backlog. See whether purchasing has gone quiet. See whether selling SKUs are at stock-out risk during that quiet.

---

### Step 5
### Decide

Typical Decisions:

- Post existing invoices first
- Declare a freeze if inactivity is intentional
- Resume buying only the holes if Stock-Out Risk Items requires it

---

### Investigation Path

```text
Are purchased goods posted, or stuck in backlog?
    ↓
Purchasing Management Dashboard
    ↓
Pending Posting Value / Posted %
    ↓
Qualified Backlog / Purchasing Pace / Stock-Out Risk Items
    ↓
Purchasing Attention List → Purchasing Report / Inventory Forecast
    ↓
Post first, declare a freeze, or resume buying the holes
```

---

# SECTION 3
# OWNER INVESTIGATION FLOWS

These are compact playbooks. Details live in Section 2.

---

## Customer Risk Investigation

Use when EQ-002 or EQ-008 points at customers, or EQ-001 shows Customers At Risk / Strategic At Risk.

```text
Customers are weakening
    ↓
Customer Risk Forecast Dashboard
    ↓
Customers At Risk → risk categories → Payment Delay / Inactivity / Purchase Decline / Credit Limit
    ↓
Customer Risk Attention List → Customer Performance Profile → Piutang Report / Sales Report
    ↓
Collect, restrict supply, recover sales, or review credit (MQ-001, MQ-015, then MQ-005 for today’s calls)
```

---

## Inventory Problem Investigation

Use when EQ-002 or EQ-006 points at warehouse capital, or Alert Center shows Dead Stock / Slow Moving / Never Sold.

```text
Stock is stuck or holes are appearing
    ↓
Slow Moving & Dead Stock Dashboard
    ↓
At-Risk Inventory % → Dead Stock Value / Slow Moving Value / Category & Supplier Risk Exposure
    ↓
Inventory Attention List → Item Performance Profile → Inventory Report
    ↓
Then Inventory Forecast (Stock-Out Risk Items) and Inventory Optimization (buy / delay / transfer / clear)
    ↓
Stop feeding sick stock; clear piles; buy holes only (MQ-002, MQ-011, MQ-006)
```

---

## Supplier Dependency Investigation

Use when EQ-007 or EQ-002 points at principals.

```text
One Principal is running purchasing or the warehouse
    ↓
Purchasing Management Dashboard
    ↓
Top 10 Principals / Principal Dependency / Inventory Cross-Risk / Compound Dependency
    ↓
Supplier Risk Exposure (Slow Moving & Dead Stock) → Supplier Performance Profile → Purchasing Report / Inventory Report
    ↓
Diversify, delay buy, or accept as strategy (MQ-013, MQ-004)
```

---

## Salesman Performance Investigation

Use when EQ-005 or EQ-002 points at people, or EQ-007 points at salesman concentration.

```text
Sales feel weak, or a few people run the month
    ↓
Salesman Performance
    ↓
Below Target vs Missing Target Setup → Top Omzet Salesman % → High Overdue Exposure / Dormant Portfolio
    ↓
Salesman Performance Profile / Salesman Detail (Principal mix) / Sales Force Overview → Salesman Field Activity / Sales Report
    ↓
Coach, fix coverage or conversion, joint-collect overdue books, or rebuild team coverage (MQ-009, MQ-003, MQ-014)
```

---

## Working Capital Investigation

Use when EQ-004 and EQ-006 both feel tight — cash is in the book or in the warehouse, not in the next cycle.

```text
Cash feels trapped
    ↓
Collection Dashboard + Cash Flow Forecast Dashboard
    ↓
Recovery vs Billing % / Overdue Piutang / Collection Gap
    ↓
Piutang Report / Collection Attention List
    ↓
and
    ↓
Slow Moving & Dead Stock + Inventory Forecast
    ↓
Dead Stock Value / Stock-Out Risk Items / Recommended Purchase Budget vs Deferrable Spend
    ↓
Inventory Report / optimization action tables
    ↓
Collect named overdue; do not buy piles; post unposted purchases before new spend (MQ-007, MQ-008, MQ-011, MQ-016)
```

---

## Today’s Action Investigation

Use when EQ-003 is the door.

```text
Something must be done today
    ↓
Collection Optimization Dashboard  and/or  Inventory Optimization Dashboard
    ↓
Actions Today  /  Critical Actions
    ↓
Typed queues and action tables
    ↓
Piutang Report / Inventory Report / Purchasing Report
    ↓
Call by type; buy holes; delay piles; transfer; clear (MQ-005, MQ-006)
```

---

## Credit and Next-Problem Investigation

Use when EQ-008 is the door and totals still look fine.

```text
Signals are forming
    ↓
Customer Analytics (Plafond Breach, Suspended + Sales)
    ↓
Customer Risk Forecast (Credit Limit, Payment Delay)
    ↓
Purchasing Management (Pending Posting, Purchasing Pace)
    ↓
Customer Attention List / Purchasing Attention List / reports
    ↓
Stop preventable credit failure; post backlog before it becomes a stock-out (MQ-015, MQ-016)
```

---

# SECTION 4
# EVIDENCE SOURCE REGISTRY

Only assets discovered in Phase-4A. “Used By” is the management question that should open this evidence, not every screen that can reach it.

| Evidence Source | Type | Used By |
|---------------|------|----------|
| Domain Summaries | Portal Table | EQ-001 (router) |
| Alert Center — Top Critical Alerts / Alerts by Category | Portal Table | EQ-001, EQ-002 (exception entry) |
| Customer Risk Attention List | Portal Table | MQ-001, MQ-010, MQ-015 |
| Top Customers by Risk Priority | Portal Table | MQ-001 |
| Customer Attention List | Portal Table | MQ-010, MQ-015 |
| Portfolio Priority Queue / Customers by Portfolio Action | Portal Table | MQ-010 |
| Today's Collection Priorities | Portal Table | MQ-005 |
| Specialized Queues (Proactive Reminders, Credit Review, Sales Recovery, Management Escalation) | Portal Table | MQ-005 |
| Top Impact Opportunities | Portal Table | MQ-005 |
| Collection Attention List | Portal Table | MQ-007, MQ-008 |
| Top 20 Outstanding Customers — Aging Breakdown | Portal Table | MQ-007 |
| Top 10 Overdue Customers | Ranking | MQ-007 |
| Top 10 Overdue Salesmen | Ranking | MQ-003, MQ-007 |
| Top 10 Overdue Wilayah | Ranking (no row drill-down) | MQ-007 |
| Top Collection Risks | Portal Table | MQ-008 |
| Salesman Attention List | Portal Table | MQ-003, MQ-009, MQ-014 |
| Salesman Detail drawer (Principal Achievement, Trend) | Portal Detail | MQ-009 |
| Sales Force Overview — salesman field table and rankings | Portal Table | MQ-009, MQ-014 |
| Salesman Field Activity (map, missed visits, visit timeline) | Portal Detail Page | MQ-009 |
| Inventory Attention List | Portal Table | MQ-002, MQ-011 |
| Top 10 Dead Stock by Value / Top 10 Slow Moving by Value | Ranking | MQ-002, MQ-011 |
| Top Inventory Risks / Purchasing Recommendations | Portal Table | MQ-011, MQ-016 |
| Top Optimization Actions / Recommended Reorder List / Warehouse Rebalancing / Overstock & Delay Purchasing / Dead Stock Recovery | Portal Table | MQ-006, MQ-011 |
| Location Attention List | Portal Table | MQ-011 |
| Top 10 Warehouse by At-Risk Value | Ranking | MQ-011 |
| Purchasing Attention List | Portal Table | MQ-004, MQ-013, MQ-016 |
| Top 10 Principals / Principal Exposure Comparison | Ranking / Table | MQ-004, MQ-013 |
| Top 10 by Omzet / Top 10 by Piutang (Customer Analytics) | Ranking | MQ-012 |
| Top 10 MTD Omzet Concentration / Top 10 Open Piutang Concentration | Ranking | MQ-012 |
| Top 5 Customers / Categories / Suppliers / Principals (Management Attention Center) | Ranking | EQ-001, MQ-012, MQ-013 |
| Top 10 Salesman (Sales Dashboard) | Ranking | MQ-009, MQ-014 |
| Customer Performance Profile | Performance Profile | MQ-001, MQ-005, MQ-007, MQ-010, MQ-012, MQ-015 |
| Salesman Performance Profile | Performance Profile | MQ-003, MQ-009, MQ-014 |
| Supplier Performance Profile | Performance Profile | MQ-004, MQ-013, MQ-016 |
| Item Performance Profile | Performance Profile | MQ-002, MQ-006, MQ-011 |
| Investigation Workspace | Portal Detail Page | Named-entity follow-up from Entity Analytics (any MQ once a name is known) |
| Compare Customers / Salesmen / Suppliers / Items | Portal Detail Page | MQ-012, MQ-014, MQ-013, MQ-002 |
| Sales Report (Faktur list) | Portal Report | MQ-001, MQ-009, MQ-010, MQ-012, MQ-014, MQ-015 |
| Piutang Report (open receivable rows) | Portal Report | MQ-001, MQ-003, MQ-005, MQ-007, MQ-008, MQ-012 |
| Customer Report (customer rows) | Portal Report | MQ-001, MQ-010, MQ-012 |
| Inventory Report (stock rows by warehouse) | Portal Report | MQ-002, MQ-006, MQ-011, MQ-013 |
| Purchasing Report (purchase invoice rows, posting status) | Portal Report | MQ-004, MQ-006, MQ-013, MQ-016 |
| Related report via Investigation (Sales / Piutang / Inventory / Purchasing / Customer) | Portal Report | Row investigation from dashboards and Alert Center |
| Profile Evidence widget | Portal Table → related report | Any profile opened above |

---

# SECTION 5
# OWNER QUICK REFERENCE

| I Want To Know | Open This | Review This KPI | Evidence Source |
|----------------|------------|-----------------|-----------------|
| Is the business healthy? | Management Attention Center | Domain Summaries; Achievement %; Customers At Risk; Pending Posting; Total Inventory Value | Domain dashboard named on the row; Alert Center |
| Where is my biggest risk? | Management Attention Center, then the largest of the four MQ dashboards | Customers At Risk vs At-Risk Inventory % vs High Overdue Exposure vs Inventory Cross-Risk | Alert Center; then the MQ evidence in Section 2 |
| Which customers are risky? | Customer Risk Forecast Dashboard | Customers At Risk | Customer Risk Attention List; Customer Performance Profile |
| Why is this customer risky? | Customer Risk Forecast Dashboard | Payment Delay; Credit Limit; Inactivity; Purchase Decline | Customer Performance Profile; Piutang Report; Sales Report |
| Which inventory is unhealthy? | Slow Moving & Dead Stock Dashboard | At-Risk Inventory % | Inventory Attention List; Item Performance Profile; Inventory Report |
| Which SKUs are dead or slow? | Slow Moving & Dead Stock Dashboard | Dead Stock Value; Slow Moving Value | Top 10 Dead / Slow Moving; Item Performance Profile |
| Which salespeople carry overdue or dormant books? | Salesman Performance | High Overdue Exposure; Dormant Portfolio | Salesman Attention List; Salesman Performance Profile; Piutang Report |
| Which principals are a buying or stock risk? | Purchasing Management Dashboard | Inventory Cross-Risk; Principal Dependency | Purchasing Attention List; Supplier Performance Profile; Inventory Report |
| Who should be contacted today? | Collection Optimization Dashboard | Actions Today | Today's Collection Priorities; Specialized Queues; Piutang Report |
| What should we buy, delay, transfer, or clear today? | Inventory Optimization Dashboard | Critical Actions | Action tables; Item Performance Profile; Inventory Report; Purchasing Report |
| Which receivables need attention? | Piutang Dashboard | Overdue Piutang | Top 20 Outstanding Customers; Piutang Report; Collection Attention List |
| Are we collecting as fast as we bill? | Collection Dashboard, then Cash Flow Forecast Dashboard | Recovery vs Billing % | Collection Attention List; Piutang Report; Top Collection Risks |
| Will this month’s cash close? | Cash Flow Forecast Dashboard | Required Daily Collection; Expected Cash Collection | Piutang Report; Collection Dashboard |
| Which salespeople are underperforming? | Salesman Performance | Below Target | Salesman Attention List; Salesman Detail drawer; Salesman Field Activity; Sales Report |
| Did the field team cover the route? | Sales Force Overview, then Salesman Field Activity | Execution %; Missed Visit; Effective Call Rate; GPS Valid Rate | Field table; missed visits; visit timeline |
| Which customers are declining? | Customer Portfolio Dashboard | Declining | Portfolio Priority Queue; Customer Performance Profile; Sales Report |
| Is warehouse capital trapped or about to run out? | Slow Moving & Dead Stock Dashboard and Inventory Forecast Dashboard | Dead Stock Value; Stock-Out Risk Items | Inventory Attention List; Warehouse Rebalancing; Inventory Report |
| Which warehouse holds idle stock? | Branch / Warehouse Performance Dashboard | Top Warehouse At-Risk %; Inactive Warehouse With Stock | Top 10 Warehouse by At-Risk Value; Inventory Report |
| Which customers dominate omzet or piutang? | Customer Analytics | Top Omzet Customer %; Top Piutang Customer % | Top 10 rankings; Customer Performance Profile; Customer Report |
| Which suppliers are becoming dominant? | Purchasing Management Dashboard | Top Principal %; Top 10 Principals | Principal Exposure Comparison; Supplier Performance Profile; Purchasing Report |
| Are we too dependent on a few salespeople? | Salesman Performance | Top Omzet Salesman % | Top 10 Omzet; Salesman Performance Profile; Sales Report |
| Is credit policy being respected? | Customer Analytics | Plafond Breach; Suspended + Sales | Customer Attention List; Customer Performance Profile; Sales Report |
| Are purchases posted or stuck? | Purchasing Management Dashboard | Pending Posting Value; Posted % | Purchasing Attention List; Purchasing Report |
| What needs attention right now (exceptions)? | Alert Center | Top Critical Alerts; Inventory Risk Summary | Investigate → related report; View Dashboard → domain dashboard |
| I have a name (customer, salesman, supplier, item) | Entity Analytics | Open Profile / Investigation / Compare | Performance Profile; Investigation Workspace; profile Evidence → related report |

---

# TRACEABILITY AND QUALITY

| Check | Result |
|-------|--------|
| Every EQ maps to MQs | EQ-001 routes to EQ-002–EQ-008. EQ-002–EQ-008 map to MQ-001–MQ-016 as in the catalog hierarchy. |
| Every MQ maps to actual assets | Pages and KPI titles taken from navigation-assets.md. Catalog-only phrases (Principal At-Risk Count, Top 1 Principal % as a tile, Purchasing Inactivity Flag, Recoverable Capital as a card, Management Escalation Count as a card) are mapped to discovered widgets, not invented. |
| Every MQ has a navigation flow | Section 2 Investigation Path. |
| Every MQ identifies evidence sources | Section 2 Step 4 and Section 4. |
| Every MQ identifies likely decisions | Section 2 Step 5, from catalog Expected Decision. |
| No invented dashboards / KPIs / reports | Desktop Report, Desktop Inquiry, and Export Data omitted because they were not discovered. |
| Interpretation | Uses category names, signal families, and the one discovered numeric target (GPS Valid Rate >95%). No invented thresholds. |

---

# Use in Phase-4C

Generate the Navigation Playbook from this file plus the two sources of truth.

```text
EQ-001
    ↓
Executive Question page (router)
    ↓
Management Question page
    ↓
Open → Review → Interpret → Evidence → Decide
```

Do not start the playbook from a dashboard list. Do not re-read the codebase to invent screens.
