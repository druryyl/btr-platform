# Sales Forecast Dashboard

**Feature:** M26 — Sales Forecast Dashboard  
**Status:** Current  
**Route:** `/dashboard/sales-forecast`  
**API:** `GET /api/dashboard/sales-forecast`

---

## Purpose

Management needs to anticipate **month-end invoiced sales** early enough to take corrective action. The Sales Forecast Dashboard answers:

> *If our current billing pace continues, where will we finish at month-end, and how much daily sales are required to still achieve the target?*

This is a **read-only**, **deterministic**, **explainable** projection layer — not AI/ML.

---

## Relationship to Sales Dashboard

| Sales Dashboard | Sales Forecast Dashboard |
| --------------- | ------------------------ |
| What happened? | What will probably happen? |
| Current achievement focus | Projected achievement focus |
| Weekly trend (historical) | Daily pace + projected line |
| No required daily sales | Required daily sales prominent |
| No scenario bands | Best / Expected / Worst range |

**Traceability:** `CurrentSales` and `TotalTarget` in the forecast snapshot **must equal** `TotalOmzet` and `TotalTarget` on `BTRPD_SalesKpi` for the same refresh (FR-40, FR-41).

---

## Data Scope (V1)

| Rule | Description |
| ---- | ----------- |
| FR-01 | Current calendar month only |
| FR-02 | Business date from `IBusinessDateProvider.Today` |
| FR-03 | Faktur-only — same as Sales Dashboard; pipeline excluded |
| FR-04 | Non-void Fakturs only |
| FR-05 | Grouping date = `FakturView.Tgl` |
| FR-06 | Future-dated Fakturs within month excluded from daily pace display |
| FR-10 | Target = `SumTargetAmountForMonth` — same as Sales Dashboard |

**Out of scope (V1):** per-salesman forecast, working-day calendar, custom periods, pipeline omzet, Alert Center registration, Executive Dashboard link.

---

## Primary Algorithm — Current Pace (Calendar-Day Linear)

Plain language:

> Divide this month's invoiced sales so far by days elapsed, then multiply by total days in the month.

| Symbol | Meaning |
| ------ | ------- |
| CS | Current sales (MTD invoiced omzet) |
| T | Monthly target |
| DE | Days elapsed (inclusive, min 1) |
| DR | Days remaining (calendar) |
| DIM | Days in month |

| KPI | Formula |
| --- | ------- |
| Daily Average Sales | CS ÷ DE |
| Forecast Sales | (CS ÷ DE) × DIM |
| Forecast Achievement % | Achievement policy on Forecast Sales vs T |
| Required Daily Sales | (T − CS) ÷ DR when T > CS and DR > 0; else 0 |
| Target Gap | T − Forecast Sales |
| Forecast Variance | Forecast Sales − CS |

**Scenario bands:**

| Scenario | Formula |
| -------- | ------- |
| Expected | Current pace (primary) |
| Best Case | MAX(MTD daily avg, recent-7-day avg) × DIM |
| Worst Case | MIN(MTD daily avg, recent-7-day avg) × DIM |

Recent-7-day average falls back to MTD average when DE < 7.

---

## Derived Indicators

### Forecast Confidence

| Days elapsed | Confidence |
| ------------ | ---------- |
| ≤ 5 | Low |
| 6–20 | Medium |
| ≥ 21 | High |

### Forecast Risk Band

Uses `ExecutiveSalesAchievementBandResolver` on **Forecast Achievement %**:

| Band | Threshold |
| ---- | --------- |
| Healthy | ≥ 100% |
| Warning | 80–99% |
| Critical | < 80% |
| Unknown | null achievement |

### Required Daily Severity

| Condition | Severity |
| --------- | -------- |
| Required > 2× daily average | Critical |
| Required > 1.5× daily average | Warning |
| Otherwise | Normal |

---

## Snapshot Tables

Refreshed with the **Sales** domain worker (~30 min). No separate worker domain.

| Table | Role |
| ----- | ---- |
| `BTRPD_SalesForecastKpi` | Forecast KPI snapshot (`SnapshotKey = 'CURRENT'`) |
| `BTRPD_SalesDailyPace` | Daily pace buckets for chart (~31 rows/month) |

Weekly trend is read from existing `BTRPD_SalesWeekTrend` — not duplicated.

---

## User Experience

1. Executive summary (server-computed plain language)
2. KPI rows: Actual vs Forecast, Pace & Gap, Scenario & Confidence
3. Daily Pace Trend chart
4. Forecast vs Target + Weekly Pace charts
5. Forecast Risk card
6. Traceability footer → Sales Report

---

## Related Documents

- [BTR Portal operational guide](../btr-portal/btr-portal-operational.md) — navigation and usage
- [Materialized dashboard architecture](../materialized-dashboard/materialized-dashboard-architecture.md) — table definitions
- [Sales person principal target](../sales-person-principal-target/feature.md) — target resolution
