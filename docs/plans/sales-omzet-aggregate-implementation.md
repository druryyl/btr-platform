# Sales Omzet Aggregate — Implementation Plan

## Goals

1. Replace on-the-fly `SalesOmzetDal` UNION query with a first-class **`SalesOmzet`** entity (`SalesOmzetId` ≠ `OrderId` / `FakturId`).
2. Model **Order >> Faktur** and **faktur-without-order** as one thread per row, updated over time (current state).
3. **Do not change** Create Order or Save Faktur use cases — all create/link/refresh happens in **reconcile (materialization)** only.
4. Keep **`SalesOmzetInfoForm` UI** unchanged; only wire Proses → reconcile → read.
5. Make **business rules easy to change** without rewriting SQL or the reconcile orchestrator.

---

## Design principles (rule-change friendly)

| Principle | Why |
|-----------|-----|
| **Thin SQL, fat policy** | DAL only loads/saves rows; rules live in application layer. |
| **Single orchestrator, many policies** | `ReconcileSalesOmzetWorker` is stable; you swap or edit small classes. |
| **One aggregate row per sale thread** | Unique `OrderId` / `FakturId` on `BTR_SalesOmzet` — no period-split UNION logic. |
| **Period is a read filter** | Reconcile links faktur by `OrderId` regardless of report dates; period applies in `ListData` only. |
| **Explicit rule classes** | Each concern = one interface + one default implementation (easy to find and change). |

---

## Architecture

```
SalesOmzetInfoForm
    │  Proses(periode)
    ▼
IReconcileSalesOmzetWorker          ← orchestration only (steps 1–5)
    │
    ├── ISalesOmzetSourceDal        ← read Order / Faktur / Customer / ControlStatus (no business rules)
    ├── ISalesOmzetDal              ← CRUD on BTR_SalesOmzet
    ├── ISalesOmzetLinker           ← find/create/link by OrderId & FakturId
    ├── ISalesOmzetSnapshotBuilder  ← map sources → denormalized fields on model
    ├── ISalesOmzetStatusPolicy     ← Outstanding / Completed / DirectSales / Void
    ├── ISalesOmzetEligibilityPolicy← include/exclude void, cancelled, etc.
    ├── ISalesOmzetPeriodPolicy     ← “does this row appear in ListData(periode)?”
    └── ISalesOmzetWriter           ← assign SalesOmzetId (counter), Insert/Update

ISalesOmzetDal.ListData(periode)
    └── SELECT + ISalesOmzetPeriodPolicy SQL fragment or post-filter
```

When business asks for a rule change, you usually touch **one policy class** (or `SalesOmzetSnapshotBuilder`), not the form or the worker loop.

---

## Folder layout

```
btr.sql/Tables/SalesContext/
    BTR_SalesOmzet.sql

btr.domain/SalesContext/SalesOmzetAgg/
    SalesOmzetModel.cs
    ISalesOmzetKey.cs
    SalesOmzetOriginSource.cs      // Order, Faktur
    SalesOmzetStatus.cs            // Outstanding, Completed, DirectSales, Void, ...

btr.application/SalesContext/SalesOmzetAgg/
    Contracts/
        ISalesOmzetDal.cs
        ISalesOmzetSourceDal.cs      // raw reads for reconcile
    Policies/                        // ← business rules live here
        ISalesOmzetPeriodPolicy.cs
        SalesOmzetPeriodPolicy.cs    // default: OrderDate OR FakturDate in periode
        ISalesOmzetStatusPolicy.cs
        SalesOmzetStatusPolicy.cs
        ISalesOmzetEligibilityPolicy.cs
        SalesOmzetEligibilityPolicy.cs  // e.g. VoidDate = 3000-01-01
        ISalesOmzetSnapshotBuilder.cs
        SalesOmzetSnapshotBuilder.cs    // totals, sales name, omzet date, customer fields
    Services/
        ISalesOmzetLinker.cs
        SalesOmzetLinker.cs
    UseCases/
        ReconcileSalesOmzetRequest.cs
        IReconcileSalesOmzetWorker.cs   // INunaServiceVoid<ReconcileSalesOmzetRequest>
        ReconcileSalesOmzetWorker.cs
    Workers/
        ISalesOmzetWriter.cs
        SalesOmzetWriter.cs             // INunaCounterBL → SalesOmzetId

btr.infrastructure/SalesContext/SalesOmzetAgg/
    SalesOmzetDal.cs
    SalesOmzetSourceDal.cs

btr.application/SalesContext/OrderFeature/
    ISalesOmzetDal.cs                  // keep; ListData(Periode) → reads aggregate
    SalesOmzetView.cs                  // keep for grid/Excel (map from model or DB)
```

Deprecate / remove logic from `btr.infrastructure/.../SalesOmzetDal.cs` (old UNION) after cutover.

---

## Database: `BTR_SalesOmzet`

```sql
-- Conceptual; adjust lengths to match BTR_Order / BTR_Faktur
CREATE TABLE BTR_SalesOmzet (
    SalesOmzetId     VARCHAR(13) NOT NULL PRIMARY KEY,
    OrderId          VARCHAR(26) NOT NULL DEFAULT(''),
    FakturId         VARCHAR(15) NOT NULL DEFAULT(''),
    OriginSource     VARCHAR(10) NOT NULL DEFAULT(''),  -- ORDER | FAKTUR

  -- Denormalized display (refreshed on reconcile)
    SalesPersonName  VARCHAR(...),
    OrderDate        DATETIME NOT NULL DEFAULT('3000-01-01'),
    OrderTotal       DECIMAL(18,2) NOT NULL DEFAULT(0),
    FakturCode       VARCHAR(11) NOT NULL DEFAULT(''),
    FakturDate       DATETIME NOT NULL DEFAULT('3000-01-01'),
    FakturTotal      DECIMAL(18,2) NOT NULL DEFAULT(0),
    CustomerName     VARCHAR(...),
    Code             VARCHAR(...),   -- CustomerCode
    Alamat           VARCHAR(...),
    OmzetDate        DATETIME NOT NULL DEFAULT('3000-01-01'),
    OmzetStatus      VARCHAR(20) NOT NULL DEFAULT(''),

    CreatedAt        DATETIME NOT NULL DEFAULT('3000-01-01'),
    LastReconciledAt DATETIME NOT NULL DEFAULT('3000-01-01')
);

CREATE UNIQUE INDEX UX_BTR_SalesOmzet_OrderId
    ON BTR_SalesOmzet(OrderId) WHERE OrderId <> '';

CREATE UNIQUE INDEX UX_BTR_SalesOmzet_FakturId
    ON BTR_SalesOmzet(FakturId) WHERE FakturId <> '';

CREATE INDEX IX_BTR_SalesOmzet_OrderDate ON BTR_SalesOmzet(OrderDate, SalesOmzetId);
CREATE INDEX IX_BTR_SalesOmzet_FakturDate ON BTR_SalesOmzet(FakturDate, SalesOmzetId);
```

Add `BTR_ParamNo` prefix for new IDs (e.g. `SO` / `OMZ`) when implementing `SalesOmzetWriter`.

---

## Policy interfaces (where rules change)

### 1. `ISalesOmzetEligibilityPolicy`

**Question:** Should this order/faktur participate in omzet at all?

```csharp
bool IsOrderEligible(OrderSnapshot order);
bool IsFakturEligible(FakturSnapshot faktur);
bool ShouldRemove(SalesOmzetModel row);  // voided both sides, etc.
```

Default: faktur `VoidDate == 3000-01-01`; define order rules when known.

*Change example:* “Exclude orders with `StatusSync = 'BATAL'`” → one line in `SalesOmzetEligibilityPolicy`.

---

### 2. `ISalesOmzetLinker`

**Question:** How do we find or create the aggregate for a source row?

```csharp
SalesOmzetModel FindOrCreateForOrder(OrderSnapshot order, ...);
SalesOmzetModel FindOrCreateForFaktur(FakturSnapshot faktur, ...);
```

Default:

- Order → `GetByOrderId`; if null → `Insert` with `OriginSource = Order`.
- Faktur → if `OrderId` set → `GetByOrderId` then attach `FakturId`; else `GetByFakturId` or create with `OriginSource = Faktur`.

*Change example:* “Merge duplicate faktur-originated row when order appears” → only `SalesOmzetLinker`.

---

### 3. `ISalesOmzetSnapshotBuilder`

**Question:** What values go on the denormalized columns?

```csharp
void ApplyOrder(SalesOmzetModel target, OrderSnapshot order);
void ApplyFaktur(SalesOmzetModel target, FakturSnapshot faktur, CustomerSnapshot customer, ...);
void ApplyOmzetDate(SalesOmzetModel target, FakturControlStatusSnapshot status);
```

Default:

- `OrderTotal` ← `Order.TotalAmount` (not line sum — align with `OrderDal`).
- `SalesPersonName` ← faktur `SalesPerson` if linked, else `Order.SalesName` / `UserEmail`.
- `OmzetDate` ← `FakturControlStatus` where `StatusFaktur == 2` (constant in one place).

*Change example:* “Omzet date = `FakturDate` when control status missing” → only `ApplyOmzetDate`.

---

### 4. `ISalesOmzetStatusPolicy`

**Question:** What is `OmzetStatus`?

```csharp
string Resolve(SalesOmzetModel row);
```

Default:

| OrderId | FakturId | Status |
|---------|----------|--------|
| set | empty | Outstanding |
| set | set | Completed |
| empty | set | DirectSales |

*Change example:* new status `Partial` → edit `SalesOmzetStatusPolicy` + enum + Excel color in form.

---

### 5. `ISalesOmzetPeriodPolicy`

**Question:** Which rows show on RO2 for `[Tgl1, Tgl2]`?

```csharp
bool IsInPeriod(SalesOmzetModel row, Periode periode);
// optional: string SqlWhereClauseFragment for efficient ListData
```

Default (document in code):  
`OrderDate ∈ [Tgl1,Tgl2] OR FakturDate ∈ [Tgl1,Tgl2]` (and optionally `OmzetDate` — business to confirm).

*Change example:* “Only faktur date counts for completed rows” → only this policy + test cases.

---

## Reconcile worker (stable orchestration)

`ReconcileSalesOmzetWorker.Execute(ReconcileSalesOmzetRequest)`:

| Step | Action | Rule location |
|------|--------|----------------|
| 1 | Load candidate orders (scoped by request window or full) | `ISalesOmzetSourceDal` |
| 2 | For each eligible order → `Linker.FindOrCreateForOrder` | Eligibility + Linker |
| 3 | Load candidate fakturs (non-void) | SourceDal |
| 4 | For each eligible faktur → `Linker.FindOrCreateForFaktur` | Eligibility + Linker |
| 5 | Load all `SalesOmzet` in scope (or batch) | Dal |
| 6 | For each row: reload order/faktur/customer/status → `SnapshotBuilder` → `StatusPolicy` → `Update` | Policies |
| 7 | `ShouldRemove` → delete or mark void | Eligibility |
| 8 | Set `LastReconciledAt` | Writer/Dal |

**Scope options** (constructor or request flag):

- `Periode` — only sources touching the date window (performance; matches 3-month UI cap).
- `Full` — first deploy or nightly job.

Linking **must** load faktur by `OrderId` without restricting faktur date to the period.

Transaction: wrap steps 2–7 in `TransHelper.NewScope()` per period batch.

---

## Read path (form)

`ISalesOmzetDal.ListData(Periode)`:

```sql
SELECT ... FROM BTR_SalesOmzet
WHERE <ISalesOmzetPeriodPolicy.SqlFilter>
ORDER BY SalesPersonName, OrderDate, FakturDate
```

Map to existing `SalesOmzetView` (add `OmzetStatus` to view later if Excel should use DB column).

**Form change (minimal):**

```csharp
_reconcileWorker.Execute(new ReconcileSalesOmzetRequest { Periode = periode, UserId = ... });
var listOmzet = _salesOmzetDal.ListData(periode)?.ToList();
```

Inject `IReconcileSalesOmzetWorker` in `SalesOmzetInfoForm` constructor.

---

## Implementation phases

### Phase 0 — Lock business defaults (½ day)

Document in `SalesOmzetEligibilityPolicy` / `SalesOmzetPeriodPolicy` XML comments:

- [ ] Period inclusion rule (OrderDate / FakturDate / OmzetDate).
- [ ] Void faktur handling.
- [ ] Show or hide Outstanding rows on RO2.
- [ ] Order total source (`TotalAmount` vs line sum).
- [ ] OmzetDate source (`StatusFaktur = 2` vs fallback).

Collect 5–10 real rows that are **wrong today**; use as acceptance tests.

---

### Phase 1 — Schema & persistence (1 day)

1. Add `BTR_SalesOmzet.sql` to `btr.sql.sqlproj`.
2. Add `SalesOmzetModel` + enums in `btr.domain`.
3. Add `ISalesOmzetDal` (`IGetData`, `IInsert`, `IUpdate`, `IListData<SalesOmzetView, Periode>`, `GetByOrderId`, `GetByFakturId`).
4. Implement `SalesOmzetDal` in infrastructure (simple SQL).
5. Add `SalesOmzetWriter` + ParamNo prefix.
6. Deploy script to dev DB.

**Exit:** can insert/update/read one row manually.

---

### Phase 2 — Policies & snapshots (1–2 days)

1. Add `OrderSnapshot` / `FakturSnapshot` DTOs (minimal fields) in application layer.
2. Implement `SalesOmzetSourceDal` (SELECT only, no void filter in SQL or with parameter — eligibility in policy).
3. Implement all **Policies** + **SnapshotBuilder** + **Linker** with defaults above.
4. Unit-test policies in isolation (no DB): status, period, eligibility, snapshot mapping.

**Exit:** rules are testable without UI.

---

### Phase 3 — Reconcile worker (1–2 days)

1. `ReconcileSalesOmzetWorker` + request DTO.
2. Wire `INunaServiceVoid<>` — auto-registered by Scrutor in `Program.cs`.
3. Optional: `ReconcileSalesOmzetScope` enum (`Periode` / `Full`).
4. Integration test or manual script: order-only → outstanding; add faktur → completed on second reconcile.

**Exit:** `BTR_SalesOmzet` populated correctly for test scenarios.

---

### Phase 4 — Switch read path & form (½ day)

1. Rewrite `ListData(Periode)` to query `BTR_SalesOmzet` + period policy.
2. Update `SalesOmzetInfoForm.Proses` to call reconcile first.
3. Optionally use `OmzetStatus` from DB in Excel export instead of `GetOrderStatus`.
4. Remove old UNION SQL from infrastructure `SalesOmzetDal` (or rename class to `SalesOmzetDalLegacy` until verified).

**Exit:** RO2 matches acceptance cases; UI unchanged.

---

### Phase 5 — Hardening (ongoing)

1. One-off **full reconcile** after production deploy.
2. Logging: rows created/updated/deleted per run.
3. (Optional) scheduled reconcile in `btr.sync` — same worker, `Scope = Full`.
4. (Optional) retire `BTR_OrderMap` for RO2 if fully superseded — separate task.

---

## Adjusting rules later (playbook)

| Business ask | File to edit |
|--------------|--------------|
| New column on grid | `BTR_SalesOmzet`, `SalesOmzetModel`, `SnapshotBuilder`, `SalesOmzetView`, form grid column |
| When omzet date is set | `SalesOmzetSnapshotBuilder.ApplyOmzetDate` |
| Hide void / cancelled | `SalesOmzetEligibilityPolicy` |
| Period appears on report | `SalesOmzetPeriodPolicy` only |
| New status label | `SalesOmzetStatusPolicy` + form Excel colors |
| Link faktur differently | `SalesOmzetLinker` |
| Reconcile performance | `ReconcileSalesOmzetRequest` scope + source DAL queries — not policies |

Avoid adding new `UNION` branches in SQL; add policy methods or snapshot steps instead.

---

## Testing checklist

| Scenario | Expected after reconcile |
|----------|---------------------------|
| Order in period, no faktur | One row, Outstanding |
| Order in period, faktur outside period | One row, Completed, faktur fields filled |
| Faktur in period, order last month | One row linked by OrderId, correct totals (not OrderDate=3000) |
| Faktur without order | One row, DirectSales |
| Void faktur | Removed or Void per eligibility |
| Proses twice same period | Idempotent — no duplicate `SalesOmzetId` |
| Order then faktur next day | Same `SalesOmzetId`, status Outstanding → Completed |

---

## DI / project registration

- **Worker:** `INunaServiceVoid<ReconcileSalesOmzetRequest>` — auto-scanned from application assembly.
- **Dal:** `IInsert`/`IUpdate`/`IListData<,>` — auto-scanned from infrastructure assembly.
- **Policies:** register explicitly in `Program.cs` as `AddScoped<ISalesOmzetPeriodPolicy, SalesOmzetPeriodPolicy>()` (or Scrutor scan `*Policy` in `SalesOmzetAgg.Policies`).

Explicit policy registration makes swapping implementations obvious for A/B rule tests.

---

## Out of scope (this plan)

- Changes to Create Order / Save Faktur.
- Real-time reconcile on faktur save (optional future; same worker).
- Replacing `OrderMap` entirely.
- Fixing other reports (`OmzetSupplier`, etc.).

---

## Summary

Build **Sales Omzet as an aggregate** with **reconcile** as the only writer, and put every volatile business rule in **`Policies/`** and **`SalesOmzetSnapshotBuilder`**. The worker orchestration stays fixed; rule changes are localized, unit-testable, and do not require another complex SQL report query.
