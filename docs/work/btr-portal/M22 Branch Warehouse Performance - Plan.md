# Implementation Plan: M22 — Branch / Warehouse Performance Dashboard

## Document Status

| Field | Value |
| --- | --- |
| Initiative | M22 Branch / Warehouse Performance Dashboard — **Are we becoming too dependent on a particular warehouse or territory?** |
| Internal purpose | **Location Concentration Dashboard** — warehouse + wilayah concentration, not operational productivity |
| Authoritative requirements | `docs/work/btr-portal/M22 Branch Warehouse Performance - Analysis.md` — **Section 16 (Product Owner Decisions)** |
| Related features | `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-operational.md`, `docs/features/btr-portal/btr-portal-architecture.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md` |
| Reference pattern | M20 Collection Dashboard (dedicated snapshot domain, attention list); M21 Purchasing Management (cross-domain snapshot denominators); M15/M19 inventory valuation rules |
| Solution | `src/j05-btr-distrib/j05-btr-distrib.sln` |
| Author role | Architect |
| Implementer input | This document |
| Status | **Ready for implementation** |
| Product decisions | **Confirmed** — analysis Section 16, 2026-06-09 |
| Open questions | **None** — all PO decisions in Section 16; architect-resolved rules in Section 2.4–2.5 |

---

## 1. Goal

Deliver **Branch / Warehouse Performance Dashboard** at `/dashboard/locations` — a **location concentration** view answering *Are we becoming too dependent on a particular warehouse or territory?*

**Primary outcomes:**

- New route `/dashboard/locations`; page title **Branch / Warehouse Performance Dashboard**.
- Sidebar label **Locations** (page subtitle clarifies warehouse + wilayah scope).
- **Dedicated Location snapshot domain** (`BTR_PortalDashboardLocation*`) with its own refresh worker — materialized KPIs, not read-time composition.
- **Dual equal framing** — Warehouse (logistics/inventory/billing) and Wilayah (commercial territory) on one page; **Branch is UI language only** — no `BranchId` entity.
- **Attention-First layout** (fixed section order per analysis Section 13.2).
- Five **Top 10 ranking** panels (four warehouse, one wilayah) plus **informational concentration cards**.
- **Attention List** at **Warehouse × Signal** grain with six approved signals.
- Mandatory **navigation cross-links** to seven sibling dashboards — **no embedded** M20 overdue wilayah or M21 qualified backlog panels.
- Drill-down: warehouse rows → Inventory Report (`?q=`); wilayah sales rows → Collection Dashboard.
- **M23-compatible** signal keys and list grain for future Alert Center consumption.

**Phase 2 (post-M22 stabilization — PO Q23):**

- Promote selected location concentration KPIs to Management Attention Center (`/dashboard`).

**Explicitly out of scope (PO confirmed):**

- Depo analytics, piutang by billing warehouse, movement KPIs (`MovingStok`, IF8).
- Productivity ratios, inventory turnover, warehouse sales targets.
- `WarehouseHotspot` auto-threshold, `CrossWilayahSelling`, SKU-level warehouse imbalance.
- Qualified backlog by warehouse, embedded Top Overdue Wilayah.
- Sales Report / Piutang Report API column extensions (Q27).
- Executive Dashboard changes, M15/M16/M19/M20/M21 snapshot table or aggregator modifications.
- Historical location trends, YoY, event-driven refresh.

---

## 2. Authoritative Product Decisions

Source: analysis Section 16. Do not re-decide these rules during implementation.

### 2.1 Positioning and audience

| Decision | Value |
| --- | --- |
| Route | `/dashboard/locations` |
| Page title | **Branch / Warehouse Performance Dashboard** |
| Sidebar label | **Locations** |
| Audience | All authenticated users — no RBAC in M22 |
| Executive relationship | **Phase 2 only** — no M16 changes in M22 V1 (Q23) |
| M20 relationship | **Link only** — no embedded wilayah overdue rankings (Q20) |
| M21 relationship | **Link only** — no qualified backlog by warehouse (Q22) |
| M19 relationship | **Reuse classification exactly** — no M19 snapshot changes (Q21) |

### 2.2 Period semantics

| Metric family | Period | Filter |
| --- | --- | --- |
| Inventory value by warehouse | **Point-in-time** at refresh | M15-aligned: `Hpp × Qty`, exclude In-Transit |
| At-risk value by warehouse | **Point-in-time** at refresh | M19 item classes × warehouse stock rows |
| MTD omzet by warehouse / wilayah | **Current calendar month** | Non-void Faktur — same as Sales Dashboard |
| MTD purchase by warehouse | **Current calendar month** | Non-void Invoice — same as Purchasing Dashboard |
| Concentration % denominators | **Snapshot KPIs** at refresh | M15 total inventory, M19 at-risk total, Sales total omzet, Purchasing grand total |
| Inactive warehouse signal | **Point-in-time** | `IsAktif = false` AND inventory value > 0 |

### 2.3 Traceability (mandatory)

| KPI | Must match |
| --- | --- |
| **Sum warehouse inventory values** | Inventory Report footer total (same exclusion rules) |
| **Sum warehouse MTD omzet** | Sales Dashboard `TotalOmzet` / Sales Report month total |
| **Sum wilayah MTD omzet** | Sales Dashboard `TotalOmzet` |
| **Sum warehouse MTD purchase** | Purchasing Dashboard `GrandTotalPurchase` / Purchasing Report footer |
| **Sum warehouse at-risk values** | ≤ M19 `AtRiskInventoryValue` (warehouse split of same classified items) |
| **Top Overdue Wilayah** | **Not computed** — Collection Dashboard link only |

### 2.4 Architect-resolved rules (delegated from analysis Sections 12, 17)

Analysis delegates warehouse universe, signal inclusion, and classification wiring to the architect. This plan resolves them.

#### Warehouse ranking universe (Q5)

| Rule | Definition |
| --- | --- |
| **Ranking-eligible warehouse** | `IsAktif = true` AND `IsSpecial = false` AND `WarehouseName` ≠ `"In-Transit"` (case-sensitive name match per M15) |
| **Top 10 rankings** | Only ranking-eligible warehouses |
| **Concentration signals (Top 10)** | Only ranking-eligible warehouses |
| **WarehouseInactiveWithStock** | **Exception** — may include `IsAktif = false` warehouses; still exclude In-Transit by name |
| **WarehouseNoSalesWithInventory** | Ranking-eligible warehouse with inventory value > 0 and MTD omzet = 0 |

**Rationale:** Aligns with Desktop `WarehouseBrowser` (`IsSpecial == false`) and portal In-Transit exclusion. Inactive sites surface only via dedicated attention signal.

#### Warehouse master read path (Q18)

| Term | Definition |
| --- | --- |
| **Portal warehouse DTO** | Extend read model with `IsAktif`, `IsSpecial` — do **not** change Desktop `ListData()` filter (`IsAktif = 1` only) |
| **New DAL method** | `IWarehouseDal.ListAllForPortal()` — returns all warehouses with `WarehouseId`, `WarehouseName`, `IsSpecial`, `IsAktif` |
| **Ranking lookup** | Build `HashSet<string>` of eligible `WarehouseId` / normalized `WarehouseName` |

Add `IsAktif` to `WarehouseModel` as optional additive property; existing Desktop paths unaffected if `ListData()` SQL unchanged.

#### Warehouse grouping key

| Term | Definition |
| --- | --- |
| **Group key** | Trimmed `WarehouseName`; fallback to `WarehouseId` when name blank |
| **Display name** | `WarehouseName` trimmed |
| **Drill-down** | Inventory Report `?q={WarehouseName}` — same string as ranking row |

#### Inventory value by warehouse

| Term | Definition |
| --- | --- |
| **Source rows** | `IStokBalanceViewDal.ListData()` — Item × Warehouse grain |
| **Row filter** | `Qty > 0` AND `WarehouseName != "In-Transit"` |
| **Warehouse value** | `SUM(Hpp × Qty)` per warehouse group key |
| **Company total (denominator)** | M15 snapshot `TotalInventoryValue`; fallback: sum all warehouse values with same exclusion rules |

#### At-risk value by warehouse (M19 reuse — Q21)

| Step | Rule |
| --- | --- |
| 1 | Build company item groups via `DashboardInventoryItemGroupBuilder.BuildItemGroups` |
| 2 | Classify each `BrgId` using **identical** M19 thresholds: Never Sold (no last Faktur), Slow Moving (≥ 90 days), Dead Stock (≥ 180 days) — constants from `DashboardInventoryRiskAggregator` |
| 3 | Build `HashSet<string>` of at-risk `BrgId` values |
| 4 | For each `StokBalanceView` row (qty > 0, not In-Transit), if `BrgId` ∈ at-risk set, add `Hpp × Qty` to warehouse bucket |
| 5 | **Do not** reclassify per warehouse last-Faktur — item movement signal is company-global |

**Implementation note:** Extract shared `DashboardInventoryRiskClassifier.BuildAtRiskBrgIdSet(...)` used by Location aggregator and optionally refactor M19 aggregator to call it — **M19 snapshot tables and refresh worker unchanged**. Add cross-aggregator unit test asserting identical at-risk BrgId sets for fixture data.

#### Sales and purchasing by warehouse

| Metric | Source | Rule |
| --- | --- | --- |
| **MTD omzet by warehouse** | `IFakturViewDal.ListData(currentMonthPeriode)` | `SUM(GrandTotal)` group by billing `WarehouseName`; void excluded by DAL |
| **MTD purchase by warehouse** | `IInvoiceViewDal.ListData(currentMonthPeriode)` | `SUM(GrandTotal)` group by `WarehouseName`; void excluded |
| **MTD omzet by wilayah** | Same Faktur set | `SUM(GrandTotal)` group by customer `WilayahName` (Q6) |
| **Wilayah normalization** | Blank/null → `"Unknown"` | Same as M17/M21 dimension normalization |

#### Concentration KPI cards (Q17 — informational only)

| KPI field | Formula |
| --- | --- |
| **Top1WarehouseInventoryPercent** | Rank-1 warehouse inventory value ÷ M15 `TotalInventoryValue` × 100 |
| **Top3WarehouseInventoryPercent** | Sum ranks 1–3 warehouse inventory ÷ total × 100 |
| **Top1WarehouseAtRiskPercent** | Rank-1 at-risk warehouse value ÷ M19 `AtRiskInventoryValue` × 100 |
| **Top1WarehouseSalesPercent** | Rank-1 warehouse MTD omzet ÷ Sales `TotalOmzet` × 100 |
| **Top1WilayahSalesPercent** | Rank-1 wilayah MTD omzet ÷ Sales `TotalOmzet` × 100 |
| **InactiveWarehouseWithStockCount** | Count distinct warehouses where `IsAktif = false` AND inventory value > 0 (exclude In-Transit) |

No `RequiresAttention` threshold engine on concentration cards — display values only (M17/M21 pattern).

#### Concentration attention signals (Q16)

| Signal | Entity | Inclusion rule |
| --- | --- | --- |
| **WarehouseInventoryConcentration** | Warehouse | Warehouse in **Top 10** by inventory value (ranking universe) |
| **WarehouseAtRiskConcentration** | Warehouse | Warehouse in **Top 10** by at-risk value |
| **WarehouseSalesConcentration** | Warehouse | Warehouse in **Top 10** by MTD omzet |
| **WarehousePurchasingConcentration** | Warehouse | Warehouse in **Top 10** by MTD purchase |
| **WarehouseNoSalesWithInventory** | Warehouse | Ranking-eligible; inventory value > 0; MTD omzet = 0 |
| **WarehouseInactiveWithStock** | Warehouse | `IsAktif = false`; inventory value > 0; exclude In-Transit |

**Presentation rules:**

- One row per **Warehouse × Signal**.
- Multiple signals per warehouse allowed (e.g. inventory concentration + no sales with inventory).
- Generic **Attention Indicator** on cards — no per-signal severity engine.
- **No wilayah rows** on attention list (Q15) — M20 owns wilayah collection attention.

**Attention list sort order:**

1. Signal priority: `WarehouseInactiveWithStock` → `WarehouseNoSalesWithInventory` → `WarehouseAtRiskConcentration` → `WarehouseInventoryConcentration` → `WarehouseSalesConcentration` → `WarehousePurchasingConcentration`.
2. `ValueAmount` desc (nulls last).
3. `EntityName` asc.

**M23 metadata (Q24):** Persist stable `SignalKey` strings exactly as named above; `EntityType = "Warehouse"`; `ReportRoute = "/reports/inventory"` for all warehouse attention rows.

### 2.5 Layout and drill-down

**Fixed section order (Q9):**

1. Location Attention Cards  
2. Top Warehouse by Inventory  
3. Top Warehouse by At-Risk Inventory  
4. Top Warehouse by Sales  
5. Top Warehouse by Purchasing  
6. Top Wilayah by Sales  
7. Location Attention List  
8. Navigation  

| UI element | Target |
| --- | --- |
| Top Warehouse by Inventory row | `/reports/inventory?q={WarehouseName}` (Q28) |
| Top Warehouse by At-Risk row | `/dashboard/inventory-risk` (primary) — optional secondary link to Inventory Report |
| Top Warehouse by Sales row | **No report drill-down** — Sales Report omits warehouse column (Q27) |
| Top Warehouse by Purchasing row | **No report drill-down** — link to `/dashboard/purchasing` via navigation only |
| Top Wilayah by Sales row | `/dashboard/collection` (Q29) — **not** Piutang Report |
| Attention list warehouse row | `/reports/inventory?q={WarehouseName}` |
| Navigation (Q30) | Inventory · Inventory Risk · Sales · Purchasing · Collection · Customer Analytics · Salesman Performance |

### 2.6 Data architecture

| Decision | Value |
| --- | --- |
| Snapshot domain | `BTR_PortalDashboardLocation*` |
| Refresh cadence | **60 minutes** (`LocationIntervalMinutes`) — inventory-dominant (Q26) |
| Refresh pattern | `SnapshotKey = 'CURRENT'` delete-and-replace |
| Read API | `GET /api/dashboard/locations` |
| Materialization | All derivations at refresh — **no read-time composition** |
| Cross-domain inputs at refresh | `IDashboardInventorySnapshotDal`, `IDashboardInventoryRiskSnapshotDal`, `IDashboardSalesSnapshotDal`, `IDashboardPurchasingSnapshotDal` (denominators only) |
| Source DALs at refresh | `IStokBalanceViewDal`, `IFakturViewDal`, `IInvoiceViewDal`, `IBrgLastFakturDal`, `IWarehouseDal.ListAllForPortal()` |

---

## 3. Architecture Overview

### 3.1 Target topology

```text
Source DALs (read at refresh time)
  IStokBalanceViewDal                         → inventory + at-risk warehouse rollup
  IBrgLastFakturDal                           → M19-compatible item classification
  IFakturViewDal                              → MTD omzet by warehouse + wilayah
  IInvoiceViewDal                             → MTD purchase by warehouse
  IWarehouseDal.ListAllForPortal()            → IsAktif, IsSpecial universe
    ↓
Denominator snapshots (read at refresh — NOT recomputed from source)
  IDashboardInventorySnapshotDal              → TotalInventoryValue
  IDashboardInventoryRiskSnapshotDal          → AtRiskInventoryValue
  IDashboardSalesSnapshotDal                  → TotalOmzet
  IDashboardPurchasingSnapshotDal             → GrandTotalPurchase
    ↓
RefreshDashboardLocationSnapshotWorker
    ↓ DashboardLocationAggregator
      ↳ DashboardInventoryRiskClassifier (shared at-risk BrgId set — new, M19-compatible)
BTR_PortalDashboardLocation* tables (7)
    ↓
Browser → GET /api/dashboard/locations
    ↓ MediatR GetDashboardLocationHandler
    ↓ IDashboardLocationDal
DashboardLocationResponse

Parallel unchanged paths:
  M15/M19/M20/M21 workers and tables — read-only consumption or navigation links only
```

**Why dedicated snapshot domain:**

- PO Q25 — warehouse aggregates are greenfield; live composition across stok + faktur + invoice + M19 classification is expensive.
- 60-minute cadence aligns with inventory-heavy domains (M15/M19).
- Failure isolation — Location refresh failure does not affect sibling dashboards.

**Why read denominators from sibling snapshots:**

- Guarantees `% of company total` matches what users see on Sales/Inventory/Purchasing/Inventory Risk dashboards.
- Avoids subtle drift if source queries differ slightly from snapshot aggregators.

**Why NOT read M20 for wilayah sales or overdue:**

- Wilayah MTD omzet is derived from Faktur (commercial lens) — orthogonal to M20 overdue exposure.
- M20 wilayah overdue is explicitly link-only (Q20).

### 3.2 Design principles

| Principle | Application |
| --- | --- |
| Parallel location lenses | Warehouse and Wilayah sections coexist — do not merge into single dimension |
| Reuse valuation rules | M15 In-Transit exclusion, `Hpp × Qty`; M19 classification thresholds unchanged |
| Reuse UI components | `ExecutiveAttentionCard`, `Top10RankingTable`, attention list DataTable pattern from M17–M21 |
| Preserve Desktop parity | Void exclusion, Faktur month filter, `"Unknown"` normalization |
| Fail gracefully | Missing Location snapshot → unavailable banner; navigation links still work |
| M23-ready signals | Stable `SignalKey`, Warehouse × Signal grain, persisted `ReportRoute` |
| No report API changes | Dashboard-only location columns — validate via Desktop SF1/IF1/PF1 |

### 3.3 Architecture decisions

| Decision | Choice | Rationale |
| --- | --- | --- |
| Snapshot tables | **7 new Location tables** | KPI + 5 rankings + attention — mirrors Collection multi-ranking pattern |
| Warehouse universe | Active, non-special, excl. In-Transit | Q5 + Desktop `WarehouseBrowser` |
| Inactive detection | Extend `WarehouseModel` + `ListAllForPortal()` | Q18 — `IsAktif` exists in SQL, not in model today |
| At-risk classification | Shared classifier helper; M19 constants | Q21 — exact reuse without M19 snapshot changes |
| Concentration signals | Top 10 inclusion (not auto-threshold) | Q16/Q17 — same philosophy as M21 concentration |
| Wilayah on Faktur | Customer `WilayahName` | Q6 — consistent with M17/M20 |
| Wilayah drill-down | Collection Dashboard | Q29 — receivable geography lives on M20 |
| Sales/Purchasing warehouse rows | No report drill-down | Q27 — report APIs unchanged |
| Executive integration | **Phase 2** | Q23 |
| Refresh cadence | **60 minutes** | Q26 — inventory-dominant |
| RefreshAll order | … → Collection → **Location** | After Inventory, InventoryRisk, Sales, Purchasing (denominator snapshots) |
| Worker domain name | `"Location"` | Distinct from `"Inventory"` / `"Collection"` |
| API route prefix | `api/dashboard/locations` | Matches page route plural |

---

## 4. Impact Analysis

### 4.1 Affected modules

| Layer | Module | Change type |
| --- | --- | --- |
| SQL | `BTR_PortalDashboardLocationKpi.sql` | **New** |
| SQL | `BTR_PortalDashboardLocationTopWarehouseInventory.sql` | **New** |
| SQL | `BTR_PortalDashboardLocationTopWarehouseAtRisk.sql` | **New** |
| SQL | `BTR_PortalDashboardLocationTopWarehouseSales.sql` | **New** |
| SQL | `BTR_PortalDashboardLocationTopWarehousePurchasing.sql` | **New** |
| SQL | `BTR_PortalDashboardLocationTopWilayahSales.sql` | **New** |
| SQL | `BTR_PortalDashboardLocationAttention.sql` | **New** |
| SQL | `btr.sql.sqlproj` | Include new tables |
| Domain | `WarehouseModel.cs` | Add `IsAktif` |
| Application | `IWarehouseDal` | Add `ListAllForPortal()` |
| Application | `DashboardSnapshotAgg/Services/DashboardLocationAggregator.cs` | **New** |
| Application | `DashboardSnapshotAgg/Services/DashboardLocationKeyResolver.cs` | **New** |
| Application | `DashboardSnapshotAgg/Services/DashboardInventoryRiskClassifier.cs` | **New** (shared M19-compatible classification) |
| Application | `DashboardSnapshotAgg/Models/DashboardLocationAggregateResult.cs` | **New** |
| Application | `DashboardSnapshotAgg/Contracts/IDashboardLocationSnapshotDal.cs` | **New** |
| Application | `DashboardSnapshotAgg/UseCases/RefreshDashboardLocationSnapshotWorker.cs` | **New** |
| Application | `DashboardSnapshotAgg/DashboardSnapshotOptions.cs` | Add `LocationIntervalMinutes` |
| Application | `DashboardSnapshotAgg/UseCases/RefreshAllDashboardSnapshotsWorker.cs` | Register Location worker |
| Application | `DashboardSnapshotAgg/Commands/RefreshDashboardSnapshotsCommand.cs` | Add domain |
| Application | `DashboardLocationAgg/Queries/GetDashboardLocationQuery.cs` | **New** |
| Application | `DashboardLocationAgg/Contracts/IDashboardLocationDal.cs` | **New** |
| Infrastructure | `WarehouseDal.cs` | `ListAllForPortal()` SELECT `IsAktif`, `IsSpecial` |
| Infrastructure | `DashboardLocationSnapshotDal.cs` | **New** |
| Infrastructure | `DashboardLocationDal.cs` | **New** |
| API | `LocationDashboardController.cs` | **New** |
| API | `HealthController.cs` | Add `Location` domain |
| API | DI registrations | New DALs, worker, aggregator |
| Worker | `btr.portal.worker/Program.cs` | Add `Location` to `--domain` |
| Frontend | `LocationDashboardView.vue` | **New** |
| Frontend | `components/dashboard/Location*.vue` | **New** section components |
| Frontend | `router/index.ts`, `MainLayout.vue` | Route + sidebar |
| Frontend | `models/dashboard.ts`, `api/dashboardApi.ts`, `stores/dashboardStore.ts` | Location types + load |
| Tests | `btr.test/ReportingContext/` | Aggregator + classifier + key resolver tests |
| Docs | Post-delivery feature docs | Operational, domain, materialized-dashboard updates |

### 4.2 Unaffected modules

| Module | Reason |
| --- | --- |
| M15/M19 aggregators and snapshot tables | Read-only denominator consumption; classification logic shared via new helper only |
| M20 Collection aggregator / tables | Link-only; no wilayah overdue duplication |
| M21 Purchasing Management | Link-only; no warehouse backlog |
| M16 Executive composer | Phase 2 (Q23) |
| Sales Report / Piutang Report DALs and APIs | Q27 — no column extensions |
| BTR Desktop warehouse forms / IF8 | No write-path changes |
| Customer / Salesman dashboards | Boundary preserved — entity × signal vs location × signal |

### 4.3 Metric traceability

| Dashboard field | Source at refresh | Validating reference |
| --- | --- | --- |
| Top 10 Warehouse Inventory + % | StokBalanceView rollup | Inventory Report grouped by Warehouse column |
| Top 10 Warehouse At-Risk + % | M19-classified BrgIds × stok rows | Inventory Risk total ≥ sum warehouse at-risk |
| Top 10 Warehouse Sales + % | FakturView month | Desktop SF1 grouped by Warehouse |
| Top 10 Wilayah Sales + % | FakturView month | Desktop SF1 grouped by Wilayah |
| Top 10 Warehouse Purchasing + % | InvoiceView month | Purchasing Report grouped by Warehouse |
| Concentration card % | Location KPI ÷ sibling snapshot denominators | Manual Top N ÷ domain dashboard total |
| Inactive Warehouse With Stock Count | Warehouse master + stok rollup | IM2 inactive warehouses with IF1 stock |
| Attention signals | Section 2.4 rules | Spot-check Top 10 membership + inactive flag |

**Reconciliation notes:**

- Sum of Top Warehouse Sales amounts across **all** warehouses (not only Top 10) must equal Sales snapshot `TotalOmzet`.
- Sum of all warehouse inventory values (excl. In-Transit) must equal M15 `TotalInventoryValue` when same rules applied.
- Sum warehouse at-risk values ≤ M19 `AtRiskInventoryValue` — strict equality when every at-risk item row allocates to exactly one warehouse bucket.

---

## 5. Database Design

Deploy all tables with `SnapshotKey = 'CURRENT'` delete-and-replace pattern.

### 5.1 `BTR_PortalDashboardLocationKpi`

Single row per refresh — attention cards.

| Column | Type | Description |
| --- | --- | --- |
| SnapshotKey | VARCHAR(10) PK | `'CURRENT'` |
| GeneratedAt | DATETIME | Refresh timestamp |
| PeriodYear | INT | Current month year |
| PeriodMonth | INT | Current month |
| Top1WarehouseInventoryPercent | DECIMAL(9,4) NULL | Informational |
| Top3WarehouseInventoryPercent | DECIMAL(9,4) NULL | Informational |
| Top1WarehouseAtRiskPercent | DECIMAL(9,4) NULL | Informational |
| Top1WarehouseSalesPercent | DECIMAL(9,4) NULL | Informational |
| Top1WilayahSalesPercent | DECIMAL(9,4) NULL | Informational |
| InactiveWarehouseWithStockCount | INT | Count inactive warehouses holding stock |
| WarehouseNoSalesWithInventoryCount | INT | Supporting count for cards |
| TotalInventoryValue | DECIMAL(18,2) | Denominator copy from M15 snapshot |
| TotalAtRiskValue | DECIMAL(18,2) | Denominator copy from M19 snapshot |
| TotalOmzet | DECIMAL(18,2) | Denominator copy from Sales snapshot |
| TotalPurchase | DECIMAL(18,2) | Denominator copy from Purchasing snapshot |
| LastRefreshLogId | VARCHAR(13) | FK to refresh log |

### 5.2 `BTR_PortalDashboardLocationTopWarehouseInventory`

| Column | Type | Description |
| --- | --- | --- |
| LocationTopWarehouseInventoryId | VARCHAR(13) PK | Generated ID |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| Rank | INT | 1–10 |
| WarehouseId | VARCHAR(5) | |
| WarehouseName | VARCHAR(50) | Display + drill-down |
| InventoryValue | DECIMAL(18,2) | |
| PercentOfTotal | DECIMAL(9,4) NULL | ÷ M15 total × 100 |
| ReportRoute | VARCHAR(100) | `/reports/inventory` |

Unique: `(SnapshotKey, Rank)`

### 5.3 `BTR_PortalDashboardLocationTopWarehouseAtRisk`

Same shape as 5.2 with `AtRiskValue` replacing `InventoryValue`; `PercentOfTotal` ÷ M19 at-risk total.

### 5.4 `BTR_PortalDashboardLocationTopWarehouseSales`

| Column | Type | Description |
| --- | --- | --- |
| … | | Same key/rank/warehouse columns |
| MtdOmzet | DECIMAL(18,2) | |
| PercentOfTotal | DECIMAL(9,4) NULL | ÷ Sales total omzet × 100 |
| ReportRoute | VARCHAR(100) NULL | **Null in V1** — no Sales Report warehouse filter |

### 5.5 `BTR_PortalDashboardLocationTopWarehousePurchasing`

Same as 5.4 with `MtdPurchaseAmount`; `ReportRoute` null in V1.

### 5.6 `BTR_PortalDashboardLocationTopWilayahSales`

| Column | Type | Description |
| --- | --- | --- |
| LocationTopWilayahSalesId | VARCHAR(13) PK | |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| Rank | INT | 1–10 |
| WilayahId | VARCHAR(5) NULL | When available from join |
| WilayahName | VARCHAR(50) | |
| MtdOmzet | DECIMAL(18,2) | |
| PercentOfTotal | DECIMAL(9,4) NULL | ÷ Sales total omzet × 100 |
| DashboardRoute | VARCHAR(100) | `/dashboard/collection` |

Unique: `(SnapshotKey, Rank)`

### 5.7 `BTR_PortalDashboardLocationAttention`

Warehouse × Signal list.

| Column | Type | Description |
| --- | --- | --- |
| LocationAttentionId | VARCHAR(13) PK | Generated ID |
| SnapshotKey | VARCHAR(10) | `'CURRENT'` |
| EntityType | VARCHAR(20) | `Warehouse` |
| EntityCode | VARCHAR(5) NULL | `WarehouseId` |
| EntityName | VARCHAR(50) | `WarehouseName` |
| SignalKey | VARCHAR(40) | Section 2.4 signal keys |
| SignalLabel | VARCHAR(50) | Display label |
| ValueAmount | DECIMAL(18,2) NULL | Primary monetary context |
| ValueText | VARCHAR(100) NULL | Secondary context (%, counts) |
| ReportRoute | VARCHAR(100) NULL | `/reports/inventory` |
| SortOrder | INT | Stable list ordering |

Index: `(SnapshotKey, SortOrder)`

---

## 6. Backend Implementation

### 6.1 New folder structure

```text
btr.application/ReportingContext/
├── DashboardSnapshotAgg/
│   ├── Services/
│   │   ├── DashboardLocationAggregator.cs
│   │   ├── DashboardLocationKeyResolver.cs
│   │   └── DashboardInventoryRiskClassifier.cs      (shared M19-compatible)
│   ├── Models/
│   │   └── DashboardLocationAggregateResult.cs
│   ├── Contracts/
│   │   └── IDashboardLocationSnapshotDal.cs
│   └── UseCases/
│       ├── RefreshDashboardLocationSnapshotWorker.cs
│       └── RefreshDashboardLocationSnapshotRequest.cs (+ Result)
└── DashboardLocationAgg/
    ├── Contracts/
    │   └── IDashboardLocationDal.cs
    └── Queries/
        └── GetDashboardLocationQuery.cs               (+ Handler, Response DTOs)

btr.infrastructure/ReportingContext/
├── DashboardSnapshotAgg/
│   └── DashboardLocationSnapshotDal.cs
└── DashboardLocationAgg/
    └── DashboardLocationDal.cs

btr.application/InventoryContext/WarehouseAgg/
├── IWarehouseDal.cs                                   (extend)
btr.domain/InventoryContext/WarehouseAgg/
├── WarehouseModel.cs                                  (add IsAktif)
btr.infrastructure/InventoryContext/WarehouseAgg/
├── WarehouseDal.cs                                    (ListAllForPortal)

btr.portal.api/Controllers/Dashboard/
└── LocationDashboardController.cs
```

### 6.2 `DashboardLocationAggregator` responsibilities

1. Load warehouse master via `ListAllForPortal()` — build eligibility map.
2. Roll up inventory and at-risk values by warehouse (Section 2.4).
3. Roll up MTD Faktur omzet by warehouse and wilayah.
4. Roll up MTD Invoice purchase by warehouse.
5. Read denominator KPIs from four sibling snapshot DALs.
6. Build Top 10 rankings (ties: amount desc, name asc — same as M17/M20).
7. Compute concentration KPI row.
8. Emit attention list rows per signal inclusion rules.
9. Return `DashboardLocationAggregateResult` with all collections + `GeneratedAt`.

**Public constants** (mirror M20/M21 pattern):

```csharp
public const int TopRankingCount = 10;
public const string EntityTypeWarehouse = "Warehouse";
public const string SignalWarehouseInventoryConcentration = "WarehouseInventoryConcentration";
// ... remaining signal keys
public const string InTransitWarehouseName = "In-Transit"; // reuse M15 constant
```

### 6.3 `RefreshDashboardLocationSnapshotWorker`

Follow `RefreshDashboardCollectionSnapshotWorker` pattern:

1. Insert refresh log `Running`.
2. Resolve `today`, `currentMonthPeriode`, `generatedAt` via `ITglJamDal`.
3. Load source rows + warehouse master + sibling snapshot KPI rows.
4. Call `_aggregator.Aggregate(...)`.
5. `_snapshotDal.ReplaceCurrent(aggregate, refreshLogId)`.
6. Complete refresh log Success/Failed.

**Graceful degradation:** If M15/M19/Sales/Purchasing snapshot missing, compute denominators from source aggregators in-worker as fallback **or** persist null `PercentOfTotal` and log warning — prefer **fallback compute** so first deploy after SQL migration still shows percentages.

### 6.4 `GetDashboardLocationQuery` response shape

```csharp
public class DashboardLocationResponse
{
    public bool IsAvailable { get; set; }
    public bool IsDataFresh { get; set; }
    public DateTime? GeneratedAt { get; set; }
    public DashboardLocationAttentionCards AttentionCards { get; set; }
    public IList<DashboardLocationRankingRow> TopWarehouseInventory { get; set; }
    public IList<DashboardLocationRankingRow> TopWarehouseAtRisk { get; set; }
    public IList<DashboardLocationRankingRow> TopWarehouseSales { get; set; }
    public IList<DashboardLocationRankingRow> TopWarehousePurchasing { get; set; }
    public IList<DashboardLocationWilayahRankingRow> TopWilayahSales { get; set; }
    public IList<DashboardLocationAttentionItem> AttentionList { get; set; }
    public DashboardLocationNavigationLinks Navigation { get; set; }
}
```

`IsDataFresh`: `GeneratedAt` within `LocationIntervalMinutes` (default 60).

### 6.5 DI and registration checklist

| Registration | Project |
| --- | --- |
| `DashboardLocationAggregator` | application |
| `DashboardInventoryRiskClassifier` | application |
| `IDashboardLocationSnapshotDal` → `DashboardLocationSnapshotDal` | infrastructure |
| `IDashboardLocationDal` → `DashboardLocationDal` | infrastructure |
| `IRefreshDashboardLocationSnapshotWorker` | application |
| MediatR handler for `GetDashboardLocationQuery` | application |
| `LocationDashboardController` | portal.api |
| Worker CLI domain `Location` | portal.worker |

### 6.6 `RefreshAllDashboardSnapshotsWorker` order

Insert **after Collection** (last domain):

```text
Piutang → Inventory → InventoryRisk → Sales → Purchasing → PurchasingManagement
  → Customer → Salesman → Collection → Location
```

Location depends on Inventory, InventoryRisk, Sales, and Purchasing snapshots for denominators.

### 6.7 `DashboardInventoryRiskClassifier` (minimal shared extraction)

```csharp
// Pseudocode — implementer extracts from DashboardInventoryRiskAggregator.ClassifyItem
public static HashSet<string> BuildAtRiskBrgIdSet(
    IEnumerable<DashboardInventoryItemGroup> itemGroups,
    IEnumerable<BrgLastFakturDto> lastFakturRows,
    DateTime today)
```

Optional follow-up: refactor `DashboardInventoryRiskAggregator` to call this helper internally — **must not** change M19 snapshot output (run existing M19 unit tests after refactor).

---

## 7. Frontend Implementation

### 7.1 Route and navigation

| Item | Value |
| --- | --- |
| Route | `dashboard/locations` → `LocationDashboardView.vue` |
| Sidebar | Add **Locations** after **Purchasing** in `MainLayout.vue` |
| Store action | `dashboardStore.loadLocation()` |
| API | `GET /api/dashboard/locations` |

### 7.2 New components

| Component | Purpose |
| --- | --- |
| `LocationAttentionCardGroup.vue` | Six informational concentration cards (Section 13.2 wireframe) |
| `LocationAttentionList.vue` | Warehouse × Signal DataTable |
| `LocationNavigationSection.vue` | Seven mandatory cross-dashboard links (Q30) |
| Reuse `Top10RankingTable.vue` | Five ranking sections with `% of Total` column |

### 7.3 `LocationDashboardView.vue` layout

Fixed section order matching Section 2.5. Use `DashboardDetailLayout` (same as Collection).

**Subtitle suggestion:** *Location concentration across warehouses (inventory, sales, purchasing) and territories (sales contribution). Receivable risk by wilayah — see Collection Dashboard.*

### 7.4 Row click behavior

```typescript
// Warehouse inventory + attention list
navigateToReport(router, '/reports/inventory', warehouseName)

// Warehouse at-risk — navigate to dashboard (no report warehouse filter on risk view)
router.push('/dashboard/inventory-risk')

// Wilayah sales
router.push('/dashboard/collection')
```

### 7.5 Cross-links (mandatory — Q30)

| Label | Route |
| --- | --- |
| Inventory Dashboard | `/dashboard/inventory` |
| Inventory Risk Dashboard | `/dashboard/inventory-risk` |
| Sales Dashboard | `/dashboard/sales` |
| Purchasing Dashboard | `/dashboard/purchasing` |
| Collection Dashboard | `/dashboard/collection` |
| Customer Analytics | `/dashboard/customers` |
| Salesman Performance | `/dashboard/salesmen` |

---

## 8. Phase 2 — Executive Signal Promotion

Execute **after** M22 Location dashboard is shipped and validated. Separate PR (Q23).

| Executive candidate | Location snapshot source |
| --- | --- |
| Top 1 Warehouse Inventory % | `Top1WarehouseInventoryPercent` |
| Top 1 Warehouse Sales % | `Top1WarehouseSalesPercent` |
| Inactive Warehouse With Stock Count | `InactiveWarehouseWithStockCount` |

**Implementation sketch:**

1. Extend `DashboardExecutiveDal` to read Location KPI snapshot.
2. Add optional location concentration cards to Management Attention Center.
3. Do **not** embed full Top 10 tables on executive home.

---

## 9. Testing

### 9.1 Unit tests — `DashboardLocationAggregatorTest`

| Case | Assertion |
| --- | --- |
| In-Transit exclusion | In-Transit rows excluded from warehouse inventory rollup |
| IsSpecial exclusion | Special warehouse excluded from Top 10 rankings |
| Inactive warehouse in rankings | `IsAktif = false` excluded from Top 10 inventory ranking |
| WarehouseInactiveWithStock | Inactive warehouse with stock → attention row |
| WarehouseNoSalesWithInventory | Stock > 0, zero MTD Faktur → signal |
| WarehouseInventoryConcentration | Top 10 inventory warehouse emits signal |
| WarehouseAtRiskConcentration | At-risk BrgId allocated to warehouse correctly |
| M19 classification parity | At-risk BrgId set matches `DashboardInventoryRiskAggregator` fixture |
| Sum warehouse omzet = total omzet | Reconciliation |
| Sum warehouse purchase = grand total | Reconciliation |
| Sum warehouse inventory = M15 total | Reconciliation (same rules) |
| Wilayah omzet | Groups by customer `WilayahName` |
| Unknown wilayah | Blank → `"Unknown"` |
| Top 1 / Top 3 % KPI | Correct arithmetic |
| Attention sort order | Inactive before concentration signals |
| One row per warehouse × signal | No duplicates |
| Missing purchasing snapshot | Fallback or null % — page still loads rankings |

### 9.2 Unit tests — `DashboardLocationKeyResolverTest`

Verify warehouse name trim, In-Transit detection, eligibility checks.

### 9.3 Unit tests — `DashboardInventoryRiskClassifierTest`

Cross-check at-risk BrgIds against existing `DashboardInventoryRiskAggregatorTest` fixtures.

### 9.4 Integration / manual test checklist

1. Deploy seven SQL tables; run Location refresh; verify all tables populated.
2. `GET /api/dashboard/locations` returns full response.
3. Page title **Branch / Warehouse Performance Dashboard**; sections in fixed order.
4. Sum warehouse inventory matches Inventory Report footer (excl. In-Transit).
5. Sum warehouse MTD sales matches Sales Dashboard `TotalOmzet`.
6. Sum warehouse MTD purchase matches Purchasing Dashboard `GrandTotalPurchase`.
7. Warehouse at-risk total ≤ Inventory Risk `AtRiskInventoryValue`.
8. Click inventory ranking row → Inventory Report with `?q=` pre-filled.
9. Click wilayah sales row → Collection Dashboard (not Piutang Report).
10. Navigation links reach all seven sibling dashboards.
11. Inactive warehouse with stock appears on attention list but not in Top 10 rankings.
12. **"⚠ Dashboard Data Not Fresh"** when snapshot exceeds 60-minute interval.
13. `GET /api/health/dashboard-snapshots` includes `Location`.
14. Manual refresh: `POST /api/admin/dashboard/refresh` with `{ "Domain": "Location" }`.
15. Worker CLI: `btr.portal.worker --domain Location --triggered-by Manual`.
16. RefreshAll order: Location runs after Collection.
17. Desktop SF1 spot-check: warehouse and wilayah MTD omzet grouping.

---

## 10. Risk Analysis

| Risk | Likelihood | Impact | Mitigation |
| --- | --- | --- | --- |
| `IsAktif` not loaded breaks inactive signal | Medium | Medium | `ListAllForPortal()` explicit SELECT; unit test inactive case |
| At-risk classification drift from M19 | Medium | High | Shared `DashboardInventoryRiskClassifier` + cross-test with M19 fixtures |
| Denominator snapshot stale vs Location refresh | Medium | Low | Accept point-in-time difference; show single `GeneratedAt` on page |
| Warehouse name mismatch vs Inventory Report `?q=` | Medium | Medium | Use same trimmed `WarehouseName` as report search expects — verify against `InventoryReportDal` filter |
| First deploy — sibling snapshots empty | Medium | Low | Fallback denominator computation in worker; unavailable banner if all missing |
| Special warehouse skew if filter wrong | Low | Medium | Unit test `IsSpecial` exclusion; align with Desktop browser |
| Performance — full stok + faktur + invoice scan | Medium | Medium | 60-minute cadence; same cost class as M15/M19 refresh |
| Scope creep (movement KPIs, piutang by warehouse) | Medium | Medium | Enforce Section 16 exclusions in PR review |
| Wilayah vs warehouse confusion in UI | Medium | Low | Dual sections with clear subtitles; navigation to Collection for receivable geography |

---

## 11. Documentation Updates (post-delivery)

| Document | Update |
| --- | --- |
| `docs/features/btr-portal/btr-portal-operational.md` | Route, title, signals, drill-down, refresh cadence, sibling links |
| `docs/features/btr-portal/btr-portal-domain.md` | Location concentration KPI definitions, signal catalog, warehouse vs wilayah semantics |
| `docs/features/btr-portal/btr-portal-architecture.md` | Location snapshot domain topology |
| `docs/features/materialized-dashboard/materialized-dashboard-domain.md` | Location domain, tenth refresh job, 60-minute cadence |
| `docs/work/btr-portal/M22 Branch Warehouse Performance - Analysis.md` | Link to this plan |

---

## 12. Implementation Steps

Execute in order. Each phase should compile before proceeding.

### Phase 1 — Database

1. Create seven `BTR_PortalDashboardLocation*.sql` table scripts (Section 5).
2. Add to `btr.sql.sqlproj`; deploy to dev database.

### Phase 2 — Warehouse master extension

3. Add `IsAktif` to `WarehouseModel`.
4. Add `IWarehouseDal.ListAllForPortal()` + `WarehouseDal` implementation (SELECT all warehouses with `IsAktif`, `IsSpecial`).
5. Confirm Desktop `ListData()` behavior unchanged.

### Phase 3 — Backend core

6. Add `DashboardInventoryRiskClassifier` with unit tests (Section 9.3).
7. Add `DashboardLocationKeyResolver`.
8. Add `DashboardLocationAggregateResult` and `DashboardLocationAggregator` with unit tests (Section 9.1).
9. Add `IDashboardLocationSnapshotDal` + `DashboardLocationSnapshotDal.ReplaceCurrent`.
10. Add `RefreshDashboardLocationSnapshotWorker` + request/result types.
11. Add `LocationIntervalMinutes` (default **60**) to `DashboardSnapshotOptions` and `appsettings.json`.

### Phase 4 — Backend integration

12. Register Location worker in `RefreshAllDashboardSnapshotsWorker` (after Collection).
13. Add domain case to `RefreshDashboardSnapshotsCommand` / admin refresh handler.
14. Add `DashboardLocationAgg` query, handler, DAL, and `LocationDashboardController`.
15. Wire DI in API and worker projects; update `HealthController`.
16. Update `btr.portal.worker/Program.cs` ValidDomains.

### Phase 5 — Frontend

17. Extend TypeScript models and `dashboardApi.ts` / `dashboardStore.ts`.
18. Build `LocationAttentionCardGroup`, `LocationAttentionList`, `LocationNavigationSection`.
19. Create `LocationDashboardView.vue` with fixed section order (Section 2.5).
20. Add route and sidebar entry in `router/index.ts` and `MainLayout.vue`.

### Phase 6 — Verification

21. Run unit tests including M19 classification parity tests.
22. Execute manual checklist (Section 9.4).
23. Validate inventory, sales, and purchasing reconciliation against sibling dashboards and Desktop reports.
24. Update feature documentation (Section 11).

### Phase 7 — Executive promotion (separate delivery)

25. Implement Section 8 after M22 dashboard validated in production.

---

## Appendix A — Signal Labels (display)

| SignalKey | SignalLabel |
| --- | --- |
| WarehouseInventoryConcentration | Inventory Concentration |
| WarehouseAtRiskConcentration | At-Risk Concentration |
| WarehouseSalesConcentration | Sales Concentration |
| WarehousePurchasingConcentration | Purchasing Concentration |
| WarehouseNoSalesWithInventory | Stock Without Sales |
| WarehouseInactiveWithStock | Inactive With Stock |

---

## Appendix B — Milestone Boundary Summary

| Question | Owner |
| --- | --- |
| Which items are at risk? | M19 Inventory Risk |
| Which wilayah has overdue receivables? | M20 Collection (link) |
| Which principals need purchasing attention? | M21 Purchasing (link) |
| Where is capital / billing / intake concentrated? | **M22 Location** |

---

*End of implementation plan — M22 Branch / Warehouse Performance Dashboard (Location Concentration Dashboard)*
