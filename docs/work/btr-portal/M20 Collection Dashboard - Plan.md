# Implementation Plan: M20 — Collection Dashboard

## Document Status

| Field | Value |
| --- | --- |
| Initiative | M20 Collection Dashboard — **Are receivables being converted into cash, and which receivables require collection attention?** |
| Authoritative requirements | `docs/work/btr-portal/M20-collection-dashboard-analysist.md` — **Section 13 (Final Product Decisions)** |
| Related features | `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-operational.md`, `docs/features/btr-portal/btr-portal-architecture.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md` |
| Reference pattern | M17 Customer Analytics + M18 Salesman Performance snapshot domains; FF2 `PenerimaanPelunasanSalesDal` recovery semantics |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |
| Status | **Ready for implementation** |
| Product decisions | **Confirmed** — analysis Section 13, 2026-06-08 |

---

## 1. Goal

Deliver **Collection Dashboard** at `/dashboard/collection` — a dedicated collection-management view to answer *Are receivables being converted into cash, and which receivables require collection attention?*

**Primary outcomes:**

- New route `/dashboard/collection` with page title **Collection Dashboard** for all authenticated users (no role-based routing).
- Sidebar label **Collection** (after Salesmen, before Inventory).
- **Dedicated Collection snapshot domain** (`BTR_PortalDashboardCollection*`) with its own refresh worker — materialized KPIs, not live composition.
- **Attention-First layout with Recovery Summary** (fixed section order per analysis Section 11.2).
- Mandatory recovery KPIs at **company level only**: Cash Collected MTD · Recovery vs Billing % · Payment Mix (Cash / Giro / Adjustment).
- **Overdue-only** rankings: Top 10 Overdue Customers · Top 10 Overdue Salesmen · Top 10 Overdue Wilayah (rank by overdue balance, not total balance).
- **Attention List** at Customer × Signal, Salesman × Signal, and Wilayah × Signal grain with approved signals.
- Drill-down to **Piutang Report only** (`?q=` pre-filter for customer and salesman rows).
- **Supplements** Piutang (M14), Customer (M17), and Salesman (M18) dashboards — does **not** duplicate Total Piutang headline or full Current+overdue aging pie.

**Phase 2 (post-M20 dashboard delivery — PO Q25):**

- Extend Executive Dashboard with **Cash Collected MTD**, **Recovery vs Billing %**, and **Overdue Concentration %** sourced from Collection snapshot (not live FF2).

**Explicitly out of scope (PO confirmed):**

- Collection CRM (follow-up, promise-to-pay, visit outcomes).
- DSO, aging deterioration trends, historical recovery charts.
- Portal Pelunasan / Collections report (Q10).
- Tagihan pipeline as headline KPIs (optional supporting only — **not implemented in M20 v1**).
- Salesman sales-performance metrics (omzet rankings, targets).
- Route, check-in, effective call (M25).
- Retur analytics as standalone dimension.
- Event-driven refresh after pelunasan (Q24).
- Changes to existing Piutang / Customer / Salesman snapshot tables or domain dashboard APIs.

---

## 2. Authoritative Product Decisions

Source: analysis Section 13. Do not re-decide these rules during implementation.

### 2.1 Positioning and audience

| Decision | Value |
| --- | --- |
| Route | `/dashboard/collection` |
| Page title | **Collection Dashboard** |
| Sidebar label | **Collection** |
| Audience | All authenticated users — no RBAC in M20 |
| Executive relationship | **Phase 2 only** — promote recovery KPIs after M20 dashboard ships (Q25) |
| Piutang Dashboard relationship | **Complements** `/dashboard/piutang` — no Total Piutang headline duplication (Q3) |

### 2.2 Period and exposure semantics

| Metric family | Period | Filter |
| --- | --- | --- |
| Recovery KPIs (Cash Collected MTD, Recovery vs Billing %, Payment Mix) | **Current calendar month** | `LunasDate` / FakturDate in month |
| Exposure KPIs (overdue, aging, rankings, legacy debt) | **All-time open balance** | `KurangBayar > 1` (same as M14/M17/M18) |
| Aging anchor | **JatuhTempo** vs refresh `today` | Reuse `DashboardPiutangAggregator.ResolveAgingBucketKey` boundaries |
| Overdue row amount | Full `KurangBayar` on rows where bucket ≠ `Current` | Not proportional split across buckets |

### 2.3 Recovery formulas (architect-resolved)

Analysis Section 13.2 left adjustment handling to the architect. This plan resolves it to align with Desktop FF2.

| Term | Definition | Source |
| --- | --- | --- |
| **Month Collections** | `SUM(BayarTunai + BayarGiro)` = FF2 **`TotalBayar`** | `IPenerimaanPelunasanSalesDal` for current month |
| **Cash Collected MTD** | `SUM(BayarTunai)` only | Same DAL — literal cash leg |
| **Month Faktur Omzet** | `SUM(GrandTotal)` non-void Faktur in current month | `IFakturViewDal` — same as Sales Dashboard Total Omzet |
| **Recovery vs Billing %** | `Month Collections ÷ Month Faktur Omzet × 100` | Null when omzet = 0 |
| **Payment Mix — Cash** | `BayarTunai ÷ SettlementTotal × 100` | Company month |
| **Payment Mix — Giro** | `BayarGiro ÷ SettlementTotal × 100` | Company month |
| **Payment Mix — Adjustment** | `(Retur + Potongan + MateraiAdmin) ÷ SettlementTotal × 100` | Company month |
| **SettlementTotal** | `BayarTunai + BayarGiro + Retur + Potongan + MateraiAdmin` | FF2 component sum |

**Note:** `UangMuka` (`JenisLunas = 2`) is excluded from FF2 `TotalBayar` today — do **not** add it in M20; preserve Desktop parity.

### 2.4 Exposure and concentration KPIs (architect-resolved)

| KPI | Formula |
| --- | --- |
| **Overdue Exposure** | `SUM(KurangBayar)` where aging bucket ≠ `Current` |
| **>90d Exposure** | `SUM(KurangBayar)` in `DaysOver90` bucket (M14 definition) |
| **Overdue Concentration %** | Top-1 customer **overdue balance** ÷ total company **overdue** × 100 |
| **Legacy Debt Count** | Distinct customers: M17 dormant rule **and** open balance `> 1` |
| **Aging Risk Summary** | Four buckets only: `Days1To30`, `Days31To60`, `Days61To90`, `DaysOver90` — sums of **overdue** rows per bucket (exclude `Current`) |

### 2.5 Attention signals (mandatory)

| Signal | Entity | Inclusion rule |
| --- | --- | --- |
| **ChronicOverdue** | Customer | Customer has open balance in `DaysOver90` bucket (`> 0`) |
| **LegacyDebt** | Customer | M17 dormant (90-day rule, prior history, not active this month) **and** open balance `> 1` |
| **PlafondBreachOverdue** | Customer | M17 plafond breach (`balance > Plafond` when `Plafond > 0`) **and** overdue balance `> 0` |
| **Overdue** | Customer | Reuse M17 — any overdue balance (bucket ≠ `Current`) |
| **HighOverdueWorkload** | Salesman | Reuse M18 `HighOverdueExposure` rule — any overdue on rep's invoiced open Faktur book |
| **LowRecoveryVsBilling** | Salesman | Current-month Faktur omzet `> 0` **and** rep month `TotalBayar <` rep month omzet |
| **WilayahHotspot** | Wilayah | Wilayah overdue balance ÷ total company overdue × 100 **≥ 15%** |

**Presentation rules:**

- One row per **entity × signal** (same pattern as M17/M18).
- Reuse M17/M18 signal **logic**; M20 uses collection-framed labels where PO specifies (`HighOverdueWorkload` not `HighOverdueExposure`).
- Do **not** emit duplicate rows for the same entity when M17/M20 rules overlap (e.g. customer with `DaysOver90` gets **ChronicOverdue** only — suppress generic **Overdue** for that customer).
- Generic **Attention Indicator** (M16/M17) — no per-signal severity engine.

**Signal priority (suppress duplicates):** `ChronicOverdue` > `PlafondBreachOverdue` > `LegacyDebt` > `Overdue` for customers; one WilayahHotspot row per qualifying wilayah.

### 2.6 Rankings

| Ranking | Rule |
| --- | --- |
| Top 10 Overdue Customers | Group open rows by customer key; sum **overdue-only** balance; exclude zero |
| Top 10 Overdue Salesmen | Group by invoicing `SalesPersonId`; sum overdue balance |
| Top 10 Overdue Wilayah | Group by `Customer.WilayahId`; sum overdue balance |
| Top N | **10** for all three rankings |
| Denominator % | Row overdue ÷ total company overdue × 100 |

### 2.7 Ownership and attribution

| Dimension | Attribution |
| --- | --- |
| Open overdue exposure | Invoicing salesman on Faktur (`SalesPersonId`) |
| Collections received | Invoicing salesman on paid Faktur (FF2 join pattern) |
| Wilayah | **`Customer.WilayahId`** — not salesman territory |
| Collector entity | **Do not invent** — none exists in BTR |

### 2.8 Layout and drill-down

**Fixed section order (Q4):**

1. Collection Attention Cards  
2. Recovery Summary  
3. Aging Risk Summary  
4. Collection Attention List  
5. Top Overdue Customers  
6. Top Overdue Salesmen  
7. Top Overdue Wilayah  
8. Navigation  

| UI element | Target |
| --- | --- |
| Customer attention / ranking row | `/reports/piutang?q={CustomerName}` |
| Salesman attention / ranking row | `/reports/piutang?q={SalesName}` |
| Wilayah ranking row | **No row drill-down in M20** — Piutang Report has no wilayah filter (analysis Section 11.3) |
| Recovery KPIs | Dashboard only — no FF2/FF4 portal report |
| Navigation links | Piutang Dashboard · Customer Analytics · Salesman Performance · Piutang Report |

### 2.9 Data architecture

| Decision | Value |
| --- | --- |
| Snapshot domain | `BTR_PortalDashboardCollection*` |
| Refresh cadence | **30 minutes** (`CollectionIntervalMinutes`) |
| Refresh pattern | `SnapshotKey = 'CURRENT'` delete-and-replace |
| Read API | `GET /api/dashboard/collection` |
| Materialization | All derivations at refresh — **no read-time composition** |

---

## 3. Architecture Overview

### 3.1 Target topology

```text
Source DALs (read at refresh time — NOT from other snapshot tables)
  IPiutangOpenBalanceDal                    → all-time open balances (exposure, customer overdue)
  IPiutangOpenBalanceWithSalesmanDal        → open balances + invoicing salesman            [EXISTING]
  IPiutangOpenBalanceWithWilayahDal         → open balances + Customer.WilayahId/Name       [NEW]
  IPenerimaanPelunasanSalesDal              → month collections by day/salesman (FF2)       [EXTEND SalesPersonId]
  IFakturViewDal                            → current-month Faktur omzet by SalesPersonId   [EXISTING]
  ICustomerLastFakturDal                    → dormant / legacy debt rule                    [EXISTING]
  ICustomerDal / List customers             → Plafond, Wilayah master                       [EXISTING]
  ISalesPersonDal                           → SalesPersonCode for attention/ranking rows    [EXISTING]
    ↓
RefreshDashboardCollectionSnapshotWorker
    ↓ DashboardCollectionAggregator
BTR_PortalDashboardCollection* tables (6)
    ↓
Browser → GET /api/dashboard/collection
    ↓ MediatR
GetDashboardCollectionHandler
    ↓ IDashboardCollectionDal
DashboardCollectionDal → DashboardCollectionResponse

Existing unchanged:
  GET /api/dashboard/executive | piutang | customers | salesmen
  BTR_PortalDashboardPiutang* / Customer* / Salesman*
  GET /api/reports/piutang
  BTR Desktop FF1/FF2/FF4
```

**Why read source DALs, not Piutang/Customer/Salesman snapshots:**

- Recovery KPIs require **`PenerimaanPelunasanSalesDal`** — not materialized anywhere in portal today.
- Rankings require **overdue-only** sums — Piutang snapshot Top 10 is by **total** balance.
- Wilayah aggregates are not in any existing snapshot.
- Keeps Collection domain self-contained; avoids coupling refresh order to other domains.

### 3.2 Design principles

| Principle | Application |
| --- | --- |
| Reuse business rules | Copy aging, dormant, plafond, customer key rules from `DashboardPiutangAggregator`, `DashboardCustomerAggregator`, `DashboardSalesmanAggregator` |
| Dedicated snapshot | Collection KPIs in own tables (Q21) |
| FF2 parity | Month collections and payment mix match Desktop FF2 component semantics |
| Differentiate from M14 | No Total Piutang headline; overdue-only aging summary (4 buckets) |
| Fail gracefully | Empty/missing snapshot → unavailable sections with clear UI message |
| Key by internal ids | `SalesPersonId`, `WilayahId`, customer code-first key |

### 3.3 Architecture decisions

| Decision | Choice | Rationale |
| --- | --- | --- |
| Snapshot approach | **New Collection domain** with 6 tables | PO Q21; recovery + overdue rankings not representable in existing tables |
| Aging bucket logic | **Copy** `ResolveAgingBucketKey` from `DashboardPiutangAggregator` | PO Q16; single authoritative boundary set |
| Overdue amount | Row `KurangBayar` when bucket ≠ `Current` | Analysis Section 14 |
| Wilayah input | **`IPiutangOpenBalanceWithWilayahDal`** | Customer.WilayahId join — same as FF1 |
| Collections input | **`IPenerimaanPelunasanSalesDal`** + extend `SalesPersonId` on DTO | LowRecoveryVsBilling needs id-keyed match to Faktur omzet |
| Cash vs collections | **Cash Collected MTD = BayarTunai**; Recovery numerator = **TotalBayar** | Resolves PO ambiguity in Section 13.2 / 14 |
| LowRecoveryVsBilling | Rep omzet `> 0` and rep `TotalBayar <` rep omzet | Consistent with company Recovery vs Billing `< 100%` |
| WilayahHotspot threshold | **≥ 15%** of company overdue | Aligns with executive concentration alerting magnitude |
| Overdue Concentration | Top-1 **customer overdue** / total overdue | PO Section 11.4 / 14; executive Phase 2 uses same denominator |
| HighOverdueWorkload | Same SQL/rule as M18 `HighOverdueExposure` | PO Q11 — collection label only |
| Wilayah drill-down | **Display-only rows** in M20 | Piutang Report lacks wilayah filter — no API change |
| Refresh cadence | **30 minutes** | PO Q23 |
| RefreshAll order | … → Customer → Salesman → **Collection** | Collection last; reads source DALs independently |
| Executive changes | **Phase 2 separate PR** | PO Q25 — ship Collection dashboard first |
| Tagihan supporting KPIs | **Omitted in M20 v1** | PO Q17 — not headline; not inexpensive enough to justify scope |

---

## 4. Impact Analysis

### 4.1 Affected modules

| Layer | Module | Change type |
| --- | --- | --- |
| SQL | `btr.sql/Tables/ReportingContext/BTR_PortalDashboardCollection*.sql` (6 tables) | **New** |
| SQL | `btr.sql.sqlproj` | Include new tables |
| Application | `DashboardSnapshotAgg/Services/DashboardCollectionAggregator.cs` | **New** |
| Application | `DashboardSnapshotAgg/Services/DashboardCollectionKeyResolver.cs` | **New** |
| Application | `DashboardSnapshotAgg/Models/DashboardCollectionAggregateResult.cs` | **New** |
| Application | `DashboardSnapshotAgg/Contracts/IDashboardCollectionSnapshotDal.cs` | **New** |
| Application | `DashboardSnapshotAgg/Contracts/IPiutangOpenBalanceWithWilayahDal.cs` | **New** |
| Application | `DashboardSnapshotAgg/UseCases/RefreshDashboardCollectionSnapshotWorker.cs` | **New** |
| Application | `DashboardSnapshotAgg/DashboardSnapshotOptions.cs` | Add `CollectionIntervalMinutes` |
| Application | `DashboardSnapshotAgg/UseCases/RefreshAllDashboardSnapshotsWorker.cs` | Register Collection worker |
| Application | `DashboardSnapshotAgg/Commands/RefreshDashboardSnapshotsCommand.cs` | Add Collection domain |
| Application | `DashboardCollectionAgg/` (query, contracts, DTOs) | **New** read-side |
| Application | `FinanceContext/PiutangAgg/Contracts/IPenerimaanPelunasanSalesDal.cs` | Add `SalesPersonId` to DTO |
| Infrastructure | `DashboardSnapshotAgg/DashboardCollectionSnapshotDal.cs` | **New** |
| Infrastructure | `DashboardSnapshotAgg/PiutangOpenBalanceWithWilayahDal.cs` | **New** |
| Infrastructure | `DashboardCollectionAgg/DashboardCollectionDal.cs` | **New** |
| Infrastructure | `FinanceContext/PiutangAgg/PenerimaanPelunasanSalesDal.cs` | SELECT/GROUP BY `SalesPersonId` |
| API | `Controllers/Dashboard/CollectionDashboardController.cs` | **New** |
| API | `HealthController.cs` | Add Collection domain |
| API | DI registrations | New DALs, worker, aggregator |
| Worker | `btr.portal.worker/Program.cs` | Add `Collection` to `--domain` |
| Frontend | `router/index.ts` | Route `/dashboard/collection` |
| Frontend | `layouts/MainLayout.vue` | Sidebar **Collection** item |
| Frontend | `views/dashboard/CollectionDashboardView.vue` | **New** |
| Frontend | `components/dashboard/Collection*.vue` | **New** section components |
| Frontend | `models/dashboard.ts`, `api/dashboardApi.ts`, `stores/dashboardStore.ts` | Collection types and loader |
| Tests | `btr.test/ReportingContext/` | Aggregator + key resolver unit tests |
| Docs | Post-delivery feature docs | Operational, domain, architecture updates |

### 4.2 Unaffected modules

| Module | Reason |
| --- | --- |
| `DashboardExecutiveComposer`, `GET /api/dashboard/executive` | Phase 2 only (Q25) |
| Existing Piutang / Customer / Salesman snapshot workers and read APIs | Additive Collection domain only |
| Piutang Report DAL and API | No column additions; drill-down uses existing `?q=` |
| BTR Desktop FF1/FF2/FT1 | No changes — portal reads same underlying tables |
| Sales Dashboard | No omzet ranking duplication in M20 |

### 4.3 Metric traceability

| Collection dashboard field | Source at refresh | Validating reference |
| --- | --- | --- |
| Overdue Exposure | Open balance rows, bucket ≠ Current | Piutang Report — filter `JatuhTempo < today`, footer sum |
| >90d Exposure | `DaysOver90` bucket sum | M14 aging bucket / Piutang Report manual bucket |
| Overdue Concentration % | Top customer overdue / total overdue | Piutang Report grouped overdue by customer |
| Cash Collected MTD | Month `BayarTunai` sum | FF2 Desktop — cash column total |
| Recovery vs Billing % | Month `TotalBayar` / month Faktur omzet | FF2 total vs Sales Report month omzet |
| Payment Mix components | FF2 decomposition | FF2 Desktop month totals |
| Legacy Debt Count | Dormant + open balance | M17 dormant rule + Piutang Report customer filter |
| Aging Risk buckets | Overdue rows per bucket | Piutang Report overdue filter + bucket rules |
| Top Overdue Customers | Customer overdue grouping | Piutang Report — **not** M14 Top 10 (total balance) |
| Top Overdue Salesmen | Salesman overdue grouping | FF1 / Piutang Report grouped by Sales, overdue filter |
| Top Overdue Wilayah | Wilayah overdue grouping | FF1 Desktop export grouped by WilayahName, overdue filter |
| ChronicOverdue / PlafondBreachOverdue / LegacyDebt | Customer rules | M17 aggregator equivalent cases |
| HighOverdueWorkload | Salesman overdue book | M18 `HighOverdueExposure` test cases |
| LowRecoveryVsBilling | Rep month TotalBayar vs omzet | FF2 by salesman vs Sales Report by salesman |
| WilayahHotspot | Wilayah overdue share ≥ 15% | FF1 wilayah grouping |

**Reconciliation notes:**

- Month collections must match FF2 Desktop for the same calendar month (allow rounding tolerance).
- Overdue exposure total must equal sum of four Aging Risk Summary buckets.
- Top Overdue Customer #1 amount must equal Overdue Concentration numerator.
- Dashboard open-balance semantics differ from Piutang Report default period filter — show UI hint on drill-down (same as M17/M18).

---

## 5. Database Design

Deploy all tables with `SnapshotKey = 'CURRENT'` delete-and-replace pattern.

### 5.1 `BTR_PortalDashboardCollectionKpi`

Single row per refresh — attention cards + recovery summary source fields.

| Column | Type | Description |
| --- | --- | --- |
| SnapshotKey | VARCHAR(10) PK | `'CURRENT'` |
| GeneratedAt | DATETIME | Refresh timestamp |
| PeriodYear | INT | Recovery period year |
| PeriodMonth | INT | Recovery period month |
| OverdueExposure | DECIMAL(18,2) | Total past-due open balance |
| AgingOver90Exposure | DECIMAL(18,2) | >90d bucket exposure |
| OverdueConcentrationPercent | DECIMAL(9,4) NULL | Top-1 customer overdue / total overdue × 100 |
| CashCollectedMtd | DECIMAL(18,2) | Month BayarTunai |
| MonthCollections | DECIMAL(18,2) | Month TotalBayar (Cash + Giro) |
| MonthFakturOmzet | DECIMAL(18,2) | Month Faktur GrandTotal |
| RecoveryVsBillingPercent | DECIMAL(9,4) NULL | MonthCollections / MonthFakturOmzet × 100 |
| PaymentMixCashAmount | DECIMAL(18,2) | Month BayarTunai |
| PaymentMixGiroAmount | DECIMAL(18,2) | Month BayarGiro |
| PaymentMixAdjustmentAmount | DECIMAL(18,2) | Month Retur + Potongan + MateraiAdmin |
| PaymentMixCashPercent | DECIMAL(9,4) NULL | Share of SettlementTotal |
| PaymentMixGiroPercent | DECIMAL(9,4) NULL | Share of SettlementTotal |
| PaymentMixAdjustmentPercent | DECIMAL(9,4) NULL | Share of SettlementTotal |
| LegacyDebtCount | INT | Dormant customers with open balance |
| ChronicOverdueCount | INT | Customers with >90d balance |
| WilayahHotspotCount | INT | Wilayahs ≥ 15% overdue share |
| LowRecoveryVsBillingCount | INT | Salesmen failing rep recovery rule |
| LastRefreshLogId | VARCHAR(13) | FK to refresh log |

### 5.2 `BTR_PortalDashboardCollectionAging`

Overdue-only aging distribution (four rows per refresh).

| Column | Type | Description |
| --- | --- | --- |
| CollectionAgingId | VARCHAR(13) PK | Generated ID |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| BucketKey | VARCHAR(20) | `Days1To30` \| `Days31To60` \| `Days61To90` \| `DaysOver90` |
| BucketLabel | VARCHAR(30) | Display label |
| Amount | DECIMAL(18,2) | Overdue sum in bucket |
| SortOrder | INT | 1–4 |

Unique: `(SnapshotKey, BucketKey)`

### 5.3 `BTR_PortalDashboardCollectionAttention`

| Column | Type | Description |
| --- | --- | --- |
| CollectionAttentionId | VARCHAR(13) PK | Generated ID |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| EntityType | VARCHAR(20) | `Customer` \| `Salesman` \| `Wilayah` |
| EntityId | VARCHAR(13) | Customer key / SalesPersonId / WilayahId |
| EntityCode | VARCHAR(20) | CustomerCode / SalesPersonCode / blank |
| EntityName | VARCHAR(50) | Display name |
| SignalKey | VARCHAR(30) | See Section 2.5 |
| SignalLabel | VARCHAR(50) | Display label |
| ValueAmount | DECIMAL(18,2) NULL | Overdue balance, omzet, share % basis, etc. |
| ValueText | VARCHAR(100) NULL | e.g. `"Recovery 62% vs billing"`, `"18% of company overdue"` |
| WilayahName | VARCHAR(30) | Customer/salesman context; entity name for Wilayah rows |
| ReportRoute | VARCHAR(100) NULL | `/reports/piutang` for Customer/Salesman; null for Wilayah |
| SortOrder | INT | Stable list ordering |

Index: `(SnapshotKey, SortOrder)`

### 5.4 `BTR_PortalDashboardCollectionTopOverdueCustomer`

| Column | Type | Description |
| --- | --- | --- |
| CollectionTopOverdueCustomerId | VARCHAR(13) PK | |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| Rank | INT | 1–10 |
| CustomerCode | VARCHAR(20) | |
| CustomerName | VARCHAR(50) | |
| OverdueBalance | DECIMAL(18,2) | |
| PercentOfTotal | DECIMAL(9,4) NULL | Row / total company overdue |

Unique: `(SnapshotKey, Rank)`

### 5.5 `BTR_PortalDashboardCollectionTopOverdueSalesman`

| Column | Type | Description |
| --- | --- | --- |
| CollectionTopOverdueSalesmanId | VARCHAR(13) PK | |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| Rank | INT | 1–10 |
| SalesPersonId | VARCHAR(13) | |
| SalesPersonCode | VARCHAR(20) | |
| SalesPersonName | VARCHAR(50) | |
| OverdueBalance | DECIMAL(18,2) | |
| PercentOfTotal | DECIMAL(9,4) NULL | |

Unique: `(SnapshotKey, Rank)`

### 5.6 `BTR_PortalDashboardCollectionTopOverdueWilayah`

| Column | Type | Description |
| --- | --- | --- |
| CollectionTopOverdueWilayahId | VARCHAR(13) PK | |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| Rank | INT | 1–10 |
| WilayahId | VARCHAR(13) | |
| WilayahName | VARCHAR(50) | |
| OverdueBalance | DECIMAL(18,2) | |
| PercentOfTotal | DECIMAL(9,4) NULL | |

Unique: `(SnapshotKey, Rank)`

---

## 6. Backend Implementation

### 6.1 New folder structure

```text
btr.application/ReportingContext/
├── DashboardSnapshotAgg/
│   ├── Services/
│   │   ├── DashboardCollectionAggregator.cs
│   │   └── DashboardCollectionKeyResolver.cs
│   ├── Models/
│   │   └── DashboardCollectionAggregateResult.cs (+ nested row types)
│   ├── Contracts/
│   │   ├── IDashboardCollectionSnapshotDal.cs
│   │   └── IPiutangOpenBalanceWithWilayahDal.cs
│   └── UseCases/
│       ├── RefreshDashboardCollectionSnapshotWorker.cs
│       └── RefreshDashboardCollectionSnapshotRequest.cs (+ Result)
└── DashboardCollectionAgg/
    ├── Contracts/
    │   └── IDashboardCollectionDal.cs
    └── Queries/
        └── GetDashboardCollectionQuery.cs

btr.infrastructure/ReportingContext/
├── DashboardSnapshotAgg/
│   ├── DashboardCollectionSnapshotDal.cs
│   └── PiutangOpenBalanceWithWilayahDal.cs
└── DashboardCollectionAgg/
    └── DashboardCollectionDal.cs

btr.portal.api/Controllers/Dashboard/
└── CollectionDashboardController.cs
```

Add all new `.cs` files to respective `.csproj` Compile includes.

### 6.2 Source DAL extensions

#### 6.2.1 `IPiutangOpenBalanceWithWilayahDal` + implementation

Mirror `PiutangOpenBalanceWithSalesmanDal` with additional joins:

```sql
SELECT
    ISNULL(w.WilayahId, '') AS WilayahId,
    ISNULL(w.WilayahName, '') AS WilayahName,
    ISNULL(c.CustomerCode, '') AS CustomerCode,
    ISNULL(c.CustomerName, '') AS CustomerName,
    ISNULL(p.DueDate, '3000-01-01') AS JatuhTempo,
    p.Sisa AS KurangBayar
FROM BTR_Piutang p
    LEFT JOIN BTR_Faktur f ON p.PiutangId = f.FakturId
    LEFT JOIN BTR_Customer c ON f.CustomerId = c.CustomerId
    LEFT JOIN BTR_Wilayah w ON c.WilayahId = w.WilayahId
WHERE p.Sisa > 1
```

Blank `WilayahId` → bucket as `"Unknown"` in rankings (display only).

#### 6.2.2 `PenerimaanPelunasanSalesDto` + `PenerimaanPelunasanSalesDal`

Add `SalesPersonId` to DTO. Extend SQL:

- Include `ISNULL(cc.SalesPersonId, '') AS SalesPersonId` in SELECT.
- Add `cc.SalesPersonId` to `GROUP BY` (alongside date and name).

Preserve existing FF2 column semantics for Desktop callers.

### 6.3 `RefreshDashboardCollectionSnapshotWorker`

Load at refresh:

| Input | Method | Purpose |
| --- | --- | --- |
| Open balances | `_openBalanceDal.ListOpenBalances()` | Customer exposure, overdue sums |
| Open + salesman | `_openBalanceWithSalesmanDal.ListOpenBalances()` | Salesman overdue, HighOverdueWorkload |
| Open + wilayah | `_openBalanceWithWilayahDal.ListOpenBalances()` | Wilayah overdue, WilayahHotspot |
| Month pelunasan | `_pelunasanDal.ListData(currentMonthPeriode)` | Recovery KPIs, LowRecoveryVsBilling |
| Month Faktur | `_fakturViewDal.ListData(currentMonthPeriode)` | Month omzet, rep omzet |
| Last Faktur | `_customerLastFakturDal.List...()` | Legacy debt / dormant |
| Customers | `_customerDal.List...()` | Plafond master |
| Salespersons | `_salesPersonDal.List...()` | Code lookup |

Pass `today = DateTime.Today` (local business date — same as other dashboard workers) and `generatedAt = UtcNow` to aggregator.

Transactional `ReplaceCurrent` via `IDashboardCollectionSnapshotDal`.

### 6.4 `DashboardCollectionAggregator` — core logic

**Exposure pass (open balance rows):**

1. Filter `KurangBayar > 1`.
2. For each row, compute bucket via copied `ResolveAgingBucketKey`.
3. If bucket ≠ `Current`, add `KurangBayar` to: total overdue, bucket aging table, customer overdue map, salesman overdue map (with-salesman rows), wilayah overdue map (with-wilayah rows).
4. If bucket = `DaysOver90`, track for ChronicOverdue.

**Recovery pass (pelunasan rows for current month):**

1. Sum company: `BayarTunai`, `BayarGiro`, `Retur`, `Potongan`, `MateraiAdmin`, `TotalBayar`.
2. Compute Payment Mix percents and Recovery vs Billing %.
3. Group by `SalesPersonId` for rep month collections.

**Omzet pass (Faktur rows for current month):**

1. Sum company `GrandTotal` → `MonthFakturOmzet`.
2. Group by `SalesPersonId` → rep month omzet.

**Customer signals:**

- Reuse helper patterns from `DashboardCustomerAggregator` for dormant, plafond breach, overdue maps.
- Apply signal priority to suppress duplicate Overdue rows.

**Salesman signals:**

- `HighOverdueWorkload`: reps with `OverdueBalance > 0` on invoiced book.
- `LowRecoveryVsBilling`: rep omzet `> 0` and rep `TotalBayar <` rep omzet.

**Wilayah signals:**

- Compute each wilayah's `OverdueBalance / TotalOverdue × 100`.
- Emit `WilayahHotspot` for wilayahs ≥ **15%**.

**Rankings:**

- Build Top 10 lists from overdue maps; tie-break by name asc.
- Compute `PercentOfTotal` against total company overdue.

**Attention list sort order:**

1. Signal priority (Critical collection signals first: LowRecoveryVsBilling, WilayahHotspot, ChronicOverdue, PlafondBreachOverdue, LegacyDebt, HighOverdueWorkload, Overdue).
2. Entity type: Customer → Salesman → Wilayah.
3. ValueAmount desc, then EntityName asc.

### 6.5 Read-side API — `DashboardCollectionResponse`

Shape mirrors M17/M18 sectioned response:

```csharp
public class DashboardCollectionResponse
{
    public DateTime GeneratedAt { get; set; }
    public bool IsDataFresh { get; set; }
    public DashboardCollectionAttentionCards AttentionCards { get; set; }
    public DashboardCollectionRecoverySummary RecoverySummary { get; set; }
    public IList<DashboardCollectionAgingBucket> AgingRiskSummary { get; set; }
    public IList<DashboardCollectionAttentionItem> AttentionList { get; set; }
    public IList<DashboardCollectionRankingRow> TopOverdueCustomers { get; set; }
    public IList<DashboardCollectionRankingRow> TopOverdueSalesmen { get; set; }
    public IList<DashboardCollectionRankingRow> TopOverdueWilayah { get; set; }
    public DashboardCollectionNavigationLinks Navigation { get; set; }
}
```

**Attention card groups and `RequiresAttention`:**

| Card group | Fields | `RequiresAttention` when |
| --- | --- | --- |
| **Exposure** | Overdue Exposure · >90d Exposure · Overdue Concentration % | OverdueExposure > 0 OR AgingOver90Exposure > 0 |
| **Recovery** | Cash Collected MTD · Recovery vs Billing % | RecoveryVsBillingPercent < 100 (when omzet > 0) OR MonthCollections = 0 with MonthFakturOmzet > 0 |
| **Portfolio** | Legacy Debt Count | LegacyDebtCount > 0 |

Apply M16 **Attention Indicator** border when respective group `RequiresAttention` is true.

**IsDataFresh:** `(UtcNow − GeneratedAt).TotalMinutes <= CollectionIntervalMinutes`

### 6.6 Configuration and infrastructure wiring

**`DashboardSnapshotOptions`:**

```csharp
public int CollectionIntervalMinutes { get; set; } = 30;
```

**Register in:**

- `ApplicationPortalExtensions.cs` — aggregator, worker, MediatR handler
- `InfrastructurePortalExtensions.cs` — snapshot DAL, read DAL, wilayah open-balance DAL
- `WorkerDependencyConfig` — same registrations for worker host

**Update domain lists in:**

- `HealthController.BuildDomainStatuses` — add `"Collection"`
- `RefreshDashboardSnapshotsHandler` — add `"Collection"` case
- `RefreshAllDashboardSnapshotsWorker` — run Collection worker **last** (after Salesman)
- `btr.portal.worker/Program.cs` — `ValidDomains` includes `Collection`

**Task Scheduler:** Add job `BTR Portal Dashboard Collection Refresh` every 30 minutes.

---

## 7. Frontend Implementation

### 7.1 Route and sidebar

| Item | Change |
| --- | --- |
| Route | `path: 'dashboard/collection'`, name: `collection-dashboard` |
| Component | `CollectionDashboardView.vue` |
| `MainLayout.vue` | Add **Collection** after **Salesmen**, before **Inventory** |
| Active class | `route.path === '/dashboard/collection'` |

### 7.2 Store and API

- `fetchDashboardCollection()` in `dashboardApi.ts`
- `collection` ref + `loadCollection()` in `dashboardStore.ts`
- TypeScript interfaces mirroring `DashboardCollectionResponse`

### 7.3 Page layout

```text
┌─────────────────────────────────────────────────────────────┐
│ Collection Dashboard                 Last Refreshed: …  [↻] │
│ Are receivables being converted into cash?                   │
│ Subtitle: Current month recovery + open overdue exposure     │
├─────────────────────────────────────────────────────────────┤
│ [⚠ Dashboard Data Not Fresh]  (when !IsDataFresh)            │
├─────────────────────────────────────────────────────────────┤
│ 1. COLLECTION ATTENTION CARDS (3 groups)                     │
│ Exposure | Recovery | Portfolio                              │
├─────────────────────────────────────────────────────────────┤
│ 2. RECOVERY SUMMARY                                          │
│ Cash Collected MTD | Recovery vs Billing % | Payment Mix bar │
├─────────────────────────────────────────────────────────────┤
│ 3. AGING RISK SUMMARY (overdue-only, 4 buckets)              │
│ Reuse M14 aging pie component — exclude Current bucket       │
├─────────────────────────────────────────────────────────────┤
│ 4. COLLECTION ATTENTION LIST                                 │
│ Type | Name | Signal | Detail | Wilayah | [→ Report]        │
├─────────────────────────────────────────────────────────────┤
│ 5. TOP OVERDUE CUSTOMERS                                     │
│ 6. TOP OVERDUE SALESMEN                                      │
│ 7. TOP OVERDUE WILAYAH (no row click)                        │
├─────────────────────────────────────────────────────────────┤
│ 8. NAVIGATION                                                │
│ → Piutang / Customers / Salesmen dashboards + Piutang Report │
└─────────────────────────────────────────────────────────────┘
```

**Do not show Total Piutang headline card** — differentiation from M14 (Q3).

### 7.4 Components

| Component | Purpose |
| --- | --- |
| `DashboardDetailLayout.vue` | Page shell (reuse) |
| `KpiCard.vue` | Attention card groups (reuse) |
| `CollectionAttentionCardGroup.vue` | **New** — Exposure / Recovery / Portfolio groups |
| `CollectionRecoverySummary.vue` | **New** — recovery KPIs + payment mix visualization |
| `CollectionAgingRiskSummary.vue` | **New** — 4-bucket overdue aging (reuse pie/bar from M14 with filtered buckets) |
| `CollectionAttentionList.vue` | **New** — multi-entity attention DataTable |
| `Top10RankingTable.vue` | Rankings (reuse/extend for overdue label) |
| `CollectionNavigationSection.vue` | **New** — domain dashboard links |

### 7.5 Row click and pre-filter

**Helper** `navigateToPiutangReport(name: string)`:

```typescript
router.push({ path: '/reports/piutang', query: { q: name } })
```

- Customer and Salesman rows: call helper with `EntityName`.
- Wilayah rows: **no click handler** — display `PercentOfTotal` only.

Subtitle on page and near rankings: *Dashboard uses all open balances; Piutang Report may default to a period filter.*

### 7.6 Recovery Summary presentation

| Field | Display |
| --- | --- |
| Cash Collected MTD | Currency — tooltip: "Cash payments received this month" |
| Recovery vs Billing % | Percent with M16 achievement-style band optional coloring: ≥100% Healthy · 80–99% Warning · <80% Critical when omzet > 0 |
| Payment Mix | Stacked bar or three segments: Cash · Giro · Adjustment with % labels |

---

## 8. Phase 2 — Executive Dashboard Promotion

Execute **after** M20 Collection dashboard is shipped and validated. Separate PR.

| Executive field | Collection snapshot source |
| --- | --- |
| Cash Collected MTD | `BTR_PortalDashboardCollectionKpi.CashCollectedMtd` |
| Recovery vs Billing % | `BTR_PortalDashboardCollectionKpi.RecoveryVsBillingPercent` |
| Overdue Concentration % | `BTR_PortalDashboardCollectionKpi.OverdueConcentrationPercent` |

**Implementation sketch:**

1. Extend `DashboardExecutiveComposer` to read Collection KPI row (via new thin `ICollectionKpiSnapshotReader` or direct DAL read of single KPI row).
2. Add fields to executive API response and Executive dashboard cards.
3. Do **not** remove existing Piutang exposure cards — Collection KPIs add recovery lens (PO Q25).

---

## 9. Testing

### 9.1 Unit tests — `DashboardCollectionAggregatorTest`

| Case | Assertion |
| --- | --- |
| Overdue exposure total | Sum of non-Current row balances |
| Aging risk buckets | Four buckets sum to OverdueExposure; Current excluded |
| >90d exposure | Matches DaysOver90 bucket amount |
| Overdue concentration | Top customer overdue / total overdue × 100 |
| Cash Collected MTD | Sum BayarTunai only |
| Recovery vs Billing | TotalBayar / month omzet × 100 |
| Payment mix percents | Three percents sum to ~100% when SettlementTotal > 0 |
| Top overdue customers | Ranked by overdue not total balance |
| Top overdue salesmen | By SalesPersonId overdue sum |
| Top overdue wilayah | By WilayahId overdue sum |
| ChronicOverdue | Customer with >90d balance → signal; suppresses Overdue |
| LegacyDebt | Dormant 91d + open balance → signal |
| PlafondBreachOverdue | Plafond breach + overdue → signal |
| HighOverdueWorkload | Rep with past-due row → signal |
| LowRecoveryVsBilling | Rep omzet 100, collections 60 → signal |
| LowRecovery — healthy rep | Collections ≥ omzet → no signal |
| WilayahHotspot | Wilayah at 20% of overdue → signal; 10% → no signal |
| Zero omzet month | RecoveryVsBillingPercent null |
| Blank WilayahId | Grouped as Unknown in rankings |

### 9.2 Unit tests — `DashboardCollectionKeyResolverTest`

Verify customer code-first key; salesman id preference; wilayah id normalization.

### 9.3 Integration / manual test checklist

1. Deploy SQL tables; run Collection refresh worker; verify six tables populated.
2. `GET /api/dashboard/collection` returns complete response with `GeneratedAt`.
3. Navigate to `/dashboard/collection` — all eight sections render in fixed order.
4. Sidebar shows **Collection** after Salesmen, before Inventory.
5. Cash Collected MTD reconciles with FF2 Desktop cash total for current month.
6. Recovery vs Billing % reconciles with FF2 TotalBayar ÷ Sales Dashboard month omzet.
7. Top Overdue Customers **differs** from Piutang Dashboard Top 10 when non-overdue balance exists.
8. Top Overdue Salesmen reconciles with Piutang Report overdue rows grouped by Sales.
9. WilayahHotspot count matches manual FF1 wilayah grouping spot-check.
10. Click customer attention row → Piutang Report with customer `?q=` pre-filled.
11. Click salesman row → Piutang Report with salesman pre-filled.
12. Wilayah ranking row has no navigation.
13. **"⚠ Dashboard Data Not Fresh"** when snapshot exceeds 30-minute interval.
14. Piutang / Customer / Salesman dashboards **unchanged**.
15. Executive dashboard **unchanged** in Phase 1.
16. `GET /api/health/dashboard-snapshots` includes Collection domain.
17. Manual refresh: `POST /api/admin/dashboard/refresh` with `{ "Domain": "Collection" }` succeeds.
18. Worker CLI: `btr.portal.worker --domain Collection --triggered-by Manual` succeeds.

---

## 10. Risk Analysis

| Risk | Likelihood | Impact | Mitigation |
| --- | --- | --- | --- |
| FF2 SQL grouping change breaks Desktop report | Low | High | Add `SalesPersonId` to SELECT/GROUP BY only; verify Desktop FF2 unchanged |
| SalesPersonName/id mismatch in LowRecoveryVsBilling | Medium | Medium | Key rep collections by `SalesPersonId`; fallback name map from master |
| Piutang rows without Faktur join (blank salesman/wilayah) | Low | Low | Exclude from salesman/wilayah aggregates; include in customer exposure |
| Overdue vs total ranking confusion in UI | Medium | Low | Label all rankings "Overdue"; subtitle distinguishes from M14 |
| Recovery lag vs 30-min refresh | Low | Low | Document staleness; no event-driven refresh per Q24 |
| Wilayah drill-down expectation | Medium | Low | No row click; document in operational guide |
| Payment mix denominator zero | Low | Low | Null percents when SettlementTotal = 0 |
| Dormant false positives | Low | Medium | Reuse exact M17 dormant rule including active-month exclusion |
| Collection refresh fails mid-RefreshAll | Low | Medium | Transactional ReplaceCurrent; refresh log marks Failed |
| Executive Phase 2 scope creep | Medium | Medium | Separate PR; Phase 1 explicitly excludes composer changes |

---

## 11. Documentation Updates (post-delivery)

| Document | Update |
| --- | --- |
| `docs/features/btr-portal/btr-portal-operational.md` | Collection route, signals, recovery formulas, drill-down limits, refresh cadence |
| `docs/features/btr-portal/btr-portal-domain.md` | Collection snapshot domain KPI definitions |
| `docs/features/btr-portal/btr-portal-architecture.md` | `DashboardCollectionAgg`, tables, worker, API endpoint |
| `docs/features/materialized-dashboard/materialized-dashboard-domain.md` | Collection domain refresh rules |
| `docs/work/btr-portal/M20-collection-dashboard-analysist.md` | Link to this plan |

---

## 12. Implementation Steps

Execute in order. Each phase should compile before proceeding.

### Phase 1 — Database

1. Create six `BTR_PortalDashboardCollection*.sql` table scripts.
2. Add to `btr.sql.sqlproj`; deploy to dev database.

### Phase 2 — Source DAL extensions

3. Add `IPiutangOpenBalanceWithWilayahDal` + `PiutangOpenBalanceWithWilayahDal`.
4. Extend `PenerimaanPelunasanSalesDto` and `PenerimaanPelunasanSalesDal` with `SalesPersonId`.

### Phase 3 — Backend core

5. Add `DashboardCollectionKeyResolver`.
6. Add aggregate models and `DashboardCollectionAggregator` with unit tests (Section 9.1).
7. Add `IDashboardCollectionSnapshotDal` + `DashboardCollectionSnapshotDal.ReplaceCurrent`.
8. Add `RefreshDashboardCollectionSnapshotWorker` + request/result types.
9. Add `CollectionIntervalMinutes` to `DashboardSnapshotOptions`.

### Phase 4 — Backend integration

10. Register Collection worker in `RefreshAllDashboardSnapshotsWorker` (last).
11. Add Collection case to `RefreshDashboardSnapshotsCommand` / admin refresh handler.
12. Add `DashboardCollectionAgg` read path: `GetDashboardCollectionQuery`, `IDashboardCollectionDal`, `DashboardCollectionDal`.
13. Add `CollectionDashboardController` — `GET /api/dashboard/collection`.
14. Wire DI in API and worker projects; update `HealthController`.
15. Update `btr.portal.worker/Program.cs` ValidDomains.

### Phase 5 — Frontend

16. Add TypeScript models and `fetchDashboardCollection`.
17. Add route and sidebar entry.
18. Build `CollectionDashboardView.vue` and section components (Section 7.3–7.6).
19. Wire dashboard store loader.

### Phase 6 — Verification

20. Run unit tests; execute manual checklist (Section 9.3).
21. Validate FF2 reconciliation sample for current month.
22. Update feature documentation (Section 11).

### Phase 7 — Executive promotion (separate delivery)

23. Extend `DashboardExecutiveComposer` with Collection KPI reads.
24. Update Executive dashboard UI cards.
25. Regression-test executive + collection dashboards together.

---

## Appendix A — Signal constants

```csharp
public const string SignalChronicOverdue = "ChronicOverdue";
public const string SignalLegacyDebt = "LegacyDebt";
public const string SignalPlafondBreachOverdue = "PlafondBreachOverdue";
public const string SignalOverdue = "Overdue";
public const string SignalHighOverdueWorkload = "HighOverdueWorkload";
public const string SignalLowRecoveryVsBilling = "LowRecoveryVsBilling";
public const string SignalWilayahHotspot = "WilayahHotspot";
```

## Appendix B — Reused business rules (do not reimplement differently)

| Rule | Source |
| --- | --- |
| Open balance: `Sisa > 1` / `KurangBayar > 1` | M5/M10/M14 |
| Aging from `JatuhTempo` vs today | `DashboardPiutangAggregator` |
| Customer key: Code else Name | `DashboardCustomerKeyResolver` |
| Salesman on open piutang: Faktur `SalesPersonId` | M18 / FF1 |
| Dormant: 90 days, prior history, exclude active month | M17 |
| Plafond breach: balance > Plafond when Plafond > 0 | M17 |
| Month omzet: current calendar month Faktur `GrandTotal` | Sales Dashboard |
| FF2 TotalBayar = Cash + Giro | `PenerimaanPelunasanSalesDal` |
| Rankings by overdue balance | PO M20 Q14 |
| Wilayah: `Customer.WilayahId` | PO M20 Q20 |
| Refresh cadence: 30 minutes | PO M20 Q23 |

---

*End of implementation plan — M20 Collection Dashboard*
