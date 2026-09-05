# Implementation Plan — Replenishment Risk Map Purchase Value Axis

**Status:** Implemented  
**Feature spec:** [replenishment-risk-map-purchase-value.md](../../../features/entity-analytics/replenishment-risk-map-purchase-value.md)  
**Scope:** Item Entity Analytics Replenishment Risk Map X-axis  
**Out of scope:** Changing IN03/IN04 dashboard tables, redefining portfolio `IN-KPI-023`, frontend chart redesign beyond unit-driven IDR behavior

---

## 1. Objective

Replace the Replenishment Risk Map default X-axis from **Recommended Purchase Qty** (`IN-KPI-021`) with an item-level **Recommended Purchase Value (IDR)** KPI, while preserving qty as an operational / detail metric.

---

## 2. Feasibility summary

| Perspective | Feasibility | Notes |
| --- | --- | --- |
| Business | **High** | Matches map question (purchase attention) and capital language already used in IN04 |
| Analytical | **High**, with one caveat | IDR is comparable; stock-out HPP resolution is required for correct stock-out placement |
| UX | **High** | Population Map already formats IDR axes (ladder ticks, log floor); labels come from KPI metadata |
| Technical | **Medium** | New L0 KPI + producer wiring + HPP fallback; no DB schema change for Entity Analytics tables if value is stored as standard numeric L0 row |

**Overall:** Feasible and recommended. Not a one-line preset swap — requires a real item value KPI and a correct unit-cost rule when on-hand qty is zero.

---

## 3. Current technical baseline

| Component | Today |
| --- | --- |
| Preset | `EntityMapPresetRegistry` → `replenishment-risk-map` → `AxisXKpiId = IN-KPI-021`, `AxisYKpiId = IN-KPI-020` |
| Item pack | `ItemEntityAnalyticsRegistrar` includes `IN-KPI-021` (Unit = `Count`) |
| Producer | `ItemEntityAnalyticsProducer` writes `IN-KPI-021` from `DashboardItemPortfolioRow.RecommendedPurchaseQty` |
| Portfolio row | Has `RecommendedPurchaseQty`; **no** purchase value / estimated cost field |
| Cost helper | `InventoryOptimizationPolicy.ComputeUnitHpp(qty, inventoryValue)` → `0` when `qty == 0` |
| IN04 row cost | `EstimatedCostIdr` = `RecommendedQty × unitHpp` on optimization reorder rows only (not Entity Analytics L0) |
| Population API | `EntityPopulationMapEngine` resolves axis values + `AxisXUnit` from KPI metadata |
| Frontend | IDR axis formatting / projection already implemented for unit `IDR` |

---

## 4. Perspective assessment

### 4.1 Business

**Benefits**

- Population attention ranked by indicative cash, not mixed UoM counts  
- Consistent with Inventory Health Map (value vs DOS) and Customer Risk Map (IDR axes)  
- Aligns Investigation Workspace language with Recommended Purchase Budget discussions without conflating portfolio vs item metrics  

**Drawbacks**

- Shifts “size” intuition from units to money; ops users must use detail for qty  
- HPP-based value is indicative cost, not supplier invoice price  

**Recommendation:** Accept for default map axis; keep qty visible in investigation detail / existing dashboards.

### 4.2 Analytical

**Benefits**

- Valid cross-sectional comparisons  
- Orthogonal axes: magnitude (IDR) × urgency (Days)  
- Peer Position on the new KPI inherits existing IDR binning  

**Drawbacks / biases**

- High-qty cheap SKUs compress leftward — usually correct for management capital triage  
- Zero on-hand + zero derived HPP collapses stock-outs to the origin on X even when qty recommendation is large — **unacceptable** without HPP fallback  

**Mitigation:** Resolve unit HPP from on-hand implied cost when qty &gt; 0; else barang master HPP (current `BTR_Brg.Hpp` or the same source IN pipelines already trust for cost). Document residual zeros as data quality (missing master HPP).

### 4.3 UX

**Benefits**

- Axis labels switch via metadata (`Recommended Purchase Value`, unit IDR)  
- Existing IDR tick ladder and projection floor apply automatically  
- Tooltips already show axis values; add qty as secondary fact if not already present for Item  

**Drawbacks**

- Dense cluster near Rp 0 for items with no buy recommendation (same as qty = 0 today)  
- Right-skewed IDR distributions — already handled for other IDR maps  

**UX requirements**

1. Preset display name stays **Replenishment Risk Map**; description may clarify “purchase value vs days of supply”.  
2. Tooltip / selection summary should show **value (IDR)** and **recommended qty** when investigating an item.  
3. Do not rename portfolio “Recommended Purchase Budget” to avoid confusion.

### 4.4 Technical

**Benefits**

- Preset change is localized  
- No new Entity Analytics table; L0 current/monthly rows already store arbitrary KPI ids  
- Frontend path is largely metadata-driven  

**Costs / risks**

- New KPI registration, catalog docs, producer, portfolio field or compute-at-produce  
- Tests: registrar pack, producer reconciliation, preset registry, optional population map fixture  
- Historical backfill: like `IN-KPI-021`, purchase value is **not reconstructable** from pure history without forecast replay — same limitation as qty; point-in-time going forward only  
- HPP fallback may require portfolio builder to carry master HPP or join at produce time  

---

## 5. Recommended solution design

### 5.1 KPI identity

Introduce **new** item KPI (proposed id: **`IN-KPI-028`** — confirm next free inventory KPI id in catalog):

| Field | Value |
| --- | --- |
| Display name | Recommended Purchase Value |
| Unit | `IDR` |
| Category | Activity (same family as qty) or Financial — prefer **Activity** with money unit, parallel to qty’s decision-support role |
| Formula | `Round(RecommendedPurchaseQty × UnitHpp, 2)` |
| Direction | Neutral (indicative, not “higher is better”) |
| TrendEligible | Yes (optional; mirror qty if monthly already written) |
| RankEligible | Optional; default false like qty unless product wants ranking |
| RadarEligible | No |

Keep:

- `IN-KPI-021` Recommended Purchase Qty — unchanged semantics  
- `IN-KPI-023` Recommended Purchase Budget — portfolio Critical/High sum only  

### 5.2 Unit HPP resolution (required business rule)

**Confirmed master cost source:** `BTR_Brg.Hpp` (current barang master HPP).

```text
if onHandQty > 0 and inventoryValue > 0:
    unitHpp = inventoryValue / onHandQty          // implied on-hand cost
else if BTR_Brg.Hpp > 0:
    unitHpp = BTR_Brg.Hpp                        // stock-out / zero-qty fallback
else:
    unitHpp = 0                                  // missing cost data
```

Then:

```text
recommendedPurchaseValue = Round(max(0, recommendedPurchaseQty) × unitHpp, 2)
```

Reuse `InventoryOptimizationPolicy.ComputePurchaseCost` for the multiply/round step. Extend or wrap `ComputeUnitHpp` so Entity Analytics and (optionally later) IN04 share the fallback — **minimum for this task** is correct value in Item portfolio / producer; aligning IN04 `EstimatedCostIdr` is a desirable follow-up but not mandatory for the map.

**Note:** Prefer implied on-hand cost when stock exists (matches inventory valuation). Use `BTR_Brg.Hpp` when on-hand qty is zero so stock-out replenishment candidates still get a non-zero purchase value on the map.

### 5.3 Data flow

```text
Inventory snapshot + forecast calc
  → DashboardItemPortfolioBuilder
      adds RecommendedPurchaseValue (and/or UnitHpp / MasterHpp inputs)
  → ItemEntityAnalyticsProducer
      L0 (+ optional L1 monthly) IN-KPI-028
  → EntityMapPresetRegistry
      replenishment-risk-map AxisX = IN-KPI-028
  → EntityPopulationMapEngine
      AxisXUnit = IDR → existing UI formatting
```

### 5.4 Required data changes

| Layer | Change |
| --- | --- |
| `DashboardItemPortfolioRow` | Add `RecommendedPurchaseValue` (decimal?) and/or carry `MasterHpp` from `BTR_Brg.Hpp` |
| Portfolio builder | Compute value using qty + HPP resolution; load `BTR_Brg.Hpp` for stock-out fallback |
| Master HPP source | **`BTR_Brg.Hpp`** — confirmed. Join/lookup by `BrgId` in portfolio build (or reuse any existing brg read already on the inventory snapshot path). No new Entity Analytics table column beyond L0 KPI rows |
| SQL Entity Analytics | None (generic KPI storage) |
| IN03/IN04 snapshot tables | No change required for map; optional later alignment of `EstimatedCostIdr` with master HPP fallback |
| KPI catalog / encyclopedia | Document `IN-KPI-028`; distinguish from `IN-KPI-023` |
| Feature docs | Update feasibility study references; keep this feature artifact as SSOT for the axis decision |

### 5.5 Code touch list (implementer)

1. `btr-portal-kpi-catalog.md` (+ encyclopedia generators if that is the house process)  
2. `ItemEntityAnalyticsRegistrar` — metadata + pack include  
3. `DashboardItemPortfolioRow` + `DashboardItemPortfolioBuilder`  
4. HPP input plumbing — load `BTR_Brg.Hpp` onto portfolio / builder for zero on-hand fallback  
5. `ItemEntityAnalyticsProducer` — L0/L1 emit `IN-KPI-028`  
6. `EntityMapPresetRegistry` — X axis KPI id  
7. Tests: `ItemEntityAnalyticsProducerTest`, reconciliation tests, preset/registry tests if present; add stock-out HPP fallback case  
8. Frontend: verify tooltip shows qty; only change if Item tooltip omits secondary qty  
9. Docs: feature artifact status → accepted; developer guide preset table if listed; M32 feasibility study note (historical work doc — optional footnote)

### 5.6 Explicitly do not

- Replace qty everywhere with value  
- Point Replenishment Map X at `IN-KPI-023`  
- Change Days of Supply axis  
- Require historical backfill of the new KPI for past months  

---

## 6. Benefits / drawbacks (decision table)

| | Purchase Value (recommended) | Keep Quantity |
| --- | --- | --- |
| Cross-item compare | Strong | Weak (UoM) |
| Capital decisions | Strong | Weak |
| PO execution | Needs qty in detail | Strong on-axis, weak across SKUs |
| Stock-out visibility | Needs HPP fallback | Works without HPP |
| Implementation cost | Medium (new KPI + HPP) | None |
| Consistency with other maps | High | Low |

---

## 7. Risks and mitigations

| Risk | Impact | Mitigation |
| --- | --- | --- |
| Stock-out HPP = 0 | High-urgency buys plot at X≈0 | Fallback to `BTR_Brg.Hpp`; QA fixture with qty=0, `BTR_Brg.Hpp`&gt;0, recommended qty&gt;0 |
| Confuse Value vs Budget | Product language clash | Distinct KPI name/id; docs call out portfolio vs item |
| Skew / clutter at zero | Visual noise | Existing IDR projection floor; filters by category; expected for non-buyers |
| Backfill gap | No history for new KPI | Accept; same class as forecast qty |
| Dual definition of purchase cost | IN04 vs EA diverge | Document formula; optional later unify EstimatedCostIdr |

---

## 8. Implementation steps

1. Confirm KPI id `IN-KPI-028` (or next free) in catalog.  
2. Specify and document HPP resolution rule in KPI catalog HOW section (`BTR_Brg.Hpp` fallback).  
3. Extend portfolio row/builder with Recommended Purchase Value; plumb `BTR_Brg.Hpp`.  
4. Wire producer + registrar pack/metadata (`Unit = IDR`).  
5. Switch `replenishment-risk-map` `AxisXKpiId` to the new KPI.  
6. Add/adjust unit tests including zero on-hand + master HPP.  
7. Manual UX check: Item workspace → Replenishment Risk Map → axis label IDR, stock-out candidate not stuck at zero when HPP exists, tooltip still shows qty.  
8. Update permanent docs; mark this plan Implemented after ship.

---

## 9. Test plan

| Case | Expectation | Status |
| --- | --- | --- |
| Normal on-hand | Value = qty × (inventoryValue/qty) | Done |
| Stock-out with `BTR_Brg.Hpp` | Value = recommendedQty × `BTR_Brg.Hpp` &gt; 0 | Done |
| Stock-out with `BTR_Brg.Hpp` = 0 | Value = 0; documented data gap | Done |
| Recommended qty = 0 | Value = 0 | Done |
| Preset | Population map X metadata = new KPI, unit IDR | Done |
| Qty KPI | `IN-KPI-021` still produced unchanged | Done |
| IDR formatting | Axis ticks use Rupiah ladder (existing path) | Done (metadata-driven) |

---

## 10. Final recommendation

**Proceed with Recommended Purchase Value (IDR) as the Replenishment Risk Map default X-axis**, implemented as a **new item KPI** (not a semantic change to qty, not reuse of portfolio budget).

**Rationale**

1. The map is a **population triage** surface; mixed UoM quantity fails the comparability requirement.  
2. Monetary magnitude + Days of Supply matches how management prioritizes replenishment attention and cash.  
3. The platform already supports IDR axes end-to-end; the missing piece is a correct item-level value in L0.  
4. The only material blocker is **unit cost when on-hand is zero** — solved by falling back to **`BTR_Brg.Hpp`** in the same change.  
5. Retaining qty outside the default X-axis preserves PO execution workflows.

**Approval gate:** User approval required before Implementer work. After approval, execute §8 in order.

---

## 11. Effort estimate (rough)

| Work | Size |
| --- | --- |
| KPI + registrar + producer + portfolio | Small–medium |
| HPP fallback plumbing | Small (`BTR_Brg.Hpp` is the confirmed source; effort is join/lookup wiring) |
| Preset + tests + docs | Small |
| Frontend (if tooltip already dual-shows) | Minimal |

**Total:** roughly **0.5–1 engineering day** with `BTR_Brg.Hpp` as the confirmed stock-out cost source.
