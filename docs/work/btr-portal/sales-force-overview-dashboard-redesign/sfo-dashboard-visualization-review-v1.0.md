# Sales Force Overview — Visualization Strategy Review

## v1.0

> Scope: **visualization strategy only.** This document does not redesign the
> dashboard. It reviews the four salesman ranking charts and recommends a
> revised visualization architecture.
>
> Supersedes: nothing. Complements:
> `sfo-dashboard-ux-blueprint-v2.2.md`
>
> Inputs:
> - `sfo-dashboard-ux-blueprint-v2.2.md`
> - `docs/features/btr-portal/dashboard-11-sf02-sales-force-overview.md`
> - Implemented UI: `btr.portal.web/src/views/dashboard/FieldActivityOverviewView.vue`
>   and `btr.portal.web/src/components/field-activity/*`
>
> Status: **Review / decision document.** No implementation in this pass.

---

# 0. Critical Finding — Read This First

Two facts change the nature of this problem. Neither is visible in the
blueprint alone.

## 0.1 The scoreboard already exists

The dashboard already contains a multi-measure, per-salesman, sortable
comparison table:

- Component: `FieldActivitySalesmanTable.vue`
- Placed in Section C "Outcomes", **below** all four ranking charts
- Columns: Rank, Code, Name, Planned, Actual, Execution %, Effective,
  Eff. Rate, Missed, Unplanned, GPS Valid %, Orders, Order Value, Status

Every measure shown by the four ranking charts is already a column in that
table. The four charts are therefore **not** the only place to compare
salesmen — they are the *fourth, fifth, sixth and seventh* place.

Full duplication map:

| Measure | Status card | Ranking chart | Salesman table | Rankings payload | Trends |
| --- | --- | --- | --- | --- | --- |
| Visit Execution % | yes | **chart 1** | yes | Top/Bottom | yes |
| Effective Call Rate | yes | **chart 2** | yes | Top/Bottom | yes |
| Orders | yes | **chart 3** | yes | Top | yes |
| Order Value | yes | **chart 4** | yes | Top | yes |

**The problem is not only "KPI silos". It is four-fold redundancy.** The four
charts are the least information-dense copy of data that exists in four other
places. Any redesign that keeps them in any form is optimizing a redundant
surface.

## 0.2 The blueprint's funnel was never built

Blueprint v2.2 §B.1 specifies an Execution Funnel and marks it "(unchanged)",
implying it exists. It does not. Section B is implemented as three ranking bar
charts (`FieldActivityOverviewView.vue:256-280`), and there is no funnel
component anywhere in `components/field-activity/`.

Consequence: **Problem 3 (Missing Business Narrative) has an already-approved,
already-specified, not-yet-built answer.** The narrative object is a known gap,
not a new design question.

## 0.3 Why the scroll problem is structural, not cosmetic

`FieldActivityComparisonChart.vue:27` — `chartHeight = max(224, items.length * 26)`
`FieldActivityComparisonChart.vue:112-115` — wrapper `max-height: 480px; overflow-y: auto`

With a horizontal bar chart (`indexAxis: 'y'`), the value axis is rendered at
the **bottom of the canvas**. With 16 salesmen the canvas is 416–520px inside a
480px scroll box, so the axis is reachable only by scrolling — and scrolling to
it pushes rank #1 out of view. This is exactly the reported symptom, and it
cannot be fixed by tuning heights: any N-salesman ranked bar chart in a bounded
box has this failure mode.

There are also **four independent scroll containers**. There is no way to view
salesman X in all four charts simultaneously.

Finally: presentation mode exists (`presentationStore.isPresentationActive`).
A 480px inner scroll box is unusable on a projector. Scrolling is not a
meeting-safe interaction.

---

# 1. Visualization Review — The Four Current Charts

## 1.1 What a descending ranked bar chart actually encodes

All four charts sort descending and render every bar in a single flat colour
(`#2563eb`, `FieldActivityComparisonChart.vue:36`).

Once a bar chart is sorted descending:

- Position = rank (readable from position alone)
- Length = value (monotonic in position)

So the bar length adds only **interval** information — "how big is the gap
between #3 and #4" — at a cost of 26px of vertical space per salesman per
chart. A descending single-measure bar chart is a **ranked list rendered at
chart cost**.

That interval information is real and is the charts' only genuine advantage.
Any replacement must preserve it. (Ours does — see §2.3, in-cell data bars.)

## 1.2 Vertical cost, measured

For a 16-salesman team, at ~1,600px content width where the auto-fit grid
places three charts per row:

| Block | Approx. height |
| --- | --- |
| Section B — 3 ranking charts | ~530px |
| Section C — Order Value chart | ~530px |
| Section C — Salesman table (16 rows) | ~800px |
| Section C — Wilayah chart | ~350px |
| **Total for salesman comparison content** | **~2,200–2,500px** |

One scoreboard row carries all four measures in ~40px. The four charts consume
~104px of canvas per salesman to carry what one 40px row carries.

**Screen efficiency verdict: the four charts are roughly a 4× vertical
overpayment for the same information.**

## 1.3 Chart-by-chart assessment

### Chart 1 — Visit Execution % (Section B)

| Dimension | Assessment |
| --- | --- |
| Question answered | Who followed the route plan? |
| Answered elsewhere? | Yes — Status card, table column, `BottomVisitExecution` ranking, "Needs Plan Review" |
| Unique contribution | None beyond interval shape |
| Verdict | **REMOVE as standalone chart** |

### Chart 2 — Effective Call Rate (Section B)

| Dimension | Assessment |
| --- | --- |
| Question answered | Whose visits convert? |
| Answered elsewhere? | Yes — Status card, table column, `BottomEffectiveCallRate` ranking, "Needs Coaching" |
| Unique contribution | None beyond interval shape |
| Verdict | **REMOVE as standalone chart** |

Note: this is the measure with the strongest management content (it is the
quality measure) and it currently has the weakest home. It deserves a better
position — the funnel conversion stage — not a bigger chart.

### Chart 3 — Orders Generated (Section B)

| Dimension | Assessment |
| --- | --- |
| Question answered | Who generates demand volume? |
| Answered elsewhere? | Yes — Status card, table column, `TopOrders` ranking, trend |
| Unique contribution | Least of the four: Orders is heavily correlated with Order Value (both ≈ Effective Calls × basket). Ranking the full population twice on two near-collinear measures is the weakest redundancy here. |
| Verdict | **REMOVE as standalone chart** |

### Chart 4 — Order Value (Section C)

| Dimension | Assessment |
| --- | --- |
| Question answered | Two questions: (a) who earns most, (b) **is revenue concentrated?** |
| Answered elsewhere? | (a) Yes, everywhere. (b) **No.** |
| Unique contribution | **Yes — concentration / key-person risk.** Blueprint §C.1 "Revenue Distribution" and the success criterion "Is revenue concentrated in a few individuals?" both depend on this chart, because C.1 was never built either. |
| Verdict | **REPLACE, do not delete.** Shrink to a compact concentration strip. |

This is the one chart that must be handled with care. Deleting it silently
deletes the concentration question.

## 1.4 Summary verdict

| Chart | Action |
| --- | --- |
| Visit Execution % | **Remove** |
| Effective Call Rate | **Remove** |
| Orders Generated | **Remove** |
| Order Value | **Replace** with a compact Revenue Concentration strip |
| `FieldActivityComparisonChart.vue` | **Deprecate** after removal (no remaining caller) |

---

# 2. Recommended Visualization Architecture

## 2.1 Option A — Sales Team Funnel

### Evaluation

| Question | Answer |
| --- | --- |
| Suitable for executive users? | **Yes — best of the four options.** Four to five numbers, three to four ratios, zero scrolling, no interaction needed. It is the only object on the dashboard an owner can read correctly in five seconds. |
| Better mental model? | **Yes, decisively.** It is literally the Visit → Effective → Order → Revenue chain. It supplies the *shared denominator* the four charts lack: every stage is expressed as a conversion of the previous one, so Activity, Productivity and Outcome stop being four unrelated numbers. |
| Actions derivable? | **Yes, and they are unambiguous.** Each leak names its own intervention (see below). |
| Recommendation | **ADOPT — primary Section B object.** |

### Design defect in the blueprint funnel — must be corrected

Blueprint §B.1 specifies four stages:

```text
Planned Visits → Actual Visits → Orders → Revenue
```

This omits **Effective Calls** — which is precisely the stage that Effective
Call Rate measures. As specified, the funnel cannot house the dashboard's most
important quality measure, and the "Productivity" step of the user's required
narrative (Activity → Productivity → Outcome → Action) has no stage.

**Recommended five-stage funnel:**

```text
Planned Visits
      ↓  Execution Rate = Actual / Planned
Actual Visits
      ↓  Effective Call Rate = Effective Calls / Actual Visits
Effective Calls
      ↓  Order Conversion = Orders / Effective Calls
Orders
      ↓  Average Order Value = Order Value / Orders
Order Value
```

### Metrics per stage

| Stage | Primary value | Conversion out | Leak (absolute) |
| --- | --- | --- | --- |
| Planned Visits | `TeamKpis.PlannedVisits` | — | — |
| Actual Visits | `TeamKpis.ActualVisits` | Execution Rate | Missed Visits |
| Effective Calls | `TeamKpis.EffectiveCalls` | Effective Call Rate | No-Order Visits |
| Orders | `TeamKpis.TotalOrders` | Order Conversion | Calls without order |
| Order Value | `TeamKpis.TotalOmzet` | Avg Order Value | — |

**Two rules:**

1. **Show the leak, not just the rate.** "Execution 88.3%" is a judgement.
   "42 planned customers not visited" is a worklist. Managers act on leaks.
2. **Show rate and volume together.** 100% execution on a 3-visit plan is not
   100% execution on a 30-visit plan. Each stage must display both.

### Manager actions derived

| Largest leak | Diagnosis | Action |
| --- | --- | --- |
| Planned → Actual | Plan realism, capacity, supervision | Review route plans; verify headcount |
| Actual → Effective Calls | Selling quality | Coaching; check stock, price, assortment |
| Effective Calls → Orders | Order capture | Check order entry, credit blocks, stock-outs |
| Orders → Order Value | Basket size / mix | Review discounting and product mix |

### Shape: not a trapezoid

Do **not** use a Chart.js funnel/trapezoid. Trapezoids distort area vs. height,
cannot legibly carry five stages plus rates plus leaks, and have no existing
implementation here.

**Use a horizontal conversion strip**: five compact stage blocks, each with
value + conversion % + leak, separated by arrows, rendered in plain markup with
CSS. It is cheaper to build, prints and projects cleanly, and every number is
exact rather than estimated from an area.

### Data and cost

No new API. All values derive from `TeamKpis` (`PlannedVisits`, `ActualVisits`,
`EffectiveCalls`, `MissedVisits`, `TotalOrders`, `TotalOmzet`) — all already
returned.

**Complexity: Medium.** **Business value: High.** It closes a documented
blueprint gap (§B.1) and is the narrative anchor the whole redesign depends on.

---

## 2.2 Option B — Salesman Scoreboard

### Evaluation

| Question | Answer |
| --- | --- |
| Superior to four separate charts? | **Yes, decisively.** It is the direct answer to "compare salesmen, not KPIs" and to the user's Problem 1. |
| Recommendation | **ADOPT — and promote to primary.** |

**This is a promotion and redesign, not a new build.** The component exists
(`FieldActivitySalesmanTable.vue`). Cost is materially lower than a new
artifact.

### The one move that makes it a scoreboard rather than a table

The table's column order must **become the funnel**. Group the columns under
three headers:

```text
ACTIVITY                │ PRODUCTIVITY        │ OUTCOME              │ SIGNAL
Planned · Actual · Exec%│ Effective · Eff.Rate│ Orders · Order Value │ Attention
```

This is the highest-leverage change in the entire review: the Activity →
Productivity → Outcome narrative is reinforced by the column order at **zero
additional pixels** and zero additional interaction. The manager reading one
row left-to-right is reading the funnel for one salesman.

### Required changes

| # | Change | Rationale |
| --- | --- | --- |
| 1 | Move from Section C to Section B, directly under the funnel | Makes "narrative → individual comparison" contiguous |
| 2 | Add funnel column groups (Activity / Productivity / Outcome / Signal) | Column order = mental model |
| 3 | Add **in-cell data bars** for the four headline measures | Recovers the interval/shape information that was the ranking charts' only advantage. Bar scaled to column max, so the distribution shape is still scannable down the column. |
| 4 | Keep existing band colouring | Reuse `services/fieldActivityKpiBands.ts` — no new thresholds |
| 5 | Add **Attention / Signal column** | "Who needs action" visible inline, not only in Section D. Reuse `commercialSignalRules.ts` tags and their existing rationale strings. |
| 6 | User-selectable **"Rank by"** | See below |
| 7 | Sticky header, ~10 visible rows, internal scroll | Kills the scroll/X-axis problem definitively: the header (the "axis") is always visible |

### Ranking: how it should work

**Default sort: Order Value descending** (outcome-first). Management's question
is "who produced", and the funnel has already established the upstream
conversion context.

**"Rank by" selector — limited to five options:**

1. Order Value
2. Orders
3. Effective Call Rate
4. Visit Execution %
5. Attention (worst first)

Not free-form. A configurable ranking with more than ~5 options becomes a
configuration burden and reintroduces the silo problem by letting users live in
one measure.

### Composite scoring: **No.**

Recommendation: **do not add a weighted composite index.** Three reasons:

1. **Blueprint §2.3 forbids it:** "never blend the three into a single
   composite score. A blended index hides the sales/collection trade-off and
   misguides incentives."
2. **Weights are arbitrary and contested.** The first time a manager asks "why
   is he ranked 3rd?", the index loses the argument and trust in the dashboard
   erodes. An index must be defensible at 7am in a sales meeting.
3. **It destroys the tension the dashboard exists to show** — the
   revenue-hero / collection-villain profile is visible *because* the
   dimensions are not blended.

**Use these two substitutes instead. Both give the "one overall verdict" benefit
without an opaque number:**

**(a) Rule-based Attention badge.** Transparent, binary, explainable. Each badge
carries its own rationale, and the rationale strings already exist in
`commercialSignalRules.ts` (`COMMERCIAL_SIGNAL_RATIONALES`). A badge can be
defended; a 73.4 index cannot.

**(b) Per-cell deviation vs. team median.** Show ▲ / ● / ▼ against the team
median for each measure. This is the correct way to make measures comparable
*without blending them* — each measure is normalised against its own
population, so a row can show "above median on execution, below on conversion"
in one glance. It delivers cross-measure comparability while keeping every
dimension distinct and honest.

### Guardrail

Row click → salesman detail must be preserved (it already exists). Charts were
a drill affordance; the scoreboard is a better one.

**Complexity: Medium.** **Business value: High.**

---

## 2.3 Option C — Performance Quadrant (Revenue × Effective Call Rate)

### Evaluation

**Business insight it reveals:** it separates **volume-driven** performance from
**conversion-driven** performance. This is the one question no ranking chart and
no sorted table can answer.

| Quadrant | Profile | Action |
| --- | --- | --- |
| High revenue, high ECR | **Model performers** — skilled | Replicate practice; expand territory |
| High revenue, low ECR | **Volume grinders** — buy revenue with coverage | Do not hold up as a skill model; check basket/mix; fragile if route shrinks |
| Low revenue, high ECR | **Under-served talent** — capacity constrained | **Highest coaching ROI.** Give them more accounts. |
| Low revenue, low ECR | Coaching or role-fit issue | Structured coaching / review |

The decisive argument for the quadrant: **the bottom-left-ish middle of both
rankings hides the best development bet in the company.** A salesman ranked 9th
on revenue and 7th on ECR is invisible in every ranking view, yet is the single
highest-return coaching investment. Only the quadrant makes him visible.

For **owners**, the cloud shape is the insight: if the population sits low and
to the right, the organisation is buying revenue with visit volume, which is
expensive and fragile.

| Question | Answer |
| --- | --- |
| Suitable for owners? | **Yes** — strong single-glance signal on how revenue is being produced. |
| Suitable for sales managers? | **Only if every point is named and clickable.** An unnamed scatter is unusable for someone who must act on a specific person. |
| Should it replace ranking charts? | **Partially** — it justifies retiring the Orders and Order Value rankings. It must **not** replace the scoreboard. |
| Recommendation | **ADOPT as SECONDARY — Phase 2, as a view toggle on the scoreboard, not a new section.** |

### Two mandatory guards (non-negotiable if built)

1. **Minimum-visit guard.** Effective Call Rate is unstable at low volume: 2
   visits and 1 order = 50%. Without a guard, the quadrant promotes noise to
   the top-right. Encode **Actual Visits as bubble size** and visually flag or
   exclude salesmen below a minimum visit count.
2. **No scatter component exists** anywhere in the portal codebase. This is a
   net-new chart type with real costs: label collision at 16–40 points,
   tooltip design, axis scaling for IDR, and the guard above.

**Complexity: Medium–High. Business value: Medium** (high for owners, low for
the daily manager who must act today).

Deploy as a **"Table │ Quadrant" view toggle** over the same data. That way it
adds a strategic lens without adding a competing section or a second place
salesmen appear.

---

## 2.4 Option D — Top / Bottom Action Lists

### Evaluation

The Action Center already exists and is well designed
(`FieldActivityGroupedActionCenter.vue`, three groups: Field Execution,
Commercial Risk, Recognition). The real question is whether action lists should
*replace* ranking charts.

**Answer: yes, for the bottom tail.** Once the scoreboard exists, "bottom 5
execution" is a sort operation. The ranking charts were the *third* copy of that
information (table + rankings payload + chart).

**But do not collapse the Action Center into "just sort the table".** Its unique
value is **the action label and the drill-across target** — "Needs Credit Review
→ Collection Dashboard" — which no sort can express.

| Question | Answer |
| --- | --- |
| Should action lists replace ranking charts? | **Yes** for bottom-tail analysis. |
| Recommendation | **KEEP AND STRENGTHEN. Do not replace with sorting.** |

### Signals that belong (final set)

| Group | Signal | Trigger |
| --- | --- | --- |
| Field Execution | Needs Immediate Attention | Zero activity today — **always first**: time-decaying urgency |
| Field Execution | Needs Plan Review | Bottom-quartile visit execution |
| Field Execution | Needs Coaching | Bottom-quartile effective call rate |
| Field Execution | Needs Investigation | High unplanned visits **and** GPS anomalies |
| Commercial Risk | Needs Collection Action | Top-quartile overdue exposure |
| Commercial Risk | Needs Credit Review | High revenue **and** high overdue |
| Commercial Risk | Needs Escalation | Aging >90 / legacy debt |
| Recognition | Recognition Candidates | High revenue + high ECR + healthy portfolio |

### Enhancements

1. **Cap each list at 5**, with "see all in scoreboard". List bloat is the
   failure mode the blueprint itself warns about (§6.1, folding "Needs
   Follow-up" into "Needs Investigation" to control it).
2. **Per-item reason line**, not just a category action. The pattern already
   exists (`contextText` on investigation items shows GPS context). Extend it:
   "Needs Coaching — 18% ECR vs team 70%". A reason converts a label into a
   conversation opener.
3. **Order lists by urgency, not by severity of value:** zero activity →
   commercial risk → coaching.

### Rejected addition

"Most improved vs 7-day average" is a strong recognition signal but is **not
computable from current data** — `FieldActivityTrendPoint` is team-level with no
salesman dimension. Noting as a data dependency, not a recommendation.

**Complexity: Low** (enhancements only). **Business value: Medium.**

---

# 3. Updated Dashboard Flow

## 3.1 Recommended reading order

```text
STATUS          →  10 seconds. Is today healthy? (selling AND collecting)
    ↓
FUNNEL          →  30 seconds. Where are we losing the day?
    ↓
SCOREBARD       →  60 seconds. Who is strong, who is weak, on which dimension?
    ↓
ACTION CENTER   →  Decide and act. Who do I call, and about what?
    ↓
CONTEXT         →  Is revenue concentrated? Which territory? (geographic lens)
    ↓
TRENDS          →  Direction. Improving or declining?
    ↓
DETAIL          →  Evidence, per salesman
```

## 3.2 Section mapping

| Order | Section | Content | Change |
| --- | --- | --- | --- |
| 1 | **A. Status** | KPI strip + Commercial Health + alert chip | Unchanged |
| 2 | **B. Performance** | **Execution Funnel** (new) + **Salesman Scoreboard** (promoted) | Funnel new; scoreboard moved from C; **3 ranking charts removed** |
| 3 | **D. Action Center** | 3 groups, capped at 5, per-item reasons | Moved up; minor enhancements |
| 4 | **C. Outcomes** | Revenue Concentration strip + Wilayah/Territory | Order Value chart replaced; moved below Action Center |
| 5 | **E. Trends** | 7/30-day sales + collection trends | Unchanged |

## 3.3 Why C and D are inverted

This is the **one deliberate change to the v2.2 section order**, and it is
justified by the blueprint's own stated principle:

> §6.2 — "Action-first, grouped. Managers spend time on actions, not on
> diagnosing."

Placing Action Center immediately after the scoreboard makes **"compare →
act"** a single contiguous motion on the same salesman population. Territory
and concentration then become the *context* you consult when an action needs
geographic interpretation — e.g. "Needs Collection Action" for a salesman in a
territory that also shows high overdue exposure.

This is not a renumbering to the domain model, which Appendix A.1 correctly
rejected. It is a reordering of two existing sections in favour of
actionability, which Appendix A.2 already establishes as the governing
preference ("prefer management actionability over metric completeness").

## 3.4 Story check

The flow now tells one story, and each step earns its place:

```text
Activity   → Status (are they out?) + Funnel stage 1-2
Productivity → Funnel stage 2-3 (ECR) + Scoreboard Productivity group
Outcome    → Funnel stage 4-5 + Scoreboard Outcome group + Concentration
Action     → Action Center, grouped by intervention type
```

The manager never has to hold four charts in working memory to evaluate one
salesman. One row does it.

---

# 4. Screen Layout Recommendation

Not pixel-level. Vertical budget and hierarchy only. Estimated for a
16-salesman team at ~1,600px content width.

```text
┌────────────────────────────────────────────────────────────────────────┐
│ HEADER — title, date preset, refresh, freshness            (~90px)     │
├────────────────────────────────────────────────────────────────────────┤
│ A. STATUS — 6 KPI cards + Commercial Health pair + alert   (~140px)    │
│    Revenue │ Visit Exec % │ Orders                                     │
│    Active │ Order Conv │ Wasted Visits │ Cash MTD │ Recovery %         │
├────────────────────────────────────────────────────────────────────────┤
│ B. EXECUTION FUNNEL — horizontal conversion strip          (~140px)    │
│    Planned → Actual → Effective → Orders → Order Value                 │
│    each stage: value + conversion % + absolute leak                    │
├────────────────────────────────────────────────────────────────────────┤
│ B. SALESMAN SCORECARD — PRIMARY COMPARISON SURFACE        (~430px)     │
│    toolbar: [Rank by ▾] [search] [Table │ Quadrant°]                   │
│    ── sticky header ────────────────────────────────────────────────   │
│    # │ Code/Name │  ACTIVITY       │ PRODUCTIVITY   │ OUTCOME  │ SIG   │
│      │           │ Plan Act Exec%  │ Eff  Eff.Rate  │ Ord Value│       │
│    1 │ ...       │ ▓▓▓ ...         │ ▓▓ ...         │ ▓▓▓ ...  │  ✓    │
│      ~10 rows visible, internal scroll, sticky header                  │
├────────────────────────────────────────────────────────────────────────┤
│ D. ACTION CENTER — 3 columns, 5 items each                (~260px)     │
│    FIELD EXECUTION      │ COMMERCIAL RISK      │ RECOGNITION           │
│    Immediate Attention  │ Collection Action    │ Candidates            │
│    Plan Review          │ Credit Review        │                       │
│    Coaching             │ Escalation           │                       │
│    Investigation        │                      │                       │
├────────────────────────────────────────────────────────────────────────┤
│ C. CONTEXT — 2 columns                                    (~300px)     │
│    Revenue Concentration (1/3)      │ Territory Performance (2/3)      │
│    Top 3 = 66% of order value       │ measure-group toggle             │
│    single 100% stacked bar          │ Sales Activity │ Financial Health │
├────────────────────────────────────────────────────────────────────────┤
│ E. TRENDS — 7/30-day toggle                               (~300px)     │
└────────────────────────────────────────────────────────────────────────┘

° Quadrant toggle = Phase 2 (§2.3)
```

## 4.1 Layout principles

| # | Principle |
| --- | --- |
| 1 | **One primary comparison surface.** Salesmen appear in exactly one ranked, comparable place. Everywhere else they appear as an action, a territory, or a trend. |
| 2 | **Full width for the scoreboard.** It is the busiest object; do not split it with a side-by-side chart. |
| 3 | **Sticky headers over scrolling canvases.** The scoreboard header is the axis. It never scrolls away. This is the structural fix for Problem 2. |
| 4 | **Context below decision.** Concentration and territory inform action; they do not precede it. |
| 5 | **Nothing scrolls except the scoreboard.** One scroll context, not five. |
| 6 | **Every row is drillable.** Preserve the click-to-detail affordance the charts provided. |

## 4.2 Estimated effect

| | Current | Proposed |
| --- | --- | --- |
| Salesman comparison content | ~2,200–2,500px | ~570px (funnel + scoreboard) |
| Independent scroll containers | 5 | 1 |
| Places a manager must look to evaluate one salesman | 4 charts | 1 row |
| Projector / presentation-mode safe | No (inner scroll boxes) | Yes |

---

# 5. Migration Impact

## 5.1 Change register

| # | Visualization | Action | Complexity | Business value | New data required |
| --- | --- | --- | --- | --- | --- |
| 1 | Visit Execution % bar chart | **Remove** | Low | Medium | No |
| 2 | Effective Call Rate bar chart | **Remove** | Low | Medium | No |
| 3 | Orders Generated bar chart | **Remove** | Low | Medium | No |
| 4 | Order Value bar chart | **Replace** with Revenue Concentration strip | Low–Med | Med–High | No (derived from `Salesmen[].OmzetAmount`) |
| 5 | `FieldActivityComparisonChart.vue` | **Deprecate** (no callers after 1–4) | Low | Low | No |
| 6 | **Execution Funnel** (team, 5-stage) | **Introduce — primary** | Medium | **High** | No (`TeamKpis` sufficient) |
| 7 | **Salesman Scoreboard** | **Redesign + promote from C to B** | Medium | **High** | No (adds Attention column, reuses `commercialSignalRules`) |
| 8 | Action Center | Keep + enhance (cap 5, per-item reasons, urgency order) | Low | Medium | No |
| 9 | Revenue Concentration strip | **Introduce — secondary** | Low–Med | Medium | No |
| 10 | Wilayah / Territory chart | Keep; add Financial Health measure group (blueprint §C.2) | Low | Medium | No |
| 11 | Trends | Keep | — | — | No |
| 12 | **Quadrant view** (Revenue × ECR) | **Introduce — Phase 2, secondary toggle** | Med–High | Medium | No (but needs min-visit guard) |

**No API change is required for any item.** Every value is already returned by
the overview endpoint. This is a pure presentation-layer migration.

## 5.2 Phasing

### Phase 1 — Consolidate (recommended, do this)

1. Build the Execution Funnel (5-stage conversion strip)
2. Redesign and promote the Salesman Scoreboard (column groups, data bars,
   Attention column, Rank-by selector, sticky header)
3. Remove the three ranking charts
4. Replace the Order Value chart with the Revenue Concentration strip
5. Move Action Center above Outcomes
6. Deprecate `FieldActivityComparisonChart.vue`

**Outcome:** Problems 1, 2 and 3 are all resolved. Highest value per unit of
complexity in the entire programme.

### Phase 2 — Deepen (optional)

7. Quadrant view toggle on the scoreboard
8. Action Center per-item reason lines and recognition refinement

## 5.3 Risks and guardrails

| Risk | Guardrail |
| --- | --- |
| Deleting the Order Value chart silently deletes the concentration question | Concentration strip is a Phase 1 item, not optional. Do not ship the removal without it. |
| Loss of drill-to-detail affordance | Scoreboard row-click already provides it. Verify before removing charts. |
| Composite-score pressure returns during design | Blueprint §2.3 rule stands: no blended index. Use rule-based Attention badges + per-cell deviation vs median. |
| Users resist losing familiar charts | The scoreboard is not a new object — it is a promoted, better-organised version of a table they already use (feature doc §5). |
| Quadrant promotes low-volume noise | Minimum-visit guard + bubble size = Actual Visits. Mandatory. |
| `Rankings` payload left orphaned | It is **not** orphaned: the Action Center consumes Top/Bottom sets. Do not remove from the model. |

## 5.4 Knowledge artifacts requiring update (mandatory)

This migration changes permanent knowledge. Per repository policy, code and
knowledge must remain consistent.

| Artifact | Change |
| --- | --- |
| `docs/features/btr-portal/dashboard-11-sf02-sales-force-overview.md` §6 "Salesman Comparison Charts" | Rewrite — charts removed; describe the scoreboard and funnel |
| Same, §2 "Management Reading Sequence" | Update to the new flow (§3.1) |
| Same, §5 "Salesman Performance Table" | Promote to primary comparison surface; document column groups and Rank-by |
| `sfo-dashboard-ux-blueprint-v2.2.md` §B.1 | Correct the funnel to five stages; remove the "(unchanged)" claim since it was never built |
| `sfo-dashboard-ux-blueprint-v2.2.md` §C.1 | Revenue Distribution → Revenue Concentration strip |
| `sfo-dashboard-ux-blueprint-v2.2.md` §3.1, Appendix B | Reflect the C/D inversion and the removed charts in the KPI placement matrix |

---

# 6. Decision Summary

| Question | Answer |
| --- | --- |
| Keep the four ranking charts? | **No.** Remove three, replace one. |
| Primary comparison surface? | **Salesman Scoreboard** (existing table, promoted and redesigned) |
| Primary narrative surface? | **Execution Funnel**, five stages, team level, with leaks |
| Composite score? | **No.** Rule-based Attention badges + per-cell deviation vs team median |
| Quadrant? | **Yes, but secondary and Phase 2**, as a view toggle with a minimum-visit guard |
| Action lists? | **Yes, keep and strengthen.** Cap at 5, add per-item reasons |
| Section order change? | **One:** Action Center moves above Outcomes |
| New API? | **None** |
| Net effect | Four siloed charts → one narrative + one comparison surface + one action list. ~4× less vertical space, one scroll context, one row per salesman. |
