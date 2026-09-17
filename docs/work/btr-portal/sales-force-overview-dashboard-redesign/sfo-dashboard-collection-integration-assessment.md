# Sales Force Overview — Collection Integration Assessment

## Business Management Review (v1.0)

> Scope: Reassess the Sales Force Overview (SFO) UX Blueprint v2.1 now that a
> Collection Dashboard capability is known to exist. This is a business
> management review, not a technical feasibility assessment. Both dashboards are
> assumed feasible.

---

## 0. Executive Summary

BTR's own product definition assigns **payment collection** to Sales Personnel
alongside visits, order collection, and invoice delivery (`docs/foundation/PRODUCT.md`).
In a distributor model, the field salesman is usually the primary — and often
only — relationship owner with the customer. That means sales and collection are
**two outcomes of the same customer relationship**, not two separate jobs.

The current SFO Blueprint is an excellent **sales execution** control dashboard.
It is not wrong — it is incomplete. It answers *"how well are salespeople
selling?"* but not *"how well are salespeople creating revenue while maintaining
portfolio health?"*

The correct response is **not** to rebuild the SFO around collection, and **not**
to leave the two dashboards fully siloed. The correct response is **moderate
integration**: keep the two dashboards separate, surface 3–5 collection signals
inside the SFO, extend Territory and the Action Center to include collection
health, and govern KPI ownership to avoid duplication.

---

## Part A — Business Model Review

### The statement

> A salesperson's effectiveness should be evaluated using both sales outcomes and
> collection outcomes.

### Verdict

**Valid in this business**, with important conditions. It is a contextual truth,
not a universal one.

### When the statement is valid

- The salesman owns the customer relationship end-to-end: visits, orders,
  invoice delivery, **and** payment collection (the case in BTR, per
  `PRODUCT.md`).
- The business runs on short-cycle credit or cash-and-carry where the salesman is
  the collection agent (typical distributor / FMCG / traditional-trade model).
- Compensation or incentives are — or should be — tied to both order value and
  cash recovered, not gross sales alone.
- Credit decisions are informed by the salesman's on-the-ground knowledge of the
  customer.

### When the statement is misleading

- When collection is actually owned by a **dedicated finance / AR team** and the
  salesman has no collection mandate — then penalizing a salesman on overdue
  would be unfair and demotivating.
- When overdue is **structural** (customer bankruptcy, systemic credit-policy
  errors, macro shock) rather than **behavioral** — conflating sales performance
  with credit risk blames the wrong party.
- When salesmen have **no authority over credit terms** and **no tools to
  collect** (no mobile payment capture, no handheld). You cannot hold someone
  accountable for an outcome they cannot influence.

### How owners should think

Sales performance and collection performance are **two distinct dimensions** of
the **same relationship**. They must be:

- **Measured separately** — so a problem is attributable (a revenue decline is
  not the same failure as a recovery decline).
- **Reviewed together** — so a salesman's total commercial contribution is
  visible. A salesman who books Rp 50 M in orders but collects nothing is
  destroying cash flow, not creating value.

The core danger is **single-metric fixation**: a salesman can look excellent on
revenue while quietly eroding portfolio health.

### Recommendation

Adopt a **two-dimensional effectiveness view** (Sales Productivity + Collection
Discipline + Portfolio Quality), but **never blend them into one composite
score**. A blended index hides the trade-off. Show the dimensions side by side
and let managers see the tension explicitly.

---

## Part B — Dashboard Boundary Review

### Option A — Completely separate dashboards

| Dimension | Assessment |
| --- | --- |
| Advantages | Clean ownership; each dashboard optimized for its own audience and cadence; lowest complexity; no cognitive overload. |
| Disadvantages | Fails to answer the integrated question; the owner must mentally stitch sales + collection together; salesman-level accountability is split across two places; creates a "revenue hero / collection villain" blind spot. |
| Complexity | Low |
| Management usefulness | Good for specialists; weak for the owner/sales manager who needs the full picture. |

### Option B — Separate dashboards with cross-dashboard indicators

| Dimension | Assessment |
| --- | --- |
| Advantages | Preserves each dashboard's focus; surfaces the handful of collection signals that change a sales manager's daily decisions; drill-across navigation connects the two; keeps each single-purpose. |
| Disadvantages | Risk of KPI duplication or inconsistent definitions if not governed; requires an explicit "hand-off" convention. |
| Complexity | Medium (governance + a few embedded signals). |
| Management usefulness | **High** — matches how an owner actually thinks: "selling is good, but are we getting paid?" |

### Option C — Single integrated dashboard

| Dimension | Assessment |
| --- | --- |
| Advantages | One view of full commercial health; zero context switching. |
| Disadvantages | Overloads a single page; mixes cadences (daily visits vs. aging buckets vs. MTD cash); different owners (sales manager vs. finance/AR) compete for the same real estate; the deliberately lean 5-section SFO would balloon; finance signals dilute the field-execution narrative. |
| Complexity | High |
| Management usefulness | High for an owner; poor for the operating managers who use these views daily. |

### Recommendation

**Option B — Separate dashboards with cross-dashboard indicators.**

Rationale: the two dashboards serve different decision cadences (daily field
execution vs. weekly/monthly receivable health) and different primary owners
(sales manager vs. finance/AR), so they should remain separate. But because the
**salesman** is the shared entity accountable for both, a small set of collection
signals must appear where the sales manager works every day. Cross-dashboard
signals + drill-across navigation deliver the integrated question without
rebuilding the SFO.

---

## Part C — KPI Integration Review

### C.1 Status — Operating Health

| Question | Answer |
| --- | --- |
| Belongs here? | **Yes — a single collection health headline.** Cash Collected MTD and Recovery vs Billing % are same-day/MTD reads that belong next to Revenue. Optionally an "Overdue exposure requires attention" flag. |
| Does NOT belong? | Aging buckets, overdue customer lists, payment mix breakdowns. Those are analysis, not status. |
| Why? | Status is a 10-second read of "is today healthy?" One or two collection numbers tell you whether cash is keeping pace with sales. |

### C.2 Performance — Execution Funnel + Quality

| Question | Answer |
| --- | --- |
| Belongs here? | **Nothing, or at most nothing structural.** The funnel is a *sales* funnel. |
| Does NOT belong? | Recovery rate as a funnel stage; "collection" as a stage after Revenue. |
| Why? | The funnel's value is **clean conversion math** (Planned → Actual → Orders → Revenue). Collection has a fundamentally different denominator — billing/piutang, not visits. Forcing it into the funnel produces meaningless math and erodes trust in the dashboard. |

### C.3 Outcomes — Revenue Distribution + Territory

| Question | Answer |
| --- | --- |
| Belongs here? | **Yes — this is the natural home.** Territory should gain "Collections" and "Overdue Exposure." Revenue Distribution (by salesman) should be pairable with overdue-by-salesman to reveal *revenue concentration vs. risk concentration*. |
| Does NOT belong? | Full per-salesman aging detail (too granular for the overview). |
| Why? | Outcomes is where managers compare "who produced what" and "which area is healthy." Collection outcomes are outcomes. |

### C.4 Action Center (Exceptions)

| Question | Answer |
| --- | --- |
| Belongs here? | **Yes.** Collection-related management actions: "Needs Collection Action" (high-overdue salesman), "High Revenue but High Overdue" (revenue booked but not collected), "Legacy Debt Risk" (aging >90 exposure). |
| Does NOT belong? | Raw overdue lists without an action implication; finance/AR housekeeping (posting, reconciliation, allocation). |
| Why? | The Action Center is action-first, and collection exceptions are among the most urgent actions a manager can take — cash recovery is time-sensitive. |

### C.5 Trends

| Question | Answer |
| --- | --- |
| Belongs here? | **Yes.** Cash Collected trend, Recovery vs Billing trend, Overdue Exposure trend (7/30 day toggle). |
| Does NOT belong? | Detailed aging migration charts (that is Collection Dashboard territory). |
| Why? | Trends answer "getting better or worse." Collection-health trends are essential to that answer now that salesmen own collection. |

---

## Part D — Cross-Dashboard Signals

Assume the Collection Dashboard remains separate. The following 3–5 signals
should be surfaced inside the SFO.

### Signal 1 — Cash Collected MTD

- **Source field:** `RecoverySummary.CashCollectedMtd` / `AttentionCards.CashCollectedMtd`
- **Business meaning:** Total cash recovered this month-to-date.
- **Recommended placement:** A. Status (secondary KPI, paired under Revenue).
- **Why it matters:** The sales manager needs to know whether cash is keeping
  pace with the revenue being booked — the daily heartbeat of "are we selling
  *and* getting paid?"

### Signal 2 — Recovery vs Billing %

- **Source field:** `AttentionCards.RecoveryVsBillingPercent` / `RecoverySummary.RecoveryVsBillingPercent`
- **Business meaning:** Proportion of billed receivables recovered in the period;
  the collection-effectiveness ratio.
- **Recommended placement:** A. Status (secondary KPI) and E. Trends.
- **Why it matters:** The single best "collection discipline" ratio. A declining
  RvB signals cash-flow stress even while revenue is rising — an early-warning
  that pure revenue KPIs will miss.

### Signal 3 — Salesmen with High Overdue Exposure

- **Source field:** `TopOverdueSalesmen[]` (`EntityName`, `Amount`, `PercentOfTotal`)
- **Business meaning:** Which salesmen carry the largest outstanding receivables.
- **Recommended placement:** D. Action Center — "Needs Collection Action," with
  drill-across to the Collection Dashboard.
- **Why it matters:** Directly identifies who is selling but not collecting — the
  key salesman-accountability bridge. This is the single most valuable signal for
  a sales manager.

### Signal 4 — Overdue Exposure (with attention flag)

- **Source field:** `AttentionCards.OverdueExposure` + `ExposureRequiresAttention`
- **Business meaning:** Total overdue exposure and whether it crossed a threshold.
- **Recommended placement:** A. Status (secondary/alert) or D. Action Center as a
  trigger.
- **Why it matters:** A portfolio-level risk signal; an early warning that
  collection is deteriorating before it becomes an aging crisis.

### Signal 5 — Overdue Concentration / Overdue by Wilayah

- **Source field:** `AttentionCards.OverdueConcentrationPercent` / `TopOverdueWilayah[]`
- **Business meaning:** Whether overdue is concentrated in a few
  customers/salesmen/territories or spread across the portfolio.
- **Recommended placement:** C.2 Territory Performance (overdue per Wilayah).
- **Why it matters:** Concentration tells a manager whether the problem is
  *systemic* (widespread — fix policy) or *localized* (a few accounts — fix those).
  It changes the intervention.

---

## Part E — Salesman Effectiveness Model

### The three dimensions

| Dimension | Examples | Question it answers |
| --- | --- | --- |
| **Sales Productivity** | Revenue, Orders, Revenue per Visit | Is he selling? |
| **Collection Discipline** | Recovery Rate, Cash Collected, Collection Success | Is he collecting? |
| **Portfolio Quality** | Overdue Exposure, Aging Risk, Legacy Debt | Is his book healthy? |

### Assessment

- **Is it useful?** Yes. It mirrors how a distributor owner actually judges a
  salesman — *"jualan, nagih, piutang sehat"* (selling, collecting, healthy
  receivables). It is a natural mental model, not an invented abstraction.
- **Is it too complex?** Only if presented as three simultaneous scorecards with
  weights. As a conceptual *framing*, it is intuitive.
- **Would owners understand it?** Yes, without explanation.
- **Would managers act on it?** Yes, **if and only if** each dimension maps to a
  concrete action (mirroring the existing Action Center): Productivity → coaching;
  Discipline → collection follow-up; Portfolio → credit review / block new orders
  for bad-pay customers.

### Recommendation

Adopt the three dimensions as the underlying **evaluation framework**, but:

1. Do **not** create a single blended "effectiveness score" — it hides the
   sales/collection trade-off and misguides incentives.
2. Present the dimensions as **three parallel columns in a salesman drill-down**,
   not in the overview. The overview stays lean.
3. Reuse the existing `Investigation.SuggestedQuery` / navigation affordances from
   the Collection schema so each dimension is actionable, not just descriptive.

---

## Part F — Territory Performance Review

### Proposed evolution

```text
From:  Visits / Orders / Revenue
To:    Visits / Orders / Revenue / Collections / Overdue Exposure
```

### Benefits

- Reveals a territory that *sells well but doesn't pay* — revenue alone flatters
  it.
- Enables fair comparison: two territories with equal revenue but different
  overdue are not equal.
- Sharpens manager attention: high-revenue + high-overdue needs a different
  intervention (credit/collection) than low-revenue (activity/coaching).

### Risks

- **Metric incompatibility:** collection is a *stock* metric (outstanding balance)
  while visits/orders/revenue are *flow* metrics (period). Mixing them without
  time-alignment can mislead.
- **Overload:** the territory measure selector grows; defaults must stay clean.
- **Misattribution:** territory overdue may be driven by legacy/structural issues,
  not current territory management.

### Recommendation

Evolve, but with discipline. Keep Visits/Orders/Revenue as the **primary** measure
set; add Collections and Overdue Exposure as a separate **"Financial Health"**
measure group with explicit time-context labels. Use a measure-group toggle
rather than one long list, so the daily sales view is not diluted.

---

## Part G — Attention Center Review

### Verdict

Yes — the Attention Center should include collection actions, because collection
exceptions are among the most urgent and time-sensitive interventions available.

### Collection exceptions that deserve visibility in the SFO

| Exception | Label (action-first convention) |
| --- | --- |
| High Revenue but High Overdue (booking revenue, receivables growing) | **Needs Collection Review** |
| Low Recovery vs Billing % (not paying down) | **Needs Collection Action** |
| Large Aging >90 Exposure (legacy debt risk) | **Needs Escalation** |
| Overdue Concentration (one/few customers dominate overdue) | **Needs Credit Review** |

### What does NOT belong in the SFO

Pure finance/AR housekeeping — payment posting, reconciliation, allocation,
tax-related follow-up. Those belong only in the Collection Dashboard.

### Design note

Keep the existing `"Needs X"` label convention for consistency. Each new label
must imply a **sales-manager** action (coach, block order, escalate, review
credit), not a finance action.

---

## Part H — Future Evolution

### Proposed hierarchy

```text
Executive Commercial Health
    ├── Sales Force Dashboard
    └── Collection Dashboard
```

### Verdict

Makes sense as a **future north star**, with one important qualification: it
should be a **hub-and-spoke** model, not a merger.

### Explanation

- The executive layer should be a **thin scorecard** (a few headline metrics
  spanning revenue, cash collected, overdue exposure, aging) that **drills into**
  each specialized dashboard — not a merged mega-dashboard.
- This gives owners a single entry point while preserving specialist depth.
- It is incremental: both leaf dashboards already exist (or will).

### Sequencing caveat

Only build the executive rollup **after** cross-dashboard signals are proven and
KPI ownership is defined (single source of truth per KPI). Otherwise the
executive view will surface inconsistent numbers and erode trust. **Do not build
the executive layer first.**

---

## Overall Assessment

**Moderate Collection Integration Recommended.**

The SFO does not need a redesign, but it should evolve to reflect that salesmen
own collection. That means: 2 collection headline KPIs in Status, a Financial
Health measure group in Territory, collection exceptions in the Action Center,
and collection trends — all fed by cross-dashboard signals from the existing
Collection Dashboard. No new data model is required.

---

## Top 5 Findings

1. **The salesman's role explicitly includes collection** (PRODUCT.md: "customer
   visits, order collection, invoice delivery, and payment collection"). The
   SFO's sell-only effectiveness view is incomplete, not incorrect.
2. **The SFO funnel is well-designed for sales and must not be contaminated.**
   Collection uses a different denominator (billing/piutang, not visits), so it
   cannot be a funnel stage without producing meaningless math.
3. **Territory and Revenue Distribution are the natural, low-risk homes for
   collection signals** — they compare "who produced" vs. "who paid."
4. **The Collection Dashboard already exposes every signal needed** (Cash
   Collected MTD, Recovery vs Billing, TopOverdueSalesmen, Overdue Concentration)
   — cross-dashboard surfacing requires no new data model.
5. **The Action Center's `"Needs X"` convention extends cleanly to collection**
   ("Needs Collection Action", "Needs Credit Review", "Needs Escalation") without
   redesign.

---

## Top 5 Risks

1. **Metric contamination** — forcing collection into the visit funnel produces
   meaningless math and erodes trust in the whole dashboard.
2. **Misattribution** — penalizing salesmen for structural/legacy overdue that
   predates them or stems from credit policy, not their behavior.
3. **Dashboard bloat** — full integration overloads the deliberately lean SFO and
   dilutes its daily field-execution focus.
4. **KPI inconsistency / double-counting** — the same KPI shown in two dashboards
   with different numbers or definitions.
5. **Composite-score temptation** — collapsing sales + collection into one index
   hides the trade-off and misguides incentives.

---

## Top 5 Recommended Enhancements

1. **Add two collection headline KPIs to Status** — Cash Collected MTD and
   Recovery vs Billing % as secondary cards next to Revenue.
2. **Extend Territory Performance with a "Financial Health" measure group** —
   Collections and Overdue Exposure, separate from the sales measures, with clear
   time labels.
3. **Add collection exceptions to the Action Center** — "Needs Collection Action,"
   "High Revenue + High Overdue," "Legacy Debt Risk," "Needs Credit Review."
4. **Surface TopOverdueSalesmen as a cross-dashboard signal** in the Action Center
   with drill-across to the Collection Dashboard.
5. **Add collection trends to Trends** — Cash Collected, Recovery vs Billing, and
   Overdue Exposure, using the existing 7/30-day toggle.

---

## Recommended Dashboard Strategy

**Keep Separate with Cross-Dashboard Signals.**

### Justification

The two dashboards serve different decision cadences (daily field execution vs.
weekly/monthly receivable health) and different primary owners (sales manager vs.
finance/AR), so they should remain separate. But the **salesman** is the shared
entity accountable for both outcomes, so a small set of collection signals must
appear where the sales manager works every day. Cross-dashboard signals plus
drill-across navigation deliver the integrated question — *"how well are
salespeople creating revenue while maintaining portfolio health?"* — without
rebuilding a well-designed dashboard or blurring the ownership boundary.

---

## Scope Notes

- This assessment does not evaluate technical feasibility, frontend design, or
  implementation effort. Both dashboards are assumed implementable.
- It does not recommend redesigning the SFO's information architecture; it
  recommends targeted additions consistent with the existing 5-section model
  (Status → Performance → Outcomes → Action Center → Trends).
- KPI governance (single source of truth per KPI, one primary location) is a
  precondition for any cross-dashboard surfacing.
