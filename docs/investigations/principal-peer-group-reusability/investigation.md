# Investigation: Peer Group Reusability for a Principal Investigation Workspace

**Date:** 2026-09-11
**Type:** Capability / reusability investigation (as-is)
**Objective:** Determine whether the existing Entity Analytics Peer Group infrastructure is truly generic and reusable for a future Principal Investigation Workspace.
**Constraint:** Investigation only — no implementation proposal. Evidence-backed determination only.

**Related artifacts:**

- `docs/features/entity-analytics/entity-analytics-developer-guide.md`
- `docs/features/entity-analytics/peer-position-distribution.md`
- `docs/work/btr-portal/principal-investigation-workspace/FEASIBILITY-ASSESSMENT.md`
- `docs/work/btr-portal/principal-investigation-workspace/adrs/ADR-EA-001-radar-retirement.md`

**Decision context used:** GAP-001 (2026-09-11) resolves that **Supplier remains the canonical technical entity** and **Principal is a presentation alias** for the same `SupplierId`; no new Principal entity type is introduced. Therefore "Principal Investigation Workspace" = the existing `Supplier` entity type presented as Principal.

---

## Verdict Summary

| # | Question | Determination |
| --- | --- | --- |
| 1 | Generic vs Customer-specific? | **Engine is generic; rule definitions and dimension mapping are hardcoded and partly entity-specific.** |
| 2 | Participating components | See inventory in §2. |
| 3 | Entity types supporting Peer Group | Customer, Item (dimension-based); Salesman, Supplier (all-active only). All four are registered. |
| 4 | What "disables" Peer Group for Supplier | Nothing disables peer-distribution. Only the **selector** and **dimension-based grouping** are absent (`PeerGroupRuleCatalog` has one Supplier rule; UI gates selector to Customer/Item). |
| 5 | Principal enablement path | **Configuration/registration only is NOT sufficient for a dimension-based peer group.** The all-active peer group already works with zero changes; anything richer requires code changes. |
| 6 | Entity-specific peer-group definitions? | Supported structurally (`EntityType`, per-registration default) but **defined in hardcoded code**, not data/config. |
| 7 | Architectural blockers | Engine is reusable; the closed/hardcoded catalog, resolver switch, and UI gate are the constraints. No true platform blocker. |
| 8 | Estimated complexity | **Moderate enhancement** for a meaningful Principal peer group; **No work** if all-active is acceptable. |

---

## 1. Is Peer Group generic or Customer-specific?

**Determination: a generic engine wrapped by hardcoded, entity-specific rule definitions.**

Evidence of generic design:

- `EntityPeerDistributionEngine.BuildDistribution` is fully entity-neutral. It validates the entity type, resolves the effective rule, loads population, builds peer index, computes ranking and bins — with no Customer-specific branching.
  `src/j05-btr-distrib/btr.application/ReportingContext/EntityAnalyticsAgg/Services/EntityPeerDistributionEngine.cs:29`
- The API surface is entity-type parameterized:
  `GET /api/entity-analytics/peer-distribution` and `GET /api/entity-analytics/peer-group-rules` accept `entityType`.
  `src/j05-btr-distrib/btr.portal.api/Controllers/EntityAnalytics/EntityAnalyticsController.cs:150,173`
- `EntityTypeRegistration.PeerGroupRuleId` is a per-entity-type registration field.
  `src/j05-btr-distrib/btr.application/ReportingContext/EntityAnalyticsAgg/Models/EntityTypeRegistration.cs:17`

Evidence of Customer-specific coupling (the "non-generic" part):

- `PeerGroupRuleCatalog` is a **hardcoded static array** of rules keyed by `EntityType`. Adding any rule is a code edit to this platform class.
  `src/j05-btr-distrib/btr.application/ReportingContext/EntityAnalyticsAgg/Services/PeerGroupRuleCatalog.cs:11-61`
- `PeerGroupResolver` hardcodes each rule id and the dimension-KPI mapping:
  `src/j05-btr-distrib/btr.application/ReportingContext/EntityAnalyticsAgg/Services/PeerGroupResolver.cs:11-33`
  Dimension grouping is applied only to Customer and Item rule ids; Salesman/Supplier fall to an "all active" branch.
  `PeerGroupResolver.cs:46-66`
- `PeerGroupLabelFormatter` and the Vue `peerGroupLabel.ts` both enumerate rule ids/entity types explicitly.
  `PeerGroupLabelFormatter.cs:20-38`; `btr.portal.web/src/services/peerGroupLabel.ts:15-60`
- The UI selector gate is hardcoded to Customer/Item:
  `showPeerGroupSelector = (entityType === 'Customer' || entityType === 'Item') && rules.length > 1`
  `btr.portal.web/src/stores/investigationWorkspaceStore.ts:103-106`

**Conclusion:** the processing/serving layer is a genuine generic capability. The *rule definition and presentation* layer is a closed, code-defined catalog with explicit entity branches — the exact place where Customer/Item specificity lives.

---

## 2. Components, services, stores, and controllers participating

Backend (application): `PeerGroupRuleCatalog`, `PeerGroupResolver`, `PeerGroupLabelFormatter`, `EntityPeerDistributionEngine`, `PeerGroupRuleDefinition`, `PeerGroupResolution`
`btr.application/ReportingContext/EntityAnalyticsAgg/Services/`, `.../Models/`

Backend (registration / metadata): `EntityTypeRegistration.PeerGroupRuleId`, `EntityAnalyticsPlatformRegistrar`, `IEntityTypeRegistry`
`EntityAnalyticsPlatformRegistrar.cs:14-57`

Backend (queries / contracts): `GetPeerDistributionQuery`/handler, `GetPeerGroupRulesQuery`/handler
`btr.application/ReportingContext/EntityAnalyticsAgg/Queries/GetInvestigationWorkspaceQueries.cs:105-186`

Backend (API): `EntityAnalyticsController.GetPeerGroupRules`, `GetPeerDistribution`
`EntityAnalyticsController.cs:150,173`

Backend (persistence): L5 `PeerGroupRuleId`/`PeerGroupSize` in `EntityAnalyticsRepository` and `EntityAnalyticsRadarScoreRow`
`btr.infrastructure/.../EntityAnalyticsRepository.cs:1054`; `btr.application/.../Models/Snapshot/EntityAnalyticsRadarScoreRow.cs:22`

DI: `IEntityPeerDistributionEngine` → `EntityPeerDistributionEngine`
`btr.portal.api/Configurations/ApplicationPortalExtensions.cs:163`

Frontend (API/model): `fetchPeerGroupRules`, `fetchPeerDistribution`, `PeerGroupRule(sResponse)` types
`btr.portal.web/src/api/entityAnalyticsApi.ts:118`; `src/models/entityAnalytics.ts:460-470`

Frontend (state): `useInvestigationWorkspaceStore` (`peerGroupRuleId`, `peerGroupRules`, `showPeerGroupSelector`, `loadPeerGroupRules`)
`src/stores/investigationWorkspaceStore.ts:41-44,103-106,152-175`

Frontend (UI): `PeerGroupSelector.vue`, `PeerPositionPanel.vue`, `InvestigationWorkspaceView.vue`
`src/components/entity-analytics/workspace/`; `src/views/analytics/InvestigationWorkspaceView.vue:283-303`

Frontend (URL/services): `investigationWorkspaceUrl.ts` (`?peerGroup=`), `peerGroupLabel.ts`, `investigationWorkspaceNavigation.ts`
`src/services/investigationWorkspaceUrl.ts:33,43`; `src/services/peerGroupLabel.ts`

Other consumers of the same rules: `EntityRadarEngine` (L5) and `EntityPopulationMapEngine` (default rule from registration)
`EntityRadarEngine.cs:51-58,329-334`; `EntityPopulationMapEngine.cs:55`

---

## 3. Which entity types currently support Peer Group?

All four entity types are **registered** with a default peer-group rule, and the generic peer-distribution endpoint accepts all four.

| Entity type | Registered default rule | Selectable rules | Dimension-based grouping | Workspace selector |
| --- | --- | --- | --- | --- |
| Customer | `customer-wilayah` | `customer-wilayah`, `customer-klasifikasi` | Yes | Shown |
| Salesman | `salesman-all-active` | `salesman-all-active` only | No (all-active) | Hidden |
| Supplier | `supplier-all-active` | `supplier-all-active` only | No (all-active) | Hidden |
| Item | `item-principal` | `item-principal`, `item-category` | Yes | Shown |

Evidence:

- Registrations: `EntityAnalyticsPlatformRegistrar.cs:14-57`
- Catalog: `PeerGroupRuleCatalog.cs:11-61` (Customer/Item have two rules; Salesman/Supplier one each)
- Selector gate: `investigationWorkspaceStore.ts:103-106`
- Supplier all-active handling: `PeerGroupResolver.cs:54-66`; `PeerGroupLabelFormatter.cs:35-36`

Note: "Hidden selector" does not mean "no peer group". `PeerPositionPanel` still renders and the engine falls back to the registered default `supplier-all-active` when the UI passes no override (`GetDefaultRuleId` → `EntityTypeRegistration.PeerGroupRuleId`).
`GetInvestigationWorkspaceQueries.cs:83-94`; `PeerGroupRuleCatalog.cs:83-94`

---

## 4. What technical or product rules currently "disable" Peer Group for Supplier?

**Precise finding: Peer Group is not disabled for Supplier.** The peer-distribution capability already works for Supplier via `supplier-all-active`. What is absent is (a) the UI selector and (b) dimension-based grouping.

Technical facts:

1. Supplier **is** registered with a peer rule: `PeerGroupRuleId = "supplier-all-active"`.
   `EntityAnalyticsPlatformRegistrar.cs:47-56`
2. `PeerGroupRuleCatalog` defines exactly **one** Supplier rule, with `DimensionLabel = null`.
   `PeerGroupRuleCatalog.cs:53-60`
3. `PeerGroupResolver.ResolveDimensionKpiId("supplier-all-active")` returns `null`, so `BuildPeerGroupIndex` uses the whole active population (no dimension split).
   `PeerGroupResolver.cs:18-33,54-66`
4. Supplier map presets carry `FilterDimensionKpiId = null`.
   `EntityMapPresetRegistry.cs:76-96`
5. The frontend selector is hidden because it is gated to Customer/Item:
   `investigationWorkspaceStore.ts:103-106`
6. When only one rule exists, the store nulls the selection and the panel uses the registration default:
   `investigationWorkspaceStore.ts:168-170`
7. The FEASIBILITY-ASSESSMENT states this explicitly: "All-active only; no dimension-based Principal peer group; selector hidden for Supplier in the UI" (GAP-011).
   `FEASIBILITY-ASSESSMENT.md:184,244`

Product rule implication: because only all-active grouping exists, every Supplier/Principal is compared against the entire Principal population, which is small (baseline ≈27 principals per `FEASIBILITY-ASSESSMENT.md`), limiting the usefulness of peer position. That is a product/meaning constraint, not a code gate.

---

## 5. If Principal replaces Supplier as the investigation entity

Given GAP-001 (Principal is a presentation alias; `SupplierId` stays authoritative; no new entity type), Principal inherits the Supplier entity registration.

- **All-active peer group:** already enabled today with **no code or config change**. `supplier-all-active` is registered and reachable through `peer-distribution`.
- **Dimension-based Principal peer group (e.g., by region/category/principal class):** **not achievable through configuration/registration alone.** It requires:
  1. Adding a rule (and its dimension KPI id) to the hardcoded `PeerGroupRuleCatalog` / `PeerGroupResolver` — code.
  2. A dimension KPI being produced onto the Supplier L0 profile — producer code.
  3. Opening the frontend selector gate in `investigationWorkspaceStore.ts` — code.
  4. Label support in `PeerGroupLabelFormatter` / `peerGroupLabel.ts` — code.

There is no externalized peer-group configuration file, database table, or registration hook that accepts a new rule definition as data. `EntityTypeRegistration` stores only the single default rule id string, not the rule set.

**Answer:** configuration/registration only is insufficient for anything beyond the existing all-active rule; code changes are required for a meaningful Principal peer dimension.

---

## 6. Does the infrastructure support entity-specific peer-group definitions?

**Yes structurally, no via configuration.**

- `PeerGroupRuleDefinition` is keyed by `EntityType`, so multiple entity types and multiple rules per entity type are representable.
  `src/j05-btr-distrib/btr.application/ReportingContext/EntityAnalyticsAgg/Models/PeerGroupRuleDefinition.cs:5-13`
- `PeerGroupRuleCatalog.GetRulesForEntityType` and `TryResolveRule` filter by entity type.
  `PeerGroupRuleCatalog.cs:63-81`
- `EntityTypeRegistration.PeerGroupRuleId` supplies a per-entity default.
  `EntityTypeRegistration.cs:17`; `PeerGroupRuleCatalog.cs:83-94`

However, the rule set is a `static readonly` C# array. There is no registry interface (unlike KPI/dimension registries), no bootstrap, and no config binding. Entity-specific definitions therefore exist but are **compiled in**.

---

## 7. Architectural blockers preventing Principal from using Peer Group

No hard platform blocker exists; the engine itself is reusable as-is. The relevant constraints are:

1. **Closed static catalog.** `PeerGroupRuleCatalog` and `PeerGroupResolver` are static classes with hardcoded rules and a dimension-KPI switch. Extending peer grouping for a new dimension/entity requires editing platform code (open/closed tension).
   `PeerGroupRuleCatalog.cs:11-61`; `PeerGroupResolver.cs:18-33`
2. **No Supplier/Principal peer dimension KPI.** Supplier registrations expose no `EA-DIM-*` peer dimension (only `EA-DIM-SUPPLIER-NAME` exists and is used as the *Item* Principal dimension). `supplier-all-active` resolves to no dimension.
   `EntityAnalyticsMetaKpiIds.cs:42`; `PeerGroupResolver.cs:18-33`; `SupplierEntityAnalyticsRegistrar.cs:59-68`
3. **UI gate is hardcoded.** Selector visibility is limited to Customer/Item.
   `investigationWorkspaceStore.ts:103-106`
4. **No Principal entity type.** `EntityTypeCode` has no `Principal`; it has `Supplier`.
   `EntityTypeCode.cs:9-20`. This is deliberately not a blocker because GAP-001 resolves Principal as a presentation alias.
5. **Small population vs. minimum size.** `PeerGroupResolver.ResolveForEntity` marks a group insufficient below `MinRadarPeerGroupSize` (5), used by the radar path. Peer-distribution itself does not block on size, but the small Principal population limits statistical meaning (`FEASIBILITY-ASSESSMENT.md:401` TQ-004).
   `PeerGroupResolver.cs:100`; `EntityAnalyticsConstants.MinRadarPeerGroupSize`
6. **Radar retirement changes the consumer surface, not the peer engine.** ADR-EA-001 retires Performance Signature/radar platform-wide; peer group remains used by the Investigation Workspace Peer Position. The ADR is "Accepted" but code still registers radar axes and the radar engine still exists — an implementation-state note, not a peer-group blocker.
   `ADR-EA-001-radar-retirement.md:16-28`; `SupplierEntityAnalyticsRegistrar.cs:517-653`

---

## 8. Estimated implementation complexity

| Scope | Complexity | Rationale |
| --- | --- | --- |
| Principal reuses Supplier **as-is with all-active peer group** | **No work (already reusable)** | Registered (`supplier-all-active`), endpoint works, panel falls back to default. |
| Enable a **dimension-based Principal peer group** (e.g., Principal region/class) | **Moderate enhancement** | Requires code edits in the hardcoded catalog/resolver, a new Supplier dimension KPI produced at L0, frontend selector gate + labels. No new engine or table. |
| Introduce **Principal as a distinct entity type** with its own peer rules | **Significant redesign (rejected by GAP-001)** | Would duplicate the profile/producer/population against the same `SupplierId`; explicitly rejected. |

Determination: the infrastructure is **reusable at the engine level**, but **not fully generic at the definition/presentation level**. For the endorsed alias approach (GAP-001), Peer Group is reusable immediately in all-active form; a meaningful Principal peer dimension is a moderate, bounded enhancement.

---

## Evidence Index

| Area | File |
| --- | --- |
| Rule engine | `btr.application/ReportingContext/EntityAnalyticsAgg/Services/EntityPeerDistributionEngine.cs` |
| Rule catalog | `btr.application/ReportingContext/EntityAnalyticsAgg/Services/PeerGroupRuleCatalog.cs` |
| Resolver | `btr.application/ReportingContext/EntityAnalyticsAgg/Services/PeerGroupResolver.cs` |
| Label formatter | `btr.application/ReportingContext/EntityAnalyticsAgg/Services/PeerGroupLabelFormatter.cs` |
| Rule model | `btr.application/ReportingContext/EntityAnalyticsAgg/Models/PeerGroupRuleDefinition.cs` |
| Entity registration | `btr.application/ReportingContext/EntityAnalyticsAgg/Models/EntityTypeRegistration.cs` |
| Platform registration | `btr.application/ReportingContext/EntityAnalyticsAgg/Services/EntityAnalyticsPlatformRegistrar.cs` |
| Queries | `btr.application/ReportingContext/EntityAnalyticsAgg/Queries/GetInvestigationWorkspaceQueries.cs` |
| API | `btr.portal.api/Controllers/EntityAnalytics/EntityAnalyticsController.cs` |
| Population query | `btr.infrastructure/ReportingContext/EntityAnalyticsAgg/EntityAnalyticsRepository.cs:1693` |
| Meta/dimension ids | `btr.application/ReportingContext/EntityAnalyticsAgg/Models/EntityAnalyticsMetaKpiIds.cs` |
| Map presets | `btr.application/ReportingContext/EntityAnalyticsAgg/Services/EntityMapPresetRegistry.cs` |
| Store / selector gate | `btr.portal.web/src/stores/investigationWorkspaceStore.ts` |
| Workspace view | `btr.portal.web/src/views/analytics/InvestigationWorkspaceView.vue` |
| Peer panel | `btr.portal.web/src/components/entity-analytics/workspace/PeerPositionPanel.vue` |
| Labels (FE) | `btr.portal.web/src/services/peerGroupLabel.ts` |
| Feasibility (Principal) | `docs/work/btr-portal/principal-investigation-workspace/FEASIBILITY-ASSESSMENT.md` |
| Radar retirement | `docs/work/btr-portal/principal-investigation-workspace/adrs/ADR-EA-001-radar-retirement.md` |
| Peer position feature | `docs/features/entity-analytics/peer-position-distribution.md` |
| Developer guide | `docs/features/entity-analytics/entity-analytics-developer-guide.md` |
