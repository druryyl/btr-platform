# Alert Center UX — Feature Specification

**Status:** Implemented (2026-06-11)  
**Type:** UX enhancement (no new business signals)  
**Route:** `/alerts` (Alert Center, M23)  
**Primary reference:** `docs/features/btr-portal/btr-portal-operational.md`, `docs/features/btr-portal/ALERT-REGISTRY.md`

---

## 1. Purpose

Improve Alert Center for **executive monitoring**: management should identify what requires attention within seconds without scrolling through large flat tables.

This enhancement changes **presentation and interaction only**. It does not change alert signals, categories, deduplication, or API contracts.

---

## 2. Business Problem

The original layout stacked one full `DataTable` per category (up to 120 rows). Category count badges were passive. Inventory Risk Summary and Concentrations appeared below all alert tables, pushing cross-domain context below the fold.

---

## 3. Solution

### Section order

1. Header, platform health (unchanged; presentation mode rules unchanged)
2. Sticky in-page section navigation
3. **Category Attention** — interactive KPI cards from `CategorySummaries`
4. **Top Critical Alerts** — cross-category top 5 rows (client-side sort on existing fields)
5. **Inventory Risk Summary** + **Concentrations** — side-by-side context row
6. **Alerts by Category** — collapsible panels with paginated, height-bounded tables
7. Domain dashboard links

### Concentrations panel

- **Collapsed by default** — informational metrics are on-demand, not part of exception triage.
- **Compact metric list** — no `DataTable`; dense label / value / dashboard-link rows.
- **Top 5 visible** when expanded; **Show all N metrics** toggles the remainder.
- **Bounded scroll** (`max-height: 16rem`) when the full list is shown.

### Prioritization (display only)

Top critical alerts sort by:

1. `AchievementBand` (Critical → Warning → none)
2. `SortOrder` ascending
3. `ValueAmount` descending
4. `EntityName` ascending

### Default interaction

- On load, the category with the highest `TotalCount` panel is expanded; others collapsed.
- Clicking a category card expands that panel and scrolls to it.
- Investigate and View Dashboard actions unchanged.

---

## 4. Components

| Component | Role |
|-----------|------|
| `AlertCenterCategoryCards.vue` | Interactive category summary cards |
| `AlertCenterCriticalStrip.vue` | Top 5 critical alerts strip |
| `AlertCenterCategoryPanels.vue` | Toggleable category panels |
| `AlertCenterCategoryTable.vue` | Paginated bounded table per category |
| `AlertCenterConcentrationsSection.vue` | Collapsed-by-default compact concentration metrics |
| `alertCenterPrioritization.ts` | Client-side sort and category helpers |
| `alertCenterAlertActions.ts` | Shared format and action guards |

Removed: `AlertCenterCategorySummary.vue`, `AlertCenterAlertTable.vue`.

---

## 5. Responsive behavior

- Category cards: 2-column grid on small screens
- Critical strip: card list on mobile; table on desktop
- Context row: stacked on mobile
- Touch-friendly action button height on mobile

---

## 6. Out of scope

- New `SignalKey` types or business rules
- API or composer changes
- Alert acknowledgment or history
