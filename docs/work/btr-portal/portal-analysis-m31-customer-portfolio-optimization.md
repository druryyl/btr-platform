# BTR Portal Analysis — M31 Customer Portfolio Optimization

**Status:** Analysis complete — Product Owner decisions recorded 2026-06-22 — feature spec extracted — ready for Architect engagement.  
**Scope:** Business discovery, asset inventory, and approved product decisions. No solution design, no implementation plan.  
**Date:** 2026-06-22 (analysis) · Product Owner decisions recorded 2026-06-22  
**Author role:** Analyst  
**Feature spec:** [docs/features/customer-portfolio-optimization/feature.md](../../features/customer-portfolio-optimization/feature.md)

**Reference documents:** `docs/foundation/PRODUCT.md`, `docs/foundation/DOMAIN.md`, `docs/foundation/LANDSCAPE.md`, `docs/foundation/WORKFLOW.md`, `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-operational.md`, `docs/features/btr-portal/btr-portal-architecture.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md`, `docs/features/customer-risk-forecast/feature.md`, `docs/features/collection-optimization/feature.md`, `docs/work/btr-portal/M17-Customer-Analytics-Analysis.md`, `docs/work/btr-portal/portal-analysis-m29-customer-risk-forecast.md`, `docs/work/btr-portal/portal-analysis-m30-collection-optimization.md`, `docs/work/btr-portal/M18-Salesmen-Performance-Analysis.md`, `docs/work/btr-portal/M20-collection-dashboard-analysist.md`, `docs/work/btr-portal/M14-piutang-dashboard-v2-analysis.md`, `docs/work/btr-portal/M18-5-Sales-Visit-Analysis.md`, `docs/features/btr-portal/ALERT-REGISTRY.md`

---

## 1. Executive Summary

BTR Portal has evolved through six business maturity levels. M31 belongs to the **Optimization** layer — the same maturity as Inventory Optimization (M28.5) and Collection Optimization (M30) — but oriented toward **customer portfolio management** rather than inventory or collection operations alone.

| Level | Question | Relevant customer surfaces |
| ----- | -------- | ---------------------------- |
| Reporting | What happened? | Sales Report, Piutang Report, Desktop SF1/FF1/RF1 |
| Analytics | How is the business performing? | Customer Analytics (M17), Piutang (M14), Sales (M11) |
| Decision Support | What requires attention today? | Executive Dashboard, Alert Center, M17 attention list |
| Forecasting | What will probably happen? | Customer Risk Forecast (M29) |
| Optimization | What should management do? | Collection Optimization (M30), **Customer Portfolio Optimization (M31)** |

**M17 answers:** *Which customers require attention across sales and receivables today?*

**M29 answers:** *Which customers are likely to become collection or relationship risks in the next 30 days — and why?*

**M30 answers:** *Given those risks, which customers should Finance and Sales contact first today for collection?*

**M31 answers (approved):** *What should Management do with each customer — grow, retain, protect, collect, recover, or exit review — across the full portfolio?*

### Milestone relationship (approved)

```text
M17  Customer Analytics          →  "What is happening?"
M29  Customer Risk Forecast       →  "What may happen?"
M30  Collection Optimization     →  "What should Finance collect?"
M31  Portfolio Optimization       →  "What should Management do with each customer?"
```

M31 **supplements** M17, M29, and M30 — it does not replace any of them.

### Architectural constraint (approved)

> **M31 must be a composition milestone, not a calculation milestone.**

M31 primarily **composes and prioritizes** insights already produced by:

- **M17** — current-state customer analytics
- **M29** — forward customer risk forecast
- **M30** — collection optimization actions
- **M18** — salesman context (summary only)
- **M22** — geographic/territory context (optional)

M31 must not become another independent analytics engine.

### Key findings

| Finding | Implication for M31 |
| ------- | --------------------- |
| Customer analytics span **five portal dashboards** (M17, M29, M30, M14, M20) plus executive, salesman, location, and field activity lenses | M31 is a **composition milestone** — it synthesizes existing signals; it should not redefine aging, dormant, plafond, or recovery semantics |
| **No customer profitability data** exists in portal or as a per-customer aggregate in Desktop | Gross margin, contribution, discount %, and retur ratio per customer are **gaps** — M31 cannot answer "most profitable customer" without new data or Desktop-only manual analysis |
| **No explicit lifecycle states** exist in BTR today | PO approved **computed lifecycle** for M31: New, Growing, Mature, Declining, Dormant (+ Never Purchased) — thresholds configurable later |
| Customer master carries **Klasifikasi** but no portfolio tier | PO approved **computed tiers** (Strategic, High/Medium/Low Value) from omzet, piutang, frequency, risk — Klasifikasi filter only |
| Salesman assignment is **route-based**, not a FK on customer master | Portal infers salesman from **last invoicing Faktur** — attribution rules differ by domain |
| M29 + M30 already materialize **forward risk, recommendations, and collection priority** in Customer snapshot worker | M31 should consume these outputs in-memory (same pattern as M30 consuming M29) |
| Desktop has richer customer evidence (SF4, FF1, FF4, RF1, FT5, RO3, RO4) **not exposed in portal** | PO approved **Customer Report** in portal — drill-down chain extends to dedicated report before Desktop |
| **IsSuspend UI gap** in Desktop CustomerForm — checkbox exists but is not wired to save/load | **Desktop fix is prerequisite** — separate work item; do not redesign M31 around bad master data |
| **No transactional credit enforcement** at Faktur save — plafond/suspend are analytical, not blocking | Portfolio optimization recommendations are read-only; operational enforcement stays in Desktop |

### Approved product outcome

Deliver **Customer Portfolio Optimization** covering **all customers** with default view **Attention Customers only**. Single dashboard serves **Owner/GM** (portfolio health, concentration, working capital) and **Sales Manager** (assigned portfolio, growth, retention, collection coordination).

Portfolio objectives (approved): **Grow · Retain · Protect · Recover** — not risk-only. Customer value uses **omzet proxy** (not profitability). Collection queue **reuses M30** via link — never duplicated.

See **Section 18** for full Product Owner decision record.

---

## 2. Existing Customer Assets

### 2.1 Asset map by layer

```text
BTR Desktop (source of truth)
  ├── Master: BTR_Customer, BTR_Klasifikasi, BTR_Wilayah, BTR_HargaType
  ├── Sales: BTR_Faktur, FakturView, BTR_SalesRute, BTR_SalesRuteItem
  ├── Finance: BTR_Piutang, BTR_PiutangLunas, BTR_PiutangElement, BTR_Tagihan
  ├── Field: BTR_VisitPlan, BTR_CheckIn, BTR_Order, EffectiveCallView
  └── Returns: BTR_ReturJual, ReturJualView

Portal snapshot layer (~15–30 min refresh)
  ├── Piutang worker (M14) → per-customer aging, Top 20 risk
  ├── Collection worker (M20, M27) → recovery, overdue workload
  ├── Customer worker (M17 → M29 → M30) → attention, forecast, optimization
  ├── Salesman worker (M18) → rep portfolio signals
  └── Location worker (M22) → wilayah/warehouse concentration

Portal live query layer
  ├── Sales Report, Piutang Report
  └── Field Activity Dashboard (M18.5)

Portal decision support
  ├── Executive Dashboard (M16) → Top 5 customers by piutang
  └── Alert Center (M23) → customer + collection alert categories
```

### 2.2 Primary reusable components (by implementation location)

| Component | Path | Role |
| --------- | ---- | ---- |
| `DashboardCustomerAggregator` | `btr.application/.../DashboardSnapshotAgg/Services/DashboardCustomerAggregator.cs` | M17 authoritative current-state customer rules |
| `DashboardCustomerRiskForecastAggregator` | same folder | M29 forward risk |
| `DashboardCollectionOptimizationAggregator` | same folder | M30 collection actions |
| `CustomerRiskForecastPolicy` | same folder | M29 thresholds and category resolution |
| `CustomerRiskSignalBuilder` | same folder | M29 forecast signals |
| `CustomerRiskRecommendationBuilder` | same folder | M29 recommendations |
| `CollectionOptimizationPolicy` | same folder | M30 action categories and priority |
| `DashboardPiutangAggregator` | same folder | M14 aging, Top 20 customer risk |
| `DashboardCollectionAggregator` | same folder | M20 recovery, overdue workload |
| `DashboardSalesmanAggregator` | same folder | M18 rep portfolio (dormant book, concentration) |
| `PiutangAgingBucketResolver` | same folder | **Authoritative** 5-bucket aging |
| `DashboardCustomerKeyResolver` | same folder | Code-first customer key |
| `RefreshDashboardCustomerSnapshotWorker` | `.../UseCases/RefreshDashboardCustomerSnapshotWorker.cs` | Orchestrates M17→M29→M30 chain |
| `ICustomerDal` / `CustomerDal` | application + infrastructure | Master data |
| `IFakturViewDal` | SalesContext/FakturInfo | MTD omzet, active customers |
| `ICustomerLastFakturDal` | SalesContext/FakturInfo | Last purchase, dormant |
| `ICustomerOmzetHistoryDal` | SalesContext/FakturInfo | Current + prior month omzet (M29 decline) |
| `ICustomerPaymentBehaviorDal` | FinanceContext/PiutangAgg | Avg payment lag (M29) |
| `ICustomerPelunasanSummaryDal` | FinanceContext/PiutangAgg | Payment recency (M29) |
| `IPiutangOpenBalanceDal` | DashboardSnapshotAgg | Open balances |
| `IPiutangSalesWilayahDal` | FinanceContext | Richest receivable row (Desktop FF1, portal piutang report) |

### 2.3 Snapshot tables with customer grain

| Table | Milestone | Contents |
| ----- | --------- | -------- |
| `BTRPD_CustomerKpi` | M17 | Portfolio headline KPIs |
| `BTRPD_CustomerTopOmzet` | M17 | Top 10 MTD omzet ranking |
| `BTRPD_CustomerTopPiutang` | M17 | Top 10 open balance ranking |
| `BTRPD_CustomerAttention` | M17 | Attention list rows |
| `BTRPD_CustomerSegmentation` | M17 | Klasifikasi, Wilayah, Activity counts |
| `BTRPD_CustomerRiskForecastKpi` | M29 | Forecast portfolio KPIs |
| `BTRPD_CustomerRiskForecastCustomer` | M29 | Top 20 risk customers |
| `BTRPD_CustomerRiskForecastAttention` | M29 | Top 25 forward signals |
| `BTRPD_CustomerRiskForecastRecommendation` | M29 | Top 15 recommendations |
| `BTRPD_CollectionOptimizationPriority` | M30 | Top 30 collection priority queue |
| `BTRPD_CollectionOptimizationQueue` | M30 | Specialized action queues |
| `BTRPD_PiutangCustomerAging` | M14 | Per-customer aging (materialized, not API-exposed) |
| `BTRPD_PiutangTopCustomerRisk` | M14 | Top 20 by open balance + bucket breakdown |

---

## 3. Customer Master

### 3.1 Data model

**Source of truth:** `BTR_Customer` — `btr.sql/Tables/SalesContext/BTR_Customer.sql`  
**Domain model:** `CustomerModel` — `btr.domain/SalesContext/CustomerAgg/CustomerModel.cs`

| Field group | Fields | Business meaning |
| ----------- | ------ | ---------------- |
| Identity | `CustomerId`, `CustomerCode`, `CustomerName` | Primary keys; portal uses code-first resolution |
| Segmentation | `WilayahId`, `KlasifikasiId`, `HargaTypeId` | Territory, user-defined classification, price tier |
| Address | `Address1/2`, `Kota`, `KodePos`, `NoTelp`, `NoFax`, `Email` | Contact and geographic detail |
| Tax | `Npwp`, `Nppkp`, `Nik`, `IsKenaPajak`, etc. | Compliance — not used in portal analytics |
| Credit | `Plafond`, `CreditBalance`, `IsSuspend` | Credit limit, running balance, suspension flag |
| Location | `Latitude`, `Longitude`, `Accuracy`, `CoordinateTimestamp` | Field sales GPS — Desktop RO4 only |

### 3.2 Classifications, groups, categories, types, status

| Concept | Exists? | Implementation | Notes |
| ------- | ------- | -------------- | ----- |
| **Klasifikasi** | Yes | `BTR_Klasifikasi` — user-maintained lookup (id + name, max 20 chars) | Not predefined High/Medium/Low; PO-defined values |
| **Wilayah** | Yes | `BTR_Wilayah` — territory master | Used in M17 segmentation, M20/M29/M30 workload |
| **HargaType** | Yes | `BTR_HargaType` — price tier | CustomerForm only; **not in portal analytics** |
| **Segment** | Partial | `BTR_Segment` on **SalesPerson**, not Customer | Trade channel applies to salesman, not customer |
| **Customer group** | No | — | No group entity beyond Klasifikasi |
| **Customer type (CRM)** | No | HargaType is pricing tier, not CRM type | — |
| **Customer status (Active/Inactive)** | Computed | M17 Active = MTD Faktur; Dormant = 90d rule | Not stored on master |
| **Credit status** | No dedicated field | Inferred from Plafond vs open balance + IsSuspend | — |
| **ABC / strategic tier** | No | — | Would require PO definition |

### 3.3 Credit, suspension, activation

| Rule | Location | Detail |
| ---- | -------- | ------ |
| Plafond ≥ 0 | `CustomerBuilder.Plafond()` | Rejects negative plafond |
| FK validation | `CustomerBuilder` | Wilayah, Klasifikasi, HargaType must exist |
| ID generation | `CustomerWriter` | Auto `"CS"` + counter on insert |
| Plafond copied to Faktur | `FakturBuilder` | Informational at invoice time |
| **No Faktur block on suspend/plafond** | Faktur save path | Credit policy enforced analytically only |
| Plafond breach (analytics) | `DashboardCustomerAggregator.BuildPlafondBreachKeys` | `Plafond > 0 AND openBalance > Plafond` |
| Suspended + Sales | `DashboardCustomerAggregator.BuildSuspendedWithSalesKeys` | `IsSuspend AND` invoiced MTD |
| **IsSuspend UI gap** | `CustomerForm` | Checkbox in Designer **not wired** to save/load |

### 3.4 Ownership and assignment

| Assignment type | Stored on customer? | How BTR resolves it |
| ----------------- | ------------------- | ------------------- |
| **Wilayah** | Yes — `WilayahId` FK | Master data; appears on Faktur rows |
| **Salesman** | **No** FK | `BTR_SalesRuteItem` — customer on salesman route by day; portal uses **last invoicing salesman** from `ICustomerLastFakturDal` |
| **Collector** | No separate entity | Collection attributed to invoicing salesman (M20 analysis) |
| **Warehouse** | No on customer | Warehouse comes from Faktur/billing origin |

**Desktop forms:** `CustomerForm` (SM1), `WilayahForm`, `SalesRuteForm`, `SalesPersonForm`, `CustomerBrowser`, `LocationCoverageInfoForm` (RO4)

**Portal DTOs:** `CustomerModel` consumed via `ICustomerDal.ListData()` in Customer snapshot worker

---

## 4. Customer Sales Analysis

### 4.1 Existing metrics

| Metric | Computed? | Period | Location |
| ------ | --------- | ------ | -------- |
| Total omzet (company) | Yes | Current month | M17 `TotalOmzet`, Sales snapshot |
| Customer MTD omzet | Yes | Current month | M17 Top 10 Omzet; per-customer in aggregator |
| Prior month omzet | Yes | Calendar prior month | `ICustomerOmzetHistoryDal` — M29 decline only |
| Active customer count | Yes | Current month | M17, Sales snapshot `TotalCustomer` |
| Top Omzet Customer % | Yes | Current month | M17 concentration KPI |
| Invoice count per customer | **No aggregate** | — | Derivable from `FakturView` row count |
| Average invoice value | **No aggregate** | — | Derivable: omzet ÷ faktur count |
| Purchase frequency | **No** | — | No invoices/month or visits/month KPI |
| Last purchase date | Yes | All-time | `ICustomerLastFakturDal` — dormant, M29 inactivity |
| Growth trend (MoM) | Partial | Current vs prior month | M29 projected decline ratio only |
| Declining customers | Partial | Forward-looking | M29 `ModerateDecline` (<70%), `SevereDecline` (<50%) |
| Faktur Kembali backlog | Partial | Row-level | Sales Report `Status = Kembali`; no per-customer aggregate |

### 4.2 Rankings and screens

| Ranking | Portal | Desktop | Rule |
| ------- | ------ | ------- | ---- |
| Top 10 Customer by Omzet | M17 `/dashboard/customers` | SF1 groupable grid | `SUM(GrandTotal)` MTD, Top N = 10 |
| Top 10 Customer by Piutang | M17 | FF1 groupable | `SUM(KurangBayar)` all open |
| Top 20 Customer Risk (balance) | M14 `/dashboard/piutang` | FF1 | Rank by total open + aging buckets |
| Top 20 Customer by Risk Priority | M29 | — | M29 `RiskPriorityScore` |
| Top 5 Customers (executive) | M16 | — | Truncated from M14 Top Customer Risk |
| Faktur Per Customer (line detail) | — | SF4 `FakturPerCustomerForm` | Line-item sales by customer |
| Top 10 Kota by customer count | — | `CustomerChartRpt` (orphan, not in menu) | Count only, not omzet |

### 4.3 Data sources

| DAL | DTO | Key fields |
| --- | --- | ---------- |
| `IFakturViewDal` | `FakturView` | `CustomerCode`, `Customer`, `GrandTotal`, `Tgl`, `WilayahName`, `KlasifikasiName`, `SalesPersonId/Name` |
| `ICustomerOmzetHistoryDal` | `CustomerOmzetHistoryDto` | `CurrentMonthOmzet`, `PriorMonthOmzet` |
| `ICustomerLastFakturDal` | `CustomerLastFakturDto`, `CustomerLastFakturWithSalesmanDto` | `LastFakturDate`, salesman attribution |
| `IFakturPerCustomerDal` | Faktur line DTOs | SF4 report — item-level detail |

**Void rule:** `VoidDate = '3000-01-01'` — all portal sales queries  
**Customer key:** `DashboardCustomerKeyResolver.ResolveCodeFirst(CustomerCode, CustomerName)`

---

## 5. Customer Collection Analysis

### 5.1 Existing metrics

| Metric | Computed? | Location |
| ------ | --------- | -------- |
| Outstanding balance (per customer) | Yes | M14, M17, piutang report |
| Aging buckets (5-bucket) | Yes | `PiutangAgingBucketResolver` — authoritative |
| Overdue customer count | Yes | M14, M17, M20 |
| Overdue exposure amount | Yes | M20 |
| Chronic overdue (>90d) | Yes | M14 aging, M20 `ChronicOverdue` signal |
| Collection effectiveness (company) | Yes | M20 `Recovery vs Billing %` |
| Cash collected MTD | Yes | M20 from `IPenerimaanPelunasanSalesDal` |
| Payment mix (cash/giro/adjustment) | Yes | M20 company-level |
| Average payment lag (per customer) | Yes | M29 via `ICustomerPaymentBehaviorDal` — 90d lookback, min 2 settled |
| Last payment date | Yes | M29 via `ICustomerPelunasanSummaryDal` — 30d recency |
| Legacy debt (dormant + balance) | Yes | M20 `LegacyDebt` signal |
| Per-customer DSO | **No** | Explicitly excluded from M29 V1 |
| Collection lag per Faktur | Desktop only | `IPiutangTrackerDal` (FT5) |
| Payment history detail | Desktop only | `IPelunasanInfoDal` (FF4) |

### 5.2 Aging model (authoritative — do not duplicate)

| Bucket | Rule | Location |
| ------ | ---- | -------- |
| Current | Not yet past due | `PiutangAgingBucketResolver` |
| Days1To30 | 1–30 days past due | same |
| Days31To60 | 31–60 days | same |
| Days61To90 | 61–90 days | same |
| DaysOver90 | >90 days | same |

**Open balance filter:** `KurangBayar > 1` (or `Sisa > 1` in SQL)

### 5.3 Desktop collection assets

| Form | ID | Purpose | Customer grain |
| ---- | -- | ------- | -------------- |
| `PiutangSalesWilayahForm` | FF1 | Full receivable grid | Per Faktur row with Customer, Sales, Wilayah, Retur, payment splits |
| `LunasPiutang2Form` | FT1 | Route-day collection workflow | Customers from salesman route |
| `PelunasanInfoForm` | FF4 | Payment history | Per customer/Faktur settlement |
| `PenerimaanPelunasanSalesForm` | FF2 | Collections by salesman-day | Salesman grain, not customer |
| `TagihanForm` | FT* | Collection document workflow | Route-based customer list |
| `PiutangTrackerForm` | FT5 | Per-Faktur lifecycle | Invoice-to-payment timeline |

**Portal piutang report** exposes subset of FF1: `CustomerName`, `SalesName`, dates, `TotalJual`, `KurangBayar`. Hidden at portal API: `Retur`, `Potongan`, `BayarTunai`, `BayarGiro`, `WilayahName`, `Alamat`.

---

## 6. Customer Credit Analysis

### 6.1 Credit limit (Plafond)

| Capability | Exists? | Location |
| ---------- | ------- | -------- |
| Plafond on master | Yes | `BTR_Customer.Plafond`, `CustomerModel.Plafond` |
| CreditBalance on master | Yes | Editable on CustomerForm; copied to Faktur |
| Current utilization | Partial | M30 `CreditUtilizationPercent = OpenBalance / Plafond × 100` |
| Remaining credit | **No KPI** | Derivable: `Plafond - OpenBalance` |
| Plafond breach (current) | Yes | M17 `PlafondBreach` signal |
| Plafond breach + overdue | Yes | M20 `PlafondBreachOverdue` |
| Approaching plafond (90%) | Yes | M29 `ApproachingPlafond` — projected ≥ 90% |
| Projected plafond breach | Yes | M29 `ProjectedPlafondBreach` — 30-day horizon |
| Breached worsening | Yes | M29 — current breach + MTD billing |
| Over-limit at Faktur save | **No enforcement** | Display only on FakturForm |
| Credit review recommendation | Yes | M29 `ReviewCredit`, M30 `CreditReview` queue |

### 6.2 Suspension

| Rule | Signal | Location |
| ---- | ------ | -------- |
| `IsSuspend = true` + MTD Faktur | `SuspendedWithSales` | M17, Alert Center |
| Projected breach + overdue + suspended | M29 recommendation `SuspendCreditReview` | `CustomerRiskRecommendationBuilder` |
| Suspended list/report | **No dedicated report** | Flag visible in CustomerForm grid (if saved) |

### 6.3 Working capital consumption

| Proxy metric | Available? | Notes |
| ------------ | ---------- | ----- |
| Open balance per customer | Yes | Direct working capital tie-up |
| Top Piutang concentration | Yes | M17 Top 10, Top Piutang % |
| Elevated-risk receivable (M29) | Yes | `ElevatedRiskReceivableTotal` |
| Combined omzet + piutang per customer | Partial | Same customer key joinable in Customer worker — not a single KPI today |
| Days outstanding (DSO) | No | Deferred |

---

## 7. Customer Activity Analysis

### 7.1 Activity states (existing definitions)

| State | Definition | Where implemented | Stored? |
| ----- | ---------- | ----------------- | ------- |
| **Active** | Customer with Faktur in **current calendar month** | `DashboardCustomerAggregator.BuildActiveSet()` | Computed |
| **Dormant** | Last Faktur ≤ today − **90 days**, prior history exists, **not** active MTD | M17 `DormantDaysThreshold = 90` | Computed |
| **Approaching dormant** | 60–89 days since last Faktur | M29 `CustomerRiskForecastPolicy` | Computed |
| **Imminent dormant** | 80–89 days | M29 CRF-I rules | Computed |
| **Legacy debt** | Dormant + open balance > 1 | M20 `LegacyDebt` | Computed |
| **Never purchased** | **No explicit rule** | Customers in master with zero Faktur history — not surfaced | — |
| **Reactivated** | **No explicit rule** | Customer dormant then active again — not tracked as event | — |
| **Lost** | **No explicit state** | Proxy: M29 "stopped after history", severe decline | — |

### 7.2 Activity metrics

| Metric | Portal | Desktop |
| ------ | ------ | ------- |
| Active customer count | M17, Sales snapshot | — |
| Dormant customer count | M17 | — |
| Days since last purchase | M17 attention detail, M29 `DaysSinceLastFaktur` | SF1 max date |
| Customer reach trend (MoM) | **No** — current month only | — |
| Effective call rate | **No** | RO3 `EffectiveCallView` |
| Visit execution per customer | **No aggregate** | M18.5 per salesman-day |
| Route membership | **No analytics** | `BTR_SalesRuteItem`, FT1 |
| GPS coverage | **No** | RO4 `LocationCoverageInfoForm` |

### 7.3 Field activity (customer grain — live query only)

**Route:** `/dashboard/field-activity` — M18.5, no snapshot

Per customer on selected salesman-day: Planned, Actual, Missed, Unplanned, Effective Call, GPS validation bands (≤50m / 50–100m / >100m).

**Data:** `IEffectiveVisitPlanDal`, `IFieldActivityCheckInDal`, `IFieldActivityOrderDal`, `ICustomerCoordinateDal`

**Gap:** No per-customer visit history trend (7/30-day); no link from visit to payment or omzet outcome in portal.

---

## 8. Customer Concentration

### 8.1 Revenue concentration

| Metric | Exists? | Formula | Location |
| ------ | ------- | ------- | -------- |
| Top Omzet Customer % | Yes | Rank-1 MTD omzet ÷ total MTD omzet | M17 |
| Top 10 Omzet ranking | Yes | Top N by MTD omzet | M17 |
| Top 5 / Top 10 / Top 20 cumulative % | **Partial** | Top 1 only as KPI; Top 10 table derivable | M17 table, not cumulative % KPI |
| Pareto analysis | **No** | — | — |
| Salesman Top Customer % | Yes | Max customer omzet ÷ rep omzet | M18 `CustomerConcentration` signal (informational) |

### 8.2 Receivable concentration

| Metric | Exists? | Location |
| ------ | ------- | -------- |
| Top Piutang Customer % | Yes | M17 |
| Top 10 Piutang ranking | Yes | M17 |
| Top 10 / Top 20 Customer % (piutang) | Yes | M14 piutang KPIs |
| Top 5 Customers (executive) | Yes | M16 from M14 |
| Overdue concentration % | Yes | M20 — top debtor share of overdue |
| M29 elevated-risk receivable % | Yes | M29 portfolio KPI |

### 8.3 Combined dependency

| Concept | Exists? | Notes |
| ------- | ------- | ----- |
| Same customer top omzet AND top piutang | **Derivable, not surfaced** | Requires cross-ranking in Customer worker |
| Strategic customer (high omzet + high debt) | **No explicit flag** | M30 strategic boost uses top MTD omzet rank only |
| Portfolio dependency score | **No** | — |

---

## 9. Customer Geography

### 9.1 Dimensions available

| Dimension | On customer master? | On Faktur? | Portal aggregate? |
| --------- | ------------------- | ---------- | ----------------- |
| **Wilayah** | Yes | Yes | M17 segmentation; M20/M29/M30 workload; M22 Top Wilayah by Sales |
| **Kota (city)** | Yes | No | Desktop `CustomerChartRpt` only (orphan) |
| **Area** | No separate field | — | — |
| **Salesman** | Via last Faktur / route | Yes | M18, M20, M30 workload charts |
| **Warehouse** | No on customer | On Faktur (billing origin) | M22 warehouse performance — not customer portfolio |

### 9.2 Geographic reports and dashboards

| Surface | Geographic content |
| ------- | ------------------ |
| M17 Customer Analytics | Segmentation table — customer counts by Wilayah |
| M20 Collection | Top Overdue Wilayah; WilayahHotspot (≥15% of company overdue) |
| M29 Risk Forecast | Risk by Wilayah chart — elevated-risk customer count |
| M30 Collection Opt | Workload by Wilayah (top 10) |
| M22 Location | Top Wilayah by Sales (MTD omzet ranking) |
| M27 Cash Flow | Wilayah hotspot — due exposure |
| Piutang Report | No wilayah column exposed (available in DAL) |
| FF1 Desktop | Full Wilayah per row |

### 9.3 Attribution note

Customer `WilayahId` on master may differ from Faktur `WilayahName` at invoice time if master was updated after billing. Portal generally uses Faktur wilayah for sales metrics and customer master for segmentation.

---

## 10. Customer Lifecycle

### 10.1 Approved lifecycle model (Product Owner decision)

M31 adopts a **computed lifecycle** — not stored on master data. Thresholds are **configurable later**; initial rules below are PO-approved starting points.

| Stage | Approved rule (V1 starting point) | Existing BTR inputs |
| ----- | ----------------------------------- | ------------------- |
| **Never Purchased** | Customer in master with **zero Faktur history** | `ICustomerLastFakturDal` absent / no history |
| **New** | First purchase within **90 days** | First Faktur date vs today |
| **Growing** | Sales increasing, **low forward risk** | M29 decline absent; omzet trend vs prior month |
| **Mature** | **Stable purchasing** — active, not declining, not new | MTD activity + no M29 severe/moderate decline |
| **Declining** | M29 **declining forecast** signals | M29 CRF-D rules (moderate/severe decline) |
| **Dormant** | No purchase **≥ 90 days**, prior history exists | M17 `DormantDaysThreshold = 90` — **authoritative** |

**Not adopted:** explicit **Lost** state — Dormant covers inactivity; Exit Review is an **action**, not a lifecycle stage.

**Never-purchased customers:** Included and flagged — not hidden. Surfaces master-data quality for management.

### 10.2 Pre-M31 lifecycle state (discovery baseline)

| Lifecycle stage | Existed before M31? | Notes |
| ----------------- | --------------------- | ----- |
| Active (MTD) | Yes — M17 | Remains; distinct from lifecycle stage |
| Dormant (90d) | Yes — M17/M20 | Adopted into lifecycle taxonomy |
| Approaching dormant | Yes — M29 only | Feeds Declining / pre-Dormant signals |
| New / Growing / Mature | **No** | **New for M31** — computed |
| Lost | **No** | Not adopted |
| Never purchased | **No** | **New for M31** — computed |

### 10.3 Activity-based flow (approved composition)

```text
Never Purchased  (master, zero Faktur history)
       ↓
   New  (first purchase < 90 days)
       ↓
   Growing  (increasing sales, low risk)  ←→  Mature  (stable purchasing)
       ↓                                      ↓
   Declining  (M29 decline forecast)  ────────┘
       ↓
   Dormant  (≥ 90 days, prior history)
       ↓
   Recover action / Legacy debt context  (M20 LegacyDebt when balance remains)
```

---

## 11. Existing Dashboards

### 11.1 Customer-primary dashboards

#### M17 — Customer Analytics (`/dashboard/customers`)

| Section | KPIs / content |
| ------- | -------------- |
| Attention Cards | Overdue, >90d exposure, Top Omzet %, Top Piutang %, Active, Dormant, Plafond breach, Suspended+Sales |
| Attention List | Customer × signal (Overdue, Dormant, PlafondBreach, SuspendedWithSales) |
| Rankings | Top 10 Omzet, Top 10 Piutang |
| Segmentation | Klasifikasi, Wilayah, Active vs Dormant |

**API:** `GET /api/dashboard/customers`

#### M29 — Customer Risk Forecast (`/dashboard/customer-risk-forecast`)

| Section | Content |
| ------- | ------- |
| Portfolio KPIs | Customers at risk, elevated-risk receivable, portfolio health score, signal mix counts |
| Charts | Risk category distribution, signal mix, risk by Wilayah, elevated vs total piutang |
| Tables | Top 20 risk customers, Top 25 forward attention, Top 15 recommendations |

**API:** `GET /api/dashboard/customer-risk-forecast`

#### M30 — Collection Optimization (`/dashboard/collection-optimization`)

| Section | Content |
| ------- | ------- |
| Executive summary | Daily collection brief |
| Workload KPIs | Actions today, immediate collection, proactive reminders, credit review, sales recovery |
| Priority queue | Top 30 by `CollectionPriorityScore` |
| Specialized queues | ProactiveReminder, CreditReview, SalesRecovery, EscalateManagement (15 each) |
| Impact | Top 15 impact opportunities |
| Workload charts | By Wilayah, Salesman |

**API:** `GET /api/dashboard/collection-optimization`

### 11.2 Customer-secondary dashboards

| Dashboard | Route | Customer content |
| --------- | ----- | ---------------- |
| **Management Attention Center** | `/dashboard` | Top 5 Customers (piutang), Overdue count, Top Customer % |
| **Piutang** | `/dashboard/piutang` | Total Customer, Overdue Customer, Top 20 Customer Risk + aging breakdown, Top 10/20 % |
| **Collection** | `/dashboard/collection` | Top Overdue Customers/Salesmen/Wilayah; legacy, chronic, plafond signals |
| **Cash Flow Forecast** | `/dashboard/cash-flow-forecast` | Top 10 Collection Risks (customer grain) |
| **Salesman Performance** | `/dashboard/salesmen` | Dormant portfolio, customer concentration per rep |
| **Field Activity** | `/dashboard/field-activity` | Per-customer visit status (live) |
| **Branch/Warehouse Performance** | `/dashboard/locations` | Top Wilayah by Sales (territory, not customer list) |
| **Sales** | `/dashboard/sales` | Total Customer computed but **not shown on detail UI** |
| **Alert Center** | `/alerts` | Customer category: Overdue, Dormant, PlafondBreach, SuspendedWithSales (M17); Collection category overlaps (M20 canonical) |

### 11.3 Dashboard refresh cadence

| Worker | Interval | Customer-related output |
| ------ | -------- | ----------------------- |
| `RefreshDashboardCustomerSnapshotWorker` | ~30 min | M17 + M29 + M30 |
| `RefreshDashboardPiutangSnapshotWorker` | ~15 min | M14 per-customer aging |
| `RefreshDashboardCollectionSnapshotWorker` | ~30 min | M20 + M27 |
| `RefreshDashboardSalesmanSnapshotWorker` | ~30 min | M18 portfolio signals |
| Field Activity | Live | No snapshot |

---

## 12. Existing Reports

### 12.1 Portal reports

#### Sales Report (`/reports/sales`)

| Aspect | Detail |
| ------ | ------ |
| Purpose | Faktur-level sales evidence |
| Grain | One row per Faktur |
| Customer columns | Customer name |
| Period | Current calendar month; max 31 days |
| Aggregates | None (no footer totals) |
| DAL | `SalesReportDal` → `IFakturViewDal` |
| Drill-down from | M17, M18, M23, M24 investigation |
| Customer filters | Client-side search by name |

#### Piutang Report (`/reports/piutang`)

| Aspect | Detail |
| ------ | ------ |
| Purpose | Open receivable evidence |
| Grain | One row per open Faktur (`KurangBayar > 1`) |
| Customer columns | Customer, Sales |
| Footer | Total Piutang, Total Customer |
| Period | Default current month on Jatuh Tempo; `allOpenBalances` mode for dashboard reconciliation |
| DAL | `PiutangReportDal` → `IPiutangSalesWilayahDal` |
| Drill-down from | M14, M17, M20, M23, M24 |
| Hidden DAL fields | Retur, Potongan, BayarTunai, BayarGiro, WilayahName, Alamat |

**No portal reports for:** Retur, Pelunasan, Effective Call, Visit Plan, Faktur Per Customer (line detail).

**Approved for M31:** Dedicated **Customer Report** in portal — drill-down chain extends beyond Sales/Piutang reports (Section 18 Q18).

### 12.2 Desktop reports (customer-centric)

| Report | Menu | Purpose | DAL | Customer aggregates |
| ------ | ---- | ------- | --- | ------------------- |
| **Info Faktur Jual** | SF1 | All Fakturs | `IFakturViewDal` | Groupable by customer — ad hoc ranking |
| **Faktur Per Customer** | SF4 | Line-item sales by customer | `IFakturPerCustomerDal` | Per customer line detail |
| **Piutang Sales Wilayah** | FF1 | Full receivable grid | `IPiutangSalesWilayahDal` | Richest row: Retur, payments, Wilayah |
| **Pelunasan Info** | FF4 | Payment history | `IPelunasanInfoDal` | Per customer/Faktur |
| **Penerimaan Pelunasan Sales** | FF2 | Collections by salesman-day | `IPenerimaanPelunasanSalesDal` | Salesman grain |
| **Retur Jual Report** | RF1 | Retur headers | `IReturJualViewDal` | Per customer retur value |
| **Retur Jual Barang Report** | RF2 | Retur lines | `IReturJualBrgViewDal` | Customer + supplier breakdown |
| **Effective Call Info** | RO3 | Visit effectiveness | `IEffectiveCallDal` | Per visit/customer |
| **Location Coverage** | RO4 | GPS coverage | `ICustomerDal.ListLocation()` | Coordinate status |
| **Customer Chart** | *(orphan)* | Customer count by Kota | `ICustomerDal.ListData()` | Top 10 Kota count |
| **Piutang Tracker** | FT5 | Faktur lifecycle | `IPiutangTrackerDal` | Collection timeline |

### 12.3 Report-to-dashboard traceability (customer KPIs)

| KPI | Validating report | Match type |
| --- | ----------------- | ---------- |
| Top 10 Omzet | Sales Report — group by customer, sum Total | Derivable |
| Top 10 Piutang | Piutang Report (all-open mode) — group by customer | Exact when unfiltered |
| Overdue customer count | Piutang Report — distinct customers past Jatuh Tempo | Derivable |
| Dormant count | Sales Report — max date per customer vs 90d | Derivable |
| Plafond breach | Piutang Report + Customer master join | Partial — requires master |
| M29 risk row | No dedicated report | Dashboard only |
| M30 action row | No dedicated report | Dashboard only |
| Retur ratio | Desktop RF1 + SF1 | Desktop only |
| Payment lag | Desktop FF4 | Desktop only |

---

## 13. Existing Business Rules

Rules below are **discovered implementations**. Threshold values shown are current code constants unless marked as PO-approved.

### 13.1 Customer identity and data scope

| Rule | Value | Location |
| ---- | ----- | -------- |
| Customer key | Code-first, fallback name | `DashboardCustomerKeyResolver` |
| Open balance threshold | `KurangBayar > 1` | All piutang aggregators |
| Void Faktur exclusion | `VoidDate = '3000-01-01'` | `FakturViewDal` SQL |
| Sales period | Current calendar month | Sales, M17 omzet |
| Piutang period | All-time open snapshot | M14, M17 piutang |
| Pelunasan UangMuka exclusion | `JenisLunas = 2` excluded | Payment behavior DALs |

### 13.2 Activity rules

| Rule | Threshold | Location |
| ---- | --------- | -------- |
| Active customer | Any MTD Faktur | `DashboardCustomerAggregator.BuildActiveSet` |
| Dormant | ≥90 days since last Faktur, prior history, not active MTD | M17 — `DormantDaysThreshold = 90` |
| Approaching dormant | 60–79 days | M29 `ApproachingDormantDaysMin = 60` |
| Imminent dormant | 80–89 days | M29 CRF-I rules |
| Legacy debt | Dormant + open balance | M20 — same 90d threshold |

### 13.3 Credit rules

| Rule | Condition | Location |
| ---- | --------- | -------- |
| Plafond breach | `Plafond > 0 AND openBalance > Plafond` | M17 |
| Plafond breach + overdue | Overdue + open > plafond | M20 |
| Approaching plafond | Projected ≥ 90% of plafond | M29 — `ApproachingPlafondRatio = 0.90` |
| Projected breach | Projected open > plafond (30d horizon) | M29 |
| Credit utilization % | `OpenBalance / Plafond × 100` | M30 |
| Suspended + sales | `IsSuspend AND` MTD Faktur | M17 |

### 13.4 Sales and decline rules

| Rule | Threshold | Location |
| ---- | --------- | -------- |
| Moderate decline | Projected month omzet / prior month < 70% | M29 — prior month floor Rp 1M default |
| Severe decline | Ratio < 50% | M29 |
| Stopped after history | Prior ≥ floor, MTD = 0, days < 90 | M29 CRF-D03 |
| Top Omzet strategic boost | Top 10 MTD (+120 score), Top 20 (+60) | M30 |

### 13.5 Collection and payment rules

| Rule | Threshold | Location |
| ---- | --------- | -------- |
| Avg payment lag — likely late | ≥ 7 days | M29 |
| Avg payment lag — escalating | ≥ 14 days | M29 |
| No payment recency | 30 days | M29 |
| Payment lag lookback | 90 days, min 2 settled Fakturs | `CustomerPaymentBehaviorDal` |
| Recovery vs billing | MTD collections ÷ MTD omzet × 100 | M20 |
| Wilayah hotspot | Wilayah overdue ≥ 15% of company overdue | M20 |
| Chronic overdue | Any >90d bucket exposure | M20, M14 |
| Low recovery salesman | Rep collections < rep omzet MTD | M20 |

### 13.6 Concentration and ranking rules

| Rule | Value | Location |
| ---- | ----- | -------- |
| Top N (customer rankings) | 10 (M17); 20 (M14, M29 customers) | Aggregator constants |
| Executive truncation | Top 5 from piutang ranking | M16 composer |
| Overdue concentration | Top debtor ÷ total overdue | M20 |
| Salesman customer concentration | Informational — no auto threshold | M18 |

### 13.7 Alert Center deduplication

| Rule | Location |
| ---- | -------- |
| M20 customer signals supersede M17 Overdue, PlafondBreach, Dormant where overlapping | `DashboardAlertCenterComposer.ApplyDeduplication` |
| M20 HighOverdueWorkload suppresses M18 HighOverdueExposure | same |
| Top 20 per category cap | `AlertCenterRegistry` |

### 13.8 Sales attribution rules

| Domain | Attribution | Location |
| ------ | ----------- | -------- |
| Sales omzet | Invoice-time `SalesPersonId` on Faktur | `FakturView` |
| Piutang exposure | Invoicing salesman on open Faktur | `IPiutangOpenBalanceWithSalesmanDal` |
| Dormant portfolio | Last invoicing salesman | `ICustomerLastFakturDal.ListLastFakturWithSalesmanByCustomer` |
| Collection workload | Invoicing salesman (no separate collector) | M20 analysis |
| Route ownership | `BTR_SalesRuteItem` — not used in portal KPIs | Desktop FT1 |

### 13.9 Effective call (Desktop only)

| Rule | Value | Location |
| ---- | ----- | -------- |
| Effective call | `OrderCount > 0` same day as visit | `EffectiveCallView` / RO3 |

---

## 14. Reuse Opportunities

### 14.1 High-confidence reuse (do not reimplement)

| Asset | Reuse for M31 |
| ----- | ------------- |
| `PiutangAgingBucketResolver` | Any aging or overdue classification |
| `DashboardCustomerKeyResolver` | Customer grain consistency |
| M17 aggregator outputs | Current-state attention, Top 10 omzet/piutang, segmentation |
| M29 in-memory contexts | Forward risk category, signals, recommendations, `RiskPriorityScore` |
| M30 in-memory contexts | Collection action category, priority score, impact amount |
| M14 per-customer aging DTOs | Working capital breakdown by bucket |
| M20 collection snapshot cross-read | Recovery vs billing, overdue workload |
| M18 salesman dormant portfolio | Rep-level portfolio health context |
| M22 Top Wilayah by Sales | Geographic portfolio context |
| `Top10RankingTable.vue` | Ranking UI pattern |
| Customer snapshot worker chain | Extend after M30 step (same pattern as M28.5, M30) |
| Investigation registry + drill-down routes | Evidence navigation to Sales/Piutang reports |

### 14.2 Compose-without-duplication patterns

| Pattern | Established by | M31 implication |
| ------- | -------------- | --------------- |
| Consume prior milestone in-memory | M30 consumes M29 | M31 should consume M17 + M29 + M30 contexts |
| Cross-read related snapshot | M30 reads M20 collection | M31 may cross-read M18 salesman, M22 location |
| Strategic customer boost from Top Omzet rank | M30 | Reusable for "grow" vs "collect" routing |
| Portfolio health score | M29 `PortfolioHealthScore` | Candidate headline KPI — already computed |
| Executive summary builder pattern | M29, M30 summary builders | Plain-language portfolio brief |

### 14.3 Desktop DALs available for future portal exposure (not currently used)

| DAL | Potential portfolio signal |
| --- | -------------------------- |
| `IReturJualViewDal` | Return ratio, quality risk |
| `IFakturPerCustomerDal` | Line-level mix, SKU breadth per customer |
| `IEffectiveCallDal` | Relationship engagement |
| `IPelunasanInfoDal` | Payment behavior detail |
| `ICustomerDal.ListLocation()` | Field coverage completeness |

---

## 15. Do Not Duplicate

| Capability | Authoritative owner | M31 must not |
| ---------- | ------------------- | ------------ |
| Aging buckets | M14 `PiutangAgingBucketResolver` | Redefine bucket boundaries |
| Dormant rule (90d) | M17 `DashboardCustomerAggregator` | Change threshold without PO approval |
| Active customer definition | M17 | Invent parallel "active" semantics |
| Plafond breach (current) | M17 | Recompute separately |
| Forward risk signals | M29 `CustomerRiskSignalBuilder` | Regenerate forecast rules |
| Risk categories (5-band) | M29 `CustomerRiskForecastPolicy` | Create parallel scoring |
| Collection priority queue | M30 | Duplicate as collection workspace |
| Recovery vs billing % | M20 | Recompute company recovery |
| Overdue workload rankings | M20 | Replace with balance-only ranking |
| Alert Center signal registry | M23 `AlertCenterRegistry` | Add signals without registry change |
| Salesman achievement / target | M18 | Mix into customer portfolio KPIs |
| Cash flow forecast | M27 | Duplicate company cash projection |
| Inventory / purchasing metrics | M19–M21 | Force customer dimension where none exists |

| **Semantic boundary:** M31 is **portfolio optimization** (Grow · Retain · Protect · Recover) — not a replacement for M17 (current attention), M29 (forward risk), or M30 (today's collection operations). **Collect action links to M30 — never duplicates collection queue.**

| **Composition rule:** M31 must compose M17 + M29 + M30 + M18 summary — not recalculate their rules (PO approved).

---

## 16. Data Gaps

### 16.1 Profitability (confirmed unavailable — PO accepted omzet proxy)

| Metric | Portal | Desktop | M31 status |
| ------ | ------ | ------- | ---------- |
| Gross margin per customer | ❌ | ❌ aggregate | Out of scope — future Mxx |
| Contribution per customer | ❌ | ❌ | Out of scope |
| Discount % per customer | ❌ | Partial | Out of scope |
| Return ratio per customer | ❌ | Derivable | **Future milestone** (PO Q10) |
| Net sales (after retur) | ❌ | ❌ | **Not required** (PO Q11) |
| Customer value | Omzet proxy only | — | **Approved** — document as NOT profitability (PO Q9) |

### 16.2 Activity and relationship gaps

| Gap | Impact on portfolio questions |
| --- | ------------------------------ |
| No purchase frequency KPI | Cannot rank by buying cadence |
| No explicit Lost / New / Growing lifecycle | Cannot segment portfolio by lifecycle stage |
| No reactivation tracking | Cannot identify recovering accounts |
| No per-customer visit history in portal | Cannot combine field engagement with portfolio tier |
| No Faktur Kembali aggregate | Document workflow completeness invisible |
| No never-purchased customer KPI | Master records without sales not surfaced |

### 16.3 Master data gaps

| Gap | Risk |
| --- | ---- |
| IsSuspend not saved from CustomerForm | Suspended+Sales signal may be unreliable |
| No strategic tier on master | "Investment-worthy" requires computed definition |
| Klasifikasi is free-text user-defined | Inconsistent segmentation across deployments |
| Salesman not on customer FK | Attribution depends on last Faktur inference |
| CreditBalance semantics unclear in analytics | Not used in portal breach rules (open piutang used instead) |

### 16.4 Historical and trend gaps

| Gap | Notes |
| --- | ----- |
| No customer snapshot history | Point-in-time only — no trend-of-trends |
| No month-over-month active customer trend | Current month count only |
| No customer balance history | Piutang snapshot is current state |
| No portal retur history | Desktop RF1 only |

### 16.5 Cross-domain gaps

| Gap | Status after PO decisions |
| --- | ------------------------- |
| Portfolio action taxonomy | **Resolved** — Grow, Retain, Protect, Collect, Review Credit, Recover, Monitor, Exit Review |
| Collection queue duplication | **Resolved** — reuse M30 via link |
| Field activity integration | **Deferred** — M18.5 stays independent; future enhancement |
| Customer Report in portal | **Gap to close in M31** — PO approved dedicated report |
| Working-capital-per-customer beyond open balance | Remains partial — open piutang only (CreditBalance excluded) |
| Purchase frequency for tier computation | **New logic required** — not in BTR today; needed for computed tiers |
| Growing / Mature lifecycle distinction | **New logic required** — composes M29 decline + omzet trend |

---

## 17. Risks

| # | Risk | Severity | Mitigation consideration |
| - | ---- | -------- | ------------------------ |
| R1 | **Profitability-blind portfolio decisions** | High | **Mitigated** — PO accepts omzet proxy; document "Customer Value = Omzet Proxy, NOT profitability" |
| R2 | **Salesman attribution inconsistency** | Medium | **Mitigated** — PO locked **last invoicing salesman** as portfolio owner |
| R3 | **IsSuspend master data integrity** | Medium | **Mitigated** — Desktop fix is prerequisite work item before M31 relies on suspend signals |
| R4 | **Customer key collisions** — code-first with blank codes falls back to name | Medium | Rows without resolvable key excluded from customer grain (existing M17/M29 behavior) |
| R5 | **Snapshot staleness (~30 min)** | Low | Same as all optimization dashboards; freshness indicator exists |
| R6 | **Scope creep into M17/M29/M30** — reimplementing existing rules | High | **Mitigated** — PO approved composition-only architectural constraint |
| R7 | **False precision on strategic/tier classification** | Medium | **Mitigated** — PO approved computed tiers from omzet, piutang, frequency, risk |
| R8 | **Geographic mismatch** — master Wilayah vs Faktur Wilayah | Low | Consistent with M17 segmentation approach |
| R9 | **Working capital overstatement** — open balance without netting expected collections | Medium | M29/M30 forward signals partially address; no full cash conversion model |
| R10 | **Executive scope creep** — detailed tables on executive page | Medium | **Mitigated** — Executive integration summary cards only, link to M31 |

---

## 18. Product Owner Decisions

**All open questions resolved.** Recorded 2026-06-22. Authoritative for Architect engagement.

### 18.1 Portfolio scope

| # | Decision |
| - | -------- |
| **Q1 — Customer coverage** | Cover **all customers**. Default dashboard view: **Attention Customers only**. Executive users can switch to **All Customers**. Complete portfolio visibility with actionable default. |
| **Q2 — Audience** | **Both** Owner/GM and Sales Manager — single dashboard, different perspectives. Owner/GM: Portfolio Health, Concentration Risk, Working Capital. Sales Manager: Assigned portfolio, Growth opportunities, Retention, Collection coordination. |
| **Q3 — Focus** | **Growth + Retention + Risk** — not risk-only. Four portfolio objectives: **Grow · Retain · Protect · Recover**. This distinguishes M31 from M30. |
| **Q4 — Relationship with M17** | M31 **supplements**, never replaces M17. See milestone relationship in Section 1. |

### 18.2 Lifecycle and segmentation

| # | Decision |
| - | -------- |
| **Q5 — Lifecycle** | **Adopt computed lifecycle.** Initial stages: New (< 90 days since first purchase), Growing (increasing sales, low risk), Mature (stable purchasing), Declining (M29 declining forecast), Dormant (≥ 90 days no purchase). Thresholds configurable later. |
| **Q6 — Portfolio tier** | **Never rely on Klasifikasi** for tier assignment. Use **computed tiers**: Strategic, High Value, Medium Value, Low Value — from omzet, piutang, purchase frequency, and risk. Klasifikasi remains a **filter only**. |
| **Q7 — HargaType** | **Out of scope** for M31. Operationally useful; not portfolio-relevant. |
| **Q8 — Never purchased** | **Include and flag.** Status: `Never Purchased`. Not hidden — surfaces master-data quality. |

### 18.3 Profitability and value

| # | Decision |
| - | -------- |
| **Q9 — Value metric** | **Yes** — omzet is acceptable value proxy. Document explicitly: **Customer Value = Omzet Proxy, NOT profitability.** Future milestone may introduce margin. |
| **Q10 — Return ratio** | **Future milestone** — not M31. |
| **Q11 — Net sales after retur** | **Not required.** Portal consistently uses invoiced omzet; changing semantics now creates inconsistency. |

### 18.4 Actions and ownership

**Approved portfolio action taxonomy** — mutually exclusive, one primary action per customer row:

| Action | Meaning | Owner |
| ------ | ------- | ----- |
| **Grow** | Opportunity customer | Sales |
| **Retain** | Valuable customer showing decline | Sales |
| **Protect** | Strategic customer with elevated risk | Management + Sales |
| **Collect** | Collection required | Finance — **link to M30 queue, never duplicate** |
| **Review Credit** | Credit/plafond issue | Finance |
| **Recover** | Dormant customer | Sales |
| **Monitor** | Watch only | Management |
| **Exit Review** | Very low value + high risk | Management |

| # | Decision |
| - | -------- |
| **Q12 — Actions** | See taxonomy above. |
| **Q13 — Action owner** | See Owner column above. |
| **Q14 — M30 queue** | **Reuse M30** — portfolio page links to Collection Queue. Never duplicate collection priority logic. |

### 18.5 Integration

| # | Decision |
| - | -------- |
| **Q15 — Executive Dashboard** | **Yes — summary only.** Example cards: Portfolio Healthy %, Customers At Risk count, Strategic Customers At Risk count. No detailed tables on executive page. |
| **Q16 — Salesman signals** | **Yes — summary only** on portfolio customer rows. Example: Owner (salesman name), Achievement %, High Piutang Exposure flag. Do not reproduce M18 dashboard. |
| **Q17 — Field activity** | **Not in M31 V1.** M18.5 stays independent. Possible future enhancement. |
| **Q18 — Customer Report** | **Yes — strongly recommended.** Approved drill-down chain: Executive → Portfolio → Customer Analytics → **Customer Report** → Desktop. |

### 18.6 Data and attribution

| # | Decision |
| - | -------- |
| **Q19 — Portfolio owner** | **Last invoicing salesman** — existing portal attribution standard. Do not invent route-owner model. |
| **Q20 — CreditBalance** | **Ignore for M31.** Continue using **open piutang** as credit exposure. Keeps semantics consistent across dashboards. |
| **Q21 — IsSuspend** | **Desktop fix is prerequisite** — separate work item. Do not redesign M31 around bad master data. |

### 18.7 Architectural constraint (Product Owner approved)

> **M31 must be a composition milestone, not a calculation milestone.**

M31 primarily composes and prioritizes insights from:

| Source | Role in M31 |
| ------ | ----------- |
| M17 | Current-state analytics and attention signals |
| M29 | Forward risk forecast and portfolio health score |
| M30 | Collection actions — linked, not duplicated |
| M18 | Salesman context — summary only (owner, achievement, exposure flags) |
| M22 | Geographic/territory context — optional |

### 18.8 Decision summary table

| Topic | Decision |
| ----- | -------- |
| Scope | All customers; default Attention Customers only |
| Audience | Owner/GM + Sales Manager (one dashboard) |
| Focus | Grow + Retain + Protect + Recover |
| Relationship | Supplement M17 / M29 / M30 |
| Lifecycle | New, Growing, Mature, Declining, Dormant (+ Never Purchased) |
| Portfolio tier | Computed — Strategic / High / Medium / Low Value |
| HargaType | Out of scope |
| Never purchased | Show and flag |
| Value metric | Omzet proxy (not profitability) |
| Return ratio | Future milestone |
| Net sales | Not required |
| Actions | 8-category taxonomy (see 18.4) |
| Collection queue | Reuse M30 via link |
| Executive integration | Summary cards only |
| Salesman signals | Summary on portfolio rows |
| Field activity | Future enhancement |
| Customer Report | Yes — new portal report |
| Salesman attribution | Last invoicing salesman |
| Credit analysis | Open piutang only |
| IsSuspend | Desktop prerequisite fix |

---

## 19. Low-Fidelity Wireframes

Wireframes reflect **approved Product Owner decisions** (Section 18). Discussion aids only — not visual design or implementation specification.

### 19.1 Customer Portfolio Dashboard (primary surface)

```text
Customer Portfolio Optimization                    [As-of: timestamp]

View: [Attention Customers ▼]  |  All Customers     Filter: Wilayah | Klasifikasi | Tier | Lifecycle

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
EXECUTIVE SUMMARY (plain-language portfolio brief)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
PORTFOLIO HEALTH
┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│ Health Score │ │ Strategic    │ │ At-Risk      │ │ Working      │
│ (from M29)   │ │ At Risk      │ │ Receivable   │ │ Capital Tied │
└──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
PORTFOLIO DISTRIBUTION
[Chart: Lifecycle — New | Growing | Mature | Declining | Dormant | Never Purchased]
[Chart: Tier — Strategic | High | Medium | Low Value]
[Chart: Omzet concentration vs Piutang concentration]

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
PRIORITY PORTFOLIO QUEUE (default: Attention Customers)
Customer | Tier | Lifecycle | Action | Owner | Salesman | Reason
─────────────────────────────────────────────────────────────────
... Grow / Retain / Protect / Review Credit / Recover / Monitor / Exit Review
... Collect → [Link to M30 Collection Queue for this customer]

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
ACTION SEGMENTS (expandable — mutually exclusive primary action)
┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐
│ Grow     │ │ Retain   │ │ Protect  │ │ Recover  │ │ Exit     │
│          │ │          │ │          │ │          │ │ Review   │
└──────────┘ └──────────┘ └──────────┘ └──────────┘ └──────────┘
┌──────────┐ ┌──────────┐
│ Review   │ │ Monitor  │     Collect → link to M30 (not duplicated here)
│ Credit   │ │          │
└──────────┘ └──────────┘

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
CONCENTRATION & GEOGRAPHY
[Top 10 Omzet]  [Top 10 Piutang]  [Wilayah breakdown — M22 context optional]

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
NAVIGATION
→ Customer Analytics (M17) | Risk Forecast (M29) | Collection Opt (M30)
→ Customer Report | Sales Report | Piutang Report
```

### 19.2 Portfolio Customer Detail (drawer or sub-page)

```text
Customer: [Name] ([Code])     Tier: Strategic    Lifecycle: Declining
Wilayah | Klasifikasi (filter) | Owner: John (last invoicing salesman)

SALESMAN CONTEXT (summary — not full M18)
Achievement 83%  |  High Piutang Exposure: Yes

CURRENT STATE (M17)          FORWARD RISK (M29)         PORTFOLIO ACTION
Active/Dormant flags         Risk category              Primary action + owner
MTD Omzet                    Signal list                Reason text
Open balance (piutang)       Recommendation             Collect? → Link M30

Customer Value = Omzet Proxy (NOT profitability)

EVIDENCE LINKS
[Customer Report] [Customer Analytics] [Risk Forecast] [Collection Opt]
[Sales Report] [Piutang Report]
```

### 19.3 Executive Portfolio Brief (approved M16 promotion)

```text
Portfolio                                    [link → M31 full dashboard]

Healthy                           82%
Customers At Risk                 24
Strategic Customers At Risk        6

(No detailed tables — summary cards only)
```

### 19.4 Approved investigation chain

```text
Executive Dashboard (summary cards)
        ↓
Portfolio Optimization (M31)
        ↓
Customer Analytics (M17) / Risk Forecast (M29) / Collection Opt (M30)
        ↓
Customer Report (new — approved)
        ↓
Sales Report / Piutang Report
        ↓
BTR Desktop (operational resolution)
```

---

## 20. Approved Scope and Constraints for Architect

Product Owner decisions (Section 18) supersede prior analyst recommendations. This section records **binding constraints** for implementation planning.

### 20.1 Composition rule (non-negotiable)

M31 is a **composition milestone, not a calculation milestone**. Extend `RefreshDashboardCustomerSnapshotWorker` after M30; consume in-memory contexts from M17, M29, and M30. Cross-read M18 (summary) and optionally M22. **Do not recalculate** aging, dormant, plafond breach, recovery %, or M29 forecast rules.

### 20.2 In scope (V1)

| Capability | Source / approach |
| ---------- | ----------------- |
| All-customer portfolio with default Attention filter | Compose M17 attention + M29 elevated risk + portfolio action logic |
| Computed lifecycle (6 states) | New logic composing M17 dormant + M29 decline + first-purchase date |
| Computed tiers (4 tiers) | New logic from omzet, piutang, frequency, risk — **not Klasifikasi** |
| 8 portfolio actions with owners | New composition layer — Collect links to M30 |
| Executive summary cards | M16 promotion — Healthy %, At Risk, Strategic At Risk |
| Salesman summary on rows | M18 cross-read — owner, achievement %, exposure flag |
| Customer Report (portal) | **New report surface** — approved prerequisite for drill-down chain |
| Never Purchased flag | Master customers with zero Faktur history |
| Omzet value proxy | Document on every value reference |

### 20.3 Out of scope (V1)

| Capability | Reason |
| ---------- | ------ |
| HargaType in portfolio logic | PO decision Q7 |
| Return ratio / net sales | Future milestone / semantic consistency |
| Profitability / margin | No data; future Mxx |
| Field activity (M18.5) | Future enhancement |
| M30 collection queue duplication | Reuse via link only |
| Alert Center integration | Not in PO V1 scope |
| CreditBalance in exposure | Open piutang only |
| Replacing M17 / M29 / M30 dashboards | Supplement only |

### 20.4 Prerequisites (separate work items)

| Prerequisite | Owner |
| ------------ | ----- |
| IsSuspend CustomerForm Desktop fix | Desktop implementer — before M31 relies on suspend signals |
| Customer Report portal surface | M31 scope (approved) — Architect must include in plan |

### 20.5 New business logic required (Architect to define thresholds)

These are **not** available in existing milestones and require new Policy/Builder work — but must **compose** existing inputs:

1. **Portfolio tier** — Strategic / High / Medium / Low Value from omzet, piutang, purchase frequency, risk
2. **Lifecycle** — Growing vs Mature distinction; Never Purchased detection
3. **Portfolio action resolver** — mutually exclusive primary action with precedence rules
4. **Attention default filter** — which customers appear in default view
5. **Portfolio priority score** — ordering within attention queue (distinct from M30 collection priority)

### 20.6 Suggested next workflow step

```text
This analysis + PO decisions (complete)
  ↓
Analyst creates docs/features/customer-portfolio-optimization/feature.md  ✓
  ↓
Architect produces implementation-plan-m31-customer-portfolio-optimization.md
  ↓
Desktop: IsSuspend fix (parallel prerequisite)
  ↓
Implementer executes approved plan
```

---

## Document Maintenance

When M31 implementation begins:

1. ~~Create `docs/features/customer-portfolio-optimization/feature.md` from Section 18 decisions~~ ✓ Done 2026-06-22
2. Update `docs/features/btr-portal/btr-portal-domain.md` with M31 dashboard entry and lifecycle/tier concepts
3. Architect produces `docs/work/btr-portal/implementation-plan-m31-customer-portfolio-optimization.md`
4. Track IsSuspend Desktop fix as prerequisite work item
5. Archive or supersede this analysis after knowledge extraction

**Success criterion:** Architect can produce an implementation plan from this document without additional business clarification.

---

## Appendix A — Cross-Domain Relationship Matrix

| Related domain | Customer participation | Portal surface | Customer grain? |
| -------------- | ---------------------- | -------------- | --------------- |
| Sales | Faktur omzet, active count, Top Omzet | M11, M17, M26 | Yes |
| Piutang | Open balance, aging, Top Piutang | M14, M17 | Yes |
| Collection | Recovery, overdue, legacy, M30 actions | M20, M27, M30 | Yes |
| Customer Analytics | Attention, segmentation | M17 | Yes |
| Customer Risk Forecast | Forward signals, health score | M29 | Yes |
| Salesman Performance | Dormant portfolio, concentration per rep | M18 | Rep grain (customer attributed) |
| Field Activity | Visit plan, check-in, effective call | M18.5 | Per customer on salesman-day |
| Location / Wilayah | Territory sales concentration | M22, M17 segmentation | Territory grain |
| Inventory | — | M19 | No |
| Purchasing | — | M21 | No |
| Retur | Retur value per customer | Desktop RF1 | Desktop only |
| Alert Center | Customer + Collection alert categories | M23 | Yes |
| Executive | Top 5 customers by piutang; **M31 portfolio summary cards (approved)** | M16 | Partial |
| Investigation / Drill-down | Customer pre-filter on reports; **Customer Report (approved)** | M24 + M31 |

## Appendix B — Customer Worker Data Flow

```text
RefreshDashboardCustomerSnapshotWorker
  │
  ├─ Load: IFakturViewDal, IPiutangOpenBalanceDal, ICustomerLastFakturDal,
  │         ICustomerDal, ICustomerOmzetHistoryDal, ICustomerPelunasanSummaryDal,
  │         ICustomerPaymentBehaviorDal
  │
  ├─ Step 1: DashboardCustomerAggregator        → M17 snapshot tables
  │
  ├─ Step 2: DashboardCustomerRiskForecastAggregator → M29 snapshot tables
  │           (in-memory M17 contexts)
  │
  ├─ Step 3: DashboardCollectionOptimizationAggregator → M30 snapshot tables
  │           (in-memory M29 contexts + M20 collection cross-read)
  │
  └─ Step 4: DashboardCustomerPortfolioOptimizer → M31 snapshot tables  [approved — composition only]
              (in-memory M17 + M29 + M30 contexts + M18 summary cross-read + optional M22)
```

## Appendix C — Terminology Mapping (authoritative BTR terms)

| Business term | BTR / Portal term | Source |
| ------------- | ----------------- | ------ |
| Customer | `CustomerModel`, `CustomerCode`, `CustomerName` | `BTR_Customer` |
| Classification | **Klasifikasi** | `BTR_Klasifikasi` |
| Territory | **Wilayah** | `BTR_Wilayah` |
| Price tier | **HargaType** | `BTR_HargaType` |
| Credit limit | **Plafond** | `CustomerModel.Plafond` |
| Suspension | **IsSuspend** | `CustomerModel.IsSuspend` |
| Sales invoice | **Faktur** | `BTR_Faktur`, `FakturView` |
| Receivable | **Piutang**, `KurangBayar` | `BTR_Piutang` |
| Payment | **Pelunasan** | `BTR_PiutangLunas` |
| Due date | **Jatuh Tempo** | `BTR_Piutang` |
| Salesman | **Sales Person** | `BTR_SalesPerson`, Faktur attribution |
| Active customer | Invoiced in current calendar month | M17 rule |
| Dormant customer | No Faktur 90+ days, prior history | M17 rule — adopted into M31 lifecycle |
| Never purchased | Master record, zero Faktur history | M31 lifecycle (approved) |
| Portfolio tier | Strategic / High / Medium / Low Value | M31 computed (approved) — not Klasifikasi |
| Customer value | Omzet proxy | M31 — explicitly NOT profitability |
| Return | **Retur Jual** | `BTR_ReturJual` — future milestone, not M31 |

**Do not introduce parallel vocabulary** in M31 artifacts.
