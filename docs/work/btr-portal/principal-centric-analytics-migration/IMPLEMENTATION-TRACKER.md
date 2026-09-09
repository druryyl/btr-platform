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

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-012

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-013

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-014

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-015

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-016

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-017

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-018

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-019

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-020

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-021

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-022

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-023

- Status: PLANNED
- Implementation History: none
- Review History: none
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

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-036

- Status: PLANNED
- Implementation History: none
- Review History: none
- Remediation History: none

### PCM-037

- Status: PLANNED
- Implementation History: none
- Review History: none
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
