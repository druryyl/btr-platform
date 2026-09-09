# IMPLEMENTATION PLAN

## Principal-Centric Analytics Migration

| Field | Value |
| --- | --- |
| Planning authority | FEASIBILITY ASSESSMENT |
| KPI semantics authority | PRINCIPAL KPI REGISTRY |
| Authority artifacts | `docs/work/btr-portal/principal-centric-analytics-migration/FEASIBILITY-ASSESSMENT.md`; `docs/work/btr-portal/principal-centric-analytics-migration/PRINCIPAL-KPI-REGISTRY.md` |
| Plan date | 2026-09-09 |
| Revised | 2026-09-09 |
| Status | PLANNED |
| Implementation mode | Feasibility-driven (Mode B) |
| Change log | `docs/work/btr-portal/principal-centric-analytics-migration/IMPLEMENTATION-PLAN-CHANGELOG.md` |

This plan realizes the approved recommendation: **Option C, the dual-axis analytical model**, with GO / major redesign.

Principal Analytics KPI identity, calculation meaning, ranking, and ownership follow the Principal KPI Registry. Where this plan previously defined other Principal KPI IDs or measurement interpretations, those definitions are withdrawn. The registry wins on every conflict.

The plan does not introduce new business decisions beyond the registry, the approved feasibility gaps, and the implementation guardrails in this document.

---

## Planning Authority

```text
FEASIBILITY ASSESSMENT
```

For Principal Analytics semantics:

```text
PRINCIPAL KPI REGISTRY
```

The registry supersedes conflicting KPI definitions, terminology, formulas, ownership assumptions, and measurement interpretations in this plan.

Authoritative inputs used:

- Principal KPI Registry v1
- Approved operating model and GAP-001 through GAP-023, except where a KPI measurement in this plan conflicted with the registry
- Resolved questions BQ-001 through BQ-010, TQ-006, TQ-007, TQ-009, TQ-010, OQ-001 through OQ-004
- Section 4 impact inventory, including dashboard disposition
- Section 7 recommended approach and mandatory constraints
- Section 11 planner guidance

Implementation-validation questions TQ-001 through TQ-005 and TQ-008 are scheduled as PCM-003. They do not change the approved model or the registry and do not block later slices.

---

## Implementation Guardrails

These rules bind every slice. A slice that violates a guardrail is out of scope even if an older acceptance criterion appears to allow it.

### GR-001 — Return semantic protection

`PRN-SALES-001` Principal Sales-Out is an independent KPI.

It must never be reduced, replaced, or redefined by Returns KPIs:

- `PRN-RET-001` Good Return Amount
- `PRN-RET-002` Broken Return Amount
- `PRN-RET-003` Total Return Amount
- `PRN-RET-004` Return Percentage

Rules:

- A slice that writes `PRN-SALES-001` must not write any `PRN-RET-*` value.
- A slice that writes any `PRN-RET-*` value must not write, overwrite, or recalculate `PRN-SALES-001`.
- `PRN-RET-004` may read `PRN-SALES-001` as a denominator. That read does not authorize an update to `PRN-SALES-001`.
- Displays may show Sales-Out and Returns together. The Sales-Out figure shown must equal the stored `PRN-SALES-001` value.
- No Net Sales, net-of-returns, or sales-after-returns KPI is defined in this plan.
- Any future Net Sales KPI must be introduced as a separate KPI ID and must not replace, rename, or become the authoritative meaning of `PRN-SALES-001`.

### GR-002 — Customer–Principal relationship projection

Customer–Principal relationships must be materialized into one dedicated analytics projection.

Canonical projection: `BTRPD_CustomerPrincipalRelationship`.

This projection is not master data and is not a new Entity Analytics entity type.

Rules:

- Historical transaction data remains the source of truth.
- Only the projection refresh may read historical transactions to create or update relationship rows.
- Coverage, Active Customer, Dormant Customer, Relationship Analytics, and Entity Analytics relationship presentation must read the projection.
- Those consumers must not recompute relationship existence, last transaction date, Active status, or Dormant status from raw transaction history.
- The projection stores pair identity, first transaction date, last transaction date, Active or Dormant status, and pair-attributed `PRN-SALES-001` copied at refresh. Consumers read those stored attributes.
- History is retained indefinitely. Inactivity does not delete a projection row.
- No pre-purchase assignment record is created.

### GR-003 — Slice granularity

Each slice has one objective and one primary deliverable class:

- one projection, or
- one KPI family, or
- one API or evidence contract, or
- one dashboard or Entity Analytics feature

A slice must not combine multiple projections, KPI families, APIs, dashboards, or Entity Analytics features.

A slice that writes `PRN-SALES-001` is not the slice that writes Returns, Target, Growth, Purchase-In, Inventory, or Customer coverage.

---

## Scope Summary

Migrate BTR Portal commercial analytics so Principal is the primary commercial performance dimension, while:

- Customer remains an independent account and credit axis
- Salesman commercial attribution uses the portfolio owner on the transaction (`Faktur.SalesPersonId`)
- Salesman field activity remains performer-attributed and is not removed
- Customer–Principal is a transaction-derived analytical relationship, materialized as a projection, not master data
- Principal financial, collection, and credit attribution remains out of scope

Approved outcome:

- Principal performance is answered by `PRN-SALES-001` Principal Sales-Out
- Returns remain independent and never change Principal Sales-Out
- Target, achievement, growth, active customers, customer coverage, purchase-in, and inventory are answered by their registry KPIs and are not substituted for Principal Sales-Out
- Coverage, Active Customer, Dormant Customer, Relationship Analytics, and Entity Analytics consume `BTRPD_CustomerPrincipalRelationship`
- Salesman analytics remain available for execution and coaching, without Customer-ownership language
- Navigation distinguishes commercial portfolio, customer account, and sales execution
- Each registry KPI is traceable to its registry evidence grain

---

## KPI Registry Binding

Implement only these Principal KPI IDs. Do not create parallel Principal KPI IDs.

| KPI ID | Name | Role | Evidence grain | Writer slice |
| --- | --- | --- | --- | --- |
| PRN-SALES-001 | Principal Sales-Out | Authoritative Principal performance KPI and authoritative ranking KPI | Faktur Item | PCM-004, PCM-005 |
| PRN-RET-001 | Good Return Amount | Independent return KPI | Return Item | PCM-023 |
| PRN-RET-002 | Broken Return Amount | Independent return KPI | Return Item | PCM-023 |
| PRN-RET-003 | Total Return Amount | Good Return + Broken Return | Return Item | PCM-023 |
| PRN-RET-004 | Return Percentage | Return Amount ÷ Sales-Out; supporting ranking KPI | Return Item and PRN-SALES-001 | PCM-024 |
| PRN-TGT-001 | Principal Target | Sum of Salesman Principal Targets | SalesPersonPrincipalTarget | PCM-006 |
| PRN-TGT-002 | Achievement Amount | Principal Sales-Out versus Target | PRN-SALES-001 and PRN-TGT-001 | PCM-028 |
| PRN-TGT-003 | Achievement Percentage | Principal Sales-Out ÷ Principal Target; supporting ranking KPI | PRN-SALES-001 and PRN-TGT-001 | PCM-028 |
| PRN-PUR-001 | Purchase-In | Independent purchasing KPI; not a Principal ranking KPI | Purchase Detail | PCM-020 |
| PRN-INV-001 | Inventory Value | Independent inventory indicator | Inventory Snapshot | PCM-021 |
| PRN-INV-002 | Inventory Days | Independent inventory indicator | Inventory Snapshot | PCM-021 |
| PRN-CUS-001 | Active Customer Count | Active Customers purchasing the Principal | Customer × Principal Relationship Projection | PCM-042 |
| PRN-CUS-002 | Customer Coverage Percentage | Customer reach against the eligible customer base on the projection | Customer × Principal Relationship Projection | PCM-022 |
| PRN-GRW-001 | Month-over-Month Growth Percentage | Growth from Principal Sales-Out; supporting ranking KPI | PRN-SALES-001 | PCM-026 |
| PRN-GRW-002 | Year-over-Year Growth Percentage | Growth from Principal Sales-Out; supporting ranking KPI | PRN-SALES-001 | PCM-027 |

Ranking hierarchy:

1. Authoritative Principal ranking KPI: `PRN-SALES-001`.
2. Supporting ranking KPIs only: `PRN-RET-004`, `PRN-TGT-003`, `PRN-GRW-001`, `PRN-GRW-002`.
3. `PRN-PUR-001`, `PRN-INV-001`, and `PRN-INV-002` are never Principal performance ranking KPIs.
4. No composite Principal Health Score is introduced in V1.
5. No Net Sales KPI is introduced in V1.

Withdrawn IDs, which must not be implemented or referenced:

- `PR-KPI-001` through `PR-KPI-015`
- `CP-KPI-001` through `CP-KPI-005`

---

## Planning Decisions

These resolve items the feasibility assessment assigned to implementation planning. KPI measurement follows the registry. Implementation follows GR-001, GR-002, and GR-003.

### PD-001 — Navigation placement and code

Adopt the feasibility section 4.4 recommended direction.

- Add Principal commercial performance under the existing Sales group.
- Do not create a Principals domain group.
- Do not replace Sales Force.
- Do not use Entity Analytics or Purchasing as the only Principal commercial entry.
- Canonical navigation registry for this initiative is the implemented menu registry. `docs/features/btr-portal/navigation-assets.md` is updated in PCM-056, not in the code slice.
- New code: `SA04` — Principal Performance.
- Route: `/dashboard/principal-performance`.
- Sales group order after PCM-001: SA01, SA04, SA02, SA03.
- Do not assign Principal commercial performance to `EX03`, `SF03`, or `SF04`.
- Current implemented uses of those codes remain: `EX03` Entity Analytics, `SF02` Sales Force Overview, `SF03` Salesman Field Activity.
- Older reservations that conflict with implemented codes are superseded by the implemented registry. Do not reassign implemented codes.

### PD-002 — Principal Sales-Out

`PRN-SALES-001` is the authoritative Principal performance KPI and is independent of Returns.

```text
PRN-SALES-001 Principal Sales-Out (DPP)
  = SUM(FakturItem.SubTotal - FakturItem.DiscRp)
```

Rules:

- Evidence grain is Faktur Item.
- Include only non-void Fakturs (`Faktur.VoidDate = '3000-01-01'`, matching existing Principal omzet evidence).
- Attribute each line through `FakturItem.BrgId → BTR_Brg.SupplierId`.
- Do not use `FakturItem.Total`. That amount includes tax (`SubTotal - DiscRp + PpnRp`).
- Do not use `FakturItem.DppRp` as the performance measure. Stored `DppRp` applies `DppProsen` and is a tax-base amount, not Sales-Out (DPP).
- Do not use `Faktur.GrandTotal`.
- Do not allocate header tax, freight, rounding, or other header adjustments to Principals.
- Do not deduct Returns, Claims, or Inventory Adjustments.
- Do not write return amounts into the Sales-Out snapshot.
- Blank or unknown `SupplierId` is excluded from Principal totals and written to the Sales-Out data-quality output. Do not create a synthetic Principal.
- Company sales totals on existing company surfaces remain header `GrandTotal`. They are not required to equal the sum of `PRN-SALES-001`.

Existing `BTRPD_SalesmanPrincipalAchievement.CompletedOmzet` remains the existing invoice-line `Total` execution measure. Do not overwrite it with `PRN-SALES-001`.

### PD-003 — Returns

Returns are independent KPIs. They never reduce, replace, or redefine `PRN-SALES-001`.

Source: non-void return documents and Return Items, Principal via item master `SupplierId`.

```text
Line return amount = Return Item amount after commercial discount and before tax
PRN-RET-001 = sum of line return amount where JenisRetur = BAGUS
PRN-RET-002 = sum of line return amount where JenisRetur = RUSAK
PRN-RET-003 = PRN-RET-001 + PRN-RET-002
PRN-RET-004 = PRN-RET-003 ÷ PRN-SALES-001
             only when PRN-SALES-001 > 0; otherwise null
```

- Evidence grain is Return Item.
- The current Return Item amount after commercial discount and before tax is `ReturJualItem.SubTotal - ReturJualItem.DiscRp`.
- Exclude `PpnRp`.
- `JenisRetur` values are the existing operational values `BAGUS` and `RUSAK` only.
- Salesman on the return document is not used to reassign Faktur revenue, target, or bonus.
- Returns may appear in Entity Analytics, supporting rankings, attention signals, radar dimensions, and quality analysis.
- `PRN-RET-004` is a supporting ranking KPI. It is not a replacement for Principal Sales-Out.
- `PRN-RET-001`, `PRN-RET-002`, and `PRN-RET-003` are not the authoritative Principal ranking KPI.

### PD-004 — Target and achievement

- `PRN-TGT-001` for a Principal and month = `SUM(BTR_SalesPersonPrincipalTarget.TargetAmount)` for that `SupplierId`, `TargetYear`, and `TargetMonth`.
- Evidence grain is `SalesPersonPrincipalTarget`.
- No independently maintained Principal Target exists. No standalone Principal Target row is created.
- A Salesman is responsible for a Principal in a month only when a target record exists for that Salesman, Principal, year, and month.
- `BTR_SalesPersonSupplier` is current eligibility reference only. It must not override target-based historical responsibility.
- `PRN-TGT-002` presents `PRN-SALES-001` versus `PRN-TGT-001`. It is not a copy of Sales-Out, and it does not deduct returns, claims, or inventory adjustments.
- `PRN-TGT-003` = `PRN-SALES-001` ÷ `PRN-TGT-001` when `PRN-TGT-001` > 0; otherwise null.
- Achievement writers read `PRN-SALES-001`. They must not write it.
- Faktur Salesman × item Principal pairs with no target record for the transaction month remain in `PRN-SALES-001` and are written as responsibility exceptions. Do not drop the sale from Principal Sales-Out.
- Salesman contribution is a decomposition of `PRN-SALES-001`. It is not a registry KPI and is not a Principal ranking KPI.

### PD-005 — Persistence and composition

- Persist registry measures in ReportingContext snapshot tables owned by the domain that calculates them.
- Sales aggregation owns `PRN-SALES-001` and the growth KPIs derived only from that history.
- Returns aggregation owns `PRN-RET-001` through `PRN-RET-004` in separate writes from Sales-Out.
- Target aggregation owns `PRN-TGT-001`. Achievement reads Sales-Out and Target and writes only `PRN-TGT-002` and `PRN-TGT-003`.
- `BTRPD_CustomerPrincipalRelationship` owns relationship status. `PRN-CUS-001` and `PRN-CUS-002` are counted from that projection only.
- Purchasing remains the calculation owner of `PRN-PUR-001`.
- Inventory remains the calculation owner of `PRN-INV-001` and `PRN-INV-002`.
- Do not persist `SupplierId` on `BTR_FakturItem`.
- Do not add a Customer–Principal master table.
- Do not add effective-dated Salesman–Principal assignment tables.
- Purchasing Management in-memory `SalesOutAmount` is not `PRN-SALES-001`.
- Entity Analytics remains a composition and presentation layer. No producer-model extension.
- `SupplierEntityAnalyticsProducer` remains the single writer of the Supplier/Principal Entity Analytics profile.
- The producer reads owned snapshots. It must not recompute `PRN-SALES-001` from purchase data, and it must not replace the profile with purchase-only metrics.
- Entity Analytics relationship presentation reads `BTRPD_CustomerPrincipalRelationship`. It must not recompute relationships from raw transactions.
- Refresh order: source snapshots and the relationship projection complete before the Principal Entity Analytics refresh that consumes them.
- Portal consumers see one Principal profile. Sales-Out, returns, target, growth, customer, purchase-in, and inventory remain separate KPI packs inside that profile.
- Principal performance ranking on that profile uses `PRN-SALES-001` unless the user explicitly selects a supporting ranking KPI.

### PD-006 — Customer–Principal relationship projection

- Not a new Entity Analytics entity type.
- Not master data and not manually assignable.
- Canonical store is `BTRPD_CustomerPrincipalRelationship`.
- Historical transaction data is the source of truth for the projection refresh only.
- A pair is retained when a transaction has attributed the Customer to the Principal.
- History is retained indefinitely and is never removed by inactivity.
- Active means last transaction on the projection is within 6 months of the snapshot as-of date.
- Dormant means the projection row exists and last transaction is not within 6 months.
- `PRN-CUS-001` counts Active rows on the projection for that Principal. It must not scan raw transactions.
- `PRN-CUS-002` = `PRN-CUS-001` ÷ count of Customers on that Principal's projection when that count is greater than zero; otherwise null.
- The eligible customer base for `PRN-CUS-002` is the retained projection population for that Principal. Do not create a pre-purchase eligibility or assignment master.
- Coverage, Active Customer, Dormant Customer, Relationship Analytics, and Entity Analytics must consume the projection.
- Pair Sales-Out stored on the projection is `PRN-SALES-001` attributed to the pair at refresh. Do not register a separate Customer–Principal KPI ID.
- Do not register a Net Sales KPI.

### PD-007 — Surface disposition

Use feasibility section 4.1. This plan does not retire Salesman execution surfaces.

| Surface | Disposition in this plan | Slice |
| --- | --- | --- |
| SA01 | Keep company header totals and Salesman contribution. Add Principal decomposition using `PRN-SALES-001` and `PRN-TGT-001`. Rank Principals by `PRN-SALES-001`. | PCM-008 |
| SA02 | Keep company forecast. Add a Principal forecast presentation from `PRN-SALES-001` history and `PRN-TGT-001`. This forecast is not a registry KPI and is not a ranking KPI. | PCM-010 |
| SA03 | Add Faktur Item evidence for `PRN-SALES-001`. Keep Faktur-header company evidence distinct. | PCM-009 |
| SA04 | Primary Principal commercial performance path. Authoritative ranking is `PRN-SALES-001`. Other KPI families are separate panels. | PCM-007, PCM-030, PCM-031, PCM-032, PCM-033, PCM-034, PCM-054 |
| CU01–CU05 | Remove exclusive Salesman-ownership implication. Add Customer × Principal context only from the relationship projection. | PCM-037 through PCM-041, PCM-014, PCM-043, PCM-044, PCM-045 |
| FI01 | No Principal financial decomposition. | none |
| FI02, FI04 | Relabel Salesman as invoice-attributed. No Principal overdue or collection KPIs. | PCM-035, PCM-036 |
| FI03 | No Principal collection forecast. | none |
| SF01 | Reposition wording. Keep coaching, allocation, and existing execution achievement. | PCM-011 |
| SF02, SF03 | Preserve unchanged. Do not add Principal as primary grain or as a filter. | none |
| EX01, EX02 | Add Principal sales attention from registry KPIs after evidence exists. No Principal collection or credit alerts. No Principal Health Score. | PCM-017, PCM-050 |
| EX03 | Compose registry KPI packs in separate slices. Rank commercial performance by `PRN-SALES-001`. Relationship presentation reads the projection. | PCM-015, PCM-046, PCM-047, PCM-048, PCM-052, PCM-053, PCM-055 |
| PU01, PU02 | Preserve purchase evidence. User-facing Principal purchase measure is `PRN-PUR-001`, not Principal Sales-Out. | PCM-018 |
| IN01–IN05 | Preserve inventory metrics. Principal inventory measures on Principal analytics are `PRN-INV-001` and `PRN-INV-002`. Cross-link to SA04. Do not add new inventory forecast or optimization views. | PCM-051, PCM-021, PCM-053 |
| OP01 | No Principal migration. Do not redesign KPI entity classification. | none |

### PD-008 — KPI identity and classification

- Use the Principal KPI Registry IDs. Do not mint `PR-KPI-*` or `CP-KPI-*` IDs.
- Do not mutate existing non-Principal KPI formulas, names, or IDs.
- Do not redesign the four-entity classification framework.
- Classify registry Principal KPIs under the existing Supplier category where a category is required, and record the registry evidence grain.
- Canonical Principal performance measure referenced by Principal dashboards, rankings, relationships, and sales evidence is `PRN-SALES-001`.
- Existing `PU-KPI-001` remains the current purchasing catalog measure. Principal Purchase-In on Principal analytics is `PRN-PUR-001` and must not be presented as `PRN-SALES-001`.
- Salesman ranking KPI `SF-KPI-008` remains a Salesman measure. Principal omzet relationships must not reference it.
- Do not define Principal Sales Concentration, Principal Salesman Coverage, Principal Forecast, Required Pace, Target Gap, or Net Sales as registry KPIs.
- Do not define a Principal Health Score.

### PD-009 — Authorization and terminology

- No Principal-scoped authorization, row filter, or menu restriction.
- Users who can access commercial analytics may view all Principals, matching current portal access.
- User-facing labels use Principal and the registry KPI names.
- Technical identifiers remain `Supplier` / `SupplierId`.
- No equivalence table or database rename.
- Return Percentage is the user-facing name for `PRN-RET-004`. Do not label it as a deduction from Sales-Out.
- Do not label any figure Net Sales unless a future separate KPI is approved. This plan does not approve that KPI.

### PD-010 — Disclosure and data quality

Every Principal sales surface and the `PRN-SALES-001` definition must state:

- measure is Principal Sales-Out (DPP) from Faktur Item
- Returns, Claims, and Inventory Adjustments are not deducted
- Returns are independent KPIs and do not redefine Sales-Out
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
- Independently maintained Principal Target
- Claims, rebates, bonuses posting, or margin KPIs as Principal performance measures
- Deducting Claims, Inventory Adjustments, or Returns from `PRN-SALES-001`
- Replacing or redefining `PRN-SALES-001` with a Net Sales KPI
- Principal-scoped authorization or user-to-Salesman data scope
- KPI entity-classification framework redesign
- Replacement or removal of SF02 or SF03
- Principal filters on field-activity surfaces
- Coverage or substitute-execution reporting that reassigns revenue, target, or bonus
- BTrade, mobile, or sync changes
- Purchase-In or purchase growth used as Principal performance or Principal ranking
- Inventory Value or Inventory Days used to modify or rank Principal Sales-Out
- A new Entity Analytics entity type for Customer–Principal
- A new producer-model or multi-writer snapshot-merge framework
- Changing `BTRPD_SalesmanPrincipalAchievement.CompletedOmzet` from its existing Total-based measure
- Principal Health Score or any other composite Principal score
- `PR-KPI-*` and `CP-KPI-*` identifiers
- Recomputing Customer–Principal Active, Dormant, Coverage, or relationship presentation from raw transactions in a consumer slice

---

## Impact Inventory

### Backend

- Sales aggregation: `PRN-SALES-001` current snapshot and monthly history only
- Returns aggregation: independent return snapshot and return history
- Growth calculation: reads Sales-Out history only
- Target aggregation: `PRN-TGT-001` only; achievement is a separate read-only consumer of Sales-Out
- `BTRPD_CustomerPrincipalRelationship`: dedicated projection refresh from historical transactions
- `PRN-CUS-001` and `PRN-CUS-002`: counted from the projection only
- Customer, Customer Risk Forecast, and Customer Portfolio aggregators: remove singular ownership inference; consume the projection for Active and Dormant status
- Sales forecast aggregator: present Principal forecast from `PRN-SALES-001` history without creating a registry KPI
- Purchasing Management aggregator: calculation owner of `PRN-PUR-001`; not the source of `PRN-SALES-001`
- Inventory snapshot path: calculation owner of `PRN-INV-001` and `PRN-INV-002`
- `SupplierEntityAnalyticsProducer`: compose one registry pack per slice; relationship lists read the projection
- Executive and Alert composers: separate Principal sales attention slices
- Investigation and report query contracts: Principal sales evidence at Faktur Item grain; return evidence at Return Item grain
- Piutang and collection query contracts: label correction only

### Database

- New ReportingContext snapshots for Principal Sales-Out, return amounts, return percentage, Principal month Sales-Out history, return month history, Principal target, achievement, Salesman contribution, data-quality exceptions, and `BTRPD_CustomerPrincipalRelationship`
- Preserve `BTRPD_Salesman*` and `BTRPD_SalesmanPrincipalAchievement`
- No change to `BTR_FakturItem`, `BTR_Customer`, `BTR_SalesPersonSupplier`, or `BTR_SalesPersonPrincipalTarget` structure
- No Principal financial columns
- Entity Analytics L0–L5 tables store composed registry KPIs; do not add a Customer–Principal entity type
- No Principal Health Score storage
- No Net Sales storage

### Frontend

- `portalMenuCodes.ts`, `portalMenuRegistry.ts`, and Sales routes: SA04
- SA04 shell for `PRN-SALES-001`, then separate panels for target, returns, growth, supporting rankings, contribution, and customer counts
- SA01, SA02, and SA03 Principal evidence and decomposition
- One ownership-label slice per Customer page, then one projection-consumer slice per reoriented Customer surface
- SF01 wording correction; SF02 and SF03 unchanged
- FI02 and FI04 invoice-attributed labels in separate slices
- Entity Analytics Principal profile: one pack per slice
- EX01 and EX02 Principal sales attention in separate slices
- PU surfaces show `PRN-PUR-001` as Purchase-In
- IN surfaces cross-link to SA04; inventory KPIs are composed separately

### Integration

- No BTrade, mobile, or sync change
- Worker refresh order: Sales-Out, returns, target, and relationship projection before consumers and Entity Analytics composition
- Freshness shown from the existing refresh contract; no new orchestration platform

### Security

- No authorization model change
- No Principal row-level scope

---

## Phases

### Phase 1 — Contracts and validation

Establish navigation identity, the Sales-Out KPI definition, and the production profiling baseline.

### Phase 2 — Independent evidence projections

Persist Sales-Out, Returns, Target, and Sales-Out history in separate slices. Do not combine those writes.

### Phase 3 — Principal commercial surfaces

Deliver SA04 Sales-Out first. Add other KPI families as separate panels. Rank by `PRN-SALES-001`.

### Phase 4 — Ownership semantic correction

Correct one surface at a time. Preserve field activity and Customer financial truth.

### Phase 5 — Relationship projection and consumers

Materialize `BTRPD_CustomerPrincipalRelationship` first. Publish Active Customer and Coverage only from that projection. Reorient one Customer surface at a time.

### Phase 6 — Entity Analytics packs and executive promotion

Compose one Principal profile pack at a time. Promote EX01 and EX02 separately after Sales-Out evidence exists.

### Phase 7 — Permanent knowledge

Synchronize one documentation family at a time after the related functional slices are complete.

---

## Slices

### PCM-001

#### Objective

Reserve SA04 as the primary Principal commercial performance path under Sales.

#### Dependencies

- None

#### Deliverables

- `src/j05-btr-distrib/btr.portal.web/src/navigation/portalMenuCodes.ts`
- `src/j05-btr-distrib/btr.portal.web/src/navigation/portalMenuRegistry.ts`
- Route registration for `/dashboard/principal-performance`
- A reachable SA04 placeholder page. Full Sales-Out analytics arrive in PCM-007.

#### Acceptance Criteria

- `SA04` exists in the menu code registry and Sales group, labeled Principal Performance, ordered after SA01 and before SA02.
- SA04 is not assigned `EX03`, `SF03`, or `SF04`.
- Existing routes and labels for SA01, SA02, SA03, SF02, SF03, EX03, PU01, and PU02 still resolve.
- No Principal-scoped menu visibility is added.
- This slice does not update permanent navigation documentation. That is PCM-056.

#### Review Focus

- Workflow Compliance
- UI State Compliance

---

### PCM-002

#### Objective

Register `PRN-SALES-001` and the return semantic protection rule. Do not register other KPI families in this slice.

#### Dependencies

- None

#### Deliverables

- KPI catalog entry for `PRN-SALES-001` only
- Catalog statement that Returns KPIs must not reduce, replace, or redefine `PRN-SALES-001`
- Catalog statement that a future Net Sales KPI must be a separate ID and must not replace Principal Sales-Out

#### Acceptance Criteria

- `PRN-SALES-001` is identified as the authoritative Principal performance KPI and the authoritative ranking KPI.
- The definition matches PD-002 and GR-001.
- The definition states that returns, claims, and inventory adjustments do not reduce Sales-Out.
- No `PRN-RET-*`, `PRN-TGT-*`, `PRN-PUR-*`, `PRN-INV-*`, `PRN-CUS-*`, or `PRN-GRW-*` entry is added in this slice.
- No `PR-KPI-*`, `CP-KPI-*`, Net Sales, or Principal Health Score ID is registered.
- No existing non-Principal KPI ID, name, or formula is changed.

#### Review Focus

- Architecture Compliance
- Workflow Compliance

---

### PCM-003

#### Objective

Execute the GAP-007 production profiling checks and record the baseline. Findings do not alter the approved model or the registry.

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
  - expected `BTRPD_CustomerPrincipalRelationship` population, last-transaction activity, and 6-month Active count
  - evidence of Item `SupplierId` change or a statement that no historical source exists
- The report states that findings do not change GAP-001 through GAP-023 or the Principal KPI Registry.
- No application behavior, schema, or KPI definition is changed by this slice.

#### Review Focus

- Persistence Compliance
- Architecture Compliance

---

### PCM-004

#### Objective

Create the Sales-owned current `PRN-SALES-001` snapshot. Do not write Returns KPIs.

#### Dependencies

- PCM-002

#### Deliverables

- Sales evidence query at Faktur Item grain implementing PD-002
- Current ReportingContext snapshot for `PRN-SALES-001` and unknown-Principal exceptions
- Preservation of existing `FakturPrincipalOmzetDal` Total-based output used by `BTRPD_SalesmanPrincipalAchievement`

#### Acceptance Criteria

- The snapshot stores `PRN-SALES-001` and no `PRN-RET-*` value.
- `PRN-SALES-001` equals `SUM(FakturItem.SubTotal - FakturItem.DiscRp)` and does not include `PpnRp`, `FakturItem.Total`, `FakturItem.DppRp`, or `Faktur.GrandTotal`.
- `PRN-SALES-001` does not deduct Returns, Claims, or Inventory Adjustments.
- Void Fakturs are excluded using the existing void sentinel.
- Lines with blank or unknown `SupplierId` are absent from Principal rows and present in the Sales-Out data-quality output with amount and count.
- Existing Salesman × Principal achievement omzet is still populated from line `Total` and is not replaced by `PRN-SALES-001`.
- No `SupplierId` column is added to `BTR_FakturItem`.
- Automated tests cover mixed-Principal Fakturs, tax exclusion, unknown Principal exclusion, and proof that return rows are not written by this slice.

#### Review Focus

- Persistence Compliance
- Architecture Compliance

---

### PCM-005

#### Objective

Persist Principal Sales-Out monthly history used by growth and forecast. Do not write return history.

#### Dependencies

- PCM-004

#### Deliverables

- Principal × year × month history snapshot of `PRN-SALES-001`
- Backfill from available Faktur history using Item-master attribution, covering at least the current month, the prior month, and the same month in the prior year where source data exists
- Documented historical limitation on the history output

#### Acceptance Criteria

- Each history row identifies Principal, year, and month and stores `PRN-SALES-001` only. It does not store return amounts.
- History does not require invoice-time `SupplierId` on `BTR_FakturItem`.
- History is not read from Purchasing Management `SalesOutAmount`.
- Current-month history equals the current PCM-004 `PRN-SALES-001` for the same Principal and month.
- The history output records that Item Principal is current Item master and historical reconstruction may be limited.
- Unknown-Principal amounts remain in the Sales-Out data-quality output and are not assigned to a Principal history row.

#### Review Focus

- Persistence Compliance
- Architecture Compliance

---

### PCM-006

#### Objective

Aggregate `PRN-TGT-001` from Salesman Principal Targets.

#### Dependencies

- PCM-002

#### Deliverables

- `PRN-TGT-001` on the Principal target snapshot
- Catalog entry for `PRN-TGT-001` only

#### Acceptance Criteria

- `PRN-TGT-001` equals the sum of `BTR_SalesPersonPrincipalTarget.TargetAmount` for that Principal and month.
- No independent Principal Target record is written.
- This slice does not write `PRN-SALES-001`, `PRN-TGT-002`, `PRN-TGT-003`, or any return KPI.
- `BTR_SalesPersonSupplier` is not used as the historical responsibility source.

#### Review Focus

- Persistence Compliance
- Workflow Compliance

---

### PCM-007

#### Objective

Deliver the SA04 shell that shows `PRN-SALES-001` and ranks Principals by that KPI only.

#### Dependencies

- PCM-001
- PCM-004

#### Deliverables

- SA04 page showing `PRN-SALES-001` and default ranking by `PRN-SALES-001`
- Drill-down that opens Faktur Item evidence for `PRN-SALES-001`
- Disclosure required by PD-010
- Unknown-Principal exception count from the Sales-Out data-quality output

#### Acceptance Criteria

- SA04 is reachable from the Sales menu and does not replace SA01, SA02, SA03, SF01, SF02, or SF03.
- The only Principal performance figure and the only ranking on this slice use `PRN-SALES-001`.
- User-facing text uses Principal Sales-Out, not Supplier, except where a technical identifier is displayed.
- No return, target, growth, purchase, inventory, coverage, or Net Sales figure is added in this slice.
- The page states that Returns do not reduce or redefine Principal Sales-Out.
- SA04 does not query Purchasing Management in-memory `SalesOutAmount` for `PRN-SALES-001`.
- No Principal Health Score is shown.

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
- PCM-004
- PCM-006

#### Deliverables

- SA01 Principal contribution using `PRN-SALES-001` and `PRN-TGT-001`
- Principal list ranked by `PRN-SALES-001`
- Navigation from that view to SA04
- Unchanged company header total source

#### Acceptance Criteria

- SA01 company sales totals still use the existing header `GrandTotal` measure.
- Top Salesman remains available as contribution and coaching, not as the only commercial decomposition.
- SA01 shows `PRN-SALES-001` and `PRN-TGT-001`.
- Principal ranking on SA01 uses `PRN-SALES-001`, not Purchase-In, Inventory, Returns, or a composite score.
- SA01 does not relabel purchase metrics as Principal sales.
- SA01 does not show Principal collection, credit, or Net Sales metrics.
- A link or route action opens SA04 for a selected Principal.
- Existing SA01 Salesman contribution behavior remains available.

#### Review Focus

- UI State Compliance
- Workflow Compliance

---

### PCM-009

#### Objective

Make SA03 able to evidence `PRN-SALES-001` at Faktur Item grain without allocating Faktur header totals.

#### Dependencies

- PCM-002
- PCM-004

#### Deliverables

- Principal filter and Principal amount column based on `PRN-SALES-001`
- Faktur Item drill-down for a Principal and period
- Disclosure that header `GrandTotal` is not the Principal measure

#### Acceptance Criteria

- Filtering or decomposing the sales report by Principal uses Faktur Item amounts and `Brg.SupplierId`.
- A mixed-Principal Faktur contributes each line to its own Principal and does not split `GrandTotal`.
- The report identifies the measure as `PRN-SALES-001` Principal Sales-Out and states that it is not required to equal Faktur `GrandTotal`.
- The report states that returns, claims, and inventory adjustments are not deducted from the Principal amount and do not redefine it.
- Company report totals that remain header-based are labeled as header totals, not Principal sales.
- Evidence rows retain Faktur Item identity sufficient to trace a Principal total back to lines.
- No Principal open-balance, payment, or return column is added.

#### Review Focus

- Persistence Compliance
- UI State Compliance

---

### PCM-010

#### Objective

Add a Principal forecast presentation to SA02 from `PRN-SALES-001` history and `PRN-TGT-001`, without creating a registry KPI or a ranking KPI.

#### Dependencies

- PCM-005
- PCM-006

#### Deliverables

- Principal forecast presentation and pace derived from `PRN-SALES-001` history compared with `PRN-TGT-001`
- Disclosure that the presentation is not a registry KPI and is not required to equal company forecast
- Unchanged company forecast

#### Acceptance Criteria

- SA02 company forecast figures remain present and use their existing company measure.
- Principal forecast inputs are monthly `PRN-SALES-001` history from PCM-005, not Purchase-In, not Returns, and not `GrandTotal`.
- The comparison with target uses `PRN-TGT-001` and `PRN-SALES-001`.
- The forecast method is the existing SA02 method applied to the `PRN-SALES-001` series. No new forecast algorithm is introduced.
- The page does not assign the forecast a Principal KPI ID and does not use it to rank Principals.
- The page states that the sum of Principal forecasts is not required to equal the company forecast.
- No Principal collection forecast is added.
- The forecast is not labeled Net Sales.

#### Review Focus

- Architecture Compliance
- UI State Compliance

---

### PCM-011

#### Objective

Remove Customer-ownership wording from SF01 without changing field-activity or achievement measures.

#### Dependencies

- PCM-002

#### Deliverables

- SF01 label and copy correction for owned-book, assigned-book, and singular Customer-owner language

#### Acceptance Criteria

- SF01 no longer describes a Customer portfolio as owned by the Salesman.
- SF01 still shows coaching, target allocation, invoiced contribution, and assigned Principal mix.
- SF01 does not change `BTRPD_SalesmanPrincipalAchievement.CompletedOmzet` to `PRN-SALES-001`.
- SF02 and SF03 routes, grains, and performer attribution are unchanged.
- No Principal filter is added to SF02 or SF03.
- This slice does not change FI01–FI04.

#### Review Focus

- UI State Compliance
- Workflow Compliance

---

### PCM-012

#### Objective

Correct Customer Entity Analytics so the singular Salesman relationship is a last-invoicing recency indicator, not exclusive Customer ownership.

#### Dependencies

- PCM-002

#### Deliverables

- Customer Entity Analytics relationship relabeled from Assigned Salesman to last-invoicing Salesman

#### Acceptance Criteria

- Customer Entity Analytics does not use the label Assigned Salesman or Owner for the latest-Faktur Salesman.
- The display states that it is the last invoicing Salesman, not the Customer owner.
- This slice does not change CU01–CU05.
- This slice does not publish `PRN-CUS-001` or `PRN-CUS-002`.
- This slice does not recompute Customer–Principal relationships from raw transactions.
- No Customer–Principal master maintenance screen or assignment action is added.

#### Review Focus

- UI State Compliance
- Workflow Compliance

---

### PCM-013

#### Objective

Materialize `BTRPD_CustomerPrincipalRelationship` from historical transaction data. Do not publish consumer screens or customer KPI counts.

#### Dependencies

- PCM-002
- PCM-004

#### Deliverables

- Dedicated relationship projection with pair identity, first transaction date, last transaction date, Active or Dormant status, and pair-attributed `PRN-SALES-001`
- Projection refresh that is the only path reading historical transactions for relationship status
- No new Entity Analytics entity type and no master assignment table

#### Acceptance Criteria

- The projection is `BTRPD_CustomerPrincipalRelationship`.
- History rows are not deleted because of inactivity.
- Active and Dormant status are stored on the projection using the 6-month last-transaction rule.
- The projection is not limited to Top-N MTD relationships.
- Pair identity is Customer plus Supplier technical identifier. No new master key or manual assignment record is created.
- Pair Sales-Out stored on the projection equals pair-attributed `PRN-SALES-001` and does not deduct returns, claims, or inventory adjustments.
- This slice does not write `PRN-CUS-001`, `PRN-CUS-002`, or any `PRN-RET-*` value.
- Automated tests cover projection retention, the 6-month status rule, and the rule that consumer queries are not part of this slice.

#### Review Focus

- Persistence Compliance
- Architecture Compliance

---

### PCM-014

#### Objective

Show CU01 Principal mix from `BTRPD_CustomerPrincipalRelationship` only.

#### Dependencies

- PCM-037
- PCM-013

#### Deliverables

- CU01 Principal mix from pair-attributed `PRN-SALES-001` on the projection

#### Acceptance Criteria

- CU01 Principal mix reads the projection. It does not recompute relationships from raw transactions.
- Customer total sales, credit, and piutang remain Customer-level and are not allocated to Principals.
- The mix lists Principals present on that Customer's projection only.
- No pre-purchase assigned Principal is shown.
- This slice does not change CU02, CU03, CU04, or CU05.

#### Review Focus

- UI State Compliance
- Workflow Compliance

---

### PCM-015

#### Objective

Compose `PRN-SALES-001` onto the Principal Entity Analytics profile as the only commercial performance and default ranking KPI.

#### Dependencies

- PCM-002
- PCM-004

#### Deliverables

- Principal Entity Analytics performance value equal to `PRN-SALES-001`
- Default ranking by `PRN-SALES-001`
- Evidence route to Faktur Item evidence
- Single-writer refresh that does not replace Sales-Out with purchase-only data

#### Acceptance Criteria

- The Principal profile commercial performance value equals `PRN-SALES-001` for the same Principal and period.
- Default ranking uses `PRN-SALES-001`.
- This slice does not compose returns, target, growth, purchase-in, inventory, or customer coverage.
- The producer does not calculate `PRN-SALES-001` from Purchasing Management in-memory `SalesOutAmount`.
- A purchase refresh does not erase persisted `PRN-SALES-001` values from the profile.
- Evidence for `PRN-SALES-001` opens Faktur Item evidence, not the purchasing report.
- No second writer replaces the Supplier/Principal entity snapshot independently.
- No Principal collection, credit, Health Score, or Net Sales KPI is composed onto the profile.

#### Review Focus

- Architecture Compliance
- Persistence Compliance

---

### PCM-016

#### Objective

Realign Supplier/Principal sales omzet relationship metadata to `PRN-SALES-001`.

#### Dependencies

- PCM-002
- PCM-015

#### Deliverables

- Supplier/Principal relationship catalog metric references for sales-derived omzet relationships point to `PRN-SALES-001`
- Evidence resolver for those relationships consistent with that KPI

#### Acceptance Criteria

- `TopCustomersByOmzet` metric KPI ID is `PRN-SALES-001`, not `PU-KPI-001` and not `PRN-PUR-001`.
- Other Supplier/Principal sales omzet relationships that currently reference `SF-KPI-008` reference `PRN-SALES-001` instead.
- `PU-KPI-001` and `SF-KPI-008` definitions remain unchanged for their own purchasing and Salesman uses.
- Relationship amounts shown as Principal omzet equal `PRN-SALES-001`.
- This slice does not change Customer Entity Analytics top-Principal metadata. That is PCM-049.
- Top-N relationship lists are not the evidence grain for `PRN-CUS-001` or `PRN-CUS-002`.
- No relationship metadata points to a withdrawn `PR-KPI-*` or `CP-KPI-*` ID.

#### Review Focus

- Architecture Compliance
- Persistence Compliance

---

### PCM-017

#### Objective

Add Principal sales attention to EX01 after `PRN-SALES-001` evidence exists.

#### Dependencies

- PCM-004
- PCM-007

#### Deliverables

- EX01 Principal sales attention beside existing Salesman contribution and existing purchase or inventory Principal exposure
- Routing from that signal to SA04

#### Acceptance Criteria

- EX01 still shows company totals and existing non-sales Principal purchase or inventory exposure.
- New Principal sales attention uses `PRN-SALES-001` only in this slice.
- Default commercial sales attention is based on `PRN-SALES-001`, not Purchase-In, Inventory, or Returns.
- EX01 does not create a Principal Health Score or Net Sales alert.
- Alert navigation opens SA04, not PU01, when the signal is Principal sales performance.
- This slice does not change EX02.

#### Review Focus

- Workflow Compliance
- UI State Compliance

---

### PCM-018

#### Objective

Label PU01 and PU02 Purchase-In so it is not presented as Principal Sales-Out.

#### Dependencies

- PCM-001
- PCM-007

#### Deliverables

- PU01 and PU02 labels that identify purchase measures as Purchase-In

#### Acceptance Criteria

- PU01 does not display `PRN-SALES-001` as if it were purchase value, and does not rename purchase growth to sales growth.
- PU02 remains purchase-invoice evidence.
- This slice does not change IN01–IN05.
- This slice does not publish `PRN-PUR-001` onto Entity Analytics. That is PCM-020 and PCM-052.

#### Review Focus

- UI State Compliance
- Workflow Compliance

---

### PCM-019

#### Objective

Synchronize the permanent KPI catalog to the implemented registry definitions and guardrails.

#### Dependencies

- PCM-002
- PCM-004
- PCM-006
- PCM-022
- PCM-023
- PCM-024
- PCM-026
- PCM-027
- PCM-028
- PCM-042

#### Deliverables

- `docs/features/btr-portal/btr-portal-kpi-catalog.md` entries for implemented registry KPIs only

#### Acceptance Criteria

- Catalog text matches the registry, GR-001, and the implemented formulas.
- `PRN-SALES-001` is described as independent of Returns.
- The catalog states that a future Net Sales KPI must not replace Principal Sales-Out.
- The catalog states that Active Customer and Coverage read `BTRPD_CustomerPrincipalRelationship`.
- No `PR-KPI-*`, `CP-KPI-*`, Health Score, or Net Sales ID is added.
- This slice does not update navigation, domain, or dashboard feature artifacts.

#### Review Focus

- Architecture Compliance
- Workflow Compliance

---

### PCM-020

#### Objective

Persist `PRN-PUR-001` Purchase-In from Purchase Detail. Do not compose it onto Entity Analytics in this slice.

#### Dependencies

- PCM-002

#### Deliverables

- `PRN-PUR-001` calculated from Purchase Detail and stored for Principal analytics
- Catalog entry for `PRN-PUR-001`

#### Acceptance Criteria

- `PRN-PUR-001` evidence grain is Purchase Detail.
- `PRN-PUR-001` is not written into `PRN-SALES-001`.
- The value is not read from Sales-Out history and is not the Purchasing Management in-memory `SalesOutAmount`.
- Existing `PU-KPI-001` remains unchanged for its current purchasing use.
- This slice does not add a purchase pack to Entity Analytics.

#### Review Focus

- Persistence Compliance
- Architecture Compliance

---

### PCM-021

#### Objective

Persist `PRN-INV-001` and `PRN-INV-002` from the Inventory Snapshot. Do not compose them onto Entity Analytics in this slice.

#### Dependencies

- PCM-002

#### Deliverables

- `PRN-INV-001` and `PRN-INV-002` mapped from the existing Inventory Snapshot for Principal products
- Catalog entries for those two inventory KPIs

#### Acceptance Criteria

- `PRN-INV-001` is current inventory value for Principal products and uses Inventory Snapshot evidence.
- `PRN-INV-002` is estimated days of inventory coverage from Inventory Snapshot evidence. It uses the existing inventory coverage measure. No new days-of-cover algorithm is introduced.
- Neither inventory KPI modifies or writes `PRN-SALES-001`.
- This slice does not change IN01–IN05 views and does not add an Entity Analytics inventory pack.

#### Review Focus

- Persistence Compliance
- Architecture Compliance

---

### PCM-022

#### Objective

Publish `PRN-CUS-002` by reading `BTRPD_CustomerPrincipalRelationship` and stored `PRN-CUS-001`.

#### Dependencies

- PCM-013
- PCM-042

#### Deliverables

- `PRN-CUS-002` stored from the projection population and `PRN-CUS-001`
- Catalog entry for `PRN-CUS-002`

#### Acceptance Criteria

- `PRN-CUS-002` = `PRN-CUS-001` ÷ count of Customers on that Principal's relationship projection when that count is greater than zero; otherwise null.
- The denominator is the retained projection population, including Dormant Customers. It is not a manually assigned eligible-customer list.
- The calculation does not scan raw transaction history.
- `PRN-CUS-002` does not modify `PRN-SALES-001`.
- No `CP-KPI-*` ID is created.
- This slice does not render a dashboard panel. Display is PCM-054.

#### Review Focus

- Persistence Compliance
- Architecture Compliance

---

### PCM-023

#### Objective

Persist `PRN-RET-001`, `PRN-RET-002`, and `PRN-RET-003` without writing `PRN-SALES-001`.

#### Dependencies

- PCM-002

#### Deliverables

- Return Item evidence query
- Return snapshot storing the three return amount KPIs
- Catalog entries for those three IDs

#### Acceptance Criteria

- `PRN-RET-001` and `PRN-RET-002` follow `JenisRetur` `BAGUS` and `RUSAK`.
- `PRN-RET-003` equals `PRN-RET-001` plus `PRN-RET-002`.
- Void returns are excluded using the existing void sentinel.
- This slice does not write or update `PRN-SALES-001`.
- This slice does not write `PRN-RET-004`.
- Automated tests prove return persistence does not change a previously stored Sales-Out value.

#### Review Focus

- Persistence Compliance
- Architecture Compliance

---

### PCM-024

#### Objective

Persist `PRN-RET-004` by reading stored Sales-Out and Total Return Amount. Do not write either source KPI.

#### Dependencies

- PCM-004
- PCM-023

#### Deliverables

- `PRN-RET-004` snapshot
- Catalog entry for `PRN-RET-004`

#### Acceptance Criteria

- `PRN-RET-004` equals `PRN-RET-003` ÷ `PRN-SALES-001` when `PRN-SALES-001` is greater than zero, and is null otherwise.
- The writer reads stored `PRN-SALES-001` and `PRN-RET-003` and does not update those rows.
- `PRN-RET-004` is labeled Return Percentage, not a deduction from Sales-Out and not Net Sales.

#### Review Focus

- Persistence Compliance
- Architecture Compliance

---

### PCM-025

#### Objective

Persist return monthly history independently of Sales-Out history.

#### Dependencies

- PCM-023

#### Deliverables

- Principal × year × month return history for `PRN-RET-001`, `PRN-RET-002`, and `PRN-RET-003`

#### Acceptance Criteria

- History rows store return amounts only. They do not store or overwrite `PRN-SALES-001` history.
- Current-month return history equals the current PCM-023 amounts for the same Principal and month.
- Growth KPIs are not calculated in this slice.

#### Review Focus

- Persistence Compliance

---

### PCM-026

#### Objective

Calculate `PRN-GRW-001` from Principal Sales-Out history only.

#### Dependencies

- PCM-005

#### Deliverables

- `PRN-GRW-001` stored from Sales-Out month history
- Catalog entry for `PRN-GRW-001`

#### Acceptance Criteria

- `PRN-GRW-001` = (current month `PRN-SALES-001` − prior month `PRN-SALES-001`) ÷ prior month `PRN-SALES-001` when the prior month is greater than zero; otherwise null.
- The calculation does not use Purchase-In, returns, claims, or inventory adjustments.
- This slice does not write `PRN-SALES-001` or `PRN-GRW-002`.

#### Review Focus

- Persistence Compliance
- Architecture Compliance

---

### PCM-027

#### Objective

Calculate `PRN-GRW-002` from Principal Sales-Out history only.

#### Dependencies

- PCM-005

#### Deliverables

- `PRN-GRW-002` stored from Sales-Out month history
- Catalog entry for `PRN-GRW-002`

#### Acceptance Criteria

- `PRN-GRW-002` = (current month `PRN-SALES-001` − same month prior year `PRN-SALES-001`) ÷ same month prior year `PRN-SALES-001` when the prior-year month is greater than zero; otherwise null.
- The calculation does not use Purchase-In, returns, claims, or inventory adjustments.
- This slice does not write `PRN-SALES-001` or `PRN-GRW-001`.

#### Review Focus

- Persistence Compliance
- Architecture Compliance

---

### PCM-028

#### Objective

Persist `PRN-TGT-002` and `PRN-TGT-003` by reading stored Sales-Out and Principal Target. Do not write those source KPIs.

#### Dependencies

- PCM-004
- PCM-006

#### Deliverables

- Achievement snapshot for `PRN-TGT-002` and `PRN-TGT-003`
- Catalog entries for those two IDs

#### Acceptance Criteria

- `PRN-TGT-002` presents stored `PRN-SALES-001` versus stored `PRN-TGT-001` and does not replace or reduce `PRN-SALES-001`.
- `PRN-TGT-003` is null when `PRN-TGT-001` is not greater than zero.
- This slice does not write `PRN-SALES-001` or `PRN-TGT-001`.
- Achievement is not labeled Net Sales.

#### Review Focus

- Persistence Compliance
- Architecture Compliance

---

### PCM-029

#### Objective

Store Principal × Salesman commercial contribution and missing-target responsibility exceptions.

#### Dependencies

- PCM-004
- PCM-006

#### Deliverables

- Principal × Salesman contribution snapshot of `PRN-SALES-001` for the current period
- Responsibility exception output for sold Salesman × Principal pairs with no target record in the transaction month

#### Acceptance Criteria

- A Faktur line whose Salesman × Principal has no target record still remains in `PRN-SALES-001` and is listed as a responsibility exception.
- Contribution uses `Faktur.SalesPersonId` as commercial owner and stored `PRN-SALES-001`, not field-activity performer.
- One Principal can show more than one contributing Salesman.
- Salesman contribution is not stored or labeled as a registry KPI.
- This slice does not write `PRN-SALES-001`.

#### Review Focus

- Persistence Compliance
- Workflow Compliance

---

### PCM-030

#### Objective

Add the SA04 target and achievement panel.

#### Dependencies

- PCM-007
- PCM-028

#### Deliverables

- SA04 panel showing `PRN-TGT-001`, `PRN-TGT-002`, and `PRN-TGT-003`

#### Acceptance Criteria

- The panel reads stored target and achievement values.
- It does not change the displayed `PRN-SALES-001` amount.
- It does not show returns or Net Sales.
- Missing-target exceptions remain visible and do not remove Sales-Out.

#### Review Focus

- UI State Compliance
- Workflow Compliance

---

### PCM-031

#### Objective

Add the SA04 returns panel without changing Principal Sales-Out.

#### Dependencies

- PCM-007
- PCM-023
- PCM-024

#### Deliverables

- SA04 panel showing `PRN-RET-001`, `PRN-RET-002`, `PRN-RET-003`, and `PRN-RET-004`
- Return Item drill-down for those amounts

#### Acceptance Criteria

- Return KPIs are shown separately and do not change the displayed `PRN-SALES-001` amount.
- Return Percentage is labeled as a quality ratio, not as Net Sales.
- The panel does not add return amounts to, or subtract them from, Sales-Out.

#### Review Focus

- UI State Compliance
- Architecture Compliance

---

### PCM-032

#### Objective

Add the SA04 growth panel from Sales-Out growth KPIs only.

#### Dependencies

- PCM-007
- PCM-026
- PCM-027

#### Deliverables

- SA04 panel showing `PRN-GRW-001` and `PRN-GRW-002`

#### Acceptance Criteria

- Both growth figures use stored Sales-Out growth KPIs.
- The panel does not show purchase growth as sales growth.
- The panel does not change `PRN-SALES-001`.

#### Review Focus

- UI State Compliance
- Architecture Compliance

---

### PCM-033

#### Objective

Add SA04 supporting ranking controls for the approved supporting KPIs only.

#### Dependencies

- PCM-007
- PCM-024
- PCM-028
- PCM-026
- PCM-027

#### Deliverables

- Optional supporting rankings only for `PRN-RET-004`, `PRN-TGT-003`, `PRN-GRW-001`, and `PRN-GRW-002`
- Default ranking remains `PRN-SALES-001`

#### Acceptance Criteria

- The default ranking remains `PRN-SALES-001`.
- No supporting ranking replaces the stored Sales-Out value.
- Purchase-In, Inventory, Coverage, and Net Sales are not offered as Principal performance rankings.

#### Review Focus

- UI State Compliance
- Architecture Compliance

---

### PCM-034

#### Objective

Add the SA04 Salesman contribution panel.

#### Dependencies

- PCM-007
- PCM-029

#### Deliverables

- Salesman contribution within a selected Principal, labeled as contribution rather than a ranking KPI

#### Acceptance Criteria

- A Principal with multiple Salesmen shows each contributing Salesman.
- The panel does not assign Customer ownership to one Salesman.
- Contribution is not labeled a registry ranking KPI.
- Displayed contribution uses stored `PRN-SALES-001` decomposition and does not redefine Sales-Out.

#### Review Focus

- UI State Compliance
- Workflow Compliance

---

### PCM-035

#### Objective

Relabel FI02 Salesman columns as invoice-attributed.

#### Dependencies

- PCM-002

#### Deliverables

- FI02 Top Overdue Salesmen label correction

#### Acceptance Criteria

- FI02 describes the Salesman as invoice-attributed, not account owner.
- FI02 measures are otherwise unchanged.
- No Principal overdue or collection KPI is added.
- This slice does not change FI04.

#### Review Focus

- UI State Compliance
- Security Compliance

---

### PCM-036

#### Objective

Relabel FI04 Salesman columns as invoice-attributed.

#### Dependencies

- PCM-002

#### Deliverables

- FI04 Salesman column label correction

#### Acceptance Criteria

- FI04 describes the Salesman as invoice-attributed, not account owner.
- FI04 measures are otherwise unchanged.
- No Principal financial column is added.
- FI01 and FI03 behavior and measures are unchanged.

#### Review Focus

- UI State Compliance
- Security Compliance

---

### PCM-037

#### Objective

Correct CU01 labels that imply one Salesman owns the Customer.

#### Dependencies

- PCM-002

#### Deliverables

- CU01 labels, filters, and columns that currently imply one Salesman owner are corrected
- Singular latest-Faktur Salesman remains available only as a recency indicator

#### Acceptance Criteria

- CU01 does not use Assigned Salesman or Owner for the latest-Faktur Salesman.
- Customer credit, piutang, and lifecycle measures remain Customer-level.
- This slice does not add Principal mix. That is PCM-014.
- This slice does not change CU02–CU05.

#### Review Focus

- UI State Compliance

---

### PCM-038

#### Objective

Correct CU02 labels that imply one Salesman owns the Customer.

#### Dependencies

- PCM-002

#### Deliverables

- CU02 labels and attribution text corrected to recency or invoice attribution

#### Acceptance Criteria

- CU02 does not present one Salesman as the Customer owner.
- This slice does not add Principal-specific decline. That is PCM-043.
- This slice does not change other Customer pages.

#### Review Focus

- UI State Compliance

---

### PCM-039

#### Objective

Correct CU03 labels that imply account ownership when routing collection work.

#### Dependencies

- PCM-002

#### Deliverables

- CU03 labels that describe Salesman routing as operational, not account ownership

#### Acceptance Criteria

- CU03 collection queues remain Customer-level.
- Salesman routing is not described as Customer ownership.
- No Principal collection impact is added.
- This slice does not change CU01, CU02, CU04, or CU05.

#### Review Focus

- UI State Compliance
- Workflow Compliance

---

### PCM-040

#### Objective

Correct CU04 labels that imply one Salesman owns the Customer portfolio.

#### Dependencies

- PCM-002

#### Deliverables

- CU04 Salesman filter and displayed Salesman labels corrected to latest invoicing or commercial attribution

#### Acceptance Criteria

- CU04 does not describe the latest-Faktur Salesman as the Customer owner.
- This slice does not add Principal portfolio mix. That is PCM-044.
- This slice does not change other Customer pages.

#### Review Focus

- UI State Compliance

---

### PCM-041

#### Objective

Correct CU05 labels that present a scalar Salesman as the Customer owner.

#### Dependencies

- PCM-002

#### Deliverables

- CU05 Owner and Salesman column labels corrected

#### Acceptance Criteria

- CU05 does not label a Salesman column as Owner of the Customer.
- Customer totals remain Customer-level.
- This slice does not add pair evidence. That is PCM-045.
- This slice does not change other Customer pages.

#### Review Focus

- UI State Compliance

---

### PCM-042

#### Objective

Publish `PRN-CUS-001` by counting Active rows on `BTRPD_CustomerPrincipalRelationship`.

#### Dependencies

- PCM-013

#### Deliverables

- `PRN-CUS-001` stored from the projection
- Catalog entry for `PRN-CUS-001`

#### Acceptance Criteria

- `PRN-CUS-001` counts Customers on the projection whose stored status is Active.
- The count does not scan raw transaction history.
- A Dormant projection row is excluded from the count and is not deleted.
- This slice does not write projection status and does not write `PRN-SALES-001`.
- This slice does not render a dashboard panel.

#### Review Focus

- Persistence Compliance
- Architecture Compliance

---

### PCM-043

#### Objective

Show CU02 Principal-specific decline or inactivity from the relationship projection only.

#### Dependencies

- PCM-038
- PCM-013

#### Deliverables

- CU02 Principal decline or inactivity using projection status and pair-attributed `PRN-SALES-001`

#### Acceptance Criteria

- Decline or dormancy reads the projection. It does not use Customer totals, latest-Faktur Salesman, or a raw transaction scan.
- A Customer with more than one Principal can show different pair statuses at the same time.
- This slice does not change other Customer pages.

#### Review Focus

- UI State Compliance
- Workflow Compliance

---

### PCM-044

#### Objective

Show CU04 portfolio mix from the relationship projection only.

#### Dependencies

- PCM-040
- PCM-013

#### Deliverables

- CU04 portfolio mix based on the projection

#### Acceptance Criteria

- Portfolio mix lists Principals present on that Customer's projection only.
- It does not show a pre-purchase assigned Principal.
- It does not recompute relationships from raw transactions.
- This slice does not change other Customer pages.

#### Review Focus

- UI State Compliance
- Workflow Compliance

---

### PCM-045

#### Objective

Show CU05 pair evidence from the relationship projection instead of a scalar Salesman-owner explanation.

#### Dependencies

- PCM-041
- PCM-013

#### Deliverables

- CU05 pair evidence from `BTRPD_CustomerPrincipalRelationship`

#### Acceptance Criteria

- CU05 does not present one Salesman column as the owner of the Customer–Principal relationship.
- Pair status and pair Sales-Out are read from the projection.
- No collection optimization queue is routed by Principal financial exposure.
- This slice does not change other Customer pages.

#### Review Focus

- UI State Compliance
- Workflow Compliance

---

### PCM-046

#### Objective

Compose `PRN-GRW-001` and `PRN-GRW-002` onto the Principal Entity Analytics profile.

#### Dependencies

- PCM-015
- PCM-026
- PCM-027

#### Deliverables

- Entity Analytics growth values from stored Sales-Out growth KPIs

#### Acceptance Criteria

- Growth on the profile equals stored `PRN-GRW-001` and `PRN-GRW-002`.
- Growth does not use Purchase-In.
- This slice does not change the stored or displayed `PRN-SALES-001` performance value.
- This slice does not add return, target, purchase, or inventory packs.

#### Review Focus

- Architecture Compliance
- Persistence Compliance

---

### PCM-047

#### Objective

Compose the return KPI pack onto the Principal Entity Analytics profile without changing Principal Sales-Out.

#### Dependencies

- PCM-015
- PCM-023
- PCM-024

#### Deliverables

- Entity Analytics return pack for `PRN-RET-001` through `PRN-RET-004`
- Return Item evidence route

#### Acceptance Criteria

- Return values on the profile equal the stored return KPIs.
- `PRN-RET-004` is a quality and supporting ranking indicator only.
- `PRN-RET-004` is absent or null when stored `PRN-SALES-001` is not greater than zero.
- This slice does not write or replace `PRN-SALES-001`.
- Evidence for return KPIs opens Return Item evidence.

#### Review Focus

- Architecture Compliance
- Persistence Compliance

---

### PCM-048

#### Objective

Compose the target and achievement pack onto the Principal Entity Analytics profile.

#### Dependencies

- PCM-015
- PCM-006
- PCM-028

#### Deliverables

- Entity Analytics target pack for `PRN-TGT-001`, `PRN-TGT-002`, and `PRN-TGT-003`

#### Acceptance Criteria

- The pack reads stored target and achievement values.
- It does not replace `PRN-SALES-001` with achievement or with a net-of-returns amount.
- This slice does not add return, growth, purchase, or inventory packs.

#### Review Focus

- Architecture Compliance
- Persistence Compliance

---

### PCM-049

#### Objective

Align Customer top-Principal sales relationship metadata to `PRN-SALES-001` and the relationship projection.

#### Dependencies

- PCM-012
- PCM-013
- PCM-015

#### Deliverables

- Customer top-Principal sales relationship metadata aligned to `PRN-SALES-001`
- Relationship list membership that can be traced to the projection, not to a raw transaction rescan for Active or Dormant status

#### Acceptance Criteria

- The sales relationship metric is `PRN-SALES-001`.
- Active or Dormant interpretation, if shown, is read from the projection.
- The list is not the evidence grain for `PRN-CUS-001` or `PRN-CUS-002`.
- Labels do not describe assigned Customer ownership.
- This slice does not change Supplier omzet metadata. That is PCM-016.

#### Review Focus

- Architecture Compliance
- UI State Compliance

---

### PCM-050

#### Objective

Add Principal sales alerts to EX02 without Principal financial alerts.

#### Dependencies

- PCM-007
- PCM-017

#### Deliverables

- EX02 Principal sales alerts with a deduplication rule against existing sales alerts
- Routing from those alerts to SA04

#### Acceptance Criteria

- EX02 Principal sales alerts use `PRN-SALES-001`. This slice does not add return, collection, or credit alerts.
- A Principal sales alert and an existing Salesman execution alert for the same underlying condition are distinguishable.
- Alert navigation opens SA04, not PU01, when the signal is Principal sales performance.
- Salesman execution alerts remain present.
- This slice does not change EX01.

#### Review Focus

- Workflow Compliance
- UI State Compliance

---

### PCM-051

#### Objective

Cross-link existing inventory Principal or Supplier exposures to SA04 without changing inventory measures.

#### Dependencies

- PCM-001
- PCM-007

#### Deliverables

- IN01 and IN02 navigation action to SA04 for the same Principal

#### Acceptance Criteria

- IN01 and IN02 remain inventory measures.
- The navigation action opens SA04.
- IN03, IN04, and IN05 measures and views are otherwise unchanged.
- No inventory metric is copied into `PRN-SALES-001`.
- This slice does not publish `PRN-INV-001` or `PRN-INV-002`.

#### Review Focus

- UI State Compliance

---

### PCM-052

#### Objective

Compose the stored `PRN-PUR-001` pack onto the Principal Entity Analytics profile.

#### Dependencies

- PCM-015
- PCM-020

#### Deliverables

- Principal Entity Analytics purchase pack labeled Purchase-In

#### Acceptance Criteria

- `PRN-PUR-001` is present on the Principal profile and labeled Purchase-In.
- It is not used as the Principal performance KPI or as a Principal ranking KPI.
- It does not change `PRN-SALES-001`, `PRN-GRW-001`, or `PRN-GRW-002`.
- No purchase refresh removes `PRN-SALES-001` from the profile.

#### Review Focus

- Architecture Compliance
- UI State Compliance

---

### PCM-053

#### Objective

Compose the stored inventory KPI pack onto the Principal Entity Analytics profile.

#### Dependencies

- PCM-015
- PCM-021

#### Deliverables

- Principal Entity Analytics inventory pack labeled as inventory indicators

#### Acceptance Criteria

- The profile shows stored `PRN-INV-001` and `PRN-INV-002`.
- Inventory KPIs are labeled as operational indicators, not sales performance.
- Neither inventory KPI modifies `PRN-SALES-001` or becomes a performance ranking KPI.
- The Principal profile still ranks commercial performance by `PRN-SALES-001`.

#### Review Focus

- Architecture Compliance
- UI State Compliance

---

### PCM-054

#### Objective

Show stored `PRN-CUS-001` and `PRN-CUS-002` on SA04.

#### Dependencies

- PCM-007
- PCM-042
- PCM-022

#### Deliverables

- SA04 customer-reach panel reading stored Active Customer Count and Customer Coverage Percentage

#### Acceptance Criteria

- The panel reads stored `PRN-CUS-001` and `PRN-CUS-002`.
- It does not recompute Active, Dormant, or Coverage from raw transactions.
- It does not change `PRN-SALES-001`.
- It states that the evidence grain is the Customer–Principal relationship projection.

#### Review Focus

- UI State Compliance
- Architecture Compliance

---

### PCM-055

#### Objective

Make Entity Analytics relationship presentation consume `BTRPD_CustomerPrincipalRelationship`.

#### Dependencies

- PCM-013
- PCM-015

#### Deliverables

- Principal and Customer Entity Analytics relationship presentation of pair status and pair Sales-Out from the projection

#### Acceptance Criteria

- Relationship existence, last transaction date, Active status, and Dormant status are read from the projection.
- The presentation does not recompute those attributes from raw transaction history.
- Pair Sales-Out shown equals the projection copy of `PRN-SALES-001` and is not reduced by returns.
- This slice does not create a Customer–Principal entity type.
- Top-N lists are not used as the Active or Dormant source.

#### Review Focus

- Architecture Compliance
- Persistence Compliance
- UI State Compliance

---

### PCM-056

#### Objective

Consolidate the navigation asset registry to the implemented SA04 code.

#### Dependencies

- PCM-001

#### Deliverables

- `docs/features/btr-portal/navigation-assets.md` update for SA04 and superseded older reservations

#### Acceptance Criteria

- The navigation asset registry names one authoritative code list and records that older conflicting reservations of `EX03`, `SF03`, and `SF04` are not used for new assignment.
- SA04 is documented under Sales.
- This slice does not change menu code implementation.

#### Review Focus

- Workflow Compliance

---

### PCM-057

#### Objective

Synchronize portal domain, architecture, and Entity Analytics guidance to the implemented model and guardrails.

#### Dependencies

- PCM-007
- PCM-013
- PCM-015
- PCM-023
- PCM-055

#### Deliverables

- `docs/features/btr-portal/btr-portal-domain.md`
- `docs/features/btr-portal/btr-portal-architecture.md`
- Entity Analytics developer guidance for separate KPI packs, ranking hierarchy, and projection consumption
- `docs/foundation/DOMAIN.md` only if implemented relationship behavior requires it

#### Acceptance Criteria

- Updated artifacts describe `PRN-SALES-001` as independent of Returns.
- Updated artifacts state that Coverage, Active Customer, Dormant Customer, Relationship Analytics, and Entity Analytics consume `BTRPD_CustomerPrincipalRelationship`.
- Documentation does not claim Principal financial attribution, invoice-time Principal snapshot, Customer–Principal master assignment, Net Sales, or a Principal Health Score.
- `docs/foundation/WORKFLOW.md` is unchanged unless an implemented workflow, not only analytics labeling, changed.

#### Review Focus

- Architecture Compliance
- Workflow Compliance

---

### PCM-058

#### Objective

Synchronize dashboard feature artifacts and question navigation to implemented surfaces only.

#### Dependencies

- PCM-007
- PCM-008
- PCM-009
- PCM-010
- PCM-011
- PCM-014
- PCM-017
- PCM-018
- PCM-030
- PCM-031
- PCM-043
- PCM-044
- PCM-045
- PCM-050
- PCM-051
- PCM-054

#### Deliverables

- Dashboard feature artifacts for changed SA, CU, FI, SF, PU, and EX surfaces
- `docs/features/btr-portal/business-question-catalog-v3.md`
- `docs/features/btr-portal/question-navigation-map.md`

#### Acceptance Criteria

- New management questions from feasibility section 4.3 that are actually implemented route to the implemented surface. Unimplemented questions are not marked as available.
- Question text does not describe Salesman as Customer owner.
- Question text does not describe Returns as a reduction of Principal Sales-Out.
- This slice does not update the KPI catalog. That is PCM-019.

#### Review Focus

- Workflow Compliance

---

## Dependency Notes

- A slice may depend only on earlier-numbered slices, except the documentation slices PCM-019, PCM-056, PCM-057, and PCM-058, whose dependencies are listed on those slices.
- PCM-003 does not block any later slice. Material profiling findings are follow-on work, not a reason to reopen the approved model, the registry, or the guardrails.
- Ownership-label slices may proceed before the relationship projection. Projection-backed Customer claims must wait for PCM-013.
- `PRN-CUS-001` must wait for PCM-042. `PRN-CUS-002` must wait for PCM-022. SA04 customer reach must wait for PCM-054.
- Executive Principal sales promotion must not start before PCM-007.
- Entity Analytics packs other than Sales-Out must not start before PCM-015.
- No consumer may treat raw transaction history as the relationship read model after PCM-013 exists.
- Permanent knowledge slices must not be treated as a substitute for the functional slices.

---

## Scope Coverage

| Authority item | Slice |
| --- | --- |
| GR-001 Return semantic protection | PCM-002, PCM-004, PCM-023, PCM-024, PCM-031, PCM-047 |
| GR-002 Relationship projection | PCM-013, PCM-042, PCM-022, PCM-014, PCM-043, PCM-044, PCM-045, PCM-049, PCM-054, PCM-055 |
| GR-003 Slice granularity | All slices |
| PRN-SALES-001 authoritative performance and ranking | PCM-002, PCM-004, PCM-005, PCM-007, PCM-008, PCM-009, PCM-015 |
| PRN-RET-001 through PRN-RET-003 | PCM-023, PCM-025, PCM-031, PCM-047 |
| PRN-RET-004 | PCM-024, PCM-031, PCM-033, PCM-047 |
| PRN-TGT-001 | PCM-006, PCM-030, PCM-048 |
| PRN-TGT-002 and PRN-TGT-003 | PCM-028, PCM-030, PCM-048 |
| PRN-PUR-001 | PCM-020, PCM-018, PCM-052 |
| PRN-INV-001 and PRN-INV-002 | PCM-021, PCM-053 |
| PRN-CUS-001 | PCM-042, PCM-054 |
| PRN-CUS-002 | PCM-022, PCM-054 |
| PRN-GRW-001 and PRN-GRW-002 | PCM-026, PCM-027, PCM-032, PCM-046 |
| No Net Sales replacement of Sales-Out | PCM-002, PD-003, Out of Scope |
| GAP-001 many-to-many responsibility | PCM-029, PCM-034, PCM-011 |
| GAP-002 commercial versus field attribution | PCM-029, PCM-011, PCM-012 |
| GAP-003 transaction-derived Customer–Principal | PCM-013, PCM-014, PCM-043, PCM-044, PCM-045 |
| GAP-004 Item-master attribution | PCM-004, PCM-005, PCM-013 |
| GAP-005 no Principal financial attribution | PCM-002, PCM-007, PCM-011, PCM-035, PCM-036, PCM-017 |
| GAP-006 monthly target responsibility | PCM-006, PCM-029 |
| GAP-007 profiling | PCM-003 |
| GAP-008 migration, not a parallel capability set | PCM-007, PCM-008, PD-007 |
| GAP-009 Customer × Principal reorientation | PCM-013, PCM-014, PCM-043, PCM-044, PCM-045 |
| GAP-010 sales-out versus purchase-in | PCM-015, PCM-018, PCM-020, PCM-052 |
| GAP-011 single authoritative composition | PCM-015, PD-005 |
| GAP-012 no invoice-time snapshot | PCM-004, PCM-005 |
| GAP-013 primary navigation path | PCM-001, PCM-007 |
| GAP-014 no exclusive Salesman ownership | PCM-011, PCM-012, PCM-037 through PCM-041 |
| GAP-015 line-item evidence grain | PCM-004, PCM-009 |
| GAP-016 no classification-framework redesign | PCM-002 |
| GAP-017 Principal user-facing terminology | PCM-001, PCM-007, PCM-056 |
| GAP-018 no Principal-scoped authorization | PCM-001, PD-009 |
| GAP-019 no header reconciliation requirement | PCM-002, PCM-004, PCM-009 |
| GAP-020 Sales-Out (DPP) and independent returns | PCM-002, PCM-004, PCM-023, PCM-024 |
| GAP-021 first-class Sales-Out from transactions | PCM-004, PCM-005, PCM-015 |
| GAP-022 omzet metadata realignment | PCM-016, PCM-049 |
| GAP-023 navigation registry | PCM-001, PCM-056 |
| TQ-006 6-month Active or Dormant | PCM-013, PCM-042, PCM-022 |
| TQ-007 no producer-model extension | PCM-015 |
| TQ-009 Invoice Item evidence for Principal sales | PCM-004, PCM-009 |
| TQ-010 canonical Principal Sales-Out KPI | PCM-002, PCM-016 |
| OQ-001 Salesman dashboards remain | PCM-008, PCM-011 |
| OQ-003 distinct KPI names | PCM-002, PCM-007, PCM-023, PCM-020, PCM-021 |
| OQ-004 historical limitations disclosed | PCM-002, PCM-005, PCM-007 |

---

## Implementation Constraints

The implementer must not:

- use any Principal KPI ID other than the Principal KPI Registry IDs
- reduce, replace, or redefine `PRN-SALES-001` with any Returns KPI
- introduce Net Sales as a replacement for Principal Sales-Out
- treat Purchase-In or purchase growth as Principal Sales-Out or as a Principal ranking KPI
- treat Inventory Value or Inventory Days as a modifier or ranking source for Principal Sales-Out
- deduct returns, claims, or inventory adjustments from `PRN-SALES-001`
- allocate piutang, open balance, or credit exposure to Principals
- infer exclusive Customer ownership from the latest Faktur
- remove Salesman execution surfaces
- require invoice-time Principal snapshots for historical Principal accuracy
- introduce a second Entity Analytics writer or a new producer framework
- introduce a Principal Health Score
- recompute Customer–Principal relationship status from raw transactions in a consumer slice
- combine a second projection, KPI family, API, dashboard, or Entity Analytics feature into the assigned slice
- change bonus, collection, or operational assignment workflows
- select a navigation code other than SA04 for Principal commercial performance
