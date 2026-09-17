# FEASIBILITY ASSESSMENT

## Principal Investigation Workspace within Entity Analytics

| Field | Value |
| --- | --- |
| Status | Analysis only; no implementation |
| Assessment date | 2026-09-11 |
| Requested decision | GO / CONDITIONAL GO / NO GO |
| Authority relationships | Declared operating model: `principal-centric-analytics-migration/FEASIBILITY-ASSESSMENT.md`; KPI semantics: `principal-centric-analytics-migration/PRINCIPAL-KPI-REGISTRY.md`; workspace behavior: `entity-analytics/M32R-Investigation-Workspace-Architecture.md` |
| Evidence boundary | Repository artifacts, source code, and SQL definitions at assessment time; production profiling already recorded in `principal-centric-analytics-migration/PROFILING-BASELINE.md` |
| Planning handoff | READY — GAP-001 through GAP-017 all resolved; planning may begin with no open planning inputs |

---

## 1. Executive Summary

### Request

Assess whether the existing Supplier Investigation Workspace within Entity Analytics should be **replaced or evolved** into a **Principal Investigation Workspace** that follows the Principal-centric analytics model and supports two investigation lenses:

- **Sales-Out Lens** — performance, growth, achievement, returns, coverage, customer/salesman/item contribution, risks and opportunities.
- **Purchasing Lens** — purchase activity, purchase trends, purchase vs sales-out alignment, inventory exposure, stock risk, purchasing efficiency.

The workspace purpose is to **understand a Principal**, not to manage the Principal portfolio. Portfolio management, executive monitoring, ranking dashboards, and strategic analysis remain in Sales Dashboard (SA04 Principal Performance, EX01/EX02).

### Recommendation

**GO.** All blocking decisions are resolved (GAP-001 identity, GAP-002 lenses, GAP-003 default preset, GAP-004 map definition). The request is highly feasible because the platform, the entity pack, and the principal data assets already exist:

- Entity Analytics already treats Supplier/Principal as a **first-class investigation entity** with a full generic six-stage workspace.
- The Principal KPI packs (`PRN-SALES-001`, returns, target, achievement, growth, purchase-in, inventory, coverage) are **already composed onto the Supplier/Principal Entity Analytics profile** by `SupplierEntityAnalyticsProducer`.
- Both lenses have persisted, keyed Principal data (sales-out DPO, returns, purchase-in, inventory value/days, inventory risk, targets).
- The investigation workspace UI is **entity-type-neutral**; the Principal workspace reuses it without component redesign.

The remaining work is **not new analytics**. It is (a) presentation/configuration following the resolved identity, lens, preset, map, KPI, driver, attention, peer, and signature decisions, (b) terminology and evidence-scope synchronization, and (c) the platform-wide signature removal. These are configuration concerns, not architectural blockers.

### Feasibility Result

```text
FEASIBLE
```

Mapped to the requested verdict vocabulary:

```text
GO
```

No blocking condition remains. The remaining purchasing-lens KPI definitions (alignment, efficiency, purchase trend) are scoped planning inputs, not planning blockers.

### Decision status

All three decisions that shaped the planning scope are resolved:

1. ~~Entity identity and user-facing naming~~ — **RESOLVED (GAP-001, 2026-09-11):** Supplier remains the canonical domain, database, integration, and legacy-application identity; Principal is a presentation alias used by Analytics, Dashboard, Navigation, KPI Registry, Documentation, and UX. No new Principal master entity is introduced. `SupplierId` remains the authoritative technical identifier; evidence lineage still references `SupplierId`.
2. ~~Lens definition and default~~ — **RESOLVED (GAP-002, 2026-09-11; GAP-003, 2026-09-11):** two presentation lenses over the same Principal (`SupplierId`) — Sales-Out Lens and Purchasing Lens — exposed via a lens switcher in a single workspace; no duplicate navigation, entity type, profile, or master data. **Default resolved (GAP-003, 2026-09-11):** the workspace defaults to the Sales-Out-oriented preset `principal-sales-out-map`; Purchasing remains available through the Purchasing Lens selector with no purchasing capabilities removed.
3. ~~Purchasing-lens KPI scope~~ — scoped by GAP-002 focus lists; alignment resolved via the Purchase-to-Sales-Out Ratio (GAP-005); efficiency redefined as Inventory Replenishment Efficiency with no PO-based metrics (GAP-006); purchase trend source resolved via `PU-KPI-001` monthly-history reuse (GAP-007); purchasing relationship drivers resolved as purchase-evidence drivers (GAP-008).

---

## 2. Request Understanding

### Requested capability

A Principal investigation experience in Entity Analytics that answers **"understand this Principal"** across a Sales-Out lens and a Purchasing lens, using the established investigation model:

```text
Population → Current Facts → Context → Explanation → Validation → Evidence
```

### Business objective

- Make Principal the primary commercial entity to investigate, following migration GAP-001/GAP-002 of the principal-centric feasibility (Principal is the primary commercial responsibility dimension).
- Let a user understand one Principal's market performance (sales-out) **and** its supply side (purchasing/inventory) without conflating the two (GAP-010).
- Preserve the separation between investigation (understand) and dashboarding (manage/rank/monitor).

### Expected outcome

- A user selects a Principal and can answer: how is it performing, growing, achieving, converting returns; who buys it, who sells it, which items drive it; what is purchased, what is in stock, what is at risk; and where is the evidence.
- Existing purchase-in data is presented as purchasing context, never relabeled as sales performance.

### Non-goals of this assessment

- Does not decide unresolved business rules.
- Does not define a target architecture or an implementation plan.
- Does not authorize code, database, or permanent knowledge changes.

---

## 3. Current State Analysis

### 3.1 Capability inventory

#### A. Investigation workspace platform (generic, entity-neutral)

| Capability | Existing asset | Evidence |
| --- | --- | --- |
| Workspace route | `/analytics/:entityType/workspace` | `btr.portal.web/src/router/index.ts` |
| Stage orchestration | Population, Current Facts, Context, Explanation, Validation | `btr.portal.web/src/views/analytics/InvestigationWorkspaceView.vue` (stage titles and panels) |
| Population map | canvas scatter, tiers, selection, hover, search, filter, zoom | `btr.portal.web/src/components/entity-analytics/workspace/PopulationMapCanvas.vue` |
| Preset engine | map presets per entity type and axis KPIs | `EntityMapPresetRegistry.cs`; `GET /api/entity-analytics/presets` |
| Profile sections | Overview, KpiSummary, Comparison, Trend, Radar, Ranking, Attention, RelatedEntities, Evidence | `GetEntityPerformanceProfileQuery.cs` |
| L0–L5 layer repositories | Current, Monthly, Ranking, Attention, Relationship, Radar | `IEntityAnalyticsLayerRepositories.cs` |
| Attention / relationship / radar engines | diff, persist, score at refresh | `EntityAttentionEngine`, `EntityRelationshipEngine`, `EntityRadarEngine` |
| Evidence resolvers | per-entity catalog-backed links | `IEntityProfileEvidenceResolver` implementations |
| Producer orchestration | domain-triggered per entity type | `EntityAnalyticsProducerOrchestrator`; `RefreshDashboardPurchasingManagementSnapshotWorker` |
| Peer distribution | peer-group rule per entity type | `PeerGroupResolver.cs`; `GET /api/entity-analytics/peer-distribution` |

Key fact: the workspace is **entity-type-neutral** (M32R Principle 9). A Principal workspace requires no new workspace component design; entity-specific content is configuration.

#### B. Principal / Supplier entity pack

| Capability | Value | Evidence |
| --- | --- | --- |
| Registered entity type | `Supplier`, DisplayName `"Supplier"` | `EntityAnalyticsPlatformRegistrar.cs:47-56` |
| KPI pack | `supplier-default` | same |
| Relationship pack | `supplier-relationships` | same |
| Peer group rule | `supplier-all-active` | same; `PeerGroupResolver.cs:16,54-66` |
| Worker domain hook | `PurchasingManagement` | same; scheduler case |
| Profile route | `/analytics/suppliers/{id}` | same |
| Single writer | `SupplierEntityAnalyticsProducer` | `SupplierEntityAnalyticsProducer.cs` |
| Entity identity | `EntityId = SupplierId`, `EntityCode = SupplierCode` | developer guide §Supplier pack |

#### C. Principal KPI data assets (Sales-Out lens)

| KPI | Registry ID | Persisted projection |
| --- | --- | --- |
| Principal Sales-Out (DPP) | `PRN-SALES-001` | `BTRPD_PrincipalSalesOut` (+Kpi, +DataQuality) |
| Sales-Out monthly history | — | `BTRPD_PrincipalSalesOutHistory` |
| Good / Broken / Total Return, Return % | `PRN-RET-001..004` | `BTRPD_PrincipalReturn`, `BTRPD_PrincipalReturnPercentage`, `BTRPD_PrincipalReturnHistory` |
| Principal Target | `PRN-TGT-001` | `BTRPD_PrincipalTarget` |
| Achievement Amount / % | `PRN-TGT-002`, `PRN-TGT-003` | `BTRPD_PrincipalAchievement` |
| MoM / YoY Growth | `PRN-GRW-001`, `PRN-GRW-002` | `BTRPD_PrincipalMomGrowth`, `BTRPD_PrincipalYoyGrowth` |
| Active Customer Count / Coverage % | `PRN-CUS-001`, `PRN-CUS-002` | `BTRPD_PrincipalActiveCustomer`, `BTRPD_PrincipalCustomerCoverage` |
| Customer × Principal relationship | (supporting) | `BTRPD_CustomerPrincipalRelationship` |
| Principal × Salesman contribution | (supporting) | `BTRPD_PrincipalContribution` |

Composition status: these are already written onto the Supplier/Principal Entity Analytics profile (PCM-015, PCM-016, PCM-046, PCM-047, PCM-048, PCM-052, PCM-053, PCM-055). `PRN-SALES-001` is the default ranking KPI.

#### D. Purchasing-lens data assets

| Capability | Existing asset |
| --- | --- |
| Purchase-In | `PRN-PUR-001` → `BTRPD_PrincipalPurchaseIn` |
| MTD Purchase, MTD Invoice Count, Posted % | `PU-KPI-001/002/003` (metadata-driven, trend-eligible; GAP-007: `PU-KPI-001` monthly history is the approved purchase-trend source) |
| Inventory Value / Inventory Days | `PRN-INV-001`, `PRN-INV-002` → `BTRPD_PrincipalInventory` |
| Inventory by Supplier | `BTRPD_InventoryBreakdown.SupplierId`, `BTRPD_InventoryRiskBreakdown.SupplierId` (PCM-051) |
| Purchase / inventory / risk aggregation | `DashboardPurchasingManagementAggregator`; `BTRPD_PurchasingManagementTopPrincipal` |
| Attention signals (purchasing/inventory) | `SupplierAttentionSignalCatalog`: Qualified Backlog, Spend Concentration, Inventory Concentration, At-Risk Exposure, Compound Dependency, Inventory-No-Purchase, Unknown Principal |
| Purchasing evidence | `/reports/purchasing`, `/reports/inventory` |

#### E. Presentation, navigation, and terminology

| Area | Current state |
| --- | --- |
| Navigation | Entity Analytics under Executive (`EX03`); Principal Performance under Sales (`SA04`); Purchasing `PU01/PU02` |
| Supplier profile/compare wrappers | `SupplierProfileView.vue`, `SupplierCompareView.vue` (thin wrappers over generic shell) |
| Workspace labels | raw `entityType` ("Supplier") is displayed; "Principal" is not used inside the workspace |
| Terminology authority | Migration GAP-017: "Principal" is the standard **user-facing** term; "Supplier" retained technically. Applied to this workspace by GAP-012 (presentation terminology mapping) and governed by GAP-017 (this assessment): business-facing surfaces use Principal (menus, navigation, workspace titles, profile pages, dashboard titles, KPI descriptions, user docs, disclosures); technical artifacts use Supplier (schema, `SupplierId`, API internals, code identifiers, registrations, legacy integrations); no technical identity migration |
| Sales-Out vs purchase semantics | GAP-010: Supplier/Principal Entity Analytics represents **sales-out performance**; purchase-in remains valid but separate |

### 3.2 Workspace model compatibility (Population → Evidence)

| Stage | Backend section | Supplier/Principal readiness |
| --- | --- | --- |
| Population | Population Map engine + presets | Map defined, pending creation: the two supplier presets are **purchase/inventory-led**; approved default `principal-sales-out-map` is X=Achievement %, Y=YoY Growth %, size=Sales-Out, color=Return % (GAP-004). Bubble size/color encoding to be validated against current map capability in planning |
| Current Facts | KPI Summary | Ready; principal KPI packs present |
| Context | Peer Position, Trend (L1), Signal History (L3), Position History (L2) | Mostly ready; peer position works via `supplier-all-active`; attention covers both lenses (GAP-009): Sales-Out Decline, Growth Deterioration, Target Miss, Return Risk, Coverage Deterioration plus existing purchase/inventory signals |
| Explanation | RelatedEntities | Sales-Out relationships ready (Top Customers/Salesmen/Products, `PRN-SALES-001`); Purchasing drivers defined (GAP-008: Top Purchased Items from invoice detail, Purchase History from monthly history) |
| Validation | Evidence resolver | Ready; sales-out → principal-performance evidence, purchase/inventory → reports; panels, links, titles, and disclosures indicate the originating lens (GAP-013), no duplicate evidence model |
| Performance Signature | Radar (L5) | **RETIRED (GAP-010, ADR-EA-001, 2026-09-11):** no Principal Performance Signature; no dual-lens signature; no replacement radar. Investigation relies on KPI Summary, Population Position, Trajectory, Business Drivers, Relationships, Attention Signals, Evidence |

Conclusion: the six-stage model is **structurally compatible**; the gaps are **lens configuration and purchasing-lens content**, not platform capability.

### 3.3 Existing degree of Principal readiness

| Subsystem | Finding |
| --- | --- |
| Workspace platform | Fully reusable; entity-neutral |
| Entity registration | Supplier is first-class but named "Supplier", not "Principal" |
| Profiles | Principal KPIs already composed onto the profile |
| Presets | Purchase-led; no sales-out preset |
| Peer group | All-active only; no dimension-based Principal peer group; selector hidden for Supplier in the UI |
| Attention | Purchasing/inventory signals plus Sales-Out signals (GAP-009) |
| Radar | Retired for all entity types (ADR-EA-001) |
| Explanation | Sales-out relationships ready; purchasing drivers defined from purchase invoice evidence (GAP-008) |
| Evidence | Present and lens-distinguishable by report route |
| Terminology | Technical `Supplier` keys vs user-facing `Principal` |

---

## 4. Impact Analysis

The assessment anticipates impact by area. It does not prescribe implementation.

### 4.1 Backend impact

- Entity type registration metadata (`EntityAnalyticsPlatformRegistrar`): display mapping only (GAP-012); technical registration (`Supplier`) unchanged.
- `EntityMapPresetRegistry` — add/adjust Principal lens presets.
- `SupplierEntityAnalyticsProducer` / related composition — no new analytics expected for the sales-out lens; purchasing-lens presentation may require composition of already-stored KPIs.
- Attention catalog approved for both lenses (GAP-009). Radar retired platform-wide; no signature work remains (GAP-010, ADR-EA-001).
- Relationship catalog — purchasing-lens relationships are not present.
- Evidence resolvers — lens-scoped presentation approved (GAP-013); no duplicate evidence model.

### 4.2 Database impact

- No new table is required for the sales-out lens (all `BTRPD_Principal*` projections exist).
- Possible additions only if new purchasing-lens KPIs beyond the resolved set are approved. No `PRN-PUR-001` history projection (GAP-007).
- No schema change is required for Item attribution (`Brg.SupplierId`).

### 4.3 Frontend impact

- Principal naming in entity metadata, navigation registry, workspace labels, breadcrumb, and scope label.
- Principal lens presentation (preset labeling, KPI grouping, lens switcher per GAP-002).
- Peer Group Selector remains hidden for Principal; all-active peer population (GAP-011).
- No core workspace component redesign expected.

### 4.4 Integration impact

- None identified for the investigation workspace; no external system is involved.
- Refresh ordering already handled within the portal worker.

### 4.5 Security impact

- None. Per GAP-018, Principal is an analytical dimension, not a security boundary; existing portal authorization applies.

---

## 5. Gap Analysis

| Gap ID | Type | Description | Severity |
| --- | --- | --- | --- |
| GAP-001 | Functional | The workspace's entity identity is **Supplier**; the requested workspace is **Principal**. **RESOLVED 2026-09-11:** Supplier remains the canonical domain/database/integration/legacy identity; Principal is a presentation alias for Analytics, Dashboard, Navigation, KPI Registry, Documentation, and UX. No new Principal master entity. | Resolved |
| GAP-002 | Functional | No product definition of the two **lenses**. **RESOLVED 2026-09-11:** a lens is a presentation perspective (not a new entity type, not a separate workspace); both lenses investigate the same Principal (`SupplierId`) in a single workspace with a lens switcher. | Resolved |
| GAP-003 | UX | The **default Principal preset is purchase-led** (`purchase-exposure-map`). **RESOLVED 2026-09-11:** the workspace defaults to the Sales-Out-oriented preset `principal-sales-out-map`; Purchasing remains available through the lens selector with no purchasing capabilities removed. | Resolved |
| GAP-004 | Functional | No **Sales-Out population preset** existed. **RESOLVED 2026-09-11:** default Sales-Out Population Map defined — X-Axis Achievement %, Y-Axis YoY Growth %, Bubble Size Principal Sales-Out, Bubble Color Return %; purpose: identify Star, Growing, Underperforming, Declining, and High-return-risk Principals. It becomes the default population preset for the Sales-Out Lens; additional presets later must not change default behavior. | Resolved |
| GAP-005 | Data | Purchasing lens lacked a defined **purchase vs sales-out alignment** KPI. **RESOLVED 2026-09-11:** derived investigation KPI Purchase-to-Sales-Out Ratio = Purchase Amount ÷ Principal Sales-Out; >1.0 = Inventory Building / Potential Over-Buying; ≈1.0 = Balanced; <1.0 = Inventory Drawdown / Potential Under-Buying. Investigation-only; not a ranking, performance, or financial KPI. Purpose: assess purchasing alignment with commercial demand. | Resolved |
| GAP-006 | Data | Purchasing lens lacked a defined **efficiency** model. **RESOLVED 2026-09-11:** efficiency is redefined as **Inventory Replenishment Efficiency** — the organization operates no formal Purchase Order workflow, so efficiency is not measured with PO-based metrics. Purchasing Lens focuses on Purchase Amount, Purchase Growth, Purchase-to-Sales-Out Ratio, Inventory Value, Inventory Days of Supply, Slow Moving Exposure. No PO Fulfillment, PO Backlog, or Outstanding PO metrics are required. | Resolved |
| GAP-007 | Data | `PRN-PUR-001` was current-only with no approved purchase-trend source. **RESOLVED 2026-09-11:** `PRN-PUR-001` remains the authoritative current-period Principal Purchase metric; historical purchase trend is sourced from existing `PU-KPI-001` monthly history (valid because Supplier and Principal share the same `SupplierId` identity). No separate Principal purchase-history projection is introduced at this stage; future migration to a dedicated Principal history series remains optional. | Resolved |
| GAP-008 | Functional | **Purchasing-lens relationship drivers** were undefined. **RESOLVED 2026-09-11:** investigation uses existing Purchase Invoice evidence — (1) Top Purchased Items from Purchase Invoice Detail, (2) Purchase History from Monthly Purchase Amount history. No additional purchasing relationship model; no PO-based drivers. Purpose: explain purchase volume and trends from existing evidence. | Resolved |
| GAP-009 | Functional | Entity attention signals were purchase/inventory only. **RESOLVED 2026-09-11:** the workspace supports both Sales-Out and Purchasing attention signals; existing EX01/EX02 Sales-Out signals reused where applicable. Principal categories: Sales-Out Decline, Growth Deterioration, Target Miss, Return Risk, Coverage Deterioration. Purchase/inventory signals remain. No new attention engine. | Resolved |
| GAP-010 | Functional | **Performance Signature (radar)** needed a dual-lens signature model. **RESOLVED 2026-09-11:** Performance Signature (Radar) is retired from Entity Analytics for all entity types (ADR-EA-001); no Principal Performance Signature; no dual-lens signature; no replacement radar. Investigation relies on KPI Summary, Population Position, Trajectory, Business Drivers, Relationships, Attention Signals, Evidence. | Resolved |
| GAP-011 | UX | **Peer group selector was disabled for Supplier** with only all-active grouping. **RESOLVED 2026-09-11:** the workspace uses the existing all-active Principal population (`supplier-all-active`) as its peer population; no Principal-specific Peer Group Selector. Rationale: small population, all-active comparison sufficient, existing peer-position capability works, extra dimensions low value vs cost. Peer Position available; selector hidden. | Resolved |
| GAP-012 | UX | Workspace surfaces displayed raw `entityType` with no Principal terminology. **RESOLVED 2026-09-11:** presentation terminology mapping — Entity Analytics displays Principal Investigation Workspace, Principal Profile, Compare Principals, Principal Relationships, Principal Evidence. Internal identifiers (`SupplierId`, `entityType=Supplier`, Supplier registrations) unchanged. No database, API, snapshot, or domain migration. | Resolved |
| GAP-013 | UX | Evidence was not explicitly **lens-scoped** in presentation. **RESOLVED 2026-09-11:** all Principal evidence is associated with the active lens. Sales-Out examples: Faktur Item, Return Item, Customer/Salesman Contribution. Purchasing examples: Purchase Invoice, Purchase History, Inventory Exposure, Inventory Movement. Panels, links, titles, and disclosures indicate the originating lens. No duplicate evidence model. | Resolved |
| GAP-014 | Governance | `EntityTypeCode` has no `Principal`. **RESOLVED 2026-09-11:** alias approach adopted — no new Principal master entity; `Supplier` technical identifiers retained. | Resolved |
| GAP-015 | UX | Population map tooltip hardcoded "Category" for the dimension value. **RESOLVED 2026-09-11:** hide the dimension row from the tooltip when no meaningful Principal dimension exists; do not display the generic "Category" label. | Resolved |
| GAP-016 | Data | Purchasing principal identity was name-based in parts of the purchasing aggregator. **RESOLVED 2026-09-11:** `SupplierId` is the authoritative Principal identity across all investigation capabilities; all Purchasing Lens aggregations, relationships, snapshots, trends, and evidence are sourced from `SupplierId`-keyed data. `SupplierName` is presentation only; name-based aggregation, matching, or joining is not permitted. Historical data may display the recorded `SupplierName`, but identity resolution uses `SupplierId`. | Resolved |
| GAP-017 | Governance | Terminology across docs/UI needed synchronization. **RESOLVED 2026-09-11:** terminology governance rule — business-facing surfaces use Principal (menus, navigation, workspace titles, profile pages, dashboard titles, KPI descriptions, user documentation, disclosures); technical artifacts use Supplier (database schema, `SupplierId`, API internals, code identifiers, analytics registrations, legacy integrations). No technical identity migration required. | Resolved |

No gap requires new platform capability; all are configuration, presentation, or bounded definition work.

---

## 6. Solution Options

### Option A — Evolve the existing Supplier entity type into a Principal workspace

Reuse `EntityTypeCode.Supplier`, keep technical keys, change user-facing identity to "Principal", and add lens presets/grouping.

**Advantages**

- Preserves the single writer, snapshot keys, routes, catalogs, and L0–L5 history.
- Zero duplicate profiles; minimal data risk.
- Consistent with migration GAP-010/GAP-011 and migration GAP-017 (technical Supplier retained).
- Fastest and lowest risk.

**Disadvantages**

- Requires discipline to keep "Principal" user-facing and "Supplier" technical.
- Display/navigation changes touch several registries.

**Risk:** Low.

### Option B — Introduce a new `Principal` entity type alongside `Supplier`

Add a distinct `Principal` entity type/producer/registrar.

**Advantages**

- Clean conceptual identity.

**Disadvantages**

- Creates two entity types over the same `SupplierId` population; risks duplicate/conflicting profiles and violates the single authoritative aggregation constraint (migration GAP-011).
- Requires a second producer over the same snapshots or snapshot duplication.
- High migration and consistency cost; no data benefit.

**Risk:** High. **Recommendation: reject.**

### Option C — Keep Supplier as-is and add a Principal sales-out preset only

Treat the workspace as Supplier with a sales-out preset; no identity change.

**Advantages**

- Minimal change.

**Disadvantages**

- Does not satisfy "understand a Principal" framing; confusing terminology remains.
- Lens model is implicit rather than designed.

**Recommendation:** Acceptable only as an interim step, not the target state.

### Recommended option

**Option A** — the default (GAP-003) is decided. The GAP-001 resolution (2026-09-11) confirms this option and rejects Option B: no new Principal master entity will be introduced. The GAP-002 resolution (2026-09-11) confirms a single workspace with a lens switcher: no duplicate navigation, entity type, profile, or master data.

---

## 7. Recommended Approach (implementation-neutral)

Adopt **Option A** and deliver the Principal workspace as a **two-lens configuration of the existing investigation workspace**:

1. **Identity (resolved)** — Supplier remains the canonical domain/database/integration/legacy identity; Principal is the presentation alias in Analytics, Dashboard, Navigation, KPI Registry, Documentation, and UX (GAP-001 resolution, 2026-09-11). `SupplierId` remains authoritative; evidence lineage still references `SupplierId`. Single entry: the current Supplier Investigation entry is replaced (BQ-005); no parallel Supplier/Principal entries.
2. **Lenses (resolved)** — the workspace supports two presentation lenses over the same Principal (`SupplierId`), exposed via a lens switcher in a single workspace (GAP-002 resolution, 2026-09-11). Sales-Out Lens focuses on Sales-Out, Growth, Achievement, Returns, Coverage, Customer Contribution, Salesman Contribution, Item Contribution. Purchasing Lens focuses on Purchase Amount, Purchase Growth, Purchase Trend, Inventory Exposure, Stock Risk, Purchase vs Sales-Out Alignment (GAP-002), refined as Inventory Replenishment Efficiency: Purchase Amount, Purchase Growth, Purchase-to-Sales-Out Ratio, Inventory Value, Inventory Days of Supply, Slow Moving Exposure, with no PO-based metrics (GAP-006 resolution, 2026-09-11). The Sales-Out lens is the primary performance lens; the Purchasing lens is operational context (GAP-010). Purchase-in must never be relabeled or ranked as sales performance. Default resolved (GAP-003, 2026-09-11): Sales-Out-oriented preset `principal-sales-out-map` with primary exploration dimensions Principal Sales-Out, Growth, Achievement, Coverage, Returns; Purchasing available through the lens selector.
3. **Population (map resolved)** — introduce `principal-sales-out-map` as the workspace default with X-Axis Achievement %, Y-Axis YoY Growth %, Bubble Size Principal Sales-Out, Bubble Color Return %, serving identification of Star, Growing, Underperforming, Declining, and High-return-risk Principals; retain the purchasing presets for the Purchasing Lens; later presets must not change default behavior.
4. **Current Facts & Context** — group the principal KPI packs by lens; reuse existing Trend, Ranking, Attention, and Peer Position engines with the expanded dual-lens attention (GAP-009). No signature (retired, GAP-010).
5. **Explanation** — reuse the existing sales-out relationship drivers; add purchasing-lens drivers only if the purchasing-lens relationship scope is approved.
6. **Validation / Evidence (resolved)** — evidence routing is explicitly lens-scoped (GAP-013): Sales-Out (Faktur Item, Return Item, Customer/Salesman Contribution) vs Purchasing (Purchase Invoice, Purchase History, Inventory Exposure, Inventory Movement); panels, links, titles, and disclosures indicate the originating lens; no duplicate evidence model. Do not present purchase evidence as sales-out evidence.
7. **Diagnostics (resolved)** — surface data-quality and completeness through a dedicated **Data Health** section visible regardless of lens (OQ-003): Target Coverage %, Principals Missing Target Count, Unknown Principal Exception Count, Unknown Principal Exception Amount, with navigation to supporting evidence for Unknown Principal exceptions. Rationale: both conditions affect interpretation and must be visible before conclusions are drawn.

Constraints:

- No new Principal master entity, no second Principal entity type, and no duplicate producer (GAP-001 resolution, 2026-09-11; migration GAP-011).
- Purchase-in is never the Principal performance/ranking KPI (PRINCIPAL KPI REGISTRY; GAP-010).
- Returns do not reduce the authoritative Sales-Out KPI (PRINCIPAL KPI REGISTRY; GAP-020).
- No Principal financial/credit/collection KPIs (migration GAP-005).
- Purchase-to-Sales-Out Ratio is investigation-only; never a ranking, performance, or financial KPI (GAP-005 resolution, 2026-09-11).
- No new Principal KPI IDs; alignment is derived from existing measures and existing KPI semantics remain unchanged (TQ-005, 2026-09-11).
- No new KPI stewardship model: derived investigation metrics require no KPI ownership, registry maintenance, ranking contracts, or evidence-route ownership; evidence ownership stays with source providers and existing contracts (OQ-001, 2026-09-11).
- Standard freshness policy: every view displays Generated At, Reporting Period, Data Snapshot Period (when applicable); no real-time guarantee; all outputs interpreted relative to the displayed snapshot (OQ-002, 2026-09-11).
- No transfer of ranking/portfolio-management responsibilities into the workspace; those remain in SA04/EX01/EX02.

### Recommended implementation sequencing (capability order — not an implementation plan)

This is a recommended order of capability decisions/work for later planning; it is not a slice list or an implementation plan.

1. **Product decisions and governance** — all section 9 decisions resolved; apply the terminology governance rule (GAP-017) across surfaces and artifacts during planning. Standards owned by the BTR Portal Product Owner; implementation teams apply (OQ-004).
2. **Principal identity and terminology** — display name, navigation/labels, URL/route display, without renaming technical keys.
3. **Lens presets** — create the defined `principal-sales-out-map` default alongside retained Purchasing presets; later presets must not change default behavior.
4. **Population and peer scoping (resolved)** — peer population is all-active (`supplier-all-active`); no Principal-specific selector or filter dimension (GAP-011).
5. **Current Facts and Context per lens** — KPI grouping, trend, ranking, attention; include the investigation-only Purchase-to-Sales-Out Ratio in the Purchasing Lens (GAP-005); attention spans both lenses with EX01/EX02 Sales-Out signal reuse where applicable (GAP-009); no signature (GAP-010).
6. **Explanation per lens (drivers resolved)** — Sales-Out drivers are the existing Top Customers/Salesmen/Products relationships; Purchasing drivers are Top Purchased Items (Purchase Invoice Detail) and Purchase History (monthly history), with no additional model and no PO-based drivers (GAP-008, BQ-010 YES). Excluded: PO-based, purchasing-user, and procurement-workflow relationships.
7. **Validation and evidence lineage** — lens-scoped evidence routes and disclosures.
8. **Knowledge synchronization** — entity analytics feature artifacts, developer guide, KPI catalog, navigation assets.

---

## 8. Risks and Constraints

| Risk | Impact | Probability | Mitigation |
| --- | --- | --- | --- |
| Purchase-in is conflated with sales-out in a combined workspace | High | High | Enforce GAP-010: separate lens sections, labels, and evidence routes; never rank by purchase. |
| A new `Principal` entity type duplicates the Supplier profile | High | Medium | Reject Option B; keep one entity type, one producer, one authoritative profile (migration GAP-011). |
| Renaming the entity type breaks snapshot keys, routes, catalogs, or URLs | High | Medium | Keep technical `Supplier` keys; change display/presentation only unless a separate migration is approved. |
| Mixed-axis population map confuses users across lenses | Medium | Medium | Define lens-specific presets with explicit axis meaning; default to the primary lens. |
| Purchasing principal identity name-based mismatch | Low | Low | Decided (GAP-016): `SupplierId`-keyed data only; `SupplierName` presentation only; no name-based aggregation, matching, or joining. |
| Low target coverage biases the Achievement lens | Medium | High | PROFILING-BASELINE: only 2026-06 targets exist (7.23% combination coverage). Present achievement as partial via the dedicated Data Health section (OQ-003): Target Coverage %, Missing Target Count, Unknown Principal count/amount, with evidence navigation. |
| Historical sales-out uses current Item master (attribution immutability) | Medium | Low | GAP-004 approved; retained history disclosure already implemented. |
| Radar retirement rollout is platform-wide (Customer, Salesman, Item, Principal) | Medium | Medium | Retire the shared signature surface once (`ValidationStagePanel` embedding, radar sections, compare radar); verify no entity profile or evidence route depends on it. |
| Stale data (latest invoice 2026-06-24 in baseline) misread as current | Medium | Medium | Standard freshness policy (OQ-002): every view displays Generated At, Reporting Period, Data Snapshot Period (when applicable); all outputs interpreted relative to the displayed snapshot; no real-time guarantee. |
| Purchase trend sourced from reused `PU-KPI-001` monthly history | Medium | Low | Approved reuse via shared `SupplierId` identity (GAP-007); future dedicated Principal history series remains optional. |
| Small Principal population limits peer granularity | Low | Low | Accepted per GAP-011: all-active comparison against all active Principals is sufficient; no peer dimensions; selector hidden. |
| Terminology drift between docs and UI | Low | Low | Governance rule decided (GAP-017): business-facing Principal, technical Supplier; apply mapping and synchronize artifacts. |

Constraints:

- Investigation, not portfolio management: no ranking dashboards or strategic analysis added to the workspace.
- Single authoritative Principal analytics layer.
- No Principal-scoped authorization (GAP-018).
- No new business rules may be invented by this assessment.

---

## 9. Open Questions

Items marked **BLOCKING** prevent planning. Items marked **RESOLVED** record approved answers; the remaining rows are unresolved. Every resolved row preserves its original question followed by the approved answer, so no asked question is lost.

### Business questions

| ID | Status | Question |
| --- | --- | --- |
| BQ-001 | **RESOLVED 2026-09-11** | Is the existing Supplier investigation entity **renamed to Principal** in user-facing surfaces, or **dual-labeled** ("Principal", formerly Supplier)? **Answer (GAP-001):** neither — Supplier remains the canonical domain/database/integration/legacy identity; Principal is a presentation alias in Analytics, Dashboard, Navigation, KPI Registry, Documentation, and UX. No new Principal master entity. |
| BQ-002 | **RESOLVED 2026-09-11** | What is the exact **definition of each lens**, and how are they exposed (preset, tab, or mode)? **Answer (GAP-002):** two presentation lenses (not new entity types, not separate workspaces), both investigating the same Principal (`SupplierId`): (1) Sales-Out Lens — Sales-Out, Growth, Achievement, Returns, Coverage, Customer/Salesman/Item Contribution; (2) Purchasing Lens — Purchase Amount, Purchase Growth, Purchase Trend, Inventory Exposure, Stock Risk, Purchase vs Sales-Out Alignment. Single workspace with a lens switcher. |
| BQ-003 | **RESOLVED 2026-09-11** | Which lens is the **default** when the Principal workspace opens? **Answer (GAP-003):** the Sales-Out Lens, via the default preset `principal-sales-out-map` (dimensions: Principal Sales-Out, Growth, Achievement, Coverage, Returns). Rationale: Principal-centric analytics is primarily driven by commercial performance (Sales-Out); Purchasing is a supporting investigation perspective available through the lens selector. |
| BQ-004 | **RESOLVED 2026-09-11** | Is the **Purchasing lens** part of this workspace, or a read-only context that links to PU01/PU02? **Answer (GAP-002):** part of the workspace — the Purchasing Lens lives in the single Principal workspace via the lens switcher. |
| BQ-005 | **RESOLVED 2026-09-11** | Does the workspace replace the current **Supplier Investigation** entry, or add a distinct **Principal Investigation** entry? **Answer:** replace the current Supplier Investigation entry. No parallel Supplier/Principal entries. |
| BQ-006 | **RESOLVED 2026-09-11** | Is **purchase vs sales-out alignment** in scope, and who owns its definition? **Answer (GAP-005):** in scope as the derived investigation KPI Purchase-to-Sales-Out Ratio (Purchase Amount ÷ Principal Sales-Out) with the >1.0 / ≈1.0 / <1.0 interpretation. Computation placement and ownership are planning concerns. |
| BQ-007 | **RESOLVED 2026-09-11** | Is **purchasing efficiency** (beyond Posted % and backlog) in scope? Define the business meaning. **Answer (GAP-006):** efficiency is in scope as **Inventory Replenishment Efficiency**, not PO-based efficiency — no formal PO workflow exists. Purchasing Lens: Purchase Amount, Purchase Growth, Purchase-to-Sales-Out Ratio, Inventory Value, Inventory Days of Supply, Slow Moving Exposure. No PO Fulfillment, PO Backlog, or Outstanding PO metrics required. |
| BQ-008 | **RESOLVED 2026-09-11** | Should Principal peer grouping be all-active, by wilayah/category, or another dimension? **Answer (GAP-011):** all-active — the existing all-active Principal population is the peer population; no Principal-specific Peer Group Selector. |
| BQ-009 | **RESOLVED 2026-09-11** | Should lens-specific **attention signals** and **radar axes** be introduced, and which ones? **Answer:** attention per GAP-009 (both lenses, five Principal categories, EX01/EX02 reuse); radar axes moot — Performance Signature retired platform-wide (GAP-010, ADR-EA-001). |
| BQ-010 | **RESOLVED 2026-09-11 — YES** | Should purchasing-lens **relationship drivers** (top purchase items, purchase history) be shown? **Answer:** yes — show Top Purchased Items and Purchase History (consistent with GAP-008). Excluded: PO-based relationships, purchasing-user relationships, procurement workflow relationships. Rationale: explain purchase volume, replenishment behavior, and inventory exposure from existing purchase evidence. |

### Technical questions

| ID | Status | Question |
| --- | --- | --- |
| TQ-001 | **ANSWERED IN PRINCIPLE (verify in planning)** | Can the entity **display name** change to Principal while retaining `EntityTypeCode.Supplier`, snapshot keys, routes, and catalog identifiers? Confirm no opaque dependency on the literal "Supplier". **Answer (GAP-001):** yes — Database: no change; API contracts: preferably no change; Legacy desktop: no change; Analytics/UI, Navigation/Menu, KPI Registry display "Principal". Planning must still confirm no opaque dependency on the literal "Supplier". |
| TQ-002 | **RESOLVED 2026-09-11** | Is legacy `PU-KPI-001` monthly history sufficient for **purchase trend**, or must `PRN-PUR-001` gain history? **Answer (GAP-007):** sufficient — `PU-KPI-001` monthly history is the approved trend source; `PRN-PUR-001` remains current-period only with no separate history projection at this stage. |
| TQ-003 | | How should evidence links be **scoped per lens** to guarantee purchase evidence is never shown as sales-out? |
| TQ-004 | **RESOLVED 2026-09-11** | Does the current Principal population (≈27 principals, baseline) satisfy peer-position gates, and what is the minimum gate? **Answer (GAP-011):** the all-active Principal population is the peer population and is sufficient for investigation; radar gates moot (retired, GAP-010). |
| TQ-005 | **RESOLVED 2026-09-11 — NO** | Are any new KPI IDs required (alignment/efficiency), and can they be added without changing existing KPI semantics? **Answer:** no new Principal KPI IDs required. Purchase-to-Sales-Out Alignment is an investigation metric derived from existing Purchase Amount and Principal Sales-Out measures. No Purchasing Efficiency KPI is introduced. Existing KPI semantics remain unchanged. |
| TQ-006 | **RESOLVED 2026-09-11** | Does the purchasing lens need `SupplierId` keying normalization in any name-based purchasing output? **Answer (GAP-016):** yes — all Purchasing Lens aggregations, relationships, snapshots, trends, and evidence use `SupplierId`-keyed data; `SupplierName` is presentation only. |

### Operational questions

| ID | Status | Question |
| --- | --- | --- |
| OQ-001 | **CLOSED 2026-09-11** | Who maintains KPI definitions and evidence routes for any new purchasing-lens KPIs? **Answer:** no new purchasing-lens KPI registry entries; analytics use existing registered KPIs plus derived investigation metrics. Derived metrics require no KPI ownership, registry maintenance, ranking contracts, or dedicated evidence-route ownership. Evidence ownership remains with the source KPI providers and existing evidence contracts. No new KPI stewardship model required. |
| OQ-002 | **RESOLVED 2026-09-11** | What is the expected data freshness disclosure policy for the workspace? **Answer:** standard Entity Analytics freshness policy — every view displays Generated At timestamp, Reporting Period, and Data Snapshot Period (when applicable). No real-time data guarantee; all KPI values, rankings, relationships, trends, attention signals, and investigation outputs are interpreted relative to the displayed snapshot. Disclosure: "Data shown in this workspace is based on the latest available analytics snapshot and may not reflect transactions entered after the snapshot was generated." |
| OQ-003 | **RESOLVED 2026-09-11** | How will low target coverage and unknown-Principal exceptions be surfaced to users? **Answer:** dedicated **Data Health** section visible regardless of lens, displaying Target Coverage %, Principals Missing Target Count, Unknown Principal Exception Count, Unknown Principal Exception Amount, with navigation to supporting evidence for Unknown Principal exceptions. Rationale: both conditions affect interpretation and must be visible before conclusions are drawn. |
| OQ-004 | **RESOLVED 2026-09-11** | Which team owns navigation/terminology consolidation once identity is decided? **Answer:** the BTR Portal Product Owner — responsible for defining and maintaining business-facing terminology standards. Implementation teams apply the approved terminology across navigation, menus, workspace titles, dashboard titles, labels, disclosures, and user-facing documentation. Technical identifiers may continue using Supplier terminology where required by code, database, integration, and legacy-system constraints. |

---

## 10. Implementation Impact Inventory

This section is mandatory. It lists impacted areas only; it does not prescribe implementation.

### Backend

- Entity registration metadata: `EntityAnalyticsPlatformRegistrar` (identity/display).
- Preset definitions: `EntityMapPresetRegistry` (Principal lens presets).
- Composition: `SupplierEntityAnalyticsProducer` and produce input (only if purchasing-lens composition changes).
- Attention catalog: `SupplierAttentionSignalCatalog` (expanded per GAP-009: Sales-Out Decline, Growth Deterioration, Target Miss, Return Risk, Coverage Deterioration with EX01/EX02 reuse where applicable; purchase/inventory signals retained; no new engine).
- Radar metadata: retired — no Principal or dual-lens signature (GAP-010, ADR-EA-001); remove radar axis registration and L5 signature composition from planning scope.
- Relationship catalog: `SupplierRelationshipCatalog` (purchasing drivers approved GAP-008: Top Purchased Items from Purchase Invoice Detail, Purchase History from monthly history; no additional model).
- Evidence resolver: `SupplierEntityAnalyticsEvidenceResolver` (lens-scoped presentation per GAP-013: panels, links, titles, disclosures indicate the originating lens).
- Peer grouping: `PeerGroupResolver` unchanged — all-active retained, no Principal peer dimension (GAP-011).

### Database

- No new tables required for the Sales-Out lens (existing `BTRPD_Principal*` projections).
- No separate Principal purchase-history projection is introduced (GAP-007); new/extension tables only if other purchasing-lens KPIs are approved.
- No change to `BTR_FakturItem` / `BTR_ReturJualItem` (Item-master attribution remains, GAP-004).

### Frontend

- Entity identity/labels: apply the GAP-012 display mapping (Principal Investigation Workspace, Principal Profile, Compare Principals, Principal Relationships, Principal Evidence) across entity type metadata, `entityAnalyticsNavigation`, `investigationWorkspaceNavigation`, `profileOverviewLayout`, `peerGroupLabel`, scope/breadcrumb, home card; internal identifiers unchanged.
- Workspace presentation: lens grouping/labeling in `InvestigationWorkspaceView` and profile sections; `principal-sales-out-map` default; map encoding (bubble size/color per GAP-004, capability to be validated in planning).
- Peer selector stays hidden for Principal; no selector enablement work (`investigationWorkspaceStore` unchanged in this respect).
- Tooltip (`PopulationMapTooltip`): hide the dimension row when no meaningful Principal dimension exists; no generic "Category" label (GAP-015).
- Retire the Performance Signature embedding from the Validation panel and related radar sections/compare views for all entity types (ADR-EA-001; planning work, not a redesign).
- No redesign of `PopulationMapCanvas` or generic panels is assumed; bubble size/color encoding per GAP-004 must be validated against current map capability in planning.

### Integration

- None external.
- Internal refresh ordering: unchanged for the sales-out lens; the purchasing worker remains the trigger for the Principal Entity Analytics refresh.

### Security

- None. Principal remains an analytical dimension, not a security boundary (GAP-018).

---

## 11. Planning Readiness

### Status

```text
READY
```

Reason: all blocking questions resolved (BQ-001–BQ-004, TQ-001 via GAP-001–GAP-003). No blocking issues remain.

### Blocking Issues

```text
None.
```

Resolved: identity and naming (GAP-001); lens definition (GAP-002); default lens and preset (GAP-003).

### Planner Guidance

- **Implementation scope:** configuration and presentation of an existing, already-composed Principal entity pack; not new analytics for the Sales-Out lens. Purchasing-lens alignment/efficiency are the only potentially new KPI definitions.
- **Major dependencies:** approved business decisions; existing `BTRPD_Principal*` projections; single-writer `SupplierEntityAnalyticsProducer`; GAP-010 separation of sales-out and purchase-in.
- **Sequencing concerns:** identity/terminology first; then lens model/presets; then context/explanation/evidence refinements. Purchasing-lens new KPIs, if approved, are the largest uncertainty and should be isolated.
- **Review concerns:** verify that purchase-in is never presented or ranked as Principal sales performance; that Returns do not reduce `PRN-SALES-001`; that no Principal financial/credit/collection KPI is introduced; that exactly one authoritative Principal profile exists; and that no PO-based metric (Fulfillment, Backlog, Outstanding PO) is introduced, including re-verifying the existing Qualified Backlog attention signal against the no-PO-metrics rule; and that purchasing drivers use purchase invoice evidence only — Top Purchased Items and Purchase History, with no PO-based, purchasing-user, or procurement-workflow relationships (GAP-008, BQ-010); and that Sales-Out attention reuses EX01/EX02 signals where applicable with no new attention engine; and that no Performance Signature (Radar) is introduced for any entity type (retired, ADR-EA-001); and that the Peer Group Selector remains hidden for Principal with the all-active peer population (GAP-011); and that Entity Analytics displays the five GAP-012 Principal strings with technical identifiers unchanged; and that every evidence panel, link, title, and disclosure indicates its originating lens with no duplicate evidence model (GAP-013); and that the tooltip shows no dimension row or generic "Category" label when no meaningful Principal dimension exists (GAP-015); and that all Principal aggregations, relationships, snapshots, trends, and evidence resolve identity via `SupplierId` with `SupplierName` presentation only (GAP-016); and that business-facing surfaces use Principal while technical artifacts use Supplier per the governance rule (GAP-017); and that exactly one investigation entry exists — the Supplier entry replaced, not duplicated (BQ-005); and that no new Principal KPI IDs are introduced with existing KPI semantics unchanged (TQ-005); and that evidence ownership remains with source providers with no new stewardship model (OQ-001); and that every view displays Generated At, Reporting Period, and Data Snapshot Period (when applicable) with no real-time guarantee (OQ-002); and that a dedicated Data Health section shows Target Coverage %, Missing Target Count, and Unknown Principal count/amount with evidence navigation, visible in both lenses (OQ-003).

The architecture and data required to build this workspace already exist. The feasibility bottleneck is **decision readiness**, not technical readiness.

---

## 12. Resolution Log

| ID | Date | Decision |
| --- | --- | --- |
| GAP-001 | 2026-09-11 | Supplier remains the canonical domain entity, database identity, integration identity, and legacy application identity. Principal becomes a presentation alias used by Analytics, Dashboard, Navigation, KPI Registry, Documentation, and UX. No new Principal master entity will be introduced. Business rule: `SupplierId` remains the authoritative technical identifier; "Principal" is a business-facing label representing the same entity within Principal-centric analytics. Impact — Database: no change; API contracts: preferably no change; Legacy desktop: no change; Analytics/UI: display "Principal"; Navigation/Menu: display "Principal"; KPI Registry: use Principal terminology; Evidence lineage: still references `SupplierId`. |
| GAP-002 | 2026-09-11 | Principal Investigation Workspace supports two business lenses: (1) Sales-Out Lens, (2) Purchasing Lens. A lens is a presentation perspective, not a new entity type and not a separate workspace. Both lenses investigate the same Principal (`SupplierId`). Sales-Out Lens focuses on Sales-Out, Growth, Achievement, Returns, Coverage, Customer/Salesman/Item Contribution. Purchasing Lens focuses on Purchase Amount, Purchase Growth, Purchase Trend, Inventory Exposure, Stock Risk, Purchase vs Sales-Out Alignment. Single workspace with a lens switcher; no duplicate navigation, entity type, profile, or master data. |
| GAP-003 | 2026-09-11 | Principal Investigation Workspace defaults to a Sales-Out-oriented preset: `principal-sales-out-map`. Primary exploration dimensions: Principal Sales-Out, Growth, Achievement, Coverage, Returns. Purchasing remains available as an alternative lens through the Purchasing Lens selector; no purchasing capabilities are removed. Rationale: Principal-centric analytics is primarily driven by commercial performance (Sales-Out); Purchasing is a supporting investigation perspective. |
| GAP-004 | 2026-09-11 | Default Sales-Out Population Map for the Sales-Out Lens: X-Axis Achievement %, Y-Axis YoY Growth %, Bubble Size Principal Sales-Out, Bubble Color Return %. Purpose: quickly identify Star, Growing, Underperforming, Declining, and High-return-risk Principals. Additional presets may be introduced later without changing the default behavior. |
| GAP-005 | 2026-09-11 | Derived investigation KPI Purchase-to-Sales-Out Ratio = Purchase Amount ÷ Principal Sales-Out. Interpretation: >1.0 = Inventory Building / Potential Over-Buying; ≈1.0 = Balanced; <1.0 = Inventory Drawdown / Potential Under-Buying. Investigation-only metric; not a ranking, performance, or financial KPI. Purpose: assess purchasing alignment with commercial demand. |
| GAP-006 | 2026-09-11 | Purchasing Efficiency redefined as Inventory Replenishment Efficiency. No formal Purchase Order workflow exists, so efficiency is not measured with PO-based metrics. Purchasing Lens: Purchase Amount, Purchase Growth, Purchase-to-Sales-Out Ratio, Inventory Value, Inventory Days of Supply, Slow Moving Exposure. No PO Fulfillment, PO Backlog, or Outstanding PO metrics required. |
| GAP-007 | 2026-09-11 | `PRN-PUR-001` remains the authoritative current-period Principal Purchase metric. Historical purchase trend sourced from existing `PU-KPI-001` monthly history (Supplier and Principal share the same `SupplierId` identity). No separate Principal purchase-history projection at this stage; future migration to a dedicated Principal history series remains optional. |
| GAP-008 | 2026-09-11 | Purchasing Lens investigates through existing Purchase Invoice evidence. Relationship drivers: (1) Top Purchased Items from Purchase Invoice Detail, (2) Purchase History from Monthly Purchase Amount history. No additional purchasing relationship model; no PO-based drivers. Purpose: explain purchase volume and trends using existing purchasing evidence. |
| GAP-009 | 2026-09-11 | Principal Investigation Workspace supports both Sales-Out and Purchasing attention signals. Existing EX01/EX02 Sales-Out signals reused where applicable. Principal categories: Sales-Out Decline, Growth Deterioration, Target Miss, Return Risk, Coverage Deterioration. Purchase/inventory signals remain. No new attention engine. |
| GAP-010 | 2026-09-11 | Performance Signature (Radar) retired from Entity Analytics for all entity types (ADR-EA-001: `principal-investigation-workspace/adrs/ADR-EA-001-radar-retirement.md`). No Principal Performance Signature; no dual-lens signature; no replacement radar. Investigation relies on KPI Summary, Population Position, Trajectory, Business Drivers, Relationships, Attention Signals, Evidence. |
| GAP-011 | 2026-09-11 | Principal Investigation Workspace uses the existing all-active Principal population as its peer population. No Principal-specific Peer Group Selector. Rationale: small population, all-active comparison sufficient, existing peer-position capability works through `supplier-all-active`, extra dimensions low value vs cost. Peer Position available; selector hidden. |
| GAP-012 | 2026-09-11 | Supplier remains the canonical technical entity; Principal is the business-facing terminology for all Principal-centric analytics surfaces. Entity Analytics displays: Principal Investigation Workspace, Principal Profile, Compare Principals, Principal Relationships, Principal Evidence. Internal identifiers (`SupplierId`, `entityType=Supplier`, Supplier analytics registrations) unchanged. No database, API, snapshot, or domain migration required. |
| GAP-013 | 2026-09-11 | All Principal Investigation evidence is explicitly associated with the active lens. Sales-Out Lens: Faktur Item, Return Item, Customer Contribution, Salesman Contribution. Purchasing Lens: Purchase Invoice, Purchase History, Inventory Exposure, Inventory Movement. Evidence panels, links, titles, and disclosures clearly indicate the originating lens. No duplicate evidence model required. |
| GAP-015 | 2026-09-11 | Hide the dimension row from the tooltip when no meaningful Principal dimension exists. Do not display the generic "Category" label. |
| GAP-016 | 2026-09-11 | `SupplierId` is the authoritative Principal identity across all Principal Investigation capabilities. All Purchasing Lens aggregations, relationships, snapshots, trends, and evidence are sourced from `SupplierId`-keyed data. `SupplierName` presentation only; no name-based aggregation, matching, or joining. Historical data may display the recorded `SupplierName`; identity resolution uses `SupplierId`. |
| GAP-017 | 2026-09-11 | Terminology governance rule for Principal-centric Analytics. Business-facing surfaces use Principal: menus, navigation, workspace titles, profile pages, dashboard titles, KPI descriptions, user documentation, disclosures. Technical artifacts use Supplier: database schema, `SupplierId`, API internals, code identifiers, analytics registrations, legacy integrations. No technical identity migration required. |
| BQ-005 | 2026-09-11 | Replace the current Supplier Investigation entry. No parallel Supplier/Principal entries. |
| BQ-010 | 2026-09-11 — YES | Purchasing Lens shows Top Purchased Items and Purchase History. Excluded: PO-based relationships, purchasing-user relationships, procurement workflow relationships. Rationale: explain purchase volume, replenishment behavior, and inventory exposure from existing purchase evidence. |
| TQ-005 | 2026-09-11 — NO | No new Principal KPI IDs required. Purchase-to-Sales-Out Alignment is an investigation metric derived from existing Purchase Amount and Principal Sales-Out measures. No Purchasing Efficiency KPI introduced. Existing KPI semantics unchanged. |
| OQ-001 | 2026-09-11 — CLOSED | No new purchasing-lens KPI registry entries. Analytics use existing registered KPIs plus derived investigation metrics. Derived metrics require no KPI ownership, registry maintenance, ranking contracts, or dedicated evidence-route ownership. Evidence ownership remains with source providers and existing evidence contracts. No new KPI stewardship model required. |
| OQ-002 | 2026-09-11 | Standard Entity Analytics freshness policy: every view displays Generated At timestamp, Reporting Period, Data Snapshot Period (when applicable). No real-time data guarantee; all outputs interpreted relative to the displayed snapshot. Disclosure: "Data shown in this workspace is based on the latest available analytics snapshot and may not reflect transactions entered after the snapshot was generated." |
| OQ-003 | 2026-09-11 | Data-quality and completeness surfaced through a dedicated Data Health section visible regardless of lens: Target Coverage %, Principals Missing Target Count, Unknown Principal Exception Count, Unknown Principal Exception Amount, with navigation to supporting evidence for Unknown Principal exceptions. Rationale: both conditions affect interpretation and must be visible before conclusions are drawn. |
| OQ-004 | 2026-09-11 | Navigation and terminology consolidation owned by the BTR Portal Product Owner, responsible for defining and maintaining business-facing terminology standards. Implementation teams apply approved terminology across navigation, menus, workspace/menu/dashboard titles, labels, disclosures, and user-facing documentation. Technical identifiers may continue using Supplier terminology where required by code, database, integration, and legacy-system constraints. |

Note: GAP-001 above belongs to this assessment. It is distinct from GAP-001 of the principal-centric migration feasibility (`principal-centric-analytics-migration/FEASIBILITY-ASSESSMENT.md`), which is a separate, already-resolved decision.
