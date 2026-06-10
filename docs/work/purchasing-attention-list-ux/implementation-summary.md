# Implementation Summary: Purchasing Attention List UX

**Date:** 2026-06-10  
**Plan:** `docs/work/purchasing-attention-list-ux/implementation-plan.md`  
**Feature:** `docs/features/btr-portal/purchasing-attention-list-ux/feature.md`  
**Scope:** Portal SPA (`btr.portal.web`) — Purchasing Management Dashboard `/dashboard/purchasing`

---

## Delivered

### New files

| File | Purpose |
| ---- | ------- |
| `src/services/purchasingAttentionSignals.ts` | Signal constants, client-side filter and count helpers for 8 M21 signals |
| `src/services/purchasingAttentionSignals.spec.ts` | Unit tests for filter/count logic |

### Modified files

| File | Changes |
| ---- | ------- |
| `src/components/dashboard/PurchasingAttentionList.vue` | Bounded table panel (28rem), client-side pagination (10/25/50), signal filter chips with counts, total item count in title, entity-vs-row hint, differentiated empty states |
| `src/components/dashboard/PurchasingAttentionCardGroup.vue` | `to`/`href` props and `anchorNavigate` emit on in-page anchor click |
| `src/components/dashboard/PurchasingAttentionCards.vue` | Card anchor links with `filterBySignal` emit per card group |
| `src/views/dashboard/PurchasingDashboardView.vue` | Sticky section nav, `attentionSignalFilter` state, card filter hooks, refresh resets filter to All, stable section `id` attributes |

### Documentation

| File | Purpose |
| ---- | ------- |
| `docs/features/btr-portal/purchasing-attention-list-ux/feature.md` | Feature specification |
| `docs/work/purchasing-attention-list-ux/implementation-plan.md` | Implementation plan |

### Unchanged (by design)

- `GET /api/dashboard/purchasing` API and payload
- `DashboardPurchasingManagementAggregator` and snapshot tables
- M24 Investigate routing per signal
- Company (PurchasingInactivity) rows remain non-investigable
- Sibling attention lists (Salesman, Inventory Risk, Location)

---

## Behavior summary

| Interaction | Result |
| ----------- | ------ |
| Load dashboard | Full attention list in memory; DOM renders one paginated page |
| Signal chips | Client-side filter; counts from full dataset (All + 8 M21 signals) |
| Posting Exposure card click | Scroll to list; `QualifiedBacklog` filter applied |
| Principal Dependency card click | Scroll to list; `CompoundDependency` filter applied |
| Purchasing Pace card click | Scroll to list; `PurchasingInactivity` filter applied |
| Inventory Cross-Risk card click | Scroll to list; `PrincipalInventoryNoPurchase` filter applied |
| Dashboard Refresh | Filter resets to All; data reloads |
| Section nav | Anchor links to Cards, Summary, List, Charts, Rankings |

---

## Verification

| Check | Result |
| ----- | ------ |
| `npm test` (purchasingAttentionSignals.spec.ts) | Pass |
| `npm run build` | Pass |
| API / backend changes | None |

Manual scenarios from the feature spec should be executed in a staging environment with production-scale purchasing attention data.

## Deploy note (IIS at `http://localhost:8080`)

Source changes do not appear until the **built** `dist/` output is copied to the IIS static site. After `npm run build`, copy all files from `btr.portal.web/dist/` to the IIS folder and hard-refresh the browser (Ctrl+Shift+R).

For local development without IIS, use `npm run dev` and open `http://localhost:5173/portal/dashboard/purchasing`.
