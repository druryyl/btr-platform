# FEASIBILITY ASSESSMENT — Principal Performance Dashboard Redesign

Assessment of the proposed redesign of the Principal Performance Dashboard
(`SA04`, `/dashboard/principal-performance`) using only the data already
available in the existing endpoint response.

- **Source endpoint:** `GET api/dashboard/principal-performance`
- **Backing contract:** `PrincipalPerformanceResponse` (`GetPrincipalPerformanceQuery.cs`)
- **Controller:** `PrincipalPerformanceDashboardController.Get()`
- **Current view:** `PrincipalPerformanceDashboardView.vue`
- **Method:** Feasibility Assessment skill (`docs/skills/feasibility-creation-skill.md`)
- **Scope constraint:** No new backend APIs, DB changes, ETL, KPI calculations, or
  external data sources. Endpoint response *may* be revised, but only if needed.

---

# Executive Summary

### Request

Redesign the table-oriented Principal Performance Dashboard into an
insight-oriented dashboard with seven sections: Portfolio Overview,
Achievement Gap Leaderboard, Coverage & Reach Analysis, Return Risk Analysis,
Salesman Dependency Analysis, Opportunity & Risk Board, and a Principal Detail
Table.

### Overall Feasibility Verdict

```text
FULLY FEASIBLE
```

### Rationale

- **All seven sections are feasible with zero backend, DB, ETL, or new KPI
  work.** Every required input is present per-Principal or at response level, or
  is a safe client-side derivation.
- **Closed by decision:** Portfolio "Total Active Customers" is excluded (no
  de-duplicated population; GAP-001). Portfolio "Coverage %" is defined as the
  weighted `SUM(ActiveCustomerCount) / SUM(TotalCustomerCount)` (GAP-002).
  "Average Return %" is dropped in favor of the existing `response.ReturnPercentage`
  as "Portfolio Return %" (GAP-003). Salesman Dependency uses Contribution % +
  `DependencyRank`, no bands (GAP-004), and is an analytical distribution, not
  reconciliation (GAP-005). Opportunity & Risk uses relative percentile rules,
  not fixed constants (GAP-006).
- **Missing-data handling is decided (OQ-8):** missing values display as
  **"No Data"**, aggregates exclude missing snapshots, and a single page-level
  **Data Completeness indicator** replaces per-aggregate footnotes. Unknown is
  never treated as zero.
- **Alerting authority is decided (OQ-9):** EX01/EX02 remain authoritative; the
  Opportunity & Risk Board is prioritization only and adds no independent alert
  thresholds.
- **Residual risks are not feasibility blockers:** coarse percentiles in small
  populations and card coexistence. The response-level aggregate
  `AchievementAmount` is still computed (`salesOut - target`) while rows use
  governed `PRN-TGT-002`; prefer the stored row values for the leaderboard.
  Per-KPI freshness metadata is excluded (GAP-007) — only page-level
  `GeneratedAt` is shown.

Facts, assumptions, and recommendations are separated throughout. All field
claims below are evidenced from the actual response contract.

---

# Current State (Evidence Base)

The endpoint returns a single `PrincipalPerformanceResponse` per period. The
following fields are **confirmed present** (fact):

**Response level** (`PrincipalPerformanceResponse`, lines 23–104):

- `IsAvailable`, `KpiId = PRN-SALES-001`, `KpiName`
- `PeriodYear`, `PeriodMonth`, `GeneratedAt`
- `PrincipalSalesOutAmount` (portfolio total)
- `UnknownPrincipalExceptionCount`
- `TargetKpiId = PRN-TGT-001`, `PrincipalTargetAmount?`
- `AchievementAmountKpiId = PRN-TGT-002`, `AchievementPercentageKpiId = PRN-TGT-003`,
  `AchievementAmount?`, `AchievementPercentage?`, `TargetAchievementIsAvailable`,
  `MissingTargetExceptionCount`
- `GoodReturnAmount? (PRN-RET-001)`, `BrokenReturnAmount? (PRN-RET-002)`,
  `TotalReturnAmount? (PRN-RET-003)`, `ReturnPercentage? (PRN-RET-004)`,
  `ReturnIsAvailable`
- `MomGrowthPercentage? (PRN-GRW-001)`, `YoyGrowthPercentage? (PRN-GRW-002)`,
  `GrowthIsAvailable`
- `ContributionIsAvailable`
- `ActiveCustomerCountKpiId = PRN-CUS-001`, `CustomerCoverageKpiId = PRN-CUS-002`,
  `CustomerReachIsAvailable`
- `Disclosures[]`, `SupportingRankingOptions[]`

**Ranking item level** (`PrincipalPerformanceRankingItem`, lines 127–184): per
Principal — `Rank`, `PrincipalName`, `SupplierId`, `KpiId`,
`PrincipalSalesOutAmount`, `PrincipalTargetAmount?`, `AchievementAmount?`,
`AchievementPercentage?`, `GoodReturnAmount?`, `BrokenReturnAmount?`,
`TotalReturnAmount?`, `ReturnPercentage?`, `MomGrowthPercentage?`,
`YoyGrowthPercentage?`, `ActiveCustomerCount?`, `TotalCustomerCount?`,
`CoveragePercentage?`.

**Salesman contribution level** (`PrincipalSalesmanContributionItem`, lines
106–125): flat list across all ranked Principals — `SupplierId`,
`PrincipalName`, `SalesPersonId`, `SalesPersonCode`, `SalesPersonName`,
`SourceSalesOutKpiId`, `ContributionAmount`, `LineCount`,
`HasTargetResponsibility`.

**Explicitly absent from the response** (fact): portfolio-level Total Active
Customers, portfolio-level weighted/average Coverage %, portfolio-level average
Return %, dependency classification, any time-series/history, per-Principal
customer overlap, and the contribution reconciliation/unattributed amount
(model has `PrincipalSalesmanContributionResult.Exceptions`, but it is **not**
mapped onto the response — see `AttachStoredSalesmanContributions`,
lines 627–684).

---

# Section-by-Section Assessment

## 1. Portfolio Overview

### Available Data

| Display field | Source | Status |
| --- | --- | --- |
| Total Sales | response `PrincipalSalesOutAmount` | Present |
| Total Target | response `PrincipalTargetAmount?` | Present (nullable) |
| Overall Achievement % | response `AchievementPercentage?` | Present (nullable) |
| Total Active Customers | none at response level; only `Ranking[].ActiveCustomerCount?` | **CLOSED — not displayed** (GAP-001; no de-duplicated portfolio population) |
| Portfolio Coverage % | derived from `Ranking[].ActiveCustomerCount?` and `Ranking[].TotalCustomerCount?` | **DEFINED — `SUM(Active)/SUM(Total)`** (GAP-002 closed) |
| Portfolio Return % | response `ReturnPercentage?` | **DEFINED — display as-is** (GAP-003 closed); "Average Return %" not displayed |

### Derived Metrics

- ~~`Total Active Customers = SUM(Ranking[].ActiveCustomerCount)`~~ — **removed**.
  Decision: do not display. The endpoint has no de-duplicated customer
  population, so any row-summed portfolio count is potentially misleading.
  Deferred until a true portfolio customer-reach dataset exists (GAP-001 closed).
- `Portfolio Coverage % = SUM(Ranking[].ActiveCustomerCount) /
  SUM(Ranking[].TotalCustomerCount)` — **decided** (GAP-002 closed). Weighted
  aggregation preserves Principal population size and avoids simple-average
  distortion. It is an aggregate Customer–Principal relationship rate, not a
  count of unique customers.
- `Portfolio Return % = response.ReturnPercentage` — **decided** (GAP-003
  closed). It is already the portfolio-level ratio
  (`TotalReturnAmount / PrincipalSalesOutAmount`). No frontend calculation. A
  simple average of per-Principal return percentages is rejected because
  Principals have very different sales volumes and the average would distort
  reality.

### Required Calculations

```text
Total Sales            = PrincipalSalesOutAmount
Total Target           = PrincipalTargetAmount            (null if unavailable)
Overall Achievement %  = AchievementPercentage            (null if unavailable)
Total Active Customers = NOT DISPLAYED                    (GAP-001 closed)
Portfolio Coverage %   = SUM(ActiveCustomerCount) / SUM(TotalCustomerCount)
Portfolio Return %     = response.ReturnPercentage (as-is, no calculation)
```

### Feasibility Verdict

**Fully Feasible.** Every displayed tile is now settled: Total Sales, Total
Target, Overall Achievement %, Portfolio Coverage % (GAP-002), and Portfolio
Return % (GAP-003, displayed as-is). Total Active Customers is excluded
(GAP-001). All frontend-only.

### Risks

- ~~**Double counting** — summed "Total Active Customers" overstates unique
  reach.~~ **Resolved:** the tile is not displayed (GAP-001 closed).
- **Coverage population labeling** — the weighted Coverage ratio includes only
  Principals present in both `ActiveCustomerCount` and `TotalCustomerCount`
  rows. Principals without a coverage snapshot must be excluded from both sums
  (never zero-filled), and the UI must state the effective population.
- **Aggregate vs row reconciliation** — response `AchievementPercentage` is
  computed from totals (`salesOut / target`, lines 972–981) while each ranking
  row uses **stored** `PRN-TGT-003`. The sum of row achievement does not
  necessarily equal the headline achievement.
- **Zero vs missing** — a Principal with no coverage snapshot displays **"No
  Data"** (never 0%), and the weighted ratio excludes it from both sums (OQ-8).
  A single page-level Data Completeness indicator replaces per-tile footnotes.

### Estimated Effort

**Very Low** (frontend-only). Every displayed tile is fully specified; Portfolio
Return % uses the existing response value with no calculation.

---

## 2. Achievement Gap Leaderboard

### Available Data

All five columns exist per `PrincipalPerformanceRankingItem`:
`PrincipalName`, `PrincipalTargetAmount?`, `PrincipalSalesOutAmount`,
`AchievementAmount? (PRN-TGT-002)`, `AchievementPercentage? (PRN-TGT-003)`.

### Derived Metrics

None for the gap itself. **`Achievement Gap = Ranking[].AchievementAmount`
(PRN-TGT-002)**, consumed as a governed KPI output. The UI must **not**
recalculate it as `Target - PrincipalSalesOutAmount` (OQ-6 closed).
`Actual Sales = PrincipalSalesOutAmount`.

### Required Calculations

```text
Achievement Gap = Ranking[].AchievementAmount        (PRN-TGT-002, authoritative)
Achievement %   = Ranking[].AchievementPercentage     (PRN-TGT-003, authoritative)
```

Governance rule: **dashboards consume KPI outputs; dashboards do not redefine KPI
calculations.**

### Feasibility Verdict

**Fully Feasible.** Frontend-only, no backend change, no derived arithmetic.

### Risks

- **Missing targets** — `MissingTargetExceptionCount` and row-level
  `AchievementAmount == null` mean some high-sales Principals show a null gap.
  Sort order must handle nulls explicitly.
- **Rank vs recompute** — row `Rank` is stored; re-sorting by gap produces a
  different order. Displaying both without a rule causes confusion.
- **Aggregate vs row** — the response-level `AchievementAmount` is currently
  computed (`salesOut - target`) while rows carry stored `PRN-TGT-002`; see the
  residual reconciliation risk in the Executive Summary.

### Estimated Effort

**Very Low.**

---

## 3. Coverage & Reach Analysis

### Available Data

Per `PrincipalPerformanceRankingItem`: `ActiveCustomerCount?`,
`TotalCustomerCount?`, `CoveragePercentage?`. The response also flags
`CustomerReachIsAvailable`.

### Derived Metrics

- Reach/achievement quadrant classification:
  `High Coverage + Low Achievement`, `Low Coverage + High Achievement`.
- These are **threshold-based** labels; no thresholds exist in the endpoint.

### Required Calculations

```text
Coverage %        = stored Ranking[].CoveragePercentage
Quadrant          = f(CoveragePercentile, AchievementPercentile)
                    Opportunity: Coverage P>=70 AND Achievement P<=40
```

### Feasibility Verdict

**Fully Feasible.** A per-Principal coverage table and scatter/quadrant view are
computable from existing fields; relative percentile thresholds are defined
(GAP-006).

### Risks

- **Population skew** — only `Ranking` Principals carry coverage; non-ranked
  Principals are invisible.
- **As-of vs period** — coverage snapshots carry an internal as-of date which is
  intentionally not exposed; only page-level `GeneratedAt` is shown (GAP-007).
  This is an accepted limitation of a management dashboard.
- **Small populations** — percentile quadrants are coarse with few Principals.

### Estimated Effort

**Low–Medium** (chart + percentile logic).

---

## 4. Return Risk Analysis

### Available Data

Per row: `TotalReturnAmount?`, `GoodReturnAmount?`, `BrokenReturnAmount?`,
`ReturnPercentage?`. Response totals and `ReturnPercentage` also present.

### Derived Metrics

- Return ranking (by amount or by %), Return Risk flag via percentile.
- `Return % = stored Ranking[].ReturnPercentage`.

### Required Calculations

```text
Return Amount       = Ranking[].TotalReturnAmount
Return %            = Ranking[].ReturnPercentage
Return Ranking      = sort(Ranking by TotalReturnAmount | ReturnPercentage)
Portfolio Return %  = response ReturnPercentage (weighted)
ReturnRisk          = Return Percentile >= 80                (GAP-006)
```

### Feasibility Verdict

**Fully Feasible.** Tables, amount-based ranking, and the relative Return Risk
flag (Return Percentile >= 80) are all frontend-only (GAP-006).

### Risks

- **Ratio distortion** — Principals with small Sales-Out produce volatile Return %.
  Rank by amount and by % will disagree.
- **Semantic protection (GR-001)** — Return % is a quality ratio, **not** a
  deduction from Sales-Out and **not** Net Sales. The redesign must not present
  returns as reducing portfolio sales.
- **Missing data** — rows without a return snapshot display **"No Data"** (never
  0%); aggregates exclude them (OQ-8).
- **Independent KPI families** — Return amounts must stay separate from
  `PRN-SALES-001`.

### Estimated Effort

**Very Low** (ranking/table) / **Low** (risk flags).

---

## 5. Salesman Dependency Analysis (Special Focus)

### 5.1 Is salesman contribution data sufficient?

**Sufficient for an analytical distribution model.** The response exposes a flat
`SalesmanContributions[]` for all ranked Principals
(`AttachStoredSalesmanContributions`, lines 627–684) with `ContributionAmount`,
`LineCount`, and `HasTargetResponsibility`. Contribution is treated as a
**distribution** of sales across salesmen, **not** a reconciliation of
`PRN-SALES-001` (GAP-005 closed).

Known and accepted characteristics:

1. **No reconciliation to Sales-Out.** The contribution snapshot's `Exceptions`
   collection (`PrincipalSalesmanContributionExceptionRow`) is **dropped** before
   building the response, so the unattributed amount is invisible and
   `SUM(ContributionAmount)` may not equal a Principal's `PrincipalSalesOutAmount`.
   This is accepted because the dashboard identifies concentration, not
   accounting completeness. The optional `ContributionCoverage` metric discloses
   the unexplained share.
2. **Coverage is conditional.** `ContributionIsAvailable` is `true` only when at
   least one row attaches; a Principal with no contribution snapshot has no
   dependency data and must be rendered as **"No Data"**, not as 0% (OQ-8).
3. **No time dimension.** Only the current period is present, so dependency
   *trend* is impossible.

### 5.2 Can dependency percentage be calculated?

**Yes, client-side. The denominator is decided:**

```text
Contribution % = ContributionAmount
               / SUM(ContributionAmount for that Principal)
```

This normalizes each Principal's salesmen to 100% (analytical distribution
model, GAP-005/OQ-7 closed). It intentionally does **not** divide by
`PrincipalSalesOutAmount`; unattributed Faktur lines are out of scope for the
distribution.

**Optional Contribution Coverage** (transparency only, non-blocking):

```text
Contribution Coverage = SUM(ContributionAmount) / PrincipalSalesOutAmount
```

Coverage near 100% means contribution explains nearly all Sales-Out; a lower
value means part of Sales-Out is unattributed. It is informational and is not a
reconciliation control. Note: the existing `principalContribution.ts` divides by
`PrincipalSalesOutAmount`, so it is reused for Coverage and a new helper is
needed for the normalized Contribution %.

### 5.3 Formulas (decided)

```text
Denominator(p)               = SUM(ContributionAmount for p)
PrincipalSalesmanShare(p, s) = ContributionAmount(p, s) / Denominator(p)
DependencyRatio(p)           = MAX over s of PrincipalSalesmanShare(p, s)
DependencyRank               = RANK p by DependencyRatio DESC
ContributingSalesmanCount(p) = COUNT(DISTINCT s in contributions of p)
ContributionCoverage(p)      = SUM(ContributionAmount for p) / PrincipalSalesOutAmount(p)
PortfolioTopSalesman         = group SalesmanContributions by SalesPersonId,
                               SUM(ContributionAmount), ORDER BY DESC
Distribution(p)              = ordered list of shares, or a concentration curve
```

**No `Low/Medium/High` bands are introduced.** The endpoint contains objective
contribution data but no approved dependency risk thresholds; ranking preserves
the information without arbitrary classification (GAP-004 closed).

`ContributingSalesmanCount` is **supplementary metadata only** (GAP-008 closed).
It describes participation, not concentration, and must **not** feed dependency
classification, ranking, alerts, or insights. The primary dependency indicators
remain Top Contributor %, `DependencyRank`, and the Contribution Distribution.

### 5.4 Can dependency be derived entirely client-side?

**Yes.** `Contribution %`, `DependencyRatio`, `DependencyRank`, and the optional
`ContributionCoverage` are pure frontend arithmetic over `SalesmanContributions[]`
and the ranking item amounts. No classification and no backend change.

### 5.5 Limitations

- Contribution is a distribution model, not a reconciliation: `Contribution
  Coverage` may be below 100% because unattributed lines are intentionally not
  exposed. This is **accepted** and does not block implementation (GAP-005).
- No dependency trend/history. Trends are out of scope for V2 (GAP-009
  deferment); the endpoint is a single-period snapshot.
- Salesman identities are period-local; a salesman spanning multiple Principals
  is aggregated only by client-side grouping, with no canonical "salesman
  dependency" definition.
- `HasTargetResponsibility` exists but its use in dependency scoring is undefined.
- Line-level allocation across salesmen on one Faktur is internal; the app cannot
  audit it from the payload.

### Feasibility Verdict

**Fully Feasible.** Contribution %, `DependencyRatio`, `DependencyRank`, top
contributor, distribution, `ContributionCoverage`, and portfolio grouping are all
computable client-side. No band classification, no reconciliation, no backend
change.

### Estimated Effort

**Medium** (grouping, per-Principal concentration, distribution visualization,
new section). No backend change required.

---

## 6. Opportunity & Risk Board

### Available Data

All rule inputs already exist (gap, coverage, return, dependency ratio,
achievement). The endpoint provides **no** insight rules, but the thresholds are
now defined relatively (GAP-006 closed).

**Governance (OQ-9 closed):** EX01/EX02 remain the authoritative alerting
mechanism. The Board is a portfolio-prioritization surface, not a second alert
engine. It may consume KPI rankings, percentiles, and EX01/EX02 outputs, but must
**not** introduce independent alert thresholds.

### Derived Metrics

Percentile ranks are computed client-side across the ranked Principal
population. Higher percentile = higher metric value.

| Insight | Rule (decided) |
| --- | --- |
| Opportunity — reachable gap | Coverage Percentile >= 70 AND Achievement Percentile <= 40 |
| Return Risk | Return Percentile >= 80 (by `ReturnPercentage` / `TotalReturnAmount`) |
| Dependency Risk | Dependency Percentile >= 90 (by `DependencyRatio`) |
| Underperforming | Achievement Percentile <= 25 |

### Required Calculations

```text
Percentile(p, metric) = 100 * (number of ranked Principals with a lower value)
                        / (population size)

Opportunity       = Percentile(coverage) >= 70 AND Percentile(achievement) <= 40
ReturnRisk        = Percentile(return) >= 80
DependencyRisk    = Percentile(dependencyRatio) >= 90
Underperforming   = Percentile(achievement) <= 25
```

Thresholds are **relative**, so they adapt to portfolio characteristics and avoid
arbitrary fixed business constants. Percentile is undefined for Principals
missing the underlying metric; those Principals are excluded from that rule, not
assumed to be low.

### Feasibility Verdict

**Fully Feasible.** The rule engine and percentiles are pure client-side
computation over existing fields; thresholds are relative (GAP-006) and the Board
does not create an alerting engine (OQ-9). Remaining work is UX: card precedence
and whether each card displays its rule and inputs.

### Risks

- **Missing ≠ bad.** A null coverage/return/achievement must not be treated as
  "low" or "high"; the Principal is excluded from that rule and the value renders
  as **"No Data"** (OQ-8).
- **Coexistence** — the same Principal can satisfy both an opportunity and a risk
  rule. This is a prioritization/display concern, not alerting; the Board must
  not raise alerts independently (OQ-9).
- **Small populations** — with few Principals, percentile ranks are coarse
  (a single Principal can be the 100th percentile). Display the population size.
- **Audit/trust** — each card should expose the rule and the Principal's metric
  and percentile, or the insight is not defensible.
- **Alert duplication avoided** — EX01/EX02 stay authoritative; the Board is
  prioritization only and introduces no independent thresholds (OQ-9).

### Estimated Effort

**Low** (percentile computation + rule cards, frontend-only).

---

## 7. Principal Detail Table

### Available Data

Complete per-Principal field set in `PrincipalPerformanceRankingItem`
(sales, target, achievement, returns, growth, customer reach), plus
`SalesmanContributions[]` for drill-down.

### Derived Metrics

None required; it is a projection of existing fields. Optional client-side
sorting/filtering.

### Required Calculations

```text
None (direct projection) + optional derived Gap Amount
```

### Feasibility Verdict

**Fully Feasible.** Drop-in frontend work; no backend impact.

### Risks

- **Null rendering** — many optional columns; must distinguish "no data" from 0.
- **Column overload** — all fields at once is dense; progressive disclosure is a
  UX choice, not a data constraint.
- **Currency/percent scaling** — percentages are stored as ratios and currently
  multiplied by 100 in the view; consistency is required.

### Estimated Effort

**Very Low.**

---

# Data Gaps

### Critical Gaps (all closed)

| Gap ID | Requested metric | Gap | Type |
| --- | --- | --- | --- |
| GAP-001 | Portfolio Total Active Customers | **CLOSED** — decision: do not display; no de-duplicated portfolio customer population exists | Data/Aggregation |
| GAP-002 | Portfolio Coverage % | **CLOSED** — defined as `SUM(ActiveCustomerCount) / SUM(TotalCustomerCount)`; frontend-only | Data/Definition |
| GAP-003 | Average Return % | **CLOSED** — display `response.ReturnPercentage` as "Portfolio Return %" as-is; no frontend calculation, no backend change | Definition |
| GAP-004 | Salesman Dependency classification | **CLOSED** — no Low/Medium/High bands; use Contribution % + Dependency Rank | Business rule |
| GAP-005 | Contribution completeness | **CLOSED** — contribution is an analytical distribution model, not reconciliation; denominator = `SUM(ContributionAmount)`; optional Contribution Coverage = `SUM(ContributionAmount)/PrincipalSalesOutAmount`; unattributed amounts do not block | Data |
| GAP-006 | Opportunity/Risk thresholds | **CLOSED** — use percentile/ranking-based thresholds, not fixed KPI constants | Business rule |

### Nice-to-Have Gaps (all resolved)

All GAP-001–GAP-011 are now closed or closed by exclusion/deferment. No open
data gap remains for V2.

| Gap ID | Gap | Value |
| --- | --- | --- |
| GAP-007 | Per-snapshot `GeneratedAt`/`AsOfDate` | **CLOSED BY EXCLUSION** — display only page-level `response.GeneratedAt`; no per-KPI lineage metadata |
| GAP-008 | Contributing salesman count per Principal | **CLOSED** — `COUNT(DISTINCT SalesPersonId)` as supplementary metadata; not used for classification, ranking, alerts, or insights |
| GAP-009 | Time-series of achievement/coverage/return/dependency | **CLOSED BY DEFERMENT** — out of scope for V2; needs a dedicated period-by-period historical analytics dataset |
| GAP-010 | Customer overlap matrix / unique customer reach | **CLOSED BY EXCLUSION** — belongs to Customer Analytics / Portfolio Optimization; revisit only when BTR adds those capabilities |
| GAP-011 | Per-Principal return reason/type split beyond Good/Broken | **CLOSED BY DEFERMENT** — dashboard identifies return risk, not root cause; consider a dedicated Return Analytics capability if operational users need root-cause investigation |

---

# Recommended MVP Scope

Implement immediately using only the existing endpoint response and **no
unapproved assumptions**. All items are frontend-only.

1. **Portfolio Overview** — Total Sales, Total Target, Overall Achievement %,
   **Portfolio Coverage %** (`SUM(Active)/SUM(Total)`), and **Portfolio Return %**
   (`response.ReturnPercentage` as-is), plus a visible "Principals with coverage
   data" count. **Total Active Customers is permanently excluded** (GAP-001
   closed) and **"Average Return %" is not displayed** (GAP-003). Label the
   headline achievement as portfolio-level (`salesOut / target`) to avoid
   row-sum confusion.
2. **Achievement Gap Leaderboard** — use governed `AchievementAmount (PRN-TGT-002)`
   as the Gap; sort by it; render null values explicitly. Do **not** derive the
   gap as `Target - Sales` (OQ-6 closed).
3. **Coverage & Reach table** — per-Principal Active / Total / Coverage; relative
   highlight via Coverage/Achievement percentiles (GAP-006).
4. **Return Risk table + amount-based ranking** — keep returns independent of
   Sales-Out and label Return % as a quality ratio (GR-001); Return Risk flag at
   Return Percentile >= 80 (GAP-006).
5. **Salesman Dependency** — show per-Principal top contributor, Contribution %
   (`ContributionAmount / SUM(ContributionAmount for Principal)`), distribution,
   `DependencyRatio` (`Max(Contribution %)`), a `DependencyRank` (Principals
   ranked by `DependencyRatio` descending), and an optional `ContributionCoverage`
   (`SUM(ContributionAmount)/PrincipalSalesOutAmount`). Add
   `ContributingSalesmanCount = COUNT(DISTINCT SalesPersonId)` as **supplementary
   metadata only** — participation, not concentration; not used for
   classification, ranking, alerts, or insights (GAP-008). **No Low/Medium/High
   bands** (GAP-004 closed); no reconciliation (GAP-005 closed). Add a
   normalized-share helper; reuse `principalContribution.ts` for Coverage.
6. **Principal Detail Table** — full projection with null-safe rendering; missing
   values show **"No Data"** (OQ-8).
7. **Opportunity & Risk Board** — percentile/ranking rules: Opportunity
   (Coverage P>=70 AND Achievement P<=40), Return Risk (Return P>=80),
   Dependency Risk (Dependency P>=90), Underperforming (Achievement P<=25). Each
    card shows its rule, metric, and percentile; exclude Principals missing the
    metric (GAP-006 closed). Prioritization only — **EX01/EX02 remain the
    authoritative alerting mechanism**; the Board introduces no independent alert
    thresholds (OQ-9 closed).
8. **Page-level Data Completeness indicator** — a single disclosure summarising
   which KPI snapshots are present/absent for the period (target, returns,
   growth, coverage, contribution), derived from the existing
   `*IsAvailable`/exception fields and non-null row counts. Replaces
   per-aggregate footnotes; all missing values elsewhere render **"No Data"**
   (OQ-8 closed).

This delivers all seven sections with zero backend, DB, ETL, or new KPI work.

## V2 Scope Exclusions

The following are deliberately **out of scope** for Principal Performance
Dashboard V2:

- Portfolio Total Active Customers (GAP-001) and unique customer reach / customer
  overlap matrix (GAP-010) — customer overlap is a Customer Analytics / Portfolio
  Optimization concern, not Principal Performance.
- Per-KPI freshness / data-lineage metadata (GAP-007).
- Trend and sustainability analysis for Achievement %, Coverage %, Return %, and
  Dependency Ratio (GAP-009) — the endpoint is a single-period snapshot.
- Dependency risk bands / Low-Medium-High classification (GAP-004).
- Contribution reconciliation (GAP-005).
- Return root-cause categories beyond the existing Good / Broken split (GAP-011)
  — the dashboard identifies return risk, not its cause.

---

# Recommended Future Enhancements

Require additional backend support, an endpoint revision, or new analytics
datasets:

1. **Customer reach / overlap analytics** — de-duplicated unique active customers
   and principal-overlap counts. Required before Portfolio Total Active Customers
   can ever be displayed (GAP-001 deferred; GAP-010 excluded). Revisit only when
   BTR introduces Customer Analytics, Cross-Sell Analytics, or Portfolio
   Optimization capabilities.
2. **Optional contribution transparency** — surface
   `PrincipalSalesmanContributionResult.Exceptions` or an "unattributed
   contribution" amount to show where `ContributionCoverage` falls short. Not
   required: contribution is an analytical distribution, not reconciliation
   (GAP-005 closed).
3. ~~**Expose snapshot provenance** — per-field `GeneratedAt`/`AsOfDate`.~~
   **Excluded by decision (GAP-007):** a single page-level `GeneratedAt` is
   shown; per-KPI lineage is not added to this management dashboard.
4. **Historical analytics dataset (trends)** — a dedicated period-by-period
   dataset for Achievement %, Coverage %, Return %, and Dependency Ratio, enabling
   trend and sustainability analysis. **Out of scope for V2** (GAP-009 closed by
   deferment); the current endpoint is a single-period snapshot and trend analysis
   must not be introduced until this dataset exists.
5. **Opportunity & Risk scoring refinements** — percentile rules are decided
   (GAP-006); future work could add minimum-population handling, weighting, or
   rule precedence controls.
6. **Target-gap root-cause decomposition** — by salesman, product, or customer
   (new analytics grain).
7. **Canonical cross-Principal salesman contribution semantics** — define how a
   salesman selling multiple Principals is represented to management.
8. **Return Analytics capability** — a dedicated surface for return root-cause
   categories if operational users require it. Out of scope for this dashboard
   (GAP-011 closed by deferment).

---

# Open Questions

### Business Questions (all resolved)

- ~~**OQ-1** — What is the approved definition of "Average Coverage %"?~~
  **RESOLVED:** Portfolio Coverage % = `SUM(ActiveCustomerCount) /
  SUM(TotalCustomerCount)` (weighted; frontend-only). GAP-002 closed.
- ~~**OQ-2** — What does "Total Active Customers" mean at portfolio level?~~
  **RESOLVED:** not displayed; deferred until a de-duplicated portfolio reach
  dataset exists (GAP-001 closed).
- ~~**OQ-3** — Is "Average Return %" the weighted portfolio ratio or a simple
  mean?~~ **RESOLVED:** display `response.ReturnPercentage` as-is, labeled
  **Portfolio Return %**. Average Return % is not displayed. GAP-003 closed.
- ~~**OQ-4** — Which threshold bands define Low/Medium/High dependency?~~
  **RESOLVED:** no bands. Use `Contribution %` and `Dependency Rank`
  (rank by `DependencyRatio` descending). GAP-004 closed.
- ~~**OQ-5** — Which thresholds and precedence rules drive the Opportunity &
  Risk Board?~~ **RESOLVED:** percentile/ranking-based thresholds (relative, not
  fixed constants). GAP-006 closed. Precedence/display detail remains a UX
  decision (OQ-9).

### Technical Questions

- ~~**OQ-6** — Which is authoritative for a Principal row: derived `Gap Amount`
  or stored `AchievementAmount (PRN-TGT-002)`?~~ **RESOLVED:** stored
  `PRN-TGT-002` is authoritative. The UI must not recalculate the gap. Rule:
  dashboards consume KPI outputs; dashboards do not redefine KPI calculations.
- ~~**OQ-7** — Is the dependency denominator `PrincipalSalesOutAmount` or the
  sum of contributions?~~ **RESOLVED:** `SUM(ContributionAmount)` per Principal
  (analytical distribution model). Optional Contribution Coverage uses
  `SUM(ContributionAmount)/PrincipalSalesOutAmount`. GAP-005 closed.
- ~~**OQ-8** — Should missing snapshots be excluded, shown as "no data", or
  footnoted in every aggregate?~~ **RESOLVED:** missing values display as
  **"No Data"**; aggregates exclude missing snapshots; a single **page-level Data
  Completeness indicator** replaces per-aggregate footnotes. Unknown is never
  treated as zero.

### Operational Questions

- ~~**OQ-9** — Does the Opportunity & Risk Board duplicate EX01/EX02 alerting, and
  if so which surface is authoritative?~~ **RESOLVED:** EX01/EX02 remain the
  authoritative alerting mechanism. The Board is a portfolio-prioritization
  surface, not a second alert engine; it may consume KPI rankings, percentiles,
  and EX01/EX02 outputs but introduces **no independent alert thresholds**.
- ~~**OQ-10** — What is the acceptable freshness lag across the independently
  refreshed snapshots?~~ **RESOLVED:** per-KPI freshness is not surfaced; a
  single page-level `GeneratedAt` ("Data Updated At") is shown (GAP-007).

---

# Implementation Impact Inventory

### Backend

- **None required for MVP.** No aggregate, entity, repository, service, command,
  or query changes.
- Optional future transparency enhancement: add an unattributed-contribution
  field to the existing `PrincipalPerformanceResponse` (same endpoint, no new
  API). Not required — GAP-005 is closed and contribution is treated as an
  analytical distribution.

### Database

- **None.** No table, view, stored procedure, index, or constraint changes.

### Frontend

- **Affected:** `PrincipalPerformanceDashboardView.vue` (section restructure),
  `models/dashboard.ts` types (read-only reuse), `services/principalContribution.ts`
  (reuse for `ContributionCoverage`), formatters, and new presentational
  components for portfolio tiles, leaderboard, coverage/return tables,
  dependency distribution, opportunity/risk cards, and detail table. Add a
  page-level "Data Updated At" from `response.GeneratedAt` (GAP-007), a
  page-level Data Completeness indicator and shared "No Data" rendering (OQ-8),
  and percentile + normalized-Contribution-% helpers.
- **Unaffected measures:** `PRN-SALES-001` display values, disclosures, evidence
  routes, and supporting ranking behavior must remain unchanged.

### Integration

- **None.** No new API, event, or external system.

### Security

- **None new.** Existing `[Authorize]` on the controller is unchanged; no new
  data exposure beyond fields already returned.

---

# Planning Readiness

### Status

```text
READY
```

### Blocking Issues

None. GAP-001 through GAP-011 are all closed (by decision, exclusion, or
deferment) and OQ-1 through OQ-10 are all resolved. No open data gap, no open
question, and no missing field prevents implementation of the MVP scope.

### Planner Guidance

- **MVP scope is frontend-only** and can be planned immediately for all seven
  sections. Dependency is fully decided (Contribution % + `DependencyRank`, no
  bands, no reconciliation); the Opportunity & Risk Board uses relative
  percentile rules (GAP-006) and is prioritization-only, not an alert engine
  (OQ-9).
- **Major dependencies:** reuse of `principalContribution.ts` (for
  `ContributionCoverage`) and existing formatters; a normalized Contribution %
  helper, a percentile helper, and shared "No Data" / Data Completeness
  rendering; preservation of `PRN-SALES-001` and GR-001 semantics.
- **Sequencing concerns:** all sections are unblocked. Confirm rule-card
  disclosures and card coexistence presentation; ensure the Board consumes
  EX01/EX02 output rather than redefining alerting.
- **Review concerns:** verify no returns are deducted from Sales-Out; verify
  missing values render as "No Data" and are excluded from percentiles and
  aggregates; verify the leaderboard consumes governed `PRN-TGT-002` rather than
  derived gap arithmetic; verify aggregate-vs-row reconciliation is documented
  in the UI.
