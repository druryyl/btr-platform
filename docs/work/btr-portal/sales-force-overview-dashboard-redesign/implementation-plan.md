# IMPLEMENTATION PLAN — Sales Force Overview Dashboard Redesign (v2.2)

- **Planning Authority:** FEASIBILITY ASSESSMENT (Mode B — Feasibility-Driven Planning)
- **Authoritative inputs:**
  - `sfo-dashboard-ux-blueprint-v2.2-feasibility-assessment.md`
  - `sfo-dashboard-ux-blueprint-v2.2.md` (approved UX design)
- **Current view:** `FieldActivityOverviewView.vue` (`SF02`, `/portal/dashboard/field-activity`)
- **Data sources:** `FieldActivityOverviewResponse` (`/api/dashboard/field-activity/overview`) and
  `DashboardCollectionResponse` (`/api/dashboard/collection`, via `dashboardStore.loadCollection()`)
- **Scope constraint:** Frontend-only. No backend, DB, ETL, KPI, or external-data changes.

---

## Planning Authority

```text
FEASIBILITY ASSESSMENT
```

The feasibility report is authoritative. This plan introduces no new business
decisions and no new architecture decisions. All thresholds, formulas,
time-window rules, join keys, and missing-data rules are taken verbatim from the
two authoritative documents and the resolved gaps (GAP-001 … GAP-008).

---

## Scope Summary

Evolve the Sales Force Overview into a "Commercial Field Force Effectiveness"
dashboard by frontend composition of the two existing APIs. The proven 5-section
decision flow is preserved; collection signals are surfaced in a governed,
sales-manager-action-oriented way without becoming a second Collection Dashboard.

**In scope:**

1. **Collection Health (MTD)** — a dedicated section (GAP-005) holding Cash
   Collected MTD, Recovery vs Billing %, and a conditional overdue alert chip.
   Collection KPIs never share a KPI group with sales KPIs (different reporting
   windows: single `visitDate` flow vs MTD stock).
2. **Territory Financial Health** — extend `FieldActivityWilayahChart` with a
   two-group toggle (Sales Activity / Financial Health) plus a C.3 Overdue
   Distribution view. Financial Health shows **Overdue Exposure / Overdue
   Concentration / Aging Risk only** (GAP-002).
3. **Grouped Action Center** — replace `FieldActivityRankingGrid` with three
   groups: Field Execution (4 categories), Commercial Risk (Needs Collection
   Action / Needs Credit Review / Needs Escalation), Recognition (candidates).
4. **Derived cross-dashboard signals** — frontend classification using the
   **Commercial Signal Rule Catalog v1** (GAP-003B) joined on **`SalesPersonId`**
   (GAP-003A). Signals are classifications, not new KPIs.
5. **Graceful degradation** — collection widgets render empty/unavailable states
   when `DashboardCollectionResponse.IsAvailable` is false or the call fails
   (GAP-004).
6. **Drill-across** — collection signals navigate to the existing **Piutang
   Dashboard** (business/UX label "Collection Dashboard") via
   `Navigation.PiutangDashboardRoute` (GAP-006).

**Explicitly out of scope (from the feasibility):**

- **GAP-001** — collection trend widgets (Cash Collected, Recovery vs Billing %,
  Overdue Exposure 7/30-day trends). Deferred; no time-series source.
- **GAP-002** — Territory "Collection Amount by Territory" measure. Removed; no
  territory-level collection metric exists.
- **GAP-007** — "per Active Salesman" derived metrics (Revenue per Active
  Salesman, Cash Collected per Active Salesman) are **not** implemented, per the
  feasibility's optional-removal recommendation. Active Salesmen uses the source
  `TeamKpis.ActiveSalesmenCount` everywhere; the dashboard never redefines it as
  `ActualVisits > 0`.

**Governance (§5.4):** collection values are read **as-is** from
`DashboardCollectionResponse` and never re-computed or re-derived.

**Documented assumptions (from the feasibility's stated frontend-only scope):**

- The "Collection Health (MTD)" time-context label derives from the global
  `presentationStore.businessReferenceDate`; **no** dedicated collection
  business-date field is added to the backend (GAP-004 "rely on the global
  business date" option).

---

## Impact Inventory

### Backend

- **None.** No aggregate, entity, repository, service, command, or query change.
  No merged endpoint is created (GAP-004 frontend composition).

### Database

- **None.** No table, view, index, or constraint change. No collection trend
  table (GAP-001 deferred).

### Frontend

- **Modified:** `views/dashboard/FieldActivityOverviewView.vue` — recompose into
  the 5-section hierarchy; call `dashboardStore.loadCollection()`; wire drill-across.
- **Modified:** `components/field-activity/FieldActivityWilayahChart.vue` — add
  Financial Health measure group (two-group toggle) + C.3 Overdue Distribution.
- **Modified (replaced):** `components/field-activity/FieldActivityRankingGrid.vue`
  → superseded by the grouped Action Center; obsolete sales-leaderboard grid removed.
- **Unchanged (sales-only):** `FieldActivityTeamKpiStrip.vue` (no collection KPIs,
  per GAP-005), `FieldActivityTeamTrendChart.vue` (collection trends deferred,
  GAP-001), `FieldActivitySalesmanTable.vue`, `FieldActivityComparisonChart.vue`.
- **New services (pure, unit-tested):**
  - `services/commercialSignalRules.ts` — Commercial Signal Rule Catalog v1
    percentile classifier.
  - `services/fieldActivityCollectionComposition.ts` — `SalesPersonId` join +
    Territory Financial Health + Overdue Distribution + collection action signals.
- **New presentational components** under `components/field-activity/`:
  - `FieldActivityCollectionHealthSection.vue` (Collection Health (MTD) + alert chip
    + graceful degradation).
  - `FieldActivityGroupedActionCenter.vue` (Field Execution / Commercial Risk /
    Recognition).
- **Read-only reuse:** `models/dashboard.ts` (`DashboardCollectionResponse` and
  related types, already modeled); `dashboardStore.collection` /
  `loadCollection()`; `services/formatters.ts`; `services/chartLayout.ts`;
  `services/navigateToInvestigation.ts` (`navigateToInvestigation`,
  `navigateToDashboard`).

### Integration

- **Frontend composition of two existing APIs** (GAP-004). The SFO page calls
  `getFieldActivityOverview(visitDate)` + `dashboardStore.loadCollection()`. No
  merged endpoint; KPI ownership stays in each source dashboard.
- **Drill-across** to Piutang Dashboard via `Navigation.PiutangDashboardRoute`
  (GAP-006). Investigation drill-down via `navigateToInvestigation`.

### Security

- **None new.** Read-only dashboards; no permission/role change; collection data is
  already exposed to the same authenticated users.

---

## Phases

| Phase | Name | Slices |
| --- | --- | --- |
| 1 | Analytics & Composition Foundations | SFO-01, SFO-02 |
| 2 | Collection Health (MTD) Section | SFO-03 |
| 3 | Territory Financial Health & Overdue Distribution | SFO-04 |
| 4 | Grouped Action Center | SFO-05 |
| 5 | Composition & Drill-Across | SFO-06 |
| 6 | Regression & Validation | SFO-07 |

---

## Slices

### SFO-01 — Commercial Signal Rule Catalog classifier

**Objective:** Implement the percentile-based Commercial Signal Rule Catalog v1
(GAP-003B) as pure functions. Signals are **business classifications** over
existing KPIs — no KPI registration, no new metric.

**Dependencies:** None.

**Acceptance Criteria:**
- Input is a normalized salesman row: `SalesPersonId`, `Revenue`, `EffectiveCallRate`
  (0–100), `OverdueExposure`. Missing values are excluded from the relevant
  percentile population, never treated as 0.
- `isHighRevenue(row)` — Revenue in the **Top 25%** of the active population.
- `isHighOverdue(row)` — Overdue Exposure in the **Top 25%** of salesmen with
  overdue data.
- `isHealthyPortfolio(row)` — Overdue Exposure in the **Bottom 25%**.
- `needsCreditReview(row)` — High Revenue **AND** High Overdue.
- `needsCollectionAction(row)` — High Overdue.
- `recognitionCandidates(rows)` — High Revenue **AND** High Effective Call Rate
  **AND** Healthy Portfolio.
- A population with ≤ 3 salesmen does not throw; percentile boundaries degrade to
  the defined rule without inventing thresholds (ties resolved deterministically).
- Signals return a classification structure (signal key + rationale), never a new
  KPI value.
- Unit tests cover: empty population, small population (≤ 3), ties, missing
  revenue/overdue exclusion, and each of the six rules at the 25% boundary.

**Review Focus:** Architecture Compliance (classifications, not KPIs); GAP-003B
percentile rules; deterministic tie handling; no new threshold invented.

---

### SFO-02 — Cross-dashboard composition & collection summaries

**Objective:** Provide the pure helpers that merge the two datasets on the
canonical join key and derive Territory Financial Health, Overdue Distribution,
and collection action inputs.

**Dependencies:** None (does not require classification).

**Acceptance Criteria:**
- `joinSalesmenToCollection(salesmen, topOverdueSalesmen)` joins on
  `salesmen[].SalesPersonId ↔ topOverdueSalesmen[].Investigation.EntityId`
  (GAP-003A). `EntityCode`/`EntityName` are display fallbacks only; salesman
  **name is never the join key**.
- `territoryFinancialHealth(topOverdueWilayah, attentionCards)` returns, per
  Wilayah, **Overdue Exposure** (Amount) and **Overdue Concentration**
  (`PercentOfTotal`), plus a page-level **Aging Risk** summary from
  `AttentionCards.AgingOver90Exposure` / `AgingRiskSummary[]`. It does **not**
  compute "Collection Amount by Territory" (GAP-002).
- `overdueDistribution(topOverdueWilayah)` returns a contribution-style list
  (name + amount + `PercentOfTotal`) for C.3.
- `collectionActionInputs(topOverdueSalesmen, attentionCards)` surfaces
  "Needs Collection Action" (highest overdue by salesman, from
  `TopOverdueSalesmen[]`) and "Needs Escalation" (`AgingOver90Exposure`,
  `LegacyDebtCount`) inputs.
- All values are read **as-is** from `DashboardCollectionResponse`; no
  re-computation (§5.4 governance).
- Unit tests cover: join by `Investigation.EntityId`, salesman without an overdue
  row (no collection signal), empty `TopOverdueWilayah`, and null
  `PercentOfTotal`.

**Review Focus:** GAP-003A join contract; GAP-002 (no territory collections);
§5.4 governance (read as-is, never re-derived).

---

### SFO-03 — Collection Health (MTD) section + conditional alert chip

**Objective:** Render the dedicated "Collection Health (MTD)" section with the two
collection KPIs and the conditional overdue alert chip, degrading gracefully.

**Dependencies:** None (reads `dashboardStore.collection` directly).

**Acceptance Criteria:**
- New `FieldActivityCollectionHealthSection.vue` renders, in its own section
  (never inside the sales KPI group):
  - **Cash Collected MTD** from `AttentionCards.CashCollectedMtd` (or
    `RecoverySummary.CashCollectedMtd`).
  - **Recovery vs Billing %** from `AttentionCards.RecoveryVsBillingPercent`
    (or `RecoverySummary.RecoveryVsBillingPercent`).
- Section carries an explicit **"MTD" time-context label** derived from the global
  business date (no new backend field).
- A conditional **overdue alert chip** renders only when
  `AttentionCards.ExposureRequiresAttention` is `true`; otherwise no chip
  (10-second read stays clean).
- When `collection.IsAvailable` is `false`, or the collection call fails, the
  section renders an empty/unavailable state **without** affecting sales sections
  (GAP-004).
- Values are formatted with existing `formatCurrency` / `formatPercent`; null
  percentages render "No Data", not 0.
- The alert chip exposes a drill-across affordance (wired in SFO-06).

**Review Focus:** GAP-005 (own section, no KPI-group mixing); GAP-004 graceful
degradation; §5.4 (read as-is); conditional chip (not a permanent card).

---

### SFO-04 — Territory Financial Health & Overdue Distribution

**Objective:** Extend `FieldActivityWilayahChart` with a two-group measure toggle
and a C.3 overdue distribution view.

**Dependencies:** SFO-02 (territory/overdue derivations).

**Acceptance Criteria:**
- A **two-group toggle** replaces a single flat measure list:
  - **Group 1 — Sales Activity (default):** Actual Visits, Orders, Revenue,
    Order Conversion Rate (existing sales behavior preserved).
  - **Group 2 — Financial Health:** Overdue Exposure and Overdue Concentration per
    Wilayah (GAP-002 compliant — no "Collection Amount by Territory").
- Financial Health measures carry an explicit **time-context label** ("outstanding
  as of today" / "MTD"), distinguishing stock metrics from flow metrics.
- A C.3 **Overdue Distribution** contribution-style view renders
  `TopOverdueWilayah[]` (EntityName + Amount + PercentOfTotal).
- The default view remains the daily sales view (not diluted by collection).
- Empty/unavailable collection data renders the existing empty state; sales views
  are unaffected (GAP-004).

**Review Focus:** GAP-002 (overdue-family measures only); stock-vs-flow labeling;
default-preserved daily sales view.

---

### SFO-05 — Grouped Action Center (Field Execution / Commercial Risk / Recognition)

**Objective:** Replace `FieldActivityRankingGrid` with the grouped, action-first
Action Center.

**Dependencies:** SFO-01 (classifications), SFO-02 (collection action inputs).

**Acceptance Criteria:**
- New `FieldActivityGroupedActionCenter.vue` renders three groups:
  - **Field Execution:** Needs Coaching (lowest order conversion), Needs Plan
    Review (lowest visit execution), Needs Investigation (highest unplanned + GPS),
    Needs Immediate Attention (zero activity) — sourced from existing
    `FieldActivityOverviewResponse.Rankings` / `Salesmen[]`. GPS "Needs Follow-up"
    is folded into "Needs Investigation" (no separate list).
  - **Commercial Risk:** Needs Collection Action (highest overdue by salesman),
    Needs Credit Review (High Revenue + High Overdue), Needs Escalation
    (Aging >90 / Legacy Debt).
  - **Recognition:** Recognition Candidates (balanced performers) as a **compact
    strip**, never a competing scoreboard.
- Each Commercial Risk and Recognition signal is a classification (SFO-01 /
  SFO-02), not a new KPI.
- Every signal maps to a **sales-manager action** (coach / review / collect /
  escalate), never a finance action.
- Drill-across affordance present on Commercial Risk items (wired in SFO-06).
- The obsolete `FieldActivityRankingGrid` sales-leaderboard grid is removed and no
  dead code remains.

**Review Focus:** Grouped structure (three groups, not one list); action-first
mapping; Recognition kept as a light strip; no finance action labels; obsolete
grid removal.

---

### SFO-06 — Overview recomposition & drill-across

**Objective:** Recompose `FieldActivityOverviewView.vue` into the 5-section
hierarchy, load the collection API, and wire all new sections and drill-across.

**Dependencies:** SFO-03, SFO-04, SFO-05.

**Acceptance Criteria:**
- Page order reflects the 5-section decision flow: Status (sales) → Performance
  (funnel + quality) → Outcomes (revenue distribution + territory + financial
  health) → Action Center → Trends (sales only).
- `loadOverview()` and `dashboardStore.loadCollection()` are both invoked; the
  collection call is non-blocking for the sales sections.
- New sections (`FieldActivityCollectionHealthSection.vue`,
  `FieldActivityGroupedActionCenter.vue`, extended `FieldActivityWilayahChart.vue`)
  are wired with correct props.
- Drill-across: the overdue alert chip and Commercial Risk items navigate to the
  Piutang Dashboard via `navigateToDashboard(router,
  collection.Navigation.PiutangDashboardRoute)` (GAP-006). Where an
  `Investigation` metadata object exists, `navigateToInvestigation` is used.
- Active Salesmen and all sales KPIs use the existing source fields; the
  dashboard never redefines `ActiveSalesmenCount` (GAP-007).
- Sales-only components (`TeamKpiStrip`, `TeamTrendChart`, `SalesmanTable`,
  `ComparisonChart`) remain functionally unchanged.
- Loading/error states for sales use existing `loading`/`loadError`; collection
  section uses its own degraded state.

**Review Focus:** 5-section hierarchy; GAP-006 drill-across target; GAP-007
(source `ActiveSalesmenCount`); preservation of sales-only components; no merged
endpoint.

---

### SFO-07 — Regression & validation pass

**Objective:** Verify no regression and that the dashboard answers the success
criteria without backend change.

**Dependencies:** SFO-06.

**Acceptance Criteria:**
- `npm run build` (vue-tsc + vite) passes; `npm run test` passes (all new
  `*.spec.ts` plus existing suites).
- Manual verification: the blueprint "Success Criteria" questions (selling,
  collecting, which territory sells but doesn't pay, which salesman needs which
  intervention, who deserves recognition) are answerable from the top sections
  within 30 seconds.
- Verify collection KPIs appear **only** in the "Collection Health (MTD)" section
  and never share a KPI group with sales KPIs (GAP-005).
- Verify collection numbers are read as-is (never re-derived); Active Salesmen
  uses `TeamKpis.ActiveSalesmenCount` (GAP-007); Rule Catalog signals are
  classifications (not KPIs); Action Center maps to sales-manager actions.
- Verify graceful degradation when the collection API is unavailable (GAP-004).
- No backend, store (other than `loadCollection()` reuse), or model change.

**Review Focus:** Overall success criteria; governance compliance; no regression
in sales routes/components.

---

## Complexity Analysis

Complexity scale: **1 = trivial**, **2 = simple**, **3 = moderate**, **4 = complex**,
**5 = most complex**.

| Slice | Complexity | Rationale |
| --- | --- | --- |
| SFO-01 | 3 | Six percentile rules, boundary/tie/missing handling; small-population edge cases. |
| SFO-02 | 3 | Join contract + territory/overdue derivations + governance constraints. |
| SFO-03 | 2 | Two KPI cards + conditional chip + degradation state; low logic. |
| SFO-04 | 3 | Two-group toggle on an existing chart + new distribution view; stock/flow labels. |
| SFO-05 | 4 | Three-group action center combining existing rankings + classifications + recognition; grid removal. |
| SFO-06 | 4 | Full-page integration, dual-API composition, drill-across wiring, behavior preservation. |
| SFO-07 | 2 | Build/test execution and manual acceptance verification. |

---

## Progress Tracker

| SLICE-ID | SLICE NAME | COMPLEXITY | STATUS |
| --- | --- | --- | --- |
| SFO-01 | Commercial Signal Rule Catalog classifier | 3 | GO |
| SFO-02 | Cross-dashboard composition & collection summaries | 3 | GO |
| SFO-03 | Collection Health (MTD) section + conditional alert chip | 2 | GO |
| SFO-04 | Territory Financial Health & Overdue Distribution | 3 | GO |
| SFO-05 | Grouped Action Center | 4 | GO |
| SFO-06 | Overview recomposition & drill-across | 4 | GO |
| SFO-07 | Regression & validation pass | 2 | PLANNED |

---

## Progress History

Entry format:

```text
1. [Implementation|Review] [yyyy-MM-dd HH:mm]
   <note of implementation or review>
```

### Slice-ID: SFO-01

```text
1. [Implementation] [2026-09-14 12:39]
   Added services/commercialSignalRules.ts (Commercial Signal Rule Catalog v1):
   percentile classifier over a normalized salesman row (SalesPersonId, Revenue,
   EffectiveCallRate, OverdueExposure) reusing services/principalPercentile.ts.
   Rules: isHighRevenue / isHighOverdue (Top 25%), isHealthyPortfolio (Bottom 25%),
   needsCreditReview (High Revenue AND High Overdue), needsCollectionAction
   (High Overdue), recognitionCandidates (High Revenue AND High Effective Call Rate
   AND Healthy Portfolio); classifyCommercialSignals returns classification
   structures (key + label + rationale), never a KPI value. Missing values excluded
   from the relevant population; ≤3 salesmen degrade to the defined rule without
   inventing thresholds; ties resolved by strict-lower percentile. Added
   services/commercialSignalRules.spec.ts (18 tests: empty, small, ties, missing
   exclusion, six rules at the 25% boundary, classification shape). Build
   (vue-tsc + vite) passes; full suite 45 files / 431 tests pass.
2. [Review] [2026-09-14 12:40]
   GO. All 10 acceptance criteria verified against source and test evidence:
   normalized row + missing-value exclusion, Top-25% High Revenue / High Overdue,
   Bottom-25% Healthy Portfolio, Needs Credit Review, Needs Collection Action,
   Recognition Candidates, ≤3-population degradation without invented thresholds,
   classification structures (key + label + rationale) with no KPI value, and
   boundary/tie/missing/small/empty tests (18). Planning authority (FEASIBILITY
   ASSESSMENT) respected: pure frontend classifier, no backend/DB/KPI change.
   Scope clean (2 new files; no unrelated edits). Build and full suite pass.
   No critical/major findings.
```


### Slice-ID: SFO-02

```text
1. [Implementation] [2026-09-14 12:44]
   Added services/fieldActivityCollectionComposition.ts (pure helpers): 
   joinSalesmenToCollection joins SFO Salesmen[].SalesPersonId to Collection 
   TopOverdueSalesmen[].Investigation.EntityId (GAP-003A) — EntityCode/EntityName 
   are display-only fallbacks and name is never the join key; 
   territoryFinancialHealth returns per-Wilayah Overdue Exposure (Amount) + Overdue 
   Concentration (PercentOfTotal) plus the page-level Aging Risk summary 
   (AttentionCards.AgingOver90Exposure + AgingRiskSummary[]); overdueDistribution 
   returns the C.3 contribution list (name + amount + PercentOfTotal); 
   collectionActionInputs surfaces Needs Collection Action (TopOverdueSalesmen[], 
   sorted by highest overdue) and Needs Escalation 
   (AttentionCards.AgingOver90Exposure / LegacyDebtCount). All values read as-is 
   from DashboardCollectionResponse (§5.4); no Collection Amount by Territory 
   (GAP-002). Added services/fieldActivityCollectionComposition.spec.ts (18 tests: 
   join by Investigation.EntityId, name-not-a-join-key, missing overdue row, 
   duplicate EntityId, empty TopOverdueWilayah, null PercentOfTotal, aging risk, 
   escalation inputs, unavailable AttentionCards). Build (vue-tsc + vite) passes; 
   full suite 46 files / 449 tests pass.
2. [Review] [2026-09-14 12:45]
   GO. All 6 acceptance criteria verified against source and test evidence:
   joinSalesmenToCollection joins on salesmen[].SalesPersonId ↔ 
   TopOverdueSalesmen[].Investigation.EntityId with EntityCode/EntityName used for 
   display only and name never a join key (GAP-003A); territoryFinancialHealth 
   returns per-Wilayah Overdue Exposure (Amount) + Overdue Concentration 
   (PercentOfTotal) with a page-level Aging Risk summary from 
   AttentionCards.AgingOver90Exposure / AgingRiskSummary[] and no Collection Amount 
   by Territory (GAP-002); overdueDistribution returns the C.3 contribution list 
   (name + amount + PercentOfTotal); collectionActionInputs surfaces Needs Collection 
   Action (highest overdue by salesman, from TopOverdueSalesmen[]) and Needs 
   Escalation (AgingOver90Exposure, LegacyDebtCount) inputs; all values read as-is 
   from DashboardCollectionResponse with no re-computation (§5.4); 18 unit tests 
   cover join by Investigation.EntityId, salesman without an overdue row, empty 
   TopOverdueWilayah, null PercentOfTotal (plus name-key, duplicate-EntityId, and 
   unavailable-cards cases). Planning authority (FEASIBILITY ASSESSMENT) respected: 
   pure frontend helpers, no backend/DB/KPI change. Scope clean (2 new files; no 
   unrelated edits). Build and full suite (46 files / 449 tests) pass. No 
   critical/major findings. Note (INFO): territoryFinancialHealth accepts an 
   optional third agingRiskSummary argument because AgingRiskSummary[] is not 
   reachable via AttentionCards — required to satisfy the AC's stated source.
```

### Slice-ID: SFO-03

```text
1. [Implementation] [2026-09-14 12:47]
   Added components/field-activity/FieldActivityCollectionHealthSection.vue: dedicated
   "Collection Health (MTD)" section (own section, never inside the sales KPI group,
   GAP-005) rendering Cash Collected MTD (AttentionCards.CashCollectedMtd, fallback
   RecoverySummary.CashCollectedMtd) and Recovery vs Billing %
   (AttentionCards.RecoveryVsBillingPercent, fallback RecoverySummary). Section carries
   an explicit "MTD · as of <global business date>" time-context label derived from a
   businessDate prop sourced from presentationStore.businessReferenceDate (no new
   backend field). Conditional overdue alert chip renders only when
   AttentionCards.ExposureRequiresAttention === true and uses the collection accent;
   the chip is a clickable drill-across affordance emitting `alertClick` (wired in
   SFO-06). When collection is null, IsAvailable is false, or loading has ended without
   data, the section renders an empty/unavailable state without touching sales sections
   (GAP-004). Values read as-is and formatted with formatCurrency / formatPercent
   (§5.4); null percentages render "No Data", never 0. Props-based presentational
   component (collection, businessDate, loading) per the plan's "wired with correct
   props" (SFO-06). Build (vue-tsc + vite) passes; full suite 46 files / 449 tests pass
   (no new spec: the project has no component-test harness — vitest environment is
   node and no @vue/test-utils).
2. [Review] [2026-09-14 12:48]
   GO. All 6 acceptance criteria verified against source evidence: dedicated
   standalone `<section class="collection-health">` titled "Collection Health (MTD)"
   (never inside the sales KPI group, GAP-005); Cash Collected MTD resolved from
   AttentionCards.CashCollectedMtd with RecoverySummary fallback and Recovery vs
   Billing % from AttentionCards.RecoveryVsBillingPercent with fallback; explicit
   "MTD · as of <global business date>" label derived from the businessDate prop
   (presentationStore.businessReferenceDate; no new backend field); overdue alert
   chip rendered only when AttentionCards.ExposureRequiresAttention === true (else
   absent); unavailable/empty state when collection is null or IsAvailable is false
   (GAP-004) isolated in its own component without touching sales sections; values
   read as-is and formatted with formatCurrency / formatPercent (§5.4) with null
   percentages rendering "No Data"; chip is a clickable drill-across affordance
   emitting `alertClick` (wiring deferred to SFO-06). Planning authority
   (FEASIBILITY ASSESSMENT) respected: frontend-only presentational component, no
   backend/DB/KPI/model change. Scope clean (1 new file; no unrelated edits; view
   wiring intentionally left to SFO-06). Build (vue-tsc + vite) passes; full suite
   46 files / 449 tests pass. No critical/major findings. Note (INFO): the slice's
   plan line "reads dashboardStore.collection directly" is realized as a
   props-based component because the plan also requires SFO-06 to wire sections
   "with correct props"; the ACs do not constrain the data-delivery mechanism.
   Note (INFO): no component-level unit test exists because the project has no
   component-test harness; the slice did not require new tests.
```

### Slice-ID: SFO-04

```text
1. [Implementation] [2026-09-14 12:53]
   Extended components/field-activity/FieldActivityWilayahChart.vue into "Territory
   Performance" with a two-group measure toggle (SelectButton): Group 1 Sales Activity
   (default) — Actual Visits (default), Orders, Revenue, Order Conversion Rate — and
   Group 2 Financial Health — Overdue Exposure, Overdue Concentration. Financial
   Health reads props.topOverdueWilayah as-is (§5.4) and carries the explicit
   stock-vs-flow time-context label "Outstanding as of <global business date>"; it
   exposes only overdue-family measures (GAP-002 — no Collection Amount by Territory).
   Sales measures reuse the existing WilayahBreakdown for Actual Visits (existing sales
   behavior preserved/default daily sales view) and derive Orders/Revenue/Order
   Conversion from props.salesmanRows aggregated per WilayahName (Order Conversion =
   weighted EffectiveCalls/ActualVisits, matching TeamKpis.EffectiveCallRate). A C.3
   Overdue Distribution contribution-style view renders TopOverdueWilayah[]
   (EntityName + Amount + PercentOfTotal) as proportional bars under the Financial
   Health group. Empty/unavailable collection data renders an empty/unavailable state
   (collectionAvailable prop, default true) without affecting the sales view (GAP-004).
   New props are optional (salesmanRows, topOverdueWilayah, collectionAvailable,
   businessDate) so the existing FieldActivityOverviewView call site keeps compiling;
   full props wiring is deferred to SFO-06 per the plan. Build (vue-tsc + vite) passes;
   full suite 46 files / 449 tests pass (no new spec: no component-test harness, and the
   slice required no tests).
2. [Review] [2026-09-14 12:56]
   GO. All 5 acceptance criteria verified against source and build/test evidence:
   (1) two-group SelectButton toggle replaces the flat measure list — Group 1 Sales
   Activity (default) exposes Actual Visits (default), Orders, Revenue, Order
   Conversion, and Group 2 Financial Health exposes Overdue Exposure and Overdue
   Concentration per Wilayah (GAP-002 compliant; no Collection Amount by Territory);
   (2) Financial Health carries the explicit stock-vs-flow label "Outstanding as of
   <business date>" (businessDate prop, "Outstanding as of today" fallback); (3) the
   C.3 Overdue Distribution contribution view renders TopOverdueWilayah[]
   (EntityName + Amount + PercentOfTotal) as proportional bars; (4) the default view
   remains the daily sales (Actual Visits) view — collection content is confined to
   the Financial Health group; (5) empty/unavailable collection renders an
   empty/unavailable state (collectionAvailable prop; default true) in the financial
   group and C.3 area, leaving the sales view unaffected (GAP-004). Planning authority
   (FEASIBILITY ASSESSMENT) respected: frontend-only extend of one existing component,
   collection values read as-is from TopOverdueWilayah (§5.4), no backend/DB/KPI/model
   change. Scope clean (1 modified component + tracker; optional props keep
   FieldActivityOverviewView compiling; SFO-06 wiring explicitly deferred by the plan).
   Build (vue-tsc + vite) passes; full suite 46 files / 449 tests pass. No
   critical/major findings. Note (INFO): Sales Activity Orders/Revenue/Conversion and
   the Financial Health group render empty until SFO-06 supplies salesmanRows /
   topOverdueWilayah — expected per the plan's deferred wiring. Note (INFO): C.3 is
   intentionally nested under the Financial Health group so the default sales view is
   not diluted.
```

### Slice-ID: SFO-05

```text
1. [Implementation] [2026-09-14 13:02]
   Added components/field-activity/FieldActivityGroupedActionCenter.vue: grouped,
   action-first Action Center with three groups (blueprint §6). Field Execution
   sources the existing server rankings — Needs Coaching from
   Rankings.BottomEffectiveCallRate (lowest order conversion), Needs Plan Review
   from Rankings.BottomVisitExecution, Needs Investigation from
   Rankings.MostUnplannedVisits (GPS "Needs Follow-up" folded in as per-row GPS
   valid % context from Salesmen[].GpsValidPercent; no separate list), Needs
   Immediate Attention from Salesmen[].ActualVisits === 0 (zero activity).
   Commercial Risk classifies via the approved pure services: Needs Collection
   Action from collectionActionInputs().needsCollectionAction (TopOverdueSalesmen[]
   highest overdue, sorted), Needs Credit Review from needsCreditReview() over the
   SalesPersonId join (Revenue = OmmenAmount, OverdueExposure = joined overdue),
   Needs Escalation from collectionActionInputs().needsEscalation
   (AgingOver90Exposure / LegacyDebtCount). Recognition renders
   recognitionCandidates() as a compact chip strip, never a scoreboard. Every
   signal carries a sales-manager action label (coach / review / investigate /
   contact / collect / review credit / escalate); no finance action labels.
   Commercial Risk and Recognition names are drill-across affordances emitting
   commercialRiskClick (signalKey + salesPersonId + investigation) and
   recognitionClick (wired in SFO-06); Field Execution items emit salesmanClick.
   Collection-derived groups show an unavailable state when
   collection.IsAvailable is false/null (GAP-004). Removed the obsolete
   components/field-activity/FieldActivityRankingGrid.vue and updated
   FieldActivityOverviewView.vue to render the new component (minimal compile
   fix; :salesmen / :rankings / :loading wired, :collection and full drill-across
   wiring intentionally deferred to SFO-06 per the plan). Build (vue-tsc + vite)
   passes; full suite 46 files / 449 tests pass (no new spec: no component-test
   harness and the slice required no tests; the SFO-01/SFO-02 services already
   carry the classification/join unit tests).
2. [Review] [2026-09-14 13:05]
   GO. All 5 acceptance criteria verified against source and build/test evidence:
   (1) FieldActivityGroupedActionCenter.vue renders three groups — Field Execution
   (Needs Coaching from Rankings.BottomEffectiveCallRate = lowest order conversion,
   Needs Plan Review from Rankings.BottomVisitExecution, Needs Investigation from
   Rankings.MostUnplannedVisits with GPS folded in as per-row GpsValidPercent
   context and no separate "Needs Follow-up" list, Needs Immediate Attention from
   Salesmen[].ActualVisits === 0), Commercial Risk (Needs Collection Action from
   collectionActionInputs().needsCollectionAction, Needs Credit Review from
   needsCreditReview(), Needs Escalation from collectionActionInputs().needsEscalation),
   and Recognition (recognitionCandidates() as a compact chip strip, not a
   scoreboard); Field Execution items are sourced from the existing Rankings /
   Salesmen[] as specified. (2) Commercial Risk and Recognition signals are
   SFO-01/SFO-02 classifications (pure services), not new KPI values. (3) Every
   category carries a sales-manager action label (coach / review / investigate /
   contact / collect / review credit / escalate); no finance action labels. (4)
   Commercial Risk items are buttons emitting commercialRiskClick (signalKey +
   salesPersonId + investigation) and Recognition chips emit recognitionClick —
   the drill-across affordance is present and its wiring is deferred to SFO-06 per
   the plan. (5) FieldActivityRankingGrid.vue is deleted, no references remain and
   no dead code is left; FieldActivityOverviewView.vue is updated as the mandatory
   compile fix. Planning authority (FEASIBILITY ASSESSMENT) respected:
   frontend-only, collection values read as-is via the SFO-02 join (§5.4), no
   backend/DB/model/KPI change, collection-derived groups degrade to an
   unavailable state when IsAvailable is false/null (GAP-004). Scope clean (1 new
   component, 1 removed component, minimal view edit + tracker; no unrelated
   changes). Build (vue-tsc + vite) passes; full suite 46 files / 449 tests pass.
   No critical/major findings. Note (INFO): the view's :collection prop and full
   drill-across wiring are intentionally deferred to SFO-06 per the plan; during
   SFO-05 the Commercial Risk group renders its unavailable state. Note (INFO):
   Needs Investigation folds GPS as row context (no separate list), consistent
   with the slice AC and v2.2 §6.1. Note (INFO): no component-level unit test
   exists because the project has no component-test harness; the slice required
   no tests and the classifier/join are covered by the SFO-01/SFO-02 specs.
```

### Slice-ID: SFO-06

```text
1. [Implementation] [2026-09-14 13:05]
   Recomposed views/dashboard/FieldActivityOverviewView.vue into the 5-section
   decision flow: Status (FieldActivityTeamKpiStrip + the dedicated
   FieldActivityCollectionHealthSection) → Performance (Visit Execution %,
   Effective Call Rate, Orders Generated comparison charts) → Outcomes (Order
   Value / revenue-distribution chart + FieldActivitySalesmanTable + extended
   FieldActivityWilayahChart) → Action Center (FieldActivityGroupedActionCenter)
   → Trends (sales-only FieldActivityTeamTrendChart). Each top-level section is a
   <section> wrapper; Collection Health remains its own component (never inside
   the sales KPI group, GAP-005). Added useDashboardStore and invoked
   dashboardStore.loadCollection() in onMounted alongside the existing
   loadOverview() (non-blocking: both are fire-and-forget), and on Refresh.
   Wired the new sections with correct props: Collection Health gets
   :collection=dashboard.collection, :business-date=presentation.businessReferenceDate,
   :loading=dashboard.loading, @alert-click; WilayahChart gets :salesman-rows,
   :top-overdue-wilayah=dashboard.collection.TopOverdueWilayah,
   :collection-available=dashboard.collection.IsAvailable === true, :business-date;
   GroupedActionCenter gets :collection=dashboard.collection plus
   @commercial-risk-click. Drill-across (GAP-006): onCollectionAlertClick and
   onCommercialRiskClick navigate to the Piutang Dashboard via
   navigateToDashboard(router, collection.Navigation.PiutangDashboardRoute); where
   the Commercial Risk payload carries Investigation metadata,
   navigateToInvestigation(router, investigation, 'Sales Force Overview') is used.
   Sales KPIs/Active Salesmen remain the existing source fields (GAP-007, no
   ActiveSalesmenCount redefinition); sales-only components are unchanged. Only
   FieldActivityOverviewView.vue was modified. Build (vue-tsc + vite) passes; full
   suite 46 files / 449 tests pass (no new spec: no component-test harness and the
   slice required none; the composed pure services already carry SFO-01/SFO-02
   tests).
2. [Review] [2026-09-14 13:07]
   GO. All 7 acceptance criteria verified against source and build/test evidence:
   (1) Page order reflects the 5-section decision flow — exactly five top-level
   <section> wrappers in order: Status (aria-label="Status", TeamKpiStrip +
   CollectionHealthSection), Performance (Visit Execution %, Effective Call Rate,
   Orders Generated comparison charts), Outcomes (Order Value / revenue
   distribution chart + SalesmanTable + WilayahChart), Action Center
   (GroupedActionCenter), Trends (TeamTrendChart); the Action Center and Trends
   sections are correctly after Outcomes and Trends is last.
   (2) loadOverview() and dashboardStore.loadCollection() are both invoked —
   onMounted calls applyDatePreset('today') (watch → loadOverview) and
   void dashboard.loadCollection(); the collection call is fire-and-forget and is
   not awaited, so sales sections are never blocked (collection loading is passed
   only to the collection component).
   (3) New sections wired with correct props — CollectionHealthSection
   (:collection, :business-date, :loading, @alert-click), GroupedActionCenter
   (:salesmen, :rankings, :collection, :loading, @salesman-click,
   @commercial-risk-click, @recognition-click), WilayahChart (:items,
   :salesman-rows, :top-overdue-wilayah, :collection-available, :business-date,
   :loading); prop names match the SFO-03/SFO-04/SFO-05 component contracts and
   the project compiles.
   (4) Drill-across (GAP-006) — onCollectionAlertClick and onCommercialRiskClick
   navigate via navigateToDashboard(router, dashboard.collection.Navigation.
   PiutangDashboardRoute); where the Commercial Risk payload carries Investigation
   metadata, navigateToInvestigation(router, payload.investigation, 'Sales Force
   Overview') is used, matching the AC's precedence rule.
   (5) Active Salesmen and all sales KPIs remain the existing source fields —
   TeamKpiStrip is unchanged and reads kpis.ActiveSalesmenCount; the view never
   redefines ActiveSalesmenCount as ActualVisits > 0 (GAP-007).
   (6) Sales-only components remain functionally unchanged — git status shows only
   FieldActivityOverviewView.vue and the tracker modified; TeamKpiStrip,
   TeamTrendChart, SalesmanTable and ComparisonChart are untouched and receive the
   same props/events as before.
   (7) Sales loading/error use the existing loading/loadError (Message + component
   loading props); the collection section degrades on its own (dashboard.loading +
   the component's IsAvailable/null unavailable state) without touching sales
   sections (GAP-004). Planning authority (FEASIBILITY ASSESSMENT) respected:
   frontend-only, no backend/DB/model change; collection consumed read-only via
   the existing dashboardStore.loadCollection() and Navigation.PiutangDashboardRoute
   (§5.4, GAP-006). Scope clean (1 modified view + tracker; no unrelated edits).
   Build (vue-tsc + vite) passes; full suite 46 files / 449 tests pass.
   No critical/major findings. Note (INFO): the Investigation source label is a
   view-local literal ('Sales Force Overview') rather than
   resolveInvestigationSourceLabel('/dashboard/field-activity'); adding that map
   entry is outside the plan's file list and the AC does not constrain the label,
   so the literal was used. Note (INFO): Refresh now reloads both the sales
   overview and the collection API as a natural consequence of composition; the
   ACs require both calls and do not forbid the refresh wiring. Note (INFO): the
   Outcomes group uses the per-salesman Order Value chart as "revenue distribution"
   and the SalesmanTable as the distribution detail, with WilayahChart providing
   territory + financial health.
```

### Slice-ID: SFO-07
*(pending)*
