# Sales Force Overview Dashboard (Field Activity — Management View)

## Document Status

| Field | Value |
| --- | --- |
| Milestone | M18.6 — Sales Force Overview Dashboard |
| Module | BTR Portal — Sales Force / Field Activity |
| Prior art | M18.5 Field Activity Control Tower (SF02 — individual operational view) |
| Status | **Approved for architecture** — pending implementation |
| Audience | Product Owners, Sales Management, Architect, Implementer |

---

## 1. Purpose

### Business problem

The current **Field Activity** dashboard (M18.5) answers:

> *How did **this** salesman perform on **this** day?*

That design serves supervisors doing daily operational review of one rep at a time. It does **not** serve Owners, Directors, General Managers, or Sales Managers who must monitor the **entire sales organization** without opening every salesman individually.

### Business objective

Introduce a **Sales Force Overview** dashboard that answers:

> *How is the entire sales organization performing today, and which salesmen require management attention?*

This dashboard becomes the **primary landing page** for the Field Activity module. The existing individual Control Tower view remains available as the drill-down detail page.

### Design philosophy

Follow the established BTR Portal management pattern:

```text
Management Attention → Comparison → Investigation → Detail
```

Comparison is the primary UX goal. Management must identify best performers, worst performers, coaching candidates, and low visit execution **without** opening multiple salesman dashboards.

---

## 2. Users and Access

| User | Goal | Primary sections |
| ---- | ---- | ---------------- |
| Owner / Director | Executive scan of field execution health | Company KPI summary, rankings, trends |
| General Manager | Compare sales force productivity | Performance table, comparison charts |
| Sales Manager | Identify coaching targets | Performance table, bottom rankings, status indicators |
| Area Supervisor | Find under-performing reps in territory | Table filters, rankings, drill-down to detail |

**Access:** Same Portal authentication as existing dashboards. No new RBAC in V1.

---

## 3. Scope

### In scope (M18.6 V1)

| Capability | Description |
| ---------- | ----------- |
| Company-wide Field Activity KPI summary | Aggregated team KPIs for selected date |
| Salesman performance comparison table | One row per salesman; sortable, searchable, color-coded |
| KPI comparison charts | Horizontal bar charts for key metrics across all salesmen |
| Performance rankings | Top and bottom performers (Top 5 default, Top 10 optional toggle) |
| Historical trends | Last 7 and last 30 days for team-level execution metrics |
| Drill-down | Any salesman click opens existing M18.5 Control Tower with salesman and date pre-selected |
| Date selection | Today (default) · Yesterday · Custom Date — same model as M18.5 |
| Navigation restructure | Overview becomes Field Activity module landing page |

### Out of scope (M18.6 V1)

| Capability | Reason |
| ---------- | ------ |
| Map-centric team view (all salesmen on one map) | Deferred from M18.5 Release 2 — high data volume; not required for comparison table UX |
| Visit History standalone page | Not implemented anywhere; future milestone |
| GPS Validation standalone page | GPS validation remains embedded in salesman detail; no separate page in codebase |
| Route replay at team level | Remains salesman-detail only |
| Alert Center integration | M18.5 Release 2 / M23 |
| Visit plan editing | Desktop SM7 |
| New GIS / territory map architecture | Section 6 optional uses existing Wilayah dimension only |
| Future KPIs: visit duration, check-in time distribution, first/last visit time | Requires new data rules — defer to M25 |

### Preserved behavior

- Existing M18.5 Control Tower functionality is **unchanged** — only relocated under drill-down route.
- M18 Salesman Performance dashboard (`/dashboard/salesmen`) remains separate — outcome KPIs (omzet, achievement, piutang) are not duplicated here.
- All M18.5 KPI definitions and business rules (RO3 effective call, RO1 GPS bands, visit dedupe) apply unchanged at per-salesman grain.

---

## 4. Dashboard Sections

### Section 1 — Company KPI Summary

Executive summary of field execution for the selected date.

| KPI | Business meaning |
| --- | ---------------- |
| Active Salesmen | Reps with field data capability (has Email) and at least one planned visit or check-in on the date |
| Planned Visits | Sum of planned customers across all salesmen |
| Actual Visits | Sum of distinct customer check-ins across all salesmen |
| Visit Execution % | `SUM(Actual) ÷ SUM(Planned) × 100` — team ratio, not average of individual percentages |
| Effective Calls | Sum of visits producing ≥1 order same day |
| Effective Call Rate | `SUM(Effective) ÷ SUM(Actual) × 100` |
| Missed Visits | Sum of planned-not-visited customers |
| Unplanned Visits | Sum of check-ins not on effective plan |
| GPS Valid Rate | `SUM(GPS Valid stops) ÷ SUM(classifiable GPS stops) × 100` — excludes Invalid (zero-coordinate) stops |

Display **N/A** when denominator is zero, consistent with M18.5.

### Section 2 — Salesman Performance Table (primary component)

One row per salesman. This is the main management tool.

| Column | Description |
| ------ | ----------- |
| Rank | Default rank by Visit Execution % (configurable sort overrides) |
| Salesman Code | `SalesPersonCode` |
| Salesman Name | `SalesPersonName` |
| Planned Visits | Per SF-KPI-012 |
| Actual Visits | Per SF-KPI-013 |
| Visit Execution % | Per SF-KPI-017 |
| Effective Calls | Per SF-KPI-016 |
| Effective Call Rate | Per SF-KPI-018 |
| Missed Visits | Per SF-KPI-014 |
| Unplanned Visits | Per SF-KPI-015 |
| GPS Valid % | Valid ÷ (Valid + Warning + Suspicious) for actual stops |
| Sales Orders | Count of `BTR_Order` rows on the visit date for the rep |
| Omzet | Sum of `BTR_Order.TotalAmount` on the visit date — **field order value**, not Faktur omzet |
| Status Indicator | Composite coaching signal (see §5.3) |

**Requirements:**

- Sortable on every numeric column
- Global text search on code and name
- Color-coded KPI cells using portal threshold bands
- Row click → drill-down to salesman detail dashboard
- Salesmen without Email shown with muted row and **No Field Data** badge — excluded from team ratio denominators where appropriate but visible for management gap visibility (consistent with M18.5 selector behavior)

### Section 3 — KPI Comparison Charts

Horizontal bar charts displaying **all salesmen simultaneously**:

| Chart | Metric | Purpose |
| ----- | ------ | ------- |
| Visit Execution % | Per-salesman execution | Identify under-performers immediately |
| Effective Call Rate | Per-salesman rate | Compare visit productivity |
| Orders Generated | Order count per salesman | Compare conversion from visits to orders |
| Omzet Generated | Order `TotalAmount` per salesman | Compare business impact from field orders |

Charts must support 20–100 salesmen. Vertical scroll or horizontal chart scroll is acceptable. Reuse portal horizontal bar chart pattern (`InventoryHorizontalBarChart` style).

Bar click → same drill-down as table row.

### Section 4 — Performance Rankings

Pre-built ranking cards for management attention. Default **Top 5**; user toggle to **Top 10**.

| Ranking card | Sort | Direction |
| ------------ | ---- | --------- |
| Top Visit Execution | Visit Execution % | Descending (min planned visits threshold: ≥ 1) |
| Bottom Visit Execution | Visit Execution % | Ascending (min planned ≥ 1) |
| Top Effective Call Rate | Effective Call Rate | Descending (min actual ≥ 1) |
| Bottom Effective Call Rate | Effective Call Rate | Ascending (min actual ≥ 1) |
| Top Omzet | Order TotalAmount | Descending |
| Top Orders | Order count | Descending |
| Most Missed Visits | Missed count | Descending |
| Most Unplanned Visits | Unplanned count | Descending |

Each card shows rank, code, name, primary metric value. Card row click → drill-down.

### Section 5 — Trends

Team-level historical trends for context (temporary vs consistent under-performance).

| Horizon | Metrics |
| ------- | ------- |
| Last 7 days | Visit Execution %, Effective Call Rate, Orders, Omzet |
| Last 30 days | Same four metrics |

Trend lines represent **team aggregates** per day (same sum-ratio formulas as Section 1), not averages of individual percentages.

### Section 6 — Territory Coverage (optional V1)

Implement **only if** achievable without new GIS infrastructure.

**Approved optional V1 deliverable:** Horizontal bar chart **Visits by Wilayah** — actual visit count grouped by salesman `WilayahName` (from `BTR_SalesPerson`).

**Deferred:** Map of all today's visits across the team (M18.5 Release 2 team map).

---

## 5. Business Rules

### 5.1 KPI grain and formulas

All per-salesman KPIs reuse M18.5 / SF-KPI-012–018 definitions via `FieldActivityComposer` logic. Team aggregates use **sum-ratio** method documented in KPI catalog for SF-KPI-017:

| Team KPI | Formula |
| -------- | ------- |
| Visit Execution % | `SUM(ActualVisits) ÷ SUM(PlannedVisits) × 100` |
| Effective Call Rate | `SUM(EffectiveCalls) ÷ SUM(ActualVisits) × 100` |
| GPS Valid Rate | `SUM(GpsValidCount) ÷ SUM(GpsValid + GpsWarning + GpsSuspicious) × 100` |

Do **not** average individual percentages — mathematically incorrect when planned visit counts differ.

### 5.2 Business KPIs (field orders)

| KPI | Source | Rule |
| --- | ------ | ---- |
| Sales Orders | `BTR_Order` | `COUNT(*)` where `OrderDate = visitDate` and `UserEmail = salesman.Email` |
| Omzet Generated | `BTR_Order.TotalAmount` | `SUM(TotalAmount)` same filter |
| Average Omzet per Visit | Derived | `Omzet ÷ ActualVisits` — display in table optional column V1.1; not required for V1 headline |
| Customers Visited | Same as Actual Visits | No separate calculation |
| Average Orders per Visit | Derived | `Orders ÷ ActualVisits` — optional column |

**Semantic note:** Omzet here is **field order value**, not invoiced Faktur omzet (SF01). Label clearly as **Order Omzet** or **Orders Value** in UI to avoid confusion with M18.

### 5.3 Status Indicator

Composite coaching signal per salesman for the selected date:

| Status | Rule |
| ------ | ---- |
| **On Track** | Visit Execution ≥ 80% AND Effective Call Rate ≥ 50% |
| **Needs Attention** | Visit Execution 50–79% OR Effective Call Rate 30–49% OR Unplanned Visits ≥ 3 |
| **Critical** | Visit Execution < 50% OR (Planned ≥ 1 AND Actual = 0) |
| **No Plan** | Planned = 0 and no check-ins |
| **No Field Data** | Salesman has no Email configured |

Thresholds align with portal Warning/Critical band philosophy (80/100 split adapted for execution).

### 5.4 Date and plan availability

- Default date: **Today** (business date from `IBusinessDateProvider`)
- Dates before `VisitPlanGoLiveDate`: planned KPIs = 0; display plan availability banner (reuse M18.5 meta semantics)
- No visit-plan history backfill (M18.5 PO Q15)

### 5.5 Active salesman definition

For **Active Salesmen** headline KPI: count salesmen where `HasEmail = true` AND (`PlannedVisits > 0` OR `ActualVisits > 0`) on the selected date.

---

## 6. Navigation and Drill-down

### Target navigation structure

```text
Sales Force (sidebar group)
├── SF02 · Sales Force Overview     → /dashboard/field-activity          (NEW landing)
├── SF03 · Salesman Field Activity  → /dashboard/field-activity/detail   (existing Control Tower)
└── SF01 · Salesmen Performance     → /dashboard/salesmen                (unchanged — outcome lens)
```

**Future (not M18.6):**

```text
├── Visit History                   → not implemented
└── GPS Validation                  → not implemented (GPS remains in salesman detail)
```

### Drill-down flow

```text
Sales Force Overview
        ↓ click salesman (table, chart, or ranking)
Salesman Field Activity Detail (M18.5 Control Tower)
        ↓ optional cross-link
Salesmen Performance (M18)
```

Drill-down passes `salesPersonId` and `visitDate` query parameters. Detail page auto-loads when both params present.

---

## 7. UX Requirements

| Requirement | Detail |
| ----------- | ------ |
| Comparison first | Performance table above the fold on desktop; charts and rankings below |
| No multi-open workflow | Management identifies outliers from overview alone |
| Color coding | Execution % and Effective Call Rate use green / amber / red bands (≥80 / 50–79 / <50) |
| Search | Filter table by salesman code or name |
| Sort | Default: Visit Execution % ascending (worst first) for coaching orientation; user can re-sort |
| Loading | Single API call for overview payload — no per-salesman loading spinners |
| Empty state | When no salesmen have field data: explain Email configuration gap |
| Freshness | Display `GeneratedAt` for snapshot-backed today view; `QueriedAt` for on-demand historical dates |

---

## 8. Acceptance Criteria

| # | Criterion |
| - | --------- |
| AC-1 | `/dashboard/field-activity` renders Sales Force Overview with all six sections (Section 6 optional) |
| AC-2 | Company KPI summary matches manual sum of per-salesman values for a test date |
| AC-3 | Performance table lists all salesmen; sort and search work on every column |
| AC-4 | Row/chart/ranking click navigates to detail page with correct salesman and date |
| AC-5 | Existing Control Tower map, replay, and KPI strip work unchanged on detail route |
| AC-6 | Team Visit Execution % uses sum-ratio, verified against KPI catalog rule |
| AC-7 | Orders and Omzet columns match `BTR_Order` counts and sums for test salesmen |
| AC-8 | 7-day and 30-day trend charts render team daily aggregates |
| AC-9 | Overview loads in < 3 seconds for 100 salesmen in staging |
| AC-10 | No regression to M18.5 API `/api/dashboard/field-activity` single-salesman contract |

---

## 9. Relationship to Other Features

| Feature | Relationship |
| ------- | ------------ |
| M18.5 Field Activity (SF02 → SF03) | Detail drill-down; reuses composer logic and components |
| M18 Salesman Performance (SF01) | Complementary — outcome vs execution lens; cross-link preserved |
| M25 Sales Force Effectiveness | Will consume execution KPIs (Q20 inputs); overview becomes data source |
| M23 Alert Center | Future — unplanned visit and low execution signals |
| Visit Plan (`docs/features/visit-plan/feature.md`) | Planned visit denominator source |

---

## 10. Open Questions — Resolved for Architecture

| # | Question | Decision |
| - | -------- | -------- |
| Q1 | Replace or extend SF02 route? | **Split routes** — Overview at `/dashboard/field-activity`; detail at `/dashboard/field-activity/detail` |
| Q2 | New menu code? | **SF03** for Salesman Field Activity; **SF02** label becomes Sales Force Overview |
| Q3 | Omzet meaning | **BTR_Order.TotalAmount** (field orders) — not Faktur; label as Order Value in UI |
| Q4 | Default table sort | **Worst execution first** (ascending execution %) for coaching |
| Q5 | Territory section | **Optional** — Wilayah visit bar chart only; no team map |
| Q6 | Ranking size | Top 5 default, Top 10 toggle |

---

*Analyst deliverable for M18.6. Hand off to Architect: `docs/work/btr-portal/M18-6-field-activity-overview/implementation-plan.md`.*
