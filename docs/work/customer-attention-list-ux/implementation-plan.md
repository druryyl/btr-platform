# Implementation Plan: Customer Attention List UX

## Document Status

| Field | Value |
| --- | --- |
| Initiative | Customer Attention List UX — bounded list, pagination, signal filters, section navigation |
| Authoritative requirements | `docs/features/btr-portal/customer-attention-list-ux/feature.md` |
| Related features | M17 Customer Analytics (`docs/work/btr-portal/M17-Customer-Analytics-Analysis.md`), M24 Investigation (`docs/work/btr-portal/M24 Dashboard Drill Down - Plan.md`) |
| Route | `/dashboard/customers` (Customer Analytics, M17) |
| Solution | `src/j05-btr-distrib/btr.portal.web` (portal SPA only) |
| Author role | Architect |
| Implementer input | This document |
| Status | **Implemented** (2026-06-10) |
| API / snapshot changes | **None required** |
| Open questions | **Resolved** — see Section 2 |

---

## 1. Goal

Improve the **Customer Attention List** presentation on Customer Analytics so management can triage flagged customers without slow page load, excessive page scroll, or burying mandatory sections (Top Customer Rankings, Segmentation Summary).

**Primary outcomes:**

- Attention list rendered inside a **bounded viewport** with **internal scroll** — page length no longer grows linearly with row count.
- **Client-side pagination** (default 25 rows; options 10 / 25 / 50) with **total item count** displayed.
- **Signal filter chips** (All · Overdue · Dormant · Plafond Breach · Suspended + Sales) with per-signal counts from the loaded dataset.
- **Card → list filter** pre-selection for Credit and Inactivity cards.
- **Sticky in-page section navigation** (Attention Cards · Attention List · Rankings · Segmentation).
- **Investigate** drill-down (M24) and all M17 business rules **unchanged**.

**Explicitly out of scope:**

- New attention signals, inclusion rules, or aggregator changes.
- Backend row caps, API pagination, or new endpoints.
- Reordering dashboard sections (Proposal A order preserved).
- Sibling dashboard rollout (Collection, Salesman, Inventory Risk, Purchasing, Location) — deferred to follow-up milestone.
- Alert Center (M23) changes.

---

## 2. Authoritative Product Decisions

Source: `feature.md` Sections 5, 10, 11. Do not re-decide during implementation.

| # | Decision | Value |
| - | -------- | ----- |
| D1 | Pagination mode | **Full paginated list** inside bounded panel — no preview-only / “View all” pattern |
| D2 | Credit card → list filter | **Plafond Breach** only (`PlafondBreach` signal key) |
| D3 | Inactivity card → list filter | **Dormant** (`Dormant` signal key) |
| D4 | Collection card | **No change** — continues linking to `/dashboard/piutang` (M17). Overdue filter on Collection card is out of scope unless PO adds anchor link later (feature AC 8.4.11) |
| D5 | Milestone scope | **Customer dashboard only**; extract shared component in follow-up |
| D6 | Default sort | **Aggregator order** (signal priority, then customer name). Optional column sort on current page is acceptable but not required |
| D7 | Refresh behavior | **Reset signal filter to All** and pagination to page 1 |
| D8 | Filter grain | Cards count **customers**; list counts **rows** (customer × signal) — surface hint in UI |
| D9 | Data source | Full `AttentionList` from existing `GET /api/dashboard/customers` — no silent omission |

---

## 3. Architecture Overview

### 3.1 Change boundary

```text
GET /api/dashboard/customers          [UNCHANGED]
  ↓ full AttentionList payload
dashboardStore.loadCustomer()         [UNCHANGED]
  ↓
CustomerDashboardView.vue             [MODIFIED — section nav, filter state, card hooks, refresh reset]
  ↓
CustomerAttentionList.vue             [MODIFIED — bounded panel, pagination, filters]
  ↓
customerAttentionSignals.ts           [NEW — filter/count helpers]
CustomerAttentionCardGroup.vue        [MODIFIED — anchor navigate emit]
```

**No changes** to:

- `DashboardCustomerAggregator`, snapshot tables, refresh worker
- `DashboardCustomerDal`, API controller, DTO shapes
- M24 `navigateToInvestigation` routing per signal

### 3.2 Why client-side only

The Customer snapshot already returns the complete `AttentionList` array. Management philosophy (M16+) requires the full attention dataset be available — not a capped preview. Client-side filter and pagination:

- Avoids new API surface and snapshot schema work.
- Keeps aggregator sort order intact until user applies optional column sort.
- Bounds DOM size to one page of rows (~25) regardless of total count.

**Performance note:** Vue still holds the full array in memory (acceptable — typical lists are hundreds, not tens of thousands). DOM rendering is the bottleneck this plan addresses.

### 3.3 Component strategy (Customer-first, shared later)

| Layer | This milestone | Follow-up milestone |
| ----- | -------------- | ------------------- |
| Filter/count logic | `customerAttentionSignals.ts` — reusable pure functions | Extend or generalize per dashboard signal keys |
| List UI | Enhance `CustomerAttentionList.vue` directly | Extract `BoundedAttentionDataTable.vue` wrapper when rolling out to siblings |
| Section nav | Inline in `CustomerDashboardView.vue` | Extract `DashboardSectionNav.vue` if second dashboard adopts same pattern |
| Card anchor hook | `CustomerAttentionCardGroup.vue` `@anchor-navigate` emit | Reuse emit pattern on other dashboards if needed |

**Rationale:** Sibling attention lists (`CollectionAttentionList`, `SalesmanAttentionList`, etc.) have different column schemas and signal sets. Premature abstraction risks over-engineering. Implement and validate on Customer dashboard first.

---

## 4. Impact Analysis

### 4.1 Affected modules

| Module | Change type | Detail |
| ------ | ----------- | ------ |
| `btr.portal.web/src/components/dashboard/CustomerAttentionList.vue` | Modify | Bounded panel, pagination, signal filters, count display, empty states |
| `btr.portal.web/src/views/dashboard/CustomerDashboardView.vue` | Modify | Section nav, `attentionSignalFilter` state, card filter hooks, refresh reset, section `id` attributes |
| `btr.portal.web/src/components/dashboard/CustomerAttentionCardGroup.vue` | Modify | Emit `anchorNavigate` on anchor click (Credit, Inactivity) |
| `btr.portal.web/src/services/customerAttentionSignals.ts` | **New** | Signal constants, `filterCustomerAttentionItems`, `countCustomerAttentionBySignal` |
| `btr.portal.web/src/services/customerAttentionSignals.spec.ts` | **New** | Unit tests for filter/count helpers |

### 4.2 Unaffected modules (regression only)

| Module | Verification |
| ------ | ------------ |
| `DashboardCustomerAggregator.cs` | Attention row generation unchanged |
| `navigateToInvestigation.ts` | Investigate per signal unchanged |
| `CustomerSegmentationSection.vue`, `Top10RankingTable.vue` | Layout position unchanged; should be reachable sooner |
| Alert Center (M23) | Separate surface |

### 4.3 Cross-dashboard consistency (deferred)

These dashboards still use unbounded `DataTable` — **no change in this milestone**:

- `CollectionAttentionList.vue`
- `SalesmanAttentionList.vue`
- `InventoryRiskAttentionList.vue`
- `PurchasingAttentionList.vue`
- `LocationAttentionList.vue`

**Rollout recommendation (follow-up):** After Customer dashboard validation, introduce `BoundedAttentionDataTable.vue` accepting slot-based columns and generic filter config; migrate Collection and Salesman first (highest row counts), then Inventory Risk / Purchasing / Location.

---

## 5. Detailed Design

### 5.1 Signal filter service (`customerAttentionSignals.ts`)

Centralize approved M17 signal keys and client-side operations:

```typescript
export const CUSTOMER_ATTENTION_SIGNAL_ALL = ''

export const CUSTOMER_ATTENTION_SIGNAL_KEYS = [
  'Overdue',
  'Dormant',
  'PlafondBreach',
  'SuspendedWithSales',
] as const

export const CUSTOMER_ATTENTION_SIGNAL_LABELS: Record<CustomerAttentionSignalKey, string> = {
  Overdue: 'Overdue',
  Dormant: 'Dormant',
  PlafondBreach: 'Plafond Breach',
  SuspendedWithSales: 'Suspended + Sales',
}

export function filterCustomerAttentionItems(items, signalKey): items[]
export function countCustomerAttentionBySignal(items): Record<SignalKey, number>
```

- Filter matches `item.SignalKey` (aggregator canonical keys — must align with `DashboardCustomerAggregator` constants).
- `CUSTOMER_ATTENTION_SIGNAL_ALL` (`''`) returns unfiltered list.
- Counts derived from **full** `props.items`, not filtered subset (chip counts always reflect total dataset).

### 5.2 `CustomerAttentionList.vue`

#### Props and state

| Item | Detail |
| ---- | ------ |
| Props | `items: DashboardCustomerAttentionItem[]`, `loading: boolean` |
| `v-model:signalFilter` | Two-way binding to parent (`defineModel<string>('signalFilter')`) |
| Local state | `first` (paginator offset, ref 0), `rows` (page size, default 25) |

#### Filter UI

- PrimeVue `SelectButton` with options built from `filterOptions` computed:
  - `All (N)` where N = `items.length`
  - `{Label} (count)` per signal from `countCustomerAttentionBySignal`
- `aria-label="Filter by attention signal"` on SelectButton.
- Hint text below filters: *“Cards count customers; this list counts customer × signal rows.”*

#### Pagination

Match portal report precedent (`PiutangReportView.vue`):

```vue
<DataTable
  v-model:first="first"
  :value="filteredItems"
  paginator
  :rows="rows"
  :rows-per-page-options="[10, 25, 50]"
  striped-rows
>
```

- Default `rows = 25`.
- `watch(signalFilter)` → reset `first = 0`.
- `watch(() => props.items)` → reset `first = 0` (data refresh).

#### Bounded viewport / scroll

**Target:** On 1920×1080, Top Customer Rankings section header visible without scrolling past an unbounded attention table.

**Implementation:**

```css
.customer-attention-list__table-panel {
  max-height: 28rem;   /* 448px — table body scroll region */
  overflow: auto;
}
```

- Wrap `DataTable` in `.customer-attention-list__table-panel`.
- **28rem** balances ~10–12 visible rows plus paginator chrome inside the card.
- Card title + filter row sit **above** the bounded panel (not inside scroll region).
- If paginator ends up inside the scroll panel during QA, acceptable for v1; optional improvement is PrimeVue `scrollable` + `scrollHeight` to pin paginator below tbody.

#### Header count display

In card title area (when not loading):

```text
{items.length} attention item(s)
```

Shows **total** API row count, not filtered count — avoids “missing rows” confusion when a filter is active. Filtered count is implicit from chip selection.

#### Empty states

| Condition | Message |
| --------- | ------- |
| `items.length === 0` | “No customers require attention.” |
| Filter active, `filteredItems.length === 0` | “No customers match this signal.” |
| No Investigate buttons when empty (DataTable `#empty` slot) |

#### Columns (unchanged — regression)

Code · Customer · Signal · Value · Wilayah · Investigate action.

`investigate()` continues using `navigateToInvestigation` + `item.Investigation` (M24).

#### Section anchor

Parent assigns `id="customer-attention-list"` on the component root (or wrapping element) for card `href` and section nav.

### 5.3 `CustomerDashboardView.vue`

#### Section navigation

Add sticky horizontal nav below page subtitle (inside `DashboardDetailLayout` default slot):

```typescript
const sectionNavItems = [
  { id: 'customer-attention-cards', label: 'Attention Cards' },
  { id: 'customer-attention-list', label: 'Attention List' },
  { id: 'customer-rankings', label: 'Rankings' },
  { id: 'customer-segmentation', label: 'Segmentation' },
]
```

Markup:

```html
<nav class="customer-dashboard__section-nav" aria-label="Dashboard sections">
  <a v-for="item in sectionNavItems" :key="item.id" :href="`#${item.id}`" ...>
    {{ item.label }}
  </a>
</nav>
```

CSS:

- `position: sticky; top: 0; z-index: 2` with content background and bottom border.
- Section targets: `scroll-margin-top: 3.5rem` to offset sticky nav on anchor jump.

**Accessibility:** Native anchor navigation; `aria-label` on `<nav>`. Active-section highlighting is **optional** (IntersectionObserver) — not required for acceptance.

#### Stable section IDs

| Section | `id` attribute |
| ------- | -------------- |
| Attention Cards | `customer-attention-cards` |
| Attention List | `customer-attention-list` |
| Rankings | `customer-rankings` |
| Segmentation | `customer-segmentation` |

#### Filter state wiring

```typescript
const attentionSignalFilter = ref(CUSTOMER_ATTENTION_SIGNAL_ALL)

function setAttentionSignalFilter(signalKey: string): void {
  attentionSignalFilter.value = signalKey
}

function onRefresh(): void {
  attentionSignalFilter.value = CUSTOMER_ATTENTION_SIGNAL_ALL
  void dashboard.loadCustomer()
}
```

```html
<CustomerAttentionList
  id="customer-attention-list"
  v-model:signal-filter="attentionSignalFilter"
  :items="dashboard.customer?.AttentionList ?? []"
  :loading="dashboard.loading"
/>
```

Wire `@refresh` on `DashboardDetailLayout` to `onRefresh` (replaces direct `loadCustomer` if not already).

#### Card → filter mapping

| Card | Navigation | Filter on navigate |
| ---- | ---------- | ------------------ |
| Collection | `to="/dashboard/piutang"` | None (unchanged M17) |
| Concentration | None | None |
| Activity | `to="/dashboard/sales"` | None |
| Inactivity | `href="#customer-attention-list"` | `Dormant` |
| Credit | `href="#customer-attention-list"` | `PlafondBreach` |

```html
<CustomerAttentionCardGroup
  title="Inactivity"
  href="#customer-attention-list"
  @anchor-navigate="setAttentionSignalFilter('Dormant')"
/>

<CustomerAttentionCardGroup
  title="Credit"
  href="#customer-attention-list"
  @anchor-navigate="setAttentionSignalFilter('PlafondBreach')"
/>
```

`CustomerAttentionCardGroup` emits `anchorNavigate` on anchor `@click` **before** browser scroll — filter applies immediately when list comes into view.

### 5.4 `CustomerAttentionCardGroup.vue`

Add emit:

```typescript
const emit = defineEmits<{ anchorNavigate: [] }>()
```

On anchor variant (`<a :href="href">`):

```html
@click="emit('anchorNavigate')"
```

Router-link and static cards unchanged.

---

## 6. State Management Summary

```text
                    ┌─────────────────────────────────────┐
                    │ CustomerDashboardView               │
                    │  attentionSignalFilter (ref)        │
                    │  onRefresh → reset filter to All  │
                    └──────────────┬──────────────────────┘
                                   │ v-model:signal-filter
                    ┌──────────────▼──────────────────────┐
                    │ CustomerAttentionList               │
                    │  filteredItems (computed)           │
                    │  first, rows (local pagination)     │
                    │  watch filter → first = 0         │
                    └─────────────────────────────────────┘
         ┌─────────────────────────┼─────────────────────────┐
         │                         │                         │
  Card anchor-navigate      SelectButton chips         Dashboard refresh
  sets parent filter        updates v-model            resets parent filter
```

| Event | Filter | Page (`first`) |
| ----- | ------ | -------------- |
| Credit / Inactivity card click | Set to mapped signal | Reset to 0 (via filter change) |
| Chip selection | Update v-model | Reset to 0 |
| Dashboard Refresh | Reset to All | Reset to 0 (via items watch + filter reset) |
| API reload (items reference change) | Preserved unless refresh handler runs | Reset to 0 |

---

## 7. API and Backend Confirmation

| Question | Answer |
| -------- | ------ |
| New endpoints? | **No** |
| Query params on `GET /api/dashboard/customers`? | **No** |
| Snapshot / aggregator changes? | **No** |
| DTO changes? | **No** — `DashboardCustomerAttentionItem.SignalKey` already present |

Existing aggregator sort order (Overdue → Plafond Breach → Suspended + Sales → Dormant) remains the implicit default order of `filteredItems` when no column sort is applied.

---

## 8. Test Strategy

### 8.1 Unit tests (required)

**File:** `customerAttentionSignals.spec.ts`

| Case | Assertion |
| ---- | --------- |
| `filterCustomerAttentionItems([], '')` | Empty array |
| `filterCustomerAttentionItems(items, '')` | Returns all |
| `filterCustomerAttentionItems(items, 'Overdue')` | Only Overdue rows |
| `countCustomerAttentionBySignal` | Correct counts per `SignalKey`; zero for absent signals |

### 8.2 Manual verification (required)

Execute scenarios from `feature.md` Section 11:

| # | Scenario | Pass criteria |
| - | -------- | ------------- |
| V1 | 300+ attention rows | Page usable ~2s; Rankings header visible at 1080p without scrolling full list |
| V2 | Overdue chip | Only Overdue rows; chip count matches |
| V3 | Page 2 (25/page) | Correct slice; header total unchanged |
| V4 | Investigate Overdue | Piutang report, customer pre-filter |
| V5 | Investigate Dormant | Sales report, customer pre-filter |
| V6 | Credit card click | Scroll to list; Plafond Breach filter active |
| V7 | Inactivity card click | Scroll to list; Dormant filter active |
| V8 | Empty filter | Clear empty-state message |
| V9 | Refresh | Filter resets to All; data reloads |
| V10 | Section nav links | Each anchor scrolls to correct section |
| V11 | `IsAvailable = false` | Existing unavailable styling preserved |

### 8.3 Component tests (optional)

Vitest + Vue Test Utils for `CustomerAttentionList` smoke render with mocked items — defer unless team wants regression guard for pagination/filter wiring.

### 8.4 Backend tests

**None** — no server changes.

---

## 9. Implementation Steps

Execute in order. Each step should compile and remain deployable.

### Step 1 — Signal filter service

1. Create `src/services/customerAttentionSignals.ts` with constants and pure functions (Section 5.1).
2. Create `src/services/customerAttentionSignals.spec.ts`.
3. Run `npm test` in `btr.portal.web`.

### Step 2 — Attention list component

1. Add `v-model:signalFilter` to `CustomerAttentionList.vue`.
2. Add `SelectButton` filter chips with counts.
3. Add `filteredItems` computed using `filterCustomerAttentionItems`.
4. Enable DataTable `paginator`, `v-model:first`, `rows=25`, `rows-per-page-options=[10,25,50]`.
5. Add `.customer-attention-list__table-panel` with `max-height: 28rem; overflow: auto`.
6. Add total count in title; add cards-vs-rows hint.
7. Implement differentiated empty messages.
8. Preserve Investigate column and `formatValue` logic.

### Step 3 — Card group anchor emit

1. Add `anchorNavigate` emit to `CustomerAttentionCardGroup.vue`.
2. Emit on anchor `@click`.

### Step 4 — Dashboard view integration

1. Add `attentionSignalFilter` ref and `setAttentionSignalFilter`.
2. Wire `v-model:signal-filter` on `CustomerAttentionList`.
3. Add `@anchor-navigate` handlers on Inactivity (Dormant) and Credit (PlafondBreach) cards.
4. Add `onRefresh` that resets filter then calls `loadCustomer`.
5. Add sticky section nav and section `id` attributes on Cards, List, Rankings, Segmentation blocks.
6. Add `scroll-margin-top` on `.customer-dashboard__section`.

### Step 5 — Verification

1. Run unit tests.
2. Run portal dev server; execute manual scenarios V1–V11.
3. Confirm no changes to network payload shape (single `GET /api/dashboard/customers`).

---

## 10. Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
| ---- | ---------- | ------ | ---------- |
| Users think rows are missing due to pagination | Medium | Medium | Prominent total count in title; paginator shows page range |
| Chip counts vs card metrics confusion | Medium | Low | Hint text documenting customer vs row grain |
| Credit card filter feels incomplete (Plafond only) | Low | Low | Suspended + Sales chip always available; PO can expand later |
| Large in-memory array (1000+ rows) | Low | Low | Acceptable for management scale; monitor if lists exceed ~2000 |
| Paginator inside scroll panel | Low | Low | Bounded page length still met; refine with `scrollable` DataTable if QA feedback |
| Filter state lost on accidental navigation away | Low | Low | Acceptable; refresh resets to All |

---

## 11. Rollout Recommendation (Sibling Dashboards)

**This milestone:** Customer Analytics only.

**Follow-up milestone (proposed):**

1. Extract `BoundedAttentionDataTable.vue`:
   - Props: `items`, `loading`, `totalLabel`, `emptyMessages`, optional `signalFilterConfig`
   - Slots: `columns`, `title-extra`
   - Encapsulates: bounded panel CSS, paginator defaults, filter SelectButton pattern
2. Migrate in priority order:
   - `CollectionAttentionList.vue` (piutang triage, likely high row count)
   - `SalesmanAttentionList.vue`
   - `InventoryRiskAttentionList.vue`, `PurchasingAttentionList.vue`, `LocationAttentionList.vue`
3. Each sibling may need dashboard-specific signal key maps — do not force Customer signal keys into shared abstraction.

**Do not block Customer milestone on shared extraction.**

---

## 12. Acceptance Criteria Traceability

| AC | Section | How verified |
| -- | ------- | ------------ |
| 8.1.1–8.1.3 Performance/layout | 5.2 bounded panel | Manual V1 |
| 8.2.4–8.2.6 Pagination | 5.2 paginator | Manual V3; code review |
| 8.3.7–8.3.9 Signal filters | 5.1, 5.2 filters | Unit tests + Manual V2, V7 |
| 8.4.10–8.4.11 Card navigation | 5.3 card mapping | Manual V6, V7 |
| 8.5.12–8.5.13 Investigation regression | 5.2 investigate unchanged | Manual V4, V5 |
| 8.6.14–8.6.15 Section nav | 5.3 section nav | Manual V10 |
| 8.7.16–8.7.17 Staleness/availability | No change to banner/unavailable | Manual V11 |

---

## 13. References

| Document | Relevance |
| -------- | --------- |
| `docs/features/btr-portal/customer-attention-list-ux/feature.md` | Authoritative requirements |
| `docs/work/btr-portal/M17-Customer-Analytics-Analysis.md` | Proposal A layout, signals |
| `docs/work/btr-portal/M24 Dashboard Drill Down - Plan.md` | Investigate routing per signal |
| `btr.portal.web/src/views/reports/PiutangReportView.vue` | Pagination interaction precedent |
| `DashboardCustomerAggregator.cs` | Signal keys and sort order (read-only) |

---

## 14. Implementer Checklist

- [x] `customerAttentionSignals.ts` + unit tests
- [x] `CustomerAttentionList.vue` — bounded panel, pagination, filters, counts, hints
- [x] `CustomerAttentionCardGroup.vue` — `anchorNavigate` emit
- [x] `CustomerDashboardView.vue` — section nav, filter state, card hooks, refresh reset, section IDs
- [x] Unit tests pass (`npm test`)
- [x] Production build passes (`npm run build`)
- [x] No API / aggregator / snapshot changes
- [x] Feature artifact `feature.md` status **Implemented**

See `implementation-summary.md` for deliverable details.
