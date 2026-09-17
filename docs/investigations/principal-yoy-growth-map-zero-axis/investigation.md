# Investigation: Principal Investigation Workspace — Y-Axis (Year-over-Year Growth) Always Shows Zero

**Date:** 2026-09-12
**Reporter:** Product / Demo preparation
**Environment:** Database `btr2` on server `JUDE7`; Presentation `BusinessDate = 2026-06-20`
**Affected component:** `btr.portal.web` — Investigation Workspace population map projection (`PRN-GRW-002` Y axis)

Related:
- Endpoint: `GET /api/entity-analytics/population?entityType=Supplier&presetId=principal-sales-out-map`
- Preset: `principal-sales-out-map` (`AxisYKpiId = PRN-GRW-002`)
- Feature: [entity-analytics-developer-guide.md](../../features/entity-analytics/entity-analytics-developer-guide.md)

---

## Summary

The population map Y axis **is not missing data**. The `PRN-GRW-002` (Year-over-Year Growth Percentage) values are present and non-null in both the source snapshot (`BTRPD_PrincipalYoyGrowth`) and the materialized L0 table (`BTRPD_EntityAnalytics_Current`).

The Y axis renders as **zero for almost the entire population because the frontend projection pipeline clamps every negative value to 0 and then log-scales it**. `PRN-GRW-002` is a signed percentage (HigherIsBetter, can be negative), but the robust projection strategy was designed for non-negative metrics (IDR amounts, Days). Once clamped, all negative-growth principals collapse onto the Y = 0 line, and the axis percentile guides also resolve to ~0.

A secondary (data) condition amplifies the effect: in the demo snapshot the current comparison month (June 2026) is **incomplete** while the prior-year month (June 2025) is complete, so nearly every principal legitimately has a negative YoY. Even a corrected signed axis would show most principals below zero.

---

## Expected Behavior

1. The `principal-sales-out-map` Y axis should plot the true Year-over-Year Growth Percentage for each principal, including negative values.
2. Principal YoY values range across the real distribution (e.g. −0.64 to +2.89 on the current snapshot).
3. The Y axis tick labels should communicate that distribution (negative → positive), not a single value of `0`.

---

## Actual Behavior

1. The Y axis tick labels are all `0`.
2. Nearly every principal point is plotted on a single horizontal line at the bottom of the chart.
3. Hovering a point shows the **correct** value in the tooltip (e.g. `-63.55%`), because the tooltip reads `Point.FormattedAxisY` while the plot position reads the clamped projection value.

---

## Reproduction / Evidence (JUDE7 / `btr2`)

### 1. Source data exists — YoY snapshot is non-zero

```sql
SELECT COUNT(*) FROM BTRPD_PrincipalYoyGrowth;   -- 24
```

Sample rows (period 2026-06 vs 2025-06): values include `2.890071`, `0.021474`, `-0.097489`, … `-0.635539`, plus `NULL` where the prior-year month is absent.

### 2. Materialized L0 exists — `PRN-GRW-002` is non-null

```sql
SELECT KpiId, COUNT(*) Cnt, MIN(NumericValue) MinV, MAX(NumericValue) MaxV,
       SUM(CASE WHEN NumericValue = 0 THEN 1 ELSE 0 END) ZeroCnt,
       SUM(CASE WHEN NumericValue IS NULL THEN 1 ELSE 0 END) NullCnt
FROM BTRPD_EntityAnalytics_Current
WHERE EntityType = 'Supplier' AND KpiId = 'PRN-GRW-002';
-- Cnt = 24, MinV = -0.6355, MaxV = 2.8901, ZeroCnt = 0, NullCnt = 8
```

**15 of the 16 non-null values are negative.** None are zero in the database.

### 3. The current month is incomplete (data condition)

```sql
-- Max FakturDate in June 2026 = 2026-06-24 (36088 lines, Rp 4.465 B)
-- Max FakturDate in June 2025 = 2025-06-30 (57183 lines, Rp 7.277 B)
```

June 2026 is a partial month, so month-over-same-month YoY is structurally negative for nearly every principal.

### 4. The frontend clamps negatives before projecting

`btr.portal.web/src/services/populationProjection/strategies/robustProjectionStrategy.ts:54`

```ts
const businessX = Math.max(entity.businessX, 0)
const businessY = Math.max(entity.businessY, 0)
```

Then `businessToProjectedLog` (`robustStats.ts:52`) also clamps and applies `log10(max(value, 0) + 1)`:

```ts
const clamped = Math.max(value, 0)
...
return businessToLog(clamped)   // log10(clamped + 1)
```

For all negative YoY values this yields `log10(0 + 1) = 0`, so every negative principal projects to the same Y position.

### 5. Axis guides are derived from the clamped values

`buildAxisGuides` (`robustProjectionStrategy.ts:70-103`) computes percentiles from `businessYValues`, which are already clamped to ≥ 0. With 14 of 16 values at 0, the 10th/25th/50th/75th percentiles resolve to 0 and the 90th to ≈0.01, so the rendered Y tick labels are `0`.

The KPI unit is `Percent` (`SupplierEntityAnalyticsRegistrar.cs:332`), which is neither IDR nor Days, so the plain `businessToLog` branch applies — negatives are still clamped.

---

## Root Cause

### Primary cause — projection cannot represent signed metrics

The population map's robust projection strategy applies a **non-negative assumption** twice:

| Location | Statement | Effect |
|----------|-----------|--------|
| `robustProjectionStrategy.ts:54-55` | `Math.max(value, 0)` | Negative YoY → 0 |
| `robustStats.ts:52-60` (`businessToProjectedLog`) | `Math.max(value, 0)` + `log10(v + 1)` | Negative YoY → 0 |

The pipeline (`businessToLog` / `IDR_PROJECTION_FLOOR` / linear strategy stub) was built for value-like KPIs (Sales-Out IDR, Days). `PRN-GRW-002` is a **ratio that is legitimately negative**. The `LinearProjectionStrategy` that could handle signed domains exists but is deliberately unimplemented (`strategies/linearProjectionStrategy.ts:16`).

This is a **code/render defect, not missing data**.

### Secondary cause — incomplete demo month inflates the negative share

Because the June 2026 month is partial while June 2025 is complete, ~94% of principal YoY values are negative. This does not cause the zero axis, but it makes the defect affect almost the whole population and hides any positive outliers.

### Why the tooltip looks correct

`PopulationMapTooltip.vue` renders `point.FormattedAxisY`, which is the true signed value produced by the backend formatter. Only the plotted projection and the axis guides use the clamped value — hence the mismatch.

---

## Affected Workflow

| Workflow | Impact |
|----------|--------|
| Principal Investigation Workspace (`/analytics/suppliers`, `principal-sales-out-map`) | Y axis unusable for signed growth; all negative principals overlap |
| Any population map whose Y/X KPI can be negative (e.g. growth, variance, target gap) | Same defect class |
| Demo / executive presentation | Charts look empty/flat despite populated snapshots |

No worker, API, or database data is corrupted. `PRN-GRW-002` production/backfill is correct.

---

## Severity

| Dimension | Assessment |
|-----------|------------|
| Business impact | **Medium** — analytics are misrepresented, but underlying data is correct |
| Data integrity | **None** — read-only rendering defect |
| Frequency | **Every render** where a plotted KPI has negative values |
| User visibility | **High** — axis reads 0 for nearly all principals |

---

## Recommended Solution (for Architect)

Business rules are unchanged; this is a presentation-layer fix. Two independent workstreams are recommended.

### A. Support signed axis domains in projection (required — primary fix)

Pick one approach consistent with existing charting conventions:

1. **Route signed KPIs to a signed/linear projection domain.** When the KPI metadata `Unit`/value semantics indicate a percentage/delta (or when observed values include negatives), use an axis domain that spans `[min, max]` (with padding) instead of `[0, max]` on a log scale. Remove the unconditional `Math.max(value, 0)` clamp for these axes.
2. **Keep log projection only for strictly positive value-like metrics** (IDR amounts, Days); add a distinct strategy (the existing `LinearProjectionStrategy` stub is the intended slot) for signed metrics.

Requirements:
- Preserve current behavior for IDR/Days presets (regression tests).
- Axis guides must be computed from **unclamped** business values so tick labels reflect negative growth.
- `computeProjectionBounds` and confidence-band geometry must handle a domain crossing zero.
- `PopulationMapPoint.FormattedAxisY` (tooltip) stays as-is.

Suggested seam: introduce a per-axis projection mode resolved from KPI metadata (e.g. signed vs positive) in `EntityMapPresetRegistry` / preset DTO (`AxisYUnit`, KPI id) so the frontend selects the correct strategy per axis.

### B. Correct the demo data window (recommended — removes the amplification)

The demo/YoY signal is dominated by a partial current month. Before demo/reconciliation:

1. Set `Presentation.BusinessDate` to the **last complete month-end** present in the snapshot (e.g. `2026-05-31` if May is complete), **or**
2. Generate/restore data through the full final month so `June 2026` is not partial, **or**
3. For YoY specifically, exclude the in-progress month from being selected as the current period (or compare last complete month vs same month prior year).

Confirm the chosen business date is applied to the YoY snapshot by re-running the worker (see below).

### C. Add regression coverage (recommended)

- Unit: projection with a signed axis containing negative and positive values — assert distinct Y positions and tick labels below zero.
- Unit: existing positive-only IDR/Days projections unchanged.
- Fixture: `principal-sales-out-map` sample with a known negative-YoY principal renders off the zero line.

### Regeneration command (only needed for B, not for the render fix)

The Supplier/Principal analytics are produced by the **PurchasingManagement** domain; `PRN-GRW-002` is derived from the stored `PrnSalesOutHistory` + `PrnYoyGrowth` domains:

```powershell
.\btr.portal.worker.exe --domain PurchasingManagement --triggered-by Manual
# or, to refresh the full chain:
.\btr.portal.worker.exe --domain All --triggered-by Manual
```

---

## Verification Plan

| Check | Expected |
|-------|----------|
| `BTRPD_EntityAnalytics_Current` `PRN-GRW-002` for `Supplier` | 24 rows, 16 non-null, min −0.64, max +2.89 (unchanged) |
| Population map Y tick labels | Show negative and positive percentage values |
| Plot positions | Negative-growth principals below zero line, positive above |
| Tooltip value vs plot position | Consistent |
| IDR preset (e.g. `principal-sales-out-map` bubble / `customer-risk-map`) | Unchanged log behavior |
| Axis guide percentiles | Derived from unclamped values |

---

## Data Notes (JUDE7 / `btr2` at time of investigation)

| Item | Value |
|------|-------|
| Faktur data range | 2025-02-07 → 2026-06-24 (plus 2 stray test Faktur on 2026-09-14) |
| `BTRPD_PrincipalSalesOutHistory` | 321 rows, 2025-02 → 2026-06 |
| `BTRPD_PrincipalYoyGrowth` | 24 rows, period 2026-06 vs 2025-06 |
| `PRN-GRW-002` in L0 | 24 rows, 0 zero, 8 null, 15 negative, 1 positive |
| `PrnYoyGrowth` refresh log | Success, 2026-09-10 21:22:53 |

The 8 nulls are expected: those principals have no June 2025 Sales-Out, so YoY is undefined by design (`PrincipalYoyGrowthComposer.Calculate` returns null when prior ≤ 0).

---

## References

- `btr.portal.web/src/services/populationProjection/strategies/robustProjectionStrategy.ts:54`
- `btr.portal.web/src/services/populationProjection/robustStats.ts:34`
- `btr.portal.web/src/services/populationProjection/strategies/linearProjectionStrategy.ts:16`
- `btr.portal.web/src/components/entity-analytics/workspace/PopulationMapCanvas.vue:460`
- `btr.application/ReportingContext/EntityAnalyticsAgg/Services/EntityMapPresetRegistry.cs:78`
- `btr.application/ReportingContext/EntityAnalyticsAgg/Registrars/SupplierEntityAnalyticsRegistrar.cs:324`
- `btr.application/ReportingContext/EntityAnalyticsAgg/Producers/SupplierEntityAnalyticsProducer.cs:848`
- `btr.application/ReportingContext/PrincipalAnalyticsAgg/Services/PrincipalYoyGrowthComposer.cs:93`
- `btr.application/ReportingContext/PrincipalAnalyticsAgg/UseCases/RefreshPrincipalYoyGrowthSnapshotWorker.cs`
