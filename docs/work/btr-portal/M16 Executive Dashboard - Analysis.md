# BTR Portal Analysis — M16 Executive Dashboard

**Status:** Implemented — knowledge extracted to `docs/features/btr-portal/` (see `knowledge-extraction-report-m16-m17.md`).

**Implementation:** [`M16 Executive Dashboard - Plan.md`](./M16%20Executive%20Dashboard%20-%20Plan.md) · [`M16 Executive Dashboard - Implementation.md`](./M16%20Executive%20Dashboard%20-%20Implementation.md)  
**Scope:** Business analysis only. No implementation plans, API design, database design, or code.  
**Date:** 2026-06-08 (analysis) · Product Owner decisions recorded 2026-06-08  
**Context:** BTR Portal Version 1 (M1–M15) is complete. M16 replaces `/dashboard` with a **Management Attention Center** guided by: *What requires management attention today?*

**Reference documents:** `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-operational.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md`, `docs/archive/btr-portal-api-scaffolding/portal-analysis-m13-m15-final.md`, `docs/work/purchasing-dashboard/analysis-report.md`, `docs/foundation/PRODUCT.md`, `docs/foundation/WORKFLOW.md`, `docs/archive/materialize-dashboard-data/analysis-report.md`

---

## 1. Executive Summary

BTR Portal V1 answers **"What happened?"** through domain-specific dashboards that require management to visit four separate pages. Each page mixes **headline totals** (omzet, piutang, inventory value, purchase spend) with **some exception signals** (target gap, overdue customers, posting backlog), but nothing synthesizes cross-domain priorities on one screen.

M16 introduces an **Executive Dashboard** — titled **Management Attention Center** — that **replaces `/dashboard`** as the primary landing page for all authenticated users. It is designed for **daily morning review**, guided by:

> What requires management attention today?

**All open questions resolved.** See Section 10 for authoritative Product Owner decisions.

### Key findings

| Finding | Implication |
| ------- | ----------- |
| **Attention signals already exist in domain dashboards** but are not promoted to the home page | Executive Dashboard can reuse snapshot data with minimal new business logic |
| **Piutang and Purchasing** have the strongest existing exception KPIs (`Overdue Customer`, aging buckets, `Pending Posting Invoice Count`) | Natural candidates for "Critical Alerts" |
| **Sales target achievement** is the only cross-company performance-vs-plan metric today | Primary sales attention signal when targets are configured |
| **Inventory** exposes concentration (Top 10 category/supplier) but **no slow-moving, dead stock, or ABC logic** in portal or snapshots | Concentration risk is available; obsolescence risk is not |
| **Desktop-only analytics** (`SalesOmzetHealthWeekly`, `FakturControl`, `StokBalanceHealth`, `PiutangTracker`) contain management-relevant signals **not exposed in portal** | Reuse candidates exist outside ReportingContext |
| **Reports support drill-down** with period filters (max 31 days) and client-side search | Executive KPIs can link to reports for validation — drill-down from charts remains deferred |
| **Current `/dashboard` home** is an operational summary, not an executive view | **Approved:** replace home with Management Attention Center; domain dashboards unchanged |

### Approved product outcome

Compose **exception-oriented metrics** from existing snapshots across Sales, Piutang, Inventory, and Purchasing, plus **snapshot health**. Use **Proposal A (Attention First)** layout. Avoid new business domains (slow-moving stock, collection effectiveness, pipeline omzet) — deferred to future milestones. **New presentation logic** is in scope for concentration ratios and consolidated freshness; avoid new transactional SQL where snapshot data suffices.

---

## 2. Management Attention Discovery

This section identifies operational situations that typically require management intervention, mapped to **existing BTR capabilities**. Items marked **Portal today** are calculable from current portal snapshots or reports. Items marked **Desktop only** exist in BTR Desktop but are not in the portal. Items marked **Not available** have no implemented logic discovered in codebase or documentation.

### 2.1 Sales

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Sales under monthly target** | Company invoiced omzet is below plan; cash and margin targets at risk | `Achievement %`, `Total Target`, `Total Achievement` on Sales Dashboard; `SalesOmzetChartAchievementPolicy` | **Portal today** |
| **Weekly sales deceleration** | Billing pace slowing mid-month; may miss target if trend continues | Weekly Invoiced Sales Trend (`BTR_PortalDashboardSalesWeekTrend`) | **Portal today** — trend visible; no automated "deceleration" flag |
| **Salesperson underperformance vs peers** | Territory or rep needs coaching or reassignment | Top 10 Salesman ranking (by omzet); no bottom-10 or per-rep target comparison on portal | **Partial** — ranking only; per-rep target achievement exists in Desktop (`SalesOmzetChartForm`) |
| **Large customer revenue concentration** | Dependency on few accounts | Not on portal dashboard; dimension available on `FakturView` (`CustomerCode`, `CustomerName`) | **Not available** in portal — data exists at source |
| **Unsigned / not-returned Faktur backlog** | Goods delivered but signed Faktur not returned (`Faktur Kembali` workflow incomplete) | Sales Report `Status = Kembali`; Desktop `FakturControlForm` filters Kembali / Belum Kembali | **Partial** — row-level in report via search; no aggregate KPI |
| **Sales omzet data quality / reconciliation risk** | RO2 omzet records out of sync with Faktur | `SalesOmzetHealthWeekly` (`Good` / `Warning` / `Poor`); `SalesOmzetHealthPolicy` with score thresholds | **Desktop only** — not in portal |
| **Pipeline / outstanding orders not invoiced** | Orders booked but not yet billed | `PipelineOmzet` reserved; always `0` after Faktur cutover | **Explicitly excluded** from V1 portal |

### 2.2 Finance / Piutang

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Large total outstanding receivables** | Working capital tied up in customer debt | `Total Piutang` (dashboard + report footer when unfiltered) | **Portal today** |
| **Customers with overdue balances** | Collection action required | `Overdue Customer` KPI; aging buckets exclude Current | **Portal today** |
| **Severely aged receivables (> 90 days)** | High bad-debt or collection failure risk | Aging bucket `> 90 Days` amount in `BTR_PortalDashboardPiutangAging` | **Portal today** |
| **Customer concentration in receivables** | Single customer default would materially impact cash | Top 10 Outstanding Customers | **Portal today** |
| **Large individual overdue invoices** | Specific collection follow-up | Piutang Report — sort by `Jatuh Tempo`; filter by period on DueDate | **Portal today** (report drill-down) |
| **Collection effectiveness / DSO** | Are collections improving over time? | Deferred in product roadmap ("collection effectiveness KPIs") | **Not available** |
| **Regional / sales-territory receivable exposure** | Which wilayah or salesman owns the risk | `IPiutangSalesWilayahDal` includes `WilayahName`, `SalesName`; not aggregated on portal | **Partial** — source data exists; no dashboard aggregate |
| **Piutang lifecycle anomalies** | Unusual adjustments, payment timing | Desktop `PiutangTrackerForm` (`IPiutangTrackerDal`) | **Desktop only** |

### 2.3 Inventory

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **High inventory capital tie-up** | Cash locked in stock | `Total Inventory Value` | **Portal today** — informational, not exception by itself |
| **Category concentration risk** | Too much value in one product category | Top 10 Categories; category horizontal bar chart | **Portal today** |
| **Supplier/principal concentration risk** | Over-dependence on one principal's stock | Top 10 Suppliers; supplier horizontal bar chart | **Portal today** |
| **Slow-moving inventory** | Stock not turning; obsolescence risk | Deferred ("ABC analysis"); Desktop `StokPeriodikForm` for point-in-time snapshots | **Not available** in portal |
| **Dead stock (zero movement)** | Items with qty but no recent sales/outflow | No portal logic discovered | **Not available** |
| **Stock balance integrity issues** | `BTR_StokBalanceWarehouse` out of sync with `BTR_Stok` | `StokBalanceHealthDal` — mismatch count | **Desktop only** (admin/IT operational) |
| **In-Transit stock buildup** | Goods in transit not available for sale | In-Transit warehouse **excluded** from all portal inventory metrics | **Not visible** — by design |
| **Warehouse-level imbalance** | Branch overstocked or understocked | Deferred ("warehouse breakdown") | **Not available** in portal |

### 2.4 Purchasing

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Purchase invoices awaiting stock posting** | Goods invoiced but not received into inventory (`BELUM`) | `Pending Posting Invoice Count`; Posting Status Breakdown (`BELUM` value) | **Portal today** |
| **High monthly purchasing spend** | Cash outflow / budget pressure | `Grand Total Purchase` | **Portal today** — informational unless budget exists |
| **Principal/supplier spend concentration** | Dependency on one supplier for monthly intake | Top 10 Principal | **Portal today** |
| **Purchasing pace anomaly** | Unusually high or low weekly purchase volume | Weekly Purchase Trend | **Portal today** — visual only; no alert flag |
| **Pending posting monetary exposure** | Value of unposted purchases (working capital in limbo) | `BELUM` slice amount in posting breakdown; deferred as dedicated KPI ("pending posting value") | **Partial** — amount in chart, not headline KPI |
| **Retur Beli (purchase returns)** | Return anomalies affecting supplier relationships | Desktop PF4; excluded from portal purchasing scope | **Not available** in portal |
| **Purchasing vs budget** | Over-spending against plan | No target master data | **Not available** |

### 2.5 Cross-domain / platform

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Dashboard data stale or failed refresh** | Management decisions based on outdated numbers | `GeneratedAt` per domain; `GET /api/health/dashboard-snapshots` (`ok` / `refreshing` / `degraded` / `unknown`); `HasUnavailableDomain` on overview | **Portal today** |
| **Multi-domain operational breakdown** | Sales up but collections failing; purchases posted but inventory not moving | No cross-domain correlation logic | **Not available** — requires Product Owner definition |

### 2.6 Workflow-derived attention points

From `docs/foundation/WORKFLOW.md` and portal operational workflows:

| Workflow stage | When management cares | Portal support today |
| -------------- | --------------------- | -------------------- |
| Sales Order → Faktur → Fulfillment → **Faktur Kembali** | Stalled signed-document return | Sales Report status column only |
| Faktur → **Piutang** → Collection → Payment | Overdue and concentrated debt | Strong — Piutang Dashboard + Report |
| Purchase → **Stock Receipt (Posting)** → Inventory | Backlog of `BELUM` invoices | Strong — Purchasing Dashboard + Report |
| Inventory planning | Capital concentration by category/supplier | Moderate — no movement/ABC signals |

---

## 3. Existing Dashboard Reuse Analysis

### 3.1 Dashboard Home (`/dashboard`) — current state

**Purpose today:** Operational summary — four KPI cards with headline totals and links to detail dashboards.

| Card | Metrics shown | Attention-oriented? |
| ---- | ------------- | ----------------- |
| Sales | Invoiced Omzet, Total Faktur, Total Customer | **No** — volume/reach totals only; no target % or trend |
| Piutang | Total Piutang, Total Customer | **Partial** — exposure total; no overdue count or aging |
| Purchasing | Grand Total Purchase, Total Invoice | **No** — spend totals; no pending posting count |
| Inventory | Total Inventory Value, Total Item | **No** — capital total; no concentration signal |

**Assessment:** Home cards are **operational**, not **executive**. They omit the strongest exception KPIs already computed in detail dashboards (`Achievement %`, `Overdue Customer`, `Pending Posting Invoice Count`, aging > 90 days share).

### 3.2 Sales Dashboard (`/dashboard/sales`)

| KPI / section | Executive suitability | Operational only? | Promote to Executive? | Rationale |
| ------------- | ---------------------- | ------------------- | --------------------- | --------- |
| Total Target | Medium | No | Optional summary | Context for achievement; less urgent than % gap |
| Total Achievement | Medium | No | Yes (compact) | Headline performance number |
| **Achievement %** | **High** | No | **Yes — priority** | Direct "requires attention" when below 100% |
| Target vs Achievement chart | Medium | No | Optional | Visual reinforcement of target gap |
| Weekly Invoiced Sales Trend | High | No | **Yes — compact sparkline or status** | Pace indicator for month-end risk |
| Top 10 Salesman | Low–Medium | Partially | Optional "top performer" | Recognition more than intervention; underperformers not shown |

### 3.3 Piutang Dashboard (`/dashboard/piutang`)

| KPI / section | Executive suitability | Operational only? | Promote to Executive? | Rationale |
| ------------- | ---------------------- | ------------------- | --------------------- | --------- |
| Total Piutang | Medium | No | Yes (compact) | Scale of exposure |
| Total Customer | Low | Yes | No | Breadth less actionable than overdue count |
| **Overdue Customer** | **High** | No | **Yes — priority** | Count of customers needing collection attention |
| **Aging Distribution (> 90 Days slice)** | **High** | No | **Yes — priority** | Severity of collection failure |
| Aging Distribution (other buckets) | Medium | No | Optional | 61–90 and 31–60 useful for finance detail |
| Top 10 Outstanding Customers | High | No | **Yes — Top 5–10 risks** | Concentration + actionable names |

### 3.4 Inventory Dashboard (`/dashboard/inventory`)

| KPI / section | Executive suitability | Operational only? | Promote to Executive? | Rationale |
| ------------- | ---------------------- | ------------------- | --------------------- | --------- |
| Total Inventory Value | Medium | No | Yes (compact) | Capital at risk — needs context (concentration) |
| Total Item | Low | Yes | No | SKU count less meaningful for executives |
| Inventory by Category (chart) | Medium | Partially | Optional summary | Concentration signal — may be too detailed for landing |
| Inventory by Supplier (chart) | Medium | Partially | Optional summary | Same as category |
| **Top 10 Categories** | **High** | No | **Yes — top 3–5** | "Where is capital stuck?" |
| **Top 10 Suppliers** | **High** | No | **Yes — top 3–5** | Principal dependency in stock |

### 3.5 Purchasing Dashboard (`/dashboard/purchasing`)

| KPI / section | Executive suitability | Operational only? | Promote to Executive? | Rationale |
| ------------- | ---------------------- | ------------------- | --------------------- | --------- |
| Grand Total Purchase | Medium | No | Yes (compact) | Monthly spend headline |
| Total Invoice | Low | Yes | No | Volume secondary to backlog and spend |
| **Pending Posting Invoice Count** | **High** | No | **Yes — priority** | Operational backlog blocking inventory update |
| Weekly Purchase Trend | Medium | No | Optional | Pace monitoring |
| **Posting Status Breakdown (BELUM amount)** | **High** | No | **Yes — priority** | Monetary exposure of unposted purchases |
| Top 10 Principal | Medium–High | No | **Yes — top 3–5** | Supplier concentration in monthly spend |

### 3.6 Summary — approved promotion list (Section 10)

**On executive view (Attention Cards + Domain Summaries):**

| Domain | Approved metrics |
| ------ | ---------------- |
| Sales | Achievement % (with Healthy/Warning/Critical band), Achievement Value |
| Piutang | Total Piutang, Overdue Customer, > 90 Day Amount (+ % of total) |
| Inventory | Total Inventory Value, Top Category % |
| Purchasing | Pending Posting Count, Pending Posting Value |
| System | Snapshot Health, consolidated Last Refreshed |

**Critical Exposure Lists (Top 5, grouped):** Top Customers, Top Categories, Top Suppliers, Top Principals — each with concentration % where applicable.

**Remain on domain detail dashboards only:** weekly trends, full Top 10 tables, aging pie chart, posting pie chart, Target vs Achievement chart, operational volume totals (Faktur, Customer, Item, Invoice counts).

---

## 4. Existing Report Reuse Analysis

Reports provide **transaction-level validation** and **drill-down** from executive indicators. Reports query Desktop DALs live (not snapshotted). Period filters: max **31 days**; default current calendar month (Piutang default: current month on `Jatuh Tempo`).

### 4.1 Reports with management-level information

| Report | Management-level content | Executive relevance |
| ------ | ------------------------ | ------------------- |
| **Sales Report** | All Fakturs in period; `Kembali` status | Validate omzet; identify unsigned Faktur rows |
| **Piutang Report** | Open balances with DueDate; footer totals | Validate exposure; sort/prioritize collection |
| **Inventory Report** | Item × warehouse balances; footer totals | Validate capital; search high-value items |
| **Purchasing Report** | Invoices with Posting Stok; footer totals | Validate spend; filter `BELUM` for action |

### 4.2 Reports as drill-down support

| Executive indicator (candidate) | Drill-down report | How user validates |
| ------------------------------ | ----------------- | ------------------ |
| Sales under target | Sales Report | Sum `Total` column for period ≈ Total Achievement |
| Overdue customers / aging | Piutang Report | Filter by `Jatuh Tempo` period; sort ascending by due date |
| Top customer exposure | Piutang Report | Search customer name; review `Kurang Bayar` rows |
| Pending posting backlog | Purchasing Report | Search `BELUM` in Posting Stok |
| Inventory concentration | Inventory Report | Search category/supplier/item |
| Top principal spend | Purchasing Report | Search supplier name |

**Note:** Piutang Dashboard shows **all-time open balances**; Piutang Report with a period filter shows a **subset** — footer totals match dashboard only when report period encompasses all open items (or no period filter applied). Product Owner must decide whether executive view follows dashboard semantics (all open) or report default (current month on DueDate).

### 4.3 KPI-to-report traceability matrix

| Executive KPI candidate | Primary dashboard source | Validating report | Reconciliation rule | Match type |
| ----------------------- | ------------------------ | ----------------- | --------------------- | ---------- |
| Invoiced Omzet / Total Achievement | Sales Dashboard / Overview | Sales Report | Sum of `GrandTotal` for same month | **Exact** (same Faktur source, void exclusion) |
| Total Target | Sales Dashboard | — (no report) | From `BTR_SalesOmzetTarget` | **Dashboard only** |
| Achievement % | Sales Dashboard | Derived from above | — | **Derived** |
| Total Piutang | Piutang Dashboard / Overview | Piutang Report (unfiltered all-open) | Sum `KurangBayar > 1` | **Exact** when report scope = all open |
| Overdue Customer | Piutang Dashboard | Piutang Report | Count distinct customers with DueDate < today | **Derivable** from report rows |
| > 90 Days aging amount | Piutang Dashboard aging | Piutang Report | Sum balances where `Today − JatuhTempo > 90` | **Derivable** from report rows |
| Top Customer balance | Piutang Dashboard Top 10 | Piutang Report | Group by customer, sum balances | **Derivable** |
| Total Inventory Value | Inventory Dashboard / Overview | Inventory Report | BrgId-first `Sum(Hpp × Qty)` excl. In-Transit | **Exact** |
| Top Category / Supplier value | Inventory Dashboard | Inventory Report | Group visible rows (approximate; footer uses BrgId-first) | **Approximate** at row level |
| Grand Total Purchase | Purchasing Dashboard / Overview | Purchasing Report | Sum `GrandTotal` same month | **Exact** |
| Pending Posting Count | Purchasing Dashboard | Purchasing Report | Count rows where Posting Stok = `BELUM` | **Derivable** |
| BELUM purchase value | Purchasing posting breakdown | Purchasing Report | Sum `GrandTotal` where `BELUM` | **Derivable** |
| Top Principal spend | Purchasing Dashboard | Purchasing Report | Group by supplier | **Derivable** |
| Snapshot freshness | All dashboards `GeneratedAt` | — | — | **System metadata** |
| Snapshot health degraded | Health endpoint | — | — | **System metadata** |

---

## 5. Business Area Coverage

### 5.1 Sales

**Can contribute today:**

- Performance vs plan (target, achievement, %)
- Billing pace (weekly trend)
- Top performer concentration

**Gaps for executive visibility:**

- Underperforming reps (bottom rank vs target)
- Customer revenue concentration
- Faktur Kembali backlog aggregate
- Omzet data health (Desktop RO2 health indicator)
- Pipeline / un-invoiced orders

### 5.2 Finance / Piutang

**Can contribute today:**

- Total exposure, overdue customer count, aging severity, top debtor concentration

**Gaps:**

- Collection effectiveness / payment trend
- Wilayah or salesman accountability views
- DSO or aging trend over time (snapshot is point-in-time only)

**Assessment:** **Strongest executive-ready area** in portal V1.

### 5.3 Inventory

**Can contribute today:**

- Capital at risk (total value)
- Category and supplier concentration (Top 10)

**Gaps:**

- Slow-moving / dead stock
- ABC classification
- Warehouse breakdown
- Stock integrity health

**Assessment:** **Adequate for concentration risk**; **insufficient for obsolescence or movement risk**.

### 5.4 Purchasing

**Can contribute today:**

- Monthly spend, posting backlog (count + value), principal concentration, weekly pace

**Gaps:**

- Budget vs actual
- Retur Beli analytics
- Warehouse-level purchasing

**Assessment:** **Strong for operational backlog** (posting); moderate for spend control without budgets.

### 5.5 Coverage summary

| Area | Executive indicator richness | Portal V1 readiness |
| ---- | ---------------------------- | ------------------- |
| Sales | Medium | Target % and trend available; exception signals incomplete |
| Finance / Piutang | **High** | Ready for executive promotion |
| Inventory | Medium–Low | Concentration only; no movement/ABC |
| Purchasing | Medium–High | Backlog signals strong; budget absent |

**No business area is completely unrepresented**, but **Inventory** and **Sales** lack the deepest exception signals management typically expects (slow stock, collection-linked sales accountability, pipeline).

---

## 6. Exception-Based Management Analysis

Focus: situations that **deserve management attention** rather than totals. Threshold values are **not chosen here** — only candidates and business meaning.

### 6.1 Warning condition candidates

| ID | Condition candidate | Business meaning | Data source | Computed today? |
| -- | ------------------- | ---------------- | ----------- | --------------- |
| W-S01 | Achievement % below plan | Sales may miss monthly target | Sales snapshot KPI | Yes — value available; threshold TBD |
| W-S02 | Latest week omzet below prior week | Billing deceleration | Sales week trend rows | Yes — requires comparison logic (new) |
| W-S03 | Achievement % blank with targets expected | Data or configuration gap | Sales snapshot | Yes — detectable |
| W-P01 | Overdue Customer > 0 | At least one customer past due | Piutang KPI | Yes |
| W-P02 | > 90 Days aging amount > 0 | Severely stale receivables exist | Piutang aging buckets | Yes |
| W-P03 | Top 1 customer > X% of Total Piutang | Customer concentration risk | Piutang top customers + total | Partial — requires ratio (new) |
| W-I01 | Top 1 category > X% of inventory value | Category concentration | Inventory breakdown | Partial — requires ratio (new) |
| W-I02 | Top 1 supplier > X% of inventory value | Principal stock dependency | Inventory breakdown | Partial — requires ratio (new) |
| W-U01 | Pending Posting Invoice Count > 0 | Purchase receipts pending | Purchasing KPI | Yes |
| W-U02 | BELUM purchase value > 0 | Monetary exposure in unposted purchases | Purchasing posting status | Yes |
| W-U03 | Top 1 principal > X% of monthly purchase | Supplier spend concentration | Purchasing top principal | Partial — requires ratio (new) |
| W-X01 | Any snapshot domain `Failed` or stale beyond 2× interval | Analytics unreliable | Health endpoint + GeneratedAt | Yes |
| W-X02 | `HasUnavailableDomain` on overview | Dashboard incomplete | Overview API | Yes |

### 6.2 Critical condition candidates

| ID | Condition candidate | Business meaning | Data source | Computed today? |
| -- | ------------------- | ---------------- | ----------- | --------------- |
| C-S01 | Achievement % critically low (TBD) late in month | Target miss likely | Sales KPI + calendar day | Partial — needs threshold + day-of-month rule |
| C-P01 | > 90 Days amount exceeds TBD % of Total Piutang | Collection crisis | Piutang aging | Partial — ratio (new) |
| C-P02 | Single customer balance exceeds TBD absolute or % | Major default exposure | Piutang top customers | Partial |
| C-U01 | Pending Posting Count exceeds TBD | Warehouse/posting breakdown | Purchasing KPI | Partial — count yes, threshold TBD |
| C-X01 | Health status `degraded` | System not refreshing analytics | Health endpoint | Yes |

### 6.3 Threshold candidates (business meaning only — values TBD by PO)

| Threshold topic | Why management cares | Available numerator/denominator |
| ----------------- | -------------------- | ------------------------------- |
| Minimum acceptable Achievement % | Board sales expectation | Achievement % |
| "Late month" calendar cutoff | Urgency increases near month-end | System date + Achievement % |
| Overdue Customer count limit | Collection team capacity vs exposure | Overdue Customer |
| > 90 Days as % of Total Piutang | Quality of receivable book | Aging bucket / Total Piutang |
| Top customer as % of piutang | Concentration policy | Top customer balance / Total Piutang |
| Top category/supplier % of inventory | Diversification policy | Breakdown / Total Inventory Value |
| Pending posting count or BELUM value | Operational SLA for goods receipt | Purchasing KPI / posting breakdown |
| Top principal % of purchase spend | Supplier diversification | Top principal / Grand Total Purchase |
| Snapshot staleness minutes | Trust in numbers | Now − GeneratedAt vs interval |

### 6.4 Ranking candidates

| Ranking | Already in system | Executive use |
| ------- | ----------------- | --------------- |
| Top N Outstanding Customers | Top 10 (Piutang snapshot) | Collection priority list |
| Top N Salesman by omzet | Top 10 (Sales snapshot) | Performance recognition |
| Bottom N Salesman | **Not available** | Intervention list — would need new calculation |
| Top N Categories by inventory value | Top 10 (Inventory snapshot) | Capital review priority |
| Top N Suppliers by inventory value | Top 10 (Inventory snapshot) | Principal dependency |
| Top N Principals by purchase amount | Top 10 (Purchasing snapshot) | Spend concentration |

### 6.5 Trend candidates

| Trend | Already in system | Attention interpretation (TBD) |
| ----- | ----------------- | ------------------------------ |
| Weekly invoiced sales | Sales week trend | Acceleration / deceleration vs prior week |
| Weekly purchase amount | Purchasing week trend | Unusual spend spikes |
| Total Piutang over time | **Not retained** (CURRENT snapshot only) | Rising exposure — **requires historical snapshots** |
| Aging bucket shift | **Not retained** | Deteriorating collection — **requires history** |
| Achievement % MTD pace | **Not computed** | Projected month-end achievement — **requires projection logic** |

---

## 7. Existing Asset Discovery

Maximize reuse — avoid new business calculations when equivalent logic exists.

### 7.1 Portal snapshot layer (primary reuse for executive KPIs)

| Asset | Path / location | Reusable for M16 |
| ----- | --------------- | ---------------- |
| Overview reader | `DashboardOverviewDal` | Headline KPIs across four domains |
| Sales aggregator | `DashboardSalesFakturAggregator` | Target, achievement, %, weekly trend, top salesman |
| Piutang aggregator | `DashboardPiutangAggregator` | Aging buckets, overdue count, top customers |
| Inventory aggregator | `DashboardInventoryAggregator` | Totals, category/supplier breakdown, top 10 |
| Purchasing aggregator | `DashboardPurchasingInvoiceAggregator` | Totals, pending count, posting split, top principal, weekly trend |
| Snapshot workers | `RefreshDashboard*SnapshotWorker` | Refresh cadence unchanged |
| Achievement policy | `SalesOmzetChartAchievementPolicy` | Achievement % calculation |
| Week grouper | `SalesOmzetChartWeekGrouper` | Weekly buckets (Sales + Purchasing) |
| Health resolver | `DashboardSnapshotHealthStatusResolver`, `HealthController` | Platform attention signal |

### 7.2 Snapshot tables (Layer A + B)

| Table | Content |
| ----- | ------- |
| `BTR_PortalDashboardSalesKpi` | TotalOmzet, TotalFaktur, TotalCustomer, TotalTarget, TotalAchievement, AchievementPercent |
| `BTR_PortalDashboardSalesWeekTrend` | Weekly invoiced amounts |
| `BTR_PortalDashboardSalesTopSalesman` | Top 10 ranking |
| `BTR_PortalDashboardPiutangKpi` | TotalPiutang, TotalCustomer, OverdueCustomer |
| `BTR_PortalDashboardPiutangAging` | Five bucket amounts |
| `BTR_PortalDashboardPiutangTopCustomer` | Top 10 customers |
| `BTR_PortalDashboardInventoryKpi` | TotalInventoryValue, TotalItem |
| `BTR_PortalDashboardInventoryBreakdown` | Category + supplier rollups |
| `BTR_PortalDashboardPurchasingKpi` | GrandTotalPurchase, TotalInvoice, PendingPostingInvoiceCount |
| `BTR_PortalDashboardPurchasingWeekTrend` | Weekly purchase amounts |
| `BTR_PortalDashboardPurchasingPostingStatus` | SUDAH / BELUM amounts |
| `BTR_PortalDashboardPurchasingTopPrincipal` | Top 10 principals |
| `BTR_PortalDashboardRefreshLog` | Per-domain refresh status |

**Implication:** An Executive Dashboard can be served primarily by **reading existing snapshot tables** (possibly via a new composer/orchestrator) without new SQL against transactional tables.

### 7.3 Portal report layer (drill-down validation)

| Asset | Path | Reuse |
| ----- | ---- | ----- |
| Sales report | `SalesReportDal`, `GetSalesReportQuery`, `SalesReportView.vue` | Faktur detail |
| Piutang report | `PiutangReportDal`, `GetPiutangReportQuery`, `PiutangReportView.vue` | Collection detail |
| Inventory report | `InventoryReportDal`, `GetInventoryReportQuery`, `InventoryReportView.vue` | Stock detail |
| Purchasing report | `PurchasingReportDal`, `GetPurchasingReportQuery`, `PurchasingReportView.vue` | Invoice/posting detail |
| Period validation | `ReportPeriodValidator`, `ReportPeriodRequest` | Consistent 31-day filter rules |

### 7.4 Desktop DALs and policies (not yet in portal — reuse candidates)

| Asset | Path | Potential executive signal |
| ----- | ---- | -------------------------- |
| Piutang open balance source | `IPiutangSalesWilayahDal`, `PiutangOpenBalanceDal` | Wilayah/salesman dimensions |
| Faktur list | `IFakturViewDal` / `FakturViewDal` | Kembali status counts, customer concentration |
| Sales omzet health | `SalesOmzetHealthPolicy`, `SalesOmzetReportHealthResolver`, `ISalesOmzetHealthWeeklyDal` | Data quality Good/Warning/Poor |
| Sales omzet chart builder | `SalesOmzetChartSummaryBuilder` | Per-rep achievement (Desktop semantics) |
| Target DAL | `ISalesOmzetTargetDal.SumTargetAmountForMonth` | Already used in sales snapshot |
| Stock balance health | `StokBalanceHealthDal` | Inventory integrity mismatches |
| Piutang tracker | `IPiutangTrackerDal` | Lifecycle anomalies |
| Invoice view | `IInvoiceViewDal` | Same source as purchasing snapshots |
| Stock balance view | `IStokBalanceViewDal` | Same source as inventory snapshots |
| Effective call | `IEffectiveCallDal` | Field sales activity (Desktop) |
| Stok periodik | `IStokPeriodikDal` | Historical stock positions (Desktop) |

### 7.5 Frontend components (composition reuse)

| Component | Path | Executive use |
| --------- | ---- | ------------- |
| `KpiCard` | `components/KpiCard.vue` | Attention cards |
| `DashboardDetailLayout` | `components/dashboard/DashboardDetailLayout.vue` | Page shell pattern |
| `Top10RankingTable` | `components/dashboard/Top10RankingTable.vue` | Exposure lists (Top 5) |
| `Message` (PrimeVue) | Used in `DashboardHomeView` | Staleness banner, unavailable data |

### 7.6 Business rules (authoritative — do not reimplement)

From `btr-portal-domain.md`: open balance threshold (`KurangBayar > 1`), void exclusion, In-Transit exclusion, BrgId-first grouping, customer key resolution, Top N = 10, aging bucket boundaries, posting status values.

---

## 8. Dashboard Layout — Approved (Proposal A)

**Product Owner decision:** Use **Proposal A — "Attention First"**. This is the only layout that fully supports the V2 philosophy: *What requires management attention?* rather than aggregating existing dashboard metrics onto one page.

### 8.1 Approved page structure

Section order is fixed:

```text
1. Attention Cards
2. Critical Exposure Lists
3. Domain Summaries
4. Navigation to Details
```

### 8.2 Approved wireframe

```text
+====================================================================+
|  MANAGEMENT ATTENTION CENTER                  [Refresh]            |
|  What requires management attention today?                           |
|  Last Refreshed: 2026-06-08 08:15                                   |
+====================================================================+
|  STALENESS BANNER (when applicable)                                 |
|  ⚠ Dashboard Data Not Fresh                                         |
+====================================================================+
|  1. ATTENTION CARDS                                                  |
|  +----------------+ +----------------+ +----------------+          |
|  | SALES          | | PIUTANG        | | PURCHASING     |          |
|  | Ach: 72%       | | Overdue: 48    | | Pending Posting|          |
|  | Rp xxx value   | | cust           | | 7 inv / Rp xxx |          |
|  +----------------+ +----------------+ +----------------+          |
|  +----------------+ +----------------+                              |
|  | INVENTORY      | | SYSTEM         |                              |
|  | Top Category   | | Snapshot OK    |                              |
|  | 42% of value   | | (or stale)     |                              |
|  +----------------+ +----------------+                              |
+====================================================================+
|  2. CRITICAL EXPOSURE LISTS (Top 5 each — grouped by domain)        |
|  Top 5 Customers | Top 5 Categories | Top 5 Suppliers | Top 5 Principals |
|  (separate tables — not mixed)                                       |
+====================================================================+
|  3. DOMAIN SUMMARIES (compact — link to domain dashboard)           |
|  Sales → Achievement % + value | Piutang → total, >90d amt & %     |
|  Purchasing → pending posting      | Inventory → value + top cat %  |
+====================================================================+
|  4. NAVIGATION TO DETAILS                                            |
|  Sales Dashboard → | Piutang Dashboard → | ... (domain dashboards)  |
+====================================================================+
```

**Excluded from executive page:** weekly trend charts (remain on domain dashboards); operational volume totals (Total Faktur, Total Customer, Total Item, Total Invoice).

**Achievement % attention indicator (Sales card only):**

| Achievement % | Indicator label |
| ------------- | --------------- |
| ≥ 100% | Healthy |
| 80–99% | Warning |
| < 80% | Critical |

No day-of-month logic. No generic Info/Warning/Critical severity engine elsewhere — use neutral **Attention Indicator** presentation except where Achievement % bands apply.

### 8.3 Navigation (approved)

```text
Login → /dashboard (Management Attention Center — replaces current home)

Sidebar:
  Dashboard
   ├─ Executive (default) → /dashboard
   ├─ Sales               → /dashboard/sales
   ├─ Piutang             → /dashboard/piutang
   ├─ Inventory           → /dashboard/inventory
   └─ Purchasing          → /dashboard/purchasing

Drill-down path:
  Executive → Domain Dashboard → Report
```

- **All authenticated users** see the Executive Dashboard — no role-based routing in M16.
- Executive cards and domain summaries link to **domain dashboards first** (not directly to reports).
- Chart-to-transaction drilldown: **not in M16** (platform deferral unchanged).

Domain detail dashboards remain the **analytical depth** layer; Executive Dashboard is the **prioritization** layer.

### 8.4 Proposals not selected (reference only)

| Proposal | Status |
| -------- | ------ |
| B — Balanced Executive Summary | Not selected — too total-heavy |
| C — Domain Gateways | Not selected — insufficient attention prioritization |

---

## 9. Gap Analysis

### 9.1 Information already available (ready for executive composition)

| Information | Source |
| ----------- | ------ |
| Sales omzet, faktur count, customer count | Sales snapshot KPI |
| Total target, achievement, achievement % | Sales snapshot KPI |
| Weekly invoiced sales trend | Sales week trend table |
| Top 10 salesman ranking | Sales top salesman table |
| Total piutang, customer count, overdue customer count | Piutang snapshot KPI |
| Five aging bucket amounts | Piutang aging table |
| Top 10 outstanding customers | Piutang top customer table |
| Total inventory value and item count | Inventory snapshot KPI |
| Category and supplier value breakdown + top 10 | Inventory breakdown table |
| Grand total purchase, invoice count, pending posting count | Purchasing snapshot KPI |
| Posting status amounts (SUDAH/BELUM) | Purchasing posting status table |
| Top 10 principals by purchase | Purchasing top principal table |
| Per-domain GeneratedAt timestamps | All snapshot KPI tables |
| Snapshot refresh health (ok/degraded/refreshing) | Health endpoint + refresh log |
| Transaction detail for validation | All four reports |

### 9.2 Information partially available (M16 will derive)

| Information | What exists | M16 treatment |
| ----------- | ----------- | ------------- |
| **Concentration ratios** (Top Customer/Category/Supplier/Principal %) | Numerator and denominator in snapshots | **In scope** — new presentation logic from existing snapshot data |
| **BELUM monetary exposure as headline** | BELUM amount in posting breakdown | **In scope** — promote to Attention Card and domain summary |
| **> 90 Days as % of Total Piutang** | Aging bucket + total | **In scope** — show amount and percentage; no color threshold |
| **Consolidated Last Refreshed timestamp** | Per-domain GeneratedAt | **In scope** — single timestamp for executives |
| **Snapshot staleness banner** | Health endpoint + intervals | **In scope** — "⚠ Dashboard Data Not Fresh" when stale |
| Piutang report ↔ dashboard alignment | Both use same DAL rules | Report link uses current month default; dashboard stays all-time open |

### 9.3 Information not in M16 scope (deferred)

| Information | Disposition |
| ----------- | ----------- |
| Collection effectiveness / payment rate | Future **Collection Dashboard** milestone |
| Slow-moving / dead stock / ABC inventory | Future **M17** (or similar) Slow Moving & Dead Stock Dashboard |
| Weekly trend on executive page | Remains on domain dashboards only |
| Faktur Kembali backlog aggregate | Sales operational dashboard/report only |
| Sales Omzet Health (Desktop) | IT/Operations — not executive |
| Sales pipeline / outstanding order omzet | Remain Faktur-based — excluded |
| Retur Beli analytics | Out of scope |
| Prior month / quarter comparison | Future milestone |
| Piutang trend over time | Future milestone — requires historical snapshots |
| Cross-domain mixed Top Risks table | Rejected — keep lists grouped by domain |
| Generic severity engine (Info/Warning/Critical) | Rejected — Achievement % bands only; else Attention Indicator |
| Role-based executive routing | Not in M16 |
| Chart-to-transaction drilldown | Platform deferral unchanged |
| Operational volume totals on executive view | Explicitly excluded (Total Faktur, Total Customer, Total Item, Total Invoice) |

---

## 10. Final Product Decisions (Authoritative)

**Status:** All open questions resolved by Product Owner — 2026-06-08.  
**Audience:** Architect — use this section as scope input. Do not re-decide business rules listed here.

### 10.1 Product positioning and audience

| # | Decision |
| - | -------- |
| Q1 | **Replace `/dashboard`** with Executive Dashboard (Management Attention Center). Domain dashboards unchanged. Sidebar: Dashboard → Executive (default), Sales, Piutang, Inventory, Purchasing. |
| Q2 | **No management-only route.** All authenticated users see Executive Dashboard. No role-based routing in M16. |
| Q3 | **Daily morning review** cadence. Design for *What requires management attention today?* — not weekly board reporting. |
| Q4 | **Explicit attention wording.** Page title/framing: **Management Attention Center** — not generic "summary". |

### 10.2 Attention rules and indicators

| # | Decision |
| - | -------- |
| Q5 | **Achievement % bands (Sales only):** ≥ 100% Healthy · 80–99% Warning · < 80% Critical. No day-of-month logic in M16. |
| Q6 | **Overdue customers:** any count > 0 is attention-worthy. Show count. No acceptable threshold in M16. |
| Q7 | **> 90 Days aging:** show **amount** and **percentage of Total Piutang**. No red/yellow threshold — management decides visually. |
| Q8 | **Pending posting:** show **count** and **value**. No threshold. |
| Q9 | **Concentration ratios in scope:** Top Customer % · Top Category % · Top Supplier % · Top Principal %. |
| Q10 | **Snapshot staleness:** show banner **"⚠ Dashboard Data Not Fresh"** when stale. |
| Q11 | **No generic severity engine** (Info/Warning/Critical). Use **Attention Indicator** presentation except Achievement % bands above. |

### 10.3 Promoted metrics (executive view)

| Domain | Promote on executive view | Explicitly exclude |
| ------ | ------------------------- | ------------------ |
| **Sales** | Achievement %, Achievement Value (`Total Achievement`) | Total Faktur, Total Customer, weekly trend |
| **Piutang** | Total Piutang, Overdue Customer, > 90 Day Amount (+ % of total) | — |
| **Inventory** | Total Inventory Value, Top Category % | Total Item |
| **Purchasing** | Pending Posting Count, Pending Posting Value (BELUM) | Total Invoice |
| **System** | Snapshot Health, consolidated Last Refreshed | Per-domain timestamps on executive page |

**Concentration ratios (Section 2 exposure lists / summaries):** Top Customer % · Top Category % · Top Supplier % · Top Principal %.

**Top N for exposure lists:** **Top 5**.

**Weekly trend:** **No** on executive page — domain dashboards only.

**Operational totals removed from executive:** Total Faktur, Total Customer, Total Item, Total Invoice.

### 10.4 Piutang semantics

| # | Decision |
| - | -------- |
| Q16 | **All-time open balance** — consistent with Piutang Dashboard (`KurangBayar > 1`, full open snapshot). |
| Q17 | Report drill-down: open Piutang Report with **current month** default. No special pre-filtering beyond standard report default. |

### 10.5 Inventory and sales — out of M16 scope

| # | Decision |
| - | -------- |
| Q18 | **Slow-moving / dead stock:** No. Separate milestone (e.g. M17 Slow Moving & Dead Stock Dashboard). |
| Q19 | **Faktur Kembali backlog:** No — operational Sales dashboard/report only. |
| Q20 | **Sales Omzet Health:** No — IT/Operations, not management. |
| Q21 | **Pipeline omzet:** No — remain Faktur-based. |

### 10.6 Purchasing and finance — out of M16 scope

| # | Decision |
| - | -------- |
| Q22 | **BELUM value:** Yes — headline executive signal. Format example: `Pending Posting · 7 invoices · Rp xxx`. |
| Q23 | **Retur Beli:** No. |
| Q24 | **Collection effectiveness:** No — future Collection Dashboard milestone. |

### 10.7 Navigation and drill-down

| # | Decision |
| - | -------- |
| Q25 | Link to **domain dashboard first**; path Executive → Domain Dashboard → Report. |
| Q26 | **No chart drilldown** in M16. |
| Q27 | **No mixed-domain Top Risks table.** Separate grouped lists: Top 5 Customers, Top 5 Categories, Top 5 Suppliers, Top 5 Principals. |

### 10.8 Historical analytics — out of M16 scope

| # | Decision |
| - | -------- |
| Q28 | **No prior month/quarter comparison** — future milestone. |
| Q29 | **No piutang trend over time** — future milestone. |

### 10.9 Platform and freshness

| # | Decision |
| - | -------- |
| Q30 | **Keep existing refresh cadence** (Piutang 15m, Sales/Purchasing 30m, Inventory 60m). No change. |
| Q31 | **Single consolidated timestamp:** `Last Refreshed: YYYY-MM-DD HH:mm` — not per-domain timestamps on executive page. |

### 10.10 Approved layout

| # | Decision |
| - | -------- |
| Layout | **Proposal A — Attention First.** Fixed section order: (1) Attention Cards → (2) Critical Exposure Lists → (3) Domain Summaries → (4) Navigation to Details. |

### 10.11 Future milestones (PO direction — not M16 scope)

| Topic | Suggested milestone |
| ----- | ------------------- |
| Slow moving & dead stock | M17 (or similar) |
| Collection effectiveness | Collection Dashboard |
| Prior period comparison | TBD |
| Piutang historical trend | TBD |

---

## Appendix A — Current vs V2 Dashboard Philosophy

| Aspect | V1 (M1–M15) | V2 (M16 — approved) |
| ------ | ----------- | ------------------- |
| Primary question | What happened? | What requires management attention today? |
| Home page (`/dashboard`) | Four equal operational summary cards | **Management Attention Center** (Proposal A layout) |
| Page title | "Dashboard" / operational summary | **Management Attention Center** |
| Metric mix | Totals-heavy (Faktur, Customer, Item, Invoice counts) | Exception-heavy; operational volume totals removed |
| User path | Home → pick domain → maybe report | Executive → domain dashboard → report |
| Audience | All users (same nav) | All users (same nav — no role split in M16) |
| Review cadence | Ad hoc | Daily morning review |
| Depth | Domain dashboards | Unchanged — weekly trends and full Top 10 remain there |

---

## Appendix B — File and Endpoint Index (discovery reference)

| Category | Location |
| -------- | -------- |
| Domain definitions | `docs/features/btr-portal/btr-portal-domain.md` |
| User workflows | `docs/features/btr-portal/btr-portal-operational.md` |
| Architecture | `docs/features/btr-portal/btr-portal-architecture.md` |
| Snapshot domain rules | `docs/features/materialized-dashboard/materialized-dashboard-domain.md` |
| Overview API | `GET /api/dashboard/overview` → `DashboardOverviewDal` |
| Sales dashboard API | `GET /api/dashboard/sales` |
| Piutang dashboard API | `GET /api/dashboard/piutang` |
| Inventory dashboard API | `GET /api/dashboard/inventory` |
| Purchasing dashboard API | `GET /api/dashboard/purchasing` |
| Health | `GET /api/health/dashboard-snapshots` |
| Report APIs | `GET /api/reports/{sales|piutang|inventory|purchasing}` |
| Frontend home (to be replaced) | `btr.portal.web/src/views/dashboard/DashboardHomeView.vue` |
| Detail views | `SalesDashboardView.vue`, `PiutangDashboardView.vue`, `InventoryDashboardView.vue`, `PurchasingDashboardView.vue` |

---

## Appendix C — Handoff to Architect

**Product scope is approved** (Section 10). Proceed with implementation planning.

The Architect should:

1. **Replace `/dashboard`** with Management Attention Center — refactor `DashboardHomeView.vue` (or equivalent) per Section 8 wireframe.
2. **Extend overview/composer API** (or equivalent) to return executive DTO: promoted KPIs, Top 5 exposure lists, concentration ratios, Achievement % band, consolidated `LastRefreshed`, staleness flag — **reading existing snapshot tables** where possible.
3. **Implement new presentation logic only:** concentration ratios (Top 1 / Top 5 as % of domain total), > 90 Days % of Total Piutang, consolidated freshness, staleness detection vs domain intervals, Achievement % Healthy/Warning/Critical band.
4. **Sidebar:** add Dashboard → Executive (default at `/dashboard`); retain Sales, Piutang, Inventory, Purchasing sub-items.
5. **Navigation links:** executive sections → domain dashboards; do not link executive cards directly to reports.
6. **Truncate Top 10 snapshot data to Top 5** at presentation layer (or read top 5 only).
7. **Do not scope:** slow-moving stock, collection effectiveness, pipeline omzet, Retur Beli, Sales Omzet Health, Faktur Kembali aggregate, weekly trends on executive page, mixed-domain risk table, generic severity engine, role-based routing, chart drilldown, refresh cadence changes.

**Explicitly out of scope for this document:** API contracts, database schema changes, implementation tasks, and delivery estimates.
