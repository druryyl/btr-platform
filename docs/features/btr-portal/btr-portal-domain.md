# BTR Portal — Domain Knowledge

**Audience:** Product Owner, Business Owner, Developers, Future Agents  
**Purpose:** Define what BTR Portal is, why it exists, and the business meaning of its dashboards, reports, and KPIs.

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
| Dashboards | Sales, Piutang (receivables), Inventory — summary home plus detail analytics |
| Reports | Sales, Piutang, Inventory, Purchasing — tabular transaction/detail views |
| Analytics | KPI cards, charts, Top 10 rankings, footer summary totals |

### Business Areas Covered

| Business Area | Portal Coverage |
| ------------- | --------------- |
| Sales | Dashboard KPIs, weekly trend, target vs achievement, salesman ranking; Sales Report |
| Finance (Piutang) | Dashboard KPIs, aging analysis, overdue customers, Top 10 customers; Piutang Report |
| Inventory | Dashboard KPIs, category/supplier breakdown; Inventory Report |
| Purchasing | Purchasing Report (invoice summary) |

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

Management reviews purchase invoice activity for the current month, including whether stock has been posted (`SUDAH` / `BELUM`). No purchasing dashboard exists; the Purchasing Report is standalone visibility.

---

## Dashboard Definitions

### Dashboard Home (`/dashboard`)

Summary landing page with three KPI cards (Sales, Piutang, Inventory). Each card shows compact headline metrics and a link to the corresponding detail dashboard. No charts on the home page.

**Data source:** `GET /api/dashboard/overview` reads Layer A KPI snapshot tables only (`BTR_PortalDashboard*Kpi`). Each card displays its domain's `GeneratedAt` timestamp from the last successful background refresh. Domains may show different refresh times when per-domain scheduler cadences differ (Piutang 15 min, Sales 30 min, Inventory 60 min).

**Background refresh:** Scheduled by `btr.portal.worker` via Windows Task Scheduler (per-domain jobs). Authenticated users may trigger an on-demand rebuild via `POST /api/admin/dashboard/refresh` with optional body `{ "domain": "All|Piutang|Inventory|Sales" }` (default `All`; domain values are case-insensitive). For full rebuilds or long-running refreshes, prefer the worker CLI — the API runs synchronously and is subject to IIS request timeouts (~110 s default). Operations may also use: `btr.portal.worker.exe --domain All --triggered-by Manual`.

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

**Period:** Current outstanding snapshot (`2000-01-01` through today; open balances only).

**Purpose:** Finance monitoring and collection prioritization through aging and customer concentration.

| Section | Content |
| ------- | ------- |
| KPI row | Total Piutang, Total Customer, Overdue Customer |
| Chart | Aging Distribution (pie — 5 buckets) |
| Table | Top 10 Outstanding Customers |

**Home card metrics (summary):** Total Piutang, Total Customer.

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

**Purpose:** Visibility into purchasing invoice activity. No linked dashboard.

| Aspect | Definition |
| ------ | ---------- |
| Period | Current calendar month |
| Row grain | One row per purchase Invoice (PF1 header) |
| Data scope | Active (non-void) invoices |
| Footer totals | Grand Total Purchase, Total Invoice |

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

### Inventory KPIs

| KPI | Definition | Formula | Business Meaning |
| --- | ---------- | ------- | ---------------- |
| **Total Inventory Value** | Monetary value of stock on hand | After BrgId grouping: `Sum(HPP × Qty)` per item, then sum; exclude In-Transit warehouse; exclude items where aggregated Qty ≤ 0 | Total capital tied up in inventory |
| **Total Item** | Distinct products in stock | Count of BrgId groups where aggregated Qty > 0 (same filters as above) | Breadth of active inventory |
| **Nilai Sediaan** | Row-level inventory value | `HPP × Qty` | Value of a specific item at a warehouse |
| **Top Category** | Categories holding the most value | Top 10 categories by inventory value after BrgId-first rollup; blank category → "Unknown" | Where inventory value is concentrated by product category |
| **Top Supplier** | Suppliers with the most inventory value | Top 10 suppliers by inventory value after BrgId-first rollup; blank supplier → "Unknown" | Which principals/suppliers dominate stock value |

**Traceability:** Total Inventory Value and Total Item = Inventory Report footer totals.

### Purchasing KPIs (Report Footer Only)

| KPI | Definition | Formula | Business Meaning |
| --- | ---------- | ------- | ---------------- |
| **Grand Total Purchase** | Total purchase value | `Sum(GrandTotal)` for current-month invoices | Monthly purchasing spend |
| **Total Invoice** | Purchase invoice count | Count of invoice rows in period | Volume of purchasing activity |

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
| Customer key | Piutang | `CustomerCode` when non-empty; fallback to `CustomerName` |
| Unknown dimensions | Inventory M15 | Blank category or supplier displayed and aggregated as `"Unknown"` |
| Top N | All rankings | Top 10 for salesman, customer, category, supplier |
| Posting Stok | Purchasing report | `SUDAH` = stock posted; `BELUM` = not yet posted |
| Faktur Kembali | Sales report | `StatusFaktur == 2` displays as `"Kembali"` |
| No paid toggle | Piutang report | Paid invoices always hidden; no user toggle |
| Fixed periods V1 | All reports | No date-range or search parameters; fixed defaults per report |
| Dashboard traceability | Piutang & Inventory | Report footer totals must match dashboard KPIs |
| DAL reuse | All | Business rules enforced in existing Desktop DALs; portal does not reimplement SQL |

---

## Current Product State

Capabilities delivered and accepted across milestones M1–M15:

| Capability | Status |
| ---------- | ------ |
| Portal API foundation (DI, MediatR, health, CORS, logging) | Complete |
| Materialized dashboard snapshots (background worker + admin refresh) | Complete |
| JWT authentication with BTR users | Complete |
| ReportingContext architecture | Complete |
| Dashboard home with Sales, Piutang, Inventory summary KPIs | Complete |
| Sales detail dashboard (target, achievement, weekly trend, Top 10 salesman) | Complete |
| Piutang detail dashboard (aging, overdue customers, Top 10 customers) | Complete |
| Inventory detail dashboard (category/supplier charts, Top 10 tables) | Complete |
| Sales Report (current month Faktur list) | Complete |
| Piutang Report (open receivables with footer totals) | Complete |
| Inventory Report (stock balance with footer totals) | Complete |
| Purchasing Report (current month invoices with footer totals) | Complete |
| Vue 3 frontend with login, layout, sidebar navigation | Complete |

---

## Future Direction

Known roadmap items explicitly **deferred** beyond M15 (not committed scope):

| Area | Deferred Capabilities |
| ---- | --------------------- |
| Filtering | Date-range parameters, search, advanced filters on reports |
| Sales analytics | Margin analysis, status breakdown chart, sales period mode toggle |
| Piutang analytics | Collection effectiveness KPIs |
| Inventory analytics | ABC analysis, warehouse breakdown, pie/donut composition views, Kartu Stok drilldown |
| Purchasing | PF2 line detail, PF3 daily detail, PF4 retur beli |
| Platform | Export (Excel/PDF), drilldown from charts to transactions, role-based menu visibility, server-side pagination |
| Reports | Sales Report footer totals (retrofit) |

Enhancement priority after M15 breadth coverage: **filtering phase** (date range, search) before deeper analytics.
