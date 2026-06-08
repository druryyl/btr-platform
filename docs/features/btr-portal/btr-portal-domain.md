# BTR Portal — Domain Knowledge

**Audience:** Product Owner, Business Owner, Developers, Future Agents  
**Purpose:** Define what BTR Portal is, why it exists, and the business meaning of its dashboards, reports, and KPIs.

**Related permanent docs:** [Architecture (WHAT)](./btr-portal-architecture.md) · [Operational (HOW)](./btr-portal-operational.md) · [Materialized dashboards](../materialized-dashboard/materialized-dashboard-domain.md) · [Extraction — M16/M17](./knowledge-extraction-report-m16-m17.md) · [Extraction — M18](./knowledge-extraction-report-m18.md) · [Extraction — Purchasing](./knowledge-extraction-report-purchasing-dashboard.md)

For how to use the portal, see [btr-portal-operational.md](./btr-portal-operational.md).  
For how it is built, see [btr-portal-architecture.md](./btr-portal-architecture.md).

---

## Purpose

BTR Portal is a web-based **read-only reporting and dashboard application** for BTR distribution management. It gives management and operational staff browser access to sales, receivable, inventory, and purchasing analytics without opening BTR Desktop.

BTR Desktop remains the **source of truth** for all transactional data. The portal reads from the same database and reuses existing BTR reporting logic. It does not create, edit, or delete business transactions.

---

## Product Vision

Provide practical, reliable **management analytics** for distribution businesses:

- Fast visibility into sales performance and targets
- Effective receivable monitoring and collection prioritization
- Inventory value and composition insight for planning and purchasing
- Traceability from dashboard KPIs to underlying transaction detail

The portal complements BTR Desktop; it does not replace operational transaction processing.

---

## Scope

### In Scope

| Area | Capabilities |
| ---- | ------------ |
| Authentication | Sign in with existing BTR user credentials |
| Dashboards | Executive home (Management Attention Center), Sales, Piutang, Customer Analytics, Salesman Performance, Inventory, Purchasing — detail analytics per domain |
| Reports | Sales, Piutang, Inventory, Purchasing — tabular transaction/detail views |
| Analytics | KPI cards, charts, Top 10 rankings, footer summary totals |

### Business Areas Covered

| Business Area | Portal Coverage |
| ------------- | --------------- |
| Sales | Dashboard KPIs, weekly trend, target vs achievement, salesman ranking; Sales Report |
| Finance (Piutang) | Dashboard KPIs, aging analysis, overdue customers, Top 10 customers; Piutang Report |
| Customer (cross-domain) | Customer Analytics dashboard — attention signals, rankings, segmentation across Sales + Piutang; drill-down to Sales/Piutang reports |
| Salesman (cross-domain) | Salesman Performance dashboard — per-rep attention signals, achievement %, piutang exposure, rankings across Sales + Piutang; drill-down to Sales/Piutang reports |
| Inventory | Dashboard KPIs, category/supplier breakdown; Inventory Report |
| Purchasing | Dashboard KPIs, weekly trend, posting-status breakdown, Top 10 Principal; Purchasing Report |

See `docs/foundation/LANDSCAPE.md` for BTR business area ownership. See `docs/foundation/DOMAIN.md` for business terminology (Faktur, Piutang, Item, etc.).

---

## Non Goals

The portal explicitly does **not** provide:

- Sales Order entry or Faktur creation
- Inventory transactions (adjustments, transfers, posting)
- Purchasing transactions or stock posting workflows
- Master data maintenance (customers, items, suppliers)
- Payment recording or collection workflows
- Export (Excel/PDF)
- Custom date-range filters or advanced search (deferred to future milestones)
- Drilldown from dashboard charts to transaction screens
- Role-based menu visibility (credentials gate access; all authenticated users see the same menus)

BTR Portal is an **analytics and reporting product**, not a transactional product.

---

## Business Areas

### Sales

Management monitors recognized sales omzet, invoice volume, customer reach, weekly trends, monthly target achievement, and top-performing salespeople. Users validate dashboard numbers against the Sales Report (per-Faktur detail for the current month).

### Finance (Piutang)

Management monitors total outstanding receivables, customer exposure, aging distribution, and customers with overdue balances. Users validate dashboard numbers against the Piutang Report (per-Faktur open balance detail).

### Inventory

Management monitors total inventory value, item count, and how value is distributed across categories and suppliers. Users validate dashboard numbers against the Inventory Report (per-item × warehouse stock balance).

### Purchasing

Management reviews purchase invoice activity for the current month, including whether stock has been posted (`SUDAH` / `BELUM`). Users validate dashboard KPIs against the Purchasing Report footer totals (Grand Total Purchase, Total Invoice).

---

## Dashboard Definitions

### Management Attention Center (`/dashboard`)

Executive landing page titled **Management Attention Center** — answers *What requires management attention today?* for all authenticated users.

| Section | Content |
| ------- | ------- |
| Attention Cards | Sales Achievement % (Healthy/Warning/Critical band), Piutang exposure signals, Purchasing pending posting, Inventory concentration |
| Critical Exposures | Top 5 Customers, Categories, Suppliers, Principals (grouped by domain) |
| Domain Summaries | One-line summary + link to domain dashboard |

**Data source:** `GET /api/dashboard/executive` composes existing snapshot data via `DashboardExecutiveComposer` — no new snapshot tables or aggregators. Reads four domain snapshot DALs plus refresh log for health.

**Promoted executive metrics:** Achievement % and Total Achievement (Sales); Total Piutang, Overdue Customer, > 90 Day amount/% (Piutang); Pending Posting count/value (Purchasing); Total Inventory Value, Top Category/Supplier % (Inventory).

**Excluded from executive view:** Total Faktur, Total Customer, Total Item, Total Invoice, weekly trends, direct report links.

**Freshness:** Consolidated `LastRefreshed` = `Min(GeneratedAt)` across domains. `IsDataFresh` when all available domains are within configured interval minutes.

**Legacy endpoint:** `GET /api/dashboard/overview` retained but no longer used by the home page.

**Background refresh:** Scheduled by `btr.portal.worker` via Windows Task Scheduler (per-domain jobs). Authenticated users may trigger an on-demand rebuild via `POST /api/admin/dashboard/refresh` with optional body `{ "domain": "All|Piutang|Inventory|Sales|Purchasing|Customer|Salesman" }` (default `All`; domain values are case-insensitive). For full rebuilds or long-running refreshes, prefer the worker CLI — the API runs synchronously and is subject to IIS request timeouts (~110 s default). Operations may also use: `btr.portal.worker.exe --domain All --triggered-by Manual`.

**Observability:** `GET /api/health/dashboard-snapshots` (no auth) returns the latest refresh attempt per domain from `BTR_PortalDashboardRefreshLog`, including status, duration, and configured interval minutes. Overall health is `unknown` when no domain has a refresh log yet; otherwise `ok`, `refreshing` (any domain `Running`), or `degraded` (any domain `Failed`).

### Sales Dashboard (`/dashboard/sales`)

**Period:** Current calendar month — non-void Fakturs only (`FakturDate` within month).

**Purpose:** Management-level sales performance — target vs achievement, weekly trend, and salesman ranking.

| Section | Content |
| ------- | ------- |
| KPI row | Total Target, Total Achievement, Achievement % |
| Chart | Company-level Target vs Achievement (two-bar comparison) |
| Chart | Weekly Invoiced Sales Trend (`SUM(GrandTotal)` by calendar week) |
| Table | Top 10 Salesman by Invoiced Omzet |

**Home card metrics (summary):** Invoiced Omzet (Faktur), Total Faktur, Total Customer.

**Stakeholder note:** Sales dashboard KPIs use **Faktur `GrandTotal`** for the current month. Pipeline / outstanding-order omzet is no longer included (`PipelineOmzet` is always `0`). This aligns dashboard totals with the Sales Report (per-Faktur list).

### Piutang Dashboard (`/dashboard/piutang`)

**Period:** Current open receivables snapshot — rows where `Sisa > 1` at refresh time (business rule: `KurangBayar > 1`). See [materialized dashboard domain](../materialized-dashboard/materialized-dashboard-domain.md).

**Purpose:** Finance monitoring and collection prioritization through aging and customer concentration.

| Section | Content |
| ------- | ------- |
| KPI row | Total Piutang, Total Customer, Overdue Customer |
| Chart | Aging Distribution (pie — 5 buckets) |
| Table | Top 10 Outstanding Customers |

**Home card metrics (summary):** Total Piutang, Total Customer.

### Customer Analytics Dashboard (`/dashboard/customers`)

**Period:** Sales metrics use **current calendar month** non-void Faktur (`GrandTotal`). Piutang metrics use **all-time open balance** (`KurangBayar > 1`) at refresh time.

**Purpose:** Cross-domain customer management view — answers *Which customers require management attention?* across sales activity and receivable exposure. **Supplements** the executive Top 5 Customers list; does not replace the Management Attention Center.

| Section | Content |
| ------- | ------- |
| Attention Cards | Collection (overdue, >90d exposure) · Concentration (Top Omzet %, Top Piutang %) · Activity (active count) · Inactivity (dormant count) · Credit (plafond breach, suspended + sales) |
| Attention List | One row per customer × signal — Overdue, Dormant, Plafond breach, Suspended + Sales |
| Rankings | Top 10 by Omzet (month) and Top 10 by Piutang (all open) — each with `CustomerCode` and % of domain total |
| Segmentation | By Klasifikasi, By Wilayah, Active vs Dormant |
| Navigation | Links to Sales/Piutang dashboards and reports |

**Data source:** Dedicated `BTR_PortalDashboardCustomer*` snapshot domain — refreshed from source DALs (Faktur, piutang open balance, customer master, last Faktur per customer), **not** composed from Sales/Piutang snapshot tables.

**Attention Indicator:** Generic M16 presentation on cards when `*RequiresAttention` is true. Concentration percentages have **no** automatic warning thresholds.

**Drill-down:** Customer row click opens Sales or Piutang Report with customer **name** pre-filter (`?q=`). Path: Customer Analytics → Domain Dashboard → Report.

**Explicitly out of scope:** Retur analytics, Effective Call, GPS, Faktur Kembali aggregates, collection effectiveness / DSO, new Customer Report route, executive dashboard changes.

### Salesman Performance Dashboard (`/dashboard/salesmen`)

**Period:** Sales metrics use **current calendar month** non-void Faktur (`GrandTotal`). Piutang metrics use **all-time open balance** (`KurangBayar > 1`) at refresh time.

**Purpose:** Cross-domain salesman management view — answers *Which salesman requires management attention and why?* across sales performance and receivable exposure. **Supplements** the Sales Dashboard Top 10 Salesman ranking; does not replace the Management Attention Center or executive dashboard.

| Section | Content |
| ------- | ------- |
| Attention Cards | Performance (Below Target, No Target) · Collection Exposure (High Overdue, High Piutang) · Portfolio (Dormant Portfolio, Top Omzet/Piutang Salesman %) |
| Attention List | One row per salesman × signal — six approved signals |
| Performance Rankings | Top 10 Omzet (month) · Top 10 Achievement % |
| Exposure Rankings | Top 10 Piutang (all open) |
| Segmentation | By Wilayah, Active vs Inactive, By Segment (when configured) |
| Navigation | Links to Sales/Piutang dashboards and reports |

**Data source:** Dedicated `BTR_PortalDashboardSalesman*` snapshot domain — refreshed from source DALs (Faktur, piutang with invoicing salesman, salesman master, targets, last Faktur per customer with salesman), **not** composed from Sales/Piutang/Customer snapshot tables.

**Salesman key:** `SalesPersonId` internal; `SalesPersonName` display; `SalesPersonCode` on ranking rows.

**Attribution rules:**

| Metric | Attribution |
| ------ | ----------- |
| Sales omzet | `Faktur.SalesPersonId` at invoice time |
| Piutang | **Invoicing salesman** — FF1 model (`SalesName` on open Faktur rows) |
| Dormant customers | **Last Invoicing Salesman** — salesman on customer's most recent Faktur |
| Achievement % | `SalesOmzetChartAchievementPolicy.ComputePercent(omzet, target)` per rep |
| Achievement bands | M16 thresholds: ≥100% Healthy · 80–99% Warning · <80% Critical · null Unknown |

**Attention signals (approved):**

| Signal | Inclusion rule |
| ------ | -------------- |
| Below Target | Target exists (`> 0`) AND achievement % in Warning or Critical band |
| No Target | Month activity (`Omzet > 0` OR distinct customers > 0) AND no configured target |
| High Overdue Exposure | Any overdue balance on rep's invoiced open Faktur rows |
| High Piutang Exposure | Open piutang balance `> 0` for rep (informational; no % threshold) |
| Customer Concentration | `Omzet > 0` AND top-customer % computable (informational; no % threshold) |
| Dormant Customer Portfolio | ≥1 dormant customer on rep's book via last-invoicing attribution |

**Attention Indicator:** Generic M16/M17 presentation on cards when `*RequiresAttention` is true. Concentration percentages have **no** automatic warning thresholds.

**Drill-down:** Salesman row click opens Sales or Piutang Report with salesman **name** pre-filter (`?q=`). Path: Salesman Performance → Domain Dashboard → Report.

**Explicitly out of scope:** Pipeline omzet, Effective Call, route coverage, visit compliance, GPS, retur, Faktur Kembali aggregates, collection effectiveness / DSO (M20), field activity metrics (M25), Bottom 10 rankings, historical trends, unified salesman score, new Salesman Report route, executive dashboard changes, changes to Sales Dashboard Top 10 Salesman table.

### Inventory Dashboard (`/dashboard/inventory`)

**Period:** Point-in-time snapshot (no date filter).

**Purpose:** Inventory planning and purchasing decisions through category and supplier composition.

| Section | Content |
| ------- | ------- |
| KPI row | Total Inventory Value, Total Item |
| Chart | Inventory by Category (horizontal bar) |
| Chart | Inventory by Supplier (horizontal bar) |
| Table | Top 10 Categories |
| Table | Top 10 Suppliers |

**Home card metrics (summary):** Total Inventory Value, Total Item.

### Purchasing Dashboard (`/dashboard/purchasing`)

**Period:** Current calendar month — non-void purchase Invoices only (`InvoiceDate` within month).

**Purpose:** Management-level purchasing activity — weekly trend, posting-status composition, and principal concentration.

| Section | Content |
| ------- | ------- |
| KPI row | Grand Total Purchase, Total Invoice, Pending Posting Invoice Count |
| Chart | Weekly Purchase Trend (`SUM(GrandTotal)` by calendar week) |
| Chart | Posting Status Breakdown (`SUDAH` / `BELUM` by purchase value) |
| Table | Top 10 Principal by Purchase Amount |

**Home card metrics (summary):** Grand Total Purchase, Total Invoice.

**Stakeholder note:** Grand Total Purchase and Total Invoice must match the Purchasing Report footer totals for the same month and invoice source (`IInvoiceViewDal`).

---

## Report Definitions

Reports allow users to **validate dashboard KPIs** against underlying records. Footer summary totals on Piutang, Inventory, and Purchasing reports reconcile with dashboard KPIs where applicable.

### Sales Report (`/reports/sales`)

**Purpose:** Inspect Faktur-level sales transactions behind Sales dashboard metrics.

| Aspect | Definition |
| ------ | ---------- |
| Period | Current calendar month |
| Row grain | One row per Faktur |
| Data scope | Active (non-void) Fakturs only |
| Footer totals | None (M9 pattern — totals not shown) |

**Columns:** Date, Faktur, Customer, Sales, Total, Status (`Kembali` when signed Faktur returned).

### Piutang Report (`/reports/piutang`)

**Purpose:** Inspect open receivable detail behind Piutang dashboard KPIs.

| Aspect | Definition |
| ------ | ---------- |
| Period | `2000-01-01` through today |
| Row grain | One row per open Faktur |
| Data scope | `KurangBayar > 1` only (paid invoices excluded) |
| Footer totals | Total Piutang, Total Customer — must reconcile with dashboard |

**Columns:** Customer, Sales, Faktur, Tanggal, Jatuh Tempo, Total Jual, Kurang Bayar.

### Inventory Report (`/reports/inventory`)

**Purpose:** Inspect stock balance detail behind Inventory dashboard KPIs.

| Aspect | Definition |
| ------ | ---------- |
| Period | Point-in-time snapshot (no period label) |
| Row grain | One row per Item × Warehouse |
| Data scope | `Qty > 0` only; In-Transit warehouse excluded |
| Footer totals | Total Inventory Value, Total Item — must reconcile with dashboard |

**Columns:** Item, Warehouse, Qty, HPP, Nilai Sediaan (`HPP × Qty`).

**Note:** Footer totals use BrgId-first aggregation (group by item, then sum). Summing visible rows naively may not match the footer.

### Purchasing Report (`/reports/purchasing`)

**Purpose:** Inspect purchase Invoice detail behind Purchasing dashboard KPIs.

| Aspect | Definition |
| ------ | ---------- |
| Period | Current calendar month |
| Row grain | One row per purchase Invoice (PF1 header) |
| Data scope | Active (non-void) invoices |
| Footer totals | Grand Total Purchase, Total Invoice — must reconcile with dashboard |

**Columns:** Invoice, Date, Supplier, Warehouse, Total, Disc, Tax, Grand Total, Posting Stok (`SUDAH` / `BELUM`).

**Terminology:** BTR has no separate Purchase Order entity. Purchasing transactions are represented as **Invoice** records.

---

## KPI Definitions

All monetary values are in Indonesian Rupiah (IDR). Dashboard and report totals must reconcile when traceability is defined below.

### Sales KPIs

| KPI | Definition | Formula | Business Meaning |
| --- | ---------- | ------- | ---------------- |
| **Total Omzet** | Invoiced sales for the current month | `SUM(GrandTotal)` of non-void Fakturs where `FakturDate` is in the current calendar month | How much sales revenue has been invoiced this month |
| **Completed Omzet** | Same as Total Omzet on detail dashboard | Same as Total Omzet — Faktur `GrandTotal` only | Explicit label for invoiced/achieved revenue |
| **Pipeline Omzet** | Reserved; not used after Faktur cutover | Always `0` | Pipeline omzet from outstanding orders is excluded |
| **Total Faktur** | Invoice count in period | Count of non-void Fakturs in current month | Volume of completed billing activity |
| **Total Customer** | Customer reach in period | Distinct `CustomerCode` (fallback `Customer` name) with Fakturs in current month | Breadth of customer activity |
| **Total Target** | Monthly sales target (company) | `Sum(TargetAmount)` for all rows in `BTR_SalesOmzetTarget` where year/month = current month | Management target for the month |
| **Total Achievement** | Achieved invoiced omzet | Same as Total Omzet / Completed Omzet for current month | Actual performance against target |
| **Achievement %** | Target attainment rate | `Total Achievement ÷ Total Target × 100%`; null when Total Target ≤ 0 | Percentage of monthly target achieved |
| **Top Salesman** | Best-performing salespeople | Top 10 by `SUM(GrandTotal)` per `SalesPersonName`, descending, current month | Identifies leading sales performers |

**Weekly Trend:** Faktur `GrandTotal` summed per calendar week within the current month (7-day segments from month start).

**Traceability:** Total Omzet / Total Achievement = sum of `GrandTotal` from Sales Report rows for the same month (same Faktur source and void exclusion).

### Piutang KPIs

| KPI | Definition | Formula | Business Meaning |
| --- | ---------- | ------- | ---------------- |
| **Total Piutang** | Total outstanding receivable balance | `Sum(KurangBayar)` where `KurangBayar > 1` | How much customers collectively owe |
| **Total Customer** | Customers with open balances | Distinct count using customer key (`CustomerCode` when present, else `CustomerName`) | How many customers carry outstanding debt |
| **Overdue Customer** | Customers with past-due balances | Distinct customers where sum of balances in aging buckets 1–30, 31–60, 61–90, and > 90 days is > 0 (Current bucket excluded) | How many customers need collection attention |
| **Aging Bucket Amount** | Receivable by due-date bracket | `Sum(KurangBayar)` per bucket; `DaysOverdue = Today − JatuhTempo` | Distribution of debt by how long it is overdue |
| **Top Customer** | Largest outstanding balances | Top 10 customers by `Sum(KurangBayar)`, descending | Customers representing the greatest collection risk/exposure |

**Aging buckets (inclusive boundaries):**

| Bucket | Rule |
| ------ | ---- |
| Current (Not Yet Due) | `DaysOverdue ≤ 0` |
| 1–30 Days | `1 ≤ DaysOverdue ≤ 30` |
| 31–60 Days | `31 ≤ DaysOverdue ≤ 60` |
| 61–90 Days | `61 ≤ DaysOverdue ≤ 90` |
| > 90 Days | `DaysOverdue > 90` |

**Traceability:** Total Piutang and Total Customer = Piutang Report footer totals.

### Customer Analytics KPIs (M17)

Dedicated snapshot domain — `BTR_PortalDashboardCustomer*`. Refreshed from source DALs (not composed from Sales/Piutang snapshots).

| KPI | Period / scope | Formula | Business meaning |
| --- | -------------- | ------- | ---------------- |
| **Active Customer** | Current month | Distinct customer keys with non-void Faktur `GrandTotal` | Customers invoiced this month |
| **Dormant Customer** | 90-day rule | Last Faktur ≤ today − 90 days with prior history; exclude active this month | Inactive accounts needing attention |
| **Overdue Customer** | All-time open | Distinct customers with any non-Current aging bucket balance | Collection attention count |
| **Plafond breach** | All-time open | `SUM(KurangBayar) > Plafond` where `Plafond > 0` | Credit policy violations |
| **Suspended + Sales** | Current month + master | `IsSuspend` and Faktur in current month | Policy violation with active billing |
| **Top Omzet Customer %** | Current month | Top-1 omzet ÷ Total Omzet × 100% | Revenue concentration (informational) |
| **Top Piutang Customer %** | All-time open | Top-1 balance ÷ Total Piutang × 100% | Receivable concentration (informational) |
| **Top 10 Omzet** | Current month | `SUM(GrandTotal)` per customer, descending | Revenue priority ranking |
| **Top 10 Piutang** | All-time open | `SUM(KurangBayar)` per customer, descending | Collection priority ranking |

**Customer key:** `CustomerCode` when non-empty, else `CustomerName` (same as Sales/Piutang aggregators).

**Attention list:** One row per customer × signal (Overdue, Dormant, Plafond breach, Suspended + Sales).

### Salesman Performance KPIs (M18)

Dedicated snapshot domain — `BTR_PortalDashboardSalesman*`. Refreshed from source DALs (not composed from Sales/Piutang/Customer snapshots).

| KPI | Period / scope | Formula | Business meaning |
| --- | -------------- | ------- | ---------------- |
| **Below Target** | Current month | Reps with target `> 0` AND achievement % in Warning or Critical band | Underperforming against plan |
| **No Target** | Current month | Month activity AND target null or `≤ 0` | Activity without configured plan |
| **High Overdue Exposure** | All-time open | Distinct reps with any overdue customer on invoiced Faktur | Collection risk by rep |
| **High Piutang Exposure** | All-time open | Distinct reps with open balance `> 0` | Receivable concentration by rep |
| **Customer Concentration** | Current month | Rep with computable top-customer % of omzet | Portfolio dependency on few accounts |
| **Dormant Portfolio** | 90-day rule | Distinct reps with ≥1 dormant customer on book (last-invoicing attribution) | Inactive accounts on rep's book |
| **Top 10 Omzet** | Current month | `SUM(GrandTotal)` per `SalesPersonId`, descending; exclude zero omzet | Revenue leaders |
| **Top 10 Achievement %** | Current month | Achievement % per rep with target; exclude null % and zero omzet | Target attainment leaders |
| **Top 10 Piutang** | All-time open | `SUM(KurangBayar)` per invoicing salesman, descending; exclude zero balance | Receivable exposure leaders |
| **Top Omzet Salesman %** | Current month | Top-1 rep omzet ÷ team total omzet × 100% | Team revenue concentration (informational) |
| **Top Piutang Salesman %** | All-time open | Top-1 rep balance ÷ company total × 100% | Team receivable concentration (informational) |
| **Active vs Inactive** | Current month | Active = Faktur in month; inactive = no current-month Faktur | Team activity segmentation |

**Salesman key:** `SalesPersonId` primary; name fallback map for piutang rows where only `SalesPersonName` is populated. Rows with blank `SalesPersonId` and no name match are excluded from aggregates.

**Attention list:** One row per salesman × signal (Below Target, No Target, High Overdue Exposure, High Piutang Exposure, Customer Concentration, Dormant Customer Portfolio).

### Inventory KPIs

| KPI | Definition | Formula | Business Meaning |
| --- | ---------- | ------- | ---------------- |
| **Total Inventory Value** | Monetary value of stock on hand | After BrgId grouping: `Sum(HPP × Qty)` per item, then sum; exclude In-Transit warehouse; exclude items where aggregated Qty ≤ 0 | Total capital tied up in inventory |
| **Total Item** | Distinct products in stock | Count of BrgId groups where aggregated Qty > 0 (same filters as above) | Breadth of active inventory |
| **Nilai Sediaan** | Row-level inventory value | `HPP × Qty` | Value of a specific item at a warehouse |
| **Top Category** | Categories holding the most value | Top 10 categories by inventory value after BrgId-first rollup; blank category → "Unknown" | Where inventory value is concentrated by product category |
| **Top Supplier** | Suppliers with the most inventory value | Top 10 suppliers by inventory value after BrgId-first rollup; blank supplier → "Unknown" | Which principals/suppliers dominate stock value |

**Traceability:** Total Inventory Value and Total Item = Inventory Report footer totals.

### Purchasing KPIs

| KPI | Definition | Formula | Business Meaning |
| --- | ---------- | ------- | ---------------- |
| **Grand Total Purchase** | Total purchase value | `Sum(GrandTotal)` for current-month invoices | Monthly purchasing spend |
| **Total Invoice** | Purchase invoice count | Count of invoice rows in period | Volume of purchasing activity |
| **Pending Posting Invoice Count** | Unposted purchase invoices | Count where `PostingStok = BELUM` | Backlog of invoices awaiting stock posting |
| **Top Principal** | Largest suppliers by purchase value | Top 10 by `SUM(GrandTotal)` per trimmed `SupplierName`; blank → "Unknown" | Principal concentration in monthly purchasing |

**Weekly Trend:** Invoice `GrandTotal` summed per calendar week within the current month.

**Posting Status Breakdown:** `GrandTotal` grouped by `PostingStok` (`SUDAH` / `BELUM`).

**Traceability:** Grand Total Purchase and Total Invoice = Purchasing Report footer totals (same `IInvoiceViewDal` source and period).

---

## Business Rules

Approved rules governing portal calculations and filters:

| Rule | Applies To | Description |
| ---- | ---------- | ----------- |
| Read-only | All | Portal never writes business data |
| Open balance threshold | Piutang | Only rows with `KurangBayar > 1` count as open receivables |
| Piutang period | Piutang dashboard & report | `PiutangDate` from `2000-01-01` through today |
| Sales period | Sales dashboard & report | Current calendar month; Faktur `GrandTotal` for dashboard KPIs |
| Void exclusion | Sales & Purchasing reports | Voided records excluded (`VoidDate = '3000-01-01'` in DAL) |
| In-Transit exclusion | Inventory | Warehouse named `"In-Transit"` excluded from all inventory calculations |
| Zero quantity exclusion | Inventory report & dashboard analytics | Rows/groups with `Qty ≤ 0` excluded after BrgId aggregation |
| BrgId-first grouping | Inventory | Aggregate by item (`BrgId`) before category/supplier rollup or footer totals |
| Customer key | Piutang, Customer Analytics | `CustomerCode` when non-empty; fallback to `CustomerName` |
| Dormant customer | Customer Analytics | No Faktur for **90 days** with **prior Faktur history**; customers active this month excluded; never-invoiced customers excluded |
| Plafond breach | Customer Analytics | Open balance **>** `Plafond` only when `Plafond > 0` |
| Suspended + sales | Customer Analytics | `IsSuspend = true` AND Faktur in **current calendar month** |
| Attention list grain | Customer Analytics | One row per customer × signal (duplicate signals expected) |
| Customer snapshot source | Customer Analytics worker | Reads source DALs — does not compose from Sales/Piutang snapshot tables |
| Salesman key | Salesman Performance | `SalesPersonId` primary; `SalesPersonCode` on rankings; name for display and report pre-filter |
| Sales omzet attribution | Salesman Performance | `Faktur.SalesPersonId` at invoice time |
| Piutang attribution | Salesman Performance | Invoicing salesman via FF1 join on open Faktur rows |
| Dormant attribution | Salesman Performance | Last invoicing `SalesPersonId` from most recent Faktur per customer |
| No Target activity | Salesman Performance | `OmzetAmount > 0` OR `CustomerCount > 0` in current month |
| Active salesman | Salesman Performance | ≥1 current-month Faktur for rep |
| Inactive salesman | Salesman Performance | No current-month Faktur — in segmentation; excluded from Top rankings when value = 0 |
| Salesman snapshot source | Salesman Performance worker | Reads source DALs — does not compose from Sales/Piutang/Customer snapshot tables |
| Unknown dimensions | Inventory M15 | Blank category or supplier displayed and aggregated as `"Unknown"` |
| Top N | All rankings | Top 10 for salesman, customer, category, supplier |
| Posting Stok | Purchasing report | `SUDAH` = stock posted; `BELUM` = not yet posted |
| Faktur Kembali | Sales report | `StatusFaktur == 2` displays as `"Kembali"` |
| No paid toggle | Piutang report | Paid invoices always hidden; no user toggle |
| Fixed periods V1 | All reports | No date-range or search parameters; fixed defaults per report |
| Dashboard traceability | Piutang, Inventory & Purchasing | Report footer totals must match dashboard KPIs |
| DAL reuse | All | Business rules enforced in existing Desktop DALs; portal does not reimplement SQL |
| Snapshot materialization | Dashboards | KPIs and charts served from `BTR_PortalDashboard*` tables; refreshed by worker |
| Live queries | Reports | Report endpoints query Desktop DALs directly — not snapshotted |
| Sales omzet source | Sales dashboard | Faktur `GrandTotal` for current month — not `BTR_SalesOmzet` pipeline omzet |

---

## Current Product State

Capabilities delivered across milestones M1–M18:

| Capability | Status |
| ---------- | ------ |
| Portal API foundation (DI, MediatR, health, CORS, logging) | Complete |
| Materialized dashboard snapshots (background worker + admin refresh) | Complete |
| JWT authentication with BTR users | Complete |
| ReportingContext architecture | Complete |
| Dashboard home — Management Attention Center (M16) | Complete |
| Customer Analytics dashboard — dedicated snapshot domain (M17) | Complete |
| Salesman Performance dashboard — dedicated snapshot domain (M18) | Complete |
| Sales detail dashboard (target, achievement, weekly trend, Top 10 salesman) | Complete |
| Piutang detail dashboard (aging, overdue customers, Top 10 customers) | Complete |
| Inventory detail dashboard (category/supplier charts, Top 10 tables) | Complete |
| Dashboard overview endpoint (`GET /api/dashboard/overview`) | Complete (retained; home uses executive) |
| Executive dashboard endpoint (`GET /api/dashboard/executive`) | Complete |
| Snapshot worker (`btr.portal.worker`) + Task Scheduler jobs | Complete |
| Admin on-demand refresh (`POST /api/admin/dashboard/refresh`) | Complete |
| Snapshot health endpoint (`GET /api/health/dashboard-snapshots`) | Complete |
| JSON-only SQL config for portal (decoupled from Desktop registry) | Complete |
| Sales Report (current month Faktur list) | Complete |
| Piutang Report (open receivables with footer totals) | Complete |
| Inventory Report (stock balance with footer totals) | Complete |
| Purchasing Report (current month invoices with footer totals) | Complete |
| Purchasing detail dashboard (weekly trend, posting breakdown, Top 10 Principal) | Complete |
| Vue 3 frontend with login, layout, sidebar navigation | Complete |

---

## Future Direction

Approved milestone roadmap after M18 (not yet delivered):

| Milestone | Focus |
| --------- | ----- |
| M19 | Slow Moving & Dead Stock |
| M20 | Collection Dashboard (collection effectiveness / DSO) |

Known capabilities explicitly **deferred** (not committed scope):

| Area | Deferred Capabilities |
| ---- | --------------------- |
| Customer analytics | Retur, Effective Call, GPS, Faktur Kembali aggregates, declining purchase trends, Harga Type segmentation |
| Salesman analytics | Pipeline omzet, Effective Call, route coverage, visit compliance, GPS, retur, collection effectiveness / DSO (M20), field activity (M25), Bottom 10 rankings, historical trends, unified salesman score |
| Filtering | Date-range parameters, search, advanced filters on reports |
| Sales analytics | Margin analysis, status breakdown chart, sales period mode toggle |
| Piutang analytics | Collection effectiveness KPIs (planned M20) |
| Inventory analytics | ABC analysis, warehouse breakdown, pie/donut composition views, Kartu Stok drilldown |
| Purchasing | Warehouse breakdown, pending posting value KPI, PF2 line detail, PF3 daily detail, PF4 retur beli |
| Platform | Export (Excel/PDF), drilldown from charts to transactions, role-based menu visibility, server-side pagination |
| Reports | Sales Report footer totals (retrofit) |

Enhancement priority after M15 breadth coverage: **filtering phase** (date range, search) before deeper analytics.
