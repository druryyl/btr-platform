# Implementation Plan: M18 Salesman Performance Dashboard

## Document Status

| Field | Value |
| --- | --- |
| Initiative | M18 Salesman Performance — **Which salesman requires management attention and why?** |
| Authoritative requirements | `docs/work/btr-portal/M18 Salesman Performance - Analysis.md` — **Section 12 (Final Product Decisions)** |
| Related features | `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-operational.md`, `docs/features/btr-portal/btr-portal-architecture.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md` |
| Reference pattern | M17 Customer Analytics snapshot domain + M16 Attention Indicator presentation |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |
| Status | **Ready for implementation** |
| Product decisions | **Confirmed** — analysis Section 12, 2026-06-08 |

---

## 1. Goal

Deliver **Salesman Performance** at `/dashboard/salesmen` — a dedicated salesman-centric management view covering **Sales and Piutang** to answer *Which salesman requires management attention and why?*

**Primary outcomes:**

- New route `/dashboard/salesmen` with page title **Salesman Performance** for all authenticated users (no role-based routing).
- **Dedicated Salesman snapshot domain** (`BTR_PortalDashboardSalesman*`) with its own refresh worker — materialized KPIs, not live composition.
- **Proposal A layout** (fixed section order): Attention Cards → Attention List → Performance Rankings → Exposure Rankings → Segmentation Summary → Navigation.
- Mandatory rankings: **Top 10 Omzet**, **Top 10 Achievement %**, **Top 10 Piutang** — each with `SalesPersonCode`; exclude reps where ranking value = 0.
- **Attention List** with approved signals: Below Target · No Target · High Overdue Exposure · High Piutang Exposure · Customer Concentration · Dormant Customer Portfolio.
- Salesman row click → Sales or Piutang Report with **salesman name pre-filter** (`?q=`).
- Navigation path: **Salesman Performance → Domain Dashboard → Report** (M16/M17 pattern).
- **Supplements** Sales Dashboard Top 10 Salesman — does **not** replace, modify executive dashboard, or alter `BTR_PortalDashboardSalesTopSalesman`.

**Explicitly out of scope (PO confirmed):**

- Pipeline omzet, Effective Call, route coverage, visit compliance, GPS, retur, Faktur Kembali aggregates.
- Collection performance metrics / DSO (deferred **M20**).
- Field activity metrics (deferred **M25 Sales Force Effectiveness**).
- Bottom 10 rankings, historical trend charts, unified salesman score.
- New Salesman Report route, executive dashboard changes.
- Changes to existing Sales/Piutang/Customer snapshot tables, workers, or domain dashboard APIs.

---

## 2. Authoritative Product Decisions

Source: analysis Section 12. Do not re-decide these rules during implementation.

### 2.1 Positioning and audience

| Decision | Value |
| --- | --- |
| Route | `/dashboard/salesmen` |
| Page title | **Salesman Performance** |
| Audience | All authenticated users — no RBAC in M18 |
| Sidebar | Dashboard → **Salesmen** (after Customers, before Inventory) |
| Executive relationship | **No executive changes** (Q35) |
| Sales Dashboard relationship | **Supplements** Top 10 Salesman — does not replace (Q4) |

### 2.2 Scope boundaries

| Decision | Value |
| --- | --- |
| Domains | **Sales + Piutang only** (Q6) |
| Sales source | **Faktur-only** — current calendar month `GrandTotal` (Q7, Q16) |
| Piutang source | **All-time open balance** — `KurangBayar > 1` via FF1 invoicing-salesman join (Q15, Q17) |
| Materialization | Dedicated `BTR_PortalDashboardSalesman*` tables + refresh worker (Q36–Q37) |
| Dashboard only | No new Salesman Report (Q13) |
| Period | **Current month snapshot only** — no historical retention (Q39) |

### 2.3 Attribution and business rules

| Rule | Value |
| --- | --- |
| Salesman key | **`SalesPersonId` internal**; `SalesPersonName` display; `SalesPersonCode` on ranking rows (Q30, Q40) |
| Sales omzet attribution | `Faktur.SalesPersonId` at invoice time |
| Piutang attribution | **Invoicing salesman** — FF1 model (`SalesName` on open Faktur rows) |
| Customer ownership (dormant) | **Last Invoicing Salesman** — salesman on customer's most recent Faktur (Q14, Q18) |
| Achievement % | `SalesOmzetChartAchievementPolicy.ComputePercent(omzet, target)` per rep |
| Achievement bands | Reuse M16: ≥100% Healthy · 80–99% Warning · <80% Critical · null Unknown (Q19) |
| Below Target signal | Rep in **Warning or Critical** achievement band |
| No Target signal | Rep has **month activity** but no configured target (Q20) |
| Dormant customer rule | Reuse M17: **90 days** since last Faktur AND prior transaction history exists |
| Concentration | Top customer % and piutang share % shown **without automatic warning thresholds** (Q22) |
| Active salesman | Faktur in **current calendar month** |
| Inactive salesman | No current-month Faktur — included in segmentation; **excluded from Top Rankings when value = 0** (Q42) |

### 2.4 Attention rules

**Approved attention list signals (Q21):** BelowTarget · NoTarget · HighOverdueExposure · HighPiutangExposure · CustomerConcentration · DormantCustomerPortfolio

| Signal | Inclusion rule |
| --- | --- |
| **BelowTarget** | Target exists (`> 0`) AND achievement % in Warning or Critical band |
| **NoTarget** | Month activity (`OmzetAmount > 0` OR `CustomerCount > 0`) AND target is null or `≤ 0` |
| **HighOverdueExposure** | Any overdue balance on rep's invoiced open Faktur rows (aging bucket ≠ `Current`) |
| **HighPiutangExposure** | Open piutang balance `> 0` for rep — show amount and % of company total (informational; no % threshold) |
| **CustomerConcentration** | `OmzetAmount > 0` AND top-customer % computable — show % in detail (informational; no % threshold) |
| **DormantCustomerPortfolio** | `DormantCustomerCount > 0` on rep's book via last-invoicing attribution |

**Presentation:** Generic **Attention Indicator** (M16/M17) — no per-signal severity system (Q25). Combined cross-domain signals encouraged — one row per salesman × signal (Q24).

### 2.5 Rankings, layout, and segmentation

| Decision | Value |
| --- | --- |
| Mandatory rankings | Top 10 Omzet · Top 10 Achievement % · Top 10 Piutang (Q26) |
| Top N | **10** for all salesman rankings (Q27) |
| Layout | **Proposal A** — Salesman Attention First (Q28) |
| Segmentation (mandatory) | **Wilayah** · **Active vs Inactive Salesman** (Q31) |
| Segmentation (optional) | **SegmentId** from `BTR_SalesPerson` |

**Fixed section order (Q27–Q31):**

1. Salesman Attention Cards  
2. Salesman Attention List  
3. Performance Rankings (Top 10 Omzet · Top 10 Achievement %)  
4. Exposure Rankings (Top 10 Piutang)  
5. Segmentation Summary  
6. Navigation to supporting dashboards and reports  

### 2.6 Drill-down

| Action | Target |
| --- | --- |
| BelowTarget, NoTarget, CustomerConcentration, DormantCustomerPortfolio | `/reports/sales` with salesman name `?q=` |
| HighOverdueExposure, HighPiutangExposure | `/reports/piutang` with salesman name `?q=` |
| Top Omzet / Top Achievement % row | `/reports/sales` |
| Top Piutang row | `/reports/piutang` |
| Domain navigation links | `/dashboard/sales`, `/dashboard/piutang`, `/reports/sales`, `/reports/piutang` |

Pre-filter uses **SalesPersonName** in `?q=` (matches existing report free-text search on `Sales` column).

---

## 3. Architecture Overview

### 3.1 Target topology

```text
Source DALs (read at refresh time — NOT from other snapshot tables)
  IFakturViewDal                    → current-month Faktur rows (+ SalesPersonId, SalesPersonCode)  [EXTEND]
  IPiutangOpenBalanceWithSalesmanDal → all-time open balances with invoicing salesman               [NEW]
  ICustomerLastFakturWithSalesmanDal → last Faktur per customer + invoicing salesman               [NEW]
  ISalesPersonDal                   → master: Code, Name, Wilayah, Segment
  ISalesOmzetTargetDal              → per-rep monthly targets (+ batch list method)                  [EXTEND]
    ↓
RefreshDashboardSalesmanSnapshotWorker
    ↓ DashboardSalesmanAggregator
BTR_PortalDashboardSalesman* tables (6)
    ↓
Browser → GET /api/dashboard/salesmen
    ↓ MediatR
GetDashboardSalesmanHandler
    ↓ IDashboardSalesmanDal
DashboardSalesmanDal → DashboardSalesmanResponse

Existing unchanged:
  GET /api/dashboard/executive
  GET /api/dashboard/sales | piutang | customers
  BTR_PortalDashboardSalesTopSalesman (Sales Dashboard only)
  GET /api/reports/sales | piutang
```

**Why read source DALs, not Sales/Piutang/Customer snapshots:**

- Sales Top 10 snapshot stores **name-only omzet** — M18 requires `SalesPersonId`, target, achievement %, piutang, and attention signals.
- `PiutangOpenBalanceDal` omits salesman — M18 piutang KPIs require FF1 join pattern.
- Dormant rollup requires **last-invoicing salesman** not present in `ICustomerLastFakturDal` today.
- Keeps Salesman domain self-contained; avoids coupling refresh order or mutating protected Sales snapshot semantics.

### 3.2 Design principles

| Principle | Application |
| --- | --- |
| Reuse business rules | Copy authoritative logic from `DashboardSalesFakturAggregator`, `DashboardPiutangAggregator`, `DashboardCustomerAggregator`, `ExecutiveSalesAchievementBandResolver` |
| Dedicated snapshot | Salesman KPIs materialized in own tables (Q36) |
| Do not compose at read time | All derivations in aggregator at refresh (Q37) |
| Preserve domain dashboards | Sales/Piutang/Customer detail pages unchanged |
| Key by SalesPersonId | Name is display only; master is authoritative for Code/Wilayah/Segment |
| Attention Indicator only | Achievement bands apply to BelowTarget signal and optional card badge — no generic severity engine |
| Fail gracefully | Empty/missing snapshot → unavailable sections with clear UI message |

### 3.3 Architecture decisions

| Decision | Choice | Rationale |
| --- | --- | --- |
| Snapshot approach | **New Salesman domain** with 6 tables | PO Q36; cross-domain per-rep metrics not representable in existing tables |
| Salesman key | **`SalesPersonId`** via extended `FakturView` + piutang FF1 join | PO Q40; avoids ambiguous name-only grouping used in Sales Top 10 |
| Piutang input | **`IPiutangOpenBalanceWithSalesmanDal.ListOpenBalances()`** | Reuses `PiutangOpenBalanceDal` filter (`Sisa > 1`) + FF1 `SalesPerson` join; no date filter (all-open semantics) |
| Dormant attribution | **`ICustomerLastFakturWithSalesmanDal`** | Last Faktur row per customer includes `SalesPersonId` for rollup (Q14) |
| Target loading | **`ISalesOmzetTargetDal.ListTargetsForMonth(year, month)`** | Batch load avoids N+1 `GetTargetAmount` calls per rep |
| Achievement band | **`ExecutiveSalesAchievementBandResolver`** | PO Q19 — same 80/100 thresholds as M16 |
| Refresh cadence | **30 minutes** (`SalesmanIntervalMinutes`) | PO Q38; aligns with Sales/Customer cadence |
| RefreshAll order | Piutang → Inventory → Sales → Purchasing → Customer → **Salesman** | Salesman last; reads source DALs independently |
| Read API | **`GET /api/dashboard/salesmen`** | Mirrors M17 domain dashboard pattern |
| Existing Sales Top 10 table | **Do not alter** | PO Q4 — M18 is additive |
| Attention list rows | **One row per salesman × signal** | Same pattern as M17 customer × signal |
| Pre-filter mechanism | **Route query `?q=`** → report store `freeText` | Reuse `useReportFreeTextFilter` / M17 pattern |
| Staleness banner | Salesman `GeneratedAt` vs `SalesmanIntervalMinutes` | Same copy: **"⚠ Dashboard Data Not Fresh"** |

---

## 4. Impact Analysis

### 4.1 Affected modules

| Layer | Module | Change type |
| --- | --- | --- |
| SQL | `btr.sql/Tables/ReportingContext/BTR_PortalDashboardSalesman*.sql` (6 tables) | **New** |
| SQL | `btr.sql.sqlproj` | Include new tables |
| Application | `DashboardSnapshotAgg/Services/DashboardSalesmanAggregator.cs` | **New** |
| Application | `DashboardSnapshotAgg/Services/DashboardSalesmanKeyResolver.cs` | **New** |
| Application | `DashboardSnapshotAgg/Models/DashboardSalesman*.cs` | **New** aggregate models |
| Application | `DashboardSnapshotAgg/Contracts/IDashboardSalesmanSnapshotDal.cs` | **New** |
| Application | `DashboardSnapshotAgg/Contracts/IPiutangOpenBalanceWithSalesmanDal.cs` | **New** |
| Application | `DashboardSnapshotAgg/UseCases/RefreshDashboardSalesmanSnapshotWorker.cs` | **New** |
| Application | `DashboardSnapshotAgg/DashboardSnapshotOptions.cs` | Add `SalesmanIntervalMinutes` |
| Application | `DashboardSnapshotAgg/UseCases/RefreshAllDashboardSnapshotsWorker.cs` | Register Salesman worker |
| Application | `DashboardSnapshotAgg/Commands/RefreshDashboardSnapshotsCommand.cs` | Add Salesman domain |
| Application | `DashboardSalesmanAgg/` (query, contracts, DTOs) | **New** read-side aggregate |
| Application | `SalesContext/FakturInfo/FakturView.cs` | Add `SalesPersonId`, `SalesPersonCode` |
| Application | `SalesContext/FakturInfo/ICustomerLastFakturDal.cs` | Add last-Faktur-with-salesman DTO + method |
| Application | `SalesContext/SalesOmzetAgg/Contracts/ISalesOmzetTargetDal.cs` | Add `ListTargetsForMonth` |
| Infrastructure | `DashboardSnapshotAgg/DashboardSalesmanSnapshotDal.cs` | **New** |
| Infrastructure | `DashboardSnapshotAgg/PiutangOpenBalanceWithSalesmanDal.cs` | **New** |
| Infrastructure | `DashboardSalesmanAgg/DashboardSalesmanDal.cs` | **New** |
| Infrastructure | `SalesContext/FakturInfoAgg/FakturViewDal.cs` | Select `SalesPersonId`, `SalesPersonCode` |
| Infrastructure | `SalesContext/FakturInfoAgg/CustomerLastFakturDal.cs` | Add last-Faktur-with-salesman query |
| Infrastructure | `SalesContext/SalesOmzetAgg/SalesOmzetTargetDal.cs` | Implement batch target list |
| API | `Controllers/Dashboard/SalesmanDashboardController.cs` | **New** |
| API | `HealthController.cs` | Add Salesman domain |
| API | DI registrations | New DALs, worker, aggregator |
| Worker | `btr.portal.worker/Program.cs` | Add `Salesman` to `--domain` |
| Frontend | `router/index.ts` | Route `/dashboard/salesmen` |
| Frontend | `layouts/MainLayout.vue` | Sidebar **Salesmen** item |
| Frontend | `views/dashboard/SalesmanDashboardView.vue` | **New** |
| Frontend | `components/dashboard/Salesman*.vue` | **New** section components |
| Frontend | `models/dashboard.ts`, `api/dashboardApi.ts`, `stores/dashboardStore.ts` | Salesman types and loader |
| Frontend | Report views | Confirm `?q=` pre-filter works for salesman name (likely exists from M17) |
| Tests | `btr.test/ReportingContext/` | Aggregator + key resolver unit tests |
| Docs | Post-delivery feature docs | Operational, domain, architecture updates |

### 4.2 Unaffected modules

| Module | Reason |
| --- | --- |
| `DashboardExecutiveComposer`, `GET /api/dashboard/executive` | PO Q35 — no executive changes |
| `DashboardSalesFakturAggregator`, `BTR_PortalDashboardSalesTopSalesman` | Protected Sales snapshot — M18 is separate domain |
| Existing Sales/Piutang/Customer snapshot workers and read APIs | Additive Salesman domain only |
| Domain dashboard views (Sales, Piutang, Customer) | Detail layer unchanged |
| Report DALs and API contracts | No column additions |
| BTR Desktop | No changes |

### 4.3 Metric traceability

| Salesman dashboard field | Source at refresh | Rule |
| --- | --- | --- |
| Below Target count | Faktur omzet + targets | Reps where `ExecutiveSalesAchievementBandResolver` ∈ {Warning, Critical} |
| No Target count | Faktur activity + targets | Activity in month AND target null/`≤ 0` |
| High Overdue Exposure count | Piutang rows with salesman | Distinct reps with any overdue customer (`aging ≠ Current`) |
| High Piutang Exposure count | Piutang rows with salesman | Distinct reps with `SUM(KurangBayar) > 0` |
| Customer Concentration count | Faktur grouped by rep × customer | Distinct reps with `OmzetAmount > 0` and computable top-customer % |
| Dormant Portfolio count | Last-Faktur-with-salesman + dormant rule | Distinct reps with ≥1 dormant customer on book |
| Top 10 Omzet | Current-month Faktur by `SalesPersonId` | `SUM(GrandTotal)` desc; exclude `CompletedOmzet = 0` |
| Top 10 Achievement % | Omzet + per-rep target | `ComputePercent`; exclude reps with null % or 0 omzet |
| Top 10 Piutang | Open balances by invoicing salesman | `SUM(KurangBayar)` desc; exclude `OutstandingBalance = 0` |
| Top customer % (concentration) | Faktur rep × customer | Top-1 customer omzet / rep total omzet × 100 |
| Piutang share % | Rep open balance / company total | Same pattern as M17 Top Piutang % |
| Segmentation by Wilayah | `ISalesPersonDal` | Count per `WilayahName` (blank → `"Unknown"`) |
| Active vs Inactive | Current-month Faktur presence | Active = invoiced this month; inactive otherwise |
| Segmentation by Segment | `SalesPersonModel.SegmentName` | Optional; blank → `"Unknown"` |

**Reconciliation notes:**

- Top 10 Omzet amounts should match Sales Report grouped by `SalesName` for current month (allowing `SalesPersonId` precision).
- Top 10 Piutang should match Piutang Report / FF1 totals grouped by `SalesName` for all open items.
- Sales Dashboard Top 10 Salesman omzet values should match M18 Top 10 Omzet when keyed by name (M18 adds Code, target, %).
- Piutang report default period differs from dashboard all-open semantics — document on drill-down (same as Piutang/Customer dashboards).

---

## 5. Database Design

Deploy all tables with `SnapshotKey = 'CURRENT'` delete-and-replace pattern (consistent with existing dashboard snapshots).

### 5.1 `BTR_PortalDashboardSalesmanKpi`

Single row per refresh — headline metrics for attention cards and concentration denominators.

| Column | Type | Description |
| --- | --- | --- |
| SnapshotKey | VARCHAR(10) PK | `'CURRENT'` |
| GeneratedAt | DATETIME | Refresh timestamp |
| PeriodYear | INT | Sales period year |
| PeriodMonth | INT | Sales period month |
| TotalTeamOmzet | DECIMAL(18,2) | Current-month invoiced total |
| TotalPiutang | DECIMAL(18,2) | All-time open balance total |
| ActiveSalesmanCount | INT | Reps with Faktur in current month |
| BelowTargetCount | INT | Warning + Critical achievement bands |
| NoTargetCount | INT | Activity without configured target |
| HighOverdueExposureCount | INT | Reps with overdue customers |
| HighPiutangExposureCount | INT | Reps with open piutang > 0 |
| CustomerConcentrationCount | INT | Reps with computable top-customer % |
| DormantPortfolioCount | INT | Reps with dormant customers on book |
| TopOmzetSalesmanPercent | DECIMAL(9,4) NULL | Top-1 rep omzet / TotalTeamOmzet |
| TopPiutangSalesmanPercent | DECIMAL(9,4) NULL | Top-1 rep balance / TotalPiutang |
| LastRefreshLogId | VARCHAR(13) | FK to refresh log |

### 5.2 `BTR_PortalDashboardSalesmanTopOmzet`

| Column | Type | Description |
| --- | --- | --- |
| SalesmanTopOmzetId | VARCHAR(13) PK | Generated ID |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| Rank | INT | 1–10 |
| SalesPersonId | VARCHAR(13) | Internal key |
| SalesPersonCode | VARCHAR(20) | Display / drill-down |
| SalesPersonName | VARCHAR(50) | Display |
| CompletedOmzet | DECIMAL(18,2) | `SUM(GrandTotal)` |
| PercentOfTotal | DECIMAL(9,4) NULL | Row amount / TotalTeamOmzet |

Unique: `(SnapshotKey, Rank)`

### 5.3 `BTR_PortalDashboardSalesmanTopAchievement`

| Column | Type | Description |
| --- | --- | --- |
| SalesmanTopAchievementId | VARCHAR(13) PK | Generated ID |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| Rank | INT | 1–10 |
| SalesPersonId | VARCHAR(13) | |
| SalesPersonCode | VARCHAR(20) | |
| SalesPersonName | VARCHAR(50) | |
| TargetAmount | DECIMAL(18,2) NULL | Monthly target |
| CompletedOmzet | DECIMAL(18,2) | Month omzet |
| AchievementPercent | DECIMAL(9,4) NULL | Policy-computed % |
| PercentOfTotal | DECIMAL(9,4) NULL | Omzet / TotalTeamOmzet (informational) |

Unique: `(SnapshotKey, Rank)`

Rank by `AchievementPercent` desc, then `CompletedOmzet` desc, then name asc. Exclude rows where `AchievementPercent` is null or `CompletedOmzet = 0`.

### 5.4 `BTR_PortalDashboardSalesmanTopPiutang`

| Column | Type | Description |
| --- | --- | --- |
| SalesmanTopPiutangId | VARCHAR(13) PK | Generated ID |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| Rank | INT | 1–10 |
| SalesPersonId | VARCHAR(13) | |
| SalesPersonCode | VARCHAR(20) | |
| SalesPersonName | VARCHAR(50) | |
| OutstandingBalance | DECIMAL(18,2) | Open balance sum |
| PercentOfTotal | DECIMAL(9,4) NULL | Row amount / TotalPiutang |

Unique: `(SnapshotKey, Rank)`

### 5.5 `BTR_PortalDashboardSalesmanAttention`

| Column | Type | Description |
| --- | --- | --- |
| SalesmanAttentionId | VARCHAR(13) PK | Generated ID |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| SalesPersonId | VARCHAR(13) | |
| SalesPersonCode | VARCHAR(20) | |
| SalesPersonName | VARCHAR(50) | |
| SignalKey | VARCHAR(30) | See Section 2.4 |
| SignalLabel | VARCHAR(50) | Display label |
| ValueAmount | DECIMAL(18,2) NULL | Balance, omzet, overdue amount, etc. |
| ValueText | VARCHAR(100) NULL | e.g. `"62% (Target Rp 500M)"`, `"45% top customer"`, `"8 dormant customers"` |
| WilayahName | VARCHAR(30) | From salesman master |
| SortOrder | INT | Stable list ordering |

Index: `(SnapshotKey, SortOrder)`

### 5.6 `BTR_PortalDashboardSalesmanSegmentation`

| Column | Type | Description |
| --- | --- | --- |
| SalesmanSegmentationId | VARCHAR(13) PK | Generated ID |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| SegmentType | VARCHAR(20) | `Wilayah` \| `Activity` \| `Segment` |
| SegmentKey | VARCHAR(30) | Normalized key |
| SegmentLabel | VARCHAR(50) | Display label |
| SalesmanCount | INT | Total in segment |
| ActiveCount | INT | Faktur in current month |
| InactiveCount | INT | No current-month Faktur |
| SortOrder | INT | |

Unique: `(SnapshotKey, SegmentType, SegmentKey)`

---

## 6. Backend Implementation

### 6.1 New folder structure

```text
btr.application/ReportingContext/
├── DashboardSnapshotAgg/
│   ├── Services/
│   │   ├── DashboardSalesmanAggregator.cs
│   │   └── DashboardSalesmanKeyResolver.cs
│   ├── Models/
│   │   └── DashboardSalesmanAggregateResult.cs (+ nested row types)
│   ├── Contracts/
│   │   ├── IDashboardSalesmanSnapshotDal.cs
│   │   └── IPiutangOpenBalanceWithSalesmanDal.cs
│   └── UseCases/
│       ├── RefreshDashboardSalesmanSnapshotWorker.cs
│       └── RefreshDashboardSalesmanSnapshotRequest.cs (+ Result)
└── DashboardSalesmanAgg/
    ├── Contracts/
    │   └── IDashboardSalesmanDal.cs
    └── Queries/
        └── GetDashboardSalesmanQuery.cs

btr.application/SalesContext/FakturInfo/
├── FakturView.cs                              [EXTEND]
└── ICustomerLastFakturDal.cs                  [EXTEND]

btr.infrastructure/ReportingContext/
├── DashboardSnapshotAgg/
│   ├── DashboardSalesmanSnapshotDal.cs
│   └── PiutangOpenBalanceWithSalesmanDal.cs
└── DashboardSalesmanAgg/
    └── DashboardSalesmanDal.cs

btr.portal.api/Controllers/Dashboard/
└── SalesmanDashboardController.cs
```

Add all new `.cs` files to respective `.csproj` Compile includes.

### 6.2 Source DAL extensions

#### 6.2.1 `FakturView` + `FakturViewDal`

Add to `FakturView`:

```csharp
public string SalesPersonId { get; set; }
public string SalesPersonCode { get; set; }
```

Extend both `ListData` and `ListTerhapus` SELECT clauses:

```sql
ISNULL(ee.SalesPersonId, '') AS SalesPersonId,
ISNULL(ee.SalesPersonCode, '') AS SalesPersonCode,
```

Existing consumers ignore new properties — no behavior change.

#### 6.2.2 `ICustomerLastFakturWithSalesmanDal`

Extend `ICustomerLastFakturDal`:

```csharp
IEnumerable<CustomerLastFakturWithSalesmanDto> ListLastFakturWithSalesmanByCustomer();
```

```csharp
public class CustomerLastFakturWithSalesmanDto
{
    public string CustomerCode { get; set; }
    public string CustomerName { get; set; }
    public DateTime LastFakturDate { get; set; }
    public string SalesPersonId { get; set; }
    public string SalesPersonName { get; set; }
}
```

**SQL pattern** (Infrastructure only — use `ROW_NUMBER` for deterministic last Faktur):

```sql
WITH Ranked AS (
    SELECT
        ISNULL(cc.CustomerCode, '') AS CustomerCode,
        ISNULL(cc.CustomerName, '') AS CustomerName,
        aa.FakturDate AS LastFakturDate,
        ISNULL(sp.SalesPersonId, '') AS SalesPersonId,
        ISNULL(sp.SalesPersonName, '') AS SalesPersonName,
        ROW_NUMBER() OVER (
            PARTITION BY cc.CustomerId
            ORDER BY aa.FakturDate DESC, aa.FakturId DESC
        ) AS rn
    FROM BTR_Faktur aa
    INNER JOIN BTR_Customer cc ON aa.CustomerId = cc.CustomerId
    LEFT JOIN BTR_SalesPerson sp ON aa.SalesPersonId = sp.SalesPersonId
    WHERE aa.VoidDate = '3000-01-01'
)
SELECT CustomerCode, CustomerName, LastFakturDate, SalesPersonId, SalesPersonName
FROM Ranked
WHERE rn = 1
```

Retain existing `ListLastFakturByCustomer()` unchanged for Customer snapshot worker.

#### 6.2.3 `IPiutangOpenBalanceWithSalesmanDal`

```csharp
public interface IPiutangOpenBalanceWithSalesmanDal
{
    IReadOnlyList<PiutangOpenBalanceWithSalesmanDto> ListOpenBalances();
}

public class PiutangOpenBalanceWithSalesmanDto
{
    public string SalesPersonId { get; set; }
    public string SalesPersonName { get; set; }
    public string CustomerCode { get; set; }
    public string CustomerName { get; set; }
    public DateTime JatuhTempo { get; set; }
    public decimal KurangBayar { get; set; }
}
```

**SQL** — extend `PiutangOpenBalanceDal` join with `BTR_SalesPerson`:

```sql
SELECT
    ISNULL(sp.SalesPersonId, '') AS SalesPersonId,
    ISNULL(sp.SalesPersonName, '') AS SalesPersonName,
    ISNULL(ee.CustomerCode, '') AS CustomerCode,
    ISNULL(ee.CustomerName, '') AS CustomerName,
    ISNULL(aa.DueDate, '3000-01-01') AS JatuhTempo,
    aa.Sisa AS KurangBayar
FROM BTR_Piutang aa
    LEFT JOIN BTR_Faktur bb ON aa.PiutangId = bb.FakturId
    LEFT JOIN BTR_SalesPerson sp ON bb.SalesPersonId = sp.SalesPersonId
    LEFT JOIN BTR_Customer ee ON bb.CustomerId = ee.CustomerId
WHERE aa.Sisa > 1
```

Rows with blank `SalesPersonId` are excluded from salesman aggregates (orphan piutang without invoicing salesman).

#### 6.2.4 `ISalesOmzetTargetDal.ListTargetsForMonth`

```csharp
IReadOnlyDictionary<string, decimal?> ListTargetsForMonth(int year, int month);
```

Returns map keyed by `SalesPersonId`. Missing rows → not in dictionary (treated as no target).

### 6.3 `DashboardSalesmanKeyResolver`

```csharp
public static string ResolveSalesPersonId(string salesPersonId, string salesPersonName)
{
    if (!string.IsNullOrWhiteSpace(salesPersonId))
        return salesPersonId.Trim();
    return string.Empty; // caller must resolve via master name lookup when id blank
}
```

Build authoritative lookup from `ISalesPersonDal.ListData()`:

- Primary key: `SalesPersonId`
- Fallback name map: `SalesPersonName` → `SalesPersonId` (case-insensitive) for piutang rows where only name is populated

### 6.4 `DashboardSalesmanAggregator`

**Input:**

| Input | Purpose |
| --- | --- |
| `IEnumerable<FakturView>` | Current-month sales |
| `IEnumerable<PiutangOpenBalanceWithSalesmanDto>` | Open receivables with invoicing salesman |
| `IEnumerable<CustomerLastFakturWithSalesmanDto>` | Dormant detection + last-invoicing attribution |
| `IEnumerable<SalesPersonModel>` | Master universe, Wilayah, Segment |
| `IReadOnlyDictionary<string, decimal?>` | Per-rep targets for month |
| `DateTime today` | Aging and dormant cutoff |
| `Periode periode` | Current month |
| `DateTime generatedAt` | Snapshot timestamp |

**Constants:**

```csharp
public const int TopSalesmanCount = 10;
public const int DormantDaysThreshold = 90;

public const string SignalBelowTarget = "BelowTarget";
public const string SignalNoTarget = "NoTarget";
public const string SignalHighOverdueExposure = "HighOverdueExposure";
public const string SignalHighPiutangExposure = "HighPiutangExposure";
public const string SignalCustomerConcentration = "CustomerConcentration";
public const string SignalDormantCustomerPortfolio = "DormantCustomerPortfolio";
```

Reuse `DashboardPiutangAggregator.ResolveAgingBucketKey` logic (duplicate constants or extract shared static if minimal).

**Core algorithms:**

| Computation | Logic |
| --- | --- |
| Salesman universe | All `SalesPersonModel` from master + any `SalesPersonId` seen in Faktur/piutang with non-blank id |
| Active set | Reps with ≥1 current-month Faktur (`SalesPersonId`) |
| Omzet per rep | `SUM(GrandTotal)` grouped by `SalesPersonId` |
| Customer count per rep | Distinct `CustomerCode`/`Customer` per rep in month Faktur |
| Target per rep | From `ListTargetsForMonth` dictionary |
| Achievement % | `SalesOmzetChartAchievementPolicy.ComputePercent(omzet, target)` |
| Achievement band | `ExecutiveSalesAchievementBandResolver.Resolve(achievementPercent)` |
| Open balance per rep | `SUM(KurangBayar)` where `> 1`, grouped by `SalesPersonId` |
| Overdue per rep | Rows where aging bucket ≠ `Current`; track distinct overdue customers and overdue amount |
| Top customer concentration | Group month Faktur by rep + customer key; top customer omzet / rep omzet |
| Dormant per rep | Apply M17 dormant rule on customers; attribute to `SalesPersonId` from last-Faktur-with-salesman; exclude customers active this month |
| Top Omzet | Top 10 by omzet; exclude `CompletedOmzet = 0` |
| Top Achievement % | Top 10 by achievement %; require target `> 0` and non-null %; exclude `CompletedOmzet = 0` |
| Top Piutang | Top 10 by open balance; exclude `OutstandingBalance = 0` |
| Attention list | Emit row per (rep, signal); sort by signal priority then name |
| Segmentation | Group master by Wilayah and Segment; count active/inactive per group; Activity summary rows |

**Attention list `Value` column:**

| Signal | ValueAmount | ValueText |
| --- | --- | --- |
| BelowTarget | `CompletedOmzet` | `"{AchievementPercent:N1}% (Target Rp {Target:N0})"` |
| NoTarget | `CompletedOmzet` | `"No target configured"` |
| HighOverdueExposure | Overdue balance sum | `"{OverdueCustomerCount} overdue customers, Rp {amount:N0} overdue"` |
| HighPiutangExposure | Open balance | `"Rp {amount:N0} open piutang ({SharePercent:N1}% of company)"` |
| CustomerConcentration | Top customer omzet | `"{TopCustomerPercent:N1}% top customer"` |
| DormantCustomerPortfolio | — | `"{DormantCustomerCount} dormant customers on book"` |

**Report route hint** (API response only — not stored in DB):

| Signal | `ReportRoute` |
| --- | --- |
| BelowTarget, NoTarget, CustomerConcentration, DormantCustomerPortfolio | `/reports/sales` |
| HighOverdueExposure, HighPiutangExposure | `/reports/piutang` |

**Attention list sorting suggestion:** BelowTarget → NoTarget → HighOverdueExposure → HighPiutangExposure → CustomerConcentration → DormantCustomerPortfolio; then by `SalesPersonName`.

### 6.5 `RefreshDashboardSalesmanSnapshotWorker`

Mirror `RefreshDashboardCustomerSnapshotWorker` pattern:

1. Insert refresh log with `Domain = "Salesman"`.
2. Load inputs via injected DALs.
3. Call `_aggregator.Aggregate(...)`.
4. `_snapshotDal.ReplaceCurrent(aggregate, refreshLogId)` in transaction.
5. Mark success/failure on refresh log.

**Period for Faktur load:** same `CurrentMonthPeriode(today)` helper as Sales/Customer workers.

### 6.6 `DashboardSalesmanSnapshotDal.ReplaceCurrent`

Delete all `SnapshotKey = 'CURRENT'` rows from six Salesman tables; insert new rows; update KPI `LastRefreshLogId`. Follow `DashboardCustomerSnapshotDal` transaction pattern.

Counter prefixes: use `PDS` for salesman snapshot child rows (consistent with `PDC` for customer).

### 6.7 API contract — `GET /api/dashboard/salesmen`

**Auth:** JWT required.

**Response:** `ApiResponse<DashboardSalesmanResponse>`

```csharp
public class DashboardSalesmanResponse
{
    public bool IsAvailable { get; set; }
    public bool IsDataFresh { get; set; }
    public DateTime? GeneratedAt { get; set; }
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }

    public DashboardSalesmanAttentionCards AttentionCards { get; set; }
    public IList<DashboardSalesmanAttentionItem> AttentionList { get; set; }
    public DashboardSalesmanPerformanceRankings PerformanceRankings { get; set; }
    public DashboardSalesmanExposureRankings ExposureRankings { get; set; }
    public DashboardSalesmanSegmentationSummary Segmentation { get; set; }
    public DashboardSalesmanNavigationLinks Navigation { get; set; }
}

public class DashboardSalesmanAttentionCards
{
    public int BelowTargetCount { get; set; }
    public int NoTargetCount { get; set; }
    public int HighOverdueExposureCount { get; set; }
    public int HighPiutangExposureCount { get; set; }
    public int CustomerConcentrationCount { get; set; }
    public int DormantPortfolioCount { get; set; }

    public decimal? TopOmzetSalesmanPercent { get; set; }
    public decimal? TopPiutangSalesmanPercent { get; set; }

    public bool PerformanceRequiresAttention { get; set; }
    public bool CollectionRequiresAttention { get; set; }
    public bool PortfolioRequiresAttention { get; set; }
}

public class DashboardSalesmanAttentionItem
{
    public string SalesPersonId { get; set; }
    public string SalesPersonCode { get; set; }
    public string SalesPersonName { get; set; }
    public string SignalKey { get; set; }
    public string SignalLabel { get; set; }
    public decimal? ValueAmount { get; set; }
    public string ValueText { get; set; }
    public string WilayahName { get; set; }
    public string ReportRoute { get; set; }
    public bool RequiresAttention { get; set; }   // always true for list rows
}

public class DashboardSalesmanRankingRow
{
    public int Rank { get; set; }
    public string SalesPersonId { get; set; }
    public string SalesPersonCode { get; set; }
    public string SalesPersonName { get; set; }
    public decimal Amount { get; set; }
    public decimal? PercentOfTotal { get; set; }
    public decimal? AchievementPercent { get; set; }  // Top Achievement only
    public decimal? TargetAmount { get; set; }        // Top Achievement only
    public string ReportRoute { get; set; }
}

public class DashboardSalesmanPerformanceRankings
{
    public IList<DashboardSalesmanRankingRow> TopOmzet { get; set; }
    public IList<DashboardSalesmanRankingRow> TopAchievement { get; set; }
}

public class DashboardSalesmanExposureRankings
{
    public IList<DashboardSalesmanRankingRow> TopPiutang { get; set; }
}

public class DashboardSalesmanSegmentRow
{
    public string SegmentType { get; set; }
    public string SegmentLabel { get; set; }
    public int SalesmanCount { get; set; }
    public int ActiveCount { get; set; }
    public int InactiveCount { get; set; }
}

public class DashboardSalesmanSegmentationSummary
{
    public IList<DashboardSalesmanSegmentRow> ByWilayah { get; set; }
    public IList<DashboardSalesmanSegmentRow> BySegment { get; set; }  // empty when no segments configured
    public DashboardSalesmanSegmentRow ActiveSummary { get; set; }
    public DashboardSalesmanSegmentRow InactiveSummary { get; set; }
}

public class DashboardSalesmanNavigationLinks
{
    public string SalesDashboardRoute { get; set; }
    public string PiutangDashboardRoute { get; set; }
    public string SalesReportRoute { get; set; }
    public string PiutangReportRoute { get; set; }
}
```

**RequiresAttention derivation (presentation):**

| Card group | `RequiresAttention = true` when |
| --- | --- |
| Performance | `BelowTargetCount > 0` OR `NoTargetCount > 0` |
| Collection | `HighOverdueExposureCount > 0` OR `HighPiutangExposureCount > 0` |
| Portfolio | `DormantPortfolioCount > 0` OR `CustomerConcentrationCount > 0` |

Concentration percentages on cards have no threshold coloring (Q22).

**IsDataFresh:** `(UtcNow − GeneratedAt).TotalMinutes <= SalesmanIntervalMinutes`

### 6.8 Configuration and infrastructure wiring

**`DashboardSnapshotOptions`:**

```csharp
public int SalesmanIntervalMinutes { get; set; } = 30;
```

**Register in:**

- `ApplicationPortalExtensions.cs` — aggregator, worker, MediatR handler
- `InfrastructurePortalExtensions.cs` — snapshot DAL, read DAL, piutang-with-salesman DAL
- `WorkerDependencyConfig` — same registrations for worker host

**Update domain lists in:**

- `HealthController.BuildDomainStatuses` — add `"Salesman"`
- `RefreshDashboardSnapshotsHandler` — add `"Salesman"` case
- `RefreshAllDashboardSnapshotsWorker` — run Salesman worker last (after Customer)
- `btr.portal.worker/Program.cs` — `ValidDomains` includes `Salesman`

**Task Scheduler:** Add sixth salesman job `BTR Portal Dashboard Salesman Refresh` every 30 minutes (document in operational guide).

---

## 7. Frontend Implementation

### 7.1 Route and sidebar

| Item | Change |
| --- | --- |
| Route | `path: 'dashboard/salesmen'`, name: `salesmen-dashboard` |
| Component | `SalesmanDashboardView.vue` |
| `MainLayout.vue` | Add **Salesmen** menu item after Customers, before Inventory |
| Active class | `route.path === '/dashboard/salesmen'` |

### 7.2 Store and API

- `fetchDashboardSalesman()` in `dashboardApi.ts`
- `salesman` ref + `loadSalesman()` in `dashboardStore.ts`
- TypeScript interfaces mirroring `DashboardSalesmanResponse`

### 7.3 Page layout — Proposal A

```text
┌─────────────────────────────────────────────────────────────┐
│ Salesman Performance                 Last Refreshed: …  [↻] │
│ Which salesman requires management attention?                  │
│ Subtitle: Current month sales + open piutang by salesman     │
├─────────────────────────────────────────────────────────────┤
│ [⚠ Dashboard Data Not Fresh]  (when !IsDataFresh)            │
├─────────────────────────────────────────────────────────────┤
│ 1. ATTENTION CARDS (3 groups)                                │
│ Performance | Collection Exposure | Portfolio                │
├─────────────────────────────────────────────────────────────┤
│ 2. ATTENTION LIST (DataTable)                                │
│ Code | Salesman | Signal | Detail | Wilayah | [→ Report]     │
├─────────────────────────────────────────────────────────────┤
│ 3. PERFORMANCE RANKINGS                                      │
│ Top 10 Omzet (month) | Top 10 Achievement %                  │
├─────────────────────────────────────────────────────────────┤
│ 4. EXPOSURE RANKINGS                                         │
│ Top 10 Piutang (all open)                                    │
├─────────────────────────────────────────────────────────────┤
│ 5. SEGMENTATION                                              │
│ By Wilayah | Active vs Inactive | By Segment (if data)       │
├─────────────────────────────────────────────────────────────┤
│ 6. NAVIGATION                                                │
│ → Sales Dashboard / Sales Report                             │
│ → Piutang Dashboard / Piutang Report                         │
└─────────────────────────────────────────────────────────────┘
```

### 7.4 Components

| Component | Purpose |
| --- | --- |
| `DashboardDetailLayout.vue` | Page shell (reuse) |
| `KpiCard.vue` | Attention card groups (reuse) |
| `Top10RankingTable.vue` | Rankings with optional `%` and achievement columns (reuse/extend minimally) |
| `ExecutiveAttentionCard.vue` | Attention Indicator border styling (reuse from M16/M17) |
| `SalesmanAttentionCardGroup.vue` | **New** — wraps KPI cards with Attention Indicator |
| `SalesmanAttentionList.vue` | **New** — attention list DataTable with contextual report link |
| `SalesmanRankingsSection.vue` | **New** — performance rankings (Omzet + Achievement) |
| `SalesmanExposureSection.vue` | **New** — Top 10 Piutang table |
| `SalesmanSegmentationSection.vue` | **New** — wilayah/segment/activity tables |
| `SalesmanNavigationSection.vue` | **New** — links to domain dashboards and reports |

### 7.5 Attention card content

| Card group | Fields |
| --- | --- |
| **Performance** | Below Target (count) · No Target (count) |
| **Collection Exposure** | High Overdue Exposure (count) · High Piutang Exposure (count) |
| **Portfolio** | Dormant Portfolio (count) · Top Omzet Salesman % · Top Piutang Salesman % (informational) |

Apply **Attention Indicator** when respective `*RequiresAttention` is true. Concentration % fields have no threshold coloring (Q22).

### 7.6 Row click and pre-filter

**Helper** `navigateToReport(route: string, salesmanName: string)`:

```typescript
router.push({ path: route, query: { q: salesmanName } })
```

Report views should already read `?q=` from M17 — verify Sales and Piutang report free-text filters match `SalesName` / `Sales` column. No report API changes required.

Pre-filter uses **SalesPersonName** in `?q=` query param.

### 7.7 Rankings table columns

| Top Omzet | Top Achievement % | Top Piutang |
| --- | --- | --- |
| Rank | Rank | Rank |
| SalesPersonCode | SalesPersonCode | SalesPersonCode |
| SalesPersonName | SalesPersonName | SalesPersonName |
| Omzet + % of total | Achievement % + Target + Omzet | Outstanding + % of total |

Row click → respective report with pre-filter.

For Top Achievement %, optionally show M16 band badge (Healthy/Warning/Critical) using same styling as executive Sales card — presentation only, not a new severity system.

---

## 8. Testing

### 8.1 Unit tests — `DashboardSalesmanAggregatorTest`

| Case | Assertion |
| --- | --- |
| Top 10 Omzet ranking | Correct order by GrandTotal; tie-break by name; excludes zero omzet |
| Top 10 Achievement ranking | Ordered by achievement %; excludes null % and zero omzet |
| Top 10 Piutang ranking | Matches piutang grouping by SalesPersonId; excludes zero balance |
| SalesPersonId key | Groups by id not name |
| BelowTarget signal | 79% achievement → Critical → signal emitted |
| BelowTarget — healthy | 105% → no BelowTarget signal |
| NoTarget signal | Omzet > 0, no target row → signal |
| NoTarget — has target | Target 100, omzet 50 → BelowTarget not NoTarget |
| NoTarget — inactive zero | No activity → no NoTarget signal |
| HighOverdueExposure | Rep with past-due row → signal with overdue count |
| HighPiutangExposure | Rep with open balance → signal with share % |
| CustomerConcentration | Top customer 45% of rep omzet → signal with % text |
| Dormant portfolio | Customer dormant 91d, last invoice by rep A → attributed to A |
| Dormant — active this month | Customer invoiced this month → not dormant |
| Dormant — no history | Customer never invoiced → not dormant |
| Multi-signal rep | BelowTarget + HighOverdue → two attention rows |
| Segmentation Wilayah | Blank Wilayah → `"Unknown"` group |
| Active vs Inactive | Rep with/without month Faktur counted correctly |
| Concentration company % | Top rep omzet / team total |

### 8.2 Unit tests — `DashboardSalesmanKeyResolverTest`

Verify `SalesPersonId` preferred; blank id handled; name fallback map behavior.

### 8.3 Integration / manual test checklist

1. Deploy SQL tables; run Salesman refresh worker; verify six tables populated.
2. `GET /api/dashboard/salesmen` returns complete response with `GeneratedAt`.
3. Navigate to `/dashboard/salesmen` — all six sections render in fixed order.
4. Sidebar shows **Salesmen** after Customers, before Inventory.
5. Below Target count matches reps with achievement 80–99% or <80% spot-check.
6. Top 10 Omzet reconciles with Sales Report grouped by salesman for current month.
7. Top 10 Piutang reconciles with Piutang Report / FF1 grouped by `SalesName`.
8. Top 10 Achievement % matches Desktop RO2 formula for sample rep.
9. Dormant portfolio count plausible — spot-check one customer last Faktur salesman.
10. Click attention list row → correct report opens with salesman search pre-filled.
11. Click ranking row → correct report with pre-filter.
12. **"⚠ Dashboard Data Not Fresh"** when snapshot exceeds 30-minute interval.
13. Executive dashboard **unchanged**.
14. Sales Dashboard Top 10 Salesman **unchanged** (same omzet values, no Code added there).
15. `GET /api/health/dashboard-snapshots` includes Salesman domain.
16. Manual refresh: `POST /api/admin/dashboard/refresh` with `{ "Domain": "Salesman" }` succeeds.
17. Worker CLI: `btr.portal.worker --domain Salesman --triggered-by Manual` succeeds.

---

## 9. Risk Analysis

| Risk | Likelihood | Impact | Mitigation |
| --- | --- | --- | --- |
| SalesPersonName/id mismatch across Faktur and piutang | Medium | Medium | Key by `SalesPersonId`; master name fallback map; extend `FakturViewDal` to emit id |
| Piutang rows without Faktur join (blank salesman) | Low | Low | Exclude from salesman aggregates; document as data quality edge case |
| Last-Faktur-with-salesman SQL performance | Medium | Medium | `ROW_NUMBER` over indexed `BTR_Faktur(CustomerId, FakturDate DESC)`; measure in staging |
| N+1 target lookups | Medium | Low | Batch `ListTargetsForMonth` |
| Concentration signals without thresholds inflate list | Medium | Low | PO Q22 — informational inclusion; document in operational guide |
| Piutang report vs dashboard total mismatch on drill-down | Medium | Low | UI hint: dashboard = all-open; report defaults to period filter |
| Salesman refresh fails mid-RefreshAll | Low | Medium | Transactional `ReplaceCurrent`; refresh log marks Failed |
| Name-based pre-filter ambiguity | Low | Low | Use exact salesman name from snapshot; substring free-text match acceptable |
| Orphan salesman names on legacy Faktur | Low | Medium | Resolve via master; unmatched names excluded from rankings requiring Code |

---

## 10. Documentation Updates (post-delivery)

| Document | Update |
| --- | --- |
| `docs/features/btr-portal/btr-portal-operational.md` | Salesman Performance route, attention signals, attribution rules, drill-down path, refresh cadence |
| `docs/features/btr-portal/btr-portal-domain.md` | Salesman snapshot domain KPI definitions and semantics |
| `docs/features/btr-portal/btr-portal-architecture.md` | `DashboardSalesmanAgg`, tables, worker, API endpoint |
| `docs/features/materialized-dashboard/materialized-dashboard-domain.md` | Salesman domain refresh rules |
| `docs/work/btr-portal/M18 Salesman Performance - Analysis.md` | Link to this plan |

---

## 11. Implementation Steps

Execute in order. Each phase should compile before proceeding.

### Phase 1 — Database

1. Create six `BTR_PortalDashboardSalesman*.sql` table scripts.
2. Add to `btr.sql.sqlproj`; deploy to dev database.

### Phase 2 — Source DAL extensions

3. Extend `FakturView` + `FakturViewDal` with `SalesPersonId`, `SalesPersonCode`.
4. Add `ICustomerLastFakturWithSalesmanDal` method + `CustomerLastFakturDal` implementation.
5. Add `IPiutangOpenBalanceWithSalesmanDal` + `PiutangOpenBalanceWithSalesmanDal`.
6. Add `ISalesOmzetTargetDal.ListTargetsForMonth` + implementation.

### Phase 3 — Backend core

7. Add `DashboardSalesmanKeyResolver`.
8. Add aggregate models and `DashboardSalesmanAggregator` with unit tests (Section 8.1).
9. Add `IDashboardSalesmanSnapshotDal` + `DashboardSalesmanSnapshotDal.ReplaceCurrent`.
10. Add `RefreshDashboardSalesmanSnapshotWorker` + request/result types.
11. Add `SalesmanIntervalMinutes` to `DashboardSnapshotOptions`.

### Phase 4 — Backend integration

12. Register all new services in Application and Infrastructure DI.
13. Update `RefreshAllDashboardSnapshotsWorker`, `RefreshDashboardSnapshotsHandler`, `HealthController`, worker `Program.cs`.
14. Add `DashboardSalesmanAgg` read path: query, handler, `DashboardSalesmanDal`, `SalesmanDashboardController`.
15. Verify `GET /api/dashboard/salesmen` against populated snapshot.
16. Run manual Salesman refresh via admin API and worker CLI.

### Phase 5 — Frontend

17. Add TypeScript models and `fetchDashboardSalesman`.
18. Extend `dashboardStore` with `loadSalesman`.
19. Add route and sidebar **Salesmen** item.
20. Create section components (Section 7.4).
21. Implement `SalesmanDashboardView.vue` per Section 7.3 layout.
22. Verify `?q=` pre-filter on Sales and Piutang report views for salesman name.
23. Wire row clicks and navigation links.

### Phase 6 — Verification and docs

24. Run unit tests and manual checklist (Section 8.3).
25. Add Task Scheduler job for Salesman domain (operations).
26. Update feature documentation (Section 10).

---

## 12. Acceptance Criteria

M18 is complete when:

1. `/dashboard/salesmen` displays **Salesman Performance** (Proposal A layout) for all authenticated users.
2. Sidebar shows **Dashboard → Salesmen** at `/dashboard/salesmen`.
3. Dedicated `BTR_PortalDashboardSalesman*` tables exist and are populated by `RefreshDashboardSalesmanSnapshotWorker`.
4. `GET /api/dashboard/salesmen` returns attention cards, attention list, Top 10 Omzet, Top 10 Achievement %, Top 10 Piutang, and segmentation per Section 12.10 of analysis.
5. All ranking rows include **SalesPersonCode** and **% of domain total** where applicable.
6. Attention list includes only approved signals: Below Target · No Target · High Overdue Exposure · High Piutang Exposure · Customer Concentration · Dormant Customer Portfolio.
7. Achievement bands per salesman follow M16 thresholds (80/100).
8. Sales metrics use **current calendar month Faktur GrandTotal**; Piutang metrics use **all-time open balance**.
9. Dormant customers attributed via **last invoicing salesman**; M17 90-day rule applies.
10. Salesman row click opens correct report with **salesman name pre-filter** applied.
11. Navigation section links to Sales/Piutang dashboards and reports.
12. **Attention Indicator** presentation matches M16/M17; concentration % has no threshold coloring.
13. **"⚠ Dashboard Data Not Fresh"** banner when Salesman snapshot is stale.
14. Executive dashboard and Sales Dashboard Top 10 **unchanged**.
15. Aggregator unit tests pass; health endpoint lists Salesman domain.
16. Top rankings **exclude reps with ranking value = 0**; inactive reps remain in segmentation.

---

## 13. Handoff Notes for Implementer

- **Section 2 is authoritative** — do not add Effective Call, route compliance, retur, pipeline omzet, collection effectiveness, trend charts, Bottom 10, or executive changes.
- **Read source DALs in Salesman worker** — do not compose from Sales/Piutang/Customer snapshot tables.
- **Do not modify** `DashboardSalesFakturAggregator`, `BTR_PortalDashboardSalesTopSalesman`, or `DashboardExecutiveComposer`.
- **Reuse** `SalesOmzetChartAchievementPolicy`, `ExecutiveSalesAchievementBandResolver`, and aging bucket definitions from `DashboardPiutangAggregator` — verify `DaysOver90` and `Current` bucket keys match exactly.
- **Piutang semantics:** all-time open balance (`KurangBayar > 1`) — same filter as `PiutangOpenBalanceDal`, with FF1 salesman join added.
- **No Target:** month activity means `OmzetAmount > 0` OR `CustomerCount > 0` for the rep — not merely existing in master.
- **CustomerConcentration / HighPiutangExposure:** informational signals per Q22 — include when metric is computable (> 0 omzet / > 0 balance), not filtered by arbitrary % thresholds.
- **Dormant:** `(today - LastFakturDate).Days >= 90` AND customer in last-Faktur results (implies history). Customers active this month are **not** dormant. Attribute to last invoicing `SalesPersonId`.
- **ReplaceCurrent** must be transactional across all six Salesman tables.
- **Pre-filter** uses `SalesPersonName` in `?q=` — report search fields include salesman name column.
- **Optional:** show achievement band badge on Top Achievement % table and BelowTarget attention rows — reuse M16 CSS classes only.
- M19–M25 scope boundaries are **not** part of this delivery.
