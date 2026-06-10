# Implementation Plan: M19 — Slow Moving & Dead Stock Dashboard

## Document Status

| Field | Value |
| --- | --- |
| Initiative | M19 Slow Moving & Dead Stock — **Which inventory requires management attention and why?** |
| Authoritative requirements | `docs/work/btr-portal/M19 Slow Moving & Dead Stock - Analysis.md` — **Section 12 (Product Owner Decisions)** |
| Related features | `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-operational.md`, `docs/features/btr-portal/btr-portal-architecture.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md` |
| Reference pattern | M17 Customer Analytics snapshot domain + M15 Inventory aggregation rules + M14 Piutang aging pie UX |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |
| Status | **Ready for implementation** |
| Product decisions | **Confirmed** — analysis Section 12, 2026-06-08 |

---

## 1. Goal

Deliver **Slow Moving & Dead Stock Dashboard** at `/dashboard/inventory-risk` — a dedicated inventory-health view to answer *Which inventory requires management attention and why?*

**Primary outcomes:**

- New route `/dashboard/inventory-risk` with page title **Slow Moving & Dead Stock Dashboard** for all authenticated users (no role-based routing).
- **Dedicated Inventory Risk snapshot domain** (`BTRPD_InventoryRisk*`) with its own refresh worker — materialized KPIs, not live composition.
- **Proposal A layout** (fixed section order): Attention Cards → Aging Distribution → Category/Supplier Risk Exposure → Attention List → Top 10 Rankings → Navigation.
- Mandatory headline KPIs: **Dead Stock Item Count**, **Dead Stock Value**, **Slow Moving Item Count**, **Slow Moving Value**, **At-Risk Inventory %**.
- **Attention List** at item × signal grain with approved signals: **Dead Stock** · **Slow Moving** · **Never Sold**.
- Item row click → Inventory Report with **item name/code pre-filter** (`?q=`).
- Navigation path: **Inventory Risk → Inventory Dashboard → Inventory Report** (M16/M17 pattern).
- **Supplements** existing Inventory Dashboard (`/dashboard/inventory`); does **not** replace composition analytics.

**Phase 2 (post-M19 dashboard delivery — PO Q2):**

- Extend Executive Dashboard inventory section with **Dead Stock Value**, **At-Risk Inventory %**, and **Inventory Risk Attention Indicator** while preserving Total Inventory Value, Top Category %, Top Supplier %.

**Explicitly out of scope (PO confirmed):**

- Salesman dimension, ABC classification, warehouse breakdown, StokBalanceHealth, export, Kartu Stok drill-down.
- New item-level sales report route, retur-adjusted demand, mutasi-based movement classification for portal KPIs.
- Changes to existing Inventory snapshot tables (`BTRPD_Inventory*`) or `GET /api/dashboard/inventory`.

---

## 2. Authoritative Product Decisions

Source: analysis Section 12. Do not re-decide these rules during implementation.

### 2.1 Positioning and audience

| Decision | Value |
| --- | --- |
| Route | `/dashboard/inventory-risk` |
| Page title | **Slow Moving & Dead Stock Dashboard** |
| Audience | All authenticated users — no RBAC in M19 |
| Sidebar | Dashboard → **Inventory Risk** (after Inventory) |
| Inventory Dashboard relationship | **Supplements** `/dashboard/inventory` — does not replace (Q3) |
| Executive relationship | **Phase 2 only** — promote risk KPIs after M19 dashboard ships (Q2) |

### 2.2 Classification rules (authoritative signal = Last Faktur Date)

| Class | Rule | Valuation |
| --- | --- | --- |
| **Never Sold** | Aggregated `Qty > 0`; **no** non-void `FakturItem` history | `Hpp × Qty` (BrgId-first, exclude In-Transit) — **separate signal (Q11)** |
| **Slow Moving** | Aggregated `Qty > 0`; `LastFakturDate` exists AND idle **≥ 90 days** and **< 180 days** | Same valuation |
| **Dead Stock** | Aggregated `Qty > 0`; `LastFakturDate` exists AND idle **≥ 180 days** | Same valuation |
| **Active** | Aggregated `Qty > 0`; `LastFakturDate` within last **89 days** | Not counted in at-risk KPIs |

**Supporting rules (Q8–Q10, Q13–Q17):**

- Authoritative movement signal = **`MAX(FakturDate)` per `BrgId`** from non-void Faktur / FakturItem (`VoidDate = '3000-01-01'`).
- **Gross Faktur only** — retur does not affect classification.
- Non-sales outflows (mutasi, opname, adjust) **do not reset** the aging clock.
- **Company-wide** BrgId-first aggregation; **exclude In-Transit** warehouse; valuation = **`BTR_Brg.Hpp`** on balance rows.
- **No minimum qty/value floor** for V1 (Q12).

### 2.3 Architect classification decisions (resolved for implementer)

Analysis Section 12 left two counting ambiguities. This plan resolves them:

| Decision | Choice | Rationale |
| --- | --- | --- |
| Slow Moving vs Dead Stock overlap | **Mutually exclusive KPI counts** — Slow = 90–179 days idle; Dead = ≥ 180 days | PO requires both KPI families separately (Q18); avoids double-counting item counts |
| Never Sold vs Slow/Dead | **Never Sold excluded** from Slow Moving and Dead Stock counts and Top 10 dead/slow tables | Q11 — separate signal; items without Faktur history cannot have `LastFakturDate` |
| **At-Risk Inventory %** numerator | Sum of inventory value for items in **Never Sold ∪ Slow Moving ∪ Dead Stock** (disjoint sets) | Reconciles with headline KPI values without double-counting capital |
| **At-Risk Inventory %** denominator | **`TotalInventoryValue`** from same BrgId-first pipeline as M15 | Q33 — must match Inventory Dashboard and Inventory Report footer |
| Aging pie buckets | **Four buckets** aligned to thresholds (see Section 5.2) | Q19 — first inventory aging model; mirrors Piutang pie UX |

**Idle days calculation:**

```text
idleDays = (today.Date - LastFakturDate.Date).Days   // when LastFakturDate exists
```

Items with `LastFakturDate` on `today − 89` are **Active** (excluded from Slow/Dead). Items on `today − 90` are **Slow Moving**.

### 2.4 Attention rules and presentation

**Approved attention list signals (Q20):** DeadStock · SlowMoving · NeverSold

| Signal | Inclusion rule |
| --- | --- |
| **NeverSold** | Item in Never Sold class |
| **SlowMoving** | Item in Slow Moving class (90–179 days) |
| **DeadStock** | Item in Dead Stock class (≥ 180 days) |

- **One row per item × signal** — an item appears in at most one signal row (classes are mutually exclusive).
- **Generic Attention Indicator** (M16–M18 pattern) — no automated Healthy/Warning/Critical bands on individual KPIs (Q23).
- **`RequiresAttention`** on inventory-risk attention cards: `true` when `AtRiskInventoryPercent > 0` OR any headline at-risk count > 0.

### 2.5 Layout (Proposal A — fixed order, Q19–Q22)

1. Inventory Attention Cards (headline KPIs + Attention Indicator)  
2. Inventory Aging Distribution (pie chart — mandatory)  
3. Category Risk Exposure (horizontal bar — at-risk value by category)  
4. Supplier Risk Exposure (horizontal bar — at-risk value by supplier/principal)  
5. Inventory Attention List (item × signal)  
6. Top 10 Dead Stock by Value \| Top 10 Slow Moving by Value  
7. Navigation to Inventory Dashboard and Inventory Report  

### 2.6 Drill-down

| Action | Target |
| --- | --- |
| Attention list row / Top 10 row | `/reports/inventory?q={BrgName or BrgCode}` |
| Domain navigation links | `/dashboard/inventory`, `/reports/inventory` |

Pre-filter uses existing report free-text search (`useReportFreeTextFilter`) — no report API changes (Q30–Q32).

### 2.7 Data sourcing

| Decision | Value |
| --- | --- |
| Snapshot domain | **`BTRPD_InventoryRisk*`** (Q25) |
| Refresh cadence | **60 minutes** — `InventoryRiskIntervalMinutes` (Q26) |
| Read path | **Snapshot-only** — no live aggregation on API request (Q27) |
| Last Faktur access | **`IBrgLastFakturDal`** following `CustomerLastFakturDal` pattern (Q28) |
| Faktur history scan | Full `FakturItem` aggregate acceptable at refresh (Q29) |

---

## 3. Architecture Overview

### 3.1 Target topology

```text
Source DALs (read at refresh time — NOT from Inventory Risk snapshot for position rules)
  IStokBalanceViewDal           → balance rows (position, dimensions, HPP × Qty)
  IBrgLastFakturDal               → MAX(FakturDate) per BrgId (all history)     [NEW]
    ↓
RefreshDashboardInventoryRiskSnapshotWorker
    ↓ DashboardInventoryRiskAggregator
      (reuses BuildItemGroups rules from DashboardInventoryAggregator)
BTRPD_InventoryRisk* tables (6)
    ↓
Browser → GET /api/dashboard/inventory-risk
    ↓ MediatR
GetDashboardInventoryRiskHandler
    ↓ IDashboardInventoryRiskDal
DashboardInventoryRiskDal → DashboardInventoryRiskResponse

Existing unchanged in Phase 1:
  GET /api/dashboard/inventory
  GET /api/dashboard/executive
  BTRPD_Inventory* worker and tables

Phase 2 (Executive):
  GetDashboardExecutiveHandler loads Inventory Risk KPI snapshot
    ↓ DashboardExecutiveComposer.ComposeInventory (extended)
  GET /api/dashboard/executive — adds DeadStockValue, AtRiskInventoryPercent, risk RequiresAttention
```

**Why read source DALs, not Inventory snapshot tables:**

- M15 inventory snapshot stores **composition only** — no last-sale, aging, or risk fields.
- Risk classification requires **full FakturItem history aggregate** not present in any existing snapshot.
- Position rules must be computed from `StokBalanceView` with the **same BrgId-first pipeline** as `DashboardInventoryAggregator` to guarantee denominator reconciliation (Q33).
- Keeps Inventory Risk domain self-contained; Inventory composition worker remains independent.

### 3.2 Design principles

| Principle | Application |
| --- | --- |
| Reuse position rules | Extract or delegate `BuildItemGroups` logic from `DashboardInventoryAggregator` — same In-Transit, BrgId-first, zero-qty, Unknown dimension rules |
| Dedicated snapshot | Risk KPIs materialized in own tables (Q25) |
| Do not compose at read time | All derivations in aggregator at refresh (Q27) |
| Preserve Inventory Dashboard | `/dashboard/inventory` and `GET /api/dashboard/inventory` unchanged |
| Last Faktur in Infrastructure only | SQL lives in `BrgLastFakturDal`; Application receives DTOs |
| Attention UX reuse | Adapt M17 `ExecutiveAttentionCard` / attention list patterns; reuse `AgingPieChart.vue`, `InventoryHorizontalBarChart.vue`, `Top10RankingTable.vue` |
| Fail gracefully | Empty/missing snapshot → unavailable sections with clear UI message |

### 3.3 Architecture decisions

| Decision | Choice | Rationale |
| --- | --- | --- |
| Snapshot approach | **New Inventory Risk domain** with 6 tables | PO Q25; movement/aging not representable in M15 tables |
| Position aggregation | **Shared constants/methods** with `DashboardInventoryAggregator` | Prevent denominator drift vs M15; consider private shared helper or base class — implementer must not duplicate divergent SQL |
| Last Faktur DAL | **`IBrgLastFakturDal.ListLastFakturByBrg()`** | Efficient `MAX(FakturDate)` at item grain; Q28–Q29 |
| Never Sold detection | BrgIds with stock **not present** in last-Faktur result set | Anti-join in aggregator — no second history scan required |
| Refresh cadence | **60 minutes** (`InventoryRiskIntervalMinutes = 60`) | PO Q26; aligns with existing `InventoryIntervalMinutes` |
| RefreshAll order | Piutang → Inventory → **InventoryRisk** → Sales → Purchasing → Customer → Salesman | InventoryRisk after Inventory composition refresh; independent inputs but similar DAL |
| Read API | **`GET /api/dashboard/inventory-risk`** | Mirrors domain dashboard pattern |
| Executive integration | **Phase 2** separate PR/delivery step | PO Q2 — dashboard first, executive promotion second |
| Top N | **10** for dead/slow rankings and dimension bars | Consistent with M15 Top 10 pattern |
| Attention list sort | DeadStock → SlowMoving → NeverSold; then by `InventoryValue` desc; then `BrgName` | Highest capital impact first within signal |
| Staleness banner | Inventory Risk `GeneratedAt` vs `InventoryRiskIntervalMinutes` | Same copy as M16: **"⚠ Dashboard Data Not Fresh"** |

---

## 4. Impact Analysis

### 4.1 Affected modules

| Layer | Module | Change type |
| --- | --- | --- |
| SQL | `btr.sql/Tables/ReportingContext/BTRPD_InventoryRisk*.sql` (6 tables) | **New** |
| SQL | `btr.sql.sqlproj` | Include new tables |
| Application | `DashboardSnapshotAgg/Services/DashboardInventoryRiskAggregator.cs` | **New** |
| Application | `DashboardSnapshotAgg/Models/DashboardInventoryRisk*.cs` | **New** aggregate models |
| Application | `DashboardSnapshotAgg/Contracts/IDashboardInventoryRiskSnapshotDal.cs` | **New** |
| Application | `DashboardSnapshotAgg/UseCases/RefreshDashboardInventoryRiskSnapshotWorker.cs` | **New** |
| Application | `DashboardSnapshotAgg/DashboardSnapshotOptions.cs` | Add `InventoryRiskIntervalMinutes` |
| Application | `DashboardSnapshotAgg/UseCases/RefreshAllDashboardSnapshotsWorker.cs` | Register InventoryRisk worker |
| Application | `DashboardSnapshotAgg/Commands/RefreshDashboardSnapshotsCommand.cs` | Add InventoryRisk domain |
| Application | `DashboardInventoryRiskAgg/` (query, contracts, DTOs) | **New** read-side aggregate |
| Application | `SalesContext/FakturInfo/IBrgLastFakturDal.cs` | **New** contract + DTO |
| Infrastructure | `DashboardSnapshotAgg/DashboardInventoryRiskSnapshotDal.cs` | **New** |
| Infrastructure | `DashboardInventoryRiskAgg/DashboardInventoryRiskDal.cs` | **New** |
| Infrastructure | `SalesContext/FakturInfoAgg/BrgLastFakturDal.cs` | **New** |
| API | `Controllers/Dashboard/InventoryRiskDashboardController.cs` | **New** |
| API | `HealthController.cs` | Add InventoryRisk domain |
| API | DI registrations | New DALs, worker, aggregator |
| Worker | `btr.portal.worker/Program.cs` | Add `InventoryRisk` to `--domain` |
| Frontend | `router/index.ts` | Route `/dashboard/inventory-risk` |
| Frontend | `layouts/MainLayout.vue` | Sidebar **Inventory Risk** item |
| Frontend | `views/dashboard/InventoryRiskDashboardView.vue` | **New** |
| Frontend | `components/dashboard/InventoryRisk*.vue` | **New** section components |
| Frontend | `models/dashboard.ts`, `api/dashboardApi.ts`, `stores/dashboardStore.ts` | Inventory Risk types and loader |
| Frontend | `views/reports/InventoryReportView.vue` | Read `?q=` on mount if not already present |
| Tests | `btr.test/ReportingContext/DashboardInventoryRiskAggregatorTest.cs` | **New** unit tests |
| **Phase 2** | `DashboardExecutiveComposer`, `GetDashboardExecutiveQuery`, executive frontend | Extend inventory attention |
| Docs | Post-delivery feature docs | Operational, domain, architecture updates |

### 4.2 Unaffected modules

| Module | Reason |
| --- | --- |
| `DashboardInventoryAggregator`, `BTRPD_Inventory*` | M19 is additive; composition dashboard unchanged |
| `KartuStokSummaryDal`, Desktop IF8 | Validation path only; not authoritative for portal classification |
| Sales/Piutang/Customer/Salesman domains | No cross-domain data overlap |
| BTR Desktop | No changes |

### 4.3 Metric traceability

| Dashboard field | Source at refresh | Rule |
| --- | --- | --- |
| TotalInventoryValue | `IStokBalanceViewDal` via shared item groups | Same as M15 — reconciliation denominator |
| DeadStockItemCount | Classified item groups | `LastFakturDate ≤ today − 180` |
| DeadStockValue | Same subset | `SUM(Hpp × Qty)` |
| SlowMovingItemCount | Classified item groups | `today − 180 < LastFakturDate ≤ today − 90` |
| SlowMovingValue | Same subset | `SUM(Hpp × Qty)` |
| NeverSoldItemCount | Item groups with no `LastFakturDate` | Anti-join vs `IBrgLastFakturDal` |
| NeverSoldValue | Same subset | `SUM(Hpp × Qty)` |
| AtRiskInventoryValue | Union of three disjoint classes | Sum of Never + Slow + Dead values |
| AtRiskInventoryPercent | Composed | `AtRiskInventoryValue / TotalInventoryValue × 100` |
| Aging pie buckets | Item groups by idle class | Four buckets — value sums |
| Category/Supplier at-risk bars | Roll up at-risk value by dimension | Top 10 by at-risk value desc |
| Top 10 Dead Stock | Dead class ranked by value | Exclude Never Sold |
| Top 10 Slow Moving | Slow class ranked by value | Exclude Never Sold and Dead |
| Attention list | One row per classified item | Signal = class name |
| DaysSinceLastFaktur | `(today − LastFakturDate).Days` | Null/absent for Never Sold — display em dash or "Never" |

**Reconciliation notes:**

- `TotalInventoryValue` on M19 must equal Inventory Dashboard KPI and Inventory Report footer (within same refresh window).
- At-risk value sum must equal NeverSoldValue + SlowMovingValue + DeadStockValue.
- Desktop Kartu Stok Summary may differ — portal uses Last Faktur Date per PO Q8; document in operational guide.

---

## 5. Database Design

Deploy all tables with `SnapshotKey = 'CURRENT'` delete-and-replace pattern (consistent with existing dashboard snapshots).

### 5.1 `BTRPD_InventoryRiskKpi`

Single row per refresh — headline metrics for attention cards.

| Column | Type | Description |
| --- | --- | --- |
| SnapshotKey | VARCHAR(10) PK | `'CURRENT'` |
| GeneratedAt | DATETIME | Refresh timestamp |
| TotalInventoryValue | DECIMAL(18,2) | M15-equivalent denominator |
| TotalItem | INT | Active SKU count (qty > 0) |
| DeadStockItemCount | INT | ≥ 180 days idle |
| DeadStockValue | DECIMAL(18,2) | |
| SlowMovingItemCount | INT | 90–179 days idle |
| SlowMovingValue | DECIMAL(18,2) | |
| NeverSoldItemCount | INT | No Faktur history |
| NeverSoldValue | DECIMAL(18,2) | |
| AtRiskInventoryValue | DECIMAL(18,2) | Sum of three disjoint classes |
| AtRiskInventoryPercent | DECIMAL(9,4) NULL | AtRisk / TotalInventoryValue |
| RequiresAttention | BIT | Generic attention flag |
| LastRefreshLogId | VARCHAR(13) | FK to refresh log |

### 5.2 `BTRPD_InventoryRiskAging`

| Column | Type | Description |
| --- | --- | --- |
| InventoryRiskAgingId | VARCHAR(13) PK | Generated ID |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| BucketKey | VARCHAR(20) | `Active` \| `SlowMoving` \| `DeadStock` \| `NeverSold` |
| BucketLabel | VARCHAR(50) | Display label |
| InventoryValue | DECIMAL(18,2) | Sum Hpp × Qty in bucket |
| ItemCount | INT | Distinct BrgId count |
| SortOrder | INT | 1–4 |

Unique: `(SnapshotKey, BucketKey)`

**Bucket definitions (architect proposal — PO sign-off via this plan):**

| BucketKey | Label | Rule |
| --- | --- | --- |
| Active | Active (≤ 90 days) | `LastFakturDate > today − 90` |
| SlowMoving | Slow Moving (91–180 days) | `today − 180 < LastFakturDate ≤ today − 90` |
| DeadStock | Dead Stock (> 180 days) | `LastFakturDate ≤ today − 180` |
| NeverSold | Never Sold | No FakturItem history |

Pie chart binds to `InventoryValue` per bucket (same as Piutang aging pie uses amount).

### 5.3 `BTRPD_InventoryRiskAttention`

| Column | Type | Description |
| --- | --- | --- |
| InventoryRiskAttentionId | VARCHAR(13) PK | Generated ID |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| BrgId | VARCHAR(13) | Internal key |
| BrgCode | VARCHAR(20) | Display / drill-down |
| BrgName | VARCHAR(100) | Display / drill-down |
| KategoriName | VARCHAR(50) | |
| SupplierName | VARCHAR(50) | |
| Qty | INT | Aggregated qty |
| InventoryValue | DECIMAL(18,2) | Hpp × Qty |
| DaysSinceLastFaktur | INT NULL | NULL for Never Sold |
| SignalKey | VARCHAR(20) | `DeadStock` \| `SlowMoving` \| `NeverSold` |
| SignalLabel | VARCHAR(50) | Display label |
| SortOrder | INT | Stable list ordering |

Index: `(SnapshotKey, SortOrder)`

### 5.4 `BTRPD_InventoryRiskTopDead`

| Column | Type | Description |
| --- | --- | --- |
| InventoryRiskTopDeadId | VARCHAR(13) PK | Generated ID |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| Rank | INT | 1–10 |
| BrgId | VARCHAR(13) | |
| BrgCode | VARCHAR(20) | |
| BrgName | VARCHAR(100) | |
| KategoriName | VARCHAR(50) | |
| SupplierName | VARCHAR(50) | |
| Qty | INT | |
| InventoryValue | DECIMAL(18,2) | |
| DaysSinceLastFaktur | INT | |
| PercentOfAtRisk | DECIMAL(9,4) NULL | Row value / DeadStockValue |

Unique: `(SnapshotKey, Rank)`

### 5.5 `BTRPD_InventoryRiskTopSlow`

Same schema as TopDead — populated from Slow Moving class; `PercentOfAtRisk` uses `SlowMovingValue` denominator.

### 5.6 `BTRPD_InventoryRiskBreakdown`

| Column | Type | Description |
| --- | --- | --- |
| InventoryRiskBreakdownId | VARCHAR(13) PK | Generated ID |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| DimensionType | VARCHAR(20) | `Category` \| `Supplier` |
| Name | VARCHAR(50) | Dimension label |
| AtRiskValue | DECIMAL(18,2) | Sum at-risk item values |
| ItemCount | INT | Distinct at-risk items |
| Rank | INT | 1–10 within dimension |
| PercentOfAtRisk | DECIMAL(9,4) NULL | Row / total at-risk value |

Unique: `(SnapshotKey, DimensionType, Rank)`

---

## 6. Backend Implementation

### 6.1 New folder structure

```text
btr.application/ReportingContext/
├── DashboardSnapshotAgg/
│   ├── Services/
│   │   └── DashboardInventoryRiskAggregator.cs
│   ├── Models/
│   │   └── DashboardInventoryRiskAggregateResult.cs (+ nested row types)
│   ├── Contracts/
│   │   └── IDashboardInventoryRiskSnapshotDal.cs
│   └── UseCases/
│       ├── RefreshDashboardInventoryRiskSnapshotWorker.cs
│       └── RefreshDashboardInventoryRiskSnapshotRequest.cs (+ Result)
└── DashboardInventoryRiskAgg/
    ├── Contracts/
    │   └── IDashboardInventoryRiskDal.cs
    └── Queries/
        └── GetDashboardInventoryRiskQuery.cs

btr.application/SalesContext/FakturInfo/
└── IBrgLastFakturDal.cs

btr.infrastructure/ReportingContext/
├── DashboardSnapshotAgg/
│   └── DashboardInventoryRiskSnapshotDal.cs
└── DashboardInventoryRiskAgg/
    └── DashboardInventoryRiskDal.cs

btr.infrastructure/SalesContext/FakturInfoAgg/
└── BrgLastFakturDal.cs

btr.portal.api/Controllers/Dashboard/
└── InventoryRiskDashboardController.cs
```

Add all new `.cs` files to respective `.csproj` Compile includes.

### 6.2 `IBrgLastFakturDal`

```csharp
public interface IBrgLastFakturDal
{
    IEnumerable<BrgLastFakturDto> ListLastFakturByBrg();
}

public class BrgLastFakturDto
{
    public string BrgId { get; set; }
    public string BrgCode { get; set; }
    public string BrgName { get; set; }
    public DateTime LastFakturDate { get; set; }
}
```

**SQL pattern** (Infrastructure only — mirror `CustomerLastFakturDal` with item grain):

```sql
SELECT
    ISNULL(bb.BrgId, '') AS BrgId,
    ISNULL(bb.BrgCode, '') AS BrgCode,
    ISNULL(bb.BrgName, '') AS BrgName,
    MAX(aa.FakturDate) AS LastFakturDate
FROM BTR_Faktur aa
INNER JOIN BTR_FakturItem fi ON aa.FakturId = fi.FakturId
INNER JOIN BTR_Brg bb ON fi.BrgId = bb.BrgId
WHERE aa.VoidDate = '3000-01-01'
GROUP BY bb.BrgId, bb.BrgCode, bb.BrgName
```

**Note:** Gross Faktur only — no retur join. Matches PO Q9.

### 6.3 `DashboardInventoryRiskAggregator`

**Constants:**

```csharp
public const int SlowMovingDaysThreshold = 90;
public const int DeadStockDaysThreshold = 180;
public const int TopItemCount = 10;
public const string SignalDeadStock = "DeadStock";
public const string SignalSlowMoving = "SlowMoving";
public const string SignalNeverSold = "NeverSold";
// Reuse InTransitWarehouseName, UnknownLabel from DashboardInventoryAggregator
```

**Input:**

| Input | Purpose |
| --- | --- |
| `IEnumerable<StokBalanceView>` | Position and dimensions |
| `IEnumerable<BrgLastFakturDto>` | Last sale date per item |
| `DateTime today` | Idle day calculation |
| `DateTime generatedAt` | Snapshot timestamp |

**Core algorithm (pseudocode):**

```text
1. itemGroups = BuildItemGroups(stokBalanceRows)     // same rules as DashboardInventoryAggregator
2. totalInventoryValue = SUM(itemGroups.InventoryValue)
3. lastFakturByBrgId = dictionary from BrgLastFakturDto

4. For each itemGroup:
     if BrgId not in lastFakturByBrgId → classify NeverSold
     else:
       idleDays = (today - LastFakturDate).Days
       if idleDays >= 180 → DeadStock
       else if idleDays >= 90 → SlowMoving
       else → Active (skip at-risk KPIs)

5. Compute headline counts/values from disjoint sets
6. atRiskValue = neverSoldValue + slowValue + deadValue
7. atRiskPercent = atRiskValue / totalInventoryValue * 100 (null if denominator 0)
8. Build aging buckets (4 rows) from same classification
9. Build category/supplier breakdown — rollup atRiskValue by dimension; top 10 each
10. Build Top 10 dead and Top 10 slow by InventoryValue DESC
11. Build attention list — one row per at-risk item with SignalKey
12. requiresAttention = atRiskValue > 0
```

**Shared item group builder:**

Implementer should **extract** `BuildItemGroups` + `NormalizeDimensionName` from `DashboardInventoryAggregator` into a shared internal helper (e.g. `DashboardInventoryItemGroupBuilder`) used by both aggregators. Do not copy-paste with divergent edits.

### 6.4 `RefreshDashboardInventoryRiskSnapshotWorker`

Mirror `RefreshDashboardInventorySnapshotWorker`:

1. Insert refresh log with `Domain = "InventoryRisk"`.
2. Load `IStokBalanceViewDal.ListData()` and `IBrgLastFakturDal.ListLastFakturByBrg()`.
3. Call `_aggregator.Aggregate(...)` with `ITglJamDal.Now` as `today`.
4. `_snapshotDal.ReplaceCurrent(aggregate, refreshLogId)` in transaction.
5. Mark success/failure on refresh log.

### 6.5 `DashboardInventoryRiskSnapshotDal.ReplaceCurrent`

Delete all `SnapshotKey = 'CURRENT'` rows from six Inventory Risk tables; insert new rows; update KPI `LastRefreshLogId`. Follow `DashboardCustomerSnapshotDal` transaction pattern.

Counter prefix for child row IDs: use `PDIR` or consistent project prefix.

### 6.6 API contract — `GET /api/dashboard/inventory-risk`

**Auth:** JWT required.

**Response:** `ApiResponse<DashboardInventoryRiskResponse>`

```csharp
public class DashboardInventoryRiskResponse
{
    public bool IsAvailable { get; set; }
    public bool IsDataFresh { get; set; }
    public DateTime? GeneratedAt { get; set; }

    public DashboardInventoryRiskAttentionCards AttentionCards { get; set; }
    public IList<DashboardInventoryRiskAgingBucket> AgingBuckets { get; set; }
    public IList<DashboardInventoryRiskBreakdownItem> CategoryRiskExposure { get; set; }
    public IList<DashboardInventoryRiskBreakdownItem> SupplierRiskExposure { get; set; }
    public IList<DashboardInventoryRiskAttentionItem> AttentionList { get; set; }
    public DashboardInventoryRiskRankings Rankings { get; set; }
    public DashboardInventoryRiskNavigationLinks Navigation { get; set; }
}

public class DashboardInventoryRiskAttentionCards
{
    public decimal TotalInventoryValue { get; set; }
    public int DeadStockItemCount { get; set; }
    public decimal DeadStockValue { get; set; }
    public int SlowMovingItemCount { get; set; }
    public decimal SlowMovingValue { get; set; }
    public decimal AtRiskInventoryPercent { get; set; }
    public bool RequiresAttention { get; set; }
}

public class DashboardInventoryRiskAttentionItem
{
    public string BrgCode { get; set; }
    public string BrgName { get; set; }
    public string KategoriName { get; set; }
    public string SupplierName { get; set; }
    public int Qty { get; set; }
    public decimal InventoryValue { get; set; }
    public int? DaysSinceLastFaktur { get; set; }
    public string SignalKey { get; set; }
    public string SignalLabel { get; set; }
    public string ReportRoute { get; set; }   // always /reports/inventory
}
```

`DashboardInventoryRiskAgingBucket` mirrors `DashboardPiutangAgingBucket` shape: `BucketKey`, `BucketLabel`, `Amount` (inventory value), `ItemCount`, `SortOrder`.

---

## 7. Frontend Implementation

### 7.1 Route and navigation

| Item | Value |
| --- | --- |
| Route path | `dashboard/inventory-risk` |
| Route name | `inventory-risk-dashboard` |
| Component | `InventoryRiskDashboardView.vue` |
| Sidebar label | **Inventory Risk** |
| Sidebar position | After **Inventory**, before **Purchasing** (or adjacent to Inventory) |
| Page title | Slow Moving & Dead Stock Dashboard |
| Subtitle | Inventory health — items requiring management attention |

### 7.2 `InventoryRiskDashboardView.vue` layout

Use `DashboardDetailLayout` (same shell as Customer/Salesman dashboards).

```text
[Staleness banner when !IsDataFresh]
[Unavailable message when !IsAvailable]

Section 1: InventoryRiskAttentionCards
  - KpiCard grid: Dead Stock Item Count | Dead Stock Value | Slow Moving Item Count
                  | Slow Moving Value | At-Risk Inventory %
  - ExecutiveAttentionCard when RequiresAttention

Section 2: Row — AgingPieChart (left) | CategoryRiskExposure bar (right)
Section 3: SupplierRiskExposure bar (full width)
Section 4: InventoryRiskAttentionList (DataTable)
Section 5: Row — Top10RankingTable Dead Stock | Top10RankingTable Slow Moving
Section 6: InventoryRiskNavigationSection
```

### 7.3 Component reuse

| Component | Usage |
| --- | --- |
| `AgingPieChart.vue` | Inventory aging distribution — bind `Amount` to inventory value |
| `InventoryHorizontalBarChart.vue` | Category and supplier at-risk exposure |
| `Top10RankingTable.vue` | Top 10 dead / slow — extend columns for Days Since Last Faktur if needed |
| `ExecutiveAttentionCard.vue` | Generic attention indicator |
| `KpiCard.vue` | Headline KPI cards |
| `DashboardDetailLayout.vue` | Page shell |

### 7.4 Store and API

- Add `fetchDashboardInventoryRisk()` in `dashboardApi.ts`.
- Add `loadInventoryRisk()` in `dashboardStore.ts`.
- Add TypeScript interfaces in `models/dashboard.ts` matching API response.

### 7.5 Drill-down wiring

- Attention list row click and Top 10 row click → `navigateToReport(router, '/reports/inventory', brgName)`.
- Prefer `BrgName` for `?q=` — matches Inventory Report free-text search fields.
- Ensure `InventoryReportView.vue` reads route query `q` on mount (mirror Sales/Piutang report pattern from M17).

### 7.6 Navigation section links

| Label | Route |
| --- | --- |
| Inventory Dashboard | `/dashboard/inventory` |
| Inventory Report | `/reports/inventory` |

---

## 8. Phase 2 — Executive Dashboard Integration

Execute **after** Phase 1 acceptance. PO Q2.

### 8.1 Backend changes

| Module | Change |
| --- | --- |
| `IDashboardExecutiveDal` / handler | Load Inventory Risk KPI snapshot alongside existing inventory composition snapshot |
| `DashboardExecutiveInventoryAttention` | Add `DeadStockValue`, `AtRiskInventoryPercent`; extend `RequiresAttention` |
| `DashboardExecutiveComposer.ComposeInventory` | Merge composition metrics (unchanged) with risk metrics from Inventory Risk KPI |
| `FormatInventorySummary` | Append at-risk % or dead stock value to summary text when available |

**Proposed executive inventory attention shape:**

```csharp
public class DashboardExecutiveInventoryAttention
{
    // Existing (preserved)
    public decimal TotalInventoryValue { get; set; }
    public decimal? TopCategoryPercent { get; set; }
    public decimal? TopSupplierPercent { get; set; }

    // New (M19 Phase 2)
    public decimal DeadStockValue { get; set; }
    public decimal? AtRiskInventoryPercent { get; set; }

    public bool RequiresAttention { get; set; }  // true if concentration OR at-risk > 0
    public bool IsAvailable { get; set; }
}
```

**RequiresAttention logic (Phase 2):**

```text
requiresAttention =
    (existing concentration rule)
    OR deadStockValue > 0
    OR (atRiskInventoryPercent ?? 0) > 0
```

### 8.2 Frontend changes

- Extend `DashboardExecutiveInventoryAttention` TypeScript interface.
- Update `DashboardHomeView.vue` inventory attention card to show Dead Stock Value and At-Risk % when available.
- Add link from executive inventory card to `/dashboard/inventory-risk`.

### 8.3 Tests

- Extend `DashboardExecutiveComposerTest` for risk metric composition and RequiresAttention OR logic.

---

## 9. Verification

### 9.1 Unit tests — `DashboardInventoryRiskAggregatorTest`

| Test | Assertion |
| --- | --- |
| BuildItemGroups parity | TotalInventoryValue matches `DashboardInventoryAggregator` on same input rows |
| Never Sold | Item with stock, absent from last-Faktur list → NeverSold class only |
| Slow Moving boundary | LastFaktur exactly `today − 90` → Slow Moving |
| Dead Stock boundary | LastFaktur exactly `today − 180` → Dead Stock |
| Active exclusion | LastFaktur `today − 89` → not in at-risk KPIs |
| Mutual exclusivity | Item counts sum ≤ TotalItem; no item in two classes |
| At-risk value | Equals NeverSold + Slow + Dead values |
| At-risk percent | Correct ratio; null when TotalInventoryValue = 0 |
| In-Transit exclusion | In-Transit warehouse rows ignored |
| Zero qty exclusion | Aggregated qty ≤ 0 excluded |
| Top 10 cap | Rankings limited to 10 per table |
| Attention list | One row per at-risk item with correct SignalKey |

Use fixed `today` date in tests — same pattern as `DashboardCustomerAggregatorTest`.

### 9.2 Integration / manual test checklist

1. Deploy SQL tables; run InventoryRisk refresh worker; verify six tables populated.
2. `GET /api/dashboard/inventory-risk` returns complete response with `GeneratedAt`.
3. Navigate to `/dashboard/inventory-risk` — all sections render in Proposal A order.
4. Sidebar shows **Inventory Risk** entry.
5. `TotalInventoryValue` matches Inventory Dashboard and Inventory Report footer.
6. `AtRiskInventoryPercent` denominator reconciles with M15 total.
7. Spot-check idle item via Desktop Faktur Brg Info — days since last Faktur matches dashboard.
8. Never Sold item appears with **Never Sold** signal only — not in dead/slow Top 10.
9. Item with Faktur within 89 days excluded from Slow/Dead signals.
10. Click attention list row → Inventory Report opens with `?q=` pre-filled.
11. Category/supplier at-risk bars sum to ≤ total at-risk value (top 10 may not sum to 100%).
12. **"⚠ Dashboard Data Not Fresh"** when snapshot exceeds 60-minute interval.
13. Inventory Dashboard (`/dashboard/inventory`) **unchanged**.
14. `GET /api/health/dashboard-snapshots` includes InventoryRisk domain.
15. Manual refresh: `POST /api/admin/dashboard/refresh` with `{ "Domain": "InventoryRisk" }` succeeds.
16. Worker CLI: `btr.portal.worker --domain InventoryRisk --triggered-by Manual` succeeds.
17. **Phase 2:** Executive dashboard shows Dead Stock Value, At-Risk %, and updated RequiresAttention.

---

## 10. Risk Analysis

| Risk | Likelihood | Impact | Mitigation |
| --- | --- | --- | --- |
| Full FakturItem history scan slow on large databases | Medium | Medium | Aggregated SQL in `BrgLastFakturDal`; measure refresh duration in staging; index on `BTR_Faktur(FakturDate, VoidDate)` + FakturItem(BrgId) if needed |
| Denominator drift vs M15 | Medium | High | Shared `BuildItemGroups` helper; unit test parity with `DashboardInventoryAggregator` |
| Desktop IF8 movement differs from portal Last Faktur | High | Low | Expected — document authoritative portal rule in operational guide; Desktop for validation only |
| Never Sold false positives (returns-only items) | Low | Medium | PO chose gross Faktur — items with only retur and no Faktur remain Never Sold; acceptable per Q9 |
| BrgId dimension mismatch on balance vs FakturItem | Low | Medium | Join on BrgId; spot-check orphan BrgIds in acceptance |
| Refresh failure leaves empty snapshot | Low | Medium | Transactional ReplaceCurrent; UI shows unavailable state; prior snapshot deleted only on success path |
| Item name pre-filter ambiguity in Inventory Report | Low | Low | Use BrgName from snapshot; substring match acceptable for V1 |
| Executive Phase 2 scope creep | Medium | Low | Separate delivery phase; Phase 1 acceptance excludes executive changes |

---

## 11. Documentation Updates (post-delivery)

| Document | Update |
| --- | --- |
| `docs/features/btr-portal/btr-portal-operational.md` | Inventory Risk route, classification rules, drill-down path, 60-minute refresh |
| `docs/features/btr-portal/btr-portal-domain.md` | Inventory Risk KPI definitions; remove "deferred" for slow/dead where implemented |
| `docs/features/btr-portal/btr-portal-architecture.md` | `DashboardInventoryRiskAgg`, tables, worker, API endpoint |
| `docs/features/materialized-dashboard/materialized-dashboard-domain.md` | InventoryRisk domain refresh rules |
| `docs/work/btr-portal/M19 Slow Moving & Dead Stock - Analysis.md` | Link to this plan as implemented |

---

## 12. Implementation Steps

Execute in order. Each phase should compile before proceeding.

### Phase 1 — Database

1. Create six `BTRPD_InventoryRisk*.sql` table scripts.
2. Add to `btr.sql.sqlproj`; deploy to dev database.

### Phase 2 — Backend core

3. Extract shared inventory item group builder from `DashboardInventoryAggregator` (minimal refactor).
4. Add `IBrgLastFakturDal` + `BrgLastFakturDal`.
5. Add aggregate models and `DashboardInventoryRiskAggregator` with unit tests (Section 9.1).
6. Add `IDashboardInventoryRiskSnapshotDal` + `DashboardInventoryRiskSnapshotDal.ReplaceCurrent`.
7. Add `RefreshDashboardInventoryRiskSnapshotWorker` + request/result types.
8. Add `InventoryRiskIntervalMinutes = 60` to `DashboardSnapshotOptions`.

### Phase 3 — Backend integration

9. Register all new services in Application and Infrastructure DI.
10. Update `RefreshAllDashboardSnapshotsWorker`, `RefreshDashboardSnapshotsHandler`, `HealthController`, worker `Program.cs`.
11. Add `DashboardInventoryRiskAgg` read path: query, handler, `DashboardInventoryRiskDal`, `InventoryRiskDashboardController`.
12. Verify `GET /api/dashboard/inventory-risk` against populated snapshot.
13. Run manual InventoryRisk refresh via admin API and worker CLI.

### Phase 4 — Frontend

14. Add TypeScript models and `fetchDashboardInventoryRisk`.
15. Extend `dashboardStore` with `loadInventoryRisk`.
16. Add route and sidebar **Inventory Risk** item.
17. Create section components (Section 7).
18. Implement `InventoryRiskDashboardView.vue` per Section 7.2 layout.
19. Ensure `InventoryReportView.vue` reads `?q=` on mount for pre-filter.
20. Wire row clicks and navigation links.

### Phase 5 — Verification and docs

21. Run unit tests and manual checklist (Section 9.2 items 1–16).
22. Add Task Scheduler job for InventoryRisk domain (operations).
23. Update feature documentation (Section 11).

### Phase 6 — Executive integration (post-M19 acceptance)

24. Extend executive read path to load Inventory Risk KPI snapshot.
25. Update `DashboardExecutiveComposer` and frontend executive inventory card.
26. Run Phase 2 tests and checklist item 17.

---

## 13. Acceptance Criteria

M19 Phase 1 is complete when:

1. `/dashboard/inventory-risk` displays **Slow Moving & Dead Stock Dashboard** (Proposal A layout) for all authenticated users.
2. Sidebar shows **Dashboard → Inventory Risk** at `/dashboard/inventory-risk`.
3. Dedicated `BTRPD_InventoryRisk*` tables exist and are populated by `RefreshDashboardInventoryRiskSnapshotWorker`.
4. `GET /api/dashboard/inventory-risk` returns attention cards, aging pie data, category/supplier exposure, attention list, Top 10 dead/slow, and navigation links.
5. Headline KPIs present: Dead Stock Item Count/Value, Slow Moving Item Count/Value, At-Risk Inventory %.
6. Classification uses **Last Faktur Date** at item grain with **90-day Slow** and **180-day Dead** thresholds; **Never Sold** is a separate signal.
7. KPI item counts are **mutually exclusive** across Never Sold, Slow Moving, and Dead Stock.
8. `TotalInventoryValue` reconciles with Inventory Dashboard and Inventory Report footer.
9. `AtRiskInventoryPercent` uses the same denominator as M15.
10. In-Transit warehouse excluded; BrgId-first aggregation matches M15 rules.
11. Item with Faktur within last 89 days excluded from Slow/Dead signals.
12. Attention list includes only approved signals: Dead Stock · Slow Moving · Never Sold — one row per item.
13. Item row click opens Inventory Report with item name pre-filter via `?q=`.
14. Navigation links to Inventory Dashboard and Inventory Report work.
15. **Attention Indicator** presentation matches M16–M18.
16. **"⚠ Dashboard Data Not Fresh"** banner when snapshot exceeds 60-minute interval.
17. Inventory Dashboard and `GET /api/dashboard/inventory` behave unchanged.
18. Aggregator unit tests pass; health endpoint lists InventoryRisk domain.

M19 Phase 2 is complete when:

19. Executive Dashboard inventory section shows **Dead Stock Value**, **At-Risk Inventory %**, and updated **RequiresAttention** while preserving composition metrics.
20. Executive inventory card links to `/dashboard/inventory-risk`.

---

## 14. Handoff Notes for Implementer

- **Section 2 is authoritative** — do not add salesman dimension, ABC, warehouse breakdown, export, Kartu Stok drill-down, or retur-adjusted classification.
- **Last Faktur Date is the portal authority** — do not use Kartu Stok `MovingStok` for M19 KPIs despite Desktop IF8 utility for validation.
- **Read source DALs in InventoryRisk worker** — do not derive risk metrics from M15 snapshot tables.
- **Shared item groups** — extract from `DashboardInventoryAggregator`; verify parity test before shipping.
- **Never Sold** items have no `LastFakturDate` — exclude from Slow/Dead counts and dead/slow Top 10 tables.
- **Slow Moving KPIs use 90–179 day band** — Dead Stock uses ≥ 180 days; this plan resolves analysis Section 12 architect notes.
- **At-risk value** = NeverSold + Slow + Dead values (disjoint); do not add Dead into Slow for percent numerator.
- **ReplaceCurrent** must be transactional across all six Inventory Risk tables.
- **Pre-filter** uses `BrgName` in `?q=` — verify Inventory Report search includes item name/code fields.
- **Phase 2 executive changes are separate** — do not include in Phase 1 PR unless PO explicitly combines delivery.
- Reuse `AgingPieChart.vue` — map inventory bucket labels in the view; no Piutang-specific wording in shared component.

---

*End of implementation plan — M19 Slow Moving & Dead Stock Dashboard*
