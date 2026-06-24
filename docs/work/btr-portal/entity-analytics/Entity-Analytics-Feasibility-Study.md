# Entity Analytics — Feasibility Study

**Status:** Revision 2 — Product analysis complete, ready for Architect handoff  
**Scope:** Business feasibility, product architecture, generic KPI framework, metadata model, snapshot evolution, cross-entity relationships, navigation model, complexity, risks, and strategic recommendations.  
**Explicitly excluded:** UI design, database schema, API contracts, implementation tasks.  
**Date:** 2026-06-24 (Rev 2)  
**Analyst role:** `docs/agents/analyst-agent.md`

**Reference documents:**

- `docs/foundation/PRODUCT.md`, `DOMAIN.md`, `LANDSCAPE.md`, `WORKFLOW.md`
- `docs/features/btr-portal/btr-portal-domain.md` — portal maturity model and business concepts
- `docs/features/btr-portal/btr-portal-kpi-catalog.md` — authoritative KPI SSOT (~214 indexed KPIs, M16–M31)
- `docs/features/materialized-dashboard/materialized-dashboard-domain.md` — snapshot semantics and refresh model
- `docs/work/btr-portal/M17-Customer-Analytics-Analysis.md` — precedent for entity-centric lens
- `docs/work/btr-portal/M18 Salesman Performance - Analysis.md` — salesman entity lens

---

## Executive Summary

BTR Portal has evolved from domain dashboards (aggregate KPIs + Top-N rankings) toward **cross-domain entity lenses** — Customer Analytics (M17), Salesman Performance (M18), Customer Risk Forecast (M29), and Customer Portfolio (M31) already answer *which entities require attention* at portfolio level. Management still lacks a unified **Performance Profile** for arbitrary entities: select Customer A vs Customer B, Item A vs Item B, compare historical trajectory, and see normalized multi-dimensional health.

**Verdict:** Entity Analytics is **business-feasible and strategically valuable**. It complements — does not replace — existing dashboards. Technical feasibility is **high for Customer and Salesman**, **medium for Supplier/Principal**, and **medium-to-high for Item** depending on historical depth and comparison scope.

**Critical dependency:** Entity Analytics requires **historical KPI retention** beyond today's `SnapshotKey = 'CURRENT'` model. The only existing precedent is `BTRPD_SalesmanRepHistory` (monthly per-rep upsert). Without period-keyed entity snapshots, Performance Profiles cannot deliver trends, period comparison, or ranking history.

**Cloud implication:** Periodic KPI snapshots are **sufficient and necessary** for a lightweight cloud portal. Full transaction replication is neither required nor desirable.

**Recommendation:** Adopt Entity Analytics as a **new major milestone family (M32+)** with phased delivery. MVP = **Customer Performance Profile + multi-entity comparison**, reusing the richest existing KPI and snapshot foundation. **Cloud snapshot synchronization** should be a **separate platform milestone** — not bundled inside Entity Analytics — because it benefits all portal features.

**Architectural foundation (Rev 2):** Eight design principles, a generic eight-category KPI taxonomy, KPI metadata and lifecycle rules, reusable Performance Profile information architecture, and a snapshot model designed for future entity types (Warehouse, Wilayah, Category, Brand, Collector, etc.) without per-entity reinvention.

---

## 1. Entity Analytics Design Principles

These principles are the **architectural foundation** of the Entity Analytics platform. Every product and technical decision should be evaluated against them. They exist to reduce duplication, improve extensibility, and ensure new entity types inherit the same analytics capabilities as Customer, Salesman, Item, and Supplier.

### 1.1 Principle catalog

| Principle | Definition | Why it exists | Product evolution influence |
| --------- | ---------- | ------------- | --------------------------- |
| **Entity First** | Analytics are organized around a selectable business entity (Customer, Item, …), not around a dashboard page | Management thinks in accounts, SKUs, reps, and principals — not in "Piutang dashboard vs Sales dashboard" | New entity types plug into the same profile shell; domain dashboards remain secondary entry points |
| **Comparison by Default** | Every profile supports comparing the entity to peers and to prior periods without extra setup | Ranking dashboards only show leaders; management decisions require relative context | Comparison slots and peer groups are first-class, not optional add-ons |
| **Historical by Default** | Profiles show trajectory, not only current state | Current-month KPIs alone cannot answer sustainability, seasonality, or intervention impact | Monthly history retention is mandatory platform capability, not per-entity feature |
| **Evidence First** | Every KPI in a profile must trace to an existing portal definition and drill to report evidence | BTR Portal maturity model ends at reports; profiles must not become opaque scores | Profile KPIs map to `btr-portal-kpi-catalog.md` IDs; Evidence section links to reports |
| **Snapshot Driven** | Profile data is read from materialized snapshots, never from live transaction scans at request time | Materialized dashboard initiative established performance and cloud constraints | Workers compute; portal reads — consistent with `materialized-dashboard-domain.md` |
| **Cross-Domain Analytics** | A single profile may compose KPIs from Sales, Finance, Inventory, and Purchasing when the entity spans domains | Customer health is omzet + piutang; Supplier health is purchase + inventory + sales-out | Aggregators cross-read source DALs once at refresh — same pattern as M17/M18/M21 |
| **Reusable Analytics Engine** | Entity-specific logic is limited to KPI selection, peer groups, and relationships — not profile structure, history, comparison, or radar mechanics | Four initial entity types will become ten+ (Warehouse, Wilayah, Category, Brand, Collector) | One platform engine; entity types declare KPI packs and relationship packs |
| **Consistent KPI Semantics** | Period rules, void exclusion, customer key resolution, and achievement bands are inherited from portal SSOT — never redefined in profiles | Metric disagreement destroys management trust | Profile KPIs reference catalog entries; semantic changes propagate from catalog |

### 1.2 Supporting principles (extensibility)

| Principle | Definition | Why it exists |
| --------- | ---------- | ------------- |
| **Metadata over Hardcode** | KPIs are described by metadata (identifier, category, period, direction) rather than embedded only in aggregator code | Adding a KPI to Customer profile should not require cloning Item profile patterns |
| **Relationship-Aware** | Entities are nodes in a business graph; profiles surface top related entities | Isolated profiles cannot explain *why* a customer grew or a principal is risky |
| **Entity-Type Extensibility** | New entity types register a KPI pack, relationship pack, and peer-group rule — they do not fork the platform | M22 Location analytics already implies Warehouse/Wilayah profiles later |
| **Versioned Semantics** | KPI meaning may evolve; historical periods retain the definition that applied when computed | Long-lived distributors need multi-year trends without silent metric breaks |

### 1.3 Anti-patterns (explicitly rejected)

| Anti-pattern | Why rejected |
| ------------ | ------------ |
| Per-entity-type profile page with unique section order | Breaks learnability; violates Reusable Analytics Engine |
| Live transaction queries per profile load | Violates Snapshot Driven; fails cloud and performance constraints |
| Parallel KPI definitions for profiles vs dashboards | Violates Consistent KPI Semantics and Evidence First |
| CRM-style notes, tasks, or master data edit in profiles | Out of BTR Portal product scope |
| Cross-entity-type comparison (Customer vs Item) | Not semantically meaningful |

### 1.4 How principles interact

```text
Entity First + Reusable Analytics Engine
  → One profile information architecture, many entity KPI packs

Comparison by Default + Historical by Default
  → CURRENT layer + monthly history + ranking history

Evidence First + Consistent KPI Semantics
  → KPI catalog SSOT + metadata registry + report drill-down

Snapshot Driven + Cross-Domain Analytics
  → Worker-side composition from source DALs, not snapshot-of-snapshots chains

Relationship-Aware + Entity First
  → Profile includes Related Entities without replacing the primary entity lens
```

---

## 2. Business Feasibility

### 2.1 Management problem

Current portal analytics optimize for **morning executive scan** and **exception discovery**:

```text
What is happening?     → Executive Dashboard
What needs attention?  → Alert Center + domain attention lists
Why?                   → Domain dashboard context
Show evidence          → Report (tabular)
Take action            → BTR Desktop
```

This workflow works when the entity is already on a Top-10 list or attention list. It breaks when management asks:

- *"How is Customer X doing compared to last quarter?"*
- *"Customer A vs Customer B — who is healthier across sales and collection?"*
- *"Salesman A improved — is that sustainable or a one-month spike?"*
- *"Item Y is not in Top 10 dead stock — should we still worry?"*
- *"Principal Z dominates purchasing — how does that compare to sales velocity of their SKUs?"*

These questions require **entity selection**, **historical context**, and **side-by-side comparison** — capabilities absent from ranking-only surfaces.

### 2.2 Management use cases

| Use case | Primary users | Entity types | Decision supported |
| -------- | ------------- | ------------ | ------------------ |
| Account review meeting | Owner, GM, Sales Manager | Customer | Retain / grow / restrict credit / reassign salesman |
| Sales coaching | Sales Manager | Salesman | Target adjustment, principal focus, collection accountability |
| SKU lifecycle review | Inventory Admin, Purchasing | Item | Replenish, promote, clearance, delist |
| Supplier negotiation | Purchasing Admin, Owner | Supplier/Principal | Dependency reduction, posting discipline, intake planning |
| Credit committee | Finance Admin | Customer | Plafond review, suspension, collection escalation |
| Competitive benchmarking | Management | Any | Peer comparison within Wilayah, Klasifikasi, category, or principal |
| Post-intervention validation | Management | Any | Did action taken last month improve entity KPIs? |

### 2.3 Decision-support benefits

| Benefit | Description |
| ------- | ----------- |
| **Depth beyond Top-N** | Evaluate mid-tier entities that matter locally but never appear in company Top 10 |
| **Cross-metric coherence** | Single Performance Profile unifies sales, receivable, activity, and risk signals already scattered across dashboards |
| **Evidence-backed comparison** | Side-by-side entity view reduces anecdotal account reviews |
| **Temporal reasoning** | Trends and period comparison support coaching and intervention follow-up |
| **Normalized health view** | Radar chart (if adopted) gives at-a-glance balance across dimensions — avoids over-focusing on one dominant metric (e.g., omzet alone) |
| **Ranking context** | Ranking history answers *"Was this customer always #1 or did they recently climb?"* |

### 2.4 Typical investigation workflow (proposed)

Entity Analytics extends — not replaces — the established portal journey. Full navigation model: **§10**.

```text
1. DISCOVER (unchanged)
   Executive / Alert Center / Domain dashboard attention list
        ↓
2. SELECT ENTITY (new)
   Search or pick entity → open Performance Profile (§9)
        ↓
3. UNDERSTAND PROFILE (new)
   Overview → KPI summary → comparison → trend → attention history
        ↓
4. EXPLORE RELATIONSHIPS (new)
   Related entities → navigate business graph (§8)
        ↓
5. COMPARE (new, optional)
   Add 2–4 peer entities → multi-entity comparison
        ↓
6. VALIDATE (unchanged)
   Evidence section → Report with entity pre-filter
        ↓
7. ACT (unchanged)
   BTR Desktop operational resolution
```

### 2.5 Complementarity with existing dashboards

| Existing surface | Role after Entity Analytics |
| ---------------- | --------------------------- |
| Executive Dashboard | Company-wide attention scan — entry point, not deep dive |
| Domain dashboards (Sales, Piutang, Inventory, Purchasing) | Segment context and company denominators |
| Customer Analytics (CU01) | Portfolio-level *which customers need attention* — links into Customer Profile |
| Salesman Performance (SF01) | Team-level *which reps need attention* — links into Salesman Profile |
| Customer Portfolio (CU04) | Optimization actions across all customers — links into Customer Profile for detail |
| Inventory Risk (IN02) | Item-level attention list — links into Item Profile |
| Purchasing Management (PU01) | Principal-level attention — links into Supplier Profile |
| Reports | Terminal evidence layer — unchanged |
| Alert Center | Cross-domain exception feed — entity row links to Profile |

**Principle:** Domain dashboards remain the **wide lens**; Entity Analytics is the **deep lens** for chosen entities.

---

## 3. Existing KPI Inventory

Source: `btr-portal-kpi-catalog.md` (~214 indexed KPIs across EX, SA, CU, FI, SF, IN, PU, OP prefixes). Below: KPIs **grouped by primary entity**, with reuse assessment for Entity Analytics.

### 3.1 Customer entity

**Already implemented (direct reuse or profile seed)**

| KPI area | Representative KPIs | Portal location | Reuse |
| -------- | --------------------- | --------------- | ----- |
| Sales activity | Active count, MTD omzet, Top 10 omzet, Faktur volume (derivable) | CU01, SA01, SA03 | **Direct** — CU-KPI-005, CU-KPI-009 |
| Receivable | Open balance, overdue flag, aging buckets, Top 10 piutang | CU01, FI01, FI04 | **Direct** — CU-KPI-010, FI-KPI-012 |
| Concentration | Top omzet %, Top piutang % | CU01, EX01 | **Direct** — CU-KPI-003, CU-KPI-004 |
| Attention signals | Overdue, Dormant (90d), Plafond breach, Suspended+Sales | CU01 attention list | **Direct** — CU-KPI-001–008 |
| Risk forecast | Risk category, signal mix, payment delay | CU02 | **Partial** — top 20 only (CU-KPI-020–036) |
| Portfolio | Lifecycle, tier, portfolio action, priority score | CU04, CU05 report | **Strong** — CU-KPI-060–071; `BTRPD_CustomerPortfolioCustomer` is richest per-customer row |
| Collection | Recovery context, due within 7 days | CU03 | **Partial** — optimization lens, not full profile |

**Gaps for Customer Performance Profile**

| Gap | Business meaning | Data availability |
| --- | ---------------- | ----------------- |
| Monthly omzet trend (12+ months) | Purchase momentum | `IFakturViewDal` — **not retained** in snapshots |
| Monthly open balance trend | Receivable trajectory | Piutang is point-in-time only |
| Period-over-period comparison | MoM / YoY | Not materialized |
| Ranking history | Was customer Top 10 in prior months? | Not stored |
| Retur rate / return value | Quality / relationship | Desktop RF1 — **not in portal** |
| Collection effectiveness per customer | Payment behavior trend | M29 partial; FF4 desktop |
| Multi-customer comparison API | A vs B | **Not available** |
| Attention history timeline | When did signals first appear? | Current snapshot only |

### 3.2 Salesman entity

**Already implemented**

| KPI area | Representative KPIs | Portal location | Reuse |
| -------- | --------------------- | --------------- | ----- |
| Sales performance | MTD omzet, achievement %, below-target count | SF01, SA01 | **Direct** — SF-KPI-001–009 |
| Receivable exposure | Top 10 piutang, high overdue/piutang exposure | SF01 | **Direct** — SF-KPI-003, SF-KPI-004, SF-KPI-010 |
| Portfolio quality | Dormant portfolio, customer concentration | SF01 attention | **Direct** — SF-KPI-005 |
| Principal breakdown | Principal achievement table | SF01 | **Direct** — SF-KPI-011 |
| Field activity | Visit execution %, effective call rate | SF02 | **Partial** — day/scoped, not profile trend |
| **Historical trend** | Monthly achievement, omzet, band | SF01 trend chart | **Direct** — `BTRPD_SalesmanRepHistory` (**only entity with monthly history today**) |

**Gaps for Salesman Performance Profile**

| Gap | Notes |
| --- | ----- |
| Ranking history | Top 10 position by month not stored |
| Multi-salesman comparison | Not available as first-class feature |
| Attention history | Signals are current-state only |
| Radar dimensions | Not defined |
| Field activity trend | SF02 is operational-day lens |

### 3.3 Item entity

**Already implemented (mostly aggregate and Top-N — not per-item profile)**

| KPI area | Representative KPIs | Portal location | Reuse |
| -------- | --------------------- | --------------- | ----- |
| Inventory position | Qty, HPP value (report row) | IN05 Inventory Report | **Direct** at row level |
| Movement class | Active / Slow / Dead / Never Sold | IN02, IN01 context | **Direct** — IN-KPI-005–009 |
| At-risk attention | Item × signal list | IN02 attention | **Direct** — `BTRPD_InventoryRiskAttention` |
| Rankings | Top 10 dead, Top 10 slow | IN02 | **Partial** — outside Top 10 invisible |
| Forecast / optimization | Days of supply, reorder qty, clearance | IN03, IN04 | **Partial** — per-item in snapshot but no unified Profile |
| Sales velocity | Last Faktur date, consumption | IN03, aggregator | **Derivable** — `IBrgLastFakturDal`, `IBrgConsumptionDal` |

**Gaps for Item Performance Profile**

| Gap | Notes |
| --- | ----- |
| Unified per-item KPI summary | Metrics spread across IN02, IN03, IN04, IN05 |
| Monthly sales qty / omzet trend | Not retained per item |
| Customer reach per item | Distinct customers buying SKU — derivable, not surfaced |
| Supplier attribution profile | Cross-link to principal — partial via item master |
| Item ranking history | Not stored |
| Multi-item comparison | Not available |

### 3.4 Supplier / Principal entity

**Already implemented**

| KPI area | Representative KPIs | Portal location | Reuse |
| -------- | --------------------- | --------------- | ----- |
| Purchase spend | MTD grand total, Top 10 principal | PU01, PU02 | **Direct** — PU-KPI-001, PU-KPI-011 |
| Posting health | Posted %, qualified backlog | PU01, PU01 management | **Direct** — PU-KPI-003–005 |
| Concentration | Top 1 %, Top 3 %, compound dependency | PU01, EX01 | **Direct** — PU-KPI-006–008 |
| Inventory linkage | Inventory value by supplier, at-risk exposure | IN01, IN02, PU01 | **Partial** — dimensional, not principal Profile |
| Sales linkage | Principal achievement per salesman | SF01 | **Indirect** — salesman×principal, not principal-centric |
| Attention signals | 8 principal signals (backlog, inactivity, compound dependency) | PU01 management | **Direct** — attention list |

**Gaps for Supplier Performance Profile**

| Gap | Notes |
| --- | ----- |
| Principal-centric unified view | Purchase + inventory + sales velocity of principal's SKUs — **composed but not unified** |
| Monthly purchase trend per principal | Company weekly trend only |
| Sales-out velocity for principal's catalog | Requires FakturItem × Supplier join — derivable |
| Ranking history | Not stored |
| Multi-principal comparison | Not available |

### 3.5 Reuse summary matrix

| Capability | Customer | Salesman | Item | Supplier |
| ---------- | -------- | -------- | ---- | -------- |
| Current-state KPI summary | **Strong** | **Strong** | **Moderate** | **Moderate** |
| Attention signals | **Strong** | **Strong** | **Strong** | **Strong** |
| Top-N ranking (current) | **Yes** | **Yes** | **Yes** | **Yes** |
| Per-entity row in snapshot | **Yes** (M31) | **Partial** (Top 10 + attention) | **Yes** (attention + forecast) | **Partial** (attention + Top 10) |
| Historical monthly trend | **No** | **Yes** (rep history) | **No** | **No** |
| Period comparison | **No** | **Partial** | **No** | **No** |
| Multi-entity comparison | **No** | **No** | **No** | **No** |
| Ranking history | **No** | **No** | **No** | **No** |
| Radar-ready dimensions | **Moderate** | **Strong** | **Moderate** | **Moderate** |

---

## 4. Generic Entity KPI Framework

This section defines the **common KPI language** for the entire Entity Analytics platform. Individual KPIs (CU-KPI-009, SF-KPI-008, etc.) are instances of generic categories. The framework is **entity-agnostic at the category level** and **entity-specific at the KPI instance level** — enabling Warehouse, Wilayah, Category, Brand, and Collector profiles later without inventing a new taxonomy.

### 4.1 Design goals

1. **One vocabulary** across all entity types — current and future.
2. **Entity-appropriate applicability** — not every category applies to every entity.
3. **Reuse portal KPI definitions** — Entity Analytics composes existing KPIs; it does not invent parallel metrics.
4. **Period semantics preserved** — sales MTD, piutang all-time open, inventory point-in-time (per `materialized-dashboard-domain.md`).
5. **Extensibility by registration** — a new entity type declares which categories and KPI instances it exposes; the engine handles presentation, history, and comparison.

### 4.2 Category taxonomy — definitions and purpose

| Category | Business meaning | Why this category exists |
| -------- | ---------------- | ------------------------ |
| **Activity** | Whether and how recently the entity participated in operational transactions | Distinguishes genuinely active entities from dormant or legacy records — activity precedes financial interpretation |
| **Financial** | Monetary scale, exposure, or capital tied to the entity | Answers *"how big is this entity's economic footprint?"* |
| **Growth** | Direction and pace of change versus a prior period | Answers *"is this entity improving or deteriorating?"* — essential for coaching and intervention follow-up |
| **Contribution** | Entity's share of a defined company or parent total | Answers *"how dependent is the company on this entity?"* — concentration and portfolio risk |
| **Portfolio** | Breadth, composition, and coverage of entities owned or influenced by this entity | Answers *"how diversified is this entity's book?"* — applies to aggregating entities (Salesman, Supplier, Warehouse) |
| **Quality** | Health of process execution, engagement depth, or fulfillment discipline | Answers *"is activity productive and compliant?"* — separates volume from effectiveness |
| **Risk** | Attention-worthy exposure requiring management review | Connects profiles to existing attention-signal philosophy (M16/M17) |
| **Trend** | Time-series behavior of key metrics, ranks, and signals | Answers *"what is the trajectory?"* — not a single KPI but the temporal dimension of all categories |

**Trend** is both a category (headline trend KPIs) and a **cross-cutting concern** — every category may expose trend series in the profile Trend section.

### 4.3 Category applicability matrix

| Category | Customer | Salesman | Item | Supplier | Warehouse* | Wilayah* | Category* | Brand* | Collector* |
| -------- | -------- | -------- | ---- | -------- | ---------- | -------- | --------- | ------ | ---------- |
| Activity | ● | ● | ● | ● | ● | ● | ○ | ○ | ● |
| Financial | ● | ● | ● | ● | ● | ● | ● | ○ | ● |
| Growth | ● | ● | ● | ● | ● | ● | ● | ○ | ● |
| Contribution | ● | ○ | ● | ● | ● | ● | ● | ○ | ○ |
| Portfolio | ○ | ● | ○ | ● | ● | ● | ● | ● | ● |
| Quality | ○ | ● | ● | ● | ○ | ○ | ○ | ○ | ● |
| Risk | ● | ● | ● | ● | ● | ● | ● | ○ | ● |
| Trend | ● | ● | ● | ● | ● | ● | ● | ○ | ● |

**Legend:** ● = core · ○ = secondary or not applicable · \* = future entity type (not in M32 MVP)

**Rationale highlights:**

- **Customer:** Portfolio is secondary (customer *is* the portfolio unit); contribution = share of company omzet/piutang.
- **Item:** Portfolio = customer reach / warehouse spread (secondary); quality = movement class + forecast health.
- **Supplier:** Portfolio = active SKU count, catalog sales penetration, inventory share.
- **Salesman:** Contribution to company total is less meaningful than portfolio breadth (customers, principals).
- **Warehouse / Wilayah (future):** Roll up M22 Location KPIs — concentration, sales, inventory, at-risk % per site or territory.
- **Category / Brand (future):** Aggregate item metrics — inventory value, sales velocity, at-risk % across SKU rollups.
- **Collector (future):** Collection workload entity — overdue exposure, recovery rate, customer queue breadth (extension of M20/M30).

### 4.4 KPI instances by category and entity type

Each row lists **catalog KPIs** (where they exist) or **proposed profile KPIs** (derivable from known DALs). Proposed KPIs must receive catalog IDs when implemented — they are not alternate definitions.

#### Activity

| Entity | KPI instances | Catalog / source |
| ------ | ------------- | ---------------- |
| Customer | Last purchase date; MTD Faktur count; Active (invoiced this month) | CU-KPI-005; derivable from `IFakturViewDal` |
| Salesman | Active flag; customers invoiced MTD; MTD Faktur count | SF-KPI-008 context; M18 `IsActive` |
| Item | Last Faktur date; movement class (Active/Slow/Dead/Never) | IN-KPI-005–009; `IBrgLastFakturDal` |
| Supplier | MTD purchase invoice count; posting status | PU-KPI-002, PU-KPI-003 |
| Warehouse* | MTD Faktur count billed from warehouse; stock movement events | OP-KPI-009; M22 |
| Wilayah* | Customers invoiced MTD; active salesman count | OP-KPI-011; M22 |
| Category* | Items sold MTD; distinct customers buying category | IN-KPI-003 rollup |
| Collector* | Customers with overdue in portfolio; collection visits MTD | M30 extension |

#### Financial

| Entity | KPI instances | Catalog / source |
| ------ | ------------- | ---------------- |
| Customer | MTD omzet; open balance; >90d exposure amount | CU-KPI-009, CU-KPI-010, CU-KPI-002 |
| Salesman | MTD omzet; achievement %; open balance (context) | SF-KPI-008, SF-KPI-009; RepHistory |
| Item | On-hand qty; inventory value (HPP×Qty) | IN-KPI-001 row grain; IN05 |
| Supplier | MTD purchase spend; inventory value attributed | PU-KPI-001; IN-KPI-004 |
| Warehouse* | Inventory value; MTD sales omzet; MTD purchase intake | OP-KPI-007–010 |
| Wilayah* | MTD sales omzet; overdue exposure | OP-KPI-011; Collection |
| Category* | Total inventory value; MTD sales omzet | IN-KPI-003 rollup |
| Collector* | Overdue exposure managed; cash collected MTD | COL-KPI extension |

#### Growth

| Entity | KPI instances | Catalog / source |
| ------ | ------------- | ---------------- |
| Customer | MoM omzet Δ%; YoY same-month omzet Δ% | **New** — monthly history |
| Salesman | 3-month achievement trend; MoM omzet Δ% | RepHistory; **new** for full customer set |
| Item | 3-month consumption trend; MoM sales qty Δ% | M28 `IBrgConsumptionDal` history |
| Supplier | MoM purchase Δ%; catalog sales-out Δ% | **New** — monthly history |
| Warehouse* | MoM sales Δ% vs prior month | M22 + history |
| Wilayah* | MoM customer count Δ%; MoM omzet Δ% | **New** rollup history |
| Category* | MoM category sales Δ% | **New** rollup history |
| Collector* | MoM recovery rate Δ% | M20 extension |

#### Contribution

| Entity | KPI instances | Catalog / source |
| ------ | ------------- | ---------------- |
| Customer | % of company MTD omzet; % of company open piutang | CU-KPI-003, CU-KPI-004 |
| Salesman | ○ — use portfolio instead | — |
| Item | % of company inventory value; % of category sales | IN-KPI-001 denominator |
| Supplier | % of company MTD purchase; % of inventory value | PU-KPI-006, PU-KPI-007 |
| Warehouse* | % of company inventory; % of company sales | OP-KPI-001, OP-KPI-004 |
| Wilayah* | % of company sales; % of overdue exposure | OP-KPI-005; Collection |
| Category* | % of company inventory; % of company sales | IN breakdown |
| Collector* | ○ — not primary | — |

#### Portfolio

| Entity | KPI instances | Catalog / source |
| ------ | ------------- | ---------------- |
| Customer | ○ — customer is atomic unit | — |
| Salesman | Customer count MTD; principal count with omzet; dormant portfolio count | SF-KPI-005, SF-KPI-011 |
| Item | Distinct customers buying (reach); warehouse spread | **New** derivable |
| Supplier | Active SKU count; % of catalog with sales in period | **New** derivable |
| Warehouse* | SKU count; customer billing count | M22 |
| Wilayah* | Customer count; salesman count; klasifikasi mix | M17 segmentation pattern |
| Category* | SKU count; supplier count in category | IN breakdown |
| Brand* | SKU count under brand; category spread | Master data rollup |
| Collector* | Customer queue size; overdue customer count | M30 |

#### Quality

| Entity | KPI instances | Catalog / source |
| ------ | ------------- | ---------------- |
| Customer | ○ — deferred: retur rate, Faktur Kembali | Desktop only |
| Salesman | Visit execution %; effective call rate | SF-KPI-017, SF-KPI-018 |
| Item | Days of supply; forecast health; movement class | IN-KPI-015, IN-KPI-020 |
| Supplier | Posted %; qualified backlog rate | PU-KPI-003, PU-KPI-005 |
| Warehouse* | ○ — inactive warehouse with stock flag | OP-KPI-006 |
| Collector* | Recovery vs billing % on managed book | FI-KPI / COL |

#### Risk

| Entity | KPI instances | Catalog / source |
| ------ | ------------- | ---------------- |
| Customer | Overdue; dormant; plafond breach; suspended+sales; risk category | CU-KPI-001–008; CU-KPI-021–036 |
| Salesman | Below target; high overdue; high piutang; customer concentration | SF-KPI-001–005 |
| Item | Dead/slow/never-sold; stock-out risk; optimization priority | IN-KPI-005–008, IN-KPI-016 |
| Supplier | Backlog; compound dependency; inactivity; principal at-risk | PU-KPI-005–010 |
| Warehouse* | At-risk concentration; no-sales-with-inventory | OP-KPI-003; M22 attention |
| Wilayah* | Overdue hotspot; sales concentration | M20 COL; OP-KPI-005 |
| Category* | At-risk inventory % in category | IN-KPI-010 |
| Collector* | Chronic overdue count; legacy debt count | COL-KPI |

#### Trend

| Entity | KPI instances | Catalog / source |
| ------ | ------------- | ---------------- |
| All types | Monthly series for 3–5 primary metrics; ranking position by month; attention signal persistence | Platform history layers (§12) |

### 4.5 Standardized KPI envelope (profile presentation)

Each entity Performance Profile presents KPIs in a **consistent envelope** regardless of type:

```text
Entity Identity
  Code, Name, Status, Primary dimensions (Wilayah, Klasifikasi, Category, …)

KPI Summary (CURRENT)
  Grouped by category — Activity │ Financial │ Growth │ Risk headlines

Historical Trends
  Monthly series (minimum 12 months) for registered trend-eligible KPIs

Period Comparison
  CURRENT vs prior month vs same month prior year

Normalized Profile (Radar)
  4–6 category-derived axes, peer-normalized (§8)

Ranking Context
  Current rank + rank history per registered rank metrics

Attention History
  Signal timeline (FirstSeen → LastSeen → Active/Resolved)

Comparison Slot
  0–4 peer entities of same type

Related Entities
  Top-N linked entities (§9)

Evidence
  Drill-down map to reports (§10)
```

### 4.6 Entity-specific headline KPI sets (quick reference)

| Entity | Activity | Financial | Growth | Risk (headline) |
| ------ | -------- | --------- | ------ | --------------- |
| **Customer** | Last purchase date, MTD Faktur count | MTD omzet, open balance | MoM omzet Δ% | Overdue, dormant, plafond breach |
| **Salesman** | Active flag, customers invoiced MTD | MTD omzet, achievement % | 3-month achievement trend | Below target, high overdue exposure |
| **Item** | Last Faktur date, movement class | Inventory value, on-hand qty | 3-month consumption trend | Dead/slow/never-sold, stock-out risk |
| **Supplier** | MTD invoice count, posting status | MTD purchase, inventory value | MoM purchase Δ% | Backlog, compound dependency |
| **Warehouse*** | MTD Faktur count, stock events | Inventory value, MTD sales | MoM sales Δ% | At-risk concentration, inactive-with-stock |
| **Wilayah*** | Active customers MTD | MTD omzet, overdue exposure | MoM omzet Δ% | Hotspot overdue, sales concentration |

All headline KPIs map to §4.4 instances and ultimately to `btr-portal-kpi-catalog.md` or approved catalog extensions.

---

## 5. KPI Metadata Architecture

Entity Analytics should treat KPIs as **registered metadata** consumed by a reusable analytics engine — not as ad hoc fields duplicated in each entity aggregator. This is a **conceptual architecture**; physical storage is an Architect decision.

### 5.1 Why metadata matters

| Problem today | Metadata solution |
| ------------- | ----------------- |
| Each milestone (M17, M18, M31) embeds KPI meaning in aggregator code and snapshot columns | KPI meaning lives in a registry; aggregators **produce values** against KPI IDs |
| Adding a KPI to Customer profile requires copying patterns from Salesman | Register KPI once with entity applicability; engine exposes it on all applicable profiles |
| Radar axes, comparison slots, and trend charts hardcode metric lists | Visualization and normalization read from KPI metadata |
| Dashboard and profile KPIs may diverge silently | Single KPI ID links dashboard card, profile summary, and catalog definition |

**Scope boundary:** Computation logic remains in aggregators (reuse existing DALs). Metadata describes **what** the KPI is and **how it behaves** — not SQL or code.

### 5.2 KPI metadata attributes (conceptual model)

| Attribute | Purpose | Example |
| --------- | ------- | ------- |
| **KPI Identifier** | Immutable link to `btr-portal-kpi-catalog.md` entry | `CU-KPI-009` |
| **KPI Category** | Generic taxonomy (§4) | `Financial` |
| **Display Name** | Management-facing label | `MTD Omzet` |
| **Description** | Business meaning (WHAT from catalog) | Invoiced sales current month |
| **Calculation Source** | Authoritative data origin (not implementation) | `FakturView.GrandTotal`, current month, non-void |
| **Period Semantics** | `MTD` · `AllTimeOpen` · `PointInTime` · `MonthClosed` | Customer omzet = MTD; piutang = AllTimeOpen |
| **Unit** | `IDR` · `Count` · `Percent` · `Days` · `Band` | Omzet = IDR; Achievement = Percent |
| **Direction** | `HigherIsBetter` · `LowerIsBetter` · `Neutral` | Overdue balance = LowerIsBetter |
| **Normalization Rule** | How radar/comparison scales the KPI | `PeerPercentile` · `AchievementBand` · `None` |
| **Visualization Type** | Default profile presentation | `Card` · `Trend` · `Comparison` · `RadarAxis` · `Table` |
| **Applicable Entity Types** | Which profiles expose this KPI | `Customer`, `CustomerPortfolio` |
| **Trend Eligible** | Whether monthly history is retained | Yes / No |
| **Rank Eligible** | Whether ranking history is computed | Yes / No — with rank metric key |
| **Evidence Link** | Report route and filter dimension for drill-down | `SalesReport` + `CustomerCode` |
| **Introduced Version** | Portal release when KPI was added | `M17` |
| **Deprecated Version** | When KPI was retired (nullable) | `null` |
| **Supersedes / Superseded By** | KPI lineage for versioning | `CU-KPI-009` unchanged |

### 5.3 Metadata registry relationship to existing catalog

```text
btr-portal-kpi-catalog.md (SSOT — business definitions)
        ↓
Entity Analytics KPI Registry (extends with profile-specific metadata)
        ↓
Entity Type KPI Pack (subset of registry IDs per entity)
        ↓
Snapshot Producer (aggregator writes values by KPI ID)
        ↓
Profile Engine (reads values + metadata for presentation)
```

**Rule:** Every profile KPI must have a catalog entry or an approved catalog extension request. Profile-only KPIs without catalog backing are **not allowed** — violates Evidence First.

### 5.4 Entity Type KPI Pack (extensibility pattern)

A **KPI Pack** is the declarative list of KPI IDs an entity type exposes, grouped by profile section:

| Pack component | Contents |
| -------------- | -------- |
| Summary KPIs | 8–15 headline KPI IDs across categories |
| Trend KPIs | 3–5 KPI IDs with `TrendEligible = true` |
| Radar axes | 4–6 KPI IDs or category rollups with normalization rules |
| Rank metrics | 1–3 KPI IDs with `RankEligible = true` |
| Comparison KPIs | Subset of summary for side-by-side view |
| Attention signals | Signal codes mapped to Risk category |

**New entity type onboarding** (e.g., Warehouse):

1. Define KPI Pack by reusing existing KPI instances from §4.4 where possible.
2. Register any new KPI IDs in catalog.
3. Register entity type in metadata — **no changes to profile information architecture** (§10).

### 5.5 Duplication reduction outcomes

| Without metadata | With metadata |
| ---------------- | ------------- |
| 4 entity aggregators each define omzet % formatting | One `Percent` unit rule applied by engine |
| Radar normalization coded per entity | `NormalizationRule` on KPI metadata |
| Comparison view picks different metrics per type | `Comparison KPI Pack` per entity type, same engine |
| Dashboard CU-KPI-009 ≠ profile omzet semantics | Same KPI ID end-to-end |

---

## 6. KPI Lifecycle and Versioning

Entity Analytics must remain maintainable for many years as business rules, portal milestones, and entity types evolve.

### 6.1 KPI addition

| Step | Rule |
| ---- | ---- |
| 1 | New KPI receives immutable catalog ID (e.g., `CU-KPI-072`) |
| 2 | Metadata registry entry created with `IntroducedVersion` |
| 3 | KPI added to applicable Entity Type KPI Pack(s) |
| 4 | Aggregator produces value at next refresh |
| 5 | **Historical backfill policy** decided per KPI (see §6.4) |

**Profile behavior:** New KPIs appear in summary when data exists; trend charts show history only from introduction forward unless backfilled.

### 6.2 KPI deprecation

| Step | Rule |
| ---- | ---- |
| 1 | Catalog entry marked deprecated; `DeprecatedVersion` set |
| 2 | KPI removed from **new** KPI Packs — hidden from default profile |
| 3 | KPI values **continue to be stored** in historical snapshots for periods when active |
| 4 | Dashboard consumers migrated to replacement KPI before deprecation |
| 5 | `Superseded By` link points management to replacement metric |

**Never** silently redefine an existing KPI ID to mean something different — that breaks Historical by Default and Evidence First.

### 6.3 Semantic change (breaking change)

When calculation rules change (e.g., dormant threshold 90 → 120 days):

| Approach | Recommendation |
| -------- | -------------- |
| Same KPI ID, new rule applied retroactively | **Reject** — corrupts historical comparison |
| New KPI ID, old ID deprecated | **Recommended** — clear lineage |
| Versioned KPI definition (`CU-KPI-006-v2`) | **Acceptable** — metadata carries `DefinitionVersion` per stored value |

**Historical periods** continue to use the definition version that was active when the period was computed. Recomputation of closed periods requires explicit **administrative backfill** (§12.4) — not automatic silent overwrite.

### 6.4 Missing historical values

| Situation | Representation | User-facing behavior |
| --------- | -------------- | -------------------- |
| KPI introduced after entity existed | No value for prior periods | Trend shows gap or "not available before [period]" |
| Entity did not exist in period | No row | Omit from ranking history |
| Entity inactive (no transactions) | Zero or null per KPI semantics | Activity KPIs = 0; Financial may be 0 or null |
| Source data unavailable at refresh | Null with freshness caveat | Show `GeneratedAt`; do not fabricate zero |
| Deprecated KPI in old period | Value retained | Read-only historical display |

**Rule:** Distinguish **zero** (entity had no activity) from **missing** (KPI not computed for period) from **not applicable** (KPI did not exist yet).

### 6.5 KPI definition versioning (recommended model)

| Layer | Versioned? | Notes |
| ----- | ---------- | ----- |
| KPI Identifier | **Never** — immutable | `CU-KPI-009` is forever |
| KPI business definition (catalog) | **Evolves via deprecation** | Prefer new ID over silent edit |
| KPI calculation policy | **Versioned per period** | Stored as `DefinitionVersion` on monthly history rows when breaking change occurs |
| KPI metadata (visualization, radar) | **Versioned independently** | Presentation can improve without changing stored values |
| Aggregator implementation | **Not versioned in profile** | Test reconciliation against catalog |

### 6.6 Long-term maintainability rules

1. **One KPI, one meaning, one ID** — documented in catalog.
2. **Profile never invents metrics** — only displays registered KPIs.
3. **Closed months are immutable** unless explicit backfill approved by Product Owner.
4. **Entity type addition** registers KPI Pack — does not fork history model.
5. **Annual catalog review** aligns dashboard, profile, and alert registry KPI references.

---

## 7. Radar Chart Feasibility

### 7.1 Assessment

Radar chart is **appropriate** for Entity Analytics **if used as a summary index**, not as a precision decision tool. Management benefits from seeing **balanced vs lopsided** entity health (e.g., high omzet but poor collection).

**Cautions:**

- Radar charts mislead when dimensions are correlated (omzet and Faktur count).
- Too many dimensions reduces interpretability.
- Absolute values on different scales require normalization.

### 7.2 Recommended dimension count

| Entity | Recommended axes | Example dimensions |
| ------ | ---------------- | ------------------ |
| Customer | **5** | Activity, Omzet scale, Growth, Collection health, Credit risk |
| Salesman | **6** | Activity, Omzet achievement, Growth, Customer coverage, Collection accountability, Target discipline |
| Item | **5** | Velocity (activity), Inventory scale, Growth (consumption), Stock-out risk, Obsolescence risk |
| Supplier | **5** | Purchase activity, Spend scale, Growth, Inventory dependency, Operational risk (posting/backlog) |

**Maximum:** 6 dimensions. **Minimum viable:** 4 dimensions.

### 7.3 Normalization approaches compared

Radar charts require converting heterogeneous KPIs onto a common scale. The following approaches were evaluated for management dashboards in BTR Portal.

#### 7.3.1 Min-Max Scaling

| Aspect | Assessment |
| ------ | ---------- |
| **Method** | Linear scale: `(value - min) / (max - min)` within peer group |
| **Advantages** | Simple; preserves ordering; easy to explain |
| **Disadvantages** | **Highly sensitive to outliers** — one dominant customer compresses all others; min/max unstable month-to-month |
| **Management dashboard suitability** | **Low** — distribution businesses have extreme concentration |
| **BTR Portal suitability** | **Low** — contradicts concentration analysis already central to portal |

#### 7.3.2 Percentile Ranking

| Aspect | Assessment |
| ------ | ---------- |
| **Method** | Entity's percentile rank within peer group (0–100) |
| **Advantages** | **Robust to outliers**; intuitive ("better than 80% of peers"); comparable across axes |
| **Disadvantages** | Loses absolute magnitude; small peer groups produce coarse scores; ties require tie-break rules |
| **Management dashboard suitability** | **High** — relative performance is how management coaches and benchmarks |
| **BTR Portal suitability** | **High** — aligns with Top-N and concentration thinking |

#### 7.3.3 Z-Score (Standard Score)

| Aspect | Assessment |
| ------ | ---------- |
| **Method** | `(value - mean) / standard deviation` within peer group |
| **Advantages** | Statistically principled; handles dispersion |
| **Disadvantages** | **Not intuitive for non-technical management**; assumes approximate normal distribution (rarely true for omzet/piutang); extreme skew produces misleading scores |
| **Management dashboard suitability** | **Low** — requires statistical literacy |
| **BTR Portal suitability** | **Low** — inconsistent with existing band-based thresholds (Healthy/Warning/Critical) |

#### 7.3.4 Threshold / Band Based

| Aspect | Assessment |
| ------ | ---------- |
| **Method** | Map KPI to discrete bands using portal thresholds (e.g., Achievement ≥100% = Healthy → score 100) |
| **Advantages** | **Consistent with existing portal semantics**; explainable; stable month-to-month |
| **Disadvantages** | Requires defined thresholds per KPI; coarse (3–4 levels); not all KPIs have bands today |
| **Management dashboard suitability** | **High** — matches how board already reads Achievement and aging |
| **BTR Portal suitability** | **Very high** — reuses SA achievement bands, aging buckets, movement classes |

#### 7.3.5 Peer Group Based (composite)

| Aspect | Assessment |
| ------ | ---------- |
| **Method** | Define peer scope first (Wilayah, category, all active), then apply normalization within group |
| **Advantages** | **Fair comparison** — customer vs customers in same territory; item vs same category |
| **Disadvantages** | Peer group definition is a product decision; small groups weaken percentile/band reliability |
| **Management dashboard suitability** | **Very high** — mirrors real coaching and negotiation context |
| **BTR Portal suitability** | **Very high** — Wilayah, Klasifikasi, category already exist as dimensions |

### 7.4 Recommended normalization strategy (platform standard)

**Adopt: Peer Group Based + Percentile Ranking, with Band Based fallback.**

| Rule | Specification |
| ---- | ------------- |
| **Primary method** | Percentile rank (0–100) within declared peer group |
| **Peer group default** | Entity-type-specific: Customer → same `Wilayah`; Item → same `Category`; Salesman → all active reps; Supplier → all principals with MTD activity |
| **Fallback when band exists** | KPIs with portal bands (Achievement %, movement class) may use **band midpoint score** instead of percentile when peer group &lt; 10 entities |
| **Direction** | Invert `LowerIsBetter` metrics before ranking |
| **Outliers** | Percentile inherently handles — no separate winsorization required |
| **Missing data** | Omit axis — never score as 0 |
| **Minimum peer group** | Radar hidden when peer group &lt; 5 eligible entities; show numeric KPIs only |
| **Refresh** | Scores computed at snapshot refresh — stored, not browser-computed |
| **Comparison overlay** | Two entities of same type on identical axes and peer group |

**Why this combination wins:**

1. **Percentile** handles BTR's skewed distributions (few large customers dominate).
2. **Peer groups** make scores meaningful for mid-tier entities outside company Top 10.
3. **Band fallback** preserves consistency with Achievement and movement-class semantics management already trusts.
4. Avoids Z-Score complexity and Min-Max outlier fragility.

**This becomes the standard for all future radar charts** — domain dashboards, entity profiles, and any future optimization surfaces.

### 7.5 Implementation notes (product level only)

| Concern | Product rule |
| ------- | ------------- |
| Radar axis definition | Each axis maps to one KPI ID or category rollup per metadata (§5) |
| Axis count | 4–6 per entity type (§7.2) |
| Cross-type comparison | Not supported |
| MVP | Radar may defer to M32.3+; normalization standard still decided now to avoid rework |

### 7.6 Shared model vs entity-specific radars

**Recommendation: shared framework, entity-specific axis sets.**

| Approach | Verdict |
| -------- | ------- |
| One identical radar for all entity types | **Reject** — dimensions are not semantically aligned (customer "plafond" has no item equivalent) |
| Completely independent radars per type | **Acceptable** but harder to train users |
| **Shared 8-category framework with 4–6 axes selected per type** | **Recommended** — consistent legend and scoring rules, entity-appropriate axes |

Comparison mode: when comparing two entities **of the same type**, overlay on identical axes. Cross-type comparison (Customer vs Salesman) is **out of scope** — not meaningful.

Comparison mode: when comparing two entities **of the same type**, overlay on identical axes. Cross-type comparison (Customer vs Salesman) is **out of scope** — not meaningful.

---

## 8. Cross-Entity Relationship Analysis

Entity Analytics must not treat entities as isolated objects. BTR's business graph connects customers, salesmen, items, suppliers, warehouses, and territories through transactions. **Related Entities** sections expose these links to accelerate investigation — without replacing the primary entity lens.

### 8.1 Why relationships matter

| Investigation question | Relationship answers it |
| ---------------------- | ----------------------- |
| *Why did this customer grow?* | Top purchased items and principals |
| *Who is responsible for this customer?* | Assigned salesman |
| *Is this salesman underperforming or carrying bad accounts?* | Top customers and their risk signals |
| *Is this principal risky or is one SKU the problem?* | Top products and their movement classes |
| *Why is this item slow-moving?* | Customer reach collapse vs supply issue |

Relationships turn a profile from a **statistics card** into an **investigation hub**.

### 8.2 Relationship catalog by primary entity

#### Customer Profile — related entities

| Relationship | Business meaning | Investigation value |
| ------------ | ---------------- | ------------------- |
| **Assigned Salesman** | Rep attributed to customer's sales/piutang | Accountability for coaching and collection |
| **Top Purchased Items** (MTD or 12 mo) | SKUs driving customer omzet | Assortment dependency; upsell/cannibalization |
| **Top Suppliers / Principals** | Principals behind purchased products | Negotiation and supply risk context |
| **Top Wilayah peers** (optional) | Other customers in same territory | Local market benchmarking |
| **Related Customers** (comparison) | User-selected or suggested peers | A vs B investigation |

#### Salesman Profile — related entities

| Relationship | Business meaning | Investigation value |
| ------------ | ---------------- | ------------------- |
| **Managed Customers** | Customers invoiced MTD or on rep's book | Portfolio breadth |
| **Top Customers by Omzet** | Revenue concentration within book | Coaching focus |
| **Top Customers by Piutang** | Collection risk within book | Collection accountability |
| **Top Principals** | Principal achievement breakdown | Product focus alignment |
| **Top Items Sold** | SKUs driving rep's omzet | Assortment effectiveness |
| **Wilayah Coverage** | Territories served | Geographic balance |

#### Supplier / Principal Profile — related entities

| Relationship | Business meaning | Investigation value |
| ------------ | ---------------- | ------------------- |
| **Top Items by Inventory Value** | SKUs tying up capital | Legacy stock vs healthy catalog |
| **Top Items by Sales Velocity** | SKUs driving sales-out | Demand validation |
| **Top Customers** | Buyers of this principal's products | Demand concentration |
| **Top Salesmen** | Reps selling this principal | Channel effectiveness |
| **Related Principals** (comparison) | Peer suppliers in same category | Negotiation benchmarking |

#### Item Profile — related entities

| Relationship | Business meaning | Investigation value |
| ------------ | ---------------- | ------------------- |
| **Supplier / Principal** | Source of item | Supply and purchase context |
| **Category / Brand** | Product classification | Peer benchmarking scope |
| **Top Customers** | Buyers driving velocity | Demand concentration |
| **Top Salesmen** | Reps selling item | Sales channel focus |
| **Warehouse Stock Distribution** | Qty by warehouse | Transfer/replenishment decisions |
| **Substitute / Same-Category Items** (future) | Category peers | Assortment rationalization |

### 8.3 Future entity types — relationship expectations

| Entity type | Primary relationships |
| ----------- | --------------------- |
| **Warehouse** | Top items by value; top customers billed; linked wilayah; at-risk stock |
| **Wilayah** | Top customers; top salesmen; overdue concentration; category mix |
| **Category** | Top items; top suppliers; top customers; inventory vs sales split |
| **Brand** | SKU roster; category; supplier; sales trend rollup |
| **Collector** | Top overdue customers; assigned wilayah; recovery vs billing context |

### 8.4 Relationship data principles

| Principle | Rule |
| --------- | ---- |
| **Top-N only** | Show Top 5–10 related entities — not exhaustive lists |
| **Same evidence chain** | Each related entity row links to its own Performance Profile |
| **Snapshot computed** | Relationship rollups materialized at refresh — not live joins on profile load |
| **Shared relationship engine** | Relationship types are registered metadata (like KPI packs) — `TopCustomersByOmzet` is reusable across Salesman, Supplier, Item profiles |
| **Period alignment** | Relationship metrics use same period semantics as parent profile KPIs |

### 8.5 Duplication reduction

One **relationship rollup pattern** serves many entity types:

```text
TopRelatedEntities(
  SourceEntityType, SourceEntityId,
  TargetEntityType,
  MetricKpiId,      -- e.g., CU-KPI-009 omzet
  Period,
  TopN = 10
)
```

Customer Top Items and Salesman Top Items use the same rollup — differing only in source filter (customer vs salesman attribution).

---

## 9. Performance Profile Information Architecture

This section defines the **conceptual structure** of every Performance Profile. All entity types share this architecture — entity types differ only in KPI packs, relationship packs, and peer-group rules (§4–§5, §8).

### 9.1 Profile sections — catalog

| Section | Business purpose | Always shown? |
| ------- | ---------------- | ------------- |
| **Overview** | Identity, status, primary dimensions, data freshness | Yes |
| **KPI Summary** | Current-state headline metrics grouped by category | Yes |
| **Trend** | Historical trajectory of 3–5 primary KPIs | Yes (when history exists) |
| **Comparison** | Period-over-period deltas (MoM, YoY) | Yes |
| **Radar** | Normalized multi-dimensional health snapshot | When peer group sufficient |
| **Ranking History** | Entity's rank position over time for key metrics | When rank-eligible KPIs exist |
| **Attention History** | Timeline of attention signals — appearance, persistence, resolution | When signals ever fired |
| **Related Entities** | Top-N linked entities across the business graph | Yes — at least one relationship block |
| **Evidence** | Map from KPI categories to validating reports | Yes |
| **Timeline** (composite) | Unified chronological view of signals, rank changes, and metric inflections | Phase 2 — composes Attention + Ranking + Trend events |

### 9.2 Section definitions

#### Overview

Answers: *Who is this entity and what is its current business status?*

Contains: entity code, name, active/dormant/suspended flags, Wilayah, Klasifikasi, category, supplier, warehouse, assigned salesman (as applicable), `GeneratedAt` freshness.

#### KPI Summary

Answers: *What are the headline facts right now?*

Contains: 8–15 KPIs from CURRENT snapshot grouped by Activity, Financial, Growth, Risk (and Portfolio/Quality when applicable). Values use catalog semantics.

#### Trend

Answers: *How has this entity behaved over time?*

Contains: monthly series (minimum 12 months) for trend-eligible KPIs. Supports overlay when comparison mode active.

#### Comparison

Answers: *How does this period compare to prior periods?*

Contains: CURRENT MTD vs prior closed month vs same month prior year for key metrics. Clear labeling when comparing partial month to full month.

#### Radar

Answers: *Is this entity balanced or lopsided across dimensions?*

Contains: 4–6 peer-normalized axes (§7). Indicative only — not a decision authority.

#### Ranking History

Answers: *Where does this entity sit relative to peers over time — not just today?*

Contains: rank position by month for registered rank metrics (e.g., omzet rank, piutang rank). Explains entities outside today's Top 10 that were historically leaders.

#### Attention History

Answers: *What management signals has this entity triggered, and for how long?*

Contains: signal code, first seen period, last seen period, active/resolved status. Links to attention signal definitions in catalog/alert registry.

#### Related Entities

Answers: *What other entities explain this entity's behavior?*

Contains: Top-N relationship blocks per §8. Each row navigates to target entity's profile.

#### Evidence

Answers: *Where is the proof?*

Contains: report links with entity pre-filter — Sales Report, Piutang Report, Inventory Report, Purchasing Report as applicable. Preserves Evidence First principle.

#### Timeline (future composite)

Answers: *What happened to this entity chronologically?*

Merges attention events, rank changes, and significant metric threshold crossings into one investigation narrative. Deferred to post-MVP — requires event model from Attention History + Ranking History.

### 9.3 Section ordering (information architecture — not UI)

Recommended cognitive flow:

```text
1. Overview          — orient
2. KPI Summary       — current facts
3. Comparison        — immediate change
4. Trend             — trajectory
5. Radar             — balanced view
6. Ranking History   — competitive position
7. Attention History — exceptions over time
8. Related Entities  — explain drivers
9. Evidence          — validate and act
```

Comparison before Trend supports the management question *"what changed?"* before *"what pattern?"* — aligns with morning scan behavior.

### 9.4 Reusability across entity types

| Component | Shared? | Entity-specific? |
| --------- | ------- | -------------- |
| Section structure | **100%** | — |
| KPI Summary content | Engine | KPI Pack |
| Trend metrics | Engine | Trend KPI list |
| Radar axes | Engine | Axis KPI list + peer group |
| Relationship blocks | Engine | Relationship Pack |
| Evidence links | Engine | Report map per entity |
| Overview dimensions | Engine | Dimension list per entity |

**New entity type cost:** Define KPI Pack + Relationship Pack + peer rules — **not** a new profile page architecture.

---

## 10. Entity Navigation Model

### 10.1 Two portal questions

| Mode | Question | Primary surfaces |
| ---- | -------- | ---------------- |
| **Aggregate analytics** (today) | *What happened?* | Executive, domain dashboards, Alert Center |
| **Entity analytics** (proposed) | *Tell me everything about this entity* | Performance Profile, comparison, related entities |

Entity Analytics does not replace aggregate analytics — it **deepens** the journey when management identifies an entity worth investigating.

### 10.2 Investigation navigation flow

```text
LAYER 1 — DISCOVER (aggregate)
  Executive Dashboard (EX01)
  Alert Center (EX02)
  Domain Dashboard (SA01, FI01, CU01, SF01, IN01, PU01, …)
        ↓
  Output: "Customer X needs attention" or "I want to check Customer Y"

LAYER 2 — FOCUS (entity)
  Entity Search / Picker
  Performance Profile (entity-type route)
        ↓
  Output: holistic understanding of one entity

LAYER 3 — COMPARE (entity)
  Add peer entity → Multi-Entity Comparison
  (same entity type, 2–4 entities)
        ↓
  Output: relative performance conclusion

LAYER 4 — EXPLORE GRAPH (entity)
  Related Entities → navigate to linked profiles
  (Customer → Item → Supplier → Salesman chains)
        ↓
  Output: root-cause hypothesis

LAYER 5 — VALIDATE (evidence)
  Report with entity pre-filter (SA03, FI04, IN05, PU02, CU05)
        ↓
  Output: transaction-level proof

LAYER 6 — ACT (operational)
  BTR Desktop
        ↓
  Output: business action
```

### 10.3 Entry points into Entity Analytics

| Entry point | Example | Navigation |
| ----------- | ------- | ---------- |
| Attention list row | CU01 customer attention | Click customer → Customer Profile |
| Top 10 ranking row | SA01 Top Salesman | Click rep → Salesman Profile |
| Alert Center entity alert | Overdue customer alert | Click → Customer Profile |
| Customer Report row | CU05 portfolio report | Click → Customer Profile |
| Executive Top 5 exposure | EX01 critical customer | Click → Customer Profile |
| Global entity search | "Find item ABC123" | Search → Item Profile |
| Related entity link | Customer Profile → Top Item | Click → Item Profile |

**Every existing entity row in portal** should become a deep link to Performance Profile — unifying navigation.

### 10.4 Why this beats Top-N-only dashboards

| Top-N limitation | Entity navigation advantage |
| ---------------- | --------------------------- |
| Only 10 entities visible | Any entity reachable via search |
| No history for #11–#500 | Ranking history shows position even when outside Top 10 |
| Separate dashboards per domain | One profile composes cross-domain KPIs |
| Comparison requires export to Excel | Built-in 2–4 entity comparison |
| Relationships implicit | Related Entities make business graph explicit |
| Investigation jumps straight to report | Profile provides context before row-level evidence |

### 10.5 Navigation principles

1. **Never dead-end** — every profile section links forward (related entity, report, or domain dashboard).
2. **Breadcrumb context** — profile shows how user arrived (from CU01, Alert, Search).
3. **Same-type comparison only** — comparison mode enforces entity type match.
4. **Report is penultimate** — profile before report, report before Desktop.
5. **Cloud navigation** — profiles available in cloud; report drill-down may route to on-prem (§13).

### 10.6 Relationship to M24 Drill Down

M24 established **Dashboard → Report** drill-down. Entity Analytics inserts **Profile** as the intermediate depth:

```text
M24 today:     Dashboard → Report → Desktop
M32 proposed:  Dashboard → Profile → Report → Desktop
               Dashboard → Profile → Related Profile → Report → Desktop
```

---

## 11. Historical Snapshot Strategy

### 11.1 Current state

| Pattern | Usage today |
| ------- | ----------- |
| `SnapshotKey = 'CURRENT'` delete-and-replace | All domain and cross-domain snapshots |
| Period-keyed upsert | **`BTRPD_SalesmanRepHistory` only** — monthly per rep, retained indefinitely |
| Live report queries | Evidence layer — full transaction grain |
| Recalculate on demand | Removed from dashboard read path after materialization |

Entity Analytics **cannot** be built on CURRENT-only snapshots alone for trends, period comparison, ranking history, or attention history.

### 11.2 Alternatives compared

| Strategy | Description | Pros | Cons |
| -------- | ----------- | ---- | ---- |
| **A. Recalculate from transactions** | Profile API queries `FakturView`, piutang, stock on each request | Always fresh; no new history tables | **Defeats materialization**; slow for piutang and inventory; unacceptable for cloud |
| **B. Monthly entity snapshot** | Upsert one row per entity per calendar month | Aligns with management cadence; minimal storage; matches `SalesmanRepHistory` precedent | Mid-month profile is stale within month for MTD metrics |
| **C. Weekly entity snapshot** | Upsert per entity per ISO week | Finer trend for fast-moving sales | 4× storage vs monthly; still not daily |
| **D. Daily entity snapshot** | Upsert per entity per day | Fine-grained trends | **Impractical at item scale** (thousands of SKUs × 365 days) |
| **E. Hybrid (recommended)** | **Monthly KPI history** for all entities + **CURRENT intra-month** for MTD + **event log** for attention signals | Balances cost and UX | Two temporal layers to document |

### 11.3 Recommended hybrid model

```text
L0 — CURRENT (existing pattern, generalized)
  Today's profile KPIs at dashboard refresh cadence (15–60 min per domain)

L1 — MONTHLY HISTORY (new — generalizes SalesmanRepHistory)
  KPI-ID-keyed values per (EntityType, EntityId, PeriodYear, PeriodMonth)
  Upserted through month (Salesman precedent) or frozen at month close

L2 — RANKING HISTORY (new, lightweight)
  Rank position per (EntityType, EntityId, Period, RankMetric)

L3 — ATTENTION HISTORY (new)
  Signal log: (EntityType, EntityId, SignalCode, FirstSeenPeriod, LastSeenPeriod, IsActive)

L4 — RELATIONSHIP ROLLOPS (new)
  Top-N related entities per (SourceEntity, TargetEntityType, Metric)

L5 — RADAR SCORES (new)
  Precomputed axis scores per (EntityType, EntityId, Period) — §7.4 standard
```

See §12 for evolution and extensibility rules.

**Period comparison semantics:**

| Comparison | Source |
| ---------- | ------ |
| MTD vs prior month (partial month) | CURRENT vs MONTHLY HISTORY |
| Full month vs full month | MONTHLY HISTORY |
| Same month YoY | MONTHLY HISTORY |

### 11.4 Storage estimates (order-of-magnitude)

Assumptions: medium-large distributor — 3,000 customers, 80 salesmen, 8,000 active items, 250 principals; 36 months retention; ~30 numeric KPI fields per monthly row (~300 bytes/row including keys and metadata).

| Store | Row count (approx.) | 36-month storage (approx.) |
| ----- | ------------------- | -------------------------- |
| Customer monthly | 3,000 × 36 = 108,000 | ~32 MB |
| Salesman monthly | 80 × 36 = 2,880 | ~1 MB |
| Item monthly | 8,000 × 36 = 288,000 | ~86 MB |
| Supplier monthly | 250 × 36 = 9,000 | ~3 MB |
| Ranking history (4 metrics × entities) | +30% overhead | ~35 MB |
| Attention history log | ~50,000 cumulative events | ~15 MB |
| **Total** | | **~170 MB** |

**Daily item snapshots** (8,000 × 365 × 36) ≈ **105M rows / ~30 GB** — **not recommended**.

**Conclusion:** Monthly entity snapshots are **economically negligible** on-premises. Even 10× larger entity counts remain manageable. Item cardinality drives storage — item history may use **category-level rollup** for trend context if full SKU history is deferred.

### 11.5 Cloud synchronization implications (snapshot layer)

If cloud portal hosts read-only analytics:

| Data class | Sync to cloud? | Rationale |
| ---------- | -------------- | --------- |
| Transaction tables (`BTR_Faktur`, `BTR_Piutang`, …) | **No** | Size, confidentiality, operational coupling |
| CURRENT snapshots (`BTRPD_*`) | **Yes** | Already aggregated; small (tens of MB) |
| Monthly entity history (proposed) | **Yes** | ~170 MB baseline — acceptable |
| Reports (live queries) | **No** — or **cloud cannot serve reports** | Reports require full transaction access |

**Sync pattern:** Worker runs on-premises → writes snapshots → replicates snapshot tables (or API export) to cloud SQL. Cloud portal **reads snapshots only**.

**Bandwidth (monthly full refresh of history):** ~170 MB — trivial. **Daily delta** of CURRENT snapshots: &lt; 10 MB — trivial.

**Bandwidth (monthly full refresh of history):** ~170 MB — trivial. **Daily delta** of CURRENT snapshots: &lt; 10 MB — trivial.

---

## 12. Snapshot Evolution Strategy

Historical storage (§11) answers *how much*. This section answers *how the snapshot model evolves over years* without breaking profiles, cloud sync, or dashboard reconciliation.

### 12.1 Design goals for evolution

| Goal | Principle |
| ---- | --------- |
| Add KPIs without schema explosion | KPI-ID-keyed values in generic history store |
| Add entity types without new history infrastructure | Same monthly + CURRENT + ranking + attention layers |
| Preserve closed-period immutability | Month-close rows are not silently overwritten |
| Support selective backfill | Administrative recomputation — not default behavior |
| Maintain portal version compatibility | Consumers tolerate unknown KPI IDs gracefully |

### 12.2 Generic snapshot layers (future-proof model)

| Layer | Scope | Generic? | Purpose |
| ----- | ----- | -------- | ------- |
| **L0 — CURRENT entity metrics** | All registered KPIs for active entities | **Yes** | Intra-month MTD and point-in-time values |
| **L1 — MONTHLY entity history** | KPI values by (EntityType, EntityId, Period) | **Yes** | Trends, comparison, closed months |
| **L2 — RANKING history** | Rank position by (EntityType, Metric, Period) | **Yes** | Ranking History section |
| **L3 — ATTENTION history** | Signal events by (EntityType, EntityId, Signal) | **Yes** | Attention History section |
| **L4 — RELATIONSHIP rollups** | Top-N related entities by (Source, Target, Metric) | **Yes** | Related Entities section |
| **L5 — RADAR scores** | Precomputed axis scores by (EntityType, EntityId, Period) | **Yes** | Radar section |
| **Domain snapshots (`BTRPD_*`)** | Existing dashboard tables | **Per-domain** | Unchanged — profiles do not replace domain snapshots |

**Key architectural decision:** L0–L5 are **entity analytics platform layers** — generic across entity types. Domain dashboard snapshots (Sales, Piutang, Customer KPI, etc.) remain as today for aggregate dashboards.

**Duplication note:** Profile CURRENT values may duplicate fields in domain snapshots (e.g., customer omzet in both `BTRPD_CustomerTopOmzet` and L0). **Acceptable** — profiles need all-entity coverage, not Top-10-only. Aggregators should compute once per refresh and write to both consumers, or L0 reads from domain snapshot where coverage is complete.

### 12.3 Evolving snapshots when new KPIs are introduced

| Scenario | Behavior |
| -------- | -------- |
| New KPI added to KPI Pack | L0 begins populating at next refresh; L1 from introduction month forward |
| New KPI with backfill approved | Administrative job recomputes L1 for specified periods — logged in refresh metadata |
| New KPI replaces deprecated KPI | Old KPI values remain in L1 under old ID; new ID populates forward |
| New radar axis | L5 populates when axis registered; prior periods have no radar axis (omit) |

**Schema evolution (product rule):** Prefer **adding KPI ID rows** to generic value store over adding columns per KPI — Architect decides physical representation; product requires KPI-ID extensibility.

### 12.4 When should history be recalculated?

| Trigger | Recalculate? | Approval |
| ------- | ------------ | -------- |
| Routine refresh | Current month upsert only | Automatic |
| Month close | Freeze prior month L1 row | Automatic |
| Bug fix in aggregator | **Selective backfill** for affected periods | Product Owner + IT |
| Semantic KPI change (new ID) | No retroactive change to old ID | — |
| Semantic KPI change (same ID) | **Avoid** — use new ID instead | — |
| New entity type added | Forward-only from registration | Automatic |
| Master data correction (customer merge) | Entity ID migration policy | Product Owner — rare |

**Default:** History is **append/update forward**, not globally recomputed. Full recomputation from transactions is an **exceptional maintenance operation** — same class as manual `btr.portal.worker --domain All`.

### 12.5 New entity type reuse pattern

```text
1. Register EntityType in metadata (code, display name, master DAL)
2. Define KPI Pack — reuse KPI IDs from §4.4 where possible
3. Define Relationship Pack — reuse relationship rollups from §8
4. Define peer group rules for radar
5. Plug into existing worker refresh order (new domain or child of existing)
6. L0–L5 layers populate automatically — no new history infrastructure
```

**Warehouse example:** Reuses OP-KPI instances, M22 attention signals, Location aggregator outputs — registers as `EntityType = Warehouse` without forking profile architecture.

### 12.6 Snapshot compatibility across portal versions

| Concern | Policy |
| ------- | ------ |
| Cloud behind on-prem version | Cloud ignores unknown KPI IDs; displays known subset |
| On-prem ahead of cloud sync | Cloud catches up on next sync — no partial KPI corruption |
| API response shape | Profile engine sends KPI list with metadata; clients render dynamically |
| Missing L1 periods | Trend shows gap — not error |
| Retention purge | Periods older than retention deleted from L1 — documented cutoff |

### 12.7 Relationship to existing `BTRPD_SalesmanRepHistory`

`BTRPD_SalesmanRepHistory` is a **domain-specific precursor** to L1. Migration path:

| Option | Recommendation |
| ------ | -------------- |
| Keep RepHistory alongside generic L1 | **Short term** — M32.3 reads RepHistory; no forced migration |
| Backfill L1 from RepHistory | **Medium term** — unify salesman trends |
| Deprecate RepHistory when L1 complete | **Long term** — single history model |

Salesman profile should not block on migration — read from RepHistory until L1 is populated.

---

## 13. Cloud Architecture Feasibility

### 13.1 Constraints (as stated)

| Constraint | Implication |
| ---------- | ----------- |
| Production transaction DB extremely large | Cannot replicate to cloud |
| Transaction data confidential | Cloud must not hold row-level Faktur/piutang |
| Portal cloud should remain lightweight | Read-only analytics over pre-aggregated data |

### 13.2 Are periodic KPI snapshots sufficient?

**Yes — necessary and sufficient** for Entity Analytics Performance Profiles, with caveats:

| Profile section | Satisfied by snapshots alone? |
| --------------- | ------------------------------ |
| KPI summary | **Yes** — CURRENT |
| Historical trends | **Yes** — monthly history layer |
| Period comparison | **Yes** — monthly history + CURRENT |
| Radar chart | **Yes** — derived at refresh from CURRENT + peer group |
| Multi-entity comparison | **Yes** — read N entity rows from snapshot |
| Ranking history | **Yes** — ranking history layer |
| Attention history | **Yes** — attention log layer |
| **Drill-down to report evidence** | **No** — requires on-prem portal or synced report extract |

### 13.3 Expected synchronization volume

| Component | Estimated size | Sync frequency |
| --------- | -------------- | -------------- |
| Existing `BTRPD_*` CURRENT (~46+ tables) | 5–20 MB | Every 15–60 min (delta) |
| Proposed entity monthly history | ~170 MB (36 mo) | Daily or on refresh |
| Proposed ranking + attention history | ~50 MB | Daily |
| **Total cloud analytics store** | **&lt; 300 MB** | — |

Compare to transaction DB (often **100 GB – 1 TB+**): snapshot sync is **&lt; 0.3%** of data volume.

### 13.4 Offline and online architecture (conceptual)

```text
ON-PREMISES (authoritative)
  BTR Desktop + SQL Server (transactions)
        ↓
  btr.portal.worker (aggregators)
        ↓
  BTRPD_* snapshots + entity history tables
        ↓
  [Optional] Snapshot sync agent
        ↓
CLOUD (read-only)
  Cloud SQL (snapshots only) OR object store + API
        ↓
  btr.portal.api (cloud) + btr.portal.web
        ↓
  Management browsers (anywhere)

ON-PREMISES PORTAL (full)
  Same API + web against local SQL
  Reports with live transaction queries remain available
```

**Recommendation:** Maintain **parity of analytics** (dashboards + profiles) in cloud; keep **reports and evidence drill-down** on-premises unless a future "synced report extract" milestone is approved. Cloud sync is a **platform capability** — see §17.2.

---

## 14. Technical Complexity

Complexity is classified for **representative profile KPIs**, not all ~214 portal KPIs.

**Legend:** **Easy** = reuse existing aggregator output or single DAL field · **Medium** = new aggregation but source data exists · **Difficult** = new data source, ambiguous semantics, or high cardinality performance risk

### 14.1 Customer profile KPIs

| KPI | Complexity | Why |
| --- | ---------- | --- |
| MTD omzet, Faktur count | **Easy** | `FakturView` — M17 pattern |
| Open balance, aging, overdue | **Easy** | Piutang open balance DAL — M17 pattern |
| Dormant, plafond, suspended signals | **Easy** | M17 attention rules exist |
| Portfolio lifecycle/tier/action | **Easy** | M31 `CustomerPortfolioCustomer` row |
| Risk forecast category | **Easy** | M29 top-customer risk rows |
| % of company omzet/piutang | **Easy** | Denominator in KPI snapshot |
| Monthly omzet trend (12 mo) | **Medium** | New monthly history — source exists |
| MoM / YoY growth % | **Medium** | Derived from monthly history |
| Ranking history | **Medium** | New lightweight store |
| Attention history timeline | **Medium** | Signal persistence log |
| Retur rate | **Difficult** | Desktop `ReturJualView` not in portal |
| Collection payment mix trend | **Difficult** | Pelunasan aggregation per customer |
| Radar normalization | **Medium** | Peer percentile computation at refresh |

### 14.2 Salesman profile KPIs

| KPI | Complexity | Why |
| --- | ---------- | --- |
| MTD omzet, achievement %, band | **Easy** | M18 snapshot |
| Attention signals | **Easy** | M18 attention list |
| Principal achievement table | **Easy** | M18 V2.2 |
| Monthly achievement trend | **Easy** | **`BTRPD_SalesmanRepHistory` already exists** |
| Customer count, dormant portfolio | **Easy** | M18 aggregator |
| Ranking history | **Medium** | Not stored today |
| Field activity trend | **Difficult** | SF02 is day-grain; multi-month visit trend is new |
| Radar normalization | **Medium** | Peer group = all active reps |

### 14.3 Item profile KPIs

| KPI | Complexity | Why |
| --- | ---------- | --- |
| On-hand qty, inventory value | **Easy** | Inventory report / stok balance |
| Movement class (dead/slow/active) | **Easy** | M19 aggregator |
| Last Faktur date | **Easy** | `IBrgLastFakturDal` |
| Days of supply, stock-out risk | **Easy** | M28 forecast per item |
| Optimization action | **Easy** | M28.5 per item |
| Monthly consumption trend | **Medium** | `IBrgConsumptionDal` — not retained monthly |
| Customer reach (distinct buyers) | **Medium** | FakturItem join — new per-item aggregate |
| Sales omzet attributed to item | **Medium** | FakturItem sum by month |
| Ranking history | **Medium** | New store at item scale |
| Full SKU monthly history (all items) | **Difficult** | Cardinality 5k–20k × months |
| Radar normalization | **Medium** | Peer group = same category |

### 14.4 Supplier profile KPIs

| KPI | Complexity | Why |
| --- | ---------- | --- |
| MTD purchase, invoice count | **Easy** | PU01 snapshot |
| Posted %, backlog | **Easy** | M21 purchasing management |
| Top principal rank | **Easy** | PU01 Top 10 |
| Inventory value by supplier | **Easy** | M15 breakdown |
| At-risk inventory by supplier | **Easy** | M19 breakdown |
| Compound dependency flag | **Easy** | M21 attention |
| Monthly purchase trend | **Medium** | Invoice history by month |
| Sales-out velocity of catalog | **Medium** | FakturItem × Supplier |
| Ranking history | **Medium** | New store |
| Radar normalization | **Medium** | Peer group = active principals |

### 14.5 Cross-cutting features

| Feature | Complexity | Why |
| ------- | ---------- | --- |
| Entity search / picker | **Easy** | Master data lists exist |
| Multi-entity comparison (same type) | **Medium** | API composition of N profiles |
| Performance Profile shell (4+ entity types) | **Medium** | Shared §9 information architecture; entity KPI/relationship packs only |
| Generic L0–L5 history layers | **Medium** | Extends `SalesmanRepHistory` pattern to all types |
| KPI metadata registry | **Medium** | One-time platform setup — reduces all future entity cost |
| Relationship rollup engine | **Medium** | Shared `TopRelatedEntities` pattern (§8) |
| Unified radar (normalization) | **Medium** | Peer percentile + band fallback per §7.4 standard |
| Report drill-down from profile | **Easy** | M24 pattern exists for customer |

---

## 15. Performance Considerations

### 15.1 Query complexity

| Operation | Expected pattern | Risk |
| --------- | ---------------- | ---- |
| Load single entity profile | PK lookup on CURRENT + latest monthly rows | **Low** |
| Load 12-month trend | 12 rows by (EntityId, Period) | **Low** |
| Multi-entity compare (2–4 entities) | N × profile load | **Low** |
| Radar peer percentile | Precomputed at refresh | **Low** on read; **Medium** on refresh |
| Item search across full catalog | Typeahead on master | **Medium** — index master |
| Rank history for one entity | 36 rows | **Low** |

**Anti-pattern:** Live `FakturView` scan per profile request — must be rejected.

### 15.2 Snapshot generation cost

| Domain | Incremental approach | Cost driver |
| ------ | -------------------- | ----------- |
| Customer monthly history | Extend Customer worker | Already loads all customers for M31 |
| Salesman monthly history | **Exists** | Minimal incremental |
| Item monthly history | New — iterate active items | **Item count** — largest cost |
| Supplier monthly history | Extend Purchasing worker | Low principal count |

**Mitigation:** Generate item monthly history only for **items with stock &gt; 0 or sale in last 24 months**; archive dormant SKUs annually.

### 15.3 Cloud synchronization

| Factor | Assessment |
| ------ | ---------- |
| Payload size | Negligible (&lt; 300 MB total) |
| Frequency | Delta sync every 15–60 min viable |
| Conflict resolution | On-prem always wins — cloud is read replica |
| Latency | Acceptable for management analytics (not operational) |

### 15.4 Historical comparison

| Comparison type | Compute location | Read cost |
| --------------- | ---------------- | --------- |
| MoM | Precomputed monthly rows | O(1) |
| YoY | Two monthly rows | O(1) |
| Partial month vs full month | CURRENT vs monthly | O(1) with clear UI labeling |

### 15.5 Multi-entity comparison

| Aspect | Recommendation |
| ------ | -------------- |
| Max entities compared | **4** (cognitive limit + chart readability) |
| Comparison metrics | Subset of headline KPIs + 1 shared trend |
| Mixed-period guard | Warn if comparing entities with different data freshness |

### 15.6 Radar generation

Compute at **refresh time**, store scores in snapshot — not at browser request. Regenerate peer group percentiles when monthly period rolls.

---

## 16. Risks

### 16.1 Technical risks

| Risk | Severity | Mitigation |
| ---- | -------- | ---------- |
| Item cardinality blows refresh time | High | Scope item history to active SKUs; phase item profiles after customer/salesman |
| Dual temporal layers confuse users (CURRENT vs monthly) | Medium | Clear period labels; match Salesman trend pattern |
| Cloud profile without report drill-down feels incomplete | Medium | Label cloud as "analytics only"; deep links to on-prem where available |
| Peer normalization gaming (small peer groups) | Low | Minimum peer group size before showing radar |
| Snapshot drift between on-prem and cloud | Medium | Sync `GeneratedAt`; monitor lag alarm |

### 16.2 Business risks

| Risk | Severity | Mitigation |
| ---- | -------- | ---------- |
| Scope creep toward full CRM | High | Enforce profile = analytics only; no master data edit |
| Over-reliance on radar for credit decisions | Medium | Radar is indicative; credit decisions require report evidence |
| Metric disagreement across dashboards | Medium | Single KPI catalog SSOT; reconciliation tests |
| Management expects real-time profiles | Low | Show `GeneratedAt`; set expectation as snapshot product |

### 16.3 Data quality risks

| Risk | Severity | Mitigation |
| ---- | -------- | ---------- |
| Customer key mismatch (code vs name) | Medium | Enforce CustomerCode-first — M17 rule |
| Salesman piutang attribution via name fallback | Medium | Document attribution; prefer `SalesPersonId` |
| Item movement class not reset by retur | Low | Document M19 rule (Faktur-only clock) |
| Missing salesman target distorts achievement | Medium | Show Unknown band — existing SF01 behavior |
| Suspended flag unreliable (M31 note) | Medium | Graceful degradation until Desktop fix |

### 16.4 Maintenance risks

| Risk | Severity | Mitigation |
| ---- | -------- | ---------- |
| Every new dashboard KPI requires profile mapping | Medium | KPI metadata registry + KPI Packs (§5) — auto-map by ID |
| History retention growth | Low | 36-month default; configurable purge |
| Four entity types = four aggregators | Medium | Shared L0–L5 infrastructure + entity plugins |
| Test matrix explosion (4 entities × comparisons) | Medium | Focus tests on reconciliation with source KPIs |
| Metadata registry drift from catalog | Medium | Annual catalog review; CI check for orphan KPI IDs |

---

## 17. Recommendations

### 17.1 Milestone decision

**Entity Analytics should become a new major milestone family** — proposed **M32 Entity Analytics Platform**, with sub-deliveries per entity type. It is the natural **Phase 6** evolution after portal maturity levels 1–5 (reporting → analytics → decision support → forecasting → optimization).

```text
Phase 5 (current)     Optimization dashboards (M28.5, M30, M31)
Phase 6 (proposed)  Entity-centric Performance Profiles + comparison
Phase 6b (parallel) Cloud snapshot platform (separate milestone — §17.2)
```

### 17.2 Cloud Snapshot Synchronization — separate milestone (REVISED)

**Recommendation: Remove cloud sync from M32 Entity Analytics. Create a separate platform milestone.**

| Factor | Rationale |
| ------ | --------- |
| **Scope** | Cloud sync benefits **all** portal features — dashboards, profiles, forecasts, alert center — not only Entity Analytics |
| **Dependency direction** | Entity Analytics **depends on** snapshot infrastructure; cloud sync is **downstream consumption** of that infrastructure |
| **Delivery independence** | On-prem Entity Analytics delivers full value without cloud; cloud can ship when operational pipeline ready |
| **Team ownership** | Analytics product defines *what* to sync (L0–L5 + `BTRPD_*`); platform/ops milestone defines *how* to sync |
| **Risk isolation** | Cloud networking, security, and DR concerns should not block analytics feature delivery |

**Proposed milestone:**

| Milestone | Scope | Depends on |
| --------- | ----- | ---------- |
| **M32 — Entity Analytics Platform** | Profile IA, metadata, L0–L5 layers, Customer/Salesman/Item/Supplier profiles | Existing on-prem worker |
| **M33 — Portal Cloud Snapshot Platform** | Snapshot replication to cloud SQL; cloud read-only API; sync monitoring; `GeneratedAt` lag alerts | M32 L0–L5 stable + existing `BTRPD_*` |

**M33 consumers (non-exhaustive):** Executive dashboard, all domain dashboards, Entity Profiles, Alert Center, forecast dashboards.

**M32 must not include cloud-specific logic** in aggregators — keeps on-prem and cloud workers identical in computation.

### 17.3 Implementation phases (recommended — revised)

| Phase | Scope | Business value | Dependency |
| ----- | ----- | -------------- | ---------- |
| **M32.1 — Platform foundation** | Design principles operationalized; KPI metadata registry; generic L0–L5 snapshot layers; attention + ranking history; relationship rollup engine; profile information architecture shell; entity picker | Enables all entity types — **highest leverage** | None |
| **M32.2 — Customer Performance Profile** | Full customer profile + multi-customer compare + related entities | Highest management demand; richest data (M17/M29/M31) | M32.1 |
| **M32.3 — Salesman Performance Profile** | Extend RepHistory into L1; ranking/attention history; compare; field activity trend (optional) | Sales coaching; lowest incremental cost | M32.1 |
| **M32.4 — Supplier Performance Profile** | Principal-centric cross-domain profile + relationships | Purchasing dependency decisions | M32.1 |
| **M32.5 — Item Performance Profile** | SKU profile with movement + forecast + relationships | Inventory capital decisions | M32.1; highest cardinality |
| **M32.6 — Future entity types** | Warehouse, Wilayah, Category profiles — KPI Pack registration only | Location and assortment analytics | M32.1; reuses M22/IN rollups |
| **M33 — Portal Cloud Snapshot Platform** *(separate)* | Replicate `BTRPD_*` + L0–L5 to cloud; cloud read-only portal | Remote management access | M32.1 stable; not blocking M32.2 |

Phases M32.2 and M32.3 may proceed in parallel after M32.1.

> **Implementation note (2026-06):** Delivery was restructured to **capability-layer-first** on Customer. M32.3 delivered L1 Monthly History + Trend (not Salesman profile). M32.4 delivered L2 Ranking History (not Supplier profile). Entity packs (Salesman, Supplier, Item) moved to M32.9–M32.11. **Authoritative sequence:** [entity-analytics-roadmap-authoritative.md](./entity-analytics-roadmap-authoritative.md). Business requirements in this study are unchanged.

### 17.4 Minimum Viable Product (MVP)

**MVP = M32.1 + M32.2:**

| Include | Exclude (defer) |
| ------- | ---------------- |
| Platform foundation (§1 principles, §5 metadata, §9 profile IA, §12 L0–L5) | Item profile (M32.5) |
| Customer Performance Profile — all §9 sections except Radar (optional) | Supplier profile (M32.4) |
| KPI summary (reuse M17/M31 KPIs via metadata registry) | Salesman profile (M32.3) |
| 12-month omzet + open balance trend (L1) | Radar chart (M32.3+; standard decided in §7.4) |
| Current vs prior month comparison | Ranking history (defer if M32.1 time-constrained) |
| 2-entity customer comparison | Cloud sync (M33) |
| Related entities: Assigned Salesman, Top Items, Top Principals | Retur / collection payment analytics |
| Attention history (basic L3) | Timeline composite section |
| Drill-down to Sales/Piutang Report | Warehouse / Wilayah profiles (M32.6) |

**MVP success criterion:** Management can answer *"How is Customer X doing vs Customer Y over the last year?"* without exporting reports to Excel.

> **Implementation note (2026-06):** The technical MVP was split across capability milestones M32.2–M32.7. L0 profile (M32.2), L1 trend (M32.3), and L2 ranking (M32.4) are complete on Customer. Comparison (M32.7) closes the MVP success criterion. See [entity-analytics-roadmap-authoritative.md](./entity-analytics-roadmap-authoritative.md).

### 17.5 Future evolution roadmap

| Horizon | Capability |
| ------- | ---------- |
| **Near (6–12 mo)** | Salesman + Supplier profiles; radar chart; ranking history; relationship engine |
| **Mid (12–18 mo)** | Item profile; M32.6 Warehouse/Wilayah/Category entity packs |
| **Mid** | M33 cloud-hosted portal with snapshot sync |
| **Long** | Brand, Collector entity types; Timeline composite section |
| **Long** | Custom period comparison (requires parameterized snapshots) |
| **Long** | Retur and collection behavior in customer profile (desktop data integration) |
| **Out of scope** | CRM workflow, master data edit, AI scoring, real-time streaming |

### 17.6 Architect handoff — resolved product decisions

The Architect should **not** re-decide the following — they are authoritative from this analysis:

| # | Product decision |
| - | ---------------- |
| P1 | Eight design principles (§1) govern all Entity Analytics work |
| P2 | Generic eight-category KPI taxonomy (§4) is the common language |
| P3 | KPI metadata registry (§5) — KPIs are registered, not ad hoc per entity |
| P4 | KPI lifecycle rules (§6) — immutable IDs, deprecation over silent change |
| P5 | Radar normalization standard: Peer Group + Percentile with Band fallback (§7.4) |
| P6 | Cross-entity relationships (§8) are required profile sections |
| P7 | Performance Profile information architecture (§9) is shared by all entity types |
| P8 | Navigation model (§10) — Profile sits between Dashboard and Report |
| P9 | Generic L0–L5 snapshot layers (§12) — entity-type extensible |
| P10 | Cloud sync is **M33**, not M32 (§17.2) |
| P11 | MVP = M32.1 + M32.2 Customer Profile |

### 17.7 Architect handoff — open technical questions

The Architect resolves in implementation planning:

1. Physical representation of KPI-ID-keyed L1 history vs columnar tables?
2. Month-close freeze vs continuous monthly upsert (Salesman precedent = upsert)?
3. Item history: all SKUs vs active subset threshold?
4. Radar in MVP or M32.3+ (product allows defer; standard is decided)?
5. RepHistory migration to L1 — timing?
6. Maximum retention period (36 vs 60 months)?
7. Relationship rollup: single generic table vs per-relationship-type?
8. Metadata registry storage: catalog extension file vs database vs code?

---

## Appendix A — Portal KPI Prefix → Entity Mapping

| Prefix | Primary entity | Count (approx.) | Entity Analytics relevance |
| ------ | -------------- | --------------- | -------------------------- |
| CU-KPI | Customer | 52 | **Primary** |
| SF-KPI | Salesman | 18 | **Primary** |
| IN-KPI | Item (company + rankings) | 27 | **Primary** (item-level subset) |
| PU-KPI | Supplier/Principal | 14 | **Primary** |
| FI-KPI | Customer (receivable) | 43 | **Supporting** customer profile |
| SA-KPI | Company / Salesman rankings | 19 | **Supporting** |
| EX-KPI | Company | 29 | **Entry point**, not profile |
| OP-KPI | Warehouse/Wilayah | 12 | **Future** entity pack (M32.6) |

---

## Appendix B — Existing Per-Entity Snapshot Assets

| Table / asset | Entity grain | Usable for profile |
| ------------- | ------------ | ------------------ |
| `BTRPD_CustomerPortfolioCustomer` | Customer | **Yes** — richest row |
| `BTRPD_CustomerAttention` | Customer × signal | **Yes** |
| `BTRPD_CustomerTopOmzet` / `TopPiutang` | Top 10 only | **Partial** |
| `BTRPD_CustomerRiskForecastCustomer` | Top 20 risk | **Partial** |
| `BTRPD_SalesmanRepHistory` | Salesman × month | **Yes** — trend |
| `BTRPD_SalesmanAttention` | Salesman × signal | **Yes** |
| `BTRPD_SalesmanTopOmzet` / `TopAchievement` / `TopPiutang` | Top 10 only | **Partial** |
| `BTRPD_InventoryRiskAttention` | Item × signal | **Yes** |
| `BTRPD_InventoryForecastRisk` | Item | **Partial** |
| `BTRPD_InventoryOptimizationAction` | Item | **Partial** |
| `BTRPD_PurchasingManagementAttention` | Principal × signal | **Yes** |
| `BTRPD_PurchasingTopPrincipal` | Top 10 only | **Partial** |

---

## Appendix C — Investigation and Navigation Model

```text
                    ┌─────────────────────┐
                    │ Executive / Alerts  │  LAYER 1 — Discover
                    └──────────┬──────────┘
                               │
              ┌────────────────┼────────────────┐
              ▼                ▼                ▼
     Domain Dashboard   Entity Profile    Domain Dashboard
     (aggregate)        (deep dive)       (another lens)
              │                │                │
              │         ┌──────┴──────┐         │
              │         ▼             ▼         │
              │    Comparison    Related Entity │
              │    (2–4 peers)   (graph walk)  │
              │         │             │         │
              └────────────────┼────────────────┘
                               ▼
                        Report (evidence)       LAYER 5 — Validate
                               ▼
                        BTR Desktop (action)    LAYER 6 — Act
```

See §10 for full navigation model.

---

## Appendix D — Entity Analytics Platform Component Map

| Component | Section | Reusable across entity types? |
| --------- | ------- | ----------------------------- |
| Design principles | §1 | Yes |
| KPI taxonomy | §4 | Yes |
| KPI metadata registry | §5 | Yes |
| KPI lifecycle rules | §6 | Yes |
| Radar normalization standard | §7.4 | Yes |
| Relationship rollup engine | §8, §12 L4 | Yes |
| Profile information architecture | §9 | Yes |
| Navigation model | §10 | Yes |
| L0–L5 snapshot layers | §11, §12 | Yes |
| Entity Type KPI Pack | §5.4 | Per type — registration only |
| Entity Type Relationship Pack | §8 | Per type — registration only |
| Domain `BTRPD_*` snapshots | Existing | Unchanged |

---

## Appendix E — Document Control

| Version | Date | Author | Change |
| ------- | ---- | ------ | ------ |
| 1.0 | 2026-06-24 | Analyst | Initial feasibility study |
| 2.0 | 2026-06-24 | Analyst | Design principles, generic KPI framework, metadata architecture, KPI lifecycle, radar normalization comparison, cross-entity relationships, profile IA, navigation model, snapshot evolution, milestone restructure (M33 cloud) |
| 2.1 | 2026-06-24 | Analyst | Implementation notes §17.3–§17.4 — capability-layer delivery; roadmap SSOT reference (no requirement change) |

**Next step:** Product Owner approves MVP scope (M32.1 + M32.2). Architect produces `docs/work/btr-portal/implementation-plan-m32-entity-analytics.md` using §17.6 resolved decisions.
