# Inventory Optimization Dashboard

**Feature:** M28.5 — Inventory Optimization Dashboard  
**Status:** Current  
**Route:** `/dashboard/inventory-optimization`  
**API:** `GET /api/dashboard/inventory-optimization`

---

## Purpose

Management needs **prioritized, explainable actions** — not just forecasts — to balance replenishment, working capital, warehouse distribution, and purchasing constraints. The Inventory Optimization Dashboard answers:

> *Given forecast, inventory risk, purchasing backlog, and warehouse distribution, what should management do today — purchase, defer, delay, transfer, post-first, promote, or clear — and why?*

This is a **read-only**, **deterministic**, **rule-based** recommendation layer. No ML, no solvers, no automatic PO or mutasi creation.

---

## Relationship to Upstream Dashboards

| Dashboard | Role in M28.5 |
| --------- | ------------- |
| **Inventory Forecast (M28)** | Primary reorder input — ADC, DOS, recommended qty, urgency via in-memory `ForecastItemContext` (IO-31) |
| **Slow Moving & Dead Stock (M19)** | Obsolescence classification — Do Not Reorder, clearance, delay elevation |
| **Purchasing Management (M21)** | Cross-read snapshot — qualified backlog (Post Purchase First), MTD purchase (reduce qty), principal inventory no purchase (delay elevation) |
| **Inventory Dashboard (M15)** | `CurrentInventoryValue` traceability via M28 (IFR-50 / IO-32) |

**Traceability:** `InventoryHealthScore` must equal M28 `InventoryHealthScore` on the same refresh (IO-50). Do Not Reorder items never appear in the Recommended Reorder list (IO-52).

---

## Recommendation Types

| Action | Business meaning |
| ------ | ---------------- |
| **Purchase** | Replenish forecast-eligible SKU within budget and urgency rules |
| **Defer** | Purchase deferred when optional budget cap exceeded |
| **Delay** | Reduce or postpone purchase for overstock / slow-moving SKUs |
| **Transfer** | Move stock from excess warehouse to shortage warehouse (same BrgId) |
| **Post Purchase First** | Complete M21 qualified backlog for supplier before new purchase of same supplier's stock-out SKU |
| **Promote / Bundle** | Marketing-oriented action for slow movers (lower priority) |
| **Clearance** | Recover capital from dead stock candidates |
| **Do Not Reorder** | Dead, Never Sold, or inactive — suppresses purchase |

**Precedence:** Do Not Reorder > Post Purchase First > Delay > Transfer > Purchase > Promote/Bundle/Clearance.

---

## Data Scope (V1)

| Rule | Description |
| ---- | ----------- |
| IO-01 | Destination warehouse DOS ≤ 14 days (configurable) |
| IO-02 | Source warehouse DOS ≥ 60 days for same item |
| IO-03 | Transfer qty from policy `ComputeTransferQty` when positive |
| IO-04 | Exclude In-Transit, special, and inactive warehouses |
| IO-05 | Max 10 transfer rows materialized |
| IO-06 | Warehouse ADC from Faktur qty by `WarehouseId` + `BrgId`, 30-day window |
| IO-10..14 | Delay / reduce rules using M28 overstock, M19 slow moving, M21 signals |
| IO-20 | Reuse M28 `InventoryHealthScore` — no separate optimization score |
| IO-21 | Recoverable capital from M19 dead + slow clearance candidates |
| IO-22 | Post Purchase First when M21 qualified backlog supplier matches stock-out supplier |
| IO-30 | Same Inventory Risk worker cadence (~60 min) as M28 |
| IO-31 | Single ADC pass — reuse M28 item contexts |
| IO-34 | Max 25 top actions; 15 reorder; 10 per specialized table |
| IO-35 | Every row has `RuleId`, `ReasonText`, `ReportRoute` |

**Out of scope (V1):** automatic PO/mutasi, user budget entry in UI, Alert Center integration, per-warehouse forecast dashboard.

---

## Priority Scoring

Integer `PriorityScore` ranks unified actions. Components:

- Category weight: Critical=1000, High=750, Medium=500, Low=250
- Strategic boosts (stock-out urgency, recoverable value, M21 signals)
- Sorted descending for Top Actions table

---

## Snapshot Materialization

Extended **Inventory Risk** worker (`RefreshDashboardInventoryRiskSnapshotWorker`, domain label `"InventoryRisk"`, ~60 min):

1. Load stock balance + last faktur (M19)
2. Load item consumption (30d + 90d) + daily company consumption (M28)
3. Aggregate M19 risk
4. Aggregate M28 forecast (exposes `ItemContexts`)
5. Load warehouse consumption (30d) + M21 purchasing snapshot `GetCurrent()`
6. Aggregate M28.5 optimization
7. Save all tables in one transaction

**Tables:** `BTRPD_InventoryOptimizationKpi`, `Action`, `Reorder`, `Transfer`, `Delay`, `Clearance`, `PriorityDist`, `ActionHeat`.

Manual refresh: `POST /api/admin/dashboard/refresh?domain=InventoryRisk` or worker CLI `--domain InventoryRisk`.

**M21 staleness note:** During `All` refresh, Inventory Risk runs before Purchasing Management; optimization reads the **previous** M21 snapshot (up to ~60 min stale — acceptable per architect plan).

---

## Portal UI

- Executive summary with action counts and budget disclaimer
- KPI grid (health score, recommended budget, recoverable capital, action counts)
- Priority distribution chart, impact chart, action heat grid
- Tables: Top Actions, Recommended Reorder, Transfer, Delay/Reduce, Clearance
- Traceability footer linking to Inventory Forecast, Inventory Risk, Purchasing Management, reports

---

## Configuration

| Key | Default | Purpose |
| --- | ------- | ------- |
| `InventoryOptimizationDefaultBudgetCapIdr` | null | Optional indicative budget cap — defers lower-priority purchases |
| `InventoryOptimizationWarehouseShortageDosDays` | 14 | Transfer destination threshold |
| `InventoryOptimizationWarehouseExcessDosDays` | 60 | Transfer source threshold |
| `InventoryOptimizationMaxTopActions` | 25 | Unified action cap |
| `InventoryOptimizationMaxReorderRows` | 15 | Reorder table cap |
| `InventoryOptimizationMaxTransferRows` | 10 | Transfer table cap |
| `InventoryOptimizationReduceQtyFactor` | 0.5 | Reduce-qty multiplier (IO-12) |

Horizon, lead time, and overstock thresholds reuse `InventoryForecast*` settings.
