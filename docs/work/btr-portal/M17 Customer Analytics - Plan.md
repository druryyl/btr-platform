# Implementation Plan: M17 Customer Analytics Dashboard

## Document Status

| Field | Value |
| --- | --- |
| Initiative | M17 Customer Analytics — **Which customers require management attention?** |
| Authoritative requirements | `docs/work/btr-portal/M17-Customer-Analytics-Analysis.md` — **Section 11 (Final Product Decisions)** |
| Related features | `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-operational.md`, `docs/features/btr-portal/btr-portal-architecture.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md` |
| Reference pattern | Domain dashboards (M13–M15) snapshot worker + read API; M16 Attention Indicator presentation |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |
| Status | **Ready for implementation** |
| Product decisions | **Confirmed** — analysis Section 11, 2026-06-08 |

---

## 1. Goal

Deliver **Customer Analytics** at `/dashboard/customers` — a dedicated customer-centric management view covering **Sales and Piutang** to answer *Which customers require management attention?*

**Primary outcomes:**

- New route `/dashboard/customers` with page title **Customer Analytics** for all authenticated users (no role-based routing).
- **Dedicated Customer snapshot domain** (`BTRPD_Customer*`) with its own refresh worker — materialized KPIs, not live composition.
- **Proposal A layout** (fixed section order): Attention Cards → Attention List → Top Customer Rankings → Segmentation Summary → Navigation.
- Mandatory **Top 10 by Omzet** (current month Faktur) and **Top 10 by Piutang** (all-time open balance), each with `CustomerCode`.
- **Attention List** with approved signals: Overdue · Dormant (90-day rule) · Plafond breach · Suspended with active sales.
- Customer row click → Sales or Piutang Report with **customer name pre-filter** applied.
- Navigation path: **Customer Analytics → Domain Dashboard → Report** (M16 pattern).
- **Supplements** Executive Dashboard Top 5 Customers — does **not** replace or modify executive dashboard.

**Explicitly out of scope (PO confirmed):**

- Retur, Effective Call, GPS, Faktur Kembali aggregate, declining purchase activity, collection effectiveness / DSO.
- Harga Type segmentation, strategic customer concept, historical trend charts.
- New Customer Report route, additional Piutang Report columns, Executive Dashboard changes.
- Changes to existing Sales/Piutang snapshot tables, workers, or domain dashboard APIs.

---

## 2. Authoritative Product Decisions

Source: analysis Section 11. Do not re-decide these rules during implementation.

### 2.1 Positioning and audience

| Decision | Value |
| --- | --- |
| Route | `/dashboard/customers` |
| Page title | **Customer Analytics** |
| Audience | All authenticated users — no RBAC in M17 |
| Sidebar | Dashboard → **Customers** (between Piutang and Inventory) |
| Executive relationship | **Supplements** Top 5 Customers on `/dashboard` — no executive changes |

### 2.2 Scope boundaries

| Decision | Value |
| --- | --- |
| Domains | **Sales + Piutang only** |
| Materialization | Dedicated `BTRPD_Customer*` tables + refresh worker |
| Dashboard only | No new Customer Report |

### 2.3 Attention rules

| Rule | Value |
| --- | --- |
| Dormant | No Faktur for **90 days** AND customer has **prior Faktur history** |
| Overdue | **Any** overdue balance on customer is attention-worthy |
| Plafond breach | Open balance **>** `Plafond` (customer master) |
| Suspended + sales | `IsSuspend = true` AND Faktur in **current calendar month** |
| Concentration | Top Omzet % and Top Piutang % shown **without warning thresholds** |
| Presentation | Generic **Attention Indicator** (M16 pattern) — no customer-specific severity bands |

**Approved attention list signals:** Overdue · Dormant · Plafond breach · Suspended with active sales

### 2.4 Data semantics

| Metric class | Period / scope |
| --- | --- |
| Sales (omzet, active count, Top Omzet, suspended+sales) | **Current calendar month** — non-void Faktur `GrandTotal` |
| Piutang (balance, overdue, Top Piutang, plafond, >90d exposure) | **All-time open balance** — `KurangBayar > 1` |
| Customer key | **`CustomerCode` first**, fallback `CustomerName` — same as existing aggregators |
| Rankings | Top **N = 10** for all customer rankings |

### 2.5 Layout (Proposal A — fixed order)

1. Customer Attention Cards  
2. Customer Attention List  
3. Top Customer Rankings (Top 10 Omzet \| Top 10 Piutang)  
4. Segmentation Summary (Klasifikasi · Wilayah · Active vs Dormant)  
5. Navigation to Sales/Piutang dashboards and reports  

### 2.6 Drill-down

| Action | Target |
| --- | --- |
| Top Omzet row / sales-related signal | `/reports/sales` with customer name pre-filter |
| Top Piutang row / piutang-related signal | `/reports/piutang` with customer name pre-filter |
| Domain navigation links | `/dashboard/sales`, `/dashboard/piutang`, `/reports/sales`, `/reports/piutang` |

---

## 3. Architecture Overview

### 3.1 Target topology

```text
Source DALs (read at refresh time — NOT from other snapshot tables)
  IFakturViewDal              → current-month Faktur rows
  ICustomerLastFakturDal      → last Faktur date per customer (all history)  [NEW]
  IPiutangOpenBalanceDal      → open balance rows
  ICustomerDal                → master: Plafond, IsSuspend, Klasifikasi, Wilayah
    ↓
RefreshDashboardCustomerSnapshotWorker
    ↓ DashboardCustomerAggregator
BTRPD_Customer* tables
    ↓
Browser → GET /api/dashboard/customers
    ↓ MediatR
GetDashboardCustomerHandler
    ↓ IDashboardCustomerDal
DashboardCustomerDal → DashboardCustomerResponse

Existing unchanged:
  GET /api/dashboard/executive
  GET /api/dashboard/sales | piutang
  GET /api/reports/sales | piutang
```

**Why read source DALs, not Sales/Piutang snapshots:**

- Piutang Top Customer snapshot stores **CustomerName only** — M17 requires **CustomerCode** (Q29).
- Dormant, plafond breach, and suspended+sales require **customer master + full Faktur history** not present in domain snapshots.
- Keeps Customer domain self-contained; avoids coupling refresh order to Sales/Piutang workers.

### 3.2 Design principles

| Principle | Application |
| --- | --- |
| Reuse business rules | Copy authoritative logic from `DashboardPiutangAggregator` and `DashboardSalesFakturAggregator` — aging buckets, customer key, `KurangBayar > 1`, void exclusion |
| Dedicated snapshot | Customer KPIs materialized in own tables (Q39–Q40) |
| No duplicate SQL in Application | New read queries live in Infrastructure DALs only |
| Preserve domain dashboards | Sales/Piutang detail pages unchanged |
| Attention Indicator only | No automated Healthy/Warning/Critical bands on customer metrics |
| Fail gracefully | Empty/missing snapshot → unavailable sections with clear UI message |

### 3.3 Architecture decisions

| Decision | Choice | Rationale |
| --- | --- | --- |
| Snapshot approach | **New Customer domain** with 5 tables | PO Q39–Q40; cross-domain metrics not representable in existing tables |
| Data inputs | **Source DALs** at refresh | CustomerCode, dormant, plafond, suspended require sources beyond domain snapshots |
| Dormant data access | **`ICustomerLastFakturDal.ListLastFakturByCustomer()`** | Efficient `MAX(FakturDate)` aggregate — avoids loading full Faktur history into memory |
| Customer key helper | **`DashboardCustomerKeyResolver`** (shared static) | Single implementation of Code-first rule; aggregators delegate or duplicate minimally |
| Refresh cadence | **30 minutes** (`CustomerIntervalMinutes`) | Balances collection urgency with cost of full-history dormant scan; aligns with Sales cadence |
| RefreshAll order | Piutang → Inventory → Sales → Purchasing → **Customer** | Customer last; independent of other snapshots but runs in same scheduler pass |
| Read API | **`GET /api/dashboard/customers`** | Mirrors domain dashboard pattern |
| Composition layer | **None** — direct snapshot read | Unlike M16 executive; all derivations happen in aggregator at refresh |
| Existing piutang top table | **Do not alter** | Executive and Piutang dashboard continue using `BTRPD_PiutangTopCustomer` |
| Attention list rows | **One row per customer × signal** | Customer with two signals appears twice; simplifies counts and report routing |
| Pre-filter mechanism | **Route query `?q=`** → report store `freeText` | Reuses `useReportFreeTextFilter`; no report API changes |
| Staleness banner | Customer `GeneratedAt` vs `CustomerIntervalMinutes` | Same copy as M16: **"⚠ Dashboard Data Not Fresh"** |

---

## 4. Impact Analysis

### 4.1 Affected modules

| Layer | Module | Change type |
| --- | --- | --- |
| SQL | `btr.sql/Tables/ReportingContext/BTRPD_Customer*.sql` (5 tables) | **New** |
| SQL | `btr.sql.sqlproj` | Include new tables |
| Application | `DashboardSnapshotAgg/Services/DashboardCustomerAggregator.cs` | **New** |
| Application | `DashboardSnapshotAgg/Services/DashboardCustomerKeyResolver.cs` | **New** |
| Application | `DashboardSnapshotAgg/Models/DashboardCustomer*.cs` | **New** aggregate models |
| Application | `DashboardSnapshotAgg/Contracts/IDashboardCustomerSnapshotDal.cs` | **New** |
| Application | `DashboardSnapshotAgg/UseCases/RefreshDashboardCustomerSnapshotWorker.cs` | **New** |
| Application | `DashboardSnapshotAgg/DashboardSnapshotOptions.cs` | Add `CustomerIntervalMinutes` |
| Application | `DashboardSnapshotAgg/UseCases/RefreshAllDashboardSnapshotsWorker.cs` | Register Customer worker |
| Application | `DashboardSnapshotAgg/Commands/RefreshDashboardSnapshotsCommand.cs` | Add Customer domain |
| Application | `DashboardCustomerAgg/` (query, contracts, DTOs) | **New** read-side aggregate |
| Application | `SalesContext/FakturInfo/ICustomerLastFakturDal.cs` | **New** contract + DTO |
| Infrastructure | `DashboardSnapshotAgg/DashboardCustomerSnapshotDal.cs` | **New** |
| Infrastructure | `DashboardCustomerAgg/DashboardCustomerDal.cs` | **New** |
| Infrastructure | `SalesContext/FakturInfoAgg/CustomerLastFakturDal.cs` | **New** |
| API | `Controllers/Dashboard/CustomerDashboardController.cs` | **New** |
| API | `HealthController.cs` | Add Customer domain |
| API | DI registrations | New DALs, worker, aggregator |
| Worker | `btr.portal.worker/Program.cs` | Add `Customer` to `--domain` |
| Frontend | `router/index.ts` | Route `/dashboard/customers` |
| Frontend | `layouts/MainLayout.vue` | Sidebar **Customers** item |
| Frontend | `views/dashboard/CustomerDashboardView.vue` | **New** |
| Frontend | `components/dashboard/Customer*.vue` | **New** section components |
| Frontend | `models/dashboard.ts`, `api/dashboardApi.ts`, `stores/dashboardStore.ts` | Customer types and loader |
| Frontend | `views/reports/SalesReportView.vue`, `PiutangReportView.vue` | Read `?q=` on mount for pre-filter |
| Tests | `btr.test/ReportingContext/` | Aggregator unit tests |
| Docs | Post-delivery feature docs | Operational, domain, architecture updates |

### 4.2 Unaffected modules

| Module | Reason |
| --- | --- |
| `DashboardExecutiveComposer`, `GET /api/dashboard/executive` | PO Q4 — no executive changes |
| Existing Sales/Piutang snapshot workers, aggregators, tables | Customer domain is additive |
| Domain dashboard views (Sales, Piutang) | Detail layer unchanged |
| Report DALs and API contracts | No column additions (Q37) |
| BTR Desktop | No changes |

### 4.3 Metric traceability

| Customer dashboard field | Source at refresh | Rule |
| --- | --- | --- |
| Active customer count | Current-month `FakturView` | Distinct customer keys with Faktur in month |
| Dormant customer count | `ICustomerLastFakturDal` | Last Faktur `< today − 90d` AND at least one historical Faktur |
| Overdue customer count | `PiutangOpenBalanceDto` | Distinct customers with any balance in non-`Current` aging bucket |
| Plafond breach count | Piutang rows + `CustomerModel.Plafond` | `SUM(KurangBayar) > Plafond` where `Plafond > 0` |
| Suspended-with-sales count | Master `IsSuspend` + current-month Faktur | Customer key in both sets |
| > 90 day exposure amount | Piutang aging | Sum of balances in `DaysOver90` bucket (company-level) |
| Top Omzet customer % | Top-1 omzet / Total Omzet (month) | Same concentration pattern as `DashboardExecutiveComposer` |
| Top Piutang customer % | Top-1 balance / Total Piutang | Same as executive Top Customer % |
| Top 10 Omzet | Current-month Faktur grouped by customer | `SUM(GrandTotal)` desc; name asc tie-break |
| Top 10 Piutang | Open balances grouped by customer | `SUM(KurangBayar)` desc; name asc tie-break |
| Segmentation by Klasifikasi | `ICustomerDal.ListData()` | Count per `KlasifikasiName` (blank → `"Unknown"`); active/dormant sub-counts |
| Segmentation by Wilayah | Customer master | Count per `WilayahName` (blank → `"Unknown"`) |
| Active vs Dormant summary | Dormant rule + active rule | Two summary rows under `SegmentType = Activity` |

**Reconciliation notes:**

- Top Piutang amounts should match Piutang Dashboard Top 10 when keyed by customer name (CustomerCode adds precision for drill-down).
- Top Omzet amounts should match Sales Report customer totals for current month.
- Piutang report default period differs from dashboard (month filter vs all-open) — document on drill-down, same as existing Piutang dashboard behavior.

---

## 5. Database Design

Deploy all tables with `SnapshotKey = 'CURRENT'` delete-and-replace pattern (consistent with existing dashboard snapshots).

### 5.1 `BTRPD_CustomerKpi`

Single row per refresh — headline metrics for attention cards.

| Column | Type | Description |
| --- | --- | --- |
| SnapshotKey | VARCHAR(10) PK | `'CURRENT'` |
| GeneratedAt | DATETIME | Refresh timestamp |
| PeriodYear | INT | Sales period year |
| PeriodMonth | INT | Sales period month |
| TotalOmzet | DECIMAL(18,2) | Current-month invoiced total (concentration denominator) |
| TotalPiutang | DECIMAL(18,2) | All-time open balance total |
| ActiveCustomerCount | INT | Distinct customers invoiced this month |
| DormantCustomerCount | INT | 90-day dormant rule |
| OverdueCustomerCount | INT | Distinct customers with overdue balance |
| PlafondBreachCount | INT | Customers exceeding credit limit |
| SuspendedWithSalesCount | INT | Suspended flag + current-month Faktur |
| AgingOver90Amount | DECIMAL(18,2) | Company >90-day bucket total |
| TopOmzetCustomerPercent | DECIMAL(9,4) NULL | Top-1 omzet / TotalOmzet |
| TopPiutangCustomerPercent | DECIMAL(9,4) NULL | Top-1 balance / TotalPiutang |
| LastRefreshLogId | VARCHAR(13) | FK to refresh log |

### 5.2 `BTRPD_CustomerTopOmzet`

| Column | Type | Description |
| --- | --- | --- |
| CustomerTopOmzetId | VARCHAR(13) PK | Generated ID |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| Rank | INT | 1–10 |
| CustomerCode | VARCHAR(20) | Drill-down key |
| CustomerName | VARCHAR(50) | Display |
| OmzetAmount | DECIMAL(18,2) | `SUM(GrandTotal)` |
| PercentOfTotal | DECIMAL(9,4) NULL | Row amount / TotalOmzet |

Unique: `(SnapshotKey, Rank)`

### 5.3 `BTRPD_CustomerTopPiutang`

| Column | Type | Description |
| --- | --- | --- |
| CustomerTopPiutangId | VARCHAR(13) PK | Generated ID |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| Rank | INT | 1–10 |
| CustomerCode | VARCHAR(20) | Drill-down key |
| CustomerName | VARCHAR(50) | Display |
| OutstandingBalance | DECIMAL(18,2) | Open balance sum |
| PercentOfTotal | DECIMAL(9,4) NULL | Row amount / TotalPiutang |

Unique: `(SnapshotKey, Rank)`

### 5.4 `BTRPD_CustomerAttention`

| Column | Type | Description |
| --- | --- | --- |
| CustomerAttentionId | VARCHAR(13) PK | Generated ID |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| CustomerCode | VARCHAR(20) | |
| CustomerName | VARCHAR(50) | |
| SignalKey | VARCHAR(30) | `Overdue` \| `Dormant` \| `PlafondBreach` \| `SuspendedWithSales` |
| SignalLabel | VARCHAR(50) | Display label |
| ValueAmount | DECIMAL(18,2) NULL | Balance, plafond excess, etc. |
| ValueText | VARCHAR(50) NULL | e.g. days since last purchase for Dormant |
| WilayahName | VARCHAR(30) | From customer master |
| SortOrder | INT | Stable list ordering |

Index: `(SnapshotKey, SortOrder)`

### 5.5 `BTRPD_CustomerSegmentation`

| Column | Type | Description |
| --- | --- | --- |
| CustomerSegmentationId | VARCHAR(13) PK | Generated ID |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| SegmentType | VARCHAR(20) | `Klasifikasi` \| `Wilayah` \| `Activity` |
| SegmentKey | VARCHAR(30) | Normalized key |
| SegmentLabel | VARCHAR(50) | Display label |
| CustomerCount | INT | Total in segment |
| ActiveCount | INT | Faktur in current month |
| DormantCount | INT | 90-day dormant rule |
| SortOrder | INT | |

Unique: `(SnapshotKey, SegmentType, SegmentKey)`

---

## 6. Backend Implementation

### 6.1 New folder structure

```text
btr.application/ReportingContext/
├── DashboardSnapshotAgg/
│   ├── Services/
│   │   ├── DashboardCustomerAggregator.cs
│   │   └── DashboardCustomerKeyResolver.cs
│   ├── Models/
│   │   └── DashboardCustomerAggregateResult.cs (+ nested row types)
│   ├── Contracts/
│   │   └── IDashboardCustomerSnapshotDal.cs
│   └── UseCases/
│       ├── RefreshDashboardCustomerSnapshotWorker.cs
│       └── RefreshDashboardCustomerSnapshotRequest.cs (+ Result)
└── DashboardCustomerAgg/
    ├── Contracts/
    │   └── IDashboardCustomerDal.cs
    └── Queries/
        └── GetDashboardCustomerQuery.cs

btr.application/SalesContext/FakturInfo/
└── ICustomerLastFakturDal.cs

btr.infrastructure/ReportingContext/
├── DashboardSnapshotAgg/
│   └── DashboardCustomerSnapshotDal.cs
└── DashboardCustomerAgg/
    └── DashboardCustomerDal.cs

btr.infrastructure/SalesContext/FakturInfoAgg/
└── CustomerLastFakturDal.cs

btr.portal.api/Controllers/Dashboard/
└── CustomerDashboardController.cs
```

Add all new `.cs` files to respective `.csproj` Compile includes.

### 6.2 `ICustomerLastFakturDal`

```csharp
public interface ICustomerLastFakturDal
{
    IEnumerable<CustomerLastFakturDto> ListLastFakturByCustomer();
}

public class CustomerLastFakturDto
{
    public string CustomerCode { get; set; }
    public string CustomerName { get; set; }
    public DateTime LastFakturDate { get; set; }
}
```

**SQL pattern** (Infrastructure only):

```sql
SELECT
    ISNULL(cc.CustomerCode, '') AS CustomerCode,
    ISNULL(cc.CustomerName, '') AS CustomerName,
    MAX(aa.FakturDate) AS LastFakturDate
FROM BTR_Faktur aa
INNER JOIN BTR_Customer cc ON aa.CustomerId = cc.CustomerId
WHERE aa.VoidDate = '3000-01-01'
GROUP BY cc.CustomerId, cc.CustomerCode, cc.CustomerName
```

### 6.3 `DashboardCustomerKeyResolver`

Mirror existing logic from `DashboardPiutangAggregator.ResolveCustomerKey` and `DashboardSalesFakturAggregator.ResolveCustomerKey`:

```csharp
public static string ResolveCodeFirst(string customerCode, string customerName)
{
    if (!string.IsNullOrWhiteSpace(customerCode))
        return customerCode.Trim();
    return customerName?.Trim() ?? string.Empty;
}
```

Use for all cross-source joins (Faktur, piutang, master).

### 6.4 `DashboardCustomerAggregator`

**Input:**

| Input | Purpose |
| --- | --- |
| `IEnumerable<FakturView>` | Current-month sales |
| `IEnumerable<PiutangOpenBalanceDto>` | Open receivables |
| `IEnumerable<CustomerLastFakturDto>` | Dormant detection |
| `IEnumerable<CustomerModel>` | Plafond, IsSuspend, Klasifikasi, Wilayah |
| `DateTime today` | Aging and dormant cutoff |
| `DateTime generatedAt` | Snapshot timestamp |

**Constants:**

```csharp
public const int TopCustomerCount = 10;
public const int DormantDaysThreshold = 90;
// Reuse aging bucket keys from DashboardPiutangAggregator — reference same definitions
private const string AgingOver90BucketKey = "DaysOver90";
```

**Core algorithms:**

| Computation | Logic |
| --- | --- |
| Customer key map | Build dictionary keyed by `ResolveCodeFirst` across all sources |
| Active set | Customer keys appearing in current-month Faktur |
| Dormant set | `LastFakturDate <= today − 90` AND historical Faktur exists; exclude active |
| Overdue per customer | Any piutang row with aging bucket ≠ `Current` |
| Open balance per customer | `SUM(KurangBayar)` where `> 1` |
| Plafond breach | `Plafond > 0` AND open balance > Plafond |
| Suspended + sales | `IsSuspend` AND key in active set |
| Top Omzet | Group Faktur by customer key; top 10 by `SUM(GrandTotal)` |
| Top Piutang | Group piutang by customer key; top 10 by open balance |
| Attention list | Emit row for each (customer, signal) pair; sort by signal priority then name |
| Segmentation | Group master customers by Klasifikasi/Wilayah; count active/dormant per group |
| Activity summary | Two `SegmentType = Activity` rows: Active, Dormant |

**Attention list `Value` column:**

| Signal | ValueAmount | ValueText |
| --- | --- | --- |
| Overdue | Total overdue balance | — |
| Dormant | — | `"{N} days since last purchase"` |
| PlafondBreach | Open balance − Plafond | — |
| SuspendedWithSales | Current-month omzet | — |

**Report route hint** (API response only — not stored in DB):

| Signal | `ReportRoute` |
| --- | --- |
| Overdue, PlafondBreach | `/reports/piutang` |
| Dormant, SuspendedWithSales | `/reports/sales` |

### 6.5 `RefreshDashboardCustomerSnapshotWorker`

Mirror `RefreshDashboardPiutangSnapshotWorker` pattern:

1. Insert refresh log with `Domain = "Customer"`.
2. Load all inputs via injected DALs.
3. Call `_aggregator.Aggregate(...)`.
4. `_snapshotDal.ReplaceCurrent(aggregate, refreshLogId)` in transaction.
5. Mark success/failure on refresh log.

**Period for Faktur load:** same `CurrentMonthPeriode(today)` helper as Sales worker.

### 6.6 `DashboardCustomerSnapshotDal.ReplaceCurrent`

Delete all `SnapshotKey = 'CURRENT'` rows from five Customer tables; insert new rows; update KPI `LastRefreshLogId`. Follow `DashboardPiutangSnapshotDal` transaction pattern.

Counter prefixes for row IDs: use existing `PD*` or new prefix consistent with project (e.g. `PDC` for customer snapshot child rows).

### 6.7 API contract — `GET /api/dashboard/customers`

**Auth:** JWT required.

**Response:** `ApiResponse<DashboardCustomerResponse>`

```csharp
public class DashboardCustomerResponse
{
    public bool IsAvailable { get; set; }
    public bool IsDataFresh { get; set; }
    public DateTime? GeneratedAt { get; set; }
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }

    public DashboardCustomerAttentionCards AttentionCards { get; set; }
    public IList<DashboardCustomerAttentionItem> AttentionList { get; set; }
    public DashboardCustomerRankings Rankings { get; set; }
    public DashboardCustomerSegmentationSummary Segmentation { get; set; }
    public DashboardCustomerNavigationLinks Navigation { get; set; }
}

public class DashboardCustomerAttentionCards
{
    // Collection group
    public int OverdueCustomerCount { get; set; }
    public decimal AgingOver90Amount { get; set; }
    public bool CollectionRequiresAttention { get; set; }

    // Concentration group
    public decimal? TopOmzetCustomerPercent { get; set; }
    public decimal? TopPiutangCustomerPercent { get; set; }

    // Activity group
    public int ActiveCustomerCount { get; set; }

    // Inactivity group
    public int DormantCustomerCount { get; set; }
    public bool InactivityRequiresAttention { get; set; }

    // Credit group
    public int PlafondBreachCount { get; set; }
    public int SuspendedWithSalesCount { get; set; }
    public bool CreditRequiresAttention { get; set; }
}

public class DashboardCustomerAttentionItem
{
    public string CustomerCode { get; set; }
    public string CustomerName { get; set; }
    public string SignalKey { get; set; }
    public string SignalLabel { get; set; }
    public decimal? ValueAmount { get; set; }
    public string ValueText { get; set; }
    public string WilayahName { get; set; }
    public string ReportRoute { get; set; }       // /reports/sales | /reports/piutang
    public bool RequiresAttention { get; set; }   // always true for list rows
}

public class DashboardCustomerRankingRow
{
    public int Rank { get; set; }
    public string CustomerCode { get; set; }
    public string CustomerName { get; set; }
    public decimal Amount { get; set; }
    public decimal? PercentOfTotal { get; set; }
    public string ReportRoute { get; set; }
}

public class DashboardCustomerRankings
{
    public IList<DashboardCustomerRankingRow> TopOmzet { get; set; }
    public IList<DashboardCustomerRankingRow> TopPiutang { get; set; }
}

public class DashboardCustomerSegmentRow
{
    public string SegmentType { get; set; }
    public string SegmentLabel { get; set; }
    public int CustomerCount { get; set; }
    public int ActiveCount { get; set; }
    public int DormantCount { get; set; }
}

public class DashboardCustomerSegmentationSummary
{
    public IList<DashboardCustomerSegmentRow> ByKlasifikasi { get; set; }
    public IList<DashboardCustomerSegmentRow> ByWilayah { get; set; }
    public DashboardCustomerSegmentRow ActiveSummary { get; set; }
    public DashboardCustomerSegmentRow DormantSummary { get; set; }
}

public class DashboardCustomerNavigationLinks
{
    public string SalesDashboardRoute { get; set; }    // /dashboard/sales
    public string PiutangDashboardRoute { get; set; }  // /dashboard/piutang
    public string SalesReportRoute { get; set; }
    public string PiutangReportRoute { get; set; }
}
```

**RequiresAttention derivation (presentation):**

| Card group | `RequiresAttention = true` when |
| --- | --- |
| Collection | `OverdueCustomerCount > 0` OR `AgingOver90Amount > 0` |
| Inactivity | `DormantCustomerCount > 0` |
| Credit | `PlafondBreachCount > 0` OR `SuspendedWithSalesCount > 0` |
| Concentration | Always false (informational — Q16) |

**IsDataFresh:** `(UtcNow − GeneratedAt).TotalMinutes <= CustomerIntervalMinutes`

### 6.8 Configuration and infrastructure wiring

**`DashboardSnapshotOptions`:**

```csharp
public int CustomerIntervalMinutes { get; set; } = 30;
```

**Register in:**

- `ApplicationPortalExtensions.cs` — aggregator, worker, MediatR handler
- `InfrastructurePortalExtensions.cs` — snapshot DAL, read DAL, `CustomerLastFakturDal`
- `WorkerDependencyConfig` — same registrations for worker host

**Update domain lists in:**

- `HealthController.BuildDomainStatuses` — add `"Customer"`
- `RefreshDashboardSnapshotsHandler` — add `"Customer"` case
- `RefreshAllDashboardSnapshotsWorker` — run Customer worker last
- `btr.portal.worker/Program.cs` — `ValidDomains` includes `Customer`

**Task Scheduler:** Add fifth job `BTR Portal Dashboard Customer Refresh` every 30 minutes (document in operational guide; script mirrors existing domain jobs).

---

## 7. Frontend Implementation

### 7.1 Route and sidebar

| Item | Change |
| --- | --- |
| Route | `path: 'dashboard/customers'`, name: `customers-dashboard` |
| Component | `CustomerDashboardView.vue` |
| `MainLayout.vue` | Add **Customers** menu item after Piutang, before Inventory |
| Active class | `route.path === '/dashboard/customers'` |

### 7.2 Store and API

- `fetchDashboardCustomer()` in `dashboardApi.ts`
- `customer` ref + `loadCustomer()` in `dashboardStore.ts`
- TypeScript interfaces mirroring `DashboardCustomerResponse`

### 7.3 Page layout — Proposal A

```text
┌─────────────────────────────────────────────────────────────┐
│ Customer Analytics                   Last Refreshed: …  [↻] │
│ Which customers require management attention?                  │
├─────────────────────────────────────────────────────────────┤
│ [⚠ Dashboard Data Not Fresh]  (when !IsDataFresh)            │
├─────────────────────────────────────────────────────────────┤
│ 1. ATTENTION CARDS (5 groups)                                │
│ Collection | Concentration | Activity | Inactivity | Credit   │
├─────────────────────────────────────────────────────────────┤
│ 2. ATTENTION LIST (DataTable)                                │
│ Code | Customer | Signal | Value | Wilayah | [→ Report]       │
├─────────────────────────────────────────────────────────────┤
│ 3. TOP RANKINGS (side-by-side or stacked)                    │
│ Top 10 Omzet (month) | Top 10 Piutang (all open)             │
├─────────────────────────────────────────────────────────────┤
│ 4. SEGMENTATION                                              │
│ By Klasifikasi | By Wilayah | Active vs Dormant              │
├─────────────────────────────────────────────────────────────┤
│ 5. NAVIGATION                                                │
│ → Sales Dashboard / Sales Report                             │
│ → Piutang Dashboard / Piutang Report                         │
└─────────────────────────────────────────────────────────────┘
```

### 7.4 Components

| Component | Purpose |
| --- | --- |
| `DashboardDetailLayout.vue` | Page shell (reuse) |
| `KpiCard.vue` | Attention card groups (reuse) |
| `Top10RankingTable.vue` | Rankings with `%` column extension (reuse; add optional percent column via props or sibling column) |
| `CustomerAttentionCardGroup.vue` | **New** — wraps 1–2 KPI cards with Attention Indicator border |
| `CustomerAttentionList.vue` | **New** — attention list DataTable with report link action |
| `CustomerSegmentationSection.vue` | **New** — klasifikasi/wilayah tables + active/dormant summary |
| `CustomerNavigationSection.vue` | **New** — links to domain dashboards and reports |

### 7.5 Attention card content

| Card group | Fields |
| --- | --- |
| **Collection** | Overdue Customer (count) · >90 Day Exposure (amount) |
| **Concentration** | Top Omzet Customer % · Top Piutang Customer % |
| **Activity** | Active Customers (month count) |
| **Inactivity** | Dormant Customers (90-day rule count) |
| **Credit** | Plafond Breach (count) · Suspended + Sales (count) |

Apply **Attention Indicator** styling when respective `*RequiresAttention` is true. Concentration card has no indicator threshold coloring on percentages (Q16).

Collection card links → `/dashboard/piutang`. Activity/Inactivity → self or Sales dashboard. Credit → self (attention list).

### 7.6 Row click and pre-filter

**Helper** `navigateToReport(route: string, customerName: string)`:

```typescript
router.push({ path: route, query: { q: customerName } })
```

**Report views** — on mount, after store init:

```typescript
const route = useRoute()
if (typeof route.query.q === 'string' && route.query.q.trim()) {
  salesReport.freeText = route.query.q.trim()
}
```

Apply same pattern to `SalesReportView.vue` and `PiutangReportView.vue`.

Pre-filter uses **CustomerName** (matches existing free-text search fields). CustomerCode remains on dashboard rows for display and future use.

### 7.7 Rankings table columns

| Top Omzet | Top Piutang |
| --- | --- |
| Rank | Rank |
| CustomerCode | CustomerCode |
| CustomerName | CustomerName |
| OmzetAmount + % of total | OutstandingBalance + % of total |

Row click → respective report with pre-filter.

---

## 8. Testing

### 8.1 Unit tests — `DashboardCustomerAggregatorTest`

| Case | Assertion |
| --- | --- |
| Top 10 Omzet ranking | Correct order by GrandTotal; tie-break by name |
| Top 10 Piutang ranking | Matches piutang aggregator grouping logic |
| Customer key fallback | Code preferred; name used when code empty |
| Dormant — 91 days, has history | Included in dormant set |
| Dormant — 89 days | Not dormant |
| Dormant — no history | Not dormant (never purchased) |
| Dormant — active this month | Not dormant (in active set) |
| Overdue signal | Customer with past-due row appears in attention list |
| Plafond breach | Open balance 150, Plafond 100 → breach |
| Plafond zero | Plafond 0, balance > 0 → no breach (rule: `Plafond > 0`) |
| Suspended + sales | IsSuspend + current Faktur → signal |
| Suspended, no sales | No signal |
| Concentration % | Top omzet 400 / total 1000 → 40% |
| Segmentation Unknown | Blank Klasifikasi → `"Unknown"` group |
| Attention list multi-signal | Customer overdue AND plafond breach → two rows |

### 8.2 Unit tests — `DashboardCustomerKeyResolverTest`

Verify parity with existing piutang/sales aggregator key resolution.

### 8.3 Integration / manual test checklist

1. Deploy SQL tables; run Customer refresh worker; verify five tables populated.
2. `GET /api/dashboard/customers` returns complete response with `GeneratedAt`.
3. Navigate to `/dashboard/customers` — all five sections render in order.
4. Sidebar shows **Customers** between Piutang and Inventory.
5. Overdue count matches Piutang Dashboard overdue count.
6. Top 10 Piutang balances match Piutang Dashboard Top 10 (allowing CustomerCode precision).
7. Top 10 Omzet reconciles with Sales Report customer totals for current month.
8. Dormant count plausible — spot-check one customer via Sales Report last Faktur date.
9. Click attention list row → report opens with search pre-filled; filtered rows shown.
10. Click Top Omzet row → Sales Report pre-filtered; Top Piutang row → Piutang Report.
11. **"⚠ Dashboard Data Not Fresh"** when snapshot exceeds 30-minute interval.
12. Executive dashboard Top 5 Customers **unchanged**.
13. `GET /api/health/dashboard-snapshots` includes Customer domain.
14. Manual refresh: `POST /api/admin/dashboard/refresh` with `{ "Domain": "Customer" }` succeeds.
15. Worker CLI: `btr.portal.worker --domain Customer --triggered-by Manual` succeeds.

---

## 9. Risk Analysis

| Risk | Likelihood | Impact | Mitigation |
| --- | --- | --- | --- |
| Dormant scan performance on large Faktur history | Medium | Medium | Use aggregated SQL in `CustomerLastFakturDal`; index on `BTR_Faktur(FakturDate, VoidDate, CustomerId)` if slow — measure in staging |
| Customer key mismatch across sources | Medium | Medium | Single `DashboardCustomerKeyResolver`; unit tests; prefer Code-first per Q30 |
| Piutang report vs dashboard total mismatch on drill-down | Medium | Low | UI hint: dashboard = all-open; report defaults to current month — same as Piutang dashboard today |
| Plafond = 0 edge cases | Low | Low | Implement PO rule literally: breach only when balance > Plafond and Plafond > 0 |
| Duplicate attention signals inflate list length | Low | Low | Expected behavior — one row per signal; document in operational guide |
| Customer refresh fails mid-RefreshAll | Low | Medium | Transactional ReplaceCurrent; refresh log marks Failed; prior CURRENT row deleted only on success path — verify delete+insert is atomic |
| Name-based pre-filter ambiguity | Low | Low | Use exact customer name from snapshot; free-text filter is substring match — acceptable for M17 |

---

## 10. Documentation Updates (post-delivery)

| Document | Update |
| --- | --- |
| `docs/features/btr-portal/btr-portal-operational.md` | Customer Analytics route, attention signals, dormant rule, drill-down path, refresh cadence |
| `docs/features/btr-portal/btr-portal-domain.md` | Customer snapshot domain KPI definitions and semantics |
| `docs/features/btr-portal/btr-portal-architecture.md` | `DashboardCustomerAgg`, tables, worker, API endpoint |
| `docs/features/materialized-dashboard/materialized-dashboard-domain.md` | Customer domain refresh rules |
| `docs/work/btr-portal/M17-Customer-Analytics-Analysis.md` | Link to this plan as implemented |

---

## 11. Implementation Steps

Execute in order. Each phase should compile before proceeding.

### Phase 1 — Database

1. Create five `BTRPD_Customer*.sql` table scripts.
2. Add to `btr.sql.sqlproj`; deploy to dev database.

### Phase 2 — Backend core

3. Add `ICustomerLastFakturDal` + `CustomerLastFakturDal`.
4. Add `DashboardCustomerKeyResolver`.
5. Add aggregate models and `DashboardCustomerAggregator` with unit tests (Section 8.1).
6. Add `IDashboardCustomerSnapshotDal` + `DashboardCustomerSnapshotDal.ReplaceCurrent`.
7. Add `RefreshDashboardCustomerSnapshotWorker` + request/result types.
8. Add `CustomerIntervalMinutes` to `DashboardSnapshotOptions`.

### Phase 3 — Backend integration

9. Register all new services in Application and Infrastructure DI.
10. Update `RefreshAllDashboardSnapshotsWorker`, `RefreshDashboardSnapshotsHandler`, `HealthController`, worker `Program.cs`.
11. Add `DashboardCustomerAgg` read path: query, handler, `DashboardCustomerDal`, `CustomerDashboardController`.
12. Verify `GET /api/dashboard/customers` against populated snapshot.
13. Run manual Customer refresh via admin API and worker CLI.

### Phase 4 — Frontend

14. Add TypeScript models and `fetchDashboardCustomer`.
15. Extend `dashboardStore` with `loadCustomer`.
16. Add route and sidebar **Customers** item.
17. Create section components (Section 7.4).
18. Implement `CustomerDashboardView.vue` per Section 7.3 layout.
19. Add `?q=` pre-filter handling to Sales and Piutang report views.
20. Wire row clicks and navigation links.

### Phase 5 — Verification and docs

21. Run unit tests and manual checklist (Section 8.3).
22. Add Task Scheduler job for Customer domain (operations).
23. Update feature documentation (Section 10).

---

## 12. Acceptance Criteria

M17 is complete when:

1. `/dashboard/customers` displays **Customer Analytics** (Proposal A layout) for all authenticated users.
2. Sidebar shows **Dashboard → Customers** at `/dashboard/customers`.
3. Dedicated `BTRPD_Customer*` tables exist and are populated by `RefreshDashboardCustomerSnapshotWorker`.
4. `GET /api/dashboard/customers` returns attention cards, attention list, Top 10 Omzet, Top 10 Piutang, and segmentation per Section 11.10 of analysis.
5. All customer rows include **CustomerCode**; rankings show **% of domain total**.
6. Attention list includes only approved signals: Overdue · Dormant (90-day) · Plafond breach · Suspended with active sales.
7. Dormant rule requires prior Faktur history — customers never invoiced are excluded.
8. Sales metrics use **current calendar month Faktur GrandTotal**; Piutang metrics use **all-time open balance**.
9. Customer row click opens correct report with **customer name pre-filter** applied.
10. Navigation section links to Sales/Piutang dashboards and reports; path Customer → Domain Dashboard → Report is achievable.
11. **Attention Indicator** presentation matches M16; concentration % has no threshold coloring.
12. **"⚠ Dashboard Data Not Fresh"** banner when Customer snapshot is stale.
13. Executive dashboard **unchanged** — Top 5 Customers still present.
14. Sales/Piutang domain dashboards and reports behave as before (except report pre-filter enhancement).
15. Aggregator unit tests pass; health endpoint lists Customer domain.

---

## 13. Handoff Notes for Implementer

- **Section 2 is authoritative** — do not add Retur, Effective Call, GPS, collection effectiveness, trend charts, or executive changes.
- **Read source DALs in Customer worker** — do not compose from Sales/Piutang snapshot tables (CustomerCode gap, dormant/plafond logic).
- **Reuse aging bucket definitions** from `DashboardPiutangAggregator` — import or duplicate constants; verify `DaysOver90` key matches exactly.
- **Do not modify** `BTRPD_PiutangTopCustomer` or `DashboardPiutangAggregator` for M17.
- **Plafond breach:** `Plafond > 0 AND openBalance > Plafond` — do not flag when Plafond is zero unless PO revises.
- **Dormant:** `(today - LastFakturDate).Days >= 90` AND customer appears in `ICustomerLastFakturDal` results (implies history). Customers active this month are **not** dormant.
- **Attention list sorting suggestion:** Overdue → PlafondBreach → SuspendedWithSales → Dormant; then by CustomerName.
- **ReplaceCurrent** must be transactional across all five Customer tables.
- **Pre-filter** uses `CustomerName` in `?q=` query param — report search fields already include `CustomerName`.
- Optional small enhancement: extend `Top10RankingTable.vue` to show percent column when provided — keep change minimal.
- Future milestones M18–M20 are **not** part of this delivery.
