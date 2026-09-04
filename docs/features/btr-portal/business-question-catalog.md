# BUSINESS QUESTION CATALOG

**Audience:** Owner, Director, Department Head, Manager, Analyst  
**Purpose:** Identify the business questions the existing BTR Portal can already answer. This catalog is the input for Phase-4 Question Navigation Mapping.  
**Sources of truth:** [kpi-inventory.md](./kpi-inventory.md), [kpi-encyclopedia.html](./kpi-encyclopedia.html)  
**Date:** 2 September 2026

This catalog does not design new dashboards, invent KPIs, or propose future features. Every question is aggregated from existing KPI families and from the business questions already stated in the KPI Encyclopedia.

---

# How to read this catalog

| Level | Audience | Use |
|-------|----------|-----|
| Level-1 Executive | Owner, Director, CEO | Morning judgment: health, risk, cash, plan, capital, dependence, discipline |
| Level-2 Management | Department Head, Manager, Supervisor | Where to look inside an area |
| Level-3 Investigative | Manager, Analyst, Owner during investigation | Why it is happening, and which names or signals to open next |

Questions are organized by **business intent**, not by dashboard and not by KPI.

BTR business terms are kept as used in the portal: Piutang, Omzet, Faktur, Plafond, Principal, Wilayah.

---

# Hierarchy map

```text
EQ-001  Is the business healthy?
        MQ-001  How healthy is the customer book over the next 30 days?
        MQ-002  Is the sales team delivering against plan?
        MQ-003  Is warehouse capital healthy?
        MQ-004  Is purchasing dependence and posting under control?

EQ-002  Where is the biggest business risk right now?
        MQ-005  Which customers are becoming risky?
        MQ-006  Which inventory is unhealthy or stuck?
        MQ-007  Which salespeople are carrying overdue or dormant books?
        MQ-008  Which principals are becoming a buying or stock risk?

EQ-003  What requires attention today?
        MQ-009  Who should be contacted today — collection, credit, or sales recovery?
        MQ-010  What inventory action is needed today — buy, delay, transfer, or clear?
        MQ-011  Which business areas are raising alerts?

EQ-004  Is cash coming back as fast as we are selling?
        MQ-012  How much piutang is outstanding, and how much is already overdue?
        MQ-013  Is overdue concentrated in a few customers, salespeople, or wilayah?
        MQ-014  Will this month’s collection close the gap?
        MQ-015  Are we collecting as fast as we are billing?

EQ-005  Will we hit this month’s sales plan?
        MQ-016  Will the current sales pace close the target gap?
        MQ-017  Which salespeople are underperforming, and do they have a target?
        MQ-018  Did the field team cover the planned route and produce orders?
        MQ-019  Who is producing omzet, and is the active force large enough?

EQ-006  Is warehouse capital working or trapped?
        MQ-020  How much capital sits in the warehouse, and in which categories?
        MQ-021  How much of that capital is slow, dead, or never sold?
        MQ-022  Will active SKUs run out, or are we overstocked — or both?
        MQ-023  How much idle capital could still be recovered?

EQ-007  Are we too dependent on a few relationships?
        MQ-024  Which customers dominate omzet or piutang?
        MQ-025  Which principals dominate buying and warehouse capital?
        MQ-026  Does one warehouse or wilayah hold too much stock, risk, or sales?

EQ-008  Is credit and purchasing discipline holding?
        MQ-027  Is credit policy actually being respected?
        MQ-028  Are purchased goods posted into inventory, or is there a backlog?
        MQ-029  Has purchasing gone quiet too late in the month?
```

---

# LEVEL-1
# EXECUTIVE QUESTIONS

## EQ-001

Question:

Is the business healthy?

Why This Matters:

An owner cannot manage a distributor from one total. Customer quality, sales delivery, warehouse capital, and purchasing control can move in different directions on the same day. This is the morning judgment before any deeper investigation.

Supported By:

- Business Intent: Cross-area business health
- Data Source: Customer Risk Assessment; Sales Target Achievement; Inventory Health; Supplier Dependency; Purchase Processing
- KPI: Portfolio Health Score; Portfolio Healthy %; Achievement %; Total Achievement (Total Omzet MTD); Total Target; Inventory Health Score; At-Risk Inventory %; Recovery vs Billing %; Domain Attention Summary Cards

---

## EQ-002

Question:

Where is the biggest business risk right now?

Why This Matters:

Attention is scarce. Overdue customers, stuck stock, weak sales books, and principal dependence can all look urgent. The owner needs to see which problem is the largest before sending Finance, Sales, Warehouse, or Purchasing into action.

Supported By:

- Business Intent: Cross-area risk
- Data Source: Customer Risk Assessment; Inventory Exposure; Customer Exposure (Salesman); Supplier Risk Monitoring; Domain Attention
- KPI: Customers At Risk Count; High Risk Customer Count; Overdue Exposure; At-Risk Inventory %; Inventory Risk Summary (Alert Center); Principal At-Risk Count; Compound Dependency Count; Alert Count — Customer; Alert Count — Collection; Alert Count — Inventory; Alert Count — Purchasing; Alert Count — Sales; Alert Count — Location; Domain Attention Summary Cards

---

## EQ-003

Question:

What requires attention today?

Why This Matters:

Monthly totals do not tell the owner what work should happen before the day ends. Collection contacts, inventory actions, and platform alerts are the portal’s “do this now” layer.

Supported By:

- Business Intent: Recovery Action Management; Inventory Action Queue; Platform attention
- Data Source: Collection Action Queue; Inventory Action Queue; Alert Center
- KPI: Actions Today; Immediate Collection Count; Critical Actions Count; Action Counts by Type (Purchase / Delay / Transfer / Clearance); Due Within 7 Days; Platform Alerts (Pinned); Alert Count — Customer; Alert Count — Collection; Alert Count — Inventory; Alert Count — Purchasing; Alert Count — Sales; Alert Count — Location; Domain Attention Summary Cards

---

## EQ-004

Question:

Is cash coming back as fast as we are selling?

Why This Matters:

In distribution, invoiced omzet is not cash. If billing outruns collection, working capital is trapped in piutang and the next purchase cycle is funded with delayed money. This is the cash-health question, not the sales-plan question.

Supported By:

- Business Intent: Risiko Piutang & Collection
- Data Source: Piutang Exposure; Collection Recovery; Cash Flow Forecast
- KPI: Total Piutang; Overdue Exposure; Piutang > 90 Hari (Amount & %); Recovery vs Billing %; Cash Collected MTD; Expected Cash Collection; Collection Gap; Collection Forecast %

---

## EQ-005

Question:

Will we hit this month’s sales plan?

Why This Matters:

Company achievement can look acceptable while many salespeople miss plan, routes are skipped, or the remaining days cannot close the gap. The owner needs to know whether the month will land, not only what has been billed so far.

Supported By:

- Business Intent: Pencapaian Target Penjualan; Produktivitas Lapangan
- Data Source: Sales Target Achievement; Sales Forecast; Visit Execution; Order Generation
- KPI: Achievement %; Total Achievement (Total Omzet MTD); Total Target; Current Achievement %; Forecast Achievement %; Target Gap; Forecast Risk Indicator; Below Target Count; Visit Execution %; Omzet Generated

---

## EQ-006

Question:

Is warehouse capital working or trapped?

Why This Matters:

Inventory is cash in physical form. A high warehouse value can fund sales or hide dead stock. The owner must know whether capital is moving, sitting idle, about to run out on selling SKUs, or waiting for a buy / delay / clearance decision.

Supported By:

- Business Intent: Nilai & Komposisi Persediaan; Risiko Persediaan; Kesehatan & Forecast Persediaan
- Data Source: Inventory Valuation; Inventory Aging; Inventory Health; Inventory Coverage
- KPI: Total Inventory Value; At-Risk Inventory %; Inventory Health Score; Dead Stock Count & Value; Slow Moving Count & Value; Stock-Out Risk Items / Value; Overstock / Understock Value; Recoverable Capital

---

## EQ-007

Question:

Are we too dependent on a few customers, salespeople, or suppliers?

Why This Matters:

A book, a sales force, or a warehouse can look healthy in total and still be one relationship away from a cash, omzet, or supply shock. Concentration is a strategic risk even when today’s aging or stock movement still looks acceptable.

Supported By:

- Business Intent: Customer Exposure Concentration; Revenue Production; Ketergantungan Supplier; Location concentration
- Data Source: Customer Exposure Concentration; Revenue Ranking; Principal Dependency; Supplier Dependency; Locations
- KPI: Top Customer % (Piutang Concentration); Top Omzet Customer %; Top Piutang Customer %; Top Omzet Salesman %; Top Piutang Salesman %; Top 1 Principal %; Top 3 Principal %; Top Principal % (Purchasing); Top Supplier % (Inventory); Compound Dependency Count; Top 1 Warehouse Inventory %; Top 1 Wilayah Sales %

---

## EQ-008

Question:

Is credit and purchasing discipline holding?

Why This Matters:

A healthy-looking month can still be leaking: customers buying above Plafond, suspended accounts still being billed, purchases sitting unposted, or buying going quiet while selling SKUs run out. These are control failures, not demand failures.

Supported By:

- Business Intent: Credit & Compliance; Kualitas Operasional Purchasing
- Data Source: Credit Control; Payment Discipline; Purchase Processing; Purchasing Backlog; Purchasing Activity
- KPI: Plafond Breach Count; Credit Limit Signal Count; Suspended + Sales Count; Posted %; Pending Posting (Count & Value); Pending Posting Value; Qualified Backlog Count & Value; Purchasing Inactivity Flag

---

# LEVEL-2
# MANAGEMENT QUESTIONS

## MQ-001

Question:

How healthy is the customer book over the next 30 days?

Parent Executive Question:

EQ-001

Why This Matters:

Overdue and dormant only appear after the problem has already happened. The 30-day portfolio view tells Sales and Finance whether risk is spreading before cash is late.

Supported By:

- Business Intent: Kesehatan Portofolio Customer
- Data Source: Customer Risk Assessment
- KPI: Portfolio Health Score; Portfolio Healthy %; Customers At Risk Count; High Risk Customer Count; Customers Forecasted at Risk; Portfolio Health Score (CU04)

---

## MQ-002

Question:

Is the sales team delivering against plan?

Parent Executive Question:

EQ-001

Why This Matters:

Company omzet can be carried by a few names while the rest of the force misses plan or has no plan at all. Management needs a team-level read before coaching individuals.

Supported By:

- Business Intent: Pencapaian Target Penjualan
- Data Source: Sales Target Achievement; Revenue Production
- KPI: Below Target Count; Missing Target Setup Count; Achievement %; Omzet Generated; Top 10 Achievement % (Ranking)

---

## MQ-003

Question:

Is warehouse capital healthy?

Parent Executive Question:

EQ-001

Why This Matters:

Total inventory value does not say whether stock will sell. Health is the share that is moving versus aging, plus whether selling SKUs still have cover.

Supported By:

- Business Intent: Kesehatan & Forecast Persediaan; Risiko Persediaan
- Data Source: Inventory Health; Inventory Exposure
- KPI: Inventory Health Score; At-Risk Inventory %; Aging Distribution (Movement Classes); Total Inventory Value

---

## MQ-004

Question:

Is purchasing dependence and posting under control?

Parent Executive Question:

EQ-001

Why This Matters:

Buying from one Principal, holding that Principal’s sick stock, and leaving invoices unposted are three different control failures. Management needs to see whether purchasing is still a managed process.

Supported By:

- Business Intent: Ketergantungan Supplier; Kualitas Operasional Purchasing
- Data Source: Principal Dependency; Purchase Processing; Purchasing Backlog
- KPI: Top 1 Principal %; Compound Dependency Count; Posted %; Qualified Backlog Count & Value; Principal At-Risk Count

---

## MQ-005

Question:

Which customers are becoming risky?

Parent Executive Question:

EQ-002

Why This Matters:

Collection time and credit attention must go to accounts that are weakening, not only to accounts that are already overdue. This is the preventive queue for Sales and Finance.

Supported By:

- Business Intent: Kesehatan Portofolio Customer
- Data Source: Customer Risk Assessment
- KPI: Customers At Risk Count; High Risk Customer Count; Customers Forecasted at Risk; Risk Category Count — Watch; Risk Category Count — Attention; Risk Category Count — High Risk; Risk Category Count — Critical; Strategic Customers At Risk Count; Elevated Risk Receivable; Elevated Risk Receivable %

---

## MQ-006

Question:

Which inventory is unhealthy or stuck?

Parent Executive Question:

EQ-002

Why This Matters:

Stuck stock is trapped cash. Slow movers become dead stock; never-sold receipts become tomorrow’s clearance problem. Management needs to see the unhealthy share before releasing more purchase cash.

Supported By:

- Business Intent: Risiko Persediaan
- Data Source: Inventory Aging; Inventory Exposure; Critical Inventory
- KPI: Dead Stock Count & Value; Slow Moving Count & Value; Never Sold Count & Value; At-Risk Inventory %; Aging Distribution (Movement Classes); Top 10 Dead / Slow Moving (Ranking); Inventory Risk Summary (Alert Center)

---

## MQ-007

Question:

Which salespeople are carrying overdue or dormant books?

Parent Executive Question:

EQ-002

Why This Matters:

A salesperson’s omzet can look strong while the owned book is late or going quiet. Those books become next month’s collection problem and next quarter’s lost offtake.

Supported By:

- Business Intent: Kualitas Portofolio Customer (Salesman)
- Data Source: Customer Retention; Customer Exposure
- KPI: Dormant Portfolio Count; High Overdue Exposure Count; High Piutang Exposure Count; Top Piutang Salesman %; Top Overdue Salesmen; Top 10 Piutang (Ranking)

---

## MQ-008

Question:

Which principals are becoming a buying or stock risk?

Parent Executive Question:

EQ-002

Why This Matters:

Continuing to buy a Principal whose goods are already aging in the warehouse compounds trapped capital. Management needs names, not only a company at-risk percentage.

Supported By:

- Business Intent: Risiko Supplier; Risiko Ketergantungan
- Data Source: Supplier Risk Monitoring; Dependency Risk; Category Exposure
- KPI: Principal At-Risk Count; Supplier Risk Exposure; Category Risk Exposure; Top 5 Suppliers (Critical Exposure); Top 5 Principals (Critical Exposure); Top 10 Supplier (Ranking)

---

## MQ-009

Question:

Who should be contacted today — collection, credit, or sales recovery?

Parent Executive Question:

EQ-003

Why This Matters:

Not every weak account needs the same work. Some need immediate collection, some a reminder before due date, some a credit review, some a sales visit, and some owner escalation. Mixing those jobs wastes the day.

Supported By:

- Business Intent: Recovery Action Management
- Data Source: Collection Action Queue
- KPI: Actions Today; Immediate Collection Count; Proactive Reminder Count; Credit Review Count; Sales Recovery Count; Management Escalation Count; Due Within 7 Days; Collection Impact Total; Immediate Impact Total

---

## MQ-010

Question:

What inventory action is needed today — buy, delay, transfer, or clear?

Parent Executive Question:

EQ-003

Why This Matters:

Holes and excess often exist at the same time. Buying everything that “feels empty” while slow movers age is how distributors trap cash. Management needs the first action, split by type.

Supported By:

- Business Intent: Optimasi Persediaan
- Data Source: Inventory Action Queue; Replenishment Planning
- KPI: Critical Actions Count; Action Counts by Type (Purchase / Delay / Transfer / Clearance); Recommended Purchase Qty (Indicative); Recommended Purchase Budget; Stock-Out Risk Items / Value

---

## MQ-011

Question:

Which business areas are raising alerts?

Parent Executive Question:

EQ-003

Why This Matters:

The owner should not open every dashboard to find the fire. Alert volume by area shows whether today’s problem is sales, customers, collection, inventory, purchasing, or location.

Supported By:

- Business Intent: Platform attention
- Data Source: Alert Center
- KPI: Alert Count — Sales; Alert Count — Customer; Alert Count — Collection; Alert Count — Inventory; Alert Count — Purchasing; Alert Count — Location; Platform Alerts (Pinned); Domain Attention Summary Cards; Inventory Risk Summary (Alert Center); Location Attention Signal Counts

---

## MQ-012

Question:

How much piutang is outstanding, and how much is already overdue?

Parent Executive Question:

EQ-004

Why This Matters:

Total piutang is scale. Overdue is urgency. Management that only watches the total will miss cash that has already aged past collectability.

Supported By:

- Business Intent: Risiko Piutang & Collection
- Data Source: Piutang Exposure
- KPI: Total Piutang; Total Open Balance (Portfolio); Overdue Exposure; Overdue Customer Count; Piutang > 90 Hari (Amount & %); >90 Day Exposure; Working Capital Tied Amount; Total Customer (with balance)

---

## MQ-013

Question:

Is overdue concentrated in a few customers, salespeople, or wilayah?

Parent Executive Question:

EQ-004

Why This Matters:

A concentrated overdue book is a relationship and coverage problem, not only a collections-capacity problem. The response changes if the same few names, books, or regions own most of the late cash.

Supported By:

- Business Intent: Customer Exposure Concentration; Collection ranking
- Data Source: Piutang Exposure; Collection Ranking; Locations
- KPI: Overdue Concentration %; Top Customer % (Piutang Concentration); Top 10 Customer %; Top 20 Customer %; Top 10 Outstanding Customers; Top Overdue Customers; Top Overdue Salesmen; Top Overdue Wilayah; Top 5 Customers (Critical Exposure)

---

## MQ-014

Question:

Will this month’s collection close the gap?

Parent Executive Question:

EQ-004

Why This Matters:

Knowing overdue today is not the same as knowing whether remaining days can recover the month. Finance needs the required daily pace, the expected finish, and whether the forecast is strong enough to act on.

Supported By:

- Business Intent: Risiko Piutang & Collection
- Data Source: Cash Flow Forecast; Collection Planning
- KPI: Expected Cash Collection; Projected Month-End Collection; Collection Forecast %; Daily Cash Collection Average; Required Daily Collection; Remaining Collection Target; Days Remaining (Collection); Collection Gap; Outstanding Due Remaining; Scenario Cash (Best / Expected / Worst); Forecast Confidence (Cash); Recovery vs Billing Forecast; Planning Confidence

---

## MQ-015

Question:

Are we collecting as fast as we are billing?

Parent Executive Question:

EQ-004

Why This Matters:

If new Faktur keep outrunning cash in, the book grows even when collectors are busy. This is the conversion of sales into cash, not the size of piutang.

Supported By:

- Business Intent: Pemulihan Collection
- Data Source: Collection Recovery
- KPI: Recovery vs Billing %; Recovery vs Billing % (Context); Cash Collected MTD; Collection Impact Total; Total Achievement (Total Omzet MTD)

---

## MQ-016

Question:

Will the current sales pace close this month’s target gap?

Parent Executive Question:

EQ-005

Why This Matters:

Current achievement is history. Required daily sales versus daily average, remaining days, and best / expected / worst finish tell management whether the month is still reachable.

Supported By:

- Business Intent: Pencapaian Target Penjualan
- Data Source: Sales Forecast
- KPI: Current Sales; Current Achievement %; Forecast Sales (Expected); Forecast Achievement %; Daily Average Sales; Required Daily Sales; Target Gap; Days Remaining (Sales); Scenario Projection (Best / Expected / Worst); Forecast Confidence; Forecast Risk Indicator; Weekly Invoiced Sales Trend; Target vs Achievement Chart; Total Target; Total Achievement (Total Omzet MTD)

---

## MQ-017

Question:

Which salespeople are underperforming, and do they even have a target?

Parent Executive Question:

EQ-005

Why This Matters:

“Below target” and “no target” are different management failures. Coaching a person who was never given a plan wastes time; ignoring people who have a plan and are missing it hides the real performance gap.

Supported By:

- Business Intent: Pencapaian Target Penjualan
- Data Source: Sales Target Achievement
- KPI: Below Target Count; Missing Target Setup Count; Top 10 Achievement % (Ranking); Principal Achievement Table

---

## MQ-018

Question:

Did the field team cover the planned route and produce orders?

Parent Executive Question:

EQ-005

Why This Matters:

Outcome and execution answer different questions. Visits without orders are activity without result. Orders without planned visits are unmanaged coverage. Both must be read together.

Supported By:

- Business Intent: Produktivitas Lapangan
- Data Source: Visit Execution; Sales Call Effectiveness; Order Generation
- KPI: Planned Visits; Actual Visits; Missed Visits; Unplanned Visits; Visit Execution %; Effective Calls; Effective Call Rate; Sales Orders; Orders Generated; Omzet Generated; GPS Valid Rate

---

## MQ-019

Question:

Who is producing omzet, and is the active force large enough for the plan?

Parent Executive Question:

EQ-005

Why This Matters:

A small active force, or omzet sitting with one or two people, makes the month fragile. Capacity and concentration are management issues before they become executive crises.

Supported By:

- Business Intent: Revenue Production; Kapasitas Tim Sales
- Data Source: Revenue Production; Sales Force Capacity; Revenue Ranking
- KPI: Omzet Generated; Top Omzet Salesman %; Top 10 Salesman (Omzet); Top 10 Omzet (Ranking); Top Omzet; Active Salesmen; Total Faktur; Total Customer (Sales)

---

## MQ-020

Question:

How much capital sits in the warehouse, and in which categories?

Parent Executive Question:

EQ-006

Why This Matters:

The owner funds inventory. Category composition shows whether that money is in a few lines that can shock the business if they stop moving.

Supported By:

- Business Intent: Nilai & Komposisi Persediaan
- Data Source: Inventory Valuation; Category Composition
- KPI: Total Inventory Value; Total Item; Top Category % (Inventory); Top 10 Category (Ranking); Top 5 Categories (Critical Exposure)

---

## MQ-021

Question:

How much of that capital is slow, dead, or never sold?

Parent Executive Question:

EQ-006

Why This Matters:

Aging classes are different decisions. Slow movers can still be sold; dead stock needs clearance; never-sold goods after intake are a buying-quality problem.

Supported By:

- Business Intent: Risiko Persediaan
- Data Source: Inventory Aging; Critical Inventory
- KPI: Aging Distribution (Movement Classes); Dead Stock Count & Value; Slow Moving Count & Value; Never Sold Count & Value; Top 10 Dead / Slow Moving (Ranking); At-Risk Inventory %

---

## MQ-022

Question:

Will active SKUs run out, or are we overstocked — or both?

Parent Executive Question:

EQ-006

Why This Matters:

Average days of supply can hide holes. A company can be overstocked on what does not sell and understocked on what does. Those two conditions require opposite actions in the same week.

Supported By:

- Business Intent: Kesehatan & Forecast Persediaan
- Data Source: Inventory Coverage; Inventory Forecast
- KPI: Average Days of Supply (Company); Days of Supply (Item); Stock-Out Risk Items / Value; Overstock / Understock Value; Projected Inventory Value @ Horizon; Scenario Projected Value (Best/Expected/Worst); Forecast Confidence (Inventory)

---

## MQ-023

Question:

How much idle capital could still be recovered?

Parent Executive Question:

EQ-006

Why This Matters:

Not all stuck stock is lost. Recoverable capital tells management how much cash a clearance, return, or delayed-buy programme could still release — and whether that dwarfs the next purchase budget.

Supported By:

- Business Intent: Optimasi Persediaan
- Data Source: Capital Recovery; Replenishment Planning
- KPI: Recoverable Capital; Recommended Purchase Budget; Dead Stock Count & Value; Slow Moving Count & Value; Action Counts by Type (Purchase / Delay / Transfer / Clearance)

---

## MQ-024

Question:

Which customers dominate omzet or piutang?

Parent Executive Question:

EQ-007

Why This Matters:

Growth that comes from a shrinking set of accounts, or piutang that sits with a few names, is concentration risk. Management must know whether the book is broad or dependent.

Supported By:

- Business Intent: Kontribusi Customer Terhadap Bisnis
- Data Source: Customer Revenue Contribution; Customer Exposure Concentration; Strategic Customer Portfolio
- KPI: Top Omzet Customer %; Top 10 Omzet (Ranking); Total MTD Omzet (Portfolio); Top Piutang Customer %; Top Customer % (Piutang Concentration); Top 10 Piutang (Ranking); Top 5 Customers (Critical Exposure); Strategic Customer Count; Working Capital Tied Amount

---

## MQ-025

Question:

Which principals dominate buying and warehouse capital?

Parent Executive Question:

EQ-007

Why This Matters:

Spend concentration and stock concentration are not the same. A Principal can take this month’s purchase cash, already hold the warehouse, and also own the sick stock. Those three views must be read together.

Supported By:

- Business Intent: Ketergantungan Supplier; Kontribusi Pembelian
- Data Source: Supplier Dependency; Principal Dependency; Purchase Volume; Principal Contribution
- KPI: Top 1 Principal %; Top 3 Principal %; Top Principal % (Purchasing); Top Supplier % (Inventory); Compound Dependency Count; Principal Exposure Comparison; Top 10 Principal (Ranking); Top 10 Supplier (Ranking); Grand Total Purchase; Top 5 Principals (Critical Exposure); Top 5 Suppliers (Critical Exposure)

---

## MQ-026

Question:

Does one warehouse or wilayah hold too much stock, risk, or sales?

Parent Executive Question:

EQ-007

Why This Matters:

Location concentration is an operating risk: one warehouse can hold most capital, most sick stock, or most sales. Inactive locations that still hold stock are capital sitting in the wrong place.

Supported By:

- Business Intent: Location concentration
- Data Source: Locations
- KPI: Top 1 Warehouse Inventory %; Top 3 Warehouse Inventory %; Top 1 Warehouse At-Risk %; Top 1 Warehouse Sales %; Top 1 Wilayah Sales %; Top Warehouse by Inventory (Ranking); Top Warehouse by At-Risk (Ranking); Top Warehouse by Sales (Ranking); Top Warehouse by Purchasing (Ranking); Top Wilayah by Sales (Ranking); Inactive Warehouse With Stock Count; Location Attention Signal Counts

---

## MQ-027

Question:

Is credit policy actually being respected?

Parent Executive Question:

EQ-008

Why This Matters:

Plafond and hold rules only protect cash if they are observed. Breaches, credit-limit signals, payment delay, and suspended accounts that are still being billed are the compliance view of the customer book.

Supported By:

- Business Intent: Credit & Compliance
- Data Source: Credit Control; Payment Discipline
- KPI: Plafond Breach Count; Credit Limit Signal Count; Payment Delay Signal Count; Collection Risk Signal Count; Suspended + Sales Count

---

## MQ-028

Question:

Are purchased goods posted into inventory, or is there a backlog?

Parent Executive Question:

EQ-008

Why This Matters:

Unposted purchases mean the company has already spent or committed without gaining sellable stock. Posting existing invoices can be a higher-priority action than placing new ones.

Supported By:

- Business Intent: Kualitas Operasional Purchasing
- Data Source: Purchase Processing; Purchasing Backlog
- KPI: Posted %; Pending Posting Value; Pending Posting (Count & Value); Qualified Backlog Count & Value; Grand Total Purchase; Total Invoice

---

## MQ-029

Question:

Has purchasing gone quiet too late in the month?

Parent Executive Question:

EQ-008

Why This Matters:

Silence after mid-month can be an intentional freeze or an accidental stall. Those are different decisions, especially if selling SKUs are already at stock-out risk.

Supported By:

- Business Intent: Kualitas Operasional Purchasing
- Data Source: Purchasing Activity
- KPI: Purchasing Inactivity Flag; Grand Total Purchase; Qualified Backlog Count & Value; Stock-Out Risk Items / Value

---

# LEVEL-3
# INVESTIGATIVE QUESTIONS

## IQ-001

Question:

Why is the customer book weakening?

Parent Management Question:

MQ-001

Why This Matters:

A falling health score is a headline. The mix of Watch versus High Risk versus Critical, and the money sitting with those accounts, tells Finance whether the problem is spreading or already expensive.

Supported By:

- Business Intent: Kesehatan Portofolio Customer
- Data Source: Customer Risk Assessment
- KPI: Risk Category Count — Healthy; Risk Category Count — Watch; Risk Category Count — Attention; Risk Category Count — High Risk; Risk Category Count — Critical; Elevated Risk Receivable; Elevated Risk Receivable %; Customers At Risk Count; High Risk Customer Count; Customers Forecasted at Risk

---

## IQ-002

Question:

Are the important customers the ones at risk?

Parent Management Question:

MQ-001

Why This Matters:

Risk in small accounts is a collections workload. Risk in strategic accounts is an owner-level relationship problem. Those two situations must not be managed as one list.

Supported By:

- Business Intent: Strategic Customer Portfolio; Customer Risk Assessment
- Data Source: Strategic Customer Portfolio; Customer Risk Assessment
- KPI: Strategic Customer Count; Strategic Customers At Risk Count; Strategic At Risk Count; Portfolio Healthy %

---

## IQ-003

Question:

Can we trust the 30-day customer risk forecast enough to act?

Parent Management Question:

MQ-001

Why This Matters:

Preventive collection and credit reviews should not be sized from a weak forecast. Confidence tells management whether to treat the 30-day view as a plan or as a caution.

Supported By:

- Business Intent: Kesehatan Portofolio Customer
- Data Source: Customer Risk Assessment
- KPI: Forecast Confidence; Portfolio Health Score

---

## IQ-004

Question:

Why is this customer risky?

Parent Management Question:

MQ-005

Why This Matters:

The next action depends on the signal: pay later, buy less, go quiet, approach Plafond, or fail collection. One risk label without the signal family leads to the wrong contact.

Supported By:

- Business Intent: Kesehatan Portofolio Customer; Credit & Compliance
- Data Source: Customer Risk Assessment; Payment Discipline; Credit Control
- KPI: Payment Delay Signal Count; Credit Limit Signal Count; Inactivity Signal Count; Purchase Decline Signal Count; Collection Risk Signal Count

---

## IQ-005

Question:

Which named customers carry the most overdue or outstanding cash?

Parent Management Question:

MQ-005

Why This Matters:

Counts do not collect money. Names do. Investigation must open the largest overdue and outstanding accounts before debating process.

Supported By:

- Business Intent: Risiko Piutang & Collection; Customer Exposure Concentration
- Data Source: Piutang Exposure; Customer Exposure Concentration
- KPI: Top Overdue Customers; Top 10 Outstanding Customers; Top 10 Piutang (Ranking); Top 5 Customers (Critical Exposure); Top Collection Risks (Table)

---

## IQ-006

Question:

Is the book still buying, going quiet, or never converted?

Parent Management Question:

MQ-005

Why This Matters:

Active, declining, dormant, and never-purchased customers require different work: protect, recover, or stop spending coverage time. Lifecycle is the growth-versus-attrition view of the same book.

Supported By:

- Business Intent: Customer Lifecycle
- Data Source: Customer Lifecycle
- KPI: Active Customer Count; Dormant Customer Count; Dormant Count (Lifecycle); Never Purchased Count; Declining Count; Inactivity Signal Count; Purchase Decline Signal Count

---

## IQ-007

Question:

How much piutang is already more than 90 days old?

Parent Management Question:

MQ-012

Why This Matters:

Cash older than 90 days is chronic capital. It changes the conversation from “collect this week” to “recover, restructure, or stop supply.”

Supported By:

- Business Intent: Risiko Piutang & Collection
- Data Source: Piutang Exposure; Collection Planning
- KPI: Piutang > 90 Hari (Amount & %); >90 Day Exposure; >90d Exposure; Aging Bucket — > 90 Days; Aging Risk Summary — > 90 Days; Legacy Debt Count

---

## IQ-008

Question:

Where is overdue sitting by age?

Parent Management Question:

MQ-012

Why This Matters:

Current versus 1–30 versus 61–90 versus >90 days are different collection plays. A book sliding from current into early overdue is still recoverable; a book stacked beyond 90 days is not the same job.

Supported By:

- Business Intent: Risiko Piutang & Collection
- Data Source: Piutang Exposure
- KPI: Aging Bucket — Current; Aging Bucket — 1–30 Days; Aging Bucket — 31–60 Days; Aging Bucket — 61–90 Days; Aging Bucket — > 90 Days; Aging Risk Summary — 1–30 Days; Aging Risk Summary — 31–60 Days; Aging Risk Summary — 61–90 Days; Aging Risk Summary — > 90 Days

---

## IQ-009

Question:

Which salespeople or wilayah are driving the overdue?

Parent Management Question:

MQ-013

Why This Matters:

If overdue is a person or region problem, Finance cannot fix it alone. Sales ownership of the book has to join the collection plan.

Supported By:

- Business Intent: Collection ranking; Customer Exposure (Salesman)
- Data Source: Collection Ranking; Customer Exposure
- KPI: Top Overdue Salesmen; Top Overdue Wilayah; High Overdue Exposure Count; High Piutang Exposure Count; Top Piutang Salesman %; Top 10 Piutang (Ranking)

---

## IQ-010

Question:

What daily collection pace is required, and what finish do the scenarios imply?

Parent Management Question:

MQ-014

Why This Matters:

A gap that cannot be closed at the current daily average needs a different plan than a gap that remaining days can still cover. Best / expected / worst cash shows how much uncertainty sits in that plan.

Supported By:

- Business Intent: Risiko Piutang & Collection
- Data Source: Cash Flow Forecast
- KPI: Daily Cash Collection Average; Required Daily Collection; Days Remaining (Collection); Remaining Collection Target; Collection Gap; Expected Cash Collection; Projected Month-End Collection; Scenario Cash (Best / Expected / Worst); Forecast Confidence (Cash); Outstanding Due Remaining

---

## IQ-011

Question:

How is cash actually arriving — cash, giro, or adjustment?

Parent Management Question:

MQ-015

Why This Matters:

Reported collection is not always spendable cash. Mix tells Finance whether the month is being closed with real receipts or with paper that still has to clear.

Supported By:

- Business Intent: Pemulihan Collection
- Data Source: Collection Recovery
- KPI: Payment Mix — Cash; Payment Mix — Giro; Payment Mix — Adjustment; Cash Collected MTD

---

## IQ-012

Question:

How much cash is at stake in the next 7 days versus in legacy debt?

Parent Management Question:

MQ-009

Why This Matters:

Work this week should prefer invoices about to go late over debt that has already become chronic — unless the legacy names are also the largest exposure. Planning confidence says whether that split is usable.

Supported By:

- Business Intent: Perencanaan Collection; Recovery Action Management
- Data Source: Collection Planning; Collection Action Queue
- KPI: Due Within 7 Days; Legacy Debt Count; Planning Confidence; Overdue Exposure (Context); Proactive Reminder Count; Immediate Collection Count

---

## IQ-013

Question:

What kind of contact does each account need?

Parent Management Question:

MQ-009

Why This Matters:

A single “actions today” total can hide an all-collection day or an all-escalation day. Split by type tells supervisors who should work the list.

Supported By:

- Business Intent: Recovery Action Management
- Data Source: Collection Action Queue
- KPI: Immediate Collection Count; Proactive Reminder Count; Credit Review Count; Sales Recovery Count; Management Escalation Count; Immediate Impact Total; Collection Impact Total

---

## IQ-014

Question:

Is company achievement being carried by remaining days, or is the month already lost?

Parent Management Question:

MQ-016

Why This Matters:

A large target gap with few days left is a different conversation from a small gap with a healthy daily average. Forecast risk and confidence tell the owner whether to push, protect, or accept the finish.

Supported By:

- Business Intent: Pencapaian Target Penjualan
- Data Source: Sales Forecast
- KPI: Target Gap; Days Remaining (Sales); Daily Average Sales; Required Daily Sales; Forecast Achievement %; Forecast Risk Indicator; Forecast Confidence; Scenario Projection (Best / Expected / Worst); Weekly Invoiced Sales Trend

---

## IQ-015

Question:

Is underperformance a missed plan, a missing plan, or a principal-mix problem?

Parent Management Question:

MQ-017

Why This Matters:

Coaching the wrong cause wastes the week. No target is administration. Missed target is performance. Missed target on one Principal is a range or buying problem, not necessarily a salesperson problem.

Supported By:

- Business Intent: Pencapaian Target Penjualan
- Data Source: Sales Target Achievement
- KPI: Below Target Count; Missing Target Setup Count; Principal Achievement Table; Top 10 Achievement % (Ranking)

---

## IQ-016

Question:

Why did visits fail — coverage, conversion, or credibility?

Parent Management Question:

MQ-018

Why This Matters:

Missed visits are a route problem. Low effective-call rate with high visit execution is a conversion problem. A perfect execution rate with weak GPS is a credibility problem. Those three causes need different supervisors.

Supported By:

- Business Intent: Produktivitas Lapangan
- Data Source: Visit Execution; Sales Call Effectiveness
- KPI: Planned Visits; Actual Visits; Missed Visits; Unplanned Visits; Visit Execution %; Top Visit Execution; Bottom Visit Execution; Effective Calls; Effective Call Rate; Top Effective Call Rate; Bottom Effective Call Rate; GPS Valid Rate

---

## IQ-017

Question:

Are field orders turning into billed omzet?

Parent Management Question:

MQ-018

Why This Matters:

Top orders without top omzet means the pipeline is not becoming Faktur. Top omzet without field orders means billing is coming from somewhere other than today’s route. Investigation must not treat those as the same success.

Supported By:

- Business Intent: Produktivitas Lapangan
- Data Source: Order Generation; Revenue Ranking
- KPI: Sales Orders; Orders Generated; Top Orders; Omzet Generated; Top Omzet; Top 10 Salesman (Omzet)

---

## IQ-018

Question:

Who should be coached this week — and for what?

Parent Management Question:

MQ-019

Why This Matters:

The top omzet list, the top achievement list, and the bottom visit list are not the same people. Coaching has to name the person and the gap: money, plan, route, or conversion.

Supported By:

- Business Intent: Ranking & Tolok Ukur
- Data Source: Revenue Ranking; Productivity Ranking; Order Ranking
- KPI: Top 10 Omzet (Ranking); Top 10 Achievement % (Ranking); Top 10 Salesman (Omzet); Bottom Visit Execution; Bottom Effective Call Rate; Top Orders; Active Salesmen

---

## IQ-019

Question:

Which SKUs are the worst dead or slow movers?

Parent Management Question:

MQ-021

Why This Matters:

Category percentages do not clear a warehouse. SKU names do. Clearance, return, or stop-buy decisions start with the worst items.

Supported By:

- Business Intent: Risiko Persediaan
- Data Source: Critical Inventory; Inventory Aging
- KPI: Top 10 Dead / Slow Moving (Ranking); Dead Stock Count & Value; Slow Moving Count & Value; Never Sold Count & Value

---

## IQ-020

Question:

Which categories and suppliers own the sick stock?

Parent Management Question:

MQ-021

Why This Matters:

If risk sits in one category or one Principal, purchasing policy can change. If risk is scattered, the problem is assortment-wide, not a single buying error.

Supported By:

- Business Intent: Risiko Persediaan; Risiko Supplier
- Data Source: Category Exposure; Dependency Risk
- KPI: Category Risk Exposure; Supplier Risk Exposure; Top 5 Categories (Critical Exposure); Top Category % (Inventory); Top 10 Category (Ranking)

---

## IQ-021

Question:

Which items are about to run out, and which are already too deep?

Parent Management Question:

MQ-022

Why This Matters:

Average cover hides both holes and piles. Item days of supply, stock-out risk, and overstock / understock must be read together before anyone releases purchase cash.

Supported By:

- Business Intent: Kesehatan & Forecast Persediaan; Optimasi Persediaan
- Data Source: Inventory Coverage; Replenishment Planning
- KPI: Days of Supply (Item); Average Days of Supply (Company); Stock-Out Risk Items / Value; Overstock / Understock Value; Recommended Purchase Qty (Indicative); Forecast Confidence (Inventory)

---

## IQ-022

Question:

What purchase budget is indicated, and how does it compare with capital we could still recover?

Parent Management Question:

MQ-023

Why This Matters:

Buying more while recoverable idle stock sits in the warehouse is a cash decision, not a warehouse feeling. Forecast confidence tells whether the indicated buy is firm enough to fund.

Supported By:

- Business Intent: Optimasi Persediaan
- Data Source: Replenishment Planning; Capital Recovery; Inventory Forecast
- KPI: Recommended Purchase Budget; Recommended Purchase Qty (Indicative); Recoverable Capital; Projected Inventory Value @ Horizon; Scenario Projected Value (Best/Expected/Worst); Forecast Confidence (Inventory)

---

## IQ-023

Question:

What is the first inventory action — buy, delay, transfer, post, or clear?

Parent Management Question:

MQ-010

Why This Matters:

A single critical-action count does not say what to do. Split by type is the work list for Purchasing and Warehouse.

Supported By:

- Business Intent: Optimasi Persediaan
- Data Source: Inventory Action Queue
- KPI: Action Counts by Type (Purchase / Delay / Transfer / Clearance); Critical Actions Count; Recommended Purchase Qty (Indicative); Recoverable Capital

---

## IQ-024

Question:

Is Principal dependence spend, stock, sick stock, or all three?

Parent Management Question:

MQ-025

Why This Matters:

A Principal that takes purchase cash but not warehouse capital is a buying-mix issue. A Principal that already holds sick stock is a stop-buy issue. Compound dependence is both, and it is the highest-risk pattern.

Supported By:

- Business Intent: Ketergantungan Supplier
- Data Source: Principal Dependency; Supplier Dependency
- KPI: Compound Dependency Count; Principal Exposure Comparison; Top 1 Principal %; Top 3 Principal %; Top Principal % (Purchasing); Top Supplier % (Inventory); Top 10 Principal (Ranking); Top 10 Supplier (Ranking)

---

## IQ-025

Question:

Which principals are already risky in the warehouse while we still buy from them?

Parent Management Question:

MQ-008

Why This Matters:

The dangerous name is the one that is both a large spend and an at-risk stock holder. Ranking purchase volume alone will miss that combination.

Supported By:

- Business Intent: Risiko Supplier; Kontribusi Pembelian
- Data Source: Supplier Risk Monitoring; Principal Contribution
- KPI: Principal At-Risk Count; Supplier Risk Exposure; Top 10 Principal (Ranking); Top 5 Principals (Critical Exposure); Top 5 Suppliers (Critical Exposure); Grand Total Purchase

---

## IQ-026

Question:

How much purchasing value is still unposted versus already an aged backlog?

Parent Management Question:

MQ-028

Why This Matters:

Pending posting is still recoverable by completing the process. Qualified backlog is aged incomplete work. New buys on top of either one add capital without adding sellable stock.

Supported By:

- Business Intent: Kualitas Operasional Purchasing
- Data Source: Purchase Processing; Purchasing Backlog
- KPI: Posted %; Pending Posting Value; Pending Posting (Count & Value); Qualified Backlog Count & Value; Total Invoice; Grand Total Purchase

---

## IQ-027

Question:

Is purchasing inactivity coinciding with stock-out risk or with unposted backlog?

Parent Management Question:

MQ-029

Why This Matters:

A quiet month is acceptable if it is a freeze with full warehouses. It is not acceptable if selling SKUs are about to run out, or if invoices are waiting to be posted. Those facts change the owner decision.

Supported By:

- Business Intent: Kualitas Operasional Purchasing; Inventory Coverage
- Data Source: Purchasing Activity; Inventory Coverage; Purchasing Backlog
- KPI: Purchasing Inactivity Flag; Stock-Out Risk Items / Value; Qualified Backlog Count & Value; Pending Posting Value

---

## IQ-028

Question:

Does one warehouse hold most of the capital or most of the sick stock?

Parent Management Question:

MQ-026

Why This Matters:

Company inventory health can hide a single location that is overloaded or aging. Transfer, clearance, and buying decisions are location decisions once the name is known.

Supported By:

- Business Intent: Location concentration
- Data Source: Locations
- KPI: Top 1 Warehouse Inventory %; Top 3 Warehouse Inventory %; Top 1 Warehouse At-Risk %; Top Warehouse by Inventory (Ranking); Top Warehouse by At-Risk (Ranking); Inactive Warehouse With Stock Count

---

## IQ-029

Question:

Does one warehouse or wilayah dominate sales or buying?

Parent Management Question:

MQ-026

Why This Matters:

Sales or purchasing sitting in one location is a coverage and supply-chain risk. It is a different problem from stock sitting in one warehouse.

Supported By:

- Business Intent: Location concentration
- Data Source: Locations
- KPI: Top 1 Warehouse Sales %; Top 1 Wilayah Sales %; Top Warehouse by Sales (Ranking); Top Warehouse by Purchasing (Ranking); Top Wilayah by Sales (Ranking); Location Attention Signal Counts

---

## IQ-030

Question:

How many accounts have already broken Plafond, versus only approaching the limit?

Parent Management Question:

MQ-027

Why This Matters:

A credit-limit signal is still preventable. A breach is already a policy failure. Suspended accounts that are still being billed are a second failure on top of the first.

Supported By:

- Business Intent: Credit & Compliance
- Data Source: Credit Control; Payment Discipline
- KPI: Credit Limit Signal Count; Plafond Breach Count; Suspended + Sales Count; Payment Delay Signal Count; Collection Risk Signal Count

---

## IQ-031

Question:

Which alert domain should be opened first?

Parent Management Question:

MQ-011

Why This Matters:

Pinned platform alerts and per-domain counts are the shortest path from “something is wrong” to the right management question. Investigation should follow the largest or pinned domain, not habit.

Supported By:

- Business Intent: Platform attention
- Data Source: Alert Center
- KPI: Platform Alerts (Pinned); Alert Count — Customer; Alert Count — Collection; Alert Count — Sales; Alert Count — Inventory; Alert Count — Purchasing; Alert Count — Location; Domain Attention Summary Cards; Inventory Risk Summary (Alert Center); Location Attention Signal Counts

---

## IQ-032

Question:

Is salesperson piutang exposure late, or only large?

Parent Management Question:

MQ-007

Why This Matters:

A large outstanding book that is still current is a credit-size issue. A large overdue book is a collection-discipline issue. Mixing those two lists sends the wrong people into the same meeting.

Supported By:

- Business Intent: Kualitas Portofolio Customer (Salesman)
- Data Source: Customer Exposure; Collection Ranking
- KPI: High Piutang Exposure Count; High Overdue Exposure Count; Top Piutang Salesman %; Top 10 Piutang (Ranking); Top Overdue Salesmen

---

# TRACEABILITY MATRIX

| Question ID | Question | Supported By KPI |
|-------------|----------|------------------|
| EQ-001 | Is the business healthy? | Portfolio Health Score, Portfolio Healthy %, Achievement %, Inventory Health Score, Recovery vs Billing %, Domain Attention Summary Cards |
| EQ-002 | Where is the biggest business risk right now? | Customers At Risk Count, Overdue Exposure, At-Risk Inventory %, Principal At-Risk Count, Compound Dependency Count, Alert Counts (all domains) |
| EQ-003 | What requires attention today? | Actions Today, Immediate Collection Count, Critical Actions Count, Action Counts by Type, Due Within 7 Days, Platform Alerts, Alert Counts |
| EQ-004 | Is cash coming back as fast as we are selling? | Total Piutang, Overdue Exposure, Piutang > 90 Hari, Recovery vs Billing %, Cash Collected MTD, Expected Cash Collection, Collection Gap |
| EQ-005 | Will we hit this month’s sales plan? | Achievement %, Total Target, Forecast Achievement %, Target Gap, Forecast Risk Indicator, Below Target Count, Visit Execution % |
| EQ-006 | Is warehouse capital working or trapped? | Total Inventory Value, At-Risk Inventory %, Inventory Health Score, Dead Stock Count & Value, Stock-Out Risk Items / Value, Recoverable Capital |
| EQ-007 | Are we too dependent on a few customers, salespeople, or suppliers? | Top Customer % (Piutang), Top Omzet Customer %, Top Omzet Salesman %, Top 1 Principal %, Top Supplier %, Compound Dependency Count, Top 1 Warehouse Inventory % |
| EQ-008 | Is credit and purchasing discipline holding? | Plafond Breach Count, Credit Limit Signal Count, Suspended + Sales Count, Posted %, Pending Posting, Qualified Backlog Count & Value, Purchasing Inactivity Flag |
| MQ-001 | How healthy is the customer book over the next 30 days? | Portfolio Health Score, Portfolio Healthy %, Customers At Risk Count, High Risk Customer Count, Customers Forecasted at Risk |
| MQ-002 | Is the sales team delivering against plan? | Below Target Count, Missing Target Setup Count, Achievement %, Omzet Generated, Top 10 Achievement % |
| MQ-003 | Is warehouse capital healthy? | Inventory Health Score, At-Risk Inventory %, Aging Distribution, Total Inventory Value |
| MQ-004 | Is purchasing dependence and posting under control? | Top 1 Principal %, Compound Dependency Count, Posted %, Qualified Backlog Count & Value, Principal At-Risk Count |
| MQ-005 | Which customers are becoming risky? | Customers At Risk Count, High Risk Customer Count, Risk Category Counts (Watch / Attention / High Risk / Critical), Strategic Customers At Risk Count, Elevated Risk Receivable % |
| MQ-006 | Which inventory is unhealthy or stuck? | Dead Stock Count & Value, Slow Moving Count & Value, Never Sold Count & Value, At-Risk Inventory %, Top 10 Dead / Slow Moving |
| MQ-007 | Which salespeople are carrying overdue or dormant books? | Dormant Portfolio Count, High Overdue Exposure Count, High Piutang Exposure Count, Top Overdue Salesmen, Top Piutang Salesman % |
| MQ-008 | Which principals are becoming a buying or stock risk? | Principal At-Risk Count, Supplier Risk Exposure, Category Risk Exposure, Top 5 Suppliers (Critical Exposure), Top 5 Principals (Critical Exposure) |
| MQ-009 | Who should be contacted today — collection, credit, or sales recovery? | Actions Today, Immediate Collection Count, Proactive Reminder Count, Credit Review Count, Sales Recovery Count, Management Escalation Count |
| MQ-010 | What inventory action is needed today — buy, delay, transfer, or clear? | Critical Actions Count, Action Counts by Type, Recommended Purchase Qty, Recommended Purchase Budget, Stock-Out Risk Items / Value |
| MQ-011 | Which business areas are raising alerts? | Alert Count — Sales / Customer / Collection / Inventory / Purchasing / Location, Platform Alerts, Domain Attention Summary Cards |
| MQ-012 | How much piutang is outstanding, and how much is already overdue? | Total Piutang, Overdue Exposure, Overdue Customer Count, Piutang > 90 Hari, Working Capital Tied Amount |
| MQ-013 | Is overdue concentrated in a few customers, salespeople, or wilayah? | Overdue Concentration %, Top Customer % (Piutang), Top Overdue Customers, Top Overdue Salesmen, Top Overdue Wilayah |
| MQ-014 | Will this month’s collection close the gap? | Expected Cash Collection, Projected Month-End Collection, Required Daily Collection, Collection Gap, Scenario Cash, Forecast Confidence (Cash) |
| MQ-015 | Are we collecting as fast as we are billing? | Recovery vs Billing %, Cash Collected MTD, Collection Impact Total |
| MQ-016 | Will the current sales pace close this month’s target gap? | Current Achievement %, Forecast Achievement %, Daily Average Sales, Required Daily Sales, Target Gap, Scenario Projection, Forecast Risk Indicator |
| MQ-017 | Which salespeople are underperforming, and do they have a target? | Below Target Count, Missing Target Setup Count, Top 10 Achievement %, Principal Achievement Table |
| MQ-018 | Did the field team cover the planned route and produce orders? | Planned / Actual / Missed / Unplanned Visits, Visit Execution %, Effective Call Rate, Orders Generated, GPS Valid Rate |
| MQ-019 | Who is producing omzet, and is the active force large enough? | Omzet Generated, Top Omzet Salesman %, Top 10 Salesman (Omzet), Active Salesmen |
| MQ-020 | How much capital sits in the warehouse, and in which categories? | Total Inventory Value, Top Category % (Inventory), Top 10 Category, Top 5 Categories (Critical Exposure) |
| MQ-021 | How much of that capital is slow, dead, or never sold? | Aging Distribution, Dead Stock Count & Value, Slow Moving Count & Value, Never Sold Count & Value, Top 10 Dead / Slow Moving |
| MQ-022 | Will active SKUs run out, or are we overstocked — or both? | Average Days of Supply, Stock-Out Risk Items / Value, Overstock / Understock Value, Days of Supply (Item), Forecast Confidence (Inventory) |
| MQ-023 | How much idle capital could still be recovered? | Recoverable Capital, Recommended Purchase Budget, Dead Stock Count & Value, Action Counts by Type |
| MQ-024 | Which customers dominate omzet or piutang? | Top Omzet Customer %, Top 10 Omzet, Top Piutang Customer %, Top Customer % (Piutang), Top 5 Customers (Critical Exposure), Strategic Customer Count |
| MQ-025 | Which principals dominate buying and warehouse capital? | Top 1 / Top 3 Principal %, Top Supplier %, Compound Dependency Count, Principal Exposure Comparison, Top 10 Principal, Grand Total Purchase |
| MQ-026 | Does one warehouse or wilayah hold too much stock, risk, or sales? | Top 1 Warehouse Inventory %, Top 1 Warehouse At-Risk %, Top 1 Warehouse Sales %, Top 1 Wilayah Sales %, Inactive Warehouse With Stock Count |
| MQ-027 | Is credit policy actually being respected? | Plafond Breach Count, Credit Limit Signal Count, Payment Delay Signal Count, Collection Risk Signal Count, Suspended + Sales Count |
| MQ-028 | Are purchased goods posted into inventory, or is there a backlog? | Posted %, Pending Posting Value, Pending Posting (Count & Value), Qualified Backlog Count & Value |
| MQ-029 | Has purchasing gone quiet too late in the month? | Purchasing Inactivity Flag, Grand Total Purchase, Qualified Backlog Count & Value, Stock-Out Risk Items / Value |
| IQ-001 | Why is the customer book weakening? | Risk Category Counts, Elevated Risk Receivable, Elevated Risk Receivable %, Customers At Risk Count, High Risk Customer Count |
| IQ-002 | Are the important customers the ones at risk? | Strategic Customer Count, Strategic Customers At Risk Count, Strategic At Risk Count |
| IQ-003 | Can we trust the 30-day customer risk forecast enough to act? | Forecast Confidence, Portfolio Health Score |
| IQ-004 | Why is this customer risky? | Payment Delay Signal Count, Credit Limit Signal Count, Inactivity Signal Count, Purchase Decline Signal Count, Collection Risk Signal Count |
| IQ-005 | Which named customers carry the most overdue or outstanding cash? | Top Overdue Customers, Top 10 Outstanding Customers, Top 10 Piutang, Top 5 Customers (Critical Exposure), Top Collection Risks |
| IQ-006 | Is the book still buying, going quiet, or never converted? | Active Customer Count, Dormant Customer Count, Never Purchased Count, Declining Count, Inactivity Signal Count, Purchase Decline Signal Count |
| IQ-007 | How much piutang is already more than 90 days old? | Piutang > 90 Hari, >90 Day Exposure, Aging Bucket — > 90 Days, Legacy Debt Count |
| IQ-008 | Where is overdue sitting by age? | Aging Buckets (Current, 1–30, 31–60, 61–90, >90), Aging Risk Summaries |
| IQ-009 | Which salespeople or wilayah are driving the overdue? | Top Overdue Salesmen, Top Overdue Wilayah, High Overdue Exposure Count, High Piutang Exposure Count |
| IQ-010 | What daily collection pace is required, and what finish do the scenarios imply? | Daily Cash Collection Average, Required Daily Collection, Collection Gap, Scenario Cash, Forecast Confidence (Cash) |
| IQ-011 | How is cash actually arriving — cash, giro, or adjustment? | Payment Mix — Cash, Payment Mix — Giro, Payment Mix — Adjustment, Cash Collected MTD |
| IQ-012 | How much cash is at stake in the next 7 days versus in legacy debt? | Due Within 7 Days, Legacy Debt Count, Planning Confidence, Immediate Collection Count, Proactive Reminder Count |
| IQ-013 | What kind of contact does each account need? | Immediate Collection Count, Proactive Reminder Count, Credit Review Count, Sales Recovery Count, Management Escalation Count |
| IQ-014 | Is company achievement being carried by remaining days, or is the month already lost? | Target Gap, Days Remaining (Sales), Required Daily Sales, Forecast Achievement %, Forecast Risk Indicator, Scenario Projection |
| IQ-015 | Is underperformance a missed plan, a missing plan, or a principal-mix problem? | Below Target Count, Missing Target Setup Count, Principal Achievement Table, Top 10 Achievement % |
| IQ-016 | Why did visits fail — coverage, conversion, or credibility? | Missed Visits, Visit Execution %, Bottom Visit Execution, Effective Call Rate, Bottom Effective Call Rate, GPS Valid Rate |
| IQ-017 | Are field orders turning into billed omzet? | Sales Orders, Orders Generated, Top Orders, Omzet Generated, Top Omzet |
| IQ-018 | Who should be coached this week — and for what? | Top 10 Omzet, Top 10 Achievement %, Bottom Visit Execution, Bottom Effective Call Rate, Active Salesmen |
| IQ-019 | Which SKUs are the worst dead or slow movers? | Top 10 Dead / Slow Moving, Dead Stock Count & Value, Slow Moving Count & Value, Never Sold Count & Value |
| IQ-020 | Which categories and suppliers own the sick stock? | Category Risk Exposure, Supplier Risk Exposure, Top 5 Categories (Critical Exposure), Top Category % |
| IQ-021 | Which items are about to run out, and which are already too deep? | Days of Supply (Item), Stock-Out Risk Items / Value, Overstock / Understock Value, Recommended Purchase Qty |
| IQ-022 | What purchase budget is indicated, and how does it compare with capital we could still recover? | Recommended Purchase Budget, Recoverable Capital, Projected Inventory Value, Forecast Confidence (Inventory) |
| IQ-023 | What is the first inventory action — buy, delay, transfer, post, or clear? | Action Counts by Type, Critical Actions Count, Recommended Purchase Qty, Recoverable Capital |
| IQ-024 | Is Principal dependence spend, stock, sick stock, or all three? | Compound Dependency Count, Principal Exposure Comparison, Top 1 Principal %, Top Supplier % |
| IQ-025 | Which principals are already risky in the warehouse while we still buy from them? | Principal At-Risk Count, Supplier Risk Exposure, Top 10 Principal, Top 5 Principals (Critical Exposure) |
| IQ-026 | How much purchasing value is still unposted versus already an aged backlog? | Posted %, Pending Posting Value, Qualified Backlog Count & Value |
| IQ-027 | Is purchasing inactivity coinciding with stock-out risk or with unposted backlog? | Purchasing Inactivity Flag, Stock-Out Risk Items / Value, Qualified Backlog Count & Value, Pending Posting Value |
| IQ-028 | Does one warehouse hold most of the capital or most of the sick stock? | Top 1 Warehouse Inventory %, Top 1 Warehouse At-Risk %, Top Warehouse by Inventory, Top Warehouse by At-Risk, Inactive Warehouse With Stock Count |
| IQ-029 | Does one warehouse or wilayah dominate sales or buying? | Top 1 Warehouse Sales %, Top 1 Wilayah Sales %, Top Warehouse by Sales, Top Warehouse by Purchasing, Top Wilayah by Sales |
| IQ-030 | How many accounts have already broken Plafond, versus only approaching the limit? | Credit Limit Signal Count, Plafond Breach Count, Suspended + Sales Count |
| IQ-031 | Which alert domain should be opened first? | Platform Alerts, Alert Counts (all domains), Domain Attention Summary Cards |
| IQ-032 | Is salesperson piutang exposure late, or only large? | High Piutang Exposure Count, High Overdue Exposure Count, Top Piutang Salesman %, Top Overdue Salesmen |

---

# Quality notes

## Counts

| Level | Count |
|-------|-------|
| Executive questions | 8 |
| Management questions | 29 |
| Investigative questions | 32 |
| **Total** | **69** |

## Hierarchy checks

- Every management question has a parent executive question.
- Every investigative question has a parent management question.
- Questions are grouped by business intent (health, risk, today’s work, cash, sales plan, warehouse capital, dependence, discipline), not by dashboard.

## Merge decisions

Similar KPI families were aggregated into one decision question:

| Merged family | Represented by |
|---------------|----------------|
| Aging buckets and aging risk summaries | IQ-008 |
| Five customer risk-category counts | IQ-001, MQ-005 |
| Collection action-type counts | MQ-009, IQ-013 |
| Inventory action-type counts | MQ-010, IQ-023 |
| Payment mix (cash / giro / adjustment) | IQ-011 |
| Ranking lists (top 10 omzet / piutang / visits) | IQ-005, IQ-018, IQ-019 |
| Sales and collection forecast scenarios | IQ-010, IQ-014 |
| Report-footer totals | Same questions as the live totals they repeat |

## KPI families not given their own question

These inventory entries exist but were not turned into standalone questions, because they duplicate another question or do not represent a business decision:

| KPI family | Why not a separate question |
|------------|-----------------------------|
| Snapshot Freshness (Last Refreshed) | Data freshness, not a commercial decision |
| Report Footer totals (Piutang, Customer, Inventory, Purchase, Invoice) | Duplicate of the live totals already used above |
| Customer Report Row Metrics | Report layout, not a decision question |
| Context copies (Total Piutang Context, Overdue Exposure Context, Recovery vs Billing Context) | Same intent as the primary KPI |

## Encyclopedia questions covered

Every business question stated in the KPI Encyclopedia areas is represented:

| Encyclopedia question | Catalog ID |
|-----------------------|------------|
| How healthy is the customer book over the next 30 days? | MQ-001 |
| How much receivable is outstanding, and how much of it is already late? | MQ-012 |
| Which customers create the highest cash and relationship risk? | MQ-005, IQ-005 |
| Is the book still buying, going quiet, or never converted? | IQ-006 |
| Are we collecting as fast as we are billing? | MQ-015 |
| Who should be contacted today, and is that work collection, credit, or sales recovery? | MQ-009 |
| Is credit policy actually being respected? | MQ-027 |
| How many salesmen are behind a real target, and how many have no target at all? | MQ-017 |
| Who is producing omzet, and is that production concentrated? | MQ-019, EQ-007 |
| Did the team walk the planned route? | MQ-018 |
| Did visits produce orders? | MQ-018, IQ-017 |
| Are check-ins physically credible? | IQ-016 |
| Which reps carry dormant customers or heavy overdue books? | MQ-007 |
| Is the active force large enough for the plan? | MQ-019 |
| How much capital is in the warehouse, and in which categories? | MQ-020 |
| How much of that capital is slow, dead, or never sold? | MQ-021 |
| Will active SKUs run out within 30 days? | MQ-022 |
| Is the company overstocked, understocked, or both? | MQ-022 |
| What is the first action today — buy, delay, transfer, post, or clear? | MQ-010, IQ-023 |
| How much idle capital could still be recovered? | MQ-023 |
| Who takes most of this month’s purchase cash? | MQ-025 |
| Who already holds most of the warehouse capital? | MQ-025, IQ-024 |
| Which principals dominate buying and stock (or sick stock) together? | IQ-024, IQ-025 |
| Are purchased goods posted into inventory? | MQ-028 |
| Is there an aged posting backlog? | IQ-026 |
| Has purchasing gone quiet too late in the month? | MQ-029 |

Company sales forecast, cash-collection forecast, location concentration, and alert-domain questions are included because those KPI families exist in the KPI Inventory even though they sit outside the four encyclopedia entity chapters.

---

# Use in Phase-4

Each management question can become one navigation path:

```text
Executive Question
 ↓
Management Question
 ↓
Dashboard / Data Source
 ↓
KPI
 ↓
Investigative Question
 ↓
Next KPI
 ↓
Decision
```

Do not start Phase-4 from a dashboard list. Start from EQ-001 through EQ-008, then follow the child MQ and IQ rows.
