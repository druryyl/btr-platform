# Collection Optimization Dashboard

**Feature:** M30 — Collection Optimization Dashboard  
**Status:** Current  
**Route:** `/dashboard/collection-optimization`  
**API:** `GET /api/dashboard/collection-optimization`

---

## Purpose

Given M29 customer risk forecasts and today's receivable position, Finance and Sales need a daily operational workspace that answers:

> *Which customers should we contact first today, with what action, and what collection impact can we expect?*

This is a **read-only**, **deterministic**, **explainable** optimization layer — not AI/ML.

---

## Relationship to Other Dashboards

| Dashboard | Question | M30 relationship |
| --------- | -------- | ---------------- |
| Customer Risk Forecast (M29) | Who will likely become a risk? | **Primary input** — consumes in-memory forecast contexts; does not recalculate |
| Collection (M20) | Is recovery working? | Cross-read `RecoveryVsBillingPercent` and attention signal keys |
| Piutang (M14) | Who owes money today? | Reuses piutang load in Customer worker for due-within-7/14 amounts |
| Customer Analytics (M17) | Current attention today? | Strategic customer rank (top MTD omzet) for priority boost |

---

## Action Categories

Ten categories with precedence (COL-OPT-CAT-01 through COL-OPT-CAT-10):

| Category | Typical owner | Meaning |
| -------- | ------------- | ------- |
| ImmediateCollection | Collection | Overdue with high severity |
| EscalateManagement | Management | Critical risk requiring leadership review |
| PriorityFollowUp | Collection | Overdue with elevated forecast risk |
| ProactiveReminder | Collection | Current balance, due soon, forecast risk |
| CreditReview | Finance | Plafond or projected breach |
| SalesRecoveryVisit | Sales | Decline dominant over collection urgency |
| LegacyDebtReview | Collection | Approaching dormant with balance |
| RelationshipMonitor | Sales/Collection | Strategic watch customer |
| DeferCollection | — | Healthy, no near-term urgency |
| NoActionToday | — | No actionable signals |

---

## Collection Priority Score

Integer sort key (not probability):

```
CollectionPriorityScore = ActionCategoryWeight + RiskCategoryWeight + ImpactComponent
                        + DueUrgencyComponent + StrategicBoost + RecoveryUrgencyBoost
```

Tie-break: `CollectionImpactAmount` descending, then customer name.

**Collection impact amount (V1):** `OverdueBalance + DueWithin7Days`

---

## Materialized Outputs

| Output | Cap | Description |
| ------ | --- | ----------- |
| Priority queue | 30 | Top customers by `CollectionPriorityScore` |
| Specialized queues | 15 each | ProactiveReminder, CreditReview, SalesRecovery, EscalateManagement |
| Impact opportunities | 15 | Highest `CollectionImpactAmount` |
| Workload | 10 per dimension | Salesman, Wilayah, Klasifikasi |

Every row includes `SelectionReasonText`, `PriorityReasonText`, `ActionReasonText`, and `TriggeredRuleIds`.

---

## Snapshot Refresh

- **Worker:** `RefreshDashboardCustomerSnapshotWorker` (Customer domain, ~30 min)
- **Chain:** M17 Customer Analytics → M29 Customer Risk Forecast → **M30 Collection Optimization**
- **M20 cross-read:** `IDashboardCollectionSnapshotDal.GetCurrent()` at refresh start
- **Transaction:** M17 + M29 + M30 written atomically via `ReplaceCurrent`

Manual refresh: `POST /api/admin/dashboard/refresh?domain=Customer`

---

## Out of Scope (V1)

- Changes to M29 forecast rules or API
- Automatic collection actions or Desktop write-back
- Promise-to-pay tracking, route planning, Alert Center integration
- Historical snapshot retention

---

## Related Artifacts

- Analysis: `docs/work/btr-portal/portal-analysis-m30-collection-optimization.md`
- Implementation plan: `docs/work/btr-portal/implementation-plan-m30-collection-optimization.md`
- M29 feature: `docs/features/customer-risk-forecast/feature.md`
