# IMPLEMENTATION PLAN

## Principal-Centric Analytics Migration

| Field | Value |
| --- | --- |
| Planning authority | FEASIBILITY ASSESSMENT |
| Authority artifact | `docs/work/btr-portal/principal-centric-analytics-migration/FEASIBILITY-ASSESSMENT.md` |
| Plan date | 2026-09-09 |
| Status | PLANNED |
| Implementation mode | Feasibility-driven (Mode B) |

This plan realizes the approved recommendation: **Option C, the dual-axis analytical model**, with GO / major redesign.

It does not introduce new business decisions. Items the feasibility assessment delegated to implementation planning are resolved in [Planning decisions](#planning-decisions) and are constrained by the approved gaps.

---

## Planning Authority

```text
FEASIBILITY ASSESSMENT
```

Authoritative inputs used:

- Approved operating model and GAP-001 through GAP-023
- Resolved questions BQ-001 through BQ-010, TQ-006, TQ-007, TQ-009, TQ-010, OQ-001 through OQ-004
- Section 4 impact inventory, including dashboard disposition
- Section 7 recommended approach and mandatory constraints
- Section 11 planner guidance

Implementation-validation questions TQ-001 through TQ-005 and TQ-008 are scheduled as PCM-003. They do not change the approved model and do not block later slices.

---

## Scope Summary

Migrate BTR Portal commercial analytics so Principal is the primary commercial performance dimension, while:

- Customer remains an independent account and credit axis
- Salesman commercial attribution uses the portfolio owner on the transaction (`Faktur.SalesPersonId`)
- Salesman field activity remains performer-attributed and is not removed
- Customer–Principal is a transaction-derived analytical relationship, not master data
- Principal financial, collection, and credit attribution remains out of scope

Approved outcome:

- Principal sales performance, target, growth, reach, mix, coverage, and returns are directly answerable
- Salesman analytics remain available for execution and coaching, without Customer-ownership language
- Customer analytics can show which Principals a Customer buys, has stopped buying, or buys relative to other purchased Principals
- Navigation distinguishes commercial portfolio, customer account, and sales execution
- Every new Principal KPI is traceable to Invoice Item evidence

---

## Planning Decisions

These resolve items the feasibility assessment assigned to implementation planning. They do not change approved business rules.

### PD-001 — Navigation placement and code

Adopt the feasibility section 4.4 recommended direction.

- Add Principal commercial performance under the existing Sales group.
- Do not create a Principals domain group.
- Do not replace Sales Force.
- Do not use Entity Analytics or Purchasing as the only Principal commercial entry.
- Canonical navigation registry for this initiative is the implemented menu registry, then `docs/features/btr-portal/navigation-assets.md` after PCM-001 consolidates it.
- New code: `SA04` — Principal Performance.
- Route: `/dashboard/principal-performance`.
- Sales group order after PCM-001: SA01, SA04, SA02, SA03.
- Do not assign Principal commercial performance to `EX03`, `SF03`, or `SF04`.
- Current implemented uses of those codes remain: `EX03` Entity Analytics, `SF02` Sales Force Overview, `SF03` Salesman Field Activity.
- Older reservations that conflict with implemented codes are superseded by the implemented registry. Do not reassign implemented codes.

### PD-002 — Principal Sales-Out (DPP) formula

Standard formula for every Principal sales KPI, dashboard, report, ranking, forecast input, and Entity Analytics commercial measure:

```text
Principal Sales-Out (DPP)
  = SUM(FakturItem.SubTotal - FakturItem.DiscRp)
```

Rules:

- Include only non-void Fakturs (`Faktur.VoidDate = '3000-01-01'`, matching existing Principal omzet evidence).
- Attribute each line through `FakturItem.BrgId → BTR_Brg.SupplierId`.
- Do not use `FakturItem.Total`. That amount includes tax (`SubTotal - DiscRp + PpnRp`).
- Do not use `FakturItem.DppRp` as the performance measure. Stored `DppRp` applies `DppProsen` and is a tax-base amount, not the approved pre-tax commercial amount.
- Do not use `Faktur.GrandTotal`.
- Do not allocate header tax, freight, rounding, or other header adjustments to Principals.
- Do not subtract returns, claims, rebates, or other post-sale adjustments.
- Blank or unknown `SupplierId` is excluded from Principal totals and written to the data-quality output. Do not create a synthetic Principal.
- Company sales totals on existing company surfaces remain header `GrandTotal`. They are not required to equal the sum of Principal Sales-Out (DPP).

Canonical KPI ID: `PR-KPI-001`.

Existing `BTRPD_SalesmanPrincipalAchievement.CompletedOmzet` remains the existing invoice-line `Total` execution measure. Do not overwrite it with Sales-Out (DPP). New Principal commercial KPIs use `PR-KPI-001`.

### PD-003 — Returns formulas

Returns are independent KPIs. They do not reduce `PR-KPI-001`.

Source: non-void `BTR_ReturJual` / `BTR_ReturJualItem`, Principal via `ReturJualItem.BrgId → BTR_Brg.SupplierId`.

```text
Line return amount = ReturJualItem.SubTotal - ReturJualItem.DiscRp
Good Return Amount   = sum of line return amount where JenisRetur = BAGUS
Broken Return Amount = sum of line return amount where JenisRetur = RUSAK
Total Return Amount  = Good Return Amount + Broken Return Amount
Return Rate (%)      = Total Return Amount / Principal Sales-Out (DPP)
                       only when Principal Sales-Out (DPP) > 0; otherwise null
```

- Exclude `PpnRp`.
- `JenisRetur` values are the existing operational values `BAGUS` and `RUSAK` only.
- A return does not establish or refresh a Customer–Principal relationship. Relationship existence and Active/Dormant status use non-void Faktur item sales only.
- Salesman on the return document is not used to reassign Faktur revenue, target, or bonus.

### PD-004 — Target, achievement, and coverage

- Principal Target for a Principal and month = `SUM(BTR_SalesPersonPrincipalTarget.TargetAmount)` for that `SupplierId`, `TargetYear`, and `TargetMonth`.
- No standalone Principal Target row is created.
- A Salesman is responsible for a Principal in a month only when a target record exists for that Salesman, Principal, year, and month.
- `BTR_SalesPersonSupplier` is current eligibility reference only. It must not override target-based historical responsibility.
- Principal Achievement Amount = Principal Sales-Out (DPP) for that month.
- Principal Achievement % = Principal Sales-Out (DPP) / Principal Target when Principal Target > 0; otherwise null.
- Salesman coverage count = distinct Salesmen with a target record for that Principal and month.
- Faktur Salesman × item Principal pairs with no target record for the transaction month remain in Principal Sales-Out (DPP) and are written as responsibility exceptions. Do not drop the sale from Principal totals.

### PD-005 — Persistence and composition

- Persist Principal commercial measures in ReportingContext snapshot tables owned by the Sales aggregation path.
- Do not persist `SupplierId` on `BTR_FakturItem`.
- Do not add a Customer–Principal master table.
- Do not add effective-dated Salesman–Principal assignment tables.
- Purchasing Management in-memory `SalesOutAmount` is not the authoritative Principal sales measure.
- Entity Analytics remains a composition and presentation layer. No producer-model extension.
- `SupplierEntityAnalyticsProducer` remains the single writer of the Supplier/Principal Entity Analytics profile.
- Sales aggregation remains the calculation owner of Principal sales-out, returns, and target snapshots.
- Purchasing remains the calculation owner of purchase-in KPIs.
- The Principal Entity Analytics producer reads those owned snapshots. It must not recompute Principal Sales-Out from purchase data, and it must not replace the profile with purchase-only metrics.
- Refresh order: Sales Principal snapshots complete before the Principal Entity Analytics refresh that consumes them.
- Portal consumers see one Principal profile. Purchase-in and sales-out are separate KPI packs inside that profile.

### PD-006 — Customer–Principal relationship

- Not a new Entity Analytics entity type.
- Not master data and not manually assignable.
- A pair exists when at least one non-void Faktur item sale attributes the Customer to the Principal.
- History is retained indefinitely.
- Active = last qualifying sale date is within the previous 6 months of the snapshot as-of date.
- Dormant = at least one historical qualifying sale, and last qualifying sale date is outside the previous 6 months.
- Apply the same status rule to Customer Coverage, Customer Lifecycle, Relationship Analytics, and Entity Analytics relationship presentation.
- Full pair population is required where portfolio, lifecycle, decline, or dormancy questions are answered. Top-N MTD links are not sufficient for those questions.
- Cross-Principal comparison is observational among Principals already purchased. Do not invent pre-purchase eligibility or assignment.

### PD-007 — Surface disposition

Use feasibility section 4.1. This plan does not retire Salesman execution surfaces.

| Surface | Disposition in this plan |
| --- | --- |
| SA01 | Keep company header totals and Salesman contribution. Add Principal Sales-Out, target, and contribution decomposition. |
| SA02 | Keep company forecast. Add Principal forecast and pace from Principal Sales-Out history. Principal forecast is not required to equal company forecast. |
| SA03 | Add Principal-compatible Invoice Item evidence. Keep Faktur-header company evidence distinct. |
| SA04 | New primary Principal commercial performance path. |
| CU01–CU05 | Remove exclusive Salesman-ownership implication. Add Customer × Principal sales/portfolio context only after pair history exists. |
| FI01 | No Principal financial decomposition. |
| FI02, FI04 | Relabel Salesman as invoice-attributed. No Principal overdue or collection KPIs. |
| FI03 | No Principal collection forecast. |
| SF01 | Reposition wording. Keep coaching, allocation, and existing execution achievement. |
| SF02, SF03 | Preserve unchanged. Do not add Principal as primary grain or as a filter. |
| EX01, EX02 | Add Principal sales attention and alerts after Principal evidence exists. No Principal collection or credit alerts. |
| EX03 | Separate sales-out from purchase-in. Correct singular Assigned Salesman. Pair analysis is a relationship projection, not a new entity type. |
| PU01, PU02 | Preserve as purchase-in. Do not present as Principal sales performance. |
| IN01–IN05 | Preserve inventory metrics. Cross-link to SA04 where a Principal identity already exists. Do not add new inventory forecast or optimization views in this initiative. |
| OP01 | No Principal migration. Do not redesign KPI entity classification. |

### PD-008 — KPI identity and classification

- Add new KPI IDs. Do not mutate existing KPI formulas, names, or IDs.
- Do not redesign the four-entity classification framework.
- Classify new Principal commercial KPIs under the existing Supplier category where a category is required, and record grain metadata as Principal and Invoice Item.
- Canonical commercial measure referenced by Principal dashboards, rankings, relationships, and evidence is `PR-KPI-001`.
- Purchase KPIs, including `PU-KPI-001`, remain purchase-in.
- Salesman ranking KPI `SF-KPI-008` remains a Salesman measure. Principal omzet relationships must not reference it.

Reserved IDs:

| ID | Name | Grain |
| --- | --- | --- |
| PR-KPI-001 | Principal Sales-Out (DPP) | Principal; evidence Invoice Item |
| PR-KPI-002 | Principal Target | Principal × month; sum of Salesman allocations |
| PR-KPI-003 | Principal Achievement Amount | Principal × month; equals PR-KPI-001 for that month |
| PR-KPI-004 | Principal Achievement % | Principal × month |
| PR-KPI-005 | Principal Sales-Out Growth (month over prior month) | Principal × month |
| PR-KPI-006 | Principal Active Customer Count | Principal; Customers with qualifying sale in previous 6 months |
| PR-KPI-007 | Principal Sales Concentration | Principal; share of top Customer Sales-Out within that Principal |
| PR-KPI-008 | Principal Salesman Coverage | Principal × month; target-record count |
| PR-KPI-009 | Principal Forecast | Principal; existing forecast method on PR-KPI-001 history |
| PR-KPI-010 | Principal Required Pace | Principal |
| PR-KPI-011 | Principal Target Gap | Principal; Target minus Sales-Out (DPP) |
| PR-KPI-012 | Good Return Amount | Principal |
| PR-KPI-013 | Broken Return Amount | Principal |
| PR-KPI-014 | Total Return Amount | Principal |
| PR-KPI-015 | Return Rate (%) | Principal; quality indicator only |
| CP-KPI-001 | Customer–Principal Sales-Out (DPP) | Customer × Principal |
| CP-KPI-002 | Customer–Principal First Purchase Date | Customer × Principal |
| CP-KPI-003 | Customer–Principal Last Purchase Date | Customer × Principal |
| CP-KPI-004 | Customer–Principal Relationship Status | Customer × Principal; Active or Dormant |
| CP-KPI-005 | Customer–Principal Share of Customer Sales-Out | Customer × Principal |

### PD-009 — Authorization and terminology

- No Principal-scoped authorization, row filter, or menu restriction.
- Users who can access commercial analytics may view all Principals, matching current portal access.
- User-facing labels use Principal.
- Technical identifiers remain `Supplier` / `SupplierId`.
- No equivalence table or database rename.

### PD-010 — Disclosure and data quality

Every Principal sales surface and KPI definition must state:

- measure is Principal Sales-Out (DPP) from Invoice Item lines
- tax and header totals are excluded
- totals are not required to reconcile to Faktur `GrandTotal`
- Item Principal comes from current Item master and is treated as immutable for analytics
- historical periods use the best available data and may contain documented limitations
- unknown Principal and missing monthly target responsibility are visible exceptions, not silent drops of Principal Sales-Out

---

## Out of Scope

- Principal piutang, overdue, aging, DSO, recovery, cash collection, credit exposure, or open-balance allocation
- Allocation of Faktur header totals or adjustments to Principals
- A required reconciliation of Principal Sales-Out to `GrandTotal`
- Invoice-time Principal snapshot or `SupplierId` on `BTR_FakturItem`
- Customer–Principal master assignment or pre-purchase linking
- Effective-dated Salesman–Principal assignment
- Standalone Principal Target authority
- Claims, rebates, bonuses posting, or margin KPIs
- Principal-scoped authorization or user-to-Salesman data scope
- KPI entity-classification framework redesign
- Replacement or removal of SF02 or SF03
- Principal filters on field-activity surfaces
- Coverage or substitute-execution reporting that reassigns revenue, target, or bonus
- BTrade, mobile, or sync changes
- Purchase growth relabeled as sales growth
- A new Entity Analytics entity type for Customer–Principal
- A new producer-model or multi-writer snapshot-merge framework
- Changing `BTRPD_SalesmanPrincipalAchievement.CompletedOmzet` from its existing Total-based measure

---

## Impact Inventory

### Backend

- Sales aggregation and `FakturPrincipalOmzetDal` evidence path: add Sales-Out (DPP), returns, unknown-Principal, and monthly history outputs
- `DashboardSalesmanAggregator` and Salesman snapshot semantics: preserve execution achievement; stop implying Customer ownership
- Customer, Customer Risk Forecast, and Customer Portfolio aggregators: remove singular ownership inference; consume pair relationship status only where scheduled
- Sales forecast aggregator: add Principal forecast from Principal Sales-Out history without replacing company forecast
- Purchasing Management aggregator: remain purchase-in calculation owner; stop being the source of authoritative Principal Sales-Out
- `SupplierEntityAnalyticsProducer`: compose sales-owned snapshots with existing purchase-in and inventory inputs under the current single-writer refresh
- Supplier and Customer relationship catalogs, registrars, and evidence resolvers: new Principal sales KPI references; correct `AssignedSalesman` meaning; realign omzet metadata from `PU-KPI-001` and `SF-KPI-008` to `PR-KPI-001`
- Executive and Alert composers: Principal sales attention only, with deduplication against existing sales alerts
- Investigation and report query contracts: Principal and Customer–Principal filters at Invoice Item grain
- Piutang and collection query contracts: label correction only

### Database

- New ReportingContext snapshots for Principal current KPI, Principal month history, Principal × Salesman contribution, Principal data-quality exceptions, and Customer × Principal relationship
- Preserve `BTRPD_Salesman*` and `BTRPD_SalesmanPrincipalAchievement`
- No change to `BTR_FakturItem`, `BTR_Customer`, `BTR_SalesPersonSupplier`, or `BTR_SalesPersonPrincipalTarget` structure
- No Principal financial columns
- Entity Analytics L0–L5 tables store composed Principal KPIs; do not add a Customer–Principal entity type

### Frontend

- `portalMenuCodes.ts`, `portalMenuRegistry.ts`, and Sales routes: SA04
- New Principal Performance dashboard
- SA01, SA02, and SA03 Principal evidence and decomposition
- CU01–CU05 ownership-label correction and later pair context
- SF01 wording correction; SF02 and SF03 unchanged
- FI02 and FI04 invoice-attributed labels
- Entity Analytics Principal profile: sales-out primary, purchase-in separate
- EX01 and EX02 Principal sales attention
- PU and IN labels and cross-links only

### Integration

- No BTrade, mobile, or sync change
- Worker refresh order: Sales Principal snapshots before Entity Analytics Principal composition
- Freshness shown from the existing refresh contract; no new orchestration platform

### Security

- No authorization model change
- No Principal row-level scope

---

## Phases

### Phase 1 — Contracts and validation

Establish navigation identity, versioned KPI definitions, and the production profiling baseline.

### Phase 2 — Principal sales evidence

Persist the authoritative Principal Sales-Out, returns, target, contribution, and history used by every later surface.

### Phase 3 — Principal commercial surfaces

Deliver the primary Principal path and add Principal decomposition to existing Sales surfaces without replacing company or Salesman execution views.

### Phase 4 — Ownership semantic correction

Correct Salesman, finance, and Customer labels that imply exclusive Customer ownership. Preserve field activity and Customer financial truth.

### Phase 5 — Customer × Principal reorientation

Build pair history first, then reorient Customer lifecycle and portfolio claims that require it.

### Phase 6 — Entity Analytics and executive promotion

Compose one Principal profile, realign relationship metadata, and only then promote Principal sales attention to executive surfaces.

### Phase 7 — Permanent knowledge

Synchronize permanent artifacts after the implemented behavior is fixed by earlier slices.

---

## Slices

### PCM-001

#### Objective

Consolidate the navigation code registry and reserve SA04 as the primary Principal commercial performance path under Sales.

#### Dependencies

- None

#### Deliverables

- `src/j05-btr-distrib/btr.portal.web/src/navigation/portalMenuCodes.ts`
- `src/j05-btr-distrib/btr.portal.web/src/navigation/portalMenuRegistry.ts`
- Route registration for `/dashboard/principal-performance`
- Navigation asset registry update that records implemented codes and the superseded older reservations
- A reachable SA04 placeholder page is acceptable in this slice; full analytics arrive in PCM-007

#### Acceptance Criteria

- `SA04` exists in the menu code registry and Sales group, labeled Principal Performance, ordered after SA01 and before SA02.
- SA04 is not assigned `EX03`, `SF03`, or `SF04`.
- Existing routes and labels for SA01, SA02, SA03, SF02, SF03, EX03, PU01, and PU02 still resolve.
- The navigation asset registry names one authoritative code list and records that older conflicting reservations of `EX03`, `SF03`, and `SF04` are not used for new assignment.
- No Principal-scoped menu visibility is added.

#### Review Focus

- Workflow Compliance
- UI State Compliance
- Architecture Compliance

---

### PCM-002

#### Objective

Register the versioned Principal and Customer–Principal KPI definitions required by PD-002, PD-003, PD-004, and PD-008 without changing existing KPI semantics.

#### Dependencies

- None

#### Deliverables

- KPI catalog entries for `PR-KPI-001` through `PR-KPI-015` and `CP-KPI-001` through `CP-KPI-005`
- Grain, attribution, inclusion, exclusion, and historical-limitation text on each new definition
- Explicit catalog statement that Principal financial and collection KPIs are absent

#### Acceptance Criteria

- Each reserved ID has one definition matching the formulas in PD-002, PD-003, and PD-004.
- `PR-KPI-001` is identified as the canonical Principal commercial measure.
- No existing KPI ID, name, or formula is changed, including `PU-KPI-001` and `SF-KPI-008`.
- New definitions state that returns do not reduce Sales-Out (DPP), tax is excluded, and reconciliation to `GrandTotal` is not required.
- New definitions state Item-master attribution and the immutability assumption.
- No Principal piutang, overdue, aging, DSO, collection, or credit KPI ID is added.
- KPI entity-classification framework structure is unchanged.

#### Review Focus

- Architecture Compliance
- Workflow Compliance
- Persistence Compliance

---

### PCM-003

#### Objective

Execute the GAP-007 production profiling checks and record the baseline. Findings do not alter the approved model.

#### Dependencies

- None

#### Deliverables

- `docs/work/btr-portal/principal-centric-analytics-migration/PROFILING-BASELINE.md`
- Measured results for TQ-001 through TQ-005 and TQ-008
- The nine section 9 validation checks

#### Acceptance Criteria

- The baseline report contains a measured result or an explicit data-unavailable result for each required check:
  - distinct Salesmen per Customer by month and trailing 12 months
  - distinct Principals per Customer by month and trailing 12 months
  - distinct Principals per Faktur and mixed-Principal distribution
  - Faktur Salesman × item Principal pairs lacking a matching `BTR_SalesPersonPrincipalTarget` for the transaction month
  - line-total versus Faktur `GrandTotal` difference distribution
  - unknown or blank Supplier rate on sold items
  - target coverage by Salesman, Principal, and company
  - Customer–Principal population, activity, and 6-month retention count
  - evidence of Item `SupplierId` change or a statement that no historical source exists
- The report states that findings do not change GAP-001 through GAP-023.
- No application behavior, schema, or KPI definition is changed by this slice.

#### Review Focus

- Persistence Compliance
- Architecture Compliance

---

### PCM-004

#### Objective

Create the Sales-owned current Principal Sales-Out (DPP), returns, and data-quality evidence used as the authoritative commercial measure.

#### Dependencies

- PCM-002

#### Deliverables

- Sales evidence query at Invoice Item grain implementing PD-002 and PD-003
- Current ReportingContext snapshots for Principal Sales-Out, returns, and data-quality exceptions
- Preservation of existing `FakturPrincipalOmzetDal` Total-based output used by `BTRPD_SalesmanPrincipalAchievement`

#### Acceptance Criteria

- A current Principal snapshot row stores `PR-KPI-001`, `PR-KPI-012`, `PR-KPI-013`, `PR-KPI-014`, and `PR-KPI-015` using the approved formulas.
- Sales-Out equals `SUM(FakturItem.SubTotal - FakturItem.DiscRp)` and does not include `PpnRp`, `FakturItem.Total`, `FakturItem.DppRp`, or `Faktur.GrandTotal`.
- Void Fakturs and void returns are excluded using the existing void sentinel.
- Good and Broken returns follow `JenisRetur` `BAGUS` and `RUSAK`.
- Return Rate is null when Sales-Out (DPP) is not greater than zero.
- Lines with blank or unknown `SupplierId` are absent from Principal rows and present in the data-quality snapshot with amount and count.
- Existing Salesman × Principal achievement omzet is still populated from line `Total` and is not replaced by Sales-Out (DPP).
- No `SupplierId` column is added to `BTR_FakturItem`.
- Automated tests cover mixed-Principal Fakturs, tax exclusion, unknown Principal exclusion, and returns not reducing Sales-Out.

#### Review Focus

- Persistence Compliance
- Architecture Compliance
- Workflow Compliance

---

### PCM-005

#### Objective

Persist Principal Sales-Out and returns monthly history from Invoice Item evidence so trend, growth, and forecast do not depend on in-memory Purchasing values.

#### Dependencies

- PCM-004

#### Deliverables

- Principal × year × month history snapshot
- Backfill from available Faktur and return history using Item-master attribution
- Documented historical limitation on the history output

#### Acceptance Criteria

- Each history row identifies Principal, year, and month and stores Sales-Out (DPP) and independent return amounts using PD-002 and PD-003.
- History does not require invoice-time `SupplierId` on `BTR_FakturItem`.
- History is not read from Purchasing Management `SalesOutAmount`.
- Current-month history equals the current PCM-004 Principal Sales-Out for the same Principal and month.
- The history output records that Item Principal is current Item master and historical reconstruction may be limited.
- Unknown-Principal amounts remain in the data-quality output and are not assigned to a Principal history row.

#### Review Focus

- Persistence Compliance
- Architecture Compliance

---

### PCM-006

#### Objective

Aggregate Principal Target from Salesman allocations and store Principal × Salesman commercial contribution without treating target absence as a reason to drop sales.

#### Dependencies

- PCM-002
- PCM-004

#### Deliverables

- Principal Target, Achievement Amount, Achievement %, and Salesman coverage on the Principal snapshot
- Principal × Salesman contribution snapshot for the current period
- Responsibility exception output for sold Salesman × Principal pairs with no target record in the transaction month

#### Acceptance Criteria

- Principal Target equals the sum of `BTR_SalesPersonPrincipalTarget.TargetAmount` for that Principal and month.
- No independent Principal Target record is written.
- Achievement % is null when Principal Target is not greater than zero.
- Salesman coverage equals the count of target records for that Principal and month, including zero-amount targets.
- A Faktur line whose Salesman × Principal has no target record still contributes to Principal Sales-Out and is listed as a responsibility exception.
- `BTR_SalesPersonSupplier` is not used as the historical responsibility source.
- One Principal can show more than one contributing Salesman.
- Contribution uses `Faktur.SalesPersonId` as commercial owner and Sales-Out (DPP), not field-activity performer.

#### Review Focus

- Persistence Compliance
- Workflow Compliance
- Architecture Compliance

---

### PCM-007

#### Objective

Deliver SA04 Principal Performance as the primary commercial path for Principal sales, target, contribution, returns, and attention.

#### Dependencies

- PCM-001
- PCM-005
- PCM-006

#### Deliverables

- SA04 dashboard showing Principal Sales-Out (DPP), target, achievement, growth, active Customer count, Salesman coverage, concentration, returns, and target gap
- Principal ranking and Salesman contribution within a selected Principal
- Drill-down identifier that can open Invoice Item evidence
- Data-quality disclosure and exception counts
- Attention signals for below-target sales and return-rate quality only

#### Acceptance Criteria

- SA04 is reachable from the Sales menu and does not replace SA01, SA02, SA03, SF01, SF02, or SF03.
- Every sales figure on SA04 uses `PR-KPI-001` or a KPI derived from it as specified in PD-008.
- User-facing text uses Principal, not Supplier, except where a technical identifier is displayed.
- Purchase amount and purchase growth are absent from SA04 performance KPIs.
- No piutang, overdue, collection, or credit metric is shown.
- Returns are shown as separate amounts and do not change the displayed Sales-Out amount.
- A Principal with multiple Salesmen shows each contributing Salesman and does not assign Customer ownership to one Salesman.
- The page states the line-item measure, tax exclusion, `GrandTotal` non-reconciliation, and Item-master historical limitation.
- Unknown Principal and missing-target exceptions are visible and are not hidden by omitting their sales from Principal Sales-Out.
- SA04 does not query Purchasing Management in-memory `SalesOutAmount` for its sales figures.

#### Review Focus

- UI State Compliance
- Workflow Compliance
- Architecture Compliance

---

### PCM-008

#### Objective

Add Principal commercial decomposition to SA01 while preserving company header totals and Salesman contribution.

#### Dependencies

- PCM-001
- PCM-006

#### Deliverables

- SA01 Principal contribution, target, and achievement view
- Navigation from that view to SA04
- Unchanged company header total source

#### Acceptance Criteria

- SA01 company sales totals still use the existing header `GrandTotal` measure.
- Top Salesman remains available as contribution and coaching, not as the only commercial decomposition.
- SA01 shows Principal Sales-Out (DPP) and Principal Target from PCM-006.
- SA01 does not relabel purchase metrics as Principal sales.
- SA01 does not show Principal collection or credit metrics.
- A link or route action opens SA04 for a selected Principal.
- Existing SA01 Salesman contribution behavior remains available.

#### Review Focus

- UI State Compliance
- Workflow Compliance
- Architecture Compliance

---

### PCM-009

#### Objective

Make SA03 able to evidence Principal sales at Invoice Item grain without allocating Faktur header totals.

#### Dependencies

- PCM-002
- PCM-004

#### Deliverables

- Principal filter and Principal amount column based on line Sales-Out (DPP)
- Invoice Item drill-down for a Principal and period
- Disclosure that header `GrandTotal` is not the Principal measure

#### Acceptance Criteria

- Filtering or decomposing the sales report by Principal uses `FakturItem` amounts and `Brg.SupplierId`.
- A mixed-Principal Faktur contributes each line to its own Principal and does not split `GrandTotal`.
- The report identifies the measure as Principal Sales-Out (DPP) and states that it is not required to equal Faktur `GrandTotal`.
- Company report totals that remain header-based are labeled as header totals, not Principal sales.
- Evidence rows retain Faktur Item identity sufficient to trace a Principal total back to lines.
- No Principal open-balance or payment column is added.

#### Review Focus

- Persistence Compliance
- UI State Compliance
- Workflow Compliance

---

### PCM-010

#### Objective

Add Principal forecast and required pace to SA02 by applying the existing sales forecast method to Principal Sales-Out history.

#### Dependencies

- PCM-005
- PCM-006

#### Deliverables

- Principal forecast, required pace, and target gap on SA02
- Disclosure that Principal forecast is not required to equal company forecast
- Unchanged company forecast

#### Acceptance Criteria

- SA02 company forecast figures remain present and use their existing company measure.
- Principal forecast inputs are Principal monthly Sales-Out (DPP) history from PCM-005, not purchase history and not `GrandTotal`.
- Principal required pace and target gap use Principal Target from PCM-006 and Principal Sales-Out (DPP).
- The forecast method is the existing SA02 method applied to the Principal series. No new forecast algorithm is introduced.
- The page states that the sum of Principal forecasts is not required to equal the company forecast.
- No Principal collection forecast is added.

#### Review Focus

- Architecture Compliance
- UI State Compliance
- Workflow Compliance

---

### PCM-011

#### Objective

Remove Customer-ownership wording from Salesman commercial and finance invoice-attribution surfaces without changing field-activity or financial measures.

#### Dependencies

- PCM-002

#### Deliverables

- SF01 label and copy correction for owned-book, assigned-book, and singular Customer-owner language
- FI02 and FI04 labels that describe Salesman as invoice-attributed
- Confirmation that SF02 and SF03 behavior is unchanged

#### Acceptance Criteria

- SF01 no longer describes a Customer portfolio as owned by the Salesman.
- SF01 still shows coaching, target allocation, invoiced contribution, and assigned Principal mix.
- SF01 does not change `BTRPD_SalesmanPrincipalAchievement.CompletedOmzet` to Sales-Out (DPP).
- FI02 Top Overdue Salesmen and FI04 Salesman columns are labeled as invoice-attributed, not account owner.
- FI01 and FI03 behavior and measures are unchanged.
- No Principal overdue, collection, or credit KPI is added to FI01–FI04.
- SF02 and SF03 routes, grains, and performer attribution are unchanged.
- No Principal filter is added to SF02 or SF03.

#### Review Focus

- UI State Compliance
- Workflow Compliance
- Security Compliance

---

### PCM-012

#### Objective

Correct Customer pages and Customer Entity Analytics so a single displayed Salesman is a recency or transaction attribution indicator, not exclusive Customer ownership.

#### Dependencies

- PCM-002

#### Deliverables

- Customer Entity Analytics relationship relabeled from Assigned Salesman to last-invoicing Salesman
- CU01–CU05 labels, filters, and columns that currently imply one Salesman owner are corrected
- Singular latest-Faktur Salesman remains available only as a recency indicator

#### Acceptance Criteria

- No Customer page or Customer Entity Analytics relationship uses the label Assigned Salesman or Owner for the latest-Faktur Salesman.
- The latest-Faktur Salesman display states that it is the last invoicing Salesman, not the Customer owner.
- Customer credit, piutang, lifecycle, and collection measures remain Customer-level.
- This slice does not add Customer × Principal decline, dormancy, or portfolio-gap claims.
- No Customer–Principal master maintenance screen or assignment action is added.
- Multiple Salesmen are not collapsed into one account-owner field.

#### Review Focus

- UI State Compliance
- Workflow Compliance
- Architecture Compliance

---

### PCM-013

#### Objective

Persist the full transaction-derived Customer–Principal relationship population and its Active or Dormant status.

#### Dependencies

- PCM-002
- PCM-004

#### Deliverables

- Customer × Principal relationship snapshot covering all pairs with at least one historical qualifying sale
- First purchase date, last purchase date, Sales-Out (DPP), and relationship status
- No new Entity Analytics entity type and no master assignment table

#### Acceptance Criteria

- A pair is created only from non-void Faktur item sales attributed by Item master.
- A return-only Customer × Principal occurrence does not create a relationship and does not update last purchase date.
- History rows are not deleted because of inactivity.
- Status is Active when last qualifying sale date is within the previous 6 months of the snapshot as-of date; otherwise Dormant.
- The snapshot is not limited to Top-N MTD relationships.
- Pair identity is Customer plus Supplier technical identifier. No new master key or manual assignment record is created.
- Pair Sales-Out uses PD-002 and does not include tax or returns as a reduction.
- Automated tests cover relationship creation, dormant transition, indefinite retention, and return-only non-creation.

#### Review Focus

- Persistence Compliance
- Workflow Compliance
- Architecture Compliance

---

### PCM-014

#### Objective

Reorient Customer lifecycle, portfolio, and decline presentation to Customer × Principal sales evidence where those claims require pair history.

#### Dependencies

- PCM-012
- PCM-013

#### Deliverables

- CU01 Principal mix from pair Sales-Out
- CU04 portfolio mix and gap view based on observed purchased Principals only
- CU02 Principal-specific decline or inactivity only from pair status and pair Sales-Out
- CU05 pair evidence instead of a scalar Salesman-owner explanation of Principal activity

#### Acceptance Criteria

- Customer total sales, credit, and piutang remain Customer-level and are not allocated to Principals.
- Principal decline or dormancy is computed from PCM-013 pair history, not from Customer totals or latest-Faktur Salesman.
- A Customer buying more than one Principal can show different pair statuses at the same time.
- Portfolio gap or mix views list Principals derived from that Customer's sales history only. They do not show a pre-purchase assigned Principal.
- CU05 does not present one Salesman column as the owner of the Customer–Principal relationship.
- No collection optimization queue is routed by Principal financial exposure.

#### Review Focus

- UI State Compliance
- Workflow Compliance
- Architecture Compliance

---

### PCM-015

#### Objective

Compose one Principal Entity Analytics profile whose primary commercial performance is sales-out, while preserving purchase-in as a separate operational context.

#### Dependencies

- PCM-002
- PCM-005
- PCM-006

#### Deliverables

- Principal Entity Analytics primary performance KPIs, growth axis, and sales evidence route sourced from Sales-owned snapshots
- Purchase-in KPIs retained and labeled as purchase-in, not as Principal sales performance
- Return amounts and Return Rate available as independent quality indicators
- Single-writer refresh that does not replace sales-out with purchase-only data
- Freshness that waits for, or reports the absence of, the Sales Principal snapshot

#### Acceptance Criteria

- The Principal profile commercial performance value equals `PR-KPI-001` for the same Principal and period.
- Growth and ranking for Principal commercial performance use Sales-Out (DPP), not MTD Purchase or purchase growth.
- Existing purchase KPIs remain available and are labeled purchase-in.
- Inventory metrics already on the Principal profile remain available and are not relabeled as sales-out.
- The producer does not calculate authoritative Sales-Out from Purchasing Management in-memory `SalesOutAmount`.
- A purchase refresh does not erase persisted Principal Sales-Out values from the profile.
- Evidence for commercial performance opens Principal Invoice Item evidence, not the purchasing report, when the selected measure is `PR-KPI-001`.
- No second writer replaces the Supplier/Principal entity snapshot independently.
- No Principal collection or credit KPI is composed onto the profile.
- Return Rate is present only as a quality indicator and is absent or null when Sales-Out (DPP) is not greater than zero.

#### Review Focus

- Architecture Compliance
- Persistence Compliance
- Workflow Compliance

---

### PCM-016

#### Objective

Realign Principal omzet relationship metadata to the canonical Principal Sales-Out KPI.

#### Dependencies

- PCM-002
- PCM-015

#### Deliverables

- Supplier/Principal relationship catalog metric references for sales-derived omzet relationships point to `PR-KPI-001`
- Evidence resolver and relationship labels consistent with that KPI
- Customer `TopPrincipalsByOmzet` relationship retained as transaction-derived sales, with KPI metadata aligned to `PR-KPI-001` or `CP-KPI-001`

#### Acceptance Criteria

- `TopCustomersByOmzet` metric KPI ID is `PR-KPI-001`, not `PU-KPI-001`.
- Other Supplier/Principal omzet relationships that currently reference `SF-KPI-008` reference `PR-KPI-001` instead.
- `PU-KPI-001` and `SF-KPI-008` definitions remain unchanged for their own purchasing and Salesman uses.
- Relationship amounts shown as Principal omzet equal the Sales-Out (DPP) evidence used by the profile.
- Relationship labels do not describe the measure as purchase or as assigned Customer ownership.
- Top-N relationship lists remain relationship presentations. They are not the source of Customer–Principal Active or Dormant status.

#### Review Focus

- Architecture Compliance
- Persistence Compliance
- UI State Compliance

---

### PCM-017

#### Objective

Add Principal sales attention to the executive attention and alert surfaces after Principal evidence exists, without Principal financial alerts.

#### Dependencies

- PCM-004
- PCM-006
- PCM-007

#### Deliverables

- EX01 Principal sales attention beside existing Salesman contribution and existing purchase or inventory Principal exposure
- EX02 Principal sales alerts with a deduplication rule against existing sales alerts
- Routing from those signals to SA04

#### Acceptance Criteria

- EX01 still shows company totals and existing non-sales Principal purchase or inventory exposure.
- New Principal sales attention uses Principal Sales-Out (DPP), target gap, or return-rate quality from approved KPIs.
- EX02 does not create Principal overdue, collection, or credit alerts.
- A Principal sales alert and an existing Salesman execution alert for the same underlying condition are distinguishable, and the implementation records which one is primary for commercial sales gap versus execution.
- Alert navigation opens SA04, not PU01, when the signal is Principal sales performance.
- Salesman execution alerts remain present.

#### Review Focus

- Workflow Compliance
- UI State Compliance
- Architecture Compliance

---

### PCM-018

#### Objective

Keep Purchasing and Inventory semantically separate from Principal sales performance and cross-link them to SA04 where Principal identity already exists.

#### Dependencies

- PCM-001
- PCM-007

#### Deliverables

- PU01 and PU02 labels that identify purchase-in, posting, stock, and dependency as purchasing measures
- Inventory Principal or Supplier rollups retain inventory meaning and link to SA04
- No new inventory forecast or optimization view

#### Acceptance Criteria

- PU01 does not display Principal Sales-Out (DPP) as if it were purchase value, and does not rename purchase growth to sales growth.
- PU02 remains purchase-invoice evidence.
- IN01 and IN02 Principal or Supplier exposures remain inventory measures and offer a navigation action to SA04 for the same Principal.
- IN03, IN04, and IN05 measures and views are otherwise unchanged.
- No inventory metric is copied into `PR-KPI-001`.

#### Review Focus

- UI State Compliance
- Workflow Compliance
- Architecture Compliance

---

### PCM-019

#### Objective

Synchronize permanent knowledge artifacts to the implemented Principal-centric analytical model after the functional slices are complete.

#### Dependencies

- PCM-001
- PCM-002
- PCM-007
- PCM-008
- PCM-009
- PCM-010
- PCM-011
- PCM-012
- PCM-014
- PCM-015
- PCM-016
- PCM-017
- PCM-018

#### Deliverables

Updates limited to behavior implemented by the prerequisite slices, in:

- `docs/foundation/DOMAIN.md` for Principal, Salesman commercial versus field dimensions, and Customer–Principal if those definitions are now reflected in product behavior
- `docs/features/btr-portal/btr-portal-domain.md`
- `docs/features/btr-portal/btr-portal-architecture.md`
- `docs/features/btr-portal/btr-portal-kpi-catalog.md`
- `docs/features/btr-portal/navigation-assets.md`
- `docs/features/btr-portal/business-question-catalog-v3.md`
- `docs/features/btr-portal/question-navigation-map.md`
- Entity Analytics developer guidance for sales-out versus purchase-in composition
- Dashboard feature artifacts for changed SA, CU, FI, SF, PU, and EX surfaces

#### Acceptance Criteria

- Updated artifacts describe Principal as the primary commercial performance dimension and do not describe Salesman as Customer owner.
- New management questions from feasibility section 4.3 that are actually implemented route to SA04, SA03, or Customer pair views. Unimplemented questions are not marked as available.
- KPI catalog text matches the implemented formulas and IDs.
- Navigation documentation matches implemented codes, including SA04 under Sales.
- Entity Analytics documentation states sales-out as Principal commercial performance and purchase-in as a separate operational context.
- Documentation does not claim Principal financial attribution, invoice-time Principal snapshot, or Customer–Principal master assignment.
- `docs/foundation/WORKFLOW.md` is unchanged unless an implemented workflow, not only analytics labeling, changed.

#### Review Focus

- Architecture Compliance
- Workflow Compliance

---

## Dependency Notes

- No slice depends on a higher-numbered slice.
- PCM-003 does not block any later slice. Material profiling findings are follow-on work, not a reason to reopen the approved model inside this plan.
- PCM-012 may proceed before pair history. Principal-specific Customer decline must wait for PCM-013 and PCM-014.
- Executive Principal sales promotion is PCM-017 and must not start before PCM-007.
- Permanent knowledge synchronization is PCM-019 and must not be treated as a substitute for the functional slices.

---

## Scope Coverage

| Authority item | Slice |
| --- | --- |
| GAP-001 many-to-many responsibility | PCM-006, PCM-007, PCM-011 |
| GAP-002 commercial versus field attribution | PCM-006, PCM-011, PCM-012 |
| GAP-003 transaction-derived Customer–Principal | PCM-013, PCM-014 |
| GAP-004 Item-master attribution | PCM-004, PCM-005, PCM-013 |
| GAP-005 no Principal financial attribution | PCM-002, PCM-007, PCM-011, PCM-014, PCM-017 |
| GAP-006 monthly target responsibility | PCM-006 |
| GAP-007 profiling | PCM-003 |
| GAP-008 migration, not a parallel capability set | PCM-007, PCM-008, PD-007 |
| GAP-009 Customer × Principal reorientation | PCM-012, PCM-013, PCM-014 |
| GAP-010 sales-out versus purchase-in | PCM-015, PCM-018 |
| GAP-011 single authoritative composition | PCM-015, PD-005 |
| GAP-012 no invoice-time snapshot | PCM-004, PCM-005 |
| GAP-013 primary navigation path | PCM-001, PCM-007 |
| GAP-014 no exclusive Salesman ownership | PCM-011, PCM-012, PCM-014 |
| GAP-015 line-item evidence grain | PCM-004, PCM-009 |
| GAP-016 no classification-framework redesign | PCM-002 |
| GAP-017 Principal user-facing terminology | PCM-001, PCM-007, PCM-019 |
| GAP-018 no Principal-scoped authorization | PCM-001, PD-009 |
| GAP-019 no header reconciliation requirement | PCM-002, PCM-004, PCM-009 |
| GAP-020 Sales-Out (DPP) and independent returns | PCM-002, PCM-004, PCM-007 |
| GAP-021 first-class Sales-Out from transactions | PCM-004, PCM-005, PCM-015 |
| GAP-022 omzet metadata realignment | PCM-016 |
| GAP-023 navigation registry | PCM-001 |
| TQ-006 6-month Active or Dormant | PCM-013 |
| TQ-007 no producer-model extension | PCM-015 |
| TQ-009 Invoice Item evidence | PCM-004, PCM-009 |
| TQ-010 canonical Principal Sales-Out KPI | PCM-002, PCM-016 |
| OQ-001 Salesman dashboards remain | PCM-008, PCM-011 |
| OQ-003 distinct KPI names | PCM-002, PCM-007, PCM-015 |
| OQ-004 historical limitations disclosed | PCM-002, PCM-005, PCM-007 |

---

## Implementation Constraints

The implementer must not:

- treat purchase growth as sales growth
- allocate piutang, open balance, or credit exposure to Principals
- infer exclusive Customer ownership from the latest Faktur
- remove Salesman execution surfaces
- require invoice-time Principal snapshots for historical Principal accuracy
- introduce a second Entity Analytics writer or a new producer framework
- change bonus, collection, or operational assignment workflows
- select a navigation code other than SA04 for Principal commercial performance
