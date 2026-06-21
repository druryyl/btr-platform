# BTR Portal — Business Domain Knowledge

**Audience:** Product Owner, Business Owner, Analysts, Future Agents  
**Purpose:** Authoritative business-domain reference for BTR Portal — what it is, what business problems it solves, what metrics and analytics concepts exist, and how management uses the system for decision making.

**Related docs:** Foundation — `docs/foundation/PRODUCT.md`, `DOMAIN.md`, `WORKFLOW.md`, `LANDSCAPE.md`. Operational usage — [btr-portal-operational.md](./btr-portal-operational.md). Technical architecture — [btr-portal-architecture.md](./btr-portal-architecture.md).

This document describes **business meaning only**. It does not describe APIs, databases, frameworks, or implementation.

---

## 1. Purpose

BTR Portal is a web-based **read-only management analytics application** for BTR distribution businesses. It gives management and operational staff browser access to sales, receivable, inventory, purchasing, collection, customer, salesman, and location analytics without opening BTR Desktop.

BTR Desktop remains the **source of truth** for all transactional data. The portal observes and analyzes that data; it does not create, edit, or delete business transactions.

BTR Portal has evolved through three product layers:

| Layer | Question answered | Maturity |
| ----- | ----------------- | -------- |
| **Reporting** | What happened? What are the underlying records? | Established (Sales, Piutang, Inventory, Purchasing reports) |
| **Analytics** | How is the business performing? Where is value concentrated? | Established (domain dashboards, KPIs, charts, rankings) |
| **Decision Support** | What requires management attention? Why? What evidence supports action? | Established and expanding (Executive Dashboard, Alert Center, attention signals, investigation workflow) |

---

## 2. Product Positioning

### What BTR Portal Is

- A **management visibility and decision-support** companion to BTR Desktop
- A **read-only** analytics product — not a transactional system
- A **distribution-business lens** on sales execution, receivable management, inventory capital, purchasing intake, field activity, and location concentration
- Designed for **daily management review** — especially morning executive scan — not real-time operational control

### What BTR Portal Is Not

- Not an ERP, accounting system, or CRM
- Not a replacement for BTR Desktop transaction processing
- Not a system for entering Sales Orders, Fakturs, payments, inventory movements, or master data
- Not an export, custom-period filtering, or role-based menu product (deferred capabilities)

### Relationship to BTR Core

| BTR Area | Portal Role |
| -------- | ----------- |
| Sales | Performance monitoring, target achievement, customer and salesman analytics |
| Finance (Piutang) | Receivable exposure, aging, collection effectiveness |
| Inventory | Capital composition and inventory health (slow moving, dead stock) |
| Purchasing | Spend monitoring, posting backlog, supplier dependency |
| Field Sales | Visit execution and route compliance visibility |
| Master Data | Consumes Customer, Item, Supplier, Salesman, Warehouse, Wilayah — does not maintain them |

### Management User Journey (Current)

```text
What is happening?        →  Management Attention Center (Executive Dashboard)
What needs attention?     →  Alert Center
Why is this happening?    →  Domain Dashboard (context)
Show me the evidence      →  Report (tabular detail)
Take action               →  BTR Desktop (operational resolution)
```

---

## 3. Business Areas

BTR Portal covers the following management business areas. Each area has one or more dedicated analytics surfaces.

| Business Area | Management Question | Portal Coverage (Current) |
| ------------- | ------------------- | ------------------------- |
| **Sales** | How much did we invoice? Are we meeting target? Who are top performers? Will we hit target at month-end? | Sales Dashboard, Sales Forecast Dashboard, Executive summary, Sales Report |
| **Finance (Piutang)** | How much is owed? How old is the debt? Who owes the most? | Piutang Dashboard, Piutang Report |
| **Collection** | Is debt being converted to cash? Where is collection risk concentrated? Will we have enough cash by month-end? | Collection Dashboard, Cash Flow Forecast Dashboard |
| **Customer Analytics** | Which customers require attention across sales and receivables? | Customer Analytics Dashboard |
| **Salesman Performance** | Which salespeople require attention and why? | Salesman Performance Dashboard |
| **Field Activity** | Did the field team execute the planned route? Where did they go? | Field Activity Dashboard |
| **Inventory** | How much capital is in stock? How is it distributed? | Inventory Dashboard, Inventory Report |
| **Inventory Risk** | Which stock is not moving? Where is obsolescence risk? | Slow Moving & Dead Stock Dashboard |
| **Purchasing** | How much did we purchase? What is unposted? Who do we depend on? | Purchasing Dashboard, Purchasing Management Dashboard, Purchasing Report |
| **Location** | Are we too dependent on one warehouse or territory? | Branch / Warehouse Performance Dashboard |
| **Executive Monitoring** | What requires management attention today across the whole business? | Management Attention Center |
| **Cross-Domain Attention** | What exceptions exist company-wide right now? | Alert Center |

See `docs/foundation/LANDSCAPE.md` for BTR business area ownership. See `docs/foundation/DOMAIN.md` for core BTR terminology (Faktur, Piutang, Item, etc.).

---

## 4. Core Business Concepts

### 4.1 Current Domain Concepts

Concepts that BTR Portal currently recognizes and uses in dashboards, reports, or decision-support surfaces.

#### Sales Concepts

| Concept | Business Meaning |
| ------- | ---------------- |
| **Invoiced Omzet (Total Omzet)** | Recognized sales revenue from non-void Fakturs in the current calendar month |
| **Sales Target** | Monthly planned sales amount set per salesperson and aggregated at company level |
| **Sales Achievement** | Actual invoiced omzet against target — the primary performance-vs-plan measure |
| **Achievement Percentage** | Target attainment rate; management uses bands: Healthy (≥100%), Warning (80–99%), Critical (<80%) |
| **Sales Ranking** | Ordering salespeople by invoiced omzet to identify leaders |
| **Weekly Sales Trend** | Billing pace within the month — whether invoicing is accelerating or decelerating |
| **Customer Reach** | Count of distinct customers invoiced in the period — breadth of market activity |
| **Faktur Volume** | Count of invoices issued — billing activity intensity |
| **Faktur Kembali** | Signed invoice physically returned to office — document workflow completeness indicator |
| **Principal Achievement** | Salesperson performance broken down by supplier/principal — who is selling which principal's products |

#### Receivable (Piutang) Concepts

| Concept | Business Meaning |
| ------- | ---------------- |
| **Outstanding Receivable (Total Piutang)** | Total amount customers collectively owe on open balances |
| **Open Balance** | Receivable row still owing meaningful amount (business threshold: balance above nominal cutoff) |
| **Aging Bucket** | Classification of receivable by days past due date — Current, 1–30, 31–60, 61–90, >90 days |
| **Overdue Customer** | Customer with any past-due balance — requires collection attention |
| **Receivable Concentration** | Share of total piutang held by top customers — default risk concentration |
| **Jatuh Tempo** | Invoice due date — anchor for aging analysis |
| **Chronic Overdue** | Receivable exposure in the >90-day bucket — severe collection failure signal |

#### Collection Concepts

| Concept | Business Meaning |
| ------- | ---------------- |
| **Collection Effectiveness** | Whether payments are keeping pace with new billing |
| **Cash Collected** | Actual cash received from customers in the current month |
| **Recovery vs Billing** | Ratio of collections to new invoiced omzet — are we recovering debt as fast as we create it? |
| **Payment Mix** | How receivables are settled — cash, giro/check, adjustments |
| **Overdue Exposure** | Total past-due receivable amount (excluding current/not-yet-due bucket) |
| **Overdue Concentration** | Share of overdue amount held by the largest debtor |
| **Legacy Debt** | Open balance on dormant customer — low-recovery receivable on inactive account |
| **Collection Workload** | Distribution of overdue balances across customers, salesmen, and territories |
| **Wilayah Hotspot** | Territory where overdue concentration exceeds management threshold |

#### Customer Analytics Concepts

| Concept | Business Meaning |
| ------- | ---------------- |
| **Active Customer** | Customer invoiced in the current month |
| **Dormant Customer** | Customer with prior purchase history but no Faktur for 90+ days |
| **Customer Concentration** | Dependency on few accounts for revenue (Top Omzet %) or receivables (Top Piutang %) |
| **Plafond Breach** | Customer open balance exceeds approved credit limit |
| **Suspended + Sales** | Customer marked suspended in master data but still invoiced this month — policy violation |
| **Customer Segmentation** | Grouping by Klasifikasi (classification), Wilayah (territory), or activity state |
| **Attention Signal** | Specific reason a customer requires management review (overdue, dormant, credit breach, etc.) |

#### Salesman Performance Concepts

| Concept | Business Meaning |
| ------- | ---------------- |
| **Salesman Attribution** | Which salesperson owns sales omzet (invoice-time salesman) vs receivable exposure (invoicing salesman on open Faktur) |
| **Below Target** | Salesperson with configured target performing in Warning or Critical achievement band |
| **Missing Target Setup** | Active salesperson billing customers but no target configured — planning gap |
| **High Overdue Exposure** | Salesperson whose invoiced book carries disproportionate overdue balance |
| **High Piutang Exposure** | Salesperson owning disproportionate share of company open receivables |
| **Dormant Customer Portfolio** | Inactive customers still attributed to a salesperson's book |
| **Customer Concentration (Salesman)** | Salesperson dependent on few customers for monthly omzet |
| **Active vs Inactive Salesman** | Rep with or without current-month invoicing activity |
| **Achievement Trend** | Monthly achievement history for coaching and performance review |

#### Inventory Concepts

| Concept | Business Meaning |
| ------- | ---------------- |
| **Inventory Value** | Capital tied up in stock on hand, valued at HPP (cost) |
| **Category Composition** | How inventory value distributes across product categories |
| **Supplier Composition** | How inventory value distributes across principals/suppliers |
| **Inventory Concentration** | Over-dependence on one category or supplier for stock value |
| **Nilai Sediaan** | Value of stock for a specific item at a location |
| **In-Transit Stock** | Goods in transit — excluded from portal inventory analytics by business rule |
| **BrgId-first Valuation** | Inventory totals aggregate by product first, then roll up — ensures consistent company totals |

#### Inventory Risk Concepts

| Concept | Business Meaning |
| ------- | ---------------- |
| **Last Faktur Date** | Most recent invoice date for an item — movement signal for aging classification |
| **Active Inventory** | Item sold within recent period (last 89 days) — healthy velocity |
| **Slow Moving Stock** | Item with stock on hand but no sale for 90–179 days |
| **Dead Stock** | Item with stock on hand but no sale for 180+ days |
| **Never Sold** | Item with stock but no sales history ever — demand failure or bad intake |
| **At-Risk Inventory** | Combined slow moving, dead, and never-sold stock requiring management attention |
| **At-Risk Inventory Percentage** | Share of total inventory capital classified as at-risk |
| **Category/Supplier Risk Exposure** | Where obsolescence risk capital concentrates by dimension |

#### Purchasing Concepts

| Concept | Business Meaning |
| ------- | ---------------- |
| **Grand Total Purchase** | Total purchase invoice value in the current month |
| **Purchase Invoice** | Purchasing transaction record (BTR has no separate Purchase Order entity) |
| **Posting Status** | Whether purchased goods have been received into inventory — Posted (SUDAH) vs Pending (BELUM) |
| **Qualified Backlog** | Pending-posting invoices aged beyond staging threshold — actionable management concern |
| **Principal Concentration** | Dependency on few suppliers for monthly purchase spend |
| **Principal Dependency** | Supplier appearing in both purchase rankings and inventory/at-risk rankings |
| **Compound Dependency** | Principal dominating purchase intake AND inventory or at-risk exposure simultaneously |
| **Purchasing Inactivity** | No purchase invoices in month after mid-month threshold — replenishment gap |
| **Principal Inventory No Purchase** | High inventory from principal with zero current-month purchase — legacy stock signal |

#### Location Concepts

| Concept | Business Meaning |
| ------- | ---------------- |
| **Warehouse** | Logical inventory and billing origin — primary logistics location dimension |
| **Wilayah** | Commercial territory for customers and salesmen — geographic sales dimension |
| **Warehouse Concentration** | Over-dependence on one warehouse for inventory, sales, purchasing, or at-risk stock |
| **Wilayah Concentration** | Over-dependence on one territory for billing or overdue exposure |
| **Inactive Warehouse With Stock** | Deactivated warehouse still holding inventory capital |
| **Warehouse No Sales With Inventory** | Site holding stock but generating no current-month billing |
| **Branch** | Informal management term for warehouse-level performance — not a separate BTR entity |

#### Field Activity Concepts

| Concept | Business Meaning |
| ------- | ---------------- |
| **Visit Plan** | Dated schedule of customers a salesman should visit |
| **Planned Visit** | Customer on effective visit plan for a given salesman-day |
| **Actual Visit (Check-in)** | Customer where salesman recorded a field check-in |
| **Missed Visit** | Planned customer with no check-in — route execution gap |
| **Unplanned Visit** | Check-in at customer not on the plan |
| **Effective Call** | Visit that produced at least one sales order same day — productive visit |
| **Visit Execution Rate** | Actual visits ÷ planned visits — route compliance measure |
| **Effective Call Rate** | Effective calls ÷ actual visits — visit productivity measure |
| **GPS Validation** | Distance between check-in coordinates and customer location — visit authenticity signal |
| **Route Template** | Recurring customer sequence defining intended coverage cycle |

#### Platform and Decision-Support Concepts

| Concept | Business Meaning |
| ------- | ---------------- |
| **Management Attention** | Situation requiring management review or intervention |
| **Attention Signal** | Specific qualified reason an entity requires attention |
| **Attention List** | Enumerated exceptions — one row per entity × signal |
| **Attention Indicator** | Visual flag when a dashboard section has qualifying exceptions |
| **Concentration Analysis** | Measuring dependency on top entities (customer, supplier, warehouse, etc.) |
| **Ranking** | Ordered list of top performers or top exposures |
| **Drill Down** | Progressive navigation from summary signal to supporting evidence |
| **Investigation** | Structured workflow to explain why a signal exists and find evidence |
| **Signal** | Actionable management exception surfaced on dashboard or alert feed |
| **Alert** | Cross-domain attention item aggregated for company-wide review |
| **Data Freshness** | Whether analytics reflect recent business state — trust indicator for decisions |

---

## 5. Reporting Concepts

Reports are the portal's **tabular evidence layer**. Management uses reports to validate dashboard KPIs and inspect underlying transactions.

### Report Purpose

| Report | Business Purpose | Period Semantics |
| ------ | ---------------- | ---------------- |
| **Sales Report** | Inspect Faktur-level sales behind sales dashboard metrics | Current calendar month; non-void Fakturs only |
| **Piutang Report** | Inspect open receivable detail behind piutang and collection metrics | All open balances from historical start through today |
| **Inventory Report** | Inspect stock balance behind inventory and inventory-risk metrics | Point-in-time snapshot |
| **Purchasing Report** | Inspect purchase invoice detail behind purchasing metrics | Current calendar month; non-void invoices |

### Reporting Principles (Current)

- **Traceability:** Piutang, Inventory, and Purchasing report footer totals reconcile with matching dashboard KPIs when unfiltered
- **Fixed periods:** Reports use business-default periods; custom date-range filtering is a deferred capability
- **Evidence grain:** Sales = one row per Faktur; Piutang = one row per open Faktur; Inventory = one row per Item × Warehouse; Purchasing = one row per purchase Invoice
- **Search and filter:** Client-side text search helps locate specific customers, salesmen, items, or suppliers within loaded report data
- **Terminal layer:** Reports are the deepest evidence layer in the portal; transaction audit continues in BTR Desktop

### Report vs Dashboard Semantic Alignment

| Domain | Dashboard View | Report Evidence | Alignment Rule |
| ------ | -------------- | --------------- | -------------- |
| Sales | Current month omzet | Current month Fakturs | Same Faktur GrandTotal basis |
| Piutang | All-time open balance | All open Faktur rows | Open-balance semantics must match |
| Inventory | Company inventory value | Item × warehouse balances | Same valuation and exclusion rules |
| Purchasing | Current month spend | Current month invoices | Footer totals must match |

---

## 6. Analytics Concepts

Analytics surfaces help management understand performance, distribution, and risk beyond raw transaction lists.

### Analytics Patterns (Current)

| Pattern | Business Purpose | Examples |
| ------- | ---------------- | -------- |
| **KPI Card** | Headline metric for quick scan | Total Piutang, Achievement %, At-Risk Inventory % |
| **Trend** | Pace or direction over time | Weekly invoiced sales, weekly purchase trend |
| **Comparison** | Actual vs plan or segment vs segment | Target vs Achievement, Active vs Dormant |
| **Distribution Analysis** | How values spread across categories | Aging pie chart, posting status breakdown, inventory aging buckets |
| **Concentration Analysis** | Dependency on top entities | Top Customer %, Top Warehouse Inventory %, Top Principal % |
| **Ranking** | Priority ordering for action | Top 10 Salesman, Top 10 Outstanding Customers, Top 10 Dead Stock |
| **Segmentation** | Breakdown by business dimension | By Wilayah, Klasifikasi, Active/Inactive, Posted/BELUM |
| **Cross-Domain Lens** | Same entity viewed across business areas | Customer omzet + piutang; Salesman achievement + exposure; Principal purchase + inventory |
| **Attention-First Layout** | Exceptions before totals | Attention Cards → Attention List → Rankings → Navigation |
| **Exposure List** | Top-N critical entities grouped by domain | Executive Top 5 Customers, Categories, Suppliers, Principals |

### Period Semantics (Current)

| Analytics Domain | Sales Metrics Period | Receivable Metrics Period | Inventory Period |
| ------------------ | -------------------- | ------------------------- | ---------------- |
| Sales Dashboard | Current calendar month | — | — |
| Sales Forecast Dashboard | Current calendar month (as-of business date) | — | — |
| Piutang Dashboard | — | All-time open balance snapshot | — |
| Customer Analytics | Current month (sales) | All-time open (piutang) | — |
| Salesman Performance | Current month (sales) | All-time open (piutang) | — |
| Collection Dashboard | Current month (recovery) | All-time open (exposure) | — |
| Cash Flow Forecast Dashboard | Current month (as-of business date) | All-time open (risk context) | — |
| Inventory Dashboard | — | — | Point-in-time |
| Inventory Risk | — | — | Point-in-time (movement classification at refresh) |
| Purchasing | Current month | — | Cross-reads inventory snapshots |
| Field Activity | Selected salesman-day | — | — |

### Analytics Evolution

```text
Phase 1 — Domain Reporting     →  Single-area transaction lists
Phase 2 — Domain Dashboards    →  KPIs, charts, Top 10 per area (M8–M15)
Phase 3 — Cross-Domain Lenses  →  Customer, Salesman, Collection, Location (M17–M22)
Phase 4 — Decision Support     →  Executive, Alert Center, Investigation (M16, M23, M24)
Phase 5 — Operational Monitor  →  Field execution visibility (M18.5)
```

---

## 7. KPI Catalog

All monetary values are Indonesian Rupiah (IDR). For each KPI: business meaning, why management uses it, and related business area.

### 7.1 Sales KPIs (Current)

| KPI | Business Meaning | Why Management Uses It | Business Area |
| --- | ---------------- | ---------------------- | ------------- |
| **Total Omzet / Total Achievement** | Invoiced sales revenue this month | Measures actual billing performance | Sales |
| **Total Target** | Sum of monthly sales targets | Defines plan against which achievement is measured | Sales |
| **Achievement %** | Actual ÷ target × 100% | Primary company performance signal; banded for attention | Sales / Executive |
| **Total Faktur** | Invoice count this month | Billing activity volume | Sales |
| **Total Customer** | Distinct customers invoiced | Market reach and breadth | Sales |
| **Weekly Invoiced Sales Trend** | Omzet by calendar week within month | Detects mid-month billing deceleration | Sales |
| **Top 10 Salesman (Omzet)** | Leading reps by invoiced amount | Identifies top performers; coaching context | Sales |

### 7.2 Piutang KPIs (Current)

| KPI | Business Meaning | Why Management Uses It | Business Area |
| --- | ---------------- | ---------------------- | ------------- |
| **Total Piutang** | Sum of all open receivable balances | Scale of working capital at risk | Finance |
| **Total Customer (with balance)** | Customers carrying debt | Breadth of collection exposure | Finance |
| **Overdue Customer** | Customers with past-due balances | Count needing collection action | Finance / Collection |
| **Overdue Piutang** | Sum in overdue aging buckets | Monetary collection urgency | Finance / Collection |
| **Piutang > 90 Hari** | Amount and % in chronic overdue bucket | Bad-debt and escalation risk | Finance / Collection |
| **Top 10 Customer % / Top 20 Customer %** | Concentration of receivables | Default risk if top debtor fails | Finance |
| **Aging Bucket Amounts** | Balance by due-date bracket | Portfolio quality distribution | Finance |

### 7.3 Collection KPIs (Current)

| KPI | Business Meaning | Why Management Uses It | Business Area |
| --- | ---------------- | ---------------------- | ------------- |
| **Overdue Exposure** | Total past-due receivable amount | Collection workload scale | Collection |
| **>90d Exposure** | Chronic overdue amount | Escalation and write-off risk | Collection |
| **Overdue Concentration %** | Top debtor's share of overdue | Prioritize collection effort | Collection |
| **Cash Collected MTD** | Cash payments received this month | Actual recovery pace | Collection |
| **Recovery vs Billing %** | Collections ÷ new invoiced omzet | Are we keeping up with new debt? | Collection |
| **Payment Mix** | Cash / Giro / Adjustment shares | Liquidity and settlement pattern | Collection |
| **Legacy Debt Count** | Dormant customers with open balance | Low-probability receivables | Collection |
| **Top Overdue Customers/Salesmen/Wilayah** | Rankings by overdue only | Collection priority queue | Collection |

### 7.4 Customer Analytics KPIs (Current)

| KPI | Business Meaning | Why Management Uses It | Business Area |
| --- | ---------------- | ---------------------- | ------------- |
| **Active Customer Count** | Customers invoiced this month | Healthy account activity | Customer Analytics |
| **Dormant Customer Count** | 90-day inactive with prior history | Revenue attrition risk | Customer Analytics |
| **Overdue Customer Count** | Customers with past-due balance | Collection attention breadth | Customer Analytics |
| **Plafond Breach Count** | Customers over credit limit | Credit policy enforcement | Customer Analytics |
| **Suspended + Sales Count** | Suspended but still invoiced | Policy violation detection | Customer Analytics |
| **Top Omzet Customer %** | Largest customer's share of monthly revenue | Revenue concentration risk | Customer Analytics |
| **Top Piutang Customer %** | Largest customer's share of receivables | Receivable concentration risk | Customer Analytics |
| **Top 10 Omzet / Top 10 Piutang** | Priority customer rankings | Account management focus | Customer Analytics |

### 7.5 Salesman Performance KPIs (Current)

| KPI | Business Meaning | Why Management Uses It | Business Area |
| --- | ---------------- | ---------------------- | ------------- |
| **Below Target Count** | Reps under plan | Coaching and intervention | Salesman Performance |
| **Missing Target Setup Count** | Active reps without target | Planning completeness | Salesman Performance |
| **High Overdue Exposure Count** | Reps with significant overdue book | Collection accountability by rep | Salesman Performance |
| **High Piutang Exposure Count** | Reps owning large receivable share | Receivable risk by rep | Salesman Performance |
| **Dormant Portfolio Count** | Reps with inactive customers on book | Account maintenance gap | Salesman Performance |
| **Top 10 Omzet / Achievement % / Piutang** | Rep rankings by dimension | Performance and exposure leaders | Salesman Performance |
| **Top Omzet Salesman % / Top Piutang Salesman %** | Team concentration | Dependency on single rep | Salesman Performance |
| **Principal Achievement Table** | Per-supplier target vs actual per rep | Principal-level coaching | Salesman Performance |

### 7.6 Inventory KPIs (Current)

| KPI | Business Meaning | Why Management Uses It | Business Area |
| --- | ---------------- | ---------------------- | ------------- |
| **Total Inventory Value** | Capital in stock on hand | Working capital tied in inventory | Inventory |
| **Total Item** | Distinct products in stock | SKU breadth | Inventory |
| **Top Category / Top Supplier** | Highest-value dimensions | Where capital concentrates | Inventory |
| **Top 10 Categories / Suppliers** | Ranked composition | Planning and purchasing focus | Inventory |

### 7.7 Inventory Risk KPIs (Current)

| KPI | Business Meaning | Why Management Uses It | Business Area |
| --- | ---------------- | ---------------------- | ------------- |
| **Dead Stock Count / Value** | Items idle 180+ days | Write-off and clearance candidates | Inventory Risk |
| **Slow Moving Count / Value** | Items idle 90–179 days | Promotion and replenishment review | Inventory Risk |
| **Never Sold Count / Value** | Stock with no sales history | Bad intake or demand failure | Inventory Risk |
| **At-Risk Inventory %** | At-risk value ÷ total inventory | Share of capital requiring attention | Inventory Risk |
| **Aging Distribution** | Value by movement class | Portfolio health overview | Inventory Risk |
| **Category/Supplier Risk Exposure** | At-risk value by dimension | Where obsolescence clusters | Inventory Risk |
| **Top 10 Dead / Slow Moving** | Highest-impact items per class | Item-level action list | Inventory Risk |

### 7.8 Purchasing KPIs (Current)

| KPI | Business Meaning | Why Management Uses It | Business Area |
| --- | ---------------- | ---------------------- | ------------- |
| **Grand Total Purchase** | Monthly purchase spend | Cash outflow monitoring | Purchasing |
| **Total Invoice** | Purchase invoice count | Purchasing activity volume | Purchasing |
| **Posted %** | Share of value already posted to stock | Posting completion health | Purchasing |
| **Pending / Qualified Backlog** | Unposted invoices (all vs aged actionable) | Inventory intake delay | Purchasing |
| **Top 10 Principal** | Leading suppliers by spend | Supplier dependency | Purchasing |
| **Top 1 / Top 3 Principal %** | Spend concentration | Supply chain risk | Purchasing |
| **Compound Dependency Count** | Principals in purchase AND inventory/risk top ranks | Multi-dimensional supplier risk | Purchasing |
| **Purchasing Inactivity** | Zero purchases mid-month | Replenishment gap signal | Purchasing |

### 7.9 Location KPIs (Current)

| KPI | Business Meaning | Why Management Uses It | Business Area |
| --- | ---------------- | ---------------------- | ------------- |
| **Top Warehouse Inventory %** | Largest warehouse's inventory share | Site-level capital concentration | Location |
| **Top 3 Warehouse Inventory %** | Top three warehouses' combined share | Multi-site dependency | Location |
| **Top Warehouse At-Risk %** | Largest warehouse's at-risk share | Obsolescence localized to site | Location |
| **Top Warehouse / Wilayah Sales %** | Dominant billing site or territory | Revenue dependency by location | Location |
| **Inactive Warehouse With Stock** | Deactivated sites still holding stock | Legacy capital trap | Location |
| **Top Wilayah by Sales** | Territory billing ranking | Commercial geography performance | Location |

### 7.10 Field Activity KPIs (Current)

| KPI | Business Meaning | Why Management Uses It | Business Area |
| --- | ---------------- | ---------------------- | ------------- |
| **Planned Visits** | Customers on effective visit plan | Expected coverage denominator | Field Activity |
| **Actual Visits** | Distinct customers checked in | Route execution numerator | Field Activity |
| **Missed Visits** | Planned minus actual | Coverage gaps | Field Activity |
| **Unplanned Visits** | Check-ins off-plan | Ad-hoc selling or plan deviation | Field Activity |
| **Effective Calls** | Visits producing orders | Productive visit count | Field Activity |
| **Visit Execution %** | Actual ÷ planned | Route compliance | Field Activity |
| **Effective Call Rate** | Effective ÷ actual | Visit productivity | Field Activity |

### 7.11 Executive and Platform KPIs (Current)

| KPI | Business Meaning | Why Management Uses It | Business Area |
| --- | ---------------- | ---------------------- | ------------- |
| **Domain Attention Cards** | Per-area exception summary | Morning executive scan | Executive |
| **Critical Exposure Top 5** | Top customers, categories, suppliers, principals | Immediate priority entities | Executive |
| **Snapshot Freshness** | Analytics recency across domains | Decision confidence | Platform |
| **Alert Category Counts** | Exceptions by business category | Company-wide attention volume | Alert Center |

---

## 8. Dashboard Concepts

Each dashboard answers a specific management question. Dashboards are **read-only analytics surfaces** — not operational workspaces.

### 8.1 Current Dashboards

| Dashboard | Management Question | Primary Audience |
| --------- | ------------------- | ---------------- |
| **Management Attention Center** | What requires management attention today? | Owner, GM, daily executive review |
| **Alert Center** | What needs attention right now across the entire business? | Management seeking exception workspace |
| **Sales Dashboard** | How is company sales performance vs target? | Sales management |
| **Sales Forecast Dashboard** | Where will invoiced sales likely finish at month-end? | Sales and operations leadership |
| **Piutang Dashboard** | How much is owed and how old is the debt? | Finance management |
| **Collection Dashboard** | Are receivables being converted to cash? | Collection management |
| **Cash Flow Forecast Dashboard** | How much cash will we likely receive by month-end? | Finance and operations leadership |
| **Customer Analytics** | Which customers require attention? | Sales and finance management |
| **Salesman Performance** | Which salespeople require attention and why? | Sales management |
| **Field Activity** | Did the field team execute the planned route? | Field supervisors, area managers |
| **Inventory Dashboard** | How is inventory capital distributed? | Inventory and purchasing management |
| **Slow Moving & Dead Stock** | Which inventory requires attention and why? | Inventory management |
| **Purchasing Dashboard** | What are monthly purchasing statistics? | Purchasing management |
| **Purchasing Management** | Which suppliers and purchasing activities require attention? | Purchasing and executive management |
| **Branch / Warehouse Performance** | Are we too dependent on one warehouse or territory? | Operations and executive management |

### 8.2 Dashboard Relationships

| Relationship | Business Meaning |
| ------------ | ---------------- |
| **Executive supplements domain dashboards** | Landing page promotes exceptions; detail dashboards retain full analytics |
| **Cross-domain dashboards supplement entity dashboards** | Customer and Salesman lenses combine sales + piutang; they do not replace domain views |
| **Piutang vs Collection** | Piutang = exposure (how much owed); Collection = recovery (is cash coming in) |
| **Inventory vs Inventory Risk** | Inventory = composition (where capital sits); Inventory Risk = health (what is not moving) |
| **Purchasing V1 vs Purchasing Management** | V1 = monthly statistics; Management = attention signals and cross-domain supplier risk |
| **Sales vs Sales Forecast** | Sales = current achievement; Forecast = projected month-end at current pace |
| **Collection vs Cash Flow Forecast** | Collection = recovery performance today; Cash Flow Forecast = projected month-end liquidity and required pace |
| **Salesman Performance vs Field Activity** | M18 = outcome lens (omzet, achievement, exposure); Field Activity = execution lens (visits, routes, GPS) |
| **Location vs Collection (Wilayah)** | Collection owns wilayah overdue hotspots; Location owns wilayah sales concentration |

### 8.3 Attention Signal Catalog (Current)

Signals materialized across portal dashboards and consumed by Alert Center.

**Customer signals:** Overdue · Dormant · Plafond Breach · Suspended + Sales

**Salesman signals:** Below Target · Missing Target Setup · High Overdue Exposure · High Piutang Exposure · Customer Concentration · Dormant Customer Portfolio

**Inventory risk signals:** Dead Stock · Slow Moving · Never Sold

**Collection signals:** Chronic Overdue · Plafond Breach Overdue · Legacy Debt · Overdue · Low Recovery vs Billing · Wilayah Hotspot · High Overdue Workload

**Purchasing signals:** Qualified Backlog · Principal Spend Concentration · Principal Inventory Concentration · Principal At-Risk Exposure · Compound Dependency · Purchasing Inactivity · Principal Inventory No Purchase · Unknown Principal

**Location signals:** Warehouse Inactive With Stock · Warehouse No Sales With Inventory · Warehouse concentration (inventory, at-risk, sales, purchasing top-10 rank)

**Executive / platform signals:** Sales Achievement band · Snapshot stale/degraded · Domain unavailable

---

## 9. Decision Support Concepts

BTR Portal has evolved from reporting into a **management decision-support platform**. These concepts define how management moves from awareness to action.

### 9.1 Current Decision Support Capabilities

| Concept | Business Meaning | Management Usage |
| ------- | ---------------- | ---------------- |
| **Management Attention Center** | Executive landing synthesizing cross-domain exceptions | Daily morning review — "What is happening?" |
| **Alert Center** | Company-wide exception aggregator across all domains | Prioritized exception workspace — "What needs attention?" |
| **Attention-First Design** | Exceptions surfaced before informational totals | Focus management time on intervention, not data browsing |
| **Attention Indicator** | Visual flag when a section has qualifying signals | Quick scan for areas needing drill-down |
| **Investigation Workflow** | Standard path from signal to evidence | Answers: Why am I seeing this? Show me evidence. What next? |
| **Investigation Depth Model** | Signal → Dashboard context → Report evidence → Desktop action | Progressive disclosure without overwhelming executives |
| **Drill Down** | Navigate from KPI, alert, or ranking to supporting detail | Validate signals and prepare operational response |
| **Report-First Navigation** | Primary investigation path leads to tabular evidence | Management validates numbers before acting |
| **Traceability** | Dashboard KPIs reconcile to report totals | Builds trust in analytics for decision making |
| **Control Tower** | Single place to see cross-domain business health | Executive Dashboard + Alert Center combination |
| **Business Monitoring** | Ongoing observation of exceptions, concentrations, and trends | Replaces ad-hoc Desktop report pulling for management |

### 9.2 Management Review Workflow (Current)

```text
1. SCAN      — Open Management Attention Center or Alert Center
2. PRIORITIZE — Identify domain or entity requiring attention
3. CONTEXTUALIZE — Open owning domain dashboard for fuller picture
4. INVESTIGATE — Follow drill-down to report evidence
5. VALIDATE  — Reconcile KPI to underlying transactions
6. ACT       — Complete operational work in BTR Desktop
```

### 9.3 Alert Center Business Rules (Current)

| Rule | Business Meaning |
| ---- | ---------------- |
| **Consumer only** | Alert Center aggregates signals defined by domain dashboards — it does not invent new business rules |
| **Category organization** | Alerts grouped by Sales, Customer, Collection, Inventory, Purchasing, Location, Platform |
| **Volume cap** | Top entity alerts per category — prevents overwhelming management |
| **Deduplication** | Same situation not shown twice under different labels (e.g., collection overdue supersedes generic customer overdue where applicable) |
| **Inventory risk summary** | Item-level inventory risk shown as summary in alerts, not full SKU dump |
| **No acknowledgment** | Read-only feed — management acts in Desktop, not in portal |

### 9.4 Investigation Principles (Current)

| Principle | Business Meaning |
| --------- | ---------------- |
| **Explain the signal** | User always knows why an item appeared (signal label and source) |
| **Evidence terminus** | Reports are the deepest portal evidence; Desktop is operational audit |
| **Semantic alignment** | Piutang investigation uses open-balance semantics consistently |
| **Entity-appropriate path** | Customer/salesman/item alerts route to the report that best proves the signal |
| **Multi-hop allowed** | Cross-domain signals may require visiting more than one dashboard before evidence |

---

## 10. Operational Monitoring Concepts

Operational monitoring extends analytics into **field execution and route compliance** — observing whether planned business activities actually occurred.

### 10.1 Current Operational Monitoring

| Concept | Business Meaning | Management Usage |
| ------- | ---------------- | ---------------- |
| **Sales Visit** | Salesperson physical presence at customer location | Verify field coverage |
| **Visit Plan** | Supervisor-approved dated customer schedule | Define expected daily coverage |
| **Visit Route** | Ordered customer sequence for a route day | Understand intended travel path |
| **Route Monitoring** | Comparing planned vs actual customer visits | Detect missed accounts and plan deviations |
| **Route Compliance** | Visit Execution % — actual visits vs planned | Measure discipline against schedule |
| **Collection Visit** | Field visit for payment collection (BTR workflow) | Part of receivable collection process — not yet fully monitored in portal |
| **Customer Coverage** | Which planned customers were actually visited | Territory execution accountability |
| **Territory Coverage** | Wilayah-level activity and concentration | Regional management oversight |
| **GPS Check-in Validation** | Proximity of visit coordinates to customer location | Visit authenticity and data quality |
| **Visit Replay** | Timeline reconstruction of a salesman's day | Supervisory review and demonstration |
| **Effective Call Monitoring** | Whether visits produced orders | Distinguish activity from productivity |
| **Coordinate Coverage** | Customers with usable map coordinates | Data readiness for route visualization |

### 10.2 Field Activity vs Salesman Performance

| Lens | Question | Metrics |
| ---- | -------- | ------- |
| **Salesman Performance (outcome)** | How did the rep perform on sales and receivables? | Achievement %, omzet, piutang exposure, dormant portfolio |
| **Field Activity (execution)** | Did the rep execute the planned route? | Planned, actual, missed, effective call, visit execution % |

Management uses **both lenses together**: a rep may show strong omzet (outcome) but poor route compliance (execution), or vice versa.

### 10.3 Operational Monitoring Design Intent

Field Activity monitoring serves dual purposes:

1. **Operational visibility** — supervisors verify daily route execution
2. **Demonstration value** — visually showcase BTR's field sales differentiation (route planning, GPS, map replay) to owners and prospects

Visibility is intended to drive an operational flywheel: **Dashboard Visibility → Management Attention → Operational Enforcement → Better Data Quality**.

---

## 11. Ubiquitous Language

Terms used consistently across BTR Portal analytics. Core BTR terms are defined in `docs/foundation/DOMAIN.md`; portal-specific extensions are listed here.

### 11.1 Portal Product Terms

| Term | Meaning |
| ---- | ------- |
| **Portal** | BTR Portal web application |
| **Dashboard** | Analytics page with KPIs, charts, and management signals |
| **Report** | Tabular transaction-level evidence view |
| **Snapshot** | Point-in-time analytics state refreshed on schedule |
| **Attention Signal** | Qualified management exception attached to an entity |
| **Attention List** | Enumerated attention signals (entity × reason) |
| **Alert** | Cross-domain attention item in Alert Center |
| **Investigation** | Structured drill-down from signal to evidence |
| **Drill Down** | Navigation deeper into supporting detail |
| **Ranking** | Top-N ordered list by business metric |
| **Concentration** | Percentage share held by top entity |
| **Segmentation** | Breakdown of population by business dimension |
| **Recovery** | Cash and payments collected against receivables |
| **Exposure** | Outstanding balance or risk amount |
| **At-Risk** | Inventory classified as slow moving, dead, or never sold |
| **Qualified Backlog** | Purchase invoices pending stock posting beyond staging period |
| **Principal** | Supplier providing goods — equivalent to Supplier in BTR |
| **Wilayah** | Sales territory — geographic commercial dimension |
| **Management Attention Center** | Executive dashboard landing page |
| **Control Tower** | Combined executive and alert surfaces for company-wide monitoring |

### 11.2 Achievement Bands

| Band | Achievement % | Management Interpretation |
| ---- | ------------- | ------------------------- |
| **Healthy** | ≥ 100% | On or above plan |
| **Warning** | 80–99% | Below plan — needs attention |
| **Critical** | < 80% | Significantly below plan — intervention required |
| **Unknown** | No target configured | Planning gap — not a performance judgment |

### 11.3 Inventory Movement Classes

| Class | Idle Period Since Last Sale | Management Interpretation |
| ----- | --------------------------- | ------------------------- |
| **Active** | 0–89 days | Healthy velocity |
| **Slow Moving** | 90–179 days | Losing velocity — review replenishment and promotion |
| **Dead Stock** | 180+ days | No recent demand — clearance or write-off candidate |
| **Never Sold** | No sales history | Demand failure or mistaken intake |

### 11.4 Receivable Aging Buckets

| Bucket | Rule | Management Interpretation |
| ------ | ---- | ------------------------- |
| **Current** | Not yet due | Normal receivable |
| **1–30 Days** | 1–30 days past due | Early collection follow-up |
| **31–60 Days** | 31–60 days past due | Escalating collection urgency |
| **61–90 Days** | 61–90 days past due | Serious collection concern |
| **> 90 Days** | More than 90 days past due | Chronic overdue — high bad-debt risk |

### 11.5 Field Activity Terms

| Term | Meaning |
| ---- | ------- |
| **Planned Visit** | Customer on effective visit plan for salesman-day |
| **Actual Visit** | Customer with recorded check-in |
| **Missed Visit** | Planned customer without check-in |
| **Unplanned Visit** | Check-in at customer not on plan |
| **Effective Call** | Visit producing at least one order same day |
| **Visit Execution %** | Actual visits ÷ planned visits |
| **Effective Call Rate** | Effective calls ÷ actual visits |
| **GPS Valid** | Check-in within acceptable distance of customer coordinates |

### 11.6 Attribution Rules (Current)

| Metric | Attribution Rule |
| ------ | ---------------- |
| Sales omzet | Salesperson on Faktur at invoice time |
| Piutang exposure | Invoicing salesperson on open Faktur |
| Dormant customer | Last invoicing salesperson (most recent Faktur) |
| Collection payments | Company-level and salesman-level; no separate Collector entity |
| Inventory movement class | Last Faktur date per item (sales outflow signal) |

---

## 12. Future Accepted Domain Concepts

Concepts **approved for BTR Portal roadmap direction** but **not yet implemented** as management capabilities. Future agents must not treat these as current portal features.

### 12.1 Executive Dashboard Enhancements (Accepted, Partial Delivery)

| Concept | Business Meaning | Status |
| ------- | ---------------- | ------ |
| **Inventory Risk on Executive** | Promote Dead Stock Value, At-Risk %, and inventory risk attention to landing page | Accepted — Phase 2 |
| **Purchasing Management on Executive** | Promote Compound Dependency, Qualified Backlog Value, Top 3 Principal % | Accepted — Phase 2 |
| **Collection Recovery on Executive** | Promote Cash Collected MTD, Recovery vs Billing %, Overdue Concentration % | Accepted — post-M20 promotion |

### 12.2 Sales Force Effectiveness (M25 — Accepted Direction)

| Concept | Business Meaning |
| ------- | ---------------- |
| **Sales Force Effectiveness** | Composite view of field productivity beyond outcome KPIs |
| **Route Compliance Score** | Unified scoring of planned vs actual coverage over time |
| **Visit Productivity Score** | Combined visit execution, effective call, and GPS quality |
| **Coaching Signal** | Rep-level guidance based on activity vs outcome gap |
| **Team Trend** | Multi-day or multi-week field execution trends |
| **Bottom Performer Ranking** | Underperformers surfaced for intervention (not just Top 10) |
| **Pipeline Omzet** | Outstanding order value not yet invoiced — excluded from current portal |

**Primary question:** *Which salesman is active but not productive?*

### 12.3 Advanced Analytics (Accepted, Deferred)

| Concept | Business Meaning |
| ------- | ---------------- |
| **DSO (Days Sales Outstanding)** | Average collection period — explicitly excluded from M20; future consideration |
| **Aging Deterioration Trend** | Whether receivable portfolio quality is worsening over time — requires historical snapshots |
| **ABC Classification** | Inventory prioritization by value × velocity |
| **Inventory Turnover** | How fast stock converts to sales |
| **Warehouse Movement KPIs** | Per-warehouse stock movement and turnover |
| **Margin Analysis** | Profitability analytics on sales |
| **Retur Analytics** | Customer and item return patterns in portal |
| **Declining Purchase Trend** | Month-over-month customer purchase decline detection |
| **Faktur Kembali Aggregate** | Company-level unsigned invoice backlog KPI |
| **Sales Omzet Data Quality Signal** | Reconciliation health between omzet records and Faktur |
| **Cross-Domain Crisis Correlation** | Automated detection of correlated multi-domain failures |

### 12.4 Collection CRM (Accepted as Out of Scope for Now)

BTR does not record structured collection activities (follow-up calls, promise-to-pay, visit outcomes tied to payment). Future CRM-style collection tracking is **not accepted** into current portal domain — collection analytics remain derived from piutang and payment data.

| Concept | Status |
| ------- | ------ |
| Collection follow-up log | Not accepted — no BTR data source |
| Promise-to-pay tracking | Not accepted |
| Visit-to-payment conversion | Deferred to M25 field effectiveness |
| Tagihan pipeline KPIs | Administrative only — not headline management metrics |

### 12.5 Platform Capabilities (Accepted, Deferred)

| Concept | Business Meaning |
| ------- | ---------------- |
| **Custom Date Range** | User-selected analysis periods on reports and dashboards |
| **Advanced Search** | Server-side search and filtering |
| **Export** | Excel/PDF export of reports and dashboards |
| **Role-Based Visibility** | Menu and data access by user role |
| **Clickable Charts** | Chart elements as drill-down entry points |
| **Desktop Deep Links** | Direct navigation from portal signal to Desktop screen |
| **Collection Report / Pelunasan Report** | Payment-level evidence report in portal |
| **Customer Report / Salesman Report** | Dedicated entity reports beyond existing four |
| **Sales Report Footer Totals** | Summary totals on sales report |
| **Server-Side Pagination** | Large dataset handling in reports |
| **Alert Acknowledgment** | Track which alerts management has reviewed |
| **Investigation Telemetry** | Usage analytics on investigation paths |

### 12.6 Field Activity Enhancements (Accepted, Future Releases)

| Concept | Business Meaning |
| ------- | ---------------- |
| **Team-Level Map** | Multi-salesman field activity view |
| **7/30-Day Visit Trends** | Historical route compliance trends |
| **Alert Center Field Signals** | Visit compliance exceptions in Alert Center |
| **Visit KPI on Salesman Dashboard** | Summary visit scores on M18 dashboard (from M25) |
| **Continuous GPS Tracking** | Real-time location trail — not available in BTR data |

### 12.7 Location Analytics Enhancements (Accepted, V2)

| Concept | Business Meaning |
| ------- | ---------------- |
| **Warehouse Movement KPIs** | Per-warehouse stock activity from movement ledger |
| **Warehouse Productivity Ratios** | Utilization and throughput metrics |
| **Warehouse Sales Targets** | Location-level plan vs achievement |
| **Cross-Wilayah Selling Analysis** | Territory policy compliance |
| **SKU-Level Warehouse Imbalance** | Same item overstocked at one site, understocked at another |
| **Depo Analytics** | Physical storage location performance — Depo excluded from M22 V1 |

### 12.8 Roadmap Milestone Map (Accepted Direction)

```text
M16  Executive Dashboard (Management Attention Center)     ✓ Current
M17  Customer Analytics                                   ✓ Current
M18  Salesman Performance                                  ✓ Current
M18.5 Field Activity (Route & Visit Monitoring)           ✓ Current
M19  Slow Moving & Dead Stock                              ✓ Current
M20  Collection Dashboard                                  ✓ Current
M21  Purchasing Management                                 ✓ Current
M22  Branch / Warehouse Performance                        ✓ Current
M23  Alert Center                                          ✓ Current
M24  Dashboard Drill-Down & Investigation                  ✓ Current
M25  Sales Force Effectiveness                             → Future Accepted
M26  Sales Forecast Dashboard                              ✓ Current
M27  Cash Flow Forecast Dashboard                          ✓ Current
M28  Inventory Forecast Dashboard                          ✓ Current
M16 Phase 2 / M19 Phase 2 / M21 Phase 2                 → Future Accepted (Executive promotions)
Filtering Phase (date range, search)                    → Future Accepted
```

---

## Document Maintenance

When portal capabilities change:

1. Move concepts from Section 12 (Future) to Sections 4–11 (Current) when implemented
2. Add new business concepts discovered during milestone analysis
3. Keep this document free of implementation detail — update `btr-portal-architecture.md` and `btr-portal-operational.md` for technical and usage specifics
4. Align terminology with `docs/foundation/DOMAIN.md` when core BTR terms evolve

**Success criterion:** A new agent reading `PRODUCT.md`, `DOMAIN.md`, `WORKFLOW.md`, `LANDSCAPE.md`, and this document can understand what BTR Portal is, what business problems it solves, what metrics exist, what dashboards mean, what signals and alerts mean, and how management uses the system — without reading source code or implementation artifacts.
