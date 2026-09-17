# IMPLEMENTATION PLAN CHANGE LOG

## Principal KPI Registry alignment

| Field | Value |
| --- | --- |
| Date | 2026-09-09 |
| Registry | `docs/work/btr-portal/principal-centric-analytics-migration/PRINCIPAL-KPI-REGISTRY.md` |
| Plan | `docs/work/btr-portal/principal-centric-analytics-migration/IMPLEMENTATION-PLAN.md` |
| Reason | The Principal KPI Registry is now the authoritative source for Principal Analytics semantics and supersedes conflicting KPI definitions in the implementation plan. |

---

## Authority

- Added the Principal KPI Registry as the KPI semantics authority.
- Feasibility assessment remains the planning authority for non-KPI scope.
- On any KPI conflict, the registry wins.

---

## KPI identity

- Replaced withdrawn IDs `PR-KPI-001` through `PR-KPI-015` and `CP-KPI-001` through `CP-KPI-005` with registry IDs.
- Canonical Principal performance KPI is `PRN-SALES-001` Principal Sales-Out, not `PR-KPI-001`.
- Added a KPI Registry Binding table that is the only allowed Principal KPI ID list.
- PCM-002 now registers registry IDs only.

| Withdrawn plan ID | Registry replacement | Change |
| --- | --- | --- |
| PR-KPI-001 Principal Sales-Out (DPP) | PRN-SALES-001 | ID and name aligned. Formula retained as Sales-Out (DPP). |
| PR-KPI-002 Principal Target | PRN-TGT-001 | ID aligned. Still the sum of Salesman Principal Targets. |
| PR-KPI-003 Principal Achievement Amount equals Sales-Out | PRN-TGT-002 | Corrected. Achievement Amount is Principal Sales-Out versus Target, not a copy of Sales-Out. |
| PR-KPI-004 Principal Achievement % | PRN-TGT-003 | ID aligned. Still Sales-Out ÷ Target. |
| PR-KPI-005 Month-over-month growth only | PRN-GRW-001 | ID aligned. Source KPI is PRN-SALES-001. |
| None | PRN-GRW-002 | Added Year-over-Year Growth Percentage. |
| PR-KPI-006 Active Customer Count | PRN-CUS-001 | ID aligned. Recency rule aligned to last transaction within 6 months. |
| None | PRN-CUS-002 | Added Customer Coverage Percentage. |
| PR-KPI-012 Good Return Amount | PRN-RET-001 | ID aligned. Evidence grain is Return Item. |
| PR-KPI-013 Broken Return Amount | PRN-RET-002 | ID aligned. |
| PR-KPI-014 Total Return Amount | PRN-RET-003 | ID aligned. |
| PR-KPI-015 Return Rate (%) | PRN-RET-004 | Renamed to Return Percentage. Formula is Return Amount ÷ Sales-Out. |
| None | PRN-PUR-001 | Added Purchase-In as an independent Principal KPI. |
| None | PRN-INV-001 | Added Inventory Value. |
| None | PRN-INV-002 | Added Inventory Days. |
| PR-KPI-007 Principal Sales Concentration | None | Removed. Not a registry KPI. |
| PR-KPI-008 Principal Salesman Coverage | None | Removed as a KPI. Salesman contribution remains a decomposition, not a KPI. |
| PR-KPI-009 Principal Forecast | None | Removed as a KPI. SA02 forecast remains a presentation over PRN-SALES-001 history. |
| PR-KPI-010 Principal Required Pace | None | Removed as a KPI. |
| PR-KPI-011 Principal Target Gap | None | Removed as a KPI. Versus-target is PRN-TGT-002. |
| CP-KPI-001 through CP-KPI-005 | None | Removed. Pair measures are snapshot attributes of PRN-SALES-001, not separate KPI IDs. |

---

## Measurement corrections

- `PRN-SALES-001` does not deduct Returns, Claims, or Inventory Adjustments. The previous plan named claims and rebates; Inventory Adjustments are now explicit.
- Return Percentage never reduces Principal Sales-Out.
- `PRN-TGT-001` evidence grain is `SalesPersonPrincipalTarget`. No independently maintained Principal Target exists.
- Active Customer uses last transaction within 6 months. Dormant means no transaction within 6 months. The previous plan's rule that returns never refresh relationship activity is withdrawn because it conflicted with the registry transaction rule.
- Relationship history remains retained indefinitely.
- `PRN-CUS-002` eligible base is the retained Customer × Principal Relationship Snapshot population. No pre-purchase eligibility master is introduced.
- Growth uses `PRN-SALES-001` only. Year-over-year growth is required, not only month-over-month.
- Authoritative ranking KPI is `PRN-SALES-001`.
- Supporting ranking KPIs are only `PRN-RET-004`, `PRN-TGT-003`, `PRN-GRW-001`, and `PRN-GRW-002`.
- `PRN-PUR-001`, `PRN-INV-001`, and `PRN-INV-002` are independent operational KPIs and are not Principal ranking KPIs.
- Principal Health Score is an explicit V1 non-goal.

---

## Slice and dependency changes

- PCM-002: catalog content replaced with registry IDs, ranking roles, and Health Score exclusion.
- PCM-003: Active-customer profiling wording aligned to the 6-month transaction rule.
- PCM-004: stores `PRN-SALES-001` and `PRN-RET-001` through `PRN-RET-004`. Return evidence grain is Return Item. Claims and Inventory Adjustments are not deducted.
- PCM-005: history must support `PRN-GRW-001` and `PRN-GRW-002`. Growth formulas added.
- PCM-006: target KPIs renamed. Achievement Amount is versus-target. Salesman contribution is not a registry KPI.
- PCM-007: SA04 uses registry names and the ranking hierarchy. Removed concentration, salesman coverage, and forecast as registry KPIs.
- PCM-008: SA01 Principal ranking uses `PRN-SALES-001`.
- PCM-009: SA03 evidences `PRN-SALES-001` and states that returns, claims, and inventory adjustments are not deducted.
- PCM-010: Principal forecast is retained as a non-registry presentation. It has no Principal KPI ID and is not a ranking KPI.
- PCM-012: must not publish `PRN-CUS-001` or `PRN-CUS-002`.
- PCM-013: relationship snapshot is the evidence grain for `PRN-CUS-001`. Active rule aligned to the registry. `CP-KPI-*` removed.
- PCM-014: decline and dormancy use the snapshot transaction rule. Coverage percentage is not this slice.
- PCM-015: Entity Analytics composes registry packs. Default ranking and growth axis use `PRN-SALES-001`. Health Score prohibited.
- PCM-016: omzet metadata points to `PRN-SALES-001`, not `PU-KPI-001`, `PRN-PUR-001`, or `SF-KPI-008`.
- PCM-017: attention uses `PRN-SALES-001`, `PRN-TGT-002`, `PRN-TGT-003`, or `PRN-RET-004`. No Health Score alert.
- PCM-018: Purchase-In labeling separated from Principal Sales-Out. Inventory profile KPIs are not this slice.
- PCM-019: knowledge sync must use registry IDs and depends on the new functional slices.
- PCM-020 added: `PRN-PUR-001` Purchase-In composition. Depends on PCM-002 and PCM-015.
- PCM-021 added: `PRN-INV-001` and `PRN-INV-002` from Inventory Snapshot. No new inventory algorithm.
- PCM-022 added: `PRN-CUS-002` from the relationship snapshot. Depends on PCM-002 and PCM-013.

Tracker updated with PCM-020, PCM-021, and PCM-022 at status PLANNED.

---

## Sections updated

- Planning authority and KPI semantics authority
- Scope summary evidence-grain statement
- Planning decisions PD-002, PD-003, PD-004, PD-005, PD-006, PD-007, PD-008, PD-009, and PD-010
- Out of scope
- Impact inventory
- Phase 2, Phase 5, and Phase 6
- Slice objectives, deliverables, acceptance criteria, and dependencies listed above
- Scope coverage
- Implementation constraints

Navigation placement, SA04, Salesman field-activity preservation, no Principal financial attribution, no invoice-time snapshot, and no Principal-scoped authorization are unchanged.

---

## Implementation guardrails

| Field | Value |
| --- | --- |
| Date | 2026-09-09 |
| Reason | Incorporate return semantic protection, a dedicated Customer–Principal relationship projection, and smaller independently reviewable slices. |

### Guardrails added

- GR-001: `PRN-SALES-001` is independent. Returns KPIs must never reduce, replace, or redefine it. A future Net Sales KPI must be a separate ID and must not replace Principal Sales-Out.
- GR-002: Customer–Principal relationships are materialized in `BTRPD_CustomerPrincipalRelationship`. Historical transactions remain the source of truth. Coverage, Active Customer, Dormant Customer, Relationship Analytics, and Entity Analytics must consume the projection and must not recompute relationships from raw transactions.
- GR-003: A slice may contain only one projection, KPI family, API, dashboard, or Entity Analytics feature. A slice that writes `PRN-SALES-001` must not write Returns.

### Measurement and ownership corrections

- Return writes are isolated from Sales-Out writes. `PRN-RET-004` may read Sales-Out and must not update it.
- Net Sales is added to out of scope.
- Relationship projection refresh is the only reader of historical transactions for relationship status.
- `PRN-CUS-001` and `PRN-CUS-002` are counted from the projection only.

### Slice splits

| Previous slice | Change |
| --- | --- |
| PCM-001 | Code and route only. Navigation documentation moved to PCM-056. |
| PCM-002 | Registers `PRN-SALES-001` and GR-001 only. Other KPI families register in their writer slices. |
| PCM-004 | Sales-Out snapshot only. Return writes moved to PCM-023 and PCM-024. |
| PCM-005 | Sales-Out history only. Return history moved to PCM-025. Growth moved to PCM-026 and PCM-027. |
| PCM-006 | `PRN-TGT-001` only. Achievement moved to PCM-028. Contribution moved to PCM-029. |
| PCM-007 | SA04 Sales-Out shell and default ranking only. Other panels moved to PCM-030 through PCM-034 and PCM-054. |
| PCM-011 | SF01 only. FI02 and FI04 moved to PCM-035 and PCM-036. |
| PCM-012 | Customer Entity Analytics label only. Customer page labels moved to PCM-037 through PCM-041. |
| PCM-013 | Projection materialization only. `PRN-CUS-001` moved to PCM-042. |
| PCM-014 | CU01 projection mix only. CU02, CU04, and CU05 moved to PCM-043, PCM-044, and PCM-045. |
| PCM-015 | Entity Analytics `PRN-SALES-001` pack only. Growth, returns, and target packs moved to PCM-046, PCM-047, and PCM-048. |
| PCM-016 | Supplier omzet metadata only. Customer top-Principal metadata moved to PCM-049. |
| PCM-017 | EX01 only. EX02 moved to PCM-050. |
| PCM-018 | PU labels only. Inventory cross-link moved to PCM-051. |
| PCM-019 | KPI catalog synchronization only. Other permanent knowledge moved to PCM-057 and PCM-058. |
| PCM-020 | Persist `PRN-PUR-001` only. Entity Analytics purchase pack moved to PCM-052. |
| PCM-021 | Persist inventory KPIs only. Entity Analytics inventory pack moved to PCM-053. |
| PCM-022 | Compute `PRN-CUS-002` from the projection only. Display moved to PCM-054. |

### Slices added

- PCM-023 Return amount snapshot
- PCM-024 Return Percentage
- PCM-025 Return monthly history
- PCM-026 Month-over-month growth
- PCM-027 Year-over-year growth
- PCM-028 Achievement amount and percentage
- PCM-029 Salesman contribution and responsibility exceptions
- PCM-030 SA04 target panel
- PCM-031 SA04 returns panel
- PCM-032 SA04 growth panel
- PCM-033 SA04 supporting rankings
- PCM-034 SA04 Salesman contribution panel
- PCM-035 FI02 labels
- PCM-036 FI04 labels
- PCM-037 CU01 ownership labels
- PCM-038 CU02 ownership labels
- PCM-039 CU03 ownership labels
- PCM-040 CU04 ownership labels
- PCM-041 CU05 ownership labels
- PCM-042 Active Customer count from the projection
- PCM-043 CU02 decline from the projection
- PCM-044 CU04 portfolio from the projection
- PCM-045 CU05 pair evidence from the projection
- PCM-046 Entity Analytics growth pack
- PCM-047 Entity Analytics returns pack
- PCM-048 Entity Analytics target pack
- PCM-049 Customer top-Principal metadata
- PCM-050 EX02 Principal sales alerts
- PCM-051 Inventory cross-link to SA04
- PCM-052 Entity Analytics purchase pack
- PCM-053 Entity Analytics inventory pack
- PCM-054 SA04 customer-reach panel
- PCM-055 Entity Analytics relationship presentation from the projection
- PCM-056 Navigation asset registry
- PCM-057 Portal domain, architecture, and Entity Analytics guidance
- PCM-058 Dashboard and question-map documentation

Tracker entries for PCM-023 through PCM-058 are PLANNED. Existing slice status remains PLANNED.
