# Implementation Plan: Purchasing Attention List UX

## Document Status

| Field | Value |
| --- | --- |
| Initiative | Purchasing Attention List UX — bounded list, pagination, signal filters, section navigation |
| Authoritative requirements | `docs/features/btr-portal/purchasing-attention-list-ux/feature.md` |
| Related features | M21 Purchasing Management Dashboard, M24 Investigation |
| Route | `/dashboard/purchasing` (Purchasing Management Dashboard, M21) |
| Solution | `src/j05-btr-distrib/btr.portal.web` (portal SPA only) |
| Status | **Implemented** (2026-06-10) |
| API / snapshot changes | **None required** |

---

## 1. Goal

Improve **Purchasing Attention List** presentation so management can triage purchasing attention without slow page load, excessive page scroll, or burying mandatory sections (Charts, Top 10 Principals, Principal Exposure).

**Primary outcomes:**

- Bounded viewport with internal scroll (`max-height: 28rem`).
- Client-side pagination (default 25; options 10 / 25 / 50) with total item count.
- Signal filter chips for all eight M21 signals with per-signal counts.
- Card → list filter for all four attention card groups.
- Sticky in-page section navigation.
- Investigate drill-down (M24) and M21 business rules **unchanged**.

---

## 2. Authoritative Product Decisions

| # | Decision | Value |
| - | -------- | ----- |
| D1 | Pagination mode | Full paginated list inside bounded panel; explicit client-side slice + standalone `Paginator` |
| D2 | Posting Exposure card → filter | `QualifiedBacklog` |
| D3 | Principal Dependency card → filter | `CompoundDependency` |
| D4 | Purchasing Pace card → filter | `PurchasingInactivity` |
| D5 | Inventory Cross-Risk card → filter | `PrincipalInventoryNoPurchase` |
| D6 | Refresh behavior | Reset signal filter to All and pagination to page 1 |
| D7 | Filter grain | Cards count **principals**; list counts **entity × signal rows** |
| D8 | Data source | Full `AttentionList` from `GET /api/dashboard/purchasing` |
| D9 | M21 section order | Unchanged |

---

## 3. Architecture Overview

```text
GET /api/dashboard/purchasing            [UNCHANGED]
  ↓ full AttentionList payload
dashboardStore.loadPurchasing()         [UNCHANGED]
  ↓
PurchasingDashboardView.vue             [MODIFIED]
  ↓
PurchasingAttentionList.vue             [MODIFIED]
  ↓
purchasingAttentionSignals.ts           [NEW]
PurchasingAttentionCardGroup.vue        [MODIFIED — anchor navigate]
PurchasingAttentionCards.vue            [MODIFIED — card filter emit]
```

---

## 4. Affected Modules

| Module | Change |
| ------ | ------ |
| `purchasingAttentionSignals.ts` | **New** — signal constants, filter/count helpers |
| `purchasingAttentionSignals.spec.ts` | **New** — unit tests |
| `PurchasingAttentionList.vue` | Bounded panel, pagination, filters |
| `PurchasingAttentionCardGroup.vue` | `href`/`to` + `anchorNavigate` emit |
| `PurchasingAttentionCards.vue` | Card anchor links + `filterBySignal` emit |
| `PurchasingDashboardView.vue` | Section nav, filter state, card hooks, refresh reset, section IDs |

---

## 5. Detailed Design

### 5.1 Signal filter service

Eight keys aligned with `DashboardPurchasingManagementAggregator`:

`QualifiedBacklog`, `CompoundDependency`, `PrincipalInventoryNoPurchase`, `UnknownPrincipal`, `PrincipalAtRiskExposure`, `PrincipalSpendConcentration`, `PrincipalInventoryConcentration`, `PurchasingInactivity`

### 5.2 PurchasingAttentionList.vue

- `defineModel<string>('signalFilter')`
- `SelectButton` filter chips with counts
- Explicit `paginatedItems = filteredItems.slice(first, first + rows)` + standalone `Paginator`
- `DataTable` with `scrollable` + `scroll-height="flex"` inside bounded panel
- `.purchasing-attention-list__table-panel { max-height: 28rem; overflow: hidden }`
- Total count in title; hint for entity vs row grain
- Preserve `canOpenReport` / Company Investigate guard

### 5.3 PurchasingDashboardView.vue

Section nav IDs:

| Section | `id` |
| ------- | ---- |
| Attention Cards | `purchasing-attention-cards` |
| Summary | `purchasing-summary` |
| Attention List | `purchasing-attention-list` |
| Charts | `purchasing-charts` |
| Rankings | `purchasing-rankings` |

---

## 6. Test Strategy

| Case | File |
| ---- | ---- |
| Filter/count unit tests | `purchasingAttentionSignals.spec.ts` |
| Build | `npm run build` |
| Manual scenarios | Feature spec §7 |

---

## 7. Implementer Checklist

- [x] `purchasingAttentionSignals.ts` + unit tests
- [x] `PurchasingAttentionList.vue` — bounded panel, pagination, filters
- [x] `PurchasingAttentionCardGroup.vue` — `anchorNavigate` emit
- [x] `PurchasingAttentionCards.vue` — card filter emit
- [x] `PurchasingDashboardView.vue` — section nav, filter state, card hooks, refresh reset
- [x] Unit tests pass (`npm test`)
- [x] Production build passes (`npm run build`)
- [x] No API / aggregator changes

See `implementation-summary.md` for deliverable details.
