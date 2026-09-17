# Sales Force Overview Dashboard

## UX Blueprint v2.1

### Purpose

The Sales Force Overview dashboard provides business owners, sales managers, and supervisors with a single-page view of the health of the field sales operation.

The dashboard is designed to answer four management questions:

1. Are salespeople executing the planned activities?
2. Are field visits productive?
3. Are activities generating orders and revenue?
4. Which areas or individuals require management attention?

The dashboard is not intended to be a detailed salesperson performance report. It is an operational overview and management control dashboard.

---

# Information Architecture

The dashboard follows a management decision flow:

```text
Status
  ↓
Performance
  ↓
Outcomes
  ↓
Action Center
  ↓
Trends
```

This creates a natural mental model for business users:

* **Status** — What is happening right now?
* **Performance** — How well are we executing?
* **Outcomes** — What did execution produce?
* **Action Center** — What should I do right now?
* **Trends** — Is it getting better or worse?

Each layer answers one management question. KPIs appear in exactly one layer to avoid duplication.

---

# Layout Structure

```text
┌────────────────────────────────────────────────────┐
│ SALES FORCE OVERVIEW                              │
├────────────────────────────────────────────────────┤
│ A. STATUS — Operating Health                      │
├────────────────────────────────────────────────────┤
│ B. PERFORMANCE — Execution Funnel + Quality       │
├────────────────────────────────────────────────────┤
│ C. OUTCOMES — Revenue Distribution                │
│                  Territory Performance             │
├────────────────────────────────────────────────────┤
│ D. ACTION CENTER — What Should I Do?              │
├────────────────────────────────────────────────────┤
│ E. TRENDS — Trend Analysis                        │
└────────────────────────────────────────────────────┘
```

---

# A. Status — Operating Health

## Objective

Provide a 10-second understanding of overall field execution status.

## Visualization

Six KPI cards with primary/secondary hierarchy.

### Primary KPIs (larger cards, leftmost position)

```text
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│ Revenue         │  │ Visit Execution │  │ Orders          │
│ Rp 38.3 M       │  │ 88.3%           │  │ 74              │
│ Rp 361K/visit   │  │                 │  │                 │
└─────────────────┘  └─────────────────┘  └─────────────────┘
```

### Secondary KPIs (smaller cards, rightmost position)

```text
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│ Active Salesmen │  │ Order Conversion│  │ Wasted Visits   │
│ 14 / 16         │  │ 69.8%           │  │ 32              │
└─────────────────┘  └─────────────────┘  └─────────────────┘
```

## Business Interpretation

### Primary KPIs

| KPI                 | Question Answered                           | Source Field                  | Why Primary |
| ------------------- | ------------------------------------------- | ----------------------------- | ----------- |
| Revenue             | How much revenue was generated?             | `TeamKpis.TotalOmzet`         | Ultimate business outcome |
| Visit Execution %   | Are plans being executed?                   | `TeamKpis.VisitExecutionPercent` | Operational discipline |
| Orders              | How many orders were generated?             | `TeamKpis.TotalOrders`        | Business activity |

### Secondary KPIs

| KPI                 | Question Answered                           | Source Field                  | Why Secondary |
| ------------------- | ------------------------------------------- | ----------------------------- | ------------- |
| Active Salesmen     | How many salesmen actually worked today?    | Computed: salesmen with ActualVisits > 0 | Capacity context |
| Order Conversion    | What percentage of visits generated orders? | `TeamKpis.EffectiveCallRate`  | Quality context |
| Wasted Visits       | How many visits produced no orders?         | `ActualVisits - EffectiveCalls` | Actionable quality signal |

### Derived KPI: Revenue per Visit

Shown as subtitle under Revenue card:

**Formula:** `TeamKpis.TotalOmzet / TeamKpis.ActualVisits`

**Business meaning:** Revenue productivity per visit. Answers: "How much revenue does each visit generate?"

**Action:** If low, investigate order value, pricing, or product mix.

## Priority

Highest.

This section must be visible without scrolling.

## Design Rule

"Eligible Salesmen" is removed as a standalone card. It is shown as denominator in "Active Salesmen: 14 / 16 eligible."

"Revenue per Visit" is promoted from funnel annotation to hero subtitle because it is the most actionable productivity metric.

---

# B. Performance — Execution Funnel + Quality

## B.1 Execution Funnel

### Objective

Visualize the conversion of planned activity into business outcomes.

### Visualization

Horizontal funnel with three stages and conversion rates.

```text
Planned Visits
      ↓
Actual Visits
      ↓
Orders
      ↓
Revenue
```

Example:

```text
120 Planned Visits
        ↓
106 Actual Visits
88.3% Execution Rate
        ↓
74 Orders
69.8% Order Conversion
        ↓
Rp 38.3 M
Rp 361 K Revenue per Visit
```

### Conversion Definitions

| Conversion           | Formula                              | Question Answered                    |
| -------------------- | ------------------------------------ | ------------------------------------ |
| Execution Rate       | Actual Visits / Planned Visits       | Are plans being executed?            |
| Order Conversion     | Orders / Actual Visits               | Are visits generating orders?        |
| Revenue per Visit    | Revenue / Actual Visits              | What is the revenue productivity?    |

### Business Interpretation

This is the primary business narrative of the dashboard.

Management can immediately identify where conversion is breaking down:

* Low Execution Rate → Plans are not being followed
* Low Order Conversion → Visits are not productive
* Low Revenue per Visit → Orders are low value

### Design Rule

The funnel measures conversion between stages. Each stage counts a distinct entity:

* Planned Visits = customers planned
* Actual Visits = customers visited (check-ins)
* Orders = total orders placed

"Effective Calls" is **not** a funnel stage. It is a quality metric (see B.2).

---

## B.2 Execution Quality

### Objective

Measure the quality and compliance of field execution.

### KPI Cards

```text
┌─────────────┐
│ Missed      │
│ 14          │
└─────────────┘

┌─────────────┐
│ Unplanned   │
│ 28          │
└─────────────┘

┌─────────────┐
│ No-Order    │
│ Visits      │
│ 32          │
└─────────────┘

┌─────────────┐
│ GPS Valid   │
│ 82.1%       │
└─────────────┘
```

### Business Interpretation

| KPI              | Question Answered                          | Source Field                      | Actionability |
| ---------------- | ------------------------------------------ | --------------------------------- | ------------- |
| Missed Visits    | How many planned visits were not executed? | `TeamKpis.MissedVisits`           | **High** — identify which salesmen, review plan realism |
| Unplanned Visits | How many visits were not in the plan?      | `TeamKpis.UnplannedVisits`        | **Medium** — investigate whether positive (proactive) or negative (non-compliant) |
| No-Order Visits  | How many visits produced no orders?        | `ActualVisits - EffectiveCalls`   | **High** — investigate customer targeting or salesman approach |
| GPS Valid Rate   | Are GPS check-ins legitimate?              | `TeamKpis.GpsValidRate`           | **Low** — technical metric, supervisor-level detail |

### Derived KPI: No-Order Visits

**Formula:** `TeamKpis.ActualVisits - TeamKpis.EffectiveCalls`

**Business meaning:** Visits that consumed time but generated no revenue. Answers: "How many visits were wasted?"

**Action:** If high, investigate: Are salesmen visiting the wrong customers? Are customers not ready to order? Is the product range wrong?

**Why it matters:** This is the most actionable quality metric. It directly tells management how many field hours produced zero business value.

---

# C. Outcomes — Revenue Distribution + Territory Performance

## C.1 Revenue Distribution

### Objective

Understand revenue concentration within the sales team.

### Visualization

Contribution chart.

```text
Tiara      ████████████ 40%
Mala       ████         17%
Anggar     ██            9%
Others     ███████      34%
```

### Business Interpretation

Answers:

* Is revenue concentrated in a few individuals?
* Is the team balanced?
* Is there key-person risk?

### Design Principle

Show contribution distribution rather than Top 10 rankings.

Distribution reveals business structure.

Rankings only reveal winners.

---

## C.2 Territory Performance

### Objective

Identify differences between sales territories (Wilayah).

### Visualization

Ranked horizontal bar chart.

```text
Yogyakarta     ██████████
Magelang       ███████
Solo           █████
Klaten         ███
```

### Available Measures

User selectable:

* Actual Visits
* Orders
* Revenue
* Order Conversion Rate

### Data Source

Territory measures are computed by aggregating `Salesmen[]` rows grouped by `WilayahName`:

| Measure              | Aggregation Logic                                    |
| -------------------- | ---------------------------------------------------- |
| Actual Visits        | `SUM(Salesmen[].ActualVisits WHERE WilayahName = X)` |
| Orders               | `SUM(Salesmen[].OrdersCount WHERE WilayahName = X)`  |
| Revenue              | `SUM(Salesmen[].OmzetAmount WHERE WilayahName = X)`  |
| Order Conversion Rate | Weighted average from salesman-level data         |

### Business Interpretation

Answers:

* Which territory is most active?
* Which territory is underperforming?
* Where should management focus?

---

# D. Action Center

## Objective

Provide actionable management insights with prescriptive labels.

This section answers:

> What should I do right now?

## Layout

Five compact ranked lists with action-oriented labels.

### Needs Plan Review

*Salesmen with lowest visit execution — review plan quality and discuss with salesman.*

```text
1. Salesman A
2. Salesman B
3. Salesman C
```

### Needs Coaching

*Salesmen with lowest order conversion — coach on customer engagement and visit approach.*

```text
1. Salesman D
2. Salesman E
3. Salesman F
```

### Needs Investigation

*Salesmen with highest unplanned visits — determine if positive (proactive) or negative (non-compliant).*

```text
1. Salesman G
2. Salesman H
3. Salesman I
```

### Needs Follow-up

*Salesmen with GPS issues — check GPS device, verify check-in process.*

```text
1. Salesman J
2. Salesman K
3. Salesman L
```

### Needs Immediate Attention

*Salesmen with zero activity today — contact salesman, verify status.*

```text
1. Salesman M
2. Salesman N
```

## Design Principle

Focus on actions, not just problems.

Each exception label implies a management intervention:

* "Needs Plan Review" → Review plan quality, discuss with salesman
* "Needs Coaching" → Coach on customer engagement, visit approach
* "Needs Investigation" → Determine context before acting
* "Needs Follow-up" → Check process, verify compliance
* "Needs Immediate Attention" → Contact salesman now

Managers should spend time on actions, not on diagnosing what to do.

---

# E. Trends — Trend Analysis

## Objective

Determine whether performance is improving or deteriorating.

## Visualizations

### Visit Execution Trend

```text
Last 7 Days / Last 30 Days (toggle)
```

Line chart.

---

### Orders Trend

```text
Last 7 Days / Last 30 Days (toggle)
```

Line chart.

---

### Revenue Trend

```text
Last 7 Days / Last 30 Days (toggle)
```

Line chart.

---

### Order Conversion Rate Trend

```text
Last 7 Days / Last 30 Days (toggle)
```

Line chart.

## Business Interpretation

Answers:

* Are we improving?
* Are we declining?
* Is current performance normal or abnormal?

## Design Note

Offer both 7-day and 30-day views via toggle.

7-day view reveals recent patterns. 30-day view reveals longer trends.

---

# Interaction Model

## Progressive Disclosure

Level 1 — Overview

Business owner sees summarized KPIs.

Level 2 — Drill Down

Click any KPI card to open supporting details.

Examples:

```text
Visit Execution %
    ↓
Salesman Visit Execution List

Revenue
    ↓
Revenue by Salesman

Territory
    ↓
Territory Detail Dashboard
```

## Design Rule

Overview page should never contain detailed salesperson tables.

Details belong in drill-down views.

---

# KPI Placement Matrix

Each KPI appears in exactly one primary location to avoid duplication.

| KPI                    | Primary Location        | Funnel Stage | Quality | Territory | Action Center | Trend |
| ---------------------- | ----------------------- | ------------ | ------- | --------- | ------------- | ----- |
| Revenue                | A. Status (Primary)     |              |         |           |               |       |
| Revenue per Visit      | A. Status (Primary)     |              |         |           |               |       |
| Visit Execution %      | A. Status (Primary)     |              |         |           |               |       |
| Orders                 | A. Status (Primary)     |              |         |           |               |       |
| Active Salesmen        | A. Status (Secondary)   |              |         |           |               |       |
| Order Conversion Rate  | A. Status (Secondary)   |              |         |           |               |       |
| Wasted Visits          | A. Status (Secondary)   |              |         |           |               |       |
| Planned Visits         | B. Funnel (Stage 1)     | ✓            |         |           |               |       |
| Actual Visits          | B. Funnel (Stage 2)     | ✓            |         |           |               |       |
| Orders (Funnel)        | B. Funnel (Stage 3)     | ✓            |         |           |               |       |
| Revenue (Funnel)       | B. Funnel (Annotation)  | ✓            |         |           |               |       |
| Execution Rate         | B. Funnel (Conversion)  | ✓            |         |           |               |       |
| Order Conversion       | B. Funnel (Conversion)  | ✓            |         |           |               |       |
| Revenue per Visit      | B. Funnel (Conversion)  | ✓            |         |           |               |       |
| Missed Visits          | B. Quality              |              | ✓       |           |               |       |
| Unplanned Visits       | B. Quality              |              | ✓       |           |               |       |
| No-Order Visits        | B. Quality              |              | ✓       |           |               |       |
| GPS Valid Rate         | B. Quality              |              | ✓       |           |               |       |
| Revenue by Salesman    | C. Revenue Distribution |              |         |           |               |       |
| Territory Measures     | C. Territory            |              |         | ✓         |               |       |
| Needs Plan Review      | D. Action Center        |              |         |           | ✓             |       |
| Needs Coaching         | D. Action Center        |              |         |           | ✓             |       |
| Needs Investigation    | D. Action Center        |              |         |           | ✓             |       |
| Needs Follow-up        | D. Action Center        |              |         |           | ✓             |       |
| Needs Immediate Attention | D. Action Center     |              |         |           | ✓             |       |
| Visit Execution Trend  | E. Trends               |              |         |           |               | ✓     |
| Orders Trend           | E. Trends               |              |         |           |               | ✓     |
| Revenue Trend          | E. Trends               |              |         |           |               | ✓     |
| Order Conversion Trend | E. Trends               |              |         |           |               | ✓     |

---

# Success Criteria

A business owner should be able to answer the following questions within 30 seconds:

✓ Are salespeople executing their visit plans?

✓ Are visits generating orders?

✓ Are orders generating revenue?

✓ Which territory is performing best?

✓ Which salespeople need intervention?

✓ Are key metrics improving or declining?

If these questions can be answered without drilling down, the dashboard has achieved its purpose.

---

# Changes from v2.0

| Change | v2.0 | v2.1 |
|--------|------|------|
| Mental model | Plan→Execute→Convert→Sell→Monitor→Improve | Status→Performance→Outcomes→Action Center→Trends |
| Section count | 7 sections (A-G) | 5 sections (A-E) |
| Funnel stages | 5 stages (Planned→Actual→Effective Calls→Orders→Revenue) | 3 stages + annotation (Planned→Actual→Orders→Revenue) |
| Effective Calls | Funnel stage | Quality KPI only |
| KPI duplication | Visit Execution % in 3 places, ECR in 4 places | Each KPI in exactly 1 primary location |
| Revenue position | Section E (5th) | Section C.1 (3rd) |
| Territory position | Section D (4th) | Section C.2 (4th) |
| Active Salesmen | Single count (eligible) | Two counts: Eligible + Active |
| Revenue per Visit | Missing | Promoted to hero section |
| Order Conversion | Missing | Added as funnel conversion metric |
| Trend views | Last 30 Days only | Last 7 Days / Last 30 Days toggle |
| Territory data | Only ActualVisits available | Frontend aggregation for all measures |
| Hero KPI hierarchy | Flat (all equal) | Primary/Secondary distinction |
| Effective Call Rate name | Confusing label | Renamed to "Order Conversion Rate" |
| Exception Center | Descriptive labels | Action Center with prescriptive labels |
| Wasted Visits metric | Missing | Added as "No-Order Visits" in Quality section |
| GPS Issues ranking | In Attention Center | Moved to Action Center as "Needs Follow-up" |
| Zero Activity signal | Missing | Added as "Needs Immediate Attention" in Action Center |
