# Sales Force Overview Dashboard

## UX Blueprint v2.2

> Evolution of v2.1. Incorporates the approved collection-related enhancements
> from the Collection Integration Assessment (v1.0).
>
> Supersedes: `sfo-dashboard-ux-blueprint-v2.1.md`
> Inputs: `sfo-dashboard-collection-integration-assessment.md`, `collection-data-dashboard-scheme.json`

### Purpose

The Sales Force Overview dashboard provides business owners, sales managers, and
supervisors with a single-page view of the health of the **commercial field
force operation** — field execution, sales conversion, and cash/portfolio
health.

The dashboard is designed to answer six management questions:

1. Are salespeople executing the planned activities?
2. Are field visits productive?
3. Are activities generating orders and revenue?
4. Are salespeople helping recover cash?
5. Are they maintaining healthy receivable portfolios?
6. Which areas or individuals require management attention?

The dashboard is not a detailed salesperson performance report and **not** a
Collection Dashboard. It is an operational overview and management control
dashboard for field force effectiveness.

---

# 1. Executive Summary

v2.1 was a correct and well-designed **sales execution** control dashboard. It
was not wrong — it was incomplete. BTR's own product definition assigns payment
collection to sales personnel alongside visits and order collection, so a
salesman's effectiveness is judged on *selling* and *collecting* together, not
selling alone.

v2.2 does **not** redesign v2.1. It evolves it in four targeted ways:

| # | Change | Why |
| --- | --- | --- |
| 1 | Add a **governing mental model** — `Coverage → Sales → Collection → Portfolio` — and a **three-dimension effectiveness framework** (Sales Productivity, Collection Discipline, Portfolio Quality). | The salesman owns the customer relationship end-to-end. A sell-only view creates a "revenue hero / collection villain" blind spot. |
| 2 | Surface a **small, governed set of collection signals** into Status, Outcomes (Territory), Trends, and the Action Center. | Cash recovery and portfolio health materially change a sales manager's daily decisions. No new data model is required — the Collection Dashboard already exposes every field. |
| 3 | Extend the **Action Center** with collection/credit/recognition categories. | Collection exceptions are among the most urgent, time-sensitive management actions. |
| 4 | Reframe the dashboard as a **child-ready node** for a future `Executive Commercial Health` hub. | Preserve the investment and avoid a later redesign. |

The two dashboards remain **separate**. The Collection Dashboard stays the system
of record for receivables. The Sales Force Overview surfaces only the collection
signals that change a *sales manager's* decision — never finance/AR housekeeping.

**What did not change:** the 5-section decision flow, the sales funnel, the
"each KPI appears in exactly one primary location" rule, the progressive
disclosure model, and the lean overview philosophy.

---

# 2. Mental Model

## 2.1 The business flow

The dashboard is now organized around a single commercial value chain:

```text
Coverage
    ↓
Sales
    ↓
Collection
    ↓
Portfolio
```

| Domain | Meaning | Example KPIs |
| --- | --- | --- |
| **Coverage** | Field activity and customer engagement | Active Salesmen, Planned Visits, Actual Visits, Visit Execution % |
| **Sales** | Commercial conversion | Orders, Revenue, Order Conversion, Revenue per Visit |
| **Collection** | Cash recovery | Cash Collected MTD, Recovery vs Billing %, Collection Risk Signals |
| **Portfolio** | Customer and receivable quality | Overdue Exposure, Aging Risk, High-Risk Salesmen, High-Risk Territories |

This model answers, in sequence:

- **Coverage** — are we reaching customers?
- **Sales** — are we converting visits into revenue?
- **Collection** — are we turning revenue into cash?
- **Portfolio** — is the book we are building financially healthy?

## 2.2 The two-axis model

The four domains and the five-section decision flow are **orthogonal, not
competing**. They must not be forced to choose between each other:

```text
BUSINESS DOMAINS (what is being measured)
        Coverage → Sales → Collection → Portfolio
                              ‖
                              ‖  every KPI maps to exactly one domain
                              ‖
DECISION FLOW (how it is read, top to bottom)
        Status → Performance → Action Center → Outcomes → Trends
```

- The **four domains** are the *content spine*: every KPI is tagged to one
  domain, so nothing is measured twice and nothing is missing.
- The **five sections** are the *reading order*: the sequence a manager uses to
  decide what to do next.

This is why the information architecture does **not** need to be renumbered from
`A–E` to `Coverage–Sales–Collection–Portfolio`. The domain model is an invisible
organizing spine; the section flow is the visible skeleton.

## 2.3 The effectiveness framework

The underlying evaluation framework for any individual salesman is
three-dimensional (adopted from the assessment, Part E):

| Dimension | Question it answers | Where it appears in the overview |
| --- | --- | --- |
| **Sales Productivity** | Is he selling? | Coverage + Sales domains |
| **Collection Discipline** | Is he collecting? | Collection domain |
| **Portfolio Quality** | Is his book healthy? | Portfolio domain |

**Rule: never blend the three into a single composite score.** A blended index
hides the sales/collection trade-off and misguides incentives. The dimensions
are shown side by side so the manager sees the tension explicitly. In the
overview they are surfaced as separate KPIs; in the salesman drill-down they are
three parallel columns (see §7 roadmap and §4 interaction note).

---

# 3. Information Architecture

## 3.1 Section hierarchy

```text
┌──────────────────────────────────────────────────────────────────┐
│ SALES FORCE OVERVIEW — Commercial Field Force Effectiveness      │
├──────────────────────────────────────────────────────────────────┤
│ A. STATUS — Operating & Commercial Health                        │
├──────────────────────────────────────────────────────────────────┤
│ B. PERFORMANCE — Execution Funnel + Salesman Scoreboard          │
├──────────────────────────────────────────────────────────────────┤
│ D. ACTION CENTER — What Should I Do?                             │
├──────────────────────────────────────────────────────────────────┤
│ C. OUTCOMES — Revenue Concentration, Territory & Financial Health│
├──────────────────────────────────────────────────────────────────┤
│ E. TRENDS — Trend Analysis                                       │
└──────────────────────────────────────────────────────────────────┘
```

The five sections and their A–E labels are preserved, but the display order of
Outcomes (C) and Action Center (D) is inverted per visualization review v1.0
§3.2–§3.3, so that "compare → act" is one contiguous motion. Performance (B) now
holds the Execution Funnel and the promoted Salesman Scoreboard; the three
redundant salesman ranking charts are removed. Outcomes (C) leads with the
Revenue Concentration strip. The v2.2 structural additions remain: a
"Commercial Health" cluster in Status, a "Financial Health" measure group in
Territory, and collection/credit categories in the Action Center.

## 3.2 Domain → section mapping

| Domain | Primary home | Supporting appearances |
| --- | --- | --- |
| Coverage | A. Status, B. Performance | C. Territory, E. Trends |
| Sales | A. Status, B. Performance | C. Outcomes, E. Trends |
| Collection | A. Status (headline) | C. Territory (Financial Health), D. Action Center, E. Trends |
| Portfolio | C. Outcomes (Financial Health) | A. Status (alert), D. Action Center (credit/escalation) |

Each KPI still appears in **exactly one primary location**. Collection KPIs obey
the same placement discipline as sales KPIs.

---

# 4. Section-by-Section Blueprint

## A. Status — Operating & Commercial Health

### Objective

Provide a 10-second understanding of overall field execution **and commercial**
status: is the operation selling *and* getting paid?

### KPIs

Primary (large cards, leftmost):

| KPI | Question Answered | Source Field |
| --- | --- | --- |
| Revenue | How much revenue was generated? | `TeamKpis.TotalOmzet` |
| Visit Execution % | Are plans being executed? | `TeamKpis.VisitExecutionPercent` |
| Orders | How many orders were generated? | `TeamKpis.TotalOrders` |

Secondary (smaller cards, rightmost):

| KPI | Question Answered | Source Field |
| --- | --- | --- |
| Active Salesmen | How many salesmen worked today? | Computed: `Salesmen[]` with `ActualVisits > 0` |
| Order Conversion | What % of visits generated orders? | `TeamKpis.EffectiveCallRate` |
| Wasted Visits | How many visits produced no orders? | `ActualVisits − EffectiveCalls` |

**New — Commercial Health cluster (secondary):**

| KPI | Question Answered | Source Field |
| --- | --- | --- |
| Cash Collected MTD | Is cash keeping pace with sales? | `AttentionCards.CashCollectedMtd` (or `RecoverySummary.CashCollectedMtd`) |
| Recovery vs Billing % | Are receivables being recovered? | `AttentionCards.RecoveryVsBillingPercent` |

**New — conditional alert chip (not a card):**

| Signal | Trigger | Source Field |
| --- | --- | --- |
| Overdue Exposure — "Needs attention" | Shown only when threshold crossed | `AttentionCards.ExposureRequiresAttention` |

### Visualizations

Six KPI cards with primary/secondary hierarchy (unchanged), plus a compact
"Commercial Health" pair on the right, plus a conditional alert chip.

```text
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│ Revenue         │  │ Visit Execution │  │ Orders          │
│ Rp 38.3 M       │  │ 88.3%           │  │ 74              │
│ Rp 361K/visit   │  │                 │  │                 │
└─────────────────┘  └─────────────────┘  └─────────────────┘

┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│ Active Salesmen │  │ Order Conversion│  │ Wasted Visits   │
│ 14 / 16         │  │ 69.8%           │  │ 32              │
└─────────────────┘  └─────────────────┘  └─────────────────┘

┌──────────────────────┐  ┌──────────────────────┐
│ Cash Collected MTD   │  │ Recovery vs Billing  │
│ Rp 27.4 M            │  │ 71.5%                │
│ Rp 1.9M/salesman     │  │                      │
└──────────────────────┘  └──────────────────────┘
        [ ⚠ Overdue exposure requires attention → ]
```

### Business Questions Answered

- Are salespeople executing their visit plans?
- Are visits generating orders and revenue?
- Is cash keeping pace with the revenue being booked?
- Are receivables being recovered at a healthy rate?
- Is overdue exposure crossing a threshold that needs attention?

### Management Actions Enabled

- A high Revenue with a lagging Cash Collected MTD → investigate "selling but
  not collecting" before it becomes a cash-flow problem.
- A falling Recovery vs Billing % → early-warning of collection stress even while
  revenue is rising.
- The overdue alert chip → one-click jump to the Action Center (or drill-across
  to the Collection Dashboard) for the specific at-risk entities.

### Design Rules (preserved from v2.1 + new)

- "Eligible Salesmen" remains a denominator inside "Active Salesmen: 14 / 16 eligible."
- "Revenue per Visit" remains the hero subtitle under Revenue.
- **New:** "Cash Collected per Active Salesman" becomes the subtitle under
  Cash Collected MTD, mirroring the Revenue card (see §4 — Productivity Review).
- **New:** Overdue Exposure is an **alert chip, not a permanent card**, so the
  10-second read stays clean. It appears only when `ExposureRequiresAttention`
  is true.
- **New:** The two Commercial Health cards are visually grouped and separated
  from the sales cards so the "sales vs collection" tension is explicit, not
  blended.

---

## B. Performance — Execution Funnel + Salesman Scoreboard

A Salesman Scoreboard also lives in this section, directly below the funnel; it
is the promoted comparison surface documented in the SF02 feature doc
(`docs/features/btr-portal/dashboard-11-sf02-sales-force-overview.md` §5).

### B.1 Execution Funnel

The funnel is the clean sales conversion narrative. **Collection is not a funnel
stage** — collection has a different denominator (billing/piutang, not visits)
and forcing it into the funnel would produce meaningless math.

```text
Planned Visits
      ↓  Execution Rate = Actual Visits / Planned Visits
Actual Visits
      ↓  Effective Call Rate = Effective Calls / Actual Visits
Effective Calls
      ↓  Order Conversion = Orders / Effective Calls
Orders
      ↓  Average Order Value = Order Value / Orders
Order Value
```

| Conversion | Formula | Question Answered |
| --- | --- | --- |
| Execution Rate | Actual Visits / Planned Visits | Are plans being executed? |
| Effective Call Rate | Effective Calls / Actual Visits | Are visits converting into orders? |
| Order Conversion | Orders / Effective Calls | Are productive visits becoming orders? |
| Average Order Value | Order Value / Orders | What is the value per order? |

Each stage shows its primary value and, where one exists, its absolute leak:
Missed Visits (Planned → Actual) and No-Order Visits
(Actual Visits − Effective Calls). The earlier four-stage spec, marked
"(unchanged)", had never been built and omitted Effective Calls; the five-stage
funnel above is the corrected, shipped design (visualization review v1.0 §2.1).

### B.2 Execution Quality (unchanged)

| KPI | Question Answered | Source Field |
| --- | --- | --- |
| Missed Visits | How many planned visits were not executed? | `TeamKpis.MissedVisits` |
| Unplanned Visits | How many visits were not in the plan? | `TeamKpis.UnplannedVisits` |
| No-Order Visits | How many visits produced no orders? | `ActualVisits − EffectiveCalls` |
| GPS Valid Rate | Are GPS check-ins legitimate? | `TeamKpis.GpsValidRate` |

### Objective / Business Questions / Actions

Unchanged from v2.1. This section is deliberately **collection-free**: the funnel
is a sales funnel and its value is clean conversion math.

---

## C. Outcomes — Revenue Concentration, Territory & Financial Health

### C.1 Revenue Concentration

A compact concentration strip replaces the former Order Value comparison chart
and answers "is revenue concentrated in a few individuals?" It shows the Top-1
and Top-3 shares of total order value and a single 100% stacked bar of the named
top three salesmen plus an "Others" remainder.

```text
Top 1 = 40% of order value    Top 3 = 66% of order value

Tiara      ████████████ 40%
Mala       ████         17%
Anggar     ██            9%
Others     ███████      34%
```

### C.2 Territory Performance (evolved)

Ranked horizontal bar chart. Measures now grouped into two measure groups:

**Group 1 — Sales Activity (primary, default):**

- Actual Visits
- Orders
- Revenue
- Order Conversion Rate

**Group 2 — Financial Health (new):**

- Collections (cash recovered per Wilayah)
- Overdue Exposure (outstanding overdue per Wilayah)

```text
Yogyakarta     ██████████
Magelang       ███████
Solo           █████
Klaten         ███
```

### C.3 Overdue Distribution (new, optional context)

A contribution-style view of overdue exposure across territories (from
`TopOverdueWilayah[]`) — answers "is overdue concentrated in a few territories or
spread across the portfolio?" Concentration changes the intervention (systemic →
fix policy; localized → fix those accounts).

### Business Questions Answered

- Is revenue concentrated in a few individuals? Is there key-person risk?
- Which territory is most active / underperforming?
- **Which territory sells well but does not pay?**
- **Is overdue risk concentrated or systemic?**

### Management Actions Enabled

- A territory with high Revenue + high Overdue Exposure → credit/collection
  intervention, not an activity/coaching intervention.
- Overdue concentration in one territory → investigate that territory's credit
  terms and collection cadence.

### Design Rules

- The measure selector uses a **two-group toggle** (Sales Activity / Financial
  Health), never one long flat list. The daily sales view is not diluted.
- Financial Health measures carry an explicit **time-context label** (e.g.
  "MTD", "outstanding as of today") because collection is a *stock* metric while
  visits/orders/revenue are *flow* metrics. Mixing them without labels misleads.

### Data Sources

| Measure | Aggregation Logic |
| --- | --- |
| Actual Visits / Orders / Revenue | `SUM(Salesmen[] WHERE WilayahName = X)` |
| Collections (per Wilayah) | Collection signal aggregated by `WilayahName` (`TopOverdueWilayah[]` + recovery data) |
| Overdue Exposure (per Wilayah) | `TopOverdueWilayah[]` (`EntityName`, `Amount`) |

---

## D. Action Center

See §6 for the full design. In summary, it is extended with a **Commercial Risk**
group and a **Recognition** strip.

---

## E. Trends — Trend Analysis

### Objective

Determine whether performance is improving or deteriorating across **both**
sales and collection.

### Visualizations (7/30-day toggle)

Sales trends (unchanged):

- Visit Execution Trend
- Orders Trend
- Revenue Trend
- Order Conversion Rate Trend

Collection trends (new):

- **Cash Collected Trend**
- **Recovery vs Billing % Trend**
- **Overdue Exposure Trend**

### Business Questions Answered

- Are we improving or declining?
- Is current performance normal or abnormal?
- **Is collection discipline improving even as sales grow?**

### Design Rule

Collection trends reuse the existing 7/30-day toggle. Detailed aging-migration
charts remain Collection Dashboard territory — they do not appear here.

---

# 5. Cross-Dashboard Collection Integration

The Collection Dashboard remains the **system of record**. The Sales Force
Overview surfaces a small, governed set of signals, each with a sales-manager
action attached. No new data model is required.

## 5.1 Signal inventory

| # | Signal | Source field(s) | Placement | Why it matters to sales management |
| --- | --- | --- | --- | --- |
| 1 | Cash Collected MTD | `AttentionCards.CashCollectedMtd` / `RecoverySummary.CashCollectedMtd` | A. Status (Commercial Health) | Daily heartbeat: is cash keeping pace with booked revenue? |
| 2 | Recovery vs Billing % | `AttentionCards.RecoveryVsBillingPercent` / `RecoverySummary.RecoveryVsBillingPercent` | A. Status (Commercial Health) + E. Trends | Best single collection-discipline ratio; early-warning of cash-flow stress. |
| 3 | Overdue Exposure (attention flag) | `AttentionCards.OverdueExposure` + `ExposureRequiresAttention` | A. Status (conditional alert chip) | Portfolio-level risk trigger before it becomes an aging crisis. |
| 4 | Salesmen with High Overdue Exposure | `TopOverdueSalesmen[]` (`EntityName`, `Amount`, `PercentOfTotal`) | D. Action Center ("Needs Collection Action") | The most valuable signal: identifies who is selling but not collecting. |
| 5 | Overdue by Territory / Concentration | `TopOverdueWilayah[]`, `AttentionCards.OverdueConcentrationPercent` | C.2 Territory (Financial Health), C.3 | Reveals "sells well but doesn't pay" territories; concentration tells systemic vs localized. |
| 6 | Aging >90 / Legacy Debt | `AttentionCards.AgingOver90Exposure`, `AttentionCards.LegacyDebtCount` | D. Action Center ("Needs Escalation") | Structural risk that is not field-solvable; needs escalation to credit control. |
| 7 | High Revenue + High Overdue | Derived: pair `Salesmen[].OmzetAmount` with `TopOverdueSalesmen[]` | D. Action Center ("Needs Credit Review") | Exposes the "revenue hero / collection villain" blind spot at the individual level. |

## 5.2 Placement rationale

- **Status** — one or two collection headlines, because Status is a 10-second
  "is today healthy?" read.
- **Outcomes (Territory)** — the natural home for "who produced vs who paid"
  comparisons.
- **Action Center** — collection exceptions are urgent, time-sensitive actions.
- **Trends** — collection-health trends are essential to "getting better or worse."
- **Performance (funnel)** — deliberately **nothing**. Collection cannot be a
  funnel stage.

## 5.3 Drill-across

Every collection signal carries a **drill-across** affordance to the Collection
Dashboard (via `Navigation.PiutangDashboardRoute` and `Investigation.DashboardRoute`).
The Sales Force Overview **shows the signal and the action**; the Collection
Dashboard **owns the detail**.

## 5.4 Governance (precondition)

- One primary location per KPI; the Collection Dashboard is the single source of
  truth for collection values.
- The Sales Force Overview never re-computes a collection value differently — it
  reads the same fields.
- If a value is shown in both dashboards, the definition and number must match
  exactly, or trust erodes.

---

# 6. Action Center Design

The Action Center evolves from "exceptions" to **grouped, action-first
interventions** spanning both sales and collection. Each label implies a
*sales-manager* action (coach, review, collect, escalate), never a finance
action.

## 6.1 Final structure

```text
D. ACTION CENTER — What Should I Do?

┌─ FIELD EXECUTION ──────────────────────────────┐
│ Needs Coaching         (lowest order conversion)│
│ Needs Plan Review      (lowest visit execution) │
│ Needs Investigation    (unplanned + GPS)        │
│ Needs Immediate Attention (zero activity)       │
└─────────────────────────────────────────────────┘

┌─ COMMERCIAL RISK ──────────────────────────────┐
│ Needs Collection Action (highest overdue)       │
│ Needs Credit Review     (high revenue + high OD,│
│                          OD concentration)      │
│ Needs Escalation        (aging >90 / legacy)    │
└─────────────────────────────────────────────────┘

┌─ RECOGNITION ──────────────────────────────────┐
│ Recognition Candidates  (balanced performers)   │
└─────────────────────────────────────────────────┘
```

### Field Execution (sales actions)

| Category | Trigger | Sales-manager action |
| --- | --- | --- |
| Needs Coaching | Lowest order conversion / revenue productivity | Coach on customer engagement and visit approach |
| Needs Plan Review | Lowest visit execution | Review plan realism, discuss with salesman |
| Needs Investigation | Highest unplanned visits **and** GPS anomalies | Determine positive (proactive) vs negative (non-compliant) before acting |
| Needs Immediate Attention | Zero activity today | Contact the salesman now |

> Note: v2.1's "Needs Follow-up" (GPS issues) is folded into "Needs
> Investigation" to control list bloat. Both are "verify context before acting"
> actions; GPS detail remains available in the drill-down.

### Commercial Risk (new)

| Category | Trigger | Source | Sales-manager action |
| --- | --- | --- | --- |
| Needs Collection Action | Highest overdue exposure by salesman | `TopOverdueSalesmen[]` | Direct cash-recovery follow-up; hold new credit sales until paid |
| Needs Credit Review | High revenue + high overdue, or overdue concentration | `TopOverdueCustomers[]`, `OverdueConcentrationPercent` | Review credit limit/terms — a policy action, not a collection visit |
| Needs Escalation | Aging >90 / legacy debt | `AgingOver90Exposure`, `LegacyDebtCount` | Escalate to finance/credit control — likely structural, not field-solvable |

### Recognition (new)

| Category | Trigger | Purpose |
| --- | --- | --- |
| Recognition Candidates | Salesmen with high sales **and** healthy collection | Balance the action-first (negative) bias; reinforce the desired behavior |

## 6.2 Design principles

- **Action-first, grouped.** Managers spend time on actions, not on diagnosing.
- **Three groups, not one long list.** Groups map to the effectiveness
  framework: Field Execution (Productivity), Commercial Risk (Discipline +
  Portfolio), Recognition (culture).
- **Recognition is compact and separated** — a light strip, never a competing
  scoreboard — to avoid a purely punitive feel.
- **Drill-across** from Commercial Risk items to the Collection Dashboard.

---

# 7. Future Roadmap — Executive Commercial Health

The dashboard is designed to become a child of a future executive hub without
redesign.

```text
Executive Commercial Health
    ├── Sales Force Dashboard   ← this dashboard
    └── Collection Dashboard
```

### Why no redesign will be required

1. **Single source of truth already enforced.** Every KPI has one primary
   location and a defined owner (§5.4). The executive layer can read the same
   fields without surfacing inconsistent numbers.
2. **Domain-tagged KPIs.** Because every KPI is tagged `Coverage / Sales /
   Collection / Portfolio`, the executive rollup can be assembled by *selecting*
   a handful of headline metrics from each domain rather than re-laying-out a
   dashboard.
3. **Drill-across navigation already exists.** The executive hub is a thin
   scorecard that drills into each leaf dashboard — no merger required.
4. **The three-dimension framework scales.** The salesman drill-down (three
   parallel columns: Sales Productivity / Collection Discipline / Portfolio
   Quality) becomes the reusable pattern for any commercial effectiveness view.

### Sequencing (from the assessment, Part H)

Build the executive rollup **only after** cross-dashboard signals are proven and
KPI ownership is stable. Do not build the executive layer first — it would
surface inconsistent numbers and erode trust.

### The one structural upgrade reserved for the hub

The salesman drill-down view will eventually carry the **three parallel
columns** (Sales Productivity / Collection Discipline / Portfolio Quality), each
mapping to a concrete action. This is anticipated in v2.2's effectiveness
framework but lives in the drill-down, not the overview, keeping the overview
lean.

---

# Appendix A — Required Review Decisions

### A.1 Structure: keep `Status/Performance/Outcomes/Exceptions/Trends` or evolve to `Coverage/Sales/Collection/Portfolio`?

**Recommendation: keep the 5-section decision flow; adopt the 4-domain model as
the invisible content spine.**

Rationale:

- The 5 sections describe **how** a manager reads the dashboard (a decision
  cadence). The 4 domains describe **what** is being measured (business content).
  They are orthogonal and both should exist.
- Renumbering sections to `Coverage → Sales → Collection → Portfolio` would
  break the proven "what is happening → how well → what did it produce → what do
  I do → is it improving" reading order, and would force a full redesign of a
  dashboard that works.
- The 4-domain model is still fully realized: every KPI is tagged to a domain
  (§3.2), Collection and Portfolio content is added to the appropriate sections,
  and the domains surface explicitly in the drill-down and the effectiveness
  framework.

### A.2 Commercial Risk Layer

**Recommendation: no dedicated "Commercial Risk" section. It belongs in the
Action Center (actionable exceptions) with analytical context in Outcomes
(Territory Financial Health).**

Rationale:

- A standalone risk section would be a passive wall of numbers with no action
  path, and would blur the boundary with the Collection Dashboard.
- Risk signals need an **owner** and an **action**; those live in the Action
  Center ("Needs Collection Action", "Needs Credit Review", "Needs Escalation").
- The analytical view (overdue by territory, concentration) lives in Outcomes
  (§C.2/C.3), which is where "who produced vs who paid" comparisons belong.
- This follows the principle: **prefer management actionability over metric
  completeness.**

### A.3 Productivity Enhancement (derived KPIs)

**Recommended additions — only those that change a decision:**

| KPI | Recommendation | Rationale |
| --- | --- | --- |
| Revenue per Visit | Keep (already in v2.1) | Most actionable productivity metric. |
| Revenue per Active Salesman | **Add** — subtitle under Active Salesmen | Fair capacity comparison; flags over/under-staffed territories. |
| Cash Collected per Active Salesman | **Add** — subtitle under Cash Collected MTD | Extends the "per salesman" lens to collection; directly surfaces non-collectors. |
| Orders per Active Salesman | Do not add | Redundant with Revenue per Active Salesman + Order Conversion; adds noise, not decisions. |
| Recovery per Territory | Do not add | Already covered by the Territory "Financial Health" measure group; would double-count. |

### A.4 Action Center Evolution

**Recommendation: evolve to a grouped Action Center** (Field Execution /
Commercial Risk / Recognition) as detailed in §6. Rationale: the "Needs X"
convention extends cleanly to collection, grouping prevents the 9-list bloat the
assessment warned against, and Recognition balances the action-first bias.

---

# Appendix B — KPI Placement Matrix

Each KPI appears in exactly one primary location. **Domain** column is new.

| KPI | Domain | Primary Location |
| --- | --- | --- |
| Revenue | Sales | A. Status (Primary) |
| Revenue per Visit | Sales | A. Status (subtitle) |
| Visit Execution % | Coverage | A. Status (Primary) |
| Orders | Sales | A. Status (Primary) |
| Active Salesmen | Coverage | A. Status (Secondary) |
| Revenue per Active Salesman | Sales | A. Status (subtitle) |
| Order Conversion Rate | Sales | A. Status (Secondary) |
| Wasted Visits | Coverage | A. Status (Secondary) |
| Cash Collected MTD | Collection | A. Status (Commercial Health) |
| Cash Collected per Active Salesman | Collection | A. Status (subtitle) |
| Recovery vs Billing % | Collection | A. Status (Commercial Health) + E. Trends |
| Overdue Exposure (flag) | Portfolio | A. Status (conditional alert chip) |
| Planned / Actual / Effective Calls / Orders / Order Value | Coverage / Sales | B. Execution Funnel (stages) |
| Execution Rate / Effective Call Rate / Order Conversion / Average Order Value | Coverage / Sales | B. Execution Funnel (conversions) |
| Missed / No-Order Visits | Coverage | B. Execution Funnel (leaks) |
| Unplanned Visits / GPS Valid Rate | Coverage | B. Execution Quality |
| Salesman comparison measures (Execution %, Eff. Rate, Orders, Order Value, Missed, Unplanned, GPS, Status) | Coverage / Sales | B. Salesman Scoreboard |
| Attention (Needs Collection Action / Needs Credit Review) | Collection / Portfolio | B. Salesman Scoreboard (SIGNAL) |
| Needs Coaching / Plan Review / Investigation / Immediate Attention | Coverage / Sales | D. Action Center (Field Execution) |
| Needs Collection Action | Collection | D. Action Center (Commercial Risk) |
| Needs Credit Review / Needs Escalation | Portfolio | D. Action Center (Commercial Risk) |
| Recognition Candidates | Sales + Collection + Portfolio | D. Action Center (Recognition) |
| Revenue by Salesman | Sales | C. Revenue Concentration |
| Territory Sales Measures | Coverage / Sales | C.2 Territory (Sales Activity) |
| Territory Collections / Overdue | Collection / Portfolio | C.2 Territory (Financial Health) |
| Overdue Distribution / Concentration | Portfolio | C.3 Overdue Distribution |
| Sales + Collection Trends | All | E. Trends |

---

# Appendix C — Changes from v2.1

| Change | v2.1 | v2.2 |
| --- | --- | --- |
| Positioning | Sales Force Effectiveness | Commercial Field Force Effectiveness |
| Mental model | Status→Performance→Outcomes→Action Center→Trends only | + Coverage→Sales→Collection→Portfolio domain spine |
| Effectiveness framework | None | 3-dimension (Productivity / Discipline / Portfolio) |
| Status | 6 cards (3 primary + 3 secondary) | + Commercial Health cluster + conditional alert chip |
| Cash Collected MTD | Missing | Status (Commercial Health) |
| Recovery vs Billing % | Missing | Status + Trends |
| Overdue Exposure flag | Missing | Conditional alert chip in Status |
| Territory measures | Sales measures only | + "Financial Health" measure group (Collections, Overdue) |
| Overdue by territory | Missing | C.3 Overdue Distribution |
| Action Center | 5 sales categories | 3 groups: Field Execution (4), Commercial Risk (3), Recognition (1) |
| "Needs Follow-up" (GPS) | Standalone | Folded into "Needs Investigation" |
| Collection trends | Missing | Cash Collected, Recovery vs Billing, Overdue Exposure |
| Salesman drill-down | Sales-only | + 3 parallel columns (Productivity / Discipline / Portfolio) |
| Derived KPIs | Revenue per Visit | + Revenue per Active Salesman, Cash Collected per Active Salesman |
| Executive alignment | None | Child-ready for Executive Commercial Health hub |

---

# Success Criteria

A business owner should be able to answer the following within 30 seconds,
without drilling down:

✓ Are salespeople executing their visit plans?

✓ Are visits generating orders?

✓ Are orders generating revenue?

✓ Is cash keeping pace with sales?

✓ Are receivables being recovered at a healthy rate?

✓ Which territory is performing best?

✓ Which territory is selling but not paying?

✓ Which salespeople need intervention — and which kind (coach / collect / credit / escalate)?

✓ Are key metrics — sales **and** collection — improving or declining?

✓ Who deserves recognition?
