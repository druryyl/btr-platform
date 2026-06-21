# Inventory Forecast Dashboard

**Feature:** M28 — Inventory Forecast Dashboard  
**Status:** Current  
**Route:** `/dashboard/inventory-forecast`  
**API:** `GET /api/dashboard/inventory-forecast`

---

## Purpose

Management needs to anticipate **stock-outs**, **overstock buildup**, and **replenishment timing** before they become operational problems. The Inventory Forecast Dashboard answers:

> *At current 30-day sales consumption pace, which active SKUs may run out within the planning horizon, how much inventory capital is projected at horizon end, and when should purchasing review replenishment?*

This is a **read-only**, **deterministic**, **explainable** projection — Rolling 30-Day Average Daily Consumption (ADC) with linear stock depletion. No AI/ML.

---

## Relationship to Inventory and Inventory Risk Dashboards

| Inventory Dashboard (M15) | Inventory Forecast Dashboard |
| ------------------------- | ---------------------------- |
| Current composition and value | Projected value at horizon |
| Category/supplier breakdown | Days of supply and stock-out timing |
| No consumption pace | 30-day Faktur sales qty as ADC |

| Inventory Risk Dashboard (M19) | Inventory Forecast Dashboard |
| ------------------------------ | ---------------------------- |
| Backward-looking obsolescence (dead/slow/never sold) | Forward-looking depletion for active SKUs |
| At-Risk Inventory % | Same % copied for traceability (IFR-51) |

**Traceability:** `CurrentInventoryValue` must equal M15 `TotalInventoryValue` on the same refresh (IFR-50). `AtRiskInventoryPercent` must equal M19 when refreshed together (IFR-51).

---

## Data Scope (V1)

| Rule | Description |
| ---- | ----------- |
| IFR-01 | Planning horizon **H = 30 calendar days** forward from business date |
| IFR-02 | Business date from `IBusinessDateProvider.Today` |
| IFR-03 | Position: BrgId-first; exclude In-Transit; `Qty > 0` only |
| IFR-04 | Valuation: `Hpp × Qty` — same as M15 |
| IFR-05 | Consumption: `SUM(QtyJual)` from non-void Fakturs |
| IFR-06 | Primary ADC window: rolling 30 calendar days `(B−29)..B` |
| IFR-07 | Secondary 90-day window for scenario bands only |
| IFR-08 | Forecast-eligible: `IsAktif = true`, not Dead Stock or Never Sold |
| IFR-13 | Gross consumption (retur not netted) |
| IFR-30 | Default lead time 7 days (configurable) |
| IFR-31 | Coverage days 14 for recommended qty hint |
| IFR-32 | Overstock threshold DOS 90 days |
| IFR-41 | Read-only — no PO creation |

**Out of scope (V1):** per-warehouse forecast, supplier lead time master, Alert Center, Executive integration, net-of-retur consumption, Kartu Stok mutasi source.

---

## Primary Algorithm — Rolling 30-Day ADC + Linear Depletion

Plain language:

> Total Faktur sales qty over the last 30 days ÷ 30 = average daily consumption (ADC). Current qty ÷ ADC = days of supply (DOS). Linear depletion projects qty and value at each day through the horizon.

| Symbol | Meaning |
| ------ | ------- |
| Q | Current on-hand qty (BrgId-first) |
| S₃₀ | Units sold in rolling 30 days |
| ADC | S₃₀ ÷ 30 |
| DOS | Q ÷ ADC when ADC > 0 |
| H | Planning horizon (default 30) |
| LT | Default lead time (default 7) |
| CD | Coverage days for reorder hint (default 14) |

| KPI | Formula |
| --- | ------- |
| Projected Stock-Out Date | B + CEILING(DOS) when DOS ≤ H |
| Reorder Review Date | Stock-out date − LT |
| Recommended Purchase Qty | MAX(0, CEILING(ADC × (LT + CD) − Q)) |
| Forecast Qty @ H | MAX(0, Q − ADC × H) |
| Projected Inventory Value @ H | SUM(forecast qty × unit Hpp) |
| Best / Worst Case Value | Use MIN/MAX(ADC₃₀, ADC₉₀) in depletion |
| Inventory Health Score | 100 − weighted penalties (stock-out %, overstock %, at-risk %) |

---

## Snapshot Materialization

Extended **Inventory Risk** worker (`RefreshDashboardInventoryRiskSnapshotWorker`, domain label `"InventoryRisk"`, ~60 min):

1. Load stock balance + last faktur (M19)
2. Load item consumption (30d + 90d) + daily company consumption
3. Aggregate M19 risk (unchanged)
4. Aggregate M28 forecast
5. Save all tables in one transaction

**Tables:** `BTRPD_InventoryForecastKpi`, `DailyConsumption`, `Level`, `Risk`, `Recommendation`.

Manual refresh: `POST /api/admin/dashboard/refresh?domain=InventoryRisk` or worker CLI `--domain InventoryRisk`.

---

## Dashboard Sections

1. Executive summary
2. KPI row — Position vs projection (current value, projected value, avg DOS, health score)
3. KPI row — Risk exposure (stock-out count, understock/overstock value, at-risk %)
4. KPI row — Scenario bands (best/expected/worst + confidence)
5. Forecast inventory level chart (days 0..H)
6. Consumption trend + risk heat summary
7. Top inventory risks (max 10)
8. Purchase recommendations (max 10, decision support only)
9. Traceability footer with cross-links

**Purchase disclaimer:** Recommended quantities are indicative. Confirm with supplier, pending postings, and in-transit stock in BTR Desktop before purchasing.

---

## Document Maintenance

When forecast rules change, update this document and `docs/features/materialized-dashboard/materialized-dashboard-domain.md` (Inventory Forecast extension section).

**Success criterion:** Product Owner can validate forecast behavior from this document without reading source code.
