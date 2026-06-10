# M18 Salesman Performance Dashboard — Feasibility Analysis

**Status:** Analysis complete — Product Owner decisions recorded (Section 9). Ready for Architect.  
**Scope:** Business and feasibility analysis only. No implementation plans, database designs, API designs, or architecture.  
**Date:** 2026-06-11 (analysis) · Product Owner decisions recorded 2026-06-11  
**Role:** Analyst (`docs/agents/analyst-agent.md`)  
**System:** BTR Portal (`/dashboard/salesmen`) consuming BTR.Distrib operational data

**Business question:** *Is my sales force performing effectively?*

**Audience:** Owner · Director · Sales Manager · Area Manager

**Explicitly excluded from M18 (future milestones):**

| Excluded capability | Reserved milestone |
| ------------------- | ------------------ |
| Route replay, GPS route visualization, visit map, check-in investigation screens | M18.5 Sales Route & Visit Monitoring |
| Visit execution KPIs (planned vs actual, visit execution %, coverage %) | M18.5 Sales Route & Visit Monitoring |
| Field effectiveness KPIs (productivity, route compliance score) | M25 Sales Force Effectiveness |
| Collection performance metrics (payments received, DSO, collection achievement) | M20 Collection Dashboard |

**Reference documents:** `docs/foundation/PRODUCT.md`, `docs/foundation/DOMAIN.md`, `docs/foundation/LANDSCAPE.md`, `docs/foundation/WORKFLOW.md`, `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/sales-person-principal-target/feature.md`, `docs/features/visit-plan/feature.md`, `docs/work/btr-portal/M18 Salesman Performance - Analysis.md`, `btr-reporting-investigation.md`

---

## 1. Executive Summary

### Overall feasibility: **High for outcome-based KPIs · Medium for execution-based KPIs · Low without operational discipline**

BTR already maintains the transactional and master data needed to evaluate **sales achievement**, **sales ranking**, **customer coverage (outcome lens)**, and **collection exposure** at the salesman level. A dedicated Portal dashboard at `/dashboard/salesmen` has been built on this foundation using materialized `BTRPD_Salesman*` snapshots refreshed from Faktur, target, and piutang source DALs.

**What works reliably today**

- Salesman identity, territory (`WilayahId`), segment, and principal assignment (SM5/SM6)
- Monthly sales targets — per-rep aggregate via `BTR_SalesPersonPrincipalTarget` (SM6) with fallback to legacy `BTR_SalesOmzetTarget`
- Invoiced achievement from `BTR_Faktur` (`SalesPersonId`, `GrandTotal`, calendar month)
- Sales rankings (Top 10 Omzet, Top 10 Achievement %, Top 10 Piutang)
- Receivable exposure attributed to invoicing salesman (FF1 pattern)
- Customer portfolio signals (concentration, dormant customers) using last-invoicing salesman attribution
- Management attention model (cards + attention list) aligned with M16/M17 Portal philosophy

**What is partially available but not M18-ready**

- **Principal-level achievement** — targets and source data exist; per-principal achievement calculation is documented but not exposed in Portal
- **Customer coverage from visits** — check-in data exists; reliable salesman linkage requires `BTR_SalesPerson.Email` ↔ `BTR_CheckIn.UserEmail`
- **Visit execution KPIs** — `BTR_VisitPlan` materialization is new; planned-vs-actual joins are feasible but depend on email mapping, check-in discipline, and visit-plan rollout
- **Territory execution** — Wilayah and route masters exist; no unified “owned customer book” on customer master

**What requires operational improvement before reliable KPI use**

- SM6 principal target completeness (legacy `BTR_SalesOmzetTarget` may be empty where SM6 is adopted)
- Salesperson email population for field-activity joins
- Consistent mobile check-in usage
- Visit plan materialization coverage (worker horizon, SM4 template maintenance)

### Alignment with BTR Portal management goals

M18 aligns strongly with Portal's decision-support philosophy: transform operational transactions into **management attention signals**, not operational replay screens. Outcome KPIs (achievement, exposure, concentration, dormant portfolio) answer *which salesman requires management attention and why* — the same pattern as M17 Customer Analytics and M14 Piutang V2.

Execution KPIs (visit compliance, effective call rate) have **high management value as leading indicators** but belong to **M18.5** (operational visit monitoring), not M18. M18 may later consume a summarized Visit Execution score from M18.5.

**All open questions resolved.** See Section 9 for authoritative Product Owner decisions.

### Approved product outcome

M18 remains **salesman-centric and performance-oriented** at `/dashboard/salesmen`. Outcome KPIs, attention signals, and rankings are in scope. Principal Achievement is a **salesman drill-down** within M18 — not a separate Principal Performance dashboard. Visit execution detail belongs to M18.5. Collection performance belongs to M20.

### Recommended Release 1 posture

**Implement Now (approved M18 scope):** Sales achievement, ranking, customer coverage (transactional), piutang/overdue exposure, attention signals (including Missing Target Setup), active-salesmen-default view, segmentation.

**Implement Later (within M18):** Principal Achievement drill-down per salesman, achievement trend via snapshot retention, new-customer KPI (first transaction only).

**Deferred to M18.5:** Visit execution %, visit-based coverage %, planned vs actual calls — using Effective Visit Plan as denominator.

**Deferred to M20:** Collection amount, collection achievement, collection ranking.

**Deferred to M25:** Field effectiveness and route compliance score beyond M18.5 summary.

**Reject for M18:** Separate Principal Performance dashboard, GPS visualization, route replay, check-in investigation UI.

---

## 2. KPI Feasibility Matrix

| KPI | Group | Feasibility | Recommendation |
| --- | ----- | ----------- | -------------- |
| Monthly Sales Target | A | **High** | Implement Now |
| Monthly Achievement | A | **High** | Implement Now |
| Achievement % | A | **High** | Implement Now |
| Principal Achievement | A | **Medium** | Implement Later (M18 drill-down) |
| Achievement Trend | A | **Medium** | Implement Later (snapshot retention) |
| Top Salesman by Omzet | B | **High** | Implement Now |
| Top Salesman by Invoice Count | B | **High** | Implement Later |
| Top Salesman by Customer Count | B | **High** | Implement Later |
| Top Salesman by Principal | B | **Medium** | Implement Later |
| Active Customers | C | **High** | Implement Now (as portfolio signal) |
| Visited Customers | C | **Medium** | Requires Operational Improvement |
| Coverage % | C | **Medium** | Requires Operational Improvement |
| New Customers | C | **Medium** | Implement Later |
| Inactive / Dormant Customers | C | **High** | Implement Now |
| Planned Visits | D | **Medium** | Requires Operational Improvement |
| Actual Calls | D | **Medium** | Requires Operational Improvement |
| Effective Calls | D | **Medium** | Requires Operational Improvement |
| Missed Visits | D | **Low–Medium** | Requires Operational Improvement |
| Visit Execution % | D | **Low–Medium** | Requires Operational Improvement |
| Coverage by Territory | E | **Medium** | Implement Later |
| Coverage by Salesman | E | **High** (transactional) / **Medium** (route) | Implement Now (transactional) |
| Coverage by Principal | E | **Medium** | Implement Later |
| Customer Penetration | E | **Low–Medium** | Requires New Feature |
| Collection Amount | F | **High** (data) | Requires New Feature (M20) |
| Collection Achievement | F | **Low–Medium** | Requires New Feature (M20) |
| Outstanding Piutang | F | **High** | Implement Now |
| Overdue Customers | F | **High** | Implement Now |
| Collection Ranking | F | **High** (data) | Requires New Feature (M20) |

---

## 3. Existing Assets Inventory

### 3.1 Master data and business concepts

| Concept | Storage / UI | Analytics relevance |
| ------- | ------------ | ------------------- |
| Sales Person | `BTR_SalesPerson` · SM1 | Identity, `WilayahId`, `Email`, `SegmentId` |
| Sales Person status | **No `IsActive` flag** — inferred from month Faktur activity | Active vs inactive segmentation |
| Territory (Wilayah) | `BTR_SalesPerson.WilayahId` · SM3 | Segmentation dimension |
| Principal assignment | `BTR_SalesPersonSupplier` · SM5 | Eligibility for principal targets |
| Principal target | `BTR_SalesPersonPrincipalTarget` · SM6 | Monthly target per Principal |
| Legacy rep target | `BTR_SalesOmzetTarget` | Fallback when no SM6 rows |
| Route template | `BTR_SalesRute`, `BTR_SalesRuteItem`, `BTR_RuteHari` · SM4 | Planned customer coverage by day |
| Visit plan (materialized) | `BTR_VisitPlan`, `BTR_VisitPlanException` · SM7 | Dated planned visits |
| Effective visit plan | `EffectiveVisitPlanResolver` (computed) | Base plan + exceptions |
| Check-in | `BTR_CheckIn` · RO1 · BTrade3 mobile | Actual field visits |
| Invoiced sales | `BTR_Faktur` (`SalesPersonId`, `GrandTotal`, `FakturDate`) | Achievement, ranking, coverage |
| Open piutang | `BTR_Piutang` → `BTR_Faktur.SalesPersonId` | Exposure by invoicing salesman |
| Payments received | `BTR_PiutangLunas` → Faktur salesman | Collection amount (FF2) |

### 3.2 Portal snapshot layer (M18 implemented)

| Asset | Table / class | Purpose |
| ----- | ------------- | ------- |
| Salesman KPI summary | `BTRPD_SalesmanKpi` | Team totals, attention card counts |
| Top Omzet ranking | `BTRPD_SalesmanTopOmzet` | Top 10 by invoiced omzet |
| Top Achievement ranking | `BTRPD_SalesmanTopAchievement` | Top 10 by achievement % |
| Top Piutang ranking | `BTRPD_SalesmanTopPiutang` | Top 10 by open balance |
| Attention list | `BTRPD_SalesmanAttention` | Salesman × signal rows |
| Segmentation | `BTRPD_SalesmanSegmentation` | Wilayah, activity, segment |
| Aggregator | `DashboardSalesmanAggregator` | Authoritative KPI computation |
| Snapshot worker | `RefreshDashboardSalesmanSnapshotWorker` | 30-minute materialization |
| Portal API | `SalesmanDashboardController` | `/dashboard/salesmen` |
| Portal UI | `SalesmanDashboardView.vue` | Attention-first layout |

### 3.3 Source DALs and policies

| Asset | Grain | Salesman field | M18 usage |
| ----- | ----- | -------------- | --------- |
| `IFakturViewDal` | Per Faktur | `SalesPersonName` | Omzet, customer reach, concentration |
| `ISalesOmzetTargetDal` | Per rep × month | `SalesPersonId` | Target resolution (SM6 sum + legacy fallback) |
| `ISalesPersonPrincipalTargetDal` | Per rep × Principal × month | `SalesPersonId`, `SupplierId` | Principal targets |
| `IPiutangOpenBalanceWithSalesmanDal` | Per open Faktur | `SalesPersonId`, `SalesName` | Piutang exposure |
| `IPiutangSalesWilayahDal` (FF1) | Per open Faktur | `SalesName` | Report validation |
| `ICustomerLastFakturDal` | Per customer | `SalesPersonId` (extended) | Dormant portfolio |
| `ISalesPersonDal` | Per salesman | Full master | Lookup, segmentation |
| `SalesOmzetChartAchievementPolicy` | Policy | — | Achievement % formula |
| `ExecutiveSalesAchievementBandResolver` | Policy | — | 80/100 Warning/Critical bands |
| `IEffectiveCallDal` | Per check-in | `UserEmail` | Effective call (Desktop RO3) |
| `IPenerimaanPelunasanSalesDal` (FF2) | Per rep × day | `SalesName` | Collection amount |
| `IEffectiveVisitPlanDal` / `EffectiveVisitPlanResolver` | Per planned visit | `SalesPersonId` | Planned visit counts |

### 3.4 Desktop reports and forms

| Code | Screen | Salesman dimension | Relevance |
| ---- | ------ | -------------------- | --------- |
| RO2 | Sales Omzet Chart | Per-rep target, achievement, top 15 comparison | Richest sales performance analytics |
| RO3 | Effective Call | `UserEmail`, `IsEffectiveCall` | Visit-to-order conversion |
| RO1 | Check-In Info | Check-in list | Actual call validation |
| SM4 | Rute | Route template maintenance | Coverage denominator |
| SM6 | Principal Target | Per-principal monthly targets | Achievement denominator |
| SM7 | Jadwal Kunjungan | Visit plan + exceptions | Planned visit source |
| FF1 | Piutang Sales Wilayah | `SalesName`, customer, overdue | Piutang validation |
| FF2 | Penerimaan Pelunasan Sales | `SalesName`, daily collections | Collection performance |
| FT1 | Lunas Piutang | Route-day collection queue | Operational collection |
| RF1 | Retur Jual | `SalesName` | Return risk (excluded from M18) |

### 3.5 Portal reports (drill-down)

| Report | Salesman column | M18 drill-down |
| ------ | --------------- | -------------- |
| Sales Report (`/reports/sales`) | `SalesName` | Validate omzet, customers, Faktur detail |
| Piutang Report (`/reports/piutang`) | `SalesName` | Validate open balance, overdue rows |

### 3.6 Reusable Portal UI patterns (M16/M17)

| Component | Reuse |
| --------- | ----- |
| `Top10RankingTable.vue` | Salesman rankings |
| `ExecutiveAttentionCard.vue` | Attention card group |
| Attention list pattern (M17) | Salesman × signal rows |
| `navigateToInvestigation` + `?q=` filter | Contextual report drill-down |

---

## 4. Gap Analysis

| Desired capability | Current state | Gap |
| ------------------ | ------------- | --- |
| Per-salesman achievement % in Portal | **Available** — M18 snapshot | Principal drill-down per salesman not yet exposed (Q1) |
| Sales force attention signals | **Partial** — 6 signals implemented | Rename NoTarget → **Missing Target Setup**; exposure thresholds → Top 20% (Q8, Q12) |
| Top salesman rankings | **Available** — Omzet, Achievement %, Piutang | Invoice count, customer count, principal rankings absent |
| Customer coverage (outcome) | **Partial** — active/dormant via Faktur | No explicit “Active Customers” KPI card; embedded in signals |
| Customer coverage (visit) | **Not in Portal** | Denominator defined (Effective Visit Plan — Q2); implementation → M18.5 |
| Visit execution metrics | **Not computed** | M18.5 scope (Q10); mandatory Email (Q11) |
| Territory execution | **Partial** — Wilayah segmentation only | No route-based penetration or principal×territory matrix |
| Collection performance | **Exposure only in M18 (approved Q7)** | Payment amount, collection achievement, ranking → M20 |
| Achievement trend | **Not in M18** | Requires snapshot retention (Q4) |
| Salesman visibility | **All reps in snapshot** | Default to active only + optional Show Inactive filter (Q9) |
| Customer ownership | **Transactional** | No `SalesPersonId` on `BTR_Customer`; route ownership separate |

---

## 5. KPI Assessment (Detailed)

### Group A — Sales Achievement KPI

---

#### Monthly Sales Target

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Establish planned sales amount per salesman for the month — denominator for achievement |
| **Existing Data Sources** | `BTR_SalesPersonPrincipalTarget` (SM6, primary); `BTR_SalesOmzetTarget` (legacy fallback); `SalesOmzetTargetDal.GetTargetAmount` / `ListTargetsForMonth` |
| **Required Business Concepts** | Sales Person, Principal (Supplier), calendar month |
| **Feasibility** | **High** |
| **Risks** | SM6 adoption incomplete; **Approved (Q5):** when Principal Targets exist for the month, `Monthly Target = SUM(Principal Targets)`; otherwise legacy `BTR_SalesOmzetTarget`; SM6 is long-term source of truth |
| **Recommendation** | **Implement Now** |

---

#### Monthly Achievement

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Actual invoiced sales credited to each salesman in the calendar month |
| **Existing Data Sources** | `BTR_Faktur` via `IFakturViewDal`; grouped by `SalesPersonId` / `SalesPersonName`; non-void Fakturs in current month |
| **Required Business Concepts** | Faktur, Sales Person, calendar month |
| **Feasibility** | **High** |
| **Risks** | Faktur-only policy excludes pipeline/order omzet; salesman on Faktur reflects invoicing rep, not visit rep |
| **Recommendation** | **Implement Now** |

---

#### Achievement %

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Target attainment rate — primary performance indicator for management |
| **Existing Data Sources** | `SalesOmzetChartAchievementPolicy.ComputePercent`; M18 uses `ExecutiveSalesAchievementBandResolver` (≥100% Healthy · 80–99% Warning · <80% Critical) |
| **Required Business Concepts** | Target + Achievement |
| **Feasibility** | **High** |
| **Risks** | Null when no target; zero target treated as no target; retur does not reduce achievement in v1 |
| **Recommendation** | **Implement Now** |

---

#### Principal Achievement

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Evaluate performance per Principal brand line — identifies which product lines underperform within a rep's portfolio |
| **Existing Data Sources** | Target: `BTR_SalesPersonPrincipalTarget`; Achievement: `FakturItem` → `Brg.SupplierId` grouped by `Faktur.SalesPersonId` + month (documented in SM6 feature, not Portal UI) |
| **Required Business Concepts** | Principal, Sales Person, Faktur line items |
| **Feasibility** | **Medium** |
| **Risks** | No existing Portal drill-down UI; multi-principal Fakturs need line-level split; retur exclusion same as rep-level; SM6 completeness varies by Principal |
| **Recommendation** | **Implement Later** — M18 salesman drill-down (not a separate Principal Performance dashboard) |

---

#### Achievement Trend

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Month-over-month or intra-month trajectory — detect sustained decline or recovery |
| **Existing Data Sources** | Historical Faktur by month per salesman; **approved:** snapshot retention from `BTRPD_Salesman*` history (not live multi-month queries) |
| **Required Business Concepts** | Multi-period snapshot materialization |
| **Feasibility** | **Medium** |
| **Risks** | Requires snapshot history table or retention policy; target history retained in SM6/Legacy tables |
| **Recommendation** | **Implement Later** — snapshot retention pattern (Portal materialized dashboard architecture) |

---

### Group B — Sales Ranking KPI

---

#### Top Salesman by Omzet

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Identify revenue leaders; recognition and benchmarking |
| **Existing Data Sources** | `BTRPD_SalesmanTopOmzet`; `DashboardSalesFakturAggregator.BuildTopSalesman` (Sales Dashboard seed); `DashboardSalesmanAggregator.BuildTopOmzet` |
| **Required Business Concepts** | Faktur omzet, current month |
| **Feasibility** | **High** |
| **Risks** | Top 10 only — bottom performers surfaced via Attention List, not ranking; zero-omzet reps excluded |
| **Recommendation** | **Implement Now** |

---

#### Top Salesman by Invoice Count

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Volume-based performance — distinguishes high-transaction from high-value reps |
| **Existing Data Sources** | Count of Fakturs per `SalesPersonId` from `IFakturViewDal` — same source as M18 omzet |
| **Required Business Concepts** | Faktur count |
| **Feasibility** | **High** |
| **Risks** | Not currently materialized; low incremental effort but not approved M18 ranking |
| **Recommendation** | **Implement Later** |

---

#### Top Salesman by Customer Count

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Coverage breadth — reps serving more distinct customers |
| **Existing Data Sources** | Distinct `CustomerCode` per `SalesPersonName` on month Fakturs; pattern in `DashboardSalesFakturAggregator.TotalCustomer` |
| **Required Business Concepts** | Customer key resolution |
| **Feasibility** | **High** |
| **Risks** | Transactional attribution — customer may appear under multiple reps if invoiced by different salesmen |
| **Recommendation** | **Implement Later** |

---

#### Top Salesman by Principal

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Principal-line sales leadership |
| **Existing Data Sources** | `FakturItem` → `Brg.SupplierId`; Desktop `OmzetSupplierInfoForm` (supplier × day matrix) |
| **Required Business Concepts** | Principal, line-level omzet |
| **Feasibility** | **Medium** |
| **Risks** | Requires line-level aggregation; no existing Top-N by Principal per salesman in Portal |
| **Recommendation** | **Implement Later** |

---

### Group C — Customer Coverage KPI

---

#### Active Customers

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Count of customers invoiced by a salesman in the period — transactional reach |
| **Existing Data Sources** | Distinct customer keys on month Fakturs per salesman; M17 Active Customer rule at company level |
| **Required Business Concepts** | Customer key (`CustomerCode` fallback `CustomerName`) |
| **Feasibility** | **High** |
| **Risks** | Measures billing activity, not visit activity; multi-rep customers split across reps |
| **Recommendation** | **Implement Now** (embedded in portfolio analysis; optional explicit KPI card) |

---

#### Visited Customers

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Count of distinct customers with field check-in — operational reach |
| **Existing Data Sources** | `BTR_CheckIn` grouped by `UserEmail` + `CustomerId`; map to salesman via `BTR_SalesPerson.Email` |
| **Required Business Concepts** | Check-in, Email bridge |
| **Feasibility** | **Medium** |
| **Risks** | Email not populated on all salesmen; check-in discipline varies; `UserEmail` may not be 1:1 with salesman; duplicate check-ins same customer/day |
| **Recommendation** | **Requires Operational Improvement** |

---

#### Coverage %

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Proportion of intended or active customers actually visited or invoiced |
| **Existing Data Sources** | Numerator: distinct check-in customers per salesman (`BTR_CheckIn` via `UserEmail`); Denominator: **Effective Visit Plan** — `BTR_VisitPlan` after `BTR_VisitPlanException` overlay via `EffectiveVisitPlanResolver` |
| **Required Business Concepts** | Visit plan materialization, exception overlay, Email ↔ SalesPerson mapping |
| **Feasibility** | **Medium** |
| **Risks** | Visit plan rollout still maturing; email mandatory for field-enabled reps (PO Q11); excluded denominators: Wilayah customers, last-12-month Faktur customers, route template directly |
| **Recommendation** | **Requires Operational Improvement** — deferred to **M18.5**; formula: `Visited Customers ÷ Planned Customers` |

---

#### New Customers

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Customer acquisition — first-ever Faktur in period attributed to salesman |
| **Existing Data Sources** | MIN(`FakturDate`) per customer across **full company history**; first Faktur's `SalesPersonId` for attribution |
| **Required Business Concepts** | First transaction in company history |
| **Feasibility** | **Medium** |
| **Risks** | **Approved:** reactivated dormant accounts are **not** New Customer; future separate metric: Reactivated Customer |
| **Recommendation** | **Implement Later** |

---

#### Inactive / Dormant Customers

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Customers on rep's book who stopped purchasing — coverage erosion signal |
| **Existing Data Sources** | M17 dormant rule: last Faktur ≤ today − 90 days with prior history; M18 rolls up via last-invoicing salesman (`DashboardSalesmanAggregator.ApplyDormantMetrics`) |
| **Required Business Concepts** | Last Invoicing Salesman attribution |
| **Feasibility** | **High** |
| **Risks** | Attribution follows last invoice, not route assignment; customer may have switched reps |
| **Recommendation** | **Implement Now** (Dormant Customer Portfolio attention signal) |

---

### Group D — Visit Execution KPI

**Scope note:** User request excludes route replay and visit maps from M18. These KPIs are analyzed for feasibility and hidden-opportunity value. **Approved:** detailed visit execution metrics belong to **M18.5**; M18 may later consume a summarized Visit Execution score only.

**Approved call definitions (PO Q3):**

| Term | Definition |
| ---- | ---------- |
| **Actual Call** | Check-In — includes visits without an order |
| **Effective Call** | Check-In resulting in a Sales Order (same day) |

---

#### Planned Visits

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Count of customers scheduled for visit on a date — execution plan baseline |
| **Existing Data Sources** | `BTR_VisitPlan` (materialized from SM4 template + SM7 exceptions); `EffectiveVisitPlanResolver` |
| **Required Business Concepts** | Visit plan materialization, rolling horizon (`VISIT_PLAN_HORIZON_DAYS`) |
| **Feasibility** | **Medium** |
| **Risks** | New capability — historical depth limited to materialization start; past rows immutable but future depends on worker + SM4 maintenance; Sunday excluded from cycle |
| **Recommendation** | **Requires Operational Improvement** |

---

#### Actual Calls

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Check-ins performed — field presence at customer (includes visits without orders) |
| **Existing Data Sources** | `BTR_CheckIn` — salesman via `UserEmail`, customer, date, time, GPS |
| **Required Business Concepts** | Check-in; **mandatory** `BTR_SalesPerson.Email` for field-enabled reps |
| **Feasibility** | **Medium** |
| **Risks** | Email population historically incomplete — now mandatory per PO; not all visits may be check-in recorded |
| **Recommendation** | **Requires Operational Improvement** — **M18.5** |

---

#### Effective Calls

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Visits that produced a sales order — productive field time |
| **Existing Data Sources** | `EffectiveCallDal` — `BTR_CheckIn` LEFT JOIN `BTR_Order` on same date + customer + `UserEmail`; `IsEffectiveCall = OrderCount > 0` |
| **Required Business Concepts** | Check-in, Sales Order, same-day match |
| **Feasibility** | **Medium** |
| **Risks** | Order same day but different user email fails match; phone orders without check-in excluded; Desktop RO3 only today |
| **Recommendation** | **Requires Operational Improvement** |

---

#### Missed Visits

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Planned customers not checked in — compliance gap |
| **Existing Data Sources** | Effective visit plan LEFT JOIN check-in on `SalesPersonId` (via email) + `CustomerId` + `VisitDate` |
| **Required Business Concepts** | Visit plan + check-in + identity bridge |
| **Feasibility** | **Low–Medium** |
| **Risks** | All Group D risks combined; exception types (Remove, Replace) must be respected |
| **Recommendation** | **Requires Operational Improvement** — **M18.5** |

---

#### Visit Execution %

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Actual Calls ÷ Planned Visits — primary field compliance metric |
| **Existing Data Sources** | Derived from Planned + Actual above |
| **Required Business Concepts** | Standard period (day/week/month), dedupe rules for multiple check-ins |
| **Feasibility** | **Low–Medium** |
| **Risks** | Denominator = Effective Visit Plan only; email mandatory for field reps |
| **Recommendation** | **Requires Operational Improvement** — **M18.5** |

---

### Group E — Territory Execution KPI

---

#### Coverage by Territory

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Wilayah-level customer reach and performance |
| **Existing Data Sources** | `BTR_SalesPerson.WilayahId`; `FakturView.WilayahName`; M18 segmentation by Wilayah |
| **Required Business Concepts** | Wilayah on salesman and customer |
| **Feasibility** | **Medium** |
| **Risks** | Customer Wilayah may differ from salesman Wilayah (cross-territory selling); Wilayah is segmentation, not strict ownership |
| **Recommendation** | **Implement Later** (Wilayah segmentation in M18 Release 1) |

---

#### Coverage by Salesman

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Each rep's customer reach vs peers |
| **Existing Data Sources** | Transactional: distinct Faktur customers per rep (**High**). Route-based: `BTR_SalesRuteItem` count per rep (**Medium**) |
| **Required Business Concepts** | Ownership model choice |
| **Feasibility** | **High** (transactional) / **Medium** (route) |
| **Risks** | Two models give different stories; M18 uses transactional + dormant attribution |
| **Recommendation** | **Implement Now** (transactional); route-based → M25 |

---

#### Coverage by Principal

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Principal-line customer penetration per salesman |
| **Existing Data Sources** | SM5 assignment + FakturItem by `SupplierId` — distinct customers per rep per Principal |
| **Required Business Concepts** | Principal assignment, line-level Faktur |
| **Feasibility** | **Medium** |
| **Risks** | Customer may buy multiple Principals; assignment in SM5 ≠ actual sales mix |
| **Recommendation** | **Implement Later** |

---

#### Customer Penetration

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Share of territory or route customers actively buying — depth of market coverage |
| **Existing Data Sources** | Requires defined universe (all customers in Wilayah? all route customers?) vs active subset |
| **Required Business Concepts** | Market universe definition — **not standardized in BTR** |
| **Feasibility** | **Low–Medium** |
| **Risks** | No canonical "addressable market" per rep; route customers ≠ all customers in Wilayah |
| **Recommendation** | **Requires New Feature** (business rule for penetration denominator) |

---

### Group F — Collection Performance KPI

---

#### Collection Amount

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Cash and giro payments received against a salesman's invoiced portfolio in a period |
| **Existing Data Sources** | `PenerimaanPelunasanSalesDal` (FF2) — `TotalBayar` per `SalesName` per day; attributes to **invoicing salesman** on underlying Faktur |
| **Required Business Concepts** | PiutangLunas, Faktur salesman |
| **Feasibility** | **High** (data exists) |
| **Risks** | FF2 data exists but **approved (Q7):** collection performance belongs to M20; M18 exposes exposure only |
| **Recommendation** | **Requires New Feature** (M20) — not in M18 |

---

#### Collection Achievement

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Collections vs target or vs billed amount — collection effectiveness |
| **Existing Data Sources** | FF2 payments vs Faktur omzet or piutang; **no collection target table** discovered |
| **Required Business Concepts** | Collection target (missing), period alignment |
| **Feasibility** | **Low–Medium** |
| **Risks** | No salesman collection target master; billing month vs collection month mismatch |
| **Recommendation** | **Requires New Feature** (M20) |

---

#### Outstanding Piutang

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Open receivable balance on salesman's invoiced Fakturs — working capital exposure |
| **Existing Data Sources** | `IPiutangOpenBalanceWithSalesmanDal`; M18 `BTRPD_SalesmanTopPiutang`; FF1 validation |
| **Required Business Concepts** | Open balance threshold (`KurangBayar > 1`) |
| **Feasibility** | **High** |
| **Risks** | All-time open scope vs period-filtered FF1 default — Portal uses all-time open; **High Piutang Exposure signal (Q8):** Top 20% of salesmen by open balance — configurable |
| **Recommendation** | **Implement Now** — exposure only; no collection performance KPI (Q7) |

---

#### Overdue Customers

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Count of distinct customers with past-due balance on salesman's invoices |
| **Existing Data Sources** | FF1 + `DashboardPiutangAggregator` aging rules applied per salesman; M18 High Overdue Exposure signal |
| **Required Business Concepts** | JatuhTempo, aging buckets |
| **Feasibility** | **High** |
| **Risks** | **Approved threshold (Q8):** Top 20% of salesmen by overdue balance — configurable, not hardcoded nominal |
| **Recommendation** | **Implement Now** |

---

#### Collection Ranking

| Field | Detail |
| ----- | ------ |
| **Business Purpose** | Rank salesmen by collections received — identify strong vs weak collectors |
| **Existing Data Sources** | FF2 grouped by `SalesName` — derivable |
| **Required Business Concepts** | Period-scoped payments |
| **Feasibility** | **High** (data) |
| **Risks** | Ranking by collections without normalizing for portfolio size misleads; overlaps M20 scope |
| **Recommendation** | **Requires New Feature** (M20) |

---

## 6. Hidden Opportunity Analysis

Visit planning and check-in data can provide **leading indicators** of future sales performance — valuable beyond operational monitoring if operational discipline is sufficient.

### 6.1 Leading indicator hypothesis

| Metric | Management value beyond operations | Relationship to sales outcomes |
| ------ | ---------------------------------- | ------------------------------ |
| **Visit Execution %** | Early warning before month-end achievement gap | Low execution often precedes low omzet in same period |
| **Customer Coverage %** (visit-based) | Detect territory erosion before billing stops | Visited-but-not-buying vs not-visited patterns differ in recovery potential |
| **Effective Call %** | Distinguish activity from productivity | High check-ins with low effective calls → coaching opportunity before target miss |
| **Missed Visit Trend** | Rising misses may predict dormant customer growth | 90-day dormant signal is lagging; missed visits are leading |

### 6.2 Feasibility of early-warning use

| Condition | Assessment |
| --------- | ---------- |
| Data exists | **Yes** — VisitPlan, CheckIn, EffectiveCall join |
| Reliable identity link | **Conditional** — requires Email population on `BTR_SalesPerson` |
| Visit plan maturity | **Conditional** — materialization recently introduced; limited history |
| Management actionability | **High** — Area Manager can intervene mid-month |
| Portal fit | **Medium** — leading indicators complement M18 outcome dashboard; fit M25 or M18.5 better |

### 6.3 Recommended treatment

| Approach | Rationale |
| -------- | --------- |
| **Do not block M18 Release 1 on execution KPIs** | Outcome KPIs are reliable today with minimal operational change |
| **Assign detailed visit KPIs to M18.5** | **Approved (Q10):** Visit Execution %, planned vs actual, visit-based Coverage % — not M18 |
| **M18 may consume summarized Visit Execution score later** | Performance dashboard stays outcome-oriented; M18.5 owns operational detail |
| **Use M18 attention list for lagging signals** | Below Target, Dormant Portfolio capture failure after it manifests |
| **Pilot correlation offline** | Validate visit execution % vs achievement % before M18.5 launch |

### 6.4 Minimum operational prerequisites for M18.5 visit KPIs

1. SM4 route templates maintained for all active salesmen  
2. Visit plan worker running with full horizon  
3. **SalesPerson.Email mandatory** for all field-enabled reps (PO Q11)  
4. Check-in adoption policy (expected visits/day)  
5. **Coverage % denominator:** Effective Visit Plan only — `BTR_VisitPlan` after exception overlay (PO Q2)

---

## 7. Recommended Dashboard Scope — M18 Release 1

Prioritized by: reliable data · high management value · minimal operational change · decision-support alignment.

### 7.1 Include in Release 1 (approved)

| Category | KPIs / capabilities |
| -------- | ------------------- |
| **Attention** | Below Target · **Missing Target Setup** · High Overdue Exposure (Top 20%) · High Piutang Exposure (Top 20%) · Customer Concentration · Dormant Customer Portfolio |
| **Rankings** | Top 10 Omzet · Top 10 Achievement % · Top 10 Piutang |
| **Achievement** | Monthly Target (SM6 sum, legacy fallback) · Monthly Achievement · Achievement % (per rep, current month) |
| **Exposure** | Outstanding Piutang · Overdue Customers (per rep) — **no collection performance KPI** |
| **Coverage (outcome)** | Dormant portfolio · Customer concentration · Active customers (transactional, via signals) |
| **Audience filter** | **Active salesmen only** by default; optional **Show Inactive Salesmen** filter |
| **Segmentation** | By Wilayah · Segment (optional) |
| **Navigation** | Drill-down to Sales Report / Piutang Report with salesman pre-filter |

### 7.2 Defer to post–Release 1 (within M18)

| KPI | Reason |
| --- | ------ |
| **Principal Achievement drill-down** | Per-salesman Principal breakdown inside M18 — not a separate dashboard (Q1) |
| **Achievement Trend** | Snapshot retention from `BTRPD_Salesman*` history (Q4) |
| Top by Invoice Count / Customer Count / Principal | Incremental value; not primary management question |
| **New Customers** | First transaction in company history only; reactivated dormant excluded (Q6) |
| Coverage by Principal | Depends on principal-level sales aggregation |

### 7.3 Deferred to M18.5 (Sales Route & Visit Monitoring)

| KPI | Prerequisite |
| --- | ------------ |
| Planned Visits · Actual Calls · Effective Calls · Missed Visits · Visit Execution % | Visit plan rollout + mandatory email + check-in discipline |
| Visited Customers · Visit-based Coverage % | Effective Visit Plan denominator (Q2); Actual Call = Check-In (Q3) |

### 7.4 Deferred to M25

| KPI | Notes |
| --- | ----- |
| Field effectiveness beyond M18.5 summary | Route compliance score, productivity composites |

### 7.5 Requires new feature (other milestones)

| KPI | Milestone |
| --- | --------- |
| Collection Amount · Collection Achievement · Collection Ranking | M20 Collection Dashboard (Q7) |
| Customer Penetration (canonical denominator) | Business rule + M25 |
| GPS / route visualization / check-in investigation | M18.5 |

---

## 9. Final Product Decisions (Authoritative)

**Status:** All open questions resolved by Product Owner — 2026-06-11.  
**Audience:** Architect — use this section as scope input. Do not re-decide business rules listed here.

### 9.1 Scope and presentation

| # | Decision |
| - | -------- |
| Q1 | **Principal Achievement** included in M18 as **salesman drill-down** — not a separate Principal Performance dashboard |
| Q7 | **Collection exposure only** in M18 (Outstanding Piutang, Overdue Customers, exposure indicators). Collection performance → **M20** |
| Q9 | **Default view:** active salesmen only. Optional filter: **Show Inactive Salesmen**. Inactive reps with zero activity and zero balance hidden by default |
| Q10 | **Visit execution KPIs** → **M18.5** (not M18). M18 may later consume a **summarized Visit Execution score** from M18.5 |

### 9.2 Target and achievement rules

| # | Decision |
| - | -------- |
| Q5 | **SM6 wins:** if Principal Targets exist for the month, `Monthly Target = SUM(Principal Targets)`; otherwise legacy `BTR_SalesOmzetTarget`. Long-term direction is SM6 |
| Q4 | **Achievement trend** uses **snapshot retention** from materialized dashboard history — not live multi-month queries |
| Q12 | Add attention signal **Missing Target Setup** — distinguishes planning gap ("no configured target") from **Below Target** ("underperforming against plan"). Cross-links to SM6 completeness |

### 9.3 Visit and coverage rules (M18.5)

| # | Decision |
| - | -------- |
| Q2 | **Coverage % denominator:** Effective Visit Plan (`BTR_VisitPlan` after `VisitPlanException`). Formula: `Visited Customers ÷ Planned Customers`. Excluded: Wilayah customers, last-12-month Faktur customers, route template directly |
| Q3 | **Actual Call = Check-In** (includes visits without order). **Effective Call = Check-In resulting in Sales Order** |
| Q11 | **SalesPerson.Email mandatory** for field-enabled reps — required for Check-In, Effective Call, Visit Execution, and Route Compliance KPIs |

### 9.4 Attention signal thresholds

| # | Decision |
| - | -------- |
| Q8 | **Configurable threshold** — initial proposal: **High Piutang Exposure** = Top 20% of salesmen by open balance; **High Overdue Exposure** = Top 20% of salesmen by overdue balance. Avoid hardcoded nominal values |

### 9.5 Customer metrics

| # | Decision |
| - | -------- |
| Q6 | **New Customer** = first transaction in **company history**. Reactivated dormant accounts **excluded**. Future separate metric: **Reactivated Customer** |

### 9.6 Approved attention list signals (updated)

| Signal | Business meaning |
| ------ | ---------------- |
| **BelowTarget** | Achievement % in Warning or Critical band — rep underperforming against configured plan |
| **MissingTargetSetup** | No configured target for rep/period — planning gap (SM6 completeness) |
| **HighOverdueExposure** | Rep in Top 20% by overdue balance (configurable) |
| **HighPiutangExposure** | Rep in Top 20% by open balance (configurable) |
| **CustomerConcentration** | Top customer % of rep omzet (informational) |
| **DormantCustomerPortfolio** | ≥1 dormant customer on book (90-day rule, last-invoicing attribution) |

**Note:** `MissingTargetSetup` replaces the prior working label `NoTarget` for clarity. Combined cross-domain signals remain encouraged.

---

## Appendix — Terminology

| Term | BTR meaning |
| ---- | ----------- |
| Salesman / Sales Person | `BTR_SalesPerson` — field sales representative |
| Principal | Brand/supplier line — `BTR_Supplier` / `SupplierId` |
| Achievement % | Invoiced omzet ÷ monthly target × 100 |
| Last Invoicing Salesman | Salesman on most recent Faktur for a customer — M18 dormant/coverage attribution |
| Invoicing Salesman | Salesman on Faktur — piutang and collection attribution |
| Open piutang | `KurangBayar > 1` (persisted `BTR_Piutang.Sisa`) |
| Effective Call | Check-in resulting in ≥1 Sales Order same day, same customer, same email |
| Actual Call | Check-in — includes visits without an order |
| Effective Visit Plan | `BTR_VisitPlan` after applying `BTR_VisitPlanException` |
| Missing Target Setup | Attention signal — no configured target (planning gap); distinct from Below Target |
| Visit Plan | Materialized dated schedule from route template + exceptions |

---

*End of feasibility analysis. Hand to Architect — Section 9 contains authoritative Product Owner decisions.*
