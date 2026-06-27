# Implementation Plan: M18.6 — Sales Force Overview Dashboard

## Document Status

| Field | Value |
| --- | --- |
| Initiative | M18.6 Sales Force Overview — management comparison dashboard for field execution |
| Authoritative requirements | `docs/features/field-activity-overview/feature.md` |
| Related features | `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-architecture.md`, `docs/features/btr-portal/btr-portal-kpi-catalog.md`, `docs/work/btr-portal/M18-5-field-activity/implementation-plan.md` |
| Reference pattern | M18 `DashboardSalesmanAggregator` + snapshot worker; M18.5 `FieldActivityComposer` (KPI logic); M15 `InventoryHorizontalBarChart` (comparison charts) |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |
| Status | **Ready for implementation** (pending PO approval) |
| Implementation | **Do not start until approved** |

---

## 1. Goal

Deliver **Sales Force Overview** at `/dashboard/field-activity` — a comparison-first management dashboard that answers:

> *How is the entire sales organization performing today, and which salesmen require management attention?*

Relocate the existing M18.5 Control Tower to `/dashboard/field-activity/detail` as the drill-down page.

**Primary outcomes:**

| Outcome | Description |
| ------- | ----------- |
| Team KPI summary | Company-wide field execution headline cards |
| Comparison table | Sortable, searchable, color-coded per-salesman grid |
| Comparison charts | Four horizontal bar charts (execution, effective rate, orders, omzet) |
| Rankings | Top/bottom performer cards |
| Trends | 7-day and 30-day team trend lines |
| Snapshot-backed today view | Fast load via `BTRPD_FieldActivity*` refreshed every 15–30 min |
| On-demand historical dates | Batch live aggregation for Yesterday / Custom Date |
| Drill-down | Pre-select salesman + date on detail route |

---

## 2. KPI Inventory — Existing vs New

### 2.1 Per-salesman KPIs — logic exists (reuse)

| KPI | Catalog ID | Current source | Reuse strategy |
| --- | ---------- | -------------- | -------------- |
| Planned Visits | SF-KPI-012 | `FieldActivityComposer` | Extract to shared calculator |
| Actual Visits | SF-KPI-013 | Same | Same |
| Missed Visits | SF-KPI-014 | Same | Same |
| Unplanned Visits | SF-KPI-015 | Same | Same |
| Effective Calls | SF-KPI-016 | Same | Same |
| Visit Execution % | SF-KPI-017 | Same | Same |
| Effective Call Rate | SF-KPI-018 | Same | Same |
| GPS Valid/Warning/Suspicious counts | (implicit M18.5) | `FieldActivityKpis` | Same |
| Coordinate Coverage % | (M18.5 meta) | Per-salesman only — **not** in overview V1 | — |

### 2.2 Per-salesman KPIs — new calculations required

| KPI | Source | New work |
| --- | ------ | -------- |
| GPS Valid % | `GpsValidCount ÷ (Valid+Warning+Suspicious)` | Derived in aggregator |
| Sales Orders | `BTR_Order` COUNT | Extend order DAL — batch by date |
| Omzet (Order Value) | `BTR_Order.TotalAmount` SUM | Same batch DAL |
| Status Indicator | Composite rules (feature §5.3) | New policy class |
| Rank | Sort-derived | Read-time |

### 2.3 Team KPIs — new aggregations

| KPI | Formula | Exists today? |
| --- | ------- | ------------- |
| Active Salesmen | Count per feature §5.5 | **No** |
| All Section 1 sums | SUM per-salesman numerators | **No** — KPI catalog documents team ratio rule but no implementation |
| Team Visit Execution % | SUM(Actual)÷SUM(Planned) | Documented only |
| Team Effective Call Rate | SUM(Effective)÷SUM(Actual) | Documented only |
| Team GPS Valid Rate | SUM(valid)÷SUM(classifiable) | **No** |

### 2.4 Trend KPIs — new

| KPI | Grain | Storage |
| --- | ----- | ------- |
| Daily team Visit Execution % | Calendar day | `BTRPD_FieldActivityTrend` |
| Daily team Effective Call Rate | Calendar day | Same |
| Daily team Orders | Calendar day | Same |
| Daily team Omzet | Calendar day | Same |

### 2.5 Deferred KPIs (not M18.6)

| KPI | Reason |
| --- | ------ |
| Average Visit Duration | No duration field in check-in model |
| Check-in Time Distribution | Requires histogram aggregation — M25 |
| First Visit Time / Last Visit Time | Optional detail metrics — M25 |
| Average Omzet per Visit (column) | Derived — V1.1 optional column |

---

## 3. Snapshot Impact Analysis

### 3.1 Current state (M18.5)

```text
Field Activity = live query only
  No BTRPD_* tables
  No RefreshDashboard* worker
  Per-request: FieldActivityComposer(salesPersonId, visitDate)
```

M18.5 implementation plan explicitly deferred:

- Team trends, rankings → Release 2
- `BTRPD_FieldActivity*` snapshot + worker

**M18.6 implements the deferred Release 2 snapshot domain** for overview and trends, while preserving live query for salesman detail (map, stops, geometry).

### 3.2 Recommended hybrid data model

| Scenario | Data path | Rationale |
| -------- | --------- | --------- |
| **Today** (business date) | Read `BTRPD_FieldActivity*` snapshot | Intraday management refresh; avoids 100× composer calls |
| **Yesterday / Custom Date** | On-demand `FieldActivityOverviewComposer` batch aggregation | Historical dates don't need 15-min refresh; single batched SQL pass |
| **Salesman detail** | Existing live `FieldActivityComposer` | Map coordinates and route geometry remain live-query |

This preserves M18.5 intraday freshness philosophy while aligning overview with portal snapshot-first pattern (`materialized-dashboard-domain.md`).

### 3.3 New snapshot tables

| Table | Grain | Purpose |
| ----- | ----- | ------- |
| `BTRPD_FieldActivityKpi` | 1 row (`SnapshotKey='CURRENT'`) | Team headline KPIs + `ActivityDate` |
| `BTRPD_FieldActivitySalesman` | 1 row per salesman | Comparison table + chart source |
| `BTRPD_FieldActivityTrend` | 1 row per calendar day (rolling 30) | Trend section |
| `BTRPD_FieldActivityRanking` | Optional — **derive at read time** from salesman table | Avoid duplicate storage in V1 |

**Snapshot key:** `CURRENT` with explicit `ActivityDate` column (same pattern as other domains storing business context on CURRENT row).

### 3.4 Refresh cadence

| Domain | Cadence | Rationale |
| ------ | ------- | --------- |
| FieldActivity | **15 min** | Field execution is intraday operational; faster than Salesman (30 min) |

Register in `RefreshDashboardSnapshotsCommand` and Task Scheduler as tenth domain job.

---

## 4. Architecture Overview

### 4.1 Target topology

```text
                    ┌─────────────────────────────────────┐
                    │  Browser — Sales Force Overview      │
                    │  /dashboard/field-activity           │
                    └─────────────────┬───────────────────┘
                                      │
              visitDate = today       │         visitDate = historical
                      ↓               │                 ↓
        GET /api/dashboard/field-activity/overview?visitDate=
                      ↓               │                 ↓
              Read BTRPD_* snapshot   │    FieldActivityOverviewComposer
                      │               │    (batch SQL, no N+1)
                      └───────────────┴─────────────────┘
                                      ↓
                        FieldActivityOverviewResponse
                          TeamKpis · Salesmen[] · Rankings · Trends · Charts

Task Scheduler (15 min)
        ↓
RefreshDashboardFieldActivitySnapshotWorker
        ↓
DashboardFieldActivityOverviewAggregator
  ├─ ISalesPersonDal.ListData()
  ├─ IFieldActivityBatchCheckInDal.ListByDate()        [NEW]
  ├─ IFieldActivityBatchOrderDal.ListByDate()          [NEW]
  ├─ IFieldActivityBatchVisitPlanDal.ListByDate()      [NEW]
  ├─ IVisitPlanExceptionDal (batch load for date)       [NEW or extend]
  ├─ FieldActivityKpiCalculator (extracted from composer) [REFACTOR]
  └─ FieldActivityStatusPolicy                           [NEW]
        ↓
IDashboardFieldActivitySnapshotDal.ReplaceCurrent()
  → BTRPD_FieldActivityKpi + Salesman + Trend

Drill-down:
  /dashboard/field-activity/detail?salesPersonId=&visitDate=
        ↓
  Existing FieldActivityDashboardView (unchanged behavior)
        ↓
  GET /api/dashboard/field-activity?salesPersonId=&visitDate=
```

### 4.2 Architecture decisions

| Decision | Choice | Rationale |
| -------- | ------ | --------- |
| Overview data for today | Snapshot read | Performance at 100 salesmen; consistent with portal pattern |
| Historical dates | Batch live composer | No snapshot explosion for arbitrary dates |
| KPI business logic | Extract `FieldActivityKpiCalculator` from `FieldActivityComposer` | Single source of truth; composer delegates to calculator |
| Avoid N+1 | **Never** loop `FieldActivityComposer` per salesman | 100 reps × 4 DAL calls = unacceptable |
| Order metrics | Batch `BTR_Order` grouped by `UserEmail` | `TotalAmount` already on order row |
| Rankings | Compute at API read from salesman rows | Sort + Take(5/10) — no extra table V1 |
| Wilayah chart | Group salesman rows by `WilayahName` | Optional §6; no GIS |
| Detail page | Keep live query | GeoJSON + replay need visit-level data |
| Menu codes | SF02 → Overview; new SF03 → Detail | Preserves SF01 Salesmen unchanged |

### 4.3 Refactor — extract KPI calculator

Extract from `FieldActivityComposer` into `FieldActivityKpiCalculator`:

```csharp
public static class FieldActivityKpiCalculator
{
    public static FieldActivityKpiInputResult Compute(
        IReadOnlyList<EffectiveVisitPlanEntry> plan,
        IReadOnlyList<FieldActivityCheckInRow> checkIns,
        ISet<string> orderCustomerIds,
        IReadOnlyList<FieldActivityOrderSummaryRow> orders); // count + amount
}
```

`FieldActivityComposer` calls calculator — existing unit tests remain valid with minimal adjustment.

---

## 5. Backend Changes

### 5.1 New application module structure

```text
ReportingContext/DashboardFieldActivityOverviewAgg/
├── Contracts/
│   ├── IDashboardFieldActivitySnapshotDal.cs
│   ├── IFieldActivityBatchCheckInDal.cs
│   ├── IFieldActivityBatchOrderDal.cs
│   └── IFieldActivityBatchVisitPlanDal.cs
├── Models/
│   ├── FieldActivityOverviewResponse.cs
│   ├── FieldActivityTeamKpis.cs
│   ├── FieldActivitySalesmanRow.cs
│   ├── FieldActivityRankingSection.cs
│   └── FieldActivityTrendPoint.cs
├── Services/
│   ├── FieldActivityKpiCalculator.cs          [extracted]
│   ├── FieldActivityOverviewComposer.cs       [batch orchestrator]
│   ├── DashboardFieldActivityOverviewAggregator.cs
│   └── FieldActivityStatusPolicy.cs
├── Queries/
│   └── GetFieldActivityOverviewQuery.cs
└── UseCases/
    └── RefreshDashboardFieldActivitySnapshotWorker.cs
```

### 5.2 New batch DAL contracts

**Check-ins for all reps on a date:**

```csharp
public interface IFieldActivityBatchCheckInDal
{
    IReadOnlyList<FieldActivityCheckInRow> ListByDate(DateTime visitDate);
    // Row includes UserEmail for grouping
}
```

SQL: same dedupe CTE as single-rep DAL, filter `CheckInDate = @VisitDate` only (no email filter), include `UserEmail` in SELECT.

**Orders for all reps on a date:**

```csharp
public interface IFieldActivityBatchOrderDal
{
    IReadOnlyList<FieldActivityOrderBatchRow> ListByDate(DateTime visitDate);
}

public class FieldActivityOrderBatchRow
{
    public string UserEmail { get; set; }
    public string CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
}
```

**Visit plans for all reps on a date:**

```csharp
public interface IFieldActivityBatchVisitPlanDal
{
    IReadOnlyList<FieldActivityVisitPlanBatchRow> ListByDate(DateTime visitDate);
}
```

Apply `EffectiveVisitPlanResolver` in-memory per salesman after batch load of `BTR_VisitPlan` + `BTR_VisitPlanException` for the date — **one SQL read each**, resolver runs in memory (100 iterations of pure logic, not 100 SQL round-trips).

### 5.3 Aggregator algorithm

```text
1. businessDate = IBusinessDateProvider.Today
2. salesmen = ISalesPersonDal.ListData()
3. checkInsByEmail = batch check-ins grouped by UserEmail
4. ordersByEmail = batch orders grouped by UserEmail
5. plansBySalesPersonId = batch visit plan → effective plan per salesman
6. FOR EACH salesman:
     a. Skip KPI ratios if no Email → Status = NoFieldData
     b. plan = plansBySalesPersonId[salesPersonId]
     c. checkIns = checkInsByEmail[email]
     d. orders = ordersByEmail[email]
     e. kpis = FieldActivityKpiCalculator.Compute(plan, checkIns, orders)
     f. status = FieldActivityStatusPolicy.Resolve(kpis)
     g. append FieldActivitySalesmanRow
7. teamKpis = sum-ratio aggregation across rows with HasFieldData
8. trendRows = for each day in [today-29 .. today]: repeat steps 3-7 team-level only
9. return aggregate result
```

### 5.4 API contracts

**New endpoint:**

```
GET /api/dashboard/field-activity/overview?visitDate=yyyy-MM-dd
```

| Param | Required | Default |
| ----- | -------- | ------- |
| `visitDate` | No | Business today |

**Response shape (abbreviated):**

```json
{
  "VisitDate": "2026-06-27",
  "DataSource": "Snapshot | LiveBatch",
  "GeneratedAt": "2026-06-27T10:15:00",
  "TeamKpis": {
    "ActiveSalesmenCount": 42,
    "PlannedVisits": 380,
    "ActualVisits": 310,
    "VisitExecutionPercent": 81.6,
    "EffectiveCalls": 185,
    "EffectiveCallRate": 59.7,
    "MissedVisits": 70,
    "UnplannedVisits": 22,
    "GpsValidRate": 88.4,
    "TotalOrders": 210,
    "TotalOmzet": 125000000.00
  },
  "Salesmen": [ { "SalesPersonId", "Rank", "Kpis", "Orders", "Omzet", "Status", "GpsValidPercent" } ],
  "Rankings": {
    "TopVisitExecution": [],
    "BottomVisitExecution": [],
    "...": []
  },
  "Trends": {
    "Last7Days": [],
    "Last30Days": []
  },
  "WilayahBreakdown": [ { "WilayahName", "ActualVisits" } ],
  "Meta": { "PlanDataAvailable", "VisitPlanGoLiveDate" }
}
```

**Unchanged endpoints:**

- `GET /api/dashboard/field-activity?salesPersonId=&visitDate=` — detail
- `GET /api/dashboard/field-activity/salesmen` — selector (detail page)

**New controller or extend existing:**

Extend `FieldActivityDashboardController` with `[Route("overview")]` action — keeps domain cohesive.

### 5.5 Worker

`RefreshDashboardFieldActivitySnapshotWorker`:

- Domain label: `"FieldActivity"`
- Follow `RefreshDashboardCollectionSnapshotWorker` template (log → aggregate → replace → mark success)
- Aggregate for `IBusinessDateProvider.Today` only
- Persist team KPI, all salesman rows, 30-day trend
- Register in `RefreshDashboardSnapshotsCommand`

### 5.6 Status policy

```csharp
public static class FieldActivityStatusPolicy
{
    public static FieldActivitySalesmanStatus Resolve(FieldActivityKpiInputResult kpis, bool hasEmail)
    {
        if (!hasEmail) return NoFieldData;
        if (kpis.PlannedVisits == 0 && kpis.ActualVisits == 0) return NoPlan;
        // Critical / NeedsAttention / OnTrack per feature §5.3
    }
}
```

---

## 6. Database Changes

### 6.1 New tables (DDL in `btr.sql/Tables/ReportingContext/`)

**`BTRPD_FieldActivityKpi`**

| Column | Type | Notes |
| ------ | ---- | ----- |
| SnapshotKey | VARCHAR(10) PK | `'CURRENT'` |
| GeneratedAt | DATETIME | |
| ActivityDate | DATE | Business date of aggregation |
| ActiveSalesmenCount | INT | |
| PlannedVisits … GpsValidRate | INT/DECIMAL | Team KPIs |
| TotalOrders | INT | |
| TotalOmzet | DECIMAL(18,2) | Order value |
| LastRefreshLogId | VARCHAR(26) | |

**`BTRPD_FieldActivitySalesman`**

| Column | Type | Notes |
| ------ | ---- | ----- |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| SalesPersonId | VARCHAR PK part | |
| SalesPersonCode, SalesPersonName | | |
| WilayahName | VARCHAR | Optional chart |
| HasEmail | BIT | |
| PlannedVisits … GpsSuspiciousCount | | Per SF-KPI-012–018 |
| GpsValidPercent | DECIMAL | |
| OrdersCount | INT | |
| OmzetAmount | DECIMAL(18,2) | |
| StatusCode | VARCHAR(20) | OnTrack / NeedsAttention / Critical / NoPlan / NoFieldData |
| Rank | INT | Default execution rank |

**`BTRPD_FieldActivityTrend`**

| Column | Type |
| ------ | ---- |
| SnapshotKey | VARCHAR(10) |
| TrendDate | DATE PK part |
| VisitExecutionPercent | DECIMAL |
| EffectiveCallRate | DECIMAL |
| OrdersCount | INT |
| OmzetAmount | DECIMAL(18,2) |

### 6.2 Indexes (operational tables)

Extend `Upgrade_M18_5_FieldActivity_Index.sql` or add `Upgrade_M18_6_FieldActivity_Batch_Index.sql`:

```sql
-- Batch check-in by date (all salesmen)
CREATE NONCLUSTERED INDEX IX_BTR_CheckIn_CheckInDate
    ON BTR_CheckIn (CheckInDate)
    INCLUDE (UserEmail, CustomerId, CheckInTime, CheckInLatitude, CheckInLongitude,
             CustomerLatitude, CustomerLongitude, Accuracy);

-- Batch orders by date
CREATE NONCLUSTERED INDEX IX_BTR_Order_OrderDate_UserEmail
    ON BTR_Order (OrderDate, UserEmail)
    INCLUDE (CustomerId, TotalAmount);
```

Idempotent script pattern per existing portal upgrades.

### 6.3 Migration script

`Scripts/Create_BTRPD_FieldActivity_Tables.sql` — add to `Create_BTRPD_PortalDashboard_Tables.sql` master script.

---

## 7. Frontend Changes

### 7.1 Route restructure

| Route | Name | Component | Notes |
| ----- | ---- | --------- | ----- |
| `/dashboard/field-activity` | `field-activity-overview` | `FieldActivityOverviewView.vue` | **NEW** |
| `/dashboard/field-activity/detail` | `field-activity-detail` | `FieldActivityDashboardView.vue` | **Moved** from parent path |

**Redirect compatibility:** None required if portal not in production. If bookmarks exist, add redirect `/dashboard/field-activity` → only when query params present → detail.

### 7.2 Navigation registry updates

| Code | Label | Route |
| ---- | ----- | ----- |
| SF02 | Sales Force Overview | `/dashboard/field-activity` |
| SF03 | Salesman Field Activity | `/dashboard/field-activity/detail` |

Update both:

- `btr.portal.web/src/navigation/portalMenuRegistry.ts`
- `btr.application/.../Shared/PortalMenuRegistry.cs`

Add `SF03` to `PortalMenuCode` type union.

### 7.3 New view — `FieldActivityOverviewView.vue`

Layout (top to bottom):

```text
┌─────────────────────────────────────────────────────────────┐
│ Header + date toolbar (Today / Yesterday / Custom) + Refresh │
├─────────────────────────────────────────────────────────────┤
│ Section 1: Team KPI cards (reuse/adapt FieldActivityKpiStrip)│
├─────────────────────────────────────────────────────────────┤
│ Section 2: Salesman Performance DataTable (primary)          │
│   PrimeVue DataTable — sortable, filterable, row click       │
├─────────────────────────────────────────────────────────────┤
│ Section 3: 2×2 chart grid (horizontal bar charts)            │
├─────────────────────────────────────────────────────────────┤
│ Section 4: Ranking cards grid (Top10RankingTable pattern)    │
├─────────────────────────────────────────────────────────────┤
│ Section 5: Trend charts (7d / 30d toggle)                    │
├─────────────────────────────────────────────────────────────┤
│ Section 6 (optional): Wilayah visits bar chart               │
└─────────────────────────────────────────────────────────────┘
```

### 7.4 Component reuse

| New component | Based on | Purpose |
| ------------- | -------- | ------- |
| `FieldActivityTeamKpiStrip.vue` | `FieldActivityKpiStrip.vue` | Extend for team KPIs + Active Salesmen + GPS Valid Rate |
| `FieldActivitySalesmanTable.vue` | PrimeVue DataTable + salesman attention color utils | Sortable comparison table |
| `FieldActivityComparisonChart.vue` | `InventoryHorizontalBarChart.vue` | Generic horizontal bar; dynamic height = rows × 26px |
| `FieldActivityRankingGrid.vue` | `Top10RankingTable.vue` | Multiple small ranking cards |
| `FieldActivityTeamTrendChart.vue` | `DailyPaceTrendChart.vue` | Line chart for 7/30 day team trends |
| `FieldActivityWilayahChart.vue` | `InventoryHorizontalBarChart.vue` | Optional territory section |

### 7.5 Detail page changes

`FieldActivityDashboardView.vue`:

- Read `salesPersonId` and `visitDate` from route query on mount
- Auto-load when both present (preserve manual load when missing)
- Breadcrumb: `Sales Force Overview → {Salesman Name}`
- Back link to overview preserving date

### 7.6 API client

`fieldActivityApi.ts`:

```typescript
export async function getFieldActivityOverview(visitDate: string): Promise<FieldActivityOverviewResponse>
```

Models in `fieldActivity.ts` — extend with overview types.

### 7.7 KPI cell coloring

Reuse band resolver pattern from salesman/customer dashboards:

| Metric | Green | Amber | Red |
| ------ | ----- | ----- | --- |
| Visit Execution % | ≥ 80 | 50–79 | < 50 |
| Effective Call Rate | ≥ 50 | 30–49 | < 30 |
| GPS Valid % | ≥ 85 | 70–84 | < 70 |

Apply via CSS class on DataTable body cells — no new threshold infrastructure required.

---

## 8. Performance Considerations

### 8.1 Load model estimates

Assumptions: 12 planned visits/day/rep avg, 10 check-ins, 3 orders; indexed batch queries.

| Salesmen | Snapshot read (Today) | Batch live (Historical) | Notes |
| -------- | ---------------------- | ------------------------- | ----- |
| 20 | ~50–80 ms | ~300–600 ms | Single API call |
| 50 | ~60–100 ms | ~600 ms–1.2 s | |
| 100 | ~80–120 ms | ~1–2.5 s | Acceptable for on-demand historical |

**Anti-patterns prohibited:**

| Pattern | 100 reps cost | Verdict |
| ------- | ------------- | ------- |
| Loop `FieldActivityComposer` | ~100 × 4 SQL ≈ 400 queries | **Reject** |
| N+1 order lookup | ~100 queries | **Reject** |
| Snapshot read | 3 SELECTs | **Accept** |

### 8.2 Payload size

~100 salesmen × ~500 bytes/row ≈ 50 KB JSON — acceptable.

Charts and rankings are client-derived from `Salesmen[]` array — no duplicate server payload.

### 8.3 Worker duration

Batch aggregation 100 reps: estimated 5–15 s — acceptable for 15-min background job.

### 8.4 Frontend rendering

100-row horizontal bar charts: use scroll container with `max-height: 480px` and dynamic canvas height — proven in inventory dashboard.

---

## 9. Implementation Complexity

| Area | Complexity | Effort (relative) |
| ---- | ---------- | ----------------- |
| Extract KPI calculator + refactor composer | Low | S |
| Batch DALs (3 SQL) | Medium | M |
| Overview aggregator + worker + snapshot DAL | Medium | L |
| API + MediatR query | Low | S |
| Overview frontend (table + charts) | Medium | L |
| Route/nav migration + detail query params | Low | S |
| DDL + index scripts | Low | S |
| Unit tests (calculator, aggregator, status policy) | Medium | M |
| KPI catalog + domain doc updates | Low | S |

**Overall:** **Medium-Large** — comparable to M18 snapshot domain but smaller table count; reuses substantial M18.5 logic.

**Suggested phases:**

1. Refactor calculator + batch DALs + aggregator (backend)
2. Snapshot tables + worker + overview API
3. Overview frontend shell + table + team KPIs
4. Charts + rankings + trends
5. Route migration + detail drill-down + nav registry
6. Optional Wilayah chart + documentation

---

## 10. Risks and Mitigations

| Risk | Impact | Mitigation |
| ---- | ------ | ---------- |
| Snapshot stale for Today | Management sees outdated execution | 15-min cadence; show `GeneratedAt`; manual refresh via existing snapshot refresh API |
| Historical batch slow | Custom date load > 3 s | Batch SQL + indexes; loading skeleton; consider caching yesterday's batch in snapshot after first request (V1.1) |
| Email-less salesmen | Incomplete team picture | Show in table with No Field Data; exclude from team ratios; align with M18.5 |
| Order vs Faktur omzet confusion | Management misinterpretation | UI label **Order Value**; tooltip explains BTR_Order not Faktur |
| Visit plan batch resolver correctness | Wrong planned counts | Reuse `EffectiveVisitPlanResolver`; unit test against known salesman-day fixtures from `FieldActivityComposerTest` |
| Route break for SF02 bookmarks | User confusion | Document in release notes; optional redirect if `?salesPersonId=` present |
| `BTR_Order` schema drift | Deploy failure | Same dependency as M18.5; document in deploy guide |
| Scope creep — team map | High effort | Explicitly defer per feature out-of-scope |
| KPI catalog drift | Documentation debt | Add SF-KPI-019–027 in post-implementation curator pass |

---

## 11. Recommended Implementation Plan (Phased)

### Phase 1 — Shared KPI extraction (backend foundation)

1. Create `FieldActivityKpiCalculator` — extract logic from `FieldActivityComposer`.
2. Update `FieldActivityComposer` to delegate; fix existing tests.
3. Add `FieldActivityStatusPolicy` + tests.
4. Add batch DAL interfaces and SQL implementations.

**Exit criteria:** Calculator tests pass; batch DALs return correct rows for seed demo date.

### Phase 2 — Overview aggregator + API (no snapshot yet)

1. Implement `FieldActivityOverviewComposer` using batch DALs.
2. Add `GetFieldActivityOverviewQuery` + controller action.
3. Unit tests: team sum-ratio, 100-rep fixture, status assignment.

**Exit criteria:** API returns overview JSON for any date via live batch.

### Phase 3 — Snapshot domain + worker

1. DDL for three `BTRPD_FieldActivity*` tables.
2. `DashboardFieldActivityOverviewAggregator` + snapshot DAL.
3. `RefreshDashboardFieldActivitySnapshotWorker` + scheduler registration.
4. Overview query reads snapshot when `visitDate == business today`.

**Exit criteria:** Today view served from snapshot; worker refresh populates tables.

### Phase 4 — Overview frontend

1. Create `FieldActivityOverviewView.vue` + API client.
2. Team KPI strip + salesman DataTable with sort/search/color.
3. Wire date toolbar.

**Exit criteria:** Management can scan table and sort by execution %.

### Phase 5 — Charts, rankings, trends

1. Comparison charts (4) from `Salesmen[]`.
2. Ranking card grid.
3. 7/30-day trend section.
4. Optional Wilayah chart.

**Exit criteria:** Full dashboard layout per feature spec.

### Phase 6 — Navigation migration + drill-down

1. Move Control Tower to `/dashboard/field-activity/detail`.
2. Update menu registries (SF02/SF03).
3. Detail page query param auto-load + breadcrumb.
4. Router tests + navigation spec updates.

**Exit criteria:** Overview → detail drill-down works end-to-end.

### Phase 7 — Documentation and indexes

1. Index upgrade script.
2. Update `btr-portal-domain.md`, `btr-portal-architecture.md`, `btr-portal-kpi-catalog.md`.
3. Manual test plan execution.

**Exit criteria:** Knowledge curator checklist complete.

---

## 12. Testing Strategy

| Layer | Tests |
| ----- | ----- |
| `FieldActivityKpiCalculatorTest` | All M18.5 composer cases still pass |
| `FieldActivityOverviewAggregatorTest` | Team sum-ratio; 3 salesmen fixture; no-email rep; N/A denominators |
| `FieldActivityStatusPolicyTest` | All status bands |
| `FieldActivityOverviewComposerTest` | Historical date live batch |
| `fieldActivityApi.spec.ts` | Overview endpoint params |
| `router/index.spec.ts` | New routes registered |
| Manual | 100-rep staging load; drill-down; date switching today↔yesterday |

---

## 13. API Impact Summary

| Endpoint | Change |
| -------- | ------ |
| `GET /api/dashboard/field-activity/overview` | **NEW** |
| `GET /api/dashboard/field-activity` | Unchanged |
| `GET /api/dashboard/field-activity/salesmen` | Unchanged |
| `POST /api/dashboard/refresh` (if exists) | Register FieldActivity domain |

---

## 14. New KPI Catalog Entries (post-implementation)

| ID | Name | Location |
| -- | ---- | -------- |
| SF-KPI-019 | Active Salesmen Count | SF02 Overview |
| SF-KPI-020 | Team Visit Execution % | SF02 Overview |
| SF-KPI-021 | Team Effective Call Rate | SF02 Overview |
| SF-KPI-022 | Team GPS Valid Rate | SF02 Overview |
| SF-KPI-023 | Sales Orders (daily) | SF02 Overview |
| SF-KPI-024 | Order Value (daily) | SF02 Overview |
| SF-KPI-025 | Salesman Status Indicator | SF02 Overview table |
| SF-KPI-026 | Team Execution Trend (7/30d) | SF02 Overview |
| SF-KPI-027 | Visits by Wilayah | SF02 Overview (optional) |

SF-KPI-012–018 remain on SF03 detail; catalog location field updated.

---

## 15. Knowledge Curator Checklist (post-implementation)

- [ ] Update `docs/features/btr-portal/btr-portal-domain.md` §7.10 and §8.1 with Sales Force Overview
- [ ] Update `docs/features/btr-portal/btr-portal-architecture.md` — replace "Field Activity Live Query only" with hybrid model
- [ ] Update `docs/features/btr-portal/btr-portal-operational.md` navigation tree (SF02/SF03)
- [ ] Add SF-KPI-019–027 to KPI catalog
- [ ] Archive this plan after knowledge sync per AGENTS.md

---

*End of implementation plan. Ready for Product Owner approval and Implementer handoff.*
