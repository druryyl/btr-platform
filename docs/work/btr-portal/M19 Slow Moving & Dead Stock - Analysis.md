# BTR Portal Analysis — M19 Slow Moving & Dead Stock Dashboard

**Status:** Analysis complete — **all open questions resolved.** See Section 12 for authoritative Product Owner decisions. Ready for Architect.  
**Scope:** Business analysis only. No implementation plans, API design, database design, or code.  
**Date:** 2026-06-08 (analysis) · Product Owner decisions recorded 2026-06-08  
**Context:** BTR Portal V2 (M16 Executive Dashboard, M17 Customer Analytics, M18 Salesman Performance complete) follows a management philosophy: *What requires management attention?* The current Inventory Dashboard answers composition questions (how much, where by category/supplier). M19 introduces a dedicated **Slow Moving & Dead Stock Dashboard** focused on **inventory risk and inventory health**.

**Approved roadmap position:** M17 Customer Analytics → M18 Salesman Performance → **M19 Slow Moving & Dead Stock** → M20 Collection Dashboard → … → M25 Sales Force Effectiveness

**Reference documents:** `docs/foundation/PRODUCT.md`, `docs/foundation/DOMAIN.md`, `docs/foundation/LANDSCAPE.md`, `docs/foundation/WORKFLOW.md`, `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-operational.md`, `docs/features/btr-portal/btr-portal-architecture.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md`, `docs/work/btr-portal/M16 Executive Dashboard - Analysis.md`, `docs/work/btr-portal/M17-Customer-Analytics-Analysis.md`, `docs/work/btr-portal/M18 Salesman Performance - Analysis.md`, `docs/archive/btr-portal-api-scaffolding/portal-analysis-m10-m12-final.md`, `docs/archive/btr-portal-api-scaffolding/portal-analysis-m13-m15-final.md`, `btr-reporting-investigation.md`

**Explicit exclusions (per product direction):** Salesman performance (M18), collection performance (M20), route effectiveness (M25). M19 maintains an **inventory-management perspective** only.

---

## 1. Executive Summary

BTR Portal today exposes **inventory composition** — total capital tied up in stock and how that value is distributed across categories and suppliers. It does **not** answer whether inventory is **moving**, **aging**, or **creating financial risk**. Management attention signals for inventory on the Executive Dashboard are limited to **concentration percentages** (Top Category %, Top Supplier %). Slow-moving, dead stock, ABC classification, turnover, and warehouse-level imbalance are **explicitly deferred** in product documentation and **not implemented** in portal or snapshot tables.

M19 answers:

> Which inventory requires management attention and why?

### Key findings

| Finding | Implication |
| ------- | ----------- |
| **Stock position data is mature** — `StokBalanceView` / `BTR_StokBalanceWarehouse` powers portal inventory dashboard and report with consistent BrgId-first aggregation, HPP valuation, In-Transit exclusion | M19 position layer can reuse existing portal rules without redefining valuation |
| **Movement data exists on Desktop but not in portal** — `BTR_StokMutasi` ledger drives Kartu Stok, Kartu Stok Summary, and Stok Periodik | Slow/dead classification requires **movement-layer DALs** already proven on Desktop; portal V1 deliberately chose balance-only for M11/M15 |
| **No item-level last-sale logic exists** — `CustomerLastFakturDal` (M17/M18) provides a reusable **pattern** for `MAX(FakturDate)` aggregation; `FakturBrgViewDal` provides item-level sales lines | M19 needs an analogous **item-level last Faktur** query; SQL join path is established |
| **Kartu Stok Summary computes `MovingStok`** per item per warehouse per period — closest existing BTR definition of inventory movement | Desktop formula is the strongest anchor for period-based movement KPIs |
| **M17 dormant pattern approved for inventory** — 90 days (Slow Moving), 180 days (Dead Stock); Last Faktur Date is authoritative | Classification rules are defined; item-level `IBrgLastFakturDal` approved |
| **Concentration KPIs already exist** (Top 10 Category/Supplier, executive Top Category/Supplier %) | M19 adds **at-risk concentration** by category/supplier; does not replace M15 composition analytics |
| **No inventory aging buckets in code today** | M19 introduces **first inventory aging model** — aging pie chart required; bucket boundaries for architect to propose aligned with 90/180-day thresholds |
| **StokHarianForm is an empty stub** — no implemented daily stock analytics | Not a reuse candidate |
| **StokBalanceHealthDal** detects balance-table vs FIFO-lot mismatch | Operational **data integrity**, not obsolescence — out of scope for management dashboard unless PO wants admin signal |
| **M18 explicitly excludes salesman dimension from M19** | Item/category/supplier lens only; no per-rep inventory attribution |

**All open questions resolved.** See Section 12 for authoritative Product Owner decisions.

### Approved product outcome

Deliver **Slow Moving & Dead Stock Dashboard** at **`/dashboard/inventory-risk`** — page title: **Slow Moving & Dead Stock Dashboard**. **Supplement** the existing Inventory Dashboard (`/dashboard/inventory`); do not replace it.

Answer: *Which inventory requires management attention and why?*

**Mandatory v1 sections (priority order):**

1. Inventory Attention Cards  
2. Inventory Aging Distribution (pie chart)  
3. Inventory Attention List (Item × Signal)  
4. Top 10 Dead Stock  
5. Top 10 Slow Moving  
6. Category and Supplier Risk Exposure  
7. Navigation to Inventory Dashboard and Inventory Report  

**Mandatory headline KPIs:** Dead Stock Item Count, Dead Stock Value, Slow Moving Item Count, Slow Moving Value, **At-Risk Inventory %**.

**Data architecture:** Dedicated snapshot domain `BTR_PortalDashboardInventoryRisk*`; snapshot-only dashboard; 60-minute refresh; new item-level Last Faktur DAL (`CustomerLastFakturDal` pattern); full `FakturItem` history scan on refresh acceptable.

**Executive Dashboard (post-M19):** Promote **Dead Stock Value**, **At-Risk Inventory %**, and **Inventory Risk Attention Indicator** — extends executive view from composition to health while preserving existing concentration metrics.

**Explicit exclusions confirmed:** No salesman dimension, ABC, warehouse breakdown, StokBalanceHealth, export, Kartu Stok drilldown in portal.

---

## 2. Management Attention Discovery

This section identifies inventory-related situations that typically require management intervention, mapped to **existing BTR capabilities**. Items marked **Portal today** are calculable from current portal snapshots or reports. Items marked **Desktop only** exist in BTR Desktop but are not in the portal. Items marked **Not available** have no implemented logic discovered in codebase or documentation. Items marked **Partial** have source data or patterns but no aggregate KPI.

### 2.1 Movement and obsolescence

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Dead stock — on-hand qty with no recent sales** | Capital locked in items customers are not buying; write-off or promotion risk | `StokBalanceView` (qty > 0) + potential `FakturBrgView` / `BTR_FakturItem` for last sale date | **Not available** — data composable; no aggregate |
| **Slow-moving inventory — low sales velocity relative to on-hand** | Stock turns slowly; excess working capital and expiry/obsolescence risk | `KartuStokSummaryForm` computes `MovingStok` over user-selected period; `FakturBrgView` for sales qty | **Desktop only** — movement summary per warehouse + period |
| **Zero movement in period with positive closing stock** | Item had stock but no in/out activity in selected window | `KartuStokSummaryForm`: `MovingStok == 0` AND `StokAkhir > 0` | **Desktop only** |
| **Items never sold (with stock on hand)** | New intake or mistaken SKU with no market demand | `FakturBrgView` history vs `StokBalanceView` items with qty > 0 | **Not computed** |
| **Long time since last Faktur (item grain)** | Same semantic as customer dormant (M17) applied to products | Pattern: `CustomerLastFakturDal`; item source: `BTR_FakturItem` + `BTR_Faktur` | **Partial** — pattern exists; item DAL does not |
| **Long time since last stock outflow (any mutasi type)** | Broader than sales — includes retur, mutasi, opname | `BTR_StokMutasi` — `MAX(MutasiDate)` where net outbound | **Not computed** |
| **Inventory aging buckets** | Time-on-hand or time-since-last-movement distribution | Piutang has 5 aging buckets; inventory has none | **Not available** |
| **ABC classification (value × velocity)** | Prioritize management action on high-value slow movers | Explicitly deferred in `btr-portal-domain.md` | **Not available** |
| **Inventory turnover / days of supply** | How fast stock converts to sales | Derivable from `FakturBrgView` + average inventory (`StokPeriodikDal` opening/closing) | **Not computed** |

### 2.2 Capital concentration and overstock

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **High total inventory capital tie-up** | Large working capital in stock | `Total Inventory Value` on Inventory Dashboard | **Portal today** — informational, not exception by itself |
| **Category concentration risk** | Too much value in one product category | Top 10 Categories; horizontal bar chart; executive Top Category % | **Portal today** |
| **Supplier/principal concentration risk** | Over-dependence on one principal's stock | Top 10 Suppliers; horizontal bar chart; executive Top Supplier % | **Portal today** |
| **Dead/slow stock concentration by category** | Obsolescence risk clustered in a category | Category dimension on `StokBalanceView` + movement classification (not built) | **Partial** |
| **Dead/slow stock concentration by supplier** | Principal-specific intake or demand problems | Supplier dimension on `StokBalanceView` + movement classification (not built) | **Partial** |
| **High-value slow movers (composite severity)** | Few items tie up disproportionate capital without movement | Rank by `Hpp × Qty` where movement signal is weak | **Not computed** — Top 10 ranking pattern exists |
| **Overstock vs recent sales (qty on hand >> recent sales qty)** | Excess relative to demand | `StokBalanceView.Qty` vs `SUM(QtyJual)` from `FakturBrgView` over period | **Not computed** |
| **Warehouse-level overstock / imbalance** | Branch holds excess while others starve | `StokBalanceView` has `WarehouseName`; deferred warehouse breakdown | **Partial** — row-level in Inventory Report; no warehouse KPI |

### 2.3 Purchasing and intake linkage

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Recent purchase but no subsequent sales** | Buying continued despite no demand signal | `BTR_StokMutasi` INVOICE/INVOICE-BONUS in period + no FAKTUR outflow | **Partial** — mutasi types in `KartuStokSummaryDal` |
| **Pending purchase posting backlog** | Goods invoiced but not in sellable stock | Purchasing Dashboard `Pending Posting Invoice Count` | **Portal today** — purchasing domain, not M19 scope but related workflow |
| **Supplier sales calendar vs stock on hand** | Principal sells well but specific SKUs stagnant | `OmzetSupplierInfoForm` — supplier × day omzet matrix | **Desktop only** — supplier sales, not SKU-level |

### 2.4 Returns and quality

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **High return volume on item with large stock** | Product quality or market fit issue | `ReturJualBrgViewDal` — item-level retur with `BrgId` | **Desktop only** |
| **Net movement distorted by retur** | Sales outflow offset by returns — true demand lower than Faktur qty | `KartuStokSummaryDal` includes RETURJUAL bucket in `MovingStok` | **Desktop only** |

### 2.5 Data integrity (operational)

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Stock balance table out of sync with FIFO lots** | Reported qty may be wrong — decisions on bad data | `StokBalanceHealthDal` — mismatch count between `BTR_StokBalanceWarehouse` and `SUM(BTR_Stok.Qty)` | **Desktop only** — admin/IT |
| **In-Transit stock buildup** | Goods not available for sale but holding value | In-Transit warehouse **excluded** from portal; Desktop `ShowInTransitCheckBox` on IF1 forms | **Not visible in portal** — by design |

### 2.6 Workflow-derived attention points

From `docs/foundation/WORKFLOW.md` and portal operational workflows:

| Workflow stage | When management cares about inventory | Portal support today |
| -------------- | ------------------------------------- | -------------------- |
| Purchase → **Stock Receipt (Posting)** → Inventory | New stock enters; slow movers may worsen | Purchasing pending posting KPI only |
| Sales Order → Faktur → **Stock outflow** | Demand signal for replenishment vs clearance | No item-level movement KPI |
| **Stok Opname** → Adjustment | Physical count reveals dead/slow stock | Opname mutasi in Kartu Stok Summary only (Desktop) |
| Inventory planning / purchasing decisions | Which SKUs to reorder, promote, or write off | Category/supplier composition only |

---

## 3. Existing Dashboard Reuse Analysis

### 3.1 Management Attention Center (`/dashboard`) — inventory metrics today

| Metric | Source | Movement/obsolescence oriented? | Reuse for M19 |
| ------ | ------ | ------------------------------- | ------------- |
| Total Inventory Value | Inventory snapshot via `DashboardExecutiveComposer` | No — capital total | Denominator for "at risk" % KPIs |
| Top Category % | Composed from `BTR_PortalDashboardInventoryBreakdown` | Partial — concentration, not movement | Context card; avoid duplicating as primary M19 story |
| Top Supplier % | Same breakdown | Partial — concentration | Same |
| `RequiresAttention` on inventory card | Set when concentration % is computable | **Weak** — not tied to slow/dead logic | M19 may warrant **new executive signals** — PO decision (Section 12) |

**Assessment:** Executive inventory attention today is **composition-only**. M19 is the natural source for **obsolescence and movement-based** executive signals if PO chooses to promote them later.

### 3.2 Inventory Dashboard (`/dashboard/inventory`)

| KPI / section | M19 relevance | Hidden movement data | Reuse rationale |
| ------------- | ------------- | ---------------------- | --------------- |
| Total Inventory Value | Denominator for exposure KPIs | None | Same `DashboardInventoryAggregator` rules |
| Total Item | Breadth of active SKUs | None | Same aggregator |
| Inventory by Category (bar chart) | Segmentation dimension for slow/dead rollups | None | Chart component `InventoryHorizontalBarChart.vue` reusable for **at-risk value by category** |
| Inventory by Supplier (bar chart) | Same for supplier | None | Same component |
| Top 10 Categories | Concentration ranking | None | `Top10RankingTable.vue` pattern for **Top 10 Dead Stock by Value** etc. |
| Top 10 Suppliers | Concentration ranking | None | Same |

**Key gap:** Inventory snapshot (`BTR_PortalDashboardInventoryKpi`, `BTR_PortalDashboardInventoryBreakdown`) stores **composition only** — no movement, last-sale, or aging fields.

### 3.3 Customer Analytics Dashboard (M17) — pattern reuse

| M17 artifact | M19 adaptation |
| ------------ | -------------- |
| Attention Cards (Collection, Concentration, Activity, Inactivity, Credit) | Cards for **Dead Stock Exposure**, **Slow Moving Exposure**, **Category/Supplier Concentration of at-risk stock**, **High-Value Idle Items** |
| Attention List (entity × signal) | **Item × signal** or **Category/Supplier × signal** list — e.g., Dead Stock, Slow Moving, Zero Movement |
| `DormantDaysThreshold = 90` | Candidate item idle threshold — **PO must confirm** |
| `ExecutiveAttentionCard.vue`, `CustomerAttentionList.vue` | Proven attention UX — adapt labels and severity |
| Dedicated snapshot domain (`BTR_PortalDashboardCustomer*`) | Template for **`BTR_PortalDashboardInventoryRisk*`** or extend inventory snapshot — PO/architect decision |
| Segmentation sections | **By Category**, **By Supplier**, **By Movement Band** (if defined) |

### 3.4 Piutang Dashboard (M14) — pattern reuse (not data)

| Piutang artifact | M19 adaptation |
| ---------------- | -------------- |
| 5-bucket aging pie chart (`AgingPieChart.vue`) | **Inventory aging by days-since-last-sale** or **days-since-last-outflow** — if PO defines buckets |
| `DashboardPiutangAggregator` bucket definitions | Template for time-since-event bucketing |
| Overdue Customer count | Analog: **Dead Stock Item Count**, **Slow Moving Item Count** |

### 3.5 Sales / Purchasing dashboards — limited reuse

| Dashboard | Reuse for M19 |
| ----------- | ------------- |
| Sales | `FakturView` / sales DALs for velocity — not on sales dashboard UI |
| Purchasing | Pending posting signals workflow-related; not slow-stock analytics |

### 3.6 Reuse summary

| Reuse candidate | Confidence | Rationale |
| --------------- | ---------- | --------- |
| `DashboardInventoryAggregator` (position rules) | **High** | Authoritative BrgId-first, In-Transit, HPP × Qty, Unknown dimensions |
| `IStokBalanceViewDal` | **High** | Same source as inventory dashboard and report |
| `InventoryReportDal` | **High** | Footer reconciliation pattern for position totals |
| `CustomerLastFakturDal` SQL pattern | **High** | Proven `MAX(FakturDate)` + void exclusion — swap grain to BrgId |
| `FakturBrgViewDal` | **High** | Item-level sales lines for velocity and last-sale |
| `KartuStokSummaryDal` + `StokPeriodikDal` | **High** | Desktop-authoritative movement buckets and opening/closing |
| `DashboardCustomerAggregator` attention list composition | **High** | Signal constants, priority ordering, `*RequiresAttention` flags |
| `InventoryHorizontalBarChart.vue`, `Top10RankingTable.vue`, `AgingPieChart.vue` | **High** | Existing chart/table components |
| Materialized snapshot worker pattern | **High** | M17/M18 dedicated domain refresh — movement scan may be expensive |
| `DashboardExecutiveComposer.ComposeInventory` | **Medium** | Extend only if PO wants executive promotion of dead-stock KPIs |
| `StokBalanceHealthDal` | **Low** | Different problem domain (integrity vs obsolescence) |
| `OmzetSupplierViewDal` | **Low–Medium** | Supplier sales calendar — not SKU-level |

---

## 4. Existing Report Reuse Analysis

### 4.1 Portal reports

| Report | Inventory movement / aging content | Drill-down role for M19 |
| ------ | ----------------------------------- | ----------------------- |
| **Inventory Report** (`/reports/inventory`) | Point-in-time balance: Item, Warehouse, Qty, HPP, Nilai Sediaan. **No** last sale, movement, or aging columns | Validate **on-hand qty and value** for flagged items; filter via client-side `?q=` on item name/code |
| **Sales Report** (`/reports/sales`) | Faktur header grain — **no item columns** | Weak for item validation — cannot see per-item sales from this report |
| **Piutang Report** | Not inventory | Out of scope |
| **Purchasing Report** | Purchase invoice header — not item stock movement | Out of scope |

**Gap:** Portal has **no item-level sales report**. Desktop `FakturBrgInfoForm` (item × Faktur lines) is the natural validation path but is **not in portal**.

### 4.2 Desktop reports (validation paths, not portal routes)

| Desktop form | Menu / context | Grain | Movement / aging fields | M19 validation role |
| ------------ | -------------- | ----- | ------------------------ | ------------------- |
| `StokBalanceInfo2Form` (IF1) | Inventory | Item × Warehouse | Qty, Hpp, NilaiSediaan; optional In-Transit toggle | Reconcile position with portal Inventory Report |
| `StokBalanceInfoForm` | Inventory | Item (aggregated) | Sum qty/value across warehouses | Same at item grain |
| `KartuStokSummaryForm` (IF8) | Inventory | Item × Warehouse × Period | Invoice, Faktur, Retur, Mutasi, Opname, **MovingStok**, StokAwal/Akhir, NilaiMoving | **Primary movement validation** |
| `KartuStokInfoForm` (IF2) | Inventory | Item × Transaction | MutasiDate, JenisMutasi, QtyIn/Out, running balance | Transaction drill-down for flagged item |
| `StokPeriodikForm` | Inventory | Item × Warehouse × As-of date | Historical qty from mutasi cumulative | Point-in-time stock verification |
| `FakturBrgInfoForm` | Sales | Item × Faktur line | FakturDate, QtyJual, Total | **Last sale and velocity validation** |
| `StokBrgSupplierForm` | Inventory | Item × Warehouse | Qty, Hpp, list prices | Supplier stock detail |
| `OmzetSupplierInfoForm` | Inventory/Sales | Supplier × Day/Month | Sales omzet — not stock | Supplier demand context only |
| `MutasiInfoForm` | Inventory | Mutasi line | MutasiDate, JenisMutasi, qty | Inter-warehouse / claim movement |
| `ReturJualBrgReportForm` | Inventory | Item × Retur | ReturJualDate, InPcs | Return-adjusted demand |

### 4.3 KPI-to-report traceability matrix

| KPI candidate | Primary data source | Portal report reconciliation | Desktop validation path | Reconciliation notes |
| ------------- | -------------------- | ---------------------------- | ----------------------- | -------------------- |
| Total Inventory Value | `IStokBalanceViewDal` | **Inventory Report footer** | `StokBalanceInfo2Form` | BrgId-first; exclude In-Transit; must match M15 rules |
| Total Item (active SKUs) | Same | **Inventory Report footer** | Same | Count BrgId with aggregated Qty > 0 |
| Top Category / Supplier (composition) | `DashboardInventoryAggregator` | No direct report column — derive from balance rows | IF1 grouped by Kategori/Supplier | Already on Inventory Dashboard |
| Dead Stock Item Count | StokBalance + last sale / zero movement rule | **Partial** — count items in Inventory Report matching rule if rule uses balance + external last-sale | Kartu Stok Summary + Faktur Brg | **New KPI** — no footer today |
| Dead Stock Value (Rp) | Sum NilaiSediaan for dead subset | Partial — sum matching Inventory Report rows | Same | **New KPI** |
| Slow Moving Item Count | Movement rule (TBD) | No | Kartu Stok Summary period view | Period-dependent |
| Slow Moving Value (Rp) | Same | No | Same | Period-dependent |
| Days Since Last Sale (item) | `BTR_FakturItem` aggregate | No column | Faktur Brg Info | **New derived field** |
| Zero Movement in Period (item count) | `KartuStokSummaryDal` + form logic | No | IF8 | Warehouse + period required on Desktop |
| At-Risk Value % of Total Inventory | Composed | Ratio of new KPI to footer Total Inventory Value | N/A | Denominator reconciles with Inventory Report |
| Top 10 Dead Stock by Value | Ranked dead subset | Filter Inventory Report + sort — manual | IF8 filtered | **New ranking** |
| Inventory Aging Bucket Amounts | Time-since-event buckets | No | Faktur Brg + balance | **New** — mirror Piutang pattern |
| Category/Supplier at-risk concentration | Roll up dead/slow by dimension | No | IF1 + IF8 join | **New** |

---

## 5. Inventory Risk Analysis

This section describes **inventory-related risks already measurable within BTR** (or derivable from existing data). Thresholds are **not defined** — business meaning only.

| Risk | Business meaning | Measurable today? | Building blocks |
| ---- | ---------------- | ----------------- | --------------- |
| **Slow-moving inventory** | Stock remains on hand while sales outflow is low or sporadic relative to quantity held; working capital and storage cost accumulate | **Partial** — Desktop period movement; not portal | `MovingStok`, `FakturBrgView` qty sums, days since last Faktur |
| **Dead stock** | Items with positive on-hand quantity but no meaningful recent customer demand (no sales outflow for extended period) | **Partial** | Qty > 0 from balance + no FAKTUR mutasi / no FakturItem in window |
| **Capital lock-up** | Monetary value (HPP × Qty) tied to non-performing SKUs | **Yes** for total; **partial** for at-risk subset | `Total Inventory Value`; subset once classification exists |
| **Category concentration of idle stock** | Obsolescence risk concentrated in one category — category-level action (promotion, return to principal) needed | **Partial** | `KategoriName` on balance rows + movement class |
| **Supplier concentration of idle stock** | Principal-specific intake or demand mismatch | **Partial** | `SupplierName` on balance rows + movement class |
| **Excess inventory (qty vs demand)** | On-hand significantly exceeds recent sales rate | **Not computed** | Balance qty vs rolling `SUM(QtyJual)` |
| **Inventory aging (time on hand / time idle)** | How long stock has been without sale or movement | **Not computed** | Last Faktur date; last outbound mutasi date; FIFO lot age (not exposed) |
| **Zero-movement with stock** | Silent items — no activity in period but stock remains | **Desktop only** | IF8 `MovingStok == 0 && StokAkhir > 0` |
| **Never-sold SKU with stock** | Master data or purchasing error; no market validation | **Not computed** | Absence in FakturItem history + qty > 0 |
| **In-Transit exposure** | Value in non-sellable warehouse | **Measurable but excluded** from portal | Include only if PO changes In-Transit rule |
| **Data integrity risk** | Balance wrong → false slow/dead classification | **Desktop only** | `StokBalanceHealthDal` |
| **Purchase-without-sale risk** | Continued intake despite no demand | **Partial** | INVOICE mutasi without offsetting FAKTUR in period |
| **Return-skewed demand** | Apparent sales masked by high retur | **Partial** | `ReturJualBrgView` + FakturBrg |

**Relationship to existing concentration risk (M15/M16):** Category and supplier concentration on **total inventory value** indicates **where capital sits**. M19 adds **whether that capital is active or idle** — complementary, not redundant.

---

## 6. Inventory Movement Analysis

### 6.1 How BTR determines inventory movement today

BTR uses **multiple movement semantics** depending on context. None is labeled "slow moving" or "dead stock" in code.

| Approach | Source | Grain | What counts as "movement" | Used by |
| -------- | ------ | ----- | ------------------------- | ------- |
| **Stock mutation ledger** | `BTR_StokMutasi` | Transaction | QtyIn/QtyOut by `JenisMutasi` | Kartu Stok, Stok Periodik, Kartu Stok Summary |
| **Period mutation buckets** | `KartuStokSummaryDal` | Item × Warehouse × Period | Invoice, Faktur, Retur, Mutasi, Opname net qty | IF8 Kartu Stok Summary |
| **Composite MovingStok** | `KartuStokSummaryForm` (client) | Item × Warehouse × Period | `MovingStok = Invoice + Faktur + Retur + Mutasi + Opname` | Desktop IF8 |
| **Sales outflow (commercial)** | `BTR_FakturItem` / `FakturBrgViewDal` | Item × Faktur line | `QtyJual` on non-void Faktur | Desktop Faktur Brg Info |
| **Last sale date** | `MAX(FakturDate)` per entity | Customer (implemented); Item (not implemented) | Most recent invoiced sale | M17 `CustomerLastFakturDal` |
| **Point-in-time balance** | `BTR_StokBalanceWarehouse` | Item × Warehouse | No movement — snapshot qty | Portal inventory dashboard/report |
| **Historical balance** | `StokPeriodikDal` | Item × Warehouse × Date | Cumulative mutasi ≤ date | Desktop Stok Periodik |
| **Purchase inflow** | Mutasi types INVOICE, INVOICE-BONUS | Item × Warehouse × Period | Stock receipt from purchasing | Kartu Stok Summary Invoice bucket |

### 6.2 Mutasi types relevant to movement (from `KartuStokSummaryDal`)

| Bucket | JenisMutasi values | Direction semantics |
| ------ | ------------------ | ------------------- |
| Invoice | INVOICE, INVOICE-BONUS | Typically inbound (purchase posting) |
| Faktur | FAKTUR, FAKTUR-BONUS | Typically outbound (sales) — **primary demand signal** |
| Retur | RETURJUAL | Customer returns — adjusts net demand |
| Mutasi | MUTASI-KLAIM, MUTASI-KELUAR, MUTASI-MASUK | Inter-warehouse / claim |
| Opname | OPNAME, STOKOP, ADJUST | Physical count / adjustment |

### 6.3 Authoritative source — Product Owner decision

| Decision | Approved rule |
| -------- | ------------- |
| **Movement / aging signal** | **Last Faktur Date** per item (`MAX(FakturDate)` from non-void `BTR_Faktur` / `BTR_FakturItem`) |
| **Retur** | **Gross Faktur only** — retur does not affect aging classification |
| **Non-sales outflows** | Mutasi, opname, adjust **do not reset** the aging clock |
| **Period semantics** | **Rolling window** based on days since Last Faktur Date (not calendar month) |
| **Warehouse scope** | **Company-wide**, BrgId-first aggregation (match M15 portal philosophy) |
| **In-Transit** | **Excluded** (unchanged) |
| **Valuation** | **`BTR_Brg.Hpp`** — consistent with existing portal inventory |

**Implementation note:** Desktop IF8 (`KartuStokSummaryForm`) remains useful for validation but is **not** the authoritative classification source for M19. Kartu Stok mutasi-based movement is superseded by Last Faktur Date for portal risk KPIs.

**Subset relationship:** Dead Stock (≥ 180 days without Faktur) is a **subset** of Slow Moving (≥ 90 days without Faktur). Architect should define whether Slow Moving KPIs count the 90–179 day band only or the full ≥ 90 day population — PO mandates both KPI families separately (Section 12).

---

## 7. Existing Asset Discovery

### 7.1 Application layer — contracts and aggregators

| Asset | Path | Role for M19 |
| ----- | ---- | ------------ |
| `StokBalanceView` | `btr.application/InventoryContext/StokBalanceInfo/StokBalanceView.cs` | Position DTO — BrgId, dimensions, Qty, Hpp |
| `IStokBalanceViewDal` | `btr.application/InventoryContext/StokBalanceInfo/IStokBalanceViewDal.cs` | List all balance rows |
| `DashboardInventoryAggregator` | `btr.application/ReportingContext/DashboardSnapshotAgg/Services/DashboardInventoryAggregator.cs` | Portal composition rules (reuse constants: InTransit, Unknown, BrgId grouping) |
| `GetDashboardInventoryQuery` | `btr.application/ReportingContext/DashboardInventoryAgg/Queries/` | Read inventory snapshot |
| `GetInventoryReportQuery` / `InventoryReportDal` | `btr.application/ReportingContext/InventoryReportAgg/` | Portal inventory report |
| `IKartuStokSummaryDal` / `KartuStokSummaryDto` | `btr.application/InventoryContext/KartuStokRpt/` | Period movement buckets |
| `IStokPeriodikDal` / `StokPeriodikDto` | `btr.application/InventoryContext/StokPeriodikInfo/` | Opening/closing stock for period |
| `IKartuStokDal` / `KartuStokView` | `btr.application/InventoryContext/KartuStokRpt/` | Transaction-level ledger |
| `IFakturBrgViewDal` / `FakturBrgView` | `btr.application/SalesContext/FakturBrgInfo/` | Item sales lines |
| `ICustomerLastFakturDal` | `btr.application/SalesContext/FakturInfo/` | Last-event aggregation **pattern** |
| `IReturJualBrgViewDal` | Retur context | Return lines with BrgId |
| `IStokBalanceHealthDal` | `btr.application/InventoryContext/StokAgg/` | Integrity check (optional) |
| `DashboardCustomerAggregator` | `btr.application/ReportingContext/DashboardSnapshotAgg/Services/DashboardCustomerAggregator.cs` | Attention list + dormant threshold pattern |
| `DashboardExecutiveComposer` | `btr.application/ReportingContext/DashboardExecutiveAgg/Services/` | Executive inventory section (composition) |

### 7.2 Infrastructure layer — SQL DALs

| Asset | Path | Role for M19 |
| ----- | ---- | ------------ |
| `StokBalanceViewDal` | `btr.infrastructure/InventoryContext/StokBalanceRpt/StokBalanceViewDal.cs` | `BTR_Brg` JOIN `BTR_StokBalanceWarehouse` |
| `KartuStokSummaryDal` | `btr.infrastructure/InventoryContext/KartuStokRpt/KartuStokSummaryDal.cs` | Mutasi bucket aggregation |
| `StokPeriodikDal` | `btr.infrastructure/InventoryContext/StokPeriodikRpt/StokPeriodikDal.cs` | Cumulative mutasi ≤ date |
| `KartuStokDal` | `btr.infrastructure/InventoryContext/KartuStokRpt/KartuStokDal.cs` | Transaction ledger |
| `FakturBrgViewDal` | `btr.infrastructure/SalesContext/FakturInfoAgg/FakturBrgViewDal.cs` | Item sales SQL (BrgId in join, not in DTO) |
| `CustomerLastFakturDal` | `btr.infrastructure/SalesContext/FakturInfoAgg/CustomerLastFakturDal.cs` | Template for item last Faktur SQL |
| `InventoryReportDal` | `btr.infrastructure/ReportingContext/InventoryReportAgg/InventoryReportDal.cs` | Portal report |
| `DashboardInventorySnapshotDal` | `btr.infrastructure/ReportingContext/DashboardSnapshotAgg/` | Snapshot read/write |
| `StokBalanceHealthDal` | `btr.infrastructure/InventoryContext/StokBalanceAgg/StokBalanceHealthDal.cs` | Mismatch detection |

### 7.3 Snapshot tables (existing inventory domain)

| Table | Content |
| ----- | ------- |
| `BTR_PortalDashboardInventoryKpi` | TotalInventoryValue, TotalItem, GeneratedAt |
| `BTR_PortalDashboardInventoryBreakdown` | Category/Supplier Top 10 rows |

**No tables today** for movement, last-sale, dead stock, or aging.

### 7.4 Portal UI components

| Component | Path | M19 reuse |
| --------- | ---- | --------- |
| `InventoryDashboardView.vue` | `btr.portal.web/src/views/dashboard/` | Layout reference — M19 likely separate view |
| `InventoryHorizontalBarChart.vue` | `components/dashboard/` | Dimension breakdown charts |
| `Top10RankingTable.vue` | `components/dashboard/` | Rankings |
| `AgingPieChart.vue` | `components/dashboard/` | Aging distribution if buckets defined |
| `ExecutiveAttentionCard.vue` | `components/dashboard/` | Attention cards |
| `DashboardDetailLayout.vue` | `components/dashboard/` | Page shell |
| `CustomerAttentionList.vue` / M18 salesman list | `components/dashboard/` | Attention list pattern |

### 7.5 Desktop analytics (business knowledge)

| Form | Business capability |
| ---- | ------------------- |
| `KartuStokSummaryForm` | **Period movement summary** — authoritative `MovingStok` formula |
| `KartuStokInfoForm` | Transaction drill-down |
| `StokPeriodikForm` | Historical as-of stock |
| `StokBalanceInfo2Form` | Current balance with In-Transit toggle |
| `FakturBrgInfoForm` | Item sales history |
| `OmzetSupplierInfoForm` | Supplier sales calendar |
| `StokBrgSupplierForm` | Stock by supplier with pricing tiers |
| `MutasiInfoForm` | Claim/transfer mutasi lines |
| `StokHarianForm` | **Not implemented** — ignore |

### 7.6 Business rules (approved for portal inventory — extend carefully)

From `btr-portal-domain.md` Business Rules table:

- In-Transit warehouse excluded
- BrgId-first grouping before category/supplier rollup
- Zero qty exclusion after aggregation
- Nilai Sediaan = HPP × Qty
- Unknown category/supplier → `"Unknown"`
- Void exclusion on Faktur: `VoidDate = '3000-01-01'`
- DAL reuse — portal should not reimplement SQL divergent from Desktop

---

## 8. Exception-Based Management Analysis

Focus: inventory situations deserving **management attention** rather than balance display. **Candidates only** — no thresholds chosen.

### 8.1 Warning condition candidates

| Candidate | Business meaning | Likely inputs |
| --------- | ---------------- | ------------- |
| **Slow moving item** | Stock present; sales outflow below implicit norm for period | Qty on hand, `SUM(QtyJual)` or `MovingStok`, days since last sale |
| **Elevated idle value** | Significant Rp in items approaching dead status | Sum NilaiSediaan where days since last sale in upper range |
| **Category slow-moving concentration** | One category dominates at-risk value | Roll up slow/dead value by KategoriName |
| **Supplier slow-moving concentration** | Principal's SKUs not turning | Roll up by SupplierName |
| **Zero movement in period (with stock)** | No activity in window | IF8 logic |
| **Purchase without subsequent sale** | Recent INVOICE mutasi but no FAKTUR in follow-on window | Mutasi buckets |

### 8.2 Critical condition candidates

| Candidate | Business meaning | Likely inputs |
| --------- | ---------------- | ------------- |
| **Dead stock item** | Qty > 0; no sale for extended period | Last Faktur date + balance |
| **High-value dead stock** | Dead items with large NilaiSediaan | Rank by value |
| **Dead stock % of total inventory** | Portfolio-level capital at risk | Dead value ÷ Total Inventory Value |
| **Never sold with stock** | SKU stocked but no FakturItem history | Anti-join FakturItem |
| **Large qty + zero period movement** | Bulk idle inventory | StokAkhir + MovingStok = 0 |

### 8.3 Aging indicator candidates

| Candidate | Business meaning | Notes |
| --------- | ---------------- | ----- |
| **Days since last sale** | Customer-visible demand recency | Mirror M17 dormant days concept |
| **Days since last outbound mutasi** | Warehouse activity recency | Broader than sales |
| **Aging buckets (e.g. 0–30, 31–60, …)** | Distribution of idle inventory value | Mirror Piutang 5-bucket UX |
| **Time since last purchase (INVOICE)** | How long ago stock was acquired | May differ from idle time if old stock |

### 8.4 Concentration indicator candidates

| Candidate | Business meaning | Already in portal? |
| --------- | ---------------- | ------------------ |
| Top Category % of **total** inventory | Capital concentration | **Yes** (M15/M16) |
| Top Supplier % of **total** inventory | Principal concentration | **Yes** |
| Top Category % of **dead/slow** inventory | Where to focus clearance | **No** |
| Top 10 items by at-risk value | SKU-level action list | **No** |

### 8.5 Ranking indicator candidates

| Candidate | Business meaning |
| --------- | ---------------- |
| Top 10 Dead Stock by Value | Highest capital tied in idle items |
| Top 10 Slow Moving by Value | Largest slow-moving exposure |
| Top 10 by Days Since Last Sale | Longest idle items |
| Bottom sales velocity (qty/month) | Weakest sellers with stock |

### 8.6 Attention list signal candidates (M17/M18 pattern)

One row per **item × signal** (or category/supplier × signal for rollups):

| Signal | Description |
| ------ | ----------- |
| Dead Stock | Meets dead definition |
| Slow Moving | Meets slow definition |
| Zero Movement | No mutasi activity in period |
| Never Sold | Stock without sales history |
| High Value Idle | Value above PO-defined floor AND idle |

---

## 9. Inventory Aggregation Analysis

### 9.1 Validated portal rules (from `DashboardInventoryAggregator` and tests)

| Rule | Implementation | Validated |
| ---- | -------------- | --------- |
| **In-Transit exclusion** | `WarehouseName != "In-Transit"` | Yes — aggregator, report, dashboard UI subtitle |
| **BrgId-first grouping** | Group balance rows by `BrgId`; sum Qty; sum `Hpp × Qty` per group | Yes — `BuildItemGroups` |
| **Inventory value** | Per group: `InventoryValue = Sum(Hpp × Qty)` across warehouse rows | Yes |
| **Zero qty exclusion** | Item groups where aggregated Qty ≤ 0 excluded from TotalItem and analytics | Yes |
| **Category/supplier rollup** | After item groups, sum InventoryValue by KategoriName / SupplierName | Yes |
| **Unknown dimensions** | Blank → `"Unknown"` | Yes |
| **Top 10 rankings** | Order by InventoryValue DESC; rank 1–10 | Yes |

### 9.2 Additional rules discovered (Desktop / infrastructure)

| Rule | Where | M19 implication |
| ---- | ----- | --------------- |
| **Dual stock sources** | `BTR_StokBalanceWarehouse` (portal) vs `BTR_Stok` FIFO lots | Movement from mutasi; balance from materialized table — usually consistent; health DAL detects drift |
| **HPP from master** | `BTR_Brg.Hpp` on balance view; period forms may use lot Hpp from mutasi | Valuation for at-risk value should **match portal** (Brg master Hpp on balance rows) unless PO wants lot-level |
| **In-Transit optional on Desktop** | `StokBalanceInfo2Form` checkbox | Portal always excludes — M19 should default same unless PO opts in |
| **Warehouse filter UI not wired** | `StokBalanceInfoForm` warehouse picker | No warehouse-scoped portal pattern today |
| **Kartu Stok Summary requires warehouse** | IF8 warehouse combo mandatory | Conflict with company-wide BrgId-first portal — PO must choose scope |
| **Stok Periodik uses mutasi cumulative** | `MutasiDate <= @date` | Historical position; different from balance table snapshot |
| **Void exclusion on Faktur** | All Faktur DALs | Applies to last-sale and velocity |
| **FakturBrgView DTO omits BrgId** | SQL has join; DTO maps BrgName only | Item-keyed portal analytics need BrgId on DTO or dedicated aggregate DAL |
| **Period max on Mutasi report** | MutasiInfoForm ~122 days | Desktop UX constraint — not necessarily portal limit |
| **Customer dormant: 90 days** | `DashboardCustomerAggregator.DormantDaysThreshold` | Candidate only for items |

### 9.3 Aggregation consistency requirement

Any M19 KPI expressing **"at risk value"** as a percentage of total inventory must use the **same denominator rules** as `Total Inventory Value` (BrgId-first, exclude In-Transit, exclude zero qty items) so executives can reconcile with Inventory Dashboard and Inventory Report footer.

---

## 10. Dashboard Layout Proposal

Text-only wireframes for discussion. Not visual design.

### 10.1 Approved layout — Attention First (`/dashboard/inventory-risk`)

**Product Owner approved** — aligns with M16/M17/M18 attention-first pattern.

```
Slow Moving & Dead Stock Dashboard
Route: /dashboard/inventory-risk
Subtitle: Inventory health — items requiring management attention

┌─────────────────────────────────────────────────────────────────────────┐
│ INVENTORY ATTENTION CARDS (mandatory)                                     │
│ [Dead Stock Item Count] [Dead Stock Value] [Slow Moving Item Count]       │
│ [Slow Moving Value] [At-Risk Inventory %]  ← headline KPIs               │
│ Generic Attention Indicator when *RequiresAttention (M16–M18 model)       │
└─────────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────┐  ┌──────────────────────────────────────┐
│ INVENTORY AGING DISTRIBUTION │  │ CATEGORY RISK EXPOSURE (bar)           │
│ (pie — required)             │  │ At-risk value by category              │
└──────────────────────────────┘  └──────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ SUPPLIER RISK EXPOSURE (bar) — at-risk value by supplier/principal        │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ INVENTORY ATTENTION LIST — Item × Signal (mandatory)                    │
│ Item | Category | Supplier | Qty | Value | Days Since Last Faktur | Signal│
│ Signals: Dead Stock, Slow Moving, Never Sold (separate)                   │
└─────────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────┐  ┌──────────────────────────────────────┐
│ TOP 10 DEAD STOCK BY VALUE   │  │ TOP 10 SLOW MOVING BY VALUE          │
│ (mandatory)                  │  │ (mandatory)                          │
└──────────────────────────────┘  └──────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ NAVIGATION — Inventory Dashboard (/dashboard/inventory)                   │
│            — Inventory Report (/reports/inventory?q= item pre-filter)   │
└─────────────────────────────────────────────────────────────────────────┘
```

### 10.2 Proposal B — Exposure First (not selected)

```
Slow Moving & Dead Stock Dashboard

┌─────────────────────────────────────────────────────────────────────────┐
│ EXPOSURE SUMMARY                                                        │
│ Total Inventory Value | At-Risk Value | Dead Stock Value | Slow Value   │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ DEAD STOCK EXPOSURE — Top 10 Items + Category/Supplier rollup bars      │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ SLOW MOVING EXPOSURE — Top 10 Items + Category/Supplier rollup bars     │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ ATTENTION LIST (compact)                                                │
└─────────────────────────────────────────────────────────────────────────┘
```

### 10.3 Proposal C — Extend existing Inventory Dashboard (not selected)

PO decision Q1/Q3: **dedicated route** `/dashboard/inventory-risk` that **supplements** (does not replace) `/dashboard/inventory`.

---

## 11. Gap Analysis

### 11.1 Information already available

| Information | Where |
| ----------- | ----- |
| Current qty and HPP per item × warehouse | `StokBalanceView` / Inventory Report |
| Total inventory value and item count (portal KPIs) | Inventory snapshot + report footer |
| Category and supplier composition | Inventory Dashboard |
| Item master dimensions (code, name, category, supplier) | Balance view joins |
| Sales transaction lines with qty and date | `FakturBrgViewDal` (Desktop + infra) |
| Stock mutation history by type | `BTR_StokMutasi` via Kartu Stok DALs |
| Period movement buckets (Invoice, Faktur, Retur, Mutasi, Opname) | `KartuStokSummaryDal` |
| Opening/closing stock for a period | `StokPeriodikDal` + IF8 form logic |
| Customer last Faktur aggregation pattern | `CustomerLastFakturDal` |
| Attention UX patterns and components | M16/M17/M18 portal |
| Materialized snapshot refresh infrastructure | `btr.portal.worker`, refresh log |
| Executive concentration % | `DashboardExecutiveComposer` |

### 11.2 Information partially available

| Information | Gap |
| ----------- | --- |
| Item last sale date | SQL path clear; no DAL or DTO |
| Item sales velocity (period qty) | `FakturBrgView` requires period scan; not aggregated |
| Zero movement / MovingStok at company level | IF8 per-warehouse; not BrgId-first portal aggregate |
| Dead vs slow classification | **PO approved** — 90d / 180d Last Faktur rules; implementation pending |
| Inventory aging buckets | **PO approved** pie chart required; bucket boundaries for architect |
| Item-level portal drill-down | Inventory Report has balance; no sales lines in portal |
| Retur-adjusted net demand | Desktop DAL exists; not portal |
| Purchase date per item | INVOICE mutasi inferable; no "last purchase date" DAL |
| BrgId on `FakturBrgView` | In SQL, omitted from DTO |

### 11.3 Information not currently available

| Information | Notes |
| ----------- | ----- |
| Pre-built slow moving / dead stock KPIs | Net-new analytics |
| ABC classification | Explicitly deferred |
| Inventory turnover / DSO-equivalent | Not implemented |
| Warehouse-level dashboard breakdown | Deferred |
| Item-level portal report route | No `/reports/faktur-brg` equivalent |
| Snapshot tables for movement/aging | **`BTR_PortalDashboardInventoryRisk*` approved** — not yet built |
| Item-level Last Faktur DAL | **`IBrgLastFakturDal` approved** — not yet built |
| Executive inventory health signals | **Approved post-M19** — Dead Stock Value, At-Risk %, attention indicator |
| Stok Harian analytics | Form is empty stub |
| Salesman × inventory cross analytics | Explicitly out of scope (M18 Q43) |

---

## 12. Product Owner Decisions

**All open questions resolved.** Product Owner decisions recorded 2026-06-08. These are authoritative for architecture and implementation planning.

### 12.1 Product scope and routing

| # | Decision |
| - | -------- |
| Q1 | Dedicated route: **`/dashboard/inventory-risk`** |
| Q2 | **Promote inventory-risk KPIs to Executive Dashboard after M19 is implemented** |
| Q3 | **Supplement** existing Inventory Dashboard; **do not replace** |
| Q4 | Page title: **Slow Moving & Dead Stock Dashboard** |

### 12.2 Business definitions

| # | Decision |
| - | -------- |
| Q5 | **Dead Stock** = `Qty > 0` AND no Faktur for **180 days** |
| Q6 | **Slow Moving** = `Qty > 0` AND no Faktur for **90 days** |
| Q7 | Reuse M17 dormant **pattern** with inventory thresholds: **90 days** (Slow Moving), **180 days** (Dead Stock) |
| Q8 | Authoritative signal = **Last Faktur Date** (item grain) |
| Q9 | **Gross Faktur only** — retur does not affect aging classification |
| Q10 | Non-sales outflows (mutasi, opname, adjust) **do not reset** aging clock |
| Q11 | **Never Sold** is a **separate attention signal** (not merged into Dead Stock) |
| Q12 | **No minimum qty/value floor** for V1 |

**Approved classification rules (summary):**

| Class | Rule | Value basis |
| ----- | ---- | ----------- |
| **Never Sold** | Aggregated `Qty > 0`; **no** non-void FakturItem history | `Hpp × Qty` (BrgId-first, exclude In-Transit) — **separate signal (Q11)** |
| **Slow Moving** | Aggregated `Qty > 0`; `LastFakturDate` exists AND `LastFakturDate <= today − 90` | Same valuation |
| **Dead Stock** | Aggregated `Qty > 0`; `LastFakturDate` exists AND `LastFakturDate <= today − 180` | Same valuation |
| **At-Risk Inventory %** | Headline KPI — `(At-Risk Value ÷ Total Inventory Value) × 100%`; **must reconcile** with Inventory Dashboard denominator (Q33) |

**Architect note:** PO defines Slow Moving at ≥ 90 days and Dead Stock at ≥ 180 days since **Last Faktur Date**. Dead Stock ⊆ Slow Moving when both apply to the same item. Confirm whether Slow Moving **Value/Count** KPIs report the full ≥ 90-day population or the 90–179 day band only; both KPI families are mandatory separately (Q18).

**Architect note:** Never-sold items (Q11) carry the **Never Sold** attention signal; confirm overlap with Slow/Dead KPI counts (e.g. Never Sold excluded from Slow/Dead counts to avoid triple classification).

**Architect note:** Aging pie chart bucket boundaries were not specified by PO — propose buckets aligned with 90/180-day thresholds (e.g. Active ≤ 90, Slow 91–180, Dead > 180, Never Sold) for sign-off during planning.

### 12.3 Aggregation and period

| # | Decision |
| - | -------- |
| Q13 | **Company-wide** aggregation — existing BrgId-first portal philosophy |
| Q14 | **Continue excluding In-Transit** warehouse |
| Q15 | **Rolling window** based on Last Faktur Date |
| Q16 | Company-wide aggregation allowed even though Desktop IF8 is warehouse-based |
| Q17 | Use **`BTR_Brg.Hpp`** for valuation — portal consistency |

### 12.4 KPI and UX priorities

| # | Decision |
| - | -------- |
| Q18 | **Mandatory KPIs:** Dead Stock Item Count, Dead Stock Value, Slow Moving Item Count, Slow Moving Value, At-Risk Inventory % |
| Q19 | **Aging pie chart required** |
| Q20 | Attention List grain = **Item × Signal** |
| Q21 | **Top 10 Dead Stock** and **Top 10 Slow Moving** — both mandatory |
| Q22 | Include **At-Risk Category** and **At-Risk Supplier** concentration analysis |
| Q23 | **Generic Attention Indicator** model (same as M16–M18) |
| Q24 | **At-Risk Inventory %** is a **headline KPI** |

### 12.5 Data sourcing and materialization

| # | Decision |
| - | -------- |
| Q25 | Dedicated snapshot domain: **`BTR_PortalDashboardInventoryRisk*`** |
| Q26 | Refresh cadence: **60 minutes** |
| Q27 | Dashboard uses **snapshot-only** architecture |
| Q28 | Approve new item-level **Last Faktur DAL** following `CustomerLastFakturDal` pattern |
| Q29 | Full **`FakturItem` history scan** during snapshot refresh is **acceptable** |

### 12.6 Drill-down and reports

| # | Decision |
| - | -------- |
| Q30 | Use existing **Inventory Report** for V1 drill-down |
| Q31 | Kartu Stok transaction detail **remains deferred** |
| Q32 | Inventory Report pre-filter **`?q=`** is sufficient for V1 |
| Q33 | **Total Inventory Value** and **At-Risk %** must reconcile with Inventory Dashboard and Inventory Report footer |

### 12.7 Explicit exclusions

| # | Decision |
| - | -------- |
| Q34 | **No salesman dimension** in M19 |
| Q35 | **ABC classification** remains deferred |
| Q36 | **Warehouse breakdown** remains deferred |
| Q37 | **StokBalanceHealth** KPI remains out of scope |
| Q38 | **Export** features remain deferred |

### 12.8 Localization and labels

| # | Decision |
| - | -------- |
| Q39 | English labels consistent with portal: **Dead Stock**, **Slow Moving**, **Inventory Risk** |
| Q40 | Tooltips and descriptions may use **Indonesian business language** if needed |

---

## 13. Executive Dashboard Integration (Post-M19)

Per Q2, after M19 delivery the **Management Attention Center** (`/dashboard`) extends inventory from **composition** to **health**:

| New executive metric | Source | Business meaning |
| -------------------- | ------ | ---------------- |
| **Dead Stock Value** | Inventory Risk snapshot | Capital tied up in items with no Faktur for ≥ 180 days |
| **At-Risk Inventory %** | Inventory Risk snapshot ÷ Total Inventory Value | Share of inventory capital requiring attention |
| **Inventory Risk Attention Indicator** | `*RequiresAttention` on inventory-risk card | Generic M16–M18 attention presentation |

**Preserved (unchanged):** Total Inventory Value, Top Category %, Top Supplier % — existing concentration metrics from M15/M16.

**Scope boundary:** Executive changes are **downstream of M19** — implement inventory-risk dashboard first, then compose executive signals from the new snapshot domain.

---

## Appendix A — Relationship to Other Milestones

| Milestone | Relationship to M19 |
| --------- | ------------------- |
| M15 Inventory Dashboard | Composition baseline; share aggregation rules and total value denominator |
| M16 Executive Dashboard | **Post-M19:** promote Dead Stock Value, At-Risk %, inventory risk attention indicator; preserve concentration metrics |
| M17 Customer Analytics | Dormant threshold pattern; attention UX; no direct data overlap |
| M18 Salesman Performance | **No salesman dimension** on inventory risk; explicit scope boundary |
| M20 Collection Dashboard | Separate finance/collection focus |
| M25 Sales Force Effectiveness | Route/field activity — not inventory obsolescence |

---

## Appendix B — Key File Index

| Path | Role |
| ---- | ---- |
| `src/j05-btr-distrib/btr.application/ReportingContext/DashboardSnapshotAgg/Services/DashboardInventoryAggregator.cs` | Portal inventory composition rules |
| `src/j05-btr-distrib/btr.infrastructure/ReportingContext/InventoryReportAgg/InventoryReportDal.cs` | Portal inventory report |
| `src/j05-btr-distrib/btr.infrastructure/InventoryContext/KartuStokRpt/KartuStokSummaryDal.cs` | Movement bucket SQL |
| `src/j05-btr-distrib/btr.distrib/InventoryContext/KartuStokRpt/KartuStokSummaryForm.cs` | MovingStok formula (Desktop) |
| `src/j05-btr-distrib/btr.infrastructure/SalesContext/FakturInfoAgg/FakturBrgViewDal.cs` | Item sales lines |
| `src/j05-btr-distrib/btr.infrastructure/SalesContext/FakturInfoAgg/CustomerLastFakturDal.cs` | Last-event aggregation pattern |
| `src/j05-btr-distrib/btr.application/ReportingContext/DashboardSnapshotAgg/Services/DashboardCustomerAggregator.cs` | Dormant + attention list pattern |
| `src/j05-btr-distrib/btr.application/ReportingContext/DashboardExecutiveAgg/Services/DashboardExecutiveComposer.cs` | Executive inventory section |
| `src/j05-btr-distrib/btr.portal.web/src/views/dashboard/InventoryDashboardView.vue` | Current inventory dashboard UI |
| `docs/features/btr-portal/btr-portal-domain.md` | Approved inventory KPIs and deferred capabilities |
| `docs/work/btr-portal/M16 Executive Dashboard - Analysis.md` | Prior gap analysis for slow/dead stock |

---

## Appendix C — Verification Scenarios (for acceptance planning)

After implementation, business acceptance should be able to answer:

1. For a known idle item in Desktop Faktur Brg Info, does the dashboard flag it with correct days since last Faktur?
2. **Dead Stock** items have `LastFakturDate <= today − 180` (or Never Sold per Q11); **Slow Moving** items have `LastFakturDate <= today − 90`.
3. Does **Total Inventory Value** on M19 page match Inventory Dashboard and Inventory Report footer?
4. Does **At-Risk Inventory %** use the same Total Inventory Value denominator as M15?
5. Does excluding In-Transit match Desktop IF1 with In-Transit unchecked?
6. For an item with Faktur in the last 89 days, is it excluded from Slow Moving and Dead Stock signals?
7. Never Sold items appear as **Never Sold** signal, not conflated with Dead Stock unless PO rules dictate otherwise.
8. Category/supplier at-risk exposure bars reflect sum of at-risk item values by dimension.
9. Inventory Report drill-down via `?q=` locates flagged item rows.
10. After M19 + executive update: Executive Dashboard shows Dead Stock Value, At-Risk %, and inventory risk attention indicator.

---

*End of analysis — M19 Slow Moving & Dead Stock Dashboard*
