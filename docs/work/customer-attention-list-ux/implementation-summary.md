# Implementation Summary: Customer Attention List UX

**Date:** 2026-06-10  
**Plan:** `docs/work/customer-attention-list-ux/implementation-plan.md`  
**Feature:** `docs/features/btr-portal/customer-attention-list-ux/feature.md`  
**Scope:** Portal SPA (`btr.portal.web`) — Customer Analytics `/dashboard/customers`

---

## Delivered

### New files

| File | Purpose |
| ---- | ------- |
| `src/services/customerAttentionSignals.ts` | Signal constants, client-side filter and count helpers |
| `src/services/customerAttentionSignals.spec.ts` | Unit tests for filter/count logic |

### Modified files

| File | Changes |
| ---- | ------- |
| `src/components/dashboard/CustomerAttentionList.vue` | Bounded table panel (28rem), client-side pagination (10/25/50), signal filter chips with counts, total item count in title, cards-vs-rows hint, differentiated empty states |
| `src/components/dashboard/CustomerAttentionCardGroup.vue` | `anchorNavigate` emit on in-page anchor click |
| `src/views/dashboard/CustomerDashboardView.vue` | Sticky section nav, `attentionSignalFilter` state, Credit/Inactivity card filter hooks, refresh resets filter to All, stable section `id` attributes |

### Unchanged (by design)

- `GET /api/dashboard/customers` API and payload
- `DashboardCustomerAggregator` and snapshot tables
- M24 Investigate routing per signal
- Sibling attention lists (Collection, Salesman, Inventory Risk, Purchasing, Location)

---

## Behavior summary

| Interaction | Result |
| ----------- | ------ |
| Load dashboard | Full attention list in memory; DOM renders one paginated page |
| Signal chips | Client-side filter; counts from full dataset |
| Credit card click | Scroll to list; `PlafondBreach` filter applied |
| Inactivity card click | Scroll to list; `Dormant` filter applied |
| Collection card | Still links to `/dashboard/piutang` (unchanged M17) |
| Dashboard Refresh | Filter resets to All; data reloads |
| Section nav | Anchor links to Cards, List, Rankings, Segmentation |

---

## Verification

| Check | Result |
| ----- | ------ |
| `npm test` (customerAttentionSignals.spec.ts) | Pass (4 tests) |
| `npm run build` | Pass |
| API / backend changes | None |

Manual scenarios V1–V11 from the feature spec should be executed in a staging environment with production-scale customer attention data.

---

## Follow-up (out of scope)

Extract `BoundedAttentionDataTable.vue` and roll out bounded + paginated + filterable pattern to Collection, Salesman, and other domain attention lists after Customer dashboard validation in production.
