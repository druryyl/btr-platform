# BTR Portal Analysis — M17 Customer Analytics Dashboard

**Status:** Implemented — knowledge extracted to `docs/features/btr-portal/` (see `knowledge-extraction-report-m16-m17.md`)  
**Scope:** Business analysis only. No implementation plans, API design, database design, or code.  
**Date:** 2026-06-08 (analysis) · Product Owner decisions recorded 2026-06-08  
**Context:** BTR Portal V2 (M16 complete) introduced a management philosophy: *What requires management attention?* M17 introduces **Customer Analytics** at `/dashboard/customers` — a dedicated customer-centric view covering **Sales and Piutang** to answer: *Which customers require management attention?*

**Milestone roadmap (approved):** M17 Customer Analytics → M18 Salesman Performance → M19 Slow Moving & Dead Stock → M20 Collection Dashboard

**Reference documents:** `docs/foundation/PRODUCT.md`, `docs/foundation/DOMAIN.md`, `docs/foundation/LANDSCAPE.md`, `docs/foundation/WORKFLOW.md`, `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-operational.md`, `docs/features/btr-portal/btr-portal-architecture.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md`, `docs/archive/materialize-dashboard-data/analysis-report.md`, `docs/work/btr-portal/M16 Executive Dashboard - Analysis.md`

---

## 1. Executive Summary

BTR Portal today treats **customer** as a supporting dimension inside domain dashboards and reports — not as a first-class management lens. Piutang analytics are the strongest customer-centric capability (open balance counts, overdue customers, aging, Top 10 outstanding). Sales exposes only **Total Customer** (distinct invoiced customers this month) with no revenue ranking or segmentation. Inventory and Purchasing have **no customer dimension**. Retur, collection effectiveness, field activity, and master-data segmentation exist in **BTR Desktop** but are not exposed in the portal.

M17 answers: *Which customers require management attention?*

**All open questions resolved.** See Section 11 for authoritative Product Owner decisions.

### Key findings

| Finding | Implication |
| ------- | ----------- |
| **Piutang customer KPIs are mature** in portal snapshots and executive dashboard | Receivable concentration, overdue counts, and Top N outstanding are ready to compose into customer snapshot |
| **Sales customer analytics are thin** — reach count only, no Top Customer by omzet | M17 requires new **Customer snapshot domain** with Top 10 by omzet from `FakturView` |
| **Customer master data carries segmentation** (`Wilayah`, `Klasifikasi`, `Plafond`, `IsSuspend`) | M17 uses **Klasifikasi** and **Wilayah** only; Plafond vs exposure in scope |
| **Dormant customer rule approved** | No Faktur for **90 days** with prior transaction history — new snapshot computation |
| **Retur, Effective Call, GPS, Faktur Kembali** | **Out of M17 scope** — deferred to other milestones or excluded |
| **Collection effectiveness / DSO** | Deferred to **M20 Collection Dashboard** |
| **Executive dashboard Top 5 Customers** | **Supplemented, not replaced** — M17 adds cross-domain customer lens at `/dashboard/customers` |
| **Dedicated Customer snapshot domain approved** | New `BTR_PortalDashboardCustomer*` tables and refresh worker — not live composition |

### Approved product outcome

Deliver **Customer Analytics** at `/dashboard/customers` using **Proposal A (Customer Attention First)** layout. Materialize customer KPIs in a **dedicated Customer snapshot domain** (Sales + Piutang sources). Mandatory rankings: **Top 10 by Omzet** and **Top 10 by Piutang**. Include **Attention List** with dormant, overdue, plafond breach, and suspended-with-sales signals. **CustomerCode** on all customer rows for drill-down. Click customer → open related report with **customer pre-filter**. Navigation: Customer Dashboard → Domain Dashboard → Report. No new Customer Report.

---

## 2. Management Attention Discovery

This section identifies customer-related situations that typically require management intervention, mapped to **existing BTR capabilities**. Items marked **Portal today** are calculable from current portal snapshots or reports. Items marked **Desktop only** exist in BTR Desktop but are not in the portal. Items marked **Not available** have no implemented logic discovered in codebase or documentation.

### 2.1 Sales — customer purchase activity

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Customer stopped purchasing** | Revenue attrition; account may be lost to competitor or closed | Last `FakturDate` per customer derivable from `IFakturViewDal` | **Not available** — no KPI; data exists |
| **Rapidly declining purchase activity** | Customer buying less month-over-month | `FakturView.GrandTotal` by customer across months — requires period comparison | **Not available** — no month-over-month customer trend |
| **Large customer revenue concentration** | Dependency on few accounts for monthly omzet | `FakturView` has `CustomerCode`, `CustomerName`, `GrandTotal`; materialized-dashboard analysis notes Customer dimension as future aggregate | **Not available** in portal — data at source |
| **Customer with unsigned Faktur backlog** | Goods delivered but signed Faktur not returned (`Faktur Kembali` incomplete) | Sales Report `Status = Kembali`; `FakturView.Kembali` | **Partial** — row-level in Sales Report via search; no per-customer aggregate |
| **Suspended customer still trading** | Master data says suspended but Fakturs still issued | `CustomerModel.IsSuspend` vs active Fakturs | **Not computed** — both data sources exist |
| **Low customer reach vs prior period** | Fewer distinct customers invoiced — market contraction signal | `TotalCustomer` in sales snapshot (current month only) | **Partial** — count only; no prior-period comparison |
| **Territory customer imbalance** | One wilayah has disproportionate customer count or omzet | `FakturView.WilayahName`; `CustomerModel.WilayahName` | **Partial** — dimension on Faktur rows; no wilayah aggregate in portal |

### 2.2 Finance / Piutang — customer receivable exposure

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Customer with large outstanding balance** | Collection priority; cash tied up | Top 10 Outstanding Customers (`BTR_PortalDashboardPiutangTopCustomer`) | **Portal today** |
| **Customer with overdue balances** | Past-due debt requires collection action | `OverdueCustomer` KPI; aging buckets per customer derivable from `PiutangOpenBalanceDto` | **Portal today** (count); per-customer overdue flag derivable |
| **Chronic overdue customer** | Repeated or severely aged debt (> 90 days) | Aging bucket `> 90 Days`; executive `AgingOver90Percent` | **Portal today** — company-level; per-customer derivable from FF1/piutang rows |
| **Receivable concentration in one customer** | Single default would materially impact cash | Executive `TopCustomerPercent`; Top 10 table | **Portal today** |
| **Customer exceeding credit limit (Plafond)** | Credit policy breach | `CustomerModel.Plafond` vs sum open `KurangBayar` | **Not computed** — both values exist |
| **Customer with high retur offset against piutang** | Returns eroding receivable; quality or relationship issue | `PiutangSalesWilayahDto.Retur` | **Desktop only** (FF1 grid); not in portal report columns |
| **Regional receivable exposure by customer** | Wilayah or salesman owns the collection risk | `PiutangSalesWilayahDto.WilayahName`, `SalesName` | **Partial** — row-level in DAL; no customer×wilayah dashboard aggregate |
| **Collection lag on specific customer** | Invoice-to-payment timeline abnormal | `IPiutangTrackerDal` (per-Faktur lifecycle) | **Desktop only** (FT5) |

### 2.3 Collection — customer payment behavior

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Customer on route not visited** | Planned collection visit missed | `BTR_SalesRuteItem` + `BTR_CheckIn` | **Not computed** |
| **Ineffective field visit** | Salesperson visited but no order placed | `EffectiveCallView.IsEffectiveCall` (`OrderCount > 0`) | **Desktop only** (RO3) |
| **Customer payment history anomaly** | Unusual payment mix (cash vs giro vs retur) | `IPelunasanInfoDal` (FF4) | **Desktop only** |
| **Route-day collection queue pressure** | Many customers on today's route with open balances | `LunasPiutang2Form` (FT1) — route-day workflow | **Desktop only** — operational, not aggregate KPI |
| **Collection effectiveness declining** | Payments not keeping pace with new billing | Deferred in portal roadmap | **Not available** |

### 2.4 Retur — customer return behavior

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **High return volume customer** | Product quality, pricing, or relationship problem | `ReturJualView` — `CustomerCode`, `CustomerName`, `GrandTotal` per retur document | **Desktop only** (RF1) |
| **High return rate relative to sales** | Retur as % of customer omzet | `ReturJualView` + `FakturView` per customer | **Not computed** |
| **Retur by customer and supplier** | Principal-specific return pattern | `ReturJualBrgViewDal` (RF2) — item/supplier breakdown | **Desktop only** |

### 2.5 Master data — customer profile integrity

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Customer missing GPS coordinates** | Field sales and visit planning impaired | `CustomerLocationView.HasCoordinate` (RO4) | **Desktop only** |
| **Stale coordinates** | Location data outdated for routing | `CoordinateTimestamp` on `CustomerModel` | **Desktop only** |
| **Customer suspended but active** | Policy violation | `IsSuspend` flag | **Partial** — flag exists; no cross-check KPI |
| **Customer without classification** | Segmentation incomplete for pricing/route planning | `KlasifikasiName` on master; blank possible | **Partial** — visible in CustomerForm (SM1) |

### 2.6 Cross-domain customer situations

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **High-value customer with rising debt and declining purchases** | Combined revenue and collection risk | Requires joining sales omzet trend + piutang exposure | **Not available** — no unified customer score |
| **Customer active in sales but no open piutang** | Healthy cash customer — low collection risk | Sales Faktur + piutang open balance | **Derivable** — not surfaced |
| **Customer with piutang but no recent sales** | Legacy debt on inactive account | Last Faktur date + open balance | **Not computed** |
| **Customer concentration across sales AND piutang** | Same account dominates revenue and debt | Top customer omzet + Top customer balance | **Partial** — piutang only in portal |

### 2.7 Workflow-derived customer attention points

From `docs/foundation/WORKFLOW.md` and portal operational workflows:

| Workflow stage | When management cares about customer | Portal support today |
| -------------- | ------------------------------------ | -------------------- |
| Customer → Sales Order → Faktur | Customer buying patterns, concentration | **Weak** — reach count only |
| Faktur → Piutang → Collection → Payment | Overdue, concentration, aging | **Strong** — Piutang Dashboard + Report |
| Customer Return → Inspection → Receivable Adjustment | High-return customers | **None** in portal |
| Collection Visit → Payment | Visit effectiveness, route coverage | **None** in portal |
| Master data maintenance | Suspended, plafond, coordinates | **None** in portal analytics |

---

## 3. Existing Dashboard Reuse Analysis

### 3.1 Management Attention Center (`/dashboard`) — customer metrics today

| Metric | Source | Customer-centric? | Reuse for M17 |
| ------ | ------ | ----------------- | ------------- |
| Overdue Customer (count) | Piutang snapshot via `DashboardExecutiveComposer` | Yes — aggregate count | Headline attention card candidate |
| Top Customer % | `#1 TopCustomer.OutstandingBalance / TotalPiutang` | Yes — concentration ratio | Receivable concentration signal |
| Aging > 90 Day amount/% | Piutang aging buckets | Partial — company-level, not per customer | Context for collection crisis |
| Critical Exposures — Top 5 Customers | Truncated from `BTR_PortalDashboardPiutangTopCustomer` | Yes — ranking by balance | Direct reuse; extend to dedicated page |
| Sales Achievement % | Sales snapshot | No — company sales, not customer | Not customer analytics |
| Total Customer (overview legacy) | Sales + Piutang overview cards | Partial — breadth counts only | Supporting metric, not attention signal |

**Assessment:** Executive dashboard provides **piutang-only customer exposure**. M17 should not duplicate this blindly — it should add **sales, retur, activity, and cross-domain** customer signals the executive page omits.

### 3.2 Sales Dashboard (`/dashboard/sales`)

| KPI / section | Customer relevance | Hidden customer data | Reuse rationale |
| ------------- | ------------------ | -------------------- | --------------- |
| Total Customer | **Yes** — distinct invoiced customers in month | Computed in `DashboardSalesFakturAggregator` but **not shown on detail page UI** | Reach metric; available in snapshot (`BTR_PortalDashboardSalesKpi.TotalCustomer`) |
| Top 10 Salesman | No — salesman dimension | `FakturView` rows contain `CustomerCode`, `CustomerName`, `WilayahName`, `KlasifikasiName` per Faktur | Same aggregator pattern can produce Top 10 Customer by omzet |
| Weekly trend | No — company total | Per-customer weekly omzet derivable from same `FakturView` rows | Week grouper (`SalesOmzetChartWeekGrouper`) reusable |
| Achievement % | No | — | Not customer analytics |

**Key gap:** Sales snapshot **computes** `TotalCustomer` but does **not** persist or display Top Customer ranking. Materialized-dashboard analysis explicitly listed **Month × Customer** and **Month × Customer Classification** as future aggregates.

### 3.3 Piutang Dashboard (`/dashboard/piutang`)

| KPI / section | Customer relevance | Reuse for M17 |
| ------------- | ------------------ | ------------- |
| Total Customer | Yes — customers with open balances | Breadth of debt exposure |
| Overdue Customer | Yes — primary attention signal | Promote to customer dashboard headline |
| Top 10 Outstanding Customers | Yes — core ranking | Primary reuse asset |
| Aging Distribution | Partial — company buckets; per-customer aging derivable from same `PiutangOpenBalanceDto` rows | Bucket logic in `DashboardPiutangAggregator` is authoritative |

**Assessment:** Piutang dashboard is the **richest existing customer analytics module**. M17 should treat it as a **source**, not a replacement — customer dashboard adds sales/retur/activity dimensions piutang dashboard lacks.

### 3.4 Inventory Dashboard — no customer dimension

Inventory analytics are item × warehouse × category × supplier. **No customer metrics** to reuse.

### 3.5 Purchasing Dashboard — no customer dimension

Purchasing analytics are invoice × supplier × warehouse. **No customer metrics** to reuse.

### 3.6 Summary — dashboard reuse map

| Asset | Customer KPIs available | M17 reuse recommendation |
| ----- | ----------------------- | ------------------------ |
| Piutang snapshot + aggregator | TotalCustomer, OverdueCustomer, Top 10 balance, aging | **Primary reuse** — read existing snapshots |
| Sales snapshot + aggregator | TotalCustomer only | Extend aggregator for Top Customer omzet (same `FakturView` source) |
| Executive composer | Top 5 customers, TopCustomerPercent, overdue | Compose into customer attention section |
| Inventory / Purchasing | None | Not applicable |

---

## 4. Existing Report Reuse Analysis

### 4.1 Reports containing customer-centric information

| Report | Route | Customer fields | Customer aggregates | Drill-down role |
| ------ | ----- | --------------- | ------------------- | --------------- |
| **Sales Report** | `/reports/sales` | `CustomerName`, `SalesName`, `Status` (Kembali) | None (row-level) | Validate per-customer Faktur history; filter by customer name via search |
| **Piutang Report** | `/reports/piutang` | `CustomerName`, `SalesName`, per-Faktur balance | Footer: TotalPiutang, TotalCustomer | Validate customer exposure; sort by Jatuh Tempo for collection priority |
| **Inventory Report** | `/reports/inventory` | None | None | Not applicable |
| **Purchasing Report** | `/reports/purchasing` | None | None | Not applicable |
| **Retur Report** | **Not in portal** | — | — | Desktop RF1/RF2 only |

**Piutang report hidden data:** `PiutangSalesWilayahDto` contains `CustomerCode`, `WilayahName`, `Alamat`, `BayarTunai`, `BayarGiro`, `Retur`, `Potongan` — but portal `PiutangReportDal.MapRow` exposes only `CustomerName`, `SalesName`, `FakturCode`, dates, `TotalJual`, `KurangBayar`. Additional columns are available at DAL layer for future customer report enrichment.

### 4.2 Reports as drill-down validation for customer KPIs

| Customer KPI candidate | Drill-down report | How user validates |
| ---------------------- | ----------------- | ------------------ |
| Top customer by outstanding balance | Piutang Report | Search customer name; sum `Kurang Bayar` |
| Overdue customer count | Piutang Report | Filter period on `Jatuh Tempo`; identify past-due rows per customer |
| Top customer by monthly omzet | Sales Report | Search customer; sum `Total` for period |
| Customer with high retur | Desktop RF1 | No portal path |
| Customer Faktur Kembali backlog | Sales Report | Search customer; count rows without `Kembali` status |
| Customer payment detail | Desktop FF4 (Pelunasan Info) | No portal path |

### 4.3 KPI-to-report traceability matrix

| Customer KPI candidate | Primary data source | Validating report | Reconciliation rule | Match type |
| ---------------------- | ------------------- | ----------------- | --------------------- | ---------- |
| Total Customer (sales reach) | Sales snapshot / `DashboardSalesFakturAggregator` | Sales Report | Distinct `Customer` values in period | **Derivable** from report rows |
| Total Customer (open piutang) | Piutang snapshot / `DashboardPiutangAggregator` | Piutang Report (unfiltered all-open) | Distinct customers with `KurangBayar > 1` | **Exact** when report scope = all open |
| Overdue Customer | Piutang snapshot | Piutang Report | Count distinct customers with `JatuhTempo < today` | **Derivable** |
| Top Customer outstanding balance | `BTR_PortalDashboardPiutangTopCustomer` | Piutang Report | Group by customer, sum balances | **Derivable** |
| Top Customer % of Total Piutang | Executive composer | Piutang Report | Top 1 customer sum / footer Total Piutang | **Derivable** |
| Aging > 90 Days (company) | Piutang aging snapshot | Piutang Report | Sum balances where overdue > 90 days | **Derivable** |
| Top Customer by omzet | **Not computed** — `IFakturViewDal` | Sales Report | Group by customer, sum `GrandTotal` | **Derivable** (no dashboard KPI yet) |
| Customer retur total | **Not computed** — `IReturJualViewDal` | Desktop RF1 | Sum `GrandTotal` per customer in period | **Desktop only** |
| Customer last purchase date | **Not computed** — `IFakturViewDal` | Sales Report | Max `Date` per customer | **Derivable** |
| Effective call rate | **Not computed** — `IEffectiveCallDal` | Desktop RO3 | Count visits where `OrderCount > 0` | **Desktop only** |
| Plafond utilization | **Not computed** — `BTR_Customer` + piutang | Piutang Report + Customer master | Open balance / Plafond | **Partial** — requires master join |
| Per-customer retur on piutang | `PiutangSalesWilayahDto.Retur` | Desktop FF1 | Sum `Retur` column per customer | **Desktop only** |

**Semantic note:** Piutang Dashboard uses **all-time open balances**; Piutang Report with period filter shows a **subset**. Customer analytics must declare which semantics apply (see Open Questions).

---

## 5. Customer Risk Analysis

Customer-related risks already measurable within BTR. Threshold values are **not defined here** — only business meaning and data availability.

### 5.1 Revenue and dependency risks

| Risk | Business meaning | Measurable from | Portal today? |
| ---- | ---------------- | --------------- | ------------- |
| **Revenue concentration** | Too much monthly omzet from one customer — loss of account would materially impact sales | `FakturView` grouped by `CustomerCode`/`Customer` | **No** — dimension exists; no ranking KPI |
| **Customer dependency (combined)** | Same customer is top omzet and top debtor | Sales Faktur + Piutang open balance per customer key | **Partial** — piutang side only |
| **Declining customer purchases** | Customer buying less over time — churn signal | `FakturView` across months per customer | **No** — requires period comparison |
| **Low customer breadth** | Fewer active customers — market shrinkage | `TotalCustomer` month over month | **Partial** — current month only |

### 5.2 Receivable and collection risks

| Risk | Business meaning | Measurable from | Portal today? |
| ---- | ---------------- | --------------- | ------------- |
| **Receivable concentration** | Too much outstanding debt from one customer | Top 10 Outstanding + TopCustomerPercent | **Yes** |
| **Overdue exposure** | Customer has past-due balances | OverdueCustomer count; per-row aging | **Yes** (count); per-customer derivable |
| **Severely aged debt** | > 90 days overdue on customer | Aging buckets applied per customer | **Derivable** |
| **Credit limit breach** | Open balance exceeds `Plafond` | `CustomerModel.Plafond` vs sum `KurangBayar` | **No** |
| **Retur-heavy collection** | Customer settles debt via returns not cash | `PiutangSalesWilayahDto.Retur` | **Desktop only** |
| **Collection ineffectiveness** | New billing outpaces payments | Payment history vs new Faktur | **No** — deferred roadmap item |

### 5.3 Activity and relationship risks

| Risk | Business meaning | Measurable from | Portal today? |
| ---- | ---------------- | --------------- | ------------- |
| **Inactive / dormant customer** | Previously active customer with no recent purchases | Last `FakturDate` vs threshold | **No** |
| **Inactive with open debt** | Customer owes money but no longer buying | Last Faktur date + open balance | **No** |
| **Ineffective field visits** | Visits without orders — wasted sales effort | `EffectiveCallView` | **Desktop only** |
| **Route non-compliance** | Customer on route not checked in | `BTR_SalesRuteItem` vs `BTR_CheckIn` | **No** |
| **Missing/stale GPS** | Cannot plan or verify visits | `CustomerLocationView.HasCoordinate`, `CoordinateTimestamp` | **Desktop only** |

### 5.4 Return and quality risks

| Risk | Business meaning | Measurable from | Portal today? |
| ---- | ---------------- | --------------- | ------------- |
| **High return customer** | Excessive retur volume or value | `ReturJualView.GrandTotal` per customer | **Desktop only** |
| **High return rate** | Retur value high relative to sales | Retur + Faktur per customer | **No** |
| **Retur applied to piutang** | Returns reducing receivable instead of cash payment | `PiutangSalesWilayahDto.Retur` | **Desktop only** |

### 5.5 Master data and policy risks

| Risk | Business meaning | Measurable from | Portal today? |
| ---- | ---------------- | --------------- | ------------- |
| **Suspended customer trading** | `IsSuspend = true` but active Fakturs exist | `BTR_Customer` + `BTR_Faktur` | **No** |
| **Unclassified customer** | Missing `Klasifikasi` — pricing/route policy unclear | `KlasifikasiName` blank on master | **Partial** — data exists |
| **Territory imbalance** | Customer distribution skewed across `Wilayah` | Customer master + Faktur `WilayahName` | **Partial** — no aggregate |

### 5.6 Risk coverage summary

| Risk category | Portal readiness | Richest existing source |
| ------------- | ---------------- | ----------------------- |
| Receivable / collection | **High** | Piutang snapshot + FF1 DAL |
| Revenue / dependency | **Low** | `IFakturViewDal` / SF4 |
| Activity / dormancy | **None** | `IFakturViewDal` + `IEffectiveCallDal` |
| Retur | **None** (portal) | `IReturJualViewDal` |
| Master data policy | **Low** | `ICustomerDal` |

---

## 6. Customer Segmentation Analysis

Investigation of whether existing business data supports customer segmentation. **No new classifications invented** — only discovered capabilities.

### 6.1 Master data segmentation (`BTR_Customer` / `CustomerModel`)

| Segment dimension | Field | Source table | Used in analytics today? | Business meaning |
| ----------------- | ----- | ------------ | ------------------------ | ---------------- |
| **Wilayah (territory)** | `WilayahId`, `WilayahName` | `BTR_Wilayah` | Piutang rows (FF1); Faktur rows | Geographic/territory grouping |
| **Klasifikasi (classification)** | `KlasifikasiId`, `KlasifikasiName` | `BTR_Klasifikasi` (free-text name, user-defined) | FakturPerCustomer (SF4); Location Coverage (RO4); FakturView | User-defined customer class — **not** predefined High/Medium/Low |
| **Harga Type (price tier)** | `HargaTypeId`, `HargaTypeName` | Harga type master | CustomerForm only | Pricing tier assignment |
| **Kota (city)** | `Kota` | Customer master | CustomerChartRpt (orphan form) — Top 10 Kota by customer count | Geographic distribution |
| **Credit limit** | `Plafond` | Customer master | CustomerForm; copied to Faktur at build | Credit policy segment |
| **Suspended status** | `IsSuspend` | Customer master | CustomerForm only | Manual business flag — inactive/blocked |
| **Tax status** | `IsKenaPajak`, NPWP fields | Customer master | Compliance — not analytics | Regulatory segment |

**Important:** `BTR_Klasifikasi` is a **user-maintained lookup** (id + name, max 20 chars). BTR does **not** ship predefined "High-Value / Medium / Low" tiers. Any value-based segmentation (high/medium/low omzet) would be a **computed** segment from Faktur data, not an existing master-data classification.

### 6.2 Activity-based segmentation (computable, not pre-built)

| Segment concept | Derivable from | Existing KPI? | Notes |
| --------------- | -------------- | ------------- | ----- |
| **Active customers** | Customers with Faktur in current month | `TotalCustomer` (sales snapshot) | Month-scoped only |
| **Dormant customers** | Customers with no Faktur in N months but history exists | **No** | Requires last-purchase-date logic + threshold (PO decision) |
| **Customers with open piutang** | `KurangBayar > 1` | `TotalCustomer` (piutang snapshot) | Debt-holding segment |
| **Overdue customers** | Past-due balance | `OverdueCustomer` | Already computed |
| **Effective-call customers** | Visits with `OrderCount > 0` | **No** | Desktop RO3 |

### 6.3 Value-based segmentation (computable, not pre-built)

| Segment concept | Derivable from | Existing KPI? | Notes |
| --------------- | -------------- | ------------- | ----- |
| **High-value by omzet** | Top N customers by `SUM(GrandTotal)` | **No** ranking | Same pattern as Top Salesman |
| **High-value by outstanding balance** | Top N customers by `SUM(KurangBayar)` | Top 10 Outstanding | **Exists** |
| **High-return customers** | Top N by retur `GrandTotal` | **No** | Desktop RF1 data |
| **Strategic customers** | **No explicit flag** in domain | — | Would require PO definition (e.g., top omzet + top debt) |

### 6.4 Route-based segmentation

| Segment | Source | Analytics? |
| ------- | ------ | ---------- |
| **Route membership** | `BTR_SalesRuteItem` — customer assigned to sales route by day | Used in FT1 collection workflow; **no portal analytics** |
| **Check-in history** | `BTR_CheckIn` | RO1 list form; **no aggregate** |

### 6.5 Segmentation summary

| Classification type | Exists in BTR? | Portal exposed? |
| ------------------- | -------------- | --------------- |
| User-defined Klasifikasi | **Yes** (master data) | **No** |
| Wilayah territory | **Yes** | **Partial** (row-level only) |
| Harga Type | **Yes** | **No** |
| IsSuspend | **Yes** | **No** |
| Active (monthly) | **Computable** | **Partial** (count only) |
| Dormant | **Computable** | **No** |
| High/Medium/Low value | **Not predefined** | **No** |
| ABC customer class | **Not in codebase** | **No** |

---

## 7. Existing Asset Discovery

Objective: maximize reuse and avoid new business calculations when equivalent logic already exists.

### 7.1 Portal snapshot layer (primary reuse)

| Asset | Path | Reusable for M17 |
| ----- | ---- | ---------------- |
| `DashboardPiutangAggregator` | `btr.application/ReportingContext/DashboardSnapshotAgg/Services/DashboardPiutangAggregator.cs` | Customer key resolution, aging, overdue count, Top N pattern — **authoritative** |
| `DashboardSalesFakturAggregator` | `btr.application/ReportingContext/DashboardSnapshotAgg/Services/DashboardSalesFakturAggregator.cs` | `TotalCustomer` count; extend for Top Customer omzet using same `ResolveCustomerKey` pattern |
| `DashboardExecutiveComposer` | `btr.application/ReportingContext/DashboardExecutiveAgg/Services/DashboardExecutiveComposer.cs` | TopCustomerPercent, Top 5 truncation, concentration ratio pattern |
| `DashboardPiutangDal` / snapshot reader | `btr.infrastructure/ReportingContext/DashboardPiutangAgg/` | Read existing piutang customer snapshots |
| `RefreshDashboardPiutangSnapshotWorker` | `btr.application/ReportingContext/DashboardSnapshotAgg/UseCases/` | Refresh cadence unchanged |
| `RefreshDashboardSalesSnapshotWorker` | Same folder | Sales customer extension would refresh with sales domain |
| `SalesOmzetChartWeekGrouper` | `btr.application/SalesContext/SalesOmzetAgg/Services/` | Weekly bucketing if customer weekly trend needed |
| `SalesOmzetChartAchievementPolicy` | `btr.application/SalesContext/SalesOmzetAgg/Policies/` | Percent calculation pattern |
| `Top10RankingTable.vue` | `btr.portal.web/src/components/dashboard/Top10RankingTable.vue` | Generic ranking UI for customer tables |
| `KpiCard.vue` / `DashboardDetailLayout.vue` | `btr.portal.web/src/components/` | Dashboard shell pattern |

### 7.2 Snapshot tables with customer data

| Table | Customer fields | M17 reuse |
| ----- | --------------- | --------- |
| `BTR_PortalDashboardSalesKpi` | `TotalCustomer` | Monthly reach |
| `BTR_PortalDashboardPiutangKpi` | `TotalCustomer`, `OverdueCustomer` | Debt breadth and urgency |
| `BTR_PortalDashboardPiutangTopCustomer` | `Rank`, `CustomerName`, `OutstandingBalance` | Top N receivable ranking |
| `BTR_PortalDashboardPiutangAging` | Bucket amounts (company-level) | Company aging context |

**Gap:** No `BTR_PortalDashboardSalesTopCustomer` table. Materialized-dashboard analysis listed **Month × Customer** as priority future aggregate. M17 may require new snapshot table **or** live query from `IFakturViewDal` at refresh time.

**Gap:** Top customer snapshot stores `CustomerName` only — **no `CustomerCode`**. Drill-down to reports may require name search until code is persisted.

### 7.3 Portal report layer (drill-down)

| Asset | Path | Reuse |
| ----- | ---- | ----- |
| `SalesReportDal` | `btr.infrastructure/ReportingContext/SalesReportAgg/SalesReportDal.cs` | Wraps `IFakturViewDal` — customer column per Faktur |
| `PiutangReportDal` | `btr.infrastructure/ReportingContext/PiutangReportAgg/PiutangReportDal.cs` | Wraps `IPiutangSalesWilayahDal` — customer balance detail |
| `ReportPeriodValidator` | `btr.application/ReportingContext/Shared/` | Period filter rules for drill-down |
| `PiutangReportView.vue` / `SalesReportView.vue` | `btr.portal.web/src/views/reports/` | Client-side search by customer name |

### 7.4 Desktop DALs — strongest M17 candidates (not in portal)

| Asset | Path | Customer signal |
| ----- | ---- | --------------- |
| `IFakturViewDal` / `FakturView` | `btr.application/SalesContext/FakturInfo/FakturView.cs` | Omzet, Klasifikasi, Wilayah, Kembali status per Faktur |
| `IFakturPerCustomerDal` | `btr.infrastructure/SalesContext/FakturPerCustomerRpt/FakturPerCustomerDal.cs` | Line-item sales per customer (SF4) |
| `IPiutangSalesWilayahDal` | `btr.application/FinanceContext/PiutangAgg/Contracts/IPiutangSalesWilayahDal.cs` | Richest receivable row: Sales, Wilayah, Retur, payment splits |
| `PiutangOpenBalanceDal` | `btr.infrastructure/FinanceContext/PiutangAgg/` | Fast open-balance scan for piutang snapshot |
| `ICustomerDal` / `CustomerDal` | `btr.infrastructure/SalesContext/CustomerAgg/CustomerDal.cs` | Master data: Plafond, IsSuspend, Klasifikasi, Wilayah; `ListLocation()` for GPS |
| `IReturJualViewDal` | `btr.application/InventoryContext/ReturJualAgg/Contracts/IReturJualViewDal.cs` | Retur per customer (RF1) |
| `IReturJualBrgViewDal` | `btr.infrastructure/InventoryContext/ReturJualRpt/` | Retur line items per customer (RF2) |
| `IEffectiveCallDal` | `btr.application/SalesContext/CheckInFeature/IEffectiveCallDal.cs` | Visit → order conversion (RO3) |
| `IPelunasanInfoDal` | `btr.infrastructure/FinanceContext/PiutangAgg/PelunasanInfoDal.cs` | Payment detail per customer (FF4) |
| `IPiutangTrackerDal` | `btr.infrastructure/FinanceContext/PiutangAgg/` | Collection lifecycle per Faktur (FT5) |
| `ISalesOmzetDal` | `btr.application/SalesContext/OrderFeature/` | Order/Faktur pipeline with customer snapshot fields |
| `SalesOmzetChartSummaryBuilder` | `btr.application/SalesContext/SalesOmzetAgg/Services/` | Top-N and week grouping pattern (salesman today; adaptable) |

### 7.5 Desktop forms (business workflow reference — not APIs)

| Form | Menu ID | Purpose |
| ---- | ------- | ------- |
| `CustomerForm` | SM1 | Customer master maintenance |
| `FakturPerCustomerForm` | SF4 | Line-item sales by customer |
| `PiutangSalesWilayahForm` | FF1 | Full receivable grid with Wilayah/Sales/Retur |
| `EffectiveCallInfoForm` | RO3 | Field visit effectiveness |
| `LocationCoverageInfoForm` | RO4 | GPS coordinate coverage |
| `ReturJualReportForm` | RF1 | Retur header per customer |
| `ReturJualBrgReportForm` | RF2 | Retur lines per customer |
| `PelunasanInfoForm` | FF4 | Payment history per customer |
| `LunasPiutang2Form` | FT1 | Route-day collection workflow |
| `CustomerChartRpt` | *(orphan — not in menu)* | Top 10 Kota by customer count |

### 7.6 Business rules (authoritative — do not reimplement)

From `btr-portal-domain.md` and aggregators:

| Rule | Applies to |
| ---- | ---------- |
| Customer key: `CustomerCode` when non-empty, else `CustomerName` | Piutang + Sales aggregators |
| Open balance threshold: `KurangBayar > 1` | Piutang |
| Aging bucket boundaries (inclusive) | Piutang |
| Top N = 10 (executive truncates to 5) | Rankings |
| Void exclusion: `VoidDate = '3000-01-01'` | Sales Faktur queries |
| Sales period: current calendar month | Sales dashboard |
| Piutang period: all-time open snapshot | Piutang dashboard |
| Faktur Kembali: `StatusFaktur == 2` | Sales report status |
| Effective call: `OrderCount > 0` | RO3 |
| DAL reuse: portal must not reimplement SQL | All reports and snapshots |

---

## 8. Exception-Based Management Analysis

Focus: customer situations that **deserve management attention** rather than displaying customer statistics. Threshold values are **not chosen here** — except where Product Owner approved specific rules (Section 11).

**Approved for M17 Attention List (Section 11.3):** W-C01 (overdue) · W-C04 (dormant — **90 days** with prior history) · W-C07 (plafond breach) · W-C08 (suspended + active sales). **Excluded from M17:** W-C05, W-C06, W-C09, W-C10, W-C11. Concentration shown as metrics without thresholds (W-C02 context).

### 8.1 Warning condition candidates

| ID | Condition candidate | Business meaning | Data source | Computed today? |
| -- | ------------------- | ---------------- | ----------- | --------------- |
| W-C01 | Customer has any overdue balance | Collection follow-up needed | Piutang rows per customer | **Derivable** from snapshot source rows |
| W-C02 | Customer in Top 1 receivable concentration | Single-account dependency | TopCustomerPercent (executive) | **Yes** (company top 1; not per-customer flag on list) |
| W-C03 | Customer balance in > 90 Days bucket | Severely stale debt on account | Per-customer aging from piutang rows | **Derivable** |
| W-C04 | Customer with no Faktur in **90 days** (with prior history) | Dormant account | `IFakturViewDal` max date | **Approved** — M17 will implement |
| W-C05 | Customer omzet declined vs prior month (TBD %) | Purchase deceleration | Faktur month comparison | **No** |
| W-C06 | Customer retur value in period > TBD | High return behavior | `IReturJualViewDal` | **No** |
| W-C07 | Open balance > Plafond | Credit limit breach | Customer master + piutang | **No** |
| W-C08 | IsSuspend but Faktur in current month | Policy violation | Customer + Faktur join | **No** |
| W-C09 | Visit with OrderCount = 0 | Ineffective call | `IEffectiveCallDal` | **Desktop only** |
| W-C10 | Missing GPS coordinates | Field ops impaired | `ICustomerDal.ListLocation()` | **Desktop only** |
| W-C11 | Faktur not Kembali for customer (aggregate) | Document return backlog | Sales Report status | **No** aggregate |
| W-C12 | Overdue Customer count > 0 (company) | At least one customer needs collection | Piutang KPI | **Yes** |

### 8.2 Critical condition candidates

| ID | Condition candidate | Business meaning | Data source | Computed today? |
| -- | ------------------- | ---------------- | ----------- | --------------- |
| C-C01 | Single customer > TBD % of Total Piutang | Major default exposure | Top customer / total | **Partial** — ratio computed for #1 |
| C-C02 | Single customer > TBD % of monthly omzet | Revenue dependency | Faktur grouped by customer | **No** |
| C-C03 | Customer with > 90 day balance > TBD absolute | Large irrecoverable debt | Per-customer aging | **Derivable** |
| C-C04 | Dormant customer with open balance > TBD | Zombie debt | Last Faktur + open balance | **No** |
| C-C05 | Retur rate > TBD % of customer omzet | Systemic quality/relationship failure | Retur + Faktur | **No** |
| C-C06 | Plafond exceeded by > TBD % | Severe credit policy breach | Master + piutang | **No** |

### 8.3 Concentration indicators

| Indicator | Numerator / denominator | Available? |
| --------- | ----------------------- | ---------- |
| Top 1 customer % of Total Piutang | Top balance / Total Piutang | **Yes** (executive) |
| Top 5 customers % of Total Piutang | Sum top 5 / total | **Derivable** from snapshot |
| Top 1 customer % of monthly omzet | Top omzet / Total Omzet | **No** — needs sales top customer |
| Top N customers % of Total Customer (reach) | Concentration of activity | **No** |
| Customer count per Wilayah | Distribution skew | **Derivable** from master or Faktur |

### 8.4 Trend indicators

| Trend | Data available | Historical retention? |
| ----- | -------------- | --------------------- |
| Customer monthly omzet trend | `FakturView` by month | **Not retained** in snapshots — live query or new history |
| Customer open balance trend | Piutang snapshot | **Point-in-time only** — no history |
| Customer retur trend | `ReturJualView` by month | **Not in portal** |
| Active customer count month-over-month | `TotalCustomer` sales KPI | **Current month only** |
| Effective call rate trend | RO3 data | **Desktop only** |

### 8.5 Ranking indicators (existing and candidate)

| Ranking | Already in system | Customer attention use |
| ------- | ----------------- | ---------------------- |
| Top 10 Outstanding Customers | **Yes** (piutang snapshot) | Collection priority |
| Top 5 Customers (executive) | **Yes** | Executive exposure |
| Top 10 Customer by omzet | **No** | Revenue priority |
| Top 10 Customer by retur value | **No** | Quality/relationship review |
| Bottom customers by omzet (active base) | **No** | Underperforming accounts |
| Customers by days since last purchase | **No** | Inactivity priority |

---

## 9. Dashboard Layout — Approved (Proposal A)

**Product Owner decision:** Use **Proposal A — Customer Attention First**. Section order is fixed per additional product direction:

1. Customer Attention Cards
2. Customer Attention List
3. Top Customer Rankings
4. Segmentation Summary
5. Navigation to supporting dashboards and reports

### 9.1 Approved page structure

**Route:** `/dashboard/customers`  
**Title:** Customer Analytics  
**Audience:** Management, Sales Manager, Collection Manager (all authenticated users — no role-based routing)

### 9.2 Approved wireframe

```text
+====================================================================+
|  CUSTOMER ANALYTICS                           [Refresh]            |
|  Which customers require management attention?                       |
|  Last Refreshed: YYYY-MM-DD HH:mm                                   |
+====================================================================+
|  STALENESS BANNER (when applicable)                                 |
+====================================================================+
|  1. CUSTOMER ATTENTION CARDS                                        |
|  +----------------+ +----------------+ +----------------+          |
|  | COLLECTION     | | CONCENTRATION  | | ACTIVITY       |          |
|  | Overdue: N     | | Top Omzet: X%  | | Active: N      |          |
|  | >90d exp: Rp   | | Top Piutang: Y%| | (month)        |          |
|  +----------------+ +----------------+ +----------------+          |
|  +----------------+ +----------------+                              |
|  | INACTIVITY     | | CREDIT         |                              |
|  | Dormant: N     | | Plafond breach |                              |
|  | (90-day rule)  | | Suspended+sales|                              |
|  +----------------+ +----------------+                              |
+====================================================================+
|  2. CUSTOMER ATTENTION LIST (required)                              |
|  CustomerCode | Customer | Signal | Value | Wilayah | [→ report]   |
|  Signals: Overdue | Dormant | Plafond breach | Suspended+active   |
+====================================================================+
|  3. TOP CUSTOMER RANKINGS (Top 10 each — mandatory)                 |
|  [Top 10 by Omzet — current month] | [Top 10 by Piutang — all open] |
|  CustomerCode | CustomerName | Amount | % of domain total           |
|  Click row → report with customer pre-filter                        |
+====================================================================+
|  4. SEGMENTATION SUMMARY                                            |
|  By Klasifikasi (existing master values) | By Wilayah (required)   |
|  Active vs Dormant counts (summary section)                         |
+====================================================================+
|  5. NAVIGATION TO DETAILS                                           |
|  → Sales Dashboard → Sales Report                                   |
|  → Piutang Dashboard → Piutang Report                               |
+====================================================================+
```

**Attention presentation:** Generic **Attention Indicator** model (same as M16). Concentration metrics shown **without warning thresholds** — management interprets visually.

**Excluded from customer dashboard:** Retur rankings, Effective Call, GPS coverage, Faktur Kembali aggregates, declining purchase activity, Harga Type segmentation, historical trend charts, new Customer Report.

### 9.3 Navigation (approved)

```text
Sidebar:
  Dashboard
   ├─ Executive (default)  → /dashboard
   ├─ Sales                → /dashboard/sales
   ├─ Piutang              → /dashboard/piutang
   ├─ Customers            → /dashboard/customers    ← M17
   ├─ Inventory            → /dashboard/inventory
   └─ Purchasing           → /dashboard/purchasing

Drill-down path:
  Customer Analytics → Domain Dashboard → Report

Customer row click:
  → Sales Report or Piutang Report with customer pre-filter applied
```

Executive Dashboard Top 5 Customers section **unchanged** — M17 supplements, does not replace.

### 9.4 Proposals not selected (reference only)

| Proposal | Status |
| -------- | ------ |
| B — Customer 360 Summary | Not selected — too statistics-heavy |
| C — Collection-Centric | Not selected — insufficient sales/customer breadth |

---

## 10. Gap Analysis

### 10.1 Information already available (reuse without new business logic)

| Information | Source | M17 use |
| ----------- | ------ | ------- |
| Total Customer with open piutang | Piutang snapshot KPI | Attention card / segmentation |
| Overdue Customer count | Piutang snapshot KPI | Attention card |
| Top 10 customers by outstanding balance | `BTR_PortalDashboardPiutangTopCustomer` | Seed ranking logic — extend with CustomerCode |
| Five aging bucket amounts (company-level) | Piutang aging snapshot | >90d exposure card |
| Top 1 customer % of Total Piutang | Executive composer pattern | Concentration card (piutang) |
| Total Customer invoiced (current month) | Sales snapshot KPI | Active customer count |
| Per-Faktur customer sales detail | Sales Report via `IFakturViewDal` | Drill-down validation |
| Per-Faktur open balance detail | Piutang Report via `IPiutangSalesWilayahDal` | Drill-down validation |
| Customer key resolution logic | `DashboardPiutangAggregator` / `DashboardSalesFakturAggregator` | Authoritative — reuse in Customer aggregator |
| Faktur dimensions: Wilayah, Klasifikasi on each row | `FakturView` | Segmentation summary |
| Customer master: Plafond, IsSuspend, Klasifikasi, Wilayah | `ICustomerDal` / `CustomerModel` | Plafond breach, suspended signal, segmentation |
| Ranking UI components | `Top10RankingTable.vue`, `KpiCard.vue` | Dashboard presentation |

### 10.2 Information partially available (M17 will implement in Customer snapshot domain)

| Information | What exists | M17 treatment |
| ----------- | ----------- | ------------- |
| **Top 10 Customer by omzet** | `FakturView` has customer + GrandTotal | **In scope** — new Customer snapshot table |
| **Top 10 Customer by piutang** | Piutang top customer snapshot (name only) | **In scope** — new table with CustomerCode |
| **Concentration % (omzet and piutang)** | Numerator/denominator derivable | **In scope** — presentation logic, no thresholds |
| **Dormant customer count and list** | Last Faktur date derivable | **In scope** — 90-day rule with prior history |
| **Active vs Dormant segmentation** | Active = Faktur in current month | **In scope** — summary + attention list |
| **Per-customer overdue flag** | Piutang rows have JatuhTempo | **In scope** — attention list signal |
| **Plafond vs open balance** | Master Plafond + piutang sum | **In scope** — attention list signal |
| **Suspended + active Faktur** | `IsSuspend` + current-month Faktur | **In scope** — attention list signal |
| **Segmentation by Klasifikasi** | Master lookup values | **In scope** — summary counts |
| **Segmentation by Wilayah** | Master + Faktur/piutang rows | **In scope** — summary counts |
| **Customer pre-filter on report click** | Report search exists | **In scope** — navigation enhancement |
| **CustomerCode on rankings** | Code in source DTOs | **In scope** — persist in new snapshot tables |

### 10.3 Information not in M17 scope (deferred or excluded)

| Information | Disposition |
| ----------- | ----------- |
| Retur per customer / high return analysis | **Excluded** (Q7, Q19) |
| Effective Call / field visit effectiveness | **Excluded** (Q8) |
| GPS / coordinate coverage | **Excluded** (Q9) |
| Faktur Kembali per customer aggregate | **Excluded** (Q11) |
| Rapidly declining purchase activity | **Excluded** (Q15) |
| Collection effectiveness / DSO / payment trend | **Deferred** — M20 Collection Dashboard (Q12, Q42) |
| Harga Type segmentation | **Excluded** (Q23) |
| Strategic customer concept | **Excluded** (Q25) |
| Historical customer trend (month-over-month) | **Excluded** (Q41) |
| New Customer Report route | **Excluded** — dashboard only (Q13) |
| Additional Piutang Report columns | **Excluded** (Q37) |
| Portal Retur Report | **Excluded** (Q38) |
| ABC / LTV customer classification | Not in BTR domain |
| Route visit compliance | Not in scope |
| Customer 360 unified score | Not in scope |
| Slow Moving & Dead Stock analytics | **Separate milestone** — M19 (Q43) |
| Report filtering phase dependency | M17 ships independently (Q44) |

---

## 11. Final Product Decisions (Authoritative)

**Status:** All open questions resolved by Product Owner — 2026-06-08.  
**Audience:** Architect — use this section as scope input. Do not re-decide business rules listed here.

### 11.1 Product positioning and audience

| # | Decision |
| - | -------- |
| Q1 | **New dedicated route:** `/dashboard/customers` |
| Q2 | **Page title:** Customer Analytics |
| Q3 | **Audience:** Management, Sales Manager, Collection Manager (all authenticated users — no role-based routing in M17) |
| Q4 | **Supplements** Executive Dashboard Top 5 Customers — does **not** replace |
| Q5 | **Milestone order:** M17 Customer Analytics → M18 Salesman Performance → M19 Slow Moving & Dead Stock → M20 Collection Dashboard |

### 11.2 Scope boundaries

| # | Decision |
| - | -------- |
| Q6 | **Sales and Piutang only** — no Retur, Collection, Inventory, or Purchasing customer analytics |
| Q7 | **Retur per customer:** No |
| Q8 | **Effective Call:** No |
| Q9 | **GPS / Coordinate Coverage:** No |
| Q10 | **Plafond vs Exposure:** Yes |
| Q11 | **Faktur Kembali per customer:** No |
| Q12 | **Collection Effectiveness:** Deferred to M20 Collection Dashboard |
| Q13 | **Dashboard only** — no new Customer Report |

### 11.3 Attention rules and indicators

| # | Decision |
| - | -------- |
| Q14 | **Dormant Customer** = no Faktur for **90 days** AND has historical transaction (prior Faktur exists) |
| Q15 | **Rapidly declining purchase activity:** No |
| Q16 | **Concentration metrics** shown without warning thresholds — management interprets visually |
| Q17 | **Any overdue balance** on a customer is attention-worthy |
| Q18 | **Suspended customer with active sales** (`IsSuspend` + Faktur in current month) is an attention signal |
| Q19 | **High Return Customer analysis:** No |
| Q20 | **Generic Attention Indicator** model (same as M16) — no customer-specific severity bands |

**Approved attention list signals:** Overdue balance · Dormant (90-day rule) · Plafond breach (open balance > Plafond) · Suspended with active sales

### 11.4 Segmentation and classification

| # | Decision |
| - | -------- |
| Q21 | Use existing **Klasifikasi** master values only — no computed High/Medium/Low tiers |
| Q22 | **Wilayah segmentation required** on customer dashboard |
| Q23 | **Harga Type segmentation:** No |
| Q24 | **Active vs Dormant** in both segmentation summary and attention list |
| Q25 | **Strategic Customer** concept not introduced |

### 11.5 Data semantics and periods

| # | Decision |
| - | -------- |
| Q26 | **Piutang metrics:** all-time open balance (`KurangBayar > 1`) — consistent with Piutang Dashboard |
| Q27 | **Sales metrics:** current calendar month — consistent with Sales Dashboard |
| Q28 | **Sales ranking:** Faktur `GrandTotal` — not `BTR_SalesOmzet` pipeline |
| Q29 | **CustomerCode included** on customer rows for drill-down support |
| Q30 | **CustomerCode-first matching** approved for cross-domain joins (sales ↔ piutang ↔ master) |

### 11.6 Rankings and layout

| # | Decision |
| - | -------- |
| Q31 | **Mandatory rankings:** Top 10 Customer by Omzet · Top 10 Customer by Piutang |
| Q32 | **Top N = 10** for all customer rankings |
| Q33 | **Layout Proposal A** — Customer Attention First (fixed section order per Additional Product Direction) |
| Q34 | **Attention List required** — not rankings alone |
| Q35 | **Navigation:** Customer Dashboard → Domain Dashboard → Report (M16 pattern) |

**Fixed section order:**

1. Customer Attention Cards  
2. Customer Attention List  
3. Top Customer Rankings  
4. Segmentation Summary  
5. Navigation to supporting dashboards and reports

### 11.7 Drill-down and navigation

| # | Decision |
| - | -------- |
| Q36 | **Click customer row** → open related report (Sales or Piutang) with **customer pre-filter applied** |
| Q37 | **No additional Piutang Report columns** for M17 |
| Q38 | **Retur Report not required** for M17 |

### 11.8 Materialization and performance

| # | Decision |
| - | -------- |
| Q39 | Customer KPIs **materialized** in snapshot tables (`BTR_PortalDashboardCustomer*`) |
| Q40 | **Dedicated Customer snapshot domain** with its own refresh process/worker |
| Q41 | **Historical trend analysis:** No |

### 11.9 Relationship to other milestones

| # | Decision |
| - | -------- |
| Q42 | **Collection Dashboard** remains separate future milestone (M20) |
| Q43 | **Slow Moving & Dead Stock** remains independent milestone (M19) |
| Q44 | M17 **ships independently** — does not wait for report enhancement work |

### 11.10 Approved M17 KPI and snapshot candidates

Based on approved scope, the Customer snapshot domain should materialize at minimum:

| Category | KPI / data | Source semantics |
| -------- | ---------- | ---------------- |
| Attention cards | Overdue customer count | Piutang — all-time open |
| Attention cards | > 90 day exposure amount | Piutang aging per customer / company |
| Attention cards | Top customer % of omzet | Sales — current month |
| Attention cards | Top customer % of piutang | Piutang — all-time open |
| Attention cards | Active customer count | Sales — current month distinct |
| Attention cards | Dormant customer count | 90-day rule with prior history |
| Attention cards | Plafond breach count | Master Plafond vs open balance |
| Attention cards | Suspended-with-sales count | `IsSuspend` + current-month Faktur |
| Rankings | Top 10 by Omzet (CustomerCode, name, amount, %) | Sales Faktur GrandTotal |
| Rankings | Top 10 by Piutang (CustomerCode, name, amount, %) | Piutang open balance |
| Attention list | Customer rows with signal type(s) | Composite of approved signals |
| Segmentation | Counts by Klasifikasi | Master + active/dormant context |
| Segmentation | Counts by Wilayah | Master + active/dormant context |
| Segmentation | Active vs Dormant counts | 90-day rule + current-month active |

**Architect determines** exact table shapes, refresh cadence, and aggregator decomposition — not re-decided here.

---

## Appendix A — Customer Concept in BTR Business Areas

```text
                    CUSTOMER
                        │
        ┌───────────────┼───────────────┬──────────────┐
        │               │               │              │
     Master Data       Sales          Finance       Inventory
        │               │               │              │
   Wilayah          Faktur           Piutang         Retur
   Klasifikasi      Sales Order      Payment         (per customer)
   Plafond          Faktur Kembali   Collection
   IsSuspend        Check-in         Pelunasan
   Coordinates      Effective Call
        │               │               │              │
        └───────────────┴───────────────┴──────────────┘
                        │
              BTR Portal today:
              Piutang ████████░░  Sales ██░░░░░░░░
              Retur   ░░░░░░░░░░  Collection ░░░░░░░░░░
              Master  ░░░░░░░░░░  Field activity ░░░░░░░░░░
```

---

## Appendix B — Portal vs Desktop Customer Capability Matrix

| Capability | Portal | Desktop |
| ---------- | ------ | ------- |
| Top customer by outstanding balance | Yes | FF1 |
| Overdue customer count | Yes | FF1 (derivable) |
| Aging distribution | Yes (company) | FF1 (per row) |
| Top customer by omzet | No | SF4, RO2 |
| Total customer reach (month) | Yes (snapshot) | RO2 |
| Customer segmentation (Klasifikasi) | No | SF4, SM1, RO4 |
| Wilayah breakdown | No | FF1, SF4 |
| Retur per customer | No | RF1, RF2 |
| Payment history | No | FF4 |
| Collection route workflow | No | FT1 |
| Effective call | No | RO3 |
| GPS coverage | No | RO4 |
| Plafond vs exposure | No | SM1 (manual compare) |
| Piutang tracker | No | FT5 |
| Customer per Kota chart | No | CustomerChartRpt (orphan) |

---

## Appendix C — File and Endpoint Index (discovery reference)

| Category | Location |
| -------- | -------- |
| Domain definitions | `docs/features/btr-portal/btr-portal-domain.md` |
| User workflows | `docs/features/btr-portal/btr-portal-operational.md` |
| Snapshot domain rules | `docs/features/materialized-dashboard/materialized-dashboard-domain.md` |
| Future customer dimensions | `docs/archive/materialize-dashboard-data/analysis-report.md` §6.2–6.3 |
| M16 executive analysis | `docs/work/btr-portal/M16 Executive Dashboard - Analysis.md` |
| Piutang aggregator | `btr.application/ReportingContext/DashboardSnapshotAgg/Services/DashboardPiutangAggregator.cs` |
| Sales aggregator | `btr.application/ReportingContext/DashboardSnapshotAgg/Services/DashboardSalesFakturAggregator.cs` |
| Executive composer | `btr.application/ReportingContext/DashboardExecutiveAgg/Services/DashboardExecutiveComposer.cs` |
| Piutang top customer table | `btr.sql/Tables/ReportingContext/BTR_PortalDashboardPiutangTopCustomer.sql` |
| Customer master | `btr.domain/SalesContext/CustomerAgg/CustomerModel.cs` |
| Faktur view (sales) | `btr.application/SalesContext/FakturInfo/FakturView.cs` |
| Piutang sales wilayah DTO | `btr.application/FinanceContext/PiutangAgg/Contracts/IPiutangSalesWilayahDal.cs` |
| Retur view | `btr.application/InventoryContext/ReturJualAgg/Contracts/IReturJualViewDal.cs` |
| Effective call | `btr.application/SalesContext/CheckInFeature/IEffectiveCallDal.cs` |
| Faktur per customer DAL | `btr.infrastructure/SalesContext/FakturPerCustomerRpt/FakturPerCustomerDal.cs` |
| Piutang dashboard API | `GET /api/dashboard/piutang` |
| Sales dashboard API | `GET /api/dashboard/sales` |
| Executive dashboard API | `GET /api/dashboard/executive` |
| Sales report API | `GET /api/reports/sales` |
| Piutang report API | `GET /api/reports/piutang` |
| Piutang dashboard UI | `btr.portal.web/src/views/dashboard/PiutangDashboardView.vue` |
| Executive home UI | `btr.portal.web/src/views/dashboard/DashboardHomeView.vue` |

---

## Appendix D — Handoff to Architect

**Product scope is approved** (Section 11). Proceed with implementation planning.

The Architect should:

1. **Create dedicated Customer snapshot domain** — new `BTR_PortalDashboardCustomer*` tables, aggregator, DAL, refresh worker, and `GET /api/dashboard/customers` endpoint per Q39–Q40.
2. **Reuse authoritative business rules** from `DashboardPiutangAggregator` and `DashboardSalesFakturAggregator` — customer key (`CustomerCode` first), aging buckets, `KurangBayar > 1`, void exclusion, sales current-month period, piutang all-time open semantics.
3. **Implement new snapshot computations:** Top 10 by Omzet, Top 10 by Piutang (with CustomerCode), dormant detection (90-day rule + prior history), plafond breach, suspended-with-sales, Klasifikasi/Wilayah segmentation counts, attention list rows.
4. **Add route** `/dashboard/customers` and sidebar item **Customers** under Dashboard; page title **Customer Analytics**.
5. **Implement Proposal A layout** with fixed section order: Attention Cards → Attention List → Rankings → Segmentation → Navigation.
6. **Reuse UI components:** `Top10RankingTable.vue`, `DashboardDetailLayout.vue`, `KpiCard.vue`; concentration ratio pattern from `DashboardExecutiveComposer`.
7. **Customer row click** → navigate to Sales Report or Piutang Report with customer pre-filter (Q36).
8. **Navigation path:** Customer Analytics → Domain Dashboard → Report (Q35).
9. **Attention Indicator** presentation consistent with M16 — concentration without thresholds (Q16, Q20).
10. **Do not scope:** Retur analytics, Effective Call, GPS, Faktur Kembali aggregate, declining purchase activity, Harga Type segmentation, collection effectiveness, historical trends, new Customer Report, additional Piutang Report columns, Executive Dashboard changes to Top 5 Customers.

**Explicitly out of scope for this document:** API contracts, database schema changes, implementation tasks, and delivery estimates.
