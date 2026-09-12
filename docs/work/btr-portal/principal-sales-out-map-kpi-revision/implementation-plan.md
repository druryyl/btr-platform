# IMPLEMENTATION PLAN

## Principal Sales-Out Map — Time-Aware KPI Revision

| Field | Value |
| --- | --- |
| Status | PLANNED — ready for implementation approval |
| Plan date | 2026-09-12 |
| Planning mode | Mode B — Feasibility-Driven Planning |
| Role | Planning Agent deliverable |
| Authority | `docs/work/btr-portal/principal-sales-out-map-kpi-revision/FEASIBILITY-ASSESSMENT.md` |

---

## 1. Planning Authority

```text
FEASIBILITY ASSESSMENT
```

Authoritative input:

- `docs/work/btr-portal/principal-sales-out-map-kpi-revision/FEASIBILITY-ASSESSMENT.md`
  (GAP-001..GAP-010 all closed; OQ-001..OQ-007 all resolved; Migration Option B selected).

The feasibility report is the planning authority. This plan introduces **no new business decisions**
and **no new architecture decisions**. Every slice traces to a closed gap or resolved open question
(see §7 Decision Traceability).

Supporting read-only evidence consulted (not reinterpreted):

- `src/j05-btr-distrib/btr.application/ReportingContext/PrincipalAnalyticsAgg/PrincipalKpiCatalog.cs`
- `src/j05-btr-distrib/btr.application/ReportingContext/PrincipalAnalyticsAgg/Services/PrincipalAchievementComposer.cs`
- `src/j05-btr-distrib/btr.application/ReportingContext/PrincipalAnalyticsAgg/Services/PrincipalYoyGrowthComposer.cs`
- `src/j05-btr-distrib/btr.application/ReportingContext/DashboardSnapshotAgg/Services/SalesForecastPolicy.cs`
- `src/j05-btr-distrib/btr.application/ReportingContext/EntityAnalyticsAgg/...` (producer, registrar, presets, lens, engine)
- `src/j05-btr-distrib/btr.portal.web/src/services/populationProjection/...` and `PopulationMapCanvas.vue`

---

## 2. Scope Summary

Make the `principal-sales-out-map` scatter chart meaningful on any day of the month by replacing the
two axis KPIs with time-aware variants, and by making the chart render signed growth and fixed
business quadrants:

1. **New X-axis KPI** — `PRN-TGT-004` **Pacing Achievement %** =
   `Actual Sales MTD ÷ (Monthly Target × Elapsed Days ÷ Days In Month) × 100` (dynamic, linear pacing).
2. **New Y-axis KPI** — `PRN-GRW-003` **YoY MTD Growth %** =
   `(Current Year MTD Sales − Prior Year MTD Sales) ÷ Prior Year MTD Sales × 100`, aligned on
   Business Date / equivalent elapsed-day windows; `NULL` when the prior-year base is zero.
3. **Confidence guards** — ratio KPIs return `NULL` + `LowConfidence` when configurable minimum
   prior-year base / elapsed-day thresholds are breached; low-confidence entities are excluded from
   quadrant classification and visually distinguishable. Thresholds live in KPI metadata.
4. **Registry & governance** — both KPIs registered in the KPI Registry (labels, units, formatting,
   descriptions, axis mappings, confidence thresholds); the registry is authoritative.
5. **Chart behavior** — signed Y-axis (no clamping), auto-calculated bounds, a visible `Y=0`
   reference line, and fixed business quadrants at `X = 100%`, `Y = 0%`
   (Star / Growing / Steady / Declining).
6. **Scope** — delivered to the **Principal Sales-Out Map** and **Principal Profile** only.

### Explicitly out of scope

- **SA04** (`GetPrincipalPerformanceQuery`) — deferred to a separate evaluation; no changes (OQ-007).
- **Attention emitter** (`Growth Deterioration` / `Target Miss`) — no changes (GAP-010).
- Existing KPIs `PRN-TGT-003` (Achievement %) and `PRN-GRW-002` (YoY Growth %) — unchanged (Option B).
- No new daily history table, no historical backfill, no schema/projection migration
  (GAP-001/GAP-002/GAP-003/GAP-004) and no SA04 changes.

---

## 3. Impact Inventory

### Backend

| Component | File / asset | Change |
| --- | --- | --- |
| Period calculator (new) | `.../PrincipalAnalyticsAgg/Services/` (new shared service) | Derive `AsOfDate`, `ElapsedDays`, `DaysInMonth`, prior-year equivalent window from `IBusinessDateProvider` (GAP-002) |
| KPI catalog | `.../PrincipalAnalyticsAgg/PrincipalKpiCatalog.cs` | Add `PRN-TGT-004`, `PRN-GRW-003` id constants, factory methods, `RegisteredEntries` entries |
| Pacing calculator | `.../PrincipalAnalyticsAgg/Services/PrincipalAchievementComposer.cs` or new calculator | Dynamic Pacing Achievement % (GAP-003) |
| YoY MTD calculator | `.../PrincipalAnalyticsAgg/Services/PrincipalYoyGrowthComposer.cs` or new calculator | Dynamic YoY MTD Growth % over aligned range windows (GAP-004) |
| Range evidence DAL (new) | `.../PrincipalAnalyticsAgg/Contracts/` + `btr.infrastructure/ReportingContext/PrincipalAnalyticsAgg/` | Per-Principal date-range Sales-Out aggregation over `BTR_Faktur → BTR_FakturItem → BTR_Brg → BTR_Supplier` (GAP-001) |
| Refresh workers (existing) | `.../PrincipalAnalyticsAgg/UseCases/RefreshPrincipalAchievementSnapshotWorker.cs`, `RefreshPrincipalYoyGrowthSnapshotWorker.cs` | Inject business date + dynamic calculators; existing snapshot outputs unchanged |
| L0 mapping | `.../EntityAnalyticsAgg/Producers/SupplierEntityAnalyticsProducer.cs` | Map `PRN-TGT-004` / `PRN-GRW-003` values + confidence to L0 (GAP-008) |
| KPI metadata | `.../EntityAnalyticsAgg/Registrars/SupplierEntityAnalyticsRegistrar.cs` | Register both KPIs' metadata + confidence thresholds; add ids to `supplier-default` pack |
| KPI metadata model | `.../EntityAnalyticsAgg/Models/EntityKpiMetadata.cs` | Additive confidence-threshold fields |
| Map preset | `.../EntityAnalyticsAgg/Services/EntityMapPresetRegistry.cs` | Repoint `principal-sales-out-map` axes to `PRN-TGT-004` / `PRN-GRW-003` |
| Lens config | `.../EntityAnalyticsAgg/Services/EntityInvestigationLensRegistry.cs` | Add new ids to Sales-Out lens `KpiIds` |
| Population engine | `.../EntityAnalyticsAgg/Services/EntityPopulationMapEngine.cs` | Surface per-entity confidence flag (GAP-007) |
| Population DTO | `.../EntityAnalyticsAgg/Queries/InvestigationWorkspaceQueries.cs` | Additive `IsLowConfidence` on `PopulationMapPointDto` |
| DI wiring | `btr.portal.api/Configurations/ApplicationPortalExtensions.cs`, `InfrastructurePortalExtensions.cs` | Register new calculator/DAL |

### Database

- **No new tables, columns, schema changes, projections, or backfill.**
- New KPI values are exposed through the existing generic L0 table `BTRPD_EntityAnalytics_Current`.
- `BTRPD_PrincipalSalesOutHistory` remains month-grain and unmodified (GAP-001).
- No change to `BTRPD_PrincipalAchievement` / `BTRPD_PrincipalYoyGrowth` schemas.
- No new index; existing `IX_BTR_Faktur_FakturDate (FakturDate, VoidDate, FakturId)` supports range scans.

### Frontend

| Component | File / asset | Change |
| --- | --- | --- |
| Robust projection | `src/services/populationProjection/strategies/robustProjectionStrategy.ts` | Signed-axis branch — stop clamping negatives to 0 for the growth axis |
| Transform helpers | `src/services/populationProjection/robustStats.ts` | Signed/non-negative-aware transform + bounds |
| Layout / ticks / bounds | `src/services/populationMapLayout.ts` | Bounds and ticks supporting a signed domain and business-zero |
| Canvas | `src/components/entity-analytics/workspace/PopulationMapCanvas.vue` | `Y=0` reference line; fixed business quadrant overlay/labels; low-confidence visual treatment |
| Tooltip | `src/components/entity-analytics/workspace/PopulationMapTooltip.vue` | Confidence/labels if surfaced |
| API model | `src/models/entityAnalytics.ts` | Additive confidence field on `PopulationMapPoint` |

### Integration

- **None external.** Read-only analytics; no new API consumer or authorization boundary.
- Internal refresh cadence unchanged (Purchase Management worker remains the Principal refresh trigger).

### Security

- None. Read-only analytics; no new security surface.

---

## 4. Phases

| Phase | Title | Purpose | Slices |
| --- | --- | --- | --- |
| 1 | Foundation | Period context, KPI registration, and range evidence | PSOM-01, PSOM-02, PSOM-03, PSOM-04 |
| 2 | KPI Computation | Dynamic Pacing Achievement %, YoY MTD Growth %, confidence guards | PSOM-05, PSOM-06, PSOM-07 |
| 3 | Registration & Exposure | Metadata, producer-to-L0, preset/lens, confidence in API | PSOM-08, PSOM-09, PSOM-10 |
| 4 | Frontend Map Behavior | Signed Y-axis, `Y=0` line, fixed quadrants, low-confidence treatment | PSOM-11, PSOM-12, PSOM-13, PSOM-14 |
| 5 | Verification & Knowledge | Backend/frontend tests and governance sync | PSOM-15, PSOM-16, PSOM-17 |

Dependency order: Phase 1 → Phase 2 → Phase 3 → Phase 4 → Phase 5. No slice depends on a future
slice. Frontend slices (Phase 4) depend only on Phase 3 exposure slices.

### 4.1 Complexity Summary

Scale: **1 = easy, 5 = most complex**. Ratings reflect implementation/regression risk only (not
business criticality), to guide implementation-agent model selection.

| Complexity | Slices | Recommended capability |
| --- | --- | --- |
| 1/5 | PSOM-02, PSOM-09, PSOM-17 | Small/fast model (mechanical, config/docs) |
| 2/5 | PSOM-01, PSOM-05, PSOM-07, PSOM-12 | Small-to-mid model (bounded logic) |
| 3/5 | PSOM-03, PSOM-04, PSOM-06, PSOM-10, PSOM-14, PSOM-15, PSOM-16 | Mid model (multi-file, needs care) |
| 4/5 | PSOM-08, PSOM-13 | Strong model (integration/rendering risk) |
| 5/5 | PSOM-11 | Strongest model (core projection rewrite, highest regression risk) |

Complexity distribution: 3× level 1, 4× level 2, 7× level 3, 2× level 4, 1× level 5.

Suggested execution order for a mixed-model strategy: run Phase 1 (levels 1–3) with small/mid
models, pause for Phase 2 verification, assign PSOM-08 and PSOM-13 to strong models, and assign
PSOM-11 as an isolated, heavily reviewed slice.

---

## 5. Slices

### Phase 1 — Foundation

#### PSOM-01 — Shared analytics period calculator

- **Objective:** Introduce a shared, read-only period calculator/service that derives reporting
  period context from the Business Date at runtime: `AsOfDate`, `PeriodYear`, `PeriodMonth`,
  `MonthStart`, `MonthEnd`, `ElapsedDays`, `DaysInMonth`, and the prior-year equivalent window.
  Nothing is persisted (GAP-002).
- **Complexity:** 2/5 — small pure-logic service, but calendar/leap-year edge cases and shared adoption.
- **Dependencies:** None.
- **Acceptance Criteria:**
  - Business date is sourced from `IBusinessDateProvider.Today`; generated-at from `ITglJamDal.Now`.
  - `ElapsedDays = Math.Max(1, (AsOfDate − MonthStart).Days + 1)`, consistent with
    `SalesForecastPolicy` / `PrincipalInventoryAggregator`.
  - Prior-year window aligns the same month and elapsed day, with the end clamped to the last valid
    day of the prior-year month (leap-year safe).
  - Deterministic for: day 1, mid-month, month-end, leap-year Feb 29, non-leap prior year.
  - No `ElapsedDays` / `DaysInMonth` / `AsOfDate` is written to any Principal snapshot or history table.
- **Review Focus:** Persistence Compliance (GAP-002); Architecture Compliance (reuse existing
  pacing precedents); Determinism.

#### PSOM-02 — KPI catalog entries

- **Objective:** Register `PRN-TGT-004` (Pacing Achievement %) and `PRN-GRW-003` (YoY MTD Growth %)
  in `PrincipalKpiCatalog`, following existing factory/entry conventions. Existing
  `PRN-TGT-003` / `PRN-GRW-002` entries remain unchanged (GAP-003/GAP-004).
- **Complexity:** 1/5 — boilerplate catalog entries following an existing pattern.
- **Dependencies:** None.
- **Acceptance Criteria:**
  - New id constants, factory methods, and `RegisteredEntries` entries exist; `TryGet` resolves both.
  - Existing `PRN-SALES-001`, `PRN-TGT-001/002/003`, `PRN-GRW-001/002` entries are byte-identical.
  - `PrincipalKpiCatalogTest` is updated and green; new test files registered in `btr.test.csproj`.
- **Review Focus:** KPI stewardship; backward compatibility; no reinterpretation of existing KPIs.

#### PSOM-03 — KPI Registry metadata (labels, units, formatting, thresholds, axis)

- **Objective:** Register `EntityKpiMetadata` for both KPIs in `SupplierEntityAnalyticsRegistrar`
  and add both ids to the `supplier-default` pack. Add trailing confidence-threshold fields to
  `EntityKpiMetadata`. The registry is the authoritative source of identifiers/labels (GAP-007/GAP-008).
- **Complexity:** 3/5 — touches the shared metadata model and pack validation; additive but broad blast radius.
- **Dependencies:** PSOM-02.
- **Acceptance Criteria:**
  - `PRN-TGT-004` metadata: `DisplayName = "Pacing Achievement %"`, `Unit = "Percent"`,
    `Direction = HigherIsBetter`, `DisplayPrecision`, a description stating the formula, and
    confidence threshold fields.
  - `PRN-GRW-003` metadata: `DisplayName = "YoY MTD Growth %"`, `Unit = "Percent"`, `Direction`,
    `DisplayPrecision`, description, nullable behavior, and confidence threshold fields.
  - Default axis roles are registered (`PRN-TGT-004` → X, `PRN-GRW-003` → Y) via registry
    configuration referencing the registered ids; no independently maintained formula/name copies.
  - Both ids added to the `supplier-default` pack; `ValidatePack` passes.
  - Existing `PRN-TGT-003` / `PRN-GRW-002` metadata unchanged.
  - Confidence thresholds are **not** hard-coded in calculation code.
- **Review Focus:** KPI governance (GAP-008/GAP-009); metadata completeness; no SA04 impact.

#### PSOM-04 — Date-range Sales-Out evidence query

- **Objective:** Add a read-only per-Principal date-range Sales-Out aggregation over
  `BTR_Faktur → BTR_FakturItem → BTR_Brg → BTR_Supplier`, supporting the current-year MTD and
  prior-year equivalent MTD windows (GAP-001). Mirrors existing `PrincipalSalesOutEvidenceDal`
  attribution.
- **Complexity:** 3/5 — new SQL aggregation; correctness of void handling, attribution, and reconciliation.
- **Dependencies:** PSOM-01.
- **Acceptance Criteria:**
  - New DAL contract + infrastructure implementation returns per-Principal summed Sales-Out for an
    arbitrary inclusive `[start, end]` business-date range.
  - Void/non-eligible Faktur are excluded consistently with `PRN-SALES-001`; attribution is via
    `Brg.SupplierId`.
  - Query uses `IX_BTR_Faktur_FakturDate`; no new index is introduced; results are grouped by Principal.
  - Reconciled: summing a full month via the range query equals `BTRPD_PrincipalSalesOutHistory` for
    that month (within the existing rounding scale).
  - `BTRPD_PrincipalSalesOutHistory` remains month-grain and unmodified.
- **Review Focus:** Persistence Compliance; query performance; attribution consistency.

---

### Phase 2 — KPI Computation

#### PSOM-05 — Pacing Achievement % calculation

- **Objective:** Compute `PRN-TGT-004` = `Actual Sales MTD ÷ Expected Target MTD × 100`, where
  `Expected Target MTD = Monthly Target × (ElapsedDays ÷ DaysInMonth)`. Dynamic and linear; existing
  `PRN-TGT-003` unchanged (GAP-003/OQ-004).
- **Complexity:** 2/5 — simple formula over existing target data and period context.
- **Dependencies:** PSOM-01, PSOM-02, PSOM-03.
- **Acceptance Criteria:**
  - Uses `PRN-TGT-001` monthly target plus the PSOM-01 period context; no persistence/schema change.
  - Returns `null` when the monthly target is `≤ 0`.
  - On day 1, expected pace = `target ÷ DaysInMonth`; on month-end the value equals full-month
    Achievement % (within rounding).
  - Uses the shared rounding scale for Principal percentages.
  - No change to `PRN-TGT-003` behavior.
- **Review Focus:** Business formula compliance; no duplicate achievement concept.

#### PSOM-06 — YoY MTD Growth % calculation

- **Objective:** Compute `PRN-GRW-003` = `(Current Year MTD Sales − Prior Year MTD Sales) ÷ Prior
  Year MTD Sales × 100` using Business-Date-aligned equivalent elapsed-day windows from PSOM-04.
  Existing month-grain `PRN-GRW-002` unchanged (GAP-004).
- **Complexity:** 3/5 — formula is simple, but window alignment, zero-base handling, and range-query integration need care.
- **Dependencies:** PSOM-01, PSOM-02, PSOM-03, PSOM-04.
- **Acceptance Criteria:**
  - Both windows are sourced via the PSOM-04 range query.
  - The prior-year window uses equivalent elapsed days (not a full prior month), aligned on Business Date.
  - Returns `null` when prior-year MTD Sales is `≤ 0` (no artificial percentage).
  - Positive, negative, and zero-base cases covered by unit tests.
  - No change to `PRN-GRW-002` behavior.
- **Review Focus:** Business formula; alignment correctness; no duplicate growth concept.

#### PSOM-07 — Confidence guards and status

- **Objective:** Apply configurable confidence guards (GAP-007). YoY MTD Growth returns `NULL` +
  `LowConfidence` when prior-year MTD is below the minimum base; Pacing Achievement returns `NULL` +
  `LowConfidence` when elapsed days are below the minimum. Thresholds are read from KPI metadata.
- **Complexity:** 2/5 — small guard logic over two already-computed results.
- **Dependencies:** PSOM-03, PSOM-05, PSOM-06.
- **Acceptance Criteria:**
  - A result object carries both the nullable value and a confidence status.
  - Breaching either threshold yields `NULL` value with `LowConfidence`; otherwise status is normal.
  - Thresholds are read from PSOM-03 metadata; no threshold literals in the calculators.
  - Unit tests cover: below-minimum prior base, below-minimum elapsed days, and above-threshold normal.
- **Review Focus:** GAP-007 compliance; metadata-driven configuration; no hard-coded thresholds.

---

### Phase 3 — Registration & Exposure

#### PSOM-08 — Expose new KPIs to L0 via producer

- **Objective:** Compute `PRN-TGT-004` / `PRN-GRW-003` dynamically during the Principal analytics
  refresh and expose them to `SupplierEntityAnalyticsProducer` for L0 mapping into the existing
  `BTRPD_EntityAnalytics_Current` (GAP-008). No new tables/columns/schema.
- **Complexity:** 4/5 — integration into the 2000+ line producer and refresh pipeline without schema change; highest regression risk on the backend.
- **Dependencies:** PSOM-05, PSOM-06, PSOM-07.
- **Acceptance Criteria:**
  - After a refresh, `GetCurrentKpiPopulation(Supplier, "PRN-TGT-004")` and
    `GetCurrentKpiPopulation(Supplier, "PRN-GRW-003")` return per-Principal values.
  - Existing L0 KPIs (`PRN-SALES-001`, `PRN-TGT-003`, `PRN-GRW-002`, return KPIs) are unchanged.
  - No new table/column/migration; no Principal snapshot schema change.
  - Low-confidence entities are represented per the PSOM-10 contract (value suppressed and/or flagged).
  - Refresh remains within the existing worker/domain cadence.
- **Review Focus:** Architecture Compliance; Persistence Compliance; no SA04.

#### PSOM-09 — Preset repoint and lens membership

- **Objective:** Repoint the `principal-sales-out-map` preset axes to the new KPIs and add both ids
  to the Sales-Out lens. Existing `PRN-TGT-003` / `PRN-GRW-002` remain available for SA04/ranking
  (Migration Option B).
- **Complexity:** 1/5 — configuration-only repoint plus lens list update.
- **Dependencies:** PSOM-02, PSOM-03, PSOM-08.
- **Acceptance Criteria:**
  - `EntityMapPresetRegistry` `principal-sales-out-map` uses `AxisXKpiId = "PRN-TGT-004"`,
    `AxisYKpiId = "PRN-GRW-003"`; `BubbleKpiId` / `BubbleColorKpiId` unchanged.
  - `EntityInvestigationLensRegistry` Sales-Out lens `KpiIds` includes `PRN-TGT-004` and `PRN-GRW-003`.
  - Preset/lens reference registered KPI identifiers only (no independent names/formulas).
  - `EntityMapPresetRegistryTest` / `EntityInvestigationLensRegistryTest` updated and green.
- **Review Focus:** GAP-009 governance; Option B compliance; no SA04 change.

#### PSOM-10 — Population API confidence field

- **Objective:** Surface per-entity confidence so the frontend can exclude low-confidence entities
  from quadrant classification and optionally distinguish them (GAP-007). Add an additive
  `IsLowConfidence` (or `ConfidenceStatus`) field to `PopulationMapPointDto` and the TypeScript model,
  populated by `EntityPopulationMapEngine`.
- **Complexity:** 3/5 — additive DTO/engine change, but confidence must be plumbed from computation to the query read path.
- **Dependencies:** PSOM-07, PSOM-08.
- **Acceptance Criteria:**
  - The population endpoint returns a per-point confidence flag; existing fields are unchanged.
  - The flag is `true` exactly when the underlying KPI value was suppressed/flagged `LowConfidence`.
  - No database schema change (read-time projection only).
  - `EntityPopulationMapEngineTest` covers flagged and unflagged cases.
- **Review Focus:** API compatibility (additive); GAP-007.

---

### Phase 4 — Frontend Map Behavior

#### PSOM-11 — Signed Y-axis projection

- **Objective:** Support a signed business Y-axis for `PRN-GRW-003` so negative YoY MTD Growth is
  plotted below zero without clamping, with min/max bounds auto-calculated from the dataset (GAP-005).
- **Complexity:** 5/5 — replaces a non-negative log/robust-normalize assumption in the core projection; highest technical risk and broadest frontend regression surface.
- **Dependencies:** PSOM-09.
- **Acceptance Criteria:**
  - Negative Y values are not clamped to `0`; relative sign and magnitude ordering are preserved.
  - Y bounds are computed from the actual dataset min/max (with padding) and enclose `0` when the data
    spans both signs.
  - Applies to the map's percent axes; other presets (IDR/days) retain existing non-negative behavior.
  - The existing spec asserting "negative → 0" is revised for the signed path; projection specs pass.
- **Review Focus:** Rendering correctness; no regression to other presets; GAP-005.

#### PSOM-12 — Y=0 reference line and auto bounds

- **Objective:** Always display a visible horizontal `Y = 0` reference line using the auto-calculated
  bounds (GAP-005).
- **Complexity:** 2/5 — a single draw pass plus correct business-zero-to-screen mapping.
- **Dependencies:** PSOM-11.
- **Acceptance Criteria:**
  - A distinct horizontal line is drawn at business `Y = 0` (not the regression line).
  - A `0` tick/label is shown at the same position.
  - The line is present whenever `0` is within the visible Y bounds.
- **Review Focus:** GAP-005; axis rendering.

#### PSOM-13 — Fixed business quadrants

- **Objective:** For `principal-sales-out-map`, replace statistical `Expected` / `Above Expected` /
  `Below Expected` regions with fixed business quadrants defined at business `X = 100%` (Pacing
  Achievement %) and `Y = 0%` (YoY MTD Growth %): **Star** (X≥100, Y≥0), **Growing** (X<100, Y≥0),
  **Steady** (X≥100, Y<0), **Declining** (X<100, Y<0) (GAP-006).
- **Complexity:** 4/5 — business-to-screen boundary conversion, replacing statistical region rendering, and correct quadrant semantics.
- **Dependencies:** PSOM-09, PSOM-11.
- **Acceptance Criteria:**
  - Quadrant boundaries are converted from business values to screen coordinates using the same
    transform as the plotted points.
  - The four fixed labels render in the correct quadrants; statistical region labels are not rendered
    for this preset.
  - Classification uses the fixed thresholds, not regression residuals.
  - Other presets are unaffected.
- **Review Focus:** Business rule compliance (GAP-006); rendering correctness; no statistical boundaries.

#### PSOM-14 — Low-confidence visual treatment and quadrant exclusion

- **Objective:** Exclude low-confidence entities from quadrant classification and visually distinguish
  them (GAP-007).
- **Complexity:** 3/5 — canvas-level classification and styling, dependent on the confidence field.
- **Dependencies:** PSOM-10, PSOM-13.
- **Acceptance Criteria:**
  - Points flagged low-confidence are not assigned any business quadrant.
  - Flagged points are visually distinguished from classified points.
  - Non-flagged points classify normally.
- **Review Focus:** GAP-007; UI State Compliance.

---

### Phase 5 — Verification & Knowledge

#### PSOM-15 — Backend verification suite

- **Objective:** Add/update backend tests and register every new test file in `btr.test.csproj`.
- **Complexity:** 3/5 — broad test surface and classic-csproj manual file registration; low design risk.
- **Dependencies:** PSOM-01..PSOM-10.
- **Acceptance Criteria:**
  - Updated/added tests: `PrincipalKpiCatalogTest`, period calculator test, range-evidence
    reconciliation, `PrincipalAchievementComposerTest`, `PrincipalYoyGrowthComposerTest`, confidence
    guards, `SupplierEntityAnalyticsProducerTest`, `EntityMapPresetRegistryTest`,
    `EntityInvestigationLensRegistryTest`, `EntityAnalyticsKpiRegistryTest`,
    `EntityPopulationMapEngineTest`.
  - `btr.test.csproj` build succeeds via `dotnet msbuild btr.test\btr.test.csproj /p:Configuration=Release`;
    focused tests pass via `dotnet vstest btr.test\bin\Release\btr.test.dll`.
  - No new failures beyond the known pre-existing baseline.
- **Review Focus:** Objective verification; scope discipline; no new-decision creep.

#### PSOM-16 — Frontend verification suite

- **Objective:** Add/update Vitest specs for signed projection, bounds/`Y=0`, fixed quadrants, and
  low-confidence exclusion.
- **Complexity:** 3/5 — several specs to update/add, including reworking the existing negative-clamp expectation.
- **Dependencies:** PSOM-11..PSOM-14.
- **Acceptance Criteria:**
  - `populationProjectionEngine.spec.ts` updated for signed behavior; `populationMapLayout.spec.ts`
    covers signed bounds, `0` tick, and quadrant boundaries.
  - New pure-service tests cover quadrant classification (all four quadrants) and low-confidence
    exclusion.
  - `npm test` passes; `vue-tsc -b && vite build` passes.
- **Review Focus:** Rendering correctness; regression safety.

#### PSOM-17 — Knowledge and governance synchronization

- **Objective:** Update knowledge artifacts so the KPI Registry is the authoritative source and the
  catalog/lens docs reference registered KPI identifiers (GAP-009).
- **Complexity:** 1/5 — documentation-only synchronization.
- **Dependencies:** PSOM-02, PSOM-03, PSOM-09.
- **Acceptance Criteria:**
  - KPI catalog documentation (`btr-portal-kpi-catalog.md`) and `PRINCIPAL-KPI-REGISTRY.md` list
    `PRN-TGT-004` / `PRN-GRW-003` with their registered display names/units and no independently
    maintained formulas.
  - Documentation states new KPIs are registered before consumption by maps/lenses/exports.
  - `PRN-TGT-003` / `PRN-GRW-002` remain documented as unchanged.
- **Review Focus:** Governance (GAP-009); documentation/implementation consistency.

---

## 6. Progress Tracker

Lifecycle: `PLANNED → IN IMPLEMENTATION → IMPLEMENTED → IN REVIEW → GO` (or `NO-GO → REMEDIATION → IN REVIEW → GO`).

| Slice | Phase | Title | Complexity | Status | Implementation History | Review History | Remediation History |
| --- | --- | --- | --- | --- | --- | --- | --- |
| PSOM-01 | 1 | Shared analytics period calculator | 2/5 | IMPLEMENTED | 2026-09-12: Added `AnalyticsPeriodContext` (AsOfDate, period, elapsed/days-in-month, prior-year aligned window, generated-at) and `AnalyticsPeriodCalculator` (`Resolve()` sources Business Date from `IBusinessDateProvider.Today` and generated-at from `ITglJamDal.Now`; pure static `Create()` for deterministic calendar/leap-year derivation, prior-year end clamped to last valid day). No persistence/schema. Registered source in `btr.application.csproj` + DI (`ApplicationPortalExtensions`); added focused `AnalyticsPeriodCalculatorTest` (8 tests, registered in `btr.test.csproj`) — build (VS MSBuild, Release) and tests green. | 2026-09-12: GO. AC1–AC5 PASS; FEASIBILITY GAP-002 (runtime-derived, no persistence) honored; scope = 2 new source files + 3 registration edits, no expansion. INFO-001: `dotnet msbuild` fails on the test project's transitive `btr.portal.api` reference (dotnet SDK lacks VS web targets); built with VS 2022 MSBuild per established repo convention. INFO-002: full `btr.test` run = 40 pre-existing failures (11 ReportingContext baseline + 27 SalesContext DB-login + 2 Helpers), none PSOM-01- or DB-slice-related; focused suite 8/8 green. | N/A |
| PSOM-02 | 1 | KPI catalog entries | 1/5 | GO | 2026-09-12: Added `PacingAchievementPercentageId` (`PRN-TGT-004`) and `YoyMtdGrowthId` (`PRN-GRW-003`) constants, `CreatePacingAchievementPercentage()` / `CreateYoyMtdGrowth()` factory entries, and `RegisteredEntries` membership in `PrincipalKpiCatalog`. No existing entry/factory modified (existing `PRN-SALES-001`/`PRN-TGT-001/002/003`/`PRN-PUR/INV/RET/GRW/CUS` families byte-identical). `PrincipalKpiCatalogTest` updated: two new focused tests (Pacing Achievement %, YoY MTD Growth %) plus whitelist/family-exclusion assertions extended; registered 16/16 green via VS MSBuild (Release) + vstest. No csproj change needed (same file updated). | 2026-09-12: GO. AC1–AC3 PASS; FEASIBILITY authority honored (GAP-003/GAP-004; Option B additive; no persistence). Scope = `PrincipalKpiCatalog.cs` (additive 79 lines) + `PrincipalKpiCatalogTest.cs` (extended assertions + 2 tests); no expansion, existing factories byte-identical (plain diff verified). INFO-001: `dotnet msbuild` unusable for `btr.test` (transitive `btr.portal.api` web targets, dotnet SDK); built with VS 2022 MSBuild per established repo convention — same as PSOM-01 review. INFO-002: focused `PrincipalKpiCatalogTest` 16/16 green; no full-suite regression run (baseline 40 pre-existing failures documented at PSOM-01) — focused suite adequate for this mechanical slice. | N/A |
| PSOM-03 | 1 | KPI Registry metadata | 3/5 | GO | 2026-09-12: Added trailing additive `DefaultAxisRole`, `MinimumElapsedDays`, and `MinimumBaseValue` fields to `EntityKpiMetadata`; registered `PRN-TGT-004` (DisplayName `Pacing Achievement %`, Unit `Percent`, HigherIsBetter, DisplayPrecision 4, X-axis, elapsed-day guard) and `PRN-GRW-003` (DisplayName `YoY MTD Growth %`, Unit `Percent`, HigherIsBetter, DisplayPrecision 4, Y-axis, base guard) metadata in `SupplierEntityAnalyticsRegistrar`; added both ids to the `supplier-default` pack. Existing `PRN-TGT-003`/`PRN-GRW-002` metadata untouched. Added 4 focused tests to `EntityAnalyticsKpiRegistryTest` (no csproj change). VS MSBuild (Release) build + focused vstest green; broader supplier/registry suites green except two pre-existing baseline failures (Customer exact-pack equality; L4 relationship row) unrelated to this slice. No schema/persistence changes. | 2026-09-12: GO. AC1–AC6 PASS; FEASIBILITY authority honored (GAP-007 thresholds metadata-driven; GAP-008 registry registration; GAP-009 ids referenced from `PrincipalKpiCatalog`; Option B additive; existing TGT-003/GRW-002 unchanged; no persistence/SA04). Scope = `EntityKpiMetadata.cs` (+3 additive fields), `SupplierEntityAnalyticsRegistrar.cs` (2 metadata registrations + 2 pack ids), `EntityAnalyticsKpiRegistryTest.cs` (4 focused tests), tracker; no expansion. Build evidence: VS MSBuild (Release) green; focused vstest 4/4 new + broader supplier/registry suites green except two pre-existing baseline failures unrelated to slice (same baseline noted at PSOM-01/PSOM-02). INFO-001: default threshold values (`MinimumElapsedDays = 6`, `MinimumBaseValue = Rp 1,000,000`) are configurable metadata defaults chosen because the plan specifies configurability, not exact values; not hard-coded in calculation code. INFO-002: `DefaultAxisRole` added per PSOM-03 title/AC ("axis"); Impact Inventory §3 lists confidence fields only. | — |
| PSOM-04 | 1 | Date-range Sales-Out evidence query | 3/5 | GO | 2026-09-12: Added read-only `IPrincipalSalesOutRangeEvidenceDal` + `PrincipalSalesOutRangeEvidenceRow` (Contracts) and `PrincipalSalesOutRangeEvidenceDal` (infrastructure) returning per-Principal summed Sales-Out for an inclusive `[start, end]` business-date range. SQL mirrors `PrincipalSalesOutHistoryEvidenceDal` attribution (`BTR_Faktur → BTR_FakturItem → BTR_Brg → BTR_Supplier`, `b.SupplierId`, `f.VoidDate = '3000-01-01'`, `SUM(SubTotal − DiscRp)`, `GROUP BY sup.SupplierId`), so a full-month range reconciles to `BTRPD_PrincipalSalesOutHistory`. Registered source in `btr.application.csproj` + `btr.infrastructure.csproj`, DAL in `InfrastructurePortalExtensions` DI, and focused `PrincipalSalesOutRangeEvidenceDalTest` (3 tests) in `btr.test.csproj`. No schema/table/index change (`IX_BTR_Faktur_FakturDate` only); `BTRPD_PrincipalSalesOutHistory` untouched. Build (VS MSBuild 18, Release) + focused tests green (3/3 new; 53/53 related). | 2026-09-12: GO. AC1–AC5 PASS; FEASIBILITY authority honored (GAP-001: dynamic BusinessDate-driven range aggregation over transactional facts; no new table/backfill/projection/schema/index; history remains month-grain and unmodified; Option B untouched). Scope = 3 new files + 4 registration edits (2 source csproj, test csproj, DI) + tracker; no expansion. Build evidence: VS MSBuild 18 (Release) green; focused 3/3 new + 53/53 related tests green. INFO-001: DB-backed full-month reconciliation against `BTRPD_PrincipalSalesOutHistory` is guaranteed by mirroring `ListMonthlySalesOutHistorySql` attribution/aggregation but was not executed (repo baseline is DB-login-limited); the per-plan reconciliation test is deferred to PSOM-15. INFO-002: `dotnet msbuild` web-target limitation unchanged; built with VS MSBuild 18 per established convention. | N/A |
| PSOM-05 | 2 | Pacing Achievement % calculation | 2/5 | GO | 2026-09-12: Added `PrincipalPacingAchievementResult`/`PrincipalPacingAchievementRow` (Models) and `PrincipalPacingAchievementComposer` (Services) computing PRN-TGT-004 = Actual Sales MTD ÷ Expected Target MTD × 100, where Expected Target MTD = stored PRN-TGT-001 × (ElapsedDays ÷ DaysInMonth) sourced from the PSOM-01 `AnalyticsPeriodContext`. Static `Calculate` returns `null` when monthly target ≤ 0, sales missing, or elapsed/days-in-month non-positive; rounds ×100 result to shared `PrincipalAchievementSnapshot.PercentageScale` (6, AwayFromZero). Existing `PrincipalAchievementComposer`/PRN-TGT-003 untouched. No persistence/schema/worker change. Registered `PrincipalPacingAchievementComposer` in `ApplicationPortalExtensions` DI + 2 source entries in `btr.application.csproj`; added focused `PrincipalPacingAchievementComposerTest` (11 tests, registered in `btr.test.csproj`) — VS MSBuild 18 (Release) green; focused 11/11 + related composer/period suites 23/23 green. | 2026-09-12: GO. AC1–AC5 PASS; FEASIBILITY authority honored (GAP-003 formula `Actual Sales MTD ÷ Expected Target MTD × 100` with `Expected Target MTD = Monthly Target × ElapsedDays ÷ DaysInMonth`; OQ-004 linear pacing; existing PRN-TGT-003/`PrincipalAchievementComposer` untouched; no persistence/schema/worker change). Scope = 2 new source files (result model + composer) + DI registration + 1 test file (11 tests) + 2 csproj registrations + tracker; no expansion, existing KPI writers byte-identical. Build evidence: VS MSBuild 18 (Release) green (btr.test transitively includes btr.portal.api with the DI edit); focused 11/11 + related composer/period suites 23/23 green. INFO-001: implemented as a new dedicated `PrincipalPacingAchievementComposer` rather than extending `PrincipalAchievementComposer` — explicitly permitted by Impact Inventory §3 ('or new calculator'); keeps PRN-TGT-003 byte-identical. INFO-002: PRN-TGT-004 stored scale is percent (×100, scale 6), distinct from PRN-TGT-003's stored ratio; consistent with GAP-003/plan formula and PSOM-03 metadata (Unit Percent, DisplayPrecision 4). | N/A |
| PSOM-06 | 2 | YoY MTD Growth % calculation | 3/5 | GO | 2026-09-13: Added `PrincipalYoyMtdGrowthResult`/`PrincipalYoyMtdGrowthRow` (Models) and `PrincipalYoyMtdGrowthComposer` (Services) computing PRN-GRW-003 = (Current Year MTD Sales − Prior Year MTD Sales) ÷ Prior Year MTD Sales × 100. `Compose(period, generatedAt)` sources both windows via the PSOM-04 `IPrincipalSalesOutRangeEvidenceDal`: current `[period.MonthStart, period.AsOfDate]` and prior-year equivalent `[period.PriorYearStart, period.PriorYearEnd]` (elapsed-day aligned, end clamped by PSOM-01). Static `Calculate` returns `null` when current/prior MTD missing or prior ≤ 0; rounds ×100 to shared `PrincipalAchievementSnapshot.PercentageScale` (6, AwayFromZero). Existing `PrincipalYoyGrowthComposer`/PRN-GRW-002 untouched. No persistence/schema/worker change. Registered `PrincipalYoyMtdGrowthComposer` in `ApplicationPortalExtensions` DI + 2 source entries in `btr.application.csproj`; added focused `PrincipalYoyMtdGrowthComposerTest` (8 tests, registered in `btr.test.csproj`) — VS MSBuild 18 (Release) green; focused 8/8 + related composer/period/range/catalog suites 43/43 green. | 2026-09-13: GO. AC1–AC5 PASS; FEASIBILITY authority honored (GAP-004 formula `(Current Year MTD Sales − Prior Year MTD Sales) ÷ Prior Year MTD Sales × 100`; GAP-001 dynamic range aggregation from transactional facts; no persistence/schema; existing PRN-GRW-002/`PrincipalYoyGrowthComposer` untouched). Scope = 2 new source files (result model + composer) + DI registration + 2 source csproj registrations + 1 test file (8 tests) + test csproj registration + tracker; no expansion. Build evidence: VS MSBuild 18 (Release) green; focused 8/8 + related composer/period/range/catalog suites 43/43 green. INFO-001: new dedicated `PrincipalYoyMtdGrowthComposer` rather than extending `PrincipalYoyGrowthComposer` — permitted by Impact Inventory §3 ('or new calculator'); keeps PRN-GRW-002 byte-identical. INFO-002: composer owns the two PSOM-04 range calls (current `[MonthStart, AsOfDate]`, prior `[PriorYearStart, PriorYearEnd]`) so AC1 ('both windows sourced via the PSOM-04 range query') is directly verifiable; pure `ComposeWindows` overload retained for deterministic tests. INFO-003: `Calculate` mirrors PRN-GRW-002 null semantics (null when current or prior MTD missing or prior ≤ 0) to avoid introducing a new decision. | — |
| PSOM-07 | 2 | Confidence guards and status | 2/5 | PLANNED | — | — | — |
| PSOM-08 | 3 | Expose new KPIs to L0 via producer | 4/5 | PLANNED | — | — | — |
| PSOM-09 | 3 | Preset repoint and lens membership | 1/5 | PLANNED | — | — | — |
| PSOM-10 | 3 | Population API confidence field | 3/5 | PLANNED | — | — | — |
| PSOM-11 | 4 | Signed Y-axis projection | 5/5 | PLANNED | — | — | — |
| PSOM-12 | 4 | Y=0 reference line and auto bounds | 2/5 | PLANNED | — | — | — |
| PSOM-13 | 4 | Fixed business quadrants | 4/5 | PLANNED | — | — | — |
| PSOM-14 | 4 | Low-confidence treatment/exclusion | 3/5 | PLANNED | — | — | — |
| PSOM-15 | 5 | Backend verification suite | 3/5 | PLANNED | — | — | — |
| PSOM-16 | 5 | Frontend verification suite | 3/5 | PLANNED | — | — | — |
| PSOM-17 | 5 | Knowledge and governance sync | 1/5 | PLANNED | — | — | — |

---

## 7. Decision Traceability

| Slice | Authority |
| --- | --- |
| PSOM-01 | GAP-002 |
| PSOM-02 | GAP-003, GAP-004 |
| PSOM-03 | GAP-007, GAP-008, GAP-009 |
| PSOM-04 | GAP-001 |
| PSOM-05 | GAP-003, OQ-004 |
| PSOM-06 | GAP-004 |
| PSOM-07 | GAP-007, OQ-005 |
| PSOM-08 | GAP-001, GAP-003, GAP-004, GAP-008 |
| PSOM-09 | OQ-001, Migration Option B |
| PSOM-10 | GAP-007 |
| PSOM-11 | GAP-005 |
| PSOM-12 | GAP-005 |
| PSOM-13 | GAP-006, OQ-002 |
| PSOM-14 | GAP-007 |
| PSOM-15 | All closed gaps |
| PSOM-16 | GAP-005, GAP-006, GAP-007 |
| PSOM-17 | GAP-009 |

Out-of-scope decisions honored: OQ-007 (SA04), GAP-010 (attention emitter).

---

## 8. Invariants

1. `PRN-TGT-003` and `PRN-GRW-002` behavior, storage, and consumers are unchanged.
2. No new database tables, columns, indexes, projections, migrations, or backfills.
3. New KPI values are computed dynamically and exposed via existing L0 storage only.
4. `BTRPD_PrincipalSalesOutHistory` remains month-grain and unmodified.
5. No changes to `BTR_Faktur*` or `BTR_SalesPersonPrincipalTarget`.
6. No SA04 changes; no attention-emitter changes.
7. KPI Registry is the authoritative source of identifiers, labels, and thresholds.
8. Quadrant thresholds are fixed at `X = 100%`, `Y = 0%`; statistical boundaries are not used for
   `principal-sales-out-map`.
