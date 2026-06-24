# Proposal — M30 Rule Code Readability in Portal UX

| Field | Value |
| --- | --- |
| Type | Team discussion material |
| Milestone | M30 — Collection Optimization Dashboard |
| Status | **Proposal** (not approved for implementation) |
| Date | 2026-06-22 |
| Audience | Product, Finance/Sales stakeholders, Portal developers |
| Related analysis | [portal-analysis-m30-collection-optimization.md](./portal-analysis-m30-collection-optimization.md), [portal-analysis-m29-customer-risk-forecast.md](./portal-analysis-m29-customer-risk-forecast.md) |
| Related feature | [docs/features/collection-optimization/feature.md](../features/collection-optimization/feature.md) |

---

## 1. Problem statement

On the **Collection Optimization** dashboard (`/dashboard/collection-optimization`), expanded priority rows show a field labeled **Rules** with raw comma-separated IDs, for example:

```text
Rules: CRF-REC-03,CRF-P02,COL-OPT-CAT-01,COL-OPT-REC-02
```

These IDs are **intentional traceability** — they record which M29 forecast rules and M30 action rules contributed to each row. They are valuable for audit, support, and rule tuning.

However, **operators (Finance and Sales) cannot interpret them without reading analysis documents or asking developers**. The codes expose internal naming (`OPT`, `CAT`, `REC`, `CRF-P`) that is meaningful to engineers but opaque in the UI.

**Question for the team:** How much rule-code explainability should we surface in the Portal, and for which audiences?

---

## 2. Current state

### 2.1 What the UI already does well

Each priority row exposes **plain-language explainability** in the expanded detail panel:

| Field | Purpose | Example audience |
| ----- | ------- | ------------------ |
| **Why selected** | Why the customer appears on today's list | Operator |
| **Why priority** | Why they rank high in the queue | Operator |
| **Why action** | Why this action category was assigned (often includes the CAT rule ID inline) | Operator |
| **Rules** | Comma-separated rule trace IDs | Support / audit / power users |

The main grid already shows human labels: **Action** (`Immediate Collection`), **Risk** (M29 category), **Owner** (Collection / Sales / Finance).

This follows the same pattern as other Portal dashboards (M29 Customer Risk Forecast shows `PrimarySignalLabel`, not `RuleId`; Inventory Risk shows `SignalLabel`).

### 2.2 Where the gap is

- **Rules** is the only field that shows raw internal IDs with no decoder.
- There is no dashboard-level **“How to read this page”** help.
- Rule IDs mix **two milestones** (M29 forecast + M30 optimization) without visual grouping.
- Prefixes **OPT** (Optimization) and **CAT** (Category) are not explained anywhere in the product UI.

### 2.3 Backend contract (unchanged today)

`TriggeredRuleIds` is built in `CollectionOptimizationActionBuilder.BuildTriggeredRuleIds()`:

1. M29 recommendation rule ID (e.g. `CRF-REC-03`)
2. Top-severity M29 forecast signal rule ID (e.g. `CRF-P02`)
3. M30 action category rule ID (e.g. `COL-OPT-CAT-01`)
4. Always `COL-OPT-REC-02` (explainability meta-rule)

Stored as a single comma-separated string in `BTRPD_CollectionOptimizationPriority.TriggeredRuleIds`.

---

## 3. Design principles (proposal)

| Principle | Rationale |
| --------- | --------- |
| **Operators read labels, not codes** | Daily workflow should not require memorizing rule IDs |
| **Codes remain available for audit** | Do not remove traceability — decode and de-emphasize |
| **Progressive disclosure** | Help at page level; detail at row level; full glossary optional |
| **Consistency with M29** | Forecast layer (CRF-*) and action layer (COL-OPT-*) should read as one product story |
| **Minimal backend change in V1** | Prefer frontend glossary first; structured API later if rules grow |

---

## 4. Recommended UX (phased)

### Phase A — Page-level “How to read this dashboard” (recommended first)

Add a collapsible help panel (Accordion or `?` Popover) on the Collection Optimization view.

**Section 1 — For daily use (no codes needed)**

```text
1. Scan Action and Owner columns — what to do and who owns it today.
2. Sort by Priority and Impact — who to contact first.
3. Expand a row for Why selected / Why priority / Why action.
4. Use Drill down to Customer Risk Forecast when you need forecast context.
```

**Section 2 — Rule code format (optional / power users)**

```text
CRF-*          Risk forecast rules (M29) — why we expect future trouble
  CRF-P*       Payment / delay signals
  CRF-C*       Credit limit signals
  CRF-I*       Inactivity signals
  CRF-D*       Purchase decline signals
  CRF-L*       Collection risk signals
  CRF-REC*     Forecast recommendations (e.g. early collection planning)

COL-OPT-CAT-*  Today's action category (Immediate Collection, Reminder, …)
COL-OPT-REC-*  How the row is written and ranked (explainability, not an action)
```

**Effort:** Low (Portal Web only). **Risk:** None to data or refresh pipeline.

---

### Phase B — Decode rule IDs in the expanded row (recommended second)

Replace the raw string:

```text
Rules: CRF-REC-03,CRF-P02,COL-OPT-CAT-01,COL-OPT-REC-02
```

With **grouped chips + tooltips**:

```text
Forecast (M29)    [CRF-REC-03] [CRF-P02]
Action (M30)      [COL-OPT-CAT-01]
Audit (M30)       [COL-OPT-REC-02]
```

Each chip shows a one-line tooltip, e.g.:

| Rule ID | Tooltip label | Tooltip description |
| ------- | ------------- | ------------------- |
| CRF-REC-03 | Early Collection Planning | Open balance due within 7 days |
| CRF-P02 | Escalating Overdue | Overdue in 1–60 day bucket with slow payment pattern |
| COL-OPT-CAT-01 | Immediate Collection | Overdue balance with elevated forecast risk |
| COL-OPT-REC-02 | Explainability | Reason text cites primary trigger (overdue, due date, or signal) |

**Copy change:** Rename **Rules** → **Rule trace (technical)** and use muted styling.

**Implementation sketch:**

- New file: `btr.portal.web/src/services/collectionOptimizationRuleGlossary.ts`
- Shared M29 rule entries can live in `customerRiskForecastRuleGlossary.ts` (or one combined glossary module)
- Component: `TriggeredRuleTrace.vue` (chips + PrimeVue Tooltip)
- Unit tests: parse unknown IDs gracefully (show raw ID, no crash)

**Effort:** Medium (Portal Web only). **Risk:** Glossary drift if backend rules change without frontend update.

---

### Phase C — Cross-links (optional enhancement)

When a row includes `CRF-*` rules:

- Link chip or “View forecast” to `/dashboard/customer-risk-forecast` (customer-scoped drill-down when route exists).

When `COL-OPT-CAT-*` applies:

- Tooltip repeats the **Action** column label so users see the same words in two places (grid + trace).

**Effort:** Low–medium. **Risk:** Depends on drill-down route availability per customer row.

---

### Phase D — Structured rules from API (longer-term)

Extend API response from:

```json
"TriggeredRuleIds": "CRF-REC-03,CRF-P02,COL-OPT-CAT-01,COL-OPT-REC-02"
```

To:

```json
"TriggeredRules": [
  {
    "RuleId": "CRF-REC-03",
    "Layer": "Forecast",
    "Label": "Early Collection Planning",
    "Description": "Balance due within 7 days."
  }
]
```

**Pros:** Single source of truth; UI stays thin; supports i18n later.  
**Cons:** Schema change, snapshot DAL, aggregator, and API DTO updates.

**Recommendation:** Defer until rule set stabilizes or glossary maintenance becomes painful.

---

## 5. Wireframe (expanded row — proposed)

```text
┌─────────────────────────────────────────────────────────────────┐
│ Today's Collection Priorities                          [? Help] │
├─────────────────────────────────────────────────────────────────┤
│ ▼ 1  PT ABC   [Immediate Collection]  142  Rp 45.000.000  ...   │
│   Why selected:  Overdue Rp 12.000.000 and Escalating Overdue…  │
│   Why priority:  Ranked high due to immediate collection…       │
│   Why action:    Immediate Collection assigned (COL-OPT-CAT-01)…  │
│                                                                 │
│   Rule trace (technical)                                        │
│   Forecast (M29)  [CRF-REC-03 ⓘ] [CRF-P02 ⓘ]                    │
│   Action (M30)    [COL-OPT-CAT-01 ⓘ]                            │
│   Audit (M30)     [COL-OPT-REC-02 ⓘ]                            │
└─────────────────────────────────────────────────────────────────┘
```

---

## 6. Reference — rule ID naming (discussion cheat sheet)

### 6.1 Prefix guide

| Prefix | Meaning | Milestone |
| ------ | ------- | --------- |
| **CRF-** | **C**ustomer **R**isk **F**orecast | M29 |
| **COL-OPT-** | **Col**lection **Opt**imization | M30 |
| **CAT** | Action **Cat**egory (what to do today) | M30 |
| **REC** | **Rec**ommendation / explainability (how rows are written) | M30 |

### 6.2 Example row walkthrough

```text
CRF-REC-03,CRF-P02,COL-OPT-CAT-01,COL-OPT-REC-02
```

| Order | Rule | Layer | Plain-language meaning |
| ----- | ---- | ----- | ---------------------- |
| 1 | CRF-REC-03 | M29 recommendation | Early collection planning — balance due within 7 days |
| 2 | CRF-P02 | M29 signal | Escalating overdue trajectory — overdue with average payment lag ≥ 14 days |
| 3 | COL-OPT-CAT-01 | M30 action | Immediate collection — overdue with high severity / chronic overdue |
| 4 | COL-OPT-REC-02 | M30 meta | Row reason text must cite primary trigger |

**Story in one sentence:** *Forecast flagged due-soon and escalating overdue risk; optimization assigned immediate collection today.*

### 6.3 M30 action categories (COL-OPT-CAT-01 … 10)

| Rule ID | Action category | Typical owner |
| ------- | ----------------- | ------------- |
| COL-OPT-CAT-01 | Immediate Collection | Collection |
| COL-OPT-CAT-02 | Escalate to Management | Management |
| COL-OPT-CAT-03 | Priority Follow-up | Collection |
| COL-OPT-CAT-04 | Proactive Reminder | Collection |
| COL-OPT-CAT-05 | Credit Review | Finance |
| COL-OPT-CAT-06 | Sales Recovery Visit | Sales |
| COL-OPT-CAT-07 | Legacy Debt Review | Collection |
| COL-OPT-CAT-08 | Relationship Monitor | Sales / Collection |
| COL-OPT-CAT-09 | Safe to Wait | — |
| COL-OPT-CAT-10 | No Action Today | — |

Full conditions: analysis §6.1.

### 6.4 M30 explainability rules (COL-OPT-REC-01 … 07)

| Rule ID | Summary |
| ------- | ------- |
| COL-OPT-REC-01 | `NoActionToday` excluded from priority queue tables |
| COL-OPT-REC-02 | Reason text cites primary trigger (always appended to trace) |
| COL-OPT-REC-03 | Priority reason cites top score components |
| COL-OPT-REC-04 | Action reason cites category rule ID |
| COL-OPT-REC-05 | `TriggeredRuleIds` composition rule |
| COL-OPT-REC-06 | Sales vs Collection owner routing |
| COL-OPT-REC-07 | Materialized row caps |

### 6.5 M29 rules most often seen in M30 traces

| Family | Example IDs | When they appear |
| ------ | ------------- | ---------------- |
| Payment delay | CRF-P01 … CRF-P04 | Slow payer, escalating overdue, no recent payment |
| Credit limit | CRF-C01 … CRF-C03 | Plafond breach or approaching limit |
| Inactivity | CRF-I01 … CRF-I03 | Approaching dormant, legacy debt forward |
| Purchase decline | CRF-D01 … CRF-D03 | Revenue drop patterns |
| Collection risk | CRF-L01 … CRF-L04 | High risk category, chronic trajectory |
| Recommendations | CRF-REC-01 … | Mapped from M29 `CustomerRiskRecommendationBuilder` |

Complete M29 rule tables: [portal-analysis-m29-customer-risk-forecast.md §7–8](./portal-analysis-m29-customer-risk-forecast.md).

---

## 7. Options comparison

| Option | Operator clarity | Audit trace | Effort | Drift risk |
| ------ | ---------------- | ----------- | ------ | ---------- |
| **A. Do nothing** | Low (codes opaque) | Full | None | None |
| **B. Page help only (Phase A)** | Medium | Full | Low | None |
| **C. Help + decoded chips (A + B)** | **High** | Full | Medium | Medium (static glossary) |
| **D. Structured API (Phase D)** | High | Full | High | **Low** |
| **E. Hide rule trace entirely** | High for operators | **Lost** | Low | None — **not recommended** |

**Recommended default for team approval:** **Option C** (Phase A + B), with Phase D on the backlog.

---

## 8. Scope and non-goals

### In scope (if approved)

- Collection Optimization priority table expanded row
- Optional: same pattern on specialized queue tabs and impact table if they expose `TriggeredRuleIds`
- Frontend glossary and tests
- Dashboard help panel copy ( Indonesian / English — **decision needed**, see §9)

### Out of scope (this proposal)

- Changing M29 or M30 rule logic or thresholds
- Changing snapshot refresh or SQL schema (unless Phase D approved)
- Full rule encyclopedia modal with every CRF-01 … CRF-KPI rule (link to analysis docs instead)
- Alert Center or Executive Dashboard integration

---

## 9. Open questions for team discussion

1. **Audience:** Should rule trace be visible to all Portal users, or only roles with Finance/Admin access?
2. **Language:** Should tooltips and help panel be **English**, **Indonesian**, or bilingual (match existing Portal copy convention)?
3. **Visibility default:** Should **Rule trace** be collapsed behind “Show technical details” by default?
4. **M29 parity:** Should we apply the same chip + help pattern to Customer Risk Forecast if `RuleId` is exposed elsewhere later?
5. **Glossary ownership:** Who maintains the frontend glossary when analysis rules change — implementer at rule change time, or periodic doc sync?
6. **Phase D timing:** Is structured `TriggeredRules[]` worth doing now, or after first UAT feedback?
7. **Training:** Is a one-page PDF / internal wiki link sufficient instead of in-app help for some teams?

---

## 10. Suggested decision record (fill in after meeting)

| Decision | Choice | Date | Notes |
| -------- | ------ | ---- | ----- |
| Approved option | A / B / C / D | | |
| Phases in scope | | | |
| Default visibility of rule trace | Expanded / Collapsed / Role-gated | | |
| UI language for help text | | | |
| Owner for implementation | | | |
| Target release | | | |

---

## 11. Implementation checklist (if Option C approved)

- [ ] Add `collectionOptimizationRuleGlossary.ts` (+ shared M29 REC/P/C/I/D/L entries used in traces)
- [ ] Add `TriggeredRuleTrace.vue` component
- [ ] Update `CollectionOptimizationPriorityTable.vue` expanded panel
- [ ] Add collapsible help to `CollectionOptimizationDashboardView.vue`
- [ ] Unit tests: glossary lookup, unknown ID fallback, grouped rendering
- [ ] UAT: Finance operator reads one row without developer assistance
- [ ] Update [feature.md](../features/collection-optimization/feature.md) § UX explainability after ship

**Estimated effort:** 1–2 developer days (Portal Web + tests), assuming no API change.

---

## 12. Related artifacts

| Artifact | Relevance |
| -------- | --------- |
| [portal-analysis-m30-collection-optimization.md §6.1, §8.3](./portal-analysis-m30-collection-optimization.md) | Authoritative COL-OPT-CAT and COL-OPT-REC definitions |
| [portal-analysis-m29-customer-risk-forecast.md §7](./portal-analysis-m29-customer-risk-forecast.md) | Authoritative CRF signal and scope rules |
| [implementation-summary-m30-collection-optimization.md](./implementation-summary-m30-collection-optimization.md) | Current delivered UI behavior |
| `CollectionOptimizationPriorityTable.vue` | Current Rules display |
| `CollectionOptimizationActionBuilder.cs` | `TriggeredRuleIds` composition |

---

*This document is temporary work for team discussion. After a decision is made and UX is implemented, incorporate the approved help copy into permanent feature knowledge and archive or remove this proposal.*
