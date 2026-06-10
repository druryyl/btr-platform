# Customer Attention List UX — Feature Specification

**Status:** Implemented (2026-06-10)  
**Type:** UX enhancement (no new business signals)  
**Date:** 2026-06-10  
**Analyst hand-off:** Architect → `docs/work/customer-attention-list-ux/implementation-plan.md`  
**Route:** `/dashboard/customers` (Customer Analytics, M17)  
**Primary reference:** `docs/work/btr-portal/M17-Customer-Analytics-Analysis.md` (Proposal A layout)

---

## 1. Purpose

Improve the **Customer Attention List** experience on the Customer Analytics dashboard so management can triage flagged customers quickly without slow page load, excessive scrolling, or burying other mandatory dashboard sections (Top Customer Rankings, Segmentation Summary).

This enhancement addresses **presentation and interaction** only. It does not change which customers appear in the attention list or how attention signals are computed.

---

## 2. Business Problem

### 2.1 Current behavior

The Customer Analytics page follows the approved **Proposal A — Customer Attention First** section order:

1. Customer Attention Cards  
2. Customer Attention List  
3. Top Customer Rankings  
4. Segmentation Summary  
5. Navigation  

The attention list renders **all** rows returned by the Customer snapshot — one row per **customer × signal** (up to four signals per customer: Overdue, Dormant, Plafond Breach, Suspended + Sales). In production-scale data this often means **hundreds of rows**.

The list is implemented as an unbounded `DataTable` with no pagination, no height constraint, and no signal filtering. Rankings and Segmentation are pushed far below the fold. Large DOM size causes noticeable slowness on initial render and during scroll.

### 2.2 User impact

| Affected user | Pain |
| ------------- | ---- |
| Management | Cannot reach Rankings or Segmentation without long scroll; page feels unresponsive |
| Sales Manager | Hard to focus on one signal type (e.g. dormant portfolio) when all signals are mixed |
| Collection Manager | Overdue triage requires scanning a very long table instead of a focused subset |

### 2.3 Business objective

Managers should be able to:

1. See summary attention counts on cards and **act on the list without friction**.  
2. Reach **Top Customer Rankings** and **Segmentation Summary** without scrolling through the entire attention list.  
3. Filter and page through attention rows when the full set is large.  
4. Continue the approved investigation flow: read signal → Investigate → Piutang or Sales report.

---

## 3. Users

| User | Goal on this page |
| ---- | ----------------- |
| Management | Identify which customers need attention; assess concentration and segmentation |
| Sales Manager | Review dormant / suspended-with-sales customers; drill to Sales report |
| Collection Manager | Review overdue / plafond-breach customers; drill to Piutang report |

No role-based routing change. All authenticated portal users retain access to Customer Analytics.

---

## 4. Scope

### 4.1 In scope

| Item | Detail |
| ---- | ------ |
| Customer Attention List panel | Bounded viewport, pagination, signal filters, total count display |
| Attention card → list interaction | Optional filter pre-selection when navigating from a card |
| Section discoverability | Sticky in-page section navigation (anchor links) |
| Consistency note | Document whether the same UX pattern should apply to other attention-list dashboards |

### 4.2 Out of scope

| Item | Reason |
| ---- | ------ |
| New attention signals or inclusion rules | Business rules unchanged (M17 approved set) |
| Backend row cap without full browse path | Would hide management risk |
| Reordering dashboard sections | Proposal A section order is PO-approved |
| New Customer Report | Explicitly excluded in M17 |
| API pagination / new endpoints | Analyst defers to Architect; client-side pagination is sufficient if full payload already loaded |
| Side-by-side layout (list + rankings) | Optional future enhancement; not in this specification |
| Alert Center (M23) changes | Separate surface; may share UX patterns only |

---

## 5. Approved UX Direction (Analyst Recommendation)

The following package is the **default requirement** for Architect and Implementer unless Product Owner overrides during review.

### 5.1 Bounded panel with internal scroll

- The Attention List card has a **maximum height** so the dashboard page length does not grow linearly with row count.  
- Rows scroll **inside** the card, not the entire page.  
- Exact pixel height is a design decision; target outcome: **Top Customer Rankings visible on a 1080p viewport without scrolling past the entire list**.

### 5.2 Client-side pagination

- Paginate the attention list using the same interaction model as portal reports (e.g. default **25 rows per page**, options **10 / 25 / 50**).  
- Display **total row count** in the panel header or footer (e.g. “342 attention items”).  
- Sorting within the current page set is desirable for triage (Signal, Customer name, Value); default sort order remains **aggregator order** (signal priority, then customer name) unless user applies a column sort.

### 5.3 Signal filter chips

- Filter controls for: **All · Overdue · Dormant · Plafond Breach · Suspended + Sales**.  
- Each chip shows **count for that signal** in the current dataset (derived from loaded rows).  
- Filtering is **client-side** on the full attention list already returned by `GET /api/dashboard/customers`.  
- Only one signal filter active at a time (plus All).  
- Empty filter state: show explicit empty message (e.g. “No customers match this signal”).

### 5.4 Attention card → list filter (recommended default)

| Card group | Suggested filter when user navigates to list |
| ---------- | --------------------------------------------- |
| Collection | Overdue |
| Inactivity | Dormant |
| Credit | Plafond Breach (or All credit-related: Plafond Breach + Suspended + Sales — **Architect to propose simplest mapping**) |
| Concentration, Activity | No filter (scroll/navigate to list only) |

The Credit card already links to `#customer-attention-list`. Extend card navigation so the list opens with the appropriate filter applied and pagination reset to page 1.

### 5.5 Sticky section navigation

- Add horizontal anchor navigation below the page subtitle: **Attention Cards · Attention List · Rankings · Segmentation**.  
- Clicking a link scrolls to the corresponding section.  
- Active section indication is optional but desirable.  
- Does not replace the bounded list; complements it for users who jump between sections.

---

## 6. Business Rules (Unchanged)

These rules remain authoritative. This feature must not alter them.

| Rule | Source |
| ---- | ------ |
| Attention list grain: **one row per customer × signal** | M17, M24 |
| Approved signals: Overdue, Dormant (90-day rule), Plafond Breach, Suspended + Sales | M17 Q18, Q14 |
| Signal sort priority in snapshot: Overdue → Plafond Breach → Suspended + Sales → Dormant | `DashboardCustomerAggregator` |
| Investigate routing: Overdue / Plafond → Piutang report; Dormant / Suspended → Sales report | M24 |
| Dashboard section order: Cards → List → Rankings → Segmentation → Navigation | M17 Proposal A |
| Full attention dataset available to the user (no silent omission) | Management attention philosophy (M16+) |

---

## 7. Workflow

### 7.1 Existing workflow (preserved)

```text
Open Customer Analytics
  → Read Attention Cards (summary counts)
  → Scan Attention List (per customer × signal)
  → Click Investigate on a row
  → Piutang or Sales Report opens with customer pre-filter
```

### 7.2 Enhanced workflow

```text
Open Customer Analytics
  → Read Attention Cards
  → [Optional] Click card (e.g. Credit) → scroll to list with signal filter applied
  → [Optional] Select signal chip to narrow list
  → Page through results inside bounded panel
  → [Optional] Use section nav to jump to Rankings or Segmentation
  → Investigate as before
```

### 7.3 Workflow impacts

| Area | Impact |
| ---- | ------ |
| Customer master / snapshot refresh | None |
| Investigation drill-down (M24) | None — Investigate action unchanged |
| Alert Center (M23) | None — separate feed with its own cap rules |
| Collection Dashboard (M20) | None — may benefit from shared UI component later |

---

## 8. Acceptance Criteria

### 8.1 Performance and layout

1. With **300+ attention rows** in the API response, the dashboard initial render does not freeze the UI for multiple seconds (subjective: usable within ~2s on typical office hardware).  
2. On a **1920×1080** display, **Top Customer Rankings** section header is visible without scrolling past an unbounded attention table (bounded panel + pagination).  
3. Scrolling the main page does not require traversing hundreds of table rows.

### 8.2 Pagination

4. Default page size is **25**; user can select 10, 25, or 50.  
5. Total attention item count is displayed and matches `AttentionList.length` from the API.  
6. Changing filter resets to **page 1**.

### 8.3 Signal filters

7. User can filter by each of the four approved signals plus All.  
8. Chip counts match the number of rows per `SignalKey` in the loaded dataset.  
9. Filtered empty state shows a clear message; Investigate is not shown when there are no rows.

### 8.4 Card navigation

10. Navigating from Credit card to `#customer-attention-list` applies a credit-related filter (exact mapping per Architect proposal in implementation plan).  
11. Collection and Inactivity cards, when they link to the list, apply Overdue and Dormant filters respectively (if those cards gain anchor links in this milestone).

### 8.5 Investigation (regression)

12. **Investigate** on every signal type still opens the correct report with customer pre-filter (M24 behavior unchanged).  
13. Row columns unchanged: Code, Customer, Signal, Value, Wilayah, action.

### 8.6 Section navigation

14. Anchor links scroll to Attention Cards, Attention List, Rankings, and Segmentation sections.  
15. Section IDs are stable for bookmarking and card `href` anchors.

### 8.7 Staleness and availability

16. When `IsAvailable` is false or list is empty, existing graceful empty/unavailable behavior is preserved.  
17. Staleness banner and refresh behavior unchanged.

---

## 9. Impact Summary (for Architect)

| Layer | Expected touch |
| ----- | -------------- |
| **Frontend** | `CustomerAttentionList.vue`, `CustomerDashboardView.vue`, possibly shared attention-list wrapper for other dashboards |
| **API** | Likely **no change** if client-side pagination/filter on existing payload |
| **Snapshot / aggregator** | **No change** to signal rules or row generation |
| **Tests** | UI/component tests; no aggregator test changes unless API changes |

### 9.1 Related components (read-only context)

| Artifact | Path |
| -------- | ---- |
| Attention list component | `btr.portal.web/src/components/dashboard/CustomerAttentionList.vue` |
| Dashboard view | `btr.portal.web/src/views/dashboard/CustomerDashboardView.vue` |
| Report pagination precedent | `btr.portal.web/src/views/reports/PiutangReportView.vue` |
| Alert Center cap precedent (informational) | M23 Top 20 per category — **not** applied to this dashboard list |

### 9.2 Cross-dashboard consistency (Architect decision)

These dashboards use the same unbounded attention-list pattern:

- Salesman (`SalesmanAttentionList.vue`)  
- Collection (`CollectionAttentionList.vue`)  
- Inventory Risk, Purchasing, Location  

**Analyst recommendation:** Implement a **reusable bounded + paginated + filterable attention table** pattern; roll out to Customer dashboard first, then evaluate rollout to siblings in the same or follow-up milestone.

---

## 10. Open Questions — Analyst Defaults

Architect may proceed with these defaults. Flag to Product Owner only if implementation reveals conflict.

| # | Question | Analyst default |
| - | -------- | --------------- |
| Q1 | Preview 20 + “View all” vs full paginated list? | **Full paginated list** inside bounded panel (no preview-only mode) |
| Q2 | Card → filter mapping for Credit card? | Filter **Plafond Breach** only when Credit card is clicked (Suspended + Sales remains separate chip) |
| Q3 | Scope: Customer only or all attention dashboards? | **Customer dashboard in this milestone**; extract shared component for future rollout |
| Q4 | User column sort vs fixed aggregator order? | **Default aggregator order**; optional column sort on current page is acceptable enhancement |

---

## 11. Verification Scenarios

| Scenario | Steps | Expected |
| -------- | ----- | -------- |
| Large list | Load dashboard with 300+ attention rows | Page responsive; rankings reachable without scrolling full list |
| Filter Overdue | Click Overdue chip | Only Overdue rows; count matches chip |
| Paginate | Go to page 2 with 25 rows/page | Correct slice; total count unchanged |
| Investigate Overdue | Click Investigate on overdue row | Piutang report with customer filter |
| Investigate Dormant | Click Investigate on dormant row | Sales report with customer filter |
| Credit card link | Click Credit card | Scroll to list; plafond-related filter applied |
| Empty filter | Filter signal with 0 rows | Empty state message |
| Refresh | Click dashboard Refresh | Filters reset or preserve — **Architect to choose; prefer reset to All** |

---

## 12. Risks (Business / UX)

| Risk | Mitigation |
| ---- | ---------- |
| Users think rows are missing due to pagination | Show total count prominently; “Page X of Y” |
| Filter counts disagree with card metrics | Cards count **customers**; list counts **rows** (customer × signal) — document in UI hint if confusion observed |
| Credit card filter feels incomplete (only Plafond) | Chip for Suspended + Sales always visible; PO can expand card filter later |

---

## 13. References

| Document | Relevance |
| -------- | --------- |
| `docs/work/btr-portal/M17-Customer-Analytics-Analysis.md` | Proposal A layout, signals, audience |
| `docs/features/btr-portal/btr-portal-domain.md` | Customer dashboard sections |
| `docs/features/btr-portal/btr-portal-architecture.md` | M17 response DTOs, attention list grain |
| `docs/work/btr-portal/M24 Dashboard Drilldown - Analysis.md` | Investigation flow per signal |
| `docs/work/btr-portal/M23 Alert Center - Plan.md` | Top-20 cap pattern (reference only, not applied here) |

---

## 14. Architect Deliverable

Produce:

```text
docs/work/customer-attention-list-ux/implementation-plan.md
```

The implementation plan should cover:

- Component structure (shared vs customer-specific)  
- Exact bounded height / scroll behavior  
- Pagination and filter state management (including refresh and card navigation)  
- Section nav markup and accessibility  
- Test strategy  
- Rollout recommendation for other attention-list dashboards  
- Confirmation that no API or snapshot changes are required (or document if otherwise)

**Do not implement** until the implementation plan is approved.
