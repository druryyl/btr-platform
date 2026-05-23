# Sales Omzet Aggregate — Implementation Plan

## Goals

1. Replace on-the-fly `SalesOmzetDal` UNION query with a first-class **`SalesOmzet`** entity (`SalesOmzetId` ≠ `OrderId` / `FakturId`).
2. Model sales as **Ordered Sale** (Order >> Faktur) or **Direct Sale** (faktur without order), as **current state** updated by reconcile only.
3. **Do not change** Create Order or Save Faktur use cases — create/link/refresh happens only in **`ReconcileSalesOmzetWorker`**.
4. RO2 (`SalesOmzetInfoForm`): **Proses** → list only; materialize via `SalesOmzetMaterializeForm`; add **period mode** control (see UX below).
5. Make **business rules easy to change** via small policy classes, not SQL unions.

---

## Business vocabulary (locked)

| Term | English | Meaning |
|------|---------|---------|
| **Tanggal Jual** | **SalesDate** | Stable “when the sale thread started” for **Sales Period** filtering. Set once; does not move when faktur / omzet is recognized later. |
| **Tanggal Omzet** | **OmzetDate** | When omzet is **recognized**: `StatusDate` on `FakturControlStatusModel` where `StatusFaktur == KembaliFaktur` (faktur received at main office). **Not** `FakturDate`. |
| **Ordered Sale** | | Sale that started with an order (`OrderId` set). May lack faktur or KembaliFaktur initially. |
| **Direct Sale** | | Sale created from faktur without an order (`OrderId` empty, `FakturId` set). |
| **Omzet Period** | | RO2 filter: `OmzetDate` in `[Tgl1, Tgl2]`. **Strict omzet** — outstanding / not-yet-KembaliFaktur rows are **hidden**. |
| **Sales Period** | | RO2 filter: `SalesDate` in `[Tgl1, Tgl2]`. Shows ordered pipeline in the sales month (Option A — date does not drift when omzet is later). |

### Sale kind (`SaleKindEnum`)

| Value | When set | `OrderId` | `FakturId` |
|-------|----------|-----------|------------|
| **OrderedSale** | Reconcile creates/links from order, or faktur links to existing order row | set | optional |
| **DirectSale** | Reconcile creates from faktur with no order | empty | set |

Use **SaleKind** on the aggregate (and grid/export) instead of informal “origin” wording. `OriginSource` in DB can map to `SaleKind` (ORDER → OrderedSale, FAKTUR → DirectSale).

### OmzetDate source (domain)

From `FakturControlModel.ListStatus`:

- Find `FakturControlStatusModel` where `StatusFaktur == StatusFakturEnum.KembaliFaktur`
- `OmzetDate = status.StatusDate`
- If none: `OmzetDate =` sentinel (`3000-01-01`) — omzet not recognized yet

Reference: `btr.domain/SalesContext/FakturControlAgg/StatusFakturEnum.cs`, `FakturControlStatusModel.cs`.

Do **not** use `Faktur.FakturDate` for omzet recognition. Keep `FakturDate` on the row for display only.

### SalesDate rules (Option A — frozen)

| Sale kind | When `SalesDate` is set | Updated later? |
|-----------|-------------------------|----------------|
| **OrderedSale** | On first create from order: `SalesDate = OrderDate` | **Never** (even when faktur / KembaliFaktur is added) |
| **DirectSale** | On create from faktur: `SalesDate = FakturDate` (or business-agreed date at create) | **Never** |

This prevents January ordered sales from jumping to February when omzet is recognized in February.

---

## RO2 UX — period filter

Add a user control on `SalesOmzetInfoForm` (checkbox or radio):

| Mode | Label (suggested) | Filter |
|------|-------------------|--------|
| **Default** | **Omzet Period** / *Periode Omzet* | `OmzetDate BETWEEN @Tgl1 AND @Tgl2` |
| **Alternate** | **Sales Period** / *Periode Jual* | `SalesDate BETWEEN @Tgl1 AND @Tgl2` |

Pass `SalesOmzetPeriodFilterMode` into `ListData` (extend request or add parameter object).

### Strict omzet (default = Omzet Period)

When **Omzet Period** is selected:

- Include row **only if** `OmzetDate` is in range (valid KembaliFaktur date, not sentinel).
- **Outstanding** ordered sales (no KembaliFaktur yet) **do not appear**.
- No “pipeline OR” exception.

When **Sales Period** is selected:

- Include row if `SalesDate` is in range.
- Ordered sales **without** KembaliFaktur **do appear** (pipeline in that sales month).
- Direct sales appear by their frozen `SalesDate`.

---

## Design principles (rule-change friendly)

| Principle | Why |
|-----------|-----|
| **Thin SQL, fat policy** | DAL loads/saves; rules in `Policies/`. |
| **Single orchestrator, many policies** | `ReconcileSalesOmzetWorker` stays stable. |
| **One row per sale thread** | Unique `OrderId` / `FakturId` on `BTR_SalesOmzet`. |
| **Reconcile links across dates** | Faktur joined by `OrderId` without restricting to report period. |
| **Period = read concern** | `ISalesOmzetPeriodPolicy` implements Omzet vs Sales period + strict omzet. |
| **Freeze SalesDate once** | `ISalesOmzetSnapshotBuilder.SetSalesDateOnCreate` only. |

---

## Architecture

```
SalesOmzetInfoForm (report / info only)
    │  Proses(periode, periodFilterMode)
    ▼
ISalesOmzetDal.ListData(periode, periodFilterMode)
    └── ISalesOmzetPeriodPolicy → SQL WHERE on BTR_SalesOmzet

SalesOmzetMaterializeForm (opened from info form — Materialisasi button)
    │  Materialisasi / Rebuild Semua
    ▼
IReconcileSalesOmzetWorker
    ├── ISalesOmzetSourceDal
    ├── ISalesOmzetLinker
    ├── ISalesOmzetSnapshotBuilder     ← SalesDate (once), OmzetDate (KembaliFaktur), totals
    ├── ISalesOmzetSaleKindPolicy      ← OrderedSale vs DirectSale
    ├── ISalesOmzetStatusPolicy        ← Outstanding / Completed / PendingOmzet / ...
    ├── ISalesOmzetEligibilityPolicy
    └── ISalesOmzetWriter
```

---

## Folder layout

```
btr.sql/Tables/SalesContext/
    BTR_SalesOmzet.sql

btr.domain/SalesContext/SalesOmzetAgg/
    SalesOmzetModel.cs
    ISalesOmzetKey.cs
    SaleKindEnum.cs                 // OrderedSale, DirectSale
    SalesOmzetStatusEnum.cs         // Outstanding, Completed, PendingOmzet, Void, ...
    SalesOmzetPeriodFilterMode.cs   // OmzetPeriod, SalesPeriod

btr.application/SalesContext/SalesOmzetAgg/
    Contracts/
        ISalesOmzetEntityDal.cs       // Phase 1 aggregate CRUD
        ISalesOmzetSourceDal.cs       // Phase 2
    Policies/
        ISalesOmzetPeriodPolicy.cs
        SalesOmzetPeriodPolicy.cs
        ISalesOmzetSaleKindPolicy.cs
        SalesOmzetSaleKindPolicy.cs
        ISalesOmzetStatusPolicy.cs
        SalesOmzetStatusPolicy.cs
        ISalesOmzetEligibilityPolicy.cs
        SalesOmzetEligibilityPolicy.cs
        ISalesOmzetSnapshotBuilder.cs
        SalesOmzetSnapshotBuilder.cs
    Services/
        ISalesOmzetLinker.cs
        SalesOmzetLinker.cs
    UseCases/
        ReconcileSalesOmzetRequest.cs
        IReconcileSalesOmzetWorker.cs
        ReconcileSalesOmzetWorker.cs
    Workers/
        ISalesOmzetWriter.cs
        SalesOmzetWriter.cs

btr.infrastructure/SalesContext/SalesOmzetAgg/
    SalesOmzetEntityDal.cs
    SalesOmzetSourceDal.cs         // Phase 2

btr.application/SalesContext/OrderFeature/
    ISalesOmzetDal.cs               // extend ListData with period filter mode
    SalesOmzetView.cs               // add SaleKind, SalesDate, OmzetStatus as needed
```

Deprecate UNION logic in `btr.infrastructure/.../SalesPersonAgg/SalesOmzetDal.cs` after cutover.

---

## Database: `BTR_SalesOmzet`

> **Implementers:** Use exact VARCHAR lengths from [Phase 1 — Agent kickoff → Column lengths](#column-lengths-use-in-sql--align-with-existing-tables). Values below are structural placeholders.

```sql
CREATE TABLE BTR_SalesOmzet (
    SalesOmzetId     VARCHAR(13) NOT NULL PRIMARY KEY,
    OrderId          VARCHAR(26) NOT NULL DEFAULT(''),
    FakturId         VARCHAR(15) NOT NULL DEFAULT(''),
    SaleKind         VARCHAR(15) NOT NULL DEFAULT(''),   -- OrderedSale | DirectSale

    SalesDate        DATETIME NOT NULL DEFAULT('3000-01-01'),  -- Tanggal Jual; frozen at create
    OmzetDate        DATETIME NOT NULL DEFAULT('3000-01-01'),  -- KembaliFaktur StatusDate

    SalesPersonName  VARCHAR(100) NOT NULL DEFAULT(''),
    OrderDate        DATETIME NOT NULL DEFAULT('3000-01-01'),
    OrderTotal       DECIMAL(18,2) NOT NULL DEFAULT(0),
    FakturCode       VARCHAR(11) NOT NULL DEFAULT(''),
    FakturDate       DATETIME NOT NULL DEFAULT('3000-01-01'),
    FakturTotal      DECIMAL(18,2) NOT NULL DEFAULT(0),
    CustomerName     VARCHAR(100) NOT NULL DEFAULT(''),
    Code             VARCHAR(20) NOT NULL DEFAULT(''),
    Alamat           VARCHAR(200) NOT NULL DEFAULT(''),
    OmzetStatus      VARCHAR(20) NOT NULL DEFAULT(''),

    CreatedAt        DATETIME NOT NULL DEFAULT('3000-01-01'),
    LastReconciledAt DATETIME NOT NULL DEFAULT('3000-01-01')
);

CREATE UNIQUE INDEX UX_BTR_SalesOmzet_OrderId
    ON BTR_SalesOmzet(OrderId) WHERE OrderId <> '';

CREATE UNIQUE INDEX UX_BTR_SalesOmzet_FakturId
    ON BTR_SalesOmzet(FakturId) WHERE FakturId <> '';

CREATE INDEX IX_BTR_SalesOmzet_SalesDate ON BTR_SalesOmzet(SalesDate, SalesOmzetId);
CREATE INDEX IX_BTR_SalesOmzet_OmzetDate ON BTR_SalesOmzet(OmzetDate, SalesOmzetId);
```

Add `BTR_ParamNo` prefix (e.g. `SO`) for `SalesOmzetWriter`.

---

## Policy interfaces

### 1. `ISalesOmzetEligibilityPolicy`

- Faktur: `VoidDate == '3000-01-01'` (align with other modules).
- Order: define exclusions (e.g. cancelled sync status) when known.
- `ShouldRemove(row)` for voided / invalid threads.

### 2. `ISalesOmzetLinker`

- **OrderedSale:** `GetByOrderId` → else insert with `SaleKind = OrderedSale`, set **`SalesDate = OrderDate` once**.
- **DirectSale:** `GetByFakturId` → else insert with `SaleKind = DirectSale`, set **`SalesDate`** once per direct-sale rule.
- Faktur with `OrderId` → link to existing ordered row; do **not** overwrite `SalesDate`.

### 3. `ISalesOmzetSnapshotBuilder`

| Method | Rule |
|--------|------|
| `ApplyOrder` | `OrderDate`, `OrderTotal` ← `Order.TotalAmount`, sales name from order |
| `ApplyFaktur` | `FakturCode`, `FakturDate`, `FakturTotal`, customer, sales person from faktur |
| `ApplyOmzetDate` | `OmzetDate` ← `KembaliFaktur.StatusDate` only; use `StatusFakturEnum.KembaliFaktur`, not magic `2` |
| `SetSalesDateOnCreate` | OrderedSale → `OrderDate`; DirectSale → `FakturDate` (document if changed) |

**Never** update `SalesDate` on refresh.

### 4. `ISalesOmzetSaleKindPolicy`

```csharp
SaleKindEnum Resolve(SalesOmzetModel row);
// OrderedSale if OrderId set; else DirectSale if FakturId set
```

Set on create/link; refresh only if business allows conversion (default: no conversion).

### 5. `ISalesOmzetStatusPolicy`

Suggested statuses (adjust labels for UI/Excel):

| Condition | Status |
|-----------|--------|
| OrderedSale, no faktur | `Outstanding` |
| OrderedSale, faktur, no KembaliFaktur | `PendingOmzet` (optional; or keep Outstanding) |
| Faktur + KembaliFaktur | `Completed` |
| DirectSale + KembaliFaktur | `Completed` |
| DirectSale, no KembaliFaktur | `PendingOmzet` |
| Void / ineligible | `Void` or removed |

Map legacy Excel colors in form from `OmzetStatus` column when ready.

### 6. `ISalesOmzetPeriodPolicy`

```csharp
bool IsInPeriod(SalesOmzetModel row, Periode periode, SalesOmzetPeriodFilterMode mode);
string ToSqlWhere(SalesOmzetPeriodFilterMode mode); // for ListData
```

| Mode | Include when |
|------|----------------|
| **OmzetPeriod** (default) | `OmzetDate` between Tgl1–Tgl2 **and** `OmzetDate` is not sentinel (**strict omzet**) |
| **SalesPeriod** | `SalesDate` between Tgl1–Tgl2 |

*Change example:* “Show pending omzet in Omzet Period” → only this policy (currently **out of scope** per strict omzet).

---

## Reconcile worker

`ReconcileSalesOmzetWorker.Execute(ReconcileSalesOmzetRequest)`:

| Step | Action |
|------|--------|
| 1 | Load candidate orders (scoped or full) |
| 2 | Eligible orders → `Linker.FindOrCreateForOrder` (sets **SalesDate** on insert) |
| 3 | Load candidate fakturs (non-void) |
| 4 | Eligible fakturs → `Linker.FindOrCreateForFaktur` (link or DirectSale create) |
| 5 | Refresh each row: order/faktur/customer/control status → snapshot → sale kind → status |
| 6 | Remove ineligible rows |
| 7 | `LastReconciledAt` |

- Link faktur by `OrderId` **without** faktur-date-in-period restriction.
- Wrap writes in `TransHelper.NewScope()`.

---

## Read path & form

```csharp
// Info form — list only (fast; reads materialized aggregate)
var list = _salesOmzetDal.ListData(periode, periodFilterMode);

// Materialize form — reconcile when user explicitly refreshes aggregate data
_reconcileWorker.Execute(new ReconcileSalesOmzetRequest { Periode = periode, UserId = ... });
```

**Form layout (RO2):**

| Form | Role |
|------|------|
| `SalesOmzetInfoForm` | Report: period filter, **Proses** → `ListData`, grid, Excel |
| `SalesOmzetMaterializeForm` | Materialize: **Materialisasi** (scoped reconcile), **Rebuild Semua** (full); opened from info form |

**Info form:**

1. Inject `ISalesOmzetDal` only (no reconcile worker).
2. Add control: **Omzet Period** (default) vs **Sales Period** (*Periode Omzet* / *Periode Jual*).
3. `Proses()` passes selected mode to `ListData` — does **not** run reconcile.
4. **Materialisasi** button opens `SalesOmzetMaterializeForm` with current date range pre-filled.
5. Optional: show `SaleKind` / `OmzetStatus` on grid; Excel uses DB status when available.

---

## Implementation phases

### Phase 0 — Lock rules (½ day)

- [x] OmzetDate = KembaliFaktur `StatusDate`
- [x] SalesDate frozen (Option A)
- [x] Sale kinds: OrderedSale, DirectSale
- [x] Omzet Period = strict (hide outstanding)
- [x] Sales Period = filter by SalesDate
- [ ] DirectSale `SalesDate` = FakturDate at create (confirm)
- [ ] Collect wrong rows from current report as acceptance tests

### Phase 1 — Schema & persistence (1 day)

`BTR_SalesOmzet`, domain model, enums, entity DAL, writer, sqlproj, dev deploy.

**→ Follow [Phase 1 — Agent kickoff](#phase-1--agent-kickoff) below before coding.**

### Phase 2 — Policies & snapshots (1–2 days)

Source DAL, all policies, linker, snapshot builder (KembaliFaktur omzet date, frozen SalesDate). Unit tests for period modes and strict omzet.

**→ Follow [Phase 2 — Agent kickoff](#phase-2--agent-kickoff) below before coding.**

### Phase 3 — Reconcile worker (1–2 days)

Worker + request; idempotent create/link; compose Phase 2 linker end-to-end.

**→ Follow [Phase 3 — Agent kickoff](#phase-3--agent-kickoff) below before coding.**

### Phase 4 — RO2 integration (1 day)

`ListData(periode, mode)`, form period control, materialize form (reconcile separate from Proses), remove old UNION.

**→ Follow [Phase 4 — Agent kickoff](#phase-4--agent-kickoff) below before coding.**

### Phase 5 — Hardening

Full reconcile scope, deploy/backfill runbook, reconcile metrics/logging, tests & ops polish.

**→ Follow [Phase 5 — Agent kickoff](#phase-5--agent-kickoff) below before coding.**

---

## Testing checklist

| Scenario | SaleKind | Omzet Period list | Sales Period list |
|----------|----------|-------------------|-------------------|
| Order Jan, no KembaliFaktur | OrderedSale | **Hidden** | Visible (SalesDate Jan) |
| Order Jan, KembaliFaktur Feb | OrderedSale | Visible in Feb omzet | Visible in Jan sales |
| Direct faktur, KembaliFaktur in Mar | DirectSale | Visible in Mar | Visible per SalesDate |
| Void faktur | — | Removed | Removed |
| Second materialize same period | — | No duplicate IDs | — |

---

## Rule-change playbook

| Business ask | Edit |
|--------------|------|
| Omzet recognition date | `ApplyOmzetDate` (KembaliFaktur only) |
| Tanggal Jual / freeze rule | `SetSalesDateOnCreate` + linker |
| Omzet vs Sales period logic | `SalesOmzetPeriodPolicy` |
| Ordered vs Direct classification | `SalesOmzetSaleKindPolicy` |
| Status labels / colors | `SalesOmzetStatusPolicy` + form |
| Void / exclude | `SalesOmzetEligibilityPolicy` |

---

## DI registration

- `IReconcileSalesOmzetWorker` → `INunaServiceVoid<>` (Scrutor).
- DAL → `IInsert` / `IUpdate` / `IListData` (Scrutor).
- Policies → explicit `AddScoped<>` in `Program.cs` or scan `*Policy` in `SalesOmzetAgg.Policies`.

---

## Out of scope

- Changes to Create Order / Save Faktur.
- Event-driven reconcile on save (optional later).
- Replacing `BTR_OrderMap` in one go.
- Showing outstanding in Omzet Period (strict omzet is fixed).

---

## Phase 1 — Agent kickoff

Use this section when starting a **new agent session** for Phase 1 only. The rest of this document is context; this section is the **execution contract**.

### Copy-paste prompt (new session)

```text
Implement Phase 1 only from docs/plans/sales-omzet-aggregate-implementation.md
(sections "Phase 1 — Agent kickoff" and "Database: BTR_SalesOmzet").

Rules:
- Create ISalesOmzetEntityDal + SalesOmzetEntityDal — do NOT replace or rename
  btr.application/SalesContext/OrderFeature/ISalesOmzetDal.cs or SalesOmzetView.
- Do NOT implement policies, linker, ReconcileSalesOmzetWorker, or SalesOmzetInfoForm changes.
- Do NOT remove btr.infrastructure/SalesContext/SalesPersonAgg/SalesOmzetDal.cs (legacy UNION).
- Match VARCHAR lengths from kickoff table below (not the rounded placeholders in the SQL sketch).
- Add BTR_SalesOmzet.sql to btr.sql.sqlproj; add <Compile Include=...> to btr.domain,
  btr.application, btr.infrastructure .csproj files.
- SalesOmzetWriter: INunaCounterBL like SalesPersonWriter (prefix SO, IDFormatEnum.PFnnn).
- Ensure BTR_ParamNo has a row for prefix SO (seed script or document manual INSERT).
- Definition of done: solution builds; table on dev DB; insert/get/update one row via writer.

Templates: SalesPersonDal, SalesPersonWriter, BTR_Faktur.sql, BTR_Customer.sql.
```

### Naming — avoid collision with existing RO2 read API

| Piece | Location / name | Phase 1 |
|-------|-----------------|---------|
| **Aggregate CRUD** | `ISalesOmzetEntityDal` / `SalesOmzetEntityDal` in `SalesOmzetAgg` | **Create** |
| **Aggregate model** | `SalesOmzetModel`, `ISalesOmzetKey` in `btr.domain` | **Create** |
| **RO2 read (legacy)** | `ISalesOmzetDal` + `SalesOmzetView` in `OrderFeature` | **Do not change** |
| **RO2 query (legacy)** | `infrastructure/.../SalesPersonAgg/SalesOmzetDal.cs` | **Do not change** |

Phase 4 will wire `ISalesOmzetDal.ListData` to read `BTR_SalesOmzet` (and period policy). Phase 1 only persists the entity.

### Phase 1 — In scope

1. `btr.sql/Tables/SalesContext/BTR_SalesOmzet.sql` — full DDL + indexes (see column lengths below).
2. `btr.sql.sqlproj` — `<Build Include="Tables\SalesContext\BTR_SalesOmzet.sql" />`.
3. Domain: `SalesOmzetModel`, `ISalesOmzetKey`, `SaleKindEnum`, `SalesOmzetStatusEnum`, `SalesOmzetPeriodFilterMode` (enums used later; define now for stable schema).
4. Application: `ISalesOmzetEntityDal`, `ISalesOmzetWriter` / `SalesOmzetWriter`.
5. Infrastructure: `SalesOmzetEntityDal` — `Insert`, `Update`, `GetData(ISalesOmzetKey)`, `GetByOrderId`, `GetByFakturId`.
6. `BTR_ParamNo` seed for prefix `SO` (if no project seed exists, add `DataSeeds/BTR_ParamNo_SalesOmzet.sql` as `<None>` or document dev INSERT).
7. Register files in **all** relevant `.csproj` files (this repo uses explicit `<Compile Include>`).

### Phase 1 — Out of scope

- `ISalesOmzetSourceDal`, all `Policies/`, `ISalesOmzetLinker`, `ReconcileSalesOmzetWorker`
- `SalesOmzetInfoForm` / `Program.cs` policy registration
- Changing `ISalesOmzetDal.ListData` or deleting legacy `SalesOmzetDal` UNION
- Create Order / Save Faktur use cases

### Column lengths (use in SQL — align with existing tables)

| Column | VARCHAR | Source |
|--------|---------|--------|
| `SalesOmzetId` | 13 | Same order of magnitude as `BTR_Piutang.PiutangId` |
| `OrderId` | 26 | `BTR_Faktur.OrderId` |
| `FakturId` | 15 | `BTR_Faktur.FakturId` |
| `SaleKind` | 15 | `OrderedSale`, `DirectSale` |
| `FakturCode` | 11 | `BTR_Faktur.FakturCode` |
| `SalesPersonName` | 30 | `BTR_SalesPerson.SalesPersonName` |
| `CustomerName` | 50 | `BTR_Customer.CustomerName` |
| `Code` | 10 | `BTR_Customer.CustomerCode` |
| `Alamat` | 60 | `BTR_Customer.Address1` |
| `OmzetStatus` | 20 | Status labels |

Use `DATETIME` defaults `'3000-01-01'` and `DECIMAL(18,2)` like other `BTR_*` sales tables.

### `ISalesOmzetEntityDal` (suggested surface)

```csharp
public interface ISalesOmzetEntityDal :
    IInsert<SalesOmzetModel>,
    IUpdate<SalesOmzetModel>,
    IGetData<SalesOmzetModel, ISalesOmzetKey>
{
    SalesOmzetModel GetByOrderId(string orderId);
    SalesOmzetModel GetByFakturId(string fakturId);
}
```

Scrutor in `Program.cs` already registers `IInsert<>`, `IUpdate<>`, `IGetData<,>` from the infrastructure assembly — no extra registration needed if the interface extends those markers.

### `SalesOmzetWriter` (suggested behavior)

- Mirror `SalesPersonWriter`: validate (optional in Phase 1), `Generate("SO", IDFormatEnum.PFnnn)` when `SalesOmzetId` empty, then `Insert` or `Update`.
- **Do not** set `SalesDate` / business fields here in Phase 1 — caller or Phase 2 linker/snapshot will populate; writer only assigns ID and persists.
- For manual smoke test, set required columns on `SalesOmzetModel` explicitly.

### Definition of done (Phase 1)

- [ ] Solution builds (`btr.domain`, `btr.application`, `btr.infrastructure`, `btr.distrib`).
- [ ] `BTR_SalesOmzet` deployed to dev database (publish script or manual run).
- [ ] `BTR_ParamNo` contains `Prefix = 'SO'`.
- [ ] Can create one `SalesOmzet` row (writer or DAL), read by `SalesOmzetId`, `GetByOrderId`, `GetByFakturId`, update a field.
- [ ] RO2 form still opens and runs (legacy `ISalesOmzetDal` unchanged).

### Reference files (read first)

| File | Why |
|------|-----|
| `btr.infrastructure/SalesContext/SalesPersonAgg/SalesPersonDal.cs` | Dapper insert/update/get pattern |
| `btr.application/SalesContext/SalesPersonAgg/Workers/SalesPersonWriter.cs` | `INunaCounterBL` ID generation |
| `btr.sql/Tables/SalesContext/BTR_Faktur.sql` | `OrderId`, `FakturId`, `FakturCode` lengths |
| `btr.sql/Tables/SalesContext/BTR_Customer.sql` | Customer field lengths |
| `btr.application/SalesContext/OrderFeature/ISalesOmzetDal.cs` | **Do not break** — `SalesOmzetView` |
| `btr.domain/SalesContext/FakturControlAgg/StatusFakturEnum.cs` | `KembaliFaktur` (for Phase 2 snapshot) |

### Open default (Phase 1 may document in code comment)

**DirectSale `SalesDate`:** default `FakturDate` at create (Phase 2 linker). Phase 1 schema only needs the `SalesDate` column.

---

## Phase 2 — Agent kickoff

Use this section when starting a **new agent session** for Phase 2 only. Read the full artifact for business context; this section is the **execution contract**.

See also: [Policy interfaces](#policy-interfaces), [Business vocabulary](#business-vocabulary-locked), [Testing checklist](#testing-checklist).

### Copy-paste prompt (new session)

The full prompt for Phase 2 is maintained in this section. Copy from **"BEGIN PHASE 2 PROMPT"** through **"END PHASE 2 PROMPT"** into a new agent session.

---

**BEGIN PHASE 2 PROMPT**

Implement **Phase 2 — Policies & snapshots** for the Sales Omzet aggregate.

**Primary reference:** `docs/plans/sales-omzet-aggregate-implementation.md` (entire document; especially Business vocabulary, Policy interfaces, Phase 2 — Agent kickoff, Testing checklist).

**Prerequisite:** Phase 1 is complete. Use existing code — do not recreate Phase 1 artifacts.

### Context

We are replacing the legacy on-the-fly RO2 query (`btr.infrastructure/SalesContext/SalesPersonAgg/SalesOmzetDal.cs`) with a **`SalesOmzet` aggregate**. Phase 1 delivered persistence only. Phase 2 delivers **all business rules** as testable policies + snapshot/linker services that Phase 3’s `ReconcileSalesOmzetWorker` will orchestrate.

**Do not change** Create Order or Save Faktur use cases.

### Phase 1 artifacts (already exist — use these)

| Artifact | Path |
|----------|------|
| Table | `btr.sql/Tables/SalesContext/BTR_SalesOmzet.sql` |
| Model | `btr.domain/SalesContext/SalesOmzetAgg/SalesOmzetModel.cs` |
| Enums | `SaleKindEnum`, `SalesOmzetStatusEnum`, `SalesOmzetPeriodFilterMode` |
| Entity DAL | `ISalesOmzetEntityDal` / `SalesOmzetEntityDal` |
| Writer | `ISalesOmzetWriter` / `SalesOmzetWriter` (prefix `SO`, `IDFormatEnum.PFnnn`) |
| DAL test | `btr.test/SalesContext/SalesOmzetEntityDalTest.cs` |

**Legacy (do not modify in Phase 2):**

- `btr.application/SalesContext/OrderFeature/ISalesOmzetDal.cs` + `SalesOmzetView`
- `btr.infrastructure/SalesContext/SalesPersonAgg/SalesOmzetDal.cs` (UNION query)
- `SalesOmzetInfoForm`

### Phase 2 — In scope

Create under `btr.application/SalesContext/SalesOmzetAgg/`:

1. **`Snapshots/`** (or `Contracts/Snapshots/`) — lightweight read DTOs, not full domain aggregates:
   - `OrderSnapshot`, `FakturSnapshot`, `CustomerSnapshot`, `FakturControlStatusSnapshot` (minimal fields for policies/snapshot builder)

2. **`Contracts/ISalesOmzetSourceDal.cs`** + **`btr.infrastructure/.../SalesOmzetSourceDal.cs`**
   - Methods to load snapshots by id or by `Periode` (for future reconcile scope), e.g.:
     - `IEnumerable<OrderSnapshot> ListOrders(Periode periode)` or `ListOrdersForReconcile(Periode scope)`
     - `IEnumerable<FakturSnapshot> ListFakturs(Periode periode)` — **no** faktur-date filter when loading by `OrderId` for link (add `GetFakturByOrderId`, `GetFakturByFakturId`, `GetOrderByOrderId`)
     - `CustomerSnapshot GetCustomer(string customerId)`
     - `IEnumerable<FakturControlStatusSnapshot> ListControlStatus(string fakturId)` or use existing `IFakturControlStatusDal` from application layer if preferable
   - SQL: align with `OrderDal`, `FakturDal` patterns; faktur lists must filter `VoidDate = '3000-01-01'` at source or in eligibility policy (pick one place, document it)

3. **`Policies/`** — interface + default implementation for each:
   - `ISalesOmzetEligibilityPolicy` / `SalesOmzetEligibilityPolicy`
   - `ISalesOmzetSaleKindPolicy` / `SalesOmzetSaleKindPolicy`
   - `ISalesOmzetStatusPolicy` / `SalesOmzetStatusPolicy`
   - `ISalesOmzetPeriodPolicy` / `SalesOmzetPeriodPolicy`
   - `ISalesOmzetSnapshotBuilder` / `SalesOmzetSnapshotBuilder`

4. **`Services/`**
   - `ISalesOmzetLinker` / `SalesOmzetLinker` — uses `ISalesOmzetEntityDal`, `ISalesOmzetWriter`, `ISalesOmzetSnapshotBuilder`, `ISalesOmzetSaleKindPolicy`, eligibility

5. **Unit tests** in `btr.test` (e.g. `btr.test/SalesContext/SalesOmzetPoliciesTest.cs`) — **no DB required** for policy tests; use constructed `SalesOmzetModel` + snapshots

6. **DI:** Register all policies + linker + snapshot builder + source DAL in `btr.distrib/Program.cs` (`AddScoped<>`), or Scrutor scan `SalesOmzetAgg.Policies` if you add a marker pattern

7. **`.csproj`** — add `<Compile Include=...>` for all new files in `btr.application`, `btr.infrastructure`, `btr.test`

### Phase 2 — Out of scope

- `ReconcileSalesOmzetWorker`, `ReconcileSalesOmzetRequest` (Phase 3)
- Changing `ISalesOmzetDal.ListData` or `SalesOmzetInfoForm` (Phase 4)
- Deleting legacy `SalesOmzetDal` UNION
- Create Order / Save Faktur hooks

### Business rules to implement (locked)

**OmzetDate (Tanggal Omzet):**

- **Not** `FakturDate`
- Set from `FakturControlStatusModel.StatusDate` where `StatusFaktur == StatusFakturEnum.KembaliFaktur`
- Reference: `btr.domain/SalesContext/FakturControlAgg/StatusFakturEnum.cs`, `FakturControlStatusModel.cs`
- Use enum name — **never** magic number `2` in new code
- If no KembaliFaktur status: `OmzetDate = new DateTime(3000, 1, 1)` (sentinel)

**SalesDate (Tanggal Jual) — frozen at create only:**

- **OrderedSale:** `SalesDate = OrderDate` when row is first created from order
- **DirectSale:** `SalesDate = FakturDate` when row is first created from faktur (default; comment in code)
- **Never** update `SalesDate` in `ApplyOrder` / `ApplyFaktur` / refresh — only in linker on insert

**Sale kind:**

- `OrderedSale` — `OrderId` set (faktur may be added later)
- `DirectSale` — no `OrderId`, `FakturId` set

**OmzetStatus (`SalesOmzetStatusPolicy`):**

| Condition | Status |
|-----------|--------|
| OrderedSale, no faktur | `Outstanding` |
| Has faktur, no KembaliFaktur (OmzetDate sentinel) | `PendingOmzet` |
| KembaliFaktur recorded | `Completed` |
| Void / ineligible | `Void` (or linker skips create; `ShouldRemove` for reconcile) |

**Period policy (`ISalesOmzetPeriodPolicy`) — for Phase 4 list; implement now:**

| Mode | UX label | Include when |
|------|----------|--------------|
| `OmzetPeriod` | Periode Omzet | `OmzetDate` in `[Tgl1,Tgl2]` **and** not sentinel — **strict omzet** (no outstanding) |
| `SalesPeriod` | Periode Jual | `SalesDate` in `[Tgl1,Tgl2]` |

Implement `bool IsInPeriod(SalesOmzetModel row, Periode periode, SalesOmzetPeriodFilterMode mode)` and optional `string ToSqlWhere(SalesOmzetPeriodFilterMode mode, DynamicParameters dp)` for Phase 4.

**Linker (`ISalesOmzetLinker`):**

- `SalesOmzetModel FindOrCreateForOrder(OrderSnapshot order)`:
  - If `GetByOrderId` exists → return it
  - Else insert via writer: `SaleKind = OrderedSale`, `SalesDate = order.OrderDate` (parse date), `OrderId` set
- `SalesOmzetModel FindOrCreateForFaktur(FakturSnapshot faktur)`:
  - If `faktur.OrderId` not empty → `GetByOrderId` → attach `FakturId`, **do not change SalesDate**
  - Else `GetByFakturId` or create `DirectSale` with `SalesDate = faktur.FakturDate`
- `void Refresh(SalesOmzetModel row, ...)` — reload snapshots, call snapshot builder + status policy, update via writer (for Phase 3; may stub or implement fully in Phase 2)

**Snapshot builder:**

- `ApplyOrder` — `OrderDate`, `OrderTotal` from `Order.TotalAmount` (not line-item sum), sales name from `SalesName` / `UserEmail` as per plan
- `ApplyFaktur` — faktur fields, customer from `CustomerSnapshot`
- `ApplyOmzetDate` — KembaliFaktur only
- `SetSalesDateOnCreate` — called only from linker on insert

**Eligibility:**

- Faktur: `VoidDate == 3000-01-01` (match `FakturDal` / `OmzetSupplierViewDal`)
- `ShouldRemove(SalesOmzetModel row)` when voided or invalid

### Linker must not filter faktur by report period

When linking faktur to order, load faktur by `OrderId` regardless of whether `FakturDate` falls inside a UI period. Period filtering is **only** in `ISalesOmzetPeriodPolicy` at read time.

### Definition of done (Phase 2)

- [ ] Solution builds (`btr.application`, `btr.infrastructure`, `btr.test`)
- [ ] All policy interfaces have default implementations in `Policies/`
- [ ] `SalesOmzetLinker` can create OrderedSale and DirectSale rows in DB (manual/integration test or extend `SalesOmzetEntityDalTest`)
- [ ] Unit tests pass for:
  - `SalesOmzetPeriodPolicy`: strict omzet hides outstanding; sales period shows outstanding
  - `SalesOmzetStatusPolicy`: Outstanding / PendingOmzet / Completed
  - `SalesOmzetSnapshotBuilder`: OmzetDate from KembaliFaktur; SalesDate not overwritten on refresh
- [ ] No changes to legacy `ISalesOmzetDal` / `SalesOmzetInfoForm` / UNION `SalesOmzetDal`
- [ ] Policies registered in DI

### Reference files

| File | Why |
|------|-----|
| `docs/plans/sales-omzet-aggregate-implementation.md` | Master plan |
| `btr.infrastructure/SalesContext/SalesPersonAgg/SalesOmzetDal.cs` | Wrong rules to avoid (period-scoped faktur join) |
| `btr.infrastructure/SalesContext/OrderFeature/OrderDal.cs` | Order fields |
| `btr.infrastructure/SalesContext/FakturAgg/FakturDal.cs` | Faktur + void filter |
| `btr.infrastructure/SalesContext/FakturControlAgg/FakturControlStatusDal.cs` | Control status / KembaliFaktur |
| `btr.application/SalesContext/SalesPersonAgg/Workers/SalesPersonWriter.cs` | Writer pattern |
| `btr.test/SalesContext/SalesOmzetEntityDalTest.cs` | DB test harness |

### Coding conventions

- Match existing namespace and Dapper `AddParam` patterns
- Prefer injecting existing DALs (`IOrderDal`, `IFakturDal`, etc.) inside `SalesOmzetSourceDal` or `SalesOmzetSnapshotBuilder` if it reduces duplication — but keep **rules** in policy classes, not in SQL
- Sentinel date: `new DateTime(3000, 1, 1)`

**END PHASE 2 PROMPT**

---

## Phase 3 — Agent kickoff

Use this section when starting a **new agent session** for Phase 3 only. Read the full artifact for business context; this section is the **execution contract**.

See also: [Reconcile worker](#reconcile-worker), [Testing checklist](#testing-checklist), [Phase 2 — Agent kickoff](#phase-2--agent-kickoff).

### Copy-paste prompt (new session)

Copy from **BEGIN PHASE 3 PROMPT** through **END PHASE 3 PROMPT**.

---

**BEGIN PHASE 3 PROMPT**

Implement **Phase 3 — Reconcile worker** for the Sales Omzet aggregate.

**Primary reference:** `docs/plans/sales-omzet-aggregate-implementation.md` (Reconcile worker, Business vocabulary, Testing checklist, Phase 3 — Agent kickoff).

**Prerequisites:** Phase 1 (entity persistence) and Phase 2 (policies, source DAL, linker) are **complete**. Compose existing services — do not reimplement business rules in the worker.

### Context

`ReconcileSalesOmzetWorker` is the **only** process that should create/link/refresh `BTR_SalesOmzet` rows in bulk (until optional scheduler in Phase 5). It orchestrates Phase 2 components inside a transaction. Phase 4 wires this worker from `SalesOmzetMaterializeForm` (not on info `Proses`).

**Do not change:** Create Order, Save Faktur, `ISalesOmzetDal` / `SalesOmzetView`, legacy UNION `SalesOmzetDal`, or `SalesOmzetInfoForm` (Phase 4).

### Phase 2 artifacts (already exist — use as-is)

| Component | Path |
|-----------|------|
| Linker | `ISalesOmzetLinker` / `SalesOmzetLinker` — `FindOrCreateForOrder`, `FindOrCreateForFaktur`, `Refresh` |
| Source | `ISalesOmzetSourceDal` / `SalesOmzetSourceDal` — `ListOrders(Periode)`, `ListFakturs(Periode)`, get-by-id |
| Policies | `Policies/SalesOmzet*.cs` (eligibility, status, sale kind, period, snapshot) |
| Entity DAL | `ISalesOmzetEntityDal` / `SalesOmzetEntityDal` |
| Writer | `ISalesOmzetWriter` / `SalesOmzetWriter` |
| Dates helper | `SalesOmzetDates.Sentinel` |
| DI (policies + linker + source) | `btr.distrib/Program.cs` (lines ~174–180) |
| Policy unit tests | `btr.test/SalesContext/SalesOmzetPoliciesTest.cs` |

**Important linker behavior (Phase 3 must call `Refresh` after link):**

- `FindOrCreateForFaktur` when attaching to an ordered row currently sets `FakturId` and `Save` but does **not** fully hydrate faktur/omzet fields — **`Refresh(row)`** must run after create/link.
- `Refresh` preserves `SalesDate` (frozen); loads faktur via `GetFakturByOrderId` without report-period filter.

### Phase 3 — In scope

1. **`ReconcileSalesOmzetRequest.cs`** (in `UseCases/`)
   - `Periode Periode` (required — same window as RO2, max 122 days validated in form later; optional validation in worker)
   - `string UserId` (optional — for future audit; set `LastReconciledAt` regardless)
   - `ReconcileSalesOmzetScope Scope` enum (see below)

2. **`ReconcileSalesOmzetScope` enum** (domain or application)
   - `PeriodeScoped` (default) — process `ListOrders(periode)` + `ListFakturs(periode)`; refresh all touched rows
   - `Full` (optional) — process all orders/fakturs or all `BTR_SalesOmzet` rows; document if deferred

3. **`IReconcileSalesOmzetWorker`** : `INunaServiceVoid<ReconcileSalesOmzetRequest>`
   **`ReconcileSalesOmzetWorker`** — orchestration only:

   | Step | Action |
   |------|--------|
   | 1 | `using (var trans = TransHelper.NewScope())` |
   | 2 | Load `orders = _source.ListOrders(request.Periode)` |
   | 3 | `HashSet<string>` or list of `SalesOmzetId` **touched** |
   | 4 | For each eligible order: `row = _linker.FindOrCreateForOrder(order)`; if row != null → add id → `_linker.Refresh(row)` |
   | 5 | Load `fakturs = _source.ListFakturs(request.Periode)` |
   | 6 | For each eligible faktur: `row = _linker.FindOrCreateForFaktur(faktur)`; if row != null → add id → `_linker.Refresh(row)` |
   | 7 | **Refresh pass (if not refreshed inline):** for each touched id, `GetData` → `_linker.Refresh(row)` — use single refresh after create/link to avoid duplicate saves |
   | 8 | **Optional scope refresh:** load existing `BTR_SalesOmzet` rows overlapping periode (add `ISalesOmzetEntityDal.ListForReconcileScope(Periode)` if needed) and `Refresh` any not yet touched — ensures stale rows update when only source lists changed |
   | 9 | **Cleanup:** rows marked `Void` by eligibility during refresh stay in DB (linker sets `OmzetStatus = Void`); physical `DELETE` only if you add explicit policy — default: leave Void rows, Phase 4 list filters them out |
   | 10 | `trans.Complete()` |

   **Idempotency:** second run with same data must not create duplicate `SalesOmzetId` (unique `OrderId` / `FakturId`).

   **Transaction pattern:** match `ChangeToCashFakturWorker` / `SaveFakturWorker` — `TransHelper.NewScope()` + `Complete()`.

4. **Optional DAL extension** (if needed for step 8):

   ```csharp
   // ISalesOmzetEntityDal
   IEnumerable<SalesOmzetModel> ListForReconcileScope(Periode periode);
   ```

   SQL: rows where `SalesDate`, `OmzetDate`, `OrderDate`, or `FakturDate` between `@Tgl1` and `@Tgl2` (wide net for scoped reconcile). Document in XML comment.

5. **`.csproj`** entries for new files in `btr.application` (and test project if added).

6. **DI:** `IReconcileSalesOmzetWorker` auto-registers via Scrutor `INunaServiceVoid<>` scan in `Program.cs` — verify no manual registration required.

7. **Tests** (recommended):
   - `btr.test/SalesContext/SalesOmzetReconcileTest.cs` — integration test against dev DB (mirror `SalesOmzetEntityDalTest` setup), OR
   - Manual test script documented in PR description.

   **Acceptance scenarios** (from plan testing checklist):

   | Action | Expected |
   |--------|----------|
   | Reconcile order Jan, no faktur | One `OrderedSale`, `Outstanding`, `SalesDate` = Jan |
   | Reconcile again after KembaliFaktur Feb | Same `SalesOmzetId`, `OmzetDate` Feb, `SalesDate` still Jan, status `Completed` |
   | Direct faktur reconcile | `DirectSale`, `SalesDate` = faktur date |
   | Second reconcile same period | No duplicate rows |

### Phase 3 — Out of scope

- `SalesOmzetInfoForm` changes / injecting worker into form (Phase 4)
- `ISalesOmzetDal.ListData(periode, SalesOmzetPeriodFilterMode)` (Phase 4)
- Removing legacy `SalesPersonAgg/SalesOmzetDal.cs` UNION
- Scheduled job / `btr.sync` (Phase 5)
- Changing Phase 2 policy rules (only fix bugs if reconcile exposes them)

### Reconcile scope vs period filter (do not confuse)

| Concept | Purpose |
|---------|---------|
| **Reconcile `Periode`** | Limits which **source** orders/fakturs are **processed** in this run (`ListOrders` / `ListFakturs`). |
| **Linker `GetFakturByOrderId`** | No faktur-date filter — faktur outside periode still links on refresh. |
| **Omzet Period / Sales Period** | **Read** filter for RO2 grid (Phase 4) — **not** used inside reconcile worker. |

### Suggested worker structure (pseudocode)

```csharp
public void Execute(ReconcileSalesOmzetRequest request)
{
    var touched = new Dictionary<string, SalesOmzetModel>(StringComparer.Ordinal);

    using (var trans = TransHelper.NewScope())
    {
        foreach (var order in _source.ListOrders(request.Periode))
        {
            var row = _linker.FindOrCreateForOrder(order);
            if (row != null) touched[row.SalesOmzetId] = row;
        }

        foreach (var faktur in _source.ListFakturs(request.Periode))
        {
            var row = _linker.FindOrCreateForFaktur(faktur);
            if (row != null) touched[row.SalesOmzetId] = row;
        }

        // Optional: merge ListForReconcileScope into touched

        foreach (var row in touched.Values)
            _linker.Refresh(row);

        trans.Complete();
    }
}
```

Avoid double-`Refresh` if you already refresh inside the loop — pick **one** pattern (refresh once after all creates is clearer).

### Definition of done (Phase 3)

- [ ] Solution builds
- [ ] `IReconcileSalesOmzetWorker.Execute` runs without error for a 1–3 month `Periode` on dev DB
- [ ] Creates ordered row without faktur; second run after faktur+KembaliFaktur updates same `SalesOmzetId`
- [ ] Direct faktur creates `DirectSale` row
- [ ] No duplicate `SalesOmzetId` for same `OrderId` / `FakturId`
- [ ] `SalesDate` unchanged after faktur link + refresh (Jan order / Feb omzet case)
- [ ] Legacy RO2 form still uses old `ISalesOmzetDal` and works unchanged
- [ ] (Recommended) At least one automated or documented manual test for idempotency

### Reference files

| File | Why |
|------|-----|
| `docs/plans/sales-omzet-aggregate-implementation.md` | Master plan |
| `SalesOmzetLinker.cs` | Create/link/refresh behavior |
| `ISalesOmzetSourceDal.cs` | Scoped source lists |
| `ChangeToCashFakturWorker.cs` | `TransHelper.NewScope` pattern |
| `SalesOmzetEntityDalTest.cs` | DB connection pattern for tests |
| `SalesOmzetPoliciesTest.cs` | Policy expectations |

### Coding conventions

- Namespace: `btr.application.SalesContext.SalesOmzetAgg.UseCases`
- Worker implements `INunaServiceVoid<ReconcileSalesOmzetRequest>` — no return value
- Log counts optional: orders processed, fakturs processed, rows refreshed (simple `Debug` or leave for Phase 5)
- Do not embed SQL in the worker — use source DAL + linker only

**END PHASE 3 PROMPT**

---

## Phase 4 — Agent kickoff

Use this section when starting a **new agent session** for Phase 4 only. Read the full artifact for business context; this section is the **execution contract**.

See also: [Read path & form](#read-path--form), [RO2 UX — period filter](#ro2-ux--period-filter), [Testing checklist](#testing-checklist), [Phase 3 — Agent kickoff](#phase-3--agent-kickoff).

### Copy-paste prompt (new session)

Copy from **BEGIN PHASE 4 PROMPT** through **END PHASE 4 PROMPT**.

---

**BEGIN PHASE 4 PROMPT**

Implement **Phase 4 — RO2 integration** for the Sales Omzet aggregate.

**Primary reference:** `docs/plans/sales-omzet-aggregate-implementation.md` (Read path & form, RO2 UX, Testing checklist, Phase 4 — Agent kickoff).

**Prerequisites:** Phase 1–3 are **complete** (`BTR_SalesOmzet`, policies, linker, `ReconcileSalesOmzetWorker`).

### Context

RO2 (`SalesOmzetInfoForm`) currently calls legacy `ISalesOmzetDal.ListData(periode)` which runs the **UNION query** in `btr.infrastructure/SalesContext/SalesPersonAgg/SalesOmzetDal.cs`. Phase 4 switches the read path to **`BTR_SalesOmzet`** and adds the **Omzet Period / Sales Period** user choice. Reconcile runs from **`SalesOmzetMaterializeForm`**, not on info **Proses** (faster reporting).

### Phase 3 artifacts (already exist)

| Component | Path |
|-----------|------|
| Reconcile | `IReconcileSalesOmzetWorker` / `ReconcileSalesOmzetWorker` |
| Request | `ReconcileSalesOmzetRequest` (`Periode`, `UserId`, `Scope`) |
| Period policy | `ISalesOmzetPeriodPolicy.ToSqlWhere(mode)` — strict omzet + sales period |
| Entity table | `BTR_SalesOmzet` via `SalesOmzetEntityDal` |
| Filter enum | `SalesOmzetPeriodFilterMode` (`OmzetPeriod`, `SalesPeriod`) |
| RO2 form | `btr.distrib/.../SalesOmzetInfoForm.cs` + `.Designer.cs` |
| Read contract | `btr.application/.../OrderFeature/ISalesOmzetDal.cs` + `SalesOmzetView` |
| Legacy read (replace) | `btr.infrastructure/.../SalesPersonAgg/SalesOmzetDal.cs` |

### Phase 4 — In scope

#### 1. Extend `ISalesOmzetDal` (keep in `OrderFeature` for form compatibility)

```csharp
public interface ISalesOmzetDal : IListData<SalesOmzetView, Periode>
{
    IEnumerable<SalesOmzetView> ListData(Periode periode, SalesOmzetPeriodFilterMode mode);
}
```

- Implement **`ListData(Periode)`** as **`ListData(periode, SalesOmzetPeriodFilterMode.OmzetPeriod)`** (default = strict omzet / Periode Omzet).
- Scrutor must still resolve `ISalesOmzetDal` — one implementation class.

#### 2. Replace legacy read implementation

**Remove the entire UNION CTE SQL** from `SalesOmzetDal` (or move implementation to `SalesOmzetAgg/SalesOmzetListDal.cs` and update `.csproj` — same interface).

New `ListData(periode, mode)`:

```sql
SELECT <columns mapped to SalesOmzetView>
FROM BTR_SalesOmzet
WHERE <ISalesOmzetPeriodPolicy.ToSqlWhere(mode)>
  AND OmzetStatus <> 'Void'   -- exclude void rows from RO2 (adjust if policy changes)
ORDER BY SalesPersonName, OrderDate, FakturDate
```

- Inject `ISalesOmzetPeriodPolicy` into the read DAL.
- Use `DynamicParameters` with `@Tgl1`, `@Tgl2` from `Periode` (match `OrderDal` date param style).
- Map DB row → `SalesOmzetView` (manual or Mapster). Columns already on `SalesOmzetModel` / table match view fields.

**Optional on `SalesOmzetView`:** add `SalesOmzetStatusEnum OmzetStatus` and/or `SaleKindEnum SaleKind` for grid/Excel — only if needed; do not break existing grid column bindings (`SalesPersonName`, `OrderId`, …).

#### 3. `SalesOmzetInfoForm` — Proses flow (read only)

Update constructor DI:

- `ISalesOmzetDal` (read)

**`Proses()` sequence:**

1. Validate period ≤ 122 days (keep existing).
2. Build `Periode` from `Tgl1Date` / `Tgl2Date`.
3. Read period mode from new UI control (see below).
4. **`var listOmzet = _salesOmzetDal.ListData(periode, mode)?.ToList()`**
5. Apply existing in-memory `Filter(listOmzet, SearchText.Text)` and bind grid.

**Materialize:** **Materialisasi** opens `SalesOmzetMaterializeForm` (DI) with dates pre-filled; that form calls `IReconcileSalesOmzetWorker` (scoped or full rebuild).

#### 4. UI — period mode control (minimal layout change)

Add to `panel1` in **Designer** (e.g. left of Search or before Proses):

| Control | Type | Default | Label (suggested) |
|---------|------|---------|-------------------|
| `SalesPeriodCheckBox` | `CheckBox` | **unchecked** | **Periode Jual** |

Behavior:

| Checked? | Mode | UX meaning |
|----------|------|------------|
| No | `SalesOmzetPeriodFilterMode.OmzetPeriod` | **Periode Omzet** (default) — strict omzet |
| Yes | `SalesOmzetPeriodFilterMode.SalesPeriod` | **Periode Jual** — filter by `SalesDate` |

Optional: tooltip on checkbox explaining both modes (Indonesian OK).

**Do not** redesign the grid layout; keep existing columns and summary rows.

#### 5. Excel export — use aggregate status

Replace `GetOrderStatus(orderId, fakturCode)` with display text from **`OmzetStatus`** when available:

| `SalesOmzetStatusEnum` | Excel / display text (suggested) |
|------------------------|----------------------------------|
| `Outstanding` | Outstanding Order |
| `PendingOmzet` | Pending Omzet (or similar) |
| `Completed` + `DirectSale` | Direct Sales (if `OrderId` empty) else Completed Order |
| `Completed` + `OrderedSale` | Completed Order |
| `Void` | (should not appear in list) |

Reuse existing `GetStatusColor` with updated strings or map enum → color in one helper.

#### 6. `.csproj` updates

- If new files (e.g. `SalesOmzetListDal`), update `btr.infrastructure.csproj`.
- If `SalesOmzetView` extended, `btr.application.csproj` only.

#### 7. Verification

- Run / extend tests: `SalesOmzetPoliciesTest` already covers period policy; add optional integration test `ListData` against dev DB.
- Manually verify [Testing checklist](#testing-checklist) scenarios on RO2 after Proses.

### Phase 4 — Out of scope

- `ReconcileSalesOmzetScope.Full` implementation
- Scheduled reconcile (`btr.sync`)
- Removing `BTR_OrderMap`
- Changing Create Order / Save Faktur
- New report menus or grid columns beyond optional status/sale kind

### Period policy (must match Phase 2 — do not reimplement in SQL)

Use **`ISalesOmzetPeriodPolicy.ToSqlWhere`** already implemented:

| Mode | WHERE |
|------|--------|
| `OmzetPeriod` | `OmzetDate BETWEEN @Tgl1 AND @Tgl2 AND OmzetDate <> '3000-01-01'` |
| `SalesPeriod` | `SalesDate BETWEEN @Tgl1 AND @Tgl2` |

Reconcile **always** runs before list; list mode only affects **which rows appear**, not how reconcile links faktur.

### Definition of done (Phase 4)

- [ ] Solution builds; RO2 opens from menu (`MainForm.RO2SalesOmzetMenu_Click`)
- [ ] Proses lists from `BTR_SalesOmzet` only; materialize form runs reconcile (no UNION SQL left in read DAL)
- [ ] Default (unchecked): **Periode Omzet** — outstanding rows **not** shown
- [ ] Checked **Periode Jual**: outstanding ordered sales in range **shown**
- [ ] Order Jan + KembaliFaktur Feb: visible in Feb omzet mode; Jan sales mode shows row with Jan `SalesDate`
- [ ] Excel export works; status column reflects `OmzetStatus`
- [ ] Second materialize for same period is idempotent (no duplicate aggregate rows)
- [ ] Legacy UNION file deleted or reduced to aggregate SELECT only

### Reference files

| File | Why |
|------|-----|
| `docs/plans/sales-omzet-aggregate-implementation.md` | Master plan + checklist |
| `SalesOmzetPeriodPolicy.cs` | `ToSqlWhere` / `IsInPeriod` |
| `ReconcileSalesOmzetWorker.cs` | How to call reconcile |
| `SalesOmzetEntityDal.cs` | Column names for SELECT |
| `SalesOmzetInfoForm.cs` | Proses, Excel, Filter, grid init |
| `SalesOmzetInfoForm.Designer.cs` | Add checkbox |
| `SalesPersonAgg/SalesOmzetDal.cs` | **Remove UNION** |
| `ISalesOmzetDal.cs` | Extend interface |
| `MainForm.cs` | `UserId` for reconcile request |
| `FakturForm.cs` | Pattern for `MainForm` user |

### Coding conventions

- WinForms designer: keep `partial` class split; wire checkbox in `.Designer.cs`
- Do not register read DAL manually if Scrutor already picks up `IListData<SalesOmzetView, Periode>` — ensure single `ISalesOmzetDal` implementation
- Sentinel dates in grid: may display `3000-01-01` for empty faktur/order dates — consider formatting empty as blank in mapper (optional polish)

**END PHASE 4 PROMPT**

---

## Phase 5 — Agent kickoff

Use this section when starting a **new agent session** for Phase 5 only. Phases 1–4 should be **complete** (aggregate, policies, materialize form + list-only RO2 Proses, read from `BTR_SalesOmzet`).

See also: [Testing checklist](#testing-checklist), [Out of scope](#out-of-scope), [Phase 3 — Agent kickoff](#phase-3--agent-kickoff).

### Copy-paste prompt (new session)

Copy from **BEGIN PHASE 5 PROMPT** through **END PHASE 5 PROMPT**.

---

**BEGIN PHASE 5 PROMPT**

Implement **Phase 5 — Hardening** for the Sales Omzet aggregate.

**Primary reference:** `docs/plans/sales-omzet-aggregate-implementation.md` (Testing checklist, Phase 5 — Agent kickoff, entire plan for business rules).

**Prerequisites:** Phases 1–4 are done. RO2 info form calls `ISalesOmzetDal.ListData(periode, mode)`; reconcile is on `SalesOmzetMaterializeForm`. Legacy UNION SQL is **removed** from read DAL.

### Current state (do not redo Phases 1–4)

| Area | Location |
|------|----------|
| Table + seed | `btr.sql/Tables/SalesContext/BTR_SalesOmzet.sql`, `DataSeeds/BTR_ParamNo_SalesOmzet.sql` |
| Reconcile (scoped) | `ReconcileSalesOmzetWorker` — `PeriodeScoped` only; **`Full` throws `NotSupportedException`** |
| RO2 | `SalesOmzetInfoForm` — `SalesPeriodCheckBox`, list-only Proses; `SalesOmzetMaterializeForm` for reconcile |
| Read | `SalesPersonAgg/SalesOmzetDal.cs` → `BTR_SalesOmzet` + `ISalesOmzetPeriodPolicy` |
| Tests | `SalesOmzetPoliciesTest`, `SalesOmzetEntityDalTest`, `SalesOmzetReconcileTest` |

### Phase 5 goals

1. **Production backfill** — ability to populate/refresh **all** `BTR_SalesOmzet` rows after deploy (not only a 3-month UI window).
2. **Operational visibility** — reconcile returns or logs counts (orders processed, fakturs processed, rows refreshed, duration).
3. **Deploy runbook** — documented steps for DB publish + first full reconcile.
4. **Quality** — extend automated tests for full scope and checklist scenarios where feasible.
5. **Optional ops UX** — safe admin path to run full reconcile without hacking code.

**Not required in Phase 5:** wiring `btr.sync` (separate .NET 4.8 app, no reference to `btr.application`) unless you add a clearly isolated follow-up doc only.

### Phase 5 — In scope

#### 1. Implement `ReconcileSalesOmzetScope.Full`

Today (`ReconcileSalesOmzetWorker`):

```csharp
if (request.Scope == ReconcileSalesOmzetScope.Full)
    throw new NotSupportedException("Full reconcile scope is not implemented yet.");
```

**Implement Full** by processing **all** eligible orders and fakturs (or a configurable wide range), not only `ListOrders(periode)` / `ListFakturs(periode)`.

Suggested approach (pick one, document in code):

| Approach | Description |
|----------|-------------|
| **A — Source DAL expansion** | Add `ISalesOmzetSourceDal.ListAllOrders()` / `ListAllFakturs()` (SQL without date filter; still exclude void faktur). Use in worker when `Scope == Full`. |
| **B — Wide periode** | `Periode` from min order/faktur date to today — only if A is too heavy; document risk. |

Full reconcile loop: same as scoped (find/create → `ListForReconcileScope` optional → `Refresh` all touched). For Full, **omit** `ListForReconcileScope(periode)` or replace with “all rows in `BTR_SalesOmzet`” refresh pass.

**Transaction:** keep single `TransHelper.NewScope()` for Full only if batch size is acceptable; if timeout risk, document batching strategy (e.g. reconcile by calendar year) in runbook.

#### 2. `ReconcileSalesOmzetResult` (metrics)

Add result DTO returned from worker (change to `INunaService<ReconcileSalesOmzetResult, ReconcileSalesOmzetRequest>` **or** add optional out-parameter / mutable result on request — prefer `INunaService<,>` if other workers use it; else keep `INunaServiceVoid` and attach `ReconcileSalesOmzetResult Result { get; set; }` on request after execute).

Suggested metrics:

- `int OrdersProcessed`
- `int FaktursProcessed`
- `int RowsRefreshed` (touched count)
- `int RowsCreated` (optional — if tracked in linker)
- `TimeSpan Duration`
- `ReconcileSalesOmzetScope Scope`

Populate counts in `ReconcileSalesOmzetWorker` during loops.

#### 3. Logging / diagnostics

This solution has **no** widespread `ILogger` in `btr.application`. Use a **lightweight** approach:

- `System.Diagnostics.Trace` or `Debug.WriteLine` with one line summary after reconcile, **and/or**
- Store last result on request for UI, **and/or**
- Optional: write to a simple ops log table — **only if** a pattern already exists (avoid new infra unless needed).

**RO2 (optional polish):** after Proses reconcile, show non-blocking feedback e.g. `ToolTip` or status label: *"Reconcile: N rows refreshed (Xs)"* — only if minimal UI change is acceptable in Phase 5.

#### 4. Deploy & backfill runbook

Add **`docs/plans/sales-omzet-deploy.md`** (or `docs/ops/sales-omzet-deploy.md`) containing:

1. Publish `btr.sql` objects: `BTR_SalesOmzet`, indexes, `BTR_ParamNo` seed (`SO`).
2. Deploy application binaries (`btr.distrib`, `btr.infrastructure`, `btr.application`, `btr.domain`).
3. **First-time backfill** — run **Full** reconcile once:
   - Option A: xUnit test / console harness documented as manual step (`SalesOmzetReconcileTest` with `[Fact]` + `Scope = Full`, or new `SalesOmzetFullReconcileTest`).
   - Option B: temporary admin button on RO2 *"Rebuild omzet (semua data)"* with `MessageBox` confirm + `Scope = Full` (guard with warning).
4. Normal ops: users continue **Proses** on RO2 (scoped reconcile for selected period).
5. Rollback note: legacy UNION removed — restore from git if emergency; table can remain.

#### 5. Extend tests (`btr.test`)

| Test | Purpose |
|------|---------|
| `UT3_FullReconcile_CompletesWithoutError` | Smoke Full scope (mark `[Trait("Integration")]` if slow; skip on CI without DB) |
| `UT4_ReconcileResult_HasNonNegativeCounts` | Metrics populated |
| Policy tests for checklist rows | Jan order / Feb KembaliFaktur — unit-level on `ISalesOmzetPeriodPolicy` if not already covered |

Do **not** require production DB credentials in committed code — follow existing `SalesOmzetEntityDalTest` pattern or use config.

#### 6. Code hygiene (small, targeted)

- Confirm **no** remaining UNION read SQL in `SalesOmzetDal`.
- Optional: format sentinel dates as blank in grid/Excel (`3000-01-01` → empty display) in `SalesOmzetDal.MapToView` or form.
- Optional: document `BTR_OrderMap` vs `SalesOmzet` — **do not remove** `OrderMap` unless product owner confirms (plan: separate task).

#### 7. `.csproj` / DI

- Register any new types; if worker signature changes to `INunaService<ReconcileSalesOmzetResult, ...>`, Scrutor still auto-registers.
- Update `SalesOmzetMaterializeForm` if extending reconcile metrics UI.

### Phase 5 — Out of scope

- Event-driven reconcile on Save Faktur / Create Order
- Replacing `BTR_OrderMap` in production flows
- Integrating reconcile into **`btr.sync`** timer (different project stack) — mention in runbook as future work
- Changing business rules in policies (unless fixing a production bug found during backfill)
- New report menus

### Definition of done (Phase 5)

- [ ] `ReconcileSalesOmzetScope.Full` runs without `NotSupportedException` on dev DB
- [ ] Reconcile exposes metrics (`ReconcileSalesOmzetResult` or equivalent)
- [ ] Deploy/backfill runbook exists and describes first Full reconcile
- [ ] At least one test or documented manual step for Full reconcile
- [ ] RO2 scoped Proses still works (regression)
- [ ] Solution builds; no duplicate `OrderId`/`FakturId` after Full + scoped runs

### Reference files

| File | Why |
|------|-----|
| `ReconcileSalesOmzetWorker.cs` | Implement Full + metrics |
| `ISalesOmzetSourceDal.cs` / `SalesOmzetSourceDal.cs` | Add list-all or wide queries |
| `ReconcileSalesOmzetScope.cs` | Enum |
| `SalesOmzetInfoForm.cs` | Optional metrics / full rebuild UI |
| `SalesOmzetReconcileTest.cs` | Extend integration tests |
| `btr.sync/SyncForm.cs` | Reference only — different architecture |
| `docs/plans/sales-omzet-aggregate-implementation.md` | Testing checklist |

### Suggested Full reconcile pseudocode

```csharp
if (request.Scope == ReconcileSalesOmzetScope.Full)
{
    orders = _source.ListAllOrders();
    fakturs = _source.ListAllFakturs();
}
else
{
    orders = _source.ListOrders(request.Periode);
    fakturs = _source.ListFakturs(request.Periode);
}
// same touched + Refresh loop; for Full, refresh all BTR_SalesOmzet rows after (optional second pass)
```

### Production backfill command (document in runbook)

Example for operators (adjust dates/method):

```text
1. Deploy database + app.
2. Run Full reconcile once (test harness or admin button).
3. Open RO2, select Periode Omzet, run Proses for current quarter to verify row counts.
4. Compare sample rows against legacy expectations (Testing checklist).
```

**END PHASE 5 PROMPT**

---

## Summary

**Sales Omzet** is an aggregate with its own ID, **Ordered Sale** vs **Direct Sale**, **`SalesDate` (Tanggal Jual)** frozen at create, and **`OmzetDate`** from **KembaliFaktur** only. RO2 defaults to **Omzet Period** (strict — no outstanding); users can switch to **Sales Period** to see the sales-month view without dates drifting when omzet is recognized. All volatile rules live in **Policies/**; reconcile is the sole writer.
