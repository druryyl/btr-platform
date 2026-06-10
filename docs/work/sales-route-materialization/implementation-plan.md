# Implementation Plan: Sales Route Materialization & Exception Visits

## Document Status

| Field | Value |
| --- | --- |
| Initiative | Territory Execution Plan — dated visit schedule materialization and exception management |
| Authoritative requirements | `docs/investigations/sales-route-materialization-analysis.md` |
| Related roadmap | M25 Sales Force Effectiveness (future consumer — route compliance metrics blocked until this foundation ships) |
| Solution | `src/j05-btr-distrib` (BTR Desktop + SQL + scheduled worker) |
| Author role | Architect |
| Implementer input | This document |
| Status | **Ready for implementation** |
| Open questions | **Resolved** — see Section 2 (one menu-id conflict flagged) |

---

## 1. Goal

Introduce a **dated operational visit plan** (Territory Execution Plan) derived from the existing recurring route template (`BTR_SalesRute` / `BTR_SalesRuteItem`), with a **separate exception overlay** for one-off schedule changes. Preserve SM4-Rute as the recurring template editor.

**Primary outcomes:**

- Calendar expansion from template using a **fixed anchor date** and **continuous 14-day cycle** (Minggu-1 / Minggu-2, Mon–Sat only).
- **Hybrid materialization:** regenerate future rows on SM4 template save **and** a scheduled worker maintaining a rolling planning horizon.
- **Historical immutability:** past materialized rows never change when templates or exceptions are edited later.
- **Exception management** form for supervisors: add / remove / replace customers on a specific date without touching the template.
- **Effective plan query** combining materialized base + exceptions — foundation for M25 route compliance joins via `SalesPerson.Email`.

**Explicitly out of scope (per analysis):**

- Visit Dashboard, Call Dashboard, Route Compliance Dashboard, Effective Call Dashboard, KPI design.
- BTrade3 mobile route sync and API endpoints.
- Substitute-salesman coverage.
- Exception approval workflow.
- FT1 / FT2 calendar-date default (future integration).
- Portal `BTRPD_*` snapshot pattern — route plans require **dated historical retention**, not `SnapshotKey = 'CURRENT'` alone.

---

## 2. Authoritative Business Decisions

Source: analysis Section 10. Do not re-decide during implementation.

| # | Decision | Value |
| - | -------- | ----- |
| D1 | Cycle rule | Fixed anchor date + continuous 14-day cycle; **no** reset on month/quarter/year |
| D2 | Weekday scope | Monday–Saturday only; **Sunday excluded** (no materialized rows) |
| D3 | Materialization strategy | **Hybrid:** SM4 save triggers future regen + scheduled worker for rolling horizon |
| D4 | Retention | **Indefinite** — all historical effective plans retained |
| D5 | Template change scope | **Future dates only** — past materialized rows immutable |
| D6 | Exception types (v1) | Add, Remove, Replace only |
| D7 | Holidays / leave | Via exceptions (SM6 form), **not** template edits |
| D8 | Exception authorization | **Supervisor level and above** — salesmen cannot modify planned schedules |
| D9 | Approval workflow | **None** in v1 |
| D10 | Identity bridge | `BTR_SalesPerson.Email` mandatory and authoritative for check-in / order joins |
| D11 | Business framing | **Territory Execution Plan** — same template serves sales visits **and** collection |
| D12 | BTrade3 sync | **Not required** in this milestone |

### 2.1 Architect implementation defaults (not open business questions)

These are configuration choices the Architect assigns; change only with business approval:

| Parameter | Recommended default | Storage |
| --------- | ------------------- | ------- |
| Route cycle anchor date | `2026-01-05` (Monday — first Monday of 2026; adjust at deploy if business specifies otherwise) | `BTR_ParamSistem` code `ROUTE_CYCLE_ANCHOR_DATE` |
| Rolling horizon | **90 days** ahead from today | `BTR_ParamSistem` code `VISIT_PLAN_HORIZON_DAYS` |
| Worker schedule | Daily at 02:00 local | Windows Task Scheduler |
| Past-date edit guard | Block exception CRUD when `VisitDate < today` (server date via `ITglJamDal`) | Application policy |
| Regeneration start date | `max(today, requestedFromDate)` for template-triggered regen | Application policy |

### 2.2 Menu ID conflict (requires confirmation at implementation kickoff)

Analysis Q15 specifies **SM6 – Jadwal Kunjungan**, but the Desktop ribbon already uses **SM6** for **Principal Target** (`SalesPersonPrincipalTargetForm` in `MainForm.Designer.cs`). SM6 is **not** in `BTR_Menu.dataseed.sql`.

**Architect recommendation:** Register the new form as **SM7 – Jadwal Kunjungan** under MASTR (GroupOrder 11) to avoid collision. If business insists on SM6, Principal Target must be renumbered first — treat as a separate breaking change.

---

## 3. Architecture Overview

### 3.1 Conceptual model

```text
BTR_ParamSistem (anchor date, horizon days)
        │
        ▼
IRuteCycleCalendar ──► calendar date → HariRuteId (or null if Sunday)
        │
BTR_HariRute (12 slots)
        │
        ▼
BTR_SalesRute + BTR_SalesRuteItem          ◄── SM4-Rute (template master, unchanged role)
        │
        │  [NEW] template expansion
        ▼
BTR_VisitPlan                              ◄── dated rows, Source = Template
        │                                      past rows immutable
        │  [NEW] exception overlay
        ▼
BTR_VisitPlanException                     ◄── SM7 Jadwal Kunjungan
        │
        ▼
IEffectiveVisitPlanResolver                ◄── base ⊕ exceptions (query/service layer)
        │
        ▼
Future: M25 compliance joins
   BTR_CheckIn (CheckInDate, CustomerId, UserEmail)
   BTR_Order   (OrderDate, CustomerId, UserEmail)
   via SalesPerson.Email
```

### 3.2 Layer responsibilities

| Layer | Responsibility | Mutability |
| ----- | -------------- | ---------- |
| **Template** (`BTR_SalesRute*`) | Recurring Territory Execution Plan | Current state only; SM4 overwrites slot items |
| **Materialized base** (`BTR_VisitPlan`) | Template expanded to `(SalesPersonId, VisitDate, CustomerId)` | Future rows regenerated on template change; **past rows never updated or deleted** |
| **Exceptions** (`BTR_VisitPlanException`) | Dated add / remove / replace | CRUD via SM7; past dates blocked |
| **Effective plan** | Computed at read time (or cached view) | Derived — not a separate mutable table in v1 |

### 3.3 Why not portal snapshot pattern

Portal dashboards (`BTRPD_*`, `SnapshotKey = 'CURRENT'`) optimize for **current-state** analytics. Route visit compliance requires answering *"who was planned on 2026-06-15?"* after template changes — that needs **dated persistence** similar to `BTR_SalesOmzetHealthWeekly`, not snapshot replace.

### 3.4 Topology

```text
SM4-Rute save
  → SalesRuteWriter.Save (existing)
  → IRegenerateVisitPlanWorker.Execute(salesPersonId, fromDate=today)

Windows Task Scheduler (daily)
  → btr.visitplan.worker.exe --triggered-by Scheduler
  → IMaintainVisitPlanHorizonWorker.Execute()

SM7 Jadwal Kunjungan
  → IVisitPlanExceptionWriter.Save / Delete
  → (no base-plan rewrite — exceptions stored separately)

RO1 / RO3 / M25 (future)
  → IEffectiveVisitPlanDal.ListEffectivePlan(salesPersonId, visitDate)
```

---

## 4. Impact Analysis

### 4.1 Affected modules

| Module | Change type | Detail |
| ------ | ----------- | ------ |
| `btr.sql` | **New tables** | `BTR_VisitPlan`, `BTR_VisitPlanException`; ParamSistem seeds |
| `btr.sql/DataSeeds/BTR_Menu.dataseed.sql` | Modify | Add SM7 menu row |
| `btr.sql/DataSeeds/BTR_ParamSistem*.sql` | **New seed** | Anchor date + horizon days |
| `btr.domain/SalesContext/VisitPlanAgg/` | **New** | Domain models, enums, keys |
| `btr.application/SalesContext/VisitPlanAgg/` | **New** | Calendar, materialization, effective-plan resolver, workers |
| `btr.infrastructure/SalesContext/VisitPlanAgg/` | **New** | DAL implementations |
| `btr.application/SalesContext/SalesRuteAgg/SalesRuteWriter.cs` | Modify | Post-save hook to regen future plans for affected salesman |
| `btr.distrib/SalesContext/VisitPlanAgg/` | **New** | SM7 `JadwalKunjunganForm` |
| `btr.distrib/SharedForm/MainForm.*` | Modify | SM7 ribbon button + click handler |
| `btr.visitplan.worker/` | **New project** | Scheduled console worker (mirror `btr.portal.worker` structure) |
| `btr.test/SalesContext/` | **New** | Calendar + effective-plan unit tests |

### 4.2 Unaffected modules (regression only)

| Module | Reason |
| ------ | ------ |
| `SalesRuteForm` UI / drag-drop behavior | Template editing unchanged except downstream regen side effect |
| `BTR_CheckIn`, `BTR_Order` | No schema or write-path changes |
| FT1 / FT2 | Continue manual HariRute checkbox selection |
| BTrade3 / sync APIs | Out of scope |
| `btr.portal.worker` / `BTRPD_*` | Separate concern |

### 4.3 Systems

| System | Impact |
| ------ | ------ |
| BTR Desktop | New SM7 form; SM4 save triggers background regen |
| SQL Server | New tables; indefinite row growth (approved — low volume) |
| Windows Task Scheduler | New daily job for horizon maintenance |
| BTR Portal | None in this milestone |

---

## 5. Database Design

### 5.1 `BTR_VisitPlan` — materialized template expansion

```sql
CREATE TABLE BTR_VisitPlan
(
    VisitPlanId     VARCHAR(13) NOT NULL,   -- ULID
    SalesPersonId   VARCHAR(5)  NOT NULL,
    VisitDate       DATE        NOT NULL,
    CustomerId      VARCHAR(6)  NOT NULL,
    NoUrut          INT         NOT NULL,
    HariRuteId      VARCHAR(3)  NOT NULL,
    PlanSource      VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_VisitPlan_PlanSource DEFAULT('Template'),
    MaterializedAt  DATETIME    NOT NULL,

    CONSTRAINT PK_BTR_VisitPlan PRIMARY KEY (VisitPlanId),
    CONSTRAINT UX_BTR_VisitPlan UNIQUE (SalesPersonId, VisitDate, CustomerId)
);
CREATE INDEX IX_BTR_VisitPlan_VisitDate ON BTR_VisitPlan (VisitDate);
CREATE INDEX IX_BTR_VisitPlan_SalesPersonDate ON BTR_VisitPlan (SalesPersonId, VisitDate);
```

**Semantics:**

- One row per planned customer visit per salesman per calendar date (from template).
- `PlanSource` reserved for future use (`Template` only in v1).
- `HariRuteId` records which slot produced the row (audit / debugging).
- **Immutability:** application never UPDATE/DELETE rows where `VisitDate < today`.

### 5.2 `BTR_VisitPlanException` — dated overlay

```sql
CREATE TABLE BTR_VisitPlanException
(
    VisitPlanExceptionId  VARCHAR(13) NOT NULL,   -- ULID
    SalesPersonId         VARCHAR(5)  NOT NULL,
    VisitDate             DATE        NOT NULL,
    ExceptionType         VARCHAR(10) NOT NULL,   -- Add | Remove | Replace
    CustomerId            VARCHAR(6)  NOT NULL,   -- target customer (removed/added/replaced-from)
    ReplacementCustomerId VARCHAR(6)  NOT NULL CONSTRAINT DF_BTR_VisitPlanException_Replacement DEFAULT(''),
    CreatedAt             DATETIME    NOT NULL,
    CreatedByUserId       VARCHAR(20) NOT NULL,

    CONSTRAINT PK_BTR_VisitPlanException PRIMARY KEY (VisitPlanExceptionId)
);
CREATE INDEX IX_BTR_VisitPlanException_Lookup
    ON BTR_VisitPlanException (SalesPersonId, VisitDate);
```

**Exception semantics:**

| Type | Fields | Effect on effective plan |
| ---- | ------ | ------------------------ |
| **Add** | `CustomerId` | Include customer even if not on template that day |
| **Remove** | `CustomerId` | Exclude customer even if on template that day |
| **Replace** | `CustomerId` → `ReplacementCustomerId` | Remove source; add replacement (single atomic exception row) |

**Validation rules:**

- `ReplacementCustomerId` required non-empty only when `ExceptionType = Replace`.
- Reject duplicate conflicting exceptions for same `(SalesPersonId, VisitDate, CustomerId, ExceptionType)`.
- Reject all CRUD when `VisitDate < today`.
- Replace: source must exist in base plan or prior add-exception; replacement must not duplicate an existing effective customer.

### 5.3 System parameters (seed)

| ParamCode | ParamValue (initial) | Purpose |
| --------- | -------------------- | ------- |
| `ROUTE_CYCLE_ANCHOR_DATE` | `2026-01-05` | Epoch for Minggu-1 / Minggu-2 cycle |
| `VISIT_PLAN_HORIZON_DAYS` | `90` | Rolling materialization window |

Administer via existing **XX2 – Parameter Sistem** form (`BTR_ParamSistem`); no new config form required in v1.

### 5.4 Counter / ID generation

Follow existing patterns:

- `VisitPlanId`, `VisitPlanExceptionId`: **ULID** (consistent with portal snapshot IDs) **or** `INunaCounterBL` with new prefix if ULID not available in Desktop DAL path — Implementer should match whichever ID strategy is already used in the target DAL projects (prefer ULID for new tables).

---

## 6. Application Design

### 6.1 Calendar service — `IRuteCycleCalendar`

**Location:** `btr.application/SalesContext/VisitPlanAgg/Services/`

```text
ResolveHariRuteId(DateTime visitDate) → string? 
  - null if Sunday
  - else map via anchor date + 14-day cycle + weekday → H11..H26

GetCycleWeekLabel(DateTime visitDate) → Minggu1 | Minggu2  (display helper)
```

**Algorithm (approved business rule):**

```text
if visitDate.DayOfWeek == Sunday → return null

daysSinceAnchor = (visitDate.Date - anchorDate.Date).Days
if daysSinceAnchor < 0 → throw or clamp per policy (anchor must be ≤ any materialized date)

cycleDayIndex = daysSinceAnchor mod 14        // 0..13
weekInCycle = cycleDayIndex < 7 ? 1 : 2     // Minggu-1 or Minggu-2
weekdayIndex = (int)visitDate.DayOfWeek      // Mon=1 .. Sat=6 (map Sunday already excluded)

HariRuteId = weekInCycle == 1 ? "H1{weekdayIndex}" : "H2{weekdayIndex}"
// e.g. Monday Minggu-1 → H11; Thursday Minggu-2 → H24
```

Implement as pure service with injectable anchor from `IParamSistemDal`. **Unit test exhaustively** — this is the highest-risk business logic component.

### 6.2 Materialization — `IRegenerateVisitPlanWorker`

**Trigger:** SM4 save (single salesman) and full-horizon maintenance worker.

**Input:** `RegenerateVisitPlanRequest { SalesPersonId?, FromDate, ToDate, TriggeredBy }`

- `SalesPersonId` null = all salesmen (scheduled worker).
- `FromDate` = `max(today, requestedFrom)` — **never regenerate past dates**.

**Steps:**

1. Load template items: join `BTR_SalesRute` + `BTR_SalesRuteItem` for target salesman(s).
2. For each date D in `[FromDate, ToDate]`:
   - Skip Sunday.
   - Resolve `HariRuteId` via calendar.
   - Find route header for `(SalesPersonId, HariRuteId)`.
   - Emit planned customers with `NoUrut` from template.
3. Within transaction for each `(SalesPersonId, date range)` batch:
   - `DELETE FROM BTR_VisitPlan WHERE SalesPersonId = @sp AND VisitDate >= @fromDate AND VisitDate <= @toDate`  
     (only future portion — `@fromDate >= today`)
   - Bulk insert new rows with fresh `MaterializedAt`.
4. **Do not touch** `BTR_VisitPlanException`.

**Empty template slot:** No rows materialized for that date — valid state (planning gap).

### 6.3 Horizon maintenance — `IMaintainVisitPlanHorizonWorker`

**Schedule:** Daily via `btr.visitplan.worker.exe`.

**Logic:**

```text
today = ITglJamDal.Now.Date
horizonDays = int(ParamSistem VISIT_PLAN_HORIZON_DAYS)
targetToDate = today.AddDays(horizonDays)

RegenerateVisitPlan(all salesmen, fromDate=today, toDate=targetToDate)
```

Also detect and fill **gaps** if worker was offline (idempotent regen handles this).

### 6.4 Effective plan resolver — `IEffectiveVisitPlanResolver`

**Location:** `btr.application/SalesContext/VisitPlanAgg/Services/`

**Input:** `(SalesPersonId, VisitDate)` or date range.

**Output:** Ordered list of `{ CustomerId, NoUrut, Origin }` where `Origin` ∈ `Template | Added | Replaced`.

**Algorithm:**

```text
base = BTR_VisitPlan rows for (sp, date)
exceptions = BTR_VisitPlanException rows for (sp, date)

result = base customers as Template

for each Remove exception → remove customer from result
for each Replace exception → remove CustomerId; add ReplacementCustomerId as Replaced
for each Add exception → add CustomerId as Added (if not already present)

sort by NoUrut (added/replaced append after template order with incrementing NoUrut)
```

Expose via `IEffectiveVisitPlanDal` for future RO/M25 consumers. SM7 form uses this for display.

### 6.5 SM4 integration point

Modify `SalesRuteWriter.Save` — after successful transaction:

```csharp
_regenerateVisitPlanWorker.Execute(new RegenerateVisitPlanRequest
{
    SalesPersonId = model.SalesPersonId,
    FromDate = _tglJamDal.Now.Date,
    ToDate = null, // worker computes horizon end from ParamSistem
    TriggeredBy = "TemplateSave"
});
```

Run **outside** the SM4 DB transaction to avoid long locks. Failure should log error but **not roll back** template save (template is authoritative; regen can be retried by scheduled worker).

### 6.6 Authorization for SM7

BTR Desktop authorization is **menu-based** via `BTR_Role` / `BTR_RoleMenu` (`MainForm.SetupUserMenu`).

| Role | Access SM7 |
| ---- | ---------- |
| `SYSAD` | Yes (all menus) |
| `LEADR` (Leader Sales) | Yes — assign via Role maintenance |
| `SFKTR` (Supervisor Faktur) | Yes — assign via Role maintenance |
| `SALES` (User Sales) | **No** |
| `FAKTR`, `PAJAK` | No (unless explicitly granted) |

**Implementation:**

1. Add `SM7` to `BTR_Menu` dataseed.
2. Add `BTR_RoleMenu` seed rows for `LEADR`, `SFKTR`, `SYSAD`.
3. Do **not** add to `SALES` role seed.
4. Existing deployments: document manual Role Menu assignment in ops notes.

No separate approval workflow (D9).

---

## 7. UI Design — SM7 Jadwal Kunjungan

### 7.1 Form concept (not pixel spec)

**Class:** `JadwalKunjunganForm` in `btr.distrib/SalesContext/VisitPlanAgg/`

**Pattern reference:** `SalesRuteForm` (salesman + customer grids) — but date-scoped, not HariRute radios.

**Capabilities:**

| Capability | Behavior |
| ---------- | -------- |
| Select salesman | Combo box — all active salesmen |
| Select visit date | Date picker — default today; disallow past dates for edits |
| View effective base plan | Read-only grid showing template-derived plan (`BTR_VisitPlan`) |
| View effective plan | Grid showing base ⊕ exceptions (via resolver) |
| Add customer | Pick customer not on effective plan → creates `Add` exception |
| Remove customer | Select row on effective plan → creates `Remove` exception |
| Replace customer | Select row → pick replacement → creates `Replace` exception |
| List exceptions | Grid of exceptions for selected date (with delete) |
| Exception list by range | Optional secondary view: date range filter listing exceptions |

**Auto-save:** Match SM4 pattern — persist exception immediately on action.

**Do not:** Edit `BTR_SalesRuteItem` from this form.

### 7.2 Optional read-only viewer

Defer dedicated INFO-form viewer to post-v1. SM7 displays both base and effective plans — sufficient for operations in initial release.

---

## 8. Scheduled Worker — `btr.visitplan.worker`

### 8.1 Project structure

Mirror `btr.portal.worker`:

| File | Purpose |
| ---- | ------- |
| `Program.cs` | Entry point |
| `WorkerRunCoordinator.cs` | Parse `--triggered-by Scheduler|Manual` |
| `WorkerDependencyConfig.cs` | DI registration |
| `appsettings.json` | Database connection |

**CLI:**

```text
btr.visitplan.worker.exe [--triggered-by Scheduler|Manual]
```

Single domain — no `--domain` switch needed.

### 8.2 Task Scheduler registration

| Setting | Value |
| ------- | ----- |
| Task name | `BTR-VisitPlan-Horizon` |
| Schedule | Daily 02:00 |
| Command | `btr.visitplan.worker.exe --triggered-by Scheduler` |
| Stop if running | > 30 minutes |

Document in `src/j05-btr-distrib/docs/ops/visit-plan-deploy.md` (new ops runbook).

### 8.3 Initial backfill

On first deploy after schema migration:

1. Confirm `ROUTE_CYCLE_ANCHOR_DATE` seed value with business.
2. Run worker manually: `--triggered-by Manual` to materialize `[today, today + horizon]`.
3. **Do not backfill historical dates before go-live** unless business explicitly requests — no template history exists to reconstruct past plans accurately.

---

## 9. Risk Assessment & Mitigations

| Risk | Severity | Mitigation |
| ---- | -------- | ---------- |
| Calendar algorithm error | **High** | Pure service + comprehensive unit tests; golden test vectors from business examples |
| SM6 menu collision | **Medium** | Use SM7; document in release notes |
| SM4 save + regen failure | **Medium** | Log error; scheduled worker fills gaps; template save still succeeds |
| Dual maintenance confusion (template vs exception) | **Medium** | SM7 shows base vs effective side-by-side; ops runbook documents three layers |
| Orphan customers on plan | **Low** | Display customer name; no automatic master cleanup (matches existing SM4 behavior) |
| Indefinite storage growth | **Low** | Approved; ~tens salesmen × tens customers × 365 days/year — negligible |
| Accidental past-date edit | **Medium** | Server-side date guard on exception writer; UI disables past dates |
| Portal CURRENT snapshot anti-pattern | **Medium** | Dedicated dated tables — do not reuse `BTRPD_*` pattern |
| Long-running regen on SM4 save | **Low** | Async/out-of-transaction; single salesman scope only |

---

## 10. Implementation Phases

Implement in order. Each phase should be deployable and testable before the next.

### Phase 1 — Foundation (database + calendar + tests)

1. Create SQL tables `BTR_VisitPlan`, `BTR_VisitPlanException`.
2. Add ParamSistem seeds (`ROUTE_CYCLE_ANCHOR_DATE`, `VISIT_PLAN_HORIZON_DAYS`).
3. Add domain models and enums (`VisitPlanModel`, `VisitPlanExceptionModel`, `VisitPlanExceptionTypeEnum`).
4. Implement `IRuteCycleCalendar` + unit tests (`RuteCycleCalendarTest.cs`).
5. Implement DAL interfaces and infrastructure.

**Exit criteria:** Calendar tests pass; tables deploy via `btr.sql` project.

### Phase 2 — Materialization pipeline

1. Implement `IRegenerateVisitPlanWorker` + `IMaintainVisitPlanHorizonWorker`.
2. Create `btr.visitplan.worker` console project.
3. Hook `SalesRuteWriter.Save` post-save regen.
4. Integration test: SM4 save → future `BTR_VisitPlan` rows match template for resolved dates.
5. Verify past rows untouched when template changes.

**Exit criteria:** Manual worker run populates 90-day horizon; SM4 edit regenerates future rows only.

### Phase 3 — Exception management (SM7)

1. Implement `IVisitPlanExceptionWriter`, `IEffectiveVisitPlanResolver`.
2. Build `JadwalKunjunganForm`.
3. Register SM7 menu + MainForm ribbon + role seeds.
4. Manual test: add / remove / replace on a future date; verify effective plan.

**Exit criteria:** Supervisor role can manage exceptions; salesman role cannot access SM7.

### Phase 4 — Ops & documentation

1. Write `docs/ops/visit-plan-deploy.md` (deploy, scheduler, backfill, parameter tuning).
2. Update `docs/features/` with permanent feature artifact (Knowledge Curator pass — post-implementation).
3. Smoke test on staging: template change + exception + worker run + effective plan query.

**Exit criteria:** Ops runbook complete; staging sign-off.

---

## 11. Testing Strategy

### 11.1 Unit tests (required)

| Test class | Coverage |
| ---------- | -------- |
| `RuteCycleCalendarTest` | Anchor mapping; Mon–Sat; Sunday null; cycle wrap at 14 days; year boundary (no reset) |
| `EffectiveVisitPlanResolverTest` | Add / Remove / Replace combinations; empty base; duplicate add |
| `RegenerateVisitPlanWorkerTest` | Future-only delete; correct customer set from template; empty slot |

### 11.2 Manual test scenarios

| # | Scenario | Expected |
| - | -------- | -------- |
| T1 | Materialize week containing known anchor | HariRuteId matches manual calculation |
| T2 | Edit SM4 template; save | Future `BTR_VisitPlan` rows update; past unchanged |
| T3 | Add exception for tomorrow | Effective plan includes added customer; template unchanged |
| T4 | Replace customer on date | Effective plan shows replacement; compliance join would not flag false miss |
| T5 | Remove customer on date | Effective plan excludes customer |
| T6 | Attempt exception on past date | Rejected |
| T7 | Sunday date | No materialized rows; SM7 shows empty base |
| T8 | SALES user login | SM7 menu disabled/hidden |
| T9 | LEADR user login | SM7 accessible |
| T10 | Worker offline 3 days, then run | Horizon extended; no duplicate rows |

---

## 12. File & Module Checklist

### 12.1 New files (expected)

```text
btr.sql/Tables/SalesContext/BTR_VisitPlan.sql
btr.sql/Tables/SalesContext/BTR_VisitPlanException.sql
btr.sql/DataSeeds/BTR_ParamSistem_VisitPlan.sql

btr.domain/SalesContext/VisitPlanAgg/
  VisitPlanModel.cs
  VisitPlanExceptionModel.cs
  VisitPlanExceptionTypeEnum.cs
  IVisitPlanKey.cs

btr.application/SalesContext/VisitPlanAgg/
  Services/IRuteCycleCalendar.cs
  Services/RuteCycleCalendar.cs
  Services/IEffectiveVisitPlanResolver.cs
  Services/EffectiveVisitPlanResolver.cs
  UseCases/IRegenerateVisitPlanWorker.cs
  UseCases/RegenerateVisitPlanWorker.cs
  UseCases/IMaintainVisitPlanHorizonWorker.cs
  UseCases/MaintainVisitPlanHorizonWorker.cs
  IVisitPlanDal.cs
  IVisitPlanExceptionDal.cs
  IVisitPlanExceptionWriter.cs
  VisitPlanExceptionWriter.cs

btr.infrastructure/SalesContext/VisitPlanAgg/
  VisitPlanDal.cs
  VisitPlanExceptionDal.cs
  EffectiveVisitPlanDal.cs

btr.distrib/SalesContext/VisitPlanAgg/
  JadwalKunjunganForm.cs
  JadwalKunjunganForm.Designer.cs

btr.visitplan.worker/
  (mirror btr.portal.worker layout)

btr.test/SalesContext/
  RuteCycleCalendarTest.cs
  EffectiveVisitPlanResolverTest.cs

docs/ops/visit-plan-deploy.md
```

### 12.2 Modified files (expected)

```text
btr.sql/btr.sql.sqlproj
btr.sql/DataSeeds/BTR_Menu.dataseed.sql
btr.application/SalesContext/SalesRuteAgg/SalesRuteWriter.cs
btr.distrib/SharedForm/MainForm.cs
btr.distrib/SharedForm/MainForm.Designer.cs
btr.distrib/Program.cs (DI registration)
btr.application/btr.application.csproj
btr.infrastructure/btr.infrastructure.csproj
btr.distrib/btr.distrib.csproj
Solution file (add btr.visitplan.worker project)
```

---

## 13. Future Integration Points (not this milestone)

| Consumer | Integration |
| -------- | ----------- |
| M25 route compliance | Join `IEffectiveVisitPlanDal` ↔ `BTR_CheckIn` / `BTR_Order` via `SalesPerson.Email` |
| FT1 / FT2 | Default HariRute checkboxes from calendar date + materialized plan |
| BTrade3 | Sync effective daily plan to mobile |
| RO1 / RO3 | Optional "planned" column via effective plan join |
| Audit trail | Exception change log if approval workflow added later |

---

## 14. Acceptance Criteria

1. **Calendar expansion** correctly maps any Mon–Sat date to `H11`–`H26` using configured anchor and continuous 14-day cycle.
2. **Materialized rows** exist for `[today, today + horizon]` for all salesmen with route templates.
3. **Template save (SM4)** regenerates future materialized rows for the edited salesman only; **past rows unchanged**.
4. **Scheduled worker** maintains rolling horizon daily without duplicate rows.
5. **SM7 form** allows supervisor to add / remove / replace customers on future dates.
6. **Effective plan** correctly applies exceptions on top of materialized base.
7. **SALES role** cannot access SM7; **LEADR / SFKTR / SYSAD** can.
8. **Sunday** produces no materialized visit plan rows.
9. **Unit tests** cover calendar and effective-plan resolver.
10. **Ops runbook** documents deploy, scheduler setup, and parameter configuration.

---

## 15. Architect Validation Notes

Investigation findings validated against source code:

| Finding | Validated |
| ------- | --------- |
| `SalesRuteWriter` delete-all + bulk insert | Confirmed — `SalesRuteWriter.cs` lines 44–45 |
| No existing materialization worker | Confirmed — only `btr.portal.worker` exists |
| SM4 in MASTR menu | Confirmed — `BTR_Menu.dataseed.sql` |
| SM6 collision with Principal Target | **New finding** — `MainForm.Designer.cs` uses SM6 for Principal Target; recommend SM7 |
| `BTR_ParamSistem` for configuration | Confirmed — existing pattern via XX2 form |
| Role-based menu access | Confirmed — `MainForm.SetupUserMenu` |
| Portal snapshot unsuitable for history | Confirmed — `materialized-dashboard-architecture.md` documents CURRENT-only pattern |

---

*Document produced by Architect Agent. Ready for Implementer execution upon approval.*
