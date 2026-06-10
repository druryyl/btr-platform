# Implementation Plan: M14 — Piutang Dashboard V2

## Document Status

| Field | Value |
| --- | --- |
| Initiative | M14 Piutang Dashboard V2 — **portfolio quality, risk exposure, and customer concentration** |
| Authoritative requirements | `docs/work/btr-portal/M14-piutang-dashboard-v2-analysis.md` — **Section 11 (Product Owner decisions)** |
| Related features | `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md`, M10 Piutang Report, M17 Customer Analytics, M20 Collection Dashboard, M16 Executive |
| Reference pattern | M14 V1 (`BTRPD_Piutang*`), M20 Collection snapshot extension pattern |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |
| Status | **Ready for implementation** |
| Product decisions | **Confirmed** — analysis Section 11, 2026-06-10 |

---

## 1. Goal

Extend `/dashboard/piutang` from a receivable snapshot (M14 V1) into a **management analytics dashboard** focused on receivable quality, risk exposure, customer concentration, and portfolio composition — without duplicating Collection execution (M20) or Salesman concentration (M18).

**Primary outcomes:**

- **Five KPI tiles** (all-time open balance): Total Piutang · Total Customer · Overdue Customer · **Overdue Piutang** · **Piutang > 90 Hari** (amount **and** % of Total Piutang).
- **Concentration KPIs:** Top 10 Customer % and Top 20 Customer % of Total Piutang.
- **Aging chart:** Unchanged five-bucket pie (`JatuhTempo` anchor, inclusive boundaries).
- **Customer risk table:** **Top 20** outstanding customers with per-customer aging breakdown (Customer · Total · Current · 1–30 · 31–60 · 61–90 · >90).
- **Customer aging snapshot** (`BTRPD_PiutangCustomerAging`) materialized on the **existing Piutang refresh worker** (15 min) — foundation for V2 and future cross-dashboard reuse.
- Drill-down to **M10 Piutang Report** (`?q=` customer name) — unchanged investigation pattern.

**Explicitly out of scope (PO confirmed):**

- Collection performance, Top Overdue rankings, recovery KPIs (M20).
- Salesman / wilayah concentration on this dashboard (M18 / M20).
- Principal concentration (no data path).
- Monthly portfolio trends (deferred — Section 8.5 / Q14–Q17).
- Changes to Collection, Customer, or Salesman snapshot domains in V2.
- Executive Dashboard, Alert Center, or Overview home card KPI expansion (future consumers of customer aging snapshot).
- Custom date filters, export, or live aggregation fallback.

---

## 2. Authoritative Product Decisions

Source: analysis Section 11. Do not re-decide these rules during implementation.

### 2.1 Positioning

| Decision | Value |
| --- | --- |
| Route | `/dashboard/piutang` (existing) |
| Page title | **Piutang Dashboard** |
| Charter | Exposure & portfolio quality — *What is receivable quality, risk, and concentration?* |
| M20 relationship | **Complements** Collection — no Top Overdue customer table on Piutang V2 |
| M17 relationship | Customer Analytics owns *management attention* signals; Piutang V2 owns *portfolio* framing |
| Data scope | **All-time open balance** (`KurangBayar > 1`) — same as M14 V1 |
| Aging anchor | **`JatuhTempo` / `DueDate` only** — not FakturDate or PiutangDate |
| Refresh cadence | **15 minutes** — bind customer aging snapshot to Piutang worker |

### 2.2 KPI and concentration formulas (architect-resolved)

| Metric | Formula | Notes |
| --- | --- | --- |
| **Total Piutang** | `SUM(KurangBayar)` where `> 1` | Unchanged |
| **Total Customer** | Distinct customer keys with open balance | Unchanged — `CustomerCode` preferred, `CustomerName` fallback |
| **Overdue Customer** | Distinct customers with any balance in bucket ≠ `Current` | Unchanged |
| **Overdue Piutang** | `Total Piutang − Current bucket amount` | Equivalent to sum of `Days1To30` + `Days31To60` + `Days61To90` + `DaysOver90` |
| **Piutang > 90 Hari** (amount) | `DaysOver90` bucket amount | Same bucket key as company aging chart |
| **Piutang > 90 Hari** (%) | `DaysOver90 amount ÷ Total Piutang × 100` | Null when Total Piutang = 0 |
| **Top 10 Customer %** | `SUM(top 10 customer Total Piutang) ÷ Total Piutang × 100` | Denominator = **Total Piutang**, not overdue |
| **Top 20 Customer %** | `SUM(top 20 customer Total Piutang) ÷ Total Piutang × 100` | Same denominator |

**Per-customer Total Piutang** (risk table and concentration numerators):

```
TotalPiutang = CurrentAmount + Aging30Amount + Aging60Amount + Aging90Amount + AgingOver90Amount
```

Each bucket amount = `SUM(KurangBayar)` of open rows assigned to that bucket at Faktur grain.

**Reconciliation invariants (must hold after every refresh):**

1. Σ company aging buckets = Total Piutang.
2. Overdue Piutang = Σ four overdue buckets = Total Piutang − Current bucket.
3. Per customer: Σ five bucket columns = customer Total Piutang.
4. Σ all `BTRPD_PiutangCustomerAging` row totals = Total Piutang.
5. Top 20 Customer % ≥ Top 10 Customer % when both computed from the same ranking.

### 2.3 Aging buckets (unchanged)

| Bucket key | Label | Rule (`DaysOverdue = Today − JatuhTempo.Date`) |
| ---------- | ----- | ------------------------------------------------ |
| `Current` | Current (Not Yet Due) | `DaysOverdue ≤ 0` |
| `Days1To30` | 1–30 Days | `1 ≤ DaysOverdue ≤ 30` |
| `Days31To60` | 31–60 Days | `31 ≤ DaysOverdue ≤ 60` |
| `Days61To90` | 61–90 Days | `61 ≤ DaysOverdue ≤ 90` |
| `DaysOver90` | > 90 Days | `DaysOverdue > 90` |

### 2.4 Customer risk table

| Rule | Value |
| --- | --- |
| Row count | **Top 20** by Total Piutang descending |
| Tie-break | Customer name ascending (case-insensitive) |
| Columns | Customer · Total · Current · 1–30 · 31–60 · 61–90 · >90 |
| Attribution columns | **No** salesman or wilayah |
| Drill-down | Row click → `/reports/piutang?q={CustomerName}` |

### 2.5 Customer aging snapshot (PO Q18, Q19)

| Field | Persisted | Definition |
| ----- | --------- | ---------- |
| `CustomerId` | Yes | Canonical key — `BTR_Customer.CustomerId` |
| `CustomerCode` | Yes | Denormalized for display / investigation |
| `CustomerName` | Yes | Denormalized display name |
| `CurrentAmount` | Yes | Non-cumulative `Current` bucket |
| `Aging30Amount` | Yes | Non-cumulative `Days1To30` bucket |
| `Aging60Amount` | Yes | Non-cumulative `Days31To60` bucket |
| `Aging90Amount` | Yes | Non-cumulative `Days61To90` bucket |
| `AgingOver90Amount` | Yes | Non-cumulative `DaysOver90` bucket |
| `LastUpdate` | Yes | Same semantics as KPI `GeneratedAt` |
| `OverdueAmount` | **No** | Derivable — do not persist |

**Grouping key at refresh:** `CustomerId` when present; rows with null/missing `CustomerId` fall back to legacy string key (`CustomerCode` / `CustomerName`) and **exclude** from `BTRPD_PiutangCustomerAging` if `CustomerId` cannot be resolved — log count in refresh metadata for ops visibility. (Open piutang without Faktur/customer join is exceptional; preserve company-level totals from all rows.)

### 2.6 Layout (fixed section order)

1. KPI row (5 tiles)
2. Concentration row (Top 10 % · Top 20 %)
3. Aging Distribution (pie chart — unchanged component)
4. Top 20 Customer Risk (aging breakdown table)
5. Navigation links (existing pattern if present on page)

Subtitle hint (preserve / extend): *Outstanding balance snapshot — all open receivables (`Sisa > 1`). Numbers may differ from Piutang Report when a period filter is applied.*

---

## 3. Architecture Overview

### 3.1 Target topology

```text
Source (refresh time only)
  IPiutangOpenBalanceDal  → open rows + CustomerId        [EXTEND DTO + SQL]
    ↓
RefreshDashboardPiutangSnapshotWorker  (15 min, unchanged entry point)
    ↓
DashboardPiutangAggregator               [EXTEND]
  ├─ company KPIs + aging (existing)
  ├─ customer aging snapshot build     [NEW]
  ├─ Top 20 risk ranking               [NEW]
  └─ concentration %                   [NEW]
    ↓
BTRPD_PiutangKpi                       [EXTEND columns]
BTRPD_PiutangAging                     [UNCHANGED]
BTRPD_PiutangCustomerAging             [NEW — all customers with balance]
BTRPD_PiutangTopCustomerRisk           [NEW — Top 20 denormalized]
    ↓
GET /api/dashboard/piutang             [EXTEND response]
    ↓
PiutangDashboardView.vue               [EXTEND layout]

Unchanged in V2:
  GET /api/dashboard/overview          (home card: Total Piutang, Total Customer only)
  GET /api/dashboard/executive         (continues deriving >90 from aging buckets + Top-1 % from ranking)
  GET /api/dashboard/collection | customers | salesmen
  BTR Desktop FF1 / M10 Piutang Report
```

**Why extend Piutang domain (not new worker):**

- PO Q18 binds customer aging to Piutang refresh for reconciliation with `BTRPD_PiutangKpi` / `BTRPD_PiutangAging`.
- Single O(n) scan of open balances already occurs in `RefreshDashboardPiutangSnapshotWorker`.
- Customer aging is input to Top 20 and concentration — same transaction replace as company snapshot.

**Why persist full customer aging + Top 20 table:**

- PO forbids request-time scan of all open piutang rows.
- Full customer aging enables future Executive / Customer / Alert Center consumers without second worker (PO intent).
- Top 20 table avoids API read path sorting hundreds of customer rows on every page load.

### 3.2 Design principles

| Principle | Application |
| --- | --- |
| Extend, don't replace | Keep `BTRPD_PiutangAging`, worker, route, investigation wiring |
| Snapshot-only reads | No live aggregation on `GET /api/dashboard/piutang` |
| Reuse bucket boundaries | Extract shared resolver from aggregator (Section 5.3) |
| Strict dashboard charter | No Top Overdue table; label metrics clearly vs M20 |
| Preserve traceability | Total Piutang / Total Customer still reconcile to M10 footer |
| Minimize cross-domain churn | Do not change Customer/Collection workers in V2 |

### 3.3 Architecture decisions

| Decision | Choice | Rationale |
| --- | --- | --- |
| Customer aging storage | **`BTRPD_PiutangCustomerAging`** in Piutang domain | PO ownership; 15-min cadence; reconciliation with company KPI |
| Top 20 presentation | **`BTRPD_PiutangTopCustomerRisk`** denormalized | Fast API read; stable rank persistence |
| Legacy `BTRPD_PiutangTopCustomer` | **Stop writing; deprecate read** | Replaced by TopCustomerRisk; remove DAL read/write in same PR |
| KPI column storage | **Extend `BTRPD_PiutangKpi`** | Company-level concentration + overdue amount alongside existing KPIs |
| `AgingOver90` on KPI | **Persist** (not only derive at read) | Matches Executive pattern; simplifies API mapping |
| Shared bucket resolver | **`PiutangAgingBucketResolver` static helper** | Mitigates R4 drift; Piutang aggregator uses it in V2 |
| Other aggregators | **No refactor in V2** | Collection/Customer/Salesman keep local copy — document follow-up task |
| `CustomerId` on source DAL | **Add to `PiutangOpenBalanceDto` + SQL** | PO canonical snapshot key |
| Executive dashboard | **No V2 changes** | Executive already derives >90 from buckets; Top-1 % from rank-1 customer |
| Overview home card | **No V2 changes** | PO scope is analytical dashboard page only |
| API compatibility | **Additive fields + replace `TopCustomers` with `TopCustomerRisk`** | Portal SPA ships with API; coordinate FE/BE deploy |

---

## 4. Impact Analysis

### 4.1 Affected modules

| Layer | Module | Change type |
| --- | --- | --- |
| SQL | `Tables/ReportingContext/BTRPD_PiutangKpi.sql` | **Alter** — add KPI columns |
| SQL | `Tables/ReportingContext/BTRPD_PiutangCustomerAging.sql` | **New** |
| SQL | `Tables/ReportingContext/BTRPD_PiutangTopCustomerRisk.sql` | **New** |
| SQL | `Tables/ReportingContext/BTRPD_PiutangTopCustomer.sql` | **Deprecate** (stop writes; optional drop script post-cutover) |
| SQL | `Scripts/Upgrade_M14_PiutangDashboard_V2.sql` | **New** — idempotent ALTER + CREATE |
| SQL | `Scripts/Create_BTRPD_PortalDashboard_Tables.sql` | Update for greenfield installs |
| SQL | `btr.sql.sqlproj` | Include new tables |
| Application | `DashboardSnapshotAgg/Models/PiutangOpenBalanceDto.cs` | Add `CustomerId` |
| Application | `DashboardSnapshotAgg/Services/PiutangAgingBucketResolver.cs` | **New** |
| Application | `DashboardSnapshotAgg/Services/DashboardPiutangAggregator.cs` | **Extend** |
| Application | `DashboardSnapshotAgg/Models/DashboardPiutangAggregateResult.cs` | **Extend** |
| Application | `DashboardSnapshotAgg/Contracts/IDashboardPiutangSnapshotDal.cs` | **Extend** replace contract |
| Application | `DashboardPiutangAgg/Queries/GetDashboardPiutangQuery.cs` | **Extend** response DTOs |
| Infrastructure | `DashboardSnapshotAgg/PiutangOpenBalanceDal.cs` | SELECT `CustomerId` |
| Infrastructure | `DashboardSnapshotAgg/DashboardPiutangSnapshotDal.cs` | Read/write new tables |
| Infrastructure | `DashboardPiutangAgg/DashboardPiutangDal.cs` | Map new fields + investigation on risk rows |
| API | `Controllers/Dashboard/PiutangDashboardController.cs` | No route change — response shape grows |
| Frontend | `views/dashboard/PiutangDashboardView.vue` | KPI, concentration, Top 20 table |
| Frontend | `components/dashboard/PiutangCustomerRiskTable.vue` | **New** (or extend ranking table) |
| Frontend | `models/dashboard.ts`, `stores/dashboardStore.ts`, `api/dashboardApi.ts` | Type updates |
| Tests | `DashboardPiutangAggregatorTest.cs` | **Extend** |
| Tests | `DashboardPiutangSnapshotVerificationTest.cs` | **Extend** reconciliation |
| Tests | `DashboardPiutangDalTest.cs` | **Extend** |
| Tests | `DashboardExecutiveComposerTest.cs` | **Verify** still passes (rank-1 source change) |
| Docs | `docs/features/btr-portal/btr-portal-domain.md` | Piutang dashboard section |
| Docs | `docs/features/materialized-dashboard/materialized-dashboard-domain.md` | Piutang KPI catalog + new table |

### 4.2 Unaffected modules

| Module | Reason |
| --- | --- |
| `RefreshDashboardCollectionSnapshotWorker` and `BTRPD_Collection*` | Explicitly out of scope |
| `RefreshDashboardCustomerSnapshotWorker` and `BTRPD_Customer*` | No V2 coupling |
| M10 Piutang Report API / DAL | Drill-down only |
| BTR Desktop FF1 | No changes |
| `GET /api/dashboard/overview` | Home card metrics unchanged |

### 4.3 Metric traceability

| V2 field | Source at refresh | Validating reference |
| --- | --- | --- |
| Total Piutang | Open rows sum | M10 Piutang Report footer (all open) |
| Total Customer | Distinct keys | M10 footer customer count |
| Overdue Customer | Customers with non-Current exposure | Manual: filter customers with `JatuhTempo < today` |
| Overdue Piutang | Sum non-Current buckets | M20 Overdue Exposure (same definition, different dashboard) |
| Piutang > 90 Hari | `DaysOver90` bucket | Executive `AgingOver90Amount` after same refresh |
| Top 10 / 20 Customer % | Ranked customer totals | Manual spreadsheet from M10 export grouped by customer |
| Customer risk row Total | Per-customer bucket sum | M10 report filtered by customer |
| Aging pie | `BTRPD_PiutangAging` | Unchanged V1 reconciliation |

---

## 5. Database Design

Deploy with `SnapshotKey = 'CURRENT'` delete-and-replace pattern. Use ULID (`VARCHAR(26)`) for row IDs.

### 5.1 `BTRPD_PiutangKpi` (alter)

Add columns (defaults `0` / `NULL`):

| Column | Type | Description |
| --- | --- | --- |
| `OverduePiutang` | DECIMAL(18,2) NOT NULL DEFAULT 0 | Sum of overdue buckets |
| `AgingOver90Amount` | DECIMAL(18,2) NOT NULL DEFAULT 0 | `DaysOver90` bucket amount |
| `AgingOver90Percent` | DECIMAL(9,4) NULL | >90 ÷ Total Piutang × 100 |
| `Top10CustomerConcentrationPercent` | DECIMAL(9,4) NULL | Cumulative top-10 share |
| `Top20CustomerConcentrationPercent` | DECIMAL(9,4) NULL | Cumulative top-20 share |

Existing columns unchanged.

### 5.2 `BTRPD_PiutangCustomerAging` (new)

Full per-customer aging snapshot — one row per customer with open balance per refresh.

| Column | Type | Description |
| --- | --- | --- |
| `PiutangCustomerAgingId` | VARCHAR(26) PK | ULID |
| `SnapshotKey` | VARCHAR(10) | `'CURRENT'` |
| `CustomerId` | VARCHAR(13) | `BTR_Customer.CustomerId` |
| `CustomerCode` | VARCHAR(20) | Denormalized |
| `CustomerName` | VARCHAR(50) | Denormalized |
| `CurrentAmount` | DECIMAL(18,2) | Current bucket |
| `Aging30Amount` | DECIMAL(18,2) | 1–30 bucket |
| `Aging60Amount` | DECIMAL(18,2) | 31–60 bucket |
| `Aging90Amount` | DECIMAL(18,2) | 61–90 bucket |
| `AgingOver90Amount` | DECIMAL(18,2) | >90 bucket |
| `LastUpdate` | DATETIME | Refresh timestamp |

**Constraints:**

- Unique: `(SnapshotKey, CustomerId)`
- Index: `(SnapshotKey)` for replace/delete

**Replace pattern:** `DELETE FROM BTRPD_PiutangCustomerAging WHERE SnapshotKey = 'CURRENT'` then bulk INSERT.

### 5.3 `BTRPD_PiutangTopCustomerRisk` (new)

Top 20 denormalized rows for dashboard table and Executive rank-1 fallback.

| Column | Type | Description |
| --- | --- | --- |
| `PiutangTopCustomerRiskId` | VARCHAR(26) PK | ULID |
| `SnapshotKey` | VARCHAR(10) | `'CURRENT'` |
| `Rank` | INT | 1–20 |
| `CustomerId` | VARCHAR(13) | |
| `CustomerCode` | VARCHAR(20) | |
| `CustomerName` | VARCHAR(50) | |
| `TotalPiutang` | DECIMAL(18,2) | Sum of five buckets |
| `CurrentAmount` | DECIMAL(18,2) | |
| `Aging30Amount` | DECIMAL(18,2) | |
| `Aging60Amount` | DECIMAL(18,2) | |
| `Aging90Amount` | DECIMAL(18,2) | |
| `AgingOver90Amount` | DECIMAL(18,2) | |

Unique: `(SnapshotKey, Rank)`

### 5.4 `BTRPD_PiutangTopCustomer` (deprecate)

- V2 worker **stops writing** to this table.
- `DashboardPiutangSnapshotDal` **stops reading** it.
- Keep table in DB for one release optional; remove from sqlproj greenfield script after cutover.
- `DashboardExecutiveComposer` rank-1 % reads from `TopCustomerRisk` rank 1 via aggregate result.

### 5.5 Upgrade script

`Scripts/Upgrade_M14_PiutangDashboard_V2.sql`:

1. `IF COL_LENGTH` guards for KPI ALTERs.
2. `IF OBJECT_ID` guards for CREATE TABLE.
3. No data backfill required — next Piutang refresh populates new columns/tables.

---

## 6. Application Design

### 6.1 `PiutangAgingBucketResolver` (new)

Extract from `DashboardPiutangAggregator`:

```csharp
public static class PiutangAgingBucketResolver
{
    public static readonly (string Key, string Label, int SortOrder)[] BucketDefinitions = ...;

    public static string ResolveBucketKey(DateTime jatuhTempo, DateTime today);
    public static int ResolveDaysOverdue(DateTime jatuhTempo, DateTime today);
}
```

`DashboardPiutangAggregator` delegates to this class. **Do not** refactor Collection/Customer/Salesman aggregators in V2.

### 6.2 `PiutangOpenBalanceDto` + DAL

Add:

```csharp
public string CustomerId { get; set; }
```

SQL addition:

```sql
ISNULL(ee.CustomerId, '') AS CustomerId
```

### 6.3 `DashboardPiutangAggregator` extensions

**Constants:**

```csharp
public const int TopCustomerRiskCount = 20;
public const int TopConcentration10 = 10;
public const int TopConcentration20 = 20;
```

**Processing order (single pass over open rows):**

1. Company KPIs + aging buckets (existing).
2. `Dictionary<string, CustomerAgingAccumulator>` keyed by `CustomerId` (skip blank CustomerId for customer snapshot; still include row in company totals).
3. For each row: resolve bucket; add `KurangBayar` to company bucket and customer bucket column.
4. After loop:
   - Compute `OverduePiutang`, `AgingOver90Amount`, `AgingOver90Percent`.
   - Materialize `CustomerAging` list with `LastUpdate = generatedAt`.
   - Rank customers by `TotalPiutang` DESC, name ASC; take 20 → `TopCustomerRisk`.
   - `Top10CustomerConcentrationPercent` = sum(top 10 totals) / Total Piutang × 100.
   - `Top20CustomerConcentrationPercent` = sum(top 20 totals) / Total Piutang × 100.

**`DashboardPiutangAggregateResult` new properties:**

| Property | Type |
| --- | --- |
| `OverduePiutang` | decimal |
| `AgingOver90Amount` | decimal |
| `AgingOver90Percent` | decimal? |
| `Top10CustomerConcentrationPercent` | decimal? |
| `Top20CustomerConcentrationPercent` | decimal? |
| `CustomerAging` | `List<DashboardPiutangCustomerAgingRow>` |
| `TopCustomerRisk` | `List<DashboardPiutangTopCustomerRiskRow>` |

Remove / obsolete: `TopCustomers` (replace with `TopCustomerRisk`).

### 6.4 `DashboardPiutangSnapshotDal`

**`ReplaceCurrent` transaction order:**

1. DELETE `BTRPD_PiutangAging`, `BTRPD_PiutangCustomerAging`, `BTRPD_PiutangTopCustomerRisk` WHERE `SnapshotKey = 'CURRENT'`.
2. MERGE `BTRPD_PiutangKpi` (include new columns).
3. INSERT aging, customer aging, top risk rows.

**`GetCurrent`:** Read KPI + aging + top risk; do not read deprecated `BTRPD_PiutangTopCustomer`.

### 6.5 API response (`DashboardPiutangResponse`)

**New top-level fields:**

| Field | Type |
| --- | --- |
| `OverduePiutang` | decimal |
| `AgingOver90Amount` | decimal |
| `AgingOver90Percent` | decimal? |
| `Top10CustomerConcentrationPercent` | decimal? |
| `Top20CustomerConcentrationPercent` | decimal? |
| `TopCustomerRisk` | array |

**`DashboardPiutangTopCustomerRiskRow`:**

| Field | Type |
| --- | --- |
| `Rank` | int |
| `CustomerCode` | string |
| `CustomerName` | string |
| `TotalPiutang` | decimal |
| `CurrentAmount` | decimal |
| `Aging30Amount` | decimal |
| `Aging60Amount` | decimal |
| `Aging90Amount` | decimal |
| `AgingOver90Amount` | decimal |
| `Investigation` | `InvestigationMetadata` |

**Remove:** `TopCustomers` from API response (breaking — update SPA in same release).

`DashboardPiutangDal.MapTopCustomerRisk` — same investigation builder as today (`InvestigationRegistry.SignalLegacyTopCustomer`).

### 6.6 Executive composer adjustment

`DashboardExecutiveComposer.ComposePiutang` today reads `piutang.TopCustomers` rank 1.

**Change:** Read `piutang.TopCustomerRisk` rank 1 for `TopCustomerPercent` numerator. `AgingOver90` can use persisted `AgingOver90Amount` from KPI when available (optional micro-optimization); deriving from buckets remains valid.

---

## 7. Frontend Design

### 7.1 `PiutangDashboardView.vue`

**KPI row** — CSS grid `repeat(5, 1fr)` on desktop; stack on mobile:

| Tile | Display |
| --- | --- |
| Total Piutang | Currency |
| Total Customer | Integer |
| Overdue Customer | Integer |
| Overdue Piutang | Currency |
| Piutang > 90 Hari | Currency primary; muted `%` subtitle from `AgingOver90Percent` |

**Concentration row** — two tiles or compact metric pair:

| Tile | Display |
| --- | --- |
| Top 10 Customer % | `formatPercent(Top10CustomerConcentrationPercent)` |
| Top 20 Customer % | `formatPercent(Top20CustomerConcentrationPercent)` |

**Aging chart** — `AgingPieChart` unchanged.

**Top 20 table** — new `PiutangCustomerRiskTable.vue`:

- PrimeVue `DataTable`, striped, horizontal scroll on narrow viewports.
- Currency format for amount columns.
- Row click → `navigateToInvestigation` (same as V1 Top 10).
- Column headers: Customer · Total · Current · 1–30 · 31–60 · 61–90 · >90.

### 7.2 Types (`dashboard.ts`)

Replace `DashboardPiutangTopCustomer` usage on piutang page with `DashboardPiutangTopCustomerRiskRow`. Update `DashboardPiutangResponse` interface.

### 7.3 UX copy

| Element | Text |
| --- | --- |
| Page subtitle | *Portfolio quality snapshot — all open receivables. Collection recovery metrics are on the Collection Dashboard.* |
| Top 20 title | **Top 20 Outstanding Customers — Aging Breakdown** |
| Concentration helper | *Share of total open piutang held by largest customers.* |

---

## 8. Test Strategy

### 8.1 Unit tests — `DashboardPiutangAggregatorTest`

| Case | Assertion |
| --- | --- |
| `OverduePiutang` | Equals Total − Current bucket |
| `AgingOver90Percent` | Correct ratio; null when Total = 0 |
| Customer aging rows | Per-customer buckets sum to customer total |
| Company reconciliation | Sum of customer totals = Total Piutang |
| Top 20 ranking | Correct order and tie-break |
| Top 10/20 concentration | Manual sum of ranked totals / Total |
| Rows with `KurangBayar ≤ 1` | Excluded everywhere |
| Bucket boundaries | Existing theory cases preserved |

### 8.2 Integration — `DashboardPiutangSnapshotVerificationTest`

After refresh against test DB:

- KPI reconciliation invariants (Section 2.2).
- Row count `BTRPD_PiutangCustomerAging` = distinct customers with open balance (with CustomerId).
- `BTRPD_PiutangTopCustomerRisk` row count ≤ 20.

### 8.3 API — `DashboardPiutangDalTest`

- Maps all new fields.
- Investigation metadata present on risk rows.

### 8.4 Regression

- `DashboardExecutiveComposerTest` — still composes piutang attention with rank-1 %.
- `npm run build` for portal SPA.

### 8.5 Manual verification checklist

| # | Step |
| - | ---- |
| V1 | Run `btr.portal.worker --domain Piutang`; confirm refresh success in `BTRPD_RefreshLog` |
| V2 | Open `/dashboard/piutang` — 5 KPIs + 2 concentration metrics populated |
| V3 | Aging pie unchanged; bucket sum = Total Piutang |
| V4 | Top 20 table shows aging columns; row click opens Piutang Report |
| V5 | Compare Total Piutang to M10 report footer (all open) |
| V6 | Confirm Collection Dashboard has no duplicate Top 20 total-balance table |
| V7 | `GeneratedAt` displayed (layout component) |

---

## 9. Implementation Phases

### Phase 1 — Database

1. Add table definitions + upgrade script.
2. Deploy to dev/test.
3. Update `btr.sql.sqlproj`.

### Phase 2 — Backend aggregation

1. `PiutangAgingBucketResolver`.
2. Extend DTO + `PiutangOpenBalanceDal`.
3. Extend aggregator + aggregate models.
4. Extend snapshot DAL read/write.
5. Extend API response + `DashboardPiutangDal`.
6. Adjust `DashboardExecutiveComposer` rank-1 source.
7. Unit + integration tests.

### Phase 3 — Frontend

1. Update TypeScript models + store.
2. KPI + concentration layout in `PiutangDashboardView.vue`.
3. `PiutangCustomerRiskTable.vue`.
4. Build + manual V1–V7.

### Phase 4 — Knowledge sync

1. Update `docs/features/btr-portal/btr-portal-domain.md` Piutang Dashboard section.
2. Update `docs/features/materialized-dashboard/materialized-dashboard-domain.md` Piutang rules + table list.
3. Add `implementation-summary.md` when complete.

---

## 10. Risks and Mitigations

| ID | Risk | Severity | Mitigation |
| -- | ---- | -------- | ---------- |
| R1 | Customer aging row count growth | Medium | Snapshot replace pattern; index on SnapshotKey; monitor refresh duration in `BTRPD_RefreshLog` |
| R2 | Rows missing `CustomerId` | Low | Company KPIs still include; log skipped count in worker progress; investigate data quality separately |
| R3 | Metric confusion with M20/M17 | Medium | Clear labels; no Top Overdue table; subtitle cross-link |
| R4 | Duplicated bucket logic elsewhere | Medium | `PiutangAgingBucketResolver` for Piutang; backlog to unify other aggregators |
| R5 | API breaking change (`TopCustomers` removed) | Low | Ship API + SPA together |
| R6 | FF1 period vs all-open scope | Medium | Retain subtitle hint on drill-down (existing pattern) |
| R7 | Refresh CPU increase | Medium | Single-pass aggregator; acceptable per analysis §7.3 — monitor Phase 1 dev refresh timing |
| R8 | Stale snapshot distrust | Low | `GeneratedAt` unchanged |

---

## 11. Deferred Work (post-V2)

Document for future milestone — **do not implement in V2**:

| Topic | PO decision |
| --- | --- |
| Monthly trends | 12-month **snapshot retention** |
| Trend metrics | Total Piutang · Overdue Piutang · Piutang > 90 Hari |
| Trend approach | Never replay transactions |
| Cross-dashboard consumers | Executive, Customer Analytics, Alert Center reading `BTRPD_PiutangCustomerAging` |
| Unified `ResolveAgingBucketKey` | Refactor Collection/Customer/Salesman aggregators to shared resolver |
| Drop `BTRPD_PiutangTopCustomer` | After one release without readers |

---

## 12. Implementer Checklist

- [ ] SQL: alter `BTRPD_PiutangKpi`, create `BTRPD_PiutangCustomerAging`, create `BTRPD_PiutangTopCustomerRisk`, upgrade script
- [ ] `PiutangOpenBalanceDto` + DAL: `CustomerId`
- [ ] `PiutangAgingBucketResolver` + refactor `DashboardPiutangAggregator`
- [ ] Customer aging + Top 20 + concentration in aggregator
- [ ] `DashboardPiutangSnapshotDal` read/write
- [ ] API DTOs + `DashboardPiutangDal` mapping
- [ ] `DashboardExecutiveComposer` rank-1 source
- [ ] Unit / integration tests
- [ ] `PiutangDashboardView.vue` + `PiutangCustomerRiskTable.vue`
- [ ] `dashboard.ts` types
- [ ] Manual verification V1–V7
- [ ] Feature docs updated
- [ ] `implementation-summary.md` written

---

*End of implementation plan. Ready for Implementer.*
