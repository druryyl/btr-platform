# BTR Portal Analysis — M23 Alert Center

**Status:** Implemented — see [M23 Alert Center - Implementation Summary.md](./M23%20Alert%20Center%20-%20Implementation%20Summary.md) and [M23 Alert Center - Plan.md](./M23%20Alert%20Center%20-%20Plan.md).  
**Scope:** Business analysis only. No implementation plans, API design, database design, or code.  
**Date:** 2026-06-09 (analysis) · Product Owner decisions recorded 2026-06-09  
**Context:** BTR Portal V2 (M16–M22) delivers domain-specific management analytics, each answering *What requires management attention?* within its business lens. Attention signals are now materialized across **nine snapshot domains** and surfaced on **eleven dashboard routes**, but management must still visit each dashboard separately. M23 introduces an **Alert Center** — a company-wide management attention view that **aggregates, prioritizes, and surfaces** signals already discovered elsewhere.

**Product direction (mandatory):** M23 is **not** another KPI dashboard, ranking dashboard, or reporting screen. It is a **management attention center** that primarily surfaces **exceptions, risks, concentrations, anomalies, and operational concerns** produced by existing dashboards.

**Reference documents:** `docs/foundation/PRODUCT.md`, `docs/foundation/DOMAIN.md`, `docs/foundation/LANDSCAPE.md`, `docs/foundation/WORKFLOW.md`, `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-operational.md`, `docs/features/btr-portal/btr-portal-architecture.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md`, `docs/work/btr-portal/M16 Executive Dashboard - Analysis.md`, `docs/work/btr-portal/M17-Customer-Analytics-Analysis.md`, `docs/work/btr-portal/M18 Salesman Performance - Analysis.md`, `docs/work/btr-portal/M19 Slow Moving & Dead Stock - Analysis.md`, `docs/work/btr-portal/M20-collection-dashboard-analysist.md`, `docs/work/btr-portal/M21-purchasing-dashboard-analysis.md`, `docs/work/btr-portal/M22 Branch Warehouse Performance - Analysis.md`

---

## 1. Executive Summary

BTR Portal V2 has successfully embedded a consistent **attention signal pattern** across milestones M16–M22:

| Pattern element | Where established |
| --------------- | ----------------- |
| Attention Cards (headline counts / exposure) | M16 executive cards; M17–M22 domain dashboards |
| Attention List (entity × signal rows) | M17 Customer, M18 Salesman, M19 Inventory Risk, M20 Collection, M21 Purchasing Management, M22 Location |
| `RequiresAttention` / Attention Indicator | M16 (Achievement bands + binary flags); M17–M22 (card-level flags) |
| Dedicated snapshot domains with refresh workers | M17 Customer through M22 Location |
| Drill-down path | Alert/dashboard row → owning domain dashboard → report (`?q=` pre-filter) |

**The remaining gap is not business logic — it is presentation and prioritization.** Management still asks:

> What requires attention **right now across the entire business**?

M23 should answer that question by **consuming** attention rows and headline flags from existing snapshot tables and aggregators — **not** by redefining classification rules, aging buckets, dormant thresholds, or concentration formulas.

### Key findings

| Finding | Implication |
| ------- | ----------- |
| **~35 distinct attention signal types** already materialized in `BTRPD_*Attention` tables and executive compose logic | M23 has a rich catalog of reusable alerts without new SQL against transactional tables |
| **Signal ownership is already bounded** by milestone aggregators (`Dashboard*Aggregator.cs`) | M23 must respect producer/consumer boundaries to avoid duplicate business logic |
| **M16 Executive Dashboard** composes **company-level** domain cards and Top 5 exposure lists — not a unified cross-domain attention feed | M16 and M23 serve different aggregation grains; relationship must be clarified (Section 15) |
| **Severity is intentionally minimal** — only Sales Achievement % uses Healthy/Warning/Critical bands; all other signals use binary Attention Indicator | M23 should not invent a generic severity engine without PO approval |
| **Alerts are snapshot-based**, not real-time — refresh intervals range **15–60 minutes** per domain | Business expectation aligns with **daily morning review**, not operational tick-by-tick monitoring |
| **Alert volume could be high** — attention lists are unbounded at source (all qualifying entity × signal rows) | Grouping, caps, and deduplication rules are business decisions (Sections 11, 17) |
| **Desktop has operational exception screens** (Faktur Control, Piutang Tracker, Lunas Piutang queue, Stok Balance Health) but **no unified alert product** | M23 is a **portal** capability; Desktop workflows inform drill-down destinations, not alert definitions |
| **Several executive promotions remain Phase 2 on M16** (M19 inventory risk, M20 collection recovery, M22 location concentration) | **Approved:** M23 may surface Phase 2 KPIs **before** M16 promotes them; M16 layout unchanged |

### Approved product outcome

Deliver **Alert Center** at **`/alerts`** (page title: **Alert Center**). **Do not replace** `/dashboard` (M16 Executive Dashboard remains landing page). Materialize nothing new — **aggregate existing attention snapshots** from M17–M22 plus selected executive/platform flags. **Cap** entity-level alerts at **Top 20 per category**; **M19 inventory risk = summary only** (no SKU dump). Separate **Alerts** and **Concentrations** sections. **Read-only** feed — no acknowledgment or history. **No new SignalKey types** in M23.

**User journey (approved):**

```text
What is happening?     →  M16 Executive Dashboard  (/dashboard)  [landing]
What needs attention?  →  M23 Alert Center         (/alerts)
Why?                   →  Domain Dashboard         (M11–M22)
Show me the evidence   →  Report                   (Sales / Piutang / Inventory / Purchasing)
```

M16 provides an **Open Alert Center** entry point; M23 is the action-oriented exception workspace.

---

## 2. Management Attention Discovery

This section identifies business situations requiring management attention, mapped to **existing BTR capabilities**. Items discovered from codebase, domain documentation, and milestone analyses — not an exhaustive prescriptive alert list.

### 2.1 Sales performance

| Situation | Business meaning | Primary owner | Availability |
| --------- | ---------------- | ------------- | ------------ |
| Company sales below monthly target | Plan miss risk | M16 / M11 Sales | **Portal today** — Achievement % with Warning/Critical bands |
| Salesman below target | Rep coaching / territory issue | M18 | **Portal today** — `BelowTarget` signal |
| Salesman active without configured target | Planning gap | M18 | **Portal today** — `NoTarget` signal |
| Sales billing deceleration (company or rep) | Month-end miss risk | M11 weekly trend | **Partial** — visual only; no automated alert flag |
| Customer revenue concentration | Dependency on few accounts | M17 | **Portal today** — Top Omzet % (informational) |
| Salesman customer concentration | Rep portfolio dependency | M18 | **Portal today** — `CustomerConcentration` (informational) |
| Unsigned Faktur backlog (`Kembali`) | Document return workflow incomplete | Sales Report row-level | **Partial** — no aggregate alert |
| Sales omzet data quality drift | RO2 vs Faktur mismatch | Desktop `SalesOmzetHealthWeekly` | **Desktop only** — excluded from portal management scope |

### 2.2 Customer relationship and credit

| Situation | Business meaning | Primary owner | Availability |
| --------- | ---------------- | ------------- | ------------ |
| Customer with overdue balance | Collection follow-up | M17, M20 | **Portal today** — multiple signals (see dedup Section 11) |
| Customer dormant (90-day rule) | Revenue attrition | M17 | **Portal today** — `Dormant` |
| Customer exceeds Plafond | Credit policy breach | M17, M20 | **Portal today** — `PlafondBreach` / `PlafondBreachOverdue` |
| Suspended customer still invoiced | Policy violation | M17 | **Portal today** — `SuspendedWithSales` |
| Customer chronic overdue (>90d exposure) | Severe collection failure | M20 | **Portal today** — `ChronicOverdue` |
| Legacy debt (dormant + open balance) | Low-recovery receivable | M20 | **Portal today** — `LegacyDebt` |
| High return customer | Quality / relationship issue | Desktop RF1 | **Not in portal** |

### 2.3 Collection and receivables

| Situation | Business meaning | Primary owner | Availability |
| --------- | ---------------- | ------------- | ------------ |
| Company overdue exposure | Working capital at risk | M14 Piutang, M16, M20 | **Portal today** |
| Severely aged receivables (>90 days) | Collection crisis indicator | M14, M16, M20 | **Portal today** |
| Low recovery vs billing pace | Collections lagging new debt | M20 | **Portal today** — `LowRecoveryVsBilling` (company + salesman) |
| Wilayah overdue hotspot | Regional collection concentration | M20 | **Portal today** — `WilayahHotspot` (≥15% of company overdue) |
| Salesman high overdue workload | Rep-level collection intervention | M18, M20 | **Portal today** — `HighOverdueExposure`, `HighOverdueWorkload` |
| Salesman high open piutang | Receivable concentration by rep | M18 | **Portal today** — `HighPiutangExposure` (informational) |
| DSO / aging trend deterioration | Portfolio quality worsening | — | **Not available** — no historical snapshots |
| Tagihan pipeline gaps | Billing step incomplete | Desktop FT2/FT5 | **Partial** — not aggregated |

### 2.4 Inventory and obsolescence

| Situation | Business meaning | Primary owner | Availability |
| --------- | ---------------- | ------------- | ------------ |
| Dead stock on hand | Capital in non-moving SKUs | M19 | **Portal today** — `DeadStock` item signals |
| Slow-moving inventory | Velocity loss | M19 | **Portal today** — `SlowMoving` |
| Never-sold SKUs with stock | Demand failure / bad intake | M19 | **Portal today** — `NeverSold` |
| At-risk inventory % elevated | Share of capital requiring attention | M19, M16 (Phase 2) | **Portal today** |
| Category/supplier inventory concentration | Capital stuck in one dimension | M15, M16 | **Portal today** — composition, not movement |
| Category/supplier **at-risk** concentration | Obsolescence clustered by dimension | M19 | **Portal today** — breakdown tables |
| Stock balance integrity mismatch | Data trust issue | Desktop `StokBalanceHealthDal` | **Desktop only** — IT/operations |

### 2.5 Purchasing and supplier dependency

| Situation | Business meaning | Primary owner | Availability |
| --------- | ---------------- | ------------- | ------------ |
| Qualified posting backlog (age ≥3 days, `BELUM`) | Purchase receipts delayed beyond staging | M21, M16 | **Portal today** — `QualifiedBacklog` |
| Raw unqualified `BELUM` count | May over-alert (default after save) | M11 V1 purchasing | **Portal today** — demoted by M21 qualified rule |
| Principal spend concentration | Supplier dependency (monthly intake) | M21, M16 | **Portal today** — `PrincipalSpendConcentration` |
| Principal inventory concentration | Stock dependency | M21 | **Portal today** — cross-read from M15 |
| Principal at-risk exposure | Idle stock from principal | M21 | **Portal today** — cross-read from M19 |
| Compound principal dependency | Same principal dominates purchase + inventory/risk | M21 | **Portal today** — `CompoundDependency` |
| Principal with inventory but no MTD purchase | Legacy stock / inactivity | M21 | **Portal today** — `PrincipalInventoryNoPurchase` |
| Purchasing inactivity (company-level) | Replenishment gap | M21 | **Portal today** — `PurchasingInactivity` |
| Unknown principal in rankings | Master data gap | M21 | **Portal today** — `UnknownPrincipal` |

### 2.6 Location and territory concentration

| Situation | Business meaning | Primary owner | Availability |
| --------- | ---------------- | ------------- | ------------ |
| Warehouse inventory concentration | Capital localized to one site | M22 | **Portal today** — Top-10 rank signals |
| Warehouse at-risk concentration | Obsolescence localized to one site | M22 | **Portal today** — composes M19 classification × warehouse |
| Warehouse sales / purchasing concentration | Operational dependency on one site | M22 | **Portal today** |
| Warehouse with stock but no MTD sales | Dormant billing site | M22 | **Portal today** — `WarehouseNoSalesWithInventory` |
| Inactive warehouse still holding stock | Legacy site tying capital | M22 | **Portal today** — `WarehouseInactiveWithStock` |
| Wilayah sales concentration | Territory billing dependency | M22 | **Portal today** — ranking (not attention list) |
| Wilayah overdue hotspot | Regional receivable risk | **M20** | **Portal today** — **not M22** (cross-link only) |

### 2.7 Platform and data trust

| Situation | Business meaning | Primary owner | Availability |
| --------- | ---------------- | ------------- | ------------ |
| Dashboard snapshot stale | Decisions on outdated analytics | M16 / health endpoint | **Portal today** — `IsDataFresh`, staleness banner |
| Snapshot refresh failed (`degraded`) | Analytics incomplete | `GET /api/health/dashboard-snapshots` | **Portal today** |
| Domain snapshot unavailable | Partial dashboard | Executive `HasUnavailableDomain` | **Portal today** |
| Cross-domain correlated crisis | e.g., sales up, recovery down, dead stock rising | — | **Not computed** — requires PO definition |

---

## 3. Existing Attention Signal Discovery

### 3.1 Dashboard attention inventory

| Milestone | Route | Attention Cards | Attention List | Concentration / Risk Indicators | Rankings (context) |
| --------- | ----- | --------------- | -------------- | ------------------------------- | ------------------ |
| **M16** Executive | `/dashboard` | Sales, Piutang, Purchasing, Inventory, System | Top 5 Customers, Categories, Suppliers, Principals (exposure lists, not signal rows) | Top Customer/Category/Supplier/Principal %; Achievement bands; >90d % | None (Top 5 only) |
| **M11** Sales | `/dashboard/sales` | None (KPI-focused) | None | Achievement % | Top 10 Salesman |
| **M14** Piutang | `/dashboard/piutang` | Implicit via KPIs | None | Aging buckets; Overdue Customer | Top 10 Customers |
| **M17** Customer | `/dashboard/customers` | Collection, Concentration, Activity, Inactivity, Credit | Customer × 4 signals | Top Omzet/Piutang % | Top 10 Omzet, Top 10 Piutang |
| **M18** Salesman | `/dashboard/salesmen` | Performance, Collection Exposure, Portfolio | Salesman × 6 signals | Top Omzet/Piutang Salesman % | Top 10 Omzet, Achievement %, Piutang |
| **M19** Inventory Risk | `/dashboard/inventory-risk` | Dead/Slow/Never counts, At-Risk % | Item × 3 signals | Category/Supplier risk exposure | Top 10 Dead, Top 10 Slow |
| **M20** Collection | `/dashboard/collection` | Overdue, Recovery, Concentration, Wilayah | Customer × 4 + Salesman × 2 + Wilayah × 1 signals | Overdue concentration %; Recovery vs Billing % | Top Overdue Customer/Salesman/Wilayah |
| **M21** Purchasing Mgmt | `/dashboard/purchasing` | Posting, Dependency, Pace, Cross-Risk | Principal × 8 signals | Posted %; qualified backlog | Top 10 Principal % |
| **M22** Location | `/dashboard/locations` | Warehouse/Wilayah concentration cards | Warehouse × 6 signals | Top 1 / Top 3 warehouse % | Top warehouse/wilayah ranking tables |
| **M15** Inventory | `/dashboard/inventory` | None | None | Category/Supplier composition | Top 10 Category, Supplier |

### 3.2 Authoritative signal catalog (materialized)

Signal keys are defined in application aggregators and persisted in `BTRPD_*Attention` snapshot tables (where applicable).

#### Customer signals (M17 — `DashboardCustomerAggregator`)

| SignalKey | SignalLabel | Inclusion rule (summary) |
| --------- | ----------- | ------------------------ |
| `Overdue` | Overdue | Any overdue balance on customer |
| `Dormant` | Dormant | 90-day dormant rule with prior Faktur history |
| `PlafondBreach` | Plafond Breach | Open balance > Plafond when Plafond > 0 |
| `SuspendedWithSales` | Suspended + Sales | `IsSuspend` and Faktur in current month |

#### Salesman signals (M18 — `DashboardSalesmanAggregator`)

| SignalKey | SignalLabel | Inclusion rule (summary) |
| --------- | ----------- | ------------------------ |
| `BelowTarget` | Below Target | Target > 0 and achievement in Warning/Critical band |
| `NoTarget` | No Target | Month activity without configured target |
| `HighOverdueExposure` | High Overdue Exposure | Any overdue on rep's invoiced open Faktur |
| `HighPiutangExposure` | High Piutang Exposure | Open piutang > 0 for rep (informational) |
| `CustomerConcentration` | Customer Concentration | Top-customer % of rep omzet computable |
| `DormantCustomerPortfolio` | Dormant Customer Portfolio | ≥1 dormant customer on rep's book (last-invoicing attribution) |

#### Inventory risk signals (M19 — `DashboardInventoryRiskAggregator`)

| SignalKey | SignalLabel | Inclusion rule (summary) |
| --------- | ----------- | ------------------------ |
| `DeadStock` | Dead Stock | Last Faktur idle ≥ 180 days |
| `SlowMoving` | Slow Moving | Idle 90–179 days (excludes Never Sold) |
| `NeverSold` | Never Sold | Stock with no FakturItem history |

#### Collection signals (M20 — `DashboardCollectionAggregator`)

| SignalKey | SignalLabel | Entity grain | Inclusion rule (summary) |
| --------- | ----------- | ------------ | ------------------------ |
| `ChronicOverdue` | Chronic Overdue | Customer | Customer with >90d bucket exposure |
| `PlafondBreachOverdue` | Plafond Breach + Overdue | Customer | Plafond breach with overdue |
| `LegacyDebt` | Legacy Debt | Customer | M17 dormant rule + open balance |
| `Overdue` | Overdue | Customer | Any overdue balance |
| `HighOverdueWorkload` | High Overdue Workload | Salesman | Elevated overdue portfolio (aggregator rule) |
| `LowRecoveryVsBilling` | Low Recovery vs Billing | Salesman | Recovery pace below billing (aggregator rule) |
| `WilayahHotspot` | Wilayah Hotspot | Wilayah | Wilayah overdue ≥ **15%** of company overdue |

**Customer signal priority (M20):** ChronicOverdue > PlafondBreachOverdue > LegacyDebt > Overdue.

#### Purchasing management signals (M21 — `DashboardPurchasingManagementAggregator`)

| SignalKey | SignalLabel | Inclusion rule (summary) |
| --------- | ----------- | ------------------------ |
| `QualifiedBacklog` | Qualified Backlog | `BELUM` and LastUpdate age ≥ 3 days (configurable) |
| `PrincipalSpendConcentration` | Spend Concentration | Principal in Top 10 MTD spend |
| `PrincipalInventoryConcentration` | Inventory Concentration | Principal in Top 10 inventory value (M15 read) |
| `PrincipalAtRiskExposure` | At-Risk Exposure | Principal in Top 10 at-risk value (M19 read) |
| `CompoundDependency` | Compound Dependency | Top spend AND (Top inventory OR Top at-risk) |
| `PrincipalInventoryNoPurchase` | Inventory, No Purchase | Top inventory principal with zero MTD purchase |
| `PurchasingInactivity` | Purchasing Inactivity | Company-level inactivity flag |
| `UnknownPrincipal` | Unknown Principal | Blank supplier in qualified contexts |

#### Location signals (M22 — `DashboardLocationAggregator`)

| SignalKey | SignalLabel | Priority | Inclusion rule (summary) |
| --------- | ----------- | -------- | ------------------------ |
| `WarehouseInactiveWithStock` | Inactive With Stock | 1 | `IsAktif = false` and inventory > 0 |
| `WarehouseNoSalesWithInventory` | Stock Without Sales | 2 | Inventory > 0, no MTD Faktur from warehouse |
| `WarehouseAtRiskConcentration` | At-Risk Concentration | 3 | Warehouse in Top 10 at-risk share |
| `WarehouseInventoryConcentration` | Inventory Concentration | 4 | Warehouse in Top 10 inventory share |
| `WarehouseSalesConcentration` | Sales Concentration | 5 | Warehouse in Top 10 sales share |
| `WarehousePurchasingConcentration` | Purchasing Concentration | 6 | Warehouse in Top 10 purchasing share |

#### Executive-level flags (M16 — `DashboardExecutiveComposer`)

| Domain | RequiresAttention when | Notes |
| ------ | ---------------------- | ----- |
| Sales | Achievement band Warning or Critical | Only domain with named severity bands |
| Piutang | OverdueCustomer > 0 OR >90d amount > 0 | Company aggregate |
| Purchasing | QualifiedBacklogCount > 0 (from M21 snapshot) | Uses qualified backlog, not raw BELUM |
| Inventory | Top Category % computable (informational flag) | Weak movement signal until M19 Phase 2 executive promotion |
| System | Snapshot stale or health degraded | Platform attention |

### 3.3 Traceability matrix — signal to source

| Signal / Alert candidate | Producer milestone | Snapshot table / API source | Owning aggregator | Validating drill-down |
| ------------------------ | ------------------ | --------------------------- | ----------------- | --------------------- |
| Below Target (company) | M16 | `BTRPD_SalesKpi` | `DashboardSalesFakturAggregator` + `DashboardExecutiveComposer` | Sales Dashboard → Sales Report |
| Below Target (rep) | M18 | `BTRPD_SalesmanAttention` | `DashboardSalesmanAggregator` | Salesman Dashboard → Sales Report `?q=` |
| Customer Overdue | M17 | `BTRPD_CustomerAttention` | `DashboardCustomerAggregator` | Customer Dashboard → Piutang Report `?q=` |
| Customer Dormant | M17 | Same | Same | Customer Dashboard → Sales Report `?q=` |
| Chronic Overdue | M20 | `BTRPD_CollectionAttention` | `DashboardCollectionAggregator` | Collection Dashboard → Piutang Report |
| Legacy Debt | M20 | Same | Same (uses M17 dormant rule) | Collection Dashboard → Piutang Report |
| Wilayah Hotspot | M20 | Same | Same | Collection Dashboard (not M22) |
| Dead Stock (item) | M19 | `BTRPD_InventoryRiskAttention` | `DashboardInventoryRiskAggregator` | Inventory Risk → Inventory Report `?q=` |
| Qualified Backlog | M21 | `BTRPD_PurchasingManagementAttention` | `DashboardPurchasingManagementAggregator` | Purchasing Dashboard → Purchasing Report (filter BELUM) |
| Compound Dependency | M21 | Same | Same (reads M15/M19 snapshots) | Purchasing Dashboard → Inventory / Inventory Risk |
| Warehouse Inactive With Stock | M22 | `BTRPD_LocationAttention` | `DashboardLocationAggregator` | Location Dashboard → Inventory Report |
| Snapshot stale | M16 | Health endpoint + `GeneratedAt` | `DashboardExecutiveComposer` | Admin refresh / worker health |
| Top 5 Customer exposure | M16 | `BTRPD_PiutangTopCustomer` | `DashboardPiutangAggregator` (presentation truncate) | Executive → Piutang Dashboard → Piutang Report |

**Implication for M23:** Every row in this matrix is a **read-and-route** candidate. M23 should store **references** to producer signal identity (domain, entity, SignalKey, snapshot row id or composite key) rather than duplicate inclusion logic.

---

## 4. Alert Ownership Analysis

**Approved principle:** Signal Produced → Dashboard Owns Logic → M23 Displays Alert.

| Business concept | Logic owner | M23 role | Must not duplicate |
| ---------------- | ----------- | -------- | ------------------ |
| Customer credit & activity signals | **M17** | Display `CustomerAttention` rows | Dormant 90-day rule, Plafond breach, Suspended+Sales |
| Salesman performance & portfolio signals | **M18** | Display `SalesmanAttention` rows | Achievement bands, FF1 piutang attribution, dormant rollup |
| Item movement classification | **M19** | Display `InventoryRiskAttention` rows (possibly capped) | 90/180-day Last Faktur rules, Never Sold exclusion |
| Collection effectiveness & overdue prioritization | **M20** | Display `CollectionAttention` rows | Recovery vs Billing %, WilayahHotspot 15%, customer signal priority |
| Purchasing backlog & principal dependency | **M21** | Display `PurchasingManagementAttention` rows | Qualified backlog days, compound dependency join |
| Warehouse / location concentration | **M22** | Display `LocationAttention` rows | Top-10 concentration rank rules, inactive warehouse |
| Company receivable exposure (aging, overdue count) | **M14** + **M16** | Optional headline alerts only | Aging bucket boundaries |
| Company sales target | **M11** + **M16** | Headline Achievement alert | `SalesOmzetChartAchievementPolicy` |
| Inventory composition concentration | **M15** + **M16** | Headline only; detail on M15 | BrgId-first inventory rollup |
| Wilayah collection risk | **M20** | Full alert ownership | M22 links only |
| Cross-domain principal inventory risk | **M19** (classification) + **M21** (principal framing) | M23 shows M21 row; links to M19 | At-risk value calculation |

**Executive composer (`DashboardExecutiveComposer`)** is a **presentation composer**, not a signal logic owner. It reads domain snapshots and M21 qualified backlog — M23 may read the same sources for headline cards but should not become a third calculator.

---

## 5. Alert Categorization Analysis

Categories should reflect **BTR business areas** and **existing dashboard structure**, not a new taxonomy invented for M23.

### 5.1 Recommended primary categories (discovered)

| Category | Business area | Primary signal sources | Typical entity grain |
| -------- | ------------- | ---------------------- | -------------------- |
| **Sales** | Sales performance | M16 Achievement, M18 salesman performance signals | Company, Salesman |
| **Customer** | Customer relationship & credit | M17 customer signals | Customer |
| **Collection** | Receivable recovery & overdue workload | M20 collection signals; M14/M16 headline exposure | Customer, Salesman, Wilayah |
| **Inventory** | Stock health & obsolescence | M19 item signals; M16/M15 composition context | Item (SKU) |
| **Purchasing** | Supplier intake & posting workflow | M21 principal signals | Principal (Supplier) |
| **Location** | Warehouse & territory concentration | M22 warehouse signals; M22 wilayah rankings (informational) | Warehouse |
| **Platform** | Data freshness & snapshot health | M16 system card, health endpoint | System |

### 5.2 Secondary grouping dimensions (for filtering, not replacement)

| Dimension | Use |
| --------- | --- |
| **Entity type** | Customer, Salesman, Item, Principal, Warehouse, Wilayah, Company |
| **Signal type** | Exception, Risk, Concentration, Operational, Anomaly |
| **Domain dashboard** | Owning route for drill-down |
| **Operational vs managerial** | See Section 5.3 |

### 5.3 Operational vs managerial alerts

| Type | Examples | Typical user | Urgency character |
| ---- | -------- | ------------ | ----------------- |
| **Operational** | QualifiedBacklog, WarehouseInactiveWithStock, SuspendedWithSales, snapshot degraded | Admin, warehouse, finance ops | Process completion / data fix |
| **Managerial** | BelowTarget, ChronicOverdue, CompoundDependency, WilayahHotspot, DeadStock concentration | Management, department heads | Strategic intervention / resource allocation |
| **Informational concentration** | CustomerConcentration, PrincipalSpendConcentration, WarehouseSalesConcentration (Top-10 rank without threshold) | Management | Monitor — may not require immediate action |

**Note:** BTR deliberately avoids automatic thresholds on concentration metrics (M17/M18/M21 PO decisions). M23 should preserve **informational** vs **exception** distinction when categorizing.

---

## 6. Alert Severity Analysis

BTR does **not** implement a generic multi-level severity engine. Business meaning of severity levels **without threshold values**:

| Severity (conceptual) | Business meaning in BTR | Where already used |
| --------------------- | ------------------------- | ------------------ |
| **Critical** | Immediate management intervention likely; significant plan miss or systemic failure | Sales Achievement **<80%** (M16 band name only) |
| **Warning** | Attention warranted; trend or policy breach | Sales Achievement **80–99%**; binary `RequiresAttention` on many cards |
| **Information** | Awareness; concentration or exposure visible without automatic escalation | Top N concentration %; HighPiutangExposure (informational by design) |
| **Healthy / None** | No attention flag | Achievement ≥100%; zero qualifying signals |

### 6.1 Signals with implicit severity hierarchy (within domain)

| Domain | Priority order | Business interpretation |
| ------ | -------------- | ----------------------- |
| M20 Customer collection | ChronicOverdue → PlafondBreachOverdue → LegacyDebt → Overdue | Escalating collection urgency |
| M22 Warehouse location | InactiveWithStock → NoSalesWithInventory → At-Risk → Inventory → Sales → Purchasing | Operational exception before concentration |
| M21 Purchasing | QualifiedBacklog → CompoundDependency → … → PurchasingInactivity | Process backlog before statistical concentration |

### 6.2 M23 severity recommendations (business only)

| Approach | Pros | Cons |
| -------- | ---- | ---- |
| **Reuse M16 Achievement bands only** for sales alerts; all else binary | Consistent with existing product | Limited cross-domain prioritization |
| **Map producer priority order to Warning vs Information** | Uses existing aggregator sort keys | Requires PO agreement per signal |
| **Introduce Critical/Warning/Info engine with thresholds** | Rich prioritization | **Rejected in M16/M17** unless PO reopens decision |

**Analyst recommendation:** M23 should inherit **producer-defined priority** (SortOrder, signal priority constants) and **Achievement bands** where applicable — not invent new threshold formulas.

---

## 7. Executive Attention Analysis

Which signals merit **company-wide management center** visibility (executive-level candidates):

### 7.1 Already on M16 Executive Dashboard

| Signal | Executive presentation |
| ------ | ------------------------ |
| Sales Achievement % (Healthy/Warning/Critical) | Attention card |
| Overdue Customer count | Piutang card |
| >90 Day amount and % of Total Piutang | Piutang card |
| Qualified posting backlog count/value | Purchasing card |
| Top Category % (inventory) | Inventory card |
| Top 5 Customers, Categories, Suppliers, Principals | Critical exposure lists |
| Snapshot health / Last Refreshed | System card / banner |

### 7.2 Approved for executive promotion (Phase 2 — not yet on `/dashboard`)

| Source milestone | Candidate executive metrics |
| ---------------- | --------------------------- |
| M19 | Dead Stock Value, At-Risk Inventory %, Inventory Risk RequiresAttention |
| M20 | Cash Collected MTD, Recovery vs Billing %, Overdue Concentration % |
| M22 | Top Warehouse Inventory %, Top Warehouse Sales %, Inactive Warehouse With Stock count |

### 7.3 High-value M23 executive feed candidates (cross-domain)

Signals important enough for **Alert Center headline** or **top-of-feed** placement even if not yet on M16:

| Alert theme | Rationale | Source |
| ----------- | --------- | ------ |
| Company Recovery vs Billing low | Cash conversion crisis | M20 KPI |
| Wilayah Hotspot | Regional collection emergency | M20 `WilayahHotspot` |
| Qualified Backlog elevated | Operational blockage across receiving | M21 |
| Compound Principal Dependency | Supply + inventory systemic risk | M21 |
| At-Risk Inventory % elevated | Capital obsolescence | M19 KPI |
| Warehouse Inactive With Stock | Legacy capital trap | M22 |
| Snapshot degraded | Trust in all other alerts compromised | Platform |

### 7.4 Signals better left domain-deep (not Alert Center entity rows)

| Signal | Rationale |
| ------ | --------- |
| Individual item Dead/Slow/Never Sold rows | **M19 summary only in M23** — full list on Inventory Risk dashboard |
| CustomerConcentration (informational %) | **Concentrations section** — not default Alerts feed |
| Every salesman BelowTarget row | Capped at Top 20 per Sales category |
| Top-10 warehouse concentration ranks (informational) | **Concentrations section** or M22 — not mixed with exceptions |

**Approved:** M23 surfaces Phase 2 executive candidates before M16 promotes them. M16 unchanged.

## 8. Existing Dashboard Reuse Analysis

### 8.1 Reusable snapshot assets (primary — read only)

| Asset | Tables / endpoints | M23 reuse |
| ----- | ------------------ | --------- |
| Customer attention rows | `BTRPD_CustomerAttention` | Direct feed |
| Salesman attention rows | `BTRPD_SalesmanAttention` | Direct feed |
| Inventory risk attention rows | `BTRPD_InventoryRiskAttention` | **Summary only** in M23 — KPI counts from snapshot; not row feed |
| Collection attention rows | `BTRPD_CollectionAttention` | Direct feed |
| Purchasing management attention rows | `BTRPD_PurchasingManagementAttention` | Direct feed |
| Location attention rows | `BTRPD_LocationAttention` | Direct feed |
| Executive compose input | Sales/Piutang/Inventory/Purchasing snapshots + M21 management KPI | Headline alerts |
| Refresh log | `BTRPD_RefreshLog` | Platform alerts |
| Domain KPI snapshots | All `BTRPD_*Kpi` | Alert context values (amounts, counts) |

### 8.2 Reusable UI components (presentation patterns)

| Component | Path | M23 reuse |
| --------- | ---- | --------- |
| `ExecutiveAttentionCard.vue` | `components/dashboard/` | Category summary cards |
| `*AttentionCardGroup.vue` | Customer, Salesman, Collection, Location, Purchasing | Card layout patterns |
| `*AttentionList.vue` | Same family | Row layout, signal badge, drill-down |
| `Top10RankingTable.vue` | Rankings with % column | Optional "top alerts by exposure" panel |
| Staleness banner pattern | M16 home / `DashboardDetailLayout` | Platform alert banner |
| `navigateToReport.ts` / `?q=` pre-filter | M17+ drill-down | Alert row navigation |

### 8.3 Reusable business rules (do not reimplement in M23)

| Rule | Authoritative location |
| ---- | ---------------------- |
| Open balance `KurangBayar > 1` | Piutang aggregators |
| Aging bucket boundaries | `DashboardPiutangAggregator` |
| Dormant 90-day + prior history | `DashboardCustomerAggregator` |
| Achievement % formula and 80/100 bands | `SalesOmzetChartAchievementPolicy`, `ExecutiveSalesAchievementBandResolver` |
| Inventory movement 90/180-day classification | `DashboardInventoryRiskAggregator` |
| Qualified backlog ≥3 days | `DashboardPurchasingManagementAggregator` + `PurchasingQualifiedBacklogDays` |
| WilayahHotspot ≥15% overdue share | `DashboardCollectionAggregator` |
| BrgId-first inventory valuation | `DashboardInventoryItemGroupBuilder` |

### 8.4 What M23 should NOT rebuild

| Avoid | Reason |
| ----- | ------ |
| New SQL against `BTR_Faktur`, `BTR_Piutang`, `BTR_Invoice`, `BTR_StokBalanceWarehouse` for alert qualification | Violates signal ownership |
| Unified cross-domain scoring formula mixing unrelated domains | Business policy undefined |
| Historical alert state / acknowledgment tables | Out of analysis scope unless PO wants workflow |
| Real-time triggers on payment posting | Snapshots are batch-refreshed |

---

## 9. Existing Desktop Capability Analysis

BTR Desktop does **not** provide a unified Alert Center. It provides **operational screens** that inform what portal alerts should link to — not what portal should recalculate.

### 9.1 Monitoring and exception screens (Desktop)

| Screen | Menu | Alert-like behavior | Portal M23 relationship |
| ------ | ---- | ------------------- | ------------------------- |
| **Faktur Control** | Faktur module | Filters Kembali / Belum Kembali unsigned Faktur | **No portal alert today** — potential future signal; drill via Sales Report |
| **Piutang Sales Wilayah (FF1)** | Finance | Full open balance grid with Sales, Wilayah, Retur | Validates M17/M18/M20 piutang-attributed alerts |
| **Penerimaan Pelunasan Sales (FF2)** | Finance | Daily collections by salesman | Validates M20 recovery KPIs |
| **Lunas Piutang (FT1)** | Finance | Route-day collection queue | Operational workflow — not aggregate alert source |
| **Piutang Tracker (FT5)** | Finance | Per-Faktur lifecycle timeline | Drill-down audit — not alert feed |
| **Tagihan / Tanda Terima (FT2/FT3)** | Finance | Billing workflow states | Partial data — excluded from M20 headline KPIs |
| **Sales Omzet Chart (RO2)** | Sales | Per-rep target vs achievement | Desktop richer than portal — M18 owns portal signals |
| **Effective Call (RO3)** | Sales | Visit effectiveness | **M25** — not M23 |
| **Kartu Stok Summary (IF8)** | Inventory | Zero movement with closing stock | Desktop movement logic — M19 uses Last Faktur instead for portal |
| **Stok Balance Health** | Inventory admin | Balance vs FIFO mismatch | IT operational — exclude from management Alert Center |
| **Sales Omzet Health Weekly** | Sales admin | RO2 reconciliation Good/Warning/Poor | IT operational — excluded from M16 |
| **Posting Stok (PT2)** | Purchasing | Completes `BELUM` → `SUDAH` | Operational resolution of M21 QualifiedBacklog alerts |

### 9.2 Follow-up lists and reminders

| Mechanism | Finding |
| --------- | ------- |
| Structured collection follow-up (calls, PTP dates) | **Not in BTR data model** |
| Route-day collection queue (FT1) | Operational list for collectors — not management alert catalog |
| Tagihan re-bill flags | Event-level — not aggregated alert |
| Windows Task Scheduler for portal worker | **Infrastructure scheduling**, not business reminder |

**Conclusion:** M23 Alert Center is a **portal analytics product** aggregating **materialized dashboard attention rows**. Desktop remains the **transaction and workflow** layer for acting on alerts (post stock, record payment, adjust master data).

---

## 10. Alert Lifecycle Analysis

### 10.1 Refresh cadence by domain (authoritative)

| Snapshot domain | Default interval | Business freshness expectation |
| --------------- | ---------------- | ------------------------------ |
| Piutang | 15 minutes | Most time-sensitive exposure |
| Sales, Purchasing, Customer, Salesman, Collection, PurchasingManagement | 30 minutes | Daily management review sufficient |
| Inventory, InventoryRisk, Location | 60 minutes | Slower-moving capital metrics |

Alerts reflect **last successful snapshot refresh**, not live transactional state. Reports query Desktop DALs live and may differ slightly from alert values until next refresh.

### 10.2 Alert temporal behavior (business expectations)

| Behavior | BTR reality | Management expectation |
| -------- | ----------- | ---------------------- |
| **Real-time** | Not supported for dashboard alerts | Users accept batch refresh if timestamp visible |
| **Snapshot-based** | All M17–M22 attention rows | Primary M23 model |
| **Daily** | Aligns with M16 "morning review" cadence | Alert Center checked once per day |
| **Weekly** | No weekly alert rollup | Would require new aggregation — out of scope |
| **Alert persistence** | Signals exist while qualification true at refresh | Alert disappears when underlying condition clears on next refresh |
| **Alert history** | Not retained in snapshots | No "what alerted last week" without new history store |

### 10.3 Event-driven refresh

Payment posting, Faktur creation, and stock posting **do not** trigger immediate snapshot rebuild. Admin may run `POST /api/admin/dashboard/refresh` or worker CLI — subject to timeout on full rebuild.

**Business implication:** M23 must show **Last Refreshed** (consolidated or per-domain) and staleness warnings — same trust model as M16.

### 10.4 Influence on alert wording

| Domain | Staleness sensitivity |
| ------ | --------------------- |
| Collection / Piutang | Higher — overdue counts change with payments |
| Inventory risk | Lower — classification changes daily at most |
| Purchasing qualified backlog | Medium — depends on invoice LastUpdate dates |

---

## 11. Alert Prioritization Analysis

Management may prioritize alerts through several **business approaches** (no scoring formulas defined here):

### 11.1 Financial risk first

| Priority lens | Example alerts |
| ------------- | -------------- |
| Cash recovery | LowRecoveryVsBilling, ChronicOverdue, WilayahHotspot |
| Working capital | DeadStock value, QualifiedBacklog value, LegacyDebt |
| Concentration | Top Customer overdue, CompoundDependency |

### 11.2 Operational blockage first

| Priority lens | Example alerts |
| ------------- | -------------- |
| Process completion | QualifiedBacklog, SuspendedWithSales |
| Site integrity | WarehouseInactiveWithStock |
| Data trust | Snapshot degraded |

### 11.3 Sales performance first

| Priority lens | Example alerts |
| ------------- | -------------- |
| Plan miss | Company Achievement Critical, BelowTarget reps |
| Coverage loss | DormantCustomerPortfolio, Dormant customers |

### 11.4 Producer-defined priority (recommended baseline)

Each aggregator already assigns **SortOrder** or **signal priority integers**. M23 can merge feeds by:

1. Platform health alerts first (data untrustworthy)  
2. Producer signal priority within domain  
3. Entity name / exposure amount as tie-breaker  

**Cross-domain priority** (e.g., Collection vs Inventory vs Purchasing) is **undefined in BTR** — requires PO policy (Section 17).

### 11.5 Volume control strategies (alert fatigue — business behavior)

| Strategy | Business behavior | Tradeoff |
| -------- | ----------------- | -------- |
| **Cap per category** (e.g., top 20 rows) | Feed stays scannable | Lower-priority entities hidden |
| **Headline + drill to domain** | Alert Center shows counts; detail on owning dashboard | Extra navigation click |
| **Collapse informational concentration** | Show count only unless user expands | May hide emerging risk |
| **Deduplicate cross-milestone** | One alert per underlying condition | Requires ownership rules (Section 11.6) |
| **Separate Operational vs Managerial tabs** | Ops team vs management sees different default sort | Role-based — **not in portal today** |

---

## 12. Alert Deduplication Analysis

Multiple dashboards may surface **related** conditions. M23 must apply **ownership boundaries** to prevent duplicate alerts for the same underlying business situation.

### 12.1 Deduplication matrix (authoritative — Section 17)

| Business situation | Producing signals | **Canonical alert owner for M23** | Suppress or link from others |
| ------------------ | ----------------- | ----------------------------------- | ------------------------------ |
| Customer has overdue balance | M17 `Overdue`; M20 `Overdue` / `ChronicOverdue` | **M20 wins** (Chronic > PlafondBreach+Overdue > Legacy > Overdue) | Show **one row** — M20 only when overlap |
| Customer dormant | M17 `Dormant`; M18 `DormantCustomerPortfolio` (rep rollup) | **M17** for customer entity; **M18** for salesman portfolio entity | Different entity grain — **not duplicate** |
| Customer dormant with open debt | M17 Dormant + balance; M20 `LegacyDebt` | **M20 `LegacyDebt` only** | **Suppress** M17 Dormant when LegacyDebt applies |
| Salesman overdue exposure | M18 `HighOverdueExposure`; M20 `HighOverdueWorkload` | **M20 wins** — collection management owns workload | Suppress M18 row when M20 workload row exists for same rep |
| Company overdue / >90d | M14 KPIs; M16 executive cards; M20 exposure KPIs | **M16 headline** for company aggregate; **M20** for collection detail feed | M23 headline ≠ per-customer rows |
| Dead stock capital | M19 item `DeadStock`; M22 `WarehouseAtRiskConcentration` | **M19** owns item classification; **M22** owns warehouse concentration | Both valid at different grains — link M22 → M19 |
| Supplier/principal risk | M16 Top Principal %; M21 multiple principal signals; M19 supplier risk exposure | **M21** for management attention list; M16 for executive % only | M23 feed from M21 rows, not recomputed Top % |
| Qualified posting backlog | M16 purchasing card; M21 `QualifiedBacklog` | **M21** (logic owner); M16 reads M21 count | Single alert |
| Wilayah overdue | M20 `WilayahHotspot`, Top Overdue Wilayah; M22 wilayah sales rankings | **M20** owns receivable geography | M22 links to M20 — **no WilayahHotspot in M22 feed** |
| Plafond breach | M17 `PlafondBreach`; M20 `PlafondBreachOverdue` | **M20** when overdue present; else **M17** | Merge to one customer row |
| Inventory concentration | M15 Top Category; M16 Top Category %; M19 category risk exposure | **M19** for at-risk; **M15/M16** for total composition | Different meaning — **do not dedupe** |
| Achievement below target | M16 company band; M18 `BelowTarget` per rep | Different grain — **both valid** | Company alert + rep alerts |

### 12.2 Entity grain rule

**Alerts at different entity grains are not duplicates** even when narratively related:

- Customer alert ≠ Salesman alert ≠ Item alert ≠ Principal alert ≠ Warehouse alert  
- Company headline KPI ≠ entity attention row  

### 12.3 Cross-link vs duplicate

When M22 surfaces warehouse at-risk concentration, M23 should **link** to M19 for item detail — not re-list every dead stock item under the warehouse alert.

---

## 13. Navigation Analysis

### 13.1 Approved drill-down path (existing convention)

```text
Alert Center (M23)
    ↓
Owning Domain Dashboard (M11–M22)
    ↓
Report (Sales / Piutang / Inventory / Purchasing)
    ↓
BTR Desktop transaction (operational resolution — outside portal)
```

### 13.2 Navigation map by alert category

| Category | Primary dashboard destination | Report destination | Notes |
| -------- | ----------------------------- | ------------------ | ----- |
| Sales (company) | `/dashboard/sales` | `/reports/sales` | Validate Achievement |
| Sales (rep) | `/dashboard/salesmen` | `/reports/sales?q={salesman}` | Contextual pre-filter |
| Customer | `/dashboard/customers` | `/reports/sales` or `/reports/piutang?q={customer}` | Signal-dependent |
| Collection | `/dashboard/collection` | `/reports/piutang` | Sort by Jatuh Tempo |
| Piutang exposure (company) | `/dashboard/piutang` | `/reports/piutang` | M14 depth |
| Inventory risk (item) | `/dashboard/inventory-risk` | `/reports/inventory?q={item}` | Item name search |
| Inventory composition | `/dashboard/inventory` | `/reports/inventory` | Capital context |
| Purchasing | `/dashboard/purchasing` | `/reports/purchasing` | Filter BELUM for backlog |
| Location (warehouse) | `/dashboard/locations` | `/reports/inventory?q={warehouse}` | Warehouse name filter |
| Location (wilayah collection) | `/dashboard/collection` | `/reports/piutang` | **Not** M22 for overdue |
| Platform | `/dashboard` (executive) or admin refresh | `/api/health/dashboard-snapshots` | Operational response |

### 13.3 Attention row metadata already supports navigation

Snapshot attention rows include `ReportRoute` (where implemented) and entity codes/names for `?q=` pre-filter — M23 should **preserve** producer routes rather than invent central routes.

---

## 14. Gap Analysis

### 14.1 Information already available (ready for M23 aggregation)

| Capability | Source |
| ---------- | ------ |
| Six domain attention list snapshot tables with SignalKey, entity, values, sort order | M17–M22 |
| Executive domain RequiresAttention flags and Top 5 exposure lists | M16 composer |
| Qualified backlog semantics aligned with executive purchasing card | M21 + M16 |
| Signal priority constants in aggregators | Application layer |
| Refresh health and per-domain GeneratedAt | Refresh log + KPI tables |
| UI patterns for cards, lists, staleness banner | Portal web components |
| Drill-down to reports with pre-filter | M17+ navigation |

### 14.2 Information partially available (M23 business decisions needed)

| Capability | Gap |
| ---------- | --- |
| **Unified alert feed** | No single view — must merge reads |
| **Cross-domain priority policy** | Producer priorities exist; global order undefined |
| **Deduplication when M17 and M20 overlap** | Rules proposed Section 12 — PO confirmation needed |
| **Executive Phase 2 metrics in alert feed** | KPIs exist on domain dashboards; not on M16 |
| **Alert counts by category** | Summable from snapshots — presentation choice |
| **Role-based alert views** | All users see same menus today |

### 14.3 Information not currently available

| Capability | Disposition |
| ---------- | ----------- |
| Alert acknowledgment / snooze / assign | No BTR workflow — would be new product scope |
| Alert history / "new since yesterday" | No snapshot history — requires retention policy |
| Real-time alerts on payment or posting events | Event-driven refresh not implemented |
| Faktur Kembali backlog aggregate alert | Row-level only in Sales Report |
| Effective call / route compliance alerts | **M25** |
| Retur-heavy customer/principal alerts | Desktop only |
| DSO / aging trend alerts | No historical time series |
| Cross-domain composite score ("company health index") | Not in BTR |
| CRM-style collection follow-up reminders | Not in data model |
| Email / push notification delivery | Not in portal scope |
| Separate Collector entity alerts | No collector dimension |

---

## 15. Relationship to Other Milestones

### 15.1 M16 vs M23 — distinct roles

| Aspect | M16 Executive Dashboard (`/dashboard`) | M23 Alert Center (`/alerts`) |
| ------ | -------------------------------------- | ---------------------------- |
| Title (unchanged) | Management Attention Center | **Alert Center** |
| Primary question | **What is happening?** — Executive Summary | **What needs attention?** — Exception Management |
| Role | **Landing page** — first screen after login | **Action workspace** — reached from M16 or sidebar |
| Aggregation grain | Domain KPI cards + Top 5 exposure lists | Entity-level attention signals (capped) + concentrations |
| Top 5 exposure lists | **Yes** — context on M16 | **No** in M23 — not alerts |
| Signal logic owner | Composer (presentation only) | **None** — consumer of M17–M22 + selected flags |
| Weekly trends / charts | Excluded | Excluded |
| Historical comparison | Excluded | Excluded |
| Best use | Daily situational scan | Daily morning exception review |

**Approved positioning:** M16 and M23 are **different use cases**. Do not replace `/dashboard`. The word **Alert** on M23 communicates purpose immediately; M16 title unchanged.

### 15.1.1 Approved user journey — first screen management sees

```text
Login
  ↓
Home / Landing  →  M16 Executive Dashboard  (/dashboard)
                       │
                       │  [Open Alert Center]
                       ↓
                   M23 Alert Center  (/alerts)
                       ↓
                   Domain Dashboard  (owning milestone)
                       ↓
                   Report  (evidence)
```

| Step | User question | Destination |
| ---- | ------------- | ----------- |
| 1 | What is happening? | M16 Executive Dashboard |
| 2 | What needs attention? | M23 Alert Center |
| 3 | Why? | Domain dashboard (M11–M22) |
| 4 | Show me the evidence | Report |

M16 gains a prominent **Open Alert Center** navigation affordance; sidebar includes **Alert Center** → `/alerts`.

### 15.2 Milestone roles in the alert ecosystem

```text
                    ┌─────────────────────────────────────┐
                    │  M23 Alert Center (AGGREGATOR)       │
                    │  Consumes signals · Does not define  │
                    └─────────────────┬───────────────────┘
                                      │ reads
        ┌─────────────────────────────┼─────────────────────────────┐
        │                             │                             │
        ▼                             ▼                             ▼
┌───────────────┐           ┌─────────────────┐           ┌─────────────────┐
│ M16 Executive │           │ Domain dashboards│           │ Platform health │
│ (headlines)   │           │ (signal owners)  │           │ (freshness)     │
└───────┬───────┘           └────────┬─────────┘           └─────────────────┘
        │ reads                        │ produce
        │                              │
        │         ┌────────────────────┼────────────────────┐
        │         │                    │                    │
        │    M11 Sales            M14 Piutang           M15 Inventory
        │    (KPI source)         (KPI source)          (composition)
        │         │                    │                    │
        │    M17 Customer ── M18 Salesman ── M19 Inv Risk ── M20 Collection
        │         │                    │                    │
        │         └──────── M21 Purchasing ── M22 Location ─┘
        │                    (cross-domain snapshots)
        ▼
   Reports (live validation) → Desktop (operational action)
```

### 15.3 Producer / consumer summary table

| Milestone | Role in M23 ecosystem | Owns signals? | Consumed by M23? |
| --------- | --------------------- | ------------- | ---------------- |
| **M16** Executive | Headline + Top 5 + freshness | Presentation only | Yes — flags & exposures |
| **M11** Sales | KPI source for achievement | Partial (company target) | Optional headline |
| **M14** Piutang | KPI source for exposure | Partial (aging rules) | Via M16/M20 |
| **M15** Inventory | Composition denominators | No attention list | Context links only |
| **M17** Customer | **Producer** | Yes | Yes — attention rows |
| **M18** Salesman | **Producer** | Yes | Yes — attention rows |
| **M19** Inventory Risk | **Producer** | Yes | Yes — **summary only** in M23 (counts + link); not item rows |
| **M20** Collection | **Producer** | Yes | Yes — attention rows |
| **M21** Purchasing Mgmt | **Producer** | Yes | Yes — attention rows |
| **M22** Location | **Producer** | Yes (warehouse); wilayah collection → M20 | Yes — warehouse rows |
| **M23** Alert Center | **Aggregator / consumer** | **No** | N/A |
| **M25** SFE (future) | Future producer (field activity) | Future | Future |

---

## 16. Special Investigation — Alert Registry

### 16.1 Current state

BTR already maintains a **de facto alert catalog** through:

- `SignalKey` string constants in each `Dashboard*Aggregator`  
- Parallel `SignalLabel` display strings  
- Snapshot persistence in `BTRPD_*Attention` tables  
- Sort/priority maps per aggregator  

There is **no central registry document or table** mapping SignalKey → owning milestone → category → severity → drill-down route.

### 16.2 Would a conceptual Alert Registry improve consistency?

**Yes — recommended as a business/architecture concept**, not necessarily a database table in V1.

| Benefit | Explanation |
| ------- | ----------- |
| **Single vocabulary** | `WilayahHotspot` defined once — M20 producer, M23 consumer |
| **Deduplication rules** | Registry documents canonical owner when keys overlap |
| **Onboarding** | New milestones (M25) register signals without ad hoc M23 logic |
| **Executive promotion tracking** | Phase 2 metrics flagged in registry |
| **PO review** | Product Owner approves new SignalKey entries before implementation |

### 16.3 Illustrative registry entries (conceptual)

| AlertTypeId | Category | Owner | EntityGrain | Severity character | Drill-down dashboard |
| ----------- | -------- | ----- | ----------- | ------------------ | -------------------- |
| `SalesAchievementCritical` | Sales | M16/M11 | Company | Critical band | `/dashboard/sales` |
| `BelowTarget` | Sales | M18 | Salesman | Warning/Critical band | `/dashboard/salesmen` |
| `ChronicOverdue` | Collection | M20 | Customer | Exception | `/dashboard/collection` |
| `DeadStock` | Inventory | M19 | Item | Exception | `/dashboard/inventory-risk` |
| `QualifiedBacklog` | Purchasing | M21 | Principal | Operational | `/dashboard/purchasing` |
| `WilayahHotspot` | Collection | M20 | Wilayah | Exception | `/dashboard/collection` |
| `WarehouseInactiveWithStock` | Location | M22 | Warehouse | Operational | `/dashboard/locations` |
| `SnapshotDegraded` | Platform | M16 | System | Critical | Health / admin refresh |

### 16.4 Registry scope boundary

The registry catalogs **alert types** — not every alert instance. Instance rows remain in domain snapshot tables. M23 reads instances filtered/grouped by registry metadata.

**Approved artifact:** [`docs/features/btr-portal/ALERT-REGISTRY.md`](../../features/btr-portal/ALERT-REGISTRY.md) — permanent catalog of SignalKey types, owners, M23 inclusion rules, and drill-down routes. **Initial version created 2026-06-09** per PO decision Q23.

**Approval authority:** **Product Owner only** for new alert types — prevents ad hoc signal invention by analysts or architects.

---

## 17. Final Product Decisions (Authoritative)

**Status:** All open questions resolved by Product Owner — 2026-06-09.  
**Audience:** Architect — use this section as scope input. Do not re-decide business rules listed here.

### 17.1 Product positioning and audience

| # | Decision |
| - | -------- |
| Q1 | **New route:** `/alerts`. **Do not replace** `/dashboard`. M16 = Executive Summary; M23 = Exception Management — different use cases. |
| Q2 | **Page title:** **Alert Center**. M16 title unchanged (Management Attention Center). |
| Q3 | **All authenticated users** — same visibility as M16–M22. No role-based routing in M23. |
| Q4 | **Daily morning review** cadence. **No real-time requirement.** |

**Landing page:** `/dashboard` (M16) remains **first screen after login**. M16 includes **Open Alert Center** → `/alerts`.

### 17.2 Alert scope and volume

| # | Decision |
| - | -------- |
| Q5 | **Cap per category:** Top **20** alerts per category (sorted by producer priority within category). |
| Q6 | **M19 summary only** — e.g. "Dead Stock: 142 Items · Slow Moving: 386 Items" with **View Inventory Risk →**. **Do not** list individual SKU rows in Alert Center. **Most important M23 volume decision.** |
| Q7 | **No** M16 Top 5 exposure lists in Alert Center. **Explicit attention snapshot rows only** for entity alerts. |
| Q8 | **Separate sections:** **Alerts** (exceptions) vs **Concentrations** (informational). Do not mix concentration metrics into default exception feed. |

### 17.3 Deduplication policy

| # | Decision |
| - | -------- |
| Q9 | **M20 wins** over M17 for customer overdue overlap. |
| Q10 | **M20 wins** over M18 `HighOverdueExposure` — use `HighOverdueWorkload`; M20 owns collection management. |
| Q11 | **Suppress** M17 `Dormant` when M20 `LegacyDebt` applies — show **LegacyDebt** only. |

### 17.4 Severity and prioritization

| # | Decision |
| - | -------- |
| Q12 | **Producer priority only** within each feed/category. **No cross-domain ranking formulas** (e.g. Collection > Inventory). |
| Q13 | **No** Critical / Warning / Info engine. Binary **Requires Attention** except **Sales Achievement** keeps existing Healthy / Warning / Critical bands. |
| Q14 | **Platform alerts pinned** always at top — stale/degraded data invalidates all other alerts. |

### 17.5 Executive and Phase 2 integration

| # | Decision |
| - | -------- |
| Q15 | **Yes** — M23 may surface Phase 2 executive candidates (M19 dead stock / at-risk %, M20 recovery metrics, M22 warehouse concentration) **before** M16 promotes them. |
| Q16 | **No** — leave M16 **unchanged**. M16 and M23 serve different purposes. |

### 17.6 Categories and grouping

| # | Decision |
| - | -------- |
| Q17 | **Seven categories:** Sales · Customer · Collection · Inventory · Purchasing · Location · Platform. **Do not merge** Customer and Collection. |
| Q18 | **No** Operational vs Managerial tabs in V1 — keep simple. Deferred to future version. |
| Q19 | **Sales category includes both** company Achievement alert and salesman Below Target (and other M18 sales performance signals in Sales category). |

### 17.7 Navigation and workflow

| # | Decision |
| - | -------- |
| Q20 | **Dashboard first:** Alert → Domain Dashboard → Report (existing portal philosophy). |
| Q21 | **No** Desktop menu links from Alert Center. Portal remains portal; Desktop remains Desktop. |
| Q22 | **CompoundDependency:** single link to **Purchasing Dashboard** — that dashboard provides further cross-links. |

### 17.8 Alert registry

| # | Decision |
| - | -------- |
| Q23 | **Approve** Alert Registry. Permanent artifact: [`ALERT-REGISTRY.md`](../../features/btr-portal/ALERT-REGISTRY.md). |
| Q24 | **Product Owner only** approves new alert types. |

### 17.9 Lifecycle and future scope

| # | Decision |
| - | -------- |
| Q25 | **Alert acknowledgment:** Out of scope. **Read-only** feed. |
| Q26 | **Alert history:** Out of scope. |
| Q27 | **Event-driven refresh after payment posting:** Out of scope. Current snapshot model sufficient. |
| Q28 | **M25 and future signals:** Require **explicit PO approval** per signal — do not auto-include via registry alone. |

### 17.10 Exclusions and scope control

| # | Decision |
| - | -------- |
| Q29 | **Approve all exclusions:** Faktur Kembali aggregate, Retur analytics, Effective Call, DSO, Tagihan pipeline KPIs, Sales Omzet Health, Stok Balance Health — **out of M23 V1**. |
| Q30 | **No new SignalKey types in M23.** Aggregate signals produced by M17–M22 only. **Most important scope-control decision** — M23 must not invent signals or violate ownership model. |

### 17.11 Approved M23 content summary (Architect input)

| Area | V1 behavior |
| ---- | ----------- |
| Route / title | `/alerts` · **Alert Center** |
| Landing | M16 `/dashboard` unchanged; **Open Alert Center** on M16 |
| Entity alert feed | Top 20 per category; producer sort; dedup per Section 12 |
| Inventory (M19) | Summary KPIs only + link to `/dashboard/inventory-risk` |
| Concentrations | Separate section — informational signals from producers (e.g. concentration % rows) |
| Platform | Pinned stale/degraded alerts |
| Phase 2 KPIs | Allowed on M23 before M16 promotion |
| Registry | `ALERT-REGISTRY.md` maintained under PO approval |
| Out of scope | Acknowledgment, history, real-time refresh, new SignalKeys, Desktop links |

---

## 18. Dashboard Layout Proposal

**Product Owner decision:** **Proposal A — Priority Feed** at route **`/alerts`**. M16 remains landing page with **Open Alert Center** entry. Fixed section order below.

### 18.1 Approved page structure

```text
+====================================================================+
|  ALERT CENTER  (/alerts)                         [Refresh]         |
|  What requires attention right now across the business?            |
|  Last Refreshed: YYYY-MM-DD HH:mm    [Platform: OK | Stale | Degraded]
+====================================================================+
|  PLATFORM ALERTS (always pinned top)                               |
|  [Snapshot Stale] [Snapshot Degraded] [Domain Unavailable]        |
+====================================================================+
|  1. ALERT SUMMARY BY CATEGORY (Top 20 cap each)                    |
|  Sales(3) | Customer(12) | Collection(28) | Inventory | ...        |
+====================================================================+
|  2. ALERTS (exceptions — entity rows, deduplicated)                |
|  Category | Entity | Signal | Value | → Dashboard                  |
|  Collection | CV Maju | Chronic Overdue | Rp 450M | Collection →   |
|  Purchasing | PT ABC | Qualified Backlog | 5 inv | Purchasing →   |
|  Sales | Budi Santoso | Below Target | 62% | Salesmen →           |
|  Location | Gudang Timur | Inactive With Stock | Rp 80M | Loc →   |
|  (max 20 rows per category; producer priority sort)                |
+====================================================================+
|  3. INVENTORY RISK SUMMARY (M19 — not SKU rows)                    |
|  Dead Stock: 142 Items · Rp xxx  |  Slow Moving: 386 Items · Rp xxx |
|  Never Sold: 12 Items · At-Risk: 18%                              |
|  [View Inventory Risk →]  (/dashboard/inventory-risk)              |
+====================================================================+
|  4. CONCENTRATIONS (informational — separate from Alerts)         |
|  Top Customer Piutang 34% | Top Principal Spend 41% | ...          |
|  (Phase 2 KPIs allowed here before M16 promotion)                  |
+====================================================================+
|  5. NAVIGATION TO DOMAIN DASHBOARDS                                |
|  Executive | Sales | Piutang | Customers | Salesmen | Collection  |
|  Inventory | Inv Risk | Purchasing | Locations                      |
+====================================================================+
```

**Excluded from Alert Center:** M16 Top 5 exposure lists; M19 item-level attention rows; Desktop links; alert acknowledgment.

### 18.2 Proposals not selected (reference only)

| Proposal | Status |
| -------- | ------ |
| B — Category Tabs First | Not selected — Proposal A approved with category summary counts |
| C — Executive Plus Feed on `/dashboard` | **Rejected** — separate `/alerts` route; M16 unchanged |

---

## 19. Alert Fatigue Investigation

### 19.1 Volume drivers (order of magnitude risk)

| Source | Volume risk | Cause |
| ------ | ----------- | ----- |
| M19 item attention list | **High** | One row per item × signal (Dead/Slow/Never) |
| M17 customer attention list | Medium | One row per customer × signal |
| M20 collection customer rows | Medium | Overlap with M17 |
| M18 salesman rows | Low–medium | One row per rep × signal |
| M21 principal rows | Low | Principal × signal, deduped in aggregator |
| M22 warehouse rows | Low | Small warehouse universe |

### 19.2 Approved fatigue controls (PO decisions)

| Control | Decision |
| ------- | -------- |
| **Top 20 per category** | Hard cap on entity alert rows |
| **M19 summary only** | No SKU dump — link to Inventory Risk |
| **Dedup M17/M20/M18 overlaps** | Section 12 / Section 17.3 |
| **Alerts vs Concentrations** | Separate sections |
| **Platform pinned top** | Stale/degraded always first |
| **Producer priority sort** | Within category only — no cross-domain formula |
| **Daily morning review** | Snapshot model — show Last Refreshed |

### 19.3 What not to do

| Anti-pattern | Why |
| ------------ | --- |
| Show every M19 item row uncapped on landing | Overwhelms management |
| Duplicate M17 and M20 customer overdue rows | Same customer appears twice |
| Treat all BELUM invoices as critical | M21 qualified rule exists precisely to avoid this |
| Add generic Critical/Warning without PO thresholds | Rejected in M16/M17 |

---

## Appendix A — Snapshot Table Index for Alert Sources

| Table | Producer |
| ----- | -------- |
| `BTRPD_CustomerAttention` | M17 |
| `BTRPD_SalesmanAttention` | M18 |
| `BTRPD_InventoryRiskAttention` | M19 |
| `BTRPD_CollectionAttention` | M20 |
| `BTRPD_PurchasingManagementAttention` | M21 |
| `BTRPD_LocationAttention` | M22 |
| `BTRPD_SalesKpi` (+ executive compose) | M11/M16 |
| `BTRPD_PiutangKpi` (+ aging, top customer) | M14/M16 |
| `BTRPD_PurchasingManagementKpi` | M21/M16 purchasing card |
| `BTRPD_RefreshLog` | Platform |

---

## Appendix B — Handoff to Architect

**Product scope is approved** (Section 17). Proceed with implementation planning.

The Architect should:

1. Design M23 at **`/alerts`** as a **consumer/aggregator** — **no new SignalKey types** (Q30).  
2. Keep **`/dashboard`** (M16) as landing page; add **Open Alert Center** navigation on M16.  
3. Implement **deduplication** per Section 12 / Section 17.3 (M20 wins overdue; LegacyDebt suppresses Dormant; M20 wins salesman workload).  
4. **Cap** entity alerts at **Top 20 per category**; **M19 summary only** — no item row feed.  
5. Separate **Alerts** and **Concentrations** sections (Q8).  
6. **Pin platform** stale/degraded alerts at top (Q14).  
7. Navigation: Alert → **domain dashboard** → report (Q20); CompoundDependency → Purchasing only (Q22).  
8. Allow **Phase 2 KPIs** on M23 before M16 promotion (Q15); **do not change M16** (Q16).  
9. Create and maintain **`docs/features/btr-portal/ALERT-REGISTRY.md`** per Section 16 — PO approves new entries.  
10. Preserve **snapshot refresh model** — no real-time, acknowledgment, or history (Q25–Q27).  

**Explicitly out of scope for this document:** API contracts, schema DDL, UI component specs, and delivery estimates.

---

## Appendix C — Related Artifacts

| Artifact | Purpose |
| -------- | ------- |
| [`ALERT-REGISTRY.md`](../../features/btr-portal/ALERT-REGISTRY.md) | Authoritative SignalKey catalog — PO-approved alert types and M23 inclusion rules |
| Section 17 | Authoritative Product Owner decisions |
| Section 3.2 | Full signal catalog discovery reference |

---

*End of analysis — M23 Alert Center*
