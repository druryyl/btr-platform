# Materialized Dashboard Data — Domain

**Audience:** Product Owners, Analysts, Future Agents  
**Purpose:** Explain why dashboard data is materialized and the authoritative business rules that govern it.

**Related docs:** [Architecture (WHAT)](./materialized-dashboard-architecture.md) · [Operational (HOW)](./materialized-dashboard-operational.md) · [BTR Portal domain](../btr-portal/btr-portal-domain.md)

For portal navigation, report definitions, and full KPI catalog, see [btr-portal-domain.md](../btr-portal/btr-portal-domain.md). This document covers **materialization-specific** product decisions and semantic rules.

---

## Business Problem

| Symptom | Impact |
| ------- | ------ |
| Slow dashboard load (especially Piutang) | Executives abandon the portal; decisions delayed |
| Server CPU spikes on each page refresh | Portal users compete with BTR Desktop for database resources |
| Full-history Piutang scan (2000 → today) | Unnecessary work — only **open balances** matter for analytics |
| Sales dashboard tied to `BTR_SalesOmzet` | Board metrics conflated with salesperson fee / RO2 incentive logic |

**Objective:** Give Board and stakeholders **fast, reliable management analytics** without overloading the database during concurrent portal sessions.

---

## Scope

### In scope

| Item | Included |
| ---- | -------- |
| Materialized data for `/dashboard/sales`, `/dashboard/piutang`, `/dashboard/inventory`, `/dashboard/purchasing` | Yes |
| Dashboard home (`/dashboard`) reading Layer A KPI snapshots | Yes — Option B confirmed |
| Background refresh via `btr.portal.worker` | Yes |
| Sales analytics from **`BTR_Faktur`** | Yes |
| Preserve KPI definitions and dashboard–report traceability | Yes |
| Display snapshot freshness (`GeneratedAt`) | Yes |
| On-demand refresh via portal API | Yes |
| Refresh health observability | Yes |

### Out of scope

| Item | Excluded |
| ---- | -------- |
| Overview as primary design driver before analytical cutover | N/A — both now use snapshots |
| Changes to `BTR_SalesOmzet` / RO2 workflows | Unchanged |
| Report pages (`/reports/*`) | Live queries — separate from dashboards |
| Custom date filters, drilldown, export | Deferred (future milestone) |
| Real-time sub-minute freshness | Not required for board cadence |
| Historical snapshot retention | `CURRENT` row only |

---

## Product Decisions (Authoritative)

| Decision | Choice | Consequence |
| -------- | ------ | ----------- |
| Overview home data source | Layer A KPI snapshots via `GET /api/dashboard/overview` | Fast home page; per-domain `GeneratedAt` may differ |
| Sales KPI source | **Faktur only** — `SUM(GrandTotal)` for non-void current-month Fakturs | Aligns with Sales Report; pipeline excluded |
| Refresh cadence | Piutang **15 min**, Sales **30 min**, Purchasing **30 min**, Customer **30 min**, Salesman **30 min**, Collection **30 min**, Location **60 min**, Inventory **60 min**, InventoryRisk **60 min** | Nine Task Scheduler jobs |
| Snapshot history | **`CURRENT` row only** | Delete-and-replace each refresh |
| Manual refresh | Portal API + worker CLI | BTR Desktop trigger deferred |
| Live aggregation fallback | **Removed** after Phase 4 cutover | Dashboards require populated snapshots |

---

## Stakeholder Communication — Sales Semantic Shift

The Sales analytical dashboard no longer uses `BTR_SalesOmzet` omzet recognition semantics.

| Aspect | Old (`BTR_SalesOmzet`) | New (`BTR_Faktur`) |
| ------ | ---------------------- | ------------------- |
| Revenue basis | Omzet recognition status (Completed / Pending / Outstanding) | Invoiced `GrandTotal` |
| Pipeline | Included in data model | **Excluded** — board view is billed sales |
| Alignment with Sales Report | Indirect | **Direct** — same Faktur source |
| UI labels | "Omzet period" | "Invoiced sales (Faktur)" |

`BTR_SalesOmzet` continues unchanged for Desktop RO2 and salesperson fee workflows.

---

## Freshness Model

Dashboard numbers are **point-in-time snapshots**, not live operational balances.

| Concept | Definition |
| ------- | ---------- |
| `GeneratedAt` | When the background worker last successfully rebuilt that domain's snapshot |
| Maximum staleness | One refresh interval (15 / 30 / 60 min per domain; Purchasing, Customer, Salesman 30 min; Inventory, InventoryRisk 60 min) |
| User Refresh button | Re-reads stored snapshot from API — does **not** trigger recalculation |
| Manual rebuild | Administrator triggers worker or `POST /api/admin/dashboard/refresh` |

**Trust signal:** Every dashboard page shows generated-at time. Home cards may show different timestamps when domains refresh on different schedules.

---

## KPI Rules Preserved by Materialization

Materialization must not change KPI meaning. Aggregation rules match pre-materialization dashboards (except Sales source change).

### Piutang

| Rule | Definition |
| ---- | ---------- |
| Outstanding filter | `KurangBayar > 1` (equivalent to `BTR_Piutang.Sisa > 1` on refresh path) |
| Customer key | `CustomerCode` trimmed; fallback to `CustomerName` |
| Aging buckets | 5 inclusive buckets from `JatuhTempo` vs refresh date |
| Overdue customers | Customers with any non-Current bucket exposure |
| Top 10 | By outstanding balance descending; name ascending tie-break |
| **Period semantics** | Current open receivables snapshot — not a historical time series |

**Traceability:** Total Piutang and Total Customer = Piutang Report footer totals.

### Inventory

| Rule | Definition |
| ---- | ---------- |
| BrgId-first grouping | Group stock rows by item, sum qty across warehouses |
| In-Transit exclusion | Exclude `WarehouseName = "In-Transit"` |
| Zero-qty exclusion | Only items with aggregated `Qty > 0` |
| Unknown dimensions | Blank category/supplier → `"Unknown"` |
| Inventory value | `Sum(Hpp × Qty)` per item group |
| **Period semantics** | Point-in-time snapshot at refresh |

**Traceability:** Total Inventory Value and Total Item = Inventory Report footer totals.

### Sales (Faktur-based)

| KPI | Definition |
| --- | ---------- |
| **Total Omzet** / **Completed Omzet** / **Total Achievement** | `SUM(GrandTotal)` of non-void Fakturs in current calendar month |
| **Pipeline Omzet** | Always `0` |
| **Total Faktur** | Count of those Fakturs |
| **Total Customer** | Distinct `CustomerCode` (fallback customer name) |
| **Total Target** | Sum of `BTR_SalesOmzetTarget` rows for current month |
| **Achievement %** | Total Achievement ÷ Total Target (blank when no targets) |
| **Weekly trend** | `GrandTotal` grouped by calendar week within month |
| **Top 10 Salesman** | Top 10 by `SUM(GrandTotal)` per `SalesPersonName` |

Void exclusion: `VoidDate = '3000-01-01'` (handled by `FakturViewDal`).

**Traceability:** Total Omzet / Total Achievement = sum of Sales Report `GrandTotal` for the same month.

### Purchasing (Invoice-based)

| KPI | Definition |
| --- | ---------- |
| **Grand Total Purchase** | `SUM(GrandTotal)` of non-void purchase Invoices in current calendar month |
| **Total Invoice** | Count of those invoices |
| **Pending Posting Invoice Count** | Count where `PostingStok = BELUM` |
| **Weekly trend** | `GrandTotal` grouped by calendar week within month |
| **Posting status breakdown** | `GrandTotal` by `SUDAH` / `BELUM` |
| **Top 10 Principal** | Top 10 by `SUM(GrandTotal)` per trimmed `SupplierName` |

Void exclusion: `VoidDate = '3000-01-01'` (handled by `InvoiceViewDal`).

**Traceability:** Grand Total Purchase and Total Invoice = Purchasing Report footer totals.

**Snapshot tables:** `BTR_PortalDashboardPurchasingKpi`, `BTR_PortalDashboardPurchasingWeekTrend`, `BTR_PortalDashboardPurchasingPostingStatus`, `BTR_PortalDashboardPurchasingTopPrincipal`.

### PurchasingManagement (cross-domain — M21)

Dedicated management snapshot domain — extends V1 purchasing statistics with attention KPIs. Reads `IInvoiceViewDal` (with `CreateTime`/`LastUpdate`), V1 purchasing snapshot, M15 inventory snapshot, and M19 inventory-risk snapshot at refresh.

| KPI | Definition |
| --- | ---------- |
| **Qualified Backlog Count/Value** | Age-qualified `BELUM` invoices (`LastUpdate` ≥ 3 days) |
| **Attention list** | Principal × Signal (8 approved signals) |
| **Top 10 Principal %** | MTD purchase amount ÷ Grand Total Purchase |
| **Compound Dependency** | Purchase Top 10 AND (Inventory Top 10 OR At-Risk Top 10) |
| **Principal Inventory No Purchase** | M15 supplier Top 10 with zero MTD purchase |

**Refresh cadence:** 30 minutes (`PurchasingManagementIntervalMinutes`).

**Refresh order in `All`:** … → Purchasing (V1) → **PurchasingManagement** → Customer → …

**Snapshot tables:** `BTR_PortalDashboardPurchasingManagementKpi`, `BTR_PortalDashboardPurchasingManagementAttention`, `BTR_PortalDashboardPurchasingManagementTopPrincipal`.

### Customer (cross-domain — M17)

Reads **source DALs** at refresh (Faktur, piutang open balance, customer master, last Faktur per customer) — not Sales/Piutang snapshot tables.

| KPI | Definition |
| --- | ---------- |
| **Attention cards** | Overdue count, >90d exposure, concentration %, active/dormant counts, plafond breach, suspended+sales |
| **Attention list** | Per customer × signal: Overdue, Dormant (90-day), Plafond breach, Suspended + Sales |
| **Top 10 Omzet** | Current-month `SUM(GrandTotal)` per customer with `CustomerCode` |
| **Top 10 Piutang** | All-time open `SUM(KurangBayar)` per customer with `CustomerCode` |
| **Segmentation** | Counts by Klasifikasi, Wilayah, Active vs Dormant |

**Refresh order in `All`:** Piutang → Inventory → Sales → Purchasing → Customer → **Salesman** (last).

**Snapshot tables:** `BTR_PortalDashboardCustomerKpi`, `BTR_PortalDashboardCustomerTopOmzet`, `BTR_PortalDashboardCustomerTopPiutang`, `BTR_PortalDashboardCustomerAttention`, `BTR_PortalDashboardCustomerSegmentation`.

### Salesman (cross-domain — M18)

Reads **source DALs** at refresh (Faktur with `SalesPersonId`, piutang open balance with invoicing salesman via FF1 join, salesman master, per-rep targets, last Faktur per customer with salesman) — not Sales/Piutang/Customer snapshot tables.

| KPI | Definition |
| --- | ---------- |
| **Attention cards** | Below Target count, No Target count, High Overdue count, High Piutang count, Dormant Portfolio count, concentration % |
| **Attention list** | Per salesman × signal: BelowTarget, NoTarget, HighOverdueExposure, HighPiutangExposure, CustomerConcentration, DormantCustomerPortfolio |
| **Top 10 Omzet** | Current-month `SUM(GrandTotal)` per `SalesPersonId` with `SalesPersonCode` |
| **Top 10 Achievement** | Achievement % per rep (omzet ÷ target); reps with no target excluded from ranking |
| **Top 10 Piutang** | All-time open `SUM(KurangBayar)` per invoicing salesman with `SalesPersonCode` |
| **Segmentation** | Counts by Wilayah, Segment, Active vs Inactive (current-month Faktur) |

**Salesman key:** `SalesPersonId` primary; `SalesPersonName` display; `SalesPersonCode` on ranking rows. Piutang rows without `SalesPersonId` resolved via name fallback map.

**Attribution rules:** Sales omzet from `Faktur.SalesPersonId`; piutang from invoicing salesman on open Faktur; dormant customers attributed to last-invoicing salesman (90-day rule, same as Customer Analytics).

**Achievement bands:** M16 thresholds — ≥100% Healthy · 80–99% Warning · <80% Critical · no target Unknown.

**Snapshot tables:** `BTR_PortalDashboardSalesmanKpi`, `BTR_PortalDashboardSalesmanTopOmzet`, `BTR_PortalDashboardSalesmanTopAchievement`, `BTR_PortalDashboardSalesmanTopPiutang`, `BTR_PortalDashboardSalesmanAttention`, `BTR_PortalDashboardSalesmanSegmentation`.

**Protected modules unchanged:** `DashboardSalesFakturAggregator`, `BTR_PortalDashboardSalesTopSalesman`, `DashboardExecutiveComposer`.

### Collection (cross-domain — M20)

Reads **source DALs** at refresh — open piutang (customer, with-salesman, with-wilayah), FF2 pelunasan (`SalesPersonId`), month Faktur, customer master, last Faktur, salesman master — not Piutang/Customer/Salesman snapshot tables.

| KPI / section | Definition |
| --- | --- |
| **Overdue exposure** | Sum `KurangBayar` where aging bucket ≠ Current |
| **Recovery vs Billing %** | Month `TotalBayar ÷ month Faktur omzet × 100` |
| **Cash Collected MTD** | Month `BayarTunai` only |
| **Top overdue rankings** | Overdue balance only — not total piutang |
| **Wilayah** | `Customer.WilayahId`; blank → Unknown |
| **Attention signals** | ChronicOverdue, LegacyDebt, PlafondBreachOverdue, Overdue, HighOverdueWorkload, LowRecoveryVsBilling, WilayahHotspot (≥15%) |

**Refresh order in `All`:** … → Customer → Salesman → **Collection** (last).

**Snapshot tables:** `BTR_PortalDashboardCollectionKpi`, `BTR_PortalDashboardCollectionAging`, `BTR_PortalDashboardCollectionAttention`, `BTR_PortalDashboardCollectionTopOverdueCustomer`, `BTR_PortalDashboardCollectionTopOverdueSalesman`, `BTR_PortalDashboardCollectionTopOverdueWilayah`.

**Executive promotion (Phase 2 — deferred):** Cash Collected MTD, Recovery vs Billing %, Overdue Concentration % on Management Attention Center.

### Location (warehouse + wilayah concentration — M22)

Reads **source DALs** at refresh (stok balance, faktur, invoice, warehouse master, brg last faktur) and **denominator snapshots** from Inventory, InventoryRisk, Sales, Purchasing KPI rows.

| KPI / section | Definition |
| --- | --- |
| **Warehouse inventory %** | Top warehouse `Hpp×Qty` ÷ M15 `TotalInventoryValue` (excl. In-Transit) |
| **Warehouse at-risk %** | M19-classified items allocated by warehouse ÷ M19 at-risk total |
| **Warehouse / Wilayah sales %** | MTD Faktur omzet by billing warehouse or customer Wilayah ÷ Sales `TotalOmzet` |
| **Attention signals** | WarehouseInventoryConcentration, WarehouseAtRiskConcentration, WarehouseSalesConcentration, WarehousePurchasingConcentration, WarehouseNoSalesWithInventory, WarehouseInactiveWithStock |
| **Ranking universe** | Active, non-special warehouses; exclude In-Transit by name |

**Refresh order in `All`:** … → Collection → **Location** (last).

**Snapshot tables:** `BTR_PortalDashboardLocationKpi`, five Top ranking tables, `BTR_PortalDashboardLocationAttention`.

**Executive promotion (Phase 2 — deferred):** Top 1 Warehouse Inventory %, Top 1 Warehouse Sales %, Inactive Warehouse With Stock Count.

### Alert Center consumer (M23 — no new snapshot domain)

M23 **Alert Center** reads existing producer snapshots at API request time via `DashboardAlertCenterComposer`. **No** new `BTR_PortalDashboard*` tables, refresh workers, or aggregator domains.

| Reads from | M23 use |
| --- | --- |
| Customer, Salesman, Collection, PurchasingManagement, Location attention tables | Entity alert rows (deduplicated, capped) |
| Inventory Risk KPI snapshot | Summary panel only — not item attention rows |
| Sales KPI + executive health inputs | Company achievement alert; platform stale/degraded flags |
| Domain KPI snapshots (Customer, Collection, Location, Piutang, Inventory Risk) | Concentration section metrics |

### Inventory Risk (slow moving & dead stock — M19)

Reads **source DALs** at refresh (`IStokBalanceViewDal` + `IBrgLastFakturDal`) — not M15 Inventory snapshot tables. Uses shared `DashboardInventoryItemGroupBuilder` for position rules.

| KPI | Definition |
| --- | ---------- |
| **Dead Stock** | Items with `LastFakturDate` idle ≥ 180 days; value = `Hpp × Qty` |
| **Slow Moving** | Items with idle **90–179 days** (mutually exclusive with Dead; excludes Never Sold) |
| **Never Sold** | Stock with no non-void FakturItem history — separate signal |
| **At-Risk Inventory %** | `(NeverSold + Slow + Dead value) / TotalInventoryValue` |
| **Aging buckets** | Active (≤89 days) · Slow Moving · Dead Stock · Never Sold |
| **Attention list** | One row per item × signal: DeadStock, SlowMoving, NeverSold |
| **Category/Supplier exposure** | Top 10 at-risk value per dimension |

**Classification authority:** `MAX(FakturDate)` per `BrgId` from gross Faktur only. Retur and non-sales outflows do not reset the clock.

**Refresh order in `All`:** Piutang → Inventory → **InventoryRisk** → Sales → Purchasing → Customer → Salesman.

**Snapshot tables:** `BTR_PortalDashboardInventoryRiskKpi`, `Aging`, `Attention`, `TopDead`, `TopSlow`, `Breakdown`.

**Protected modules unchanged:** `DashboardInventoryAggregator`, `BTR_PortalDashboardInventory*`, `GET /api/dashboard/inventory`.

**Executive promotion (Phase 2 — deferred):** Dead Stock Value, At-Risk %, Inventory Risk Attention Indicator on Management Attention Center.

---

## Dashboard–Report Traceability Matrix

| Dashboard KPI | Report | Reconciliation |
| ------------- | ------ | -------------- |
| Total Piutang | Piutang Report footer | `Sum(KurangBayar)` where `> 1` |
| Total Customer (Piutang) | Piutang Report footer | Distinct customer key count |
| Total Inventory Value | Inventory Report footer | BrgId-grouped `Sum(Hpp × Qty)` excl. In-Transit |
| Total Item | Inventory Report footer | Count BrgId where aggregated Qty > 0 |
| Total Inventory Value (Inventory Risk) | Inventory Dashboard / Report footer | Same BrgId-first denominator as M15 |
| At-Risk Inventory % | — | Sum of disjoint Never/Slow/Dead values ÷ TotalInventoryValue |
| Sales Total Omzet / Achievement | Sales Report | Sum of `GrandTotal` from report rows (same month) |
| Grand Total Purchase / Total Invoice | Purchasing Report footer | Same invoice source and current-month period |

If dashboard and report totals diverge after both pages are refreshed, escalate — indicates snapshot staleness, worker failure, or data issue.

---

## Affected Users

| User | Need |
| ---- | ---- |
| Board of Directors | Monthly/quarterly performance, receivable exposure, inventory capital |
| Stakeholders / owners | High-level KPIs, trends, concentration risk |
| Finance leadership | Aging distribution, overdue customer counts |
| Operations leadership | Inventory composition, sales ranking |
| IT / administrators | Worker scheduling, monitoring, manual refresh |

---

## Acceptance Criteria (Feature Complete)

1. Analytical dashboard APIs read from materialized storage; p95 target &lt; 500 ms with populated snapshots.
2. Piutang KPIs, aging buckets, and Top 10 match pre-materialization semantics.
3. Inventory KPIs and breakdowns match pre-materialization semantics.
4. Sales KPIs match Faktur-based definitions and reconcile with Sales Report for current month.
5. `GeneratedAt` reflects last successful background refresh.
6. `BTR_SalesOmzet` reconcile workflow unaffected.
7. Overview home loads from Layer A snapshots only.
8. Refresh cadence operational: 15 / 30 / 30 / 30 / 30 / 60 / 60 min per domain (Piutang / Sales / Purchasing / Customer / Salesman / Inventory / InventoryRisk).
9. Purchasing KPIs reconcile with Purchasing Report footer totals.
10. Customer snapshot populated by dedicated worker reading source DALs; Customer Analytics API serves from `BTR_PortalDashboardCustomer*` tables.
11. Salesman snapshot populated by dedicated worker reading source DALs; Salesman Performance API serves from `BTR_PortalDashboardSalesman*` tables; Top 10 Omzet reconciles with Sales Report totals grouped by salesman name for current month.
12. Inventory Risk snapshot populated by dedicated worker reading `IStokBalanceViewDal` + `IBrgLastFakturDal`; API serves from `BTR_PortalDashboardInventoryRisk*` tables; `TotalInventoryValue` reconciles with M15 Inventory Dashboard.

---

## Future Extensions (Not Delivered)

| Feature | Implication |
| ------- | ----------- |
| Custom date-range filters | Requires parameterized snapshots or on-demand re-aggregation |
| Drilldown from chart to transaction | New API endpoints and grain |
| Sales pipeline from orders | Requires `BTR_Order` — intentionally excluded in Faktur-only board view |
| Purchasing warehouse breakdown / Retur Beli analytics | Deferred from Purchasing dashboard V1 |
| Event-driven refresh | Hook after pelunasan, stock balance generation |
| Monthly snapshot history | Only if date-range analytics require slowly changing aggregates |
