# PRINCIPAL-PERFORMANCE-DASHBOARD-UX-BLUEPRINT

Version: V2
Dashboard: SA04 — Principal Performance Dashboard
Status: Approved UX Blueprint
Depends On: FEASIBILITY-ASSESSMENT-PRINCIPAL-PERFORMANCE-DASHBOARD.md

---

# 1. Dashboard Philosophy

## Purpose

The dashboard exists to help the Owner identify:

* Which Principals require immediate attention
* Which Principals represent the biggest opportunities
* Which Principals create business risk
* Where management discussion should focus

The dashboard is not intended to:

* Act as a reporting table
* Replace detailed analytics screens
* Display every available KPI
* Explain root causes

The dashboard answers:

> "Where should management focus today?"

---

# 2. Information Hierarchy

The page must be organized according to management attention priority.

## Level 1 — Executive Snapshot (5 Seconds)

Visible immediately after page load.

Contains:

1. Portfolio Overview
2. Opportunity & Risk Board

Purpose:

Allow management to understand portfolio health without scrolling.

---

## Level 2 — Management Analysis (30 Seconds)

Contains:

3. Achievement Gap Leaderboard
4. Coverage & Reach Analysis
5. Return Risk Analysis
6. Salesman Dependency Analysis

Purpose:

Help management understand why attention is required.

---

## Level 3 — Investigation

Contains:

7. Principal Detail Table

Purpose:

Allow validation and investigation of findings from upper sections.

---

# 3. Page Layout

Desktop Layout

┌───────────────────────────────────────────────┐
│ Header                                        │
│ Period • Data Updated At • Data Completeness  │
└───────────────────────────────────────────────┘

┌───────────────────────────────────────────────┐
│ Portfolio Overview                            │
└───────────────────────────────────────────────┘

┌───────────────────────────────────────────────┐
│ Opportunity & Risk Board                      │
└───────────────────────────────────────────────┘

┌──────────────────────┬────────────────────────┐
│ Achievement Gap      │ Coverage & Reach       │
│ Leaderboard          │ Analysis               │
└──────────────────────┴────────────────────────┘

┌──────────────────────┬────────────────────────┐
│ Return Risk          │ Salesman Dependency    │
│ Analysis             │ Analysis               │
└──────────────────────┴────────────────────────┘

┌───────────────────────────────────────────────┐
│ Principal Detail Table                        │
└───────────────────────────────────────────────┘

---

# 4. Header Section

Display:

* Dashboard Title
* Reporting Period
* Data Updated At
* Data Completeness Indicator

Example:

Data Updated:
13 September 2026 08:00

Data Completeness:
Coverage 100%
Target 97%
Return 100%
Contribution 92%

Purpose:

Create confidence in the dashboard before any analysis is performed.

---

# 5. Portfolio Overview

## Purpose

Provide a single-page health summary of the Principal portfolio.

## Visual Style

Large KPI Cards.

No charts.

No gauges.

No pie charts.

## Cards

### Total Sales

Large currency value.

### Total Target

Large currency value.

### Achievement %

Primary KPI card.

Largest visual emphasis.

### Portfolio Coverage %

Secondary KPI card.

### Portfolio Return %

Secondary KPI card.

## Visual Rules

Achievement:

* Higher is better

Coverage:

* Higher is better

Return:

* Higher is worse

---

# 6. Opportunity & Risk Board

## Purpose

Tell management where attention should be directed.

This section replaces the need to visually inspect multiple tables.

## Position

Directly below Portfolio Overview.

Must be visible without scrolling on large monitors.

---

## Layout

Two columns:

### Opportunities

Left side.

### Risks

Right side.

---

## Opportunity Card

Example:

Principal:
ABC Food

Reason:
Coverage P85
Achievement P20

Interpretation:
Strong market reach but underperforming sales.

---

## Risk Card

Example:

Principal:
XYZ Pharma

Reason:
Return P95

Interpretation:
Return level significantly above portfolio peers.

---

## Rules

Maximum:

5 Opportunities

5 Risks

Display highest priority items only.

---

## Card Content

Every card must display:

* Principal Name
* Category
* Supporting KPI
* Percentile
* Why the card exists

No hidden logic.

---

# 7. Achievement Gap Leaderboard

## Purpose

Show which Principals are furthest from expected performance.

## Visual Style

Ranked Horizontal Bar List.

Not a table.

---

## Sort

Descending by:

AchievementAmount (PRN-TGT-002)

---

## Display

Rank

Principal

Gap Amount

Achievement %

Sales

Target

---

## Visual Emphasis

Top 3 items receive stronger visual prominence.

---

## User Question Answered

"Which Principals need sales attention first?"

---

# 8. Coverage & Reach Analysis

## Purpose

Identify market penetration opportunities.

---

## Visual Style

Coverage vs Achievement Scatter Plot

X Axis:
Coverage %

Y Axis:
Achievement %

---

## Highlight

Automatically highlight:

Coverage P70+
Achievement P40-

These are opportunity candidates.

---

## Supporting Table

Below chart:

Principal

Active Customer

Total Customer

Coverage %

Achievement %

---

## User Question Answered

"Which Principals have customer reach but weak sales performance?"

---

# 9. Return Risk Analysis

## Purpose

Identify quality and operational risk.

---

## Visual Style

Ranked Bar Chart

Not pie chart.

Not donut chart.

---

## Sort Options

Return Amount

Return %

---

## Display

Principal

Return Amount

Return %

Good Return

Broken Return

---

## Highlight

Return Percentile >= 80

---

## User Question Answered

"Which Principals create the largest return risk?"

---

# 10. Salesman Dependency Analysis

## Purpose

Identify concentration risk.

---

## Visual Style

100% Stacked Contribution Bars

One row per Principal.

Example:

Principal A

███████████░░░░░░░░░

Top Salesman 55%

Principal B

██████░░░░░░░░░░░░░░

Top Salesman 30%

---

## Display

Principal

Top Contributor

Contribution %

Dependency Ratio

Dependency Rank

Contributing Salesman Count

Contribution Coverage

---

## Sort

Dependency Ratio Descending

---

## User Question Answered

"Which Principals depend too heavily on a small number of salesmen?"

---

# 11. Principal Detail Table

## Purpose

Provide detailed validation.

Not intended as the primary dashboard experience.

---

## Default Columns

Principal

Sales

Target

Achievement %

Achievement Gap

Coverage %

Return %

MoM Growth

YoY Growth

Dependency Rank

---

## Behavior

Sortable

Filterable

Searchable

---

## Drill Down

Click Principal

Navigate to:

Principal Analytics

Evidence

Supporting Rankings

Existing analytics screens

---

# 12. Visual Design Rules

## Preferred Visuals

* KPI Cards
* Horizontal Rankings
* Scatter Plots
* Stacked Contribution Bars
* Status Cards

## Avoid

* Pie Charts
* Donut Charts
* Speedometers
* Gauge Charts
* 3D Charts
* Decorative Graphics

---

# 13. Color Semantics

Must remain consistent across dashboard.

## Positive

Higher is Better

* Achievement %
* Coverage %
* Sales

## Negative

Higher is Worse

* Return %
* Dependency Ratio
* Achievement Gap

---

# 14. Missing Data Rules

All missing values display:

"No Data"

Never:

* 0
* Empty string
* Dash only

Unknown must never be interpreted as zero.

---

# 15. Navigation Rules

Dashboard
→ Principal Detail

Dashboard
→ Evidence

Dashboard
→ Supporting Rankings

Dashboard
→ Entity Analytics

The dashboard is a decision surface.

Detailed investigation belongs in downstream analytics screens.

---

# 16. Success Criteria

A management user should be able to answer the following questions within 30 seconds:

1. Is the portfolio healthy?
2. Which Principals need attention?
3. Which Principals are opportunities?
4. Which Principals are creating return risk?
5. Which Principals are concentrated on a small number of salesmen?
6. Which Principal should I discuss first in a management meeting?

If these questions cannot be answered without opening the detail table, the dashboard design has failed.
