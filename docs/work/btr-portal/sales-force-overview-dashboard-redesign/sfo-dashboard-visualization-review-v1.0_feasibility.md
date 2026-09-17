# FEASIBILITY ASSESSMENT

## 1. Executive Summary

### Request

Redesign the Sales Force Overview dashboard visualization architecture by:
- Removing three redundant ranking charts (Visit Execution %, Effective Call Rate, Orders Generated)
- Replacing the Order Value chart with a Revenue Concentration strip
- Introducing a 5-stage Execution Funnel as the primary narrative surface
- Promoting and redesigning the existing Salesman Table into a Scoreboard with column groups, in-cell data bars, and Attention signals
- Reordering sections (Action Center above Outcomes)
- Adding a Quadrant view toggle (Phase 2)

### Recommendation

**FEASIBLE** — The review document is well-structured, evidence-based, and identifies a clear path forward. All required data is already available from the existing API. No backend changes are required. The migration is a pure presentation-layer change with manageable complexity.

### Feasibility Result

```text
FEASIBLE
```

---

## 2. Request Understanding

### Requested Capability

Consolidate four siloed ranking charts into a unified visualization architecture consisting of:
1. **Execution Funnel** — team-level 5-stage conversion strip showing Planned → Actual → Effective → Orders → Order Value
2. **Salesman Scoreboard** — promoted table with column groups (Activity / Productivity / Outcome / Signal), in-cell data bars, and Attention badges
3. **Revenue Concentration strip** — compact visualization showing revenue distribution across salesmen
4. **Quadrant view** (Phase 2) — scatter chart plotting Revenue × Effective Call Rate

### Business Objective

- Eliminate four-fold redundancy in salesman comparison surfaces
- Provide a narrative anchor (funnel) for the dashboard
- Reduce vertical space consumption (~2,200px → ~570px)
- Fix structural scroll problem (4 independent scroll containers → 1)
- Make the dashboard presentation-mode safe

### Expected Outcome

- One primary comparison surface (Scoreboard)
- One narrative surface (Funnel)
- One scroll context
- One row per salesman evaluation
- ~4× reduction in vertical space for salesman comparison content

---

## 3. Current State Analysis

### Existing Business Flow

The current dashboard follows this reading sequence:
1. Status (KPI strip + Commercial Health)
2. Performance (3 ranking charts: Visit Execution %, Effective Call Rate, Orders Generated)
3. Outcomes (Order Value chart + Salesman Table + Wilayah chart)
4. Action Center (3 groups: Field Execution, Commercial Risk, Recognition)
5. Trends (7/30-day sales trends)

**Problem:** The four ranking charts duplicate data already present in Status cards, Salesman Table, Rankings payload, and Trends.

### Existing Components

| Component | Path | Purpose |
|-----------|------|---------|
| `FieldActivityComparisonChart.vue` | `src/components/field-activity/` | Generic horizontal bar chart for salesman ranking |
| `FieldActivitySalesmanTable.vue` | `src/components/field-activity/` | Sortable table with all salesman KPIs |
| `FieldActivityGroupedActionCenter.vue` | `src/components/field-activity/` | Action lists grouped by intervention type |
| `FieldActivityTeamKpiStrip.vue` | `src/components/field-activity/` | Team-level KPI cards |
| `FieldActivityWilayahChart.vue` | `src/components/field-activity/` | Territory performance chart |
| `FieldActivityTeamTrendChart.vue` | `src/components/field-activity/` | 7/30-day trend charts |

### Existing Database

No database changes required. All data comes from the `getFieldActivityOverview` API endpoint.

### Existing API

**Endpoint:** `getFieldActivityOverview(visitDate)`

**Response model:** `FieldActivityOverviewResponse`

| Field | Type | Used By Funnel |
|-------|------|----------------|
| `TeamKpis.PlannedVisits` | number | ✓ Stage 1 |
| `TeamKpis.ActualVisits` | number | ✓ Stage 2 |
| `TeamKpis.EffectiveCalls` | number | ✓ Stage 3 |
| `TeamKpis.MissedVisits` | number | ✓ Leak calculation |
| `TeamKpis.TotalOrders` | number | ✓ Stage 4 |
| `TeamKpis.TotalOmzet` | number | ✓ Stage 5 |
| `Salesmen[]` | array | ✓ Scoreboard + Concentration |
| `Rankings` | object | ✓ Action Center (unchanged) |

**Critical finding:** All funnel values derive from `TeamKpis` — all already returned. No API change required.

### Existing Integrations

None affected. This is a presentation-layer only change.

### Existing Security Model

None affected. No changes to data access or authorization.

---

## 4. Impact Analysis

### Backend Impact

**None.** The review explicitly states: "No API change is required for any item. Every value is already returned by the overview endpoint."

### Database Impact

**None.** No schema changes, no new tables, no new views.

### Frontend Impact

| Component | Action | Complexity |
|-----------|--------|------------|
| `FieldActivityComparisonChart.vue` | Deprecate (no callers after migration) | Low |
| `FieldActivitySalesmanTable.vue` | Redesign (column groups, data bars, Attention column, Rank-by selector, sticky header) | Medium |
| `FieldActivityOverviewView.vue` | Restructure layout (section reordering, remove charts, add funnel) | Medium |
| **New: ExecutionFunnel.vue** | Create 5-stage conversion strip | Medium |
| **New: RevenueConcentrationStrip.vue** | Create compact concentration visualization | Low–Med |
| **New: QuadrantView.vue** (Phase 2) | Create scatter chart (Revenue × ECR) | Med–High |

### Integration Impact

**None.** No API, event, or external system changes.

### Security Impact

**None.** No changes to data access, authorization, or sensitive data handling.

---

## 5. Gap Analysis

| Gap ID | Type | Description |
|--------|------|-------------|
| GAP-001 | Functional | Execution Funnel does not exist — blueprint §B.1 specifies it but it was never built |
| GAP-002 | Functional | Revenue Distribution (blueprint §C.1) was never built — Order Value chart is the only concentration indicator |
| GAP-003 | UX | Four independent scroll containers prevent simultaneous salesman comparison across charts |
| GAP-004 | UX | Scroll boxes are unusable in presentation mode (480px inner scroll) |
| GAP-005 | Functional | Blueprint funnel specifies 4 stages but should be 5 (Effective Calls stage missing) |
| GAP-006 | Functional | No scatter chart component exists in the field-activity module (exists elsewhere: `PrincipalCoverageReachAnalysis.vue`) |

---

## 6. Solution Options

### Option A — Execution Funnel

**Approach:** Build a horizontal conversion strip using plain markup + CSS (not Chart.js funnel/trapezoid).

**Advantages:**
- All values derive from existing `TeamKpis` — no new API
- Closes documented blueprint gap (§B.1)
- Narrative anchor for entire dashboard
- Prints and projects cleanly

**Disadvantages:**
- New component to build and test

**Recommendation:** ADOPT — primary Section B object.

### Option B — Salesman Scoreboard

**Approach:** Redesign existing `FieldActivitySalesmanTable.vue` with column groups, in-cell data bars, Attention column, Rank-by selector, and sticky header.

**Advantages:**
- Promotion of existing component — materially lower cost than new build
- Reuses existing `fieldActivityKpiBands.ts` and `commercialSignalRules.ts`
- Solves scroll problem (sticky header = always-visible axis)
- Column order = mental model (Activity → Productivity → Outcome → Signal)

**Disadvantages:**
- Requires careful column group design
- In-cell data bars need custom rendering

**Recommendation:** ADOPT — primary comparison surface.

### Option C — Revenue Concentration Strip

**Approach:** Compact visualization showing top-N salesmen as percentage of total order value.

**Advantages:**
- Derived from existing `Salesmen[].OmzetAmount` — no new data
- Replaces Order Value chart without losing concentration insight
- Low complexity

**Disadvantages:**
- New component to build

**Recommendation:** ADOPT — Phase 1, replaces Order Value chart.

### Option D — Quadrant View (Phase 2)

**Approach:** Scatter chart plotting Revenue × Effective Call Rate, with minimum-visit guard and bubble size = Actual Visits.

**Advantages:**
- Existing scatter chart implementation in codebase (`PrincipalCoverageReachAnalysis.vue`) provides pattern
- Reveals volume-driven vs conversion-driven performance
- High value for owners/executives

**Disadvantages:**
- Net-new chart type for field-activity module
- Label collision at 16–40 points needs handling
- Minimum-visit guard required to prevent noise
- Lower value for daily managers

**Recommendation:** ADOPT as Phase 2 — view toggle on scoreboard, not new section.

---

## 7. Recommended Approach

### Phase 1 — Consolidate (recommended)

1. Build Execution Funnel (5-stage conversion strip)
2. Redesign Salesman Scoreboard (column groups, data bars, Attention column, Rank-by selector, sticky header)
3. Remove three ranking charts
4. Replace Order Value chart with Revenue Concentration strip
5. Move Action Center above Outcomes
6. Deprecate `FieldActivityComparisonChart.vue`

**Rationale:** Highest value per unit of complexity. Resolves all three identified problems (redundancy, scroll, missing narrative).

### Phase 2 — Deepen (optional)

7. Quadrant view toggle on scoreboard
8. Action Center per-item reason lines and recognition refinement

**Rationale:** Adds strategic lens but lower priority for daily operations.

### Implementation Constraints

- No API changes — pure presentation-layer migration
- Preserve existing drill-to-detail affordance (row click → salesman detail)
- Reuse existing services (`fieldActivityKpiBands.ts`, `commercialSignalRules.ts`)
- Follow blueprint rule: no composite score (§2.3)
- Concentration strip must ship with chart removal (do not silently delete concentration question)

---

## 8. Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Deleting Order Value chart silently deletes concentration question | High | Low | Concentration strip is Phase 1, not optional. Ship together. |
| Loss of drill-to-detail affordance | Medium | Low | Scoreboard row-click already provides it. Verify before removing charts. |
| Composite-score pressure returns during design | Medium | Medium | Blueprint §2.3 rule stands: no blended index. Use Attention badges + per-cell deviation vs median. |
| Users resist losing familiar charts | Low | Medium | Scoreboard is promoted version of existing table they already use. |
| Quadrant promotes low-volume noise | Medium | Low | Minimum-visit guard + bubble size = Actual Visits. Mandatory. |
| `Rankings` payload left orphaned | Low | Low | Not orphaned: Action Center consumes Top/Bottom sets. Do not remove from model. |

---

## 9. Open Questions

### Business Question

None. The review document provides clear business justification and aligns with blueprint principles.

### Technical Question

1. **In-cell data bar implementation:** Should data bars use CSS-only approach (background gradient) or require custom rendering in DataTable? CSS-only is simpler but less flexible.

2. **Sticky header implementation:** PrimeVue DataTable supports sticky headers natively. Verify compatibility with column groups and custom rendering.

3. **Scatter chart label collision:** For Phase 2, what is the expected maximum salesman count? 16–40 points need different collision strategies.

### Operational Question

None. No changes to data collection, scheduling, or operational processes.

---

## 10. Implementation Impact Inventory

### Backend

None.

### Database

None.

### Frontend

| Component | Path | Action |
|-----------|------|--------|
| `FieldActivityComparisonChart.vue` | `src/components/field-activity/` | Deprecate |
| `FieldActivitySalesmanTable.vue` | `src/components/field-activity/` | Redesign |
| `FieldActivityOverviewView.vue` | `src/views/dashboard/` | Restructure |
| `ExecutionFunnel.vue` | `src/components/field-activity/` | Create (new) |
| `RevenueConcentrationStrip.vue` | `src/components/field-activity/` | Create (new) |
| `QuadrantView.vue` | `src/components/field-activity/` | Create (Phase 2) |

### Services (reuse, no changes)

| Service | Path | Usage |
|---------|------|-------|
| `fieldActivityKpiBands.ts` | `src/services/` | Scoreboard band coloring |
| `commercialSignalRules.ts` | `src/services/` | Attention badges |
| `formatters.ts` | `src/services/` | Number/currency/percent formatting |
| `chartLayout.ts` | `src/services/` | Chart options (Phase 2 scatter) |

### Integration

None.

### Security

None.

---

## 11. Planning Readiness

### Status

```text
READY
```

### Blocking Issues

None. All required information is available:
- Request is clearly understood
- Current state is analyzed
- All impacted areas are identified
- Gaps are documented
- Viable solutions identified
- Risks are identified
- No blocking open questions

### Planner Guidance

**Implementation scope:**
- Phase 1: 6 items (funnel, scoreboard redesign, 3 chart removals, concentration strip, section reorder, deprecation)
- Phase 2: 2 items (quadrant view, action center enhancements)

**Major dependencies:**
- `FieldActivitySalesmanTable.vue` redesign is the highest-complexity item and should be planned first
- `ExecutionFunnel.vue` is the highest-business-value item and should be prioritized
- Chart removals depend on scoreboard and concentration strip being ready

**Sequencing concerns:**
- Do not remove charts before replacements are verified
- Concentration strip must ship with Order Value chart removal
- Phase 2 quadrant depends on Phase 1 scoreboard completion

**Review concerns:**
- Verify drill-to-detail affordance preserved after chart removal
- Verify presentation-mode usability with new layout
- Verify PrimeVue DataTable sticky header compatibility with column groups

---

# PLANNING HANDOFF RULE

This assessment identifies **what must change**:

1. Remove 3 ranking charts
2. Replace 1 chart with concentration strip
3. Build Execution Funnel
4. Redesign Salesman Scoreboard
5. Reorder sections
6. Build Quadrant view (Phase 2)

The Planning Agent determines **how it will be implemented** (slices, phases, sequencing).

The Implementation Agent writes code.

The Review Agent verifies compliance.
