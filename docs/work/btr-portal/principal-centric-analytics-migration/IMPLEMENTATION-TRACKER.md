# IMPLEMENTATION TRACKER

## Principal-Centric Analytics Migration

| Field | Value |
| --- | --- |
| Planning authority | FEASIBILITY ASSESSMENT |
| KPI semantics authority | PRINCIPAL KPI REGISTRY |
| Plan | `docs/work/btr-portal/principal-centric-analytics-migration/IMPLEMENTATION-PLAN.md` |
| Change log | `docs/work/btr-portal/principal-centric-analytics-migration/IMPLEMENTATION-PLAN-CHANGELOG.md` |
| Tracker date | 2026-09-09 |

Lifecycle:

```text
PLANNED
    ↓
IN IMPLEMENTATION
    ↓
IMPLEMENTED
    ↓
IN REVIEW
    ↓
GO
```

or

```text
PLANNED
    ↓
IN IMPLEMENTATION
    ↓
IMPLEMENTED
    ↓
IN REVIEW
    ↓
NO-GO
    ↓
REMEDIATION
    ↓
IN REVIEW
    ↓
GO
```

Only a review agent may set `GO` or `NO-GO`.

---

### PCM-001

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. Reserved SA04 Principal Performance under Sales at `/dashboard/principal-performance` with a placeholder page. Navigation documentation left for PCM-056.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. SA04 is registered under Sales after SA01 and before SA02. Route `/dashboard/principal-performance` resolves to the placeholder page. Existing listed routes and labels are unchanged. No Principal-scoped visibility and no permanent navigation documentation were added.
- Remediation History: none

### PCM-002

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. Registered `PRN-SALES-001` only in the working Principal KPI catalog, including PD-002 definition, GR-001 return protection, and the separate-ID Net Sales rule. Other KPI families and the permanent catalog were left for their writer slices and PCM-019.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. `PRN-SALES-001` is the only registered entry and is identified as the authoritative Principal performance and ranking KPI. The definition matches PD-002 and GR-001, including that returns, claims, and inventory adjustments do not reduce Sales-Out. No other Principal family, withdrawn ID, Net Sales ID, or Principal Health Score ID is registered. Existing non-Principal KPI definitions are unchanged. Permanent catalog sync remains PCM-019.
- Remediation History: none

### PCM-003

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. Recorded the GAP-007 profiling baseline from `btr2` for TQ-001 through TQ-005, TQ-008, and the nine section 9 checks. Findings do not change GAP-001 through GAP-023, the Principal KPI Registry, application behavior, schema, or KPI definitions.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. `PROFILING-BASELINE.md` contains a measured result for each required check, including TQ-001 through TQ-005 and TQ-008. The report states that findings do not change GAP-001 through GAP-023 or the Principal KPI Registry. No application behavior, schema, or KPI definition was changed.
  - 2026-09-09: IN REVIEW (independent re-review)
  - 2026-09-09: GO. Remeasured TQ-001 through TQ-005, TQ-008, and the nine section 9 checks against `btr2`. Results match `PROFILING-BASELINE.md`. Findings do not change GAP-001 through GAP-023 or the Principal KPI Registry. No application behavior, schema, or KPI definition was changed.
- Remediation History: none

### PCM-004

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. Created the Sales-owned current `PRN-SALES-001` snapshot from Faktur Item evidence. Unknown-Principal lines are written to the Sales-Out data-quality output. Existing `FakturPrincipalOmzetDal` Total-based `CompletedOmzet` is unchanged. No return KPIs are written.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. Current snapshot stores `PRN-SALES-001` only from `SUM(FakturItem.SubTotal - FakturItem.DiscRp)`. Void Fakturs use the existing void sentinel. Blank and unknown Principal lines are absent from Principal rows and stored in the Sales-Out data-quality output with amount and count. Returns, claims, and inventory adjustments are not deducted. Existing Total-based `CompletedOmzet` is unchanged. No `SupplierId` was added to `BTR_FakturItem`. Tests cover mixed-Principal Fakturs, tax exclusion, unknown Principal exclusion, and the rule that this slice does not write return rows.
- Remediation History: none

### PCM-005

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. Persisted Principal × year × month `PRN-SALES-001` history from Faktur Item evidence using current Item-master attribution. Current-month rows are copied from the PCM-004 snapshot. Unknown-Principal amounts are not assigned to history rows. The history header records that Item Principal is current Item master and historical reconstruction may be limited. No return history is written.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. History rows identify Principal, year, and month and store `PRN-SALES-001` only. Attribution uses current Item master and does not require invoice-time `SupplierId` or Purchasing Management `SalesOutAmount`. Current-month history is copied from the PCM-004 snapshot. Unknown-Principal amounts stay in the Sales-Out data-quality output and are not written to history. The history header records the Item-master historical limitation. No return history is written.
- Remediation History: none

### PCM-006

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. Persisted current-month `PRN-TGT-001` as the sum of `BTR_SalesPersonPrincipalTarget.TargetAmount` for that Principal and month. Catalog registers `PRN-TGT-001` only. No independent Principal Target record, `PRN-SALES-001`, `PRN-TGT-002`, `PRN-TGT-003`, or return KPI is written. `BTR_SalesPersonSupplier` is not used as the historical responsibility source.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. `PRN-TGT-001` equals the sum of salesman Principal Target amounts for that Principal and month. The snapshot is a derived ReportingContext projection, not an independently maintained Principal Target. The writer does not write `PRN-SALES-001`, `PRN-TGT-002`, `PRN-TGT-003`, or any return KPI. Historical responsibility is read from `BTR_SalesPersonPrincipalTarget` only. Catalog entry is `PRN-TGT-001` only.
- Remediation History: none

### PCM-007

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. SA04 reads the current `PRN-SALES-001` snapshot, ranks Principals by that KPI, shows the Unknown Principal exception count from the Sales-Out data-quality output, and states the PD-010 disclosures. Ranking drill-down opens Faktur Item evidence for the selected Principal. SA01, SA02, SA03, SF01, SF02, and SF03 are unchanged. No Purchasing Management `SalesOutAmount` is queried.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. SA04 remains under Sales and does not replace SA01, SA02, SA03, SF01, SF02, or SF03. The only Principal performance figure and ranking use stored `PRN-SALES-001`. Unknown Principal exception count is summed from Sales-Out data-quality line counts. PD-010 disclosures are shown, including that Returns do not reduce or redefine Principal Sales-Out. Drill-down opens Faktur Item evidence and does not query Purchasing Management `SalesOutAmount`. No return, target, growth, purchase, inventory, coverage, Net Sales, or Principal Health Score figure is shown.
- Remediation History: none

### PCM-008

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. SA01 keeps company header totals from the existing Faktur header `GrandTotal` snapshot and keeps Top Salesman contribution. Principal contribution reads stored `PRN-SALES-001` and `PRN-TGT-001`, ranks Principals by `PRN-SALES-001`, and opens SA04 for the selected Principal. No purchase, collection, credit, return, or Net Sales figure is shown.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. Company sales totals still come from the existing header `GrandTotal` measure. Top Salesman contribution remains. Principal contribution shows stored `PRN-SALES-001` and `PRN-TGT-001` and ranks by `PRN-SALES-001` only. A row action opens SA04 for the selected Principal. No purchase metric is relabeled as Principal sales. No Principal collection, credit, or Net Sales metric is shown.
- Remediation History: none

### PCM-009

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. SA03 keeps Faktur-header company totals labeled as header totals and adds Principal Sales-Out evidence for `PRN-SALES-001`. Principal filter and amount use Faktur Item `SubTotal - DiscRp` attributed by `Brg.SupplierId`. A selected Principal and period opens Faktur Item evidence. Header `GrandTotal` is not allocated. The report states that the measure is not required to equal `GrandTotal` and that returns, claims, and inventory adjustments are not deducted.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. SA03 Principal filter and amount use Faktur Item amounts and `Brg.SupplierId`. A mixed-Principal Faktur contributes each line to its own Principal and does not split `GrandTotal`. The measure is identified as `PRN-SALES-001` Principal Sales-Out and is stated not to equal Faktur `GrandTotal`. Returns, claims, and inventory adjustments are stated not to be deducted. Company header totals remain header totals. Evidence rows keep Faktur Item identity. No Principal open-balance, payment, or return column was added.
- Remediation History: none

### PCM-010

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. SA02 keeps company forecast figures on the existing company measure and adds a Principal forecast presentation. The presentation applies the existing SA02 forecast method to current-month `PRN-SALES-001` history compared with `PRN-TGT-001`. It is not a registry KPI, is not used to rank Principals, and states that the sum of Principal forecasts is not required to equal the company forecast. No Principal collection forecast was added.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. SA02 company forecast figures remain on the existing company measure. Principal forecast and pace use monthly `PRN-SALES-001` history compared with `PRN-TGT-001` through the existing SA02 forecast method. The presentation has no Principal KPI ID, is not used to rank Principals, and states that the sum of Principal forecasts is not required to equal the company forecast. It is not labeled Net Sales. No Principal collection forecast was added.
- Remediation History: none

### PCM-011

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. SF01 no longer describes a Customer portfolio as owned by the Salesman. Last-invoice dormant labels replace owned-book and dormant-portfolio wording. Assigned Customers on the Salesman profile is invoiced contribution, not an assigned book. Coaching, target allocation, invoiced contribution, and assigned Principal mix remain. `BTRPD_SalesmanPrincipalAchievement.CompletedOmzet` is unchanged. SF02, SF03, and FI01–FI04 are unchanged.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. SF01 no longer describes a Customer portfolio as owned by the Salesman. Last-invoice dormant labels replace owned-book and dormant-portfolio wording. Open piutang and overdue are stated as invoice-attributed exposure, not an owned book. Coaching, target allocation, invoiced contribution, and assigned Principal mix remain. `BTRPD_SalesmanPrincipalAchievement.CompletedOmzet` is unchanged. SF02, SF03, and FI01–FI04 are unchanged. No Principal filter was added to SF02 or SF03.
- Remediation History: none

### PCM-012

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. Customer Entity Analytics relabels the latest-Faktur Salesman relationship from Assigned Salesman to Last Invoicing Salesman. The profile overview and related-entity display state that this is the last invoicing Salesman, not the Customer owner. The stored relationship code is unchanged. CU01–CU05, `PRN-CUS-001`, `PRN-CUS-002`, and Customer–Principal relationship computation are unchanged.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. Customer Entity Analytics does not use Assigned Salesman or Owner as the label for the latest-Faktur Salesman. The display states that it is the last invoicing Salesman, not the Customer owner. CU01–CU05 are unchanged. `PRN-CUS-001` and `PRN-CUS-002` are not published. Customer–Principal relationships are not recomputed from raw transactions. No Customer–Principal master maintenance screen or assignment action was added.
- Remediation History: none

### PCM-013

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. Materialized `BTRPD_CustomerPrincipalRelationship` from all non-void Faktur Item history. Pair identity is Customer plus Supplier. Rows store first and last transaction dates, Active or Dormant status from the 6-month last-transaction rule, and pair-attributed `PRN-SALES-001`. Inactivity does not remove a pair. The projection refresh is the only path that reads historical transactions for relationship status. No consumer screen, `PRN-CUS-001`, `PRN-CUS-002`, return KPI, Entity Analytics entity type, or master assignment table was added.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. The projection is `BTRPD_CustomerPrincipalRelationship`. History is retained when inactive. Active and Dormant use the inclusive 6-month last-transaction rule. Pair identity is Customer plus Supplier. Pair Sales-Out is pair-attributed `PRN-SALES-001` and does not deduct returns, claims, or inventory adjustments. The projection is not limited to Top-N MTD. No `PRN-CUS-001`, `PRN-CUS-002`, or `PRN-RET-*` value is written. No consumer screen, Entity Analytics entity type, or master assignment table was added. Tests cover retention, the 6-month rule, and the absence of consumer queries.
- Remediation History: none

### PCM-014

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. CU01 shows Principal mix for ranking customers from `BTRPD_CustomerPrincipalRelationship` only. Pair amount is pair-attributed `PRN-SALES-001`. Customer sales, credit, and piutang remain Customer-level and are not allocated to Principals. No pre-purchase assigned Principal is shown. CU02–CU05 are unchanged.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. CU01 Principal mix reads `BTRPD_CustomerPrincipalRelationship` and does not recompute relationships from raw transactions. Pair amount is stored pair-attributed `PRN-SALES-001`. Customer sales, credit, and piutang remain Customer-level and are not allocated to Principals. The mix lists only Principals on that Customer's projection. No pre-purchase assigned Principal is shown. CU02–CU05 are unchanged.
- Remediation History: none

### PCM-015

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. Composed stored `PRN-SALES-001` onto the Supplier/Principal Entity Analytics profile as the commercial performance and default ranking KPI. The existing `SupplierEntityAnalyticsProducer` reads the Sales-owned snapshot and does not calculate Sales-Out from Purchasing Management `SalesOutAmount`. A purchase refresh retains persisted `PRN-SALES-001`. Evidence opens Faktur Item evidence. No return, target, growth, purchase-in, inventory, coverage, collection, credit, Health Score, or Net Sales pack was added.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. Principal profile commercial performance equals stored `PRN-SALES-001` for the same Principal and period. Default ranking uses `PRN-SALES-001`. The existing Supplier producer reads the owned snapshot and does not calculate Sales-Out from Purchasing Management `SalesOutAmount`. A purchase refresh does not erase persisted `PRN-SALES-001`. Evidence opens Faktur Item evidence, not the purchasing report. No second writer was added. No return, target, growth, purchase-in, inventory, coverage, collection, credit, Health Score, or Net Sales pack was composed.
- Remediation History: none

### PCM-016

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. Supplier/Principal sales omzet relationship metadata now references `PRN-SALES-001`. Relationship amounts use `SUM(FakturItem.SubTotal - FakturItem.DiscRp)`. Evidence for those relationships opens Faktur Item evidence. `PU-KPI-001` and `SF-KPI-008` definitions, and Customer top-Principal metadata, are unchanged.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. `TopCustomersByOmzet`, `TopSalesmenByOmzet`, and `TopProductsByOmzet` reference `PRN-SALES-001`, not `PU-KPI-001`, `PRN-PUR-001`, or `SF-KPI-008`. Relationship amounts use `SubTotal - DiscRp`. Omzet relationship evidence opens Faktur Item evidence and is not the evidence grain for `PRN-CUS-001` or `PRN-CUS-002`. `PU-KPI-001` and `SF-KPI-008` definitions remain unchanged. Customer top-Principal metadata remains `CU-KPI-009`. No withdrawn `PR-KPI-*` or `CP-KPI-*` relationship metadata was added.
- Remediation History: none

### PCM-017

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. EX01 shows Principal Sales-Out attention from stored `PRN-SALES-001` beside company sales and existing purchase or inventory Principal exposure. The signal routes to SA04. Purchase-In, Inventory, Returns, Health Score, and Net Sales are not used. EX02 is unchanged.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. EX01 still shows company totals and existing purchase or inventory Principal exposure. New Principal sales attention uses stored `PRN-SALES-001` only. Default commercial sales attention is Principal Sales-Out, not Purchase-In, Inventory, or Returns. No Principal Health Score or Net Sales alert. Principal sales navigation opens SA04, not PU01. EX02 is unchanged.
- Remediation History: none

### PCM-018

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. PU01 and PU02 identify purchase amounts as Purchase-In. Those amounts are not labeled Principal Sales-Out and do not display `PRN-SALES-001` as purchase value. Purchase growth is not renamed to sales growth. PU02 remains purchase-invoice evidence. IN01–IN05 and Entity Analytics `PRN-PUR-001` are unchanged.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. PU01 labels purchase amounts as Purchase-In and does not display `PRN-SALES-001` as purchase value. Purchase growth is not renamed to sales growth. PU02 remains purchase-invoice evidence and identifies invoice totals as Purchase-In. IN01–IN05 are unchanged. `PRN-PUR-001` is not published onto Entity Analytics.
- Remediation History: none

### PCM-019

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-020

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. Persisted `PRN-PUR-001` Purchase-In from Purchase Detail (`BTR_InvoiceItem.Total` attributed by `Invoice.SupplierId`) into a Purchasing-owned current snapshot. Catalog entry added. The writer does not write `PRN-SALES-001`, does not read Sales-Out history or Purchasing Management `SalesOutAmount`, leaves `PU-KPI-001` unchanged, and does not add an Entity Analytics purchase pack.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. `PRN-PUR-001` is stored from Purchase Detail and registered as Purchase-In. The writer does not write `PRN-SALES-001`, does not read Sales-Out history or Purchasing Management `SalesOutAmount`, leaves `PU-KPI-001` unchanged, and does not add an Entity Analytics purchase pack.
- Remediation History: none

### PCM-021

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. Persisted `PRN-INV-001` Inventory Value and `PRN-INV-002` Inventory Days from Inventory Snapshot evidence for Principal products. Inventory Days uses the existing Average Days of Supply measure. Catalog entries added. The writer does not write `PRN-SALES-001`, does not change IN01–IN05, and does not add an Entity Analytics inventory pack.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. `PRN-INV-001` is current inventory value for Principal products from Inventory Snapshot evidence. `PRN-INV-002` uses the existing Average Days of Supply coverage measure and introduces no new days-of-cover algorithm. Neither writer writes `PRN-SALES-001`. IN01–IN05 are unchanged. No Entity Analytics inventory pack was added.
- Remediation History: none

### PCM-022

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-023

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. Persisted current-month `PRN-RET-001`, `PRN-RET-002`, and `PRN-RET-003` from Return Item evidence (`ReturJualItem.SubTotal - ReturJualItem.DiscRp`, `JenisRetur` `BAGUS` and `RUSAK`). Catalog entries added. Void returns use the existing void sentinel. The writer does not write or update `PRN-SALES-001` and does not write `PRN-RET-004`.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. Current snapshot stores `PRN-RET-001` and `PRN-RET-002` from Return Item evidence using `JenisRetur` `BAGUS` and `RUSAK`. `PRN-RET-003` equals their sum. Void returns use the existing void sentinel. The writer does not write or update `PRN-SALES-001` and does not write `PRN-RET-004`. Tests prove return persistence leaves a previously stored Sales-Out value unchanged.
- Remediation History: none

### PCM-024

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-025

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-026

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-027

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-028

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-029

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-030

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-031

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-032

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-033

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-034

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-035

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. FI02 labels the Top Overdue Salesmen ranking as invoice-attributed overdue exposure, not account ownership. Ranking measures, customer and wilayah rankings, and FI04 are unchanged. No Principal overdue or collection KPI was added.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. FI02 describes the Salesman ranking as invoice-attributed overdue exposure, not account ownership. Ranking amounts, customer and wilayah rankings, and FI04 are unchanged. No Principal overdue or collection KPI was added. The investigation signal key, route, and period mode are unchanged.
- Remediation History: none

### PCM-036

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. FI04 labels the Salesman column as invoice-attributed on the open Faktur, not the Customer account owner. Report measures, row fields, and summary totals are unchanged. No Principal financial column was added. FI01 and FI03 are unchanged.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. FI04 describes the Salesman column as invoice-attributed on the open Faktur, not the Customer account owner. Report measures, row fields, and summary totals are unchanged. No Principal financial column was added. FI01 and FI03 are unchanged.
- Remediation History: none

### PCM-037

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. CU01 labels the latest-Faktur Salesman as Last Invoicing Salesman, a recency indicator, not Assigned Salesman or Owner. Customer credit, piutang, and lifecycle measures remain Customer-level. No Principal mix was added. CU02–CU05 are unchanged.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. CU01 does not use Assigned Salesman or Owner for the latest-Faktur Salesman. The column and disclosure state that it is the last invoicing Salesman, a recency indicator, not the Customer owner. Customer credit, piutang, and lifecycle measures remain Customer-level. No Principal mix was added. CU02–CU05 are unchanged.
  - 2026-09-09: IN REVIEW (independent)
  - 2026-09-09: GO. Independent review confirms CU01 column headers, ranking note, and attention-list column use Last Invoicing Salesman as a recency indicator, not Assigned Salesman or Owner. Customer credit, piutang, and lifecycle measures remain Customer-level. Principal mix on CU01 is PCM-014, not this slice. CU02–CU05 are unchanged. `customerAnalyticsAttribution.spec.ts`: 2 passed.
- Remediation History: none

### PCM-038

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-039

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-040

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-041

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-042

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-043

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-044

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-045

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-046

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-047

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-048

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-049

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-050

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-051

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-052

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-053

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-054

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-055

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-056

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-057

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-058

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none
