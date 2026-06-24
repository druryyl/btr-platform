# Entity Analytics Platform — Architecture

**Status:** Architect deliverable — long-term platform architecture  
**Audience:** Architects, Implementers, Future Agents  
**Authoritative inputs:** [Entity Analytics Feasibility Study](./Entity-Analytics-Feasibility-Study.md) (Rev 2), [btr-portal-architecture.md](../../features/btr-portal/btr-portal-architecture.md), [materialized-dashboard-domain.md](../../features/materialized-dashboard/materialized-dashboard-domain.md), [btr-portal-kpi-catalog.md](../../features/btr-portal/btr-portal-kpi-catalog.md)  
**Date:** 2026-06-24  
**Milestone family:** M32 (Entity Analytics Platform) · M33 (Portal Cloud Snapshot Platform — separate)

This document describes the **reusable Entity Analytics Platform** — not four independent analytics modules. Customer, Salesman, Supplier, and Item analytics are **consumers** of a common engine.

**Roadmap SSOT:** [entity-analytics-roadmap-authoritative.md](./entity-analytics-roadmap-authoritative.md)

---

## Implementation Status (M32.1–M32.4)

As of M32.4, the following platform capabilities are **implemented** on the Customer entity pack:

| Layer / Engine | SQL table | Engine | Profile section | Milestone |
| -------------- | --------- | ------ | ----------------- | --------- |
| L0 CURRENT | `BTRPD_EntityAnalytics_Current` | Producer | Overview, KPI Summary | M32.2 |
| L1 MONTHLY | `BTRPD_EntityAnalytics_Monthly` | `EntityTrendEngine` | Trend | M32.3 |
| L2 RANKING | `BTRPD_EntityAnalytics_Ranking` | `EntityRankingEngine` | Ranking History | M32.4 |
| L3 ATTENTION | `BTRPD_EntityAnalytics_Attention` | `EntityAttentionEngine` | Attention History | M32.5 |
| L4 RELATIONSHIP | `BTRPD_EntityAnalytics_Relationship` | `EntityRelationshipEngine` | Related Entities | M32.6 |
| Comparison | — | `EntityComparisonEngine` | Comparison (MoM/YoY) | M32.7 |
| L5 RADAR | `BTRPD_EntityAnalytics_Radar` | `EntityRadarEngine` | Radar | M32.8 |

Layer repository interfaces and row models for L3–L5 exist (M32.2R). Only Customer is enabled in production configuration.

---

## 1. Architecture Goals

| Goal | Definition | Why it exists |
| ---- | ---------- | ------------- |
| **Reusable** | One profile information architecture, comparison engine, history model, and relationship engine serve all entity types | Four initial entity types will become ten+; per-type forks multiply maintenance and break learnability |
| **Extensible** | New entity types register KPI Packs and Relationship Packs — they do not fork platform infrastructure | Warehouse, Wilayah, Category, Brand, and Collector are planned; registration must be low-cost |
| **Snapshot-driven** | Profile reads materialized snapshots only — never live transaction scans on HTTP request path | Established by materialized dashboard initiative; required for performance and cloud feasibility |
| **Evidence-based** | Every profile KPI traces to catalog ID and drills to report evidence | Portal maturity model ends at reports; opaque scores destroy management trust |
| **Cross-domain** | A single profile composes KPIs from Sales, Finance, Inventory, and Purchasing when the entity spans domains | Customer health is omzet + piutang; Supplier health is purchase + inventory + sales-out |
| **Historical by default** | Trends, period comparison, ranking history, and attention timelines are platform capabilities | Current-month KPIs alone cannot answer sustainability, seasonality, or intervention impact |
| **Comparison by default** | Entity vs entity, period vs period, and peer context are first-class — not optional add-ons | Management decisions require relative context beyond Top-N rankings |
| **Cloud-friendly** | Analytics payload is pre-aggregated snapshots (&lt; 300 MB total) — not transaction replication | Production DB is large and confidential; cloud portal must remain lightweight |
| **Compatible** | Domain dashboards (M8–M31), Alert Center (M23), and drill-down (M24) remain unchanged; Entity Analytics deepens the journey | Entity Analytics complements aggregate analytics — it does not replace them |
| **High-performance** | Profile load = PK lookups on snapshot tables; heavy computation at worker refresh | Thousands of items and hundreds of customers must not degrade portal response time |

### Anti-goals (explicitly rejected)

| Anti-pattern | Reason |
| ------------ | ------ |
| Per-entity-type profile page with unique section order | Violates Reusable Analytics Engine |
| Live transaction queries per profile load | Violates Snapshot Driven; fails cloud |
| Parallel KPI definitions for profiles vs dashboards | Violates Consistent KPI Semantics |
| Cross-entity-type comparison (Customer vs Item) | Not semantically meaningful |
| CRM-style notes, tasks, or master data edit in profiles | Out of BTR Portal scope |

---

## 2. Platform Architecture

Entity Analytics is a **platform layer** sitting above existing domain snapshot workers and below the Portal Web presentation layer.

```text
┌─────────────────────────────────────────────────────────────────────────┐
│                        Entity Analytics Platform                         │
├─────────────────────────────────────────────────────────────────────────┤
│  Visualization Layer          │  Portal Web — profile shell, widgets   │
├───────────────────────────────┼─────────────────────────────────────────┤
│  Performance Profile Engine   │  Composes sections from snapshot reads  │
├───────────────────────────────┼─────────────────────────────────────────┤
│  KPI Engine                   │  Metadata registry · KPI Packs · envelope│
│  Snapshot Engine              │  L0–L5 layers · refresh orchestration    │
│  Comparison Engine            │  Entity · period · peer · radar overlay  │
│  Trend Engine                 │  Monthly series · growth derivation      │
│  Ranking Engine               │  Current rank · rank history             │
│  Radar Engine                 │  Peer normalization · axis scores (L5)   │
│  Attention Engine             │  Signal persistence · timeline (L3)      │
│  Relationship Engine          │  Top-N rollups · graph navigation (L4)   │
├───────────────────────────────┴─────────────────────────────────────────┤
│  Entity Type Plugins (registration only)                                 │
│    Customer · Salesman · Supplier · Item · (future: Warehouse, …)        │
├─────────────────────────────────────────────────────────────────────────┤
│  Existing Domain Snapshot Workers (unchanged responsibility)               │
│    Customer · Salesman · InventoryRisk · PurchasingManagement · …       │
├─────────────────────────────────────────────────────────────────────────┤
│  BTR Desktop DALs (source of truth at refresh time)                      │
└─────────────────────────────────────────────────────────────────────────┘
```

### Component responsibilities

| Component | Responsibility | Does not |
| --------- | -------------- | -------- |
| **KPI Engine** | KPI metadata registry; entity-type KPI Pack resolution; KPI envelope formatting (unit, direction, period semantics); lifecycle (introduce, deprecate, version) | Compute values — aggregators produce values by KPI ID |
| **Snapshot Engine** | L0–L5 generic storage; refresh hooks; month-close policy; retention; `GeneratedAt` propagation | Replace domain `BTRPD_*` dashboard snapshots |
| **Comparison Engine** | Entity-vs-entity (2–4 same type); cross-period (CURRENT vs prior month vs YoY); average/peer baseline; trend overlay in compare mode | Compare across entity types |
| **Trend Engine** | Read L1 monthly history; organize chronological series; gap handling for missing periods | Compute business KPIs; derive MoM/YoY (Comparison Engine) |
| **Ranking Engine** | Compute rank at refresh for rank-eligible KPIs; store L2 ranking history; expose current rank context on profile | Maintain separate Top-10 dashboard rankings |
| **Radar Engine** | Apply peer-group + percentile normalization (with band fallback); store L5 axis scores at refresh | Render charts — Visualization Layer only |
| **Attention Engine** | Diff attention signals at refresh; upsert L3 event log (FirstSeen, LastSeen, IsActive); expose timeline | Invent new attention rules — signals come from existing domain aggregators |
| **Relationship Engine** | Execute registered relationship rollups at refresh; store L4 Top-N rows; resolve navigation targets | Full graph traversal or exhaustive entity lists |
| **Performance Profile Engine** | Assemble Overview, KPI Summary, Trend, Comparison, Radar, Ranking History, Attention History, Related Entities, Evidence sections from engines | Entity-specific layout or section ordering |
| **Visualization Layer** | Vue profile shell, reusable widgets, entity picker, comparison slot UI, chart components | Business rules or KPI semantics |

### Data flow (read path)

```text
Browser → GET /api/entity-analytics/{type}/{id}/profile
        ↓ MediatR → GetEntityPerformanceProfileQuery
        ↓ IEntityAnalyticsProfileDal
        ↓ parallel reads: L0 CURRENT + L1 history + L2 ranks + L3 attention + L4 relationships + L5 radar
        ↓ EntityPerformanceProfileComposer (Performance Profile Engine)
        ↓ KPI Engine (metadata envelope per KPI ID)
        ↓ EntityPerformanceProfileResponse (section-oriented DTO)
```

### Data flow (write path)

```text
Task Scheduler → btr.portal.worker (--domain Customer|Salesman|…|EntityAnalytics)
        ↓ existing domain aggregator (unchanged formulas)
        ↓ Entity Type Plugin (IEntityAnalyticsProducer)
        ↓ writes L0 CURRENT rows for all entities in scope
        ↓ upserts L1 monthly rows for current period
        ↓ Ranking Engine → L2
        ↓ Attention Engine (signal diff) → L3
        ↓ Relationship Engine → L4
        ↓ Radar Engine → L5
        ↓ BTRPD_EntityAnalytics_* tables (generic layers)
```

**Key rule:** Domain aggregators remain authoritative for KPI **calculation**. Entity Analytics producers **extract and persist** values under KPI IDs — they do not redefine business rules.

---

## 3. Domain Architecture

### Core concepts

| Concept | Business meaning | Platform role |
| ------- | ---------------- | --------------- |
| **Entity** | A selectable business object (Customer, Item, Salesman, Supplier) identified by stable key | Primary analytics subject; all profile data is keyed by `(EntityType, EntityId)` |
| **Entity Type** | Registered kind with KPI Pack, Relationship Pack, peer-group rules, and master DAL | Extensibility unit — new types plug in without platform fork |
| **KPI** | A catalog-defined metric with period semantics, unit, and direction | Value produced at refresh; presented via KPI envelope |
| **KPI Pack** | Declarative subset of KPI IDs exposed on a profile for one entity type | Entity-specific configuration — not entity-specific engine |
| **Snapshot** | Materialized analytics state at a point in time or period | All profile reads come from snapshots |
| **Performance Profile** | Holistic analytics view for one entity — shared section structure across types | Primary user-facing artifact |
| **Comparison** | Relative evaluation — entity vs entity, period vs period, vs peer average | Decision-support primitive |
| **Trend** | Time-series behavior of KPIs over months | Derived from L1 history |
| **Ranking** | Ordinal position within peer population for a metric | Current rank + L2 history |
| **Attention** | Management exception signal attached to an entity | Current state + L3 persistence log |
| **Relationship** | Directed link between entities with a metric (e.g., Customer → Top Items by omzet) | Investigation navigation — not master data |
| **History** | Retained monthly KPI values, ranks, and attention events | Enables temporal reasoning |
| **Evidence** | Report route with entity pre-filter | Terminal validation layer before Desktop action |
| **Peer Group** | Scope for normalization and fair comparison (Wilayah, Category, all active, etc.) | Radar and relative benchmarks |

### Concept interactions

```text
Entity Type
  ├── registers KPI Pack ──────────► KPI Engine
  ├── registers Relationship Pack ─► Relationship Engine
  └── defines Peer Group rules ────► Radar Engine · Comparison Engine

Domain Aggregator (refresh)
  ├── produces KPI values ─────────► Snapshot Engine (L0, L1)
  ├── produces attention signals ──► Attention Engine (L3)
  └── produces relationship inputs ─► Relationship Engine (L4)

Snapshot Engine
  ├── L0 CURRENT ──────────────────► Performance Profile (KPI Summary, Comparison partial)
  ├── L1 MONTHLY ──────────────────► Trend Engine · Comparison Engine
  ├── L2 RANKING ──────────────────► Ranking Engine
  ├── L3 ATTENTION ────────────────► Attention Engine
  ├── L4 RELATIONSHIP ─────────────► Relationship Engine
  └── L5 RADAR ────────────────────► Radar Engine

Performance Profile Engine
  └── composes all sections ───────► Visualization Layer

Evidence
  └── links KPI categories ────────► Reports (live queries, on-prem)
```

---

## 4. Entity Abstraction

### Generic entity model

Every entity type implements a common contract:

| Capability | Common behavior | Entity-specific extension |
| ---------- | --------------- | ------------------------- |
| **Identity** | `EntityType`, `EntityId`, `EntityCode`, `DisplayName`, `IsActive` | Additional dimension fields (Wilayah, Klasifikasi, Category, SupplierId, …) |
| **Master resolution** | Lookup via registered master DAL at refresh | Customer → `ICustomerDal`; Item → item master; Salesman → `ISalesPersonDal`; Supplier → supplier master |
| **KPI production** | Domain aggregator output mapped to KPI IDs | Which KPI IDs apply; which source aggregators run |
| **Attention signals** | Signal codes from existing domain attention lists | Signal catalog per entity type |
| **Relationships** | Top-N rollup pattern with metric KPI ID | Which relationship types are registered |
| **Peer group** | Declared scope for radar and benchmarks | Customer → same Wilayah; Item → same Category; Salesman → all active reps |
| **Evidence map** | Category → report route + filter dimension | Customer → Sales Report + Piutang Report; Item → Inventory Report |
| **Profile route** | `/analytics/{entityTypePlural}/{entityCode}` | Route segment naming only |

### Entity type registration (extensibility pattern)

```text
EntityTypeRegistration
  EntityTypeCode          : Customer | Salesman | Item | Supplier | Warehouse | …
  MasterDalContract       : ICustomerDal | ISalesPersonDal | …
  EntityIdResolver        : CustomerCode-first | SalesPersonId | BrgId | SupplierId
  KpiPackId               : customer-default | salesman-default | …
  RelationshipPackId      : customer-relationships | …
  PeerGroupRuleId         : customer-wilayah | item-category | …
  ProducerType            : IEntityAnalyticsProducer implementation
  WorkerDomainHook        : Customer | Salesman | InventoryRisk | PurchasingManagement
```

**Future entity onboarding** (e.g., Warehouse at M32.12):

1. Register `EntityType = Warehouse` in metadata.
2. Define KPI Pack reusing OP-KPI instances from M22.
3. Define Relationship Pack from §8.3 of feasibility study.
4. Implement `WarehouseEntityAnalyticsProducer` hooking into Location worker refresh.
5. L0–L5 populate automatically — **no new history infrastructure**.

### Common vs entity-specific

| Layer | Shared (100%) | Entity-specific (registration only) |
| ----- | ------------- | ----------------------------------- |
| Profile section structure | Yes | — |
| L0–L5 storage model | Yes | — |
| Comparison / Trend / Radar engines | Yes | — |
| KPI values and packs | Engine | KPI Pack content |
| Relationship blocks | Engine | Relationship Pack content |
| Overview dimensions | Engine | Dimension list |
| Peer group definition | Engine | Rule per type |
| Worker hook point | Pattern | Which domain worker runs producer |

---

## 5. KPI Engine Architecture

### KPI lifecycle

```text
Catalog SSOT (btr-portal-kpi-catalog.md)
        ↓
Entity Analytics KPI Registry (extends with profile metadata)
        ↓
Entity Type KPI Pack (subset per entity type)
        ↓
Aggregator produces value at refresh (by KPI ID)
        ↓
Snapshot Engine persists L0 / L1
        ↓
Profile Engine reads value + metadata → KPI envelope
```

| Stage | Rule |
| ----- | ---- |
| **Introduction** | New immutable catalog ID; registry entry with `IntroducedVersion`; add to KPI Pack(s); aggregator produces at next refresh |
| **Active use** | Profile, comparison, and trend surfaces display when data exists |
| **Deprecation** | Catalog marked deprecated; removed from default KPI Pack; historical values retained in L1 |
| **Semantic change** | **New KPI ID** — never silently redefine existing ID |
| **Backfill** | Administrative job only — logged, Product Owner approved |

### KPI metadata attributes

| Attribute | Purpose |
| --------- | ------- |
| `KpiId` | Immutable link to catalog entry (`CU-KPI-009`) |
| `Category` | Generic taxonomy: Activity, Financial, Growth, Contribution, Portfolio, Quality, Risk, Trend |
| `DisplayName`, `Description` | Management-facing labels from catalog |
| `PeriodSemantics` | `MTD` · `AllTimeOpen` · `PointInTime` · `MonthClosed` |
| `Unit` | `IDR` · `Count` · `Percent` · `Days` · `Band` |
| `Direction` | `HigherIsBetter` · `LowerIsBetter` · `Neutral` |
| `NormalizationRule` | `PeerPercentile` · `AchievementBand` · `None` |
| `VisualizationType` | `Card` · `Trend` · `Comparison` · `RadarAxis` · `Table` |
| `ApplicableEntityTypes` | Which profiles expose this KPI |
| `TrendEligible`, `RankEligible` | History and ranking participation |
| `EvidenceLink` | Report route + filter dimension |
| `IntroducedVersion`, `DeprecatedVersion` | Lifecycle tracking |
| `Supersedes` / `SupersededBy` | Lineage |

### KPI calculation responsibility

| Responsibility | Owner |
| -------------- | ----- |
| Business calculation (SQL, DAL, policy) | Existing domain aggregators — **unchanged** |
| Mapping aggregator output → KPI ID | Entity Type Producer |
| Persisting values | Snapshot Engine |
| Formatting, grouping, comparison presentation | KPI Engine + Profile Engine |
| Normalization for radar | Radar Engine at refresh |

**Rule:** KPI Engine never contains SQL or DAL calls. Computation stays in aggregators per btr-portal-architecture Rule 6.

### KPI normalization

| Context | Method |
| ------- | ------ |
| Radar axes | Peer Group + Percentile (0–100); band midpoint fallback when peer group &lt; 10 and portal band exists |
| Comparison cards | Raw values with unit formatting; growth % derived from L1 |
| Ranking | Descending sort on metric; direction inversion for `LowerIsBetter` before rank assignment |

### KPI categorization

Eight generic categories (feasibility §4.2) group profile KPIs consistently. Not every category applies to every entity type — applicability matrix is authoritative from feasibility study.

### KPI extensibility

Adding a KPI to Customer profile:

1. Ensure catalog entry exists.
2. Add registry metadata row.
3. Add KPI ID to `customer-default` KPI Pack (appropriate section: Summary, Trend, Radar, Rank, Comparison).
4. Extend `DashboardCustomerAggregator` or producer mapping to emit value — **no profile code change**.

---

## 6. Snapshot Architecture

Entity Analytics introduces **generic platform layers L0–L5** alongside existing domain `BTRPD_*` tables. Domain snapshots remain unchanged for aggregate dashboards.

### Layer model

| Layer | Name | Grain | Purpose | Refresh pattern |
| ----- | ---- | ----- | ------- | --------------- |
| **L0** | CURRENT entity metrics | `(EntityType, EntityId)` × KPI IDs | Intra-month MTD and point-in-time profile summary | Upsert each domain refresh; aligns with `SnapshotKey = 'CURRENT'` cadence |
| **L1** | MONTHLY entity history | `(EntityType, EntityId, PeriodYear, PeriodMonth)` × KPI IDs | Trends, closed-month comparison, YoY | Upsert during open month; **freeze** at month close |
| **L2** | RANKING history | `(EntityType, EntityId, Period, RankMetricKpiId)` | Ranking History section | Computed at refresh after L1 for period |
| **L3** | ATTENTION history | `(EntityType, EntityId, SignalCode)` + periods | Attention timeline | Event log — diff signals each refresh |
| **L4** | RELATIONSHIP rollups | `(SourceEntity, TargetEntityType, RelationshipCode, Period)` × TopN | Related Entities section | Materialized at refresh |
| **L5** | RADAR scores | `(EntityType, EntityId, Period)` × axis KPI IDs | Radar section | Precomputed peer-normalized scores |

Domain snapshots (`BTRPD_Customer*`, `BTRPD_Salesman*`, etc.) continue serving CU01, SF01, and other dashboards.

### Snapshot types in detail

#### Current Snapshot (L0)

- Contains all KPI IDs in entity type's KPI Pack for **every entity in scope** — not Top-10 only.
- Source: domain aggregator output at end of worker run.
- Duplication with domain snapshot rows (e.g., customer omzet in `BTRPD_CustomerTopOmzet` and L0) is **acceptable** — profiles need full-entity coverage.
- Mitigation: producer reads in-memory aggregator result and writes L0 in same worker pass — no double DAL load.

#### Historical Snapshot (L1)

- Monthly KPI values keyed by KPI ID — **not** wide column-per-KPI tables.
- Physical model: generic value store `(EntityType, EntityId, PeriodYear, PeriodMonth, KpiId, NumericValue, TextValue, DefinitionVersion)`.
- Extends `BTRPD_SalesmanRepHistory` precedent to all entity types.
- Retention: **36 months** default (configurable).
- Item scope: active subset only — stock &gt; 0 OR sale in last 24 months.

#### Ranking Snapshot (L2)

- Lightweight: rank position + peer count + metric KPI ID per period.
- Computed over entity population eligible for ranking (e.g., active customers MTD).
- Does not duplicate metric values — references L1/L0.

#### Attention Snapshot (L3)

- Append/update event log: `FirstSeenPeriod`, `LastSeenPeriod`, `IsActive`.
- Signals sourced from existing domain attention aggregators — no new signal rules in platform.
- Resolved when signal no longer fires — `IsActive = false`, `LastSeenPeriod` set.

### Generation flow

Implemented worker sequence (M32.4):

```text
1. Domain worker starts (e.g., Customer refresh)
2. Existing aggregator runs → domain BTRPD_* tables (unchanged)
3. EntityAnalyticsProducer runs on same in-memory result
4. Write L0 for all entities in scope
5. EnsurePriorMonthClosed + upsert L1 for current (Year, Month)
6. RankingEngine.ComputeAndPersistRanks() → L2
7. Commit in same transaction as domain snapshot where practical
```

Target sequence when M32.5–M32.8 complete:

```text
8. AttentionEngine.DiffSignals() → L3
9. RelationshipEngine.ComputeRollups() → L4
10. RadarEngine.ComputeScores() → L5
```

L2 runs immediately after L1 because ranking reads the L1 population for the same period. L3 and L4 do not depend on L2 data but follow in the worker pipeline for operational clarity.

**Month-close job** (daily check or first refresh of new month):

- Mark prior month L1 rows `IsClosed = true` — no further upsert.
- Recompute L2 ranks for closed month if needed.

### Synchronization

| Consumer | Reads |
| -------- | ----- |
| Performance Profile API | L0–L5 |
| Domain dashboards | `BTRPD_*` only — unchanged |
| Alert Center | Domain snapshots — unchanged; entity row links added to profiles |
| Executive dashboard | Domain snapshots — unchanged |

Producers **must not** chain snapshot-of-snapshots for primary KPI values — read source DALs or in-memory aggregator output (feasibility principle: Cross-Domain Analytics).

### Historical consistency

| Policy | Rule |
| ------ | ---- |
| Closed months | Immutable unless explicit administrative backfill |
| Open month | Continuous upsert (Salesman RepHistory precedent) |
| KPI introduced mid-history | Gap in trend — not zero |
| Deprecated KPI | Values retained under old ID in L1 |
| Definition version | Stored on L1 row when breaking change uses versioned definition |

### RepHistory migration

| Phase | Approach |
| ----- | -------- |
| Short term | Salesman entity pack (M32.9) may read `BTRPD_SalesmanRepHistory` via adapter for Trend section |
| Medium term | Backfill L1 from RepHistory for salesman KPI IDs |
| Long term | Deprecate RepHistory when L1 parity verified |

---

## 7. Comparison Engine

One reusable engine serves all comparison modes via a **comparison context** object:

```text
ComparisonContext
  Mode            : Entity | MultiEntity | CrossPeriod | Peer | Average | Trend | Radar
  EntityType      : required
  PrimaryEntityId : required
  PeerEntityIds   : 0–3 additional (same type)
  PeriodAnchor    : CURRENT | Month(Year, Month)
  PeerGroupScope  : from entity type registration
  MetricKpiIds    : from Comparison subset of KPI Pack
```

### Comparison modes

| Mode | Input | Output | Engine behavior |
| ---- | ----- | ------ | --------------- |
| **Entity vs Entity** | 2 entities, same type | Side-by-side KPI cards | Load L0 for both; align by KPI ID |
| **Multi-Entity Comparison** | 2–4 entities | Table + shared trend overlay | N × L0 load; max 4 entities |
| **Cross-Period Comparison** | 1 entity, 2–3 periods | MoM, YoY deltas | CURRENT (L0) vs L1 rows; label partial month |
| **Peer Comparison** | 1 entity, peer group | Percentile rank per KPI | Read L5 or compute rank from L0 peer distribution |
| **Average Comparison** | 1 entity vs peer mean | Delta vs average | Mean from L0 over peer group at last refresh |
| **Trend Comparison** | 1–4 entities | Overlay chart series | L1 series per entity; same KPI ID |
| **Radar Comparison** | 2 entities | Overlay on identical axes | L5 scores; same peer group scope |

### Unified engine design

```text
IComparisonEngine.Build(ComparisonContext)
  → resolves data sources by mode
  → KPI Engine formats aligned envelopes
  → returns ComparisonResult (sections vary by mode)
```

Profile **Comparison section** uses Cross-Period mode by default. Separate **Compare page** uses Multi-Entity mode. Radar overlay uses Radar mode.

**Guards:**

- Reject mixed entity types.
- Warn when `GeneratedAt` differs across compared entities.
- Hide radar when peer group &lt; 5 entities.

---

## 8. Relationship Architecture

### Reusable navigation model

Relationships are **registered metadata** — same pattern as KPI Packs:

```text
RelationshipDefinition
  RelationshipCode     : TopItemsByOmzet | TopCustomersByOmzet | AssignedSalesman | …
  SourceEntityTypes    : [Customer, Salesman, …]
  TargetEntityType     : Item | Customer | Salesman | Supplier | …
  MetricKpiId          : CU-KPI-009 (drives sort)
  PeriodSemantics      : MTD | Rolling12Mo
  TopN                 : 10
  ProfileRouteTemplate : /analytics/items/{code}
```

### Generic rollup pattern

```text
TopRelatedEntities(
  SourceEntityType, SourceEntityId,
  TargetEntityType,
  RelationshipCode,
  MetricKpiId,
  Period,
  TopN = 10
)
```

Customer Top Items and Salesman Top Items share the same rollup engine — differing only in source filter.

### Relationship catalog (initial entity types)

| Primary entity | Relationships | Investigation value |
| -------------- | ------------- | ------------------- |
| **Customer** | Assigned Salesman; Top Purchased Items; Top Principals; Wilayah peers (optional) | Accountability; assortment; supply risk |
| **Salesman** | Managed Customers; Top Customers by Omzet/Piutang; Top Principals; Top Items Sold; Wilayah Coverage | Portfolio coaching |
| **Supplier** | Top Items by Inventory Value; Top Items by Sales Velocity; Top Customers; Top Salesmen | Catalog health; demand validation |
| **Item** | Supplier; Category/Brand; Top Customers; Top Salesmen; Warehouse Stock Distribution | Demand and supply context |

### Principles

| Principle | Rule |
| --------- | ---- |
| Top-N only | 5–10 rows per block — not exhaustive |
| Snapshot computed | L4 at refresh — no live joins on profile load |
| Profile link | Each row navigates to target entity's Performance Profile |
| Period alignment | Same period semantics as parent profile KPIs |
| Evidence chain | Related entity → Profile → Report → Desktop |

---

## 9. Performance Profile Architecture

Every entity type shares **identical section structure** (feasibility §9). Entity types differ only in KPI Pack, Relationship Pack, and peer rules.

### Section responsibilities

| Section | Business question | Data source | Always shown? |
| ------- | ----------------- | ----------- | ------------- |
| **Overview** | Who is this entity and what is its status? | Master DAL + L0 dimension fields + `GeneratedAt` | Yes |
| **KPI Summary** | What are the headline facts right now? | L0, grouped by category via KPI Engine | Yes |
| **Comparison** | What changed vs prior periods? | L0 vs L1 — Cross-Period mode | Yes |
| **Trend** | How has this entity behaved over time? | L1 — 12+ months for trend-eligible KPIs | Yes when history exists |
| **Radar** | Is health balanced or lopsided? | L5 — 4–6 axes | When peer group ≥ 5 |
| **Ranking History** | Where did this entity rank over time? | L2 | When rank-eligible KPIs exist |
| **Attention History** | What signals fired and for how long? | L3 | When signals exist |
| **Related Entities** | What entities explain behavior? | L4 — relationship blocks | Yes — ≥1 block |
| **Evidence** | Where is the proof? | InvestigationRegistry + entity filter map | Yes |
| **Timeline** (future) | What happened chronologically? | Composes L2 + L3 + trend inflections | Phase 2 — post-MVP |

### Cognitive flow (information architecture)

```text
Overview → KPI Summary → Comparison → Trend → Radar → Ranking History
  → Attention History → Related Entities → Evidence
```

Comparison before Trend aligns with management question *"what changed?"* before *"what pattern?"*.

### Reusability matrix

| Component | Shared engine | Entity-specific config |
| --------- | ------------- | ---------------------- |
| Section structure | 100% | — |
| KPI Summary content | Engine | KPI Pack |
| Trend metrics | Engine | Trend KPI list |
| Radar axes | Engine | Axis list + peer group |
| Relationship blocks | Engine | Relationship Pack |
| Evidence links | Engine | Report map per type |
| Overview dimensions | Engine | Dimension list |

---

## 10. Cloud Architecture

Cloud snapshot synchronization is **M33 — separate milestone**. Entity Analytics (M32) must not embed cloud-specific logic in aggregators.

### Data classification

| Data class | Location | Sync to cloud? |
| ---------- | -------- | -------------- |
| Transaction tables (`BTR_Faktur`, `BTR_Piutang`, …) | On-prem only | **No** |
| Domain snapshots (`BTRPD_*`) | On-prem authoritative | **Yes** (M33) |
| Entity Analytics L0–L5 | On-prem authoritative | **Yes** (M33) |
| KPI metadata registry | Repo + optional DB cache | **Yes** (static / versioned) |
| Reports (live queries) | On-prem | **No** — cloud cannot serve full evidence |

### Architecture topology

```text
ON-PREMISES (authoritative)
  BTR Desktop + SQL Server
        ↓
  btr.portal.worker → BTRPD_* + BTRPD_EntityAnalytics_*
        ↓
  [M33] Snapshot sync agent
        ↓
CLOUD (read-only replica)
  Cloud SQL (snapshots only)
        ↓
  btr.portal.api (cloud) + btr.portal.web
        ↓
  Management browsers

ON-PREMISES PORTAL (full)
  Same API + web against local SQL
  Reports with live transaction queries available
```

### Cloud profile behavior

| Profile section | Cloud support |
| --------------- | ------------- |
| KPI Summary, Trend, Comparison, Radar, Ranking, Attention, Relationships | **Yes** — snapshot-backed |
| Evidence drill-down to live report | **Limited** — link to on-prem or future synced extract |
| `GeneratedAt` / sync lag | Displayed — cloud is read replica |

**Rule:** On-prem always wins in conflict. Cloud ignores unknown KPI IDs gracefully.

---

## 11. Performance Architecture

### Snapshot generation

| Entity type | Cost driver | Mitigation |
| ----------- | ----------- | ---------- |
| Customer | ~3,000 entities × KPI pack | Extend existing Customer worker; in-memory pass |
| Salesman | ~80 entities | Low — RepHistory already exists |
| Supplier | ~250 principals | Extend PurchasingManagement worker |
| Item | 5,000–20,000 SKUs | **Highest risk** — scope L1 to active subset; phase after Customer/Salesman |

**Target refresh overhead:** &lt; 20% increase on hooked domain worker duration for Customer; Item may require dedicated longer window.

### Query performance

| Operation | Pattern | Expected cost |
| --------- | ------- | ------------- |
| Single profile load | PK seek on L0 + indexed L1 range | &lt; 50 ms DB |
| 12-month trend | 12 × KPI rows per metric | Low |
| 4-entity compare | 4 × profile load | Low |
| Entity search / picker | Master data index | Medium — typeahead on code/name |
| Rank history | 36 rows per metric | Low |

**Anti-pattern:** `FakturView` scan per profile request — rejected.

### Comparison performance

Pre-aligned KPI IDs enable single-query batch loads:

```text
LoadEntityProfileBatch(entityType, entityIds[]) → single round-trip per layer
```

### Trend calculation

`EntityTrendEngine` returns chronological L1 series at profile read time. **MoM %, YoY %, and cross-period deltas** are derived by the **Comparison Engine** (M32.7) from L0/L1 pairs — not stored redundantly unless refresh cost warrants it.

### Ranking generation

Computed once at refresh over full eligible population — not per profile request.

### Cloud synchronization

| Factor | Assessment |
| ------ | ---------- |
| Total analytics payload | &lt; 300 MB |
| Monthly history full refresh | ~170 MB — trivial bandwidth |
| CURRENT delta | &lt; 10 MB per cycle |
| Conflict resolution | On-prem wins |

### Scalability expectations

| Scale | Profile read | Refresh |
| ----- | ------------ | ------- |
| 3k customers | Excellent | Acceptable |
| 80 salesmen | Excellent | Excellent |
| 250 suppliers | Excellent | Excellent |
| 8k active items | Good | Moderate — monitor worker duration |
| 20k items (unscoped) | Good reads | **Poor** refresh — must scope |

### Bottlenecks and mitigations

| Bottleneck | Mitigation |
| ---------- | ---------- |
| Item L1 row volume | Active SKU threshold; category rollup fallback for dormant SKUs |
| Radar peer percentile at refresh | Precompute L5; cache peer distributions per period |
| Dual temporal layers (CURRENT vs monthly) | Clear UI period labels; document in operational guide |
| Producer duplication vs domain snapshots | Single worker pass; shared in-memory aggregate |
| Metadata registry parse at startup | Code-first registry; lazy load per entity type |

---

## 12. Risks

### Technical risks

| Risk | Severity | Mitigation |
| ---- | -------- | ---------- |
| Item cardinality blows refresh time | High | Active subset; defer Item to M32.11; monitor `BTRPD_RefreshLog` duration |
| L0–L5 generic schema query complexity | Medium | Index `(EntityType, EntityId, Period)`; cover KPI ID in INCLUDE |
| Producer/domain aggregator drift | Medium | Reconciliation tests: L0 KPI = domain snapshot for Top-10 overlap |
| RepHistory / L1 dual read period | Low | Adapter pattern; migration checklist |

### Performance risks

| Risk | Severity | Mitigation |
| ---- | -------- | ---------- |
| Batch profile compare N+1 queries | Medium | `LoadEntityProfileBatch` API |
| Radar refresh over large peer groups | Medium | Precompute L5; minimum peer group gate |
| Item relationship rollups (FakturItem joins) | High | Materialize L4 at refresh only; Top 10 cap |

### Maintenance risks

| Risk | Severity | Mitigation |
| ---- | -------- | ---------- |
| KPI catalog / registry drift | Medium | CI check: every KPI Pack ID exists in catalog; annual review |
| Every new dashboard KPI needs profile mapping | Medium | Metadata registry auto-map by ID |
| Test matrix explosion | Medium | Focus reconciliation tests per entity type; shared engine unit tests |

### Data consistency risks

| Risk | Severity | Mitigation |
| ---- | -------- | ---------- |
| Customer key mismatch (code vs name) | Medium | CustomerCode-first — M17 rule |
| Salesman piutang attribution via name fallback | Medium | Prefer `SalesPersonId`; document in operational guide |
| Partial month vs full month comparison | Medium | UI labels; feasibility comparison semantics |
| Month-close race during refresh | Low | Freeze job serializes after last open-month upsert |

### Cloud synchronization risks

| Risk | Severity | Mitigation |
| ---- | -------- | ---------- |
| Cloud without report drill-down feels incomplete | Medium | Label cloud analytics-only; deep link on-prem |
| Snapshot lag | Medium | Sync `GeneratedAt`; lag alarms in M33 |
| Schema version skew | Medium | API sends KPI list dynamically; clients ignore unknown IDs |

### Future extensibility risks

| Risk | Severity | Mitigation |
| ---- | -------- | ---------- |
| Per-entity fork temptation | High | Enforce KPI Pack registration in code review |
| Columnar L1 temptation for "simple" KPIs | Medium | ADR: KPI-ID row model — resist schema explosion |
| Timeline composite scope creep | Low | Defer to post-M32; event model from L2+L3 |

---

## 13. Architecture Decisions (ADR)

### ADR-EA-001: Platform-first, not per-entity modules

| | |
| - | - |
| **Context** | Four entity analytics modules could be built as separate profile implementations (M17-style forks). |
| **Decision** | Build Entity Analytics Platform (M32.1) first; entity types are plugins registering KPI/Relationship Packs. |
| **Alternatives** | Independent Customer/Item/Salesman/Supplier profile stacks. |
| **Consequences** | Higher upfront platform cost; dramatically lower cost per future entity type; shared testing and UX. |

### ADR-EA-002: Generic L0–L5 layers alongside domain snapshots

| | |
| - | - |
| **Context** | Domain `BTRPD_*` tables serve aggregate dashboards with Top-N and CURRENT-only semantics. |
| **Decision** | Introduce `BTRPD_EntityAnalytics_*` generic tables for L0–L5; do not retrofit domain tables for per-entity history. |
| **Alternatives** | Extend `BTRPD_CustomerPortfolioCustomer` pattern per entity; daily snapshots. |
| **Consequences** | Some KPI duplication between domain and L0; clear separation of aggregate vs entity analytics; ~170 MB storage. |

### ADR-EA-003: KPI-ID row model for L1 history

| | |
| - | - |
| **Context** | Monthly history could be wide tables (column per KPI) or generic `(Entity, Period, KpiId, Value)` rows. |
| **Decision** | **KPI-ID keyed row model** for L1 (and L0 CURRENT values). |
| **Alternatives** | Columnar per entity type; document store. |
| **Consequences** | More rows; unlimited KPI extensibility without schema migrations; slightly more complex reads (pivot in DAL). |

### ADR-EA-004: Hybrid temporal model — CURRENT + monthly

| | |
| - | - |
| **Context** | Profiles need intra-month MTD and multi-year trends. |
| **Decision** | L0 CURRENT at domain refresh cadence + L1 monthly history with open-month upsert and month-close freeze. |
| **Alternatives** | Daily snapshots; recalculate from transactions on read. |
| **Consequences** | Two layers to label in UI; matches Salesman RepHistory precedent; economically negligible storage. |

### ADR-EA-005: Domain aggregators remain calculation authority

| | |
| - | - |
| **Context** | Entity Analytics could own new SQL for profile KPIs. |
| **Decision** | Reuse existing aggregators; Entity Producers map outputs to KPI IDs. New calculations extend domain aggregators only when catalog requires it. |
| **Alternatives** | Profile-specific aggregator fork. |
| **Consequences** | Preserves reconciliation with dashboards; may require aggregator extension for new history KPIs. |

### ADR-EA-006: Code-first KPI metadata registry

| | |
| - | - |
| **Context** | KPI metadata could live in database, catalog markdown only, or code registry. |
| **Decision** | **Code-first registry** (`EntityAnalyticsKpiRegistry.cs` + JSON supplement) extending catalog SSOT; catalog remains business definition authority. |
| **Alternatives** | DB-only registry; catalog-only without profile metadata. |
| **Consequences** | Version-controlled metadata; deploy with API; CI validation against catalog IDs. |

### ADR-EA-007: Radar normalization — peer percentile with band fallback

| | |
| - | - |
| **Context** | Radar requires cross-scale normalization (feasibility §7.3). |
| **Decision** | Peer Group + Percentile (0–100); band midpoint when peer group &lt; 10 and portal band exists; omit axis when missing data. |
| **Alternatives** | Min-max; Z-score; pure band. |
| **Consequences** | Robust to skewed distributions; consistent with Achievement and movement-class semantics; scores stored in L5 at refresh. |

### ADR-EA-008: Entity producers hook into existing domain workers

| | |
| - | - |
| **Context** | Entity Analytics could run as a separate `--domain EntityAnalytics` full recompute. |
| **Decision** | Hook `IEntityAnalyticsProducer` into existing Customer, Salesman, InventoryRisk, and PurchasingManagement workers immediately after domain aggregator. |
| **Alternatives** | Standalone worker scanning all entities independently. |
| **Consequences** | No duplicate DAL loads; temporal alignment with domain snapshots; worker duration increases proportionally. |

### ADR-EA-009: Cloud sync is M33, not M32

| | |
| - | - |
| **Context** | Cloud replication could be bundled with Entity Analytics. |
| **Decision** | M32 is on-prem complete; M33 replicates all `BTRPD_*` + L0–L5. |
| **Alternatives** | Cloud-only Entity Analytics; sync inside M32. |
| **Consequences** | M32 delivers value without cloud; sync benefits all portal features uniformly. |

### ADR-EA-010: Profile routes insert between Dashboard and Report

| | |
| - | - |
| **Context** | M24 established Dashboard → Report drill-down. |
| **Decision** | Entity Analytics inserts Profile layer: Dashboard → **Profile** → Report → Desktop. Extend `InvestigationRegistry` with profile routes. |
| **Alternatives** | Profile replaces report; profile only from search. |
| **Consequences** | Every entity row in dashboards/alerts should deep-link to profile; investigation depth increases without changing report semantics. |

### ADR-EA-011: Item history scoped to active SKUs

| | |
| - | - |
| **Context** | Full SKU monthly history at 8k–20k scale is costly. |
| **Decision** | L1 Item history for SKUs with stock &gt; 0 OR sale in last 24 months; dormant SKUs get L0 CURRENT only. |
| **Alternatives** | All SKUs; category-only history. |
| **Consequences** | Mid-tier dormant item profiles show current state without 12-month trend until reactivated. |

### ADR-EA-012: Single generic relationship rollup table (L4)

| | |
| - | - |
| **Context** | Relationships could use per-type tables. |
| **Decision** | One L4 table with `RelationshipCode` discriminator and Top-N rows. |
| **Alternatives** | `BTRPD_CustomerTopItems`, `BTRPD_SalesmanTopItems`, etc. |
| **Consequences** | Unified engine; wider table; index on `(SourceEntityType, SourceEntityId, RelationshipCode)`. |

---

## Related documents

| Document | Role |
| -------- | ---- |
| [entity-analytics-roadmap-authoritative.md](./entity-analytics-roadmap-authoritative.md) | Milestone sequence SSOT |
| [Entity Analytics Feasibility Study](./Entity-Analytics-Feasibility-Study.md) | Approved business requirements |
| [implementation-plan-entity-analytics.md](./implementation-plan-entity-analytics.md) | Implementer roadmap |
| [entity-analytics-developer-guide.md](../../features/entity-analytics/entity-analytics-developer-guide.md) | Permanent implementer knowledge |
| [btr-portal-architecture.md](../../features/btr-portal/btr-portal-architecture.md) | Portal conventions |
| [materialized-dashboard-architecture.md](../../features/materialized-dashboard/materialized-dashboard-architecture.md) | Snapshot worker patterns |
| [btr-portal-kpi-catalog.md](../../features/btr-portal/btr-portal-kpi-catalog.md) | KPI SSOT |

---

## Document control

| Version | Date | Author | Change |
| ------- | ---- | ------ | ------ |
| 1.0 | 2026-06-24 | Architect | Initial platform architecture |
| 1.1 | 2026-06-24 | Architect | M32.1–M32.4 implementation status; generation flow aligned with code; Trend vs Comparison responsibilities; milestone references updated |
