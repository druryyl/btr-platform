# Sales Omzet Materialization — Domain Reference

Reference for developers and AI agents working on **Sales Omzet** (`BTR_SalesOmzet`): what it is, how it relates to Order and Faktur, and where business rules live in code.

**Related:** operational deploy/backfill → [`docs/ops/sales-omzet-deploy.md`](../ops/sales-omzet-deploy.md); chart/KPI UI → [`sales-omzet-chart-visualization.md`](sales-omzet-chart-visualization.md)

---

## Purpose

RO2 (Sales Omzet report) used to build rows with a complex on-the-fly SQL `UNION` over `BTR_Order` and `BTR_Faktur`. That was hard to maintain and produced incorrect rows when period rules did not match business intent.

The replacement is a **materialized aggregate**:

- Each sale thread has its own **`SalesOmzetId`** (not equal to `OrderId` or `FakturId`).
- Rows are **created, linked, and refreshed** only by **`ReconcileSalesOmzetWorker`** (materialization).
- The report **reads** `BTR_SalesOmzet` with a period filter (Omzet Period vs Sales Period).
- **Create Order** and **Save Faktur** are **not** modified; they do not write to `BTR_SalesOmzet`.

This is a **current-state** model: one row per sale thread, updated when materialization runs—not an event history or immutable snapshot per report run.

---

## Core concepts

### Order >> Faktur (ordered sale)

Normal flow: order first, faktur later. The aggregate row may start with only order fields filled; faktur and omzet recognition are added on later materialize runs.

### Direct sale

Faktur exists without an order (`OrderId` empty). The aggregate is **`DirectSale`** with `SalesDate` set at create (typically `FakturDate`).

### Materialization vs reporting

| Activity | What it does |
|----------|----------------|
| **Materialization (reconcile)** | Syncs `BTR_SalesOmzet` from orders, fakturs, customers, faktur control status. |
| **Reporting (RO2 Proses)** | Reads existing aggregate rows; **does not** reconcile. |

Users refresh data via **`SalesOmzetMaterializeForm`** (*Materialisasi* / *Rebuild Semua*). **`SalesOmzetInfoForm` Proses** only queries the table (fast).

---

## Business vocabulary

| Term (ID) | Code / field | Meaning |
|-----------|--------------|---------|
| **Tanggal Jual** | `SalesDate` | When the sale **thread started** for **Sales Period** filtering. Set **once** at create; **never** updated on refresh. |
| **Tanggal Omzet** | `OmzetDate` | When omzet is **recognized** — **not** `FakturDate`. |
| **Ordered Sale** | `SaleKind = OrderedSale` | Started from an order (`OrderId` set). |
| **Direct Sale** | `SaleKind = DirectSale` | Faktur without order (`FakturId` set, no `OrderId`). |
| **Periode Omzet** | `SalesOmzetPeriodFilterMode.OmzetPeriod` | Filter list by `OmzetDate` in range (**strict omzet**). |
| **Periode Jual** | `SalesOmzetPeriodFilterMode.SalesPeriod` | Filter list by `SalesDate` in range. |

### OmzetDate (recognition)

From `FakturControlModel.ListStatus`:

1. Find `FakturControlStatusModel` where `StatusFaktur == StatusFakturEnum.KembaliFaktur` (faktur received at main office / sales-admin).
2. `OmzetDate = that status’s StatusDate`.
3. If no KembaliFaktur: `OmzetDate =` sentinel (`3000-01-01`) — not recognized yet.

References: `btr.domain/SalesContext/FakturControlAgg/StatusFakturEnum.cs`, `FakturControlStatusModel.cs`.

Use the **enum name** in code—not magic number `2` (which maps to `KembaliFaktur` in the enum order).

`FakturDate` on the aggregate is for **display only**, not omzet recognition.

### SalesDate (frozen — Option A)

| Sale kind | Set on create | Updated on refresh? |
|-----------|---------------|---------------------|
| **OrderedSale** | `SalesDate = OrderDate` | **Never** |
| **DirectSale** | `SalesDate = FakturDate` | **Never** |

This keeps January ordered sales in January’s **Sales Period** even when KembaliFaktur happens in February.

### Period filters (RO2)

| Mode | UI | SQL (via `ISalesOmzetPeriodPolicy`) | Outstanding rows (no KembaliFaktur) |
|------|-----|-------------------------------------|-------------------------------------|
| **Omzet Period** (default) | *Periode Omzet* — checkbox **unchecked** | `OmzetDate` in `[Tgl1,Tgl2]` and not sentinel | **Hidden** (strict omzet) |
| **Sales Period** | *Periode Jual* — checkbox **checked** | `SalesDate` in `[Tgl1,Tgl2]` | **Shown** in that sales month |

Reconcile **scope** (which sources to process) is **independent** of this read filter. Linking faktur to an order uses `GetFakturByOrderId` **without** restricting faktur date to the UI period.

---

## Sale kind and status

### `SaleKindEnum`

| Value | `OrderId` | `FakturId` |
|-------|-----------|------------|
| `OrderedSale` | set | optional |
| `DirectSale` | empty | set |

### `SalesOmzetStatusEnum` (typical)

| Condition | Status |
|-----------|--------|
| Ordered sale, no faktur | `Outstanding` |
| Faktur, no KembaliFaktur (`OmzetDate` sentinel) | `PendingOmzet` |
| KembaliFaktur recorded | `Completed` |
| Void / ineligible | `Void` (excluded from RO2 list: `OmzetStatus <> 'Void'`) |

Excel/grid labels map from `OmzetStatus` + `SaleKind` (e.g. Completed + DirectSale → “Direct Sales”).

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│  SalesOmzetInfoForm (RO2 report)                                 │
│  Proses → ISalesOmzetDal.ListData(periode, periodMode)           │
│  Materialisasi → reconcile each ISO week in report period + health │
│  panel1 BackColor = worst-bucket health (no text labels)           │
└────────────────────────────┬────────────────────────────────────┘
                             │ read
                             ▼
                    BTR_SalesOmzet
                             ▲
                             │ write (only)
┌────────────────────────────┴────────────────────────────────────┐
│  ReconcileSalesOmzetWorker                                       │
│    ISalesOmzetSourceDal → orders/fakturs/customers/status        │
│    ISalesOmzetLinker → find/create/link + Refresh                │
│    ISalesOmzetSnapshotBuilder + policies → field values & rules  │
│    ISalesOmzetWriter → SalesOmzetId (prefix SO)                    │
└─────────────────────────────────────────────────────────────────┘
                             ▲
┌────────────────────────────┴────────────────────────────────────┐
│  SalesOmzetMaterializeForm (admin)                               │
│  Rebuild Semua → Full reconcile (all orders/fakturs)             │
└─────────────────────────────────────────────────────────────────┘
```

### Design principles

| Principle | Implication |
|-----------|-------------|
| **Thin SQL, fat policy** | DAL reads/writes rows; rules in `Policies/`. |
| **One row per sale thread** | Unique `OrderId` / `FakturId` (filtered indexes). |
| **Single writer** | Only reconcile (via linker/writer) mutates aggregate data. |
| **Period = read concern** | `ISalesOmzetPeriodPolicy` for RO2 only. |
| **Freeze SalesDate once** | `SetSalesDateOnCreate` in linker only. |

---

## Database: `BTR_SalesOmzet`

Script: `btr.sql/Tables/SalesContext/BTR_SalesOmzet.sql`

| Column | Type | Notes |
|--------|------|--------|
| `SalesOmzetId` | `VARCHAR(13)` PK | From `BTR_ParamNo` prefix `SO`, `IDFormatEnum.PFnnn` |
| `OrderId` | `VARCHAR(26)` | Unique when non-empty |
| `FakturId` | `VARCHAR(13)` | Unique when non-empty |
| `SaleKind` | `VARCHAR(15)` | `OrderedSale` / `DirectSale` |
| `SalesDate` | `DATETIME` | Tanggal Jual; frozen |
| `OmzetDate` | `DATETIME` | KembaliFaktur `StatusDate` |
| `SalesPersonName` | `VARCHAR(30)` | |
| `OrderDate`, `OrderTotal` | | From order (`TotalAmount`, not line sum) |
| `FakturCode`, `FakturDate`, `FakturTotal` | | From faktur |
| `CustomerName`, `Code`, `Alamat` | | Customer denormalized |
| `OmzetStatus` | `VARCHAR(20)` | Enum string |
| `CreatedAt`, `LastReconciledAt` | | Audit |

Indexes: `UX` on `OrderId` / `FakturId`; `IX` on `SalesDate`, `OmzetDate`.

Sentinel convention: `SalesOmzetDates.Sentinel` = `3000-01-01` (also void faktur: `VoidDate` must be sentinel to be eligible).

---

## Code layout

```
btr.sql/Tables/SalesContext/BTR_SalesOmzet.sql
btr.sql/Tables/SalesContext/BTR_SalesOmzetHealthWeekly.sql
btr.sql/DataSeeds/BTR_ParamNo_SalesOmzet.sql
btr.sql/DataSeeds/BTR_ParamNo_SalesOmzetHealthWeekly.sql

btr.domain/SalesContext/SalesOmzetAgg/
  SalesOmzetModel.cs, ISalesOmzetKey.cs
  SaleKindEnum.cs, SalesOmzetStatusEnum.cs
  SalesOmzetPeriodFilterMode.cs, ReconcileSalesOmzetScope.cs

btr.domain/SalesContext/SalesOmzetHealthWeeklyAgg/
  SalesOmzetHealthWeeklyModel.cs, SalesOmzetHealthLevelEnum.cs, IsoWeekIdentifier.cs

btr.application/SalesContext/SalesOmzetAgg/
  Contracts/     ISalesOmzetEntityDal, ISalesOmzetSourceDal
  Policies/      Eligibility, Period, Status, SaleKind, SnapshotBuilder
  Services/      ISalesOmzetLinker
  UseCases/      ReconcileSalesOmzetWorker, Request, Result
  Workers/       SalesOmzetWriter
  Snapshots/     OrderSnapshot, FakturSnapshot, ...
  SalesOmzetDates.cs

btr.application/SalesContext/SalesOmzetHealthWeeklyAgg/
  Contracts/     ISalesOmzetHealthMetricsDal, ISalesOmzetHealthWeeklyDal
  Policies/      ISalesOmzetHealthPolicy, ISalesOmzetReportHealthResolver
  Services/      IIsoWeekCalendar (IsoWeekCalendar)
  UseCases/      GenerateSalesOmzetHealthWeeklyWorker
  Workers/       SalesOmzetHealthWeeklyWriter

btr.infrastructure/SalesContext/SalesOmzetAgg/
  SalesOmzetEntityDal.cs, SalesOmzetSourceDal.cs, SalesOmzetHealthMetricsDal.cs

btr.infrastructure/SalesContext/SalesOmzetHealthWeeklyAgg/
  SalesOmzetHealthWeeklyDal.cs

btr.infrastructure/SalesContext/SalesPersonAgg/
  SalesOmzetDal.cs              ← RO2 read (BTR_SalesOmzet + period policy)

btr.application/SalesContext/OrderFeature/
  ISalesOmzetDal.cs, SalesOmzetView.cs   ← report contract

btr.distrib/SalesContext/SalesPersonAgg/
  SalesOmzetInfoForm.cs         ← report UI + weekly health resolution
  SalesOmzetMaterializeForm.cs  ← materialize UI
  (health via GenerateSalesOmzetHealthWeeklyWorker after each week materialize)

btr.test/SalesContext/
  SalesOmzetPoliciesTest.cs, SalesOmzetEntityDalTest.cs, SalesOmzetReconcileTest.cs
  IsoWeekCalendarTest.cs, SalesOmzetHealthPolicyTest.cs, SalesOmzetReportHealthResolverTest.cs
```

### Important naming split

| Interface | Role |
|-----------|------|
| **`ISalesOmzetEntityDal`** | CRUD on `SalesOmzetModel` / `BTR_SalesOmzet` |
| **`ISalesOmzetDal`** | RO2 read → `SalesOmzetView` (do not use for writes) |

Legacy **UNION** logic in `SalesOmzetDal` has been **removed**; read path queries `BTR_SalesOmzet` only.

---

## Materialization (reconcile)

### Entry points

| Entry | Scope | Typical use |
|-------|--------|-------------|
| `SalesOmzetInfoForm` — Materialisasi | `ReconcileSalesOmzetScope.PeriodeScoped` per ISO week | For each week intersecting report `Tgl1`–`Tgl2` (max 3 months), reconcile week bounds then `GenerateSalesOmzetHealthWeeklyWorker` |
| `SalesOmzetMaterializeForm` — Rebuild Semua (context menu) | `ReconcileSalesOmzetScope.Full` | Post-deploy backfill; all eligible orders/fakturs via `ListAllOrders` / `ListAllFakturs`; does not auto-run weekly health |

### Worker flow (`ReconcileSalesOmzetWorker`)

1. `TransHelper.NewScope()` transaction.
2. Load orders: `ListOrders(periode)` or `ListAllOrders()` (full).
3. Load fakturs: `ListFakturs(periode)` or `ListAllFakturs()` (full); void fakturs excluded at source.
4. For each order → `Linker.FindOrCreateForOrder` (sets `SalesDate` on insert).
5. For each faktur → `Linker.FindOrCreateForFaktur` (link by `OrderId` or create `DirectSale`).
6. Merge existing rows from `ListForReconcileScope(periode)` when scoped.
7. **One `Refresh` per touched row** (hydrates faktur/omzet; preserves `SalesDate`).
8. `trans.Complete()`.
9. Populate `request.Result` (`ReconcileSalesOmzetResult`: counts, duration, scope).

**Idempotent:** unique `OrderId` / `FakturId` prevent duplicate `SalesOmzetId`.

### Linker rules (`ISalesOmzetLinker`)

- **Order:** `GetByOrderId` or insert `OrderedSale` with `SetSalesDateOnCreate(..., OrderDate)`.
- **Faktur with order:** attach to order row; **do not** change `SalesDate`.
- **Faktur without order:** `DirectSale`, `SalesDate = FakturDate` at create.
- **`Refresh`:** reload order/faktur/customer/status; `ApplyOmzetDate` from KembaliFaktur only; restore `SalesDate` if anything tries to change it.

### Policy responsibilities (where to change rules)

| Policy | Responsibility |
|--------|----------------|
| **`ISalesOmzetEligibilityPolicy`** | Void faktur (`VoidDate` sentinel); order exclusions; `ShouldRemove` |
| **`ISalesOmzetSnapshotBuilder`** | `ApplyOrder`, `ApplyFaktur`, `ApplyOmzetDate`, `SetSalesDateOnCreate` |
| **`ISalesOmzetSaleKindPolicy`** | `OrderedSale` vs `DirectSale` |
| **`ISalesOmzetStatusPolicy`** | `Outstanding` / `PendingOmzet` / `Completed` / `Void` |
| **`ISalesOmzetPeriodPolicy`** | `IsInPeriod`, `ToSqlWhere` for Omzet vs Sales period |

### Read path (`ISalesOmzetDal`)

```sql
SELECT ... FROM BTR_SalesOmzet
WHERE <ISalesOmzetPeriodPolicy.ToSqlWhere(mode)>
  AND OmzetStatus <> 'Void'
ORDER BY SalesPersonName, OrderDate, FakturDate
```

Default `ListData(Periode)` → `OmzetPeriod` (strict omzet).

---

## RO2 user interface

### `SalesOmzetInfoForm`

- Date range `Tgl1`–`Tgl2` (max 122 days).
- **`SalesPeriodCheckBox`** — *Periode Jual* (unchecked = *Periode Omzet*).
- **Proses** — list only.
- **Materialisasi** — materializes all ISO weeks in the report period and recalculates health per week (no dialog). Right-click → *Rebuild semua omzet...* (admin).
- **Health indicator** — `panel1` background only (green / yellow / red); worst-bucket across intersecting weeks.
- Search filter, grid, Excel export (status from `OmzetStatus`).

#### Weekly materialize health indicator (persisted ISO weeks)

Operational signal for whether aggregate data in a calendar week is trustworthy. **Not** a rolling window; **not** event history — one **current-state row** per `(YearNumber, WeekNumber)` in `BTR_SalesOmzetHealthWeekly`.

| Item | Detail |
|------|--------|
| **Week definition** | ISO-8601 (Monday first day). `IIsoWeekCalendar` resolves week ↔ date range. |
| **Calculation** | `GenerateSalesOmzetHealthWeeklyWorker` runs automatically after each week's materialize (and can be invoked from tests/jobs). Loads metrics for that ISO week, scores via `ISalesOmzetHealthPolicy`, upserts row (idempotent). |
| **Metrics** | Same gap/freshness SQL as before, scoped to `PeriodStartDate`–`PeriodEndDate` (`ISalesOmzetHealthMetricsDal`). |
| **Score / level** | Score 0–100 in policy; level: ≥90 Baik, ≥70 Peringatan, else Buruk. |
| **Report display** | On RO2, intersecting ISO weeks for `Tgl1`–`Tgl2` are loaded; **worst-bucket-wins** (any Buruk → Buruk). Missing week row → Buruk. Shown as `panel1` color only (no labels). |
| **Materialisasi** | One `PeriodeScoped` reconcile per intersecting ISO week (`GetWeekBounds`), then health for that week. |

Code: `GenerateSalesOmzetHealthWeeklyWorker`, `SalesOmzetHealthWeeklyDal`, `SalesOmzetHealthPolicy`, `SalesOmzetReportHealthResolver`, `IsoWeekCalendar`.

### `SalesOmzetMaterializeForm`

- Admin-only **Rebuild Semua** (opened from RO2 Materialisasi context menu).
- Runs full reconcile async; shows result metrics from `ReconcileSalesOmzetResult`.

---

## Reconcile vs period filter (common confusion)

| Concept | Purpose |
|---------|---------|
| **Reconcile `Periode`** | Which **source** orders/fakturs to **process** in this run. |
| **`GetFakturByOrderId`** | No report-period filter; links faktur outside UI window. |
| **Omzet / Sales Period** | Which **materialized rows appear** on RO2 after Proses. |

Example: Order in January, KembaliFaktur in February.

- After materialize: one row; `SalesDate` = Jan; `OmzetDate` = Feb.
- **Omzet Period** Jan → hidden. **Feb** → visible.
- **Sales Period** Jan → visible (pipeline/completed with Jan tanggal jual). **Feb** → hidden unless `SalesDate` falls in Feb.

---

## Rule-change playbook

| Business change | Edit |
|-----------------|------|
| When omzet is recognized | `SalesOmzetSnapshotBuilder.ApplyOmzetDate` |
| Tanggal Jual / freeze | `SetSalesDateOnCreate` + `SalesOmzetLinker` |
| Omzet vs Sales period on report | `SalesOmzetPeriodPolicy` only |
| Ordered vs Direct | `SalesOmzetSaleKindPolicy` |
| Status labels / Excel colors | `SalesOmzetStatusPolicy` + `SalesOmzetInfoForm` |
| Exclude void / cancelled | `SalesOmzetEligibilityPolicy` |
| How rows are linked | `SalesOmzetLinker` |
| Reconcile performance / scope | `ReconcileSalesOmzetWorker`, `ISalesOmzetSourceDal` |

Avoid new `UNION` branches in SQL; extend policies or linker steps.

---

## Verification scenarios

| Scenario | SaleKind | Omzet Period list | Sales Period list |
|----------|----------|-------------------|-------------------|
| Order Jan, no KembaliFaktur | OrderedSale | Hidden | Visible (`SalesDate` Jan) |
| Order Jan, KembaliFaktur Feb | OrderedSale | Visible in Feb | Visible in Jan |
| Direct faktur, KembaliFaktur Mar | DirectSale | Visible in Mar | Per `SalesDate` |
| Void faktur | — | Removed / Void | Removed |
| Materialize twice same period | — | No duplicate `SalesOmzetId` | |

---

## Boundaries (out of scope by design)

- **No hooks** on Create Order / Save Faktur (materialize is explicit or scheduled separately).
- **No** showing outstanding in Omzet Period (strict omzet is fixed).
- **`BTR_OrderMap`** still exists for other features; not replaced by Sales Omzet in one step.
- **`btr.sync`** does not reference `btr.application`; scheduled reconcile there would be a separate integration project.

---

## Dependency injection

Registered in `btr.distrib/Program.cs`:

- Policies, linker, snapshot builder, `ISalesOmzetSourceDal` — explicit `AddScoped`.
- `IReconcileSalesOmzetWorker` — Scrutor `INunaServiceVoid<>`.
- `ISalesOmzetEntityDal`, `ISalesOmzetDal` — Scrutor `IInsert` / `IUpdate` / `IListData`.

---

## Key files for investigation

| Topic | File |
|-------|------|
| Business rules (period) | `SalesOmzetPeriodPolicy.cs` |
| Business rules (omzet date) | `SalesOmzetSnapshotBuilder.cs` |
| Create/link/refresh | `SalesOmzetLinker.cs` |
| Orchestration | `ReconcileSalesOmzetWorker.cs` |
| Wrong pattern (old report) | git history of `SalesOmzetDal` UNION — do not reintroduce period-scoped faktur join on read |
| Report UI | `SalesOmzetInfoForm.cs` |
| Materialize UI | `SalesOmzetMaterializeForm.cs` |
| Weekly health worker | `GenerateSalesOmzetHealthWeeklyWorker.cs` (triggered from RO2 materialize) |
| ISO week / report health | `IsoWeekCalendar.cs`, `SalesOmzetReportHealthResolver.cs` |
| Tests | `SalesOmzetPoliciesTest.cs`, `SalesOmzetReconcileTest.cs`, `IsoWeekCalendarTest.cs`, `SalesOmzetHealthPolicyTest.cs` |
| Deploy / backfill | `docs/ops/sales-omzet-deploy.md` |

---

## Summary

**Sales Omzet** is a materialized aggregate: one **`SalesOmzetId`** per sale thread (**Ordered** or **Direct**), **`SalesDate`** frozen at create, **`OmzetDate`** from **KembaliFaktur** only. **Materialization** writes the table; **RO2** reads it with **Periode Omzet** (strict) or **Periode Jual**. All volatile business rules belong in **`Policies/`** and the **linker**—not in ad-hoc report SQL.
