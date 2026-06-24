# Entity Analytics Architecture & Roadmap Alignment Review

Review of all listed artifacts plus the implemented codebase (`EntityAnalyticsAgg`, SQL tables, API, portal shell). **No code or documentation was modified** — this report is the recommended authoritative input for the next documentation pass.

---

## Part 1 — Architecture Conformance Review

### Summary verdict

The **platform architecture holds**. M32.1–M32.4 delivered a reusable, entity-agnostic foundation with Customer as the proving consumer. Drift is concentrated in **milestone sequencing** and **deferred MVP scope**, not in structural design.

| Area | Status | Classification |
|------|--------|----------------|
| Entity abstraction | `IEntityTypeRegistry`, `IEntityAnalyticsRegistrar`, Scrutor producer discovery, `CustomerCode`-first identity | **Conforms** |
| KPI Registry | Code-first `EntityAnalyticsKpiRegistry`, `customer-default` pack, `ValidatePack()`, full metadata on 3 KPIs | **Conforms** |
| Producer architecture | In-memory `DomainInput`, orchestrator hook in Customer worker, same `TransHelper` scope | **Conforms** |
| Snapshot architecture | L0/L1/L2 SQL + repository implemented; L3–L5 row models + read stubs only | **Acceptable evolution** |
| Trend architecture | `EntityTrendEngine` reads L1; no MoM/YoY at read time | **Minor deviation** (intentional) |
| Ranking architecture | `EntityRankingEngine` computes L2 from L1 population at refresh | **Conforms** |
| Profile architecture | Shared section shell; Overview/KPI/Trend/Ranking/Evidence active; 4 sections placeholder | **Minor deviation** (MVP split) |
| Comparison / Attention / Relationship / Radar engines | **Not implemented** — DTO placeholders only | **Expected gap** |

### Detailed assessment

**Entity abstraction — conforms**

The M32.1 verification refactor (`IEntityTypeRegistry`, registrar bootstrap) matches ADR-EA-001. Customer extends via registrar + producer + evidence resolver only; composer and API are generic.

**KPI Registry — conforms**

Registry is metadata-driven with no entity branching. `CustomerEntityAnalyticsRegistrar` owns pack content. Matches ADR-EA-006.

**Producer architecture — conforms**

`CustomerEntityAnalyticsProducer` maps M31 portfolio rows to KPI IDs without DAL calls. Worker hook pattern matches ADR-EA-005 and ADR-EA-008.

**Snapshot architecture — acceptable evolution**

Architecture and feasibility assumed L0–L5 tables in M32.1. Implementation correctly phased:

- **Implemented:** `BTRPD_EntityAnalytics_Current`, `_Monthly`, `_MonthClose`, `_Ranking`
- **Not yet:** `_Attention`, `_Relationship`, `_Radar` SQL

Layer interfaces (`IEntityAnalyticsLayerRepositories`) were front-loaded in M32.2R — good seam preservation.

**Trend — minor deviation**

Architecture §2 assigns MoM/YoY derivation to Trend Engine. M32.3 deliberately scoped Trend to **series retrieval only** and deferred deltas to Comparison. This is sound separation; architecture text should be updated to match.

**Ranking — conforms**

Competition ranking over L1 values, `RankEligible` from registry, active-entity filter via L0 meta. Matches architecture §6.

**Profile — minor deviation**

Feasibility §9 and architecture §9 define Comparison, Attention, and Related Entities as core sections. M32.2 delivered L0 + Evidence only; history layers followed in M32.3–M32.4. MVP was intentionally split across capability milestones.

**Not yet built (architectural role unchanged)**

| Engine | Planned role | Code status |
|--------|--------------|---------------|
| Comparison | Read-time cross-period / multi-entity / peer overlay | `CreatePlaceholderSection` in composer |
| Attention | L3 signal diff at refresh | Stub `IEntityAnalyticsAttentionRepository` |
| Relationship | L4 Top-N rollups at refresh | Stub `IEntityAnalyticsRelationshipRepository` |
| Radar | L5 peer-normalized scores at refresh | Stub `IEntityAnalyticsRadarRepository` |

Also missing from original plan: `GET /api/entity-analytics/search`, `GET /api/entity-analytics/compare`, compare page UI.

### Platform vision checklist

| Principle | Assessment |
|-----------|------------|
| Reusable engine approach | **Yes** — Trend and Ranking are entity-agnostic |
| Entity-agnostic design | **Yes** — only Customer producer is live |
| Snapshot-driven reads | **Yes** — profile never scans transactions |
| Domain aggregators as calculation authority | **Yes** |
| Cloud sync separate (M33) | **Yes** — no cloud logic in M32 code |

### Architectural drift to flag

1. **Milestone numbering repurposed** — M32.3/M32.4 are capability layers, not Salesman/Supplier profiles (**acceptable evolution**).
2. **M32.4 summary contradicts layer numbering** — assigns Comparison to M32.5 while repository comments assign L3 to M32.5+ (**documentation drift**).
3. **Architecture generation-flow inconsistency** — §2 write path orders Ranking before Attention; §6 step list orders Attention/Relationship before Ranking (**minor documentation bug**).
4. **MVP scope incomplete** — original M32.1+M32.2 MVP included comparison, attention, relationships; still open after M32.4.

---

## Part 2 — Milestone Review

### Original roadmap (Feasibility §17.3 + Implementation Plan §1)

```text
M32.1  Platform Foundation
M32.2  Customer Performance Profile
M32.3  Salesman Performance Profile
M32.4  Supplier Performance Profile
M32.5  Item Performance Profile
M32.6  Future entity types (Warehouse, Wilayah, …)
M33    Cloud Snapshot Platform
```

Within M32.1, architecture assumed **all engines + L0–L5** in one milestone.

### What actually shipped

| Milestone | Original intent | Actual delivery |
|-----------|-----------------|-----------------|
| **M32.1** | Full platform + all layers | L0 only, shell, registry, composer placeholders |
| **M32.2** | Full Customer MVP (L0–L5 sections, compare, relationships, attention) | Customer L0 profile + Evidence + dashboard links |
| **M32.2R** | — | Hardening: layer interfaces, developer guide, API contract |
| **M32.3** | Salesman profile | **L1 Monthly History + Trend Engine** (Customer) |
| **M32.4** | Supplier profile | **L2 Ranking History + Ranking Engine** (Customer) |

### Why it changed

1. **Platform-first validation** — prove each snapshot layer on Customer before second entity type.
2. **M32.1 scope realism** — delivering all engines in one sprint was infeasible; seams were preserved.
3. **Incremental risk** — Item/Supplier cardinality deferred until generic L1/L2 pipeline is proven.
4. **RepHistory precedent** — Salesman benefits from completed L1 infrastructure, not the reverse.

### Does the change improve architecture?

**Yes — adopt permanently.** The pattern should be:

```text
M32.x  Platform capability layers (L1 → L2 → L3 → L4 → L5 + Comparison)
M32.y  Entity pack adoption (Salesman, Supplier, Item, …)
```

Entity packs become **thin registration + producer** work once layers exist, matching ADR-EA-001 and the developer guide extension model.

### Internal documentation conflict (must resolve)

| Source | M32.5 | M32.6 | M32.7 | M32.8 |
|--------|-------|-------|-------|-------|
| m32.2 summary | Attention L3 | Relationship L4 | Radar L5 | — |
| m32.2R summary | Attention L3 | Relationship L4 | Radar L5 | Comparison |
| m32.4 summary | **Comparison** | Attention L3 | Relationship L4 | Radar L5 |
| `IEntityAnalyticsLayerRepositories` comments | L3 M32.5+ | L4 M32.6+ | L5 M32.7+ | — |
| User / layer model | Attention L3 | Relationship L4 | Radar L5 | Comparison |

**Authoritative resolution:** Milestone numbers track **L-layer completion order** for snapshot layers; Comparison is a **read engine** assigned after L3+L4, before L5.

---

## Part 3 — Comparison Engine Placement Review

Comparison is **not an L-layer**. It composes existing snapshots at read time. Milestone order is a **delivery priority** decision, not a write-path dependency.

### Option A — Trend → Ranking → **Comparison** → Attention → Relationship → Radar

| Factor | Assessment |
|--------|------------|
| Dependencies | Cross-Period needs L0+L1 (**ready**). Multi-Entity needs L0 (**ready**). Peer/Radar modes need L5 (**not ready**). |
| User value | **High early** — closes MVP gap ("what changed vs last month?"); MoM/YoY explicitly deferred from Trend |
| Write path | Leaves L3/L4 worker pipeline incomplete |
| Maintainability | Comparison ships with partial modes; Peer/Radar modes added when L5 exists |
| Architecture IA | Profile cognitive flow puts Comparison **before** Trend — early delivery aligns with UX |

### Option B — Trend → Ranking → Attention → Relationship → Radar → **Comparison**

| Factor | Assessment |
|--------|------------|
| Dependencies | All snapshot layers exist before Comparison ships |
| User value | **Delayed** — profile lacks period deltas and compare page longer |
| Write path | **Complete** before any new read engine |
| Maintainability | Comparison implemented once with all data sources |
| Architecture IA | Conflicts with "Comparison before Trend" presentation order |

### Recommendation — **neither option exactly**

Adopt a **hybrid sequence**:

```text
Trend (L1) → Ranking (L2) → Attention (L3) → Relationship (L4) → Comparison → Radar (L5)
```

**Rationale:**

1. **L3 + L4 next** — complete the worker write pipeline in layer order; both were original MVP scope; repository seams already label L3=M32.5+, L4=M32.6+.
2. **Comparison before Radar** — Cross-Period and Multi-Entity modes need only L0+L1; delivers MVP success criterion without waiting for peer normalization. Peer/Radar comparison modes ship incrementally when L5 arrives.
3. **Radar last among engines** — highest complexity (peer groups, percentile, band fallback); P2 in original plan; optional profile section when peer group &lt; 5.

Comparison **before** Attention (Option A) optimizes UX but leaves snapshot layers 3–4 unimplemented in the worker — inconsistent with the layer-first strategy that made M32.3/M32.4 successful.

Comparison **after** Radar (Option B) over-constrains delivery; Cross-Period does not require L5.

---

## Part 4 — Future Layer Dependency Map

### Snapshot write path (platform infrastructure)

```text
Domain Aggregator (unchanged)
        ↓
IEntityAnalyticsProducer
        ↓
L0 CURRENT ─────────────────────────────────────────────┐
        ↓                                                │
L1 MONTHLY (TrendEligible KPIs)                        │
        ↓                                                │
L2 RANKING (RankEligible KPIs, reads L1 population)     │
        ↓                                                │
L3 ATTENTION (signal diff from domain attention lists)   │  Platform
        ↓                                                │  infrastructure
L4 RELATIONSHIP (Top-N rollups at refresh)                │
        ↓                                                │
L5 RADAR (peer-normalized axis scores) ─────────────────┘
```

### Read path / profile engines (consumers of infrastructure)

```text
L0 ──────────► Overview, KPI Summary, Evidence (partial)
L0 + L1 ─────► Trend Engine
L1 ──────────► Ranking Engine (write); Ranking section (read)
L3 ──────────► Attention Engine → Attention History section
L4 ──────────► Relationship Engine → Related Entities section
L5 ──────────► Radar Engine → Radar section

L0 + L1 ─────► Comparison Engine (Cross-Period, Trend overlay)
L0 (×N) ─────► Comparison Engine (Multi-Entity)
L0 or L5 ────► Comparison Engine (Peer / Average — L5 preferred)
L5 (×2) ─────► Comparison Engine (Radar overlay mode)
```

### Consolidated dependency graph

```text
                    ┌─────────────────────────────────┐
                    │     Domain Snapshot Workers      │
                    │  (Customer, Salesman, …)         │
                    └──────────────┬──────────────────┘
                                   ↓
┌──────────────────────────────────────────────────────────────────┐
│                    PLATFORM INFRASTRUCTURE                          │
│                                                                   │
│  L0 Current ──► L1 History ──► L2 Ranking                        │
│       │              │              │                             │
│       │              │              └── independent of L3–L5        │
│       ↓              ↓                                            │
│  L3 Attention    (L3 reads domain signals, not L2)               │
│       ↓                                                           │
│  L4 Relationship (reads source DALs/aggregator at refresh)       │
│       ↓                                                           │
│  L5 Radar (reads L0/L1 peer population + peer group rules)       │
└──────────────────────────────────────────────────────────────────┘
                                   ↓
┌──────────────────────────────────────────────────────────────────┐
│                    READ ENGINES (no new tables)                   │
│                                                                   │
│  Trend Engine ◄── L1                                              │
│  Ranking Engine ◄── L2                                            │
│  Comparison Engine ◄── L0 + L1 [+ L5 for peer/radar modes]       │
│  Profile Composer ◄── all engines + KPI Registry                  │
└──────────────────────────────────────────────────────────────────┘
                                   ↓
┌──────────────────────────────────────────────────────────────────┐
│                    ENTITY PACKS (consumers)                         │
│  Customer ✓ │ Salesman │ Supplier │ Item │ Warehouse …           │
│  (registrar + producer + evidence resolver per type)              │
└──────────────────────────────────────────────────────────────────┘
```

**Key dependencies:**

| Layer / Engine | Depends on | Blocks |
|----------------|------------|--------|
| L3 Attention | L0 producer path, domain attention signals | Attention section only |
| L4 Relationship | Domain aggregator/DAL inputs at refresh | Related Entities section |
| L5 Radar | L0 (peer distribution), peer group rules | Radar section; Peer comparison mode |
| Comparison | L0; L1 for cross-period/trend overlay; L5 for peer/radar modes | Comparison section, compare page |
| Entity packs | All layers they choose to adopt | Nothing — can enable per-type incrementally |

**L3 does not depend on L2.** **L4 does not depend on L3.** Layers L3–L5 are **sequential by convention** (worker pipeline clarity), not hard data prerequisites.

---

## Part 5 — Artifact Alignment & Documentation Update Recommendations

### 1. `implementation-plan-entity-analytics.md` — **major revision**

| Section | Issue | Required change |
|---------|-------|-----------------|
| §1 Implementation Strategy | Entity-type milestone sequence | Replace with **capability-layer-first** strategy; entity packs as separate track |
| §2 Phase breakdown | M32.3–M32.5 = Salesman/Supplier/Item | Renumber: M32.3 L1+Trend ✓, M32.4 L2+Ranking ✓, M32.5 L3 Attention, M32.6 L4 Relationship, M32.7 Comparison, M32.8 L5 Radar, M32.9+ entity packs |
| §3 Component priorities | Comparison P0 in M32.1; Attention P0 in M32.2 | Reflect actual phasing; mark M32.1 delivered scope (L0 shell only) |
| §8 Milestone sections M32.3–M32.6 | Entity profile milestones | Rewrite as capability milestones + new entity-pack milestones |
| MVP table §2 | Claims M32.2 includes comparison, attention, L1 | Annotate **delivered vs deferred**; point to revised roadmap |
| §10.3 M32.1 sequence | Lists all engines in sprint 1 | Mark completed vs deferred per M32.1 summary |

### 2. `entity-analytics-architecture.md` — **targeted updates**

| Section | Issue | Required change |
|---------|-------|-----------------|
| §2 write path diagram | Ranking before Attention | Align with §6 generation flow **or** document that L2 runs immediately after L1 (implementation choice) |
| §6 Generation flow steps 6–9 | Attention → Relationship → Ranking → Radar | Update to match implementation: **L2 after L1**; then L3 → L4 → L5 |
| §2 Trend Engine responsibility | "derive growth KPIs" | Clarify: Trend Engine = series; Comparison Engine = MoM/YoY deltas |
| §6 Risks | "defer Item to M32.5" | Change to M32.11 or "entity pack milestone" |
| §4 Entity onboarding example | "Warehouse at M32.6" | Update to post-platform-layer milestone |
| Document control | No M32.3–M32.4 revision | Add v1.1 entry documenting layer-first delivery |

### 3. `entity-analytics-developer-guide.md` — **moderate updates**

| Section | Issue | Required change |
|---------|-------|-----------------|
| Overview diagram | Stops at L2 | Extend pipeline diagram to show L3–L5 placeholders |
| Producer lifecycle | No L3–L5 steps | Add numbered steps for future Attention/Relationship/Radar hooks |
| Profile composition | Lists Comparison as NotImplemented | Keep current; add "planned milestone" column |
| Related documents | Missing M32.3/M32.4 summaries | Add links |
| Status header | "M32.2R hardening" | Update to M32.4 complete |

### 4. `Entity-Analytics-Feasibility-Study.md` — **annotation only**

Feasibility remains **business authority**. Do not rewrite requirements.

| Section | Change |
|---------|--------|
| §17.3 Implementation phases | Add **Implementation Note (2026-06)** explaining capability-layer delivery on Customer before entity-pack expansion |
| §17.4 MVP | Footnote: MVP technical scope split across M32.2–M32.7; business criterion unchanged |

### 5. Implementation summaries — **corrections**

| File | Issue | Change |
|------|-------|--------|
| `m32.4-implementation-summary.md` §9 | M32.5 = Comparison | **Correct** to M32.5 = Attention L3; move Comparison to M32.7 |
| `m32.2-implementation-summary.md` §Remaining Work | M32.7 Radar, no Comparison number | Align to authoritative roadmap |
| `m32.2r-hardening-summary.md` §Readiness | M32.8 Comparison | Renumber to M32.7 per resolved roadmap |
| All summaries | No cross-link to authoritative roadmap | Add "see implementation-plan §8 (rev 2)" |

### 6. New artifact recommended

Create **`docs/work/btr-portal/entity-analytics/m32-roadmap-authoritative.md`** (or update implementation plan in place) as the **single roadmap SSOT** so milestone numbers do not drift across per-milestone summaries.

---

## Part 6 — Revised Authoritative Roadmap (Remaining Work)

### M32.5 — Attention History (L3)

**Purpose:** Persist attention signal lifecycle (FirstSeen, LastSeen, IsActive) at refresh; activate Attention History profile section.

**Dependencies:** M32.2 Customer producer; M17 attention signals in `CustomerEntityAnalyticsProduceInput`; L0 write path.

**Deliverables:**
- `BTRPD_EntityAnalytics_Attention` SQL + repository write/read
- `EntityAttentionEngine` (signal diff)
- Producer hook after L2 in Customer worker
- `ProfileAttentionHistorySection.vue`
- Customer signal catalog mapping (Overdue, Dormant, etc.)

**Architectural role:** Platform infrastructure — L3 snapshot layer.

---

### M32.6 — Relationship Engine (L4)

**Purpose:** Materialize Top-N related entities at refresh; activate Related Entities profile section.

**Dependencies:** M32.2 producer; domain data for salesman/items/principals (M31/M17); relationship pack metadata.

**Deliverables:**
- `BTRPD_EntityAnalytics_Relationship` SQL + repository
- `EntityRelationshipEngine` + relationship pack registration
- Customer relationships: Assigned Salesman, Top Items, Top Principals
- `ProfileRelatedEntitiesSection.vue` with profile-to-profile navigation

**Architectural role:** Platform infrastructure — L4 snapshot layer.

---

### M32.7 — Comparison Engine

**Purpose:** Cross-period (CURRENT vs prior month vs YoY) and multi-entity comparison; dedicated compare API/page; MoM/YoY derivation deferred from Trend.

**Dependencies:** L0 + L1 (minimum); L5 optional for Peer/Radar modes (ship basic modes first).

**Deliverables:**
- `EntityComparisonEngine` + `ComparisonContext`
- `GET /api/entity-analytics/compare`
- Profile Comparison section (Cross-Period default)
- `CustomerCompareView.vue` (2–4 entities)
- `GET /api/entity-analytics/search` (if not bundled elsewhere)

**Architectural role:** Read-time composition engine (no new snapshot layer).

---

### M32.8 — Radar Engine (L5)

**Purpose:** Peer-percentile normalized axis scores at refresh; Radar profile section.

**Dependencies:** L0 peer distributions; peer group rules from entity registration; ADR-EA-007 band fallback.

**Deliverables:**
- `BTRPD_EntityAnalytics_Radar` SQL + repository
- `EntityRadarEngine`
- Customer radar axes (4–6 from KPI pack)
- `ProfileRadarSection.vue`
- Peer group minimum gate (≥ 5 entities)

**Architectural role:** Platform infrastructure — L5 snapshot layer.

---

### M32.9 — Salesman Entity Pack

**Purpose:** Second entity type adopting full L0→L5 pipeline; RepHistory → L1 adapter/backfill.

**Dependencies:** M32.5–M32.8 platform layers complete (or partial enable per section).

**Deliverables:**
- `SalesmanEntityAnalyticsRegistrar/Producer/EvidenceResolver`
- Worker hook on Salesman refresh
- SF01 `ProfileRoute` links
- Optional: RepHistory backfill job

**Architectural role:** Entity consumer — registration only.

---

### M32.10 — Supplier Entity Pack

**Purpose:** Principal-centric profile; cross-domain PU + IN KPIs.

**Dependencies:** M32.9 pattern proven; M21 purchasing worker.

**Deliverables:** Supplier registrar/producer/resolver; PU01 profile links.

**Architectural role:** Entity consumer.

---

### M32.11 — Item Entity Pack

**Purpose:** SKU profile with active-subset L1 scope (ADR-EA-011); highest cardinality.

**Dependencies:** M32.8 platform complete; M19/M28 workers.

**Deliverables:** Item registrar/producer/resolver; IN02 profile links; refresh SLA monitoring.

**Architectural role:** Entity consumer.

---

### M32.12 — Future Entity Types

**Purpose:** Warehouse, Wilayah, Category, Brand, Collector — KPI Pack registration only.

**Dependencies:** M32.1 platform; M22 Location worker.

**Architectural role:** Entity consumers — no platform changes.

---

### M33 — Portal Cloud Snapshot Platform (unchanged)

Separate from M32; replicates L0–L5 + `BTRPD_*`.

---

## Part 7 — Deliverables

### 1. Architecture Alignment Report

| Dimension | Status |
|-----------|--------|
| Platform vision | **Aligned** — reusable engines, entity plugins, snapshot-driven reads |
| Implemented layers | L0, L1, L2 **complete** for Customer |
| Pending layers | L3, L4, L5 SQL + engines |
| Pending read engines | Comparison |
| Entity packs | Customer only; Salesman/Supplier/Item metadata registered |
| API surface | `types` + `profile` only; search/compare deferred |
| Conformance | **No major architectural deviations** — scope phasing and milestone renumbering only |

**Observations:**
- Platform-first / open-closed design validated across M32.2–M32.4.
- Trend/Ranking engines prove the layer-adoption model works.
- Original MVP business goal is **not yet met** — needs Comparison + ideally Attention/Relationships.
- Internal docs disagree on M32.5 identity — must be normalized before implementation starts.

---

### 2. Roadmap Drift Analysis

| | Sequence |
|---|----------|
| **Original** | M32.1 Platform → M32.2 Customer → M32.3 Salesman → M32.4 Supplier → M32.5 Item |
| **Current (implemented)** | M32.1 Platform (L0) → M32.2 Customer → M32.2R → M32.3 L1+Trend → M32.4 L2+Ranking |
| **Recommended (authoritative)** | M32.5 L3 Attention → M32.6 L4 Relationship → M32.7 Comparison → M32.8 L5 Radar → M32.9–M32.12 entity packs |

**Permanent adoption:** Capability layers on Customer first, then entity packs. Entity-type milestone numbering is **retired**.

---

### 3. Dependency Map

See Part 4 above.

---

### 4. Documentation Update Recommendations

| Priority | Artifact | Action |
|----------|----------|--------|
| P0 | `implementation-plan-entity-analytics.md` | Full milestone restructure (§5 table) |
| P0 | `m32.4-implementation-summary.md` §9 | Fix M32.5 = Attention, not Comparison |
| P0 | New or updated roadmap SSOT | Single authoritative sequence (Part 6) |
| P1 | `entity-analytics-architecture.md` | Generation flow order; Trend vs Comparison responsibilities |
| P1 | `entity-analytics-developer-guide.md` | Status, pipeline diagram, L3–L5 producer steps |
| P2 | `Entity-Analytics-Feasibility-Study.md` §17.3 | Implementation note (no requirement change) |
| P2 | m32.2 / m32.2R summaries | Align "Remaining Work" tables |

---

### 5. Final Recommendation

## **Continue with M32.5 as Attention History (L3) — with reordered remaining milestones**

Do **not** adopt m32.4's Comparison-as-M32.5 assignment.

| Decision | Choice |
|----------|--------|
| Next milestone | **M32.5 Attention History (L3)** |
| Milestone strategy | **Reorder** remaining work (not merge, not split) |
| Comparison placement | **M32.7** — after L3+L4, before Radar |
| Entity packs | **M32.9+** — after platform layers proven on Customer |

**Rationale:**
1. Matches L-layer numbering, repository interface comments, and the layer-first strategy that succeeded in M32.3/M32.4.
2. Completes the worker write pipeline before adding another read-only engine.
3. Closes two original MVP gaps (Attention + Relationships) before Comparison and Radar.
4. Comparison can ship Cross-Period and Multi-Entity at M32.7 without L5; Peer/Radar modes follow in M32.8.
5. Avoids building Salesman/Supplier/Item profiles on an incomplete platform — the root cause of the original milestone pivot.

**Do not merge milestones** — L3, L4, Comparison, and Radar have distinct deliverables and test surfaces.

**Do not split M32.5** — Attention engine + L3 table + profile section is already a coherent unit.

---

If you want the next step to be applying these updates to the artifacts (implementation plan, architecture doc, developer guide, and a new roadmap SSOT), say so and I can execute that documentation pass only — still no feature implementation.