# IMPLEMENTATION PLAN — Principal Performance Dashboard Redesign (V2)

- **Planning Authority:** FEASIBILITY ASSESSMENT (Mode B — Feasibility-Driven Planning)
- **Supporting authority:** UX-BLUEPRINT.md (approved UX design)
- **Backing contract:** `PrincipalPerformanceResponse` (`GetPrincipalPerformanceQuery.cs`)
- **Controller:** `PrincipalPerformanceDashboardController.Get()`
- **Current view:** `PrincipalPerformanceDashboardView.vue`
- **Scope constraint:** Frontend-only. No backend, DB, ETL, KPI, or external-data changes.

---

## Planning Authority

```text
FEASIBILITY ASSESSMENT
```

Authoritative inputs:

- `docs/work/btr-portal/principal-performance-dashboard-redesign/FEASIBILITY-ASSESSMENT.md`
- `docs/work/btr-portal/principal-performance-dashboard-redesign/UX-BLUEPRINT.md`

The feasibility report is authoritative. This plan introduces no new business
decisions and no new architecture decisions. All thresholds, formulas, and
missing-data rules are taken verbatim from the two authoritative documents.

---

## Scope Summary

Replace the table-oriented Principal Performance Dashboard (`SA04`,
`/dashboard/principal-performance`) with an insight-oriented dashboard of seven
sections using only the data already present in
`GET api/dashboard/principal-performance`:

1. **Portfolio Overview** — Total Sales, Total Target, Overall Achievement %,
   Portfolio Coverage % (`SUM(ActiveCustomerCount)/SUM(TotalCustomerCount)`),
   Portfolio Return % (`response.ReturnPercentage` as-is), and a
   "Principals with coverage data" count. Total Active Customers and
   "Average Return %" are **not** displayed (GAP-001, GAP-003).
2. **Opportunity & Risk Board** — percentile/ranking rules (GAP-006), max 5
   opportunities and 5 risks, prioritization only, no independent alerting
   (OQ-9).
3. **Achievement Gap Leaderboard** — ranked horizontal bar list sorted by governed
   `AchievementAmount (PRN-TGT-002)`; no derived gap arithmetic (OQ-6).
4. **Coverage & Reach Analysis** — coverage-vs-achievement scatter plot with a
   supporting table and opportunity highlight (Coverage P>=70 AND Achievement
   P<=40).
5. **Return Risk Analysis** — ranked bar chart (by amount / by %) with return-risk
   flag at Return Percentile >= 80 (GAP-006); returns remain independent of
   Sales-Out (GR-001).
6. **Salesman Dependency Analysis** — per-Principal top contributor, normalized
   Contribution %, contribution distribution, DependencyRatio, DependencyRank,
   ContributingSalesmanCount (metadata only, GAP-008), and optional
   ContributionCoverage. No Low/Medium/High bands (GAP-004); no reconciliation
   (GAP-005).
7. **Principal Detail Table** — full projection, sortable/filterable/searchable,
   drill-down to existing analytics screens, null-safe rendering.

Plus a page-level **Data Completeness indicator** (OQ-8) and a single
"Data Updated At" from `response.GeneratedAt` (GAP-007).

All missing values render as **"No Data"**, never 0 / empty / dash (OQ-8).
Unknown is never treated as zero.

---

## Impact Inventory

### Backend

- **None.** No aggregate, entity, repository, service, command, or query change.

### Database

- **None.** No table, view, stored procedure, index, or constraint change.

### Frontend

- **Modified:** `views/dashboard/PrincipalPerformanceDashboardView.vue`
  (restructured into seven sections + header + data-completeness indicator).
- **Extended:** `services/principalContribution.ts` (add normalized-share and
  dependency-grouping helpers; keep `contributionPercentage` for
  `ContributionCoverage` reuse).
- **Read-only reuse:** `models/dashboard.ts` types (no contract change);
  `services/formatters.ts` (`formatCurrency`, `formatCurrencyCompact`,
  `formatNumber`, `formatPercent`, `formatDateTime`); `services/chartLayout.ts`
  (`createChartOptions`, `chartLegend`).
- **New services (pure, unit-tested):**
  - `services/principalPercentile.ts` — percentile-rank computation.
  - `services/principalPortfolio.ts` — portfolio aggregate derivations.
  - `services/principalOpportunityRisk.ts` — opportunity/risk rule engine.
  - `services/principalDataCompleteness.ts` — page-level completeness summary.
  - a shared "No Data" presentation helper.
- **New presentational components** under `components/dashboard/`:
  - portfolio KPI tiles, opportunity/risk cards, achievement-gap bar list,
    coverage scatter + supporting table, return-risk chart + supporting table,
    salesman-dependency stacked bars + metrics table, principal detail table.
- **Preserved (unchanged):** `PRN-SALES-001` display values, `Disclosures[]`
  rendering, evidence routes, and supporting-ranking behavior.

### Integration

- **None.** No new API, event, or external system.

### Security

- **None new.** Existing `[Authorize]` unchanged; no new data exposure.

---

## Phases

| Phase | Name | Slices |
| --- | --- | --- |
| 1 | Analytics Foundations | PPD-01 .. PPD-06 |
| 2 | Header & Portfolio Overview | PPD-07, PPD-08 |
| 3 | Opportunity & Risk Board | PPD-09 |
| 4 | Achievement Gap Leaderboard | PPD-10 |
| 5 | Coverage & Reach Analysis | PPD-11 |
| 6 | Return Risk Analysis | PPD-12 |
| 7 | Salesman Dependency Analysis | PPD-13 |
| 8 | Principal Detail Table | PPD-14 |
| 9 | Composition & Preservation | PPD-15, PPD-16 |

---

## Slices

### PPD-01 — Percentile-rank helper

**Objective:** Provide a pure percentile-rank function over the ranked Principal
population, producing the "Percentile(p, metric)" value used by all relative
thresholds.

**Dependencies:** None.

**Acceptance Criteria:**
- `percentileRank(values, target)` returns `100 * (count of values strictly
  lower than target) / populationSize`, in percent units.
- Null/undefined metric values are excluded from the population; a Principal
  whose metric is null has **no** percentile (undefined), not 0.
- A single-Principal population does not throw; it returns the defined value
  (a lone Principal can be the 100th percentile).
- The population size is returned alongside the percentile so the UI can display
  it (feasibility "Display the population size").
- Unit tests (`*.spec.ts`) cover: empty population, single element, null
  exclusion, ties, and the population-size return.

**Review Focus:** Architecture Compliance; Persistence Compliance (no backend);
correct exclusion of missing values.

---

### PPD-02 — Salesman dependency analytics

**Objective:** Extend `services/principalContribution.ts` with the client-side
dependency derivations over `SalesmanContributions[]` and ranking amounts.

**Dependencies:** PPD-01 (not required for grouping; no strict dependency).

**Acceptance Criteria:**
- `contributionShare(amount, denominator)` returns
  `amount / SUM(ContributionAmount for Principal)` in percent units; denominator
  is `SUM(ContributionAmount)` for that Principal (analytical distribution
  model), **not** `PrincipalSalesOutAmount` (GAP-005, OQ-7).
- `contributionCoverage` reuses existing `contributionPercentage` =
  `SUM(ContributionAmount)/PrincipalSalesOutAmount`.
- Per Principal: `DependencyRatio = MAX over salesmen of share`;
  `ContributingSalesmanCount = COUNT(DISTINCT SalesPersonId)` (metadata only);
  top contributor (name + share); ordered distribution of shares.
- `DependencyRank = RANK by DependencyRatio DESC` across Principals.
- A Principal with no contribution rows has no dependency data (renders "No
  Data"), not 0% (OQ-8).
- No Low/Medium/High band classification is introduced (GAP-004).
- `ContributingSalesmanCount` never feeds ranking, classification, alerts, or
  insights (GAP-008).
- Unit tests cover grouping, denominator, ratio, rank ordering, and the
  no-contribution case.

**Review Focus:** Workflow Compliance (GAP-004, GAP-005, GAP-008); no
reconciliation implied; no band classification.

---

### PPD-03 — Portfolio overview aggregates

**Objective:** Derive portfolio-level aggregates from existing response fields.

**Dependencies:** None.

**Acceptance Criteria:**
- `Portfolio Coverage % = SUM(Ranking[].ActiveCustomerCount) /
  SUM(Ranking[].TotalCustomerCount)` (GAP-002), computed only over Principals
  with **both** `ActiveCustomerCount` and `TotalCustomerCount` non-null (never
  zero-filled).
- Returns the "Principals with coverage data" count (effective population) and
  excludes the same population from both sums.
- `Portfolio Return % = response.ReturnPercentage` (passed through, no
  calculation) (GAP-003).
- Total Sales = `PrincipalSalesOutAmount`; Total Target =
  `PrincipalTargetAmount` (nullable); Overall Achievement % =
  `AchievementPercentage` (nullable).
- **Total Active Customers is not computed** (GAP-001).
- Unit tests cover: empty coverage population, partial coverage population,
  weighted aggregation correctness, and null handling.

**Review Focus:** Workflow Compliance (GAP-001/002/003); weighted (not
simple-average) aggregation; zero-vs-missing distinction.

---

### PPD-04 — Opportunity & Risk rule engine

**Objective:** Implement the percentile-based rules for the Opportunity & Risk
Board (GAP-006).

**Dependencies:** PPD-01 (percentile rank).

**Acceptance Criteria:**
- Opportunity = Coverage Percentile >= 70 AND Achievement Percentile <= 40.
- Return Risk = Return Percentile >= 80 (by `ReturnPercentage` /
  `TotalReturnAmount`).
- Dependency Risk = Dependency Percentile >= 90 (by `DependencyRatio`).
- Underperforming = Achievement Percentile <= 25.
- A Principal missing the underlying metric is **excluded** from that rule
  (never assumed low/high) (OQ-8).
- Rules are pure functions returning, for each matching Principal: name,
  SupplierId, rule id, supporting metric, percentile, and population size.
- The engine produces **no** alert records and introduces **no** independent
  alert thresholds; it is prioritization only (OQ-9).
- Unit tests cover every threshold boundary, missing-metric exclusion, and
  coexistence (same Principal in multiple rules).

**Review Focus:** Architecture Compliance; no alerting engine; relative (not
fixed-constant) thresholds.

---

### PPD-05 — Data Completeness summary

**Objective:** Produce a single page-level data-completeness summary from the
existing availability flags, exception counts, and non-null row counts.

**Dependencies:** None.

**Acceptance Criteria:**
- Summarizes presence/absence for target, returns, growth, coverage, and
  contribution from `*IsAvailable` fields, `*ExceptionCount` fields, and
  non-null ranking row counts.
- Output is a small keyed structure the header can render (e.g. Coverage %,
  Target %, Return %, Contribution % of the ranked population).
- Unknown is never treated as zero; "not available" is expressed as absent, not
  0%.
- Unit tests cover: fully available, partially available, and unavailable
  populations.

**Review Focus:** Workflow Compliance (OQ-8, GAP-007); never zero-fills missing
snapshots.

---

### PPD-06 — Shared "No Data" presentation helper

**Objective:** Provide a single shared rendering rule for missing values.

**Dependencies:** None.

**Acceptance Criteria:**
- A shared constant/helper renders `null`/`undefined` metric values as the
  string `"No Data"` (never `0`, empty string, or a lone dash) (OQ-8, blueprint
  §14).
- The existing `formatPercent`/`DashboardMetric` `'—'` behavior for the
  **unchanged** `PRN-SALES-001` Sales-Out KPI and its existing metrics is not
  repurposed; the new helper is used only by new sections.
- Unit tests cover null, undefined, zero, and non-null passthrough.

**Review Focus:** Workflow Compliance (OQ-8); preservation of unchanged
PRN-SALES-001 displays.

---

### PPD-07 — Header: period, data updated at, data completeness

**Objective:** Replace the page header meta with Period, Data Updated At, and a
Data Completeness indicator.

**Dependencies:** PPD-05, PPD-06.

**Acceptance Criteria:**
- Header shows the reporting period label and "Data Updated At" from
  `response.GeneratedAt` (GAP-007), reusing `DashboardDetailLayout` meta or an
  equivalent header block.
- A Data Completeness indicator renders the PPD-05 summary (blueprint §4
  example: Coverage/Target/Return/Contribution percentages).
- No per-KPI freshness/lineage metadata is shown (GAP-007).
- Unit test where applicable (completeness rendering is covered by PPD-05).

**Review Focus:** Workflow Compliance (GAP-007, OQ-8); page-level only (no
per-snapshot dates).

---

### PPD-08 — Portfolio Overview KPI cards

**Objective:** Render the five Portfolio Overview tiles per blueprint §5.

**Dependencies:** PPD-03, PPD-06.

**Acceptance Criteria:**
- Five cards: Total Sales, Total Target, Achievement % (primary, largest
  emphasis), Portfolio Coverage %, Portfolio Return %.
- Nullable tiles render "No Data" when unavailable.
- Coverage tile states the effective "Principals with coverage data" population.
- Portfolio Return % is labeled as a portfolio-level quality ratio; Return is
  visually treated as "higher is worse" (blueprint §13).
- A portfolio-level achievement label is used (avoiding row-sum confusion).
- Total Active Customers and "Average Return %" are absent (GAP-001, GAP-003).

**Review Focus:** Workflow Compliance (GAP-001/002/003); color semantics; no
double-counted customer total.

---

### PPD-09 — Opportunity & Risk Board cards

**Objective:** Render the two-column Opportunity/Risk board (blueprint §6).

**Dependencies:** PPD-04, PPD-06.

**Acceptance Criteria:**
- Two columns: Opportunities (left) and Risks (right), directly below Portfolio
  Overview.
- Max 5 opportunities and 5 risks; highest-priority first.
- Every card shows: Principal name, category, supporting KPI, percentile, and a
  human interpretation of why it exists (no hidden logic).
- Return-risk and dependency-risk cards are grouped on the Risk side;
  opportunity and underperforming appear on the appropriate side per rule.
- The board renders no alert and introduces no independent thresholds (OQ-9).
- Population size is visible (feasibility "Display the population size").

**Review Focus:** Architecture Compliance (no second alert engine); auditability
(rule + inputs visible); coexistence handled as prioritization, not alerting.

---

### PPD-10 — Achievement Gap Leaderboard

**Objective:** Render the ranked horizontal bar list (blueprint §7).

**Dependencies:** PPD-06.

**Acceptance Criteria:**
- Ranked bar list (not a table), sorted **descending by**
  `Ranking[].AchievementAmount (PRN-TGT-002)`.
- Shows Rank, Principal, Gap Amount, Achievement %, Sales, Target.
- The gap is **not** recalculated as `Target - Sales` (OQ-6); governed
  `PRN-TGT-002` is consumed as-is.
- Null `AchievementAmount` rows are handled explicitly and sort stably without
  being rendered as 0.
- Top 3 items receive stronger visual prominence.
- Achievement % is scaled consistently (stored ratio -> percent) using existing
  `formatPercent`.

**Review Focus:** Workflow Compliance (OQ-6 — dashboards consume KPI outputs);
null sort ordering; no derived gap arithmetic.

---

### PPD-11 — Coverage & Reach Analysis

**Objective:** Render the coverage-vs-achievement scatter plot and supporting
table (blueprint §8).

**Dependencies:** PPD-01, PPD-06.

**Acceptance Criteria:**
- Scatter plot: X = Coverage %, Y = Achievement % (chart.js `scatter`).
- Auto-highlight opportunity candidates (Coverage P>=70 AND Achievement P<=40).
- Supporting table below the chart: Principal, Active Customer, Total Customer,
  Coverage %, Achievement %.
- Null coverage/achievement renders "No Data" and is excluded from the scatter
  and from percentile computation.
- Coverage % uses stored `Ranking[].CoveragePercentage`, scaled consistently.

**Review Focus:** Workflow Compliance (GAP-006); zero-vs-missing; population
skew disclosure where relevant.

---

### PPD-12 — Return Risk Analysis

**Objective:** Render the return-risk ranked bar chart and supporting table
(blueprint §9).

**Dependencies:** PPD-01, PPD-06.

**Acceptance Criteria:**
- Ranked bar chart (not pie/donut) with sort toggle by Return Amount and by
  Return %.
- Highlight Principals with Return Percentile >= 80 (GAP-006).
- Supporting table: Principal, Return Amount, Return %, Good Return, Broken
  Return.
- Returns remain independent of Sales-Out; Return % is labeled a quality ratio,
  not a deduction and not Net Sales (GR-001).
- Null return snapshots render "No Data" and are excluded from percentiles and
  aggregates (OQ-8).

**Review Focus:** Semantic protection (GR-001); independent KPI families; no
return deducted from Sales-Out.

---

### PPD-13 — Salesman Dependency Analysis

**Objective:** Render the 100% stacked contribution bars and per-Principal
dependency metrics (blueprint §10).

**Dependencies:** PPD-02, PPD-06.

**Acceptance Criteria:**
- One row per Principal with 100% stacked contribution bars.
- Displays: Principal, Top Contributor, Contribution %, Dependency Ratio,
  Dependency Rank, Contributing Salesman Count, Contribution Coverage.
- Sort by Dependency Ratio descending.
- Contribution % uses normalized `SUM(ContributionAmount)` denominator; Coverage
  uses `SUM(ContributionAmount)/PrincipalSalesOutAmount`.
- Principals with no contribution snapshot render "No Data", not 0%.
- Contributing Salesman Count is presented as supplementary metadata only
  (GAP-008).
- No Low/Medium/High bands; no reconciliation claim (GAP-004, GAP-005).

**Review Focus:** Workflow Compliance (GAP-004/005/008); denominator correctness;
distribution vs reconciliation framing.

---

### PPD-14 — Principal Detail Table

**Objective:** Render the full Principal projection table with sorting, filtering,
search, and drill-down (blueprint §11).

**Dependencies:** PPD-02 (for Dependency Rank column), PPD-06.

**Acceptance Criteria:**
- Default columns: Principal, Sales, Target, Achievement %, Achievement Gap,
  Coverage %, Return %, MoM Growth, YoY Growth, Dependency Rank.
- Sortable, filterable, and searchable.
- Clicking a Principal navigates to existing downstream analytics/evidence/
  supporting-ranking routes (blueprint §15).
- Null columns render "No Data", never 0 or blank.
- Percent columns use consistent stored-ratio scaling; currency columns use
  `formatCurrency`.

**Review Focus:** Null rendering; percent scaling consistency; navigation
preservation.

---

### PPD-15 — Dashboard composition & preservation

**Objective:** Assemble the new sections into `PrincipalPerformanceDashboardView.vue`
in the blueprint's information hierarchy while preserving existing unchanged
behaviors.

**Dependencies:** PPD-07 through PPD-14.

**Acceptance Criteria:**
- Page order matches blueprint §2/§3: Header → Portfolio Overview → Opportunity
  & Risk Board → Achievement Gap Leaderboard + Coverage & Reach (side-by-side)
  → Return Risk + Salesman Dependency (side-by-side) → Principal Detail Table.
- `PRN-SALES-001` display values, `Disclosures[]` rendering, evidence routes
  (return evidence, sales-out evidence), and supporting-ranking behavior remain
  functional and unchanged.
- The obsolete table sections (target/return/growth/customer-reach/contribution
  tables) are replaced by the new sections; no dead code remains.
- Loading and error states use the existing `dashboard.loading` / `dashboard.error`
  and `DashboardDetailLayout`.
- No backend, store, or model changes introduced.

**Review Focus:** Architecture Compliance; preservation of unchanged measures;
layout hierarchy compliance.

---

### PPD-16 — Regression & validation pass

**Objective:** Verify no regression and that the redesign satisfies the success
criteria without opening the detail table.

**Dependencies:** PPD-15.

**Acceptance Criteria:**
- `npm run build` (vue-tsc + vite) passes; `npm run test` passes (all new
  `*.spec.ts` plus existing suites).
- Manual verification: the six blueprint §16 questions are answerable from the
  top sections within 30 seconds.
- Verify: returns never deducted from Sales-Out; missing values render "No Data"
  and are excluded from percentiles/aggregates; leaderboard consumes governed
  `PRN-TGT-002`; aggregate-vs-row reconciliation is documented in the UI.

**Review Focus:** Overall success criteria; no regression in existing routes or
`PRN-SALES-001` semantics.

---

## Complexity Analysis

Complexity scale: **1 = trivial**, **2 = simple**, **3 = moderate**, **4 = complex**,
**5 = most complex**. Complexity reflects implementation effort, number of edge
cases, and integration/chart risk — not business importance.

| Slice | Complexity | Rationale |
| --- | --- | --- |
| PPD-01 | 2 | Single pure function; edge cases limited to nulls/ties/singletons. |
| PPD-02 | 3 | Grouping, per-Principal normalization, ratio/rank/count plus several edge cases. |
| PPD-03 | 2 | Straightforward sum/weighted ratio with a simple population filter. |
| PPD-04 | 3 | Four percentile rules, boundary handling, missing-metric exclusion, coexistence. |
| PPD-05 | 2 | Aggregation of existing flags/counts; moderate field coverage. |
| PPD-06 | 1 | Single shared label/helper; near-zero logic. |
| PPD-07 | 2 | Header composition reusing existing layout; low logic. |
| PPD-08 | 2 | Five static tiles with two nullable states and population note. |
| PPD-09 | 3 | Two-column card board, prioritization, per-card rule/percentile disclosure. |
| PPD-10 | 3 | Horizontal bar list with governed-KPI sort and explicit null ordering. |
| PPD-11 | 4 | Scatter plot + supporting table + highlight mask + null exclusion. |
| PPD-12 | 3 | Ranked bar chart with sort toggle + supporting table + risk flag. |
| PPD-13 | 5 | Full dependency model plus 100% stacked multi-dataset chart + metrics table + null handling. |
| PPD-14 | 4 | Many columns with sort/filter/search, drill-down, and null-safe rendering. |
| PPD-15 | 4 | Full-page integration, layout hierarchy, deletion of obsolete sections, behavior preservation. |
| PPD-16 | 2 | Build/test execution and manual acceptance verification across the page. |

---

## Progress Tracker

| SLICE-ID | SLICE NAME | COMPLEXITY | STATUS |
| --- | --- | --- | --- |
| PPD-01 | Percentile-rank helper | 2 | GO |
| PPD-02 | Salesman dependency analytics | 3 | GO |
| PPD-03 | Portfolio overview aggregates | 2 | GO |
| PPD-04 | Opportunity & Risk rule engine | 3 | GO |
| PPD-05 | Data Completeness summary | 2 | GO |
| PPD-06 | Shared "No Data" presentation helper | 1 | GO |
| PPD-07 | Header: period, data updated at, data completeness | 2 | GO |
| PPD-08 | Portfolio Overview KPI cards | 2 | GO |
| PPD-09 | Opportunity & Risk Board cards | 3 | GO |
| PPD-10 | Achievement Gap Leaderboard | 3 | IMPLEMENTED |
| PPD-11 | Coverage & Reach Analysis | 4 | PLANNED |
| PPD-12 | Return Risk Analysis | 3 | PLANNED |
| PPD-13 | Salesman Dependency Analysis | 5 | PLANNED |
| PPD-14 | Principal Detail Table | 4 | PLANNED |
| PPD-15 | Dashboard composition & preservation | 4 | PLANNED |
| PPD-16 | Regression & validation pass | 2 | PLANNED |

---

## Progress History

Entry format:

```text
1. [Implementation|Review] [yyyy-MM-dd HH:mm]
   <note of implementation or review>
```

### Slice-ID: PPD-01
1. [Implementation 2026-09-13 20:10]
   Implemented percentileRank in services/principalPercentile.ts with unit tests; status IMPLEMENTED.
2. [Review 2026-09-13 20:12]
   Verified all acceptance criteria, feasibility authority compliance, and scope; no findings; status GO.

### Slice-ID: PPD-02
1. [Implementation 2026-09-13 20:16]
   Extended services/principalContribution.ts with contributionShare, contributionCoverage, and principalDependencies (grouping, normalized share, max-ratio, distinct count, top contributor, ordered distribution, dependency rank); 23 unit tests pass; status IMPLEMENTED.
2. [Review 2026-09-13 20:17]
   Verified all PPD-02 acceptance criteria, feasibility authority compliance, GAP-004/005/008 compliance, and scope; build (vue-tsc) and full suite (368 tests) pass; no findings; status GO.

### Slice-ID: PPD-03
1. [Implementation 2026-09-13 20:27]
   Implemented portfolioCoverage + portfolioOverview in services/principalPortfolio.ts with unit tests; status IMPLEMENTED.
2. [Review 2026-09-13 20:28]
   Verified all PPD-03 acceptance criteria, feasibility authority compliance, GAP-001/002/003 compliance, and scope; build (vue-tsc) and full suite (382 tests) pass; no findings; status GO.

### Slice-ID: PPD-04
1. [Implementation 2026-09-13 20:33]
   Implemented percentile-based opportunity/risk rule engine in services/principalOpportunityRisk.ts (opportunityEntries, returnRiskEntries, dependencyRiskEntries, underperformingEntries, opportunityRiskBoard) with approved thresholds and missing-metric exclusion; 21 unit tests pass; status IMPLEMENTED.
2. [Review 2026-09-13 20:34]
   Verified all PPD-04 acceptance criteria, feasibility authority compliance (relative thresholds, no alerting engine), and scope; boundary/missing/coexistence tests present; build (vue-tsc) and full suite (403 tests) pass; no findings; status GO.

### Slice-ID: PPD-05
1. [Implementation 2026-09-13 20:37]
   Implemented principalDataCompleteness in services/principalDataCompleteness.ts (target/return/growth/coverage/contribution metrics from *IsAvailable flags, *ExceptionCount passthrough, and non-null ranking row counts; null percentage for empty population, zero never fabricated); 5 unit tests pass; status IMPLEMENTED.
2. [Review 2026-09-13 20:40]
   Verified all PPD-05 acceptance criteria, feasibility authority compliance (OQ-8, GAP-007), and scope; build (vue-tsc) and full suite (408 tests) pass; no findings; status GO.

### Slice-ID: PPD-06
1. [Implementation 2026-09-13 20:43]
    Implemented shared No Data helper in services/principalNoData.ts (NO_DATA_LABEL, isMissingValue, formatWithNoData, orNoData) with unit tests for null/undefined/zero/passthrough; existing formatPercent/DashboardMetric '—' behavior untouched; status IMPLEMENTED.
2. [Review 2026-09-13 20:45]
    Verified all PPD-06 acceptance criteria, feasibility authority compliance (OQ-8, blueprint §14), and scope; existing PRN-SALES-001 '—' behavior preserved; build (vue-tsc) and full suite (413 tests) pass; no findings; status GO.

### Slice-ID: PPD-07
1. [Implementation 2026-09-13 20:50]
    Created PrincipalDataCompletenessIndicator.vue (PPD-05 summary + PPD-06 No Data rule) and integrated header meta into PrincipalPerformanceDashboardView.vue (period + GeneratedAt via DashboardDetailLayout + completeness indicator); no per-KPI metadata; build and full suite (413 tests) pass; status IMPLEMENTED.
2. [Review 2026-09-13 20:55]
    Verified all PPD-07 acceptance criteria, feasibility authority compliance (GAP-007 page-level only, OQ-8 No Data), and scope; period + GeneratedAt reuse preserved, completeness renders Coverage/Target/Return/Contribution; build (vue-tsc) and full suite (413 tests) pass; no findings; status GO.

### Slice-ID: PPD-08
1. [Implementation 2026-09-13 21:00]
    Created PrincipalPortfolioOverview.vue (five tiles via PPD-03 portfolioOverview + PPD-06 No Data rule; Portfolio Achievement % primary, coverage population note, return as quality-ratio/higher-is-worse risk treatment; no Total Active Customers / Average Return %) and integrated into PrincipalPerformanceDashboardView.vue after completeness indicator; build (vue-tsc) and full suite (413 tests) pass; status IMPLEMENTED.
2. [Review 2026-09-13 21:05]
    Verified all PPD-08 acceptance criteria, feasibility authority compliance (GAP-001/002/003, OQ-8 No Data, blueprint §5/§13), and scope; five tiles with primary achievement emphasis, coverage population note, return quality-ratio risk treatment, no customer-total/average-return; build (vue-tsc) and full suite (413 tests) pass; no findings; status GO.

### Slice-ID: PPD-09
1. [Implementation 2026-09-13 21:25]
   Created PrincipalOpportunityRiskBoard.vue (two-column Opportunities/Risks board via PPD-04 opportunityRiskBoard + PPD-06 No Data rule; direct dependency grouping through principalDependencies; max 5 per side, severity-priority ordering, per-card category/supporting KPI/percentile/population/interpretation, no alerting, no independent thresholds) and integrated it directly below the Portfolio Overview in PrincipalPerformanceDashboardView.vue; build (vue-tsc) and full suite (413 tests) pass; status IMPLEMENTED.
2. [Review 2026-09-13 21:30]
   Verified all PPD-09 acceptance criteria (two-column left/right placement directly below Portfolio Overview; max 5 per side with severity-priority ordering; principal/category/supporting-KPI/percentile/population/interpretation on every card; opportunity left, return-risk/dependency-risk/underperforming right; no alert records and no independent thresholds; population size visible), feasibility authority compliance (GAP-006 relative percentiles, OQ-9 prioritization-only), and scope (no backend/service/model change); build (vue-tsc) and full suite (413 tests) pass; no critical/major findings; status GO.

### Slice-ID: PPD-10
1. [Implementation 2026-09-13 21:15]
   Created PrincipalAchievementGapLeaderboard.vue (ranked horizontal bar list, not a table) sorted descending by governed Ranking[].AchievementAmount (PRN-TGT-002) consumed as-is (no Target-Sales recalculation); explicit stable null ordering with null rows rendered via PPD-06 "No Data" (never 0), top-3 visual prominence, and consistent stored-ratio scaling via formatPercent; integrated into PrincipalPerformanceDashboardView.vue after the Portfolio Overview; build (vue-tsc) and full suite (413 tests) pass; status IMPLEMENTED.
2. [Review 2026-09-13 21:20]
   Verified all PPD-10 acceptance criteria, feasibility authority compliance (OQ-6 governed PRN-TGT-002, OQ-8 No Data), and scope (no backend/service change); build (vue-tsc) and full suite (413 tests) pass; no findings; status GO.

### Slice-ID: PPD-11
_No entries yet._

### Slice-ID: PPD-12
_No entries yet._

### Slice-ID: PPD-13
_No entries yet._

### Slice-ID: PPD-14
_No entries yet._

### Slice-ID: PPD-15
_No entries yet._

### Slice-ID: PPD-16
_No entries yet._
