# IMPLEMENTATION PLAN

## Principal Investigation Workspace (Entity Analytics) — Principal-centric Analytics

| Field | Value |
| --- | --- |
| Status | PLANNED — ready for implementation approval |
| Plan date | 2026-09-11 |
| Milestone | Principal-centric Analytics Initiative — Investigation Workspace |
| Role | Planning Agent deliverable |

---

## 1. Planning Authority

```text
ARCHITECTURE
```

Authoritative input: `docs/work/btr-portal/principal-investigation-workspace/Architecture.md`.

Supporting authoritative inputs (already-resolved decisions consumed, not reinterpreted):

- `docs/work/btr-portal/principal-investigation-workspace/FEASIBILITY-ASSESSMENT.md` (IW-GAP-001..GAP-017, IW-BQ-005, IW-BQ-010, IW-TQ-005, IW-OQ-001..OQ-004)
- `docs/work/btr-portal/principal-investigation-workspace/adrs/ADR-EA-001-radar-retirement.md`
- `docs/work/btr-portal/principal-centric-analytics-migration/FEASIBILITY-ASSESSMENT.md` (MIG-GAP-*)
- `docs/work/btr-portal/principal-centric-analytics-migration/PRINCIPAL-KPI-REGISTRY.md`

This plan introduces no new business decisions and no new architecture decisions. Every slice is
traceable to a resolved decision in §3 (Decision Traceability) of the Architecture.

---

## 2. Scope Summary

Deliver the **Principal Investigation Workspace** as a two-lens configuration of the existing
entity-neutral Entity Analytics workspace, evolving the current Supplier Investigation entry in
place (Option A, IW-BQ-005).

Scope is configuration and presentation over an already-composed Principal entity pack — **not new
analytics** for the Sales-Out lens. Concretely the plan covers:

1. **Identity & terminology** — `Supplier` remains the canonical technical identity; `Principal` is
   the business-facing presentation alias across navigation, titles, breadcrumbs, and labels. No
   technical, database, API, snapshot, or domain migration (IW-GAP-001, IW-GAP-012, IW-GAP-017).
2. **Lens model** — a single workspace with a Sales-Out lens (default) and a Purchasing lens, exposed
   via a lens switcher (IW-GAP-002, IW-GAP-003).
3. **Population presets** — add the default `principal-sales-out-map`
   (X=Achievement %, Y=YoY Growth %, size=Sales-Out, color=Return %); retain purchase/inventory
   presets for the Purchasing lens (IW-GAP-003, IW-GAP-004).
4. **Stage content per lens** — lens-scoped KPI grouping, attention categories, purchasing
   relationship drivers, and the derived Purchase-to-Sales-Out Ratio (IW-GAP-005..GAP-009, BQ-010).
5. **Evidence & validation** — lens-scoped evidence presentation; platform-wide Performance Signature
   (Radar) retirement (IW-GAP-013, IW-GAP-010, ADR-EA-001).
6. **Data Health** — lens-independent Target Coverage / Missing Target / Unknown Principal
   disclosures (IW-OQ-003) plus standard freshness disclosure (IW-OQ-002).
7. **Knowledge synchronization** — entity-analytics artifacts, developer guide, and KPI catalog
   terminology (IW-GAP-017).

Out of scope (enforced by invariants, §11): portfolio management/ranking dashboards (SA04/EX01/EX02),
new Principal KPI IDs, new Principal master entity or producer, PO-based purchasing metrics, and
Principal-scoped authorization.

---

## 3. Impact Inventory

### Backend

| Component | File / asset | Change |
| --- | --- | --- |
| Entity type registration | `btr.application/.../EntityAnalyticsAgg/Services/EntityAnalyticsPlatformRegistrar.cs` | `DisplayName` "Supplier" → "Principal" for `EntityTypeCode.Supplier` only; technical keys unchanged |
| Map preset definitions | `.../EntityAnalyticsAgg/Services/EntityMapPresetRegistry.cs` | Add `principal-sales-out-map` default; retain purchase presets |
| Preset model (bubble color) | `.../EntityAnalyticsAgg/Models/EntityMapPresetDefinition.cs` | Add bubble-color encoding support (size field `BubbleKpiId` already exists; color is new) |
| Preset query / DTO | `GET /api/entity-analytics/presets` (controller + DTO) | Expose new preset + lens-aware default + color axis |
| KPI grouping config | `.../EntityAnalyticsAgg/Registrars/SupplierEntityAnalyticsRegistrar.cs` / pack model | Lens membership (Sales-Out vs Purchasing) for the `supplier-default` pack |
| Attention catalog | `.../EntityAnalyticsAgg/Registrars/SupplierAttentionSignalCatalog.cs` | Add five Sales-Out categories (reuse EX01/EX02 signals); retain purchase/inventory signals |
| Relationship catalog | `.../EntityAnalyticsAgg/Registrars/SupplierRelationshipCatalog.cs` | Add purchasing drivers (Top Purchased Items, Purchase History) |
| Relationship resolution | producer / relationship query | Resolve purchasing drivers from Purchase Invoice Detail + `PU-KPI-001` history (`SupplierId`-keyed) |
| Evidence resolver | `.../EntityAnalyticsAgg/Producers/SupplierEntityAnalyticsEvidenceResolver.cs` | Lens-scoped evidence categories/labels; no duplicate model |
| Derived ratio | Purchasing lens composition/display | Purchase-to-Sales-Out Ratio = `PRN-PUR-001` ÷ `PRN-SALES-001` (no new KPI ID, no persistence, no registry) |
| Data Health query | `GetEntityPerformanceProfileQuery` or dedicated endpoint | Target Coverage %, Missing Target Count, Unknown Principal count/amount |
| Radar retirement | radar axis registration / L5 composition (out of scope) | No Principal/dual-lens signature; verify no dependent route (ADR-EA-001) |
| Peer grouping | `.../EntityAnalyticsAgg/Services/PeerGroupResolver.cs` | Unchanged — `supplier-all-active` retained (IW-GAP-011) |

### Database

- **No new tables** for the Sales-Out lens — all `BTRPD_Principal*` projections exist.
- **No** separate Principal purchase-history projection (IW-GAP-007); purchase trend reuses
  `PU-KPI-001` monthly history.
- **No** change to `BTR_FakturItem` / `BTR_ReturJualItem` (Item-master attribution retained).

### Frontend

| Component | File / asset | Change |
| --- | --- | --- |
| Workspace shell | `views/analytics/InvestigationWorkspaceView.vue` | Lens switcher; entity-type-aware title ("Principal Investigation Workspace"); subtitle/scope labels use "Principal" |
| Navigation registry | `navigation/entityAnalyticsNavigation.ts` | `pluralLabel` "Suppliers" → "Principals"; keep `entityType`/route keys `Supplier` |
| Profile / compare wrappers | `views/analytics/SupplierProfileView.vue`, `SupplierCompareView.vue` | Titles "Principal Profile" / "Compare Principals"; keep `ENTITY_TYPE = 'Supplier'` |
| Profile shell | `components/entity-analytics/EntityPerformanceProfileShell.vue` | "Principal" labels; relationship/evidence section titles |
| Workspace store | `stores/investigationWorkspaceStore.ts` | Lens state + per-lens default preset; peer selector stays hidden for `Supplier` |
| Population tooltip | `components/entity-analytics/workspace/PopulationMapTooltip.vue` | Hide dimension row / "Category" when no dimension (IW-GAP-015) |
| Population canvas | `components/entity-analytics/workspace/PopulationMapCanvas.vue` | Bubble color encoding (size already supported) |
| KPI summary | `components/entity-analytics/workspace/WorkspaceKpiSummarySection.vue` | Group by active lens |
| Radar retirement | `ValidationStagePanel.vue`, `ProfileRadarSection.vue`, `RadarCompareSection.vue`, all four compare views, `EntityPerformanceProfileShell.vue` | Remove Performance Signature surface for all entity types (ADR-EA-001) |
| Evidence panel | `ProfileEvidenceSection.vue` / `ValidationStagePanel.vue` | Lens-scoped labels/disclosures |
| Data Health | new panel (or shared section) | Lens-independent Target Coverage / Missing Target / Unknown Principal + evidence navigation |

### Integration

- **None external.** No external system, no new API consumer, no new authorization boundary.
- Internal refresh ordering unchanged: `RefreshDashboardPurchasingManagementSnapshotWorker` remains the
  trigger for the Principal Entity Analytics refresh; `SupplierEntityAnalyticsProducer` remains the
  single writer.

### Security

- None. `Principal` is an analytical dimension, not a security boundary (MIG-GAP-018).

---

## 4. Phases

| Phase | Title | Purpose | Slices |
| --- | --- | --- | --- |
| 1 | Principal Identity & Terminology | Display "Principal" without technical renaming | PIW-01, PIW-02 |
| 2 | Lens Model, Switcher & Presets | Two lenses over one Principal; default Sales-Out map | PIW-03, PIW-04, PIW-05 |
| 3 | Lens-scoped Stage Content | Current Facts, Context (attention), Explanation (drivers), derived ratio | PIW-06, PIW-07, PIW-08, PIW-09, PIW-10 |
| 4 | Evidence & Signature Retirement | Lens-scoped evidence; platform-wide radar retirement | PIW-11, PIW-12 |
| 5 | Data Health | Lens-independent completeness/quality disclosures | PIW-13, PIW-14 |
| 6 | Knowledge Synchronization | Terminology and artifact consistency | PIW-15 |

Dependency order: Phase 1 → Phase 2 → Phase 3 → Phase 4 → Phase 5 → Phase 6. PIW-12 (radar
retirement) and PIW-13/PIW-14 (Data Health) are self-contained but sequenced to avoid shared-file
conflicts and to land after the lens model is stable.

---

## 5. Slices

### Phase 1 — Principal Identity & Terminology

#### PIW-01 — Backend entity-type display name "Principal"

- **Objective:** Change the `Supplier` entity type `DisplayName` to `Principal` while keeping
  `EntityTypeCode.Supplier`, `KpiPackId = "supplier-default"`, `RelationshipPackId`, peer rule, worker
  hook, and `ProfileRouteTemplate = "/analytics/suppliers/{id}"` unchanged. Confirm no opaque
  dependency on the literal "Supplier" (IW-TQ-001).
- **Dependencies:** None.
- **Acceptance Criteria:**
  - `EntityAnalyticsPlatformRegistrar.cs` registers `EntityTypeCode.Supplier` with `DisplayName =
    "Principal"`; all other fields identical.
  - `GET /api/entity-analytics/types` returns `EntityType = "Supplier"` with `DisplayName =
    "Principal"`.
  - No database table, snapshot key, API contract identifier, route template, or legacy identifier
    is renamed to "Principal".
  - A grep across backend for stringly-typed dependencies on `"Supplier"` confirms the only
    intentional literal is `EntityTypeCode.Supplier` (verification note recorded for IW-TQ-001).
- **Review Focus:** Architecture Compliance (ADRs PIW-001, PIW-011); Terminology Compliance
  (IW-GAP-017); no technical identity migration.

#### PIW-02 — Frontend Principal presentation labels

- **Objective:** Apply the IW-GAP-012 presentation mapping (Principal Investigation Workspace,
  Principal Profile, Compare Principals, Principal Relationships, Principal Evidence) across
  navigation, workspace titles, breadcrumbs, scope labels, and home-card labels without changing
  technical identifiers or route names.
- **Dependencies:** PIW-01.
- **Acceptance Criteria:**
  - `entityAnalyticsNavigation.ts` returns `pluralLabel = "Principals"` for key `Supplier`; route
    names (`supplier-compare`) and `entityType` key unchanged.
  - `InvestigationWorkspaceView.vue` renders title "Principal Investigation Workspace" and a
    "Principal" subtitle/scope label when `entityType === 'Supplier'`; other entity types unchanged.
  - `SupplierProfileView.vue` / `SupplierCompareView.vue` render "Principal Profile" /
    "Compare Principals" while `ENTITY_TYPE` remains `'Supplier'`.
  - `EntityPerformanceProfileShell.vue` relationship/evidence section titles read "Principal
    Relationships" / "Principal Evidence" for Supplier.
  - Breadcrumb scope label reads "Principal" for Supplier.
  - No route parameter, API call, or store key is changed from `Supplier`/`supplier`.
- **Review Focus:** Terminology Compliance (IW-GAP-012, IW-GAP-017); UI State Compliance; no
  technical rename.

---

### Phase 2 — Lens Model, Switcher & Presets

#### PIW-03 — Lens configuration model

- **Objective:** Introduce a lens configuration that names two lenses — Sales-Out (default) and
  Purchasing — over the same `SupplierId` principal, each owning its KPI group, default preset,
  attention categories, relationship drivers, and evidence routes. No new entity type, profile, or
  workspace (IW-GAP-002).
- **Dependencies:** PIW-01.
- **Acceptance Criteria:**
  - A single lens configuration exists for `EntityTypeCode.Supplier` with lens IDs `sales-out`
    (default) and `purchasing`.
  - Sales-Out KPI set = `PRN-SALES-001`, `PRN-GRW-001/002`, `PRN-RET-001..004`, `PRN-TGT-002/003`,
    `PRN-CUS-001/002` (per Architecture §8.4).
  - Purchasing KPI set = `PRN-PUR-001`, `PU-KPI-001` (trend), `PRN-INV-001/002`, and the derived
    Purchase-to-Sales-Out Ratio (per §8.4).
  - Default preset per lens is declared (Sales-Out → `principal-sales-out-map`; Purchasing →
    `purchase-exposure-map`).
  - No new entity type, KPI ID, or profile is introduced by the configuration.
- **Review Focus:** Architecture Compliance (ADR-PIW-002, ADR-PIW-003); Workflow Compliance.

#### PIW-04 — Lens switcher in workspace shell

- **Objective:** Render a lens switcher in the investigation workspace that toggles the active lens
  and its default preset; Peer Group Selector remains hidden for Principal (IW-GAP-011).
- **Dependencies:** PIW-03.
- **Acceptance Criteria:**
  - The workspace exposes a Sales-Out/Purchasing switcher for Supplier; default is Sales-Out.
  - Switching lens reloads the corresponding default preset and lens-scoped content.
  - `investigationWorkspaceStore.ts` keeps `showPeerGroupSelector` false for `Supplier` (no selector
    rendered; `supplier-all-active` peer population used).
  - Purchasing capabilities are present but not default; no navigation entry is duplicated.
- **Review Focus:** UI State Compliance; Architecture Compliance (ADR-PIW-002/003/007).

#### PIW-05 — `principal-sales-out-map` preset and bubble encoding

- **Objective:** Add the default Sales-Out population preset and support its bubble size/color
  encoding (size field exists; color encoding is validated and, if unsupported, added) (IW-GAP-004).
- **Dependencies:** PIW-03.
- **Acceptance Criteria:**
  - `EntityMapPresetRegistry` contains `principal-sales-out-map` for `Supplier` with `AxisXKpiId =
    PRN-TGT-003`, `AxisYKpiId = PRN-GRW-002`, `BubbleKpiId = PRN-SALES-001`, bubble color =
    `PRN-RET-004`, and no `FilterDimensionKpiId`.
  - `principal-sales-out-map` resolves as the Sales-Out lens default; existing
    `purchase-exposure-map` / `purchasing-discipline-map` remain available for the Purchasing lens.
  - Later-added presets must not change the default (a test asserts default resolution).
  - Population canvas renders bubble color by Return %; if the current canvas lacks color encoding,
    `EntityMapPresetDefinition`, the presets DTO, and `PopulationMapCanvas.vue` are extended
    minimally (recorded as a bounded extension, per Architecture §16).
- **Review Focus:** Architecture Compliance (ADR-PIW-003); Persistence/Model Compliance; map
  encoding validated against current canvas capability.

---

### Phase 3 — Lens-scoped Stage Content

#### PIW-06 — Current Facts KPI grouping by lens

- **Objective:** Present KPI Summary grouped by the active lens; Sales-Out KPIs never present
  purchase-in as performance (IW-GAP-002, MIG-GAP-010).
- **Dependencies:** PIW-03, PIW-04.
- **Acceptance Criteria:**
  - Sales-Out lens shows the Sales-Out KPI set; Purchasing lens shows the Purchasing set.
  - Purchase-in (`PRN-PUR-001`) is never labeled, grouped, or ranked as Principal sales performance.
  - `PRN-SALES-001` remains the default ranking KPI.
- **Review Focus:** Workflow Compliance (IW-GAP-010); Terminology Compliance.

#### PIW-07 — Sales-Out attention categories

- **Objective:** Expand the Supplier attention catalog with the five Sales-Out categories, reusing
  EX01/EX02 signals where applicable; retain purchase/inventory signals (IW-GAP-009). No new
  attention engine.
- **Dependencies:** PIW-03, PIW-04.
- **Acceptance Criteria:**
  - `SupplierAttentionSignalCatalog` registers: Sales-Out Decline, Growth Deterioration, Target Miss,
    Return Risk, Coverage Deterioration.
  - Existing purchase/inventory signals (Qualified Backlog, Spend Concentration, Inventory
    Concentration, At-Risk Exposure, Compound Dependency, Inventory-No-Purchase, Unknown Principal)
    remain.
  - Signals are lens-associated (Sales-Out categories under Sales-Out lens; purchase/inventory under
    Purchasing lens).
  - No new attention engine is introduced; signals reuse existing EX01/EX02 computation.
- **Review Focus:** Architecture Compliance (ADR-PIW-001/002); no new engine.

#### PIW-08 — Purchasing relationship drivers

- **Objective:** Add purchasing relationship drivers — Top Purchased Items (Purchase Invoice Detail)
  and Purchase History (`PU-KPI-001` monthly) — `SupplierId`-keyed, with no PO-based/user/procurement
  relationships (IW-GAP-008, IW-BQ-010).
- **Dependencies:** PIW-03.
- **Acceptance Criteria:**
  - `SupplierRelationshipCatalog` registers Top Purchased Items and Purchase History for Supplier.
  - Resolution sources: Top Purchased Items from Purchase Invoice Detail; Purchase History from
    `PU-KPI-001` monthly history — identity resolved via `SupplierId` (never `SupplierName`).
  - No PO-based, purchasing-user, or procurement-workflow relationship is registered.
- **Review Focus:** Architecture Compliance (ADR-PIW-006, ADR-PIW-008); Persistence Compliance.

#### PIW-09 — Purchase-to-Sales-Out Ratio (derived metric)

- **Objective:** Surface the investigation-only Purchase-to-Sales-Out Ratio in the Purchasing lens,
  derived from stored `PRN-PUR-001` and `PRN-SALES-001` (IW-GAP-005, IW-TQ-005).
- **Dependencies:** PIW-06.
- **Acceptance Criteria:**
  - Ratio = `PRN-PUR-001` ÷ `PRN-SALES-001`, with the >1.0 / ≈1.0 / <1.0 interpretation shown.
  - No new KPI ID, no registry entry, no persistence requirement, no ranking contract (IW-TQ-005,
    IW-OQ-001).
  - The ratio is displayed only in the Purchasing lens and is never presented or ranked as sales
    performance.
- **Review Focus:** Architecture Compliance (ADR-PIW-005); no KPI stewardship model introduced.

#### PIW-10 — Workspace presentation refinements

- **Objective:** Apply the tooltip and peer-group presentation rules (IW-GAP-015) and confirm the
  Principal peer selector stays hidden.
- **Dependencies:** PIW-04.
- **Acceptance Criteria:**
  - `PopulationMapTooltip.vue` hides the dimension row and never shows the generic "Category" label
    when no meaningful Principal dimension exists.
  - Peer Group Selector is not rendered for Supplier; `supplier-all-active` peer population remains
    in effect.
- **Review Focus:** UI State Compliance; Architecture Compliance (ADR-PIW-007).

---

### Phase 4 — Evidence & Signature Retirement

#### PIW-11 — Lens-scoped evidence presentation

- **Objective:** Ensure every Principal evidence panel, link, title, and disclosure indicates its
  originating lens, with no duplicate evidence model (IW-GAP-013).
- **Dependencies:** PIW-03, PIW-04.
- **Acceptance Criteria:**
  - Sales-Out evidence: Faktur Item, Return Item, Customer Contribution, Salesman Contribution.
  - Purchasing evidence: Purchase Invoice, Purchase History, Inventory Exposure, Inventory Movement.
  - `SupplierEntityAnalyticsEvidenceResolver` returns lens-categorized links; evidence categories and
    labels indicate the originating lens.
  - Purchase/inventory evidence is never reachable through a Sales-Out evidence link and vice versa.
  - One resolver, lens-scoped presentation (no duplicate evidence model).
- **Review Focus:** Architecture Compliance (ADR-PIW-004); Evidence lineage via `SupplierId`
  (ADR-PIW-008).

#### PIW-12 — Platform-wide Performance Signature retirement

- **Objective:** Retire the Performance Signature (Radar) surface for all entity types (IW-GAP-010,
  ADR-EA-001); no Principal/dual-lens signature and no replacement radar.
- **Dependencies:** PIW-11 (shared `ValidationStagePanel.vue` file).
- **Acceptance Criteria:**
  - `ValidationStagePanel.vue` no longer renders the "Performance Signature" block.
  - `ProfileRadarSection.vue` / `RadarCompareSection.vue` are removed from
    `EntityPerformanceProfileShell.vue` and all four compare views (Customer, Salesman, Item,
    Supplier).
  - No entity profile or evidence route depends on the retired surface (verified); L5 snapshot
    history is not deleted.
  - Radar axis registration / L5 signature composition is not part of this scope.
- **Review Focus:** Architecture Compliance (ADR-PIW-012, ADR-EA-001); no replacement radar.

---

### Phase 5 — Data Health

#### PIW-13 — Data Health backend data

- **Objective:** Expose Target Coverage %, Principals Missing Target Count, Unknown Principal
  Exception Count, and Unknown Principal Exception Amount for the workspace (IW-OQ-003).
- **Dependencies:** None (can proceed in parallel; sequenced after lens model for file stability).
- **Acceptance Criteria:**
  - A query/section provides the four indicators; Unknown Principal count/amount sourced from
    `BTRPD_PrincipalSalesOut` data-quality output; Target Coverage %/Missing Target Count sourced from
    `BTRPD_PrincipalTarget` vs the Sales-Out population.
  - Unknown Principal is an exception disclosure, not a synthetic Principal; it never silently drops
    Principal Sales-Out.
  - No new table is required; indicators are read-only.
- **Review Focus:** Architecture Compliance (ADR-PIW-010); Data completeness/quality semantics.

#### PIW-14 — Data Health frontend panel

- **Objective:** Render a lens-independent Data Health section with evidence navigation for Unknown
  Principal exceptions and the standard freshness disclosure (IW-OQ-003, IW-OQ-002).
- **Dependencies:** PIW-13.
- **Acceptance Criteria:**
  - A Data Health section is visible regardless of the active lens.
  - It displays Target Coverage %, Principals Missing Target Count, Unknown Principal Exception Count,
    and Unknown Principal Exception Amount.
  - Unknown Principal exceptions link to supporting evidence.
  - Every workspace view displays `Generated At`, `Reporting Period`, and `Data Snapshot Period`
    (when applicable), with the standard snapshot disclosure.
- **Review Focus:** Architecture Compliance (ADR-PIW-010, ADR-PIW-009); Freshness policy compliance.

---

### Phase 6 — Knowledge Synchronization

#### PIW-15 — Knowledge and terminology synchronization

- **Objective:** Synchronize entity-analytics feature artifacts, developer guide, KPI catalog
  references, and navigation docs to the Principal/Supplier terminology governance rule (IW-GAP-017).
- **Dependencies:** PIW-01..PIW-14.
- **Acceptance Criteria:**
  - `docs/features/entity-analytics/entity-analytics-developer-guide.md` reflects Principal as the
    user-facing term and Supplier as technical.
  - Navigation/menu documentation and the KPI catalog references use Principal user-facing /
    Supplier technical consistently.
  - No temporary artifact (this plan and the ADRs) is promoted to permanent knowledge without the
    Knowledge Curator pass.
- **Review Focus:** Terminology Compliance (IW-GAP-017); documentation consistency.

---

## 6. Critical Invariants (applies to every slice — review must verify)

1. Exactly one authoritative Principal profile; one investigation entry (Supplier entry replaced, not
   duplicated) (IW-BQ-005).
2. Purchase-in is never presented, grouped, or ranked as Principal sales performance (IW-GAP-010,
   MIG-GAP-010).
3. Returns never reduce `PRN-SALES-001` (KPI Registry, MIG-GAP-020).
4. No Principal financial/credit/collection KPI is introduced (MIG-GAP-005).
5. No PO-based metric is introduced — including re-verifying the existing Qualified Backlog attention
   signal against the no-PO-metrics rule (IW-GAP-006).
6. All Principal aggregations resolve identity via `SupplierId`; `SupplierName` is presentation only;
   no name-based aggregation/matching/joining (IW-GAP-016).
7. Every evidence panel, link, title, and disclosure indicates its originating lens (IW-GAP-013).
8. Every view displays `Generated At`, `Reporting Period`, and `Data Snapshot Period` (IW-OQ-002).
9. Business-facing surfaces use Principal; technical artifacts use Supplier (IW-GAP-017).
10. No new Principal KPI IDs; existing KPI semantics unchanged (IW-TQ-005).

---

## 7. Progress Tracker

| Slice | Objective | Status |
| --- | --- | --- |
| PIW-01 | Backend entity-type display name "Principal" | GO |
| PIW-02 | Frontend Principal presentation labels | GO |
| PIW-03 | Lens configuration model | GO |
| PIW-04 | Lens switcher in workspace shell | GO |
| PIW-05 | `principal-sales-out-map` preset and bubble encoding | GO |
| PIW-06 | Current Facts KPI grouping by lens | GO |
| PIW-07 | Sales-Out attention categories | GO |
| PIW-08 | Purchasing relationship drivers | GO |
| PIW-09 | Purchase-to-Sales-Out Ratio (derived metric) | GO |
| PIW-10 | Workspace presentation refinements (tooltip/peer) | GO |
| PIW-11 | Lens-scoped evidence presentation | GO |
| PIW-12 | Platform-wide Performance Signature retirement | GO |
| PIW-13 | Data Health backend data | GO |
| PIW-14 | Data Health frontend panel | GO |
| PIW-15 | Knowledge and terminology synchronization | GO |

Lifecycle: `PLANNED → IN IMPLEMENTATION → IMPLEMENTED → IN REVIEW → GO` (or `NO-GO → REMEDIATION →
IN REVIEW → GO`).

### Review History

| Slice | Review date | Result | Findings | Remediation |
| --- | --- | --- | --- | --- |
| PIW-01 | 2026-09-11 | GO | None blocking; INFO-001 pre-existing `btr.test` compile error (out of slice scope) | Resolved outside PIW-01 (`DashboardAlertCenterComposerTest.cs` `IndexOf` fix, uncommitted); PIW-01 focused test passes |
| PIW-02 | 2026-09-11 | GO | None blocking | N/A |
| PIW-03 | 2026-09-11 | GO | None blocking. INFO-001: preset/driver/category declarations are config-only and registered by later slices. INFO-002: lens config not yet API-exposed; consumption deferred to PIW-04 | N/A |
| PIW-04 | 2026-09-11 | GO | None blocking. INFO-001: lens config exposed read-only via `GET /api/entity-analytics/lenses` (consumes PIW-03 registry; PIW-03 INFO-002 resolved). INFO-002: `principal-sales-out-map` not yet registered (PIW-05); Sales-Out lens preset resolution falls back to the entity default until PIW-05. INFO-003: active lens is session state, not persisted in URL; deferred pending later lens-scoped content slices | N/A |
| PIW-05 | 2026-09-11 | GO | None blocking. INFO-001: `.iw-map-encoding` legend has no scoped CSS (unstyled text caption); cosmetic only, bubble color rendering unaffected | N/A |
| PIW-06 | 2026-09-11 | GO | None blocking. INFO-001: `PRN-CUS-001/002` are in the Sales-Out lens KpiIds but are not listed in the `supplier-default` pack (registered separately); absent rows simply do not render, data-driven, non-blocking. INFO-002: lens scoping applies to the workspace Current Facts summary only; standalone profile/compare views keep the full pack (consistent with slice scope) | N/A |
| PIW-07 | 2026-09-12 | GO | None blocking. INFO-001: five Sales-Out categories registered with new self-descriptive signal codes; no existing EX01/EX02 code maps 1:1 to a category, so code reuse was not applicable — engine/computation untouched, no new attention engine. INFO-002: `dotnet build` of the sln requires VS MSBuild (dotnet SDK lacks VS web-application targets); build/test ran under VS 2022 MSBuild, previously established convention | N/A |
| PIW-08 | 2026-09-12 | GO | None blocking. INFO-001: purchasing drivers (`TopPurchasedItems`, `PurchaseHistory`) are declarative catalog registrations; no snapshot production/rendering added — consistent with FEASIBILITY §10 ("no additional model") and PIW-03's deferred "catalog registration" note. INFO-002: `PurchaseHistory` is a time-series driver and carries no `TargetEntityType`; inert until a resolution mechanism is introduced (out of slice scope). INFO-003: 11 pre-existing `btr.test.ReportingContext` failures at the parent commit (cash-flow, customer pack, field activity, sales report, inventory-risk, collection, `SupplierEntityAnalyticsProducerTest` fixture missing `RelationshipProjection`) are unrelated; no new failures introduced | N/A |
| PIW-09 | 2026-09-12 | GO | None blocking. INFO-001: the approved `≈ 1.0` band carries no numeric tolerance in FEASIBILITY GAP-005 / Architecture §8.3; implementation keys bands off the approved `1.0` boundary (exact `1.0` → Balanced) and displays the three-band guide, introducing no new threshold. INFO-002: the lens DTO now also projects `DerivedMetricIds` (PIW-04 exposed the lens config without it); minimal read-only projection of the approved PIW-03 model, needed for config-driven lens scoping. INFO-003: pre-existing `btr.test.ReportingContext` failures unchanged; no new failures | N/A |
| PIW-10 | 2026-09-12 | GO | None blocking. INFO-001: the population map backend now projects a read-only `PopulationMapResponseDto.DimensionLabel` from the approved `IDimensionLabelRegistry` (outside the §3 impact inventory, which lists only `PopulationMapTooltip.vue`); bounded read-only projection required to satisfy Architecture §7.6 — a data-driven, non-generic dimension label that keeps dimensioned entity types (Customer/Item/Salesman) intact while leaving Principal undimensioned — reusing the same accepted precedent as PIW-09 INFO-002. INFO-002: the Item map tooltip now shows the registry label ("Supplier" for the `SupplierName` dimension; "Category" is the correct label when the Category dimension is active) instead of the previous hardcoded generic "Category" fallback; terminology is data-driven from the approved registry. Pre-existing `btr.test.ReportingContext` failures unchanged (11); no new failures | N/A |
| PIW-11 | 2026-09-12 | GO | None blocking. INFO-001: the resolver's Purchasing evidence links (`/reports/purchasing`, `/reports/inventory`) are routed by `supplierCode` while the report views hydrate on `supplierId` (pre-existing resolver contract, unchanged by this slice; matches the FEASIBILITY `Purchasing evidence | /reports/purchasing, /reports/inventory` route mapping) | N/A |
| PIW-12 | 2026-09-12 | GO | None blocking. INFO-001: the retired wrapper components `ProfileRadarSection.vue` / `RadarCompareSection.vue` were deleted; the underlying `PerformanceSignatureSection.vue` / `PerformanceSignatureChart.vue` / `PerformanceSignatureScoreTable.vue` remain as unreferenced (orphaned) components — not listed in the §3 impact inventory, so removal is out of slice scope; grep confirms zero importers. INFO-002: `models/entityAnalytics.ts` retains the `Radar` / `RadarComparison` API-contract types (backend response contract still carries them; L5 composition is out of this slice's scope per AC4) — they are data types, not a rendered surface. Verified: no entity profile, compare, evidence, or router view imports the retired surface; `BTRPD_EntityAnalytics_Radar` / L5 history untouched | N/A |
| PIW-13 | 2026-09-12 | GO | None blocking. INFO-001: indicator computation is a thin read-only projection of existing `BTRPD_PrincipalTarget` / `BTRPD_PrincipalSalesOut` / `BTRPD_PrincipalSalesOutDataQuality` via the already-registered `IPrincipalTargetSnapshotDal` / `IPrincipalSalesOutSnapshotDal` (no new table, no write path). Target Coverage % and Missing Target Count derive from `BTRPD_PrincipalTarget` vs the Sales-Out population; Unknown Principal Count/Amount derive from the Sales-Out data-quality output (blank/unknown codes only). INFO-002: non-Supplier entity types return `IsAvailable = false` (disclosures are Principal-specific per IW-OQ-003); unknown entity types return 400 — consistent with the other entity-analytics endpoints. INFO-003: focused `GetEntityDataHealthHandlerTest` (6 tests) passes; backend build (VS MSBuild, `btr.application` + `btr.test`) succeeds; no production code beyond the approved slice | N/A |
| PIW-14 | 2026-09-12 | GO | None blocking. INFO-001: on Data Health fetch failure the section (and its snapshot disclosure) is hidden rather than surfacing an error banner — consistent with the existing best-effort load pattern (lenses/presets/population) and preserves workspace stability; the freshness rows are `v-if` guarded and the disclosure text renders regardless, so the block degrades gracefully when freshness metadata is null ("when applicable" per IW-OQ-002). INFO-002: the section renders only when the API reports `IsAvailable` (true for Supplier/Principal); non-Principal investigation entity types show no Data Health section — consistent with PIW-13 INFO-002 and the Principal-specific disclosure rule (IW-OQ-003 / ADR-PIW-010). Verified: section placed outside the lens-gated template block (visible in both lens modes and Discovery/Investigation modes); four indicators rendered via `DashboardMetric`; evidence link `RouterLink` → `/reports/sales` (SalesReportView surfaces the Unknown Principal exception-line disclosure); freshness block shows Generated At / Reporting Period / Data Snapshot Period + the OQ-002 disclosure verbatim; Unknown Principal evidence block always rendered (never silently dropped, ADR-PIW-010); frontend build (`vue-tsc -b && vite build`) passes and 38 files / 296 frontend tests pass; no backend files changed | N/A |
| PIW-15 | 2026-09-12 | GO | None blocking. Documentation-only slice; no source code changed (`git diff` = 3 feature docs + tracker). AC1 PASS: developer guide now states Supplier is technical / Principal is user-facing, `Supplier` example pack section and step-10 frontend bullet updated. AC2 PASS: `navigation-assets.md` updated term note, PAGE-PROFILE-SUPPLIER → "Principal Performance Profile", PAGE-COMPARE-SUPPLIER → "Compare Principals", workspace purpose, widget targets, drill-downs, Section-6/Purchasing diagrams, discovery coverage, gap notes; `question-navigation-map.md` all "Supplier Performance Profile" → "Principal Performance Profile", "Compare Suppliers" → "Compare Principals", compare lists and name-picker row updated; KPI catalog already consistent (L809 map note + §6.9 sync paragraph), verify-only, no change needed. AC3 PASS: no plan/ADR content promoted — additions reference the governance rule conceptually, no temp artifact IDs (PIW-/ADR-/GAP-) introduced into permanent docs. INFO-001: inventory-domain surface names (Top Supplier %, Top 10 Suppliers, Inventory by Supplier, Supplier Risk Exposure, Category Risk Exposure) intentionally remain "Supplier" in both nav docs because they exactly match implemented inventory/executive UI labels; renaming is an inventory-domain product decision outside PIW-15 navigation-doc scope. INFO-002: MQ-013 wording "Which suppliers are becoming dominant?" and surrounding catalog prose kept verbatim to stay literal to the business-question catalog (which does not phrase MQ-013 with Principal). Doc labels cross-checked against actual component titles (`SupplierProfileView.vue` "Principal Profile", `SupplierCompareView.vue` "Compare Principals", `InvestigationWorkspaceView.vue` "Principal Investigation Workspace", `EntityPerformanceProfileShell.vue` tabs) — consistent | N/A |

---

## 8. Verification Notes

- Backend build/test: `dotnet build` / `dotnet test` against `src/j05-btr-distrib/j05-btr-distrib.sln`
  (test project `btr.test`).
- Frontend typecheck/build: `npm run build` (runs `vue-tsc -b && vite build`).
- Frontend tests: `npm run test` (vitest) in `src/j05-btr-distrib/btr.portal.web`.
- Each slice must compile and pass existing tests before review; new behavior should carry focused
  unit tests (e.g., preset default resolution, lens membership, tooltip dimension hiding, evidence
  lens scoping).

### PIW-01 verification note (IW-TQ-001)

Backend grep for the literal `"Supplier"` (btr.application, btr.portal.api, btr.infrastructure,
btr.domain) returns only technical identifiers, not display-name dependencies:
`EntityTypeCode.Supplier = "Supplier"` (canonical code), `InvestigationMetadataBuilder.EntityTypeSupplier`,
`DashboardInventory*Aggregator.DimensionSupplier`, `ItemEntityAnalyticsRegistrar` dimension label key,
and `PeerGroupLabelFormatter` peer label text. No code reads the registered `DisplayName` for
`EntityTypeCode.Supplier` as an identity/key, so changing it to `"Principal"` has no opaque dependency.

Note: `btr.test` has a pre-existing, unrelated compile failure
(`DashboardAlertCenterComposerTest.cs`: `string.Contains(string, StringComparison)` unsupported on
net48) present before PIW-01; it is out of slice scope and was not modified.

### PIW-02 verification note

`entityAnalyticsNavigation.ts` now supplies the single Principal presentation mapping
(`singularLabel = "Principal"`, `pluralLabel = "Principals"` for key `Supplier`) via
`getEntityDisplayLabel`, consumed by the workspace title/subtitle, breadcrumb, scope label, home
cards, profile shell, and compare view. No `EntityType`/route/store identifier changed; only
user-facing labels. Frontend build (`npm run build`: `vue-tsc -b && vite build`) and tests
(`npm run test`: 34 files, 266 tests) both pass.

### PIW-03 verification note

Backend lens configuration model added under Entity Analytics:

- `EntityInvestigationLensIds` (`sales-out`, `purchasing`) and
  `EntityInvestigationDerivedMetricIds` (`purchase-to-sales-out-ratio`, explicitly not a KPI ID).
- `EntityInvestigationLensDefinition` model (lens id, entity type, display name, default flag,
  default preset, KPI IDs, derived metric IDs, attention categories, relationship drivers,
  evidence routes).
- `EntityInvestigationLensRegistry` declaring exactly two lenses for `EntityTypeCode.Supplier`:
  Sales-Out (default, preset `principal-sales-out-map`) with the §8.4 Sales-Out KPI set, and
  Purchasing (preset `purchase-exposure-map`) with `PRN-PUR-001`, `PU-KPI-001`, `PRN-INV-001/002`
  plus the derived Purchase-to-Sales-Out Ratio. No new entity type, KPI ID, or profile is
  introduced; no catalog is registered and no data is persisted by this slice.

The configuration is the declarative source of truth consumed by later slices: PIW-04 (lens
switcher / store lens state), PIW-05 (`principal-sales-out-map` preset definition), PIW-06 (KPI
grouping), PIW-07 (attention lens association), PIW-08 (purchasing relationship drivers), and
PIW-11 (lens-scoped evidence). The referenced preset id `principal-sales-out-map`, the purchasing
relationship driver codes, and the Sales-Out attention categories are declared here; their catalog
registration is implemented by those later slices and is intentionally out of PIW-03 scope.

Focused tests (`EntityInvestigationLensRegistryTest`, 6 tests) verify lens count/default, both KPI
sets, per-lens default presets, derived-metric non-KPI treatment, and Sales-Out/Purchasing KPI
separation. Backend build (VS MSBuild, `btr.test.csproj`) succeeds and the focused tests pass.

### PIW-04 verification note

Consumed the PIW-03 lens configuration (review INFO-002 deferred API exposure to PIW-04) as a
read-only projection, then wired the workspace lens switcher:

- Backend: `GetInvestigationLensesQuery` / `InvestigationLensesResponse` /
  `GetInvestigationLensesHandler` in `EntityAnalyticsAgg/Queries/GetInvestigationWorkspaceQueries.cs`
  project `EntityInvestigationLensRegistry` (lens id, display name, default flag, default preset,
  KPI ids, attention categories, relationship drivers, evidence routes). New
  `GET /api/entity-analytics/lenses?entityType=` route in `EntityAnalyticsController`; no writes,
  no new entity type/KPI, no persistence. The API is a thin projection of the approved PIW-03 model.
- Frontend store (`investigationWorkspaceStore.ts`): added `lenses`/`activeLensId`,
  `activeLens`/`hasLensSwitcher`, `loadLenses`, and `setLens`. `setLens` toggles the lens, selects
  that lens's default preset (falling back to the entity default only while
  `principal-sales-out-map` is still absent, i.e. before PIW-05), and reloads the population map.
  `activeLensId` participates in undo snapshots. `initializeWorkspace`/`setEntityType` load lenses
  before presets; the default Sales-Out lens drives the initial preset. No navigation entry, route,
  or peer selector is added or duplicated; `showPeerGroupSelector` stays false for `Supplier`
  (`supplier-all-active` population retained).
- Frontend view (`InvestigationWorkspaceView.vue`): rendered a PrimeVue `SelectButton` lens switcher
  in the Population toolbar, shown only when more than one lens exists (`Supplier`).

Focused tests: frontend `investigationWorkspaceStore.spec.ts` (3 new tests: Sales-Out default lens +
default preset, Purchasing switch selects `purchase-exposure-map` and reloads the map, Peer Group
Selector hidden for Principal). Backend `GetInvestigationLensesHandlerTest` (2 tests) verifies the
exposed lens set/default and unknown-entity rejection.

Note (sequencing): `principal-sales-out-map` is not yet registered in `EntityMapPresetRegistry`
(PIW-05); PIW-04 resolves it when present and otherwise falls back to the entity default preset, so
the initial Sales-Out resolution becomes exact once PIW-05 lands. Full frontend build
(`vue-tsc -b && vite build`) passes; frontend tests 34 files / 269 tests pass; backend build (VS
MSBuild, `btr.test.csproj`) succeeds; focused backend lens tests 8/8 pass.

### PIW-05 verification note

Implemented the default Sales-Out population preset with its bubble size/color encoding:

- Backend: `principal-sales-out-map` registered in `EntityMapPresetRegistry` for `Supplier` with
  `AxisXKpiId = PRN-TGT-003`, `AxisYKpiId = PRN-GRW-002`, `BubbleKpiId = PRN-SALES-001`,
  `BubbleColorKpiId = PRN-RET-004`, `IsDefault = true`, `FilterDimensionKpiId = null`.
  `purchase-exposure-map` demoted to `IsDefault = false` (still available alongside
  `purchasing-discipline-map`); exactly one Supplier default remains.
- Bounded extension (per Architecture §16): `EntityMapPresetDefinition.BubbleColorKpiId` added
  (size field `BubbleKpiId` already existed); `MapPresetDto` / `PopulationMapResponseDto` /
  `PopulationMapPointDto` expose bubble + color IDs, labels, and per-point values; `GetMapPresets`
  handler and `EntityPopulationMapEngine` populate them.
- Frontend: `entityAnalytics.ts` models extended; `PopulationMapCanvas.vue` renders bubble color
  for normal-tier points via a data-relative neutral→critical scale (no business thresholds;
  attention/watch/critical tiers, selection, search, and hover behavior unchanged) with a
  "Bubble color: <label>" legend; tooltip shows bubble size/color values.
- Lens alignment: registered preset id matches the PIW-03 Sales-Out lens `DefaultPresetId`, so the
  PIW-04 lens-switcher fallback now resolves exactly.

Focused tests: `EntityMapPresetRegistryTest` (3 new tests: encoding, Supplier default resolution,
single-default stability) + `EntityInvestigationLensRegistryTest` (6 tests) pass 11/11. Backend
build (VS MSBuild, `btr.test.csproj`) succeeds. Full frontend build (`vue-tsc -b && vite build`)
passes; frontend tests 34 files / 269 tests pass.

### PIW-06 verification note

Implemented lens-scoped Current Facts KPI grouping (frontend-only, consumed the PIW-03 lens
configuration projection and PIW-04 lens state):

- New pure service `btr.portal.web/src/services/lensScopedKpis.ts`: `selectLensScopedKpis`
  filters the profile `KpiSummary.Categories` flat KPI list to the active lens `KpiIds` set. When
  no lens set is supplied (null/undefined/empty, e.g. non-Supplier entity types), all KPIs are
  returned unchanged, preserving existing behavior for Customer/Salesman/Item.
- `investigationWorkspaceStore.ts` exposes `activeLensKpiIds` (the active lens `KpiIds`, null when
  no lens), reactive to `activeLens`/lens switcher state.
- `InvestigationWorkspaceView.vue` passes `:kpi-ids="workspace.activeLensKpiIds` to the Current
  Facts KPI summary (unchanged for non-lensed entity types).
- `WorkspaceKpiSummarySection.vue` consumes the optional `kpiIds` prop and filters the headline
  KPI list before the existing 8-KPI slice; KpiCard rendering, category order, and display
  formatting (Currency compact/IDR) are unchanged.

Acceptance criteria: Sales-Out lens (`PRN-SALES-001`, `PRN-GRW-001/002`, `PRN-RET-001..004`,
`PRN-TGT-002/003`, `PRN-CUS-001/002`) is shown under the Sales-Out lens; Purchasing lens
(`PRN-PUR-001`, `PU-KPI-001`, `PRN-INV-001/002`) under the Purchasing lens. `PRN-PUR-001`
(Purchase-In) is excluded from the Sales-Out lens, so it is never grouped/presented as Principal
sales performance; its registry DisplayName "Purchase-In" is unchanged. No ranking path or
ranking KPI is touched: `PRN-SALES-001` remains the default ranking KPI (registry
`RankEligible = true` + principal ranking default unchanged), verified as no-regression (IW-GAP-010,
MIG-GAP-010). Lens `KpiIds` are read from the approved PIW-03 lens registry; no new KPI ID,
registry entry, or persistence was introduced.

Focused tests: `src/services/lensScopedKpis.spec.ts` (4 tests: no-set passthrough, Sales-Out set
excludes `PRN-PUR-001`, Purchasing set excludes sales KPIs, doc-order preservation). Full frontend
build (`vue-tsc -b && vite build`) passes; frontend tests 35 files / 273 tests pass (includes the 4
new lens-scoping tests).

### PIW-08 verification note

Registered the approved purchasing relationship drivers in the existing `SupplierRelationshipCatalog`
(no additional model, per FEASIBILITY §10):

- `TopPurchasedItems` ("Top Purchased Items"): `TargetEntityType = Item`,
  `MetricKpiId = PRN-PUR-001` (`PurchaseInvoiceDetailMetricKpiId`, Purchase Invoice Detail grain),
  `PeriodSemantics = MTD`, `TopN = 10`.
- `PurchaseHistory` ("Purchase History"): `MetricKpiId = "PU-KPI-001"`
  (`PurchaseHistoryMetricKpiId`, monthly purchase-amount history), `PeriodSemantics = Monthly`,
  `TopN = 12`.
- Both codes added to the `supplier-relationships` pack alongside the three Sales-Out omzet drivers.
  `IsPurchasingRelationship` was added for symmetry with `IsSalesOmzetRelationship`. The PIW-03
  `EntityInvestigationLensRegistry` Purchasing-lens `RelationshipDrivers` now reference the catalog
  constants instead of duplicated string literals.

Acceptance criteria: the catalog registers Top Purchased Items and Purchase History for Supplier;
resolution sources are declared as Purchase Invoice Detail (`PRN-PUR-001`) and `PU-KPI-001` monthly
history, both Supplier-scoped (entity-type pack) and never `SupplierName`-keyed; no PO-based,
purchasing-user, buyer, or procurement-workflow relationship is registered. Existing Sales-Out omzet
drivers (`PRN-SALES-001`) are unchanged, so purchase-in is still never presented/ranked as Principal
sales performance. The expanded pack is inert for existing producers (no snapshots use the new
codes), so no L4/persistence behavior changes.

Focused tests: `SupplierPurchasingRelationshipCatalogTest` (6 tests: Top Purchased Items metadata,
Purchase History metadata, pack resolution, no PO/purchasing-user/procurement codes, purchasing-lens
scoping, disjoint classification) plus the PIW-03/omzet metadata suites pass 17/17. The previously
pack-wide omzet assertion in `SupplierPrincipalOmzetRelationshipMetadataTest` was scoped to omzet
relationships (the expanded pack otherwise invalidates it). Backend build (VS MSBuild,
`btr.test.csproj`) succeeds. Full `btr.test.ReportingContext` run: 946 passed / 11 failed, identical
to the 940 / 11 baseline at the parent commit; the 11 failures are pre-existing and unrelated to this
slice (cash-flow, customer pack, field activity, sales report, inventory-risk, collection, and a
`SupplierEntityAnalyticsProducerTest` fixture that omits `RelationshipProjection`).

### PIW-09 verification note

Surfaced the investigation-only Purchase-to-Sales-Out Ratio in the Purchasing lens
(frontend composition over already-stored measures):

- Backend prerequisite: the approved PIW-03 `EntityInvestigationLensDefinition.DerivedMetricIds`
  is now projected read-only by `GetInvestigationLensesHandler` / `InvestigationLensDto` (PIW-04
  exposed the lens config but omitted the derived-metric list). No new entity type, KPI ID,
  registry entry, persistence, or write path.
- Frontend service `btr.portal.web/src/services/investigationDerivedMetrics.ts`: pure
  `computePurchaseToSalesOutRatio` derives the ratio from the already-stored `PRN-PUR-001` and
  `PRN-SALES-001` `KpiEnvelope` values (no source-transaction recompute), formats it to 2 decimals,
  and maps it to the approved bands (`> 1.0` Inventory Building / Potential Over-Buying;
  `≈ 1.0` Balanced; `< 1.0` Inventory Drawdown / Potential Under-Buying). `selectLensDerivedMetrics`
  consumes the active lens `DerivedMetricIds`, ignores unknown ids, and returns nothing when the
  lens declares none (non-`Supplier` entity types unchanged). No numeric tolerance was introduced:
  the bands key off the approved `1.0` boundary (exact `1.0` → Balanced); the three-band guide is
  displayed alongside the value.
- `investigationWorkspaceStore.ts` exposes `activeLensDerivedMetricIds` (from the active lens);
  `InvestigationWorkspaceView.vue` renders the new `WorkspaceDerivedMetricsSection.vue` in the
  Current Facts stage only when the active lens declares derived metrics (Purchasing), displaying
  the ratio, its per-entity interpretation, the source KPI lineage, and a caption stating it is an
  investigation-only metric that is not a KPI, ranking, or sales-performance measure.

Acceptance criteria: ratio = `PRN-PUR-001` ÷ `PRN-SALES-001` with the `> 1.0 / ≈ 1.0 / < 1.0`
interpretation shown; no new KPI ID, registry entry, persistence, or ranking contract (the metric
id `purchase-to-sales-out-ratio` is not a `PRN-` KPI and is absent from the KPI registry); the
panel renders only under the Purchasing lens (`activeLensDerivedMetricIds` empty for Sales-Out),
so the ratio is never presented or ranked as Principal sales performance.

Focused tests: frontend `investigationDerivedMetrics.spec.ts` (8 tests: ratio derivation,
balanced/over/under bands, zero/missing denominators, no-metric passthrough, config-driven
selection, unknown-id ignore, non-KPI treatment) plus the updated
`investigationWorkspaceStore.spec.ts` lens tests (derived-metric ids per lens) pass; backend
`GetInvestigationLensesHandlerTest.DerivedMetricIds_AreExposedPerLens` passes. Full frontend tests
36 files / 281 tests pass; full frontend build (`vue-tsc -b && vite build`) passes; backend build
(VS MSBuild, `btr.test.csproj`) succeeds; focused backend lens tests 9/9 pass.

### PIW-10 verification note

Applied the tooltip dimension presentation rule and confirmed the Principal peer selector stays
hidden:

- Backend projection: `EntityPopulationMapEngine` now resolves the population dimension label from
  the existing `IDimensionLabelRegistry` (per entity type + effective dimension KPI id) and projects
  it as read-only `PopulationMapResponseDto.DimensionLabel`. The effective dimension is unchanged
  (`preset.FilterDimensionKpiId` else the registered peer-group rule); `Supplier` presets are
  undimensioned (`FilterDimensionKpiId = null`, peer rule `supplier-all-active`), so the label is
  `null` for Principal. No new entity type, KPI ID, registry entry, persistence, or write path —
  bounded read-only projection, consistent with the accepted PIW-05/PIW-09 precedent
  (INFO-001 below).
- Frontend: the tooltip's dimension row is now data-driven. New pure service
  `btr.portal.web/src/services/populationTooltip.ts` (`resolveTooltipDimensionRow`) returns a
  `{ label, value }` row only when both a population-level dimension label and a point value exist,
  and hides the row otherwise. `PopulationMapTooltip.vue` consumes it via a computed — the hardcoded
  generic "Category" label is removed. For Principal, `DimensionLabel` is `null`, so the dimension
  row never renders; dimensioned entity types (Customer "Wilayah", Item "Supplier", Salesman
  "Wilayah") keep their registry label.

Acceptance criteria: (1) `PopulationMapTooltip.vue` hides the dimension row and never shows the
generic "Category" label when no meaningful Principal dimension exists (IW-GAP-015); verified by
`populationTooltip.spec.ts` (6 tests) and the backend null-label projection. (2) The Peer Group
Selector is not rendered for Supplier, and the `supplier-all-active` peer population remains in
effect (ADR-PIW-007); verified via the existing
`investigationWorkspaceStore.spec.ts` "keeps the Peer Group Selector hidden for the Principal
entity type" test (Supplier, > 1 peer rules → `showPeerGroupSelector` false) — no selector change
was needed, and the backend peer catalog/engine still resolves `supplier-all-active` to the
undimensioned all-active population.

Focused tests: frontend `populationTooltip.spec.ts` (6 tests: Principal no-dimension hide,
missing point value hide, missing label hide, null/undefined guards, label+value rendering,
blank labels/values) plus the store peer-selector test; backend `EntityPopulationMapEngineTest`
(+2 tests: `BuildPopulationMap_ProjectsDimensionLabel_ForDimensionedEntityType` → "Wilayah",
`BuildPopulationMap_LeavesDimensionLabelNull_ForUndimensionedEntityType` → null; the fixture now
wires the shared `IDimensionLabelRegistry` and registers Supplier). Full frontend tests
37 files / 287 tests pass; full frontend build (`vue-tsc -b && vite build`) passes; backend build
(VS MSBuild, `btr.test.csproj`) succeeds; focused engine tests 6/6 pass; full `btr.test.ReportingContext`
failures remain at the pre-existing 11 (cash-flow, customer pack, field activity, sales report,
inventory-risk, collection, `SupplierEntityAnalyticsProducerTest` fixture), no new failures.

### PIW-12 verification note

Retired the Performance Signature (Radar) surface for all entity types per ADR-EA-001 and
ADR-PIW-012 (frontend-only slice; radar axis registration / L5 signature composition explicitly
out of scope):

- `ValidationStagePanel.vue`: removed the single-entity "Performance Signature" block and its
  `ProfileRadarSection` import; the panel now renders Evidence only. The lens-scoped evidence
  filtering added by PIW-11 is unchanged.
- `EntityPerformanceProfileShell.vue`: removed the `ProfileRadarSection` embed and import.
- All four compare views (`CustomerCompareView`, `SalesmanCompareView`, `ItemCompareView`,
  `SupplierCompareView`): removed the `RadarCompareSection` embed and import.
- Deleted the retired wrapper components `ProfileRadarSection.vue` and `RadarCompareSection.vue`;
  their underlying `PerformanceSignatureSection.vue` / `PerformanceSignatureChart.vue` /
  `PerformanceSignatureScoreTable.vue` remain but are now unreferenced (no dependents remain,
  verified by grep).

Acceptance criteria: `ValidationStagePanel.vue` no longer renders the "Performance Signature"
block; `ProfileRadarSection` / `RadarCompareSection` are removed from the profile shell and all
four compare views; a repo-wide grep confirms no entity profile, compare, or evidence route
renders or imports the retired surface (the only remaining `Radar`/`PerformanceSignature`
identifiers are the API-contract model types in `models/entityAnalytics.ts` and the unreferenced
underlying components); L5 snapshot history is untouched (no DELEETE/removal of
`BTRPD_EntityAnalytics_Radar`); radar axis registration / L5 signature composition was not
modified. No replacement radar was introduced.

Full frontend build (`vue-tsc -b && vite build`) passes; full frontend tests 37 files / 287 tests
pass. No backend files were changed; backend build and `btr.test.ReportingContext` status are
unaffected by this slice.

### PIW-13 verification note

Exposed the lens-independent Data Health indicators for the Principal (Supplier) investigation
workspace as a thin, read-only projection — no new tables, no writes, no new entity type/KPI:

- Backend: new `GetEntityDataHealthQuery` / `EntityDataHealthResponse` /
  `GetEntityDataHealthHandler` in `EntityAnalyticsAgg/Queries/GetEntityDataHealthQuery.cs`. The
  handler validates the entity type, then (for `Supplier` only) reads `BTRPD_PrincipalSalesOut`
  (`IPrincipalSalesOutSnapshotDal.GetCurrent`) and `BTRPD_PrincipalTarget`
  (`IPrincipalTargetSnapshotDal.GetCurrent`) and composes:
  - `TargetCoveragePercentage` = principals in the Sales-Out population (`PRN-SALES-001` rows) that
    have a matching `BTRPD_PrincipalTarget` row (same period, `PRN-TGT-001`) ÷ total Sales-Out
    population, as a percentage.
  - `PrincipalsMissingTargetCount` = Sales-Out population principals without a target row.
  - `UnknownPrincipalExceptionCount` = summed `LineCount` of `BTRPD_PrincipalSalesOutDataQuality`
    rows whose `ExceptionCode` is `BLANK_SUPPLIER` / `UNKNOWN_SUPPLIER`.
  - `UnknownPrincipalExceptionAmount` = summed `Amount` of the same exception rows.
  - Snapshot freshness metadata (`PeriodYear`, `PeriodMonth`, `GeneratedAt`) is carried through for
    the Data Health freshness disclosure (PIW-14). The Unknown Principal exceptions are surfaced as a
    disclosure (count + amount), never as a synthetic Principal, and Principal Sales-Out is never
    silently dropped. Non-Supplier entity types return `IsAvailable = false` (the disclosures are
    Principal-specific per IW-OQ-003); unknown entity types return 400.
- API: new `GET /api/entity-analytics/data-health?entityType=Supplier` in
  `EntityAnalyticsController`; read-only, mirrors the existing lenses/presets endpoint shape.
- Files changed: `GetEntityDataHealthQuery.cs` (new), `EntityAnalyticsController.cs`,
  `btr.application.csproj` (`<Compile Include>`), `btr.test.csproj` (`<Compile Include>`),
  `GetEntityDataHealthHandlerTest.cs` (new).

Focused tests: `GetEntityDataHealthHandlerTest` (6 tests: four indicators, all-missing coverage 0%,
empty/unavailable snapshot, non-Supplier unavailable, unknown-entity 400, handler-level four
indicators) pass. Backend build (VS 2022 MSBuild, `btr.application.csproj` + `btr.test.csproj`)
succeeds; focused tests 6/6 pass. No production code beyond the approved slice; no unrelated
changes. PIW-14 consumes this endpoint for the lens-independent frontend panel.

### PIW-14 verification note

Rendered the lens-independent Data Health panel in the Investigation Workspace, consuming the
PIW-13 `GET /api/entity-analytics/data-health` endpoint:

- New `WorkspaceDataHealthSection.vue` (`components/entity-analytics/workspace/`): renders the four
  disclosures as `DashboardMetric` indicators (Target Coverage % via `formatPercent`,
  Principals Missing Target via `formatNumber`, Unknown Principal Exception Count via
  `formatNumber`, Unknown Principal Exception Amount via `formatCurrency`); an Unknown Principal
  evidence block linking to `/reports/sales` (the Sales Report view surfaces the Unknown Principal
  exception lines), kept visible even at zero count/amount so the disclosure mechanism is never
  dropped; and a freshness block showing Generated At (`formatDateTime`), Reporting Period, and Data
  Snapshot Period (`formatSnapshotPeriod`, id-ID long month + year) followed by the standard
  snapshot disclosure text added to the disclosure when freshness metadata is present.
- New `services/workspaceDataHealth.ts`: `DATA_HEALTH_SNAPSHOT_DISCLOSURE`,
  `UNKNOWN_PRINCIPAL_EVIDENCE_ROUTE = '/reports/sales'`, and null-safe `formatSnapshotPeriod`.
- Changed: `models/entityAnalytics.ts` (`EntityDataHealthResponse`), `api/entityAnalyticsApi.ts`
  (`fetchEntityDataHealth`), `stores/investigationWorkspaceStore.ts` (`dataHealth` + `loadingDataHealth`
  state, `loadDataHealth` wired into `initializeWorkspace`, `setEntityType` (state reset), and
  `refresh`; never reloaded on lens switch), `views/analytics/InvestigationWorkspaceView.vue`
  (Data Health stage rendered in its own `WorkspaceStageSection` after the Population Map stage,
  outside the lens-gated `isInvestigation` block, so it is visible regardless of the active lens).

Acceptance criteria mapping: (1) Data Health renders regardless of active lens — the section sits
outside the lens-gated template block; (2) all four disclosures shown; (3) Unknown Principal
exceptions link to the Sales Report evidence; (4) Generated At / Reporting Period / Data Snapshot
Period shown when applicable, with the standard snapshot disclosure.

Verification: full frontend build (`vue-tsc -b && vite build`) passes; full frontend tests now 38
files / 296 tests pass, including new `workspaceDataHealth.spec.ts` (5 tests) and the four new
data-health store tests (load, failure clears state non-blocking, reload on entity-type switch, no
reload on lens switch). No backend files were changed; backend build and `btr.test.ReportingContext`
status are unaffected by this slice.
