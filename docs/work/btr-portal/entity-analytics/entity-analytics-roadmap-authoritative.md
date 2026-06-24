# Entity Analytics — Authoritative Roadmap (M32)

**Status:** Single source of truth for M32 milestone sequencing  
**Audience:** Architects, Implementers, Future Agents  
**Supersedes:** Entity-by-entity milestone numbering in feasibility §17.3 and implementation-plan v1.0  
**Architecture:** [entity-analytics-architecture.md](./entity-analytics-architecture.md)  
**Requirements:** [Entity-Analytics-Feasibility-Study.md](./Entity-Analytics-Feasibility-Study.md) (business rules unchanged)

---

## Delivery Strategy

Entity Analytics is delivered in two tracks:

1. **Capability layers** — prove each snapshot layer and read engine on **Customer** first (M32.1–M32.8).
2. **Entity packs** — register Salesman, Supplier, Item, and future types by adopting the proven pipeline (M32.9+).

Customer remains the reference consumer until platform layers M32.6–M32.8 complete.

---

## Milestone Sequence

| Milestone | Name | Status | Consumer |
| --------- | ---- | ------ | -------- |
| **M32.1** | Platform Foundation (L0 shell) | **Complete** | — |
| **M32.2** | Customer Performance Profile (L0) | **Complete** | Customer |
| **M32.2R** | Platform Hardening | **Complete** | — |
| **M32.3** | Monthly History + Trend (L1) | **Complete** | Customer |
| **M32.4** | Ranking History (L2) | **Complete** | Customer |
| **M32.5** | Attention History (L3) | **Complete** | Customer |
| **M32.6** | Relationship Engine (L4) | **Complete** | Customer |
| **M32.7** | Comparison Engine | **Complete** | Customer |
| **M32.8** | Radar Engine (L5) | **Complete** | Customer |
| **M32.9** | Salesman Entity Pack | Planned | Salesman |
| **M32.10** | Supplier Entity Pack | Planned | Supplier |
| **M32.11** | Item Entity Pack | Planned | Item |
| **M32.12** | Future Entity Types | Planned | Warehouse, Wilayah, … |
| **M33** | Portal Cloud Snapshot Platform | Separate | All portal features |

**Next milestone:** M32.9 Salesman Entity Pack.

---

## Completed Milestones

### M32.1 — Platform Foundation

| Field | Content |
| ----- | ------- |
| **Objective** | Reusable platform shell: entity model, L0 schema, KPI registry structure, profile composer, producer orchestrator, API, Vue shell |
| **Delivered** | `BTRPD_EntityAnalytics_Current`; `EntityAnalyticsAgg` module; `GET types` + `GET profile`; section DTOs (empty-safe); layer repository seams |
| **Deferred** | L1–L5 SQL; analytic engines; entity picker/search; compare API |
| **Summary** | [m32.1-implementation-summary.md](./m32.1-implementation-summary.md) |

### M32.2 — Customer Performance Profile

| Field | Content |
| ----- | ------- |
| **Objective** | First entity consumer: L0 CURRENT for all customers from M31 portfolio rows |
| **Delivered** | `CustomerEntityAnalyticsProducer`; `customer-default` KPI pack (3 KPIs); Overview, KPI Summary, Evidence; dashboard `ProfileRoute` links |
| **Deferred** | L1–L5 sections; compare page; search API |
| **Summary** | [m32.2-implementation-summary.md](./m32.2-implementation-summary.md) |

### M32.2R — Platform Hardening

| Field | Content |
| ----- | ------- |
| **Objective** | Stabilize extension points before history layers |
| **Delivered** | Layer interfaces; `EntityKpiEnvelopeFormatter`; enabled-type gate; developer guide |
| **Summary** | [m32.2r-hardening-summary.md](./m32.2r-hardening-summary.md) |

### M32.3 — Monthly History + Trend (L1)

| Field | Content |
| ----- | ------- |
| **Objective** | Generic L1 monthly snapshots and Trend Engine read path |
| **Delivered** | `BTRPD_EntityAnalytics_Monthly`, `_MonthClose`; `EntityTrendEngine`; `EntityAnalyticsMonthCloseService`; Trend profile section + `KpiTrendChart` |
| **Note** | MoM/YoY deltas deferred to M32.7 Comparison Engine |
| **Summary** | [m32.3-implementation-summary.md](./m32.3-implementation-summary.md) |

### M32.4 — Ranking History (L2)

| Field | Content |
| ----- | ------- |
| **Objective** | Generic L2 ranking snapshots and Ranking Engine |
| **Delivered** | `BTRPD_EntityAnalytics_Ranking`; `EntityRankingEngine`; Ranking profile section + `KpiRankChart` |
| **Summary** | [m32.4-implementation-summary.md](./m32.4-implementation-summary.md) |

### M32.5 — Attention History (L3)

| Field | Content |
| ----- | ------- |
| **Objective** | Generic L3 attention lifecycle snapshots; Attention Engine at refresh |
| **Delivered** | `BTRPD_EntityAnalytics_Attention`; `EntityAttentionEngine`; Attention History profile section |
| **Summary** | [m32.5-implementation-summary.md](./m32.5-implementation-summary.md) |

### M32.6 — Relationship Engine (L4)

| Field | Content |
| ----- | ------- |
| **Objective** | Materialize Top-N related entities at refresh; activate Related Entities profile section |
| **Delivered** | `BTRPD_EntityAnalytics_Relationship`; `EntityRelationshipEngine`; Customer relationships (Assigned Salesman, Top Items, Top Principals); `ProfileRelatedEntitiesSection.vue` |
| **Summary** | [m32.6-implementation-summary.md](./m32.6-implementation-summary.md) |

### M32.7 — Comparison Engine

| Field | Content |
| ----- | ------- |
| **Objective** | Cross-period and multi-entity comparison; MoM/YoY derivation; compare API/page |
| **Delivered** | `EntityComparisonEngine` + `ComparisonContext`; `GET /api/entity-analytics/compare`; `GET /api/entity-analytics/search`; Profile Comparison section; `CustomerCompareView.vue` |
| **Summary** | [m32.7-implementation-summary.md](./m32.7-implementation-summary.md) |

---

### M32.8 — Radar Engine (L5)

| Field | Content |
| ----- | ------- |
| **Objective** | Peer-percentile normalized axis scores at refresh; Radar profile section |
| **Delivered** | `BTRPD_EntityAnalytics_Radar`; `EntityRadarEngine` + `PeerGroupResolver`; Customer 6-axis radar pack; `ProfileRadarSection.vue`; `RadarCompareSection.vue`; peer group minimum gate (≥ 5) |
| **Summary** | [m32.8-implementation-summary.md](./m32.8-implementation-summary.md) |

---

## Remaining Milestones

### M32.7 — Comparison Engine

| Field | Content |
| ----- | ------- |
| **Objective** | Cross-period and multi-entity comparison; MoM/YoY derivation; compare API/page |
| **Dependencies** | L0 + L1 (required); L5 optional for Peer/Radar modes (ship basic modes first) |
| **Deliverables** | `EntityComparisonEngine` + `ComparisonContext`; `GET /api/entity-analytics/compare`; Profile Comparison section; `CustomerCompareView.vue`; `GET /api/entity-analytics/search` (if not bundled elsewhere) |
| **Architectural role** | Read-time composition engine (no new snapshot layer) |
| **Status** | Complete — see [m32.7-implementation-summary.md](./m32.7-implementation-summary.md) |

### M32.8 — Radar Engine (L5)

| Field | Content |
| ----- | ------- |
| **Objective** | Peer-percentile normalized axis scores at refresh; Radar profile section |
| **Dependencies** | L0 peer distributions; peer group rules; ADR-EA-007 band fallback |
| **Deliverables** | `BTRPD_EntityAnalytics_Radar` SQL + repository; `EntityRadarEngine`; Customer radar axes; `ProfileRadarSection.vue`; peer group minimum gate (≥ 5) |
| **Architectural role** | Platform infrastructure — L5 snapshot layer |

### M32.9 — Salesman Entity Pack

| Field | Content |
| ----- | ------- |
| **Objective** | Second entity type adopting L0→L5 pipeline; RepHistory → L1 adapter/backfill |
| **Dependencies** | M32.5–M32.8 platform layers (or partial enable per section) |
| **Deliverables** | `SalesmanEntityAnalyticsRegistrar/Producer/EvidenceResolver`; Salesman worker hook; SF01 `ProfileRoute` links; optional RepHistory backfill |
| **Architectural role** | Entity consumer — registration only |

### M32.10 — Supplier Entity Pack

| Field | Content |
| ----- | ------- |
| **Objective** | Principal-centric profile; cross-domain PU + IN KPIs |
| **Dependencies** | M32.9 pattern proven; M21 purchasing worker |
| **Deliverables** | Supplier registrar/producer/resolver; PU01 profile links |
| **Architectural role** | Entity consumer |

### M32.11 — Item Entity Pack

| Field | Content |
| ----- | ------- |
| **Objective** | SKU profile with active-subset L1 scope (ADR-EA-011); highest cardinality |
| **Dependencies** | M32.8 platform complete; M19/M28 workers |
| **Deliverables** | Item registrar/producer/resolver; IN02 profile links; refresh SLA monitoring |
| **Architectural role** | Entity consumer |

### M32.12 — Future Entity Types

| Field | Content |
| ----- | ------- |
| **Objective** | Warehouse, Wilayah, Category, Brand, Collector — KPI Pack registration only |
| **Dependencies** | M32.1 platform; M22 Location worker |
| **Deliverables** | Per-entity registrar + producer + evidence resolver; no L0–L5 schema changes |
| **Architectural role** | Entity consumers |

---

## Worker Write Path (Target State)

```text
Domain Aggregator (unchanged)
  → IEntityAnalyticsProducer
      → L0 CURRENT
      → L1 MONTHLY (TrendEligible KPIs)
      → L2 RANKING (RankEligible KPIs)          ← M32.4 complete
      → L3 ATTENTION (signal diff)              ← M32.5
      → L4 RELATIONSHIP (Top-N rollups)         ← M32.6
      → L5 RADAR (peer-normalized scores)       ← M32.8
```

Comparison Engine (M32.7) is **read-only** — composes L0/L1 (+ L5 for peer modes) at API request time.

---

## MVP Scope Note

Original MVP (feasibility §17.4) = M32.1 + M32.2 with full profile sections. Technical delivery was split across capability milestones M32.2–M32.7. The **business success criterion** is unchanged:

> Management can answer *"How is Customer X doing vs Customer Y over the last year?"* without exporting reports to Excel.

Full MVP closure requires M32.7 (Comparison) at minimum; Attention and Relationships close additional §17.4 MVP items.

---

## Document References

| Document | Role |
| -------- | ---- |
| This file | **Milestone sequence SSOT** |
| [implementation-plan-entity-analytics.md](./implementation-plan-entity-analytics.md) | Implementer execution detail |
| [entity-analytics-architecture.md](./entity-analytics-architecture.md) | Long-term platform architecture |
| [entity-analytics-developer-guide.md](../../features/entity-analytics/entity-analytics-developer-guide.md) | Permanent implementer knowledge |

Future milestone summaries **must** reference this document for milestone numbering.

---

## Document Control

| Version | Date | Author | Change |
| ------- | ---- | ------ | ------ |
| 1.0 | 2026-06-24 | Architect | Authoritative roadmap after M32.4 alignment review |
