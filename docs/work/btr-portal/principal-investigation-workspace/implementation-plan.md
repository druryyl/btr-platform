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
| PIW-02 | Frontend Principal presentation labels | PLANNED |
| PIW-03 | Lens configuration model | PLANNED |
| PIW-04 | Lens switcher in workspace shell | PLANNED |
| PIW-05 | `principal-sales-out-map` preset and bubble encoding | PLANNED |
| PIW-06 | Current Facts KPI grouping by lens | PLANNED |
| PIW-07 | Sales-Out attention categories | PLANNED |
| PIW-08 | Purchasing relationship drivers | PLANNED |
| PIW-09 | Purchase-to-Sales-Out Ratio (derived metric) | PLANNED |
| PIW-10 | Workspace presentation refinements (tooltip/peer) | PLANNED |
| PIW-11 | Lens-scoped evidence presentation | PLANNED |
| PIW-12 | Platform-wide Performance Signature retirement | PLANNED |
| PIW-13 | Data Health backend data | PLANNED |
| PIW-14 | Data Health frontend panel | PLANNED |
| PIW-15 | Knowledge and terminology synchronization | PLANNED |

Lifecycle: `PLANNED → IN IMPLEMENTATION → IMPLEMENTED → IN REVIEW → GO` (or `NO-GO → REMEDIATION →
IN REVIEW → GO`).

### Review History

| Slice | Review date | Result | Findings | Remediation |
| --- | --- | --- | --- | --- |
| PIW-01 | 2026-09-11 | GO | None blocking; INFO-001 pre-existing `btr.test` compile error (out of slice scope) | Resolved outside PIW-01 (`DashboardAlertCenterComposerTest.cs` `IndexOf` fix, uncommitted); PIW-01 focused test passes |

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
