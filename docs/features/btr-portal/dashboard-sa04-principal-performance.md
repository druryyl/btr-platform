# SA04 — Principal Performance Dashboard

## Scope

**SA04 — Principal Performance** at `/dashboard/principal-performance` is the
primary Principal commercial performance path under Sales (PCM-001, PCM-007).

It answers:

> Which Principals drive company sales, and what are their target, return,
> and customer-reach positions?

The authoritative Principal performance and ranking KPI is `PRN-SALES-001`
Principal Sales-Out (DPP) from Faktur Item evidence:

```text
PRN-SALES-001 = SUM(FakturItem.SubTotal - FakturItem.DiscRp)
```

Evidence grain is Faktur Item. Principal is attributed through
`FakturItem.BrgId → BTR_Brg.SupplierId` (current Item master, treated as
immutable for analytics). Only non-void Fakturs are included. Tax
(`PpnRp`), `FakturItem.Total`, `FakturItem.DppRp`, and `Faktur.GrandTotal`
are not the Principal measure. Header tax, freight, rounding, and other
header adjustments are not allocated to Principals.

## Return semantic protection (GR-001)

`PRN-SALES-001` is independent of Returns. Returns, Claims, and Inventory
Adjustments are not deducted from Principal Sales-Out and do not redefine
it. Returns are independent KPIs (`PRN-RET-001` through `PRN-RET-004`) shown
on a separate SA04 panel. `PRN-RET-004` Return Percentage is a quality ratio,
not a deduction and not Net Sales. No Net Sales KPI is defined in V1.

## Disclosure (PD-010)

SA04 states that the measure is Principal Sales-Out (DPP) from Faktur Item;
Returns, Claims, and Inventory Adjustments are not deducted; Returns are
independent KPIs; tax and header totals are excluded; totals are not
required to reconcile to Faktur `GrandTotal`; Item Principal comes from the
current Item master; unknown Principal and missing monthly target
responsibility are visible exceptions, not silent drops of Principal
Sales-Out.

## Sections implemented in this slice family

- **Principal Sales-Out ranking.** Default ranking is `PRN-SALES-001` only.
  Drill-down opens Faktur Item evidence for the selected Principal and
  period. Unknown-Principal exception count is read from the Sales-Out
  data-quality output (PCM-007).
- **Target and achievement panel.** Reads stored `PRN-TGT-001`,
  `PRN-TGT-002`, and `PRN-TGT-003` for the matching period. It does not
  change the displayed `PRN-SALES-001` amount. Missing-target exceptions
  remain visible and do not remove Sales-Out (PCM-030).
- **Returns panel.** Reads stored `PRN-RET-001`, `PRN-RET-002`,
  `PRN-RET-003`, and `PRN-RET-004` for the matching period. Return
  Percentage is labeled as a quality ratio. Drill-down opens Return Item
  evidence at Return Item grain. The panel does not add return amounts to,
  or subtract them from, Sales-Out (PCM-031).
- **Customer-reach panel.** Reads stored `PRN-CUS-001` Active Customer Count
  and `PRN-CUS-002` Customer Coverage Percentage. It states that the
  evidence grain is the Customer–Principal relationship projection
  (`BTRPD_CustomerPrincipalRelationship`). It does not recompute Active,
  Dormant, or Coverage from raw transactions and does not change
  `PRN-SALES-001` (PCM-054).

## Out of scope for this artifact

SA04 growth display, supporting ranking controls, and Salesman contribution
decomposition are owned by their own slices and are not described here.
Company header `GrandTotal` totals remain on SA01 and are not the Principal
measure. Purchase-In (`PRN-PUR-001`) and inventory indicators
(`PRN-INV-001`, `PRN-INV-002`) are separate KPI families and are not
Principal performance measures. No Principal financial, collection, or
credit KPIs are shown. No Principal Health Score is shown.

## Related surfaces

- **SA01** shows Principal contribution with `PRN-SALES-001` and
  `PRN-TGT-001`, ranked by `PRN-SALES-001`, with navigation to SA04.
- **SA02** shows a Principal forecast presentation from `PRN-SALES-001`
  history compared with `PRN-TGT-001`. It is not a registry KPI and is not
  a ranking KPI.
- **SA03 / Sales Report** provides Faktur Item evidence for `PRN-SALES-001`.
  Header `GrandTotal` is not allocated across Principals.
- **EX01 / EX02** route Principal sales attention and Principal sales alerts
  to SA04.
- **IN01 / IN02** supplier rows navigate to SA04 for the same Principal.
  Inventory measures are unchanged.
