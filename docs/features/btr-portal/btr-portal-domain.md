# BTR Portal — Domain Knowledge

> **Table naming:** Portal snapshot tables use the `BTRPD_*` prefix (formerly `BTR_PortalDashboard*`).

**Audience:** Product Owner, Business Owner, Developers, Future Agents  
**Purpose:** Define what BTR Portal is, why it exists, and the business meaning of its dashboards, reports, and KPIs.

**Related permanent docs:** [Architecture (WHAT)](./btr-portal-architecture.md) · [Operational (HOW)](./btr-portal-operational.md) · [Materialized dashboards](../materialized-dashboard/materialized-dashboard-domain.md) · [Extraction — M16/M17](./knowledge-extraction-report-m16-m17.md) · [Extraction — M18](./knowledge-extraction-report-m18.md) · [Extraction — M19](./knowledge-extraction-report-m19.md) · [Extraction — M21](./knowledge-extraction-report-m21.md) · [Extraction — Purchasing V1](./knowledge-extraction-report-purchasing-dashboard.md)

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
| Dashboards | Executive home (Management Attention Center), Sales, Piutang, Customer Analytics, Salesman Performance, Inventory, Inventory Risk (Slow Moving & Dead Stock), Purchasing Management — detail analytics per domain |
| Reports | Sales, Piutang, Inventory, Purchasing — tabular transaction/detail views |
| Analytics | KPI cards, charts, Top 10 rankings, footer summary totals |
| Investigation / drill-down (M24) | Signal → report evidence navigation with traceability metadata across dashboards, Alert Center, and rankings |

### Business Areas Covered

| Business Area | Portal Coverage |
| ------------- | --------------- |
| Sales | Dashboard KPIs, weekly trend, target vs achievement, salesman ranking; Sales Report |
| Finance (Piutang) | Dashboard KPIs, aging analysis, overdue customers, Top 10 customers; Piutang Report |
| Customer (cross-domain) | Customer Analytics dashboard — attention signals, rankings, segmentation across Sales + Piutang; drill-down to Sales/Piutang reports |
| Salesman (cross-domain) | Salesman Performance dashboard — per-rep attention signals, achievement %, piutang exposure, rankings across Sales + Piutang; drill-down to Sales/Piutang reports |
| Inventory | Composition dashboard (value, category/supplier breakdown); Inventory Risk dashboard (slow moving, dead stock, never sold); Inventory Report |
| Purchasing | Management attention dashboard (qualified backlog, attention list, cross-domain exposure); V1 KPIs, weekly trend, posting breakdown, Top 10 Principal; Purchasing Report |

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

Management monitors total inventory value, item count, and how value is distributed across categories and suppliers. **Inventory Risk** (M19) adds slow-moving, dead-stock, and never-sold signals based on last Faktur date per item. Users validate composition KPIs against the Inventory Report (per-item × warehouse stock balance) and spot-check idle items against Desktop Faktur history.

### Purchasing

Management reviews which suppliers and purchasing activities require attention — qualified posting backlog, principal concentration, and cross-domain inventory/at-risk exposure — alongside monthly purchase statistics. Users validate traceability KPIs (Grand Total Purchase, Total Invoice) against the Purchasing Report footer totals.

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

### Alert Center (`/alerts`) — M23

Cross-domain **management attention aggregator** — consumes M17–M22 snapshot attention rows and selected platform/executive flags. **Does not** define signals, refresh workers, or snapshot tables.

| Aspect | Rule |
| ------ | ---- |
| Role | Consumer only — read-time composition via `DashboardAlertCenterComposer` |
| Categories | Sales · Customer · Collection · Inventory · Purchasing · Location · Platform |
| Volume | Top 20 entity alerts per category; M19 item rows excluded (summary KPI panel only) |
| Deduplication | M20 canonical for customer overdue; Legacy Debt suppresses Dormant; M20 workload suppresses M18 overdue exposure |
| Catalog | `AlertCenterRegistry` mirrors [ALERT-REGISTRY.md](./ALERT-REGISTRY.md) |

**API:** `GET /api/dashboard/alerts`

**Relationship to M16:** M16 answers *What is happening?* (executive summary). M23 answers *What needs attention?* (exception workspace). Landing page remains `/dashboard`.

### Investigation Framework (M24) — platform capability

Horizontal **investigation and navigation framework** unifying drill-down from KPIs, alerts, rankings, and attention signals (M16–M23) to report evidence. Answers three management questions:

1. **Why am I seeing this?** — signal label and originating surface shown on report breadcrumb
2. **Show me the evidence.** — report opens with correct filters, entity IDs, and period semantics
3. **What should I inspect next?** — optional dashboard context, multi-step guidance, or Desktop screen name (text only)

**Investigation depth model (portal):**

| Depth | Surface | M24 role |
| ----- | ------- | -------- |
| 1 — Signal | KPI card, alert row, attention / ranking row | Emit `InvestigationMetadata` |
| 2 — Context | Domain dashboard | Optional via **View Dashboard** |
| 3 — Tabular evidence | Four reports | Primary terminus — breadcrumb + filters |
| 4 — Transaction audit | BTR Desktop | Text guidance only — no deep links |

**Report-first navigation defaults:**

| Entity type | Primary action | Secondary |
| ----------- | -------------- | --------- |
| Customer, Salesman, Warehouse, Principal, Item | **Investigate** → Report | View Dashboard |
| Company, System | View Dashboard only | — |
| Wilayah | View Dashboard → Collection | — |

**Piutang alignment:** Drill-down from Customer Analytics, Collection, Piutang Dashboard, piutang-bound alerts, and piutang rankings opens Piutang Report in **All open balances** mode so report evidence matches dashboard open-balance semantics (`Sisa > 1`).

**Contract:** Shared `InvestigationMetadata` shape and `InvestigationRegistry` routing defaults — see [btr-portal-architecture.md](./btr-portal-architecture.md). Operational workflow — see [btr-portal-operational.md](./btr-portal-operational.md) § Investigation Framework.

**Relationship to M16 / M23:** M16 surfaces executive Top 5 exposures (clickable with investigation metadata). M23 Alert Center uses **Investigate** (report) as primary action for entity alerts; M24 does not change alert qualification or deduplication (M23 ownership preserved).

**Background refresh:** Scheduled by `btr.portal.worker` via Windows Task Scheduler (per-domain jobs). Authenticated users may trigger an on-demand rebuild via `POST /api/admin/dashboard/refresh` with optional body `{ "domain": "All|Piutang|Inventory|InventoryRisk|Sales|Purchasing|Customer|Salesman" }` (default `All`; domain values are case-insensitive). For full rebuilds or long-running refreshes, prefer the worker CLI — the API runs synchronously and is subject to IIS request timeouts (~110 s default). Operations may also use: `btr.portal.worker.exe --domain All --triggered-by Manual`.

**Observability:** `GET /api/health/dashboard-snapshots` (no auth) returns the latest refresh attempt per domain from `BTRPD_RefreshLog`, including status, duration, and configured interval minutes. Overall health is `unknown` when no domain has a refresh log yet; otherwise `ok`, `refreshing` (any domain `Running`), or `degraded` (any domain `Failed`).

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

**Purpose:** Portfolio quality — receivable exposure, aging risk, and customer concentration. Complements the Collection Dashboard (recovery execution); does not duplicate Top Overdue rankings or salesman/wilayah concentration.

| Section | Content |
| ------- | ------- |
| KPI row | Total Piutang, Total Customer, Overdue Customer, Overdue Piutang, Piutang > 90 Hari (amount + % of total) |
| Concentration | Top 10 Customer %, Top 20 Customer % of Total Piutang |
| Chart | Aging Distribution (pie — 5 buckets, `JatuhTempo` anchor) |
| Table | Top 20 Outstanding Customers — aging breakdown (Customer · Total · Current · 1–30 · 31–60 · 61–90 · >90) |

**Drill-down:** Row click opens Piutang Report with customer name pre-filter (`?q=`).

**Home card metrics (summary):** Total Piutang, Total Customer (unchanged — overview Layer A only).

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

**Data source:** Dedicated `BTRPD_Customer*` snapshot domain — refreshed from source DALs (Faktur, piutang open balance, customer master, last Faktur per customer), **not** composed from Sales/Piutang snapshot tables.

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

**Data source:** Dedicated `BTRPD_Salesman*` snapshot domain — refreshed from source DALs (Faktur, piutang with invoicing salesman, salesman master, targets, last Faktur per customer with salesman), **not** composed from Sales/Piutang/Customer snapshot tables.

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

### Slow Moving & Dead Stock Dashboard (`/dashboard/inventory-risk`)

**Period:** Point-in-time snapshot (no date filter). Classification uses **Last Faktur Date** per item at refresh time.

**Purpose:** Inventory health view — answers *Which inventory requires management attention and why?* **Supplements** the Inventory Dashboard (`/dashboard/inventory`); does not replace composition analytics.

| Section | Content |
| ------- | ------- |
| Attention Cards | Dead Stock Item Count/Value, Slow Moving Item Count/Value, At-Risk Inventory % |
| Attention Indicator | Generic M16–M18 presentation when `RequiresAttention` |
| Aging Distribution | Four-bucket pie: Active, Slow Moving, Dead Stock, Never Sold |
| Category Risk Exposure | Top 10 categories by at-risk inventory value |
| Supplier Risk Exposure | Top 10 suppliers/principals by at-risk inventory value |
| Attention List | One row per item × signal — Dead Stock · Slow Moving · Never Sold |
| Rankings | Top 10 Dead Stock by Value \| Top 10 Slow Moving by Value |
| Navigation | Links to Inventory Dashboard and Inventory Report |

**Data source:** Dedicated `BTRPD_InventoryRisk*` snapshot domain — refreshed from `IStokBalanceViewDal` + `IBrgLastFakturDal` (full FakturItem history aggregate), **not** composed from M15 Inventory snapshot tables.

**Classification rules (authoritative):**

| Class | Rule | Valuation |
| ----- | ---- | --------- |
| **Never Sold** | Aggregated `Qty > 0`; no non-void `FakturItem` history | `Hpp × Qty` (BrgId-first, exclude In-Transit) |
| **Slow Moving** | `LastFakturDate` exists AND idle **90–179 days** | Same |
| **Dead Stock** | `LastFakturDate` exists AND idle **≥ 180 days** | Same |
| **Active** | `LastFakturDate` within last **89 days** | Excluded from at-risk KPIs |

**Supporting rules:** Authoritative movement signal = `MAX(FakturDate)` per `BrgId` from gross Faktur only (`VoidDate = '3000-01-01'`); retur and non-sales outflows do not reset the aging clock; mutually exclusive KPI counts (Never Sold excluded from Slow/Dead counts and dead/slow Top 10).

**At-Risk Inventory %:** `(NeverSoldValue + SlowMovingValue + DeadStockValue) / TotalInventoryValue × 100` — denominator uses same BrgId-first pipeline as M15 Inventory Dashboard.

**Attention Indicator:** `RequiresAttention` when `AtRiskInventoryValue > 0` or any headline at-risk count > 0.

**Drill-down:** Item row click → Inventory Report with item **name** pre-filter (`?q=`). Path: Inventory Risk → Inventory Dashboard → Inventory Report.

**Executive integration (Phase 2 — not yet delivered):** Promote Dead Stock Value, At-Risk Inventory %, and Inventory Risk Attention Indicator to Management Attention Center while preserving composition metrics.

**Explicitly out of scope:** Salesman dimension, ABC classification, warehouse breakdown, export, Kartu Stok drill-down, retur-adjusted demand, mutasi-based movement classification for portal KPIs.

### Purchasing Management Dashboard (`/dashboard/purchasing`)

**Period:** Current calendar month — non-void purchase Invoices only (`InvoiceDate` within month). Cross-domain inventory metrics are point-in-time from M15/M19 snapshots at refresh.

**Purpose:** *Which suppliers and purchasing activities require management attention?* Extends V1 purchasing statistics with qualified backlog, attention signals, and cross-domain principal exposure.

| Section | Content |
| ------- | ------- |
| Attention cards | Posting Exposure, Principal Dependency, Purchasing Pace, Inventory Cross-Risk |
| Summary row | Grand Total Purchase, Total Invoice, Posted %, pending/qualified posting |
| Attention list | Principal × Signal (8 approved signals) |
| Charts | Weekly Purchase Trend, Posting Status Breakdown (V1) |
| Tables | Top 10 Principal with %; Principal Exposure Comparison |

**Qualified backlog rule:** `PostingStok = BELUM` AND `(today − LastUpdate.Date).TotalDays ≥ 3` (default; configurable via `PurchasingQualifiedBacklogDays`).

**Approved attention signals:** `QualifiedBacklog`, `PrincipalSpendConcentration`, `PrincipalInventoryConcentration`, `PrincipalAtRiskExposure`, `CompoundDependency`, `PurchasingInactivity`, `PrincipalInventoryNoPurchase`, `UnknownPrincipal`.

**Executive RequiresAttention (M21):** `QualifiedBacklogCount > 0` — not unqualified `BELUM` count/value.

**Stakeholder note:** Grand Total Purchase and Total Invoice must match the Purchasing Report footer totals (sourced from V1 purchasing snapshot unchanged).

**Snapshot domain:** `BTRPD_PurchasingManagement*` (separate from V1 `BTRPD_Purchasing*`).

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

**Drill-down from Inventory Risk:** Dashboard row click opens this report with `?q=` pre-filled (item name search). Same free-text search fields: item display name/code, warehouse.

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

Dedicated snapshot domain — `BTRPD_Customer*`. Refreshed from source DALs (not composed from Sales/Piutang snapshots).

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

Dedicated snapshot domain — `BTRPD_Salesman*`. Refreshed from source DALs (not composed from Sales/Piutang/Customer snapshots).

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

### Collection Dashboard KPIs (M20)

Dedicated snapshot domain — `BTRPD_Collection*`. Refreshed from source DALs (not Piutang/Customer/Salesman snapshots).

| KPI | Period / scope | Formula | Business meaning |
| --- | -------------- | ------- | ---------------- |
| **Overdue Exposure** | All-time open | Sum overdue row balances (bucket ≠ Current) | Total past-due receivables |
| **>90d Exposure** | All-time open | Sum in `DaysOver90` bucket | Chronic overdue amount |
| **Overdue Concentration %** | All-time open | Top-1 customer overdue ÷ total overdue × 100 | Receivable concentration risk |
| **Cash Collected MTD** | Current month | `SUM(BayarTunai)` | Cash collections this month |
| **Recovery vs Billing %** | Current month | `TotalBayar ÷ month Faktur omzet × 100` | Collections pace vs billing |
| **Payment Mix** | Current month | Cash / Giro / Adjustment share of settlement total | How receivables are settled |
| **Legacy Debt Count** | Dormant + open | M17 dormant rule with balance > 1 | Stale receivable accounts |
| **Top Overdue Customers/Salesmen/Wilayah** | All-time open | Rank by overdue balance only | Collection priority by dimension |

**Attention list:** One row per entity × signal with customer signal priority (ChronicOverdue > PlafondBreachOverdue > LegacyDebt > Overdue).

### Location Dashboard KPIs (M22)

Dedicated snapshot domain — `BTRPD_Location*`. Composes stok/faktur/invoice at refresh; denominators from Inventory, InventoryRisk, Sales, Purchasing snapshots.

| KPI | Period / scope | Formula | Business meaning |
| --- | -------------- | ------- | ---------------- |
| **Top Warehouse Inventory %** | Point-in-time | Rank-1 warehouse inventory ÷ M15 total × 100 | Inventory concentration |
| **Top 3 Warehouse Inventory %** | Point-in-time | Sum top 3 warehouse inventory ÷ M15 total × 100 | Multi-site concentration |
| **Top Warehouse At-Risk %** | Point-in-time | Rank-1 warehouse at-risk ÷ M19 at-risk total × 100 | Obsolescence concentration by site |
| **Top Warehouse / Wilayah Sales %** | Current month | Rank-1 warehouse or Wilayah omzet ÷ Sales total × 100 | Billing / territory dependency |
| **Inactive Warehouse With Stock** | Point-in-time | Count `IsAktif = false` warehouses with inventory > 0 | Legacy sites holding capital |

**Attention list:** Warehouse × Signal — WarehouseInactiveWithStock, WarehouseNoSalesWithInventory, Top-10 concentration signals (inventory, at-risk, sales, purchasing). Wilayah collection attention remains on M20.

### Inventory KPIs

| KPI | Definition | Formula | Business Meaning |
| --- | ---------- | ------- | ---------------- |
| **Total Inventory Value** | Monetary value of stock on hand | After BrgId grouping: `Sum(HPP × Qty)` per item, then sum; exclude In-Transit warehouse; exclude items where aggregated Qty ≤ 0 | Total capital tied up in inventory |
| **Total Item** | Distinct products in stock | Count of BrgId groups where aggregated Qty > 0 (same filters as above) | Breadth of active inventory |
| **Nilai Sediaan** | Row-level inventory value | `HPP × Qty` | Value of a specific item at a warehouse |
| **Top Category** | Categories holding the most value | Top 10 categories by inventory value after BrgId-first rollup; blank category → "Unknown" | Where inventory value is concentrated by product category |
| **Top Supplier** | Suppliers with the most inventory value | Top 10 suppliers by inventory value after BrgId-first rollup; blank supplier → "Unknown" | Which principals/suppliers dominate stock value |

**Traceability:** Total Inventory Value and Total Item = Inventory Report footer totals.

### Inventory Risk KPIs (M19)

Dedicated snapshot domain — `BTRPD_InventoryRisk*`. Refreshed from `IStokBalanceViewDal` + `IBrgLastFakturDal` (not M15 Inventory snapshot).

| KPI | Definition | Business meaning |
| --- | ---------- | ---------------- |
| **Dead Stock Item Count** | Items with `LastFakturDate` idle ≥ 180 days | SKUs with no recent sales |
| **Dead Stock Value** | `SUM(Hpp × Qty)` for dead-stock class | Capital in dead inventory |
| **Slow Moving Item Count** | Items with idle **90–179 days** (excludes Never Sold) | SKUs losing velocity |
| **Slow Moving Value** | `SUM(Hpp × Qty)` for slow-moving class | Capital in slow inventory |
| **Never Sold Item Count** | Items with stock but no Faktur history | SKUs never invoiced |
| **Never Sold Value** | `SUM(Hpp × Qty)` for never-sold class | Capital in unsold inventory |
| **At-Risk Inventory %** | `(NeverSold + Slow + Dead value) / TotalInventoryValue` | Share of inventory capital requiring attention |
| **Aging buckets** | Active · Slow Moving · Dead Stock · Never Sold | Value distribution by movement class |
| **Category/Supplier risk exposure** | Top 10 by at-risk value per dimension | Where risk capital concentrates |
| **Top 10 Dead / Slow** | Ranked by inventory value within class | Highest-impact items per signal |

**Attention list:** One row per item × signal (Dead Stock, Slow Moving, Never Sold) — classes are mutually exclusive.

**Portal vs Desktop:** Portal uses **Last Faktur Date** per PO decision. Desktop Kartu Stok Summary (IF8) may differ — use Desktop for validation only, not as portal authority.

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

### Purchasing Management KPIs (M21)

Management KPIs are materialized in `BTRPD_PurchasingManagement*` — dashboard-only unless noted.

| KPI | Definition | Business Meaning |
| --- | ---------- | ---------------- |
| **Qualified Backlog Count** | Count of `BELUM` invoices where `(today − LastUpdate.Date).TotalDays ≥ 3` | Actionable posting backlog (excludes in-progress drafts) |
| **Qualified Backlog Value** | Sum `GrandTotal` of qualified `BELUM` invoices | Monetary exposure of actionable backlog |
| **Pending Posting Value (unqualified)** | Sum `GrandTotal` where `BELUM` (all unposted) | Supporting context — includes normal staging |
| **Posted %** | `SUDAH` value ÷ (`SUDAH` + `BELUM`) × 100 | Posting completion ratio for the month |
| **Top 1 / Top 3 Principal %** | Top N MTD purchase ÷ Grand Total Purchase × 100 | Monthly spend concentration |
| **Compound Dependency Count** | Principals in purchase Top 10 AND (inventory Top 10 OR at-risk Top 10) | Multi-dimensional supplier dependency |
| **Principal Inventory No Purchase Count** | M15 supplier Top 10 with zero MTD purchase | Legacy stock without current intake |
| **Purchasing Inactivity** | Zero invoices in month when calendar day ≥ 15 | Company-level replenishment gap signal |

**BELUM workflow:** Invoice save always sets `IsStokPosted = false`. Stock posting (PT2) is a separate deliberate step. Not every `BELUM` invoice requires management attention.

**Executive integration (Phase 2 — not yet delivered):** Promote Compound Dependency Count, Qualified Backlog Value, and Top 3 Principal % to Management Attention Center purchasing card while retaining qualified-backlog `RequiresAttention`.

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
| Unknown dimensions | Inventory M15/M19 | Blank category or supplier displayed and aggregated as `"Unknown"` |
| Last Faktur signal | Inventory Risk M19 | `MAX(FakturDate)` per `BrgId` from gross non-void Faktur / FakturItem |
| Slow moving threshold | Inventory Risk M19 | Idle **90–179 days** since last Faktur (item on `today − 90` = Slow Moving) |
| Dead stock threshold | Inventory Risk M19 | Idle **≥ 180 days** since last Faktur |
| Never sold | Inventory Risk M19 | Stock with no FakturItem history — separate signal; excluded from Slow/Dead counts |
| Mutual exclusivity | Inventory Risk M19 | Item counts disjoint across Never Sold, Slow Moving, Dead Stock |
| At-risk denominator | Inventory Risk M19 | `TotalInventoryValue` — same BrgId-first pipeline as M15 |
| Inventory risk snapshot source | Inventory Risk worker | Reads `IStokBalanceViewDal` + `IBrgLastFakturDal` — not M15 snapshot tables |
| Retur / mutasi | Inventory Risk M19 | Do not affect classification clock (gross Faktur only) |
| Top N | All rankings | Top 10 for salesman, customer, category, supplier |
| Posting Stok | Purchasing report | `SUDAH` = stock posted; `BELUM` = not yet posted (default after invoice save) |
| Qualified backlog | Purchasing M21 | `BELUM` AND `LastUpdate` age ≥ `PurchasingQualifiedBacklogDays` (default 3) |
| Purchasing inactivity | Purchasing M21 | `TotalInvoice = 0` for current month AND calendar day ≥ 15 |
| Compound dependency | Purchasing M21 | Purchase Top 10 AND (inventory Top 10 OR at-risk Top 10) by principal name |
| Principal inventory no purchase | Purchasing M21 | M15 supplier Top 10 with zero MTD purchase amount |
| Attention list grain | Purchasing M21 | One row per principal × signal; invoice detail in drill-down only |
| Purchasing snapshot source | Purchasing M21 management worker | Reads invoice view + V1 purchasing + M15 + M19 snapshots — no duplicate inventory SQL |
| Executive purchasing attention | Executive M21 | `RequiresAttention` when `QualifiedBacklogCount > 0` only |
| Faktur Kembali | Sales report | `StatusFaktur == 2` displays as `"Kembali"` |
| No paid toggle | Piutang report | Paid invoices always hidden; no user toggle |
| Fixed periods V1 | All reports | No date-range or search parameters; fixed defaults per report |
| Dashboard traceability | Piutang, Inventory & Purchasing | Report footer totals must match dashboard KPIs |
| DAL reuse | All | Business rules enforced in existing Desktop DALs; portal does not reimplement SQL |
| Snapshot materialization | Dashboards | KPIs and charts served from `BTRPD_*` tables; refreshed by worker |
| Live queries | Reports | Report endpoints query Desktop DALs directly — not snapshotted |
| Sales omzet source | Sales dashboard | Faktur `GrandTotal` for current month — not `BTR_SalesOmzet` pipeline omzet |

---

## Current Product State

Capabilities delivered across milestones M1–M21 (Phase 1 where noted):

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
| Inventory Risk dashboard — Slow Moving & Dead Stock (M19) | Complete (Phase 1) |
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
| Purchasing Management Dashboard — qualified backlog, attention list, cross-domain exposure (M21) | Complete (Phase 1) |
| Vue 3 frontend with login, layout, sidebar navigation | Complete |

---

## Future Direction

Approved milestone roadmap (not yet delivered unless noted):

| Milestone | Focus |
| --------- | ----- |
| M19 Phase 2 | Executive Dashboard — promote Dead Stock Value, At-Risk %, Inventory Risk Attention Indicator |
| M21 Phase 2 | Executive Dashboard — promote Compound Dependency, Qualified Backlog Value, Top 3 Principal % |

Known capabilities explicitly **deferred** (not committed scope):

| Area | Deferred Capabilities |
| ---- | --------------------- |
| Customer analytics | Retur, Effective Call, GPS, Faktur Kembali aggregates, declining purchase trends, Harga Type segmentation |
| Salesman analytics | Pipeline omzet, Effective Call, route coverage, visit compliance, GPS, retur, collection effectiveness / DSO (M20), field activity (M25), Bottom 10 rankings, historical trends, unified salesman score |
| Filtering | Date-range parameters, search, advanced filters on reports |
| Sales analytics | Margin analysis, status breakdown chart, sales period mode toggle |
| Piutang analytics | Collection effectiveness KPIs (planned M20) |
| Inventory analytics | ABC analysis, warehouse breakdown, Kartu Stok drilldown (slow/dead/never-sold delivered M19) |
| Purchasing | Warehouse breakdown, PF2 line detail, PF3 daily detail, PF4 retur beli, purchase-to-sales ratio, automated weekly spike flags |
| Platform | Export (Excel/PDF), drilldown from charts to transactions, role-based menu visibility, server-side pagination |
| Reports | Sales Report footer totals (retrofit) |

Enhancement priority after M15 breadth coverage: **filtering phase** (date range, search) before deeper analytics.
