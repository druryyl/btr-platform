# Implementation Plan: M24 — Dashboard Drill Down & Investigation Framework

## Document Status

| Field | Value |
| --- | --- |
| Initiative | M24 Dashboard Drill Down — **Why is this happening? Show me the evidence.** |
| Internal purpose | **Investigation and navigation framework** — unifies progressive disclosure from KPI / Alert / Ranking / Attention signals to report evidence across M16–M23 |
| Authoritative requirements | `docs/work/btr-portal/M24 Dashboard Drilldown - Analysis.md` — **Section 19 (Product Owner Decisions)** |
| Alert / signal catalog | `docs/features/btr-portal/ALERT-REGISTRY.md` — drill-down destinations; extended by Investigation Registry (Section 5.3) |
| Related features | `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-operational.md`, `docs/features/btr-portal/btr-portal-architecture.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md` |
| Reference pattern | M17–M22 attention lists (`navigateToReport` + `ReportRoute` on rows); M23 Alert Center dual actions |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |
| Status | **Ready for implementation** |
| Product decisions | **Confirmed** — analysis Section 19, 2026-06-09 |
| Open questions | **None** — all PO decisions resolved; architect-resolved technical rules in Section 2.4–2.6 |

---

## 1. Goal

Deliver a **horizontal investigation framework** that makes every clickable KPI, Alert, Ranking, and Attention item across M16–M23 answer:

1. **Why am I seeing this?** — signal label + source surfaced on report (breadcrumb)
2. **Show me the evidence.** — report-first navigation with correct filters and entity IDs
3. **What should I inspect next?** — optional dashboard context, multi-step metadata, Desktop guidance text

**Primary outcomes:**

- Formal **Investigation Metadata Contract** shared by dashboards, Alert Center, and reports (Section 5)
- **Purchasing Report `?q=` hydration** — prerequisite bug fix (Q9)
- **Piutang semantic alignment** — `allOpenBalances` mode when drilling from all-time receivable sources (Q5)
- **Executive Critical Exposure Top 5** rows become clickable with same destinations as domain dashboards (Q2)
- **Legacy dashboard retrofit** — M11 Sales, M14 Piutang, M15 Inventory Top 10 ranking drill-down (Q1)
- **Investigation breadcrumb** on all four report pages (Q4)
- **Stable entity IDs** in drill-down query params (Q6)
- **Qualified Backlog** → Purchasing Report auto-filter `PostingStok = BELUM` (Q7)
- **`Investigate`** as standardized action label everywhere (Q17)
- **Report-first** default for entity alerts; **View Dashboard** secondary (Q12)

**Explicitly out of scope (PO confirmed):**

- New KPIs, snapshot tables, aggregators, or `SignalKey` alert definitions (Q24, Q25)
- Clickable charts (Q3)
- Desktop deep links (Q15)
- Export, server-side search, Collection Report, transaction detail pages, Kartu Stok in portal (Q23)
- Wilayah filter on Piutang Report — Wilayah Hotspot stops at Collection Dashboard (Q10)
- Company-level alert report buttons (Q11)
- Investigation telemetry (Q22)
- Sales Report footer totals (Q8)
- Alert deduplication changes (M23 ownership preserved)

---

## 2. Authoritative Product Decisions

Source: analysis Section 19. Do not re-decide these rules during implementation.

### 2.1 Scope tiers

| Tier | Items |
| ---- | ----- |
| **Mandatory** | Investigation Metadata Contract; Purchasing `?q=`; Executive Top 5; Legacy M11/M14/M15 rankings; breadcrumb; entity IDs; Piutang all-open alignment; Qualified Backlog BELUM filter; `Investigate` label; Report-first entity alerts |
| **Nice to have** (if cheap) | Signal context banner beyond breadcrumb; Desktop next-step text; guided multi-step investigation UI (metadata only) |
| **Deferred** | Charts; Desktop links; export; server search; new reports; transaction pages |

### 2.2 Investigation depth model (portal)

| Depth | Surface | M24 role |
| ----- | ------- | -------- |
| 1 — Signal | KPI card, Alert row, Attention / Ranking row | Emit investigation metadata |
| 2 — Context | Domain dashboard | Optional via **View Dashboard** |
| 3 — Tabular evidence | Four reports | Primary terminus — breadcrumb + filters |
| 4 — Transaction audit | BTR Desktop | Text guidance only — no deep links |

### 2.3 Entity navigation defaults (Q10–Q12)

| Entity type | Primary action | Secondary | Report |
| ----------- | -------------- | --------- | ------ |
| Customer, Salesman, Warehouse, Principal, Item | **Investigate** → Report | **View Dashboard** | Per signal routing |
| Company, System | **View Dashboard** only | — | None |
| Wilayah | **View Dashboard** → Collection | — | None |

### 2.4 Architect-resolved rules

Analysis delegates contract shape, query encoding, Piutang all-open mechanics, and executive/legacy routing to the architect.

#### 2.4.1 Investigation Metadata Contract (canonical shape)

Shared TypeScript interface (`models/investigation.ts`) and C# DTO (`InvestigationMetadata.cs` in `btr.application/ReportingContext/Shared/`):

```csharp
public class InvestigationMetadata
{
    public string SignalKey { get; set; }
    public string SignalLabel { get; set; }
    public string EntityType { get; set; }
    public string EntityId { get; set; }
    public string EntityName { get; set; }
    public string DashboardRoute { get; set; }
    public string ReportRoute { get; set; }
    public InvestigationSuggestedQuery SuggestedQuery { get; set; }
    public IList<InvestigationStep> InvestigationSteps { get; set; }
    public string DesktopNextStep { get; set; }
}

public class InvestigationSuggestedQuery
{
    public string FreeText { get; set; }           // display name → ?q=
    public string CustomerId { get; set; }
    public string SalesmanId { get; set; }
    public string BrgId { get; set; }
    public string WarehouseId { get; set; }
    public string SupplierId { get; set; }
    public string PeriodMode { get; set; }         // "currentMonth" | "allOpenBalances"
    public string PostingFilter { get; set; }      // "BELUM" for Qualified Backlog
}

public class InvestigationStep
{
    public int Order { get; set; }
    public string Label { get; set; }
    public string ReportRoute { get; set; }
    public string DashboardRoute { get; set; }
    public InvestigationSuggestedQuery SuggestedQuery { get; set; }
}
```

**Producer rule:** Backend attention rows, executive exposure rows, and legacy ranking rows expose `Investigation` (nested `InvestigationMetadata`). Producers set all fields except `SourceDashboard` — the **consumer sets `SourceDashboard` at click time** in the frontend navigation service.

**Future milestones (Q21):** Any new clickable dashboard surface must populate `Investigation` using the same contract.

#### 2.4.2 URL query string contract

Preserve backward-compatible `?q=` while adding structured params. Single navigation primitive replaces `navigateToReport`.

| Query param | Purpose | Example |
| ----------- | ------- | ------- |
| `q` | Free-text search pre-fill (display name) | `q=PT%20ABC` |
| `customerId` | Stable customer key (code or id) | `customerId=C001` |
| `salesmanId` | SalesPersonId | `salesmanId=SP12` |
| `brgId` | Item id | `brgId=B0042` |
| `warehouseId` | Warehouse id | `warehouseId=W3` |
| `supplierId` | Principal / supplier id | `supplierId=P9` |
| `periodMode` | Piutang period semantics | `periodMode=allOpenBalances` |
| `posting` | Purchasing posting filter | `posting=BELUM` |
| `signalKey` | Breadcrumb — which rule fired | `signalKey=ChronicOverdue` |
| `signalLabel` | Breadcrumb — human label | `signalLabel=Chronic%20Overdue` |
| `source` | Breadcrumb — originating surface | `source=Alert%20Center` |
| `entityType` | Breadcrumb entity grain | `entityType=Customer` |

**Do not** introduce `?redirect=` on reports. **Do not** use opaque JSON blobs in URLs — flat params aid debugging and bookmarking.

`navigateToInvestigation(router, metadata, sourceDashboard)` in `services/navigateToInvestigation.ts`:

1. Merge `sourceDashboard` into metadata copy
2. Build `query` from `SuggestedQuery` + breadcrumb fields
3. `router.push({ path: ReportRoute, query })`

Reports read params in `onMounted` via shared `applyInvestigationQuery(route, store)` helper.

#### 2.4.3 Piutang all-open balances mode (Q5)

**Problem:** Piutang dashboards use `IPiutangOpenBalanceDal.ListOpenBalances()` (no date filter). Piutang Report defaults to current calendar month on Jatuh Tempo — mismatched evidence.

**Solution:** Add optional `AllOpenBalances` flag to `GetPiutangReportQuery` / API `?allOpenBalances=true` (mirrored in `periodMode=allOpenBalances` on frontend).

When `AllOpenBalances = true`:

- **Skip** `ReportPeriodValidator` day-count constraint
- Load rows via new `IPiutangReportDal.GetAllOpenBalancesReport()` path that queries outstanding piutang **without date predicate** (reuse `PiutangSalesWilayahDal` SQL body; `WHERE aa.Sisa > 1` only — same grain as period report)
- Set response `PeriodFrom` / `PeriodTo` to sentinel labels; UI shows **"All open balances"** instead of month range
- Drill-down from Customer Analytics, Collection Dashboard, Alert Center (piutang-bound signals), M14 Piutang Dashboard, Executive Top Customers sets `periodMode=allOpenBalances`

When `AllOpenBalances = false` (default): **unchanged** current-month behavior.

#### 2.4.4 Qualified Backlog posting filter (Q7)

When `SignalKey = QualifiedBacklog` (or `posting=BELUM` in query):

- Purchasing Report sets `freeText` empty and applies **client filter** `PostingStok === 'BELUM'` via dedicated `posting` query param
- Still pass `q` = principal name when entity-scoped

#### 2.4.5 Entity ID filtering on reports (Q6)

| Report | ID param | Row field for exact match |
| ------ | -------- | ------------------------- |
| Sales | `salesmanId` | Add `SalesPersonId` to `SalesReportRow` if available from DAL; fallback name via `q` |
| Piutang | `customerId` | Add `CustomerCode` to `PiutangReportRow` (already in underlying DTO) |
| Inventory | `brgId`, `warehouseId` | `BrgId`, `WarehouseName` (id preferred when present) |
| Purchasing | `supplierId` | Match `SupplierName` via `q`; add `SupplierId` to row if DAL provides |

Filter precedence in `useReportInvestigationFilter`:

1. If stable ID param present → exact match on ID/code column
2. Else → existing multi-word AND free-text on configured columns

#### 2.4.6 Executive Critical Exposure routing (Q2)

Extend `DashboardExecutiveRiskItem` with nested `Investigation`:

| Executive list | EntityType | ReportRoute | SuggestedQuery |
| -------------- | ---------- | ----------- | -------------- |
| TopCustomers | Customer | `/reports/piutang` | `FreeText=Name`, `CustomerId` from piutang snapshot, `PeriodMode=allOpenBalances` |
| TopCategories | Category | `/reports/inventory` | `FreeText=Name` |
| TopSuppliers | Supplier | `/reports/inventory` | `FreeText=Name` |
| TopPrincipals | Principal | `/reports/purchasing` | `FreeText=Name` |

Synthetic investigation signal keys (not ALERT-REGISTRY alert types):

| SignalKey | SignalLabel |
| --------- | ----------- |
| `ExecutiveTopCustomerExposure` | Top Customer Exposure |
| `ExecutiveTopCategoryExposure` | Top Category Exposure |
| `ExecutiveTopSupplierExposure` | Top Supplier Exposure |
| `ExecutiveTopPrincipalExposure` | Top Principal Exposure |

`DashboardExecutiveComposer.ComposeCriticalExposures` must pass through `CustomerCode` from `DashboardPiutangTopCustomer` — **add `CustomerCode` field** to piutang top-customer DTO and snapshot persistence (additive column; backfill on next refresh).

`DashboardRoute` per list: `/dashboard/customers` or `/dashboard/collection` (customers), `/dashboard/inventory`, `/dashboard/inventory`, `/dashboard/purchasing`.

#### 2.4.7 Legacy dashboard ranking routing (Q1)

| Dashboard | Table | Investigation destination |
| --------- | ----- | ------------------------- |
| M11 Sales `/dashboard/sales` | Top 10 Salesman | Sales Report — `q=SalesPersonName`, `salesmanId` |
| M14 Piutang `/dashboard/piutang` | Top 10 Customers | Piutang Report — `periodMode=allOpenBalances`, `customerId`, `q=CustomerName` |
| M15 Inventory `/dashboard/inventory` | Top 10 Category | Inventory Report — `q=Name` |
| M15 Inventory | Top 10 Supplier | Inventory Report — `q=Name` |

Add `Investigation` to ranking DTOs in application layer; populate in DAL/composer from existing ranking data + `InvestigationRegistry` defaults.

**M11:** Add `SalesPersonId` to `DashboardSalesRankingItem` (aggregator already groups by id — expose on ranking row).

**M14:** Add `CustomerCode` to `DashboardPiutangTopCustomer`.

#### 2.4.8 Attention dashboard standardization (Q17, Q12)

Retrofit existing attention components to:

- Replace arrow-only / row-click with labeled **`Investigate`** button (primary)
- Add **`View Dashboard`** text button where `DashboardRoute` exists (secondary — Alert Center, optional elsewhere)
- Call `navigateToInvestigation` instead of `navigateToReport`

**Alert Center (Q12):** Swap column order — **Investigate** (report) primary; **View Dashboard** secondary. Re-label icons per Q17.

**No changes** to attention list qualification, signal labels, or deduplication logic.

#### 2.4.9 Cross-domain investigation steps (Q13 — nice to have)

`InvestigationRegistry` defines ordered steps for:

| SignalKey | Steps |
| --------- | ----- |
| `CompoundDependency` | 1) Purchasing Report (`q=principal`) → 2) Inventory Report (`q=principal`) → 3) Inventory Risk Dashboard |
| `WarehouseAtRiskConcentration` | 1) Inventory Risk Dashboard → 2) Inventory Report (warehouse) |

Render as simple numbered list in report breadcrumb area when `InvestigationSteps` present — **no workflow engine**, no state machine.

#### 2.4.10 Desktop next-step guidance (Q14, Q16 — nice to have)

`InvestigationRegistry` maps `SignalKey` → `DesktopNextStep` text:

| Signal family | DesktopNextStep example |
| ------------- | ----------------------- |
| Customer overdue / chronic | `Next validation: Piutang Tracker (FT5)` |
| Salesman below target | `Next validation: Sales Omzet Chart (RO2)` |
| Dead / slow stock | `Next validation: Kartu Stok (IF8)` |
| Qualified backlog | `Next validation: Posting Stok (PT2)` |

Display below breadcrumb on report pages when present.

---

## 3. Architecture Overview

### 3.1 Target topology

```text
Dashboard producers (M11–M22 snapshots + M16 composer)
  Attention rows / Rankings / Executive Top 5
    ↓ each clickable row includes InvestigationMetadata
Existing dashboard APIs (extended response shape — additive fields)
    ↓
M23 Alert Center composer (maps producer Investigation + registry defaults)
    ↓
Vue dashboards + Alert Center
  navigateToInvestigation(metadata, sourceDashboard)
    ↓ structured query params
Report views (Sales, Piutang, Inventory, Purchasing)
  applyInvestigationQuery → filters + InvestigationBreadcrumb
    ↓
User continues in BTR Desktop (documentation only)
```

**No new API endpoints.** **No new snapshot domains or workers.** **No DDL required** except optional additive `CustomerCode` on `BTR_PortalDashboardPiutangTopCustomer` (recommended for executive drill-down stability).

### 3.2 Design principles

| Principle | Application |
| --------- | ----------- |
| Navigation only | No new KPI formulas, aggregations, or alert rules |
| Extend, don't replace | `?q=` remains; structured params additive |
| Producer-owned routing | Signal → report rules stay in aggregators / DAL mappers |
| Registry for cross-cutting text | Desktop next-step and multi-step sequences in `InvestigationRegistry` |
| Report-first for entities | Dashboard is context, not mandatory hop |
| Preserve M23 dedup | Alert Center composer unchanged except Investigation field mapping + UI affordance order |

### 3.3 Architecture decisions

| Decision | Choice | Rationale |
| -------- | ------ | ----------- |
| Contract location | `ReportingContext/Shared/InvestigationMetadata.cs` + `models/investigation.ts` | Cross-cutting; consumed by all dashboard and report modules |
| Registry | `InvestigationRegistry.cs` static class | Mirrors `AlertCenterRegistry`; PO-governed investigation text |
| Navigation service | `navigateToInvestigation.ts` replaces direct `navigateToReport` calls | Single encoder for query contract |
| Piutang all-open | New report query flag + unrestricted DAL path | Aligns report with `PiutangOpenBalanceDal` semantics without new KPI |
| Executive metadata | Extend `DashboardExecutiveRiskItem` + composer | Top 5 currently name/amount only — largest investigation gap |
| Legacy retrofit | Frontend row-click + backend `Investigation` on ranking DTOs | `Top10RankingTable` already supports `clickable` + `@row-click` |
| Breadcrumb component | `InvestigationBreadcrumb.vue` on all report views | Mandatory Q4 |
| Database | Optional `CustomerCode` on piutang top-customer snapshot | Enables stable executive → piutang drill-down |
| Deprecation | Keep `navigateToReport` as thin wrapper calling `navigateToInvestigation` until all call sites migrated | Reduces churn |

---

## 4. Impact Analysis

### 4.1 Affected modules

| Layer | Module | Change type |
| ----- | ------ | ----------- |
| Application | `ReportingContext/Shared/InvestigationMetadata.cs` | **New** |
| Application | `ReportingContext/Shared/InvestigationRegistry.cs` | **New** |
| Application | `DashboardExecutiveAgg` — `DashboardExecutiveRiskItem`, composer | **Extend** — Investigation on Top 5 |
| Application | `DashboardPiutangAgg` — `DashboardPiutangTopCustomer` | **Extend** — `CustomerCode` |
| Application | `DashboardSalesAgg` — `DashboardSalesRankingItem` | **Extend** — `SalesPersonId` |
| Application | `DashboardCustomerAgg`, `SalesmanAgg`, `CollectionAgg`, `InventoryRiskAgg`, `PurchasingAgg`, `LocationAgg` | **Extend** attention/ranking DTOs with `Investigation` |
| Application | `DashboardAlertCenterAgg` — composer + alert row DTO | **Extend** — map `Investigation`; report-first metadata |
| Application | `PiutangReportAgg` — query + handler | **Extend** — `AllOpenBalances` flag |
| Application | `PiutangReportAgg` — `PiutangReportRow` | **Extend** — `CustomerCode` |
| Infrastructure | `PiutangReportAgg/PiutangReportDal` | **Extend** — all-open query path |
| Infrastructure | `FinanceContext/PiutangSalesWilayahRpt` | **Extend** — `ListAllOpenBalances()` or equivalent |
| Infrastructure | Dashboard DALs (Customer, Sales, Piutang, Inventory, Executive) | **Map** Investigation metadata on read |
| Infrastructure | `DashboardPiutangSnapshotDal` | **Optional** — persist `CustomerCode` on top-customer rows |
| API | `PiutangReportController` | **Extend** — `allOpenBalances` query param |
| Frontend | `services/navigateToInvestigation.ts`, `services/applyInvestigationQuery.ts` | **New** |
| Frontend | `components/reports/InvestigationBreadcrumb.vue` | **New** |
| Frontend | `components/reports/InvestigationStepsList.vue` | **New** (nice to have) |
| Frontend | All four `*ReportView.vue` | **Extend** — hydration + breadcrumb |
| Frontend | `views/dashboard/DashboardHomeView.vue`, `ExecutiveExposureSection.vue` | **Extend** — clickable Top 5 |
| Frontend | `SalesDashboardView`, `PiutangDashboardView`, `InventoryDashboardView` | **Extend** — ranking drill-down |
| Frontend | M17–M22 attention components + `AlertCenterAlertTable.vue` | **Extend** — Investigate label + new navigation |
| Frontend | `models/dashboard.ts`, `models/investigation.ts`, `models/reports.ts` | **Extend** types |
| Tests | `InvestigationRegistryTest`, `navigateToInvestigation` unit tests | **New** |
| Tests | `PiutangReportDalTest` — all-open mode | **Extend** |
| Tests | `DashboardExecutiveComposerTest` — investigation on Top 5 | **Extend** |
| Docs | `btr-portal-operational.md`, `btr-portal-architecture.md`, `ALERT-REGISTRY.md` | Investigation framework section |

### 4.2 Unaffected modules

| Module | Reason |
| ------ | ------ |
| M17–M22 snapshot workers and aggregators (qualification logic) | No signal rule changes (Q24) |
| M16 executive KPI composition formulas | Navigation metadata only (Q25) |
| BTR Desktop | No deep links (Q15) |
| Chart components | Chart drill-down deferred (Q3) |
| Auth, health, worker scheduling | No relationship |

### 4.3 Traceability

| Investigation entry | Evidence report | Reconciliation |
| ------------------- | --------------- | -------------- |
| Customer overdue attention | Piutang Report (all-open) | Footer Total Piutang ↔ customer open balance when unfiltered by entity |
| Salesman below target | Sales Report (current month) | Sum of rep Faktur ↔ salesman dashboard KPI |
| Executive Top Customer | Piutang Report (all-open + customer filter) | Row sum ↔ Top 5 amount |
| Legacy Top 10 Salesman | Sales Report | Same as M18 ranking drill-down |
| Qualified Backlog alert | Purchasing Report (BELUM + principal) | PF1 backlog rows |
| Inventory Risk item | Inventory Report | Stock rows for BrgId |

---

## 5. Investigation Registry Design

Static class `InvestigationRegistry` in application layer. **Not** a duplicate of alert qualification — investigation-only metadata keyed by `SignalKey`.

```csharp
public sealed class InvestigationRegistryEntry
{
    public string SignalKey { get; }
    public string DefaultSignalLabel { get; }
    public string EntityType { get; }
    public string DashboardRoute { get; }
    public string ReportRoute { get; }              // null when dashboard-only
    public string DesktopNextStep { get; }
    public string DefaultPeriodMode { get; }         // null | "allOpenBalances"
    public string DefaultPostingFilter { get; }      // null | "BELUM"
    public IReadOnlyList<InvestigationStep> Steps { get; }
}
```

**Implementer requirements:**

1. Seed entries for all producer `SignalKey` constants used in attention lists and M23 alert feed.
2. Add synthetic executive exposure keys (Section 2.4.6).
3. Add legacy ranking keys: `LegacyTopSalesman`, `LegacyTopCustomer`, `LegacyTopCategory`, `LegacyTopSupplier`.
4. Unit test `InvestigationRegistry_ContainsAllAttentionSignalKeys` — cross-check aggregator constants.
5. Update `ALERT-REGISTRY.md` with **Investigation** column group: Report route, Period mode, Desktop next step (documentation sync).

**Helper:** `InvestigationMetadataBuilder.Build(entry, entityId, entityName, overrides)` — used by DALs to avoid duplication.

---

## 6. Backend Design

### 6.1 Dashboard DTO extensions (additive)

Add to attention row types (example — pattern repeats per domain):

```csharp
public class DashboardCustomerAttentionItem
{
    // existing fields unchanged
    public InvestigationMetadata Investigation { get; set; }
}
```

Populate in infrastructure DAL when mapping snapshot rows — merge:

- `ReportRoute` / entity fields from existing row mapping
- `InvestigationRegistry` defaults for `SignalKey`
- `SuggestedQuery.FreeText` = display name
- `SuggestedQuery.PeriodMode = allOpenBalances` when piutang-bound from all-time producers

**Alert Center row** (`DashboardAlertCenterAlertRow`):

```csharp
public InvestigationMetadata Investigation { get; set; }
```

`DashboardAlertCenterComposer` builds `Investigation` from producer attention row when available; for synthetic platform/achievement alerts, build dashboard-only metadata (`ReportRoute = null`).

### 6.2 Executive composer changes

`DashboardExecutiveRiskItem`:

```csharp
public class DashboardExecutiveRiskItem
{
    public int Rank { get; set; }
    public string Name { get; set; }
    public decimal Amount { get; set; }
    public InvestigationMetadata Investigation { get; set; }
}
```

`ComposeCriticalExposures` attaches investigation per Section 2.4.6.

### 6.3 Piutang report API

**Request** — extend `GetPiutangReportQuery`:

```csharp
public bool AllOpenBalances { get; set; }
```

**Controller** — `GET /api/reports/piutang?from=&to=&dateField=&allOpenBalances=true`

**Handler logic:**

```text
if (request.AllOpenBalances)
  return dal.GetAllOpenBalancesReport(dateField);
else
  return dal.GetReport(periode, dateField);  // unchanged
```

**Response** when all-open: include `bool AllOpenBalances = true` on response for UI labeling.

### 6.4 Piutang report DAL — all-open path

Add method using same SELECT as `PiutangSalesWilayahDal` but `WHERE aa.Sisa > 1` without date predicate. Map to existing `PiutangReportRow` including new `CustomerCode` field.

### 6.5 Optional snapshot DDL

```sql
ALTER TABLE BTR_PortalDashboardPiutangTopCustomer
  ADD CustomerCode NVARCHAR(50) NULL;
```

Worker/aggregator: populate `CustomerCode` when writing top-customer rows. **If DDL deferred:** executive Top Customer drill-down uses `CustomerName` in `q` only (still ships; ID param omitted until column populated).

---

## 7. Frontend Design

### 7.1 `navigateToInvestigation`

```typescript
export function navigateToInvestigation(
  router: Router,
  investigation: InvestigationMetadata,
  sourceDashboard: string,
): void {
  const query = buildInvestigationQuery(investigation, sourceDashboard)
  void router.push({ path: investigation.reportRoute, query })
}

export function navigateToDashboard(
  router: Router,
  dashboardRoute: string,
): void {
  void router.push(dashboardRoute)
}
```

`buildInvestigationQuery` maps `SuggestedQuery` → flat params (Section 2.4.2).

### 7.2 `applyInvestigationQuery`

Called from each report view `onMounted`:

| Param | Store action |
| ----- | ------------ |
| `q` | `freeText = q` |
| `periodMode=allOpenBalances` | Piutang: set flag, call `loadReport({ allOpenBalances: true })` |
| `posting=BELUM` | Purchasing: set `postingFilter` ref |
| `customerId`, `salesmanId`, etc. | Set ID filter refs consumed by `useReportInvestigationFilter` |
| `signalKey`, `signalLabel`, `source`, `entityType` | Pass to `InvestigationBreadcrumb` |

### 7.3 `InvestigationBreadcrumb.vue` (mandatory)

Display when any breadcrumb param present:

```text
Investigating: {EntityName} · Signal: {SignalLabel} · Source: {Source}
```

Optional link: **View dashboard context →** when `dashboardRoute` available in session metadata (pass via router state or encode `dashboard` query param — **prefer router state** to avoid polluting URL; fallback: omit link if state lost).

### 7.4 Component retrofit checklist

| Component | Change |
| --------- | ------ |
| `ExecutiveExposureSection` | `clickable` + `@row-click` → `navigateToInvestigation` |
| `SalesDashboardView` | Top 10 row click → Sales Report |
| `PiutangDashboardView` | Top 10 row click → Piutang all-open |
| `InventoryDashboardView` | Top 10 category/supplier click → Inventory Report |
| `CustomerAttentionList` | **Investigate** button label |
| `SalesmanAttentionList` | Same |
| `CollectionAttentionList` | Same |
| `InventoryRiskAttentionList` | Same |
| `PurchasingAttentionList` | Same |
| `LocationAttentionList` | Same |
| `AlertCenterAlertTable` | **Investigate** primary (report); **View Dashboard** secondary |
| `PurchasingReportView` | Add `?q=` hydration (prerequisite) |

### 7.5 Source dashboard labels

| Route | `source` label |
| ----- | -------------- |
| `/dashboard` | Executive Dashboard |
| `/alerts` | Alert Center |
| `/dashboard/customers` | Customer Analytics |
| `/dashboard/collection` | Collection Dashboard |
| `/dashboard/salesmen` | Salesman Performance |
| `/dashboard/inventory-risk` | Inventory Risk |
| `/dashboard/purchasing` | Purchasing Dashboard |
| `/dashboard/locations` | Location Performance |
| `/dashboard/sales` | Sales Dashboard |
| `/dashboard/piutang` | Piutang Dashboard |
| `/dashboard/inventory` | Inventory Dashboard |

---

## 8. Testing

### 8.1 Unit tests — `InvestigationRegistryTest`

| Case | Assertion |
| ---- | --------- |
| All M17–M22 attention SignalKey constants | Have registry entry |
| `CompoundDependency` | Has 3 investigation steps |
| `QualifiedBacklog` | `DefaultPostingFilter = BELUM` |
| Executive synthetic keys | Present with correct ReportRoute |

### 8.2 Unit tests — `buildInvestigationQuery` (frontend or shared)

| Case | Assertion |
| ---- | --------- |
| Piutang customer drill-down | `q`, `customerId`, `periodMode=allOpenBalances` |
| Qualified Backlog | `posting=BELUM`, `q=principal` |
| Minimal sales drill-down | `q` only when no id |

### 8.3 Unit tests — `PiutangReportDalTest`

| Case | Assertion |
| ---- | --------- |
| `GetAllOpenBalancesReport` | Returns rows with `KurangBayar > 1` regardless of due date outside current month |
| Default period report | Unchanged behavior |

### 8.4 Unit tests — `DashboardExecutiveComposerTest`

| Case | Assertion |
| ---- | --------- |
| TopCustomers[0] | `Investigation.ReportRoute = /reports/piutang` |
| TopCustomers[0] | `SuggestedQuery.PeriodMode = allOpenBalances` |
| TopPrincipals[0] | `Investigation.ReportRoute = /reports/purchasing` |

### 8.5 Manual test checklist

1. **Purchasing Report** — navigate with `?q=PrincipalName` → search pre-filled (prerequisite fix).
2. **Executive Top 5** — each of four tables: click row → correct report with filters.
3. **M11 Sales** — Top 10 salesman → Sales Report with rep name.
4. **M14 Piutang** — Top 10 customer → Piutang all-open + customer filter; footer reconciles.
5. **M15 Inventory** — Top 10 category/supplier → Inventory Report.
6. **Customer Attention** — Investigate → Piutang all-open or Sales per signal.
7. **Alert Center** — entity alert: **Investigate** opens report first; **View Dashboard** opens domain dashboard.
8. **Company alert** (Sales Achievement Critical) — Investigate absent; dashboard only.
9. **Wilayah Hotspot** — no report button; Collection Dashboard only.
10. **Qualified Backlog** — Purchasing Report shows only `BELUM` rows.
11. **Breadcrumb** — visible on report after drill-down with correct source label.
12. **Compound Dependency** (nice to have) — numbered steps visible on report.
13. Direct navigation to `/reports/piutang` without query — **unchanged** default month behavior.
14. No new snapshot tables or KPI changes on executive dashboard.

---

## 9. Risk Analysis

| Risk | Likelihood | Impact | Mitigation |
| ---- | ---------- | ------ | ---------- |
| Piutang all-open report performance (full outstanding set) | Medium | Medium | Same dataset already loaded for dashboards via `PiutangOpenBalanceDal`; monitor; acceptable for investigation use case |
| Entity name ambiguity in `?q=` | Medium | Medium | Prefer ID param filtering when producer provides code/id (Q6) |
| Executive CustomerCode snapshot migration | Low | Low | Optional DDL; name fallback |
| Investigation metadata drift across producers | Medium | High | Shared `InvestigationMetadataBuilder` + registry unit tests |
| Breaking existing `?q=` bookmarks | Low | High | Preserve `q` param semantics; new params optional |
| Alert Center UX change (report-first) | Low | Low | PO Q12 explicit; operational doc update |
| Purchasing BELUM filter + name search interaction | Low | Medium | Apply posting filter after name filter; document in test |
| Scope creep into chart drill-down | Medium | Medium | PR review against Q3 deferred list |

---

## 10. Documentation Updates (post-delivery)

| Document | Update |
| -------- | ------ |
| `docs/features/btr-portal/btr-portal-operational.md` | Investigation workflow KPI → Report → Desktop; Report-first alert path; breadcrumb |
| `docs/features/btr-portal/btr-portal-architecture.md` | Investigation Metadata Contract, `navigateToInvestigation`, Piutang all-open report flag |
| `docs/features/btr-portal/ALERT-REGISTRY.md` | Investigation columns: report route, period mode, desktop next step |
| `docs/features/btr-portal/btr-portal-domain.md` | M24 as platform capability reference |
| `docs/work/btr-portal/M24 Dashboard Drilldown - Analysis.md` | Link to this plan |

---

## 11. Implementation Steps

Execute in order. Each phase should compile before proceeding.

### Phase 1 — Investigation foundation

1. Add `InvestigationMetadata.cs`, `InvestigationRegistry.cs`, `InvestigationMetadataBuilder.cs` in application Shared.
2. Add `models/investigation.ts` TypeScript interfaces.
3. Implement `InvestigationRegistryTest`.
4. Add `navigateToInvestigation.ts`, `buildInvestigationQuery.ts`, `applyInvestigationQuery.ts`.
5. Add `InvestigationBreadcrumb.vue`.

### Phase 2 — Report prerequisites (mandatory bug fixes)

6. **Purchasing Report** — hydrate `?q=` on mount (mirror Sales/Piutang/Inventory).
7. **Piutang Report API** — `AllOpenBalances` flag + DAL all-open path + controller param.
8. Extend `PiutangReportRow` with `CustomerCode`; update `PiutangReportDalTest`.
9. Add `applyInvestigationQuery` to all four report views; wire breadcrumb.
10. Implement `posting=BELUM` filter on Purchasing Report.
11. Implement `useReportInvestigationFilter` (ID + free-text).

### Phase 3 — Backend producer metadata

12. Extend attention row DTOs with `Investigation` across Customer, Salesman, Collection, Inventory Risk, Purchasing, Location queries.
13. Map `Investigation` in respective infrastructure DALs using builder + registry.
14. Extend `DashboardExecutiveRiskItem` + composer with Investigation (Top 5).
15. Add `CustomerCode` to `DashboardPiutangTopCustomer` + aggregator/snapshot (optional DDL).
16. Add `SalesPersonId` to `DashboardSalesRankingItem` + sales snapshot read path.
17. Extend `DashboardAlertCenterAlertRow` + composer to emit `Investigation`.
18. Run backend unit tests.

### Phase 4 — Frontend dashboard retrofit

19. Executive `ExecutiveExposureSection` — clickable Top 5 with Investigate.
20. Legacy dashboards M11, M14, M15 — ranking row click handlers.
21. M17–M22 attention lists — **Investigate** label + `navigateToInvestigation`.
22. M23 `AlertCenterAlertTable` — report-first **Investigate** + **View Dashboard**.
23. Deprecate direct `navigateToReport` call sites (wrapper remains).

### Phase 5 — Nice to have (if time permits)

24. `InvestigationStepsList.vue` for cross-domain signals.
25. Desktop next-step text below breadcrumb from registry.
26. Signal context banner (richer than breadcrumb one-liner).

### Phase 6 — Verification and docs

27. Run full unit test suite.
28. Execute manual checklist (Section 8.5).
29. Spot-check Piutang footer reconciliation: dashboard total ↔ all-open report.
30. Update feature documentation (Section 10).

---

## Appendix A — Investigation routing matrix (authoritative)

| SignalKey / entry | ReportRoute | PeriodMode | Posting | Entity ID field |
| ----------------- | ----------- | ---------- | ------- | --------------- |
| `Overdue`, `ChronicOverdue`, `PlafondBreach*`, `HighOverdue*` | `/reports/piutang` | allOpenBalances | — | customerId / salesmanId |
| `Dormant`, `SuspendedWithSales`, `BelowTarget`, `NoTarget` | `/reports/sales` | currentMonth | — | customerId / salesmanId |
| `DeadStock`, `SlowMoving`, `NeverSold` | `/reports/inventory` | — | — | brgId |
| `QualifiedBacklog`, principal signals | `/reports/purchasing` | currentMonth | BELUM when QualifiedBacklog | supplierId |
| `WarehouseInactive*`, warehouse inventory | `/reports/inventory` | — | — | warehouseId |
| `ExecutiveTopCustomerExposure` | `/reports/piutang` | allOpenBalances | — | customerId |
| `ExecutiveTopCategoryExposure` | `/reports/inventory` | — | — | — |
| `ExecutiveTopSupplierExposure` | `/reports/inventory` | — | — | — |
| `ExecutiveTopPrincipalExposure` | `/reports/purchasing` | currentMonth | — | supplierId |
| `LegacyTopSalesman` | `/reports/sales` | currentMonth | — | salesmanId |
| `LegacyTopCustomer` | `/reports/piutang` | allOpenBalances | — | customerId |
| `LegacyTopCategory`, `LegacyTopSupplier` | `/reports/inventory` | — | — | — |
| Company / Platform signals | — | — | — | — |

---

## Appendix B — Milestone boundary summary

| Question | Owner |
| -------- | ----- |
| Which entities are flagged and why? | M17–M22 producers (unchanged) |
| Which alerts appear in Alert Center? | M23 composer (unchanged qualification) |
| How does user reach evidence? | **M24 Investigation Framework** |
| What KPIs mean? | Existing domain docs (unchanged) |
| Transaction posting, payment, stock movement? | BTR Desktop (portal stops at reports) |

---

## Appendix C — Success test (PO criteria)

For any KPI, Alert, Ranking, or Attention item in M16–M23, after M24:

1. User sees **why** — signal label + source in breadcrumb
2. User reaches **evidence** — report opens with correct period, posting, and entity filters
3. User knows **what's next** — optional dashboard link, multi-step list, or Desktop screen name

---

*End of implementation plan — M24 Dashboard Drill Down & Investigation Framework*
