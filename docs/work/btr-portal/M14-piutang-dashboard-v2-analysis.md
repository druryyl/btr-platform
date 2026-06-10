# BTR Portal Analysis — M14 Piutang Dashboard V2

**Status:** Analysis complete — Product Owner decisions recorded (Section 11). Ready for Architect.  
**Scope:** Business analysis only. No implementation plans, API design, database design, or code.  
**Date:** 2026-06-10 (analysis) · Product Owner decisions recorded 2026-06-10  
**Role:** Analyst (`docs/agents/analyst-agent.md`)

**Business goal:** Transform the Piutang Dashboard from a receivable snapshot into a **management analytics dashboard** focused on receivable quality, risk exposure, customer concentration, and portfolio trends.

**Explicitly out of scope:** Collection performance and collection execution (handled by M20 Collection Dashboard).

**Reference documents:** `docs/foundation/PRODUCT.md`, `docs/foundation/DOMAIN.md`, `docs/foundation/LANDSCAPE.md`, `docs/foundation/WORKFLOW.md`, `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md`, `docs/features/materialized-dashboard/materialized-dashboard-architecture.md`, `docs/work/btr-portal/M17-Customer-Analytics-Analysis.md`, `docs/work/btr-portal/M20-collection-dashboard-analysist.md`, `btr-reporting-investigation.md`, `docs/archive/btr-portal-api-scaffolding/portal-analysis-m13-m15-final.md`

---

## 1. Executive Summary

BTR maintains **authoritative open-receivable data at Faktur (piutang header) grain** in `BTR_Piutang`, with outstanding balance persisted as `Sisa` (exposed in reports as `KurangBayar`). Desktop finance reporting centers on **FF1 Piutang Sales Wilayah**, which lists open Faktur with `JatuhTempo` (due date) but performs **no aging bucket assignment** in the UI or DAL layer.

The BTR Portal already delivers a **materialized Piutang Dashboard (M14 V1)** at `/dashboard/piutang`: three KPIs (Total Piutang, Total Customer, Overdue Customer), a five-bucket aging pie chart, and Top 10 customers by **total outstanding balance**. Data is pre-computed every **15 minutes** by `RefreshDashboardPiutangSnapshotWorker` into `BTRPD_Piutang*` tables via `DashboardPiutangAggregator`. This addresses the original M14 goal (aging distribution + top customer) but **does not yet fulfill the broader V2 management analytics vision** described in this analysis.

**All open questions resolved.** See Section 11 for authoritative Product Owner decisions.

### Approved product outcome (M14 V2)

Extend `/dashboard/piutang` into a **portfolio quality and exposure** dashboard. Collection execution, Top Overdue rankings, and recovery KPIs remain on M20. Salesman concentration remains on M18. Principal concentration is **excluded**.

**In scope for V2:**

| Area | Approved capability |
| ---- | ------------------- |
| **KPI row** | Total Piutang · Total Customer · Overdue Customer · **Overdue Piutang** (amount) · **Piutang > 90 Hari** (amount + % of Total Piutang) — all **all-time open** |
| **Aging** | Single five-bucket view (`JatuhTempo` only; inclusive boundaries unchanged) |
| **Customer risk** | **Top 20** outstanding customers with per-customer aging breakdown (Customer · Total · Current · 1–30 · 31–60 · 61–90 · >90) — no salesman/wilayah columns |
| **Concentration** | **Top 10 Customer %** and **Top 20 Customer %** of Total Piutang |
| **Customer aging snapshot** | **Approved** — foundation for customer risk table, concentration, and future cross-dashboard reuse (Executive, Customer, Alert Center) |

**Deferred from V2 (future milestone):**

| Area | PO decision |
| ---- | ----------- |
| **Monthly trends** | Deferred — does not block V2. When built: **snapshot retention** (not transactional replay), **12 months**, metrics: Total Piutang · Overdue Piutang · Piutang > 90 Hari |

**Critical implementation dependency:** Customer aging snapshot (Section 7, Section 11) — **do not** compute Top N customer aging or concentration from all open piutang rows at request time.

---

## 2. Dashboard Section Analysis

Each subsection follows the required analysis structure: Desktop Screens, DALs, DTOs, Builders, Policies, Business Rules, Calculations, Reuse Opportunities, Risks, Open Questions. **Open Questions in subsections are superseded by Section 11 (Product Owner decisions).**

---

### 2.1 Section 1 — KPI Summary

**Target KPIs:** Total Piutang · Total Customer · Overdue Customer · Piutang > 90 Hari

#### Existing Desktop Screens

| Menu | Screen | Path | Relevance |
| ---- | ------ | ---- | --------- |
| FF1 | `PiutangSalesWilayahForm` | `FinanceContext/PiutangSalesWilayahRpt/` | Primary open-receivable grid — `KurangBayar`, `JatuhTempo`, customer, salesman, wilayah; footer sums via Syncfusion summary row |
| FT5 | `PiutangTrackerForm` | `FinanceContext/TagihanAgg/` | Per-Faktur lifecycle timeline — not KPI aggregation |
| FF2 | `PenerimaanPelunasanSalesForm` | `FinanceContext/PenerimaanPelunasanSalesRpt/` | Collections received — **out of Piutang V2 scope** |
| FF4 | `PelunasanInfoForm` | `FinanceContext/` | Payment detail listing — out of scope |

FF1 supports **"Show Paid" toggle** (`KurangBayar > 1` when unchecked) — same open-balance threshold as portal analytics.

#### Existing DALs

| DAL | Interface | Data scope | Portal usage |
| --- | --------- | ---------- | -------------- |
| `PiutangSalesWilayahDal` | `IPiutangSalesWilayahDal` | Period-filtered or `ListAllOpenBalances()` (`Sisa > 1`) | M10 Piutang Report (live); FF1 Desktop |
| `PiutangOpenBalanceDal` | `IPiutangOpenBalanceDal` | All open balances — lightweight join (`BTR_Piutang`, `BTR_Faktur`, `BTR_Customer`) | Piutang snapshot worker source |
| `PiutangOpenBalanceWithSalesmanDal` | `IPiutangOpenBalanceWithSalesmanDal` | Open balances + invoicing salesman | M18 Salesman, M20 Collection workers |
| `PiutangOpenBalanceWithWilayahDal` | `IPiutangOpenBalanceWithWilayahDal` | Open balances + customer wilayah | M20 Collection worker |
| `DashboardPiutangSnapshotDal` | `IDashboardPiutangSnapshotDal` | Reads `BTRPD_PiutangKpi`, `BTRPD_PiutangAging`, `BTRPD_PiutangTopCustomer` | Piutang Dashboard API |

#### Existing DTOs

| DTO | Fields relevant to KPIs |
| --- | ----------------------- |
| `PiutangSalesWilayahDto` | `KurangBayar`, `JatuhTempo`, `CustomerCode`, `CustomerName`, `SalesName`, `WilayahName`, `FakturDate` |
| `PiutangOpenBalanceDto` | `KurangBayar`, `JatuhTempo`, `CustomerCode`, `CustomerName` |
| `DashboardPiutangAggregateResult` | `TotalPiutang`, `TotalCustomer`, `OverdueCustomer`, `AgingBuckets`, `TopCustomers`, `GeneratedAt` |
| `DashboardPiutangResponse` | Portal API shape — same KPI fields as aggregate result |

#### Existing Builders

| Component | Role |
| --------- | ---- |
| `DashboardPiutangAggregator` | **Authoritative portal KPI + company aging + Top 10** computation |
| `PiutangReportDal.BuildResponse` | Footer `TotalPiutang`, `TotalCustomer` for M10 report traceability |
| `PiutangBuilder.ReCalc()` | **Transactional** — recalculates `BTR_Piutang.Sisa` on write; not used at dashboard read time |

No dedicated `PiutangKpiBuilder` in Desktop — KPI logic is inline in portal aggregator and report DAL.

#### Existing Policies

No formal policy classes for piutang KPIs. Rules are embedded in aggregator/report code and documented in `docs/features/btr-portal/btr-portal-domain.md`.

#### Existing Business Rules

| Rule | Definition | Where enforced |
| ---- | ---------- | -------------- |
| Open balance threshold | `KurangBayar > 1` (equivalent to `BTR_Piutang.Sisa > 1`) | `DashboardPiutangAggregator`, `PiutangReportDal`, FF1 filter |
| Total Piutang | `SUM(KurangBayar)` over open rows | Aggregator, M10 footer |
| Total Customer | Distinct customer key count | Aggregator, M10 footer |
| Customer key | `CustomerCode` when non-empty, else `CustomerName` | `DashboardPiutangAggregator.ResolveCustomerKey` |
| Overdue Customer | Distinct customers with any balance in non-`Current` aging bucket | Aggregator |
| Piutang > 90 Hari | `SUM(KurangBayar)` where `DaysOverdue > 90` (bucket `DaysOver90`) | **Computed in aggregator** as bucket amount; **also** `DashboardCustomerAggregator.AgingOver90Amount`; **Executive** `AgingOver90Amount` — **not on Piutang Dashboard KPI row today** |
| Dashboard data scope | All open balances (no period filter on snapshot source) | `PiutangOpenBalanceDal` — differs from FF1 default period filter on `PiutangDate` |
| Reference date | `ITglJamDal.Now.Date` for aging | Snapshot worker at refresh time |

#### Existing Calculations

```
DaysOverdue = Today − JatuhTempo.Date   (calendar days)
Total Piutang = Σ KurangBayar where KurangBayar > 1
Overdue Customer = COUNT DISTINCT customer keys where ANY row has DaysOverdue > 0
Piutang > 90 Hari = Σ KurangBayar where DaysOverdue > 90
```

**Traceability (documented):** Total Piutang and Total Customer on Piutang Dashboard = M10 Piutang Report footer (same open-balance filter and customer key).

#### Reuse Opportunities

| Asset | Reuse for V2 KPI row |
| ----- | -------------------- |
| `BTRPD_PiutangKpi` | Extend with `AgingOver90Amount` (or read from `BTRPD_PiutangAging` where `BucketKey = 'DaysOver90'`) |
| `DashboardPiutangAggregator` | Already computes >90 bucket — KPI is a **projection**, not new business logic |
| `DashboardExecutiveComposer` | Already surfaces `AgingOver90Amount` / `AgingOver90Percent` for executive card |
| M10 `PiutangReportDal` | KPI reconciliation / investigation drill-down |

#### Risks

| Risk | Notes |
| ---- | ----- |
| FF1 vs snapshot scope mismatch | FF1 defaults to user-selected `PiutangDate` period; dashboard uses **all open balances** — users comparing FF1 grid to dashboard may see different totals if period ≠ all-time open |
| `Sisa` persistence vs recalculation | `btr-reporting-investigation.md` notes `PiutangBuilder` paths may differ; KPIs trust persisted `Sisa` |
| Stale KPIs | 15-minute refresh — `GeneratedAt` must remain visible for management trust |
| Duplicate >90 definitions | Same bucket key used across Piutang, Customer, Collection, Executive — drift risk if constants diverge |

#### Open Questions

| # | Question |
| - | -------- |
| Q1 | Should **Piutang > 90 Hari** display as **amount only**, **amount + % of Total Piutang**, or both? (Executive uses both.) |
| Q2 | Should KPI row distinguish **Overdue Exposure** (sum of non-Current buckets) from **Overdue Customer** (count)? Collection dashboard exposes the amount; Piutang dashboard does not today. |
| Q3 | Is all-time open balance the correct management scope for all four KPIs, or should any KPI be period-scoped? |

---

### 2.2 Section 2 — Aging Analysis

**Target:** Understand aging reports, calculations, bucket definitions, and reusable assets.

#### Existing Desktop Screens

| Screen | Aging capability |
| ------ | ---------------- |
| FF1 `PiutangSalesWilayahForm` | Displays `JatuhTempo` column only — **no bucket columns, no overdue-days calculation in form code** |
| FT5 `PiutangTrackerForm` | Event dates (piutang created, tagihan, pelunasan) — not due-date aging |
| FF2 / FF4 | Payment-focused — N/A for outstanding aging |

**Desktop conclusion:** No named "Aging Report" or "Umur Piutang" screen exists. Users infer aging manually from `JatuhTempo`.

#### Existing DALs

Same open-balance DALs as Section 1. Aging assignment happens **above** the DAL in application aggregators, not in SQL.

#### Existing DTOs

| DTO | Aging fields |
| --- | ------------ |
| `PiutangSalesWilayahDto` | `JatuhTempo` (raw due date) |
| `DashboardPiutangAgingBucket` | `BucketKey`, `BucketLabel`, `Amount`, `SortOrder` |
| `DashboardCollectionAgingRiskRow` | Four overdue-only buckets (excludes Current) — M20 |

#### Existing Builders

| Builder / Aggregator | Aging output |
| -------------------- | ------------ |
| `DashboardPiutangAggregator` | Five company buckets — **includes Current** |
| `DashboardCollectionAggregator` | Four **overdue-only** buckets for Aging Risk Summary |
| `DashboardCustomerAggregator` | Uses bucket logic for overdue signals and `AgingOver90Amount` — no per-customer bucket persistence |
| `DashboardSalesmanAggregator` | Overdue exposure via same bucket boundaries |

No Desktop builder for aging.

#### Existing Policies

**Single authoritative bucket standard** for piutang due-date aging in Portal (documented in `btr-portal-domain.md`):

| Bucket key | Label | Rule (`DaysOverdue = Today − JatuhTempo.Date`) |
| ---------- | ----- | ------------------------------------------------ |
| `Current` | Current (Not Yet Due) | `DaysOverdue ≤ 0` |
| `Days1To30` | 1–30 Days | `1 ≤ DaysOverdue ≤ 30` |
| `Days31To60` | 31–60 Days | `31 ≤ DaysOverdue ≤ 60` |
| `Days61To90` | 61–90 Days | `61 ≤ DaysOverdue ≤ 90` |
| `DaysOver90` | > 90 Days | `DaysOverdue > 90` |

**Boundary style:** Inclusive ranges; each open Faktur row assigns to **exactly one** bucket. Amount = `SUM(KurangBayar)` per bucket. **Reconciliation:** Σ buckets = Total Piutang.

**Aging anchor:** `JatuhTempo` / `BTR_Piutang.DueDate` only — **not** `FakturDate` or `PiutangDate`.

**Multiple standards in BTR?**

| Context | Standard | Same as piutang? |
| ------- | -------- | ---------------- |
| Piutang / Customer / Salesman / Collection (due-date) | Five-bucket due-date aging above | Yes — duplicated `ResolveAgingBucketKey` in multiple aggregators |
| Collection Aging Risk Summary | Four buckets — **excludes Current** | Subset view of same assignment |
| Inventory Risk Dashboard | Days since last sale (`BrgLastFaktur`) | **Different domain** — not receivable aging |
| Desktop FF1 | No buckets | N/A |

#### Existing Business Rules

- Open rows only: `KurangBayar > 1`
- Bucket assignment at **Faktur (piutang header) grain** — partial payments do not split a single Faktur across buckets
- `FakturDate` available in `PiutangSalesWilayahDto` but **not used** for aging in any discovered analytics

#### Existing Calculations

Persisted in `BTRPD_PiutangAging` (five rows per refresh, `SnapshotKey = 'CURRENT'`).

Example interpretation (illustrative):

| Bucket | Meaning |
| ------ | ------- |
| Current | Not yet due |
| 1–30 | 1 to 30 calendar days past due |
| 31–60 | 31 to 60 days past due |
| 61–90 | 61 to 90 days past due |
| > 90 | More than 90 days past due |

#### Reuse Opportunities

| Asset | Reuse |
| ----- | ----- |
| `BTRPD_PiutangAging` | Company aging chart — **already on Piutang Dashboard** |
| `DashboardPiutangAggregator.AgingBucketDefinitions` | Canonical labels and sort order |
| `AgingPieChart.vue` | Portal chart component |
| M10 Piutang Report | Row-level `JatuhTempo` for manual verification / investigation |

#### Risks

| Risk | Notes |
| ---- | ----- |
| Logic duplication | `ResolveAgingBucketKey` copied in Piutang, Customer, Collection, Salesman aggregators — inconsistent boundary risk |
| No Desktop aging precedent | Product cannot point to legacy FF1 bucket definitions — portal standard **is** the de facto business standard |
| Faktur-level grain | A Faktur with partial payment still ages as a whole — may not match finance mental model for "weighted" aging |

#### Open Questions

| # | Question |
| - | -------- |
| Q4 | Should Piutang V2 show **both** full five-bucket distribution (quality) and overdue-only four-bucket view (risk) like M20 — or keep single chart? |
| Q5 | Should aging support alternate anchor (`FakturDate`, `PiutangDate`) for management views? No Desktop precedent exists. |
| Q6 | Are inclusive boundaries (day 30 in 1–30, day 90 in 61–90) confirmed for external reporting / auditor alignment? |

---

### 2.3 Section 3 — Customer Risk Analysis

**Target:** Top Outstanding Customer · Top Overdue Customer · per-customer aging breakdown

#### Existing Desktop Screens

| Screen | Customer ranking |
| ------ | ---------------- |
| FF1 | Grid groupable by `CustomerName` via Syncfusion — **no built-in Top N ranking or aging breakdown** |
| Excel export (FF1) | Flat list sorted Sales → Wilayah → Customer — manual analysis possible |

#### Existing DALs

| DAL | Customer aggregation support |
| --- | --------------------------- |
| `PiutangOpenBalanceDal` | Customer + `JatuhTempo` + `KurangBayar` — sufficient for per-customer aging at refresh |
| `PiutangSalesWilayahDal` | Adds salesman, wilayah, payment decomposition columns |
| `DashboardPiutangSnapshotDal` | Top 10 **total outstanding** in `BTRPD_PiutangTopCustomer` |
| `DashboardCustomerSnapshotDal` | Top 10 piutang + `PercentOfTotal` in `BTRPD_CustomerTopPiutang` |
| `DashboardCollectionSnapshotDal` | Top 10 **overdue balance** in `BTRPD_CollectionTopOverdueCustomer` |

#### Existing DTOs

| DTO | Purpose |
| --- | ------- |
| `DashboardPiutangTopCustomer` | Rank, CustomerName, CustomerCode, OutstandingBalance |
| `DashboardCustomerRankingRow` | Top piutang with `PercentOfTotal` (M17) |
| `DashboardCollectionTopOverdueCustomerRow` | Rank, CustomerCode, CustomerName, OverdueBalance, PercentOfTotal |

**Per-customer aging breakdown** (Customer · Total · Current · 1–30 · 31–60 · 61–90 · 90+): **No persisted DTO or table discovered.** Logic is derivable by grouping `PiutangOpenBalanceDto` rows by customer and applying `ResolveAgingBucketKey` — same pattern as company aging.

#### Existing Builders

| Aggregator | Customer risk outputs |
| ---------- | --------------------- |
| `DashboardPiutangAggregator` | Top 10 by **total** outstanding |
| `DashboardCustomerAggregator` | Top 10 piutang, overdue balance per customer (for attention list), `AgingOver90Amount` company-wide |
| `DashboardCollectionAggregator` | Top 10 by **overdue** balance; chronic overdue signals |

#### Existing Policies

| Ranking type | Metric | Dashboard home |
| ------------ | ------ | -------------- |
| Top Outstanding | `SUM(KurangBayar)` all buckets | Piutang (M14), Customer (M17) |
| Top Overdue | `SUM(KurangBayar)` where bucket ≠ Current | Collection (M20) only |

M20 Product decision: Collection rankings use **overdue balance**, not total balance — intentional differentiation from Piutang Top 10.

#### Existing Business Rules

- Customer key: `CustomerCode` preferred, `CustomerName` fallback
- Top N = **10** across Piutang, Customer, Collection snapshots (constant `TopRankingCount` / hardcoded Take(10))
- Sort: descending balance, then name ascending (Piutang aggregator)

#### Existing Calculations

**Top Outstanding Customer (portal today):**

```
GROUP BY customer key
OutstandingBalance = SUM(KurangBayar)
ORDER BY OutstandingBalance DESC
TAKE 10
```

**Top Overdue Customer (M20 — not Piutang dashboard):**

```
For each open row where bucket ≠ Current:
  ADD KurangBayar to customer overdue total
ORDER BY overdue total DESC
TAKE 10
```

**Per-customer aging breakdown (not persisted):**

```
For each customer key:
  For each bucket B: Amount_B = SUM(KurangBayar) of rows assigned to B
  Total Piutang = SUM(Amount_B) across all five buckets
```

#### Reuse Opportunities

| Asset | Reuse |
| ----- | ----- |
| `BTRPD_PiutangTopCustomer` | Top Outstanding on Piutang Dashboard — **exists** |
| `BTRPD_CustomerTopPiutang` | Top Outstanding with `PercentOfTotal` — Customer Dashboard |
| `BTRPD_CollectionTopOverdueCustomer` | Top Overdue — **Collection scope**; reference for metric definition, not duplicate on Piutang V2 without PO decision |
| `DashboardCustomerAggregator.BuildOverdueBalanceByKey` | Pattern for customer overdue totals |
| M10 Piutang Report | Customer pre-filter drill-down (investigation pattern exists on portal) |

#### Risks

| Risk | Notes |
| ---- | ----- |
| Metric confusion | "Top Customer" on Piutang = total balance; "Top Overdue" on Collection = past-due only — V2 must label clearly |
| Scale | Per-customer aging for all customers with open balance is **O(rows)** — likely requires snapshot, not request-time |
| M17 overlap | Customer Analytics already shows Top 10 Piutang and attention signals — Piutang V2 needs distinct **portfolio** framing |
| Missing CustomerCode in `BTRPD_PiutangTopCustomer` | Snapshot table stores `CustomerName` only; API enriches `CustomerCode` from aggregator memory at write time inconsistently — verify data for investigation links |

#### Open Questions

| # | Question |
| - | -------- |
| Q7 | Should Piutang V2 include **Top Overdue Customers** (overdue balance), or defer entirely to Collection Dashboard? |
| Q8 | Is **per-customer aging breakdown** required for **all** customers with balance, or Top N only? |
| Q9 | Should customer risk table include salesman / wilayah attribution (available in `PiutangSalesWilayahDal` but not in lightweight open-balance DAL)? |

---

### 2.4 Section 4 — Concentration Analysis

**Target:** Top 10 / Top 20 customer concentration · receivable concentration % · dimensions: Customer, Salesman, Wilayah, Principal

#### Existing Desktop Screens

No Desktop screen computes receivable concentration percentage or Top-N cumulative share. FF1 allows manual grouping; no concentration KPI.

#### Existing DALs

| Dimension | DAL | Aggregation discovered |
| --------- | --- | ---------------------- |
| **Customer** | `PiutangOpenBalanceDal` | Customer-level `SUM(KurangBayar)` |
| **Customer** | `PiutangSalesWilayahDal` | Same + salesman/wilayah context |
| **Salesman** | `PiutangOpenBalanceWithSalesmanDal` | Invoicing salesman from `BTR_Faktur.SalesPersonId` |
| **Wilayah** | `PiutangOpenBalanceWithWilayahDal` | `BTR_Customer.WilayahId` |
| **Principal** | — | **No DAL** — piutang header has no supplier/principal; would require Faktur line allocation |

#### Existing DTOs / Snapshot tables

| Table / DTO | Concentration metric |
| ----------- | -------------------- |
| `BTRPD_PiutangTopCustomer` | Top 10 balances — **no `PercentOfTotal` column** |
| `BTRPD_CustomerTopPiutang` | Top 10 + `PercentOfTotal` per row |
| `BTRPD_SalesmanTopPiutang` | Top 10 salesman + `PercentOfTotal` |
| `BTRPD_CollectionTopOverdueCustomer` | Top overdue + `PercentOfTotal` of **total overdue** |
| Executive `DashboardExecutivePiutangAttention` | `TopCustomerPercent`, `AgingOver90Percent` — Top **1** customer share |
| M17 KPI `TopPiutangCustomerPercent` | Top **1** customer ÷ Total Piutang |
| M18 KPI `TopPiutangSalesmanPercent` | Top **1** salesman ÷ Total Piutang |

**Top 20 concentration:** Not implemented in any snapshot table discovered.

**Cumulative Top 10 / Top 20 share** (sum of Top N balances ÷ Total Piutang): Not pre-computed; derivable from ranking rows.

#### Existing Builders

| Aggregator | Concentration logic |
| ---------- | ------------------- |
| `DashboardCustomerAggregator` | `TopPiutangCustomerPercent` = rank-1 ÷ total |
| `DashboardSalesmanAggregator` | `TopPiutangSalesmanPercent`, per-rep `TopCustomerPercent` (omzet concentration, not piutang) |
| `DashboardCollectionAggregator` | `OverdueConcentrationPercent` = top-1 customer overdue ÷ total overdue |
| `DashboardExecutiveComposer` | Promotes piutang concentration to executive attention |

#### Existing Policies

- Concentration percentages in Customer/Salesman snapshots use **Top-1** or **per-row PercentOfTotal** in ranking tables
- Collection `PercentOfTotal` on overdue rankings is relative to **overdue exposure**, not Total Piutang

#### Dimension analysis

| Dimension | Existing reports / snapshots | Management usefulness | Data quality |
| --------- | --------------------------- | --------------------- | ------------ |
| **Customer** | Piutang Top 10, Customer Top 10, Executive Top 1 % | **High** — default risk concentration | Mature |
| **Salesman** | M18 `BTRPD_SalesmanTopPiutang` | **High** — which rep's book carries debt | Attribution = invoicing salesman on Faktur |
| **Wilayah** | FF1 row-level; M20 Top Overdue Wilayah | **Medium** — regional exposure | Wilayah from customer master; blank → "Unknown" in Collection aggregator |
| **Principal** | None for piutang | **Uncertain** — would show which supplier's goods drive receivable risk | **Not available** without allocating Faktur `GrandTotal` to line-item suppliers — no existing business rule |

#### Reuse Opportunities

| Asset | Reuse |
| ----- | ----- |
| `PercentOfTotal` pattern from `BTRPD_CustomerTopPiutang` | Extend Piutang Top Customer snapshot or compute cumulative Top 10/20 % |
| `DashboardExecutiveComposer` Top 1 % pattern | Expand to Top 10/20 cumulative KPIs |
| `PiutangOpenBalanceWithSalesmanDal` / `WilayahDal` | Wilayah/salesman concentration at refresh time |
| M18 Salesman Top Piutang | Cross-link from Piutang V2 — avoid duplicate ranking unless PO wants single home |

#### Risks

| Risk | Notes |
| ---- | ----- |
| Principal allocation | No authoritative rule for splitting piutang across principals on multi-supplier Faktur |
| Double-counting across dimensions | Customer concentration + salesman concentration describe different lenses — may confuse if shown without context |
| Top 20 vs Top 10 | Doubling ranking rows increases snapshot size marginally but needs Product approval for layout |
| Wilayah vs salesman | Collection ownership follows invoicing salesman; wilayah is customer attribute — different business meaning |

#### Open Questions

| # | Question |
| - | -------- |
| Q10 | Should Piutang V2 own **Top 10 and Top 20 cumulative concentration %**, or only surface Top 1 (Executive/M17 pattern)? |
| Q11 | Is **salesman piutang concentration** in scope for Piutang Dashboard, or remain on Salesman Performance (M18)? |
| Q12 | Is **Principal concentration** a V2 requirement despite no existing data path — or explicitly excluded? |
| Q13 | Should concentration use **total piutang** or **overdue-only** denominator? (Collection uses overdue for its concentration KPI.) |

---

### 2.5 Section 5 — Receivable Trend Analysis

**Target:** Monthly Total Piutang · Monthly Overdue · Monthly >90 Day trends

#### Existing Desktop Screens

**None discovered** for piutang portfolio trends. FF1 is point-in-time with period filter on creation/due date — not monthly snapshot history.

#### Existing DALs

| DAL | Historical capability |
| --- | --------------------- |
| `PiutangOpenBalanceDal` | Current open balances only |
| `PiutangSalesWilayahDal` | Can filter by `PiutangDate` or `DueDate` range — **not** used for trend series |
| `IPenerimaanPelunasanSalesDal` | Payment history by `LunasDate` — cash flow, not outstanding trend |
| `IFakturViewDal` | Sales omzet trends — different metric family |

#### Existing DTOs / Snapshot tables

| Asset | Trend support |
| ----- | ------------- |
| `BTRPD_PiutangKpi.GeneratedAt` | Point-in-time freshness only |
| `BTRPD_RefreshLog` | Operational refresh history — **not** business KPI history |
| `BTR_SalesOmzet` | Salesperson omzet trends (Desktop RO2) — **not piutang** |
| Sales dashboard `BTRPD_SalesWeekTrend` | Weekly **sales** pattern — analogous pattern does not exist for piutang |

**Materialized dashboard policy:** `CURRENT` snapshot only — **no historical retention** (`materialized-dashboard-domain.md`).

#### Existing Builders

No piutang trend builder. `DashboardSalesFakturAggregator` builds week trend for sales — closest architectural analogy.

#### Existing Policies

M20 Product decision: **Aging deterioration trend excluded** — no historical aging snapshot.

#### Existing Calculations

**Not available** for:

- Month-end Total Piutang time series
- Month-end Overdue exposure time series  
- Month-end >90d amount time series

**Theoretical inputs** (not implemented as trends):

- Reconstructing past outstanding would require replaying `BTR_Piutang`, `BTR_PiutangLunas`, and `BTR_PiutangElement` history at each month-end — **no worker or report does this**
- Payment inflows (`BTR_PiutangLunas`) could support collection trend (M20 Recovery) but not outstanding balance trend without balance snapshots

#### Reuse Opportunities

| Asset | Reuse |
| ----- | ----- |
| Sales `WeekTrend` snapshot pattern | Architectural reference only — Product must approve new historical domain |
| `BTRPD_RefreshLog` | Ops monitoring — not business trend |
| Chart components (`Chart` / PrimeVue) | UI capability exists portal-side |

#### Risks

| Risk | Notes |
| ---- | ----- |
| No historical data | **Highest gap** for V2 "portfolio trends" goal — cannot be served from existing snapshots |
| Storage / retention policy | Month-end series requires new retention rule contradicting current `CURRENT`-only pattern |
| Definition drift | Past months computed with today's bucket rules vs. rules at time — audit question |
| Performance | Backfilling historical series from transactional tables is expensive and error-prone |

#### Open Questions

| # | Question |
| - | -------- |
| Q14 | Are monthly trends a **mandatory** V2 outcome, or can V2 ship without trends (deferred milestone)? |
| Q15 | If trends required: **month-end snapshot retention** vs. **recomputed from transactions**? |
| Q16 | Which trend metrics: Total Piutang only, or also overdue / >90d / customer count? |
| Q17 | How many months of history does management require (3, 6, 12, 24)? |

---

## 3. Existing Asset Inventory

### 3.1 Core transactional tables

| Table | Role in piutang analytics |
| ----- | ------------------------- |
| `BTR_Piutang` | Receivable header — `DueDate`, `Sisa`, `PiutangDate`, `CustomerId` |
| `BTR_Faktur` | Links piutang to customer, salesman, invoice date |
| `BTR_PiutangLunas` | Payments — collection analytics (M20), not outstanding trend |
| `BTR_PiutangElement` | Retur, potongan, materai adjustments |
| `BTR_Customer` | Customer identity, `WilayahId`, `Plafond`, `Klasifikasi` |
| `BTR_Wilayah` | Regional dimension |
| `BTR_SalesPerson` | Salesman dimension |

Index `IX_BTR_Piutang_OpenBalance` supports open-balance reads (`Sisa > 1`, includes `DueDate`).

### 3.2 Portal snapshot tables (Piutang domain)

| Table | Content |
| ----- | ------- |
| `BTRPD_PiutangKpi` | TotalPiutang, TotalCustomer, OverdueCustomer, GeneratedAt |
| `BTRPD_PiutangAging` | Five company aging buckets |
| `BTRPD_PiutangTopCustomer` | Top 10 outstanding (name, balance) |

### 3.3 Related portal snapshot tables (cross-dashboard)

| Table | Relevance to Piutang V2 |
| ----- | ---------------------- |
| `BTRPD_CustomerTopPiutang` | Top 10 + PercentOfTotal (M17) |
| `BTRPD_SalesmanTopPiutang` | Salesman concentration (M18) |
| `BTRPD_CollectionTopOverdueCustomer` | Top overdue customers (M20) |
| `BTRPD_CollectionTopOverdueSalesman` | Top overdue salesmen (M20) |
| `BTRPD_CollectionTopOverdueWilayah` | Top overdue wilayah (M20) |
| `BTRPD_CollectionAging` | Overdue-only four buckets (M20) |

### 3.4 Application components

| Layer | Components |
| ----- | ---------- |
| Workers | `RefreshDashboardPiutangSnapshotWorker`, `RefreshAllDashboardSnapshotsWorker` |
| Aggregators | `DashboardPiutangAggregator`, `DashboardCustomerAggregator`, `DashboardCollectionAggregator`, `DashboardSalesmanAggregator` |
| Read facades | `DashboardPiutangDal`, `PiutangReportDal` |
| Portal UI | `PiutangDashboardView.vue`, `AgingPieChart.vue`, `Top10RankingTable.vue` |
| API | `GET /api/dashboard/piutang` (`PiutangDashboardController`) |

### 3.5 Desktop reports (Finance)

| Code | Report | Portal equivalent |
| ---- | ------ | ----------------- |
| FF1 | Piutang Sales Wilayah | M10 Piutang Report (live query) |
| FF2 | Penerimaan Pelunasan Sales | None — M20 uses same DAL in snapshot |
| FF4 | Pelunasan Info | None |
| FT5 | Piutang Tracker | None — per-Faktur investigation |

---

## 4. Reuse Opportunities (Cross-cutting)

| Priority | Asset | V2 application |
| -------- | ----- | -------------- |
| **High** | `DashboardPiutangAggregator` + `BTRPD_Piutang*` | Foundation for company KPIs and aging — extend, don't replace |
| **High** | `PiutangOpenBalanceDal` | Lightweight source for customer-level snapshot generation |
| **High** | Documented aging bucket rules (`btr-portal-domain.md`) | Single product standard across dashboards |
| **Medium** | `BTRPD_CustomerTopPiutang.PercentOfTotal` | Model for concentration columns |
| **Medium** | `PiutangOpenBalanceWithSalesmanDal` / `WilayahDal` | Dimensional concentration without full FF1 join cost |
| **Medium** | `AgingPieChart.vue`, ranking table components | UI patterns for expanded sections |
| **Medium** | Investigation / drill-down to M10 Piutang Report | Already wired for Top Customers |
| **Low** | FF1 `PiutangSalesWilayahDal` full row | Payment decomposition (Retur, Giro) — quality signals, not core V2 |
| **Reference** | Sales week trend snapshot | Pattern for **future** piutang trend domain only |

---

## 5. Risks

| ID | Risk | Severity | Mitigation direction (Product / Architect — not designed here) |
| -- | ---- | -------- | -------------------------------------------------------------- |
| R1 | **No historical snapshots** — trends deferred from V2 | Medium | Future milestone: 12-month snapshot retention (Q14–Q17) |
| R2 | **Customer aging not persisted** — blocks V2 customer risk table | High | **Approved** customer aging snapshot (Q18, Q19; Section 7.1) |
| R3 | **Metric overlap with M17 / M20** — duplicate Top N tables confuse users | Medium | **Mitigated** — strict dashboard charter (Q20); Top Overdue on M20 only (Q7) |
| R4 | **Duplicated aging logic** across four aggregators | Medium | Shared definition ownership — customer snapshot should reuse same bucket function |
| R5 | **Principal dimension unavailable** | Low | **Excluded** from V2 (Q12) |
| R6 | **FF1 period vs all-open scope** | Medium | User education; consistent labeling on dashboard |
| R7 | **Trust in persisted `Sisa`** | Medium | Periodic reconciliation (investigation workflow exists) |
| R8 | **Snapshot staleness** (15 min) | Low | `GeneratedAt` display; optional manual refresh |
| R9 | **Scale growth** — open row count increases | Medium | Snapshot approach already adopted for company level; customer snapshot size = # customers with balance |

---

## 6. Open Questions — Resolved

All questions from the discovery analysis are resolved. Authoritative decisions are in **Section 11**.

---

## 7. Customer Aging Snapshot — Feasibility Analysis

**Product constraint:** There is currently **no customer aging snapshot** persisted in BTR. Analysis must not assume runtime calculation from all open piutang records on each dashboard request.

### 7.1 Approved snapshot structure (Product Owner — Q18, Q19)

**Decision:** Customer aging snapshot **approved**. Canonical key is **`CustomerId`**. Do **not** persist `OverdueAmount` — it is derivable and redundant.

| Field | Definition |
| ----- | ---------- |
| `CustomerId` | `BTR_Customer.CustomerId` — canonical snapshot key |
| `CurrentAmount` | Sum of open balance in `Current` bucket (`DaysOverdue ≤ 0`) |
| `Aging30Amount` | Sum in `Days1To30` bucket (non-cumulative) |
| `Aging60Amount` | Sum in `Days31To60` bucket (non-cumulative) |
| `Aging90Amount` | Sum in `Days61To90` bucket (non-cumulative) |
| `AgingOver90Amount` | Sum in `DaysOver90` bucket |
| `LastUpdate` | Snapshot refresh timestamp (same semantics as `GeneratedAt` on company KPI) |

**Derived fields (not stored):**

| Derived | Formula |
| ------- | ------- |
| Total Piutang (per customer) | `CurrentAmount + Aging30Amount + Aging60Amount + Aging90Amount + AgingOver90Amount` |
| Overdue Piutang (per customer) | `Aging30Amount + Aging60Amount + Aging90Amount + AgingOver90Amount` |

**Reconciliation invariants:**

- Per customer: sum of five bucket amounts = customer Total Piutang (open rows only, `KurangBayar > 1`).
- Company level: sum across all customer snapshot rows = `BTRPD_PiutangKpi.TotalPiutang` (within same refresh).

**PO rationale:** `OverdueAmount` duplicates the sum of the four overdue buckets. Storing `CurrentAmount` plus each bucket enables full aging breakdown and Total Piutang without redundant columns.

**Cross-dashboard reuse:** PO notes this snapshot is intended as a **shared foundation** for M14 V2, future Executive Dashboard enhancements, Customer Analytics, and Alert Center — not only the Piutang Dashboard customer risk table.

### 7.2 Benefits of snapshot-based aging vs runtime calculation

| Benefit | Explanation |
| ------- | ----------- |
| **Dashboard performance** | Company piutang already moved off live aggregation due to full open-balance scan cost; customer-level breakdown multiplies aggregation work |
| **Consistent numbers** | Single refresh produces aligned figures across Piutang, Customer, Collection, Executive |
| **Predictable load** | Refresh every 15–30 min vs. unbounded concurrent API aggregation |
| **Enables richer tables** | Full customer aging grid impractical at HTTP request time for large debtor bases |

### 7.3 Estimated impact on dashboard performance

| Approach | Read path | Write path |
| -------- | --------- | ---------- |
| **Current (company snapshot)** | O(1) SQL read from `BTRPD_*` | O(n) scan of open piutang rows every 15 min |
| **Runtime per request** | O(n) per API call — **rejected for company piutang already** | None |
| **Customer aging snapshot** | O(1) read per customer row or Top N | O(n) same scan + O(customers) grouping — **incremental cost at refresh, not per user** |

Row count reference: M10 analysis noted ~11K open rows on dev DB — customer snapshot would be ≤ distinct customers with open balance (likely hundreds to low thousands). Storage is modest; **refresh CPU** is the main cost — already paid once per Piutang refresh today.

### 7.4 Existing BTR worker patterns to reuse

| Pattern | Example |
| ------- | ------- |
| Domain-specific refresh worker | `RefreshDashboardPiutangSnapshotWorker` |
| Aggregator service | `DashboardPiutangAggregator` |
| Replace-current snapshot DAL | `DashboardPiutangSnapshotDal.ReplaceCurrent` |
| Transactional replace | Delete child rows + MERGE KPI in `TransHelper` scope |
| CLI + Task Scheduler | `btr.portal.worker --domain Piutang` |
| Refresh logging | `BTRPD_RefreshLog` |
| Shared open-balance source | `IPiutangOpenBalanceDal` |

Customer snapshot could be:

- **Extension of Piutang refresh** (same worker, additional tables), or
- **Dedicated worker** (e.g. `CustomerAging` domain) with aligned cadence

Collection and Customer workers already consume `IPiutangOpenBalanceDal` — a customer aging table could feed multiple consumers.

### 7.5 Recommended ownership location

| Context | Ownership |
| ------- | --------- |
| **Business domain** | Finance / Receivable reporting — piutang is a core finance entity per `DOMAIN.md` |
| **Technical context** | `ReportingContext` / `DashboardSnapshotAgg` — alongside existing `BTRPD_Piutang*` and `BTRPD_Customer*` |
| **Documentation** | `docs/features/btr-portal/btr-portal-domain.md` KPI catalog + `materialized-dashboard-domain.md` retention rules |

### 7.6 Risks and operational considerations

| Consideration | Detail |
| ------------- | ------ |
| **Field naming** | **Resolved (Q19)** — non-cumulative buckets; no `OverdueAmount` column |
| **Stale per-customer view** | Same 15-min staleness as company dashboard |
| **No history by default** | Customer `CURRENT` snapshot does not solve trend analysis — trends deferred (Q14) |
| **Cross-dashboard drift** | Customer, Collection, Piutang workers refresh at different cadences (15 vs 30 min) — temporary inconsistency |
| **Empty CustomerCode** | Fallback to name — same key collision risk as today |
| **Operational monitoring** | Extend refresh log / health endpoint for new domain |

### 7.7 Refresh frequency considerations

| Cadence | Tradeoff |
| ------- | -------- |
| **15 min** (current Piutang) | Aligns piutang company + customer aging; adequate for board/management |
| **30 min** (Customer/Collection) | Lower DB load; visible lag vs piutang |
| **Event-driven** (after pelunasan) | M20 explicitly deferred — ops complexity |

**Approved:** Bind customer aging snapshot refresh to **Piutang worker cadence (15 min)** for reconciliation consistency with company-level `BTRPD_Piutang*` snapshots.

---

## 8. Approved V2 Scope Summary

Mapping of Product Owner decisions to V2 deliverables. Implementation details belong to the Architect.

### 8.1 KPI row (approved)

| KPI | Definition | PO notes |
| --- | ---------- | -------- |
| Total Piutang | All-time open `SUM(KurangBayar)` where `> 1` | Unchanged from M14 |
| Total Customer | Distinct customers with open balance | Unchanged |
| Overdue Customer | Distinct customers with any non-Current bucket balance | Unchanged |
| **Overdue Piutang** | Sum of balances in buckets 1–30, 31–60, 61–90, >90 | **New** — key portfolio quality indicator |
| **Piutang > 90 Hari** | `DaysOver90` bucket amount **and** % of Total Piutang | **New** — amount shows scale; % shows severity |

### 8.2 Aging (approved)

- Single five-bucket pie chart (includes Current).
- Anchor: `JatuhTempo` only — invoice date is not aging.
- Inclusive boundaries unchanged (Q6).

### 8.3 Customer risk table (approved)

- **Top 20** outstanding customers (not all customers).
- Columns: Customer · Total Piutang · Current · 1–30 · 31–60 · 61–90 · >90.
- No salesman or wilayah columns.
- Top Overdue customers: **M20 Collection only** (Q7).
- Drill-down for full detail: M10 Piutang Report.

### 8.4 Concentration (approved)

| Metric | Definition |
| ------ | ---------- |
| Top 10 Customer % | Sum of top 10 customer Total Piutang ÷ company Total Piutang × 100 |
| Top 20 Customer % | Sum of top 20 customer Total Piutang ÷ company Total Piutang × 100 |

Denominator: **Total Piutang** (not overdue). Salesman concentration: **M18 only**. Principal: **excluded**.

### 8.5 Trends (deferred)

Not in V2 scope. Recorded architecture intent for future milestone:

| Aspect | Decision |
| ------ | -------- |
| Approach | Month-end **snapshot retention** — never replay historical transactions |
| Metrics | Total Piutang · Overdue Piutang · Piutang > 90 Hari |
| History | **12 months** |

---

## 9. Current State vs V2 Target — Gap Summary

| V2 section | Available today | V2 work (per PO decisions) |
| ---------- | --------------- | -------------------------- |
| **KPI Summary** | 3 KPIs + aging pie | Add Overdue Piutang, Piutang > 90 Hari (amount + %) |
| **Aging Analysis** | Company five-bucket snapshot | No change to chart; boundaries confirmed |
| **Customer Risk** | Top 10 outstanding (no aging columns) | Top 20 table with aging breakdown from **customer aging snapshot** |
| **Concentration** | Top 1 % elsewhere | Top 10 % and Top 20 % on Piutang Dashboard |
| **Trends** | None | **Deferred** — 12-month snapshot retention when scheduled |

---

## 10. Milestone Positioning

| Milestone | Charter (approved — Q20) |
| --------- | ------------------------ |
| **M14 Piutang** | Exposure & portfolio quality |
| **M17 Customer** | Customer insight |
| **M18 Salesman** | Salesman insight |
| **M20 Collection** | Recovery & collection effectiveness |

Strict separation prevents duplicate rankings with slightly different numbers across dashboards.

| Milestone | Question answered |
| --------- | ----------------- |
| M14 V1 (implemented) | How much is owed and how is it aged (company)? |
| **M14 V2** | What is receivable **quality**, **risk**, and **concentration**? |
| M17 Customer Analytics | Which customers require **management attention**? |
| M20 Collection Dashboard | Are receivables converting to **cash**? |
| M16 Executive | What requires **executive** attention? |

---

## 11. Product Owner Decisions (Authoritative)

Recorded 2026-06-10. These decisions supersede open questions in Sections 2.1–2.5.

| # | Topic | Decision |
| - | ----- | -------- |
| Q1 | Piutang > 90 Hari display | **Amount + %** of Total Piutang |
| Q2 | Overdue Exposure on Piutang Dashboard | **Yes** — add **Overdue Piutang** KPI |
| Q3 | KPI scope | **All-time open balance** (consistent with M14) |
| Q4 | Aging views | **Single five-bucket view** (portfolio quality; Collection owns overdue-only) |
| Q5 | Alternate aging anchor | **No** — `JatuhTempo` (`DueDate`) only |
| Q6 | Bucket boundaries | **Confirm current inclusive boundaries** — no change |
| Q7 | Top Overdue Customer | **Collection Dashboard only** |
| Q8 | Customer aging table scope | **Top N only** — **Top 20** outstanding with aging breakdown |
| Q9 | Salesman/wilayah on customer risk table | **No** |
| Q10 | Top 10 / Top 20 concentration % | **Yes** — Top 10 Customer % and Top 20 Customer % |
| Q11 | Salesman concentration on Piutang | **M18 only** |
| Q12 | Principal concentration | **Excluded** |
| Q13 | Concentration denominator | **Total Piutang** |
| Q14 | Monthly trends in V2 | **Deferred** — does not block V2 |
| Q15 | Trend architecture (future) | **Snapshot retention** — not transactional replay |
| Q16 | Trend metrics (future) | Total Piutang · Overdue Piutang · Piutang > 90 Hari |
| Q17 | Trend history (future) | **12 months** |
| Q18 | Customer aging snapshot | **Approved** — bind to Piutang refresh (15 min) |
| Q19 | Snapshot fields | `CustomerId`, `CurrentAmount`, `Aging30Amount`, `Aging60Amount`, `Aging90Amount`, `AgingOver90Amount`, `LastUpdate` — **no `OverdueAmount`** |
| Q20 | Dashboard charter | Keep dashboards **strictly separated** (see Section 10) |

### 11.1 Decision rationale (summary)

**Q1 — Amount + %:** A large >90 balance has different severity when Total Piutang is Rp 5B vs Rp 50B.

**Q2 — Overdue Piutang:** Three count/balance KPIs without overdue **amount** is incomplete for portfolio quality.

**Q4 — Single five-bucket view:** Piutang = portfolio quality; Collection = overdue management.

**Q5 — JatuhTempo only:** Aging by invoice date is invoice age, not receivable aging.

**Q7 — Collection only for Top Overdue:** Piutang focuses exposure; Collection focuses recovery.

**Q8 — Top 20 only:** Management does not need thousands of rows; M10 report for detail.

**Q10 — Top 10/20 %:** High-value owner-level concentration insight.

**Q12 — Principal excluded:** No clean allocation path exists — do not invent a business rule.

**Q14 — Trends deferred:** V2 has sufficient value without trends.

**Q18 + Q19 — Customer aging snapshot:** Most important architectural decision; enables customer risk table and concentration without repeated full open-balance scans; intended foundation for Executive, Customer, and Alert Center features.

---

*End of analysis. Product Owner decisions recorded. Ready for Architect. No implementation plans, architecture decisions, database changes, API designs, or UI implementations are included per analyst scope.*
