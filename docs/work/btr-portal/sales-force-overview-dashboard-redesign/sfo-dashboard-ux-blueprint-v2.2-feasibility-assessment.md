# FEASIBILITY ASSESSMENT

## Sales Force Overview Dashboard — UX Blueprint v2.2

> Supersedes the v2.0 assessment (`sfo-dashboard-feasibility-assessment.md`).
> This assessment evaluates `sfo-dashboard-ux-blueprint-v2.2.md`, which evolves
> v2.1 by adding governed cross-dashboard collection signals from the Collection
> Dashboard.

---

## 1. Executive Summary

### Request

Replace the current Sales Force Overview dashboard (SF02,
`/portal/dashboard/field-activity`, rendered by `FieldActivityOverviewView.vue`)
with the UX Blueprint v2.2. v2.2 preserves the five-section decision flow from
v2.1 and adds: (1) a `Coverage → Sales → Collection → Portfolio` domain spine and
a three-dimension effectiveness framework; (2) a small, governed set of
collection signals (Status, Territory, Action Center); (3) a grouped
Action Center (Field Execution / Commercial Risk / Recognition); and (4)
positioning as a child-ready node for a future executive hub.

### Recommendation

**Proceed. No open scoped decisions remain.** The blueprint's sales execution
content is fully feasible against the existing
`/api/dashboard/field-activity/overview` contract. The collection "current-state"
signals (Cash Collected MTD, Recovery vs Billing %, Overdue Exposure,
TopOverdueSalesmen/Customers/Wilayah, Aging>90, Legacy Debt) are all available in
the existing `DashboardCollectionResponse`
(`/api/dashboard/collection`), already modeled in the portal and already loaded by
`dashboardStore.loadCollection()`. Collection *trends* are **resolved and deferred**
(GAP-001 — no time-series source; do not implement in v2.2), and the Territory
**Collections** measure is **resolved and removed** (GAP-002 — no territory-level
collection metric exists in the Collection Dashboard source; Financial Health
shows Overdue Exposure / Overdue Concentration / Aging Risk only). The derived
cross-dataset signals (High Revenue + High Overdue, Recognition Candidates) are
**unblocked** — the canonical join key is `SalesPersonId` (GAP-003A — **CLOSED**;
contract documented in §5) and the governing thresholds are defined by the
**Commercial Signal Rule Catalog v1** (GAP-003B — **CLOSED**; no new KPIs). The
Status time-window mismatch is **resolved by layout amendment** (GAP-005 —
**CLOSED**): collection signals move into a dedicated **"Collection Health (MTD)"**
section, separate from the sales KPI group, so metrics from different reporting
windows never share a KPI group. No metric recalculation and no backend changes are
required. Drill-across is **clarified** (GAP-006 — **CLOSED**): the target is the
existing **Piutang Dashboard** (business/UX label "Collection Dashboard") via
`Navigation.PiutangDashboardRoute`; documentation only, no navigation change.

### Feasibility Result

```text
FEASIBLE
```

**Reason:** All identified gaps are resolved or explicitly deferred. Collection
trends are deferred (GAP-001); the Territory Collections measure is removed
(GAP-002); salesman identity mapping is confirmed via `SalesPersonId` (GAP-003A);
two-API merge uses frontend composition (GAP-004); the Status time-window mismatch
is resolved with a dedicated "Collection Health (MTD)" section (GAP-005); the
drill-across target is clarified (GAP-006); Active Salesmen uses the source
`TeamKpis.ActiveSalesmenCount` (GAP-007); and the cross-dashboard business rules are
defined by the Commercial Signal Rule Catalog v1 (GAP-003B). The blueprint is
implementable today with existing data and no new data model; the derived signals
are frontend classifications over existing KPIs.

---

## 2. Request Understanding

### Requested Capability

Evolve the Sales Force Overview into a "Commercial Field Force Effectiveness"
dashboard that answers *selling* and *collecting* questions together, while
remaining a lean operational overview (not a detailed performance report and not
a Collection Dashboard).

### Business Objective

Give owners/sales managers a single-page, 30-second read of field execution,
sales conversion, cash recovery, and portfolio health, so a salesman is judged on
both revenue and collection outcomes without blending them into one composite
score.

### Expected Outcome

A dashboard that keeps the proven 5-section decision flow, surfaces a governed
set of collection signals (each with a sales-manager action), and stays
child-ready for a future `Executive Commercial Health` hub — without redesigning
or duplicating the Collection Dashboard.

---

## 3. Current State Analysis

### Existing Business Flow

The dashboard is date-scoped (single `visitDate`). Today it answers only
execution/sales questions. Collection recovery is owned by the separate
Collection Dashboard (`/dashboard/collection`) and Piutang Dashboard
(`/dashboard/piutang`). The salesman is the shared entity accountable for both
outcomes (per `docs/foundation/PRODUCT.md`).

### Existing Components

| Component | Path | Role |
| --- | --- | --- |
| `FieldActivityOverviewView.vue` | `src/views/dashboard/` | SF02 page; single-source loader of `getFieldActivityOverview(visitDate)` |
| `FieldActivityTeamKpiStrip` | `components/field-activity/` | Team headline KPIs |
| `FieldActivitySalesmanTable` | `components/field-activity/` | Per-salesman table |
| `FieldActivityComparisonChart` | `components/field-activity/` | Ranked horizontal bars |
| `FieldActivityRankingGrid` | `components/field-activity/` | Pre-computed leaderboards |
| `FieldActivityTeamTrendChart` | `components/field-activity/` | 7/30-day trends |
| `FieldActivityWilayahChart` | `components/field-activity/` | Region breakdown |
| `CollectionDashboardView.vue` + `CollectionAttentionCardGroup`, `Top10RankingTable`, etc. | `views/dashboard/`, `components/dashboard/` | Collection system of record |
| `dashboardStore.loadCollection()` | `stores/dashboardStore.ts` | Already loads `DashboardCollectionResponse` into `dashboard.collection` |
| `navigateToInvestigation` / `navigateToDashboard` | `services/navigateToInvestigation.ts` | Drill-across affordance |

### Existing APIs

| API | Response model | Notes |
| --- | --- | --- |
| `GET /api/dashboard/field-activity/overview?visitDate=` | `FieldActivityOverviewResponse` | `TeamKpis` (11 fields), `Salesmen[]` (18 fields incl. `OmzetAmount`, `OrdersCount`, `ActualVisits`, `WilayahName`, `HasEmail`, `StatusCode`), `Rankings` (8 lists), `Trends.Last7Days/Last30Days`, `WilayahBreakdown[]` (WilayahName, ActualVisits) |
| `GET /api/dashboard/collection` | `DashboardCollectionResponse` | `AttentionCards`, `RecoverySummary`, `AgingRiskSummary[]`, `AttentionList[]`, `TopOverdueCustomers[]`, `TopOverdueSalesmen[]`, `TopOverdueWilayah[]`, `Navigation` |

### Existing Database

Field-activity snapshot tables (`BTRPD_FieldActivityKpi`, `_Salesman`, `_Trend`)
feed the overview. Collection snapshot tables (`BTRPD_CollectionKpi`,
`BTRPD_CollectionAging`, `BTRPD_CollectionAttention`, `BTRPD_CollectionTopOverdue*`)
feed the Collection Dashboard (see `src/j05-btr-distrib/docs/ops/btr-portal-deploy.md`).

### Existing Integrations

Two separate read endpoints. No cross-dashboard merge today. Drill-across exists
in one direction (Collection → Investigation/Report) via `Investigation` metadata.

---

## 4. Impact Analysis

### Backend Impact

- **None required.** No merged backend endpoint is created (GAP-004 — CLOSED:
  frontend composition), and KPI ownership stays in the source dashboards.
  Backend work is limited at most to exposing business-date/freshness metadata
  (GAP-004 requirement); derived cross-dataset signals are composed on the
  frontend. Collection trends (GAP-001) and the Territory Collections measure
  (GAP-002) are out of scope.

### Database Impact

- None for the feasible portion. Collection trends are out of scope (GAP-001
  deferred), so no collection trend/snapshot table is added.

### Frontend Impact

- **`FieldActivityOverviewView.vue`** — recompose into the 5-section hierarchy
  (§3.1); this is the primary change.
- New components: a dedicated **"Collection Health (MTD)"** section (GAP-005),
  conditional alert chip, grouped Action Center (3 groups), Recognition strip,
  Territory measure-group toggle.
- **Layout amendment (GAP-005):** collection KPIs are **not** placed in the sales
  Status KPI group. They live in a separate "Collection Health (MTD)" section so
  different reporting windows are never mixed in one KPI group.
- Modified components: `FieldActivityTeamKpiStrip` (sales KPIs only),
  `FieldActivityWilayahChart` (add Financial Health group + overdue distribution),
  `FieldActivityRankingGrid` (replaced/simplified by grouped Action Center).
  `FieldActivityTeamTrendChart` is unchanged (collection trends deferred, GAP-001).
- Collection data consumption: reuse `dashboardStore.collection`
  (`loadCollection()`) — a second API call on the same page.

### Integration Impact

- **Frontend composition of two existing APIs** (GAP-004 — CLOSED): the SFO page
  calls `field-activity/overview` + `dashboard/collection` (via
  `dashboardStore.loadCollection()`). No merged endpoint; KPI ownership stays in
  each source dashboard.
- **Metadata requirement:** both APIs must expose business date + freshness
  metadata. Current state — field-activity overview exposes business date
  (`VisitDate`) and freshness (`DataSource`, `GeneratedAt`, `QueriedAt`);
  collection exposes freshness (`IsDataFresh`, `GeneratedAt`) and availability
  (`IsAvailable`), but has **no dedicated business-date field** (the app-wide
  `presentationStore.businessReferenceDate` / `presentationApi.BusinessDate` is
  available). Confirm whether to add a collection business date or rely on the
  global business date.
- **Graceful degradation:** collection widgets must render an empty/unavailable
  state when `DashboardCollectionResponse.IsAvailable` is false or the call fails,
  without affecting the sales sections.
- **Drill-across target (GAP-006 — CLOSED):** drill-across goes to the existing
  **Piutang Dashboard** (business/UX label: "Collection Dashboard") via
  `Navigation.PiutangDashboardRoute` (`/dashboard/piutang`). Documentation
  clarification only — no backend or navigation change.

### Security Impact

- None. Read-only dashboards; no permission/role changes. Collection data is
  already exposed to the same authenticated portal users.

---

## 5. Gap Analysis

| Gap ID | Type | Description | Status |
| --- | --- | --- | --- |
| GAP-001 | Data | **Collection trends (§E)** — Cash Collected, Recovery vs Billing %, Overdue Exposure 7/30-day trends have **no time-series source**. `DashboardCollectionResponse` is a current-state snapshot only (no history). Unlike `FieldActivityOverviewResponse.Trends`, there is no collection trend array. | **RESOLVED — deferred.** Do not implement Collection Trends in v2.2; remove the collection trend widgets. Collection remains current-state KPIs + cross-dashboard signals + Action Center signals. Revisit only when a historical collection snapshot dataset exists. |
| GAP-002 | Data | **Territory "Collections" measure (§C.2 Financial Health)** — no "cash collected per Wilayah" field exists. `TopOverdueWilayah[]` carries overdue `Amount`/`PercentOfTotal` only, not collections. The blueprint's "Collection signal aggregated by `WilayahName`" is not backed by a collection-amount-per-wilayah field. | **RESOLVED — removed.** Approved amendment: Territory Financial Health shows **Overdue Exposure / Overdue Concentration / Aging Risk only**. Remove "Collection Amount by Territory" until a territory-level collection metric exists in the Collection Dashboard source. |
| GAP-003A | Technical + Data | **Salesman identity mapping across dashboards** — the join key between SFO `Salesmen[]` and Collection `TopOverdueSalesmen[]` was unverified. | **RESOLVED — CLOSED.** Collection domain exposes `SalesPersonId` (`BTR_Tagihan.SalesPersonId`); the collection snapshot `BTRPD_CollectionTopOverdueSalesman` carries `SalesPersonId`, and the API surfaces it as `Investigation.EntityId`. Canonical identifier contract documented below. |
| GAP-003B | Business | **Cross-dashboard business rules** — thresholds for **High Revenue**, **High Overdue**, **Healthy Portfolio**, and **Recognition Candidate** are not defined. | **RESOLVED — CLOSED.** Define a lightweight **Commercial Signal Rule Catalog** (no new KPIs); signals are business classifications that consume existing KPIs. Rule Catalog v1 uses percentile-based thresholds (Top/Bottom 25%). KPI ownership unchanged. See §6. |
| GAP-004 | Technical | **Two-API merge strategy undecided** — the SFO page will fetch field-activity and collection from separate endpoints unless the backend is extended to merge them. Impacts loading, error/partial states, and KPI consistency. | **RESOLVED — CLOSED.** Use **frontend composition**; **do not** create a merged backend endpoint. Keep KPI ownership in the source dashboards. Require both APIs to expose business date + freshness metadata. Allow collection widgets to **degrade gracefully** when collection data is unavailable. |
| GAP-005 | UX | **Time-window mismatch in Status** — sales KPIs (Revenue, Visit Execution) are single-`visitDate` flow metrics; collection KPIs (Cash Collected MTD, Recovery vs Billing %) are month-to-date stock metrics. Placing them side-by-side as "is cash keeping pace with sales" (§A) compares different windows and can mislead. | **RESOLVED — CLOSED.** Collection signals remain in the dashboard but move into a dedicated **"Collection Health (MTD)"** section. Sales KPIs and collection KPIs do **not** appear in the same KPI group (different reporting windows). No metric recalculation required; no backend changes required. |
| GAP-006 | Technical | **Drill-across route naming** — §5.3 says drill-across to the "Collection Dashboard" but cites `Navigation.PiutangDashboardRoute` (which points to `/dashboard/piutang`, not `/dashboard/collection`). Which dashboard is the drill-across target must be confirmed. | **RESOLVED — CLOSED.** "Collection Dashboard" is a business/UX label; the implementation target is the existing **Piutang Dashboard**. All drill-across references in v2.2 are updated to **"Collection Dashboard (Piutang Dashboard)"**, route `Navigation.PiutangDashboardRoute`. No backend change; no navigation change; documentation clarification only. |
| GAP-007 | Business | **"Active Salesmen" denominator undefined** — §A redefines Active Salesmen as `Salesmen[] with ActualVisits > 0` while the existing `TeamKpis.ActiveSalesmenCount` has its own (email-eligible) semantics. The new "per Active Salesman" subtitles (Revenue, Cash Collected) depend on this denominator being agreed. | **RESOLVED — CLOSED.** Active Salesmen uses the existing **`TeamKpis.ActiveSalesmenCount`** definition. The dashboard shall **not** redefine it as `ActualVisits > 0`; all KPI calculations referencing Active Salesmen must use the source API value. **Optional:** remove the "per Active Salesman" derived metrics from v2.2 to avoid denominator ambiguity. |
| GAP-008 | Business | **Recognition thresholds undefined** — "high sales AND healthy collection" has no quantitative rule; this is a new business decision, not yet specified. | **MERGED into GAP-003B** — "Recognition Candidate" thresholds are part of the KPI rule catalog. |

### Canonical Identifier Contract (GAP-003A) — documented

All salesperson-linked data and dashboards join on the canonical
**`SalesPersonId`**.

| Layer | Field | Type | Notes |
| --- | --- | --- | --- |
| Master | `BTR_SalesPerson.SalesPersonId` | `VARCHAR(5)` | Primary key; canonical source of identity |
| Sales | `BTR_Faktur.SalesPersonId` | `VARCHAR(5)` | Invoice attribution |
| Collection (finance) | `BTR_Tagihan.SalesPersonId` | `VARCHAR(5)` | Confirms collection-domain salesman linkage |
| Collection snapshot | `BTRPD_CollectionTopOverdueSalesman.SalesPersonId` | `VARCHAR(13)` | + `SalesPersonCode`, `SalesPersonName` |
| Field-activity snapshot | `BTRPD_FieldActivitySalesman.SalesPersonId` | `VARCHAR(26)` | + `SalesPersonCode`, `SalesPersonName` |

**API exposure of the identifier**

- SFO (`FieldActivityOverviewResponse.Salesmen[]`): `SalesPersonId`,
  `SalesPersonCode`, `SalesPersonName`.
- Collection (`DashboardCollectionResponse.TopOverdueSalesmen[]`): `EntityCode`
  (= `SalesPersonCode`), `EntityName` (= `SalesPersonName`), and
  `Investigation.EntityId` (= canonical `SalesPersonId`) — see
  `DashboardCollectionDal.cs:124`.

**Join rule**

Match SFO `Salesmen[].SalesPersonId` to Collection
`TopOverdueSalesmen[].Investigation.EntityId`. `EntityCode`/`EntityName`
(`SalesPersonCode`/`SalesPersonName`) are fallback display attributes only.
Salesman **names must never be the join key**.

**Implementation note:** the canonical ID is present end-to-end but is projected
onto the collection API only via `Investigation.EntityId`. If a first-class
`SalesPersonId` field is preferred on `DashboardCollectionRankingRow`, surface it
explicitly at the API boundary — no new data or data model is required.

---

## 6. Solution Options

### GAP-001 — Collection trends — RESOLVED

**Decision (accepted): Do not implement Collection Trends in v2.2.** Remove the
collection trend widgets from the dashboard. Collection is kept as:

- Current-State KPIs (Status: Cash Collected MTD, Recovery vs Billing %, Overdue alert),
- Cross-Dashboard Signals (Territory overdue, drill-across),
- Action Center Signals (Needs Collection Action / Credit Review / Escalation).

Trend analysis is deferred until a historical collection snapshot dataset exists.
This closes GAP-001 and removes the "collection trends" workstream (formerly
Option B below) from the plan.

- **Option A — Backend collection trend aggregator.** (Not selected.)
- **Option B — Defer/omit collection trends.** **SELECTED** — ship v2.2 without
  §E collection trends; keep sales trends only. Preserves the lean overview and
  avoids shipping trends on invented data.

### GAP-002 — Territory "Collections per Wilayah" — RESOLVED

**Decision (accepted): Approve v2.2 with amendment.** Territory Financial Health
shows **Overdue Exposure / Overdue Concentration / Aging Risk only**. Remove
"Collection Amount by Territory" until a territory-level collection metric exists
in the Collection Dashboard source. All three retained measures are fully
available today (`TopOverdueWilayah[]` for Overdue Exposure + Concentration;
`AttentionCards.AgingOver90Exposure` / `AgingRiskSummary[]` for Aging Risk).

- **Option A — Reduce Financial Health group to overdue-family measures.** **SELECTED.**
  Zero backend work; still answers "which territory is over-exposed and how
  concentrated."
- **Option B — Derive collections-per-wilayah** from `AttentionList[]` or extend
  the collection aggregator. (Not selected — no clean territory-level collection
  metric exists.)

### GAP-003A — Salesman identity mapping — RESOLVED (CLOSED)

Collection domain contains `BTR_Tagihan.SalesPersonId`; cross-dashboard salesman
linkage is supported. The canonical identifier contract is documented in §5.
**No backend change required** — join on `SalesPersonId` (SFO
`Salesmen[].SalesPersonId` ↔ Collection `TopOverdueSalesmen[].Investigation.EntityId`).

### GAP-003B — Cross-dashboard business rules — RESOLVED (CLOSED)

**Decision (accepted): define a lightweight Commercial Signal Rule Catalog — no new
KPIs.**

- The dashboard shall **not introduce new KPIs** for High Revenue, High Overdue,
  Healthy Portfolio, or Recognition Candidate. These are **business classification
  signals**, not KPIs; the underlying KPIs already exist and remain owned by their
  respective dashboards.
- The dashboard **consumes the classification result**, not a new KPI. Cross-dashboard
  signals are derived classifications that combine existing KPIs and **do not require
  KPI registration**.
- Implementation: **frontend merge** (Option B) — combine the two APIs on the
  confirmed `SalesPersonId` join key and apply the rule catalog. Backend merge
  (Option A) remains ruled out by GAP-004.

**Rule Catalog v1**

| Signal | Definition |
| --- | --- |
| High Revenue | Salesman Revenue in Top 25% of active salesmen |
| High Overdue | Overdue Exposure in Top 25% of salesmen |
| Healthy Portfolio | Overdue Exposure in Bottom 25% of salesmen |
| Recognition Candidate | High Revenue AND High Effective Call Rate AND Healthy Portfolio |
| Needs Credit Review | High Revenue AND High Overdue |
| Needs Collection Action | High Overdue |

**KPI ownership:** Sales KPIs (Revenue, Orders, Effective Call Rate, Visit
Execution) remain owned by the Sales Force Dashboard; collection KPIs (Overdue
Exposure, Recovery vs Billing %, Cash Collected, Aging Risk) remain owned by the
Collection Dashboard. The SFO shall **not** redefine or duplicate them.

**Future enhancement:** thresholds may later evolve to percentile-based,
territory-specific, role-specific, or dynamic benchmarking rules; **v2.2 uses the
simple percentile-based catalog above.**

### GAP-004 — Two-API merge strategy — RESOLVED (CLOSED)

**Decision (accepted): Frontend composition.**

- Use **frontend composition** of the two existing APIs
  (`field-activity/overview` + `dashboard/collection`).
- **Do not** create a merged backend endpoint.
- **Keep KPI ownership** in the source dashboards (no re-computation in SFO; §5.4
  governance preserved).
- **Require both APIs to expose business date and freshness metadata** (see the
  metadata note in §4 — the collection API currently exposes freshness but not a
  dedicated business date).
- **Graceful degradation:** collection widgets must render empty/unavailable
  states when collection data is unavailable, without breaking the sales sections.

This also constrains GAP-003B: the derived cross-dataset signals must be composed
on the frontend (GAP-003B Option B), not via a merged backend projection.

### GAP-005 — Status time-window mismatch — RESOLVED (CLOSED)

**Decision (accepted): dedicated "Collection Health (MTD)" section.**

- Collection signals remain in the dashboard but are moved into a dedicated
  **"Collection Health (MTD)"** section.
- Sales KPIs and collection KPIs will **not** appear in the same KPI group, because
  they represent different reporting windows (single `visitDate` flow vs MTD stock).
- **No metric recalculation required; no backend changes required.**

### GAP-006 — Drill-across route naming — RESOLVED (CLOSED)

**Decision (accepted): documentation clarification only.**

- "Collection Dashboard" is a **business/UX label**; the actual implementation
  target is the existing **Piutang Dashboard**.
- All drill-across navigation references in UX Blueprint v2.2 shall be updated to
  **"Collection Dashboard (Piutang Dashboard)"**, route
  `Navigation.PiutangDashboardRoute` (`/dashboard/piutang`).
- **No backend change. No navigation change.** Documentation clarification only.

### GAP-007 — Active Salesmen denominator — RESOLVED (CLOSED)

**Decision (accepted): use the source API value.**

- Active Salesmen uses the existing **`TeamKpis.ActiveSalesmenCount`** definition.
- The dashboard shall **not** redefine Active Salesmen as `ActualVisits > 0`.
- All KPI calculations referencing Active Salesmen must use the **source API
  value** (no frontend re-derivation of the denominator).
- **Optional:** remove the "per Active Salesman" derived metrics from v2.2
  ("Revenue per Active Salesman", "Cash Collected per Active Salesman") to avoid
  denominator ambiguity.

---

## 7. Recommended Approach

Implement v2.2 as a **frontend-first evolution of `FieldActivityOverviewView.vue`**
in two tranches. The dashboard **composes two existing APIs on the frontend**
(GAP-004 — CLOSED); no merged backend endpoint is created and KPI ownership stays
in each source dashboard.

1. **Tranche 1 (fully feasible now):** recompose into the 5-section hierarchy;
   add a dedicated **"Collection Health (MTD)"** section (GAP-005 — collection KPIs
   kept out of the sales KPI group) with the conditional overdue alert chip (from
   `dashboardStore.collection.AttentionCards`); extend Territory with the
   Financial Health group showing **Overdue Exposure / Overdue Concentration /
   Aging Risk only** (from `TopOverdueWilayah[]` and
   `AttentionCards.AgingOver90Exposure` / `AgingRiskSummary[]`) plus C.3 Overdue
   Distribution; add the grouped Action Center limited to Field Execution +
   "Needs Collection Action" / "Needs Escalation" (from `TopOverdueSalesmen[]`,
   `AgingOver90Exposure`, `LegacyDebtCount`); add drill-across.
2. **Tranche 2:** derived cross-dashboard signals using the **Commercial Signal
   Rule Catalog v1** (GAP-003B) on the `SalesPersonId` join (GAP-003A) — High
   Revenue, High Overdue, Healthy Portfolio, Recognition Candidate, Needs Credit
   Review, Needs Collection Action. Signals are **classifications**, not KPIs; no
   KPI re-registration.

**Explicitly out of scope for v2.2:**

- **GAP-001:** collection trend widgets (Cash Collected Trend, Recovery vs
  Billing % Trend, Overdue Exposure Trend) — deferred until a historical
  collection snapshot dataset exists.
- **GAP-002:** Territory "Collection Amount by Territory" — removed until a
  territory-level collection metric exists in the Collection Dashboard source.

The collection values must be read **as-is** from `DashboardCollectionResponse`
(never re-computed) to satisfy the §5.4 governance precondition. No new data model
is required for Tranche 1.

Metadata and resilience requirements (GAP-004): both APIs must expose business date
+ freshness metadata; collection widgets must degrade gracefully (empty/unavailable
state) when collection data is unavailable, without affecting the sales sections.

Active Salesmen (GAP-007): all references use the source `TeamKpis.ActiveSalesmenCount`;
the dashboard must **not** redefine it as `ActualVisits > 0`. The "per Active
Salesman" derived metrics (Revenue / Cash Collected per Active Salesman) are
**optional** and may be removed from v2.2 to avoid denominator ambiguity.

---

## 8. Risks

| Risk | Impact | Probability | Mitigation |
| --- | --- | --- | --- |
| Metric time-window mismatch (daily sales vs MTD collection) misleads managers | Medium | High | **Resolved (GAP-005):** collection KPIs moved to a dedicated "Collection Health (MTD)" section; sales and collection never share a KPI group. |
| KPI drift/double-counting between SFO and Collection Dashboard | High | Medium | Enforce §5.4 governance; read collection values from the same `DashboardCollectionResponse` fields, never re-derive. |
| Active Salesmen denominator ambiguity | Medium | Medium | **Resolved (GAP-007):** use the source `TeamKpis.ActiveSalesmenCount`; do not redefine as `ActualVisits > 0`; the "per Active Salesman" derived metrics are optional and may be removed. |
| Cross-dataset derived signals use undefined business rules | High | Medium | **Resolved (GAP-003B):** Commercial Signal Rule Catalog v1 (percentile-based) defines all thresholds; signals are classifications, not KPIs. |
| Dashboard bloat / loss of lean 10-second read | Medium | Medium | Keep Overdue Exposure as a conditional alert chip (not a permanent card); keep Recognition as a light strip. |
| Collection API unavailable / stale while sales data loads | Medium | Medium | GAP-004: require business-date + freshness metadata on both APIs; degrade collection widgets gracefully (empty/unavailable state) without breaking sales sections. |
| Drill-across targets wrong dashboard (Piutang vs Collection) | Low | Medium | **Resolved (GAP-006):** the target is the Piutang Dashboard (labelled "Collection Dashboard"); route `Navigation.PiutangDashboardRoute`. Documentation clarification only. |

---

## 9. Open Questions

### Business Questions

> **Resolved:** cross-dashboard thresholds are defined by the **Commercial Signal
> Rule Catalog v1** (GAP-003B). No open business question remains.

### Technical Questions

> **Resolved (no open technical questions):**
> - Identity mapping — canonical join key `SalesPersonId` (GAP-003A).
> - Two-API merge — frontend composition, no merged backend endpoint (GAP-004).
> - Status time-window alignment — dedicated "Collection Health (MTD)" section (GAP-005).
> - Business rules — Commercial Signal Rule Catalog v1 (GAP-003B).
> - Active Salesmen denominator — source `TeamKpis.ActiveSalesmenCount` (GAP-007).

### Operational Questions

> **Resolved:** drill-across targets the existing **Piutang Dashboard** (business/UX
> label "Collection Dashboard") via `Navigation.PiutangDashboardRoute` (GAP-006).
> No operational question remains.

### Blocking Questions

- **None.** All gaps (GAP-001 through GAP-008) are resolved or explicitly deferred.
  No open question blocks planning or implementation.
- **Resolved (all gaps):** GAP-001 (collection trends deferred), GAP-002 (Territory
  collections removed), GAP-003A (identity — `SalesPersonId`), GAP-003B (Commercial
  Signal Rule Catalog v1), GAP-004 (frontend composition), GAP-005 (dedicated
  "Collection Health (MTD)" section), GAP-006 (drill-across — Piutang Dashboard),
  GAP-007 (Active Salesmen — source `TeamKpis.ActiveSalesmenCount`), GAP-008
  (merged into GAP-003B).

---

## 10. Implementation Impact Inventory

### Backend

| Component | Change | Required |
| --- | --- | --- |
| (none for Tranche 1) | — | — |
| ~~Commercial overview merge endpoint~~ | ~~New merged projection~~ | **Not created (GAP-004 — frontend composition)** |
| Collection API business-date field | Expose business date (or confirm reliance on global `presentationApi.BusinessDate`) | Only if a dedicated business date is required (GAP-004 requirement) |
| `DashboardCollectionRankingRow` | Optionally surface first-class `SalesPersonId` (already available via `Investigation.EntityId`) | Optional (GAP-003A contract documented) |
| ~~Collection trend aggregator~~ | ~~Add trend series~~ | **Out of scope (GAP-001 deferred)** |
| ~~Territory collections rollup~~ | ~~Add collections-by-wilayah~~ | **Out of scope (GAP-002 removed)** |

### Database

| Object | Change | Required |
| --- | --- | --- |
| `BTRPD_Collection*` | No change | No |
| ~~New collection trend table~~ | ~~Add (mirror `BTRPD_FieldActivityTrend`)~~ | **Out of scope (GAP-001 deferred)** |

### Frontend

| Component | Change | Required |
| --- | --- | --- |
| `FieldActivityOverviewView.vue` | Recompose into 5-section hierarchy; call `dashboardStore.loadCollection()` | Yes |
| `FieldActivityTeamKpiStrip.vue` | Sales KPIs only (collection kept out per GAP-005) | Yes |
| New: "Collection Health (MTD)" section + conditional alert chip | Create; collection KPIs in their own section | Yes (GAP-005) |
| New: Collection widget unavailable/empty state | Graceful degradation when collection data unavailable | Yes (GAP-004) |
| New: Commercial Signal Rule Catalog classifier (frontend) | Classify existing KPIs into signals (percentile-based v1); no KPI registration | Yes (GAP-003B) |
| New: Grouped Action Center (Field Execution / Commercial Risk / Recognition) | Create | Yes |
| `FieldActivityWilayahChart.vue` | Add Financial Health group (overdue) + C.3 distribution | Yes |
| `FieldActivityTeamTrendChart.vue` | No change (sales trends only; collection trends deferred) | No |
| `FieldActivityRankingGrid.vue` | Simplify into grouped Action Center | Yes |

### Integration

| Integration | Change | Required |
| --- | --- | --- |
| `dashboardStore.loadCollection()` reuse | Consume on SFO page | Yes |
| Frontend composition of the two APIs | SFO calls both; no merged endpoint (GAP-004) | Yes |
| Business-date + freshness metadata | Surface/confirm on both APIs (GAP-004) | Yes |
| Graceful degradation | Collection widgets render empty/unavailable state (GAP-004) | Yes |
| Drill-across navigation | Wire collection signals to the Piutang Dashboard via `Navigation.PiutangDashboardRoute` (GAP-006) | Yes |

### Security

No security model changes.

---

## 11. Planning Readiness

### Status

```text
READY
```

(All gaps are resolved or explicitly deferred. Planning may proceed for the full
v2.2 scope: collection trends (GAP-001) and the Territory Collections measure
(GAP-002) are out of scope; salesman identity mapping (GAP-003A — `SalesPersonId`),
the Commercial Signal Rule Catalog (GAP-003B), two-API frontend composition
(GAP-004), the dedicated "Collection Health (MTD)" section (GAP-005), the
drill-across target (GAP-006 — Piutang Dashboard), and the Active Salesmen
denominator (GAP-007 — source `TeamKpis.ActiveSalesmenCount`) are all resolved.)

### Blocking Issues

```text
None.
```

### Planner Guidance

**Implementation scope:** frontend recomposition of `FieldActivityOverviewView.vue`
plus ~4 new components and ~3 modified components; no backend change.

**Major dependencies:** `DashboardCollectionResponse` (already modeled + loaded via
`dashboardStore`); drill-across mechanism (`navigateToInvestigation` /
`navigateToDashboard`) already present; the `SalesPersonId` join key.

**Sequencing concerns:** compose the two APIs on the frontend per GAP-004 (no
merged endpoint); drill-across targets the Piutang Dashboard per GAP-006
(`Navigation.PiutangDashboardRoute`); apply the Commercial Signal Rule Catalog v1
(GAP-003B) for the derived classifications, built on the `SalesPersonId` join
(GAP-003A). Collection trends (GAP-001) and the Territory Collections measure
(GAP-002) are removed from scope.

**Review concerns:** verify collection KPIs live only in the dedicated "Collection
Health (MTD)" section and never share a KPI group with sales KPIs (GAP-005); verify
collection numbers are read as-is (governance §5.4) with no frontend
re-derivation; verify Active Salesmen uses the source `TeamKpis.ActiveSalesmenCount`
(GAP-007); verify the Rule Catalog signals are treated as classifications (not
KPIs) and that percentile rules behave sensibly on small salesman populations;
verify the Action Center groups map each signal to a sales-manager action (not a
finance action).

---

*Assessment completed: 2026-09-14*
*Source: `sfo-dashboard-ux-blueprint-v2.2.md`*
*Inputs verified: `FieldActivityOverviewView.vue`, `models/fieldActivity.ts`,
`models/dashboard.ts`, `CollectionDashboardView.vue`, `dashboardStore.ts`,
`collection-data-dashboard-scheme.json`, router, `navigateToInvestigation.ts`.*
