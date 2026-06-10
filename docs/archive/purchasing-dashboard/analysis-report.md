# Purchasing Dashboard — Feasibility Analysis

**Author role:** Analyst  
**Date:** 2026-06-07  
**Audience:** Product Owner, Architect, Implementer  
**Status:** Product scope approved — ready for Architect implementation plan  

**Related documents:** `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md`, `docs/work/btr-portal-api-scaffolding/implementation-plan-m12-purchasing-report-v1.md`, `docs/foundation/DOMAIN.md`, `docs/foundation/WORKFLOW.md`, `docs/foundation/LANDSCAPE.md`

---

## 1. Executive Summary

BTR Portal today provides **Sales, Piutang, and Inventory** dashboards (summary home cards plus detail analytics) and four matching reports. **Purchasing** has only the **Purchasing Report** (`/reports/purchasing`). Product documentation explicitly states there is no purchasing dashboard and no dashboard–report traceability for purchasing KPIs.

**Conclusion: adding a Purchasing Dashboard to `btr.portal.web` (with supporting API and snapshot infrastructure) is feasible and recommended** as a natural extension of the established portal pattern. Purchase invoice data, business rules, and report aggregation already exist via `IInvoiceViewDal` and `PurchasingReportDal`. The Vue portal already has reusable dashboard layout, chart, and table components.

Product scope has been **approved by the Product Owner** (see Section 10). The Architect may proceed with the implementation plan using Section 6 as the authoritative V1 specification.

---

## 2. Business Objective

| Question | Answer |
| -------- | ------ |
| **Why is this needed?** | Purchasing Administration and management currently rely on a tabular report only. Other business areas get KPI cards, trends, and concentration views for faster operational decisions (replenishment monitoring, unposted stock identification, supplier spend concentration). |
| **Business problem** | No at-a-glance purchasing visibility on the dashboard home; no analytical views comparable to Sales/Inventory dashboards. |
| **Expected outcome** | Management monitors monthly purchasing spend and activity from `/dashboard` and `/dashboard/purchasing`, then validates numbers against the Purchasing Report. |
| **Affected users** | Purchasing Administration, Operations leadership, Finance/management reviewers |

---

## 3. Domain Analysis

### 3.1 Business concepts involved

| Concept | Role in feature |
| ------- | --------------- |
| **Purchase / Invoice** | Primary transaction grain — BTR has no separate Purchase Order entity; purchasing transactions are **Invoice** records. |
| **Supplier (Principal)** | Dimension for spend concentration and Top 10 ranking. Business users typically say Principal; system fields use Supplier. |
| **Warehouse** | Dimension for branch/location purchasing (optional in V1). |
| **Posting Stok** | Operational status — `SUDAH` (stock posted) / `BELUM` (awaiting stock receipt). |
| **Grand Total** | Monetary KPI basis — same as report footer **Grand Total Purchase**. |

### 3.2 Existing KPI definitions (report footer only today)

From `docs/features/btr-portal/btr-portal-domain.md`:

| KPI | Definition | Business meaning |
| --- | ---------- | ---------------- |
| **Grand Total Purchase** | `Sum(GrandTotal)` for current-calendar-month non-void invoices | Monthly purchasing spend |
| **Total Invoice** | Count of invoice rows in the same period | Volume of purchasing activity |

**Approved dashboard-only KPI (V1):**

| KPI | Definition | Business meaning |
| --- | ---------- | ---------------- |
| **Pending Posting Invoice Count** | Count of invoices where Posting Stok = `BELUM` in current month | Operational backlog — invoices awaiting stock receipt |

### 3.3 Business rules already approved

| Rule | Applies to |
| ---- | ---------- |
| Period = current calendar month | Purchasing Report (fixed V1) |
| Void exclusion (`VoidDate = '3000-01-01'`) | Purchasing Report |
| Posting Stok: `SUDAH` / `BELUM` | Purchasing Report |
| Read-only portal | All portal surfaces |
| Top N = 10 | Rankings on other dashboards |
| Dashboard–report traceability | Piutang and Inventory today; **Purchasing approved for V1** (Grand Total Purchase, Total Invoice) |

---

## 4. Landscape Analysis

### 4.1 Systems and ownership

| System | Current purchasing role | Change needed |
| ------ | ----------------------- | ------------- |
| **BTR Desktop** | Source of truth for purchase invoices (`InvoiceInfoForm` PF1) | None — portal remains read-only |
| **btr.portal.api** | `GET /api/reports/purchasing` only | New dashboard endpoint(s); likely overview extension |
| **btr.portal.worker** | Refreshes Sales/Piutang/Inventory snapshots only | Likely new Purchasing domain refresh job |
| **btr.portal.web** | `PurchasingReportView.vue` under Reports menu | New dashboard route, store, overview card, sidebar entry |
| **btr.sql** | No `BTRPD_Purchasing*` tables | New snapshot tables (materialized — approved) |

### 4.2 Current state in `btr.portal.web`

| Asset | Status |
| ----- | ------ |
| Route `/reports/purchasing` | Exists |
| Route `/dashboard/purchasing` | Missing |
| Sidebar Dashboard → Purchasing | Missing |
| Overview home KPI card (4th domain) | Missing — home shows Sales, Piutang, Inventory only |
| `dashboardStore.loadPurchasing()` | Missing |
| Reusable UI components | Available — `DashboardDetailLayout`, `WeeklyTrendChart`, `Top10RankingTable`, `InventoryHorizontalBarChart`, `AgingPieChart` (adaptable for posting-status breakdown) |

### 4.3 Backend aggregates today

| Aggregate | Purpose | Status |
| --------- | ------- | ------ |
| `PurchasingReportAgg` | Tabular report via `GET /api/reports/purchasing` | Complete (M12) |
| `DashboardPurchasingAgg` | Analytical dashboard | **Does not exist** |
| `DashboardOverviewAgg` | Home summary | Sales, Piutang, Inventory only |

### 4.4 Desktop reference (PF1 — `InvoiceInfoForm`)

- Grid and Excel export only; **no chart or analytics screen** (unlike Sales RO2 `SalesOmzetChartForm`).
- User-selectable date range up to 3 months; portal report uses **fixed current month**.
- Same underlying read path: `IInvoiceViewDal.ListData(periode)` from `BTR_Invoice`.

**Implication:** The portal purchasing dashboard will **define new management analytics** rather than mirror an existing desktop chart. Sales dashboard is the best **UX pattern reference**, not a desktop purchasing screen.

### 4.5 Data source fields available (`InvoiceView`)

| Field | Use in dashboard |
| ----- | ---------------- |
| `Tgl` (InvoiceDate) | Period filter; weekly trend bucketing |
| `GrandTotal` | KPI totals; rankings; posting-status split |
| `SupplierName` | Top 10 Principal ranking (UI label; field remains `SupplierName` in data) |
| `WarehouseName` | Deferred to V1.1 — not in V1 scope |
| `PostingStok` | `SUDAH` / `BELUM` status analytics |
| `InvoiceCode` | Row identity in underlying report (not dashboard grain) |

Void exclusion and Posting Stok derivation are enforced in `InvoiceViewDal` SQL (`VoidDate = '3000-01-01'`; `IIF(IsStokPosted = 1, 'SUDAH', 'BELUM')`).

---

## 5. Gap Analysis

### 5.1 Current vs desired behavior

| Area | Current | Desired (minimum) |
| ---- | ------- | ----------------- |
| Dashboard home | 3 domain cards | 4th Purchasing card with headline KPIs and detail link |
| Detail analytics | None for purchasing | `/dashboard/purchasing` with KPI row and analytical views |
| API | Report endpoint only | Dashboard read endpoint consistent with other domains |
| Traceability | Report standalone | Dashboard KPIs reconcile with Purchasing Report footer |
| Snapshot worker | 3 domains (Piutang, Sales, Inventory) | Purchasing domain refresh — **30 min cadence** (approved) |
| Navigation | Report under Reports menu only | Dashboard submenu entry and home card link |

### 5.2 Missing capabilities

1. Backend **DashboardPurchasing** aggregate (query, DAL, controller).
2. Frontend **PurchasingDashboardView** plus store and API wiring.
3. Overview extension (4th card and purchasing section on `GET /api/dashboard/overview`).
4. Snapshot infrastructure extension (SQL tables, worker, health/admin refresh domain) — materialized snapshot model approved.
5. Product documentation update (`btr-portal-domain.md` currently states purchasing dashboard does not exist).

### 5.3 Assumptions — resolved (Product Owner approved)

| # | Assumption | Decision |
| - | ---------- | -------- |
| A1 | V1 period = current calendar month | **Confirmed** |
| A2 | KPI basis = invoice header `GrandTotal` | **Confirmed** |
| A3 | Retur Beli (PF4) excluded from V1 | **Confirmed** — same source as Purchasing Report |
| A4 | Materialized snapshot model | **Confirmed** — same architecture as Sales, Piutang, Inventory |
| A5 | No purchasing target/budget KPI in V1 | **Confirmed** — deferred |
| A6 | Blank supplier/principal name → `"Unknown"` | **Confirmed** — follow Inventory precedent |

### 5.4 Prior product decision reversed

M12 accepted **"no purchasing dashboard"** — the Purchasing Report was standalone visibility. Product Owner has **approved** adding a Purchasing Dashboard with equal visibility to Sales, Piutang, and Inventory.

---

## 6. Approved Feature Specification

> Authoritative V1 scope — Product Owner approved 2026-06-07. No database, API, or class design.

### 6.1 Purpose

Give management and Purchasing Administration **fast, visual insight** into current-month purchasing activity: spend volume, invoice count, posting backlog, weekly pace, and principal concentration — with mandatory traceability to the Purchasing Report.

### 6.2 Users and goals

| User | Goal |
| ---- | ---- |
| Purchasing Administration | Spot invoices with `BELUM` posting; monitor monthly intake |
| Operations leadership | Understand purchasing pace and principal dependency |
| Management | See monthly spend headline on dashboard home |

### 6.3 Page identity

| Element | Value |
| ------- | ----- |
| **Title** | Purchasing Dashboard |
| **Subtitle** | Current Month Purchasing Activity (Void invoices excluded) |
| **Route** | `/dashboard/purchasing` |

### 6.4 Terminology (UI)

Use **Principal** in user-facing labels wherever possible. Data fields may remain `SupplierName` internally.

| UI label | Notes |
| -------- | ----- |
| Top 10 Principal | Not "Top 10 Supplier" |
| Principal Purchase Ranking | Where ranking context is shown |

### 6.5 Approved V1 scope

**Period:** Current calendar month — aligned with Purchasing Report.

**Data architecture:** Materialized snapshot (same model as Sales, Piutang, Inventory).

**Refresh cadence:** 30 minutes.

**Retur Beli:** Excluded — dashboard uses the same invoice source as the Purchasing Report today.

#### Dashboard Home — Purchasing summary card (4th card on `/dashboard`)

| Metric | Definition |
| ------ | ---------- |
| Grand Total Purchase | Same as report footer — must reconcile exactly |
| Total Invoice | Same as report footer — must reconcile exactly |
| Link | "View purchasing analytics →" → `/dashboard/purchasing` |
| Timestamp | `GeneratedAt` from last snapshot refresh |

#### Purchasing Dashboard (`/dashboard/purchasing`)

| # | Section | Content |
| - | ------- | ------- |
| 1 | **KPI row** | Grand Total Purchase, Total Invoice, Pending Posting Invoice Count (count of invoices where Posting Stok = `BELUM`) |
| 2 | **Weekly Purchase Trend** | `Sum(GrandTotal)` per calendar week within current month |
| 3 | **Posting Status Breakdown** | Purchase value split: `SUDAH` vs `BELUM` |
| 4 | **Top 10 Principal** | Ranked by `Sum(GrandTotal)` descending |

**Rationale (Product Owner):** Volume, operational backlog, trend, and principal concentration — consistent with Sales, Piutang, and Inventory dashboard philosophy. No additional business rules required beyond existing report rules.

#### Navigation

- Sidebar: Dashboard → **Purchasing** → `/dashboard/purchasing`
- Home card link as above
- Purchasing Report remains at `/reports/purchasing` (unchanged)

### 6.6 Dashboard–report traceability (mandatory)

| Dashboard KPI | Purchasing Report footer | Rule |
| ------------- | ------------------------ | ---- |
| Grand Total Purchase | Grand Total Purchase | Must match exactly for the same snapshot period |
| Total Invoice | Total Invoice | Must match exactly for the same snapshot period |

Pending Posting Count, weekly trend, posting breakdown, and Top 10 Principal are **dashboard-only analytics** — no report footer equivalent in V1.

### 6.7 Explicitly deferred (not V1)

| Item | Notes |
| ---- | ----- |
| Warehouse analysis | V1.1 — inventory already covers warehouse composition |
| Pending Posting Purchase Value | Count only in V1; value-based backlog deferred |
| Retur Beli analytics | Excluded to preserve report reconciliation |
| Budget vs Actual | No target master data |
| Drill-down from charts | Platform-wide deferral |
| Custom date range | Platform-wide deferral |
| Export (Excel/PDF) | Platform-wide deferral |
| PF2 line detail, PF3 daily detail, PF4 Retur Beli | Prior product deferrals |
| Role-based menu visibility | Platform-wide deferral |
| BTR Desktop transactional changes | Out of scope |

---

## 7. Workflow Analysis

### 7.1 Existing operational workflow (unchanged)

```text
Purchase
    ↓
Stock Receipt
    ↓
Inventory Update
```

Portal does not participate in posting; it **observes** Posting Stok status.

### 7.2 New management workflow

```text
Sign in
    ↓
Dashboard home → Purchasing card (headline KPIs)
    ↓
Purchasing Dashboard (trend, posting mix, Top 10 Principal)
    ↓
Purchasing Report (validate individual invoices; visually filter BELUM rows)
    ↓
BTR Desktop (act on specific invoices / complete posting)
```

### 7.3 Operational use case

1. Review monthly purchasing spend on dashboard.
2. Use posting-status view to prioritize invoices awaiting stock receipt (`BELUM`).
3. Open Purchasing Report to identify specific invoice rows.
4. Complete stock posting in BTR Desktop as needed.

---

## 8. Acceptance Criteria

1. Authenticated user can open `/dashboard/purchasing` from sidebar and from overview home link.
2. Overview home displays a **Purchasing summary card** as the 4th domain card with Grand Total Purchase and Total Invoice.
3. Purchasing Dashboard title is **Purchasing Dashboard** with subtitle **Current Month Purchasing Activity (Void invoices excluded)**.
4. KPI row shows Grand Total Purchase, Total Invoice, and **Pending Posting Invoice Count** (count of `BELUM` invoices only — no pending value KPI).
5. Dashboard includes Weekly Purchase Trend, Posting Status Breakdown (`SUDAH` / `BELUM` by purchase value), and **Top 10 Principal** table (UI uses Principal terminology).
6. **Grand Total Purchase** and **Total Invoice** on dashboard **match exactly** Purchasing Report footer totals for the same snapshot period.
7. Dashboard shows `GeneratedAt` freshness from materialized snapshot; refresh cadence is 30 minutes via worker.
8. Voided purchase invoices and Retur Beli are excluded (same calculation basis as Purchasing Report).
9. Posting Stok values display as `SUDAH` / `BELUM` in posting status breakdown.
10. Warehouse breakdown, pending posting value, budget comparison, drill-down, custom date range, and export are **not** present in V1.
11. No transactional write capability is introduced.

---

## 9. Feasibility Assessment

| Dimension | Rating | Notes |
| --------- | ------ | ----- |
| **Data availability** | High | `InvoiceView` exposes date, supplier, warehouse, GrandTotal, PostingStok |
| **Business rules clarity** | High | Report DAL and domain docs define period, void rule, KPI formulas |
| **Frontend pattern reuse** | High | Sales/Inventory dashboards provide layout, charts, tables, store/API patterns |
| **Backend pattern reuse** | High | Same MediatR + ReportingContext aggregate pattern as other dashboards |
| **Desktop parity** | N/A | No desktop chart to copy — V1 analytics approved by Product Owner |
| **Architecture consistency** | High | Materialized snapshot + 30 min worker cadence approved |
| **Performance risk** | Low | Current-month invoice volume is bounded; aggregation is lightweight vs Piutang full-history |
| **Product precedent** | Approved | M12 "no dashboard" decision reversed; scope locked in Section 6 |

**Overall: Feasible with medium effort** — the platform pattern is proven; the work is extending it to a fourth domain with newly defined analytics.

---

## 10. Product Owner Decisions (Approved)

**Date:** 2026-06-07  
**Status:** Final — Architect may proceed.

### 10.1 V1 analytics sections — Approved

| Section | Approved content |
| ------- | ---------------- |
| KPI row | Grand Total Purchase, Total Invoice, Pending Posting Invoice Count |
| Weekly Purchase Trend | `Sum(GrandTotal)` per calendar week |
| Posting Status Breakdown | `SUDAH` vs `BELUM` (by purchase value) |
| Top 10 Principal | Ranked by `Sum(GrandTotal)` |

### 10.2 Overview home card — Approved

Add Purchasing as the **4th dashboard card** on `/dashboard`. Equal visibility with Sales, Piutang, and Inventory.

### 10.3 Pending Posting KPI — Count only

V1 displays **Pending Posting Invoice Count** only. Pending Posting Purchase Value is **deferred**.

### 10.4 Warehouse breakdown — Deferred to V1.1

Not included in V1. Purchasing ownership is supplier/principal-oriented; warehouse analysis is covered by Inventory.

### 10.5 Snapshot vs live — Materialized snapshot

Follow the same architecture as Sales, Piutang, and Inventory dashboards.

### 10.6 Refresh cadence — 30 minutes

Aligned with Sales domain cadence; sufficient for management dashboard usage.

### 10.7 Retur Beli — Excluded from V1

Dashboard calculations use the same source as the Purchasing Report. Retur Beli excluded to preserve exact footer reconciliation.

### 10.8 Additional decisions

| Topic | Decision |
| ----- | -------- |
| Page title | Purchasing Dashboard |
| Page subtitle | Current Month Purchasing Activity (Void invoices excluded) |
| UI terminology | Use **Principal** in labels (e.g. Top 10 Principal) |
| Traceability | Grand Total Purchase and Total Invoice must match Purchasing Report footer exactly |

### 10.9 Final scope summary

**In scope:** Dashboard home Purchasing card; detail dashboard with KPI row, weekly trend, posting breakdown, Top 10 Principal; materialized snapshot; 30 min refresh; report traceability for two footer KPIs.

**Deferred:** Warehouse analysis, pending posting value, Retur Beli analytics, budget vs actual, drill-down, custom date range, export.

---

## 11. Impact Summary

| Area | Impact |
| ---- | ------ |
| **Business areas** | Purchasing, Portal reporting |
| **Workflows** | New management monitoring path; no change to purchase/posting transactions |
| **Users** | Purchasing Administration, Operations leadership, management |
| **Documentation to update after delivery** | `docs/features/btr-portal/btr-portal-domain.md`, `btr-portal-operational.md`, `materialized-dashboard-domain.md` |
| **Systems touched** | `btr.portal.web`, `btr.portal.api`, `btr.portal.worker`, `btr.application`, `btr.infrastructure`, `btr.sql`, `btr.test` |

---

## 12. Handoff Notes for Architect

Product scope is **approved** (Sections 6 and 10). Proceed with `docs/work/purchasing-dashboard/implementation-plan.md`.

The Architect should decide:

- Snapshot table design and worker integration (mirror Sales/Inventory/Piutang aggregates; **30 min** cadence for Purchasing domain).
- `GET /api/dashboard/purchasing` contract and overview extension (4th home card section).
- Whether to share aggregation logic with `PurchasingReportDal` vs keep dashboard and report DALs independent (portal precedent: avoid cross-aggregate coupling; duplicate filter logic inline if needed).
- Reuse of existing Vue chart components (`WeeklyTrendChart`, `AgingPieChart` or equivalent for posting breakdown, `Top10RankingTable`) vs purchasing-specific wrappers.
- Admin refresh domain value (`Purchasing`) and health endpoint extension.
- UI labels: **Principal** terminology in Vue templates; map from `SupplierName` data field.

### Reference implementations (technical patterns — not business copy)

| Area | Reference |
| ---- | --------- |
| Report data path | `PurchasingReportDal`, `PurchasingReportView.vue`, `GET /api/reports/purchasing` |
| Dashboard detail UX | `SalesDashboardView.vue` (weekly trend, Top 10), `PiutangDashboardView.vue` (status pie), `InventoryDashboardView.vue` (supplier breakdown) |
| Snapshot infrastructure | `RefreshDashboardSalesSnapshotWorker`, `DashboardOverviewDal`, `BTRPD_*Kpi` SQL tables |
| Product rules | `docs/features/btr-portal/btr-portal-domain.md` — Purchasing section and KPI Definitions |
| Prior purchasing report plan | `docs/work/btr-portal-api-scaffolding/implementation-plan-m12-purchasing-report-v1.md` |
| Future dimension hints | `docs/work/materialize-dashboard-data/analysis-report.md` § 6.5 Purchasing dimensions |

---

## 13. Architect Deliverables (next step)

1. `docs/work/purchasing-dashboard/implementation-plan.md` — technical plan using Section 6 as authoritative scope
2. SQL scripts for `BTRPD_Purchasing*` snapshot tables
3. API contract and Vue route/store specification aligned with existing dashboard milestones
4. Worker Task Scheduler job for Purchasing domain (30 min interval)
