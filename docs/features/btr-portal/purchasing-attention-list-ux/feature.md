# Purchasing Attention List UX — Feature Specification

**Status:** Implemented (2026-06-10)  
**Type:** UX enhancement (no new business signals)  
**Date:** 2026-06-10  
**Architect hand-off:** `docs/work/purchasing-attention-list-ux/implementation-plan.md`  
**Route:** `/dashboard/purchasing` (Purchasing Management Dashboard, M21)  
**Primary reference:** `docs/archive/btr-portal/m21-purchasing-management/M21 Purchasing Dashboard - Plan.md`

---

## 1. Purpose

Improve the **Purchasing Attention List** experience on the Purchasing Management Dashboard so management can triage flagged principals and purchasing activities quickly without slow page load, excessive scrolling, or burying other mandatory dashboard sections (Charts, Top 10 Principals, Principal Exposure).

This enhancement addresses **presentation and interaction** only. It does not change which entities appear in the attention list or how attention signals are computed.

---

## 2. Business Problem

### 2.1 Current behavior

The Purchasing Management Dashboard follows the approved M21 **Attention-First layout** section order:

1. Purchasing Attention Cards  
2. Purchasing Summary  
3. Purchasing Attention List  
4. Weekly Purchase Trend + Posting Status Breakdown  
5. Top 10 Principals + Principal Exposure Comparison  
6. Navigation  

The attention list renders **all** rows returned by the Purchasing Management snapshot — one row per **entity × signal** (Principal or Company with up to eight approved signals). In production-scale data this often means **hundreds of rows**.

The list is implemented as an unbounded `DataTable` with no pagination, no height constraint, and no signal filtering. Charts and Rankings are pushed far below the fold. Large DOM size causes noticeable slowness on initial render and during scroll.

### 2.2 User impact

| Affected user | Pain |
| ------------- | ---- |
| Management | Cannot reach Charts or Rankings without long scroll; page feels unresponsive |
| Purchasing Manager | Hard to focus on one signal type (e.g. qualified backlog) when all signals are mixed |
| Inventory Manager | Cross-risk signals buried in an undifferentiated long table |

### 2.3 Business objective

Managers should be able to:

1. See summary attention counts on cards and **act on the list without friction**.  
2. Reach **Charts** and **Top 10 Principals** without scrolling through the entire attention list.  
3. Filter and page through attention rows when the full set is large.  
4. Continue the approved investigation flow: read signal → Investigate → Purchasing Report.

---

## 3. Users

| User | Goal on this page |
| ---- | ----------------- |
| Management | Identify which principals and purchasing activities require attention |
| Purchasing Manager | Review qualified backlog, posting exposure, and purchasing pace |
| Inventory Manager | Review inventory cross-risk principals with no purchase activity |

No role-based routing change. All authenticated portal users retain access to Purchasing Management Dashboard.

---

## 4. Scope

### 4.1 In scope

| Item | Detail |
| ---- | ------ |
| Purchasing Attention List panel | Bounded viewport, pagination, signal filters, total count display |
| Attention card → list interaction | Filter pre-selection when navigating from card groups |
| Section discoverability | Sticky in-page section navigation (anchor links) |
| Consistency | Same UX pattern as Customer and Collection Attention List (2026-06-10) |

### 4.2 Out of scope

| Item | Reason |
| ---- | ------ |
| New attention signals or inclusion rules | M21 approved set unchanged |
| Backend row cap without full browse path | Would hide management risk |
| Reordering dashboard sections | M21 fixed section order is PO-approved |
| API pagination / new endpoints | Client-side pagination on existing payload |
| Shared `BoundedAttentionDataTable.vue` extraction | Deferred to follow-up milestone |
| V1 statistics / chart changes | Unchanged |

---

## 5. Approved UX Direction

### 5.1 Bounded panel with internal scroll

- The Attention List card has a **maximum height** so the dashboard page length does not grow linearly with row count.  
- Rows scroll **inside** the card, not the entire page.  
- Target outcome: **Top 10 Principals** section header visible on a 1080p viewport without scrolling past the entire list.

### 5.2 Client-side pagination

- Paginate using portal report precedent: default **25 rows per page**, options **10 / 25 / 50**.  
- Display **total row count** in the panel header (e.g. "342 attention items").  
- Default sort order remains **aggregator order** (signal priority per `DashboardPurchasingManagementAggregator`).

### 5.3 Signal filter chips

- Filter controls for all eight M21 signals plus **All**.  
- Each chip shows **count for that signal** in the current dataset.  
- Filtering is **client-side** on the full `AttentionList` from `GET /api/dashboard/purchasing`.  
- Only one signal filter active at a time (plus All).  
- Empty filter state: explicit message (e.g. "No items match this signal").

### 5.4 Attention card → list filter

| Card | Filter when user navigates to list |
| ---- | ---------------------------------- |
| Posting Exposure | `QualifiedBacklog` |
| Principal Dependency | `CompoundDependency` |
| Purchasing Pace | `PurchasingInactivity` |
| Inventory Cross-Risk | `PrincipalInventoryNoPurchase` |

### 5.5 Sticky section navigation

- Horizontal anchor navigation below the page subtitle: **Attention Cards · Summary · Attention List · Charts · Rankings**.  
- Clicking a link scrolls to the corresponding section.  
- Complements the bounded list for users who jump between sections.

---

## 6. Business Rules (Unchanged)

| Rule | Source |
| ---- | ------ |
| Attention list grain: **one row per entity × signal** | M21 §2.5 |
| Approved signals: QualifiedBacklog, PrincipalSpendConcentration, PrincipalInventoryConcentration, PrincipalAtRiskExposure, CompoundDependency, PurchasingInactivity, PrincipalInventoryNoPurchase, UnknownPrincipal | M21 §2.5 |
| Signal priority in snapshot: QualifiedBacklog → CompoundDependency → PrincipalInventoryNoPurchase → UnknownPrincipal → PrincipalAtRiskExposure → PrincipalSpendConcentration → PrincipalInventoryConcentration → PurchasingInactivity | `DashboardPurchasingManagementAggregator` |
| Investigate routing: Principal rows → Purchasing Report; Company (PurchasingInactivity) → no Investigate | M21 §2.6 |
| Dashboard section order: Cards → Summary → List → Charts → Rankings → Navigation | M21 §2.6 |
| Full attention dataset available (no silent omission) | Management attention philosophy |

---

## 7. Acceptance Criteria

### 7.1 Performance and layout

1. With **300+ attention rows**, dashboard initial render does not freeze the UI for multiple seconds.  
2. On **1920×1080**, **Top 10 Principals** section header visible without scrolling past an unbounded attention table.  
3. Scrolling the main page does not require traversing hundreds of table rows.

### 7.2 Pagination

4. Default page size **25**; user can select 10, 25, or 50.  
5. Total attention item count displayed and matches `AttentionList.length`.  
6. Changing filter resets to **page 1**.

### 7.3 Signal filters

7. User can filter by each of eight approved signals plus All.  
8. Chip counts match rows per `SignalKey` in the loaded dataset.  
9. Filtered empty state shows a clear message.

### 7.4 Card navigation

10. Posting Exposure, Principal Dependency, Purchasing Pace, and Inventory Cross-Risk cards scroll to list with mapped filter applied.

### 7.5 Investigation (regression)

11. **Investigate** on Principal rows still opens Purchasing Report with pre-filter.  
12. Company (PurchasingInactivity) rows show no Investigate button.  
13. Row columns unchanged: Entity, Signal, Amount, Context, action.

### 7.6 Section navigation

14. Anchor links scroll to Attention Cards, Summary, Attention List, Charts, and Rankings.  
15. Section IDs stable for bookmarking and card `href` anchors.

### 7.7 Staleness and availability

16. When management data is unavailable or list is empty, existing graceful behavior preserved.  
17. Staleness banner and refresh behavior unchanged; refresh resets filter to All.

---

## 8. References

| Document | Relevance |
| -------- | --------- |
| `docs/archive/btr-portal/m21-purchasing-management/M21 Purchasing Dashboard - Plan.md` | M21 layout, signals, audience |
| `docs/features/btr-portal/customer-attention-list-ux/feature.md` | Reference UX pattern |
| `docs/features/btr-portal/collection-attention-list-ux/feature.md` | Reference UX pattern (pagination fix) |
| `docs/work/btr-portal/M24 Dashboard Drill Down - Plan.md` | Investigation flow |
