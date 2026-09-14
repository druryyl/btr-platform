# Sales Force Overview Dashboard

## UX Blueprint v2.0

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

The dashboard follows a business process flow:

```text
Plan
  ↓
Execute
  ↓
Convert
  ↓
Sell
  ↓
Monitor
  ↓
Improve
```

This creates a natural mental model for business users and eliminates the need to interpret disconnected KPIs.

---

# Layout Structure

```text
┌────────────────────────────────────────────────────┐
│ SALES FORCE OVERVIEW                              │
├────────────────────────────────────────────────────┤
│ A. SALES FORCE OPERATING HEALTH                   │
├────────────────────────────────────────────────────┤
│ B. EXECUTION FUNNEL                               │
├────────────────────────────────────────────────────┤
│ C. EXECUTION QUALITY                              │
├────────────────────────────────────────────────────┤
│ D. TERRITORY PERFORMANCE                          │
├────────────────────────────────────────────────────┤
│ E. REVENUE DISTRIBUTION                           │
├────────────────────────────────────────────────────┤
│ F. ATTENTION CENTER                               │
├────────────────────────────────────────────────────┤
│ G. TREND ANALYSIS                                 │
└────────────────────────────────────────────────────┘
```

---

# A. Sales Force Operating Health

## Objective

Provide a 10-second understanding of overall field execution status.

## Visualization

Four KPI cards displayed horizontally.

```text
┌─────────────┐
│ Active      │
│ Salesmen    │
│ 16          │
└─────────────┘

┌─────────────┐
│ Visit       │
│ Execution   │
│ 88.3%       │
└─────────────┘

┌─────────────┐
│ Actual      │
│ Visits      │
│ 106         │
└─────────────┘

┌─────────────┐
│ Effective   │
│ Call Rate   │
│ 19.8%       │
└─────────────┘
```

## Business Interpretation

| KPI                 | Question Answered                 |
| ------------------- | --------------------------------- |
| Active Salesmen     | Do we have enough field capacity? |
| Visit Execution %   | Are plans being executed?         |
| Actual Visits       | How much field activity occurred? |
| Effective Call Rate | How productive were the visits?   |

## Priority

Highest.

This section must be visible without scrolling.

---

# B. Execution Funnel

## Objective

Visualize the conversion of planned activity into business outcomes.

## Visualization

Horizontal funnel.

```text
Planned Visits
      ↓
Actual Visits
      ↓
Effective Calls
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
88.3% Execution
        ↓
21 Effective Calls
19.8% ECR
        ↓
74 Orders
        ↓
Rp 38.3 M
```

## Business Interpretation

This is the primary business narrative of the dashboard.

Management can immediately identify where conversion is breaking down.

---

# C. Execution Quality

## Objective

Measure the quality and compliance of field execution.

## KPI Cards

```text
GPS Valid Rate

Missed Visits

Unplanned Visits

GPS Warning Count

GPS Suspicious Count
```

## Recommended Layout

```text
┌─────────────┐
│ GPS Valid   │
│ 82.1%       │
└─────────────┘

┌─────────────┐
│ Missed      │
│ 14          │
└─────────────┘

┌─────────────┐
│ Unplanned   │
│ 28          │
└─────────────┘

┌─────────────┐
│ GPS Issues  │
│ 6           │
└─────────────┘
```

## Business Interpretation

Answers:

* Are visits legitimate?
* Are plans being followed?
* Is GPS compliance acceptable?

---

# D. Territory Performance

## Objective

Identify differences between sales territories.

## Visualization

Ranked horizontal bar chart.

```text
Yogyakarta     ██████████
Magelang       ███████
Solo           █████
Klaten         ███
```

## Available Measures

User selectable:

* Actual Visits
* Orders
* Revenue
* Effective Call Rate

## Business Interpretation

Answers:

* Which territory is most active?
* Which territory is underperforming?
* Where should management focus?

---

# E. Revenue Distribution

## Objective

Understand revenue concentration within the sales team.

## Visualization

Contribution chart.

```text
Tiara      ████████████ 40%
Mala       ████         17%
Anggar     ██            9%
Others     ███████      34%
```

## Business Interpretation

Answers:

* Is revenue concentrated in a few individuals?
* Is the team balanced?
* Is there key-person risk?

## Design Principle

Show contribution distribution rather than Top 10 rankings.

Distribution reveals business structure.

Rankings only reveal winners.

---

# F. Attention Center

## Objective

Provide actionable management insights.

This section answers:

> Who requires intervention today?

## Layout

Four compact ranked lists.

### Lowest Visit Execution

```text
1. Salesman A
2. Salesman B
3. Salesman C
```

### Lowest Effective Call Rate

```text
1. Salesman D
2. Salesman E
3. Salesman F
```

### Highest Unplanned Visits

```text
1. Salesman G
2. Salesman H
3. Salesman I
```

### Highest GPS Issues

```text
1. Salesman J
2. Salesman K
3. Salesman L
```

## Design Principle

Focus on exceptions.

Managers should spend time on problems, not averages.

---

# G. Trend Analysis

## Objective

Determine whether performance is improving or deteriorating.

## Visualizations

### Visit Execution Trend

```text
Last 30 Days
```

Line chart.

---

### Effective Call Rate Trend

```text
Last 30 Days
```

Line chart.

---

### Orders Trend

```text
Last 30 Days
```

Line chart.

---

### Revenue Trend

```text
Last 30 Days
```

Line chart.

## Business Interpretation

Answers:

* Are we improving?
* Are we declining?
* Is current performance normal or abnormal?

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

# Success Criteria

A business owner should be able to answer the following questions within 30 seconds:

✓ Are salespeople executing their visit plans?

✓ Are visits generating meaningful customer engagement?

✓ Are activities producing orders?

✓ Are orders generating revenue?

✓ Which territory is performing best?

✓ Which salespeople need intervention?

✓ Are key metrics improving or declining?

If these questions can be answered without drilling down, the dashboard has achieved its purpose.
