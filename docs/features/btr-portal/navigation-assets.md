# NAVIGATION ASSET REGISTRY

**Audience:** Business analysts, Product Owner, Phase-4B Navigation Playbook authors  
**Purpose:** Source of truth for what navigation assets actually exist in BTR Portal today.  
**Scope:** Discover only. This is not a playbook, not a question-to-screen map, and not a redesign.  
**Source of truth:** Portal as implemented (sidebar menu, pages, visible widgets, drill-downs). Where older documents use different names, this registry uses the names the user sees.

**Related:** [btr-portal-domain.md](./btr-portal-domain.md) · [btr-portal-kpi-catalog.md](./btr-portal-kpi-catalog.md) · [kpi-inventory.md](./kpi-inventory.md) · [business-question-catalog-v3.md](./business-question-catalog-v3.md)

**How to read names**

| Layer | What it is | Example |
| ----- | ---------- | ------- |
| Menu group | Sidebar section header | Customers |
| Menu item | Clickable sidebar entry | Customer Risk Forecast |
| Page | Screen title after opening | Customer Risk Forecast Dashboard |
| Widget | Named block on the page | Overdue Exposure, Top 10 Salesman |
| Drill-down | Where a click takes the user | Customer Performance Profile, Piutang Report |

Sidebar labels and page titles are not always the same. Both are recorded.

**Not included:** Login. Presentation Mode is a header overlay, not a menu page.

---

# SECTION 1
# MENU REGISTRY

These are the eight sidebar groups. Each group contains the clickable items below.

---

### MENU-EXECUTIVE

```text
Menu ID:     MENU-EXECUTIVE
Menu Name:   Executive
Purpose:     Company-wide attention scan, exception review, and entity investigation entry.
Items:
  EX01  Executive              → Management Attention Center
  EX02  Alert Center           → Alert Center
  EX03  Entity Analytics       → Entity Analytics
```

---

### MENU-SALES

```text
Menu ID:     MENU-SALES
Menu Name:   Sales
Purpose:     Current-month invoiced sales performance, month-end forecast, and invoice evidence.
Items:
  SA01  Sales                  → Sales Dashboard
  SA02  Sales Forecast         → Sales Forecast Dashboard
  SA03  Sales Report           → Sales Report
```

---

### MENU-CUSTOMERS

```text
Menu ID:     MENU-CUSTOMERS
Menu Name:   Customers
Purpose:     Customer health, forward risk, collection action, portfolio management, and customer evidence.
Items:
  CU01  Customers              → Customer Analytics
  CU02  Customer Risk Forecast → Customer Risk Forecast Dashboard
  CU03  Collection Optimization→ Collection Optimization Dashboard
  CU04  Customer Portfolio     → Customer Portfolio Dashboard
  CU05  Customer Report        → Customer Report
```

---

### MENU-FINANCE

```text
Menu ID:     MENU-FINANCE
Menu Name:   Finance
Purpose:     Receivable quality, collection recovery, cash-collection forecast, and receivable evidence.
Items:
  FI01  Piutang                → Piutang Dashboard
  FI02  Collection             → Collection Dashboard
  FI03  Cash Flow Forecast     → Cash Flow Forecast Dashboard
  FI04  Piutang Report         → Piutang Report
```

---

### MENU-SALES-FORCE

```text
Menu ID:     MENU-SALES-FORCE
Menu Name:   Sales Force
Purpose:     Salesperson performance, team field execution, and individual route replay.
Items:
  SF01  Salesmen               → Salesman Performance
  SF02  Sales Force Overview   → Sales Force Overview
  SF03  Salesman Field Activity→ Salesman Field Activity
```

---

### MENU-INVENTORY

```text
Menu ID:     MENU-INVENTORY
Menu Name:   Inventory
Purpose:     Stock value, aging risk, 30-day forecast, optimization actions, and stock evidence.
Items:
  IN01  Inventory              → Inventory Dashboard
  IN02  Inventory Risk         → Slow Moving & Dead Stock Dashboard
  IN03  Inventory Forecast     → Inventory Forecast Dashboard
  IN04  Inventory Optimization → Inventory Optimization Dashboard
  IN05  Inventory Report       → Inventory Report
```

---

### MENU-PURCHASING

```text
Menu ID:     MENU-PURCHASING
Menu Name:   Purchasing
Purpose:     Purchase attention, principal dependency, posting backlog, and purchase-invoice evidence.
Items:
  PU01  Purchasing             → Purchasing Management Dashboard
  PU02  Purchasing Report      → Purchasing Report
```

---

### MENU-OPERATIONS

```text
Menu ID:     MENU-OPERATIONS
Menu Name:   Operations
Purpose:     Warehouse and wilayah concentration of inventory, sales, and purchasing.
Items:
  OP01  Locations              → Branch / Warehouse Performance Dashboard
```

---

# SECTION 2
# PAGE REGISTRY

Pages opened from the sidebar, plus pages reached only by drill-down.

---

## 2.1 Pages linked from the sidebar

### PAGE-EX01

```text
Page ID:         PAGE-EX01
Page Name:       Management Attention Center
Parent Menu:     Executive
Sidebar Label:   Executive
Purpose:         Answer “what requires management attention today?” across sales, piutang, purchasing, inventory, and customer portfolio.
Primary Entity:  Company
Business Intent: Morning executive scan before opening a domain dashboard or Alert Center.
```

### PAGE-EX02

```text
Page ID:         PAGE-EX02
Page Name:       Alert Center
Parent Menu:     Executive
Sidebar Label:   Alert Center
Purpose:         Show exceptions that require attention right now, with paths to investigate or open a domain dashboard.
Primary Entity:  Alert / Exception
Business Intent: Exception triage across the business.
```

### PAGE-EX03

```text
Page ID:         PAGE-EX03
Page Name:       Entity Analytics
Parent Menu:     Executive
Sidebar Label:   Entity Analytics
Purpose:         Choose Customer, Salesman, Supplier, or Item and open Investigation Workspace, Performance Profile, or Compare.
Primary Entity:  Customer / Salesman / Supplier / Item
Business Intent: Cross-domain entity investigation, not a domain dashboard.
```

### PAGE-SA01

```text
Page ID:         PAGE-SA01
Page Name:       Sales Dashboard
Parent Menu:     Sales
Sidebar Label:   Sales
Purpose:         Current-month invoiced sales (Faktur) versus target, weekly pace, and top salesmen.
Primary Entity:  Salesman
Business Intent: Is billing on plan this month, and who is contributing?
```

### PAGE-SA02

```text
Page ID:         PAGE-SA02
Page Name:       Sales Forecast Dashboard
Parent Menu:     Sales
Sidebar Label:   Sales Forecast
Purpose:         Project month-end invoiced sales, daily pace required, and forecast risk versus target.
Primary Entity:  Company (sales)
Business Intent: Will we hit sales target at month-end?
```

### PAGE-SA03

```text
Page ID:         PAGE-SA03
Page Name:       Sales Report
Parent Menu:     Sales
Sidebar Label:   Sales Report
Purpose:         Faktur jual rows for a selected period — evidence behind sales analytics.
Primary Entity:  Faktur
Business Intent: Show the invoices behind a sales number.
```

### PAGE-CU01

```text
Page ID:         PAGE-CU01
Page Name:       Customer Analytics
Parent Menu:     Customers
Sidebar Label:   Customers
Purpose:         Which customers require attention across collection, concentration, activity, inactivity, and credit.
Primary Entity:  Customer
Business Intent: Find customers that need management action.
```

### PAGE-CU02

```text
Page ID:         PAGE-CU02
Page Name:       Customer Risk Forecast Dashboard
Parent Menu:     Customers
Sidebar Label:   Customer Risk Forecast
Purpose:         Forward-looking customer risk over a forecast horizon, by category and signal family.
Primary Entity:  Customer
Business Intent: Who is likely to become a collection or credit problem?
```

### PAGE-CU03

```text
Page ID:         PAGE-CU03
Page Name:       Collection Optimization Dashboard
Parent Menu:     Customers
Sidebar Label:   Collection Optimization
Purpose:         Today’s prioritized collection actions from risk forecast and receivables.
Primary Entity:  Customer (collection action)
Business Intent: What collection work should be done today?
```

### PAGE-CU04

```text
Page ID:         PAGE-CU04
Page Name:       Customer Portfolio Dashboard
Parent Menu:     Customers
Sidebar Label:   Customer Portfolio
Purpose:         Portfolio health, lifecycle/tier mix, and recommended portfolio actions.
Primary Entity:  Customer
Business Intent: How healthy is the customer book, and who needs a portfolio action?
```

### PAGE-CU05

```text
Page ID:         PAGE-CU05
Page Name:       Customer Report
Parent Menu:     Customers
Sidebar Label:   Customer Report
Purpose:         Tabular customer portfolio rows for investigation and search.
Primary Entity:  Customer
Business Intent: Evidence and row-level customer facts. Customer Value is omzet proxy, not profitability.
```

### PAGE-FI01

```text
Page ID:         PAGE-FI01
Page Name:       Piutang Dashboard
Parent Menu:     Finance
Sidebar Label:   Piutang
Purpose:         Portfolio quality of all open receivables — size, overdue, aging, concentration.
Primary Entity:  Customer (receivable)
Business Intent: How large and how old is the receivable book? Recovery metrics live on Collection Dashboard.
```

### PAGE-FI02

```text
Page ID:         PAGE-FI02
Page Name:       Collection Dashboard
Parent Menu:     Finance
Sidebar Label:   Collection
Purpose:         Current-month cash recovery plus open overdue exposure by customer, salesman, and wilayah.
Primary Entity:  Customer (overdue)
Business Intent: Is debt converting to cash, and where is overdue concentrated?
```

### PAGE-FI03

```text
Page ID:         PAGE-FI03
Page Name:       Cash Flow Forecast Dashboard
Parent Menu:     Finance
Sidebar Label:   Cash Flow Forecast
Purpose:         Project month-end cash collection and recovery pace versus billing.
Primary Entity:  Company (cash collection)
Business Intent: Will we collect enough cash by month-end?
```

### PAGE-FI04

```text
Page ID:         PAGE-FI04
Page Name:       Piutang Report
Parent Menu:     Finance
Sidebar Label:   Piutang Report
Purpose:         Open receivable rows (outstanding balance) for a selected period / due-date basis.
Primary Entity:  Faktur / Customer receivable
Business Intent: Evidence behind piutang and collection numbers.
```

### PAGE-SF01

```text
Page ID:         PAGE-SF01
Page Name:       Salesman Performance
Parent Menu:     Sales Force
Sidebar Label:   Salesmen
Purpose:         Which salesman requires attention on performance, collection exposure, and portfolio.
Primary Entity:  Salesman
Business Intent: Find underperforming or high-exposure salespeople.
```

### PAGE-SF02

```text
Page ID:         PAGE-SF02
Page Name:       Sales Force Overview
Parent Menu:     Sales Force
Sidebar Label:   Sales Force Overview
Purpose:         Compare field visit execution across the sales organization for a chosen visit date.
Primary Entity:  Salesman (field activity)
Business Intent: Did the team execute the planned route, and who is off plan?
```

### PAGE-SF03

```text
Page ID:         PAGE-SF03
Page Name:       Salesman Field Activity
Parent Menu:     Sales Force
Sidebar Label:   Salesman Field Activity
Purpose:         One salesman’s route execution, GPS check-in, missed visits, and visit replay for a date.
Primary Entity:  Salesman / Visit
Business Intent: Reconstruct a day’s field work for one salesperson.
```

### PAGE-IN01

```text
Page ID:         PAGE-IN01
Page Name:       Inventory Dashboard
Parent Menu:     Inventory
Sidebar Label:   Inventory
Purpose:         Point-in-time stock value by category and supplier (excludes in-transit and zero quantity).
Primary Entity:  Item / Category / Supplier
Business Intent: How much capital is in stock, and where is it concentrated?
```

### PAGE-IN02

```text
Page ID:         PAGE-IN02
Page Name:       Slow Moving & Dead Stock Dashboard
Parent Menu:     Inventory
Sidebar Label:   Inventory Risk
Purpose:         Inventory health — dead stock, slow moving, aging, and at-risk items.
Primary Entity:  Item
Business Intent: Which stock is not moving and where is obsolescence risk?
```

### PAGE-IN03

```text
Page ID:         PAGE-IN03
Page Name:       Inventory Forecast Dashboard
Parent Menu:     Inventory
Sidebar Label:   Inventory Forecast
Purpose:         30-day forward inventory level, stock-out / overstock risk, and purchase recommendations.
Primary Entity:  Item
Business Intent: Will active SKUs run out or stay overstocked?
```

### PAGE-IN04

```text
Page ID:         PAGE-IN04
Page Name:       Inventory Optimization Dashboard
Parent Menu:     Inventory
Sidebar Label:   Inventory Optimization
Purpose:         Recommended purchase, delay, transfer, and clearance actions from forecast and risk.
Primary Entity:  Item / Warehouse
Business Intent: What should purchasing and warehouse do next?
```

### PAGE-IN05

```text
Page ID:         PAGE-IN05
Page Name:       Inventory Report
Parent Menu:     Inventory
Sidebar Label:   Inventory Report
Purpose:         Stock balance rows (quantity greater than zero) by item and warehouse.
Primary Entity:  Item / Warehouse
Business Intent: Evidence behind inventory value.
```

### PAGE-PU01

```text
Page ID:         PAGE-PU01
Page Name:       Purchasing Management Dashboard
Parent Menu:     Purchasing
Sidebar Label:   Purchasing
Purpose:         Current-month purchasing attention — posting backlog, principal dependency, pace, inventory cross-risk.
Primary Entity:  Supplier / Principal
Business Intent: Which suppliers and purchase activities need management attention?
```

### PAGE-PU02

```text
Page ID:         PAGE-PU02
Page Name:       Purchasing Report
Parent Menu:     Purchasing
Sidebar Label:   Purchasing Report
Purpose:         Purchase invoice rows for a selected period, including posting status.
Primary Entity:  Purchase invoice / Supplier
Business Intent: Evidence behind purchasing totals and unposted invoices.
```

### PAGE-OP01

```text
Page ID:         PAGE-OP01
Page Name:       Branch / Warehouse Performance Dashboard
Parent Menu:     Operations
Sidebar Label:   Locations
Purpose:         Location concentration of inventory, at-risk stock, sales, and purchasing; wilayah sales contribution.
Primary Entity:  Warehouse / Wilayah
Business Intent: Are we too dependent on one warehouse or territory? Receivable risk by wilayah is on Collection Dashboard.
```

---

## 2.2 Pages not listed as their own sidebar item

These exist and are reachable from Entity Analytics or from dashboard rows.

### PAGE-WORKSPACE

```text
Page ID:         PAGE-WORKSPACE
Page Name:       Investigation Workspace
Parent Menu:     Executive (via Entity Analytics)
Purpose:         Population map and staged investigation for Customer, Salesman, Supplier, or Item.
Primary Entity:  Selected entity type
Business Intent: Compare an entity against its population and explain position with facts, context, and drivers.
```

### PAGE-PROFILE-CUSTOMER

```text
Page ID:         PAGE-PROFILE-CUSTOMER
Page Name:       Customer Performance Profile
Parent Menu:     Executive (via Entity Analytics) or Customer dashboards
Purpose:         Single-customer performance profile (KPI summary, comparison, trend, ranking, attention, related entities, evidence).
Primary Entity:  Customer
Business Intent: Understand one customer across domains.
```

### PAGE-PROFILE-SALESMAN

```text
Page ID:         PAGE-PROFILE-SALESMAN
Page Name:       Salesman Performance Profile
Parent Menu:     Executive (via Entity Analytics) or Salesman Performance
Purpose:         Single-salesman performance profile (same profile sections as other entities).
Primary Entity:  Salesman
Business Intent: Understand one salesperson across sales and receivables.
```

### PAGE-PROFILE-SUPPLIER

```text
Page ID:         PAGE-PROFILE-SUPPLIER
Page Name:       Supplier Performance Profile
Parent Menu:     Executive (via Entity Analytics) or Purchasing rankings
Purpose:         Single-supplier / principal performance profile.
Primary Entity:  Supplier
Business Intent: Understand one supplier’s commercial footprint.
```

### PAGE-PROFILE-ITEM

```text
Page ID:         PAGE-PROFILE-ITEM
Page Name:       Item Performance Profile
Parent Menu:     Executive (via Entity Analytics) or Inventory Risk rankings
Purpose:         Single-item performance profile.
Primary Entity:  Item
Business Intent: Understand one SKU’s movement, risk, and related entities.
```

Profile page title on screen is the entity display name (for example a customer name), not the generic title above.

### PAGE-COMPARE-CUSTOMER

```text
Page ID:         PAGE-COMPARE-CUSTOMER
Page Name:       Compare Customers
Parent Menu:     Executive (via Entity Analytics)
Purpose:         Side-by-side KPI, trend, ranking, attention, relationship, and performance-signature comparison (2–5 customers).
Primary Entity:  Customer
Business Intent: Compare selected customers.
```

### PAGE-COMPARE-SALESMAN

```text
Page ID:         PAGE-COMPARE-SALESMAN
Page Name:       Compare Salesmen
Parent Menu:     Executive (via Entity Analytics)
Purpose:         Same compare workspace for salesmen.
Primary Entity:  Salesman
Business Intent: Compare selected salespeople.
```

### PAGE-COMPARE-SUPPLIER

```text
Page ID:         PAGE-COMPARE-SUPPLIER
Page Name:       Compare Suppliers
Parent Menu:     Executive (via Entity Analytics)
Purpose:         Same compare workspace for suppliers.
Primary Entity:  Supplier
Business Intent: Compare selected suppliers.
```

### PAGE-COMPARE-ITEM

```text
Page ID:         PAGE-COMPARE-ITEM
Page Name:       Compare Items
Parent Menu:     Executive (via Entity Analytics)
Purpose:         Same compare workspace for items.
Primary Entity:  Item
Business Intent: Compare selected items.
```

### PAGE-DRAWER-SALESMAN

```text
Page ID:         PAGE-DRAWER-SALESMAN
Page Name:       Salesman Detail (side drawer)
Parent Menu:     Sales Force (from Salesman Performance)
Purpose:         Quick principal achievement and trend for one salesman without leaving the dashboard.
Primary Entity:  Salesman
Business Intent: Inspect principal mix when a profile is not used. Not a full page.
```

---

# SECTION 3
# WIDGET REGISTRY

Widget Type uses: KPI Card, Chart, Table, Distribution, Ranking, Trend, Forecast, Risk Indicator.

KPI Card here means a named metric tile or a named attention card that presents KPIs.  
Attention lists are Tables. Rankings are Ranking.

---

## PAGE-EX01 — Management Attention Center

**Filters:** none (Refresh only)

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-EX01-SALES | Sales | Risk Indicator | Sales attention at company level | Achievement % | Total Achievement | Sales Dashboard |
| W-EX01-PIUTANG | Piutang | Risk Indicator | Receivable attention | Total Piutang | Overdue Customer; > 90 Day Amount; Top Customer % | Piutang Dashboard |
| W-EX01-PURCHASING | Purchasing | Risk Indicator | Purchasing attention | Pending Posting | Top Principal % | Purchasing Management Dashboard |
| W-EX01-INVENTORY | Inventory | Risk Indicator | Inventory attention | Total Inventory Value | Top Category %; Top Supplier % | Inventory Dashboard |
| W-EX01-PF-HEALTHY | Portfolio Healthy % | KPI Card | Share of healthy customers | Portfolio Healthy % | Healthy Customers | Customer Portfolio Dashboard |
| W-EX01-PF-AT-RISK | Customers At Risk | KPI Card | Count of at-risk customers | Customers At Risk | At Risk Count | Customer Portfolio Dashboard |
| W-EX01-PF-STRATEGIC | Strategic At Risk | KPI Card | Strategic customers in risk | Strategic At Risk | Strategic Customers | Customer Portfolio Dashboard |
| W-EX01-TOP-CUST | Top 5 Customers | Ranking | Critical customer exposure | Amount (exposure) | Rank | Investigation → related report (when investigation is provided) |
| W-EX01-TOP-CAT | Top 5 Categories | Ranking | Critical category exposure | Amount | Rank | Investigation → related report |
| W-EX01-TOP-SUP | Top 5 Suppliers | Ranking | Critical supplier exposure | Amount | Rank | Investigation → related report |
| W-EX01-TOP-PRIN | Top 5 Principals | Ranking | Critical principal exposure | Amount | Rank | Investigation → related report |
| W-EX01-DOMAIN | Domain Summaries | Table | One-line status per domain with a detail path | Domain summary text | — | Domain dashboard named on the row |

Also on this page: **Open Alert Center** (not a widget — header action to Alert Center).

---

## PAGE-EX02 — Alert Center

**Filters / section jump:** Summary · Critical · Inventory · Concentrations · Alerts · Dashboards

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-EX02-PLATFORM | Platform Alerts | Risk Indicator | Platform / snapshot health messages | Platform status | — | Executive (Management Attention Center) when offered |
| W-EX02-CATEGORY | Category Attention | Risk Indicator | Alert counts by category (category names come from live data) | Alert count per category | — | Scrolls to that category panel |
| W-EX02-CRITICAL | Top Critical Alerts | Table | Highest-priority exceptions | Alert value | Category | Investigate → report; View Dashboard → domain dashboard |
| W-EX02-INV-RISK | Inventory Risk Summary | KPI Card | Inventory exception snapshot | Dead Stock | Slow Moving; Never Sold; At-Risk Inventory | Slow Moving & Dead Stock Dashboard |
| W-EX02-CONC | Concentrations | Ranking | Informational concentration (not treated as exceptions) | Concentration measure | — | Related domain dashboard |
| W-EX02-BY-CAT | Alerts by Category | Table | Full alert list grouped by category | Alert | — | View Dashboard; Investigate |
| W-EX02-NAV | Domain Dashboards | Table | Jump list of domain dashboards | — | — | Named dashboard |

---

## PAGE-EX03 — Entity Analytics

**Filters:** Search by code or name per entity type

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-EX03-WS | {Entity} Investigation | Table | Open Investigation Workspace for that entity type | — | — | Investigation Workspace |
| W-EX03-PROFILE | Open Profile | Table | Open a selected entity’s performance profile | — | — | Performance Profile |
| W-EX03-COMPARE | Compare {Entities} | Table | Open side-by-side compare | — | — | Compare Customers / Salesmen / Suppliers / Items |

---

## PAGE-SA01 — Sales Dashboard

**Filters:** none

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-SA01-TARGET | Total Target | KPI Card | Monthly sales target | Total Target | — | No |
| W-SA01-ACH | Total Achievement | KPI Card | Invoiced omzet this month | Total Achievement | — | No |
| W-SA01-PCT | Achievement % | KPI Card | Attainment versus target | Achievement % | — | No |
| W-SA01-TVA | Target vs Achievement | Chart | Target and achievement amounts | Total Target | Total Achievement | No |
| W-SA01-WEEK | Weekly Trend | Trend | Weekly invoiced omzet in the month | Weekly omzet | — | No |
| W-SA01-TOP10 | Top 10 Salesman | Ranking | Rank salesmen by invoiced omzet | Invoiced Omzet | Rank | Investigation → Sales Report (typical) |

---

## PAGE-SA02 — Sales Forecast Dashboard

**Filters:** none (period shown as text)

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-SA02-SUMMARY | Executive summary (sales forecast) | Forecast | Narrative of projected month-end sales | Forecast Sales | Forecast Achievement | No |
| W-SA02-KPI-ACT | Current vs Forecast KPIs | KPI Card | Current Sales; Current Achievement; Forecast Sales; Forecast Achievement | Forecast Sales | Current Achievement | No |
| W-SA02-KPI-PACE | Pace and gap KPIs | KPI Card | Daily Average Sales; Required Daily Sales; Target Gap; Days Remaining | Required Daily Sales | Target Gap | No |
| W-SA02-KPI-SCEN | Scenario KPIs | Forecast | Best Case; Expected; Worst Case; Forecast Confidence | Expected | Forecast Confidence | No |
| W-SA02-PACE | Daily Pace Trend | Trend | Daily invoiced sales versus MTD average | Daily actual | MTD daily average | No |
| W-SA02-FVT | Forecast vs Target | Chart | Forecast against target | Forecast Sales | Total Target | No |
| W-SA02-WEEK | Weekly Pace | Trend | Weekly invoiced omzet | Weekly omzet | — | No |
| W-SA02-RISK | Forecast Risk | Risk Indicator | Healthy / Warning / Critical forecast band | Forecast achievement | Required Daily Sales | Sales Report (footer: View evidence) |

---

## PAGE-SA03 — Sales Report

**Filters:** Period · Search (Filter rows…) · Apply

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-SA03-TABLE | Faktur list | Table | Invoice rows: Tanggal, Faktur, Customer, Sales, Total, Status | Total | — | No further portal drill-down (investigation breadcrumb may return to source dashboard) |

---

## PAGE-CU01 — Customer Analytics

**Filters / section jump:** Attention Cards · Attention List · Rankings · Segmentation  
**List filter:** All · signal labels from live attention data (for example Dormant, Plafond Breach)

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-CU01-COLL | Collection | Risk Indicator | Overdue and aged exposure | Overdue Customers | >90 Day Exposure | Piutang Dashboard |
| W-CU01-CONC | Concentration | KPI Card | Customer concentration of omzet and piutang | Top Omzet Customer % | Top Piutang Customer % | No |
| W-CU01-ACT | Activity | KPI Card | Customers billed this month | Active Customers (month) | — | Sales Dashboard |
| W-CU01-INACT | Inactivity | Risk Indicator | 90-day dormant customers | Dormant Customers (90-day) | — | Customer Attention List (Dormant) |
| W-CU01-CREDIT | Credit | Risk Indicator | Credit-limit and suspend issues | Plafond Breach | Suspended + Sales | Customer Attention List (Plafond Breach) |
| W-CU01-LIST | Customer Attention List | Table | Customers requiring attention, filterable by signal | Attention signal | Amounts on row | Performance Profile; Investigate → report |
| W-CU01-OMZET | Top 10 by Omzet (current month) | Ranking | Largest invoiced customers | Omzet | % of Total | Performance Profile (preferred) or Investigate |
| W-CU01-PIUT | Top 10 by Piutang (all open) | Ranking | Largest outstanding customers | Outstanding | % of Total | Performance Profile (preferred) or Investigate |
| W-CU01-SEG-AD | Active vs Dormant | Distribution | Active this month vs dormant 90-day | Active (month) | Dormant (90-day) | No |
| W-CU01-SEG-KL | By Klasifikasi | Distribution | Customer counts by classification | Customer count | Active / Dormant | No |
| W-CU01-SEG-WIL | By Wilayah | Distribution | Customer counts by territory | Customer count | Active / Dormant | No |
| W-CU01-NAV | Navigation | Table | Related sales / piutang links | — | — | Linked dashboard or report |

---

## PAGE-CU02 — Customer Risk Forecast Dashboard

**Filters:** none on the page (attention list has signal-family filter)

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-CU02-SUMMARY | Executive summary (risk forecast) | Forecast | Narrative of forward customer risk | Customers At Risk | Portfolio Health Score | Footer links (see drill-down registry) |
| W-CU02-KPI-PORT | Horizon / health KPIs | KPI Card | Horizon (days); Customers At Risk; Portfolio Health Score; Forecast Confidence | Customers At Risk | Portfolio Health Score | No |
| W-CU02-KPI-EXP | Exposure KPIs | KPI Card | Elevated Risk Receivable; Elevated Risk %; Total Piutang; High / Critical Customers | Elevated Risk Receivable | Total Piutang | No |
| W-CU02-KPI-CAT | Risk category counts | Distribution | Healthy; Watch; Attention; High Risk | High Risk | Attention | No |
| W-CU02-KPI-SIG | Signal family counts | Risk Indicator | Payment Delay; Credit Limit; Inactivity; Purchase Decline | Payment Delay | Inactivity | No |
| W-CU02-KPI-COLL | Collection-risk KPIs | Risk Indicator | Collection Risk; Critical Category; High Risk Category; Customers Forecasted At Risk | Customers Forecasted At Risk | Collection Risk | No |
| W-CU02-CAT | Risk Category Distribution | Chart | Mix of risk categories | Category count | — | No |
| W-CU02-EXP | Elevated Risk vs Total Piutang | Chart | Elevated risk receivable versus book | Elevated Risk Receivable | Total Piutang | No |
| W-CU02-WIL | Top Wilayah by Elevated Risk | Ranking | Territories with elevated risk | Elevated risk amount | — | No |
| W-CU02-MIX | Signal Family Mix | Chart | Mix of signal families | Signal count | — | No |
| W-CU02-TOP | Top Customers by Risk Priority | Table | Highest-priority at-risk customers | Risk priority | Elevated risk | Typical: profile or report when row actions exist |
| W-CU02-LIST | Customer Risk Attention List | Table | At-risk customers filterable by signal family | Signal family | — | Typical: profile / investigation |
| W-CU02-REC | Top Recommended Actions | Table | Recommended management actions | Action | Customer | No guaranteed entity page |

Footer navigation (not widgets): Customers Dashboard, Piutang Dashboard, Collection Dashboard, Cash Flow Forecast, Piutang Report, Sales Report.

---

## PAGE-CU03 — Collection Optimization Dashboard

**Filters:** Specialized Queues tabs — Proactive Reminders · Credit Review · Sales Recovery · Management Escalation

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-CU03-SUMMARY | Executive summary (collection optimization) | Forecast | Narrative of today’s collection plan | Actions Today | Collection Impact | Footer links |
| W-CU03-KPI-WORK | Workload KPIs | KPI Card | Actions Today; Immediate Collection; Proactive Reminders; Credit Review; Sales Recovery; Collection Impact | Actions Today | Collection Impact | No |
| W-CU03-KPI-CTX | Context KPIs | KPI Card | Overdue Exposure; Due Within 7 Days; Recovery vs Billing; Planning Confidence | Overdue Exposure | Recovery vs Billing | No |
| W-CU03-ACT | Actions by Category | Chart | Mix of action types | Action count | — | No |
| W-CU03-LOAD | Workload | Chart | Workload distribution | Actions | — | No |
| W-CU03-IMP-CH | Impact by Action Category | Chart | Value impact by action type | Impact (IDR) | — | No |
| W-CU03-PRI | Today's Collection Priorities | Table | Prioritized customers/actions for today | Priority | Impact | Typical: customer / report via row if present |
| W-CU03-Q | Specialized Queues | Table | Tabbed action queues | Queue count | — | No |
| W-CU03-IMP-TB | Top Impact Opportunities | Table | Highest-impact collection opportunities | Impact | Customer | Typical: customer / report |

Footer: Customer Risk Forecast, Collection, Customer Analytics, Piutang, Piutang Report, Sales Report.

---

## PAGE-CU04 — Customer Portfolio Dashboard

**Filters:** Attention Only · All Customers · Wilayah · Klasifikasi · Tier · Lifecycle · Action · Salesman

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-CU04-SUMMARY | Executive summary (portfolio) | KPI Card | Narrative plus value disclaimer | Portfolio Health Score | — | Footer links |
| W-CU04-KPI-H | Health KPIs | KPI Card | Portfolio Health Score; Portfolio Healthy %; Attention Customers; Customers At Risk | Portfolio Health Score | Customers At Risk | No |
| W-CU04-KPI-S | Strategic KPIs | KPI Card | Strategic Customers; Strategic At Risk; Working Capital Tied; Total Customers | Strategic At Risk | Working Capital Tied | No |
| W-CU04-KPI-L | Lifecycle KPIs | KPI Card | Never Purchased; Dormant; Declining; Total MTD Omzet | Dormant | Total MTD Omzet | No |
| W-CU04-LIFE | Lifecycle Distribution | Distribution | Mix of lifecycle stages | Stage count | — | No |
| W-CU04-TIER | Tier Distribution | Distribution | Mix of portfolio tiers | Tier count | — | No |
| W-CU04-QUEUE | Portfolio Priority Queue | Table | Customers ranked for portfolio action | Priority score | Action | Performance Profile; Customer Report (when row links exist) |
| W-CU04-ACTION | Customers by Portfolio Action | Table | Customers grouped by recommended action | Action | Count | Same as queue |
| W-CU04-OMZET | Top 10 MTD Omzet Concentration | Ranking | Largest MTD omzet customers | MTD Omzet | — | Typical: profile |
| W-CU04-PIUT | Top 10 Open Piutang Concentration | Ranking | Largest open-balance customers | Open Piutang | — | Typical: profile |

Footer: Customer Analytics, Customer Risk Forecast, Collection Optimization, Customer Report, Sales Report, Piutang Report.

---

## PAGE-CU05 — Customer Report

**Filters:** Search customer code or name · Customer code · Apply

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-CU05-SUM | Report totals | KPI Card | Total Customers; Total MTD Omzet; Total Open Balance | Total MTD Omzet | Total Open Balance | No |
| W-CU05-TABLE | Customer rows | Table | Code, Customer, Wilayah, Klasifikasi, Tier, Lifecycle, Action, Owner, Salesman, MTD Omzet, Open Balance, Overdue, Risk, Last Purchase, Sales Achievement | MTD Omzet | Open Balance | No further portal drill-down |

---

## PAGE-FI01 — Piutang Dashboard

**Filters:** none

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-FI01-TOT | Total Piutang | KPI Card | All open receivables | Total Piutang | — | No |
| W-FI01-CUST | Total Customer | KPI Card | Customers with open balance | Total Customer | — | No |
| W-FI01-ODC | Overdue Customer | KPI Card | Customers past due | Overdue Customer | — | No |
| W-FI01-ODP | Overdue Piutang | KPI Card | Past-due amount | Overdue Piutang | — | No |
| W-FI01-90 | Piutang > 90 Hari | KPI Card | Amount aged over 90 days | Piutang > 90 Hari | % of total | No |
| W-FI01-T10 | Top 10 Customer % | KPI Card | Share of book in top 10 customers | Top 10 Customer % | — | No |
| W-FI01-T20 | Top 20 Customer % | KPI Card | Share of book in top 20 customers | Top 20 Customer % | — | No |
| W-FI01-AGING | Aging Distribution | Distribution | Current, 1–30, 31–60, 61–90, >90 | Aging bucket amount | — | No |
| W-FI01-TOP20 | Top 20 Outstanding Customers — Aging Breakdown | Table | Largest outstanding customers with aging split | Outstanding | Aging buckets | Investigation → Piutang Report (typical) |

---

## PAGE-FI02 — Collection Dashboard

**Filters / section jump:** Attention Cards · Recovery · Aging Risk · Attention List · Rankings  
**List filter:** All · signal labels from live collection attention data

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-FI02-EXP | Exposure | Risk Indicator | Overdue size and concentration | Overdue Exposure | >90d Exposure; Overdue Concentration % | Collection Attention List (Chronic Overdue) |
| W-FI02-REC | Recovery | Risk Indicator | Month-to-date collection effectiveness | Cash Collected MTD | Recovery vs Billing % | Collection Attention List (Low Recovery vs Billing) |
| W-FI02-PORT | Portfolio | Risk Indicator | Legacy overdue accounts | Legacy Debt Count | — | Collection Attention List (Legacy Debt) |
| W-FI02-RSUM | Recovery Summary | KPI Card | Cash Collected MTD; Recovery vs Billing %; Payment Mix (Cash / Giro / Adjustment) | Cash Collected MTD | Recovery vs Billing % | No |
| W-FI02-AGING | Aging Risk Summary (Overdue Only) | Distribution | Aging of overdue balances only | Overdue aging | — | No |
| W-FI02-LIST | Collection Attention List | Table | Collection-attention customers | Signal | Exposure | Investigate → report |
| W-FI02-RC | Top 10 Overdue Customers | Ranking | Largest overdue customers | Amount | % of Total | Investigation → report |
| W-FI02-RS | Top 10 Overdue Salesmen | Ranking | Salesmen with largest overdue | Amount | % of Total | Investigation → report |
| W-FI02-RW | Top 10 Overdue Wilayah | Ranking | Territories with largest overdue | Amount | % of Total | No |
| W-FI02-NAV | Navigation | Table | Related links | — | — | Linked page |

---

## PAGE-FI03 — Cash Flow Forecast Dashboard

**Filters:** none

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-FI03-SUMMARY | Executive summary (cash flow) | Forecast | Narrative of projected collections | Expected Cash Collection | Collection Forecast % | Footer links |
| W-FI03-KPI-CASH | Cash position KPIs | KPI Card | Cash Collected MTD; Expected Cash Collection; Projected Month-End Collection; Collection Forecast % | Expected Cash Collection | Cash Collected MTD | No |
| W-FI03-KPI-PACE | Pace KPIs | KPI Card | Daily Cash Collection Average; Required Daily Collection; Remaining Collection Target; Remaining Calendar Days | Required Daily Collection | Remaining Collection Target | No |
| W-FI03-KPI-REC | Recovery scenario KPIs | Forecast | Recovery vs Billing (Actual); Recovery vs Billing Forecast; Best / Exp / Worst Cash; Forecast Confidence | Recovery vs Billing Forecast | Forecast Confidence | No |
| W-FI03-KPI-AR | Receivable context KPIs | KPI Card | Outstanding Due Remaining; Overdue Outstanding; Collection Gap; Forecast Variance (Cash) | Collection Gap | Overdue Outstanding | No |
| W-FI03-PACE | Daily Collection Pace | Trend | Daily cash versus MTD average | Daily cash | MTD daily cash average | No |
| W-FI03-FVB | Cash Forecast vs Billing | Chart | Billing versus cash MTD and projected cash | Expected Cash Collection | Month Faktur Omzet | No |
| W-FI03-TREND | Recovery Trend | Trend | Cumulative collections versus billing | Cumulative collections | Cumulative billing | No |
| W-FI03-RISK | Top Collection Risks | Table | Customers / situations threatening the cash forecast | Risk amount | — | Typical: report via footer |

Footer: Piutang Report, Collection Dashboard.

---

## PAGE-FI04 — Piutang Report

**Filters:** Period · Filter by Jatuh Tempo or Piutang Date · Search · Apply

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-FI04-SUM | Report totals | KPI Card | Total Piutang; Total Customer | Total Piutang | Total Customer | No |
| W-FI04-TABLE | Open receivable rows | Table | Customer, Sales, Faktur, Tanggal, Jatuh Tempo, Total Jual, Kurang Bayar | Kurang Bayar | — | No further portal drill-down |

---

## PAGE-SF01 — Salesman Performance

**Filters:** Show Inactive Salesmen · Attention List signal filter  
**Section jump:** Attention Cards · Filters · Attention List · Performance Rankings · Exposure Rankings · Segmentation

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-SF01-PERF | Performance | Risk Indicator | Below-target and missing-target salesmen | Below Target | Missing Target Setup | Sales Dashboard |
| W-SF01-COLL | Collection Exposure | Risk Indicator | Salesmen with high overdue / piutang | High Overdue Exposure | High Piutang Exposure | Piutang Dashboard |
| W-SF01-PORT | Portfolio | Risk Indicator | Dormant books and salesman concentration | Dormant Portfolio | Top Omzet Salesman %; Top Piutang Salesman % | Salesman Attention List (Dormant Customer Portfolio) |
| W-SF01-LIST | Salesman Attention List | Table | Salesmen requiring attention | Signal | — | Performance Profile; or Salesman Detail drawer when no profile route |
| W-SF01-OMZET | Top 10 Omzet (current month) | Ranking | Highest invoiced salesmen | Omzet | % of Total | Performance Profile |
| W-SF01-ACH | Top 10 Achievement % | Ranking | Highest attainment | Achievement % | — | Performance Profile |
| W-SF01-PIUT | Top 10 Piutang (all open) | Ranking | Highest outstanding books | Outstanding | % of Total | Performance Profile |
| W-SF01-SEG-AI | Active vs Inactive | Distribution | Active versus inactive salesmen | Count | — | No |
| W-SF01-SEG-WIL | By Wilayah | Distribution | Salesmen by territory | Count | — | No |
| W-SF01-SEG-SEG | By Segment | Distribution | Salesmen by segment | Count | — | No |
| W-SF01-NAV | Navigation | Table | Related links | — | — | Linked page |
| W-SF01-DRAWER | Salesman Detail | Table | Drawer: Principal Achievement; Trend | Principal omzet | Achievement % | Stays on this page |

---

## PAGE-SF02 — Sales Force Overview

**Filters:** Today · Yesterday · Custom date · Refresh · table search (code or name) · ranking Top 5 / Top 10 · trend 7 days / 30 days

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-SF02-ACTIVE | Active Salesmen | KPI Card | Field-active headcount | Active Salesmen | — | Salesman Field Activity |
| W-SF02-PLAN | Planned | KPI Card | Scheduled visits | Planned | — | Salesman Field Activity |
| W-SF02-ACT | Actual | KPI Card | Executed visits | Actual | — | Salesman Field Activity |
| W-SF02-EXEC | Execution % | KPI Card | Actual versus plan | Execution % | — | Salesman Field Activity |
| W-SF02-CALLS | Effective Calls | KPI Card | Customer contacted | Effective Calls | — | Salesman Field Activity |
| W-SF02-ECR | Effective Call Rate | KPI Card | Effective calls of actual visits | Effective Call Rate | — | Salesman Field Activity |
| W-SF02-ORD | Orders | KPI Card | Sales orders | Orders | — | Salesman Field Activity |
| W-SF02-VAL | Order Value | KPI Card | Order value | Order Value | — | Salesman Field Activity |
| W-SF02-GPS | GPS Valid Rate | KPI Card | GPS quality (target >95%) | GPS Valid Rate | — | Salesman Field Activity |
| W-SF02-MISS | Missed Visit | KPI Card | Planned but skipped | Missed Visit | — | Salesman Field Activity |
| W-SF02-UNP | Unplanned Visit | KPI Card | Off-schedule visits | Unplanned Visit | — | Salesman Field Activity |
| W-SF02-TABLE | Salesman Performance (field table) | Table | One row per salesman for the visit date | Execution % | Orders | Salesman Field Activity (that salesman + date) |
| W-SF02-CH-EXEC | Visit Execution % | Chart | Compare execution across salesmen | Execution % | — | Salesman Field Activity |
| W-SF02-CH-ECR | Effective Call Rate | Chart | Compare effective-call rate | Effective Call Rate | — | Salesman Field Activity |
| W-SF02-CH-ORD | Orders Generated | Chart | Compare order counts | Orders | — | Salesman Field Activity |
| W-SF02-CH-VAL | Order Value | Chart | Compare order value | Order Value | — | Salesman Field Activity |
| W-SF02-RK-TE | Top Visit Execution | Ranking | Best execution | Execution % | — | Salesman Field Activity |
| W-SF02-RK-BE | Bottom Visit Execution | Ranking | Worst execution | Execution % | — | Salesman Field Activity |
| W-SF02-RK-TF | Top Effective Call Rate | Ranking | Best effective-call rate | Effective Call Rate | — | Salesman Field Activity |
| W-SF02-RK-BF | Bottom Effective Call Rate | Ranking | Worst effective-call rate | Effective Call Rate | — | Salesman Field Activity |
| W-SF02-RK-VAL | Top Order Value | Ranking | Highest order value | Order Value | — | Salesman Field Activity |
| W-SF02-RK-ORD | Top Orders | Ranking | Highest order count | Orders | — | Salesman Field Activity |
| W-SF02-RK-MISS | Most Missed Visits | Ranking | Most missed | Missed Visit | — | Salesman Field Activity |
| W-SF02-RK-UNP | Most Unplanned Visits | Ranking | Most unplanned | Unplanned Visit | — | Salesman Field Activity |
| W-SF02-TREND | Team Execution Trends | Trend | Team execution over 7 or 30 days | Execution % | — | No |
| W-SF02-WIL | Visits by Wilayah | Chart | Visit counts by territory | Visit count | — | No |

---

## PAGE-SF03 — Salesman Field Activity

**Filters:** Select salesman · Today · Yesterday · Custom Date · Load

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-SF03-PLAN | Planned | KPI Card | Planned visits | Planned | — | No |
| W-SF03-ACT | Actual | KPI Card | Actual visits | Actual | — | No |
| W-SF03-EFF | Effective | KPI Card | Effective calls | Effective | — | No |
| W-SF03-MISS | Missed | KPI Card | Missed visits | Missed | — | No |
| W-SF03-UNP | Unplanned | KPI Card | Unplanned visits | Unplanned | — | No |
| W-SF03-EXEC | Execution % | KPI Card | Actual versus plan | Execution % | — | No |
| W-SF03-ECR | Effective Call Rate | KPI Card | Effective of actual | Effective Call Rate | — | No |
| W-SF03-MAP | Field Activity Map | Chart | Geographic visit points | — | — | No |
| W-SF03-MISS-L | Missed Visits | Table | List of missed stops | — | — | No |
| W-SF03-TIME | Visit Timeline | Table | Ordered visits with Play / Pause / Reset replay | — | — | No |

Header links: Sales Force Overview; View sales performance → Salesman Performance.

---

## PAGE-IN01 — Inventory Dashboard

**Filters:** none

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-IN01-VAL | Total Inventory Value | KPI Card | Stock capital | Total Inventory Value | — | No |
| W-IN01-ITEM | Total Item | KPI Card | Item count with stock | Total Item | — | No |
| W-IN01-CAT | Inventory by Category | Chart | Value by category | Category value | — | No |
| W-IN01-SUP | Inventory by Supplier | Chart | Value by supplier | Supplier value | — | No |
| W-IN01-T10C | Top 10 Categories | Ranking | Largest categories | Value | — | Investigation → Inventory Report (typical) |
| W-IN01-T10S | Top 10 Suppliers | Ranking | Largest suppliers | Value | — | Investigation → Inventory Report (typical) |

---

## PAGE-IN02 — Slow Moving & Dead Stock Dashboard

**Filters / section jump:** Attention Cards · Risk Exposure · Attention List · Rankings  
**List filter:** attention signal (including Dead Stock / Slow Moving when driven from KPI cards)

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-IN02-DSC | Dead Stock Item Count | KPI Card | Count of dead-stock items | Dead Stock Item Count | — | Inventory Attention List (Dead Stock) |
| W-IN02-DSV | Dead Stock Value | KPI Card | Value of dead stock | Dead Stock Value | — | Inventory Attention List (Dead Stock) |
| W-IN02-SMC | Slow Moving Item Count | KPI Card | Count of slow-moving items | Slow Moving Item Count | — | Inventory Attention List (Slow Moving) |
| W-IN02-SMV | Slow Moving Value | KPI Card | Value of slow moving | Slow Moving Value | — | Inventory Attention List (Slow Moving) |
| W-IN02-AR | At-Risk Inventory % | KPI Card | At-risk share of inventory | At-Risk Inventory % | — | Inventory Attention List |
| W-IN02-CARD | Inventory Risk | Risk Indicator | Conditional attention card for at-risk inventory | At-Risk Inventory | — | Inventory Attention List |
| W-IN02-AGING | Inventory Aging Distribution | Distribution | Active / Slow Moving / Dead Stock / Never Sold style buckets | Bucket value | — | No |
| W-IN02-CAT | Category Risk Exposure | Chart | Risk value by category | Risk value | — | No |
| W-IN02-SUP | Supplier Risk Exposure | Chart | Risk value by supplier | Risk value | — | No |
| W-IN02-LIST | Inventory Attention List | Table | Items requiring inventory attention | Signal | Value | Performance Profile; Investigate |
| W-IN02-T10D | Top 10 Dead Stock by Value | Ranking | Highest dead-stock value | Value | — | Performance Profile or Investigate |
| W-IN02-T10S | Top 10 Slow Moving by Value | Ranking | Highest slow-moving value | Value | — | Performance Profile or Investigate |

---

## PAGE-IN03 — Inventory Forecast Dashboard

**Filters:** none

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-IN03-SUMMARY | Executive summary (inventory forecast) | Forecast | Narrative of 30-day inventory outlook | Inventory Health Score | Stock-Out Risk Items | Footer links |
| W-IN03-KPI-POS | Position KPIs | KPI Card | Current Inventory Value; Projected Inventory Value @ H; Avg Days of Supply; Inventory Health Score | Projected Inventory Value @ H | Inventory Health Score | No |
| W-IN03-KPI-RISK | Risk exposure KPIs | Risk Indicator | Stock-Out Risk Items; Overstock Value; Understock Value; At-Risk Inventory % | Stock-Out Risk Items | Overstock Value | No |
| W-IN03-KPI-SCEN | Scenario KPIs | Forecast | Best Case Projected; Expected Projected; Worst Case Projected; Forecast Confidence | Expected Projected | Forecast Confidence | No |
| W-IN03-KPI-PACE | Consumption KPIs | KPI Card | Avg Daily Consumption (units); Forecast Consumption @ H; Inventory Coverage %; Turnover Forecast | Inventory Coverage % | Turnover Forecast | No |
| W-IN03-LEVEL | Forecast Inventory Level | Trend | Projected inventory value over the horizon | Projected inventory value | — | No |
| W-IN03-CONS | Consumption Trend | Trend | Daily units sold vs 30-day ADC | Daily units sold | 30-day ADC | No |
| W-IN03-HEAT | Risk Heat Summary | Chart | Heat of inventory risks | Risk heat | — | No |
| W-IN03-RISK | Top Inventory Risks | Table | Highest-risk items | Risk | Value | Typical: item / report |
| W-IN03-PUR | Purchasing Recommendations | Table | Suggested purchases | Recommended buy | — | Purchasing Management Dashboard (footer) |

Footer: Inventory Report, Inventory Dashboard, Inventory Risk, Purchasing Dashboard.

---

## PAGE-IN04 — Inventory Optimization Dashboard

**Filters:** none

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-IN04-SUMMARY | Executive summary (optimization) | Forecast | Narrative of recommended actions | Critical Actions | Recommended Purchase Budget | Footer links |
| W-IN04-KPI-H | Health / budget KPIs | KPI Card | Inventory Health Score; Critical Actions; Recommended Purchase Budget; Deferrable Spend | Critical Actions | Recommended Purchase Budget | No |
| W-IN04-KPI-MIX | Action mix KPIs | KPI Card | Purchase Now; Delay; Transfer; Clearance Review | Purchase Now | Clearance Review | No |
| W-IN04-PRI | Priority Score Distribution | Distribution | Mix of action priorities | Action count | — | No |
| W-IN04-IMP | Business Impact Summary | Chart | Purchase impact, deferrable spend, recoverable capital | Purchase impact | Deferrable Spend | No |
| W-IN04-HEAT | Action Heat Summary | Chart | Heat of recommended actions | Heat | — | No |
| W-IN04-ACT | Top Optimization Actions | Table | Highest-priority actions | Priority | Item | Typical: item |
| W-IN04-REO | Recommended Reorder List | Table | Purchase-now candidates | Reorder qty / value | — | Typical: item / purchasing |
| W-IN04-XFER | Warehouse Rebalancing | Table | Transfer recommendations | From / to warehouse | — | Typical: item |
| W-IN04-DELAY | Overstock & Delay Purchasing | Table | Delay-buy recommendations | Delay value | — | Typical: item / purchasing |
| W-IN04-CLR | Dead Stock Recovery | Table | Clearance candidates | Dead stock value | — | Typical: item / inventory risk |

Footer: Inventory Forecast, Inventory Risk, Purchasing Management, Inventory Report, Purchasing Report.

---

## PAGE-IN05 — Inventory Report

**Filters:** Search only (no date range)

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-IN05-SUM | Report totals | KPI Card | Total Inventory Value; Total Item | Total Inventory Value | Total Item | No |
| W-IN05-TABLE | Stock rows | Table | Item, Warehouse, Qty, HPP, Nilai Sediaan (row is per warehouse; totals are by item) | Nilai Sediaan | Qty | No further portal drill-down |

---

## PAGE-PU01 — Purchasing Management Dashboard

**Filters / section jump:** Attention Cards · Summary · Attention List · Charts · Rankings  
**List filter:** attention signal

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-PU01-POST | Posting Exposure | Risk Indicator | Unposted purchase invoices | Metrics supplied on the card (live labels) | — | Purchasing Attention List (Qualified Backlog) |
| W-PU01-DEP | Principal Dependency | Risk Indicator | Dependence on a few principals | Metrics on the card | — | Purchasing Attention List (Compound Dependency) |
| W-PU01-PACE | Purchasing Pace | Risk Indicator | Purchase pace this month | Metrics on the card | — | Purchasing Attention List |
| W-PU01-XRISK | Inventory Cross-Risk | Risk Indicator | Purchasing overlapping inventory risk | Metrics on the card | — | Purchasing Attention List |
| W-PU01-SUM | Purchasing Summary | KPI Card | Grand Total Purchase; Total Invoice; Posted %; Pending Posting Value (all BELUM); Qualified Backlog | Grand Total Purchase | Pending Posting Value (all BELUM) | No |
| W-PU01-LIST | Purchasing Attention List | Table | Suppliers / invoices needing attention | Signal | — | Performance Profile; Investigate |
| W-PU01-WEEK | Weekly Purchase Trend | Trend | Weekly purchase amounts | Weekly purchase | — | No |
| W-PU01-POST-CH | Posting Status Breakdown | Distribution | Posted versus unposted mix | Posted % | — | No |
| W-PU01-T10 | Top 10 Principals | Ranking | Largest MTD purchase principals | MTD Purchase Amount | % of Purchase | Performance Profile or Investigate |
| W-PU01-EXP | Principal Exposure Comparison | Table | Principal concentration comparison | Purchase share | — | Performance Profile |
| W-PU01-NAV | Navigation | Table | Related links | — | — | Linked page |

---

## PAGE-PU02 — Purchasing Report

**Filters:** Period · Search · Apply · optional posting filter when opened from investigation

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-PU02-SUM | Report totals | KPI Card | Grand Total Purchase; Total Invoice | Grand Total Purchase | Total Invoice | No |
| W-PU02-TABLE | Purchase invoice rows | Table | Invoice, Date, Supplier, Warehouse, Total, Disc, Tax, Grand Total, Posting Stok | Grand Total | Posting Stok | No further portal drill-down |

---

## PAGE-OP01 — Branch / Warehouse Performance Dashboard

**Filters:** none

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-OP01-INV | Inventory Concentration | Risk Indicator | Warehouse inventory concentration | Top Warehouse Inventory % | Top 3 Warehouse Inventory % | No |
| W-OP01-AR | At-Risk Concentration | Risk Indicator | Warehouse at-risk stock concentration | Top Warehouse At-Risk % | — | No |
| W-OP01-SALES | Sales Concentration | Risk Indicator | Warehouse and wilayah sales concentration | Top Warehouse Sales % | Top Wilayah Sales % | No |
| W-OP01-OPS | Operational Signals | Risk Indicator | Idle or unsold location stock | Inactive Warehouse With Stock | Stock Without Sales | No |
| W-OP01-LIST | Location Attention List | Table | Locations requiring attention | Signal | — | Investigate |
| W-OP01-T10I | Top 10 Warehouse by Inventory Value | Ranking | Largest warehouses by stock value | Inventory value | — | Investigation → Inventory Report (typical) |
| W-OP01-T10R | Top 10 Warehouse by At-Risk Value | Ranking | Largest at-risk warehouses | At-risk value | — | Slow Moving & Dead Stock Dashboard (or investigation dashboard) |
| W-OP01-T10O | Top 10 Warehouse by MTD Omzet | Ranking | Largest billing warehouses | MTD Omzet | — | Investigation when provided |
| W-OP01-T10P | Top 10 Warehouse by MTD Purchase | Ranking | Largest purchasing warehouses | MTD Purchase | — | Investigation when provided |
| W-OP01-T10W | Top 10 Wilayah by MTD Omzet | Ranking | Largest territories by billing | MTD Omzet | — | Collection Dashboard |

---

## PAGE-WORKSPACE — Investigation Workspace

**Filters:** Entity type · map preset · search · dimension filter · Attention only · Clear selection

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-WS-MAP | Population Map | Chart | Place entities on the investigation map | Axis KPIs from the selected map preset | — | Select entity on map |
| W-WS-FACTS | Current Facts | KPI Card | KPI summary for the selected entity (names from live KPI display names) | KPI DisplayName | — | Related entity may switch workspace |
| W-WS-CTX | Context | Chart | Peer position, trajectory, signal history, position history | Peer position | Trajectory | No |
| W-WS-EXPLAIN | Explanation | Table | Business drivers | Driver | — | No |
| W-WS-VAL | Validation | Table | Completeness / validation of the investigation | — | — | No |

---

## PAGE-PROFILE-* — Performance Profile (all four entity types)

Same widgets on Customer, Salesman, Supplier, and Item profiles. KPI card titles are the live KPI display names for that entity.

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-PRF-OVER | Overview | Table | Identity, business, performance, activity, details | Entity identity | — | No |
| W-PRF-KPI | KPI Summary | KPI Card | Entity KPIs | KPI DisplayName | — | No |
| W-PRF-COMP | Comparison | Table | Versus peers | Peer KPI | — | Compare {Entities} (header) |
| W-PRF-TREND | Trend | Trend | KPI over time | Trend KPI | — | No |
| W-PRF-SIG | Performance Signature | Chart | Radar / signature versus peer average | Signature KPIs | Peer average | No |
| W-PRF-RANK | Ranking History | Ranking | Rank over time | Rank | — | No |
| W-PRF-ATT | Attention History | Table | Past attention signals | Signal | — | No |
| W-PRF-REL | Related Entities | Table | Linked customers / salesmen / items / suppliers | Related name | — | Related Performance Profile or Workspace |
| W-PRF-EVD | Evidence | Table | Supporting records | — | — | Related report |

Header actions: Investigation Workspace; Compare {Entities}; Entity Analytics.

---

## PAGE-COMPARE-* — Compare (all four entity types)

**Filters:** Entity 1..N pickers (2–5) · Add entity · Compare

| Widget ID | Widget Name | Type | Purpose | Primary KPI | Secondary KPI | Drill-Down |
| --------- | ----------- | ---- | ------- | ----------- | ------------- | ---------- |
| W-CMP-KPI | KPI Comparison | Table | Side-by-side KPIs | KPI DisplayName | — | No |
| W-CMP-TREND | Trend Comparison | Trend | Side-by-side trends | Trend KPI | — | No |
| W-CMP-RANK | Ranking Comparison | Ranking | Side-by-side ranks | Rank | — | No |
| W-CMP-ATT | Attention Comparison | Table | Side-by-side attention | Signal | — | No |
| W-CMP-REL | Relationship Comparison | Table | Side-by-side related entities | Related name | — | No |
| W-CMP-SIG | Performance Signature | Chart | Side-by-side signatures | Signature KPIs | — | No |

---

# SECTION 4
# DRILL-DOWN REGISTRY

Two reusable patterns appear on many widgets:

1. **Investigation** — row opens a **Report** (Sales, Piutang, Inventory, Purchasing, or Customer) already filtered to the investigated subject. A breadcrumb returns to the source dashboard. The exact report is chosen by the investigation attached to that row.
2. **Performance Profile** — row opens the entity’s profile (Customer, Salesman, Supplier, or Item). Hint on several rankings: “Click a row to open Performance Profile”.

---

```text
DrillDown ID:    DD-EX01-ALERT
Source Widget:   Open Alert Center (header)
Target Page:     Alert Center
Target Entity:   Alert
Purpose:         Move from company scan to exception list.
```

```text
DrillDown ID:    DD-EX01-SALES
Source Widget:   Sales
Target Page:     Sales Dashboard
Target Entity:   Company sales
Purpose:         Open current-month sales performance.
```

```text
DrillDown ID:    DD-EX01-PIUTANG
Source Widget:   Piutang
Target Page:     Piutang Dashboard
Target Entity:   Receivable book
Purpose:         Open receivable quality.
```

```text
DrillDown ID:    DD-EX01-PURCHASING
Source Widget:   Purchasing
Target Page:     Purchasing Management Dashboard
Target Entity:   Purchasing
Purpose:         Open purchasing attention.
```

```text
DrillDown ID:    DD-EX01-INVENTORY
Source Widget:   Inventory
Target Page:     Inventory Dashboard
Target Entity:   Inventory
Purpose:         Open stock snapshot.
```

```text
DrillDown ID:    DD-EX01-PORTFOLIO
Source Widget:   Portfolio Healthy % / Customers At Risk / Strategic At Risk
Target Page:     Customer Portfolio Dashboard
Target Entity:   Customer portfolio
Purpose:         Open portfolio management.
```

```text
DrillDown ID:    DD-EX01-DOMAIN
Source Widget:   Domain Summaries
Target Page:     The domain dashboard named on the row
Target Entity:   Domain
Purpose:         Jump from executive one-liner to the detailed dashboard.
```

```text
DrillDown ID:    DD-EX01-EXPOSURE
Source Widget:   Top 5 Customers / Categories / Suppliers / Principals
Target Page:     Related report (investigation)
Target Entity:   Customer / Category / Supplier / Principal
Purpose:         Show evidence behind a critical exposure row.
```

```text
DrillDown ID:    DD-EX02-INVESTIGATE
Source Widget:   Top Critical Alerts / Alerts by Category
Target Page:     Related report (investigation)
Target Entity:   Subject of the alert
Purpose:         Show records behind the exception.
```

```text
DrillDown ID:    DD-EX02-DASHBOARD
Source Widget:   View Dashboard / Concentrations / Domain Dashboards / Inventory Risk Summary
Target Page:     Named domain dashboard
Target Entity:   Domain
Purpose:         Open the dashboard that owns the exception.
```

```text
DrillDown ID:    DD-EX03-WORKSPACE
Source Widget:   {Entity} Investigation
Target Page:     Investigation Workspace
Target Entity:   Customer / Salesman / Supplier / Item
Purpose:         Population investigation for that entity type.
```

```text
DrillDown ID:    DD-EX03-PROFILE
Source Widget:   Open Profile
Target Page:     Performance Profile
Target Entity:   Selected entity
Purpose:         Open one entity’s profile.
```

```text
DrillDown ID:    DD-EX03-COMPARE
Source Widget:   Compare {Entities}
Target Page:     Compare Customers / Salesmen / Suppliers / Items
Target Entity:   Same type
Purpose:         Side-by-side comparison.
```

```text
DrillDown ID:    DD-SA01-TOP10
Source Widget:   Top 10 Salesman
Target Page:     Sales Report (typical investigation)
Target Entity:   Salesman
Purpose:         Show fakturs behind a salesman ranking.
```

```text
DrillDown ID:    DD-SA02-EVIDENCE
Source Widget:   Forecast Risk / footer
Target Page:     Sales Report
Target Entity:   Faktur
Purpose:         Evidence for the sales forecast.
```

```text
DrillDown ID:    DD-CU01-COLL
Source Widget:   Collection
Target Page:     Piutang Dashboard
Target Entity:   Receivable
Purpose:         See overdue quality behind customer collection attention.
```

```text
DrillDown ID:    DD-CU01-ACT
Source Widget:   Activity
Target Page:     Sales Dashboard
Target Entity:   Sales
Purpose:         See invoicing behind active-customer count.
```

```text
DrillDown ID:    DD-CU01-LIST-FILTER
Source Widget:   Inactivity / Credit
Target Page:     Customer Attention List (same page)
Target Entity:   Customer
Purpose:         Jump to the matching attention signal.
```

```text
DrillDown ID:    DD-CU01-PROFILE
Source Widget:   Customer Attention List / Top 10 by Omzet / Top 10 by Piutang
Target Page:     Customer Performance Profile
Target Entity:   Customer
Purpose:         Open the customer profile. Falls back to investigation/report if no profile route.
```

```text
DrillDown ID:    DD-CU02-FOOTER
Source Widget:   Footer links
Target Page:     Customer Analytics; Piutang Dashboard; Collection Dashboard; Cash Flow Forecast; Piutang Report; Sales Report
Target Entity:   Mixed
Purpose:         Related context for risk forecast.
```

```text
DrillDown ID:    DD-CU03-FOOTER
Source Widget:   Footer links
Target Page:     Customer Risk Forecast; Collection; Customer Analytics; Piutang; Piutang Report; Sales Report
Target Entity:   Mixed
Purpose:         Related context for today’s collection actions.
```

```text
DrillDown ID:    DD-CU04-ROW
Source Widget:   Portfolio Priority Queue / action and concentration tables
Target Page:     Customer Performance Profile and/or Customer Report
Target Entity:   Customer
Purpose:         Inspect the customer behind a portfolio action.
```

```text
DrillDown ID:    DD-FI01-TOP20
Source Widget:   Top 20 Outstanding Customers — Aging Breakdown
Target Page:     Piutang Report (typical investigation)
Target Entity:   Customer
Purpose:         Show open invoices behind a large outstanding customer.
```

```text
DrillDown ID:    DD-FI02-LIST
Source Widget:   Collection Attention Cards
Target Page:     Collection Attention List (same page)
Target Entity:   Customer
Purpose:         Filter the list to Chronic Overdue, Low Recovery vs Billing, or Legacy Debt.
```

```text
DrillDown ID:    DD-FI02-RANK
Source Widget:   Top 10 Overdue Customers / Top 10 Overdue Salesmen
Target Page:     Related report (investigation)
Target Entity:   Customer or Salesman
Purpose:         Evidence behind overdue ranking.
```

```text
DrillDown ID:    DD-FI02-WILAYAH
Source Widget:   Top 10 Overdue Wilayah
Target Page:     (none)
Target Entity:   Wilayah
Purpose:         Display only — no row drill-down on this ranking.
```

```text
DrillDown ID:    DD-FI03-FOOTER
Source Widget:   Footer
Target Page:     Piutang Report; Collection Dashboard
Target Entity:   Receivable / Collection
Purpose:         Evidence and recovery context for the cash forecast.
```

```text
DrillDown ID:    DD-SF01-PERF
Source Widget:   Performance
Target Page:     Sales Dashboard
Target Entity:   Sales
Purpose:         Company sales context for below-target salesmen.
```

```text
DrillDown ID:    DD-SF01-COLL
Source Widget:   Collection Exposure
Target Page:     Piutang Dashboard
Target Entity:   Receivable
Purpose:         Company receivable context for high-exposure salesmen.
```

```text
DrillDown ID:    DD-SF01-PROFILE
Source Widget:   Salesman Attention List / performance and exposure rankings
Target Page:     Salesman Performance Profile
Target Entity:   Salesman
Purpose:         Open the salesman profile.
```

```text
DrillDown ID:    DD-SF01-DRAWER
Source Widget:   Salesman Attention List (when no profile route)
Target Page:     Salesman Detail drawer
Target Entity:   Salesman
Purpose:         Principal Achievement and Trend without leaving the dashboard.
```

```text
DrillDown ID:    DD-SF02-DETAIL
Source Widget:   Field KPIs / Salesman Performance table / comparison charts / performance rankings
Target Page:     Salesman Field Activity
Target Entity:   Salesman + visit date
Purpose:         Replay one salesman’s day.
```

```text
DrillDown ID:    DD-SF03-OVERVIEW
Source Widget:   Breadcrumb
Target Page:     Sales Force Overview
Target Entity:   Sales force
Purpose:         Return to team comparison.
```

```text
DrillDown ID:    DD-SF03-SALESMEN
Source Widget:   View sales performance
Target Page:     Salesman Performance
Target Entity:   Salesman
Purpose:         Commercial performance for the same salesperson.
```

```text
DrillDown ID:    DD-IN01-RANK
Source Widget:   Top 10 Categories / Top 10 Suppliers
Target Page:     Inventory Report (typical investigation)
Target Entity:   Category / Supplier
Purpose:         Stock rows behind a concentration ranking.
```

```text
DrillDown ID:    DD-IN02-LIST
Source Widget:   Dead Stock / Slow Moving KPI cards
Target Page:     Inventory Attention List (same page)
Target Entity:   Item
Purpose:         Filter to dead stock or slow moving.
```

```text
DrillDown ID:    DD-IN02-PROFILE
Source Widget:   Inventory Attention List / Top 10 Dead Stock / Top 10 Slow Moving
Target Page:     Item Performance Profile
Target Entity:   Item
Purpose:         Open the item profile. Falls back to investigation/report.
```

```text
DrillDown ID:    DD-IN03-FOOTER
Source Widget:   Footer
Target Page:     Inventory Report; Inventory Dashboard; Slow Moving & Dead Stock; Purchasing Management
Target Entity:   Mixed
Purpose:         Trace forecast to stock, risk, and purchasing.
```

```text
DrillDown ID:    DD-IN04-FOOTER
Source Widget:   Footer
Target Page:     Inventory Forecast; Inventory Risk; Purchasing Management; Inventory Report; Purchasing Report
Target Entity:   Mixed
Purpose:         Trace optimization actions to source dashboards and evidence.
```

```text
DrillDown ID:    DD-PU01-LIST
Source Widget:   Purchasing attention cards
Target Page:     Purchasing Attention List (same page)
Target Entity:   Supplier
Purpose:         Filter the list to the card’s signal.
```

```text
DrillDown ID:    DD-PU01-PROFILE
Source Widget:   Top 10 Principals / Principal Exposure Comparison / Purchasing Attention List
Target Page:     Supplier Performance Profile
Target Entity:   Supplier / Principal
Purpose:         Open the supplier profile.
```

```text
DrillDown ID:    DD-OP01-INV
Source Widget:   Top 10 Warehouse by Inventory Value
Target Page:     Inventory Report (typical investigation)
Target Entity:   Warehouse
Purpose:         Stock evidence for a warehouse.
```

```text
DrillDown ID:    DD-OP01-RISK
Source Widget:   Top 10 Warehouse by At-Risk Value
Target Page:     Slow Moving & Dead Stock Dashboard
Target Entity:   Warehouse / Item
Purpose:         Inventory risk behind location at-risk value.
```

```text
DrillDown ID:    DD-OP01-WIL
Source Widget:   Top 10 Wilayah by MTD Omzet
Target Page:     Collection Dashboard
Target Entity:   Wilayah
Purpose:         Receivable / collection view of a territory. (The locations page itself does not show receivable risk by wilayah.)
```

```text
DrillDown ID:    DD-PRF-WS
Source Widget:   Investigation Workspace (profile header)
Target Page:     Investigation Workspace
Target Entity:   Same entity
Purpose:         Population context for the profiled entity.
```

```text
DrillDown ID:    DD-PRF-COMPARE
Source Widget:   Compare {Entities} (profile header)
Target Page:     Compare page for that entity type
Target Entity:   Same type
Purpose:         Compare this entity with others.
```

```text
DrillDown ID:    DD-PRF-REL
Source Widget:   Related Entities
Target Page:     Related Performance Profile or Investigation Workspace
Target Entity:   Related Customer / Salesman / Supplier / Item
Purpose:         Follow a commercial relationship.
```

```text
DrillDown ID:    DD-PRF-EVD
Source Widget:   Evidence
Target Page:     Related report
Target Entity:   Supporting records
Purpose:         Show source rows for the profile.
```

---

# SECTION 5
# KPI LOCATION MAP

Each entry is a user-visible KPI (or closely named metric). Drill-Down is the typical next screen from the widget that shows it. “—” means the KPI is visible but that widget does not drill.

KPIs that appear on Entity Analytics profiles/workspace use live display names; they are not repeated one-by-one here.

---

### Sales

```text
Achievement %
  Menu: Executive / Sales / Sales Force
  Page: Management Attention Center; Sales Dashboard; Sales Forecast Dashboard (Current Achievement / Forecast Achievement)
  Widget: Sales; Achievement %; Current Achievement; Forecast Achievement
  Drill-Down: Sales Dashboard (from Executive); none on Sales Dashboard KPI tiles

Total Achievement / Current Sales / Forecast Sales
  Menu: Executive / Sales
  Page: Management Attention Center; Sales Dashboard; Sales Forecast Dashboard
  Widget: Sales; Total Achievement; Current Sales; Forecast Sales
  Drill-Down: Sales Dashboard (from Executive)

Total Target
  Menu: Sales
  Page: Sales Dashboard
  Widget: Total Target; Target vs Achievement
  Drill-Down: —

Daily Average Sales / Required Daily Sales / Target Gap / Days Remaining
  Menu: Sales
  Page: Sales Forecast Dashboard
  Widget: Pace and gap KPIs
  Drill-Down: —

Best Case / Expected / Worst Case / Forecast Confidence (sales)
  Menu: Sales
  Page: Sales Forecast Dashboard
  Widget: Scenario KPIs; Forecast Risk
  Drill-Down: Sales Report

Invoiced Omzet (ranking)
  Menu: Sales
  Page: Sales Dashboard
  Widget: Top 10 Salesman
  Drill-Down: Sales Report (investigation)
```

---

### Customer / Portfolio / Risk

```text
Portfolio Healthy % / Healthy Customers
  Menu: Executive / Customers
  Page: Management Attention Center; Customer Portfolio Dashboard
  Widget: Portfolio Healthy %; Health KPIs
  Drill-Down: Customer Portfolio Dashboard (from Executive)

Customers At Risk / At Risk Count / Attention Customers
  Menu: Executive / Customers
  Page: Management Attention Center; Customer Risk Forecast; Customer Portfolio
  Widget: Customers At Risk; Horizon / health KPIs; Health KPIs
  Drill-Down: Customer Portfolio Dashboard (from Executive)

Strategic At Risk / Strategic Customers
  Menu: Executive / Customers
  Page: Management Attention Center; Customer Portfolio Dashboard
  Widget: Strategic At Risk; Strategic KPIs
  Drill-Down: Customer Portfolio Dashboard (from Executive)

Overdue Customers
  Menu: Customers
  Page: Customer Analytics
  Widget: Collection
  Drill-Down: Piutang Dashboard

>90 Day Exposure
  Menu: Customers
  Page: Customer Analytics
  Widget: Collection
  Drill-Down: Piutang Dashboard

Top Omzet Customer %
  Menu: Customers
  Page: Customer Analytics
  Widget: Concentration
  Drill-Down: —

Top Piutang Customer %
  Menu: Customers
  Page: Customer Analytics
  Widget: Concentration
  Drill-Down: —

Active Customers (month)
  Menu: Customers
  Page: Customer Analytics
  Widget: Activity; Active vs Dormant
  Drill-Down: Sales Dashboard (from Activity card)

Dormant Customers (90-day)
  Menu: Customers
  Page: Customer Analytics; Customer Portfolio Dashboard
  Widget: Inactivity; Active vs Dormant; Lifecycle KPIs (Dormant)
  Drill-Down: Customer Attention List (from Inactivity)

Plafond Breach
  Menu: Customers
  Page: Customer Analytics
  Widget: Credit
  Drill-Down: Customer Attention List

Suspended + Sales
  Menu: Customers
  Page: Customer Analytics
  Widget: Credit
  Drill-Down: Customer Attention List

Horizon (days) / Portfolio Health Score / Forecast Confidence (customer risk)
  Menu: Customers
  Page: Customer Risk Forecast Dashboard
  Widget: Horizon / health KPIs
  Drill-Down: —

Elevated Risk Receivable / Elevated Risk %
  Menu: Customers
  Page: Customer Risk Forecast Dashboard
  Widget: Exposure KPIs; Elevated Risk vs Total Piutang
  Drill-Down: —

High / Critical Customers / Healthy / Watch / Attention / High Risk
  Menu: Customers
  Page: Customer Risk Forecast Dashboard
  Widget: Risk category counts; Risk Category Distribution
  Drill-Down: —

Payment Delay / Credit Limit / Inactivity / Purchase Decline
  Menu: Customers
  Page: Customer Risk Forecast Dashboard
  Widget: Signal family counts; Signal Family Mix
  Drill-Down: —

Collection Risk / Critical Category / High Risk Category / Customers Forecasted At Risk
  Menu: Customers
  Page: Customer Risk Forecast Dashboard
  Widget: Collection-risk KPIs
  Drill-Down: —

Working Capital Tied / Total Customers / Never Purchased / Declining / Total MTD Omzet
  Menu: Customers
  Page: Customer Portfolio Dashboard
  Widget: Strategic KPIs; Lifecycle KPIs
  Drill-Down: —

Actions Today / Immediate Collection / Proactive Reminders / Credit Review / Sales Recovery / Collection Impact
  Menu: Customers
  Page: Collection Optimization Dashboard
  Widget: Workload KPIs
  Drill-Down: —

Due Within 7 Days / Planning Confidence
  Menu: Customers
  Page: Collection Optimization Dashboard
  Widget: Context KPIs
  Drill-Down: —
```

---

### Finance / Collection / Cash

```text
Total Piutang
  Menu: Executive / Finance / Customers
  Page: Management Attention Center; Piutang Dashboard; Customer Risk Forecast; Piutang Report
  Widget: Piutang; Total Piutang; Exposure KPIs; Report totals
  Drill-Down: Piutang Dashboard (from Executive)

Overdue Customer
  Menu: Executive / Finance / Customers
  Page: Management Attention Center; Piutang Dashboard; Customer Analytics
  Widget: Piutang; Overdue Customer; Collection (Overdue Customers)
  Drill-Down: Piutang Dashboard (from Executive / Customer Collection card)

> 90 Day Amount / Piutang > 90 Hari / >90d Exposure
  Menu: Executive / Finance / Customers
  Page: Management Attention Center; Piutang Dashboard; Customer Analytics; Collection Dashboard
  Widget: Piutang; Piutang > 90 Hari; Collection; Exposure
  Drill-Down: Piutang Dashboard or Collection Attention List

Top Customer % / Top 10 Customer % / Top 20 Customer %
  Menu: Executive / Finance
  Page: Management Attention Center; Piutang Dashboard
  Widget: Piutang; Top 10 Customer %; Top 20 Customer %
  Drill-Down: Piutang Dashboard (from Executive)

Overdue Piutang / Overdue Exposure / Overdue Outstanding
  Menu: Finance / Customers
  Page: Piutang Dashboard; Collection Dashboard; Collection Optimization; Cash Flow Forecast
  Widget: Overdue Piutang; Exposure; Context KPIs; Receivable context KPIs
  Drill-Down: Collection Attention List (from Collection Exposure card)

Overdue Concentration %
  Menu: Finance
  Page: Collection Dashboard
  Widget: Exposure
  Drill-Down: Collection Attention List

Cash Collected MTD
  Menu: Finance
  Page: Collection Dashboard; Cash Flow Forecast Dashboard
  Widget: Recovery; Recovery Summary; Cash position KPIs
  Drill-Down: Collection Attention List (from Recovery card)

Recovery vs Billing % / Recovery vs Billing (Actual) / Recovery vs Billing Forecast
  Menu: Finance / Customers
  Page: Collection Dashboard; Cash Flow Forecast; Collection Optimization
  Widget: Recovery; Recovery Summary; Recovery scenario KPIs; Context KPIs
  Drill-Down: Collection Attention List (from Recovery card)

Payment Mix (Cash / Giro / Adjustment)
  Menu: Finance
  Page: Collection Dashboard
  Widget: Recovery Summary
  Drill-Down: —

Legacy Debt Count
  Menu: Finance
  Page: Collection Dashboard
  Widget: Portfolio
  Drill-Down: Collection Attention List

Expected Cash Collection / Projected Month-End Collection / Collection Forecast %
  Menu: Finance
  Page: Cash Flow Forecast Dashboard
  Widget: Cash position KPIs; Cash Forecast vs Billing
  Drill-Down: Piutang Report / Collection Dashboard (footer)

Daily Cash Collection Average / Required Daily Collection / Remaining Collection Target / Remaining Calendar Days
  Menu: Finance
  Page: Cash Flow Forecast Dashboard
  Widget: Pace KPIs
  Drill-Down: —

Best / Exp / Worst Cash / Forecast Confidence (cash) / Collection Gap / Forecast Variance (Cash) / Outstanding Due Remaining
  Menu: Finance
  Page: Cash Flow Forecast Dashboard
  Widget: Recovery scenario KPIs; Receivable context KPIs
  Drill-Down: —
```

---

### Sales Force / Field

```text
Below Target / Missing Target Setup
  Menu: Sales Force
  Page: Salesman Performance
  Widget: Performance
  Drill-Down: Sales Dashboard

High Overdue Exposure / High Piutang Exposure
  Menu: Sales Force
  Page: Salesman Performance
  Widget: Collection Exposure
  Drill-Down: Piutang Dashboard

Dormant Portfolio / Top Omzet Salesman % / Top Piutang Salesman %
  Menu: Sales Force
  Page: Salesman Performance
  Widget: Portfolio
  Drill-Down: Salesman Attention List

Active Salesmen / Planned / Actual / Execution % / Effective Calls / Effective Call Rate / Orders / Order Value / GPS Valid Rate / Missed Visit / Unplanned Visit
  Menu: Sales Force
  Page: Sales Force Overview; Salesman Field Activity (subset: Planned, Actual, Effective, Missed, Unplanned, Execution %, Effective Call Rate)
  Widget: Field Execution KPI strip; individual field KPI cards
  Drill-Down: Salesman Field Activity (from Overview)
```

---

### Inventory / Purchasing / Locations

```text
Total Inventory Value / Total Item
  Menu: Executive / Inventory
  Page: Management Attention Center; Inventory Dashboard; Inventory Report; Inventory Forecast (Current Inventory Value)
  Widget: Inventory; Total Inventory Value; Total Item; Report totals
  Drill-Down: Inventory Dashboard (from Executive)

Top Category % / Top Supplier %
  Menu: Executive
  Page: Management Attention Center
  Widget: Inventory
  Drill-Down: Inventory Dashboard

Dead Stock Item Count / Dead Stock Value
  Menu: Executive / Inventory
  Page: Alert Center; Slow Moving & Dead Stock Dashboard
  Widget: Inventory Risk Summary; Dead Stock KPI cards
  Drill-Down: Slow Moving & Dead Stock Dashboard; Inventory Attention List

Slow Moving Item Count / Slow Moving Value
  Menu: Executive / Inventory
  Page: Alert Center; Slow Moving & Dead Stock Dashboard
  Widget: Inventory Risk Summary; Slow Moving KPI cards
  Drill-Down: Slow Moving & Dead Stock Dashboard; Inventory Attention List

Never Sold
  Menu: Executive
  Page: Alert Center
  Widget: Inventory Risk Summary
  Drill-Down: Slow Moving & Dead Stock Dashboard

At-Risk Inventory % / At-Risk Inventory
  Menu: Inventory
  Page: Slow Moving & Dead Stock; Inventory Forecast; Alert Center
  Widget: At-Risk Inventory %; Inventory Risk; Risk exposure KPIs
  Drill-Down: Inventory Attention List / Inventory Risk dashboard

Projected Inventory Value @ H / Avg Days of Supply / Inventory Health Score
  Menu: Inventory
  Page: Inventory Forecast; Inventory Optimization (health score)
  Widget: Position KPIs; Health / budget KPIs
  Drill-Down: —

Stock-Out Risk Items / Overstock Value / Understock Value
  Menu: Inventory
  Page: Inventory Forecast Dashboard
  Widget: Risk exposure KPIs
  Drill-Down: —

Best Case Projected / Expected Projected / Worst Case Projected / Forecast Confidence (inventory)
  Menu: Inventory
  Page: Inventory Forecast Dashboard
  Widget: Scenario KPIs
  Drill-Down: —

Avg Daily Consumption (units) / Forecast Consumption @ H / Inventory Coverage % / Turnover Forecast
  Menu: Inventory
  Page: Inventory Forecast Dashboard
  Widget: Consumption KPIs
  Drill-Down: —

Critical Actions / Recommended Purchase Budget / Deferrable Spend / Purchase Now / Delay / Transfer / Clearance Review
  Menu: Inventory
  Page: Inventory Optimization Dashboard
  Widget: Health / budget KPIs; Action mix KPIs
  Drill-Down: —

Pending Posting / Pending Posting Value (all BELUM) / Qualified Backlog / Posted %
  Menu: Executive / Purchasing
  Page: Management Attention Center; Purchasing Management Dashboard
  Widget: Purchasing; Purchasing Summary
  Drill-Down: Purchasing Management Dashboard (from Executive); Purchasing Attention List (from posting card)

Top Principal %
  Menu: Executive
  Page: Management Attention Center
  Widget: Purchasing
  Drill-Down: Purchasing Management Dashboard

Grand Total Purchase / Total Invoice
  Menu: Purchasing
  Page: Purchasing Management Dashboard; Purchasing Report
  Widget: Purchasing Summary; Report totals
  Drill-Down: —

Top Warehouse Inventory % / Top 3 Warehouse Inventory % / Top Warehouse At-Risk % / Top Warehouse Sales % / Top Wilayah Sales %
  Menu: Operations
  Page: Branch / Warehouse Performance Dashboard
  Widget: Inventory Concentration; At-Risk Concentration; Sales Concentration
  Drill-Down: —

Inactive Warehouse With Stock / Stock Without Sales
  Menu: Operations
  Page: Branch / Warehouse Performance Dashboard
  Widget: Operational Signals
  Drill-Down: —
```

---

# SECTION 6
# NAVIGATION GRAPH

```text
Executive
 ├─ Executive  →  Management Attention Center
 │   ├─ Sales  →  Sales Dashboard
 │   ├─ Piutang  →  Piutang Dashboard
 │   ├─ Purchasing  →  Purchasing Management Dashboard
 │   ├─ Inventory  →  Inventory Dashboard
 │   ├─ Portfolio Healthy % / Customers At Risk / Strategic At Risk  →  Customer Portfolio Dashboard
 │   ├─ Top 5 Customers / Categories / Suppliers / Principals  →  related Report (investigation)
 │   ├─ Domain Summaries  →  named domain dashboard
 │   └─ Open Alert Center  →  Alert Center
 │
 ├─ Alert Center
 │   ├─ Category Attention / Top Critical Alerts / Alerts by Category
 │   │   ├─ Investigate  →  related Report
 │   │   └─ View Dashboard  →  named domain dashboard
 │   ├─ Inventory Risk Summary  →  Slow Moving & Dead Stock Dashboard
 │   ├─ Concentrations  →  named domain dashboard
 │   └─ Domain Dashboards  →  named domain dashboard
 │
 └─ Entity Analytics
     ├─ {Entity} Investigation  →  Investigation Workspace
     │   ├─ Population Map
     │   ├─ Current Facts
     │   ├─ Context
     │   ├─ Explanation
     │   └─ Validation
     ├─ Open Profile  →  Performance Profile (Customer / Salesman / Supplier / Item)
     │   ├─ Overview
     │   ├─ KPI Summary
     │   ├─ Comparison
     │   ├─ Trend
     │   ├─ Performance Signature
     │   ├─ Ranking History
     │   ├─ Attention History
     │   ├─ Related Entities  →  related Profile / Workspace
     │   └─ Evidence  →  related Report
     └─ Compare {Entities}  →  Compare Customers / Salesmen / Suppliers / Items
         ├─ KPI Comparison
         ├─ Trend Comparison
         ├─ Ranking Comparison
         ├─ Attention Comparison
         ├─ Relationship Comparison
         └─ Performance Signature

Sales
 ├─ Sales  →  Sales Dashboard
 │   ├─ Total Target
 │   ├─ Total Achievement
 │   ├─ Achievement %
 │   ├─ Target vs Achievement
 │   ├─ Weekly Trend
 │   └─ Top 10 Salesman  →  Sales Report (investigation)
 ├─ Sales Forecast  →  Sales Forecast Dashboard
 │   ├─ Current vs Forecast KPIs
 │   ├─ Pace and gap KPIs
 │   ├─ Scenario KPIs
 │   ├─ Daily Pace Trend
 │   ├─ Forecast vs Target
 │   ├─ Weekly Pace
 │   └─ Forecast Risk  →  Sales Report
 └─ Sales Report  →  Faktur list

Customers
 ├─ Customers  →  Customer Analytics
 │   ├─ Collection  →  Piutang Dashboard
 │   ├─ Concentration
 │   ├─ Activity  →  Sales Dashboard
 │   ├─ Inactivity  →  Customer Attention List
 │   ├─ Credit  →  Customer Attention List
 │   ├─ Customer Attention List  →  Customer Performance Profile / Report
 │   ├─ Top 10 by Omzet  →  Customer Performance Profile
 │   ├─ Top 10 by Piutang  →  Customer Performance Profile
 │   └─ Segmentation Summary
 ├─ Customer Risk Forecast  →  Customer Risk Forecast Dashboard
 │   ├─ Risk KPIs and charts
 │   ├─ Top Customers by Risk Priority
 │   ├─ Customer Risk Attention List
 │   └─ Top Recommended Actions
 ├─ Collection Optimization  →  Collection Optimization Dashboard
 │   ├─ Workload / context KPIs
 │   ├─ Actions by Category / Workload / Impact charts
 │   ├─ Today's Collection Priorities
 │   ├─ Specialized Queues
 │   └─ Top Impact Opportunities
 ├─ Customer Portfolio  →  Customer Portfolio Dashboard
 │   ├─ Health / strategic / lifecycle KPIs
 │   ├─ Lifecycle Distribution / Tier Distribution
 │   ├─ Portfolio Priority Queue  →  Customer Performance Profile / Customer Report
 │   ├─ Customers by Portfolio Action
 │   ├─ Top 10 MTD Omzet Concentration
 │   └─ Top 10 Open Piutang Concentration
 └─ Customer Report  →  Customer rows

Finance
 ├─ Piutang  →  Piutang Dashboard
 │   ├─ Total Piutang / Total Customer / Overdue Customer / Overdue Piutang / Piutang > 90 Hari
 │   ├─ Top 10 / Top 20 Customer %
 │   ├─ Aging Distribution
 │   └─ Top 20 Outstanding Customers  →  Piutang Report (investigation)
 ├─ Collection  →  Collection Dashboard
 │   ├─ Exposure / Recovery / Portfolio  →  Collection Attention List
 │   ├─ Recovery Summary
 │   ├─ Aging Risk Summary (Overdue Only)
 │   ├─ Collection Attention List  →  related Report
 │   ├─ Top 10 Overdue Customers  →  related Report
 │   ├─ Top 10 Overdue Salesmen  →  related Report
 │   └─ Top 10 Overdue Wilayah  (no row drill-down)
 ├─ Cash Flow Forecast  →  Cash Flow Forecast Dashboard
 │   ├─ Cash / pace / recovery / receivable KPIs
 │   ├─ Daily Collection Pace
 │   ├─ Cash Forecast vs Billing
 │   ├─ Recovery Trend
 │   └─ Top Collection Risks
 └─ Piutang Report  →  Open receivable rows

Sales Force
 ├─ Salesmen  →  Salesman Performance
 │   ├─ Performance  →  Sales Dashboard
 │   ├─ Collection Exposure  →  Piutang Dashboard
 │   ├─ Portfolio  →  Salesman Attention List
 │   ├─ Salesman Attention List  →  Salesman Performance Profile or Salesman Detail drawer
 │   ├─ Top 10 Omzet / Achievement % / Piutang  →  Salesman Performance Profile
 │   ├─ Segmentation Summary
 │   └─ Salesman Detail  (Principal Achievement, Trend)
 ├─ Sales Force Overview
 │   ├─ Field Execution KPIs
 │   ├─ Salesman Performance table  →  Salesman Field Activity
 │   ├─ Visit Execution % / Effective Call Rate / Orders Generated / Order Value  →  Salesman Field Activity
 │   ├─ Performance Rankings  →  Salesman Field Activity
 │   ├─ Team Execution Trends
 │   └─ Visits by Wilayah
 └─ Salesman Field Activity
     ├─ Planned / Actual / Effective / Missed / Unplanned / Execution % / Effective Call Rate
     ├─ Field Activity Map
     ├─ Missed Visits
     ├─ Visit Timeline
     ├─ → Sales Force Overview
     └─ → Salesman Performance

Inventory
 ├─ Inventory  →  Inventory Dashboard
 │   ├─ Total Inventory Value / Total Item
 │   ├─ Inventory by Category / Inventory by Supplier
 │   └─ Top 10 Categories / Top 10 Suppliers  →  Inventory Report (investigation)
 ├─ Inventory Risk  →  Slow Moving & Dead Stock Dashboard
 │   ├─ Dead Stock / Slow Moving / At-Risk KPIs  →  Inventory Attention List
 │   ├─ Inventory Aging Distribution
 │   ├─ Category / Supplier Risk Exposure
 │   ├─ Inventory Attention List  →  Item Performance Profile
 │   └─ Top 10 Dead Stock / Slow Moving  →  Item Performance Profile
 ├─ Inventory Forecast  →  Inventory Forecast Dashboard
 │   ├─ Position / risk / scenario / consumption KPIs
 │   ├─ Forecast Inventory Level / Consumption Trend / Risk Heat Summary
 │   ├─ Top Inventory Risks
 │   └─ Purchasing Recommendations
 ├─ Inventory Optimization  →  Inventory Optimization Dashboard
 │   ├─ Health / budget / action-mix KPIs
 │   ├─ Priority / impact / heat charts
 │   ├─ Top Optimization Actions
 │   ├─ Recommended Reorder List
 │   ├─ Warehouse Rebalancing
 │   ├─ Overstock & Delay Purchasing
 │   └─ Dead Stock Recovery
 └─ Inventory Report  →  Stock rows

Purchasing
 ├─ Purchasing  →  Purchasing Management Dashboard
 │   ├─ Posting Exposure / Principal Dependency / Purchasing Pace / Inventory Cross-Risk  →  Purchasing Attention List
 │   ├─ Purchasing Summary
 │   ├─ Purchasing Attention List  →  Supplier Performance Profile
 │   ├─ Weekly Purchase Trend
 │   ├─ Posting Status Breakdown
 │   ├─ Top 10 Principals  →  Supplier Performance Profile
 │   └─ Principal Exposure Comparison  →  Supplier Performance Profile
 └─ Purchasing Report  →  Purchase invoice rows

Operations
 └─ Locations  →  Branch / Warehouse Performance Dashboard
     ├─ Inventory / At-Risk / Sales Concentration / Operational Signals
     ├─ Location Attention List  →  related Report (investigation)
     ├─ Top 10 Warehouse by Inventory Value  →  Inventory Report
     ├─ Top 10 Warehouse by At-Risk Value  →  Slow Moving & Dead Stock Dashboard
     ├─ Top 10 Warehouse by MTD Omzet
     ├─ Top 10 Warehouse by MTD Purchase
     └─ Top 10 Wilayah by MTD Omzet  →  Collection Dashboard
```

---

# SECTION 7
# DISCOVERY COVERAGE

```text
Total Menus (sidebar groups)                         8
Total Sidebar Items                                  25
Total Pages (sidebar + profile/compare/workspace)    34
  Sidebar-linked pages                               25
  Investigation Workspace                            1
  Performance Profiles                               4
  Compare pages                                      4
Total Widgets (named blocks in Section 3)            245
Total KPI Cards                                      66
Total Charts                                         26
Total Tables                                         54
Total Rankings                                       37
Total Distributions                                  14
Total Trends                                         11
Total Forecast widgets                               9
Total Risk Indicators                                28
Total Drill-Down Targets (distinct page families)    18
```

**Distinct drill-down target families**

1. Management Attention Center  
2. Alert Center  
3. Domain dashboards (Sales, Piutang, Purchasing, Inventory, Collection, Customer Analytics, Customer Portfolio, Customer Risk Forecast, Cash Flow Forecast, Inventory Risk, Inventory Forecast, Salesman Performance, Sales Force Overview, Salesman Field Activity)  
4. Sales Report  
5. Piutang Report  
6. Inventory Report  
7. Purchasing Report  
8. Customer Report  
9. Investigation Workspace  
10. Customer Performance Profile  
11. Salesman Performance Profile  
12. Supplier Performance Profile  
13. Item Performance Profile  
14. Compare Customers  
15. Compare Salesmen  
16. Compare Suppliers  
17. Compare Items  
18. Salesman Detail drawer (same page)

Counts match named widgets in Section 3 (one row = one widget). A KPI strip registered as one row counts as one KPI Card. Use Section 3 as the authoritative list.

---

# GAP NOTES

Findings only. No changes proposed.

```text
Sidebar label ≠ page title
  Executive → Management Attention Center
  Customers → Customer Analytics
  Salesmen → Salesman Performance
  Inventory Risk → Slow Moving & Dead Stock Dashboard
  Purchasing → Purchasing Management Dashboard
  Locations → Branch / Warehouse Performance Dashboard
  Investigation source label for the executive page is “Executive Dashboard”, which is not the on-screen title.

Pages exist but are not sidebar items
  Investigation Workspace
  Customer / Salesman / Supplier / Item Performance Profile
  Compare Customers / Salesmen / Suppliers / Items
  Salesman Detail drawer

KPI appears on multiple pages
  Achievement %, Total Piutang, Overdue Customer, Cash Collected MTD, Recovery vs Billing %,
  Total Inventory Value, Dead Stock / Slow Moving, Portfolio Healthy %, Customers At Risk,
  Forecast Confidence (used on several forecast pages with different meanings).

Duplicate / parallel widgets
  Top 10 by Omzet and Top 10 MTD Omzet Concentration (Customer Analytics vs Customer Portfolio)
  Top 10 by Piutang and Top 10 Open Piutang Concentration
  Aging Distribution (Piutang Dashboard) vs Aging Risk Summary Overdue Only (Collection Dashboard)
  Weekly Trend (Sales) vs Weekly Pace (Sales Forecast) — same chart family, different page
  Planned / Actual / Execution % / Effective Call Rate on both Sales Force Overview and Salesman Field Activity

Widget exists but no row drill-down
  Customer Analytics — Concentration card
  Sales Dashboard — KPI tiles and charts (only Top 10 Salesman drills)
  Piutang Dashboard — KPI tiles and Aging Distribution (only Top 20 table drills)
  Inventory Dashboard — KPI tiles and category/supplier charts (only Top 10 tables drill)
  Collection Dashboard — Top 10 Overdue Wilayah (explicitly not click-investigated)
  Sales Force Overview — Team Execution Trends; Visits by Wilayah
  Many forecast KPI tiles and charts (navigation is footer links, not widget clicks)

Inconsistent ranking destinations
  Sales Top 10 Salesman → investigation/report, not Salesman Performance Profile
  Piutang Top 20 Outstanding Customers → investigation/report, not Customer Performance Profile
  Customer Analytics rankings prefer Performance Profile
  Salesman Performance rankings prefer Performance Profile
  Inventory Risk rankings prefer Performance Profile

Same-page “drill-down”
  Several attention cards only scroll/filter a list on the same page (Customer, Collection, Inventory Risk, Purchasing, Salesman).

Investigation target is not a fixed page
  The report opened depends on the investigation attached to the row. Analysts should treat “related Report” as Sales, Piutang, Inventory, Purchasing, or Customer Report.

Purchasing attention card metric labels are live data
  Posting Exposure, Principal Dependency, Purchasing Pace, and Inventory Cross-Risk show metric names supplied with the data, not a fixed on-screen dictionary.

Entity Analytics KPI names are live data
  Profile KPI Summary and Workspace Current Facts use KPI display names from the snapshot, not a fixed widget list.

Broken or mismatched fallback path observed
  Inventory Optimization footer fallback for Purchasing Management uses a path that is not a sidebar page
  (/dashboard/purchasing-management). The actual Purchasing page is Purchasing Management Dashboard
  under menu Purchasing. When live traceability is present it may still point to the real page.

Domain documentation vs portal
  Older domain text still says “Customer Analytics Dashboard”, “Slow Moving & Dead Stock Dashboard”
  as a business-area name, “Management Attention Center”, and “Purchasing Management Dashboard”.
  This registry follows on-screen titles. Trust this file over older aliases when mapping questions in Phase-4B.

Presentation Mode
  Header overlay (business date) on authenticated pages. Not a navigation asset.

Refresh / Last Refreshed / snapshot health banners
  Present on many dashboards. Operational chrome, not navigation widgets.
```

---

# HOW TO USE THIS FILE IN PHASE-4B

1. Start from a business question.  
2. Find the KPI in Section 5.  
3. Open the Page in Section 2 and the Widget in Section 3.  
4. Follow Drill-Down in Section 4 if the owner needs evidence or an entity.  
5. Do not assume a dashboard or drill-down that is not listed here.
