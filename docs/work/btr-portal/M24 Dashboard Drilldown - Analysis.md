# BTR Portal Analysis — M24 Dashboard Drill-Down & Investigation Framework

**Status:** Analysis complete — **all open questions resolved** (Product Owner decisions recorded 2026-06-09). Implementation plan: [M24 Dashboard Drill Down - Plan.md](./M24%20Dashboard%20Drill%20Down%20-%20Plan.md). Summary: [M24 Dashboard Drill Down - Implementation Summary.md](./M24%20Dashboard%20Drill%20Down%20-%20Implementation%20Summary.md).  
**Scope:** Business analysis only. No implementation plans, API design, database design, or code.  
**Date:** 2026-06-09 (analysis) · Product Owner decisions recorded 2026-06-09  
**Context:** BTR Portal V2 (M16–M23) successfully answers *What requires management attention?* across eleven dashboard routes and the Alert Center. Management still asks *Why is this happening?* and *Show me the underlying evidence.* Investigation workflows exist but are **inconsistent, partial, and dashboard-specific.** M24 introduces a **common Drill-Down and Investigation Framework** — a platform capability, not another analytics dashboard.

**Product direction (mandatory):** M24 enables **progressive investigation** — KPI → Alert → Dashboard → Report → Transaction Evidence — without creating new business metrics, snapshot aggregators, or dashboard calculations. Focus: investigation, traceability, explainability, and navigation consistency.

**Reference documents:** `docs/foundation/PRODUCT.md`, `docs/foundation/DOMAIN.md`, `docs/foundation/LANDSCAPE.md`, `docs/foundation/WORKFLOW.md`, `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-operational.md`, `docs/features/btr-portal/btr-portal-architecture.md`, `docs/features/btr-portal/ALERT-REGISTRY.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md`, `docs/work/btr-portal/M16 Executive Dashboard - Analysis.md`, `docs/work/btr-portal/M17-Customer-Analytics-Analysis.md`, `docs/work/btr-portal/M20-collection-dashboard-analysist.md`, `docs/work/btr-portal/M23-Alert-Center-Analysis.md`

---

## 1. Executive Summary

BTR Portal V2 has matured into a **management attention platform** with two complementary entry points:

| Entry point | Question answered | Route |
| ----------- | ----------------- | ----- |
| Management Attention Center (M16) | What is happening? | `/dashboard` |
| Alert Center (M23) | What needs attention right now? | `/alerts` |

Both surfaces **identify** situations requiring management attention. Neither consistently answers **why** or provides a **standard path to evidence.**

### Key findings

| Finding | Implication for M24 |
| ------- | ------------------- |
| **Two dashboard generations coexist** — M11–M15 legacy detail dashboards (Sales, Piutang, Inventory) are display-only; M17–M22 attention dashboards implement entity drill-down | M24 must unify investigation across both generations without rewriting KPI logic |
| **Attention dashboards share a de facto pattern** — Attention List / Rankings → Report with `?q=` pre-filter via `navigateToReport()` | Strong reuse candidate for a formal investigation contract |
| **Executive and legacy dashboards link only to domain dashboards** — no direct report links, no ranking drill-down | Largest investigation gap at the management entry point |
| **Four reports exist** — Sales, Piutang, Inventory, Purchasing — with partial filter capability | Reports are the portal's terminal evidence layer; no Faktur-level detail screen exists in portal |
| **Piutang dashboard vs Piutang Report semantic gap** — dashboard uses all-time open balances; report defaults to current-month period on Jatuh Tempo | Investigation paths must document or resolve this mismatch |
| **Purchasing Report does not hydrate `?q=`** on load — unlike other three reports | Known inconsistency blocking purchasing drill-down |
| **Desktop remains authoritative for transaction resolution** — posting, payment recording, Kartu Stok, Piutang Tracker | Portal investigation should stop at reports; Desktop is the operational evidence layer |
| **M23 Alert Center implements dual navigation** — Dashboard button + Report button per alert row | Validates Alert → Dashboard → Report pattern; Report-first shortcut also exists |
| **Cross-domain signals require multi-dashboard paths** — Compound Dependency, Warehouse At-Risk Concentration, Chronic Overdue | M24 must support multi-hop investigation without new KPIs |
| **Charts are never clickable** on any dashboard | **Deferred for M24** — PO Q3: focus on Card, List, Ranking, Alert; not chart interactions |

### Approved product intent (from milestone brief)

M24 is a **platform capability** that explains why a KPI, alert, or attention signal exists. It should **not** add business metrics, snapshot tables, or aggregators. It should standardize navigation, traceability metadata, and progressive disclosure from attention signals to transaction evidence.

### M24 success criteria (Product Owner — authoritative)

Any **KPI**, **Alert**, **Ranking**, or **Attention item** across M16–M23 must consistently answer:

1. **Why am I seeing this?**
2. **Show me the evidence.**
3. **What should I inspect next?**

If the Architect satisfies those three questions across all investigation entry points without adding new business KPIs, M24 is successful.

### Approved scope summary (Product Owner)

| Tier | Items |
| ---- | ----- |
| **Mandatory** | Purchasing Report `?q=` hydration (prerequisite bug fix); Executive Top 5 drill-down; Legacy dashboard ranking drill-down (M11, M14, M15); Investigation breadcrumb; Stable entity ID support; Piutang drill-down semantic alignment (all open balances); Investigation Metadata Contract |
| **Nice to have** (if cheap) | Signal context banner; Desktop next-step guidance; Guided multi-step investigation (metadata only) |
| **Explicitly deferred** | Clickable charts; Desktop deep links; Export; Server-side search; New reports / Collection Report; Transaction detail pages; Kartu Stok in portal; Investigation telemetry |

---

## 2. Investigation Workflow Discovery

This section maps **how management currently investigates business problems** using BTR Portal and BTR Desktop, based on operational documentation, milestone analyses, and implemented UI behavior.

### 2.1 Investigation philosophy today

Management review follows a recurring daily pattern documented in `btr-portal-operational.md`:

1. **Scan** — Executive Dashboard or Alert Center for exceptions.
2. **Contextualize** — Open the owning domain dashboard.
3. **Validate** — Open the matching report and reconcile KPIs to rows.
4. **Act** — Complete operational work in BTR Desktop (payment, posting, collection visit).

Steps 2–3 are **inconsistent**. Attention dashboards (M17–M22) automate step 3 via row click. Legacy dashboards (M11–M15) require manual sidebar navigation to reports.

### 2.2 Situation → investigation path inventory

| Business situation | Primary attention signal | Typical investigation path (as supported today) | Evidence terminus |
| ------------------ | ------------------------ | ----------------------------------------------- | ----------------- |
| **Sales decline / below target** | M16 Achievement Warning/Critical; M18 Below Target | Executive → Sales Dashboard (visual trend only) → manual Sales Report | Faktur rows (`GrandTotal`) |
| **Salesman underperformance** | M18 Below Target, No Target | Salesmen Dashboard → Attention List → Sales Report `?q=` | Faktur rows per rep |
| **Customer inactivity** | M17 Dormant; M20 Legacy Debt | Customers or Collection Dashboard → Attention List → Sales or Piutang Report `?q=` | Faktur history / open Faktur |
| **Overdue receivables** | M14/M16 Overdue Customer; M17/M20 Overdue; M20 Chronic Overdue | Piutang Dashboard (aging) OR Collection Dashboard → Piutang Report `?q=` | Open Faktur balances |
| **Low collection pace** | M20 Low Recovery vs Billing | Collection Dashboard (recovery KPIs) → Piutang Report (manual) | FF2 pelunasan (Desktop); portal shows aggregates only |
| **Dead stock** | M19 Dead Stock | Inventory Risk → Attention List → Inventory Report `?q=` | Item × warehouse stock balance |
| **Slow moving / never sold** | M19 Slow Moving, Never Sold | Same as dead stock | Stock balance + implied Faktur absence |
| **Purchasing backlog** | M21 Qualified Backlog; M16 Pending Posting | Purchasing Dashboard → Attention List → Purchasing Report `?q=` (**broken pre-filter**) | PF1 invoice + Posting Stok |
| **Principal dependency** | M21 Compound Dependency, concentration signals | Purchasing Dashboard → cross-links to Inventory Risk (navigation section only) | Purchasing Report + Inventory Report (manual) |
| **Warehouse concentration** | M22 Warehouse Inventory/At-Risk Concentration | Locations Dashboard → Inventory Report `?q=` (warehouse) OR Inventory Risk (at-risk rows) | Stock balance by warehouse |
| **Wilayah collection hotspot** | M20 Wilayah Hotspot | Collection Dashboard → Wilayah ranking (display only) OR Alert Center (no report button) | Piutang by wilayah — **no direct report filter** |
| **Platform data trust** | M23 Snapshot Stale/Degraded | Alert Center → Executive Dashboard / admin refresh | Refresh log — not business evidence |
| **Unsigned Faktur backlog** | Sales Report `Status = Kembali` (row-level) | Manual Sales Report search — **no dashboard alert** | Faktur Control (Desktop) |
| **Payment / collection audit** | — | Piutang Report sort by Jatuh Tempo | Piutang Tracker FT5 (Desktop) |
| **Stock movement validation** | — | Inventory Report item search | Kartu Stok / Kartu Stok Summary (Desktop) |

### 2.3 Investigation depth model

BTR naturally supports **four investigation depths**:

| Depth | Portal surface | Desktop surface |
| ----- | -------------- | --------------- |
| **1 — Signal** | KPI card, Alert row, Attention List row | — |
| **2 — Context** | Domain dashboard charts/rankings | — |
| **3 — Tabular evidence** | Report (Faktur, open balance, stock row, invoice) | — |
| **4 — Transaction audit** | *Not in portal* | Faktur form, Piutang Tracker, Posting Stok PT2, Kartu Stok |

M24 should standardize depths 1–3 in the portal. Depth 4 remains Desktop by product scope (`btr-portal-domain.md` Non Goals).

---

## 3. Existing Dashboard Navigation Analysis

### 3.1 Dashboard navigation inventory

| Dashboard | Route | Clickable elements | Navigation target | Non-clickable elements |
| --------- | ----- | ------------------ | ----------------- | ---------------------- |
| **Executive (M16)** | `/dashboard` | 4 Attention Cards → domain dashboards; Domain Summary "Details →" links; Open Alert Center button | `/dashboard/sales`, `/piutang`, `/purchasing`, `/inventory`, `/alerts` | Critical Exposure Top 5 lists (4 tables); KPI values inside cards; freshness banners |
| **Alert Center (M23)** | `/alerts` | Platform alert buttons; Alert table Dashboard/Report buttons; Concentration dashboard links; Inventory Risk summary link; Navigation section (11 links) | Per-alert `DashboardRoute`; `navigateToReport(ReportRoute, EntityFilterQuery)`; fixed dashboard routes | Category summary badges; alert data columns; Wilayah rows (no report button) |
| **Sales (M11)** | `/dashboard/sales` | Refresh only | — | KPI row, both charts, Top 10 Salesman table |
| **Piutang (M14)** | `/dashboard/piutang` | Refresh only | — | KPI row, aging pie, Top 10 Customers table |
| **Customer (M17)** | `/dashboard/customers` | Collection card → Piutang; Activity card → Sales; Credit card → in-page anchor; Attention List arrows; Top 10 Omzet/Piutang rows; Navigation section | Reports via `?q=`; cross-dashboard links | Concentration & Inactivity cards; segmentation tables |
| **Salesman (M18)** | `/dashboard/salesmen` | Performance → Sales; Collection Exposure → Piutang; Portfolio → anchor; Attention List; all Top 10 tables; Navigation section | Reports via `?q=` | Segmentation tables |
| **Collection (M20)** | `/dashboard/collection` | Attention List arrows (not Wilayah); Top 10 Overdue Customers/Salesmen; Navigation section | Piutang Report `?q=` | All attention cards; Recovery/Aging summaries; Wilayah rankings |
| **Inventory (M15)** | `/dashboard/inventory` | Refresh only | — | KPI row, charts, Top 10 tables |
| **Inventory Risk (M19)** | `/dashboard/inventory-risk` | Attention List row click; Top 10 Dead/Slow rows; Navigation section | Inventory Report `?q=` | KPI cards, aging pie, category/supplier risk bars |
| **Purchasing (M21)** | `/dashboard/purchasing` | Attention List arrows (not Company); Top 10 Principals; Principal Exposure table; Navigation section | Purchasing Report `?q=` (intended) | Attention cards, summary, charts |
| **Location (M22)** | `/dashboard/locations` | Warehouse inventory rows → Inventory Report; at-risk rows → Inventory Risk; Wilayah sales → Collection; Attention List arrows; Navigation section (7 links) | Mixed: report, dashboard, no drill for sales/purchasing warehouse rankings | Attention cards; warehouse sales/purchasing Top 10 |

### 3.2 Navigation section pattern (M17–M22, M23)

Attention-era dashboards include a bottom **Navigation** section with static cross-links to related dashboards and reports. Routes are API-driven constants (e.g. `SalesDashboardRoute`, `PiutangReportRoute`) returned in query responses.

| Dashboard | Navigation links |
| --------- | ---------------- |
| Customer | Sales Dashboard, Sales Report, Piutang Dashboard, Piutang Report |
| Salesman | Same as Customer |
| Collection | Piutang Dashboard, Customer Analytics, Salesman Performance, Piutang Report |
| Inventory Risk | Inventory Dashboard, Inventory Report |
| Purchasing | Purchasing Report, Inventory Dashboard, Inventory Risk Dashboard |
| Location | Inventory, Inventory Risk, Sales, Purchasing, Collection, Customer, Salesman |
| Alert Center | All 11 dashboard routes |

Legacy dashboards (Sales, Piutang, Inventory) have **no Navigation section**.

### 3.3 Shared UI primitives

| Component | Role in navigation |
| --------- | ------------------ |
| `ExecutiveAttentionCard` | Whole-card `RouterLink` to domain dashboard |
| `ExecutiveDomainSummaryRow` | Conditional "Details →" link |
| `CustomerAttentionList` / `SalesmanAttentionList` / etc. | Arrow button → `navigateToReport()` |
| `InventoryRiskAttentionList` | Full row click → report |
| `AlertCenterAlertTable` | Separate Dashboard and Report icon buttons |
| `*NavigationSection.vue` | Static cross-domain link grid |
| `navigateToReport.ts` | `router.push({ path, query: { q } })` |

### 3.4 Chart and KPI clickability

**No dashboard chart or KPI card navigates to a report** except Executive/Purchasing/Customer/Salesman attention cards that link to other dashboards. Weekly trends, aging pies, posting breakdowns, and concentration bar charts are **visual dead-ends**.

---

## 4. Report Capability Analysis

### 4.1 Report capability matrix

| Capability | Sales Report | Piutang Report | Inventory Report | Purchasing Report |
| ---------- | ------------ | -------------- | ---------------- | ----------------- |
| **Route** | `/reports/sales` | `/reports/piutang` | `/reports/inventory` | `/reports/purchasing` |
| **Row grain** | 1 × Faktur | 1 × open Faktur | 1 × Item × Warehouse | 1 × purchase Invoice |
| **Date range filter** | Yes (server, max 31 days) | Yes (server, max 31 days) | No (point-in-time) | Yes (server, max 31 days) |
| **Default period** | Current calendar month | Current calendar month | N/A | Current calendar month |
| **Date field selector** | No (always Faktur date) | Yes: Jatuh Tempo (default) or Piutang Date | No | No (always invoice date) |
| **Free-text search** | Yes (client-side, instant) | Yes (client-side, instant) | Yes (client-side, instant) | Yes (client-side, instant) |
| **`?q=` deep-link** | **Yes** | **Yes** | **Yes** | **No** (gap — not read on mount) |
| **Grouping** | None (flat table, column sort) | None | None (footer groups by BrgId) | None |
| **Export** | None | None | None | None |
| **Footer summary** | None | Total Piutang, Total Customer | Total Inventory Value, Total Item | Grand Total Purchase, Total Invoice |
| **Summary when filtered** | N/A | Recalculated client-side | Recalculated client-side | Recalculated client-side |
| **Pagination** | Client-side (25/page) | Client-side | Client-side | Client-side |

### 4.2 Free-text search fields

| Report | Searchable columns |
| ------ | ------------------ |
| Sales | FakturCode, CustomerName, SalesName, Status |
| Piutang | CustomerName, SalesName, FakturCode |
| Inventory | ItemDisplay (code + name), WarehouseName |
| Purchasing | InvoiceCode, SupplierName, WarehouseName, PostingStok |

Search algorithm: multi-word AND across configured fields (`reportFreeTextFilter.ts`). Search is **never sent to the API** — full dataset loaded, filtered in browser.

### 4.3 Reports as investigation destinations

| Report | Best investigation use | Reconciles with dashboard when |
| ------ | ---------------------- | ------------------------------ |
| **Sales** | Validate omzet, rep performance, customer billing, Faktur Kembali | Same calendar month, no search filter |
| **Piutang** | Validate customer/salesman open balances, due dates | All-time open balances **only if** period filter cleared to full range — default month filter **does not** match Piutang Dashboard |
| **Inventory** | Validate item/warehouse stock, dead stock item detail | Empty search; footer uses BrgId-first aggregation |
| **Purchasing** | Validate principal spend, posting backlog | Same calendar month, no search filter |

### 4.4 Report limitations affecting investigation

| Limitation | Impact |
| ---------- | ------ |
| **31-day max period** | Cannot investigate full-quarter sales or purchasing in one report view without multiple Apply cycles |
| **Client-side-only search** | Large datasets may perform poorly; search does not reduce server load |
| **No export** | Users cannot take evidence offline from portal |
| **No Faktur detail view** | Report row is terminal — no line items, payments, or movement history in portal |
| **Piutang period default** | Drill-down from all-time dashboards arrives at month-filtered report — investigator must widen period manually |
| **Purchasing `?q=` gap** | Principal drill-down from dashboards/alerts does not pre-fill search |

---

## 5. Dashboard → Report Traceability Analysis

### 5.1 Authoritative traceability chains

These chains are **documented and tested** — report footer or row sums must reconcile with dashboard KPIs under stated conditions.

| KPI | Dashboard | Report | Transaction source | Reconciliation rule |
| --- | --------- | ------ | ------------------ | ------------------- |
| Total Achievement / Total Omzet | Sales | Sales Report | `BTR_Faktur.GrandTotal` | Sum of report Total column = dashboard KPI (current month, non-void) |
| Total Piutang | Piutang | Piutang Report | Open Faktur `KurangBayar` | Footer Total Piutang = dashboard when report shows all open balances |
| Total Customer (Piutang) | Piutang | Piutang Report | Distinct customer keys | Footer Total Customer = dashboard when unfiltered |
| Total Inventory Value | Inventory, Inventory Risk | Inventory Report | `BTR_StokBalanceWarehouse` via `IStokBalanceViewDal` | Footer = dashboard (BrgId-first, exclude In-Transit) |
| Total Item | Inventory, Inventory Risk | Inventory Report | Same | Footer distinct BrgId count |
| Grand Total Purchase | Purchasing | Purchasing Report | PF1 `GrandTotal` | Footer = dashboard (current month) |
| Total Invoice | Purchasing | Purchasing Report | PF1 count | Footer = dashboard (current month) |
| Dead Stock Value | Inventory Risk | Inventory Report | Stock balance for classified items | Item search reveals underlying rows; no single footer for at-risk subset |
| Cash Collected MTD | Collection | — | FF2 pelunasan | **No report** — dashboard only |
| Recovery vs Billing % | Collection | — | FF2 + Faktur omzet | **No report** |
| Overdue Exposure | Collection | Piutang Report | Open Faktur aging | Partial — report period filter applies |
| Achievement % | Executive, Sales | Sales Report (indirect) | Faktur + targets | No single report row — derived KPI |

### 5.2 Attention signal → report routing (implemented)

Signal-to-report routing is encoded in aggregators and `DashboardAlertCenterComposer`:

| Entity / signal family | Report destination | Filter value (`?q=`) |
| ---------------------- | ------------------ | -------------------- |
| Customer — Overdue, Plafond Breach | Piutang Report | Customer name |
| Customer — Dormant, Suspended + Sales | Sales Report | Customer name |
| Salesman — High Overdue/Piutang Exposure | Piutang Report | Salesman name |
| Salesman — Below Target, No Target, Concentration, Dormant Portfolio | Sales Report | Salesman name |
| Collection — Customer/Salesman signals | Piutang Report | Entity name |
| Collection — Wilayah Hotspot | **None** | Wilayah name stored but report button hidden |
| Inventory Risk — item signals | Inventory Report | Item name (BrgName) |
| Purchasing — principal signals | Purchasing Report | Principal/supplier name |
| Location — warehouse inventory signals | Inventory Report | Warehouse name |
| Location — at-risk concentration | Inventory Risk Dashboard | No report pre-filter |
| M16 Sales Achievement | Sales Dashboard only | No report link |
| M19 summary in Alert Center | Inventory Risk Dashboard | No item rows in Alert Center |

### 5.3 Full traceability matrix (extended)

| Signal / KPI | Dashboard | Report | Transaction evidence | Desktop validation |
| ------------ | --------- | ------ | -------------------- | ------------------ |
| Sales Achievement Warning | Executive → Sales | Sales Report (manual) | Faktur | Sales Omzet Chart RO2 |
| Below Target (salesman) | Salesmen | Sales Report `?q=` | Faktur per rep | RO2 per-rep chart |
| Overdue (customer) | Customers / Collection | Piutang Report `?q=` | Open Faktur | Piutang Tracker FT5 |
| Chronic Overdue | Collection | Piutang Report `?q=` | Open Faktur >90d bucket | FT5, collection visit log |
| Dormant / Legacy Debt | Customers / Collection | Sales or Piutang Report | Last Faktur date + balance | Customer master, Faktur history |
| Dead Stock | Inventory Risk | Inventory Report `?q=` | Stock balance + Last Faktur | Faktur Brg Info, Kartu Stok |
| Qualified Backlog | Purchasing | Purchasing Report `?q=` | PF1 BELUM invoices | Posting Stok PT2 |
| Compound Dependency | Purchasing | Purchasing + Inventory Reports (manual) | PF1 + stock balance | Principal master |
| Warehouse At-Risk Concentration | Locations | Inventory Risk → Inventory Report | Stock by warehouse + item class | Kartu Stok per warehouse |
| Pending Posting (executive) | Executive → Purchasing | Purchasing Report search `BELUM` | PF1 posting flag | PT2 posting workflow |
| Faktur Kembali | — | Sales Report search `Kembali` | Faktur control status | Faktur Control |

---

## 6. Existing Search and Filter Analysis

### 6.1 Filter bar capabilities (`ReportFilterBar.vue`)

| Filter | Sales | Piutang | Inventory | Purchasing |
| ------ | ----- | ------- | --------- | ---------- |
| Period datepicker | Yes | Yes | No | Yes |
| Apply button (server reload) | Yes | Yes | N/A | Yes |
| Date field selector | No | Yes | No | No |
| Search box | Yes | Yes | Yes | Yes |
| Clear search | Yes | Yes | Yes | Yes |
| Max period | 31 days | 31 days | — | 31 days |

### 6.2 Query string patterns

| Pattern | Usage | Implemented |
| ------- | ----- | ----------- |
| `?q=<free text>` | Pre-fill report search from dashboard/alert drill-down | Sales, Piutang, Inventory yes; Purchasing **no** |
| `?redirect=<path>` | Post-login return URL | Login only |

**No structured filter query params exist** — no `?customerCode=`, `?warehouse=`, `?signalKey=`, `?from=&to=` in URLs from drill-down.

### 6.3 Semantic filter gaps

| Gap | Description |
| --- | ----------- |
| **Dashboard all-time vs report period** | Piutang/Collection/Customer dashboards use all open balances; Piutang Report defaults to current month on Jatuh Tempo |
| **Name-based `?q=` only** | Drill-down uses display names, not stable IDs (`CustomerCode`, `BrgId`, `SalesPersonId`) — risk of ambiguous matches |
| **Wilayah not searchable** | No report supports wilayah filter; M20 Wilayah Hotspot has no report drill-down |
| **Signal context not passed** | Report opens with search text only — user does not see *why* the entity was flagged |
| **Posting status not pre-filtered** | Qualified Backlog drill-down does not auto-filter `BELUM` in Purchasing Report |

---

## 7. Attention List Investigation Analysis

Attention lists are the **richest investigation entry point** in the portal. All follow the grain: **one row per entity × signal**.

### 7.1 Attention list inventory

| Dashboard | Entity grain | Signals (count) | Row action | Report routing rule |
| --------- | ------------ | --------------- | ---------- | ------------------- |
| Customer (M17) | Customer | 4 | Arrow button | Overdue/Plafond → Piutang; Dormant/Suspended → Sales |
| Salesman (M18) | Salesman | 6 | Arrow button | Overdue/Piutang exposure → Piutang; Performance signals → Sales |
| Collection (M20) | Customer, Salesman, Wilayah | 7 | Arrow (not Wilayah) | Piutang Report for customer/salesman |
| Inventory Risk (M19) | Item (BrgId) | 3 | Full row click | Inventory Report |
| Purchasing (M21) | Principal, Company | 8 | Arrow (not Company) | Purchasing Report |
| Location (M22) | Warehouse | 6 | Arrow when ReportRoute set | Inventory Report for warehouse signals |

### 7.2 Recommended investigation flow per attention list

| List | User question | Investigation steps |
| ---- | ------------- | --------------------- |
| **Customer Attention** | Why is this customer flagged? | Read Signal column → click arrow → correct report opens with name filter → sort/filter further → escalate to Desktop for collection |
| **Salesman Attention** | Why is this rep flagged? | Same — signal determines Sales vs Piutang report |
| **Collection Attention** | Who owns this receivable risk? | Priority signal shown (Chronic > Plafond+Overdue > Legacy > Overdue) → Piutang Report |
| **Inventory Risk Attention** | Why is this SKU at risk? | Signal shows Dead/Slow/Never Sold + days since last Faktur → Inventory Report shows warehouse breakdown |
| **Purchasing Attention** | Why is this principal flagged? | Signal explains backlog/concentration/dependency → Purchasing Report (when `?q=` fixed) |
| **Location Attention** | Why is this warehouse flagged? | Inactive/no-sales/concentration signal → Inventory Report by warehouse OR Inventory Risk for at-risk |

### 7.3 Attention list gaps

| Gap | Detail |
| --- | ------ |
| No signal explanation panel | Row shows label only — no expandable "rule applied" text beyond Value column |
| Duplicate rows per entity | Same customer with Overdue + Dormant = two rows (by design) — can confuse investigators |
| Rankings vs attention list | Top 10 rankings on same page are clickable; attention cards often are not |
| M23 consumes same rows | Alert Center re-surfaces capped subset — investigator may land from alert or dashboard list |

---

## 8. Executive Dashboard Investigation Analysis

### 8.1 Current executive investigation capability

| Executive element | Clickable? | Investigation path today |
| ----------------- | ---------- | ------------------------ |
| Sales Achievement Card | Yes → Sales Dashboard | Sales Dashboard has **no** report link; user must use sidebar |
| Piutang Card | Yes → Piutang Dashboard | Piutang Dashboard has **no** row drill-down |
| Purchasing Card | Yes → Purchasing Dashboard | Purchasing Attention List available on destination |
| Inventory Card | Yes → Inventory Dashboard | Inventory Dashboard has **no** drill-down |
| Critical Exposure Top 5 (×4) | **No** | User must manually find entity on Customer/Piutang/Inventory/Purchasing dashboards |
| Domain Summary rows | Details link only | Domain dashboard — same limitations as above |
| Open Alert Center | Yes → `/alerts` | Full investigation feed |

### 8.2 Desired executive investigation paths (not fully implemented)

| Card / exposure | Ideal investigation chain | Current state |
| --------------- | --------------------------- | ------------- |
| Achievement Warning | Executive → Sales → Sales Report | Partial — stops at Sales Dashboard |
| Overdue / >90d exposure | Executive → Collection or Piutang → Piutang Report | Partial — Piutang Dashboard not clickable |
| Pending Posting | Executive → Purchasing → Purchasing Report (`BELUM`) | Partial — no auto-filter |
| Inventory concentration | Executive → Inventory or Locations | Partial — Top 5 lists not clickable |
| Top 5 Customer exposure | Executive → Customer Analytics → Piutang Report | **Not linked** — user must navigate manually |
| Top 5 Principal exposure | Executive → Purchasing → Purchasing Report | **Not linked** |

### 8.3 M24 role for executive dashboard

M24 should **extend investigation from M16 without changing KPI composition.** Executive remains the "what is happening" surface; investigation links are additive metadata, not new calculations.

---

## 9. Alert Center Investigation Analysis

### 9.1 Implemented M23 investigation pattern

```text
Alert row
  ├── [Dashboard button] → owning domain dashboard (context)
  └── [Report button]    → report with ?q= pre-filter (evidence)
       └── disabled when EntityType = Wilayah or Company-level signal
```

Documented path in `btr-portal-operational.md`:

```text
Alert Center → Domain Dashboard → Report (?q= when available)
```

### 9.2 Alert Center investigation by section

| Section | Investigation behavior |
| ------- | ---------------------- |
| **Platform Alerts** | Link to Executive Dashboard or admin action — meta-investigation (data freshness) |
| **Alerts table** | Dual buttons per row; Top 20 per category cap |
| **Inventory Risk Summary** | Aggregate counts only → link to Inventory Risk Dashboard (no SKU evidence in Alert Center) |
| **Concentrations** | Informational → dashboard link only, no report |
| **Navigation** | Manual dashboard picker — escape hatch |

### 9.3 Is Alert → Dashboard → Report sufficient?

| Scenario | Sufficient? | Notes |
| -------- | ----------- | ----- |
| Single-entity customer overdue | **Yes** | Report button goes directly to Piutang Report |
| Company-level Sales Achievement Critical | **Partial** | Dashboard only — no report button (no entity filter) |
| Wilayah Hotspot | **No** | No report destination — must use Collection Dashboard |
| M19 inventory (summary) | **Partial** | Must visit Inventory Risk then item row |
| Compound Dependency | **Partial** | Dashboard only in Alert Center — cross-domain evidence requires second hop |
| Platform stale snapshot | **N/A** | Operational — not business investigation |

**Conclusion (updated per PO Q12):** Entity alerts use **Report-first** with optional **View Dashboard**. Company-level and Wilayah signals stop at dashboard. Cross-domain signals use **metadata-only multi-step sequences** (Q13). Legacy dashboards and Executive Top 5 gaps are **mandatory M24 scope** (Q1, Q2).

---

## 10. Cross-Dashboard Navigation Analysis

### 10.1 Documented cross-dashboard paths

| Origin signal | Destination | Purpose |
| ------------- | ----------- | ------- |
| M23 Alert (Collection) | M20 Collection Dashboard | Canonical overdue investigation |
| M22 Warehouse At-Risk | M19 Inventory Risk | Item-level obsolescence evidence |
| M21 Compound Dependency | M19 Inventory Risk (navigation) | Principal stock risk after purchase concentration |
| M22 Wilayah Sales concentration | M20 Collection | Receivable investigation by territory |
| M17 Collection card | M14 Piutang Dashboard | Domain receivable context |
| M18 Performance card | M11 Sales Dashboard | Domain sales context |
| M21 Purchasing navigation | M15 Inventory + M19 Inventory Risk | Cross-domain principal exposure |
| M22 Location navigation | 7 dashboards | Broad cross-domain hub |

### 10.2 Cross-domain investigation scenarios

| Scenario | Dashboards involved | Reports involved | Gap |
| -------- | ------------------- | ---------------- | --- |
| **Compound Dependency** | Purchasing → Inventory Risk | Purchasing + Inventory | No guided multi-report workflow |
| **Warehouse At-Risk Concentration** | Location → Inventory Risk | Inventory (by warehouse, then item) | Two-hop manual |
| **Chronic Overdue** | Alert/Collection → Customer | Piutang | M17/M20 dedup handled in M23; investigation path clear |
| **Principal spend + at-risk stock** | Purchasing | Purchasing + Inventory | Principal name may differ between purchase supplier and inventory supplier fields |
| **Sales decline + collection failure** | Sales + Collection | Sales + Piutang | No unified customer thread |

### 10.3 Navigation graph (simplified)

```text
                    ┌─────────────┐
                    │  Executive  │
                    └──────┬──────┘
                           │
              ┌────────────┼────────────┐
              ▼            ▼            ▼
         ┌────────┐  ┌───────────┐  ┌─────────┐
         │ Alerts │  │ Domain    │  │ Legacy  │
         └────┬───┘  │ Attention │  │ Detail  │
              │      │ Dashboards│  │ Dash    │
              │      └─────┬─────┘  └────┬────┘
              │            │             │
              └────────────┼─────────────┘
                           ▼
                    ┌─────────────┐
                    │   Reports   │
                    └──────┬──────┘
                           ▼
                    ┌─────────────┐
                    │ BTR Desktop │
                    └─────────────┘
```

---

## 11. Desktop Workflow Analysis

### 11.1 Desktop validation screens referenced in portal documentation

| Desktop screen | Module | Portal relationship | Investigation role |
| -------------- | ------ | --------------------- | ------------------ |
| **Faktur Control** | Sales | Sales Report `Kembali` status | Validate unsigned Faktur backlog — **no portal aggregate** |
| **Sales Omzet Chart (RO2)** | Sales | Richer per-rep target view than portal | Validate achievement when portal shows Warning |
| **Sales Omzet Health Weekly** | Sales admin | Excluded from portal (IT scope) | Data quality — not management investigation |
| **Piutang Tracker (FT5)** | Finance | No portal equivalent | Per-Faktur lifecycle timeline after Piutang Report identifies invoice |
| **Lunas Piutang queue** | Finance | No portal equivalent | Payment application workflow |
| **Posting Stok (PT2)** | Purchasing | Purchasing Report `PostingStok` column | Resolve `BELUM` from Qualified Backlog alerts |
| **Kartu Stok / Kartu Stok Summary (IF8)** | Inventory | M19 explicitly uses Last Faktur, not mutasi | Validate movement vs portal classification |
| **Stok Balance Health** | Inventory admin | Excluded (IT scope) | Data integrity |
| **Faktur Brg Info** | Sales/Inventory | Spot-check Last Faktur per item | Validate M19 idle days |

### 11.2 Should portal investigation continue to Desktop?

**Yes — by product scope.** `btr-portal-domain.md` Non Goals explicitly exclude transaction entry, payment recording, posting, and master data maintenance. Portal reports show **aggregated transaction headers** (Faktur, open balance row, stock row, invoice). Line items, payment history, stock mutations, and workflow state changes require Desktop.

### 11.3 Recommended investigation termination points

| Portal stops at | User continues in Desktop when |
| --------------- | ------------------------------ |
| Sales Report row | Need line items, retur, or Faktur correction |
| Piutang Report row | Need payment recording, adjustment, or FT5 audit trail |
| Inventory Report row | Need Kartu Stok movement history or stock adjustment |
| Purchasing Report row | Need PT2 posting, goods receipt, or PF2 lines |

M24 should **document Desktop handoff** per investigation path. **Desktop deep links from portal are explicitly out of scope** per ALERT-REGISTRY exclusions — unless Product Owner revises this decision.

---

## 12. Evidence Hierarchy Analysis

### 12.1 Proposed hierarchy

```text
KPI → Alert → Entity → Report → Transaction
```

### 12.2 Validation against existing capabilities

| Layer | Exists in portal? | Examples | Gaps |
| ----- | ----------------- | -------- | ---- |
| **KPI** | Yes | Executive cards, domain KPI rows | Legacy dashboards — KPI only, no forward link |
| **Alert** | Yes | M23 Alert Center, attention list rows | Company-level alerts lack entity grain |
| **Entity** | Partial | Customer, Salesman, Item, Principal, Warehouse | Wilayah, Company, System — weak entity drill-down |
| **Report** | Yes | Four reports | No report for Collection recovery KPIs |
| **Transaction** | **No** | — | Desktop only; portal row is not editable drill-down |

### 12.3 Alternate path: Alert-first

M23 implements:

```text
Alert → Dashboard → Report
```

This inserts **Dashboard (context)** between Alert and Report. Dashboard provides charts, related signals, and rankings that reports lack. **Report-first** (Alert → Report) is faster for experienced users but loses context — Alert Center already offers Report button as shortcut.

**Assessment:** Both paths are valid. M24 should treat Dashboard as **optional context layer**, not mandatory middle hop, when Report destination is unambiguous.

---

## 13. Consistency Analysis

### 13.1 Navigation inconsistencies

| Inconsistency | Examples |
| ------------- | -------- |
| **Clickable vs display-only dashboards** | M17–M22 vs M11–M15 |
| **Row click vs arrow button** | Inventory Risk uses row click; Customer uses arrow in attention list |
| **Attention card clickability** | Customer/Salesman cards partial; Collection/Purchasing/Location cards none |
| **Executive Top 5 not clickable** | Same entities clickable on M17/M20 dashboards |
| **Chart elements never clickable** | All dashboards |
| **Location hardcoded routes** | At-risk → always Inventory Risk; Wilayah → always Collection (ignores row `DashboardRoute`) |
| **Purchasing `?q=` not hydrated** | Breaks parity with other reports |
| **Navigation section presence** | Only on attention-era dashboards |

### 13.2 Terminology inconsistencies

| Topic | Variation |
| ----- | --------- |
| Receivables | "Piutang" (dashboard/report) vs "Collection" (M20 dashboard) |
| Inventory risk | "Inventory Risk", "Slow Moving & Dead Stock", "At-Risk" |
| Supplier dimension | "Supplier" (inventory), "Principal" (purchasing) |
| Attention vs Alert | "Attention List" on dashboards; "Alert" in Alert Center — same underlying signals |
| Search label | "Search" in filter bar; `?q=` param undocumented to users |

### 13.3 Filter behavior inconsistencies

| Topic | Variation |
| ----- | --------- |
| Period semantics | Current month vs all-time open vs point-in-time |
| Summary on filter | Sales has none; others recalculate |
| Apply required | Period changes need Apply; search is instant |

### 13.4 Standardization opportunities for M24 (approved)

| Opportunity | PO status | Description |
| ----------- | --------- | ----------- |
| **Investigation Metadata Contract** | **Mandatory (Q20)** | Every clickable signal exports contract fields (Section 19.8) |
| **Unified `Investigate` action** | **Mandatory (Q17)** | Same label and affordance across dashboards and Alert Center |
| **Legacy dashboard retrofit** | **Mandatory (Q1)** | M11, M14, M15 Top 10 ranking drill-down |
| **Executive exposure drill-down** | **Mandatory (Q2)** | Top 5 rows link to same destinations as domain dashboards |
| **Report deep-link parity** | **Mandatory (Q9)** | All four reports honor drill-down query params including entity IDs |
| **Investigation breadcrumb** | **Mandatory (Q4)** | Source + signal + entity on report page |
| **Signal context banner** | Nice to have | Richer context beyond breadcrumb |
| **Chart drill-down** | **Deferred (Q3)** | Not in M24 |

---

## 14. Existing Asset Discovery

### 14.1 Reusable navigation patterns

| Asset | Location | Reuse for M24 |
| ----- | -------- | ------------- |
| `navigateToReport()` | `services/navigateToReport.ts` | Core drill-down primitive — extend for structured query |
| `ReportRoute` on aggregator rows | `Dashboard*Aggregator.cs` | Already encodes signal → report routing |
| `DashboardRoute` / `ReportRoute` on alert rows | `DashboardAlertCenterComposer.cs` | Alert Center investigation contract |
| `AlertCenterRegistry` | `AlertCenterRegistry.cs` + `ALERT-REGISTRY.md` | Catalog of signal → dashboard mapping |
| `*NavigationSection.vue` | 7 components | Cross-domain link pattern |
| `ExecutiveAttentionCard` | `components/dashboard/` | Card → dashboard pattern |
| `useReportFreeTextFilter` | `composables/` | Client search from `?q=` |
| `ReportFilterBar` | `components/reports/` | Shared filter UI |
| `ReportSummaryBar` | `components/reports/` | Footer reconciliation display |

### 14.2 Reusable query string patterns

| Pattern | Status |
| ------- | ------ |
| `?q=<display name>` | De facto standard — 3/4 reports |
| `?redirect=` | Login only |

### 14.3 Reusable dashboard components

| Component | Investigation relevance |
| --------- | ---------------------- |
| `Top10RankingTable` | Used on legacy dashboards — add `clickable` + `@row-click` |
| `AlertCenterAlertTable` | Dual Dashboard/Report actions — template for other tables |
| `InventoryRiskAttentionList` | Row-click pattern |
| `CustomerAttentionList` | Arrow + `navigateToReport` pattern |
| `KpiCard` | Currently display-only everywhere |
| `DashboardDetailLayout` | Wraps all detail dashboards — candidate for breadcrumb slot |

### 14.4 Backend assets (read-only metadata — not new calculations)

| Asset | Role |
| ----- | ---- |
| Aggregator `ReportRoute` fields | Per-row investigation destination |
| `ResolveCustomerReportRoute` / `ResolveSalesmanReportRoute` | Signal-dependent report selection |
| `GetDashboard*Query` Navigation DTOs | Cross-link constants in API responses |
| `ALERT-REGISTRY.md` | Business catalog for signal traceability |

---

## 15. Dashboard Layout Proposals (Discussion Support)

Text-only illustrations of investigation flow. **Not implementation designs.**

### 15.1 Proposal A — Progressive disclosure (attention dashboard)

```text
┌─────────────────────────────────────────────────────────────┐
│ Customer Analytics                                          │
├─────────────────────────────────────────────────────────────┤
│ [Attention Cards]                                           │
│   Collection (3 overdue)  ──click──► Piutang Dashboard    │
├─────────────────────────────────────────────────────────────┤
│ [Attention List]                                            │
│   PT ABC · Chronic Overdue · Rp 45M  [► Investigate]        │
│         │                                                   │
│         ▼                                                   │
│   Piutang Report  (all open balances + entity IDs)         │
│   ┌─────────────────────────────────────────────────────┐   │
│   │ Investigating: Chronic Overdue · PT ABC              │   │
│   │ Source: Alert Center                                 │   │
│   │ Next Validation: Piutang Tracker (FT5)               │   │
│   └─────────────────────────────────────────────────────┘   │
│   Faktur rows...                                            │
│         │                                                   │
│         ▼ (Desktop handoff — documentation only)            │
│   BTR Desktop → Piutang Tracker FT5                         │
└─────────────────────────────────────────────────────────────┘
```

### 15.2 Proposal B — Executive investigation overlay

```text
┌─────────────────────────────────────────────────────────────┐
│ Management Attention Center                                 │
├─────────────────────────────────────────────────────────────┤
│ [Piutang Card — Requires Attention]                         │
│   Overdue: 12 customers · >90d: Rp 120M                     │
│   [View Dashboard]  [View Collection]  [Top Overdue ▼]      │
│         │                              │                    │
│         ▼                              ▼                    │
│   Piutang Dashboard          Customer list → Report       │
└─────────────────────────────────────────────────────────────┘
```

### 15.3 Proposal C — Alert Center fast path (approved — PO Q12)

```text
Alert: High Overdue Workload · Salesman: Budi
  [Investigate]  → Report-first (default for entity alerts)
  [View Dashboard]  → optional context

Report opens with entity IDs + display name + breadcrumb
Optional link: "View full salesman context →"
```

### 15.4 Proposal D — Cross-domain investigation (Compound Dependency)

```text
Alert: Compound Dependency · Principal: XYZ
  Step 1: Purchasing Report (?q=XYZ) — monthly purchase evidence
  Step 2: Inventory Report (?q=XYZ) — stock held
  Step 3: Inventory Risk Dashboard — at-risk exposure
  (Guided sequence — not a new KPI)
```

---

## 16. Gap Analysis

Gap items below are reclassified per Product Owner decisions (Section 19). Mandatory gaps are **core M24 outcomes** the Architect must plan for.

### 16.1 Investigation capabilities — AVAILABLE

| Capability | Where |
| ---------- | ----- |
| Entity attention lists with signal labels | M17–M22 |
| Signal → report routing in aggregators | Backend attention rows |
| `navigateToReport` with `?q=` | Customer, Salesman, Collection, Inventory Risk, Location, Alert Center |
| Cross-dashboard navigation sections | M17–M22, M23 |
| Report period + search filters | All reports (varying) |
| Alert Center dual Dashboard/Report actions | M23 |
| KPI → dashboard links on executive | M16 |
| Traceability documentation | Domain doc footer reconciliation rules |
| Alert registry catalog | ALERT-REGISTRY.md |

### 16.2 Mandatory for M24 (was partial — now approved scope)

| Capability | PO decision | M24 outcome |
| ---------- | ----------- | ----------- |
| Piutang investigation from dashboards | Q5: auto-expand to all open balances | When arriving from Customer Analytics, Collection Dashboard, or Alert Center, Piutang Report opens in **All Open Balances** mode |
| Purchasing principal drill-down | Q9: mandatory prerequisite | Purchasing Report must hydrate `?q=` on load |
| Executive Top 5 investigation | Q2: yes | Top 5 Critical Exposure rows become clickable with same destinations as domain dashboards |
| Legacy dashboard rankings | Q1: yes — retrofit M11, M14, M15 | Top 10 tables on Sales, Piutang, Inventory dashboards gain **Investigate** drill-down |
| Stable entity identification | Q6: yes | Pass `CustomerId`, `SalesmanId`, `BrgId`, `WarehouseId`, `SupplierId` in addition to display name |
| Investigation breadcrumb | Q4: yes | Report pages show source signal and entity context |
| Investigation Metadata Contract | Q20: yes — primary technical outcome | Shared metadata across dashboards and Alert Center (Section 19.8) |
| Qualified Backlog drill-down | Q7: yes | Purchasing Report auto-filters `PostingStok = BELUM` on drill-down from Qualified Backlog signal |

### 16.3 Nice to have (Architect may include if cheap)

| Capability | PO decision |
| ---------- | ----------- |
| Signal context banner on report | Q4/Q13 adjacent — breadcrumb is mandatory; richer banner optional |
| Desktop next-step guidance | Q14, Q16: yes — text only, e.g. "Next Validation: Piutang Tracker (FT5)" |
| Guided multi-step investigation | Q13: yes — metadata only, no workflow engine (e.g. Compound Dependency steps) |

### 16.4 Accepted limitations (not gaps for M24)

| Capability | PO decision |
| ---------- | ----------- |
| Wilayah Hotspot evidence | Q10: Collection Dashboard only — do **not** add Wilayah filter to Piutang Report |
| Company-level alert investigation | Q11: Dashboard-only permanently (Sales Achievement Critical, Purchasing Inactivity, etc.) |
| Entity alert default path | Q12: **Report-first** for Customer, Salesman, Warehouse, Principal, Item; **View Dashboard** as optional context |
| Chart drill-down | Q3: explicitly deferred |
| Sales Report footer totals | Q8: out of scope |
| Collection recovery KPIs | No Collection Report — Q23 confirmed excluded |

### 16.5 Investigation capabilities — NOT AVAILABLE (confirmed out of scope)

| Capability | Notes |
| ---------- | ----- |
| Portal Faktur line-item view | Out of product scope — Q23 |
| Portal payment history | Desktop FF2 |
| Kartu Stok / movement ledger in portal | Q23 — explicitly deferred |
| Desktop deep links from portal | Q15 — explicitly out of scope |
| Export for offline investigation | Q23 — non goal |
| Server-side search / pagination | Q23 — full dataset loaded |
| Collection Report | Q23 — excluded |
| Investigation history / bookmarks | Not in M24 scope |
| Role-based investigation menus | All users see same paths |
| Investigation telemetry | Q22 — out of scope |

---

## 17. Relationship to Other Milestones

M24 is a **horizontal platform capability** that **enables investigation** across milestones without replacing them.

| Milestone | Role today | M24 relationship |
| --------- | ---------- | ---------------- |
| **M16 Executive** | Company-level "what is happening" | **Mandatory:** clickable Top 5 lists; **do not** promote new KPIs (Q25) |
| **M17 Customer** | Customer attention signals | Standardize as reference investigation pattern; dedup with M20 via M23 |
| **M18 Salesman** | Salesman attention signals | Same; report routing already signal-aware |
| **M19 Inventory Risk** | Item-level obsolescence | Item → Inventory Report is canonical evidence path |
| **M20 Collection** | Receivable recovery + overdue priority | Piutang Report primary evidence; recovery KPIs remain dashboard-only |
| **M21 Purchasing** | Principal purchasing attention | Fix Purchasing `?q=`; guide cross-link to M19 |
| **M22 Location** | Warehouse/territory concentration | Standardize ranking click behavior; wilayah path to M20 |
| **M23 Alert Center** | Aggregated exception feed | **Report-first** for entity alerts; **Investigate** as primary action; company alerts Dashboard-only |
| **M11 Sales** | Legacy detail dashboard | **Mandatory retrofit:** Top 10 Salesman ranking → Sales Report |
| **M14 Piutang** | Legacy detail dashboard | **Mandatory retrofit:** Top 10 Customers → Piutang Report (all open balances) |
| **M15 Inventory** | Legacy detail dashboard | **Mandatory retrofit:** Top 10 Category/Supplier → Inventory Report |

**M24 must not:**

- Create new `SignalKey` types (governed by ALERT-REGISTRY.md) — **Q24 confirmed**
- Add snapshot tables or refresh workers
- Change classification rules (dormant 90-day, dead stock 180-day, etc.) or alert deduplication (M20 over M17, etc.)
- Add footer calculations or KPI formulas — **Q8, Q25 confirmed**
- Promote M19/M20/M22 KPIs to Executive — **Q25: navigation only**

**M24 must:**

- Unify drill-down metadata across producers (M17–M22) and consumers (M16, M23) via **Investigation Metadata Contract** — **Q20, Q21**
- Retrofit legacy dashboards M11, M14, M15 with ranking drill-down — **Q1**
- Make Executive Top 5 lists clickable — **Q2**
- Fix Purchasing Report `?q=` hydration as prerequisite — **Q9**
- Align Piutang Report period when drilling from all-time dashboards — **Q5**
- Standardize **Investigate** action label everywhere — **Q17**
- Document Desktop next-step guidance (text only) — **Q14, Q16**

---

## 18. Special Investigation Areas

### 18.1 Navigation consistency — Dashboard → Report → Transaction?

| Dashboard tier | Should follow full chain? | Exception |
| -------------- | ------------------------- | --------- |
| Attention dashboards (M17–M22) | **Yes** — already partially do | Wilayah, Company signals stop at dashboard |
| Legacy dashboards (M11, M14, M15) | **Yes** — **mandatory retrofit** (Q1) | Charts remain display-only (Q3) |
| Executive (M16) | Top 5 → entity investigation; cards → domain dashboard | Company-level KPIs Dashboard-only (Q11) |
| Alert Center (M23) | **Report-first** for entity alerts (Q12); **View Dashboard** optional | Platform alerts operational; Wilayah → Collection only (Q10) |

**Exceptions are valid** when no report dimension exists (Collection recovery KPIs) or entity grain is territorial (Wilayah → Collection Dashboard, not Piutang Report).

### 18.2 Report-first vs Dashboard-first (approved — Q12)

| Entity type | Default action | Optional context |
| ----------- | -------------- | ---------------- |
| Customer, Salesman, Warehouse, Principal, Item | **Investigate** → Report-first | **View Dashboard** |
| Company, System | **View Dashboard** only | No report |
| Wilayah | **View Dashboard** → Collection | No Piutang Report wilayah filter (Q10) |

Breadcrumb (Q4) preserves source when user needs to understand *why* without requiring a dashboard stop.

### 18.3 Cross-domain investigation (approved — Q13)

| Scenario | Recommended path | M24 role |
| -------- | ---------------- | -------- |
| **Compound Dependency** | Step 1: Purchasing Report → Step 2: Inventory Report → Step 3: Inventory Risk Dashboard | **Guided sequence — metadata only**, no workflow engine |
| **Warehouse At-Risk Concentration** | Location → Inventory Risk → Inventory Report | Fix Location ranking to use row metadata consistently |
| **Chronic Overdue** | Alert/Collection → Piutang Report | Already supported — document canonical path (M20 over M17) |
| **Sales decline + same customer overdue** | Customer Analytics → Sales Report + Piutang Report | Investigation "playlist" — two reports, same entity |
| **Principal backlog + at-risk stock** | Purchasing → Inventory Risk | Navigation section exists — add guided flow |

---

## 19. Product Owner Decisions

**All open questions resolved 2026-06-09.** Architects must treat these as authoritative requirements.

### 19.1 Scope and philosophy

| # | Decision |
| - | -------- |
| **Q1** | **Yes** — retrofit legacy dashboards M11 (Sales), M14 (Piutang), M15 (Inventory). If legacy dashboards remain dead ends, M24 fails its purpose. |
| **Q2** | **Yes** — Executive Critical Exposure Top 5 lists become clickable. Strong yes — core M24 evidence path. |
| **Q3** | **No** — charts explicitly deferred. M24 focuses on Card, List, Ranking, Alert — not chart interactions. |
| **Q4** | **Yes** — investigation breadcrumbs on report pages. Example: `Investigating: Customer PT ABC · Signal: Chronic Overdue · Source: Alert Center` |

### 19.2 Report behavior

| # | Decision |
| - | -------- |
| **Q5** | **Yes** — when arriving from Customer Analytics, Collection Dashboard, or Alert Center, Piutang Report auto-expands to **All Open Balances** (dashboard already represents all open balances). |
| **Q6** | **Yes** — pass stable entity IDs (`CustomerId`, `SalesmanId`, `BrgId`, `WarehouseId`, `SupplierId`) in addition to display name. Future-proof. |
| **Q7** | **Yes** — Qualified Backlog drill-down auto-filters Purchasing Report to `PostingStok = BELUM`. |
| **Q8** | **No** — Sales Report footer out of scope. Nice to have, not required for investigation framework. |
| **Q9** | **Mandatory** — Purchasing Report `?q=` hydration is prerequisite bug fix. Architect must not treat as optional. |

### 19.3 Navigation paths

| # | Decision |
| - | -------- |
| **Q10** | **Collection Dashboard only** for Wilayah Hotspot. Do **not** add Wilayah filter to Piutang Report. M20 owns Wilayah analytics. |
| **Q11** | **Dashboard-only** permanently for company-level alerts (e.g. Sales Achievement Critical, Purchasing Inactivity). No entity grain. |
| **Q12** | **Report-first** for entity alerts (Customer, Salesman, Warehouse, Principal, Item). Keep **View Dashboard** as optional context. |
| **Q13** | **Yes** — multi-step investigation sequences for cross-domain signals. **Metadata only** — no workflow engine. Example: Compound Dependency → Purchasing Report → Inventory Report → Inventory Risk Dashboard. |

### 19.4 Desktop boundary

| # | Decision |
| - | -------- |
| **Q14** | **Yes** — name Desktop screens in next-step guidance (e.g. "Next Validation: Piutang Tracker (FT5)"). Text only. |
| **Q15** | **No** — Desktop deep links explicitly out of scope. |
| **Q16** | **Yes** — simple Desktop reconciliation guidance per signal. No workflow automation. |

### 19.5 Terminology and UX

| # | Decision |
| - | -------- |
| **Q17** | Standardize action label **`Investigate`** everywhere. Avoid mixed arrow / Details / Open Report terminology. |
| **Q18** | **Keep both** Piutang and Collection names. Piutang = financial state; Collection = operational process. |
| **Q19** | **Keep both** Principal and Supplier. BTR treats them differently — no artificial harmonization. |

### 19.6 Platform contract

| # | Decision |
| - | -------- |
| **Q20** | **Yes** — formal **Investigation Metadata Contract**. Probably the most important technical outcome of M24. See Section 19.8. |
| **Q21** | **Yes** — mandatory standard for all future dashboard milestones. |
| **Q22** | **No** — investigation telemetry out of scope. |

### 19.7 Explicit non-goals (confirmed)

| # | Decision |
| - | -------- |
| **Q23** | **Excluded:** export, server-side search, Kartu Stok in portal, Collection Report, transaction detail pages. Keep M24 focused. |
| **Q24** | **Excluded:** alert deduplication changes. M23 ownership preserved. |
| **Q25** | **Excluded:** promoting KPIs to Executive. M24 is navigation, not analytics. |

### 19.8 Investigation Metadata Contract (product direction — Q20)

Not an implementation design. Conceptual fields the Architect should formalize:

| Field | Purpose |
| ----- | ------- |
| `SignalKey` | Which attention rule fired |
| `SignalLabel` | Human-readable signal name for breadcrumb |
| `EntityType` | Customer, Salesman, Item, Principal, Warehouse, Wilayah, Company, System |
| `EntityId` | Stable ID (`CustomerId`, `SalesmanId`, `BrgId`, `WarehouseId`, `SupplierId`) |
| `EntityName` | Display name for search pre-fill |
| `DashboardRoute` | Owning domain dashboard for context |
| `ReportRoute` | Primary evidence report |
| `SuggestedQuery` | Report query params: free text, period mode, posting filter, etc. |
| `SourceDashboard` | Where user clicked (Executive, Alert Center, Customer Analytics, …) |
| `InvestigationSteps` | Optional ordered steps for cross-domain signals (Q13) |
| `DesktopNextStep` | Optional text guidance — screen name only (Q14, Q16) |

All clickable KPI, Alert, Ranking, and Attention surfaces across M16–M23 should emit or consume this contract. Future milestones must comply (Q21).

---

## 20. Summary for Architect Handoff

M24 is an **investigation and navigation framework** layered on existing M16–M23 analytics. **No new KPIs, signals, or snapshot logic.**

### Mandatory outcomes (planning required)

| # | Outcome |
| - | ------- |
| 1 | **Investigation Metadata Contract** — shared across dashboards, Alert Center, and reports (Section 19.8) |
| 2 | **Purchasing Report `?q=` hydration** — prerequisite bug fix (Q9) |
| 3 | **Executive Top 5 drill-down** — clickable exposure lists (Q2) |
| 4 | **Legacy dashboard retrofit** — M11, M14, M15 Top 10 ranking drill-down (Q1) |
| 5 | **Investigation breadcrumb** on report pages (Q4) |
| 6 | **Stable entity ID** in drill-down query params (Q6) |
| 7 | **Piutang semantic alignment** — all open balances when drilling from all-time sources (Q5) |
| 8 | **Qualified Backlog** → Purchasing Report `BELUM` auto-filter (Q7) |
| 9 | **`Investigate` action label** standardized (Q17) |
| 10 | **Report-first** default for entity alerts; **View Dashboard** secondary (Q12) |

### Nice to have (include if cheap)

- Signal context banner (beyond mandatory breadcrumb)
- Desktop next-step and reconciliation guidance text (Q14, Q16)
- Guided multi-step investigation metadata for Compound Dependency and similar (Q13)

### Explicitly out of scope

Clickable charts · Desktop deep links · Export · Server-side search · New reports · Collection Report · Transaction detail pages · Kartu Stok in portal · Investigation telemetry · Sales Report footer · Alert deduplication changes · Executive KPI promotion

### Success test

For any KPI, Alert, Ranking, or Attention item in M16–M23, a user can answer:

1. **Why am I seeing this?** (signal + breadcrumb)
2. **Show me the evidence.** (Report-first with correct filters and IDs)
3. **What should I inspect next?** (optional dashboard context, multi-step metadata, Desktop guidance)

The portal already answers *what requires attention.* M24 makes *why*, *evidence*, and *next step* consistently answerable within four reports and eleven dashboards — stopping where BTR Desktop owns transaction truth.

---

*End of analysis. No implementation plans, APIs, database changes, or code are included per Analyst scope.*
