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

- Status: GO
- Implementation History:
  - 2026-09-10: IN IMPLEMENTATION
  - 2026-09-10: IMPLEMENTED. Synchronized `docs/features/btr-portal/btr-portal-kpi-catalog.md` to the Principal KPI Registry v1 and GR-001/GR-002: added §6.9 with all 15 implemented registry KPI entries (PRN-SALES-001, PRN-RET-001–004, PRN-TGT-001–003, PRN-GRW-001–002, PRN-CUS-001–002, PRN-PUR-001, PRN-INV-001–002) with registry formulas, evidence grains, ranking hierarchy, return-semantic protection, separate-ID Net Sales rule, and projection consumption; added PRN prefix/registry rows (§3/§3.1), SA04 menu/index rows (§4/§7), and Daftar Isi §6.9. No PR-KPI/CP-KPI/Health Score/Net Sales entry added. Navigation, domain, and dashboard artifacts unchanged.
- Review History:
  - 2026-09-10: IN REVIEW
  - 2026-09-10: GO. Catalog text matches registry, GR-001/GR-002, and implemented formulas; PRN-SALES-001 independent of Returns; future Net Sales separate-ID rule stated; Active/Coverage read BTRPD_CustomerPrincipalRelationship; no PR-KPI/CP-KPI/Health Score/Net Sales entry added; only catalog + tracker changed.
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

- Status: GO
- Implementation History:
  - 2026-09-10: IN IMPLEMENTATION
  - 2026-09-10: IMPLEMENTED. Computed `PRN-CUS-002` Customer Coverage Percentage from the stored `BTRPD_CustomerPrincipalRelationship` projection population and stored `PRN-CUS-001`. The writer reads the projection and the Active Customer snapshot, does not scan raw transactions, does not write projection status, and does not write `PRN-SALES-001`. Catalog registers `PRN-CUS-002` only. No dashboard panel, `CP-KPI-*` ID, or Net Sales KPI is introduced.
- Review History:
  - 2026-09-10: IN REVIEW
  - 2026-09-10: GO. `PRN-CUS-002` equals stored `PRN-CUS-001` divided by the count of Customers on that Principal's relationship projection when the count is greater than zero; otherwise null. The denominator includes Dormant Customers and is the retained projection population, not a manually assigned eligible-customer list. The writer reads the stored projection and the stored Active Customer snapshot, does not scan raw transaction history, does not write projection status, and does not write `PRN-SALES-001`. No `CP-KPI-*` ID or dashboard panel is introduced. `btr.application`, `btr.infrastructure`, `btr.test`, `btr.portal.worker`, and `btr.sql` build. 9 targeted tests pass. Broader ReportingContext/Principal tests pass; remaining full-suite failures are pre-existing database-connectivity issues unrelated to this slice.
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

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. Persisted `PRN-RET-004` Return Percentage from stored `PRN-RET-003` and stored `PRN-SALES-001` (`PRN-RET-003 ÷ PRN-SALES-001` when Sales-Out > 0, otherwise null). Catalog registers `PRN-RET-004` as Return Percentage, a supporting ranking KPI, not a deduction from Sales-Out and not Net Sales. The writer does not write, overwrite, or recalculate `PRN-SALES-001` or `PRN-RET-003`.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. `PRN-RET-004` equals stored `PRN-RET-003` ÷ stored `PRN-SALES-001` when Sales-Out > 0, otherwise null. The writer reads stored sources and does not update those rows. Catalog labels it Return Percentage, not a deduction and not Net Sales. Full solution builds. 28 tests pass, including percentage composition, source-row protection, catalog, and refresh orchestration.
- Review History: none
- Remediation History: none

### PCM-025

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. Persisted Principal × year × month `PRN-RET-001`, `PRN-RET-002`, and `PRN-RET-003` history from Return Item evidence using Item-master attribution. Current-month history rows are replaced by the current PCM-023 snapshot. No `PRN-SALES-001` history is stored or overwritten and no growth KPI is calculated.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. History rows store return amounts only from Return Item evidence and do not store or overwrite `PRN-SALES-001` history. Current-month history equals the current PCM-023 snapshot for the same Principal and month. No growth KPI is calculated. Returns remain independent under GR-001. `btr.application` and `btr.infrastructure` build. 47 Principal tests pass, including 4 new `PrincipalReturnHistoryComposerTest` tests.
- Remediation History: none

### PCM-026

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. Calculated `PRN-GRW-001` Month-over-Month Growth Percentage from stored `PRN-SALES-001` month history only. Catalog registers `PRN-GRW-001` only. The writer does not write `PRN-SALES-001` or `PRN-GRW-002` and does not use Purchase-In, returns, claims, or inventory adjustments.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. `PRN-GRW-001` equals (current month `PRN-SALES-001` − prior month `PRN-SALES-001`) ÷ prior month `PRN-SALES-001` when the prior month is greater than zero, otherwise null, computed from stored Sales-Out history only. The writer touches only `BTRPD_PrincipalMomGrowthKpi` and `BTRPD_PrincipalMomGrowth`, writes no `PRN-SALES-001` or `PRN-GRW-002`, and uses no Purchase-In, return, claim, or inventory source. Full `btr.application`, `btr.infrastructure`, and `btr.test` build via MSBuild. 83 ReportingContext Principal/refresh tests pass, including 5 new `PrincipalMomGrowthComposerTest` tests.
- Remediation History: none

### PCM-027

- Status: GO
- Implementation History:
  - 2026-09-10: IN IMPLEMENTATION
  - 2026-09-10: IMPLEMENTED. Calculated `PRN-GRW-002` Year-over-Year Growth Percentage from stored `PRN-SALES-001` month history only. Catalog registers `PRN-GRW-002` only. The writer does not write `PRN-SALES-001` or `PRN-GRW-001` and does not use Purchase-In, returns, claims, or inventory adjustments.
- Review History:
  - 2026-09-10: IN REVIEW
  - 2026-09-10: GO. `PRN-GRW-002` equals (current month `PRN-SALES-001` − same month prior year `PRN-SALES-001`) ÷ same month prior year `PRN-SALES-001` when the prior-year month is greater than zero, otherwise null, computed from stored Sales-Out history only. The writer touches only `BTRPD_PrincipalYoyGrowthKpi` and `BTRPD_PrincipalYoyGrowth`, writes no `PRN-SALES-001` or `PRN-GRW-001`, and uses no Purchase-In, return, claim, or inventory source. Full `btr.application`, `btr.infrastructure`, and `btr.test` build. 5 new `PrincipalYoyGrowthComposerTest` tests, 14 `PrincipalKpiCatalogTest` tests, and 14 refresh/orchestration tests pass.
- Remediation History: none

### PCM-028

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. Persisted `PRN-TGT-002` Achievement Amount (`PRN-SALES-001 − PRN-TGT-001` when target > 0 and sales present, otherwise null) and `PRN-TGT-003` Achievement Percentage (`PRN-SALES-001 ÷ PRN-TGT-001` when target > 0, otherwise null) from stored Sales-Out and stored Principal Target. Catalog registers `PRN-TGT-002` and `PRN-TGT-003` as versus-target, not a copy of Sales-Out, not Net Sales. The writer does not write, overwrite, or recalculate `PRN-SALES-001` or `PRN-TGT-001` and writes no return KPI.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. Achievement snapshot stores `PRN-TGT-002` and `PRN-TGT-003` from stored `PRN-SALES-001` and `PRN-TGT-001` without writing either source. `PRN-TGT-003` is null when `PRN-TGT-001` is not greater than zero. Achievement is not labeled Net Sales. `btr.application`, `btr.infrastructure`, and `btr.test` build. 32 targeted tests pass, including 5 new `PrincipalAchievementComposerTest` tests covering versus-target composition, null rules, cross-period isolation, and source-row protection.
- Remediation History: none

### PCM-029

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. Stored Principal × Salesman commercial contribution decomposed from `PRN-SALES-001` Faktur Item evidence by `Faktur.SalesPersonId` into `BTRPD_PrincipalContribution`, with missing-target responsibility exceptions in `BTRPD_PrincipalContributionException`. A sold Salesman × Principal pair with no target record for the transaction month remains in the contribution and is listed as an exception. Contribution carries only the source `PRN-SALES-001` provenance and no registry KPI ID. The writer does not write `PRN-SALES-001` or `PRN-TGT-001`.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. Contribution snapshot stores Principal × Salesman `PRN-SALES-001` decomposition by `Faktur.SalesPersonId` with `SUM(SubTotal - DiscRp)`; per-Principal sums reconcile to the Sales-Out measure with zero mismatches on `btr2`. Missing-target pairs remain in the contribution and are listed as responsibility exceptions. Contribution carries only source `PRN-SALES-001` provenance and no registry KPI ID. The writer touches only the three contribution tables. Full solution builds. 15 targeted tests pass (6 new composer tests plus orchestration tests).
- Remediation History: none

### PCM-030

- Status: GO
- Implementation History:
  - 2026-09-10: IN IMPLEMENTATION
  - 2026-09-10: IMPLEMENTED. SA04 reads stored `PRN-TGT-001` and stored `PRN-TGT-002`/`PRN-TGT-003` and shows them in a separate target and achievement panel. The displayed `PRN-SALES-001` ranking is unchanged. Principals sold without a target record remain in the ranking with a missing-target exception count. No return or Net Sales figure is shown.
- Review History:
  - 2026-09-10: IN REVIEW
  - 2026-09-10: GO. SA04 target and achievement panel reads stored `PRN-TGT-001` and stored achievement values for the matching period. Displayed `PRN-SALES-001` ranking and totals are unchanged. No return, Net Sales, or Health Score figure is shown. Missing-target principals remain in the ranking with a visible exception count and do not lose Sales-Out. `btr.application` builds, `btr.test` builds via MSBuild, portal.web `vue-tsc + vite` builds. 7 PrincipalPerformanceQueryTest, 34 related backend, and 251 frontend tests pass.
- Remediation History: none

### PCM-031

- Status: GO
- Implementation History:
  - 2026-09-10: IN IMPLEMENTATION
  - 2026-09-10: IMPLEMENTED. SA04 reads stored `PRN-RET-001`, `PRN-RET-002`, `PRN-RET-003`, and `PRN-RET-004` for the matching period into a separate returns panel with Return Item drill-down. Displayed `PRN-SALES-001` ranking and totals are unchanged. Return Percentage is labeled as a quality ratio, not Net Sales.
- Review History:
  - 2026-09-10: IN REVIEW
  - 2026-09-10: GO. SA04 returns panel reads stored return snapshots for the matching period only and leaves the `PRN-SALES-001` ranking and totals unchanged. Return Percentage is labeled as a quality ratio, not a deduction and not Net Sales. Return Item drill-down opens Return Item evidence at Return Item grain. `btr.application`, `btr.infrastructure`, and `btr.test` build via MSBuild. 71 ReportingContext Principal tests pass, including 4 new `PrincipalPerformanceQueryTest` returns tests. Portal.web `vite` builds. 252 frontend tests pass, including the new return-evidence route test.
- Remediation History: none

### PCM-032

- Status: IMPLEMENTED
- Implementation History:
  - 2026-09-10: IN IMPLEMENTATION
  - 2026-09-10: IMPLEMENTED. Added SA04 growth panel showing stored `PRN-GRW-001` (Month-over-Month Growth %) and `PRN-GRW-002` (Year-over-Year Growth %) from Sales-Out growth KPI snapshots. Backend injects `IPrincipalMomGrowthSnapshotDal` and `IPrincipalYoyGrowthSnapshotDal` into the performance query handler and composes growth onto the response and ranking items. Frontend adds a dedicated growth section with two metric cards and a ranking table. Growth is from stored Sales-Out history only and does not change `PRN-SALES-001`. 4 new tests verify growth attachment, period matching, and Sales-Out immutability. All 18 PrincipalPerformanceQueryTest tests pass. Frontend type-checks and all 258 frontend tests pass.
- Review History: none
- Remediation History: none

### PCM-033

- Status: GO
- Implementation History:
  - 2026-09-10: IN IMPLEMENTATION
  - 2026-09-10: IMPLEMENTED. Added SA04 supporting ranking controls. Backend exposes `SupportingRankingOptions` listing available supporting KPIs (`PRN-RET-004`, `PRN-TGT-003`, `PRN-GRW-001`, `PRN-GRW-002`) based on data availability. Frontend adds a ranking selector dropdown defaulting to `PRN-SALES-001`. When a supporting KPI is selected, the ranking table re-sorts client-side by that KPI's value with recomputed rank numbers. Default ranking and stored Sales-Out values are unchanged. Purchase-In, Inventory, Coverage, and Net Sales are not offered. 3 new backend tests verify supporting ranking option composition, exclusion of unavailable KPIs, and exclusion of non-ranking KPIs. All 21 `PrincipalPerformanceQueryTest` tests pass. Frontend type-checks and all 258 frontend tests pass.
- Review History:
  - 2026-09-10: IN REVIEW
  - 2026-09-10: GO. Default ranking remains `PRN-SALES-001`. No supporting ranking replaces the stored Sales-Out value. Purchase-In, Inventory, Coverage, and Net Sales are not offered as ranking options. Only the four approved supporting ranking KPIs (`PRN-RET-004`, `PRN-TGT-003`, `PRN-GRW-001`, `PRN-GRW-002`) are available when data exists. `btr.application` builds, `btr.test` builds via MSBuild, portal.web `vue-tsc + vite` builds. 21 `PrincipalPerformanceQueryTest` tests pass including 3 new supporting ranking tests. 258 frontend tests pass.
- Remediation History: none

### PCM-034

- Status: GO
- Implementation History:
  - 2026-09-10: IN IMPLEMENTATION
  - 2026-09-10: IMPLEMENTED. SA04 reads the stored Principal × Salesman contribution snapshot for the matching period into a separate Salesman contribution panel within the selected Principal. Each contributing Salesman is shown with its stored `PRN-SALES-001` decomposition amount. Contribution is labeled as contribution, not a registry ranking KPI, and does not assign Customer ownership. Displayed `PRN-SALES-001` ranking and totals are unchanged.
- Review History:
  - 2026-09-10: IN REVIEW
  - 2026-09-10: GO. SA04 contribution panel reads the stored PCM-029 snapshot for the matching period only and leaves the `PRN-SALES-001` ranking and totals unchanged. Each contributing Salesman of the selected Principal is shown with its stored decomposition amount. Contribution is labeled as contribution, not a registry ranking KPI, carries only `PRN-SALES-001` provenance, and disclaims Customer ownership. `btr.application` builds, `btr.test` builds via MSBuild, portal.web `vue-tsc + vite` builds. 14 PrincipalPerformanceQueryTest tests pass, including 3 new contribution tests. 252 frontend tests pass.
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

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. CU02 labels the latest current-month invoice Salesman as Last Invoicing Salesman, a recency indicator, not the Customer owner. The low-recovery explanation uses invoice attribution, not Assigned Salesman. Customer risk, credit, piutang, and decline measures remain Customer-level. Principal-specific decline was not added. Other Customer pages are unchanged.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. CU02 does not present one Salesman as the Customer owner. The column, recency note, and disclosure state that the displayed Salesman is the last invoicing Salesman on the current-month invoice, not Assigned Salesman or Owner. The low-recovery explanation uses invoice attribution. Customer risk, credit, piutang, and decline measures remain Customer-level. Principal-specific decline was not added. Other Customer pages are unchanged. `customerAnalyticsAttribution.spec.ts`: 4 passed. `CustomerRiskSignalBuilderTest`: 6 passed.
- Remediation History: none

### PCM-039

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. CU03 labels Salesman routing as operational Action Route and Routed Salesman, not Customer ownership. Collection queues remain Customer-level. No Principal collection impact was added. CU01, CU02, CU04, and CU05 are unchanged.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. CU03 describes Salesman routing as operational Action Route and Routed Salesman, not Customer ownership. Collection queues remain Customer-level. No Principal collection impact was added. CU01, CU02, CU04, and CU05 are unchanged. `collectionOptimizationRouting.spec.ts`: 3 passed.
- Remediation History: none

### PCM-040

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. CU04 labels the Salesman filter and displayed Salesman as Last Invoicing Salesman, a commercial attribution on the latest invoice, not the Customer owner. Customer portfolio measures remain Customer-level. Principal portfolio mix was not added. Other Customer pages are unchanged.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. CU04 does not describe the latest-Faktur Salesman as the Customer owner. The filter and displayed Salesman use Last Invoicing Salesman, a commercial attribution on the latest invoice. Customer portfolio measures remain Customer-level. Principal portfolio mix was not added. Other Customer pages are unchanged. `customerAnalyticsAttribution.spec.ts`: 6 passed. `customerPortfolioSignals.spec.ts`: 8 passed.
- Remediation History: none

### PCM-041

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. CU05 labels the action function as Action Route and the latest-invoice Salesman as Last Invoicing Salesman, a commercial attribution, not the Customer owner. Customer totals remain Customer-level. Pair evidence was not added. Other Customer pages are unchanged.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. CU05 does not label a Salesman column as Owner of the Customer. The action function is Action Route and the latest-invoice Salesman is Last Invoicing Salesman, a commercial attribution. Customer totals remain Customer-level. Pair evidence was not added. Other Customer pages are unchanged. `customerAnalyticsAttribution.spec.ts`: 8 passed.
- Remediation History: none

### PCM-042

- Status: GO
- Implementation History:
  - 2026-09-10: IN IMPLEMENTATION
  - 2026-09-10: IMPLEMENTED. Counted `PRN-CUS-001` Active Customer Count from stored `BTRPD_CustomerPrincipalRelationship` rows whose stored status is Active into `BTRPD_PrincipalActiveCustomer`. Catalog registers `PRN-CUS-001` only. The writer reads the stored projection, does not scan raw transactions, does not write projection status, does not write `PRN-SALES-001`, and renders no dashboard panel.
- Review History:
  - 2026-09-10: IN REVIEW
  - 2026-09-10: GO. `PRN-CUS-001` counts stored Active rows on the projection only. Dormant rows are excluded from the count and retained. The writer touches only `BTRPD_PrincipalActiveCustomerKpi` and `BTRPD_PrincipalActiveCustomer`, writes no projection status and no `PRN-SALES-001`, and renders no dashboard panel. Catalog registers `PRN-CUS-001` only. `btr.application`, `btr.infrastructure`, `btr.test` (MSBuild), and `btr.portal.worker` build. 29 targeted tests pass, including 4 new composer tests and the new catalog test. 10 broader ReportingContext failures are pre-existing on baseline.
- Remediation History: none

### PCM-043

- Status: IMPLEMENTED
- Implementation History:
  - 2026-09-10: IN IMPLEMENTATION
  - 2026-09-10: IMPLEMENTED. CU02 shows Principal decline or inactivity from stored `BTRPD_CustomerPrincipalRelationship` rows only. Pair status and pair-attributed `PRN-SALES-001` are read for the at-risk customers on the page. Customer totals, latest-Faktur Salesman, and raw transaction scans are not used. One Customer can show Active and Dormant pairs at the same time. Other Customer pages are unchanged.
- Review History:
  - 2026-09-10: IN REVIEW
  - 2026-09-10: GO. CU02 Principal decline reads the relationship projection only and preserves per-pair Active/Dormant status with pair-attributed `PRN-SALES-001`. Customer totals and latest-Faktur Salesman are unchanged. Other Customer pages are unchanged. `btr.application`, `btr.infrastructure`, `btr.test` build via MSBuild; portal.web `vue-tsc + vite` builds. 5 new `CustomerRiskForecastPrincipalDeclineTest` tests pass; 54 targeted backend and 253 frontend tests pass. Full-suite failures are pre-existing on baseline.
- Remediation History: none

### PCM-044

- Status: GO
- Implementation History:
  - 2026-09-10: IN IMPLEMENTATION
  - 2026-09-10: IMPLEMENTED. CU04 shows portfolio mix from stored `BTRPD_CustomerPrincipalRelationship` rows only for priority-queue customers. Pair status and pair-attributed `PRN-SALES-001` are read from the projection. Customer portfolio measures remain Customer-level and are not allocated to Principals. No pre-purchase assigned Principal is shown. Other Customer pages are unchanged.
- Review History:
  - 2026-09-10: IN REVIEW
  - 2026-09-10: GO. CU04 portfolio mix reads the relationship projection only via `ListPairsForCustomerCodes` with stored status and pair-attributed `PRN-SALES-001`. The mix lists only Principals on each Customer's projection with no pre-purchase assigned Principal and no raw-transaction recomputation. Customer portfolio measures remain Customer-level. Other Customer pages are unchanged. `btr.application` builds, `btr.test` builds via MSBuild, portal.web `vue-tsc + vite` builds. 5 new `CustomerPortfolioPrincipalMixTest` tests pass; 11 related backend and 254 frontend tests pass.
- Remediation History: none

### PCM-045

- Status: GO
- Implementation History:
  - 2026-09-10: IN IMPLEMENTATION
  - 2026-09-10: IMPLEMENTED. CU05 shows pair evidence from stored `BTRPD_CustomerPrincipalRelationship` rows only for report customers. Pair status and pair-attributed `PRN-SALES-001` are read from the projection via `ListPairsForCustomerCodes`. Customer totals, latest-Faktur Salesman, and raw transaction scans are not used. No collection queue is routed by Principal financial exposure. Other Customer pages are unchanged.
- Review History:
  - 2026-09-10: IN REVIEW
  - 2026-09-10: GO. CU05 keeps Action Route and Last Invoicing Salesman labels with no Owner column and adds pair evidence reading stored status and pair-attributed `PRN-SALES-001` from the projection only. No collection queue is routed by Principal financial exposure. Other Customer pages are unchanged. `btr.application` and `btr.test` build via MSBuild; portal.web `vue-tsc -b` passes. 5 new `CustomerReportPrincipalPairTest` tests pass; 19 related backend and 255 frontend tests pass.
- Remediation History: none

### PCM-046

- Status: IMPLEMENTED
- Implementation History:
  - 2026-09-10: IN IMPLEMENTATION
  - 2026-09-10: IMPLEMENTED. Composed stored `PRN-GRW-001` and `PRN-GRW-002` onto the Supplier/Principal Entity Analytics profile from the Sales-Out growth KPI snapshots. The existing `SupplierEntityAnalyticsProducer` injects `IPrincipalMomGrowthSnapshotDal` and `IPrincipalYoyGrowthSnapshotDal`, reads the stored growth snapshots, and writes growth KPIs as L0 rows. The Growth radar axis now sources from `PRN-GRW-001` instead of `PU-KPI-001`. Registrar adds both growth KPIs to the pack and metadata with Growth category. The producer does not write, overwrite, or recalculate `PRN-SALES-001`. No return, target, purchase, or inventory pack was added.
- Review History: none
- Remediation History: none

### PCM-047

- Status: GO
- Implementation History:
  - 2026-09-10: IN IMPLEMENTATION
  - 2026-09-10: IMPLEMENTED. Composed stored `PRN-RET-001`, `PRN-RET-002`, `PRN-RET-003`, and `PRN-RET-004` onto the Supplier/Principal Entity Analytics profile from the Returns-owned snapshots. The existing `SupplierEntityAnalyticsProducer` reads the stored return snapshots and does not write, overwrite, or recalculate `PRN-SALES-001`. `PRN-RET-004` is null or absent when stored Sales-Out is not greater than zero. Return evidence opens Return Item evidence. No target, growth, purchase, inventory, or coverage pack was added.
- Review History:
  - 2026-09-10: IN REVIEW
  - 2026-09-10: GO. Return values equal the stored return snapshots. `PRN-RET-004` is a quality supporting ranking indicator only and is null or absent when stored Sales-Out is not greater than zero. The producer does not write or replace `PRN-SALES-001`. Return evidence opens Return Item evidence. `btr.application` builds, `btr.test` builds via MSBuild, 67 related tests pass including 5 new `SupplierPrincipalReturnCompositionTest` tests.
- Remediation History: none

### PCM-048

- Status: GO
- Implementation History:
  - 2026-09-10: IN IMPLEMENTATION
  - 2026-09-10: IMPLEMENTED. Composed stored `PRN-TGT-001`, `PRN-TGT-002`, and `PRN-TGT-003` onto the Supplier/Principal Entity Analytics profile from the Target-owned and Achievement-owned snapshots. The existing `SupplierEntityAnalyticsProducer` reads the stored target and achievement snapshots and does not write, overwrite, or recalculate `PRN-SALES-001`. No return, growth, purchase, inventory, or coverage pack was added.
- Review History:
  - 2026-09-10: IN REVIEW
  - 2026-09-10: GO. Target pack reads stored `PRN-TGT-001` and stored `PRN-TGT-002`/`PRN-TGT-003` for the matching period and does not replace `PRN-SALES-001` with achievement or a net-of-returns amount. No return, growth, purchase, or inventory pack was added. `btr.application` and `btr.test` build via MSBuild. 5 new `SupplierPrincipalTargetCompositionTest` tests and 38 related tests pass; full ReportingContext failures are pre-existing on baseline.
- Remediation History: none

### PCM-049

- Status: GO
- Implementation History:
  - 2026-09-10: IN IMPLEMENTATION
  - 2026-09-10: IMPLEMENTED. Aligned Customer top-Principal sales relationship metadata to `PRN-SALES-001` and sourced the Customer Entity Analytics top-Principal relationship list from the `BTRPD_CustomerPrincipalRelationship` projection. `CustomerRelationshipCatalog.TopPrincipalsByOmzet.MetricKpiId` is `PrincipalKpiCatalog.SalesOutId` (`PRN-SALES-001`) via `SalesOmzetMetricKpiId`, with an `IsSalesOmzetRelationship` helper. `RefreshDashboardCustomerSnapshotWorker` reads `ICustomerPrincipalRelationshipDal.GetProjection()` and passes it through `CustomerEntityAnalyticsProduceInput.RelationshipProjection`; `CustomerEntityAnalyticsProducer.BuildRelationshipSnapshots` builds `TopPrincipalsByOmzet` rows from the stored projection pairs (pair-attributed `SalesOutAmount`), not the raw MTD line rollup. Replay/backfill (which has no projection) simply writes no principal relationship rows rather than fabricating attributions. `TopItemsByOmzet` remains `CU-KPI-009`. Supplier omzet metadata is unchanged (PCM-016). `btr.application` and `btr.test` build via MSBuild; related relationship and producer tests pass.
- Review History:
  - 2026-09-10: IN REVIEW
  - 2026-09-10: GO. Acceptance criteria verified: the Customer top-Principal sales relationship metric is `PRN-SALES-001` (block `MetricKpiId` from `TopPrincipalsByOmzet` metadata, values copied from stored projection pairs); Active/Dormant interpretation, if shown, would be read from projection rows only (not surfaced in this slice; presentation remains PCM-055); the list is not the evidence grain for `PRN-CUS-001`/`PRN-CUS-002` (no evidence links, no PRN-CUS writes); labels remain "Top Principals" with no assigned-ownership wording; Supplier omzet metadata is untouched. Scope verified: `TopItemsByOmzet` stays `CU-KPI-009`; no Supplier relationship, aggregator, evidence, or projection changes. Authority verified: FEASIBILITY ASSESSMENT (GR-002 Entity Analytics consumes the projection, no raw-transaction recompute) and the Principal KPI Registry. Builds verified: `btr.application` via `dotnet`, `btr.test` via VS MSBuild. Tests: updated `SupplierPrincipalOmzetRelationshipMetadataTest` asserts Customer top-Principal = `PRN-SALES-001` (not CU-KPI-009/PU-KPI-001/SF-KPI-008/PRN-CUS-*/PR-KPI-*/CP-KPI-*); `CustomerEntityAnalyticsProducerTest` feeds a projection and asserts projection-sourced `TopPrincipalsByOmzet` rows plus a new no-projection skip test; 38 related tests pass. Three pre-existing ReportingContext failures (`CustomerDefaultPack_RegistersCatalogKpiIds`, `EntityAnalyticsKpiRegistryTest.CustomerDefaultPack_ContainsCatalogBackedKpiIds`, `EntityPerformanceProfileComposerTest.Build_NoSnapshotData_ReturnsEmptySafeProfile`) are confirmed on baseline and unrelated to this slice. INFO notes: backfill/replay produces no Customer top-Principal rows because no historical projection exists (out of PCM-049 scope; revisit in PCM-055); relationship `MetricValue` now reflects stored pair-attributed `PRN-SALES-001` rather than the MTD LineTotal rollup; `DashboardCustomerRelationshipAggregator.BuildTopPrincipals` is now redundant but retained (still directly tested) and its removal is not this slice.
- Remediation History: none

### PCM-050

- Status: IMPLEMENTED
- Implementation History:
  - 2026-09-10: IN IMPLEMENTATION
  - 2026-09-10: IMPLEMENTED. Added Principal Sales-Out alerts to EX02 (Alert Center). Backend: `AlertCenterRegistry` registers `SignalPrincipalSalesOut` in Sales category routing to `/dashboard/principal-performance`; `DashboardAlertCenterComposer` gains `BuildPrincipalSalesAlerts` reading `PrincipalSalesOutAggregateResult` via `PrincipalPerformanceComposer.Compose`, emitting top-5 Principal alerts with `EntityType=Principal`, `KpiId=PRN-SALES-001`, `DashboardRoute=/dashboard/principal-performance`; `AlertCenterComposeInput` gains `PrincipalSalesOut` property; `DashboardAlertCenterDal` injects `IPrincipalSalesOutSnapshotDal` and loads the snapshot; `DashboardAlertCenterNavigationLinks` gains `PrincipalPerformanceDashboardRoute`; `InvestigationRegistry` registers the signal for investigation metadata. Frontend: `DashboardAlertCenterNavigationLinks` TypeScript interface gains `PrincipalPerformanceDashboardRoute`. Tests: 6 new tests covering alert creation routed to SA04, no return/collection/credit alerts, coexistence with Salesman alerts, null/wrong-KpiId guards, and navigation route. `btr.application` and `btr.infrastructure` build successfully. No return, collection, credit, or financial alerts added. EX01 unchanged.
- Review History:
  - 2026-09-10: IN REVIEW
- Remediation History: none

### PCM-051

- Status: IMPLEMENTED
- Implementation History:
  - 2026-09-10: IN IMPLEMENTATION
  - 2026-09-10: IMPLEMENTED. Added a SupplierId (`VARCHAR(5) NOT NULL DEFAULT('')`) column to `BTRPD_InventoryBreakdown` and `BTRPD_InventoryRiskBreakdown` via a new idempotent upgrade script (`Upgrade_PCM051_InventoryCrossLink.sql`) and updated the canonical table definition files and `btr.sql.sqlproj`. Backend carries the supplier id through `DashboardInventoryItemGroup` and both snapshot aggregators (grouping and measures unchanged) into the snapshot DALs (read/write/map). IN01 `TopSuppliers`, `SupplierBreakdown`, and IN02 `SupplierRiskExposure` now expose `SupplierId` plus `DashboardRoute = /dashboard/principal-performance` on supplier rows (live DAL mirrored for parity; category rows expose neither). Frontend: `navigateToPrincipalPerformance.ts` service navigates to SA04 with the trimmed `supplierId` query; IN01 Top-10 Supplier rows prefer the `DashboardRoute` navigate-to-SA04 branch (falling back to the existing supplier investigation) and the "Inventory by Supplier" chart is clickable; IN02 "Supplier Risk Exposure" chart is clickable. `InventoryHorizontalBarChart` gained an opt-in `clickable` prop + `bar-click` emit (no other call sites). Inventory measures are unchanged; `PRN-SALES-001` and `PRN-INV-*` packs are not written by this slice.
- Review History:
  - 2026-09-10: IN REVIEW
  - 2026-09-10: GO. Acceptance criteria verified individually: IN01 and IN02 remain inventory measures (aggregators only add SupplierId; grouping and value computations unchanged; no KPI/pack code touched); the navigation action opens SA04 (`/dashboard/principal-performance` with `supplierId` query, which SA04 reads to select the Principal) from IN01 Top-10 Suppliers rows and the IN01 Supplier chart and IN02 Supplier Risk Exposure chart; IN03, IN04, IN05 are untouched (the only shared component change is an opt-in `clickable` prop on `InventoryHorizontalBarChart` used solely by the IN01/IN02 supplier charts); nothing is written to `PRN-SALES-001`; no `PRN-INV-001`/`PRN-INV-002` publish. Supplied evidence: `btr.application`, `btr.infrastructure`, and `btr.test` build via VS MSBuild; 6 new backend tests (aggregator SupplierId carry, category empty SupplierId/no-route, IN01 ranking and IN02 risk navigation, snapshot↔live parity) pass with their suites; full-suite runs on this build and on a clean worktree at HEAD 477a4eee both produce an identical 39-test failure set (zero new failures; the failures are DB-connectivity/environmental plus pre-existing ReportingContext and helper cases); frontend `vue-tsc` + `vite build` is clean and all 258 vitest tests pass, including the 3 new `navigateToPrincipalPerformance` specs. Findings: Minor (legacy snapshot rows with blank SupplierId still expose a DashboardRoute, so a click no-ops instead of falling back to the supplier investigation until the snapshot is refreshed) and 2 observations (supplier Top-10 click target intentionally switches from investigation to SA04 per the approved deliverable; the SA04 route constant is duplicated across the backend and frontend layers and verified equal). No required actions.
- Remediation History: none

### PCM-052

- Status: GO
- Implementation History:
  - 2026-09-10: IN IMPLEMENTATION
  - 2026-09-10: IMPLEMENTED. Composed stored `PRN-PUR-001` onto the Supplier/Principal Entity Analytics profile from the Purchasing-owned snapshot. The existing `SupplierEntityAnalyticsProducer` injects `IPrincipalPurchaseInSnapshotDal`, reads the stored purchase-in snapshot, and writes the purchase-in KPI as L0 rows labeled Purchase-In. The producer does not write, overwrite, or recalculate `PRN-SALES-001`, `PRN-GRW-001`, or `PRN-GRW-002`. A purchase refresh retains persisted `PRN-SALES-001`. No return, target, growth, inventory, or coverage pack was added.
- Review History:
  - 2026-09-10: IN REVIEW
  - 2026-09-10: GO. `PRN-PUR-001` is present on the Principal profile and labeled Purchase-In. It is not used as the Principal performance KPI or as a Principal ranking KPI. It does not change `PRN-SALES-001`, `PRN-GRW-001`, or `PRN-GRW-002`. No purchase refresh removes `PRN-SALES-001` from the profile. `btr.application` builds. 5 new `SupplierPrincipalPurchaseInCompositionTest` tests and 77 related tests pass; 2 remaining failures (`CustomerDefaultPack_ContainsCatalogBackedKpiIds`, `Produce_WritesL4RelationshipRows`) and the `DashboardAlertCenterComposerTest` compile errors are pre-existing on baseline from committed PCM-049/PCM-050/PCM-055 work and are unrelated to this slice.
- Review History: none
- Remediation History: none

### PCM-053

- Status: GO
- Implementation History:
  - 2026-09-10: IN IMPLEMENTATION
  - 2026-09-10: IMPLEMENTED. Composed stored `PRN-INV-001` and `PRN-INV-002` onto the Supplier/Principal Entity Analytics profile from the Inventory-owned snapshot. The existing `SupplierEntityAnalyticsProducer` injects `IPrincipalInventorySnapshotDal`, reads the stored inventory snapshot, and writes both inventory KPIs as L0 rows labeled as operational indicators. The producer does not write, overwrite, or recalculate `PRN-SALES-001`. Neither inventory KPI is a performance ranking KPI. No return, target, growth, purchase, or coverage pack was added.
- Review History:
  - 2026-09-10: IN REVIEW
  - 2026-09-10: GO. Profile shows stored `PRN-INV-001`/`PRN-INV-002` as operational indicators with no `PRN-SALES-001` modification and no performance ranking. Ranking remains `PRN-SALES-001`. `btr.application` builds. 5 new `SupplierPrincipalInventoryCompositionTest` tests and 62 related tests pass; 1 remaining failure (`Produce_WritesL4RelationshipRows`) and the `DashboardAlertCenterComposerTest` compile errors are pre-existing on baseline and unrelated to this slice.
- Remediation History: none

### PCM-054

- Status: GO
- Implementation History:
  - 2026-09-10: IN IMPLEMENTATION
  - 2026-09-10: IMPLEMENTED. SA04 reads stored `PRN-CUS-001` and stored `PRN-CUS-002` into a separate customer-reach panel. The existing `GetPrincipalPerformanceHandler` injects `IPrincipalActiveCustomerSnapshotDal` and `IPrincipalCustomerCoverageSnapshotDal` and composes both stored snapshots onto the response and ranking items by SupplierId. Frontend adds a dedicated customer-reach section with a ranking table and states that the evidence grain is the Customer–Principal relationship projection. Displayed `PRN-SALES-001` ranking and totals are unchanged. Coverage is not offered as a ranking option.
- Review History:
  - 2026-09-10: IN REVIEW
  - 2026-09-10: GO. SA04 customer-reach panel reads stored `PRN-CUS-001` and stored `PRN-CUS-002` for ranked Principals only and leaves the `PRN-SALES-001` ranking and totals unchanged. No Active, Dormant, or Coverage value is recomputed from raw transactions. The panel states that the evidence grain is the Customer–Principal relationship projection. Coverage is not offered as a ranking option. `btr.application`, `btr.infrastructure`, and `btr.test` build via MSBuild; portal.web `vue-tsc + vite` builds. 26 `PrincipalPerformanceQueryTest` tests pass, including 5 new customer-reach tests; 37 related backend and 258 frontend tests pass.
- Remediation History: none

### PCM-055

- Status: IN REVIEW
- Implementation History:
  - 2026-09-10: IN IMPLEMENTATION
  - 2026-09-10: IMPLEMENTED. Made Entity Analytics relationship presentation consume `BTRPD_CustomerPrincipalRelationship` for both Supplier and Customer profiles. Supplier `TopCustomersByOmzet` now sources from the projection (pair-attributed `PRN-SALES-001`, status, last transaction date) instead of MTD rollup. Customer `TopPrincipalsByOmzet` now carries projection status and last transaction date. Added `RelationshipStatus` and `LastTransactionDate` to the relationship pipeline: `EntityRelationshipSnapshot` → `EntityAnalyticsRelationshipRow` → `BTRPD_EntityAnalytics_Relationship` table → `ProfileRelatedEntityRowDto` → frontend `ProfileRelatedEntityRow`. Frontend displays Active/Dormant status badge and last transaction date when available. Files changed: upgrade script `Upgrade_PCM055_EntityAnalyticsRelationshipStatus.sql`; `EntityRelationshipSnapshot.cs`; `EntityAnalyticsRelationshipRow.cs`; `EntityAnalyticsRepository.cs` (SELECT/INSERT for new columns); `GetEntityPerformanceProfileQuery.cs` (DTO); `EntityRelationshipEngine.cs` (persist + map); `SupplierEntityAnalyticsProduceInput.cs` (projection input); `SupplierEntityAnalyticsProducer.cs` (projection-sourced `TopCustomersByOmzet` + `BuildSupplierPairIndex`); `CustomerEntityAnalyticsProducer.cs` (carry status/date); `RefreshDashboardPurchasingManagementSnapshotWorker.cs` (inject + load projection); `entityAnalytics.ts` (model); `RelatedEntitiesBlocks.vue` (status + date display).
- Review History:
- Remediation History: none

### PCM-056

- Status: GO
- Implementation History:
  - 2026-09-09: IN IMPLEMENTATION
  - 2026-09-09: IMPLEMENTED. Updated `docs/features/btr-portal/navigation-assets.md` so the implemented sidebar codes are the single authoritative code list. SA04 Principal Performance is documented under Sales after SA01 and before SA02. Older conflicting reservations of EX03, SF03, and SF04 are recorded as not used for new assignment. Menu code implementation was not changed.
- Review History:
  - 2026-09-09: IN REVIEW
  - 2026-09-09: GO. The navigation asset registry names the implemented sidebar codes as the single authoritative code list. Older conflicting reservations of EX03, SF03, and SF04 are recorded as not used for new assignment. SA04 Principal Performance is documented under Sales after SA01 and before SA02. Menu code implementation was not changed.
- Remediation History: none

### PCM-057

- Status: GO
- Implementation History:
  - 2026-09-10: IN IMPLEMENTATION
  - 2026-09-10: IMPLEMENTED. Synchronized portal domain, architecture, and Entity Analytics guidance to the implemented model and guardrails. `btr-portal-domain.md` §11.7 records PRN-SALES-001 independent of Returns, projection consumption for Coverage/Active/Dormant/Relationship/Entity Analytics, ranking hierarchy, and explicit non-goals. `btr-portal-architecture.md` records GR-001/GR-002, snapshot ownership with single-writer SupplierEntityAnalyticsProducer, refresh order, ranking hierarchy, and SA04/no-authorization rules. `entity-analytics-developer-guide.md` Supplier pack updated to separate PRN-* KPI packs, ranking hierarchy, owned-snapshot reads, projection-consumed relationships, and Faktur/Return Item evidence routes. `docs/foundation/DOMAIN.md` and `docs/foundation/WORKFLOW.md` unchanged (no workflow change; relationship is an analytics projection, not core vocabulary).
- Review History:
  - 2026-09-10: IN REVIEW
  - 2026-09-10: GO. All acceptance criteria verified against registry and implemented code; docs-only scope with no forbidden claims; WORKFLOW/DOMAIN foundation untouched as permitted.
- Remediation History: none

### PCM-058

- Status: GO
- Implementation History:
  - 2026-09-10: IN IMPLEMENTATION
  - 2026-09-10: IMPLEMENTED. Synchronized dashboard feature artifacts and question navigation to implemented surfaces only. Created dashboard-sa04-principal-performance.md (PRN-SALES-001 ranking, target/achievement, returns, customer-reach panels with GR-001 and PD-010). Added implemented-only addenda to SA01, SA02, CU01, CU02, CU04, SF01, PU01, IN01, IN02. Fixed CU04 stale no-mix line. Reworded MQ-003 in business-question-catalog-v3.md and question-navigation-map.md to invoice-attributed/last-invoice language. Added PQ-001–PQ-008 addenda routing Available questions to SA04/SA01/SA02/SA03/CU01/CU02/CU04/CU05/EX01/EX02/IN01/IN02 and marking PQ-004 and PQ-006 Not yet available. Returns described as independent KPIs, never a reduction of PRN-SALES-001. KPI catalog untouched.
- Review History:
  - 2026-09-10: IN REVIEW
  - 2026-09-10: GO. Acceptance criteria verified individually: implemented feasibility §4.3 questions route to SA04/SA01/SA02/SA03/CU01/CU02/CU04/CU05/EX01/EX02/IN01/IN02 while PQ-004 and PQ-006 are marked Not yet available; MQ-003 and new question text use invoice-attributed/last-invoice/contribution language with no Salesman-as-Customer-owner description; Returns are independent KPIs, never a reduction of PRN-SALES-001; KPI catalog untouched. Scope verified: docs-only, SA04 + SA01/SA02/CU01/CU02/CU04/SF01/PU01/IN01/IN02 addenda plus v3 catalog and navigation map; no code, catalog, navigation, domain, or architecture changes. Authority verified: FEASIBILITY ASSESSMENT with PRINCIPAL KPI REGISTRY; GR-001/GR-002/GR-003 and PD-010 preserved; no new KPI IDs, Net Sales, or Health Score. Docs-only slice: no build or test run required. INFO: PCM-050 EX02 alerts are IMPLEMENTED (IN REVIEW) rather than GO; documenting the existing SA04-routed alert path as implemented code is consistent with the slice's implemented-surfaces rule and does not block GO.
- Review History: none
- Remediation History: none
