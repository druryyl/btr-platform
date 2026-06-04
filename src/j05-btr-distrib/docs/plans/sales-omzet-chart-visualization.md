# Sales Omzet Chart Visualization — Implementation Plan

Reference for developers and AI agents implementing **chart and KPI visualization** on RO2 (`SalesOmzetInfoForm`). Read this together with the domain model in [`sales-omzet-aggregate-implementation.md`](sales-omzet-aggregate-implementation.md).

**Scope:** WinForms UI on existing RO2 read path (`ISalesOmzetDal` → `SalesOmzetView`). No changes to materialization, linker, or `BTR_SalesOmzet` schema in phase 1–2.

**Out of scope (this plan):** Omzet target master data, incentive calculation engine, scheduled reconcile, mobile/web.

---

## Purpose

Sales persons use RO2 to see **how much omzet counts toward monthly incentive**. Today the form is grid + Excel + materialize health only. This plan adds:

1. **KPI summary** — recognized omzet, transaction count, placeholder for target.
2. **Charts** — status breakdown and optional weekly pace.
3. **Consistent amount rules** — shared between KPI, chart, and (later) target comparison.

Primary users: **individual sales reps** (filter to own name). Secondary (later): **managers** comparing reps.

---

## Business rules for chart amounts

Charts must not mix `OrderTotal` and `FakturTotal` arbitrarily. Use one resolver everywhere (policy or static helper in `btr.application`).

| `OmzetStatus` | Amount field | Bucket label (ID) | Incentive / “Omzet diakui” |
|---------------|--------------|-------------------|----------------------------|
| `Completed` | `FakturTotal` | Selesai | **Yes** |
| `PendingOmzet` | `FakturTotal` | Menunggu omzet | No (pipeline; optional series) |
| `Outstanding` | `OrderTotal` | Order outstanding | No (pipeline; **Sales Period only**) |

### Period mode (existing checkbox)

| Mode | UI | Chart behavior |
|------|-----|----------------|
| **Omzet Period** (default) | *Periode Omzet* — `SalesPeriodCheckBox` **unchecked** | Only rows with real `OmzetDate` (same as SQL). Stacked chart effectively shows **Completed** only. KPI **Omzet diakui** = sum Completed `FakturTotal`. |
| **Sales Period** | *Periode Jual* — checkbox **checked** | Includes outstanding. **Stacked column** by status: Completed + Pending + Outstanding. KPI may show **Omzet diakui** + **Pipeline** (Pending + Outstanding). |

### Date used for weekly breakdown

| Mode | Group rows by |
|------|----------------|
| Omzet Period | `OmzetDate` (week of month) |
| Sales Period | Prefer `OrderDate` if outstanding; else `FakturDate` or `OmzetDate` — document in code: use **`SalesDate` equivalent from view**: for chart grouping use `OrderDate` when `OmzetStatus == Outstanding`, else `FakturDate` if set, else `OmzetDate`. *(View does not expose `SalesDate`; if needed add to `SalesOmzetView` + DAL in a small follow-up.)* |

**Agent note:** If weekly Sales Period grouping is ambiguous without `SalesDate` on the view, add `SalesDate` to `SalesOmzetDal` SELECT and `SalesOmzetView` before implementing phase 2 weekly chart.

### User-facing copy (subtitle under chart)

- Omzet Period: `Periode Omzet — hanya omzet yang sudah Kembali Faktur`
- Sales Period: `Periode Jual — termasuk order outstanding dan pending omzet`

Remind in tooltip: order Jan + Kembali Faktur Feb appears in **February** omzet period, not January.

---

## Recommended UX (target layout)

```
┌──────────────────────────────────────────────────────────────────┐
│ panel1: Tgl1, Tgl2, Periode Jual, Search, Materialisasi, Proses │
│ healthPanel (unchanged)                                           │
├──────────────────────────────────────────────────────────────────┤
│ kpiPanel: Omzet diakui | Transaksi | Target (—) | [optional %]   │
├─────────────────────────────┬────────────────────────────────────┤
│ chartPanel (Chart control)  │ InfoGrid (existing Syncfusion grid) │
│ + chart mode combo (optional)│                                    │
└─────────────────────────────┴────────────────────────────────────┘
```

- **Default split:** ~35% chart / 65% grid vertically, or horizontal split on wide screens — match `CustomerChartRpt` (`SplitContainer`).
- **Chart updates** when `Proses()` completes — same filtered list as grid (`_dataSource` after `Filter()`).
- **Excel export** unchanged; exports grid only.
- **Month shortcut (optional):** buttons or menu “Bulan ini” sets `Tgl1` = first of month, `Tgl2` = today.

### KPI labels (Indonesian)

| Control | Source |
|---------|--------|
| Omzet diakui | Sum Completed `FakturTotal` |
| Pipeline | Sum Pending `FakturTotal` + Outstanding `OrderTotal` (show only in Sales Period, or always with 0 in Omzet Period) |
| Transaksi | Count of rows in recognized sum (or count all visible rows — pick one and document; prefer **count contributing to Omzet diakui**) |
| Target | `—` until target feature; then `target` and `%` |

### Chart types by phase

| Phase | Chart | Type |
|-------|-------|------|
| 1 | Status breakdown | Stacked column (`SeriesChartType.StackedColumn`) |
| 2 | Weekly pace | Clustered column by ISO week label within `Tgl1`–`Tgl2` |
| 3 | Target | Horizontal bar + `StripLine` target, or cumulative line actual vs target |
| 4 | Manager | Horizontal bar top-N `SalesPersonName` (separate form or toggle “Semua sales”) |

**Avoid as primary:** pie for one rep; line per invoice; dual unrelated OrderTotal/FakturTotal series.

---

## Architecture

```
SalesOmzetInfoForm
  Proses() → ISalesOmzetDal.ListData(periode, mode)
          → Filter(search) → _dataSource
          → ISalesOmzetChartSummaryBuilder.Build(_dataSource, mode) → SalesOmzetChartSummary
          → Bind KPI labels + Chart series

btr.application/SalesContext/SalesOmzetAgg/
  Policies/ISalesOmzetChartAmountPolicy.cs   (or SalesOmzetChartAmountPolicy.cs)
  SalesOmzetChartSummary.cs
  Services/ISalesOmzetChartSummaryBuilder.cs (optional; can be static builder in phase 1)

btr.distrib/SalesContext/SalesPersonAgg/
  SalesOmzetInfoForm.cs          — KPI + chart bind
  SalesOmzetInfoForm.Designer.cs — kpiPanel, chartPanel, SplitContainer
```

**Principles (align with aggregate doc):**

- **Thin SQL, fat policy** — phase 1 aggregates in memory from `List<SalesOmzetView>`.
- **No UNION / no new read path** in phase 1.
- **Single amount resolver** — chart, KPI, and future target % must call the same policy.
- Reuse **`System.Windows.Forms.DataVisualization.Charting`** (see `btr.distrib/SharedForm/ChartHelper.cs`, `ReportingContext/CustomerChartRpt.cs`).

---

## Data model (application layer)

```csharp
// btr.application — names indicative
public sealed class SalesOmzetChartSummary
{
    public decimal RecognizedOmzet { get; init; }      // Completed FakturTotal
    public decimal PipelineOmzet { get; init; }       // Pending + Outstanding
    public int RecognizedTransactionCount { get; init; }
    public decimal? Target { get; init; }             // null until phase 3
    public IReadOnlyList<SalesOmzetStatusSlice> ByStatus { get; init; }
    public IReadOnlyList<SalesOmzetWeekSlice> ByWeek { get; init; }
}

public sealed class SalesOmzetStatusSlice
{
    public string Label { get; init; }               // e.g. "Selesai"
    public decimal Amount { get; init; }
    public SalesOmzetStatusEnum Status { get; init; }
}

public sealed class SalesOmzetWeekSlice
{
    public string WeekLabel { get; init; }           // e.g. "01–07 Mar"
    public decimal RecognizedAmount { get; init; }
}
```

### `ISalesOmzetChartAmountPolicy`

```csharp
decimal ResolveAmount(SalesOmzetView row);
bool IncludeInRecognizedTotal(SalesOmzetView row);
bool IncludeInPipelineTotal(SalesOmzetView row);
```

Implementation maps status → `FakturTotal` vs `OrderTotal` per table in **Business rules** above.

---

## Phased implementation

### Phase 1 — KPI + stacked status chart (MVP)

**Goal:** Sales rep sees total recognized omzet and status mix for filtered data.

**Tasks:**

1. Add `SalesOmzetChartAmountPolicy` + `SalesOmzetChartSummary` + builder in `btr.application`.
2. Unit tests in `btr.test/SalesContext/SalesOmzetChartSummaryTest.cs`:
   - Completed → `FakturTotal` in recognized.
   - Outstanding → `OrderTotal` in pipeline only.
   - Pending → pipeline, not recognized.
3. `SalesOmzetInfoForm.Designer.cs`:
   - Add `SplitContainer` (or second split): `kpiPanel` + `Chart` + existing `InfoGrid`.
   - Register `Chart` from `System.Windows.Forms.DataVisualization` (same as `CustomerChartRpt`).
4. `SalesOmzetInfoForm.cs`:
   - After `InfoGrid.DataSource = _dataSource`, call `RefreshChartAndKpi()`.
   - Bind KPI text; build stacked series from `ByStatus` with colors matching `GetStatusColor()` in form.
   - Chart subtitle from period mode.
   - Empty data → “Tidak ada data” annotation (pattern from `ChartForm<T>.ShowNoDataMessage`).
5. Manual test: Omzet Period month range; Sales Period same range with outstanding rows; search filter narrows chart.

**Acceptance:**

- [ ] KPI **Omzet diakui** matches sum of Completed `FakturTotal` in grid for same filter.
- [ ] Toggling *Periode Jual* changes chart subtitle and stacked segments.
- [ ] Proses with 0 rows clears chart and shows empty state.
- [ ] Grid behavior and Excel export unchanged.

---

### Phase 2 — Weekly pace chart

**Goal:** Show recognized omzet per week within selected period.

**Tasks:**

1. If needed: add `SalesDate` to `SalesOmzetView` and `SalesOmzetDal` SELECT (for consistent Sales Period weekly buckets).
2. Extend builder: `ByWeek` grouped by calendar week; only **recognized** amounts in Omzet Period mode; in Sales Period mode document whether week chart is recognized-only or includes pipeline (recommend **recognized-only** for incentive pace, pipeline as separate optional series).
3. UI: `ComboBox` or toggle — `Status` | `Mingguan` chart mode.
4. Format week labels in Indonesian short date (`dd MMM`).

**Acceptance:**

- [ ] Full-month Omzet Period shows 4–5 week buckets summing to KPI **Omzet diakui**.
- [ ] Partial month (`Tgl2` = today) only includes weeks intersecting range.

---

### Phase 3 — Omzet target integration

**Prerequisite:** Target storage (table + CRUD) — separate plan; not defined here.

**Goal:** KPI shows target and %; chart shows target line or cumulative vs target.

**Tasks:**

1. `ISalesOmzetTargetDal` (or extend sales person aggregate) — `GetTarget(salesPersonId, year, month)`.
2. Pass `decimal? target` into `SalesOmzetChartSummary`.
3. KPI: `Target: Rp …` and `Tercapai: nn%` (cap display at 100% or show over-achievement — product decision).
4. Chart: `ChartArea.AxisY.StripLines` for target on status chart, or second series on weekly cumulative line.

**Acceptance:**

- [ ] Rep with target sees % on KPI; without target sees `—`.
- [ ] Changing month reloads target for filtered sales person (requires identity or explicit sales person on form).

---

### Phase 4 — Manager comparison (optional)

**Goal:** Compare recognized omzet across sales persons for same period.

**Tasks:**

1. When search is empty, horizontal bar chart: top 15 `SalesPersonName` by `RecognizedOmzet` (Omzet Period).
2. Consider separate menu item `RO2 - Sales Omzet (Pusat)` to avoid confusing reps — product decision.

**Acceptance:**

- [ ] Manager view sorted descending; clicking bar filters grid to that rep (optional enhancement).

---

## Sales person scoping (security / UX)

RO2 currently lists **all** reps; filtering is via `SearchText` only.

| Approach | When |
|----------|------|
| **A. Default search** to logged-in user’s `SalesPersonName` on form load | Quick; not security |
| **B. DAL filter** `WHERE SalesPersonName = @current` for role Sales | Stronger |

**Agent:** Check how `MainForm` / login exposes current user and sales person link before phase 1. Document chosen approach in PR. Chart should never show other reps’ KPI unless user clears filter (manager).

---

## Chart technology

| Option | Decision |
|--------|----------|
| `System.Windows.Forms.DataVisualization.Charting` | **Use** — already in project (`ChartHelper`, `CustomerChartRpt`) |
| Syncfusion chart | Not required; grid already Syncfusion |
| New NuGet | Avoid unless team standard changes |

**Colors (match `SalesOmzetInfoForm.GetStatusColor`):**

| Status | Color |
|--------|-------|
| Outstanding | `MistyRose` |
| Pending omzet | `LightGoldenrodYellow` |
| Completed | `PaleGreen` |
| Direct sales (Completed + DirectSale) | `PowderBlue` — optional 4th stack segment or merge into Selesai |

For stacked chart, either map Direct Sales as separate slice or include in **Selesai** — prefer **four slices** if `SaleKind` is available on view (already is).

---

## Testing

| Layer | What |
|-------|------|
| **Unit** | `SalesOmzetChartAmountPolicy`, summary builder (status sums, empty list, single row) |
| **Unit** | Week grouping edge cases (month boundary) |
| **Manual** | Scenarios from aggregate doc verification table (Jan order, Feb Kembali Faktur — Omzet Period Feb only) |
| **Manual** | Materialize health ≠ Good — chart still works; user warned separately |

Existing tests: `SalesOmzetPoliciesTest.cs`, `SalesOmzetReconcileTest.cs` — do not duplicate reconcile tests here.

---

## Files to touch (checklist)

| File | Phase |
|------|-------|
| `docs/plans/sales-omzet-chart-visualization.md` | — (this doc) |
| `btr.application/.../Policies/SalesOmzetChartAmountPolicy.cs` | 1 |
| `btr.application/.../SalesOmzetChartSummary.cs` | 1 |
| `btr.application/.../Services/SalesOmzetChartSummaryBuilder.cs` | 1 |
| `btr.test/SalesContext/SalesOmzetChartSummaryTest.cs` | 1 |
| `btr.distrib/.../SalesOmzetInfoForm.cs` | 1–2 |
| `btr.distrib/.../SalesOmzetInfoForm.Designer.cs` | 1–2 |
| `btr.application/.../OrderFeature/ISalesOmzetDal.cs` (`SalesOmzetView`) | 2 if `SalesDate` added |
| `btr.infrastructure/.../SalesOmzetDal.cs` | 2 |
| Target DAL + entity | 3 |

**DI (`Program.cs`):** Register `ISalesOmzetChartAmountPolicy` and builder as scoped if interfaces used.

---

## Dependencies and constraints

- Report period max **122 days** (~3 months) — in-memory aggregation is acceptable.
- Chart reads same data as grid — run **Materialisasi** when health ≠ Good (existing UX).
- Do **not** reintroduce UNION read path in `SalesOmzetDal`.
- Phase 3 target feature depends on business schema not yet in system.

---

## Verification scenarios (chart-specific)

| Scenario | Period mode | Expected KPI / chart |
|----------|-------------|----------------------|
| 10 Completed rows, `FakturTotal` sum 100M | Omzet | KPI 100M; stack only Selesai |
| 5 Outstanding, no faktur | Omzet | Hidden from list (strict) — KPI 0 |
| Same 5 Outstanding | Sales | Pipeline > 0; stack Outstanding segment |
| Pending (faktur, no Kembali Faktur) | Sales | Pipeline; not in Omzet diakui |
| Jan order, Feb Kembali Faktur | Omzet, Feb range | Row in Feb; not in Jan chart |
| Search filters one rep | Either | KPI/chart only that rep’s rows |
| Void rows | Either | Excluded (DAL already `OmzetStatus <> 'Void'`) |

---

## Related code references

| Topic | File |
|-------|------|
| Domain / period rules | [`sales-omzet-aggregate-implementation.md`](sales-omzet-aggregate-implementation.md) |
| Report UI | `btr.distrib/SalesContext/SalesPersonAgg/SalesOmzetInfoForm.cs` |
| Read DAL | `btr.infrastructure/SalesContext/SalesPersonAgg/SalesOmzetDal.cs` |
| View model | `btr.application/SalesContext/OrderFeature/ISalesOmzetDal.cs` |
| Period policy | `btr.application/.../Policies/SalesOmzetPeriodPolicy.cs` |
| Chart examples | `btr.distrib/SharedForm/ChartHelper.cs`, `ReportingContext/CustomerChartRpt.cs` |
| Status colors | `SalesOmzetInfoForm.GetStatusColor` |

---

## Summary for agents

1. Implement **amount policy first**, then **summary builder**, then **UI bind**.
2. Default UX = **Omzet Period** + **Omzet diakui** KPI + **stacked status** chart; **Periode Jual** adds pipeline segments.
3. Reuse **WinForms Chart** control; keep grid as drill-down.
4. **Phase 1** is complete when KPI matches manual sum of Completed rows; no DB schema changes required.
5. Add **`SalesDate` to view** if weekly Sales Period grouping is required in phase 2.
6. **Target** is phase 3 after separate target data exists.
