# IMPLEMENTATION PLAN — Sales Force Overview Visualization Redesign, Phase 1 (Consolidate)

- **Planning Mode:** FEASIBILITY-DRIVEN PLANNING (Mode B)
- **Status:** PLANNED — awaiting implementation
- **Authoritative inputs:**
  - `docs/work/btr-portal/sales-force-overview-dashboard-redesign/sfo-dashboard-visualization-review-v1.0_feasibility.md`
    (planning authority)
  - `docs/work/btr-portal/sales-force-overview-dashboard-redesign/sfo-dashboard-visualization-review-v1.0.md`
    (approved visualization design)
- **Supporting references:**
  - `docs/features/btr-portal/dashboard-11-sf02-sales-force-overview.md`
  - `docs/work/btr-portal/sales-force-overview-dashboard-redesign/sfo-dashboard-ux-blueprint-v2.2.md`
  - Implemented UI: `src/j05-btr-distrib/btr.portal.web/src/views/dashboard/FieldActivityOverviewView.vue`,
    `src/j05-btr-distrib/btr.portal.web/src/components/field-activity/*`
- **Scope constraint:** Frontend-only. No backend, database, API, KPI, ETL, or
  integration changes (feasibility §4, §10).
- **Phase 2 (excluded, not planned here):** Quadrant view; Action Center
  per-item reason lines and recognition refinement (review §5.2; feasibility §7).

---

## Planning Authority

```text
FEASIBILITY ASSESSMENT
```

The feasibility assessment is authoritative. It approves the visualization
review v1.0 and fixes Phase 1 as six approved items (feasibility §7, mirroring
review §5.2):

1. Build Execution Funnel (5-stage conversion strip)
2. Redesign Salesman Scoreboard (column groups, data bars, Attention column,
   Rank-by selector, sticky header)
3. Remove the three ranking charts (Visit Execution %, Effective Call Rate,
   Orders Generated)
4. Replace the Order Value chart with the Revenue Concentration strip
5. Move Action Center above Outcomes
6. Deprecate `FieldActivityComparisonChart.vue`

This plan introduces **no new business decisions** and **no new architecture
decisions**. All formulas, thresholds, rankings, signals, column groups, and
section order are taken verbatim from the two authority documents and the
existing dashboard services. Where the authorities required an implementation
interpretation to remain executable, it is recorded in
“Authority Alignment Notes” below and is mechanical, not a business rule.

---

## Scope Summary

Phase 1 consolidates four redundant salesman ranking charts into:

- **one narrative surface** — the 5-stage Execution Funnel (team level,
  Plan → Actual → Effective → Orders → Order Value, with conversion rates and
  absolute leaks), and
- **one comparison surface** — the Salesman Scoreboard (promoted existing
  table, funnel-ordered column groups, in-cell data bars, Attention signals,
  bounded “Rank by” ranking, sticky header), and
- **one concentration surface** — the Revenue Concentration strip replacing the
  Order Value chart (the only chart whose question was not answered elsewhere).

Plus the approved companion work: removal of the three redundant ranking
charts, replacement of the Order Value chart, moving the Action Center above
Outcomes, removal of the orphaned chart component, and the mandatory knowledge
artifact updates (review §5.4).

**Expected outcome (review §4.2, feasibility §2):**

| | Current | Phase 1 |
| --- | --- | --- |
| Salesman comparison content | ~2,200–2,500 px | ~570 px (funnel + scoreboard) |
| Independent scroll containers | 4–5 | 1 (scoreboard body only) |
| Places to evaluate one salesman | 4 charts | 1 row |
| Narrative anchor | none | Execution Funnel |
| Presentation-mode safe | no | yes |

**Explicitly out of scope:** Quadrant view; Action Center cap/reason/urgency
changes; any new KPI, threshold, composite score, API, database, or ETL change;
`Rankings` payload changes.

---

## Authority Alignment Notes (implementation interpretations, no new rules)

These are the only points where the authorities did not prescribe a mechanical
detail. Each is anchored to existing artifacts and introduces no threshold or
business rule. The Review Agent should evaluate each directly.

1. **Funnel leaks.** Only two absolute leaks are rendered: Missed Visits
   (`TeamKpis.MissedVisits`, Planned → Actual) and No-Order Visits
   (`ActualVisits − EffectiveCalls`, Actual → Effective; blueprint v2.2 §B.2,
   review §2.1). Review §2.1’s “Calls without order” on the Orders stage names
   the same `ActualVisits − EffectiveCalls` quantity as “No-Order Visits” and is
   therefore rendered once, at the Effective Calls stage; no third absolute leak
   is derivable from `TeamKpis`.
2. **Attention ordering.** “Attention (worst first)” (review §2.2) is implemented
   as a deterministic, rule-based ordering over the existing
   `commercialSignalRules.ts` output: action-signal rows
   (`NeedsCollectionAction`, `NeedsCreditReview`) first, ordered by joined
   `OverdueExposure` descending; remaining rows by Order Value descending;
   final tie-break `SalesPersonCode` ascending. No composite score, no new
   percentile threshold.
3. **Rank-by replaces free-form column sorting.** Review §2.2 limits ranking to
   five options and forbids free-form configuration (“not free-form”); the
   Rank-by selector is therefore the single ranking control, and the `#` column
   is the position in the active ranking (not the backend
   `FieldActivitySalesmanOverviewRow.Rank`, which is visit-execution rank).
4. **Retained columns.** The review adds/regroups columns and does not authorize
   removing existing evidence. Missed, Unplanned, GPS Valid %, and Status are
   retained and mapped into the approved groups (Missed/Unplanned → ACTIVITY;
   GPS Valid %/Status → SIGNAL) with existing formatting and band coloring.
5. **Calculation helpers.** Funnel, scoreboard-ordering, and concentration
   calculations are implemented as pure helpers under `src/services/` with
   vitest specs, following the established pattern
   (`fieldActivityCollectionComposition.ts`, `commercialSignalRules.ts`). This is
   the repository’s existing test convention (no component test infrastructure);
   it is not a new architecture decision.
6. **Concentration display values.** The strip renders Top-1 and Top-3 shares of
   total order value plus a single 100% stacked bar (Top 3 named + Others),
   exactly as shown in review §4 (“Top 3 = 66% of order value”, “single 100%
   stacked bar”); Top-1 is the key-person indicator of blueprint v2.2 GAP-025.

---

## Impact Inventory

### Backend

- **None.** No aggregate, entity, repository, command, query, service, or
  policy change. No new endpoint. All values are already returned by
  `getFieldActivityOverview(visitDate)` (feasibility §3, §4, §10).

### Database

- **None.** No table, view, index, constraint, or migration change
  (feasibility §4, §10).

### Frontend

**Modified:**

| File | Change |
| --- | --- |
| `src/views/dashboard/FieldActivityOverviewView.vue` | Recomposition: funnel mounted in B; scoreboard promoted from C to B and wired with collection; Order Value chart replaced by concentration strip; three ranking charts removed; Action Center moved above Outcomes; dead imports/computeds removed |
| `src/components/field-activity/FieldActivitySalesmanTable.vue` | Redesigned into the Salesman Scoreboard (title, column groups, retained columns, sticky header + internal scroll, Rank-by selector, in-cell data bars, Attention column, `collection` prop) |

**New components** (`src/components/field-activity/`):

| File | Purpose |
| --- | --- |
| `ExecutionFunnel.vue` | 5-stage horizontal conversion strip (plain markup + CSS) |
| `RevenueConcentrationStrip.vue` | Compact 100% stacked concentration strip |

**New pure calculation helpers** (`src/services/`, following existing pattern):

| File | Purpose |
| --- | --- |
| `fieldActivityFunnel.ts` (+ `.spec.ts`) | Funnel stage values, conversions, leaks from `FieldActivityTeamKpis` |
| `fieldActivityScoreboard.ts` (+ `.spec.ts`) | Rank modes, display rank, attention keys, column bar maxima |
| `fieldActivityConcentration.ts` (+ `.spec.ts`) | Top-1 / Top-3 concentration and stacked-bar segments from `Salesmen[].OmzetAmount` |

**Removed:**

| File | Reason |
| --- | --- |
| `src/components/field-activity/FieldActivityComparisonChart.vue` | No remaining caller after chart removals (review §1.4, §5.1 row 5, §5.2 item 6) |

**Reused unchanged** (per feasibility §10 “Services (reuse, no changes)”):

- `services/fieldActivityKpiBands.ts` — band coloring (`executionBand`,
  `effectiveCallBand`, `gpsValidBand`, `statusLabel`, `statusSeverity`)
- `services/commercialSignalRules.ts` — Attention classification (percentiles
  75 / 75 / 25 unchanged)
- `services/fieldActivityCollectionComposition.ts` — `joinSalesmenToCollection`
- `services/formatters.ts` — `formatCurrency`, `formatNumber`, `formatPercent`
- `components/field-activity/FieldActivityTeamKpiStrip.vue`,
  `FieldActivityCollectionHealthSection.vue`,
  `FieldActivityGroupedActionCenter.vue`, `FieldActivityWilayahChart.vue`,
  `FieldActivityTeamTrendChart.vue`
- `models/fieldActivity.ts` (`FieldActivityOverviewResponse`,
  `FieldActivityTeamKpis`, `FieldActivitySalesmanOverviewRow`),
  `models/dashboard.ts` (`DashboardCollectionResponse`)

**Knowledge artifacts (mandatory, review §5.4):**

- `docs/features/btr-portal/dashboard-11-sf02-sales-force-overview.md` — §2,
  §5, §6
- `docs/work/btr-portal/sales-force-overview-dashboard-redesign/sfo-dashboard-ux-blueprint-v2.2.md`
  — §B.1, §C.1, §3.1, Appendix B

### Integration

- **None new.** The page continues to compose the existing
  `getFieldActivityOverview(visitDate)` endpoint and
  `dashboardStore.loadCollection()`; drill-down to `field-activity-detail`
  (`salesPersonId`, `visitDate`) and investigation drill-across are unchanged
  (feasibility §3, §10).

---

## Phases

| Phase | Name | Slices | Business value |
| --- | --- | --- | --- |
| 1 | Execution Funnel | SFO-P1-01 … SFO-P1-03 | Team conversion narrative is visible with leaks; blueprint gap GAP-001 closed |
| 2 | Salesman Scoreboard | SFO-P1-04 … SFO-P1-09 | One bounded, funnel-ordered comparison surface with attention signals replaces four charts |
| 3 | Revenue Concentration Strip | SFO-P1-10 … SFO-P1-12 | Concentration/key-person question preserved while the redundant Order Value chart is removed |
| 4 | Consolidation & Knowledge Sync | SFO-P1-13 … SFO-P1-16 | Redundant charts removed, action-first reading order in place, dead component gone, knowledge updated |
| 5 | Validation | SFO-P1-17 | Phase 1 accepted against objective criteria |

Each phase is independently testable and leaves the dashboard in a coherent
state: additive surfaces ship before anything is removed (feasibility §11
sequencing concern).

### Complexity Index (model-selection aid)

| Slice ID | Complexity (1–5) | Nature of work |
| --- | --- | --- |
| SFO-P1-01 | 2 | Pure funnel formulas + edge-case tests |
| SFO-P1-02 | 3 | New funnel presentational component |
| SFO-P1-03 | 1 | Additive mount edit in an existing section |
| SFO-P1-04 | 4 | Scoreboard ranking/attention/bar-scaling helper + tests |
| SFO-P1-05 | 5 | Scoreboard structure, column groups, sticky header integration |
| SFO-P1-06 | 3 | Rank-by selector, display rank, sort-state consolidation |
| SFO-P1-07 | 2 | CSS in-cell data bars |
| SFO-P1-08 | 3 | Attention column, collection wiring, degradation |
| SFO-P1-09 | 1 | Promote scoreboard between sections |
| SFO-P1-10 | 2 | Concentration aggregation + tests |
| SFO-P1-11 | 2 | Concentration strip component |
| SFO-P1-12 | 2 | Replace Order Value chart with the strip (atomic) |
| SFO-P1-13 | 2 | Remove three ranking charts and dependent code |
| SFO-P1-14 | 1 | DOM reorder of two sections |
| SFO-P1-15 | 1 | Delete orphaned component, verify references |
| SFO-P1-16 | 3 | Knowledge artifact rewrite across two documents |
| SFO-P1-17 | 3 | Regression and acceptance validation |

Suggested agent tier by level (for model selection): 1–2 → lightweight/fast
model; 3 → mid-tier model; 4 → strong reasoning model; 5 → strongest reasoning
model.

---

## Implementation Scope

### Execution Funnel

- **Slices:** SFO-P1-01 (calculations), SFO-P1-02 (component), SFO-P1-03
  (mount in Section B); removal of the replaced charts in SFO-P1-13.
- **Data sourcing:** `FieldActivityOverviewResponse.TeamKpis` only —
  `PlannedVisits`, `ActualVisits`, `EffectiveCalls`, `TotalOrders`,
  `TotalOmzet`, `MissedVisits`, `VisitExecutionPercent`, `EffectiveCallRate`
  (review §2.1).
- **Calculations:** 5 stages; Execution Rate, Effective Call Rate,
  Order Conversion (`TotalOrders / EffectiveCalls`), Average Order Value
  (`TotalOmzet / TotalOrders`); leaks Missed Visits and No-Order Visits
  (`ActualVisits − EffectiveCalls`); null conversion when denominator is zero
  or the API value is null (domain rule: Execution % N/A when Planned = 0;
  Effective Call Rate not meaningful when Actual = 0).
- **API usage:** Existing `getFieldActivityOverview(visitDate)`; **no API
  change** (review §2.1, feasibility §3).
- **UI implementation:** `ExecutionFunnel.vue`, plain markup + CSS (no
  Chart.js funnel/trapezoid — review §2.1 “Shape: not a trapezoid”), five stage
  blocks, value + conversion + leak, no internal scroll, loading/empty states.

### Salesman Scoreboard

- **Slices:** SFO-P1-04 (derivation), SFO-P1-05 (structure), SFO-P1-06
  (ranking), SFO-P1-07 (data bars), SFO-P1-08 (Attention), SFO-P1-09
  (promotion to Section B).
- **Ranking:** Default Order Value descending; “Rank by” limited to Order
  Value, Orders, Effective Call Rate, Visit Execution %, Attention (worst
  first); no composite score (review §2.2).
- **Sorting:** Rank-by selector is the single ranking control; `#` is the
  active display position; deterministic null-last and tie-break rules.
- **Comparison layout:** Column groups ACTIVITY | PRODUCTIVITY | OUTCOME |
  SIGNAL; retained columns mapped; in-cell data bars for the four headline
  measures scaled to column max; existing band coloring reused; sticky header,
  ~10 visible rows, internal scroll (review §2.2 changes 1–7).
- **Data sourcing:** `Salesmen[]` (`FieldActivitySalesmanOverviewRow`);
  `dashboardStore.collection.TopOverdueSalesmen` via
  `joinSalesmenToCollection` + `classifyCommercialSignals` for Attention
  (same population as the Action Center); `fieldActivityKpiBands.ts` for bands.
- **API usage:** Existing endpoint and collection store; no API change.
- **UI implementation:** `FieldActivitySalesmanTable.vue` redesign (PrimeVue
  DataTable with `ColumnGroup`/`Row`, scrollable body, existing Card wrapper).

### Revenue Concentration Strip

- **Slices:** SFO-P1-10 (calculations), SFO-P1-11 (component), SFO-P1-12
  (replacement in Outcomes, shipped together with the Order Value chart
  removal).
- **Concentration calculations:** total order value = Σ `Salesmen[].OmzetAmount`;
  Top-1 share; Top-3 share; ordered segments (Top 3 named + “Others” remainder);
  empty state when total = 0; deterministic tie-break by `SalesPersonCode`.
- **Data sourcing:** `Salesmen[].OmzetAmount` only (review §5.1 row 9,
  feasibility Option C).
- **API usage:** Existing endpoint; no API change.
- **UI implementation:** `RevenueConcentrationStrip.vue`, single 100% stacked
  bar + Top-1/Top-3 summary text, plain markup + CSS, no internal scroll, no
  drill requirement (scoreboard row click remains the drill affordance).

---

## Slices

### SFO-P1-01 — Funnel stage calculation helper

**Objective:** Implement the pure 5-stage Execution Funnel derivation from
`FieldActivityTeamKpis` so the funnel is unit-testable and the component is
purely presentational.

**Deliverable:** `src/services/fieldActivityFunnel.ts` + `fieldActivityFunnel.spec.ts`.

**Complexity:** 2/5 — pure formula and edge-case tests; no UI, state, or integration risk.

**Dependencies:** None.

**Acceptance Criteria:**
- Exports a pure function accepting `FieldActivityTeamKpis | null` and
  returning exactly five ordered stages: Planned Visits
  (`PlannedVisits`), Actual Visits (`ActualVisits`), Effective Calls
  (`EffectiveCalls`), Orders (`TotalOrders`), Order Value (`TotalOmzet`)
  (review §2.1).
- Conversion-out per stage: Execution Rate = API `VisitExecutionPercent`;
  Effective Call Rate = API `EffectiveCallRate`; Order Conversion =
  `TotalOrders / EffectiveCalls`; Average Order Value = `TotalOmzet /
  TotalOrders`.
- Conversion result is `null` when the API value is null or the denominator is
  zero; no `NaN`/`Infinity` is ever produced.
- Leaks: Actual Visits stage → `MissedVisits`; Effective Calls stage →
  No-Order Visits = `ActualVisits − EffectiveCalls`; Planned Visits, Orders,
  and Order Value stages → no leak (see Authority Alignment Note 1).
- Unit tests cover: full happy path; zero Planned (Execution null); zero
  Actual (Effective Call Rate null); zero Effective Calls (Order Conversion
  null); zero Orders (Avg Order Value null); all-zero team day (all stages
  returned, conversions null); leak arithmetic.
- No API, model, or service behavior change; `package.json` unchanged.

**Review Focus:** authority fidelity (five stages, exact formulas, two leaks);
no new threshold; purity/testability; zero/null safety.

---

### SFO-P1-02 — Execution Funnel component

**Objective:** Present the approved 5-stage horizontal conversion strip as a
presentational component.

**Deliverable:** `src/components/field-activity/ExecutionFunnel.vue`.

**Complexity:** 3/5 — new multi-stage presentational component; layout, conversion/leak rendering, and state handling, but no chart or data logic.

**Dependencies:** SFO-P1-01.

**Acceptance Criteria:**
- Props mirror existing component conventions: `kpis: FieldActivityTeamKpis |
  null`, `loading?: boolean`.
- Renders five stage blocks in order Planned Visits → Actual Visits →
  Effective Calls → Orders → Order Value, separated by arrows, in plain markup
  + CSS; no Chart.js import and no new dependency (review §2.1).
- Each stage shows its primary value; stages with a defined conversion show the
  conversion; stages with a leak show the absolute leak as a worklist value
  (e.g., “Missed Visits”, “No-Order Visits”).
- Formatting exclusively via `formatNumber`, `formatPercent`, `formatCurrency`;
  null conversions render as “—”.
- Loading state consistent with existing field-activity components; explicit
  empty state when `kpis` is null.
- No internal scroll container (review §4.1 principle 5).

**Review Focus:** UI state compliance; authority fidelity (stage list, rates,
leaks); presentation-mode safety; no new visualization concept.

---

### SFO-P1-03 — Mount the Execution Funnel in Section B

**Objective:** Render the funnel as the first object of the Performance section,
without removing any existing chart yet.

**Deliverable:** `FieldActivityOverviewView.vue` (Performance section only).

**Complexity:** 1/5 — single additive edit to an existing section; no logic, styling system, or behavior change.

**Dependencies:** SFO-P1-02.

**Acceptance Criteria:**
- `ExecutionFunnel` rendered inside the section with `aria-label="Performance"`,
  above the existing chart grid, with `:kpis="overview?.TeamKpis ?? null"` and
  `:loading="loading"`.
- Funnel appears before the ranking charts in DOM order.
- Existing charts, table, collection health, action center, and trends render
  unchanged; `npm test` and `npm run build` pass.

**Review Focus:** section order compliance (A → B); additive-only change; no
regression to existing consumers.

---

### SFO-P1-04 — Scoreboard derivation helper

**Objective:** Implement the pure scoreboard view-model logic: attention
classification, bounded ranking modes, display rank, and data-bar scaling.

**Deliverable:** `src/services/fieldActivityScoreboard.ts` +
`fieldActivityScoreboard.spec.ts`.

**Complexity:** 4/5 — logic-dense helper: five ranking modes, null-last and tie-break determinism, attention ordering, cross-surface classification consistency, and bar scaling; edge-case-heavy tests.

**Dependencies:** None (uses existing `joinSalesmenToCollection`,
`classifyCommercialSignals`).

**Acceptance Criteria:**
- `rankScoreboardRows(rows, mode, attentionBySalesman?)` supports exactly five
  modes — Order Value, Orders, Effective Call Rate, Visit Execution %,
  Attention (worst first) — and no other ranking dimension (review §2.2).
- Value modes order descending by `OmzetAmount` / `OrdersCount` /
  `EffectiveCallRate` / `VisitExecutionPercent`; rows with `null` measure sort
  last; final tie-break is `SalesPersonCode` ascending.
- Attention mode uses only existing rule output: rows with
  `NeedsCollectionAction` or `NeedsCreditReview` first, ordered by joined
  `OverdueExposure` descending; then remaining rows by Order Value descending;
  tie-break `SalesPersonCode` ascending. No composite score and no new
  percentile threshold.
- `classifyScoreboardAttention(salesmen, topOverdueSalesmen)` returns each
  salesman’s `classifyCommercialSignals` classifications using the same
  population/join as `FieldActivityGroupedActionCenter.vue`; a salesman flagged
  by the Action Center Commercial Risk group receives the identical Attention
  classification here.
- `displayRank(orderedRows)` returns 1-based positions in the current display
  order.
- `columnBarMax(rows, accessor)` returns the maximum non-null value over the
  current rows (0 when empty/all-zero); data bars use value ÷ max × 100%.
- Unit tests: each of the five modes; null-last; tie-breaks; empty rows;
  all-zero; attention ordering; Action Center consistency; bar max with nulls.

**Review Focus:** review §2.2 ranking rules; rule reuse (no new thresholds, no
blended index); deterministic ordering; purity/testability.

---

### SFO-P1-05 — Scoreboard structure, column groups, sticky header

**Objective:** Restructure the existing salesman table into the scoreboard
layout with funnel column groups and a sticky header, preserving all existing
evidence columns and behaviors.

**Deliverable:** `FieldActivitySalesmanTable.vue` (no ranking/data-bar/attention
logic yet).

**Complexity:** 5/5 — highest-risk surface: PrimeVue `ColumnGroup` plus scrollable sticky header compatibility, replacement of pagination with bounded scroll, retained-column remapping, and preservation of search/drill behavior.

**Dependencies:** None.

**Acceptance Criteria:**
- Card title is “Salesman Scoreboard”.
- Column group headers are present exactly as approved — ACTIVITY,
  PRODUCTIVITY, OUTCOME, SIGNAL (review §2.2) — implemented with PrimeVue
  `ColumnGroup`/`Row`.
- Headline columns grouped: ACTIVITY (Planned, Actual, Execution %),
  PRODUCTIVITY (Effective, Eff. Rate), OUTCOME (Orders, Order Value).
- Existing columns Missed, Unplanned, GPS Valid %, Status are retained with
  unchanged formatting and bands, mapped into ACTIVITY (Missed, Unplanned) and
  SIGNAL (GPS Valid %, Status) (Authority Alignment Note 4).
- Pagination is replaced by a bounded internal scroll body showing ~10 rows,
  with the header row remaining visible while the body scrolls (review §2.2
  change 7; feasibility §9 TQ2 — verify PrimeVue compatibility with column
  groups and custom cells).
- Search filter is retained; row click still emits `rowClick` with the same
  payload; row cursor remains pointer.
- No new dependency; `npm test` and `npm run build` pass.

**Review Focus:** UI state compliance; PrimeVue group/sticky compatibility;
preservation of retained columns, search, and drill; no behavior loss.

---

### SFO-P1-06 — Rank-by selector and display rank

**Objective:** Add the bounded ranking control and make the `#` column reflect
the active display order.

**Deliverable:** `FieldActivitySalesmanTable.vue` (ranking behavior).

**Complexity:** 3/5 — selector wiring to a single sort state, display-rank renumbering, removal of header sorting, and interaction with the filtered population.

**Dependencies:** SFO-P1-04, SFO-P1-05.

**Acceptance Criteria:**
- Toolbar contains a “Rank by” selector with exactly five options: Order Value
  (default), Orders, Effective Call Rate, Visit Execution %, Attention (worst
  first) (review §2.2).
- On load, rows are ordered Order Value descending.
- Changing the selection reorders rows per `rankScoreboardRows`; the `#` column
  shows the 1-based position in the active order, not the backend `Rank` field.
- The selector is the single ranking control: free-form column-header sorting is
  removed (`sortable`/`removable-sort` no longer used), so no ranking dimension
  outside the approved five exists (Authority Alignment Note 3).
- No composite score or blended index is introduced (review §2.2).

**Review Focus:** review §2.2 ranking rules; bounded options; deterministic
order; rank/position correctness.

---

### SFO-P1-07 — In-cell data bars for headline measures

**Objective:** Recover the ranking charts’ interval/shape information inside the
scoreboard cells for the four headline measures.

**Deliverable:** `FieldActivitySalesmanTable.vue` (cell rendering).

**Complexity:** 2/5 — CSS in-cell bar widths across four column templates; scaling logic already provided by SFO-P1-04.

**Dependencies:** SFO-P1-04, SFO-P1-05.

**Acceptance Criteria:**
- Execution %, Eff. Rate, Orders, and Order Value cells render a CSS in-cell
  bar whose width equals value ÷ `columnBarMax` × 100% (0 for null/zero); max is
  computed over the currently filtered rows (review §2.2 change 3;
  feasibility §9 TQ1 — CSS-only approach).
- Formatted value text remains exact and readable; bars are decorative
  (`aria-hidden` or equivalent) and do not change the formatted value.
- Existing band coloring on Execution %, Eff. Rate, and GPS Valid % is retained
  (reuse `fieldActivityKpiBands.ts`; review §2.2 change 4).
- No canvas, no chart library, no new dependency; `npm run build` passes.

**Review Focus:** data mapping; band reuse; accessibility (value text); no new
visualization concept.

---

### SFO-P1-08 — Attention / Signal column

**Objective:** Surface “who needs action” inline by reusing the existing
commercial signal rules with their rationale strings.

**Deliverable:** `FieldActivitySalesmanTable.vue` (+ `collection` prop) and
`FieldActivityOverviewView.vue` (pass `dashboard.collection`).

**Complexity:** 3/5 — new nullable prop and view wiring, rule reuse on the Action Center population, badge/tooltip rendering, and graceful degradation.

**Dependencies:** SFO-P1-04, SFO-P1-05.

**Acceptance Criteria:**
- Each row renders Attention tags for the action classifications returned by
  `classifyCommercialSignals` (`NeedsCollectionAction`, `NeedsCreditReview`)
  using `COMMERCIAL_SIGNAL_LABELS`, with `COMMERCIAL_SIGNAL_RATIONALES`
  available as the tooltip/title (review §2.2 change 5); rows with none show an
  explicit “—” state.
- Classification uses the same join (`joinSalesmenToCollection` on
  `TopOverdueSalesmen`) and percentile population as
  `FieldActivityGroupedActionCenter.vue`; badges match the Action Center
  Commercial Risk flags for the same date.
- No new thresholds: percentiles in `commercialSignalRules.ts` are unchanged.
- `collection` prop is nullable; when collection data is absent/unavailable the
  column degrades gracefully (placeholder, no page error) and the rest of the
  scoreboard remains usable.
- `FieldActivityOverviewView.vue` passes `:collection="dashboard.collection"`.
- `npm test` and `npm run build` pass.

**Review Focus:** rule reuse (no new thresholds); Action Center consistency;
null handling; no business rule invention.

---

### SFO-P1-09 — Promote the scoreboard to Section B

**Objective:** Move the scoreboard from Outcomes to Performance, directly under
the Execution Funnel (review §2.2 change 1, §3.2).

**Deliverable:** `FieldActivityOverviewView.vue` (section placement).

**Complexity:** 1/5 — move one component between section containers and verify existing behaviors still work.

**Dependencies:** SFO-P1-03, SFO-P1-08.

**Acceptance Criteria:**
- DOM order inside the `aria-label="Performance"` section: Execution Funnel,
  then Salesman Scoreboard.
- The scoreboard is no longer rendered inside the `aria-label="Outcomes"`
  section; the Wilayah chart remains in Outcomes.
- Scoreboard search, Rank-by, data bars, Attention, and row-click drill all
  continue to work at the new position.

**Review Focus:** section mapping per review §3.2; no duplicate instance; drill
preserved.

---

### SFO-P1-10 — Revenue concentration calculation helper

**Objective:** Implement the pure concentration derivation from
`Salesmen[].OmzetAmount`.

**Deliverable:** `src/services/fieldActivityConcentration.ts` +
`fieldActivityConcentration.spec.ts`.

**Complexity:** 2/5 — simple aggregation with degenerate cases (zero totals, fewer than three salesmen, ties) plus unit tests.

**Dependencies:** None.

**Acceptance Criteria:**
- Exports a pure function over `FieldActivitySalesmanOverviewRow[]` returning:
  total order value; Top-1 share (%); Top-3 share (%); ordered segments of
  named top-3 salesmen plus an “Others” remainder, each with amount and percent
  of total (review §4, §5.1 row 9; blueprint GAP-025).
- Shares = sum(top-N `OmzetAmount`) ÷ total × 100, using one decimal place at
  most for display; segment widths use the same shares.
- Empty state (`isEmpty: true`) when there are no salesmen or total order value
  is 0; no `NaN`/`Infinity`.
- Deterministic ordering: amount descending, then `SalesPersonCode`
  ascending; zero-amount salesmen never outrank positive ones.
- Fewer than three salesmen produce no phantom/zero “Others” segment; zero-width
  segments are omitted.
- Unit tests: normal multi-salesman case; single salesman; two salesmen;
  all-zero; ties; `Others` remainder arithmetic; exact totals.
- No new API and no change to `Salesmen[]` usage elsewhere.

**Review Focus:** concentration math traced to review §4; empty/degenerate cases;
purity/testability.

---

### SFO-P1-11 — Revenue Concentration Strip component

**Objective:** Present the concentration result as a compact strip that replaces
the Order Value chart without losing the concentration question.

**Deliverable:** `src/components/field-activity/RevenueConcentrationStrip.vue`.

**Complexity:** 2/5 — one stacked-bar render with helper-provided segments, labels, and empty state; no chart library or interaction.

**Dependencies:** SFO-P1-10.

**Acceptance Criteria:**
- Props: `salesmen: FieldActivitySalesmanOverviewRow[]`, `loading?: boolean`.
- Renders a single 100% stacked horizontal bar: one segment per named top-3
  salesman plus “Others”; segment widths equal the computed shares (review §4).
- Displays Top-1 and Top-3 shares (e.g., “Top 3 = 66% of order value”) using
  `formatPercent`, and monetary amounts using `formatCurrency`; top-3 segments
  are identified by code/name.
- Empty state when concentration `isEmpty` is true (e.g., “No order value for
  this date.”).
- Plain markup + CSS only: no Chart.js, no new dependency, no internal scroll.
- No drill handler required; the scoreboard row click remains the drill
  affordance (review §5.3 guardrail).

**Review Focus:** concentration insight preserved (review §1.3 chart 4);
presentation-mode safety; formatting reuse.

---

### SFO-P1-12 — Replace the Order Value chart with the concentration strip

**Objective:** Remove the Order Value comparison chart from Outcomes and place
the Revenue Concentration strip in a single slice (mandatory pairing — the
chart must not be removed without its replacement).

**Deliverable:** `FieldActivityOverviewView.vue` (Outcomes section).

**Complexity:** 2/5 — atomic remove-and-mount in one section, including removal of the now-unused computed.

**Dependencies:** SFO-P1-11.

**Acceptance Criteria:**
- The Outcomes section no longer renders the Order Value
  `FieldActivityComparisonChart`; `omzetChartItems` and its import usage are
  removed.
- `RevenueConcentrationStrip` is rendered in the Outcomes section together with
  the Wilayah chart, with the concentration object before the territory chart
  (review §3.2, §4).
- In the delivered revision there is no state in which the Order Value chart is
  removed while the strip is absent (feasibility §8 risk row 1; review §5.3).
- `npm test` and `npm run build` pass.

**Review Focus:** mandatory pairing; concentration question preserved; no
residual chart code.

---

### SFO-P1-13 — Remove the three ranking charts

**Objective:** Remove the Visit Execution %, Effective Call Rate, and Orders
Generated charts from Section B now that the funnel and scoreboard have
replaced them.

**Deliverable:** `FieldActivityOverviewView.vue` (Performance section).

**Complexity:** 2/5 — mechanical removal of three charts plus their dependent computed/imports; breadth of cleanup, no new logic.

**Dependencies:** SFO-P1-03, SFO-P1-09, SFO-P1-12.

**Acceptance Criteria:**
- No `FieldActivityComparisonChart` element remains in the Performance section;
  `executionChartItems`, `effectiveChartItems`, `ordersChartItems`,
  `toComparisonItems`, `chartLabel`, and the comparison-chart import are
  removed if unused.
- No chart scroll containers remain on the page (the scoreboard body is the
  only internal scroll surface).
- The `Rankings` payload remains in the model and is still consumed by
  `FieldActivityGroupedActionCenter.vue` (review §5.3 guardrail).
- `npm test` and `npm run build` pass.

**Review Focus:** replacement-before-removal; completeness (no orphaned
code/imports); Rankings untouched.

---

### SFO-P1-14 — Move Action Center above Outcomes

**Objective:** Apply the single approved section-order change: Action Center
before Outcomes (review §3.2, §3.3).

**Deliverable:** `FieldActivityOverviewView.vue` (section order).

**Complexity:** 1/5 — pure DOM reorder of two existing sections; no content or behavior change.

**Dependencies:** SFO-P1-13.

**Acceptance Criteria:**
- Final page order: A. Status → B. Performance (Funnel, Scoreboard) →
  D. Action Center → C. Outcomes (Concentration, Territory) → E. Trends.
- The Action Center component receives the same props/events as before; no
  content, cap, reason, or ordering changes inside it (Phase 2 scope).
- All drill handlers (salesman, commercial risk, recognition) continue to work.

**Review Focus:** layout compliance with review §3.2/§3.3; no behavioral change
to Action Center internals.

---

### SFO-P1-15 — Remove the orphaned comparison chart component

**Objective:** Complete the approved deprecation by removing the component that
now has no callers.

**Deliverable:** deletion of
`src/components/field-activity/FieldActivityComparisonChart.vue`.

**Complexity:** 1/5 — delete one orphaned file and verify no references remain; build confirms correctness.

**Dependencies:** SFO-P1-12, SFO-P1-13.

**Acceptance Criteria:**
- A repository search for `FieldActivityComparisonChart` and
  `FieldActivityComparisonItem` returns no source references (historical docs
  excluded).
- The file is removed; no import, type, or test references remain.
- `npm run build` (vue-tsc + vite) passes.

**Review Focus:** dead-code removal; no hidden callers; typecheck.

---

### SFO-P1-16 — Knowledge artifact updates

**Objective:** Synchronize permanent and work knowledge with the shipped
behavior (review §5.4; AGENTS.md “Keep Knowledge Updated”).

**Deliverables:**
- `docs/features/btr-portal/dashboard-11-sf02-sales-force-overview.md`
- `docs/work/btr-portal/sales-force-overview-dashboard-redesign/sfo-dashboard-ux-blueprint-v2.2.md`

**Complexity:** 3/5 — multi-section rewrite across two documents with mandatory traceability to the authority and knowledge–code consistency.

**Dependencies:** SFO-P1-15 (all code slices implemented).

**Acceptance Criteria:**
- Feature doc §2 “Management Reading Sequence” reflects the new flow
  (Status → Funnel → Scoreboard → Action Center → Context → Trends);
  §5 documents the Salesman Scoreboard (column groups, five Rank-by options,
  Attention signals, sticky header) as the primary comparison surface; §6 is
  rewritten — it no longer describes the four ranking charts as current
  behavior and instead documents the funnel, scoreboard, and concentration
  strip.
- Blueprint v2.2 §B.1 shows the corrected five-stage funnel (Effective Calls
  included) and no longer claims “(unchanged)” for a surface that was never
  built; §C.1 becomes the Revenue Concentration strip; §3.1 hierarchy and
  Appendix B placement matrix reflect Action Center above Outcomes and the
  removed charts (review §5.4).
- No new KPI definition, threshold, or rule is introduced by documentation;
  every statement traces to review v1.0 / feasibility v1.0.

**Review Focus:** knowledge–code consistency; no invented business rules in
docs; traceability.

---

### SFO-P1-17 — Phase 1 regression and acceptance validation

**Objective:** Independently validate the complete Phase 1 outcome against the
approved scope.

**Complexity:** 3/5 — end-to-end regression across build/tests plus presentation-mode and degenerate-data checks; broad criteria, low change risk.

**Dependencies:** SFO-P1-16.

**Acceptance Criteria:**
- `npm test` (vitest run) and `npm run build` (vue-tsc + vite build) pass from
  `src/j05-btr-distrib/btr.portal.web`.
- Manual acceptance on a date with full data:
  - funnel values match the KPI strip / `TeamKpis` for the selected date;
  - five Rank-by options reorder the scoreboard and `#` follows the order;
  - data bars scale to the visible column maximum; band colors unchanged;
  - Attention badges match the Action Center Commercial Risk flags;
  - concentration Top-1/Top-3 shares match manual sums of `OmzetAmount`;
  - scoreboard header stays visible while the body scrolls (~10 rows);
  - scoreboard row click opens `field-activity-detail` with the correct
    `salesPersonId` and `visitDate`;
  - no chart scroll boxes remain; page is usable in presentation mode.
- Degenerate data (before visit-plan go-live, zero order value, zero activity)
  renders funnel, scoreboard, and strip without `NaN`, `Infinity`, or errors.
- `git diff` confirms no backend, database, ETL, or API changes.

**Review Focus:** full acceptance matrix; presentation mode; empty/degenerate
states; scope containment.

---

## Dependencies

### Prerequisite matrix

| Slice | Depends on | Order constraint |
| --- | --- | --- |
| SFO-P1-01 | — | May start first |
| SFO-P1-02 | SFO-P1-01 | After 01 |
| SFO-P1-03 | SFO-P1-02 | After 02 |
| SFO-P1-04 | — | May start first |
| SFO-P1-05 | — | May start first |
| SFO-P1-06 | SFO-P1-04, SFO-P1-05 | After both |
| SFO-P1-07 | SFO-P1-04, SFO-P1-05 | After both |
| SFO-P1-08 | SFO-P1-04, SFO-P1-05 | After both |
| SFO-P1-09 | SFO-P1-03, SFO-P1-08 | After both |
| SFO-P1-10 | — | May start first |
| SFO-P1-11 | SFO-P1-10 | After 10 |
| SFO-P1-12 | SFO-P1-11 | After 11 |
| SFO-P1-13 | SFO-P1-03, SFO-P1-09, SFO-P1-12 | After all replacements verified |
| SFO-P1-14 | SFO-P1-13 | After 13 |
| SFO-P1-15 | SFO-P1-12, SFO-P1-13 | After last caller removed |
| SFO-P1-16 | SFO-P1-15 | After code complete |
| SFO-P1-17 | SFO-P1-16 | Last |

No slice depends on a future slice.

### Blocking conditions

1. **SFO-P1-12 must not ship without SFO-P1-11 GO** — concentration strip and
   Order Value chart removal are one atomic change (mandatory pairing).
2. **SFO-P1-13 is blocked until SFO-P1-03, SFO-P1-09, SFO-P1-12 are GO** — no
   chart may be removed before its replacement exists and is verified.
3. **SFO-P1-06/07/08 are blocked until SFO-P1-05 is GO** — ranking, bars, and
   attention build on the scoreboard structure.
4. **SFO-P1-16 is blocked until all code slices are GO** — knowledge must
   describe shipped behavior only.
5. **PrimeVue sticky-header verification (SFO-P1-05):** if the DataTable
   sticky/scrollable header proves incompatible with `ColumnGroup`, the slice
   returns NO-GO for remediation; the fallback allowed by the review’s own
   problem analysis is CSS `position: sticky` on header cells inside the
   existing scroll container — no new component library or architecture may be
   introduced without replanning.
6. **Risk:** the review v1.0 §2.1 “Calls without order” label and the five-option
   ranking control warrant confirmation during review of SFO-P1-01 and
   SFO-P1-06; if the Review Agent rejects the recorded interpretations, those
   slices return NO-GO for replanning rather than being silently adjusted.

### Recommended implementation order

```text
Track A: 01 → 02 → 03 ─────────────────────────────┐
Track B: 04 → 06 ─┐                                │
         05 → 07 ─┼→ 08 → 09 ──────────────────────┤
Track C: 10 → 11 → 12 ─────────────────────────────┤
                                                   ↓
                                        13 → 14 → 15 → 16 → 17
```

Tracks A, B, and C are independent and may run in parallel; the merge point is
the view recomposition in SFO-P1-13. All slices touch disjoint files until the
view slices, which are deliberately ordered (03 → 09 → 12 → 13 → 14) to avoid
conflicting edits to `FieldActivityOverviewView.vue`.

---

## Progress Tracker

Lifecycle: PLANNED → IN IMPLEMENTATION → IMPLEMENTED → IN REVIEW → GO
(or NO-GO → REMEDIATION → IN REVIEW → GO).

| Slice ID | Status | Implementation History | Review History | Remediation History |
| --- | --- | --- | --- | --- |
| SFO-P1-01 | GO | 2026-09-14: buildFunnelStages + spec (15 tests) | 2026-09-14: GO — no findings | — |
| SFO-P1-02 | GO | 2026-09-14: ExecutionFunnel.vue created (Card + 5-stage strip, conversions/leaks, loading/empty) | 2026-09-14: GO — all AC pass; test 464, build OK | — |
| SFO-P1-03 | GO | 2026-09-14: ExecutionFunnel mounted in Performance section (additive, above chart grid) | 2026-09-14: GO — additive-only, DOM order correct; test 464, build OK | — |
| SFO-P1-04 | GO | 2026-09-14: fieldActivityScoreboard.ts (rankScoreboardRows, classifyScoreboardAttention, displayRank, columnBarMax, barWidthPercent) + spec (31 tests) | 2026-09-14: GO — all AC pass; test 495, build OK | — |
| SFO-P1-05 | GO | 2026-09-14: FieldActivitySalesmanTable.vue restructured into Salesman Scoreboard (ColumnGroup ACTIVITY/PRODUCTIVITY/OUTCOME/SIGNAL, retained columns, scrollable sticky header ~10 rows, pagination removed; search, initial sort, drill preserved) | 2026-09-14: GO — all AC pass; ColumnGroup + native sticky thead verified via SSR structural check; test 495, build OK | — |
| SFO-P1-06 | GO | 2026-09-14: FieldActivitySalesmanTable.vue — Rank-by Select (five options, default Order Value), rankScoreboardRows ordering, displayRank-driven # column, header sorting removed | 2026-09-14: GO — all AC pass; bounded ranking, no free-form sort, display rank correct; test 495, build OK (INFO: attention map wiring deferred to SFO-P1-08) | — |
| SFO-P1-07 | GO | 2026-09-14: FieldActivitySalesmanTable.vue — CSS in-cell data bars on Execution %, Eff. Rate, Orders, Order Value scaled to filtered column max via columnBarMax/barWidthPercent; bands retained; bars aria-hidden | 2026-09-14: GO — all AC pass; bars scaled to filtered max, decorative, bands retained, no new dependency; test 495, build OK | — |
| SFO-P1-08 | GO | 2026-09-14: FieldActivitySalesmanTable.vue — nullable `collection` prop, `classifyScoreboardAttention` map wired into `rankScoreboardRows` (Attention mode), Attention column added to SIGNAL group (tags via `COMMERCIAL_SIGNAL_LABELS`, rationale title via `COMMERCIAL_SIGNAL_RATIONALES`, “—” empty state, unavailable placeholder); FieldActivityOverviewView.vue passes `:collection="dashboard.collection"` | 2026-09-14: GO — all 5 AC pass; tags reuse the two action rules on the Action Center join/population, no new thresholds; nullable collection degrades to a “—” placeholder; test 495, build OK | — |
| SFO-P1-09 | GO | 2026-09-14: FieldActivityOverviewView.vue — FieldActivitySalesmanTable promoted from Outcomes to Performance, rendered directly under ExecutionFunnel; Outcomes retains Order Value chart + Wilayah chart; search/Rank-by/bars/Attention/drill props unchanged | 2026-09-14: GO — all 3 AC pass; single instance in Performance, DOM order Funnel→Scoreboard, no duplicate in Outcomes; test 495, build OK | — |
| SFO-P1-10 | GO | 2026-09-14: fieldActivityConcentration.ts + spec (13 tests) — buildRevenueConcentration returns total, Top-1/Top-3 shares, named top-3 + Others segments, empty state, one-decimal shares, deterministic amount-desc/code-asc ordering | 2026-09-14: GO — all AC pass; concentration math traced to review §4, empty/degenerate safe, no new threshold/API; test 508, build OK | — |
| SFO-P1-11 | GO | 2026-09-14: RevenueConcentrationStrip.vue created (Card + Top-1/Top-3 summary, single 100% stacked bar with named top-3 + Others, legend with code/name + currency + percent, loading/empty) | 2026-09-14: GO — all 6 AC pass; plain markup + CSS, `buildRevenueConcentration` reuse, formatPercent/formatCurrency, no Chart.js/no new dependency/no internal scroll; test 508, build OK | — |
| SFO-P1-12 | GO | 2026-09-14: FieldActivityOverviewView.vue — Order Value FieldActivityComparisonChart removed from Outcomes together with its `omzetChartItems` computed; RevenueConcentrationStrip mounted in Outcomes before FieldActivityWilayahChart (atomic replace) | 2026-09-14: GO — all 4 AC pass; chart + omzetChartItems removed, strip rendered before Wilayah in Outcomes, atomic pairing intact, comparison-chart import retained only for the three Phase-1 charts still pending SFO-P1-13; test 508, build OK | — |
| SFO-P1-13 | GO | 2026-09-14: FieldActivityOverviewView.vue — removed the three ranking FieldActivityComparisonChart elements (Visit Execution %, Effective Call Rate, Orders Generated), `executionChartItems`/`effectiveChartItems`/`ordersChartItems` computeds, `toComparisonItems`/`chartLabel` helpers, the now-unused comparison-chart + `FieldActivityComparisonItem` + `FieldActivitySalesmanOverviewRow` imports, and the now-empty `__charts` grid style; `Rankings` payload untouched (still passed to FieldActivityGroupedActionCenter) | 2026-09-14: GO — all 4 AC pass; no comparison-chart element/scroll container remains in Performance, `Rankings` still consumed by Action Center, diff confined to the one deliverable file; test 508, build OK (INFO: retained Territory/Wilayah chart keeps its pre-existing bounded scroll, explicitly out of this slice per §5.1 row 10 and the Impact Inventory) | — |
| SFO-P1-14 | GO | 2026-09-14: FieldActivityOverviewView.vue — Action Center section moved above Outcomes section in DOM (final order A Status → B Performance → D Action Center → C Outcomes → E Trends); Action Center props/events unchanged, drill handlers intact | 2026-09-14: GO — all 3 AC pass; DOM order verified Status→Performance→Action Center→Outcomes→Trends; Action Center props/events and all three drill handlers unchanged; diff is a pure 2-section reorder in one file; test 508, build OK | — |
| SFO-P1-15 | GO | 2026-09-14: deleted orphaned `FieldActivityComparisonChart.vue`; repo-wide search confirms no source references to `FieldActivityComparisonChart`/`FieldActivityComparisonItem` (docs excluded); `npm test` 508 passed, `npm run build` OK | 2026-09-14: GO — all 3 AC pass; no source/import/type/test references remain, diff confined to the deliverable deletion + tracker, feasibility §7 item 6 deprecation realized; test 508, build OK | — |
| SFO-P1-16 | GO | 2026-09-14: Feature doc §2 reading sequence (Status → Funnel → Scoreboard → Action Center → Context → Trends), §5 rewritten as Salesman Scoreboard (column groups, five Rank-by options, Attention, sticky header/scroll), §6 rewritten (funnel/scoreboard/concentration, charts no longer current), §12 surface list; Blueprint §3.1 hierarchy (Action Center above Outcomes), §B.1 corrected five-stage funnel (removes "(unchanged)"), §C.1 Revenue Concentration strip, §B heading, Appendix B placement matrix | 2026-09-14: GO — all 3 AC pass; knowledge–code consistency verified against shipped components/services (ExecutionFunnel, FieldActivitySalesmanTable, RevenueConcentrationStrip, fieldActivityFunnel/Scoreboard/Concentration); no new KPI/threshold/rule; docs-only, no code/build/test impact; INFO: consistency edits beyond the named sections (§12 surface list, blueprint §2.2 reading order, section B heading) | — |
| SFO-P1-17 | GO | 2026-09-14: Phase 1 validation — `npm test` 508/508 pass and `npm run build` (vue-tsc + vite) pass from `btr.portal.web`; Phase 1 diff (42720ade..5f2d281c) confined to frontend (`btr.portal.web`) + docs (no backend/database/ETL/API change); no `FieldActivityComparisonChart`/`FieldActivityComparisonItem` source references; funnel/scoreboard/concentration acceptance verified in code and specs | 2026-09-14: GO — all 4 AC pass; test 508, build OK; five Rank-by options (SCOREBOARD_RANK_OPTIONS), display-rank renumbering, data bars scaled to filtered column max via columnBarMax/barWidthPercent, Attention reuse of classifyCommercialSignals on the Action Center join/population, concentration Top-1/Top-3 from OmzetAmount, drill to field-activity-detail (salesPersonId+visitDate) all verified in code; no backend/db/ETL/API diff; INFO: pre-existing Wilayah chart bounded scroll retained (accepted out of scope in SFO-P1-13), so "no chart scroll boxes" is satisfied for the removed ranking charts only | — |

---

## Validation

### Scope coverage

- Execution Funnel: data sourcing (SFO-P1-01), calculations (SFO-P1-01), API
  usage (existing `TeamKpis`, no change), UI (SFO-P1-02, SFO-P1-03). ✅
- Salesman Scoreboard: ranking and sorting (SFO-P1-04, SFO-P1-06), comparison
  layout (SFO-P1-05, SFO-P1-07), Attention data sourcing (SFO-P1-08), promotion
  (SFO-P1-09). ✅
- Revenue Concentration Strip: concentration calculations (SFO-P1-10), data
  sourcing (`Salesmen[].OmzetAmount`), UI (SFO-P1-11) and chart replacement
  (SFO-P1-12). ✅
- Approved Phase 1 companions: three chart removals (SFO-P1-13), Order Value
  replacement (SFO-P1-12), section reorder (SFO-P1-14), deprecation
  (SFO-P1-15), knowledge updates (SFO-P1-16). ✅

### Slice and dependency validation

- Every slice has a single objective, an explicit dependency list, objective
  acceptance criteria, and a review focus. ✅
- No slice depends on a future slice; the dependency graph is acyclic. ✅
- All slices are independently reviewable: pure-helper slices through unit
  tests, component slices through build/typecheck plus behavioral criteria,
  view slices through build plus DOM-order/manual criteria. ✅
- Tracker covers all 17 slices with the planning-skill lifecycle. ✅

### Decision isolation

- No new KPI, threshold, formula, signal, ranking dimension, visualization, or
  section is introduced beyond the review v1.0 / feasibility v1.0. ✅
- No backend, database, API, or integration change is planned. ✅
- No Phase 2 item (quadrant view; Action Center reason lines/recognition
  refinement) is included. ✅
- The six implementation interpretations are recorded in “Authority Alignment
  Notes” for direct review rather than applied silently. ✅

### Authority compliance

- Planning authority FEASIBILITY ASSESSMENT respected; the fixed Phase 1 scope
  from feasibility §7 / review §5.2 is implemented without challenge,
  replacement, or alternative proposals. ✅
