# Replenishment Risk Map — Purchase Value Axis

**Status:** Permanent knowledge (implemented)  
**Feature area:** Entity Analytics — Investigation Workspace  
**Related:** [peer-position-distribution.md](peer-position-distribution.md), [entity-analytics-developer-guide.md](entity-analytics-developer-guide.md)  
**Work plan:** [implementation-plan.md](../../work/btr-portal/entity-analytics/replenishment-risk-map-purchase-value/implementation-plan.md)

---

## Purpose

Decide whether the Item **Replenishment Risk Map** should use **Recommended Purchase Value (IDR)** instead of **Recommended Purchase Qty** on the X-axis, so management can compare purchase attention across SKUs with different units of measure.

---

## Business problem

Recommended Purchase Qty is meaningful **per item** (how many units to buy in that SKU’s unit). On a **population map** that mixes pieces, boxes, kilograms, liters, and other units, raw quantity is not a comparable axis. Distance on X can mislead: a large qty of a cheap unit item can look more important than a small qty of a high-cost SKU.

The map’s business question is: **Which items need purchase attention?** In the Investigation Workspace that is a **management triage** question (where to look first), not PO-line writing.

---

## Current behavior

| Aspect | Current |
| --- | --- |
| Preset | `replenishment-risk-map` |
| Question | Which items need purchase attention? |
| X-axis | `IN-KPI-028` Recommended Purchase Value (unit: IDR) |
| Y-axis | `IN-KPI-020` Days of Supply (unit: Days) |
| Peer filter | Principal |

Related portfolio KPI `IN-KPI-023` Recommended Purchase Budget is a **company-level** sum of Critical/High purchase recommendations (`RecommendedQty × HPP`). It is not an item-level Entity Analytics axis KPI today.

---

## Desired behavior

| Aspect | Desired |
| --- | --- |
| X-axis | Item-level **Recommended Purchase Value** in **IDR** (indicative cost of the suggested buy) |
| Y-axis | Unchanged: Days of Supply |
| Qty | Remains available as a supporting fact (cards, tooltip, detail, IN03/IN04), not as the population X-axis |
| Comparability | All items share one monetary unit for population position |

Map reading after change:

- **Far right** → larger indicative purchase cash / capital commitment  
- **Low Days of Supply** → more urgent replenishment  
- **Upper-right / low-DOS + high value** → priority purchase attention for management

---

## Users

| Role | Interest |
| --- | --- |
| Inventory / Purchasing management | Prioritize which SKUs deserve buy review and budget attention |
| Owner / ops lead | See cash exposure of indicated replenishment vs stock risk |
| Purchasing clerk (downstream) | Still needs qty + UoM when executing Desktop PO — not replaced by this map |

---

## Business rules

1. **Recommended Purchase Value (item)** = indicative `Recommended Purchase Qty × Unit HPP`, rounded as other inventory cost metrics (HPP basis, not selling price).
2. Value is **decision support only** — not an approved PO and not a budget commitment (same contract as qty / budget KPIs).
3. Do **not** redefine or remove `IN-KPI-021` Recommended Purchase Qty; qty remains the operational quantity KPI.
4. Do **not** overload `IN-KPI-023` Recommended Purchase Budget as the item axis; budget stays the portfolio Critical/High sum.
5. Zero or missing recommended qty → purchase value is zero / empty consistently with qty eligibility rules.
6. When on-hand qty is zero (stock-out), unit cost falls back to **`BTR_Brg.Hpp`**; if that is also zero/missing, value understates stock-out buys and is treated as a data-quality gap.
7. Axis labels and tooltips use business language (Rupiah), consistent with Inventory Health Map and Customer Risk Map IDR axes.

---

## Workflow impact

| Workflow | Change |
| --- | --- |
| Entity Analytics Item → Replenishment Risk Map discovery | X positions re-rank by cash magnitude instead of mixed units |
| Drill to IN03 / IN04 / Desktop purchasing | Unchanged; qty and actions remain source of execution |
| Peer Position on purchase-related KPI | If Peer Position is opened on the new value KPI, IDR binning already applies |
| Historical trend of qty | Unchanged (`IN-KPI-021` remains) |

---

## Analytical notes

### Benefits

- Cross-SKU comparability on the population surface  
- Aligns map attention with working-capital / cash planning language already used in IN04 (`Recommended Purchase Budget`, `EstimatedCostIdr`)  
- Matches other Entity Analytics maps that use IDR for “how big is this?”  
- Keeps urgency on Y (Days of Supply) so value alone does not define risk

### Drawbacks / risks

- Cheap, high-velocity SKUs with large unit counts may look smaller on X even when operationally urgent — mitigated by low Days of Supply on Y and by retaining qty in detail  
- Stock-out items with unresolved HPP (`BTR_Brg.Hpp` also zero) may plot near zero value — treat as data quality, not ignored  
- Value inherits HPP quality (current cost assumptions); same as existing inventory value metrics  
- Users who currently scan for “big qty dots” need a short product language shift to “big purchase cash”

### Alternatives considered

| Alternative | Verdict |
| --- | --- |
| Keep qty; filter by category only | Reduces but does not remove cross-UoM confusion inside a category |
| Days-of-cover gap only (unitless) | Good urgency signal but loses buy magnitude; duplicates Y information |
| Stock-out risk value if available | Complementary; not a full substitute for indicated buy cash |
| Dual axis / bubble = qty | Adds cognitive load; qty better as tooltip/detail than second primary encoding |

---

## Acceptance criteria

1. Replenishment Risk Map default X-axis is Recommended Purchase Value in IDR. ✓  
2. Y-axis remains Days of Supply. ✓  
3. Recommended Purchase Qty remains defined and available outside the default X-axis. ✓  
4. Items with different UoMs are comparable on X via Rupiah. ✓  
5. Tooltips / investigation facts still expose qty (and UoM where already shown) so execution is not blocked. ✓  
6. Stock-out / zero on-hand items with known HPP show non-zero purchase value when recommended qty &gt; 0. ✓  
7. Permanent KPI catalog and Entity Analytics docs name the item value metric distinctly from portfolio Recommended Purchase Budget. ✓

---

## Open questions (resolved in plan unless product overrides)

| Question | Proposed answer |
| --- | --- |
| New KPI vs reuse `IN-KPI-021`? | **New item KPI** (e.g. `IN-KPI-028`); do not change qty semantics |
| HPP when qty on hand = 0? | Fall back to **`BTR_Brg.Hpp`**; document if still zero |
| Include only Critical/High like budget? | **No** for map axis — use full indicative recommended qty × HPP (IN03 semantics), same eligibility as qty |
| Keep qty selectable as custom axis later? | Optional advanced; not required for this change |

---

## Success measure

A manager scanning the Replenishment Risk Map can identify high-cash, short-cover items without comparing incomparable units, and can still open an item and see recommended qty for purchasing follow-up.
