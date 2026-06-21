# Cash Flow Forecast Dashboard

**Feature:** M27 — Cash Flow Forecast Dashboard  
**Status:** Current  
**Route:** `/dashboard/cash-flow-forecast`  
**API:** `GET /api/dashboard/cash-flow-forecast`

---

## Purpose

Management needs to anticipate **month-end cash collection** and **recovery vs billing** early enough to act on liquidity risk. The Cash Flow Forecast Dashboard answers:

> *If current collection performance continues, how much cash will we likely receive before month-end, where are the biggest collection risks, and what daily collection pace is required to keep up with billing?*

This is a **read-only**, **deterministic**, **explainable** projection layer — not AI/ML.

---

## Relationship to Collection and Sales Dashboards

| Collection Dashboard | Cash Flow Forecast Dashboard |
| -------------------- | ---------------------------- |
| What was collected? | What cash will we likely receive by month-end? |
| Recovery vs Billing (actual) | Recovery vs Billing (forecast) |
| Attention signals (operational) | Top collection risks (forecast-specific rules) |
| No pace projection | Daily cash pace + required daily collection |

| Sales Dashboard | Cash Flow Forecast Dashboard |
| --------------- | ---------------------------- |
| Month Faktur Omzet (billing) | Same benchmark for recovery % and implicit collection target |

**Traceability:** `CashCollectedMtd`, `MonthCollections`, and `MonthFakturOmzet` in the forecast snapshot **must equal** the same fields on `BTRPD_CollectionKpi` for the same refresh (CFR-50, CFR-51, CFR-52).

---

## Data Scope (V1)

| Rule | Description |
| ---- | ----------- |
| CFR-01 | Current calendar month only |
| CFR-02 | Business date from `IBusinessDateProvider.Today` |
| CFR-03 | Pelunasan grouped by `LunasDate` |
| CFR-04 | Billing benchmark = non-void Faktur `GrandTotal` current month |
| CFR-05 | Open receivable rows: `KurangBayar > 1` |
| CFR-07 | UangMuka excluded from collections (FF2 parity) |
| CFR-10 | No collection target master — implicit target = Month Faktur Omzet |
| CFR-40 | Calendar days for pace (no working-day calendar) |

**Out of scope (V1):** per-customer/salesman/wilayah forecast, working-day calendar, collection target master, Alert Center, Executive integration, Pelunasan Report, purchasing cash-outflow forecast.

---

## Primary Algorithm — Current Pace (Calendar-Day Linear)

Plain language:

> Divide this month's cash collected so far by days elapsed, then multiply by total days in the month. The same projection applies to total collections (cash + giro) for recovery vs billing forecast.

| Symbol | Meaning |
| ------ | ------- |
| CC | Cash Collected MTD (`BayarTunai`) |
| MC | Month Collections (`TotalBayar`) |
| BO | Month Faktur Omzet |
| DE | Days elapsed (inclusive, min 1) |
| DR | Days remaining (calendar) |
| DIM | Days in month |

| KPI | Formula |
| --- | ------- |
| Daily Cash Collection Average | CC ÷ DE |
| Expected Cash Collection | (CC ÷ DE) × DIM |
| Projected Month-End Total Collections | (MC ÷ DE) × DIM |
| Collection Forecast % | Projected total collections ÷ BO × 100 |
| Remaining Collection Target | BO − MC when BO > MC; else 0 |
| Required Daily Collection | (BO − MC) ÷ DR when BO > MC and DR > 0 |
| Collection Gap | BO − projected total collections |
| Forecast Variance (cash) | Expected cash − CC |
| Outstanding Due Remaining | Sum open balance where `JatuhTempo` in (today, month-end] |

**Scenario bands (cash):**

| Scenario | Formula |
| -------- | ------- |
| Expected | Current cash pace |
| Best Case | MAX(MTD daily cash avg, recent-7-day cash avg) × DIM |
| Worst Case | MIN(MTD daily cash avg, recent-7-day cash avg) × DIM |

**Forecast Confidence:** Low ≤5d elapsed; Medium 6–20d; High ≥21d.

**Forecast Risk Band:** Based on Collection Forecast % — Healthy ≥100%, Warning 80–99%, Critical <80%, Unknown when BO = 0.

---

## Collection Risk Rules (Top 10)

Deterministic rules evaluated at refresh; priority ordered:

1. Large Invoice Due Soon — due within 7 days, balance ≥ max(P75 overdue, configurable floor)
2. Chronic Overdue — Large — >90d bucket, customer overdue ≥ top-10 threshold
3. Collection Concentration Risk — customer overdue ≥ 15% of company overdue
4. Legacy Debt — Overdue
5. Plafond Breach — Due Soon (due within 14 days)
6. Deteriorating — Low Recovery (salesman in low-recovery set, customer overdue > 0)
7. Wilayah Hotspot — Due Exposure
8. Expected Overdue Growth — no cash from assigned salesman in last 30 days

Configurable floor: `DashboardSnapshot:CashFlowForecastLargeDueSoonFloorAmount` (default IDR 50M).

---

## Materialized Data

**Worker:** Extended `RefreshDashboardCollectionSnapshotWorker` (domain label remains `"Collection"`).

**Tables:**

| Table | Layer | Content |
| ----- | ----- | ------- |
| `BTRPD_CashFlowForecastKpi` | A | Forecast KPI snapshot |
| `BTRPD_CashFlowDailyPace` | B | Daily cash pace chart |
| `BTRPD_CashFlowRecoveryTrend` | B | Cumulative collections vs billing |
| `BTRPD_CashFlowCollectionRisk` | B | Top 10 collection risk rows |

Collection KPI and child tables (`BTRPD_Collection*`) are **unchanged**.

---

## API Response

`GET /api/dashboard/cash-flow-forecast` returns metadata, KPIs, daily pace, recovery trend, collection risks, and server-computed executive summary. When snapshot is missing: `IsAvailable = false`, HTTP 200.

---

## UI

Route `/dashboard/cash-flow-forecast` — sidebar **Cash Flow Forecast** (after Collection).

Four KPI rows: Cash Position → Pace & Target → Recovery & Scenarios → Receivable Context. Charts: daily pace, forecast vs billing, recovery trend. Top Collection Risks table. Traceability footer links Collection Dashboard and Piutang Report.
