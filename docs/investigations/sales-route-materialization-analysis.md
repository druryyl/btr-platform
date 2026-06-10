# Sales Route Materialization & Exception Visit Analysis

**Analyst artifact** — discovery and analysis only.  
**Scope:** Route planning, schedule materialization, and exception visit management.  
**Explicitly out of scope:** Visit Dashboard, Call Dashboard, Route Compliance Dashboard, Effective Call Dashboard, KPI design, implementation plans, database schema, and code.

**Related roadmap context:** Route-based visit analytics are reserved for **M25 Sales Force Effectiveness** (deferred from M18/M20). This analysis informs that foundation without designing M25 deliverables.

**Status:** Open questions Q1–Q15 have been **resolved** — see [Section 10 — Approved Business Decisions](#10-approved-business-decisions). Document is ready for Architect handoff.

---

## 1. Executive Summary

BTR stores routes as a **recurring two-week Territory Execution Plan**, not as dated visit schedules. Three tables — `BTR_HariRute`, `BTR_SalesRute`, and `BTR_SalesRuteItem` — define which customers a salesman should visit on each slot (e.g. Minggu-1 Senin). The same template serves **both sales visits and collection activities** — BTR has no dedicated collection personnel. The two-week cycle is intentionally aligned with the company's default **14-day receivable cycle**.

Maintenance happens exclusively through Desktop form **SM4-Rute**. Route data supports **operational planning and collection UI filtering**; it does **not** participate in transactional posting, mobile check-in, or sales order capture.

Future analytics requiring comparison of **Planned Visit × Check-In × Sales Order** on a **calendar date** cannot be answered reliably from the current model. Check-ins and orders are date-based; routes are template-based with **no persisted link** between a calendar date and a `HariRuteId`. Finance forms (`FT1`, `FT2`) let users **manually select** route-day checkboxes instead of deriving the day from a date.

**Materialization is feasible.** Business rules for calendar expansion are now **approved**: a **fixed anchor date** drives a **continuous 14-day cycle** (Minggu-1 / Minggu-2) that never resets on month, quarter, or year boundaries. Materialization shall use a **hybrid approach** — regenerate future schedules when templates change, plus a **scheduled worker** maintaining a rolling planning horizon. Historical effective plans are **retained indefinitely**; template changes affect **future dates only**.

**Exception visits** (add / remove / replace customers for a specific date without changing the template) have **no existing support** but are **approved for initial scope**. Holidays and salesman leave are handled through exception management, not template edits. Exception management belongs in **SM6 – Jadwal Kunjungan** (Master menu), authorized for **supervisor level and above**, with **no approval workflow** in the initial implementation. Substitute-salesman coverage and BTrade3 route sync are **out of scope**.

**Identity bridge resolved:** `BTR_SalesPerson.Email` is **mandatory and authoritative** for BTrade3 login and shall be the official link between route planning (`SalesPersonId`), check-in (`UserEmail`), and sales order (`UserEmail`).

**Recommendation (analysis-level):** Proceed to Architect with approved business rules. Model materialized schedules as **operational visit plans** (Territory Execution Plan), not sales-only plans. Preserve SM4-Rute as the recurring template master; introduce dated materialization, exception overlay, and SM6 exception management as separate capabilities.

---

## 2. Existing Route Architecture

### 2.1 Data model

| Table | Role | Key fields | Notes |
| ----- | ---- | ---------- | ----- |
| `BTR_HariRute` | Route-day slot dictionary | `HariRuteId`, `HariRuteName`, `ShortName` | 12 fixed rows seeded: Minggu-1 and Minggu-2 × Senin–Sabtu (no Minggu/Sunday slot) |
| `BTR_SalesRute` | Route header per salesman per slot | `SalesRuteId`, `SalesPersonId`, `HariRuteId` | Unique index on `(SalesPersonId, HariRuteId)` — at most one route per salesman per slot |
| `BTR_SalesRuteItem` | Ordered customer list | `SalesRuteId`, `NoUrut`, `CustomerId` | Unique `(SalesRuteId, CustomerId)` — customer appears once per slot |

**HariRute seed pattern** (`BTR_HariRuteDataSeed.sql`):

| HariRuteId | HariRuteName | ShortName |
| ---------- | ------------ | --------- |
| H11–H16 | Minggu-1 Senin … Sabtu | Senin-1 … Sabtu-1 |
| H21–H26 | Minggu-2 Senin … Sabtu | Senin-2 … Sabtu-2 |

This is a **template coordinate system** (week-in-cycle + weekday), not a calendar date.

**Approved business framing:** The route template is a **Territory Execution Plan** — a unified plan for sales and collection on the same customer visits. The 14-day recurring cycle aligns with the default receivable due cycle (visit → order → invoice → 14-day due → revisit → collection opportunity).

### 2.2 Application layer

| Component | Location | Responsibility |
| --------- | -------- | -------------- |
| `SalesRuteForm` | `btr.distrib/SalesContext/SalesRuteAgg/` | Desktop UI — template maintenance |
| `SalesRuteBuilder` | `btr.application/SalesContext/SalesRuteAgg/` | Load or create route aggregate by salesman + HariRute |
| `SalesRuteWriter` | same | Persist header; **delete all items then bulk re-insert** on save |
| `SalesRuteDal` | `btr.infrastructure/.../SalesRuteAgg/` | CRUD on `BTR_SalesRute` |
| `SalesRuteItemDal` | same | Bulk insert / delete items on `BTR_SalesRuteItem` |
| `SalesRuteItemViewDal` | same | Cross-slot view: all customers for a salesman with HariRute labels |
| `HariRuteDal` | `btr.infrastructure/.../HariRuteAgg/` | CRUD on `BTR_HariRute` (DAL exists; **no Desktop maintenance form found**) |
| `HariRuteBrowser` | `btr.distrib/Browsers/` | Read-only browser for HariRute selection |

**Legacy naming:** `btr.domain/SalesContext/RuteAgg/RuteModel` exists as a separate domain concept with no active `BTR_Rute` table discovered in the sales route path. Operational routes use `SalesRuteModel` / `SalesRuteItemModel` exclusively.

### 2.3 Workers and reports

| Area | Finding |
| ---- | ------- |
| **Workers** | **None** — no scheduled job materializes or refreshes route data |
| **Route-specific reports** | **None discovered** — no report form or SQL view dedicated to route listing or compliance |
| **Portal** | Route tables are **not** consumed by any `BTRPD_*` snapshot worker today |

### 2.4 Related operational forms (route consumers, not maintainers)

| Menu | Form | Route usage |
| ---- | ---- | ----------- |
| **FT1** | `LunasPiutang2Form` | Loads customers from selected route-day checkboxes (H11–H26) for chosen salesman — collection entry queue |
| **FT2** | `TagihanForm` | Same pattern — filters piutang/tagihan worklist by route-day selection |
| **RO1** | `CheckInInfoForm` | Lists `BTR_CheckIn` by date range — **no route join** |
| **RO3** | `EffectiveCallInfoForm` | Joins check-in to `BTR_Order` by date + customer + email — **no route join** |

Finance forms expose **twelve checkboxes** (Senin-1 … Sabtu-2) mapped to `HariRuteId` values. Users choose which template slots to include; the system does **not** auto-select today's slot from the calendar.

### 2.5 Mobile / sync

| System | Route reference |
| ------ | --------------- |
| **BTrade3** (Android) | **No** SalesRute/HariRute usage found |
| **j06-pkl-btrade-api** | Check-in and order APIs — **no** route endpoints |
| **j07-btrade-sync** | Syncs check-in data — **no** route sync |

Field sales operate without route guidance from BTR mobile apps today.

---

## 3. Existing Route Maintenance Workflow

### 3.1 Who maintains route data

| Actor | Activity |
| ----- | -------- |
| **Sales / operations administrator** | Opens SM4-Rute under Master menu (`MASTR`, menu id `SM4`) |
| **Finance staff** | Consumes route data indirectly via FT1/FT2 day checkboxes — does not edit templates |

No role-based approval workflow, audit log, or effective-dating was discovered for route changes.

### 3.2 SM4-Rute workflow (observed behavior)

1. Select **Sales Person** from combo box.
2. Select **route-day slot** via radio buttons (Senin-1 … Sabtu-2).
3. **Left grid:** searchable customer master list; shows short Hari label for customers already on any of the salesman’s routes.
4. **Right grid:** customers assigned to the selected slot; supports drag-and-drop reordering.
5. Double-click customer on left → add to right grid (duplicate customer per slot prevented).
6. Delete from right grid → auto-save.
7. Reorder → auto-save.
8. **Save semantics:** `SalesRuteWriter` upserts `BTR_SalesRute` header, **deletes all** `BTR_SalesRuteItem` rows for that `SalesRuteId`, then bulk-inserts the current grid state.

**Characteristics:**

- Immediate persistence on each change (no explicit Save button).
- Template is **authoritative current state** only.
- Customer visit order preserved via `NoUrut`.
- A customer may appear on multiple slots (different `HariRuteId`) but not twice on the same slot. **Approved:** multiple slots per customer per salesman is intentional (some customers require multiple visits within a cycle).

### 3.3 How route data is used operationally

| Use case | Behavior | Affects transactions? |
| -------- | -------- | --------------------- |
| **Collection planning (FT1, FT2)** | Filter which customers appear in payment/tagihan worklist | **No** — UI filter only; payments still post against Faktur/Piutang |
| **Visual coverage map (SM4)** | Show which day label each customer is assigned to | **No** |
| **Visit compliance analytics** | Not implemented | N/A |
| **Sales order routing** | Not used | **No** |
| **Check-in validation** | Not used — any customer can be checked in | **No** |

Route data is **planning and convenience**, not a system-enforced constraint on field or finance transactions.

---

## 4. Materialized Schedule Feasibility Analysis

### 4.1 Can templates expand into daily schedules?

**Conceptually yes**, if the following inputs are defined:

| Input | Status |
| ----- | ------ |
| Template customers per `(SalesPersonId, HariRuteId)` | **Available** in `BTR_SalesRute` + `BTR_SalesRuteItem` |
| Weekday for a calendar date | **Trivial** — standard calendar; **Monday–Saturday only** (no Sunday planning — approved) |
| Whether date falls in Minggu-1 or Minggu-2 | **Approved** — fixed anchor date + continuous 14-day cycle |
| Anchor / epoch for the 2-week cycle | **Approved** — configured anchor date; cycle does not reset on calendar boundaries |
| Holiday / salesman leave | **Approved** — handled via exception management (SM6), not template changes |
| Exception overrides per date | **Approved for build** — add / remove / replace only; no substitute salesman in initial scope |

**Expansion logic (approved business rule, not implementation design):**

```text
calendar date
    → weekday (Mon–Sat only; Sunday excluded)
    → Minggu-1 or Minggu-2  (from anchor date + continuous 14-day cycle)
    → HariRuteId (e.g. H14 = Minggu-1 Kamis)
    → BTR_SalesRute for (SalesPersonId, HariRuteId)
    → list of CustomerId from BTR_SalesRuteItem
    → apply exception overlay (if any) for that date
    → effective planned visit list for that date
```

**Note:** The anchor date value itself is a configuration decision for implementation — the *rule* (fixed anchor, continuous cycle) is approved.

### 4.2 Comparison with date-based activities

| Activity | Storage | Date field | Route link |
| -------- | ------- | ---------- | ---------- |
| **Planned visit (template)** | `BTR_SalesRuteItem` | None — `HariRuteId` only | Self |
| **Check-in** | `BTR_CheckIn` | `CheckInDate` (yyyy-MM-dd) | **None** — `CustomerId`, `UserEmail` |
| **Sales order (mobile)** | `BTR_Order` | `OrderDate` | **None** — joined to check-in via customer + email + date in RO3 |
| **Faktur** | `BTR_Faktur` | `FakturDate` | **None** — `SalesPersonId` is transactional, not route-based |

Bridging planned vs actual requires a **common key**: `(SalesPersonId, VisitDate, CustomerId)` joined to field data via **`BTR_SalesPerson.Email`**.

**Approved identity model:** `SalesPerson.Email` is mandatory for BTrade3 mobile login. A user cannot log in unless the email is registered in SalesPerson master data. Email is the **official, authoritative bridge** — not a heuristic — between:

- Route planning (`SalesPersonId` → `SalesPerson.Email`)
- Check-in (`BTR_CheckIn.UserEmail`)
- Sales order (`BTR_Order.UserEmail`)

Future route compliance analysis may safely use this bridge.

### 4.3 Existing materialization patterns in BTR

BTR has established patterns that could inform route schedule materialization (patterns only — not a design proposal):

| Pattern | Example | Relevance to route schedules |
| ------- | ------- | ------------------------------ |
| **Snapshot replace (`CURRENT`)** | `BTRPD_*` portal dashboard tables via `btr.portal.worker` | Pre-compute aggregates; **no historical retention** by default |
| **Dated historical rows** | `BTR_SalesOmzetHealthWeekly` (per ISO year/week) | Retains one row per time bucket — closer to audit needs |
| **Transactional recompute worker** | `ReconcileSalesOmzetWorker` | Background reconciliation from source tables |
| **On-demand materialize (Desktop)** | `SalesOmzetInfoForm` — materialize health metrics per ISO week from UI | Manual trigger pattern |

**Implication:** Portal materialization today optimizes for **current-state dashboards**, not historical "what was planned on date X." Route visit compliance needs **historical planned state**, which aligns more with `BTR_SalesOmzetHealthWeekly`-style **dated persistence** than with `SnapshotKey = 'CURRENT'` alone.

### 4.4 Materialization strategies (analysis options)

| Strategy | Description | Trade-off summary |
| -------- | ----------- | ----------------- |
| **A. Runtime expansion** | Compute planned visits from template + calendar rule at query time | No storage; **cannot reconstruct history** after template changes; exceptions awkward |
| **B. Rolling materialized schedule** | Worker or trigger generates rows for next N days | Supports exceptions and near-future ops; needs refresh when template changes |
| **C. Immutable daily snapshot** | Persist effective plan each day (or on template change) | Best historical audit; highest storage and sync complexity |
| **D. Hybrid** | Template expansion + exception table overlay | Separates recurring plan from one-off changes; matches business exception example |

**Approved approach (Q5):** **Hybrid materialization** — combine strategies B and D:

1. **Regenerate future schedules** when route templates are modified (SM4 save).
2. **Scheduled worker** maintains the rolling planning horizon.

### 4.5 Impact on existing route maintenance

| Area | Impact if materialization introduced |
| ---- | ------------------------------------ |
| **SM4-Rute** | Should remain template editor; changes should propagate to future materialized rows, not rewrite history |
| **FT1 / FT2** | Could eventually select customers by **calendar date** instead of manual checkboxes — behavior change for finance users |
| **Performance** | Template expansion is lightweight; volume driver is `(salesmen × customers × days in window)` |
| **Data quality** | Salesmen without routes, customers on route but suspended — surface as planning gaps; email is mandatory for BTrade3 so identity bridge risk is resolved |

### 4.6 Impact on historical reporting

| Scenario | Today | With materialization |
| -------- | ----- | -------------------- |
| "Who was on the route last month?" | **Cannot answer** — only current template | Answerable if dated rows retained |
| "Was Customer A planned on 2026-06-15?" | **Cannot answer** | Answerable |
| Route change mid-month | Retroactively changes interpretation of entire past | **Approved:** future dates only — historical materialized rows unchanged |

### 4.7 Impact on future analytics (M25 foundation)

Materialized (or historically persisted) daily schedules would provide:

- A stable **planned visit denominator** per date per salesman
- A join path to `BTR_CheckIn` and `BTR_Order` without retroactive template drift
- A container for **exception visits** before analytics consume the data

Without this foundation, M25 metrics (route coverage, visit compliance, effective call by plan) remain **joins against a moving target**.

---

## 5. Historical Integrity Analysis

### 5.1 What happens when a route template changes today

`SalesRuteWriter.Save` performs an **in-place overwrite**:

1. Update `BTR_SalesRute` header (if exists).
2. `DELETE FROM BTR_SalesRuteItem WHERE SalesRuteId = @SalesRuteId`.
3. Bulk insert new item list.

**There is no:**

- Version number or effective date
- Audit trail table
- Soft delete of removed customers
- Trigger capturing prior state

A customer removed from Minggu-1 Senin **vanishes from the template** with no record they were ever planned there.

### 5.2 Can historical route assignments be reconstructed?

| Source | Reconstruct past plan? |
| ------ | ---------------------- |
| `BTR_SalesRute` / `BTR_SalesRuteItem` (current) | **No** — current state only |
| Database backups | **Partial** — operational restore, not application feature |
| Check-in history | Shows **actual** visits, not **planned** |
| Finance route-day checkbox usage | **Not persisted** |
| Application logs | **Not discovered** for route edits |

**Conclusion:** Historical planned coverage **cannot** be reconstructed from BTR application data today.

### 5.3 Would materialized schedules improve auditability?

**Yes**, provided materialized rows are:

- **Dated** (each row tied to `VisitDate` or equivalent business date)
- **Immutable or versioned** once the visit day passes (business policy)
- **Generated from template + exceptions** with a recorded source indicator

A materialized schedule creates an auditable answer to: *"On 2026-06-15, Salesman X was planned to visit customers {A, C, D}."* That statement is impossible to make reliably today.

### 5.4 Template change policy (approved — Q7)

When SM4-Rute changes, **future dates only** are affected. Historical materialized schedules must remain unchanged.

| Policy | Status |
| ------ | ------ |
| **Future-only** | **Approved** — past materialized rows immutable; forward rows regenerated |
| **Retroactive regenerate** | **Rejected** — distorts compliance history |
| **Indefinite retention (Q6)** | **Approved** — historical effective plans retained forever |

Rationale: historical visit plans must remain auditable for compliance analysis, operational audits, and management reviews.

---

## 6. Exception Visit Analysis

### 6.1 Business example (from requirements)

> Salesman normally visits Customer A on Week-1 Monday.  
> For **2026-06-15** management wants: replace A with X, add Y, remove B — **without changing the recurring template.**

This is a **dated exception overlay** on top of the template-derived plan.

### 6.2 Existing capabilities

| Capability | Exists? | Evidence |
| ---------- | ------- | -------- |
| Per-date visit override | **No** | No table or form |
| Add/remove customer for single date | **No** | SM4 edits template for all future lookups |
| Exception approval workflow | **No** | No approval pattern in sales route area |
| Temporary salesman substitution | **No** | No dated assignment table |
| Scheduling calendar UI | **No** | Only 12-slot template radios |

**Closest analogues (different business intent):**

| Module | Pattern | Why not a direct fit |
| ------ | ------- | -------------------- |
| **SM5 Principal Assignment** (`BTR_SalesPersonPrincipalTarget`) | Monthly target per salesman × principal | Financial target, not visit plan; keyed by year/month not arbitrary date |
| **FT1/FT2 route-day checkboxes** | Ad-hoc multi-slot selection | Session UI state; not persisted; not date-specific |
| **Tagihan / collection documents** | Dated transactional documents | Financial artifacts, not visit planning |

### 6.3 Should exceptions be separate from templates?

**Yes — analysis conclusion.**

| Concern | Template (SM4) | Exception layer |
| ------- | -------------- | --------------- |
| Recurrence | Defines repeating 2-week pattern | One-off or date-range override |
| Audience | Route administrator | **Supervisor level and above** (approved — Q8) |
| Change frequency | Occasional structural changes | Frequent operational adjustments |
| Historical meaning | "Default expectation" | "What we actually intended that day" |

Mixing exceptions into `BTR_SalesRuteItem` would corrupt the recurring template and destroy the exception-vs-template distinction.

### 6.4 Are dated schedules required before exception management?

**Practically yes.**

Exceptions are meaningless without answering: *"What was the base plan before the exception?"* That base plan is either:

- Computed at runtime from template + calendar rule, or
- Read from a **materialized dated schedule**

A persisted dated schedule (materialized ahead or snapshotted at start of day) provides:

- Stable base for apply-replace-add-remove operations
- Clear audit: `base + exceptions = effective plan`
- Join target for check-in compliance

Exception-only records without a dated base would require runtime template expansion anyway.

### 6.5 Exception types (approved initial scope — Q3, Q10)

| Type | In initial scope? | Example / use |
| ---- | ----------------- | ------------- |
| **Replace** | **Yes** | Customer A → Customer X on date D |
| **Add** | **Yes** | Include Customer Y on date D (not on template that day) |
| **Remove** | **Yes** | Exclude Customer B on date D (on template but skip visit) |
| **Holiday / leave** | **Yes (via exceptions)** | Handled through exception management — do not edit recurring template (Q3) |
| **Substitute salesman** | **No** | Out of initial scope — separate territory ownership problem (Q10) |

**Not in initial scope:** approval workflow for exceptions (Q9). Current route maintenance operates without approval; auditability may be added later.

### 6.6 Impact on historical reporting

| Question | With exceptions (conceptual) |
| -------- | ---------------------------- |
| Effective plan on date D | `materialized_base(D) ⊕ exceptions(D)` |
| Was visit to X a plan compliance or ad-hoc? | Traceable if add-exception flagged |
| Template unchanged? | SM4 remains source of truth for recurrence |

### 6.7 Impact on future visit compliance analytics

Compliance metrics need:

```text
planned customers on date D  (from materialized schedule + exceptions)
visited customers on date D  (from BTR_CheckIn)
ordered customers on date D  (from BTR_Order)
```

Exceptions must flow into the **planned** set, not the template alone. Otherwise a planned replacement (A→X) would appear as "missed A" and "unplanned visit to X."

---

## 7. Existing Form Discovery

Desktop form concepts only — **no UI design**. Identifies logical placement in the BTR Desktop menu structure.

### 7.1 Current menu neighborhood

| Menu group | Id | Examples | Relationship to routes |
| ---------- | -- | -------- | ---------------------- |
| **MASTR** (Master) | 11 | SM1 Customer, SM2 Sales Person, SM3 Wilayah, **SM4 Rute**, SM5 Principal Assignment | SM4 is template home |
| **INFO** (Sales info) | 13 | RO1 Check-In List, RO3 Effective Call | Actual field activity — downstream of planning |
| **TRS** (Finance transactions) | 40 | FT1 Lunas Piutang, FT2 Tagihan Sales | Route-day filter consumers |

### 7.2 Form concept: Exception Visit Management (approved — Q15)

| Aspect | Decision |
| ------ | -------- |
| **Logical home** | **Master (MASTR)** — approved |
| **Menu** | **SM6 – Jadwal Kunjungan** — approved |
| **Closest existing form** | **SM4-Rute** — same domain entities (salesman, customer, visit intent) |
| **New form required?** | **Yes** — SM4 edits recurring template; exceptions are date-scoped operations |
| **Extend SM4?** | **No** — different lifecycle, risks template corruption, no date dimension in SM4 UI |
| **Authorized users** | **Supervisor level and above** — salesmen must not modify planned schedules (Q8) |

**Conceptual capabilities (not UI spec):**

- Select salesman + **calendar date** (not HariRute radio)
- Display **effective base plan** (from materialized schedule or computed preview)
- Apply replace / add / remove actions
- List existing exceptions for date range
- Optional: copy exceptions to another date

### 7.3 Form concept: Materialized Schedule Viewer (operations)

| Aspect | Recommendation |
| ------ | -------------- |
| **Purpose** | Read-only or ops review — see generated plan for date range |
| **Logical home** | **INFO (13)** alongside RO1/RO3, or SM4 sub-action |
| **New form required?** | **Optional** — could be read-only tab on exception form |
| **Extend RO1?** | **No** — RO1 is actual check-in, not planned visits |

### 7.4 Form concept: Calendar anchor configuration

| Aspect | Recommendation |
| ------ | -------------- |
| **Purpose** | Define Minggu-1/2 anchor (if not hard-coded) |
| **Logical home** | **MASTR** or system configuration — likely **one-time admin** |
| **Existing form** | **None** — `BTR_HariRute` has DAL but no maintenance UI |
| **New form required?** | **Probably yes** unless anchor is fixed by business policy and deployed via seed/config |

### 7.5 Forms that should NOT own exceptions

| Form | Reason |
| ---- | ------ |
| **FT1 / FT2** | Finance collection — different workflow objective |
| **RO1 / RO3** | Reporting on actuals — not planning |
| **BTrade3 mobile** | Out of scope for Desktop discovery; mobile would consume effective plan later |

### 7.6 Approval workflow (approved — Q9)

No approval workflow is required for the **initial implementation** of exception management. BTR sales route maintenance already operates with direct edit and immediate save. Supervisor-level authorization (Q8) provides governance without a formal approval chain. Auditability may be added later if required.

---

## 8. Risks

### 8.1 Technical risks

| Risk | Description | Severity |
| ---- | ----------- | -------- |
| ~~**Undefined Minggu-1/2 calendar rule**~~ | **Resolved (Q1, Q2)** — fixed anchor date, continuous 14-day cycle | — |
| ~~**Salesman ↔ check-in identity**~~ | **Resolved (Q12)** — `SalesPerson.Email` mandatory and authoritative for BTrade3 | — |
| ~~**Sunday visit planning**~~ | **Resolved (Q11)** — not required; Mon–Sat only | — |
| **Anchor date configuration** | Approved rule requires a configured anchor date value — not yet in BTR | **Medium** — implementation detail |
| **Dead SQL in SalesRuteItemDal** | `ListData(ISalesPersonKey)` references non-existent column — unused via interface | **Low** — latent code smell |
| **HariRuteDal.Update bug** | SET clause uses `@HariRuteId = HariRuteId` pattern — update may be broken if ever used | **Low** — no UI calls it today |

### 8.2 Data volume risks

| Dimension | Order of magnitude | Assessment |
| --------- | ------------------ | ---------- |
| Salesmen | Tens | Small |
| Customers per slot | Tens | Small |
| Slots per salesman | Up to 12 | Fixed |
| Materialized rows | salesmen × customers-per-day × days-in-window | **Manageable** for rolling horizon |
| Full history forever | Grows daily indefinitely | **Approved (Q6)** — retention policy is indefinite; volume not a practical concern |

Data volume is **unlikely to be a primary constraint** for a distributor-scale deployment.

### 8.3 Maintenance risks

| Risk | Description |
| ---- | ----------- |
| **Dual maintenance** | Template + exceptions + materialization sync — operators must understand three layers |
| **SM4 accidental edit** | Users may confuse template change with exception |
| **Orphan salesmen** | Routes incomplete — some slots empty; compliance denominators uneven |
| **Customer master changes** | Suspended/deleted customers on routes — no automatic cleanup discovered |

### 8.4 Synchronization risks

| Risk | Description |
| ---- | ----------- |
| **Template change vs materialized future** | Must regenerate or invalidate future dated rows |
| **Exception after materialization** | Effective plan must recompute |
| **Worker failure** | Gaps in dated schedule if batch materialization used |
| **No mobile sync (current scope)** | BTrade3 route sync **out of scope (Q13)** — materialization is Desktop/planning foundation first |

### 8.5 Historical consistency risks

| Risk | Description |
| ---- | ----------- |
| **Retroactive template edit** | **Mitigated (Q7)** — approved policy is future-only; architect must enforce immutability of past materialized rows |
| **Exception after visit day passed** | Backdating exceptions distorts compliance |
| **CURRENT-only snapshot mistake** | Reusing portal `BTRPD_*` pattern without dated rows loses audit trail |

---

## 9. Recommendations

### 9.1 Strategic (aligned with approved decisions)

1. **Adopt a dated visit schedule as a first-class business concept** — model as **Territory Execution Plan** (sales + collection), not sales-only (Q14).
2. **Keep `BTR_SalesRute*` as the recurring template** — do not repurpose it for dated or exception data.
3. **Treat exception visits as a separate concern** applied on top of the effective daily plan.
4. **Implement calendar expansion** using fixed anchor date + continuous 14-day cycle (Q1, Q2).

### 9.2 Materialization (approved — Q5, Q6, Q7)

1. **Hybrid generation:** regenerate future schedules on template change **and** run a scheduled worker for rolling horizon maintenance.
2. **Dated persistence with indefinite retention** — do not use `SnapshotKey = 'CURRENT'` portal pattern alone.
3. **Future dates only** when templates change — historical materialized rows are immutable.
4. **Freeze past dates** — compliance and audit depend on stable historical effective plans.

### 9.3 Exception management (approved — Q3, Q8, Q9, Q10, Q15)

1. **SM6 – Jadwal Kunjungan** under MASTR for date-scoped exceptions — do not extend SM4.
2. **Initial scope:** add, remove, replace only — no substitute salesman.
3. **Holidays and leave** via exceptions, not template edits.
4. **Supervisor level and above** may create exceptions; no approval workflow in v1.
5. **Compute effective plan** as `template_expand(date) + exceptions(date)`.

### 9.4 Operational integration (future, not current scope)

1. **FT1/FT2** may eventually default route-day selection from calendar date — coordinate with finance users.
2. **Email identity bridge** is approved and authoritative (Q12) — safe for compliance joins.
3. **BTrade3 route sync** explicitly out of current scope (Q13) — may be evaluated in a future phase.

### 9.5 What not to do

- Do not infer visit compliance from current template vs historical check-ins — results are misleading after any SM4 edit.
- Do not store exceptions by editing `BTR_SalesRuteItem` directly.
- Do not block M25 planning on dashboard UI — but do block M25 **route compliance metrics** on dated schedule + exception foundation.

---

## 10. Approved Business Decisions

The following decisions supersede the original open questions. Approved by business review.

### Q1. Anchor rule for Minggu-1 vs Minggu-2

**Decision:** Fixed anchor date + continuous 14-day repeating cycle.

The system determines Minggu-1 and Minggu-2 from a configured anchor date. The cycle represents a recurring operational territory pattern and does not depend on calendar month boundaries.

### Q2. Cycle reset on calendar boundaries

**Decision:** No. The cycle runs continuously and never resets on month, quarter, or year boundaries.

### Q3. Public holidays and salesman leave

**Decision:** Handled through **Exception Visit management** (SM6). Do not modify the recurring route template for temporary disruptions.

### Q4. Customer on multiple route slots

**Decision:** Yes — intentional and supported. Some customers require multiple visits within a route cycle.

### Q5. Materialized schedule generation

**Decision:** Hybrid approach:

1. Regenerate **future** schedules when route templates are modified (SM4).
2. Run a **scheduled worker** to maintain the rolling planning horizon.

### Q6. Retention period for historical effective plans

**Decision:** Retain **indefinitely**. Required for compliance analysis, operational audits, and management reviews. Storage volume is not a practical concern.

### Q7. Template change effective dates

**Decision:** **Future dates only.** Historical materialized schedules must remain unchanged and auditable.

### Q8. Exception authorization

**Decision:** **Supervisor level and above.** Salesmen must not directly modify planned schedules.

### Q9. Approval workflow for exceptions

**Decision:** **No** approval workflow in initial implementation. Auditability may be added later.

### Q10. Substitute-salesman coverage

**Decision:** **Out of initial scope.** Scope limited to: Add Customer, Remove Customer, Replace Customer.

### Q11. Sunday visit planning

**Decision:** **Not required.** Route planning operates Monday through Saturday only.

### Q12. SalesPerson.Email reliability

**Decision:** **Reliable and authoritative.**

- `SalesPerson.Email` is **mandatory** for BTrade3 mobile login.
- Email is the **official identity bridge** between SalesPerson, Check-In, and Sales Order.
- Future route compliance analysis may safely use Email for joins.

### Q13. BTrade3 effective plan sync

**Decision:** **Not required** for current scope. Mobile route guidance may be evaluated in a future phase.

### Q14. Collection routes vs sales routes

**Decision:** **Same template.** BTR has no dedicated collection personnel — salesmen perform both sales visits and collection on the same route structure.

**Territory Execution Plan:** The route template is a unified plan aligned with the default **14-day receivable cycle**:

```text
Customer Visit → Sales Order → Invoice → 14-Day Due Date → Same Route Cycle Revisit → Collection Opportunity
```

Materialized schedules shall be modeled as **operational visit plans**, not sales-only plans.

### Q15. Exception form menu placement

**Decision:** **Master Menu — SM6 – Jadwal Kunjungan.** Exception management is an extension of route planning beside SM4-Rute.

---

## 11. Guidance for Architect

Preserve these business rules in all design decisions:

1. **Territory Execution Plan** — route materialization serves sales **and** collection on the same visit plan (Q14).
2. **14-day cycle alignment** — the two-week route cycle matches the default receivable cycle; do not treat routes as sales-only artifacts.
3. **Operational visit plans** — materialized rows represent what the salesman is expected to execute that day (visit, order opportunity, collection opportunity).
4. **Future consumers** may include: visit planning, collection planning, check-in validation, effective call analysis, route compliance analysis, territory coverage analysis.
5. **Email identity bridge** is a supported business model — design joins accordingly (Q12).
6. **Immutability of history** — enforce future-only template propagation (Q6, Q7).

**Remaining implementation inputs (not open business questions):**

- Specific **anchor date** configuration value and where it is stored/administered.
- **Rolling horizon** window size (e.g. days ahead) for the scheduled worker.
- **Role/permission** mapping for "supervisor level and above" in BTR Desktop security.

---

## Appendix A — Entity relationship (conceptual)

```text
[Configured anchor date] → continuous 14-day cycle → Minggu-1 / Minggu-2

BTR_HariRute (12 slots: week-in-cycle × weekday, Mon–Sat)
        │
        ▼
BTR_SalesRute (SalesPersonId + HariRuteId) ──► BTR_SalesRuteItem (CustomerId, NoUrut)
        │                                         Territory Execution Plan
        │                                         (sales + collection, same template)
        │
        │  [TO BE BUILT — approved Q5, Q6, Q7]
        ▼
   Materialized daily visit plan (SalesPersonId, VisitDate, CustomerId, Source)
        ▲                                          retained indefinitely
        │  exception overlay (SM6 – Jadwal Kunjungan)
        │  [TO BE BUILT — approved Q3, Q8, Q10, Q15]
        │
   Exception visit (VisitDate, operation: add|remove|replace)

Identity bridge (approved Q12):
   BTR_SalesPerson.Email  ↔  BTR_CheckIn.UserEmail  ↔  BTR_Order.UserEmail

BTR_CheckIn (CheckInDate, CustomerId, UserEmail)     — actual visit
BTR_Order   (OrderDate, CustomerId, UserEmail)       — sales order
```

---

## Appendix B — Code references

| Artifact | Path |
| -------- | ---- |
| HariRute table | `src/j05-btr-distrib/btr.sql/Tables/SalesContext/BTR_HariRute.sql` |
| HariRute seed | `src/j05-btr-distrib/btr.sql/Tables/SalesContext/BTR_HariRuteDataSeed.sql` |
| SalesRute table | `src/j05-btr-distrib/btr.sql/Tables/SalesContext/BTR_SalesRute.sql` |
| SalesRuteItem table | `src/j05-btr-distrib/btr.sql/Tables/SalesContext/BTR_SalesRuteItem.sql` |
| CheckIn table | `src/j05-btr-distrib/btr.sql/Tables/SalesContext/BTR_CheckIn.sql` |
| SM4 form | `src/j05-btr-distrib/btr.distrib/SalesContext/SalesRuteAgg/SalesRuteForm.cs` |
| Route writer (delete+insert) | `src/j05-btr-distrib/btr.application/SalesContext/SalesRuteAgg/SalesRuteWriter.cs` |
| FT2 route usage | `src/j05-btr-distrib/btr.distrib/FinanceContext/TagihanAgg/TagihanForm.cs` |
| FT1 route usage | `src/j05-btr-distrib/btr.distrib/FinanceContext/LunasPiutangAgg/LunasPiutang2Form.cs` |
| Effective call join | `src/j05-btr-distrib/btr.infrastructure/SalesContext/CheckInFeature/EffectiveCallDal.cs` |
| Menu seed SM4 | `src/j05-btr-distrib/btr.sql/DataSeeds/BTR_Menu.dataseed.sql` |
| Materialization reference | `docs/features/materialized-dashboard/materialized-dashboard-architecture.md` |
| M18 route exclusion | `docs/work/btr-portal/M18 Salesman Performance - Analysis.md` |

---

*Document produced by Analyst discovery. Open questions Q1–Q15 resolved — ready for Architect handoff.*
