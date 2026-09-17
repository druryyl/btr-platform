# ARCHITECTURE

## Principal Investigation Workspace (Entity Analytics) — Principal-centric Analytics

**Status:** DRAFT — Architectural Reference
**Milestone:** Principal-centric Analytics Initiative — Investigation Workspace
**Date:** 2026-09-11
**Role:** Architect deliverable
**Audience:** Planning Agents, Implementation Agents, Review Agents

> This document is implementation-neutral and planning-ready. It translates the approved
> feasibility decisions into a stable architectural blueprint. It does not create an
> implementation plan, phases, slices, backlog items, estimates, or task breakdowns. It
> introduces no new business decisions.

---

## 1. Executive Summary

### Purpose

This architecture defines the software structures, ownership boundaries, persistence model,
presentation model, and technical decisions required to realize the **Principal Investigation
Workspace** within Entity Analytics, while preserving compatibility with the existing
Supplier-based technical model.

The workspace answers one business question per Principal:

> "Understand this Principal" — across a Sales-Out lens (market performance) and a Purchasing
> lens (supply context).

It does **not** manage the Principal portfolio. Ranking, portfolio management, executive
monitoring, and strategic analysis remain in the Sales dashboard (`SA04` Principal Performance)
and the Executive surfaces (`EX01` / `EX02`).

### Status

```text
DRAFT
```

### Inputs

| Artifact | Role |
| --- | --- |
| `docs/work/btr-portal/principal-investigation-workspace/FEASIBILITY-ASSESSMENT.md` | **Primary authoritative input** — GAP-001..GAP-017, BQ-005, BQ-010, TQ-005, OQ-001..OQ-004 (all resolved) |
| `docs/work/btr-portal/principal-investigation-workspace/adrs/ADR-EA-001-radar-retirement.md` | Radar (Performance Signature) retirement decision |
| `docs/work/btr-portal/principal-centric-analytics-migration/FEASIBILITY-ASSESSMENT.md` | Operating-model and attribution decisions (GAP-001..GAP-023) |
| `docs/work/btr-portal/principal-centric-analytics-migration/PRINCIPAL-KPI-REGISTRY.md` | Authoritative Principal KPI catalog |
| `docs/work/btr-portal/entity-analytics/entity-analytics-architecture.md` | Entity Analytics platform architecture (L0–L5, engines, entity packs) |
| `docs/work/btr-portal/entity-analytics/M32R-Investigation-Workspace-Architecture.md` | Generic six-stage investigation workspace architecture |
| `docs/features/entity-analytics/entity-analytics-developer-guide.md` | Implemented Supplier/Principal entity pack |
| `docs/features/btr-portal/dashboard-sa04-principal-performance.md` | Existing Principal Sales-Out dashboard (`SA04`) |
| `docs/features/sales-person-principal/feature.md` | Principal = Supplier terminology precedent |

### Scope

The architecture covers exactly one surface: the **Principal Investigation Workspace** inside
Entity Analytics (two investigation lenses over the `Supplier` entity type). It does **not**
redesign the platform, the KPI registry, the reporting model, or the dashboard surfaces that
surround it.

---

## 2. Architecture Principles

1. **Preserve the technical model.** `Supplier` is the canonical domain, database, integration,
   and legacy identity. `Principal` is a presentation alias. No technical identity migration.
2. **One authoritative Principal analytics layer.** The portal exposes exactly one Principal
   Entity Analytics profile, composed by the single `SupplierEntityAnalyticsProducer`.
3. **Investigation is configuration, not new analytics.** The workspace platform is
   entity-type-neutral; Principal-specific content is lens/preset/KPI/evidence configuration.
4. **Lenses are presentation perspectives.** Sales-Out and Purchasing lenses investigate the
   same Principal (`SupplierId`); they never create a second entity type, profile, or workspace.
5. **Sales-Out and purchase-in never conflate.** Purchase-in is operational context and is never
   relabeled, ranked, or presented as sales performance.
6. **Returns never reduce Sales-Out.** `PRN-SALES-001` is authoritative and independent of
   returns (GAP-020 / KPI Registry).
7. **Identity is `SupplierId`-keyed everywhere.** Purchasing aggregations, relationships,
   snapshots, trends, and evidence resolve identity via `SupplierId`; `SupplierName` is
   presentation only.
8. **Evidence is lens-scoped.** Every evidence panel, link, title, and disclosure indicates its
   originating lens; no duplicate evidence model.
9. **Freshness is explicit.** Every view displays `Generated At`, `Reporting Period`, and `Data
   Snapshot Period` (when applicable); no real-time guarantee.
10. **No new KPI stewardship.** Derived investigation metrics require no KPI ownership, registry
    entry, ranking contract, or evidence-route ownership.

---

## 3. Decision Traceability

All architecture decisions are traceable to approved feasibility decisions. Source identifiers
are disambiguated as follows: `IW-GAP-###` = Principal Investigation Workspace feasibility;
`MIG-GAP-###` = Principal-centric migration feasibility.

| Source Decision | Decision consumed (summary) | Realized in |
| --- | --- | --- |
| `IW-GAP-001` | Supplier canonical; Principal presentation alias; `SupplierId` authoritative; no new Principal master entity | §5 Identity, ADR-PIW-001 |
| `IW-GAP-002` | Two presentation lenses over one Principal in a single workspace via lens switcher | §6 Workspace, ADR-PIW-002 |
| `IW-GAP-003` | Workspace defaults to Sales-Out lens (`principal-sales-out-map`) | §6, §9 Navigation, ADR-PIW-003 |
| `IW-GAP-004` | Default Sales-Out Population Map axes (Achievement %, YoY Growth %, size Sales-Out, color Return %) | §7 Stages, §8 Snapshot |
| `IW-GAP-005` | Purchase-to-Sales-Out Ratio = Purchase Amount ÷ Sales-Out (investigation-only) | §8 KPI, ADR-PIW-005 |
| `IW-GAP-006` | Efficiency = Inventory Replenishment Efficiency; no PO-based metrics | §6, §8, ADR-PIW-006 |
| `IW-GAP-007` | Purchase trend sourced from `PU-KPI-001` monthly history; `PRN-PUR-001` current-only | §8 Snapshot |
| `IW-GAP-008` | Purchasing drivers = Top Purchased Items + Purchase History (invoice evidence) | §9 Relationships |
| `IW-GAP-009` | Attention spans both lenses; five Principal sales categories + purchase/inventory signals | §7 Stages |
| `IW-GAP-010` + `ADR-EA-001` | Performance Signature (Radar) retired for all entity types | §7 Stages, ADR-PIW-012 |
| `IW-GAP-011` | `supplier-all-active` peer population; no Principal peer selector | §7 Stages, ADR-PIW-007 |
| `IW-GAP-012` | Five Principal presentation strings; internal identifiers unchanged | §5 Identity, §9 Navigation |
| `IW-GAP-013` | Evidence explicitly lens-scoped in presentation | §10 Evidence, ADR-PIW-004 |
| `IW-GAP-015` | Hide map-tooltip dimension row; no generic "Category" label | §7 Stages |
| `IW-GAP-016` | `SupplierId`-keyed purchasing identity; `SupplierName` presentation only | §5 Identity, ADR-PIW-008 |
| `IW-GAP-017` | Terminology governance: Principal user-facing, Supplier technical | §5 Identity, ADR-PIW-011 |
| `IW-BQ-005` | Replace Supplier Investigation entry; no parallel entries | §11 Migration |
| `IW-BQ-010` | Purchasing drivers shown; PO-based/user/procurement relationships excluded | §9 Relationships |
| `IW-TQ-005` | No new Principal KPI IDs; derived metrics only | §8 KPI, ADR-PIW-005 |
| `IW-OQ-001` | No new KPI stewardship model; evidence ownership stays with source providers | §8, §10 |
| `IW-OQ-002` | Standard freshness policy (Generated At, Reporting Period, Data Snapshot Period) | §11 Data Health, ADR-PIW-009 |
| `IW-OQ-003` | Dedicated Data Health section with Target Coverage / Missing Target / Unknown Principal | §11 Data Health, ADR-PIW-010 |
| `IW-OQ-004` | Product Owner owns terminology standards; teams apply them | §5 Identity |
| `MIG-GAP-003` | Customer–Principal is transaction-derived; no master assignment | §9 Relationships |
| `MIG-GAP-004` | Item–Principal via immutable Item master (`Brg.SupplierId`); no invoice-time snapshot | §9 Relationships |
| `MIG-GAP-005` | No Principal financial/credit/collection KPIs | §8 KPI |
| `MIG-GAP-010` | Supplier/Principal Entity Analytics = sales-out performance; purchase-in separate | §6 Workspace |
| `MIG-GAP-011` + `MIG-TQ-007` | Single authoritative aggregation layer; single refresh/freshness contract; no producer extension | §8 Snapshot, §13 Backend |
| `MIG-GAP-017` | Principal user-facing; Supplier technical (no separate distinction) | §5 Identity |
| `MIG-GAP-018` | No Principal-scoped authorization; analytical dimension only | §13 Backend |
| `MIG-GAP-019` | Principal sales line-item authoritative; no header reconciliation | §10 Evidence |
| `MIG-GAP-020` | Sales-Out (DPP) authoritative; sales and returns independent | §8 KPI |
| `MIG-TQ-006` | Active = transaction within 6 months; Dormant = none within 6 months; history retained | §9 Relationships |
| `MIG-TQ-009` | Faktur Item is canonical evidence grain | §10 Evidence |
| `MIG-TQ-010` | Single canonical Principal Sales-Out KPI (`PRN-SALES-001`) | §8 KPI |

---

## 4. Architectural Overview

### 4.1 Principal-centric Analytics landscape

The Principal-centric Analytics initiative spans three distinct surfaces, each with a bounded
responsibility. The Principal Investigation Workspace is exactly one of them.

```text
┌─────────────────────────────────────────────────────────────────────────────┐
│                     Principal-centric Analytics (business term)              │
│                     technical identity: Supplier / SupplierId                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌───────────────────────────┐   ┌───────────────────────────────┐          │
│  │ Principal Performance     │   │ Principal Investigation       │          │
│  │ Dashboard (SA04)          │   │ Workspace (Entity Analytics)  │          │
│  │  — analytical dashboard   │   │  — investigation workspace    │          │
│  │  — manage / rank / monitor│   │  — understand one Principal   │          │
│  └───────────────────────────┘   └───────────────────────────────┘          │
│               ▲                              ▲                              │
│               │ cross-navigate               │ cross-navigate               │
│               └──────────────────────────────┘                              │
│                              ▲                                              │
│                              │ lens switcher (Sales-Out / Purchasing)       │
│  ┌───────────────────────────────────────────────────────────────────────┐ │
│  │ Existing Principal Sales-Out analytics (PRN-KPI Registry, BTRPD_*      │ │
│  │  projections, EX01/EX02 attention, SA01 contribution)                 │ │
│  └───────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Platform layering

The workspace is a **configuration of the existing entity-neutral platform**, not a new
subsystem.

```text
┌──────────────────────────────────────────────────────────────────────────┐
│  Principal Investigation Workspace (presentation)                        │
│    — two lenses, lens switcher, lens-scoped panels & evidence            │
├──────────────────────────────────────────────────────────────────────────┤
│  Entity Analytics Platform (reusable, entity-type-neutral)               │
│    — six-stage workspace · Population Map · KPI/Relationship/Attention  │
│    — L0–L5 snapshot layers · preset engine · evidence resolvers         │
├──────────────────────────────────────────────────────────────────────────┤
│  Supplier/Principal entity pack (registration-only configuration)        │
│    — SupplierEntityAnalyticsRegistrar · Producer · Resolver · Catalogs   │
├──────────────────────────────────────────────────────────────────────────┤
│  Domain snapshots (BTRPD_Principal*, BTRPD_CustomerPrincipalRelationship,│
│    BTRPD_Inventory* by SupplierId, Purchasing Management aggregates)     │
├──────────────────────────────────────────────────────────────────────────┤
│  Source of truth (Sales transactions, Purchasing, Inventory, Item master)│
└──────────────────────────────────────────────────────────────────────────┘
```

### 4.3 Bounded responsibilities

| Surface | Responsibility | Does NOT do |
| --- | --- | --- |
| **Principal Investigation Workspace** (this architecture) | Investigate one Principal across Sales-Out and Purchasing lenses; reach a defensible, evidence-backed conclusion | Manage portfolio; rank/compare Principals; strategic monitoring; execute workflows |
| **Principal Performance Dashboard** (`SA04`) | Rank, monitor, and manage Principal commercial performance; target/return/customer-reach panels | Investigate causal context; supply-side analysis |
| **Purchasing surfaces** (`PU01`/`PU02`) | Purchase-in, posting, stock, and dependency analysis | Present sales performance |
| **Entity Analytics (other entity types)** | Investigate Customer, Salesman, Item | Principal investigation |
| **Executive surfaces** (`EX01`/`EX02`) | Cross-domain attention and alert routing | Investigation detail |

The investigation workspace and the performance dashboard **share the same authoritative KPI
values** (`PRN-*` projections and L0–L5 snapshots) but serve different questions: the workspace
asks *why*; the dashboard asks *which*.

---

## 5. Principal Identity Architecture

### 5.1 Supplier vs Principal terminology model

Two terms describe one entity. The relationship is **alias**, not hierarchy or mapping:

```text
Business-facing term        Principal
Technical term              Supplier
Canonical technical key     SupplierId
Canonical code key          SupplierCode
```

- `Principal` = the business term used by Analytics, Dashboard, Navigation, KPI Registry,
  Documentation, and UX.
- `Supplier` = the technical term used by database schema, API internals, code identifiers,
  analytics registrations, and legacy integrations.
- There is **no equivalence table, translation layer, or separate Principal master entity**
  (IW-GAP-001, MIG-GAP-017).

### 5.2 Canonical identity rules

1. `SupplierId` is the **authoritative technical identifier** for all Principal investigation
   capabilities (IW-GAP-016).
2. `EntityId = SupplierId`, `EntityCode = SupplierCode` within Entity Analytics.
3. All Purchasing Lens aggregations, relationships, snapshots, trends, and evidence resolve
   identity via `SupplierId`-keyed data.
4. `SupplierName` is presentation only. **Name-based aggregation, matching, or joining is
   prohibited** (IW-GAP-016).
5. Historical data may display the recorded `SupplierName`, but identity resolution always uses
   `SupplierId`.

### 5.3 Presentation terminology mapping

Entity Analytics surfaces display the following Principal strings (IW-GAP-012):

| Surface | Presentation label |
| --- | --- |
| Investigation workspace title | Principal Investigation Workspace |
| Profile page | Principal Profile |
| Comparison page | Compare Principals |
| Relationship section | Principal Relationships |
| Evidence section | Principal Evidence |

Internal identifiers (`SupplierId`, `entityType=Supplier`, Supplier analytics registrations,
snapshot keys, routes) are unchanged. No database, API, snapshot, or domain migration is
performed (IW-GAP-012).

### 5.4 Navigation terminology mapping

- User-facing navigation and menu entries display **Principal**.
- Technical route templates and URL identifiers retain **Supplier** form (`/analytics/suppliers/{supplierId}`)
  (IW-GAP-012, IW-TQ-001).
- Breadcrumb scope labels and menu labels use Principal; the URL path is a technical concern.

### 5.5 Terminology governance

Terminology standards are owned by the **BTR Portal Product Owner** (IW-OQ-004). Implementation
teams apply the approved mapping across navigation, menus, workspace titles, dashboard titles,
labels, disclosures, and documentation. Technical artifacts may continue using Supplier
terminology where required by code, database, integration, and legacy-system constraints
(IW-GAP-017).

---

## 6. Principal Investigation Workspace Architecture

### 6.1 Workspace shape

A **single workspace** exposes two presentation lenses over the same Principal (`SupplierId`),
selected via a **lens switcher** (IW-GAP-002). The lens is a presentation perspective — not a new
entity type, not a separate workspace, not a duplicate profile, and not duplicate master data.

```text
Principal Investigation Workspace
  ├── Lens switcher
  │     ├── Sales-Out Lens   (default)
  │     └── Purchasing Lens
  ├── Shared investigation shell (Population → … → Evidence)
  └── Shared Data Health section (always visible, lens-independent)
```

The lens switches the KPI grouping, the population preset, the attention categories, the
relationship drivers, and the evidence routes shown in the investigation stages. The investigation
model itself is unchanged.

### 6.2 Sales-Out Lens — bounded responsibility

Owns the **commercial performance** investigation of a Principal.

Investigation capabilities (IW-GAP-002):

| Capability | Source KPI / asset | Semantics |
| --- | --- | --- |
| Principal Sales-Out | `PRN-SALES-001` (`BTRPD_PrincipalSalesOut`) | Authoritative performance + default ranking KPI; independent of Returns |
| Growth | `PRN-GRW-001`, `PRN-GRW-002` (`BTRPD_PrincipalMomGrowth`, `BTRPD_PrincipalYoyGrowth`) | From stored `PRN-SALES-001` history only |
| Returns | `PRN-RET-001..004` (`BTRPD_PrincipalReturn*`) | Independent KPIs; never reduce Sales-Out |
| Achievement | `PRN-TGT-002`, `PRN-TGT-003` (`BTRPD_PrincipalAchievement`) | Sales-Out vs Principal Target |
| Customer Reach | `PRN-CUS-001`, `PRN-CUS-002` (`BTRPD_PrincipalActiveCustomer`, `BTRPD_PrincipalCustomerCoverage`) | From `BTRPD_CustomerPrincipalRelationship` |
| Customer Contribution | `BTRPD_CustomerPrincipalRelationship` (pair-attributed `PRN-SALES-001`) | Top customers by sales-out |
| Salesman Contribution | `BTRPD_PrincipalContribution` | Top salesmen by sales-out |
| Item Contribution | `BTRPD_PrincipalSalesOut` / item-line rollup | Top items by sales-out |
| Attention Signals | Sales-Out categories (IW-GAP-009) | Sales-Out Decline, Growth Deterioration, Target Miss, Return Risk, Coverage Deterioration |
| Evidence | Faktur Item, Return Item, Customer/Salesman Contribution | Lens-scoped (IW-GAP-013) |

### 6.3 Purchasing Lens — bounded responsibility

Owns the **supply-side / inventory context** investigation of a Principal. It is operational
context, never sales performance (MIG-GAP-010).

Investigation capabilities (IW-GAP-002, IW-GAP-006):

| Capability | Source KPI / asset | Semantics |
| --- | --- | --- |
| Purchase Amount | `PRN-PUR-001` (`BTRPD_PrincipalPurchaseIn`) | Current-period Purchase-In; never a performance/ranking KPI |
| Purchase Trend | `PU-KPI-001` monthly history (IW-GAP-007) | Reused via shared `SupplierId` identity |
| Purchase-to-Sales-Out Ratio | Derived: Purchase Amount ÷ `PRN-SALES-001` (IW-GAP-005) | Investigation-only alignment indicator |
| Inventory Exposure | `PRN-INV-001`, `PRN-INV-002` (`BTRPD_PrincipalInventory`) | Inventory Value, Inventory Days |
| Stock Risk | `BTRPD_InventoryRiskBreakdown.SupplierId` | At-risk / slow-moving exposure |
| Purchasing Drivers | Top Purchased Items (Purchase Invoice Detail), Purchase History (IW-GAP-008) | No PO-based drivers |
| Purchase Evidence | Purchase Invoice, Purchase History, Inventory Exposure, Inventory Movement | Lens-scoped (IW-GAP-013) |

Purchasing efficiency is modeled as **Inventory Replenishment Efficiency** — the organization
operates no formal Purchase Order workflow, so no PO Fulfillment, PO Backlog, or Outstanding PO
metrics exist (IW-GAP-006).

### 6.4 Architectural boundaries and ownership

| Boundary | Owner | Responsibility |
| --- | --- | --- |
| Lens state | Investigation workspace (frontend) | Which lens is active; lens-scoped panel visibility |
| KPI grouping | KPI Pack / lens configuration | Which KPI IDs belong to Sales-Out vs Purchasing lens |
| Preset selection | Preset engine + lens | Default preset per lens |
| Evidence routing | Evidence resolver | Lens-scoped evidence links |
| Data authority | Domain snapshots + `SupplierEntityAnalyticsProducer` | KPI values; never recomputed in the workspace |

No lens owns new analytics. The Sales-Out lens composes already-persisted `BTRPD_Principal*`
projections; the Purchasing lens composes already-persisted purchase/inventory data plus the
single derived alignment ratio (IW-GAP-005, IW-TQ-005).

---

## 7. Investigation Stage Architecture

### 7.1 Mapping to the Entity Analytics investigation model

The Principal Investigation reuses the generic six-stage model (M32R). The mapping and required
adaptations are:

| Stage | Generic capability | Principal adaptation | Reuse vs adapt |
| --- | --- | --- | --- |
| **Population** | Population Map + presets | `principal-sales-out-map` (default, Sales-Out lens); retained purchasing presets for Purchasing lens | Reuse platform; add preset |
| **Current Facts** | KPI Summary | Principal KPI packs grouped by active lens | Reuse; lens grouping |
| **Context** | Peer Position, Trend, Signal History, Position History | Peer via `supplier-all-active`; attention spans both lenses; purchase trend from `PU-KPI-001` history | Reuse; lens configuration |
| **Explanation** | Related Entities / Business Drivers | Sales-Out drivers (Top Customers/Salesmen/Items); Purchasing drivers (Top Purchased Items, Purchase History) | Reuse; add purchasing drivers |
| **Validation** | Evidence resolver | Lens-scoped evidence routes; **no Performance Signature** | Reuse; retire signature |
| **Evidence** | Evidence links | Lens-distinguishable evidence | Reuse; lens scoping |

### 7.2 Population Stage — `principal-sales-out-map`

Default preset for the Sales-Out lens (IW-GAP-003, IW-GAP-004):

```text
X-Axis      : Achievement %
Y-Axis      : YoY Growth %
Bubble Size : Principal Sales-Out (PRN-SALES-001)
Bubble Color: Return % (PRN-RET-004)
```

Purpose: identify Star, Growing, Underperforming, Declining, and High-return-risk Principals.
Additional presets may be introduced later but must not change default behavior (IW-GAP-004).

The Purchasing lens retains the existing purchase/inventory-led presets (e.g., purchase exposure)
as its population view.

### 7.3 Peer population

The comparison population is the existing **all-active Principal population**
(`supplier-all-active`) (IW-GAP-011). There is no Principal-specific Peer Group Selector and no
dimension-based peer grouping. The Peer Group Selector remains hidden for Principal. Peer Position
remains available against the all-active population.

### 7.4 Context — attention signals

Attention spans both lenses (IW-GAP-009). Reused EX01/EX02 Sales-Out signals apply where
applicable; existing purchase/inventory signals are retained. No new attention engine is
introduced.

Principal Sales-Out attention categories:

```text
Sales-Out Decline · Growth Deterioration · Target Miss · Return Risk · Coverage Deterioration
```

Existing purchasing/inventory signals (Qualified Backlog, Spend Concentration, Inventory
Concentration, At-Risk Exposure, Compound Dependency, Inventory-No-Purchase, Unknown Principal)
remain available under the Purchasing lens.

### 7.5 Retired Performance Signature

The Performance Signature (Radar / L5) is retired for **all** entity types (IW-GAP-010,
ADR-EA-001). There is no Principal Performance Signature, no dual-lens signature, and no
replacement radar. Investigation relies on: KPI Summary, Population Position, Trajectory,
Business Drivers, Relationships, Attention Signals, and Evidence.

The Validation Stage omits the Performance Signature surface for Principal (and every entity
type). Existing L5 snapshot history is not deleted by this decision (ADR-EA-001).

### 7.6 Tooltip behavior

The Population Map tooltip hides the dimension row when no meaningful Principal dimension exists,
and does not display a generic "Category" label (IW-GAP-015).

---

## 8. KPI Architecture

### 8.1 Reused KPI registries

The Principal KPI catalog (`PRINCIPAL-KPI-REGISTRY.md`) is authoritative and is **not expanded**
by this initiative. The workspace composes the already-composed Supplier/Principal KPI packs
(`supplier-default` via `SupplierEntityAnalyticsRegistrar`).

### 8.2 Authoritative measures vs derived metrics

| KPI | Type | Authority |
| --- | --- | --- |
| `PRN-SALES-001` Principal Sales-Out | **Authoritative** performance + ranking KPI | Registry + `BTRPD_PrincipalSalesOut` |
| `PRN-RET-001..004` Returns | **Authoritative** independent KPIs | Registry + `BTRPD_PrincipalReturn*` |
| `PRN-TGT-001..003` Target/Achievement | **Authoritative** | Registry + `BTRPD_PrincipalTarget`, `BTRPD_PrincipalAchievement` |
| `PRN-GRW-001..002` Growth | **Authoritative** (derived from stored `PRN-SALES-001` history) | Registry + `BTRPD_Principal*Growth` |
| `PRN-CUS-001..002` Customer Reach | **Authoritative** | Registry + `BTRPD_PrincipalActiveCustomer`, `BTRPD_PrincipalCustomerCoverage` |
| `PRN-PUR-001` Purchase-In | **Authoritative** (operational, never ranking) | Registry + `BTRPD_PrincipalPurchaseIn` |
| `PRN-INV-001..002` Inventory | **Authoritative** (operational, never ranking) | Registry + `BTRPD_PrincipalInventory` |
| Purchase-to-Sales-Out Ratio | **Derived investigation metric** | Not a registry KPI (IW-GAP-005, IW-TQ-005) |

### 8.3 Derived investigation metrics

The **Purchase-to-Sales-Out Ratio** is the only derived investigation metric:

```text
Purchase-to-Sales-Out Ratio = Purchase Amount (PRN-PUR-001) ÷ Principal Sales-Out (PRN-SALES-001)
  > 1.0   : Inventory Building / Potential Over-Buying
  ≈ 1.0   : Balanced
  < 1.0   : Inventory Drawdown / Potential Under-Buying
```

Constraints (IW-GAP-005, IW-TQ-005, IW-OQ-001):

- Investigation-only; **never** a ranking, performance, or financial KPI.
- No new Principal KPI ID is introduced.
- No KPI ownership, registry maintenance, ranking contract, or evidence-route ownership is created.
- Evidence ownership stays with source providers and existing contracts.

### 8.4 Lens-specific metrics

| Lens | KPIs / metrics |
| --- | --- |
| Sales-Out | `PRN-SALES-001`, `PRN-GRW-001/002`, `PRN-RET-001..004`, `PRN-TGT-002/003`, `PRN-CUS-001/002` |
| Purchasing | `PRN-PUR-001`, `PU-KPI-001` history (trend), Purchase-to-Sales-Out Ratio, `PRN-INV-001/002` |

No KPI semantics change. `PRN-SALES-001` remains the default ranking KPI; `PRN-RET-004`,
`PRN-TGT-003`, `PRN-GRW-001`, `PRN-GRW-002` are supporting ranking KPIs only.

### 8.5 KPI lineage expectations

- Every displayed KPI traces to a catalog ID and to its persisted projection or the reused
  `PU-KPI-001` history.
- Sales-Out lineage: Faktur Item (`PRN-SALES-001 = SUM(FakturItem.SubTotal - FakturItem.DiscRp)`),
  attributed through `FakturItem.BrgId → BTR_Brg.SupplierId` (immutable Item master, MIG-GAP-004).
- Purchasing lineage: Purchase Invoice detail and `PU-KPI-001` monthly history, `SupplierId`-keyed.
- Returns lineage: Return Item; returns never reduce Sales-Out.
- Growth lineage: derived only from stored `PRN-SALES-001` history (never from purchase growth).

---

## 9. Relationship Architecture

### 9.1 Principal relationships

| Relationship | Source | Lineage | Investigation usage |
| --- | --- | --- | --- |
| **Principal → Customer** | `BTRPD_CustomerPrincipalRelationship` | Transaction-derived from item sales (MIG-GAP-003); projection-sourced, never recomputed | Customer Contribution, Customer Reach, Top Customers |
| **Principal → Salesman** | `BTRPD_PrincipalContribution` | Salesman contribution by sales-out | Salesman Contribution, Top Salesmen |
| **Principal → Item** | item-line sales rollup | `FakturItem → Brg.SupplierId` (immutable Item master, MIG-GAP-004) | Item Contribution, Top Items |
| **Principal → Purchasing Activity** | Purchase Invoice Detail + `PU-KPI-001` history | `SupplierId`-keyed (IW-GAP-016) | Top Purchased Items, Purchase History |

### 9.2 Ownership and lineage

- The Principal → Customer relationship existence, last transaction date, Active/Dormant status,
  and pair-attributed `PRN-SALES-001` are **read from the projection**, never recomputed from raw
  transactions (developer guide §Supplier pack).
- Active/Dormant status uses the 6-month last-transaction rule (MIG-TQ-006). History is retained
  indefinitely.
- Pair Sales-Out is not reduced by returns.
- There is no Customer–Principal entity type and no master assignment table (MIG-GAP-003).

### 9.3 Sales-Out relationship drivers

Reused existing drivers (already implemented): Top Customers, Top Salesmen, Top Items by
sales-out (`PRN-SALES-001`).

### 9.4 Purchasing relationship drivers

Approved drivers (IW-GAP-008, IW-BQ-010):

```text
1. Top Purchased Items   — from Purchase Invoice Detail
2. Purchase History      — from Monthly Purchase Amount history (PU-KPI-001)
```

Explicitly **excluded**: PO-based relationships, purchasing-user relationships, and
procurement-workflow relationships (IW-BQ-010).

### 9.5 Relationship boundary ownership

| Element | Owner |
| --- | --- |
| Relationship definitions | `SupplierRelationshipCatalog` (extended with purchasing drivers) |
| Relationship values | `SupplierEntityAnalyticsProducer` (single writer) |
| Relationship presentation | Investigation workspace Explanation Stage, lens-scoped |

---

## 10. Evidence Architecture

### 10.1 Sales-Out evidence model

| Evidence | Route / grain | Source |
| --- | --- | --- |
| Principal Sales-Out | Faktur Item (canonical grain, MIG-TQ-009) | Sales report evidence |
| Returns | Return Item | Return evidence |
| Customer Contribution | Customer–Principal pair | `BTRPD_CustomerPrincipalRelationship` |
| Salesman Contribution | Salesman contribution | `BTRPD_PrincipalContribution` |

### 10.2 Purchasing evidence model

| Evidence | Route / grain |
| --- | --- |
| Purchase Amount | Purchase Invoice |
| Purchase Trend | Purchase History (`PU-KPI-001` monthly) |
| Inventory Exposure | Inventory report (`/reports/inventory`) |
| Stock Risk | Inventory risk breakdown |

### 10.3 Evidence routing

- `SupplierEntityAnalyticsEvidenceResolver` resolves evidence for Sales-Out (Faktur Item for
  `PRN-SALES-001`, Return Item for return KPIs) and Purchasing/Inventory reports.
- Purchase/inventory evidence is **never** presented as Sales-Out evidence.

### 10.4 Evidence lineage

All Principal evidence resolves identity via `SupplierId` (IW-GAP-016). Evidence lineage
references `SupplierId`, never name-matched data. Evidence ownership remains with source KPI
providers and existing evidence contracts (IW-OQ-001).

### 10.5 Lens-scoped evidence presentation

All Principal evidence is associated with the active lens (IW-GAP-013):

- Sales-Out lens evidence: Faktur Item, Return Item, Customer Contribution, Salesman Contribution.
- Purchasing lens evidence: Purchase Invoice, Purchase History, Inventory Exposure, Inventory
  Movement.
- Evidence panels, links, titles, and disclosures **indicate the originating lens**.
- There is **no duplicate evidence model** — one resolver, lens-scoped presentation.

Explainability is preserved by the evidence chain: Population position → KPI facts → peer
normality → trend → signals → relationship drivers → source report (M32R §7.1). Every conclusion
formed in the workspace remains traceable to a source report.

---

## 11. Snapshot & Analytics Architecture

### 11.1 Reused snapshots and projections

The Sales-Out lens requires no new tables — all `BTRPD_Principal*` projections already exist
(IW §4.2).

| Projection | Purpose |
| --- | --- |
| `BTRPD_PrincipalSalesOut` (+`Kpi`, `DataQuality`) | `PRN-SALES-001` current + data-quality output |
| `BTRPD_PrincipalSalesOutHistory` | Sales-Out monthly history (growth basis) |
| `BTRPD_PrincipalReturn`, `BTRPD_PrincipalReturnPercentage`, `BTRPD_PrincipalReturnHistory` | Returns KPIs |
| `BTRPD_PrincipalTarget` | `PRN-TGT-001` |
| `BTRPD_PrincipalAchievement` | `PRN-TGT-002/003` |
| `BTRPD_PrincipalMomGrowth`, `BTRPD_PrincipalYoyGrowth` | Growth KPIs |
| `BTRPD_PrincipalActiveCustomer`, `BTRPD_PrincipalCustomerCoverage` | Customer reach |
| `BTRPD_CustomerPrincipalRelationship` | Principal → Customer relationship + pair `PRN-SALES-001` |
| `BTRPD_PrincipalContribution` | Principal × Salesman contribution |
| `BTRPD_PrincipalPurchaseIn` | `PRN-PUR-001` |
| `BTRPD_PrincipalInventory` | `PRN-INV-001/002` |
| `BTRPD_InventoryBreakdown.SupplierId`, `BTRPD_InventoryRiskBreakdown.SupplierId` | Inventory exposure / stock risk by Principal |

Entity Analytics L0–L5 generic layers hold the composed Supplier/Principal profile.

### 11.2 New projections

**None required** for the Sales-Out lens. No separate Principal purchase-history projection is
introduced (IW-GAP-007). Possible additions would only arise from purchasing-lens KPIs beyond the
resolved set — none are approved.

### 11.3 Historical trend architecture

- Sales-Out trend: `BTRPD_PrincipalSalesOutHistory` (stored `PRN-SALES-001` history).
- Growth: derived from stored `PRN-SALES-001` history only.
- Purchase trend: reused `PU-KPI-001` monthly history via shared `SupplierId` identity (IW-GAP-007).
  `PRN-PUR-001` remains current-period only. A future dedicated Principal purchase-history series
  remains optional.

### 11.4 Refresh boundaries

- Single writer: `SupplierEntityAnalyticsProducer` (`EntityId = SupplierId`).
- Worker trigger: `RefreshDashboardPurchasingManagementSnapshotWorker` (Purchasing Management
  domain hook).
- Refresh order: source snapshots and `BTRPD_CustomerPrincipalRelationship` complete **before** the
  Principal Entity Analytics refresh (developer guide §Supplier pack).
- The producer reads owned snapshots only; it never recomputes `PRN-SALES-001` from Purchasing
  Management in-memory values or from purchase data (MIG-GAP-011).
- A purchase or inventory refresh never erases persisted `PRN-SALES-001`.
- Cross-domain composition occurs under a **single Entity Analytics refresh/freshness contract**
  (MIG-TQ-007); each source domain remains authoritative for its own KPI calculations; Entity
  Analytics is composition + presentation only. No producer-model extension is required.

### 11.5 Freshness boundaries

Standard freshness policy (IW-OQ-002): every view displays `Generated At`, `Reporting Period`, and
`Data Snapshot Period` (when applicable). No real-time guarantee; all outputs are interpreted
relative to the displayed snapshot.

---

## 12. Navigation Architecture

### 12.1 Entry points

| Entry | Target | Notes |
| --- | --- | --- |
| Entity Analytics home → Principal | Principal Investigation Workspace (Discovery Mode, default Sales-Out lens) | Replaces the Supplier Investigation entry (IW-BQ-005) |
| `SA04` Principal Performance row | Principal Investigation Workspace (Investigation Mode, entity pre-selected) | Cross-navigation |
| `PU01`/`PU02` supplier row | Principal Investigation Workspace (Purchasing lens or entity profile) | Cross-navigation |
| `EX01`/`EX02` attention → Principal | Principal Investigation Workspace (entity pre-selected) | Alert-driven entry |

The workspace preserves the generic entry modes (Population, Alert-driven, Profile-direct,
Report-driven, Cross-entity) defined in M32R §2.1.

### 12.2 Principal Investigation vs Principal Performance

| Surface | Navigation path | Responsibility |
| --- | --- | --- |
| Principal Investigation Workspace | Entity Analytics (`EX03`) → Principal | Investigate one Principal |
| Principal Performance Dashboard | Sales (`SA04`) | Manage / rank / monitor Principals |

The two are separate navigation entries; neither is hidden behind the other. The workspace is the
investigation surface; `SA04` is the analytical dashboard. They cross-navigate but do not merge
responsibilities.

### 12.3 Cross-navigation rules

- Workspace → `SA04`: the Principal Performance dashboard remains the ranking/management path;
  the workspace does not absorb ranking dashboards.
- Workspace → `PU01`/`PU02`: purchasing context links remain available; the Purchasing lens does
  not replace the purchasing dashboards.
- Related-entity navigation (Business Drivers → Customer/Salesman/Item investigation) follows the
  existing cross-entity navigation model (M32R §2.3), with investigation continuity preserved.

### 12.4 Evidence navigation rules

Evidence links navigate to source reports pre-filtered to the selected Principal and the active
lens context. Lens scoping ensures purchase evidence is never reachable through a Sales-Out
evidence link and vice versa (IW-GAP-013).

### 12.5 Breadcrumb

```text
Entity Analytics / Principal / [Map Preset] / [Principal Name]
```

The breadcrumb reflects the active lens; the preset name reflects the active lens's default
(`principal-sales-out-map` for Sales-Out).

---

## 13. Data Health Architecture

A dedicated **Data Health** section is visible regardless of active lens (IW-OQ-003). Both
conditions below affect interpretation and must be visible before conclusions are drawn.

### 13.1 Indicators

| Indicator | Source | Purpose |
| --- | --- | --- |
| Target Coverage % | `BTRPD_PrincipalTarget` vs Sales-Out population | Disclose that achievement is partial (only 2026-06 targets exist in baseline) |
| Principals Missing Target Count | Target coverage output | Disclose missing target responsibility |
| Unknown Principal Exception Count | `BTRPD_PrincipalSalesOut` data-quality output (`Unknown Principal` signal) | Disclose identity-resolution exceptions |
| Unknown Principal Exception Amount | Same | Disclose value of unresolved attribution |

### 13.2 Unknown Principal exceptions

- Surface the `Unknown Principal` exception count and amount with navigation to supporting
  evidence (IW-OQ-003).
- Unknown Principal is an exception to be disclosed, not a synthetic Principal (baseline Check 6
  measured 0% unknown in the profiled source; the disclosure mechanism remains required).
- Unknown-Principal handling must never silently drop Principal Sales-Out.

### 13.3 Data completeness and quality indicators

- Data completeness: Target Coverage % and Missing Target Count disclose incomplete responsibility
  assignment.
- Data quality: Unknown Principal count/amount disclose attribution failures.
- Freshness disclosure (IW-OQ-002) is part of Data Health: every view states the `Generated At`,
  `Reporting Period`, and `Data Snapshot Period`, with the standard disclosure that the workspace
  reflects the latest available snapshot.

---

## 14. Migration Architecture

### 14.1 Supplier Workspace evolution strategy

The current Supplier Investigation entry **evolves into** the Principal Investigation Workspace
(in-place replacement, not parallel) (IW-BQ-005, Option A):

- Keep `EntityTypeCode.Supplier`, snapshot keys, routes, catalogs, and L0–L5 history.
- Change only user-facing identity to "Principal" and add lens presets/grouping.
- No parallel Supplier/Principal entries.

### 14.2 Principal Workspace introduction strategy

- The workspace is introduced as a **two-lens configuration** of the existing entity-neutral
  workspace (IW-GAP-002).
- Sales-Out lens is the default; Purchasing lens is available via the lens switcher with no
  purchasing capabilities removed (IW-GAP-003).

### 14.3 Backward compatibility requirements

| Concern | Requirement |
| --- | --- |
| Database | No migration; `SupplierId` retained; no new tables for Sales-Out lens |
| Entity type | No entity-type migration; no new `Principal` entity type |
| API | Preferable no change; internal `Supplier` identifiers retained |
| Routes | Technical routes retain Supplier form; display shows Principal |
| Snapshots | No snapshot migration; `BTRPD_Principal*` and L0–L5 reused |
| KPI semantics | No semantic change; `PRN-SALES-001` authoritative |

### 14.4 Legacy compatibility requirements

- **Legacy desktop: no change.** Principal is a presentation alias; desktop continues on Supplier
  identity.
- **No Purchasing Order domain** is introduced; purchasing uses invoice-based purchasing records
  (IW-GAP-006).
- **Radar retirement is platform-wide** — the shared signature surface is retired once for all
  entity types (ADR-EA-001); no entity profile or evidence route depends on it afterward.
- **Peer Group Selector remains hidden** for Principal (IW-GAP-011).

---

## 15. Architecture Decisions (ADR Section)

Consolidated architectural decisions derived from the approved feasibility decisions.

### ADR-PIW-001 — Principal is a presentation alias of Supplier

- **Decision:** Supplier remains the canonical domain, database, integration, and legacy identity;
  `Principal` is a presentation alias. `SupplierId` remains the authoritative technical identifier.
  No new Principal master entity, entity type, producer, profile, or master data is introduced.
- **Rationale:** Reuse of the single-writer Supplier/Principal pack avoids duplicate profiles and
  preserves snapshot keys, routes, and history (IW-GAP-001, MIG-GAP-011, Option A).
- **Consequences:** All Principal surfaces resolve to `SupplierId`; discipline is required to keep
  "Principal" user-facing and "Supplier" technical; no database, API, snapshot, or domain
  migration.

### ADR-PIW-002 — Single workspace with two lenses

- **Decision:** The Principal Investigation Workspace exposes two presentation lenses (Sales-Out,
  Purchasing) over the same Principal in a single workspace via a lens switcher.
- **Rationale:** A lens is a presentation perspective, not an entity type or workspace; one
  workspace avoids duplicate navigation, profiles, and master data (IW-GAP-002).
- **Consequences:** Lens-specific KPI grouping, presets, attention, drivers, and evidence; the
  investigation model and platform are unchanged.

### ADR-PIW-003 — Sales-Out lens is the default

- **Decision:** The workspace defaults to the Sales-Out lens via the `principal-sales-out-map`
  preset; Purchasing remains available through the lens selector.
- **Rationale:** Principal-centric analytics is driven by commercial performance; purchasing is a
  supporting investigation perspective (IW-GAP-003).
- **Consequences:** Default population map is Sales-Out-led; later presets must not change default
  behavior.

### ADR-PIW-004 — Lens-scoped evidence presentation

- **Decision:** All Principal evidence is associated with the active lens; panels, links, titles,
  and disclosures indicate the originating lens. No duplicate evidence model.
- **Rationale:** Prevents purchase evidence being presented as sales-out and preserves
  explainability (IW-GAP-013).
- **Consequences:** One evidence resolver with lens-scoped routing; review must verify lens
  labeling on every evidence surface.

### ADR-PIW-005 — No new KPI IDs; derived investigation metrics only

- **Decision:** No new Principal KPI IDs. The Purchase-to-Sales-Out Ratio is a derived investigation
  metric from existing measures; no purchasing efficiency KPI is introduced. Existing KPI semantics
  remain unchanged.
- **Rationale:** Avoids registry expansion, KPI stewardship, ranking contracts, and evidence-route
  ownership (IW-TQ-005, IW-OQ-001, IW-GAP-005).
- **Consequences:** Investigation-only metrics require no ownership; evidence ownership stays with
  source providers.

### ADR-PIW-006 — Invoice-based purchasing; no PO domain

- **Decision:** Purchasing is modeled from invoice-based purchasing records; efficiency is Inventory
  Replenishment Efficiency. No Purchase Order domain or PO-based metrics (Fulfillment, Backlog,
  Outstanding PO) are introduced.
- **Rationale:** No formal Purchase Order workflow exists (IW-GAP-006).
- **Consequences:** Purchasing drivers and trend use Purchase Invoice Detail and `PU-KPI-001`
  history only.

### ADR-PIW-007 — All-active peer population; selector hidden

- **Decision:** The comparison population is the existing all-active Principal population
  (`supplier-all-active`); the Peer Group Selector remains hidden for Principal.
- **Rationale:** Small population; all-active comparison is sufficient; extra dimensions are low
  value vs cost (IW-GAP-011).
- **Consequences:** Peer Position available against all-active population; no peer dimensions.

### ADR-PIW-008 — SupplierId-keyed identity everywhere

- **Decision:** All Purchasing Lens aggregations, relationships, snapshots, trends, and evidence use
  `SupplierId`-keyed data; `SupplierName` is presentation only; name-based aggregation/matching/
  joining is prohibited.
- **Rationale:** `SupplierId` is the authoritative identity across all investigation capabilities
  (IW-GAP-016).
- **Consequences:** Identity resolution is stable; historical display may show recorded
  `SupplierName` without affecting resolution.

### ADR-PIW-009 — Standard freshness policy

- **Decision:** Every view displays `Generated At`, `Reporting Period`, and `Data Snapshot Period`
  (when applicable); no real-time guarantee; outputs interpreted relative to the snapshot.
- **Rationale:** Stale data must not be misread as current (IW-OQ-002).
- **Consequences:** Uniform freshness disclosure across every workspace view.

### ADR-PIW-010 — Data Health disclosures

- **Decision:** A lens-independent Data Health section discloses Target Coverage %, Principals
  Missing Target Count, Unknown Principal Exception Count/Amount, with evidence navigation.
- **Rationale:** Low target coverage and unknown-Principal exceptions affect interpretation and must
  be visible before conclusions (IW-OQ-003).
- **Consequences:** Interpretation is guarded; unknown-Principal is never silently dropped.

### ADR-PIW-011 — Terminology governance

- **Decision:** Business-facing surfaces use Principal; technical artifacts use Supplier. Standards
  owned by the Product Owner.
- **Rationale:** Consistent user-facing terminology without technical renaming (IW-GAP-017,
  MIG-GAP-017).
- **Consequences:** A documented mapping applied across surfaces; technical identifiers retained.

### ADR-PIW-012 — Radar (Performance Signature) retired

- **Decision:** The Performance Signature (Radar) is retired from Entity Analytics for all entity
  types; no Principal or dual-lens signature and no replacement radar.
- **Rationale:** Investigation relies on KPI Summary, Population Position, Trajectory, Business
  Drivers, Relationships, Attention, and Evidence (IW-GAP-010, ADR-EA-001).
- **Consequences:** Shared signature surface retired once; L5 history retained (not deleted);
  planning must verify no profile/evidence route depends on the retired surface.

---

## 16. Architecture Risks

| Risk | Impact | Mitigation |
| --- | --- | --- |
| Purchase-in conflated with Sales-Out in a combined workspace | High | Lens separation, labels, and lens-scoped evidence routes; purchase never ranked as performance (IW-GAP-010) |
| Terminology drift between docs and UI | Low | Governance rule (IW-GAP-017); Product Owner owns standards; apply mapping consistently |
| Low target coverage biases Achievement lens | Medium | Data Health disclosures (IW-OQ-003): Target Coverage %, Missing Target Count |
| Name-based purchasing identity mismatch | Low | `SupplierId`-keyed data only (IW-GAP-016) |
| Radar retirement leaves dependent surfaces | Medium | Retire shared surface once; verify no profile/evidence route depends on it (ADR-EA-001) |
| Stale data misread as current | Medium | Standard freshness policy (IW-OQ-002) |
| Map bubble size/color encoding unsupported by current canvas | Low | Validate against current Population Map capability during planning (IW §3.2 note) |

---

## 17. Implementation Guidance

### Backend constraints

- Entity registration metadata (`EntityAnalyticsPlatformRegistrar`): display mapping only;
  technical registration (`Supplier`) unchanged.
- `EntityMapPresetRegistry`: add/adjust Principal lens presets (default `principal-sales-out-map`).
- `SupplierEntityAnalyticsProducer`: no new Sales-Out analytics; purchasing-lens presentation may
  compose already-stored KPIs. Never recompute `PRN-SALES-001` from purchase data; never erase it
  on purchase/inventory refresh.
- `SupplierAttentionSignalCatalog`: expand per IW-GAP-009 (five Sales-Out categories with EX01/EX02
  reuse); purchase/inventory signals retained; no new attention engine.
- `SupplierRelationshipCatalog`: add purchasing drivers (Top Purchased Items, Purchase History);
  no PO-based/user/procurement relationships.
- `SupplierEntityAnalyticsEvidenceResolver`: lens-scoped presentation; no duplicate evidence model.
- `PeerGroupResolver`: unchanged — `supplier-all-active` retained; no Principal peer dimension.
- No Principal-scoped authorization (MIG-GAP-018).

### Frontend constraints

- Apply the IW-GAP-012 display mapping (Principal Investigation Workspace, Principal Profile,
  Compare Principals, Principal Relationships, Principal Evidence) across metadata, navigation,
  labels, breadcrumb, and scope labels; internal identifiers unchanged.
- Lens switcher + lens-scoped KPI grouping in the workspace shell and profile sections.
- Peer Group Selector remains hidden for Principal.
- Population Map tooltip hides the dimension row and generic "Category" label (IW-GAP-015).
- Retire the Performance Signature from the Validation panel and related radar/compare views for
  all entity types (ADR-EA-001).
- No redesign of the generic workspace components (PopulationMapCanvas, panels) is assumed.

### Critical invariants

- Exactly one authoritative Principal profile; one investigation entry (Supplier entry replaced,
  not duplicated).
- Purchase-in is never presented or ranked as Principal sales performance.
- Returns never reduce `PRN-SALES-001`.
- No Principal financial/credit/collection KPI is introduced.
- No PO-based metric is introduced (including re-verifying the existing Qualified Backlog signal
  against the no-PO-metrics rule).
- All Principal aggregations resolve identity via `SupplierId`; `SupplierName` presentation only.
- Every evidence panel, link, title, and disclosure indicates its originating lens.
- Every view displays `Generated At`, `Reporting Period`, and `Data Snapshot Period`.

### Prohibited shortcuts

- Do not introduce a new `Principal` entity type or a second producer over the same snapshots.
- Do not relabel purchase growth as sales growth.
- Do not create a dual-lens Performance Signature or any replacement radar.
- Do not enable a Principal Peer Group Selector.
- Do not add Principal KPI IDs or a new KPI stewardship model.
- Do not name-match, aggregate, or join purchasing data by `SupplierName`.
- Do not present purchase evidence through Sales-Out evidence routes (and vice versa).

---

## 18. Architecture Readiness

### Ready For Planning

```text
YES
```

All blocking feasibility decisions are resolved (IW-GAP-001..GAP-017, BQ-005, BQ-010, TQ-005,
OQ-001..OQ-004; MIG-GAP-001..GAP-023; ADR-EA-001). No open business decisions remain.

### Blocking Issues

```text
None.
```

### Planner Guidance

Expected implementation areas (not a task breakdown):

- Principal identity and terminology mapping (display/navigation/labels; no technical renaming).
- Lens model and lens switcher (Sales-Out default, Purchasing available).
- `principal-sales-out-map` preset and lens presets.
- Lens-scoped Current Facts, Context, Attention, Explanation, and Evidence configuration.
- Data Health section (Target Coverage %, Missing Target Count, Unknown Principal count/amount).
- Platform-wide Performance Signature retirement.

Major dependencies:

- Approved feasibility decisions (authoritative).
- Existing `BTRPD_Principal*` projections and `BTRPD_CustomerPrincipalRelationship`.
- Single-writer `SupplierEntityAnalyticsProducer` and the Purchasing Management worker refresh
  ordering.
- Sales-Out vs purchase-in separation (IW-GAP-010, MIG-GAP-010).

Sequencing considerations (capability order only, not a plan):

- Identity/terminology first; then lens model/presets; then context/explanation/evidence; then
  Data Health and platform-wide radar retirement verification.

---

## Document control

| Version | Date | Author | Change |
| --- | --- | --- | --- |
| 1.0 | 2026-09-11 | Architect | Initial Principal Investigation Workspace architecture, derived from approved feasibility decisions |
