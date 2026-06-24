# BTR Portal Analysis — M30 Collection Optimization Dashboard

**Status:** Analysis complete — ready for Architect review and Product Owner approval before implementation.  
**Scope:** Business analysis, optimization framework, recommendation model, priority model, dashboard wireframe, KPI definitions, and business rules only. No production code.  
**Date:** 2026-06-22  
**Author role:** Analyst  
**Companion document:** [implementation-plan-m30-collection-optimization.md](./implementation-plan-m30-collection-optimization.md)

**Reference documents:** `docs/foundation/PRODUCT.md`, `docs/foundation/DOMAIN.md`, `docs/foundation/LANDSCAPE.md`, `docs/foundation/WORKFLOW.md`, `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-operational.md`, `docs/features/customer-risk-forecast/feature.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md`, `docs/work/btr-portal/M14-piutang-dashboard-v2-analysis.md`, `docs/work/btr-portal/M17-Customer-Analytics-Analysis.md`, `docs/work/btr-portal/M20-collection-dashboard-analysist.md`, `docs/work/btr-portal/portal-analysis-m27-cash-flow-forecast.md`, `docs/work/btr-portal/portal-analysis-m28-5-inventory-optimization.md`, `docs/work/btr-portal/portal-analysis-m29-customer-risk-forecast.md`

---

## 1. Executive Summary

BTR Portal has evolved through six business maturity levels:

| Level | Question | Example dashboards |
| ----- | -------- | ------------------ |
| Reporting | What happened? | Piutang Report, Pelunasan Report (Desktop) |
| Analytics | How is the business performing? | Piutang, Customer Analytics, Collection dashboards |
| Decision Support | What requires attention today? | Executive Dashboard, Alert Center, Collection Management |
| Forecasting | What will probably happen? | Sales Forecast (M26), Cash Flow Forecast (M27), Inventory Forecast (M28), Customer Risk Forecast (M29) |
| Optimization | Given the forecast, what should management do? | Inventory Optimization (M28.5), **Collection Optimization (M30)** |
| **Collection Optimization** | **Given customer risk forecasts and current receivables, what should Finance and Sales do today to maximize collections while preserving relationships?** | **Collection Optimization (M30)** |

**M14/M20 answer:** *How much is owed today and is recovery working?*

**M17 answer:** *Which customers require attention across sales and receivables today?*

**M29 answers:** *Which customers are likely to become collection risks in the next 30 days — and why?*

**M30 answers:** *Given those risks and today's receivable position, which customers should Finance and Sales contact first, with what action, and what collection impact can we expect?*

### Key findings

| Finding | Implication for M30 |
| ------- | --------------------- |
| M29 already computes risk categories, forecast signals, `RiskPriorityScore`, and primary `RecommendationKey` per customer | M30 **consumes M29 outputs in-memory** — does not recalculate forecast rules |
| M20 owns recovery effectiveness (Recovery vs Billing %), overdue exposure, chronic/legacy/plafond attention signals, and salesman/wilayah overdue rankings | M30 **cross-reads M20 snapshot** for current collection posture and workload context |
| M14 owns aging buckets via `PiutangAgingBucketResolver` and per-customer open balance | M30 reuses **same piutang load** in Customer worker — no duplicate aging SQL |
| M17 owns dormant (90-day), plafond breach, overdue attention signals | M30 uses M17 signals only as **current-state context** — not duplicated in attention list |
| M29 recommendations are forecast-oriented (`CallCustomer`, `ReviewCredit`, `SalesRecovery`) | M30 **operationalizes** into today's action queues with impact-weighted prioritization |
| No collection CRM, promise-to-pay, or visit-outcome data in BTR | Actions are **recommendations only** — users execute in Desktop or field |
| M28.5 established the optimization pattern: extend snapshot worker, pass in-memory forecast context, cross-read related snapshot | M30 follows **same architectural pattern** on Customer domain after M29 step |
| Collector is not a separate BTR entity — collection follows invoicing **Salesman** | Workload dimensions: Salesman, Wilayah, Customer Group (`Klasifikasi`) |

### Product intent

At the start of each business day, Finance and Collection management should answer:

> **Given customer risk forecasts and today's receivable position, what should Finance and Sales focus on today to maximize collections while preserving valuable customer relationships?**

**Explicit constraints (non-negotiable):**

- Read-only — no automatic collection actions, no Desktop write-back
- Deterministic — every recommendation reproducible from business formulas
- Explainable — every row includes human-readable reason text, priority rationale, and rule traceability
- No AI, ML, optimization solvers, or probability scores
- Consumes M29 — does not regenerate customer risk forecasts
- Traceable to Piutang (M14), Customer Analytics (M17), Collection (M20), Cash Flow Forecast (M27), and Customer Risk Forecast (M29)

---

## 2. Business Objectives

### 2.1 Current business pain

Management today must synthesize multiple dashboards manually:

| Dashboard | What it shows | What it cannot show |
| --------- | ------------- | ------------------- |
| Piutang (M14) | Who owes money and aging today | Prioritized **today's action plan** |
| Customer Analytics (M17) | Current attention signals | **Operational sequencing** — who first |
| Collection (M20) | Recovery pace and overdue workload | **Per-customer action** with forecast context |
| Customer Risk Forecast (M29) | Forward risk and forecast recommendations | **Today's collection operations workspace** |
| Cash Flow Forecast (M27) | Company month-end cash | Actionable customer queue |

Finance managers open Piutang Report, Collection Dashboard, and Customer Risk Forecast separately. They mentally rank customers by overdue amount, due date, and relationship importance. Sales managers lack a unified view of when **collection** vs **sales recovery** should lead. This synthesis is inconsistent, not scalable across hundreds of customers, and does not quantify **expected collection impact**.

### 2.2 Decision improvements

| Decision | Enabled by M30 |
| -------- | -------------- |
| Which customers to contact first today | Unified **Collection Priority Queue** ranked by deterministic score |
| Which overdue accounts need immediate escalation | **Immediate Collection** action category |
| Which current customers need proactive reminder before overdue | **Proactive Reminder** queue (due soon + forecast risk) |
| Which accounts need credit review before more sales | **Credit Review Queue** |
| When declining purchases should be handled by Sales, not Collection | **Sales Recovery Queue** |
| How collection workload distributes across territory | **Workload by Salesman / Wilayah** |
| Where successful collection produces greatest cash impact | **High Impact Opportunities** ranked by impact amount |
| What Finance and Sales should focus on this morning | **Executive Summary** — plain-language daily brief |

### 2.3 Business value

| User | Value |
| ---- | ----- |
| Owner / GM | Daily prioritized action brief — cash at risk and top opportunities |
| Finance administration | Operational collection workspace replacing manual list building |
| Collection management | Explainable priority queue with forecast + current-state rationale |
| Sales management | Clear handoff — when Sales owns the visit vs when Collection leads |
| Credit control | Credit review queue separated from collection follow-up |

### 2.4 Expected outcomes

1. **Faster collection response** — high-impact overdue accounts surfaced first each morning.
2. **Reduced overdue growth** — proactive reminders before due date for at-risk payers.
3. **Preserved relationships** — sales recovery routed to Sales when purchase decline dominates over collection urgency.
4. **Balanced workload** — territory and salesman workload visible for daily planning.
5. **Foundation for M31/M32** — operational decision support layer for Customer Portfolio Optimization and Executive Business Health.

### 2.5 Explicitly out of scope (V1)

- AI / ML / probability scoring
- Automatic collection calls, emails, or Desktop write-back
- Promise-to-pay tracking or collection CRM
- Collector route planning or GPS optimization
- Collection capacity planning solvers
- Scenario simulation / what-if (extension hooks only)
- Alert Center / Executive Dashboard integration
- Changes to M29 forecast rules or API response shape
- Per-Faktur collection queue (customer grain only)
- DSO headline KPI (deferred per M20/M29)

---

## 3. Terminology Mapping

Business terminology must map to BTR Desktop and portal implementation terms. **Do not introduce parallel vocabulary.**

| Business term | BTR / Portal term | Source |
| ------------- | ----------------- | ------ |
| Collection | **Pelunasan** (payment recording), collection management (operational) | `BTR_PiutangLunas`, M20 Collection Dashboard |
| Receivable / Outstanding | **Piutang**, `KurangBayar`, `Sisa` | `BTR_Piutang` |
| Sales Invoice | **Faktur** | `BTR_Faktur`, `FakturView` |
| Due date | **Jatuh Tempo**, `DueDate` | `BTR_Piutang` |
| Payment | **Pelunasan**, **Lunas** | `BTR_PiutangLunas` |
| Credit limit | **Plafond** | `CustomerModel.Plafond` |
| Overdue | Non-`Current` aging bucket | `PiutangAgingBucketResolver` |
| Reminder | Proactive collection contact before overdue — **not a BTR entity** | M30 operational recommendation only |
| Aging | 5-bucket model: Current, Days1To30, Days31To60, Days61To90, DaysOver90 | M14 |
| Customer | `CustomerCode`, `CustomerName`, `CustomerId` | `BTR_Customer` |
| Salesman / Collector | **Sales Person** — no separate Collector entity | `BTR_Faktur.SalesPersonId` |
| Territory | **Wilayah** | Customer / Faktur |
| Customer group | **Klasifikasi** | `CustomerModel.Klasifikasi` |
| Risk category | M29 five-band: Healthy, Watch, Attention, High Risk, Critical | `CustomerRiskForecastPolicy` |
| Forecast recommendation | M29 `RecommendationKey` | `CustomerRiskRecommendationBuilder` |
| Optimization action | M30 `ActionCategoryKey` | **New** — operational action for today |

**M30 canonical customer key:** `DashboardCustomerKeyResolver.ResolveCodeFirst(CustomerCode, CustomerName)` — consistent with M17/M20/M29.

---

## 4. Existing Capability Analysis

### 4.1 Piutang Dashboard (M14 V2)

**Source:** `DashboardPiutangAggregator` → `BTRPD_PiutangKpi`, `BTRPD_PiutangAging`, `BTRPD_PiutangCustomerAging`, `BTRPD_PiutangTopCustomerRisk`

| Capability | Rule | M30 reuse |
| ---------- | ---- | --------- |
| Open balance filter | `KurangBayar > 1` | Same |
| Aging buckets | `PiutangAgingBucketResolver` | **Authoritative** — exposure and due urgency inputs |
| Per-customer aging | By `CustomerId` in snapshot | Recomputed in-memory from same DTOs in Customer worker |
| Top 20 customer risk | Rank by total open balance | **Context** — M30 ranks by optimization priority, not balance alone |
| Overdue customer count | Distinct customers with non-Current exposure | Workload KPI denominator |

**M30 does not duplicate Piutang Dashboard.** Piutang answers *current exposure*; M30 answers *today's prioritized collection actions*.

### 4.2 Customer Analytics Dashboard (M17)

**Source:** `DashboardCustomerAggregator` → `BTRPD_CustomerKpi`, `BTRPD_CustomerAttention`, `BTRPD_CustomerTopOmzet`, `BTRPD_CustomerTopPiutang`

| Capability | Rule | M30 reuse |
| ---------- | ---- | --------- |
| Active customer | Invoiced in current month | Strategic importance input |
| Dormant | 90-day rule | Legacy debt context — M20 signal alignment |
| Plafond breach | `Plafond > 0 AND openBalance > Plafond` | Credit review queue input |
| Attention signals | Overdue, Dormant, PlafondBreach, SuspendedWithSales | **Current-state flags** on optimization rows — not duplicated as separate attention list |
| Top 10 Omzet | Current month ranking | **Strategic customer boost** on priority score |
| Segmentation | Klasifikasi, Wilayah | Workload breakdown dimensions |

### 4.3 Collection Dashboard (M20)

**Source:** `DashboardCollectionAggregator` → `BTRPD_CollectionKpi`, `BTRPD_CollectionAttention`, `BTRPD_CollectionTopOverdue*`

| Capability | Rule | M30 reuse |
| ---------- | ---- | --------- |
| Overdue exposure | Sum non-Current balances | Impact denominator; immediate collection triggers |
| Recovery vs Billing % | `MonthCollections ÷ MonthFakturOmzet × 100` | Company context KPI |
| Chronic overdue signal | `DaysOver90` exposure | Immediate collection escalation |
| Legacy debt | Dormant + open balance | Relationship-preserve routing |
| Plafond breach + overdue | M20 `SignalPlafondBreachOverdue` | Credit + collection combined urgency |
| Low recovery salesman | Rep collections < rep omzet | Workload context; salesman queue weighting |
| Wilayah hotspot | Overdue share ≥ 15% | Territory workload elevation |
| Top overdue customers/salesmen/wilayah | Rank by **overdue balance** | Cross-check workload tables — M30 adds **action count** per entity |

**M20 remains authoritative for recovery effectiveness.** M30 cross-reads M20 snapshot at Customer refresh for recovery context (COL-OPT-31).

### 4.4 Cash Flow Forecast Dashboard (M27)

**Source:** `CashFlowCollectionRiskBuilder` → `BTRPD_CashFlowCollectionRisk`

| Capability | Rule | M30 reuse |
| ---------- | ---- | --------- |
| Large due soon (7d) | Due within 7 days + amount floor | Proactive reminder and priority due urgency |
| Plafond breach due soon (14d) | M27 threshold | Credit review + collection combo |
| Chronic overdue large | DaysOver90 + floor | Immediate collection |
| Low recovery customer | Salesman in low recovery set + customer overdue | Priority boost |
| Due concentration 15% | Customer share of company due-within-horizon | High impact opportunity |

**M27 remains company-level liquidity forecast.** M30 applies same threshold constants at customer operational grain where M29 already consumed them.

### 4.5 Customer Risk Forecast Dashboard (M29)

**Source:** `DashboardCustomerRiskForecastAggregator` → `BTRPD_CustomerRiskForecast*`

| Capability | Rule | M30 reuse |
| ---------- | ---- | --------- |
| Risk category | Five-band from signal severity | **Primary input** — category weight in priority score |
| Forecast signals | Payment delay, credit, inactivity, decline, collection | Traceability on optimization rows — not re-evaluated |
| `RiskPriorityScore` | M29 ranking within forecast | **Seed sort** — M30 recomputes `CollectionPriorityScore` for operational ranking |
| `RecommendationKey` | M29 primary recommendation | **Maps to M30 action category** — refined with current-state |
| Top 20 customer risk rows | Materialized cap | M30 expands eligible set — all customers with action ≠ NoActionToday materialized up to cap |
| Executive summary | Forecast-oriented text | M30 generates **operations-oriented** summary |

**M29 is the authoritative forecast layer.** M30 is the authoritative **operational optimization layer** — analogous to M28 → M28.5.

### 4.6 Salesman Performance (M18) — read-only context

| Signal | M30 use |
| ------ | ------- |
| High overdue exposure salesman | Workload queue — action count per salesman |
| Top piutang concentration by rep | Context for territory planning |

### 4.7 Reusable calculations — do not duplicate

| Calculation | Reuse from |
| ----------- | ---------- |
| Aging bucket assignment | `PiutangAgingBucketResolver` |
| Open balance filter | `KurangBayar > 1` |
| M29 risk category and signals | In-memory `CustomerRiskForecastContext` from M29 aggregator |
| M29 recommendation key | `CustomerRiskRecommendationBuilder` output |
| Dormant detection (90-day) | M17 / M20 `DormantDaysThreshold` |
| Plafond breach (current) | M17 rule |
| Collection attention signals | M20 `DashboardCollectionAggregator` constants |
| Recovery vs billing | M20 collection KPI |
| Due-soon / chronic thresholds | M27 `CashFlowCollectionRiskBuilder` constants |
| Customer key resolution | `DashboardCustomerKeyResolver` |
| Top omzet strategic rank | M17 top omzet computation (in-memory from Faktur load) |
| Business date | `IBusinessDateProvider.Today` |

### 4.8 Business rule ownership

| Rule domain | Authoritative owner | M30 relationship |
| ----------- | ------------------- | ------------------ |
| Aging buckets | M14 | Consumer |
| Current customer attention | M17 | Context input |
| Recovery effectiveness | M20 | Cross-read snapshot |
| Company collection risk thresholds | M27 | Threshold reference |
| Customer risk forecast | M29 | **Primary input — in-memory** |
| Collection optimization actions | **M30 (new)** | `CollectionOptimizationPolicy` |

---

## 5. Collection Optimization Opportunities

### 5.1 Collection Priority Queue

**Business question:** Which customers should be contacted first today?

**Eligible customers:** Any customer with open balance > 0 OR M29 category ∈ {Watch, Attention, High Risk, Critical} OR M29 recommendation ≠ `NoAction`.

**Priority factors (deterministic integer score — see §8):**

| Factor | Source | Role |
| ------ | ------ | ---- |
| M29 risk category | M29 `Category` | Primary urgency band |
| Overdue balance | M14 piutang metrics | Current collection pressure |
| Due within 7 / 14 days | Piutang `JatuhTempo` | Time urgency |
| Payment behavior | M29 signals (lag, no recent payment) | Follow-up intensity |
| Customer importance | Top 10 MTD omzet flag, prior month omzet floor | Strategic boost |
| Sales trend | M29 decline signals | Route to Sales when dominant |
| M20 chronic / legacy / plafond signals | M20 cross-read | Escalation boost |

**Output:** Unified **Collection Priority Queue** — top 30 customers by `CollectionPriorityScore` with action category, impact amount, and explainability fields.

### 5.2 Early Collection Planning (Proactive Reminder Queue)

**Business question:** Which customers should receive proactive reminders before becoming overdue?

| Condition | Rule |
| --------- | ---- |
| **Proactive Reminder** | All balances Current AND `MinDaysUntilDue` in [1, 14] AND M29 category ∈ {Watch, Attention, High Risk, Critical} |
| **Early Collection Planning** | `MinDaysUntilDue` in [1, 7] AND (M29 `LikelyLatePayer` OR `DueSoonSlowPayer` OR `AvgPaymentLagDays ≥ 7`) |
| **Due Soon — Large Exposure** | Due within 7 days AND open balance ≥ configurable large-due floor (reuse M27 floor pattern) |

**Owner:** Finance / Collection. **Not** overdue yet — relationship-preserving contact.

**Plain language:** *"Contact before due date — customer shows slow payment pattern."*

### 5.3 Credit Review Queue

**Business question:** Which customers require credit review before additional sales?

| Condition | Rule | M29 alignment |
| --------- | ---- | ------------- |
| **Suspend Credit Review** | M29 recommendation `SuspendCreditReview` | CRF-REC-02 |
| **Review Credit** | M29 recommendation `ReviewCredit` OR current plafond breach | CRF-REC-04 |
| **Credit Hold Discussion** | Projected plafond breach + MTD omzet > 0 + overdue > 0 | Combined urgency |

**Owner:** Finance / Credit control. Separated from collection call queue — may block new Faktur discussion.

### 5.4 Sales Recovery Queue

**Business question:** When should declining purchases be handled by Sales instead of Collection?

| Condition | Rule |
| --------- | ---- |
| **Sales Recovery Visit** | M29 recommendation ∈ {`SalesRecovery`, `ScheduleVisit`} AND overdue balance < configurable floor OR overdue = 0 |
| **Relationship Recovery** | M29 decline signal (moderate/severe) AND NOT immediate collection eligible |
| **Purchase Recovery Priority** | Severe decline + prior month omzet ≥ floor + open balance Current only |

**Routing rule (COL-OPT-R01):** When `SalesRecovery` queue conditions met AND `ImmediateCollection` conditions not met → action owner = **Sales**. Collection defers to preserve relationship.

**Plain language:** *"Revenue declining — sales visit recommended before collection pressure."*

### 5.5 Territory Workload

**Business question:** How is today's collection workload distributed?

| Dimension | Aggregation | Metrics |
| --------- | ----------- | ------- |
| **Salesman** | Group priority queue by `SalesPersonName` | Action count, immediate count, total impact amount, overdue exposure |
| **Wilayah** | Group by `WilayahName` | Same + M20 hotspot flag |
| **Klasifikasi** | Group by customer classification | Action count, elevated-risk count |

**Output:** Top 10 workload rows per dimension — bar chart + table. Not route optimization — informational workload planning only.

### 5.6 High Impact Opportunities

**Business question:** Which customers produce the greatest collection impact if action succeeds today?

**Collection Impact Amount (V1):**

```
CollectionImpactAmount = OverdueBalance + DueWithin7Days
```

| Opportunity type | Rule |
| ---------------- | ---- |
| **Top Impact — Overdue** | Rank by `CollectionImpactAmount` where action ∈ {ImmediateCollection, PriorityFollowUp} |
| **Top Impact — Due Soon** | Rank by `DueWithin7Days` where action = ProactiveReminder |
| **Concentration Opportunity** | Customer impact ≥ 10% of company total impact amount |

**Expected impact label (plain language):** *"Successful collection could recover approximately Rp X (overdue + due this week)."*

Not a probability — deterministic exposure amount only.

---

## 6. Collection Action Categories

Categories derived from business analysis. Every customer receives exactly one **primary action category** for today (highest-precedence match).

| ActionCategoryKey | Label | Business meaning | Typical owner | Precedence |
| ----------------- | ----- | ---------------- | ------------- | ---------- |
| `ImmediateCollection` | Immediate Collection | Overdue with high severity — contact today | Finance / Collection | 1 |
| `EscalateManagement` | Escalate to Management | Critical risk + material exposure — leadership review | Management | 2 |
| `PriorityFollowUp` | Priority Follow-up | Overdue or due ≤7d with elevated forecast risk | Collection | 3 |
| `ProactiveReminder` | Send Reminder | Current balance, due soon, forecast risk — contact before overdue | Collection | 4 |
| `CreditReview` | Credit Review | Plafond or projected breach — review before more sales | Finance | 5 |
| `SalesRecoveryVisit` | Schedule Sales Visit | Decline/inactivity — Sales leads, not Collection | Sales | 6 |
| `LegacyDebtReview` | Legacy Debt Review | Approaching dormant + balance — specialized handling | Finance / Collection | 7 |
| `RelationshipMonitor` | Continue Monitoring | Watch + strategic customer — light touch | Sales / Collection | 8 |
| `DeferCollection` | Safe to Wait | Healthy/Watch, no overdue, due >14d | — | 9 |
| `NoActionToday` | No Action Today | Healthy, no balance, no actionable signals | — | 10 |

### 6.1 Category assignment rules (COL-OPT-CAT)

| Rule ID | Condition → Category |
| ------- | --------------------- |
| COL-OPT-CAT-01 | `OverdueBalance > 0` AND (`Category ∈ {High Risk, Critical}` OR chronic overdue OR `DaysOver90 > 0`) → **ImmediateCollection** |
| COL-OPT-CAT-02 | `Category = Critical` OR M29 `ManagementReview` → **EscalateManagement** (unless COL-OPT-CAT-01 already applied — ImmediateCollection wins for operational queue) |
| COL-OPT-CAT-03 | `OverdueBalance > 0` AND `Category ∈ {Attention, High Risk}` → **PriorityFollowUp** |
| COL-OPT-CAT-04 | `OverdueBalance = 0` AND `MinDaysUntilDue` in [1,14] AND `Category ∈ {Watch, Attention, High Risk, Critical}` → **ProactiveReminder** |
| COL-OPT-CAT-05 | M29 recommendation ∈ {`ReviewCredit`, `SuspendCreditReview`} → **CreditReview** |
| COL-OPT-CAT-06 | M29 recommendation ∈ {`SalesRecovery`, `ScheduleVisit`} AND NOT (COL-OPT-CAT-01 OR COL-OPT-CAT-03) → **SalesRecoveryVisit** |
| COL-OPT-CAT-07 | M29 recommendation = `LegacyDebtReview` → **LegacyDebtReview** |
| COL-OPT-CAT-08 | `Category = Watch` AND Top 10 MTD omzet flag → **RelationshipMonitor** |
| COL-OPT-CAT-09 | `Category = Healthy` AND `OverdueBalance = 0` AND (`MinDaysUntilDue > 14` OR no open balance) → **DeferCollection** |
| COL-OPT-CAT-10 | Default → **NoActionToday** |

**Note:** COL-OPT-CAT-02 `EscalateManagement` appears in executive summary and management queue; operational primary action for critical+overdue customers is **ImmediateCollection** (COL-OPT-CAT-01 takes precedence in priority queue).

### 6.2 Recommended action detail (sub-actions)

Each category maps to one **RecommendedActionKey** for traceability:

| ActionCategoryKey | RecommendedActionKey | Label |
| ----------------- | -------------------- | ----- |
| ImmediateCollection | `CallCustomer` | Call Customer |
| EscalateManagement | `EscalateManagement` | Escalate to Management |
| PriorityFollowUp | `CallCustomer` | Call Customer |
| ProactiveReminder | `SendReminder` | Send Reminder |
| CreditReview | `ReviewCredit` | Review Credit |
| SalesRecoveryVisit | `ScheduleVisit` | Schedule Visit |
| LegacyDebtReview | `LegacyDebtReview` | Legacy Debt Review |
| RelationshipMonitor | `IncreaseMonitoring` | Increase Monitoring |
| DeferCollection | `DelayCollection` | Delay Collection |
| NoActionToday | `NoAction` | No Action Today |

---

## 7. Dashboard Wireframe

**Route:** `/dashboard/collection-optimization`  
**Layout:** Action-first (mirrors Inventory Optimization and Customer Risk Forecast)

```text
┌─────────────────────────────────────────────────────────────────────────────┐
│ Collection Optimization Dashboard                   [Refresh] [Generated at] │
│ Today's prioritized collection actions from risk forecast and receivables    │
├─────────────────────────────────────────────────────────────────────────────┤
│ EXECUTIVE SUMMARY (plain language, server-composed)                         │
│ "Today's Collection Priorities: Contact 18 high-risk customers. Review      │
│  credit for 6. Schedule 12 sales recovery visits. Immediate collection      │
│  focus represents Rp 1.8B outstanding."                                     │
├─────────────────────────────────────────────────────────────────────────────┤
│ KPI ROW 1 — Today's workload                                                │
│ [Actions Today] [Immediate Collection] [Proactive Reminders] [Credit Review]│
│ [Sales Recovery] [Collection Impact Total]                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│ KPI ROW 2 — Context                                                         │
│ [Overdue Exposure] [Due Within 7 Days] [Recovery vs Billing %] [Confidence] │
├─────────────────────────────────────────────────────────────────────────────┤
│ CHARTS ROW                                                                  │
│ ┌──────────────────────────┐  ┌──────────────────────────┐                │
│ │ Actions by Category      │  │ Workload by Wilayah      │                │
│ │ (donut)                  │  │ (bar — action count)     │                │
│ └──────────────────────────┘  └──────────────────────────┘                │
│ ┌──────────────────────────┐  ┌──────────────────────────┐                │
│ │ Impact by Action Category│  │ Workload by Salesman     │                │
│ │ (bar — Rp impact)        │  │ (bar — top 10)           │                │
│ └──────────────────────────┘  └──────────────────────────┘                │
├─────────────────────────────────────────────────────────────────────────────┤
│ TODAY'S COLLECTION PRIORITIES (table, top 30 by CollectionPriorityScore)  │
│ Customer | Action | Priority | Impact | Overdue | Due | Risk | Reason     │
├─────────────────────────────────────────────────────────────────────────────┤
│ SPECIALIZED QUEUES (tabbed or accordion)                                    │
│ [Proactive Reminders] [Credit Review] [Sales Recovery] [Management Esc.]  │
├─────────────────────────────────────────────────────────────────────────────┤
│ TOP IMPACT OPPORTUNITIES (table, top 15 by CollectionImpactAmount)          │
│ Customer | Impact | Action | Overdue | Due 7d | Salesman | Wilayah         │
├─────────────────────────────────────────────────────────────────────────────┤
│ RECOMMENDATION DETAIL PANEL (expand row)                                    │
│ Why selected | Why priority | Why action | Rules triggered | Drill-down     │
├─────────────────────────────────────────────────────────────────────────────┤
│ TRACEABILITY FOOTER                                                         │
│ Links: Customer Risk Forecast | Collection | Customer Analytics | Piutang │
│ Reports: Piutang Report | Sales Report                                      │
│ Disclaimer: Recommendations only — execute actions in BTR Desktop           │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 7.1 Navigation placement

Sidebar: after **Customer Risk Forecast**, before **Salesman Performance** (or grouped under Finance section after Collection).

Investigation path:

```text
Collection Optimization → Customer Risk Forecast (why forecasted) → Collection (recovery context) → Piutang Report (evidence) → BTR Desktop
```

### 7.2 Drill-down navigation

| From | To | Purpose |
| ---- | -- | ------- |
| Customer row | `/dashboard/customer-risk-forecast` | Forecast signals |
| Customer row | `/dashboard/customers` | Current-state attention |
| Customer row | `/reports/piutang?customer={code}` | Faktur-level evidence |
| Salesman workload | `/dashboard/salesmen` | Rep performance context |
| Wilayah workload | `/dashboard/collection` | Regional recovery |

---

## 8. Recommendation Engine

### 8.1 Design principles

1. **Consume M29** — category, signals, `RecommendationKey`, `RiskPriorityScore` passed in-memory.
2. **Enrich with current state** — overdue, due urgency, M20 recovery context from cross-read.
3. **Assign action category** — COL-OPT-CAT rules (§6.1).
4. **Compute operational priority** — `CollectionPriorityScore` (§8.2).
5. **Explain** — `ReasonText`, `PriorityReasonText`, `ActionReasonText`, `TriggeredRuleIds`.

### 8.2 Collection Priority Score

Integer sort key — **not probability**:

```
CollectionPriorityScore = ActionCategoryWeight + RiskCategoryWeight + ImpactComponent + DueUrgencyComponent + StrategicBoost + RecoveryUrgencyBoost
```

| Component | Formula |
| --------- | ------- |
| ActionCategoryWeight | ImmediateCollection=1000, EscalateManagement=950, PriorityFollowUp=800, ProactiveReminder=600, CreditReview=550, SalesRecoveryVisit=500, LegacyDebtReview=450, RelationshipMonitor=300, DeferCollection=100, NoActionToday=0 |
| RiskCategoryWeight | Critical=400, High Risk=320, Attention=240, Watch=160, Healthy=0 |
| ImpactComponent | `MIN(350, FLOOR(CollectionImpactAmount / 1_000_000) × 8)` |
| DueUrgencyComponent | 200 if any balance due ≤ 3d; 150 if ≤ 7d; 80 if ≤ 14d; 0 otherwise |
| StrategicBoost | 120 if Top 10 MTD omzet; 60 if Top 20 MTD omzet |
| RecoveryUrgencyBoost | 80 if M20 chronic signal on customer; 60 if plafond breach + overdue; 40 if legacy debt |

**Tie-break:** `CollectionImpactAmount` descending → customer name ascending.

### 8.3 Recommendation rules (COL-OPT-REC)

| Rule ID | Rule |
| ------- | ---- |
| COL-OPT-REC-01 | Every customer with `NoActionToday` excluded from priority queue tables (included in KPI counts only) |
| COL-OPT-REC-02 | `ReasonText` must cite primary trigger: overdue amount, due date, or M29 signal label |
| COL-OPT-REC-03 | `PriorityReasonText` cites top 2 score components in plain language |
| COL-OPT-REC-04 | `ActionReasonText` cites action category rule ID |
| COL-OPT-REC-05 | `TriggeredRuleIds` = comma-separated M29 rule IDs + COL-OPT-CAT rule ID |
| COL-OPT-REC-06 | Sales recovery routing: when COL-OPT-CAT-06 applies, `ActionOwner = Sales` else `ActionOwner = Collection` (CreditReview → Finance, EscalateManagement → Management) |
| COL-OPT-REC-07 | Max materialized rows: 30 priority, 15 per specialized queue, 15 impact opportunities, 10 per workload dimension |

### 8.4 Scope rules (COL-OPT-XX)

| Rule ID | Rule |
| ------- | ---- |
| COL-OPT-01 | Business date from `IBusinessDateProvider.Today` |
| COL-OPT-02 | Open balance rows: `KurangBayar > 1` |
| COL-OPT-03 | Customer grain: `DashboardCustomerKeyResolver` code-first |
| COL-OPT-04 | No AI/ML/solvers — integer scores and threshold rules only |
| COL-OPT-05 | Read-only — no Desktop write-back |
| COL-OPT-06 | Does not recalculate M29 forecast signals — consumes M29 context |
| COL-OPT-07 | Cross-read M20 collection snapshot for recovery KPIs and attention keys |
| COL-OPT-08 | Large due floor reuses M27 `largeDueSoonFloorAmount` from config |
| COL-OPT-09 | Sales recovery overdue floor default Rp 500,000 — below this, route to Sales when decline dominant |
| COL-OPT-10 | UangMuka excluded from pelunasan (FF2 parity) — inherited from M29 load |

---

## 9. Prioritization Rules

### 9.1 Input summary

| Input | Source | Used for |
| ----- | ------ | -------- |
| Customer Risk Category | M29 | RiskCategoryWeight |
| Outstanding Balance | Piutang load | Impact, eligibility |
| Overdue Balance | Piutang aging | Immediate collection, impact |
| Days Overdue / Min Days Until Due | Piutang `JatuhTempo` | DueUrgencyComponent |
| Due Within 7 Days amount | Piutang aggregation | Impact, proactive queue |
| Credit Utilization | `OpenBalance / Plafond` when plafond > 0 | Credit review context column |
| Payment Lag | M29 `AvgPaymentLagDays` | Proactive reminder boost |
| Purchase Trend | M29 decline signals | Sales recovery routing |
| Customer Classification | `Klasifikasi` | Workload dimension |
| Sales Importance | Top 10/20 MTD omzet | StrategicBoost |
| Strategic Customer | Top 10 MTD omzet OR prior month omzet ≥ Rp 5M | Relationship monitor, defer penalty avoidance |
| M20 signals | Cross-read | RecoveryUrgencyBoost |
| M29 RecommendationKey | M29 | Action category mapping |

### 9.2 Queue inclusion rules

| Queue | Inclusion |
| ----- | --------- |
| Collection Priority | `ActionCategoryKey` ∉ {DeferCollection, NoActionToday} |
| Proactive Reminder | `ProactiveReminder` |
| Credit Review | `CreditReview` |
| Sales Recovery | `SalesRecoveryVisit` |
| Management Escalation | `EscalateManagement` |
| High Impact | `CollectionImpactAmount > 0` AND action ∉ {DeferCollection, NoActionToday} |

### 9.3 Defer and no-action rules

| Rule ID | Rule |
| ------- | ---- |
| COL-OPT-DEF-01 | Healthy + all Current + due > 14d → DeferCollection |
| COL-OPT-DEF-02 | Healthy + zero open balance + no M29 signals → NoActionToday |
| COL-OPT-DEF-03 | Strategic customer (Top 10 omzet) never assigned NoActionToday if any Watch signal — minimum RelationshipMonitor |

---

## 10. Executive Summary Generator

Server-composed plain-language block at top of dashboard.

### 10.1 Template

```
Collection Optimization (as of {BusinessDate}):

Today's Collection Priorities:
• Contact {ImmediateCount + PriorityFollowUpCount} customers for collection follow-up ({ImmediateCount} immediate).
• Send proactive reminders to {ProactiveReminderCount} customers before due date.
• Review credit for {CreditReviewCount} customers before additional sales.
• Schedule sales recovery visits for {SalesRecoveryCount} declining accounts.
• Escalate {EscalateCount} accounts to management review.
• Immediate and priority collection focus represents approximately Rp {ImmediateImpactTotal} in overdue and due-this-week exposure.

Top priority: {TopCustomerName} — {TopActionLabel} (Rp {TopImpactAmount})
Recovery context: {RecoveryVsBillingPercent}% recovery vs billing MTD ({RecoveryContextLabel})
Planning confidence: {Confidence} (based on {DaysElapsed} days elapsed in month)
```

### 10.2 Confidence

Reuse M29/M27 pattern on days elapsed in month:

| Confidence | Condition |
| ---------- | --------- |
| Low | Days elapsed ≤ 5 |
| Medium | Days elapsed 6–20 |
| High | Days elapsed ≥ 21 |

Early month: append *"Early-month plan — customer actions may shift as billing and payments progress."*

---

## 11. KPI Definitions

All monetary values IDR. Traceability IDs use prefix **COL-OPT-KPI-**.

### 11.1 Workload KPIs

| KPI ID | Name | Formula | Meaning | Interpretation | Data source | Refresh |
| ------ | ---- | ------- | ------- | -------------- | ----------- | ------- |
| COL-OPT-KPI-01 | Actions Today | Count customers where `ActionCategoryKey` ∉ {DeferCollection, NoActionToday} | Daily operational workload | Higher = busier collection day | M30 aggregator | Customer worker ~30 min |
| COL-OPT-KPI-02 | Immediate Collection Count | Count `ImmediateCollection` | Accounts needing same-day contact | Urgent queue size | M30 | Same |
| COL-OPT-KPI-03 | Proactive Reminder Count | Count `ProactiveReminder` | Pre-overdue contacts | Preventive workload | M30 | Same |
| COL-OPT-KPI-04 | Credit Review Count | Count `CreditReview` | Credit decisions pending | Credit control workload | M30 | Same |
| COL-OPT-KPI-05 | Sales Recovery Count | Count `SalesRecoveryVisit` | Sales-led interventions | Revenue retention workload | M30 | Same |
| COL-OPT-KPI-06 | Management Escalation Count | Count `EscalateManagement` | Leadership review queue | Escalation breadth | M30 | Same |
| COL-OPT-KPI-07 | Collection Impact Total | `SUM(CollectionImpactAmount)` for actions ∉ {Defer, NoAction} | Exposure addressable today | Cash opportunity | Piutang + M30 | Same |

### 11.2 Context KPIs

| KPI ID | Name | Formula | Meaning | Traceability |
| ------ | ---- | ------- | ------- | ------------ |
| COL-OPT-KPI-10 | Overdue Exposure | `SUM(OverdueBalance)` eligible customers | Current past-due total | Must match M20 `OverdueExposure` same cycle (COL-OPT-KPI-50) |
| COL-OPT-KPI-11 | Due Within 7 Days | `SUM(DueWithin7Days)` | Near-term cash expectation | Piutang due dates |
| COL-OPT-KPI-12 | Recovery vs Billing % | M20 `RecoveryVsBillingPercent` | Collection effectiveness context | M20 cross-read (COL-OPT-KPI-51) |
| COL-OPT-KPI-13 | Immediate Impact Total | `SUM(CollectionImpactAmount)` where action ∈ {Immediate, Priority} | Focus exposure for collection team | M30 |
| COL-OPT-KPI-14 | Planning Confidence | M29 confidence pattern | Trust in pace-based routing | Days elapsed |
| COL-OPT-KPI-15 | Defer / No Action Count | Count Defer + NoAction | Accounts safely waiting | M30 |

### 11.3 Distribution KPIs

| KPI ID | Name | Formula |
| ------ | ---- | ------- |
| COL-OPT-KPI-20 | Actions by Category | Count per `ActionCategoryKey` (chart) |
| COL-OPT-KPI-21 | Impact by Category | `SUM(CollectionImpactAmount)` per category |
| COL-OPT-KPI-22 | Workload by Wilayah | Action count per wilayah (top 10) |
| COL-OPT-KPI-23 | Workload by Salesman | Action count per salesman (top 10) |

### 11.4 Traceability rules

| Rule ID | Rule |
| ------- | ---- |
| COL-OPT-KPI-50 | COL-OPT-KPI-10 must equal M20 `OverdueExposure` when M20 snapshot `GeneratedAt` within same refresh batch tolerance (document cross-read staleness ≤ 30 min) |
| COL-OPT-KPI-51 | COL-OPT-KPI-12 copied from M20 KPI — not recomputed |
| COL-OPT-KPI-52 | Sum of queue counts ≤ COL-OPT-KPI-01 (customer may appear in priority + specialized queue — document as acceptable overlap for V1 OR dedupe: priority queue is superset, specialized queues are filters) |
| COL-OPT-KPI-53 | Top priority customer must match rank-1 row in priority table |

**V1 overlap policy:** Specialized queues are **filters** of the full eligible set — same customer may appear in Priority Queue and Credit Review Queue. KPI counts for specialized queues count unique customers per queue.

---

## 12. Recommendation Explainability

Every materialized row must answer:

### 12.1 Why was this customer selected?

`SelectionReasonText` — e.g. *"Overdue Rp 45M with High Risk forecast category and chronic overdue exposure."*

Built from: overdue flag, M29 category, primary M29 signal label, open balance.

### 12.2 Why this priority?

`PriorityReasonText` — e.g. *"Ranked high due to immediate collection category, Rp 45M impact, and balance due in 3 days."*

Built from: top 2 `CollectionPriorityScore` components.

### 12.3 Why this recommended action?

`ActionReasonText` — e.g. *"Immediate collection assigned: overdue balance with chronic aging and Critical risk forecast."*

Built from: COL-OPT-CAT rule citation + M29 recommendation alignment note when different.

### 12.4 Which rules triggered?

| Field | Content |
| ----- | ------- |
| `TriggeredRuleIds` | e.g. `CRF-L03,COL-OPT-CAT-01,COL-OPT-REC-02` |
| `M29Category` | Risk category from forecast |
| `M29RecommendationKey` | Forecast recommendation for traceability |
| `M29PrimarySignalKey` | Top forecast signal |

### 12.5 Expandable detail panel (UI)

| Section | Fields |
| ------- | ------ |
| Exposure | Open, Overdue, Due 7d, Due 14d, Plafond, Credit utilization % |
| Forecast | Category, signals list (read from M29 context, not re-fetched) |
| Collection context | Recovery vs billing, salesman low recovery flag |
| Action | Category, owner, recommended action, report route |
| Traceability | Rule IDs, links to M29 / Collection / Piutang |

---

## 13. Future Extensibility

Design hooks for later milestones — **not V1 implementation**.

| Future capability | Extension point | Depends on |
| ----------------- | --------------- | ---------- |
| **M31 Customer Portfolio Optimization** | Combine M30 actions with M17/M18 portfolio signals | M30 action tables |
| **M32 Executive Business Health** | Promote COL-OPT-KPI-07, recovery context to executive composite | M16 Phase 2 |
| **Collection Scenario Planning** | What-if due date / payment lag adjustments in policy | Config UI |
| **Collector Route Planning** | Group priority queue by salesman route day | M25 field activity |
| **Collection Capacity Planning** | Compare action count vs team capacity config | Headcount config |
| **Payment Commitment Tracking** | Promise-to-pay date column; defer rules respect commitment | CRM / Desktop extension |
| **Promise-to-Pay Management** | New action category `FollowUpCommitment` | External data |
| **Collection Performance Optimization** | Compare recommended vs completed actions | Activity logging |
| **AI Recommendation Explanation** | Narrative over deterministic `ReasonText` | External — not V1 |
| **Executive Business Health narrative** | Cross-domain daily brief | M32 |

### 13.1 Recommended roadmap sequence

```text
M29  Customer Risk Forecast           ← complete
M30  Collection Optimization          ← this milestone
M31  Customer Portfolio Optimization  ← combines M30 + M17 + M18
M32  Executive Business Health        ← cross-domain operational composite
```

---

## 14. Product Owner Decision Checklist

Before implementation, confirm:

| # | Decision | Recommendation |
| - | -------- | -------------- |
| 1 | Consume M29 in-memory — no forecast recalculation | Approve |
| 2 | Extend Customer snapshot worker (after M29 step) | Approve — mirrors M28.5 pattern |
| 3 | Cross-read M20 collection snapshot for recovery context | Approve |
| 4 | Ten action categories (§6) | Approve |
| 5 | Collection impact = overdue + due within 7 days | Approve |
| 6 | Sales recovery overdue floor Rp 500K | Approve |
| 7 | Top 30 priority queue materialization | Approve |
| 8 | Specialized queue overlap allowed (filter model) | Approve |
| 9 | No Alert Center / Executive in V1 | Approve deferral |
| 10 | Strategic customer = Top 10 MTD omzet | Approve |

---

## Document Maintenance

When M30 is implemented:

1. Add Collection Optimization to `docs/features/btr-portal/btr-portal-domain.md`
2. Create `docs/features/collection-optimization/feature.md` permanent knowledge
3. Remove or archive this analysis after knowledge extraction

**Success criterion:** An Architect can design implementation from this document without additional business clarification.
