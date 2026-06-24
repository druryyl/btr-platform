# BTR Portal Analysis — M29 Customer Risk Forecast Dashboard

**Status:** Analysis complete — ready for Architect review and Product Owner approval before implementation.  
**Scope:** Business analysis, customer risk forecasting framework, recommendation definitions, KPI definitions, wireframe, and business rules only. No production code.  
**Date:** 2026-06-21  
**Author role:** Analyst  
**Companion document:** [implementation-plan-m29-customer-risk-forecast.md](./implementation-plan-m29-customer-risk-forecast.md)

**Reference documents:** `docs/foundation/PRODUCT.md`, `docs/foundation/DOMAIN.md`, `docs/foundation/LANDSCAPE.md`, `docs/foundation/WORKFLOW.md`, `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-operational.md`, `docs/features/btr-portal/btr-portal-architecture.md`, `docs/features/cash-flow-forecast/feature.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md`, `docs/work/btr-portal/M14-piutang-dashboard-v2-analysis.md`, `docs/work/btr-portal/M17-Customer-Analytics-Analysis.md`, `docs/work/btr-portal/M20-collection-dashboard-analysist.md`, `docs/work/btr-portal/portal-analysis-m27-cash-flow-forecast.md`, `docs/work/btr-portal/portal-analysis-m28-5-inventory-optimization.md`

---

## 1. Executive Summary

BTR Portal has evolved through six business maturity levels:

| Level | Question | Example dashboards |
| ----- | -------- | ------------------ |
| Reporting | What happened? | Sales Report, Piutang Report |
| Analytics | How is the business performing? | Sales, Piutang, Customer Analytics dashboards |
| Decision Support | What requires attention today? | Executive Dashboard, Alert Center, Collection Management |
| Forecasting | What will probably happen? | Sales Forecast (M26), Cash Flow Forecast (M27), Inventory Forecast (M28) |
| Optimization | Given the forecast, what should management do? | Inventory Optimization (M28.5) |
| **Customer Risk Forecast** | **Which customers will probably become collection or relationship risks, and what should management prepare before problems occur?** | **Customer Risk Forecast (M29)** |

**M14/M20 answer:** *How much is owed today and who is overdue now?*

**M17 answer:** *Which customers require attention across sales and receivables today?*

**M27 answer:** *How much cash will we likely receive by month-end at company level?*

**M29 answers:** *Which customers are likely to become overdue, exceed credit limits, stop purchasing, or require early collection planning within the next 30 days — and why?*

### Key findings

| Finding | Implication for M29 |
| ------- | --------------------- |
| Customer receivable analytics are mature across M14 (piutang aging), M17 (customer attention), M20 (collection effectiveness), and M27 (company cash forecast) | M29 **composes and extends** existing rules — does not redefine aging, dormant, plafond, or recovery semantics |
| `BTRPD_PiutangCustomerAging` already materializes full per-customer aging but is **not API-exposed** | M29 can compute customer aging in-memory from the same `PiutangOpenBalanceDto` rows already loaded in Customer refresh — no cross-snapshot read required |
| M17 dormant rule (90 days) and plafond breach rule are authoritative | Inactivity forecast uses **lead indicators** (60–89 days) before dormant; credit forecast **projects** open balance + billing pace |
| M27 `CashFlowCollectionRiskBuilder` defines forward-looking collection risk at company level (due-soon, chronic, legacy, plafond+due) | M29 **elevates customer-level forecast rules** using the same thresholds where applicable — does not duplicate M27 top-10 company table |
| `PenerimaanPelunasanSalesDal` aggregates pelunasan by **salesman-day** only — no customer grain | M29 requires a **new customer pelunasan summary DAL** (aggregated SQL) for payment recency and pace |
| Purchase decline and payment lag are documented gaps in M17 analysis | M29 introduces **deterministic MoM omzet comparison** and **average payment lag** — first portal customer-behavior metrics |
| No collection CRM, promise-to-pay, or visit-to-payment data in BTR | Forecast rules derive from **Faktur, Piutang, Pelunasan, Customer master** only — no CRM inputs |
| M26/M27/M28 established **Current Pace (calendar-day linear extrapolation)** as approved deterministic projection | M29 applies pace projection to **customer billing** for credit-limit forecast — same explainability standard |

### Product intent

At any point during the month, management should answer:

> **Which customers are likely to become collection risks in the near future, and what should management prepare before the problem occurs?**

**Explicit constraints (non-negotiable):**

- Read-only — no automatic credit holds, no Desktop write-back
- Deterministic — every forecast reproducible from business formulas
- Explainable — every customer row includes human-readable reason text and rule traceability
- No AI, ML, statistical libraries, probability scores, or regression models
- Traceable to Piutang (M14), Customer Analytics (M17), Collection (M20), and Cash Flow Forecast (M27) artifacts

---

## 2. Business Objectives

### 2.1 Current business pain

Management today reviews **current-state** dashboards:

| Dashboard | What it shows | What it cannot show |
| --------- | ------------- | ------------------- |
| Piutang | Who owes money and aging today | Who **will probably** become overdue next |
| Customer Analytics | Dormant (already 90+ days inactive), plafond breach (already breached) | Customers **approaching** inactivity or credit breach |
| Collection | Recovery pace and overdue exposure today | Per-customer **forward collection risk score** |
| Cash Flow Forecast | Company month-end cash projection | **Customer portfolio** risk trajectory |

Finance and sales leadership must mentally combine: open balances, due dates, purchase trends, payment history, and credit limits. This synthesis is inconsistent, not available in one surface, and does not scale across hundreds of customers.

### 2.2 Future decisions enabled

| Decision | Enabled by M29 |
| -------- | -------------- |
| Early collection call before due date | Payment delay forecast + due-soon exposure |
| Credit review before breach | Credit limit forecast (projected balance vs plafond) |
| Sales recovery visit before churn | Inactivity forecast (approaching dormant) |
| Account manager intervention on declining accounts | Purchase decline forecast |
| Management escalation before chronic overdue | Collection risk forecast combining multiple signals |
| Portfolio-level resource allocation | Customer health distribution and elevated-risk receivable total |

### 2.3 Management value

| User | Value |
| ---- | ----- |
| Owner / GM | Preventive portfolio view — act before receivables deteriorate |
| Finance administration | Prioritized early collection queue with explainable rules |
| Collection management | Customer-level forward risk beyond company cash forecast |
| Sales management | Identify at-risk revenue relationships before churn |
| Credit control | Anticipate plafond breaches from billing pace |

### 2.4 Expected outcomes

1. **Reduced surprise overdue** — customers flagged while balances are still current or mildly overdue.
2. **Earlier credit intervention** — projected breach before open balance exceeds plafond.
3. **Revenue retention** — declining and approaching-dormant customers surfaced for sales action.
4. **Unified customer risk surface** — one dashboard replacing manual cross-dashboard synthesis.
5. **Foundation for M30+** — Collection Optimization, Customer Portfolio Optimization, Executive Business Health.

### 2.5 Explicitly out of scope (V1)

- AI / ML / probability scoring / statistical prediction
- Per-salesman or per-wilayah forecast dashboards (customer grain only; salesman/wilayah as context columns)
- Custom forecast horizon or date range (fixed **30-day horizon**)
- DSO (Days Sales Outstanding) as headline KPI — deferred per M20; M29 uses simpler **average payment lag** where needed
- Collection CRM (promise-to-pay, follow-up logs, visit outcomes)
- Alert Center / Executive Dashboard integration
- Pelunasan Report or Customer Report in portal
- Historical snapshot retention for trend-of-trends (single `CURRENT` snapshot only)
- Retur analytics per customer
- Field activity / visit-to-payment conversion

---

## 3. Terminology Mapping

Business terminology must map to BTR Desktop and portal implementation terms. **Do not introduce parallel vocabulary.**

| Business term | BTR / Portal term | Source |
| ------------- | ----------------- | ------ |
| Customer | `CustomerModel`, `CustomerCode`, `CustomerName`, `CustomerId` | `BTR_Customer` |
| Sales Invoice | **Faktur** | `BTR_Faktur`, `FakturView` |
| Receivable / Outstanding | **Piutang**, `KurangBayar`, `Sisa` | `BTR_Piutang` |
| Due date | **Jatuh Tempo**, `JatuhTempo`, `DueDate` | `BTR_Piutang` |
| Payment / Collection | **Pelunasan**, **Lunas** | `BTR_PiutangLunas` |
| Cash payment | `BayarTunai`, `JenisLunas = 0` | FF2 / pelunasan DALs |
| Giro / check payment | `BayarGiro`, `JenisLunas = 1` | FF2 |
| Total settlement | `TotalBayar` = Cash + Giro | FF2 |
| Adjustment | Retur + Potongan + MateraiAdmin | `BTR_PiutangElement` |
| Credit limit | **Plafond** | `CustomerModel.Plafond` |
| Overdue | Non-`Current` aging bucket | `PiutangAgingBucketResolver` |
| Chronic overdue | `DaysOver90` bucket | M14/M20 |
| Dormant customer | No Faktur this month + `LastFakturDate ≤ today − 90` | M17 `DormantDaysThreshold = 90` |
| Active customer | Invoiced in current calendar month | M17 |
| Legacy debt | Dormant customer with open balance | M20 `SignalLegacyDebt` |
| Invoiced revenue / Omzet | `FakturView.GrandTotal` | Sales aggregators |
| Recovery vs billing | `MonthCollections ÷ MonthFakturOmzet × 100` | M20 |
| Territory | **Wilayah**, `WilayahId`, `WilayahName` | Customer / Faktur |
| Salesman | **Sales Person**, `SalesPersonId`, `SalesPersonName` | Faktur / Piutang attribution |
| Collector | **Not a separate BTR entity** — collection attributed to invoicing salesman | M20 analysis |
| Aging bucket | `Current`, `Days1To30`, `Days31To60`, `Days61To90`, `DaysOver90` | `PiutangAgingBucketResolver` |
| Attention signal | `SignalKey` on attention row | M17/M20 |
| Customer key (most domains) | `CustomerCode` first, fallback `CustomerName` | `DashboardCustomerKeyResolver` |
| Customer key (piutang aging table) | `CustomerId` | M14 V2 |
| Payment history detail | Desktop **FF4 Pelunasan Info** | `IPelunasanInfoDal` — row-level, not aggregated in portal |

**M29 canonical customer key:** `DashboardCustomerKeyResolver.ResolveCodeFirst(CustomerCode, CustomerName)` — consistent with M17/M20. Piutang rows without resolvable code/name are excluded from customer-level forecast rows but remain in company totals.

---

## 4. Existing Capability Analysis

### 4.1 Piutang Dashboard (M14 V2)

**Source:** `DashboardPiutangAggregator` → `BTRPD_PiutangKpi`, `BTRPD_PiutangAging`, `BTRPD_PiutangCustomerAging`, `BTRPD_PiutangTopCustomerRisk`

| Capability | Rule | M29 reuse |
| ---------- | ---- | --------- |
| Open balance filter | `KurangBayar > 1` | Same |
| Aging buckets | `PiutangAgingBucketResolver` | **Authoritative** — do not duplicate |
| Per-customer aging accumulation | By `CustomerId` in snapshot | Recompute in-memory from same DTOs in Customer worker |
| Top 20 customer risk | Rank by total open balance | **Current exposure** context — not forecast ranking |
| Overdue customer count | Distinct customers with non-Current exposure | Traceability KPI |
| Top 10/20 concentration | Cumulative share of largest debtors | Portfolio concentration context |

**M29 does not duplicate Piutang Dashboard.** Piutang answers *current exposure*; M29 answers *forward customer risk*.

### 4.2 Customer Analytics Dashboard (M17)

**Source:** `DashboardCustomerAggregator` → `BTRPD_CustomerKpi`, `BTRPD_CustomerAttention`, `BTRPD_CustomerTopOmzet`, `BTRPD_CustomerTopPiutang`

| Capability | Rule | M29 reuse |
| ---------- | ---- | --------- |
| Active customer | Invoiced in current month | Purchase activity baseline |
| Dormant | 90-day rule via `ICustomerLastFakturDal` | **Extend** with 60-day approaching-dormant lead |
| Plafond breach | `Plafond > 0 AND openBalance > Plafond` | **Extend** with projected breach |
| Attention signals | Overdue, Dormant, PlafondBreach, SuspendedWithSales | **Backward-looking** — M29 adds **forecast signals** |
| Top 10 Omzet / Piutang | Current month / all-time open | Context rankings — not duplicated |
| Segmentation | Klasifikasi, Wilayah, Activity | Portfolio breakdown chart |

**M17 remains authoritative for current-state customer attention.** M29 adds forecast layer without modifying M17 API.

### 4.3 Collection Dashboard (M20)

**Source:** `DashboardCollectionAggregator` → `BTRPD_CollectionKpi`, `BTRPD_CollectionAttention`

| Capability | Rule | M29 reuse |
| ---------- | ---- | --------- |
| Overdue exposure | Sum non-Current balances | Denominator for concentration rules |
| Recovery vs Billing % | `MonthCollections ÷ MonthFakturOmzet × 100` | Company context KPI |
| Legacy debt | Dormant + open balance | Forecast signal input |
| Chronic overdue | Any `DaysOver90` exposure | Forecast signal input |
| Plafond breach + overdue | M20 `SignalPlafondBreachOverdue` | Strong collection risk input |
| Wilayah hotspot | Overdue share ≥ 15% | Context for customer rows in hotspot wilayah |
| Low recovery salesman | Rep collections < rep omzet | Indirect customer signal via salesman attribution |
| Attention priority suppression | Chronic > PlafondBreachOverdue > LegacyDebt > Overdue | M29 recommendation priority mirrors this ordering |

### 4.4 Cash Flow Forecast Dashboard (M27)

**Source:** `DashboardCashFlowForecastAggregator`, `CashFlowCollectionRiskBuilder` → `BTRPD_CashFlowForecast*`, `BTRPD_CashFlowCollectionRisk`

| Capability | Rule | M29 reuse |
| ---------- | ---- | --------- |
| Company cash pace projection | Linear extrapolation on `BayarTunai` | Pattern reference for customer billing pace |
| Collection risk rules | LargeDueSoon (7d), PlafondBreachDueSoon (14d), ChronicOverdueLarge, LegacyDebtOverdue, etc. | **Threshold reuse** at customer grain |
| Pelunasan 30-day lookback | Salesman-attributed cash | Pattern for customer pelunasan summary DAL |
| Forecast confidence | Low ≤5d, Medium 6–20d, High ≥21d elapsed | Reuse for portfolio forecast confidence |

**M27 remains company-level liquidity forecast.** M29 is customer portfolio risk — complementary, not replacement.

### 4.5 Sales Dashboard (M26 context)

| Capability | Rule | M29 reuse |
| ---------- | ---- | --------- |
| Month Faktur Omzet | `SUM(GrandTotal)` current month | Customer billing pace denominator |
| Current Pace pattern | `SalesForecastPolicy` | Customer projected billing = `(MTD omzet ÷ DE) × horizon days` |

### 4.6 Salesman Performance (M18) — read-only context

| Signal | M29 use |
| ------ | ------- |
| High overdue exposure salesman | Context column on customer risk row |
| Dormant customer portfolio | Cross-check approaching-dormant customers |
| Customer concentration by rep | Not V1 driver |

### 4.7 Desktop-only sources (not V1 portal aggregates)

| Desktop form | Data | M29 treatment |
| ------------ | ---- | ------------- |
| FF4 Pelunasan Info | Payment row detail | Pattern for new aggregated customer pelunasan DAL |
| FT5 Piutang Tracker | Per-Faktur lifecycle timeline | Deferred — too granular for V1 snapshot |
| FF1 Piutang Sales Wilayah | Retur offset per customer | Deferred |
| RF1 Retur Jual | Return volume | Out of scope V1 |

### 4.8 Reusable calculations — do not duplicate

| Calculation | Reuse from |
| ----------- | ---------- |
| Aging bucket assignment | `PiutangAgingBucketResolver` |
| Open balance filter | `KurangBayar > 1` |
| Dormant detection (90-day) | `DashboardCustomerAggregator.DormantDaysThreshold` |
| Plafond breach (current) | M17 plafond rule |
| Customer key resolution | `DashboardCustomerKeyResolver` |
| Last Faktur date | `ICustomerLastFakturDal` |
| Collection risk thresholds | M27 `CashFlowCollectionRiskBuilder` constants (7d, 14d, 15%, 10%) |
| Current month period | `CurrentMonthPeriode(today)` |
| Business date | `IBusinessDateProvider.Today` |
| Achievement / forecast confidence bands | M26/M27 policy patterns |

### 4.9 Business rule ownership

| Rule domain | Authoritative owner | M29 relationship |
| ----------- | ------------------- | ---------------- |
| Aging buckets | M14 `PiutangAgingBucketResolver` | Consumer |
| Dormant (90-day) | M17 Customer aggregator | Extends with 60-day lead |
| Plafond breach (current) | M17 Customer aggregator | Extends with projected breach |
| Collection attention signals | M20 Collection aggregator | Input signals to forecast score |
| Company collection risk rules | M27 `CashFlowCollectionRiskBuilder` | Threshold reference |
| Customer forecast rules | **M29 (new)** | `CustomerRiskForecastPolicy` |

---

## 5. Customer Risk Opportunities

All forecasts use a **30-day horizon** (`H = 30` calendar days from business date) unless noted.

### 5.1 Payment Delay Forecast

**Business question:** Which customers are likely to pay late in the near future?

| Signal | Deterministic rule | Inputs |
| ------ | ------------------ | ------ |
| **Likely Late Payer** | Customer has open balance AND (`AvgPaymentLagDays ≥ 7` OR `DaysSinceLastPayment ≥ 45`) AND has balance due within H | Payment behavior summary, open balances |
| **Escalating Overdue Trajectory** | Currently in `Days1To30` or `Days31To60` AND `AvgPaymentLagDays ≥ 14` | Aging + payment lag |
| **No Recent Payment** | Open balance > 0 AND no pelunasan in last 30 days AND total open ≥ configurable floor | Pelunasan recency |
| **Due Soon — Slow Payer** | Balance due within 14 days AND `AvgPaymentLagDays ≥ 7` | Reuses M27 14-day due-soon window |

**AvgPaymentLagDays definition (V1):**

For each customer, over settled Fakturs in the last **90-day lookback**:

```
AvgPaymentLagDays = AVG(LunasDate − JatuhTempo) in calendar days
```

Only include Fakturs where `Sisa ≤ 1` (fully settled) and at least one `BTR_PiutangLunas` row exists. Minimum 2 settled Fakturs required; otherwise treat as **unknown lag** (use recency rules only).

**Plain language:** *"This customer historically pays an average of X days after due date."*

### 5.2 Credit Limit Forecast

**Business question:** Which customers are likely to exceed their credit limit?

| Signal | Deterministic rule | Inputs |
| ------ | ------------------ | ------ |
| **Projected Plafond Breach** | `Plafond > 0` AND `ProjectedOpenBalance > Plafond` | Open balance, billing pace, plafond |
| **Approaching Plafond (Watch)** | `Plafond > 0` AND `ProjectedOpenBalance ≥ Plafond × 0.90` AND not yet breached | Same |
| **Already Breached — Worsening** | Current plafond breach AND `MTD omzet > 0` | M17 breach + active billing |

**ProjectedOpenBalance formula:**

```
ProjectedBilling = (MTD Omzet ÷ DaysElapsed) × HorizonDays   // capped: only add remaining days in horizon
ProjectedOpenBalance = CurrentOpenBalance + MAX(0, ProjectedBilling − MTD CollectionsForCustomer)
```

When customer-level MTD collections unavailable from existing DAL, use simplified V1:

```
ProjectedOpenBalance = CurrentOpenBalance + ProjectedBilling
```

This is intentionally conservative (assumes new billing adds to exposure). Document in UI as *"indicative upper bound if billing continues at current pace."*

**Plain language:** *"At current billing pace, this customer may exceed plafond within 30 days."*

### 5.3 Customer Inactivity Forecast

**Business question:** Which customers are likely to become dormant?

| Signal | Deterministic rule | Inputs |
| ------ | ------------------ | ------ |
| **Approaching Dormant** | Not active this month AND `DaysSinceLastFaktur` in [60, 89] | `ICustomerLastFakturDal` |
| **Imminent Dormant** | `DaysSinceLastFaktur` in [80, 89] | Same — higher severity |
| **Legacy Debt + Inactivity** | Approaching dormant AND open balance > 1 | M20 legacy debt pattern forward |

Reuse M17 dormant definition at 90 days for **already dormant** classification — M29 forecasts the **approach** to that threshold.

**Plain language:** *"This customer has not purchased for X days and may become dormant in Y days."*

### 5.4 Purchase Decline Forecast

**Business question:** Which customers show declining purchasing volume?

| Signal | Deterministic rule | Inputs |
| ------ | ------------------ | ------ |
| **Moderate Decline** | Prior full calendar month omzet > 0 AND current MTD omzet pace projects to `< 70%` of prior month total | Prior month + current month Faktur |
| **Severe Decline** | Same but `< 50%` | Same |
| **Stopped After History** | Prior month omzet > 0 AND current MTD omzet = 0 AND not yet dormant (< 90 days) | Faktur history |

**Projected month omzet (customer):**

```
ProjectedMonthOmzet = (MTD Omzet ÷ DaysElapsed) × DaysInMonth
DeclineRatio = ProjectedMonthOmzet ÷ PriorMonthOmzet
```

Minimum prior month omzet floor: configurable default **Rp 1,000,000** to avoid noise on tiny accounts.

**Plain language:** *"This customer's billing pace suggests a X% decline vs last month."*

### 5.5 Collection Risk Forecast

**Business question:** Which customers are likely to require collection attention?

Composite of receivable-forward signals (not a probability score):

| Signal | Rule | Source alignment |
| ------ | ---- | ---------------- |
| **High Collection Risk** | ≥ 2 strong signals OR 1 strong + 2 moderate | See §6 scoring |
| **Due Exposure Concentration** | Customer due within H represents ≥ 15% of company due-within-H total | M27 concentration threshold |
| **Chronic Trajectory** | Any `DaysOver90` balance AND no payment in 30 days | M20 chronic + M27 ExpectedOverdueGrowth pattern |
| **Legacy + Overdue Forward** | Approaching dormant + any overdue balance | M20 legacy debt |
| **Low Recovery Customer** | Customer's invoicing salesman in M20 LowRecoveryVsBilling set AND customer has overdue | M27 LowRecoveryCustomer pattern |

### 5.6 Customer Health (Portfolio)

**Business question:** What is overall customer portfolio health?

| Metric | Definition |
| ------ | ---------- |
| **Portfolio Health Score** | `100 − MIN(100, (ElevatedRiskReceivable ÷ TotalPiutang × 50) + (HighRiskCustomerCount ÷ ActiveCustomerBase × 50))` when denominators > 0 |
| **Elevated Risk Receivable** | Sum open balance for customers in **High Risk** or **Critical** categories |
| **Risk Distribution** | Count customers per risk category |
| **Risk Trend (V1 static)** | Distribution snapshot only — historical trend deferred |

Health score is a **deterministic composite index**, not ML. Every component traceable to rule outputs.

---

## 6. Risk Categories

Categories derived from **count and severity of forecast rule hits** — not probability.

### 6.1 Category definitions

| Category | Label | Entry conditions (evaluated in order) |
| -------- | ----- | --------------------------------------- |
| **Healthy** | Healthy | No forecast rule triggered; current open balance = 0 OR (all balances Current AND active purchasing AND no decline signal) |
| **Watch** | Watch | Exactly 1 **moderate** rule OR 2 **weak** rules; no strong rules |
| **Attention** | Attention | Exactly 1 **strong** rule OR ≥ 2 moderate rules; not yet High Risk |
| **High Risk** | High Risk | ≥ 2 strong rules OR (1 strong + ≥ 2 moderate) OR chronic overdue + any forward signal |
| **Critical** | Critical | ≥ 3 strong rules OR (chronic overdue + plafond breach/projection + decline/severe inactivity) OR legacy debt + chronic overdue |

### 6.2 Rule severity classification

| Severity | Forecast rules |
| -------- | -------------- |
| **Strong** | Projected Plafond Breach; Chronic Trajectory; Severe Decline; Imminent Dormant (80–89d); Escalating Overdue Trajectory |
| **Moderate** | Likely Late Payer; Approaching Plafond (90%); Moderate Decline; Approaching Dormant (60–79d); Due Soon — Slow Payer |
| **Weak** | No Recent Payment (below top concentration); Stopped After History; Due Exposure Concentration (below High threshold) |

### 6.3 Category display bands

| Category | Color token | Management interpretation |
| -------- | ----------- | ------------------------- |
| Healthy | `severity-success` | Normal monitoring |
| Watch | `severity-info` | Increase monitoring; no immediate escalation |
| Attention | `severity-warning` | Schedule review within horizon |
| High Risk | `severity-critical` | Early collection or sales intervention required |
| Critical | `severity-critical` + emphasis | Management review; potential credit suspension discussion |

### 6.4 Priority score (for ranking within category)

Integer sort key — **not probability**:

```
RiskPriorityScore = CategoryWeight + ExposureComponent + SignalCountComponent + DueUrgencyComponent
```

| Component | Formula |
| --------- | ------- |
| CategoryWeight | Critical=1000, High Risk=800, Attention=600, Watch=400, Healthy=0 |
| ExposureComponent | `MIN(300, FLOOR(OpenBalance / 1_000_000) × 5)` |
| SignalCountComponent | Strong×50 + Moderate×25 + Weak×10 |
| DueUrgencyComponent | 200 if any balance due ≤ 7d; 100 if due ≤ 14d; 0 otherwise |

**Tie-break:** Open balance descending → customer name ascending.

---

## 7. Dashboard Wireframe

**Route:** `/dashboard/customer-risk-forecast`  
**Layout:** Forecast-first (mirrors Cash Flow Forecast and Inventory Forecast pages)

```text
┌─────────────────────────────────────────────────────────────────────────────┐
│ Customer Risk Forecast Dashboard                    [Refresh] [Generated at]  │
├─────────────────────────────────────────────────────────────────────────────┤
│ EXECUTIVE SUMMARY (plain language, server-composed)                         │
│ "5 customers are expected to reach High Risk within 30 days. 3 show severe  │
│  purchase decline. Elevated-risk receivables ≈ Rp X. Top priority: ABC Co." │
├─────────────────────────────────────────────────────────────────────────────┤
│ KPI ROW 1 — Portfolio                                                       │
│ [Customers Forecasted at Risk] [Elevated Risk Receivable] [Portfolio Health]│
│ [Forecast Confidence]                                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│ KPI ROW 2 — Signal mix                                                      │
│ [Payment Delay] [Credit Limit] [Inactivity] [Purchase Decline] [Collection]│
├─────────────────────────────────────────────────────────────────────────────┤
│ CHARTS ROW                                                                  │
│ ┌──────────────────────────┐  ┌──────────────────────────┐                │
│ │ Risk Distribution (donut)│  │ Risk by Wilayah (bar)    │                │
│ │ Healthy/Watch/Attn/Hi/Cr │  │ Top 10 wilayah by count  │                │
│ └──────────────────────────┘  └──────────────────────────┘                │
│ ┌──────────────────────────┐  ┌──────────────────────────┐                │
│ │ Forecast Signal Trend    │  │ Elevated Risk Receivable │                │
│ │ (stacked bar by category)│  │ vs Total Piutang (bar)   │                │
│ └──────────────────────────┘  └──────────────────────────┘                │
├─────────────────────────────────────────────────────────────────────────────┤
│ TOP RISK CUSTOMERS (table, top 20 by RiskPriorityScore)                     │
│ Customer | Category | Score | Open Balance | Top Signal | Reason | Action │
├─────────────────────────────────────────────────────────────────────────────┤
│ ATTENTION LIST — Forecast signals (entity × signal, top 25)               │
│ Customer | Signal | Severity | Amount | Horizon | Explanation | Report    │
├─────────────────────────────────────────────────────────────────────────────┤
│ RECOMMENDED ACTIONS (grouped cards, top 15)                                 │
│ [Call Customer] [Schedule Visit] [Review Credit] [Increase Monitoring] ...  │
├─────────────────────────────────────────────────────────────────────────────┤
│ TRACEABILITY FOOTER                                                         │
│ Links: Customer Analytics | Piutang | Collection | Cash Flow Forecast       │
│ Reports: Piutang Report | Sales Report                                      │
│ Disclaimer: Indicative forecast — not credit decision automation            │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 7.1 Navigation placement

Sidebar: after **Customer Analytics**, before **Salesman Performance**.

Investigation path:

```text
Customer Risk Forecast → Customer Analytics (current state) → Piutang Report (evidence) → BTR Desktop
```

---

## 8. Recommendation Types

Explainable, rule-triggered recommendations — **one primary recommendation per customer** (highest-priority type wins).

| RecommendationKey | Label | Trigger | Owner |
| ----------------- | ----- | ------- | ----- |
| `CallCustomer` | Call Customer | Due within 14d + Likely Late Payer OR moderate collection risk | Collection |
| `ScheduleVisit` | Schedule Visit | Approaching Dormant + prior month omzet ≥ floor OR Severe Decline | Sales |
| `ReviewCredit` | Review Credit | Projected Plafond Breach OR Approaching Plafond (90%) | Finance / Credit |
| `IncreaseMonitoring` | Increase Monitoring | Watch category with 2+ moderate signals | Sales / Collection |
| `EarlyCollection` | Early Collection Planning | Due within 7d + open balance ≥ large due floor | Collection |
| `ManagementReview` | Management Review | Critical category OR chronic + plafond + decline | Management |
| `SalesRecovery` | Sales Recovery Campaign | Severe Decline OR Stopped After History (not dormant) | Sales |
| `SuspendCreditReview` | Suspend Credit Review | Projected breach + already has overdue OR SuspendedWithSales pattern | Finance |
| `LegacyDebtReview` | Legacy Debt Review | Legacy debt pattern (approaching dormant + balance) | Finance / Collection |
| `NoAction` | Continue Monitoring | Healthy or single weak signal only | — |

### 8.1 Recommendation priority (first match wins)

1. ManagementReview (Critical)
2. SuspendCreditReview
3. EarlyCollection
4. ReviewCredit
5. LegacyDebtReview
6. CallCustomer
7. ScheduleVisit
8. SalesRecovery
9. IncreaseMonitoring
10. NoAction

Each recommendation row includes: `RecommendationKey`, `RecommendationLabel`, `ReasonText`, `RuleId`, `ReportRoute`, `DrillDownRoute`.

---

## 9. Forecast Rules

### 9.1 Scope rules (CRF-XX)

| Rule ID | Rule |
| ------- | ---- |
| CRF-01 | Forecast horizon = 30 calendar days from business date |
| CRF-02 | Business date from `IBusinessDateProvider.Today` |
| CRF-03 | Open balance rows: `KurangBayar > 1` |
| CRF-04 | Customer grain: `DashboardCustomerKeyResolver` code-first |
| CRF-05 | No AI/ML/probability — integer scores and threshold rules only |
| CRF-06 | Read-only — no Desktop write-back |
| CRF-07 | UangMuka excluded from customer collections (FF2 parity) |
| CRF-08 | Minimum prior-month omzet floor for decline rules (default Rp 1M) |
| CRF-09 | Minimum settled Faktur count = 2 for AvgPaymentLagDays |
| CRF-10 | Max materialized rows: 20 top risk, 25 attention, 15 recommendations |

### 9.2 Payment delay rules (CRF-PXX)

| Rule ID | Rule |
| ------- | ---- |
| CRF-P01 | Likely Late Payer: `AvgPaymentLagDays ≥ 7` + open balance + due within H |
| CRF-P02 | Escalating Overdue: bucket ∈ {Days1To30, Days31To60} + `AvgPaymentLagDays ≥ 14` |
| CRF-P03 | No Recent Payment: no pelunasan 30d + open balance ≥ floor |
| CRF-P04 | Due Soon Slow Payer: due ≤ 14d + `AvgPaymentLagDays ≥ 7` |

### 9.3 Credit limit rules (CRF-CXX)

| Rule ID | Rule |
| ------- | ---- |
| CRF-C01 | Projected breach: `Plafond > 0` + `ProjectedOpenBalance > Plafond` |
| CRF-C02 | Approaching: `ProjectedOpenBalance ≥ 0.90 × Plafond` |
| CRF-C03 | Breached worsening: current breach + MTD omzet > 0 |

### 9.4 Inactivity rules (CRF-IXX)

| Rule ID | Rule |
| ------- | ---- |
| CRF-I01 | Approaching dormant: days since last Faktur ∈ [60, 79] |
| CRF-I02 | Imminent dormant: days since last Faktur ∈ [80, 89] |
| CRF-I03 | Legacy forward: CRF-I01 or CRF-I02 + open balance > 1 |

### 9.5 Purchase decline rules (CRF-DXX)

| Rule ID | Rule |
| ------- | ---- |
| CRF-D01 | Moderate decline: projected month omzet < 70% of prior month |
| CRF-D02 | Severe decline: projected month omzet < 50% of prior month |
| CRF-D03 | Stopped after history: prior month > 0, MTD = 0, days since last < 90 |

### 9.6 Collection risk rules (CRF-LXX)

| Rule ID | Rule |
| ------- | ---- |
| CRF-L01 | High collection risk: category ∈ {High Risk, Critical} |
| CRF-L02 | Due concentration: customer due-within-H ≥ 15% of company due-within-H |
| CRF-L03 | Chronic trajectory: DaysOver90 exposure + no payment 30d |
| CRF-L04 | Low recovery customer: salesman in M20 low recovery set + customer overdue |

### 9.7 Category resolution rules (CRF-GXX)

| Rule ID | Rule |
| ------- | ---- |
| CRF-G01 | Evaluate all signals → count by severity → map to §6.1 category table |
| CRF-G02 | Healthy requires zero strong/moderate forecast signals |
| CRF-G03 | Customers with zero historical Faktur excluded from forecast (no baseline) |

---

## 10. Executive Summary Generator

Server-composed plain-language block at top of dashboard.

### 10.1 Template

```
Customer Risk Forecast (as of {BusinessDate}, {HorizonDays}-day horizon):

• {HighRiskCount + CriticalCount} customers are forecast at elevated collection risk
• {PaymentDelayCount} customers show likely payment delay patterns
• {DeclineCount} customers show declining purchase activity ({SevereDeclineCount} severe)
• {InactivityCount} customers are approaching dormant status
• {CreditForecastCount} customers may exceed credit limit at current billing pace
• Elevated-risk receivables: approximately Rp {ElevatedRiskReceivable}

Highest priority: {TopCustomerName} — {TopRecommendationLabel} ({TopCategory})
Forecast confidence: {Confidence} (based on {DaysElapsed} days elapsed in month)
```

### 10.2 Confidence (portfolio-level)

Reuse M26/M27 pattern on **count of customers with MTD Faktur activity**:

| Confidence | Condition |
| ---------- | --------- |
| Low | Days elapsed ≤ 5 |
| Medium | Days elapsed 6–20 |
| High | Days elapsed ≥ 21 |

Early month: executive summary appends *"Early-month forecast — billing and decline signals may shift as month progresses."*

---

## 11. KPI Definitions

All monetary values IDR. Traceability IDs use prefix **CRF-KPI-**.

### 11.1 Portfolio KPIs

| KPI ID | Name | Formula | Meaning | Interpretation | Traceability | Owner |
| ------ | ---- | ------- | ------- | -------------- | ------------ | ----- |
| CRF-KPI-01 | Customers Forecasted at Risk | Count customers where category ∈ {Watch, Attention, High Risk, Critical} | Portfolio breadth requiring review | Higher = wider preventive workload | Rule output count | M29 |
| CRF-KPI-02 | High Risk Customer Count | Count category ∈ {High Risk, Critical} | Severe forward risk breadth | Immediate intervention queue size | Category filter | M29 |
| CRF-KPI-03 | Elevated Risk Receivable | `SUM(open balance)` for High Risk + Critical customers | Monetary exposure on forecast-risk accounts | Working capital at preventive risk | Piutang rows + category | M29 |
| CRF-KPI-04 | Elevated Risk Receivable % | CRF-KPI-03 ÷ Total Piutang × 100 | Share of debt on forecast-risk customers | Concentration of forward risk | CRF-KPI-03, M14 TotalPiutang | M29 |
| CRF-KPI-05 | Portfolio Health Score | §5.6 formula | Composite portfolio quality index | Higher = healthier forecast portfolio | Derived | M29 |
| CRF-KPI-06 | Forecast Confidence | M27 confidence pattern | Trust indicator for pace-based rules | Low early month | Days elapsed | M29 |
| CRF-KPI-07 | Total Piutang (context) | `SUM(KurangBayar)` all customers | Current exposure denominator | Must match M14 KPI | M14 traceability | M14 |

### 11.2 Signal mix KPIs

| KPI ID | Name | Formula | Meaning | Owner |
| ------ | ---- | ------- | ------- | ----- |
| CRF-KPI-10 | Payment Delay Signal Count | Customers with any CRF-P rule | Likely late payers | M29 |
| CRF-KPI-11 | Credit Limit Signal Count | Customers with any CRF-C rule | Plafond risk | M29 |
| CRF-KPI-12 | Inactivity Signal Count | Customers with any CRF-I rule | Approaching dormant | M29 |
| CRF-KPI-13 | Purchase Decline Signal Count | Customers with any CRF-D rule | Revenue attrition risk | M29 |
| CRF-KPI-14 | Collection Risk Signal Count | Customers with any CRF-L rule | Forward collection concern | M29 |

### 11.3 Distribution KPIs

| KPI ID | Name | Formula | Owner |
| ------ | ---- | ------- | ----- |
| CRF-KPI-20 | Healthy Count | Category = Healthy | M29 |
| CRF-KPI-21 | Watch Count | Category = Watch | M29 |
| CRF-KPI-22 | Attention Count | Category = Attention | M29 |
| CRF-KPI-23 | High Risk Count | Category = High Risk | M29 |
| CRF-KPI-24 | Critical Count | Category = Critical | M29 |

### 11.4 Traceability rules

| Rule ID | Rule |
| ------- | ---- |
| CRF-KPI-50 | CRF-KPI-07 must equal `BTRPD_PiutangKpi.TotalPiutang` same refresh cycle (within Customer worker piutang load) |
| CRF-KPI-51 | Customer count with open balance must reconcile with M14 `TotalCustomer` when keyed consistently |
| CRF-KPI-52 | Prior month omzet sum across customers ≤ Sales Dashboard prior month total (when computed for full prior month) |

---

## 12. Future Extensibility

Design hooks for later milestones — **not V1 implementation**.

| Future capability | Extension point | Depends on |
| ----------------- | --------------- | ---------- |
| **M30 Collection Optimization** | Recommendation engine consumes M29 customer risk rows as input queue | M29 action table |
| **Customer Lifetime Value** | Add LTV tier as `StrategicBoost` on priority score | Margin data (not in BTR today) |
| **Customer Profitability** | Weight risk by margin contribution | Cost/margin master |
| **Customer Segmentation** | Segment-specific thresholds in `CustomerRiskForecastPolicy` | M17 segmentation |
| **Sales Opportunity Forecast** | Inverse of decline forecast — growth signals | Historical snapshots |
| **Credit Recommendation** | Auto-suggest plafond adjustment amount | Credit policy master |
| **Aging Deterioration Trend** | Store category counts per refresh in history table | Historical snapshots |
| **DSO per customer** | Replace avg payment lag with rolling DSO | Pelunasan + Faktur history |
| **Executive Business Health** | Promote CRF-KPI-03 and CRF-KPI-05 to Executive Dashboard | M16 Phase 2 |
| **Alert Center integration** | Register forecast signals in `AlertCenterRegistry` | Product decision |
| **Executive AI Narrative** | Optional future narrative over deterministic summary | External — not V1 |

### 12.1 Recommended roadmap sequence

```text
M29  Customer Risk Forecast          ← this milestone
M30  Collection Optimization          ← consumes M29 risk queue
M31  Customer Portfolio Optimization  ← combines M29 + M17 + M18
M32  Executive Business Health        ← cross-domain forecast composite
```

---

## 13. Product Owner Decision Checklist

Before implementation, confirm:

| # | Decision | Recommendation |
| - | -------- | -------------- |
| 1 | Forecast horizon | 30 calendar days |
| 2 | Simplified projected plafond formula (conservative) | Approve V1 upper-bound approach |
| 3 | Approaching dormant lead at 60 days | Approve |
| 4 | Decline thresholds 70% / 50% | Approve |
| 5 | Avg payment lag lookback 90 days, min 2 Fakturs | Approve |
| 6 | Risk category five-band model | Approve |
| 7 | Extend Customer snapshot worker (not new domain) | Approve |
| 8 | No Alert Center / Executive in V1 | Approve deferral |

---

## Document Maintenance

When M29 is implemented:

1. Add Customer Risk Forecast to `docs/features/btr-portal/btr-portal-domain.md` Section 8 and 12.8
2. Create `docs/features/customer-risk-forecast/feature.md` permanent knowledge
3. Remove or archive this analysis after knowledge extraction

**Success criterion:** An Architect can design implementation from this document without additional business clarification.
