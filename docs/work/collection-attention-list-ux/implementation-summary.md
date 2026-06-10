# Implementation Summary: Collection Attention List UX

**Date:** 2026-06-10  
**Plan:** `docs/work/collection-attention-list-ux/implementation-plan.md`  
**Feature:** `docs/features/btr-portal/collection-attention-list-ux/feature.md`  
**Scope:** Portal SPA (`btr.portal.web`) — Collection Dashboard `/dashboard/collection`

---

## Delivered

### New files

| File | Purpose |
| ---- | ------- |
| `src/services/collectionAttentionSignals.ts` | Signal constants, client-side filter and count helpers for 7 M20 signals |
| `src/services/collectionAttentionSignals.spec.ts` | Unit tests for filter/count logic |

### Modified files

| File | Changes |
| ---- | ------- |
| `src/components/dashboard/CollectionAttentionList.vue` | Bounded table panel (28rem), client-side pagination (10/25/50), signal filter chips with counts, total item count in title, entity-vs-row hint, differentiated empty states |
| `src/components/dashboard/CollectionAttentionCardGroup.vue` | `to`/`href` props and `anchorNavigate` emit on in-page anchor click |
| `src/views/dashboard/CollectionDashboardView.vue` | Sticky section nav, `attentionSignalFilter` state, Exposure/Recovery/Portfolio card filter hooks, refresh resets filter to All, stable section `id` attributes |

### Documentation

| File | Purpose |
| ---- | ------- |
| `docs/features/btr-portal/collection-attention-list-ux/feature.md` | Feature specification |
| `docs/work/collection-attention-list-ux/implementation-plan.md` | Implementation plan |

### Unchanged (by design)

- `GET /api/dashboard/collection` API and payload
- `DashboardCollectionAggregator` and snapshot tables
- M24 Investigate routing per signal
- Wilayah rows remain non-investigable
- Sibling attention lists (Salesman, Inventory Risk, Purchasing, Location)

---

## Behavior summary

| Interaction | Result |
| ----------- | ------ |
| Load dashboard | Full attention list in memory; DOM renders one paginated page |
| Signal chips | Client-side filter; counts from full dataset (All + 7 M20 signals) |
| Exposure card click | Scroll to list; `ChronicOverdue` filter applied |
| Recovery card click | Scroll to list; `LowRecoveryVsBilling` filter applied |
| Portfolio card click | Scroll to list; `LegacyDebt` filter applied |
| Dashboard Refresh | Filter resets to All; data reloads |
| Section nav | Anchor links to Cards, Recovery, Aging Risk, List, Rankings |

---

## Verification

| Check | Result |
| ----- | ------ |
| `npm test` (collectionAttentionSignals.spec.ts) | Pass (4 tests) |
| `npm run build` | Pass |
| API / backend changes | None |

Manual scenarios from the feature spec should be executed in a staging environment with production-scale collection attention data.

## Deploy note (IIS at `http://localhost:8080`)

Source changes do not appear until the **built** `dist/` output is copied to the IIS static site (e.g. `C:\inetpub\btr-portal`). After `npm run build`, copy all files from `btr.portal.web/dist/` to that folder and hard-refresh the browser (Ctrl+Shift+R).

For local development without IIS, use `npm run dev` and open `http://localhost:5173/portal/dashboard/collection`.

## Pagination fix (2026-06-10 follow-up)

`CollectionAttentionList` uses **explicit client-side slicing** (`paginatedItems = filteredItems.slice(first, first + rows)`) plus a standalone PrimeVue `Paginator` below the table. This guarantees at most 25 rows in the DOM even if the DataTable built-in paginator is bypassed by caching or theme issues.

---

## Follow-up (out of scope)

Extract `BoundedAttentionDataTable.vue` and roll out bounded + paginated + filterable pattern to Salesman and other domain attention lists after Collection dashboard validation in production.
