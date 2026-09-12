# FEASIBILITY ASSESSMENT — Principal Sales-Out Map Time-Aware KPI Revision

| Field | Value |
| ----- | ----- |
| Document | `FEASIBILITY-ASSESSMENT.md` |
| Status | Analysis only — no implementation |
| Date | 2026-09-12 |
| Request | Replace the Principal Sales-Out Map axis KPI definitions with time-aware metrics valid on any day of the month |
| Affected surface | `principal-sales-out-map` population preset in the Principal Investigation Workspace |
| Environment of evidence | Database `btr2`, server `JUDE7` (production snapshot, generated dashboard data) |
| Related | [entity-analytics-developer-guide.md](../../features/entity-analytics/entity-analytics-developer-guide.md), [principal-investigation-workspace/FEASIBILITY-ASSESSMENT.md](../principal-investigation-workspace/FEASIBILITY-ASSESSMENT.md), `PRINCIPAL-KPI-REGISTRY.md`, [investigation: principal-yoy-growth-map-zero-axis](../../investigations/principal-yoy-growth-map-zero-axis/investigation.md) |

---

## Decision Log

| Date | Decision | Impact |
| ---- | -------- | ------ |
| 2026-09-12 | **GAP-001 CLOSED.** `BTRPD_PrincipalSalesOutHistory` remains month-grain and will not be modified. YoY MTD Growth for Principal Sales-Out Map V2 is calculated using dynamic BusinessDate-driven aggregation from transactional Sales-Out facts for both the current-year MTD window and the prior-year equivalent MTD window. No new daily history table, no historical backfill, and no migration of existing analytics projections are required. This minimizes implementation cost and isolates the change to the Principal Sales-Out Map feature. | GAP-001 closed; prior-year window sourced dynamically from transactional facts; scope isolated to the feature |
| 2026-09-12 | **GAP-002 CLOSED.** The system will not persist `ElapsedDays`, `DaysInMonth`, or `AsOfDate` in any Principal analytics snapshot or history table. These values are deterministic calendar attributes that can be derived from the existing Business Date and reporting period context at runtime. KPI calculations requiring pacing or same-period comparisons shall compute these values dynamically through a shared analytics period calculator/service. No schema changes, projection changes, or historical backfill are required. | GAP-002 closed; period attributes derived at runtime via a shared calculator; no schema/projection/backfill |
| 2026-09-12 | **GAP-003 CLOSED.** Introduce a new KPI named **Pacing Achievement %** = `Actual Sales MTD ÷ Expected Target MTD × 100`, where `Expected Target MTD = Monthly Target × (Elapsed Days ÷ Days In Month)`. The existing Achievement % KPI remains unchanged for backward compatibility. Pacing Achievement % is calculated dynamically from Monthly Target and Business Date context; no new tables, projections, schema changes, or historical backfill are required. MVP assumes **linear** target pacing across the month; seasonal or weighted pacing models are out of scope. | GAP-003 closed; new Pacing Achievement % KPI defined; existing Achievement % unchanged; dynamic calculation; linear pacing only |
| 2026-09-12 | **GAP-004 CLOSED.** Introduce a new KPI named **YoY MTD Growth %** = `(Current Year MTD Sales − Prior Year MTD Sales) ÷ Prior Year MTD Sales × 100`. Current and prior-year periods are aligned using Business Date and equivalent elapsed-day windows. The existing month-grain YoY Growth % KPI remains unchanged for backward compatibility. YoY MTD Growth % is calculated dynamically from transactional Sales-Out facts; no new tables, schema changes, projections, or historical backfill are required. If Prior Year MTD Sales is zero, the KPI value is returned as **NULL** (not calculable) rather than an artificial percentage. | GAP-004 closed; new YoY MTD Growth % KPI defined; existing YoY Growth % unchanged; dynamic from transactional facts; NULL on zero prior-year base |
| 2026-09-12 | **GAP-005 CLOSED.** The Principal Sales-Out Map shall support a **signed Y-axis** for YoY MTD Growth %. Negative growth values shall be rendered below the horizontal zero line and must **not** be clamped to zero. The chart shall automatically calculate suitable minimum and maximum Y-axis bounds based on the dataset and shall always display a visible **Y=0 reference line**. No changes to KPI calculations, database schema, projections, or reporting data structures are required. This change is limited to **chart rendering and axis configuration**. | GAP-005 closed; signed Y-axis with auto bounds and Y=0 line; rendering/axis-only change; no data/schema impact |
| 2026-09-12 | **GAP-006 CLOSED.** The Principal Sales-Out Map shall use **fixed business quadrants** rather than statistical regions. Boundaries: **X = 100% Pacing Achievement**, **Y = 0% YoY MTD Growth**. Labels: **Star** (X ≥ 100%, Y ≥ 0%), **Growing** (X < 100%, Y ≥ 0%), **Steady** (X ≥ 100%, Y < 0%), **Declining** (X < 100%, Y < 0%). Thresholds correspond directly to business performance expectations and remain stable over time. Statistical or population-relative boundaries are **out of scope**. | GAP-006 closed; fixed business quadrants at X=100% / Y=0%; statistical regions out of scope; quadrant labels defined |
| 2026-09-12 | **GAP-007 CLOSED.** Ratio-based KPIs shall include **confidence guards** to prevent misleading results caused by very small denominator values or insufficient elapsed time. For **YoY MTD Growth %**, if Prior Year MTD Sales are below a configurable minimum threshold, the KPI is reported as **NULL** with confidence status **LowConfidence**. For **Pacing Achievement %**, if elapsed days in the reporting month are below a configurable minimum threshold, the KPI is reported as **NULL** with confidence status **LowConfidence**. Low-confidence entities **shall not participate** in business quadrant classification and may be visually distinguished in the scatter chart. Threshold values shall be configurable through **KPI metadata** rather than hard-coded in calculations. | GAP-007 closed; minimum prior-year-base and minimum elapsed-day guards; NULL + LowConfidence; excluded from quadrants; thresholds configurable via KPI metadata |
| 2026-09-12 | **GAP-008 CLOSED.** KPI metadata registration is an **implementation activity** rather than an unresolved gap. As part of implementation, the **KPI Registry** shall be updated to register **Pacing Achievement %** and **YoY MTD Growth %**, including display labels, units, formatting rules, descriptions, and chart-axis mappings. No additional business, product, or architectural decisions are required. Moved from Gap Analysis to the **Implementation Checklist**. | GAP-008 closed; KPI Registry registration is implementation work, not a feasibility gap; relocated to Implementation Checklist |
| 2026-09-12 | **GAP-009 CLOSED.** The **KPI Registry** shall be the authoritative source of KPI definitions. KPI Catalog documentation and Lens Configuration references shall use registered KPI identifiers rather than independently maintained KPI names or formulas. Any newly introduced KPI must be registered in the KPI Registry before being consumed by dashboards, investigations, exports, or analytics lenses. No additional architectural component, synchronization service, schema change, or projection is required. This is a **governance rule** enforced through implementation and review processes. Moved from Gap Analysis to **Non-Functional Governance Requirements**. | GAP-009 closed; KPI Registry is authoritative; catalog/lens reference registered ids; governance rule, not a feasibility gap |
| 2026-09-12 | **GAP-010 CLOSED.** Principal Attention Emitter enhancements are **not required** to achieve the objective of this initiative, which is to make the Principal Sales-Out Map meaningful throughout the reporting month. The introduction of **Pacing Achievement %** and **YoY MTD Growth %** does not require new attention events. **Existing attention-emitter behavior remains unchanged.** Future attention rules such as Growth Deterioration and Target Miss may be evaluated as a **separate analytics enhancement initiative**. | GAP-010 closed; no attention-emitter changes; out of scope; existing behavior unchanged |
| 2026-09-12 | **OQ-007 CLOSED.** The scope of this initiative is limited to the **Principal Sales-Out Map** and **Principal Profile** experiences, which are directly impacted by the identified partial-month comparison bias. New KPIs (**Pacing Achievement %** and **YoY MTD Growth %**) shall be introduced and consumed by the Principal Sales-Out Map and Principal Profile **only**. **SA04 is explicitly out of scope** and shall be evaluated separately to determine whether it experiences the same business problem and whether time-aware KPI variants provide additional value. **No SA04 changes are required** as part of the current implementation. | OQ-007 resolved; scope = Map + Profile only; SA04 deferred to separate evaluation; no SA04 changes |
| 2026-09-12 | **MIGRATION OPTION SELECTED: Option B.** Introduce new KPI versions (e.g. `PRN-TGT-004` Pacing Achievement %, `PRN-GRW-003` YoY MTD Growth %); repoint the `principal-sales-out-map` preset; keep `PRN-TGT-003`/`PRN-GRW-002` unchanged. Option A (replace in place) is rejected; Option C (user-selectable mode) is deferred. | Section 8 decision; Option B adopted; existing KPI ids preserved |

---

## 1. Executive Summary

### Request

The `principal-sales-out-map` scatter chart currently plots:

- **X-Axis** = Achievement % (`PRN-TGT-003`)
- **Y-Axis** = Year-on-Year Growth % (`PRN-GRW-002`)

Both are **full-period vs month-to-date** comparisons. The Y-axis compares `current month-to-date` against `prior-year full month`, producing a systematic negative bias for most of the month, distorting the quadrant reading and management decisions. The request is to replace the axis KPIs with time-aware definitions that remain valid on any day:

- **X-Axis (proposed):** Pacing Achievement % = `Sales MTD ÷ Expected Target MTD`, where `Expected Target MTD = Monthly Target × (Elapsed Days ÷ Total Days)`
- **Y-Axis (proposed):** YoY MTD Growth % = `Sales MTD current year` vs `Sales MTD same period last year`

### Recommendation

**FEASIBLE.** Both candidate definitions are computable from existing evidence and are consistent with pacing patterns already used in the platform (`SalesForecastPolicy`, `PrincipalInventoryAggregator`). The selected route is **Option B — introduce new KPI versions** rather than overwrite `PRN-TGT-003`/`PRN-GRW-002`, because those IDs carry strong catalog invariants and feed SA04 and the Principal profile. The revision also requires the **signed Y-axis rendering fix** already identified (negative growth must not clamp to zero); this is now **closed** as **GAP-005** — the map renders negative growth below a visible **Y=0** reference line with auto-calculated min/max bounds, and the change is limited to chart rendering/axis configuration. The prior-year data gap (**GAP-001**) is **closed**: YoY MTD is sourced dynamically via BusinessDate-driven aggregation from transactional facts, with no daily history table, backfill, or projection migration. The period-attribute gap (**GAP-002**) is also **closed**: `ElapsedDays`, `DaysInMonth`, and `AsOfDate` are derived at runtime by a shared analytics period calculator/service rather than persisted. The pacing KPI gap (**GAP-003**) is **closed**: a new **Pacing Achievement %** KPI is defined (`Actual Sales MTD ÷ Expected Target MTD × 100`, `Expected Target MTD = Monthly Target × Elapsed Days ÷ Days In Month`), the existing Achievement % remains unchanged, and MVP uses **linear** pacing only. The YoY KPI gap (**GAP-004**) is **closed**: a new **YoY MTD Growth %** (`(Current Year MTD Sales − Prior Year MTD Sales) ÷ Prior Year MTD Sales × 100`) is aligned on Business Date / equivalent elapsed-day windows, the existing month-grain YoY Growth % remains unchanged, and the value is **NULL** when the prior-year base is zero. The quadrant gap (**GAP-006**) is **closed**: the map uses **fixed business quadrants** at X = 100% Pacing Achievement and Y = 0% YoY MTD Growth (**Star** / **Growing** / **Steady** / **Declining**), replacing statistical regions; population-relative boundaries are out of scope. The confidence gap (**GAP-007**) is **closed**: ratio KPIs apply configurable minimum prior-year-base and minimum elapsed-day guards, returning **NULL** with **LowConfidence** status; low-confidence entities are excluded from business quadrant classification and may be visually distinguished; thresholds are supplied via **KPI metadata** rather than hard-coded. **Planning readiness is READY** — all gaps (GAP-001–GAP-010) and open questions (OQ-001–OQ-007) are closed/resolved. Scope is limited to the **Principal Sales-Out Map** and **Principal Profile**; **SA04 is explicitly out of scope** and deferred to a separate evaluation. GAP-008 is tracked as implementation work in the Implementation Checklist, GAP-009 is a non-functional governance requirement, and GAP-010 is a confirmed out-of-scope item with no attention-emitter changes required.

### Feasibility Result

```text
FEASIBLE
```

---

## 2. Request Understanding

| Dimension | Description |
| --------- | ----------- |
| Requested capability | Time-aware axis KPIs for the Principal Sales-Out Map that are meaningful on any day of the month |
| Business objective | Remove systematic negative bias in YoY and low bias in Achievement so quadrant classification is trustworthy intra-month |
| Expected outcome | Stable X/Y classification: performance measured against pace-to-date plan and like-for-like prior-year period |
| Affected users | Owners / management using the Principal Investigation Workspace; users of the Principal Profile |
| Explicit ask | Assess feasibility only; produce this artifact for a subsequent ARCHITECTURE.md |

Out of scope: changing `PRN-SALES-001`, returns, purchasing, or inventory KPIs; changing target persistence (`BTR_SalesPersonPrincipalTarget`); any write path; **SA04 (explicitly deferred — to be evaluated separately)**.

---

## 3. Current State Analysis

### 3.1 Where the map KPI values originate

```text
BTR_Faktur / BTR_FakturItem / BTR_Brg / BTR_Supplier  (raw evidence)
        ↓  worker domains (btr.portal.worker --domain All)
BTRPD_PrincipalSalesOut            (PRN-SALES-001, current MTD, per Principal)
BTRPD_PrincipalSalesOutHistory     (PRN-SALES-001, month-grain history)
BTRPD_PrincipalTarget              (PRN-TGT-001, monthly target)
BTRPD_PrincipalAchievement         (PRN-TGT-002, PRN-TGT-003)
BTRPD_PrincipalYoyGrowth           (PRN-GRW-002)
BTRPD_PrincipalMomGrowth           (PRN-GRW-001)
        ↓  SupplierEntityAnalyticsProducer (reads stored snapshots; writes L0)
BTRPD_EntityAnalytics_Current      (L0, per EntityType/EntityId/KpiId)
        ↓  EntityPopulationMapEngine (read-only)
GET /api/entity-analytics/population?entityType=Supplier&presetId=principal-sales-out-map
        ↓  btr.portal.web (PopulationMapCanvas + projection engine)
```

- Raw evidence read: `PrincipalSalesOutEvidenceDal.ListFakturItemEvidence` (`btr.infrastructure/.../PrincipalSalesOutEvidenceDal.cs:15`).
- History read (no date filter, month grain): `PrincipalSalesOutHistoryEvidenceDal.ListMonthlySalesOutHistory` (`.../PrincipalSalesOutHistoryEvidenceDal.cs:14`).
- Target evidence: `BTR_SalesPersonPrincipalTarget` summed per Principal/month by `PrincipalTargetAggregator`.
- L0 mapping: `SupplierEntityAnalyticsProducer` (`.../Producers/SupplierEntityAnalyticsProducer.cs:848-925`).
- Map engine: `EntityPopulationMapEngine.BuildPopulationMap` (`.../Services/EntityPopulationMapEngine.cs:35`).

### 3.2 Existing KPI definitions (authoritative)

| KPI | Name | Definition | Evidence |
| --- | ---- | ---------- | -------- |
| `PRN-SALES-001` | Principal Sales-Out | `SUM(FakturItem.SubTotal − FakturItem.DiscRp)` attributed via `Brg.SupplierId`; current month is **MTD** | `PrincipalSalesOutEvidenceDal`; `RefreshPrincipalSalesOutSnapshotWorker.CurrentMonthPeriode` |
| `PRN-TGT-001` | Principal Target | Sum of `BTR_SalesPersonPrincipalTarget.TargetAmount` for Principal × month (monthly, **not** paced) | `PrincipalTargetAggregator` |
| `PRN-TGT-003` | Achievement Percentage | `PRN-SALES-001 ÷ PRN-TGT-001` (stored MTD ÷ **full monthly** target); null when target ≤ 0 | `PrincipalAchievementComposer.CalculatePercentage:95`; `PrincipalKpiCatalog.cs:257` |
| `PRN-GRW-002` | Year-over-Year Growth Percentage | `(current month PRN-SALES-001 − same month prior year PRN-SALES-001) ÷ prior year`; null when prior ≤ 0 | `PrincipalYoyGrowthComposer.Calculate:93`; `PrincipalKpiCatalog.cs:560` |

**Root of the defect:** `PRN-TGT-003` compares MTD numerator against a full-month denominator (low bias), and `PRN-GRW-002` compares a partial current month against a full prior-year month (negative bias during the month; only self-corrects at month end).

**Measured evidence (JUDE7 / btr2, period 2026-06):**

- `PRN-GRW-002` in L0: 24 rows, **15 negative**, 1 positive, 8 null.
- June 2026 raw data ends **2026-06-24** (Rp 4.47 B) vs June 2025 full month (Rp 7.28 B).
- `PRN-TGT-003` in L0: 22 rows, range 0.2131–4.8694 (mixed; the low tail is expected intra-month).

### 3.3 Existing scatter chart implementation

- Preset: `EntityMapPresetRegistry.cs:78-88` — `AxisXKpiId = "PRN-TGT-003"`, `AxisYKpiId = "PRN-GRW-002"`, `BubbleKpiId = "PRN-SALES-001"`, `BubbleColorKpiId = "PRN-RET-004"`.
- API contract: `GET /api/entity-analytics/population` (`EntityAnalyticsController.cs:140`); DTO `PopulationMapResponseDto` with generic `AxisX*`/`AxisY*` fields.
- Projection/plot: `PopulationMapCanvas.vue` + `populationProjection/` (robust strategy). **Defect (now GAP-005 closed):** negative values were clamped at `robustProjectionStrategy.ts:54` and `robustStats.ts:52`, collapsing negative growth onto Y = 0. The map must render negative growth below the zero line with auto-calculated min/max bounds and a visible Y=0 reference line.
- Region labels are currently statistical (`Expected`, `Above Expected`, `Below Expected`) from regression residuals. **Superseded (GAP-006 closed):** the map shall use fixed business quadrants (Star / Growing / Steady / Declining) at X=100% / Y=0%; statistical population-relative boundaries are out of scope.
- Business intent (documented): identify **Star, Growing, Underperforming, Declining, High-return-risk** Principals (`principal-investigation-workspace/Architecture.md:377`). Fixed quadrant labels per GAP-006 align with this intent (note: the "Underperforming" legacy term maps to the **Declining** quadrant).

### 3.4 Principal Analytics architecture

- Materialized, worker-refreshed snapshots; API is read-only over `BTRPD_*` tables and L0/L1–L4 snapshots.
- Producer writes the entire Principal profile from **owned snapshots only**; it must not recompute source KPIs (`entity-analytics-developer-guide.md`).
- KPI identity is registry-driven: `PrincipalKpiCatalog` ↔ `SupplierEntityAnalyticsRegistrar` metadata ↔ `EntityMapPresetRegistry` ↔ `EntityInvestigationLensRegistry`.
- **No SQL views or stored procedures** were found for Principal reporting; all aggregation is in C# workers/services.

### 3.5 Existing KPI calculation services

| Service | Role | Pacing precedent |
| ------- | ---- | ---------------- |
| `PrincipalAchievementComposer` | Target/achievement | None (full-month target) |
| `PrincipalYoyGrowthComposer` | YoY from month-grain history | None |
| `PrincipalMomGrowthComposer` | MoM from month-grain history | None |
| `SalesForecastPolicy` | Company month-end forecast | **Yes** — `daysElapsed`, `DailyAverageSales`, `ForecastSales` (`SalesForecastPolicy.cs:44`) |
| `PrincipalInventoryAggregator` | Principal inventory | **Yes** — computes `daysElapsedInMonth` (`PrincipalInventoryAggregator.cs:29`) |
| `SalesForecastPrincipalPresentationComposer` | Per-Principal forecast (presentation only) | Reuses `SalesForecastPolicy` |

Calculations are **centralized** in Principal/Reporting services, not duplicated in SQL. Pacing is already an established platform pattern.

### 3.6 Reporting views / SPs / queries

- No `CREATE VIEW` / `CREATE PROCEDURE` for Principal reporting in `btr.sql`.
- Principal consumption surfaces: `GetPrincipalPerformanceQuery` (SA04 + legacy Principal performance API) and `EntityPopulationMapEngine` (workspace map).
- Indexes present: `IX_BTR_Faktur_FakturDate (FakturDate, VoidDate, FakturId)` — supports date-range scans; `IX_BTR_FakturItem_FakturId`.

### 3.7 Existing filtering behavior

- Population map supports `dimensionFilter` and `attentionOnly`; preset default for Supplier has `FilterDimensionKpiId = null` (no dimension filter).
- Lens config: `EntityInvestigationLensRegistry` Sales-Out lens `KpiIds` includes `PRN-TGT-003`, `PRN-GRW-002`.
- Attention categories for the Sales-Out lens include `Growth Deterioration` and `Target Miss`, registered in `SupplierAttentionSignalCatalog.cs:20-22`. **Note:** the Principal attention emitter (`DashboardPurchasingManagementAggregator`) currently emits only purchase/inventory signal codes, so these Sales-Out categories appear to be registry-only today. **Out of scope (GAP-010 closed):** no attention-emitter changes are required for this initiative; existing behavior remains unchanged, and future Growth Deterioration / Target Miss rules are deferred to a separate analytics initiative.

---

## 4. Business Feasibility

### 4.1 Does MTD-based comparison solve the bias?

**Yes.** Comparing like-for-like elapsed windows removes the structural asymmetry:

- YoY: `1–D current month` vs `1–D same month prior year` — no denominator inflation.
- Achievement: `Sales MTD ÷ Paced Target MTD` — numerator and denominator share the same elapsed horizon.

### 4.2 Is target pacing meaningful?

**Yes, with a semantic caveat.** Pacing answers "are we on plan at this point in the month?" rather than "did we attain the full target?". Both are legitimate; they are not interchangeable. The user-facing label and the KPI definition must state which question is answered. A second caveat: linear pacing (`Target × elapsed/total`) does not model intra-month seasonality. **Resolved (GAP-003):** MVP adopts linear pacing; seasonal or weighted pacing models are out of scope. `PRN-TGT-003` (existing Achievement %) remains unchanged for backward compatibility.

### 4.3 Edge-case behavior

| Scenario | Behavior with proposed definitions | Residual risk |
| -------- | ---------------------------------- | ------------- |
| Beginning of month (day 1–3) | Elapsed days below configured minimum → Pacing Achievement = **NULL**, confidence **LowConfidence** (GAP-007) | Managed — excluded from quadrant classification |
| Missing target (`PRN-TGT-001` null/0) | Pacing Achievement = null | Principal absent from X; needs explicit "no target" handling |
| New principal (no prior-year sales) | YoY MTD = NULL | Principal absent from Y; may need alternative or exclusion |
| Prior-year period sparse/zero | Prior Year MTD Sales = 0 → KPI **NULL** (GAP-004 rule), never an artificial percentage | Correct but drops principals from growth view |
| Small prior-year base | Prior Year MTD Sales below configured minimum → KPI **NULL**, confidence **LowConfidence** (GAP-007) | Managed — excluded from quadrant classification |
| Seasonal products | Month-aligned comparison controls annual season, not intra-month | Acceptable; disclose |
| Sparse transaction months | Volatile percentages | Managed via minimum-base / confidence guards (GAP-007) |
| Leap year / differing month length | Clamp prior-year day to last valid day | Handled by rule |
| Mid-month business date vs data cutoff | Elapsed days must match the MTD cutoff | Resolved — single BusinessDate-driven as-of rule (OQ-003); derived at runtime |

### 4.4 Quadrant interpretability

Pacing Achievement (X) and YoY MTD Growth (Y) yield intuitive quadrants. **Fixed (GAP-006): boundaries are X = 100% Pacing Achievement and Y = 0% YoY MTD Growth.**

| Quadrant | Boundary | Indicative meaning | Label (fixed) |
| -------- | -------- | ------------------ | ------------- |
| High pacing + High growth | X ≥ 100%, Y ≥ 0% | Ahead of plan and growing | **Star** |
| High pacing + Low growth | X ≥ 100%, Y < 0% | On plan but contracting | **Steady** |
| Low pacing + High growth | X < 100%, Y ≥ 0% | Ramping, behind pace | **Growing** |
| Low pacing + Low growth | X < 100%, Y < 0% | Behind plan and declining | **Declining** |

Labels and thresholds are fixed business decisions (OQ-002 resolved); statistical/population-relative boundaries are out of scope. **Low-confidence entities (GAP-007) do not participate in quadrant classification** and may be visually distinguished. Bubble Color (Return %, `PRN-RET-004`) remains the fifth business dimension (High-return-risk).

---

## 5. Technical Feasibility

### 5.1 Data sufficiency

| Requirement | Existing | Verdict |
| ----------- | -------- | ------- |
| Current MTD Sales-Out per Principal | `BTRPD_PrincipalSalesOut` / `PRN-SALES-001` | Sufficient |
| Monthly target per Principal | `BTRPD_PrincipalTarget` / `PRN-TGT-001` | Sufficient |
| Elapsed days / days in month | Derivable from business date; precedent in `PrincipalInventoryAggregator` and `SalesForecastPolicy` | Sufficient — derived at runtime by a shared period calculator, not persisted (GAP-002 closed) |
| Prior-year same-period Sales-Out | Raw `BTR_Faktur` (data from 2025-02); month-grain history is **insufficient** for day-range windows | Sufficient via dynamic BusinessDate-driven aggregation over transactional facts (GAP-001 closed) |

**Resolved as GAP-001 (2026-09-12):** `BTRPD_PrincipalSalesOutHistory` stores **month grain only** and will not be modified. The prior-year equivalent MTD window is obtained by a dynamic **BusinessDate-driven range aggregation** over transactional facts `BTR_Faktur → BTR_FakturItem → BTR_Brg → BTR_Supplier` (mirrors existing evidence queries). No new daily history table, no historical backfill, and no migration of existing analytics projections are required.

### 5.2 Query complexity / performance

- Two aggregate range queries per refresh (current window + prior-year window), grouped by Principal. Comparable in cost to existing history/return evidence queries.
- `IX_BTR_Faktur_FakturDate (FakturDate, VoidDate, FakturId)` supports the range predicate; join to `BTR_FakturItem` uses its clustered/PK path. No new index strictly required; monitor.
- Worker cadence (PurchasingManagement 30 min) is ample; aggregation is materialized, so API latency is unaffected.

### 5.3 Aggregation / historical / cache

- Pacing Achievement % is computed **dynamically** from `BTRPD_PrincipalTarget` / `PRN-TGT-001` and the Business Date context; no new tables, projections, or schema changes (GAP-003 closed).
- YoY MTD Growth % is computed dynamically from transactional Sales-Out facts, aligned on Business Date / equivalent elapsed-day windows, and returns **NULL** when Prior Year MTD Sales is zero (GAP-004 / GAP-001); no new daily history table or backfill.
- Period attributes (`ElapsedDays`, `DaysInMonth`, `AsOfDate`) are derived at runtime by a shared period calculator/service (GAP-002 closed).
- Historical requirement: 12+ months for YoY; available from Feb 2025.
- Cache implications: none beyond existing snapshot refresh ordering (`PrnSalesOutHistory` → `PrnYoyGrowth` → `PurchasingManagement`).

### 5.4 API contract implications

- The population endpoint and DTO are **axis-agnostic** (KPI ids in preset; values resolved from L0). Repointing axes requires **no API contract change**.
- Signed Y-axis is supported (GAP-005 closed): negative YoY renders below the zero line; no clamping; auto min/max bounds and a visible Y=0 reference line. Frontend projection/axis-config change only; no API/DTO change.
- KPI Registry registration is implementation work (GAP-008, see **Implementation Checklist**): register both KPIs with display labels, units, formatting, descriptions, and chart-axis mappings, plus the configurable confidence thresholds/status (GAP-007). No calculation-side hard-coding.
- Population DTO/response shape may need to carry per-entity confidence so the frontend can exclude low-confidence entities from quadrants (pending architecture; axis-agnostic today).

---

## 6. Impact Analysis

### 6.1 KPI / widget / report impact inventory

| Artifact | Type of change if axes are revised | Direct | Indirect | User-visible | Data compatibility |
| -------- | ---------------------------------- | ------ | -------- | ------------ | ------------------ |
| `principal-sales-out-map` preset | Axis KPI ids / labels | Yes | Workspace map | Yes | Additive if new KPIs |
| `EntityPopulationMapEngine` / population API | Generic; no change if new KPIs | No | Read path | No | Compatible |
| `PopulationMapCanvas` projection | Signed Y-axis, auto bounds, Y=0 line; no clamping (GAP-005 closed) | Yes | Map render | Yes | Compatible (rendering only) |
| Map quadrant overlay/labels | Replace statistical regions with fixed business quadrants at X=100% / Y=0% (GAP-006 closed) | Yes | Map render | Yes | Compatible (rendering only) |
| Confidence guards / status | NULL + `LowConfidence`; thresholds from KPI metadata; exclude from quadrants (GAP-007 closed) | Yes | KPI values + map | Yes | Additive (no data/schema) |
| `SupplierEntityAnalyticsProducer` | Map new KPI to L0 | Yes | Profile | Yes | Additive |
| `SupplierEntityAnalyticsRegistrar` | Metadata for new KPI(s) | Yes | Profile/map | Yes | Additive |
| `EntityInvestigationLensRegistry` | Add KPI to Sales-Out lens | Yes | Lens filter | Yes | Additive |
| `GetPrincipalPerformanceQuery` (SA04) | **Out of scope** — no SA04 changes; deferred to separate evaluation (OQ-007 closed) | No | SA04 panels | No | Unchanged |
| `PrincipalAchievementComposer` | Pacing calculation — dynamic; no snapshot/schema change (GAP-003 closed) | Yes | Snapshot | Indirect | Additive if new KPI |
| `PrincipalYoyGrowthComposer` / worker | YoY MTD calculation — dynamic from transactional facts; no snapshot/schema change (GAP-004 closed) | Yes | Snapshot | Indirect | Additive |
| `BTRPD_PrincipalAchievement` / `BTRPD_PrincipalYoyGrowth` | No change — YoY MTD derived dynamically from transactional facts | No | Storage | No | No migration (GAP-001 closed) |
| Attention `Growth Deterioration` / `Target Miss` | No change — out of scope; existing emitter behavior unchanged (GAP-010 closed) | No | Alert Center | No | Unchanged |
| `btr-portal-kpi-catalog.md` / `PRINCIPAL-KPI-REGISTRY.md` | Documentation | Yes | Docs | No | Must sync |
| Principal KPI tests | Assertions | Yes | CI | No | Update |

### 6.2 Backend impact

`PrincipalAchievementComposer`, `PrincipalYoyGrowthComposer`, `Refresh*SnapshotWorker`, `SupplierEntityAnalyticsProducer`, `SupplierEntityAnalyticsRegistrar`, `EntityMapPresetRegistry`, `EntityInvestigationLensRegistry`, `PrincipalKpiCatalog`. (`GetPrincipalPerformanceQuery`/SA04 is **out of scope** — OQ-007 closed.)

### 6.3 Database impact

No migration of existing analytics projections is required (GAP-001 closed). `BTRPD_PrincipalSalesOutHistory` remains month-grain and unmodified. The prior-year equivalent MTD window is produced by dynamic BusinessDate-driven aggregation over transactional Sales-Out facts, so no new daily history table, no historical backfill, and no existing snapshot migration are introduced. Period attributes (`ElapsedDays`, `DaysInMonth`, `AsOfDate`) are not persisted in any Principal snapshot or history table; they are derived at runtime by a shared analytics period calculator/service (GAP-002 closed). Pacing Achievement % and YoY MTD Growth % introduce no new tables, projections, or schema changes; they are computed dynamically (GAP-003/GAP-004 closed). No source table (`BTR_Faktur*`, `BTR_SalesPersonPrincipalTarget`) change.

### 6.4 Frontend impact

`PopulationMapCanvas` + `populationProjection` (signed axis, fixed business quadrant overlay/labels at X=100% / Y=0%, low-confidence visual distinction/exclusion from quadrants), preset label/tooltip text, Principal Profile Growth/Target sections. **SA04 growth/target panels are out of scope (OQ-007 closed).**

### 6.5 Integration / security impact

- Integration: none external; worker-only materialization.
- Security: none; read-only analytics, no new authorization surface.

---

## 7. Quadrant Interpretation Review

The current canvas renders **statistical** regions (`Expected` / `Above` / `Below Expected`) and a statistical classification (Normal/Watch/Attention/Critical), while the documented business purpose is **Star / Growing / Underperforming / Declining / High-return-risk**. No explicit business quadrant labels were implemented.

Decision (GAP-006 closed):
- **Replace** the statistical/population-relative regions with **fixed business quadrants**.
- Boundaries: **X = 100% Pacing Achievement**, **Y = 0% YoY MTD Growth**.
- Labels: **Star** (X ≥ 100%, Y ≥ 0%), **Growing** (X < 100%, Y ≥ 0%), **Steady** (X ≥ 100%, Y < 0%), **Declining** (X < 100%, Y < 0%).
- Statistical or population-relative boundaries are **out of scope**.
- Retain Bubble Color = Return % as the risk dimension; do not merge risk into the quadrant.
- Thresholds are stable over time and not Product-configurable (OQ-002 resolved).

---

## 8. Migration Options

**Decision (2026-09-12): Option B is selected** — introduce new KPI versions and keep the existing KPIs unchanged. This is consistent with OQ-001 (resolved), GAP-003, and GAP-004. Options A and C are not adopted.

### Option A — Replace existing KPI definitions

Redefine `PRN-TGT-003` and `PRN-GRW-002` in place.

| Aspect | Assessment |
| ------ | ---------- |
| Advantages | Fewer IDs; map/profile/SA04 pick up new semantics automatically |
| Disadvantages | Breaks catalog invariants and tests; changes historical meaning of stored KPI ids; SA04 and profile silently change; audit/traceability loss |
| User impact | High — same label, different meaning |
| Technical impact | High — touches many consumers simultaneously |
| Operational impact | Hard to reason about historical snapshots |
| Verdict | **Rejected** |

### Option B — Introduce new KPI versions (SELECTED)

Add e.g. `PRN-TGT-004` (Pacing Achievement %) and `PRN-GRW-003` (YoY MTD Growth %); repoint the preset axes; keep `PRN-TGT-003`/`PRN-GRW-002` unchanged for SA04/ranking.

| Aspect | Assessment |
| ------ | ---------- |
| Advantages | Additive; preserves invariants and historical comparability; map gains time-aware axes; reversible |
| Disadvantages | More KPI ids to maintain; requires catalog test updates; two growth/achievement concepts coexist and must be clearly labeled |
| User impact | Medium — new labels on map; SA04 unchanged |
| Technical impact | Medium — additive registrar/catalog/producer wiring (no snapshot/schema change per GAP-002–GAP-004) |
| Operational impact | Low |
| Verdict | **Selected / Adopted (2026-09-12)** |

### Option C — User-selectable KPI mode

Allow the user to toggle "Full-period" vs "Time-aware".

| Aspect | Assessment |
| ------ | ---------- |
| Advantages | Maximum flexibility; supports both questions |
| Disadvantages | Highest UX and test complexity; quadrant meaning changes with mode; state management |
| User impact | Medium–High — requires user understanding |
| Technical impact | Medium–High |
| Operational impact | Low |
| Verdict | **Deferred** to post-MVP if demand exists |

---

## 9. Gap Analysis

| Gap ID | Type | Description | Status |
| ------ | ---- | ----------- | ------ |
| GAP-001 | Data | `BTRPD_PrincipalSalesOutHistory` is month-grain; cannot supply prior-year same-period windows | **Closed** (2026-09-12) — history stays month-grain; YoY MTD derived via dynamic BusinessDate-driven aggregation from transactional facts; no new table/backfill/projection migration |
| GAP-002 | Data | No persisted "elapsed days / days in month / as-of date" on Principal growth/achievement snapshots | **Closed** (2026-09-12) — not persisted by design; derived at runtime via a shared analytics period calculator/service; no schema/projection/backfill |
| GAP-003 | Functional | No Pacing Achievement KPI exists (full-month target only) | **Closed** (2026-09-12) — new **Pacing Achievement %** = `Actual Sales MTD ÷ Expected Target MTD × 100`, `Expected Target MTD = Monthly Target × Elapsed Days ÷ Days In Month`; existing Achievement % unchanged; dynamic calculation; linear pacing; no tables/schema/backfill |
| GAP-004 | Functional | No YoY MTD Growth KPI exists (full-month comparison only) | **Closed** (2026-09-12) — new **YoY MTD Growth %** = `(Current Year MTD Sales − Prior Year MTD Sales) ÷ Prior Year MTD Sales × 100`; Business Date / equivalent elapsed-day alignment; existing month-grain YoY Growth % unchanged; dynamic from transactional facts; NULL on zero prior-year base; no tables/schema/backfill |
| GAP-005 | UX | Signed Y-axis not supported; negative growth clamps to zero | **Closed** (2026-09-12) — signed Y-axis; negative growth below Y=0; auto min/max bounds; visible Y=0 reference line; chart-rendering/axis-only change |
| GAP-006 | UX | No explicit business quadrant labels/thresholds; only statistical regions | **Closed** (2026-09-12) — fixed business quadrants at X=100% / Y=0%: Star (X≥100%, Y≥0%), Growing (X<100%, Y≥0%), Steady (X≥100%, Y<0%), Declining (X<100%, Y<0%); statistical boundaries out of scope |
| GAP-007 | Technical | No minimum-base / low-confidence guard for pacing or YoY ratios | **Closed** (2026-09-12) — ratio KPIs return NULL + `LowConfidence` when prior-year base or elapsed days fall below configurable minimums; low-confidence excluded from quadrants and visually distinguishable; thresholds in KPI metadata |
| GAP-010 | Technical | Principal attention emitter does not emit `Growth Deterioration`/`Target Miss` | **Closed** (2026-09-12) — out of scope; no attention-emitter enhancements required; existing behavior unchanged; future rule evaluation deferred to a separate analytics initiative |

> GAP-008 (KPI metadata registration) was **closed** as implementation work and moved to the **Implementation Checklist**; it is no longer a feasibility gap.
> GAP-009 (KPI catalog/registry/lens synchronization) was **closed** as a governance rule and moved to **Non-Functional Governance Requirements**; it is no longer a feasibility gap.

---

## 10. Open Questions

| OQ ID | Class | Question | Recommended answer | Blocking |
| ----- | ----- | -------- | ------------------ | -------- |
| OQ-001 | Business | Replace `PRN-TGT-003`/`PRN-GRW-002` or add new KPI versions? | **Add new versions (Option B)** — resolved by GAP-003/GAP-004: existing Achievement % and YoY Growth % remain unchanged; new Pacing Achievement % and YoY MTD Growth % introduced | No — resolved |
| OQ-002 | Business | Quadrant labels and thresholds? | **Fixed (GAP-006)** at X=100% / Y=0%: Star (X≥100%, Y≥0%), Growing (X<100%, Y≥0%), Steady (X≥100%, Y<0%), Declining (X<100%, Y<0%); statistical boundaries out of scope | No — resolved |
| OQ-003 | Business | Define the MTD window "as-of": business date, or last data date? | **Business date** — resolved by GAP-001 decision (dynamic BusinessDate-driven aggregation) | No — resolved |
| OQ-004 | Business | Linear pacing vs calendar/seasonal-weighted pacing? | **Linear** for MVP — resolved by GAP-003 decision; seasonal/weighted pacing out of scope | No — resolved |
| OQ-005 | Technical | Minimum elapsed-day / minimum prior-year base guard? | **Resolved (GAP-007)** — configurable minimums via KPI metadata; below threshold → NULL + `LowConfidence`; excluded from quadrants | No — resolved |
| OQ-006 | Technical | Store paced/MTD KPIs in existing snapshots (new columns) vs new tables? | **Neither** — computed dynamically, not persisted (GAP-002/GAP-003); no schema change | No — resolved |
| OQ-007 | Operational | Does SA04 need the time-aware metrics too, or map only? | **Resolved** — scope is **Map + Principal Profile only**; **SA04 explicitly out of scope**, deferred to separate evaluation; no SA04 changes (OQ-007 closed) | No — resolved |

---

## 11. Risks

| Risk | Impact | Probability | Mitigation |
| ---- | ------ | ----------- | ---------- |
| Changing `PRN-TGT-003`/`PRN-GRW-002` in place corrupts historical meaning | High | Medium (if Option A) | Adopt Option B |
| Early-month volatility makes pacing/YoY misleading | Medium | **Mitigated** (GAP-007 closed) | Configurable minimum elapsed-day / prior-year-base guards; NULL + `LowConfidence`; excluded from quadrants |
| Negative growth still renders as zero | High | **Resolved** (GAP-005 closed) | Signed-axis rendering + Y=0 reference line; no clamping |
| Prior-year data incomplete (data starts 2025-02) | Medium | Medium | NULL when prior-year base is zero (GAP-004) or below threshold (GAP-007); disclose |
| Two growth/achievement concepts confuse users | Medium | Medium | Distinct labels + tooltip definitions |
| Worker refresh omits dedicated growth CLI domain | Low | Low | Growth runs under `--domain All`; document |
| Catalog invariant tests fail on new KPI ids | Low | Certain | Update tests intentionally |

---

## 12. Recommendation

1. **Adopt Option B** — introduce time-aware KPI versions and repoint the `principal-sales-out-map` preset. Keep `PRN-TGT-003`/`PRN-GRW-002` unchanged (resolved by GAP-003/GAP-004).
2. **X (GAP-003 closed):** new **Pacing Achievement %** = `Actual Sales MTD ÷ (Monthly Target × Elapsed Days ÷ Days In Month) × 100`; the existing Achievement % (`PRN-TGT-003`) remains unchanged for backward compatibility. Computed dynamically; **linear** pacing only (MVP).
3. **Y (GAP-004 closed):** new **YoY MTD Growth %** = `(Current Year MTD Sales − Prior Year MTD Sales) ÷ Prior Year MTD Sales × 100`, aligned on Business Date / equivalent elapsed-day windows; the existing month-grain YoY Growth % (`PRN-GRW-002`) remains unchanged; returns **NULL** when the prior-year base is zero.
4. **Compute** both dynamically from transactional Sales-Out facts (current and prior-year MTD windows) and Business Date context. GAP-001/GAP-004 closed: no new daily history table, no historical backfill, and no migration of existing analytics projections; existing snapshots remain unmodified.
5. **Signed Y-axis (GAP-005 closed):** render negative growth below the zero line, auto-calculate min/max bounds from the dataset, and always display a visible **Y=0** reference line; no clamping. Chart rendering/axis configuration only.
6. **Confidence guards (GAP-007 closed):** apply configurable minimum prior-year-base and minimum elapsed-day thresholds; return **NULL** + `LowConfidence` when breached; exclude low-confidence entities from quadrants and optionally distinguish them visually; source thresholds from **KPI metadata**.
7. **Quadrants (GAP-006 closed):** replace statistical regions with fixed business quadrants at X = 100% Pacing Achievement, Y = 0% YoY MTD Growth — **Star** (X≥100%, Y≥0%), **Growing** (X<100%, Y≥0%), **Steady** (X≥100%, Y<0%), **Declining** (X<100%, Y<0%); statistical boundaries out of scope.
8. **Sync** catalog, registry docs, lens config, and tests.
9. **Scope (OQ-007 closed):** deliver to the **Principal Sales-Out Map** and **Principal Profile** only; make **no SA04 changes**, and evaluate SA04 separately.

---

## 13. Go / No-Go Decision

```text
GO — FEASIBLE
```

Conditions:
- All blocking gaps/questions resolved (GAP-001–GAP-010, OQ-001–OQ-007).
- Signed-axis rendering (GAP-005), fixed business quadrants (GAP-006), and confidence guards (GAP-007) are **closed** and included in scope; KPI Registry registration (GAP-008) is tracked in the Implementation Checklist and KPI Registry governance (GAP-009) is a Non-Functional Governance Requirement; attention-emitter changes (GAP-010) and SA04 (OQ-007) are confirmed out of scope.

The change is additive, computed dynamically from existing data, index-supported, and does not alter source KPIs, persistence, or schema.

---

## 14. Implementation Impact Inventory

### Backend
- `PrincipalAchievementComposer` — Pacing Achievement % calculation (dynamic, linear pacing; existing Achievement % unchanged) (GAP-003 closed)
- `PrincipalYoyGrowthComposer` — YoY MTD Growth % from aligned Business Date / equivalent elapsed-day windows; existing month-grain YoY Growth % unchanged (GAP-004 closed)
- Confidence guard logic — NULL + `LowConfidence` when prior-year base / elapsed days below configured minimums; thresholds read from KPI metadata (GAP-007 closed)
- Shared analytics period calculator/service — derive `ElapsedDays`, `DaysInMonth`, `AsOfDate` at runtime (GAP-002 closed)
- `RefreshPrincipalSalesOutHistoryWorker` / evidence DAL — date-range aggregation (or new range DAL)
- `RefreshPrincipalAchievementSnapshotWorker`, `RefreshPrincipalYoyGrowthSnapshotWorker`
- `SupplierEntityAnalyticsProducer` — map new KPIs to L0
- `SupplierEntityAnalyticsRegistrar` / KPI Registry — register Pacing Achievement % and YoY MTD Growth % metadata (labels, units, formatting, descriptions, axis mappings, confidence thresholds) (GAP-008; see Implementation Checklist)
- `EntityMapPresetRegistry` — axis KPI ids
- `EntityInvestigationLensRegistry` — lens KPI list
- `PrincipalKpiCatalog` — new entries
- `GetPrincipalPerformanceQuery` — **out of scope** (SA04 deferred; OQ-007 closed); no changes

### Database
- No migration of existing analytics projections (GAP-001 closed)
- `BTRPD_PrincipalSalesOutHistory` remains month-grain and unmodified
- No new daily history table and no historical backfill; prior-year MTD window sourced dynamically from transactional facts
- No persistence of `ElapsedDays` / `DaysInMonth` / `AsOfDate`; derived at runtime via a shared analytics period calculator/service (GAP-002 closed)
- No new tables, projections, or schema changes for Pacing Achievement % — computed dynamically from Monthly Target + Business Date (GAP-003 closed)
- No new tables, schema, projections, or backfill for YoY MTD Growth % — computed dynamically from transactional facts; NULL when prior-year base is zero (GAP-004 closed)
- No persistence of confidence thresholds/status; thresholds come from KPI metadata and `LowConfidence` is computed at runtime (GAP-007 closed)
- No change to `BTR_Faktur*` / `BTR_SalesPersonPrincipalTarget`
- Indexes: none new required (`IX_BTR_Faktur_FakturDate` exists)

### Frontend
- `PopulationMapCanvas.vue` + `services/populationProjection/*` — signed Y-axis, auto min/max bounds, Y=0 reference line; no clamping (GAP-005 closed)
- Map quadrant overlay/labels — fixed business quadrants at X=100% / Y=0% (Star / Growing / Steady / Declining); replace statistical regions (GAP-006 closed)
- Low-confidence visual treatment — distinguish and/or exclude `LowConfidence` entities from quadrant classification (GAP-007 closed)
- `PopulationMapTooltip.vue` — labels
- `InvestigationWorkspaceView.vue` / preset display name
- Principal profile Growth/Target sections
- SA04 growth/target panels — **out of scope** (OQ-007 closed); no changes

### Integration
- None external.

### Security
- None; read-only analytics.

---

## 15. Implementation Checklist

The following is necessary implementation work, not a feasibility gap.

### KPI Registry Registration (GAP-008)

- [ ] Register **Pacing Achievement %** in the KPI Registry: display label, description, unit (`%`), formatting rules, confidence thresholds, and chart-axis mapping (**X**).
- [ ] Register **YoY MTD Growth %** in the KPI Registry: display label, description, unit (`%`), formatting rules, confidence thresholds, and chart-axis mapping (**Y**).
- [ ] Verify `AxisXLabel` / `AxisYLabel` / unit / formatting resolve from metadata (no calculation-side hard-coding).

### Feature Implementation

- [ ] Compute Pacing Achievement % dynamically (GAP-003) and YoY MTD Growth % dynamically (GAP-004).
- [ ] Apply confidence guards: NULL + `LowConfidence`, thresholds sourced from KPI metadata (GAP-007).
- [ ] Repoint the `principal-sales-out-map` preset axes to the new KPI ids.
- [ ] Render the signed Y-axis with auto min/max bounds and a visible Y=0 reference line (GAP-005).
- [ ] Render fixed business quadrants at X = 100% / Y = 0% and exclude/visually distinguish `LowConfidence` entities (GAP-006/GAP-007).
- [ ] Add the new KPI ids to the Sales-Out investigation lens.
- [ ] Reference registered KPI identifiers in KPI Catalog docs and Lens Configuration; update KPI tests (see §16).

---

## 16. Non-Functional Governance Requirements

- **KPI Registry is the authoritative source of KPI definitions**, identifiers, and metadata. (GAP-009)
- KPI Catalog documentation and Lens Configuration references **must use registered KPI identifiers** rather than independently maintained KPI names or formulas.
- Any newly introduced KPI (e.g., **Pacing Achievement %**, **YoY MTD Growth %**) **must be registered in the KPI Registry before** it is consumed by dashboards, investigations, exports, or analytics lenses.
- No additional architectural component, synchronization service, schema change, or projection is required; this is a governance rule enforced through implementation and review processes, not runtime machinery.

---

## 17. Planning Readiness

### Status

```text
READY
```

### Blocking Issues

None. All gaps (GAP-001–GAP-010) and open questions (OQ-001–OQ-007) are closed/resolved. Scope is limited to the **Principal Sales-Out Map** and **Principal Profile** (SA04 out of scope). GAP-008 is tracked as implementation work in the **Implementation Checklist** (Section 15); GAP-009 is a **Non-Functional Governance Requirement** (Section 16); GAP-010 is confirmed out of scope (no attention-emitter changes).

### Resolved Since Prior Draft

- GAP-001 (prior-year same-period data source) — **closed 2026-09-12**: history stays month-grain; YoY MTD uses dynamic BusinessDate-driven aggregation from transactional facts; no new table, backfill, or projection migration.
- GAP-002 (persistence of period attributes) — **closed 2026-09-12**: `ElapsedDays` / `DaysInMonth` / `AsOfDate` are not persisted; derived at runtime via a shared analytics period calculator/service; no schema, projection, or backfill changes.
- GAP-003 (Pacing Achievement KPI) — **closed 2026-09-12**: new **Pacing Achievement %** = `Actual Sales MTD ÷ Expected Target MTD × 100`; existing Achievement % unchanged; dynamic calculation; linear pacing (MVP); no tables/schema/backfill.
- GAP-004 (YoY MTD Growth KPI) — **closed 2026-09-12**: new **YoY MTD Growth %** = `(Current Year MTD Sales − Prior Year MTD Sales) ÷ Prior Year MTD Sales × 100`; existing month-grain YoY Growth % unchanged; dynamic from transactional facts; NULL on zero prior-year base; no tables/schema/backfill.
- GAP-005 (signed Y-axis rendering) — **closed 2026-09-12**: map supports a signed Y-axis; negative growth renders below the Y=0 line without clamping; auto min/max bounds and a visible Y=0 reference line; chart-rendering-only change.
- GAP-006 (fixed business quadrants) — **closed 2026-09-12**: quadrants fixed at X=100% Pacing Achievement and Y=0% YoY MTD Growth — Star (X≥100%, Y≥0%), Growing (X<100%, Y≥0%), Steady (X≥100%, Y<0%), Declining (X<100%, Y<0%); statistical boundaries out of scope; rendering-only change.
- GAP-007 (confidence guards) — **closed 2026-09-12**: ratio KPIs return NULL + `LowConfidence` when prior-year base / elapsed days fall below configurable minimums; low-confidence excluded from quadrants and visually distinguishable; thresholds from KPI metadata.
- GAP-008 (KPI metadata registration) — **closed 2026-09-12**: implementation activity, not a feasibility gap; relocated to the **Implementation Checklist** (Section 15).
- GAP-009 (KPI catalog/registry/lens synchronization) — **closed 2026-09-12**: governance rule, not a feasibility gap; relocated to **Non-Functional Governance Requirements** (Section 16).
- GAP-010 (Principal attention emitter) — **closed 2026-09-12**: out of scope; no attention-emitter changes required; existing behavior unchanged; future Growth Deterioration / Target Miss rules deferred to a separate analytics initiative.
- OQ-001 (KPI identity strategy) — **resolved**: add new KPI versions (Option B); existing Achievement % / YoY Growth % unchanged (GAP-003/GAP-004).
- OQ-002 (quadrant labels/thresholds) — **resolved**: fixed business quadrants at X=100% / Y=0% (GAP-006).
- OQ-003 (MTD as-of semantics) — **resolved**: BusinessDate (implied by the GAP-001 decision).
- OQ-004 (pacing model) — **resolved**: linear pacing; seasonal/weighted out of scope (GAP-003 decision).
- OQ-005 (minimum-base / minimum-elapsed guards) — **resolved**: configurable thresholds via KPI metadata; NULL + `LowConfidence` below threshold (GAP-007).
- OQ-006 (storage of paced/MTD KPIs) — **resolved**: neither new columns nor new tables; computed dynamically and not persisted (GAP-002/GAP-003/GAP-004).
- OQ-007 (SA04 scope) — **resolved**: scope limited to the Principal Sales-Out Map and Principal Profile; SA04 explicitly out of scope and deferred to separate evaluation; no SA04 changes.

### Planner Guidance

- Scope: add new time-aware KPI versions (Pacing Achievement %, YoY MTD Growth %) + preset repoint + signed-Y-axis rendering + fixed business quadrant overlay/labels + confidence guards; preserve existing KPIs.
- Dependencies: Business Date from `Presentation` config; transactional Sales-Out facts for YoY MTD; shared analytics period calculator for elapsed-day context; confidence thresholds in KPI metadata.
- Data sourcing: YoY MTD uses dynamic BusinessDate-driven aggregation from transactional Sales-Out facts; Pacing Achievement % is computed dynamically from Monthly Target + Business Date. Do not plan a daily history table, historical backfill, or migration of existing analytics projections (GAP-001/GAP-004 closed). Scope stays within the Principal Sales-Out Map feature.
- Sequencing concerns: KPI computation/metadata (including confidence thresholds) precede producer-to-L0 mapping; signed-Y-axis fix is independent of KPI data work. No schema/snapshot migration to sequence.
- Review concerns: verify `PRN-SALES-001` and returns remain untouched; verify old KPIs/SA04 unchanged; verify negative growth renders below the zero line; verify fixed quadrants at X=100% / Y=0% (no statistical boundaries); verify low-confidence entities are excluded from quadrants and thresholds are metadata-driven, not hard-coded; verify null/edge handling for missing target and missing prior-year.
