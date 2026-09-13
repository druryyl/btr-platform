# Investigation: Principal Investigation Workspace — Purchase Exposure Map Scatter Chart Empty

**Date:** 2026-09-13
**Reporter:** Product / Demo preparation
**Environment:** `btr.portal.web` (Investigation Workspace) against `btr.portal.api`
**Affected component:** Entity Analytics population map Y axis for presets that point at a **numeric `EA-DIM-*` dimension**

Related:
- Endpoint: `GET /api/entity-analytics/population?entityType=Supplier&presetId=purchase-exposure-map`
- Preset: `purchase-exposure-map` (`AxisXKpiId = PU-KPI-001`, `AxisYKpiId = EA-DIM-INVENTORY-VALUE`)
- Sibling incident: [principal-yoy-growth-map-zero-axis](../principal-yoy-growth-map-zero-axis/investigation.md) (same chart, different cause — projection clamping, not this data bug)
- Feature: [entity-analytics-developer-guide.md](../../features/entity-analytics/entity-analytics-developer-guide.md)

---

## Summary

The Purchase Exposure Map scatter chart is **empty** because the Y axis is `null` for every point. This is **not missing source data** and **not a frontend defect**.

The L0 producer writes numeric entity dimensions (such as `EA-DIM-INVENTORY-VALUE`) into the `NumericValue` column and leaves `TextValue` null. The population map reader for dimensions (`GetCurrentDimensionPopulation`) reads the value from `TextValue` only, so it returns `null` for every numeric dimension. The engine then marks every point `IsLowConfidence = true`, and the canvas only plots points where **both** `AxisX` and `AxisY` are non-null — so nothing is drawn.

The X axis works because `PU-KPI-001` is a real KPI, read by a different query (`GetCurrentKpiPopulation`) that reads `NumericValue`.

---

## Expected Behavior

1. `GET /api/entity-analytics/population?...&presetId=purchase-exposure-map` returns non-null `AxisY` for suppliers that have an inventory value.
2. Suppliers with both axes present are plotted as scatter points; only entities genuinely missing one axis appear in the low-confidence strip.

## Actual Behavior

1. Every one of the 25 returned points has `AxisY: null`, `FormattedAxisY: "—"`, `AxisYPercentile: null`, `IsLowConfidence: true`.
2. `AxisX` is populated for most suppliers (e.g. `SP021` = `588435939.69`, `SP030` = `1452763549.14`), proving the population and the X pipeline are healthy.
3. The scatter canvas filters out all points, so the chart renders empty.

---

## Reproduction / Evidence

### 1. Endpoint response (excerpt)

```json
{
  "EntityType": "Supplier",
  "PresetId": "purchase-exposure-map",
  "AxisXKpiId": "PU-KPI-001",
  "AxisYKpiId": "EA-DIM-INVENTORY-VALUE",
  "AxisXLabel": "MTD Purchase",
  "AxisYLabel": "Inventory Value",
  "TotalPopulationCount": 25,
  "FilteredPopulationCount": 25,
  "Points": [
    { "EntityId": "SP021", "AxisX": 588435939.6900, "AxisY": null,
      "AxisXPercentile": 81.25, "AxisYPercentile": null, "IsLowConfidence": true },
    { "EntityId": "SP030", "AxisX": 1452763549.1400, "AxisY": null,
      "AxisXPercentile": 100.0, "AxisYPercentile": null, "IsLowConfidence": true }
  ]
}
```

All 25 points follow the same pattern; `AxisY` never has a value.

### 2. Writer stores numeric dimensions in `NumericValue`

`btr.application/ReportingContext/EntityAnalyticsAgg/Producers/SupplierEntityAnalyticsProducer.cs:2288-2297`

```csharp
if (supplier.InventoryValue.HasValue)
{
    rows.Add(CreateRow(
        entityId,
        entityCode,
        EntityAnalyticsMetaKpiIds.InventoryValue,   // "EA-DIM-INVENTORY-VALUE"
        supplier.InventoryValue,                    // -> NumericValue
        null,                                       // -> TextValue left null
        generatedAt));
}
```

The generic numeric dimension overload does the same (`SupplierEntityAnalyticsProducer.cs:2331-2343`):

```csharp
private static void AddDimension(..., decimal? value, ...)
{
    if (!value.HasValue) return;

    rows.Add(CreateMetaRow(entityId, entityCode, dimensionKpiId, value, null, generatedAt));
}
```

Applied to `EA-DIM-PURCHASE-SHARE` (`:2302`) and `EA-DIM-AT-RISK-VALUE` (`:2303`). `EA-DIM-CATALOG-PENETRATION` is written via the same decimal overload (`:2306`).

### 3. Reader only reads `TextValue`

`btr.infrastructure/ReportingContext/EntityAnalyticsAgg/EntityAnalyticsRepository.cs:1816-1848`

```sql
SELECT c.EntityId,
       c.EntityCode,
       TRY_CAST(c.TextValue AS DECIMAL(18,4)) AS NumericValue,   -- TextValue is NULL for numeric dims
       ...
FROM BTRPD_EntityAnalytics_Current c
...
WHERE c.SnapshotKey = @SnapshotKey
  AND c.EntityType = @EntityType
  AND c.KpiId = @DimensionKpiId
```

For `EA-DIM-INVENTORY-VALUE` the stored row has `NumericValue = <value>` and `TextValue = NULL`, so `TRY_CAST(NULL ...)` yields `NULL`.

### 4. Contrast: the KPI reader works

`EntityAnalyticsRepository.GetCurrentKpiPopulation` (`EntityAnalyticsRepository.cs:1762-1792`) selects `c.NumericValue` directly for KPI rows. `PU-KPI-001` is a KPI (`EntityAnalyticsMetaKpiIds.IsMetaOrDimension` returns false), so the X axis is correctly populated.

### 5. Engine routes dimensions to the broken reader

`btr.application/ReportingContext/EntityAnalyticsAgg/Services/EntityPopulationMapEngine.cs:160-167`

```csharp
private Dictionary<string, decimal?> GetKpiValueMap(string entityType, string kpiId)
{
    var rows = EntityAnalyticsMetaKpiIds.IsMetaOrDimension(kpiId)
        ? _repository.GetCurrentDimensionPopulation(entityType, kpiId)   // <-- numeric dims read as NULL
        : _repository.GetCurrentKpiPopulation(entityType, kpiId);

    return ToValueMap(rows);
}
```

The point is flagged low-confidence when either axis is missing (`EntityPopulationMapEngine.cs:116`):

```csharp
IsLowConfidence = !axisX.HasValue || !axisY.HasValue,
```

### 6. Frontend only plots dual-axis points

`btr.portal.web/src/components/entity-analytics/workspace/PopulationMapCanvas.vue:203-205`

```ts
const visiblePoints = computed(() =>
  (props.population?.Points ?? []).filter((p) => p.AxisX != null && p.AxisY != null),
)
```

With `AxisY` always null, `visiblePoints` is empty, the projection is null, and the axis/label drawing bails early. All points are diverted to the low-confidence strip lane (`lowConfidenceRenderable`, `:152-161`), so the scatter area is blank.

---

## Root Cause

**Primary cause — write/read column mismatch for numeric `EA-DIM-*` dimensions.**

| Layer | Location | Behavior |
|-------|----------|----------|
| Write | `SupplierEntityAnalyticsProducer.cs:2288-2297`, `:2331-2343` | Numeric dimension value -> `NumericValue`; `TextValue = null` |
| Read | `EntityAnalyticsRepository.cs:1826` | `TRY_CAST(TextValue AS DECIMAL(18,4))` -> `NULL` |
| Route | `EntityPopulationMapEngine.cs:162-164` | Any `EA-DIM-*` axis uses the dimension reader |
| Flag | `EntityPopulationMapEngine.cs:116` | Missing Y -> `IsLowConfidence = true` |
| Render | `PopulationMapCanvas.vue:203-205` | Requires `AxisX != null && AxisY != null` -> 0 points |

The repository already uses the correct fallback pattern for entity identity dimensions at `EntityAnalyticsRepository.cs:159-161`:

```csharp
var value = !string.IsNullOrWhiteSpace(row.TextValue)
    ? row.TextValue
    : row.NumericValue?.ToString();
```

`GetCurrentDimensionPopulation` simply never adopted the equivalent fallback in SQL.

This is a **code defect (data-read bug), not missing data**. The source `NumericValue` is present and correct; only the read path ignores it.

---

## Affected Workflow

The bug affects **any population map, radar, or peer-distribution view whose axis/KPI points at a numeric `EA-DIM-*` dimension**.

| Preset / consumer | Numeric dimension | Impact |
|-------------------|-------------------|--------|
| `purchase-exposure-map` (Supplier) | `EA-DIM-INVENTORY-VALUE` | Y axis empty -> chart blank |
| `purchasing-discipline-map` (Supplier) | `EA-DIM-AT-RISK-VALUE` | Y axis empty -> chart blank |
| Any preset using `EA-DIM-PURCHASE-SHARE` | `EA-DIM-PURCHASE-SHARE` | Axis empty |
| Any preset using `EA-DIM-CATALOG-PENETRATION` | `EA-DIM-CATALOG-PENETRATION` | Axis empty |
| `EntityRadarEngine.cs:409` | any numeric `EA-DIM-*` axis | Radar axis empty |
| `EntityPeerDistributionEngine.cs:54` | any numeric `EA-DIM-*` KPI | Distribution empty |

String/text dimensions (`EA-DIM-ACTIVE-MTD`, `EA-DIM-ACTIVE-SKU-COUNT`, `EA-DIM-ATTENTION-SIGNALS`, `EA-DIM-WILAYAH`, `EA-DIM-SALESMAN`, etc.) store their value in `TextValue` and are **not** affected.

No worker, API, or database data is corrupted. The materialized snapshot is correct.

---

## Why Existing Tests Missed It

- `btr.test/ReportingContext/EntityPopulationMapEngineTest.cs` seeds its axis values as **KPI** rows (`CU-KPI-009`, `CU-KPI-010`) at lines 284-302, which route through `GetCurrentKpiPopulation` (reads `NumericValue`). No test exercises a numeric `EA-DIM-*` axis through the engine.
- The test repository stub mirrors the buggy read: `btr.test/ReportingContext/EntityAnalyticsRepositoryStubBase.cs:812-837` parses only `r.TextValue` for dimensions and ignores `r.NumericValue`.
- Consequently unit tests never reproduce the production write/read mismatch.

---

## Severity

| Dimension | Assessment |
|-----------|------------|
| Business impact | **High** — a headline workspace chart renders blank for the Purchasing lens |
| Data integrity | **None** — read-only rendering defect; snapshot data correct |
| Frequency | **Every render** of any preset using a numeric dimension axis |
| User visibility | **High** — chart appears empty with no error |
| Blast radius | Multiple presets and two additional engines share the faulty query |

---

## Resolution (Implemented 2026-09-13)

The recommended read-side fix was implemented:

- `EntityAnalyticsRepository.cs:1826` now selects `COALESCE(c.NumericValue, TRY_CAST(c.TextValue AS DECIMAL(18,4))) AS NumericValue`.
- `EntityAnalyticsRepositoryStubBase.GetCurrentDimensionPopulation` (`:812-837`) now mirrors the same fallback (`r.NumericValue ?? parsed TextValue`).
- Regression tests added in `EntityPopulationMapEngineTest`: numeric `EA-DIM-INVENTORY-VALUE` in `NumericValue`, numeric text in `TextValue`, and non-numeric text still null / low-confidence.

No source data regeneration is required. Existing text-dimension behavior is preserved.

---

## Recommended Solution (for Architect)

Business rules are unchanged; this is a data-access fix. No source data regeneration is required.

### A. Fix the dimension read query (required — primary fix)

`btr.infrastructure/ReportingContext/EntityAnalyticsAgg/EntityAnalyticsRepository.cs:1823-1836`

Change the select to fall back to `NumericValue`, so both numeric and text dimensions resolve:

```sql
SELECT c.EntityId,
       c.EntityCode,
       COALESCE(c.NumericValue, TRY_CAST(c.TextValue AS DECIMAL(18,4))) AS NumericValue,
       CASE WHEN COALESCE(active.NumericValue, 1) > 0 THEN 1 ELSE 0 END AS IsActive
FROM BTRPD_EntityAnalytics_Current c
...
```

Rationale:
- Numeric dimensions store the authoritative value in `NumericValue`; text dimensions store a parseable number in `TextValue`.
- `COALESCE(NumericValue, ...)` prefers the typed column and only parses text when needed.
- Works against the **already-persisted** snapshot; no re-run of the Supplier/Principal analytics worker is necessary.
- Aligns with the existing fallback intent at `EntityAnalyticsRepository.cs:159-161`.

Requirements:
- Preserve behavior for text dimensions (including non-numeric text such as `ACTIVE-MTD = "Yes"`, which yields `NULL`, as today).
- No API/DTO change; `PopulationMapPointDto.AxisY` already nullable and formatted server-side.

### B. Mirror the fallback in the test stub (required for meaningful coverage)

`btr.test/ReportingContext/EntityAnalyticsRepositoryStubBase.cs:812-837` — update `GetCurrentDimensionPopulation` to return `r.NumericValue ?? parsed TextValue`, matching the production query, otherwise regression tests written against the stub will not catch this class of defect.

### C. Add regression coverage (recommended)

- Unit (`EntityPopulationMapEngineTest`): seed a preset rule / dimension-driven entity where the Y-axis `EA-DIM-*` row carries **only** `NumericValue`; assert `AxisY` is non-null and `IsLowConfidence == false`.
- Unit: seed a text dimension (e.g. `ACTIVE-MTD = "Yes"`) and assert it still yields `NULL` (no accidental parse).
- Fixture: `purchase-exposure-map` sample renders non-empty when both axes resolve.

### Rejected alternative

Writing numeric dimensions into `TextValue` (in addition to or instead of `NumericValue`) would also fix the read, but:
- requires regenerating every snapshot (backfill), and
- is semantically wrong — `NumericValue` is the typed column other consumers rely on (`GetCurrentKpiPopulation`, ranking engines).

The read-side fix is smaller, non-destructive, and future-proof.

---

## Verification Plan

| Check | Expected |
|-------|----------|
| `BTRPD_EntityAnalytics_Current` for `Supplier` / `EA-DIM-INVENTORY-VALUE` | Rows present with non-null `NumericValue` (unchanged) |
| `GET /api/entity-analytics/population?entityType=Supplier&presetId=purchase-exposure-map` | `AxisY` non-null for suppliers with inventory; `AxisYPercentile` populated; `IsLowConfidence` false where both axes exist |
| Scatter chart | Points rendered; only genuinely null-axis suppliers in the low-confidence strip |
| `purchasing-discipline-map` | `EA-DIM-AT-RISK-VALUE` Y axis populates |
| Existing text-dimension presets | Unchanged (`ACTIVE-MTD`, `WILAYAH`, etc.) |
| `dotnet test` Entity Analytics suite | Green, including new regression tests |
| Radar / peer-distribution views using numeric dimensions | Axes populate (same query fix) |

---

## References

- `btr.portal.api/Controllers/EntityAnalytics/EntityAnalyticsController.cs:140-171`
- `btr.application/ReportingContext/EntityAnalyticsAgg/Queries/GetInvestigationWorkspaceQueries.cs:78-110`
- `btr.application/ReportingContext/EntityAnalyticsAgg/Queries/InvestigationWorkspaceQueries.cs:58-99`
- `btr.application/ReportingContext/EntityAnalyticsAgg/Services/EntityPopulationMapEngine.cs:160-167`, `:116`
- `btr.application/ReportingContext/EntityAnalyticsAgg/Services/EntityMapPresetRegistry.cs:89-99`
- `btr.application/ReportingContext/EntityAnalyticsAgg/Models/EntityAnalyticsMetaKpiIds.cs:33-37`
- `btr.application/ReportingContext/EntityAnalyticsAgg/Producers/SupplierEntityAnalyticsProducer.cs:2288-2297`, `:2302-2306`, `:2331-2343`
- `btr.infrastructure/ReportingContext/EntityAnalyticsAgg/EntityAnalyticsRepository.cs:1826`, `:159-161`, `:1762-1792`
- `btr.application/ReportingContext/EntityAnalyticsAgg/Services/EntityRadarEngine.cs:409`
- `btr.application/ReportingContext/EntityAnalyticsAgg/Services/EntityPeerDistributionEngine.cs:54`
- `btr.portal.web/src/components/entity-analytics/workspace/PopulationMapCanvas.vue:203-205`, `:152-161`
- `btr.test/ReportingContext/EntityAnalyticsRepositoryStubBase.cs:812-837`
- `btr.test/ReportingContext/EntityPopulationMapEngineTest.cs:284-302`
