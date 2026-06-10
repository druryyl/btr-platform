# Implementation Plan: Collection Attention List UX

## Document Status

| Field | Value |
| --- | --- |
| Initiative | Collection Attention List UX — bounded list, pagination, signal filters, section navigation |
| Authoritative requirements | `docs/features/btr-portal/collection-attention-list-ux/feature.md` |
| Related features | M20 Collection Dashboard (`docs/work/btr-portal/M20 Collection Dashboard - Plan.md`), M24 Investigation |
| Route | `/dashboard/collection` (Collection Dashboard, M20) |
| Solution | `src/j05-btr-distrib/btr.portal.web` (portal SPA only) |
| Status | **Implemented** (2026-06-10) |
| API / snapshot changes | **None required** |

---

## 1. Goal

Improve **Collection Attention List** presentation so management can triage collection attention without slow page load, excessive page scroll, or burying mandatory sections (Recovery Summary, Aging Risk Summary, Top Overdue Rankings).

**Primary outcomes:**

- Bounded viewport with internal scroll (`max-height: 28rem`).
- Client-side pagination (default 25; options 10 / 25 / 50) with total item count.
- Signal filter chips for all seven M20 signals with per-signal counts.
- Card → list filter for Exposure, Recovery, Portfolio cards.
- Sticky in-page section navigation.
- Investigate drill-down (M24) and M20 business rules **unchanged**.

---

## 2. Authoritative Product Decisions

| # | Decision | Value |
| - | -------- | ----- |
| D1 | Pagination mode | Full paginated list inside bounded panel |
| D2 | Exposure card → filter | `ChronicOverdue` |
| D3 | Recovery card → filter | `LowRecoveryVsBilling` |
| D4 | Portfolio card → filter | `LegacyDebt` |
| D5 | Refresh behavior | Reset signal filter to All and pagination to page 1 |
| D6 | Filter grain | Cards count **entities**; list counts **entity × signal rows** |
| D7 | Data source | Full `AttentionList` from `GET /api/dashboard/collection` |
| D8 | M20 section order | Unchanged |

---

## 3. Architecture Overview

```text
GET /api/dashboard/collection          [UNCHANGED]
  ↓ full AttentionList payload
dashboardStore.loadCollection()       [UNCHANGED]
  ↓
CollectionDashboardView.vue           [MODIFIED]
  ↓
CollectionAttentionList.vue           [MODIFIED]
  ↓
collectionAttentionSignals.ts         [NEW]
CollectionAttentionCardGroup.vue      [MODIFIED — anchor navigate]
```

---

## 4. Affected Modules

| Module | Change |
| ------ | ------ |
| `collectionAttentionSignals.ts` | **New** — signal constants, filter/count helpers |
| `collectionAttentionSignals.spec.ts` | **New** — unit tests |
| `CollectionAttentionList.vue` | Bounded panel, pagination, filters |
| `CollectionAttentionCardGroup.vue` | `href`/`to` + `anchorNavigate` emit |
| `CollectionDashboardView.vue` | Section nav, filter state, card hooks, refresh reset, section IDs |

---

## 5. Detailed Design

### 5.1 Signal filter service

Seven keys aligned with `DashboardCollectionAggregator`:

`ChronicOverdue`, `PlafondBreachOverdue`, `LegacyDebt`, `Overdue`, `HighOverdueWorkload`, `LowRecoveryVsBilling`, `WilayahHotspot`

### 5.2 CollectionAttentionList.vue

- `defineModel<string>('signalFilter')`
- `SelectButton` filter chips with counts
- `DataTable` with `paginator`, `v-model:first`, `rows=25`, options `[10,25,50]`
- `.collection-attention-list__table-panel { max-height: 28rem; overflow: auto }`
- Total count in title; hint for entity vs row grain
- Preserve `canOpenReport` / Wilayah Investigate guard

### 5.3 CollectionDashboardView.vue

Section nav IDs:

| Section | `id` |
| ------- | ---- |
| Attention Cards | `collection-attention-cards` |
| Recovery Summary | `collection-recovery-summary` |
| Aging Risk Summary | `collection-aging-risk` |
| Attention List | `collection-attention-list` |
| Rankings | `collection-rankings` |

### 5.4 CollectionAttentionCardGroup.vue

Mirror `CustomerAttentionCardGroup.vue`: optional `to`/`href`, `anchorNavigate` emit.

---

## 6. Test Strategy

| Case | File |
| ---- | ---- |
| Filter/count unit tests | `collectionAttentionSignals.spec.ts` |
| Build | `npm run build` |
| Manual V1–V11 | Feature spec §7 |

---

## 7. Implementer Checklist

- [x] `collectionAttentionSignals.ts` + unit tests
- [x] `CollectionAttentionList.vue` — bounded panel, pagination, filters
- [x] `CollectionAttentionCardGroup.vue` — `anchorNavigate` emit
- [x] `CollectionDashboardView.vue` — section nav, filter state, card hooks, refresh reset
- [x] Unit tests pass (`npm test`)
- [x] Production build passes (`npm run build`)
- [x] No API / aggregator changes

See `implementation-summary.md` for deliverable details.
