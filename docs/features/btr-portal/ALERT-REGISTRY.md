# BTR Portal — Alert Registry

**Audience:** Product Owner, Architect, Implementer, Future Analysts  
**Purpose:** Authoritative catalog of portal **alert types** (`SignalKey`), their **owning milestones**, **M23 Alert Center inclusion rules**, and **drill-down destinations**. Investigation routing defaults (report route, period mode, Desktop next step) are defined in `InvestigationRegistry.cs` (M24) — entity alerts use **Investigate → Report** as the primary path.

**Governance:** New alert types require **Product Owner approval only**. Update this document before implementing new `SignalKey` values in aggregators or adding types to M23.

**Related:** [M23 Alert Center Analysis](../../work/btr-portal/M23-Alert-Center-Analysis.md) · [btr-portal-domain.md](./btr-portal-domain.md)

---

## M23 Alert Center rules (summary)

| Rule | Value |
| ---- | ----- |
| Route | `/alerts` — title **Alert Center** |
| Landing page | M16 `/dashboard` unchanged — **Open Alert Center** from executive page |
| Entity alert cap | **Top 20 per category** (producer priority sort within category) |
| M19 inventory | **Summary only** — no item rows in Alert Center |
| M16 Top 5 lists | **Not alerts** — excluded |
| Sections | **Alerts** (exceptions) · **Concentrations** (informational) · **Inventory risk summary** |
| Deduplication | M20 wins customer overdue; LegacyDebt suppresses Dormant; M20 wins salesman overdue workload |
| New SignalKeys in M23 | **Forbidden** — aggregate M17–M22 producers only |
| Severity | Binary Requires Attention; **Sales Achievement bands only** exception |
| Platform alerts | **Pinned top** always |

---

## Category index

| Category | Producer milestones |
| -------- | ------------------- |
| Sales | M16, M18 |
| Customer | M17 |
| Collection | M20 |
| Inventory | M19 (summary), M21 cross-risk |
| Purchasing | M21 |
| Location | M22 |
| Platform | M16 / refresh health |

---

## Registry entries

**Investigation columns (M24):** **Investigate →** is the primary report route from `InvestigationRegistry.cs`. **Period mode** and **Desktop next step** are default query/UI hints for that path. **Drill-down dashboard** remains the **View Dashboard** destination. M23 inclusion rules are unchanged.

### Sales

| SignalKey | Label | Owner | Entity grain | M23 section | M23 inclusion | Drill-down dashboard | Investigate → | Period mode | Desktop next step | Dedup / notes |
| --------- | ----- | ----- | ------------ | ----------- | ------------- | -------------------- | ------------- | ----------- | ----------------- | ------------- |
| `SalesAchievementWarning` | Achievement Warning | M16 / M11 | Company | Alerts | Yes — when band = Warning | `/dashboard/sales` | — | — | — | Achievement bands only severity exception |
| `SalesAchievementCritical` | Achievement Critical | M16 / M11 | Company | Alerts | Yes — when band = Critical | `/dashboard/sales` | — | — | — | Company headline |
| `BelowTarget` | Below Target | M18 | Salesman | Alerts | Yes — Top 20 Sales | `/dashboard/salesmen` | `/reports/sales` | `currentMonth` | Sales Omzet Chart (RO2) | Achievement band on rep |
| `MissingTargetSetup` | Missing Target Setup | M18 | Salesman | Alerts | Yes — Top 20 Sales | `/dashboard/salesmen` | `/reports/sales` | `currentMonth` | — | |
| `CustomerConcentration` | Customer Concentration | M18 | Salesman | Concentrations | Yes — informational | `/dashboard/salesmen` | `/reports/sales` | `currentMonth` | Sales Omzet Chart (RO2) | Not default Alerts feed |

### Customer

| SignalKey | Label | Owner | Entity grain | M23 section | M23 inclusion | Drill-down dashboard | Investigate → | Period mode | Desktop next step | Dedup / notes |
| --------- | ----- | ----- | ------------ | ----------- | ------------- | -------------------- | ------------- | ----------- | ----------------- | ------------- |
| `Overdue` | Overdue | M17 | Customer | Alerts | Yes — unless M20 canonical row exists | `/dashboard/customers` | `/reports/piutang` | `allOpenBalances` | Piutang Tracker (FT5) | **Suppress when M20 Overdue/Chronic/Plafond/Legacy applies** |
| `Dormant` | Dormant | M17 | Customer | Alerts | Yes — unless LegacyDebt | `/dashboard/customers` | `/reports/sales` | — | Customer master / Faktur history | **Suppress when M20 LegacyDebt** |
| `PlafondBreach` | Plafond Breach | M17 | Customer | Alerts | Yes — unless M20 PlafondBreachOverdue | `/dashboard/customers` | `/reports/piutang` | `allOpenBalances` | Piutang Tracker (FT5) | |
| `SuspendedWithSales` | Suspended + Sales | M17 | Customer | Alerts | Yes | `/dashboard/customers` | `/reports/sales` | — | Customer master / Faktur history | Operational exception |

### Collection

| SignalKey | Label | Owner | Entity grain | M23 section | M23 inclusion | Drill-down dashboard | Investigate → | Period mode | Desktop next step | Dedup / notes |
| --------- | ----- | ----- | ------------ | ----------- | ------------- | -------------------- | ------------- | ----------- | ----------------- | ------------- |
| `ChronicOverdue` | Chronic Overdue | M20 | Customer | Alerts | Yes — canonical overdue | `/dashboard/collection` | `/reports/piutang` | `allOpenBalances` | Piutang Tracker (FT5) | Priority 1 customer signal |
| `PlafondBreachOverdue` | Plafond Breach + Overdue | M20 | Customer | Alerts | Yes | `/dashboard/collection` | `/reports/piutang` | `allOpenBalances` | Piutang Tracker (FT5) | |
| `LegacyDebt` | Legacy Debt | M20 | Customer | Alerts | Yes — replaces M17 Dormant | `/dashboard/collection` | `/reports/piutang` | `allOpenBalances` | Piutang Tracker (FT5) | |
| `Overdue` | Overdue | M20 | Customer | Alerts | Yes — when no higher priority | `/dashboard/collection` | `/reports/piutang` | `allOpenBalances` | Piutang Tracker (FT5) | **Canonical over M17 Overdue** |
| `HighOverdueWorkload` | High Overdue Workload | M20 | Salesman | Alerts | Yes — canonical | `/dashboard/collection` | `/reports/piutang` | `allOpenBalances` | Piutang Tracker (FT5) | **Suppress M18 HighOverdueExposure** |
| `LowRecoveryVsBilling` | Low Recovery vs Billing | M20 | Salesman / Company | Alerts / Concentrations | Yes | `/dashboard/collection` | — | — | — | Phase 2 executive candidate |
| `WilayahHotspot` | Wilayah Hotspot | M20 | Wilayah | Alerts | Yes — ≥15% company overdue | `/dashboard/collection` | — | — | — | **M22 must not duplicate** |

### Inventory

| SignalKey | Label | Owner | Entity grain | M23 section | M23 inclusion | Drill-down dashboard | Investigate → | Period mode | Desktop next step | Dedup / notes |
| --------- | ----- | ----- | ------------ | ----------- | ------------- | -------------------- | ------------- | ----------- | ----------------- | ------------- |
| `DeadStock` | Dead Stock | M19 | Item | — | **Summary only** in M23 | `/dashboard/inventory-risk` | `/reports/inventory` | — | Kartu Stok (IF8) | Count + value on summary panel |
| `SlowMoving` | Slow Moving | M19 | Item | — | **Summary only** in M23 | `/dashboard/inventory-risk` | `/reports/inventory` | — | Kartu Stok (IF8) | |
| `NeverSold` | Never Sold | M19 | Item | — | **Summary only** in M23 | `/dashboard/inventory-risk` | `/reports/inventory` | — | Kartu Stok (IF8) | |
| `AtRiskInventoryPercent` | At-Risk Inventory % | M19 | Company | Concentrations | Yes — Phase 2 OK | `/dashboard/inventory-risk` | — | — | — | KPI not SignalKey row |
| `PrincipalAtRiskExposure` | At-Risk Exposure | M21 | Principal | Alerts | Yes — from M21 attention | `/dashboard/purchasing` | `/reports/inventory` | — | — | Cross-domain read M19 |

### Purchasing

| SignalKey | Label | Owner | Entity grain | M23 section | M23 inclusion | Drill-down dashboard | Investigate → | Period mode | Desktop next step | Dedup / notes |
| --------- | ----- | ----- | ------------ | ----------- | ------------- | -------------------- | ------------- | ----------- | ----------------- | ------------- |
| `QualifiedBacklog` | Qualified Backlog | M21 | Principal | Alerts | Yes | `/dashboard/purchasing` | `/reports/purchasing` | `currentMonth` (+ `posting=BELUM`) | Posting Stok (PT2) | Age ≥ `PurchasingQualifiedBacklogDays` |
| `PrincipalSpendConcentration` | Spend Concentration | M21 | Principal | Concentrations | Yes | `/dashboard/purchasing` | `/reports/purchasing` | `currentMonth` | — | |
| `PrincipalInventoryConcentration` | Inventory Concentration | M21 | Principal | Concentrations | Yes | `/dashboard/purchasing` | `/reports/inventory` | — | — | Reads M15 |
| `CompoundDependency` | Compound Dependency | M21 | Principal | Alerts | Yes | `/dashboard/purchasing` | `/reports/purchasing` | — | Principal master | **Single link only**; multi-step on report |
| `PrincipalInventoryNoPurchase` | Inventory, No Purchase | M21 | Principal | Alerts | Yes | `/dashboard/purchasing` | `/reports/inventory` | — | — | |
| `PurchasingInactivity` | Purchasing Inactivity | M21 | Company | Alerts | Yes | `/dashboard/purchasing` | — | — | — | |
| `UnknownPrincipal` | Unknown Principal | M21 | Principal | Alerts | Yes | `/dashboard/purchasing` | `/reports/purchasing` | `currentMonth` | — | |

### Location

| SignalKey | Label | Owner | Entity grain | M23 section | M23 inclusion | Drill-down dashboard | Investigate → | Period mode | Desktop next step | Dedup / notes |
| --------- | ----- | ----- | ------------ | ----------- | ------------- | -------------------- | ------------- | ----------- | ----------------- | ------------- |
| `WarehouseInactiveWithStock` | Inactive With Stock | M22 | Warehouse | Alerts | Yes | `/dashboard/locations` | `/reports/inventory` | — | Kartu Stok (IF8) | Priority 1 location signal |
| `WarehouseNoSalesWithInventory` | Stock Without Sales | M22 | Warehouse | Alerts | Yes | `/dashboard/locations` | `/reports/inventory` | — | Kartu Stok (IF8) | |
| `WarehouseAtRiskConcentration` | At-Risk Concentration | M22 | Warehouse | Concentrations | Yes | `/dashboard/locations` | — | — | — | Link M19 for item detail; multi-step on report |
| `WarehouseInventoryConcentration` | Inventory Concentration | M22 | Warehouse | Concentrations | Yes | `/dashboard/locations` | `/reports/inventory` | — | — | |
| `WarehouseSalesConcentration` | Sales Concentration | M22 | Warehouse | Concentrations | Yes | `/dashboard/locations` | `/reports/sales` | `currentMonth` | — | |
| `WarehousePurchasingConcentration` | Purchasing Concentration | M22 | Warehouse | Concentrations | Yes | `/dashboard/locations` | `/reports/purchasing` | `currentMonth` | — | |

### Platform

| SignalKey | Label | Owner | Entity grain | M23 section | M23 inclusion | Drill-down dashboard | Investigate → | Period mode | Desktop next step | Dedup / notes |
| --------- | ----- | ----- | ------------ | ----------- | ------------- | -------------------- | ------------- | ----------- | ----------------- | ------------- |
| `SnapshotStale` | Dashboard Data Not Fresh | M16 / health | System | Alerts (pinned) | Yes — always top | `/dashboard` or admin refresh | — | — | — | Invalidates trust in other alerts |
| `SnapshotDegraded` | Snapshot Refresh Failed | Health endpoint | System | Alerts (pinned) | Yes — always top | Health / worker | — | — | — | |
| `DomainUnavailable` | Domain Snapshot Unavailable | M16 composer | System | Alerts (pinned) | Yes | `/dashboard` | — | — | — | |

---

## Ranking investigation keys (not alerts)

Synthetic `SignalKey` values for **Top 10 / ranking row** drill-down (M24). Not included in M23 Alert Center. Defined in `InvestigationRegistry.cs`.

| SignalKey | Source surface | Entity grain | Investigate → | Period mode | Desktop next step |
| --------- | -------------- | ------------ | ------------- | ----------- | ----------------- |
| `ExecutiveTopCustomerExposure` | M16 Critical Exposures | Customer | `/reports/piutang` | `allOpenBalances` | Piutang Tracker (FT5) |
| `ExecutiveTopCategoryExposure` | M16 Critical Exposures | Category | `/reports/inventory` | — | — |
| `ExecutiveTopSupplierExposure` | M16 Critical Exposures | Supplier | `/reports/inventory` | — | — |
| `ExecutiveTopPrincipalExposure` | M16 Critical Exposures | Principal | `/reports/purchasing` | `currentMonth` | — |
| `LegacyTopSalesman` | M11 Sales Top 10 | Salesman | `/reports/sales` | `currentMonth` | Sales Omzet Chart (RO2) |
| `LegacyTopCustomer` | M14 Piutang Top 10 | Customer | `/reports/piutang` | `allOpenBalances` | Piutang Tracker (FT5) |
| `LegacyTopCategory` | M15 Inventory Top 10 | Category | `/reports/inventory` | — | — |
| `LegacyTopSupplier` | M15 Inventory Top 10 | Supplier | `/reports/inventory` | — | — |
| `RankingCustomerTopOmzet` | M17 Customer rankings | Customer | `/reports/sales` | `currentMonth` | — |
| `RankingCustomerTopPiutang` | M17 Customer rankings | Customer | `/reports/piutang` | `allOpenBalances` | Piutang Tracker (FT5) |
| `RankingSalesmanTopOmzet` | M18 Salesman rankings | Salesman | `/reports/sales` | `currentMonth` | Sales Omzet Chart (RO2) |
| `RankingSalesmanTopAchievement` | M18 Salesman rankings | Salesman | `/reports/sales` | `currentMonth` | Sales Omzet Chart (RO2) |
| `RankingSalesmanTopPiutang` | M18 Salesman rankings | Salesman | `/reports/piutang` | `allOpenBalances` | Piutang Tracker (FT5) |
| `RankingCollectionTopOverdueCustomer` | M20 Collection rankings | Customer | `/reports/piutang` | `allOpenBalances` | Piutang Tracker (FT5) |
| `RankingCollectionTopOverdueSalesman` | M20 Collection rankings | Salesman | `/reports/piutang` | `allOpenBalances` | Piutang Tracker (FT5) |
| `RankingTopPrincipal` | M21 Purchasing Top 10 | Principal | `/reports/purchasing` | `currentMonth` | — |

---

## Explicitly excluded from portal alerts (V1)

| Topic | Disposition |
| ----- | ----------- |
| Faktur Kembali aggregate | Sales Report row-level only |
| Retur analytics | Desktop RF1/RF2 |
| Effective Call / route compliance | M25 |
| DSO / aging trend | No history |
| Tagihan pipeline KPIs | Desktop workflow |
| Sales Omzet Health | IT/operations |
| Stok Balance Health | IT/operations |
| Desktop menu deep links | Out of portal scope |

---

## Change log

| Date | Change | Approver |
| ---- | ------ | -------- |
| 2026-06-09 | Initial registry from M23 analysis + PO decisions Q1–Q30 | Product Owner |
| 2026-06-09 | Shipped in `AlertCenterRegistry.cs` — source-aware lookup for duplicate `Overdue` key (M17 vs M20) | Implementer |
| 2026-06-09 | M24 Investigation column group + ranking investigation keys subsection | Implementer |

---

*Update this file when PO approves new SignalKey types or changes M23 inclusion rules.*
