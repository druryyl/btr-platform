# Implementation Plan — Entity Analytics Platform (M32)

## Document Status

| Field | Value |
| ----- | ----- |
| Milestone family | **M32** — Entity Analytics Platform |
| Related milestone | **M33** — Portal Cloud Snapshot Platform (separate — not in this plan's scope) |
| Architecture reference | [entity-analytics-architecture.md](./entity-analytics-architecture.md) |
| Requirements source | [Entity-Analytics-Feasibility-Study.md](./Entity-Analytics-Feasibility-Study.md) (Rev 2 — approved) |
| Portal conventions | [btr-portal-architecture.md](../../features/btr-portal/btr-portal-architecture.md) |
| KPI SSOT | [btr-portal-kpi-catalog.md](../../features/btr-portal/btr-portal-kpi-catalog.md) |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |
| Roadmap SSOT | [entity-analytics-roadmap-authoritative.md](./entity-analytics-roadmap-authoritative.md) |
| Status | **In progress** — M32.1–M32.4 complete; M32.5 next |

**MVP scope (Product approved):** M32.1 Platform Foundation + M32.2 Customer Performance Profile. Technical MVP sections (L1 trend, comparison, attention, relationships) are delivered across capability milestones M32.3–M32.7 — see roadmap SSOT.

**Prerequisites:**

| Prerequisite | Status | Notes |
| ------------ | ------ | ----- |
| M17 Customer Analytics | Complete | Richest customer snapshot foundation |
| M18 Salesman Performance | Required for M32.9 | RepHistory precedent for Salesman entity pack |
| M24 Dashboard Drill Down | Complete | InvestigationRegistry pattern |
| M31 Customer Portfolio | Complete | `BTRPD_CustomerPortfolioCustomer` per-customer row |
| Materialized dashboard cutover | Complete | Snapshot-only read path |

---

## 1. Implementation Strategy

### Approach

Deliver Entity Analytics as an **incremental platform** — never as four parallel feature projects.

**Authoritative sequence:** [entity-analytics-roadmap-authoritative.md](./entity-analytics-roadmap-authoritative.md)

```text
M32.1 Platform Foundation (L0 shell)
    ↓
M32.2 Customer Performance Profile (first entity consumer)
    ↓
M32.3 L1 Monthly History + Trend Engine
    ↓
M32.4 L2 Ranking History + Ranking Engine
    ↓
M32.5 L3 Attention History
    ↓
M32.6 L4 Relationship Engine
    ↓
M32.7 Comparison Engine
    ↓
M32.8 L5 Radar Engine
    ↓
M32.9–M32.12 Entity packs (Salesman, Supplier, Item, future types)
```

Capability layers are proven on **Customer** before additional entity packs. Entity-by-entity sequencing immediately after Customer is **retired**.

### Incremental delivery principles

| Principle | Implementation rule |
| --------- | --------------------- |
| Platform before entities | Prove capability layers on Customer before additional entity packs (M32.9+) |
| Reuse aggregators | Extend producers — do not fork KPI calculations |
| Additive portal changes | New routes and APIs; existing dashboard endpoints unchanged |
| Feature flags per entity type | `EntityAnalytics:EnabledEntityTypes` in appsettings |
| Reconciliation over coverage | Each milestone includes KPI reconciliation tests vs source dashboard |

### What not to do

- Do not embed cloud sync logic in M32 aggregators (M33 owns sync).
- Do not modify `DashboardPiutangAggregator`, `DashboardSalesFakturAggregator`, or `DashboardExecutiveComposer` for entity analytics.
- Do not invent KPIs — catalog extension requires Analyst approval.
- Do not build Item entity pack (M32.11) before platform layers M32.5–M32.8 are proven on Customer.

---

## 2. Platform First

### Phase breakdown

| Phase | Milestone | Scope | Status |
| ----- | --------- | ----- | ------ |
| **1** | M32.1 | Platform shell — L0, registry structure, profile composer, API, Vue shell | **Complete** |
| **2** | M32.2 | Customer Performance Profile — L0 CURRENT, Evidence, dashboard links | **Complete** |
| **2R** | M32.2R | Platform hardening — layer interfaces, developer guide | **Complete** |
| **3** | M32.3 | L1 Monthly History + Trend Engine | **Complete** |
| **4** | M32.4 | L2 Ranking History + Ranking Engine | **Complete** |
| **5** | M32.5 | L3 Attention History + Attention Engine | Planned |
| **6** | M32.6 | L4 Relationship Engine + Related Entities section | Planned |
| **7** | M32.7 | Comparison Engine — cross-period, multi-entity, compare API/page | Planned |
| **8** | M32.8 | L5 Radar Engine + Radar section | Planned |
| **9** | M32.9 | Salesman Entity Pack | Planned |
| **10** | M32.10 | Supplier Entity Pack | Planned |
| **11** | M32.11 | Item Entity Pack (active SKU L1 scope) | Planned |
| **12** | M32.12 | Warehouse, Wilayah, Category, Brand, Collector | Planned |
| **Parallel** | M33 | Cloud snapshot replication | Separate — not blocking M32 |

### MVP definition (M32.1 + M32.2 — business scope)

Original product MVP spans multiple capability milestones. Items below note **delivery milestone**:

| Include (business) | Delivered | Remaining milestone |
| ------------------ | --------- | ------------------- |
| Platform foundation | M32.1 ✓ | — |
| Customer Performance Profile — KPI summary, Evidence | M32.2 ✓ | — |
| 12-month omzet + open balance trend (L1) | M32.3 ✓ | — |
| Ranking history | M32.4 ✓ | — |
| Current vs prior month comparison; 2-entity compare | — | M32.7 |
| Related entities: Assigned Salesman, Top Items, Top Principals | — | M32.6 |
| Attention history (basic L3) | — | M32.5 |
| Drill-down to Sales/Piutang Report | M32.2 ✓ | — |
| Radar chart | — | M32.8 |

| Exclude (defer) | Milestone |
| --------------- | --------- |
| Salesman / Supplier / Item entity packs | M32.9–M32.11 |
| Cloud sync | M33 |
| Timeline composite section | Post-M32 |
| Warehouse / Wilayah profiles | M32.12 |

**MVP success criterion:** Management can answer *"How is Customer X doing vs Customer Y over the last year?"* without exporting reports to Excel.

---

## 3. Component Breakdown

### 3.1 KPI Engine

| Attribute | Value |
| --------- | ----- |
| **Purpose** | KPI metadata registry; KPI Pack resolution; KPI envelope formatting; lifecycle enforcement |
| **Dependencies** | `btr-portal-kpi-catalog.md`; none runtime |
| **Complexity** | Medium |
| **Priority** | P0 — M32.1 |

**Deliverables:**

- `EntityAnalyticsKpiRegistry` (code-first + optional JSON)
- `EntityAnalyticsKpiPack` definitions per entity type
- `KpiEnvelope` DTO (id, category, value, unit, direction, period, evidence link)
- CI test: all KPI Pack IDs exist in catalog

### 3.2 Snapshot Engine (L0–L5)

| Attribute | Value |
| --------- | ----- |
| **Purpose** | Generic storage and DAL for entity analytics layers |
| **Dependencies** | SQL tables; refresh log integration |
| **Complexity** | High |
| **Priority** | P0 — M32.1 |

**Deliverables:**

- `BTRPD_EntityAnalytics_Current` (L0)
- `BTRPD_EntityAnalytics_Monthly` (L1)
- `BTRPD_EntityAnalytics_Ranking` (L2)
- `BTRPD_EntityAnalytics_Attention` (L3)
- `BTRPD_EntityAnalytics_Relationship` (L4)
- `BTRPD_EntityAnalytics_Radar` (L5)
- `IEntityAnalyticsSnapshotDal` read/write contracts
- Month-close freeze job or inline freeze check

### 3.3 Snapshot Generator (Entity Producers)

| Attribute | Value |
| --------- | ----- |
| **Purpose** | Map domain aggregator output → L0–L5 writes per entity type |
| **Dependencies** | Snapshot Engine; domain aggregators |
| **Complexity** | Medium per entity type |
| **Priority** | P0 Customer producer stub in M32.1; full in M32.2 |

**Deliverables:**

- `IEntityAnalyticsProducer` interface
- `CustomerEntityAnalyticsProducer` (M32.2)
- `SalesmanEntityAnalyticsProducer` (M32.9)
- `SupplierEntityAnalyticsProducer` (M32.10)
- `ItemEntityAnalyticsProducer` (M32.11)

### 3.4 Snapshot Scheduler

| Attribute | Value |
| --------- | ----- |
| **Purpose** | Hook producers into existing Task Scheduler domain jobs |
| **Dependencies** | Existing `RefreshDashboard*SnapshotWorker` |
| **Complexity** | Low |
| **Priority** | P0 — M32.1 hooks; entity fill per phase |

**Changes:**

- Extend `RefreshDashboardCustomerSnapshotWorker` — call `CustomerEntityAnalyticsProducer` after M31 step ✓
- Extend `RefreshDashboardSalesmanSnapshotWorker` — M32.9 (Salesman entity pack)
- Extend `RefreshDashboardPurchasingManagementSnapshotWorker` — M32.10 (Supplier entity pack)
- Extend `RefreshDashboardInventoryRiskSnapshotWorker` — M32.11 (Item entity pack)
- Optional: `--domain EntityAnalyticsMonthClose` for L1 freeze

### 3.5 Snapshot Synchronization

| Attribute | Value |
| --------- | ----- |
| **Purpose** | Replicate L0–L5 to cloud |
| **Dependencies** | M33 platform |
| **Complexity** | Medium |
| **Priority** | **Out of M32 scope** — M33 |

M32 tables must be sync-ready (`UpdatedAt`, `LastRefreshLogId`) but contain no cloud-specific code.

### 3.6 Comparison Engine

| Attribute | Value |
| --------- | ----- |
| **Purpose** | Entity, multi-entity, cross-period, peer, average, trend, radar comparison modes |
| **Dependencies** | Snapshot Engine; KPI Engine |
| **Complexity** | Medium |
| **Priority** | P0 — M32.1 shell; M32.7 compare API |

**Deliverables:**

- `EntityComparisonEngine` + `ComparisonContext` model
- `GET /api/entity-analytics/compare` endpoint

### 3.7 Trend Engine

| Attribute | Value |
| --------- | ----- |
| **Purpose** | L1 series retrieval; gap handling |
| **Dependencies** | L1 |
| **Complexity** | Low–Medium |
| **Priority** | P0 — M32.3 ✓ |

### 3.8 Ranking Engine

| Attribute | Value |
| --------- | ----- |
| **Purpose** | Rank computation at refresh; L2 storage; rank history section |
| **Dependencies** | L0/L1; KPI Pack rank-eligible flags |
| **Complexity** | Medium |
| **Priority** | P1 — M32.4 ✓ |

### 3.9 Radar Engine

| Attribute | Value |
| --------- | ----- |
| **Purpose** | Peer percentile normalization; L5 storage |
| **Dependencies** | L0 peer distributions; peer group rules |
| **Complexity** | Medium |
| **Priority** | P2 — M32.8 (standard decided; implementation deferred from MVP) |

### 3.10 Attention Engine

| Attribute | Value |
| --------- | ----- |
| **Purpose** | Signal diff at refresh; L3 event log; attention history section |
| **Dependencies** | Domain attention lists |
| **Complexity** | Medium |
| **Priority** | P0 basic — M32.5 |

### 3.11 Relationship Engine

| Attribute | Value |
| --------- | ----- |
| **Purpose** | Top-N relationship rollups; L4 storage |
| **Dependencies** | Source DALs at refresh (FakturItem, etc.) |
| **Complexity** | Medium |
| **Priority** | P0 — M32.6 Customer relationships |

### 3.12 Performance Profile Engine

| Attribute | Value |
| --------- | ----- |
| **Purpose** | Compose all profile sections from engines |
| **Dependencies** | All engines; KPI Engine |
| **Complexity** | Medium |
| **Priority** | P0 — M32.1 shell; M32.2 Customer content |

**Deliverables:**

- `EntityPerformanceProfileComposer`
- `GetEntityPerformanceProfileQuery` + handler
- Section DTOs per feasibility §9

### 3.13 History (L1 + L2 + L3)

| Attribute | Value |
| --------- | ----- |
| **Purpose** | Monthly KPI retention, ranking history, attention timeline |
| **Dependencies** | Snapshot Engine |
| **Complexity** | Medium |
| **Priority** | L1 P0; L2 P1; L3 P0 |

### 3.14 Attention Timeline

| Attribute | Value |
| --------- | ----- |
| **Purpose** | Attention History section UI + API |
| **Dependencies** | Attention Engine L3 |
| **Complexity** | Low |
| **Priority** | P0 — M32.2 |

### 3.15 Entity Relationship Navigation

| Attribute | Value |
| --------- | ----- |
| **Purpose** | Related Entities blocks; profile-to-profile links |
| **Dependencies** | Relationship Engine L4; router |
| **Complexity** | Medium |
| **Priority** | P0 — M32.2 |

### 3.16 Entity Picker / Search

| Attribute | Value |
| --------- | ----- |
| **Purpose** | Global entity search; comparison slot picker |
| **Dependencies** | Master data APIs |
| **Complexity** | Low |
| **Priority** | P0 — M32.1 |

**Deliverables:**

- `GET /api/entity-analytics/search?type=&q=`
- `EntityPicker.vue` component

### 3.17 Visualization Layer (Portal Web)

| Attribute | Value |
| --------- | ----- |
| **Purpose** | Profile shell, reusable widgets, compare page |
| **Dependencies** | Profile API |
| **Complexity** | Medium |
| **Priority** | P0 shell M32.1; Customer pages M32.2 |

---

## 4. Database Impact

No SQL in this plan — structural impact only.

### 4.1 New structures

| Table | Layer | Grain | Notes |
| ----- | ----- | ----- | ----- |
| `BTRPD_EntityAnalytics_Current` | L0 | Entity × KpiId | Includes `EntityType`, `EntityId`, `EntityCode`, `GeneratedAt`, value columns |
| `BTRPD_EntityAnalytics_Monthly` | L1 | Entity × Period × KpiId | `IsClosed` flag; `DefinitionVersion` nullable |
| `BTRPD_EntityAnalytics_Ranking` | L2 | Entity × Period × RankMetricKpiId | Rank, peer count |
| `BTRPD_EntityAnalytics_Attention` | L3 | Entity × SignalCode | First/last seen periods, `IsActive` |
| `BTRPD_EntityAnalytics_Relationship` | L4 | Source × RelationshipCode × Rank | Target entity ref, metric value |
| `BTRPD_EntityAnalytics_Radar` | L5 | Entity × Period × AxisKpiId | Score 0–100, peer group id |
| `BTRPD_EntityAnalytics_MonthClose` | Meta | Period | Tracks frozen months |

**Indexes (required):**

- L0: `(EntityType, EntityId)` clustered seek
- L1: `(EntityType, EntityId, PeriodYear DESC, PeriodMonth DESC)` + `(EntityType, PeriodYear, PeriodMonth)` for rank pass
- L3: `(EntityType, EntityId, IsActive)`
- L4: `(SourceEntityType, SourceEntityId, RelationshipCode)`

### 4.2 Existing structures — unchanged

All `BTRPD_{Sales,Piutang,Inventory,Purchasing,Customer,Salesman,…}*` tables remain as-is for domain dashboards.

`BTRPD_SalesmanRepHistory` — retained until L1 salesman backfill complete (ADR-EA migration path).

### 4.3 Migration impact

| Area | Impact |
| ---- | ------ |
| New upgrade script | `Scripts/Upgrade_EntityAnalytics_M32.sql` |
| Existing data | No migration of transactional data |
| RepHistory | Optional backfill job M32.9 Salesman pack — not blocking |
| Retention purge | Scheduled job deletes L1/L2 rows older than configured months |

### 4.4 Snapshot storage estimates

Per feasibility §11.4: ~170 MB for 36 months at medium-large distributor scale. Negligible vs transaction DB.

### 4.5 Cloud synchronization (M33 prep)

| Requirement | M32 preparation |
| ----------- | --------------- |
| `UpdatedAt` on all L0–L5 rows | Yes |
| `LastRefreshLogId` linkage | Yes |
| Delta sync key | `(EntityType, EntityId, KpiId, Period)` composite |
| No cloud-only columns | Yes — on-prem/cloud identical schema |

---

## 5. API Impact

### 5.1 New APIs

| Endpoint | Purpose | Milestone |
| -------- | ------- | --------- |
| `GET /api/entity-analytics/types` | List enabled entity types + metadata | M32.1 |
| `GET /api/entity-analytics/search` | Entity picker typeahead | M32.1 |
| `GET /api/entity-analytics/{entityType}/{entityId}/profile` | Full Performance Profile | M32.1 shell; M32.2 Customer |
| `GET /api/entity-analytics/{entityType}/{entityId}/trend` | Trend section (optional dedicated) | M32.2 — may be embedded in profile |
| `GET /api/entity-analytics/compare` | Multi-entity / cross-period compare | M32.2 |
| `GET /api/entity-analytics/{entityType}/{entityId}/relationships` | Related entities (optional — may be in profile) | M32.2 |

**Query parameters (compare):**

```text
GET /api/entity-analytics/compare
  ?entityType=Customer
  &entityIds=C001,C002
  &mode=MultiEntity
  &metricKpiIds=CU-KPI-009,CU-KPI-010
```

### 5.2 Modified APIs — additive links only

| Endpoint | Change |
| -------- | ------ |
| `GET /api/dashboard/customers` | Add `ProfileRoute` on attention list and ranking rows |
| `GET /api/dashboard/salesmen` | Add `ProfileRoute` — M32.9 |
| `GET /api/dashboard/inventory-risk` | Add `ProfileRoute` — M32.11 |
| `GET /api/dashboard/purchasing` | Add `ProfileRoute` on principal rows — M32.10 |
| `GET /api/dashboard/alerts` | Add `ProfileRoute` on entity alerts — M32.2+ |
| `GET /api/dashboard/executive` | Add `ProfileRoute` on Top 5 exposure entities — M32.2 |

**Rule:** Response shapes remain backward compatible — new nullable `ProfileRoute` field only.

### 5.3 Shared APIs

Profile API is shared across all entity types — `entityType` discriminator drives KPI Pack and section content. No per-entity controllers.

### 5.4 Controller structure

```text
btr.portal.api/Controllers/EntityAnalytics/
  EntityAnalyticsController.cs       ← profile, search, types
  EntityAnalyticsCompareController.cs ← compare (or merged)
```

MediatR queries in `btr.application/ReportingContext/EntityAnalyticsAgg/`.

### 5.5 Auth

JWT required — same as dashboard endpoints.

---

## 6. Portal Impact

### 6.1 New pages

| Route | View | Milestone |
| ----- | ---- | --------- |
| `/analytics` | `EntityAnalyticsHomeView` — type selection + search | M32.1 |
| `/analytics/customers/:code` | `CustomerPerformanceProfileView` | M32.2 |
| `/analytics/customers/compare` | `CustomerCompareView` | M32.2 |
| `/analytics/salesmen/:code` | `SalesmanPerformanceProfileView` | M32.9 |
| `/analytics/suppliers/:code` | `SupplierPerformanceProfileView` | M32.10 |
| `/analytics/items/:code` | `ItemPerformanceProfileView` | M32.11 |

### 6.2 Navigation changes

| Location | Change |
| -------- | ------ |
| Main nav | Add **Entity Analytics** entry → `/analytics` |
| Customer Analytics dashboard | Row click → Customer Profile |
| Alert Center | Entity alert → Profile |
| Executive Top 5 | Link → Profile |
| Customer Report (M31) | Row → Profile (before report drill) |

Extend `InvestigationRegistry` with profile routes per signal — pattern from M24.

### 6.3 Shared components (reusable widgets)

| Component | Used by |
| --------- | ------- |
| `EntityPerformanceProfileShell.vue` | All entity profile pages |
| `ProfileOverviewSection.vue` | All |
| `ProfileKpiSummarySection.vue` | All |
| `ProfileComparisonSection.vue` | All |
| `ProfileTrendSection.vue` | All |
| `ProfileRadarSection.vue` | M32.8 |
| `ProfileRankingHistorySection.vue` | M32.4 ✓ |
| `ProfileAttentionHistorySection.vue` | All |
| `ProfileRelatedEntitiesSection.vue` | All |
| `ProfileEvidenceSection.vue` | All |
| `EntityPicker.vue` | Search, compare slot |
| `EntityCompareTable.vue` | Compare page |
| `KpiCard.vue` | Summary, compare |
| `KpiTrendChart.vue` | Trend — reuse Chart.js patterns from dashboards |

### 6.4 Pinia / API layer

```text
stores/entityAnalytics.ts
api/entityAnalyticsApi.ts
types/entityAnalytics.ts
```

### 6.5 Existing pages — no layout breaking changes

Domain dashboards retain current layout. Profile links are additive (row click or explicit link icon).

---

## 7. Cloud Synchronization

**Owned by M33 — not implemented in M32.**

M32 deliverables for M33 readiness:

| Topic | M32 responsibility |
| ----- | ------------------ |
| Snapshot upload | Design tables with `UpdatedAt`; no upload code |
| Snapshot versioning | `DefinitionVersion` on L1; KPI registry version in API meta |
| Incremental sync | Primary keys documented in architecture |
| Offline behavior | On-prem portal fully functional without cloud |
| Sync scheduling | M33 Task Scheduler / sync agent |
| Failure recovery | `BTRPD_RefreshLog` linkage; M33 retry policy |

**Cloud profile behavior (M33):**

- Analytics sections served from cloud replica.
- Evidence links route to on-prem portal URL (configurable `Portal:OnPremBaseUrl`) or show "available on local portal" message.

---

## 8. Milestone Planning

**Authoritative sequence:** [entity-analytics-roadmap-authoritative.md](./entity-analytics-roadmap-authoritative.md)

### M32.1 — Platform Foundation ✓ Complete

| Field | Content |
| ----- | ------- |
| **Objectives** | Operationalize feasibility principles; deliver reusable platform shell |
| **Delivered** | L0 table + repository; KPI registry structure; profile composer (empty-safe sections); producer orchestrator; `GET types` + `GET profile`; Vue shell |
| **Deferred from original plan** | L1–L5 SQL; analytic engines; search/compare APIs |
| **Summary** | [m32.1-implementation-summary.md](./m32.1-implementation-summary.md) |

---

### M32.2 — Customer Performance Profile ✓ Complete

| Field | Content |
| ----- | ------- |
| **Objectives** | First entity consumer — L0 CURRENT for all customers |
| **Delivered** | `CustomerEntityAnalyticsProducer`; `customer-default` pack; Overview, KPI Summary, Evidence; dashboard `ProfileRoute` links |
| **Deferred** | L1–L5 profile sections; compare page; search API |
| **Summary** | [m32.2-implementation-summary.md](./m32.2-implementation-summary.md) |

**KPI Pack (customer-default):**

| Category | KPI IDs (catalog) |
| -------- | ----------------- |
| Financial | CU-KPI-009, CU-KPI-010 |
| Risk | FI-KPI-013 |

**Producer hook:**

```text
RefreshDashboardCustomerSnapshotWorker
  → (existing M17 → M29 → M30 → M31 chain)
  → CustomerEntityAnalyticsProducer.Produce(aggregateContext)
  → L0 write (M32.2); L0+L1+L2 (M32.3–M32.4)
```

---

### M32.3 — Monthly History + Trend (L1) ✓ Complete

| Field | Content |
| ----- | ------- |
| **Objectives** | Generic L1 monthly snapshots; Trend Engine; month-close policy |
| **Delivered** | `BTRPD_EntityAnalytics_Monthly`, `_MonthClose`; `EntityTrendEngine`; Trend profile section |
| **Note** | MoM/YoY deferred to M32.7 Comparison Engine |
| **Summary** | [m32.3-implementation-summary.md](./m32.3-implementation-summary.md) |

---

### M32.4 — Ranking History (L2) ✓ Complete

| Field | Content |
| ----- | ------- |
| **Objectives** | Generic L2 ranking snapshots; Ranking Engine at refresh |
| **Delivered** | `BTRPD_EntityAnalytics_Ranking`; `EntityRankingEngine`; Ranking profile section |
| **Summary** | [m32.4-implementation-summary.md](./m32.4-implementation-summary.md) |

---

### M32.5 — Attention History (L3)

| Field | Content |
| ----- | ------- |
| **Objectives** | Signal diff at refresh; L3 event log; Attention History profile section |
| **Features** | `BTRPD_EntityAnalytics_Attention` SQL; `EntityAttentionEngine`; Customer M17 signal mapping; `ProfileAttentionHistorySection.vue` |
| **Dependencies** | M32.2 Customer producer; M17 attention signals |
| **Acceptance criteria** | Attention timeline shows first-seen/last-seen/active; signals reconcile to CU01 attention list; closed-month policy respected |
| **Deliverables** | Attention engine; L3 repository; producer hook after L2; unit tests for signal diff |
| **Risk** | Low–Medium |

---

### M32.6 — Relationship Engine (L4)

| Field | Content |
| ----- | ------- |
| **Objectives** | Top-N relationship rollups at refresh; Related Entities profile section |
| **Features** | `BTRPD_EntityAnalytics_Relationship` SQL; `EntityRelationshipEngine`; Customer relationships (salesman, items, principals); profile-to-profile navigation |
| **Dependencies** | M32.2 producer; M31/M17 domain data |
| **Acceptance criteria** | Top 10 rows per relationship block; profile links navigate; same period semantics as parent KPIs |
| **Deliverables** | Relationship engine; L4 repository; relationship pack registration; `ProfileRelatedEntitiesSection.vue` |
| **Risk** | Medium |

---

### M32.7 — Comparison Engine

| Field | Content |
| ----- | ------- |
| **Objectives** | Cross-period (CURRENT vs prior month vs YoY) and multi-entity comparison; compare API/page |
| **Features** | `EntityComparisonEngine` + `ComparisonContext`; `GET /api/entity-analytics/compare`; Profile Comparison section; `CustomerCompareView.vue`; MoM/YoY derivation; `GET /api/entity-analytics/search` |
| **Dependencies** | L0 + L1 (required); L5 optional for Peer/Radar modes |
| **Acceptance criteria** | Cross-period deltas for CU-KPI-009/010; 2–4 entity side-by-side; type guard rejects mixed entity types; MVP success criterion met |
| **Deliverables** | Comparison engine; compare controller; compare page; unit tests for guards and delta math |
| **Risk** | Low–Medium |

---

### M32.8 — Radar Engine (L5)

| Field | Content |
| ----- | ------- |
| **Objectives** | Peer-percentile normalized axis scores at refresh; Radar profile section |
| **Features** | `BTRPD_EntityAnalytics_Radar` SQL; `EntityRadarEngine`; Customer radar axes (4–6); band fallback when peer group &lt; 10 |
| **Dependencies** | L0 peer distributions; peer group rules (ADR-EA-007) |
| **Acceptance criteria** | Radar displays when peer group ≥ 5; scores 0–100; axis omitted when data missing |
| **Deliverables** | Radar engine; L5 repository; `ProfileRadarSection.vue` |
| **Risk** | Medium |

---

### M32.9 — Salesman Entity Pack

| Field | Content |
| ----- | ------- |
| **Objectives** | Second entity type adopting full L0→L5 pipeline |
| **Features** | `SalesmanEntityAnalyticsRegistrar/Producer/EvidenceResolver`; RepHistory → L1 adapter/backfill; SF01 profile links |
| **Dependencies** | M32.5–M32.8 platform layers (or partial enable); M18; `BTRPD_SalesmanRepHistory` |
| **Acceptance criteria** | Trend matches SF01 rep history; rank history for omzet metric; profile links from SF01 |
| **Deliverables** | Salesman producer; worker hook; reconciliation tests |
| **Risk** | Low |

---

### M32.10 — Supplier Entity Pack

| Field | Content |
| ----- | ------- |
| **Objectives** | Principal-centric cross-domain profile |
| **Features** | `SupplierEntityAnalyticsProducer` on PurchasingManagement worker; purchase + inventory + sales-out KPIs; L4 relationships |
| **Dependencies** | M32.9 pattern proven; M21; M15/M19 cross-reads |
| **Acceptance criteria** | MTD purchase reconciles PU01; inventory value reconciles M15 supplier breakdown; relationships navigate |
| **Deliverables** | Supplier producer; PU01 profile links |
| **Risk** | Medium — cross-domain joins |

---

### M32.11 — Item Entity Pack

| Field | Content |
| ----- | ------- |
| **Objectives** | SKU profile with movement, forecast, relationships |
| **Features** | `ItemEntityAnalyticsProducer` on InventoryRisk worker; active SKU L1 scope (ADR-EA-011); L4 customers/supplier/warehouses |
| **Dependencies** | M32.8 platform complete; M19; M28 |
| **Acceptance criteria** | Movement class reconciles IN02; forecast KPIs from M28; refresh duration within agreed SLA; inactive SKU gets L0 only |
| **Deliverables** | Item producer; IN02 profile links |
| **Risk** | **High** — cardinality |

---

### M32.12 — Future Entity Types

| Field | Content |
| ----- | ------- |
| **Objectives** | Register Warehouse, Wilayah, Category entity packs |
| **Features** | KPI Pack + Relationship Pack + producer hook on Location worker (M22) |
| **Dependencies** | M32.1 platform; M22 |
| **Acceptance criteria** | New entity type added without schema change to L0–L5 |
| **Deliverables** | Registration files only per entity |
| **Risk** | Low per type |

---

## 9. Risk Assessment

| Risk | Category | Severity | Mitigation |
| ---- | -------- | -------- | ---------- |
| Four independent profile implementations | Maintenance | High | Enforce platform-first gate in M32.1; code review checklist |
| Item refresh timeout | Performance | High | Active SKU scope; separate scheduler window; monitor RefreshLog |
| KPI semantic drift profile vs dashboard | Data consistency | Medium | Reconciliation tests; single KPI ID end-to-end |
| L1 generic table read performance | Performance | Medium | Indexes; DAL pivot caching in handler |
| Scope creep — CRM features | Business | High | Profile = analytics only; reject master data edit |
| Breaking dashboard APIs | Breaking change | Medium | Additive `ProfileRoute` only; no DTO removals |
| RepHistory / L1 duplication | Maintenance | Low | Adapter + migration plan in M32.9 Salesman pack |
| Cloud expectation mismatch | Cloud | Medium | M33 separate; label evidence limitations |
| Test matrix size | Testing | Medium | Shared engine tests + per-entity reconciliation samples |
| Worker transaction size | Operational | Medium | Batch L0 writes; configurable batch size |

### Testing strategy

| Layer | Focus |
| ----- | ----- |
| Unit | Attention diff, comparison guards, trend derivation, radar percentile |
| Integration | Producer → L0–L5 round-trip |
| Reconciliation | Profile KPI = dashboard KPI for shared entities (sample set) |
| E2E | Dashboard row → Profile → Report drill (Customer path) |

---

## 10. Handover Plan

### 10.1 Implementer reading order

1. [Entity-Analytics-Feasibility-Study.md](./Entity-Analytics-Feasibility-Study.md) — approved business rules (do not redesign)
2. [entity-analytics-roadmap-authoritative.md](./entity-analytics-roadmap-authoritative.md) — **milestone sequence SSOT**
3. [entity-analytics-architecture.md](./entity-analytics-architecture.md) — platform design and ADRs
4. This document — execution detail
5. [entity-analytics-developer-guide.md](../../features/entity-analytics/entity-analytics-developer-guide.md) — implementer patterns
6. [btr-portal-architecture.md](../../features/btr-portal/btr-portal-architecture.md) — portal conventions

### 10.2 Module map for Implementer

```text
btr.application/ReportingContext/EntityAnalyticsAgg/
├── Contracts/
│   ├── IEntityAnalyticsSnapshotDal.cs
│   ├── IEntityAnalyticsProfileDal.cs
│   └── IEntityAnalyticsProducer.cs
├── Models/
│   ├── EntityPerformanceProfileResponse.cs
│   ├── KpiEnvelope.cs
│   ├── ComparisonContext.cs
│   └── EntityTypeRegistration.cs
├── Services/
│   ├── EntityPerformanceProfileComposer.cs
│   ├── EntityComparisonEngine.cs
│   ├── EntityTrendEngine.cs
│   ├── EntityRankingEngine.cs
│   ├── EntityRadarEngine.cs
│   ├── EntityAttentionEngine.cs
│   ├── EntityRelationshipEngine.cs
│   ├── EntityAnalyticsKpiRegistry.cs
│   └── Producers/
│       ├── CustomerEntityAnalyticsProducer.cs
│       ├── SalesmanEntityAnalyticsProducer.cs
│       ├── SupplierEntityAnalyticsProducer.cs
│       └── ItemEntityAnalyticsProducer.cs
├── Queries/
│   ├── GetEntityPerformanceProfileQuery.cs
│   ├── GetEntityAnalyticsSearchQuery.cs
│   └── CompareEntitiesQuery.cs
└── Packs/
    ├── customer-default-kpi-pack.json
    ├── customer-relationship-pack.json
    └── (per entity type)

btr.infrastructure/ReportingContext/EntityAnalyticsAgg/
└── EntityAnalyticsSnapshotDal.cs

btr.portal.api/Controllers/EntityAnalytics/
└── EntityAnalyticsController.cs

btr.portal.web/src/
├── views/analytics/
├── components/analytics/
├── api/entityAnalyticsApi.ts
└── stores/entityAnalytics.ts

btr.sql/Tables/ReportingContext/
└── BTRPD_EntityAnalytics_*.sql
```

### 10.3 M32.1 implementation sequence — completed (revised scope)

| Step | Task | Status |
| ---- | ---- | ------ |
| 1 | SQL: L0 table + indexes | ✓ Done |
| 2 | `EntityAnalyticsKpiRegistry` structure | ✓ Done |
| 3 | `EntityAnalyticsRepository` — L0 read/write | ✓ Done |
| 4 | `EntityPerformanceProfileComposer` — empty-safe sections | ✓ Done |
| 5 | `IEntityAnalyticsProducer` + orchestrator | ✓ Done |
| 6 | API: types + profile | ✓ Done |
| 7 | `EntityPerformanceProfileShell.vue` + routes | ✓ Done |
| 8 | L1–L5 SQL + engines | Deferred → M32.3–M32.8 |
| 9 | Search/compare APIs | Deferred → M32.7 |
| 10 | Attention/Comparison engines | Deferred → M32.5, M32.7 |

### 10.4 M32.2 implementation sequence — completed (revised scope)

| Step | Task | Status |
| ---- | ---- | ------ |
| 1 | `CustomerEntityAnalyticsProducer` — L0 all customers | ✓ Done |
| 2 | Worker hook in Customer transaction | ✓ Done |
| 3 | Overview, KPI Summary, Evidence sections | ✓ Done |
| 4 | `ProfileRoute` on dashboard rows | ✓ Done |
| 5 | Reconciliation tests | ✓ Done |
| 6 | L1 monthly history | ✓ M32.3 |
| 7 | L3 attention | → M32.5 |
| 8 | L4 relationships | → M32.6 |
| 9 | Compare page | → M32.7 |

### 10.5 Resolved technical questions (from feasibility §17.7)

| # | Question | Decision |
| - | -------- | -------- |
| 1 | L1 physical model | KPI-ID row model (`BTRPD_EntityAnalytics_Monthly`) — ADR-EA-003 |
| 2 | Month-close vs upsert | Hybrid: upsert during open month; freeze at close — ADR-EA-004 |
| 3 | Item history scope | Active SKUs only (stock &gt; 0 OR sale in 24 mo) — ADR-EA-011 |
| 4 | Radar in MVP? | **Defer** to M32.8; normalization standard already decided (ADR-EA-007) |
| 5 | RepHistory migration | Adapter in M32.9 Salesman pack; backfill then deprecate |
| 6 | Retention | 36 months default; `EntityAnalytics:HistoryRetentionMonths` config |
| 7 | Relationship storage | Single L4 generic table — ADR-EA-012 |
| 8 | Metadata registry | Code-first `EntityAnalyticsKpiRegistry` — ADR-EA-006 |

### 10.6 Definition of done (per milestone)

- [ ] Code merged with unit + reconciliation tests
- [ ] SQL upgrade script in repo
- [ ] `BTRPD_RefreshLog` shows successful worker run
- [ ] Portal routes documented in `btr-portal-operational.md` (Implementer updates)
- [ ] No changes to forbidden modules (Piutang aggregator, Sales Faktur aggregator, Executive composer formulas)
- [ ] KPI Pack IDs traceable to catalog

### 10.7 Post-M32 knowledge curation

When M32.2 MVP ships, Analyst/Implementer should:

1. Add Entity Analytics section to `docs/features/btr-portal/btr-portal-domain.md` (Performance Profile concepts).
2. Extend `btr-portal-architecture.md` with Entity Analytics module map.
3. Archive or fold key content from this work folder into permanent knowledge.
4. Keep `entity-analytics-architecture.md` as long-term ADR reference.

---

## Document control

| Version | Date | Author | Change |
| ------- | ---- | ------ | ------ |
| 1.0 | 2026-06-24 | Architect | Initial implementation plan for M32 family |
| 2.0 | 2026-06-24 | Architect | Capability-layer roadmap; M32.1–M32.4 complete; SSOT reference; entity-pack milestones M32.9–M32.12 |
