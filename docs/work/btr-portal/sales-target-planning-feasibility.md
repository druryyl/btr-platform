# Sales Target Planning — Feasibility Analysis

| Field | Value |
| ----- | ----- |
| Status | Feasibility assessment — analysis only; no implementation |
| Date | 2026-09-05 |
| Request type | Business + technical feasibility |
| Systems | BTR Portal (`btr.portal.web` / API / worker), BTR Desktop SM6 |
| Related artifacts | [sales-person-principal-target/feature.md](../../features/sales-person-principal-target/feature.md), [salesman-target-plan.md](../salesman-target/salesman-target-plan.md), [dashboard-02-sa02-sales-forecast.md](../../features/btr-portal/dashboard-02-sa02-sales-forecast.md), [dashboard-10-sf01-salesmen.md](../../features/btr-portal/dashboard-10-sf01-salesmen.md), [btr-portal-domain.md](../../features/btr-portal/btr-portal-domain.md), [btr-portal-architecture.md](../../features/btr-portal/btr-portal-architecture.md), Entity Analytics / Inventory Optimization recommendation patterns |

---

## 1. Executive Summary

The proposed shift from a simple target-entry screen to a **management planning and decision-making workflow** is strategically sound and aligns with BTR Portal’s maturity path (Analytics → Forecasting → Optimization / Decision Support).

Most of the **context metrics** needed for planning cards already exist in Portal salesman snapshots, achievement history, and entity analytics. A deterministic Safe / Normal / Stretch recommendation model can be built from historical omzet and growth rates without AI/ML.

The full vision — company goal setting, planning cards, explainable recommendations, risk scoring, portfolio balancing, and locked planning snapshots — is **too large for a single delivery**. The main constraints are:

1. **Portal is architecturally read-only** for business data; target writes live in Desktop SM6 today.
2. Several proposed signals (monthly visit completion, territory potential) are **not available** at planning grain.
3. **Planning lock / snapshot versioning** does not exist and conflicts with current last-write-wins SM6 rules.
4. Card-per-salesman UX does not scale well for large sales forces without hybrid grid/filter patterns.

**Final verdict: GO WITH REDUCTIONS** — deliver an MVP that makes target setting contextual and recommendable, keep writes on Desktop (or a thin Portal write exception later), defer lock/versioning and advanced risk/territory signals.

---

## 2. Business Value Assessment

### 2.1 Does this provide value beyond data entry?

**Yes — high incremental value if scoped correctly.**

Today’s planning loop is fragmented:

| Step | Current state |
| ---- | ------------- |
| Historical performance | Available in SF01 / SA01 / trend APIs — but **not** presented during target entry |
| Forecast | Available company-wide in SA02 — **not** used when setting next-month targets |
| Target entry | Desktop SM6 — Principal × Salesman grid; copy previous month; no recommendations |
| Execution monitoring | SA01 / SF01 / Alert Center |
| Achievement evaluation | SF01 bands, SA02 forecast achievement — after the fact |

The proposed workflow closes the gap between **seeing performance** and **setting expectations**. That is a real management problem: owners already use spreadsheets and judgment outside BTR because SM6 is entry-only.

Meaningful value comes from:

- Starting from a **company goal**, not from blank rows
- Giving each salesman a **decision context** before assigning a number
- Surfacing **unrealistic stretch** before month start
- Preserving a **planned vs actual** comparison later

Value is weak if the feature becomes a decorative card wrapper over the same manual entry with unreliable recommendations.

### 2.2 Alignment with distributor owner planning behavior

Typical distributor owner / sales manager behavior:

1. Decide desired company growth for next month (often from last month + gut feel).
2. Allocate to salesmen based on territory strength, recent performance, and trust.
3. Negotiate verbally with key salesmen for stretch cases.
4. Enter numbers (or copy last month and tweak).
5. Review mid-month when achievement looks weak.

The proposed concept matches steps 1–3 well. It overshoots typical practice if it requires reviewing long rationale text for every salesman every month, or if lock/reopen governance is heavier than current “plans change mid-month” reality (SM6 already allows mid-month revision with last-write-wins).

**Assessment:** Strong alignment for **monthly pre-month planning ritual**; weaker alignment for heavy governance (immutable snapshots) in v1.

### 2.3 Likely impact on planning quality and visibility

| Outcome | Likelihood | Condition |
| ------- | ---------- | --------- |
| Fewer missing targets | High | Completeness + company gap visible at planning time |
| More realistic targets | Medium–High | If Safe/Normal/Stretch are transparent and based on recent actuals |
| Better coaching conversations | Medium | If cards show portfolio/piutang alongside target |
| Better company goal discipline | Medium | If portfolio balancing is first-class |
| Trust in recommendations | Medium | Only if explanations are simple and deterministic |
| Over-engineering / unused UI | Medium risk | If full card UX + risk taxonomy ships before habits form |

**Business feasibility: Strong.** The concept is the natural “Optimization” layer for Sales after SA02 forecasting, analogous to Inventory Optimization after Inventory Forecast.

---

## 3. Data Availability Assessment

Classification key:

| Class | Meaning |
| ----- | ------- |
| **Available** | Exists today in Portal snapshots / APIs / Desktop |
| **Available with minor work** | Derive from existing stores with light aggregation or API shaping |
| **Requires new analytics** | New formulas, rollups, or snapshot fields needed |
| **Not currently feasible** | Missing foundation data or product definition |

### 3.1 Company-level planning summary

| Proposed metric | Required data | Status | Notes |
| --------------- | ------------- | ------ | ----- |
| Current month forecast | SA02 Forecast Sales | **Available** | Company-level pace forecast |
| Previous month actual sales | Prior-month Faktur omzet | **Available with minor work** | Company sum from sales history / prior snapshot month |
| Previous month target achievement | Prior target + prior omzet | **Available with minor work** | Same resolution as current achievement |
| Proposed target for next month | Company goal input | **Requires new analytics** | New planning artifact (not a KPI today) |
| Expected growth % | Goal vs baseline | **Requires new analytics** | Trivial once company goal exists |

### 3.2 Salesman planning card — historical context

| Proposed metric | Status | Evidence / gap |
| --------------- | ------ | -------------- |
| Current month forecast (per salesman) | **Requires new analytics** | `SalesForecastPolicy` exists for company only; apply same formula to SF01 MTD omzet |
| Last month actual | **Available with minor work** | `BTRPD_SalesmanRepHistory` + trend API (`CompletedOmzet`) |
| 3-month average | **Available with minor work** | Average last 3 history points |
| Best month this year | **Available with minor work** | `MAX(CompletedOmzet)` YTD from history |

### 3.3 Salesman planning card — operational context

| Proposed metric | Status | Evidence / gap |
| --------------- | ------ | -------------- |
| Customer count | **Available** | SF01 / entity analytics portfolio |
| Active customer count | **Available** | Invoiced this month |
| Dormant customer count | **Available** | Supporting context |
| Visit completion rate (monthly) | **Requires new analytics** | SF02/SF03 are day-scoped; team 7/30-day trends exist; calendar-month per-rep execution needs new rollup |
| Collection / piutang risk | **Available** | Open balance, overdue, high-exposure signals (SF01) |
| Achievement band / MoM growth | **Available** | SF01 + entity analytics |
| Territory potential | **Not currently feasible** | Wilayah is a segment only; no TAM / opportunity / potential model |

### 3.4 Suggested target scenarios & explanations

| Proposed capability | Status | Notes |
| ------------------- | ------ | ----- |
| Safe / Normal / Stretch amounts | **Requires new analytics** | Inputs largely available; engine does not exist |
| Recommendation explanation | **Requires new analytics** | Can be rule-based from trend, portfolio, piutang — same pattern as Inventory/Collection Optimization reasons |
| Collection-risk-aware suggestion | **Available with minor work** (as input) | Risk exists; mapping into target adjustment is new policy |
| Visit-driven suggestion | **Requires new analytics** | Depends on monthly visit rollup |
| Territory-potential-driven suggestion | **Not currently feasible** | Do not include in MVP |

### 3.5 Risk assessment after target selection

| Proposed metric | Status | Notes |
| --------------- | ------ | ----- |
| Expected sales (baseline forecast) | **Requires new analytics** | Per-rep pace or historical baseline |
| Target gap | **Available with minor work** | `Target − Expected` once expected exists |
| Difficulty / risk class | **Requires new analytics** | Objective formula possible; reliability medium early on |

### 3.6 Portfolio balancing

| Proposed metric | Status | Notes |
| --------------- | ------ | ----- |
| Sum of salesman targets | **Available** | Same as `SumTargetAmountForMonth` once next-month rows exist |
| Company goal | **Requires new analytics** | New planning field |
| Gap (goal − assigned) | **Requires new analytics** | Trivial UI once both exist |

### 3.7 Planning finalization / lock

| Proposed capability | Status | Notes |
| ------------------- | ------ | ----- |
| Finalize monthly plan | **Requires new analytics** | New lifecycle state |
| Planning snapshot lock | **Requires new analytics** | New tables; conflicts with SM6 last-write-wins |
| Reopen / revision workflow | **Requires new analytics** | Process + permissions |
| Planned vs actual evaluation | **Available with minor work** after lock | Compare locked plan to month-end omzet |

### 3.8 Target write path (cross-cutting)

| Capability | Status |
| ---------- | ------ |
| Read resolved targets (rep / company / principal) | **Available** |
| Write targets in Desktop SM6 | **Available** |
| Write targets in Portal | **Not currently feasible** under current architecture (Portal is read-only for business data) |

---

## 4. Technical Feasibility Assessment

### 4.1 Recommendation engine (Safe / Normal / Stretch)

**Feasible without AI/ML.** Prefer deterministic, explainable formulas consistent with Portal’s forecasting philosophy.

#### Recommended MVP model

For each salesman, planning period `P` (usually next calendar month):

```text
Baseline     = Average(CompletedOmzet for last 3 completed months)
               fallback: last month actual, then YTD monthly average
TrendFactor  = LastMonth / PriorMonth   (clamped, e.g. 0.85–1.15)
Safe         = Baseline × 0.95
Normal       = Baseline × TrendFactor          (or Baseline × 1.00 if trend weak)
Stretch      = Baseline × max(TrendFactor, 1.05) × StretchMultiplier (e.g. 1.08–1.12)
```

Optional dampeners (Phase 2):

- High overdue exposure → reduce Stretch (or flag warning)
- Shrinking active customer base → reduce Normal/Stretch
- Missing history (< 2 months) → only Safe + Custom; suppress Stretch

#### Alternative approaches

| Approach | Complexity | Risk | Fit |
| -------- | ---------- | ---- | --- |
| Historical average only | Low | Low | Good MVP floor |
| Trend-based projection | Low–Medium | Medium (volatile reps) | Recommended with clamps |
| Growth-rate from company goal | Medium | Medium | Good for portfolio balancing |
| Combination model | Medium | Medium | Best Phase 2 |
| ML / predictive models | High | High | **Not recommended** — breaks Portal explainability norms |

#### Implementation risk: **Medium**

Risk is not math — it is **trust**. Wrong Stretch targets will destroy adoption. Mitigate with:

- Transparent formula text on each card
- Clamp outliers
- Always allow Custom
- Never auto-save recommendations without Owner confirmation

### 4.2 Risk assessment (target difficulty)

**Objectively calculable at a useful but imperfect level.**

Suggested formula family:

```text
Expected     = per-rep month-end pace forecast OR baseline Normal
Coverage     = Target / Expected
Difficulty   =
  Low    if Coverage ≤ 1.05
  Medium if Coverage ≤ 1.20
  High   if Coverage > 1.20
```

Enhancements:

- Adjust band thresholds by salesman volatility (std. dev. of last 6 months)
- Elevate risk if High Overdue Exposure even when Coverage is Medium
- Unknown if Expected cannot be computed (new salesman)

| Concern | Assessment |
| ------- | ---------- |
| Required formulas | Simple ratio + bands — Low complexity |
| Data requirements | Expected sales is the hard part; history is available |
| Reliability | Medium — good for warning Owner; not a precision score |
| Misuse risk | High if treated as performance judgment before month starts |

**Recommendation:** Ship as **advisory difficulty**, not as KPI used in Alert Center initially.

### 4.3 Portfolio balancing

**Technically easy; product-definition medium.**

```text
CompanyGoal      = Owner input for period P
AssignedTargets  = SUM(selected salesman targets for P)
Gap              = CompanyGoal − AssignedTargets
```

Backend support options:

1. **Ephemeral UI state** — company goal stored only in session until finalize (weak)
2. **Period planning header table** — `BTR_SalesTargetPlan(Year, Month, CompanyGoal, Status, …)` (**recommended**)
3. Derive company goal as sum of recommendations — not true Owner decision

Complexity: **Low–Medium** once write path is decided.

### 4.4 Explainability

Reusable Portal patterns already exist:

- Inventory Optimization: action + score + reasons
- Collection Optimization: “Safe to Wait” style categories + reasons
- SA02: Best / Expected / Worst scenario bands (company)

Target planning should follow the same rule: **every recommendation shows 1–3 short reasons**, e.g.:

- “3-month average Rp 185M”
- “Last month up 8% vs prior”
- “High overdue exposure — Stretch reduced”

Do **not** invent territory potential explanations in MVP.

### 4.5 Integration with Principal-level targets (SM6)

Important product constraint: **source of truth is Principal × Salesman × Month**, not only rep total.

Planning UX options:

| Option | Pros | Cons |
| ------ | ---- | ---- |
| A. Plan at **rep total**, write as single legacy/aggregate equivalent | Simple UX | Conflicts with SM6 Principal model; may need auto-allocation |
| B. Plan at **rep total**, allocate to Principals by prior mix | Keeps SM6 truth | Allocation policy needed |
| C. Plan per Principal cards | Most accurate | UX heavy; worse scalability |

**MVP recommendation:** Plan at **rep-total decision level**, persist by allocating across assigned Principals using **prior-month omzet mix** (or equal/copy prior targets), with optional drill-down edit. This preserves SM6 without forcing Principal-level scenario cards in v1.

---

## 5. UX Feasibility Assessment

### 5.1 Cards vs traditional grid

| Approach | Strength | Weakness |
| -------- | -------- | -------- |
| Planning cards | Decision quality, context, scenarios | Slow for 40–100 salesmen; vertical scroll fatigue |
| Editable grid | Fast bulk edit, familiar to SM6 users | Weak context; easy to copy blindly |
| Hybrid | Best of both | Slightly more design work |

**Verdict:** Pure card UI is superior for **quality** and inferior for **throughput**. Hybrid is required for real distributors.

### 5.2 Recommended interaction pattern (MVP)

```text
1. Period selector (next month default)
2. Company planning summary strip (goal, forecast, prior achievement, gap)
3. Filters: Missing target / High risk / Wilayah / Search
4. Dense planning table as default:
     Salesman | Baseline | Safe | Normal | Stretch | Selected | Risk | Status
5. Row expand / side drawer opens planning card detail + reasons
6. Apply Normal to all incomplete / Apply filtered
7. Portfolio footer sticky: Goal / Assigned / Gap
8. Finalize (soft) or Save draft
```

This preserves “management decision tool” positioning without forcing card browsing for every row.

### 5.3 Usability risks

| Risk | Mitigation |
| ---- | ---------- |
| Too much text/rationale | Max 3 reasons; progressive disclosure |
| Scenario buttons ignored | Default select Normal; one-click Apply |
| Owner still copies last month only | Show delta vs last month target prominently |
| Confusing current-month vs next-month | Explicit period language (“Planning for October”) |
| Principal allocation surprises | Show allocated Principal breakdown before save |
| Mobile / small laptop density | Table-first; card drawer secondary |

### 5.4 Scalability

| Team size | UX guidance |
| --------- | ----------- |
| ≤ 15 salesmen | Cards-first is acceptable |
| 15–40 | Hybrid table + drawer |
| > 40 | Table mandatory; bulk apply; filters; virtualized list |

Most BTR distributors will hit the hybrid range quickly.

---

## 6. Analytics Integration Assessment

### 6.1 Fit in Portal maturity model

Portal already defines:

```text
Reporting → Analytics → Decision Support → Forecasting → Optimization
```

Sales Target Planning is the missing **Sales Optimization / Planning** step after SA02 Forecasting — analogous to Inventory Optimization after Inventory Forecast.

### 6.2 Integration map

| Capability | Integration role |
| ---------- | ---------------- |
| SA01 Sales | Baseline company achievement context |
| SA02 Sales Forecast | Company forecast into planning header |
| SF01 Salesmen | Per-rep performance, portfolio, piutang, missing targets |
| SF02 / SF03 | Phase 2+ visit signals only |
| Entity Analytics (Salesman) | Peer percentiles, growth, attention evidence for explanations |
| Alert Center | Later: “Plan incomplete”, “Unbalanced portfolio gap” |
| Executive dashboards | After finalize: planned company goal vs in-month achievement |

### 6.3 Standalone vs broader Planning & Execution

| Option | Assessment |
| ------ | ---------- |
| Standalone “Sales Target Planning” page | Correct for MVP — clear Owner ritual |
| Part of broader Planning & Execution platform | Correct long-term — share plan header, lock lifecycle, evaluation views |

**Recommendation:** Build as a **Sales Planning** capability with a dedicated route/workflow, but design the plan header / lock model so Inventory or Collection planning could reuse the lifecycle later. Do **not** wait for a generic Planning platform before MVP.

Entity Analytics should remain the **investigation evidence layer**; Target Planning consumes evidence and produces a decision. Do not bury planning inside Population Map UX.

---

## 7. Architecture Impact Assessment

### 7.1 Change inventory

| Change | Impact | Class |
| ------ | ------ | ----- |
| Derive history metrics API (last month, 3-mo avg, best month) | Extend salesman history read model | **Low** |
| Per-salesman current-month forecast | Reuse `SalesForecastPolicy` in salesman aggregator/snapshot | **Medium** |
| Recommendation policy service | New application policy + unit tests | **Medium** |
| Company plan header (`Year/Month/Goal/Status`) | New table + DAL | **Medium** |
| Portal planning UI (read + scenarios) | New view/components | **Medium** |
| Persist selected targets | Requires write path | **High** if Portal write; **Low–Medium** if Desktop-assisted |
| Principal allocation on save | New writer logic on top of SM6 model | **Medium** |
| Risk classification | Policy on top of expected vs target | **Low–Medium** |
| Planning lock + reopen | New lifecycle, permissions, history | **High** |
| Monthly visit execution rollup | New field-activity aggregation | **High** |
| Territory potential model | New domain concept + data | **High** (and currently undefined) |
| Worker / background jobs | Prefer compute during existing salesman/sales snapshot refresh | **Low–Medium** |
| Portal architecture exception (business writes) | Breaks “read-only analytics” principle | **High** (product decision) |

### 7.2 Database

**Likely new tables (full vision):**

| Table (conceptual) | Purpose | MVP? |
| ------------------ | ------- | ---- |
| `BTR_SalesTargetPlan` | Company goal, status, finalized metadata for period | Yes (or Phase 1.5) |
| `BTR_SalesTargetPlanItem` | Optional draft selections / scenario chosen before commit | Optional |
| `BTR_SalesTargetPlanHistory` | Locked snapshot of decisions | Phase 2+ |
| Existing `BTR_SalesPersonPrincipalTarget` | Remains execution source of truth | Yes — reuse |

**MVP alternative:** Skip plan tables initially; compute recommendations read-only in Portal and continue saving in SM6. Lower architecture impact, weaker portfolio-goal persistence.

### 7.3 APIs

| API | MVP need |
| --- | -------- |
| `GET` planning context (company + salesman cards/table) | Yes |
| `POST/PUT` company goal / draft selections | Only if Portal becomes writer |
| `POST` finalize / reopen | Phase 2 |
| Existing salesman trend / principals | Reuse |

### 7.4 Worker / processing

Prefer **no dedicated worker** for MVP:

- Recommendations computed on read from RepHistory + current snapshot, or
- Precomputed during existing Salesman snapshot refresh into optional planning columns

Dedicated overnight planning jobs only needed if explanation/risk models become heavy.

### 7.5 Architectural principle conflict

From Portal architecture:

> Portal remains read-only — no transactional / business POST-PUT-DELETE.

Sales Target Planning as an Owner decision tool **wants writes**. Options:

| Strategy | Pros | Cons | Recommendation |
| -------- | ---- | ---- | -------------- |
| S1. Portal read-only advisor; save in SM6 | Honors architecture | Split UX; friction | **MVP default** |
| S2. Explicit Portal planning-write exception for master planning data only | Best UX | Policy change; auth/audit | Phase 2 if MVP proves value |
| S3. Desktop planning redesign only | No Portal write debate | Owners increasingly live in Portal | Poor strategic fit |

---

## 8. Risks & Constraints

| # | Risk / constraint | Severity | Mitigation |
| - | ----------------- | -------- | ---------- |
| R1 | Portal read-only vs planning writes | High | MVP advisor + SM6 save; decide write exception later |
| R2 | Principal-level source of truth vs rep-level planning UX | High | Allocate by prior Principal mix on save |
| R3 | Untrusted / volatile recommendations | High | Deterministic formulas, clamps, Custom always available |
| R4 | Card UX does not scale | Medium | Hybrid table + drawer |
| R5 | Locking conflicts with mid-month revision culture | Medium | Soft finalize first; hard lock later |
| R6 | Visit / territory signals promised but unavailable | Medium | Exclude from MVP scope explicitly |
| R7 | Dual target stores (legacy + Principal) | Medium | Always resolve via existing target resolver |
| R8 | Early-month forecast noise feeding next-month plan | Medium | Prefer completed-month history for Baseline; use current forecast only as header context |
| R9 | Owner ignores tool and still copies previous month | Medium | Make gap-to-goal and delta-vs-baseline unavoidable in UI |
| R10 | Scope creep into generic Planning platform | Medium | Keep Sales-scoped MVP; share lifecycle patterns later |

---

## 9. Recommended MVP

### 9.1 MVP goal

Deliver the smallest feature that changes target setting from **blind entry** into **informed decision**, without rewriting Portal architecture or inventing unavailable metrics.

### 9.2 MVP scope (in)

1. **Planning period** = next calendar month (selectable).
2. **Company summary strip**
   - Prior month actual
   - Prior month achievement %
   - Current month forecast (from SA02)
   - Editable company goal (session or plan header)
   - Assigned sum + gap
3. **Salesman planning table** with expandable detail:
   - Last month actual, 3-month average, best month YTD
   - Customer / active customer counts
   - Piutang risk indicator (existing SF01 signals)
   - Safe / Normal / Stretch suggestions
   - Custom amount
   - Advisory difficulty vs baseline
4. **Explainable reasons** (1–3 bullets) per suggestion.
5. **Bulk actions:** Apply Normal to all missing; filter incomplete.
6. **Persistence path (MVP):** Export/deep-link guidance to SM6 **or** “recommended amounts” panel that staff enter/confirm in SM6.
   - Preferred technical MVP if a write exception is approved: save allocated Principal targets through a narrow planning API.
7. **Completeness + portfolio gap** as first-class outcomes.

### 9.3 MVP scope (out)

- Hard planning lock / reopen workflow
- Monthly visit completion on cards
- Territory potential
- ML forecasting
- Principal-level scenario cards
- Alert Center integration
- Auto-finalization from company goal without Owner review

### 9.4 MVP success criteria

- Owner can set next-month company goal and see assignment gap.
- Every active salesman has a suggested Normal target with visible rationale.
- Missing-target count drops after planning ritual.
- At least one planning cycle completed without spreadsheet side process.
- No silent auto-write of targets without confirmation.

---

## 10. Recommended Roadmap

### Phase 1 — MVP: Informed Target Planning (Advisor + Allocation)

- History-derived baselines and Safe/Normal/Stretch
- Company goal + portfolio gap
- Hybrid table UX
- Persist via SM6 (or limited Portal write if approved)
- Principal allocation by prior mix

**Outcome:** Better decisions; architecture mostly preserved.

### Phase 2 — Planning Execution Loop

- Portal write exception for planning master data (if S1 friction is real)
- Soft finalize + plan snapshot (immutable copy of decided targets)
- Per-salesman expected sales + stronger difficulty model
- Planned vs actual evaluation view at month-end
- Optional monthly visit-execution signal as explanation input
- Alert: plan incomplete / large gap unresolved near month start

**Outcome:** Closed loop Planning → Execution → Evaluation.

### Phase 3 — Future Vision: Sales Planning & Execution Capability

- Hard lock + controlled reopen
- Negotiation notes / Owner comments per salesman
- Principal-aware planning drill-down
- Peer-relative stretch using Entity Analytics percentiles
- Cross-domain planning (sales target vs collection capacity vs inventory supply constraints)
- Executive “Plan health” card

**Outcome:** Broader Planning & Execution platform with Sales as the first domain.

```text
Phase 1: Decide with context
    ↓
Phase 2: Commit, track, evaluate
    ↓
Phase 3: Govern and cross-constrain
```

---

## 11. Final Verdict

### Verdict: **GO WITH REDUCTIONS**

### Reasoning

1. **Business value is real.** Moving from entry-only SM6 to context-aware planning matches how owners actually plan and fills a clear Portal maturity gap after Sales Forecast.
2. **Core data is sufficiently available.** Rep history, portfolio, piutang risk, company forecast, and target resolution already exist. Safe/Normal/Stretch can be deterministic and explainable.
3. **Full vision is not yet feasible as one release.** Portal write architecture, planning lock/versioning, monthly visit rollups, and territory potential are material blockers or open product inventions.
4. **UX must be reduced.** Cards alone will not scale; hybrid table + detail is the feasible pattern.
5. **A reduced MVP still delivers the strategic repositioning:**  
   `Historical Performance → Recommendation → Owner Decision → (existing) Execution → Achievement Evaluation`.

### Conditions for staying on the GO path

- Keep recommendations deterministic and transparent.
- Respect Principal-level target storage (allocate, don’t invent a second truth).
- Explicitly defer lock, visit-monthly, and territory potential.
- Decide write strategy early (SM6-assisted MVP vs approved Portal planning-write exception).

### When this would become CONDITIONAL GO or NO GO

| Trigger | Shift verdict to |
| ------- | ---------------- |
| Product insists on hard lock + Portal writes + visit/territory signals in v1 | **CONDITIONAL GO** (needs architecture exception + extra analytics first) |
| Product rejects any recommendation model and only wants richer entry UI | Still valuable, but reframe as UX enhancement — not this vision |
| No willingness to allocate Principal targets from rep decisions | **CONDITIONAL GO** (SM6 UX conflict unresolved) |
| Expectation of AI-predicted targets as the core value | **NO GO** for current Portal analytics philosophy |

---

## Appendix A — Metric classification quick reference

| Metric / capability | Classification |
| ------------------- | -------------- |
| Company current-month forecast | Available |
| Company prior actual / achievement | Available with minor work |
| Company goal + growth % | Requires new analytics |
| Last month / 3-mo avg / best month (salesman) | Available with minor work |
| Per-salesman current-month forecast | Requires new analytics |
| Customer / active customer counts | Available |
| Piutang / overdue risk | Available |
| Monthly visit completion | Requires new analytics |
| Territory potential | Not currently feasible |
| Safe / Normal / Stretch engine | Requires new analytics |
| Explanation reasons | Requires new analytics (pattern exists) |
| Difficulty / risk class | Requires new analytics |
| Portfolio balancing UI | Requires new analytics (sum available) |
| Planning lock / reopen | Requires new analytics |
| Portal target write | Not currently feasible (architecture) |
| Desktop SM6 target write | Available |

## Appendix B — Suggested next artifact

If Product Owner accepts **GO WITH REDUCTIONS**:

1. Analyst: `docs/features/btr-portal/sales-target-planning/feature.md` (MVP scope only)
2. Architect: `docs/work/btr-portal/sales-target-planning/implementation-plan.md`
3. Explicit decision record on **Portal write exception vs SM6-assisted save**
