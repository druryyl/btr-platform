# BTR Portal — Operational Guide

> **Table naming:** Portal snapshot tables use the `BTRPD_*` prefix (formerly `BTR_PortalDashboard*`).

**Audience:** End Users, Trainers, Support Team  
**Purpose:** Explain how to use BTR Portal day to day.

**Related permanent docs:** [Domain (WHY)](./btr-portal-domain.md) · [Architecture (WHAT)](./btr-portal-architecture.md) · [Materialized dashboard ops](../materialized-dashboard/materialized-dashboard-operational.md) · [Extraction — M16/M17](./knowledge-extraction-report-m16-m17.md) · [Extraction — M18](./knowledge-extraction-report-m18.md) · [Extraction — M19](./knowledge-extraction-report-m19.md) · [Extraction — M21](./knowledge-extraction-report-m21.md) · [Extraction — Purchasing V1](./knowledge-extraction-report-purchasing-dashboard.md)

For business definitions and KPI formulas, see **[btr-portal-kpi-catalog.md](./btr-portal-kpi-catalog.md)** (authoritative SSOT). Domain concept summary: [btr-portal-domain.md](./btr-portal-domain.md) §7.

---

## Login

1. Open the BTR Portal URL in a web browser.
2. Enter your **User ID** and **Password** (same credentials as BTR Desktop).
3. Click **Login**.
4. On success, you are redirected to the **Management Attention Center** at `/dashboard`.
5. Your session persists across browser refreshes until you log out or the token expires.
6. If you access a protected page without signing in, you are redirected to Login and returned to your original page after authentication.
7. Click **Logout** in the header to end your session.

**Support note:** Invalid credentials show an error on the login page. A 401 response from the API (expired token) clears the session and returns you to Login.

---

## Presentation Mode (Demonstrations)

For management demos, proposal screenshots, or executive reviews, an administrator can enable **Presentation Mode** in API configuration. When enabled:

- Platform diagnostics (freshness banners, worker warnings, Last Refreshed timestamps, Platform Alerts) are hidden.
- Business KPIs and alerts remain visible.
- Optional **Business Date** aligns dashboard aging, MTD metrics, and report defaults with a restored snapshot date (no OS clock change).
- A header badge shows `Presentation Mode` and the active business date.

**Demo checklist:** Set `Presentation.Enabled = true` and `Presentation.BusinessDate` to the snapshot date → recycle API app pool → re-run the portal snapshot worker → verify the header badge and executive dashboard → revert after the session.

See [presentation-mode/feature.md](./presentation-mode/feature.md).

---

## Management Attention Center (Executive Dashboard)

The Dashboard has two levels:

| Level | Route | What You See |
| ----- | ----- | ------------ |
| **Executive (Home)** | `/dashboard` | **Management Attention Center** — attention-oriented KPIs across Sales, Piutang, Inventory, and Purchasing; Top 5 exposure lists; domain summaries. Links go to domain dashboards only. |
| **Detail** | `/dashboard/sales`, `/dashboard/sales-forecast`, `/dashboard/piutang`, `/dashboard/customers`, `/dashboard/customer-risk-forecast`, `/dashboard/collection-optimization`, `/dashboard/salesmen`, `/dashboard/inventory`, `/dashboard/inventory-risk`, `/dashboard/inventory-forecast`, `/dashboard/purchasing`, `/dashboard/collection`, `/dashboard/cash-flow-forecast` | Full KPI row, charts, and Top 10 tables for that business area (Customer, Salesman, and Inventory Risk use attention-oriented layout). |

**Navigate:** Sidebar → Executive → **EX01 Executive** (default home). Use **Open Alert Center** on the executive page or Sidebar → **EX02 Alert Center** for the company-wide exception feed.

**Daily review question:** *What requires management attention today?*

**Related:** For cross-domain exception review, use **Alert Center** at `/alerts` (M23).

### Executive Page Sections

1. **Attention Cards** — Sales (Achievement % band), Piutang (overdue, > 90 day exposure), Purchasing (pending posting), Inventory (value and concentration).
2. **Critical Exposure Lists** — Top 5 Customers, Categories, Suppliers, Principals.
3. **Domain Summaries** — Compact summary with link to each domain dashboard.

**Navigation path:** Executive → Domain Dashboard → Report. No direct report links on the executive page. For customer-specific attention, use **Dashboard → Customers** (M17).

### Sales Achievement Bands

| Achievement % | Band |
| ------------- | ---- |
| ≥ 100% | Healthy |
| 80–99% | Warning |
| < 80% | Critical |
| No target | Unknown |

### Freshness

- **Last Refreshed** — Single timestamp in header (oldest domain snapshot on screen).
- **⚠ Dashboard Data Not Fresh** — When any domain exceeds its refresh interval.

Use **Refresh** on any dashboard page to reload data from the server. Detail pages show a **generated-at** timestamp per domain.

**Unavailable data:** If the executive page shows a warning that data is not yet available, snapshot tables have not been populated. An administrator must run the snapshot worker before dashboards will load.

## Sales Dashboard

**Navigate:** Sidebar → Dashboard → Sales, or click the Sales attention card on the Management Attention Center.

**Route:** `/dashboard/sales`

### What You See

1. **KPI row** — Total Target, Total Achievement, Achievement %
2. **Target vs Achievement chart** — Company-level bar comparing monthly target to invoiced omzet
3. **Weekly Invoiced Sales Trend** — Line chart of Faktur totals by week within the current month
4. **Top 10 Salesman** — Table ranked by invoiced omzet (highest first)

### How to Read It

- **Total Target** is the sum of all salesperson monthly targets set in BTR.
- **Total Achievement** is invoiced omzet (`GrandTotal` on Fakturs) for the current calendar month.
- **Achievement %** shows progress toward target; it is blank when no targets are set.
- Rankings show at most 10 salespeople.

### Typical Use

- Morning management review of monthly sales progress
- Identifying top performers for the current month
- Checking whether weekly omzet trend is accelerating or slowing

---

## Sales Forecast Dashboard (M26)

**Navigate:** Sidebar → Dashboard → Sales Forecast.

**Route:** `/dashboard/sales-forecast`

**Question answered:** If current billing pace continues, where will invoiced sales finish at month-end?

### What You See

1. **Executive summary** — plain-language forecast sentence (server-computed)
2. **KPI row — Actual vs Forecast** — Current Sales, Current Achievement, Forecast Sales, Forecast Achievement
3. **KPI row — Pace & Gap** — Daily Average, Required Daily Sales, Target Gap, Days Remaining
4. **KPI row — Scenario & Confidence** — Best Case, Expected, Worst Case, Forecast Confidence
5. **Daily Pace Trend** — Bar chart of daily invoiced omzet with MTD daily-average reference line
6. **Forecast vs Target** — Three-bar comparison (Target, Current, Forecast)
7. **Weekly Pace** — Same weekly trend as Sales Dashboard (context for momentum)
8. **Forecast Risk card** — Healthy / Warning / Critical band on projected achievement
9. **Traceability footer** — link to Sales Report for Faktur evidence

### How to Read It

- Uses the **same Faktur data and monthly target** as the Sales Dashboard.
- **Forecast Sales** = (MTD sales ÷ days elapsed) × days in month (calendar-day linear extrapolation).
- **Required Daily Sales** = average daily billing needed on remaining days to hit target.
- **Forecast Confidence** is Low early in the month; becomes High after day 21.
- Refreshes with the existing **Sales** snapshot worker (~30 minutes).

### Typical Use

- Mid-month review: will the team hit target at current pace?
- Identifying when required daily billing exceeds recent daily average
- Finance cross-check before month-end close

**Related:** [Sales Forecast feature doc](../sales-forecast/feature.md)

---

## Cash Flow Forecast Dashboard (M27)

**Navigate:** Sidebar → Dashboard → Cash Flow Forecast.

**Route:** `/dashboard/cash-flow-forecast`

**Question answered:** If current collection pace continues, how much cash will we likely receive by month-end?

### What You See

1. **Executive summary** — plain-language liquidity forecast (server-computed)
2. **KPI row — Cash Position** — Cash Collected MTD, Expected Cash, Projected Month-End, Collection Forecast %
3. **KPI row — Pace & Target** — Daily Cash Average, Required Daily Collection, Remaining Target, Days Remaining
4. **KPI row — Recovery & Scenarios** — Recovery vs Billing (actual/forecast), Best/Exp/Worst cash, Confidence
5. **KPI row — Receivable Context** — Outstanding Due Remaining, Overdue, Collection Gap, Forecast Variance
6. **Daily Collection Pace** — Bar chart of daily cash with MTD average reference line
7. **Cash Forecast vs Billing** — Billing, Cash MTD, Projected Cash comparison
8. **Recovery Trend** — Cumulative collections vs cumulative billing
9. **Top Collection Risks** — Priority-ordered forecast risk table (max 10)
10. **Traceability footer** — links to Collection Dashboard and Piutang Report

### How to Read It

- Uses the **same pelunasan and billing data** as Collection and Sales dashboards.
- **Expected Cash Collection** = (Cash MTD ÷ days elapsed) × days in month.
- **Collection Forecast %** = projected total collections ÷ month billing × 100.
- Distinguishes **cash** (BayarTunai) from **total collections** (cash + giro).
- Refreshes with the existing **Collection** snapshot worker (~30 minutes).

### Typical Use

- Mid-month liquidity review before cash shortfall
- Collection management: required daily pace vs actual daily cash average
- Finance validation of projected recovery vs current-month billing

**Related:** [Cash Flow Forecast feature doc](../cash-flow-forecast/feature.md)

---

## Customer Risk Forecast Dashboard (M29)

**Navigate:** Sidebar → Dashboard → Customer Risk Forecast.

**Route:** `/dashboard/customer-risk-forecast`

**Question answered:** Which customers are likely to become collection or relationship risks within the next 30 days?

### What You See

1. **Executive summary** — plain-language portfolio forecast (server-computed)
2. **KPI row — Portfolio** — Customers at risk, elevated-risk receivable, portfolio health, forecast confidence
3. **KPI row — Signal mix** — Payment delay, credit limit, inactivity, purchase decline, collection risk counts
4. **Risk distribution chart** — Healthy / Watch / Attention / High Risk / Critical
5. **Risk by Wilayah** — Top territories by elevated-risk customer count
6. **Signal mix chart** — Forecast signal families
7. **Elevated risk vs total piutang** — Monetary exposure comparison
8. **Top risk customers** — Top 20 by priority score with category, reason, recommendation
9. **Forecast attention list** — Top 25 forward-looking signals
10. **Recommended actions** — Top 15 rule-triggered recommendations
11. **Traceability footer** — links to Customer Analytics, Piutang, Collection, Cash Flow Forecast, reports; indicative forecast disclaimer

### How to Read It

- Uses **deterministic business rules** — every row includes rule traceability (CRF-*).
- **Projected plafond** is an indicative upper bound if billing continues at current pace — not an automatic credit hold.
- Refreshes with the existing **Customer** snapshot worker (~30 minutes) alongside Customer Analytics.

### Typical Use

- Early collection planning before due dates
- Credit review before projected plafond breach
- Sales recovery on declining or approaching-dormant accounts
- Portfolio-level preventive resource allocation

**Related:** [Customer Risk Forecast feature doc](../customer-risk-forecast/feature.md)

---

## Collection Optimization Dashboard (M30)

**Navigate:** Sidebar → Dashboard → Collection Optimization.

**Route:** `/dashboard/collection-optimization`

**Question answered:** Given customer risk forecasts and today's receivables, which customers should Finance and Sales contact first today?

### What You See

1. **Executive summary** — daily collection priorities (server-computed)
2. **KPI row — Workload** — Actions today, immediate collection, proactive reminders, credit review, sales recovery, collection impact
3. **KPI row — Context** — Overdue exposure, due within 7 days, recovery vs billing %, planning confidence
4. **Action category chart** — Donut distribution
5. **Workload chart** — Wilayah or Salesman action counts
6. **Impact chart** — Impact by action category
7. **Priority queue** — Top 30 with expandable explainability (reason, rules, M29 traceability)
8. **Specialized queues** — Proactive reminders, credit review, sales recovery, management escalation
9. **Top impact opportunities** — Top 15 by collection impact amount
10. **Traceability footer** — links to Customer Risk Forecast, Collection, Customer Analytics, Piutang, reports; recommendation disclaimer

### How to Read It

- Consumes **M29 forecast outputs** — does not recalculate forecast rules.
- **Recovery vs billing %** copied from M20 collection snapshot at refresh (may lag up to ~30 min).
- **Sales recovery** rows route to Sales when overdue is below configured floor (default Rp 500K).
- Refreshes with **Customer** snapshot worker (~30 minutes) after M29 forecast step.

### Typical Use

- Morning collection planning — prioritized contact list for Finance
- Proactive reminders before due date for at-risk payers
- Credit review queue separated from collection follow-up
- Sales recovery visits when purchase decline dominates over collection urgency

**Related:** [Collection Optimization feature doc](../collection-optimization/feature.md)

---

## Customer Portfolio Optimization Dashboard (M31)

**Navigate:** Sidebar → Dashboard → Customer Portfolio.

**Route:** `/dashboard/customer-portfolio`

**Question answered:** What should Management do with each customer — grow, retain, protect, collect, recover, or exit review — across the full portfolio?

### What You See

1. **Executive summary** — plain-language portfolio brief (server-computed)
2. **KPI row** — Portfolio health %, strategic at risk, working capital tied, attention customer count
3. **Lifecycle / tier / action distribution charts**
4. **Priority portfolio queue** — default Attention Customers (top 50 by portfolio priority score)
5. **Action segments** — expandable lists by portfolio action
6. **Concentration tables** — Top 10 omzet and piutang (from M17 snapshot)
7. **Traceability footer** — links to M17, M29, M30, Customer Report, Sales/Piutang reports; value disclaimer

### Filters

- View toggle: **Attention Customers** (default) / **All Customers**
- Wilayah, Klasifikasi (filter only), Tier, Lifecycle, Action, Salesman

### How to Read It

- **Composition milestone** — consumes M17, M29, and M30 outputs; does not recalculate upstream rules.
- **Collect** action links to M30 Collection Optimization (`?customerKey=`) — does not duplicate M30 queue.
- **Customer Value = Omzet Proxy, NOT profitability** — shown in KPI disclaimer and row context.
- Refreshes with **Customer** snapshot worker (~30 minutes) after M30 step.

### Customer Report (M31)

**Navigate:** Sidebar → Reports → Customer Report, or drill from portfolio row.

**Route:** `/reports/customers` (optional `?customerCode=` pre-filter)

One row per customer from M31 snapshot — lifecycle, tier, portfolio action, salesman summary, MTD omzet, open balance.

**Related:** [Customer Portfolio Optimization feature doc](../customer-portfolio-optimization/feature.md)

---

## Inventory Forecast Dashboard (M28)

**Navigate:** Sidebar → Dashboard → Inventory Forecast.

**Route:** `/dashboard/inventory-forecast`

**Question answered:** Which active SKUs may run out of stock within 30 days, and when should purchasing review replenishment?

### What You See

1. **Executive summary** — stock-out count, projected inventory value, top risks
2. **KPI row — Position vs Projection** — Current Value, Projected Value @ H, Avg DOS, Health Score
3. **KPI row — Risk Exposure** — Stock-Out Risk Items, Overstock/Understock Value, At-Risk %
4. **KPI row — Scenario Bands** — Best/Expected/Worst projected value + Confidence
5. **Forecast Inventory Level** — Projected value depletion chart (days 0..H)
6. **Consumption Trend + Risk Heat** — 30-day daily units with ADC line; DOS × value heat grid
7. **Top Inventory Risks** — Priority-ordered table (max 10)
8. **Purchasing Recommendations** — Reorder date and indicative qty (max 10)
9. **Traceability footer** — links to Inventory, Inventory Risk, Inventory Report, Purchasing

### How to Read It

- Uses **30-day Faktur sales qty** as consumption (gross, not net of retur).
- **Days of Supply** = current qty ÷ average daily consumption.
- **Recommended purchase qty** is decision support only — not an approved PO.
- Refreshes with the existing **InventoryRisk** snapshot worker (~60 minutes).

### Typical Use

- Replenishment planning before stock-outs
- Working-capital review of projected inventory value
- Cross-check with Inventory Risk for obsolescence context

**Related:** [Inventory Forecast feature doc](../inventory-forecast/feature.md)

---

## Customer Analytics Dashboard (M17)

**Navigate:** Sidebar → Dashboard → Customers.

**Route:** `/dashboard/customers`

**Question answered:** Which customers require management attention?

### What You See (fixed section order)

1. **Attention Cards** — Collection, Concentration, Activity, Inactivity, Credit
2. **Customer Attention List** — one row per customer × signal
3. **Top 10 Rankings** — Omzet (current month) and Piutang (all open)
4. **Segmentation** — By Klasifikasi, By Wilayah, Active vs Dormant
5. **Navigation** — links to Sales/Piutang dashboards and reports

### Attention signals

| Signal | Rule |
| ------ | ---- |
| Overdue | Any overdue balance on the customer |
| Dormant | No Faktur for 90 days with prior purchase history; active this month excluded |
| Plafond breach | Open balance > `Plafond` when `Plafond > 0` |
| Suspended + Sales | `IsSuspend` and Faktur in current calendar month |

Concentration percentages (Top Omzet %, Top Piutang %) are informational — no automatic warning thresholds.

**Card shortcuts:** Collection card → Piutang Dashboard; Activity card → Sales Dashboard; Credit card → Attention List on this page.

### Drill-down

Click a customer row (attention list or rankings) → Sales or Piutang Report with customer name pre-filter (`?q=`). Piutang dashboard uses all-time open balance; Piutang Report defaults to a period filter — same semantic gap as the Piutang Dashboard.

**Supplements** Executive Dashboard Top 5 Customers — does not replace it.

---

## Salesman Performance Dashboard (M18)

**Navigate:** Sidebar → Dashboard → Salesmen.

**Route:** `/dashboard/salesmen`

**Question answered:** Which salesman requires management attention?

### What You See (fixed section order)

1. **Attention Cards** — Performance, Collection Exposure, Portfolio
2. **Salesman Attention List** — one row per salesman × signal
3. **Performance Rankings** — Top 10 Omzet (current month) and Top 10 Achievement %
4. **Exposure Rankings** — Top 10 Piutang (all open)
5. **Segmentation** — By Wilayah, Active vs Inactive, By Segment (when configured)
6. **Navigation** — links to Sales/Piutang dashboards and reports

### Attention signals

| Signal | Rule |
| ------ | ---- |
| Below Target | Target configured AND achievement % in Warning (80–99%) or Critical (<80%) band |
| Missing Target Setup | Month activity (omzet or customers) but no target configured |
| High Overdue Exposure | Rep in top N% by overdue balance (default 20%) among reps with overdue > 0 |
| High Piutang Exposure | Rep in top N% by open balance (default 20%) among reps with balance > 0 |
| Customer Concentration | Top-customer % of rep omzet (informational — no automatic threshold) |
| Dormant Customer Portfolio | ≥1 dormant customer (90-day rule) attributed via last invoicing salesman |

Concentration percentages on Portfolio card (Top Omzet Salesman %, Top Piutang Salesman %) are informational — no automatic warning thresholds.

**Card shortcuts:** Performance card → Sales Dashboard; Collection Exposure card → Piutang Dashboard; Portfolio card → Attention List on this page.

### Achievement bands (Top Achievement % table)

Same M16 thresholds as executive Sales card: ≥100% Healthy · 80–99% Warning · <80% Critical · no target Unknown.

### Drill-down

Click salesman name or ranking row → detail drawer (Principal Achievement + achievement trend). **Investigate** button → Sales or Piutang Report with salesman name pre-filter (`?q=`).

Default view shows **active salesmen only**; enable **Show Inactive Salesmen** to include reps without current-month Faktur.

| Signal type | Report |
| ----------- | ------ |
| Below Target, Missing Target Setup, Customer Concentration, Dormant Portfolio | Sales Report |
| High Overdue Exposure, High Piutang Exposure | Piutang Report |
| Top Omzet, Top Achievement % | Sales Report |
| Top Piutang | Piutang Report |

Piutang dashboard uses all-time open balance; Piutang Report defaults to a period filter — same semantic gap as Piutang/Customer dashboards.

**Supplements** Sales Dashboard Top 10 Salesman — does not replace executive dashboard or Sales Top 10 table.

---

## Collection Dashboard (M20)

**Navigate:** Sidebar → Dashboard → Collection.

**Route:** `/dashboard/collection`

**Question answered:** Are receivables being converted into cash, and which receivables require collection attention?

### What You See (fixed section order)

1. **Collection Attention Cards** — Exposure, Recovery, Portfolio
2. **Recovery Summary** — Cash Collected MTD, Recovery vs Billing %, Payment Mix (Cash / Giro / Adjustment)
3. **Aging Risk Summary** — overdue-only four buckets (1–30, 31–60, 61–90, >90 days)
4. **Collection Attention List** — Customer, Salesman, or Wilayah × signal
5. **Top Overdue Customers / Salesmen / Wilayah** — ranked by overdue balance only
6. **Navigation** — Piutang, Customer, Salesman dashboards and Piutang Report

No Total Piutang headline (differentiator from Piutang Dashboard).

### Recovery KPIs (current calendar month)

| KPI | Definition |
| --- | --- |
| Cash Collected MTD | `SUM(BayarTunai)` from FF2 pelunasan |
| Recovery vs Billing % | `TotalBayar (Cash+Giro) ÷ month Faktur omzet × 100` |
| Payment Mix | Cash / Giro / Adjustment share of settlement total |

### Attention signals

| Signal | Entity | Rule |
| --- | --- | --- |
| ChronicOverdue | Customer | Open balance in >90d bucket |
| LegacyDebt | Customer | M17 dormant (90-day) + open balance > 1 |
| PlafondBreachOverdue | Customer | Plafond breach + overdue balance |
| Overdue | Customer | Any overdue (suppressed when higher-priority signal applies) |
| HighOverdueWorkload | Salesman | Any overdue on rep's invoiced book |
| LowRecoveryVsBilling | Salesman | Month omzet > 0 and rep collections < omzet |
| WilayahHotspot | Wilayah | Wilayah overdue ≥ 15% of company overdue |

### Drill-down

| Row type | Action |
| --- | --- |
| Customer / Salesman (attention list or rankings) | Piutang Report with `?q=` name pre-filter |
| Wilayah ranking | Display only — no row click |

Dashboard uses all open balances; Piutang Report may default to a period filter.

**Refresh cadence:** 30 minutes (`CollectionIntervalMinutes`). Worker domain: `Collection`.

**Supplements** Piutang, Customer, and Salesman dashboards — does not duplicate Total Piutang or full aging pie.

---

## Alert Center (M23)

**Route:** `/alerts`  
**Page title:** **Alert Center**

**Purpose:** Company-wide management attention feed — answers *What requires attention right now across the entire business?* Aggregates attention signals from M17–M22 snapshots; does not define new signals.

**Entry points:**
- Sidebar → Dashboard → **Alert Center**
- Executive Dashboard → **Open Alert Center**

**Daily review question:** *What requires attention right now across the business?*

### Page Sections (fixed order)

1. **Platform Alerts** — pinned stale/degraded/unavailable warnings
2. **Alert Summary by Category** — count badges (Sales, Customer, Collection, Inventory, Purchasing, Location)
3. **Alerts** — exception entity rows, Top 20 per category, producer priority sort
4. **Inventory Risk Summary** — M19 KPI counts only (no SKU rows); link to Inventory Risk dashboard
5. **Concentrations** — informational concentration metrics (separate from exceptions)
6. **Domain Dashboards** — navigation links to all dashboards

### Drill-down path

```text
Alert Center → Domain Dashboard → Report (?q= pre-filter when available)
```

**Deduplication (automatic):** M20 wins customer overdue over M17; Legacy Debt replaces Dormant; M20 High Overdue Workload replaces M18 High Overdue Exposure.

**Out of scope:** Alert acknowledgment, history, Desktop deep links, real-time refresh.

**Catalog:** See [ALERT-REGISTRY.md](./ALERT-REGISTRY.md).

---

## Branch / Warehouse Performance Dashboard (M22 — Location)

**Navigate:** Sidebar → Dashboard → Locations.

**Route:** `/dashboard/locations`

**Question answered:** Are we becoming too dependent on a particular warehouse or territory?

### What You See (fixed section order)

1. **Location Attention Cards** — informational concentration % (inventory, at-risk, sales, wilayah) and operational counts
2. **Top Warehouse by Inventory / At-Risk / Sales / Purchasing** — Top 10 with % of company total
3. **Top Wilayah by Sales** — customer Wilayah on Faktur (MTD omzet)
4. **Location Attention List** — Warehouse × Signal only (six signals)
5. **Navigation** — Inventory, Inventory Risk, Sales, Purchasing, Collection, Customer Analytics, Salesman Performance

### Drill-down

| Row type | Action |
| --- | --- |
| Warehouse inventory ranking / attention list | Inventory Report `?q={WarehouseName}` |
| Warehouse at-risk ranking | Inventory Risk Dashboard |
| Wilayah sales ranking | Collection Dashboard (not Piutang Report) |
| Sales / Purchasing warehouse rankings | No report drill-down in V1 |

**Refresh cadence:** 60 minutes (`LocationIntervalMinutes`). Worker domain: `Location`. Runs **after Collection** in `RefreshAll`.

**Cross-links only** to M20 wilayah overdue and M21 purchasing backlog — no embedded panels.

---

## Piutang Dashboard

**Navigate:** Sidebar → Dashboard → Piutang, or click the Piutang attention card on the Management Attention Center.

**Route:** `/dashboard/piutang`

### What You See

1. **KPI row** — Total Piutang, Total Customer, Overdue Customer
2. **Aging Distribution** — Pie chart with five buckets: Current, 1–30 Days, 31–60 Days, 61–90 Days, > 90 Days
3. **Top 10 Outstanding Customers** — Table ranked by outstanding balance

### How to Read It

- **Total Piutang** is the sum of all open invoice balances (`KurangBayar`).
- **Total Customer** counts distinct customers with open balances.
- **Overdue Customer** counts customers with any balance past the due date (Current bucket excluded).
- Aging is calculated from **Jatuh Tempo** (due date) versus today.
- The sum of all aging slices equals Total Piutang.

### Typical Use

- Finance team daily monitoring of collection exposure
- Identifying customers with the largest outstanding balances
- Understanding how much debt is severely overdue (> 90 days)

---

## Inventory Dashboard

**Navigate:** Sidebar → Dashboard → Inventory, or click the Inventory attention card on the Management Attention Center.

**Route:** `/dashboard/inventory`

### What You See

1. **KPI row** — Total Inventory Value, Total Item
2. **Inventory by Category** — Horizontal bar chart
3. **Inventory by Supplier** — Horizontal bar chart
4. **Top 10 Categories** — Table ranked by inventory value
5. **Top 10 Suppliers** — Table ranked by inventory value

### How to Read It

- Values use **HPP × Qty** (cost × quantity).
- In-Transit warehouse stock is excluded.
- Items with zero net quantity are excluded.
- Blank category or supplier labels appear as **Unknown**.
- Chart and table data show the same Top 10 for each dimension.

### Typical Use

- Reviewing where inventory capital is concentrated
- Supporting purchasing decisions by supplier exposure
- Planning category-level stock reviews

---

## Slow Moving & Dead Stock Dashboard (M19)

**Navigate:** Sidebar → Dashboard → **Inventory Risk**.

**Route:** `/dashboard/inventory-risk`

**Question answered:** Which inventory requires management attention and why?

### What You See (fixed section order)

1. **Attention Cards** — Dead Stock Item Count/Value, Slow Moving Item Count/Value, At-Risk Inventory %
2. **Attention Indicator** — when any at-risk inventory exists (generic M16–M18 style)
3. **Inventory Aging Distribution** — pie chart: Active, Slow Moving, Dead Stock, Never Sold
4. **Category Risk Exposure** — horizontal bar of at-risk value by category (Top 10)
5. **Supplier Risk Exposure** — horizontal bar of at-risk value by supplier/principal (Top 10)
6. **Inventory Attention List** — one row per item × signal (Dead Stock · Slow Moving · Never Sold)
7. **Top 10 Rankings** — Dead Stock by Value \| Slow Moving by Value
8. **Navigation** — links to Inventory Dashboard and Inventory Report

### Classification signals

| Signal | Rule |
| ------ | ---- |
| Never Sold | Item has stock but no Faktur sales history |
| Slow Moving | Last Faktur **90–179 days** ago |
| Dead Stock | Last Faktur **≥ 180 days** ago |

Items with a Faktur within the last **89 days** are **Active** — excluded from at-risk KPIs. An item appears in **at most one** signal row.

**Movement basis:** Last Faktur Date per item (gross invoice history). Returns, stock transfers, and adjustments do not reset the aging clock on this dashboard.

### Drill-down

Click an attention list row or Top 10 row → **Inventory Report** opens with item name pre-filter (`?q=`).

**Navigation path:** Inventory Risk → Inventory Dashboard → Inventory Report.

**Supplements** the Inventory Dashboard (composition view) — does not replace it.

### Freshness

- **⚠ Dashboard Data Not Fresh** when snapshot exceeds **60-minute** refresh interval.
- **Total Inventory Value** on this dashboard should match the Inventory Dashboard and Inventory Report footer (same BrgId-first aggregation).

---

## Purchasing Management Dashboard

**Navigate:** Sidebar → Dashboard → Purchasing, or click the Purchasing attention card on the Management Attention Center.

**Route:** `/dashboard/purchasing` (page title: **Purchasing Management Dashboard**)

### What You See (attention-first layout)

1. **Purchasing Attention Cards** — Posting Exposure, Principal Dependency, Purchasing Pace, Inventory Cross-Risk
2. **Purchasing Summary** — Grand Total Purchase, Total Invoice, Posted %, pending/qualified posting context
3. **Purchasing Attention List** — Principal × Signal rows with drill-down to Purchasing Report
4. **Weekly Purchase Trend** and **Posting Status Breakdown** (V1 statistics — unchanged)
5. **Top 10 Principals** (with % of purchase) and **Principal Exposure Comparison** (MTD purchase · inventory · at-risk)
6. **Navigation** — Purchasing Report, Inventory Dashboard, Inventory Risk Dashboard

### How to Read It

- **Grand Total Purchase** and **Total Invoice** remain traceability KPIs — must match the Purchasing Report footer for the current month.
- **Qualified backlog** = `BELUM` invoices where `LastUpdate` is **3+ calendar days** ago (configurable). Fresh `BELUM` after invoice entry is normal staging, not management attention.
- **Pending Posting Value** shows all unqualified `BELUM` value as supporting context.
- **Principal** is the supplier name on the purchase invoice; blank names appear as **Unknown**.
- Attention list rows open **Purchasing Report** with `?q=` pre-filtered by principal name.
- Void invoices are excluded from all metrics.

### Executive Dashboard (M21)

Purchasing **RequiresAttention** on the Management Attention Center uses **Qualified Backlog Count > 0** only — not all unqualified `BELUM` invoices.

### Typical Use

- Identify suppliers and posting situations requiring management attention
- Cross-check purchase concentration against inventory and at-risk exposure (M15/M19 snapshots)
- Validate KPIs on Purchasing Report; complete posting in BTR Desktop PT2 when needed

### Freshness

- **⚠ Dashboard Data Not Fresh** when either V1 or management snapshot exceeds the **30-minute** refresh interval.
- Grand Total Purchase and Total Invoice come from the V1 purchasing snapshot; management sections may share a different refresh timestamp — the page shows the earlier of the two as freshness anchor.

---

## Report Filtering (All Reports)

Each report includes a filter bar above the table:

| Filter | Applies to | Behavior |
|--------|------------|----------|
| **Period** (date range) | Sales, Piutang, Purchasing | Server-side query; max **31 days**; defaults to current calendar month |
| **Filter by** (date field) | Piutang only | `Jatuh Tempo` (DueDate, default) or `Piutang Date` (PiutangDate) |
| **Search** (free text) | All four reports | Client-side only; filters visible rows instantly |

Click **Apply** after changing the period (or Piutang date field). **Refresh** reloads data using the active period filters.

When search text is active on reports with a summary bar, footer totals recalculate from the filtered rows.

---

## Sales Report

**Navigate:** Sidebar → Reports → Sales Report

**Route:** `/reports/sales`

### What You See

- Period label reflecting the active date range (default: current calendar month)
- Filter bar: period picker and search box
- DataTable with columns: Date, Faktur, Customer, Sales, Total, Status
- Client-side pagination (default 25 rows per page)
- Sortable columns
- Refresh button and generated-at timestamp

### How to Use

1. Open the report to see Fakturs issued in the selected period (default: current month).
2. Adjust the period (max 31 days) and click **Apply** to reload from the server.
3. Use **Search** to filter by Faktur code, customer, sales person, or status (`Kembali`).
4. Sort by any column (click column header).
5. **Status** shows `Kembali` when the signed Faktur has been physically returned.
6. Use **Refresh** to reload after new Desktop transactions.

**Search fields:** Faktur, Customer, Sales, Status.

**Note:** This report does not show footer summary totals. Use the Sales Dashboard for aggregate KPIs.

---

## Piutang Report

**Navigate:** Sidebar → Reports → Piutang Report

**Route:** `/reports/piutang`

### What You See

- Period label with active date field (`Jatuh Tempo` or `Piutang Date`) and range (default: current month on Jatuh Tempo)
- Filter bar: period picker, date-field selector, and search box
- DataTable columns: Customer, Sales, Faktur, Tanggal, Jatuh Tempo, Total Jual, Kurang Bayar
- **Summary bar** below the table: Total Piutang, Total Customer
- Only open balances (`Kurang Bayar > 1`) — paid invoices are not shown

### Piutang Report vs Piutang Dashboard

The **Piutang Dashboard** shows **all** open receivables (all-time analytics). The **Piutang Report** defaults to open receivables whose selected date field falls within the chosen period (max 31 days). When opened from dashboard or alert drill-down with `periodMode=allOpenBalances`, the report shows **All open balances** (no period constraint) so footer totals can reconcile with the Piutang Dashboard.

### How to Use

1. Choose **Filter by**: `Jatuh Tempo` (collection planning) or `Piutang Date` (receivable record date).
2. Set the period (max 31 days) and click **Apply** (not required in all-open investigation mode).
3. Use **Search** to filter by customer, sales person, or faktur code.
4. Sort by Jatuh Tempo to prioritize collection calls.
5. Use **Refresh** after payments are recorded in BTR Desktop.
6. After drill-down, read the **Investigating** breadcrumb for signal and source context.

**Search fields:** Customer, Sales, Faktur.

---

## Inventory Report

**Navigate:** Sidebar → Reports → Inventory Report

**Route:** `/reports/inventory`

### What You See

- Point-in-time stock balance (no date-range filter — current snapshot only)
- Filter bar: search box only
- DataTable columns: Item, Warehouse, Qty, HPP, Nilai Sediaan
- Only rows with Qty > 0; In-Transit warehouse excluded
- **Summary bar:** Total Inventory Value, Total Item
- Helper text explaining footer uses item-level aggregation

### How to Use

1. Browse stock by item and warehouse.
2. Use **Search** to filter by item name/code or warehouse.
3. Confirm footer totals match the Inventory Dashboard (when search is empty).
4. Remember: footer totals group by item first — the sum of visible row values may differ from the footer.
5. Use **Refresh** after stock movements in BTR Desktop.
6. When opened from **Inventory Risk** drill-down, the search box is pre-filled from `?q=` (item name).

**Search fields:** Item, Warehouse.

---

## Purchasing Report

**Navigate:** Sidebar → Reports → Purchasing Report

**Route:** `/reports/purchasing`

### What You See

- Period label reflecting the active date range (default: current calendar month)
- Filter bar: period picker and search box
- DataTable columns: Invoice, Date, Supplier, Warehouse, Total, Disc, Tax, Grand Total, Posting Stok
- **Posting Stok:** `SUDAH` (stock posted) or `BELUM` (not yet posted)
- **Summary bar:** Grand Total Purchase, Total Invoice

### How to Use

1. Review purchase invoices in the selected period (default: current month).
2. Adjust the period (max 31 days) and click **Apply** to reload from the server.
3. Use **Search** to filter by invoice code, supplier, warehouse, or posting status (`SUDAH` / `BELUM`).
4. Confirm footer **Grand Total Purchase** and **Total Invoice** match the Purchasing Dashboard when viewing the same period without search text.
5. Use **Refresh** after new purchases are entered in BTR Desktop.
6. When opened from dashboard or Alert Center drill-down, the search box is pre-filled from `?q=` (supplier/principal name).
7. **Qualified Backlog** investigations auto-filter to `Posting Stok = BELUM` when `posting=BELUM` is present in the URL.

**Search fields:** Invoice, Supplier, Warehouse, Posting Stok.

---

## Investigation Framework (M24)

Management can move from **signal → evidence** consistently across dashboards and Alert Center.

### Workflow

1. **Signal** — KPI card, attention row, ranking row, or alert row shows *what* requires attention.
2. **Investigate** — primary action opens the matching report with structured query params (`q`, entity IDs, `periodMode`, `posting`).
3. **Breadcrumb** — report pages show *Investigating: {entity} · Signal: {label} · Source: {dashboard}*.
4. **View Dashboard** — optional secondary action (Alert Center, breadcrumb link) for domain context.
5. **Desktop** — optional next-step text (e.g. Piutang Tracker FT5); transaction work remains in BTR Desktop.

### Report-first alert path (M23 + M24)

| Entity type | Primary | Secondary |
| ----------- | ------- | --------- |
| Customer, Salesman, Warehouse, Principal, Item | **Investigate** → Report | View Dashboard |
| Company, System | View Dashboard only | — |
| Wilayah | View Dashboard → Collection | — |

### Piutang all-open balances

Drill-down from Customer Analytics, Collection, Piutang Dashboard, or piutang-bound alerts opens Piutang Report in **All open balances** mode (`periodMode=allOpenBalances`) so report evidence matches dashboard open-balance semantics.

---

## Navigation Structure

### Menu Hierarchy

Navigation is organized by **business domain**. Each menu item displays a permanent code (`CODE · Label`) for support and training communication.

```text
BTR Portal
├── Executive
│   ├── EX01 · Executive           → /dashboard
│   └── EX02 · Alert Center        → /alerts
├── Sales
│   ├── SA01 · Sales               → /dashboard/sales
│   ├── SA02 · Sales Forecast      → /dashboard/sales-forecast
│   └── SA03 · Sales Report        → /reports/sales
├── Customers
│   ├── CU01 · Customers           → /dashboard/customers
│   ├── CU02 · Customer Risk Forecast → /dashboard/customer-risk-forecast
│   ├── CU03 · Collection Optimization → /dashboard/collection-optimization
│   ├── CU04 · Customer Portfolio  → /dashboard/customer-portfolio
│   └── CU05 · Customer Report     → /reports/customers
├── Finance
│   ├── FI01 · Piutang             → /dashboard/piutang
│   ├── FI02 · Collection          → /dashboard/collection
│   ├── FI03 · Cash Flow Forecast  → /dashboard/cash-flow-forecast
│   └── FI04 · Piutang Report      → /reports/piutang
├── Sales Force
│   ├── SF01 · Salesmen            → /dashboard/salesmen
│   ├── SF02 · Sales Force Overview → /dashboard/field-activity
│   └── SF03 · Salesman Field Activity → /dashboard/field-activity/detail
├── Inventory
│   ├── IN01 · Inventory           → /dashboard/inventory
│   ├── IN02 · Inventory Risk        → /dashboard/inventory-risk
│   ├── IN03 · Inventory Forecast  → /dashboard/inventory-forecast
│   ├── IN04 · Inventory Optimization → /dashboard/inventory-optimization
│   └── IN05 · Inventory Report    → /reports/inventory
├── Purchasing
│   ├── PU01 · Purchasing          → /dashboard/purchasing
│   └── PU02 · Purchasing Report   → /reports/purchasing
└── Operations
    └── OP01 · Locations           → /dashboard/locations
```

**Menu codes** are Portal-specific identifiers (e.g. `CU03`, `SA02`). They are independent of BTR Desktop screen codes (e.g. `RO2`, `FT5`). When both systems are involved, SOPs should specify which code namespace applies.

**Support communication:** Use menu codes in WhatsApp, phone, and SOP references — e.g. "Open **CU03**" instead of describing scroll position in the sidebar.

### Routes

| Route | Page | Auth Required |
| ----- | ---- | ------------- |
| `/login` | Login | No |
| `/` | Redirect to `/dashboard` | — |
| `/dashboard` | Management Attention Center (executive) | Yes |
| `/alerts` | Alert Center | Yes |
| `/dashboard/sales` | Sales analytics | Yes |
| `/dashboard/piutang` | Piutang analytics | Yes |
| `/dashboard/customers` | Customer Analytics | Yes |
| `/dashboard/salesmen` | Salesman Performance | Yes |
| `/dashboard/collection` | Collection Dashboard | Yes |
| `/dashboard/cash-flow-forecast` | Cash Flow Forecast Dashboard | Yes |
| `/dashboard/customer-risk-forecast` | Customer Risk Forecast Dashboard | Yes |
| `/dashboard/collection-optimization` | Collection Optimization Dashboard | Yes |
| `/dashboard/customer-portfolio` | Customer Portfolio Optimization Dashboard | Yes |
| `/dashboard/inventory-forecast` | Inventory Forecast Dashboard | Yes |
| `/dashboard/locations` | Branch / Warehouse Performance Dashboard | Yes |
| `/dashboard/inventory` | Inventory analytics | Yes |
| `/dashboard/inventory-risk` | Slow Moving & Dead Stock | Yes |
| `/dashboard/purchasing` | Purchasing analytics | Yes |
| `/reports/sales` | Sales Report | Yes |
| `/reports/piutang` | Piutang Report | Yes |
| `/reports/inventory` | Inventory Report | Yes |
| `/reports/purchasing` | Purchasing Report | Yes |
| `/reports/customers` | Customer Report | Yes |

### Navigation Flow

```text
Login → Dashboard Home (Management Attention Center)
          ├── Sidebar → Sales Dashboard
          ├── Sidebar → Piutang Dashboard
          ├── Sidebar → Customer Analytics
          ├── Sidebar → Salesman Performance
          ├── Sidebar → Collection
          ├── Sidebar → Cash Flow Forecast
          ├── Sidebar → Inventory Dashboard
          ├── Sidebar → Inventory Risk
          └── Sidebar → Purchasing Dashboard

Customer Analytics → click customer row → Sales or Piutang Report (pre-filtered)
Salesman Performance → click salesman row → Sales or Piutang Report (pre-filtered)
Inventory Risk → click item row → Inventory Report (pre-filtered)
Executive / Domain Dashboard → Report (domain reports)

Sidebar → [domain group] → [menu code]
```

Authenticated users visiting `/login` are redirected to Dashboard home.

---

## Common User Workflows

### Reviewing Sales Performance

1. Sign in → Dashboard home → check Sales card (Total Omzet, Faktur, Customer).
2. Open **Dashboard → Sales** for target, achievement %, weekly trend, and Top 10 salesman.
3. Open **Reports → Sales Report** to verify individual Fakturs for the current month.

### Monitoring Overdue Receivables

1. Sign in → Dashboard home → check Total Piutang on Piutang card.
2. Open **Dashboard → Piutang** → review Aging pie chart and Overdue Customer count.
3. Check **Top 10 Outstanding Customers** for collection priorities.
4. Open **Reports → Piutang Report** → sort by Jatuh Tempo → follow up on oldest balances.
5. Confirm report footer matches dashboard KPIs.

### Reviewing Customer Attention (M17)

1. Sign in → **Dashboard → Customers** (`/dashboard/customers`).
2. Scan **Attention Cards** — Collection (overdue, >90d exposure), Inactivity (dormant), Credit (plafond breach, suspended + sales).
3. Review **Customer Attention List** for specific customers and signals.
4. Check **Top 10 Rankings** for revenue and receivable concentration.
5. Click a customer row → Sales or Piutang Report opens with customer name pre-filled in search.
6. Use **Navigation** section to jump to Sales/Piutang dashboards for domain context.
7. Cross-check overdue count against **Dashboard → Piutang** when reconciling collection exposure.

### Reviewing Salesman Attention (M18)

1. Sign in → **Dashboard → Salesmen** (`/dashboard/salesmen`).
2. Scan **Attention Cards** — Performance (below/no target), Collection Exposure (overdue/piutang), Portfolio (dormant, concentration %).
3. Review **Salesman Attention List** for specific reps and signals.
4. Check **Performance Rankings** (Top 10 Omzet, Top 10 Achievement %) and **Exposure Rankings** (Top 10 Piutang).
5. Click a salesman row → Sales or Piutang Report opens with salesman name pre-filled in search.
6. Use **Navigation** section to jump to Sales/Piutang dashboards for domain context.
7. Cross-check Top 10 Omzet against **Dashboard → Sales** Top 10 Salesman (same omzet by name; M18 adds Code, target, achievement %).

### Monitoring Inventory Value

1. Sign in → Dashboard home → check Total Inventory Value and Total Item.
2. Open **Dashboard → Inventory** → review category and supplier charts.
3. Check Top 10 Categories and Top 10 Suppliers for concentration risk.
4. Open **Reports → Inventory Report** for item × warehouse detail.
5. Confirm report footer matches dashboard KPIs.

### Reviewing Monthly Purchasing

1. Sign in → Dashboard home → check Grand Total Purchase and Total Invoice on the Purchasing card.
2. Open **Dashboard → Purchasing** → review weekly trend, posting-status breakdown, and Top 10 Principal.
3. Check **Pending Posting Invoice Count** against the posting pie chart (`BELUM` slice).
4. Open **Reports → Purchasing Report** → scan Posting Stok column for `BELUM` invoices needing warehouse action in BTR Desktop.
5. Confirm report footer matches dashboard KPIs.

### Reviewing Inventory Risk (M19)

1. Sign in → **Dashboard → Inventory Risk** (`/dashboard/inventory-risk`).
2. Scan **Attention Cards** — dead stock value, slow moving value, at-risk %.
3. Review **Aging Distribution** pie for capital split across movement classes.
4. Check **Category** and **Supplier Risk Exposure** bars for concentration.
5. Use **Inventory Attention List** for item-level signals and days since last Faktur.
6. Click an item row → Inventory Report opens with item name pre-filled.
7. Cross-check **Total Inventory Value** against **Dashboard → Inventory** when reconciling.

---

## Frequently Asked Questions

### Why do my dashboard numbers differ from the report?

- **Sales:** Dashboard Total Omzet / Total Achievement should equal the sum of **Total** column values on the Sales Report for the same month (both use Faktur `GrandTotal`). If they differ after refreshing both pages, escalate to support.
- **Piutang & Inventory:** Footer totals should match dashboard KPIs. If they differ, refresh both pages. Persistent mismatch indicates a support escalation.
- **Purchasing:** Grand Total Purchase and Total Invoice on the dashboard must match the Purchasing Report footer for the same month. If they differ after refreshing both pages, escalate to support.
- **Inventory report footer vs row sum:** Footer groups by item first; row-level Nilai Sediaan sums across warehouses will not necessarily equal the footer.

### Why does the dashboard show an old timestamp?

Dashboard data refreshes on a background schedule, not on every page load. The **generated-at** time shows when snapshots were last rebuilt. Piutang refreshes every 15 minutes; Sales, Purchasing, PurchasingManagement, Customer, and Salesman every 30 minutes; Inventory and Inventory Risk every 60 minutes. Click **Refresh** to re-read the latest stored snapshot; it does not force an immediate recalculation unless an administrator triggers a manual rebuild.

### Why does the dashboard say data is not available?

Snapshot tables may be empty (new deployment or worker not running). Contact your administrator to run the snapshot worker. Detail dashboard pages return an error until snapshots exist.

### Why can't I change the date range?

V1 reports and dashboards use **fixed periods** (current month for sales/purchasing; open-balance snapshot for piutang; point-in-time for inventory). Date-range filters are planned for a future release.

### Why don't I see paid invoices on the Piutang Report?

Only open balances (`Kurang Bayar > 1`) are shown. Fully paid invoices are excluded by design — there is no "Show Paid" toggle.

### Why is Achievement % blank?

Achievement % requires at least one salesperson target row for the current month in BTR. When Total Target is zero, the percentage is not displayed.

### Can I enter transactions in the portal?

No. BTR Portal is read-only. All transactions must be entered in BTR Desktop.

### What does Posting Stok mean?

On the Purchasing Report, `SUDAH` means the invoice has been posted to inventory; `BELUM` means stock has not yet been received/posted. Warehouse staff complete posting in BTR Desktop.

### What does "Kembali" mean on the Sales Report?

The customer has returned the signed Faktur document to the office (Faktur Kembali status).

### Who can access the portal?

Any BTR user with valid credentials. All authenticated users see the same menus. There is no per-role menu filtering in the current version.

### How do I export to Excel?

Export is not available in the current version. Use BTR Desktop reports for Excel export if needed.

### Why does Inventory Risk differ from Kartu Stok on Desktop?

The portal uses **Last Faktur Date** per item for slow/dead/never-sold classification. Desktop Kartu Stok Summary may use different movement logic. Use Desktop Faktur Brg Info to validate days since last Faktur for spot checks.

### What is "Never Sold" vs "Dead Stock"?

**Never Sold** means the item has stock but has never appeared on a non-void Faktur. **Dead Stock** means the item was sold before but the last Faktur was at least 180 days ago. An item cannot be both.

---

## For Administrators and IT

Operational steps for deploying and maintaining BTR Portal on a Windows server. End users can skip this section.

### Prerequisites

1. Deploy `btr.sql` schema (includes `BTRPD_*` tables).
2. Deploy `btr.portal.api`, `btr.portal.web`, and `btr.portal.worker`.
3. Configure database, JWT, and CORS on each server (see below).
4. Run an initial snapshot backfill before users access dashboards.

### Database Configuration

Portal does **not** use the BTR Desktop registry. Create `appsettings.{MACHINE_NAME}.json` in **both** the API folder and the worker folder (`{MACHINE_NAME}` must match `Environment.MachineName`):

```json
{
  "Database": {
    "ServerName": "OFFICE-SQL01\\SQLEXPRESS",
    "DbName": "btr",
    "IsTest": false
  },
  "Jwt": {
    "Issuer": "btr-portal-api",
    "Audience": "btr-portal-vue",
    "Key": "REPLACE-WITH-STRONG-SECRET-256-BITS-MINIMUM",
    "ExpiryMinutes": 480
  },
  "Cors": {
    "AllowedOrigins": [ "http://your-server/btr-portal" ]
  }
}
```

The IIS app pool identity must reach SQL Server using the embedded `btrLogin` credentials.

### Publishing the API

1. Open `j05-btr-distrib.sln` in Visual Studio 2022 (**ASP.NET and web development** workload required).
2. Right-click `btr.portal.api` → **Publish** → **FolderProfile**.
3. Output: `src/j05-btr-distrib/publish/btr-portal-api/`
4. Copy to IIS physical path (e.g. `/btr-portal-api`).
5. Add `appsettings.{MACHINE_NAME}.json` and ensure `logs/` is writable.

### Publishing the Frontend

```powershell
cd src\j05-btr-distrib\btr.portal.web
npm install
npm run build
```

Copy `dist/` contents to the IIS static site (e.g. `/btr-portal`). Set `VITE_API_BASE_URL` at build time to the production API URL.

### Snapshot Worker Setup

**Initial backfill** (required once after deploy):

```powershell
cd C:\path\to\btr.portal.worker
.\btr.portal.worker.exe --domain All --triggered-by Manual
```

Verify `BTRPD_RefreshLog` shows `Success` for Piutang, Inventory, InventoryRisk, Sales, Purchasing, PurchasingManagement, Customer, Salesman, and Collection.

**Scheduled tasks** — create separate Windows Task Scheduler jobs (including M21 PurchasingManagement):

| Task name | Interval | Command |
| --------- | -------- | ------- |
| `BTR-Portal-Dashboard-Piutang` | Every 15 min | `btr.portal.worker.exe --domain Piutang --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Sales` | Every 30 min | `btr.portal.worker.exe --domain Sales --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Purchasing` | Every 30 min | `btr.portal.worker.exe --domain Purchasing --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-PurchasingManagement` | Every 30 min | `btr.portal.worker.exe --domain PurchasingManagement --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Inventory` | Every 60 min | `btr.portal.worker.exe --domain Inventory --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-InventoryRisk` | Every 60 min | `btr.portal.worker.exe --domain InventoryRisk --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Customer` | Every 30 min | `btr.portal.worker.exe --domain Customer --triggered-by Scheduler` |
| `BTR-Portal-Dashboard-Salesman` | Every 30 min | `btr.portal.worker.exe --domain Salesman --triggered-by Scheduler` |

Task settings: run whether user is logged on or not; service account with SQL access; **Start in** = worker folder; stop if running longer than 30 minutes.

**Worker CLI reference:**

| Argument | Values | Default |
| -------- | ------ | ------- |
| `--domain` | `All`, `Piutang`, `Inventory`, `InventoryRisk`, `Sales`, `Purchasing`, `Customer`, `Salesman` | `All` |
| `--triggered-by` | `Scheduler`, `Manual` | `Scheduler` |

Exit code `0` = success. Logs: `{worker-folder}/logs/btr-portal-worker-{date}.log`.

### Manual Dashboard Refresh

**From the portal (authenticated):**

```http
POST /api/admin/dashboard/refresh
Authorization: Bearer <token>
Content-Type: application/json

{ "domain": "All" }
```

`domain` accepts `All` (default), `Piutang`, `Inventory`, `InventoryRisk`, `Sales`, `Purchasing`, `Customer`, or `Salesman` (case-insensitive).

**Prefer worker CLI** for full rebuilds — the API runs refresh synchronously and may hit IIS request timeout (~110 seconds). Use the API for single-domain ad-hoc refresh; use the worker for `--domain All` or initial backfill.

### Monitoring

| Check | How |
| ----- | --- |
| API health | `GET /api/health` → 200 |
| Snapshot health | `GET /api/health/dashboard-snapshots` — status: `unknown`, `ok`, `refreshing`, or `degraded`; each domain (Piutang, Inventory, InventoryRisk, Sales, Purchasing, Customer, Salesman) shows `LastRefresh.Status` |
| Last refresh (SQL) | `SELECT TOP 1 * FROM BTRPD_RefreshLog WHERE Domain = 'Purchasing' ORDER BY CompletedAt DESC` |
| Worker log | `{worker-folder}/logs/btr-portal-worker-{date}.log` |
| Task Scheduler | History tab on each scheduled task |

### Troubleshooting

| Symptom | Likely cause | Action |
| ------- | ------------ | ------ |
| Login fails with SQL error | Missing or wrong `appsettings.{MACHINE}.json` | Fix Database section; recycle app pool |
| Dashboard 503 / unavailable | Empty snapshot tables | Run worker `--domain All --triggered-by Manual` |
| Stale dashboard data | Scheduler not running | Check Task Scheduler history; re-run worker |
| API refresh times out | Full rebuild too slow for IIS | Use worker CLI instead |
| CORS error in browser | Origin not in `Cors:AllowedOrigins` | Add frontend URL to API appsettings |

### Post-Deploy Verification

```text
GET  /api/health                         → 200
GET  /api/health/dashboard-snapshots     → 200
POST /api/auth/login                     → 200 with token
GET  /api/dashboard/executive            → 200 with token (after worker run)
GET  /api/dashboard/customers            → 200 with token (after Customer worker run)
GET  /api/dashboard/salesmen             → 200 with token (after Salesman worker run)
GET  /api/dashboard/inventory-risk       → 200 with token (after InventoryRisk worker run)
GET  /api/dashboard/sales                → 200 with token
GET  /api/dashboard/sales-forecast       → 200 with token (after Sales worker run)
GET  /api/dashboard/cash-flow-forecast     → 200 with token (after Collection worker run)
GET  /api/dashboard/customer-risk-forecast → 200 with token (after Customer worker run)
GET  /api/dashboard/collection-optimization → 200 with token (after Customer worker run)
GET  /api/dashboard/inventory-forecast     → 200 with token (after InventoryRisk worker run)
GET  /api/dashboard/purchasing           → 200 with token
GET  /api/reports/sales                  → 200 with token
GET  /api/reports/purchasing             → 200 with token
```
