# BTR Portal Analysis — M22 Branch / Warehouse Performance Dashboard

**Status:** Product scope approved — implementation plan delivered.  
**Implementation plan:** [M22 Branch Warehouse Performance - Plan.md](./M22%20Branch%20Warehouse%20Performance%20-%20Plan.md)  
**Scope:** Business analysis only. No implementation plans, API design, database design, or code.  
**Date:** 2026-06-09 (analysis) · Product Owner decisions recorded 2026-06-09  
**Context:** BTR Portal V2 (M16–M21 complete or in delivery) follows a management philosophy: *What requires management attention?* M22 introduces the **Branch / Warehouse Performance Dashboard** at `/dashboard/locations` — a **location concentration** view answering: *Are we becoming too dependent on a particular warehouse or territory?*

**Internal milestone purpose (PO):** **Location Concentration Dashboard** — approved KPIs measure inventory, sales, purchasing, and at-risk **concentration** by location, not operational productivity.

**Approved roadmap position (inferred from M16–M21 sequence):** M17 Customer Analytics → M18 Salesman Performance → M19 Slow Moving & Dead Stock → M20 Collection Dashboard → M21 Purchasing Management Dashboard → **M22 Branch / Warehouse Performance** → … → M23 Alert Center → … → M25 Sales Force Effectiveness

**Reference documents:** `docs/foundation/PRODUCT.md`, `docs/foundation/DOMAIN.md`, `docs/foundation/LANDSCAPE.md`, `docs/foundation/WORKFLOW.md`, `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-operational.md`, `docs/features/btr-portal/btr-portal-architecture.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md`, `docs/work/btr-portal/M16 Executive Dashboard - Analysis.md`, `docs/work/btr-portal/M17-Customer-Analytics-Analysis.md`, `docs/work/btr-portal/M18 Salesman Performance - Analysis.md`, `docs/work/btr-portal/M19 Slow Moving & Dead Stock - Analysis.md`, `docs/work/btr-portal/M20-collection-dashboard-analysist.md`, `docs/work/btr-portal/M21-purchasing-dashboard-analysis.md`, `docs/archive/materialize-dashboard-data/analysis-report.md`, `btr-reporting-investigation.md`

---

## 1. Executive Summary

BTR Portal today provides strong **entity-centric** management analytics (customer, salesman, supplier/principal, item) and **company-level** domain dashboards (sales, piutang, inventory, purchasing, collection). **Wilayah** (sales territory) appears as a **segmentation and ranking dimension** in Customer Analytics (M17), Salesman Performance (M18), and Collection Dashboard (M20). **Warehouse** appears only at **row level** in Inventory Report (Item × Warehouse) and Purchasing Report (Invoice × Warehouse). No portal dashboard aggregates KPIs by warehouse, and no cross-domain **location attention** view exists.

**Critical discovery — "Branch" is not a BTR business concept:** There is no `BTR_Branch`, `BranchId`, or branch master in the schema. Planning documents use "branch" informally to mean **warehouse-level imbalance**. The authoritative location concepts in BTR are **Warehouse** (logical inventory / billing origin), **Wilayah** (customer and salesman territory), and **Depo** (physical storage for picking/packing — operational, not an analytics axis).

**Critical discovery — Warehouse and Wilayah measure different things:** A Faktur belongs to exactly one **Warehouse** (`BTR_Faktur.WarehouseId`) while customer **Wilayah** comes from `BTR_Customer.WilayahId`. Salesman **Wilayah** comes from `BTR_SalesPerson.WilayahId`. These dimensions are **not hierarchical** and may **diverge** (cross-wilayah selling, warehouse serving multiple territories). M22 must treat them as **parallel location lenses**, not assume Branch = Warehouse = Wilayah.

**Critical discovery — Wilayah receivable analytics are owned by M20:** Collection Dashboard already materializes **Top Overdue Wilayah**, **WilayahHotspot** (≥ 15% of company overdue), and wilayah rows on the attention list. M22 must **cross-link** to M20 for collection-oriented wilayah signals — not duplicate recovery effectiveness or overdue rankings.

**Critical discovery — Inventory and risk analytics roll up across warehouses:** M15 Inventory Dashboard and M19 Inventory Risk Dashboard use **BrgId-first aggregation** that **sums qty/value across all warehouses** (excluding In-Transit). Warehouse breakdown was **explicitly deferred** in M15, M16, M19, and M21. M22 is the natural milestone to introduce **warehouse-level inventory exposure** without changing M15/M19 company-wide semantics.

**Critical discovery — Warehouse productivity metrics are partially supported on Desktop only:** `KartuStokSummaryDal` + `KartuStokSummaryForm` compute per-warehouse period movement (`MovingStok = Invoice + Faktur + Retur + Mutasi + Opname`) with opening/closing stock from `StokPeriodikDal`. **Sales by warehouse** is derivable from `FakturViewDal` (`WarehouseName` on every Faktur). **Inventory turnover** and **utilization ratios** have **no implemented formula** in BTR. Do not invent turnover KPIs without PO approval.

### Key findings

| Finding | Implication |
| ------- | ----------- |
| **Wilayah analytics are mature** in M17/M18/M20 snapshots | M22 regional lens should **compose and cross-link**, not rebuild wilayah overdue/collection logic |
| **Warehouse analytics are row-level only** in portal | M22 requires **new warehouse aggregates** — primary greenfield within location analytics |
| **Branch does not exist** | Dashboard title may say "Branch / Warehouse" for management language; **implementation dimension = Warehouse + Wilayah** |
| **Depo is operational** (supplier default, packing routing) | **Excluded from M22** (Q4) |
| **Piutang has no warehouse dimension** in any report or dashboard | Receivable exposure by location = **Wilayah** (customer territory), not billing warehouse |
| **Sales Report and Piutang Report drop location columns** | Source DALs have dimensions; **M22 does not extend report APIs** (Q27) — drill-down via Inventory Report warehouse filter |
| **In-Transit warehouse excluded** from portal inventory (`WarehouseName = "In-Transit"`) | M22 **excludes In-Transit and `IsSpecial`** from warehouse universe (Q5) |
| **`IsSpecial` warehouses filtered** in Desktop browser (`WarehouseBrowser` excludes `IsSpecial == true`) | **Approved:** exclude `IsSpecial = true` from rankings; inactive warehouses only in attention signals (Q5) |
| **`IsAktif` on `BTR_Warehouse` table** exists in SQL but **not** on `WarehouseModel` | **Approved signal:** `WarehouseInactiveWithStock` = `IsAktif = false` AND inventory > 0 (Q18); DAL extension required |
| **Inter-warehouse transfer exists** (`MutasiKeluar`, `MutasiMasuk`, `KlaimSupplier`) | Imbalance correction workflow exists; **no aggregate transfer KPI** discovered |
| **No M23 Alert Center implementation** found | M22 signals must be **M23-compatible** (Q24) |

**All open questions resolved.** See Section 16 for authoritative Product Owner decisions.

### Approved product outcome

Deliver **Branch / Warehouse Performance Dashboard** at **`/dashboard/locations`** (page title: **Branch / Warehouse Performance Dashboard**). Internally: **Location Concentration Dashboard** — dimensions **Warehouse** + **Wilayah**.

Answer:

> Are we becoming too dependent on a particular warehouse or territory?

**Management theme:** **Location concentration** — inventory, sales, purchasing, and at-risk exposure concentrated at specific warehouses; sales contribution concentrated by wilayah. **Not** operational productivity (no turnover, movement, or warehouse targets in V1).

**Dual equal framing (Q2):** Warehouse answers logistics and inventory; Wilayah answers commercial geography. Neither replaces the other.

**Mandatory V1 sections (fixed order, Q9):**

1. Location Attention Cards  
2. Top Warehouse by Inventory  
3. Top Warehouse by At-Risk Inventory  
4. Top Warehouse by Sales  
5. Top Warehouse by Purchasing  
6. Top Wilayah by Sales  
7. Location Attention List (Warehouse × Signal only)  
8. Navigation (mandatory cross-links per Q30)

**Dedicated snapshot domain:** `BTRPD_Location*` (e.g. warehouse rankings, attention rows) — **60-minute refresh** (Q25, Q26). **Dashboard-only** — no Sales Report or Piutang Report API changes (Q27).

**Concentration display:** Top Warehouse % and Top 3 Warehouse % — **informational only**; no `WarehouseHotspot` auto-threshold in V1 (Q17).

**Explicitly out of M22 V1:** Depo, piutang by warehouse, movement KPIs (V2 candidate), productivity ratios, warehouse sales targets, CrossWilayahSelling, warehouse imbalance (SKU-level), qualified backlog by warehouse (M21 link only), embedded Top Overdue Wilayah (M20 link only), executive promotion (Phase 2), report API extensions.

---

## 2. Management Attention Discovery

This section identifies location-related situations that typically require management intervention, mapped to **existing BTR capabilities**. Items marked **Portal today** are in current portal snapshots or reports. **Desktop only** exists in BTR Desktop but is not portal-exposed. **Not available** means no implemented logic was discovered. **Derivable** means source data exists but no aggregate KPI is computed. **Owned by Mxx** means another milestone already delivers the signal — M22 should cross-link, not duplicate.

### 2.1 Warehouse — inventory and capital exposure

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Inventory value concentrated in one warehouse** | Capital and obsolescence risk localized; operational dependency on one site | `StokBalanceViewDal` — row grain Item × Warehouse; M15 sums **across** warehouses | **Derivable** — not aggregated |
| **Warehouse holds disproportionate share of at-risk inventory** | Dead/slow/never-sold capital concentrated at one site | M19 classifies at **company/item** level; `StokBalanceView` has `WarehouseName` | **Derivable** — join M19 item classification × warehouse balance |
| **Warehouse overstock while peer warehouses understocked** | Internal imbalance — transfer or replenishment misallocation | Same item may exist in multiple warehouses via `BTR_StokBalanceWarehouse`; `MutasiMasuk`/`MutasiKeluar` workflow | **Partial** — row-level IF1; no imbalance KPI |
| **Inactive warehouse still holding stock** | Legacy location tying up capital | `BTR_Warehouse.IsAktif`; stock in `BTR_StokBalanceWarehouse` | **Partial** — DB column exists; not in `WarehouseModel` or portal |
| **In-Transit warehouse buildup** | Goods not sellable but holding value | In-Transit excluded from portal; Desktop IF1 optional checkbox | **Desktop only** — deliberately hidden in portal |
| **Special warehouse dominating metrics** | Skews location rankings | `WarehouseBrowser` filters `IsSpecial == false` | **Partial** — Desktop pattern; portal has no equivalent filter |
| **Zero-qty warehouse still in master** | Master data noise | Warehouse master IM2; stock rows filtered `Qty > 0` in reports | **Partial** |
| **Warehouse with no recent stock movement** | Low operational activity — stale or dormant site | `KartuStokSummaryDal` per warehouse + period; `MovingStok == 0` with `StokAkhir > 0` highlighted in Desktop | **Desktop only** |
| **High mutasi (transfer) volume at warehouse** | Heavy internal redistribution — may indicate imbalance | `KartuStokSummaryDal` Mutasi bucket; `MutasiBrgViewDal` | **Desktop only** — not aggregated |

### 2.2 Warehouse — sales and billing origin

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Sales omzet concentrated in one billing warehouse** | Revenue dependency on one distribution point | `FakturView.WarehouseName` on every Faktur; Sales Dashboard aggregates **company-wide** | **Derivable** — `SUM(GrandTotal)` grouped by warehouse, current month |
| **Warehouse with sales but minimal inventory** | Billing location not aligned with stock position — fulfillment risk | Join Faktur warehouse × StokBalance warehouse | **Derivable** — not computed |
| **Warehouse with inventory but no MTD sales** | Stock site without billing activity — possible dead branch | Faktur month filter + StokBalance by warehouse | **Derivable** |
| **Warehouse sales deceleration vs peers** | Location losing billing pace mid-month | Weekly trend exists **company-wide** only (`BTRPD_SalesWeekTrend`) | **Not available** per warehouse |
| **Single warehouse dominates Faktur volume** | Operational bottleneck at one site | Count Faktur by `WarehouseId` | **Derivable** |

### 2.3 Warehouse — purchasing and intake

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Purchasing concentrated into one warehouse** | Replenishment dependency on one receiving site | `InvoiceView.WarehouseName`; Purchasing Report column; M21 aggregates **by principal** not warehouse | **Partial** — row-level in Purchasing Report |
| **High-value BELUM invoices for one warehouse** | Unposted purchases destined for one site | M21 qualified backlog by principal; invoice has `WarehouseId` | **Derivable** — group M21 backlog by warehouse |
| **Warehouse receiving purchases but low sales outflow** | Intake without sell-through at site | Invoice warehouse + Faktur warehouse + Kartu Stok Faktur bucket | **Derivable** — complex join |

### 2.4 Wilayah — commercial territory performance

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Wilayah with high overdue receivable concentration** | Regional collection problem | M20 `TopOverdueWilayah`, `WilayahHotspot` (≥ 15%) | **Portal today** — **Owned by M20** |
| **Wilayah with low sales contribution** | Territory underperforming vs peers | `FakturView.WilayahName`; M17 `ByWilayah` segmentation **counts only** | **Partial** — counts, not omzet ranking by wilayah |
| **Wilayah customer concentration** | Few customers dominate territory revenue | M17 Top Customer + `WilayahName` on Faktur | **Derivable** |
| **Cross-wilayah selling** | Salesman/customer wilayah mismatch — policy or data issue | `Customer.WilayahId` vs `SalesPerson.WilayahId` on Faktur | **Not computed** — noted in M18 analysis |
| **Wilayah with dormant customer cluster** | Inactive accounts concentrated regionally | M17 dormant + `WilayahName` | **Derivable** |
| **Salesman wilayah underperformance** | Territory assigned to rep underperforms | M18 `ByWilayah` segmentation (rep counts by wilayah) | **Partial** — rep distribution, not territory omzet vs target |
| **Wilayah with high at-risk inventory exposure (customers)** | Commercial geography correlated with stock risk | No direct link — inventory has no wilayah | **Not available** without arbitrary join |

### 2.5 Cross-location imbalance and dependency

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Sales concentration + inventory concentration in same warehouse** | Single-site operational dependency | Independent Top-N rankings composable | **Derivable** |
| **High wilayah sales + high wilayah overdue** | Territory generating revenue and collection risk simultaneously | M20 overdue wilayah + Faktur wilayah omzet | **Derivable** — cross-domain location signal |
| **Purchasing into warehouse A, selling from warehouse B** | Split supply chain — transfer latency risk | Faktur vs Invoice warehouse IDs differ | **Derivable** — not computed |
| **Company-wide metrics mask location crisis** | One location failing while company totals look healthy | All current dashboards aggregate company-wide | **Motivation for M22** |

### 2.6 Workflow-derived attention points

From `docs/foundation/DOMAIN.md`, `docs/foundation/WORKFLOW.md`, and Desktop menus:

| Workflow stage | When management cares about location | Portal support today |
| -------------- | ------------------------------------ | -------------------- |
| **Faktur creation** — warehouse selection | Which site books the sale | Row-level in `FakturView`; not in Sales Report API |
| **Stock fulfillment** — picking/packing from Depo | Physical throughput by site | **None** — Depo not in analytics |
| **Purchase posting (PT2)** — warehouse receipt | Goods entering specific warehouse | Purchasing Report `Warehouse` column |
| **Mutasi** — inter-warehouse transfer | Rebalancing stock between sites | Desktop Mutasi Info only |
| **Stok Opname** — per warehouse count | Local accuracy / shrinkage | Desktop Kartu Stok / Opname mutasi types |
| **Collection** — customer wilayah | Regional receivable risk | **M20** — WilayahHotspot, Top Overdue Wilayah |
| **Salesman territory (Wilayah master SM3)** | Rep accountability by region | M18 segmentation counts |

---

## 3. Business Structure Discovery

### 3.1 Authoritative location concepts in BTR

| Concept | Domain definition (`DOMAIN.md`) | Schema / master | Used on transactions | Suitable for management analytics? |
| ------- | ------------------------------ | --------------- | ---------------------- | ---------------------------------- |
| **Warehouse (Gudang)** | Logical grouping of inventory; **not necessarily a physical building**; Faktur belongs to exactly one Warehouse | `BTR_Warehouse` (`WarehouseId`, `WarehouseName`, `IsSpecial`, `IsAktif`) | `BTR_Faktur`, `BTR_Invoice`, `BTR_Stok`, `BTR_StokBalanceWarehouse`, `BTR_Mutasi`, `BTR_ReturJual`, `BTR_Packing` | **Yes — primary logistics/billing location axis** |
| **Wilayah** | Not in DOMAIN.md glossary; operational **sales territory** | `BTR_Wilayah` (`WilayahId`, `WilayahName`) | `BTR_Customer.WilayahId`, `BTR_SalesPerson.WilayahId`; joined on Faktur via Customer | **Yes — primary commercial territory axis** |
| **Depo** | Physical storage location for pick/pack | `BTR_Depo` (`DepoId`, `DepoName`) | `BTR_Supplier.DepoId`; `BTR_PackingOrderDepo` | **No for M22 V1** — operational routing, not management rollup |
| **Branch** | **Not defined** | **No entity** | **No `BranchId`** | **Documentation alias only** — treat as informal label for Warehouse |
| **Territory** | Documentation synonym for **Wilayah** (M18 analysis) | Same as Wilayah | Same as Wilayah | Same as Wilayah |
| **Kota (city)** | Customer address geography | `Customer` address fields | `CustomerChartRpt` groups by **Kota**, not Wilayah | **Different from Wilayah** — not recommended as primary axis |

### 3.2 Branch vs Warehouse — investigation conclusion

| Question | Finding |
| -------- | ------- |
| Does Branch exist in BTR? | **No** — no master table, no foreign key, no DAL |
| Does Warehouse act as a branch proxy? | **Partially** — in distribution businesses, each Warehouse often corresponds to a distribution center users call "cabang." BTR models it as **inventory/billing origin**, not organizational hierarchy |
| Is territory-based analysis more appropriate for some questions? | **Yes** for sales reach, customer concentration, and **receivable geography** (customer Wilayah). **No** for inventory position, purchasing receipt, and **billing warehouse** analysis |
| Can one Warehouse serve multiple Wilayah? | **Yes** — no constraint linking Warehouse to Wilayah |
| Can one Wilayah span multiple Warehouses? | **Yes** — customers in same Wilayah may be invoiced from different warehouses |
| Should M22 use a single "location" dimension? | **No** — PO must decide presentation, but **analytics require both Warehouse and Wilayah** with clear semantic labels |

### 3.3 Warehouse master attributes

| Field | Business meaning | Analytics use |
| ----- | ---------------- | ------------- |
| `WarehouseId` | Primary key | Grouping key for all warehouse aggregates |
| `WarehouseName` | Display name | Rankings, attention list, report pre-filter |
| `IsSpecial` | Special-purpose warehouse (Desktop browser excludes from default selection) | Candidate exclusion rule (In-Transit may be special) |
| `IsAktif` | Active flag in SQL | **Inactive warehouse** attention candidate — not exposed in `WarehouseModel` today |

**Gap:** `WarehouseModel` exposes `IsSpecial` but **not** `IsAktif`. Portal cannot filter inactive warehouses without DAL extension or direct SQL.

### 3.4 Wilayah assignment semantics

| Entity | Wilayah source | Meaning |
| ------ | -------------- | ------- |
| **Customer** | `BTR_Customer.WilayahId` | Customer's commercial territory / geographic segment |
| **SalesPerson** | `BTR_SalesPerson.WilayahId` | Salesman's assigned territory |
| **Faktur (reporting)** | Join Customer → Wilayah | **Customer Wilayah** on `FakturView.WilayahName` — not salesman wilayah |
| **Open Piutang (M20)** | `Customer.WilayahId` via `PiutangOpenBalanceWithWilayahDal` | Receivable geography follows **customer** territory |

**Important:** Wilayah on Faktur reflects **customer master**, not **salesman territory**. Cross-wilayah analysis (customer wilayah ≠ salesman wilayah) is detectable but **not computed** anywhere today.

### 3.5 Depo — why not a dashboard dimension

| Evidence | Implication |
| -------- | ----------- |
| Depo linked to **Supplier** default and **PackingOrder** routing | Supports warehouse fulfillment workflow |
| No Depo column on Faktur, Invoice, StokBalance, or portal reports | No consistent analytic grain |
| DOMAIN.md distinguishes Depo (physical) from Warehouse (logical) | M22 should respect domain boundary |

**Recommendation:** **Excluded from M22 V1** (Q4) — confirmed by Product Owner.

---

## 4. Existing Dashboard Reuse Analysis

### 4.1 Management Attention Center (`/dashboard`)

| Metric | Location dimension? | M22 relationship |
| ------ | ------------------- | ---------------- |
| Sales Achievement % | No | Company-wide |
| Piutang / Overdue / >90d | No | Company-wide |
| Top Customer / Category / Supplier / Principal % | No | Entity concentration, not location |
| Pending Posting (Purchasing) | No | Company-wide |
| Inventory Top Category % | No | Company-wide composition |

**Assessment:** Executive dashboard has **no location dimension**. M22 signals are **candidates for future executive promotion** (Phase 2) — similar to M19/M20/M21 deferred executive integration.

### 4.2 Sales Dashboard (`/dashboard/sales`)

| KPI / section | Location data | Reuse for M22 |
| ------------- | ------------- | ------------- |
| Total Omzet / Achievement | Company-wide | Denominator for **warehouse / wilayah share %** |
| Weekly trend | Company-wide | Pattern for **per-location weekly series** (new) |
| Top 10 Salesman | No location | Salesman carries `WilayahId` on master — indirect |

**Hidden source data:** `IFakturViewDal` / `DashboardSalesFakturAggregator` read Faktur rows with `WarehouseName` and `WilayahName` — **not persisted** in sales snapshot breakdown.

### 4.3 Piutang Dashboard (`/dashboard/piutang`)

| KPI / section | Location data | Reuse for M22 |
| ------------- | ------------- | ------------- |
| All KPIs | Company-wide | No wilayah breakdown |
| Top 10 Customers | Customer grain | Customer has Wilayah on master — **not shown** |

**Source:** `DashboardPiutangAggregator` uses open balance DAL **without** wilayah dimension in snapshot.

### 4.4 Customer Analytics (M17)

| KPI / section | Location data | Reuse for M22 |
| ------------- | ------------- | ------------- |
| **By Wilayah segmentation** | Count of customers by `WilayahName` | **Partial reuse** — activity/dormant counts, not omzet or piutang by wilayah |
| Attention list | `WilayahName` context column | UX pattern for location context on rows |
| Top 10 Omzet / Piutang | No wilayah column on ranking | Wilayah derivable from customer master |

**Boundary:** M17 owns **customer × signal**. M22 owns **location × cross-domain signal** (wilayah or warehouse as primary entity).

### 4.5 Salesman Performance (M18)

| KPI / section | Location data | Reuse for M22 |
| ------------- | ------------- | ------------- |
| **By Wilayah segmentation** | Salesman count per `SalesPerson.WilayahId` | **Rep distribution** by territory — not territory performance |
| Attention list | `WilayahName` on rows | Context column pattern |
| Top 10 Omzet / Piutang | Salesman grain | Aggregate by salesman wilayah is **derivable** |

**Boundary:** M18 owns **salesman × signal**. M22 may show **wilayah-level sales contribution** without duplicating salesman attention lists.

### 4.6 Collection Dashboard (M20)

| KPI / section | Location data | Reuse for M22 |
| ------------- | ------------- | ------------- |
| **Top Overdue Wilayah** | Ranked by overdue balance | **Owned by M20 — cross-link, do not duplicate** |
| **WilayahHotspot** signal | Wilayah overdue ≥ 15% of company overdue | **Owned by M20** |
| `WilayahHotspotCount` KPI | Count of hotspot wilayah | **Owned by M20** |
| Recovery vs Billing % | Company-wide only | Not location-decomposed |

**Assessment:** M20 is the **authoritative portal source** for **wilayah receivable risk**. M22 regional panel should **link to Collection Dashboard** for overdue/wilayah detail.

### 4.7 Inventory Dashboard (M15)

| KPI / section | Location data | Reuse for M22 |
| ------------- | ------------- | ------------- |
| Total Inventory Value / Total Item | **Summed across warehouses** | Denominator for warehouse **share %** |
| Category / Supplier breakdown | No warehouse | M22 adds **warehouse breakdown** — deferred from M15 |
| Top 10 Category / Supplier | No warehouse | Parallel **Top 10 Warehouse by value** (new) |

**Reuse:** `DashboardInventoryItemGroupBuilder` rules (BrgId-first, In-Transit exclusion, HPP × Qty, Unknown dimensions).

### 4.8 Inventory Risk Dashboard (M19)

| KPI / section | Location data | Reuse for M22 |
| ------------- | ------------- | ------------- |
| At-risk value / counts | **Company-wide item classification** | **Compose:** sum at-risk item value **by warehouse** from `IStokBalanceViewDal` × M19 classification — do not reclassify items |
| Category / Supplier risk exposure | No warehouse | Parallel **Warehouse risk exposure** ranking (derivable) |
| Attention list | Item × signal | Item rows include warehouse in Inventory Report — drill-down path |

**Boundary:** M19 owns **item movement classification**. M22 owns **where** at-risk capital sits (warehouse concentration).

### 4.9 Purchasing Management Dashboard (M21)

| KPI / section | Location data | Reuse for M22 |
| ------------- | ------------- | ------------- |
| Top 10 Principal | No warehouse | Principal spend is company-wide |
| Qualified backlog | No warehouse breakdown | **Derivable** by `Invoice.WarehouseId` |
| Purchasing Report | `WarehouseName` per row | Validation path for warehouse purchasing |

**Boundary:** M21 owns **supplier/principal purchasing attention**. M22 may add **warehouse purchasing concentration** as a location signal.

### 4.10 Reuse summary

| Reuse candidate | Confidence | Rationale |
| --------------- | ---------- | --------- |
| M20 wilayah overdue rankings / WilayahHotspot | **High** (read-only cross-link) | Do not recompute |
| M17/M18 `ByWilayah` segmentation pattern | **High** | Extend to omzet/piutang amounts, not just counts |
| `DashboardInventoryItemGroupBuilder` | **High** | Warehouse rollup must respect BrgId-first and In-Transit rules |
| `IFakturViewDal` | **High** | Sales by warehouse and by customer-Wilayah |
| `IStokBalanceViewDal` | **High** | Inventory by warehouse |
| `IInvoiceViewDal` | **High** | Purchasing by warehouse |
| `DashboardCollectionAggregator` WilayahHotspot threshold (15%) | **Medium** | Pattern for warehouse concentration hotspot — **new threshold needs PO** |
| `KartuStokSummaryDal` | **Medium** | Movement by warehouse — Desktop period filter; expensive for snapshot |
| M19 item classification output | **High** | Join for at-risk-by-warehouse |
| `ExecutiveAttentionCard`, `Top10RankingTable`, attention list UX | **High** | M17/M18/M20/M21 proven patterns |

---

## 5. Warehouse Analysis

### 5.1 Inventory position by warehouse

| Indicator | Source | Portal today | Management meaning |
| --------- | ------ | ------------ | ------------------ |
| **Qty by warehouse** | `BTR_StokBalanceWarehouse` via `StokBalanceViewDal` | Inventory Report rows | Physical stock presence |
| **Value by warehouse** | `Hpp × Qty` per row, sum by `WarehouseName` | **Not aggregated** | Capital tied up per site |
| **Share of company inventory %** | Warehouse value ÷ M15 `TotalInventoryValue` | **Derivable** | **Concentration risk** |
| **Item count by warehouse** | Distinct `BrgId` with qty > 0 per warehouse | **Derivable** | SKU breadth per site |
| **In-Transit exclusion** | `WarehouseName = "In-Transit"` filtered | **Portal rule** | Must inherit in M22 |

**Valuation rule:** Use same HPP × Qty as M15 — **do not introduce** alternate valuation without PO approval.

### 5.2 Inventory movement by warehouse (Desktop)

| Indicator | Source | Formula / rule | Management meaning |
| --------- | ------ | -------------- | ------------------ |
| **Period purchases into warehouse** | `KartuStokSummaryDal` Invoice bucket | `INVOICE`, `INVOICE-BONUS` mutasi types | Replenishment activity |
| **Period sales outflow from warehouse** | Faktur bucket | `FAKTUR`, `FAKTUR-BONUS` | Sell-through activity |
| **Returns** | Retur bucket | `RETURJUAL` | Customer returns at site |
| **Internal transfers** | Mutasi bucket | `MUTASI-KLAIM`, `MUTASI-KELUAR`, `MUTASI-MASUK` | Rebalancing activity |
| **Adjustments / opname** | Opname bucket | `OPNAME`, `STOKOP`, `ADJUST` | Corrections and physical counts |
| **MovingStok** | `KartuStokSummaryForm` | `Invoice + Faktur + Retur + Mutasi + Opname` | **Total movement magnitude** — closest BTR definition of warehouse activity |
| **Zero movement + closing stock > 0** | Same form (highlighted pink) | Stagnant stock at warehouse in period | **Dormant inventory at location** |

**Note:** Movement KPIs require **period selection**. Portal reports use **fixed periods** today. PO must decide period for M22 movement signals (MTD, rolling 30 days, etc.).

### 5.3 Dead stock and slow moving by warehouse

| Indicator | Availability | Approach |
| --------- | ------------ | -------- |
| Dead/slow/never-sold **item classification** | M19 (`IBrgLastFakturDal`) | **Reuse classification** — do not redefine thresholds |
| At-risk **value by warehouse** | **Derivable** | Sum `Hpp × Qty` for M19-classified items per warehouse from `StokBalanceView` |
| Dead stock **concentration %** at warehouse | **Derivable** | Warehouse at-risk value ÷ company at-risk value |
| Top 10 dead stock **by warehouse** | **Derivable** | Rank warehouses by dead stock value |

**Boundary:** M19 owns **which items** are at risk. M22 owns **where** that risk sits.

### 5.4 Sales by warehouse

| Indicator | Source | Availability |
| --------- | ------ | ------------ |
| MTD omzet by billing warehouse | `FakturView` — `SUM(GrandTotal)` group by `WarehouseName`, void excluded | **Derivable** |
| Faktur count by warehouse | Count Faktur rows | **Derivable** |
| Customer count by warehouse | Distinct customer keys on Faktur | **Derivable** |
| Share of company omzet % | Warehouse omzet ÷ Sales Dashboard `TotalOmzet` | **Derivable** |
| Top 10 warehouses by omzet | Ranking pattern | **New KPI** |

**Semantic note:** Warehouse on Faktur is **billing/fulfillment origin**, not customer location.

### 5.5 Purchasing by warehouse

| Indicator | Source | Availability |
| --------- | ------ | ------------ |
| MTD purchase value by warehouse | `InvoiceView.WarehouseName` | **Derivable** — row-level in Purchasing Report |
| Pending posting by warehouse | `BELUM` invoices grouped by warehouse | **Derivable** |
| Share of company purchase % | Warehouse purchase ÷ `GrandTotalPurchase` | **Derivable** |

### 5.6 Warehouse productivity — evidence assessment

| Metric | Evidence in BTR? | Assessment |
| ------ | ---------------- | ---------- |
| **Sales generated from warehouse** | `Faktur.WarehouseId` | **Supported** — aggregate MTD omzet |
| **Inventory utilization** | No defined formula | **Not available** — would require capacity master data |
| **Inventory turnover** | No `turnover` KPI in codebase | **Not available** — PO must define if desired |
| **Stock movement activity** | `MovingStok` in Kartu Stok Summary | **Supported Desktop only** — period-based |
| **Sales ÷ inventory value at warehouse** | Both derivable | **Derivable ratio** — not validated as business KPI; PO decision |
| **Packing/fulfillment throughput** | `BTR_Packing*` tables | **Not aggregated** — operational tables exist; no management KPI discovered |

**Recommendation:** M22 V1 should anchor productivity on **observable BTR metrics**: sales omzet, purchase intake, movement magnitude, and at-risk exposure **per warehouse** — not invented turnover formulas.

---

## 6. Territory / Regional Analysis

### 6.1 Wilayah performance — existing portal coverage

| Indicator | Milestone | Availability |
| --------- | --------- | ------------ |
| Customer count by Wilayah | M17 segmentation | **Portal today** |
| Salesman count by Wilayah | M18 segmentation | **Portal today** |
| Active vs dormant customers (wilayah segment) | M17 | **Portal today** (counts) |
| Top Overdue Wilayah | M20 | **Portal today** |
| WilayahHotspot (overdue concentration) | M20 | **Portal today** |
| Wilayah on collection attention list | M20 | **Portal today** |

### 6.2 Wilayah performance — gaps (M22 candidates)

| Indicator | Source | Availability | Management meaning |
| --------- | ------ | ------------ | ------------------ |
| **MTD omzet by Wilayah** | `FakturView.WilayahName` | **Derivable** | Regional sales contribution |
| **Share of company omzet % by Wilayah** | Same | **Derivable** | **Sales concentration** by territory |
| **Open piutang by Wilayah** (total, not just overdue) | FF1 / `PiutangOpenBalanceWithWilayahDal` | **Derivable** | Regional receivable exposure |
| **Top 10 Wilayah by omzet** | Ranking | **New KPI** |
| **Wilayah with sales but low collection recovery** | M20 recovery is company-wide | **Partial** — would need wilayah on pelunasan (FF2 lacks wilayah) |
| **Cross-wilayah selling rate** | Customer vs SalesPerson WilayahId | **Not computed** | Policy / data quality signal |

### 6.3 Regional vs warehouse — when to use which

| Management question | Preferred dimension | Rationale |
| ------------------- | ------------------- | --------- |
| Where is inventory capital stuck? | **Warehouse** | Stock is warehouse-scoped |
| Where are we billing sales from? | **Warehouse** | Faktur.WarehouseId |
| Where are customers geographically segmented? | **Wilayah** | Customer master |
| Where is collection risk concentrated? | **Wilayah** | M20 already answers for overdue |
| Which territory underperforms on sales? | **Wilayah** | Customer Wilayah on Faktur |
| Which receiving site gets all purchases? | **Warehouse** | Invoice.WarehouseId |
| Are we balanced across sites? | **Warehouse** | Imbalance is stock-location concept |

### 6.4 Piutang by warehouse — not supported

Piutang links to Faktur, and Faktur has `WarehouseId`, but **no report or dashboard** groups receivables by billing warehouse. Business interpretation is **ambiguous** (debt follows customer, not warehouse). **Recommendation:** M22 receivable geography should use **Wilayah** only — consistent with M20.

---

## 7. Inventory Concentration Analysis

### 7.1 Company-wide concentration (existing)

| Type | Milestone | Dimension | M22 relationship |
| ---- | --------- | --------- | ---------------- |
| Inventory value by category | M15 | Category | **Complement** — company composition |
| Inventory value by supplier | M15 | Supplier | **Complement** |
| At-risk value by category | M19 | Category | **Complement** |
| At-risk value by supplier | M19 | Supplier | **Complement** |
| Top Category % / Top Supplier % | M16 Executive | Company | **Not location** |

### 7.2 Warehouse concentration (M22 focus)

| Concentration indicator | Business meaning | Data path | Status |
| ----------------------- | ---------------- | --------- | ------ |
| **Top warehouse inventory %** | One site holds majority of stock value | Sum `Hpp×Qty` by warehouse ÷ total | **Derivable** |
| **Top warehouse at-risk %** | Obsolescence capital concentrated at one site | M19 class × warehouse balance | **Derivable** |
| **Top warehouse dead stock %** | Dead stock value concentration | M19 dead class × warehouse | **Derivable** |
| **Warehouse inventory hotspot** | One warehouse exceeds peer share threshold | Pattern: M20 WilayahHotspot (15%) | **Candidate signal** — threshold needs PO |
| **SKU duplicated across warehouses** | Same item stocked at many sites | Count warehouses per `BrgId` with qty > 0 | **Derivable** — imbalance indicator |
| **Single-warehouse SKU dependency** | Item exists at only one site | Inverse of above | **Derivable** — supply risk if site fails |

### 7.3 Slow moving / dead stock concentration by warehouse

M19 computes item-level classes **company-wide**. For warehouse concentration:

1. Classify items using **existing M19 rules** (90/180-day Last Faktur, Never Sold).
2. Join to `StokBalanceView` at Item × Warehouse grain.
3. Sum at-risk value per warehouse.

**Do not** re-run Last Faktur per warehouse — BTR movement signal is **item-global**, not warehouse-specific (item idle everywhere if no recent sale anywhere).

### 7.4 Supplier inventory concentration by warehouse

| Indicator | Meaning | Status |
| --------- | ------- | ------ |
| Principal X inventory dominated by warehouse Y | Supplier stock trapped at one site | **Derivable** from StokBalanceView |
| M21 compound dependency + warehouse | Same principal heavy in one warehouse | **Cross-domain derivable** |

---

## 8. Sales Contribution Analysis

### 8.1 Location-based sales analytics — existing

| Analytics | Grain | Source | Portal |
| --------- | ----- | ------ | ------ |
| Company MTD omzet | Company | Sales snapshot / `FakturView` | **Yes** |
| Top 10 Salesman omzet | Salesman | Sales snapshot | **Yes** |
| Customer Top 10 omzet | Customer | M17 snapshot | **Yes** |
| Wilayah customer counts | Wilayah | M17 segmentation | **Yes** (counts only) |
| Faktur row with Warehouse + Wilayah | Faktur | `FakturViewDal` | **Sales Report — columns dropped** |

### 8.2 Location-based sales analytics — derivable (M22 candidates)

| KPI | Formula | Reconciliation |
| --- | ------- | -------------- |
| **Omzet by Warehouse** | `SUM(GrandTotal)` current month, group `WarehouseName` | Sum of warehouse rows = Sales Dashboard `TotalOmzet` |
| **Omzet by Wilayah** | Same, group `WilayahName` (customer) | Sum of wilayah rows = `TotalOmzet` |
| **Top 10 Warehouse by omzet** | Rank descending | New ranking table |
| **Top 10 Wilayah by omzet** | Rank descending | New ranking table |
| **Warehouse omzet %** | Warehouse omzet ÷ company omzet | Concentration indicator |
| **Wilayah omzet %** | Wilayah omzet ÷ company omzet | Concentration indicator |
| **Weekly omzet trend by warehouse** | Week grouper × warehouse | **Not available** — new series |
| **Customer count by warehouse** | Distinct customers on Faktur | Reach per billing site |

### 8.3 Reusable calculations and reports

| Asset | Reuse |
| ----- | ----- |
| `DashboardSalesFakturAggregator` void exclusion + month filter | Same rules for warehouse/wilayah sales aggregates |
| `SalesOmzetChartWeekGrouper` | Weekly buckets if PO wants warehouse trend |
| Sales Report | Extend API to expose `WarehouseName` / `WilayahName` OR M22 snapshot only |
| `FakturInfoForm` (Desktop) | Validation — both columns visible |

---

## 9. Receivable Exposure Analysis

### 9.1 By Wilayah — strong existing coverage (M20)

| Indicator | Owner | M22 action |
| --------- | ----- | ---------- |
| Overdue balance by Wilayah | M20 `TopOverdueWilayah` | **Cross-link** |
| WilayahHotspot (≥ 15% overdue share) | M20 attention signal | **Cross-link** |
| Wilayah on collection attention list | M20 | **Cross-link** |
| Total open piutang by Wilayah | FF1 / `PiutangOpenBalanceWithWilayahDal` | **Optional** — M22 regional panel if not duplicating M20 overdue framing |
| Overdue concentration by Wilayah | M20 | **Do not duplicate** |

### 9.2 By Warehouse — not available

| Indicator | Status | Rationale |
| --------- | ------ | --------- |
| Piutang by billing warehouse | **Not recommended** | Receivable follows customer obligation, not warehouse |
| Overdue by warehouse | **Not computed** | Same ambiguity |

### 9.3 Collection effectiveness by location

| Indicator | Source | Status |
| --------- | ------ | ------ |
| Recovery vs Billing % (company) | M20 | **Company-wide only** |
| Recovery by Wilayah | FF2 pelunasan — **no wilayah column** | **Not available** without new join path |
| Recovery by salesman | M20 `LowRecoveryVsBilling` | **Owned by M18/M20** — not location dashboard |

**Assessment:** M22 should surface **wilayah receivable exposure** via **links to M20**, not rebuild collection effectiveness analytics.

---

## 10. Existing Desktop Capability Analysis

### 10.1 Master data maintenance

| Menu | Form | Location relevance |
| ---- | ---- | ------------------ |
| SM3 | `WilayahForm` | Wilayah master — segmentation dimension |
| IM2 | `WarehouseForm` | Gudang master — warehouse analytics universe |
| SM1 | `CustomerForm` | Customer `WilayahId` |
| SM2 | `SalesPersonForm` | Salesman `WilayahId` |

### 10.2 Desktop reports — warehouse-oriented

| Report ID | Form | DAL | Location columns / filters |
| --------- | ---- | --- | -------------------------- |
| IF1 | `StokBalanceInfoForm` / `StokBalanceInfo2Form` | `StokBalanceViewDal` | Multi-warehouse picker; `Warehouse` column; optional In-Transit |
| IF8 | `KartuStokSummaryForm` | `KartuStokSummaryDal` + `StokPeriodikDal` | **Requires warehouse selection**; `MovingStok`, opening/closing stock |
| IF7 | `KartuStokInfoForm` | `KartuStokDal` | Single warehouse filter |
| IF6 | `StokPeriodikForm` | `StokPeriodikDal` | Warehouse-scoped period balances |
| IF3 | `StokBrgSupplierForm` | `StokBrgSupplierDal` | `WarehouseName`; In-Transit toggle |
| PF1 | `InvoiceInfoForm` | `InvoiceViewDal` | `WarehouseName` |
| SF1 | `FakturInfoForm` | `FakturViewDal` | `WilayahName`, `WarehouseName` |
| RF1 | `ReturJualReportForm` | `ReturJualViewDal` | `WilayahName`, `WarehouseName` |
| IM? | `MutasiInfoForm` | `MutasiBrgViewDal` | Mutasi per warehouse |

### 10.3 Desktop reports — territory-oriented

| Report ID | Form | DAL | Location columns |
| --------- | ---- | --- | ---------------- |
| FF1 | `PiutangSalesWilayahForm` | `PiutangSalesWilayahDal` | **`WilayahName`**, `SalesName`; grouped by wilayah in grid/Excel |
| SF1 | `FakturInfoForm` | `FakturViewDal` | `WilayahName` |
| RO4 | `LocationCoverageInfoForm` | `ICustomerDal` | Customer `Wilayah` + GPS |
| Customer Chart | `CustomerChartRpt` | `ICustomerDal` | **Groups by Kota (city)** — not Wilayah despite menu label ambiguity |

### 10.4 Desktop analytics not location-aggregated

| Feature | Note |
| ------- | ---- |
| `SalesOmzetChartForm` | Salesman and company omzet — **no warehouse/wilayah** |
| `OmzetSupplierInfoForm` | Supplier sales calendar — no location |
| `EffectiveCallInfoForm` | Field activity — **M25** scope |
| `StokBalanceHealthDal` | Data integrity — not location performance |

### 10.5 Operational workflows with location impact

| Workflow | Desktop | Location signal potential |
| -------- | ------- | ------------------------- |
| Mutasi Keluar / Masuk | Mutasi forms | Transfer volume between warehouses |
| Stok Opname | Opname per warehouse | Adjustment mutasi at site |
| Posting Stok (PT2) | Invoice → warehouse stock | Purchasing intake location |
| Packing | Packing orders + Depo | **Operational** — not in scope V1 |

---

## 11. Existing Asset Discovery

Maximize reuse — avoid new business calculations when equivalent logic exists.

### 11.1 Portal — reuse directly

| Asset | Path / type | Reuse for M22 |
| ----- | ----------- | ------------- |
| `DashboardInventoryItemGroupBuilder` | Application service | In-Transit exclusion; BrgId-first rules for warehouse rollup |
| `IFakturViewDal` / `FakturView` | Sales read path | Sales by warehouse and wilayah |
| `IStokBalanceViewDal` / `StokBalanceView` | Inventory read path | Inventory by warehouse |
| `IInvoiceViewDal` / `InvoiceView` | Purchase read path | Purchasing by warehouse |
| `IPiutangOpenBalanceWithWilayahDal` | Collection refresh | Wilayah on open balances — **prefer over FF1 scan** for snapshots |
| `DashboardCollectionAggregator` | M20 | WilayahHotspot pattern; **read M20 snapshot** for overdue wilayah |
| `DashboardCustomerAggregator` | M17 | Wilayah segmentation pattern |
| `DashboardSalesmanAggregator` | M18 | Wilayah segmentation pattern |
| `DashboardInventoryRiskAggregator` / `IBrgLastFakturDal` | M19 | Item classification for at-risk-by-warehouse |
| `InventoryReportDal` | Report | Item × Warehouse validation |
| `PurchasingReportDal` | Report | Invoice × Warehouse validation |
| Vue: `ExecutiveAttentionCard`, `Top10RankingTable`, attention list components | Frontend | M17–M21 UX patterns |

### 11.2 Portal — excluded from M22 scope (Q27)

| Asset | Gap | PO decision |
| ----- | --- | ----------- |
| `SalesReportDal` | Omits `WilayahName`, `WarehouseName` from `FakturView` | **Do not extend** in M22 — dashboard-only; Desktop SF1 for validation |
| `PiutangReportDal` | Omits `WilayahName` from FF1 DTO | **Do not extend** — wilayah receivable detail via Collection Dashboard link |

### 11.3 Desktop DALs — portal candidates

| Asset | Reuse potential |
| ----- | --------------- |
| `StokBalanceViewDal` | **High** — already used by portal inventory |
| `KartuStokSummaryDal` | **Medium** — movement by warehouse; period-scoped; performance cost |
| `StokPeriodikDal` | **Medium** — opening/closing per warehouse |
| `PiutangSalesWilayahDal` | **Medium** — superseded for snapshots by `PiutangOpenBalanceWithWilayahDal` where possible |
| `MutasiBrgViewDal` | **Low–Medium** — transfer analysis |
| `IWarehouseDal` | **High** — warehouse master universe, `IsSpecial` filter |
| `FakturViewDal` | **High** — already portal dependency |

### 11.4 Explicitly do not reuse (wrong semantics)

| Asset | Reason |
| ----- | ------ |
| `BTR_SalesOmzet` | No warehouse or wilayah dimension |
| `DashboardPiutangAggregator` alone | No wilayah in piutang snapshot |
| `CustomerChartRpt` (Kota grouping) | City ≠ Wilayah |
| Depo DALs | Not consistent analytic grain |
| `StokBalanceHealthDal` | Integrity, not performance |

### 11.5 Snapshot pattern reference

**Best reference implementation for wilayah materialization:** M20 `DashboardCollectionAggregator` + `PiutangOpenBalanceWithWilayahDal` + `BTRPD_CollectionTopOverdueWilayah`.

**Best reference for new warehouse dimension:** M15 `DashboardInventoryAggregator` breakdown pattern — but with `DimensionType = Warehouse` (does not exist today).

---

## 12. Exception-Based Management Analysis

Focus: situations deserving management attention — concentration and exposure at warehouse level. **Wilayah collection attention belongs to M20** — not duplicated in M22 attention list.

### 12.1 Approved V1 attention signals (Q16)

| Signal | Entity | Business meaning | Inclusion rule (architect to formalize) |
| ------ | ------ | ---------------- | ---------------------------------------- |
| **WarehouseInventoryConcentration** | Warehouse | One warehouse holds material share of total inventory value | Warehouse ranks in Top 10 by inventory value with computable % of company total |
| **WarehouseAtRiskConcentration** | Warehouse | At-risk capital (M19 classes) concentrated at one warehouse | Warehouse ranks in Top 10 at-risk value; M19 classification reused unchanged (Q21) |
| **WarehouseSalesConcentration** | Warehouse | Billing omzet heavily concentrated in one warehouse | Warehouse ranks in Top 10 MTD omzet with computable % |
| **WarehousePurchasingConcentration** | Warehouse | Purchase intake concentrated at one receiving site | Warehouse ranks in Top 10 MTD purchase with computable % |
| **WarehouseNoSalesWithInventory** | Warehouse | Positive stock, zero MTD Faktur from warehouse | Inventory value > 0 AND MTD omzet from warehouse = 0 |
| **WarehouseInactiveWithStock** | Warehouse | Inactive master still holding stock | `IsAktif = false` AND inventory value > 0 (Q18) |

**Attention list grain:** **Warehouse × Signal only** (Q15). Wilayah attention remains on Collection Dashboard (M20).

### 12.2 Deferred / excluded signals

| Signal | Status | Reason |
| ------ | ------ | ------ |
| **CrossWilayahSelling** | **Deferred** (Q7) | Niche; not first management question |
| **WarehouseImbalance** | **Deferred** (Q19) | Expensive; difficult to explain |
| **WilayahHotspot** | **M20 owns** | Navigation link only (Q20) |
| **QualifiedBacklogAtWarehouse** | **Excluded** | M21 link only (Q22) |
| **WarehouseNoPurchasingWithSales** | **Not approved** | Not in Q16 approved list |
| **WilayahLowSalesHighOverdue** | **Not approved** | Compound signal not in V1 scope |

### 12.3 Concentration indicators — informational only (Q17)

| Indicator | Display | Auto-attention? |
| --------- | ------- | --------------- |
| Top 1 warehouse inventory % | Attention card / KPI | **Informational** — management interprets |
| Top 3 warehouse inventory % | Attention card / KPI | **Informational** |
| Top 1 warehouse omzet % | Attention card / KPI | **Informational** |
| Top 3 warehouse omzet % | Attention card / KPI | **Informational** |
| Top 1 wilayah omzet % | Attention card / KPI | **Informational** |
| **WarehouseHotspot** (auto-threshold) | — | **Do not create in V1** |
| WilayahHotspot | M20 Collection Dashboard | **M20 owns** — link only |

Following M17/M18/M21 philosophy: show concentration percentages and approved signals; **avoid threshold-heavy rule engines** beyond explicit signal inclusion rules.

### 12.4 Performance indicators — excluded from V1

| Indicator | Status | Reason |
| --------- | ------ | ------ |
| Warehouse movement / `MovingStok` | **V2 candidate** (Q12) | Period complexity; lower value than concentration |
| Inventory turnover / sales÷inventory ratio | **Excluded** (Q13) | No BTR business definition |
| Warehouse achievement vs target | **Excluded** (Q14) | No warehouse target master |
| YoY location comparison | **Excluded** | No snapshot history |

---

## 13. Dashboard Layout — Approved

**Product Owner decision:** Dual Equal framing — **Proposal A (Location Attention First)** (Q2). Route: `/dashboard/locations`. Page title: **Branch / Warehouse Performance Dashboard**.

### 13.1 Approved page structure

**Route:** `/dashboard/locations`  
**Title:** Branch / Warehouse Performance Dashboard  
**Internal purpose:** Location Concentration Dashboard  
**Audience:** All authenticated users — same visibility model as M16–M21

### 13.2 Approved wireframe (fixed section order)

```text
Branch / Warehouse Performance Dashboard                 [/dashboard/locations]

1. Location Attention Cards
┌──────────────────────────────────────────────────────────────────────────┐
│  Top Warehouse Inventory % · Top 3 Warehouse Inventory %                   │
│  Top Warehouse At-Risk % · Top Warehouse Sales % · Top Wilayah Sales %   │
│  Inactive Warehouse With Stock Count                                       │
└──────────────────────────────────────────────────────────────────────────┘

2. Top Warehouse by Inventory
┌──────────────────────────────────────────────────────────────────────────┐
│  Top 10 Warehouse by Inventory Value (+ % of company total)              │
│  Row click → Inventory Report ?q={WarehouseName}                         │
└──────────────────────────────────────────────────────────────────────────┘

3. Top Warehouse by At-Risk Inventory
┌──────────────────────────────────────────────────────────────────────────┐
│  Top 10 Warehouse by At-Risk Value (+ %) — M19 classification reused     │
│  Row click → Inventory Risk Dashboard / Inventory Report               │
└──────────────────────────────────────────────────────────────────────────┘

4. Top Warehouse by Sales
┌──────────────────────────────────────────────────────────────────────────┐
│  Top 10 Warehouse by MTD Omzet (+ %) — current calendar month            │
└──────────────────────────────────────────────────────────────────────────┘

5. Top Warehouse by Purchasing
┌──────────────────────────────────────────────────────────────────────────┐
│  Top 10 Warehouse by MTD Purchase (+ %) — current calendar month         │
└──────────────────────────────────────────────────────────────────────────┘

6. Top Wilayah by Sales
┌──────────────────────────────────────────────────────────────────────────┐
│  Top 10 Wilayah by MTD Omzet (+ %) — customer Wilayah on Faktur         │
│  Row click → Collection Dashboard (Q29) — NOT Piutang Report           │
└──────────────────────────────────────────────────────────────────────────┘

7. Location Attention List
┌──────────────────────────────────────────────────────────────────────────┐
│  Warehouse × Signal ONLY                                                 │
│  WarehouseInventoryConcentration · WarehouseAtRiskConcentration          │
│  WarehouseSalesConcentration · WarehousePurchasingConcentration          │
│  WarehouseNoSalesWithInventory · WarehouseInactiveWithStock              │
└──────────────────────────────────────────────────────────────────────────┘

8. Navigation (mandatory)
┌──────────────────────────────────────────────────────────────────────────┐
│  → Inventory Dashboard · Inventory Risk Dashboard · Sales Dashboard      │
│  → Purchasing Dashboard · Collection Dashboard                           │
│  → Customer Analytics · Salesman Performance Dashboard                   │
└──────────────────────────────────────────────────────────────────────────┘
```

**Design constraints:**

- **Do not embed** M20 Top Overdue Wilayah — **link to Collection Dashboard only** (Q20).
- **Do not embed** M21 qualified backlog by warehouse — **link to Purchasing Dashboard only** (Q22).
- **Wilayah receivable detail** — Collection Dashboard, not Piutang Report (Q29).
- **No movement KPI section** in V1 (Q12).

### 13.3 Rejected alternatives (discussion closed)

| Proposal | Status |
| -------- | ------ |
| Proposal B — Dual Tab | Not selected — dual equal content on single page preferred |
| Proposal C — Ranking-First only | Not selected — attention cards and attention list required |

---

## 14. Gap Analysis

### 14.1 Information already available

| Information | Where |
| ----------- | ----- |
| Item × Warehouse stock rows | Inventory Report, `StokBalanceViewDal` |
| Invoice × Warehouse rows | Purchasing Report |
| Faktur with WarehouseName and WilayahName | `FakturViewDal` (Desktop SF1; not Sales Report API) |
| Wilayah on open piutang | `PiutangOpenBalanceWithWilayahDal`, FF1 |
| Top Overdue Wilayah, WilayahHotspot | M20 Collection snapshot |
| Wilayah segmentation counts | M17 Customer, M18 Salesman snapshots |
| Company inventory/risk/purchasing KPIs | M15, M19, M21 snapshots |
| In-Transit exclusion rule | M15/M19/report DALs |
| Warehouse master list | `BTR_Warehouse`, `IWarehouseDal` |
| Per-warehouse movement (Desktop) | `KartuStokSummaryDal` |

### 14.2 Information partially available

| Information | Gap |
| ----------- | --- |
| **Inventory value by warehouse** | Row-level exists; **no aggregate KPI or snapshot** |
| **Sales by warehouse / wilayah** | Source exists; **not aggregated** in portal |
| **Purchasing by warehouse** | Report column exists; **no dashboard aggregate** |
| **At-risk value by warehouse** | M19 item class + warehouse rows; **join not computed** |
| **Inactive warehouse detection** | `IsAktif` in SQL; **not in application model** |
| **Warehouse movement / productivity** | Desktop IF8; **not in portal** |
| **Wilayah omzet ranking** | FakturView wilayah; **only customer counts in M17** |
| **Cross-domain location signals** | Sources exist separately; **not composed** |
| **Sales/Piutang Report location columns** | DAL has fields; **API strips them** |
| **Inter-warehouse imbalance** | Balance rows exist; **no pairwise KPI** |
| **Recovery by wilayah** | Pelunasan lacks wilayah | **Not joinable** without new path |

### 14.3 Information not currently available

| Information | Note |
| ----------- | ---- |
| **Branch as entity** | Does not exist — use Warehouse |
| **Piutang by warehouse** | No business rule or implementation |
| **Inventory turnover by location** | No formula in BTR |
| **Warehouse-level sales targets** | No target master |
| **Depo-level analytics** | No reporting grain |
| **Location YoY / trend history** | Snapshots are point-in-time |
| **Warehouse utilization / capacity** | No capacity master |
| **Automated location anomaly detection** | No statistical baselines |
| **Unified location score** | Not defined |
| **M23 Alert Center aggregation** | Milestone not implemented |

---

## 15. Relationship to Other Milestones

| Milestone | Question answered | M22 boundary |
| --------- | ----------------- | ------------ |
| **M16 Executive** | What requires attention company-wide? | **Phase 2** executive promotion of location concentration (Q23); **no M16 changes in V1** |
| **M17 Customer Analytics** | Which **customers** require attention? | M17 owns customer × signal. M22 owns **location × signal**. M17 `ByWilayah` is **customer counts** — M22 adds **wilayah omzet/piutang amounts** without customer attention list duplication |
| **M18 Salesman Performance** | Which **salesmen** require attention? | M18 owns salesman × signal. M22 may show **wilayah sales contribution** but **not** salesman rankings or achievement |
| **M19 Inventory Risk** | Which **items** require attention (slow/dead/never sold)? | M19 owns item classification. M22 **reuses M19 classification exactly** — composes at-risk value by warehouse; **no M19 snapshot changes** (Q21) |
| **M20 Collection** | Are receivables converting to cash? Which **wilayah** has overdue concentration? | M20 **owns** all wilayah collection attention. M22 shows **Top Wilayah by Sales** only; **navigation link** to Collection Dashboard — **no embedded** Top Overdue Wilayah (Q20) |
| **M21 Purchasing** | Which **suppliers/principals** require purchasing attention? | M21 owns principal × signal. M22 shows **warehouse purchasing concentration** ranking; **link only** to Purchasing Dashboard — no qualified backlog by warehouse (Q22) |
| **M23 Alert Center** (future) | Unified alert aggregation? | M22 signals designed **M23-consumable** (Q24) — stable signal keys, attention list grain |
| **M25 Sales Force Effectiveness** | Route, check-in, effective call? | **No overlap** — field activity is out of M22 scope |

### Milestone question answered by M22

| Prior dashboard | Question |
| --------------- | -------- |
| Domain dashboards (company totals) | *How much sales, inventory, purchasing company-wide?* |
| M17/M18 (entity segmentation) | *Which customers/salesmen — with wilayah as context?* |
| M20 (collection geography) | *Which wilayah has overdue collection risk?* |
| **M22 Location Concentration** | ***Are we becoming too dependent on a particular warehouse or territory?*** |

### Explicit exclusions (M22 V1 — approved)

| Exclusion | Reason (PO) |
| --------- | ----------- |
| Depo analytics | Operational routing — not management analytics (Q4) |
| Piutang by billing warehouse | Receivables follow customer geography — Wilayah only (Q8) |
| Movement KPIs (`MovingStok`, IF8) | V2 candidate — complexity vs value (Q12) |
| Productivity ratios / inventory turnover | No BTR business definition (Q13) |
| Warehouse sales targets / achievement % | No warehouse target master (Q14) |
| CrossWilayahSelling signal | Deferred — niche (Q7) |
| Warehouse imbalance (SKU-level) | Deferred — expensive, hard to explain (Q19) |
| Qualified backlog by warehouse | M21 ownership — link only (Q22) |
| Embedded Top Overdue Wilayah | M20 ownership — link only (Q20) |
| Wilayah × Signal attention list | M20 owns wilayah collection attention (Q15) |
| `WarehouseHotspot` auto-threshold | Informational concentration only in V1 (Q17) |
| Sales Report / Piutang Report API changes | Dashboard-only milestone (Q27) |
| Executive Dashboard promotion | Phase 2 (Q23) |
| Kartu Stok / mutasi drill-down in portal | Desktop validation only |
| Duplicating M19 item attention list | M19 owns item × signal |
| Salesman performance by location | M18 scope |
| Route / GPS / effective call | M25 scope |
| Branch master creation | Not a BTR concept |
| Historical location trend / YoY | No snapshot history |

---

## 15.1 KPI-to-Report Traceability Matrix

| M22 KPI candidate | Primary source DAL / snapshot | Validation report | Reconciliation rule |
| ----------------- | ----------------------------- | ----------------- | ------------------- |
| Total Inventory Value (company) | M15 snapshot / `StokBalanceViewDal` | Inventory Report footer | BrgId-first; excl. In-Transit — **unchanged** |
| Inventory Value by Warehouse | `StokBalanceViewDal` grouped | Inventory Report — sum rows per warehouse | Sum warehouse values = footer total (if all warehouses included) |
| At-Risk Value by Warehouse | M19 classification × `StokBalanceViewDal` | Inventory Risk + Inventory Report | At-risk sum ≤ M19 company at-risk total |
| MTD Omzet by Warehouse | `FakturViewDal` | Desktop SF1 | Sum = Sales Dashboard `TotalOmzet` |
| MTD Omzet by Wilayah | `FakturViewDal` (customer Wilayah) | Desktop SF1 | Sum = `TotalOmzet` |
| MTD Purchase by Warehouse | `InvoiceViewDal` | Purchasing Report | Sum = Purchasing footer `GrandTotalPurchase` |
| Top Overdue Wilayah | **M20 snapshot** | Collection Dashboard (link) | **Do not recompute or embed** — Q20 |
| Warehouse row drill-down | M22 snapshot | Inventory Report `?q={WarehouseName}` | Q28 |
| Wilayah row drill-down | M22 snapshot | Collection Dashboard | Q29 — not Piutang Report |
| Movement by warehouse | `KartuStokSummaryDal` | Desktop IF8 only | **Excluded from M22 V1** — Q12 |

---

## 16. Product Owner Decisions (Authoritative)

**All open questions resolved.** Recorded 2026-06-09. Architects must treat this section as the source of truth for business scope.

### 16.1 Scope and naming

| # | Decision |
| - | -------- |
| Q1 | Route: **`/dashboard/locations`**. Page title: **Branch / Warehouse Performance Dashboard**. |
| Q2 | **Dual Equal** framing — Proposal A layout. Warehouse (logistics/inventory) and Wilayah (commercial/receivable geography) equally important. |
| Q3 | **Yes** — use "Branch / Warehouse" in UI. Internally implement **Warehouse** and **Wilayah** only. |
| Q4 | **Depo excluded** from M22. |

**Internal milestone purpose:** **Location Concentration Dashboard** — theme is dependency/concentration across warehouses and territories, not operational productivity.

### 16.2 Location dimensions

| # | Decision |
| - | -------- |
| Q5 | Warehouse universe for rankings: **active warehouses only**; **exclude** `In-Transit` and `IsSpecial = true`. Inactive warehouses (`IsAktif = false`) appear **only** via `WarehouseInactiveWithStock` attention signal. |
| Q6 | Sales wilayah: **Customer Wilayah** (`FakturView.WilayahName`) — consistent with M17, M20, FakturView. |
| Q7 | **CrossWilayahSelling deferred** — not V1. |
| Q8 | **Piutang by warehouse excluded.** Receivable geography = Wilayah via M20 link. |

### 16.3 KPIs and periods

| # | Decision |
| - | -------- |
| Q9 | **Mandatory sections:** Location Attention Cards · Top Warehouse by Inventory · Top Warehouse by At-Risk Inventory · Top Warehouse by Sales · Top Warehouse by Purchasing · Top Wilayah by Sales · Location Attention List. **Optional:** Movement KPI (not in V1). |
| Q10 | Sales period: **current calendar month (MTD)**. |
| Q11 | Inventory period: **point-in-time snapshot** (M15/M19 aligned). |
| Q12 | **Movement KPI excluded from V1** — V2 candidate. |
| Q13 | **Productivity ratios excluded** — no existing business definition. |
| Q14 | **Warehouse sales targets excluded** — none exist in BTR. |

### 16.4 Attention signals and thresholds

| # | Decision |
| - | -------- |
| Q15 | Attention list grain: **Warehouse × Signal only.** Wilayah attention stays on M20. |
| Q16 | **Approved signals:** `WarehouseInventoryConcentration`, `WarehouseAtRiskConcentration`, `WarehouseSalesConcentration`, `WarehousePurchasingConcentration`, `WarehouseNoSalesWithInventory`, `WarehouseInactiveWithStock`. **Not approved:** `CrossWilayahSelling`, `WarehouseImbalance`. |
| Q17 | Concentration: **informational only.** Show Top Warehouse % and Top 3 Warehouse %. **Do not create** `WarehouseHotspot` auto-threshold in V1. |
| Q18 | **WarehouseInactiveWithStock:** `IsAktif = false` **AND** inventory value > 0. |
| Q19 | **WarehouseImbalance deferred.** |

### 16.5 Relationship to other milestones

| # | Decision |
| - | -------- |
| Q20 | M20 overlap: **navigation link only** — do not embed Top Overdue Wilayah. |
| Q21 | M19 overlap: **reuse M19 classification exactly** — no modifications to M19 snapshots. |
| Q22 | M21 overlap: **link only** — do not introduce qualified backlog by warehouse. |
| Q23 | Executive Dashboard promotion: **Phase 2** — no M16 changes in V1. |
| Q24 | **M23 compatibility: Yes** — architect designs signals for future Alert Center consumption. |

### 16.6 Data architecture (business-level)

| # | Decision |
| - | -------- |
| Q25 | **Dedicated snapshot domain:** e.g. `BTRPD_LocationWarehouse`, `BTRPD_LocationAttention`. Avoid expensive live composition. |
| Q26 | Refresh cadence: **60 minutes** (align with M15/M19 — inventory-dominant). |
| Q27 | **Dashboard-only** — do **not** modify Sales Report or Piutang Report APIs in M22. |

### 16.7 Drill-down and navigation

| # | Decision |
| - | -------- |
| Q28 | Warehouse row click → **Inventory Report** with `?q={WarehouseName}`. |
| Q29 | Wilayah row click → **Collection Dashboard** — not Piutang Report. |
| Q30 | **Mandatory navigation links:** Inventory Dashboard · Inventory Risk Dashboard · Sales Dashboard · Purchasing Dashboard · Collection Dashboard · Customer Analytics · Salesman Performance Dashboard. |

---

## 17. Location Concentration KPI Definitions (Approved)

All monetary values in IDR. Periods: sales/purchasing = **current calendar month**; inventory/at-risk = **point-in-time** at refresh.

| KPI | Period / scope | Formula / rule | Business meaning |
| --- | -------------- | ---------------- | ---------------- |
| **Inventory Value by Warehouse** | Point-in-time | Sum `Hpp × Qty` by `WarehouseName`; exclude In-Transit, `IsSpecial`; active warehouses in rankings | Capital at each site |
| **Warehouse Inventory %** | Point-in-time | Warehouse value ÷ M15 `TotalInventoryValue` × 100 | Inventory concentration |
| **At-Risk Value by Warehouse** | Point-in-time | M19 item classes × `StokBalanceView` per warehouse; same classification rules | Obsolescence capital by site |
| **Warehouse At-Risk %** | Point-in-time | Warehouse at-risk ÷ M19 total at-risk value × 100 | Risk concentration |
| **MTD Omzet by Warehouse** | Current month | `SUM(GrandTotal)` per `FakturView.WarehouseName`, void excluded | Billing concentration |
| **Warehouse Sales %** | Current month | Warehouse omzet ÷ Sales Dashboard `TotalOmzet` × 100 | Sales dependency on site |
| **MTD Purchase by Warehouse** | Current month | `SUM(GrandTotal)` per `InvoiceView.WarehouseName`, void excluded | Intake concentration |
| **Warehouse Purchasing %** | Current month | Warehouse purchase ÷ Purchasing `GrandTotalPurchase` × 100 | Receiving-site dependency |
| **MTD Omzet by Wilayah** | Current month | `SUM(GrandTotal)` per customer `WilayahName` on Faktur | Territory sales contribution |
| **Wilayah Sales %** | Current month | Wilayah omzet ÷ `TotalOmzet` × 100 | Territory concentration |
| **Top 1 / Top 3 Warehouse %** | Per domain | Top warehouse share of inventory, at-risk, sales, or purchasing total | **Informational** concentration headline |
| **Inactive Warehouse With Stock Count** | Point-in-time | Count warehouses where `IsAktif = false` AND inventory value > 0 | Legacy sites holding capital |

**Warehouse universe rule (Q5):** Rankings include **active, non-special** warehouses only. `WarehouseInactiveWithStock` may surface **inactive** warehouses on the attention list only.

**Reconciliation:** Sum of warehouse MTD omzet rows = Sales Dashboard `TotalOmzet`. Sum of warehouse MTD purchase rows = Purchasing footer `GrandTotalPurchase`. Sum of warehouse inventory values = Inventory Report footer total (when same exclusion rules applied). Sum of warehouse at-risk values ≤ M19 company at-risk total.

**Traceability:** Dashboard-only for location columns — validate against Desktop SF1 (Faktur), IF1 (Inventory), PF1 (Purchasing). No portal report API changes in M22.

---

## Appendix A — Ubiquitous Language (M22)

| Term | M22 usage |
| ---- | --------- |
| **Location Concentration Dashboard** | Internal milestone purpose — dependency on warehouse or territory |
| **Warehouse** | Logical inventory and billing origin — **primary logistics location** |
| **Wilayah** | Customer territory on Faktur — **primary commercial region** for sales ranking |
| **Branch** | **UI language only** — not a system entity; maps to Warehouse in implementation |
| **Territory** | Synonym for Wilayah |
| **Depo** | **Out of scope V1** |
| **Concentration** | Top-N location holds material % of domain total — **informational in V1** |
| **At-risk inventory** | M19 Never Sold + Slow Moving + Dead Stock — **reused unchanged** |
| **In-Transit / IsSpecial** | Excluded from warehouse ranking universe |
| **WarehouseInactiveWithStock** | `IsAktif = false` AND inventory value > 0 |
| **WarehouseHotspot** | **Not in V1** — no auto-threshold concentration alert |

## Appendix B — Key Source Files (for Architect handoff)

| Purpose | Path |
| ------- | ---- |
| Warehouse master SQL | `src/j05-btr-distrib/btr.sql/Tables/InventoryContext/BTR_Warehouse.sql` |
| Wilayah master SQL | `src/j05-btr-distrib/btr.sql/Tables/SalesContext/BTR_Wilayah.sql` |
| Faktur with warehouse + wilayah | `src/j05-btr-distrib/btr.infrastructure/SalesContext/FakturInfoAgg/FakturViewDal.cs` |
| Stock balance Item × Warehouse | `src/j05-btr-distrib/btr.infrastructure/InventoryContext/StokBalanceRpt/StokBalanceViewDal.cs` |
| Movement summary per warehouse | `src/j05-btr-distrib/btr.infrastructure/InventoryContext/KartuStokRpt/KartuStokSummaryDal.cs` |
| Inventory report (portal) | `src/j05-btr-distrib/btr.infrastructure/ReportingContext/InventoryReportAgg/InventoryReportDal.cs` |
| Sales report (portal — strips location) | `src/j05-btr-distrib/btr.infrastructure/ReportingContext/SalesReportAgg/SalesReportDal.cs` |
| Piutang report (portal — strips wilayah) | `src/j05-btr-distrib/btr.infrastructure/ReportingContext/PiutangReportAgg/PiutangReportDal.cs` |
| M20 wilayah aggregator | `src/j05-btr-distrib/btr.application/ReportingContext/DashboardSnapshotAgg/Services/DashboardCollectionAggregator.cs` |
| M15 inventory rollup (no warehouse) | `src/j05-btr-distrib/btr.application/ReportingContext/DashboardSnapshotAgg/Services/DashboardInventoryAggregator.cs` |
| In-Transit exclusion | `src/j05-btr-distrib/btr.application/ReportingContext/DashboardSnapshotAgg/Services/DashboardInventoryItemGroupBuilder.cs` |
| Desktop reporting inventory | `btr-reporting-investigation.md` §2.3 |

---

*End of analysis — M22 Branch / Warehouse Performance Dashboard (Location Concentration Dashboard)*
