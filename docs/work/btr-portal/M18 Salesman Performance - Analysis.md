# BTR Portal Analysis — M18 Salesman Performance Dashboard

**Status:** Analysis complete — Product Owner decisions recorded (Section 12). Ready for Architect.  
**Scope:** Business analysis only. No implementation plans, API design, database design, or code.  
**Date:** 2026-06-08 (analysis) · Product Owner decisions recorded 2026-06-08  
**Context:** BTR Portal V2 (M16 Executive Dashboard, M17 Customer Analytics complete) follows a management philosophy: *What requires management attention?* M18 introduces a **Salesman Performance Dashboard** at `/dashboard/salesmen` to answer: *Which salesman requires management attention and why?*

**Approved roadmap position:** M17 Customer Analytics → **M18 Salesman Performance** → M19 Slow Moving & Dead Stock → M20 Collection Dashboard → … → **M25 Sales Force Effectiveness**

**Reference documents:** `docs/foundation/PRODUCT.md`, `docs/foundation/DOMAIN.md`, `docs/foundation/LANDSCAPE.md`, `docs/foundation/WORKFLOW.md`, `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-operational.md`, `docs/features/btr-portal/btr-portal-architecture.md`, `docs/features/btr-portal/knowledge-extraction-report-m16-m17.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md`, `docs/work/btr-portal/M16 Executive Dashboard - Analysis.md`, `docs/work/btr-portal/M17-Customer-Analytics-Analysis.md`, `btr-reporting-investigation.md`

---

## 1. Executive Summary

BTR Portal today exposes **salesman** primarily as a **ranking dimension** on the Sales Dashboard — Top 10 Salesman by invoiced omzet for the current month. Company-level target and achievement are computed, but **per-salesman target achievement, collection exposure, customer coverage, and exception signals are not surfaced**. Piutang analytics are customer-centric (M17) or company-centric (Piutang Dashboard); receivable risk **attributed to the invoicing salesman** exists in source data but is not aggregated for management.

M18 answers: *Which salesman requires management attention and why?*

**All open questions resolved.** See Section 12 for authoritative Product Owner decisions.

### Key findings

| Finding | Implication |
| ------- | ----------- |
| **Top 10 Salesman ranking exists** in Sales snapshot (`BTR_PortalDashboardSalesTopSalesman`) — omzet only, no target % | Ranking shows performers; **underperformers and attention signals are invisible** |
| **Per-salesman monthly targets exist** in `BTR_SalesOmzetTarget` and Desktop RO2 (`SalesOmzetChartForm`) | Per-rep achievement % is **computable today** — not exposed in portal |
| **Sales attribution on Faktur** (`BTR_Faktur.SalesPersonId`) is authoritative for invoiced sales and piutang linkage | Sales omzet, customer reach, and open balances can be grouped by salesman from existing DALs |
| **Customer master has no SalesPersonId** — ownership is transactional (Faktur) and operational (route) | **Approved:** Last Invoicing Salesman for dormant/coverage attribution; route reserved for M25 |
| **Piutang snapshot source** (`PiutangOpenBalanceDal`) omits salesman — but **Piutang Report / FF1** (`IPiutangSalesWilayahDal`) includes `SalesName` | Salesman piutang KPIs require reusing FF1 join pattern, not current snapshot DAL alone |
| **M17 Customer Analytics patterns** (attention cards, attention list, Top N rankings, segmentation) are directly adaptable to salesman lens | Reuse aggregator and UI patterns; swap customer key for salesman key |
| **Field activity, route compliance** exist in Desktop (RO3, SM4) | **Excluded from M18** — deferred to **M25 Sales Force Effectiveness Dashboard** |
| **Collection exposure** attributable via FF1; **collection performance metrics** (payments, DSO) | Exposure signals in M18; performance metrics deferred to **M20 Collection Dashboard** |
| **Executive dashboard** promotes company Achievement % only — no salesman dimension | M18 supplements domain dashboards; **does not modify** executive page or replace Sales Top 10 |

### Approved product outcome

Deliver **Salesman Performance** at `/dashboard/salesmen` using **Proposal A (Attention First)** layout. Materialize salesman KPIs in a **dedicated `BTR_PortalDashboardSalesman*` snapshot domain** (Sales + Piutang sources, Faktur-only, current month). Mandatory rankings: **Top 10 Omzet**, **Top 10 Achievement %**, **Top 10 Piutang** — each with `SalesPersonCode`. Include **Attention List** with Below Target, No Target, High Overdue Exposure, High Piutang Exposure, Customer Concentration, and Dormant Customer Portfolio signals. Combined cross-domain signals encouraged. Contextual drill-down: sales signals → Sales Report; piutang signals → Piutang Report (`?q=` salesman name). Navigation: Salesman Dashboard → Domain Dashboard → Report. No new Salesman Report. No executive dashboard changes. **Not** a sales activity dashboard — activity metrics reserved for M25.

---

## 2. Management Attention Discovery

This section identifies salesman-related situations that typically require management intervention, mapped to **existing BTR capabilities**. Items marked **Portal today** are calculable from current portal snapshots or reports. Items marked **Desktop only** exist in BTR Desktop but are not in the portal. Items marked **Not available** have no implemented logic discovered in codebase or documentation.

### 2.1 Sales performance and target

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Salesman consistently below monthly target** | Territory or rep not meeting plan; coaching or reassignment may be needed | `BTR_SalesOmzetTarget` per `SalesPersonId`; `SalesOmzetTargetResolver.GetTargetAmount`; Desktop RO2 target vs achievement when filtered to one salesman | **Partial** — logic exists; portal shows company total target only (`SumTargetAmountForMonth`) |
| **Salesman with zero or missing target** | Achievement % undefined — may indicate planning gap | `GetTargetAmount` returns null when no row | **Partial** — Desktop handles as Unknown; portal has no per-rep view |
| **Large gap between top and bottom performers** | Uneven team performance; resource or territory imbalance | Top 10 Salesman ranking; Desktop `BuildManagerComparison` (top 15 by recognized omzet from `BTR_SalesOmzet`) | **Partial** — top only in portal; bottom performers not ranked |
| **Weekly sales deceleration (per salesman)** | Billing pace slowing mid-month for a specific rep | `SalesOmzetChartWeekGrouper` + `FakturView` grouped by `SalesPersonName` | **Not available** — company weekly trend only in portal |
| **Month-over-month sales decline (per salesman)** | Sustained underperformance trend | `FakturView` or `BTR_SalesOmzet` across months per `SalesPersonName` | **Not available** — no prior-period comparison in portal |
| **High pipeline / outstanding orders not invoiced (per salesman)** | Orders booked but not yet billed — future risk or fulfillment backlog | `BTR_SalesOmzet` with `OmzetStatus = Outstanding`; Desktop RO2 pipeline omzet | **Desktop only** — portal Sales Dashboard uses Faktur-only (`PipelineOmzet = 0`) |
| **Sales omzet data quality risk** | RO2 aggregate out of sync with Faktur | `SalesOmzetHealthWeekly` (Good / Warning / Poor) | **Desktop only** |

### 2.2 Customer coverage and territory

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Shrinking active customer count (per salesman)** | Fewer distinct customers invoiced — coverage loss or territory erosion | `DashboardSalesFakturAggregator` computes `TotalCustomer` at company level; same distinct-count logic per `SalesPersonName` on `FakturView` | **Not available** — pattern exists; not computed per rep |
| **Many dormant customers on salesman's book** | Customers previously served by rep no longer purchasing | M17 dormant rule (90 days, prior history) applied per customer, attributed via last Faktur's `SalesPersonName` or route ownership | **Not available** — M17 customer logic exists; salesman rollup not computed |
| **Route customers not visited** | Planned coverage not executed | `BTR_SalesRuteItem` (customers on route) vs `BTR_CheckIn` (actual visits) | **Not computed** — tables exist; SM4-Rute master + RO1 check-in list |
| **Low effective call rate** | Field visits without orders — wasted effort | `EffectiveCallView` (`OrderCount > 0`); Desktop RO3 | **Desktop only** — `UserEmail` on check-in, not `SalesPersonId` |
| **Territory (Wilayah) underperformance** | Salesman's assigned `WilayahId` underperforms vs peers | `SalesPersonModel.WilayahId`; `FakturView.WilayahName` grouped by salesman or wilayah | **Partial** — dimensions on rows; no wilayah×salesman aggregate in portal |
| **Customer concentration within salesman portfolio** | Rep depends on few accounts — loss of one customer hurts rep disproportionately | `FakturView` grouped by `SalesPersonName` + `CustomerCode`, top customer % of rep omzet | **Not available** — same pattern as M17 Top Customer % |
| **Customers outside salesman's Wilayah** | Cross-territory selling — policy or data issue | Compare `Customer.WilayahId` vs `SalesPerson.WilayahId` on Faktur joins | **Not computed** |

### 2.3 Receivable and collection exposure

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Salesman responsible for large overdue balances** | Collection follow-up needed on rep's invoiced accounts | `PiutangSalesWilayahDto.SalesName`, `KurangBayar`, `JatuhTempo`; Piutang Report `Sales` column | **Partial** — row-level in report; no per-salesman aggregate KPI |
| **High piutang concentration for one salesman** | Single rep owns disproportionate company receivable risk | Sum `KurangBayar` grouped by `SalesName` from `IPiutangSalesWilayahDal` | **Not available** — data in FF1 DAL |
| **Severely aged receivables on salesman's invoices** | > 90 days overdue attributed to invoicing rep | `DashboardPiutangAggregator` aging bucket logic applied per salesman via FF1 rows | **Not available** — company aging only in portal |
| **Many overdue customers on salesman's book** | Count of distinct customers with past-due balance per `SalesName` | Same overdue rule as Piutang snapshot (`JatuhTempo < today`) grouped by salesman | **Not available** |
| **Legacy debt on inactive customers (per salesman)** | Open balance on customers no longer buying from that rep | Last Faktur date per customer + open balance + `SalesName` on originating Faktur | **Not computed** |
| **Collection payments lagging new billing (per salesman)** | Rep bills more than collections recover | `PenerimaanPelunasanSalesDal` (payments by `SalesName`) vs Faktur omzet by salesman | **Desktop only** — deferred collection effectiveness (M20) |
| **High retur offset on salesman's piutang** | Returns eroding receivable instead of cash | `PiutangSalesWilayahDto.Retur` per row | **Desktop only** (FF1) |

### 2.4 Returns and quality

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **High return volume attributed to salesman** | Product, pricing, or relationship issues on rep's accounts | `ReturJualViewDal` — `SalesName` on `BTR_ReturJual` | **Desktop only** (RF1) |
| **High return rate relative to sales (per salesman)** | Retur value vs Faktur omzet for same rep | `ReturJualView` + `FakturView` per `SalesPersonName` | **Not computed** |

### 2.5 Master data and workflow integrity

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Unsigned Faktur backlog per salesman** | Goods delivered but `Faktur Kembali` incomplete for rep's invoices | Sales Report `Status = Kembali`; `FakturView.Kembali` | **Partial** — row-level via Sales Report search on `Sales` column |
| **Suspended customers still invoiced by salesman** | Policy violation on rep's transactions | M17 `SuspendedWithSales` signal + `FakturView.SalesPersonName` | **Not computed** at salesman level |
| **Salesman without Wilayah assignment** | Territory accountability unclear | `BTR_SalesPerson.WilayahId` blank check | **Partial** — master data exists |
| **Salesman without route configured** | No planned customer coverage | `BTR_SalesRute` per `SalesPersonId` | **Partial** — SM4 master; no analytics |

### 2.6 Cross-domain salesman situations

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **High omzet but high overdue (same salesman)** | Rep sells aggressively but collections fail | Join per-salesman omzet + per-salesman open balance | **Not available** — requires cross-metric composition |
| **Low omzet but large piutang (same salesman)** | Legacy debt on shrinking book | Per-salesman omzet (month) + all-time open balance | **Not computed** |
| **Top performer by omzet with worst overdue exposure** | Recognition vs intervention conflict | Independent rankings composable from same sources | **Not available** |

### 2.7 Workflow-derived attention points

From `docs/foundation/WORKFLOW.md` and portal operational workflows:

| Workflow stage | When management cares about salesman | Portal support today |
| -------------- | ------------------------------------ | -------------------- |
| Sales Order → Faktur → Fulfillment | Rep performance vs target, customer reach | **Weak** — Top 10 omzet only |
| Faktur → Piutang | Receivable exposure by invoicing rep | **None** — customer/company only |
| Collection Visit → Payment | Visit effectiveness, route execution | **None** — Desktop FT1/RO3 |
| Customer Return | Return patterns by rep | **None** — Desktop RF1 |
| Route planning (SM4) | Coverage vs actual visits | **None** |

---

## 3. Existing Dashboard Reuse Analysis

### 3.1 Management Attention Center (`/dashboard`) — salesman metrics today

| Metric | Source | Salesman-centric? | Reuse for M18 |
| ------ | ------ | ----------------- | ------------- |
| Sales Achievement % (company) | Sales snapshot via `DashboardExecutiveComposer` | No — company-wide | Context only; not rep-level |
| Total Achievement / Total Target | Sales snapshot KPI | No | Company baseline for comparison |
| Top 10 Salesman | **Not on executive page** — lives on Sales Dashboard only | Yes — ranking | Optional cross-link; not attention-oriented alone |
| Overdue Customer / Piutang signals | Piutang snapshot | No — customer aggregate | Underlying FF1 data can attribute to `SalesName` |
| Customer Analytics attention signals | Customer snapshot (M17) | No — customer lens | **Pattern reuse** for salesman attention list |

**Assessment:** Executive dashboard has **no salesman dimension**. M18 must add a dedicated salesman lens; repeating Sales Dashboard Top 10 alone does not satisfy the management question.

### 3.2 Sales Dashboard (`/dashboard/sales`)

| KPI / section | Salesman relevance | Hidden salesman data | Reuse rationale |
| ------------- | ------------------ | -------------------- | --------------- |
| Total Target | Partial — sum of all `BTR_SalesOmzetTarget` rows for month | Per-rep targets in same table via `GetTargetAmount` | Denominator for per-rep achievement |
| Total Achievement / Achievement % | Company only | Per-rep omzet groupable from same `FakturView` rows used in refresh | `SalesOmzetChartAchievementPolicy.ComputePercent` reusable per rep |
| Weekly Invoiced Sales Trend | Company only | Per-rep weekly buckets derivable via `SalesOmzetChartWeekGrouper` | Same grouper as `DashboardSalesFakturAggregator.BuildWeekTrend` |
| **Top 10 Salesman** | **Yes** — primary existing salesman KPI | `Rank`, `SalesPersonName`, `CompletedOmzet` only — **no `SalesPersonId`, no target, no %** | Direct seed for ranking section; extend with achievement % and attention metadata |
| Total Customer | Company reach | Distinct customers per `SalesPersonName` in same Faktur set | Coverage metric per rep |
| Target vs Achievement chart | Company two-bar chart | Per-rep bar chart possible from target table + grouped omzet | Presentation pattern exists in `TargetVsAchievementChart.vue` |

**Key gap:** Sales snapshot persists Top Salesman omzet but **does not** persist per-rep target, achievement %, customer count, or bottom-performer signals.

### 3.3 Piutang Dashboard (`/dashboard/piutang`)

| KPI / section | Salesman relevance | Reuse for M18 |
| ------------- | ------------------ | ------------- |
| Total Piutang / Overdue Customer / Aging | Company and customer only | Aggregate FF1 rows by `SalesName` using same aging rules as `DashboardPiutangAggregator` |
| Top 10 Outstanding Customers | Customer ranking | Drill-down validation for customer risk on rep's book — not salesman KPI |

### 3.4 Customer Analytics Dashboard (`/dashboard/customers`) — pattern reuse

| M17 artifact | Salesman adaptation |
| ------------ | ------------------- |
| Attention Cards (Collection, Concentration, Activity, Inactivity, Credit) | Cards for Achievement, Collection Exposure, Customer Coverage, Inactivity, Concentration **per salesman population** |
| Attention List (customer × signal) | **Salesman × signal** list — e.g., Below Target, High Overdue Exposure, High Customer Concentration |
| Top 10 Omzet + Top 10 Piutang rankings | **Top 10 Salesman by Omzet** (exists) + **Top 10 Salesman by Piutang** (new) |
| Segmentation (Klasifikasi, Wilayah, Active/Dormant) | Segmentation by **Salesman Wilayah**, **Segment**, active vs inactive reps |
| `DashboardCustomerAggregator` signal constants | Template for salesman signal naming and list composition |
| `Top10RankingTable.vue`, `ExecutiveAttentionCard.vue` | UI components proven in M16/M17 |

### 3.5 Reuse summary

| Reuse candidate | Confidence | Rationale |
| --------------- | ---------- | --------- |
| `DashboardSalesFakturAggregator.BuildTopSalesman` | **High** | Authoritative portal omzet ranking logic |
| `SalesOmzetChartAchievementPolicy` | **High** | Same achievement % formula as Desktop and portal |
| `ExecutiveSalesAchievementBandResolver` (80/100 bands) | **High** | Proven attention bands for achievement — apply per rep if PO approves |
| `SalesOmzetChartWeekGrouper` | **High** | Weekly pace per salesman |
| `DashboardPiutangAggregator` aging/overdue rules | **High** | Apply to FF1 rows grouped by `SalesName` |
| `ISalesOmzetTargetDal` | **High** | Per-rep and company targets already maintained |
| `IPiutangSalesWilayahDal` | **High** | Richest piutang row grain with salesman |
| `IFakturViewDal` | **High** | Sales, customer, wilayah per Faktur |
| `ISalesPersonDal` | **High** | Master list, Wilayah, Email, Segment |
| `DashboardCustomerAggregator` dormant logic | **Medium** | Requires attribution rule (Faktur vs route) |
| `BTR_SalesOmzet` + Desktop policies | **Medium** | Richer than Faktur-only but portal deliberately uses Faktur for M9+ consistency |
| `IEffectiveCallDal` | **Low–Medium** | Requires Email ↔ SalesPerson mapping |

---

## 4. Existing Report Reuse Analysis

### 4.1 Portal reports

| Report | Salesman-related columns | Drill-down role for M18 |
| ------ | ------------------------ | ----------------------- |
| **Sales Report** (`/reports/sales`) | `SalesName` per Faktur row | Validate per-rep omzet, customer list, Faktur Kembali status; filter via `?q=` on salesman name |
| **Piutang Report** (`/reports/piutang`) | `SalesName` per open Faktur row | Validate per-rep outstanding balance, overdue rows, customer exposure |

**Semantic note:** Piutang Report period filter (max 31 days) differs from Piutang Dashboard all-time open balance. M18 must declare which semantics apply to salesman piutang KPIs (see Open Questions).

### 4.2 Desktop reports (not in portal — validation / future exposure)

| Desktop screen | Menu code | Salesman dimension | Business use |
| -------------- | --------- | ------------------ | ------------ |
| Sales Omzet Chart | RO2 / RO2-Pusat | `SalesPersonName`, per-rep target, pipeline, top 15 comparison | Richest sales performance analytics in BTR |
| Effective Call | RO3 | `UserEmail` → customer visits, `IsEffectiveCall` | Field productivity |
| Coordinate Coverage | RO4 | Customer GPS — no salesman column | Indirect — route/customer coverage |
| Piutang Sales Wilayah | FF1 | `SalesName`, `WilayahName`, customer, `KurangBayar`, `Retur` | Authoritative piutang detail with salesman |
| Penerimaan Pelunasan Sales | FF2 (Finance) | `SalesName`, daily payment totals | Collection performance by rep |
| Lunas Piutang | FT1 | Sales combo — route-day collection queue | Operational collection by salesman |
| Piutang Tracker | FT5 | `SalesPersonName` on Faktur | Invoice-level collection lifecycle |
| Retur Jual | RF1 | `SalesName` | Return volume by rep |
| Faktur Control | (Faktur module) | Filter by salesman | Faktur Kembali workflow backlog |

### 4.3 KPI-to-report traceability matrix

| Salesman KPI candidate | Primary data source | Validating report | Reconciliation rule | Match type |
| ---------------------- | ------------------- | ----------------- | --------------------- | ---------- |
| Top Salesman by omzet (month) | `BTR_PortalDashboardSalesTopSalesman` / `DashboardSalesFakturAggregator` | Sales Report | Group by `SalesName`, sum `FakturTotal` in month | **Exact** |
| Per-salesman achievement % | `BTR_SalesOmzetTarget` + `FakturView` grouped by salesman | Sales Report + target master (Desktop RO2) | `SUM(GrandTotal) / TargetAmount × 100` | **Derivable** |
| Company achievement % (baseline) | Sales snapshot KPI | Sales Report | Total omzet / `SumTargetAmountForMonth` | **Exact** (existing) |
| Per-salesman customer reach | `FakturView` distinct `CustomerCode` per `SalesPersonName` | Sales Report | Distinct customers per `SalesName` in period | **Derivable** |
| Per-salesman total open piutang | `IPiutangSalesWilayahDal` grouped by `SalesName` | Piutang Report (scope per PO) | Sum `KurangBayar` where `> 1` per salesman | **Derivable** |
| Per-salesman overdue customer count | FF1 rows + aging rule from `DashboardPiutangAggregator` | Piutang Report | Distinct customers with `JatuhTempo < today` per `SalesName` | **Derivable** |
| Per-salesman aging > 90d amount | FF1 + `DaysOver90` bucket key | Piutang Report | Sum balances where overdue > 90 days per salesman | **Derivable** |
| Top salesman by piutang | FF1 grouped by `SalesName` | Piutang Report | Rank by sum `KurangBayar` | **Derivable** (no dashboard KPI yet) |
| Top customer % within salesman omzet | `FakturView` salesman × customer | Sales Report | Top customer sum / rep total omzet | **Derivable** |
| Dormant customers per salesman | Last Faktur per customer + salesman attribution | Sales Report (historical) | M17 dormant rule rolled up to rep | **Partial** — attribution rule required |
| Collection received (period) | `PenerimaanPelunasanSalesDal` | Desktop FF2 | Sum `TotalBayar` per `SalesName` in period | **Desktop only** |
| Effective call count / rate | `IEffectiveCallDal` | Desktop RO3 | Visits and `OrderCount > 0` per `UserEmail` | **Desktop only** |
| Retur total per salesman | `IReturJualViewDal` | Desktop RF1 | Sum `GrandTotal` per `SalesName` | **Desktop only** |
| Route customer count | `BTR_SalesRute` + `BTR_SalesRuteItem` | Desktop SM4 | Count customers assigned to rep's routes | **Desktop only** (master UI) |
| Pipeline omzet per salesman | `BTR_SalesOmzet` Outstanding rows | Desktop RO2 | Sum `OrderTotal` per `SalesPersonName` | **Desktop only** |

---

## 5. Salesman Risk Analysis

Salesman-related risks already measurable within BTR. Threshold values are **not defined here** — only business meaning and data availability.

### 5.1 Sales performance risks

| Risk | Business meaning | Measurable from | Portal today? |
| ---- | ---------------- | --------------- | ------------- |
| **Low target achievement** | Rep below monthly sales plan | `BTR_SalesOmzetTarget` + Faktur omzet per rep | **No** |
| **No target configured** | Plan gap — cannot evaluate performance | Missing row in `BTR_SalesOmzetTarget` | **No** |
| **Uneven team performance** | Wide spread between top and bottom omzet | Top 10 ranking + full rep list omzet | **Partial** — top only |
| **Decelerating weekly pace** | Rep billing slower week-over-week | Weekly omzet per rep | **No** |
| **Pipeline buildup** | Orders not converting to Faktur | `BTR_SalesOmzet` Outstanding | **Desktop only** |
| **Omzet reconciliation drift** | Materialized omzet out of sync | `SalesOmzetHealthWeekly` | **Desktop only** |

### 5.2 Customer portfolio risks

| Risk | Business meaning | Measurable from | Portal today? |
| ---- | ---------------- | --------------- | ------------- |
| **Customer concentration** | Few customers dominate rep's omzet | Faktur grouped by salesman + customer | **No** |
| **Shrinking customer base** | Fewer active customers vs prior period | Distinct customers per rep month-over-month | **No** |
| **Dormant customers on rep's book** | Stopped buying but still assigned or historically served | M17 dormant logic + attribution | **No** |
| **Cross-wilayah selling** | Customer territory mismatch | Customer vs SalesPerson `WilayahId` | **No** |

### 5.3 Receivable and collection risks

| Risk | Business meaning | Measurable from | Portal today? |
| ---- | ---------------- | --------------- | ------------- |
| **High open piutang per salesman** | Rep's invoices tie up working capital | FF1 `SalesName` + `KurangBayar` | **No** |
| **Piutang concentration** | One rep owns large share of company debt | Sum piutang per rep / Total Piutang | **No** |
| **Overdue exposure per salesman** | Past-due balances on rep's invoices | FF1 + overdue rules | **No** |
| **Severely aged debt per salesman** | > 90 days on rep's book | Aging buckets per rep | **No** |
| **Inactive customer with legacy debt** | No recent sales but open balance on rep's Faktur history | Last Faktur + balance + SalesName | **No** |
| **Collection lag** | Payments not keeping pace with rep's billing | Penerimaan Pelunasan vs Faktur omzet | **Desktop only** (M20) |
| **Retur-heavy settlement** | Rep's customers settle via returns | FF1 `Retur` column per salesman | **Desktop only** |

### 5.4 Field execution risks

| Risk | Business meaning | Measurable from | Portal today? |
| ---- | ---------------- | --------------- | ------------- |
| **Ineffective visits** | Check-ins without orders | `EffectiveCallView` | **Desktop only** |
| **Route non-compliance** | Route customers not visited | `BTR_SalesRuteItem` vs `BTR_CheckIn` | **No** |
| **Missing GPS on route customers** | Cannot verify visit coverage | `CustomerLocationView` for route customers | **Desktop only** |

### 5.5 Return and quality risks

| Risk | Business meaning | Measurable from | Portal today? |
| ---- | ---------------- | --------------- | ------------- |
| **High return volume per rep** | Excessive retur on rep's transactions | `ReturJualView.SalesName` | **Desktop only** |
| **High return rate per rep** | Retur vs sales ratio | Retur + Faktur per salesman | **No** |

### 5.6 Risk coverage summary

| Risk category | Portal readiness | Richest existing source |
| ------------- | ---------------- | ----------------------- |
| Sales performance / target | **Low** | `BTR_SalesOmzetTarget` + `IFakturViewDal` / RO2 |
| Customer portfolio | **Low** | `IFakturViewDal` + M17 patterns |
| Receivable / collection | **None** (salesman aggregate) | `IPiutangSalesWilayahDal` (FF1) |
| Field execution | **None** | RO3 + SM4 + `BTR_CheckIn` |
| Returns | **None** | `IReturJualViewDal` (RF1) |

---

## 6. Customer Ownership Analysis

**Authoritative attribution (Product Owner):** Customer ownership for M18 coverage and dormant metrics uses **Last Invoicing Salesman** (most recent Faktur's `SalesPersonId`). Piutang attribution uses **Invoicing Salesman** (FF1 model). Route-based ownership (`BTR_SalesRute`) is excluded from M18 — reserved for M25.

### 6.1 Ownership models discovered in BTR

BTR does **not** store a direct `SalesPersonId` on `BTR_Customer`. Salesman–customer relationships are implied through multiple models:

| Model | Mechanism | Tables / fields | Business meaning |
| ----- | --------- | --------------- | ---------------- |
| **Transactional ownership** | Salesman on each Faktur at invoice time | `BTR_Faktur.SalesPersonId` → `BTR_SalesPerson` | **Authoritative for revenue and piutang attribution** — who invoiced the sale |
| **Route / planned coverage** | Customers assigned to salesman route by day of week | `BTR_SalesRute` (`SalesPersonId`, `HariRuteId`) + `BTR_SalesRuteItem` (`CustomerId`) | **Operational plan** — who should visit/collect which customers on which day (SM4-Rute, FT1) |
| **Territory (Wilayah)** | Parallel wilayah on master records | `BTR_SalesPerson.WilayahId`; `BTR_Customer.WilayahId` | **Geographic segmentation** — not strict ownership; customer and salesman wilayah may differ |
| **Collection session** | User selects salesman in collection UI | `LunasPiutang2Form` — `SalesCombo` filters route customers | **Workflow filter** — not persisted analytics ownership |
| **Mobile field activity** | Check-in linked to user email | `BTR_CheckIn.UserEmail`; `BTR_SalesPerson.Email` | **Potential bridge** between field user and salesman master — requires reliable email population |

### 6.2 Collection responsibility

| Concept | How BTR represents it | Reliable for analytics? |
| ------- | ---------------------- | ----------------------- |
| **Invoicing salesman** | `Faktur.SalesPersonId` on open piutang via `BTR_Piutang` → `BTR_Faktur` join (FF1, Piutang Report) | **Yes** — used in all piutang reports with `SalesName` |
| **Collection route salesman** | `BTR_SalesRute` defines customers per rep per day; FT1 uses this for collection queue | **Yes for planned coverage** — different from who invoiced |
| **Payment recording** | `BTR_PiutangLunas` — attributed back to Faktur's `SalesPersonId` in `PenerimaanPelunasanSalesDal` | **Yes** — payments attributed to invoicing salesman |

**Business implication:** Collection analytics by salesman in Desktop (FF2) attribute payments to the **invoicing salesman** on the underlying Faktur, not necessarily the collector who visited the customer.

### 6.3 Metrics attribution reliability

| Metric | Recommended attribution key | Reliability | Caveat |
| ------ | --------------------------- | ----------- | ------ |
| Invoiced omzet (month) | `Faktur.SalesPersonId` / `SalesPersonName` | **High** | Portal already uses `SalesPersonName` on `FakturView` |
| Monthly target achievement | `SalesPersonId` in `BTR_SalesOmzetTarget` | **High** | Name-based join requires `ISalesPersonDal` lookup |
| Open piutang / overdue | `SalesName` from FF1 (Faktur join) | **High** | Piutang snapshot DAL omits salesman — must use FF1 pattern |
| Distinct customers (month) | `SalesPersonName` on month Fakturs | **High** | Transactional — customers can appear under multiple reps if invoiced by different salesmen |
| Dormant customers | Last Faktur's salesman | **High** | **Approved:** Last Invoicing Salesman (M17 dormant rule rolled up per rep) |
| Route coverage / visit compliance | `SalesPersonId` on route vs `UserEmail` on check-in | **Medium** | Email must map 1:1 to salesman |
| Effective call rate | `UserEmail` | **Medium** | Same email mapping requirement |
| Customer Wilayah segmentation | `Customer.WilayahId` on Faktur or master | **High** | Independent of salesman — used for territory imbalance analysis |
| Retur volume | `ReturJual.SalesPersonId` | **High** | Retur document carries salesman |

### 6.4 Salesman master data (`BTR_SalesPerson`)

| Field | Analytics use |
| ----- | ------------- |
| `SalesPersonId` | Primary key for targets, routes, Faktur joins |
| `SalesPersonCode` | Display / drill-down (not on portal Top 10 today) |
| `SalesPersonName` | Current portal ranking key — **ambiguous if names change** |
| `WilayahId` | Territory assignment for rep |
| `Email` | Bridge to `BTR_CheckIn.UserEmail` for field metrics |
| `SegmentId` | User-defined salesman segment — **no portal analytics today** |

---

## 7. Existing Asset Discovery

Objective: maximize reuse and avoid new business calculations when equivalent logic already exists.

### 7.1 Portal snapshot layer

| Asset | Path / table | Reusable for M18 |
| ----- | ------------ | ---------------- |
| Sales Top 10 snapshot | `BTR_PortalDashboardSalesTopSalesman` | Seed omzet ranking — extend with `SalesPersonId`, target, achievement % |
| Sales KPI snapshot | `BTR_PortalDashboardSalesKpi` | Company target/achievement reference |
| Sales week trend | `BTR_PortalDashboardSalesWeekTrend` | Pattern for per-rep weekly rows |
| Piutang KPI / aging / top customer | `BTR_PortalDashboardPiutang*` | Aging bucket definitions in `DashboardPiutangAggregator` |
| Customer attention snapshot | `BTR_PortalDashboardCustomer*` | Attention list composition pattern (M17) |
| `DashboardSalesFakturAggregator` | `ReportingContext/DashboardSnapshotAgg/Services` | `BuildTopSalesman`, `BuildWeekTrend`, `TotalCustomer` logic |
| `DashboardPiutangAggregator` | Same | Overdue count, aging buckets, top-N pattern |
| `DashboardCustomerAggregator` | Same | Dormant, plafond, suspended signal patterns — adapt to salesman rollup |
| `ExecutiveSalesAchievementBandResolver` | `DashboardExecutiveAgg/Services` | 80/100 achievement bands |
| `RefreshDashboardSalesSnapshotWorker` | Uses `IFakturViewDal` + `ISalesOmzetTargetDal` | Source wiring reference |

### 7.2 Source DALs (read paths)

| Asset | Interface / class | Grain | Salesman field |
| ----- | ----------------- | ----- | -------------- |
| Faktur list | `IFakturViewDal` | Per Faktur | `SalesPersonName` |
| Piutang open detail | `IPiutangSalesWilayahDal` | Per open Faktur | `SalesName` |
| Piutang open (snapshot) | `IPiutangOpenBalanceDal` | Per open Faktur | **None** — gap for M18 |
| Sales targets | `ISalesOmzetTargetDal` | Per SalesPersonId × month | `GetTargetAmount`, `SumTargetAmountForMonth` |
| Salesman master | `ISalesPersonDal` | Per salesman | Id, Code, Name, Wilayah, Email, Segment |
| Customer master | `ICustomerDal` | Per customer | Wilayah, Plafond, IsSuspend |
| Last Faktur per customer | `ICustomerLastFakturDal` (M17) | Per customer | **No salesman on DTO** — extend or join Faktur |
| Sales report | `ISalesReportDal` | Per Faktur | `SalesName` |
| Piutang report | `IPiutangReportDal` | Per open Faktur | `SalesName` |

### 7.3 Desktop application layer (not yet in portal)

| Asset | Purpose | Reuse value |
| ----- | ------- | ----------- |
| `SalesOmzetChartSummaryBuilder` | KPI summary, weekly slices, `BuildManagerComparison` (top 15 reps) | Richest salesman comparison logic |
| `SalesOmzetChartAchievementPolicy` | Achievement % | Already used in portal sales refresh |
| `SalesOmzetChartAmountPolicy` | Recognized vs pipeline amounts | Needed only if M18 includes pipeline |
| `SalesOmzetTargetResolver` | Resolve target for filtered salesman | Per-rep target wiring |
| `ReconcileSalesOmzetWorker` | Maintains `BTR_SalesOmzet` | Background data dependency if pipeline metrics included |
| `SalesOmzetHealthWeeklyDal` | Data quality health | Optional attention signal |
| `EffectiveCallDal` | Visit effectiveness | Field activity KPIs |
| `PenerimaanPelunasanSalesDal` | Collections by salesman | Collection performance — overlaps M20 |
| `ReturJualViewDal` | Returns by salesman | Return risk KPIs |
| `SalesRuteItemDal` | Route membership | Coverage KPIs |

### 7.4 Portal frontend components

| Component | Reuse |
| --------- | ----- |
| `Top10RankingTable.vue` | Salesman rankings with optional % column |
| `ExecutiveAttentionCard.vue` / `ExecutiveAttentionCardGroup` | Attention cards |
| `WeeklyTrendChart.vue` | Per-rep or team trend if exposed |
| `TargetVsAchievementChart.vue` | Per-rep or comparison chart |
| `ReportFilterBar.vue` + `useReportFreeTextFilter` | Salesman name pre-filter on reports (`?q=`) |
| `navigateToReport.ts` | Drill-down navigation pattern from M17 |

### 7.5 Protected / avoid duplication

| Asset | Note |
| ----- | ---- |
| `DashboardSalesFakturAggregator` | Protected sales snapshot logic — extend via new salesman domain, do not alter company KPI semantics |
| `DashboardExecutiveComposer` | **No M18 changes** — executive page unchanged (Q35) |
| `BTR_PortalDashboardSalesTopSalesman` | Existing table serves Sales Dashboard — M18 adds **separate** `BTR_PortalDashboardSalesman*` tables (Q36) |

---

## 8. Exception-Based Management Analysis

Focus: salesman situations deserving management attention — not just rankings. **Approved signals and bands** are in Section 12.3–12.4. Items below include discovery context; in-scope vs excluded per PO decisions.

### 8.1 Warning condition candidates

| Signal candidate | Business meaning | Data basis | Existing precedent |
| ---------------- | ---------------- | ---------- | ------------------ |
| **Achievement below plan** | Rep under monthly target | Per-rep omzet vs `BTR_SalesOmzetTarget` | M16 company Achievement Warning/Critical bands (80%/100%) |
| **No target configured** | Cannot evaluate rep against plan | Missing target row | Desktop shows Unknown |
| **Overdue customers on rep's book** | Collection action needed | FF1 overdue count per `SalesName` | M17 Overdue customer signal |
| **Elevated > 90d exposure** | Severely aged debt on rep's invoices | Aging bucket per salesman | M16/M17 >90d exposure |
| **High customer concentration** | Rep depends on few accounts | Top customer % within rep omzet | M17 Top Omzet % (no threshold) |
| **High piutang concentration** | Rep holds large share of company debt | Rep piutang / Total Piutang | M17 Top Piutang % pattern |
| **Dormant customers on book** | Coverage erosion | M17 dormant rule rolled up | M17 90-day dormant |
| **Suspended customers invoiced** | Policy breach by rep | Suspended + month Faktur per salesman | **Excluded** from M18 (not in approved signal list) |
| **Weekly pace deceleration** | Slowing billing intra-month | Week-over-week omzet drop per rep | **Deferred** — historical trend excluded (Q39) |
| **Pipeline buildup** | Orders not invoiced | Outstanding `BTR_SalesOmzet` per rep | **Excluded** — Faktur-only policy (Q7) |

### 8.2 Critical condition candidates

| Signal candidate | Business meaning | Data basis |
| ---------------- | ---------------- | ---------- |
| **Achievement critically low** | Far below plan — immediate intervention | Achievement % below Critical band (<80% per M16 precedent) |
| **Large overdue balance concentration** | Material collection failure on rep's portfolio | Sum overdue per rep exceeds PO-defined significance |
| **Rep with high omzet and high overdue** | Selling without collecting | Combined sales + piutang attention |
| **Zero customer reach with active target** | Target assigned but no invoiced customers | Target row exists, omzet = 0, customer count = 0 |

### 8.3 Ranking indicators (informational — not necessarily attention)

| Ranking | Existing? | Attention value |
| ------- | --------- | ----------------- |
| Top 10 by omzet | **Yes** (portal) | Recognition — low intervention value alone |
| Bottom 10 by omzet | No | **Excluded** — use Attention List instead (Q23) |
| Top 10 by open piutang | No | Collection prioritization by rep |
| Top 10 by overdue amount | No | Intervention prioritization |
| Top 10 by customer count | No | Coverage comparison |
| Top 10 by achievement % (with target) | No | **Approved mandatory ranking** (Q26) |

### 8.4 Trend indicators

| Trend | Data basis | Portal today? |
| ----- | ---------- | ------------- |
| Weekly invoiced omzet per rep | `FakturView` + week grouper | No |
| Month-over-month omzet per rep | Faktur across months | No |
| Customer count trend per rep | Distinct customers per month | No |
| Collection payments trend per rep | `PenerimaanPelunasanSalesDal` | Desktop only |

### 8.5 Attention list composition (approved)

Following M17 **customer × signal** pattern, M18 uses a **salesman × signal** attention list:

| Signal (approved) | Row grain | Example row |
| ----------------- | --------- | ----------- |
| **BelowTarget** | One row per salesman in Warning or Critical achievement band | "Budi — Achievement 62% (Target Rp 500M)" |
| **NoTarget** | One row per salesman with activity but no target configured | "Rina — No target configured" |
| **HighOverdueExposure** | One row per salesman with overdue customers on book | "Siti — 12 overdue customers, Rp 85M overdue" |
| **HighPiutangExposure** | One row per salesman with elevated open piutang / company share | "Ahmad — Rp 2.1B open piutang (28% of company)" |
| **CustomerConcentration** | One row per salesman with high top-customer % of rep omzet | "Dewi — Top customer 45% of rep omzet" |
| **DormantCustomerPortfolio** | One row per salesman with dormant customers (90-day rule, last-invoice attribution) | "Budi — 8 dormant customers on book" |

**Combined signals:** Cross-domain rows encouraged (e.g., Below Target + High Overdue Exposure on same salesman).

**Presentation:** Generic M16 **Attention Indicator** on cards (no per-signal severity). Concentration % shown without automatic thresholds (Q22). Contextual drill-down: sales-related signals → Sales Report; piutang-related signals → Piutang Report — salesman name via `?q=`.

---

## 9. Existing Desktop Capability Analysis

Desktop contains the richest salesman-oriented analytics in BTR. These should inform M18 scope before inventing new calculations.

### 9.1 Sales performance (RO2 — Sales Omzet)

| Capability | Business value for management | Portal overlap |
| ---------- | ------------------------------ | -------------- |
| Recognized vs pipeline omzet | Distinguishes billed vs ordered | Portal uses Faktur-only — pipeline excluded |
| Per-salesman target vs achievement | Core performance signal | **Gap M18 should close** |
| Top 15 salesman comparison chart | Team benchmarking | Portal has Top 10 table (omzet only) |
| Weekly omzet chart (filtered by rep) | Pace monitoring | Company trend only in portal |
| Omzet status breakdown (Selesai, Outstanding, etc.) | Order-to-invoice health | Not in portal |
| Sales Omzet Health Weekly | Data integrity monitoring | Not in portal |

### 9.2 Field activity (RO3 — Effective Call) — excluded from M18

| Capability | Business value | M18 disposition |
| ---------- | -------------- | --------------- |
| Check-in list with order count | Visit productivity | **M25 Sales Force Effectiveness Dashboard** |
| Effective vs ineffective visit counts | Coaching signal | **M25** |
| Route coverage / visit compliance (SM4) | Planned vs actual coverage | **M25** |

M18 is **outcome-based** (sales + piutang results), not activity-based. Primary M25 question: *Which salesman is active but not productive?*

### 9.3 Territory and coverage (SM3, SM4, RO4)

| Screen | Capability | M18 relevance |
| ------ | ---------- | ------------- |
| SM3-Wilayah | Wilayah master maintenance | Segmentation dimension |
| SM4-Rute | Route definition — customers per salesman per day | Coverage denominator for visit compliance |
| RO4-Coordinate Coverage | Customers with/without GPS | Indirect coverage quality — excluded from M17 |

### 9.4 Collection (FF1, FF2, FT1, FT5)

| Screen | Capability | M18 relevance |
| ------ | ---------- | ------------- |
| FF1-Piutang Sales | Detailed open piutang by salesman → wilayah → customer | **Primary piutang validation report** |
| FF2-Penerimaan Pelunasan Sales | Daily collections by salesman | **Deferred M20** — collection performance metrics |
| FT1-Lunas Piutang | Route-day collection workflow | Operational — aggregate metrics possible |
| FT5-Piutang Tracker | Per-invoice collection timeline | Deep drill-down — not dashboard KPI |

### 9.5 Returns (RF1)

| Capability | M18 relevance |
| ---------- | ------------- |
| Retur Jual list with `SalesName` | Return risk per salesman — excluded from M17 customer scope |

### 9.6 Desktop vs portal strategy

| Approach | When to use |
| -------- | ----------- |
| **Port existing DAL + policy** | Per-rep achievement, FF1 piutang aggregates — proven logic |
| **Adapt M17 snapshot pattern** | Materialize salesman KPIs for performance |
| **Defer to M20** | Collection effectiveness, DSO, payment trend (FF2) |
| **Defer to M25** | Effective Call, route coverage, visit compliance, GPS |
| **Exclude from M18** | Pipeline omzet, retur, Faktur Kembali aggregates |

---

## 10. Dashboard Layout Proposal

Text-only wireframes for discussion. Section order follows M16/M17 **Attention First** philosophy.

### 10.1 Proposal A — Salesman Attention First (**approved**)

```text
+====================================================================+
|  SALESMAN PERFORMANCE                          [/dashboard/salesmen]|
|  Subtitle: Current month sales + open piutang by salesman           |
+====================================================================+
|  1. SALESMAN ATTENTION CARDS                                        |
|  [Below Target] [High Overdue] [Coverage Loss] [Concentration]     |
|  [High Piutang Exposure]                                            |
|  Each card: count of salesmen triggering signal + Attention Indicator|
+====================================================================+
|  2. SALESMAN ATTENTION LIST                                         |
|  Salesman | Signal | Detail | Severity hint                         |
|  (one row per salesman × signal — e.g. Below Target, High Overdue)  |
+====================================================================+
|  3. PERFORMANCE & EXPOSURE RANKINGS                                  |
|  [Top 10 by Omzet]  [Top 10 by Achievement %]  [Top 10 by Piutang] |
|  Columns: Rank, Name, Code, Value, % of total                       |
|  (Exclude reps with value = 0 from Top Rankings — Q42)              |
+====================================================================+
|  4. SEGMENTATION SUMMARY                                            |
|  By Wilayah (required) | Active vs Inactive Salesman (required)     |
|  By Segment (optional) | Inactive reps included in segmentation     |
+====================================================================+
|  5. NAVIGATION                                                      |
|  → Sales Dashboard → Sales Report (salesman pre-filter)             |
|  → Piutang Dashboard → Piutang Report (salesman pre-filter)         |
+====================================================================+
```

**Rationale:** Mirrors proven M17 layout. Rankings support context; attention cards and list answer the management question.

**Fixed section order (approved):**

1. Salesman Attention Cards  
2. Salesman Attention List  
3. Performance Rankings (Top 10 Omzet, Achievement %)  
4. Exposure Rankings (Top 10 Piutang)  
5. Segmentation Summary  
6. Navigation to supporting dashboards and reports

### 10.2 Proposal B — Performance Summary First (not selected)

```text
+====================================================================+
|  SALESMAN PERFORMANCE                                               |
+====================================================================+
|  KPI ROW: Active Salesmen | Below Target Count | Total Team Omzet   |
|           Team Achievement % | Total Piutang by Team              |
+====================================================================+
|  ACHIEVEMENT CHART: Team target vs achievement (existing company)   |
|  + small multiples or table: each rep target vs achievement         |
+====================================================================+
|  ATTENTION LIST (compact)                                           |
+====================================================================+
|  RANKINGS (Top 10 omzet, Top 10 piutang)                           |
+====================================================================+
```

**Rationale:** Familiar to Sales Dashboard users. Risk: repeats company-level view before attention signals.

### 10.3 Proposal C — Collection-Heavy (not selected)

```text
+====================================================================+
|  SALESMAN PERFORMANCE & COLLECTION EXPOSURE                         |
+====================================================================+
|  ATTENTION: [Overdue Exposure] [>90d Debt] [Collection Gap]       |
+====================================================================+
|  PIUTANG BY SALESMAN (horizontal bar or table)                      |
+====================================================================+
|  SALES ACHIEVEMENT TABLE                                            |
+====================================================================+
|  → Piutang Report / FF1-style drill-down                            |
+====================================================================+
```

**Rationale:** Emphasizes collection risk. May overlap M20 — only if PO wants combined milestone scope.

### 10.4 Navigation (proposed)

```text
Sidebar:
  Dashboard
   ├─ Executive (default)  → /dashboard
   ├─ Sales                → /dashboard/sales
   ├─ Piutang              → /dashboard/piutang
   ├─ Customers            → /dashboard/customers
   ├─ Salesmen             → /dashboard/salesmen    ← M18
   ├─ Inventory            → /dashboard/inventory
   └─ Purchasing           → /dashboard/purchasing

Drill-down path:
  Salesman Performance → Domain Dashboard → Report

Salesman row click:
  → Sales Report or Piutang Report with salesman name pre-filter (?q=)
```

---

## 11. Gap Analysis

### 11.1 Information already available (reuse without new business logic)

| Information | Source | M18 use |
| ----------- | ------ | ------- |
| Top 10 salesman by omzet (month) | `BTR_PortalDashboardSalesTopSalesman` | Ranking section seed |
| Company target and achievement % | Sales snapshot KPI | Team context baseline |
| Per-salesman monthly target | `BTR_SalesOmzetTarget` | Per-rep achievement denominator |
| Achievement % formula | `SalesOmzetChartAchievementPolicy` | Per-rep achievement |
| Achievement bands (80/100) | `ExecutiveSalesAchievementBandResolver` | Attention card bands |
| Weekly omzet bucketing | `SalesOmzetChartWeekGrouper` | Per-rep pace if needed |
| Faktur-level salesman, customer, wilayah | `IFakturViewDal` / Sales Report | Coverage, concentration, validation |
| Piutang rows with salesman | `IPiutangSalesWilayahDal` / Piutang Report | Piutang and overdue by rep |
| Overdue and aging rules | `DashboardPiutangAggregator` | Apply per salesman |
| Salesman master (Wilayah, Email, Segment) | `ISalesPersonDal` | Segmentation and email bridge |
| Ranking and attention UI | M16/M17 components | Presentation |
| Report salesman pre-filter | `?q=` free-text filter | Drill-down |

### 11.2 Information partially available (M18 must implement aggregation)

| Information | What exists | Gap |
| ----------- | ----------- | --- |
| **Per-salesman achievement %** | Target table + Faktur omzet | Not aggregated in portal |
| **Per-salesman customer reach** | Faktur rows | Not computed per rep |
| **Per-salesman open piutang** | FF1 rows with `SalesName` | Not in snapshot DAL or dashboard |
| **Per-salesman overdue customers** | FF1 + aging rules | Not aggregated |
| **Per-salesman top customer concentration** | Faktur salesman × customer | Not computed |
| **Per-salesman piutang share of company total** | FF1 sums | Not computed |
| **Dormant customers per salesman** | M17 customer logic | **In scope** — Last Invoicing Salesman attribution (Q14, Q18) |
| **Bottom performers** | All reps in master + omzet | **Excluded** — Attention List replaces Bottom 10 (Q23) |
| **SalesPersonId on rankings** | Master + Faktur join | **In scope** — internal key; `SalesPersonCode` on display rows (Q30, Q40) |
| **Combined cross-domain signals** | Sales + piutang per rep | **In scope** — encouraged (Q24) |

### 11.3 Information not in M18 scope (deferred or excluded)

| Information | Disposition |
| ----------- | ----------- |
| Pipeline / outstanding order omzet per rep | **Excluded** — Faktur-only policy (Q7) |
| Effective call rate per salesman | **Excluded** — **M25 Sales Force Effectiveness** (Q8) |
| Route visit compliance | **Excluded** — **M25** (Q9) |
| GPS / coordinate coverage per rep's customers | **Excluded** (Q12) |
| Collection performance / DSO / payment trend per rep | **Deferred M20** (Q5, Q44) |
| Retur volume / rate per rep | **Excluded** (Q10) |
| Faktur Kembali aggregate per rep | **Excluded** (Q11) |
| Sales omzet health weekly per rep | Desktop admin — not in M18 scope |
| Month-over-month trend per rep | **Deferred** — current month only (Q39) |
| Salesman performance unified score | Not in scope |
| Sales activity metrics (check-in, route, visit productivity, order conversion) | **M25** — see Additional Product Direction |
| M19 slow moving / dead stock by salesman | **No salesman dimension** (Q43) |

---

## 12. Final Product Decisions (Authoritative)

**Status:** All open questions resolved by Product Owner — 2026-06-08.  
**Audience:** Architect — use this section as scope input. Do not re-decide business rules listed here.

### 12.1 Product positioning and audience

| # | Decision |
| - | -------- |
| Q1 | **Route:** `/dashboard/salesmen` · **Page title:** Salesman Performance |
| Q2 | **Primary question:** *Which salesman requires management attention and why?* |
| Q3 | **Audience:** All authenticated users — same visibility model as M16/M17 (no role-based routing) |
| Q4 | **Supplements** Sales Dashboard Top 10 — does **not** replace |
| Q5 | **Collection exposure signals** in M18; **collection performance metrics** deferred to M20 Collection Dashboard |

### 12.2 Scope boundaries

| # | Decision |
| - | -------- |
| Q6 | **Sales and Piutang only** |
| Q7 | **Faktur-only policy** — pipeline omzet excluded |
| Q8 | **Effective Call excluded** — planned **M25 Sales Force Effectiveness Dashboard** |
| Q9 | **Route coverage and visit compliance excluded** — planned **M25** |
| Q10 | **Retur per salesman:** No |
| Q11 | **Faktur Kembali aggregate:** No |
| Q12 | **GPS / coordinate coverage:** No |
| Q13 | **Dashboard only** — no new Salesman Report |

### 12.3 Attribution and business rules

| # | Decision |
| - | -------- |
| Q14 | **Customer ownership:** Last Invoicing Salesman (most recent Faktur's salesman) |
| Q15 | **Piutang attribution:** Invoicing Salesman — FF1 model |
| Q16 | **Sales metrics:** current calendar month Faktur `GrandTotal` |
| Q17 | **Piutang metrics:** all-time open balance (`KurangBayar > 1`) |
| Q18 | **Dormant customer rule:** reuse M17 — 90 days + prior transaction history; rolled up per salesman |
| Q19 | **Achievement bands:** M16 bands per salesman — ≥100% Healthy · 80–99% Warning · <80% Critical |
| Q20 | **No target configured** is an **Attention Signal** (not merely Unknown) |

### 12.4 Attention rules and indicators

| # | Decision |
| - | -------- |
| Q21 | **Approved attention list signals:** Below Target · No Target · High Overdue Exposure · High Piutang Exposure · Customer Concentration · Dormant Customer Portfolio |
| Q22 | **Concentration percentages** without automatic warning thresholds |
| Q23 | **No Bottom 10 rankings** — Attention List replaces bottom-performer visibility |
| Q24 | **Combined cross-domain signals** allowed and encouraged |
| Q25 | **Generic Attention Indicator** model (M16/M17) — no per-signal severity system |

### 12.5 Rankings, layout, and segmentation

| # | Decision |
| - | -------- |
| Q26 | **Mandatory rankings:** Top 10 Omzet · Top 10 Achievement % · Top 10 Piutang |
| Q27 | **Top N = 10** for all salesman rankings |
| Q28 | **Layout Proposal A** — Salesman Attention First |
| Q29 | **Attention List required** |
| Q30 | **SalesPersonCode** on ranking rows |
| Q31 | **Mandatory segmentation:** Wilayah · Active vs Inactive Salesman. **SegmentId optional** |

**Fixed section order:**

1. Salesman Attention Cards  
2. Salesman Attention List  
3. Performance Rankings (Top 10 Omzet, Top 10 Achievement %)  
4. Exposure Rankings (Top 10 Piutang)  
5. Segmentation Summary  
6. Navigation to supporting dashboards and reports

### 12.6 Drill-down and navigation

| # | Decision |
| - | -------- |
| Q32 | **Contextual drill-down:** sales signals → Sales Report; piutang signals → Piutang Report |
| Q33 | **Salesman name pre-filter** via `?q=` (M17 pattern) |
| Q34 | **Navigation:** Salesman Dashboard → Domain Dashboard → Report (M16/M17 pattern) |
| Q35 | **No Executive Dashboard changes** in M18 |

### 12.7 Materialization and performance

| # | Decision |
| - | -------- |
| Q36 | **Dedicated `BTR_PortalDashboardSalesman*` snapshot tables** |
| Q37 | **Do not compose at read time** — follow M17 materialization pattern |
| Q38 | **Refresh cadence:** 30 minutes |
| Q39 | **Current month snapshot only** — historical trend deferred |

### 12.8 Data quality and identity

| # | Decision |
| - | -------- |
| Q40 | **`SalesPersonId` internal key**; `SalesPersonName` display value |
| Q41 | **Email mapping not required** — RO3 excluded from M18 |
| Q42 | **Inactive salesmen** in segmentation; **excluded from Top Rankings when value = 0** |

### 12.9 Relationship to other milestones

| # | Decision |
| - | -------- |
| Q43 | **M19 Slow Moving & Dead Stock** has no salesman dimension |
| Q44 | **Collection performance metrics** deferred entirely to **M20 Collection Dashboard** |
| Q45 | **M17 Customer Analytics** may cross-link to responsible salesman when available |

### 12.10 Approved M18 KPI and snapshot candidates

Based on approved scope, the Salesman snapshot domain should materialize at minimum:

| Category | KPI / data | Source semantics |
| -------- | ---------- | ---------------- |
| Attention cards | Below Target count | Achievement % in Warning/Critical band per salesman |
| Attention cards | No Target count | Salesmen with activity but no `BTR_SalesOmzetTarget` row |
| Attention cards | High Overdue Exposure count | FF1 — overdue customers per invoicing salesman |
| Attention cards | High Piutang Exposure count | FF1 — open balance concentration per salesman |
| Attention cards | Customer Concentration signal count | Top customer % of rep omzet (informational threshold) |
| Attention cards | Dormant Portfolio count | M17 dormant rule — customers attributed via last invoicing salesman |
| Attention list | Salesman × signal rows | Six approved signals; combined signals encouraged |
| Rankings | Top 10 Omzet | Current month Faktur `GrandTotal` per `SalesPersonId` |
| Rankings | Top 10 Achievement % | Per-rep target vs omzet; exclude zero-value from ranking |
| Rankings | Top 10 Piutang | All-time open balance per invoicing salesman |
| Segmentation | By Wilayah | `BTR_SalesPerson.WilayahId` |
| Segmentation | Active vs Inactive | Active = Faktur in current month; inactive otherwise |
| Segmentation | By Segment (optional) | `BTR_SalesPerson.SegmentId` |
| Identity | SalesPersonCode on rows | From `BTR_SalesPerson` master |

**Explicitly out of scope:** Pipeline omzet, Effective Call, route compliance, retur, Faktur Kembali aggregates, GPS, collection performance/DSO, Bottom 10 rankings, historical month retention, new Salesman Report, executive dashboard changes.

---

## 13. Additional Product Direction

M18 focuses on **outcome-based performance management** — sales results and receivable exposure attributed to each salesman. It is **not** a sales activity dashboard.

### 13.1 Dashboard priority order

1. Salesman Attention Cards  
2. Salesman Attention List  
3. Performance Rankings  
4. Exposure Rankings  
5. Segmentation Summary  
6. Navigation to supporting reports  

### 13.2 Reserved for M25 — Sales Force Effectiveness Dashboard

Sales **activity** metrics are explicitly excluded from M18 and reserved for a future milestone:

| Reserved metric area | Examples |
| -------------------- | -------- |
| Route coverage | Planned vs visited customers |
| Check-in / visit activity | Visit counts, visit frequency |
| Effective Call | Visit-to-order conversion |
| Visit productivity | Activity without outcome |
| Sales Order conversion | Order pipeline behavior |

**M25 primary question:** *Which salesman is active but not productive?*

This complements M18's question: *Which salesman requires management attention based on outcomes (sales + collection exposure)?*

### 13.3 Milestone boundary summary

| Milestone | Salesman lens | Primary question |
| --------- | ------------- | ---------------- |
| **M18** | Outcome performance | Which salesman requires management attention and why? (achievement, exposure, concentration, dormant portfolio) |
| **M20** | Collection performance | How effective is collection? (payments, DSO — company/collection lens) |
| **M25** | Field effectiveness | Which salesman is active but not productive? (routes, visits, calls) |

---

## Appendix A — Terminology

| Term | BTR meaning |
| ---- | ----------- |
| **Salesman / Sales Person** | `BTR_SalesPerson` — field sales and invoicing representative |
| **SalesPersonId** | Internal key for snapshot tables and joins (approved Q40) |
| **SalesPersonCode** | Business code on ranking rows for drill-down support (approved Q30) |
| **SalesPersonName** | Display name on Faktur and reports |
| **Wilayah** | Territory — on both Customer and SalesPerson master |
| **Route (Rute)** | Planned customer visit list per salesman per day (`BTR_SalesRute`) |
| **Achievement %** | Recognized omzet / target × 100 — `SalesOmzetChartAchievementPolicy` |
| **Open piutang** | `KurangBayar > 1` (business rule from Piutang snapshot) |
| **FF1** | Desktop Piutang Sales Wilayah report — piutang with salesman dimension |

---

## Appendix B — Discovery sources

| Source | Use in this analysis |
| ------ | -------------------- |
| `DashboardSalesFakturAggregator.cs` | Top salesman, week trend, customer count logic |
| `RefreshDashboardSalesSnapshotWorker.cs` | Company target sum, Faktur source |
| `PiutangOpenBalanceDal.cs` vs `PiutangSalesWilayahDal.cs` | Salesman gap in snapshot vs report |
| `DashboardCustomerAggregator.cs` | Attention signal patterns for M18 adaptation |
| `BTR_SalesOmzetTarget.sql` | Per-rep target schema |
| `btr-portal-domain.md` | Portal scope, M18 roadmap position |
| `M17-Customer-Analytics-Analysis.md` | Parallel analysis structure and PO decision patterns |
| `btr-reporting-investigation.md` | Desktop report inventory and RO2 maturity |

---

*End of analysis. Hand to Architect — Section 12 contains authoritative Product Owner decisions.*
