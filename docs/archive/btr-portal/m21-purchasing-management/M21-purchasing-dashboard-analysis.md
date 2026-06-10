# BTR Portal Analysis — M21 Purchasing Dashboard

**Status:** Product scope approved — implementation plan delivered.  
**Implementation plan:** [M21 Purchasing Dashboard - Plan.md](./M21%20Purchasing%20Dashboard%20-%20Plan.md)  
**Scope:** Business analysis only. No implementation plans, API design, database design, or code.  
**Date:** 2026-06-09 (analysis) · Product Owner decisions recorded 2026-06-09  
**Context:** BTR Portal V2 (M16–M20 complete) follows a management philosophy: *What requires management attention?* M21 evolves the **Purchasing Dashboard** from purchasing statistics toward **purchasing management and purchasing risk** — answering: *Which purchasing activities require management attention and why?*

**Approved roadmap position:** M17 Customer Analytics → M18 Salesman Performance → M19 Slow Moving & Dead Stock → M20 Collection Dashboard → **M21 Purchasing Dashboard (management attention)** → … → M25 Sales Force Effectiveness

**Relationship to delivered V1:** A basic **Purchasing Dashboard** (`/dashboard/purchasing`) and **Purchasing Report** (`/reports/purchasing`) already exist from the pre-M16 purchasing initiative. M21 does **not** repeat V1 feasibility analysis. It discovers what **additional management-attention capabilities** BTR can support using existing business knowledge, and how M21 should differentiate from statistics-only views.

**Reference documents:** `docs/foundation/PRODUCT.md`, `docs/foundation/DOMAIN.md`, `docs/foundation/LANDSCAPE.md`, `docs/foundation/WORKFLOW.md`, `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-operational.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md`, `docs/work/btr-portal/M16 Executive Dashboard - Analysis.md`, `docs/work/btr-portal/M17-Customer-Analytics-Analysis.md`, `docs/work/btr-portal/M18 Salesman Performance - Analysis.md`, `docs/work/btr-portal/M19 Slow Moving & Dead Stock - Analysis.md`, `docs/work/btr-portal/M20-collection-dashboard-analysist.md`, `docs/archive/purchasing-dashboard/analysis-report.md`, `docs/archive/btr-portal-api-scaffolding/portal-analysis-m10-m12-final.md`, `btr-reporting-investigation.md`

---

## 1. Executive Summary

BTR Portal today exposes **purchasing activity statistics** well — monthly spend (`Grand Total Purchase`), invoice volume, weekly pace, posting-status composition, and Top 10 Principal ranking. The **Management Attention Center** (M16) already promotes **Pending Posting count/value** and **Top Principal %** as purchasing attention signals. However, the detail **Purchasing Dashboard** remains primarily a **volume and composition** view. It does not systematically answer:

> Which purchasing activities require management attention and why?

**Critical discovery — Posting Stok (`BELUM`):** BTR Desktop **intentionally** saves every purchase Invoice with `IsStokPosted = false` (`SaveInvoiceWorker` sets `.IsPosted(false)` on every save). Stock posting is a **separate deliberate step** via **PT2 Posting Stok** (`PostingStokForm`), which sets `IsStokPosted = true` and runs `GenStokInvoiceWorker` to add FIFO stock. Therefore `PostingStok = BELUM` is the **default state after invoice entry** and may represent an **in-progress draft**, not a receiving failure or management backlog. Treating all `BELUM` invoices as operational risk would **over-alert** management.

**Critical discovery — no Purchase Order entity:** BTR has no separate PO, approval workflow, or budget master. Purchasing transactions are **`BTR_Invoice`** records. Management attention must be derived from **invoice lifecycle**, **supplier/principal dimensions**, and **cross-domain inventory signals** — not from PO compliance metrics that do not exist.

**Critical discovery — supplier analytics span three domains:** Supplier/principal dependency appears in **Purchasing** (monthly spend concentration), **Inventory** (stock value concentration), and **Inventory Risk** (at-risk stock by supplier). M21 should **compose** these existing signals rather than invent parallel supplier calculations.

### Key findings

| Finding | Implication |
| ------- | ----------- |
| **V1 purchasing KPIs are statistics**, not exception management | M21 must add **attention cards**, **attention list**, and **risk-framed rankings** — following M17/M18/M19/M20 patterns |
| **`BELUM` is staged workflow, not inherently backlog** | Pending posting KPIs require **qualification** (age, completeness, value) before becoming management attention signals |
| **`CreateTime` / `LastUpdate` exist on `BTR_Invoice` but not in `InvoiceViewDal`** | Duration-of-`BELUM` analysis is **partially available** at database level; **not exposed** in portal DAL today |
| **No purchasing approval process** discovered in Desktop | Exclude approval-pending, PO-mismatch, and authorization KPIs |
| **Retur Beli (PT3/PF4) is a separate transaction** from purchase Invoice | **Excluded from M21 V1** — remains operational Desktop reporting |
| **Desktop has no purchasing chart** (unlike `SalesOmzetChartForm`) | M21 defines **new management analytics**; PF1 grid is the operational reference, not a chart to mirror |
| **Inventory Risk (M19) owns idle-stock logic** | M21 **cross-links** to M19; does not compute "buying into slow/dead stock" |
| **Executive dashboard treats all `BELUM` as attention** | **Revised** — executive `RequiresAttention` must use qualified backlog, not raw `BELUM` count |

### Approved product outcome

**All open questions resolved.** See Section 13 for authoritative Product Owner decisions.

Deliver **Purchasing Management Dashboard** at `/dashboard/purchasing` (page title: **Purchasing Management Dashboard**). **Extend** the existing V1 Purchasing Dashboard — do not replace traceability sections. Add management-attention layers using **Proposal A (Purchasing Attention First)** layout.

**Mandatory section priority:**

1. Purchasing Attention Cards  
2. Supplier Dependency Analysis (cross-domain panels)  
3. Compound Dependency Signals  
4. Purchasing Attention List (Principal × Signal)  
5. Purchasing Trend Context (weekly trend, posting status — visual-only pace)  
6. Cross-links to Inventory Dashboard and Inventory Risk Dashboard  
7. Purchasing Report validation (M17 drill-down pattern)

**Dedicated snapshot domain:** `BTRPD_PurchasingManagement*` — separate from existing `BTRPD_Purchasing*` (V1 statistics). Extend read path with `CreateTime`/`LastUpdate` for age-qualified `BELUM` analysis.

**Approved attention signals:** `QualifiedBacklog`, `PrincipalSpendConcentration`, `PrincipalInventoryConcentration`, `PrincipalAtRiskExposure`, `CompoundDependency`, `PurchasingInactivity`, `UnknownPrincipal`. Plus principal-level **High Inventory Exposure + Zero MTD Purchase** (Q13).

**Executive promotion:** Selected M21 signals promoted to Management Attention Center **after M21 stabilizes** — not in initial delivery scope.

**Explicitly out of M21 V1:** Retur Beli analytics, PF2 line-level aggregates, purchase-to-sales ratio, automated weekly spike/deceleration flags, "buying into slow/dead stock" calculation, Purchasing Line Report route, salesman performance (M18), collection (M20), route effectiveness (M25), budget vs actual, PO workflows, forecasting models, transactional write capability.

---

## 2. Management Attention Discovery

This section identifies purchasing-related situations that typically require management intervention, mapped to **existing BTR capabilities**. Items marked **Portal today** are in current portal snapshots or reports. **Desktop only** exists in BTR Desktop but is not portal-exposed. **Not available** means no implemented logic was discovered. **Derivable** means source data exists but no aggregate KPI is computed.

### 2.1 Posting and receiving workflow

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Purchase invoice saved but stock not posted** | Goods may not be in sellable inventory | `PostingStok = BELUM`; `Pending Posting Invoice Count` | **Portal today** — count and value in posting breakdown |
| **Long-running `BELUM` invoice (stale draft)** | Entry abandoned or posting delayed beyond normal staging | `BTR_Invoice.CreateTime`, `LastUpdate` — **not in `InvoiceViewDal`** | **Partial** — data in `InvoiceDal`; not portal-exposed |
| **Large-value `BELUM` invoice** | High capital in limbo if invoice is complete but unposted | Sum `GrandTotal` where `BELUM` | **Portal today** — posting breakdown amount; not ranked by invoice |
| **Posted invoice with inventory not reflecting purchase** | Data integrity / posting failure | `StokBalanceHealthDal` (Desktop admin) | **Desktop only** — not purchasing-specific |
| **Goods received (posted) but HPP updated without stock movement review** | Cost basis changed on save; posting adds stock | `SaveInvoiceWorker` updates HPP; `GenStokInvoiceWorker` adds stock on PT2 | **Workflow knowledge** — no aggregate KPI |

**Posting Stok investigation (special area):**

| Question | Finding |
| -------- | ------- |
| What does `BELUM` mean operationally? | `SaveInvoiceWorker` always sets `IsStokPosted = false` on save. `BELUM` = invoice saved, posting not yet executed via PT2. |
| Is posting separate from entry? | **Yes.** `PostingStokForm` (PT2) is a dedicated screen: select invoice → confirm → `IsPosted(true)` + `GenStokInvoiceWorker`. |
| Why separate steps? | Large supplier invoices may have hundreds of lines; users may save draft and complete posting later (per product direction). |
| Does management monitor `BELUM` today in portal? | **Yes, partially** — M16 Executive promotes Pending Posting count/value; V1 Purchasing Dashboard shows count + pie chart. |
| Is every `BELUM` a problem? | **No.** Default after save. Without age/completeness rules, count/value **overstates** actionable backlog. |
| Can stale `BELUM` be measured? | `CreateTime`/`LastUpdate` on `BTR_Invoice` — requires DAL extension or alternate read path. **Not in `IInvoiceViewDal` today.** |
| Does `BELUM` block sales? | Unposted stock is **not in FIFO inventory** until `GenStokInvoiceWorker` runs — sellable qty unaffected until posting. **Replenishment gap** if goods physically arrived but not posted. |

### 2.2 Supplier and principal dependency

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Monthly spend concentrated in one principal** | Supplier dependency — supply disruption risk | Top 10 Principal; executive `TopPrincipalPercent` | **Portal today** |
| **Inventory value concentrated in one supplier** | Stock dependency — different from monthly spend | Inventory Dashboard Top 10 Suppliers; executive Top Supplier % | **Portal today** (M15) |
| **At-risk inventory concentrated by supplier** | Capital in slow/dead/never-sold stock from one principal | M19 Supplier Risk Exposure | **Portal today** (M19) |
| **Principal with purchases but no recent sales of their stock** | Buying without demand signal | `InvoiceBrgViewDal` + `IBrgLastFakturDal` / M19 classification | **Derivable** — not computed |
| **Single principal dominates both purchase and inventory** | Compound dependency | Join purchasing Top Principal + inventory Top Supplier | **Derivable** — not computed |
| **Principal with zero purchases this month but large inventory** | Legacy stock / purchasing inactivity for active supplier | Inventory snapshot + empty purchasing month slice | **Derivable** |

### 2.3 Purchasing volume and pace

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Unusually high monthly purchasing spend** | Cash outflow spike; possible over-ordering | `Grand Total Purchase`; weekly trend chart | **Portal today** — visual; no anomaly flag |
| **Unusually low purchasing activity** | Replenishment gap; possible stockout risk ahead | `Total Invoice = 0` or low; weekly trend flat | **Partial** — detectable; no "inactivity" signal |
| **Weekly purchasing spike** | Mid-month bulk ordering | `BTRPD_PurchasingWeekTrend` | **Portal today** — visual only |
| **Purchasing deceleration late in month** | Pace may miss replenishment needs | Week trend comparison | **Derivable** — requires week-over-week comparison logic (new) |
| **Purchasing vs sales pace mismatch** | Buying faster/slower than billing | `Grand Total Purchase` vs Sales `Total Omzet` | **Derivable** — not computed; no causal rule |

### 2.4 Purchase returns (Retur Beli)

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **High purchase return volume** | Supplier quality, pricing dispute, or receiving error | `ReturBeliBrgViewDal` (PF4) | **Desktop only** |
| **Return concentration by principal** | Recurring issues with one supplier | PF4 grouped by `SupplierName` | **Desktop only** — not aggregated |
| **Return spike vs purchase volume** | Anomaly in supplier relationship | PF4 + PF1/PF2 same period | **Derivable** — not computed |
| **Retur Beli affecting inventory** | Stock removed via `GenStokReturBeliWorker` | PT3 `ReturBeliForm` | **Workflow exists** — no portal KPI |

**Assessment:** Retur Beli is primarily **operational reporting** today. It may become a **management attention signal** if PO defines return rate or concentration thresholds — data exists at PF4 grain.

### 2.5 Purchasing data quality and voids

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Voided purchase invoice after posting** | Stock/inventory reversal complexity | `VoidInvoiceWorker`; void exclusion in reports | **Partial** — voids excluded from portal; no void-rate KPI |
| **Invoice with blank supplier** | Master data gap | Blank → `"Unknown"` in aggregator | **Portal today** — visible in rankings |
| **Duplicate or abnormal invoice amounts** | Data entry error | Row-level in Purchasing Report | **Partial** — manual review only |

### 2.6 Cross-domain purchasing-inventory situations

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Heavy purchasing into slow-moving SKUs** | Capital accumulation without sell-through | PF2 line items + M19 item classification | **Derivable** |
| **Heavy purchasing into dead stock categories** | Replenishment against no demand | PF2 `Kategori` + M19 category risk | **Derivable** |
| **Pending posting while stockouts on purchased items** | Operational urgency — goods needed but not posted | PF2 items + inventory qty + `BELUM` | **Not computed** — complex join |
| **Supplier inventory exposure rising while purchases slow** | Aging stock from principal | M15 + M19 + purchasing month trend | **Derivable** |

### 2.7 Workflow-derived attention points

From `docs/foundation/WORKFLOW.md` (Purchase → Stock Receipt → Inventory Update) and Desktop menus:

| Workflow stage | When management cares | Portal support today |
| -------------- | --------------------- | -------------------- |
| **PT1 Invoice entry** | Large drafts, data errors, supplier selection | Purchasing Report row list only |
| **Save draft (`BELUM`)** | Normal staging — not necessarily attention | Counted in pending posting KPIs |
| **PT2 Posting Stok** | Completing receipt into inventory | `SUDAH`/`BELUM` column; no link to PT2 |
| **Inventory update (FIFO)** | Stock available for sale | Indirect — Inventory Dashboard |
| **PT3 Retur Beli** | Supplier relationship / quality | None in portal |
| **PF1–PF4 reporting** | Validation and drill-down | PF1 only in portal |

---

## 3. Existing Dashboard Reuse Analysis

### 3.1 Management Attention Center (`/dashboard`) — purchasing metrics today

| Metric | Source | Attention-oriented? | M21 relationship |
| ------ | ------ | ------------------- | ---------------- |
| Pending Posting Invoice Count | `DashboardPurchasingInvoiceAggregator` → executive | **Qualified** — flags when > 0 | **Reuse or refine** — PO must decide if unqualified count remains valid |
| Pending Posting Value (`BELUM` amount) | Posting status snapshot rows | **Qualified** — same caveat | **Reuse** with optional age filter |
| Top Principal % | `#1 principal / Grand Total Purchase` | **Yes** — concentration | **Reuse** — informational unless threshold added |
| `RequiresAttention` (Purchasing card) | `PendingPostingInvoiceCount > 0 \|\| belumValue > 0` | **Binary** — treats all `BELUM` as attention | **Candidate for revision** if M21 qualifies backlog |
| Critical Exposures — Top 5 Principals | Purchasing Top N snapshot | Concentration list | **Navigation** to M21 detail |

**Assessment:** Executive purchasing attention is **backlog-centric**. M21 should either **justify and operationalize** that framing (with age/value rules) or **broaden** to supplier dependency and cross-inventory risk.

### 3.2 Purchasing Dashboard (`/dashboard/purchasing`) — V1 delivered

| KPI / section | Management vs statistics | Reuse for M21 |
| ------------- | ------------------------ | ------------- |
| Grand Total Purchase | Statistics — monthly spend total | **Retain** — traceability to report footer |
| Total Invoice | Statistics — volume | **Retain** — traceability; demote as attention signal |
| Pending Posting Invoice Count | Operational exception (unqualified) | **Reframe** — see Section 2.1 |
| Weekly Purchase Trend | Pace monitoring | **Retain** — support spike/deceleration interpretation |
| Posting Status Breakdown | Exposure split | **Retain** — validate pending value |
| Top 10 Principal | Concentration ranking | **Retain** — add **% of total** column (executive already computes Top 1 %) |

**Key gap:** V1 answers *how much was purchased this month?* M21 should answer *what purchasing situations require management attention?*

### 3.3 Inventory Dashboard (`/dashboard/inventory`) — purchasing-relevant

| KPI / section | Purchasing relevance | M21 reuse |
| ------------- | -------------------- | --------- |
| Top 10 Suppliers (by inventory value) | **Supplier stock dependency** — complements monthly spend | **Cross-link** — "Inventory exposure by principal" card |
| Top Category % | Indirect — category buying patterns | Optional context |
| Supplier horizontal bar chart | Visual concentration | **Component reuse** (`InventoryHorizontalBarChart.vue`) |

**Distinction:** Inventory supplier ranking = **stock on hand**. Purchasing Top Principal = **monthly intake**. Same principal may rank differently — both are meaningful.

### 3.4 Inventory Risk Dashboard (`/dashboard/inventory-risk`) — M19

| KPI / section | Purchasing relevance | M21 reuse |
| ------------- | -------------------- | --------- |
| Supplier Risk Exposure (Top 10 by at-risk value) | **Purchasing into principals with idle stock** | **Cross-link** — replenishment concern signal |
| Attention List (item × signal) | SKU-level idle stock | **Drill context** — not duplicate in M21 |
| At-Risk Inventory % | Company-level capital at risk | Optional supporting KPI |

**M19 explicit note:** Pending posting backlog is **purchasing domain**, related but not M19 scope. M21 should **reference** M19 for inventory-health context.

### 3.5 Sales Dashboard — indirect purchasing context

| KPI | Purchasing relevance |
| --- | -------------------- |
| Weekly Invoiced Sales Trend | Demand pace vs purchasing pace |
| Total Omzet | Denominator for purchase/sales ratio (if PO wants) |

### 3.6 Customer / Salesman / Collection dashboards

**Out of M21 scope** per milestone brief. No reuse except shared UI patterns (Attention Cards, Attention List, `ExecutiveAttentionCard.vue`).

### 3.7 Aggregator and pattern reuse (authoritative)

| Component | Location | M21 reuse |
| --------- | -------- | --------- |
| `DashboardPurchasingInvoiceAggregator` | `btr.application` | **Extend** — add concentration %, qualified backlog, attention list inputs |
| `SalesOmzetChartWeekGrouper` | Weekly bucket logic | **Reuse** for trend and spike detection |
| `DashboardExecutiveComposer.ComposePurchasing` | Executive attention | **Align** if M21 changes backlog semantics |
| M17/M18/M20 Attention List grain (entity × signal) | Portal UX pattern | **Apply** — Principal × signal or Invoice × signal |
| `Top10RankingTable.vue`, `PostingStatusPieChart.vue`, `WeeklyTrendChart.vue` | Frontend | **Reuse** |
| `DashboardInventoryAggregator` supplier rollup | M15 | **Read-only cross-domain** — do not duplicate SQL |
| `DashboardInventoryRiskAggregator` supplier risk | M19 | **Read-only cross-domain** |

---

## 4. Existing Report Reuse Analysis

### 4.1 Portal reports

| Report | Route | Purchasing information | Drill-down role |
| ------ | ----- | ---------------------- | --------------- |
| **Purchasing Report** | `/reports/purchasing` | Invoice header: Supplier, Warehouse, Grand Total, Posting Stok; footer totals | **Primary validation** for V1 KPIs; filter/search `BELUM` |
| Sales Report | `/reports/sales` | Demand context | Cross-check sales pace |
| Inventory Report | `/reports/inventory` | Stock by item × warehouse | Validate supplier/category exposure |
| Piutang Report | — | None | Out of scope |

**Gap:** No portal report for **PF2 line detail**, **PF3 daily detail**, or **PF4 Retur Beli**.

### 4.2 Desktop purchasing reports (not in portal)

| Menu | Form | DAL | Grain | Management information |
| ---- | ---- | --- | ----- | ------------------------ |
| PF1 | `InvoiceInfoForm` | `IInvoiceViewDal` | Invoice header | Same as portal Purchasing Report; Excel export; user date range up to 3 months |
| PF2 | `InvoiceBrgInfoForm` | `IInvoiceBrgViewDal` | Invoice × item | Line-level purchase: Brg, Qty, Hpp, Kategori, Supplier — **replenishment validation** |
| PF3 | `InvoiceHarianDetilForm` | `IInvoiceHarianDetilDal` | Invoice × item (discount/PPN detail) | Financial/audit detail — less management-oriented |
| PF4 | `ReturBeliBrgInfoForm` | `IReturBeliBrgViewDal` | Retur × item | Purchase return volume by supplier/item |

### 4.3 Desktop purchasing transactions (operational, not reports)

| Menu | Form | Purpose |
| ---- | ---- | ------- |
| PT1 | `InvoiceForm` | Create/edit purchase Invoice |
| PT2 | `PostingStokForm` | Post stock for saved invoices |
| PT3 | `ReturBeliForm` | Purchase return entry |
| PM1 | `SupplierForm` | Supplier master maintenance |

### 4.4 Related Desktop inventory/supplier analytics

| Menu | Form | DAL | Relevance to M21 |
| ---- | ---- | --- | ---------------- |
| IF4 | `StokBrgSupplierForm` | `IStokBrgSupplierDal` | Stock by supplier with pricing — **inventory exposure detail** |
| RO2 (sales) | `OmzetSupplierInfoForm` | `OmzetSupplierViewDal` | Supplier **sales** calendar — demand signal, not purchase |
| RF2 | `ReturJualPerSupplierReportForm` | `ReturJualBrgViewDal` | Customer retur by supplier — quality/demand signal |

### 4.5 KPI-to-report traceability matrix

| KPI candidate | Primary source | Validating report / screen | Reconciliation | Match type |
| ------------- | -------------- | -------------------------- | -------------- | ---------- |
| Grand Total Purchase | `DashboardPurchasingInvoiceAggregator` | Purchasing Report footer | Same `IInvoiceViewDal`, same month, void exclusion | **Exact** |
| Total Invoice | Same | Purchasing Report footer | Count of rows | **Exact** |
| Pending Posting Invoice Count | Aggregator | Purchasing Report | Count `PostingStok = BELUM` | **Exact** (unqualified) |
| Pending Posting Value | Posting status snapshot | Purchasing Report | Sum `GrandTotal` where `BELUM` | **Exact** (unqualified) |
| Top 10 Principal | Aggregator | Purchasing Report | Group by Supplier, sum GrandTotal | **Derivable** |
| Top Principal % | Executive composer | Purchasing Report | Top 1 / footer total | **Derivable** |
| Weekly purchase amount | Week trend snapshot | Purchasing Report | Group by week of `InvoiceDate` | **Derivable** |
| Stale `BELUM` count (if defined) | **Requires `CreateTime` in read path** | PF1 / Purchasing Report + Desktop invoice browse | Per-invoice age | **Not available** in portal DAL |
| Line-level purchase by category | **PF2 only** | `InvoiceBrgInfoForm` | Sum by Kategori | **Desktop only** |
| Retur Beli volume | **PF4 only** | `ReturBeliBrgInfoForm` | Sum by period | **Desktop only** |
| Inventory supplier concentration | M15 snapshot | Inventory Report | Supplier rollup | **Exact** (inventory domain) |
| Supplier at-risk exposure | M19 snapshot | Inventory Risk + Inventory Report | M19 rules | **Exact** (inventory risk domain) |
| Posting completed (`SUDAH`) | Posting breakdown | Purchasing Report | Sum/count `SUDAH` | **Derivable** |

---

## 5. Purchasing Workflow Analysis

### 5.1 Authoritative workflow (Desktop)

BTR implements purchasing as **Invoice-centric** flow. There is **no** Purchase Order, **no** approval gate, and **no** portal write path.

```text
Supplier selection (PT1 / Supplier browser)
    ↓
Invoice entry — header + line items (PT1 InvoiceForm)
    ↓
Save Invoice → IsStokPosted = false (BELUM) — always on save
    ↓
[Optional: continue editing draft — still BELUM]
    ↓
Posting Stok (PT2 PostingStokForm) — separate session/step
    ↓
IsStokPosted = true (SUDAH) + GenStokInvoiceWorker
    ↓
FIFO stock added (INVOICE / INVOICE-BONUS mutasi types)
    ↓
HPP on items updated on save (SaveInvoiceWorker.UpdateHpp)
```

**Parallel path — Purchase Return:**

```text
Retur Beli entry (PT3 ReturBeliForm)
    ↓
SaveReturBeliWorker + GenStokReturBeliWorker
    ↓
Stock reduced; separate from Invoice void
```

### 5.2 Invoice entry details (PT1)

| Step | Desktop behavior | Portal visibility |
| ---- | ---------------- | ----------------- |
| Select Supplier | `SupplierBrowser` / `ISupplierDal` | Supplier name on report rows |
| Select Warehouse | `WarehouseBrowser` | Warehouse column on report (not in V1 dashboard) |
| Add line items | `BrgStokBrowser`, qty/price/discount | PF2 only (Desktop) |
| Save | `SaveInvoiceWorker` — **always `IsPosted(false)`** | Appears as `BELUM` |
| Void | `VoidInvoiceWorker` | Excluded from portal (void filter) |
| Print | RDLC printout | N/A |

**Implication:** Saving an invoice **never** posts stock. Posting is **always** a second step. This is **by design**, not an exception path.

### 5.3 Posting Stok details (PT2)

| Step | Desktop behavior |
| ---- | ---------------- |
| Search invoices by date | `IInvoiceDal.ListData(periode)` — includes posted and unposted |
| Visual cue | Posted rows highlighted (light blue) |
| Post action | Confirm dialog → `IsPosted(true)` + `GenStokInvoiceWorker` |
| Stock effect | Adds FIFO lots per line; bonus qty as `INVOICE-BONUS` |

**Operational ownership:** Warehouse / inventory staff — not a management approval workflow.

### 5.4 Supplier master (PM1)

`SupplierForm` maintains supplier records used across purchasing and item master (`BTR_Brg.SupplierId`). No purchasing analytics on supplier form itself.

### 5.5 Portal role (unchanged)

Portal **observes** invoice and posting status. Users validate on Purchasing Report and act in Desktop (PT1/PT2).

### 5.6 Proposed management workflow (M21)

```text
Sign in
    ↓
Management Attention Center — purchasing card (qualified signals TBD)
    ↓
Purchasing Management Dashboard — attention cards, list, concentrations
    ↓
Purchasing Report — validate specific invoices (search BELUM, supplier)
    ↓
[Optional] Inventory Risk / Inventory Dashboard — supplier stock context
    ↓
BTR Desktop PT1/PT2 — complete entry or posting
```

---

## 6. Purchasing Risk Analysis

Focus: risks **already measurable** within BTR (thresholds **not** defined here).

| Risk | Business meaning | Measurable today? | Primary source |
| ---- | ---------------- | ------------------- | -------------- |
| **Unposted purchase exposure** | Invoice value not yet in sellable stock | **Yes** — count and `BELUM` value | Purchasing snapshot |
| **Stale draft exposure** | `BELUM` invoices older than operational norm | **Partial** — `CreateTime` not in `InvoiceView` | `BTR_Invoice` via extended DAL |
| **Principal spend concentration** | Monthly intake dependent on few suppliers | **Yes** — Top 10 + Top 1 % | Purchasing snapshot |
| **Principal inventory concentration** | Stock value dependent on few suppliers | **Yes** | M15 Inventory snapshot |
| **Principal at-risk inventory** | Idle capital tied to one supplier's SKUs | **Yes** | M19 Inventory Risk snapshot |
| **Purchasing pace spike** | Abnormal weekly spend | **Partial** — weekly amounts exist; anomaly logic new | Week trend |
| **Purchasing inactivity** | No/int minimal invoices in period | **Partial** — `Total Invoice` low/zero | Purchasing KPI |
| **Purchase return spike** | Supplier quality or dispute pattern | **Partial** — PF4 data; not in portal | `ReturBeliBrgViewDal` |
| **Buying into non-moving SKUs** | Replenishment without demand | **Derivable** — PF2 + M19 | Cross-domain |
| **Budget overrun** | Spend vs plan | **No** — no budget master | N/A |
| **Supplier single-source item** | SKU available from only one principal | **Partial** — `BTR_Brg.SupplierId` | Item master |
| **Void after posting** | Inventory/cost integrity | **Partial** — void workflow exists | Desktop |

---

## 7. Supplier Analysis

### 7.1 Supplier indicators already available

| Indicator | Domain | Period | Formula (existing) | Management meaning |
| --------- | ------ | ------ | ------------------ | ------------------ |
| Top Principal by purchase amount | Purchasing | Current month | `SUM(GrandTotal)` per `SupplierName` | Who receives the most purchase spend |
| Top Principal % | Executive / Purchasing | Current month | Top 1 / Grand Total Purchase | Monthly spend concentration |
| Top Supplier by inventory value | Inventory | Point-in-time | BrgId-first rollup by supplier | Who holds the most stock value |
| Top Supplier % | Executive | Point-in-time | Top 1 / Total Inventory Value | Stock concentration |
| Supplier Risk Exposure | Inventory Risk | Point-in-time | Top 10 suppliers by at-risk value | Where idle capital sits by principal |
| Omzet by Supplier (sales) | Desktop RO2 | User period | `FakturItem` aggregated | **Demand** — how fast principal's products sell |
| Stock by Supplier (IF4) | Desktop | Point-in-time | `StokBrgSupplierDal` | Item-level stock detail per principal |

### 7.2 Meaningful management attention indicators (candidates)

| Indicator | Why management cares | Available? |
| --------- | -------------------- | ---------- |
| **Spend concentration** (Top 1–3 principals %) | Supply chain vulnerability | **Yes** — purchasing |
| **Inventory concentration** (Top supplier % of stock) | Different lens — legacy stock vs current buying | **Yes** — M15 |
| **At-risk stock by supplier** | Buying more from principals with idle inventory | **Yes** — M19 |
| **Purchase–inventory divergence** | High inventory + low monthly purchase (or reverse) | **Derivable** |
| **Return rate by supplier** | Relationship/quality risk | **Desktop only** (PF4) |
| **Sales velocity by supplier** (Omzet Supplier) | Demand context for buy decisions | **Desktop only** |

### 7.3 Terminology

Per `docs/foundation/DOMAIN.md`: **Supplier** and **Principal** are equivalent. Portal UI uses **Principal** in labels; data fields use `SupplierName`.

### 7.4 Supplier master limitations

No discovered fields for: supplier lead time, contract tier, alternate source flag, or performance score. Analytics are **transaction-derived only**.

---

## 8. Exception-Based Management Analysis

Situations that **deserve management attention** rather than totals. Threshold values are **not chosen** — candidates and business meaning only.

### 8.1 Warning condition candidates

| ID | Condition candidate | Business meaning | Data source | Computed today? |
| -- | ------------------- | ---------------- | ----------- | --------------- |
| W-U01 | Any `BELUM` invoice in period | Potential unstaged stock (may include drafts) | Purchasing KPI | **Yes** — may over-alert |
| W-U02 | `BELUM` value > 0 | Monetary exposure in limbo | Posting breakdown | **Yes** |
| W-U03 | `BELUM` invoice older than TBD days | Stale draft or delayed posting | `CreateTime` + `BELUM` | **No** — needs DAL extension |
| W-U04 | Top 1 principal > TBD % of monthly purchase | Spend concentration | Top Principal % | **Partial** — % in executive only |
| W-U05 | Top 1 supplier > TBD % of inventory value | Stock dependency | Executive inventory | **Yes** (executive) |
| W-U06 | Weekly purchase > prior week by TBD % | Spend spike | Week trend | **No** — comparison logic new |
| W-U07 | Zero invoices mid/late month | Purchasing inactivity | `Total Invoice` + calendar | **Partial** |
| W-U08 | Principal in Top 10 purchase AND Top 10 at-risk inventory | Compound dependency + idle stock | Cross-domain join | **No** |
| W-U09 | Retur Beli volume > TBD vs purchases | Return anomaly | PF4 + PF1 | **No** |
| W-U10 | `Unknown` principal in Top 10 | Master data quality | Aggregator | **Yes** — detectable |

### 8.2 Critical condition candidates

| ID | Condition candidate | Business meaning | Data source | Computed today? |
| -- | ------------------- | ---------------- | ----------- | --------------- |
| C-U01 | Stale `BELUM` value exceeds TBD absolute | Large capital blocked from inventory | Age-qualified backlog | **No** |
| C-U02 | Single principal > TBD % spend AND > TBD % inventory | Critical supplier dependency | Cross-domain | **Partial** |
| C-U03 | Purchase spike > TBD % with flat sales trend | Over-purchasing risk | Purchase + sales week trend | **No** |

### 8.3 Concentration indicators (informational — no auto threshold in M17/M18 pattern)

| Indicator | Numerator / denominator | Already computed? |
| --------- | ----------------------- | ----------------- |
| Top Principal % of purchase | Top 1 principal amount / Grand Total Purchase | Executive — **Yes** |
| Top 3 Principals % | Sum top 3 / total | **No** |
| Top Supplier % of inventory | Executive inventory | **Yes** |
| Top Supplier % of at-risk inventory | M19 supplier risk / at-risk total | **Partial** — bar chart values |

### 8.4 Attention list candidates (entity × signal grain)

Following M17/M18/M20 pattern:

| Entity grain | Signal candidates | Source |
| ------------ | ----------------- | ------ |
| **Principal × signal** | HighSpendConcentration, HighInventoryExposure, HighAtRiskExposure, PurchasingInactive (no MTD purchase), ReturnSpike (if PF4 exposed) | Snapshots + PF4 |
| **Invoice × signal** | StaleBelum (if age rule), LargeBelumValue | Invoice rows |
| **Company × signal** | PurchasingPaceSpike, PurchasingInactivity, UnqualifiedBacklog | Aggregates |

**PO must decide** preferred grain (principal-first aligns with business language).

### 8.5 Trend candidates

| Trend | Available | Attention interpretation (TBD) |
| ----- | --------- | ------------------------------ |
| Weekly purchase amount | **Yes** | Spike vs prior week; late-month deceleration |
| Monthly purchase total | **Yes** (current month only) | No prior-month retention in snapshot |
| Pending posting count over time | **No** | Requires historical snapshots |
| Principal share over time | **No** | Requires history |

---

## 9. Existing Desktop Capability Analysis

### 9.1 Purchasing-oriented Desktop screens

| Screen | Analytics? | Portal exposure | Reuse potential |
| ------ | ---------- | --------------- | --------------- |
| PF1 `InvoiceInfoForm` | Grid + Excel; user date range | **Yes** — Purchasing Report | Data path authority |
| PF2 `InvoiceBrgInfoForm` | Line-level grid | **No** | Item/category purchase validation |
| PF3 `InvoiceHarianDetilForm` | Financial detail | **No** | Low management value |
| PF4 `ReturBeliBrgInfoForm` | Return grid | **No** | Return anomaly signals |
| PT1 `InvoiceForm` | Transaction entry | **No** | Workflow knowledge |
| PT2 `PostingStokForm` | Operational queue | **No** | Defines `BELUM`→`SUDAH` semantics |
| PT3 `ReturBeliForm` | Transaction entry | **No** | Return workflow |
| PM1 `SupplierForm` | Master CRUD | **No** | None |

**Unlike Sales:** No `SalesOmzetChartForm` equivalent for purchasing. Portal purchasing analytics are **net-new** relative to Desktop.

### 9.2 Cross-domain Desktop analytics relevant to purchasing management

| Screen | Relevance |
| ------ | --------- |
| `StokBrgSupplierForm` (IF4) | Stock detail by supplier — inventory exposure drill-down |
| `OmzetSupplierInfoForm` | Sales by supplier — demand context for buy decisions |
| `StokBalanceInfo2Form` (IF1) | Portal Inventory Report source |
| `KartuStokSummaryForm` (IF8) | Movement classes include INVOICE — purchase inflow timing |
| `FakturPerSupplierForm` (RO2) | Sales by supplier's products |

### 9.3 Business rules embedded in Desktop (reuse, do not reimplement differently)

| Rule | Location |
| ---- | -------- |
| Void exclusion `VoidDate = '3000-01-01'` | `InvoiceViewDal` SQL |
| `PostingStok` derivation | `IIF(IsStokPosted = 1, 'SUDAH', 'BELUM')` |
| Save always unposted | `SaveInvoiceWorker` `.IsPosted(false)` |
| Posting adds FIFO stock | `GenStokInvoiceWorker` |
| Blank supplier → Unknown | `DashboardPurchasingInvoiceAggregator` |
| Current month period | `PurchasingReportDal.CurrentMonthPeriode()` pattern |

---

## 10. Inventory-Purchasing Relationship Analysis

### 10.1 How purchasing affects inventory

| Effect | Mechanism | When it occurs |
| ------ | --------- | -------------- |
| Stock quantity increase | `GenStokInvoiceWorker` → `AddStokWorker` | PT2 posting (`SUDAH`) |
| HPP update | `SaveInvoiceWorker.UpdateHpp` | PT1 save (before or after posting) |
| Bonus stock | `INVOICE-BONUS` mutasi | Posting when `QtyPotStok > QtyBeli` |
| Stock decrease (return) | `GenStokReturBeliWorker` | PT3 Retur Beli |

**Key:** Physical/sellable stock increment happens at **posting**, not at invoice save.

### 10.2 Replenishment signals already available (no forecasting)

| Signal | Source | Management meaning |
| ------ | ------ | ------------------ |
| Low stock qty (item × warehouse) | Inventory Report / `IStokBalanceViewDal` | Potential need to buy — **operational**, not M21 purchasing history |
| Slow/dead/never-sold classification | M19 | **Avoid replenishing** idle SKUs |
| Supplier at-risk exposure | M19 | Principal-specific idle capital |
| Recent purchase lines (PF2) | Desktop | What was recently bought |
| Pending `BELUM` purchases | Purchasing | Ordered on paper but not in stock |

**No reorder point, safety stock, or demand forecast** discovered in BTR for portal use.

### 10.3 Composite attention candidates (inventory + purchasing)

| Composite signal | Logic sketch | Available? |
| ---------------- | ------------ | ---------- |
| **Buy-heavy + sell-slow principal** | High MTD purchase + high M19 at-risk by supplier | **Derivable** |
| **Posted purchase of never-sold items** | PF2 lines where BrgId in M19 Never Sold list | **Derivable** |
| **BELUM purchase for low-stock item** | PF2 + inventory qty — complex | **Not computed** |
| **Inventory supplier concentration without recent purchase** | M15 top supplier + zero MTD purchase | **Derivable** |

### 10.4 What M21 should not invent

- Forecast models, EOQ, or automated replenishment recommendations
- Warehouse-level purchase planning (deferred in V1 purchasing)
- Mutasi-based movement classification for portal (M19 uses Last Faktur Date)

### 10.5 Recommended cross-navigation (pattern from M17/M19)

| From M21 | To | Purpose |
| -------- | -- | ------- |
| Principal row | Purchasing Report `?q=principal` | Validate invoices |
| Principal row | Inventory Risk (supplier filter/search if supported) | At-risk stock context |
| Attention item (SKU) | Inventory Report | Stock position |
| Company signal | Inventory Dashboard | Stock composition |

---

## 11. Dashboard Layout

**Approved:** Proposal A — Purchasing Attention First. Proposals B and C rejected.

Text-only wireframe for discussion support — not visual design specification.

### 11.1 Approved layout — Purchasing Attention First

Aligns with M17 Customer Analytics and M20 Collection Dashboard.

```text
Purchasing Management Dashboard
Subtitle: Current Month Purchasing — Management Attention View

┌─────────────────────────────────────────────────────────────────┐
│ ATTENTION CARDS                                                 │
│ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────┐ │
│ │ Posting      │ │ Principal    │ │ Purchasing   │ │ Inventory│ │
│ │ Exposure     │ │ Dependency   │ │ Pace         │ │ Cross-   │ │
│ │ (qualified   │ │ (Top % +     │ │ (spike /     │ │ Risk     │ │
│ │  BELUM TBD)  │ │  count)      │ │  inactivity) │ │ (at-risk │ │
│ └──────────────┘ └──────────────┘ └──────────────┘ │  supplier)│ │
│                                                     └──────────┘ │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ PURCHASING SUMMARY (traceability row)                           │
│ Grand Total Purchase │ Total Invoice │ Posted % │ BELUM value    │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ ATTENTION LIST                                                  │
│ Principal │ Signal │ Amount │ Context │ Link                    │
│ (one row per Principal × signal — invoice detail in drill-down) │
└─────────────────────────────────────────────────────────────────┘

┌────────────────────────────┐ ┌──────────────────────────────────┐
│ WEEKLY PURCHASE TREND      │ │ POSTING STATUS (SUDAH / BELUM)   │
└────────────────────────────┘ └──────────────────────────────────┘

┌────────────────────────────┐ ┌──────────────────────────────────┐
│ TOP 10 PRINCIPALS          │ │ PRINCIPAL EXPOSURE COMPARISON    │
│ (purchase amount + %)      │ │ Purchase MTD vs Inventory vs     │
│                            │ │ At-Risk (top principals)         │
└────────────────────────────┘ └──────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ NAVIGATION                                                      │
│ → Purchasing Report │ Inventory Dashboard │ Inventory Risk       │
└─────────────────────────────────────────────────────────────────┘
```

### 11.2 Rejected alternatives

| Proposal | Reason rejected |
| -------- | --------------- |
| B — Supplier Dependency First | Approved content is incorporated into Proposal A cross-domain panels |
| C — Minimal V1 evolution | Insufficient management value; PO requires cross-domain panels (Q9) |

---

## 12. Gap Analysis

### 12.1 Information already available (portal or executive)

| Information | Where |
| ----------- | ----- |
| Monthly purchase spend and invoice count | Purchasing Dashboard + Report footer |
| Pending posting count and `BELUM` value (unqualified) | Purchasing Dashboard + Executive |
| Posting status split (`SUDAH`/`BELUM`) | Purchasing Dashboard pie chart |
| Weekly purchase trend | Purchasing Dashboard |
| Top 10 principals by purchase amount | Purchasing Dashboard |
| Top 1 principal % of purchase | Executive Purchasing card |
| Top supplier/category inventory concentration | Inventory Dashboard + Executive |
| Supplier at-risk exposure | Inventory Risk Dashboard |
| Invoice-level posting status | Purchasing Report |

### 12.2 Information partially available

| Information | Gap |
| ----------- | --- |
| **Stale `BELUM` invoices** | `CreateTime`/`LastUpdate` on `BTR_Invoice` not in `InvoiceViewDal` |
| **Principal compound dependency** | Requires joining purchasing + M15 + M19 snapshots (composition, not new SQL) |
| **Purchasing pace anomaly** | Week amounts exist; comparison/flag logic missing |
| **Purchasing inactivity** | `Total Invoice` exists; no attention signal or calendar-aware rule |
| **Line-level purchase by item/category** | PF2 DAL exists; not in portal |
| **Retur Beli analytics** | PF4 DAL exists; not in portal |
| **Purchase vs sales ratio** | Both totals exist; ratio not computed |
| **Warehouse purchasing breakdown** | `WarehouseName` on `InvoiceView`; deferred in V1 |
| **Qualified posting backlog** | PO approved age-based rule; architect proposes threshold; `CreateTime` extension approved (Q29) |

### 12.3 Information not currently available

| Information | Reason |
| ----------- | ------ |
| Budget vs actual purchasing | No budget master |
| Purchase Order status / approval pending | No PO entity or approval workflow |
| Supplier lead time / performance score | Not in master data |
| Historical purchasing trends (prior months in snapshot) | Current-month-only purchasing snapshot |
| Replenishment forecast / reorder recommendations | No forecasting model |
| Purchase-to-stockout linkage | Not computed |
| Collector-style purchasing workload queue | No purchasing queue entity |
| Event-driven snapshot refresh on post | Scheduled worker only |

---

## 13. Final Product Decisions (Authoritative)

**Status:** All open questions resolved by Product Owner — 2026-06-09.  
**Audience:** Architect — use this section as scope input. Do not re-decide business rules listed here.

**Product direction:** M21 answers *Which suppliers and purchasing activities require management attention?* The dashboard must remain a **Purchasing Management Dashboard** — not a statistics dashboard and not an operations dashboard.

### 13.1 Posting Stok / `BELUM` semantics

| # | Decision |
| - | -------- |
| Q1 | Any `BELUM` invoice does **not** automatically require management attention. |
| Q2 | **Actionable backlog** = `BELUM` invoice older than operationally reasonable age. Architect may propose age-based implementation. |
| Q3 | **Yes.** Revise Executive Purchasing `RequiresAttention` logic to avoid treating all `BELUM` invoices as attention. |
| Q4 | **Pending Posting Value** is a **supporting KPI**, not the primary KPI. Primary posting attention = **qualified backlog** count/exposure. |
| Q5 | Attention List = **Principal × Signal**. Invoice-level details belong in **drill-down** (Purchasing Report). |

**PO note — staged entry:** `SaveInvoiceWorker` always saves with `IsStokPosted = false`. Qualification by age distinguishes draft-in-progress from actionable backlog. Architect proposes age threshold; PO does not fix numeric value in analysis phase.

### 13.2 Scope and differentiation

| # | Decision |
| - | -------- |
| Q6 | **Extend** existing Purchasing Dashboard. Do **not** replace V1 sections. |
| Q7 | **Dedicated snapshot domain:** `BTRPD_PurchasingManagement*`. |
| Q8 | Page title: **Purchasing Management Dashboard**. |
| Q9 | **Include cross-domain panels** — primary source of M21 management value. |
| Q10 | **Promote selected signals** to Executive Dashboard **after M21 stabilizes**. |

### 13.3 Supplier and concentration

| # | Decision |
| - | -------- |
| Q11 | **Mandatory:** Top 10 with **percentage column**. Top 1 and Top 3 percentages may surface in KPI cards. |
| Q12 | **Yes.** `CompoundDependency` is a valid management attention signal. |
| Q13 | **Yes.** High Inventory Exposure + Zero MTD Purchase is a useful attention signal. |

### 13.4 Retur Beli and line-level analytics

| # | Decision |
| - | -------- |
| Q14 | **Retur Beli out of scope** for M21 V1. |
| Q15 | N/A |
| Q16 | **Stay header-level.** Do not introduce PF2 line-level analytics in M21. |

**PO rationale:** M21 remains a management dashboard, not another report.

### 13.5 Pace and anomaly signals

| # | Decision |
| - | -------- |
| Q17 | **Purchasing Inactivity** is a valid attention signal. Architect may propose calendar-aware logic. |
| Q18 | **Weekly spike/deceleration remains visual-only** for V1 (no automated attention flag). |
| Q19 | **Purchase-to-Sales Ratio excluded** — potentially misleading. |

### 13.6 Inventory replenishment framing

| # | Decision |
| - | -------- |
| Q20 | **Do not calculate** "Buying Into Slow/Dead Stock" in M21 V1. |
| Q21 | **Yes.** Strong navigation and cross-linking to **Inventory Risk Dashboard** required. M19 owns inventory-risk logic. |

### 13.7 Period, traceability, and reports

| # | Decision |
| - | -------- |
| Q22 | **Current calendar month** remains authoritative period for purchasing metrics. |
| Q23 | **Grand Total Purchase** and **Total Invoice** traceability to Purchasing Report footer **remains mandatory**. |
| Q24 | **No** Purchasing Line Report route in M21. |
| Q25 | **Continue M17 pattern:** row click → Purchasing Report with `?q=` pre-filter applied. |

### 13.8 Attention list and signals

| # | Decision |
| - | -------- |
| Q26 | **Approved signals:** `QualifiedBacklog`, `PrincipalSpendConcentration`, `PrincipalInventoryConcentration`, `PrincipalAtRiskExposure`, `CompoundDependency`, `PurchasingInactivity`, `UnknownPrincipal`. |
| Q27 | Attention list grain = **Principal × Signal**. |
| Q28 | Follow **M17/M18/M19 philosophy:** show metrics and signals; **avoid threshold-heavy rule engines**. |

**Signal business meanings (for architect — concentration thresholds informational, not auto-alert bands unless inherited):**

| Signal | Entity | Business meaning |
| ------ | ------ | ---------------- |
| **QualifiedBacklog** | Principal | Principal has `BELUM` invoice(s) older than architect-proposed age threshold |
| **PrincipalSpendConcentration** | Principal | Principal ranks in Top 10 MTD purchase with material % of `Grand Total Purchase` |
| **PrincipalInventoryConcentration** | Principal | Principal ranks in Top 10 inventory value with material % of `Total Inventory Value` |
| **PrincipalAtRiskExposure** | Principal | Principal ranks in M19 supplier at-risk exposure |
| **CompoundDependency** | Principal | Principal shows high concentration across **purchase + inventory + at-risk** dimensions |
| **PurchasingInactivity** | Company or Principal | Low/zero MTD purchasing activity when calendar context expects activity (architect proposes rule) |
| **PrincipalInventoryNoPurchase** | Principal | High inventory exposure (M15) with **zero MTD purchase** (Q13) |
| **UnknownPrincipal** | Principal | Blank/unknown supplier name in Top 10 or attention context |

### 13.9 Data access

| # | Decision |
| - | -------- |
| Q29 | **Approved** to extend read path (`InvoiceViewDal` or equivalent) with `CreateTime` / `LastUpdate` for age-qualified `BELUM` analysis. |
| Q30 | **Retur Beli remains excluded** from `Grand Total Purchase` traceability. |

### 13.10 Explicit exclusions (M21 V1)

| Exclusion | Reason |
| --------- | ------ |
| Retur Beli analytics | Q14 — operational Desktop only |
| PF2 line-level aggregates | Q16 — not a report milestone |
| Purchase-to-Sales Ratio | Q19 — misleading |
| Automated weekly spike/deceleration flags | Q18 — visual-only |
| Buying into slow/dead stock | Q20 — M19 owns risk logic |
| Purchasing Line Report | Q24 |
| Threshold-heavy alert engine | Q28 |
| Executive promotion in initial delivery | Q10 — post-stabilization |

---

## 14. Purchasing Management KPI Definitions (Approved)

All monetary values in IDR. Purchasing activity metrics use **current calendar month** unless noted. Cross-domain inventory metrics use **point-in-time** M15/M19 snapshots at refresh time.

| KPI | Period / scope | Formula / rule | Business meaning |
| --- | -------------- | ---------------- | ---------------- |
| **Grand Total Purchase** | Current month | `SUM(GrandTotal)` non-void invoices | Monthly purchasing spend — **traceability mandatory** |
| **Total Invoice** | Current month | Count of invoice rows | Volume of purchasing activity — **traceability mandatory** |
| **Qualified Backlog Count** | Current month | Count `BELUM` invoices where age > architect-proposed threshold | Actionable posting backlog (not all drafts) |
| **Qualified Backlog Value** | Current month | Sum `GrandTotal` of qualified `BELUM` invoices | Monetary exposure of actionable backlog |
| **Pending Posting Value (unqualified)** | Current month | Sum `GrandTotal` where `BELUM` | **Supporting KPI** — total unposted value including drafts |
| **Top 10 Principal** | Current month | Rank by `SUM(GrandTotal)` with **% of Grand Total Purchase** column | Spend concentration ranking |
| **Top 1 / Top 3 Principal %** | Current month | Top N principal amount ÷ `Grand Total Purchase` × 100 | Optional attention card metrics |
| **Top 1 Supplier % (inventory)** | Point-in-time | From M15 snapshot | Stock dependency context |
| **Principal At-Risk Value** | Point-in-time | From M19 supplier risk exposure | Idle capital by principal |
| **Compound Dependency flag** | Cross-domain | Principal in top tiers across purchase, inventory, and/or at-risk | Multi-dimensional supplier dependency |
| **Purchasing Inactivity** | Current month + calendar | Architect-proposed: company-level low/zero invoice activity | Replenishment gap signal |
| **Principal Inventory No Purchase** | MTD + M15 | High inventory value principal with zero MTD purchase | Legacy stock without current intake |
| **Weekly Purchase Trend** | Current month | `SUM(GrandTotal)` per calendar week | Pace context — **visual only** (no auto alert) |
| **Posting Status Breakdown** | Current month | `SUDAH` / `BELUM` by value | Context panel — retained from V1 |

**Qualified backlog age anchor:** Use `CreateTime` or `LastUpdate` from extended invoice read path — architect to document chosen field and proposed day threshold.

**Traceability:** `Grand Total Purchase` and `Total Invoice` must match Purchasing Report footer exactly (same `IInvoiceViewDal` source, void exclusion, current month). Qualified backlog metrics are **dashboard-only** — no report footer equivalent.

**Cross-domain panels (mandatory):** For top principals, show side-by-side or comparative view: **MTD Purchase** · **Inventory Value (M15)** · **At-Risk Value (M19)**. Read from existing snapshot domains — do not duplicate inventory aggregation SQL.

---

## 15. Relationship to Other Milestones

| Milestone | Relationship to M21 |
| --------- | --------------------- |
| **V1 Purchasing Dashboard** | Statistics foundation — **extended**, not replaced |
| **M15 Inventory Dashboard** | Supplier stock concentration — **cross-domain input** to M21 panels |
| **M19 Inventory Risk** | At-risk supplier exposure — **cross-link**; M21 does not own item-level risk |
| **M16 Executive** | Revise purchasing `RequiresAttention`; promote M21 signals **after stabilization** |
| **M17 Customer Analytics** | Attention list / drill-down pattern reuse |
| **M18 Salesman Performance** | No overlap |
| **M20 Collection** | No overlap |
| **M25 Sales Force Effectiveness** | No overlap |

**Milestone question answered:**

| Prior dashboard | Question |
| --------------- | -------- |
| V1 Purchasing | *How much was purchased this month?* |
| **M21 Purchasing Management** | *Which suppliers and purchasing activities require management attention?* |

---

## 16. Existing Asset Discovery Summary

Maximize reuse — avoid new business calculations when equivalent logic exists.

### 16.1 Portal — reuse directly

| Asset | Path / type | Reuse for M21 |
| ----- | ----------- | ------------- |
| `DashboardPurchasingInvoiceAggregator` | Application service | V1 traceability; management aggregator is **separate** |
| `RefreshDashboardPurchasingSnapshotWorker` | Worker | V1 refresh retained; **new** management refresh worker |
| `BTRPD_Purchasing*` tables | SQL | V1 unchanged; **new** `BTRPD_PurchasingManagement*` |
| `DashboardExecutiveComposer.ComposePurchasing` | Executive | Align attention semantics |
| `IInvoiceViewDal` / `InvoiceView` | Purchase read path | Primary purchasing source — **extend** with `CreateTime`/`LastUpdate` (Q29) |
| `PurchasingReportDal` | Report | Traceability authority |
| `DashboardInventoryAggregator` | M15 | Cross-domain supplier % |
| `DashboardInventoryRiskAggregator` | M19 | Cross-domain at-risk by supplier |
| `SalesOmzetChartWeekGrouper` | Week buckets | Trend/spike logic |
| Vue: `WeeklyTrendChart`, `PostingStatusPieChart`, `Top10RankingTable`, `ExecutiveAttentionCard` | Frontend | Layout and charts |

### 16.2 Desktop DALs — portal candidates (not in M21 V1 scope)

| Asset | Reuse potential |
| ----- | --------------- |
| `IInvoiceBrgViewDal` | **Excluded** — Q16 header-level only |
| `IReturBeliBrgViewDal` | **Excluded** — Q14 |
| `IInvoiceDal` (full model) | Superseded by extended `InvoiceViewDal` for age fields |
| `IStokBrgSupplierDal` | Supplier stock detail (if report added) |
| `OmzetSupplierViewDal` | Demand context (sales-side) |

### 16.3 Explicitly do not reuse (write path)

| Asset | Reason |
| ----- | ------ |
| `SaveInvoiceWorker`, `GenStokInvoiceWorker` | Transactional write |
| `InvoiceBuilder`, `PostingStokForm` actions | Operational only |
| `PurchasingReportDal` SQL duplication | Keep single `InvoiceViewDal` filter source |

---

## 17. Impact Summary

| Area | Impact |
| ---- | ------ |
| **Business areas** | Purchasing (primary), Inventory (cross-link), Portal reporting |
| **Workflows** | New management monitoring path; no change to PT1/PT2/PT3 transactions |
| **Users** | Purchasing Administration, Operations leadership, Inventory planning reviewers |
| **Differentiation** | M21 extends V1 statistics with attention layers; cross-domain panels; qualified `BELUM` semantics |
| **Executive dashboard** | Revise purchasing `RequiresAttention` in M21 delivery; further promotion after stabilization |
| **Documentation after delivery** | `btr-portal-domain.md`, `btr-portal-operational.md`, `materialized-dashboard-domain.md` |
| **Systems touched** | `btr.portal.web`, `btr.portal.api`, `btr.portal.worker`, `btr.application`, `btr.infrastructure`, `btr.sql`, `btr.test` |

---

## 18. Architect Handoff Checklist

Product Owner decisions recorded — ready for implementation planning.

- [x] M21 scope: management attention; extend V1; not statistics-only or operations-only
- [x] Page title: Purchasing Management Dashboard; route `/dashboard/purchasing`
- [x] Dedicated `BTRPD_PurchasingManagement*` snapshot domain
- [x] `BELUM` qualification by age — architect proposes threshold; not all `BELUM` = attention
- [x] Revise executive purchasing `RequiresAttention` for qualified backlog
- [x] Attention list: Principal × Signal; drill-down to Purchasing Report (`?q=`)
- [x] Approved signal set (Section 13.8) including `PrincipalInventoryNoPurchase`
- [x] Cross-domain panels mandatory (M15 + M19); no duplicate inventory SQL
- [x] Top 10 Principal with % column; Top 1/Top 3 % optional on cards
- [x] Traceability: Grand Total Purchase + Total Invoice unchanged
- [x] Extend `InvoiceViewDal` with `CreateTime`/`LastUpdate`
- [x] Weekly trend visual-only; Purchasing Inactivity signal (calendar rule TBD by architect)
- [x] Strong navigation to Inventory Dashboard and Inventory Risk
- [x] Executive promotion deferred until post-stabilization
- [x] Exclusions: Retur Beli, PF2, purchase/sales ratio, slow/dead buy signal, line report

**Architect proposes (business decision delegated):**

- Age threshold for qualified `BELUM` backlog
- Calendar-aware rule for Purchasing Inactivity
- Compound Dependency composition rule (which top-tier intersections qualify)
- `RequiresAttention` resolver inputs per attention card (without threshold-heavy engine)

**Reference implementations:**

| Area | Reference |
| ---- | --------- |
| Attention-first layout | `M20-collection-dashboard-analysist.md`, `M17-Customer-Analytics-Analysis.md` |
| V1 purchasing baseline | `docs/archive/purchasing-dashboard/analysis-report.md` |
| Executive composer | `DashboardExecutiveComposer.ComposePurchasing` — revise for Q3 |
| Cross-domain read | M15/M19 snapshot DALs — compose, do not re-aggregate |

---

*End of analysis — M21 Purchasing Dashboard*

