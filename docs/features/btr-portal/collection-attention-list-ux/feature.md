# Collection Attention List UX — Feature Specification

**Status:** Implemented (2026-06-10)  
**Type:** UX enhancement (no new business signals)  
**Date:** 2026-06-10  
**Architect hand-off:** `docs/work/collection-attention-list-ux/implementation-plan.md`  
**Route:** `/dashboard/collection` (Collection Dashboard, M20)  
**Primary reference:** `docs/work/btr-portal/M20 Collection Dashboard - Plan.md`

---

## 1. Purpose

Improve the **Collection Attention List** experience on the Collection Dashboard so management can triage flagged customers, salesmen, and wilayah hotspots quickly without slow page load, excessive scrolling, or burying other mandatory dashboard sections (Recovery Summary, Aging Risk Summary, Top Overdue Rankings).

This enhancement addresses **presentation and interaction** only. It does not change which entities appear in the attention list or how attention signals are computed.

---

## 2. Business Problem

### 2.1 Current behavior

The Collection Dashboard follows the approved M20 **Attention-First layout** section order:

1. Collection Attention Cards  
2. Recovery Summary  
3. Aging Risk Summary  
4. Collection Attention List  
5. Top Overdue Customers / Salesmen / Wilayah  
6. Navigation  

The attention list renders **all** rows returned by the Collection snapshot — one row per **entity × signal** (Customer, Salesman, or Wilayah with up to seven approved signals). In production-scale data this often means **hundreds of rows**.

The list is implemented as an unbounded `DataTable` with no pagination, no height constraint, and no signal filtering. Recovery, Aging, and Rankings are pushed far below the fold. Large DOM size causes noticeable slowness on initial render and during scroll.

### 2.2 User impact

| Affected user | Pain |
| ------------- | ---- |
| Management | Cannot reach Recovery, Aging, or Rankings without long scroll; page feels unresponsive |
| Collection Manager | Hard to focus on one signal type (e.g. chronic overdue customers) when all signals are mixed |
| Sales Manager | Recovery and workload signals buried in an undifferentiated long table |

### 2.3 Business objective

Managers should be able to:

1. See summary attention counts on cards and **act on the list without friction**.  
2. Reach **Recovery Summary**, **Aging Risk Summary**, and **Top Overdue Rankings** without scrolling through the entire attention list.  
3. Filter and page through attention rows when the full set is large.  
4. Continue the approved investigation flow: read signal → Investigate → Piutang report (Customer and Salesman rows).

---

## 3. Users

| User | Goal on this page |
| ---- | ----------------- |
| Management | Assess recovery performance and overdue exposure concentration |
| Collection Manager | Triage chronic overdue, legacy debt, and plafond-breach customers |
| Sales Manager | Review low recovery and high overdue workload on reps |

No role-based routing change. All authenticated portal users retain access to Collection Dashboard.

---

## 4. Scope

### 4.1 In scope

| Item | Detail |
| ---- | ------ |
| Collection Attention List panel | Bounded viewport, pagination, signal filters, total count display |
| Attention card → list interaction | Filter pre-selection when navigating from Exposure, Recovery, or Portfolio cards |
| Section discoverability | Sticky in-page section navigation (anchor links) |
| Consistency | Same UX pattern as Customer Attention List (2026-06-10) |

### 4.2 Out of scope

| Item | Reason |
| ---- | ------ |
| New attention signals or inclusion rules | M20 approved set unchanged |
| Backend row cap without full browse path | Would hide management risk |
| Reordering dashboard sections | M20 fixed section order is PO-approved |
| API pagination / new endpoints | Client-side pagination on existing payload |
| Entity-type filter chips | Signal filters suffice for v1 |
| Shared `BoundedAttentionDataTable.vue` extraction | Deferred to follow-up milestone |
| Wilayah row drill-down | M20 — no Piutang report filter for wilayah |

---

## 5. Approved UX Direction

### 5.1 Bounded panel with internal scroll

- The Attention List card has a **maximum height** so the dashboard page length does not grow linearly with row count.  
- Rows scroll **inside** the card, not the entire page.  
- Target outcome: **Top Overdue Customers** section header visible on a 1080p viewport without scrolling past the entire list.

### 5.2 Client-side pagination

- Paginate using portal report precedent: default **25 rows per page**, options **10 / 25 / 50**.  
- Display **total row count** in the panel header (e.g. "342 attention items").  
- Default sort order remains **aggregator order** (signal priority per `DashboardCollectionAggregator`).

### 5.3 Signal filter chips

- Filter controls for: **All · Chronic Overdue · Plafond Breach + Overdue · Legacy Debt · Overdue · High Overdue Workload · Low Recovery vs Billing · Wilayah Hotspot**.  
- Each chip shows **count for that signal** in the current dataset.  
- Filtering is **client-side** on the full `AttentionList` from `GET /api/dashboard/collection`.  
- Only one signal filter active at a time (plus All).  
- Empty filter state: explicit message (e.g. "No items match this signal").

### 5.4 Attention card → list filter

| Card | Filter when user navigates to list |
| ---- | ---------------------------------- |
| Exposure | `ChronicOverdue` |
| Recovery | `LowRecoveryVsBilling` |
| Portfolio | `LegacyDebt` |

### 5.5 Sticky section navigation

- Horizontal anchor navigation below the page subtitle: **Attention Cards · Recovery · Aging Risk · Attention List · Rankings**.  
- Clicking a link scrolls to the corresponding section.  
- Complements the bounded list for users who jump between sections.

---

## 6. Business Rules (Unchanged)

| Rule | Source |
| ---- | ------ |
| Attention list grain: **one row per entity × signal** | M20 §2.5 |
| Approved signals: ChronicOverdue, LegacyDebt, PlafondBreachOverdue, Overdue, HighOverdueWorkload, LowRecoveryVsBilling, WilayahHotspot | M20 §2.5 |
| Signal priority in snapshot: LowRecovery → WilayahHotspot → ChronicOverdue → PlafondBreach → LegacyDebt → HighOverdueWorkload → Overdue | `DashboardCollectionAggregator` |
| Investigate routing: Customer and Salesman rows → Piutang report; Wilayah rows → no Investigate | M20 §2.8 |
| Dashboard section order: Cards → Recovery → Aging → List → Rankings → Navigation | M20 §2.8 |
| Full attention dataset available (no silent omission) | Management attention philosophy |

---

## 7. Acceptance Criteria

### 7.1 Performance and layout

1. With **300+ attention rows**, dashboard initial render does not freeze the UI for multiple seconds.  
2. On **1920×1080**, **Top Overdue Customers** section header visible without scrolling past an unbounded attention table.  
3. Scrolling the main page does not require traversing hundreds of table rows.

### 7.2 Pagination

4. Default page size **25**; user can select 10, 25, or 50.  
5. Total attention item count displayed and matches `AttentionList.length`.  
6. Changing filter resets to **page 1**.

### 7.3 Signal filters

7. User can filter by each of seven approved signals plus All.  
8. Chip counts match rows per `SignalKey` in the loaded dataset.  
9. Filtered empty state shows a clear message.

### 7.4 Card navigation

10. Exposure, Recovery, and Portfolio cards scroll to list with mapped filter applied.

### 7.5 Investigation (regression)

11. **Investigate** on Customer and Salesman rows still opens Piutang report with pre-filter (M24).  
12. Wilayah rows show no Investigate button.  
13. Row columns unchanged: Type, Name, Signal, Detail, Wilayah, action.

### 7.6 Section navigation

14. Anchor links scroll to Attention Cards, Recovery, Aging Risk, Attention List, and Rankings.  
15. Section IDs stable for bookmarking and card `href` anchors.

### 7.7 Staleness and availability

16. When `IsAvailable` is false or list is empty, existing graceful behavior preserved.  
17. Staleness banner and refresh behavior unchanged; refresh resets filter to All.

---

## 8. References

| Document | Relevance |
| -------- | --------- |
| `docs/work/btr-portal/M20 Collection Dashboard - Plan.md` | M20 layout, signals, audience |
| `docs/features/btr-portal/customer-attention-list-ux/feature.md` | Reference UX pattern |
| `docs/work/btr-portal/M24 Dashboard Drill Down - Plan.md` | Investigation flow |
