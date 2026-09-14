# FEASIBILITY ASSESSMENT

## Sales Force Overview Dashboard — UX Blueprint Review

---

## 1. Executive Summary

### Request

Evaluate the feasibility and consistency of the proposed Sales Force Overview Dashboard UX Blueprint v2.0 against the existing API structure, data model, and dashboard architecture principles.

### Recommendation

**Recommended with Significant Changes.**

The UX Blueprint demonstrates strong business thinking and a sound mental model. However, several proposed visualizations exceed the available data, introduce redundancy with existing dashboards, or risk misleading users. The Blueprint requires targeted adjustments before implementation.

### Feasibility Result

```text
PARTIALLY FEASIBLE
```

**Reason:** 5 of 7 proposed sections are fully feasible. 2 sections (Execution Funnel, Revenue Distribution) require data model extensions or risk producing misleading visualizations.

---

## 2. Request Understanding

### Requested Capability

Redesign the Sales Force Overview dashboard (`/portal/dashboard/field-activity`) following a new UX Blueprint that organizes information into 7 sections aligned to a Plan → Execute → Convert → Sell → Monitor → Improve mental model.

### Business Objective

Provide business owners, sales managers, and supervisors with a single-page operational overview that answers:
1. Are salespeople executing planned activities?
2. Are field visits productive?
3. Are activities generating orders and revenue?
4. Which areas or individuals require management attention?

### Expected Outcome

A dashboard that enables a business owner to assess field sales health within 30 seconds and identify intervention targets within 60 seconds.

---

## 3. Current State Analysis

### Existing API Structure

**Endpoint:** `GET /api/dashboard/field-activity/overview?visitDate=YYYY-MM-DD`

**Response Structure:**

| Section | Fields | Notes |
|---------|--------|-------|
| `TeamKpis` | ActiveSalesmenCount, PlannedVisits, ActualVisits, VisitExecutionPercent, EffectiveCalls, EffectiveCallRate, MissedVisits, UnplannedVisits, GpsValidRate, TotalOrders, TotalOmzet | 11 team-level KPIs |
| `Salesmen[]` | 18 fields per row including StatusCode | Per-salesman breakdown |
| `Rankings` | 8 lists (Top/Bottom x4 metrics) | Pre-computed leaderboards |
| `Trends` | Last7Days[], Last30Days[] | 4 metrics per point |
| `WilayahBreakdown[]` | WilayahName, ActualVisits | Region-level aggregation |
| `Meta` | PlanDataAvailable, VisitPlanGoLiveDate | Data availability flags |

### Existing Frontend Components

| Component | Purpose |
|-----------|---------|
| `FieldActivityTeamKpiStrip` | Team headline KPIs |
| `FieldActivitySalesmanTable` | Sortable per-salesman comparison |
| `FieldActivityComparisonChart` | Horizontal bar charts (4 metrics) |
| `FieldActivityRankingGrid` | 8 ranking lists |
| `FieldActivityTeamTrendChart` | 7-day and 30-day trend lines |
| `FieldActivityWilayahChart` | Region breakdown |

### Existing Data Flow

```
Snapshot (today, 15-min refresh)
    ↓
BTRPD_FieldActivityKpi + BTRPD_FieldActivitySalesman + BTRPD_FieldActivityTrend
    ↓
Live Batch (historical dates)
    ↓
FieldActivityOverviewComposer → FieldActivityKpiCalculator → FieldActivityStatusPolicy
```

---

## 4. Part A — Business Fit

### Owner Persona

| Question | Blueprint Answer Quality | Gap |
|----------|--------------------------|-----|
| Are salespeople executing today? | ✅ Strong (Visit Execution %) | None |
| Are visits generating orders? | ⚠️ Partial (Effective Call Rate) | No order-to-visit conversion rate |
| How much revenue today? | ✅ Strong (TotalOmzet) | None |
| Who needs intervention? | ✅ Strong (Attention Center) | None |
| Is performance improving? | ✅ Strong (Trend Analysis) | None |
| Which territory is underperforming? | ⚠️ Partial | Only ActualVisits available; no revenue/orders by territory |

**Unanswered:** Revenue productivity per visit, workload balance across team, capacity utilization.

### Sales Manager Persona

| Question | Blueprint Answer Quality | Gap |
|----------|--------------------------|-----|
| Which salesman is underperforming? | ✅ Strong | None |
| Why is execution low? | ❌ Weak | No root cause indicators |
| Are GPS check-ins legitimate? | ✅ Strong (GPS Valid Rate) | None |
| Which territory needs coaching? | ⚠️ Partial | No territory-level revenue/orders |
| Is the team improving week-over-week? | ⚠️ Partial | Only 7-day/30-day trend; no week-over-week comparison |

**Unanswered:** Visit duration quality, customer engagement depth, coaching priority signals.

### Sales Supervisor Persona

| Question | Blueprint Answer Quality | Gap |
|----------|--------------------------|-----|
| Who didn't execute their plan? | ✅ Strong (Missed Visits) | None |
| Who made unplanned visits? | ✅ Strong (Unplanned Visits) | None |
| Which salesman has GPS issues? | ✅ Strong (GPS Issues) | None |
| Are plans realistic? | ❌ Weak | No plan quality indicators |

**Unanswered:** Plan adherence by route, customer coverage gaps.

---

## 5. Part B — Mental Model Review

### Proposed Flow

```text
Plan → Execute → Convert → Sell → Monitor → Improve
```

### Evaluation

| Dimension | Assessment |
|-----------|------------|
| **Correctness** | ✅ The flow correctly represents the field activity business process |
| **Completeness** | ⚠️ Missing "Plan Quality" — plans may be unrealistic |
| **Order** | ⚠️ "Monitor" should be implicit throughout, not a separate stage |
| **Mapping to Sections** | ⚠️ Weak — the 7 sections don't cleanly map to the 6 stages |

### Section-to-Stage Mapping Issues

| Section | Mapped Stage | Issue |
|---------|--------------|-------|
| A. Operating Health | Monitor | ✅ Correct |
| B. Execution Funnel | Plan→Execute→Convert→Sell | ⚠️ Spans 4 stages; too broad |
| C. Execution Quality | Execute | ✅ Correct |
| D. Territory Performance | Monitor | ⚠️ Could also be "Improve" |
| E. Revenue Distribution | Sell | ✅ Correct |
| F. Attention Center | Improve | ✅ Correct |
| G. Trend Analysis | Monitor | ✅ Correct |

### Recommended Mental Model

A better mental model for this specific dashboard:

```text
Status (What happened today?)
    ↓
Performance (How well did it happen?)
    ↓
Outcomes (What did it produce?)
    ↓
Exceptions (Who needs help?)
    ↓
Trends (Is it getting better or worse?)
```

This is simpler, requires fewer cognitive leaps, and maps more naturally to the proposed sections.

### Section Order Issues

The proposed order places "Revenue Distribution" (Section E) after "Territory Performance" (Section D). This is incorrect for an owner persona — revenue outcomes are more important than territory breakdown. Revenue should appear immediately after the funnel.

---

## 6. Part C — Data Feasibility Review

### C.1 Sales Force Operating Health

**Proposed KPIs vs Available Data:**

| Proposed KPI | Available | Source Field | Verdict |
|--------------|-----------|--------------|---------|
| Active Salesmen | ✅ | `TeamKpis.ActiveSalesmenCount` | Fully feasible |
| Visit Execution % | ✅ | `TeamKpis.VisitExecutionPercent` | Fully feasible |
| Actual Visits | ✅ | `TeamKpis.ActualVisits` | Fully feasible |
| Effective Call Rate | ✅ | `TeamKpis.EffectiveCallRate` | Fully feasible |

**Assessment:** ✅ Fully feasible. All 4 KPIs are available in the API.

**Risk:** "Active Salesmen" counts salesmen with email configured, not necessarily salesmen who worked today. The label may mislead users into thinking these salesmen were active in the field.

**Recommendation:** Consider renaming to "Eligible Salesmen" or adding a secondary "Salesmen with Activity" count.

---

### C.2 Execution Funnel

**Proposed Funnel Stages:**

| Stage | Available | Source Field | Verdict |
|-------|-----------|--------------|---------|
| Planned Visits | ✅ | `TeamKpis.PlannedVisits` | Fully feasible |
| Actual Visits | ✅ | `TeamKpis.ActualVisits` | Fully feasible |
| Effective Calls | ✅ | `TeamKpis.EffectiveCalls` | Fully feasible |
| Orders | ✅ | `TeamKpis.TotalOrders` | Fully feasible |
| Revenue | ✅ | `TeamKpis.TotalOmzet` | Fully feasible |

**Assessment:** ⚠️ Partially feasible with significant risks.

**Critical Issues:**

1. **Stage 2→3 Conversion Misleading:** "Effective Calls" is defined as visits where the customer placed an order. The conversion from "Actual Visits" to "Effective Calls" measures order placement rate, not visit quality. The label "Effective Call Rate" suggests visit productivity but actually measures order conversion.

2. **Stage 3→4 Logic Gap:** "Effective Calls" counts unique customers who ordered. "Orders" counts total orders. A customer can place multiple orders. The funnel implies linear conversion but the metrics measure different things:
   - Effective Calls = distinct customers with orders
   - Orders = total order count (may include multiple orders per customer)

3. **Stage 4→5 Missing Context:** Orders → Revenue conversion is just average order value. This is not a meaningful conversion metric.

4. **Missing Conversion Metrics:**
   - Visit-to-Order conversion (Actual Visits → Orders)
   - Revenue per Visit
   - Revenue per Effective Call

**Recommendation:** Simplify to 3 stages: Planned Visits → Actual Visits → Orders (with Revenue annotation). Or add "Customers Ordered" as a distinct stage between "Effective Calls" and "Orders."

---

### C.3 Execution Quality

**Proposed KPIs vs Available Data:**

| Proposed KPI | Available | Source Field | Verdict |
|--------------|-----------|--------------|---------|
| GPS Valid Rate | ✅ | `TeamKpis.GpsValidRate` | Fully feasible |
| Missed Visits | ✅ | `TeamKpis.MissedVisits` | Fully feasible |
| Unplanned Visits | ✅ | `TeamKpis.UnplannedVisits` | Fully feasible |
| GPS Warning Count | ⚠️ | Per-salesman only (`Salesmen[].GpsWarningCount`) | Requires aggregation |
| GPS Suspicious Count | ⚠️ | Per-salesman only (`Salesmen[].GpsSuspiciousCount`) | Requires aggregation |

**Assessment:** ⚠️ Partially feasible.

**Issue:** GPS Warning Count and GPS Suspicious Count are available at the salesman level but not in `TeamKpis`. The Blueprint proposes "GPS Issues" as a single KPI combining these. Two options:
1. Sum from `Salesmen[]` array (frontend aggregation)
2. Add to `TeamKpis` in backend (cleaner)

**Recommendation:** Add `GpsWarningCount` and `GpsSuspiciousCount` to `FieldActivityTeamKpis` in the backend. This is a minor backend change.

**Missing Quality Indicators:**
- Visit duration (time spent at customer)
- Check-in accuracy distribution
- Route deviation metrics

---

### C.4 Territory Performance

**Proposed Visualization:** Ranked horizontal bar chart with selectable measures (Actual Visits, Orders, Revenue, Effective Call Rate).

**Available Data:**

| Measure | Available at Territory Level | Source |
|---------|------------------------------|--------|
| Actual Visits | ✅ | `WilayahBreakdown[].ActualVisits` |
| Orders | ❌ | Not in `WilayahBreakdown` |
| Revenue | ❌ | Not in `WilayahBreakdown` |
| Effective Call Rate | ❌ | Not in `WilayahBreakdown` |

**Assessment:** ❌ Not feasible as proposed.

**Critical Gap:** `FieldActivityWilayahBreakdownRow` only contains `WilayahName` and `ActualVisits`. Orders, Revenue, and Effective Call Rate are not available at the territory level.

**Options:**
1. **Extend backend:** Add Orders, Omzet, EffectiveCalls to `WilayahBreakdown` (requires modifying `FieldActivityOverviewComposer` and snapshot tables)
2. **Frontend aggregation:** Compute from `Salesmen[]` array grouped by `WilayahName` (possible but limited to salesman-level data)
3. **Reduce scope:** Only show Actual Visits in territory chart

**Recommendation:** Option 2 (frontend aggregation) for initial implementation. This provides Orders, Revenue, and Effective Call Rate by aggregating salesman rows. Option 1 for production quality.

---

### C.5 Revenue Distribution

**Proposed Visualization:** Contribution chart showing revenue concentration by salesman.

**Available Data:**

| Data Point | Available | Source |
|------------|-----------|--------|
| Revenue per Salesman | ✅ | `Salesmen[].OmzetAmount` |
| Revenue share % | ❌ | Must compute from TotalOmzet |
| Top N vs Others | ❌ | Must compute |

**Assessment:** ✅ Fully feasible with frontend computation.

**Risk:** The Blueprint shows "Tiara 40%, Mala 17%, Anggar 9%, Others 34%". This implies a Pareto-style concentration analysis. However, with a typical sales team of 16 salesmen, "Others" may represent 13 salesmen — the chart may not reveal meaningful concentration patterns if revenue is relatively evenly distributed.

**Recommendation:** Consider adding a Gini coefficient or concentration ratio (Top 3 %) as a summary indicator alongside the chart.

---

### C.6 Attention Center

**Proposed Lists:**

| List | Available | Source | Verdict |
|------|-----------|--------|---------|
| Lowest Visit Execution | ✅ | `Rankings.BottomVisitExecution` | Fully feasible |
| Lowest Effective Call Rate | ✅ | `Rankings.BottomEffectiveCallRate` | Fully feasible |
| Highest Unplanned Visits | ✅ | `Rankings.MostUnplannedVisits` | Fully feasible |
| Highest GPS Issues | ❌ | Not in `Rankings` | Missing |

**Assessment:** ⚠️ Mostly feasible.

**Gap:** "Highest GPS Issues" is not a pre-computed ranking. The API provides `MostMissedVisits` and `MostUnplannedVisits` but not a GPS issues ranking.

**Options:**
1. Add `MostGpsIssues` to `FieldActivityRankingSection` (backend change)
2. Compute from `Salesmen[]` by summing `GpsWarningCount + GpsSuspiciousCount` (frontend)

**Recommendation:** Option 2 for initial implementation. Option 1 for production.

**Missing Attention Signals:**
- Salesmen with no activity (Critical status)
- Salesmen with declining trend (week-over-week drop)
- Salesmen with zero orders despite visits

---

### C.7 Trend Analysis

**Proposed Charts:**

| Trend | Available | Source | Verdict |
|-------|-----------|--------|---------|
| Visit Execution Trend | ✅ | `Trends.Last30Days[].VisitExecutionPercent` | Fully feasible |
| Effective Call Rate Trend | ✅ | `Trends.Last30Days[].EffectiveCallRate` | Fully feasible |
| Orders Trend | ✅ | `Trends.Last30Days[].OrdersCount` | Fully feasible |
| Revenue Trend | ✅ | `Trends.Last30Days[].OmzetAmount` | Fully feasible |

**Assessment:** ✅ Fully feasible.

**Issue:** The API provides both `Last7Days` and `Last30Days` trends. The Blueprint only mentions "Last 30 Days". The 7-day view is useful for recent pattern detection.

**Recommendation:** Offer both 7-day and 30-day views via toggle, as the existing implementation already does.

---

## 7. Part D — Information Architecture Review

### Visibility Hierarchy

| Current Blueprint Order | Recommended Order | Rationale |
|-------------------------|-------------------|-----------|
| A. Operating Health | A. Operating Health | ✅ Correct — most important, must be above fold |
| B. Execution Funnel | B. Execution Funnel | ✅ Correct — primary narrative |
| C. Execution Quality | E. Revenue Distribution | Revenue outcomes are more important than quality metrics for owners |
| D. Territory Performance | F. Attention Center | Exceptions should surface before territory analysis |
| E. Revenue Distribution | C. Execution Quality | Quality is diagnostic, belongs lower |
| F. Attention Center | D. Territory Performance | Territory is analytical, belongs lower |
| G. Trend Analysis | G. Trend Analysis | ✅ Correct — trends are reference, not urgent |

### Scannability Issues

1. **Too many KPI cards:** Section A has 4 cards, Section C has 4 cards. Total: 8 KPI cards before reaching the funnel. This exceeds the "5±2" cognitive load limit.

2. **Funnel width:** A 5-stage horizontal funnel requires significant horizontal space. On mobile or narrow screens, this will compress to unreadability.

3. **Attention Center:** 4 ranked lists side-by-side requires wide screens. May stack vertically on tablets, defeating the purpose.

### Cognitive Load

| Issue | Severity | Recommendation |
|-------|----------|----------------|
| 8 KPI cards before funnel | High | Merge Operating Health and Execution Quality into a single 6-card strip |
| 4 trend charts | Medium | Show 2 by default (Visit Execution, Revenue); hide others behind toggle |
| 4 attention lists | Medium | Show 2 by default (Lowest Execution, Lowest ECR); hide others behind toggle |

### Progressive Disclosure

The Blueprint correctly defines Level 1 (Overview) and Level 2 (Drill-Down). However:

**Missing Level 0:** No executive summary or health indicator. An owner should see a single "Field Health: ⚠️ Needs Attention" indicator before reading any KPIs.

**Missing Level 3:** No link to historical reports or comparison views.

### Signal-to-Noise Ratio

| Signal | Noise Risk |
|--------|------------|
| Visit Execution % | Low — direct measure |
| Effective Call Rate | Medium — confusing definition |
| GPS Valid Rate | Medium — technical metric, low business relevance for owners |
| GPS Issues count | High — raw count without context |
| Unplanned Visits | Medium — may be positive (proactive selling) |

---

## 8. Part E — Dashboard Anti-Pattern Detection

### E.1 Vanity Metrics

| Metric | Issue | Severity |
|--------|-------|----------|
| Active Salesmen | Counts eligible salesmen, not active ones. A salesman with 0 visits still counts as "Active." | Medium |

### E.2 Redundant KPIs

| KPI | Redundant With | Issue | Severity |
|-----|----------------|-------|----------|
| Actual Visits (Card) | Actual Visits (Funnel Stage 2) | Same number appears in two places | Medium |
| GPS Valid Rate | GPS Issues (Attention Center) | Both measure GPS compliance from different angles | Low |

### E.3 Decorative Charts

| Chart | Issue | Severity |
|-------|-------|----------|
| Territory Performance | If only Actual Visits is available, a simple ranked list would suffice. Bar chart adds visual weight without added insight. | Medium |

### E.4 Unactionable Information

| Information | Issue | Severity |
|-------------|-------|----------|
| GPS Valid Rate trend | Owner cannot act on GPS compliance trends; this is a technical metric | Low |
| Revenue Distribution "Others" | "Others" category is unactionable — who are they? | Low |

### E.5 Duplicate Information

| Information | Locations | Issue | Severity |
|-------------|-----------|-------|----------|
| Visit Execution % | KPI Card (A) + Funnel (B) + Trend (G) | Appears 3 times | Medium |
| Effective Call Rate | KPI Card (A) + Funnel (B) + Attention (F) + Trend (G) | Appears 4 times | High |
| Actual Visits | KPI Card (A) + Funnel (B) + Territory (D) | Appears 3 times | Medium |

### E.6 Ranking Overload

The Attention Center proposes 4 ranked lists. Combined with the existing 8 rankings in `FieldActivityRankingGrid`, users face 12 ranked lists total. This exceeds cognitive capacity.

**Recommendation:** Reduce Attention Center to 2 lists: "Needs Intervention" (composite) and "GPS Issues" (if kept).

### E.7 Attention Dilution

7 sections competing for attention. A business owner scanning the page will struggle to identify the most urgent information.

**Recommendation:** Add a composite "Health Score" or "Status Summary" at the top that distills all sections into a single signal.

### E.8 False Precision

| Metric | Issue | Severity |
|--------|-------|----------|
| Effective Call Rate 19.8% | Displays precision to 1 decimal place. With 106 visits, this is ±0.9% margin. False precision creates false confidence. | Low |
| GPS Valid Rate 82.1% | Same issue | Low |

### E.9 Metrics Without Management Action

| Metric | Missing Action | Severity |
|--------|----------------|----------|
| Unplanned Visits (High) | What should a manager do? Is this good (proactive) or bad (non-compliant)? | Medium |
| GPS Warning Count | What action is expected? | Low |

---

## 9. Part F — Missing Information

### Genuinely Important Missing Indicators

| Indicator | Rationale | Priority |
|-----------|-----------|----------|
| **Orders per Visit** | Direct productivity measure. More meaningful than Effective Call Rate. | High |
| **Revenue per Visit** | Revenue productivity. Critical for owner decision-making. | High |
| **Customers with No Order** | Identifies unproductive visits that consume salesman time. | Medium |
| **Team Capacity Utilization** | Active Salesmen / Total Salesmen. Shows workforce deployment. | Medium |
| **Salesmen with Zero Activity** | Critical intervention signal. Currently buried in "Critical" status. | High |
| **Week-over-Week Change** | Trend direction is more actionable than absolute trend. | Medium |
| **Plan Quality Score** | Are plans realistic? High missed-visit rates may indicate bad plans, not bad execution. | Low |
| **Revenue Concentration Risk** | Top 1 salesman as % of total. Key-person dependency indicator. | Medium |

---

## 10. Part G — Implementation Risk Assessment

### Backend Availability

| Risk | Level | Reasoning |
|------|-------|-----------|
| Team KPIs | Low | All proposed KPIs exist in `TeamKpis` |
| Funnel Data | Low | All funnel stages available in `TeamKpis` |
| Territory Data | **Medium** | Only `ActualVisits` in `WilayahBreakdown`; Orders/Revenue require frontend aggregation or backend extension |
| GPS Issue Rankings | **Medium** | Not in `Rankings` section; requires frontend computation or backend addition |
| Trend Data | Low | All 4 trends available in `Trends.Last30Days` |

### Data Quality

| Risk | Level | Reasoning |
|------|-------|-----------|
| Visit Plan Data | **Medium** | Plan data only available from `VisitPlanGoLiveDate` (2026-03-01). Earlier dates show zero planned visits. |
| GPS Classification | Low | GPS validation is deterministic (distance-based) |
| Order Attribution | **Medium** | Orders are attributed to salesman, not to specific visit. "Effective Calls" uses customer-level matching. |

### Data Completeness

| Risk | Level | Reasoning |
|------|-------|-----------|
| Salesmen without Email | **Medium** | Salesmen without email are excluded from field activity tracking. `HasEmail=false` shows as `NoFieldData`. |
| Missing GPS Coordinates | Low | Handled gracefully with `GpsValidationClass` classification |
| Historical Trend Gaps | Low | Snapshot worker runs every 15 minutes; gaps are minimal |

### Performance

| Risk | Level | Reasoning |
|------|-------|-----------|
| API Response Time | Low | Snapshot-based for today; single API call |
| Frontend Rendering | Low | 16 salesmen × 8 rankings is lightweight |
| Chart Rendering | Low | PrimeVue Chart (Chart.js) handles 30-point trends efficiently |

### UX Complexity

| Risk | Level | Reasoning |
|------|-------|-----------|
| Information Overload | **High** | 7 sections with 8+ KPI cards, 1 funnel, 4 charts, 4 ranked lists |
| Mobile Responsiveness | **Medium** | Horizontal funnel and 4-column attention lists require responsive redesign |
| Cognitive Load | **High** | Too many metrics competing for attention |

### User Adoption

| Risk | Level | Reasoning |
|------|-------|-----------|
| Change Resistance | Low | Existing users already familiar with field activity concepts |
| Learning Curve | Low | KPIs are straightforward |
| Trust | **Medium** | "Effective Call Rate" definition may confuse users expecting "productive visit" semantics |

---

## 11. Part H — Final Verdict

### Overall Assessment

```text
RECOMMENDED WITH SIGNIFICANT CHANGES
```

### Top 5 Strengths

| Rank | Strength |
|------|----------|
| 1 | **Sound mental model.** The Plan→Execute→Convert→Sell→Monitor→Improve flow correctly represents the business process and provides natural information architecture. |
| 2 | **Strong exception management.** The Attention Center design focuses on problems, not averages. This is the correct approach for operational dashboards. |
| 3 | **Complete API coverage.** The existing API provides most data needed for the proposed visualizations. No new API endpoints are required for core functionality. |
| 4 | **Progressive disclosure design.** The Level 1/Level 2 drill-down model correctly separates overview from detail. |
| 5 | **Revenue Distribution insight.** Showing contribution distribution rather than rankings reveals business structure (key-person risk, team balance) that rankings cannot. |

### Top 5 Weaknesses

| Rank | Weakness | Severity |
|------|----------|----------|
| 1 | **Execution Funnel stage definitions are misleading.** "Effective Calls" measures order conversion, not visit quality. The funnel implies linear conversion but stages measure different things. | High |
| 2 | **Excessive KPI duplication.** Visit Execution % appears 3 times; Effective Call Rate appears 4 times. This dilutes attention and wastes screen space. | High |
| 3 | **Territory Performance exceeds available data.** Only Actual Visits is available at territory level. Orders, Revenue, and Effective Call Rate require backend extension. | High |
| 4 | **Missing revenue productivity metrics.** Orders per Visit and Revenue per Visit are absent. These are the most actionable metrics for an owner. | Medium |
| 5 | **Cognitive overload.** 7 sections with 8+ KPI cards, 1 funnel, 4 charts, and 4 ranked lists exceeds the cognitive capacity of a time-constrained owner. | Medium |

### Top 5 Recommended Improvements

| Rank | Improvement | Rationale |
|------|-------------|-----------|
| 1 | **Simplify the funnel to 3 stages:** Planned Visits → Actual Visits → Orders (with Revenue annotation). Add conversion rates between stages. Remove "Effective Calls" from the funnel — it is a quality metric, not a conversion stage. | Eliminates misleading conversion semantics and reduces cognitive load. |
| 2 | **Merge Operating Health and Execution Quality into a single 6-card strip.** Show: Active Salesmen, Visit Execution %, Actual Visits, Orders, Revenue, Effective Call Rate. Remove GPS Valid Rate from the primary strip — move to Execution Quality drill-down. | Reduces KPI cards from 8 to 6 and eliminates duplication. |
| 3 | **Add Orders per Visit and Revenue per Visit** as derived KPIs. These can be computed from existing data (TotalOrders/ActualVisits, TotalOmzet/ActualVisits). | Provides the most actionable productivity metrics currently missing. |
| 4 | **Reduce Attention Center to 2 composite lists:** "Needs Intervention" (merge of Bottom Execution + Bottom ECR + Critical Status) and "GPS Compliance Issues." | Reduces ranking overload and focuses management attention. |
| 5 | **Add a composite Health Score** at the top of the page: "Field Health: ⚠️ Needs Attention" based on `FieldActivityStatusPolicy` distribution across the team. | Provides instant orientation before reading any KPIs. |

---

## 12. GAP ANALYSIS

| GAP-ID | Description | Status | Resolution |
|--------|-------------|--------|------------|
| GAP-001 | Funnel stage "Effective Calls" measures order conversion (customers with orders / total visits), not visit quality. Label misleadingly suggests visit productivity. | OPEN | |
| GAP-002 | Funnel stage 3→4 logic gap: "Effective Calls" counts distinct customers with orders; "Orders" counts total orders (multiple per customer). Funnel implies linear conversion but stages measure different entities. | OPEN | |
| GAP-003 | Visit Execution % appears 3 times (KPI Card A, Funnel B, Trend G). Excessive duplication dilutes attention. | OPEN | |
| GAP-004 | Effective Call Rate appears 4 times (KPI Card A, Funnel B, Attention Center F, Trend G). Excessive duplication dilutes attention. | OPEN | |
| GAP-005 | Actual Visits appears 3 times (KPI Card A, Funnel B, Territory D). Excessive duplication dilutes attention. | OPEN | |
| GAP-006 | `FieldActivityWilayahBreakdownRow` only contains `WilayahName` and `ActualVisits`. Orders, Revenue, and Effective Call Rate not available at territory level. Territory Performance section proposes selectable measures that cannot be fulfilled. | OPEN | |
| GAP-007 | "Highest GPS Issues" ranking not in `FieldActivityRankingSection`. API provides `MostMissedVisits` and `MostUnplannedVisits` but no GPS issues ranking. | OPEN | |
| GAP-008 | `FieldActivityTeamKpis` missing `GpsWarningCount` and `GpsSuspiciousCount`. Values only available per-salesman in `Salesmen[]` array. | OPEN | |
| GAP-009 | Missing derived KPI: Orders per Visit (TotalOrders / ActualVisits). Direct productivity measure more meaningful than Effective Call Rate. | OPEN | |
| GAP-010 | Missing derived KPI: Revenue per Visit (TotalOmzet / ActualVisits). Critical revenue productivity metric for owner decision-making. | OPEN | |
| GAP-011 | "Active Salesmen" label misleading. `ActiveSalesmenCount` counts salesmen with email configured (`HasEmail=true`), not salesmen who actually worked today. A salesman with 0 visits still counts as "Active." | OPEN | |
| GAP-012 | "Unplanned Visits" interpretation ambiguous. No guidance on whether high unplanned visits is positive (proactive selling) or negative (non-compliance). | OPEN | |
| GAP-013 | Cognitive overload: 7 sections with 8+ KPI cards, 1 funnel, 4 charts, and 4 ranked lists exceeds the "5±2" cognitive load limit for a time-constrained owner. | OPEN | |
| GAP-014 | Section order suboptimal. Revenue Distribution (E) placed after Territory Performance (D). Revenue outcomes are more important than territory breakdown for owner persona. | OPEN | |
| GAP-015 | Missing composite Health Score indicator at page top. No single "Field Health: ⚠️ Needs Attention" signal to orient the owner before reading individual KPIs. | OPEN | |
| GAP-016 | Mental model stages (Plan→Execute→Convert→Sell→Monitor→Improve) do not map cleanly to the 7 proposed sections. Section B (Execution Funnel) spans 4 stages. | OPEN | |
| GAP-017 | Missing Week-over-Week change indicator. Trend Analysis shows absolute values only. Directional change (improving/declining) is more actionable than absolute trend. | OPEN | |
| GAP-018 | Missing "Salesmen with Zero Activity" as a distinct attention signal. Currently buried in `StatusCode=Critical` but requires immediate management intervention. | OPEN | |
| GAP-019 | Trend Analysis only mentions "Last 30 Days." API provides both `Last7Days` and `Last30Days`. 7-day view useful for recent pattern detection. | OPEN | |
| GAP-020 | Territory Performance bar chart assumes multiple data dimensions (Actual Visits, Orders, Revenue, ECR). If only Actual Visits is available, chart is decorative — a ranked list would suffice. | OPEN | |
| GAP-021 | Revenue Distribution "Others" category is unactionable. With 16 salesmen, "Others" may represent 13 salesmen — chart may not reveal meaningful concentration patterns. | OPEN | |
| GAP-022 | GPS Valid Rate is a technical metric with low business relevance for owners. Unclear management action expected from this KPI. | OPEN | |
| GAP-023 | Missing "Customers with No Order" indicator. Identifies unproductive visits that consume salesman time without generating revenue. | OPEN | |
| GAP-024 | Missing "Team Capacity Utilization" indicator (Active Salesmen / Total Salesmen). Shows workforce deployment status. | OPEN | |
| GAP-025 | Missing "Revenue Concentration Risk" indicator (Top 1 salesman as % of total). Key-person dependency metric. | OPEN | |
| GAP-026 | Percentage metrics display false precision (e.g., 19.8% ECR with 106 visits = ±0.9% margin). Creates false confidence in exactness. | OPEN | |
| GAP-027 | GPS Warning Count in Attention Center has no expected management action. What should a manager do with this information? | OPEN | |
| GAP-028 | Missing plan quality indicators. High missed-visit rates may indicate bad plans, not bad execution. No way to distinguish root cause. | OPEN | |
| GAP-029 | Orders → Revenue funnel stage conversion is just average order value. Not a meaningful conversion metric. | OPEN | |
| GAP-030 | "Revenue Distribution" risk: with relatively evenly distributed revenue across 16 salesmen, contribution chart may not reveal meaningful concentration patterns. | OPEN | |

---

## 13. OPEN QUESTION

### Business Questions

| ID | Question | Impact |
|----|----------|--------|
| BQ-1 | Is "Effective Call Rate" intended to measure visit productivity or order conversion? The current definition (customers with orders / total visits) measures order conversion. | Affects funnel stage definition and user mental model. |
| BQ-2 | Should "Unplanned Visits" be treated as a negative (non-compliance) or positive (proactive selling)? | Affects Attention Center signal interpretation. |
| BQ-3 | Is GPS compliance a management concern for owners, or is it a supervisor-level detail? | Determines whether GPS metrics belong in the primary view. |
| BQ-4 | Should the dashboard show data for salesmen without email (`HasEmail=false`)? Currently excluded. | Affects completeness of team view. |

### Technical Questions

| ID | Question | Impact |
|----|----------|--------|
| TQ-1 | Should `WilayahBreakdown` be extended to include Orders, Omzet, and EffectiveCalls? | Determines Territory Performance feasibility. |
| TQ-2 | Should `Rankings` be extended to include a GPS Issues ranking? | Determines Attention Center feasibility. |
| TQ-3 | Should `TeamKpis` include `GpsWarningCount` and `GpsSuspiciousCount`? | Determines Execution Quality implementation approach. |

### Blocking Questions

None identified. All gaps can be addressed through frontend aggregation or minor backend extensions.

---

## 14. Planning Readiness

### Status

```text
READY WITH CONDITIONS
```

### Conditions

1. **BQ-1 must be resolved** before funnel implementation. The funnel stage definitions depend on the intended business semantics.
2. **TQ-1 must be resolved** before Territory Performance implementation. Either extend backend or accept frontend-only aggregation.
3. **Information Architecture must be simplified** per recommendations (merge sections, reduce KPI cards).

### Planner Guidance

**Implementation Scope:**
- Frontend redesign of `FieldActivityOverviewView.vue`
- New components: Funnel, Contribution Chart, Health Score
- Modified components: KPI Strip, Attention Center, Territory Chart
- Backend changes: Optional (extend `WilayahBreakdown`, `Rankings`, `TeamKpis`)

**Major Dependencies:**
- PrimeVue Chart already available (Chart.js wrapper)
- No new chart library required
- Funnel visualization requires custom component (no existing funnel component)

**Sequencing Concerns:**
- Backend extensions (if pursued) must precede frontend work
- Information Architecture decisions must precede component design
- Mobile responsiveness must be considered from the start

**Review Concerns:**
- The funnel visualization needs careful UX review to avoid misleading conversion semantics
- The Attention Center needs validation with actual sales team data to ensure exceptions are meaningful

---

## 15. Implementation Impact Inventory

### Backend

| Component | Change Type | Required |
|-----------|-------------|----------|
| `FieldActivityTeamKpis` | Add `GpsWarningCount`, `GpsSuspiciousCount` | Optional |
| `FieldActivityRankingSection` | Add `MostGpsIssues` | Optional |
| `FieldActivityWilayahBreakdownRow` | Add `OrdersCount`, `OmzetAmount`, `EffectiveCalls` | Optional |
| `FieldActivityOverviewComposer` | Update aggregation for new fields | Optional |
| `DashboardFieldActivitySnapshotDal` | Update snapshot schema | Optional |

### Database

| Object | Change Type | Required |
|--------|-------------|----------|
| `BTRPD_FieldActivityKpi` | Add GPS columns | Optional |
| `BTRPD_FieldActivitySalesman` | No change | N/A |
| `BTRPD_FieldActivityTrend` | No change | N/A |

### Frontend

| Component | Change Type | Required |
|-----------|-------------|----------|
| `FieldActivityOverviewView.vue` | Redesign layout | Required |
| `FieldActivityTeamKpiStrip.vue` | Modify KPI set | Required |
| New: Funnel component | Create | Required |
| New: Contribution chart | Create | Required |
| New: Health Score indicator | Create | Recommended |
| `FieldActivityComparisonChart.vue` | May replace with territory chart | Required |
| `FieldActivityRankingGrid.vue` | Simplify for Attention Center | Required |
| `FieldActivityTeamTrendChart.vue` | Minor layout adjustment | Required |
| `FieldActivityWilayahChart.vue` | Enhance with selectable measures | Required |

### Integration

No external integration changes required.

### Security

No security model changes required.

---

## 16. Anti-Pattern Summary Table

| Anti-Pattern | Location | Issue | Severity |
|--------------|----------|-------|----------|
| Vanity Metric | A. Active Salesmen | Counts eligible, not active | Medium |
| Redundant KPI | A+B Visit Execution % | Same number in 2 places | Medium |
| Redundant KPI | A+B+F+G Effective Call Rate | Same number in 4 places | High |
| Decorative Chart | D. Territory Performance | Bar chart without sufficient data dimensions | Medium |
| Unactionable Info | C. GPS Valid Rate | Owner cannot act on GPS trends | Low |
| Duplicate Info | A+B Actual Visits | Same number in 2 places | Medium |
| Ranking Overload | F. Attention Center | 4 lists + 8 existing rankings = 12 total | High |
| Attention Dilution | Full Page | 7 sections competing for attention | Medium |
| False Precision | B. Funnel 19.8% | Precision exceeds data reliability | Low |
| Missing Action | C. Unplanned Visits | Unclear if positive or negative | Medium |
| Missing Action | C. GPS Warning Count | No expected management response | Low |
| Misleading Stage | B. Funnel "Effective Calls" | Measures order conversion, not visit quality | High |

---

*Assessment completed: 2026-09-14*
*Reviewer: Feasibility Assessment Agent*
*Source: sfo-dashboard-ux-blueprint.md v2.0*
