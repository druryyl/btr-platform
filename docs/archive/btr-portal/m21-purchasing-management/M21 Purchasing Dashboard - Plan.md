# Implementation Plan: M21 — Purchasing Management Dashboard

## Document Status

| Field | Value |
| --- | --- |
| Initiative | M21 Purchasing Management Dashboard — **Which suppliers and purchasing activities require management attention?** |
| Authoritative requirements | `docs/work/btr-portal/M21-purchasing-dashboard-analysis.md` — **Section 13 (Final Product Decisions)** |
| Related features | `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-operational.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md` |
| Reference pattern | M17 Customer Analytics (attention list + cards); M20 Collection Dashboard (dedicated snapshot domain); V1 Purchasing Dashboard (traceability baseline) |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |
| Status | **Ready for implementation** |
| Product decisions | **Confirmed** — analysis Section 13, 2026-06-09 |
| Open questions | **None** — all PO decisions in Section 13; architect-resolved rules in Section 2.4–2.5 |

---

## 1. Goal

Evolve the existing **Purchasing Dashboard** at `/dashboard/purchasing` into a **Purchasing Management Dashboard** that answers *Which suppliers and purchasing activities require management attention and why?* — without replacing V1 statistics or changing Desktop purchasing workflows.

**Primary outcomes:**

- Same route `/dashboard/purchasing`; page title **Purchasing Management Dashboard**.
- **Extend** V1 sections (Grand Total Purchase, Total Invoice, weekly trend, posting breakdown) — do not remove traceability KPIs.
- **Dedicated management snapshot domain** (`BTRPD_PurchasingManagement*`) with its own refresh worker — materialized attention KPIs, not read-time composition.
- **Attention-First layout** (Proposal A — fixed section order per analysis Section 11.1).
- **Qualified `BELUM` backlog** — age-based posting attention; not all unposted invoices are alerts.
- **Cross-domain panels** composing M15 inventory supplier concentration and M19 at-risk supplier exposure — no duplicate inventory SQL.
- **Attention List** at **Principal × Signal** grain with approved signals and drill-down to Purchasing Report (`?q=`).
- **Revise Executive Dashboard** purchasing `RequiresAttention` to use qualified backlog (Q3 — in M21 delivery scope).
- Strong navigation to Inventory Dashboard, Inventory Risk Dashboard, and Purchasing Report.

**Phase 2 (post-M21 stabilization — PO Q10):**

- Promote selected M21 signals to Management Attention Center cards beyond qualified backlog revision.

**Explicitly out of scope (PO confirmed):**

- Retur Beli analytics, PF2 line-level aggregates, purchase-to-sales ratio.
- Automated weekly spike/deceleration attention flags (visual-only trend retained).
- "Buying into slow/dead stock" calculation (M19 owns inventory risk).
- Purchasing Line Report route, PO/budget workflows, portal write path.
- Changes to V1 `BTRPD_Purchasing*` table schemas or V1 aggregator formulas for traceability KPIs.
- Event-driven snapshot refresh after invoice save/post.

---

## 2. Authoritative Product Decisions

Source: analysis Section 13. Do not re-decide these rules during implementation.

### 2.1 Positioning and audience

| Decision | Value |
| --- | --- |
| Route | `/dashboard/purchasing` (unchanged) |
| Page title | **Purchasing Management Dashboard** |
| Sidebar label | **Purchasing** (unchanged) |
| Audience | All authenticated users — no RBAC in M21 |
| V1 relationship | **Extend** — retain statistics sections |
| Executive relationship | Revise `RequiresAttention` in M21; further signal promotion **Phase 2** (Q10) |

### 2.2 Period semantics

| Metric family | Period | Filter |
| --- | --- | --- |
| Purchasing activity (Grand Total, Total Invoice, Top 10 principal, weekly trend, posting breakdown) | **Current calendar month** | Same as V1 — `InvoiceDate` in month, void exclusion |
| Qualified backlog | **Current calendar month invoices** | `PostingStok = BELUM` + age rule on extended invoice fields |
| Inventory / at-risk cross-domain | **Point-in-time** at refresh | Read M15/M19 snapshot rows — do not re-query inventory source tables |
| Purchasing Inactivity | **Current month + calendar day** | Architect rule in Section 2.5 |

### 2.3 Traceability (mandatory — unchanged)

| KPI | Must match |
| --- | --- |
| **Grand Total Purchase** | Purchasing Report footer total — same `IInvoiceViewDal`, current month, void exclusion |
| **Total Invoice** | Purchasing Report footer count |
| **Pending Posting Invoice Count / BELUM value (unqualified)** | V1 aggregator — supporting context only |

Qualified backlog metrics are **dashboard-only** — no report footer equivalent.

### 2.4 Architect-resolved rules (delegated from analysis Section 13 / 18)

Analysis explicitly delegates these to the architect. This plan resolves them.

#### Qualified backlog age rule (Q2, Q29)

| Term | Definition |
| --- | --- |
| **Age anchor field** | `LastUpdate` on `BTR_Invoice` (exposed via extended `InvoiceView`) |
| **Age threshold** | **3 calendar days** |
| **Qualified `BELUM` invoice** | `PostingStok = 'BELUM'` AND `(refreshToday − LastUpdate.Date).TotalDays ≥ 3` |
| **Qualified Backlog Count** | Count of qualified invoices in current-month read set |
| **Qualified Backlog Value** | `SUM(GrandTotal)` of qualified invoices |

**Rationale:** `SaveInvoiceWorker` always saves unposted. Active drafts are edited and `LastUpdate` advances; invoices untouched for ≥3 days while still `BELUM` represent delayed posting or abandoned entry — aligned with staged-entry workflow (Section 2.1 analysis). Use `LastUpdate` over `CreateTime` so in-progress multi-session entry is not prematurely flagged.

**Configuration:** Expose threshold as `DashboardSnapshotOptions.PurchasingQualifiedBacklogDays` (default **3**) for operational tuning without code change.

#### Purchasing Inactivity rule (Q17)

| Scope | Rule |
| --- | --- |
| **Company-level `PurchasingInactivity` signal** | `TotalInvoice = 0` for current month **AND** `DateTime.Today.Day ≥ 15` |
| **Attention list entity** | `EntityType = Company`, `EntityName = "(All)"` |
| **ValueText** | `"No purchase invoices recorded by mid-month"` |

No principal-level inactivity signal beyond `PrincipalInventoryNoPurchase` (Q13).

#### Compound Dependency rule (Q12)

| Term | Definition |
| --- | --- |
| **CompoundDependency flag** | Principal appears in **Top 10 MTD purchase** **AND** in **Top 10 inventory value (M15)** **OR** **Top 10 at-risk value (M19)** — i.e. purchase Top 10 plus at least one inventory-risk Top 10 |
| **Attention list** | One `CompoundDependency` row per qualifying principal |

Minimum two dimensions in Top 10; purchase concentration is required.

#### PrincipalInventoryNoPurchase rule (Q13)

| Term | Definition |
| --- | --- |
| **Qualifying principal** | In M15 supplier Top 10 (`DimensionType = Supplier`, `IsTop10 = true`) |
| **Condition** | `MtdPurchase = 0` for current calendar month |
| **ValueAmount** | M15 `InventoryValue` for context |

#### Concentration signals (Q28 — informational, not threshold engine)

| Signal | Inclusion rule |
| --- | --- |
| **PrincipalSpendConcentration** | Principal in Top 10 MTD purchase ranking |
| **PrincipalInventoryConcentration** | Principal in M15 supplier Top 10 |
| **PrincipalAtRiskExposure** | Principal in M19 supplier at-risk Top 10 |
| **UnknownPrincipal** | `PrincipalName` equals `"Unknown"` (same normalization as V1 aggregator) in Top 10 purchase **or** has qualified backlog |

**Top 1 / Top 3 Principal %:** Computed for attention cards — `Top N amount ÷ Grand Total Purchase × 100`. No auto-alert bands (Q28).

#### QualifiedBacklog signal (attention list)

| Signal | Entity | Rule |
| --- | --- | --- |
| **QualifiedBacklog** | Principal | Principal has ≥1 qualified `BELUM` invoice; `ValueAmount` = sum qualified `GrandTotal` for principal; `ValueText` = count of qualified invoices |

One row per principal with qualified backlog (not per invoice — Q5).

### 2.5 Attention signals (mandatory)

| Signal | Entity | Inclusion rule |
| --- | --- | --- |
| **QualifiedBacklog** | Principal | Section 2.4 |
| **PrincipalSpendConcentration** | Principal | Top 10 MTD purchase |
| **PrincipalInventoryConcentration** | Principal | M15 supplier Top 10 |
| **PrincipalAtRiskExposure** | Principal | M19 supplier at-risk Top 10 |
| **CompoundDependency** | Principal | Section 2.4 compound rule |
| **PurchasingInactivity** | Company | Section 2.4 inactivity rule |
| **PrincipalInventoryNoPurchase** | Principal | Section 2.4 |
| **UnknownPrincipal** | Principal | Section 2.4 |

**Presentation rules:**

- One row per **Principal × Signal** (Company row only for `PurchasingInactivity`).
- No duplicate rows for same principal × signal.
- Multiple signals per principal allowed (e.g. spend concentration + compound dependency).
- Generic **Attention Indicator** on cards — no per-signal severity engine (Q28).

**Attention list sort order:**

1. Signal priority: `QualifiedBacklog` → `CompoundDependency` → `PrincipalInventoryNoPurchase` → `UnknownPrincipal` → `PrincipalAtRiskExposure` → `PrincipalSpendConcentration` → `PrincipalInventoryConcentration` → `PurchasingInactivity`.
2. `ValueAmount` desc (nulls last).
3. `EntityName` asc.

### 2.6 Layout and drill-down

**Fixed section order (analysis Section 11.1):**

1. Purchasing Attention Cards  
2. Purchasing Summary (traceability row)  
3. Purchasing Attention List  
4. Weekly Purchase Trend + Posting Status Breakdown (V1 — visual only)  
5. Top 10 Principals + Principal Exposure Comparison (cross-domain)  
6. Navigation  

| UI element | Target |
| --- | --- |
| Principal attention / ranking row | `/reports/purchasing?q={PrincipalName}` |
| Navigation | Purchasing Report · Inventory Dashboard · Inventory Risk Dashboard |
| Inventory cross-links | `/dashboard/inventory`, `/dashboard/inventory-risk` |
| Qualified backlog context | Subtitle/tooltip: unqualified BELUM includes in-progress drafts |

### 2.7 Data architecture

| Decision | Value |
| --- | --- |
| V1 snapshot domain | `BTRPD_Purchasing*` — **unchanged** |
| Management snapshot domain | `BTRPD_PurchasingManagement*` |
| Refresh cadence | **30 minutes** (`PurchasingManagementIntervalMinutes`) |
| Refresh pattern | `SnapshotKey = 'CURRENT'` delete-and-replace |
| Read API | Extend **`GET /api/dashboard/purchasing`** — single response merging V1 + management |
| Materialization | All management derivations at refresh — **no read-time cross-domain composition** |
| Cross-domain inputs at refresh | `IDashboardInventorySnapshotDal`, `IDashboardInventoryRiskSnapshotDal` |

---

## 3. Architecture Overview

### 3.1 Target topology

```text
Source DALs (read at refresh time)
  IInvoiceViewDal (+ CreateTime, LastUpdate)     → month invoices, qualified backlog
  IDashboardInventorySnapshotDal                 → M15 supplier Top 10, total inventory
  IDashboardInventoryRiskSnapshotDal             → M19 supplier at-risk Top 10
    ↓
RefreshDashboardPurchasingManagementSnapshotWorker
    ↓ DashboardPurchasingManagementAggregator
BTRPD_PurchasingManagement* tables (3)
    ↓
Browser → GET /api/dashboard/purchasing
    ↓ MediatR GetDashboardPurchasingHandler
    ↓ DashboardPurchasingDal
      reads: IDashboardPurchasingSnapshotDal (V1)
            + IDashboardPurchasingManagementSnapshotDal (M21)
    ↓ DashboardPurchasingResponse (extended)

Parallel unchanged V1 path:
  RefreshDashboardPurchasingSnapshotWorker → BTRPD_Purchasing* (4 tables)

Executive (revised in M21):
  DashboardExecutiveDal reads V1 purchasing + management KPI
    ↓ DashboardExecutiveComposer.ComposePurchasing (qualified backlog RequiresAttention)
```

**Why separate management domain from V1:**

- PO Q7 — dedicated `BTRPD_PurchasingManagement*`.
- V1 traceability KPIs and tables must remain unchanged (Section 16.1 analysis).
- Management worker depends on Inventory/InventoryRisk snapshots — separate refresh order and failure isolation.

**Why extend existing API rather than new route:**

- Same page route; single load for V1 charts + management sections.
- Avoids frontend orchestrating two endpoints with mismatched `GeneratedAt`.

### 3.2 Design principles

| Principle | Application |
| --- | --- |
| Reuse V1 statistics | Weekly trend, posting pie, Grand Total, Total Invoice from V1 snapshot |
| Reuse cross-domain snapshots | Read M15/M19 snapshot DALs at refresh — do not duplicate inventory SQL |
| Reuse UI components | `ExecutiveAttentionCard`, `WeeklyTrendChart`, `PostingStatusPieChart`, `Top10RankingTable`, `InventoryHorizontalBarChart` (if suitable) |
| Preserve Desktop parity | Void exclusion, posting derivation, principal `"Unknown"` normalization |
| Fail gracefully | Missing management snapshot → management sections unavailable; V1 sections still render if V1 snapshot exists |
| Key by principal name | Match V1 aggregator `ResolvePrincipalName` — code-first not available on invoice view |

### 3.3 Architecture decisions

| Decision | Choice | Rationale |
| --- | --- | --- |
| Snapshot approach | **3 new management tables** | KPI + attention list + cross-domain top principal |
| Age field | **`LastUpdate`**, default **3 days** | Section 2.4 — distinguishes drafts from backlog |
| Top 10 % column | **Management TopPrincipal table** | V1 table unchanged; frontend uses management ranking |
| Cross-domain data | **Snapshot DAL reads at refresh** | PO Q9 — compose, don't re-aggregate |
| Compound rule | Purchase Top 10 + (Inventory Top 10 OR At-Risk Top 10) | Section 2.4 |
| Inactivity | Zero invoices + day ≥ 15 | Calendar-aware without complex week logic |
| Weekly trend alerts | **None** — visual only | Q18 |
| Executive RequiresAttention | `QualifiedBacklogCount > 0` | Q3 — replace all-BELUM logic |
| Executive PendingPostingValue | Retain unqualified BELUM value as **supporting metric** | Q4 |
| Refresh cadence | **30 minutes** | Align with V1 Purchasing |
| RefreshAll order | … → Purchasing (V1) → **PurchasingManagement** → Customer → … | Management after Inventory, InventoryRisk, V1 Purchasing |
| Worker domain name | `"PurchasingManagement"` | Distinct from V1 `"Purchasing"` |
| InvoiceView extension | Add `CreateTime`, `LastUpdate` to model + SQL | Q29 — CreateTime stored for future use; age rule uses LastUpdate |

---

## 4. Impact Analysis

### 4.1 Affected modules

| Layer | Module | Change type |
| --- | --- | --- |
| SQL | `BTRPD_PurchasingManagementKpi.sql` | **New** |
| SQL | `BTRPD_PurchasingManagementAttention.sql` | **New** |
| SQL | `BTRPD_PurchasingManagementTopPrincipal.sql` | **New** |
| SQL | `btr.sql.sqlproj` | Include new tables |
| Application | `InvoiceView.cs`, `IInvoiceViewDal` | Add `CreateTime`, `LastUpdate` |
| Application | `DashboardSnapshotAgg/Services/DashboardPurchasingManagementAggregator.cs` | **New** |
| Application | `DashboardSnapshotAgg/Services/DashboardPurchasingManagementKeyResolver.cs` | **New** |
| Application | `DashboardSnapshotAgg/Models/DashboardPurchasingManagementAggregateResult.cs` | **New** |
| Application | `DashboardSnapshotAgg/Contracts/IDashboardPurchasingManagementSnapshotDal.cs` | **New** |
| Application | `DashboardSnapshotAgg/UseCases/RefreshDashboardPurchasingManagementSnapshotWorker.cs` | **New** |
| Application | `DashboardSnapshotAgg/DashboardSnapshotOptions.cs` | Add `PurchasingManagementIntervalMinutes`, `PurchasingQualifiedBacklogDays` |
| Application | `DashboardSnapshotAgg/UseCases/RefreshAllDashboardSnapshotsWorker.cs` | Register management worker |
| Application | `DashboardSnapshotAgg/Commands/RefreshDashboardSnapshotsCommand.cs` | Add domain |
| Application | `DashboardPurchasingAgg/Queries/GetDashboardPurchasingQuery.cs` | Extend response DTOs |
| Application | `DashboardExecutiveAgg/Services/DashboardExecutiveComposer.cs` | Revise `ComposePurchasing` |
| Infrastructure | `InvoiceViewDal.cs` | SELECT `CreateTime`, `LastUpdate` |
| Infrastructure | `DashboardPurchasingManagementSnapshotDal.cs` | **New** |
| Infrastructure | `DashboardPurchasingDal.cs` | Merge V1 + management snapshots |
| Infrastructure | `DashboardExecutiveDal.cs` | Inject management snapshot for composer |
| API | `PurchasingDashboardController.cs` | No route change — response shape grows |
| API | `HealthController.cs` | Add `PurchasingManagement` domain |
| API | DI registrations | New DALs, worker, aggregator |
| Worker | `btr.portal.worker/Program.cs` | Add `PurchasingManagement` to `--domain` |
| Frontend | `PurchasingDashboardView.vue` | Attention-first layout; title change |
| Frontend | `components/dashboard/Purchasing*.vue` | **New** section components |
| Frontend | `models/dashboard.ts`, `api/dashboardApi.ts`, `stores/dashboardStore.ts` | Extended types |
| Tests | `btr.test/ReportingContext/` | Aggregator + key resolver + executive composer tests |
| Docs | Post-delivery feature docs | Operational, domain, materialized-dashboard updates |

### 4.2 Unaffected modules

| Module | Reason |
| --- | --- |
| V1 `DashboardPurchasingInvoiceAggregator` formulas | Traceability unchanged |
| V1 `BTRPD_Purchasing*` tables | PO Q7 — separate domain |
| `RefreshDashboardPurchasingSnapshotWorker` | Retained for V1 statistics |
| Purchasing Report DAL/API | Drill-down uses existing `?q=` |
| BTR Desktop PT1/PT2/PF1 | No write-path changes |
| M15/M19 aggregators and workers | Read-only consumption |
| Collection / Customer / Salesman dashboards | No overlap |

### 4.3 Metric traceability

| Management dashboard field | Source at refresh | Validating reference |
| --- | --- | --- |
| Grand Total Purchase | V1 snapshot (unchanged) | Purchasing Report footer |
| Total Invoice | V1 snapshot (unchanged) | Purchasing Report footer |
| Pending Posting (unqualified) | V1 snapshot | Purchasing Report — count/filter BELUM |
| Qualified Backlog Count/Value | Extended invoice view + age rule | Purchasing Report — manual filter BELUM + inspect dates |
| Top 10 Principal + % | Management aggregator | Purchasing Report grouped by Supplier |
| Top 1 / Top 3 Principal % | Management KPI | Executive / manual Top N ÷ footer |
| Inventory Value column | M15 snapshot breakdown | Inventory Dashboard supplier ranking |
| At-Risk Value column | M19 snapshot breakdown | Inventory Risk supplier exposure |
| Compound Dependency | Cross-domain Top 10 intersection | Manual spot-check across three dashboards |
| PrincipalInventoryNoPurchase | M15 Top 10 + zero month purchase | Inventory Report supplier + empty PF1 month |
| PurchasingInactivity | V1 TotalInvoice + calendar | PF1 empty month after day 15 |
| Weekly trend / posting pie | V1 snapshot | V1 reconciliation tests (existing) |

**Reconciliation notes:**

- `Grand Total Purchase` and `Total Invoice` on page must match V1 API fields exactly — sourced from V1 snapshot row.
- Management `GeneratedAt` may differ from V1 `GeneratedAt`; display **earlier of the two** as page freshness or show both in tooltip.
- Qualified backlog is a subset of unqualified BELUM — UI must label both clearly.

---

## 5. Database Design

Deploy all tables with `SnapshotKey = 'CURRENT'` delete-and-replace pattern.

### 5.1 `BTRPD_PurchasingManagementKpi`

Single row per refresh — attention cards + summary extensions.

| Column | Type | Description |
| --- | --- | --- |
| SnapshotKey | VARCHAR(10) PK | `'CURRENT'` |
| GeneratedAt | DATETIME | Refresh timestamp |
| PeriodYear | INT | Current month year |
| PeriodMonth | INT | Current month |
| QualifiedBacklogCount | INT | Age-qualified BELUM count |
| QualifiedBacklogValue | DECIMAL(18,2) | Sum qualified BELUM GrandTotal |
| PendingPostingValue | DECIMAL(18,2) | Unqualified BELUM value (supporting) |
| PostedPercent | DECIMAL(9,4) NULL | SUDAH value ÷ (SUDAH + BELUM) × 100 from V1 posting rows |
| Top1PrincipalPercent | DECIMAL(9,4) NULL | Top 1 MTD purchase ÷ Grand Total × 100 |
| Top3PrincipalPercent | DECIMAL(9,4) NULL | Top 3 MTD purchase ÷ Grand Total × 100 |
| Top1SupplierInventoryPercent | DECIMAL(9,4) NULL | From M15 KPI / breakdown |
| CompoundDependencyCount | INT | Principals matching compound rule |
| PrincipalInventoryNoPurchaseCount | INT | Qualifying principals |
| UnknownPrincipalCount | INT | Unknown in Top 10 or qualified backlog |
| PurchasingInactivityFlag | BIT | Company inactivity signal active |
| QualifiedBacklogPrincipalCount | INT | Distinct principals with qualified backlog |
| LastRefreshLogId | VARCHAR(13) | FK to refresh log |

### 5.2 `BTRPD_PurchasingManagementAttention`

Principal × Signal list (plus one Company row for inactivity).

| Column | Type | Description |
| --- | --- | --- |
| PurchasingManagementAttentionId | VARCHAR(13) PK | Generated ID |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| EntityType | VARCHAR(20) | `Principal` \| `Company` |
| EntityName | VARCHAR(50) | Principal or `"(All)"` |
| SignalKey | VARCHAR(40) | See Section 2.5 |
| SignalLabel | VARCHAR(50) | Display label |
| ValueAmount | DECIMAL(18,2) NULL | Primary monetary context |
| ValueText | VARCHAR(100) NULL | Secondary context (counts, %) |
| ReportRoute | VARCHAR(100) NULL | `/reports/purchasing` for Principal rows |
| SortOrder | INT | Stable list ordering |

Index: `(SnapshotKey, SortOrder)`

### 5.3 `BTRPD_PurchasingManagementTopPrincipal`

Top 10 MTD purchase with % and cross-domain comparison panel.

| Column | Type | Description |
| --- | --- | --- |
| PurchasingManagementTopPrincipalId | VARCHAR(13) PK | |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| Rank | INT | 1–10 |
| PrincipalName | VARCHAR(50) | |
| MtdPurchaseAmount | DECIMAL(18,2) | Current month purchase |
| PercentOfPurchase | DECIMAL(9,4) NULL | ÷ Grand Total Purchase × 100 |
| InventoryValue | DECIMAL(18,2) NULL | M15 supplier match; null if not in rollup |
| PercentOfInventory | DECIMAL(9,4) NULL | ÷ total inventory × 100 when matched |
| AtRiskValue | DECIMAL(18,2) NULL | M19 supplier match |
| PercentOfAtRisk | DECIMAL(9,4) NULL | ÷ total at-risk × 100 when matched |
| IsCompoundDependency | BIT | Compound rule satisfied |
| IsInventoryNoPurchase | BIT | Top 10 inventory + zero MTD purchase |
| ReportRoute | VARCHAR(100) | `/reports/purchasing` |

Unique: `(SnapshotKey, Rank)`

**Principal name matching:** Case-insensitive trim match between purchasing `SupplierName`, M15 `Name`, M19 `Name` — same as executive inventory supplier matching pattern.

---

## 6. Backend Implementation

### 6.1 New folder structure

```text
btr.application/ReportingContext/
├── DashboardSnapshotAgg/
│   ├── Services/
│   │   ├── DashboardPurchasingManagementAggregator.cs
│   │   └── DashboardPurchasingManagementKeyResolver.cs
│   ├── Models/
│   │   └── DashboardPurchasingManagementAggregateResult.cs
│   ├── Contracts/
│   │   └── IDashboardPurchasingManagementSnapshotDal.cs
│   └── UseCases/
│       ├── RefreshDashboardPurchasingManagementSnapshotWorker.cs
│       └── RefreshDashboardPurchasingManagementSnapshotRequest.cs (+ Result)
└── DashboardPurchasingAgg/
    └── Queries/
        └── GetDashboardPurchasingQuery.cs          (extend response)

btr.infrastructure/ReportingContext/
└── DashboardSnapshotAgg/
    └── DashboardPurchasingManagementSnapshotDal.cs

btr.application/PurchaseContext/InvoiceInfo/
├── InvoiceView.cs                                   (extend)
└── IInvoiceViewDal.cs

btr.infrastructure/PurchaseContext/InvoiceInfoRpt/
└── InvoiceViewDal.cs                                (extend SQL)
```

Add all new `.cs` files to respective `.csproj` Compile includes.

### 6.2 Source DAL extension — `InvoiceView`

Add to `InvoiceView`:

```csharp
public DateTime CreateTime { get; set; }
public DateTime LastUpdate { get; set; }
```

Extend `InvoiceViewDal` SQL:

```sql
aa.CreateTime, aa.LastUpdate,
```

Preserve existing filters and column aliases. Verify Purchasing Report traceability tests still pass — new columns are additive.

### 6.3 `RefreshDashboardPurchasingManagementSnapshotWorker`

Load at refresh:

| Input | Method | Purpose |
| --- | --- | --- |
| Month invoices | `_invoiceViewDal.ListData(currentMonthPeriode)` | Purchase grouping, qualified backlog |
| Inventory snapshot | `_inventorySnapshotDal.GetCurrent()` | Supplier Top 10, total inventory value |
| Inventory risk snapshot | `_inventoryRiskSnapshotDal.GetCurrent()` | Supplier at-risk Top 10, total at-risk |
| V1 purchasing snapshot | `_purchasingSnapshotDal.GetCurrent()` | Grand Total denominator, posting split, optional fallback if invoice read fails |

Pass `today = DateTime.Today`, `generatedAt = UtcNow`, `qualifiedBacklogDays` from options to aggregator.

Transactional `ReplaceCurrent` via `IDashboardPurchasingManagementSnapshotDal`.

**Failure handling:** If Inventory or InventoryRisk snapshot is null, still refresh purchasing-only signals; cross-domain columns null; compound/principal-inventory signals omitted; log warning in refresh result.

### 6.4 `DashboardPurchasingManagementAggregator` — core logic

**Pass 1 — Qualified backlog (invoice rows):**

1. Filter current-month rows where `PostingStok = BELUM`.
2. Apply age rule: `(today − row.LastUpdate.Date).TotalDays ≥ qualifiedBacklogDays`.
3. Aggregate company qualified count/value; group by principal for attention list and principal count.

**Pass 2 — MTD purchase Top 10:**

1. Group all month invoices by principal (reuse `ResolvePrincipalName`).
2. Build Top 10 by purchase amount; compute `PercentOfPurchase`.
3. Compute Top1/Top3 percentages for KPI row.

**Pass 3 — Cross-domain maps:**

1. From M15 breakdown: supplier Top 10 lookup (`DimensionType = Supplier`, `IsTop10`).
2. From M19 breakdown: supplier at-risk Top 10 lookup.
3. Build `PrincipalExposure` rows for Top 10 purchase principals — left-join inventory and at-risk values by name.

**Pass 4 — Signals and attention list:**

- Emit rows per Section 2.5 rules.
- Set KPI counters from signal emission.
- `PurchasingInactivityFlag` from V1 `TotalInvoice` (or recount) + calendar day rule.

**Pass 5 — Posted percent:**

- Read V1 posting status rows from purchasing snapshot: `SUDAH / (SUDAH + BELUM) × 100`.

### 6.5 Read-side API — extended `DashboardPurchasingResponse`

Add sections (V1 fields unchanged):

```csharp
public bool IsManagementAvailable { get; set; }
public bool IsDataFresh { get; set; }
public DashboardPurchasingAttentionCards AttentionCards { get; set; }
public DashboardPurchasingSummaryRow Summary { get; set; }
public IList<DashboardPurchasingAttentionItem> AttentionList { get; set; }
public IList<DashboardPurchasingPrincipalExposureItem> PrincipalExposure { get; set; }
public DashboardPurchasingNavigationLinks Navigation { get; set; }
```

**V1 fields retained at root:** `GrandTotalPurchase`, `TotalInvoice`, `PendingPostingInvoiceCount`, `WeeklyTrend`, `PostingStatusBreakdown`, `TopPrincipalRanking` (optional — frontend may prefer `PrincipalExposure` for Top 10 display with %).

**Attention card groups and `RequiresAttention`:**

| Card group | Fields | `RequiresAttention` when |
| --- | --- | --- |
| **Posting Exposure** | Qualified Backlog Count · Qualified Backlog Value · Pending Posting Value (supporting) | `QualifiedBacklogCount > 0` |
| **Principal Dependency** | Top 1 Principal % · Top 3 Principal % · Compound Dependency Count | `CompoundDependencyCount > 0` OR `UnknownPrincipalCount > 0` |
| **Purchasing Pace** | Total Invoice · Purchasing Inactivity indicator | `PurchasingInactivityFlag = true` |
| **Inventory Cross-Risk** | Top 1 Supplier Inventory % · Principal At-Risk Count | `PrincipalAtRiskExposure` attention count > 0 OR `PrincipalInventoryNoPurchaseCount > 0` |

Apply M16 **Attention Indicator** border when respective group `RequiresAttention` is true.

**IsDataFresh:** `(UtcNow − min(V1.GeneratedAt, Management.GeneratedAt)).TotalMinutes ≤ PurchasingManagementIntervalMinutes`

### 6.6 Executive composer revision

Update `DashboardExecutiveDal` to inject `IDashboardPurchasingManagementSnapshotDal`.

Update `ExecutiveComposeInput` / `ComposePurchasing`:

| Field | Source |
| --- | --- |
| `PendingPostingInvoiceCount` | V1 snapshot (unchanged display) |
| `PendingPostingValue` | V1 unqualified BELUM value (supporting) |
| `QualifiedBacklogCount` | Management KPI (**new field on executive DTO**) |
| `TopPrincipalPercent` | V1 top principal % (unchanged) |
| **`RequiresAttention`** | **`QualifiedBacklogCount > 0`** |

Do **not** use unqualified BELUM count/value for `RequiresAttention` (Q3).

Update `DashboardExecutiveComposerTest` and executive frontend if new qualified field displayed on purchasing card.

### 6.7 Configuration and infrastructure wiring

**`DashboardSnapshotOptions`:**

```csharp
public int PurchasingManagementIntervalMinutes { get; set; } = 30;
public int PurchasingQualifiedBacklogDays { get; set; } = 3;
```

**Register in:**

- `ApplicationPortalExtensions.cs` — aggregator, worker
- `InfrastructurePortalExtensions.cs` — snapshot DAL
- `WorkerDependencyConfig` — worker host

**Update domain lists in:**

- `HealthController.BuildDomainStatuses` — add `"PurchasingManagement"`
- `RefreshDashboardSnapshotsHandler` — add case
- `RefreshAllDashboardSnapshotsWorker` — run **after** V1 Purchasing worker
- `btr.portal.worker/Program.cs` — `ValidDomains` includes `PurchasingManagement`

**Task Scheduler:** Add job `BTR Portal Dashboard Purchasing Management Refresh` every 30 minutes.

---

## 7. Frontend Implementation

### 7.1 Route and sidebar

| Item | Change |
| --- | --- |
| Route | `/dashboard/purchasing` — **unchanged** |
| Page title | **Purchasing Management Dashboard** |
| Subtitle | *Which suppliers and purchasing activities require management attention?* |
| Sidebar | **Purchasing** — unchanged |

### 7.2 Store and API

- Extend `DashboardPurchasingResponse` TypeScript interface in `dashboard.ts`.
- `fetchDashboardPurchasing()` unchanged URL — parse new sections.
- `loadPurchasing()` unchanged.

### 7.3 Page layout

```text
┌─────────────────────────────────────────────────────────────┐
│ Purchasing Management Dashboard          Last Refreshed: …  │
│ Which suppliers and purchasing activities require attention? │
│ Subtitle: Current Month Purchasing — Management Attention    │
├─────────────────────────────────────────────────────────────┤
│ [⚠ Dashboard Data Not Fresh]  (when !IsDataFresh)            │
├─────────────────────────────────────────────────────────────┤
│ 1. PURCHASING ATTENTION CARDS (4 groups)                     │
│ Posting Exposure | Principal Dependency | Pace | Cross-Risk  │
├─────────────────────────────────────────────────────────────┤
│ 2. PURCHASING SUMMARY (traceability)                         │
│ Grand Total Purchase | Total Invoice | Posted % | BELUM val  │
├─────────────────────────────────────────────────────────────┤
│ 3. PURCHASING ATTENTION LIST                                 │
│ Principal | Signal | Amount | Context | [→ Report]           │
├─────────────────────────────────────────────────────────────┤
│ 4. WEEKLY PURCHASE TREND  |  POSTING STATUS (V1 charts)    │
├─────────────────────────────────────────────────────────────┤
│ 5. TOP 10 PRINCIPALS (with %)  |  PRINCIPAL EXPOSURE TABLE   │
│ MTD Purchase · Inventory Value · At-Risk Value               │
├─────────────────────────────────────────────────────────────┤
│ 6. NAVIGATION                                                │
│ → Purchasing Report | Inventory Dashboard | Inventory Risk   │
└─────────────────────────────────────────────────────────────┘
```

**Purchasing Summary row:** Grand Total and Total Invoice from V1 root fields; Posted % and BELUM value from management summary / V1 posting breakdown.

**Tooltip on Pending/Qualified posting:** *Unposted (BELUM) is normal immediately after invoice entry. Qualified backlog counts invoices unposted for 3+ days.*

### 7.4 Components

| Component | Purpose |
| --- | --- |
| `DashboardDetailLayout.vue` | Page shell (reuse) |
| `PurchasingAttentionCardGroup.vue` | **New** — four card groups |
| `PurchasingSummaryRow.vue` | **New** — traceability metrics |
| `PurchasingAttentionList.vue` | **New** — Principal × Signal DataTable |
| `PurchasingPrincipalExposureTable.vue` | **New** — cross-domain comparison grid |
| `WeeklyTrendChart.vue` | V1 weekly trend (reuse) |
| `PostingStatusPieChart.vue` | V1 posting breakdown (reuse) |
| `Top10RankingTable.vue` | Top 10 with `PercentOfTotal` (extend columns) |
| `PurchasingNavigationSection.vue` | **New** — cross-dashboard links |

### 7.5 Row click and pre-filter

**Helper** — reuse `navigateToReport`:

```typescript
navigateToReport(router, '/reports/purchasing', principalName)
```

- Attention list and Top 10 / exposure rows: navigate with `?q=` principal name.
- Company `PurchasingInactivity` row: no drill-down (no invoice subset).

### 7.6 Cross-links

| Link | Route |
| --- | --- |
| Inventory Dashboard | `/dashboard/inventory` |
| Inventory Risk Dashboard | `/dashboard/inventory-risk` |
| Purchasing Report | `/reports/purchasing` |

Optional: pass search hint query param if Inventory Risk supports supplier search in future — **not required for M21** if unsupported.

---

## 8. Phase 2 — Executive Signal Promotion

Execute **after** M21 Purchasing Management dashboard is shipped and validated. Separate PR.

| Executive field | Management snapshot source |
| --- | --- |
| Compound Dependency Count | `CompoundDependencyCount` |
| Qualified Backlog Value | `QualifiedBacklogValue` |
| Top 3 Principal % | `Top3PrincipalPercent` |

**Implementation sketch:**

1. Extend executive purchasing card UI with selected M21 KPIs.
2. Keep `RequiresAttention` on qualified backlog (already revised in Phase 1).
3. Do not remove existing pending posting supporting metrics.

---

## 9. Testing

### 9.1 Unit tests — `DashboardPurchasingManagementAggregatorTest`

| Case | Assertion |
| --- | --- |
| Qualified backlog — fresh BELUM | Invoice BELUM with LastUpdate 1 day ago → excluded |
| Qualified backlog — stale BELUM | BELUM with LastUpdate 4 days ago → included |
| Qualified backlog age boundary | Exactly 3 days → included (≥ threshold) |
| Qualified backlog — SUDAH | Posted invoice excluded regardless of age |
| Top 10 purchase % | Row percent sums logically; Top1/Top3 KPI correct |
| PrincipalSpendConcentration | Top 10 principals emit signal |
| PrincipalInventoryConcentration | M15 Top 10 match emits signal |
| PrincipalAtRiskExposure | M19 Top 10 match emits signal |
| CompoundDependency | Purchase Top 10 + inventory Top 10 → flag and signal |
| CompoundDependency — partial | Purchase Top 10 only → no compound |
| PrincipalInventoryNoPurchase | M15 Top 10 + zero purchase → signal |
| UnknownPrincipal | Blank supplier in Top 10 → signal |
| PurchasingInactivity | Zero invoices + day 20 → company signal |
| PurchasingInactivity — early month | Zero invoices + day 5 → no signal |
| Missing inventory snapshot | Cross-domain columns null; purchase signals still emit |
| Attention sort order | QualifiedBacklog rows before concentration rows |
| One row per principal × signal | No duplicates |

### 9.2 Unit tests — `DashboardPurchasingManagementKeyResolverTest`

Verify principal name normalization matches V1 aggregator (`Unknown`, trim, case).

### 9.3 Unit tests — `DashboardExecutiveComposerTest`

| Case | Assertion |
| --- | --- |
| Unqualified BELUM only | `RequiresAttention = false` when qualified count = 0 |
| Qualified backlog present | `RequiresAttention = true` |
| Qualified count zero, BELUM value > 0 | `RequiresAttention = false` (Q3) |

### 9.4 Integration / manual test checklist

1. Deploy SQL tables; run PurchasingManagement refresh; verify three tables populated.
2. `GET /api/dashboard/purchasing` returns V1 + management sections.
3. Page title **Purchasing Management Dashboard**; sections in fixed order.
4. Grand Total Purchase and Total Invoice match Purchasing Report footer.
5. Qualified backlog spot-check: filter BELUM in report; verify aged invoices match count.
6. Top 10 % column present; Top 1 % matches manual calculation.
7. Principal Exposure table shows inventory and at-risk columns for matched names.
8. Click attention row → Purchasing Report with `?q=` pre-filled.
9. Navigation links reach Inventory and Inventory Risk dashboards.
10. Executive purchasing card: `RequiresAttention` only when qualified backlog > 0.
11. **"⚠ Dashboard Data Not Fresh"** when snapshot exceeds 30-minute interval.
12. V1 weekly trend and posting pie unchanged.
13. `GET /api/health/dashboard-snapshots` includes `PurchasingManagement`.
14. Manual refresh: `POST /api/admin/dashboard/refresh` with `{ "Domain": "PurchasingManagement" }`.
15. Worker CLI: `btr.portal.worker --domain PurchasingManagement --triggered-by Manual`.
16. RefreshAll order: management runs after V1 Purchasing and Inventory domains.

---

## 10. Risk Analysis

| Risk | Likelihood | Impact | Mitigation |
| --- | --- | --- | --- |
| `LastUpdate` not maintained on all invoice saves | Low | Medium | Spot-check `InvoiceDal` writes; fall back to `CreateTime` if field static in QA |
| Principal name mismatch across domains | Medium | Medium | Case-insensitive trim join; document unmatched rows as null cross-domain columns |
| Inventory snapshot stale vs purchasing refresh | Medium | Low | Accept point-in-time difference; show separate GeneratedAt in tooltip |
| Management snapshot missing on first deploy | Medium | Low | V1 sections render; management shows unavailable banner |
| Executive card confusion (two posting metrics) | Medium | Low | Label "Qualified Backlog" vs "Pending Posting (all BELUM)" |
| 3-day threshold too aggressive/lenient | Medium | Low | Configurable via `PurchasingQualifiedBacklogDays` |
| Extended InvoiceView breaks report tests | Low | Medium | Additive columns only; run existing traceability tests |
| RefreshAll failure when Inventory null | Low | Medium | Management worker tolerates missing cross-domain snapshot |
| UI scope creep (PF2, Retur Beli) | Low | Medium | Enforce Section 13 exclusions in PR review |

---

## 11. Documentation Updates (post-delivery)

| Document | Update |
| --- | --- |
| `docs/features/btr-portal/btr-portal-operational.md` | Management route title, signals, qualified backlog rule, drill-down, refresh cadence |
| `docs/features/btr-portal/btr-portal-domain.md` | Purchasing management KPI definitions, signal catalog |
| `docs/features/materialized-dashboard/materialized-dashboard-domain.md` | PurchasingManagement domain, ninth refresh job |
| `docs/work/btr-portal/M21-purchasing-dashboard-analysis.md` | Link to this plan |

---

## 12. Implementation Steps

Execute in order. Each phase should compile before proceeding.

### Phase 1 — Database

1. Create three `BTRPD_PurchasingManagement*.sql` table scripts.
2. Add to `btr.sql.sqlproj`; deploy to dev database.

### Phase 2 — Source DAL extension

3. Add `CreateTime`, `LastUpdate` to `InvoiceView` and `InvoiceViewDal` SQL.
4. Run existing purchasing traceability tests — confirm no regression.

### Phase 3 — Backend core

5. Add `DashboardPurchasingManagementKeyResolver`.
6. Add aggregate models and `DashboardPurchasingManagementAggregator` with unit tests (Section 9.1).
7. Add `IDashboardPurchasingManagementSnapshotDal` + `DashboardPurchasingManagementSnapshotDal.ReplaceCurrent`.
8. Add `RefreshDashboardPurchasingManagementSnapshotWorker` + request/result types.
9. Add `PurchasingManagementIntervalMinutes` and `PurchasingQualifiedBacklogDays` to `DashboardSnapshotOptions`.

### Phase 4 — Backend integration

10. Register PurchasingManagement worker in `RefreshAllDashboardSnapshotsWorker` (after V1 Purchasing).
11. Add domain case to `RefreshDashboardSnapshotsCommand` / admin refresh handler.
12. Extend `GetDashboardPurchasingQuery` response; update `DashboardPurchasingDal` to merge snapshots.
13. Revise `DashboardExecutiveComposer.ComposePurchasing` and `DashboardExecutiveDal` (Section 6.6).
14. Wire DI in API and worker projects; update `HealthController`.
15. Update `btr.portal.worker/Program.cs` ValidDomains.

### Phase 5 — Frontend

16. Extend TypeScript models for management sections.
17. Build `PurchasingAttentionCardGroup`, `PurchasingSummaryRow`, `PurchasingAttentionList`, `PurchasingPrincipalExposureTable`, `PurchasingNavigationSection`.
18. Refactor `PurchasingDashboardView.vue` to attention-first layout (Section 7.3).
19. Update page title and freshness banner.

### Phase 6 — Verification

20. Run unit tests including executive composer tests (Section 9.3).
21. Execute manual checklist (Section 9.4).
22. Validate Grand Total / Total Invoice traceability against Purchasing Report.
23. Update feature documentation (Section 11).

### Phase 7 — Executive promotion (separate delivery)

24. Add supplementary M21 KPIs to executive purchasing card UI.
25. Regression-test executive + purchasing dashboards together.

---

## Appendix A — Signal constants

```csharp
public const string SignalQualifiedBacklog = "QualifiedBacklog";
public const string SignalPrincipalSpendConcentration = "PrincipalSpendConcentration";
public const string SignalPrincipalInventoryConcentration = "PrincipalInventoryConcentration";
public const string SignalPrincipalAtRiskExposure = "PrincipalAtRiskExposure";
public const string SignalCompoundDependency = "CompoundDependency";
public const string SignalPurchasingInactivity = "PurchasingInactivity";
public const string SignalPrincipalInventoryNoPurchase = "PrincipalInventoryNoPurchase";
public const string SignalUnknownPrincipal = "UnknownPrincipal";
```

## Appendix B — Reused business rules (do not reimplement differently)

| Rule | Source |
| --- | --- |
| Void exclusion `VoidDate = '3000-01-01'` | `InvoiceViewDal` |
| `PostingStok` derivation | `IIF(IsStokPosted = 1, 'SUDAH', 'BELUM')` |
| Blank supplier → Unknown | `DashboardPurchasingInvoiceAggregator.ResolvePrincipalName` |
| Current month period | `PurchasingReportDal.CurrentMonthPeriode()` pattern |
| Week buckets | `SalesOmzetChartWeekGrouper` |
| M15 supplier Top 10 | `DashboardInventoryAggregator.DimensionSupplier`, `IsTop10` |
| M19 supplier at-risk Top 10 | `DashboardInventoryRiskSnapshotDal` breakdown |
| Drill-down `?q=` | M17 `navigateToReport` pattern |
| Refresh cadence 30 min | V1 Purchasing / M20 Collection |
| Attention list grain | M17/M20 Principal × Signal pattern |

## Appendix C — Open questions verification

| Analysis item | Status |
| --- | --- |
| Section 13 PO decisions | **Resolved** — authoritative |
| Section 18 architect handoff checklist | **Complete** — all `[x]` |
| Age threshold for qualified BELUM | **Resolved** — Section 2.4 (3 days, LastUpdate) |
| Calendar rule for Purchasing Inactivity | **Resolved** — Section 2.4 (day ≥ 15, zero invoices) |
| Compound Dependency composition | **Resolved** — Section 2.4 |
| Attention list grain | **Resolved** — Principal × Signal (Q27) |
| Executive RequiresAttention inputs | **Resolved** — Section 6.6 |
| Weekly spike/deceleration | **Resolved** — visual only (Q18), excluded |
| Retur Beli / PF2 / purchase-sales ratio | **Resolved** — excluded |

---

*End of implementation plan — M21 Purchasing Management Dashboard*
